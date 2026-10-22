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
using Si_HeavyTeleporter;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(HeavyTeleporter), "Heavy Teleporter", "1.2.0", "schwe", null)]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_HeavyTeleporter")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+3aa81f9e5ff912c8cf378a59776b1b0ad7b1ccd1")]
[assembly: AssemblyProduct("Si_HeavyTeleporter")]
[assembly: AssemblyTitle("Si_HeavyTeleporter")]
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
namespace Si_HeavyTeleporter
{
	public class HeavyTeleporter : MelonMod
	{
		private class TeleporterConfig
		{
			public bool Enabled = true;

			public int TechTier = 8;

			public int MaxCharges = 5;

			public float RechargeTime = 300f;

			public float RechargeAnnounceInterval = 60f;

			public float TeleportRadius = 30f;

			public float Countdown = 3f;

			public string SoundFile = "sounds/cannon_boom.wav";

			public float MinEnemyBaseDistance = 800f;
		}

		private class TeamState
		{
			public int Charges;

			public bool Ready;

			public bool Announced;

			public bool Recharging;

			public float RechargeTimer;

			public float LastRechargeAnnounce;
		}

		private class VehicleCandidate
		{
			public Vehicle Vehicle;

			public string Name;

			public string FactoryLabel;
		}

		private class TeleportRequest
		{
			public Player Requester;

			public Player Target;

			public Vector3 TargetPosition;

			public bool UseCoords;

			public float RequestTime;

			public int LastAnnounced;

			public Vehicle ChosenVehicle;
		}

		private static string _configPath = "";

		private static bool _enabled = true;

		private static int TechTier = 8;

		private static int MaxCharges = 5;

		private static float RechargeTime = 300f;

		private static float RechargeAnnounceInterval = 60f;

		private static float TeleportRadius = 30f;

		private static float Countdown = 3f;

		private static string SoundFile = "sounds/cannon_boom.wav";

		private static float MinEnemyBaseDistance = 800f;

		private static readonly Dictionary<int, TeamState> _teamStates = new Dictionary<int, TeamState>();

		private static readonly Dictionary<int, List<VehicleCandidate>> _vehicleMenus = new Dictionary<int, List<VehicleCandidate>>();

		private static readonly Dictionary<int, TeleportRequest> _pendingRequests = new Dictionary<int, TeleportRequest>();

		private static TeamState GetTeamState(Team team)
		{
			int instanceID = ((Object)team).GetInstanceID();
			if (!_teamStates.TryGetValue(instanceID, out TeamState value))
			{
				value = new TeamState();
				_teamStates[instanceID] = value;
			}
			return value;
		}

