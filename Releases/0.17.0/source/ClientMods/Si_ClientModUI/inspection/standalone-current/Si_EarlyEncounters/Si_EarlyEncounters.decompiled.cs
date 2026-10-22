using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using Microsoft.CodeAnalysis;
using Si_EarlyEncounters;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(EarlyEncounters), "Early Encounters", "1.1.7", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_EarlyEncounters")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_EarlyEncounters")]
[assembly: AssemblyTitle("Si_EarlyEncounters")]
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
namespace Si_EarlyEncounters
{
	public class EarlyEncounters : MelonMod
	{
		public enum EEncounterType
		{
			TeamResources,
			FriendlyUnit,
			EnemyWorm,
			EnemyUnit,
			SpecialSpawn,
			FriendlySwarm
		}

		[HarmonyPatch(typeof(StrategyTeamSetup), "SpawnAIUnits")]
		private static class EarlyEncounters_Patch_StrategyTeamSetup_SpawnAIUnits
		{
			public static void Postfix(StrategyTeamSetup __instance)
			{
				try
				{
					MelonLogger.Msg("Setting up random crates");
					for (int i = 0; i < _Pref_EarlyEncounters_CratesPerTeam.Value; i++)
					{
						SpawnRandomCrate();
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run StrategyTeamSetup::SpawnAIUnits");
				}
			}
		}

