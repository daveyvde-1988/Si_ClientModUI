using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using MelonLoader;
using MelonLoader.Utils;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Si_SpawnConfigs;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(SpawnConfigs), "Admin Spawn Configs", "0.9.4", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_SpawnConfigs")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_SpawnConfigs")]
[assembly: AssemblyTitle("Si_SpawnConfigs")]
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
namespace Si_SpawnConfigs
{
	public class SpawnConfigs : MelonMod
	{
		public class SpawnSetup
		{
			public string? Map { get; set; }

			public string? VersusMode { get; set; }

			public List<TeamSpawn>? Teams { get; set; }

			public List<StructureSpawn>? Structures { get; set; }

			public List<UnitSpawn>? Units { get; set; }
		}

		public class TeamSpawn
		{
			public int TeamIndex { get; set; }

			public int Resources { get; set; }

			public int TechTier { get; set; }
		}

		public class ObjectSpawn
		{
			private string _classname;

			private float[] _position;

			private float[] _rotation;

			public string Classname
			{
				get
				{
					return _classname;
				}
				set
				{
					_classname = value ?? throw new ArgumentNullException("Classname name is required.");
				}
			}

			public float[] Position
			{
				get
				{
					return _position;
				}
				set
				{
					_position = value ?? throw new ArgumentNullException("Position is required.");
				}
			}

			public float[] Rotation
			{
				get
				{
					return _rotation;
				}
				set
				{
					_rotation = value ?? throw new ArgumentNullException("Rotation is required.");
				}
			}

			public uint? NetID { get; set; }
		}

		public class StructureSpawn : ObjectSpawn
		{
			public int TeamIndex { get; set; }

			public int? TechTier { get; set; }

			public float? Health { get; set; }

			public int? Resources { get; set; }
		}

		public class UnitSpawn : ObjectSpawn
		{
			public int TeamIndex { get; set; }

			public float? Health { get; set; }

			public int? Resources { get; set; }
		}

		private static bool AdminModAvailable;

		private static GameObject? lastSpawnedObject;

		public override void OnLateInitializeMelon()
		{
			AdminModAvailable = MelonTypeBase<MelonMod>.RegisteredMelons.Any((MelonMod m) => ((MelonBase)m).Info.Name == "Admin Mod");
			if (AdminModAvailable)
			{
				HelperMethods.CommandCallback adminCallback = Command_Spawn;
				HelperMethods.RegisterAdminCommand("spawn", adminCallback, Power.Cheat, "Spawns an object. Usage: !spawn <objectname>");
				HelperMethods.CommandCallback adminCallback2 = Command_UndoSpawn;
				HelperMethods.RegisterAdminCommand("undospawn", adminCallback2, Power.Cheat, "Removes last spawned object. Usage !undospawn");
				HelperMethods.CommandCallback adminCallback3 = Command_SaveSetup;
				HelperMethods.RegisterAdminCommand("saveconfig", adminCallback3, Power.Cheat, "Saves all current objects to a file. Usage: !saveconfig <filename>");
				HelperMethods.CommandCallback adminCallback4 = Command_LoadSetup;
				HelperMethods.RegisterAdminCommand("loadconfig", adminCallback4, Power.Cheat, "Removes all current objects and loads set objects from file. Usage !loadconfig <filename>");
				HelperMethods.CommandCallback adminCallback5 = Command_AddSetup;
				HelperMethods.RegisterAdminCommand("addconfig", adminCallback5, Power.Cheat, "Keeps all current objects and adds set objects from file. Usage !addconfig <filename>");
			}
			else
			{
				MelonLogger.Warning("Dependency missing: Admin Mod");
			}
		}

		public static void Command_UndoSpawn(Player? callerPlayer, string args)
		{
			if (callerPlayer == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Console not supported.");
				return;
			}
			string text = args.Split(' ')[0];
			if (args.Split(' ').Length - 1 > 0)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too many arguments");
			}
			else if (lastSpawnedObject == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Nothing to undo");
			}
			else
			{
				BaseGameObject baseGameObject = lastSpawnedObject.GetBaseGameObject();
				string text2 = baseGameObject.ToString();
				baseGameObject.DamageManager.SetHealth01(0f);
				lastSpawnedObject = null;
				HelperMethods.AlertAdminAction(callerPlayer, "destroyed last spawned item (" + text2 + ")");
			}
		}

