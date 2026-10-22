using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SilicaAdminMod;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Si_KingOfTheHill;

public class KingOfTheHill : MelonMod
{
	public class BountyEntry
	{
		public string Name = "";

		public int Amount;

		public string Note = "";
	}

	public class BountyConfig
	{
		public int DefaultAmount = 1000;

		public bool AnnounceOnConnect = true;

		public bool BroadcastOnKill = true;

		public Dictionary<long, BountyEntry> Bounties = new Dictionary<long, BountyEntry>();
	}

	private enum BuyLevel
	{
		Closed,
		Categories,
		UnitList
	}

	private class BuyChoice
	{
		public string Display = "";

		public string Prefab = "";

		public string ObjectInfoName = "";

		public int Cost;

		public bool IsStructure;
	}

	private class BuyState
	{
		public BuyLevel Level;

		public string CategoryBuiltAt = "";

		public List<BuyChoice> UnitsShown = new List<BuyChoice>();

		public List<string> CategoriesShown = new List<string>();
	}

	public class KohConfig
	{
		public bool Enabled = true;

		public bool OnlyInStrategyMode = true;

		public int Mode = 1;

		public int StarterCreditsPerPlayer;

		public bool BuyAdminOnly;

		public bool BountiesEnabled = true;

		public float CaptureRadius = 50f;

		public float BuildExclusionRadius;

		public float EvalIntervalSeconds = 1f;

		public float OwnershipHandoverDelay = 3f;

		public float WinThreshold = 50000f;

		public float MaxCapturePointsPerSecond = 100f;

		public float DominanceThreshold = 0.6f;

		public Dictionary<string, float> WinThresholdPerMap = new Dictionary<string, float>();

		public bool CumulativeAccumulation = true;

		public float PlayerControlledMultiplier = 2f;

		public int OutpostCount = 12;

		public float OutpostBuryDepth = 30f;

		public bool EnterExitNotifications = true;

		public float StatusAnnounceIntervalSeconds = 60f;

		public float RewardKillFraction = 0.1f;

		public float RewardZonePresenceMultiplier = 2f;

		public bool RewardZoneCrabSkipPenalty = true;

		public bool RewardResetOnGameEnd = true;

		public float BuySpawnDistance = 15f;

		public float BuyMinDistanceFromEnemyCritical = 400f;

		public float BuyMaxDistanceFromOwnCritical = 800f;

		public float ShrimpBuyUnlockSeconds = 600f;

		public float BuyCooldownSeconds = 240f;

		public bool StructureKillRewardsEnabled = true;

		public bool BuyStructuresEnabled;

		public bool CommanderRewardEnabled = true;

		public float CommanderRewardMultiplier = 1f;

		public bool HandicapEnabled = true;

		public float HandicapResourceWeight = 1f;

		public float HandicapMilitaryWeight = 0.5f;

		public float HandicapStrength = 1f;

		public float HandicapFloorMult = 0.25f;

		public float HandicapCeilingMult = 3f;

		public float HandicapRecomputeIntervalSeconds = 5f;

		public bool IntroEnabled = true;

		public float IntroStartDelaySeconds = 5f;

		public float IntroSoundLeadSeconds = 0.3f;

		public float IntroLinePauseSeconds = 1f;

		public string SoundIntro = "";

		public float IntroLineIntervalSeconds = 10f;

		public float IntroSoundDelaySeconds;

		public string SoundFirstCapture = "sounds/first_capture.wav";

		public string SoundMilestone25 = "sounds/milestone_25.wav";

		public string SoundMilestone50 = "sounds/milestone_50.wav";

		public string SoundMilestone75 = "sounds/milestone_75.wav";

		public string SoundMilestone95 = "sounds/milestone_95.wav";

		public string SoundFinishingForce = "sounds/cannon_boom.wav";

		public float WinFinishingForceDelaySeconds = 4f;

		public string SoundWinSol = "sounds/sol_wins.wav";

		public string SoundWinCentauri = "sounds/centauri_wins.wav";

		public string SoundWinAlien = "sounds/alien_wins.wav";

		public string WinMessageSol = "Sol Wins - Sol lives forever";

		public string WinMessageCentauri = "Centauri Wins - Get outta here";

		public string WinMessageAlien = "Alien wins - Praise the Queen motherfuckers";

		public string WinMessageDefault = "{name} have conquered the hill — a most resounding victory!";

		public Dictionary<string, string> BuildingToCategory = new Dictionary<string, string>
		{
			{ "Barracks", "Infantry" },
			{ "Light Factory", "LightVehicle" },
			{ "Heavy Factory", "HeavyVehicle" },
			{ "Ultra Heavy Factory", "UltraHeavyVehicle" },
			{ "Air Factory", "Air" },
			{ "Lesser Spawning Cyst", "LesserAlien" },
			{ "Greater Spawning Cyst", "GreaterAlien" },
			{ "Grand Spawning Cyst", "GrandSpawner" },
			{ "Colossal Spawning Cyst", "ColossalAlien" }
		};

		public Dictionary<string, float> CategoryMultiplier = new Dictionary<string, float>
		{
			{ "Infantry", 1f },
			{ "LightVehicle", 1f },
			{ "HeavyVehicle", 1f },
			{ "UltraHeavyVehicle", 1f },
			{ "Air", 1f },
			{ "LesserAlien", 1f },
			{ "GreaterAlien", 1f },
			{ "GrandSpawner", 1f },
			{ "ColossalAlien", 1f },
			{ "Auxiliary", 0f }
		};

		public Dictionary<string, float> PerUnitOverride = new Dictionary<string, float> { { "Crab", 0.25f } };

		public string DefaultCategoryForUnknown = "Infantry";
	}

	internal class StructRow
	{
		public string Display = "";

		public string Prefab = "";

		public string Faction = "";

		public string ObjectInfo = "";
	}

	private static bool _antiBuildHooked;

	private static float _lastBuildDenyNoticeTime = -10f;

	private static BountyConfig _bountyCfg = new BountyConfig();

	private static string _bountyConfigPath = "";

	private static readonly HashSet<long> _announcedBounties = new HashSet<long>();

	private static readonly Dictionary<long, BuyState> _buyStates = new Dictionary<long, BuyState>();

	private static readonly Dictionary<long, float> _lastPurchaseTime = new Dictionary<long, float>();

	private const string COLOR_AFFORD = "#39ff14";

	private const string COLOR_TOOEXPENSIVE = "#888888";

	private const string STRUCTURES_CATEGORY = "Buildings";

	private static bool _buyChatHooked;

	private static readonly Dictionary<string, GameObject> _prefabByObjectInfo = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

	private static bool _prefabIndexBuilt;

	private static FieldInfo? _ubMenuStatesField;

	private static bool _ubProbed;

	public static KohConfig Cfg = new KohConfig();

	private static string _configPath = "";

	private static Team? _currentKing;

	private static Team? _provisionalKing;

	private static float _handoverElapsed;

	private static readonly Dictionary<Team, float> _accumulated = new Dictionary<Team, float>();

	private static float _totalAccumulated;

	private static int _lastLoggedAccumulationBucket = -1;

	private static float _lastTickGain;

	private static float _lastTickKingScore;

	private static float _lastTickTotalScore;

	private static float _lastTickMaxAllowed;

	private static readonly Dictionary<int, HashSet<Team>> _milestonesPlayed = new Dictionary<int, HashSet<Team>>();

	private static readonly HashSet<int> _milestonesPlayedCumulative = new HashSet<int>();

	private static bool _firstCapturePlayed;

	private static bool _winTriggered;

	private static float _statusAnnounceTimer;

	private const string COLOR_SOL = "#328cff";

	private const string COLOR_CENT = "#eb4646";

	private const string COLOR_ALIEN = "#50c832";

	private const string COLOR_WILDLIFE = "#c8963c";

	private const string COLOR_URANIUM = "#39ff14";

	private const string COLOR_DEFAULT = "#cccccc";

	private static MethodInfo? _printLogLineMethod;

	private static bool _printLogLineResolved;

	private const string OUTPOST_PREFAB_NAME = "Outpost";

	private static readonly List<GameObject> _outposts = new List<GameObject>();

	private static readonly List<string> _outpostTeamNames = new List<string>();

	private static readonly List<Vector3> _outpostPositions = new List<Vector3>();

	private static readonly List<Team?> _outpostSlotTeams = new List<Team>();

	private static Team? _outpostKingTeam;

	private static int _outpostKingCount;

	private static Team? _neutralTeam;

	private static Type? _damageManagerType;

	private static MethodInfo? _applyDamageMethod;

	private static bool _damagePatchInstalled;

	private static Type? _teamTypeForTargeting;

	private static FieldInfo? _teamShortNameField;

	private static object? _cachedGamemasterTeam;

	private static bool _targetingPatchInstalled;

	private static readonly Dictionary<object, bool> _teamClassification = new Dictionary<object, bool>();

	private static int _damageMissLog = 0;

	private static int _damageHitLog = 0;

	private const int DAMAGE_DIAG_LIMIT = 5;

	private static int _clientHitBlockLog = 0;

	private static readonly Dictionary<long, int> _playerCredits = new Dictionary<long, int>();

	private static bool _rewardsHooked;

	private static readonly Dictionary<Team, float> _handicapMult = new Dictionary<Team, float>();

	private static float _handicapTimer = 0f;

	private static float _evalTimer = 0f;

	internal static float _roundStartTime = 0f;

	private static readonly HashSet<long> _starterCreditsGranted = new HashSet<long>();

	private static float _starterCreditsCheckTimer = 0f;

	private const float STARTER_CREDITS_CHECK_INTERVAL = 5f;

	private const float STARTUP_CHECK_DELAY = 3f;

	private static float _startupCheckTimer = 0f;

	private static bool _startupCheckPending = false;

	private static readonly (string chat, (string sound, float dur)[] clips)[] _introScript = new (string, (string, float)[])[7]
	{
		("[KGT] <color=#39ff14>=== KING OF THE GALACTIC TELEPORT ===</color>", new (string, float)[1] { ("sounds/intro_01a.wav", 2.76f) }),
		("[KGT] A neutral Galactic Teleporter has appeared somewhere on the map. Capture it for your team.", new (string, float)[2]
		{
			("sounds/intro_02a.wav", 4.34f),
			("sounds/intro_02b.wav", 2.5f)
		}),
		("[KGT] Move units into the capture zone. The team with majority presence holds the Teleporter.", new (string, float)[2]
		{
			("sounds/intro_03a.wav", 3.29f),
			("sounds/intro_03b.wav", 4.34f)
		}),
		("[KGT] Capture progress is shown in your team's colour. First team to fully capture wins the round.", new (string, float)[2]
		{
			("sounds/intro_04a.wav", 3.6f),
			("sounds/intro_04b.wav", 3.62f)
		}),
		("[KGT] <color=#ffcc00>Commander reward:</color> while your team has units in the zone, resources tick into your faction treasury automatically.", new (string, float)[2]
		{
			("sounds/intro_05a.wav", 5.16f),
			("sounds/intro_05b.wav", 4.13f)
		}),
		("[KGT] <color=#ffcc00>Player reward:</color> earn personal credits for kills and for time spent in the zone.", new (string, float)[2]
		{
			("sounds/intro_06a.wav", 4.9f),
			("sounds/intro_06b.wav", 2.66f)
		}),
		("[KGT] Use <color=#39ff14>/buy</color> to spend credits on units. Type <color=#39ff14>/koh</color> for live capture status.", new (string, float)[2]
		{
			("sounds/intro_07a.wav", 3.7f),
			("sounds/intro_07b.wav", 3.53f)
		})
	};

	private const float INTRO_INTER_CLIP_GAP = 0.15f;

	private static int _introIdx = -1;

	private static int _introClipIdx = 0;

	private static int _introStage = 0;

	private static float _introTimer = 0f;

	private const float WIN_FORCE_FALLBACK_SECONDS = 60f;

	private static float _winFallbackTimer;

	private static Team? _winningTeam;

	private static float _winFinishingDelayTimer;

	private static bool _winFinishingPending;

	private const string PREFAB_GOLIATH = "Goliath";

	private const string PREFAB_SIEGE_TANK = "Sol_UltraHeavy_SiegeTank";

	private const string PREFAB_CRIMSON_TANK = "Cent_UltraHeavy_CrimsonTank";

	private const float FORCE_RING_RADIUS = 90f;

	private const float GOLIATH_DROP_HEIGHT = 80f;

	private static Transform? _kohTower;

	private static Vector3 _kohCenter = Vector3.zero;

	private static float _captureRadius = 50f;

	private static float _exclusionRadius = 0f;

	private static bool _hasKoh = false;

	private static GameObject? _kohPrefabRef;

	private static Team? _kohTeam;

	private static Quaternion _kohRotation = Quaternion.identity;

	private static float _kohAliveCheckTimer = 0f;

	private static int _kohRespawnCount = 0;

	private const float KOH_ALIVE_CHECK_INTERVAL = 3f;

	private static float _winThreshold = 50000f;

	private static Type? _specialsType;

	private static FieldInfo? _specialsKohField;

	private static EventInfo? _specialsReadyEvent;

	private static bool _subscribedReady;

	private static readonly Dictionary<string, string> _unitToBuilding = new Dictionary<string, string>();

	private static readonly Dictionary<string, int> _unitCost = new Dictionary<string, int>();

	private static readonly Dictionary<string, int> _unitCostVanilla = new Dictionary<string, int>();

	private static readonly Dictionary<string, string> _unitFaction = new Dictionary<string, string>();

	private static readonly Dictionary<string, string> _unitPrefab = new Dictionary<string, string>();

	private static readonly HashSet<string> _isStructure = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, int> _unitCostByInternal = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, string> _internalByDisplay = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private static readonly List<StructRow> _structRows = new List<StructRow>();

	private static bool _dumpLoaded;

	private static readonly HashSet<Player> _playersInZone = new HashSet<Player>();

	private static readonly HashSet<Player> _playersInZonePrev = new HashSet<Player>();

	public static bool ModeKohActive
	{
		get
		{
			if (Cfg != null && Cfg.Enabled)
			{
				return Cfg.Mode == 1;
			}
			return false;
		}
	}

	public static bool ModeBuyActive
	{
		get
		{
			if (Cfg != null && Cfg.Enabled)
			{
				if (Cfg.Mode != 1)
				{
					return Cfg.Mode == 2;
				}
				return true;
			}
			return false;
		}
	}

	public static bool ModeAnyActive
	{
		get
		{
			if (Cfg != null && Cfg.Enabled)
			{
				return Cfg.Mode != 3;
			}
			return false;
		}
	}