		[HarmonyPatch(typeof(OpenableBase), "OnUnitEnterZone")]
		private static class EarlyEncounters_Patch_OpenableBase_OnUnitEnterZone
		{
			public static void Postfix(OpenableBase __instance, Zone __0, Unit __1)
			{
				try
				{
					if (__instance == null || __instance.NetworkComponent == null || __instance.NetworkComponent.Owner == null || __instance.NetworkComponent.Owner.Team == null || __instance.NetworkComponent.Owner.Team.Index != 1)
					{
						return;
					}
					MelonLogger.Msg("Checking for wildlife crate.");
					ObjectInfo objectInfo = __instance.NetworkComponent.Owner.ObjectInfo;
					if (!(objectInfo == null) && objectInfo.DisplayName.Equals("Container") && !(__1 == null) && !(__1.NetworkComponent == null))
					{
						if (__1.NetworkComponent.OwnerPlayer == null)
						{
							UnityEngine.Object.Destroy(__instance.NetworkComponent.gameObject);
							MelonLogger.Msg("Removing wildlife crate opened by AI.");
							return;
						}
						EEncounterType encounterType = FindEncounterType();
						MelonLogger.Msg("Found player " + __1.NetworkComponent.OwnerPlayer.PlayerName + " opening container crate with " + __1.ToString() + " to find encounter " + encounterType);
						string text = HandleCrateEncounter(encounterType, __1.NetworkComponent.OwnerPlayer);
						HelperMethods.SendChatMessageToPlayer(__1.NetworkComponent.OwnerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Unlocked a crate with ", text);
						UnityEngine.Object.Destroy(__instance.NetworkComponent.gameObject);
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run OpenableBase::OnUnitEnterZone");
				}
			}
		}

		private static MelonPreferences_Category _Pref_modCategory = null;

		private static MelonPreferences_Entry<int> _Pref_EarlyEncounters_Chance_TeamResourceEvent = null;

		private static MelonPreferences_Entry<int> _Pref_EarlyEncounters_Chance_FriendlyUnitEvent = null;

		private static MelonPreferences_Entry<int> _Pref_EarlyEncounters_Chance_EnemyWormEvent = null;

		private static MelonPreferences_Entry<int> _Pref_EarlyEncounters_Chance_EnemyUnitEvent = null;

		private static MelonPreferences_Entry<int> _Pref_EarlyEncounters_Chance_SpecialSpawn = null;

		private static MelonPreferences_Entry<int> _Pref_EarlyEncounters_Chance_FriendlySwarm = null;

		private static MelonPreferences_Entry<int> _Pref_EarlyEncounters_CratesPerTeam = null;

		public static Dictionary<EEncounterType, MelonPreferences_Entry<int>> encounterProbabilities = new Dictionary<EEncounterType, MelonPreferences_Entry<int>>();

		public static string[] crateSpawnNames = new string[3] { "SpecialContainer_Long_01_Large", "SpecialContainer_Long_01_Small", "SpecialContainer_Short_01_Small" };

		public override void OnInitializeMelon()
		{
			if (_Pref_modCategory == null)
			{
				_Pref_modCategory = MelonPreferences.CreateCategory("Silica");
			}
			if (_Pref_EarlyEncounters_Chance_TeamResourceEvent == null)
			{
				_Pref_EarlyEncounters_Chance_TeamResourceEvent = _Pref_modCategory.CreateEntry<int>("EarlyEncounter_TeamResourceEventChance", 100, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_EarlyEncounters_Chance_FriendlyUnitEvent == null)
			{
				_Pref_EarlyEncounters_Chance_FriendlyUnitEvent = _Pref_modCategory.CreateEntry<int>("EarlyEncounter_FriendlyUnitEventChance", 100, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_EarlyEncounters_Chance_EnemyWormEvent == null)
			{
				_Pref_EarlyEncounters_Chance_EnemyWormEvent = _Pref_modCategory.CreateEntry<int>("EarlyEncounter_EnemyWormEventChance", 100, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_EarlyEncounters_Chance_EnemyUnitEvent == null)
			{
				_Pref_EarlyEncounters_Chance_EnemyUnitEvent = _Pref_modCategory.CreateEntry<int>("EarlyEncounter_EnemyUnitEventChance", 100, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_EarlyEncounters_Chance_SpecialSpawn == null)
			{
				_Pref_EarlyEncounters_Chance_SpecialSpawn = _Pref_modCategory.CreateEntry<int>("EarlyEncounters_SpecialSpawnEventChance", 35, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_EarlyEncounters_Chance_FriendlySwarm == null)
			{
				_Pref_EarlyEncounters_Chance_FriendlySwarm = _Pref_modCategory.CreateEntry<int>("EarlyEncounters_FriendlySwarmChance", 35, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_EarlyEncounters_CratesPerTeam == null)
			{
				_Pref_EarlyEncounters_CratesPerTeam = _Pref_modCategory.CreateEntry<int>("EarlyEncounter_CratesPerTeam", 8, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
		}

		public override void OnLateInitializeMelon()
		{
			FillEncounterDictionary();
		}

		private static void FillEncounterDictionary()
		{
			encounterProbabilities.Clear();
			encounterProbabilities.Add(EEncounterType.TeamResources, _Pref_EarlyEncounters_Chance_TeamResourceEvent);
			encounterProbabilities.Add(EEncounterType.FriendlyUnit, _Pref_EarlyEncounters_Chance_FriendlyUnitEvent);
			encounterProbabilities.Add(EEncounterType.EnemyWorm, _Pref_EarlyEncounters_Chance_EnemyWormEvent);
			encounterProbabilities.Add(EEncounterType.EnemyUnit, _Pref_EarlyEncounters_Chance_EnemyUnitEvent);
			encounterProbabilities.Add(EEncounterType.SpecialSpawn, _Pref_EarlyEncounters_Chance_SpecialSpawn);
			encounterProbabilities.Add(EEncounterType.FriendlySwarm, _Pref_EarlyEncounters_Chance_FriendlySwarm);
		}

		private static int TotalProbabilityValue()
		{
			int num = 0;
			foreach (KeyValuePair<EEncounterType, MelonPreferences_Entry<int>> encounterProbability in encounterProbabilities)
			{
				if (encounterProbability.Value.Value > 0)
				{
					num += encounterProbability.Value.Value;
				}
			}
			return num;
		}

		public static EEncounterType FindEncounterType()
		{
			int maxExclusive = TotalProbabilityValue();
			int num = UnityEngine.Random.Range(0, maxExclusive);
			MelonLogger.Msg("Crate Probability = (" + num + "/" + maxExclusive + ")");
			int num2 = 0;
			foreach (KeyValuePair<EEncounterType, MelonPreferences_Entry<int>> encounterProbability in encounterProbabilities)
			{
				num2 += encounterProbability.Value.Value;
				if (encounterProbability.Value.Value > 0 && num <= num2)
				{
					return encounterProbability.Key;
				}
			}
			MelonLogger.Warning("Could not find a valid random encounter. Check probability parameters.");
			return EEncounterType.EnemyWorm;
		}

		private static int GetResourceAwardAmount(Team team)
		{
			return 250 * (1 + team.TechnologyTierHighestReached);
		}

		private static string GetFriendlySwarmUnitName(Team team)
		{
			return team.Index switch
			{
				0 => "Crab_Horned", 
				3 => "Cent_Soldier_Marksman", 
				_ => "Sol_Soldier_Sniper", 
			};
		}

		private static string GetFriendlyUnitName(Team team)
		{
			return team.Index switch
			{
				0 => "Wasp", 
				3 => "Cent_Soldier_Trooper", 
				_ => "Sol_Soldier_Rifleman", 
			};
		}

		private static void SpawnNearby(Player player, string name, float startOffset, bool friendlyUnit = true)
		{
			Vector3 worldPhysicalCenter = player.ControlledUnit.WorldPhysicalCenter;
			Quaternion rotatedQuaternion = GameMath.GetRotatedQuaternion(Quaternion.identity, Vector3.up * UnityEngine.Random.Range(-180f, 180f));
			Vector3 vector = worldPhysicalCenter + rotatedQuaternion * Vector3.forward * UnityEngine.Random.Range(startOffset, startOffset + 10f);
			vector = GetFinalWorldMapPosition(new Vector2(vector.x, vector.z));
			int teamIndex = ((!friendlyUnit) ? 1 : player.Team.Index);
			HelperMethods.SpawnAtLocation(name, vector, rotatedQuaternion, teamIndex);
		}

		public static string HandleCrateEncounter(EEncounterType encounterType, Player player)
		{
			Vector3 worldPhysicalCenter = player.ControlledUnit.WorldPhysicalCenter;
			switch (encounterType)
			{
			case EEncounterType.TeamResources:
			{
				Team team = player.Team;
				int resourceAwardAmount = GetResourceAwardAmount(team);
				team.StoreResource(resourceAwardAmount);
				return "a Team Resource Bonus (" + resourceAwardAmount + ")";
			}
			case EEncounterType.FriendlyUnit:
			{
				for (int l = 0; l < 1; l++)
				{
					SpawnNearby(player, GetFriendlyUnitName(player.Team), 10f);
				}
				return "a Friendly Defector";
			}
			case EEncounterType.EnemyUnit:
			{
				for (int i = 0; i < 2; i++)
				{
					SpawnNearby(player, "Sol_Soldier_Heavy", 10f, friendlyUnit: false);
				}
				return "Enemey Forces";
			}
			case EEncounterType.SpecialSpawn:
			{
				if (player.Team.Index == 0)
				{
					for (int j = 0; j < 2; j++)
					{
						SpawnNearby(player, "Worm", 20f);
					}
					return "Friendly Worms!";
				}
				for (int k = 0; k < 1; k++)
				{
					SpawnNearby(player, "RetroHatchback", 20f);
				}
				return "a retro hatchback!";
			}
			case EEncounterType.FriendlySwarm:
			{
				for (int m = 0; m < 4; m++)
				{
					SpawnNearby(player, GetFriendlySwarmUnitName(player.Team), 15f);
				}
				return "a Friendly Swarm!";
			}
			case EEncounterType.EnemyWorm:
			{
				AmbientLife ambientLife = UnityEngine.Object.FindFirstObjectByType<AmbientLife>();
				if (ambientLife == null)
				{
					MelonLogger.Warning("Could not find AmbientLife instance.");
					return "a Worm";
				}
				Target.GetTargetByNetID(player.ControlledUnit.NetworkComponent.NetID);
				ObjectInfo objectInfo = ambientLife.Basic[UnityEngine.Random.Range(0, ambientLife.Basic.Count - 1)];
				Unit unit = Game.SpawnPrefab(rotation: GameMath.GetRotatedQuaternion(Quaternion.identity, Vector3.up * UnityEngine.Random.Range(-180f, 180f)), objectPrefab: objectInfo.Prefab, ownerPlayer: null, team: Team.Teams[1], position: worldPhysicalCenter).GetBaseGameObject() as Unit;
				if (unit == null)
				{
					UnityEngine.Object.Destroy(unit);
					MelonLogger.Warning("Could not spawn enemy worm correctly.");
					return "a Worm";
				}
				return "a Worm";
			}
			default:
				return "an invalid encounter (ERROR)";
			}
		}

		private static void SpawnRandomCrate()
		{
			Vector3 finalWorldMapPosition = GetFinalWorldMapPosition(GetWorldMapPositionFromPercentage(new Vector2(UnityEngine.Random.Range(0.01f, 0.99f), UnityEngine.Random.Range(0.01f, 0.99f))));
			string name = crateSpawnNames[UnityEngine.Random.Range(0, crateSpawnNames.Length)];
			Quaternion rotatedQuaternion = GameMath.GetRotatedQuaternion(Quaternion.identity, Vector3.up * UnityEngine.Random.Range(-180f, 180f));
			if (HelperMethods.SpawnAtLocation(name, finalWorldMapPosition, rotatedQuaternion, 1) == null)
			{
				MelonLogger.Warning("Failed to spawn crate");
			}
		}

		private static Vector2 GetWorldMapPositionFromPercentage(Vector2 mapPositionPercentages)
		{
			Bounds bounds = Game.MainTerrain.terrainData.bounds;
			Vector3 position = Game.MainTerrain.transform.position;
			return new Vector2(bounds.size.x * mapPositionPercentages.x + position.x, bounds.size.z * mapPositionPercentages.y + position.z);
		}

		private static Vector3 GetFinalWorldMapPosition(Vector2 mapPositions)
		{
			Vector3 vector = new Vector3
			{
				x = mapPositions.x,
				y = 2000f,
				z = mapPositions.y
			};
			if (Physics.Raycast(vector, Vector3.down, out var hitInfo, 4000f, GamePhysics.TRACELAYERMASK_IGNOREDEBRIS, QueryTriggerInteraction.Ignore))
			{
				return hitInfo.point;
			}
			MelonLogger.Warning("Could not find intercept with ground terrain for x,z (", new object[3]
			{
				(int)vector.x,
				", ",
				(int)vector.z
			});
			return vector;
		}
	}
}