		public static void Command_Spawn(Player? callerPlayer, string args)
		{
			if (callerPlayer == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Console not supported.");
				return;
			}
			string text = args.Split(' ')[0];
			int num = args.Split(' ').Length - 1;
			if (num > 1)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too many arguments");
				return;
			}
			if (num < 1)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too few arguments");
				return;
			}
			Vector3 worldPhysicalCenter = callerPlayer.ControlledUnit.WorldPhysicalCenter;
			Quaternion facingRotation = callerPlayer.ControlledUnit.GetFacingRotation();
			string text2 = args.Split(' ')[1];
			int index = callerPlayer.Team.Index;
			if (HelperMethods.SpawnAtLocation(text2, worldPhysicalCenter, facingRotation, index) == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Failed to spawn");
			}
			else
			{
				HelperMethods.AlertAdminAction(callerPlayer, "spawned " + text2);
			}
		}

		public static void Command_SaveSetup(Player? callerPlayer, string args)
		{
			string text = args.Split(' ')[0];
			int num = args.Split(' ').Length - 1;
			if (num > 1)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too many arguments");
				return;
			}
			if (num < 1)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too few arguments");
				return;
			}
			string text2 = args.Split(' ')[1];
			try
			{
				string spawnConfigsDirectory = GetSpawnConfigsDirectory();
				if (!Directory.Exists(spawnConfigsDirectory))
				{
					MelonLogger.Msg("Creating SpawnConfigs directory at: " + spawnConfigsDirectory);
					Directory.CreateDirectory(spawnConfigsDirectory);
				}
				if (text2.Contains('.') && !text2.EndsWith("json"))
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Invalid save name (not .json)");
					return;
				}
				if (!text2.Contains('.'))
				{
					text2 += ".json";
				}
				if (text2.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Cannot use input as filename");
					return;
				}
				string path = Path.Combine(spawnConfigsDirectory, text2);
				if (File.Exists(path))
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": configuration already exists");
				}
				else if (!GameMode.CurrentGameMode.GameOngoing)
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Nothing to save with current game state");
				}
				else
				{
					string contents = JsonConvert.SerializeObject(GenerateSpawnSetup(), Formatting.Indented);
					File.WriteAllText(path, contents);
					HelperMethods.ReplyToCommand(text + ": Saved config to file");
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Command_SaveSetup failed");
			}
		}

		public static void Command_AddSetup(Player? callerPlayer, string args)
		{
			string text = args.Split(' ')[0];
			int num = args.Split(' ').Length - 1;
			if (num > 1)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too many arguments");
				return;
			}
			if (num < 1)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too few arguments");
				return;
			}
			string text2 = args.Split(' ')[1];
			try
			{
				string spawnConfigsDirectory = GetSpawnConfigsDirectory();
				if (text2.Contains('.') && !text2.EndsWith("json"))
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Invalid save name (not .json)");
					return;
				}
				if (!text2.Contains('.'))
				{
					text2 += ".json";
				}
				if (text2.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Cannot use input as filename");
					return;
				}
				string path = Path.Combine(spawnConfigsDirectory, text2);
				if (!File.Exists(path))
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": configuration not found");
					return;
				}
				SpawnSetup spawnSetup = JsonConvert.DeserializeObject<SpawnSetup>(File.ReadAllText(path));
				if (spawnSetup == null)
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": json error in configuration file");
					return;
				}
				if (spawnSetup.Map != null && spawnSetup.Map != NetworkGameServer.GetServerMap())
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": incompatible map specified");
					return;
				}
				MP_Strategy mP_Strategy = UnityEngine.Object.FindFirstObjectByType<MP_Strategy>();
				if (spawnSetup.VersusMode != null && spawnSetup.VersusMode != mP_Strategy.TeamsVersus.ToString())
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": incompatible mode specified");
				}
				else if (!ExecuteBatchSpawn(spawnSetup))
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": bad name in config file");
				}
				else
				{
					HelperMethods.ReplyToCommand(text + ": Added config from file");
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Command_AddSetup failed");
			}
		}

		public static void Command_LoadSetup(Player? callerPlayer, string args)
		{
			string text = args.Split(' ')[0];
			int num = args.Split(' ').Length - 1;
			if (num > 1)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too many arguments");
				return;
			}
			if (num < 1)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too few arguments");
				return;
			}
			string text2 = args.Split(' ')[1];
			try
			{
				string spawnConfigsDirectory = GetSpawnConfigsDirectory();
				if (text2.Contains('.') && !text2.EndsWith("json"))
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Invalid save name (not .json)");
					return;
				}
				if (!text2.Contains('.'))
				{
					text2 += ".json";
				}
				if (text2.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Cannot use input as filename");
					return;
				}
				string path = Path.Combine(spawnConfigsDirectory, text2);
				if (!File.Exists(path))
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": configuration not found");
					return;
				}
				SpawnSetup spawnSetup = JsonConvert.DeserializeObject<SpawnSetup>(File.ReadAllText(path));
				if (spawnSetup == null)
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": json error in configuration file");
					return;
				}
				if (spawnSetup.Map != null && spawnSetup.Map != NetworkGameServer.GetServerMap())
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": incompatible map specified");
					return;
				}
				MP_Strategy mP_Strategy = UnityEngine.Object.FindFirstObjectByType<MP_Strategy>();
				if (spawnSetup.VersusMode != null && spawnSetup.VersusMode != mP_Strategy.TeamsVersus.ToString())
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": incompatible mode specified");
					return;
				}
				SpawnSetup removeSetup = GenerateSpawnSetup(includeNetIDs: true);
				RemoveConstructionSites();
				if (!LoadStructures(spawnSetup))
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": invalid structure in config file");
					return;
				}
				RemoveStructures(removeSetup);
				RemoveUnits(removeSetup);
				if (!LoadUnits(spawnSetup))
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": invalid unit in config file");
				}
				else
				{
					LoadTeams(spawnSetup);
					HelperMethods.ReplyToCommand(text + ": Loaded config from file");
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Command_LoadSetup failed");
			}
		}

		private static string GetSpawnConfigsDirectory()
		{
			return Path.Combine(MelonEnvironment.UserDataDirectory, "SpawnConfigs\\");
		}

		public static void RemoveConstructionSites()
		{
			MelonLogger.Msg("Removing all construciton sites");
			ConstructionSite.ClearAllConstructionSites();
		}

		public static void RemoveStructures(SpawnSetup removeSetup)
		{
			if (removeSetup.Structures == null)
			{
				return;
			}
			foreach (StructureSpawn structure in removeSetup.Structures)
			{
				if (structure.NetID.HasValue)
				{
					MelonLogger.Msg("Removing structure " + structure.Classname);
					Structure.GetStructureByNetID(structure.NetID.Value).DamageManager.SetHealth01(0f);
				}
			}
		}

		public static void RemoveUnits(SpawnSetup removeSetup)
		{
			if (removeSetup.Units == null)
			{
				return;
			}
			foreach (UnitSpawn unit in removeSetup.Units)
			{
				if (unit.NetID.HasValue)
				{
					MelonLogger.Msg("Removing unit " + unit.Classname);
					Unit.GetUnitByNetID(unit.NetID.Value).DamageManager.SetHealth01(0f);
				}
			}
		}

		public static void ExecuteBatchRemoval(SpawnSetup removeSetup)
		{
			RemoveConstructionSites();
			RemoveStructures(removeSetup);
			RemoveUnits(removeSetup);
		}

		public static bool LoadUnits(SpawnSetup addSetup)
		{
			if (addSetup.Units != null)
			{
				foreach (UnitSpawn unit in addSetup.Units)
				{
					MelonLogger.Msg("Adding unit " + unit.Classname);
					Vector3 position = new Vector3
					{
						x = unit.Position[0],
						y = unit.Position[1],
						z = unit.Position[2]
					};
					Quaternion rotation = new Quaternion
					{
						x = unit.Rotation[0],
						y = unit.Rotation[1],
						z = unit.Rotation[2],
						w = unit.Rotation[3]
					};
					GameObject gameObject = HelperMethods.SpawnAtLocation(unit.Classname, position, rotation, unit.TeamIndex);
					if (gameObject == null)
					{
						return false;
					}
					BaseGameObject baseGameObject = gameObject.GetBaseGameObject();
					if (unit.Health.HasValue)
					{
						baseGameObject.DamageManager.SetHealth(unit.Health.Value);
					}
					if (baseGameObject.IsResourceHolder && unit.Resources.HasValue)
					{
						Resource resourceType = ((unit.TeamIndex == 0) ? Resource.Resources[1] : Resource.Resources[0]);
						baseGameObject.StoreResource(resourceType, unit.Resources.Value);
					}
				}
			}
			return true;
		}

		public static bool LoadStructures(SpawnSetup addSetup)
		{
			if (addSetup.Structures != null)
			{
				foreach (StructureSpawn structure in addSetup.Structures)
				{
					MelonLogger.Msg("Adding structure " + structure.Classname);
					Vector3 position = new Vector3
					{
						x = structure.Position[0],
						y = structure.Position[1],
						z = structure.Position[2]
					};
					Quaternion rotation = new Quaternion
					{
						x = structure.Rotation[0],
						y = structure.Rotation[1],
						z = structure.Rotation[2],
						w = structure.Rotation[3]
					};
					GameObject gameObject = HelperMethods.SpawnAtLocation(structure.Classname, position, rotation, structure.TeamIndex);
					if (gameObject == null)
					{
						return false;
					}
					BaseGameObject baseGameObject = gameObject.GetBaseGameObject();
					if (structure.Health.HasValue)
					{
						baseGameObject.DamageManager.SetHealth(structure.Health.Value);
					}
					if (structure.TechTier.HasValue)
					{
						Structure structureByNetID = Structure.GetStructureByNetID(gameObject.GetNetworkComponent().NetID);
						if (structureByNetID != null)
						{
							Team.Teams[structure.TeamIndex].TechnologyTier = structure.TechTier.Value;
							structureByNetID.UpdateTechnologyTier();
						}
					}
					if (baseGameObject.IsResourceHolder && structure.Resources.HasValue)
					{
						Resource resourceType = ((structure.TeamIndex == 0) ? Resource.Resources[1] : Resource.Resources[0]);
						baseGameObject.StoreResource(resourceType, structure.Resources.Value);
					}
				}
			}
			return true;
		}

		public static void LoadTeams(SpawnSetup addSetup)
		{
			if (addSetup.Teams == null)
			{
				return;
			}
			foreach (TeamSpawn team in addSetup.Teams)
			{
				Team.Teams[team.TeamIndex].StartingResources = team.Resources;
			}
		}

		public static bool ExecuteBatchSpawn(SpawnSetup spawnSetup)
		{
			if (!LoadStructures(spawnSetup))
			{
				return false;
			}
			if (!LoadUnits(spawnSetup))
			{
				return false;
			}
			LoadTeams(spawnSetup);
			return true;
		}

		public static SpawnSetup GenerateSpawnSetup(bool includeNetIDs = false)
		{
			SpawnSetup spawnSetup = new SpawnSetup
			{
				Map = NetworkGameServer.GetServerMap()
			};
			MP_Strategy mP_Strategy = UnityEngine.Object.FindFirstObjectByType<MP_Strategy>();
			spawnSetup.VersusMode = mP_Strategy.TeamsVersus.ToString();
			spawnSetup.Structures = new List<StructureSpawn>();
			spawnSetup.Units = new List<UnitSpawn>();
			spawnSetup.Teams = new List<TeamSpawn>();
			foreach (Team team in Team.Teams)
			{
				if (team == null)
				{
					continue;
				}
				TeamSpawn item = new TeamSpawn
				{
					Resources = team.StartingResources,
					TeamIndex = team.Index,
					TechTier = team.TechnologyTier
				};
				spawnSetup.Teams.Add(item);
				foreach (Structure structure in team.Structures)
				{
					if (!structure.ToString().StartsWith("Bunk"))
					{
						StructureSpawn structureSpawn = new StructureSpawn();
						float[] position = new float[3]
						{
							structure.transform.position.x,
							structure.transform.position.y,
							structure.transform.position.z
						};
						structureSpawn.Position = position;
						float[] rotation = new float[4]
						{
							structure.transform.rotation.x,
							structure.transform.rotation.y,
							structure.transform.rotation.z,
							structure.transform.rotation.w
						};
						structureSpawn.Rotation = rotation;
						structureSpawn.TeamIndex = structure.Team.Index;
						structureSpawn.Classname = structure.ToString().Split('(')[0];
						if (structure.DamageManager.Health01 < 0.99f)
						{
							structureSpawn.Health = structure.DamageManager.Health;
						}
						if (team.TechnologyTier > 0)
						{
							structureSpawn.TechTier = team.TechnologyTier;
						}
						if (structure.IsResourceHolder)
						{
							structureSpawn.Resources = structure.GetStoredResources();
						}
						if (includeNetIDs)
						{
							structureSpawn.NetID = structure.NetworkComponent.NetID;
						}
						spawnSetup.Structures.Add(structureSpawn);
					}
				}
				foreach (Unit unit in team.Units)
				{
					UnitSpawn unitSpawn = new UnitSpawn();
					float[] position2 = new float[3]
					{
						unit.transform.position.x,
						unit.transform.position.y,
						unit.transform.position.z
					};
					unitSpawn.Position = position2;
					Quaternion facingRotation = unit.GetFacingRotation();
					float[] rotation2 = new float[4] { facingRotation.x, facingRotation.y, facingRotation.z, facingRotation.w };
					unitSpawn.Rotation = rotation2;
					unitSpawn.TeamIndex = unit.Team.Index;
					unitSpawn.Classname = unit.ToString().Split('(')[0];
					if (unit.DamageManager.Health01 < 0.99f)
					{
						unitSpawn.Health = unit.DamageManager.Health;
					}
					if (unit.IsResourceHolder)
					{
						unitSpawn.Resources = unit.GetResourceCapacity();
					}
					if (includeNetIDs)
					{
						unitSpawn.NetID = unit.NetworkComponent.NetID;
					}
					spawnSetup.Units.Add(unitSpawn);
				}
			}
			return spawnSetup;
		}
	}
}
