using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silica.AI;
using SilicaAdminMod;
using UnityEngine;

namespace Si_Logistics;

public sealed class LogisticsMod : MelonMod
{
	private sealed class Cargo
	{
		public int Amount;

		public float CooldownUntil;

		public string Transfer;

		public int TransferFrom;

		public int TransferTotal;

		public float NextProgress;

		public int LastPercent = -1;

		public float LastDiagnostic;
	}

	private static class KohCredits
	{
		public enum GrantResult
		{
			Granted,
			Unavailable,
			Uncertain
		}

		private static MethodInfo _grant;

		private static MethodInfo _balance;

		private static bool _missingLogged;

		public static bool Available => Resolve();

		private static bool Resolve()
		{
			if (_grant != null)
			{
				return true;
			}
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type type;
				try
				{
					type = assembly.GetType("Si_KingOfTheHill.KingOfTheHill");
				}
				catch
				{
					continue;
				}
				if (!(type == null))
				{
					_grant = type.GetMethod("GrantExternalCredits", BindingFlags.Static | BindingFlags.Public, null, new Type[3]
					{
						typeof(long),
						typeof(int),
						typeof(string)
					}, null);
					_balance = type.GetMethod("GetCreditsBySteamId", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(long) }, null);
					break;
				}
			}
			if (_grant == null && !_missingLogged)
			{
				DebugMsg("Logistics: Si_KingOfTheHill credit API not found; personal bonus inactive.");
				_missingLogged = true;
			}
			return _grant != null;
		}

		public static GrantResult TryGrant(long steamId, int amount, string reason)
		{
			if (!Resolve())
			{
				return GrantResult.Unavailable;
			}
			try
			{
				return (!(bool)_grant.Invoke(null, new object[3] { steamId, amount, reason })) ? GrantResult.Unavailable : GrantResult.Granted;
			}
			catch (Exception ex)
			{
				DebugWarning("Logistics: KoH credit result uncertain; attempted batch will NOT be retried to avoid duplicate credits: " + ex.Message);
				return GrantResult.Uncertain;
			}
		}

		public static int TryGetBalance(long steamId)
		{
			if (!Resolve() || _balance == null)
			{
				return -1;
			}
			try
			{
				return (int)_balance.Invoke(null, new object[1] { steamId });
			}
			catch
			{
				return -1;
			}
		}
	}

	private sealed class RewardAccount
	{
		internal Player Player;

		internal decimal Pending;

		internal float Due;
	}

	private sealed class DepositReceipt
	{
		internal DepositReceipt Parent;

		internal Unit Unit;

		internal ResourceHolder Holder;

		internal Player Driver;

		internal Team Team;

		internal decimal Rate;

		internal int ScoreRate;

		internal long Retrieved;

		internal long Accepted;

		internal bool Consumed;

		internal int Epoch;
	}

	private sealed class ScoreAccount
	{
		internal Player Player;

		internal int UnloadRemainder;

		internal int TransferRemainder;
	}

	private enum HarvestPhase
	{
		Starting,
		Harvesting,
		WaitingForRefinery,
		Queued,
		Depositing,
		Manual
	}

	private sealed class HarvestJob
	{
		internal AIHarvesterAgent Agent;

		internal Team Team;

		internal ResourceArea Area;

		internal bool PatchReserved;

		internal HarvestPhase Phase;

		internal float ReadyAt;

		internal float NextCheck;

		internal float PickupUntil;

		internal bool ResumeAfterPickup;

		internal RefineryLine Line;

		internal int LastSlot = -1;

		internal bool ExitRequested;

		internal Structure PreferredRefinery;

		internal string LastDiagnostic;
	}

	private sealed class RefineryLine
	{
		internal Structure Refinery;

		internal ResourceDepositPoint Point;

		internal Vector3 Approach;

		internal Vector3 Direction;

		internal readonly List<HarvestJob> Jobs = new List<HarvestJob>();
	}

	private sealed class AITransportJob
	{
		internal Unit Transport;

		internal Unit Harvester;

		internal Team Team;

		internal Structure Refinery;

		internal bool Delivering;

		internal bool Holding;

		internal float NextMove;

		internal float NextRefinerySearch;

		internal Vector3 Destination;

		internal Vector3 TargetPosition;

		internal string Status;

		internal int Attempts;

		internal int ServiceFrame = -1;

		internal Vector3 ServiceCenter;

		internal Vector3 ServiceFrom;

		internal Vector3 ServicePoint;

		internal float ServiceRadius;

		internal Structure ServiceRefinery;

		internal bool ServiceFound;

		internal ResourceArea AssignedArea;

		internal Unit AvoidHarvester;

		internal float AvoidUntil;

		internal float NextAllocation;

		internal float LastProgress;

		internal float BestDistance = float.MaxValue;

		internal bool Parking;

		internal Vector3 ParkingPoint;
	}

	private static readonly Dictionary<int, Cargo> CargoByTransport = new Dictionary<int, Cargo>();

	private static readonly HashSet<string> LoggedDriverVehicles = new HashSet<string>();

	private static LogisticsConfig Cfg = new LogisticsConfig();

	private static readonly string ConfigPath = Path.Combine(MelonEnvironment.UserDataDirectory, "Logistics_cfg", "Si_Logistics_Config.json");

	private float _elapsed;

	private static bool PerformanceTickActive;

	private static readonly List<Unit> TickHarvesters = new List<Unit>();

	private static readonly List<Unit> TickTransports = new List<Unit>();

	private static readonly List<Structure> TickRefineries = new List<Structure>();

	private static readonly Dictionary<int, float> TickRefineryDistances = new Dictionary<int, float>();

	private static readonly HashSet<int> TickActiveVehicles = new HashSet<int>();

	private static readonly List<int> TickStaleCargo = new List<int>();

	private static readonly List<KeyValuePair<int, AITransportJob>> TickJobSnapshot = new List<KeyValuePair<int, AITransportJob>>();

	private static readonly Dictionary<int, float> TransportErrorUntil = new Dictionary<int, float>();

	private static readonly List<int> TickStaleErrors = new List<int>();

	private static readonly float[] ServiceRings = new float[5] { 0.94f, 0.8f, 0.65f, 0.5f, 0.3f };

	private static long PerfTicks;

	private static long PerfSearches;

	private static long PerfNavProbes;

	private static long PerfServiceHits;

	private static double PerfTotalMs;

	private static double PerfMaxMs;

	private static float PerfNextReport;

	private static float NextRewardCheck = float.PositiveInfinity;

	private static readonly Dictionary<long, RewardAccount> Rewards = new Dictionary<long, RewardAccount>();

	private static bool MatchAcceptsRewards = true;

	private static int RewardEpoch;

	[ThreadStatic]
	private static DepositReceipt CurrentDeposit;

	[ThreadStatic]
	private static Stack<DepositReceipt> DepositReceiptPool;

	private static readonly Dictionary<long, ScoreAccount> ScoreRewards = new Dictionary<long, ScoreAccount>();

	private static readonly HashSet<BaseGameObject> CommanderBought = new HashSet<BaseGameObject>();

	private static readonly HashSet<BaseGameObject> CommanderProduced = new HashSet<BaseGameObject>();

	private static readonly HashSet<BaseGameObject> CommanderDestroyed = new HashSet<BaseGameObject>();

	private static readonly Dictionary<Team, int> CommanderHighestTier = new Dictionary<Team, int>();

	private static readonly Dictionary<CommanderScoreRule, Dictionary<Tuple<Player, Team>, decimal>> CommanderFractions = new Dictionary<CommanderScoreRule, Dictionary<Tuple<Player, Team>, decimal>>();

	[ThreadStatic]
	private static int CommanderPurchaseDepth;

	private static bool CommanderPurchaseHookReady;

	private static readonly Dictionary<int, HarvestJob> HarvestJobs = new Dictionary<int, HarvestJob>();

	private static readonly Dictionary<int, RefineryLine> RefineryLines = new Dictionary<int, RefineryLine>();

	private static readonly HashSet<int> NewHarvesters = new HashSet<int>();

	private static bool HarvestFixInstalled;

	private static bool HarvestOwnOrder;

	private static float NextHarvestScan;

	private static readonly Dictionary<int, AITransportJob> AITransportJobs = new Dictionary<int, AITransportJob>();

	private static bool AITransportInstalled;

	private static int AITransportOwnOrder;

	private static IEnumerable<Unit> AllocationHarvesters
	{
		get
		{
			if (!PerformanceTickActive)
			{
				return Unit.Units;
			}
			return TickHarvesters;
		}
	}

	private static IEnumerable<Structure> AllocationRefineries
	{
		get
		{
			if (!PerformanceTickActive)
			{
				return Structure.Structures;
			}
			return TickRefineries;
		}
	}

	private static float RewardWindowSeconds => Cfg.DeliveryCooldownSeconds;

	private static decimal RewardRate => (decimal)Cfg.PersonalBonus.Share;

	private static decimal HarvesterDepositRate => RewardRate;

	private static decimal HarvesterTransferRate => (decimal)Cfg.PersonalBonus.TransferShare;

	private static bool CommanderMatchActive
	{
		get
		{
			if (Game.GetIsServer() && MatchAcceptsRewards && (Object)(object)GameMode.CurrentGameMode != (Object)null)
			{
				return GameMode.CurrentGameMode.GameOngoing;
			}
			return false;
		}
	}

	private static bool CommanderScoringActive
	{
		get
		{
			if (CommanderMatchActive && Cfg.Enabled && Cfg.CommanderScore.Enabled)
			{
				return CommanderPurchaseHookReady;
			}
			return false;
		}
	}

	private static bool HarvestFixActive
	{
		get
		{
			if (HarvestFixInstalled && Cfg.Enabled && Cfg.HarvesterFix.Enabled && Game.GetIsServer())
			{
				return MatchAcceptsRewards;
			}
			return false;
		}
	}

	private static bool AITransportActive
	{
		get
		{
			if (AITransportInstalled && Cfg.Enabled && Cfg.AITransport.Enabled && Game.GetIsServer())
			{
				return MatchAcceptsRewards;
			}
			return false;
		}
	}

	private static void DebugMsg(string text)
	{
		if (Cfg.Debug)
		{
			MelonLogger.Msg(text);
		}
	}

	private static void DebugWarning(string text)
	{
		if (Cfg.Debug)
		{
			MelonLogger.Warning(text);
		}
	}

	private static void DebugError(string text)
	{
		if (Cfg.Debug)
		{
			MelonLogger.Error(text);
		}
	}

	public override void OnInitializeMelon()
	{
		LoadConfig();
		DebugMsg("Harvester Logistics config " + ConfigPath + ": enabled=" + Cfg.Enabled + ", vehicles=" + string.Join(", ", Cfg.TransportUnits) + ", capacity air/ground=" + Cfg.AirCargoCapacity + "/" + Cfg.GroundCargoCapacity + ", load/unload per s=" + Cfg.LoadUnitsPerSecond + "/" + Cfg.UnloadUnitsPerSecond + ", radius load/unload=" + Cfg.LoadRadius + "/" + Cfg.UnloadRadius + ", cooldown=" + Cfg.DeliveryCooldownSeconds + "s, debug=" + Cfg.Debug + ", sounds=" + Cfg.Sounds.Enabled + ", personal rewards=" + RewardSettings() + " as KoH credits");
	}

	private static void LoadConfig()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected Obj, but got Unknown
		string text = JsonConvert.SerializeObject((object)Cfg.HarvesterFix);
		try
		{
			if (File.Exists(ConfigPath))
			{
				JsonSerializerSettings val = new JsonSerializerSettings
				{
					ObjectCreationHandling = (ObjectCreationHandling)2
				};
				JObject val2 = JObject.Parse(File.ReadAllText(ConfigPath));
				JToken value = val2.GetValue("AItransport", StringComparison.OrdinalIgnoreCase);
				JObject val3 = (JObject)(object)((value is JObject) ? value : null);
				if (val3 != null)
				{
					if (val3.GetValue("airloadradius", StringComparison.OrdinalIgnoreCase) == null)
					{
						val3["airloadradius"] = val3.GetValue("DropshipLoadRadius", StringComparison.OrdinalIgnoreCase) ?? JToken.op_Implicit(150f);
					}
					if (val3.GetValue("airunloadradius", StringComparison.OrdinalIgnoreCase) == null)
					{
						val3["airunloadradius"] = val3.GetValue("DropshipUnloadRadius", StringComparison.OrdinalIgnoreCase) ?? JToken.op_Implicit(150f);
					}
					if (val3.GetValue("groundloadradius", StringComparison.OrdinalIgnoreCase) == null)
					{
						val3["groundloadradius"] = val2.GetValue("LoadRadius", StringComparison.OrdinalIgnoreCase) ?? JToken.op_Implicit(40f);
					}
					if (val3.GetValue("groundunloadradius", StringComparison.OrdinalIgnoreCase) == null)
					{
						val3["groundunloadradius"] = val2.GetValue("UnloadRadius", StringComparison.OrdinalIgnoreCase) ?? JToken.op_Implicit(65f);
					}
				}
				JToken value2 = val2.GetValue("PersonalBonus", StringComparison.OrdinalIgnoreCase);
				JObject val4 = (JObject)(object)((value2 is JObject) ? value2 : null);
				if (val4 != null && val4.GetValue("TransferShare", StringComparison.OrdinalIgnoreCase) == null)
				{
					val4["TransferShare"] = val4.GetValue("HarvesterTransferShare", StringComparison.OrdinalIgnoreCase) ?? JToken.op_Implicit(0.08f);
				}
				Cfg = JsonConvert.DeserializeObject<LogisticsConfig>(((object)val2).ToString(), val) ?? new LogisticsConfig();
			}
			else
			{
				Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
				File.WriteAllText(ConfigPath, JsonConvert.SerializeObject((object)Cfg, (Formatting)1));
				DebugMsg("Logistics: wrote default config " + ConfigPath);
			}
		}
		catch (Exception ex)
		{
			DebugError("Logistics: config " + ConfigPath + " unreadable, using defaults: " + ex.Message);
			Cfg = new LogisticsConfig();
		}
		if (Cfg.TransportUnits == null)
		{
			Cfg.TransportUnits = new List<string>();
		}
		if (Cfg.Sounds == null)
		{
			Cfg.Sounds = new LogisticsConfig.SoundConfig();
		}
		if (Cfg.PersonalBonus == null)
		{
			Cfg.PersonalBonus = new LogisticsConfig.BonusConfig();
		}
		if (Cfg.Chat == null)
		{
			Cfg.Chat = new LogisticsConfig.ChatConfig();
		}
		if (Cfg.Commands == null)
		{
			Cfg.Commands = new LogisticsConfig.CommandConfig();
		}
		Cfg.ScoreUnload = Math.Max(0, Math.Min(100, Cfg.ScoreUnload));
		Cfg.ScoreTransfer = Math.Max(0, Math.Min(100, Cfg.ScoreTransfer));
		if (!Cfg.LogisticsScore)
		{
			ScoreRewards.Clear();
		}
		ValidateCommanderScoreConfig();
		Cfg.PersonalBonus.Share = ValidRewardShare(Cfg.PersonalBonus.Share, 0.1f);
		Cfg.PersonalBonus.TransferShare = ValidRewardShare(Cfg.PersonalBonus.TransferShare, 0.08f);
		if (float.IsNaN(Cfg.DeliveryCooldownSeconds) || float.IsInfinity(Cfg.DeliveryCooldownSeconds) || Cfg.DeliveryCooldownSeconds < 0f)
		{
			Cfg.DeliveryCooldownSeconds = 20f;
		}
		List<string> list = new List<string>();
		foreach (string name in Cfg.TransportUnits)
		{
			if (!string.IsNullOrWhiteSpace(name) && !list.Exists((string n) => n.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
			{
				list.Add(name.Trim());
			}
		}
		Cfg.TransportUnits = list;
		ValidateHarvesterFixConfig();
		if (text != JsonConvert.SerializeObject((object)Cfg.HarvesterFix))
		{
			ResetHarvesterFix(resumeVanilla: true);
		}
		if (Cfg.AITransport == null)
		{
			Cfg.AITransport = new AITransportConfig();
		}
		ValidateAITransportConfig();
		if (!Cfg.Enabled)
		{
			ReleaseLogisticsControl();
		}
		else if (!Cfg.AITransport.Enabled)
		{
			ResetAITransports(stop: true);
		}
	}

	private static float ValidRewardShare(float? value, float fallback)
	{
		float num = value ?? fallback;
		if (!float.IsNaN(num) && !float.IsInfinity(num))
		{
			return Mathf.Clamp01(num);
		}
		return Mathf.Clamp01(fallback);
	}

	private static string RewardSettings()
	{
		return "transport " + Cfg.PersonalBonus.Share.ToString("P1", CultureInfo.InvariantCulture) + ", harvester refinery " + Cfg.PersonalBonus.Share.ToString("P1", CultureInfo.InvariantCulture) + ", harvester transfer " + Cfg.PersonalBonus.TransferShare.ToString("P1", CultureInfo.InvariantCulture) + ", payout " + Cfg.DeliveryCooldownSeconds.ToString("0.##", CultureInfo.InvariantCulture) + "s";
	}

	private static void SaveConfig()
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
			File.WriteAllText(ConfigPath, JsonConvert.SerializeObject((object)Cfg, (Formatting)1));
		}
		catch (Exception ex)
		{
			DebugWarning("Logistics: could not save config: " + ex.Message);
		}
	}

	public override void OnLateInitializeMelon()
	{
		InstallHarvesterRewards();
		BindRewardEvents();
		InstallHarvesterFix();
		InstallAITransport();
		InstallCommanderScore();
		HelperMethods.RegisterPlayerCommand("logistics", OnLogisticsCommand, Cfg.Commands.HideFromChat);
		HelperMethods.RegisterPlayerCommand("logi", OnLogisticsCommand, Cfg.Commands.HideFromChat);
		HelperMethods.RegisterPlayerCommand("logisticsscore", OnScoreCommand, Cfg.Commands.HideFromChat);
		HelperMethods.RegisterPlayerCommand("scoreunload", OnScoreCommand, Cfg.Commands.HideFromChat);
		HelperMethods.RegisterPlayerCommand("scoretransfer", OnScoreCommand, Cfg.Commands.HideFromChat);
		HelperMethods.RegisterPlayerCommand("logisticsscorecomander", OnCommanderScoreCommand, Cfg.Commands.HideFromChat);
		HelperMethods.RegisterPlayerCommand("logisticsscorecommander", OnCommanderScoreCommand, Cfg.Commands.HideFromChat);
	}

	private static void ReleaseLogisticsControl()
	{
		ResetAITransports(stop: true);
		ResetHarvesterFix(resumeVanilla: true);
		if (Unit.Units == null)
		{
			return;
		}
		foreach (Unit unit in Unit.Units)
		{
			if (IsTransport(unit) && CargoByTransport.TryGetValue(((Object)unit).GetInstanceID(), out var value))
			{
				SetTransfer(unit, value, null);
			}
		}
	}

	private static void OnLogisticsCommand(Player caller, string args)
	{
		try
		{
			string[] array = (args ?? string.Empty).Trim().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string text = ((array.Length > 1) ? array[1].ToLowerInvariant() : string.Empty);
			string text2 = ((array.Length > 2) ? array[2].ToLowerInvariant() : string.Empty);
			string text3 = ((array.Length > 3) ? array[3] : string.Empty);
			if (text == string.Empty || text == "status")
			{
				ShowStatus(caller);
				return;
			}
			if (text == "help")
			{
				ShowHelp(caller);
				return;
			}
			if ((Object)(object)caller != (Object)null && (!caller.IsAdmin() || !AdminMethods.PowerInPowers(Power.Generic, caller.GetAdminPowers())))
			{
				Reply(caller, "Admin only. Use /logistics for status.");
				return;
			}
			int num;
			switch (text)
			{
			default:
				num = 3;
				break;
			case "on":
			case "off":
			case "reload":
				num = 2;
				break;
			case "capacity":
			case "rate":
			case "radius":
				num = 4;
				break;
			}
			int num2 = num;
			if (array.Length != num2)
			{
				ShowHelp(caller);
				return;
			}
			switch (text)
			{
			case "on":
			case "off":
				Cfg.Enabled = text == "on";
				if (!Cfg.Enabled)
				{
					ReleaseLogisticsControl();
				}
				SaveConfig();
				Reply(caller, "Logistics " + (Cfg.Enabled ? "enabled." : "disabled."));
				break;
			case "share":
			case "transfershare":
			{
				if (!TryParseFloat(text2, out var value) || value < 0f || value > 1f)
				{
					Reply(caller, "Usage: " + text + " <0..1>");
					break;
				}
				if (text == "share")
				{
					Cfg.PersonalBonus.Share = value;
				}
				else
				{
					Cfg.PersonalBonus.TransferShare = value;
				}
				SaveConfig();
				Reply(caller, ((text == "share") ? "Transport and harvester refinery reward" : "Harvester-to-transport pickup reward") + " = " + value.ToString("P1", CultureInfo.InvariantCulture) + ".");
				break;
			}
			case "harvesterfix":
			case "aitransport":
				if (text2 != "on" && text2 != "off")
				{
					Reply(caller, "Usage: /logistics " + text + " on|off");
					break;
				}
				if (text == "harvesterfix")
				{
					Cfg.HarvesterFix.Enabled = text2 == "on";
					if (!Cfg.HarvesterFix.Enabled)
					{
						ResetHarvesterFix(resumeVanilla: true);
					}
				}
				else
				{
					Cfg.AITransport.Enabled = text2 == "on";
					if (!Cfg.AITransport.Enabled)
					{
						ResetAITransports(stop: true);
					}
				}
				SaveConfig();
				Reply(caller, text + " " + text2.ToUpperInvariant() + (Cfg.Enabled ? "." : " (master Logistics is OFF)."));
				break;
			case "capacity":
			{
				if (!int.TryParse(text3, out var result2) || result2 <= 0 || (text2 != "air" && text2 != "ground"))
				{
					Reply(caller, "Usage: capacity air|ground <n>");
					break;
				}
				if (text2 == "air")
				{
					Cfg.AirCargoCapacity = result2;
				}
				else
				{
					Cfg.GroundCargoCapacity = result2;
				}
				SaveConfig();
				Reply(caller, text2 + " capacity = " + result2 + ".");
				break;
			}
			case "rate":
			{
				if (!int.TryParse(text3, out var result) || result <= 0 || (text2 != "load" && text2 != "unload"))
				{
					Reply(caller, "Usage: rate load|unload <units per second>");
					break;
				}
				if (text2 == "load")
				{
					Cfg.LoadUnitsPerSecond = result;
				}
				else
				{
					Cfg.UnloadUnitsPerSecond = result;
				}
				SaveConfig();
				Reply(caller, text2 + " rate = " + result + "/s.");
				break;
			}
			case "radius":
			{
				if (!TryParseFloat(text3, out var value2) || value2 <= 0f || (text2 != "load" && text2 != "unload"))
				{
					Reply(caller, "Usage: radius load|unload <metres>");
					break;
				}
				if (text2 == "load")
				{
					Cfg.LoadRadius = value2;
				}
				else
				{
					Cfg.UnloadRadius = value2;
				}
				SaveConfig();
				Reply(caller, text2 + " radius = " + value2.ToString("F0", CultureInfo.InvariantCulture) + " m.");
				break;
			}
			case "cooldown":
			{
				if (!TryParseFloat(text2, out var value3) || value3 < 0f)
				{
					Reply(caller, "Usage: cooldown <seconds>");
					break;
				}
				Cfg.DeliveryCooldownSeconds = value3;
				SaveConfig();
				Reply(caller, "Delivery cooldown and new reward payout delay = " + value3.ToString("F0", CultureInfo.InvariantCulture) + " s.");
				break;
			}
			case "debug":
				if (text2 != "on" && text2 != "off")
				{
					Reply(caller, "Usage: debug on|off");
					break;
				}
				Cfg.Debug = text2 == "on";
				SaveConfig();
				Reply(caller, "Debug " + text2 + ".");
				break;
			case "reload":
				LoadConfig();
				Reply(caller, "Config reloaded from " + ConfigPath + ".");
				break;
			default:
				ShowHelp(caller);
				break;
			}
		}
		catch (Exception ex)
		{
			DebugWarning("Logistics command: " + ex);
		}
	}

	private static void ShowStatus(Player caller)
	{
		Reply(caller, "Logistics " + (Cfg.Enabled ? "ON" : "OFF") + " | load " + Cfg.LoadUnitsPerSecond + "/s within " + Cfg.LoadRadius.ToString("F0", CultureInfo.InvariantCulture) + " m | unload " + Cfg.UnloadUnitsPerSecond + "/s within " + Cfg.UnloadRadius.ToString("F0", CultureInfo.InvariantCulture) + " m | air rates " + Cfg.AirLoadUnitsPerSecond + "/" + Cfg.AirUnloadUnitsPerSecond + "/s | air " + Cfg.AirCargoCapacity + ", ground " + Cfg.GroundCargoCapacity + " | cooldown " + Cfg.DeliveryCooldownSeconds.ToString("F0", CultureInfo.InvariantCulture) + " s | rewards " + RewardSettings() + (KohCredits.Available ? " (KoH linked)" : " (KoH not found)"));
		ShowHarvesterStatus(caller);
		ShowAITransportStatus(caller);
		Reply(caller, "Logistics score " + (Cfg.LogisticsScore ? "ON" : "OFF") + " | unload " + Cfg.ScoreUnload + " | transfer " + Cfg.ScoreTransfer + " points per 100 Balterium" + (Cfg.Enabled ? "." : " (master Logistics is OFF)."));
		Reply(caller, "Commander score " + (Cfg.CommanderScore.Enabled ? "ON" : "OFF") + " | active " + (CommanderScoringActive ? "YES" : "NO") + " | KoH purchase exclusion " + (CommanderPurchaseHookReady ? "ready" : "unavailable; scoring blocked") + ".");
		Unit val = (((Object)(object)caller != (Object)null) ? caller.ControlledUnit : null);
		if (!((Object)(object)val == (Object)null) && IsTransport(val))
		{
			int num = (CargoByTransport.TryGetValue(((Object)val).GetInstanceID(), out var value) ? value.Amount : 0);
			Reply(caller, "Your " + ((BaseGameObject)val).ObjectInfo.DisplayName + ": " + num + "/" + CapacityFor(val) + " Balterium aboard.");
		}
	}

	private static void ShowHelp(Player caller)
	{
		Reply(caller, "/logistics or /logi: [status|help]. Status is available to everyone.");
		Reply(caller, "Admin: on|off; share <0..1>; transfershare <0..1>; capacity air|ground <positive integer>.");
		Reply(caller, "Admin: rate load|unload <positive integer> (ground); radius load|unload <metres> (player transfers).");
		Reply(caller, "Admin: cooldown <seconds>; debug on|off; reload; harvesterfix on|off; aitransport on|off.");
		Reply(caller, "Admin: /logisticsscore on|off; /scoreunload <0..100>; /scoretransfer <0..100> (points per 100 Balterium).");
		Reply(caller, "Admin: /logisticsscorecomander on|off; edit CommanderScore.Rules in config, then /logistics reload.");
	}

	private static bool TryParseFloat(string text, out float value)
	{
		if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) && !float.IsNaN(value))
		{
			return !float.IsInfinity(value);
		}
		return false;
	}

	private static void Reply(Player caller, string text)
	{
		if ((Object)(object)caller != (Object)null)
		{
			HelperMethods.SendChatMessageToPlayer(caller, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " " + text);
		}
		else
		{
			DebugMsg(text);
		}
	}

	public override void OnUpdate()
	{
		if (!Game.GetIsServer())
		{
			return;
		}
		UpdateHarvesterFix();
		try
		{
			PayDueRewards();
		}
		catch (Exception ex)
		{
			DebugWarning("Logistics payout: " + ex.Message);
		}
		if (!AITransportActive && AITransportJobs.Count > 0)
		{
			ResetAITransports(stop: true);
		}
		if (!Cfg.Enabled || !MatchAcceptsRewards)
		{
			return;
		}
		_elapsed += Time.deltaTime;
		if (_elapsed < 1f)
		{
			return;
		}
		_elapsed = 0f;
		try
		{
			Tick();
		}
		catch (Exception ex2)
		{
			DebugError("Logistics tick: " + ex2);
		}
	}

	private static bool IsTransport(Unit unit)
	{
		if ((Object)(object)unit == (Object)null || (Object)(object)unit.DriverCompartment == (Object)null || (Object)(object)((BaseGameObject)unit).DamageManager == (Object)null || ((BaseGameObject)unit).DamageManager.IsDestroyed || (Object)(object)((BaseGameObject)unit).Team == (Object)null || (Object)(object)((BaseGameObject)unit).ObjectInfo == (Object)null)
		{
			return false;
		}
		string name = ((BaseGameObject)unit).ObjectInfo.DisplayName.ToString();
		return Cfg.TransportUnits.Exists((string n) => n.Equals(name, StringComparison.OrdinalIgnoreCase));
	}

	private static void TickCore()
	{
		if (Unit.Units == null || Structure.Structures == null)
		{
			return;
		}
		HashSet<int> tickActiveVehicles = TickActiveVehicles;
		tickActiveVehicles.Clear();
		foreach (Unit tickTransport in TickTransports)
		{
			if (Cfg.Debug && (Object)(object)tickTransport != (Object)null && (Object)(object)tickTransport.ControlledBy != (Object)null && (Object)(object)tickTransport.DriverCompartment != (Object)null && (Object)(object)((BaseGameObject)tickTransport).ObjectInfo != (Object)null)
			{
				string text = ((BaseGameObject)tickTransport).ObjectInfo.DisplayName.ToString();
				if (LoggedDriverVehicles.Add(text))
				{
					DebugMsg("Logistics diagnostic vehicle name: " + text);
				}
			}
			if (!IsTransport(tickTransport))
			{
				continue;
			}
			int instanceID = ((Object)tickTransport).GetInstanceID();
			tickActiveVehicles.Add(instanceID);
			if (TransportErrorUntil.TryGetValue(instanceID, out var value) && Time.unscaledTime < value)
			{
				continue;
			}
			try
			{
				if (AITransportJobs.TryGetValue(instanceID, out var value2) && TickAITransport(value2))
				{
					continue;
				}
				if (!Cfg.Enabled || (Object)(object)tickTransport.ControlledBy == (Object)null)
				{
					if (CargoByTransport.TryGetValue(instanceID, out var value3))
					{
						SetTransfer(tickTransport, value3, null);
					}
					continue;
				}
				if (!CargoByTransport.TryGetValue(instanceID, out var value4))
				{
					value4 = new Cargo();
					CargoByTransport.Add(instanceID, value4);
				}
				if (Cfg.Debug)
				{
					DiagnoseNearby(tickTransport, value4);
				}
				if (Time.unscaledTime < value4.CooldownUntil)
				{
					SetTransfer(tickTransport, value4, null);
				}
				else if ((value4.Amount <= 0 || !TryUnload(tickTransport, value4)) && (value4.Amount >= CapacityFor(tickTransport) || !TryLoad(tickTransport, value4)))
				{
					SetTransfer(tickTransport, value4, null);
				}
			}
			catch (Exception ex)
			{
				TransportErrorUntil[instanceID] = Time.unscaledTime + 10f;
				DebugError("Logistics transport=" + instanceID + " update failed; retry in 10s: " + ex);
			}
		}
		List<int> tickStaleCargo = TickStaleCargo;
		tickStaleCargo.Clear();
		foreach (KeyValuePair<int, Cargo> item in CargoByTransport)
		{
			if (!tickActiveVehicles.Contains(item.Key))
			{
				tickStaleCargo.Add(item.Key);
			}
		}
		foreach (int item2 in tickStaleCargo)
		{
			CargoByTransport.Remove(item2);
		}
		TickStaleErrors.Clear();
		foreach (KeyValuePair<int, float> item3 in TransportErrorUntil)
		{
			if (!tickActiveVehicles.Contains(item3.Key))
			{
				TickStaleErrors.Add(item3.Key);
			}
		}
		foreach (int tickStaleError in TickStaleErrors)
		{
			TransportErrorUntil.Remove(tickStaleError);
		}
		TickJobSnapshot.Clear();
		foreach (KeyValuePair<int, AITransportJob> aITransportJob in AITransportJobs)
		{
			TickJobSnapshot.Add(aITransportJob);
		}
		foreach (KeyValuePair<int, AITransportJob> item4 in TickJobSnapshot)
		{
			if (!tickActiveVehicles.Contains(item4.Key))
			{
				CancelAITransport(item4.Value, "transport destroyed or no longer eligible", stop: false);
			}
		}
	}

	private static void DiagnoseNearby(Unit transport, Cargo cargo)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Invalid comparison between Unknown and I4
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		if (Time.time - cargo.LastDiagnostic < 5f)
		{
			return;
		}
		cargo.LastDiagnostic = Time.time;
		Unit val = null;
		float num = float.MaxValue;
		int num2 = 0;
		foreach (Unit unit in Unit.Units)
		{
			if ((Object)(object)unit == (Object)null || (Object)(object)((BaseGameObject)unit).Team != (Object)(object)((BaseGameObject)transport).Team || (Object)(object)((BaseGameObject)unit).ObjectInfo == (Object)null)
			{
				continue;
			}
			if ((int)((BaseGameObject)unit).ObjectInfo.UnitType == 7)
			{
				num2++;
			}
			if (((BaseGameObject)unit).IsResourceHolder)
			{
				float num3 = Vector3.Distance(((Component)unit).transform.position, ((Component)transport).transform.position);
				if (num3 < num)
				{
					val = unit;
					num = num3;
				}
			}
		}
		Structure val2 = null;
		float num4 = float.MaxValue;
		foreach (Structure structure in Structure.Structures)
		{
			if (!((Object)(object)structure == (Object)null) && !((Object)(object)((BaseGameObject)structure).Team != (Object)(object)((BaseGameObject)transport).Team) && !((Object)(object)((BaseGameObject)structure).ObjectInfo == (Object)null) && ((BaseGameObject)structure).ObjectInfo.DisplayName.ToString().IndexOf("Refinery", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				float num5 = Vector3.Distance(((Component)structure).transform.position, ((Component)transport).transform.position);
				if (num5 < num4)
				{
					val2 = structure;
					num4 = num5;
				}
			}
		}
		string text = (((Object)(object)val == (Object)null) ? "none" : (((BaseGameObject)val).ObjectInfo.DisplayName + " distance=" + num.ToString("F1") + " stored=" + ((BaseGameObject)val).GetStoredResources(((BaseGameObject)transport).Team.UsableResource) + " holderCount=" + ((((BaseGameObject)val).ResourceHolders != null) ? ((BaseGameObject)val).ResourceHolders.Count : 0) + " type=" + ((object)((BaseGameObject)val).ObjectInfo.UnitType/*cast due to constrained. prefix*/).ToString()));
		string text2 = (((Object)(object)val2 == (Object)null) ? "none" : (((BaseGameObject)val2).ObjectInfo.DisplayName + " distance=" + num4.ToString("F1") + " depositPoints=" + ((val2.ResourceDepositPoints != null) ? val2.ResourceDepositPoints.Count : 0)));
		DebugMsg("Logistics probe " + ((BaseGameObject)transport).ObjectInfo.DisplayName + " cargo=" + cargo.Amount + " friendlyHarvesters=" + num2 + " nearestHolder=[" + text + "] nearestRefinery=[" + text2 + "]");
	}

	private static int CapacityFor(Unit transport)
	{
		return Math.Max(1, transport.IsFlyingType ? Cfg.AirCargoCapacity : Cfg.GroundCargoCapacity);
	}

	private static bool TryLoad(Unit transport, Cargo cargo, Unit assignedHarvester = null, float? loadRadius = null)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Invalid comparison between Unknown and I4
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		int num = (Object.op_Implicit((Object)(object)assignedHarvester) ? 1 : TickHarvesters.Count);
		for (int i = 0; i < num; i++)
		{
			Unit val = (Object.op_Implicit((Object)(object)assignedHarvester) ? assignedHarvester : TickHarvesters[i]);
			if (((Object)(object)assignedHarvester != (Object)null && (Object)(object)val != (Object)(object)assignedHarvester) || (Object)(object)val == (Object)null || (Object)(object)((BaseGameObject)val).ObjectInfo == (Object)null || (int)((BaseGameObject)val).ObjectInfo.UnitType != 7 || (Object)(object)((BaseGameObject)val).Team != (Object)(object)((BaseGameObject)transport).Team || (Object)(object)((BaseGameObject)val).DamageManager == (Object)null || ((BaseGameObject)val).DamageManager.IsDestroyed || !((BaseGameObject)val).IsResourceHolder || Vector3.Distance(((Component)val).transform.position, ((Component)transport).transform.position) > (loadRadius ?? Cfg.LoadRadius))
			{
				continue;
			}
			flag = true;
			Resource usableResource = ((BaseGameObject)transport).Team.UsableResource;
			int storedResources = ((BaseGameObject)val).GetStoredResources(usableResource);
			if (storedResources == 0)
			{
				continue;
			}
			int num2 = Math.Min(Math.Min(storedResources, Math.Max(1, transport.IsFlyingType ? Cfg.AirLoadUnitsPerSecond : Cfg.LoadUnitsPerSecond)), Math.Max(0, CapacityFor(transport) - cargo.Amount));
			if (num2 <= 0)
			{
				continue;
			}
			Player val2 = DriverOf(val);
			Player val3 = DriverOf(transport);
			decimal harvesterTransferRate = HarvesterTransferRate;
			int rewardEpoch = RewardEpoch;
			((BaseGameObject)val).RetrieveResource(usableResource, num2);
			int storedResources2 = ((BaseGameObject)val).GetStoredResources(usableResource);
			int num3 = Math.Max(0, storedResources - storedResources2);
			if (num3 > num2)
			{
				((BaseGameObject)val).StoreResource(usableResource, num3 - num2);
				num3 = num2;
			}
			cargo.Amount += num3;
			if (num3 > 0)
			{
				HarvesterTransportPickup(val);
				if ((Object)(object)val3 != (Object)null)
				{
					QueueReward(val2, num3, harvesterTransferRate, rewardEpoch);
					if (Connected(val3, SteamId(val3)))
					{
						AwardScore(val2, num3, Cfg.ScoreTransfer, transfer: true, rewardEpoch);
					}
				}
				SetTransfer(transport, cargo, "Loading", 0, CapacityFor(transport));
				Progress(transport, cargo);
				if (Cfg.Debug)
				{
					DebugMsg("Logistics LOAD " + num3 + " from " + ((BaseGameObject)val).ObjectInfo.DisplayName + "; cargo=" + cargo.Amount);
				}
				bool flag2 = cargo.Amount >= CapacityFor(transport);
				PlaySound(transport, flag2 ? Cfg.Sounds.OnFull : Cfg.Sounds.OnLoad);
				if (flag2)
				{
					SetTransfer(transport, cargo, null);
				}
			}
			if (num3 > 0)
			{
				return true;
			}
		}
		if (flag)
		{
			return cargo.Transfer == "Loading";
		}
		return false;
	}

	private static bool TryUnload(Unit transport, Cargo cargo, Structure assignedRefinery = null, float? unloadRadius = null)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		int num = (Object.op_Implicit((Object)(object)assignedRefinery) ? 1 : TickRefineries.Count);
		for (int i = 0; i < num; i++)
		{
			Structure val = (Object.op_Implicit((Object)(object)assignedRefinery) ? assignedRefinery : TickRefineries[i]);
			if (((Object)(object)assignedRefinery != (Object)null && (Object)(object)val != (Object)(object)assignedRefinery) || (Object)(object)val == (Object)null || (Object)(object)((BaseGameObject)val).Team != (Object)(object)((BaseGameObject)transport).Team || val.ResourceDepositPoints == null || val.ResourceDepositPoints.Count == 0 || (Object)(object)((BaseGameObject)val).DamageManager == (Object)null || ((BaseGameObject)val).DamageManager.IsDestroyed || Vector3.Distance(((Component)val).transform.position, ((Component)transport).transform.position) > (unloadRadius ?? Cfg.UnloadRadius))
			{
				continue;
			}
			int num2 = Math.Min(Math.Min(cargo.Amount, Math.Max(1, transport.IsFlyingType ? Cfg.AirUnloadUnitsPerSecond : Cfg.UnloadUnitsPerSecond)), Math.Max(0, ((BaseGameObject)transport).Team.RemainingResourceCapacity));
			if (num2 <= 0)
			{
				return false;
			}
			Player val2 = DriverOf(transport);
			decimal rewardRate = RewardRate;
			int rewardEpoch = RewardEpoch;
			SetTransfer(transport, cargo, "Unloading", cargo.Amount, cargo.Amount);
			int val3 = ((BaseGameObject)transport).Team.StoreResource(num2);
			int num3 = Math.Max(0, num2 - Math.Max(0, val3));
			cargo.Amount -= num3;
			if (num3 > 0)
			{
				QueueReward(val2, num3, rewardRate, rewardEpoch);
				AwardScore(val2, num3, Cfg.ScoreUnload, transfer: false, rewardEpoch);
				CommanderDeposit(((BaseGameObject)transport).Team, num3, rewardEpoch);
				if (Cfg.Debug)
				{
					DebugMsg("Logistics UNLOAD " + num3 + "; cargo=" + cargo.Amount);
				}
				Progress(transport, cargo);
				if (cargo.Amount == 0)
				{
					cargo.CooldownUntil = Time.unscaledTime + Math.Max(0f, Cfg.DeliveryCooldownSeconds);
					SetTransfer(transport, cargo, null);
				}
				else if (((BaseGameObject)transport).Team.RemainingResourceCapacity <= 0)
				{
					SetTransfer(transport, cargo, null);
				}
			}
			return num3 > 0;
		}
		return false;
	}

	private static void PlaySound(Unit transport, string filename)
	{
		if (Cfg.Sounds.Enabled && !string.IsNullOrEmpty(filename) && !((Object)(object)transport.ControlledBy == (Object)null))
		{
			string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData", "sounds", filename);
			if (File.Exists(text))
			{
				AudioHelper.PlaySoundFile(text, transport.ControlledBy);
			}
		}
	}

	private static void SetTransfer(Unit transport, Cargo cargo, string transfer, int from = 0, int total = 0)
	{
		if (cargo.Transfer == transfer)
		{
			return;
		}
		string transfer2 = cargo.Transfer;
		Player controlledBy = transport.ControlledBy;
		if (transfer2 != null)
		{
			DebugMsg("Logistics: " + (((Object)(object)controlledBy != (Object)null) ? controlledBy.PlayerName : "no driver") + " " + ((BaseGameObject)transport).ObjectInfo.DisplayName + " " + transfer2.ToLowerInvariant() + " ended at " + TransferPercent(cargo) + "%, cargo " + cargo.Amount + "/" + CapacityFor(transport) + ((transfer2 == "Unloading") ? (", delivered " + (cargo.TransferFrom - cargo.Amount)) : ""));
		}
		if (transfer2 != null && (Object)(object)controlledBy != (Object)null)
		{
			int percent = TransferPercent(cargo);
			HelperMethods.SendChatMessageToPlayer(controlledBy, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " " + transfer2 + (TransferComplete(cargo) ? " complete " : " stopped ") + Bar(percent) + ((Time.unscaledTime < cargo.CooldownUntil) ? (" Transfer cooldown: " + Cfg.DeliveryCooldownSeconds + " seconds.") : ""));
		}
		cargo.Transfer = transfer;
		if (transfer != null)
		{
			cargo.TransferFrom = from;
			cargo.TransferTotal = Math.Max(1, total);
			cargo.NextProgress = Time.unscaledTime + Cfg.Chat.ProgressIntervalSeconds;
			cargo.LastPercent = -1;
			if ((Object)(object)controlledBy != (Object)null)
			{
				HelperMethods.SendChatMessageToPlayer(controlledBy, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", (transfer == "Loading") ? (" Loading started: cargo " + cargo.Amount + "/" + total + " Balterium.") : (" Unloading started: " + total + " Balterium to deliver. " + Bar(100) + " remaining."));
			}
		}
	}

	private static void Progress(Unit transport, Cargo cargo)
	{
		if (cargo.Transfer != null && !((Object)(object)transport.ControlledBy == (Object)null) && !(Cfg.Chat.ProgressIntervalSeconds <= 0f) && !(Time.unscaledTime < cargo.NextProgress))
		{
			int num = TransferPercent(cargo);
			if (!TransferComplete(cargo) && num != cargo.LastPercent)
			{
				cargo.LastPercent = num;
				cargo.NextProgress = Time.unscaledTime + Cfg.Chat.ProgressIntervalSeconds;
				HelperMethods.SendChatMessageToPlayer(transport.ControlledBy, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " " + cargo.Transfer + " " + Bar(num));
			}
		}
	}

	private static int TransferPercent(Cargo cargo)
	{
		if (cargo.Transfer == "Unloading")
		{
			return Math.Min(100, (int)((100L * (long)Math.Max(0, cargo.Amount) + Math.Max(1, cargo.TransferTotal) - 1) / Math.Max(1, cargo.TransferTotal)));
		}
		int num = Math.Abs(cargo.Amount - cargo.TransferFrom);
		return Math.Min(100, (int)(100L * (long)num / Math.Max(1, cargo.TransferTotal)));
	}

	private static bool TransferComplete(Cargo cargo)
	{
		if (!(cargo.Transfer == "Unloading"))
		{
			return cargo.Amount >= cargo.TransferTotal;
		}
		return cargo.Amount == 0;
	}

	private static string Bar(int percent)
	{
		int num = Math.Max(1, Math.Min(40, Cfg.Chat.ProgressBarLength));
		int num2 = percent * num / 100;
		StringBuilder stringBuilder = new StringBuilder("[");
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append((i < num2) ? Cfg.Chat.BarFilled : Cfg.Chat.BarEmpty);
		}
		return stringBuilder.Append("] ").Append(percent).Append('%')
			.ToString();
	}

	private static void Tick()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Invalid comparison between Unknown and I4
		if (Unit.Units == null || Structure.Structures == null)
		{
			return;
		}
		bool debug = Cfg.Debug;
		long num = (debug ? Stopwatch.GetTimestamp() : 0);
		TickHarvesters.Clear();
		TickTransports.Clear();
		TickRefineries.Clear();
		TickRefineryDistances.Clear();
		try
		{
			foreach (Unit unit in Unit.Units)
			{
				if (Object.op_Implicit((Object)(object)unit))
				{
					if (IsTransport(unit))
					{
						TickTransports.Add(unit);
					}
					if (Object.op_Implicit((Object)(object)((BaseGameObject)unit).ObjectInfo) && (int)((BaseGameObject)unit).ObjectInfo.UnitType == 7)
					{
						TickHarvesters.Add(unit);
					}
				}
			}
			foreach (Structure structure in Structure.Structures)
			{
				if (Object.op_Implicit((Object)(object)structure) && !((BaseGameObject)structure).IsDestroyed && structure.ResourceDepositPoints != null && structure.ResourceDepositPoints.Count > 0)
				{
					TickRefineries.Add(structure);
				}
			}
			PerformanceTickActive = true;
			TickCore();
		}
		finally
		{
			PerformanceTickActive = false;
			TickHarvesters.Clear();
			TickTransports.Clear();
			TickRefineries.Clear();
			TickRefineryDistances.Clear();
			TickJobSnapshot.Clear();
			TickActiveVehicles.Clear();
			TickStaleCargo.Clear();
			TickStaleErrors.Clear();
			if (debug)
			{
				double num2 = (double)(Stopwatch.GetTimestamp() - num) * 1000.0 / (double)Stopwatch.Frequency;
				PerfTicks++;
				PerfTotalMs += num2;
				PerfMaxMs = Math.Max(PerfMaxMs, num2);
				if (Time.unscaledTime >= PerfNextReport)
				{
					DebugMsg("[LogisticsPerf] transfer ticks=" + PerfTicks + " avgMs=" + (PerfTotalMs / (double)PerfTicks).ToString("F3") + " maxMs=" + PerfMaxMs.ToString("F3") + " serviceSearches=" + PerfSearches + " navProbes=" + PerfNavProbes + " sameFrameServiceHits=" + PerfServiceHits);
					PerfNextReport = Time.unscaledTime + 30f;
					PerfTicks = (PerfSearches = (PerfNavProbes = (PerfServiceHits = 0L)));
					PerfTotalMs = (PerfMaxMs = 0.0);
				}
			}
			else
			{
				PerfTicks = (PerfSearches = (PerfNavProbes = (PerfServiceHits = 0L)));
				PerfTotalMs = (PerfMaxMs = 0.0);
				PerfNextReport = Time.unscaledTime + 30f;
			}
		}
	}

	private static float AIHarvesterRefineryDistance(Unit h)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!PerformanceTickActive)
		{
			return NearestAIRefineryDistance(((Component)h).transform.position, ((BaseGameObject)h).Team);
		}
		int instanceID = ((Object)h).GetInstanceID();
		if (!TickRefineryDistances.TryGetValue(instanceID, out var value))
		{
			value = NearestAIRefineryDistance(((Component)h).transform.position, ((BaseGameObject)h).Team);
			TickRefineryDistances[instanceID] = value;
		}
		return value;
	}

	private static Player DriverOf(Unit unit)
	{
		if (!((Object)(object)unit == (Object)null))
		{
			if (!((Object)(object)unit.ControlledBy != (Object)null))
			{
				if (!((Object)(object)unit.Driver != (Object)null))
				{
					return null;
				}
				return unit.Driver.ControlledBy;
			}
			return unit.ControlledBy;
		}
		return null;
	}

	private static long SteamId(Player player)
	{
		if (!((Object)(object)player == (Object)null))
		{
			return (long)player.PlayerID.m_ID;
		}
		return 0L;
	}

	private static bool Connected(Player player, long id)
	{
		if ((Object)(object)player != (Object)null && id != 0L && Player.Players != null && Player.Players.Contains(player))
		{
			return SteamId(player) == id;
		}
		return false;
	}

	private static void QueueReward(Player driver, int amount, decimal rate, int epoch)
	{
		if (epoch != RewardEpoch || !Game.GetIsServer() || !MatchAcceptsRewards || !Cfg.Enabled || amount <= 0 || rate <= 0m)
		{
			return;
		}
		long num = SteamId(driver);
		if (Connected(driver, num))
		{
			if (!Rewards.TryGetValue(num, out var value) || value.Player != driver)
			{
				Dictionary<long, RewardAccount> rewards = Rewards;
				RewardAccount rewardAccount = new RewardAccount
				{
					Player = driver
				};
				value = rewardAccount;
				rewards[num] = rewardAccount;
			}
			if (value.Due == 0f)
			{
				value.Due = Math.Max(float.Epsilon, Time.unscaledTime + RewardWindowSeconds);
			}
			value.Pending += (decimal)amount * rate;
			NextRewardCheck = Math.Min(NextRewardCheck, value.Due);
		}
	}

	private static void PayDueRewards()
	{
		if (!Game.GetIsServer() || !MatchAcceptsRewards || !Cfg.Enabled || Time.unscaledTime < NextRewardCheck)
		{
			return;
		}
		NextRewardCheck = Time.unscaledTime + 1f;
		foreach (KeyValuePair<long, RewardAccount> item in new List<KeyValuePair<long, RewardAccount>>(Rewards))
		{
			long key = item.Key;
			RewardAccount value = item.Value;
			if (!Connected(value.Player, key))
			{
				Rewards.Remove(key);
			}
			else
			{
				if (value.Due == 0f || Time.unscaledTime < value.Due)
				{
					continue;
				}
				value.Due = 0f;
				decimal num = decimal.Floor(value.Pending);
				if (num < 1m)
				{
					continue;
				}
				int num2 = KohCredits.TryGetBalance(key);
				if (num2 < 0)
				{
					value.Due = Time.unscaledTime + Math.Max(1f, RewardWindowSeconds);
					continue;
				}
				long val = 2147483647L - (long)num2;
				int num3 = (int)Math.Min(num, Math.Max(0L, val));
				if (num3 == 0)
				{
					value.Due = Time.unscaledTime + Math.Max(1f, RewardWindowSeconds);
					continue;
				}
				value.Pending -= (decimal)num3;
				KohCredits.GrantResult grantResult = KohCredits.TryGrant(key, num3, "Logistics rewards (batched)");
				if (Rewards.TryGetValue(key, out var value2) && value2 == value)
				{
					if (grantResult == KohCredits.GrantResult.Unavailable)
					{
						value.Pending += (decimal)num3;
					}
					if (value.Pending >= 1m)
					{
						value.Due = Time.unscaledTime + Math.Max(1f, RewardWindowSeconds);
					}
				}
				if (grantResult == KohCredits.GrantResult.Granted && Cfg.PersonalBonus.AnnounceToPlayer && Connected(value.Player, key))
				{
					int num4 = KohCredits.TryGetBalance(key);
					HelperMethods.SendChatMessageToPlayer(value.Player, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " +" + num3 + " credits for logistics" + ((num4 >= 0) ? (" (balance " + num4 + ").") : "."));
				}
			}
		}
		NextRewardCheck = float.PositiveInfinity;
		foreach (RewardAccount value3 in Rewards.Values)
		{
			if (value3.Due > 0f)
			{
				NextRewardCheck = Math.Min(NextRewardCheck, value3.Due);
			}
		}
	}

	private static void BindRewardEvents()
	{
		BindHarvesterEvents();
		BindCommanderScoreEvents();
		GameEvents.OnGameStarted = (Action<GameMode>)Delegate.Remove(GameEvents.OnGameStarted, new Action<GameMode>(LogisticsMatchStarted));
		GameEvents.OnGameStarted = (Action<GameMode>)Delegate.Combine(GameEvents.OnGameStarted, new Action<GameMode>(LogisticsMatchStarted));
		GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Remove(GameEvents.OnGameEnded, new Action<GameMode, Team>(LogisticsMatchEnded));
		GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Combine(GameEvents.OnGameEnded, new Action<GameMode, Team>(LogisticsMatchEnded));
		GameEvents.OnPlayerLeft = (Action<Player>)Delegate.Remove(GameEvents.OnPlayerLeft, new Action<Player>(LogisticsPlayerLeft));
		GameEvents.OnPlayerLeft = (Action<Player>)Delegate.Combine(GameEvents.OnPlayerLeft, new Action<Player>(LogisticsPlayerLeft));
	}

	private static void LogisticsPlayerLeft(Player player)
	{
		CommanderPlayerLeft(player);
		foreach (KeyValuePair<long, ScoreAccount> item in new List<KeyValuePair<long, ScoreAccount>>(ScoreRewards))
		{
			if (item.Value.Player == player)
			{
				ScoreRewards.Remove(item.Key);
			}
		}
		foreach (KeyValuePair<long, RewardAccount> item2 in new List<KeyValuePair<long, RewardAccount>>(Rewards))
		{
			if (item2.Value.Player == player)
			{
				Rewards.Remove(item2.Key);
			}
		}
	}

	private static void LogisticsMatchStarted(GameMode mode)
	{
		ResetAITransports(stop: false);
		ResetHarvesterFix(resumeVanilla: false);
		ResetCommanderScore();
		RewardEpoch++;
		Rewards.Clear();
		ScoreRewards.Clear();
		NextRewardCheck = float.PositiveInfinity;
		TransportErrorUntil.Clear();
		CargoByTransport.Clear();
		MatchAcceptsRewards = true;
	}

	private static void LogisticsMatchEnded(GameMode mode, Team winner)
	{
		ResetAITransports(stop: false);
		ResetHarvesterFix(resumeVanilla: false);
		ResetCommanderScore();
		RewardEpoch++;
		MatchAcceptsRewards = false;
		Rewards.Clear();
		ScoreRewards.Clear();
		NextRewardCheck = float.PositiveInfinity;
		TransportErrorUntil.Clear();
		CargoByTransport.Clear();
	}

	public override void OnSceneWasInitialized(int buildIndex, string sceneName)
	{
		BindRewardEvents();
	}

	public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
	{
		ResetAITransports(stop: false);
		ResetHarvesterFix(resumeVanilla: false);
		ResetCommanderScore();
		RewardEpoch++;
		MatchAcceptsRewards = false;
		Rewards.Clear();
		ScoreRewards.Clear();
		NextRewardCheck = float.PositiveInfinity;
		TransportErrorUntil.Clear();
		CargoByTransport.Clear();
	}

	public override void OnDeinitializeMelon()
	{
		UnbindCommanderScoreEvents();
		ResetAITransports(stop: true);
		ResetHarvesterFix(resumeVanilla: true);
		GameEvents.OnUnitSpawned = (Action<Unit>)Delegate.Remove(GameEvents.OnUnitSpawned, new Action<Unit>(HarvesterSpawned));
		ResetCommanderScore();
		RewardEpoch++;
		MatchAcceptsRewards = false;
		Rewards.Clear();
		ScoreRewards.Clear();
		NextRewardCheck = float.PositiveInfinity;
		TransportErrorUntil.Clear();
		CargoByTransport.Clear();
		GameEvents.OnGameStarted = (Action<GameMode>)Delegate.Remove(GameEvents.OnGameStarted, new Action<GameMode>(LogisticsMatchStarted));
		GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Remove(GameEvents.OnGameEnded, new Action<GameMode, Team>(LogisticsMatchEnded));
		GameEvents.OnPlayerLeft = (Action<Player>)Delegate.Remove(GameEvents.OnPlayerLeft, new Action<Player>(LogisticsPlayerLeft));
		((MelonBase)this).HarmonyInstance.UnpatchSelf();
	}

	private void InstallHarvesterRewards()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected Obj, but got Unknown
		//IL_009d: Expected Obj, but got Unknown
		//IL_009d: Expected Obj, but got Unknown
		Type[] array = new Type[2]
		{
			typeof(MiningArmsBase),
			typeof(MiningClaws)
		};
		foreach (Type type in array)
		{
			string text = ((type == typeof(MiningArmsBase)) ? "ArmsBegin" : "ClawsBegin");
			((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(type, "OrderedUpdate", (Type[])null, (Type[])null), new HarmonyMethod(typeof(LogisticsMod), text, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(typeof(LogisticsMod), "ObserveDepositCalls", (Type[])null), new HarmonyMethod(typeof(LogisticsMod), "DepositFinished", (Type[])null), (HarmonyMethod)null);
		}
	}

	private static void ArmsBegin(MiningArmsBase __instance, out DepositReceipt __state)
	{
		__state = BeginDeposit((Unit)(object)__instance.Vehicle, __instance.ResourceHolder);
	}

	private static void ClawsBegin(MiningClaws __instance, out DepositReceipt __state)
	{
		__state = BeginDeposit((Unit)(object)__instance.Creature, __instance.ResourceHolder);
	}

	private static DepositReceipt BeginDeposit(Unit unit, ResourceHolder holder)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Invalid comparison between Unknown and I4
		if (DepositReceiptPool == null)
		{
			DepositReceiptPool = new Stack<DepositReceipt>();
		}
		DepositReceipt depositReceipt = ((DepositReceiptPool.Count > 0) ? DepositReceiptPool.Pop() : new DepositReceipt());
		depositReceipt.Parent = CurrentDeposit;
		depositReceipt.Epoch = RewardEpoch;
		if (Game.GetIsServer() && MatchAcceptsRewards && Cfg.Enabled && (Object)(object)unit != (Object)null && (Object)(object)((BaseGameObject)unit).ObjectInfo != (Object)null && (int)((BaseGameObject)unit).ObjectInfo.UnitType == 7 && (Object)(object)((BaseGameObject)unit).Team != (Object)null && (Object)(object)holder != (Object)null && (Object)(object)holder.ResourceType == (Object)(object)((BaseGameObject)unit).Team.UsableResource)
		{
			depositReceipt.Unit = unit;
			depositReceipt.Holder = holder;
			depositReceipt.Team = ((BaseGameObject)unit).Team;
		}
		CurrentDeposit = depositReceipt;
		return depositReceipt;
	}

	private static Exception DepositFinished(Exception __exception, DepositReceipt __state)
	{
		if (__state == null)
		{
			return __exception;
		}
		try
		{
			if (!__state.Consumed)
			{
				__state.Consumed = true;
				int amount = (int)Math.Min(2147483647L, Math.Min(__state.Retrieved, __state.Accepted));
				QueueReward(__state.Driver, amount, __state.Rate, __state.Epoch);
				AwardScore(__state.Driver, amount, __state.ScoreRate, transfer: false, __state.Epoch);
				CommanderDeposit(__state.Team, amount, __state.Epoch);
			}
		}
		catch (Exception ex)
		{
			DebugWarning("Logistics harvester reward: " + ex.Message);
		}
		finally
		{
			CurrentDeposit = __state.Parent;
			__state.Parent = null;
			__state.Unit = null;
			__state.Holder = null;
			__state.Driver = null;
			__state.Team = null;
			__state.Rate = 0m;
			__state.ScoreRate = 0;
			__state.Retrieved = (__state.Accepted = 0L);
			__state.Consumed = false;
			__state.Epoch = 0;
			if (DepositReceiptPool != null && DepositReceiptPool.Count < 8)
			{
				DepositReceiptPool.Push(__state);
			}
		}
		return __exception;
	}

	private static int ObservedRetrieve(ResourceHolder holder, int requested)
	{
		DepositReceipt currentDeposit = CurrentDeposit;
		Player driver = ((currentDeposit != null && (Object)(object)currentDeposit.Holder == (Object)(object)holder) ? DriverOf(currentDeposit.Unit) : null);
		decimal harvesterDepositRate = HarvesterDepositRate;
		int num = holder.RetrieveResource(requested);
		if (currentDeposit != null && (Object)(object)currentDeposit.Holder == (Object)(object)holder && num > 0)
		{
			currentDeposit.Driver = driver;
			currentDeposit.Rate = harvesterDepositRate;
			currentDeposit.ScoreRate = (Cfg.LogisticsScore ? Cfg.ScoreUnload : 0);
			currentDeposit.Retrieved += Math.Min(Math.Max(0, requested), num);
		}
		return num;
	}

	private static int ObservedStructureStore(BaseGameObject structure, Resource resource, int amount)
	{
		int num = structure.StoreResource(resource, amount);
		DepositReceipt currentDeposit = CurrentDeposit;
		if (currentDeposit != null && (Object)(object)currentDeposit.Team != (Object)null && (Object)(object)structure.Team == (Object)(object)currentDeposit.Team && (Object)(object)resource == (Object)(object)currentDeposit.Team.UsableResource)
		{
			currentDeposit.Accepted += Math.Max(0L, (long)amount - (long)Math.Max(0, num));
		}
		return num;
	}

	private static int ObservedTeamStore(Team team, int amount)
	{
		int num = team.StoreResource(amount);
		DepositReceipt currentDeposit = CurrentDeposit;
		if (currentDeposit != null && (Object)(object)currentDeposit.Team == (Object)(object)team)
		{
			currentDeposit.Accepted += Math.Max(0L, (long)amount - (long)Math.Max(0, num));
		}
		return num;
	}

	private static IEnumerable<CodeInstruction> ObserveDepositCalls(IEnumerable<CodeInstruction> instructions)
	{
		MethodInfo retrieve = AccessTools.Method(typeof(ResourceHolder), "RetrieveResource", new Type[1] { typeof(int) }, (Type[])null);
		MethodInfo store = AccessTools.Method(typeof(BaseGameObject), "StoreResource", new Type[2]
		{
			typeof(Resource),
			typeof(int)
		}, (Type[])null);
		MethodInfo teamStore = AccessTools.Method(typeof(Team), "StoreResource", new Type[1] { typeof(int) }, (Type[])null);
		int retrieves = 0;
		int stores = 0;
		int teamStores = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			string text = null;
			if (CodeInstructionExtensions.Calls(instruction, retrieve))
			{
				text = "ObservedRetrieve";
				retrieves++;
			}
			else if (CodeInstructionExtensions.Calls(instruction, store))
			{
				text = "ObservedStructureStore";
				stores++;
			}
			else if (CodeInstructionExtensions.Calls(instruction, teamStore))
			{
				text = "ObservedTeamStore";
				teamStores++;
			}
			if (text != null)
			{
				instruction.opcode = OpCodes.Call;
				instruction.operand = AccessTools.Method(typeof(LogisticsMod), text, (Type[])null, (Type[])null);
			}
			yield return instruction;
		}
		if (retrieves != 1 || stores != 1 || teamStores != 1)
		{
			throw new InvalidOperationException("Native harvester deposit call sites changed; expected one retrieve and two storage paths.");
		}
	}

	private static void AwardScore(Player player, int amount, int rate, bool transfer, int epoch)
	{
		if (epoch != RewardEpoch || !Game.GetIsServer() || !MatchAcceptsRewards || !Cfg.Enabled || !Cfg.LogisticsScore || amount <= 0 || rate <= 0)
		{
			return;
		}
		long num = SteamId(player);
		if (!Connected(player, num))
		{
			return;
		}
		try
		{
			if (!ScoreRewards.TryGetValue(num, out var value) || value.Player != player)
			{
				Dictionary<long, ScoreAccount> scoreRewards = ScoreRewards;
				long key = num;
				ScoreAccount scoreAccount = new ScoreAccount
				{
					Player = player
				};
				value = scoreAccount;
				scoreRewards[key] = scoreAccount;
			}
			long num2 = (long)amount * (long)rate + (transfer ? value.TransferRemainder : value.UnloadRemainder);
			if (transfer)
			{
				value.TransferRemainder = (int)(num2 % 100);
			}
			else
			{
				value.UnloadRemainder = (int)(num2 % 100);
			}
			long num3 = num2 / 100;
			if (num3 != 0L)
			{
				int score = player.Score;
				player.Score = (int)Math.Min(2147483647L, score + num3);
				if (Cfg.Debug)
				{
					DebugMsg("Logistics score player=" + num + " +" + ((long)player.Score - (long)score) + " for " + amount + " Balterium " + (transfer ? "transferred" : "unloaded"));
				}
			}
		}
		catch (Exception ex)
		{
			DebugWarning("Logistics score: " + ex.Message);
		}
	}

	private static void OnScoreCommand(Player caller, string args)
	{
		try
		{
			if ((Object)(object)caller != (Object)null && (!caller.IsAdmin() || !AdminMethods.PowerInPowers(Power.Generic, caller.GetAdminPowers())))
			{
				Reply(caller, "Admin only.");
				return;
			}
			string[] array = (args ?? string.Empty).Trim().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string text = ((array.Length != 0) ? array[0].TrimStart('/').ToLowerInvariant() : string.Empty);
			if (text == "logisticsscore")
			{
				string text2 = ((array.Length == 2) ? array[1].ToLowerInvariant() : string.Empty);
				if (text2 != "on" && text2 != "off")
				{
					Reply(caller, "Usage: /logisticsscore on|off");
					return;
				}
				Cfg.LogisticsScore = text2 == "on";
				if (!Cfg.LogisticsScore)
				{
					ScoreRewards.Clear();
				}
				SaveConfig();
				Reply(caller, "Logistics score " + (Cfg.LogisticsScore ? "ON." : "OFF."));
			}
			else
			{
				if (text != "scoreunload" && text != "scoretransfer")
				{
					return;
				}
				if (array.Length != 2 || !int.TryParse(array[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) || result < 0 || result > 100)
				{
					Reply(caller, "Usage: /" + text + " <0..100> points per 100 Balterium");
					return;
				}
				if (text == "scoreunload")
				{
					Cfg.ScoreUnload = result;
				}
				else
				{
					Cfg.ScoreTransfer = result;
				}
				SaveConfig();
				Reply(caller, text + " = " + result + " points per 100 Balterium.");
			}
		}
		catch (Exception ex)
		{
			DebugWarning("Logistics score command: " + ex.Message);
		}
	}

	private static void ValidateCommanderScoreConfig()
	{
		if (Cfg.CommanderScore == null)
		{
			Cfg.CommanderScore = new CommanderScoreConfig();
		}
		if (Cfg.CommanderScore.Rules == null)
		{
			Cfg.CommanderScore.Rules = new List<CommanderScoreRule>();
		}
		Cfg.CommanderScore.Rules.RemoveAll((CommanderScoreRule rule) => rule == null);
		foreach (CommanderScoreRule rule in Cfg.CommanderScore.Rules)
		{
			rule.Divisor = Math.Max(1, rule.Divisor);
			rule.Minimum = Math.Max(0, rule.Minimum);
			if (rule.Objects == null)
			{
				rule.Objects = new List<string>();
			}
			if (rule.Overrides == null)
			{
				rule.Overrides = new Dictionary<string, int>();
			}
		}
		CommanderFractions.Clear();
	}

	private void InstallCommanderScore()
	{
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Expected Obj, but got Unknown
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Expected Obj, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected Obj, but got Unknown
		//IL_00cc: Expected Obj, but got Unknown
		Type type = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
			select assembly.GetType("Si_KingOfTheHill.KingOfTheHill", throwOnError: false)).FirstOrDefault((Type type2) => type2 != null);
		CommanderPurchaseHookReady = type == null;
		try
		{
			if (type != null)
			{
				MethodInfo methodInfo = AccessTools.GetDeclaredMethods(type).Single((MethodInfo method) => method.Name == "TryPurchase" && method.IsStatic && method.ReturnType == typeof(void) && method.GetParameters().Length == 2 && method.GetParameters()[0].ParameterType == typeof(Player));
				((MelonBase)this).HarmonyInstance.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(LogisticsMod), "CommanderPurchaseBegin", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(LogisticsMod), "CommanderPurchaseEnd", (Type[])null), (HarmonyMethod)null);
			}
			MethodInfo methodInfo2 = AccessTools.Method(typeof(Game), "SpawnPrefab", new Type[7]
			{
				typeof(GameObject),
				typeof(Player),
				typeof(Team),
				typeof(Vector3),
				typeof(Quaternion),
				typeof(bool),
				typeof(bool)
			}, (Type[])null);
			((MelonBase)this).HarmonyInstance.Patch((MethodBase)methodInfo2, (HarmonyMethod)null, new HarmonyMethod(typeof(LogisticsMod), "CommanderSpawnedPrefab", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			MethodInfo methodInfo3 = AccessTools.Method(typeof(Structure), "ConstructNonPlaceableInstant", new Type[3]
			{
				typeof(ConstructionData),
				typeof(Vector3),
				typeof(Quaternion)
			}, (Type[])null);
			((MelonBase)this).HarmonyInstance.Patch((MethodBase)methodInfo3, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(LogisticsMod), "CommanderObserveInstantSpawn", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
			CommanderPurchaseHookReady = true;
			DebugMsg("Commander score hooks ready; KoH " + ((type == null) ? "not installed" : "purchase exclusion installed") + ".");
		}
		catch (Exception ex)
		{
			CommanderPurchaseHookReady = false;
			DebugWarning("Commander score disabled: production/purchase hooks unavailable: " + ex.Message);
		}
	}

	private static void CommanderPurchaseBegin(out int __state)
	{
		__state = CommanderPurchaseDepth;
		CommanderPurchaseDepth++;
	}

	private static Exception CommanderPurchaseEnd(Exception __exception, int __state)
	{
		CommanderPurchaseDepth = __state;
		return __exception;
	}

	private static IEnumerable<CodeInstruction> CommanderObserveInstantSpawn(IEnumerable<CodeInstruction> instructions)
	{
		MethodInfo spawn = AccessTools.Method(typeof(GameMode), "SpawnUnit", new Type[6]
		{
			typeof(GameObject),
			typeof(Vector3),
			typeof(Quaternion),
			typeof(Team),
			typeof(bool),
			typeof(bool)
		}, (Type[])null);
		int calls = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (CodeInstructionExtensions.Calls(instruction, spawn))
			{
				instruction.opcode = OpCodes.Call;
				instruction.operand = AccessTools.Method(typeof(LogisticsMod), "CommanderInstantSpawn", (Type[])null, (Type[])null);
				calls++;
			}
			yield return instruction;
		}
		if (calls != 1)
		{
			throw new InvalidOperationException("Instant production spawn path changed.");
		}
	}

	private static Unit CommanderInstantSpawn(GameMode mode, GameObject prefab, Vector3 position, Quaternion rotation, Team team, bool sendNetInit, bool sendNetSpawn)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Unit val = mode.SpawnUnit(prefab, position, rotation, team, sendNetInit, sendNetSpawn);
		try
		{
			if ((Object)(object)val != (Object)null)
			{
				CommanderProductionCompleted((BaseGameObject)(object)val);
			}
		}
		catch (Exception ex)
		{
			DebugWarning("Commander score instant production: " + ex.Message);
		}
		return val;
	}

	private static void CommanderSpawnedPrefab(GameObject __result)
	{
		if (!Game.GetIsServer() || (Object)(object)__result == (Object)null)
		{
			return;
		}
		try
		{
			BaseGameObject baseGameObject = GameFuncs.GetBaseGameObject(__result, true);
			if (!((Object)(object)baseGameObject == (Object)null) && CommanderPurchaseDepth > 0)
			{
				CommanderBought.Add(baseGameObject);
			}
		}
		catch (Exception ex)
		{
			DebugWarning("Commander score spawn observation: " + ex.Message);
		}
	}

	private static void BindCommanderScoreEvents()
	{
		UnbindCommanderScoreEvents();
		GameEvents.OnConstructionCompleted = (Action<ConstructionSite, bool>)Delegate.Combine(GameEvents.OnConstructionCompleted, new Action<ConstructionSite, bool>(CommanderConstructionCompleted));
		GameEvents.OnTargetDestroyed = (Action<Target, GameObject>)Delegate.Combine(GameEvents.OnTargetDestroyed, new Action<Target, GameObject>(CommanderTargetDestroyed));
		GameEvents.OnBaseGameObjectDeleted = (Action<BaseGameObject>)Delegate.Combine(GameEvents.OnBaseGameObjectDeleted, new Action<BaseGameObject>(CommanderObjectDeleted));
		GameEvents.OnTeamTechnologyTierChanged = (Action<Team, int, int>)Delegate.Combine(GameEvents.OnTeamTechnologyTierChanged, new Action<Team, int, int>(CommanderTierChanged));
		GameEvents.OnGameRestart = (Action<GameMode>)Delegate.Combine(GameEvents.OnGameRestart, new Action<GameMode>(CommanderGameRestarted));
		if (CommanderHighestTier.Count == 0)
		{
			SeedCommanderTiers();
		}
	}

	private static void UnbindCommanderScoreEvents()
	{
		GameEvents.OnConstructionCompleted = (Action<ConstructionSite, bool>)Delegate.Remove(GameEvents.OnConstructionCompleted, new Action<ConstructionSite, bool>(CommanderConstructionCompleted));
		GameEvents.OnTargetDestroyed = (Action<Target, GameObject>)Delegate.Remove(GameEvents.OnTargetDestroyed, new Action<Target, GameObject>(CommanderTargetDestroyed));
		GameEvents.OnBaseGameObjectDeleted = (Action<BaseGameObject>)Delegate.Remove(GameEvents.OnBaseGameObjectDeleted, new Action<BaseGameObject>(CommanderObjectDeleted));
		GameEvents.OnTeamTechnologyTierChanged = (Action<Team, int, int>)Delegate.Remove(GameEvents.OnTeamTechnologyTierChanged, new Action<Team, int, int>(CommanderTierChanged));
		GameEvents.OnGameRestart = (Action<GameMode>)Delegate.Remove(GameEvents.OnGameRestart, new Action<GameMode>(CommanderGameRestarted));
	}

	private static void SeedCommanderTiers()
	{
		if (Team.Teams == null)
		{
			return;
		}
		foreach (Team team in Team.Teams)
		{
			if ((Object)(object)team != (Object)null)
			{
				CommanderHighestTier[team] = Math.Max(team.TechnologyTier, team.TechnologyTierHighestReached);
			}
		}
	}

	private static void ResetCommanderScore()
	{
		CommanderBought.Clear();
		CommanderProduced.Clear();
		CommanderDestroyed.Clear();
		CommanderHighestTier.Clear();
		CommanderFractions.Clear();
		SeedCommanderTiers();
	}

	private static void CommanderGameRestarted(GameMode mode)
	{
		ResetCommanderScore();
	}

	private static void CommanderObjectDeleted(BaseGameObject obj)
	{
		CommanderBought.Remove(obj);
		CommanderProduced.Remove(obj);
		CommanderDestroyed.Remove(obj);
	}

	private static void CommanderPlayerLeft(Player player)
	{
		foreach (Dictionary<Tuple<Player, Team>, decimal> value in CommanderFractions.Values)
		{
			foreach (Tuple<Player, Team> item in value.Keys.Where((Tuple<Player, Team> key) => key.Item1 == player).ToList())
			{
				value.Remove(item);
			}
		}
	}

	private static void CommanderConstructionCompleted(ConstructionSite site, bool completed)
	{
		if (!Game.GetIsServer() || !completed || (Object)(object)site == (Object)null || site.IsDeconstructing)
		{
			return;
		}
		try
		{
			if ((Object)(object)site.SpawnedGameObject != (Object)null)
			{
				CommanderProductionCompleted(site.SpawnedGameObject);
			}
		}
		catch (Exception ex)
		{
			DebugWarning("Commander score construction: " + ex.Message);
		}
	}

	private static void CommanderProductionCompleted(BaseGameObject obj)
	{
		if (CommanderMatchActive && !((Object)(object)obj == (Object)null) && CommanderPurchaseDepth <= 0 && !CommanderBought.Contains(obj) && CommanderProduced.Add(obj))
		{
			if (obj is Unit)
			{
				ApplyCommanderRules(obj.Team, "UnitProduced", obj.ObjectInfo);
			}
			else if (obj is Structure)
			{
				ApplyCommanderRules(obj.Team, "BuildingCompleted", obj.ObjectInfo);
			}
		}
	}

	private static bool CommanderPlayerControlled(Unit unit)
	{
		if ((Object)(object)unit != (Object)null)
		{
			if (!unit.PlayerControlled)
			{
				return (Object)(object)DriverOf(unit) != (Object)null;
			}
			return true;
		}
		return false;
	}

	private static void CommanderTargetDestroyed(Target target, GameObject instigator)
	{
		if (!CommanderMatchActive || (Object)(object)target == (Object)null)
		{
			return;
		}
		try
		{
			BaseGameObject owner = target.Owner;
			if ((Object)(object)owner == (Object)null || (!(owner is Unit) && !(owner is Structure)) || !CommanderDestroyed.Add(owner))
			{
				return;
			}
			int num;
			if (CommanderPurchaseDepth <= 0)
			{
				num = (CommanderBought.Contains(owner) ? 1 : 0);
				if (num == 0)
				{
					Unit val = (Unit)(object)((owner is Unit) ? owner : null);
					if (val == null || !CommanderPlayerControlled(val))
					{
						ApplyCommanderRules(owner.Team, (owner is Unit) ? "UnitLost" : "BuildingLost", owner.ObjectInfo);
					}
				}
			}
			else
			{
				num = 1;
			}
			BaseGameObject val2 = (((Object)(object)instigator != (Object)null) ? GameFuncs.GetBaseGameObject(instigator, true) : null);
			if (num != 0 || (Object)(object)val2 == (Object)null || CommanderBought.Contains(val2) || !Team.GetTeamsAreEnemy(val2.Team, owner.Team))
			{
				return;
			}
			Unit val3 = (Unit)(object)((val2 is Unit) ? val2 : null);
			if (val3 != null)
			{
				if (CommanderPlayerControlled(val3) || !val3.AIControlled)
				{
					return;
				}
			}
			else if (!(val2 is Structure))
			{
				return;
			}
			if (!(val2 is Structure) || !((Object)(object)val2.NetworkComponent != (Object)null) || !((Object)(object)val2.NetworkComponent.OwnerPlayer != (Object)null))
			{
				ApplyCommanderRules(val2.Team, (owner is Unit) ? "EnemyUnitDestroyed" : "EnemyBuildingDestroyed", owner.ObjectInfo);
			}
		}
		catch (Exception ex)
		{
			DebugWarning("Commander score destruction: " + ex.Message);
		}
	}

	private static void CommanderTierChanged(Team team, int previous, int current)
	{
		if (!Game.GetIsServer() || (Object)(object)team == (Object)null)
		{
			return;
		}
		try
		{
			int num = (CommanderHighestTier.TryGetValue(team, out var value) ? Math.Max(value, previous) : previous);
			CommanderHighestTier[team] = Math.Max(num, current);
			if (CommanderMatchActive)
			{
				for (int i = num + 1; i <= current; i++)
				{
					ApplyCommanderRules(team, "TechnologyReached", null);
				}
			}
		}
		catch (Exception ex)
		{
			DebugWarning("Commander score technology: " + ex.Message);
		}
	}

	private static Player CurrentCommander(Team team)
	{
		GameMode currentGameMode = GameMode.CurrentGameMode;
		GameModeExt val = (GameModeExt)(object)((currentGameMode is GameModeExt) ? currentGameMode : null);
		Player val2 = (((Object)(object)team != (Object)null && (Object)(object)val != (Object)null) ? val.GetCommanderForTeam(team) : null);
		if (!Connected(val2, SteamId(val2)))
		{
			return null;
		}
		return val2;
	}

	private static bool CommanderNameMatches(string name, ObjectInfo info)
	{
		if ((Object)(object)info != (Object)null)
		{
			if (!string.Equals(name, ((Object)info).name, StringComparison.OrdinalIgnoreCase))
			{
				return string.Equals(name, info.DisplayName, StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		return false;
	}

	private static decimal CommanderRulePoints(CommanderScoreRule rule, ObjectInfo info, int amount)
	{
		foreach (KeyValuePair<string, int> @override in rule.Overrides)
		{
			if (CommanderNameMatches(@override.Key, info))
			{
				return @override.Value;
			}
		}
		string text = (rule.ValueSource ?? "Cost").ToLowerInvariant();
		if (text == "fixed")
		{
			return (decimal)rule.FixedPoints * (decimal)rule.Multiplier;
		}
		if (text == "amount")
		{
			return (decimal)amount / (decimal)rule.Divisor * (decimal)rule.Multiplier;
		}
		if ((Object)(object)info == (Object)null)
		{
			return 0m;
		}
		if (text != "cost" && text != "score")
		{
			return 0m;
		}
		int val = ((text == "score") ? info.Score : info.Cost);
		return Math.Max(rule.Minimum, Math.Floor((decimal)Math.Max(0, val) / (decimal)rule.Divisor)) * (decimal)rule.Multiplier;
	}

	private static void ApplyCommanderRules(Team team, string eventName, ObjectInfo info, int amount = 0)
	{
		if (!CommanderScoringActive)
		{
			return;
		}
		Player val = CurrentCommander(team);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		foreach (CommanderScoreRule rule in Cfg.CommanderScore.Rules)
		{
			if (!rule.Enabled || !string.Equals(rule.Event, eventName, StringComparison.OrdinalIgnoreCase) || (rule.Objects.Count > 0 && !rule.Objects.Any((string name) => CommanderNameMatches(name, info))))
			{
				continue;
			}
			decimal num = CommanderRulePoints(rule, info, amount);
			if (eventName == "BalteriumDeposited")
			{
				if (!CommanderFractions.TryGetValue(rule, out var value))
				{
					value = (CommanderFractions[rule] = new Dictionary<Tuple<Player, Team>, decimal>());
				}
				Tuple<Player, Team> key = Tuple.Create<Player, Team>(val, team);
				value.TryGetValue(key, out var value2);
				decimal num2 = num + value2;
				num = decimal.Truncate(num2);
				value[key] = num2 - num;
			}
			if (!(num == 0m))
			{
				int score = val.Score;
				val.Score = (int)Math.Max(-2147483648m, Math.Min(2147483647m, (decimal)score + decimal.Truncate(num)));
				if (Cfg.Debug)
				{
					DebugMsg("Commander score player=" + SteamId(val) + " event=" + eventName + " object=" + (((Object)(object)info != (Object)null) ? info.DisplayName : "team") + " delta=" + ((long)val.Score - (long)score));
				}
			}
		}
	}

	private static void CommanderDeposit(Team team, int amount, int epoch)
	{
		if (epoch != RewardEpoch || (Object)(object)team == (Object)null || amount <= 0)
		{
			return;
		}
		try
		{
			ApplyCommanderRules(team, "BalteriumDeposited", null, amount);
		}
		catch (Exception ex)
		{
			DebugWarning("Commander score deposit: " + ex.Message);
		}
	}

	private static void OnCommanderScoreCommand(Player caller, string args)
	{
		try
		{
			if ((Object)(object)caller != (Object)null && (!caller.IsAdmin() || !AdminMethods.PowerInPowers(Power.Generic, caller.GetAdminPowers())))
			{
				Reply(caller, "Admin only.");
				return;
			}
			string[] array = (args ?? string.Empty).Trim().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string text = ((array.Length == 2) ? array[1].ToLowerInvariant() : string.Empty);
			if (text != "on" && text != "off")
			{
				Reply(caller, "Usage: /logisticsscorecomander on|off");
				return;
			}
			Cfg.CommanderScore.Enabled = text == "on";
			if (!Cfg.CommanderScore.Enabled)
			{
				CommanderFractions.Clear();
			}
			SaveConfig();
			Reply(caller, "Commander score " + (Cfg.CommanderScore.Enabled ? "ON." : "OFF."));
		}
		catch (Exception ex)
		{
			DebugWarning("Commander score command: " + ex.Message);
		}
	}

	private static void BindHarvesterEvents()
	{
		GameEvents.OnUnitSpawned = (Action<Unit>)Delegate.Remove(GameEvents.OnUnitSpawned, new Action<Unit>(HarvesterSpawned));
		GameEvents.OnUnitSpawned = (Action<Unit>)Delegate.Combine(GameEvents.OnUnitSpawned, new Action<Unit>(HarvesterSpawned));
	}

	private static float ValidHarvestSetting(float value, float fallback, float minimum, float maximum)
	{
		if (!float.IsNaN(value) && !float.IsInfinity(value))
		{
			return Mathf.Clamp(value, minimum, maximum);
		}
		return fallback;
	}

	private static void ValidateHarvesterFixConfig()
	{
		if (Cfg.HarvesterFix == null)
		{
			Cfg.HarvesterFix = new HarvesterFixConfig();
		}
		HarvesterFixConfig harvesterFix = Cfg.HarvesterFix;
		harvesterFix.MaximumReservationsPerRefinery = Math.Max(1, Math.Min(32, harvesterFix.MaximumReservationsPerRefinery));
		harvesterFix.NearPatchDistanceMetres = ValidHarvestSetting(harvesterFix.NearPatchDistanceMetres, 400f, 0f, 100000f);
		harvesterFix.MediumPatchDistanceMetres = ValidHarvestSetting(harvesterFix.MediumPatchDistanceMetres, 800f, harvesterFix.NearPatchDistanceMetres, 100000f);
		harvesterFix.NearPatchReservations = Math.Max(1, Math.Min(32, harvesterFix.NearPatchReservations));
		harvesterFix.MediumPatchReservations = Math.Max(1, Math.Min(32, harvesterFix.MediumPatchReservations));
		harvesterFix.FarPatchReservations = Math.Max(1, Math.Min(32, harvesterFix.FarPatchReservations));
		harvesterFix.TransferWaitingTimeSeconds = ValidHarvestSetting(harvesterFix.TransferWaitingTimeSeconds, 120f, 0f, 3600f);
		harvesterFix.ReservationCheckTimeSeconds = ValidHarvestSetting(harvesterFix.ReservationCheckTimeSeconds, 10f, 1f, 300f);
		harvesterFix.ReservationDistanceIncrementMetres = ValidHarvestSetting(harvesterFix.ReservationDistanceIncrementMetres, 50f, 25f, 2000f);
	}

	private void InstallHarvesterFix()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected Obj, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected Obj, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected Obj, but got Unknown
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected Obj, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected Obj, but got Unknown
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Expected Obj, but got Unknown
		//IL_0175: Expected Obj, but got Unknown
		((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(AIHarvesterAgent), "OnOrderedUpdate", (Type[])null, (Type[])null), new HarmonyMethod(typeof(LogisticsMod), "HarvestUpdatePrefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(AIHarvesterAgent), "HandleOrderReturnToBase", (Type[])null, (Type[])null), new HarmonyMethod(typeof(LogisticsMod), "HarvestReturnPrefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(AIHarvesterAgent), "Harvest", (Type[])null, (Type[])null), new HarmonyMethod(typeof(LogisticsMod), "HarvestAreaPrefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(AIHarvesterAgent), "FindDepositPoint", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(typeof(LogisticsMod), "HarvestFindDepositPostfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(AIHarvesterAgent), "Deposit", (Type[])null, (Type[])null), new HarmonyMethod(typeof(LogisticsMod), "HarvestDepositPrefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(AIOrderProcessor), "IssueOrder", (Type[])null, (Type[])null), new HarmonyMethod(typeof(LogisticsMod), "HarvestOrderPrefix", (Type[])null), new HarmonyMethod(typeof(LogisticsMod), "HarvestOrderPostfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		BindHarvesterEvents();
		HarvestFixInstalled = true;
	}

	private static void HarvesterSpawned(Unit unit)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		if (HarvestFixActive && Object.op_Implicit((Object)(object)unit) && Object.op_Implicit((Object)(object)((BaseGameObject)unit).ObjectInfo) && (int)((BaseGameObject)unit).ObjectInfo.UnitType == 7)
		{
			NewHarvesters.Add(((Object)unit).GetInstanceID());
		}
	}

	private static void ShowHarvesterStatus(Player caller)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (HarvestJob value in HarvestJobs.Values)
		{
			if (value.PatchReserved)
			{
				num++;
			}
			if (value.Line != null)
			{
				num2++;
			}
			if (value.Phase != HarvestPhase.Manual)
			{
				num3++;
			}
		}
		HarvesterFixConfig harvesterFix = Cfg.HarvesterFix;
		Reply(caller, "Harvester fix " + (harvesterFix.Enabled ? "ON" : "OFF (vanilla harvester AI)") + " | managed " + num3 + ", patch reservations " + num + ", refinery reservations " + num2 + " | refinery slots " + harvesterFix.MaximumReservationsPerRefinery + " | home patch slots " + harvesterFix.NearPatchReservations + "/" + harvesterFix.MediumPatchReservations + "/" + harvesterFix.FarPatchReservations + " at <=" + harvesterFix.NearPatchDistanceMetres + "m / <" + harvesterFix.MediumPatchDistanceMetres + "m / farther | pickup wait " + harvesterFix.TransferWaitingTimeSeconds + "s, check " + harvesterFix.ReservationCheckTimeSeconds + "s, first wait 100m, subsequent spacing " + harvesterFix.ReservationDistanceIncrementMetres + "m");
		if (!Cfg.Enabled && harvesterFix.Enabled)
		{
			Reply(caller, "Harvester fix paused by Logistics master switch.");
		}
		if (Cfg.Debug)
		{
			DumpHarvesterStatus();
		}
	}

	private static int HarvestObjectId(Object value)
	{
		if (!Object.op_Implicit(value))
		{
			return 0;
		}
		return value.GetInstanceID();
	}

	private static void HarvestDiagnostic(HarvestJob j, string reason)
	{
		if (!(j.LastDiagnostic == reason))
		{
			j.LastDiagnostic = reason;
			DebugMsg("[HarvesterFix] unit=" + HarvestObjectId((Object)(object)((AIBaseAgent)j.Agent).Unit) + " " + reason);
		}
	}

	private static void DumpHarvesterStatus()
	{
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		DebugMsg("[HarvesterFix] status enabled=" + Cfg.HarvesterFix.Enabled + " active=" + HarvestFixActive);
		foreach (HarvestJob value in HarvestJobs.Values)
		{
			AIHarvesterAgent agent = value.Agent;
			if (Object.op_Implicit((Object)(object)agent) && Object.op_Implicit((Object)(object)((AIBaseAgent)agent).Unit) && Object.op_Implicit((Object)(object)agent.ResourceHolder))
			{
				DebugMsg("[HarvesterFix] unit=" + HarvestObjectId((Object)(object)((AIBaseAgent)agent).Unit) + " phase=" + value.Phase.ToString() + " dock=" + ((object)agent.DockState/*cast due to constrained. prefix*/).ToString() + " cargo=" + agent.ResourceHolder.AmountStored + "/" + agent.ResourceHolder.MaxAmount + " teamRoom=" + (Object.op_Implicit((Object)(object)((AIBaseAgent)agent).Team) ? ((AIBaseAgent)agent).Team.RemainingResourceCapacity : 0) + " refinery=" + ((value.Line != null) ? HarvestObjectId((Object)(object)value.Line.Refinery) : 0) + " slot=" + (value.LastSlot + 1) + " nativePoint=" + HarvestObjectId((Object)(object)agent.CurrentDepositPoint) + " path=" + ((AIBaseAgent)agent).IsFollowingPath + " calculating=" + ((AIBaseAgent)agent).IsCalculatingPath + " forceMove=" + agent.Vehicle.HasForceMove + " homePatch=" + HarvestObjectId((Object)(object)value.Area) + " patchReserved=" + value.PatchReserved + " patchLimit=" + PatchReservationLimit(value, value.Area) + " pickupWait=" + Math.Max(0f, value.PickupUntil - Time.time).ToString("F1") + " mining=" + agent.MiningArms.IsMining + " extracting=" + agent.MiningArms.IsExtracting + " position=" + ((object)((Component)((AIBaseAgent)agent).Unit).transform.position/*cast due to constrained. prefix*/).ToString() + " reason=" + value.LastDiagnostic);
			}
		}
		foreach (Structure structure in Structure.Structures)
		{
			if (!Object.op_Implicit((Object)(object)structure) || structure.ResourceDepositPoints == null)
			{
				continue;
			}
			foreach (ResourceDepositPoint resourceDepositPoint in structure.ResourceDepositPoints)
			{
				if (Object.op_Implicit((Object)(object)resourceDepositPoint))
				{
					DebugMsg("[HarvesterFix] refinery=" + HarvestObjectId((Object)(object)structure) + " point=" + HarvestObjectId((Object)(object)resourceDepositPoint) + " reserved=" + HarvestObjectId((Object)(object)resourceDepositPoint.ReservedByUnit) + " occupied=" + HarvestObjectId((Object)(object)resourceDepositPoint.OccupiedByUnit));
				}
			}
		}
	}

	private static bool HarvestEligible(AIHarvesterAgent a)
	{
		if (Object.op_Implicit((Object)(object)a) && Object.op_Implicit((Object)(object)((AIBaseAgent)a).Unit) && !((BaseGameObject)((AIBaseAgent)a).Unit).IsDestroyed && Object.op_Implicit((Object)(object)((AIBaseAgent)a).Team) && ((AIBaseAgent)a).AIControlled && !((AIBaseAgent)a).Unit.PlayerControlled && (Object)(object)DriverOf(((AIBaseAgent)a).Unit) == (Object)null && !Object.op_Implicit((Object)(object)((AIBaseAgent)a).Unit.InCompartment) && Object.op_Implicit((Object)(object)a.ResourceHolder) && Object.op_Implicit((Object)(object)a.MiningArms) && Object.op_Implicit((Object)(object)((BaseGameObject)((AIBaseAgent)a).Unit).OrderAgent))
		{
			return Object.op_Implicit((Object)(object)((AIBaseAgent)a).AgentPathfinding);
		}
		return false;
	}

	private static HarvestJob GetHarvestJob(AIHarvesterAgent a)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		int instanceID = ((Object)((AIBaseAgent)a).Unit).GetInstanceID();
		List<AIOrder> orders;
		bool flag;
		int num;
		if (!HarvestJobs.TryGetValue(instanceID, out var value))
		{
			value = new HarvestJob
			{
				Agent = a,
				Team = ((AIBaseAgent)a).Team,
				Area = a.CurrentHarvestArea,
				ReadyAt = Time.time + 10f,
				Phase = HarvestPhase.Starting
			};
			HarvestJobs.Add(instanceID, value);
			orders = ((BaseGameObject)((AIBaseAgent)a).Unit).OrderAgent.Orders;
			flag = NewHarvesters.Remove(instanceID);
			if (a.ResourceHolder.RemainingSpace == 0 && !((AIBaseAgent)a).IsFollowingPath && !((AIBaseAgent)a).IsCalculatingPath && orders.Count > 0)
			{
				AIOrder val = orders[0];
				AIMoveOrder val2 = (AIMoveOrder)(object)((val is AIMoveOrder) ? val : null);
				if (val2 != null)
				{
					num = ((Vector3.Distance(val2.WorldPosition, ((Component)a).transform.position) <= Math.Max(15f, ((BaseGameObject)((AIBaseAgent)a).Unit).PhysicalRadius)) ? 1 : 0);
					goto IL_00ef;
				}
			}
			num = 0;
			goto IL_00ef;
		}
		goto IL_012f;
		IL_012f:
		return value;
		IL_00ef:
		bool flag2 = (byte)num != 0;
		if (!flag && !flag2 && orders.Count > 0 && !(orders[0] is AIHarvestOrder) && !(orders[0] is AIDepositOrder))
		{
			value.Phase = HarvestPhase.Manual;
		}
		if (flag2)
		{
			HarvestDiagnostic(value, "adopted full harvester in completed vanilla fallback move");
		}
		goto IL_012f;
	}

	private static void UpdateHarvesterFix()
	{
		if (!HarvestFixActive)
		{
			if (HarvestJobs.Count > 0)
			{
				ResetHarvesterFix(resumeVanilla: true);
			}
		}
		else
		{
			if (Time.time < NextHarvestScan)
			{
				return;
			}
			NextHarvestScan = Time.time + 1f;
			foreach (KeyValuePair<int, HarvestJob> item in new List<KeyValuePair<int, HarvestJob>>(HarvestJobs))
			{
				if (!HarvestEligible(item.Value.Agent) || (Object)(object)item.Value.Team != (Object)(object)((AIBaseAgent)item.Value.Agent).Team)
				{
					ReleaseHarvestReservations(item.Value);
					HarvestJobs.Remove(item.Key);
				}
			}
			foreach (HarvestJob value in HarvestJobs.Values)
			{
				if (value.PatchReserved && (!Object.op_Implicit((Object)(object)value.Area) || value.Area.ResourceAmountCurrent <= 0))
				{
					value.PatchReserved = false;
					HarvestDiagnostic(value, "home patch depleted: assignment released");
				}
			}
			if (Unit.Units == null)
			{
				return;
			}
			foreach (Unit unit in Unit.Units)
			{
				if (Object.op_Implicit((Object)(object)unit))
				{
					AIBaseAgent aIAgent = unit.AIAgent;
					AIHarvesterAgent a = (AIHarvesterAgent)(object)((aIAgent is AIHarvesterAgent) ? aIAgent : null);
					if (HarvestEligible(a))
					{
						GetHarvestJob(a);
					}
				}
			}
		}
	}

	private static void ReleaseHarvestReservations(HarvestJob j)
	{
		j.PatchReserved = false;
		ReleaseRefineryReservation(j);
	}

	private static void ReleaseRefineryReservation(HarvestJob j)
	{
		if (j.Line != null)
		{
			RefineryLine line = j.Line;
			line.Jobs.Remove(j);
			if (Object.op_Implicit((Object)(object)line.Point) && Object.op_Implicit((Object)(object)j.Agent) && (Object)(object)line.Point.ReservedByUnit == (Object)(object)((AIBaseAgent)j.Agent).Unit)
			{
				line.Point.ReservedByUnit = null;
			}
			j.Line = null;
			j.LastSlot = -1;
			if (line.Jobs.Count == 0 && Object.op_Implicit((Object)(object)line.Refinery))
			{
				RefineryLines.Remove(((Object)line.Refinery).GetInstanceID());
			}
		}
	}

	private static void ResetHarvesterFix(bool resumeVanilla)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		List<HarvestJob> list = new List<HarvestJob>(HarvestJobs.Values);
		foreach (HarvestJob item in list)
		{
			ReleaseHarvestReservations(item);
		}
		HarvestJobs.Clear();
		RefineryLines.Clear();
		NewHarvesters.Clear();
		NextHarvestScan = 0f;
		if (!resumeVanilla || !Game.GetIsServer())
		{
			return;
		}
		foreach (HarvestJob item2 in list)
		{
			if (!HarvestEligible(item2.Agent) || item2.Phase == HarvestPhase.Manual || item2.Phase == HarvestPhase.Starting || (int)item2.Agent.DockState != 0 || item2.Phase == HarvestPhase.Depositing)
			{
				continue;
			}
			try
			{
				HarvestOwnOrder = true;
				AIHarvesterAgent agent = item2.Agent;
				ResourceDepositPoint nearestResourceDepositPoint = ((AIBaseAgent)agent).Team.GetNearestResourceDepositPoint(agent.ResourceHolder.ResourceType, ((Component)agent).transform.position, ((AIBaseAgent)agent).Unit);
				if (agent.ResourceHolder.AmountStored > 0 && (agent.ResourceHolder.RemainingSpace == 0 || !Object.op_Implicit((Object)(object)item2.Area) || item2.Area.ResourceAmountCurrent <= 0) && Object.op_Implicit((Object)(object)nearestResourceDepositPoint))
				{
					AIOrderProcessor orderAgent = ((BaseGameObject)((AIBaseAgent)agent).Unit).OrderAgent;
					DepositOrderDefinition deposit = OrderDefinitionRegistry.Deposit;
					OrderTarget val = new OrderTarget(((Component)nearestResourceDepositPoint).transform.position, (Target)null, (Component)(object)nearestResourceDepositPoint);
					OrderIssueParams val2 = OrderIssueParams.SelfDirected((AgentMoveSpeed)2, false, 1f);
					orderAgent.IssueOrder((OrderDefinition)(object)deposit, ref val, ref val2);
				}
				else if (Object.op_Implicit((Object)(object)item2.Area) && item2.Area.ResourceAmountCurrent > 0)
				{
					AIOrderProcessor orderAgent2 = ((BaseGameObject)((AIBaseAgent)agent).Unit).OrderAgent;
					HarvestOrderDefinition harvest = OrderDefinitionRegistry.Harvest;
					OrderTarget val = new OrderTarget(((Component)item2.Area).transform.position, (Target)null, (Component)(object)item2.Area);
					OrderIssueParams val2 = OrderIssueParams.SelfDirected((AgentMoveSpeed)2, false, 1f);
					orderAgent2.IssueOrder((OrderDefinition)(object)harvest, ref val, ref val2);
				}
			}
			catch (Exception ex)
			{
				DebugWarning("Harvester fix release: " + ex.Message);
			}
			finally
			{
				HarvestOwnOrder = false;
			}
		}
	}

	private static bool IssueHarvestOrder(HarvestJob j, OrderDefinition definition, OrderTarget target)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		bool harvestOwnOrder = HarvestOwnOrder;
		try
		{
			HarvestOwnOrder = true;
			AIOrderProcessor orderAgent = ((BaseGameObject)((AIBaseAgent)j.Agent).Unit).OrderAgent;
			OrderIssueParams val = OrderIssueParams.SelfDirected((AgentMoveSpeed)2, false, 1f);
			return orderAgent.IssueOrder(definition, ref target, ref val);
		}
		finally
		{
			HarvestOwnOrder = harvestOwnOrder;
		}
	}

	private static bool HarvestOrderPrefix(AIOrderProcessor __instance, in OrderIssueParams issueParams, ref bool __result)
	{
		if (HarvestOwnOrder || !HarvestFixActive || !issueParams.CalledByAI)
		{
			return true;
		}
		BaseGameObject owner = __instance.Owner;
		Unit val = (Unit)(object)((owner is Unit) ? owner : null);
		if (Object.op_Implicit((Object)(object)val) && HarvestJobs.TryGetValue(((Object)val).GetInstanceID(), out var value) && HarvestEligible(value.Agent) && value.Phase != HarvestPhase.Manual)
		{
			__result = false;
			return false;
		}
		return true;
	}

	private static void HarvestOrderPostfix(AIOrderProcessor __instance, OrderDefinition definition, in OrderTarget target, in OrderIssueParams issueParams, bool __result)
	{
		if (!__result || HarvestOwnOrder || !HarvestFixActive || !issueParams.BroadcastEvent || issueParams.QueueOrder)
		{
			return;
		}
		BaseGameObject owner = __instance.Owner;
		Unit val = (Unit)(object)((owner is Unit) ? owner : null);
		object obj;
		if (!Object.op_Implicit((Object)(object)val))
		{
			obj = null;
		}
		else
		{
			AIBaseAgent aIAgent = val.AIAgent;
			obj = ((aIAgent is AIHarvesterAgent) ? aIAgent : null);
		}
		AIHarvesterAgent a = (AIHarvesterAgent)obj;
		if (!HarvestEligible(a))
		{
			return;
		}
		HarvestJob harvestJob = GetHarvestJob(a);
		if ((Object)(object)definition == (Object)(object)OrderDefinitionRegistry.Deposit)
		{
			return;
		}
		if ((Object)(object)definition == (Object)(object)OrderDefinitionRegistry.Harvest)
		{
			object obj2;
			if (harvestJob.Phase != HarvestPhase.Harvesting)
			{
				Component payload = target.Payload;
				obj2 = ((payload is ResourceArea) ? payload : null);
			}
			else
			{
				obj2 = harvestJob.Area;
			}
			ResourceArea val2 = (ResourceArea)obj2;
			ReleaseRefineryReservation(harvestJob);
			if ((Object)(object)harvestJob.Area != (Object)(object)val2)
			{
				harvestJob.PatchReserved = false;
			}
			harvestJob.Area = val2;
			harvestJob.Phase = HarvestPhase.Starting;
			harvestJob.NextCheck = 0f;
			harvestJob.ReadyAt = Time.time;
			harvestJob.PatchReserved = PatchAvailable(harvestJob, val2);
		}
		else
		{
			ReleaseHarvestReservations(harvestJob);
			harvestJob.PickupUntil = 0f;
			harvestJob.ResumeAfterPickup = false;
			harvestJob.Phase = HarvestPhase.Manual;
			HarvestDiagnostic(harvestJob, "manual order: queue control released");
		}
	}

	private static void HarvestFindDepositPostfix(AIHarvesterAgent __instance, Vector3 worldPosition, ref ResourceDepositPoint __result)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)__result) && HarvestFixActive && HarvestEligible(__instance))
		{
			__result = ((AIBaseAgent)__instance).Team.GetResourceDepositPoint(__instance.ResourceHolder.ResourceType, worldPosition, (Unit)null, 2f, 5f);
		}
	}

	private static bool HarvestDepositPrefix(AIHarvesterAgent __instance, ResourceDepositPoint depositPoint)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (HarvestOwnOrder || !HarvestFixActive || !HarvestEligible(__instance))
		{
			return true;
		}
		if (!Object.op_Implicit((Object)(object)depositPoint) || !Object.op_Implicit((Object)(object)depositPoint.Structure) || (Object)(object)((BaseGameObject)depositPoint.Structure).Team != (Object)(object)((AIBaseAgent)__instance).Team)
		{
			return true;
		}
		HarvestJob harvestJob = GetHarvestJob(__instance);
		if (harvestJob.Line != null && (Object)(object)harvestJob.Line.Refinery == (Object)(object)depositPoint.Structure)
		{
			harvestJob.NextCheck = 0f;
			return false;
		}
		if ((int)__instance.DockState != 0)
		{
			if (harvestJob.Line == null)
			{
				harvestJob.Phase = HarvestPhase.Starting;
			}
			return false;
		}
		ReleaseRefineryReservation(harvestJob);
		__instance.CurrentDepositPoint = null;
		((AIBaseAgent)__instance).Stop(true);
		__instance.MiningArms.SetMining(false);
		harvestJob.PreferredRefinery = depositPoint.Structure;
		harvestJob.Phase = HarvestPhase.WaitingForRefinery;
		harvestJob.ReadyAt = Time.time;
		harvestJob.NextCheck = 0f;
		HarvestDiagnostic(harvestJob, "deposit requested: waiting for managed refinery admission");
		return false;
	}

	private static bool PatchAvailable(HarvestJob j, ResourceArea area)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)area) || (Object)(object)area.ResourceType != (Object)(object)j.Agent.ResourceHolder.ResourceType || area.ResourceAmountCurrent <= 0 || !FogOfWar.GetPositionVisible(j.Team, area.SignalCenter, false, false))
		{
			return false;
		}
		if (j.PatchReserved && (Object)(object)j.Area == (Object)(object)area)
		{
			return true;
		}
		int num = 0;
		foreach (HarvestJob value in HarvestJobs.Values)
		{
			if (value != j && (Object)(object)value.Team == (Object)(object)j.Team && value.PatchReserved && (Object)(object)value.Area == (Object)(object)area)
			{
				num++;
			}
		}
		return num < PatchReservationLimit(j, area);
	}

	private static int PatchReservationLimit(HarvestJob j, ResourceArea area)
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		HarvesterFixConfig harvesterFix = Cfg.HarvesterFix;
		if (!Object.op_Implicit((Object)(object)area))
		{
			return harvesterFix.FarPatchReservations;
		}
		float num = float.PositiveInfinity;
		if (Structure.Structures != null)
		{
			foreach (Structure structure in Structure.Structures)
			{
				if (!Object.op_Implicit((Object)(object)structure) || ((BaseGameObject)structure).IsDestroyed || (Object)(object)((BaseGameObject)structure).Team != (Object)(object)j.Team || structure.ResourceDepositPoints == null)
				{
					continue;
				}
				bool flag = false;
				foreach (ResourceDepositPoint resourceDepositPoint in structure.ResourceDepositPoints)
				{
					if (Object.op_Implicit((Object)(object)resourceDepositPoint) && (Object)(object)resourceDepositPoint.ResourceType == (Object)(object)area.ResourceType)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					Vector3 val = ((Component)structure).transform.position - area.SignalCenter;
					float num2 = Mathf.Sqrt(val.x * val.x + val.z * val.z);
					if (num2 < num)
					{
						num = num2;
					}
				}
			}
		}
		if (num <= harvesterFix.NearPatchDistanceMetres)
		{
			return harvesterFix.NearPatchReservations;
		}
		if (num < harvesterFix.MediumPatchDistanceMetres)
		{
			return harvesterFix.MediumPatchReservations;
		}
		return harvesterFix.FarPatchReservations;
	}

	private static ResourceArea ClosestHarvestPatch(HarvestJob j)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		ResourceArea result = null;
		float num = float.MaxValue;
		Vector3 val = default;
		foreach (ResourceArea allResourceArea in ResourceArea.AllResourceAreas)
		{
			if (!PatchAvailable(j, allResourceArea))
			{
				continue;
			}
			Vector3 nearestResourceCell = allResourceArea.GetNearestResourceCell(j.Agent.ExtractionPosition, ((Component)j.Agent).transform.forward);
			if (((AIBaseAgent)j.Agent).Unit.GetPointValidForAI(nearestResourceCell, ref val))
			{
				float num2 = Vector3.Distance(((Component)j.Agent).transform.position, nearestResourceCell);
				if (num2 < num)
				{
					result = allResourceArea;
					num = num2;
				}
			}
		}
		return result;
	}

	private static bool HarvestAreaPrefix(AIHarvesterAgent __instance, ref ResourceArea resourceArea, ref Vector3 worldPosition)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		if (HarvestOwnOrder || !HarvestFixActive || !HarvestEligible(__instance))
		{
			return true;
		}
		HarvestJob harvestJob = GetHarvestJob(__instance);
		if (harvestJob.Line != null)
		{
			return false;
		}
		if (Time.time < harvestJob.ReadyAt)
		{
			return false;
		}
		ReleaseRefineryReservation(harvestJob);
		if ((Object)(object)harvestJob.Area != (Object)(object)resourceArea)
		{
			harvestJob.PatchReserved = false;
		}
		if (!PatchAvailable(harvestJob, resourceArea))
		{
			resourceArea = ClosestHarvestPatch(harvestJob);
		}
		harvestJob.Area = resourceArea;
		harvestJob.Phase = HarvestPhase.Harvesting;
		harvestJob.NextCheck = 0f;
		if (!Object.op_Implicit((Object)(object)resourceArea))
		{
			((AIBaseAgent)__instance).Stop(true);
			return false;
		}
		harvestJob.PatchReserved = true;
		worldPosition = resourceArea.GetNearestResourceCell(__instance.ExtractionPosition, ((Component)__instance).transform.forward);
		List<AIOrder> orders = ((BaseGameObject)((AIBaseAgent)__instance).Unit).OrderAgent.Orders;
		if (orders.Count > 0)
		{
			AIOrder val = orders[0];
			AIHarvestOrder val2 = (AIHarvestOrder)(object)((val is AIHarvestOrder) ? val : null);
			if (val2 != null)
			{
				val2.ResourceArea = resourceArea;
				val2.WorldPosition = worldPosition;
			}
		}
		return true;
	}

	private static bool HarvestReturnPrefix(AIHarvesterAgent __instance)
	{
		if (HarvestOwnOrder || !HarvestFixActive || !HarvestEligible(__instance))
		{
			return true;
		}
		HarvestJob harvestJob = GetHarvestJob(__instance);
		if (harvestJob.Phase == HarvestPhase.Manual)
		{
			return true;
		}
		if (harvestJob.Line == null)
		{
			harvestJob.Phase = HarvestPhase.WaitingForRefinery;
		}
		return false;
	}

	private static void HarvesterTransportPickup(Unit unit)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (!HarvestFixActive)
		{
			return;
		}
		AIBaseAgent aIAgent = unit.AIAgent;
		AIHarvesterAgent val = (AIHarvesterAgent)(object)((aIAgent is AIHarvesterAgent) ? aIAgent : null);
		if (!HarvestEligible(val))
		{
			return;
		}
		HarvestJob harvestJob = GetHarvestJob(val);
		if ((int)val.DockState != 0 || val.Vehicle.HasForceMove)
		{
			return;
		}
		float num = ((BaseGameObject)unit).PhysicalRadius + val.MiningArms.ExtractionRadius;
		ResourceArea val2 = ((Object.op_Implicit((Object)(object)harvestJob.Area) && harvestJob.Area.GetPositionInBounds(val.ExtractionPosition, num)) ? harvestJob.Area : ResourceArea.GetResourceAreaAtPosition(((AIBaseAgent)val).Team, val.ResourceHolder.ResourceType, val.ExtractionPosition, true, num));
		if (!Object.op_Implicit((Object)(object)val2) || val2.ResourceAmountCurrent <= 0)
		{
			if (harvestJob.PickupUntil > 0f || harvestJob.ResumeAfterPickup)
			{
				harvestJob.PickupUntil = 0f;
				harvestJob.ResumeAfterPickup = false;
				harvestJob.NextCheck = 0f;
			}
			return;
		}
		bool flag = Time.time >= harvestJob.PickupUntil;
		bool flag2 = flag || harvestJob.Line != null || harvestJob.Phase != HarvestPhase.Harvesting || (Object)(object)val.CurrentHarvestArea != (Object)(object)val2;
		bool flag3 = harvestJob.PatchReserved && (Object)(object)harvestJob.Area == (Object)(object)val2;
		ReleaseRefineryReservation(harvestJob);
		if ((Object)(object)harvestJob.Area != (Object)(object)val2)
		{
			harvestJob.PatchReserved = false;
		}
		val.CurrentDepositPoint = null;
		harvestJob.Area = val2;
		harvestJob.PatchReserved = flag3 || PatchAvailable(harvestJob, val2);
		harvestJob.Phase = HarvestPhase.Harvesting;
		harvestJob.ReadyAt = Time.time;
		harvestJob.PreferredRefinery = null;
		harvestJob.ExitRequested = false;
		harvestJob.ResumeAfterPickup |= flag2;
		harvestJob.PickupUntil = Time.time + Cfg.HarvesterFix.TransferWaitingTimeSeconds;
		harvestJob.NextCheck = 0f;
		if (flag)
		{
			HarvestDiagnostic(harvestJob, "field pickup: resuming top-up harvesting; timer resets on each transfer");
		}
	}

	private static bool RefineryHasRoom(HarvestJob j, Structure refinery)
	{
		if (Object.op_Implicit((Object)(object)refinery) && !((BaseGameObject)refinery).IsDestroyed)
		{
			return (Object)(object)((BaseGameObject)refinery).Team == (Object)(object)j.Team;
		}
		return false;
	}

	private static void RepairBayReservation(ResourceDepositPoint p)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)p.OccupiedByUnit) && ((BaseGameObject)p.OccupiedByUnit).IsDestroyed)
		{
			p.OccupiedByUnit = null;
		}
		Unit occupiedByUnit = p.OccupiedByUnit;
		if (Object.op_Implicit((Object)(object)occupiedByUnit) && HarvestJobs.TryGetValue(((Object)occupiedByUnit).GetInstanceID(), out var value) && value.Phase != HarvestPhase.Manual && HarvestEligible(value.Agent) && (int)value.Agent.DockState == 0 && (Object)(object)value.Agent.CurrentDepositPoint != (Object)(object)p && Vector3.Distance(((Component)occupiedByUnit).transform.position, ((Component)p).transform.position) > Math.Max(100f, p.DepositRadius + ((BaseGameObject)occupiedByUnit).PhysicalRadius * 2f))
		{
			p.OccupiedByUnit = null;
			HarvestDiagnostic(value, "released stale occupancy after leaving bay " + HarvestObjectId((Object)(object)p));
		}
		Unit reservedByUnit = p.ReservedByUnit;
		if (Object.op_Implicit((Object)(object)reservedByUnit))
		{
			HarvestJob value2;
			if (((BaseGameObject)reservedByUnit).IsDestroyed)
			{
				p.ReservedByUnit = null;
			}
			else if (HarvestJobs.TryGetValue(((Object)reservedByUnit).GetInstanceID(), out value2) && value2.Phase != HarvestPhase.Manual && HarvestEligible(value2.Agent) && (int)value2.Agent.DockState == 0 && (value2.Line == null || !((Object)(object)value2.Line.Point == (Object)(object)p) || value2.Line.Jobs.Count <= 0 || value2.Line.Jobs[0] != value2 || value2.Phase != HarvestPhase.Depositing || (Object)(object)value2.Agent.CurrentDepositPoint != (Object)(object)p))
			{
				p.ReservedByUnit = null;
				HarvestDiagnostic(value2, "released obsolete native bay reservation " + HarvestObjectId((Object)(object)p));
			}
		}
	}

	private static bool ForeignBayBusy(RefineryLine line)
	{
		foreach (ResourceDepositPoint resourceDepositPoint in line.Refinery.ResourceDepositPoints)
		{
			if (!Object.op_Implicit((Object)(object)resourceDepositPoint))
			{
				continue;
			}
			RepairBayReservation(resourceDepositPoint);
			Unit[] array = new Unit[2] { resourceDepositPoint.OccupiedByUnit, resourceDepositPoint.ReservedByUnit };
			foreach (Unit val in array)
			{
				if (Object.op_Implicit((Object)(object)val) && !((BaseGameObject)val).IsDestroyed && (line.Jobs.Count <= 0 || !Object.op_Implicit((Object)(object)line.Jobs[0].Agent) || !((Object)(object)((AIBaseAgent)line.Jobs[0].Agent).Unit == (Object)(object)val)))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static RefineryLine GetRefineryLine(Structure refinery, ResourceDepositPoint point, Vector3 from)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (RefineryLines.TryGetValue(((Object)refinery).GetInstanceID(), out var value))
		{
			return value;
		}
		Vector3 closestApproachPoint = point.GetClosestApproachPoint(from);
		Vector3 val = closestApproachPoint - ((Component)point).transform.position;
		val.y = 0f;
		if (val.sqrMagnitude < 1f)
		{
			val = from - closestApproachPoint;
			val.y = 0f;
		}
		if (val.sqrMagnitude < 1f)
		{
			val = ((Component)refinery).transform.forward;
		}
		return new RefineryLine
		{
			Refinery = refinery,
			Point = point,
			Approach = closestApproachPoint,
			Direction = val.normalized
		};
	}

	private static RefineryLine ClosestRefinery(HarvestJob j, bool requireSlot)
	{
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		RefineryLine refineryLine = null;
		RefineryLine refineryLine2 = null;
		List<RefineryLine> list = new List<RefineryLine>();
		float num = float.MaxValue;
		foreach (Structure structure in Structure.Structures)
		{
			if (!RefineryHasRoom(j, structure) || structure.ResourceDepositPoints == null)
			{
				continue;
			}
			if (!RefineryLines.TryGetValue(((Object)structure).GetInstanceID(), out var value))
			{
				ResourceDepositPoint val = null;
				float num2 = float.MaxValue;
				foreach (ResourceDepositPoint resourceDepositPoint in structure.ResourceDepositPoints)
				{
					if (Object.op_Implicit((Object)(object)resourceDepositPoint) && !((Object)(object)resourceDepositPoint.ResourceType != (Object)(object)j.Agent.ResourceHolder.ResourceType))
					{
						float num3 = Vector3.Distance(((Component)j.Agent).transform.position, ((Component)resourceDepositPoint).transform.position);
						if (num3 < num2)
						{
							num2 = num3;
							val = resourceDepositPoint;
						}
					}
				}
				if (!Object.op_Implicit((Object)(object)val))
				{
					continue;
				}
				value = GetRefineryLine(structure, val, ((Component)j.Agent).transform.position);
			}
			list.Add(value);
			if (requireSlot && (Object)(object)structure == (Object)(object)j.PreferredRefinery)
			{
				refineryLine2 = value;
			}
			float num4 = Vector3.Distance(((Component)j.Agent).transform.position, ((Component)value.Point).transform.position);
			if (num4 < num)
			{
				refineryLine = value;
				num = num4;
			}
		}
		if (!requireSlot || refineryLine == null)
		{
			return refineryLine;
		}
		if (refineryLine2 != null)
		{
			refineryLine = refineryLine2;
		}
		int num5 = RefineryQueueCount(refineryLine);
		if (num5 >= 3)
		{
			RefineryLine refineryLine3 = null;
			float num6 = float.MaxValue;
			foreach (RefineryLine item in list)
			{
				if (item == refineryLine || Vector3.Distance(((Component)item.Refinery).transform.position, ((Component)refineryLine.Refinery).transform.position) > 800f)
				{
					continue;
				}
				int num7 = RefineryQueueCount(item);
				if (num7 < num5 && num7 < Cfg.HarvesterFix.MaximumReservationsPerRefinery)
				{
					float num8 = Vector3.Distance(((Component)((AIBaseAgent)j.Agent).Unit).transform.position, ((Component)item.Point).transform.position);
					if (num8 < num6)
					{
						refineryLine3 = item;
						num6 = num8;
					}
				}
			}
			if (refineryLine3 != null)
			{
				return refineryLine3;
			}
		}
		if (num5 >= Cfg.HarvesterFix.MaximumReservationsPerRefinery)
		{
			return null;
		}
		return refineryLine;
	}

	private static int RefineryQueueCount(RefineryLine line)
	{
		int count = line.Jobs.Count;
		foreach (ResourceDepositPoint resourceDepositPoint in line.Refinery.ResourceDepositPoints)
		{
			if (!Object.op_Implicit((Object)(object)resourceDepositPoint))
			{
				continue;
			}
			RepairBayReservation(resourceDepositPoint);
			Unit[] array = new Unit[2] { resourceDepositPoint.ReservedByUnit, resourceDepositPoint.OccupiedByUnit };
			foreach (Unit val in array)
			{
				if (!Object.op_Implicit((Object)(object)val) || ((BaseGameObject)val).IsDestroyed)
				{
					continue;
				}
				bool flag = false;
				foreach (HarvestJob job in line.Jobs)
				{
					if (Object.op_Implicit((Object)(object)job.Agent) && (Object)(object)((AIBaseAgent)job.Agent).Unit == (Object)(object)val)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return count + 1;
				}
			}
		}
		return count;
	}

	private static void HoldHarvester(HarvestJob j)
	{
		if (((AIBaseAgent)j.Agent).IsFollowingPath)
		{
			((AIBaseAgent)j.Agent).Stop(true);
		}
		j.Agent.MiningArms.SetMining(Object.op_Implicit((Object)(object)j.Area) && j.Area.ResourceAmountCurrent > 0 && j.Agent.ResourceHolder.RemainingSpace > 0 && j.Agent.MiningArms.IsResourceAvailable(true));
	}

	private static void SendToPatch(HarvestJob j, ResourceArea area)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (!PatchAvailable(j, area))
		{
			area = ClosestHarvestPatch(j);
		}
		ReleaseRefineryReservation(j);
		if ((Object)(object)j.Area != (Object)(object)area)
		{
			j.PatchReserved = false;
		}
		j.Area = area;
		j.Phase = HarvestPhase.Harvesting;
		if (!Object.op_Implicit((Object)(object)area))
		{
			HoldHarvester(j);
			return;
		}
		j.PatchReserved = true;
		if (!IssueHarvestOrder(j, (OrderDefinition)(object)OrderDefinitionRegistry.Harvest, new OrderTarget(((Component)area).transform.position, (Target)null, (Component)(object)area)))
		{
			j.PatchReserved = false;
			j.Area = null;
		}
	}

	private static void JoinRefinery(HarvestJob j)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)j.Agent.DockState == 0)
		{
			j.Agent.CurrentDepositPoint = null;
		}
		RefineryLine refineryLine = ClosestRefinery(j, requireSlot: true);
		if (refineryLine == null)
		{
			j.Phase = HarvestPhase.WaitingForRefinery;
			HoldHarvester(j);
			HarvestDiagnostic(j, (j.Team.RemainingResourceCapacity <= 0) ? "waiting: team resource storage is full" : "waiting: no eligible refinery reservation; retrying");
			return;
		}
		RefineryLines[((Object)refineryLine.Refinery).GetInstanceID()] = refineryLine;
		refineryLine.Jobs.Add(j);
		j.Line = refineryLine;
		j.Phase = HarvestPhase.Queued;
		j.LastSlot = -1;
		HarvestDiagnostic(j, "joined refinery=" + HarvestObjectId((Object)(object)refineryLine.Refinery) + " queue index=" + refineryLine.Jobs.Count);
		AdvanceRefinery(j);
	}

	private static void AdvanceRefinery(HarvestJob j)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		RefineryLine line = j.Line;
		if (line == null)
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)line.Point) || !RefineryHasRoom(j, line.Refinery))
		{
			ReleaseRefineryReservation(j);
			j.Agent.CurrentDepositPoint = null;
			j.Phase = HarvestPhase.WaitingForRefinery;
			JoinRefinery(j);
			return;
		}
		int num = line.Jobs.IndexOf(j) + (ForeignBayBusy(line) ? 1 : 0);
		if (num == 0)
		{
			if (j.Phase != HarvestPhase.Depositing || (Object)(object)j.Agent.CurrentDepositPoint != (Object)(object)line.Point)
			{
				j.Agent.MiningArms.SetMining(false);
				if (IssueHarvestOrder(j, (OrderDefinition)(object)OrderDefinitionRegistry.Deposit, new OrderTarget(((Component)line.Point).transform.position, (Target)null, (Component)(object)line.Point)))
				{
					j.Phase = HarvestPhase.Depositing;
					j.LastSlot = 0;
					HarvestDiagnostic(j, "admitted to refinery=" + HarvestObjectId((Object)(object)line.Refinery) + " slot=1");
				}
				else
				{
					HarvestDiagnostic(j, "deposit order rejected for refinery=" + HarvestObjectId((Object)(object)line.Refinery));
				}
			}
			if (!Object.op_Implicit((Object)(object)line.Point.ReservedByUnit) && !Object.op_Implicit((Object)(object)line.Point.OccupiedByUnit) && j.Phase == HarvestPhase.Depositing)
			{
				j.Agent.CurrentDepositPoint = line.Point;
			}
			return;
		}
		if (j.Phase == HarvestPhase.Depositing)
		{
			j.Phase = HarvestPhase.Queued;
			j.Agent.CurrentDepositPoint = null;
			j.LastSlot = -1;
		}
		if (j.LastSlot == num && (((AIBaseAgent)j.Agent).IsFollowingPath || ((AIBaseAgent)j.Agent).IsCalculatingPath))
		{
			return;
		}
		float num2 = 100f + (float)(num - 1) * Cfg.HarvesterFix.ReservationDistanceIncrementMetres;
		Vector3 val = line.Approach + line.Direction * num2;
		val = GamePhysics.GetTerrainPosition(val, 0f, (Terrain)null);
		if (!TryQueuePoint(j, val, out var point))
		{
			HoldHarvester(j);
			HarvestDiagnostic(j, "waiting: no navigable position for slot=" + (num + 1));
		}
		else if (j.LastSlot != num || Vector3.Distance(((Component)j.Agent).transform.position, point) > Math.Max(15f, ((BaseGameObject)((AIBaseAgent)j.Agent).Unit).PhysicalRadius))
		{
			j.Agent.MiningArms.SetMining(false);
			if (IssueHarvestOrder(j, (OrderDefinition)(object)OrderDefinitionRegistry.Move, new OrderTarget(point, (Target)null, (Component)null)))
			{
				j.LastSlot = num;
				string[] array = new string[6]
				{
					"moving to refinery=",
					HarvestObjectId((Object)(object)line.Refinery).ToString(),
					" slot=",
					(num + 1).ToString(),
					" position=",
					null
				};
				Vector3 val2 = point;
				array[5] = ((object)val2/*cast due to constrained. prefix*/).ToString();
				HarvestDiagnostic(j, string.Concat(array));
			}
			else
			{
				HarvestDiagnostic(j, "queue move rejected for slot=" + (num + 1));
			}
		}
	}

	private static bool TryQueuePoint(HarvestJob j, Vector3 desired, out Vector3 point)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (((AIBaseAgent)j.Agent).Unit.GetPointValidForAI(desired, ref point))
		{
			return true;
		}
		Vector3 val = Vector3.Cross(Vector3.up, j.Line.Direction);
		Vector3 normalized = val.normalized;
		for (int i = 10; i <= 40; i += 10)
		{
			int[] array = new int[2] { -1, 1 };
			foreach (int num in array)
			{
				Vector3 terrainPosition = GamePhysics.GetTerrainPosition(desired + normalized * (float)(i * num), 0f, (Terrain)null);
				if (((AIBaseAgent)j.Agent).Unit.GetPointValidForAI(terrainPosition, ref point))
				{
					return true;
				}
			}
		}
		point = Vector3.zero;
		return false;
	}

	private static bool HarvestUpdatePrefix(AIHarvesterAgent __instance)
	{
		if (!HarvestFixActive || !HarvestEligible(__instance))
		{
			return true;
		}
		HarvestJob harvestJob = GetHarvestJob(__instance);
		if (harvestJob.Phase == HarvestPhase.Manual)
		{
			return true;
		}
		try
		{
			return ManagedHarvestUpdate(harvestJob);
		}
		catch (Exception ex)
		{
			ReleaseHarvestReservations(harvestJob);
			harvestJob.Phase = HarvestPhase.Manual;
			DebugError("Harvester fix released vehicle to vanilla after error: " + ex);
			return true;
		}
	}

	private static bool ManagedHarvestUpdate(HarvestJob j)
	{
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0601: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		AIHarvesterAgent agent = j.Agent;
		float time = Time.time;
		((AIBaseAgent)agent).IsIdle = false;
		if (j.Line == null && !j.ExitRequested && Object.op_Implicit((Object)(object)agent.CurrentDepositPoint) && Object.op_Implicit((Object)(object)agent.CurrentDepositPoint.Structure) && (Object)(object)((BaseGameObject)agent.CurrentDepositPoint.Structure).Team == (Object)(object)j.Team)
		{
			ResourceDepositPoint currentDepositPoint = agent.CurrentDepositPoint;
			if ((int)agent.DockState != 0)
			{
				RefineryLine refineryLine = GetRefineryLine(currentDepositPoint.Structure, currentDepositPoint, ((Component)((AIBaseAgent)agent).Unit).transform.position);
				if (refineryLine.Jobs.Count >= Cfg.HarvesterFix.MaximumReservationsPerRefinery)
				{
					j.Phase = HarvestPhase.WaitingForRefinery;
					j.ExitRequested = IssueHarvestOrder(j, (OrderDefinition)(object)OrderDefinitionRegistry.Move, new OrderTarget(currentDepositPoint.GetClosestApproachPoint(((Component)((AIBaseAgent)agent).Unit).transform.position), (Target)null, (Component)null));
					HarvestDiagnostic(j, "native bay entrant exiting: queue at hard reservation limit");
					return true;
				}
				RefineryLines[((Object)refineryLine.Refinery).GetInstanceID()] = refineryLine;
				refineryLine.Jobs.Add(j);
				j.Line = refineryLine;
				j.Phase = ((refineryLine.Jobs.Count == 1) ? HarvestPhase.Depositing : HarvestPhase.Queued);
				j.LastSlot = -1;
				HarvestDiagnostic(j, "adopted native docking at BACK of refinery=" + HarvestObjectId((Object)(object)refineryLine.Refinery) + " queue index=" + refineryLine.Jobs.Count);
			}
			else
			{
				j.PreferredRefinery = currentDepositPoint.Structure;
				agent.CurrentDepositPoint = null;
				j.Phase = ((agent.ResourceHolder.AmountStored > 0) ? HarvestPhase.WaitingForRefinery : HarvestPhase.Starting);
				j.NextCheck = 0f;
			}
		}
		if (!Object.op_Implicit((Object)(object)agent.CurrentDepositPoint) && !agent.Vehicle.HasForceMove && (int)agent.DockState != 0)
		{
			agent.DockState = (EHarvesterAIDockState)0;
			ReleaseRefineryReservation(j);
			j.Phase = HarvestPhase.Starting;
			j.NextCheck = 0f;
			HarvestDiagnostic(j, "recovered missing native dock target");
		}
		if ((int)agent.DockState != 0)
		{
			if (j.Line != null && j.Line.Jobs.IndexOf(j) > 0 && !j.ExitRequested)
			{
				j.ExitRequested = IssueHarvestOrder(j, (OrderDefinition)(object)OrderDefinitionRegistry.Move, new OrderTarget(j.Line.Approach, (Target)null, (Component)null));
				j.Phase = HarvestPhase.Queued;
			}
			if (j.Line != null && !j.ExitRequested && time >= j.NextCheck && !RefineryHasRoom(j, j.Line.Refinery))
			{
				j.NextCheck = time + Cfg.HarvesterFix.ReservationCheckTimeSeconds;
				j.ExitRequested = IssueHarvestOrder(j, (OrderDefinition)(object)OrderDefinitionRegistry.Move, new OrderTarget(j.Line.Approach, (Target)null, (Component)null));
			}
			return true;
		}
		if (agent.Vehicle.HasForceMove)
		{
			return false;
		}
		if (time < j.ReadyAt)
		{
			return false;
		}
		if (j.Line == null && j.ExitRequested && !Object.op_Implicit((Object)(object)agent.CurrentDepositPoint))
		{
			j.ExitRequested = false;
			j.NextCheck = 0f;
		}
		if (j.Phase == HarvestPhase.Queued && j.ExitRequested && !Object.op_Implicit((Object)(object)agent.CurrentDepositPoint))
		{
			j.ExitRequested = false;
			j.NextCheck = 0f;
		}
		if (j.Phase == HarvestPhase.Depositing && (agent.ResourceHolder.AmountStored == 0 || j.ExitRequested) && !Object.op_Implicit((Object)(object)agent.CurrentDepositPoint))
		{
			ReleaseRefineryReservation(j);
			j.ExitRequested = false;
			j.PreferredRefinery = null;
			j.Phase = HarvestPhase.Starting;
			j.NextCheck = 0f;
			HarvestDiagnostic(j, "cleared refinery; released queue head");
		}
		if (j.PickupUntil > 0f && time >= j.PickupUntil)
		{
			j.PickupUntil = 0f;
			j.ResumeAfterPickup = false;
			j.NextCheck = 0f;
			HarvestDiagnostic(j, "pickup window expired: normal harvesting/refinery routing resumed");
		}
		bool flag = !Object.op_Implicit((Object)(object)j.Area) || j.Area.ResourceAmountCurrent <= 0;
		if (flag && (j.PickupUntil > 0f || j.ResumeAfterPickup))
		{
			j.PickupUntil = 0f;
			j.ResumeAfterPickup = false;
			j.NextCheck = 0f;
			HarvestDiagnostic(j, "patch depleted: pickup hold cleared; normal routing resumed");
		}
		bool flag2 = agent.ResourceHolder.RemainingSpace == 0;
		bool flag3 = time < j.PickupUntil;
		if (flag3 || j.ResumeAfterPickup)
		{
			if (flag2 | flag)
			{
				HoldHarvester(j);
				return false;
			}
			if (j.ResumeAfterPickup || (Object)(object)agent.CurrentHarvestArea != (Object)(object)j.Area)
			{
				if (time < j.NextCheck)
				{
					return false;
				}
				j.NextCheck = time + Cfg.HarvesterFix.ReservationCheckTimeSeconds;
				if (!IssueHarvestOrder(j, (OrderDefinition)(object)OrderDefinitionRegistry.Harvest, new OrderTarget(((Component)j.Area).transform.position, (Target)null, (Component)(object)j.Area)))
				{
					HarvestDiagnostic(j, "top-up harvest order rejected; retrying");
					return false;
				}
				j.ResumeAfterPickup = false;
			}
			j.Phase = HarvestPhase.Harvesting;
			return true;
		}
		if (time >= j.NextCheck)
		{
			j.NextCheck = time + Cfg.HarvesterFix.ReservationCheckTimeSeconds;
			if (j.Line != null)
			{
				AdvanceRefinery(j);
			}
			else if (flag3 && (flag2 | flag))
			{
				HoldHarvester(j);
			}
			else if (j.Phase == HarvestPhase.WaitingForRefinery && !flag3)
			{
				JoinRefinery(j);
			}
			else if (flag2 && !flag3)
			{
				JoinRefinery(j);
			}
			else if (flag && !flag3)
			{
				ResourceArea val = ClosestHarvestPatch(j);
				RefineryLine refineryLine2 = ((agent.ResourceHolder.AmountStored > 0) ? ClosestRefinery(j, requireSlot: false) : null);
				float num = (Object.op_Implicit((Object)(object)val) ? Vector3.Distance(((Component)agent).transform.position, val.GetNearestResourceCell(agent.ExtractionPosition, ((Component)agent).transform.forward)) : float.MaxValue);
				if (agent.ResourceHolder.AmountStored > 0 && ((Object)(object)val == (Object)null || (refineryLine2 != null && Vector3.Distance(((Component)agent).transform.position, ((Component)refineryLine2.Point).transform.position) < num)))
				{
					JoinRefinery(j);
				}
				else
				{
					SendToPatch(j, val);
				}
			}
			else if (j.Phase == HarvestPhase.Starting || (Object.op_Implicit((Object)(object)j.Area) && (Object)(object)agent.CurrentHarvestArea != (Object)(object)j.Area))
			{
				SendToPatch(j, PatchAvailable(j, j.Area) ? j.Area : ClosestHarvestPatch(j));
			}
		}
		if (j.Phase == HarvestPhase.Depositing)
		{
			return true;
		}
		if (j.Phase == HarvestPhase.Queued)
		{
			return false;
		}
		if (j.Phase == HarvestPhase.WaitingForRefinery || agent.ResourceHolder.RemainingSpace == 0 || !Object.op_Implicit((Object)(object)j.Area) || j.Area.ResourceAmountCurrent <= 0)
		{
			HoldHarvester(j);
			return false;
		}
		return (Object)(object)j.Area != (Object)null;
	}

	private static float AILoadRadius(Unit u)
	{
		if (!Object.op_Implicit((Object)(object)u) || !u.IsFlyingType)
		{
			return Cfg.AITransport.GroundLoadRadius;
		}
		return Cfg.AITransport.AirLoadRadius;
	}

	private static float AIUnloadRadius(Unit u)
	{
		if (!Object.op_Implicit((Object)(object)u) || !u.IsFlyingType)
		{
			return Cfg.AITransport.GroundUnloadRadius;
		}
		return Cfg.AITransport.AirUnloadRadius;
	}

	private void InstallAITransport()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected Obj, but got Unknown
		//IL_004e: Expected Obj, but got Unknown
		((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(AIOrderProcessor), "IssueOrder", (Type[])null, (Type[])null), new HarmonyMethod(typeof(LogisticsMod), "AITransportOrderPrefix", (Type[])null), new HarmonyMethod(typeof(LogisticsMod), "AITransportOrderPostfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		AITransportInstalled = true;
	}

	private static bool AITransportEligible(Unit u)
	{
		if (IsTransport(u) && Object.op_Implicit((Object)(object)u.AIAgent) && u.AIAgent.AIControlled && Object.op_Implicit((Object)(object)((BaseGameObject)u).OrderAgent) && !u.PlayerControlled && (Object)(object)DriverOf(u) == (Object)null)
		{
			return !Object.op_Implicit((Object)(object)u.InCompartment);
		}
		return false;
	}

	private static bool AIHarvesterTarget(Unit h, Team team)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		if (Object.op_Implicit((Object)(object)h) && !((BaseGameObject)h).IsDestroyed && (Object)(object)((BaseGameObject)h).Team == (Object)(object)team && Object.op_Implicit((Object)(object)((BaseGameObject)h).ObjectInfo) && (int)((BaseGameObject)h).ObjectInfo.UnitType == 7)
		{
			return ((BaseGameObject)h).IsResourceHolder;
		}
		return false;
	}

	private static bool AIRefineryTarget(Structure s, Team team)
	{
		if (Object.op_Implicit((Object)(object)s) && !((BaseGameObject)s).IsDestroyed && (Object)(object)((BaseGameObject)s).Team == (Object)(object)team && s.ResourceDepositPoints != null)
		{
			return s.ResourceDepositPoints.Count > 0;
		}
		return false;
	}

	private static bool AITransportOrderPrefix(AIOrderProcessor __instance, in OrderIssueParams issueParams, ref bool __result)
	{
		if (AITransportOwnOrder > 0 || !AITransportActive)
		{
			return true;
		}
		BaseGameObject owner = __instance.Owner;
		Unit val = (Unit)(object)((owner is Unit) ? owner : null);
		if (Object.op_Implicit((Object)(object)val) && AITransportJobs.ContainsKey(((Object)val).GetInstanceID()) && AITransportEligible(val) && (issueParams.CalledByAI || !issueParams.BroadcastEvent))
		{
			__result = false;
			return false;
		}
		return true;
	}

	private static void AITransportOrderPostfix(AIOrderProcessor __instance, OrderDefinition definition, in OrderTarget target, in OrderIssueParams issueParams, bool __result)
	{
		if (!__result || AITransportOwnOrder > 0 || !Game.GetIsServer() || issueParams.CalledByAI || !issueParams.BroadcastEvent)
		{
			return;
		}
		BaseGameObject owner = __instance.Owner;
		Unit val = (Unit)(object)((owner is Unit) ? owner : null);
		if (!Object.op_Implicit((Object)(object)val))
		{
			return;
		}
		bool flag = AITransportJobs.TryGetValue(((Object)val).GetInstanceID(), out var value);
		if (flag)
		{
			CancelAITransport(value, "new external order " + ((Object)definition).name, stop: false);
		}
		if (!AITransportActive || !AITransportEligible(val))
		{
			return;
		}
		Unit val2 = (Object.op_Implicit((Object)(object)target.Object) ? target.Object.OwnerUnit : null);
		if ((Cfg.Debug | flag) || AIHarvesterTarget(val2, ((BaseGameObject)val).Team))
		{
			DebugMsg("[AITransport] order transport=" + ((Object)val).GetInstanceID() + " definition=" + ((Object)definition).name + " target=" + HarvestObjectId((Object)(object)val2) + " queued=" + issueParams.QueueOrder);
		}
		if (!issueParams.QueueOrder && !((Object)(object)definition != (Object)(object)OrderDefinitionRegistry.Defend) && AIHarvesterTarget(val2, ((BaseGameObject)val).Team))
		{
			AITransportJob aITransportJob = new AITransportJob
			{
				Transport = val,
				Team = ((BaseGameObject)val).Team,
				LastProgress = Time.time
			};
			AITransportJobs.Add(((Object)val).GetInstanceID(), aITransportJob);
			if (CanAssignAIHarvester(aITransportJob, val2))
			{
				AssignAIHarvester(aITransportJob, val2);
			}
			else
			{
				AILog(aITransportJob, "enrolled in logistics duty; selecting available harvester");
			}
		}
	}

	private static void ShowAITransportStatus(Player caller)
	{
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		Reply(caller, "AI transport " + (Cfg.AITransport.Enabled ? "ON" : "OFF") + " | assigned " + AITransportJobs.Count + " | right-click a friendly harvester to enrol in automatic logistics duty; a new order cancels. Details in server log.");
		if (!Cfg.Enabled && Cfg.AITransport.Enabled)
		{
			Reply(caller, "AI transport paused by Logistics master switch.");
		}
		if (!Cfg.Debug)
		{
			return;
		}
		DebugMsg("[AITransport] status enabled=" + Cfg.AITransport.Enabled + " active=" + AITransportActive + " duty=" + AITransportJobs.Count + " bands=" + Cfg.AITransport.NearHarvesterDistanceMetres + "/" + Cfg.AITransport.MediumHarvesterDistanceMetres + " groundLimits=" + Cfg.AITransport.NearGroundTransports + "/" + Cfg.AITransport.MediumGroundTransports + "/" + Cfg.AITransport.FarGroundTransports + " airLimits=" + Cfg.AITransport.NearAirTransports + "/" + Cfg.AITransport.MediumAirTransports + "/" + Cfg.AITransport.FarAirTransports);
		foreach (AITransportJob value2 in AITransportJobs.Values)
		{
			int num = ((Object.op_Implicit((Object)(object)value2.Transport) && CargoByTransport.TryGetValue(((Object)value2.Transport).GetInstanceID(), out var value)) ? value.Amount : 0);
			string[] array = new string[24]
			{
				"[AITransport] transport=",
				HarvestObjectId((Object)(object)value2.Transport).ToString(),
				" harvester=",
				HarvestObjectId((Object)(object)value2.Harvester).ToString(),
				" patch=",
				HarvestObjectId((Object)(object)value2.AssignedArea).ToString(),
				" refinery=",
				HarvestObjectId((Object)(object)value2.Refinery).ToString(),
				" delivering=",
				value2.Delivering.ToString(),
				" cargo=",
				num.ToString(),
				" loadRadius=",
				AILoadRadius(value2.Transport).ToString(),
				" unloadRadius=",
				AIUnloadRadius(value2.Transport).ToString(),
				" holding=",
				value2.Holding.ToString(),
				" position=",
				Object.op_Implicit((Object)(object)value2.Transport) ? ((object)((Component)value2.Transport).transform.position/*cast due to constrained. prefix*/).ToString() : "destroyed",
				" destination=",
				null,
				null,
				null
			};
			Vector3 destination = value2.Destination;
			array[21] = ((object)destination/*cast due to constrained. prefix*/).ToString();
			array[22] = " status=";
			array[23] = value2.Status;
			DebugMsg(string.Concat(array));
		}
	}

	private static void AILog(AITransportJob j, string status)
	{
		if (!(j.Status == status))
		{
			j.Status = status;
			DebugMsg("[AITransport] transport=" + HarvestObjectId((Object)(object)j.Transport) + " harvester=" + HarvestObjectId((Object)(object)j.Harvester) + " " + status);
		}
	}

	private static bool AIOrder(AITransportJob j, OrderDefinition definition, Vector3 position)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		AITransportOwnOrder++;
		try
		{
			AIOrderProcessor orderAgent = ((BaseGameObject)j.Transport).OrderAgent;
			OrderTarget val = new OrderTarget(position, (Target)null, (Component)null);
			OrderIssueParams val2 = OrderIssueParams.SelfDirected((AgentMoveSpeed)2, false, 1f);
			return orderAgent.IssueOrder(definition, ref val, ref val2);
		}
		finally
		{
			AITransportOwnOrder--;
		}
	}

	private static void AIHold(AITransportJob j)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!j.Holding)
		{
			j.Holding = AIOrder(j, (OrderDefinition)(object)OrderDefinitionRegistry.Stop, ((Component)j.Transport).transform.position);
		}
	}

	private static void CancelAITransport(AITransportJob j, string reason, bool stop)
	{
		if (Object.op_Implicit((Object)(object)j.Transport))
		{
			AITransportJobs.Remove(((Object)j.Transport).GetInstanceID());
			if (CargoByTransport.TryGetValue(((Object)j.Transport).GetInstanceID(), out var value))
			{
				SetTransfer(j.Transport, value, null);
				if ((Object)(object)((BaseGameObject)j.Transport).Team != (Object)(object)j.Team)
				{
					CargoByTransport.Remove(((Object)j.Transport).GetInstanceID());
				}
			}
			if (stop && AITransportEligible(j.Transport))
			{
				AIHold(j);
			}
		}
		else
		{
			foreach (KeyValuePair<int, AITransportJob> item in new List<KeyValuePair<int, AITransportJob>>(AITransportJobs))
			{
				if (item.Value == j)
				{
					AITransportJobs.Remove(item.Key);
				}
			}
		}
		AILog(j, "released: " + reason);
	}

	private static void ResetAITransports(bool stop)
	{
		foreach (AITransportJob item in new List<AITransportJob>(AITransportJobs.Values))
		{
			CancelAITransport(item, "disabled or match reset", stop);
		}
		AITransportJobs.Clear();
	}

	private static bool TickAITransport(AITransportJob j)
	{
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		Unit transport = j.Transport;
		if (!AITransportActive || !AITransportEligible(transport) || (Object)(object)((BaseGameObject)transport).Team != (Object)(object)j.Team)
		{
			CancelAITransport(j, "disabled, team changed or player takeover", Object.op_Implicit((Object)(object)transport) && !transport.PlayerControlled && (Object)(object)DriverOf(transport) == (Object)null);
			return false;
		}
		if (!CargoByTransport.TryGetValue(((Object)transport).GetInstanceID(), out var value))
		{
			value = new Cargo();
			CargoByTransport.Add(((Object)transport).GetInstanceID(), value);
		}
		if (!j.Delivering && value.Amount >= CapacityFor(transport))
		{
			BeginAIDelivery(j, "full; heading to refinery");
		}
		if (j.Delivering && value.Amount == 0)
		{
			j.Delivering = false;
			j.Refinery = null;
			j.NextAllocation = 0f;
			ReleaseAIHarvester(j);
			AILog(j, "empty; selecting next collection assignment");
		}
		if (!j.Delivering)
		{
			AllocateAITransport(j, value);
		}
		if (j.Delivering)
		{
			if (!AIRefineryTarget(j.Refinery, j.Team))
			{
				j.Refinery = null;
				if (Time.time >= j.NextRefinerySearch)
				{
					j.NextRefinerySearch = Time.time + 5f;
					j.Refinery = ClosestAIRefinery(j);
					j.Holding = false;
					j.NextMove = 0f;
					if (Object.op_Implicit((Object)(object)j.Refinery))
					{
						AILog(j, "delivery refinery=" + ((Object)j.Refinery).GetInstanceID());
					}
				}
			}
			if (!Object.op_Implicit((Object)(object)j.Refinery))
			{
				AIHold(j);
				SetTransfer(transport, value, null);
				AILog(j, "waiting for a reachable friendly refinery");
				return true;
			}
			if (Vector3.Distance(((Component)transport).transform.position, ((Component)j.Refinery).transform.position) <= AIUnloadRadius(transport))
			{
				AIHold(j);
				if (Time.unscaledTime >= value.CooldownUntil && TryUnload(transport, value, j.Refinery, AIUnloadRadius(transport)))
				{
					AILog(j, (value.Amount == 0) ? "delivery complete" : "unloading");
				}
				else
				{
					SetTransfer(transport, value, null);
					AILog(j, "waiting for team storage or transfer cooldown");
				}
			}
			else
			{
				SetTransfer(transport, value, null);
				AIDrive(j, ((Component)j.Refinery).transform.position, AIUnloadRadius(transport), j.Refinery);
			}
		}
		else if (Object.op_Implicit((Object)(object)j.Harvester))
		{
			if (Vector3.Distance(((Component)transport).transform.position, ((Component)j.Harvester).transform.position) <= AILoadRadius(transport))
			{
				AIHold(j);
				if (Time.unscaledTime >= value.CooldownUntil)
				{
					int amount = value.Amount;
					TryLoad(transport, value, j.Harvester, AILoadRadius(transport));
					if (value.Amount > amount)
					{
						j.LastProgress = Time.time;
					}
					AILog(j, (value.Amount > amount) ? "loading from assigned harvester" : "waiting for assigned harvester to mine");
				}
				else
				{
					j.LastProgress = Time.time;
					SetTransfer(transport, value, null);
					AILog(j, "waiting for transfer cooldown");
				}
			}
			else
			{
				SetTransfer(transport, value, null);
				AIDrive(j, ((Component)j.Harvester).transform.position, AILoadRadius(transport));
			}
		}
		else
		{
			SetTransfer(transport, value, null);
			ParkAITransport(j);
		}
		return true;
	}

	private static Structure ClosestAIRefinery(AITransportJob j)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Structure result = null;
		float num = float.MaxValue;
		foreach (Structure allocationRefinery in AllocationRefineries)
		{
			if (AIRefineryTarget(allocationRefinery, j.Team))
			{
				Vector3 val = ((Component)allocationRefinery).transform.position - ((Component)j.Transport).transform.position;
				float sqrMagnitude = val.sqrMagnitude;
				if (!(sqrMagnitude >= num) && AIServicePosition(j, ((Component)allocationRefinery).transform.position, AIUnloadRadius(j.Transport), out var _, allocationRefinery))
				{
					result = allocationRefinery;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	private static void AIDrive(AITransportJob j, Vector3 center, float radius, Structure refinery = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = center - j.TargetPosition;
		bool flag = val.sqrMagnitude > Mathf.Pow(Math.Max(5f, radius * 0.25f), 2f);
		if (!flag && Time.time < j.NextMove)
		{
			return;
		}
		j.NextMove = Time.time + 5f;
		if (!j.Holding && !flag && j.Transport.AIAgent.IsFollowingPath && j.Attempts % 4 != 3)
		{
			j.Attempts++;
			return;
		}
		if (!AIServicePosition(j, center, radius, out var result, refinery))
		{
			j.TargetPosition = center;
			AIHold(j);
			AILog(j, "no navigable transfer position; retrying");
			return;
		}
		j.Attempts++;
		if (AIOrder(j, (OrderDefinition)(object)OrderDefinitionRegistry.Move, result))
		{
			j.Destination = result;
			j.TargetPosition = center;
			j.Holding = false;
			AILog(j, j.Delivering ? "driving to refinery transfer range" : "driving to assigned harvester transfer range");
		}
		else
		{
			AILog(j, "movement order rejected; retrying");
		}
	}

	private static bool AIServicePosition(AITransportJob j, Vector3 center, float radius, out Vector3 result, Structure refinery = null)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)j.Transport).transform.position;
		if (PerformanceTickActive && j.ServiceFrame == Time.frameCount && j.ServiceCenter.Equals(center) && j.ServiceFrom.Equals(position) && j.ServiceRadius == radius && (Object)(object)j.ServiceRefinery == (Object)(object)refinery)
		{
			if (Cfg.Debug)
			{
				PerfServiceHits++;
			}
			result = j.ServicePoint;
			return j.ServiceFound;
		}
		if (Cfg.Debug)
		{
			PerfSearches++;
		}
		bool flag = FindAIServicePosition(j, center, radius, out result, refinery);
		j.ServiceFrame = (PerformanceTickActive ? Time.frameCount : (-1));
		j.ServiceCenter = center;
		j.ServiceFrom = position;
		j.ServiceRadius = radius;
		j.ServiceRefinery = refinery;
		j.ServicePoint = result;
		j.ServiceFound = flag;
		return flag;
	}

	private static bool FindAIServicePosition(AITransportJob j, Vector3 center, float radius, out Vector3 result, Structure refinery)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		result = Vector3.zero;
		if (float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0f)
		{
			return false;
		}
		Unit transport = j.Transport;
		Vector3 val = ((Component)transport).transform.position - center;
		val.y = 0f;
		float num = Mathf.Atan2(val.z, val.x);
		float best = float.MaxValue;
		bool found = false;
		float[] serviceRings = ServiceRings;
		Vector3 point = default;
		foreach (float num2 in serviceRings)
		{
			for (int k = 0; k < 32; k++)
			{
				float num3 = num + (float)k * MathF.PI / 16f;
				Vector3 val2 = center + new Vector3(Mathf.Cos(num3) * radius * num2, 0f, Mathf.Sin(num3) * radius * num2);
				if (Object.op_Implicit((Object)(object)Game.MainTerrain))
				{
					val2.y = Game.MainTerrain.SampleHeight(val2) + ((Component)Game.MainTerrain).transform.position.y;
				}
				if (Cfg.Debug)
				{
					PerfNavProbes++;
				}
				if (transport.GetPointValidForAI(val2, ref point))
				{
					ConsiderAIServicePoint(j, center, radius * 0.99f, point, ref best, ref result, ref found);
				}
			}
		}
		if (Object.op_Implicit((Object)(object)refinery))
		{
			foreach (ResourceDepositPoint resourceDepositPoint in refinery.ResourceDepositPoints)
			{
				if (!Object.op_Implicit((Object)(object)resourceDepositPoint))
				{
					continue;
				}
				Vector3 closestApproachPoint = resourceDepositPoint.GetClosestApproachPoint(((Component)transport).transform.position);
				ConsiderAIServicePoint(j, center, radius * 0.99f, closestApproachPoint, ref best, ref result, ref found);
				foreach (Transform aIEntryPoint in resourceDepositPoint.AIEntryPoints)
				{
					if (Object.op_Implicit((Object)(object)aIEntryPoint))
					{
						ConsiderAIServicePoint(j, center, radius * 0.99f, aIEntryPoint.position, ref best, ref result, ref found);
					}
				}
			}
			if (!found)
			{
				foreach (ResourceDepositPoint resourceDepositPoint2 in refinery.ResourceDepositPoints)
				{
					if (Object.op_Implicit((Object)(object)resourceDepositPoint2))
					{
						ConsiderAIServicePoint(j, center, radius * 0.99f, ((Component)resourceDepositPoint2).transform.position, ref best, ref result, ref found);
					}
				}
			}
			if (!found && j.Status != "waiting for a reachable friendly refinery")
			{
				string text = "";
				foreach (ResourceDepositPoint resourceDepositPoint3 in refinery.ResourceDepositPoints)
				{
					if (Object.op_Implicit((Object)(object)resourceDepositPoint3))
					{
						text = text + " deposit=" + ((object)((Component)resourceDepositPoint3).transform.position/*cast due to constrained. prefix*/).ToString() + " approach=" + ((object)resourceDepositPoint3.GetClosestApproachPoint(((Component)transport).transform.position)/*cast due to constrained. prefix*/).ToString();
					}
				}
				string[] array = new string[9]
				{
					"[AITransport] refinery approach rejected transport=",
					((Object)transport).GetInstanceID().ToString(),
					" refinery=",
					((Object)refinery).GetInstanceID().ToString(),
					" center=",
					null,
					null,
					null,
					null
				};
				Vector3 val3 = center;
				array[5] = ((object)val3/*cast due to constrained. prefix*/).ToString();
				array[6] = " unloadRadius=";
				array[7] = radius.ToString();
				array[8] = text;
				DebugMsg(string.Concat(array));
			}
		}
		return found;
	}

	private static void ConsiderAIServicePoint(AITransportJob j, Vector3 center, float radius, Vector3 point, ref float best, ref Vector3 result, ref bool found)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		if (float.IsNaN(point.sqrMagnitude) || float.IsInfinity(point.sqrMagnitude) || Vector3.Distance(point, center) > radius)
		{
			return;
		}
		Unit transport = j.Transport;
		Vector3 val = point - ((Component)transport).transform.position;
		float num = val.sqrMagnitude;
		if (num >= best)
		{
			return;
		}
		foreach (AITransportJob value in AITransportJobs.Values)
		{
			if (value != j && Object.op_Implicit((Object)(object)value.Transport))
			{
				Vector3 val2 = (value.Holding ? ((Component)value.Transport).transform.position : value.Destination);
				float num2 = Math.Max(12f, ((BaseGameObject)transport).PhysicalRadius + ((BaseGameObject)value.Transport).PhysicalRadius + 5f);
				val = val2 - point;
				if (val.sqrMagnitude < num2 * num2)
				{
					num += 1000000f;
				}
			}
		}
		if (!(num >= best))
		{
			best = num;
			result = point;
			found = true;
		}
	}

	private static void ValidateAITransportConfig()
	{
		AITransportConfig aITransport = Cfg.AITransport;
		aITransport.AirLoadRadius = ValidHarvestSetting(aITransport.AirLoadRadius, 150f, 1f, 2000f);
		aITransport.AirUnloadRadius = ValidHarvestSetting(aITransport.AirUnloadRadius, 150f, 1f, 2000f);
		aITransport.GroundLoadRadius = ValidHarvestSetting(aITransport.GroundLoadRadius, 40f, 1f, 2000f);
		aITransport.GroundUnloadRadius = ValidHarvestSetting(aITransport.GroundUnloadRadius, 65f, 1f, 2000f);
		aITransport.NearHarvesterDistanceMetres = ValidHarvestSetting(aITransport.NearHarvesterDistanceMetres, 300f, 0f, 100000f);
		aITransport.MediumHarvesterDistanceMetres = ValidHarvestSetting(aITransport.MediumHarvesterDistanceMetres, 600f, aITransport.NearHarvesterDistanceMetres, 100000f);
		aITransport.NearGroundTransports = Math.Max(0, Math.Min(32, aITransport.NearGroundTransports));
		aITransport.MediumGroundTransports = Math.Max(0, Math.Min(32, aITransport.MediumGroundTransports));
		aITransport.FarGroundTransports = Math.Max(0, Math.Min(32, aITransport.FarGroundTransports));
		aITransport.NearAirTransports = Math.Max(0, Math.Min(32, aITransport.NearAirTransports));
		aITransport.MediumAirTransports = Math.Max(0, Math.Min(32, aITransport.MediumAirTransports));
		aITransport.FarAirTransports = Math.Max(0, Math.Min(32, aITransport.FarAirTransports));
		aITransport.ReassignmentCheckSeconds = ValidHarvestSetting(aITransport.ReassignmentCheckSeconds, 10f, 1f, 300f);
		aITransport.NoProgressTimeoutSeconds = ValidHarvestSetting(aITransport.NoProgressTimeoutSeconds, 120f, 10f, 3600f);
		Cfg.AirLoadUnitsPerSecond = Math.Max(1, Math.Min(100000, Cfg.AirLoadUnitsPerSecond));
		Cfg.AirUnloadUnitsPerSecond = Math.Max(1, Math.Min(100000, Cfg.AirUnloadUnitsPerSecond));
		foreach (AITransportJob value in AITransportJobs.Values)
		{
			value.NextAllocation = 0f;
		}
	}

	private static ResourceArea ProductiveAIArea(Unit h, Team team)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (!AIHarvesterTarget(h, team) || h.PlayerControlled || (Object)(object)DriverOf(h) != (Object)null)
		{
			return null;
		}
		AIBaseAgent aIAgent = h.AIAgent;
		AIHarvesterAgent val = (AIHarvesterAgent)(object)((aIAgent is AIHarvesterAgent) ? aIAgent : null);
		if (!Object.op_Implicit((Object)(object)val) || !((AIBaseAgent)val).AIControlled || (int)val.DockState != 0 || val.Vehicle.HasForceMove)
		{
			return null;
		}
		ResourceArea val2 = val.CurrentHarvestArea;
		if (HarvestFixActive && HarvestJobs.TryGetValue(((Object)h).GetInstanceID(), out var value))
		{
			if (value.Line != null || value.Phase == HarvestPhase.Manual || value.Phase == HarvestPhase.Depositing || value.Phase == HarvestPhase.Queued)
			{
				return null;
			}
			val2 = value.Area;
		}
		if (!Object.op_Implicit((Object)(object)val2) || val2.ResourceAmountCurrent <= 0)
		{
			return null;
		}
		return val2;
	}

	private static float NearestAIRefineryDistance(Vector3 from, Team team)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		if (Structure.Structures == null)
		{
			return num;
		}
		foreach (Structure allocationRefinery in AllocationRefineries)
		{
			if (AIRefineryTarget(allocationRefinery, team))
			{
				Vector3 val = ((Component)allocationRefinery).transform.position - from;
				val.y = 0f;
				num = Math.Min(num, val.magnitude);
			}
		}
		return num;
	}

	private static int AIHarvesterLimit(Unit h, bool air)
	{
		float num = AIHarvesterRefineryDistance(h);
		AITransportConfig aITransport = Cfg.AITransport;
		if (num <= aITransport.NearHarvesterDistanceMetres)
		{
			if (!air)
			{
				return aITransport.NearGroundTransports;
			}
			return aITransport.NearAirTransports;
		}
		if (num <= aITransport.MediumHarvesterDistanceMetres)
		{
			if (!air)
			{
				return aITransport.MediumGroundTransports;
			}
			return aITransport.MediumAirTransports;
		}
		if (!air)
		{
			return aITransport.FarGroundTransports;
		}
		return aITransport.FarAirTransports;
	}

	private static float AIHarvesterUse(AITransportJob self, Unit h)
	{
		float num = 0f;
		ResourceArea val = ProductiveAIArea(h, ((BaseGameObject)h).Team);
		int num2 = AIHarvesterLimit(h, air: false);
		int num3 = AIHarvesterLimit(h, air: true);
		foreach (AITransportJob value in AITransportJobs.Values)
		{
			if (value != self && !value.Delivering && !((Object)(object)value.Harvester != (Object)(object)h) && AITransportEligible(value.Transport) && !((Object)(object)value.Team != (Object)(object)((BaseGameObject)h).Team) && !((Object)(object)value.AssignedArea != (Object)(object)val))
			{
				int num4 = (value.Transport.IsFlyingType ? num3 : num2);
				num += ((num4 > 0) ? (1f / (float)num4) : 1f);
			}
		}
		return num;
	}

	private static bool CanAssignAIHarvester(AITransportJob j, Unit h)
	{
		if (!Object.op_Implicit((Object)(object)ProductiveAIArea(h, j.Team)) || ((Object)(object)h == (Object)(object)j.AvoidHarvester && Time.time < j.AvoidUntil))
		{
			return false;
		}
		int num = AIHarvesterLimit(h, j.Transport.IsFlyingType);
		if (num > 0)
		{
			return AIHarvesterUse(j, h) + 1f / (float)num <= 1.0001f;
		}
		return false;
	}

	private static void ReleaseAIHarvester(AITransportJob j)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		j.Harvester = null;
		j.AssignedArea = null;
		j.Holding = false;
		j.Parking = false;
		j.NextMove = 0f;
		j.Attempts = 0;
		j.TargetPosition = Vector3.zero;
		j.BestDistance = float.MaxValue;
	}

	private static void AssignAIHarvester(AITransportJob j, Unit h)
	{
		ReleaseAIHarvester(j);
		j.Harvester = h;
		j.AssignedArea = ProductiveAIArea(h, j.Team);
		j.LastProgress = Time.time;
		j.NextAllocation = Time.time + Cfg.AITransport.ReassignmentCheckSeconds;
		AILog(j, "collection reserved patch=" + HarvestObjectId((Object)(object)j.AssignedArea) + " tierLimit=" + AIHarvesterLimit(h, j.Transport.IsFlyingType));
	}

	private static Unit ChooseAIHarvester(AITransportJob j)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		List<KeyValuePair<Unit, float>> list = new List<KeyValuePair<Unit, float>>();
		foreach (Unit allocationHarvester in AllocationHarvesters)
		{
			if (CanAssignAIHarvester(j, allocationHarvester))
			{
				float num = AIHarvesterRefineryDistance(allocationHarvester);
				if (num != float.MaxValue)
				{
					float num2 = Vector3.Distance(((Component)j.Transport).transform.position, ((Component)allocationHarvester).transform.position);
					float num3 = Math.Min(1f, (float)((BaseGameObject)allocationHarvester).GetStoredResources(j.Team.UsableResource) / (float)CapacityFor(j.Transport));
					float value = (50f + num2 * (j.Transport.IsFlyingType ? 1f : 1.5f)) * (1f + AIHarvesterUse(j, allocationHarvester)) / ((1f + Math.Min(1200f, num) / 600f) * (0.5f + num3));
					list.Add(new KeyValuePair<Unit, float>(allocationHarvester, value));
				}
			}
		}
		list.Sort((KeyValuePair<Unit, float> a, KeyValuePair<Unit, float> b) =>
		{
			int num4 = a.Value.CompareTo(b.Value);
			return (num4 == 0) ? ((Object)a.Key).GetInstanceID().CompareTo(((Object)b.Key).GetInstanceID()) : num4;
		});
		foreach (KeyValuePair<Unit, float> item in list)
		{
			if (AIServicePosition(j, ((Component)item.Key).transform.position, AILoadRadius(j.Transport), out var _))
			{
				return item.Key;
			}
		}
		return null;
	}

	private static void BeginAIDelivery(AITransportJob j, string reason)
	{
		ReleaseAIHarvester(j);
		j.Delivering = true;
		j.Refinery = null;
		j.NextRefinerySearch = 0f;
		AILog(j, reason);
	}

	private static void AllocateAITransport(AITransportJob j, Cargo cargo)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)j.Harvester))
		{
			float num = Vector3.Distance(((Component)j.Transport).transform.position, ((Component)j.Harvester).transform.position);
			if (num + 10f < j.BestDistance)
			{
				j.BestDistance = num;
				j.LastProgress = Time.time;
			}
			ResourceArea val = ProductiveAIArea(j.Harvester, j.Team);
			bool flag = !Object.op_Implicit((Object)(object)val) || (Object)(object)j.AssignedArea != (Object)(object)val;
			bool flag2 = Time.time - j.LastProgress >= Cfg.AITransport.NoProgressTimeoutSeconds;
			bool flag3 = Time.time >= j.NextAllocation;
			if (!flag && !flag2 && (!flag3 || CanAssignAIHarvester(j, j.Harvester)))
			{
				if (flag3)
				{
					j.NextAllocation = Time.time + Cfg.AITransport.ReassignmentCheckSeconds;
				}
				return;
			}
			if (flag2)
			{
				j.AvoidHarvester = j.Harvester;
				j.AvoidUntil = Time.time + Cfg.AITransport.NoProgressTimeoutSeconds;
			}
			AILog(j, flag ? "collection patch lost/depleted; reallocating" : "collection limit or progress check; reallocating");
			ReleaseAIHarvester(j);
			j.NextAllocation = 0f;
			SetTransfer(j.Transport, cargo, null);
		}
		if (Time.time < j.NextAllocation)
		{
			return;
		}
		j.NextAllocation = Time.time + Cfg.AITransport.ReassignmentCheckSeconds;
		Unit val2 = ChooseAIHarvester(j);
		if (cargo.Amount > 0)
		{
			Structure val3 = ClosestAIRefinery(j);
			if (!Object.op_Implicit((Object)(object)val2) || (Object.op_Implicit((Object)(object)val3) && Vector3.Distance(((Component)j.Transport).transform.position, ((Component)val3).transform.position) <= Vector3.Distance(((Component)j.Transport).transform.position, ((Component)val2).transform.position)))
			{
				BeginAIDelivery(j, "delivering partial cargo before next collection");
				return;
			}
		}
		if (Object.op_Implicit((Object)(object)val2))
		{
			AssignAIHarvester(j, val2);
		}
	}

	private static void ParkAITransport(AITransportJob j)
	{
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		if (!j.Parking && Time.time >= j.NextMove)
		{
			j.NextMove = Time.time + Cfg.AITransport.ReassignmentCheckSeconds;
			Structure val = null;
			float num = float.MaxValue;
			foreach (Structure allocationRefinery in AllocationRefineries)
			{
				if (AIRefineryTarget(allocationRefinery, j.Team))
				{
					Vector3 val2 = ((Component)allocationRefinery).transform.position - ((Component)j.Transport).transform.position;
					float sqrMagnitude = val2.sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						val = allocationRefinery;
					}
				}
			}
			if (Object.op_Implicit((Object)(object)val))
			{
				float num2 = AIUnloadRadius(j.Transport) + 150f + ((BaseGameObject)j.Transport).PhysicalRadius;
				float num3 = (float)(Math.Abs((long)((Object)j.Transport).GetInstanceID()) % 32) * MathF.PI / 16f;
				Vector3 val3 = default;
				for (int i = 0; i < 16; i++)
				{
					float num4 = num3 + (float)i * MathF.PI / 8f;
					Vector3 terrainPosition = GamePhysics.GetTerrainPosition(((Component)val).transform.position + new Vector3(Mathf.Cos(num4), 0f, Mathf.Sin(num4)) * num2, 0f, (Terrain)null);
					if (j.Transport.GetPointValidForAI(terrainPosition, ref val3) && !(NearestAIRefineryDistance(val3, j.Team) < AIUnloadRadius(j.Transport) + 75f))
					{
						j.ParkingPoint = val3;
						j.Parking = true;
						j.Holding = false;
						j.NextMove = Time.time + 5f;
						if (AIOrder(j, (OrderDefinition)(object)OrderDefinitionRegistry.Move, val3))
						{
							j.Destination = val3;
						}
						break;
					}
				}
			}
		}
		if (j.Parking && Vector3.Distance(((Component)j.Transport).transform.position, j.ParkingPoint) > 25f)
		{
			if (Time.time >= j.NextMove && !j.Transport.AIAgent.IsFollowingPath)
			{
				j.NextMove = Time.time + 5f;
				if (AIOrder(j, (OrderDefinition)(object)OrderDefinitionRegistry.Move, j.ParkingPoint))
				{
					j.Holding = false;
				}
			}
		}
		else
		{
			AIHold(j);
		}
		AILog(j, "logistics duty idle; waiting for productive collection slot");
	}
}