	private static void HookAntiBuild()
	{
		if (_antiBuildHooked)
		{
			return;
		}
		try
		{
			Event_Construction.OnRequestBuildStructure -= OnRequestBuild_KohExclusion;
			Event_Construction.OnRequestBuildStructure += OnRequestBuild_KohExclusion;
			_antiBuildHooked = true;
			MelonLogger.Msg("[KGT] Subscribed to Event_Construction.OnRequestBuildStructure for build-exclusion enforcement");
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] HookAntiBuild failed: " + ex.Message);
		}
	}

	private static void OnRequestBuild_KohExclusion(object sender, OnRequestBuildArgs args)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!ModeKohActive || !_hasKoh || args == null || args.Block || (Object)(object)args.ParentStructure == (Object)null)
			{
				return;
			}
			Team team = ((BaseGameObject)args.ParentStructure).Team;
			if ((Object)(object)team == (Object)null)
			{
				return;
			}
			float num = ((_exclusionRadius > 0f) ? _exclusionRadius : Cfg.BuildExclusionRadius);
			if (!(num <= 0f))
			{
				bool flag = IsGamemasterTeam(team);
				float num2 = args.Position.x - _kohCenter.x;
				float num3 = args.Position.z - _kohCenter.z;
				float num4 = num2 * num2 + num3 * num3;
				float num5 = num * num;
				bool flag2 = num4 <= num5;
				MelonLogger.Msg(string.Format("[KGT] AntiBuild fired: team={0} type={1} dist={2:F0}m (excl={3:F0}m) isNeutral={4} inside={5}", team.GetTeamShortName(), args.ConstructionData?.ObjectInfo?.DisplayName ?? "?", Mathf.Sqrt(num4), num, flag, flag2));
				if (!flag && flag2)
				{
					args.Block = true;
					float num6 = Mathf.Sqrt(num4);
					MelonLogger.Msg(string.Format("[KGT] Build denied: team={0} type={1} dist={2:F0}m (excl={3:F0}m)", team.GetTeamShortName(), args.ConstructionData?.ObjectInfo?.DisplayName ?? "?", num6, num));
					NotifyBuildBlocked(team, args.ConstructionData?.ObjectInfo?.DisplayName ?? "structure", num6, num);
				}
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] OnRequestBuild_KohExclusion threw: " + ex.Message);
		}
	}

	private static void NotifyBuildBlocked(Team team, string what, float dist, float excl)
	{
		try
		{
			if (!(Time.unscaledTime - _lastBuildDenyNoticeTime < 3f))
			{
				_lastBuildDenyNoticeTime = Time.unscaledTime;
				BroadcastAllChat($"[KGT] {team.GetTeamShortName()} cannot build {what} inside the Galactic Teleporter exclusion zone ({dist:F0}m / {excl:F0}m).");
			}
		}
		catch
		{
		}
	}

	private static void LoadBountyConfig()
	{
		_bountyConfigPath = Path.Combine("UserData", "KingOfTheHill_cfg", "Bounties.json");
		try
		{
			if (!File.Exists(_bountyConfigPath))
			{
				_bountyCfg = new BountyConfig();
				SaveBountyConfig();
				MelonLogger.Msg("[KGT/Bounty] No bounty config — wrote empty default to " + _bountyConfigPath);
				return;
			}
			_bountyCfg = JsonConvert.DeserializeObject<BountyConfig>(File.ReadAllText(_bountyConfigPath)) ?? new BountyConfig();
			if (_bountyCfg.Bounties == null)
			{
				_bountyCfg.Bounties = new Dictionary<long, BountyEntry>();
			}
			MelonLogger.Msg($"[KGT/Bounty] Loaded {_bountyCfg.Bounties.Count} bounty target(s) from {_bountyConfigPath} (default {_bountyCfg.DefaultAmount}cr)");
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT/Bounty] Failed to load " + _bountyConfigPath + ": " + ex.Message);
			_bountyCfg = new BountyConfig();
		}
	}

	private static void SaveBountyConfig()
	{
		try
		{
			string directoryName = Path.GetDirectoryName(_bountyConfigPath);
			if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			string contents = JsonConvert.SerializeObject((object)_bountyCfg, (Formatting)1);
			File.WriteAllText(_bountyConfigPath, contents);
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT/Bounty] Failed to save " + _bountyConfigPath + ": " + ex.Message);
		}
	}

	private static int GetBountyAmount(long steamId)
	{
		if (!_bountyCfg.Bounties.TryGetValue(steamId, out BountyEntry value))
		{
			return 0;
		}
		if (value.Amount <= 0)
		{
			return _bountyCfg.DefaultAmount;
		}
		return value.Amount;
	}

	private static string GetBountyDisplayName(long steamId)
	{
		if (_bountyCfg.Bounties.TryGetValue(steamId, out BountyEntry value) && !string.IsNullOrEmpty(value.Name))
		{
			return value.Name;
		}
		return steamId.ToString();
	}

	private static void AnnounceBountiesIfNeeded()
	{
		if (!Cfg.BountiesEnabled || !_bountyCfg.AnnounceOnConnect || _bountyCfg.Bounties.Count == 0)
		{
			return;
		}
		try
		{
			List<Player> players = Player.Players;
			if (players == null)
			{
				return;
			}
			for (int i = 0; i < players.Count; i++)
			{
				Player val = players[i];
				if (!((Object)(object)val == (Object)null) && !((Object)(object)val == (Object)(object)NetworkGameServer.GetServerPlayer()))
				{
					long playerSteamId = GetPlayerSteamId(val);
					if (playerSteamId != 0L && _bountyCfg.Bounties.ContainsKey(playerSteamId) && _announcedBounties.Add(playerSteamId))
					{
						int bountyAmount = GetBountyAmount(playerSteamId);
						string bountyDisplayName = GetBountyDisplayName(playerSteamId);
						BroadcastAllChat($"[KGT/Bounty] <color=#ff5050>HEADHUNTER:</color> <color=#39ff14>{bountyAmount}</color>cr bonus to anyone who kills <color=#ffcc00>{bountyDisplayName}</color>!");
					}
				}
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT/Bounty] AnnounceBountiesIfNeeded threw: " + ex.Message);
		}
	}

	private static int PayBountyIfApplicable(Unit victim, Player killer)
	{
		if (!Cfg.BountiesEnabled)
		{
			return 0;
		}
		if (_bountyCfg.Bounties.Count == 0)
		{
			return 0;
		}
		if ((Object)(object)victim == (Object)null || (Object)(object)killer == (Object)null)
		{
			return 0;
		}
		Player val = null;
		try
		{
			val = victim.ControlledBy;
		}
		catch
		{
		}
		if ((Object)(object)val == (Object)null)
		{
			return 0;
		}
		if ((Object)(object)val == (Object)(object)killer)
		{
			return 0;
		}
		long playerSteamId = GetPlayerSteamId(val);
		if (playerSteamId == 0L)
		{
			return 0;
		}
		int bountyAmount = GetBountyAmount(playerSteamId);
		if (bountyAmount <= 0)
		{
			return 0;
		}
		long playerSteamId2 = GetPlayerSteamId(killer);
		if (playerSteamId2 == 0L)
		{
			return 0;
		}
		AddCredits(playerSteamId2, bountyAmount);
		string bountyDisplayName = GetBountyDisplayName(playerSteamId);
		string arg = "";
		try
		{
			arg = killer.PlayerName ?? "";
		}
		catch
		{
		}
		try
		{
			HelperMethods.SendChatMessageToPlayer(killer, $"[KGT/Bounty] <color=#39ff14>+{bountyAmount}</color>cr BOUNTY for killing <color=#ffcc00>{bountyDisplayName}</color>!");
		}
		catch
		{
		}
		if (_bountyCfg.BroadcastOnKill)
		{
			BroadcastAllChat($"[KGT/Bounty] <color=#ffcc00>{bountyDisplayName}</color> has been hunted down by <color=#39ff14>{arg}</color> — <color=#39ff14>{bountyAmount}</color>cr bounty paid.");
		}
		return bountyAmount;
	}

	private static float RoundElapsedSeconds()
	{
		if (!(_roundStartTime > 0f))
		{
			return float.PositiveInfinity;
		}
		return Mathf.Max(0f, Time.time - _roundStartTime);
	}

	private static bool IsShrimpUnit(string displayName)
	{
		if (!string.IsNullOrEmpty(displayName))
		{
			return displayName.IndexOf("shrimp", StringComparison.OrdinalIgnoreCase) >= 0;
		}
		return false;
	}

	private static string FormatMMSS(float seconds)
	{
		if (seconds < 0f)
		{
			seconds = 0f;
		}
		int num = Mathf.CeilToInt(seconds);
		int num2 = num / 60;
		int num3 = num % 60;
		if (num2 <= 0)
		{
			return $"{num3}s";
		}
		return $"{num2}m{num3:00}s";
	}

	private static void OnBuyCommand(Player? caller, string args)
	{
		if ((Object)(object)caller == (Object)null)
		{
			return;
		}
		if (!ModeBuyActive)
		{
			Reply(caller, "[KGT/Buy] /buy is currently disabled.");
			return;
		}
		if (Cfg.BuyAdminOnly && !caller.CanAdminExecute(Power.Generic))
		{
			Reply(caller, "[KGT/Buy] /buy is admin-only on this server.");
			return;
		}
		long playerSteamId = GetPlayerSteamId(caller);
		if (caller.CanAdminExecute(Power.Generic) && IsUnitBalanceEditorOpen(playerSteamId))
		{
			Reply(caller, "[KGT/Buy] You have a balance editor (/b) session open. Close it first (type /b again), then run /buy.");
			return;
		}
		if (!_buyStates.TryGetValue(playerSteamId, out BuyState value))
		{
			value = new BuyState();
			_buyStates[playerSteamId] = value;
		}
		string text = (args ?? "").Trim();
		string[] array = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		string text2;
		if (array.Length >= 2)
		{
			text2 = array[1].ToLowerInvariant();
		}
		else
		{
			text2 = ((array.Length != 1 || array[0].Length <= 0 || array[0][0] == '/' || array[0].Equals("buy", StringComparison.OrdinalIgnoreCase)) ? "" : array[0].ToLowerInvariant());
		}
		MelonLogger.Msg($"[KGT/Buy/DBG] args='{text}' parts={array.Length} sub='{text2}' level={value.Level} cats={value.CategoriesShown.Count} units={value.UnitsShown.Count}");
		if (string.IsNullOrEmpty(text2))
		{
			if (value.Level == BuyLevel.Closed)
			{
				value.Level = BuyLevel.Categories;
				value.CategoryBuiltAt = "";
				ShowCategories(caller, value);
			}
			else if (value.Level == BuyLevel.UnitList)
			{
				ShowUnits(caller, value);
			}
			else
			{
				ShowCategories(caller, value);
			}
		}
		else
		{
			HandleBuyNav(caller, value, text2);
		}
	}

	private static void HandleBuyNav(Player caller, BuyState state, string sub)
	{
		switch (sub)
		{
		case "exit":
		case "close":
		case "0":
			state.Level = BuyLevel.Closed;
			Reply(caller, "[KGT/Buy] Menu closed.");
			return;
		case "back":
			if (state.Level == BuyLevel.UnitList)
			{
				state.Level = BuyLevel.Categories;
				ShowCategories(caller, state);
			}
			else
			{
				state.Level = BuyLevel.Closed;
				Reply(caller, "[KGT/Buy] Menu closed.");
			}
			return;
		}
		if (int.TryParse(sub, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) && result >= 1)
		{
			if (state.Level == BuyLevel.Categories)
			{
				if (result > state.CategoriesShown.Count)
				{
					Reply(caller, "[KGT/Buy] Invalid choice.");
					return;
				}
				state.CategoryBuiltAt = state.CategoriesShown[result - 1];
				state.Level = BuyLevel.UnitList;
				ShowUnits(caller, state);
			}
			else if (state.Level == BuyLevel.UnitList)
			{
				if (result > state.UnitsShown.Count)
				{
					Reply(caller, "[KGT/Buy] Invalid choice.");
				}
				else
				{
					TryPurchase(caller, state.UnitsShown[result - 1]);
				}
			}
			else
			{
				Reply(caller, "[KGT/Buy] Open the menu first: /buy");
			}
		}
		else
		{
			Reply(caller, "[KGT/Buy] Usage: /1-N to drill in / buy  ·  /back  ·  /0 to close");
		}
	}

	private static void OnRequestChat_BuyNav(object? sender, OnRequestPlayerChatArgs args)
	{
		try
		{
			if ((Object)(object)args.Player == (Object)null)
			{
				return;
			}
			long playerSteamId = GetPlayerSteamId(args.Player);
			if (!_buyStates.TryGetValue(playerSteamId, out BuyState value) || value.Level == BuyLevel.Closed)
			{
				return;
			}
			string text = (args.Text ?? "").Trim();
			if (text.Length >= 2 && text[0] == '/')
			{
				string text2 = text.Substring(1).ToLowerInvariant();
				if (int.TryParse(text2, NumberStyles.Integer, CultureInfo.InvariantCulture, out var _) || !(text2 != "back") || !(text2 != "0") || !(text2 != "exit") || !(text2 != "close"))
				{
					HandleBuyNav(args.Player, value, text2);
					args.Block = true;
				}
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT/Buy] OnRequestChat threw: " + ex.Message);
		}
	}

	private static void HookBuyChat()
	{
		if (_buyChatHooked)
		{
			return;
		}
		try
		{
			Event_Chat.OnRequestPlayerChat -= OnRequestChat_BuyNav;
			Event_Chat.OnRequestPlayerChat += OnRequestChat_BuyNav;
			_buyChatHooked = true;
			MelonLogger.Msg("[KGT/Buy] Subscribed to Event_Chat.OnRequestPlayerChat for /1-/N nav");
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT/Buy] HookBuyChat failed: " + ex.Message);
		}
	}

	private static void ReHookBuyChat()
	{
		_buyChatHooked = false;
		HookBuyChat();
	}

	private static void BuildPrefabIndex(bool force)
	{
		if (_prefabIndexBuilt && !force)
		{
			return;
		}
		_prefabIndexBuilt = true;
		try
		{
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			if (array == null)
			{
				return;
			}
			_prefabByObjectInfo.Clear();
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo val in array2)
			{
				if (!((Object)(object)val == (Object)null) && !((Object)(object)val.Prefab == (Object)null))
				{
					string text = ((Object)val).name ?? "";
					if (text.Length != 0)
					{
						_prefabByObjectInfo[text] = val.Prefab;
					}
				}
			}
			MelonLogger.Msg($"[KGT/Buy] Prefab index built: {_prefabByObjectInfo.Count} ObjectInfo→prefab entries");
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT/Buy] Prefab index build failed (" + ex.Message + ") — falling back to name lookup");
		}
	}

	private static GameObject? PrefabForObjectInfo(string objectInfoName)
	{
		if (string.IsNullOrEmpty(objectInfoName))
		{
			return null;
		}
		BuildPrefabIndex(force: false);
		bool flag = _prefabByObjectInfo.TryGetValue(objectInfoName, out GameObject value);
		if (flag && (Object)(object)value != (Object)null)
		{
			return value;
		}
		if (!flag)
		{
			return null;
		}
		BuildPrefabIndex(force: true);
		if (!_prefabByObjectInfo.TryGetValue(objectInfoName, out value) || !((Object)(object)value != (Object)null))
		{
			return null;
		}
		return value;
	}

	private static GameObject? PrefabByName(string prefabName)
	{
		if (string.IsNullOrEmpty(prefabName))
		{
			return null;
		}
		try
		{
			int spawnablePrefabIndex = GameDatabase.GetSpawnablePrefabIndex(prefabName);
			return (spawnablePrefabIndex >= 0) ? GameDatabase.GetSpawnablePrefab(spawnablePrefabIndex) : null;
		}
		catch
		{
			return null;
		}
	}

	private static GameObject? ResolveBuyPrefab(BuyChoice choice, out string resolvedName)
	{
		GameObject val = PrefabForObjectInfo(choice.ObjectInfoName);
		if ((Object)(object)val != (Object)null)
		{
			resolvedName = ((Object)val).name;
			try
			{
				if (GameDatabase.GetSpawnablePrefabIndex(val) < 0)
				{
					MelonLogger.Warning("[KGT/Buy] '" + resolvedName + "' (from " + choice.ObjectInfoName + ") is not in the spawnable DB — spawning the ObjectInfo prefab directly.");
				}
			}
			catch
			{
			}
			return val;
		}
		val = PrefabByName(choice.Prefab);
		resolvedName = (((Object)(object)val != (Object)null) ? ((Object)val).name : choice.Prefab);
		return val;
	}

	private static bool CanResolveBuyChoice(BuyChoice choice)
	{
		if (!((Object)(object)PrefabForObjectInfo(choice.ObjectInfoName) != (Object)null))
		{
			return (Object)(object)PrefabByName(choice.Prefab) != (Object)null;
		}
		return true;
	}

	private static bool CanResolveUnit(string display)
	{
		_internalByDisplay.TryGetValue(display, out string value);
		_unitPrefab.TryGetValue(display, out string value2);
		if (!((Object)(object)PrefabForObjectInfo(value ?? "") != (Object)null))
		{
			return (Object)(object)PrefabByName(value2 ?? "") != (Object)null;
		}
		return true;
	}

	private static void ShowCategories(Player caller, BuyState state)
	{
		int credits = GetCredits(caller);
		string text = DumpFactionForTeam(caller.Team);
		if (string.IsNullOrEmpty(text))
		{
			Reply(caller, "[KGT/Buy] No faction detected for your team — cannot show menu.");
			return;
		}
		HashSet<string> hashSet = new HashSet<string>();
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, string> item in _unitFaction)
		{
			if (string.Equals(item.Value, text, StringComparison.OrdinalIgnoreCase) && !_isStructure.Contains(item.Key) && _unitToBuilding.TryGetValue(item.Key, out string value) && !string.IsNullOrEmpty(value) && _unitCost.TryGetValue(item.Key, out var value2) && value2 > 0 && CanResolveUnit(item.Key) && hashSet.Add(value))
			{
				list.Add(value);
			}
		}
		list.Sort((string a, string b) => CategoryOrder(a).CompareTo(CategoryOrder(b)));
		if (Cfg.BuyStructuresEnabled && CountStructuresForFaction(text) > 0)
		{
			list.Add("Buildings");
		}
		state.CategoriesShown = list;
		Reply(caller, string.Format("[KGT/Buy] === Buy Menu === Credits: <color={0}>{1}</color>", "#39ff14", credits));
		for (int num = 0; num < list.Count; num++)
		{
			int num2 = CountUnitsInCategoryForFaction(list[num], text);
			Reply(caller, $"  {num + 1}. {list[num]} ({num2})");
		}
		Reply(caller, "/1-/N drill in  ·  /0 to close  (or /buy <N>)");
	}

	private static void ShowUnits(Player caller, BuyState state)
	{
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		int credits = GetCredits(caller);
		string b = DumpFactionForTeam(caller.Team);
		bool flag = string.Equals(state.CategoryBuiltAt, "Buildings", StringComparison.OrdinalIgnoreCase);
		List<BuyChoice> list = new List<BuyChoice>();
		if (flag)
		{
			foreach (StructRow structRow in _structRows)
			{
				if (string.Equals(structRow.Faction, b, StringComparison.OrdinalIgnoreCase) && (!string.IsNullOrEmpty(structRow.Prefab) || !string.IsNullOrEmpty(structRow.ObjectInfo)) && _unitCost.TryGetValue(structRow.Display, out var value) && value > 0)
				{
					BuyChoice buyChoice = new BuyChoice
					{
						Display = structRow.Display,
						Prefab = structRow.Prefab,
						ObjectInfoName = structRow.ObjectInfo,
						Cost = value,
						IsStructure = true
					};
					if (CanResolveBuyChoice(buyChoice))
					{
						list.Add(buyChoice);
					}
				}
			}
		}
		else
		{
			foreach (KeyValuePair<string, string> item in _unitFaction)
			{
				if (string.Equals(item.Value, b, StringComparison.OrdinalIgnoreCase) && !_isStructure.Contains(item.Key) && _unitToBuilding.TryGetValue(item.Key, out string value2) && string.Equals(value2, state.CategoryBuiltAt, StringComparison.OrdinalIgnoreCase) && _unitCost.TryGetValue(item.Key, out var value3) && value3 > 0)
				{
					_unitPrefab.TryGetValue(item.Key, out string value4);
					_internalByDisplay.TryGetValue(item.Key, out string value5);
					BuyChoice buyChoice2 = new BuyChoice
					{
						Display = item.Key,
						Prefab = (value4 ?? ""),
						ObjectInfoName = (value5 ?? ""),
						Cost = value3,
						IsStructure = false
					};
					if (CanResolveBuyChoice(buyChoice2))
					{
						list.Add(buyChoice2);
					}
				}
			}
		}
		list.Sort((BuyChoice a, BuyChoice buyChoice4) => a.Cost.CompareTo(buyChoice4.Cost));
		state.UnitsShown = list;
		float num = ((Cfg.ShrimpBuyUnlockSeconds > 0f) ? (Cfg.ShrimpBuyUnlockSeconds - RoundElapsedSeconds()) : 0f);
		Reply(caller, string.Format("[KGT/Buy] === {0} === Credits: <color={1}>{2}</color>", state.CategoryBuiltAt, "#39ff14", credits));
		Unit controlledUnit = caller.ControlledUnit;
		if ((Object)(object)controlledUnit != (Object)null && IsTooFarFromOwnCritical(caller, ((Component)controlledUnit).transform.position, out var distance))
		{
			Reply(caller, string.Format("  <color={0}>You are {1:F0}m from your nearest HQ/Nest — purchases are refused beyond {2:F0}m.</color>", "#888888", distance, Cfg.BuyMaxDistanceFromOwnCritical));
		}
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			BuyChoice buyChoice3 = list[num2];
			if (num > 0f && IsShrimpUnit(buyChoice3.Display))
			{
				Reply(caller, string.Format("  {0}. <color={1}>{2}</color>  <color={3}>LOCKED — unlocks in {4}</color>", num2 + 1, "#888888", buyChoice3.Display, "#888888", FormatMMSS(num)));
			}
			else
			{
				string text = ((buyChoice3.Cost <= credits) ? "#39ff14" : "#888888");
				Reply(caller, $"  {num2 + 1}. <color={text}>{buyChoice3.Display}</color>  cost <color={text}>{buyChoice3.Cost}</color>cr");
			}
		}
		Reply(caller, "/1-/N to buy  ·  /back  ·  /0 to close  (or /buy <N>)");
	}

	private static void TryPurchase(Player caller, BuyChoice choice)
	{
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_0445: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		long playerSteamId = GetPlayerSteamId(caller);
		int credits = GetCredits(playerSteamId);
		string display = choice.Display;
		int cost = choice.Cost;
		if (cost <= 0)
		{
			Reply(caller, "[KGT/Buy] " + display + ": cost unknown — cannot purchase.");
			return;
		}
		if (Cfg.ShrimpBuyUnlockSeconds > 0f && IsShrimpUnit(display))
		{
			float num = RoundElapsedSeconds();
			if (num < Cfg.ShrimpBuyUnlockSeconds)
			{
				float seconds = Cfg.ShrimpBuyUnlockSeconds - num;
				Reply(caller, "[KGT/Buy] " + display + " is locked for the first " + FormatMMSS(Cfg.ShrimpBuyUnlockSeconds) + " of the round — unlocks in " + FormatMMSS(seconds) + ". <color=#888888>Credits not charged.</color>");
				return;
			}
		}
		if (Cfg.BuyCooldownSeconds > 0f && _lastPurchaseTime.TryGetValue(playerSteamId, out var value))
		{
			float num2 = Time.time - value;
			if (num2 < Cfg.BuyCooldownSeconds)
			{
				float seconds2 = Cfg.BuyCooldownSeconds - num2;
				Reply(caller, "[KGT/Buy] Purchase on cooldown — wait " + FormatMMSS(seconds2) + " before buying again. <color=#888888>Credits not charged.</color>");
				return;
			}
		}
		if (credits < cost)
		{
			Reply(caller, $"[KGT/Buy] Insufficient credits — {display} costs {cost}, you have {credits}.");
			return;
		}
		Unit controlledUnit = caller.ControlledUnit;
		if ((Object)(object)controlledUnit == (Object)null)
		{
			Reply(caller, "[KGT/Buy] You must be controlling a unit (the purchase spawns next to you).");
			return;
		}
		if (string.IsNullOrEmpty(choice.Prefab) && string.IsNullOrEmpty(choice.ObjectInfoName))
		{
			Reply(caller, "[KGT/Buy] " + display + ": prefab name unknown — cannot spawn.");
			return;
		}
		GameObject val = ResolveBuyPrefab(choice, out string resolvedName);
		if ((Object)(object)val == (Object)null)
		{
			Reply(caller, "[KGT/Buy] Prefab '" + resolvedName + "' not in spawnable database — purchase aborted.");
			MelonLogger.Warning("[KGT/Buy] Could not resolve " + display + ": objectInfo='" + choice.ObjectInfoName + "' nameHint='" + choice.Prefab + "'");
			return;
		}
		Vector3 position = ((Component)controlledUnit).transform.position;
		Vector3 forward = ((Component)controlledUnit).transform.forward;
		float num3 = Mathf.Max(2f, Cfg.BuySpawnDistance);
		Vector3 val2 = position + forward * num3;
		float num4 = SampleSurfaceY_NearAnchor(Terrain.activeTerrain, val2.x, val2.z, position.y) + 2f;
		Vector3 val3 = new Vector3(val2.x, num4, val2.z);
		if (IsTooCloseToEnemyCritical(caller, val3, out string blocker, out float distance))
		{
			Reply(caller, string.Format("[KGT/Buy] Too close to enemy {0}: {1:F0}m (min {2:F0}m). Move further away and try again. <color={3}>Credits not charged.</color>", blocker, distance, Cfg.BuyMinDistanceFromEnemyCritical, "#888888"));
			return;
		}
		if (IsTooFarFromOwnCritical(caller, val3, out var distance2))
		{
			Reply(caller, string.Format("[KGT/Buy] Too far from your nearest HQ/Nest: {0:F0}m (max {1:F0}m). Move closer to base and try again. <color={2}>Credits not charged.</color>", distance2, Cfg.BuyMaxDistanceFromOwnCritical, "#888888"));
			return;
		}
		GameObject val4 = null;
		try
		{
			val4 = Game.SpawnPrefab(val, (Player)null, caller.Team, val3, ((Component)controlledUnit).transform.rotation, true, true);
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT/Buy] Game.SpawnPrefab threw for " + resolvedName + ": " + ex.Message);
		}
		if ((Object)(object)val4 == (Object)null)
		{
			Reply(caller, "[KGT/Buy] Spawn failed for '" + display + "' — credits NOT charged.");
			return;
		}
		AddCredits(playerSteamId, -cost);
		_lastPurchaseTime[playerSteamId] = Time.time;
		int credits2 = GetCredits(playerSteamId);
		string text = ((Cfg.BuyCooldownSeconds > 0f) ? (" Next buy in " + FormatMMSS(Cfg.BuyCooldownSeconds) + ".") : "");
		Reply(caller, string.Format("[KGT/Buy] Purchased <color={0}>{1}</color> for {2}cr. Remaining: {3}cr.{4}", "#39ff14", display, cost, credits2, text));
		MelonLogger.Msg($"[KGT/Buy] {caller.PlayerName} bought {display} ({resolvedName}) for {cost}cr. Spawned at ({val3.x:F0},{val3.z:F0}).");
	}

	private static int CountUnitsInCategoryForFaction(string builtAt, string faction)
	{
		if (string.Equals(builtAt, "Buildings", StringComparison.OrdinalIgnoreCase))
		{
			return CountStructuresForFaction(faction);
		}
		int num = 0;
		foreach (KeyValuePair<string, string> item in _unitFaction)
		{
			if (string.Equals(item.Value, faction, StringComparison.OrdinalIgnoreCase) && !_isStructure.Contains(item.Key) && _unitToBuilding.TryGetValue(item.Key, out string value) && string.Equals(value, builtAt, StringComparison.OrdinalIgnoreCase) && _unitCost.TryGetValue(item.Key, out var value2) && value2 > 0 && CanResolveUnit(item.Key))
			{
				num++;
			}
		}
		return num;
	}

	private static int CountStructuresForFaction(string faction)
	{
		int num = 0;
		foreach (StructRow structRow in _structRows)
		{
			if (string.Equals(structRow.Faction, faction, StringComparison.OrdinalIgnoreCase) && (!string.IsNullOrEmpty(structRow.Prefab) || !string.IsNullOrEmpty(structRow.ObjectInfo)) && _unitCost.TryGetValue(structRow.Display, out var value) && value > 0 && CanResolveBuyChoice(new BuyChoice
			{
				Prefab = structRow.Prefab,
				ObjectInfoName = structRow.ObjectInfo
			}))
			{
				num++;
			}
		}
		return num;
	}

	private static bool IsTooCloseToEnemyCritical(Player player, Vector3 spawnPos, out string blocker, out float distance)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		blocker = "";
		distance = float.PositiveInfinity;
		float buyMinDistanceFromEnemyCritical = Cfg.BuyMinDistanceFromEnemyCritical;
		if (buyMinDistanceFromEnemyCritical <= 0f)
		{
			return false;
		}
		Team val = ((player != null) ? player.Team : null);
		try
		{
			foreach (Team team in Team.Teams)
			{
				if ((Object)(object)team == (Object)null || team == val || IsGamemasterTeam(team))
				{
					continue;
				}
				List<Structure> structures = team.Structures;
				if (structures != null)
				{
					for (int i = 0; i < structures.Count; i++)
					{
						Structure val2 = structures[i];
						if (!((Object)(object)val2 == (Object)null) && !((BaseGameObject)val2).IsDestroyed && !((Object)(object)((BaseGameObject)val2).ObjectInfo == (Object)null) && ((BaseGameObject)val2).ObjectInfo.Critical)
						{
							float num = HorizontalDistance(spawnPos, ((Component)val2).transform.position);
							if (num < buyMinDistanceFromEnemyCritical && num < distance)
							{
								distance = num;
								blocker = (((BaseGameObject)val2).ObjectInfo.DisplayName ?? ((Object)val2).name) + " (" + SafeTeamName(team) + ")";
							}
						}
					}
				}
				List<Unit> units = team.Units;
				if (units == null)
				{
					continue;
				}
				for (int j = 0; j < units.Count; j++)
				{
					Unit val3 = units[j];
					if (!((Object)(object)val3 == (Object)null) && !((BaseGameObject)val3).IsDestroyed && !((Object)(object)((BaseGameObject)val3).ObjectInfo == (Object)null) && ((BaseGameObject)val3).ObjectInfo.Critical)
					{
						float num2 = HorizontalDistance(spawnPos, ((Component)val3).transform.position);
						if (num2 < buyMinDistanceFromEnemyCritical && num2 < distance)
						{
							distance = num2;
							blocker = (((BaseGameObject)val3).ObjectInfo.DisplayName ?? ((Object)val3).name) + " (" + SafeTeamName(team) + ")";
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT/Buy] IsTooCloseToEnemyCritical threw: " + ex.Message);
		}
		return !string.IsNullOrEmpty(blocker);
	}

	private static bool IsTooFarFromOwnCritical(Player player, Vector3 spawnPos, out float distance)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		distance = float.PositiveInfinity;
		float buyMaxDistanceFromOwnCritical = Cfg.BuyMaxDistanceFromOwnCritical;
		if (buyMaxDistanceFromOwnCritical <= 0f)
		{
			return false;
		}
		Team val = ((player != null) ? player.Team : null);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		int num = 0;
		try
		{
			List<Structure> structures = val.Structures;
			if (structures != null)
			{
				for (int i = 0; i < structures.Count; i++)
				{
					Structure val2 = structures[i];
					if (!((Object)(object)val2 == (Object)null) && !((BaseGameObject)val2).IsDestroyed && !((Object)(object)((BaseGameObject)val2).ObjectInfo == (Object)null) && ((BaseGameObject)val2).ObjectInfo.Critical)
					{
						num++;
						float num2 = HorizontalDistance(spawnPos, ((Component)val2).transform.position);
						if (num2 < distance)
						{
							distance = num2;
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT/Buy] IsTooFarFromOwnCritical threw: " + ex.Message);
			return false;
		}
		if (num == 0)
		{
			MelonLogger.Msg("[KGT/Buy] " + player?.PlayerName + " has no own HQ/Nest to anchor the buy radius — allowing purchase.");
			return false;
		}
		return distance > buyMaxDistanceFromOwnCritical;
	}

	private static float HorizontalDistance(Vector3 a, Vector3 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		float num = a.x - b.x;
		float num2 = a.z - b.z;
		return Mathf.Sqrt(num * num + num2 * num2);
	}

	private static string DumpFactionForTeam(Team team)
	{
		string text = ((team != null) ? team.GetTeamShortName() : null) ?? "";
		if (text.Equals("Cent", StringComparison.OrdinalIgnoreCase))
		{
			return "Centauri";
		}
		if (text.Equals("Sol", StringComparison.OrdinalIgnoreCase))
		{
			return "Sol";
		}
		if (text.Equals("Alien", StringComparison.OrdinalIgnoreCase))
		{
			return "Alien";
		}
		return text;
	}

	private static int CategoryOrder(string builtAt)
	{
		return builtAt switch
		{
			"Barracks" => 1, 
			"Light Factory" => 2, 
			"Heavy Factory" => 3, 
			"Ultra Heavy Factory" => 4, 
			"Air Factory" => 5, 
			"Lesser Spawning Cyst" => 11, 
			"Greater Spawning Cyst" => 12, 
			"Grand Spawning Cyst" => 13, 
			"Colossal Spawning Cyst" => 14, 
			_ => 99, 
		};
	}

	private static void ResetBuyState()
	{
		_buyStates.Clear();
		_lastPurchaseTime.Clear();
		_prefabIndexBuilt = false;
	}

	private static bool IsUnitBalanceEditorOpen(long steamId)
	{
		if (!_ubProbed)
		{
			_ubProbed = true;
			try
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (int i = 0; i < assemblies.Length; i++)
				{
					Type type = assemblies[i].GetType("Si_UnitBalance.Si_UnitBalance");
					if (!(type == null))
					{
						_ubMenuStatesField = type.GetField("_menuStates", BindingFlags.Static | BindingFlags.NonPublic);
						MelonLogger.Msg($"[KGT/Buy] Si_UnitBalanceUI integration: _menuStates={_ubMenuStatesField != null}");
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[KGT/Buy] UnitBalanceUI probe failed: " + ex.Message);
			}
		}
		if (_ubMenuStatesField == null)
		{
			return false;
		}
		try
		{
			object value = _ubMenuStatesField.GetValue(null);
			if (value == null)
			{
				return false;
			}
			object obj = value.GetType().GetMethod("ContainsKey", new Type[1] { typeof(long) })?.Invoke(value, new object[1] { steamId });
			bool flag = default;
			int num;
			if (obj is bool)
			{
				flag = (bool)obj;
				num = 1;
			}
			else
			{
				num = 0;
			}
			return (byte)((uint)num & (flag ? 1u : 0u)) != 0;
		}
		catch
		{
			return false;
		}
	}

	private static void OnKohCommand(Player? caller, string args)
	{
		string text = "";
		string text2 = "";
		string[] array = (args ?? "").Trim().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length > 1)
		{
			text = array[1].ToLowerInvariant();
		}
		if (array.Length > 2)
		{
			text2 = array[2];
		}
		if (string.IsNullOrEmpty(text) || text == "status")
		{
			ShowStatus(caller);
			return;
		}
		if (!((Object)(object)caller == (Object)null) && !caller.CanAdminExecute(Power.Generic))
		{
			Reply(caller, "[KGT] Admin only.");
			return;
		}
		switch (text)
		{
		case "on":
			Cfg.Enabled = true;
			SaveConfig();
			Reply(caller, "[KGT] ON");
			break;
		case "off":
			Cfg.Enabled = false;
			SaveConfig();
			Reply(caller, "[KGT] OFF");
			break;
		case "radius":
		{
			if (TryParseFloat(text2, out var val2) && val2 > 0f)
			{
				Cfg.CaptureRadius = val2;
				_captureRadius = val2;
				SaveConfig();
				Reply(caller, $"[KGT] capture radius = {val2}m (applied live)");
			}
			else
			{
				Reply(caller, $"[KGT] capture radius = {_captureRadius}m (Cfg default = {Cfg.CaptureRadius})");
			}
			break;
		}
		case "threshold":
		{
			if (TryParseFloat(text2, out var val) && val > 0f)
			{
				Cfg.WinThreshold = val;
				_winThreshold = val;
				SaveConfig();
				Reply(caller, $"[KGT] win threshold = {val} pts (applied live)");
			}
			else
			{
				Reply(caller, $"[KGT] win threshold = {_winThreshold} pts (Cfg default = {Cfg.WinThreshold})");
			}
			break;
		}
		case "rate":
		{
			if (TryParseFloat(text2, out var val3) && val3 > 0f)
			{
				Cfg.MaxCapturePointsPerSecond = val3;
				SaveConfig();
				Reply(caller, $"[KGT] max capture rate = {val3} pts/s");
			}
			else
			{
				Reply(caller, $"[KGT] max capture rate = {Cfg.MaxCapturePointsPerSecond} pts/s");
			}
			break;
		}
		case "reset":
			ResetKingState();
			Reply(caller, "[KGT] State cleared (king + accumulation + milestones).");
			break;
		case "mode":
		{
			if (int.TryParse(text2, out var result) && result >= 1 && result <= 3)
			{
				Cfg.Mode = result;
				SaveConfig();
				Reply(caller, $"[KGT] Mode = {result} ({ModeLabel(result)}). Takes full effect at next round start.");
			}
			else
			{
				Reply(caller, $"[KGT] Mode = {Cfg.Mode} ({ModeLabel(Cfg.Mode)}). Usage: /koh mode <1|2|3>  (1=Full, 2=BuyOnly, 3=Disabled)");
			}
			break;
		}
		case "starter":
		{
			if (int.TryParse(text2, out var result2) && result2 >= 0)
			{
				Cfg.StarterCreditsPerPlayer = result2;
				SaveConfig();
				Reply(caller, $"[KGT] Starter credits = {result2}cr per player per round.");
			}
			else
			{
				Reply(caller, $"[KGT] Starter credits = {Cfg.StarterCreditsPerPlayer}cr per player per round. Usage: /koh starter <amount>");
			}
			break;
		}
		case "bounty":
			HandleBountySubcommand(caller, text2, array);
			break;
		case "buildingreward":
		case "structurereward":
		{
			bool? flag2 = ParseOnOff(text2);
			if (!flag2.HasValue)
			{
				Reply(caller, "[KGT] Building-kill reward = " + (Cfg.StructureKillRewardsEnabled ? "ON" : "OFF") + ". Usage: /koh buildingreward <on|off>");
				break;
			}
			Cfg.StructureKillRewardsEnabled = flag2.Value;
			SaveConfig();
			Reply(caller, "[KGT] Building-kill reward = " + (flag2.Value ? "ON" : "OFF") + ". " + (flag2.Value ? "Destroying enemy structures now pays the killer credits (cost × RewardKillFraction)." : "Structure destruction no longer pays the killer. Unit kills still pay."));
			break;
		}
		case "buystructures":
		case "buybuildings":
		{
			bool? flag = ParseOnOff(text2);
			if (!flag.HasValue)
			{
				Reply(caller, "[KGT] /buy structures = " + (Cfg.BuyStructuresEnabled ? "ON" : "OFF") + ". Usage: /koh buystructures <on|off>");
				break;
			}
			Cfg.BuyStructuresEnabled = flag.Value;
			SaveConfig();
			Reply(caller, "[KGT] /buy structures = " + (flag.Value ? "ON" : "OFF") + ". " + (flag.Value ? "A 'Buildings' category now appears at the end of the /buy menu." : "Structures are hidden from the /buy menu again."));
			break;
		}
		default:
			Reply(caller, "/koh [status|on|off|radius <m>|threshold <pts>|rate <pts/s>|reset|mode <1-3>|starter <amount>|bounty (on|off|list|add|remove|reload)|buildingreward <on|off>|buystructures <on|off>]");
			break;
		}
	}

	private static void ShowStatus(Player? caller)
	{
		string text;
		if ((Object)(object)_currentKing == (Object)null)
		{
			text = "No king (zone empty or contested)";
		}
		else
		{
			float currentProgress = GetCurrentProgress(_currentKing);
			float pct = ((_winThreshold > 0f) ? (currentProgress / _winThreshold * 100f) : 0f);
			text = $"King: {ColoredTeamName(_currentKing)}  {ColoredPercent(pct)}  ({currentProgress:F0}/{_winThreshold:F0})";
		}
		Reply(caller, string.Format("[KGT] {0} mode={1}({2}) | r={3}m excl={4}m | {5}", Cfg.Enabled ? "ON" : "OFF", Cfg.Mode, ModeLabel(Cfg.Mode), Cfg.CaptureRadius, Cfg.BuildExclusionRadius, text));
		float lastTickGain = _lastTickGain;
		float lastTickMaxAllowed = _lastTickMaxAllowed;
		float lastTickKingScore = _lastTickKingScore;
		float lastTickTotalScore = _lastTickTotalScore;
		string text2;
		if (lastTickMaxAllowed > 0f && lastTickGain >= lastTickMaxAllowed)
		{
			text2 = "CAPPED";
		}
		else
		{
			text2 = ((lastTickGain > 0f) ? "below cap" : "no gain");
		}
		Reply(caller, $"[KGT] Last tick: gain={lastTickGain:F1}/{lastTickMaxAllowed:F1} ({text2}) | king-score={lastTickKingScore:F1} of total {lastTickTotalScore:F1} in zone");
		if (!Cfg.HandicapEnabled)
		{
			return;
		}
		if (_handicapMult.Count == 0)
		{
			Reply(caller, "[KGT] Handicap: <i>computing…</i>");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder("[KGT] Handicap (×CommReward):");
		bool flag = true;
		foreach (KeyValuePair<Team, float> item in _handicapMult)
		{
			stringBuilder.Append(flag ? " " : " · ");
			flag = false;
			string arg;
			if (item.Value > 1.05f)
			{
				arg = "#39ff14";
			}
			else
			{
				arg = ((item.Value < 0.95f) ? "#ff5050" : "#cccccc");
			}
			stringBuilder.Append($"{ColoredTeamName(item.Key)} <color={arg}>×{item.Value:F2}</color>");
		}
		Reply(caller, stringBuilder.ToString());
	}

	private static void Reply(Player? caller, string msg)
	{
		if ((Object)(object)caller != (Object)null)
		{
			HelperMethods.SendChatMessageToPlayer(caller, msg);
		}
		MelonLogger.Msg(msg);
	}

	private static bool? ParseOnOff(string arg)
	{
		if (string.IsNullOrWhiteSpace(arg))
		{
			return null;
		}
		switch (arg.Trim().ToLowerInvariant())
		{
		case "on":
		case "1":
		case "true":
		case "yes":
		case "y":
			return true;
		case "off":
		case "0":
		case "false":
		case "no":
		case "n":
			return false;
		default:
			return null;
		}
	}

	private static void HandleBountySubcommand(Player? caller, string action, string[] parts)
	{
		action = (action ?? "").ToLowerInvariant();
		switch (action)
		{
		case "on":
		case "enable":
			Cfg.BountiesEnabled = true;
			SaveConfig();
			_announcedBounties.Clear();
			Reply(caller, "[KGT/Bounty] Bounties ENABLED.");
			break;
		case "off":
		case "disable":
			Cfg.BountiesEnabled = false;
			SaveConfig();
			Reply(caller, "[KGT/Bounty] Bounties DISABLED. (Entries kept; re-enable with /koh bounty on.)");
			break;
		case "list":
		case "":
		{
			string text = (Cfg.BountiesEnabled ? "<color=#39ff14>ON</color>" : "<color=#ff5050>OFF</color>");
			if (_bountyCfg.Bounties.Count == 0)
			{
				Reply(caller, "[KGT/Bounty] " + text + ". No bounties active.");
				break;
			}
			Reply(caller, $"[KGT/Bounty] {text} — {_bountyCfg.Bounties.Count} active (default {_bountyCfg.DefaultAmount}cr):");
			{
				foreach (KeyValuePair<long, BountyEntry> bounty in _bountyCfg.Bounties)
				{
					int num = ((bounty.Value.Amount > 0) ? bounty.Value.Amount : _bountyCfg.DefaultAmount);
					string arg = (string.IsNullOrEmpty(bounty.Value.Name) ? "(no name)" : bounty.Value.Name);
					Reply(caller, $"  {bounty.Key} — {arg} — {num}cr");
				}
				break;
			}
		}
		case "reload":
			LoadBountyConfig();
			_announcedBounties.Clear();
			Reply(caller, $"[KGT/Bounty] Reloaded. {_bountyCfg.Bounties.Count} active.");
			break;
		case "add":
		{
			if (parts.Length < 4 || !long.TryParse(parts[3], out var result2))
			{
				Reply(caller, "[KGT/Bounty] Usage: /koh bounty add <steamId> <amount> [name...]");
				break;
			}
			LoadBountyConfig();
			int num2 = _bountyCfg.DefaultAmount;
			int num3 = 4;
			if (parts.Length >= 5 && int.TryParse(parts[4], out var result3))
			{
				num2 = result3;
				num3 = 5;
			}
			string text2 = ((parts.Length > num3) ? string.Join(" ", parts, num3, parts.Length - num3) : "");
			string note = "";
			if (_bountyCfg.Bounties.TryGetValue(result2, out BountyEntry value))
			{
				if (string.IsNullOrEmpty(text2))
				{
					text2 = value.Name ?? "";
				}
				note = value.Note ?? "";
			}
			_bountyCfg.Bounties[result2] = new BountyEntry
			{
				Name = text2,
				Amount = num2,
				Note = note
			};
			SaveBountyConfig();
			_announcedBounties.Remove(result2);
			Reply(caller, string.Format("[KGT/Bounty] Added: {0} — {1} — {2}cr", result2, string.IsNullOrEmpty(text2) ? "(no name)" : text2, num2));
			break;
		}
		case "rm":
		case "remove":
		case "del":
		{
			if (parts.Length < 4 || !long.TryParse(parts[3], out var result))
			{
				Reply(caller, "[KGT/Bounty] Usage: /koh bounty remove <steamId>");
				break;
			}
			LoadBountyConfig();
			if (!_bountyCfg.Bounties.Remove(result))
			{
				Reply(caller, $"[KGT/Bounty] No entry for {result}.");
				break;
			}
			SaveBountyConfig();
			_announcedBounties.Remove(result);
			Reply(caller, $"[KGT/Bounty] Removed: {result}");
			break;
		}
		default:
			Reply(caller, "[KGT/Bounty] Usage: /koh bounty [on|off|list|reload|add <steamId> <amount> [name]|remove <steamId>]");
			break;
		}
	}

	private static string ModeLabel(int m)
	{
		return m switch
		{
			1 => "Full", 
			2 => "BuyOnly", 
			3 => "Disabled", 
			_ => "?", 
		};
	}

	private static void BroadcastAllChat(string msg)
	{
		try
		{
			Player serverPlayer = NetworkGameServer.GetServerPlayer();
			for (int i = 0; i < Player.Players.Count; i++)
			{
				Player val = Player.Players[i];
				if ((Object)(object)val != (Object)null && (Object)(object)val != (Object)(object)serverPlayer)
				{
					HelperMethods.SendChatMessageToPlayer(val, msg);
				}
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] BroadcastAllChat failed: " + ex.Message);
		}
		MelonLogger.Msg(msg);
	}

	private static bool TryParseFloat(string s, out float val)
	{
		return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out val);
	}

	private static void LoadConfig()
	{
		try
		{
			if (File.Exists(_configPath))
			{
				KohConfig kohConfig = JsonConvert.DeserializeObject<KohConfig>(File.ReadAllText(_configPath));
				if (kohConfig != null)
				{
					Cfg = kohConfig;
					MelonLogger.Msg("[KGT] Config loaded from " + _configPath);
					SaveConfig();
					return;
				}
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] Failed to load config: " + ex.Message + " — writing defaults.");
		}
		SaveConfig();
		MelonLogger.Msg("[KGT] Default config written to " + _configPath);
	}

	private static void SaveConfig()
	{
		try
		{
			string directoryName = Path.GetDirectoryName(_configPath);
			if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			File.WriteAllText(_configPath, JsonConvert.SerializeObject((object)Cfg, (Formatting)1));
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] Failed to save config: " + ex.Message);
		}
	}

	private static void ResetKingState()
	{
		_currentKing = null;
		_provisionalKing = null;
		_handoverElapsed = 0f;
		_accumulated.Clear();
		_totalAccumulated = 0f;
		_lastLoggedAccumulationBucket = -1;
		_lastTickGain = 0f;
		_lastTickKingScore = 0f;
		_lastTickTotalScore = 0f;
		_lastTickMaxAllowed = 0f;
		_milestonesPlayed.Clear();
		_milestonesPlayedCumulative.Clear();
		_firstCapturePlayed = false;
		_winTriggered = false;
		_statusAnnounceTimer = 0f;
		ResetTimerState();
	}

	private static void UpdateOwnership(Dictionary<Team, float> scores, float deltaSeconds)
	{
		if (!_hasKoh || _winTriggered)
		{
			return;
		}
		float num = 0f;
		foreach (float value2 in scores.Values)
		{
			num += value2;
		}
		Team val = null;
		if (num > 0f)
		{
			float num2 = num * Cfg.DominanceThreshold;
			foreach (KeyValuePair<Team, float> score in scores)
			{
				if (score.Value >= num2)
				{
					val = score.Key;
					break;
				}
			}
		}
		if ((Object)(object)val == (Object)null)
		{
			_provisionalKing = null;
			_handoverElapsed = 0f;
		}
		else if (val == _currentKing)
		{
			_provisionalKing = null;
			_handoverElapsed = 0f;
		}
		else if (val == _provisionalKing)
		{
			_handoverElapsed += deltaSeconds;
			if (_handoverElapsed >= Cfg.OwnershipHandoverDelay)
			{
				CommitNewKing(val);
			}
		}
		else
		{
			_provisionalKing = val;
			_handoverElapsed = 0f;
			if (Cfg.OwnershipHandoverDelay <= 0f)
			{
				CommitNewKing(val);
			}
		}
		_lastTickTotalScore = num;
		_lastTickMaxAllowed = Cfg.MaxCapturePointsPerSecond * deltaSeconds;
		if ((Object)(object)_currentKing != (Object)null && _currentKing == val)
		{
			float num3 = scores[_currentKing];
			float num4 = Cfg.MaxCapturePointsPerSecond * deltaSeconds;
			float num5 = Mathf.Min(num3, num4);
			_lastTickKingScore = num3;
			_lastTickGain = num5;
			if (!_firstCapturePlayed)
			{
				_firstCapturePlayed = true;
				PlaySound(Cfg.SoundFirstCapture);
			}
			if (Cfg.CumulativeAccumulation)
			{
				float totalAccumulated = _totalAccumulated;
				_totalAccumulated = totalAccumulated + num5;
				int num6 = (int)(_totalAccumulated / 1000f);
				if (num6 != _lastLoggedAccumulationBucket)
				{
					_lastLoggedAccumulationBucket = num6;
					MelonLogger.Msg($"[KGT/DBG] accumulation: this-tick gain={num5:F1}  (cap={num4:F1}, raw kingScore={num3:F1})  total={_totalAccumulated:F0}/{_winThreshold:F0}");
				}
				CheckMilestonesCumulative(_currentKing, totalAccumulated, _totalAccumulated);
			}
			else
			{
				if (!_accumulated.TryGetValue(_currentKing, out var value))
				{
					value = 0f;
				}
				float num7 = value + num5;
				_accumulated[_currentKing] = num7;
				CheckMilestones(_currentKing, value, num7);
			}
		}
		else
		{
			_lastTickGain = 0f;
			_lastTickKingScore = 0f;
		}
	}

	private static float GetCurrentProgress(Team king)
	{
		if (Cfg.CumulativeAccumulation)
		{
			return _totalAccumulated;
		}
		if (!_accumulated.TryGetValue(king, out var value))
		{
			return 0f;
		}
		return value;
	}

	private static void CheckMilestonesCumulative(Team king, float prev, float now)
	{
		float winThreshold = _winThreshold;
		if (!(winThreshold <= 0f))
		{
			if (now >= winThreshold)
			{
				TriggerWin(king);
				return;
			}
			TryMilestoneCumulative(25, Cfg.SoundMilestone25, prev, now, winThreshold, king);
			TryMilestoneCumulative(50, Cfg.SoundMilestone50, prev, now, winThreshold, king);
			TryMilestoneCumulative(75, Cfg.SoundMilestone75, prev, now, winThreshold, king);
			TryMilestoneCumulative(95, Cfg.SoundMilestone95, prev, now, winThreshold, king);
			UpdateOutpostClock(king, now / winThreshold);
		}
	}

	private static void TryMilestoneCumulative(int percent, string sound, float prev, float now, float threshold, Team king)
	{
		float num = threshold * ((float)percent / 100f);
		if (!(prev >= num) && !(now < num) && _milestonesPlayedCumulative.Add(percent))
		{
			BroadcastMilestoneChat(king, percent);
			PlaySound(sound);
			LogToReplay("kgt_progress", ("team", ReplayTeamTag(king)), ("pct", percent.ToString()), ("accumulated", now.ToString("F0")), ("threshold", threshold.ToString("F0")));
		}
	}

	private static void BroadcastMilestoneChat(Team king, int percent)
	{
		string text = ColoredTeamName(king);
		string text2 = ColoredPercent(percent);
		BroadcastAllChat(percent switch
		{
			25 => "[KGT] " + text + " have claimed a quarter of the hill — " + text2 + ".", 
			50 => "[KGT] " + text + " hold half the prize — " + text2 + ".", 
			75 => "[KGT] " + text + " command three quarters of the hill — " + text2 + ".", 
			95 => "[KGT] " + text + " stand a hair's breadth from triumph — " + text2 + ".", 
			_ => "[KGT] " + text + " hold " + text2 + " of the hill.", 
		});
	}

	private static void CommitNewKing(Team newKing)
	{
		Team currentKing = _currentKing;
		_currentKing = newKing;
		_provisionalKing = null;
		_handoverElapsed = 0f;
		string text = ColoredTeamName(newKing);
		if ((Object)(object)currentKing == (Object)null)
		{
			BroadcastAllChat("[KGT] The contest for the King of the Galactic Teleporter has commenced — " + text + " have asserted their claim.");
		}
		else
		{
			BroadcastAllChat("[KGT] " + text + " have wrested the Galactic Teleporter from " + ColoredTeamName(currentKing) + ".");
		}
		float num = ((_winThreshold > 0f) ? (GetCurrentProgress(newKing) / _winThreshold) : 0f);
		UpdateOutpostClock(newKing, num);
		LogToReplay("kgt_king_change", ("team", ReplayTeamTag(newKing)), ("previous_team", ((Object)(object)currentKing == (Object)null) ? "none" : ReplayTeamTag(currentKing)), ("progress_pct", (num * 100f).ToString("F0")));
	}

	private static void CheckMilestones(Team king, float prev, float now)
	{
		float winThreshold = _winThreshold;
		if (!(winThreshold <= 0f))
		{
			if (now >= winThreshold)
			{
				TriggerWin(king);
				return;
			}
			TryMilestone(king, 25, Cfg.SoundMilestone25, prev, now, winThreshold);
			TryMilestone(king, 50, Cfg.SoundMilestone50, prev, now, winThreshold);
			TryMilestone(king, 75, Cfg.SoundMilestone75, prev, now, winThreshold);
			TryMilestone(king, 95, Cfg.SoundMilestone95, prev, now, winThreshold);
		}
	}

	private static void TryMilestone(Team king, int percent, string sound, float prev, float now, float threshold)
	{
		float num = threshold * ((float)percent / 100f);
		if (!(prev >= num) && !(now < num))
		{
			if (!_milestonesPlayed.TryGetValue(percent, out HashSet<Team> value))
			{
				value = new HashSet<Team>();
				_milestonesPlayed[percent] = value;
			}
			if (value.Add(king))
			{
				BroadcastMilestoneChat(king, percent);
				PlaySound(sound);
			}
		}
	}

	private static string SafeTeamName(Team t)
	{
		try
		{
			string teamShortName = t.GetTeamShortName();
			if (!string.IsNullOrEmpty(teamShortName))
			{
				return teamShortName;
			}
			string teamName = t.GetTeamName();
			if (!string.IsNullOrEmpty(teamName))
			{
				return teamName;
			}
			return ((Object)t).name ?? "?";
		}
		catch
		{
			return "?";
		}
	}

	private static string GetTeamColor(Team t)
	{
		try
		{
			string text = t.GetTeamShortName() ?? "";
			if (text.IndexOf("Sol", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "#328cff";
			}
			if (text.IndexOf("Cent", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "#eb4646";
			}
			if (text.IndexOf("Alien", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "#50c832";
			}
			if (text.IndexOf("Worm", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "#c8963c";
			}
			if (text.IndexOf("Wild", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "#c8963c";
			}
		}
		catch
		{
		}
		return "#cccccc";
	}

	private static string ColoredTeamName(Team t)
	{
		return "<color=" + GetTeamColor(t) + ">" + SafeTeamName(t) + "</color>";
	}

	private static string ColoredPercent(float pct)
	{
		return string.Format("<color={0}>{1:F0}%</color>", "#39ff14", pct);
	}

	private static string ColoredPercent(int pct)
	{
		return string.Format("<color={0}>{1}%</color>", "#39ff14", pct);
	}

	private static void ResolvePrintLogLine()
	{
		if (_printLogLineResolved)
		{
			return;
		}
		_printLogLineResolved = true;
		try
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Type type = assemblies[i].GetType("Si_Logging.HL_Logging");
				if (!(type == null))
				{
					_printLogLineMethod = type.GetMethod("PrintLogLine", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
					{
						typeof(string),
						typeof(bool)
					}, null);
					break;
				}
			}
			if (_printLogLineMethod != null)
			{
				MelonLogger.Msg("[KGT/Replay] HL_Logging.PrintLogLine resolved — MapReplay events enabled");
			}
			else
			{
				MelonLogger.Msg("[KGT/Replay] Si_Logging.HL_Logging not loaded — MapReplay events skipped");
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT/Replay] ResolvePrintLogLine failed: " + ex.Message);
		}
	}

	private static void LogToReplay(string eventName, params (string Key, object Value)[] kv)
	{
		ResolvePrintLogLine();
		if (_printLogLineMethod == null)
		{
			return;
		}
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("World triggered \"").Append(eventName).Append("\"");
			if (kv != null)
			{
				for (int i = 0; i < kv.Length; i++)
				{
					(string, object) tuple = kv[i];
					if (!string.IsNullOrEmpty(tuple.Item1))
					{
						stringBuilder.Append(" (").Append(tuple.Item1).Append(" \"")
							.Append(tuple.Item2?.ToString() ?? "")
							.Append("\")");
					}
				}
			}
			_printLogLineMethod.Invoke(null, new object[2]
			{
				stringBuilder.ToString(),
				true
			});
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT/Replay] LogToReplay('" + eventName + "') failed: " + ex.Message);
		}
	}

	private static string ReplayTeamTag(Team t)
	{
		if ((Object)(object)t == (Object)null)
		{
			return "none";
		}
		try
		{
			string teamShortName = t.GetTeamShortName();
			if (!string.IsNullOrEmpty(teamShortName))
			{
				return teamShortName;
			}
			return ((Object)t).name ?? "?";
		}
		catch
		{
			return "?";
		}
	}

	private static void LogReplayTeamEvent(string teamName, string eventName, params (string Key, object Value)[] kv)
	{
		ResolvePrintLogLine();
		if (_printLogLineMethod == null)
		{
			return;
		}
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Team \"").Append(teamName).Append("\" triggered \"")
				.Append(eventName)
				.Append("\"");
			if (kv != null)
			{
				for (int i = 0; i < kv.Length; i++)
				{
					(string, object) tuple = kv[i];
					if (!string.IsNullOrEmpty(tuple.Item1))
					{
						stringBuilder.Append(" (").Append(tuple.Item1).Append(" \"")
							.Append(tuple.Item2?.ToString() ?? "")
							.Append("\")");
					}
				}
			}
			_printLogLineMethod.Invoke(null, new object[2]
			{
				stringBuilder.ToString(),
				true
			});
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT/Replay] LogReplayTeamEvent('" + eventName + "') failed: " + ex.Message);
		}
	}

	private static void LogReplayConstructionComplete(string teamName, string buildingName, Vector3 pos)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		LogReplayTeamEvent(teamName, "construction_complete", ("building_name", buildingName), ("building_position", $"{pos.x:F0} {pos.z:F0} {pos.y:F0}"));
	}

	private static void LogReplayStructureKill(string buildingName, string buildingTeam, Vector3 pos)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		ResolvePrintLogLine();
		if (_printLogLineMethod == null)
		{
			return;
		}
		try
		{
			string text = $"{pos.x:F0} {pos.z:F0} {pos.y:F0}";
			string text2 = "\"KGT<0><BOT:KGT><KGT>\" triggered \"structure_kill\" (structure \"" + buildingName + "\") (weapon \"kgt_clock\") (struct_team \"" + buildingTeam + "\") (attacker_position \"" + text + "\") (building_position \"" + text + "\")";
			_printLogLineMethod.Invoke(null, new object[2] { text2, true });
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT/Replay] LogReplayStructureKill failed: " + ex.Message);
		}
	}

	private static void SpawnOutpostsSplit(Team? kingTeam, int kingCount)
	{
		DespawnOutposts();
		if (!_hasKoh || (Object)(object)_kohTower == (Object)null)
		{
			return;
		}
		if ((Object)(object)_neutralTeam == (Object)null)
		{
			try
			{
				BaseGameObject component = ((Component)_kohTower).GetComponent<BaseGameObject>();
				_neutralTeam = ((component != null) ? component.Team : null);
			}
			catch
			{
			}
		}
		if ((Object)(object)_neutralTeam == (Object)null)
		{
			MelonLogger.Warning("[KGT] SpawnOutpostsSplit: neutral team unresolved — skipping ring");
			return;
		}
		int num = Mathf.Max(1, Cfg.OutpostCount);
		int num2 = Mathf.Clamp(kingCount, 0, num);
		int spawnablePrefabIndex;
		try
		{
			spawnablePrefabIndex = GameDatabase.GetSpawnablePrefabIndex("Outpost");
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] GetSpawnablePrefabIndex threw: " + ex.Message);
			return;
		}
		if (spawnablePrefabIndex < 0)
		{
			MelonLogger.Warning("[KGT] Outpost prefab 'Outpost' not found");
			return;
		}
		GameObject spawnablePrefab = GameDatabase.GetSpawnablePrefab(spawnablePrefabIndex);
		if ((Object)(object)spawnablePrefab == (Object)null)
		{
			return;
		}
		Terrain activeTerrain = Terrain.activeTerrain;
		float outpostBuryDepth = Cfg.OutpostBuryDepth;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < num; i++)
		{
			Team team = ((i < num2 && (Object)(object)kingTeam != (Object)null) ? kingTeam : _neutralTeam);
			if (SpawnSlot(i, team, spawnablePrefab, activeTerrain, outpostBuryDepth))
			{
				if (i < num2)
				{
					num3++;
				}
				else
				{
					num4++;
				}
			}
		}
		_outpostKingTeam = kingTeam;
		_outpostKingCount = num2;
		MelonLogger.Msg($"[KGT] Outpost clock: {num3} on {SafeTeamName(kingTeam ?? _neutralTeam)} / {num4} on {SafeTeamName(_neutralTeam)}");
	}

	private static bool SpawnSlot(int i, Team team, GameObject prefab, Terrain terrain, float buryDepth)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Max(1, Cfg.OutpostCount);
		float num2 = (float)i / (float)num * 2f * MathF.PI;
		float num3 = _kohCenter.x + _captureRadius * Mathf.Cos(num2);
		float num4 = _kohCenter.z + _captureRadius * Mathf.Sin(num2);
		float num5 = SampleSurfaceY(terrain, num3, num4) - buryDepth;
		Vector3 val = new Vector3(num3, num5, num4);
		try
		{
			GameObject val2 = Game.SpawnPrefab(prefab, (Player)null, team, val, Quaternion.identity, true, true);
			if ((Object)(object)val2 == (Object)null)
			{
				return false;
			}
			while (_outposts.Count <= i)
			{
				_outposts.Add(null);
				_outpostTeamNames.Add("");
				_outpostPositions.Add(Vector3.zero);
				_outpostSlotTeams.Add(null);
			}
			_outposts[i] = val2;
			string text = ReplayTeamTag(team);
			_outpostTeamNames[i] = text;
			_outpostPositions[i] = val;
			_outpostSlotTeams[i] = team;
			LogReplayConstructionComplete(text, "Outpost", val);
			return true;
		}
		catch (Exception ex)
		{
			MelonLogger.Warning($"[KGT] Outpost spawn at slot {i} ({num3:F0},{num4:F0}) failed: {ex.Message}");
			return false;
		}
	}

	private static void DespawnSlot(int i)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (i < 0 || i >= _outposts.Count)
		{
			return;
		}
		if (i < _outpostPositions.Count && i < _outpostTeamNames.Count && !string.IsNullOrEmpty(_outpostTeamNames[i]))
		{
			LogReplayStructureKill("Outpost", _outpostTeamNames[i], _outpostPositions[i]);
		}
		GameObject val = _outposts[i];
		if ((Object)(object)val != (Object)null)
		{
			try
			{
				DamageManager component = val.GetComponent<DamageManager>();
				if ((Object)(object)component != (Object)null)
				{
					component.SetHealth01(0f);
				}
				else
				{
					Object.Destroy((Object)(object)val);
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning($"[KGT] Slot {i} despawn failed: {ex.Message}");
			}
		}
		_outposts[i] = null;
		if (i < _outpostTeamNames.Count)
		{
			_outpostTeamNames[i] = "";
		}
		if (i < _outpostSlotTeams.Count)
		{
			_outpostSlotTeams[i] = null;
		}
	}

	private static void UpdateOutpostClock(Team? king, float progressFraction)
	{
		if (!_hasKoh)
		{
			return;
		}
		int num = Mathf.Max(1, Cfg.OutpostCount);
		int num2 = Mathf.Clamp(Mathf.RoundToInt((float)num * progressFraction), 0, num);
		if (num2 == _outpostKingCount && king == _outpostKingTeam)
		{
			return;
		}
		if (_outposts.Count == 0)
		{
			SpawnOutpostsSplit(king, num2);
			return;
		}
		if ((Object)(object)_neutralTeam == (Object)null && (Object)(object)_kohTower != (Object)null)
		{
			try
			{
				BaseGameObject component = ((Component)_kohTower).GetComponent<BaseGameObject>();
				_neutralTeam = ((component != null) ? component.Team : null);
			}
			catch
			{
			}
		}
		if ((Object)(object)_neutralTeam == (Object)null)
		{
			SpawnOutpostsSplit(king, num2);
			return;
		}
		int spawnablePrefabIndex;
		try
		{
			spawnablePrefabIndex = GameDatabase.GetSpawnablePrefabIndex("Outpost");
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] GetSpawnablePrefabIndex threw: " + ex.Message);
			return;
		}
		if (spawnablePrefabIndex < 0)
		{
			return;
		}
		GameObject spawnablePrefab = GameDatabase.GetSpawnablePrefab(spawnablePrefabIndex);
		if ((Object)(object)spawnablePrefab == (Object)null)
		{
			return;
		}
		Terrain activeTerrain = Terrain.activeTerrain;
		float outpostBuryDepth = Cfg.OutpostBuryDepth;
		int num3 = 0;
		for (int i = 0; i < num; i++)
		{
			Team val = ((i < num2 && (Object)(object)king != (Object)null) ? king : _neutralTeam);
			if (((i < _outpostSlotTeams.Count) ? _outpostSlotTeams[i] : null) != val)
			{
				DespawnSlot(i);
				SpawnSlot(i, val, spawnablePrefab, activeTerrain, outpostBuryDepth);
				num3++;
			}
		}
		_outpostKingTeam = king;
		_outpostKingCount = num2;
		if (num3 > 0)
		{
			MelonLogger.Msg($"[KGT] Outpost clock incremental: flipped {num3}/{num} slot(s)");
		}
	}

	private static void DespawnOutposts()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		int count = _outposts.Count;
		for (int i = 0; i < _outposts.Count; i++)
		{
			GameObject val = _outposts[i];
			if (i < _outpostPositions.Count && i < _outpostTeamNames.Count)
			{
				LogReplayStructureKill("Outpost", _outpostTeamNames[i], _outpostPositions[i]);
			}
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			try
			{
				DamageManager component = val.GetComponent<DamageManager>();
				if ((Object)(object)component != (Object)null)
				{
					component.SetHealth01(0f);
				}
				else
				{
					Object.Destroy((Object)(object)val);
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[KGT] Outpost despawn failed: " + ex.Message);
			}
		}
		if (count > 0)
		{
			MelonLogger.Msg($"[KGT] Despawned {count} outpost(s).");
		}
		_outposts.Clear();
		_outpostTeamNames.Clear();
		_outpostPositions.Clear();
		_outpostSlotTeams.Clear();
		_outpostKingTeam = null;
		_outpostKingCount = 0;
	}

	private static float SampleTerrainYLocal(Terrain terrain, float x, float z)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)terrain == (Object)null)
		{
			return 0f;
		}
		try
		{
			return terrain.SampleHeight(new Vector3(x, 0f, z)) + ((Component)terrain).transform.position.y;
		}
		catch
		{
			return 0f;
		}
	}

	private static float SampleSurfaceY_NearAnchor(Terrain terrain, float x, float z, float anchorY)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		float num = anchorY + 10f;
		try
		{
			RaycastHit val = default;
			if (Physics.Raycast(new Vector3(x, num, z), Vector3.down, ref val, 60f, -1, (QueryTriggerInteraction)1))
			{
				return val.point.y;
			}
		}
		catch
		{
		}
		float num2 = SampleTerrainYLocal(terrain, x, z);
		if (Mathf.Abs(num2 - anchorY) > 50f)
		{
			return anchorY;
		}
		return num2;
	}

	private static float SampleSurfaceY(Terrain terrain, float x, float z)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		float num = SampleTerrainYLocal(terrain, x, z);
		float num2 = num + 200f;
		try
		{
			RaycastHit val = default;
			if (Physics.Raycast(new Vector3(x, num2, z), Vector3.down, ref val, 400f, -1, (QueryTriggerInteraction)1))
			{
				return val.point.y;
			}
		}
		catch
		{
		}
		return num;
	}

	private static void InstallDamagePatch(Harmony harmony)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected Obj, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Expected Obj, but got Unknown
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Expected Obj, but got Unknown
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Expected Obj, but got Unknown
		if (_damagePatchInstalled)
		{
			return;
		}
		try
		{
			if (_damageManagerType == null)
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (int i = 0; i < assemblies.Length; i++)
				{
					Type type = assemblies[i].GetType("DamageManager");
					if (type != null)
					{
						_damageManagerType = type;
						break;
					}
				}
			}
			if (_damageManagerType == null)
			{
				MelonLogger.Warning("[KGT] DamageManager type not found — damage immunity patch deferred");
				return;
			}
			_applyDamageMethod = _damageManagerType.GetMethod("ApplyDamage", BindingFlags.Instance | BindingFlags.Public);
			if (_applyDamageMethod != null)
			{
				MethodInfo method = typeof(KingOfTheHill).GetMethod("Prefix_ApplyDamage", BindingFlags.Static | BindingFlags.NonPublic);
				harmony.Patch((MethodBase)_applyDamageMethod, new HarmonyMethod(method), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				MelonLogger.Msg("[KGT] Patched DamageManager.ApplyDamage");
			}
			else
			{
				MelonLogger.Warning("[KGT] DamageManager.ApplyDamage method not found");
			}
			MethodInfo method2 = _damageManagerType.GetMethod("SetHealth", BindingFlags.Instance | BindingFlags.Public, null, new Type[3]
			{
				typeof(float),
				typeof(GameObject),
				typeof(bool)
			}, null);
			if (method2 != null)
			{
				MethodInfo method3 = typeof(KingOfTheHill).GetMethod("Prefix_SetHealth", BindingFlags.Static | BindingFlags.NonPublic);
				harmony.Patch((MethodBase)method2, new HarmonyMethod(method3), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				MelonLogger.Msg("[KGT] Patched DamageManager.SetHealth");
			}
			else
			{
				MelonLogger.Warning("[KGT] DamageManager.SetHealth(float,GameObject,bool) not found");
			}
			MethodInfo method4 = _damageManagerType.GetMethod("SetHealth01", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(float) }, null);
			if (method4 != null)
			{
				MethodInfo method5 = typeof(KingOfTheHill).GetMethod("Prefix_SetHealth01", BindingFlags.Static | BindingFlags.NonPublic);
				harmony.Patch((MethodBase)method4, new HarmonyMethod(method5), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				MelonLogger.Msg("[KGT] Patched DamageManager.SetHealth01");
			}
			else
			{
				MelonLogger.Warning("[KGT] DamageManager.SetHealth01(float) not found");
			}
			MethodInfo method6 = _damageManagerType.GetMethod("OnReceiveClientDamageHitPacket", BindingFlags.Instance | BindingFlags.Public);
			if (method6 != null)
			{
				MethodInfo method7 = typeof(KingOfTheHill).GetMethod("Transpile_OnReceiveClientDamageHitPacket", BindingFlags.Static | BindingFlags.NonPublic);
				harmony.Patch((MethodBase)method6, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(method7), (HarmonyMethod)null, (HarmonyMethod)null);
				MelonLogger.Msg("[KGT] Transpiled DamageManager.OnReceiveClientDamageHitPacket");
			}
			else
			{
				MelonLogger.Warning("[KGT] DamageManager.OnReceiveClientDamageHitPacket not found");
			}
			_damagePatchInstalled = true;
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] InstallDamagePatch failed: " + ex.Message);
		}
		InstallTargetingPatch(harmony);
	}

	private static void InstallTargetingPatch(Harmony harmony)
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected Obj, but got Unknown
		if (_targetingPatchInstalled)
		{
			return;
		}
		try
		{
			Type type = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Type type2 = assemblies[i].GetType("GameMode");
				if (type2 != null)
				{
					type = type2;
					break;
				}
			}
			if (type == null)
			{
				MelonLogger.Warning("[KGT] GameMode type not found — targeting bypass skipped");
				return;
			}
			_teamTypeForTargeting = type.Assembly.GetType("Team");
			_teamShortNameField = _teamTypeForTargeting?.GetField("TeamShortName", BindingFlags.Instance | BindingFlags.Public);
			MethodInfo method = type.GetMethod("GetTeamsAreEnemy", BindingFlags.Instance | BindingFlags.Public);
			if (method == null)
			{
				MelonLogger.Warning("[KGT] GameMode.GetTeamsAreEnemy not found");
				return;
			}
			MethodInfo method2 = typeof(KingOfTheHill).GetMethod("Postfix_GetTeamsAreEnemy", BindingFlags.Static | BindingFlags.NonPublic);
			harmony.Patch((MethodBase)method, (HarmonyMethod)null, new HarmonyMethod(method2), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			_targetingPatchInstalled = true;
			MelonLogger.Msg("[KGT] Patched GameMode.GetTeamsAreEnemy (Gamemaster team is always allied)");
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] InstallTargetingPatch failed: " + ex.Message);
		}
	}

	private static void Postfix_GetTeamsAreEnemy(object team1, object team2, ref bool __result)
	{
		if (_hasKoh && __result && (IsGamemasterTeam(team1) || IsGamemasterTeam(team2)))
		{
			__result = false;
		}
	}

	private static bool ShortNameIsNeutral(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return false;
		}
		if (!s.Equals("GM", StringComparison.OrdinalIgnoreCase) && !s.Equals("Master", StringComparison.OrdinalIgnoreCase) && !s.Equals("Wildlife", StringComparison.OrdinalIgnoreCase))
		{
			return s.Equals("Worm", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool GoNameIsNeutral(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return false;
		}
		if (s.IndexOf("Master", StringComparison.OrdinalIgnoreCase) < 0 && s.IndexOf("Gamemaster", StringComparison.OrdinalIgnoreCase) < 0 && s.IndexOf("Wildlife", StringComparison.OrdinalIgnoreCase) < 0)
		{
			return s.IndexOf("Worm", StringComparison.OrdinalIgnoreCase) >= 0;
		}
		return true;
	}

	private static bool IsGamemasterTeam(object? team)
	{
		if (team == null)
		{
			return false;
		}
		if (_teamClassification.TryGetValue(team, out var value))
		{
			return value;
		}
		string text = (_teamShortNameField?.GetValue(team) as string) ?? "";
		object? obj = ((team is Component) ? team : null);
		string text2 = ((obj != null) ? ((Object)obj).name : null) ?? "";
		bool flag = ShortNameIsNeutral(text) || GoNameIsNeutral(text2);
		_teamClassification[team] = flag;
		MelonLogger.Msg($"[KGT] Team classified: short='{text}', go='{text2}' -> neutral={flag}");
		return flag;
	}

	private static void ResetDamageDiag()
	{
		_damageMissLog = 0;
		_damageHitLog = 0;
		_clientHitBlockLog = 0;
	}

	private static bool Prefix_ApplyDamage(object __instance, ref float __result, GameObject? __3)
	{
		try
		{
			if (!_hasKoh || (Object)(object)_kohTower == (Object)null)
			{
				return true;
			}
			Component val = (Component)((__instance is Component) ? __instance : null);
			if ((Object)(object)val == (Object)null)
			{
				return true;
			}
			GameObject gameObject = ((Component)_kohTower).gameObject;
			Transform val2 = val.transform;
			while ((Object)(object)val2 != (Object)null)
			{
				if (((Component)val2).gameObject == gameObject)
				{
					if (_damageHitLog < 5)
					{
						_damageHitLog++;
						string arg = DescribeInstigator(__3);
						MelonLogger.Msg($"[KoH/DBG] ApplyDamage HIT #{_damageHitLog} — KoH immunity engaged (damage zeroed) | attacker={arg}");
					}
					__result = 0f;
					return false;
				}
				val2 = val2.parent;
			}
			if (_damageMissLog < 5)
			{
				_damageMissLog++;
				string text = BuildHierarchyChain(val.transform);
				MelonLogger.Msg($"[KoH/DBG] ApplyDamage MISS #{_damageMissLog}: target={text} | koh={((Object)gameObject).name}#{((Object)gameObject).GetInstanceID()}");
			}
		}
		catch (Exception)
		{
		}
		return true;
	}

	private static bool Prefix_SetHealth(object __instance, float health, GameObject instigator, bool force)
	{
		try
		{
			if (!_hasKoh || (Object)(object)_kohTower == (Object)null)
			{
				return true;
			}
			Component val = (Component)((__instance is Component) ? __instance : null);
			if ((Object)(object)val == (Object)null)
			{
				return true;
			}
			GameObject gameObject = ((Component)_kohTower).gameObject;
			Transform val2 = val.transform;
			while ((Object)(object)val2 != (Object)null)
			{
				if (((Component)val2).gameObject == gameObject)
				{
					MelonLogger.Msg($"[KoH/DBG] SetHealth on KoH BLOCKED (caller wanted health={health}, force={force})");
					return false;
				}
				val2 = val2.parent;
			}
		}
		catch (Exception)
		{
		}
		return true;
	}

	private static bool Prefix_SetHealth01(object __instance, float health01)
	{
		try
		{
			if (!_hasKoh || (Object)(object)_kohTower == (Object)null)
			{
				return true;
			}
			Component val = (Component)((__instance is Component) ? __instance : null);
			if ((Object)(object)val == (Object)null)
			{
				return true;
			}
			GameObject gameObject = ((Component)_kohTower).gameObject;
			Transform val2 = val.transform;
			while ((Object)(object)val2 != (Object)null)
			{
				if (((Component)val2).gameObject == gameObject)
				{
					MelonLogger.Msg($"[KoH/DBG] SetHealth01 on KoH BLOCKED (caller wanted health01={health01})");
					return false;
				}
				val2 = val2.parent;
			}
		}
		catch (Exception)
		{
		}
		return true;
	}

	public static bool IsKohDamageManager(object dm)
	{
		try
		{
			if (!_hasKoh || (Object)(object)_kohTower == (Object)null)
			{
				return false;
			}
			Component val = (Component)((dm is Component) ? dm : null);
			if ((Object)(object)val == (Object)null)
			{
				return false;
			}
			GameObject gameObject = ((Component)_kohTower).gameObject;
			Transform val2 = val.transform;
			while ((Object)(object)val2 != (Object)null)
			{
				if (((Component)val2).gameObject == gameObject)
				{
					if (_clientHitBlockLog < 5)
					{
						_clientHitBlockLog++;
						MelonLogger.Msg($"[KoH/DBG] OnReceiveClientDamageHitPacket: KoH damage skipped (block #{_clientHitBlockLog})");
					}
					return true;
				}
				val2 = val2.parent;
			}
			return false;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static IEnumerable<CodeInstruction> Transpile_OnReceiveClientDamageHitPacket(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Expected Obj, but got Unknown
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Expected Obj, but got Unknown
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Expected Obj, but got Unknown
		List<CodeInstruction> list = new List<CodeInstruction>(instructions);
		FieldInfo fieldInfo = _damageManagerType?.GetField("Health", BindingFlags.Instance | BindingFlags.Public);
		PropertyInfo propertyInfo = _damageManagerType?.GetProperty("Health", BindingFlags.Instance | BindingFlags.Public);
		MethodInfo methodInfo = propertyInfo?.GetGetMethod(nonPublic: true);
		MethodInfo methodInfo2 = propertyInfo?.GetSetMethod(nonPublic: true);
		MethodInfo methodInfo3 = _damageManagerType?.GetMethod("OnDamageReceived", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		bool flag = fieldInfo == null && methodInfo != null && methodInfo2 != null;
		bool flag2 = fieldInfo != null;
		if (methodInfo3 == null || (!flag && !flag2))
		{
			MelonLogger.Warning("[KGT] Transpile_OnReceive: Health not resolvable (field=" + (fieldInfo != null) + ", prop=" + (propertyInfo != null) + ", get=" + (methodInfo != null) + ", set=" + (methodInfo2 != null) + ") or OnDamageReceived missing — passing through original IL");
			return list;
		}
		int num = -1;
		for (int i = 0; i < list.Count; i++)
		{
			CodeInstruction val = list[i];
			if ((val.opcode == OpCodes.Call || val.opcode == OpCodes.Callvirt) && val.operand is MethodInfo methodInfo4 && methodInfo4 == methodInfo3)
			{
				num = i;
				break;
			}
		}
		if (num < 0 || num + 1 >= list.Count)
		{
			MelonLogger.Warning("[KGT] Transpile_OnReceive: OnDamageReceived call site not found — passing through");
			return list;
		}
		int index = num + 1;
		Label label = generator.DefineLabel();
		list[index].labels.Add(label);
		int num2 = -1;
		for (int num3 = num - 1; num3 >= 0; num3--)
		{
			OpCode opcode = list[num3].opcode;
			if (flag2 && opcode == OpCodes.Stfld && list[num3].operand is FieldInfo fieldInfo2 && fieldInfo2 == fieldInfo)
			{
				num2 = num3;
				break;
			}
			if (flag && (opcode == OpCodes.Call || opcode == OpCodes.Callvirt) && list[num3].operand is MethodInfo methodInfo5 && methodInfo5 == methodInfo2)
			{
				num2 = num3;
				break;
			}
		}
		if (num2 < 0)
		{
			MelonLogger.Warning("[KGT] Transpile_OnReceive: Health write site (stfld or set_Health) not found before OnDamageReceived — passing through");
			return list;
		}
		int num4 = -1;
		for (int num5 = num2 - 1; num5 >= Math.Max(0, num2 - 10); num5--)
		{
			if (num5 + 2 < list.Count && !(list[num5].opcode != OpCodes.Ldarg_0) && !(list[num5 + 1].opcode != OpCodes.Ldarg_0))
			{
				CodeInstruction val2 = list[num5 + 2];
				bool num6 = flag2 && val2.opcode == OpCodes.Ldfld && val2.operand is FieldInfo fieldInfo3 && fieldInfo3 == fieldInfo;
				bool flag3 = flag && (val2.opcode == OpCodes.Call || val2.opcode == OpCodes.Callvirt) && val2.operand is MethodInfo methodInfo6 && methodInfo6 == methodInfo;
				if (num6 | flag3)
				{
					num4 = num5;
					break;
				}
			}
		}
		if (num4 < 0)
		{
			MelonLogger.Warning("[KGT] Transpile_OnReceive: Health-decrement preamble (ldarg.0 ldarg.0 ld[fld|call get]_Health) not found — passing through");
			return list;
		}
		MethodInfo methodInfo7 = AccessTools.Method(typeof(KingOfTheHill), "IsKohDamageManager", (Type[])null, (Type[])null);
		List<CodeInstruction> list2 = new List<CodeInstruction>
		{
			new CodeInstruction(OpCodes.Ldarg_0, (object)null),
			new CodeInstruction(OpCodes.Call, (object)methodInfo7),
			new CodeInstruction(OpCodes.Brtrue, (object)label)
		};
		if (list[num4].labels.Count > 0)
		{
			list2[0].labels.AddRange(list[num4].labels);
			list[num4].labels.Clear();
		}
		list.InsertRange(num4, list2);
		MelonLogger.Msg("[KGT] Transpiled OnReceiveClientDamageHitPacket — KoH skip-branch injected before Health decrement");
		return list;
	}

	private static string DescribeInstigator(GameObject? instigator)
	{
		if ((Object)(object)instigator == (Object)null)
		{
			return "<null>";
		}
		try
		{
			string text = ((Object)instigator).name ?? "?";
			BaseGameObject componentInParent = instigator.GetComponentInParent<BaseGameObject>();
			string text2 = "?";
			string text3 = "";
			if ((Object)(object)componentInParent != (Object)null)
			{
				try
				{
					Team team = componentInParent.Team;
					text2 = ((team != null) ? team.GetTeamShortName() : null) ?? "?";
				}
				catch
				{
				}
				try
				{
					NetworkComponent networkComponent = componentInParent.NetworkComponent;
					Player val = ((networkComponent != null) ? networkComponent.OwnerPlayer : null);
					if ((Object)(object)val != (Object)null)
					{
						text3 = val.PlayerName ?? "";
					}
				}
				catch
				{
				}
			}
			if (!string.IsNullOrEmpty(text3))
			{
				return "player='" + text3 + "' team=" + text2 + " unit=" + text;
			}
			return "ai team=" + text2 + " unit=" + text;
		}
		catch (Exception)
		{
			return ((Object)instigator).name ?? "<err>";
		}
	}

	private static string BuildHierarchyChain(Transform leaf)
	{
		List<string> list = new List<string>();
		Transform val = leaf;
		int num = 0;
		while ((Object)(object)val != (Object)null && num < 10)
		{
			list.Add($"{((Object)((Component)val).gameObject).name}#{((Object)((Component)val).gameObject).GetInstanceID()}");
			num++;
			val = val.parent;
		}
		list.Reverse();
		return string.Join(" > ", list);
	}

	public static int GetCredits(Player player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return 0;
		}
		long playerSteamId = GetPlayerSteamId(player);
		if (!_playerCredits.TryGetValue(playerSteamId, out var value))
		{
			return 0;
		}
		return value;
	}

	private static int GetCredits(long steamId)
	{
		if (!_playerCredits.TryGetValue(steamId, out var value))
		{
			return 0;
		}
		return value;
	}

	private static void AddCredits(long steamId, int delta)
	{
		if (delta != 0)
		{
			int num = GetCredits(steamId) + delta;
			if (num < 0)
			{
				num = 0;
			}
			_playerCredits[steamId] = num;
		}
	}

	public static bool GrantExternalCredits(long steamId, int amount, string reason)
	{
		if (!ModeBuyActive || steamId == 0L || amount <= 0)
		{
			return false;
		}
		AddCredits(steamId, amount);
		MelonLogger.Msg(string.Format("[KGT] +{0}cr -> {1} ({2}). Total: {3}cr", amount, steamId, reason ?? "external", GetCredits(steamId)));
		return true;
	}

	public static int GetCreditsBySteamId(long steamId)
	{
		if (!ModeBuyActive)
		{
			return 0;
		}
		return GetCredits(steamId);
	}

	private static long GetPlayerSteamId(Player p)
	{
		try
		{
			return (long)p.PlayerID.m_ID;
		}
		catch
		{
			return 0L;
		}
	}

	private static void ResetAllCredits()
	{
		_playerCredits.Clear();
	}

	private static void GrantStarterCreditsToAll()
	{
		int starterCreditsPerPlayer = Cfg.StarterCreditsPerPlayer;
		if (starterCreditsPerPlayer <= 0)
		{
			return;
		}
		try
		{
			List<Player> players = Player.Players;
			if (players == null)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < players.Count; i++)
			{
				Player val = players[i];
				if ((Object)(object)val == (Object)null || (Object)(object)val == (Object)(object)NetworkGameServer.GetServerPlayer())
				{
					continue;
				}
				long playerSteamId = GetPlayerSteamId(val);
				if (playerSteamId != 0L && _starterCreditsGranted.Add(playerSteamId))
				{
					AddCredits(playerSteamId, starterCreditsPerPlayer);
					num++;
					try
					{
						HelperMethods.SendChatMessageToPlayer(val, $"[KGT] Welcome — starter credits: <color=#39ff14>{starterCreditsPerPlayer}</color>cr. Type /buy to spend.");
					}
					catch
					{
					}
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[KGT] Starter credits: granted {starterCreditsPerPlayer}cr to {num} player(s)");
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] GrantStarterCreditsToAll threw: " + ex.Message);
		}
	}

	private static void HookRewards()
	{
		if (_rewardsHooked)
		{
			return;
		}
		try
		{
			GameEvents.OnUnitDestroyed = (Action<Unit, GameObject>)Delegate.Remove(GameEvents.OnUnitDestroyed, new Action<Unit, GameObject>(OnUnitDestroyed_Reward));
			GameEvents.OnUnitDestroyed = (Action<Unit, GameObject>)Delegate.Combine(GameEvents.OnUnitDestroyed, new Action<Unit, GameObject>(OnUnitDestroyed_Reward));
			GameEvents.OnStructureDestroyed = (Action<Structure, GameObject>)Delegate.Remove(GameEvents.OnStructureDestroyed, new Action<Structure, GameObject>(OnStructureDestroyed_Reward));
			GameEvents.OnStructureDestroyed = (Action<Structure, GameObject>)Delegate.Combine(GameEvents.OnStructureDestroyed, new Action<Structure, GameObject>(OnStructureDestroyed_Reward));
			_rewardsHooked = true;
			MelonLogger.Msg("[KGT] Rewards: subscribed to OnUnitDestroyed + OnStructureDestroyed");
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] HookRewards failed: " + ex.Message);
		}
	}

	private static void ReHookRewards()
	{
		_rewardsHooked = false;
		HookRewards();
	}

	private static void OnUnitDestroyed_Reward(Unit unit, GameObject instigator)
	{
		try
		{
			if (!ModeBuyActive || (Object)(object)unit == (Object)null || (Object)(object)((BaseGameObject)unit).ObjectInfo == (Object)null || (Object)(object)instigator == (Object)null)
			{
				return;
			}
			BaseGameObject baseGameObject = GameFuncs.GetBaseGameObject(instigator, true);
			object obj;
			if (baseGameObject == null)
			{
				obj = null;
			}
			else
			{
				NetworkComponent networkComponent = baseGameObject.NetworkComponent;
				obj = ((networkComponent != null) ? networkComponent.OwnerPlayer : null);
			}
			Player val = (Player)obj;
			if ((Object)(object)val == (Object)null || ((Object)(object)((BaseGameObject)unit).Team != (Object)null && (Object)(object)val.Team != (Object)null && (Object)(object)((BaseGameObject)unit).Team == (Object)(object)val.Team) || !TryGetEffectiveCost(((BaseGameObject)unit).ObjectInfo, out int cost, out string lookupKey))
			{
				return;
			}
			int num = Mathf.RoundToInt((float)cost * Cfg.RewardKillFraction);
			if (num > 0)
			{
				long playerSteamId = GetPlayerSteamId(val);
				AddCredits(playerSteamId, num);
				try
				{
					HelperMethods.SendChatMessageToPlayer(val, $"[KGT] +<color=#39ff14>{num}</color>cr (killed {lookupKey}). Total: {GetCredits(playerSteamId)}cr");
				}
				catch
				{
				}
				PayBountyIfApplicable(unit, val);
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] OnUnitDestroyed_Reward threw: " + ex.Message);
		}
	}

	private static void OnStructureDestroyed_Reward(Structure structure, GameObject instigator)
	{
		try
		{
			if (!ModeBuyActive || !Cfg.StructureKillRewardsEnabled || (Object)(object)structure == (Object)null || (Object)(object)((BaseGameObject)structure).ObjectInfo == (Object)null || (Object)(object)instigator == (Object)null)
			{
				return;
			}
			BaseGameObject baseGameObject = GameFuncs.GetBaseGameObject(instigator, true);
			object obj;
			if (baseGameObject == null)
			{
				obj = null;
			}
			else
			{
				NetworkComponent networkComponent = baseGameObject.NetworkComponent;
				obj = ((networkComponent != null) ? networkComponent.OwnerPlayer : null);
			}
			Player val = (Player)obj;
			if ((Object)(object)val == (Object)null || ((Object)(object)((BaseGameObject)structure).Team != (Object)null && (Object)(object)val.Team != (Object)null && (Object)(object)((BaseGameObject)structure).Team == (Object)(object)val.Team) || !TryGetEffectiveCost(((BaseGameObject)structure).ObjectInfo, out int cost, out string lookupKey))
			{
				return;
			}
			int num = Mathf.RoundToInt((float)cost * Cfg.RewardKillFraction);
			if (num <= 0)
			{
				return;
			}
			long playerSteamId = GetPlayerSteamId(val);
			AddCredits(playerSteamId, num);
			try
			{
				HelperMethods.SendChatMessageToPlayer(val, $"[KGT] +<color=#39ff14>{num}</color>cr (destroyed {lookupKey}). Total: {GetCredits(playerSteamId)}cr");
			}
			catch
			{
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] OnStructureDestroyed_Reward threw: " + ex.Message);
		}
	}

	private static void RecomputeHandicap(float dt)
	{
		if (!Cfg.HandicapEnabled)
		{
			if (_handicapMult.Count > 0)
			{
				_handicapMult.Clear();
			}
			return;
		}
		_handicapTimer -= dt;
		if (_handicapTimer > 0f)
		{
			return;
		}
		_handicapTimer = Mathf.Max(1f, Cfg.HandicapRecomputeIntervalSeconds);
		try
		{
			List<Team> teams = Team.Teams;
			if (teams == null || teams.Count < 2)
			{
				_handicapMult.Clear();
				return;
			}
			Dictionary<Team, float> dictionary = new Dictionary<Team, float>();
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < teams.Count; i++)
			{
				Team val = teams[i];
				if (!((Object)(object)val == (Object)null) && !IsGamemasterTeam(val))
				{
					float num3 = 0f;
					int num4 = 0;
					try
					{
						num3 = val.TotalResources;
					}
					catch
					{
					}
					try
					{
						num4 = val.Units?.Count ?? 0;
					}
					catch
					{
					}
					float num5 = (dictionary[val] = num3 * Cfg.HandicapResourceWeight + (float)num4 * 100f * Cfg.HandicapMilitaryWeight);
					num += num5;
					num2++;
				}
			}
			_handicapMult.Clear();
			if (num2 < 2 || num <= 0f)
			{
				return;
			}
			float num7 = num / (float)num2;
			float handicapStrength = Cfg.HandicapStrength;
			float num8 = Mathf.Max(0.01f, Cfg.HandicapFloorMult);
			float num9 = Mathf.Max(num8, Cfg.HandicapCeilingMult);
			foreach (KeyValuePair<Team, float> item in dictionary)
			{
				float num10 = item.Value / num7;
				if (num10 <= 0f)
				{
					num10 = 0.001f;
				}
				float num11 = Mathf.Pow(num10, 0f - handicapStrength);
				num11 = Mathf.Clamp(num11, num8, num9);
				_handicapMult[item.Key] = num11;
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] RecomputeHandicap threw: " + ex.Message);
		}
	}

	private static float GetHandicapMult(Team team)
	{
		if (!Cfg.HandicapEnabled || (Object)(object)team == (Object)null)
		{
			return 1f;
		}
		if (!_handicapMult.TryGetValue(team, out var value))
		{
			return 1f;
		}
		return value;
	}

	private static void GrantCommanderRewards(Dictionary<Team, float> scores, float deltaSeconds)
	{
		try
		{
			if (!ModeKohActive || !Cfg.CommanderRewardEnabled || Cfg.CommanderRewardMultiplier <= 0f || scores == null || scores.Count == 0)
			{
				return;
			}
			float num = Cfg.MaxCapturePointsPerSecond * deltaSeconds;
			if (num <= 0f)
			{
				return;
			}
			foreach (KeyValuePair<Team, float> score in scores)
			{
				Team key = score.Key;
				if ((Object)(object)key == (Object)null || IsGamemasterTeam(key))
				{
					continue;
				}
				float num2 = Mathf.Min(score.Value, num);
				float handicapMult = GetHandicapMult(key);
				int num3 = Mathf.RoundToInt(num2 * Cfg.CommanderRewardMultiplier * handicapMult);
				if (num3 <= 0)
				{
					continue;
				}
				int num4 = key.StoreResource(num3);
				if (num4 > 0)
				{
					try
					{
						key.StartingResources += num4;
					}
					catch
					{
					}
				}
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] GrantCommanderRewards threw: " + ex.Message);
		}
	}

	private static void GrantZonePresenceReward(Player player, int unitCap, string unitName)
	{
		try
		{
			if (!((Object)(object)player == (Object)null) && unitCap > 0 && !(Cfg.RewardZonePresenceMultiplier <= 0f))
			{
				float num = unitCap;
				if (Cfg.RewardZoneCrabSkipPenalty && unitName == "Crab" && num < 1f)
				{
					num = 1f;
				}
				int num2 = Mathf.RoundToInt(Cfg.EvalIntervalSeconds * num * Cfg.RewardZonePresenceMultiplier);
				if (num2 > 0)
				{
					AddCredits(GetPlayerSteamId(player), num2);
				}
			}
		}
		catch
		{
		}
	}

	public override void OnInitializeMelon()
	{
		_configPath = Path.Combine("UserData", "KingOfTheHill_cfg", "Si_KingOfTheHill_Config.json");
		LoadConfig();
		MelonLogger.Msg("Si_KingOfTheHill v0.1.0 loaded (scaffold — game logic not yet implemented).");
		GameEvents.OnGameStarted = (Action<GameMode>)Delegate.Combine(GameEvents.OnGameStarted, new Action<GameMode>(OnGameStarted));
		GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Combine(GameEvents.OnGameEnded, new Action<GameMode, Team>(OnGameEnded));
		InstallDamagePatch(((MelonBase)this).HarmonyInstance);
		HookRewards();
		HookBuyChat();
		LoadBountyConfig();
	}

	public override void OnLateInitializeMelon()
	{
		PlayerMethods.RegisterPlayerCommand("koh", OnKohCommand, hideFromChat: true);
		PlayerMethods.RegisterPlayerCommand("buy", OnBuyCommand, hideFromChat: true);
		MelonLogger.Msg("Si_KingOfTheHill: Registered /koh and /buy commands.");
		TrySubscribeMapBalance();
		HookAntiBuild();
	}

	public override void OnSceneWasLoaded(int buildIndex, string sceneName)
	{
		GameEvents.OnGameStarted = (Action<GameMode>)Delegate.Remove(GameEvents.OnGameStarted, new Action<GameMode>(OnGameStarted));
		GameEvents.OnGameStarted = (Action<GameMode>)Delegate.Combine(GameEvents.OnGameStarted, new Action<GameMode>(OnGameStarted));
		GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Remove(GameEvents.OnGameEnded, new Action<GameMode, Team>(OnGameEnded));
		GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Combine(GameEvents.OnGameEnded, new Action<GameMode, Team>(OnGameEnded));
		ReHookRewards();
		ReHookBuyChat();
	}

	private static void OnGameStarted(GameMode mode)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (ModeAnyActive)
		{
			_roundStartTime = Time.time;
			if (ModeKohActive)
			{
				TrySubscribeMapBalance();
			}
			LoadUnitDump();
			ApplyUnitBalanceCostOverrides();
			if (ModeKohActive)
			{
				_startupCheckTimer = 3f;
				_startupCheckPending = true;
			}
			Scene activeScene = SceneManager.GetActiveScene();
			MelonLogger.Msg($"[KGT] OnGameStarted — map={activeScene.name} (_hasKoh={_hasKoh}) Mode={Cfg.Mode}");
			if (ModeKohActive && _hasKoh && Cfg.IntroEnabled)
			{
				BroadcastIntro();
			}
			_starterCreditsGranted.Clear();
			_starterCreditsCheckTimer = 5f;
			if (ModeBuyActive && Cfg.StarterCreditsPerPlayer > 0)
			{
				GrantStarterCreditsToAll();
			}
		}
	}

	private static void BroadcastIntro()
	{
		_introIdx = 0;
		_introClipIdx = 0;
		_introStage = 0;
		_introTimer = Mathf.Max(0f, Cfg.IntroStartDelaySeconds);
	}

	private static void TickIntroQueue(float dt)
	{
		if (_introIdx < 0 || _introIdx >= _introScript.Length)
		{
			return;
		}
		_introTimer -= dt;
		if (_introTimer > 0f)
		{
			return;
		}
		(string, (string, float)[]) tuple = _introScript[_introIdx];
		if (_introStage == 0)
		{
			BroadcastAllChat(tuple.Item1);
			_introClipIdx = 0;
			_introTimer = Mathf.Max(0f, Cfg.IntroSoundLeadSeconds);
			_introStage = 1;
		}
		else if (_introStage == 1)
		{
			if (_introClipIdx < tuple.Item2.Length)
			{
				(string, float) tuple2 = tuple.Item2[_introClipIdx];
				if (!string.IsNullOrEmpty(tuple2.Item1))
				{
					PlaySound(tuple2.Item1);
				}
				_introClipIdx++;
				_introTimer = tuple2.Item2 + 0.15f;
			}
			else
			{
				_introTimer = Mathf.Max(0f, Cfg.IntroLinePauseSeconds);
				_introStage = 2;
			}
		}
		else
		{
			_introIdx++;
			_introClipIdx = 0;
			_introStage = 0;
			_introTimer = 0f;
			if (_introIdx >= _introScript.Length)
			{
				_introIdx = -1;
			}
		}
	}

	private static void ResetIntroState()
	{
		_introIdx = -1;
		_introClipIdx = 0;
		_introStage = 0;
		_introTimer = 0f;
	}

	private static void OnGameEnded(GameMode mode, Team winner)
	{
		ResetKohState();
		_startupCheckPending = false;
		ResetIntroState();
		if (Cfg.RewardResetOnGameEnd)
		{
			ResetAllCredits();
		}
		ResetBuyState();
		_announcedBounties.Clear();
		MelonLogger.Msg("[KGT] OnGameEnded — KoH state cleared.");
	}

	public override void OnUpdate()
	{
		if (!NetworkGameServer.GetServerStarted() || !ModeAnyActive)
		{
			return;
		}
		if (ModeBuyActive)
		{
			_starterCreditsCheckTimer -= Time.deltaTime;
			if (_starterCreditsCheckTimer <= 0f)
			{
				_starterCreditsCheckTimer = 5f;
				if (Cfg.StarterCreditsPerPlayer > 0)
				{
					GrantStarterCreditsToAll();
				}
				AnnounceBountiesIfNeeded();
			}
		}
		if (!ModeKohActive)
		{
			return;
		}
		if (_startupCheckPending)
		{
			_startupCheckTimer -= Time.deltaTime;
			if (_startupCheckTimer <= 0f)
			{
				_startupCheckPending = false;
				if (!_hasKoh)
				{
					BroadcastAllChat("[KGT] mod not active, spawn cfg missing");
				}
				else
				{
					MelonLogger.Msg("[KGT] startup check OK — KoH active this round.");
				}
			}
		}
		TickIntroQueue(Time.deltaTime);
		TickKohAliveCheck(Time.deltaTime);
		_evalTimer -= Time.deltaTime;
		if (_evalTimer > 0f)
		{
			return;
		}
		float num = (_evalTimer = Cfg.EvalIntervalSeconds);
		try
		{
			RecomputeHandicap(num);
			Dictionary<Team, float> scores = EvaluateScores();
			UpdateOwnership(scores, num);
			GrantCommanderRewards(scores, num);
			TickTimer(num);
			ProcessPlayerNotifications();
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] tick failed: " + ex.Message);
		}
	}

	private static void ResetTimerState()
	{
		_statusAnnounceTimer = 0f;
		_winFallbackTimer = 0f;
		_winningTeam = null;
		_winFinishingDelayTimer = 0f;
		_winFinishingPending = false;
	}

	private static void TickTimer(float deltaSeconds)
	{
		if (_winTriggered)
		{
			TickWinFinishingDelay(deltaSeconds);
			TickWinFallback(deltaSeconds);
		}
		else
		{
			if (!_hasKoh || Cfg.StatusAnnounceIntervalSeconds <= 0f)
			{
				return;
			}
			_statusAnnounceTimer -= deltaSeconds;
			if (!(_statusAnnounceTimer > 0f))
			{
				_statusAnnounceTimer = Cfg.StatusAnnounceIntervalSeconds;
				if (!((Object)(object)_currentKing == (Object)null))
				{
					float currentProgress = GetCurrentProgress(_currentKing);
					float pct = ((_winThreshold > 0f) ? (currentProgress / _winThreshold * 100f) : 0f);
					BroadcastAllChat("[KGT] " + ColoredTeamName(_currentKing) + " presently hold " + ColoredPercent(pct) + " of the hill.");
				}
			}
		}
	}

	private static void PlaySound(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		try
		{
			AudioHelper.PlaySoundFile(path);
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] PlaySoundFile('" + path + "') failed: " + ex.Message);
		}
	}

	private static void TriggerWin(Team winner)
	{
		if (!_winTriggered)
		{
			_winTriggered = true;
			_winningTeam = winner;
			_winFallbackTimer = 60f;
			string text = SafeTeamName(winner);
			BroadcastAllChat("[KGT] " + GetWinMessageForTeam(winner, text));
			PlaySound(GetWinSoundForTeam(winner));
			MelonLogger.Msg($"[KGT] WIN: {text}. Voice line playing; cannon + finishing force in {Cfg.WinFinishingForceDelaySeconds}s.");
			LogToReplay("kgt_win", ("winner", ReplayTeamTag(winner)));
			_winFinishingDelayTimer = Mathf.Max(0f, Cfg.WinFinishingForceDelaySeconds);
			_winFinishingPending = true;
		}
	}

	private static void TickWinFinishingDelay(float deltaSeconds)
	{
		if (!_winFinishingPending || (Object)(object)_winningTeam == (Object)null)
		{
			return;
		}
		_winFinishingDelayTimer -= deltaSeconds;
		if (_winFinishingDelayTimer > 0f)
		{
			return;
		}
		_winFinishingPending = false;
		MelonLogger.Msg("[KGT] Finishing force time — cannon + unit spawn.");
		PlaySound(Cfg.SoundFinishingForce);
		try
		{
			SpawnFinishingForce(_winningTeam);
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] SpawnFinishingForce threw: " + ex.Message);
		}
	}

	private static void TickWinFallback(float deltaSeconds)
	{
		if (!_winTriggered || (Object)(object)_winningTeam == (Object)null)
		{
			return;
		}
		_winFallbackTimer -= deltaSeconds;
		if (_winFallbackTimer > 0f)
		{
			return;
		}
		_winFallbackTimer = float.PositiveInfinity;
		try
		{
			int num = 0;
			int num2 = 0;
			foreach (Team team in Team.Teams)
			{
				if ((Object)(object)team == (Object)null || team == _winningTeam || IsGamemasterTeam(team))
				{
					continue;
				}
				List<Structure> structures = team.Structures;
				if (structures != null)
				{
					for (int num3 = structures.Count - 1; num3 >= 0; num3--)
					{
						Structure val = structures[num3];
						if (!((Object)(object)val == (Object)null) && !((BaseGameObject)val).IsDestroyed && !((Object)(object)((BaseGameObject)val).ObjectInfo == (Object)null) && ((BaseGameObject)val).ObjectInfo.Critical)
						{
							try
							{
								DamageManager damageManager = ((BaseGameObject)val).DamageManager;
								if (damageManager != null)
								{
									damageManager.SetHealth01(0f);
								}
								num++;
							}
							catch
							{
							}
						}
					}
				}
				List<Unit> units = team.Units;
				if (units == null)
				{
					continue;
				}
				for (int num4 = units.Count - 1; num4 >= 0; num4--)
				{
					Unit val2 = units[num4];
					if (!((Object)(object)val2 == (Object)null) && !((BaseGameObject)val2).IsDestroyed && !((Object)(object)((BaseGameObject)val2).ObjectInfo == (Object)null) && ((BaseGameObject)val2).ObjectInfo.Critical)
					{
						try
						{
							DamageManager damageManager2 = ((BaseGameObject)val2).DamageManager;
							if (damageManager2 != null)
							{
								damageManager2.SetHealth01(0f);
							}
							num2++;
						}
						catch
						{
						}
					}
				}
			}
			if (num + num2 > 0)
			{
				MelonLogger.Msg($"[KGT] Fallback after {60f}s: destroyed {num} struct(s) + {num2} unit(s) the finishing force couldn't kill.");
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] win fallback failed: " + ex.Message);
		}
	}

	private static void SpawnFinishingForce(Team winner)
	{
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		string text = winner.GetTeamShortName() ?? "";
		bool flag = text.IndexOf("Alien", StringComparison.OrdinalIgnoreCase) >= 0;
		bool flag2 = text.IndexOf("Sol", StringComparison.OrdinalIgnoreCase) >= 0;
		bool flag3 = text.IndexOf("Cent", StringComparison.OrdinalIgnoreCase) >= 0;
		string text2;
		if (flag)
		{
			text2 = "Goliath";
		}
		else if (flag3)
		{
			text2 = "Cent_UltraHeavy_CrimsonTank";
		}
		else
		{
			if (!flag2)
			{
				MelonLogger.Msg("[KGT] No finishing-force prefab for unknown winner — skipping.");
				return;
			}
			text2 = "Sol_UltraHeavy_SiegeTank";
		}
		int spawnablePrefabIndex = GameDatabase.GetSpawnablePrefabIndex(text2);
		if (spawnablePrefabIndex < 0)
		{
			MelonLogger.Warning("[KGT] Finishing prefab '" + text2 + "' not found");
			return;
		}
		GameObject spawnablePrefab = GameDatabase.GetSpawnablePrefab(spawnablePrefabIndex);
		if ((Object)(object)spawnablePrefab == (Object)null)
		{
			return;
		}
		int num = 0;
		foreach (Team team in Team.Teams)
		{
			if ((Object)(object)team == (Object)null || team == winner || IsGamemasterTeam(team))
			{
				continue;
			}
			bool losingIsAlien = (team.GetTeamShortName() ?? "").IndexOf("Alien", StringComparison.OrdinalIgnoreCase) >= 0;
			List<Structure> structures = team.Structures;
			if (structures != null)
			{
				for (int i = 0; i < structures.Count; i++)
				{
					Structure val = structures[i];
					if (!((Object)(object)val == (Object)null) && !((BaseGameObject)val).IsDestroyed && !((Object)(object)((BaseGameObject)val).ObjectInfo == (Object)null) && ((BaseGameObject)val).ObjectInfo.Critical)
					{
						Vector3 position = ((Component)val).transform.position;
						num += (flag ? SpawnDropOnHQ(spawnablePrefab, winner, position, countFor(dropping: true)) : SpawnRingAround(spawnablePrefab, winner, position, countFor(dropping: false), 90f));
					}
				}
			}
			List<Unit> units = team.Units;
			if (units == null)
			{
				continue;
			}
			for (int j = 0; j < units.Count; j++)
			{
				Unit val2 = units[j];
				if (!((Object)(object)val2 == (Object)null) && !((BaseGameObject)val2).IsDestroyed && !((Object)(object)((BaseGameObject)val2).ObjectInfo == (Object)null) && ((BaseGameObject)val2).ObjectInfo.Critical)
				{
					Vector3 position2 = ((Component)val2).transform.position;
					MelonLogger.Msg($"[KGT] Finishing force target (Unit): {((BaseGameObject)val2).ObjectInfo.DisplayName} on team {SafeTeamName(team)} at ({position2.x:F0},{position2.z:F0})");
					num += (flag ? SpawnDropOnHQ(spawnablePrefab, winner, position2, countFor(dropping: true)) : SpawnRingAround(spawnablePrefab, winner, position2, countFor(dropping: false), 90f));
				}
			}
			int countFor(bool dropping)
			{
				if (!dropping)
				{
					if (!losingIsAlien)
					{
						return 8;
					}
					return 16;
				}
				return 8;
			}
		}
		MelonLogger.Msg($"[KGT] Finishing force: spawned {num} {text2}(s) for team {SafeTeamName(winner)}.");
	}

	private static int SpawnRingAround(GameObject prefab, Team team, Vector3 center, int count, float radius)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		Terrain activeTerrain = Terrain.activeTerrain;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			float num2 = (float)i / (float)count * 2f * MathF.PI;
			float num3 = center.x + radius * Mathf.Cos(num2);
			float num4 = center.z + radius * Mathf.Sin(num2);
			float num5 = SampleSurfaceY(activeTerrain, num3, num4) + 2f;
			try
			{
				if ((Object)(object)Game.SpawnPrefab(prefab, (Player)null, team, new Vector3(num3, num5, num4), Quaternion.Euler(0f, (0f - num2) * 57.29578f + 90f, 0f), true, true) != (Object)null)
				{
					num++;
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[KGT] ring spawn failed: " + ex.Message);
			}
		}
		return num;
	}

	private static int SpawnDropOnHQ(GameObject prefab, Team team, Vector3 hqPos, int count)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			float num2 = (((i & 1) == 0) ? (-3f) : 3f);
			float num3 = ((i < 2) ? (-3f) : 3f);
			Vector3 val = new Vector3(hqPos.x + num2, hqPos.y + 80f + (float)i * 8f, hqPos.z + num3);
			try
			{
				if ((Object)(object)Game.SpawnPrefab(prefab, (Player)null, team, val, Quaternion.identity, true, true) != (Object)null)
				{
					num++;
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[KGT] drop spawn failed: " + ex.Message);
			}
		}
		return num;
	}

	private static string GetWinSoundForTeam(Team winner)
	{
		try
		{
			string text = winner.GetTeamShortName() ?? "";
			if (text.IndexOf("Alien", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return Cfg.SoundWinAlien;
			}
			if (text.IndexOf("Cent", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return Cfg.SoundWinCentauri;
			}
			if (text.IndexOf("Sol", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return Cfg.SoundWinSol;
			}
		}
		catch
		{
		}
		return Cfg.SoundWinSol;
	}

	private static string GetWinMessageForTeam(Team winner, string displayName)
	{
		try
		{
			string text = winner.GetTeamShortName() ?? "";
			if (text.IndexOf("Sol", StringComparison.OrdinalIgnoreCase) >= 0 && !string.IsNullOrEmpty(Cfg.WinMessageSol))
			{
				return Cfg.WinMessageSol;
			}
			if ((text.IndexOf("Cent", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("Centauri", StringComparison.OrdinalIgnoreCase) >= 0) && !string.IsNullOrEmpty(Cfg.WinMessageCentauri))
			{
				return Cfg.WinMessageCentauri;
			}
			if (text.IndexOf("Alien", StringComparison.OrdinalIgnoreCase) >= 0 && !string.IsNullOrEmpty(Cfg.WinMessageAlien))
			{
				return Cfg.WinMessageAlien;
			}
		}
		catch
		{
		}
		return (Cfg.WinMessageDefault ?? "{name} wins").Replace("{name}", displayName);
	}

	public static bool TryGetEffectiveCost(ObjectInfo? info, out int cost, out string lookupKey)
	{
		cost = 0;
		lookupKey = "";
		if ((Object)(object)info == (Object)null)
		{
			return false;
		}
		string text = info.DisplayName ?? "";
		if (!string.IsNullOrEmpty(text) && _unitCost.TryGetValue(text, out cost) && cost > 0)
		{
			lookupKey = text;
			return true;
		}
		string text2 = ((Object)info).name ?? "";
		if (!string.IsNullOrEmpty(text2) && _unitCostByInternal.TryGetValue(text2, out cost) && cost > 0)
		{
			lookupKey = ((text.Length > 0) ? text : text2);
			return true;
		}
		return false;
	}

	private static void LoadUnitDump()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Invalid comparison between Unknown and I4
		if (_dumpLoaded)
		{
			return;
		}
		_dumpLoaded = true;
		string text = Path.Combine("UserData", "UnitBalance_cfg", "Si_UnitBalance_Dump.json");
		if (!File.Exists(text))
		{
			MelonLogger.Warning("[KGT] Unit dump not found at " + text + " — all units will fall back to '" + Cfg.DefaultCategoryForUnknown + "' category");
			return;
		}
		try
		{
			JToken val = JObject.Parse(File.ReadAllText(text))["units"];
			if (val == null || (int)val.Type != 2)
			{
				MelonLogger.Warning("[KGT] Unit dump 'units' array missing");
				return;
			}
			int num = 0;
			int num2 = 0;
			foreach (JToken item in (IEnumerable<JToken>)val)
			{
				string text2 = ((object)item[(object)"name"])?.ToString();
				if (!string.IsNullOrEmpty(text2))
				{
					JToken val2 = item[(object)"is_structure"];
					bool flag = val2 != null && Extensions.Value<bool>((IEnumerable<JToken>)val2);
					if (flag)
					{
						_isStructure.Add(text2);
						num2++;
					}
					_unitToBuilding[text2] = ((object)item[(object)"built_at"])?.ToString() ?? "";
					JToken val3 = item[(object)"cost"];
					int value = ((val3 != null) ? Extensions.Value<int>((IEnumerable<JToken>)val3) : 0);
					_unitCostVanilla[text2] = value;
					_unitCost[text2] = value;
					_unitFaction[text2] = ((object)item[(object)"faction"])?.ToString() ?? "";
					string text3 = ((object)item[(object)"internal"])?.ToString() ?? "";
					if (!string.IsNullOrEmpty(text3))
					{
						_unitCostByInternal[text3] = value;
						_internalByDisplay[text2] = text3;
					}
					string text4 = ((object)item[(object)"internal"])?.ToString() ?? "";
					if (text4.StartsWith("ObjectInfo_"))
					{
						text4 = text4.Substring("ObjectInfo_".Length);
					}
					if (text4.StartsWith("Alien_") && !flag)
					{
						text4 = text4.Substring("Alien_".Length);
					}
					_unitPrefab[text2] = text4;
					if (flag)
					{
						_structRows.Add(new StructRow
						{
							Display = text2,
							Prefab = text4,
							Faction = (((object)item[(object)"faction"])?.ToString() ?? ""),
							ObjectInfo = text3
						});
					}
					num++;
				}
			}
			MelonLogger.Msg($"[KGT] Loaded {num} dump entries ({num2} marked as structures — included for kill rewards, hidden from /buy)");
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] Failed to parse unit dump: " + ex.Message);
		}
	}

	private static void ApplyUnitBalanceCostOverrides()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Invalid comparison between Unknown and I4
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Invalid comparison between Unknown and I4
		if (_unitCostVanilla.Count == 0)
		{
			int num = SeedVanillaCostsFromConstructionData();
			MelonLogger.Msg($"[KGT] No dump — seeded {num} vanilla unit costs from live ConstructionData (fallback)");
		}
		foreach (KeyValuePair<string, int> item in _unitCostVanilla)
		{
			_unitCost[item.Key] = item.Value;
		}
		string text = Path.Combine("UserData", "UnitBalance_cfg", "Si_UnitBalance_Config.json");
		if (!File.Exists(text))
		{
			MelonLogger.Msg("[KGT] " + text + " not found — rewards use vanilla unit costs");
			return;
		}
		try
		{
			JToken val = JObject.Parse(File.ReadAllText(text))["units"];
			if (val == null || (int)val.Type != 1)
			{
				MelonLogger.Warning("[KGT] Si_UnitBalance_Config.json: 'units' object missing");
				return;
			}
			int num2 = 0;
			int num3 = 0;
			foreach (JProperty item2 in val.Children<JProperty>())
			{
				string name = item2.Name;
				if (string.IsNullOrEmpty(name) || name[0] == '_')
				{
					continue;
				}
				JToken value = item2.Value;
				if (value == null || (int)value.Type != 1)
				{
					continue;
				}
				JToken val2 = value[(object)"cost_mult"];
				if (val2 == null)
				{
					continue;
				}
				float num4 = Extensions.Value<float>((IEnumerable<JToken>)val2);
				if (num4 <= 0f)
				{
					continue;
				}
				if (!_unitCostVanilla.TryGetValue(name, out var value2) || value2 <= 0)
				{
					num3++;
					continue;
				}
				int num5 = (int)Math.Round((float)value2 * num4);
				if (num5 != value2)
				{
					_unitCost[name] = num5;
					if (_internalByDisplay.TryGetValue(name, out string value3))
					{
						_unitCostByInternal[value3] = num5;
					}
					num2++;
				}
			}
			MelonLogger.Msg($"[KGT] Applied cost_mult overrides from Si_UnitBalance_Config.json: {num2} units changed, {num3} not in dump");
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] ApplyUnitBalanceCostOverrides failed: " + ex.Message);
		}
	}

	private static int SeedVanillaCostsFromConstructionData()
	{
		int num = 0;
		try
		{
			ConstructionData[] array = Resources.FindObjectsOfTypeAll<ConstructionData>();
			foreach (ConstructionData val in array)
			{
				if ((Object)(object)val == (Object)null || (Object)(object)val.ObjectInfo == (Object)null)
				{
					continue;
				}
				string displayName = val.ObjectInfo.DisplayName;
				if (!string.IsNullOrEmpty(displayName))
				{
					int resourceCost = val.ResourceCost;
					if (resourceCost > 0 && !_unitCostVanilla.ContainsKey(displayName))
					{
						_unitCostVanilla[displayName] = resourceCost;
						num++;
					}
				}
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] SeedVanillaCostsFromConstructionData failed: " + ex.Message);
		}
		return num;
	}

	private static void TickKohAliveCheck(float dt)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (!_hasKoh)
		{
			return;
		}
		_kohAliveCheckTimer -= dt;
		if (_kohAliveCheckTimer > 0f)
		{
			return;
		}
		_kohAliveCheckTimer = 3f;
		bool flag = (Object)(object)_kohTower == (Object)null || (Object)(object)((Component)_kohTower).gameObject == (Object)null;
		if (!flag && (Object)(object)_kohTower != (Object)null)
		{
			try
			{
				DamageManager component = ((Component)_kohTower).gameObject.GetComponent<DamageManager>();
				if ((Object)(object)component != (Object)null && component.IsDestroyed)
				{
					flag = true;
				}
			}
			catch
			{
			}
		}
		if (!flag)
		{
			return;
		}
		if ((Object)(object)_kohPrefabRef == (Object)null || (Object)(object)_kohTeam == (Object)null)
		{
			if (_kohRespawnCount == 0)
			{
				MelonLogger.Warning("[KGT] KoH destroyed but cache is empty — cannot auto-respawn. Capture continues by position.");
				_kohRespawnCount = -1;
			}
			return;
		}
		try
		{
			GameObject val = Game.SpawnPrefab(_kohPrefabRef, (Player)null, _kohTeam, _kohCenter, _kohRotation, true, true);
			if ((Object)(object)val != (Object)null)
			{
				_kohTower = val.transform;
				_kohRespawnCount = Mathf.Max(0, _kohRespawnCount) + 1;
				MelonLogger.Msg($"[KGT] KoH auto-respawned at ({_kohCenter.x:F0},{_kohCenter.z:F0}) — respawn #{_kohRespawnCount}");
				BroadcastAllChat("[KGT] <color=#39ff14>The Galactic Teleporter has rebuilt itself.</color>");
			}
			else
			{
				MelonLogger.Warning("[KGT] KoH respawn returned null GameObject");
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] KoH respawn threw: " + ex.Message);
		}
	}

	private static bool TrySubscribeMapBalance()
	{
		if (_subscribedReady)
		{
			return true;
		}
		try
		{
			if (_specialsType == null)
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (int i = 0; i < assemblies.Length; i++)
				{
					Type type = assemblies[i].GetType("Si_MapBalance.MapBalanceSpecials");
					if (type != null)
					{
						_specialsType = type;
						break;
					}
				}
			}
			if (_specialsType == null)
			{
				return false;
			}
			if ((object)_specialsKohField == null)
			{
				_specialsKohField = _specialsType.GetField("Koh", BindingFlags.Static | BindingFlags.Public);
			}
			if ((object)_specialsReadyEvent == null)
			{
				_specialsReadyEvent = _specialsType.GetEvent("OnSpecialsReady", BindingFlags.Static | BindingFlags.Public);
			}
			if (_specialsReadyEvent == null)
			{
				MelonLogger.Warning("[KGT] MapBalance.OnSpecialsReady event not found");
				return false;
			}
			Action handler = OnMapBalanceSpecialsReady;
			_specialsReadyEvent.AddEventHandler(null, handler);
			_subscribedReady = true;
			MelonLogger.Msg("[KGT] Subscribed to Si_MapBalance.OnSpecialsReady");
			return true;
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[KGT] TrySubscribeMapBalance failed: " + ex.Message);
			return false;
		}
	}

	private static void OnMapBalanceSpecialsReady()
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!ModeKohActive)
			{
				MelonLogger.Msg($"[KGT] OnMapBalanceSpecialsReady ignored — Mode={Cfg.Mode} (KoH gameplay disabled)");
				return;
			}
			ResetKohState();
			if (_specialsKohField == null)
			{
				return;
			}
			object value = _specialsKohField.GetValue(null);
			if (value == null)
			{
				MelonLogger.Msg("[KGT] No KoH configured for this map — running inert this round.");
				return;
			}
			Type type = value.GetType();
			object? obj = type.GetField("Obj")?.GetValue(value);
			GameObject val = (GameObject)((obj is GameObject) ? obj : null);
			if ((Object)(object)val == (Object)null)
			{
				MelonLogger.Warning("[KGT] MapBalance KoH.Obj is null");
				return;
			}
			_captureRadius = (float)(type.GetField("CaptureRadius")?.GetValue(value) ?? ((object)50f));
			_exclusionRadius = (float)(type.GetField("ExclusionRadius")?.GetValue(value) ?? ((object)0f));
			Scene activeScene = SceneManager.GetActiveScene();
			string text = activeScene.name ?? "";
			bool flag = !string.IsNullOrEmpty(text) && Cfg.WinThresholdPerMap != null && Cfg.WinThresholdPerMap.TryGetValue(text, out var value2) && value2 > 0f;
			_winThreshold = (flag ? Cfg.WinThresholdPerMap[text] : Cfg.WinThreshold);
			_kohTower = val.transform;
			_kohCenter = _kohTower.position;
			_kohRotation = _kohTower.rotation;
			_hasKoh = true;
			_kohRespawnCount = 0;
			_kohAliveCheckTimer = 3f;
			try
			{
				string text2 = ((Object)val).name ?? "";
				if (text2.EndsWith("(Clone)"))
				{
					text2 = text2.Substring(0, text2.Length - "(Clone)".Length);
				}
				int spawnablePrefabIndex = GameDatabase.GetSpawnablePrefabIndex(text2);
				_kohPrefabRef = ((spawnablePrefabIndex >= 0) ? GameDatabase.GetSpawnablePrefab(spawnablePrefabIndex) : null);
				BaseGameObject component = val.GetComponent<BaseGameObject>();
				_kohTeam = ((component != null) ? component.Team : null);
				if ((Object)(object)_kohPrefabRef == (Object)null)
				{
					MelonLogger.Warning("[KGT] KoH prefab '" + text2 + "' not in GameDatabase — auto-respawn disabled this round");
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[KGT] Caching KoH prefab/team failed: " + ex.Message);
			}
			ResetDamageDiag();
			MelonLogger.Msg($"[KGT] Tower acquired from MapBalance: {((Object)val).name} at ({_kohCenter.x:F0},{_kohCenter.z:F0}) " + $"R_capture={_captureRadius} R_excl={_exclusionRadius} " + string.Format("win_threshold={0:F0} ({1})", _winThreshold, flag ? ("per-map override for " + text) : "global default"));
			LogToReplay("kgt_koh_spawn", ("x", _kohCenter.x.ToString("F0")), ("z", _kohCenter.z.ToString("F0")), ("capture_radius", _captureRadius.ToString("F0")), ("exclusion_radius", _exclusionRadius.ToString("F0")), ("win_threshold", _winThreshold.ToString("F0")));
			LogToReplay("kgt_outpost_ring", ("center_x", _kohCenter.x.ToString("F0")), ("center_z", _kohCenter.z.ToString("F0")), ("radius", _captureRadius.ToString("F0")), ("count", Cfg.OutpostCount.ToString()), ("bury_depth", Cfg.OutpostBuryDepth.ToString("F0")));
			SpawnOutpostsSplit(null, 0);
		}
		catch (Exception ex2)
		{
			MelonLogger.Warning("[KGT] OnMapBalanceSpecialsReady failed: " + ex2.Message);
		}
	}

	private static void ResetKohState()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		DespawnOutposts();
		_kohTower = null;
		_kohCenter = Vector3.zero;
		_kohRotation = Quaternion.identity;
		_hasKoh = false;
		_neutralTeam = null;
		_kohPrefabRef = null;
		_kohTeam = null;
		_kohRespawnCount = 0;
		_kohAliveCheckTimer = 0f;
		ResetKingState();
	}

	private static bool InCaptureZone(Vector3 pos)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!_hasKoh)
		{
			return false;
		}
		float num = pos.x - _kohCenter.x;
		float num2 = pos.z - _kohCenter.z;
		return num * num + num2 * num2 <= _captureRadius * _captureRadius;
	}

	private static bool InExclusionZone(Vector3 pos)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!_hasKoh)
		{
			return false;
		}
		float num = pos.x - _kohCenter.x;
		float num2 = pos.z - _kohCenter.z;
		return num * num + num2 * num2 <= _exclusionRadius * _exclusionRadius;
	}

	private static Dictionary<Team, float> EvaluateScores()
	{
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		_playersInZonePrev.Clear();
		foreach (Player item in _playersInZone)
		{
			_playersInZonePrev.Add(item);
		}
		_playersInZone.Clear();
		Dictionary<Team, float> dictionary = new Dictionary<Team, float>();
		if (!_hasKoh || (Object)(object)_kohTower == (Object)null)
		{
			return dictionary;
		}
		Dictionary<string, float> perUnitOverride = Cfg.PerUnitOverride;
		Dictionary<string, float> categoryMultiplier = Cfg.CategoryMultiplier;
		Dictionary<string, string> buildingToCategory = Cfg.BuildingToCategory;
		float playerControlledMultiplier = Cfg.PlayerControlledMultiplier;
		string text = (string.IsNullOrEmpty(Cfg.DefaultCategoryForUnknown) ? "Infantry" : Cfg.DefaultCategoryForUnknown);
		List<Unit> units = Unit.Units;
		for (int i = 0; i < units.Count; i++)
		{
			Unit val = units[i];
			if ((Object)(object)val == (Object)null || ((BaseGameObject)val).IsDestroyed || (Object)(object)((BaseGameObject)val).ObjectInfo == (Object)null || (Object)(object)((BaseGameObject)val).Team == (Object)null || !InCaptureZone(((Component)val).transform.position))
			{
				continue;
			}
			int unitCapValue = ((BaseGameObject)val).ObjectInfo.UnitCapValue;
			if (unitCapValue <= 0)
			{
				continue;
			}
			string text2 = ((BaseGameObject)val).ObjectInfo.DisplayName ?? "";
			if (!perUnitOverride.TryGetValue(text2, out var value))
			{
				string value3;
				if (_unitToBuilding.TryGetValue(text2, out string value2) && !string.IsNullOrEmpty(value2))
				{
					if (!buildingToCategory.TryGetValue(value2, out value3))
					{
						value3 = text;
					}
				}
				else
				{
					value3 = "Auxiliary";
				}
				if (!categoryMultiplier.TryGetValue(value3, out value))
				{
					value = 1f;
				}
			}
			if (!(value <= 0f))
			{
				float num = (float)unitCapValue * value;
				if ((Object)(object)val.ControlledBy != (Object)null)
				{
					num *= playerControlledMultiplier;
					_playersInZone.Add(val.ControlledBy);
					GrantZonePresenceReward(val.ControlledBy, unitCapValue, text2);
				}
				if (dictionary.TryGetValue(((BaseGameObject)val).Team, out var value4))
				{
					dictionary[((BaseGameObject)val).Team] = value4 + num;
				}
				else
				{
					dictionary[((BaseGameObject)val).Team] = num;
				}
			}
		}
		return dictionary;
	}

	private static void ProcessPlayerNotifications()
	{
		if (!Cfg.EnterExitNotifications)
		{
			return;
		}
		foreach (Player item in _playersInZone)
		{
			if (!((Object)(object)item == (Object)null) && !_playersInZonePrev.Contains(item))
			{
				try
				{
					HelperMethods.SendChatMessageToPlayer(item, "[KGT] You have entered the king's domain.");
				}
				catch (Exception)
				{
				}
			}
		}
		foreach (Player item2 in _playersInZonePrev)
		{
			if (!((Object)(object)item2 == (Object)null) && !_playersInZone.Contains(item2))
			{
				try
				{
					HelperMethods.SendChatMessageToPlayer(item2, "[KGT] You have departed the king's domain.");
				}
				catch (Exception)
				{
				}
			}
		}
	}

	static KingOfTheHill()
	{
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
	}
}
