using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using MelonLoader;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Si_CrabCannon;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(CrabCannon), "Crab Cannon", "2.7.0", "schwe", null)]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_CrabCannon")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6686d98d0173c77ad21eb529a5cd8f2227b761ac")]
[assembly: AssemblyProduct("Si_CrabCannon")]
[assembly: AssemblyTitle("Si_CrabCannon")]
[assembly: AssemblyVersion("1.0.0.0")]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		public readonly byte[] NullableFlags;

		public NullableAttribute(byte P_0)
		{
			NullableFlags = new byte[1] { P_0 };
		}

		public NullableAttribute(byte[] P_0)
		{
			NullableFlags = P_0;
		}
	}
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableContextAttribute : Attribute
	{
		public readonly byte Flag;

		public NullableContextAttribute(byte P_0)
		{
			Flag = P_0;
		}
	}
}
namespace Si_CrabCannon
{
	public class CrabCannon : MelonMod
	{
		private class CrabCannonConfig
		{
			public bool Enabled = true;

			public float TriggerRadius = 15f;

			public float LaunchSpeed = 180f;

			public float LaunchAngle = 22.5f;

			public float CooldownSeconds = 10f;

			public int MinTier = 1;

			public bool CrabEnabled = true;

			public bool GoliathEnabled;

			public bool BehemothEnabled;

			public bool ScorpionEnabled;

			public bool HunterEnabled;

			public bool SuperEnabled = true;

			public int SuperTier = 8;

			public int SuperMaxCharges = 5;

			public float SuperRechargeTime = 360f;

			public float SuperCountdownInterval = 60f;

			public float SuperAngle = 20f;

			public float SuperSpeed = 180f;

			public bool PlayerAimAllowed = true;

			public bool CommanderAimAllowed = true;

			public float CannonCountdown = 3f;
		}

		private class CountdownState
		{
			public float EnterTime;

			public int LastAnnounced;

			public int NestId;

			public bool IsSuperLaunch;
		}

		private class FlightState
		{
			public Unit Unit;

			public Player Player;

			public float StartY;

			public float LaunchTime;

			public float MaxFlightTime;

			public bool PastApex;

			public int OrigLayer;

			public float DragOrig;

			public MonoBehaviour CreatureScript;

			public MonoBehaviour SensorScript;

			public Collider[] DisabledColliders;

			public bool CollidersRestored;

			public Vector3 PendingVelocity;

			public bool Launched;

			public float OrigTeleportDist;
		}

		private static string _configPath = "";

		private static float TriggerRadius = 15f;

		private static float LaunchSpeed = 180f;

		private static float LaunchAngle = 22.5f;

		private static float CooldownSeconds = 10f;

		private static int MinTier = 1;

		private static string CannonSoundFile = "sounds/cannon_boom.wav";

		private static bool CrabEnabled = true;

		private static bool GoliathEnabled = false;

		private static bool BehemothEnabled = false;

		private static bool ScorpionEnabled = false;

		private static bool HunterEnabled = false;

		private static bool SuperEnabled = true;

		private static int SuperTier = 8;

		private static int SuperMaxCharges = 5;

		private static float SuperRechargeTime = 360f;

		private static float SuperCountdownInterval = 60f;

		private static bool PlayerAimAllowed = true;

		private static bool CommanderAimAllowed = true;

		private static float CannonCountdown = 3f;

		private static readonly Dictionary<int, float> _cooldowns = new Dictionary<int, float>();

		private static readonly Dictionary<int, FlightState> _activeFlights = new Dictionary<int, FlightState>();

		private static readonly Dictionary<int, CountdownState> _countdowns = new Dictionary<int, CountdownState>();

		private const float G_EFF = 21.506f;

		private const float DRAG_H = 2.747f;

		private const float MAX_RANGE = 6000f;

		private static readonly Dictionary<int, float[]> _playerAim = new Dictionary<int, float[]>();

		private static bool _enabled = true;

		private static float _proximityCheckTimer = 0f;

		private const float PROXIMITY_CHECK_INTERVAL = 0.5f;

		private static bool _cannonTierAnnounced = false;

		private static int _superCharges = 0;

		private static bool _superReady = false;

		private static bool _superAnnounced = false;

		private static float _superRechargeTimer = 0f;

		private static bool _superRecharging = false;

		private static float _superLastCountdown = 0f;

		private static readonly HashSet<int> _superTeamsContributed = new HashSet<int>();

		private static float _superAngle = 20f;

		private static float _superSpeed = 180f;