		private static void LoadConfig()
		{
			try
			{
				if (File.Exists(_configPath))
				{
					TeleporterConfig teleporterConfig = JsonConvert.DeserializeObject<TeleporterConfig>(File.ReadAllText(_configPath));
					if (teleporterConfig != null)
					{
						_enabled = teleporterConfig.Enabled;
						TechTier = teleporterConfig.TechTier;
						MaxCharges = teleporterConfig.MaxCharges;
						RechargeTime = teleporterConfig.RechargeTime;
						RechargeAnnounceInterval = teleporterConfig.RechargeAnnounceInterval;
						TeleportRadius = teleporterConfig.TeleportRadius;
						Countdown = teleporterConfig.Countdown;
						SoundFile = teleporterConfig.SoundFile;
						MinEnemyBaseDistance = teleporterConfig.MinEnemyBaseDistance;
						MelonLogger.Msg("HeavyTeleporter: Config loaded from " + _configPath);
						return;
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("HeavyTeleporter: Failed to load config: " + ex.Message);
			}
			SaveConfig();
			MelonLogger.Msg("HeavyTeleporter: Default config saved to " + _configPath);
		}

		private static void SaveConfig()
		{
			try
			{
				TeleporterConfig teleporterConfig = new TeleporterConfig
				{
					Enabled = _enabled,
					TechTier = TechTier,
					MaxCharges = MaxCharges,
					RechargeTime = RechargeTime,
					RechargeAnnounceInterval = RechargeAnnounceInterval,
					TeleportRadius = TeleportRadius,
					Countdown = Countdown,
					SoundFile = SoundFile,
					MinEnemyBaseDistance = MinEnemyBaseDistance
				};
				string directoryName = Path.GetDirectoryName(_configPath);
				if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				File.WriteAllText(_configPath, JsonConvert.SerializeObject((object)teleporterConfig, (Formatting)1));
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("HeavyTeleporter: Failed to save config: " + ex.Message);
			}
		}

		public override void OnInitializeMelon()
		{
			_configPath = Path.Combine("UserData", "HeavyTeleporter_Config.json");
			LoadConfig();
			MelonLogger.Msg("Heavy Teleporter v1.2.0 loaded! (Sol + Centauri)");
			GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Combine(GameEvents.OnGameEnded, new Action<GameMode, Team>(OnGameEnded));
		}

		public override void OnLateInitializeMelon()
		{
			PlayerMethods.RegisterPlayerCommand("st", OnStCommand, hideFromChat: true);
			MelonLogger.Msg("HeavyTeleporter: Registered /st command.");
		}

		private static void OnGameEnded(GameMode mode, Team winner)
		{
			_teamStates.Clear();
			_pendingRequests.Clear();
			_vehicleMenus.Clear();
			MelonLogger.Msg("HeavyTeleporter: Round ended, cleared state.");
		}

		private static void OnStCommand(Player? caller, string args)
		{
			//IL_068a: Unknown result type (might be due to invalid IL or missing references)
			//IL_068f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0691: Unknown result type (might be due to invalid IL or missing references)
			//IL_069d: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_06bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_06c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_06c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_06de: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)caller == (Object)null)
			{
				return;
			}
			args = (args ?? "").Trim();
			string[] array = args.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string text = ((array.Length > 1) ? array[1].ToLower() : "");
			if (!string.IsNullOrEmpty(text))
			{
				string s = ((array.Length > 2) ? array[2].Trim() : "");
				bool flag = caller.CanAdminExecute(Power.Generic);
				switch (text)
				{
				case "on":
					if (!flag)
					{
						Deny(caller);
						return;
					}
					_enabled = true;
					SaveConfig();
					Reply(caller, "Heavy Teleporter: ON");
					return;
				case "off":
					if (!flag)
					{
						Deny(caller);
						return;
					}
					_enabled = false;
					SaveConfig();
					Reply(caller, "Heavy Teleporter: OFF");
					return;
				case "charges":
				{
					int result;
					if (!flag)
					{
						Deny(caller);
					}
					else if (int.TryParse(s, out result) && result > 0)
					{
						MaxCharges = result;
						SaveConfig();
						Reply(caller, "Charges = " + MaxCharges);
					}
					else
					{
						Reply(caller, "Charges = " + MaxCharges);
					}
					return;
				}
				case "cd":
				{
					float result3;
					if (!flag)
					{
						Deny(caller);
					}
					else if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result3) && result3 > 0f)
					{
						RechargeTime = result3;
						SaveConfig();
						Reply(caller, "Recharge = " + RechargeTime + "s");
					}
					else
					{
						Reply(caller, "Recharge = " + RechargeTime + "s");
					}
					return;
				}
				case "tier":
				{
					int result5;
					if (!flag)
					{
						Deny(caller);
					}
					else if (int.TryParse(s, out result5) && result5 >= 0)
					{
						TechTier = result5;
						SaveConfig();
						Reply(caller, "Tech tier = " + TechTier);
					}
					else
					{
						Reply(caller, "Tech tier = " + TechTier);
					}
					return;
				}
				case "countdown":
				{
					float result2;
					if (!flag)
					{
						Deny(caller);
					}
					else if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result2) && result2 >= 0f)
					{
						Countdown = result2;
						SaveConfig();
						Reply(caller, "Countdown = " + Countdown + "s");
					}
					else
					{
						Reply(caller, "Countdown = " + Countdown + "s");
					}
					return;
				}
				case "radius":
				{
					float result4;
					if (!flag)
					{
						Deny(caller);
					}
					else if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result4) && result4 > 0f)
					{
						TeleportRadius = result4;
						SaveConfig();
						Reply(caller, "Radius = " + TeleportRadius + "m");
					}
					else
					{
						Reply(caller, "Radius = " + TeleportRadius + "m");
					}
					return;
				}
				case "mindist":
				{
					float result6;
					if (!flag)
					{
						Deny(caller);
					}
					else if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result6) && result6 >= 0f)
					{
						MinEnemyBaseDistance = result6;
						SaveConfig();
						Reply(caller, "Min enemy base distance = " + MinEnemyBaseDistance + "m");
					}
					else
					{
						Reply(caller, "Min enemy base distance = " + MinEnemyBaseDistance + "m (usage: /st mindist <meters>)");
					}
					return;
				}
				case "status":
				{
					string text2 = "";
					foreach (KeyValuePair<int, TeamState> teamState2 in _teamStates)
					{
						TeamState value = teamState2.Value;
						if (value.Announced)
						{
							text2 += string.Format(" [{0}/{1}{2}]", value.Charges, MaxCharges, value.Recharging ? $" cd:{value.RechargeTimer:F0}s" : "");
						}
					}
					Reply(caller, string.Format("Heavy Teleporter: {0} | tier>={1} charges={2} cd={3}s countdown={4}s radius={5}m{6}", _enabled ? "ON" : "OFF", TechTier, MaxCharges, RechargeTime, Countdown, TeleportRadius, text2));
					return;
				}
				}
				if (int.TryParse(text, out var result7) && result7 >= 1)
				{
					int instanceID = ((Object)caller).GetInstanceID();
					if (_vehicleMenus.TryGetValue(instanceID, out List<VehicleCandidate> value2) && result7 <= value2.Count)
					{
						VehicleCandidate vehicleCandidate = value2[result7 - 1];
						if ((Object)(object)vehicleCandidate.Vehicle != (Object)null && !((BaseGameObject)vehicleCandidate.Vehicle).IsDestroyed)
						{
							_vehicleMenus.Remove(instanceID);
							RequestTeleportSpecific(caller, caller, vehicleCandidate.Vehicle);
						}
						else
						{
							HelperMethods.SendChatMessageToPlayer(caller, "[TELEPORTER] Vehicle no longer available.");
							_vehicleMenus.Remove(instanceID);
						}
						return;
					}
				}
				if ((caller.IsCommander | flag) && array.Length >= 3 && float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result8) && float.TryParse(array[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var result9))
				{
					Vector3 val = new Vector3(result8, 2000f, result9);
					RaycastHit val2 = default;
					val = ((!Physics.Raycast(val, Vector3.down, ref val2, 3000f, LayerMask.op_Implicit(GamePhysics.TRACELAYERMASK_TERRAIN))) ? new Vector3(result8, 200f, result9) : (val2.point + Vector3.up * 5f));
					RequestTeleportToPosition(caller, val);
				}
				else if (caller.IsCommander | flag)
				{
					Player val3 = FindPlayerByName(text);
					if ((Object)(object)val3 == (Object)null)
					{
						HelperMethods.SendChatMessageToPlayer(caller, "Player not found: " + text);
					}
					else if ((Object)(object)val3.ControlledUnit == (Object)null || ((BaseGameObject)val3.ControlledUnit).IsDestroyed)
					{
						HelperMethods.SendChatMessageToPlayer(caller, val3.PlayerName + " has no active unit.");
					}
					else
					{
						RequestTeleport(caller, val3);
					}
				}
				else
				{
					HelperMethods.SendChatMessageToPlayer(caller, "Usage: /st [status] | /st <playername> | /st <x> <z>");
				}
				return;
			}
			if (!_enabled)
			{
				HelperMethods.SendChatMessageToPlayer(caller, "Heavy Teleporter is disabled.");
				return;
			}
			if ((Object)(object)caller.Team == (Object)null || (!caller.Team.TeamName.Contains("Sol") && !caller.Team.TeamName.Contains("Centauri")))
			{
				HelperMethods.SendChatMessageToPlayer(caller, "[TELEPORTER] Sol or Centauri faction only.");
				return;
			}
			if (caller.Team.TechnologyTier < TechTier)
			{
				HelperMethods.SendChatMessageToPlayer(caller, $"[TELEPORTER] Requires tech {TechTier} (current: {caller.Team.TechnologyTier}).");
				return;
			}
			TeamState teamState = GetTeamState(caller.Team);
			if (!teamState.Ready || teamState.Charges <= 0)
			{
				if (teamState.Recharging)
				{
					HelperMethods.SendChatMessageToPlayer(caller, $"[TELEPORTER] Recharging... {Mathf.CeilToInt(teamState.RechargeTimer)}s remaining.");
				}
				else
				{
					HelperMethods.SendChatMessageToPlayer(caller, "[TELEPORTER] Not available.");
				}
				return;
			}
			if ((Object)(object)caller.ControlledUnit == (Object)null || ((BaseGameObject)caller.ControlledUnit).IsDestroyed)
			{
				HelperMethods.SendChatMessageToPlayer(caller, "[TELEPORTER] You need to be controlling a unit.");
				return;
			}
			List<VehicleCandidate> list = FindAllEligibleVehicles(caller.Team);
			if (list.Count == 0)
			{
				HelperMethods.SendChatMessageToPlayer(caller, "[TELEPORTER] No vehicle in teleport zone. Move a vehicle near the Ultra Heavy Factory waypoint.");
				return;
			}
			if (list.Count == 1)
			{
				RequestTeleportSpecific(caller, caller, list[0].Vehicle);
				return;
			}
			int instanceID2 = ((Object)caller).GetInstanceID();
			_vehicleMenus[instanceID2] = list;
			HelperMethods.SendChatMessageToPlayer(caller, "[TELEPORTER] Select vehicle:");
			for (int i = 0; i < list.Count; i++)
			{
				HelperMethods.SendChatMessageToPlayer(caller, $"  {i + 1}: {list[i].Name} ({list[i].FactoryLabel})");
			}
			HelperMethods.SendChatMessageToPlayer(caller, "Type /st <number> to select.");
		}

		private static Structure? FindUltraHeavyFactory(Team team)
		{
			List<Structure> structures = Structure.Structures;
			for (int i = 0; i < structures.Count; i++)
			{
				Structure val = structures[i];
				if (!((Object)(object)val == (Object)null) && !((BaseGameObject)val).IsDestroyed && !((Object)(object)((BaseGameObject)val).Team != (Object)(object)team) && !((Object)(object)((BaseGameObject)val).ObjectInfo == (Object)null) && ((Object)((BaseGameObject)val).ObjectInfo).name.Contains("UltraHeavyFactory"))
				{
					return val;
				}
			}
			return null;
		}

		private static Vector3 GetTeleportZoneCenter(Structure factory)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)factory.ProductionUnitMoveToTransform != (Object)null)
			{
				return factory.ProductionUnitMoveToTransform.position;
			}
			return ((Component)factory).transform.position + ((Component)factory).transform.forward * 15f;
		}

		private static List<VehicleCandidate> FindAllEligibleVehicles(Team team)
		{
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			List<VehicleCandidate> list = new List<VehicleCandidate>();
			HashSet<int> hashSet = new HashSet<int>();
			int num = 0;
			List<Structure> structures = Structure.Structures;
			for (int i = 0; i < structures.Count; i++)
			{
				Structure val = structures[i];
				if ((Object)(object)val == (Object)null || ((BaseGameObject)val).IsDestroyed || (Object)(object)((BaseGameObject)val).Team != (Object)(object)team || (Object)(object)((BaseGameObject)val).ObjectInfo == (Object)null || !((Object)((BaseGameObject)val).ObjectInfo).name.Contains("UltraHeavyFactory"))
				{
					continue;
				}
				num++;
				Vector3 teleportZoneCenter = GetTeleportZoneCenter(val);
				string factoryLabel = ((num > 1) ? ("Factory #" + num) : "Factory");
				Collider[] array = Physics.OverlapSphere(teleportZoneCenter, TeleportRadius);
				foreach (Collider val2 in array)
				{
					if ((Object)(object)val2 == (Object)null)
					{
						continue;
					}
					Vehicle componentInParent = ((Component)val2).GetComponentInParent<Vehicle>();
					if (!((Object)(object)componentInParent == (Object)null) && !((BaseGameObject)componentInParent).IsDestroyed && !((Object)(object)((BaseGameObject)componentInParent).Team != (Object)(object)team) && (!((Object)(object)((Unit)componentInParent).DriverCompartment != (Object)null) || ((Unit)componentInParent).DriverCompartment.NumBoarded <= 0))
					{
						int instanceID = ((Object)componentInParent).GetInstanceID();
						if (!hashSet.Contains(instanceID))
						{
							hashSet.Add(instanceID);
							string name = (((Object)(object)((BaseGameObject)componentInParent).ObjectInfo != (Object)null) ? ((BaseGameObject)componentInParent).ObjectInfo.DisplayName : "Vehicle");
							list.Add(new VehicleCandidate
							{
								Vehicle = componentInParent,
								Name = name,
								FactoryLabel = factoryLabel
							});
						}
					}
				}
			}
			return list;
		}

		private static Vehicle? FindEligibleVehicle(Team team)
		{
			List<VehicleCandidate> list = FindAllEligibleVehicles(team);
			if (list.Count <= 0)
			{
				return null;
			}
			return list[0].Vehicle;
		}

		private static void RequestTeleport(Player requester, Player target)
		{
			Vehicle val = FindEligibleVehicle(requester.Team);
			if ((Object)(object)val == (Object)null)
			{
				HelperMethods.SendChatMessageToPlayer(requester, "[TELEPORTER] No vehicle in teleport zone.");
			}
			else
			{
				RequestTeleportSpecific(requester, target, val);
			}
		}

		private static void RequestTeleportSpecific(Player requester, Player target, Vehicle vehicle)
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			int instanceID = ((Object)requester).GetInstanceID();
			if (_pendingRequests.ContainsKey(instanceID))
			{
				HelperMethods.SendChatMessageToPlayer(requester, "[TELEPORTER] Request already pending.");
				return;
			}
			string text = CheckEnemyBaseProximity(((Object)(object)target.ControlledUnit != (Object)null) ? ((Component)target.ControlledUnit).transform.position : Vector3.zero, requester.Team);
			if (text != null)
			{
				HelperMethods.SendChatMessageToPlayer(requester, "[TELEPORTER] " + text);
				return;
			}
			string arg = (((Object)(object)((BaseGameObject)vehicle).ObjectInfo != (Object)null) ? ((BaseGameObject)vehicle).ObjectInfo.DisplayName : "Vehicle");
			string arg2 = (((Object)(object)target == (Object)(object)requester) ? "your position" : target.PlayerName);
			_pendingRequests[instanceID] = new TeleportRequest
			{
				Requester = requester,
				Target = target,
				RequestTime = Time.time,
				LastAnnounced = -1,
				ChosenVehicle = vehicle
			};
			HelperMethods.SendChatMessageToPlayer(requester, $"[TELEPORTER] {arg} teleporting to {arg2} in {Countdown}s...");
		}

		private static void RequestTeleportToPosition(Player requester, Vector3 position)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			int instanceID = ((Object)requester).GetInstanceID();
			if (_pendingRequests.ContainsKey(instanceID))
			{
				HelperMethods.SendChatMessageToPlayer(requester, "[TELEPORTER] Request already pending.");
				return;
			}
			string text = CheckEnemyBaseProximity(position, requester.Team);
			if (text != null)
			{
				HelperMethods.SendChatMessageToPlayer(requester, "[TELEPORTER] " + text);
				return;
			}
			Vehicle val = FindEligibleVehicle(requester.Team);
			if ((Object)(object)val == (Object)null)
			{
				HelperMethods.SendChatMessageToPlayer(requester, "[TELEPORTER] No vehicle in teleport zone.");
				return;
			}
			string text2 = (((Object)(object)((BaseGameObject)val).ObjectInfo != (Object)null) ? ((BaseGameObject)val).ObjectInfo.DisplayName : "Vehicle");
			_pendingRequests[instanceID] = new TeleportRequest
			{
				Requester = requester,
				Target = null,
				TargetPosition = position,
				UseCoords = true,
				RequestTime = Time.time,
				LastAnnounced = -1,
				ChosenVehicle = val
			};
			HelperMethods.SendChatMessageToPlayer(requester, $"[TELEPORTER] {text2} teleporting to ({position.x:F0}, {position.z:F0}) in {Countdown}s...");
		}

		private static string? CheckEnemyBaseProximity(Vector3 position, Team requesterTeam)
		{
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			if (MinEnemyBaseDistance <= 0f)
			{
				return null;
			}
			List<Structure> structures = Structure.Structures;
			for (int i = 0; i < structures.Count; i++)
			{
				Structure val = structures[i];
				if ((Object)(object)val == (Object)null || ((BaseGameObject)val).IsDestroyed || (Object)(object)((BaseGameObject)val).Team == (Object)null || (Object)(object)((BaseGameObject)val).Team == (Object)(object)requesterTeam || (Object)(object)((BaseGameObject)val).ObjectInfo == (Object)null)
				{
					continue;
				}
				string name = ((Object)((BaseGameObject)val).ObjectInfo).name;
				if (name.Contains("Nest") || name.Contains("Headquarters"))
				{
					float num = Vector3.Distance(position, ((Component)val).transform.position);
					if (num < MinEnemyBaseDistance)
					{
						string arg = (name.Contains("Nest") ? "Nest" : "HQ");
						return $"Too close to enemy {arg} ({num:F0}m, min {MinEnemyBaseDistance}m).";
					}
				}
			}
			return null;
		}

		private static void Deny(Player caller)
		{
			HelperMethods.SendChatMessageToPlayer(caller, "Heavy Teleporter: admin only.");
		}

		private static void Reply(Player? player, string msg)
		{
			if ((Object)(object)player != (Object)null)
			{
				HelperMethods.SendChatMessageToPlayer(player, msg);
			}
			MelonLogger.Msg("HeavyTeleporter: " + msg);
		}

		private static void SendTeamChat(Team team, string msg)
		{
			if ((Object)(object)team == (Object)null)
			{
				return;
			}
			for (int i = 0; i < Player.Players.Count; i++)
			{
				Player val = Player.Players[i];
				if ((Object)(object)val != (Object)null && (Object)(object)val.Team == (Object)(object)team && (Object)(object)val != (Object)(object)NetworkGameServer.GetServerPlayer())
				{
					HelperMethods.SendChatMessageToPlayer(val, msg);
				}
			}
			MelonLogger.Msg("HeavyTeleporter [TeamChat]: " + msg);
		}

		private static Player? FindPlayerByName(string name)
		{
			name = name.ToLower();
			for (int i = 0; i < Player.Players.Count; i++)
			{
				Player val = Player.Players[i];
				if ((Object)(object)val != (Object)null && val.PlayerName != null && val.PlayerName.ToLower().Contains(name))
				{
					return val;
				}
			}
			return null;
		}

		public override void OnUpdate()
		{
			if (NetworkGameServer.GetServerStarted() && _enabled)
			{
				UpdateTeams();
				ProcessPendingRequests();
			}
		}

		private void UpdateTeams()
		{
			HashSet<int> hashSet = new HashSet<int>();
			for (int i = 0; i < Player.Players.Count; i++)
			{
				Player val = Player.Players[i];
				if ((Object)(object)val == (Object)null || (Object)(object)val.Team == (Object)null || val.Team.TeamName == null || (!val.Team.TeamName.Contains("Sol") && !val.Team.TeamName.Contains("Centauri")))
				{
					continue;
				}
				int instanceID = ((Object)val.Team).GetInstanceID();
				if (hashSet.Contains(instanceID))
				{
					continue;
				}
				hashSet.Add(instanceID);
				TeamState teamState = GetTeamState(val.Team);
				if (!teamState.Announced && val.Team.TechnologyTier >= TechTier)
				{
					teamState.Announced = true;
					teamState.Charges = MaxCharges;
					teamState.Ready = true;
					teamState.Recharging = false;
					SendTeamChat(val.Team, $"[TELEPORTER] ONLINE! {MaxCharges} vehicle teleports available. Use /st to request a vehicle to your position.");
				}
				if (teamState.Recharging)
				{
					teamState.RechargeTimer -= Time.deltaTime;
					teamState.LastRechargeAnnounce -= Time.deltaTime;
					if (teamState.LastRechargeAnnounce <= 0f)
					{
						teamState.LastRechargeAnnounce = RechargeAnnounceInterval;
						int num = Mathf.CeilToInt(teamState.RechargeTimer);
						SendTeamChat(val.Team, $"[TELEPORTER] Recharging... {num / 60}:{num % 60:D2} remaining.");
					}
					if (teamState.RechargeTimer <= 0f)
					{
						teamState.Recharging = false;
						teamState.Charges = MaxCharges;
						teamState.Ready = true;
						SendTeamChat(val.Team, $"[TELEPORTER] RECHARGED! {MaxCharges} vehicle teleports available. Use /st to request.");
					}
				}
			}
		}

		private void ProcessPendingRequests()
		{
			if (_pendingRequests.Count == 0)
			{
				return;
			}
			List<int> list = new List<int>();
			foreach (KeyValuePair<int, TeleportRequest> pendingRequest in _pendingRequests)
			{
				TeleportRequest value = pendingRequest.Value;
				if (!value.UseCoords && ((Object)(object)value.Target == (Object)null || (Object)(object)value.Target.ControlledUnit == (Object)null || ((BaseGameObject)value.Target.ControlledUnit).IsDestroyed))
				{
					HelperMethods.SendChatMessageToPlayer(value.Requester, "[TELEPORTER] Target lost. Cancelled.");
					list.Add(pendingRequest.Key);
					continue;
				}
				TeamState teamState = GetTeamState(value.Requester.Team);
				if (teamState.Charges <= 0)
				{
					HelperMethods.SendChatMessageToPlayer(value.Requester, "[TELEPORTER] No charges left. Cancelled.");
					list.Add(pendingRequest.Key);
					continue;
				}
				float num = Time.time - value.RequestTime;
				float num2 = Countdown - num;
				if (num2 <= 0f)
				{
					if (ExecuteTeleport(value))
					{
						teamState.Charges--;
						string arg;
						if (value.UseCoords)
						{
							arg = $"({value.TargetPosition.x:F0}, {value.TargetPosition.z:F0})";
						}
						else
						{
							arg = (((Object)(object)value.Target == (Object)(object)value.Requester) ? value.Requester.PlayerName : value.Target.PlayerName);
						}
						SendTeamChat(value.Requester.Team, $"[TELEPORTER] Vehicle teleported to {arg}! {teamState.Charges}/{MaxCharges} charges remaining.");
						if (teamState.Charges <= 0)
						{
							teamState.Ready = false;
							teamState.Recharging = true;
							teamState.RechargeTimer = RechargeTime;
							teamState.LastRechargeAnnounce = RechargeAnnounceInterval;
							SendTeamChat(value.Requester.Team, $"[TELEPORTER] All charges spent! Recharging in {(int)RechargeTime / 60} minutes...");
						}
					}
					else
					{
						HelperMethods.SendChatMessageToPlayer(value.Requester, "[TELEPORTER] No eligible vehicle found near Ultra Heavy Factory.");
					}
					list.Add(pendingRequest.Key);
				}
				else
				{
					int num3 = Mathf.CeilToInt(num2);
					if (num3 != value.LastAnnounced)
					{
						value.LastAnnounced = num3;
						HelperMethods.SendChatMessageToPlayer(value.Requester, $"[TELEPORTER] Teleporting in {num3}s...");
					}
				}
			}
			foreach (int item in list)
			{
				_pendingRequests.Remove(item);
			}
		}

		private bool ExecuteTeleport(TeleportRequest req)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = (req.UseCoords ? req.TargetPosition : ((Component)req.Target.ControlledUnit).transform.position);
			Vehicle val2 = req.ChosenVehicle;
			if ((Object)(object)val2 == (Object)null || ((BaseGameObject)val2).IsDestroyed)
			{
				val2 = FindEligibleVehicle(req.Requester.Team);
				if ((Object)(object)val2 == (Object)null)
				{
					return false;
				}
			}
			Vector3 val3;
			if (req.UseCoords)
			{
				val3 = val;
			}
			else
			{
				float num = Mathf.Max(((BaseGameObject)val2).PhysicalRadius * 2f + 5f, 15f);
				Vector3 forward = ((Component)req.Target.ControlledUnit).transform.forward;
				forward.y = 0f;
				val3 = val + forward.normalized * num;
			}
			RaycastHit val4 = default;
			if (Physics.Raycast(new Vector3(val3.x, 2000f, val3.z), Vector3.down, ref val4, 3000f, LayerMask.op_Implicit(GamePhysics.TRACELAYERMASK_TERRAIN)))
			{
				val3.y = val4.point.y + 5f;
			}
			Vector3 val5 = (req.UseCoords ? ((Component)val2).transform.forward : ((Component)req.Target.ControlledUnit).transform.forward);
			val5.y = 0f;
			if (val5.sqrMagnitude < 0.01f)
			{
				val5 = Vector3.forward;
			}
			Quaternion val6 = Quaternion.LookRotation(val5.normalized, Vector3.up);
			Player val7 = null;
			NetworkComponent networkComponent = ((BaseGameObject)val2).NetworkComponent;
			if ((Object)(object)networkComponent != (Object)null)
			{
				val7 = networkComponent.OwnerPlayer;
				if ((Object)(object)val7 != (Object)null)
				{
					networkComponent.SetPlayerOwner((Player)null);
				}
			}
			((BaseGameObject)val2).QueueTeleport(val3, val6, false);
			BaseGameObject.PerformDelayedTeleportAll();
			Rigidbody[] rigidBodies = val2.RigidBodies;
			if (rigidBodies != null)
			{
				Rigidbody[] array = rigidBodies;
				foreach (Rigidbody val8 in array)
				{
					if ((Object)(object)val8 != (Object)null && !val8.isKinematic)
					{
						val8.linearVelocity = Vector3.zero;
						val8.angularVelocity = Vector3.zero;
					}
				}
			}
			if ((Object)(object)val7 != (Object)null && (Object)(object)networkComponent != (Object)null)
			{
				networkComponent.SetPlayerOwner(val7);
			}
			try
			{
				if ((Object)(object)req.Target != (Object)null)
				{
					AudioHelper.PlaySoundFile(SoundFile, req.Target);
				}
				if ((Object)(object)req.Requester != (Object)null && (Object)(object)req.Requester != (Object)(object)req.Target)
				{
					AudioHelper.PlaySoundFile(SoundFile, req.Requester);
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("HeavyTeleporter sound failed: " + ex.Message);
			}
			string arg = (((Object)(object)((BaseGameObject)val2).ObjectInfo != (Object)null) ? ((BaseGameObject)val2).ObjectInfo.DisplayName : "Vehicle");
			MelonLogger.Msg($"TELEPORT! {arg} teleported (requested by {req.Requester.PlayerName})");
			return true;
		}
	}
}
