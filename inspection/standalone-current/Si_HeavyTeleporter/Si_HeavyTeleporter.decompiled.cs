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
			int instanceID = team.GetInstanceID();
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
				TeleporterConfig value = new TeleporterConfig
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
				File.WriteAllText(_configPath, JsonConvert.SerializeObject(value, Formatting.Indented));
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
			if (caller == null)
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
					int instanceID = caller.GetInstanceID();
					if (_vehicleMenus.TryGetValue(instanceID, out List<VehicleCandidate> value2) && result7 <= value2.Count)
					{
						VehicleCandidate vehicleCandidate = value2[result7 - 1];
						if (vehicleCandidate.Vehicle != null && !vehicleCandidate.Vehicle.IsDestroyed)
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
					Vector3 origin = new Vector3(result8, 2000f, result9);
					origin = ((!Physics.Raycast(origin, Vector3.down, out var hitInfo, 3000f, GamePhysics.TRACELAYERMASK_TERRAIN)) ? new Vector3(result8, 200f, result9) : (hitInfo.point + Vector3.up * 5f));
					RequestTeleportToPosition(caller, origin);
				}
				else if (caller.IsCommander | flag)
				{
					Player player = FindPlayerByName(text);
					if (player == null)
					{
						HelperMethods.SendChatMessageToPlayer(caller, "Player not found: " + text);
					}
					else if (player.ControlledUnit == null || player.ControlledUnit.IsDestroyed)
					{
						HelperMethods.SendChatMessageToPlayer(caller, player.PlayerName + " has no active unit.");
					}
					else
					{
						RequestTeleport(caller, player);
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
			if (caller.Team == null || (!caller.Team.TeamName.Contains("Sol") && !caller.Team.TeamName.Contains("Centauri")))
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
			if (caller.ControlledUnit == null || caller.ControlledUnit.IsDestroyed)
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
			int instanceID2 = caller.GetInstanceID();
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
				Structure structure = structures[i];
				if (!(structure == null) && !structure.IsDestroyed && !(structure.Team != team) && !(structure.ObjectInfo == null) && structure.ObjectInfo.name.Contains("UltraHeavyFactory"))
				{
					return structure;
				}
			}
			return null;
		}

		private static Vector3 GetTeleportZoneCenter(Structure factory)
		{
			if (factory.ProductionUnitMoveToTransform != null)
			{
				return factory.ProductionUnitMoveToTransform.position;
			}
			return factory.transform.position + factory.transform.forward * 15f;
		}

		private static List<VehicleCandidate> FindAllEligibleVehicles(Team team)
		{
			List<VehicleCandidate> list = new List<VehicleCandidate>();
			HashSet<int> hashSet = new HashSet<int>();
			int num = 0;
			List<Structure> structures = Structure.Structures;
			for (int i = 0; i < structures.Count; i++)
			{
				Structure structure = structures[i];
				if (structure == null || structure.IsDestroyed || structure.Team != team || structure.ObjectInfo == null || !structure.ObjectInfo.name.Contains("UltraHeavyFactory"))
				{
					continue;
				}
				num++;
				Vector3 teleportZoneCenter = GetTeleportZoneCenter(structure);
				string factoryLabel = ((num > 1) ? ("Factory #" + num) : "Factory");
				Collider[] array = Physics.OverlapSphere(teleportZoneCenter, TeleportRadius);
				foreach (Collider collider in array)
				{
					if (collider == null)
					{
						continue;
					}
					Vehicle componentInParent = collider.GetComponentInParent<Vehicle>();
					if (!(componentInParent == null) && !componentInParent.IsDestroyed && !(componentInParent.Team != team) && (!(componentInParent.DriverCompartment != null) || componentInParent.DriverCompartment.NumBoarded <= 0))
					{
						int instanceID = componentInParent.GetInstanceID();
						if (!hashSet.Contains(instanceID))
						{
							hashSet.Add(instanceID);
							string name = ((componentInParent.ObjectInfo != null) ? componentInParent.ObjectInfo.DisplayName : "Vehicle");
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
			Vehicle vehicle = FindEligibleVehicle(requester.Team);
			if (vehicle == null)
			{
				HelperMethods.SendChatMessageToPlayer(requester, "[TELEPORTER] No vehicle in teleport zone.");
			}
			else
			{
				RequestTeleportSpecific(requester, target, vehicle);
			}
		}

		private static void RequestTeleportSpecific(Player requester, Player target, Vehicle vehicle)
		{
			int instanceID = requester.GetInstanceID();
			if (_pendingRequests.ContainsKey(instanceID))
			{
				HelperMethods.SendChatMessageToPlayer(requester, "[TELEPORTER] Request already pending.");
				return;
			}
			string text = CheckEnemyBaseProximity((target.ControlledUnit != null) ? target.ControlledUnit.transform.position : Vector3.zero, requester.Team);
			if (text != null)
			{
				HelperMethods.SendChatMessageToPlayer(requester, "[TELEPORTER] " + text);
				return;
			}
			string arg = ((vehicle.ObjectInfo != null) ? vehicle.ObjectInfo.DisplayName : "Vehicle");
			string arg2 = ((target == requester) ? "your position" : target.PlayerName);
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
			int instanceID = requester.GetInstanceID();
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
			Vehicle vehicle = FindEligibleVehicle(requester.Team);
			if (vehicle == null)
			{
				HelperMethods.SendChatMessageToPlayer(requester, "[TELEPORTER] No vehicle in teleport zone.");
				return;
			}
			string text2 = ((vehicle.ObjectInfo != null) ? vehicle.ObjectInfo.DisplayName : "Vehicle");
			_pendingRequests[instanceID] = new TeleportRequest
			{
				Requester = requester,
				Target = null,
				TargetPosition = position,
				UseCoords = true,
				RequestTime = Time.time,
				LastAnnounced = -1,
				ChosenVehicle = vehicle
			};
			HelperMethods.SendChatMessageToPlayer(requester, $"[TELEPORTER] {text2} teleporting to ({position.x:F0}, {position.z:F0}) in {Countdown}s...");
		}

		private static string? CheckEnemyBaseProximity(Vector3 position, Team requesterTeam)
		{
			if (MinEnemyBaseDistance <= 0f)
			{
				return null;
			}
			List<Structure> structures = Structure.Structures;
			for (int i = 0; i < structures.Count; i++)
			{
				Structure structure = structures[i];
				if (structure == null || structure.IsDestroyed || structure.Team == null || structure.Team == requesterTeam || structure.ObjectInfo == null)
				{
					continue;
				}
				string name = structure.ObjectInfo.name;
				if (name.Contains("Nest") || name.Contains("Headquarters"))
				{
					float num = Vector3.Distance(position, structure.transform.position);
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
			if (player != null)
			{
				HelperMethods.SendChatMessageToPlayer(player, msg);
			}
			MelonLogger.Msg("HeavyTeleporter: " + msg);
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
			MelonLogger.Msg("HeavyTeleporter [TeamChat]: " + msg);
		}

		private static Player? FindPlayerByName(string name)
		{
			name = name.ToLower();
			for (int i = 0; i < Player.Players.Count; i++)
			{
				Player player = Player.Players[i];
				if (player != null && player.PlayerName != null && player.PlayerName.ToLower().Contains(name))
				{
					return player;
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
				Player player = Player.Players[i];
				if (player == null || player.Team == null || player.Team.TeamName == null || (!player.Team.TeamName.Contains("Sol") && !player.Team.TeamName.Contains("Centauri")))
				{
					continue;
				}
				int instanceID = player.Team.GetInstanceID();
				if (hashSet.Contains(instanceID))
				{
					continue;
				}
				hashSet.Add(instanceID);
				TeamState teamState = GetTeamState(player.Team);
				if (!teamState.Announced && player.Team.TechnologyTier >= TechTier)
				{
					teamState.Announced = true;
					teamState.Charges = MaxCharges;
					teamState.Ready = true;
					teamState.Recharging = false;
					SendTeamChat(player.Team, $"[TELEPORTER] ONLINE! {MaxCharges} vehicle teleports available. Use /st to request a vehicle to your position.");
				}
				if (teamState.Recharging)
				{
					teamState.RechargeTimer -= Time.deltaTime;
					teamState.LastRechargeAnnounce -= Time.deltaTime;
					if (teamState.LastRechargeAnnounce <= 0f)
					{
						teamState.LastRechargeAnnounce = RechargeAnnounceInterval;
						int num = Mathf.CeilToInt(teamState.RechargeTimer);
						SendTeamChat(player.Team, $"[TELEPORTER] Recharging... {num / 60}:{num % 60:D2} remaining.");
					}
					if (teamState.RechargeTimer <= 0f)
					{
						teamState.Recharging = false;
						teamState.Charges = MaxCharges;
						teamState.Ready = true;
						SendTeamChat(player.Team, $"[TELEPORTER] RECHARGED! {MaxCharges} vehicle teleports available. Use /st to request.");
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
				if (!value.UseCoords && (value.Target == null || value.Target.ControlledUnit == null || value.Target.ControlledUnit.IsDestroyed))
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
							arg = ((value.Target == value.Requester) ? value.Requester.PlayerName : value.Target.PlayerName);
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
			Vector3 vector = (req.UseCoords ? req.TargetPosition : req.Target.ControlledUnit.transform.position);
			Vehicle vehicle = req.ChosenVehicle;
			if (vehicle == null || vehicle.IsDestroyed)
			{
				vehicle = FindEligibleVehicle(req.Requester.Team);
				if (vehicle == null)
				{
					return false;
				}
			}
			Vector3 position;
			if (req.UseCoords)
			{
				position = vector;
			}
			else
			{
				float num = Mathf.Max(vehicle.PhysicalRadius * 2f + 5f, 15f);
				Vector3 forward = req.Target.ControlledUnit.transform.forward;
				forward.y = 0f;
				position = vector + forward.normalized * num;
			}
			if (Physics.Raycast(new Vector3(position.x, 2000f, position.z), Vector3.down, out var hitInfo, 3000f, GamePhysics.TRACELAYERMASK_TERRAIN))
			{
				position.y = hitInfo.point.y + 5f;
			}
			Vector3 vector2 = (req.UseCoords ? vehicle.transform.forward : req.Target.ControlledUnit.transform.forward);
			vector2.y = 0f;
			if (vector2.sqrMagnitude < 0.01f)
			{
				vector2 = Vector3.forward;
			}
			Quaternion rotation = Quaternion.LookRotation(vector2.normalized, Vector3.up);
			Player player = null;
			NetworkComponent networkComponent = vehicle.NetworkComponent;
			if (networkComponent != null)
			{
				player = networkComponent.OwnerPlayer;
				if (player != null)
				{
					networkComponent.SetPlayerOwner(null);
				}
			}
			vehicle.QueueTeleport(position, rotation);
			BaseGameObject.PerformDelayedTeleportAll();
			Rigidbody[] rigidBodies = vehicle.RigidBodies;
			if (rigidBodies != null)
			{
				Rigidbody[] array = rigidBodies;
				foreach (Rigidbody rigidbody in array)
				{
					if (rigidbody != null && !rigidbody.isKinematic)
					{
						rigidbody.linearVelocity = Vector3.zero;
						rigidbody.angularVelocity = Vector3.zero;
					}
				}
			}
			if (player != null && networkComponent != null)
			{
				networkComponent.SetPlayerOwner(player);
			}
			try
			{
				if (req.Target != null)
				{
					AudioHelper.PlaySoundFile(SoundFile, req.Target);
				}
				if (req.Requester != null && req.Requester != req.Target)
				{
					AudioHelper.PlaySoundFile(SoundFile, req.Requester);
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("HeavyTeleporter sound failed: " + ex.Message);
			}
			string arg = ((vehicle.ObjectInfo != null) ? vehicle.ObjectInfo.DisplayName : "Vehicle");
			MelonLogger.Msg($"TELEPORT! {arg} teleported (requested by {req.Requester.PlayerName})");
			return true;
		}
	}
}