		private static void OnCcAimCommand(Player? caller, string args)
		{
			if (caller == null)
			{
				return;
			}
			bool flag = caller.CanAdminExecute(Power.Generic);
			bool isCommander = caller.IsCommander;
			if (!flag)
			{
				if (isCommander && !CommanderAimAllowed)
				{
					HelperMethods.SendChatMessageToPlayer(caller, "Commander aim is locked by admin.");
					return;
				}
				if (!isCommander && !PlayerAimAllowed)
				{
					HelperMethods.SendChatMessageToPlayer(caller, "Player aim is locked. Only commanders/admins can change.");
					return;
				}
			}
			args = (args ?? "").Trim();
			string[] array = args.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string text = ((array.Length > 1) ? array[1] : "");
			string text2 = ((array.Length > 2) ? array[2] : "");
			if (string.IsNullOrEmpty(text))
			{
				int instanceID = caller.GetInstanceID();
				if (_playerAim.TryGetValue(instanceID, out float[] value))
				{
					float[] array2 = ComputeBallisticStats(value[1], value[0]);
					HelperMethods.SendChatMessageToPlayer(caller, $"Your cannon aim: angle={value[0]} speed={value[1]} -> peak={array2[0]:F0}m range={array2[1]:F0}m");
				}
				else
				{
					float[] array3 = ComputeBallisticStats(LaunchSpeed, LaunchAngle);
					float[] array4 = ComputeBallisticStats(_superSpeed, _superAngle);
					HelperMethods.SendChatMessageToPlayer(caller, $"Cannon defaults: angle={LaunchAngle} speed={LaunchSpeed} (peak={array3[0]:F0}m range={array3[1]:F0}m) | Super: angle={_superAngle} speed={_superSpeed} (peak={array4[0]:F0}m range={array4[1]:F0}m) | /ccaim <angle> <speed> to set your own");
				}
				return;
			}
			if (text.Equals("range", StringComparison.OrdinalIgnoreCase))
			{
				if (!TryParseFloat(text2, out var val) || val <= 0f)
				{
					HelperMethods.SendChatMessageToPlayer(caller, "Usage: /ccaim range <meters>");
					return;
				}
				if (val > 6000f)
				{
					HelperMethods.SendChatMessageToPlayer(caller, $"Denied: range {val:F0}m exceeds {6000f}m limit.");
					return;
				}
				int instanceID2 = caller.GetInstanceID();
				float num;
				string text3;
				if (_playerAim.TryGetValue(instanceID2, out float[] value2) && value2 != null && value2.Length >= 1)
				{
					num = value2[0];
					text3 = "personal";
				}
				else
				{
					num = _superAngle;
					text3 = "super default";
				}
				float num2 = ComputeSpeedForRange(num, val, out var achievedRange);
				if (num2 < 0f)
				{
					HelperMethods.SendChatMessageToPlayer(caller, $"Angle {num}° cannot achieve that range. Change angle first via /ccaim <angle> <speed>.");
				}
				else if (num2 > 800f)
				{
					HelperMethods.SendChatMessageToPlayer(caller, $"Denied: range {val:F0}m at angle {num}° needs speed {num2:F0} (max 800). Lower range or raise angle.");
				}
				else if (caller.IsCommander)
				{
					_superAngle = num;
					_superSpeed = num2;
					_playerAim.Remove(instanceID2);
					SaveConfig();
					SendTeamChat(caller.Team, $"[SUPER WEAPON] Commander set aim: angle={_superAngle} speed={_superSpeed:F0} -> range={achievedRange:F0}m ({text3})");
				}
				else
				{
					_playerAim[instanceID2] = new float[2] { num, num2 };
					HelperMethods.SendChatMessageToPlayer(caller, $"Your cannon aim set: angle={num} speed={num2:F0} -> range={achievedRange:F0}m ({text3})");
				}
				return;
			}
			if (!TryParseFloat(text, out var val2) || val2 <= 0f || val2 >= 90f)
			{
				HelperMethods.SendChatMessageToPlayer(caller, "Usage: /ccaim <angle 1-89> <speed>  OR  /ccaim range <meters>");
				return;
			}
			float val3 = _superSpeed;
			if (!string.IsNullOrEmpty(text2) && (!TryParseFloat(text2, out val3) || val3 <= 0f || val3 > 800f))
			{
				HelperMethods.SendChatMessageToPlayer(caller, "Usage: /ccaim <angle 1-89> <speed 1-800>");
				return;
			}
			float[] array5 = ComputeBallisticStats(val3, val2);
			if (array5[1] > 6000f)
			{
				HelperMethods.SendChatMessageToPlayer(caller, $"Denied: range={array5[1]:F0}m exceeds {6000f}m limit. Lower speed or angle.");
			}
			else if (caller.IsCommander)
			{
				_superAngle = val2;
				_superSpeed = val3;
				_playerAim.Remove(caller.GetInstanceID());
				SaveConfig();
				float[] array6 = ComputeBallisticStats(_superSpeed, _superAngle);
				SendTeamChat(caller.Team, $"[SUPER WEAPON] Commander set aim: angle={_superAngle} speed={_superSpeed} -> peak={array6[0]:F0}m range={array6[1]:F0}m");
			}
			else
			{
				int instanceID3 = caller.GetInstanceID();
				_playerAim[instanceID3] = new float[2] { val2, val3 };
				float[] array7 = ComputeBallisticStats(val3, val2);
				HelperMethods.SendChatMessageToPlayer(caller, $"Your cannon aim set: angle={val2} speed={val3} -> peak={array7[0]:F0}m range={array7[1]:F0}m");
			}
		}

		private static void OnCcCommand(Player? caller, string args)
		{
			if (caller != null && !caller.CanAdminExecute(Power.Generic))
			{
				HelperMethods.SendChatMessageToPlayer(caller, "Crab Cannon: admin only");
				return;
			}
			args = (args ?? "").Trim();
			string[] array = args.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string text = ((array.Length > 1) ? array[1].ToLower() : "");
			string s = ((array.Length > 2) ? array[2].Trim() : "");
			if (string.IsNullOrEmpty(text))
			{
				_enabled = !_enabled;
				SaveConfig();
				Reply(caller, "Crab Cannon: " + (_enabled ? "ON" : "OFF"));
				return;
			}
			switch (text)
			{
			case "speed":
			{
				if (TryParseFloat(s, out var val5) && val5 > 0f && val5 <= 800f)
				{
					float[] array4 = ComputeBallisticStats(val5, LaunchAngle);
					if (array4[1] > 6000f)
					{
						Reply(caller, $"Denied: range={array4[1]:F0}m exceeds {6000f}m limit. Lower speed or angle.");
						break;
					}
					LaunchSpeed = val5;
					SaveConfig();
					Reply(caller, $"Crab Cannon: speed={LaunchSpeed} -> peak={array4[0]:F0}m range={array4[1]:F0}m");
				}
				else
				{
					Reply(caller, "Crab Cannon: speed = " + LaunchSpeed + " (usage: /cc speed <number>)");
				}
				break;
			}
			case "range":
			case "superrange":
			{
				bool flag = text == "superrange";
				if (!TryParseFloat(s, out var val7) || val7 <= 0f)
				{
					Reply(caller, $"Usage: /cc {text} <meters>");
					break;
				}
				if (val7 > 6000f)
				{
					Reply(caller, $"Denied: range {val7:F0}m exceeds {6000f}m limit.");
					break;
				}
				float num = (flag ? _superAngle : LaunchAngle);
				float num2 = ComputeSpeedForRange(num, val7, out var achievedRange);
				if (num2 < 0f)
				{
					Reply(caller, $"Angle {num}° cannot achieve any range. Change angle first.");
					break;
				}
				if (num2 > 800f)
				{
					Reply(caller, $"Denied: range {val7:F0}m at angle {num}° needs speed {num2:F0} (max 800). Raise angle or lower range.");
					break;
				}
				if (flag)
				{
					_superSpeed = num2;
				}
				else
				{
					LaunchSpeed = num2;
				}
				SaveConfig();
				string text4 = (flag ? "Super" : "Default");
				Reply(caller, $"Crab Cannon {text4}: angle={num} speed={num2:F0} -> range={achievedRange:F0}m");
				break;
			}
			case "angle":
			{
				if (TryParseFloat(s, out var val4) && val4 > 0f && val4 < 90f)
				{
					float[] array3 = ComputeBallisticStats(LaunchSpeed, val4);
					if (array3[1] > 6000f)
					{
						Reply(caller, $"Denied: range={array3[1]:F0}m exceeds {6000f}m limit. Lower speed or angle.");
						break;
					}
					LaunchAngle = val4;
					SaveConfig();
					Reply(caller, $"Crab Cannon: angle={LaunchAngle} -> peak={array3[0]:F0}m range={array3[1]:F0}m");
				}
				else
				{
					Reply(caller, "Crab Cannon: angle = " + LaunchAngle + " (usage: /cc angle <0-90>)");
				}
				break;
			}
			case "cooldown":
			{
				if (TryParseFloat(s, out var val3) && val3 >= 0f)
				{
					CooldownSeconds = val3;
					SaveConfig();
					Reply(caller, "Crab Cannon: cooldown = " + CooldownSeconds + "s");
				}
				else
				{
					Reply(caller, "Crab Cannon: cooldown = " + CooldownSeconds + "s");
				}
				break;
			}
			case "radius":
			{
				if (TryParseFloat(s, out var val6) && val6 > 0f)
				{
					TriggerRadius = val6;
					SaveConfig();
					Reply(caller, "Crab Cannon: radius = " + TriggerRadius + "m");
				}
				else
				{
					Reply(caller, "Crab Cannon: radius = " + TriggerRadius + "m");
				}
				break;
			}
			case "tier":
			{
				if (int.TryParse(s, out var result2) && result2 >= 0)
				{
					MinTier = result2;
					SaveConfig();
					Reply(caller, "Crab Cannon: min tier = " + MinTier);
				}
				else
				{
					Reply(caller, "Crab Cannon: min tier = " + MinTier);
				}
				break;
			}
			case "crab":
				CrabEnabled = !CrabEnabled;
				SaveConfig();
				Reply(caller, "crab = " + (CrabEnabled ? "ON" : "OFF"));
				break;
			case "goliath":
				GoliathEnabled = !GoliathEnabled;
				SaveConfig();
				Reply(caller, "goliath = " + (GoliathEnabled ? "ON" : "OFF"));
				break;
			case "behemoth":
				BehemothEnabled = !BehemothEnabled;
				SaveConfig();
				Reply(caller, "behemoth = " + (BehemothEnabled ? "ON" : "OFF"));
				break;
			case "scorpion":
				ScorpionEnabled = !ScorpionEnabled;
				SaveConfig();
				Reply(caller, "scorpion = " + (ScorpionEnabled ? "ON" : "OFF"));
				break;
			case "hunter":
				HunterEnabled = !HunterEnabled;
				SaveConfig();
				Reply(caller, "hunter = " + (HunterEnabled ? "ON" : "OFF"));
				break;
			case "super":
				SuperEnabled = !SuperEnabled;
				SaveConfig();
				Reply(caller, "Super weapon = " + (SuperEnabled ? "ON" : "OFF"));
				break;
			case "countdown":
			{
				if (TryParseFloat(s, out var val) && val >= 0f)
				{
					CannonCountdown = val;
					SaveConfig();
					Reply(caller, "Cannon countdown = " + CannonCountdown + "s");
				}
				else
				{
					Reply(caller, "Cannon countdown = " + CannonCountdown + "s (usage: /cc countdown <seconds>)");
				}
				break;
			}
			case "playeraim":
				PlayerAimAllowed = !PlayerAimAllowed;
				SaveConfig();
				Reply(caller, "Player aim = " + (PlayerAimAllowed ? "ALLOWED" : "LOCKED"));
				break;
			case "commanderaim":
				CommanderAimAllowed = !CommanderAimAllowed;
				SaveConfig();
				Reply(caller, "Commander aim = " + (CommanderAimAllowed ? "ALLOWED" : "LOCKED"));
				break;
			case "supercharges":
			{
				if (int.TryParse(s, out var result) && result > 0)
				{
					SuperMaxCharges = result;
					SaveConfig();
					Reply(caller, "Super weapon charges = " + SuperMaxCharges);
				}
				else
				{
					Reply(caller, "Super weapon charges = " + SuperMaxCharges);
				}
				break;
			}
			case "supercd":
			{
				if (TryParseFloat(s, out var val2) && val2 > 0f)
				{
					SuperRechargeTime = val2;
					SaveConfig();
					Reply(caller, "Super weapon recharge = " + SuperRechargeTime + "s");
				}
				else
				{
					Reply(caller, "Super weapon recharge = " + SuperRechargeTime + "s");
				}
				break;
			}
			case "on":
				_enabled = true;
				SaveConfig();
				Reply(caller, "Crab Cannon: ON");
				break;
			case "off":
				_enabled = false;
				SaveConfig();
				Reply(caller, "Crab Cannon: OFF");
				break;
			case "status":
			{
				float[] array2 = ComputeBallisticStats(LaunchSpeed, LaunchAngle);
				string text2 = string.Format("crab={0} gol={1} beh={2} scorp={3} hunt={4}", CrabEnabled ? "+" : "-", GoliathEnabled ? "+" : "-", BehemothEnabled ? "+" : "-", ScorpionEnabled ? "+" : "-", HunterEnabled ? "+" : "-");
				string text3 = (SuperEnabled ? $"SUPER: {_superCharges}/{SuperMaxCharges} charges, cd={SuperRechargeTime}s, aim={_superAngle}/{_superSpeed}" : "SUPER: OFF");
				Reply(caller, string.Format("CC: {0} | spd={1} ang={2} pk={3:F0}m rng={4:F0}m cd={5}s r={6}m t>={7} | {8} | {9}", _enabled ? "ON" : "OFF", LaunchSpeed, LaunchAngle, array2[0], array2[1], CooldownSeconds, TriggerRadius, MinTier, text2, text3));
				break;
			}
			default:
				Reply(caller, "/cc [on|off|speed|angle|range|superrange|cooldown|radius|tier|status|crab|goliath|behemoth|scorpion|hunter|super|supercharges|supercd]");
				break;
			}
		}

		private static void LoadConfig()
		{
			try
			{
				if (File.Exists(_configPath))
				{
					CrabCannonConfig crabCannonConfig = JsonConvert.DeserializeObject<CrabCannonConfig>(File.ReadAllText(_configPath));
					if (crabCannonConfig != null)
					{
						_enabled = crabCannonConfig.Enabled;
						TriggerRadius = crabCannonConfig.TriggerRadius;
						LaunchSpeed = crabCannonConfig.LaunchSpeed;
						LaunchAngle = crabCannonConfig.LaunchAngle;
						CooldownSeconds = crabCannonConfig.CooldownSeconds;
						MinTier = crabCannonConfig.MinTier;
						CrabEnabled = crabCannonConfig.CrabEnabled;
						GoliathEnabled = crabCannonConfig.GoliathEnabled;
						BehemothEnabled = crabCannonConfig.BehemothEnabled;
						ScorpionEnabled = crabCannonConfig.ScorpionEnabled;
						HunterEnabled = crabCannonConfig.HunterEnabled;
						SuperEnabled = crabCannonConfig.SuperEnabled;
						SuperTier = crabCannonConfig.SuperTier;
						SuperMaxCharges = crabCannonConfig.SuperMaxCharges;
						SuperRechargeTime = crabCannonConfig.SuperRechargeTime;
						SuperCountdownInterval = crabCannonConfig.SuperCountdownInterval;
						_superAngle = crabCannonConfig.SuperAngle;
						_superSpeed = crabCannonConfig.SuperSpeed;
						PlayerAimAllowed = crabCannonConfig.PlayerAimAllowed;
						CommanderAimAllowed = crabCannonConfig.CommanderAimAllowed;
						CannonCountdown = crabCannonConfig.CannonCountdown;
						MelonLogger.Msg("CrabCannon: Config loaded from " + _configPath);
						return;
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("CrabCannon: Failed to load config: " + ex.Message);
			}
			SaveConfig();
			MelonLogger.Msg("CrabCannon: Default config saved to " + _configPath);
		}

		private static void SaveConfig()
		{
			try
			{
				CrabCannonConfig value = new CrabCannonConfig
				{
					Enabled = _enabled,
					TriggerRadius = TriggerRadius,
					LaunchSpeed = LaunchSpeed,
					LaunchAngle = LaunchAngle,
					CooldownSeconds = CooldownSeconds,
					MinTier = MinTier,
					CrabEnabled = CrabEnabled,
					GoliathEnabled = GoliathEnabled,
					BehemothEnabled = BehemothEnabled,
					ScorpionEnabled = ScorpionEnabled,
					HunterEnabled = HunterEnabled,
					SuperEnabled = SuperEnabled,
					SuperTier = SuperTier,
					SuperMaxCharges = SuperMaxCharges,
					SuperRechargeTime = SuperRechargeTime,
					SuperCountdownInterval = SuperCountdownInterval,
					SuperAngle = _superAngle,
					SuperSpeed = _superSpeed,
					PlayerAimAllowed = PlayerAimAllowed,
					CommanderAimAllowed = CommanderAimAllowed,
					CannonCountdown = CannonCountdown
				};
				string directoryName = Path.GetDirectoryName(_configPath);
				if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				File.WriteAllText(_configPath, JsonConvert.SerializeObject(value, Formatting.Indented));
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("CrabCannon: Failed to save config: " + ex.Message);
			}
		}

		private void CheckProximityTriggers()
		{
			List<Structure> structures = Structure.Structures;
			if (structures == null || structures.Count == 0)
			{
				return;
			}
			HashSet<int> hashSet = new HashSet<int>();
			for (int i = 0; i < Player.Players.Count; i++)
			{
				Player player = Player.Players[i];
				if (player == null)
				{
					continue;
				}
				Unit controlledUnit = player.ControlledUnit;
				if (controlledUnit == null || controlledUnit.IsDestroyed)
				{
					continue;
				}
				int instanceID = player.GetInstanceID();
				if (_activeFlights.ContainsKey(instanceID))
				{
					continue;
				}
				if (_cooldowns.TryGetValue(instanceID, out var value) && Time.time < value)
				{
					if (IsNearNest(controlledUnit, structures))
					{
						int num = Mathf.CeilToInt(value - Time.time);
						if (_countdowns.TryGetValue(instanceID, out CountdownState value2) && value2.LastAnnounced != num)
						{
							value2.LastAnnounced = num;
							HelperMethods.SendChatMessageToPlayer(player, $"[CANNON] Cooldown: {num}s");
						}
						else if (!_countdowns.ContainsKey(instanceID))
						{
							_countdowns[instanceID] = new CountdownState
							{
								LastAnnounced = num
							};
							HelperMethods.SendChatMessageToPlayer(player, $"[CANNON] Cooldown: {num}s");
						}
					}
					continue;
				}
				bool flag = false;
				bool flag2 = false;
				string text = ((controlledUnit.ObjectInfo != null) ? controlledUnit.ObjectInfo.name : "");
				if (SuperEnabled && _superReady && _superCharges > 0 && text.Contains("Goliath") && controlledUnit.Team != null && controlledUnit.Team.TechnologyTier >= SuperTier)
				{
					flag = true;
				}
				if (!flag && IsLaunchableUnit(controlledUnit))
				{
					flag2 = ((MinTier <= 0 || !(controlledUnit.Team != null) || controlledUnit.Team.TechnologyTier >= MinTier) ? true : false);
				}
				if (!flag && !flag2)
				{
					continue;
				}
				Structure structure = null;
				Vector3 position = controlledUnit.transform.position;
				for (int j = 0; j < structures.Count; j++)
				{
					Structure structure2 = structures[j];
					if (!(structure2 == null) && !structure2.IsDestroyed && !(structure2.Team != controlledUnit.Team) && IsNest(structure2) && Vector3.Distance(position, structure2.transform.position) <= TriggerRadius)
					{
						structure = structure2;
						break;
					}
				}
				if (structure == null)
				{
					_countdowns.Remove(instanceID);
					continue;
				}
				hashSet.Add(instanceID);
				int instanceID2 = structure.GetInstanceID();
				if (!_countdowns.TryGetValue(instanceID, out CountdownState value3) || value3.NestId != instanceID2)
				{
					value3 = new CountdownState
					{
						EnterTime = Time.time,
						LastAnnounced = -1,
						NestId = instanceID2,
						IsSuperLaunch = flag
					};
					_countdowns[instanceID] = value3;
				}
				float num2 = Time.time - value3.EnterTime;
				float num3 = CannonCountdown - num2;
				if (num3 <= 0f)
				{
					_countdowns.Remove(instanceID);
					if (flag)
					{
						SuperLaunch(player, controlledUnit, structure);
						continue;
					}
					float speed = LaunchSpeed;
					float angle = LaunchAngle;
					if (_playerAim.TryGetValue(instanceID, out float[] value4))
					{
						angle = value4[0];
						speed = value4[1];
					}
					LaunchUnit(player, controlledUnit, structure, speed, angle);
				}
				else
				{
					int num4 = Mathf.CeilToInt(num3);
					if (num4 != value3.LastAnnounced)
					{
						value3.LastAnnounced = num4;
						string arg = (flag ? "SUPER WEAPON" : "Cannon");
						HelperMethods.SendChatMessageToPlayer(player, $"[{arg}] Launching in {num4}s...");
					}
				}
			}
			List<int> list = new List<int>();
			foreach (KeyValuePair<int, CountdownState> countdown in _countdowns)
			{
				if (!hashSet.Contains(countdown.Key) && !_cooldowns.ContainsKey(countdown.Key))
				{
					list.Add(countdown.Key);
				}
			}
			foreach (int item in list)
			{
				_countdowns.Remove(item);
			}
		}

		private void LaunchUnit(Player player, Unit unit, Structure nest, float speed, float angle)
		{
			int instanceID = player.GetInstanceID();
			Vector3 position = unit.transform.position;
			Vector3 vector = position - nest.transform.position;
			vector.y = 0f;
			if (vector.sqrMagnitude < 0.01f)
			{
				vector = unit.transform.forward;
			}
			vector.Normalize();
			float f = angle * (MathF.PI / 180f);
			float num = speed * Mathf.Cos(f);
			float num2 = speed * Mathf.Sin(f);
			Vector3 pendingVelocity = vector * num + Vector3.up * num2;
			float num3 = 2f * num2 / 9.81f;
			NetworkComponent networkComponent = unit.NetworkComponent;
			if (networkComponent != null)
			{
				networkComponent.SetPlayerOwner(null);
			}
			float origTeleportDist = 0f;
			Rigidbody component = unit.GetComponent<Rigidbody>();
			float dragOrig = 0f;
			if (component != null)
			{
				dragOrig = component.linearDamping;
				component.linearDamping = 0f;
				component.angularVelocity = Vector3.zero;
				component.linearVelocity = Vector3.zero;
			}
			int layer = unit.gameObject.layer;
			unit.gameObject.layer = GamePhysics.LAYER_NOCOLLIDE;
			MonoBehaviour creatureScript = null;
			CreatureDecapod component2 = unit.GetComponent<CreatureDecapod>();
			if (component2 != null)
			{
				creatureScript = component2;
				component2.enabled = false;
			}
			MonoBehaviour sensorScript = null;
			Sensor component3 = unit.GetComponent<Sensor>();
			if (component3 != null)
			{
				sensorScript = component3;
				component3.enabled = false;
			}
			Collider[] componentsInChildren = unit.GetComponentsInChildren<Collider>();
			List<Collider> list = new List<Collider>();
			Collider[] array = componentsInChildren;
			foreach (Collider collider in array)
			{
				if (collider.enabled)
				{
					collider.enabled = false;
					list.Add(collider);
				}
			}
			float maxFlightTime = Mathf.Max(num3 * 2f, 5f);
			FlightState value = new FlightState
			{
				Unit = unit,
				Player = player,
				StartY = position.y,
				LaunchTime = Time.time,
				MaxFlightTime = maxFlightTime,
				PastApex = false,
				OrigLayer = layer,
				DragOrig = dragOrig,
				CreatureScript = creatureScript,
				SensorScript = sensorScript,
				DisabledColliders = list.ToArray(),
				PendingVelocity = pendingVelocity,
				Launched = false,
				OrigTeleportDist = origTeleportDist
			};
			_activeFlights[instanceID] = value;
			_cooldowns[instanceID] = Time.time + CooldownSeconds;
			unit.transform.rotation = Quaternion.LookRotation(vector + Vector3.up * 0.3f);
			PlayCannonSound(player);
			string text = ((unit.ObjectInfo != null) ? unit.ObjectInfo.name : "?");
			float[] array2 = ComputeBallisticStats(speed, angle);
			MelonLogger.Msg($"CANNON! {player.PlayerName} ({text}) launched! Speed={speed} Angle={angle} ExpPeak={array2[0]:F0}m ExpRange={array2[1]:F0}m");
		}

		private void MonitorFlights()
		{
			if (_activeFlights.Count == 0)
			{
				return;
			}
			List<int> list = new List<int>();
			foreach (KeyValuePair<int, FlightState> activeFlight in _activeFlights)
			{
				FlightState value = activeFlight.Value;
				if (value.Unit == null || value.Unit.IsDestroyed)
				{
					list.Add(activeFlight.Key);
					continue;
				}
				float num = Time.time - value.LaunchTime;
				if (!value.Launched && num >= 0.3f)
				{
					value.Launched = true;
					value.StartY = value.Unit.transform.position.y;
					Rigidbody component = value.Unit.GetComponent<Rigidbody>();
					if (component != null)
					{
						component.AddForce(value.PendingVelocity, ForceMode.VelocityChange);
					}
					if (value.Unit.NetworkTransformComponent != null)
					{
						value.Unit.NetworkTransformComponent.OnTeleport();
					}
				}
				if (!value.Launched)
				{
					continue;
				}
				float y = value.Unit.transform.position.y;
				Rigidbody component2 = value.Unit.GetComponent<Rigidbody>();
				float num2 = ((component2 != null) ? component2.linearVelocity.y : 0f);
				if (!value.PastApex && num2 < 0f && num > 0.8f)
				{
					value.PastApex = true;
				}
				if (value.PastApex && !value.CollidersRestored && y <= value.StartY + 50f)
				{
					value.CollidersRestored = true;
					value.Unit.gameObject.layer = value.OrigLayer;
					if (value.DisabledColliders != null)
					{
						Collider[] disabledColliders = value.DisabledColliders;
						foreach (Collider collider in disabledColliders)
						{
							if (collider != null)
							{
								collider.enabled = true;
							}
						}
					}
				}
				bool flag = y <= value.StartY + 5f;
				bool flag2 = value.PastApex && num2 >= -1f && num2 <= 1f && num > 1.5f;
				bool num3 = value.PastApex && (flag | flag2) && num > 1f;
				bool flag3 = num > value.MaxFlightTime;
				if (num3 | flag3)
				{
					Land(value);
					if (value.Player != null)
					{
						_cooldowns[value.Player.GetInstanceID()] = Time.time + CooldownSeconds;
					}
					list.Add(activeFlight.Key);
					MelonLogger.Msg(string.Format("Cannon {0}! Player: {1} ({2:F1}s)", flag3 ? "timeout" : "landed", value.Player?.PlayerName ?? "?", num));
				}
			}
			foreach (int item in list)
			{
				_activeFlights.Remove(item);
			}
		}

		private static void Land(FlightState flight)
		{
			if (flight.Unit == null || flight.Unit.IsDestroyed)
			{
				return;
			}
			if (flight.CreatureScript != null)
			{
				flight.CreatureScript.enabled = true;
			}
			if (flight.SensorScript != null)
			{
				flight.SensorScript.enabled = true;
			}
			if (!flight.CollidersRestored)
			{
				if (flight.DisabledColliders != null)
				{
					Collider[] disabledColliders = flight.DisabledColliders;
					foreach (Collider collider in disabledColliders)
					{
						if (collider != null)
						{
							collider.enabled = true;
						}
					}
				}
				flight.Unit.gameObject.layer = flight.OrigLayer;
			}
			Rigidbody component = flight.Unit.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.linearVelocity = Vector3.zero;
				component.angularVelocity = Vector3.zero;
				component.linearDamping = flight.DragOrig;
			}
			if (flight.Player != null)
			{
				NetworkComponent networkComponent = flight.Unit.NetworkComponent;
				if (networkComponent != null)
				{
					networkComponent.SetPlayerOwner(flight.Player);
				}
			}
		}

		private static float ComputeSpeedForRange(float angleDeg, float rangeMeters, out float achievedRange)
		{
			achievedRange = 0f;
			float num = angleDeg * (MathF.PI / 180f);
			float num2 = Mathf.Sin(2f * num);
			float num3 = Mathf.Sin(num);
			float num4 = num2 / 21.506f - 5.494f * num3 * num3 / 462.50806f;
			if (num4 <= 0f)
			{
				return -1f;
			}
			float num5 = Mathf.Sqrt(rangeMeters / num4);
			float[] array = ComputeBallisticStats(num5, angleDeg);
			achievedRange = array[1];
			return num5;
		}

		private static float[] ComputeBallisticStats(float spd, float angleDeg)
		{
			float f = angleDeg * (MathF.PI / 180f);
			float num = spd * Mathf.Sin(f);
			float num2 = spd * Mathf.Cos(f);
			float num3 = num * num / 43.012f;
			float num4 = 2f * num / 21.506f;
			float num5 = num2 * num4 - 1.3735f * num4 * num4;
			if (num5 < 0f)
			{
				num5 = 0f;
			}
			return new float[3] { num3, num5, num4 };
		}

		private static void Reply(Player? player, string msg)
		{
			if (player != null)
			{
				HelperMethods.SendChatMessageToPlayer(player, msg);
			}
			MelonLogger.Msg("CrabCannon: " + msg);
		}

		private static bool TryParseFloat(string s, out float val)
		{
			return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out val);
		}

		private static void SendTeamChat(Team team, string msg)
		{
			if (team == null)
			{
				return;
			}
			for (int i = 0; i < Player.Players.Count; i++)
			{
				Player player = Player.Players[i];
				if (player != null && player.Team == team && player != NetworkGameServer.GetServerPlayer())
				{
					HelperMethods.SendChatMessageToPlayer(player, msg);
				}
			}
			MelonLogger.Msg("CrabCannon [TeamChat]: " + msg);
		}

		private void PlayCannonSound(Player player)
		{
			try
			{
				AudioHelper.PlaySoundFile(CannonSoundFile, player);
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("CrabCannon sound failed: " + ex.Message);
			}
		}

		private bool IsLaunchableUnit(Unit unit)
		{
			if (unit == null || unit.ObjectInfo == null)
			{
				return false;
			}
			string name = unit.ObjectInfo.name;
			if (CrabEnabled && name.Contains("Crab"))
			{
				return true;
			}
			if (GoliathEnabled && name.Contains("Goliath"))
			{
				return true;
			}
			if (BehemothEnabled && name.Contains("Behemoth"))
			{
				return true;
			}
			if (ScorpionEnabled && name.Contains("Scorpion"))
			{
				return true;
			}
			if (HunterEnabled && name.Contains("Hunter"))
			{
				return true;
			}
			return false;
		}

		private bool IsNest(Structure structure)
		{
			if (structure == null || structure.ObjectInfo == null)
			{
				return false;
			}
			return structure.ObjectInfo.name.Contains("Nest");
		}

		private static bool IsAlienSideTeamName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}
			if (!name.Contains("Alien") && !name.Contains("Wildlife"))
			{
				return name.Contains("Worm");
			}
			return true;
		}

		private static Team FindAlienTeam()
		{
			Team team = null;
			List<Team> list = FindAllAlienTeams();
			for (int i = 0; i < list.Count; i++)
			{
				if (team == null || list[i].TechnologyTier > team.TechnologyTier)
				{
					team = list[i];
				}
			}
			return team;
		}

		private static List<Team> FindAllAlienTeams()
		{
			List<Team> list = new List<Team>();
			for (int i = 0; i < Team.Teams.Count; i++)
			{
				Team team = Team.Teams[i];
				if (!(team == null) && IsAlienSideTeamName(team.TeamName))
				{
					list.Add(team);
				}
			}
			return list;
		}

		private void CheckCannonTierAnnouncement()
		{
			if (_cannonTierAnnounced || MinTier <= 0)
			{
				return;
			}
			List<Team> list = FindAllAlienTeams();
			if (list.Count == 0)
			{
				return;
			}
			bool flag = false;
			foreach (Team item in list)
			{
				if (item.TechnologyTier >= MinTier)
				{
					float[] array = ComputeBallisticStats(LaunchSpeed, LaunchAngle);
					SendTeamChat(item, $"[CANNON] Crab Cannon unlocked! Walk a crab to the Nest to launch. Use /ccaim <angle> <speed> to aim (peak={array[0]:F0}m range={array[1]:F0}m).");
					flag = true;
				}
			}
			if (flag)
			{
				_cannonTierAnnounced = true;
			}
		}

		private bool IsNearNest(Unit unit, List<Structure> structures)
		{
			Vector3 position = unit.transform.position;
			for (int i = 0; i < structures.Count; i++)
			{
				Structure structure = structures[i];
				if (!(structure == null) && !structure.IsDestroyed && !(structure.Team != unit.Team) && IsNest(structure) && Vector3.Distance(position, structure.transform.position) <= TriggerRadius)
				{
					return true;
				}
			}
			return false;
		}

		public override void OnInitializeMelon()
		{
			_configPath = Path.Combine("UserData", "CrabCannon_Config.json");
			LoadConfig();
			MelonLogger.Msg("Crab Cannon v2.7.0 loaded! (real physics + super weapon)");
			GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Combine(GameEvents.OnGameEnded, new Action<GameMode, Team>(OnGameEnded));
		}

		public override void OnLateInitializeMelon()
		{
			PlayerMethods.RegisterPlayerCommand("cc", OnCcCommand, hideFromChat: true);
			PlayerMethods.RegisterPlayerCommand("ccaim", OnCcAimCommand, hideFromChat: true);
			MelonLogger.Msg("CrabCannon: Registered /cc and /ccaim commands.");
		}

		private static void OnGameEnded(GameMode mode, Team winner)
		{
			foreach (KeyValuePair<int, FlightState> activeFlight in _activeFlights)
			{
				Land(activeFlight.Value);
			}
			_activeFlights.Clear();
			_cooldowns.Clear();
			_countdowns.Clear();
			_cannonTierAnnounced = false;
			_superCharges = 0;
			_superReady = false;
			_superAnnounced = false;
			_superRecharging = false;
			_superRechargeTimer = 0f;
			_superTeamsContributed.Clear();
			_playerAim.Clear();
			MelonLogger.Msg("CrabCannon: Round ended, cleared state.");
		}

		public override void OnUpdate()
		{
			if (!NetworkGameServer.GetServerStarted())
			{
				return;
			}
			if (_enabled)
			{
				_proximityCheckTimer -= Time.deltaTime;
				if (_proximityCheckTimer <= 0f)
				{
					_proximityCheckTimer = 0.5f;
					CheckProximityTriggers();
				}
			}
			MonitorFlights();
			CheckCannonTierAnnouncement();
			if (SuperEnabled)
			{
				UpdateSuperWeapon();
			}
		}

		private void UpdateSuperWeapon()
		{
			List<Team> list = FindAllAlienTeams();
			if (list.Count == 0)
			{
				return;
			}
			foreach (Team item in list)
			{
				if (item.TechnologyTier >= SuperTier)
				{
					int index = item.Index;
					if (!_superTeamsContributed.Contains(index))
					{
						_superTeamsContributed.Add(index);
						_superCharges += SuperMaxCharges;
						_superReady = true;
						_superRecharging = false;
						_superAnnounced = true;
						SendTeamChat(item, $"[SUPER WEAPON] ONLINE! {SuperMaxCharges} Goliath cannons added to the pool (total {_superCharges}). Commander: use /ccaim <angle> <speed> to aim.");
						MelonLogger.Msg($"CrabCannon: Super weapon activated for {item.TeamShortName} (pool now {_superCharges}).");
					}
				}
			}
			if (!_superAnnounced || !_superRecharging)
			{
				return;
			}
			_superRechargeTimer -= Time.deltaTime;
			_superLastCountdown -= Time.deltaTime;
			if (_superLastCountdown <= 0f)
			{
				_superLastCountdown = SuperCountdownInterval;
				int num = Mathf.CeilToInt(_superRechargeTimer);
				int num2 = num / 60;
				int num3 = num % 60;
				foreach (Team item2 in list)
				{
					if (_superTeamsContributed.Contains(item2.Index))
					{
						SendTeamChat(item2, $"[SUPER WEAPON] Recharging... {num2}:{num3:D2} remaining");
					}
				}
			}
			if (!(_superRechargeTimer <= 0f))
			{
				return;
			}
			_superRecharging = false;
			int num4 = (_superCharges = SuperMaxCharges * _superTeamsContributed.Count);
			_superReady = true;
			float[] array = ComputeBallisticStats(_superSpeed, _superAngle);
			foreach (Team item3 in list)
			{
				if (_superTeamsContributed.Contains(item3.Index))
				{
					SendTeamChat(item3, $"[SUPER WEAPON] RECHARGED! {num4} Goliath cannons ready! Aim: angle={_superAngle} speed={_superSpeed} (peak={array[0]:F0}m range={array[1]:F0}m). Use /ccaim <angle> <speed> to adjust.");
				}
			}
		}

		private void SuperLaunch(Player player, Unit unit, Structure nest)
		{
			int instanceID = player.GetInstanceID();
			float num = _superAngle;
			float num2 = _superSpeed;
			if (_playerAim.TryGetValue(instanceID, out float[] value))
			{
				num = value[0];
				num2 = value[1];
			}
			MelonLogger.Msg(string.Format("SUPER LAUNCH: player={0} aimSpeed={1} aimAngle={2} (team: {3}/{4}, playerOverride: {5})", player.PlayerName, num2, num, _superSpeed, _superAngle, _playerAim.ContainsKey(instanceID) ? "YES" : "no"));
			LaunchUnit(player, unit, nest, num2, num);
			_superCharges--;
			float[] array = ComputeBallisticStats(num2, num);
			int num3 = SuperMaxCharges * Math.Max(1, _superTeamsContributed.Count);
			SendTeamChat(player.Team, $"[SUPER WEAPON] {player.PlayerName} launched Goliath! {_superCharges}/{num3} remaining | spd={num2} ang={num} peak={array[0]:F0}m range={array[1]:F0}m");
			if (_superCharges <= 0)
			{
				_superReady = false;
				_superRecharging = true;
				_superRechargeTimer = SuperRechargeTime;
				_superLastCountdown = SuperCountdownInterval;
				int num4 = (int)SuperRechargeTime / 60;
				SendTeamChat(player.Team, $"[SUPER WEAPON] All charges spent! Recharging in {num4} minutes...");
			}
		}
	}
}
