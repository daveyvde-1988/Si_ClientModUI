using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using Si_UnitBalance;
using Silica;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(UnitBalance), "Unit Balance", "7.7.1", "schwe", null)]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_UnitBalance")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+26b7a9c8a7c1a4cd47b5e7e56c9262cf03335de1")]
[assembly: AssemblyProduct("Si_UnitBalance")]
[assembly: AssemblyTitle("Si_UnitBalance")]
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
namespace Si_UnitBalance
{
	public class UnitBalance : MelonMod
	{
		private enum MenuLevel
		{
			Root,
			Faction,
			Category,
			Unit,
			ParamGroup,
			JsonMenu,
			JsonLoad,
			HTPMenu,
			HTPHoverbike,
			HTPHoverbikeParam,
			HTPTier,
			HTPTierTime,
			HTPTierCost,
			HTPTeleport,
			HTPDecay,
			HTPDecayFaction,
			HTPUnitCap,
			DiscordMenu,
			CapDecayMenu,
			CountCap,
			NestReach,
			QueensMax,
			StrategyMenu
		}

		private class BalanceMenuState
		{
			public MenuLevel Level;

			public int FactionIdx;

			public int CategoryIdx;

			public int UnitIdx;

			public int ParamGroupIdx;

			public int HTPParamGroupIdx;

			public int HTPTeleportStructIdx;

			public int HTPDecayFactionIdx;

			public int HTPUnitCapModeIdx;

			public int CountCapIdx;

			public bool PendingConfirm;

			public string PendingParamKey;

			public float PendingValue;

			public string PendingOldVal;

			public string PendingUnitName;

			public string PendingTechTierKey;

			public bool PendingIsTechCost;

			public bool JsonPendingSave;

			public string JsonPendingAction;

			public string JsonPendingFile;

			public string[] JsonLoadFileList;
		}

		private class DecaySettings
		{
			public bool Enabled = true;

			public float Delay = -1f;

			public float Tick = -1f;

			public float AmountPct = -1f;

			public float RandomizePct = -1f;

			public bool KeepProduction;

			public bool HasCustomParams
			{
				get
				{
					if (!(Delay >= 0f) && !(Tick >= 0f) && !(AmountPct >= 0f))
					{
						return RandomizePct >= 0f;
					}
					return true;
				}
			}

			public bool IsVanilla
			{
				get
				{
					if (Enabled && !HasCustomParams)
					{
						return !KeepProduction;
					}
					return false;
				}
			}
		}

		private struct BalanceChange(string section, string unitName, string paramName, string defaultVal, string activeVal, bool isNew, string baseInfo = null)
		{
			public string Section = section;

			public string UnitName = unitName;

			public string ParamName = paramName;

			public string DefaultVal = defaultVal;

			public string ActiveVal = activeVal;

			public bool IsNew = isNew;

			public string BaseInfo = baseInfo;
		}

		private static class Patch_GameInit
		{
			public static void Postfix()
			{
				_watchdogGeneration++;
				_watchdogFired = false;
				_lastMapName = GetCurrentMapName();
				MelonLogger.Msg("[UnitBalance] Game init (" + _lastMapName + ")");
			}
		}

		private static class Patch_GameRestart
		{
			public static void Postfix()
			{
				_watchdogGeneration++;
				_watchdogFired = false;
				string currentMapName = GetCurrentMapName();
				if (!string.Equals(currentMapName, _lastMapName, StringComparison.OrdinalIgnoreCase))
				{
					MelonLogger.Msg("[UnitBalance] Game restart — map changed (" + _lastMapName + " → " + currentMapName + ")");
					_lastMapName = currentMapName;
					_vanillaBaseCache.Clear();
					DecayCacheClear();
				}
				else
				{
					MelonLogger.Msg("[UnitBalance] Game restart (same map)");
				}
				_overridesApplied = false;
			}
		}

		private static class Patch_GameStarted
		{
			public static void Postfix()
			{
				_gameStartedRan = true;
				_watchdogGeneration++;
				_watchdogFired = false;
				if (_legacyOverrideMode && _enabled && _configLoaded)
				{
					MelonLogger.Msg("[UnitBalance] [LEGACY] Reverting + applying overrides (legacy mode — no OnFinishLoadingLevel)");
					_overridesApplied = false;
					_vanillaBaseCache.Clear();
					ApplyOverridesLogic();
				}
				OnGameStartedLogic();
			}
		}

		private static class Patch_OMRevertAll
		{
			public static bool Prefix()
			{
				if (_allowRevertAll)
				{
					return true;
				}
				if (_legacyOverrideMode)
				{
					return true;
				}
				LogDebug("[OverrideManager] RevertAll BLOCKED (game scene transition)");
				return false;
			}
		}

		private static class Patch_GameEnded
		{
			public static void Postfix()
			{
				MelonLogger.Msg("[UnitBalance] Game ended");
				_originalCreatureAttackAimDist.Clear();
				_originalSpread.Clear();
				_originalProjectileLifetimes.Clear();
				_originalProjectileSpeeds.Clear();
				_originalMoveSpeeds.Clear();
				_originalMeleeDamage.Clear();
				_originalHealth.Clear();
				_originalAimAngle.Clear();
				if (_watchdogEnabled && !_watchdogFired)
				{
					MelonCoroutines.Start(RoundTransitionWatchdog(_watchdogGeneration));
				}
			}
		}

		private static class Patch_UnitCapPlayerInfantry
		{
			public static void Postfix(Team __instance)
			{
				if (!_playerInfantryIgnoreCap || __instance == null)
				{
					return;
				}
				try
				{
					List<Unit> units = __instance.Units;
					if (units == null)
					{
						return;
					}
					for (int i = 0; i < units.Count; i++)
					{
						Unit unit = units[i];
						if (unit == null || unit.ObjectInfo == null || unit.ControlledBy == null || unit.ObjectInfo.UnitType != UnitType.Infantry)
						{
							continue;
						}
						UnitCapEntry unitTypeCapEntryForUnit = __instance.GetUnitTypeCapEntryForUnit(unit.ObjectInfo);
						if (unitTypeCapEntryForUnit != null)
						{
							unitTypeCapEntryForUnit.Current -= unit.ObjectInfo.UnitCapValue;
							if (unitTypeCapEntryForUnit.Current < 0)
							{
								unitTypeCapEntryForUnit.Current = 0;
							}
						}
					}
				}
				catch (Exception ex)
				{
					MelonLogger.Warning("[UnitCap] player-infantry exemption error: " + ex.Message);
				}
			}
		}

		private static class Patch_VehicleDispenser
		{
			public static bool Prefix(object __instance, object player)
			{
				try
				{
					if (_minTierOverrides.Count == 0)
					{
						return true;
					}
					FieldInfo field = __instance.GetType().GetField("VehicleToDispense", BindingFlags.Instance | BindingFlags.Public);
					if (field == null)
					{
						return true;
					}
					object value = field.GetValue(__instance);
					if (value == null)
					{
						return true;
					}
					FieldInfo field2 = value.GetType().GetField("DisplayName", BindingFlags.Instance | BindingFlags.Public);
					string text = null;
					if (field2 != null)
					{
						text = field2.GetValue(value) as string;
					}
					else
					{
						PropertyInfo property = value.GetType().GetProperty("DisplayName", BindingFlags.Instance | BindingFlags.Public);
						if (property != null)
						{
							text = property.GetValue(value) as string;
						}
					}
					if (string.IsNullOrEmpty(text))
					{
						return true;
					}
					if (!_minTierOverrides.TryGetValue(text, out var value2))
					{
						return true;
					}
					if (value2 < 0)
					{
						return true;
					}
					PropertyInfo property2 = __instance.GetType().GetProperty("Team", BindingFlags.Instance | BindingFlags.Public);
					if (property2 == null)
					{
						return true;
					}
					object value3 = property2.GetValue(__instance);
					if (value3 == null)
					{
						return true;
					}
					PropertyInfo property3 = value3.GetType().GetProperty("TechnologyTier", BindingFlags.Instance | BindingFlags.Public);
					if (property3 == null)
					{
						return true;
					}
					int num = (int)property3.GetValue(value3);
					if (num < value2)
					{
						MelonLogger.Msg($"[DISPENSER] Blocked '{text}' — team tier {num} < required {value2}");
						try
						{
							if (_sendChatToPlayerMethod != null && player != null)
							{
								string text2 = _chatPrefix + $"<color=#FF6666>{text}</color> requires tech tier <color=#FFD700>{value2}</color> (current: {num})";
								_sendChatToPlayerMethod.Invoke(null, new object[2]
								{
									player,
									new string[1] { text2 }
								});
							}
						}
						catch
						{
						}
						return false;
					}
				}
				catch (Exception ex)
				{
					MelonLogger.Warning("[DISPENSER] Tier check error: " + ex.Message);
				}
				return true;
			}
		}

		private static class Patch_HumanStructure_UpdateHealDecay
		{
			public static bool Prefix(object __instance)
			{
				try
				{
					if (!_decayHuman.KeepProduction || !_decayHuman.Enabled)
					{
						return true;
					}
					HumanStructure humanStructure = __instance as HumanStructure;
					if (humanStructure == null)
					{
						return true;
					}
					if (_humanAnchorAccessor == null)
					{
						if (_humanAnchorField == null)
						{
							_humanAnchorField = typeof(HumanStructure).GetField("AnchorStructure", BindingFlags.Instance | BindingFlags.NonPublic);
						}
						if (_humanAnchorField == null)
						{
							return true;
						}
						_humanAnchorAccessor = AccessTools.FieldRefAccess<HumanStructure, Structure>(_humanAnchorField);
					}
					Structure structure = _humanAnchorAccessor.Invoke(humanStructure);
					bool flag = !humanStructure.IsConstructionAnchor && structure == null;
					int instanceID = humanStructure.GetInstanceID();
					if (!_dmCacheHuman.TryGetValue(instanceID, out DamageManager value) || value == null)
					{
						value = humanStructure.gameObject.GetComponent<DamageManager>();
						if (value != null)
						{
							_dmCacheHuman[instanceID] = value;
						}
					}
					if (value != null && value.DisableAutoHeal != flag)
					{
						value.DisableAutoHeal = flag;
					}
					if (humanStructure.Decay != null && humanStructure.Decay.enabled != flag)
					{
						humanStructure.Decay.enabled = flag;
					}
					return false;
				}
				catch
				{
					return true;
				}
			}
		}

		private static class Patch_AlienStructure_UpdateHealDecay
		{
			public static bool Prefix(object __instance)
			{
				try
				{
					if (!_decayAlien.KeepProduction || !_decayAlien.Enabled)
					{
						return true;
					}
					AlienStructure alienStructure = __instance as AlienStructure;
					if (alienStructure == null)
					{
						return true;
					}
					bool flag = !alienStructure.IsLinkedToMainBuilding;
					int instanceID = alienStructure.GetInstanceID();
					if (!_dmCacheAlien.TryGetValue(instanceID, out DamageManager value) || value == null)
					{
						value = alienStructure.gameObject.GetComponent<DamageManager>();
						if (value != null)
						{
							_dmCacheAlien[instanceID] = value;
						}
					}
					if (value != null && value.DisableAutoHeal != flag)
					{
						value.DisableAutoHeal = flag;
					}
					if (alienStructure.Decay != null && alienStructure.Decay.enabled != flag)
					{
						alienStructure.Decay.enabled = flag;
					}
					return false;
				}
				catch
				{
					return true;
				}
			}
		}

		private static class Patch_GetSpawnablePrefabIndex
		{
			public static bool Prefix(string prefabName, ref int __result)
			{
				try
				{
					__result = -1;
					GameDatabase database = GameDatabase.Database;
					if (database == null || prefabName == null)
					{
						return false;
					}
					List<GameObject> allNetworkedPrefabs = database.AllNetworkedPrefabs;
					if (allNetworkedPrefabs == null)
					{
						return false;
					}
					for (int i = 0; i < allNetworkedPrefabs.Count; i++)
					{
						GameObject gameObject = allNetworkedPrefabs[i];
						if (!(gameObject == null) && string.Equals(gameObject.name, prefabName, StringComparison.OrdinalIgnoreCase))
						{
							__result = i;
							return false;
						}
					}
					return false;
				}
				catch
				{
					return true;
				}
			}
		}

		private static class Patch_CountCap
		{
			public static bool Prefix(Structure __instance, ConstructionData constructionData, bool isClientRequest, ref ProductionActionResult __result)
			{
				try
				{
					if (__instance == null || constructionData == null || constructionData.ObjectInfo == null)
					{
						return true;
					}
					Team team = __instance.Team;
					if (team == null)
					{
						return true;
					}
					int num = CountCapFor(constructionData.ObjectInfo, out string label);
					if (num < 0)
					{
						return true;
					}
					int num2 = CountTeamUnitsOf(team, constructionData.ObjectInfo);
					if (num2 < num)
					{
						return true;
					}
					__result = ProductionActionResult.UnitCap;
					LogDebug(string.Format("[COUNTCAP] {0}: {1} {2}/{3} — refused {4}{5}", team.TeamShortName, label, num2, num, constructionData.ObjectInfo.DisplayName, isClientRequest ? " (client request)" : ""));
					string key = team.TeamShortName + "|" + label;
					float realtimeSinceStartup = Time.realtimeSinceStartup;
					if (!_countCapLastWarn.TryGetValue(key, out var value) || realtimeSinceStartup - value > 3f)
					{
						_countCapLastWarn[key] = realtimeSinceStartup;
						Player player = (GameMode.CurrentGameMode as GameModeExt)?.GetCommanderForTeam(team);
						if (player != null)
						{
							SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>" + label + " cap reached</color> " + _dimColor + "(" + num2 + "/" + num + ") — no more can be built.</color>");
						}
					}
					return false;
				}
				catch (Exception ex)
				{
					MelonLogger.Warning("[CountCap] prefix error: " + ex.Message);
					return true;
				}
			}
		}

		private static class Patch_NestReach
		{
			public static bool Prefix(Structure __instance, ConstructionData constructionData, Vector3 worldPosition, bool isClientRequest, ref ProductionActionResult __result)
			{
				try
				{
					if (_alienNestReachRadius < 0f)
					{
						return true;
					}
					if (__instance == null || constructionData == null || !constructionData.Placeable)
					{
						return true;
					}
					if (!IsNestReachLimited(constructionData.ObjectInfo, out string label))
					{
						return true;
					}
					Team team = __instance.Team;
					if (team == null)
					{
						return true;
					}
					float num = NearestNestDistance2D(team, worldPosition, out var nests);
					if (num <= _alienNestReachRadius)
					{
						return true;
					}
					__result = ProductionActionResult.AreaObstructed;
					string text = ((nests > 0) ? (num.ToString("F0", CultureInfo.InvariantCulture) + " m") : "no Nest");
					LogDebug(string.Format("[NESTREACH] {0}: {1} refused at {2} — nearest Nest {3} > {4:F0} m{5}", team.TeamShortName, label, worldPosition, text, _alienNestReachRadius, isClientRequest ? " (client request)" : ""));
					string teamShortName = team.TeamShortName;
					float realtimeSinceStartup = Time.realtimeSinceStartup;
					if (!_nestReachLastWarn.TryGetValue(teamShortName, out var value) || realtimeSinceStartup - value > 3f)
					{
						_nestReachLastWarn[teamShortName] = realtimeSinceStartup;
						Player player = (GameMode.CurrentGameMode as GameModeExt)?.GetCommanderForTeam(team);
						if (player != null)
						{
							SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>" + label + " out of Nest reach</color> " + _dimColor + "(nearest Nest " + text + ", limit " + _alienNestReachRadius.ToString("F0", CultureInfo.InvariantCulture) + " m) — build closer to a Nest. A Nest is required for further expansion.</color>");
							PlaySoundToPlayer(player, "sounds/access_denied.wav");
						}
					}
					return false;
				}
				catch (Exception ex)
				{
					MelonLogger.Warning("[NestReach] prefix error: " + ex.Message);
					return true;
				}
			}
		}

		private static class Patch_QueenLogisticsPerNest
		{
			public static bool Prefix(Structure __instance, ConstructionData constructionData, bool isClientRequest, ref ProductionActionResult __result)
			{
				try
				{
					if (!_alienQueenLogisticsPerNest)
					{
						return true;
					}
					if (ConstructionSite.LimitationsDisabled)
					{
						return true;
					}
					AlienStructure alienStructure = __instance as AlienStructure;
					if (alienStructure == null || constructionData == null)
					{
						return true;
					}
					Team team = alienStructure.Team;
					if (team == null)
					{
						return true;
					}
					Structure structure = NearestNest(team, alienStructure.transform.position, out var dist);
					if (structure == null)
					{
						return true;
					}
					if (HasDockedLogisticsUnit(structure))
					{
						return true;
					}
					__result = ProductionActionResult.MissingLogistics;
					string text = ((constructionData.ObjectInfo != null) ? constructionData.ObjectInfo.DisplayName : "build");
					string text2 = ((alienStructure.ObjectInfo != null) ? alienStructure.ObjectInfo.DisplayName : "structure");
					bool flag = structure == __instance;
					string text3 = (flag ? "itself" : (dist.ToString("F0", CultureInfo.InvariantCulture) + " m away"));
					LogDebug("[QUEENLOG] " + team.TeamShortName + ": " + text + " from " + text2 + " refused — nearest Nest (" + text3 + ") has no docked Queen" + (isClientRequest ? " (client request)" : ""));
					string teamShortName = team.TeamShortName;
					float realtimeSinceStartup = Time.realtimeSinceStartup;
					if (!_queenLogLastWarn.TryGetValue(teamShortName, out var value) || realtimeSinceStartup - value > 3f)
					{
						_queenLogLastWarn[teamShortName] = realtimeSinceStartup;
						Player player = (GameMode.CurrentGameMode as GameModeExt)?.GetCommanderForTeam(team);
						if (player != null)
						{
							string text4 = (flag ? "this Nest has no docked Queen" : ("its Nest (nearest, " + text3 + ") has no docked Queen"));
							SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Queen away — " + text2 + " can't build</color> " + _dimColor + "(" + text4 + ") — return the Queen to that Nest.</color>");
							PlaySoundToPlayer(player, "sounds/access_denied.wav");
						}
					}
					return false;
				}
				catch (Exception ex)
				{
					MelonLogger.Warning("[QueenLog] prefix error: " + ex.Message);
					return true;
				}
			}
		}

		private static class Patch_NestFinishedAnchor
		{
			public static bool Prefix(Structure __instance, ConstructionData constructionData, Vector3 worldPosition, Quaternion worldRotation, bool isClientRequest, ref ProductionActionResult __result)
			{
				try
				{
					if (!_alienNestFinishedAnchorOnly)
					{
						return true;
					}
					if (ConstructionSite.LimitationsDisabled)
					{
						return true;
					}
					if (__instance == null || constructionData == null || !constructionData.Placeable || constructionData.ObjectInfo == null)
					{
						return true;
					}
					Team team = __instance.Team;
					if (team == null || !IsNestInfo(constructionData.ObjectInfo, team))
					{
						return true;
					}
					float maximumBaseStructureDistance = constructionData.MaximumBaseStructureDistance;
					float num = NearestFinishedAnchorDistance2D(team, constructionData, worldPosition, worldRotation, out Structure anchor);
					if (num <= maximumBaseStructureDistance)
					{
						return true;
					}
					__result = ProductionActionResult.AreaObstructed;
					ConstructionSite constructionSite = FindNestUnderConstruction(team);
					string text = ((constructionSite != null) ? (Mathf.RoundToInt(Mathf.Clamp01(constructionSite.ProgressTime01) * 100f).ToString(CultureInfo.InvariantCulture) + "%") : null);
					string text2 = ((anchor != null) ? (num.ToString("F0", CultureInfo.InvariantCulture) + " m") : "none");
					LogDebug(string.Format("[NESTANCHOR] {0}: Nest refused at {1} — nearest finished anchor {2} > {3:F0} m{4}{5}", team.TeamShortName, worldPosition, text2, maximumBaseStructureDistance, (text != null) ? (", Nest under construction " + text) : "", isClientRequest ? " (client request)" : ""));
					string teamShortName = team.TeamShortName;
					float realtimeSinceStartup = Time.realtimeSinceStartup;
					if (!_nestAnchorLastWarn.TryGetValue(teamShortName, out var value) || realtimeSinceStartup - value > 3f)
					{
						_nestAnchorLastWarn[teamShortName] = realtimeSinceStartup;
						Player player = (GameMode.CurrentGameMode as GameModeExt)?.GetCommanderForTeam(team);
						if (player != null)
						{
							string text3 = ((text != null) ? ("the Nest under construction (" + text + ") can't anchor it yet — wait for it to finish or build next to a finished Nest.") : ("build within " + maximumBaseStructureDistance.ToString("F0", CultureInfo.InvariantCulture) + " m of a finished Nest."));
							SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Nest needs a finished anchor</color> " + _dimColor + "(nearest finished anchor " + text2 + ") — " + text3 + "</color>");
							PlaySoundToPlayer(player, "sounds/access_denied.wav");
						}
					}
					return false;
				}
				catch (Exception ex)
				{
					MelonLogger.Warning("[NestAnchor] prefix error: " + ex.Message);
					return true;
				}
			}
		}

		private static class Patch_UnitCapMultVersus
		{
			public static void Postfix(MP_Strategy __instance)
			{
				if (_enabled && !(__instance == null))
				{
					ApplyUnitCapMultiplier("versus mode set");
				}
			}
		}

		private static class Patch_UnitCapMultSetting
		{
			public static void Postfix(GameModeExt __instance)
			{
				if (_enabled && !(__instance == null) && !_inVanillaCapMultRefresh && __instance.TeamsVersus != GameModeExt.ETeamsVersus.NONE)
				{
					ApplyUnitCapMultiplier("capmult refresh");
				}
			}
		}

		private static class Patch_ConstructionSpawn
		{
			private static MethodInfo _healthSetter;

			public static void Prefix(object __instance)
			{
			}
		}

		private static class Patch_BaseSpawn
		{
			public static void Prefix()
			{
				try
				{
					if (_enabled && _configLoaded && !_overridesApplied)
					{
						MelonLogger.Msg("[BASESPAWN] overrides not applied yet at the base spawn — applying now");
						ApplyOverridesLogic();
					}
				}
				catch (Exception ex)
				{
					MelonLogger.Warning("[BASESPAWN] prefix threw: " + ex.Message);
				}
			}

			public static void Postfix()
			{
				try
				{
					if (!_enabled || !_configLoaded)
					{
						return;
					}
					MelonLogger.Msg("[BASESPAWN] base structures spawned — propagating and re-syncing");
					try
					{
						PropagateToLiveInstances();
					}
					catch (Exception ex)
					{
						MelonLogger.Warning("[BASESPAWN] propagation threw: " + ex.Message);
					}
					ProbeLiveFoW("after-base-spawn");
					try
					{
						MelonCoroutines.Start(SyncOverridesToAllPlayers(0.5f));
					}
					catch (Exception ex2)
					{
						MelonLogger.Warning("[BASESPAWN] sync threw: " + ex2.Message);
					}
				}
				catch (Exception ex3)
				{
					MelonLogger.Warning("[BASESPAWN] postfix threw: " + ex3.Message);
				}
			}
		}

		private static class Patch_SendPlayerOverrides
		{
			public static bool Prefix(object __0)
			{
				if (_inChunkedSend)
				{
					return true;
				}
				if (_overrideManagerType == null || _sendPlayerOverridesMethod == null)
				{
					return true;
				}
				FieldInfo field = _overrideManagerType.GetField("_overrides", BindingFlags.Static | BindingFlags.NonPublic);
				if (field == null)
				{
					return true;
				}
				if (!(field.GetValue(null) is IDictionary { Count: not 0 } dictionary))
				{
					return false;
				}
				PropertyInfo propertyInfo = _overrideManagerType.GetNestedType("MemberOverride", BindingFlags.Public | BindingFlags.NonPublic)?.GetProperty("Enabled");
				List<object> list = new List<object>();
				foreach (DictionaryEntry item in dictionary)
				{
					if (!(item.Value is IDictionary { Count: not 0 } dictionary2))
					{
						continue;
					}
					bool flag = false;
					if (propertyInfo != null)
					{
						foreach (DictionaryEntry item2 in dictionary2)
						{
							if ((bool)propertyInfo.GetValue(item2.Value))
							{
								flag = true;
								break;
							}
						}
					}
					else
					{
						flag = true;
					}
					if (flag)
					{
						list.Add(item.Key);
					}
				}
				if (list.Count == 0)
				{
					return false;
				}
				MelonLogger.Msg($"[OverrideSync] Chunking {list.Count} target groups to player");
				Type type = dictionary.GetType();
				_inChunkedSend = true;
				int num = 0;
				try
				{
					foreach (object item3 in list)
					{
						IDictionary dictionary3 = (IDictionary)Activator.CreateInstance(type);
						dictionary3.Add(item3, dictionary[item3]);
						field.SetValue(null, dictionary3);
						_sendPlayerOverridesMethod.Invoke(null, new object[1] { __0 });
						num++;
					}
				}
				catch (Exception ex)
				{
					MelonLogger.Warning($"[OverrideSync] Chunked send error at target #{num}: {ex.InnerException?.Message ?? ex.Message}");
				}
				finally
				{
					_inChunkedSend = false;
					field.SetValue(null, dictionary);
				}
				MelonLogger.Msg($"[OverrideSync] Sent {num}/{list.Count} target groups");
				return false;
			}
		}

		private class StatsViewState
		{
			public string UnitName;

			public string[] GroupNames;

			public string[][] GroupKeys;

			public int Level;

			public int GroupIdx;
		}

		private static string _configPath = "";

		private static string _configSaveDir = "";

		private static bool _configLoaded;

		private static bool _fieldsDumped;

		private static bool _enabled = true;

		private static bool _dumpFields = true;

		private static bool _shrimpDisableAim = false;

		private static bool _additionalSpawn = false;

		private static bool _playerInfantryIgnoreCap = false;

		private static bool _alienNestOnlyBuild = false;

		private static int _countCapShrimp = -1;

		private static int _countCapHarvester = -1;

		private static float _alienNestReachRadius = -1f;

		private static bool _alienNestFinishedAnchorOnly = false;

		private static bool _alienQueenLogisticsPerNest = false;

		private static int _alienQueensMax = -1;

		private static int _unitCapMult2Way = -1;

		private static int _unitCapMult3Way = -1;

		private static bool _healthMultEnabled = true;

		private static bool _overridesApplied;

		private static bool _discordAutoPost = false;

		private static string _discordWebhookUrl = "";

		private static bool _harmonyApplied;

		private static bool _debugLogging = false;

		private static Type _overrideManagerType;

		private static MethodInfo _omSetMethod;

		private static MethodInfo _omRevertAllMethod;

		private static object _omTypeFloat;

		private static object _omTypeInt;

		private static object _omTypeBool;

		private static bool _omInitialized;

		private static MethodInfo _sendPlayerOverridesMethod;

		private static bool _inChunkedSend;

		private static MethodInfo _sendChatToPlayerMethod;

		private static MethodInfo _sendConsoleToPlayerMethod;

		private static MethodInfo _playSoundFileMethod;

		private static MethodInfo _registerAdminCmdMethod;

		private static MethodInfo _registerPlayerCmdMethod;

		private static Type _adminPowerType;

		private static Type _adminCallbackType;

		private static FieldInfo _playerNameField;

		private static FieldInfo _playerIdField;

		private static readonly Dictionary<long, BalanceMenuState> _menuStates = new Dictionary<long, BalanceMenuState>();

		private static readonly Dictionary<string, float> _damageMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _healthMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _costMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _buildTimeMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, int> _minTierOverrides = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _rangeMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _speedMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _reloadTimeMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _moveSpeedMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _accuracyMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _magazineMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _fireRateMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _buildRadiusOverrides = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _depositRadiusOverrides = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _extractionRadiusOverrides = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _turnRadiusMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _turboSpeedMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _targetDistanceOverrides = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _jumpSpeedMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _fowDistanceOverrides = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _visibleEventRadiusMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _lifetimeMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _strafeSpeedMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _flySpeedMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _runSpeedMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _sprintSpeedMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _splashRadiusMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, int> _unitCapOverrides = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<int, float> _techTierTimes = new Dictionary<int, float>();

		private static readonly Dictionary<int, int> _techTierCosts = new Dictionary<int, int>();

		private static readonly Dictionary<string, float> _originalHealth = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _originalAimAngle = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _originalMeleeDamage = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _originalMoveSpeeds = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<int, string> _nameCache = new Dictionary<int, string>();

		private static readonly Dictionary<string, Dictionary<string, Dictionary<string, float>>> _projectileOverrides = new Dictionary<string, Dictionary<string, Dictionary<string, float>>>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, bool> _impactScaleBySpeedOverrides = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _dragMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _aiLeadDragOverrides = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _aiLeadVelocityOverrides = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _aiLeadGravityOverrides = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, int> _aimCompensationOverrides = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, int> _aimCompVanilla = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _pitchMaxUpOverrides = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _pitchMaxDownOverrides = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, float> _pitchVanilla = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, Dictionary<string, float>> _proximityOverrides = new Dictionary<string, Dictionary<string, float>>(StringComparer.OrdinalIgnoreCase);

		private static float _teleportCooldown = -1f;

		private static float _teleportDuration = -1f;

		private static float _dispenseTimeout = -1f;

		private static DecaySettings _decayHuman = new DecaySettings();

		private static DecaySettings _decayAlien = new DecaySettings();

		private static readonly HttpClient _httpClient = new HttpClient();

		private static List<string> _discordMessageIds = new List<string>();

		private static readonly (string commentKey, string displayName)[] _discordSections = new (string, string)[18]
		{
			("_comment_sol_barracks", "Sol -- Barracks"),
			("_comment_sol_lf", "Sol -- Light Factory"),
			("_comment_sol_hf", "Sol -- Heavy Factory"),
			("_comment_sol_uhf", "Sol -- Ultra Heavy Factory"),
			("_comment_sol_air", "Sol -- Air Factory"),
			("_comment_struct", "Structures (Sol/Centauri)"),
			("_comment_cen_barracks", "Centauri -- Barracks"),
			("_comment_cen_lf", "Centauri -- Light Factory"),
			("_comment_cen_hf", "Centauri -- Heavy Factory"),
			("_comment_cen_uhf", "Centauri -- Ultra Heavy Factory"),
			("_comment_cen_air", "Centauri -- Air Factory"),
			("_comment_htp", "Hover Bike"),
			("_comment_alien_lesser", "Alien -- Lesser Spawning Cyst"),
			("_comment_alien_greater", "Alien -- Greater Spawning Cyst"),
			("_comment_alien_grand", "Alien -- Grand Spawning Cyst"),
			("_comment_alien_colossal", "Alien -- Colossal Spawning Cyst"),
			("_comment_alien_nest", "Alien -- Nest"),
			("_comment_alien_struct", "Alien -- Structures")
		};

		private static readonly HashSet<string> _skipParams = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "_base", "_pri_weapon", "_sec_weapon", "_base_speed", "_base_sense", "_weapon", "_base_movement", "_note" };

		private static bool _hqDecayWarningHooked = false;

		private const float VanillaDecayDelaySeconds = 10f;

		private const float HQDirectionDeadzoneM = 600f;

		private static bool _gameStartedRan;

		private static int _watchdogGeneration;

		private const float WatchdogSilentSeconds = 20f;

		private const float WatchdogCountdownSeconds = 60f;

		private static bool _watchdogEnabled = true;

		private static bool _watchdogFired;

		private static string _lastMapName = "";

		private static bool _legacyOverrideMode = false;

		private static bool _allowRevertAll;

		private static readonly (string faction, string unitName, int count)[] _additionalSpawnMap = new (string, string, int)[10]
		{
			("Sol", "Platoon Hauler", 2),
			("Sol", "Light Striker", 2),
			("Sol", "Heavy Quad", 2),
			("Centauri", "Squad Transport", 2),
			("Centauri", "Assault Car", 2),
			("Centauri", "Heavy Raider", 2),
			("Alien", "Hunter", 3),
			("Alien", "Squid", 2),
			("Alien", "Shocker", 4),
			("Alien", "Wasp", 1)
		};

		private static readonly Dictionary<string, Dictionary<string, string>> _vanillaBaseCache = new Dictionary<string, Dictionary<string, string>>();

		private static ObjectInfo[] _sharedAllInfos = null;

		private static readonly string[][] _factionNames = new string[3][]
		{
			new string[1] { "Sol" },
			new string[1] { "Centauri" },
			new string[1] { "Alien" }
		};

		private static readonly string[][] _categoryNames = new string[3][]
		{
			new string[6] { "Barracks", "Light Factory", "Heavy Factory", "Ultra Heavy Factory", "Air Factory", "Structures" },
			new string[6] { "Barracks", "Light Factory", "Heavy Factory", "Ultra Heavy Factory", "Air Factory", "Structures" },
			new string[6] { "Lesser Spawning Cyst", "Greater Spawning Cyst", "Grand Spawning Cyst", "Colossal Spawning Cyst", "Nest", "Structures" }
		};

		private static readonly string[][][] _unitNames = new string[3][][]
		{
			new string[6][]
			{
				new string[5] { "Scout", "Rifleman", "Sniper", "Heavy", "Commando" },
				new string[6] { "Light Quad", "Platoon Hauler", "Heavy Quad", "Light Striker", "Heavy Striker", "AA Truck" },
				new string[5] { "Hover Tank", "Barrage Truck", "Railgun Tank", "Pulse Truck", "Repair Rig" },
				new string[2] { "Sol Harvester", "Siege Tank" },
				new string[4] { "Gunship", "Dropship", "Fighter", "Bomber" },
				new string[12]
				{
					"Sol Headquarters", "Refinery", "Research Facility", "Barracks", "Air Factory", "Heavy Factory", "Ultra Heavy Factory", "Silo", "Radar Station", "Turret",
					"Heavy Turret", "Anti-Air Rocket Turret"
				}
			},
			new string[6][]
			{
				new string[5] { "Militia", "Trooper", "Marksman", "Juggernaut", "Templar" },
				new string[6] { "Light Raider", "Squad Transport", "Heavy Raider", "Assault Car", "Strike Tank", "Flak Car" },
				new string[5] { "Combat Tank", "Rocket Tank", "Heavy Tank", "Pyro Tank", "Repair Truck" },
				new string[2] { "Cent Harvester", "Crimson Tank" },
				new string[4] { "Shuttle", "Dreadnought", "Interceptor", "Freighter" },
				new string[12]
				{
					"Cent Headquarters", "Refinery", "Research Facility", "Barracks", "Air Factory", "Heavy Factory", "Ultra Heavy Factory", "Silo", "Radar Station", "Turret",
					"Heavy Turret", "Anti-Air Rocket Turret"
				}
			},
			new string[6][]
			{
				new string[6] { "Crab", "Shrimp", "Shocker", "Wasp", "Dragonfly", "Squid" },
				new string[5] { "Horned Crab", "Hunter", "Behemoth", "Scorpion", "Firebug" },
				new string[1] { "Goliath" },
				new string[2] { "Defiler", "Colossus" },
				new string[1] { "Queen" },
				new string[10] { "Nest", "Node", "Bio Cache", "Lesser Spawning Cyst", "Greater Spawning Cyst", "Grand Spawning Cyst", "Colossal Spawning Cyst", "Quantum Cortex", "Hive Spire", "Thorn Spire" }
			}
		};

		private static readonly string[] _paramGroupNames = new string[4] { "Health & Production", "Damage & Weapons", "Movement", "Vision & Sense" };

		private static readonly string[][] _paramGroupKeys = new string[4][]
		{
			new string[7] { "health_mult", "cost_mult", "build_time_mult", "min_tier", "build_radius", "deposit_radius", "extraction_radius" },
			new string[7] { "damage_mult", "range_mult", "proj_speed_mult", "accuracy_mult", "magazine_mult", "fire_rate_mult", "reload_time_mult" },
			new string[4] { "move_speed_mult", "jump_speed_mult", "turbo_speed_mult", "turn_radius_mult" },
			new string[3] { "target_distance", "fow_distance", "visible_event_radius_mult" }
		};

		private static readonly string[] _weaponParamKeys = new string[7] { "damage_mult", "proj_speed_mult", "proj_lifetime_mult", "accuracy_mult", "magazine_mult", "fire_rate_mult", "reload_time_mult" };

		private static readonly string[] _weaponParamKeysInfantry = new string[3] { "damage_mult", "proj_speed_mult", "proj_lifetime_mult" };

		private static readonly string[] _weaponParamKeysCreatureRanged = new string[4] { "damage_mult", "proj_speed_mult", "proj_lifetime_mult", "accuracy_mult" };

		private static readonly string[] _weaponParamKeysCreatureMelee = new string[1] { "damage_mult" };

		private static readonly string _chatPrefix = "<b><color=#DDE98C>[</color><color=#7DD4FF>BAL</color><color=#DDE98C>]</color></b> ";

		private static readonly string _headerColor = "<color=#FFD700>";

		private static readonly string _itemColor = "<color=#AAFFAA>";

		private static readonly string _valueColor = "<color=#FFB86C>";

		private static readonly string _dimColor = "<color=#888888>";

		private static string _auditLogPath = "";

		private static readonly string[] _damageFields = new string[4] { "m_fImpactDamage", "m_fRicochetDamage", "m_fSplashDamageMax", "m_fPenetratingDamage" };

		private static readonly Dictionary<string, string> _damageFieldToSubtype = new Dictionary<string, string>
		{
			{ "m_fImpactDamage", "impact:" },
			{ "m_fRicochetDamage", "ricochet:" },
			{ "m_fSplashDamageMax", "splash:" },
			{ "m_fPenetratingDamage", "penetrating:" }
		};

		private static readonly Dictionary<string, int> _vtPriIndex = new Dictionary<string, int>
		{
			{ "Bomber", 1 },
			{ "Freighter", 1 },
			{ "Shuttle", 1 },
			{ "Gunship", 1 },
			{ "Platoon Hauler", 1 }
		};

		private static readonly string[] _splashRadiusFields = new string[3] { "m_fSplashDamageRadiusMax", "m_fSplashDamageRadiusMin", "m_fSplashDamageRadiusPow" };

		private static readonly Dictionary<string, string> _splashRadiusFieldToKey = new Dictionary<string, string>
		{
			{ "m_fSplashDamageRadiusMax", "max:" },
			{ "m_fSplashDamageRadiusMin", "min:" },
			{ "m_fSplashDamageRadiusPow", "pow:" }
		};

		private static readonly Dictionary<string, float> _originalCreatureAttackAimDist = new Dictionary<string, float>();

		private static readonly Dictionary<string, float> _originalSpread = new Dictionary<string, float>();

		private static readonly Dictionary<string, float> _originalProjectileLifetimes = new Dictionary<string, float>();

		private static readonly Dictionary<string, float> _originalProjectileSpeeds = new Dictionary<string, float>();

		private static readonly Dictionary<string, float> _originalAimLeadVelocity = new Dictionary<string, float>();

		private static readonly Dictionary<string, float> _originalAimLeadGravity = new Dictionary<string, float>();

		private static readonly Dictionary<string, float> _originalAimLeadDrag = new Dictionary<string, float>();

		private static readonly string[] _speedFieldNames = new string[8] { "MoveSpeed", "FlyMoveSpeed", "WalkSpeed", "RunSpeed", "SprintSpeed", "ForwardSpeed", "StrafeSpeed", "TurboSpeed" };

		private static readonly Dictionary<string, GameObject> _networkedPrefabs = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

		private static readonly HashSet<int> _fowPrefabsTouched = new HashSet<int>();

		private static readonly HashSet<string> _nestOnlyMutated = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private static int? _originalQueenMaxPerTeam;

		private static ScriptableObject _decayDataHuman;

		private static ScriptableObject _decayDataAlien;

		private static FieldInfo _humanAnchorField;

		private static FieldRef<HumanStructure, Structure>? _humanAnchorAccessor;

		private static readonly Dictionary<int, DamageManager> _dmCacheHuman = new Dictionary<int, DamageManager>();

		private static readonly Dictionary<int, DamageManager> _dmCacheAlien = new Dictionary<int, DamageManager>();

		private static readonly Dictionary<string, float> _countCapLastWarn = new Dictionary<string, float>();

		private const string NEST_REACH_SOUND = "sounds/access_denied.wav";

		private static readonly Dictionary<string, float> _nestReachLastWarn = new Dictionary<string, float>();

		private static bool? _nestReachSoundAvailable;

		private static readonly Dictionary<string, float> _nestAnchorLastWarn = new Dictionary<string, float>();

		private static readonly Dictionary<string, float> _queenLogLastWarn = new Dictionary<string, float>();

		private static bool _inVanillaCapMultRefresh;

		private static readonly Dictionary<long, StatsViewState> _statsStates = new Dictionary<long, StatsViewState>();

		internal static void LogDebug(string msg)
		{
			if (_debugLogging)
			{
				MelonLogger.Msg(msg);
			}
		}

		private static string ResolveConfigName(string displayName, string internalName)
		{
			if (!string.IsNullOrEmpty(internalName))
			{
				if (string.Equals(displayName, "Harvester", StringComparison.OrdinalIgnoreCase))
				{
					if (internalName.IndexOf("_Sol_", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return "Sol Harvester";
					}
					if (internalName.IndexOf("_Cent_", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return "Cent Harvester";
					}
				}
				else if (string.Equals(displayName, "Headquarters", StringComparison.OrdinalIgnoreCase))
				{
					if (internalName.IndexOf("_Sol_", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return "Sol Headquarters";
					}
					if (internalName.IndexOf("_Cent_", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return "Cent Headquarters";
					}
				}
			}
			return displayName;
		}

		public override void OnInitializeMelon()
		{
			string text = Path.Combine(MelonEnvironment.UserDataDirectory, "UnitBalance_cfg");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			_configPath = Path.Combine(text, "Si_UnitBalance_Config.json");
			_configSaveDir = Path.Combine(text, "Saved_Configs");
			LoadConfig();
			TryApplyHarmonyPatches();
			SceneManager.sceneLoaded += OnSceneLoaded;
			try
			{
				FieldInfo field = typeof(GameLevelLoader).GetField("OnFinishLoadingLevel", BindingFlags.Static | BindingFlags.Public);
				if (field != null)
				{
					Action<string, GameModeInfo> b = (Action<string, GameModeInfo>)Delegate.CreateDelegate(typeof(Action<string, GameModeInfo>), typeof(UnitBalance).GetMethod("OnFinishLoadingLevel", BindingFlags.Static | BindingFlags.NonPublic));
					Action<string, GameModeInfo> a = (Action<string, GameModeInfo>)field.GetValue(null);
					field.SetValue(null, (Action<string, GameModeInfo>)Delegate.Combine(a, b));
					MelonLogger.Msg("Subscribed to GameLevelLoader.OnFinishLoadingLevel");
				}
				else
				{
					_legacyOverrideMode = true;
					MelonLogger.Warning("GameLevelLoader.OnFinishLoadingLevel field not found — using legacy override mode (revert+apply each round)");
				}
			}
			catch (Exception ex)
			{
				_legacyOverrideMode = true;
				MelonLogger.Warning("Failed to subscribe to OnFinishLoadingLevel: " + ex.Message + " — using legacy override mode");
			}
			MelonLogger.Msg("Unit Balance v7.7.0 initialized. Config: " + _configPath);
			MelonLogger.Msg($"  Enabled: {_enabled} | Damage: {_damageMultipliers.Count} | Health: {_healthMultipliers.Count} | Cost: {_costMultipliers.Count} | BuildTime: {_buildTimeMultipliers.Count} | Range: {_rangeMultipliers.Count} | Speed: {_speedMultipliers.Count} | Reload: {_reloadTimeMultipliers.Count} | MoveSpeed: {_moveSpeedMultipliers.Count} | MinTier: {_minTierOverrides.Count} | TechTime: {_techTierTimes.Count} | TechCost: {_techTierCosts.Count}");
		}

		private void TryApplyHarmonyPatches()
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Expected Obj, but got Unknown
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Expected Obj, but got Unknown
			//IL_0418: Unknown result type (might be due to invalid IL or missing references)
			//IL_0425: Expected Obj, but got Unknown
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Expected Obj, but got Unknown
			//IL_051f: Unknown result type (might be due to invalid IL or missing references)
			//IL_052d: Expected Obj, but got Unknown
			//IL_0541: Unknown result type (might be due to invalid IL or missing references)
			//IL_054f: Expected Obj, but got Unknown
			//IL_0563: Unknown result type (might be due to invalid IL or missing references)
			//IL_0571: Expected Obj, but got Unknown
			//IL_0585: Unknown result type (might be due to invalid IL or missing references)
			//IL_0593: Expected Obj, but got Unknown
			//IL_05fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_060a: Expected Obj, but got Unknown
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Expected Obj, but got Unknown
			//IL_0478: Unknown result type (might be due to invalid IL or missing references)
			//IL_0486: Expected Obj, but got Unknown
			//IL_064b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0658: Expected Obj, but got Unknown
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Expected Obj, but got Unknown
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0238: Expected Obj, but got Unknown
			//IL_0238: Expected Obj, but got Unknown
			//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02da: Expected Obj, but got Unknown
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Expected Obj, but got Unknown
			//IL_031a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0328: Expected Obj, but got Unknown
			//IL_0369: Unknown result type (might be due to invalid IL or missing references)
			//IL_0377: Expected Obj, but got Unknown
			//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bb: Expected Obj, but got Unknown
			try
			{
				Type typeFromHandle = typeof(MusicJukeboxHandler);
				Harmony harmonyInstance = ((MelonBase)this).HarmonyInstance;
				MethodInfo methodInfo = AccessTools.Method(typeFromHandle, "OnGameInit", (Type[])null, (Type[])null);
				if (methodInfo != null)
				{
					harmonyInstance.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(typeof(Patch_GameInit), "Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				MethodInfo methodInfo2 = AccessTools.Method(typeFromHandle, "OnGameRestart", (Type[])null, (Type[])null);
				if (methodInfo2 != null)
				{
					harmonyInstance.Patch((MethodBase)methodInfo2, (HarmonyMethod)null, new HarmonyMethod(typeof(Patch_GameRestart), "Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				MethodInfo methodInfo3 = AccessTools.Method(typeFromHandle, "OnGameStarted", (Type[])null, (Type[])null);
				if (methodInfo3 != null)
				{
					harmonyInstance.Patch((MethodBase)methodInfo3, (HarmonyMethod)null, new HarmonyMethod(typeof(Patch_GameStarted), "Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				MethodInfo methodInfo4 = AccessTools.Method(typeFromHandle, "OnGameEnded", (Type[])null, (Type[])null);
				if (methodInfo4 != null)
				{
					harmonyInstance.Patch((MethodBase)methodInfo4, (HarmonyMethod)null, new HarmonyMethod(typeof(Patch_GameEnded), "Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				MethodInfo methodInfo5 = AccessTools.Method(typeof(VehicleDispenser), "RequestVehicle", (Type[])null, (Type[])null);
				if (methodInfo5 != null)
				{
					harmonyInstance.Patch((MethodBase)methodInfo5, new HarmonyMethod(typeof(Patch_VehicleDispenser), "Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					MelonLogger.Msg("Patched VehicleDispenser.RequestVehicle for min_tier enforcement");
				}
				MethodInfo method = typeof(NetworkLayer).GetMethod("SendPlayerOverrides", BindingFlags.Static | BindingFlags.Public);
				if (method != null)
				{
					_sendPlayerOverridesMethod = method;
					harmonyInstance.Patch((MethodBase)method, new HarmonyMethod(typeof(Patch_SendPlayerOverrides), "Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					MelonLogger.Msg("Patched SendPlayerOverrides for chunked override sync");
				}
				else
				{
					MelonLogger.Warning("SendPlayerOverrides method not found — override sync may fail for large configs");
				}
				int num = 0;
				string[] array = new string[2] { "SpawnStructuresAndStartingResources", "SpawnBaseStructures" };
				foreach (string text in array)
				{
					try
					{
						MethodInfo methodInfo6 = AccessTools.Method(typeof(MP_Strategy), text, (Type[])null, (Type[])null);
						if (methodInfo6 == null)
						{
							MelonLogger.Msg("MP_Strategy." + text + " not present on this build");
							continue;
						}
						harmonyInstance.Patch((MethodBase)methodInfo6, new HarmonyMethod(typeof(Patch_BaseSpawn), "Prefix", (Type[])null), new HarmonyMethod(typeof(Patch_BaseSpawn), "Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
						num++;
						MelonLogger.Msg("Bracketed MP_Strategy." + text + " — overrides applied before the base spawn, propagated after");
					}
					catch (Exception ex)
					{
						MelonLogger.Warning("Patching " + text + " threw: " + ex.Message);
					}
				}
				if (num == 0)
				{
					MelonLogger.Warning("No base-spawn method found — starter structures may keep vanilla values");
				}
				MethodInfo method2 = typeof(OverrideManager).GetMethod("RevertAll", BindingFlags.Static | BindingFlags.Public);
				if (method2 != null)
				{
					harmonyInstance.Patch((MethodBase)method2, new HarmonyMethod(typeof(Patch_OMRevertAll), "Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					MelonLogger.Msg("Patched OverrideManager.RevertAll — blocking game revert during scene transitions");
				}
				MethodInfo method3 = typeof(ConstructionSite).GetMethod("SpawnObject", BindingFlags.Instance | BindingFlags.NonPublic);
				if (method3 != null)
				{
					harmonyInstance.Patch((MethodBase)method3, new HarmonyMethod(typeof(Patch_ConstructionSpawn), "Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					MelonLogger.Msg("Patched ConstructionSite.SpawnObject for construction health fix");
				}
				try
				{
					MethodInfo method4 = typeof(HumanStructure).GetMethod("UpdateHealDecay", BindingFlags.Instance | BindingFlags.NonPublic);
					if (method4 != null)
					{
						harmonyInstance.Patch((MethodBase)method4, new HarmonyMethod(typeof(Patch_HumanStructure_UpdateHealDecay), "Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					}
					MethodInfo method5 = typeof(AlienStructure).GetMethod("UpdateHealDecay", BindingFlags.Instance | BindingFlags.NonPublic);
					if (method5 != null)
					{
						harmonyInstance.Patch((MethodBase)method5, new HarmonyMethod(typeof(Patch_AlienStructure_UpdateHealDecay), "Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					}
					MelonLogger.Msg("Patched UpdateHealDecay for keep_production decay forcing");
				}
				catch (Exception ex2)
				{
					MelonLogger.Warning("[keep_production] Failed to apply patches: " + ex2.Message);
				}
				MethodInfo methodInfo7 = AccessTools.Method(typeof(Team), "UpdateUnitCapValues", (Type[])null, (Type[])null);
				if (methodInfo7 != null)
				{
					harmonyInstance.Patch((MethodBase)methodInfo7, (HarmonyMethod)null, new HarmonyMethod(typeof(Patch_UnitCapPlayerInfantry), "Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					MelonLogger.Msg("Patched Team.UpdateUnitCapValues for player-infantry cap exemption");
				}
				try
				{
					MethodInfo methodInfo8 = AccessTools.Method(typeof(GameDatabase), "GetSpawnablePrefabIndex", new Type[1] { typeof(string) }, (Type[])null);
					if (methodInfo8 != null)
					{
						harmonyInstance.Patch((MethodBase)methodInfo8, new HarmonyMethod(typeof(Patch_GetSpawnablePrefabIndex), "Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
						MelonLogger.Msg("Patched GameDatabase.GetSpawnablePrefabIndex(string) to skip null prefab entries");
					}
				}
				catch (Exception ex3)
				{
					MelonLogger.Warning("[PREFAB] Failed to patch GetSpawnablePrefabIndex: " + ex3.Message);
				}
				try
				{
					MethodInfo methodInfo9 = AccessTools.Method(typeof(Structure), "Construct", new Type[4]
					{
						typeof(ConstructionData),
						typeof(Vector3),
						typeof(Quaternion),
						typeof(bool)
					}, (Type[])null);
					if (methodInfo9 != null)
					{
						harmonyInstance.Patch((MethodBase)methodInfo9, new HarmonyMethod(typeof(Patch_CountCap), "Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
						harmonyInstance.Patch((MethodBase)methodInfo9, new HarmonyMethod(typeof(Patch_NestReach), "Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
						harmonyInstance.Patch((MethodBase)methodInfo9, new HarmonyMethod(typeof(Patch_NestFinishedAnchor), "Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
						harmonyInstance.Patch((MethodBase)methodInfo9, new HarmonyMethod(typeof(Patch_QueenLogisticsPerNest), "Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
						MelonLogger.Msg("Patched Structure.Construct for Shrimp/Harvester count caps + alien nest reach + nest finished-anchor + queen logistics per nest");
					}
					else
					{
						MelonLogger.Warning("[CountCap] Structure.Construct not found — count caps / nest reach / nest finished-anchor / queen logistics inactive");
					}
				}
				catch (Exception ex4)
				{
					MelonLogger.Warning("[CountCap] Failed to patch Structure.Construct: " + ex4.Message);
				}
				try
				{
					MethodInfo methodInfo10 = AccessTools.Method(typeof(MP_Strategy), "SetTeamVersusMode", (Type[])null, (Type[])null);
					if (methodInfo10 != null)
					{
						harmonyInstance.Patch((MethodBase)methodInfo10, (HarmonyMethod)null, new HarmonyMethod(typeof(Patch_UnitCapMultVersus), "Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
						MelonLogger.Msg("Patched MP_Strategy.SetTeamVersusMode for per-match-type unit cap multiplier");
					}
					MethodInfo methodInfo11 = AccessTools.Method(typeof(GameModeExt), "UpdateUnitCapMultFromSetting", (Type[])null, (Type[])null);
					if (methodInfo11 != null)
					{
						harmonyInstance.Patch((MethodBase)methodInfo11, (HarmonyMethod)null, new HarmonyMethod(typeof(Patch_UnitCapMultSetting), "Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					}
				}
				catch (Exception ex5)
				{
					MelonLogger.Warning("[UnitCapMult] Failed to apply patches: " + ex5.Message);
				}
				try
				{
					Type type = null;
					Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
					for (int i = 0; i < assemblies.Length; i++)
					{
						type = assemblies[i].GetType("SilicaAdminMod.HelperMethods");
						if (type != null)
						{
							break;
						}
					}
					if (type != null)
					{
						_adminCallbackType = type.GetNestedType("CommandCallback", BindingFlags.Public);
						if (_adminCallbackType == null)
						{
							_adminCallbackType = type.Assembly.GetType("SilicaAdminMod.HelperMethods+CommandCallback");
						}
						_adminPowerType = type.Assembly.GetType("SilicaAdminMod.Power");
						_registerAdminCmdMethod = type.GetMethod("RegisterAdminCommand", BindingFlags.Static | BindingFlags.Public);
						_registerPlayerCmdMethod = type.GetMethod("RegisterPlayerCommand", BindingFlags.Static | BindingFlags.Public);
						_sendChatToPlayerMethod = type.GetMethod("SendChatMessageToPlayer", BindingFlags.Static | BindingFlags.Public);
						_sendConsoleToPlayerMethod = type.GetMethod("SendConsoleMessageToPlayer", BindingFlags.Static | BindingFlags.Public);
						_playSoundFileMethod = type.Assembly.GetType("SilicaAdminMod.AudioHelper")?.GetMethod("PlaySoundFile", BindingFlags.Static | BindingFlags.Public);
						_playerNameField = typeof(Player).GetField("PlayerName", BindingFlags.Instance | BindingFlags.Public);
						_playerIdField = typeof(Player).GetField("PlayerID", BindingFlags.Instance | BindingFlags.Public);
						if (_adminCallbackType != null && _registerAdminCmdMethod != null)
						{
							Enum.ToObject(_adminPowerType, 0);
							object obj = Enum.ToObject(_adminPowerType, 16384);
							_registerAdminCmdMethod.Invoke(null, new object[4]
							{
								"rebalance",
								MakeShim("RebalanceShim", "OnRebalanceCommand"),
								obj,
								"Hot-reload unit balance config"
							});
							MelonLogger.Msg("Registered !rebalance admin command via AdminMod");
							_registerAdminCmdMethod.Invoke(null, new object[4]
							{
								"b",
								MakeShim("BalanceShim", "OnBalanceCommand"),
								obj,
								"Balance editor UI"
							});
							MelonLogger.Msg("Registered !b admin command (balance editor)");
							Delegate obj2 = MakeShim("MenuShortcutShim", "OnMenuShortcut");
							string[] array2 = new string[22]
							{
								"1", "2", "3", "4", "5", "6", "7", "8", "9", "10",
								"11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
								"0", "back"
							};
							if (_registerPlayerCmdMethod != null)
							{
								array = array2;
								foreach (string text2 in array)
								{
									try
									{
										_registerPlayerCmdMethod.Invoke(null, new object[3] { text2, obj2, true });
									}
									catch
									{
									}
								}
								MelonLogger.Msg("Registered /1-/20, /0, /back as player commands (state-gated)");
							}
							else
							{
								MelonLogger.Warning("RegisterPlayerCommand not found — falling back to admin-only shortcuts");
								array = array2;
								foreach (string text3 in array)
								{
									try
									{
										_registerAdminCmdMethod.Invoke(null, new object[4] { text3, obj2, obj, null });
									}
									catch
									{
									}
								}
							}
							if (_registerPlayerCmdMethod != null)
							{
								try
								{
									_registerPlayerCmdMethod.Invoke(null, new object[3]
									{
										"stats",
										MakeShim("StatsShim", "OnStatsCommand"),
										true
									});
									MelonLogger.Msg("Registered /stats as player command");
								}
								catch (Exception ex6)
								{
									MelonLogger.Warning("Failed to register /stats: " + ex6.Message);
								}
							}
						}
						else
						{
							MelonLogger.Warning("Could not find CommandCallback or RegisterAdminCommand in AdminMod");
						}
					}
					else
					{
						MelonLogger.Warning("AdminMod not found — chat commands (!rebalance, !b) disabled");
					}
				}
				catch (Exception ex7)
				{
					MelonLogger.Warning("Failed to register chat commands: " + ex7.Message);
				}
				_harmonyApplied = true;
				MelonLogger.Msg("Harmony patches applied successfully");
			}
			catch (Exception ex8)
			{
				MelonLogger.Warning("Harmony patching failed: " + ex8.Message);
			}
			static Delegate MakeShim(string name, string targetMethod)
			{
				MethodInfo method6 = typeof(UnitBalance).GetMethod(targetMethod, BindingFlags.Static | BindingFlags.NonPublic);
				DynamicMethod dynamicMethod = new DynamicMethod(name, typeof(void), new Type[2]
				{
					typeof(Player),
					typeof(string)
				}, typeof(UnitBalance).Module, skipVisibility: true);
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldarg_1);
				iLGenerator.Emit(OpCodes.Call, method6);
				iLGenerator.Emit(OpCodes.Ret);
				return dynamicMethod.CreateDelegate(_adminCallbackType);
			}
		}

		private static void LoadConfig()
		{
			if (!File.Exists(_configPath))
			{
				MelonLogger.Warning("Config not found, creating default...");
				CreateDefaultConfig();
			}
			try
			{
				JObject jObject = JObject.Parse(File.ReadAllText(_configPath));
				_enabled = jObject["enabled"]?.Value<bool>() ?? true;
				_dumpFields = jObject["dump_fields"]?.Value<bool>() ?? false;
				_shrimpDisableAim = jObject["shrimp_disable_aim"]?.Value<bool>() ?? false;
				_additionalSpawn = jObject["additional_spawn"]?.Value<bool>() ?? false;
				_discordAutoPost = jObject["discord_auto_post"]?.Value<bool>() ?? false;
				_discordWebhookUrl = jObject["discord_webhook_url"]?.Value<string>() ?? "";
				_watchdogEnabled = jObject["watchdog_enabled"]?.Value<bool>() ?? true;
				_playerInfantryIgnoreCap = jObject["player_infantry_ignore_cap"]?.Value<bool>() ?? false;
				_alienNestOnlyBuild = jObject["alien_nest_only_build"]?.Value<bool>() ?? false;
				_alienNestReachRadius = jObject["alien_nest_reach_radius"]?.Value<float>() ?? (-1f);
				if (_alienNestReachRadius < 1f)
				{
					_alienNestReachRadius = -1f;
				}
				if (_alienNestReachRadius > 0f)
				{
					MelonLogger.Msg($"[CONFIG] Alien nest reach radius: {_alienNestReachRadius:F0} m (Nodes / Bio Caches only within reach of a Nest)");
				}
				_alienNestFinishedAnchorOnly = jObject["alien_nest_finished_anchor_only"]?.Value<bool>() ?? false;
				if (_alienNestFinishedAnchorOnly)
				{
					MelonLogger.Msg("[CONFIG] Alien nest finished-anchor only: a new Nest must anchor to a finished structure, not a construction site");
				}
				_alienQueenLogisticsPerNest = jObject["alien_queen_logistics_per_nest"]?.Value<bool>() ?? false;
				if (_alienQueenLogisticsPerNest)
				{
					MelonLogger.Msg("[CONFIG] Alien Queen logistics per Nest: structures build only while their own Nest's Queen is docked");
				}
				_alienQueensMax = jObject["alien_queens_max"]?.Value<int>() ?? (-1);
				if (_alienQueensMax < -1)
				{
					_alienQueensMax = -1;
				}
				if (_alienQueensMax >= 0)
				{
					MelonLogger.Msg("[CONFIG] Alien queens max: " + FmtQueensMax(_alienQueensMax));
				}
				_healthMultEnabled = jObject["health_mult_enabled"]?.Value<bool>() ?? true;
				string text = jObject["game_version"]?.Value<string>() ?? "";
				if (!string.IsNullOrEmpty(text))
				{
					MelonLogger.Msg("[CONFIG] Config calibrated for game version: " + text);
				}
				try
				{
					MelonPreferences_Category category = MelonPreferences.GetCategory("Silica");
					_debugLogging = ((category != null) ? category.GetEntry<bool>("Admin_EnableDebugLogging") : null)?.Value ?? false;
				}
				catch
				{
					_debugLogging = false;
				}
				MelonLogger.Msg($"[CONFIG] Debug logging: {_debugLogging}");
				_damageMultipliers.Clear();
				_healthMultipliers.Clear();
				_costMultipliers.Clear();
				_buildTimeMultipliers.Clear();
				_minTierOverrides.Clear();
				_unitCapOverrides.Clear();
				_rangeMultipliers.Clear();
				_speedMultipliers.Clear();
				_reloadTimeMultipliers.Clear();
				_moveSpeedMultipliers.Clear();
				_accuracyMultipliers.Clear();
				_magazineMultipliers.Clear();
				_fireRateMultipliers.Clear();
				_buildRadiusOverrides.Clear();
				_depositRadiusOverrides.Clear();
				_extractionRadiusOverrides.Clear();
				_turnRadiusMultipliers.Clear();
				_turboSpeedMultipliers.Clear();
				_targetDistanceOverrides.Clear();
				_jumpSpeedMultipliers.Clear();
				_fowDistanceOverrides.Clear();
				_visibleEventRadiusMultipliers.Clear();
				_lifetimeMultipliers.Clear();
				_strafeSpeedMultipliers.Clear();
				_flySpeedMultipliers.Clear();
				_runSpeedMultipliers.Clear();
				_sprintSpeedMultipliers.Clear();
				_splashRadiusMultipliers.Clear();
				_projectileOverrides.Clear();
				_impactScaleBySpeedOverrides.Clear();
				_dragMultipliers.Clear();
				_aiLeadDragOverrides.Clear();
				_aiLeadVelocityOverrides.Clear();
				_aiLeadGravityOverrides.Clear();
				_aimCompensationOverrides.Clear();
				_pitchMaxUpOverrides.Clear();
				_pitchMaxDownOverrides.Clear();
				_proximityOverrides.Clear();
				_techTierTimes.Clear();
				_techTierCosts.Clear();
				_teleportCooldown = -1f;
				_teleportDuration = -1f;
				_dispenseTimeout = -1f;
				_decayHuman = new DecaySettings();
				_decayAlien = new DecaySettings();
				_unitCapMult2Way = -1;
				_unitCapMult3Way = -1;
				_countCapShrimp = -1;
				_countCapHarvester = -1;
				if (jObject["count_caps"] is JObject jObject2)
				{
					_countCapShrimp = jObject2["shrimp"]?.Value<int>() ?? (-1);
					_countCapHarvester = jObject2["harvester"]?.Value<int>() ?? (-1);
					if (_countCapShrimp < -1)
					{
						_countCapShrimp = -1;
					}
					if (_countCapHarvester < -1)
					{
						_countCapHarvester = -1;
					}
					if (_countCapShrimp >= 0 || _countCapHarvester >= 0)
					{
						MelonLogger.Msg("[CONFIG] Count caps — Shrimp: " + FmtCountCap(_countCapShrimp) + ", Harvester: " + FmtCountCap(_countCapHarvester));
					}
				}
				if (jObject["unit_cap_mult"] is JObject jObject3)
				{
					_unitCapMult2Way = jObject3["two_way"]?.Value<int>() ?? (-1);
					_unitCapMult3Way = jObject3["three_way"]?.Value<int>() ?? (-1);
					if (_unitCapMult2Way < -1)
					{
						_unitCapMult2Way = -1;
					}
					if (_unitCapMult3Way < -1)
					{
						_unitCapMult3Way = -1;
					}
					if (_unitCapMult2Way > 255)
					{
						_unitCapMult2Way = 255;
					}
					if (_unitCapMult3Way > 255)
					{
						_unitCapMult3Way = 255;
					}
					if (_unitCapMult2Way >= 0 || _unitCapMult3Way >= 0)
					{
						MelonLogger.Msg("[CONFIG] Unit cap mult — 2-way: " + FmtCapMult(_unitCapMult2Way) + ", 3-way: " + FmtCapMult(_unitCapMult3Way));
					}
				}
				if (jObject["decay"] is JObject jObject4)
				{
					(string, DecaySettings)[] array = new (string, DecaySettings)[2]
					{
						("human", _decayHuman),
						("alien", _decayAlien)
					};
					for (int i = 0; i < array.Length; i++)
					{
						(string, DecaySettings) tuple = array[i];
						if (jObject4[tuple.Item1] is JObject jObject5)
						{
							DecaySettings item = tuple.Item2;
							item.Enabled = jObject5["enabled"]?.Value<bool>() ?? true;
							item.Delay = jObject5["delay"]?.Value<float>() ?? (-1f);
							item.Tick = jObject5["tick"]?.Value<float>() ?? (-1f);
							item.AmountPct = jObject5["amount_pct"]?.Value<float>() ?? (-1f);
							item.RandomizePct = jObject5["randomize_pct"]?.Value<float>() ?? (-1f);
							item.KeepProduction = jObject5["keep_production"]?.Value<bool>() ?? false;
						}
					}
					if (!_decayHuman.IsVanilla || !_decayAlien.IsVanilla)
					{
						MelonLogger.Msg($"[CONFIG] Decay — Human: enabled={_decayHuman.Enabled} custom={_decayHuman.HasCustomParams}, Alien: enabled={_decayAlien.Enabled} custom={_decayAlien.HasCustomParams}");
					}
				}
				if (jObject["units"] is JObject jObject6)
				{
					if (jObject6["_teleport"] is JObject jObject7)
					{
						_teleportCooldown = jObject7["cooldown"]?.Value<float>() ?? (-1f);
						_teleportDuration = jObject7["duration"]?.Value<float>() ?? (-1f);
					}
					foreach (KeyValuePair<string, JToken> item2 in jObject6)
					{
						string key = item2.Key;
						if (key.StartsWith("_") || !(item2.Value is JObject jObject8))
						{
							continue;
						}
						float num = jObject8["damage_mult"]?.Value<float>() ?? 1f;
						float num2 = jObject8["health_mult"]?.Value<float>() ?? 1f;
						float num3 = jObject8["cost_mult"]?.Value<float>() ?? 1f;
						float num4 = jObject8["build_time_mult"]?.Value<float>() ?? 1f;
						float num5 = jObject8["range_mult"]?.Value<float>() ?? 1f;
						float num6 = jObject8["proj_speed_mult"]?.Value<float>() ?? 1f;
						float num7 = jObject8["reload_time_mult"]?.Value<float>() ?? 1f;
						float num8 = jObject8["move_speed_mult"]?.Value<float>() ?? 1f;
						float num9 = jObject8["accuracy_mult"]?.Value<float>() ?? 1f;
						float num10 = jObject8["magazine_mult"]?.Value<float>() ?? 1f;
						float num11 = jObject8["fire_rate_mult"]?.Value<float>() ?? 1f;
						float num12 = jObject8["turn_radius_mult"]?.Value<float>() ?? 1f;
						float num13 = jObject8["turbo_speed_mult"]?.Value<float>() ?? 1f;
						float num14 = jObject8["target_distance"]?.Value<float>() ?? (-1f);
						float num15 = jObject8["build_radius"]?.Value<float>() ?? (-1f);
						float num16 = jObject8["deposit_radius"]?.Value<float>() ?? (-1f);
						float num17 = jObject8["extraction_radius"]?.Value<float>() ?? (-1f);
						int num18 = jObject8["min_tier"]?.Value<int>() ?? (-1);
						float num19 = jObject8["jump_speed_mult"]?.Value<float>() ?? 1f;
						float num20 = jObject8["fow_distance"]?.Value<float>() ?? (-1f);
						float num21 = jObject8["visible_event_radius_mult"]?.Value<float>() ?? 1f;
						float num22 = jObject8["proj_lifetime_mult"]?.Value<float>() ?? 1f;
						bool? flag = jObject8["impact_scale_by_speed"]?.Value<bool?>();
						float num23 = jObject8["drag_mult"]?.Value<float>() ?? 1f;
						float num24 = jObject8["ai_lead_drag"]?.Value<float>() ?? 1f;
						float num25 = jObject8["ai_lead_velocity"]?.Value<float>() ?? 1f;
						float num26 = jObject8["ai_lead_gravity"]?.Value<float>() ?? 1f;
						int num27 = jObject8["aim_compensation"]?.Value<int>() ?? (-1);
						if (num27 > 4)
						{
							MelonLogger.Warning($"[UnitBalance] {key}: aim_compensation {num27} out of range (valid 0-4) — clamping to 4");
							num27 = 4;
						}
						float num28 = jObject8["pitch_max_up"]?.Value<float>() ?? (-1f);
						float num29 = jObject8["pitch_max_down"]?.Value<float>() ?? (-1f);
						float num30 = jObject8["strafe_speed_mult"]?.Value<float>() ?? 1f;
						float num31 = jObject8["fly_speed_mult"]?.Value<float>() ?? 1f;
						float num32 = jObject8["run_speed_mult"]?.Value<float>() ?? 1f;
						float num33 = jObject8["sprint_speed_mult"]?.Value<float>() ?? 1f;
						float num34 = jObject8["dispense_timeout"]?.Value<float>() ?? (-1f);
						int num35 = jObject8["unit_cap_value"]?.Value<int>() ?? (-1);
						float num36 = jObject8["impact_damage_mult"]?.Value<float>() ?? 1f;
						float num37 = jObject8["splash_damage_mult"]?.Value<float>() ?? 1f;
						float num38 = jObject8["penetrating_damage_mult"]?.Value<float>() ?? 1f;
						float num39 = jObject8["ricochet_damage_mult"]?.Value<float>() ?? 1f;
						float num40 = jObject8["pri_impact_damage_mult"]?.Value<float>() ?? 1f;
						float num41 = jObject8["pri_splash_damage_mult"]?.Value<float>() ?? 1f;
						float num42 = jObject8["pri_penetrating_damage_mult"]?.Value<float>() ?? 1f;
						float num43 = jObject8["pri_ricochet_damage_mult"]?.Value<float>() ?? 1f;
						float num44 = jObject8["sec_impact_damage_mult"]?.Value<float>() ?? 1f;
						float num45 = jObject8["sec_splash_damage_mult"]?.Value<float>() ?? 1f;
						float num46 = jObject8["sec_penetrating_damage_mult"]?.Value<float>() ?? 1f;
						float num47 = jObject8["sec_ricochet_damage_mult"]?.Value<float>() ?? 1f;
						float num48 = jObject8["splash_radius_max_mult"]?.Value<float>() ?? 1f;
						float num49 = jObject8["splash_radius_min_mult"]?.Value<float>() ?? 1f;
						float num50 = jObject8["splash_radius_pow_mult"]?.Value<float>() ?? 1f;
						float num51 = jObject8["pri_splash_radius_max_mult"]?.Value<float>() ?? 1f;
						float num52 = jObject8["pri_splash_radius_min_mult"]?.Value<float>() ?? 1f;
						float num53 = jObject8["pri_splash_radius_pow_mult"]?.Value<float>() ?? 1f;
						float num54 = jObject8["sec_splash_radius_max_mult"]?.Value<float>() ?? 1f;
						float num55 = jObject8["sec_splash_radius_min_mult"]?.Value<float>() ?? 1f;
						float num56 = jObject8["sec_splash_radius_pow_mult"]?.Value<float>() ?? 1f;
						float num57 = jObject8["pri_damage_mult"]?.Value<float>() ?? 1f;
						float num58 = jObject8["sec_damage_mult"]?.Value<float>() ?? 1f;
						float num59 = jObject8["pri_range_mult"]?.Value<float>() ?? 1f;
						float num60 = jObject8["sec_range_mult"]?.Value<float>() ?? 1f;
						float num61 = jObject8["pri_proj_speed_mult"]?.Value<float>() ?? 1f;
						float num62 = jObject8["sec_proj_speed_mult"]?.Value<float>() ?? 1f;
						float num63 = jObject8["pri_proj_lifetime_mult"]?.Value<float>() ?? 1f;
						float num64 = jObject8["sec_proj_lifetime_mult"]?.Value<float>() ?? 1f;
						float num65 = jObject8["pri_accuracy_mult"]?.Value<float>() ?? 1f;
						float num66 = jObject8["sec_accuracy_mult"]?.Value<float>() ?? 1f;
						float num67 = jObject8["pri_magazine_mult"]?.Value<float>() ?? 1f;
						float num68 = jObject8["sec_magazine_mult"]?.Value<float>() ?? 1f;
						float num69 = jObject8["pri_fire_rate_mult"]?.Value<float>() ?? 1f;
						float num70 = jObject8["sec_fire_rate_mult"]?.Value<float>() ?? 1f;
						float num71 = jObject8["pri_reload_time_mult"]?.Value<float>() ?? 1f;
						float num72 = jObject8["sec_reload_time_mult"]?.Value<float>() ?? 1f;
						if (Math.Abs(num - 1f) > 0.001f)
						{
							_damageMultipliers[key] = num;
						}
						if (Math.Abs(num2 - 1f) > 0.001f)
						{
							_healthMultipliers[key] = num2;
						}
						if (Math.Abs(num3 - 1f) > 0.001f)
						{
							_costMultipliers[key] = num3;
						}
						if (Math.Abs(num4 - 1f) > 0.001f)
						{
							_buildTimeMultipliers[key] = num4;
						}
						if (Math.Abs(num5 - 1f) > 0.001f)
						{
							_rangeMultipliers[key] = num5;
						}
						if (Math.Abs(num6 - 1f) > 0.001f)
						{
							_speedMultipliers[key] = num6;
						}
						if (Math.Abs(num7 - 1f) > 0.001f)
						{
							_reloadTimeMultipliers[key] = num7;
						}
						if (Math.Abs(num8 - 1f) > 0.001f)
						{
							_moveSpeedMultipliers[key] = num8;
						}
						if (Math.Abs(num9 - 1f) > 0.001f)
						{
							_accuracyMultipliers[key] = num9;
						}
						if (Math.Abs(num10 - 1f) > 0.001f)
						{
							_magazineMultipliers[key] = num10;
						}
						if (Math.Abs(num11 - 1f) > 0.001f)
						{
							_fireRateMultipliers[key] = num11;
						}
						if (Math.Abs(num12 - 1f) > 0.001f)
						{
							_turnRadiusMultipliers[key] = num12;
						}
						if (Math.Abs(num13 - 1f) > 0.001f)
						{
							_turboSpeedMultipliers[key] = num13;
						}
						if (num14 >= 0f)
						{
							_targetDistanceOverrides[key] = num14;
						}
						if (num15 >= 0f)
						{
							_buildRadiusOverrides[key] = num15;
						}
						if (num16 >= 0f)
						{
							_depositRadiusOverrides[key] = num16;
						}
						if (num17 >= 0f)
						{
							_extractionRadiusOverrides[key] = num17;
						}
						if (num18 >= 0)
						{
							_minTierOverrides[key] = num18;
						}
						if (Math.Abs(num19 - 1f) > 0.001f)
						{
							_jumpSpeedMultipliers[key] = num19;
						}
						if (num20 >= 0f)
						{
							_fowDistanceOverrides[key] = num20;
						}
						if (Math.Abs(num21 - 1f) > 0.001f)
						{
							_visibleEventRadiusMultipliers[key] = num21;
						}
						if (Math.Abs(num22 - 1f) > 0.001f)
						{
							_lifetimeMultipliers[key] = num22;
						}
						if (flag.HasValue)
						{
							_impactScaleBySpeedOverrides[key] = flag.Value;
						}
						if (Math.Abs(num23 - 1f) > 0.001f)
						{
							_dragMultipliers[key] = num23;
						}
						if (Math.Abs(num24 - 1f) > 0.001f)
						{
							_aiLeadDragOverrides[key] = num24;
						}
						if (Math.Abs(num25 - 1f) > 0.001f)
						{
							_aiLeadVelocityOverrides[key] = num25;
						}
						if (Math.Abs(num26 - 1f) > 0.001f)
						{
							_aiLeadGravityOverrides[key] = num26;
						}
						if (num27 >= 0)
						{
							_aimCompensationOverrides[key] = num27;
						}
						if (num28 >= 0f)
						{
							_pitchMaxUpOverrides[key] = num28;
						}
						if (num29 >= 0f)
						{
							_pitchMaxDownOverrides[key] = num29;
						}
						if (Math.Abs(num30 - 1f) > 0.001f)
						{
							_strafeSpeedMultipliers[key] = num30;
						}
						if (Math.Abs(num31 - 1f) > 0.001f)
						{
							_flySpeedMultipliers[key] = num31;
						}
						if (Math.Abs(num32 - 1f) > 0.001f)
						{
							_runSpeedMultipliers[key] = num32;
						}
						if (Math.Abs(num33 - 1f) > 0.001f)
						{
							_sprintSpeedMultipliers[key] = num33;
						}
						if (num34 >= 0f)
						{
							_dispenseTimeout = num34;
						}
						if (num35 >= 0)
						{
							_unitCapOverrides[key] = num35;
						}
						if (Math.Abs(num57 - 1f) > 0.001f)
						{
							_damageMultipliers["pri:" + key] = num57;
						}
						if (Math.Abs(num58 - 1f) > 0.001f)
						{
							_damageMultipliers["sec:" + key] = num58;
						}
						if (Math.Abs(num36 - 1f) > 0.001f)
						{
							_damageMultipliers["impact:" + key] = num36;
						}
						if (Math.Abs(num37 - 1f) > 0.001f)
						{
							_damageMultipliers["splash:" + key] = num37;
						}
						if (Math.Abs(num38 - 1f) > 0.001f)
						{
							_damageMultipliers["penetrating:" + key] = num38;
						}
						if (Math.Abs(num39 - 1f) > 0.001f)
						{
							_damageMultipliers["ricochet:" + key] = num39;
						}
						if (Math.Abs(num40 - 1f) > 0.001f)
						{
							_damageMultipliers["pri:impact:" + key] = num40;
						}
						if (Math.Abs(num41 - 1f) > 0.001f)
						{
							_damageMultipliers["pri:splash:" + key] = num41;
						}
						if (Math.Abs(num42 - 1f) > 0.001f)
						{
							_damageMultipliers["pri:penetrating:" + key] = num42;
						}
						if (Math.Abs(num43 - 1f) > 0.001f)
						{
							_damageMultipliers["pri:ricochet:" + key] = num43;
						}
						if (Math.Abs(num44 - 1f) > 0.001f)
						{
							_damageMultipliers["sec:impact:" + key] = num44;
						}
						if (Math.Abs(num45 - 1f) > 0.001f)
						{
							_damageMultipliers["sec:splash:" + key] = num45;
						}
						if (Math.Abs(num46 - 1f) > 0.001f)
						{
							_damageMultipliers["sec:penetrating:" + key] = num46;
						}
						if (Math.Abs(num47 - 1f) > 0.001f)
						{
							_damageMultipliers["sec:ricochet:" + key] = num47;
						}
						if (Math.Abs(num48 - 1f) > 0.001f)
						{
							_splashRadiusMultipliers["max:" + key] = num48;
						}
						if (Math.Abs(num49 - 1f) > 0.001f)
						{
							_splashRadiusMultipliers["min:" + key] = num49;
						}
						if (Math.Abs(num50 - 1f) > 0.001f)
						{
							_splashRadiusMultipliers["pow:" + key] = num50;
						}
						if (Math.Abs(num51 - 1f) > 0.001f)
						{
							_splashRadiusMultipliers["pri:max:" + key] = num51;
						}
						if (Math.Abs(num52 - 1f) > 0.001f)
						{
							_splashRadiusMultipliers["pri:min:" + key] = num52;
						}
						if (Math.Abs(num53 - 1f) > 0.001f)
						{
							_splashRadiusMultipliers["pri:pow:" + key] = num53;
						}
						if (Math.Abs(num54 - 1f) > 0.001f)
						{
							_splashRadiusMultipliers["sec:max:" + key] = num54;
						}
						if (Math.Abs(num55 - 1f) > 0.001f)
						{
							_splashRadiusMultipliers["sec:min:" + key] = num55;
						}
						if (Math.Abs(num56 - 1f) > 0.001f)
						{
							_splashRadiusMultipliers["sec:pow:" + key] = num56;
						}
						if (Math.Abs(num59 - 1f) > 0.001f)
						{
							_rangeMultipliers["pri:" + key] = num59;
						}
						if (Math.Abs(num60 - 1f) > 0.001f)
						{
							_rangeMultipliers["sec:" + key] = num60;
						}
						if (Math.Abs(num61 - 1f) > 0.001f)
						{
							_speedMultipliers["pri:" + key] = num61;
						}
						if (Math.Abs(num62 - 1f) > 0.001f)
						{
							_speedMultipliers["sec:" + key] = num62;
						}
						if (Math.Abs(num63 - 1f) > 0.001f)
						{
							_lifetimeMultipliers["pri:" + key] = num63;
						}
						if (Math.Abs(num64 - 1f) > 0.001f)
						{
							_lifetimeMultipliers["sec:" + key] = num64;
						}
						if (Math.Abs(num65 - 1f) > 0.001f)
						{
							_accuracyMultipliers["pri:" + key] = num65;
						}
						if (Math.Abs(num66 - 1f) > 0.001f)
						{
							_accuracyMultipliers["sec:" + key] = num66;
						}
						if (Math.Abs(num67 - 1f) > 0.001f)
						{
							_magazineMultipliers["pri:" + key] = num67;
						}
						if (Math.Abs(num68 - 1f) > 0.001f)
						{
							_magazineMultipliers["sec:" + key] = num68;
						}
						if (Math.Abs(num69 - 1f) > 0.001f)
						{
							_fireRateMultipliers["pri:" + key] = num69;
						}
						if (Math.Abs(num70 - 1f) > 0.001f)
						{
							_fireRateMultipliers["sec:" + key] = num70;
						}
						if (Math.Abs(num71 - 1f) > 0.001f)
						{
							_reloadTimeMultipliers["pri:" + key] = num71;
						}
						if (Math.Abs(num72 - 1f) > 0.001f)
						{
							_reloadTimeMultipliers["sec:" + key] = num72;
						}
						if (!(jObject8["projectiles"] is JObject jObject9))
						{
							continue;
						}
						Dictionary<string, Dictionary<string, float>> dictionary = new Dictionary<string, Dictionary<string, float>>(StringComparer.OrdinalIgnoreCase);
						foreach (KeyValuePair<string, JToken> item3 in jObject9)
						{
							string key2 = item3.Key;
							if (!(item3.Value is JObject jObject10))
							{
								continue;
							}
							Dictionary<string, float> dictionary2 = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
							foreach (KeyValuePair<string, JToken> item4 in jObject10)
							{
								if (string.Equals(item4.Key, "proximity", StringComparison.OrdinalIgnoreCase))
								{
									if (!(item4.Value is JObject jObject11))
									{
										continue;
									}
									Dictionary<string, float> dictionary3 = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
									foreach (KeyValuePair<string, JToken> item5 in jObject11)
									{
										if (item5.Value == null)
										{
											continue;
										}
										if (item5.Value.Type == JTokenType.Boolean)
										{
											dictionary3[item5.Key] = (item5.Value.Value<bool>() ? 1f : 0f);
											continue;
										}
										float? num73 = item5.Value.Value<float>();
										if (num73.HasValue)
										{
											dictionary3[item5.Key] = num73.Value;
										}
									}
									if (dictionary3.Count > 0)
									{
										_proximityOverrides[key2] = dictionary3;
									}
								}
								else
								{
									float? num74 = item4.Value?.Value<float>();
									if (num74.HasValue)
									{
										dictionary2[item4.Key] = num74.Value;
									}
								}
							}
							if (dictionary2.Count > 0)
							{
								dictionary[key2] = dictionary2;
							}
						}
						if (dictionary.Count > 0)
						{
							_projectileOverrides[key] = dictionary;
						}
					}
				}
				if (jObject["tech_time"] is JObject jObject12)
				{
					for (int j = 1; j <= 8; j++)
					{
						float? num75 = jObject12[$"tier_{j}"]?.Value<float>();
						if (num75.HasValue)
						{
							_techTierTimes[j] = num75.Value;
						}
					}
				}
				if (jObject["tech_cost"] is JObject jObject13)
				{
					for (int k = 1; k <= 8; k++)
					{
						int? num76 = jObject13[$"tier_{k}"]?.Value<int>();
						if (num76.HasValue && num76.Value >= 0)
						{
							_techTierCosts[k] = num76.Value;
						}
					}
				}
				_configLoaded = true;
			}
			catch (Exception ex)
			{
				MelonLogger.Error("Failed to load config: " + ex.Message);
				_enabled = false;
			}
		}

		private static void CreateDefaultConfig()
		{
			try
			{
				string text = Path.Combine(Path.GetDirectoryName(_configPath), "Si_UnitBalance_Config_Default.json");
				if (!File.Exists(text))
				{
					string text2 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Si_UnitBalance_Config_Default.json");
					if (File.Exists(text2))
					{
						text = text2;
					}
				}
				if (File.Exists(text))
				{
					File.Copy(text, _configPath);
					MelonLogger.Msg("Created config from default template: " + text);
				}
				else
				{
					string contents = "{\n    \"enabled\": true,\n    \"dump_fields\": false,\n    \"shrimp_disable_aim\": false,\n    \"alien_nest_only_build\": false,\n    \"alien_nest_reach_radius\": -1,\n    \"alien_nest_finished_anchor_only\": false,\n    \"alien_queen_logistics_per_nest\": false,\n    \"alien_queens_max\": -1,\n    \"description\": \"Vanilla base config. All multipliers at 1.00 = no change. See Si_UnitBalance_Config_Default.json for comprehensive template with all units.\",\n    \"tech_time\": { \"tier_1\": 30, \"tier_2\": 30, \"tier_3\": 30, \"tier_4\": 30, \"tier_5\": 30, \"tier_6\": 30, \"tier_7\": 30, \"tier_8\": 30 },\n    \"tech_cost\": { \"tier_1\": -1, \"tier_2\": -1, \"tier_3\": -1, \"tier_4\": -1, \"tier_5\": -1, \"tier_6\": -1, \"tier_7\": -1, \"tier_8\": -1 },\n    \"decay\": { \"human\": { \"enabled\": true, \"keep_production\": false }, \"alien\": { \"enabled\": true, \"keep_production\": false } },\n    \"unit_cap_mult\": { \"two_way\": -1, \"three_way\": -1 },\n    \"count_caps\": { \"shrimp\": -1, \"harvester\": -1 },\n    \"units\": {}\n}";
					File.WriteAllText(_configPath, contents);
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Error("Failed to create default config: " + ex.Message);
			}
		}

		private static void PostBalanceChangesToDiscord()
		{
			if (string.IsNullOrEmpty(_discordWebhookUrl))
			{
				MelonLogger.Warning("[Discord] No webhook URL configured. Set 'discord_webhook_url' in config.");
				return;
			}
			try
			{
				LoadMessageIds();
				string serverName = GetServerName();
				List<BalanceChange> list = BuildBalanceChangeSummary();
				if (list.Count == 0)
				{
					MelonLogger.Msg("[Discord] No balance changes vs vanilla.");
					if (_discordMessageIds.Count > 0)
					{
						DeleteOldMessages(_discordMessageIds);
						_discordMessageIds.Clear();
						SaveMessageIds();
						MelonLogger.Msg("[Discord] Cleaned up old messages (balance reverted to vanilla).");
					}
				}
				else
				{
					PostEmbedsToDiscord(FormatDiscordEmbeds(list, serverName));
					SavePushSnapshot();
					MelonLogger.Msg($"[Discord] Posted {list.Count} balance changes to Discord.");
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Error("[Discord] Failed to post: " + ex.Message);
			}
		}

		private static string GetServerName()
		{
			try
			{
				Type type = null;
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (int i = 0; i < assemblies.Length; i++)
				{
					type = assemblies[i].GetType("NetworkGameServer");
					if (type != null)
					{
						break;
					}
				}
				if (type != null)
				{
					MethodInfo method = type.GetMethod("GetServerName", BindingFlags.Static | BindingFlags.Public);
					if (method != null && method.Invoke(null, null) is string text && !string.IsNullOrEmpty(text))
					{
						return text;
					}
				}
			}
			catch
			{
			}
			return "Silica Server";
		}

		private static List<BalanceChange> BuildBalanceChangeSummary()
		{
			string directoryName = Path.GetDirectoryName(_configPath);
			string path = Path.Combine(directoryName, "Si_UnitBalance_Config_Default.json");
			string path2 = Path.Combine(directoryName, "Si_UnitBalance_LastPush.json");
			if (!File.Exists(path) || !File.Exists(_configPath))
			{
				return new List<BalanceChange>();
			}
			JObject jObject = JObject.Parse(File.ReadAllText(path));
			JObject jObject2 = JObject.Parse(File.ReadAllText(_configPath));
			JObject jObject3 = null;
			if (File.Exists(path2))
			{
				jObject3 = JObject.Parse(File.ReadAllText(path2));
			}
			List<BalanceChange> list = new List<BalanceChange>();
			CompareGlobalSettings(jObject, jObject2, jObject3, list);
			JObject jObject4 = jObject["units"] as JObject;
			JObject jObject5 = jObject2["units"] as JObject;
			JObject lastPushUnits = jObject3?["units"] as JObject;
			if (jObject4 == null || jObject5 == null)
			{
				return list;
			}
			Dictionary<string, string> dictionary = BuildUnitSectionMap(jObject4);
			ComparePseudoUnit("_teleport", "Global Settings", jObject4, jObject5, lastPushUnits, list);
			foreach (KeyValuePair<string, JToken> item in jObject5)
			{
				string key = item.Key;
				if (key.StartsWith("_"))
				{
					continue;
				}
				JObject jObject6 = item.Value as JObject;
				JObject jObject7 = jObject4[key] as JObject;
				if (jObject6 == null)
				{
					continue;
				}
				string section = (dictionary.ContainsKey(key) ? dictionary[key] : "Other");
				string baseInfo = jObject7?["_base"]?.Value<string>();
				if (jObject7 == null)
				{
					foreach (KeyValuePair<string, JToken> item2 in jObject6)
					{
						if (!item2.Key.StartsWith("_") && !_skipParams.Contains(item2.Key))
						{
							bool isNew = IsParamNew(key, item2.Key, item2.Value, lastPushUnits);
							list.Add(new BalanceChange(section, key, item2.Key, "-", item2.Value.ToString(), isNew, baseInfo));
						}
					}
					continue;
				}
				foreach (KeyValuePair<string, JToken> item3 in jObject6)
				{
					if (!item3.Key.StartsWith("_") && !_skipParams.Contains(item3.Key))
					{
						JToken jToken = jObject7[item3.Key];
						if (jToken != null && !TokensEqual(item3.Value, jToken))
						{
							bool isNew2 = IsParamNew(key, item3.Key, item3.Value, lastPushUnits);
							list.Add(new BalanceChange(section, key, item3.Key, FormatValue(jToken), FormatValue(item3.Value), isNew2, baseInfo));
						}
					}
				}
			}
			return list;
		}

		private static void CompareGlobalSettings(JObject defaultCfg, JObject activeCfg, JObject lastPushCfg, List<BalanceChange> changes)
		{
			JObject jObject = defaultCfg["tech_time"] as JObject;
			JObject jObject2 = activeCfg["tech_time"] as JObject;
			JObject jObject3 = lastPushCfg?["tech_time"] as JObject;
			if (jObject != null && jObject2 != null)
			{
				for (int i = 1; i <= 8; i++)
				{
					string text = $"tier_{i}";
					JToken jToken = jObject[text];
					JToken jToken2 = jObject2[text];
					if (jToken != null && jToken2 != null && !TokensEqual(jToken, jToken2))
					{
						bool isNew = jObject3 == null || !TokensEqual(jToken2, jObject3[text]);
						changes.Add(new BalanceChange("Global Settings", "Tech Time", text, FormatValue(jToken), FormatValue(jToken2), isNew));
					}
				}
			}
			JObject jObject4 = defaultCfg["tech_cost"] as JObject;
			JObject jObject5 = activeCfg["tech_cost"] as JObject;
			JObject jObject6 = lastPushCfg?["tech_cost"] as JObject;
			if (jObject4 != null && jObject5 != null)
			{
				for (int j = 1; j <= 8; j++)
				{
					string text2 = $"tier_{j}";
					JToken jToken3 = jObject4[text2];
					JToken jToken4 = jObject5[text2];
					if (jToken3 != null && jToken4 != null && !TokensEqual(jToken3, jToken4))
					{
						bool isNew2 = jObject6 == null || !TokensEqual(jToken4, jObject6[text2]);
						changes.Add(new BalanceChange("Global Settings", "Tech Cost", text2, FormatValue(jToken3), FormatValue(jToken4), isNew2));
					}
				}
			}
			if (activeCfg["unit_cap_mult"] is JObject jObject7)
			{
				JObject jObject8 = defaultCfg["unit_cap_mult"] as JObject;
				JObject jObject9 = lastPushCfg?["unit_cap_mult"] as JObject;
				string[] array = new string[2] { "two_way", "three_way" };
				foreach (string text3 in array)
				{
					int num = jObject7[text3]?.Value<int>() ?? (-1);
					int num2 = jObject8?[text3]?.Value<int>() ?? (-1);
					if (num != num2)
					{
						int num3 = jObject9?[text3]?.Value<int>() ?? num2;
						changes.Add(new BalanceChange("Global Settings", "Unit Cap Mult", text3, FmtCapMult(num2), FmtCapMult(num), num != num3));
					}
				}
			}
			if (activeCfg["count_caps"] is JObject jObject10)
			{
				JObject jObject11 = defaultCfg["count_caps"] as JObject;
				JObject jObject12 = lastPushCfg?["count_caps"] as JObject;
				string[] array = new string[2] { "shrimp", "harvester" };
				foreach (string text4 in array)
				{
					int num4 = jObject10[text4]?.Value<int>() ?? (-1);
					int num5 = jObject11?[text4]?.Value<int>() ?? (-1);
					if (num4 != num5)
					{
						int num6 = jObject12?[text4]?.Value<int>() ?? num5;
						changes.Add(new BalanceChange("Global Settings", "Count Cap", text4, FmtCountCap(num5), FmtCountCap(num4), num4 != num6));
					}
				}
			}
			float num7 = activeCfg["alien_nest_reach_radius"]?.Value<float>() ?? (-1f);
			float num8 = defaultCfg["alien_nest_reach_radius"]?.Value<float>() ?? (-1f);
			if (num7 < 1f)
			{
				num7 = -1f;
			}
			if (num8 < 1f)
			{
				num8 = -1f;
			}
			if (Math.Abs(num7 - num8) > 0.01f)
			{
				float num9 = lastPushCfg?["alien_nest_reach_radius"]?.Value<float>() ?? num8;
				if (num9 < 1f)
				{
					num9 = -1f;
				}
				changes.Add(new BalanceChange("Global Settings", "Alien Nest Reach Radius", "alien_nest_reach_radius", FmtNestReach(num8), FmtNestReach(num7), Math.Abs(num7 - num9) > 0.01f));
			}
			int num10 = activeCfg["alien_queens_max"]?.Value<int>() ?? (-1);
			int num11 = defaultCfg["alien_queens_max"]?.Value<int>() ?? (-1);
			if (num10 != num11)
			{
				int num12 = lastPushCfg?["alien_queens_max"]?.Value<int>() ?? num11;
				changes.Add(new BalanceChange("Global Settings", "Alien Queens", "alien_queens_max", FmtQueensMax(num11), FmtQueensMax(num10), num10 != num12));
			}
			CompareBoolSetting("additional_spawn", "Additional Spawn", defaultCfg, activeCfg, lastPushCfg, changes);
			CompareBoolSetting("shrimp_disable_aim", "Shrimp Disable Aim", defaultCfg, activeCfg, lastPushCfg, changes);
			CompareBoolSetting("alien_nest_only_build", "Alien Nest-Only Build", defaultCfg, activeCfg, lastPushCfg, changes);
			CompareBoolSetting("alien_nest_finished_anchor_only", "Alien Nest Finished-Anchor Only", defaultCfg, activeCfg, lastPushCfg, changes);
			CompareBoolSetting("alien_queen_logistics_per_nest", "Queen Logistics per Nest", defaultCfg, activeCfg, lastPushCfg, changes);
			CompareBoolSetting("health_mult_enabled", "Health Mult (server-only)", defaultCfg, activeCfg, lastPushCfg, changes);
		}

		private static void CompareBoolSetting(string key, string displayName, JObject defaultCfg, JObject activeCfg, JObject lastPushCfg, List<BalanceChange> changes)
		{
			bool flag = defaultCfg[key]?.Value<bool>() ?? false;
			bool flag2 = activeCfg[key]?.Value<bool>() ?? false;
			if (flag != flag2)
			{
				bool flag3 = lastPushCfg?[key]?.Value<bool>() ?? flag;
				bool isNew = flag2 != flag3;
				changes.Add(new BalanceChange("Global Settings", displayName, "", flag.ToString(), flag2.ToString(), isNew));
			}
		}

		private static void ComparePseudoUnit(string pseudoName, string section, JObject defaultUnits, JObject activeUnits, JObject lastPushUnits, List<BalanceChange> changes)
		{
			JObject jObject = activeUnits?[pseudoName] as JObject;
			JObject jObject2 = defaultUnits?[pseudoName] as JObject;
			if (jObject == null)
			{
				return;
			}
			foreach (KeyValuePair<string, JToken> item in jObject)
			{
				if (!item.Key.StartsWith("_"))
				{
					JToken jToken = jObject2?[item.Key];
					string text = ((jToken != null) ? FormatValue(jToken) : "-");
					string text2 = FormatValue(item.Value);
					if (text != text2)
					{
						bool isNew = IsParamNew(pseudoName, item.Key, item.Value, lastPushUnits);
						string text3 = pseudoName.TrimStart('_');
						text3 = char.ToUpper(text3[0]) + text3.Substring(1);
						changes.Add(new BalanceChange(section, text3, item.Key, text, text2, isNew));
					}
				}
			}
		}

		private static Dictionary<string, string> BuildUnitSectionMap(JObject defaultUnits)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			string value = "Other";
			foreach (KeyValuePair<string, JToken> defaultUnit in defaultUnits)
			{
				if (defaultUnit.Key.StartsWith("_comment"))
				{
					(string, string)[] discordSections = _discordSections;
					for (int i = 0; i < discordSections.Length; i++)
					{
						(string, string) tuple = discordSections[i];
						if (tuple.Item1 == defaultUnit.Key)
						{
							value = tuple.Item2;
							break;
						}
					}
				}
				else if (!defaultUnit.Key.StartsWith("_"))
				{
					dictionary[defaultUnit.Key] = value;
				}
			}
			return dictionary;
		}

		private static bool IsParamNew(string unitName, string paramName, JToken activeVal, JObject lastPushUnits)
		{
			if (lastPushUnits == null)
			{
				return true;
			}
			if (!(lastPushUnits[unitName] is JObject jObject))
			{
				return true;
			}
			JToken jToken = jObject[paramName];
			if (jToken == null)
			{
				return true;
			}
			return !TokensEqual(activeVal, jToken);
		}

		private static bool TokensEqual(JToken a, JToken b)
		{
			if (a == null && b == null)
			{
				return true;
			}
			if (a == null || b == null)
			{
				return false;
			}
			if ((a.Type == JTokenType.Integer || a.Type == JTokenType.Float) && (b.Type == JTokenType.Integer || b.Type == JTokenType.Float))
			{
				return Math.Abs(a.Value<double>() - b.Value<double>()) < 0.0001;
			}
			return JToken.DeepEquals(a, b);
		}

		private static string FormatValue(JToken token)
		{
			if (token.Type == JTokenType.Float)
			{
				return token.Value<float>().ToString("G");
			}
			return token.ToString();
		}

		private static List<JObject> FormatDiscordEmbeds(List<BalanceChange> changes, string serverName)
		{
			List<JObject> list = new List<JObject>();
			List<string> list2 = new List<string>();
			Dictionary<string, List<BalanceChange>> dictionary = new Dictionary<string, List<BalanceChange>>(StringComparer.OrdinalIgnoreCase);
			foreach (BalanceChange change in changes)
			{
				if (!dictionary.ContainsKey(change.Section))
				{
					list2.Add(change.Section);
					dictionary[change.Section] = new List<BalanceChange>();
				}
				dictionary[change.Section].Add(change);
			}
			List<string> list3 = new List<string>();
			foreach (string item in list2)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("# " + item);
				stringBuilder.AppendLine("```diff");
				string text = null;
				foreach (BalanceChange item2 in dictionary[item])
				{
					if (item2.UnitName != text && !string.IsNullOrEmpty(item2.BaseInfo))
					{
						stringBuilder.AppendLine("  [" + item2.UnitName + "] (" + item2.BaseInfo + ")");
						text = item2.UnitName;
					}
					else if (item2.UnitName != text)
					{
						text = item2.UnitName;
					}
					string text2 = (item2.IsNew ? "+ " : "  ");
					string text3 = (string.IsNullOrEmpty(item2.ParamName) ? "" : ("." + item2.ParamName));
					string value = text2 + item2.UnitName + text3 + ": " + item2.DefaultVal + " > " + item2.ActiveVal;
					stringBuilder.AppendLine(value);
				}
				stringBuilder.AppendLine("```");
				list3.Add(stringBuilder.ToString());
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			bool flag = true;
			foreach (string item3 in list3)
			{
				if (item3.Length > 3900)
				{
					if (stringBuilder2.Length > 0)
					{
						list.Add(CreateEmbed(flag ? (serverName + " -- Balance Changes") : null, stringBuilder2.ToString(), flag));
						stringBuilder2.Clear();
						flag = false;
					}
					string[] array = item3.Split('\n');
					StringBuilder stringBuilder3 = new StringBuilder();
					bool flag2 = false;
					string[] array2 = array;
					foreach (string text4 in array2)
					{
						if (text4.TrimStart().StartsWith("```"))
						{
							flag2 = !flag2;
						}
						if (stringBuilder3.Length + text4.Length + 1 > 3900 && stringBuilder3.Length > 0)
						{
							if (flag2)
							{
								stringBuilder3.AppendLine("```");
							}
							list.Add(CreateEmbed(flag ? (serverName + " -- Balance Changes") : null, stringBuilder3.ToString(), flag));
							stringBuilder3.Clear();
							flag = false;
							if (flag2)
							{
								stringBuilder3.AppendLine("```diff");
							}
						}
						stringBuilder3.AppendLine(text4);
					}
					if (stringBuilder3.Length > 0)
					{
						stringBuilder2.Append(stringBuilder3);
					}
				}
				else
				{
					if (stringBuilder2.Length + item3.Length > 3900 && stringBuilder2.Length > 0)
					{
						list.Add(CreateEmbed(flag ? (serverName + " -- Balance Changes") : null, stringBuilder2.ToString(), flag));
						stringBuilder2.Clear();
						flag = false;
					}
					stringBuilder2.Append(item3);
				}
			}
			if (stringBuilder2.Length > 0)
			{
				list.Add(CreateEmbed(flag ? (serverName + " -- Balance Changes") : null, stringBuilder2.ToString(), flag));
			}
			return list;
		}

		private static JObject CreateEmbed(string title, string description, bool includeFooter)
		{
			JObject jObject = new JObject();
			if (title != null)
			{
				jObject["title"] = title;
			}
			jObject["description"] = description;
			jObject["color"] = 3447003;
			if (includeFooter)
			{
				jObject["footer"] = new JObject { ["text"] = "Si_UnitBalance | + = new change" };
				jObject["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
			}
			return jObject;
		}

		private static void PostEmbedsToDiscord(List<JObject> embeds)
		{
			List<string> oldIds = new List<string>(_discordMessageIds);
			Thread thread = new Thread(() =>
			{
				List<string> list = new List<string>();
				for (int i = 0; i < embeds.Count; i++)
				{
					try
					{
						StringContent jsonContent = new StringContent(new JObject { ["embeds"] = new JArray { embeds[i] } }.ToString(), Encoding.UTF8, "application/json");
						string text = null;
						if (i < oldIds.Count)
						{
							text = TryEditMessage(oldIds[i], jsonContent);
						}
						if (text == null)
						{
							text = PostNewMessage(jsonContent, i + 1, embeds.Count);
						}
						else
						{
							MelonLogger.Msg($"[Discord] Embed {i + 1}/{embeds.Count} edited.");
						}
						if (text != null)
						{
							list.Add(text);
						}
						if (i < embeds.Count - 1)
						{
							Thread.Sleep(1000);
						}
					}
					catch (Exception ex)
					{
						MelonLogger.Error($"[Discord] Failed for embed {i + 1}: {ex.InnerException?.Message ?? ex.Message}");
					}
				}
				if (oldIds.Count > embeds.Count)
				{
					for (int j = embeds.Count; j < oldIds.Count; j++)
					{
						try
						{
							string requestUri = _discordWebhookUrl + "/messages/" + oldIds[j];
							if (_httpClient.DeleteAsync(requestUri).Result.IsSuccessStatusCode)
							{
								MelonLogger.Msg($"[Discord] Deleted old message {j + 1}.");
							}
							Thread.Sleep(1000);
						}
						catch (Exception ex2)
						{
							MelonLogger.Warning("[Discord] Failed to delete old message: " + (ex2.InnerException?.Message ?? ex2.Message));
						}
					}
				}
				_discordMessageIds = list;
				SaveMessageIds();
			});
			thread.IsBackground = true;
			thread.Start();
		}

		private static string TryEditMessage(string messageId, StringContent jsonContent)
		{
			try
			{
				string requestUri = _discordWebhookUrl + "/messages/" + messageId;
				HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
				{
					Content = jsonContent
				};
				HttpResponseMessage result = _httpClient.SendAsync(request).Result;
				if (result.IsSuccessStatusCode)
				{
					return messageId;
				}
				int statusCode = (int)result.StatusCode;
				if (statusCode == 404)
				{
					MelonLogger.Msg("[Discord] Message " + messageId + " was deleted — will create new.");
				}
				else
				{
					string arg = "";
					try
					{
						arg = result.Content.ReadAsStringAsync().Result;
					}
					catch
					{
					}
					MelonLogger.Warning($"[Discord] PATCH returned {statusCode}: {arg}");
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[Discord] PATCH failed: " + (ex.InnerException?.Message ?? ex.Message));
			}
			return null;
		}

		private static string PostNewMessage(StringContent jsonContent, int index, int total)
		{
			try
			{
				string requestUri = _discordWebhookUrl + "?wait=true";
				HttpResponseMessage result = _httpClient.PostAsync(requestUri, jsonContent).Result;
				if (!result.IsSuccessStatusCode)
				{
					string arg = "";
					try
					{
						arg = result.Content.ReadAsStringAsync().Result;
					}
					catch
					{
					}
					MelonLogger.Warning($"[Discord] POST returned {(int)result.StatusCode}: {arg}");
					return null;
				}
				MelonLogger.Msg($"[Discord] Embed {index}/{total} posted.");
				return JObject.Parse(result.Content.ReadAsStringAsync().Result)["id"]?.Value<string>();
			}
			catch (Exception ex)
			{
				MelonLogger.Error("[Discord] POST failed: " + (ex.InnerException?.Message ?? ex.Message));
				return null;
			}
		}

		private static void DeleteOldMessages(List<string> messageIds)
		{
			Thread thread = new Thread(() =>
			{
				foreach (string messageId in messageIds)
				{
					try
					{
						string requestUri = _discordWebhookUrl + "/messages/" + messageId;
						_ = _httpClient.DeleteAsync(requestUri).Result;
						Thread.Sleep(1000);
					}
					catch
					{
					}
				}
			});
			thread.IsBackground = true;
			thread.Start();
		}

		private static string GetMessageIdsPath()
		{
			return Path.Combine(Path.GetDirectoryName(_configPath), "Si_UnitBalance_DiscordMsgIds.json");
		}

		private static void LoadMessageIds()
		{
			try
			{
				string messageIdsPath = GetMessageIdsPath();
				if (!File.Exists(messageIdsPath))
				{
					_discordMessageIds = new List<string>();
					return;
				}
				JArray jArray = JArray.Parse(File.ReadAllText(messageIdsPath));
				_discordMessageIds = new List<string>();
				foreach (JToken item in jArray)
				{
					string text = item.Value<string>();
					if (!string.IsNullOrEmpty(text))
					{
						_discordMessageIds.Add(text);
					}
				}
				MelonLogger.Msg($"[Discord] Loaded {_discordMessageIds.Count} stored message ID(s).");
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[Discord] Failed to load message IDs: " + ex.Message);
				_discordMessageIds = new List<string>();
			}
		}

		private static void SaveMessageIds()
		{
			try
			{
				string messageIdsPath = GetMessageIdsPath();
				JArray jArray = new JArray();
				foreach (string discordMessageId in _discordMessageIds)
				{
					jArray.Add(discordMessageId);
				}
				File.WriteAllText(messageIdsPath, jArray.ToString());
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[Discord] Failed to save message IDs: " + ex.Message);
			}
		}

		private static void SavePushSnapshot()
		{
			try
			{
				string text = Path.Combine(Path.GetDirectoryName(_configPath), "Si_UnitBalance_LastPush.json");
				File.Copy(_configPath, text, overwrite: true);
				MelonLogger.Msg("[Discord] Saved push snapshot to " + text);
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[Discord] Failed to save push snapshot: " + ex.Message);
			}
		}

		private static void DumpFieldDiscovery()
		{
			LogDebug("========== Field Discovery ==========");
			try
			{
				ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
				ObjectInfo[] array2;
				if (array.Length != 0)
				{
					DumpTypeFields("ObjectInfo", array[0]);
					array2 = array;
					foreach (ObjectInfo objectInfo in array2)
					{
						if (objectInfo == null || objectInfo.Prefab == null)
						{
							continue;
						}
						Unit component = objectInfo.Prefab.GetComponent<Unit>();
						if (!(component != null))
						{
							continue;
						}
						LogDebug("--- Unit fields (from '" + objectInfo.DisplayName + "') ---");
						DumpTypeFields("Unit", component);
						DamageManager component2 = objectInfo.Prefab.GetComponent<DamageManager>();
						if (!(component2 != null))
						{
							break;
						}
						DumpTypeFields("DamageManager", component2);
						FieldInfo field = component2.GetType().GetField("Data", BindingFlags.Instance | BindingFlags.Public);
						if (field != null)
						{
							object value = field.GetValue(component2);
							if (value != null)
							{
								DumpTypeFields("DamageManagerData", value);
							}
						}
						break;
					}
					array2 = array;
					foreach (ObjectInfo objectInfo2 in array2)
					{
						if (!(objectInfo2 == null) && !(objectInfo2.ConstructionData == null))
						{
							LogDebug("--- ConstructionData fields (from '" + objectInfo2.DisplayName + "') ---");
							DumpTypeFields("ConstructionData", objectInfo2.ConstructionData);
							break;
						}
					}
					array2 = array;
					foreach (ObjectInfo objectInfo3 in array2)
					{
						if (!(objectInfo3 == null) && !(objectInfo3.Prefab == null))
						{
							Structure component3 = objectInfo3.Prefab.GetComponent<Structure>();
							if (component3 != null)
							{
								LogDebug("--- Structure fields (from '" + objectInfo3.DisplayName + "') ---");
								DumpTypeFields("Structure", component3);
								break;
							}
						}
					}
				}
				string[] array3 = new string[20]
				{
					"Shrimp", "Crab", "Behemoth", "Scorpion", "Defiler", "Colossus", "Dragonfly", "Goliath", "Hunter", "Firebug",
					"Railgun Tank", "Nest", "Interceptor", "Headquarters", "Combat Tank", "Heavy Tank", "Barrage Truck", "Siege Tank", "Harvester", "Bio Cache"
				};
				foreach (string text in array3)
				{
					LogDebug("--- " + text.ToUpper() + " DEEP COMPONENT DUMP ---");
					array2 = array;
					foreach (ObjectInfo objectInfo4 in array2)
					{
						if (objectInfo4 == null || objectInfo4.DisplayName != text)
						{
							continue;
						}
						if (objectInfo4.Prefab == null)
						{
							LogDebug("  " + text + " prefab is null");
							break;
						}
						Component[] components = objectInfo4.Prefab.GetComponents<Component>();
						LogDebug($"  {text} has {components.Length} root components:");
						Component[] array4 = components;
						foreach (Component component4 in array4)
						{
							if (component4 == null)
							{
								continue;
							}
							string name = component4.GetType().Name;
							LogDebug("  --- Root Component: " + name + " (" + component4.GetType().FullName + ") ---");
							DumpTypeFields(name, component4);
							FieldInfo[] fields;
							if (name == "CreatureDecapod")
							{
								fields = component4.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
								foreach (FieldInfo fieldInfo in fields)
								{
									if (fieldInfo.FieldType.Name == "CreatureAttack")
									{
										object value2 = fieldInfo.GetValue(component4);
										if (value2 != null)
										{
											LogDebug("  --- Referenced Object: " + fieldInfo.Name + " (type " + fieldInfo.FieldType.FullName + ") ---");
											DumpTypeFields("CreatureAttack:" + fieldInfo.Name, value2);
										}
										else
										{
											LogDebug("  --- Referenced Object: " + fieldInfo.Name + " = NULL ---");
										}
									}
								}
							}
							if (!(name == "VehicleTurret"))
							{
								continue;
							}
							Type type = component4.GetType();
							string[] array5 = new string[2] { "PrimaryProjectile", "SecondaryProjectile" };
							foreach (string text2 in array5)
							{
								try
								{
									FieldInfo field2 = type.GetField(text2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
									if (field2 == null)
									{
										PropertyInfo property = type.GetProperty(text2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
										if (property != null)
										{
											object value3 = property.GetValue(component4);
											if (value3 != null)
											{
												LogDebug("  --- VT " + text2 + ": " + ((UnityEngine.Object)value3).name + " ---");
												DumpTypeFields("VT_ProjectileData:" + text2, value3);
											}
										}
									}
									else
									{
										object value4 = field2.GetValue(component4);
										if (value4 != null)
										{
											LogDebug("  --- VT " + text2 + ": " + ((UnityEngine.Object)value4).name + " ---");
											DumpTypeFields("VT_ProjectileData:" + text2, value4);
										}
										else
										{
											LogDebug("  --- VT " + text2 + ": NULL ---");
										}
									}
								}
								catch (Exception ex)
								{
									LogDebug("  --- VT " + text2 + ": Error: " + ex.Message + " ---");
								}
							}
							fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							foreach (FieldInfo fieldInfo2 in fields)
							{
								if (!(fieldInfo2.FieldType.Name == "ProjectileData") || !(fieldInfo2.Name != "PrimaryProjectile") || !(fieldInfo2.Name != "SecondaryProjectile"))
								{
									continue;
								}
								try
								{
									object value5 = fieldInfo2.GetValue(component4);
									if (value5 != null)
									{
										LogDebug("  --- VT " + fieldInfo2.Name + ": " + ((UnityEngine.Object)value5).name + " ---");
										DumpTypeFields("VT_ProjectileData:" + fieldInfo2.Name, value5);
									}
								}
								catch
								{
								}
							}
						}
						Component[] componentsInChildren = objectInfo4.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
						HashSet<int> hashSet = new HashSet<int>();
						array4 = components;
						foreach (Component component5 in array4)
						{
							if (component5 != null)
							{
								hashSet.Add(component5.GetInstanceID());
							}
						}
						LogDebug($"  --- Child components (total {componentsInChildren.Length}, skipping root-level) ---");
						array4 = componentsInChildren;
						foreach (Component component6 in array4)
						{
							if (!(component6 == null) && !hashSet.Contains(component6.GetInstanceID()))
							{
								hashSet.Add(component6.GetInstanceID());
								string name2 = component6.GetType().Name;
								string text3 = name2.ToLower();
								if (!(text3 == "transform") && !text3.Contains("mesh") && !text3.Contains("renderer") && !text3.Contains("animator") && !text3.Contains("lodgroup") && !text3.Contains("particle") && !text3.Contains("collider") && !text3.Contains("light") && !text3.Contains("audio") && !text3.Contains("canvas") && !text3.Contains("cloth"))
								{
									string text4 = ((component6.gameObject != null) ? component6.gameObject.name : "?");
									LogDebug("  --- Child [" + text4 + "] Component: " + name2 + " (" + component6.GetType().FullName + ") ---");
									DumpTypeFields(name2, component6);
								}
							}
						}
						break;
					}
					LogDebug("--- END " + text.ToUpper() + " DUMP ---");
				}
				LogDebug("--- Production Trees (Structure -> Producible Items) ---");
				array2 = array;
				foreach (ObjectInfo objectInfo5 in array2)
				{
					if (objectInfo5 == null || objectInfo5.Prefab == null)
					{
						continue;
					}
					Structure component7 = objectInfo5.Prefab.GetComponent<Structure>();
					if (component7 == null || component7.ConstructionOptions == null || component7.ConstructionOptions.Count == 0)
					{
						continue;
					}
					string text5 = "?";
					try
					{
						Team defaultTeam = GetDefaultTeam(component7);
						if (defaultTeam != null)
						{
							text5 = defaultTeam.name;
						}
					}
					catch
					{
					}
					string text6 = "";
					if (objectInfo5.ConstructionData != null)
					{
						text6 = $" Cost={objectInfo5.ConstructionData.ResourceCost} Build={objectInfo5.ConstructionData.BuildUpTime:F0}s MinTier={objectInfo5.ConstructionData.MinimumTeamTier} MaxDist={objectInfo5.ConstructionData.MaximumBaseStructureDistance:F0} UseRadius={objectInfo5.ConstructionData.MaximumDistanceUseRadius}";
					}
					LogDebug("  PRODUCER: " + objectInfo5.DisplayName + " (" + objectInfo5.name + ") [" + text5 + "]" + text6);
					foreach (ConstructionData constructionOption in component7.ConstructionOptions)
					{
						if (constructionOption == null)
						{
							continue;
						}
						string text7 = "(unknown)";
						string text8 = "";
						try
						{
							if (constructionOption.ObjectInfo != null)
							{
								text7 = constructionOption.ObjectInfo.DisplayName ?? constructionOption.ObjectInfo.name;
								text8 = constructionOption.ObjectInfo.name;
							}
						}
						catch
						{
						}
						LogDebug($"    -> {text7,-25} Cost={constructionOption.ResourceCost,5} Build={constructionOption.BuildUpTime,4:F0}s MinTier={constructionOption.MinimumTeamTier,2} TechTier={constructionOption.TechnologyTier,2} MaxDist={constructionOption.MaximumBaseStructureDistance:F0} UseRadius={constructionOption.MaximumDistanceUseRadius} ({text8})");
					}
				}
				LogDebug("--- ProjectileData Asset Dump ---");
				ProjectileData[] array6 = Resources.FindObjectsOfTypeAll<ProjectileData>();
				LogDebug($"  Found {array6.Length} ProjectileData assets");
				ProjectileData[] array7 = array6;
				foreach (ProjectileData projectileData in array7)
				{
					if (!(projectileData == null))
					{
						LogDebug("  === ProjectileData: " + projectileData.name + " ===");
						DumpTypeFields("ProjectileData", projectileData);
					}
				}
				LogDebug("--- END ProjectileData Dump ---");
			}
			catch (Exception ex2)
			{
				MelonLogger.Error("DumpFieldDiscovery error: " + ex2.Message);
			}
			LogDebug("========== End Field Discovery ==========");
		}

		private static float RF(Type t, object o, string name, float def = 0f)
		{
			try
			{
				FieldInfo field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public);
				if (field != null)
				{
					return Convert.ToSingle(field.GetValue(o));
				}
			}
			catch
			{
			}
			return def;
		}

		private static bool RB(Type t, object o, string name, bool def = false)
		{
			try
			{
				FieldInfo field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public);
				if (field != null)
				{
					return (bool)field.GetValue(o);
				}
			}
			catch
			{
			}
			return def;
		}

		private static string RS(Type t, object o, string name)
		{
			try
			{
				FieldInfo field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public);
				if (field != null)
				{
					return field.GetValue(o)?.ToString() ?? "";
				}
			}
			catch
			{
			}
			return "";
		}

		internal static void DumpProjectileDataCsv()
		{
			try
			{
				string text = Path.Combine(Path.GetDirectoryName(_configPath), "ProjectileData_Dump.csv");
				ProjectileData[] array = Resources.FindObjectsOfTypeAll<ProjectileData>();
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("name,instant_hit,impact_scale_by_speed,impact_damage,impact_dmg_type,base_speed,min_speed_pct,lifetime,drag_coeff,air_drag_mult,gravity_scale,mass,diameter,penetrating,penetrating_damage,splash,splash_dmg_max,splash_radius_min,splash_radius_max,splash_radius_pow,ai_lead_drag,ai_lead_velocity,ai_lead_gravity");
				int num = 0;
				ProjectileData[] array2 = array;
				foreach (ProjectileData projectileData in array2)
				{
					if (!(projectileData == null))
					{
						Type type = projectileData.GetType();
						string value = (projectileData.name ?? "").Replace(",", " ");
						stringBuilder.Append(value).Append(',').Append(RB(type, projectileData, "m_InstantHit"))
							.Append(',')
							.Append(RB(type, projectileData, "m_bImpactScaleBySpeed"))
							.Append(',')
							.Append(RF(type, projectileData, "m_fImpactDamage").ToString("0.##"))
							.Append(',')
							.Append(RS(type, projectileData, "m_ImpactDamageType"))
							.Append(',')
							.Append(RF(type, projectileData, "m_fBaseSpeed").ToString("0.##"))
							.Append(',')
							.Append(RF(type, projectileData, "m_fMinSpeedPct").ToString("0.###"))
							.Append(',')
							.Append(RF(type, projectileData, "m_fLifeTime").ToString("0.##"))
							.Append(',')
							.Append(RF(type, projectileData, "m_fDragCoefficient").ToString("0.####"))
							.Append(',')
							.Append(RF(type, projectileData, "AirDragForceMultiplier").ToString("0.####"))
							.Append(',')
							.Append(RF(type, projectileData, "m_fGravityScale").ToString("0.###"))
							.Append(',')
							.Append(RF(type, projectileData, "m_fMass").ToString("0.###"))
							.Append(',')
							.Append(RF(type, projectileData, "m_fDiameter").ToString("0.###"))
							.Append(',')
							.Append(RB(type, projectileData, "m_bPenetrating"))
							.Append(',')
							.Append(RF(type, projectileData, "m_fPenetratingDamage").ToString("0.##"))
							.Append(',')
							.Append(RB(type, projectileData, "m_bSplash"))
							.Append(',')
							.Append(RF(type, projectileData, "m_fSplashDamageMax").ToString("0.##"))
							.Append(',')
							.Append(RF(type, projectileData, "m_fSplashDamageRadiusMin").ToString("0.##"))
							.Append(',')
							.Append(RF(type, projectileData, "m_fSplashDamageRadiusMax").ToString("0.##"))
							.Append(',')
							.Append(RF(type, projectileData, "m_fSplashDamageRadiusPow").ToString("0.###"))
							.Append(',')
							.Append(RF(type, projectileData, "m_AIAimLeadFactor_Drag").ToString("0.###"))
							.Append(',')
							.Append(RF(type, projectileData, "m_AIAimLeadFactor_Velocity").ToString("0.###"))
							.Append(',')
							.Append(RF(type, projectileData, "m_AIAimLeadFactor_Gravity").ToString("0.###"))
							.AppendLine();
						num++;
					}
				}
				File.WriteAllText(text, stringBuilder.ToString());
				MelonLogger.Msg($"[UnitBalance] Wrote {num} projectiles to {text}");
			}
			catch (Exception ex)
			{
				MelonLogger.Error("DumpProjectileDataCsv error: " + ex.Message);
			}
		}

		internal static void DumpTurretAimCompCsv()
		{
			try
			{
				string text = Path.Combine(Path.GetDirectoryName(_configPath), "TurretAimComp_Dump.csv");
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("unit,turret_go,aim_compensation,pitch_max_up,pitch_max_down,pitch_up_yawcurve,pitch_down_yawcurve,pri_projectile,sec_projectile");
				ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
				int num = 0;
				HashSet<string> hashSet = new HashSet<string>();
				ObjectInfo[] array2 = array;
				foreach (ObjectInfo objectInfo in array2)
				{
					if (objectInfo == null || objectInfo.Prefab == null)
					{
						continue;
					}
					string text2 = objectInfo.DisplayName ?? objectInfo.name ?? "";
					if (string.IsNullOrEmpty(text2))
					{
						continue;
					}
					Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
					foreach (Component component in componentsInChildren)
					{
						if (component == null || component.GetType().Name != "VehicleTurret")
						{
							continue;
						}
						Type type = component.GetType();
						FieldInfo field = type.GetField("AimCompensation", BindingFlags.Instance | BindingFlags.Public);
						if (!(field == null))
						{
							string text3 = "?";
							try
							{
								text3 = field.GetValue(component)?.ToString() ?? "?";
							}
							catch
							{
							}
							float num2 = TurretFloat(type, component, "PitchMaxUp");
							float num3 = TurretFloat(type, component, "PitchMaxDown");
							bool value = TurretBool(type, component, "PitchUpClampByYawCurve");
							bool value2 = TurretBool(type, component, "PitchDownClampByYawCurve");
							string value3 = ProjName(type, component, "PrimaryProjectileData");
							string value4 = ProjName(type, component, "SecondaryProjectileData");
							string text4 = ((component.gameObject != null) ? component.gameObject.name : "?");
							string item = text2 + "|" + text4 + "|" + text3;
							if (hashSet.Add(item))
							{
								stringBuilder.Append(text2.Replace(",", " ")).Append(',').Append(text4.Replace(",", " "))
									.Append(',')
									.Append(text3)
									.Append(',')
									.Append(num2.ToString("0.#"))
									.Append(',')
									.Append(num3.ToString("0.#"))
									.Append(',')
									.Append(value)
									.Append(',')
									.Append(value2)
									.Append(',')
									.Append(value3)
									.Append(',')
									.Append(value4)
									.AppendLine();
								num++;
							}
						}
					}
				}
				File.WriteAllText(text, stringBuilder.ToString());
				MelonLogger.Msg($"[UnitBalance] Wrote {num} turret aim-compensation rows to {text}");
			}
			catch (Exception ex)
			{
				MelonLogger.Error("DumpTurretAimCompCsv error: " + ex.Message);
			}
		}

		private static float TurretFloat(Type ct, object comp, string field)
		{
			try
			{
				FieldInfo field2 = ct.GetField(field, BindingFlags.Instance | BindingFlags.Public);
				if (field2 != null)
				{
					return (float)field2.GetValue(comp);
				}
			}
			catch
			{
			}
			return -1f;
		}

		private static bool TurretBool(Type ct, object comp, string field)
		{
			try
			{
				FieldInfo field2 = ct.GetField(field, BindingFlags.Instance | BindingFlags.Public);
				if (field2 != null)
				{
					return (bool)field2.GetValue(comp);
				}
			}
			catch
			{
			}
			return false;
		}

		private static string ProjName(Type ct, object comp, string field)
		{
			try
			{
				FieldInfo field2 = ct.GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field2 == null)
				{
					return "";
				}
				object value = field2.GetValue(comp);
				if (value == null)
				{
					return "";
				}
				return (((UnityEngine.Object)value).name ?? "").Replace(",", " ");
			}
			catch
			{
				return "";
			}
		}

		private static void DumpTypeFields(string typeName, object obj)
		{
			try
			{
				Type type = obj.GetType();
				LogDebug("  [" + typeName + "] Type: " + type.FullName);
				FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
				foreach (FieldInfo fieldInfo in fields)
				{
					try
					{
						string text = fieldInfo.GetValue(obj)?.ToString() ?? "null";
						if (text.Length > 80)
						{
							text = text.Substring(0, 80) + "...";
						}
						LogDebug($"  [{typeName}] field {fieldInfo.FieldType.Name,-16} {fieldInfo.Name} = {text}");
					}
					catch
					{
						LogDebug($"  [{typeName}] field {fieldInfo.FieldType.Name,-16} {fieldInfo.Name} = (error reading)");
					}
				}
				PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
				foreach (PropertyInfo propertyInfo in properties)
				{
					if (!propertyInfo.CanRead)
					{
						continue;
					}
					try
					{
						string text2 = propertyInfo.GetValue(obj)?.ToString() ?? "null";
						if (text2.Length > 80)
						{
							text2 = text2.Substring(0, 80) + "...";
						}
						LogDebug($"  [{typeName}] prop  {propertyInfo.PropertyType.Name,-16} {propertyInfo.Name} = {text2}");
					}
					catch
					{
						LogDebug($"  [{typeName}] prop  {propertyInfo.PropertyType.Name,-16} {propertyInfo.Name} = (error reading)");
					}
				}
			}
			catch (Exception ex)
			{
				LogDebug("  [" + typeName + "] Error dumping: " + ex.Message);
			}
		}

		internal static void EnsureHQDecayWarningHook()
		{
			try
			{
				GameEvents.OnStructureDestroyed = (Action<Structure, GameObject>)Delegate.Remove(GameEvents.OnStructureDestroyed, new Action<Structure, GameObject>(OnStructureDestroyed_HQWarning));
				GameEvents.OnStructureDestroyed = (Action<Structure, GameObject>)Delegate.Combine(GameEvents.OnStructureDestroyed, new Action<Structure, GameObject>(OnStructureDestroyed_HQWarning));
				if (!_hqDecayWarningHooked)
				{
					_hqDecayWarningHooked = true;
					MelonLogger.Msg("[HQWarn] HQ-killed decay warning hooked (GameEvents.OnStructureDestroyed)");
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[HQWarn] Failed to hook OnStructureDestroyed: " + ex.Message);
			}
		}

		private static void OnStructureDestroyed_HQWarning(Structure structure, GameObject instigator)
		{
			try
			{
				if (structure == null || structure.Team == null)
				{
					return;
				}
				ObjectInfo baseStructure = structure.Team.BaseStructure;
				if (!(baseStructure == null) && !(structure.ObjectInfo != baseStructure))
				{
					DecaySettings decaySettings = ((structure is AlienStructure) ? _decayAlien : _decayHuman);
					if (decaySettings != null && decaySettings.Enabled)
					{
						float seconds = ((decaySettings.Delay >= 0f) ? decaySettings.Delay : 10f);
						string compassDirection = GetCompassDirection(structure.transform.position);
						string text = FormatDelay(seconds);
						string message = _chatPrefix + "<color=#FFAA00>Your HQ (" + compassDirection + ") was destroyed — building decay starts in " + text + ".</color>";
						BroadcastChatToTeam(structure.Team, message);
						MelonLogger.Msg("[HQWarn] HQ destroyed for team " + structure.Team.TeamShortName + " (" + compassDirection + ") — decay in " + text);
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[HQWarn] handler error: " + ex.Message);
			}
		}

		private static string GetCompassDirection(Vector3 pos)
		{
			float x = pos.x;
			float z = pos.z;
			string text;
			if (z > 600f)
			{
				text = "north";
			}
			else
			{
				text = ((z < -600f) ? "south" : "");
			}
			string text2;
			if (x > 600f)
			{
				text2 = "east";
			}
			else
			{
				text2 = ((x < -600f) ? "west" : "");
			}
			if (text.Length > 0 && text2.Length > 0)
			{
				return text + "-" + text2;
			}
			if (text.Length > 0)
			{
				return text;
			}
			if (text2.Length > 0)
			{
				return text2;
			}
			return "center";
		}

		private static string FormatDelay(float seconds)
		{
			if (seconds >= 60f)
			{
				float num = seconds / 60f;
				if (Math.Abs(num - Mathf.Round(num)) < 0.05f)
				{
					return Mathf.RoundToInt(num) + " min";
				}
				return num.ToString("0.#") + " min";
			}
			return Mathf.RoundToInt(seconds) + " sec";
		}

		private static void BroadcastChatToTeam(Team team, string message)
		{
			try
			{
				if (team == null)
				{
					return;
				}
				IList list = null;
				PropertyInfo property = typeof(Player).GetProperty("Players", BindingFlags.Static | BindingFlags.Public);
				if (property != null)
				{
					list = property.GetValue(null) as IList;
				}
				else
				{
					FieldInfo field = typeof(Player).GetField("Players", BindingFlags.Static | BindingFlags.Public);
					if (field != null)
					{
						list = field.GetValue(null) as IList;
					}
				}
				if (list == null)
				{
					return;
				}
				object obj = typeof(NetworkGameServer).GetMethod("GetServerPlayer", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
				foreach (object item in list)
				{
					if (item != null && !item.Equals(obj))
					{
						Player player = item as Player;
						if (!(player == null) && !(player.Team != team))
						{
							SendChatToPlayer(item, message);
						}
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[HQWarn] team broadcast error: " + ex.Message);
			}
		}

		private static string SanitizeFileName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return "";
			}
			StringBuilder stringBuilder = new StringBuilder(name.Length);
			foreach (char c in name)
			{
				if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.')
				{
					stringBuilder.Append(c);
				}
			}
			string text = stringBuilder.ToString().Trim('.');
			string text2 = text.ToUpperInvariant();
			string[] array = new string[22]
			{
				"CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6",
				"COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7",
				"LPT8", "LPT9"
			};
			foreach (string text3 in array)
			{
				if (text2 == text3 || text2.StartsWith(text3 + "."))
				{
					return "_" + text;
				}
			}
			if (text.Length > 60)
			{
				text = text.Substring(0, 60);
			}
			return text;
		}

		private static bool EnsureSaveDir()
		{
			try
			{
				if (!Directory.Exists(_configSaveDir))
				{
					Directory.CreateDirectory(_configSaveDir);
				}
				return true;
			}
			catch (Exception ex)
			{
				MelonLogger.Error("[JSON] Failed to create save dir: " + ex.Message);
				return false;
			}
		}

		private static string[] GetSavedConfigs()
		{
			try
			{
				if (!Directory.Exists(_configSaveDir))
				{
					return new string[0];
				}
				string[] files = Directory.GetFiles(_configSaveDir, "*.json");
				string fullPath = Path.GetFullPath(_configSaveDir);
				List<string> list = new List<string>();
				string[] array = files;
				foreach (string path in array)
				{
					string fullPath2 = Path.GetFullPath(path);
					if (fullPath2.StartsWith(fullPath + Path.DirectorySeparatorChar) || fullPath2.StartsWith(fullPath + Path.AltDirectorySeparatorChar) || !(fullPath2 != fullPath))
					{
						list.Add(Path.GetFileName(path));
					}
				}
				list.Sort((string a, string b) => string.Compare(b, a, StringComparison.OrdinalIgnoreCase));
				return list.ToArray();
			}
			catch (Exception ex)
			{
				MelonLogger.Error("[JSON] Failed to list saved configs: " + ex.Message);
				return new string[0];
			}
		}

		private static bool SaveConfigAs(string optionalName)
		{
			if (!EnsureSaveDir())
			{
				return false;
			}
			try
			{
				string text = DateTime.Now.ToString("yyyyMMddHHmm");
				string text3;
				if (!string.IsNullOrEmpty(optionalName))
				{
					string text2 = SanitizeFileName(optionalName);
					if (string.IsNullOrEmpty(text2))
					{
						text2 = "config";
					}
					text3 = text + "_" + text2 + ".json";
				}
				else
				{
					text3 = text + ".json";
				}
				string text4 = Path.Combine(_configSaveDir, text3);
				if (!Path.GetFullPath(text4).StartsWith(Path.GetFullPath(_configSaveDir)))
				{
					MelonLogger.Error("[JSON] Path traversal attempt blocked in save.");
					return false;
				}
				if (!File.Exists(_configPath))
				{
					MelonLogger.Error("[JSON] Active config not found, nothing to save.");
					return false;
				}
				File.Copy(_configPath, text4, overwrite: false);
				MelonLogger.Msg("[JSON] Saved config as: " + text3);
				return true;
			}
			catch (Exception ex)
			{
				MelonLogger.Error("[JSON] Failed to save config: " + ex.Message);
				return false;
			}
		}

		private static bool LoadSavedConfig(string fileName)
		{
			try
			{
				string fileName2 = Path.GetFileName(fileName);
				if (!fileName2.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
				{
					MelonLogger.Error("[JSON] Rejected non-JSON file: " + fileName2);
					return false;
				}
				string text = Path.Combine(_configSaveDir, fileName2);
				string fullPath = Path.GetFullPath(text);
				string fullPath2 = Path.GetFullPath(_configSaveDir);
				if (!fullPath.StartsWith(fullPath2 + Path.DirectorySeparatorChar) && !fullPath.StartsWith(fullPath2 + Path.AltDirectorySeparatorChar))
				{
					MelonLogger.Error("[JSON] Path traversal attempt blocked in load: " + fileName2);
					return false;
				}
				if (!File.Exists(text))
				{
					MelonLogger.Error("[JSON] Saved config not found: " + fileName2);
					return false;
				}
				try
				{
					JObject.Parse(File.ReadAllText(text));
				}
				catch (Exception ex)
				{
					MelonLogger.Error("[JSON] Invalid JSON in " + fileName2 + ": " + ex.Message);
					return false;
				}
				File.Copy(text, _configPath, overwrite: true);
				MelonLogger.Msg("[JSON] Loaded config from: " + fileName2);
				return true;
			}
			catch (Exception ex2)
			{
				MelonLogger.Error("[JSON] Failed to load config: " + ex2.Message);
				return false;
			}
		}

		private static bool ResetToBlankConfig()
		{
			try
			{
				string contents = "{\r\n    \"enabled\": true,\r\n    \"dump_fields\": false,\r\n    \"description\": \"Blank config with vanilla settings. All multipliers at defaults.\",\r\n    \"tech_time\": {\r\n        \"tier_1\": 30,\r\n        \"tier_2\": 30,\r\n        \"tier_3\": 30,\r\n        \"tier_4\": 30,\r\n        \"tier_5\": 30,\r\n        \"tier_6\": 30,\r\n        \"tier_7\": 30,\r\n        \"tier_8\": 30\r\n    },\r\n    \"tech_cost\": {\r\n        \"tier_1\": -1,\r\n        \"tier_2\": -1,\r\n        \"tier_3\": -1,\r\n        \"tier_4\": -1,\r\n        \"tier_5\": -1,\r\n        \"tier_6\": -1,\r\n        \"tier_7\": -1,\r\n        \"tier_8\": -1\r\n    },\r\n    \"units\": {}\r\n}";
				File.WriteAllText(_configPath, contents);
				MelonLogger.Msg("[JSON] Config reset to blank (vanilla settings).");
				return true;
			}
			catch (Exception ex)
			{
				MelonLogger.Error("[JSON] Failed to reset config: " + ex.Message);
				return false;
			}
		}

		private static string GetCurrentMapName()
		{
			try
			{
				return SceneManager.GetActiveScene().name ?? "";
			}
			catch
			{
				return "";
			}
		}

		private static void OnFinishLoadingLevel(string sceneName, GameModeInfo gameModeInfo)
		{
			if (_enabled && _configLoaded && !string.IsNullOrEmpty(sceneName) && !(sceneName == "Loading") && !(sceneName == "MainMenu"))
			{
				if (_overridesApplied)
				{
					MelonLogger.Msg("[UnitBalance] OnFinishLoadingLevel (" + sceneName + ") — overrides already active, skipping");
					return;
				}
				MelonLogger.Msg("[UnitBalance] OnFinishLoadingLevel (" + sceneName + ") — applying overrides (first time)");
				_lastMapName = sceneName;
				_vanillaBaseCache.Clear();
				ApplyOverridesLogic();
			}
		}

		private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			string name = scene.name;
			if (!string.IsNullOrEmpty(name) && !(name == "Loading") && !(name == "MainMenu"))
			{
				MelonLogger.Msg("[UnitBalance] Scene loaded: " + name);
			}
		}

		private static bool InitOverrideManager()
		{
			if (_omInitialized)
			{
				return _overrideManagerType != null;
			}
			_omInitialized = true;
			try
			{
				_overrideManagerType = typeof(OverrideManager);
				Type nestedType = _overrideManagerType.GetNestedType("EOverrideDataType");
				if (nestedType == null)
				{
					MelonLogger.Warning("[OverrideManager] EOverrideDataType enum not found");
					return false;
				}
				_omTypeFloat = Enum.Parse(nestedType, "Float");
				_omTypeInt = Enum.Parse(nestedType, "Int");
				_omTypeBool = Enum.Parse(nestedType, "Bool");
				_omSetMethod = _overrideManagerType.GetMethod("Set", BindingFlags.Static | BindingFlags.Public, null, new Type[6]
				{
					nestedType,
					typeof(string),
					typeof(string),
					typeof(object),
					typeof(bool),
					typeof(bool)
				}, null);
				if (_omSetMethod == null)
				{
					MelonLogger.Warning("[OverrideManager] Set method not found");
					return false;
				}
				_omRevertAllMethod = _overrideManagerType.GetMethod("RevertAll", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
				{
					typeof(bool),
					typeof(bool)
				}, null);
				if (_omRevertAllMethod == null)
				{
					MelonLogger.Warning("[OverrideManager] RevertAll method not found");
					return false;
				}
				MelonLogger.Msg("[OverrideManager] Reflection wrapper initialized successfully");
				return true;
			}
			catch (Exception ex)
			{
				MelonLogger.Error("[OverrideManager] Init error: " + ex.Message);
				return false;
			}
		}

		private static void RegisterDamageManagerDataInOM()
		{
			try
			{
				Type nestedType = typeof(OverrideManager).GetNestedType("AssetSourceRegistry", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (nestedType == null)
				{
					MelonLogger.Warning("[OM-DMD] AssetSourceRegistry type not found");
					return;
				}
				FieldInfo field = nestedType.GetField("_sources", BindingFlags.Static | BindingFlags.NonPublic);
				if (field == null)
				{
					MelonLogger.Warning("[OM-DMD] _sources field not found");
					return;
				}
				if (!(field.GetValue(null) is IDictionary dictionary))
				{
					MelonLogger.Warning("[OM-DMD] _sources is null");
					return;
				}
				if (dictionary.Count == 0)
				{
					MethodInfo method = nestedType.GetMethod("BuildSources", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (method != null)
					{
						method.Invoke(null, null);
						MelonLogger.Msg($"[OM-DMD] Triggered BuildSources() — {dictionary.Count} built-in types registered");
					}
					else
					{
						MelonLogger.Warning("[OM-DMD] BuildSources method not found — built-in OM types may be missing");
					}
				}
				ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
				List<object> list = new List<object>();
				List<string> list2 = new List<string>();
				HashSet<string> hashSet = new HashSet<string>();
				ObjectInfo[] array2 = array;
				foreach (ObjectInfo objectInfo in array2)
				{
					if (objectInfo == null || objectInfo.Prefab == null)
					{
						continue;
					}
					DamageManager component = objectInfo.Prefab.GetComponent<DamageManager>();
					if (component == null)
					{
						continue;
					}
					FieldInfo field2 = component.GetType().GetField("Data", BindingFlags.Instance | BindingFlags.Public);
					if (field2 == null)
					{
						continue;
					}
					UnityEngine.Object obj = field2.GetValue(component) as UnityEngine.Object;
					if (!(obj == null))
					{
						string name = obj.name;
						if (!string.IsNullOrEmpty(name) && !hashSet.Contains(name))
						{
							hashSet.Add(name);
							string item = "DamageManagerData/DamageManagerData_" + name + ".asset";
							list.Add(obj);
							list2.Add(item);
						}
					}
				}
				if (list.Count == 0)
				{
					MelonLogger.Warning("[OM-DMD] No DamageManagerData assets found");
					return;
				}
				Type type = list[0].GetType();
				IList list3 = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
				foreach (object item2 in list)
				{
					list3.Add(item2);
				}
				List<string> list4 = new List<string>(list2);
				object value = Activator.CreateInstance(field.FieldType.GetGenericArguments()[1], type, list3, list4, null);
				dictionary["DamageManagerData"] = value;
				MelonLogger.Msg($"[OM-DMD] Registered {list.Count} DamageManagerData assets in AssetSourceRegistry");
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[OM-DMD] Registration failed: " + ex.Message);
			}
		}

		private static bool OMSetFloat(string target, string memberName, float value)
		{
			try
			{
				object obj = _omSetMethod.Invoke(null, new object[6] { _omTypeFloat, target, memberName, value, true, false });
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
				int num2 = num & (flag ? 1 : 0);
				if (num2 == 0)
				{
					MelonLogger.Warning($"[OM] Set failed: Float '{target}'.'{memberName}' = {value}");
				}
				return (byte)num2 != 0;
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[OM] Set exception: Float '" + target + "'.'" + memberName + "': " + (ex.InnerException?.Message ?? ex.Message));
				return false;
			}
		}

		private static bool OMSetBool(string target, string memberName, bool value)
		{
			try
			{
				object obj = _omSetMethod.Invoke(null, new object[6] { _omTypeBool, target, memberName, value, true, false });
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
				int num2 = num & (flag ? 1 : 0);
				if (num2 == 0)
				{
					MelonLogger.Warning($"[OM] Set failed: Bool '{target}'.'{memberName}' = {value}");
				}
				return (byte)num2 != 0;
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[OM] Set exception: Bool '" + target + "'.'" + memberName + "': " + (ex.InnerException?.Message ?? ex.Message));
				return false;
			}
		}

		private static bool OMSetInt(string target, string memberName, int value)
		{
			try
			{
				object obj = _omSetMethod.Invoke(null, new object[6] { _omTypeInt, target, memberName, value, true, false });
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
				int num2 = num & (flag ? 1 : 0);
				if (num2 == 0)
				{
					MelonLogger.Warning($"[OM] Set failed: Int '{target}'.'{memberName}' = {value}");
				}
				return (byte)num2 != 0;
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[OM] Set exception: Int '" + target + "'.'" + memberName + "': " + (ex.InnerException?.Message ?? ex.Message));
				return false;
			}
		}

		private static void OMRevertAll()
		{
			try
			{
				_allowRevertAll = true;
				_omRevertAllMethod.Invoke(null, new object[2] { true, false });
				MelonLogger.Msg("[OverrideManager] RevertAll called (notify=true)");
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[OverrideManager] RevertAll error: " + (ex.InnerException?.Message ?? ex.Message));
			}
			finally
			{
				_allowRevertAll = false;
			}
		}

		private static void ApplyOverridesLogic()
		{
			try
			{
				if (!_enabled || !_configLoaded)
				{
					return;
				}
				if (_overridesApplied)
				{
					MelonLogger.Msg("[UnitBalance] Overrides already active — skipping re-apply");
					return;
				}
				_nameCache.Clear();
				string text = null;
				string newVersion = null;
				if (!_fieldsDumped)
				{
					try
					{
						string text2 = Application.version ?? "unknown";
						string path = Path.Combine(Path.GetDirectoryName(_configPath), "game_version.txt");
						string text3 = "";
						if (File.Exists(path))
						{
							text3 = File.ReadAllText(path).Trim();
						}
						if (text3 != text2)
						{
							MelonLogger.Msg("[VERSION] Game version changed: '" + text3 + "' -> '" + text2 + "' — triggering auto-dump");
							_dumpFields = true;
							text = text3;
							newVersion = text2;
							File.WriteAllText(path, text2);
						}
					}
					catch (Exception ex)
					{
						MelonLogger.Warning("[VERSION] Failed to check game version: " + ex.Message);
					}
				}
				if (_dumpFields && !_fieldsDumped)
				{
					JObject jObject = null;
					if (text != null)
					{
						try
						{
							string path2 = Path.Combine(Path.GetDirectoryName(_configPath), "Si_UnitBalance_Dump.json");
							if (File.Exists(path2))
							{
								jObject = JObject.Parse(File.ReadAllText(path2));
							}
						}
						catch (Exception ex2)
						{
							MelonLogger.Warning("[MIGRATE] Failed to read old dump for migration: " + ex2.Message);
						}
					}
					DumpFieldDiscovery();
					DumpAllUnitsJson();
					DumpProjectileDataCsv();
					DumpTurretAimCompCsv();
					if (jObject != null && text != null)
					{
						MigrateMultipliersForVersionChange(jObject, text, newVersion);
					}
					UpdateConfigBaseAnnotations();
					_fieldsDumped = true;
					if (jObject != null)
					{
						MelonLogger.Msg("[MIGRATE] Reloading config after migration...");
						LoadConfig();
					}
				}
				bool flag = InitOverrideManager();
				if (!flag)
				{
					MelonLogger.Warning("OverrideManager not available — falling back to direct mutation (no client sync)");
				}
				if (flag && _overrideManagerType != null)
				{
					OMRevertAll();
				}
				if (flag && _healthMultEnabled && _healthMultipliers.Count > 0)
				{
					RegisterDamageManagerDataInOM();
				}
				if (_vanillaBaseCache.Count == 0)
				{
					CacheVanillaBaseValues();
				}
				ApplyConstructionDataOverrides(flag);
				if (_healthMultEnabled)
				{
					ApplyHealthOverrides(flag);
					ReclampLiveHealth();
				}
				ApplyProjectileDamageOverrides(flag);
				ApplyProjectileTuningOverrides(flag);
				ApplyAimCompensationOverrides();
				ApplyTurretPitchOverrides();
				ApplyRangeOverrides(flag);
				ApplyTargetDistanceOverrides(flag);
				ApplyFoWDistanceOverrides(flag);
				ApplyJumpSpeedOverrides(flag);
				ApplyVisibleEventRadiusOverrides(flag);
				ApplyMoveSpeedOverrides(flag);
				ApplyStrafeSpeedOverrides(flag);
				ApplyTurnRadiusOverrides(flag);
				ApplyTeleportOverrides(flag);
				ApplyDispenserTimeoutOverrides(flag);
				ApplyProximityDetonationOverrides();
				ApplyUnitCapOverrides(flag);
				ApplyDepositRadiusOverrides(flag);
				ApplyExtractionRadiusOverrides(flag);
				if (_shrimpDisableAim)
				{
					ApplyShrimpAimDisable();
				}
				ApplyAlienNestOnlyBuild(flag);
				ApplyAlienQueensMax();
				ApplyDecayOverrides();
				_overridesApplied = true;
				MelonLogger.Msg($"[UnitBalance] Overrides applied (OverrideManager={flag}): " + $"{_damageMultipliers.Count} damage, {_healthMultipliers.Count} health, " + $"{_costMultipliers.Count} cost, {_buildTimeMultipliers.Count} buildTime, " + $"{_rangeMultipliers.Count} range, {_moveSpeedMultipliers.Count} moveSpeed, " + $"{_projectileOverrides.Count} projOverrides, {_proximityOverrides.Count} proximity, " + $"{_minTierOverrides.Count} minTier, {_techTierTimes.Count} techTime, {_techTierCosts.Count} techCost");
				FlushPrefabPool("after overrides applied");
			}
			catch (Exception ex3)
			{
				MelonLogger.Error("ApplyOverridesLogic error: " + ex3.Message);
			}
		}

		private static void OnGameStartedLogic()
		{
			try
			{
				if (_enabled && _configLoaded)
				{
					EnsureHQDecayWarningHook();
					if (!_overridesApplied)
					{
						ApplyOverridesLogic();
					}
					bool omInitialized = _omInitialized;
					try
					{
						PropagateToLiveInstances();
					}
					catch (Exception ex)
					{
						MelonLogger.Warning("[LIVE] Propagation error on game start: " + ex.Message);
					}
					ApplyDecayTeamSettings();
					MelonLogger.Msg("[UnitBalance] Game started — propagation complete");
					MelonCoroutines.Start(FowProbeAfter(30f, "t+30s"));
					MelonCoroutines.Start(FowProbeAfter(120f, "t+120s"));
					MelonCoroutines.Start(FowProbeAfter(300f, "t+300s"));
					if (_additionalSpawn)
					{
						MelonCoroutines.Start(SpawnAdditionalUnits(5f));
					}
					if (omInitialized)
					{
						MelonCoroutines.Start(SyncOverridesToAllPlayers(2f));
					}
				}
			}
			catch (Exception ex2)
			{
				MelonLogger.Error("GameStarted error: " + ex2.Message);
			}
		}

		private static IEnumerator FowProbeAfter(float delay, string tag)
		{
			yield return new WaitForSeconds(delay);
			ProbeLiveFoW(tag);
		}

		private static IEnumerator SyncOverridesToAllPlayers(float initialDelay)
		{
			if (initialDelay > 0f)
			{
				yield return new WaitForSeconds(initialDelay);
			}
			IList list = null;
			object obj = null;
			bool flag = false;
			try
			{
				Type typeFromHandle = typeof(Player);
				Type typeFromHandle2 = typeof(NetworkGameServer);
				if (_sendPlayerOverridesMethod == null)
				{
					MelonLogger.Warning("[OverrideSync] Cannot sync: missing Player type or SendPlayerOverrides method");
				}
				else
				{
					PropertyInfo property = typeFromHandle.GetProperty("Players", BindingFlags.Static | BindingFlags.Public);
					if (property != null)
					{
						list = property.GetValue(null) as IList;
					}
					else
					{
						FieldInfo field = typeFromHandle.GetField("Players", BindingFlags.Static | BindingFlags.Public);
						if (field != null)
						{
							list = field.GetValue(null) as IList;
						}
					}
					if (typeFromHandle2 != null)
					{
						obj = typeFromHandle2.GetMethod("GetServerPlayer", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
					}
					flag = list != null && list.Count > 0;
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[OverrideSync] Init error: " + ex.Message);
			}
			if (!flag || list == null)
			{
				MelonLogger.Warning("[OverrideSync] No players to sync to");
				yield break;
			}
			List<object> players = new List<object>();
			foreach (object item in list)
			{
				if (item != null && !item.Equals(obj))
				{
					players.Add(item);
				}
			}
			MelonLogger.Msg($"[OverrideSync] Syncing overrides to {players.Count} connected player(s)...");
			int synced = 0;
			for (int i = 0; i < players.Count; i++)
			{
				try
				{
					_sendPlayerOverridesMethod.Invoke(null, new object[1] { players[i] });
					synced++;
				}
				catch (Exception ex2)
				{
					MelonLogger.Warning($"[OverrideSync] Sync to player #{i} failed: {ex2.InnerException?.Message ?? ex2.Message}");
				}
				yield return new WaitForSeconds(0.5f);
			}
			MelonLogger.Msg($"[OverrideSync] Sync complete: {synced}/{players.Count} players");
		}

		private static IEnumerator SpawnAdditionalUnits(float delay)
		{
			if (delay > 0f)
			{
				yield return new WaitForSeconds(delay);
			}
			MelonLogger.Msg("[SPAWN] Additional spawn triggered");
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			Dictionary<string, ObjectInfo> dictionary = new Dictionary<string, ObjectInfo>(StringComparer.OrdinalIgnoreCase);
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (!(objectInfo == null) && !(objectInfo.Prefab == null))
				{
					string displayName = objectInfo.DisplayName;
					if (!string.IsNullOrEmpty(displayName) && !dictionary.ContainsKey(displayName))
					{
						dictionary[displayName] = objectInfo;
					}
				}
			}
			int num = 0;
			try
			{
				foreach (Team team in Team.Teams)
				{
					if (team == null || team.Structures == null || team.Structures.Count == 0)
					{
						continue;
					}
					string text = team.TeamShortName ?? team.name ?? "";
					string text2 = null;
					(string, string, int)[] additionalSpawnMap = _additionalSpawnMap;
					for (int i = 0; i < additionalSpawnMap.Length; i++)
					{
						(string, string, int) tuple = additionalSpawnMap[i];
						if (text.IndexOf(tuple.Item1, StringComparison.OrdinalIgnoreCase) >= 0)
						{
							(text2, _, _) = tuple;
							break;
						}
					}
					if (text2 == null)
					{
						continue;
					}
					Structure structure = team.Structures[0];
					if (structure == null)
					{
						continue;
					}
					Vector3 position = structure.transform.position;
					Vector3 forward = structure.transform.forward;
					List<(string, ObjectInfo)> list = new List<(string, ObjectInfo)>();
					additionalSpawnMap = _additionalSpawnMap;
					for (int i = 0; i < additionalSpawnMap.Length; i++)
					{
						(string, string, int) tuple3 = additionalSpawnMap[i];
						if (!string.Equals(tuple3.Item1, text2, StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}
						if (!dictionary.TryGetValue(tuple3.Item2, out var value))
						{
							MelonLogger.Warning("[SPAWN] ObjectInfo not found for '" + tuple3.Item2 + "'");
							continue;
						}
						for (int j = 0; j < tuple3.Item3; j++)
						{
							list.Add((tuple3.Item2, value));
						}
					}
					float num2 = 120f;
					int count = list.Count;
					for (int k = 0; k < count; k++)
					{
						try
						{
							float y = 360f / (float)count * (float)k;
							Vector3 vector = Quaternion.Euler(0f, y, 0f) * forward;
							Vector3 vector2 = position + vector * num2;
							float num3 = vector2.y;
							Terrain[] activeTerrains = Terrain.activeTerrains;
							foreach (Terrain terrain in activeTerrains)
							{
								if (!(terrain == null))
								{
									Vector3 position2 = terrain.transform.position;
									TerrainData terrainData = terrain.terrainData;
									if (!(terrainData == null) && vector2.x >= position2.x && vector2.x <= position2.x + terrainData.size.x && vector2.z >= position2.z && vector2.z <= position2.z + terrainData.size.z)
									{
										num3 = terrain.SampleHeight(vector2) + position2.y;
										break;
									}
								}
							}
							vector2.y = num3 + 1f;
							Quaternion rotation = Quaternion.LookRotation(vector, Vector3.up);
							Game.SpawnPrefab(list[k].Item2.Prefab, null, team, vector2, rotation);
							num++;
						}
						catch (Exception ex)
						{
							MelonLogger.Warning("[SPAWN] Failed to spawn '" + list[k].Item1 + "' for " + text + ": " + ex.Message);
						}
					}
					MelonLogger.Msg($"[SPAWN] Spawned {count} units for {text}");
				}
			}
			finally
			{
				MelonLogger.Msg($"[SPAWN] Additional spawn complete: {num} units (no global damage disable)");
			}
		}

		private static IEnumerator RoundTransitionWatchdog(int generation)
		{
			yield return new WaitForSeconds(20f);
			if (generation != _watchdogGeneration)
			{
				yield break;
			}
			float remaining = 60f;
			bool announced = false;
			while (true)
			{
				if (GameLevelLoader.IsLoading)
				{
					MelonLogger.Msg("[Watchdog] Level load in progress (" + GameLevelLoader.CurrentLoadingPath + ") — countdown paused");
					while (GameLevelLoader.IsLoading)
					{
						yield return new WaitForSeconds(1f);
						if (generation != _watchdogGeneration)
						{
							yield break;
						}
					}
					remaining = 60f;
					MelonLogger.Msg($"[Watchdog] Level load finished — countdown restarted ({remaining}s)");
				}
				if (remaining <= 0f)
				{
					break;
				}
				if (!announced)
				{
					announced = true;
					BroadcastChat($"{_chatPrefix}<color=#FFAA00>[Watchdog] Round transition stalled — auto-recovery in {remaining:0}s</color>");
					MelonLogger.Warning($"[Watchdog] Round transition stalled — countdown started ({remaining}s)");
				}
				else
				{
					BroadcastChat($"{_chatPrefix}<color=#FFAA00>[Watchdog] Auto-recovery in {remaining:0}s...</color>");
				}
				float wait = ((remaining > 10f) ? 10f : remaining);
				yield return new WaitForSeconds(wait);
				remaining -= wait;
				if (generation != _watchdogGeneration)
				{
					BroadcastChat(_chatPrefix + "<color=#55FF55>[Watchdog] New round detected — watchdog cancelled</color>");
					MelonLogger.Msg("[Watchdog] New round started — watchdog cancelled");
					yield break;
				}
			}
			_watchdogFired = true;
			BroadcastChat(_chatPrefix + "<color=#FF5555>[Watchdog] Forcing round end — server recovery</color>");
			MelonLogger.Warning($"[Watchdog] No new game started within {80f}s — forcing EndRound");
			try
			{
				Type type = null;
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (int i = 0; i < assemblies.Length; i++)
				{
					type = assemblies[i].GetType("GameMode");
					if (type != null)
					{
						break;
					}
				}
				if (!(type != null))
				{
					yield break;
				}
				object obj = type.GetProperty("CurrentGameMode", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
				if (obj != null)
				{
					MethodInfo method = obj.GetType().GetMethod("EndRound", BindingFlags.Instance | BindingFlags.Public);
					if (method != null)
					{
						method.Invoke(obj, null);
						MelonLogger.Msg("[Watchdog] EndRound() called — server should recover");
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Error("[Watchdog] Recovery failed: " + ex.Message);
			}
		}

		private static void BroadcastChat(string message)
		{
			try
			{
				Type typeFromHandle = typeof(Player);
				IList list = null;
				PropertyInfo property = typeFromHandle.GetProperty("Players", BindingFlags.Static | BindingFlags.Public);
				if (property != null)
				{
					list = property.GetValue(null) as IList;
				}
				else
				{
					FieldInfo field = typeFromHandle.GetField("Players", BindingFlags.Static | BindingFlags.Public);
					if (field != null)
					{
						list = field.GetValue(null) as IList;
					}
				}
				if (list == null)
				{
					return;
				}
				object obj = null;
				obj = typeof(NetworkGameServer).GetMethod("GetServerPlayer", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
				foreach (object item in list)
				{
					if (item != null && !item.Equals(obj))
					{
						SendChatToPlayer(item, message);
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[Watchdog] BroadcastChat error: " + ex.Message);
			}
		}

		private static void PropagateToLiveInstances()
		{
			int liveCount = 0;
			int totalUpdated = 0;
			HashSet<int> processedGOs = new HashSet<int>();
			Unit[] array = UnityEngine.Object.FindObjectsOfType<Unit>();
			Structure[] array2 = UnityEngine.Object.FindObjectsOfType<Structure>();
			Unit[] array3 = array;
			foreach (Unit unit in array3)
			{
				if (!(unit == null) && !(unit.ObjectInfo == null) && !(unit.ObjectInfo.Prefab == null))
				{
					PropagateToInstance(unit.gameObject, unit.ObjectInfo, processedGOs, ref liveCount, ref totalUpdated);
				}
			}
			Structure[] array4 = array2;
			foreach (Structure structure in array4)
			{
				if (!(structure == null) && !(structure.ObjectInfo == null) && !(structure.ObjectInfo.Prefab == null))
				{
					PropagateToInstance(structure.gameObject, structure.ObjectInfo, processedGOs, ref liveCount, ref totalUpdated);
				}
			}
			MelonLogger.Msg($"[LIVE] Propagated overrides to {totalUpdated}/{liveCount} live instances (units={array.Length}, structures={array2.Length})");
		}

		private static void PropagateToInstance(GameObject liveGO, ObjectInfo info, HashSet<int> processedGOs, ref int liveCount, ref int totalUpdated)
		{
			int instanceID = liveGO.GetInstanceID();
			if (processedGOs.Contains(instanceID))
			{
				return;
			}
			processedGOs.Add(instanceID);
			liveCount++;
			string text = ResolveConfigName(info.DisplayName, info.name);
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			bool flag = HasAnyWeaponMult(_rangeMultipliers, text);
			bool flag2 = HasAnyWeaponMult(_reloadTimeMultipliers, text);
			bool flag3 = HasAnyWeaponMult(_accuracyMultipliers, text);
			bool flag4 = HasAnyWeaponMult(_magazineMultipliers, text);
			bool flag5 = HasAnyWeaponMult(_fireRateMultipliers, text);
			bool flag6 = _moveSpeedMultipliers.TryGetValue(text, out var value);
			bool flag7 = _turboSpeedMultipliers.TryGetValue(text, out var value2);
			bool flag8 = _strafeSpeedMultipliers.TryGetValue(text, out var value3);
			bool flag9 = _flySpeedMultipliers.TryGetValue(text, out var value4);
			bool flag10 = _runSpeedMultipliers.TryGetValue(text, out var value5);
			bool flag11 = _sprintSpeedMultipliers.TryGetValue(text, out var value6);
			bool flag12 = _turnRadiusMultipliers.TryGetValue(text, out var value7);
			bool flag13 = _targetDistanceOverrides.TryGetValue(text, out var value8);
			bool flag14 = _fowDistanceOverrides.TryGetValue(text, out var value9);
			bool flag15 = _jumpSpeedMultipliers.TryGetValue(text, out var value10);
			if (!flag && !flag2 && !flag3 && !flag4 && !flag5 && !flag6 && !flag7 && !flag9 && !flag10 && !flag11 && !flag8 && !flag12 && !flag13 && !flag14 && !flag15)
			{
				return;
			}
			Component[] componentsInChildren = info.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
			Dictionary<string, Component> dictionary = new Dictionary<string, Component>();
			Component[] array = componentsInChildren;
			foreach (Component component in array)
			{
				if (!(component == null))
				{
					string name = component.GetType().Name;
					if (!dictionary.ContainsKey(name))
					{
						dictionary[name] = component;
					}
				}
			}
			Component[] componentsInChildren2 = liveGO.GetComponentsInChildren<Component>(includeInactive: true);
			int num = 0;
			array = componentsInChildren2;
			foreach (Component component2 in array)
			{
				if (component2 == null)
				{
					continue;
				}
				string name2 = component2.GetType().Name;
				Type type = component2.GetType();
				dictionary.TryGetValue(name2, out var value11);
				if (name2 == "VehicleTurret" && value11 != null)
				{
					if (flag)
					{
						float weaponMult = GetWeaponMult(_rangeMultipliers, text, "pri");
						if (Math.Abs(weaponMult - 1f) > 0.001f)
						{
							num += LiveScaleFieldLog(value11, component2, "AimDistance", weaponMult, text);
						}
					}
					string[] array2 = new string[2] { "Primary", "Secondary" };
					foreach (string text2 in array2)
					{
						string weapon = ((text2 == "Primary") ? "pri" : "sec");
						if (flag2)
						{
							float weaponMult2 = GetWeaponMult(_reloadTimeMultipliers, text, weapon);
							if (Math.Abs(weaponMult2 - 1f) > 0.001f)
							{
								num += LiveScaleField(value11, component2, text2 + "ReloadTime", weaponMult2, 0.1f);
							}
						}
						if (flag5)
						{
							float weaponMult3 = GetWeaponMult(_fireRateMultipliers, text, weapon);
							if (Math.Abs(weaponMult3 - 1f) > 0.001f)
							{
								num += LiveScaleField(value11, component2, text2 + "FireInterval", 1f / weaponMult3, 0.01f);
							}
						}
						if (flag4)
						{
							float weaponMult4 = GetWeaponMult(_magazineMultipliers, text, weapon);
							if (Math.Abs(weaponMult4 - 1f) > 0.001f)
							{
								num += LiveScaleIntField(value11, component2, text2 + "MagazineSize", weaponMult4);
							}
						}
						if (flag3)
						{
							float weaponMult5 = GetWeaponMult(_accuracyMultipliers, text, weapon);
							if (Math.Abs(weaponMult5 - 1f) > 0.001f)
							{
								num += LiveScaleField(value11, component2, text2 + "MuzzleSpread", weaponMult5);
							}
						}
					}
				}
				if ((name2 == "UnitAimAt" && value11 != null) & flag)
				{
					float weaponMult6 = GetWeaponMult(_rangeMultipliers, text, "pri");
					if (Math.Abs(weaponMult6 - 1f) > 0.001f)
					{
						num += LiveScaleFieldLog(value11, component2, "AimDistanceMax", weaponMult6, text);
					}
				}
				if (name2 == "Sensor")
				{
					if (flag13)
					{
						num += LiveSetAbsolute(component2, "TargetingDistance", value8);
					}
					if (flag14)
					{
						FieldInfo field = component2.GetType().GetField("FogOfWarViewDistance", BindingFlags.Instance | BindingFlags.Public);
						float num2 = ((field != null) ? ((float)field.GetValue(component2)) : (-1f));
						int num3 = LiveSetAbsolute(component2, "FogOfWarViewDistance", value9);
						float num4 = ((field != null) ? ((float)field.GetValue(component2)) : (-1f));
						MelonLogger.Msg($"[LIVE-FOW] {text}: {num2} -> {num4} (target={value9}, ok={num3})");
						num += num3;
					}
				}
				switch (name2)
				{
				case "Soldier":
				case "PlayerMovement":
				case "FPSMovement":
					if (flag15 && value11 != null)
					{
						num += LiveScaleFieldLog(value11, component2, "JumpSpeed", value10, text);
					}
					break;
				}
				if ((name2 == "VehicleWheeled" && value11 != null) & flag12)
				{
					num += LiveScaleFieldLog(value11, component2, "TurningCircleRadius", value7, text);
				}
				if (name2 == "CreatureDecapod" && value11 != null)
				{
					string[] array2 = new string[2] { "AttackPrimary", "AttackSecondary" };
					foreach (string text3 in array2)
					{
						string weapon2 = ((text3 == "AttackPrimary") ? "pri" : "sec");
						FieldInfo field2 = type.GetField(text3, BindingFlags.Instance | BindingFlags.Public);
						if (field2 == null)
						{
							continue;
						}
						object obj = null;
						object obj2 = null;
						try
						{
							obj = field2.GetValue(component2);
						}
						catch
						{
						}
						try
						{
							obj2 = field2.GetValue(value11);
						}
						catch
						{
						}
						if (obj == null || obj2 == null)
						{
							continue;
						}
						Type type2 = obj.GetType();
						if (Math.Abs(GetWeaponMult(_rangeMultipliers, text, weapon2) - 1f) > 0.001f)
						{
							FieldInfo field3 = type2.GetField("AttackProjectileAimDistMax", BindingFlags.Instance | BindingFlags.Public);
							if (field3 != null)
							{
								field3.SetValue(obj, field3.GetValue(obj2));
								num++;
							}
						}
						if (Math.Abs(GetWeaponMult(_accuracyMultipliers, text, weapon2) - 1f) > 0.001f)
						{
							FieldInfo field4 = type2.GetField("AttackProjectileSpread", BindingFlags.Instance | BindingFlags.Public);
							if (field4 != null)
							{
								field4.SetValue(obj, field4.GetValue(obj2));
								num++;
							}
						}
					}
				}
				if ((flag6 | flag7 | flag9 | flag10 | flag11) && value11 != null)
				{
					string[] array2 = _speedFieldNames;
					foreach (string text4 in array2)
					{
						float multiplier;
						switch (text4)
						{
						case "FlyMoveSpeed":
							if (flag9)
							{
								multiplier = value4;
								break;
							}
							if (!flag6)
							{
								continue;
							}
							multiplier = value;
							break;
						case "TurboSpeed":
							if (flag7)
							{
								multiplier = value2;
								break;
							}
							if (!flag6)
							{
								continue;
							}
							multiplier = value;
							break;
						case "RunSpeed":
							if (flag10)
							{
								multiplier = value5;
								break;
							}
							if (!flag6)
							{
								continue;
							}
							multiplier = value;
							break;
						case "SprintSpeed":
							if (flag11)
							{
								multiplier = value6;
								break;
							}
							if (!flag6)
							{
								continue;
							}
							multiplier = value;
							break;
						default:
							if (!flag6)
							{
								continue;
							}
							multiplier = value;
							break;
						}
						num += LiveScaleFieldDeclaredOnlyLog(value11, component2, text4, multiplier, text);
					}
				}
				if (flag8 && value11 != null)
				{
					if (name2 == "CreatureDecapod")
					{
						num += LiveScaleField(value11, component2, "FlyMoveScaleSide", value3);
					}
					else if (name2 == "VehicleAir")
					{
						num += LiveScaleFieldDeclaredOnly(value11, component2, "StrafeSpeed", value3);
					}
				}
			}
			if (num > 0)
			{
				totalUpdated++;
			}
		}

		private static int LiveScaleField(Component prefab, Component live, string fieldName, float multiplier, float minVal = 0f)
		{
			try
			{
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				FieldInfo field = prefab.GetType().GetField(fieldName, bindingAttr);
				if (field == null || field.FieldType != typeof(float))
				{
					return 0;
				}
				FieldInfo field2 = live.GetType().GetField(fieldName, bindingAttr);
				if (field2 == null)
				{
					return 0;
				}
				float num = (float)field.GetValue(prefab) * multiplier;
				if (minVal > 0f && num < minVal)
				{
					num = minVal;
				}
				field2.SetValue(live, num);
				return 1;
			}
			catch
			{
				return 0;
			}
		}

		private static int LiveScaleFieldDeclaredOnly(Component prefab, Component live, string fieldName, float multiplier)
		{
			try
			{
				BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;
				FieldInfo field = prefab.GetType().GetField(fieldName, bindingAttr);
				if (field != null && field.FieldType == typeof(float))
				{
					FieldInfo field2 = live.GetType().GetField(fieldName, bindingAttr);
					if (field2 == null)
					{
						return 0;
					}
					float num = (float)field.GetValue(prefab);
					if (num <= 0f)
					{
						return 0;
					}
					field2.SetValue(live, num * multiplier);
				}
				else
				{
					PropertyInfo property = prefab.GetType().GetProperty(fieldName, bindingAttr);
					if (property == null || property.PropertyType != typeof(float))
					{
						return 0;
					}
					PropertyInfo property2 = live.GetType().GetProperty(fieldName, bindingAttr);
					if (property2 == null || !property2.CanWrite)
					{
						return 0;
					}
					float num = (float)property.GetValue(prefab);
					if (num <= 0f)
					{
						return 0;
					}
					property2.SetValue(live, num * multiplier);
				}
				return 1;
			}
			catch
			{
				return 0;
			}
		}

		private static int LiveScaleIntField(Component prefab, Component live, string fieldName, float multiplier)
		{
			try
			{
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				FieldInfo field = prefab.GetType().GetField(fieldName, bindingAttr);
				if (field == null || field.FieldType != typeof(int))
				{
					return 0;
				}
				FieldInfo field2 = live.GetType().GetField(fieldName, bindingAttr);
				if (field2 == null)
				{
					return 0;
				}
				int num = (int)field.GetValue(prefab);
				int num2 = Math.Max(1, (int)Math.Round((float)num * multiplier));
				field2.SetValue(live, num2);
				return 1;
			}
			catch
			{
				return 0;
			}
		}

		private static int LiveScaleFieldLog(Component prefab, Component live, string fieldName, float multiplier, string unitName)
		{
			try
			{
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				FieldInfo field = prefab.GetType().GetField(fieldName, bindingAttr);
				if (field == null || field.FieldType != typeof(float))
				{
					return 0;
				}
				FieldInfo field2 = live.GetType().GetField(fieldName, bindingAttr);
				if (field2 == null)
				{
					return 0;
				}
				float num = (float)field.GetValue(prefab);
				float num2 = (float)field2.GetValue(live);
				float num3 = num * multiplier;
				field2.SetValue(live, num3);
				float num4 = (float)field2.GetValue(live);
				LogDebug($"[LIVE-DBG] {unitName}: {fieldName} prefab={num:F2} old={num2:F2} -> new={num3:F2} verify={num4:F2}");
				return 1;
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[LIVE-DBG] " + unitName + ": " + fieldName + " error: " + ex.Message);
				return 0;
			}
		}

		private static int LiveScaleFieldDeclaredOnlyLog(Component prefab, Component live, string fieldName, float multiplier, string unitName)
		{
			try
			{
				BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;
				FieldInfo field = prefab.GetType().GetField(fieldName, bindingAttr);
				float num;
				float num2;
				float num3;
				float num4;
				if (field != null && field.FieldType == typeof(float))
				{
					FieldInfo field2 = live.GetType().GetField(fieldName, bindingAttr);
					if (field2 == null)
					{
						return 0;
					}
					num = (float)field.GetValue(prefab);
					if (num <= 0f)
					{
						return 0;
					}
					num2 = (float)field2.GetValue(live);
					num3 = num * multiplier;
					field2.SetValue(live, num3);
					num4 = (float)field2.GetValue(live);
				}
				else
				{
					PropertyInfo property = prefab.GetType().GetProperty(fieldName, bindingAttr);
					if (property == null || property.PropertyType != typeof(float))
					{
						return 0;
					}
					PropertyInfo property2 = live.GetType().GetProperty(fieldName, bindingAttr);
					if (property2 == null || !property2.CanWrite)
					{
						return 0;
					}
					num = (float)property.GetValue(prefab);
					if (num <= 0f)
					{
						return 0;
					}
					num2 = (float)property2.GetValue(live);
					num3 = num * multiplier;
					property2.SetValue(live, num3);
					num4 = (float)property2.GetValue(live);
				}
				LogDebug($"[LIVE-DBG] {unitName}: {fieldName} prefab={num:F2} old={num2:F2} -> new={num3:F2} verify={num4:F2}");
				return 1;
			}
			catch
			{
				return 0;
			}
		}

		private static int LiveSetAbsolute(Component live, string fieldName, float value)
		{
			try
			{
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				FieldInfo field = live.GetType().GetField(fieldName, bindingAttr);
				if (field == null || field.FieldType != typeof(float))
				{
					return 0;
				}
				field.SetValue(live, value);
				return 1;
			}
			catch
			{
				return 0;
			}
		}

		private static string GetCachedName(GameObject obj)
		{
			if (obj == null)
			{
				return null;
			}
			int instanceID = obj.GetInstanceID();
			if (_nameCache.TryGetValue(instanceID, out string value))
			{
				return value;
			}
			string text = null;
			try
			{
				Unit component = obj.GetComponent<Unit>();
				if (component?.ObjectInfo != null)
				{
					text = component.ObjectInfo.DisplayName;
				}
			}
			catch
			{
			}
			if (text == null)
			{
				try
				{
					Structure component2 = obj.GetComponent<Structure>();
					if (component2?.ObjectInfo != null)
					{
						text = component2.ObjectInfo.DisplayName;
					}
				}
				catch
				{
				}
			}
			if (text != null)
			{
				_nameCache[instanceID] = text;
			}
			return text;
		}

		private static void DumpAllUnitsJson()
		{
			try
			{
				string text = Path.Combine(Path.GetDirectoryName(_configPath), "Si_UnitBalance_Dump.json");
				ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
				LogDebug($"[JSON DUMP] Found {array.Length} ObjectInfo entries, writing to {text}");
				Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
				ObjectInfo[] array2 = array;
				foreach (ObjectInfo objectInfo in array2)
				{
					if (objectInfo == null || objectInfo.Prefab == null)
					{
						continue;
					}
					Structure component = objectInfo.Prefab.GetComponent<Structure>();
					if (component == null || component.ConstructionOptions == null)
					{
						continue;
					}
					string displayName = objectInfo.DisplayName;
					if (string.IsNullOrEmpty(displayName) || (objectInfo.name != null && (objectInfo.name.Contains("_TD") || objectInfo.name.Contains("_Intro") || objectInfo.name.Contains("_Tutorial"))))
					{
						continue;
					}
					try
					{
						Team defaultTeam = GetDefaultTeam(component);
						if (defaultTeam != null)
						{
							_ = defaultTeam.name;
						}
					}
					catch
					{
					}
					foreach (ConstructionData constructionOption in component.ConstructionOptions)
					{
						if (constructionOption == null || constructionOption.ObjectInfo == null)
						{
							continue;
						}
						string displayName2 = constructionOption.ObjectInfo.DisplayName;
						if (!string.IsNullOrEmpty(displayName2))
						{
							if (!dictionary.ContainsKey(displayName))
							{
								dictionary[displayName] = new List<string>();
							}
							if (!dictionary[displayName].Contains(displayName2))
							{
								dictionary[displayName].Add(displayName2);
							}
							if (!dictionary2.ContainsKey(displayName2))
							{
								dictionary2[displayName2] = displayName;
							}
						}
					}
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("{");
				stringBuilder.AppendLine("  \"units\": [");
				HashSet<string> hashSet = new HashSet<string>();
				bool flag = true;
				array2 = array;
				foreach (ObjectInfo objectInfo2 in array2)
				{
					if (objectInfo2 == null)
					{
						continue;
					}
					string text2 = ResolveConfigName(objectInfo2.DisplayName, objectInfo2.name);
					if (string.IsNullOrEmpty(text2))
					{
						continue;
					}
					string text3 = objectInfo2.name ?? "";
					if (text3.Contains("_TD") || text3.Contains("_Intro") || text3.Contains("_Tutorial") || hashSet.Contains(text3) || objectInfo2.ConstructionData == null)
					{
						continue;
					}
					hashSet.Add(text3);
					ConstructionData constructionData = objectInfo2.ConstructionData;
					int resourceCost = constructionData.ResourceCost;
					float buildUpTime = constructionData.BuildUpTime;
					float num = 0f;
					float num2 = 0f;
					try
					{
						Type type = constructionData.GetType();
						FieldInfo field = type.GetField("FinishedWaitTime", BindingFlags.Instance | BindingFlags.Public);
						FieldInfo field2 = type.GetField("CleanUpTime", BindingFlags.Instance | BindingFlags.Public);
						if (field != null)
						{
							num = (float)field.GetValue(constructionData);
						}
						if (field2 != null)
						{
							num2 = (float)field2.GetValue(constructionData);
						}
					}
					catch
					{
					}
					int minimumTeamTier = constructionData.MinimumTeamTier;
					int maximumTeamTier = constructionData.MaximumTeamTier;
					float maximumBaseStructureDistance = constructionData.MaximumBaseStructureDistance;
					bool maximumDistanceUseRadius = constructionData.MaximumDistanceUseRadius;
					string text4 = "?";
					string text5 = "Unknown";
					int num3 = 0;
					if (objectInfo2.Prefab != null)
					{
						DamageManager component2 = objectInfo2.Prefab.GetComponent<DamageManager>();
						if (component2 != null)
						{
							try
							{
								num3 = (int)component2.MaxHealth;
							}
							catch
							{
							}
						}
					}
					if (objectInfo2.Prefab != null)
					{
						Structure component3 = objectInfo2.Prefab.GetComponent<Structure>();
						if (component3 != null)
						{
							try
							{
								Team defaultTeam2 = GetDefaultTeam(component3);
								if (defaultTeam2 != null)
								{
									text4 = defaultTeam2.name;
								}
							}
							catch
							{
							}
						}
						Unit component4 = objectInfo2.Prefab.GetComponent<Unit>();
						if (component4 != null)
						{
							try
							{
								Team defaultTeam3 = GetDefaultTeam(component4);
								if (defaultTeam3 != null)
								{
									text4 = defaultTeam3.name;
								}
							}
							catch
							{
							}
						}
					}
					if (text4.Contains("Sol"))
					{
						text5 = "Sol";
					}
					else if (text4.Contains("Centauri"))
					{
						text5 = "Centauri";
					}
					else if (text4.Contains("Alien"))
					{
						text5 = "Alien";
					}
					bool flag2 = false;
					if (objectInfo2.Prefab != null)
					{
						flag2 = objectInfo2.Prefab.GetComponent<Structure>() != null;
					}
					string s = (dictionary2.ContainsKey(text2) ? dictionary2[text2] : "");
					float num4 = 0f;
					float num5 = 0f;
					float num6 = -1f;
					float num7 = -1f;
					float num8 = 0f;
					float num9 = 0f;
					float num10 = 0f;
					float num11 = 0f;
					float num12 = 0f;
					float num13 = 0f;
					string text6 = "";
					float num14 = 0f;
					float num15 = 0f;
					float num16 = 0f;
					float num17 = 0f;
					float num18 = 0f;
					float num19 = 0f;
					float num20 = 0f;
					float num21 = 0f;
					string s2 = "";
					float num22 = 0f;
					float num23 = 0f;
					float num24 = 0f;
					float num25 = 0f;
					string s3 = "";
					float num26 = 0f;
					float num27 = 0f;
					bool flag3 = false;
					float num28 = 0f;
					float num29 = 0f;
					bool flag4 = false;
					float num30 = 0f;
					float num31 = 0f;
					int num32 = 0;
					float num33 = 0f;
					float num34 = 0f;
					int num35 = 0;
					int num36 = 0;
					float num37 = 0f;
					float num38 = 0f;
					float num39 = 0f;
					float num40 = 0f;
					int num41 = 0;
					int num42 = 0;
					string s4 = "";
					string s5 = "";
					float num43 = 0f;
					float num44 = 0f;
					float num45 = 0f;
					float num46 = 0f;
					bool flag5 = false;
					bool flag6 = false;
					bool flag7 = false;
					float num47 = 0f;
					float num48 = 0f;
					float num49 = 0f;
					float num50 = 0f;
					float num51 = 0f;
					float num52 = 0f;
					bool flag8 = false;
					bool flag9 = false;
					bool flag10 = false;
					float num53 = 0f;
					float num54 = 0f;
					float num55 = 0f;
					float num56 = 0f;
					int num57 = 0;
					int num58 = 0;
					float num59 = 0f;
					string s6 = "";
					float num60 = 0f;
					float num61 = 0f;
					float num62 = 0f;
					float num63 = 0f;
					bool flag11 = false;
					bool flag12 = false;
					bool flag13 = false;
					float num64 = 0f;
					float num65 = 0f;
					float num66 = 0f;
					float num67 = 0f;
					float num68 = 0f;
					float num69 = 0f;
					float num70 = 0f;
					float num71 = 0f;
					string text7 = "";
					int num72 = 0;
					float num73 = 0f;
					float num74 = 0f;
					float num75 = 0f;
					float num76 = 0f;
					float num77 = 0f;
					float num78 = 0f;
					float num79 = 0f;
					bool flag14 = false;
					bool flag15 = false;
					float num80 = 0f;
					float num81 = 0f;
					float num82 = 0f;
					bool flag16 = false;
					if (objectInfo2.Prefab != null)
					{
						Component[] componentsInChildren = objectInfo2.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
						foreach (Component component5 in componentsInChildren)
						{
							if (component5 == null)
							{
								continue;
							}
							Type type2 = component5.GetType();
							string name = type2.Name;
							if (name == "CreatureDecapod")
							{
								try
								{
									FieldInfo field3 = type2.GetField("MoveSpeed", BindingFlags.Instance | BindingFlags.Public);
									if (field3 != null)
									{
										num4 = (float)field3.GetValue(component5);
									}
								}
								catch
								{
								}
								try
								{
									FieldInfo field4 = type2.GetField("FlyMoveSpeed", BindingFlags.Instance | BindingFlags.Public);
									if (field4 != null)
									{
										num5 = (float)field4.GetValue(component5);
									}
								}
								catch
								{
								}
								try
								{
									FieldInfo field5 = type2.GetField("FlyMoveScaleSide", BindingFlags.Instance | BindingFlags.Public);
									if (field5 != null)
									{
										num6 = (float)field5.GetValue(component5);
									}
								}
								catch
								{
								}
								try
								{
									FieldInfo field6 = type2.GetField("FlyMoveScaleBack", BindingFlags.Instance | BindingFlags.Public);
									if (field6 != null)
									{
										num7 = (float)field6.GetValue(component5);
									}
								}
								catch
								{
								}
								try
								{
									object obj10 = null;
									FieldInfo field7 = type2.GetField("AttackPrimary", BindingFlags.Instance | BindingFlags.Public);
									if (field7 != null)
									{
										obj10 = field7.GetValue(component5);
									}
									else
									{
										PropertyInfo property = type2.GetProperty("AttackPrimary", BindingFlags.Instance | BindingFlags.Public);
										if (property != null)
										{
											obj10 = property.GetValue(component5);
										}
									}
									if (obj10 != null)
									{
										Type type3 = obj10.GetType();
										try
										{
											num18 = GetFloatField(type3, obj10, "Damage");
										}
										catch
										{
										}
										try
										{
											num19 = GetFloatField(type3, obj10, "CoolDownTime");
										}
										catch
										{
										}
										try
										{
											num20 = GetFloatField(type3, obj10, "AttackProjectileAimDistMax");
										}
										catch
										{
										}
										try
										{
											num21 = GetFloatField(type3, obj10, "AttackProjectileSpread");
										}
										catch
										{
										}
										try
										{
											object obj15 = null;
											FieldInfo field8 = type3.GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
											if (field8 != null)
											{
												obj15 = field8.GetValue(obj10);
											}
											else
											{
												PropertyInfo property2 = type3.GetProperty("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
												if (property2 != null)
												{
													obj15 = property2.GetValue(obj10);
												}
											}
											if (obj15 != null)
											{
												s2 = (obj15 as UnityEngine.Object)?.name ?? "";
												Type type4 = obj15.GetType();
												try
												{
													num26 = GetFloatField(type4, obj15, "m_fBaseSpeed");
												}
												catch
												{
												}
												try
												{
													num27 = GetFloatField(type4, obj15, "m_fLifeTime");
												}
												catch
												{
												}
												try
												{
													FieldInfo field9 = type4.GetField("m_InstantHit", BindingFlags.Instance | BindingFlags.Public);
													if (field9 != null)
													{
														flag3 = (bool)field9.GetValue(obj15);
													}
													else
													{
														PropertyInfo property3 = type4.GetProperty("m_InstantHit", BindingFlags.Instance | BindingFlags.Public);
														if (property3 != null)
														{
															flag3 = (bool)property3.GetValue(obj15);
														}
													}
												}
												catch
												{
												}
												try
												{
													num66 = GetFloatField(type4, obj15, "m_fImpactDamage");
												}
												catch
												{
												}
												try
												{
													num67 = GetFloatField(type4, obj15, "m_fRicochetDamage");
												}
												catch
												{
												}
												try
												{
													num68 = GetFloatField(type4, obj15, "m_fSplashDamageMax");
												}
												catch
												{
												}
											}
										}
										catch
										{
										}
									}
								}
								catch
								{
								}
								try
								{
									object obj24 = null;
									FieldInfo field10 = type2.GetField("AttackSecondary", BindingFlags.Instance | BindingFlags.Public);
									if (field10 != null)
									{
										obj24 = field10.GetValue(component5);
									}
									else
									{
										PropertyInfo property4 = type2.GetProperty("AttackSecondary", BindingFlags.Instance | BindingFlags.Public);
										if (property4 != null)
										{
											obj24 = property4.GetValue(component5);
										}
									}
									if (obj24 != null)
									{
										Type type5 = obj24.GetType();
										try
										{
											num22 = GetFloatField(type5, obj24, "Damage");
										}
										catch
										{
										}
										try
										{
											num23 = GetFloatField(type5, obj24, "CoolDownTime");
										}
										catch
										{
										}
										try
										{
											num24 = GetFloatField(type5, obj24, "AttackProjectileAimDistMax");
										}
										catch
										{
										}
										try
										{
											num25 = GetFloatField(type5, obj24, "AttackProjectileSpread");
										}
										catch
										{
										}
										try
										{
											object obj29 = null;
											FieldInfo field11 = type5.GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
											if (field11 != null)
											{
												obj29 = field11.GetValue(obj24);
											}
											else
											{
												PropertyInfo property5 = type5.GetProperty("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
												if (property5 != null)
												{
													obj29 = property5.GetValue(obj24);
												}
											}
											if (obj29 != null)
											{
												s3 = (obj29 as UnityEngine.Object)?.name ?? "";
												Type type6 = obj29.GetType();
												try
												{
													num28 = GetFloatField(type6, obj29, "m_fBaseSpeed");
												}
												catch
												{
												}
												try
												{
													num29 = GetFloatField(type6, obj29, "m_fLifeTime");
												}
												catch
												{
												}
												try
												{
													FieldInfo field12 = type6.GetField("m_InstantHit", BindingFlags.Instance | BindingFlags.Public);
													if (field12 != null)
													{
														flag4 = (bool)field12.GetValue(obj29);
													}
													else
													{
														PropertyInfo property6 = type6.GetProperty("m_InstantHit", BindingFlags.Instance | BindingFlags.Public);
														if (property6 != null)
														{
															flag4 = (bool)property6.GetValue(obj29);
														}
													}
												}
												catch
												{
												}
												try
												{
													num69 = GetFloatField(type6, obj29, "m_fImpactDamage");
												}
												catch
												{
												}
												try
												{
													num70 = GetFloatField(type6, obj29, "m_fRicochetDamage");
												}
												catch
												{
												}
												try
												{
													num71 = GetFloatField(type6, obj29, "m_fSplashDamageMax");
												}
												catch
												{
												}
											}
										}
										catch
										{
										}
									}
								}
								catch
								{
								}
							}
							if (name == "Sensor")
							{
								try
								{
									FieldInfo field13 = type2.GetField("FogOfWarViewDistance", BindingFlags.Instance | BindingFlags.Public);
									if (field13 != null)
									{
										float num83 = (float)field13.GetValue(component5);
										if (num83 > num30)
										{
											num30 = num83;
										}
									}
								}
								catch
								{
								}
								try
								{
									FieldInfo field14 = type2.GetField("TargetingDistance", BindingFlags.Instance | BindingFlags.Public);
									if (field14 != null)
									{
										float num84 = (float)field14.GetValue(component5);
										if (num84 > num31)
										{
											num31 = num84;
										}
									}
								}
								catch
								{
								}
							}
							if (name == "VehicleTurret")
							{
								num32++;
								switch (num32)
								{
								case 1:
									try
									{
										FieldInfo field24 = type2.GetField("PrimaryFireInterval", BindingFlags.Instance | BindingFlags.Public);
										if (field24 != null)
										{
											num33 = (float)field24.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										FieldInfo field25 = type2.GetField("PrimaryMuzzleSpread", BindingFlags.Instance | BindingFlags.Public);
										if (field25 != null)
										{
											num34 = (float)field25.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										FieldInfo field26 = type2.GetField("PrimaryMagazineSize", BindingFlags.Instance | BindingFlags.Public);
										if (field26 != null)
										{
											num35 = (int)field26.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										FieldInfo field27 = type2.GetField("PrimaryReloadTime", BindingFlags.Instance | BindingFlags.Public);
										if (field27 != null)
										{
											num37 = (float)field27.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										FieldInfo field28 = type2.GetField("PrimaryShotCount", BindingFlags.Instance | BindingFlags.Public);
										if (field28 != null)
										{
											num36 = (int)field28.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										FieldInfo field29 = type2.GetField("SecondaryFireInterval", BindingFlags.Instance | BindingFlags.Public);
										if (field29 != null)
										{
											num38 = (float)field29.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										FieldInfo field30 = type2.GetField("SecondaryMuzzleSpread", BindingFlags.Instance | BindingFlags.Public);
										if (field30 != null)
										{
											num39 = (float)field30.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										FieldInfo field31 = type2.GetField("SecondaryMagazineSize", BindingFlags.Instance | BindingFlags.Public);
										if (field31 != null)
										{
											num41 = (int)field31.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										FieldInfo field32 = type2.GetField("SecondaryReloadTime", BindingFlags.Instance | BindingFlags.Public);
										if (field32 != null)
										{
											num40 = (float)field32.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										FieldInfo field33 = type2.GetField("SecondaryShotCount", BindingFlags.Instance | BindingFlags.Public);
										if (field33 != null)
										{
											num42 = (int)field33.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										object obj66 = null;
										FieldInfo field34 = type2.GetField("PrimaryProjectileData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
										if (field34 != null)
										{
											obj66 = field34.GetValue(component5);
										}
										if (obj66 != null)
										{
											s4 = (obj66 as UnityEngine.Object)?.name ?? "";
											Type type8 = obj66.GetType();
											try
											{
												num43 = GetFloatField(type8, obj66, "m_fImpactDamage");
											}
											catch
											{
											}
											try
											{
												num44 = GetFloatField(type8, obj66, "m_fRicochetDamage");
											}
											catch
											{
											}
											try
											{
												num45 = GetFloatField(type8, obj66, "m_fSplashDamageMax");
											}
											catch
											{
											}
											try
											{
												num46 = GetFloatField(type8, obj66, "m_fPenetratingDamage");
											}
											catch
											{
											}
											try
											{
												num47 = GetFloatField(type8, obj66, "m_fBaseSpeed");
											}
											catch
											{
											}
											try
											{
												num48 = GetFloatField(type8, obj66, "m_fLifeTime");
											}
											catch
											{
											}
											try
											{
												FieldInfo field35 = type8.GetField("m_bSplash", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
												if (field35 != null)
												{
													flag5 = (bool)field35.GetValue(obj66);
												}
											}
											catch
											{
											}
											try
											{
												FieldInfo field36 = type8.GetField("m_bPenetrating", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
												if (field36 != null)
												{
													flag6 = (bool)field36.GetValue(obj66);
												}
											}
											catch
											{
											}
											try
											{
												FieldInfo field37 = type8.GetField("m_InstantHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
												if (field37 != null)
												{
													flag7 = (bool)field37.GetValue(obj66);
												}
												else
												{
													PropertyInfo property8 = type8.GetProperty("m_InstantHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
													if (property8 != null)
													{
														flag7 = (bool)property8.GetValue(obj66);
													}
												}
											}
											catch
											{
											}
										}
									}
									catch
									{
									}
									try
									{
										object obj77 = null;
										FieldInfo field38 = type2.GetField("SecondaryProjectileData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
										if (field38 != null)
										{
											obj77 = field38.GetValue(component5);
										}
										if (obj77 == null)
										{
											break;
										}
										s5 = (obj77 as UnityEngine.Object)?.name ?? "";
										Type type9 = obj77.GetType();
										try
										{
											num49 = GetFloatField(type9, obj77, "m_fImpactDamage");
										}
										catch
										{
										}
										try
										{
											num50 = GetFloatField(type9, obj77, "m_fRicochetDamage");
										}
										catch
										{
										}
										try
										{
											num51 = GetFloatField(type9, obj77, "m_fSplashDamageMax");
										}
										catch
										{
										}
										try
										{
											num52 = GetFloatField(type9, obj77, "m_fPenetratingDamage");
										}
										catch
										{
										}
										try
										{
											num53 = GetFloatField(type9, obj77, "m_fBaseSpeed");
										}
										catch
										{
										}
										try
										{
											num54 = GetFloatField(type9, obj77, "m_fLifeTime");
										}
										catch
										{
										}
										try
										{
											FieldInfo field39 = type9.GetField("m_bSplash", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
											if (field39 != null)
											{
												flag8 = (bool)field39.GetValue(obj77);
											}
										}
										catch
										{
										}
										try
										{
											FieldInfo field40 = type9.GetField("m_bPenetrating", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
											if (field40 != null)
											{
												flag9 = (bool)field40.GetValue(obj77);
											}
										}
										catch
										{
										}
										try
										{
											FieldInfo field41 = type9.GetField("m_InstantHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
											if (field41 != null)
											{
												flag10 = (bool)field41.GetValue(obj77);
												break;
											}
											PropertyInfo property9 = type9.GetProperty("m_InstantHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
											if (property9 != null)
											{
												flag10 = (bool)property9.GetValue(obj77);
											}
										}
										catch
										{
										}
									}
									catch
									{
									}
									break;
								case 2:
									try
									{
										FieldInfo field15 = type2.GetField("PrimaryFireInterval", BindingFlags.Instance | BindingFlags.Public);
										if (field15 != null)
										{
											num55 = (float)field15.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										FieldInfo field16 = type2.GetField("PrimaryMuzzleSpread", BindingFlags.Instance | BindingFlags.Public);
										if (field16 != null)
										{
											num56 = (float)field16.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										FieldInfo field17 = type2.GetField("PrimaryMagazineSize", BindingFlags.Instance | BindingFlags.Public);
										if (field17 != null)
										{
											num57 = (int)field17.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										FieldInfo field18 = type2.GetField("PrimaryReloadTime", BindingFlags.Instance | BindingFlags.Public);
										if (field18 != null)
										{
											num59 = (float)field18.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										FieldInfo field19 = type2.GetField("PrimaryShotCount", BindingFlags.Instance | BindingFlags.Public);
										if (field19 != null)
										{
											num58 = (int)field19.GetValue(component5);
										}
									}
									catch
									{
									}
									try
									{
										object obj45 = null;
										FieldInfo field20 = type2.GetField("PrimaryProjectileData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
										if (field20 != null)
										{
											obj45 = field20.GetValue(component5);
										}
										if (obj45 == null)
										{
											break;
										}
										s6 = (obj45 as UnityEngine.Object)?.name ?? "";
										Type type7 = obj45.GetType();
										try
										{
											num60 = GetFloatField(type7, obj45, "m_fImpactDamage");
										}
										catch
										{
										}
										try
										{
											num61 = GetFloatField(type7, obj45, "m_fRicochetDamage");
										}
										catch
										{
										}
										try
										{
											num62 = GetFloatField(type7, obj45, "m_fSplashDamageMax");
										}
										catch
										{
										}
										try
										{
											num63 = GetFloatField(type7, obj45, "m_fPenetratingDamage");
										}
										catch
										{
										}
										try
										{
											num64 = GetFloatField(type7, obj45, "m_fBaseSpeed");
										}
										catch
										{
										}
										try
										{
											num65 = GetFloatField(type7, obj45, "m_fLifeTime");
										}
										catch
										{
										}
										try
										{
											FieldInfo field21 = type7.GetField("m_bSplash", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
											if (field21 != null)
											{
												flag11 = (bool)field21.GetValue(obj45);
											}
										}
										catch
										{
										}
										try
										{
											FieldInfo field22 = type7.GetField("m_bPenetrating", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
											if (field22 != null)
											{
												flag12 = (bool)field22.GetValue(obj45);
											}
										}
										catch
										{
										}
										try
										{
											FieldInfo field23 = type7.GetField("m_InstantHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
											if (field23 != null)
											{
												flag13 = (bool)field23.GetValue(obj45);
												break;
											}
											PropertyInfo property7 = type7.GetProperty("m_InstantHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
											if (property7 != null)
											{
												flag13 = (bool)property7.GetValue(obj45);
											}
										}
										catch
										{
										}
									}
									catch
									{
									}
									break;
								}
							}
							_ = name == "CreatureTurret";
							if (name == "PlayerMovement" || name == "FPSMovement")
							{
								try
								{
									FieldInfo field42 = type2.GetField("WalkSpeed", BindingFlags.Instance | BindingFlags.Public);
									if (field42 != null)
									{
										num8 = (float)field42.GetValue(component5);
									}
								}
								catch
								{
								}
								try
								{
									FieldInfo field43 = type2.GetField("RunSpeed", BindingFlags.Instance | BindingFlags.Public);
									if (field43 != null)
									{
										num9 = (float)field43.GetValue(component5);
									}
								}
								catch
								{
								}
								try
								{
									FieldInfo field44 = type2.GetField("JumpSpeed", BindingFlags.Instance | BindingFlags.Public);
									if (field44 != null)
									{
										num10 = (float)field44.GetValue(component5);
									}
								}
								catch
								{
								}
							}
							if (name == "Soldier" && string.IsNullOrEmpty(text7))
							{
								try
								{
									string text8 = objectInfo2.name ?? "";
									string text9 = (text8.StartsWith("ObjectInfo_") ? text8.Substring(11) : text8);
									List<CharacterAttachment> characterAttachmentDatas = CharacterAttachment.CharacterAttachmentDatas;
									if (characterAttachmentDatas != null)
									{
										foreach (CharacterAttachment item in characterAttachmentDatas)
										{
											if (item == null || item.AttachObject == null || !item.name.Contains(text9))
											{
												continue;
											}
											Component[] componentsInChildren2 = item.AttachObject.GetComponentsInChildren<Component>(includeInactive: true);
											foreach (Component component6 in componentsInChildren2)
											{
												if (component6 == null || component6.GetType().Name != "HumanHandsAnimator")
												{
													continue;
												}
												Type type10 = component6.GetType();
												try
												{
													num73 = GetFloatField(type10, component6, "FireDelay");
												}
												catch
												{
												}
												try
												{
													FieldInfo field45 = type10.GetField("MagazineSize", BindingFlags.Instance | BindingFlags.Public);
													if (field45 != null)
													{
														num72 = (int)field45.GetValue(component6);
													}
												}
												catch
												{
												}
												try
												{
													num74 = GetFloatField(type10, component6, "ReloadTime");
												}
												catch
												{
												}
												try
												{
													num75 = GetFloatField(type10, component6, "MuzzleSpreadMin");
												}
												catch
												{
												}
												try
												{
													num76 = GetFloatField(type10, component6, "MuzzleSpreadMax");
												}
												catch
												{
												}
												try
												{
													FieldInfo field46 = type10.GetField("BulletType", BindingFlags.Instance | BindingFlags.Public);
													if (field46 != null)
													{
														object value = field46.GetValue(component6);
														if (value != null)
														{
															text7 = (value as UnityEngine.Object)?.name ?? "";
															Type type11 = value.GetType();
															try
															{
																num77 = GetFloatField(type11, value, "m_fImpactDamage");
															}
															catch
															{
															}
															try
															{
																num78 = GetFloatField(type11, value, "m_fBaseSpeed");
															}
															catch
															{
															}
															try
															{
																num79 = GetFloatField(type11, value, "m_fLifeTime");
															}
															catch
															{
															}
															try
															{
																FieldInfo field47 = type11.GetField("m_InstantHit", BindingFlags.Instance | BindingFlags.Public);
																if (field47 != null)
																{
																	flag14 = (bool)field47.GetValue(value);
																}
																else
																{
																	PropertyInfo property10 = type11.GetProperty("m_InstantHit", BindingFlags.Instance | BindingFlags.Public);
																	if (property10 != null)
																	{
																		flag14 = (bool)property10.GetValue(value);
																	}
																}
															}
															catch
															{
															}
														}
													}
												}
												catch
												{
												}
												LogDebug($"[DUMP-DBG] {text2}: HHA via CA '{item.name}' proj={text7} mag={num72} fi={num73:F4}");
												break;
											}
											if (!string.IsNullOrEmpty(text7))
											{
												break;
											}
										}
										if (string.IsNullOrEmpty(text7))
										{
											LogDebug($"[DUMP-DBG] {text2}: no CA match for '{text9}' ({characterAttachmentDatas.Count} CAs)");
										}
									}
								}
								catch (Exception ex)
								{
									MelonLogger.Warning("[DUMP-DBG] " + text2 + ": HHA error: " + ex.Message);
								}
							}
							if (name == "VehicleWheeled")
							{
								text6 = "Wheeled";
								try
								{
									FieldInfo field48 = type2.GetField("MoveSpeed", BindingFlags.Instance | BindingFlags.Public);
									if (field48 != null)
									{
										float num85 = (float)field48.GetValue(component5);
										if (num85 > num4)
										{
											num4 = num85;
										}
									}
								}
								catch
								{
								}
								try
								{
									FieldInfo field49 = type2.GetField("WheelMotorAccel", BindingFlags.Instance | BindingFlags.Public);
									if (field49 != null)
									{
										num14 = (float)field49.GetValue(component5);
									}
								}
								catch
								{
								}
								try
								{
									FieldInfo field50 = type2.GetField("TurningCircleRadius", BindingFlags.Instance | BindingFlags.Public);
									if (field50 != null)
									{
										num15 = (float)field50.GetValue(component5);
									}
								}
								catch
								{
								}
								try
								{
									FieldInfo field51 = type2.GetField("ReverseSpeedScale", BindingFlags.Instance | BindingFlags.Public);
									if (field51 != null)
									{
										num17 = (float)field51.GetValue(component5);
									}
								}
								catch
								{
								}
							}
							if (name == "VehicleHovered")
							{
								text6 = "Hovered";
								try
								{
									FieldInfo field52 = type2.GetField("MoveSpeed", BindingFlags.Instance | BindingFlags.Public);
									if (field52 != null)
									{
										float num86 = (float)field52.GetValue(component5);
										if (num86 > num4)
										{
											num4 = num86;
										}
									}
								}
								catch
								{
								}
								try
								{
									FieldInfo field53 = type2.GetField("MoveAcceleration", BindingFlags.Instance | BindingFlags.Public);
									if (field53 != null)
									{
										num14 = (float)field53.GetValue(component5);
									}
								}
								catch
								{
								}
								try
								{
									FieldInfo field54 = type2.GetField("TurnSpeed", BindingFlags.Instance | BindingFlags.Public);
									if (field54 != null)
									{
										num16 = (float)field54.GetValue(component5);
									}
								}
								catch
								{
								}
							}
							if (name == "VehicleAir")
							{
								text6 = "Air";
								try
								{
									FieldInfo field55 = type2.GetField("ForwardSpeed", BindingFlags.Instance | BindingFlags.Public);
									if (field55 != null)
									{
										num11 = (float)field55.GetValue(component5);
									}
								}
								catch
								{
								}
								try
								{
									FieldInfo field56 = type2.GetField("StrafeSpeed", BindingFlags.Instance | BindingFlags.Public);
									if (field56 != null)
									{
										num12 = (float)field56.GetValue(component5);
									}
								}
								catch
								{
								}
								try
								{
									FieldInfo field57 = type2.GetField("TurboSpeed", BindingFlags.Instance | BindingFlags.Public);
									if (field57 != null)
									{
										num13 = (float)field57.GetValue(component5);
									}
								}
								catch
								{
								}
							}
							if (!(name == "TeleportUI"))
							{
								continue;
							}
							flag15 = true;
							try
							{
								FieldInfo field58 = type2.GetField("TeleportTime", BindingFlags.Instance | BindingFlags.Public);
								if (field58 != null)
								{
									num80 = (float)field58.GetValue(component5);
								}
							}
							catch
							{
							}
							try
							{
								FieldInfo field59 = type2.GetField("TeleportCooldownTime", BindingFlags.Instance | BindingFlags.Public);
								if (field59 != null)
								{
									num81 = (float)field59.GetValue(component5);
								}
							}
							catch
							{
							}
							try
							{
								FieldInfo field60 = type2.GetField("TeleportPct", BindingFlags.Instance | BindingFlags.Public);
								if (field60 != null)
								{
									num82 = (float)field60.GetValue(component5);
								}
							}
							catch
							{
							}
							try
							{
								FieldInfo field61 = type2.GetField("OnlyForSameTeamUnit", BindingFlags.Instance | BindingFlags.Public);
								if (field61 != null)
								{
									flag16 = (bool)field61.GetValue(component5);
								}
							}
							catch
							{
							}
						}
					}
					if (!flag)
					{
						stringBuilder.AppendLine(",");
					}
					flag = false;
					stringBuilder.Append("    {");
					stringBuilder.Append("\"name\":\"" + EscJson(text2) + "\",");
					stringBuilder.Append("\"internal\":\"" + EscJson(objectInfo2.name) + "\",");
					stringBuilder.Append("\"faction\":\"" + text5 + "\",");
					stringBuilder.Append("\"team\":\"" + EscJson(text4) + "\",");
					stringBuilder.Append("\"is_structure\":" + (flag2 ? "true" : "false") + ",");
					stringBuilder.Append("\"built_at\":\"" + EscJson(s) + "\",");
					stringBuilder.Append($"\"cost\":{resourceCost},");
					stringBuilder.Append($"\"build_time\":{buildUpTime:F1},");
					stringBuilder.Append($"\"finished_wait_time\":{num:F1},");
					stringBuilder.Append($"\"clean_up_time\":{num2:F1},");
					stringBuilder.Append($"\"min_tier\":{minimumTeamTier},");
					stringBuilder.Append($"\"max_tier\":{maximumTeamTier},");
					stringBuilder.Append($"\"hp\":{num3},");
					string text10 = "None";
					int num87 = 0;
					try
					{
						FieldInfo field62 = objectInfo2.GetType().GetField("UnitCapType", BindingFlags.Instance | BindingFlags.Public);
						FieldInfo field63 = objectInfo2.GetType().GetField("UnitCapValue", BindingFlags.Instance | BindingFlags.Public);
						if (field62 != null)
						{
							text10 = field62.GetValue(objectInfo2)?.ToString() ?? "None";
						}
						if (field63 != null)
						{
							num87 = (int)field63.GetValue(objectInfo2);
						}
					}
					catch
					{
					}
					stringBuilder.Append("\"unit_cap_type\":\"" + text10 + "\",");
					stringBuilder.Append($"\"unit_cap_value\":{num87},");
					stringBuilder.Append($"\"max_dist\":{maximumBaseStructureDistance:F0},");
					stringBuilder.Append("\"use_radius\":" + (maximumDistanceUseRadius ? "true" : "false") + ",");
					stringBuilder.Append($"\"move_speed\":{num4:F1},");
					stringBuilder.Append($"\"fly_speed\":{num5:F1},");
					if (num6 >= 0f)
					{
						stringBuilder.Append($"\"fly_strafe_scale\":{num6:F2},");
					}
					if (num7 >= 0f)
					{
						stringBuilder.Append($"\"fly_back_scale\":{num7:F2},");
					}
					stringBuilder.Append($"\"walk_speed\":{num8:F1},");
					stringBuilder.Append($"\"run_speed\":{num9:F1},");
					stringBuilder.Append($"\"jump_speed\":{num10:F2},");
					stringBuilder.Append("\"veh_type\":\"" + text6 + "\",");
					stringBuilder.Append($"\"veh_accel\":{num14:F2},");
					stringBuilder.Append($"\"veh_turn_radius\":{num15:F1},");
					stringBuilder.Append($"\"veh_turn_speed\":{num16:F1},");
					stringBuilder.Append($"\"veh_reverse_scale\":{num17:F2},");
					if (num11 > 0f)
					{
						stringBuilder.Append($"\"air_forward_speed\":{num11:F1},");
					}
					if (num12 > 0f)
					{
						stringBuilder.Append($"\"air_strafe_speed\":{num12:F1},");
					}
					if (num13 > 0f)
					{
						stringBuilder.Append($"\"air_turbo_speed\":{num13:F1},");
					}
					stringBuilder.Append($"\"atk_damage\":{num18:F0},");
					stringBuilder.Append($"\"atk_cooldown\":{num19:F2},");
					stringBuilder.Append($"\"atk_range\":{num20:F0},");
					stringBuilder.Append($"\"atk_spread\":{num21:F2},");
					stringBuilder.Append("\"atk_proj\":\"" + EscJson(s2) + "\",");
					stringBuilder.Append($"\"proj_speed\":{num26:F1},");
					stringBuilder.Append($"\"proj_lifetime\":{num27:F2},");
					stringBuilder.Append("\"instant_hit\":" + (flag3 ? "true" : "false") + ",");
					stringBuilder.Append($"\"atk2_damage\":{num22:F0},");
					stringBuilder.Append($"\"atk2_cooldown\":{num23:F2},");
					stringBuilder.Append($"\"atk2_range\":{num24:F0},");
					stringBuilder.Append($"\"atk2_spread\":{num25:F2},");
					stringBuilder.Append("\"atk2_proj\":\"" + EscJson(s3) + "\",");
					stringBuilder.Append($"\"proj2_speed\":{num28:F1},");
					stringBuilder.Append($"\"proj2_lifetime\":{num29:F2},");
					stringBuilder.Append("\"instant_hit2\":" + (flag4 ? "true" : "false") + ",");
					stringBuilder.Append($"\"fow_view\":{num30:F0},");
					stringBuilder.Append($"\"target_dist\":{num31:F0},");
					stringBuilder.Append($"\"vt_fire_interval\":{num33:F4},");
					stringBuilder.Append($"\"vt_spread\":{num34:F3},");
					stringBuilder.Append($"\"vt_magazine\":{num35},");
					stringBuilder.Append($"\"vt_reload\":{num37:F1},");
					stringBuilder.Append($"\"vt_shot_count\":{num36},");
					stringBuilder.Append($"\"vt2_fire_interval\":{num38:F4},");
					stringBuilder.Append($"\"vt2_spread\":{num39:F3},");
					stringBuilder.Append($"\"vt2_magazine\":{num41},");
					stringBuilder.Append($"\"vt2_shot_count\":{num42},");
					stringBuilder.Append($"\"vt2_reload\":{num40:F1},");
					stringBuilder.Append("\"vt_proj\":\"" + EscJson(s4) + "\",");
					stringBuilder.Append($"\"vt_impact_dmg\":{num43:F1},");
					stringBuilder.Append($"\"vt_ricochet_dmg\":{num44:F1},");
					stringBuilder.Append($"\"vt_splash_dmg\":{num45:F1},");
					stringBuilder.Append("\"vt_has_splash\":" + (flag5 ? "true" : "false") + ",");
					stringBuilder.Append($"\"vt_pen_dmg\":{num46:F1},");
					stringBuilder.Append("\"vt_has_pen\":" + (flag6 ? "true" : "false") + ",");
					stringBuilder.Append("\"vt_instant_hit\":" + (flag7 ? "true" : "false") + ",");
					stringBuilder.Append($"\"vt_proj_speed\":{num47:F1},");
					stringBuilder.Append($"\"vt_proj_lifetime\":{num48:F2},");
					stringBuilder.Append("\"vt2_proj\":\"" + EscJson(s5) + "\",");
					stringBuilder.Append($"\"vt2_impact_dmg\":{num49:F1},");
					stringBuilder.Append($"\"vt2_ricochet_dmg\":{num50:F1},");
					stringBuilder.Append($"\"vt2_splash_dmg\":{num51:F1},");
					stringBuilder.Append("\"vt2_has_splash\":" + (flag8 ? "true" : "false") + ",");
					stringBuilder.Append($"\"vt2_pen_dmg\":{num52:F1},");
					stringBuilder.Append("\"vt2_has_pen\":" + (flag9 ? "true" : "false") + ",");
					stringBuilder.Append("\"vt2_instant_hit\":" + (flag10 ? "true" : "false") + ",");
					stringBuilder.Append($"\"vt2_proj_speed\":{num53:F1},");
					stringBuilder.Append($"\"vt2_proj_lifetime\":{num54:F2},");
					stringBuilder.Append($"\"vt_count\":{num32},");
					if (num32 >= 2)
					{
						stringBuilder.Append("\"vt3_proj\":\"" + EscJson(s6) + "\",");
						stringBuilder.Append($"\"vt3_fire_interval\":{num55:F4},");
						stringBuilder.Append($"\"vt3_spread\":{num56:F3},");
						stringBuilder.Append($"\"vt3_magazine\":{num57},");
						stringBuilder.Append($"\"vt3_reload\":{num59:F1},");
						stringBuilder.Append($"\"vt3_shot_count\":{num58},");
						stringBuilder.Append($"\"vt3_impact_dmg\":{num60:F1},");
						stringBuilder.Append($"\"vt3_ricochet_dmg\":{num61:F1},");
						stringBuilder.Append($"\"vt3_splash_dmg\":{num62:F1},");
						stringBuilder.Append("\"vt3_has_splash\":" + (flag11 ? "true" : "false") + ",");
						stringBuilder.Append($"\"vt3_pen_dmg\":{num63:F1},");
						stringBuilder.Append("\"vt3_has_pen\":" + (flag12 ? "true" : "false") + ",");
						stringBuilder.Append("\"vt3_instant_hit\":" + (flag13 ? "true" : "false") + ",");
						stringBuilder.Append($"\"vt3_proj_speed\":{num64:F1},");
						stringBuilder.Append($"\"vt3_proj_lifetime\":{num65:F2},");
					}
					stringBuilder.Append($"\"proj_impact_dmg\":{num66:F1},");
					stringBuilder.Append($"\"proj_ricochet_dmg\":{num67:F1},");
					stringBuilder.Append($"\"proj_splash_dmg\":{num68:F1},");
					stringBuilder.Append($"\"proj2_impact_dmg\":{num69:F1},");
					stringBuilder.Append($"\"proj2_ricochet_dmg\":{num70:F1},");
					stringBuilder.Append($"\"proj2_splash_dmg\":{num71:F1},");
					if (!string.IsNullOrEmpty(text7))
					{
						stringBuilder.Append("\"hha_proj\":\"" + EscJson(text7) + "\",");
						stringBuilder.Append($"\"hha_impact_dmg\":{num77:F1},");
						stringBuilder.Append($"\"hha_proj_speed\":{num78:F1},");
						stringBuilder.Append($"\"hha_proj_lifetime\":{num79:F2},");
						stringBuilder.Append("\"hha_instant_hit\":" + (flag14 ? "true" : "false") + ",");
						stringBuilder.Append($"\"hha_magazine\":{num72},");
						stringBuilder.Append($"\"hha_fire_delay\":{num73:F4},");
						stringBuilder.Append($"\"hha_reload_time\":{num74:F2},");
						stringBuilder.Append($"\"hha_spread_min\":{num75:F3},");
						stringBuilder.Append($"\"hha_spread_max\":{num76:F3},");
					}
					stringBuilder.Append("\"has_teleport\":" + (flag15 ? "true" : "false") + ",");
					stringBuilder.Append($"\"teleport_time\":{num80:F1},");
					stringBuilder.Append($"\"teleport_cooldown\":{num81:F1},");
					stringBuilder.Append($"\"teleport_pct\":{num82:F2},");
					stringBuilder.Append("\"teleport_same_team\":" + (flag16 ? "true" : "false"));
					stringBuilder.Append("}");
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("  ],");
				stringBuilder.AppendLine("  \"production_tree\": {");
				bool flag17 = true;
				foreach (KeyValuePair<string, List<string>> item2 in dictionary)
				{
					if (!flag17)
					{
						stringBuilder.AppendLine(",");
					}
					flag17 = false;
					stringBuilder.Append("    \"" + EscJson(item2.Key) + "\": [");
					stringBuilder.Append(string.Join(", ", item2.Value.ConvertAll((string n) => "\"" + EscJson(n) + "\"")));
					stringBuilder.Append("]");
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("  }");
				stringBuilder.AppendLine("}");
				File.WriteAllText(text, stringBuilder.ToString());
				LogDebug($"[JSON DUMP] Wrote {hashSet.Count} entries to {text}");
			}
			catch (Exception arg)
			{
				MelonLogger.Error($"[JSON DUMP] Error: {arg}");
			}
		}

		private static void UpdateConfigBaseAnnotations()
		{
			try
			{
				string path = Path.Combine(Path.GetDirectoryName(_configPath), "Si_UnitBalance_Dump.json");
				if (!File.Exists(path) || !File.Exists(_configPath))
				{
					return;
				}
				JObject jObject = JObject.Parse(File.ReadAllText(path));
				JObject jObject2 = JObject.Parse(File.ReadAllText(_configPath));
				JArray jArray = jObject["units"] as JArray;
				JObject jObject3 = jObject2["units"] as JObject;
				if (jArray == null || jObject3 == null)
				{
					return;
				}
				Dictionary<string, JObject> dictionary = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);
				foreach (JObject item in jArray)
				{
					string displayName = Js(item, "name");
					string internalName = Js(item, "internal");
					string text = ResolveConfigName(displayName, internalName);
					if (!string.IsNullOrEmpty(text) && !dictionary.ContainsKey(text))
					{
						dictionary[text] = item;
					}
				}
				int num = 0;
				foreach (JProperty item2 in jObject3.Properties())
				{
					string name = item2.Name;
					if (name.StartsWith("_") || !(item2.Value is JObject jObject5) || !dictionary.TryGetValue(name, out var value))
					{
						continue;
					}
					bool flag = false;
					int num2 = Ji(value, "hp");
					int num3 = Ji(value, "cost");
					float num4 = Jf(value, "build_time") + Jf(value, "finished_wait_time") + Jf(value, "clean_up_time");
					int num5 = Ji(value, "min_tier", -1);
					string text2 = ((num5 < 0) ? "T0" : $"T{num5}");
					string text3 = $"HP:{num2} Cost:{num3} Build:{num4:F1}s {text2}";
					if (jObject5["_base"]?.ToString() != text3)
					{
						jObject5["_base"] = text3;
						flag = true;
					}
					bool num6 = Js(value, "faction") == "Alien" && !Jb(value, "is_structure");
					bool flag2 = !string.IsNullOrEmpty(Js(value, "vt_proj"));
					if (num6)
					{
						string text4 = Js(value, "atk_proj");
						if (!string.IsNullOrEmpty(text4))
						{
							string text5 = FormatWeaponAnnotation(text4, Jf(value, "proj_impact_dmg"), Jf(value, "proj_ricochet_dmg"), Jf(value, "proj_splash_dmg"), hasSplash: true, 0f, hasPen: false, Jf(value, "proj_speed"), Jf(value, "proj_lifetime"), Jf(value, "atk_spread"), 0, 0f, 0f);
							if (!string.IsNullOrEmpty(text5))
							{
								jObject5["_pri_weapon"] = text5;
								flag = true;
							}
						}
						int num7 = Ji(value, "atk2_damage");
						if (num7 > 0)
						{
							jObject5["_sec_weapon"] = $"Melee | dmg:{num7}";
							flag = true;
						}
						float num8 = Jf(value, "move_speed");
						float num9 = Jf(value, "fly_speed");
						if (num8 > 0f || num9 > 0f)
						{
							string text6 = $"Move:{num8:G} Fly:{num9:G}";
							float num10 = Jf(value, "fly_strafe_scale", 1f);
							if (Math.Abs(num10 - 1f) > 0.01f)
							{
								text6 += $" Strafe:{num10:G}";
							}
							jObject5["_base_speed"] = text6;
							flag = true;
						}
					}
					else if (flag2)
					{
						int value2 = 0;
						_vtPriIndex.TryGetValue(name, out value2);
						string text7 = Js(value, "vt_proj");
						string text8 = Js(value, "vt3_proj");
						int num11 = Ji(value, "vt_count", 1);
						string text9;
						string pfx;
						string text10;
						string pfx2;
						if (value2 == 0)
						{
							text9 = text7;
							pfx = "vt_";
							text10 = ((num11 >= 2) ? text8 : "");
							pfx2 = "vt3_";
						}
						else
						{
							text9 = ((num11 >= 2) ? text8 : "");
							pfx = "vt3_";
							text10 = text7;
							pfx2 = "vt_";
						}
						if (!string.IsNullOrEmpty(text9))
						{
							string text11 = FormatVTWeapon(value, pfx, text9);
							if (!string.IsNullOrEmpty(text11))
							{
								jObject5["_pri_weapon"] = text11;
								flag = true;
							}
						}
						if (!string.IsNullOrEmpty(text10))
						{
							string text12 = FormatVTWeapon(value, pfx2, text10);
							if (!string.IsNullOrEmpty(text12))
							{
								jObject5["_sec_weapon"] = text12;
								flag = true;
							}
						}
						float num12 = Jf(value, "move_speed");
						if (num12 > 0f)
						{
							jObject5["_base_speed"] = $"Speed:{num12:G}";
							flag = true;
						}
					}
					int num13 = Ji(value, "fow_view");
					int num14 = Ji(value, "target_dist");
					if (num13 > 0 || num14 > 0)
					{
						string text13 = $"FOW:{num13} Target:{num14}";
						if (jObject5["_base_sense"]?.ToString() != text13)
						{
							jObject5["_base_sense"] = text13;
							flag = true;
						}
					}
					if (flag)
					{
						num++;
					}
				}
				File.WriteAllText(_configPath, jObject2.ToString());
				MelonLogger.Msg($"[ANNOTATIONS] Updated base annotations for {num} units in config");
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[ANNOTATIONS] Failed to update config annotations: " + ex.Message);
			}
		}

		private static string Js(JObject o, string k)
		{
			JToken jToken = o[k];
			if (jToken == null)
			{
				return "";
			}
			return jToken.ToString();
		}

		private static int Ji(JObject o, string k, int def = 0)
		{
			JToken jToken = o[k];
			if (jToken == null)
			{
				return def;
			}
			return (int)jToken;
		}

		private static float Jf(JObject o, string k, float def = 0f)
		{
			JToken jToken = o[k];
			if (jToken == null)
			{
				return def;
			}
			return (float)jToken;
		}

		private static bool Jb(JObject o, string k)
		{
			JToken jToken = o[k];
			if (jToken != null)
			{
				return (bool)jToken;
			}
			return false;
		}

		private static string FormatWeaponAnnotation(string projName, float impact, float ricochet, float splash, bool hasSplash, float pen, bool hasPen, float speed, float lifetime, float spread, int magazine, float fireInterval, float reload)
		{
			List<string> list = new List<string> { projName + " |" };
			if (impact > 0f)
			{
				list.Add($"imp:{impact:G}");
			}
			if ((splash > 0f) & hasSplash)
			{
				list.Add($"spl:{splash:G}");
			}
			if ((pen > 0f) & hasPen)
			{
				list.Add($"pen:{pen:G}");
			}
			if (ricochet > 0f)
			{
				list.Add($"ric:{ricochet:G}");
			}
			if (speed > 0f)
			{
				list.Add($"spd:{speed:G}");
			}
			if (lifetime > 0f)
			{
				list.Add($"life:{lifetime:G}");
			}
			if (spread > 0f)
			{
				list.Add($"spread:{spread:G}");
			}
			if (magazine > 0)
			{
				list.Add($"mag:{magazine}");
			}
			if (fireInterval > 0f)
			{
				list.Add($"fi:{fireInterval:G}");
			}
			if (reload > 0f)
			{
				list.Add($"reload:{reload:G}");
			}
			if (list.Count <= 1)
			{
				return "";
			}
			return string.Join(" ", list);
		}

		private static string FormatVTWeapon(JObject dump, string pfx, string projName)
		{
			return FormatWeaponAnnotation(projName, Jf(dump, pfx + "impact_dmg"), Jf(dump, pfx + "ricochet_dmg"), Jf(dump, pfx + "splash_dmg"), Jb(dump, pfx + "has_splash"), Jf(dump, pfx + "pen_dmg"), Jb(dump, pfx + "has_pen"), Jf(dump, pfx + "proj_speed"), Jf(dump, pfx + "proj_lifetime"), Jf(dump, pfx + "spread"), Ji(dump, pfx + "magazine"), Jf(dump, pfx + "fire_interval"), Jf(dump, pfx + "reload"));
		}

		private static void MigrateMultipliersForVersionChange(JObject oldDump, string oldVersion, string newVersion)
		{
			try
			{
				string path = Path.Combine(Path.GetDirectoryName(_configPath), "Si_UnitBalance_Dump.json");
				if (File.Exists(path))
				{
					JObject dump = JObject.Parse(File.ReadAllText(path));
					Dictionary<string, JObject> oldLookup = BuildDumpLookup(oldDump);
					Dictionary<string, JObject> newLookup = BuildDumpLookup(dump);
					string text = Path.Combine(Path.GetDirectoryName(_configPath), "Si_UnitBalance_Config_Default.json");
					int num = 0;
					int num2 = 0;
					if (File.Exists(_configPath))
					{
						(int adjusted, int units) tuple = MigrateConfigFile(_configPath, oldLookup, newLookup, oldVersion, newVersion);
						int item = tuple.adjusted;
						int item2 = tuple.units;
						num += item;
						num2 += item2;
					}
					if (File.Exists(text))
					{
						(int adjusted, int units) tuple2 = MigrateConfigFile(text, oldLookup, newLookup, oldVersion, newVersion);
						int item3 = tuple2.adjusted;
						int item4 = tuple2.units;
						num += item3;
						num2 += item4;
					}
					if (num > 0)
					{
						MelonLogger.Msg($"[MIGRATE] Adjusted {num} multipliers across {num2} units for version change {oldVersion} -> {newVersion}");
					}
					else
					{
						MelonLogger.Msg("[MIGRATE] No multiplier adjustments needed for version change " + oldVersion + " -> " + newVersion);
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Error("[MIGRATE] Migration error: " + ex.Message);
			}
		}

		private static Dictionary<string, JObject> BuildDumpLookup(JObject dump)
		{
			Dictionary<string, JObject> dictionary = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);
			if (!(dump["units"] is JArray jArray))
			{
				return dictionary;
			}
			foreach (JObject item in jArray)
			{
				string displayName = Js(item, "name");
				string internalName = Js(item, "internal");
				string text = ResolveConfigName(displayName, internalName);
				if (!string.IsNullOrEmpty(text) && !dictionary.ContainsKey(text))
				{
					dictionary[text] = item;
				}
			}
			return dictionary;
		}

		private static (int adjusted, int units) MigrateConfigFile(string configPath, Dictionary<string, JObject> oldLookup, Dictionary<string, JObject> newLookup, string oldVersion, string newVersion)
		{
			JObject jObject = JObject.Parse(File.ReadAllText(configPath));
			if (!(jObject["units"] is JObject jObject2))
			{
				return (adjusted: 0, units: 0);
			}
			int num = 0;
			int num2 = 0;
			string fileName = Path.GetFileName(configPath);
			foreach (JProperty item in jObject2.Properties())
			{
				string name = item.Name;
				if (!name.StartsWith("_") && item.Value is JObject entry && oldLookup.TryGetValue(name, out JObject value) && newLookup.TryGetValue(name, out JObject value2))
				{
					bool flag = Js(value, "faction") == "Alien" && !Jb(value, "is_structure");
					bool flag2 = !string.IsNullOrEmpty(Js(value, "vt_proj"));
					int value3 = 0;
					_vtPriIndex.TryGetValue(name, out value3);
					int num3 = 0;
					num3 += MigrateMult(entry, "health_mult", name, fileName, Ji(value, "hp"), Ji(value2, "hp"));
					num3 += MigrateMult(entry, "cost_mult", name, fileName, Ji(value, "cost"), Ji(value2, "cost"));
					float oldVanilla = Jf(value, "build_time") + Jf(value, "finished_wait_time") + Jf(value, "clean_up_time");
					float newVanilla = Jf(value2, "build_time") + Jf(value2, "finished_wait_time") + Jf(value2, "clean_up_time");
					num3 += MigrateMult(entry, "build_time_mult", name, fileName, oldVanilla, newVanilla);
					float num4 = Jf(value, "move_speed");
					float newVanilla2 = Jf(value2, "move_speed");
					if (num4 <= 0f)
					{
						num4 = Jf(value, "run_speed");
						newVanilla2 = Jf(value2, "run_speed");
					}
					num3 += MigrateMult(entry, "move_speed_mult", name, fileName, num4, newVanilla2);
					num3 += MigrateMult(entry, "jump_speed_mult", name, fileName, Jf(value, "jump_speed"), Jf(value2, "jump_speed"));
					num3 += MigrateMult(entry, "walk_speed_mult", name, fileName, Jf(value, "walk_speed"), Jf(value2, "walk_speed"));
					num3 += MigrateMult(entry, "run_speed_mult", name, fileName, Jf(value, "run_speed"), Jf(value2, "run_speed"));
					num3 += MigrateMult(entry, "sprint_speed_mult", name, fileName, Jf(value, "sprint_speed"), Jf(value2, "sprint_speed"));
					if (flag2)
					{
						string dumpPfx = ((value3 == 0) ? "vt_" : "vt3_");
						string dumpPfx2 = ((value3 == 0) ? "vt3_" : "vt_");
						num3 += MigrateWeaponMults(entry, "pri", name, fileName, value, value2, dumpPfx);
						num3 += MigrateWeaponMults(entry, "sec", name, fileName, value, value2, dumpPfx2);
					}
					else if (flag)
					{
						num3 += MigrateCreatureWeaponMults(entry, "pri", name, fileName, value, value2, "", "");
						num3 += MigrateCreatureWeaponMults(entry, "sec", name, fileName, value, value2, "2", "2");
					}
					if (num3 > 0)
					{
						num += num3;
						num2++;
					}
				}
			}
			string a = (string?)jObject["game_version"];
			jObject["game_version"] = newVersion;
			if (num > 0 || !string.Equals(a, newVersion, StringComparison.Ordinal))
			{
				File.WriteAllText(configPath, jObject.ToString());
			}
			return (adjusted: num, units: num2);
		}

		private static int MigrateWeaponMults(JObject entry, string weapon, string unitName, string fileName, JObject oldUnit, JObject newUnit, string dumpPfx)
		{
			return 0 + MigrateMult(entry, weapon + "_impact_damage_mult", unitName, fileName, Jf(oldUnit, dumpPfx + "impact_dmg"), Jf(newUnit, dumpPfx + "impact_dmg")) + MigrateMult(entry, weapon + "_splash_damage_mult", unitName, fileName, Jf(oldUnit, dumpPfx + "splash_dmg"), Jf(newUnit, dumpPfx + "splash_dmg")) + MigrateMult(entry, weapon + "_ricochet_damage_mult", unitName, fileName, Jf(oldUnit, dumpPfx + "ricochet_dmg"), Jf(newUnit, dumpPfx + "ricochet_dmg")) + MigrateMult(entry, weapon + "_penetrating_damage_mult", unitName, fileName, Jf(oldUnit, dumpPfx + "pen_dmg"), Jf(newUnit, dumpPfx + "pen_dmg")) + MigrateMult(entry, weapon + "_proj_speed_mult", unitName, fileName, Jf(oldUnit, dumpPfx + "proj_speed"), Jf(newUnit, dumpPfx + "proj_speed")) + MigrateMult(entry, weapon + "_proj_lifetime_mult", unitName, fileName, Jf(oldUnit, dumpPfx + "proj_lifetime"), Jf(newUnit, dumpPfx + "proj_lifetime")) + MigrateMult(entry, weapon + "_accuracy_mult", unitName, fileName, Jf(oldUnit, dumpPfx + "spread"), Jf(newUnit, dumpPfx + "spread")) + MigrateMult(entry, weapon + "_magazine_mult", unitName, fileName, Jf(oldUnit, dumpPfx + "magazine"), Jf(newUnit, dumpPfx + "magazine")) + MigrateMult(entry, weapon + "_fire_rate_mult", unitName, fileName, Jf(oldUnit, dumpPfx + "fire_interval"), Jf(newUnit, dumpPfx + "fire_interval")) + MigrateMult(entry, weapon + "_reload_time_mult", unitName, fileName, Jf(oldUnit, dumpPfx + "reload"), Jf(newUnit, dumpPfx + "reload"));
		}

		private static int MigrateCreatureWeaponMults(JObject entry, string weapon, string unitName, string fileName, JObject oldUnit, JObject newUnit, string atkSuffix, string projSuffix)
		{
			return 0 + MigrateMult(entry, weapon + "_impact_damage_mult", unitName, fileName, Jf(oldUnit, "proj" + projSuffix + "_impact_dmg"), Jf(newUnit, "proj" + projSuffix + "_impact_dmg")) + MigrateMult(entry, weapon + "_splash_damage_mult", unitName, fileName, Jf(oldUnit, "proj" + projSuffix + "_splash_dmg"), Jf(newUnit, "proj" + projSuffix + "_splash_dmg")) + MigrateMult(entry, weapon + "_ricochet_damage_mult", unitName, fileName, Jf(oldUnit, "proj" + projSuffix + "_ricochet_dmg"), Jf(newUnit, "proj" + projSuffix + "_ricochet_dmg")) + MigrateMult(entry, weapon + "_proj_speed_mult", unitName, fileName, Jf(oldUnit, "proj_speed" + ((projSuffix == "2") ? "2" : "")), Jf(newUnit, "proj_speed" + ((projSuffix == "2") ? "2" : ""))) + MigrateMult(entry, weapon + "_proj_lifetime_mult", unitName, fileName, Jf(oldUnit, "proj_lifetime" + ((projSuffix == "2") ? "2" : "")), Jf(newUnit, "proj_lifetime" + ((projSuffix == "2") ? "2" : ""))) + MigrateMult(entry, weapon + "_accuracy_mult", unitName, fileName, Jf(oldUnit, "atk" + atkSuffix + "_spread"), Jf(newUnit, "atk" + atkSuffix + "_spread"));
		}

		private static int MigrateMult(JObject entry, string key, string unitName, string fileName, float oldVanilla, float newVanilla)
		{
			JToken jToken = entry[key];
			if (jToken == null)
			{
				return 0;
			}
			float num;
			if (jToken.Type == JTokenType.Integer)
			{
				num = (int)jToken;
			}
			else
			{
				if (jToken.Type != JTokenType.Float)
				{
					return 0;
				}
				num = (float)jToken;
			}
			if (Math.Abs(num - 1f) < 0.0001f)
			{
				return 0;
			}
			if (Math.Abs(oldVanilla - newVanilla) < 0.0001f)
			{
				return 0;
			}
			if (Math.Abs(newVanilla) < 0.0001f)
			{
				MelonLogger.Warning($"[MIGRATE] {unitName}.{key}: new vanilla=0, skipping (was {num}x * {oldVanilla})");
				return 0;
			}
			if (Math.Abs(oldVanilla) < 0.0001f)
			{
				return 0;
			}
			float num2 = oldVanilla * num;
			float num3 = num2 / newVanilla;
			num3 = (float)Math.Round(num3, 4);
			MelonLogger.Msg($"[MIGRATE] [{fileName}] {unitName}.{key}: {num} -> {num3} " + $"(vanilla {oldVanilla} -> {newVanilla}, preserving modded={num2:G})");
			entry[key] = num3;
			return 1;
		}

		private static float GetFloatField(Type t, object obj, string fieldName)
		{
			FieldInfo field = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
			if (field != null)
			{
				object value = field.GetValue(obj);
				if (value is float)
				{
					return (float)value;
				}
				if (value is int num)
				{
					return num;
				}
			}
			PropertyInfo property = t.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public);
			if (property != null)
			{
				object value2 = property.GetValue(obj);
				if (value2 is float)
				{
					return (float)value2;
				}
				if (value2 is int num2)
				{
					return num2;
				}
			}
			return 0f;
		}

		private static string EscJson(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return "";
			}
			return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
		}

		internal static void CacheVanillaBaseValues()
		{
			_vanillaBaseCache.Clear();
			_sharedAllInfos = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			string[][][] unitNames = _unitNames;
			foreach (string[][] array in unitNames)
			{
				foreach (string[] array2 in array)
				{
					foreach (string text in array2)
					{
						try
						{
							GetUnitParamGroups(text, out string[] _, out string[][] groupKeys);
							List<string> list = new List<string>();
							string[][] array3 = groupKeys;
							foreach (string[] collection in array3)
							{
								list.AddRange(collection);
							}
							Dictionary<string, string> baseValuesLive = GetBaseValuesLive(text, list.ToArray());
							if (baseValuesLive.Count > 0)
							{
								_vanillaBaseCache[text] = baseValuesLive;
								num++;
							}
						}
						catch
						{
						}
					}
				}
			}
			_sharedAllInfos = null;
			MelonLogger.Msg($"[BASE] Cached vanilla base values for {num} units");
		}

		private static bool MatchesUnitName(ObjectInfo info, string unitName)
		{
			if (info.DisplayName == unitName)
			{
				return true;
			}
			return ResolveConfigName(info.DisplayName, info.name) == unitName;
		}

		private static float GetWeaponMult(Dictionary<string, float> dict, string unitName, string weapon)
		{
			if (dict.TryGetValue(weapon + ":" + unitName, out var value))
			{
				return value;
			}
			if (dict.TryGetValue(unitName, out var value2))
			{
				return value2;
			}
			return 1f;
		}

		private static bool HasAnyWeaponMult(Dictionary<string, float> dict, string unitName)
		{
			if (!dict.ContainsKey(unitName) && !dict.ContainsKey("pri:" + unitName))
			{
				return dict.ContainsKey("sec:" + unitName);
			}
			return true;
		}

		private static bool IsBoolMenuParam(string key)
		{
			return key == "impact_scale_by_speed";
		}

		internal static string AimCompName(int v)
		{
			return v switch
			{
				0 => "None (off)", 
				1 => "Primary_VerticalOnly", 
				2 => "Secondary_VerticalOnly", 
				3 => "Primary_Full", 
				4 => "Secondary_Full", 
				_ => "INVALID", 
			};
		}

		private static string FmtAimCompVal(string raw)
		{
			if (int.TryParse((raw ?? "").Trim(), out var result))
			{
				return result + " = " + AimCompName(result);
			}
			return raw;
		}

		internal static string FmtBoolCompact(string key, string raw)
		{
			if (!IsBoolMenuParam(key))
			{
				return raw;
			}
			switch ((raw ?? "").Trim().ToLowerInvariant())
			{
			case "true":
			case "1":
				return "1 (ON)";
			case "false":
			case "0":
				return "0 (OFF)";
			default:
				return raw;
			}
		}

		private static string FmtBoolMenuVal(string key, string raw)
		{
			string text = (raw ?? "").Trim().ToLowerInvariant();
			if (key == "impact_scale_by_speed")
			{
				switch (text)
				{
				case "true":
				case "1":
					return "1 = ON (damage falls off w/ range)";
				case "false":
				case "0":
					return "0 = OFF (flat damage)";
				default:
					return "- (unset → vanilla)";
				}
			}
			return raw;
		}

		private static string CleanWeaponName(string pdName)
		{
			if (string.IsNullOrEmpty(pdName))
			{
				return "";
			}
			if (pdName.StartsWith("ProjectileData_"))
			{
				return pdName.Substring(15);
			}
			if (pdName.StartsWith("Projectile_"))
			{
				return pdName.Substring(11);
			}
			return pdName;
		}

		private static string[] GetDamageKeysForProjectile(object pdObj)
		{
			if (pdObj == null)
			{
				return new string[1] { "damage_mult" };
			}
			List<string> list = new List<string>();
			if (GetFloatMember(pdObj, "m_fImpactDamage") > 0f)
			{
				list.Add("impact_damage_mult");
			}
			bool flag = GetBoolMember(pdObj, "m_bSplash") && GetFloatMember(pdObj, "m_fSplashDamageMax") > 0f;
			if (flag)
			{
				list.Add("splash_damage_mult");
			}
			if (GetBoolMember(pdObj, "m_bPenetrating") && GetFloatMember(pdObj, "m_fPenetratingDamage") > 0f)
			{
				list.Add("penetrating_damage_mult");
			}
			if (GetFloatMember(pdObj, "m_fRicochetDamage") > 0f)
			{
				list.Add("ricochet_damage_mult");
			}
			if (list.Count == 0)
			{
				return new string[1] { "damage_mult" };
			}
			if (flag)
			{
				list.Add("splash_radius_max_mult");
				list.Add("splash_radius_min_mult");
				list.Add("splash_radius_pow_mult");
			}
			return list.ToArray();
		}

		private static void GetWeaponPDObjects(string unitName, out object priPD, out object secPD)
		{
			priPD = null;
			secPD = null;
			try
			{
				ObjectInfo[] obj = _sharedAllInfos ?? Resources.FindObjectsOfTypeAll<ObjectInfo>();
				ObjectInfo objectInfo = null;
				ObjectInfo[] array = obj;
				foreach (ObjectInfo objectInfo2 in array)
				{
					if (!(objectInfo2 == null) && !(objectInfo2.Prefab == null) && MatchesUnitName(objectInfo2, unitName))
					{
						objectInfo = objectInfo2;
						break;
					}
				}
				if (objectInfo == null)
				{
					return;
				}
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				int num = 0;
				Component[] array2 = componentsInChildren;
				foreach (Component component in array2)
				{
					if (component == null)
					{
						continue;
					}
					switch (component.GetType().Name)
					{
					case "VehicleTurret":
					{
						int value = 0;
						_vtPriIndex.TryGetValue(unitName, out value);
						Type type2 = component.GetType();
						string[] array3 = new string[2] { "Primary", "Secondary" };
						foreach (string text2 in array3)
						{
							string[] array4 = new string[2] { "Projectile", "ProjectileData" };
							foreach (string text3 in array4)
							{
								FieldInfo field3 = type2.GetField(text2 + text3, bindingAttr);
								if (field3 == null)
								{
									continue;
								}
								object value2 = field3.GetValue(component);
								if (value2 == null)
								{
									continue;
								}
								if (text2 == "Primary")
								{
									if (num == value)
									{
										if (priPD == null)
										{
											priPD = value2;
										}
									}
									else if (secPD == null)
									{
										secPD = value2;
									}
								}
								else if (text2 == "Secondary" && secPD == null)
								{
									secPD = value2;
								}
								break;
							}
						}
						num++;
						break;
					}
					case "TurretWeapon":
					{
						Type type3 = component.GetType();
						string[] array3 = new string[2] { "Projectile", "ProjectileData" };
						foreach (string name in array3)
						{
							FieldInfo field4 = type3.GetField(name, bindingAttr);
							if (!(field4 == null))
							{
								object value3 = field4.GetValue(component);
								if (value3 != null && priPD == null)
								{
									priPD = value3;
									break;
								}
							}
						}
						break;
					}
					case "CreatureDecapod":
					{
						Type type = component.GetType();
						string[] array3 = new string[2] { "AttackPrimary", "AttackSecondary" };
						foreach (string text in array3)
						{
							object obj2 = null;
							FieldInfo field = type.GetField(text, BindingFlags.Instance | BindingFlags.Public);
							if (field != null)
							{
								try
								{
									obj2 = field.GetValue(component);
								}
								catch
								{
								}
							}
							if (obj2 == null)
							{
								PropertyInfo property = type.GetProperty(text, BindingFlags.Instance | BindingFlags.Public);
								if (property != null)
								{
									try
									{
										obj2 = property.GetValue(component);
									}
									catch
									{
									}
								}
							}
							if (obj2 == null)
							{
								continue;
							}
							FieldInfo field2 = obj2.GetType().GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
							if (field2 == null)
							{
								continue;
							}
							object obj5 = null;
							try
							{
								obj5 = field2.GetValue(obj2);
							}
							catch
							{
							}
							if (obj5 != null)
							{
								if (text == "AttackPrimary" && priPD == null)
								{
									priPD = obj5;
								}
								else if (text == "AttackSecondary" && secPD == null)
								{
									secPD = obj5;
								}
							}
						}
						return;
					}
					}
				}
			}
			catch
			{
			}
		}

		private static Component GetProximityComponent(object pdObj, out string pdName)
		{
			pdName = "";
			if (pdObj == null)
			{
				return null;
			}
			try
			{
				pdName = ((UnityEngine.Object)pdObj).name;
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				object obj = null;
				FieldInfo field = pdObj.GetType().GetField("m_ProjectilePrefab", bindingAttr);
				if (field != null)
				{
					obj = field.GetValue(pdObj);
				}
				if (obj == null)
				{
					PropertyInfo property = pdObj.GetType().GetProperty("m_ProjectilePrefab", bindingAttr);
					if (property != null)
					{
						obj = property.GetValue(pdObj);
					}
				}
				if (obj == null)
				{
					return null;
				}
				GameObject gameObject = null;
				if (obj is GameObject gameObject2)
				{
					gameObject = gameObject2;
				}
				else if (obj is Component component)
				{
					gameObject = component.gameObject;
				}
				if (gameObject == null)
				{
					return null;
				}
				Component[] componentsInChildren = gameObject.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component2 in componentsInChildren)
				{
					if (component2 != null && component2.GetType().Name == "ProjectileProximityDetonation")
					{
						return component2;
					}
				}
			}
			catch
			{
			}
			return null;
		}

		private static void GetUnitWeaponInfo(string unitName, out bool hasPrimary, out bool hasSecondary, out string priName, out string secName)
		{
			hasPrimary = false;
			hasSecondary = false;
			priName = "";
			secName = "";
			try
			{
				ObjectInfo[] obj = _sharedAllInfos ?? Resources.FindObjectsOfTypeAll<ObjectInfo>();
				ObjectInfo objectInfo = null;
				ObjectInfo[] array = obj;
				foreach (ObjectInfo objectInfo2 in array)
				{
					if (!(objectInfo2 == null) && !(objectInfo2.Prefab == null) && MatchesUnitName(objectInfo2, unitName))
					{
						objectInfo = objectInfo2;
						break;
					}
				}
				if (objectInfo == null)
				{
					return;
				}
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				List<string> list = new List<string>();
				bool flag = false;
				Component[] array2 = componentsInChildren;
				foreach (Component component in array2)
				{
					if (component == null || !(component.GetType().Name == "VehicleTurret"))
					{
						continue;
					}
					flag = true;
					Type type = component.GetType();
					string[] array3 = new string[2] { "Primary", "Secondary" };
					foreach (string text in array3)
					{
						string[] array4 = new string[2] { "Projectile", "ProjectileData" };
						foreach (string text2 in array4)
						{
							FieldInfo field = type.GetField(text + text2, bindingAttr);
							if (field == null)
							{
								continue;
							}
							object value = field.GetValue(component);
							if (value != null)
							{
								string text3 = "";
								try
								{
									text3 = ((UnityEngine.Object)value).name;
								}
								catch
								{
								}
								if (!string.IsNullOrEmpty(text3) && !list.Contains(text3))
								{
									list.Add(text3);
								}
								break;
							}
						}
					}
				}
				if (flag)
				{
					if (list.Count >= 1)
					{
						hasPrimary = true;
						priName = CleanWeaponName(list[0]);
					}
					if (list.Count >= 2)
					{
						hasSecondary = true;
						secName = CleanWeaponName(list[1]);
					}
					return;
				}
				array2 = componentsInChildren;
				foreach (Component component2 in array2)
				{
					if (component2 == null || component2.GetType().Name != "CreatureDecapod")
					{
						continue;
					}
					Type type2 = component2.GetType();
					FieldInfo field2 = type2.GetField("AttackPrimary", BindingFlags.Instance | BindingFlags.Public);
					if (field2 != null)
					{
						object value2 = field2.GetValue(component2);
						if (value2 != null)
						{
							float floatMember = GetFloatMember(value2, "Damage");
							FieldInfo field3 = value2.GetType().GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
							object obj3 = null;
							if (field3 != null)
							{
								try
								{
									obj3 = field3.GetValue(value2);
								}
								catch
								{
								}
							}
							if (floatMember > 0f || obj3 != null)
							{
								hasPrimary = true;
								if (obj3 != null)
								{
									try
									{
										priName = CleanWeaponName(((UnityEngine.Object)obj3).name);
									}
									catch
									{
									}
								}
								else
								{
									priName = "Melee";
								}
							}
						}
					}
					FieldInfo field4 = type2.GetField("AttackSecondary", BindingFlags.Instance | BindingFlags.Public);
					if (!(field4 != null))
					{
						break;
					}
					object value3 = field4.GetValue(component2);
					if (value3 == null)
					{
						break;
					}
					float floatMember2 = GetFloatMember(value3, "Damage");
					FieldInfo field5 = value3.GetType().GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
					object obj6 = null;
					if (field5 != null)
					{
						try
						{
							obj6 = field5.GetValue(value3);
						}
						catch
						{
						}
					}
					if (floatMember2 > 0f || obj6 != null)
					{
						hasSecondary = true;
						if (obj6 != null)
						{
							try
							{
								secName = CleanWeaponName(((UnityEngine.Object)obj6).name);
								break;
							}
							catch
							{
								break;
							}
						}
						secName = "Melee";
						break;
					}
					break;
				}
			}
			catch
			{
			}
		}

		private static void GetUnitParamGroups(string unitName, out string[] groupNames, out string[][] groupKeys)
		{
			GetUnitWeaponInfo(unitName, out bool hasPrimary, out bool hasSecondary, out string priName, out string secName);
			GetWeaponPDObjects(unitName, out object priPD, out object secPD);
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			try
			{
				ObjectInfo[] obj = _sharedAllInfos ?? Resources.FindObjectsOfTypeAll<ObjectInfo>();
				ObjectInfo objectInfo = null;
				ObjectInfo[] array = obj;
				foreach (ObjectInfo objectInfo2 in array)
				{
					if (!(objectInfo2 == null) && !(objectInfo2.Prefab == null) && MatchesUnitName(objectInfo2, unitName))
					{
						objectInfo = objectInfo2;
						break;
					}
				}
				if (objectInfo != null)
				{
					Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
					foreach (Component component in componentsInChildren)
					{
						if (!(component == null))
						{
							switch (component.GetType().Name)
							{
							case "Soldier":
							case "PlayerMovement":
							case "FPSMovement":
								flag = true;
								break;
							case "VehicleTurret":
							case "TurretWeapon":
								flag2 = true;
								break;
							case "VehicleWheeled":
								flag3 = true;
								break;
							case "VehicleHovered":
								flag4 = true;
								break;
							case "VehicleAir":
								flag5 = true;
								break;
							case "CreatureDecapod":
								flag6 = true;
								break;
							case "MiningClaws":
							case "MiningArmsBase":
							case "MiningArmsLaser":
							case "MiningArmsRoller":
								flag7 = true;
								break;
							}
						}
					}
				}
			}
			catch
			{
			}
			bool flag8 = flag | flag3 | flag4 | flag5 | flag6;
			bool flag9 = !flag8;
			List<string> list = new List<string>();
			List<string[]> list2 = new List<string[]>();
			list.Add("Health & Production");
			if (flag9)
			{
				list2.Add(new string[6] { "health_mult", "cost_mult", "build_time_mult", "min_tier", "build_radius", "deposit_radius" });
			}
			else if (string.Equals(unitName, "Hover Bike", StringComparison.OrdinalIgnoreCase))
			{
				list2.Add(new string[6] { "health_mult", "cost_mult", "build_time_mult", "min_tier", "unit_cap_value", "dispense_timeout" });
			}
			else if (flag7)
			{
				list2.Add(new string[6] { "health_mult", "cost_mult", "build_time_mult", "min_tier", "unit_cap_value", "extraction_radius" });
			}
			else
			{
				list2.Add(new string[5] { "health_mult", "cost_mult", "build_time_mult", "min_tier", "unit_cap_value" });
			}
			if (flag9 & flag2)
			{
				string text = "Damage & Weapons";
				if (!string.IsNullOrEmpty(priName))
				{
					text = text + " (" + priName + ")";
				}
				list.Add(text);
				List<string> list3 = new List<string>();
				list3.AddRange(GetDamageKeysForProjectile(priPD));
				list3.AddRange(new string[7] { "proj_speed_mult", "proj_lifetime_mult", "range_mult", "accuracy_mult", "magazine_mult", "fire_rate_mult", "reload_time_mult" });
				list2.Add(list3.ToArray());
			}
			else if (hasPrimary & flag2)
			{
				string text2 = "Primary Weapon";
				if (!string.IsNullOrEmpty(priName))
				{
					text2 = text2 + " (" + priName + ")";
				}
				list.Add(text2);
				List<string> list4 = new List<string>();
				string[] damageKeysForProjectile = GetDamageKeysForProjectile(priPD);
				foreach (string text3 in damageKeysForProjectile)
				{
					list4.Add("pri_" + text3);
				}
				list4.AddRange(new string[6] { "pri_proj_speed_mult", "pri_proj_lifetime_mult", "pri_accuracy_mult", "pri_magazine_mult", "pri_fire_rate_mult", "pri_reload_time_mult" });
				list2.Add(list4.ToArray());
			}
			else if (hasPrimary & flag6)
			{
				string text4 = "Primary Weapon";
				if (!string.IsNullOrEmpty(priName))
				{
					text4 = text4 + " (" + priName + ")";
				}
				list.Add(text4);
				bool flag10 = string.Equals(priName, "Melee", StringComparison.OrdinalIgnoreCase);
				List<string> list5 = new List<string>();
				if (flag10)
				{
					list5.Add("pri_damage_mult");
				}
				else
				{
					string[] damageKeysForProjectile = GetDamageKeysForProjectile(priPD);
					foreach (string text5 in damageKeysForProjectile)
					{
						list5.Add("pri_" + text5);
					}
					list5.AddRange(new string[3] { "pri_proj_speed_mult", "pri_proj_lifetime_mult", "pri_accuracy_mult" });
				}
				list2.Add(list5.ToArray());
			}
			else if (!hasPrimary & flag)
			{
				list.Add("Primary Weapon");
				List<string> list6 = new List<string>();
				string[] damageKeysForProjectile = GetDamageKeysForProjectile(priPD);
				foreach (string text6 in damageKeysForProjectile)
				{
					list6.Add("pri_" + text6);
				}
				list6.AddRange(new string[2] { "pri_proj_speed_mult", "pri_proj_lifetime_mult" });
				list2.Add(list6.ToArray());
			}
			if (!flag9)
			{
				if (hasSecondary & flag2)
				{
					string text7 = "Secondary Weapon";
					if (!string.IsNullOrEmpty(secName))
					{
						text7 = text7 + " (" + secName + ")";
					}
					list.Add(text7);
					List<string> list7 = new List<string>();
					string[] damageKeysForProjectile = GetDamageKeysForProjectile(secPD);
					foreach (string text8 in damageKeysForProjectile)
					{
						list7.Add("sec_" + text8);
					}
					list7.AddRange(new string[6] { "sec_proj_speed_mult", "sec_proj_lifetime_mult", "sec_accuracy_mult", "sec_magazine_mult", "sec_fire_rate_mult", "sec_reload_time_mult" });
					list2.Add(list7.ToArray());
				}
				else if (hasSecondary & flag6)
				{
					string text9 = "Secondary Weapon";
					if (!string.IsNullOrEmpty(secName))
					{
						text9 = text9 + " (" + secName + ")";
					}
					list.Add(text9);
					bool flag11 = string.Equals(secName, "Melee", StringComparison.OrdinalIgnoreCase);
					List<string> list8 = new List<string>();
					if (flag11)
					{
						list8.Add("sec_damage_mult");
					}
					else
					{
						string[] damageKeysForProjectile = GetDamageKeysForProjectile(secPD);
						foreach (string text10 in damageKeysForProjectile)
						{
							list8.Add("sec_" + text10);
						}
						list8.AddRange(new string[3] { "sec_proj_speed_mult", "sec_proj_lifetime_mult", "sec_accuracy_mult" });
					}
					list2.Add(list8.ToArray());
				}
			}
			if (priPD != null)
			{
				list.Add("Ballistics");
				list2.Add(new string[8] { "impact_scale_by_speed", "drag_mult", "ai_lead_velocity", "ai_lead_gravity", "ai_lead_drag", "aim_compensation", "pitch_max_up", "pitch_max_down" });
			}
			if (flag)
			{
				list.Add("Movement");
				list2.Add(new string[4] { "move_speed_mult", "run_speed_mult", "sprint_speed_mult", "jump_speed_mult" });
			}
			else if (flag3)
			{
				list.Add("Movement");
				list2.Add(new string[2] { "move_speed_mult", "turn_radius_mult" });
			}
			else if (flag4 | flag5)
			{
				list.Add("Movement");
				list2.Add(new string[3] { "move_speed_mult", "turbo_speed_mult", "strafe_speed_mult" });
			}
			else if (flag6)
			{
				list.Add("Movement");
				list2.Add(new string[3] { "move_speed_mult", "fly_speed_mult", "strafe_speed_mult" });
			}
			if (flag8)
			{
				list.Add("Vision & Sense");
				list2.Add(new string[3] { "target_distance", "fow_distance", "visible_event_radius_mult" });
			}
			else if (flag9 & flag2)
			{
				list.Add("Vision & Sense");
				list2.Add(new string[2] { "target_distance", "fow_distance" });
			}
			else if (flag9)
			{
				list.Add("Vision & Sense");
				list2.Add(new string[1] { "fow_distance" });
			}
			Component proximityComponent = GetProximityComponent(priPD, out string pdName);
			if (proximityComponent == null && secPD != null)
			{
				proximityComponent = GetProximityComponent(secPD, out pdName);
			}
			if (proximityComponent != null)
			{
				list.Add("Proximity Detonation");
				list2.Add(new string[5] { "prox_MinimumTime", "prox_SplashRadiusScale", "prox_FlyingUnits", "prox_GroundUnits", "prox_Structures" });
			}
			groupNames = list.ToArray();
			groupKeys = list2.ToArray();
		}

		private static void SendChatToPlayer(object player, string message)
		{
			if (_sendChatToPlayerMethod == null || player == null)
			{
				return;
			}
			try
			{
				_sendChatToPlayerMethod.Invoke(null, new object[2]
				{
					player,
					new string[1] { message }
				});
			}
			catch
			{
			}
		}

		private static void SendConsoleToPlayer(object player, string message)
		{
			if (_sendConsoleToPlayerMethod == null || player == null)
			{
				return;
			}
			try
			{
				_sendConsoleToPlayerMethod.Invoke(null, new object[2]
				{
					player,
					new string[1] { message }
				});
			}
			catch
			{
			}
		}

		private static long GetPlayerKey(object player)
		{
			try
			{
				if (_playerIdField != null)
				{
					object value = _playerIdField.GetValue(player);
					if (value != null)
					{
						FieldInfo field = value.GetType().GetField("m_ID", BindingFlags.Instance | BindingFlags.Public);
						if (field != null)
						{
							object value2 = field.GetValue(value);
							if (value2 is ulong result)
							{
								return (long)result;
							}
							if (value2 is long result2)
							{
								return result2;
							}
						}
					}
				}
			}
			catch
			{
			}
			return 0L;
		}

		private static string GetPlayerName(object player)
		{
			try
			{
				if (_playerNameField != null)
				{
					return _playerNameField.GetValue(player)?.ToString() ?? "Unknown";
				}
			}
			catch
			{
			}
			return "Unknown";
		}

		private static string GetPlayerSteamId(object player)
		{
			try
			{
				if (_playerIdField != null)
				{
					object value = _playerIdField.GetValue(player);
					if (value != null)
					{
						FieldInfo field = value.GetType().GetField("m_ID", BindingFlags.Instance | BindingFlags.Public);
						if (field != null)
						{
							return field.GetValue(value)?.ToString() ?? "0";
						}
					}
				}
			}
			catch
			{
			}
			return "0";
		}

		private static void OnMenuShortcut(object player, string args)
		{
			long playerKey = GetPlayerKey(player);
			if (_menuStates.ContainsKey(playerKey) || _statsStates.ContainsKey(playerKey))
			{
				string text = args?.Trim() ?? "";
				if (text.Length > 0 && (text[0] == '!' || text[0] == '/' || text[0] == '.'))
				{
					text = text.Substring(1);
				}
				ProcessMenuInput(player, text);
			}
		}

		private static void OnBalanceCommand(object player, string args)
		{
			long playerKey = GetPlayerKey(player);
			string text = "";
			if (args != null)
			{
				string[] array = args.Trim().Split(new char[1] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length > 1)
				{
					text = array[1].Trim();
				}
			}
			if (text.Equals("exit", StringComparison.OrdinalIgnoreCase))
			{
				_menuStates.Remove(playerKey);
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Exited balance editor.</color>");
				return;
			}
			if (_menuStates.ContainsKey(playerKey))
			{
				if (string.IsNullOrEmpty(text))
				{
					_menuStates.Remove(playerKey);
					SendChatToPlayer(player, _chatPrefix + _dimColor + "Exited balance editor.</color>");
				}
				else
				{
					ProcessMenuInput(player, text);
				}
				return;
			}
			_statsStates.Remove(playerKey);
			BalanceMenuState balanceMenuState = new BalanceMenuState();
			_menuStates[playerKey] = balanceMenuState;
			if (!string.IsNullOrEmpty(text))
			{
				ProcessMenuInput(player, text);
				return;
			}
			SendChatToPlayer(player, _chatPrefix + _headerColor + "=== Balance Editor ===</color> " + _dimColor + "(.1 .2 .. or !b 1)</color>");
			ShowCurrentMenu(player, balanceMenuState);
		}

		private static Dictionary<string, string> GetBaseValues(string unitName, string[] paramKeys)
		{
			if (_vanillaBaseCache.TryGetValue(unitName, out Dictionary<string, string> value))
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				foreach (string key in paramKeys)
				{
					if (value.TryGetValue(key, out var value2))
					{
						dictionary[key] = value2;
					}
				}
				return dictionary;
			}
			return GetBaseValuesLive(unitName, paramKeys);
		}

		private static Dictionary<string, string> GetBaseValuesLive(string unitName, string[] paramKeys)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			try
			{
				ObjectInfo[] array = _sharedAllInfos ?? Resources.FindObjectsOfTypeAll<ObjectInfo>();
				ObjectInfo objectInfo = null;
				ObjectInfo[] array2 = array;
				foreach (ObjectInfo objectInfo2 in array2)
				{
					if (!(objectInfo2 == null) && !(objectInfo2.Prefab == null) && MatchesUnitName(objectInfo2, unitName))
					{
						objectInfo = objectInfo2;
						break;
					}
				}
				if (objectInfo == null)
				{
					return dictionary;
				}
				GameObject prefab = objectInfo.Prefab;
				Component[] componentsInChildren = prefab.GetComponentsInChildren<Component>(includeInactive: true);
				Component component = null;
				Component component2 = null;
				Component component3 = null;
				Component component4 = null;
				Component component5 = null;
				Component component6 = null;
				List<Component> list = new List<Component>();
				Component[] array3 = componentsInChildren;
				foreach (Component component7 in array3)
				{
					if (component7 == null)
					{
						continue;
					}
					string name = component7.GetType().Name;
					if (name == "VehicleTurret")
					{
						list.Add(component7);
						if (component == null)
						{
							component = component7;
						}
						continue;
					}
					if (name == "TurretWeapon" && component6 == null)
					{
						component6 = component7;
						continue;
					}
					if (name == "Sensor" && component2 == null)
					{
						component2 = component7;
						continue;
					}
					switch (name)
					{
					case "Soldier":
					case "PlayerMovement":
					case "FPSMovement":
						if (component3 == null)
						{
							component3 = component7;
							continue;
						}
						break;
					}
					if (name == "VehicleWheeled" && component4 == null)
					{
						component4 = component7;
					}
					else if (name == "CreatureDecapod" && component5 == null)
					{
						component5 = component7;
					}
				}
				Component component8 = null;
				Component component9 = null;
				string text = "Primary";
				string text2 = "Secondary";
				object obj = null;
				object obj2 = null;
				HashSet<string> hashSet = new HashSet<string>();
				string[] array4;
				foreach (Component item in list)
				{
					Type type = item.GetType();
					array4 = new string[2] { "Primary", "Secondary" };
					foreach (string text3 in array4)
					{
						object obj3 = null;
						string[] array5 = new string[2] { "Projectile", "ProjectileData" };
						foreach (string text4 in array5)
						{
							FieldInfo field = type.GetField(text3 + text4, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							if (!(field == null))
							{
								try
								{
									obj3 = field.GetValue(item);
								}
								catch
								{
								}
								if (obj3 != null)
								{
									break;
								}
							}
						}
						if (obj3 == null)
						{
							continue;
						}
						string text5 = "";
						try
						{
							text5 = ((UnityEngine.Object)obj3).name;
						}
						catch
						{
						}
						if (!string.IsNullOrEmpty(text5) && !hashSet.Contains(text5))
						{
							hashSet.Add(text5);
							if (obj == null)
							{
								obj = obj3;
								component8 = item;
								text = text3;
							}
							else if (obj2 == null)
							{
								obj2 = obj3;
								component9 = item;
								text2 = text3;
							}
						}
					}
				}
				object obj6 = null;
				if (component6 != null)
				{
					FieldInfo field2 = component6.GetType().GetField("ProjectileData", BindingFlags.Instance | BindingFlags.Public);
					if (field2 != null)
					{
						try
						{
							obj6 = field2.GetValue(component6);
						}
						catch
						{
						}
					}
				}
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				array4 = paramKeys;
				foreach (string text6 in array4)
				{
					try
					{
						string text7 = null;
						switch (text6)
						{
						case "health_mult":
						{
							DamageManager component16 = prefab.GetComponent<DamageManager>();
							if (!(component16 != null))
							{
								break;
							}
							FieldInfo field44 = component16.GetType().GetField("Data", bindingAttr);
							if (!(field44 != null))
							{
								break;
							}
							object value28 = field44.GetValue(component16);
							if (value28 != null)
							{
								float floatMember27 = GetFloatMember(value28, "Health");
								if (floatMember27 <= 0f)
								{
									floatMember27 = GetFloatMember(value28, "MaxHealth");
								}
								if (floatMember27 > 0f)
								{
									text7 = floatMember27.ToString("F0");
								}
							}
							break;
						}
						case "cost_mult":
							if (objectInfo.ConstructionData != null)
							{
								text7 = objectInfo.ConstructionData.ResourceCost.ToString();
							}
							break;
						case "build_time_mult":
						{
							if (!(objectInfo.ConstructionData != null))
							{
								break;
							}
							ConstructionData constructionData = objectInfo.ConstructionData;
							float num = constructionData.BuildUpTime;
							try
							{
								Type type2 = constructionData.GetType();
								FieldInfo field29 = type2.GetField("FinishedWaitTime", BindingFlags.Instance | BindingFlags.Public);
								FieldInfo field30 = type2.GetField("CleanUpTime", BindingFlags.Instance | BindingFlags.Public);
								if (field29 != null)
								{
									num += (float)field29.GetValue(constructionData);
								}
								if (field30 != null)
								{
									num += (float)field30.GetValue(constructionData);
								}
							}
							catch
							{
							}
							text7 = num.ToString("F1") + "s";
							break;
						}
						case "min_tier":
							if (objectInfo.ConstructionData != null)
							{
								text7 = objectInfo.ConstructionData.MinimumTeamTier.ToString();
							}
							break;
						case "unit_cap_value":
							text7 = objectInfo.UnitCapValue.ToString();
							break;
						case "build_radius":
							if (objectInfo.ConstructionData != null)
							{
								text7 = objectInfo.ConstructionData.MaximumBaseStructureDistance.ToString("F0");
							}
							break;
						case "deposit_radius":
							if (!(objectInfo.Prefab != null))
							{
								break;
							}
							array3 = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
							foreach (Component component18 in array3)
							{
								if (!(component18 == null) && !(component18.GetType().Name != "ResourceDepositPoint"))
								{
									FieldInfo field52 = component18.GetType().GetField("DepositRadius", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
									if (field52 != null && field52.FieldType == typeof(float))
									{
										text7 = ((float)field52.GetValue(component18)).ToString("F0");
									}
									break;
								}
							}
							break;
						case "extraction_radius":
							if (!(objectInfo.Prefab != null))
							{
								break;
							}
							array3 = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
							foreach (Component component11 in array3)
							{
								if (!(component11 == null))
								{
									FieldInfo field24 = component11.GetType().GetField("ExtractionRadius", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
									if (!(field24 == null) && !(field24.FieldType != typeof(float)))
									{
										text7 = ((float)field24.GetValue(component11)).ToString("F0");
										break;
									}
								}
							}
							break;
						case "damage_mult":
						{
							if (component != null)
							{
								FieldInfo field26 = component.GetType().GetField("PrimaryProjectile", bindingAttr);
								if (field26 != null)
								{
									object value14 = field26.GetValue(component);
									if (value14 != null)
									{
										float floatMember16 = GetFloatMember(value14, "m_fImpactDamage");
										if (floatMember16 > 0f)
										{
											text7 = floatMember16.ToString("F0");
										}
									}
								}
							}
							if (text7 == null && obj6 != null)
							{
								float floatMember17 = GetFloatMember(obj6, "m_fImpactDamage");
								if (floatMember17 > 0f)
								{
									text7 = floatMember17.ToString("F0");
								}
							}
							if (text7 != null || !(component5 != null))
							{
								break;
							}
							FieldInfo field27 = component5.GetType().GetField("AttackPrimary", BindingFlags.Instance | BindingFlags.Public);
							if (!(field27 != null))
							{
								break;
							}
							object value15 = field27.GetValue(component5);
							if (value15 == null)
							{
								break;
							}
							float floatMember18 = GetFloatMember(value15, "Damage");
							if (floatMember18 > 0f)
							{
								text7 = floatMember18.ToString("F0");
								break;
							}
							FieldInfo field28 = value15.GetType().GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
							if (!(field28 != null))
							{
								break;
							}
							object value16 = field28.GetValue(value15);
							if (value16 != null)
							{
								floatMember18 = GetFloatMember(value16, "m_fImpactDamage");
								if (floatMember18 > 0f)
								{
									text7 = floatMember18.ToString("F0");
								}
							}
							break;
						}
						case "impact_damage_mult":
						case "splash_damage_mult":
						case "penetrating_damage_mult":
						case "ricochet_damage_mult":
						{
							string name7 = text6 switch
							{
								"penetrating_damage_mult" => "m_fPenetratingDamage", 
								"splash_damage_mult" => "m_fSplashDamageMax", 
								"impact_damage_mult" => "m_fImpactDamage", 
								_ => "m_fRicochetDamage", 
							};
							if (component != null)
							{
								FieldInfo field49 = component.GetType().GetField("PrimaryProjectile", bindingAttr);
								if (field49 != null)
								{
									object value30 = field49.GetValue(component);
									if (value30 != null)
									{
										float floatMember31 = GetFloatMember(value30, name7);
										if (floatMember31 > 0f)
										{
											text7 = floatMember31.ToString("F0");
										}
									}
								}
							}
							if (text7 == null && obj6 != null)
							{
								float floatMember32 = GetFloatMember(obj6, name7);
								if (floatMember32 > 0f)
								{
									text7 = floatMember32.ToString("F0");
								}
							}
							break;
						}
						case "splash_radius_pow_mult":
						case "splash_radius_max_mult":
						case "splash_radius_min_mult":
						{
							string name9;
							if (text6 == "splash_radius_max_mult")
							{
								name9 = "m_fSplashDamageRadiusMax";
							}
							else
							{
								name9 = ((text6 == "splash_radius_min_mult") ? "m_fSplashDamageRadiusMin" : "m_fSplashDamageRadiusPow");
							}
							if (component != null)
							{
								FieldInfo field55 = component.GetType().GetField("PrimaryProjectile", bindingAttr);
								if (field55 != null)
								{
									object value31 = field55.GetValue(component);
									if (value31 != null)
									{
										float floatMember35 = GetFloatMember(value31, name9);
										if (floatMember35 > 0f)
										{
											text7 = floatMember35.ToString("F1");
										}
									}
								}
							}
							if (text7 == null && obj6 != null)
							{
								float floatMember36 = GetFloatMember(obj6, name9);
								if (floatMember36 > 0f)
								{
									text7 = floatMember36.ToString("F1");
								}
							}
							break;
						}
						case "range_mult":
							if (component != null)
							{
								FieldInfo field42 = component.GetType().GetField("AimDistance", bindingAttr);
								if (field42 != null && field42.FieldType == typeof(float))
								{
									text7 = ((float)field42.GetValue(component)).ToString("F0");
								}
							}
							if (text7 == null && component6 != null && component2 != null)
							{
								FieldInfo field43 = component2.GetType().GetField("TargetingDistance", bindingAttr);
								if (field43 != null && field43.FieldType == typeof(float))
								{
									text7 = ((float)field43.GetValue(component2)).ToString("F0");
								}
							}
							break;
						case "proj_speed_mult":
							if (component != null)
							{
								FieldInfo field35 = component.GetType().GetField("PrimaryProjectile", bindingAttr);
								if (field35 != null)
								{
									object value22 = field35.GetValue(component);
									if (value22 != null)
									{
										float floatMember24 = GetFloatMember(value22, "m_fBaseSpeed");
										if (floatMember24 > 0f)
										{
											text7 = floatMember24.ToString("F0");
										}
									}
								}
							}
							if (text7 == null && obj6 != null)
							{
								float floatMember25 = GetFloatMember(obj6, "m_fBaseSpeed");
								if (floatMember25 > 0f)
								{
									text7 = floatMember25.ToString("F0");
								}
							}
							break;
						case "drag_mult":
						case "ai_lead_gravity":
						case "ai_lead_drag":
						case "ai_lead_velocity":
						case "impact_scale_by_speed":
						{
							object obj9 = obj ?? obj6;
							if (obj9 != null)
							{
								switch (text6)
								{
								case "impact_scale_by_speed":
									text7 = (GetBoolMember(obj9, "m_bImpactScaleBySpeed") ? "1 = On (falls off)" : "0 = Off (flat)");
									break;
								case "drag_mult":
									text7 = GetFloatMember(obj9, "m_fDragCoefficient").ToString("F3");
									break;
								case "ai_lead_velocity":
									text7 = GetFloatMember(obj9, "m_AIAimLeadFactor_Velocity").ToString("F2");
									break;
								case "ai_lead_gravity":
									text7 = GetFloatMember(obj9, "m_AIAimLeadFactor_Gravity").ToString("F2");
									break;
								case "ai_lead_drag":
									text7 = GetFloatMember(obj9, "m_AIAimLeadFactor_Drag").ToString("F2");
									break;
								}
							}
							break;
						}
						case "aim_compensation":
						{
							if (!(component != null))
							{
								break;
							}
							FieldInfo field25 = component.GetType().GetField("AimCompensation", bindingAttr);
							if (field25 != null)
							{
								try
								{
									object value13 = field25.GetValue(component);
									text7 = value13?.ToString() + " (" + Convert.ToInt32(value13) + ")";
								}
								catch
								{
								}
							}
							break;
						}
						case "pitch_max_down":
						case "pitch_max_up":
						{
							if (!(component != null))
							{
								break;
							}
							string name8 = ((text6 == "pitch_max_up") ? "PitchMaxUp" : "PitchMaxDown");
							FieldInfo field50 = component.GetType().GetField(name8, bindingAttr);
							if (field50 != null)
							{
								try
								{
									text7 = ((float)field50.GetValue(component)).ToString("0.#") + "°";
								}
								catch
								{
								}
							}
							break;
						}
						case "proj_lifetime_mult":
							if (component != null)
							{
								FieldInfo field31 = component.GetType().GetField("PrimaryProjectile", bindingAttr);
								if (field31 != null)
								{
									object value17 = field31.GetValue(component);
									if (value17 != null)
									{
										float floatMember19 = GetFloatMember(value17, "m_fLifeTime");
										if (floatMember19 > 0f)
										{
											text7 = floatMember19.ToString("F2") + "s";
										}
									}
								}
							}
							if (text7 == null && obj6 != null)
							{
								float floatMember20 = GetFloatMember(obj6, "m_fLifeTime");
								if (floatMember20 > 0f)
								{
									text7 = floatMember20.ToString("F2") + "s";
								}
							}
							break;
						case "accuracy_mult":
							if (component != null)
							{
								FieldInfo field48 = component.GetType().GetField("PrimaryMuzzleSpread", bindingAttr);
								if (field48 != null && field48.FieldType == typeof(float))
								{
									text7 = ((float)field48.GetValue(component)).ToString("F4");
								}
							}
							if (text7 == null && component6 != null)
							{
								float floatMember30 = GetFloatMember(component6, "MuzzleSpread");
								if (floatMember30 >= 0f)
								{
									text7 = floatMember30.ToString("F4");
								}
							}
							break;
						case "magazine_mult":
						{
							if (component != null)
							{
								FieldInfo field21 = component.GetType().GetField("PrimaryMagazineSize", bindingAttr);
								if (field21 != null && field21.FieldType == typeof(int))
								{
									text7 = ((int)field21.GetValue(component)).ToString();
								}
							}
							if (text7 != null || !(component6 != null))
							{
								break;
							}
							FieldInfo field22 = component6.GetType().GetField("MagazineSize", bindingAttr);
							if (field22 != null && field22.FieldType == typeof(int))
							{
								text7 = ((int)field22.GetValue(component6)).ToString();
								break;
							}
							PropertyInfo property = component6.GetType().GetProperty("MagazineSize", bindingAttr);
							if (property != null && property.PropertyType == typeof(int))
							{
								text7 = ((int)property.GetValue(component6)).ToString();
							}
							break;
						}
						case "fire_rate_mult":
						{
							if (component != null)
							{
								FieldInfo field46 = component.GetType().GetField("PrimaryFireInterval", bindingAttr);
								if (field46 != null && field46.FieldType == typeof(float))
								{
									text7 = ((float)field46.GetValue(component)).ToString("F3") + "s";
								}
							}
							if (text7 == null && component6 != null)
							{
								float floatMember28 = GetFloatMember(component6, "FireInterval");
								if (floatMember28 > 0f)
								{
									text7 = floatMember28.ToString("F3") + "s";
								}
							}
							if (text7 != null || !(component5 != null))
							{
								break;
							}
							FieldInfo field47 = component5.GetType().GetField("AttackPrimary", BindingFlags.Instance | BindingFlags.Public);
							if (!(field47 != null))
							{
								break;
							}
							object value29 = field47.GetValue(component5);
							if (value29 != null)
							{
								float floatMember29 = GetFloatMember(value29, "CoolDownTime");
								if (floatMember29 > 0f)
								{
									text7 = floatMember29.ToString("F3") + "s";
								}
							}
							break;
						}
						case "reload_time_mult":
							if (component != null)
							{
								FieldInfo field19 = component.GetType().GetField("PrimaryReloadTime", bindingAttr);
								if (field19 != null && field19.FieldType == typeof(float))
								{
									text7 = ((float)field19.GetValue(component)).ToString("F1") + "s";
								}
							}
							if (text7 == null && component6 != null)
							{
								float floatMember13 = GetFloatMember(component6, "ReloadTime");
								if (floatMember13 > 0f)
								{
									text7 = floatMember13.ToString("F1") + "s";
								}
							}
							break;
						case "move_speed_mult":
							array3 = componentsInChildren;
							foreach (Component component19 in array3)
							{
								if (component19 == null || text7 != null)
								{
									continue;
								}
								string[] array5 = _speedFieldNames;
								foreach (string text10 in array5)
								{
									if (text10 == "TurboSpeed")
									{
										continue;
									}
									FieldInfo field53 = component19.GetType().GetField(text10, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
									if (field53 != null && field53.FieldType == typeof(float))
									{
										float num5 = (float)field53.GetValue(component19);
										if (num5 > 0f)
										{
											text7 = num5.ToString("F1");
											break;
										}
									}
								}
							}
							break;
						case "run_speed_mult":
							if (component3 != null)
							{
								float floatMember34 = GetFloatMember(component3, "RunSpeed");
								if (floatMember34 > 0f)
								{
									text7 = floatMember34.ToString("F1");
								}
							}
							break;
						case "sprint_speed_mult":
							if (component3 != null)
							{
								float floatMember33 = GetFloatMember(component3, "SprintSpeed");
								if (floatMember33 > 0f)
								{
									text7 = floatMember33.ToString("F1");
								}
							}
							break;
						case "jump_speed_mult":
							if (component3 != null)
							{
								FieldInfo field45 = component3.GetType().GetField("JumpSpeed", bindingAttr);
								if (field45 != null && field45.FieldType == typeof(float))
								{
									text7 = ((float)field45.GetValue(component3)).ToString("F1");
								}
							}
							break;
						case "turbo_speed_mult":
							array3 = componentsInChildren;
							foreach (Component component17 in array3)
							{
								if (component17 == null || text7 != null)
								{
									continue;
								}
								FieldInfo field51 = component17.GetType().GetField("TurboSpeed", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
								if (field51 != null && field51.FieldType == typeof(float))
								{
									float num4 = (float)field51.GetValue(component17);
									if (num4 > 0f)
									{
										text7 = num4.ToString("F1");
									}
								}
							}
							break;
						case "fly_speed_mult":
							if (component5 != null)
							{
								float floatMember26 = GetFloatMember(component5, "FlyMoveSpeed");
								if (floatMember26 > 0f)
								{
									text7 = floatMember26.ToString("F1");
								}
							}
							break;
						case "strafe_speed_mult":
							if (component5 != null)
							{
								float floatMember14 = GetFloatMember(component5, "FlyMoveScaleSide");
								if (floatMember14 >= 0f)
								{
									text7 = floatMember14.ToString("F2");
								}
							}
							if (text7 != null)
							{
								break;
							}
							array3 = componentsInChildren;
							foreach (Component component12 in array3)
							{
								if (!(component12 == null) && text7 == null && !(component12.GetType().Name != "VehicleAir"))
								{
									float floatMember15 = GetFloatMember(component12, "StrafeSpeed");
									if (floatMember15 > 0f)
									{
										text7 = floatMember15.ToString("F1");
									}
								}
							}
							break;
						case "turn_radius_mult":
							if (component4 != null)
							{
								FieldInfo field23 = component4.GetType().GetField("TurningCircleRadius", BindingFlags.Instance | BindingFlags.Public);
								if (field23 != null && field23.FieldType == typeof(float))
								{
									text7 = ((float)field23.GetValue(component4)).ToString("F1");
								}
							}
							break;
						case "target_distance":
							if (component2 != null)
							{
								FieldInfo field20 = component2.GetType().GetField("TargetingDistance", bindingAttr);
								if (field20 != null && field20.FieldType == typeof(float))
								{
									text7 = ((float)field20.GetValue(component2)).ToString("F0");
								}
							}
							break;
						case "fow_distance":
							if (component2 != null)
							{
								FieldInfo field54 = component2.GetType().GetField("FogOfWarViewDistance", bindingAttr);
								if (field54 != null && field54.FieldType == typeof(float))
								{
									text7 = ((float)field54.GetValue(component2)).ToString("F0");
								}
							}
							break;
						case "visible_event_radius_mult":
							if (component != null)
							{
								FieldInfo field32 = component.GetType().GetField("PrimaryProjectile", bindingAttr);
								if (field32 != null)
								{
									object value18 = field32.GetValue(component);
									if (value18 != null)
									{
										float floatMember21 = GetFloatMember(value18, "VisibleEventRadius");
										if (floatMember21 > 0f)
										{
											text7 = floatMember21.ToString("F0");
										}
									}
								}
							}
							if (text7 == null && component5 != null)
							{
								FieldInfo field33 = component5.GetType().GetField("AttackPrimary", BindingFlags.Instance | BindingFlags.Public);
								if (field33 != null)
								{
									object value19 = field33.GetValue(component5);
									if (value19 != null)
									{
										FieldInfo field34 = value19.GetType().GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
										if (field34 != null)
										{
											object value20 = field34.GetValue(value19);
											if (value20 != null)
											{
												float floatMember22 = GetFloatMember(value20, "VisibleEventRadius");
												if (floatMember22 > 0f)
												{
													text7 = floatMember22.ToString("F0");
												}
											}
										}
									}
								}
							}
							if (text7 != null)
							{
								break;
							}
							array3 = componentsInChildren;
							foreach (Component component13 in array3)
							{
								if (component13 == null || text7 != null)
								{
									continue;
								}
								FieldInfo[] fields = component13.GetType().GetFields(bindingAttr);
								foreach (FieldInfo fieldInfo in fields)
								{
									if (fieldInfo.FieldType.Name != "ProjectileData")
									{
										continue;
									}
									object value21;
									try
									{
										value21 = fieldInfo.GetValue(component13);
									}
									catch
									{
										continue;
									}
									if (value21 != null)
									{
										float floatMember23 = GetFloatMember(value21, "VisibleEventRadius");
										if (floatMember23 > 0f)
										{
											text7 = floatMember23.ToString("F0");
											break;
										}
									}
								}
							}
							break;
						case "prox_Structures":
						case "prox_SplashRadiusScale":
						case "prox_MinimumTime":
						case "prox_FlyingUnits":
						case "prox_GroundUnits":
						{
							string name5 = text6.Substring(5);
							Component component15 = null;
							string pdName;
							if (obj != null)
							{
								component15 = GetProximityComponent(obj, out pdName);
							}
							if (component15 == null && obj2 != null)
							{
								component15 = GetProximityComponent(obj2, out pdName);
							}
							if (component15 == null && component5 != null)
							{
								string[] array5 = new string[2] { "AttackPrimary", "AttackSecondary" };
								foreach (string name6 in array5)
								{
									FieldInfo field39 = component5.GetType().GetField(name6, BindingFlags.Instance | BindingFlags.Public);
									if (field39 == null)
									{
										continue;
									}
									object value24 = field39.GetValue(component5);
									if (value24 == null)
									{
										continue;
									}
									FieldInfo field40 = value24.GetType().GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
									if (field40 == null)
									{
										continue;
									}
									object value25 = field40.GetValue(value24);
									if (value25 != null)
									{
										component15 = GetProximityComponent(value25, out pdName);
										if (component15 != null)
										{
											break;
										}
									}
								}
							}
							if (!(component15 != null))
							{
								break;
							}
							Type type3 = component15.GetType();
							FieldInfo field41 = type3.GetField(name5, bindingAttr);
							if (field41 != null)
							{
								object value26 = field41.GetValue(component15);
								if (value26 is bool)
								{
									text7 = (((bool)value26) ? "True" : "False");
								}
								else if (value26 is float num2)
								{
									text7 = num2.ToString("F2");
								}
								else if (value26 != null)
								{
									text7 = value26.ToString();
								}
								break;
							}
							PropertyInfo property4 = type3.GetProperty(name5, bindingAttr);
							if (property4 != null)
							{
								object value27 = property4.GetValue(component15);
								if (value27 is bool)
								{
									text7 = (((bool)value27) ? "True" : "False");
								}
								else if (value27 is float num3)
								{
									text7 = num3.ToString("F2");
								}
								else if (value27 != null)
								{
									text7 = value27.ToString();
								}
							}
							break;
						}
						case "dispense_timeout":
							array2 = array;
							foreach (ObjectInfo objectInfo3 in array2)
							{
								if (objectInfo3 == null || objectInfo3.Prefab == null || text7 != null)
								{
									continue;
								}
								array3 = objectInfo3.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
								foreach (Component component14 in array3)
								{
									if (component14 == null || component14.GetType().Name != "VehicleDispenser")
									{
										continue;
									}
									FieldInfo field36 = component14.GetType().GetField("VehicleToDispense", BindingFlags.Instance | BindingFlags.Public);
									if (field36 == null)
									{
										continue;
									}
									object value23 = field36.GetValue(component14);
									if (value23 == null)
									{
										continue;
									}
									string a = null;
									FieldInfo field37 = value23.GetType().GetField("DisplayName", BindingFlags.Instance | BindingFlags.Public);
									if (field37 != null)
									{
										a = field37.GetValue(value23) as string;
									}
									else
									{
										PropertyInfo property2 = value23.GetType().GetProperty("DisplayName", BindingFlags.Instance | BindingFlags.Public);
										if (property2 != null)
										{
											a = property2.GetValue(value23) as string;
										}
									}
									if (!string.Equals(a, unitName, StringComparison.OrdinalIgnoreCase))
									{
										continue;
									}
									FieldInfo field38 = component14.GetType().GetField("DispenseTimeout", BindingFlags.Instance | BindingFlags.Public);
									if (field38 != null && field38.FieldType == typeof(float))
									{
										text7 = ((float)field38.GetValue(component14)).ToString("F1") + "s";
										continue;
									}
									PropertyInfo property3 = component14.GetType().GetProperty("DispenseTimeout", BindingFlags.Instance | BindingFlags.Public);
									if (property3 != null && property3.PropertyType == typeof(float))
									{
										text7 = ((float)property3.GetValue(component14)).ToString("F1") + "s";
									}
								}
							}
							break;
						default:
						{
							bool flag = text6.StartsWith("pri_");
							bool flag2 = text6.StartsWith("sec_");
							if (!flag && !flag2)
							{
								break;
							}
							string text8 = text6.Substring(4);
							string name2 = (flag ? "AttackPrimary" : "AttackSecondary");
							object obj8 = (flag ? obj : obj2);
							Component component10 = (flag ? component8 : component9);
							string text9 = (flag ? text : text2);
							switch (text8)
							{
							case "damage_mult":
							{
								if (obj8 != null)
								{
									float floatMember = GetFloatMember(obj8, "m_fImpactDamage");
									if (floatMember > 0f)
									{
										text7 = floatMember.ToString("F0");
									}
								}
								if (text7 != null || !(component5 != null))
								{
									break;
								}
								FieldInfo field4 = component5.GetType().GetField(name2, BindingFlags.Instance | BindingFlags.Public);
								if (!(field4 != null))
								{
									break;
								}
								object value = field4.GetValue(component5);
								if (value == null)
								{
									break;
								}
								float floatMember2 = GetFloatMember(value, "Damage");
								if (floatMember2 > 0f)
								{
									text7 = floatMember2.ToString("F0");
									break;
								}
								FieldInfo field5 = value.GetType().GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
								if (!(field5 != null))
								{
									break;
								}
								object value2 = field5.GetValue(value);
								if (value2 != null)
								{
									floatMember2 = GetFloatMember(value2, "m_fImpactDamage");
									if (floatMember2 > 0f)
									{
										text7 = floatMember2.ToString("F0");
									}
								}
								break;
							}
							case "impact_damage_mult":
							case "splash_damage_mult":
							case "penetrating_damage_mult":
							case "ricochet_damage_mult":
							{
								string name3 = text8 switch
								{
									"penetrating_damage_mult" => "m_fPenetratingDamage", 
									"splash_damage_mult" => "m_fSplashDamageMax", 
									"impact_damage_mult" => "m_fImpactDamage", 
									_ => "m_fRicochetDamage", 
								};
								if (obj8 != null)
								{
									float floatMember7 = GetFloatMember(obj8, name3);
									if (floatMember7 > 0f)
									{
										text7 = floatMember7.ToString("F0");
									}
								}
								if (text7 != null || !(component5 != null))
								{
									break;
								}
								FieldInfo field12 = component5.GetType().GetField(name2, BindingFlags.Instance | BindingFlags.Public);
								if (!(field12 != null))
								{
									break;
								}
								object value7 = field12.GetValue(component5);
								if (value7 == null)
								{
									break;
								}
								FieldInfo field13 = value7.GetType().GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
								if (!(field13 != null))
								{
									break;
								}
								object value8 = field13.GetValue(value7);
								if (value8 != null)
								{
									float floatMember8 = GetFloatMember(value8, name3);
									if (floatMember8 > 0f)
									{
										text7 = floatMember8.ToString("F0");
									}
								}
								break;
							}
							case "splash_radius_max_mult":
							case "splash_radius_min_mult":
							case "splash_radius_pow_mult":
							{
								string name4;
								if (text8 == "splash_radius_max_mult")
								{
									name4 = "m_fSplashDamageRadiusMax";
								}
								else
								{
									name4 = ((text8 == "splash_radius_min_mult") ? "m_fSplashDamageRadiusMin" : "m_fSplashDamageRadiusPow");
								}
								if (obj8 != null)
								{
									float floatMember9 = GetFloatMember(obj8, name4);
									if (floatMember9 > 0f)
									{
										text7 = floatMember9.ToString("F1");
									}
								}
								if (text7 != null || !(component5 != null))
								{
									break;
								}
								FieldInfo field15 = component5.GetType().GetField(name2, BindingFlags.Instance | BindingFlags.Public);
								if (!(field15 != null))
								{
									break;
								}
								object value9 = field15.GetValue(component5);
								if (value9 == null)
								{
									break;
								}
								FieldInfo field16 = value9.GetType().GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
								if (!(field16 != null))
								{
									break;
								}
								object value10 = field16.GetValue(value9);
								if (value10 != null)
								{
									float floatMember10 = GetFloatMember(value10, name4);
									if (floatMember10 > 0f)
									{
										text7 = floatMember10.ToString("F1");
									}
								}
								break;
							}
							case "proj_speed_mult":
							{
								if (obj8 != null)
								{
									float floatMember11 = GetFloatMember(obj8, "m_fBaseSpeed");
									if (floatMember11 > 0f)
									{
										text7 = floatMember11.ToString("F0");
									}
								}
								if (text7 != null || !(component5 != null))
								{
									break;
								}
								FieldInfo field17 = component5.GetType().GetField(name2, BindingFlags.Instance | BindingFlags.Public);
								if (!(field17 != null))
								{
									break;
								}
								object value11 = field17.GetValue(component5);
								if (value11 == null)
								{
									break;
								}
								FieldInfo field18 = value11.GetType().GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
								if (!(field18 != null))
								{
									break;
								}
								object value12 = field18.GetValue(value11);
								if (value12 != null)
								{
									float floatMember12 = GetFloatMember(value12, "m_fBaseSpeed");
									if (floatMember12 > 0f)
									{
										text7 = floatMember12.ToString("F0");
									}
								}
								break;
							}
							case "proj_lifetime_mult":
							{
								if (obj8 != null)
								{
									float floatMember3 = GetFloatMember(obj8, "m_fLifeTime");
									if (floatMember3 > 0f)
									{
										text7 = floatMember3.ToString("F2") + "s";
									}
								}
								if (text7 != null || !(component5 != null))
								{
									break;
								}
								FieldInfo field6 = component5.GetType().GetField(name2, BindingFlags.Instance | BindingFlags.Public);
								if (!(field6 != null))
								{
									break;
								}
								object value3 = field6.GetValue(component5);
								if (value3 == null)
								{
									break;
								}
								FieldInfo field7 = value3.GetType().GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
								if (!(field7 != null))
								{
									break;
								}
								object value4 = field7.GetValue(value3);
								if (value4 != null)
								{
									float floatMember4 = GetFloatMember(value4, "m_fLifeTime");
									if (floatMember4 > 0f)
									{
										text7 = floatMember4.ToString("F2") + "s";
									}
								}
								break;
							}
							case "accuracy_mult":
							{
								if (component10 != null)
								{
									FieldInfo field8 = component10.GetType().GetField(text9 + "MuzzleSpread", bindingAttr);
									if (field8 != null && field8.FieldType == typeof(float))
									{
										text7 = ((float)field8.GetValue(component10)).ToString("F4");
									}
								}
								if (text7 != null || !(component5 != null))
								{
									break;
								}
								FieldInfo field9 = component5.GetType().GetField(name2, BindingFlags.Instance | BindingFlags.Public);
								if (!(field9 != null))
								{
									break;
								}
								object value5 = field9.GetValue(component5);
								if (value5 != null)
								{
									float floatMember5 = GetFloatMember(value5, "AttackProjectileSpread");
									if (floatMember5 >= 0f)
									{
										text7 = floatMember5.ToString("F4");
									}
								}
								break;
							}
							case "magazine_mult":
								if (component10 != null)
								{
									FieldInfo field14 = component10.GetType().GetField(text9 + "MagazineSize", bindingAttr);
									if (field14 != null && field14.FieldType == typeof(int))
									{
										text7 = ((int)field14.GetValue(component10)).ToString();
									}
								}
								break;
							case "fire_rate_mult":
							{
								if (component10 != null)
								{
									FieldInfo field10 = component10.GetType().GetField(text9 + "FireInterval", bindingAttr);
									if (field10 != null && field10.FieldType == typeof(float))
									{
										text7 = ((float)field10.GetValue(component10)).ToString("F3") + "s";
									}
								}
								if (text7 != null || !(component5 != null))
								{
									break;
								}
								FieldInfo field11 = component5.GetType().GetField(name2, BindingFlags.Instance | BindingFlags.Public);
								if (!(field11 != null))
								{
									break;
								}
								object value6 = field11.GetValue(component5);
								if (value6 != null)
								{
									float floatMember6 = GetFloatMember(value6, "CoolDownTime");
									if (floatMember6 > 0f)
									{
										text7 = floatMember6.ToString("F3") + "s";
									}
								}
								break;
							}
							case "reload_time_mult":
								if (component10 != null)
								{
									FieldInfo field3 = component10.GetType().GetField(text9 + "ReloadTime", bindingAttr);
									if (field3 != null && field3.FieldType == typeof(float))
									{
										text7 = ((float)field3.GetValue(component10)).ToString("F1") + "s";
									}
								}
								break;
							}
							break;
						}
						}
						if (text7 != null)
						{
							dictionary[text6] = text7;
						}
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
			return dictionary;
		}

		private static void ShowCurrentMenu(object player, BalanceMenuState state)
		{
			switch (state.Level)
			{
			case MenuLevel.Root:
				SendChatToPlayer(player, _chatPrefix + _itemColor + "1.</color> Sol  " + _itemColor + "2.</color> Centauri  " + _itemColor + "3.</color> Alien  " + _itemColor + "4.</color> JSON  " + _itemColor + "5.</color> HTP  " + _itemColor + "6.</color> Cap&Decay  " + _itemColor + "7.</color> Strategystyle");
				break;
			case MenuLevel.Faction:
			{
				string text16 = (new string[3] { "Sol", "Centauri", "Alien" })[state.FactionIdx];
				SendChatToPlayer(player, _chatPrefix + _headerColor + text16 + "</color>");
				string[] array2 = _categoryNames[state.FactionIdx];
				string text17 = "";
				for (int k = 0; k < array2.Length; k++)
				{
					if (k > 0)
					{
						text17 += "  ";
					}
					text17 = text17 + _itemColor + (k + 1) + ".</color> " + array2[k];
				}
				SendChatToPlayer(player, _chatPrefix + text17);
				break;
			}
			case MenuLevel.Category:
			{
				string text22 = (new string[3] { "Sol", "Centauri", "Alien" })[state.FactionIdx];
				string text23 = _categoryNames[state.FactionIdx][state.CategoryIdx];
				SendChatToPlayer(player, _chatPrefix + _headerColor + text22 + " > " + text23 + "</color>");
				string[] array4 = _unitNames[state.FactionIdx][state.CategoryIdx];
				for (int m = 0; m < array4.Length; m += 3)
				{
					string text24 = "";
					for (int n = m; n < Math.Min(m + 3, array4.Length); n++)
					{
						if (n > m)
						{
							text24 += "  ";
						}
						text24 = text24 + _itemColor + (n + 1) + ".</color> " + array4[n];
					}
					SendChatToPlayer(player, _chatPrefix + text24);
				}
				break;
			}
			case MenuLevel.Unit:
			{
				string text7 = _unitNames[state.FactionIdx][state.CategoryIdx][state.UnitIdx];
				GetUnitParamGroups(text7, out string[] groupNames2, out string[][] _);
				SendChatToPlayer(player, _chatPrefix + _headerColor + text7 + "</color>");
				for (int j = 0; j < groupNames2.Length; j++)
				{
					SendChatToPlayer(player, _chatPrefix + _itemColor + (j + 1) + ".</color> " + groupNames2[j]);
				}
				break;
			}
			case MenuLevel.ParamGroup:
			{
				string text2 = _unitNames[state.FactionIdx][state.CategoryIdx][state.UnitIdx];
				GetUnitParamGroups(text2, out string[] groupNames, out string[][] groupKeys);
				if (state.ParamGroupIdx >= groupNames.Length)
				{
					state.Level = MenuLevel.Unit;
					ShowCurrentMenu(player, state);
					break;
				}
				string text3 = groupNames[state.ParamGroupIdx];
				SendChatToPlayer(player, _chatPrefix + _headerColor + text2 + " > " + text3 + "</color>");
				string[] array = groupKeys[state.ParamGroupIdx];
				JObject jObject = ReadUnitConfigFromJson(text2);
				Dictionary<string, string> baseValues = GetBaseValues(text2, array);
				for (int i = 0; i < array.Length; i++)
				{
					string text4 = "-";
					if (array[i].StartsWith("prox_"))
					{
						text4 = ReadProximityConfigValue(jObject, array[i].Substring(5));
					}
					else if (jObject != null && jObject.ContainsKey(array[i]))
					{
						text4 = jObject[array[i]].ToString();
					}
					if (IsBoolMenuParam(array[i]))
					{
						text4 = FmtBoolMenuVal(array[i], text4);
					}
					else if (array[i] == "aim_compensation" && text4 != "-")
					{
						text4 = FmtAimCompVal(text4);
					}
					string text5 = "";
					if (baseValues.TryGetValue(array[i], out var value))
					{
						text5 = " " + _dimColor + "(base: " + value + ")</color>";
					}
					string text6 = array[i];
					if (text6.StartsWith("pri_") || text6.StartsWith("sec_"))
					{
						text6 = text6.Substring(4);
					}
					else if (text6.StartsWith("prox_"))
					{
						text6 = text6.Substring(5);
					}
					SendChatToPlayer(player, _chatPrefix + _itemColor + (i + 1) + ".</color> " + text6 + " = " + _valueColor + text4 + "</color>" + text5);
				}
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .1 1.5  ·  toggle 1=ON/0=OFF  ·  aim_compensation 0=None 1/3=Primary(Vert/Full) 2/4=Secondary</color>");
				break;
			}
			case MenuLevel.JsonMenu:
				SendChatToPlayer(player, _chatPrefix + _headerColor + "JSON Config</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "1.</color> Reset to Blank " + _dimColor + "(vanilla settings)</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "2.</color> Save Current Config");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "3.</color> Load Saved Config");
				break;
			case MenuLevel.JsonLoad:
			{
				SendChatToPlayer(player, _chatPrefix + _headerColor + "Load Saved Config</color>");
				if (state.JsonLoadFileList == null || state.JsonLoadFileList.Length == 0)
				{
					SendChatToPlayer(player, _chatPrefix + _dimColor + "No saved configs found.</color>");
					SendChatToPlayer(player, _chatPrefix + _dimColor + "Use /back to return.</color>");
					break;
				}
				for (int num6 = 0; num6 < state.JsonLoadFileList.Length; num6++)
				{
					SendChatToPlayer(player, _chatPrefix + _itemColor + (num6 + 1) + ".</color> " + state.JsonLoadFileList[num6]);
				}
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Pick a number to load.</color>");
				break;
			}
			case MenuLevel.HTPMenu:
			{
				SendChatToPlayer(player, _chatPrefix + _headerColor + "HTP</color> " + _dimColor + "(Hover · Tier · Teleportation · misc)</color>");
				string text31 = (_shrimpDisableAim ? "<color=#FF5555>OFF</color>" : "<color=#55FF55>ON</color>");
				string text32 = (_additionalSpawn ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "1.</color> Hoverbike  " + _itemColor + "2.</color> Tier  " + _itemColor + "3.</color> Teleportation  " + _itemColor + "4.</color> Shrimp Aim [" + text31 + "]");
				string text33 = (_watchdogEnabled ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "5.</color> Additional Spawn [" + text32 + "]  " + _itemColor + "6.</color> Discord");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "7.</color> Watchdog [" + text33 + "]");
				break;
			}
			case MenuLevel.CapDecayMenu:
			{
				SendChatToPlayer(player, _chatPrefix + _headerColor + "Cap&Decay</color> " + _dimColor + "(unit cap · structure decay · count caps)</color>");
				string text8 = (_decayHuman.Enabled ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
				string text9 = (_decayAlien.Enabled ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "1.</color> Decay " + _dimColor + "(Human [</color>" + text8 + _dimColor + "] · Alien [</color>" + text9 + _dimColor + "])</color>");
				string text10 = (_playerInfantryIgnoreCap ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "2.</color> Player Cap Exempt [" + text10 + "] " + _dimColor + "(player soldiers don't fill cap)</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "3.</color> Unit Cap Mult 2-way = " + _valueColor + FmtCapMult(_unitCapMult2Way) + "</color> " + _dimColor + "(HvH, HvA)</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "4.</color> Unit Cap Mult 3-way = " + _valueColor + FmtCapMult(_unitCapMult3Way) + "</color> " + _dimColor + "(HvHvA, H2vA)</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "5.</color> Shrimp Cap = " + _valueColor + FmtCountCap(_countCapShrimp) + "</color>  " + _itemColor + "6.</color> Harvester Cap = " + _valueColor + FmtCountCap(_countCapHarvester) + "</color> " + _dimColor + "(max per team)</color>");
				break;
			}
			case MenuLevel.StrategyMenu:
			{
				SendChatToPlayer(player, _chatPrefix + _headerColor + "Strategystyle</color> " + _dimColor + "(alien nest-centric build rules)</color>");
				string text25 = (_alienNestOnlyBuild ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "1.</color> Alien Nest-Only Build [" + text25 + "] " + _dimColor + "(only Nests anchor new buildings; !rebalance)</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "2.</color> Alien Nest Reach Radius = " + _valueColor + FmtNestReach(_alienNestReachRadius) + "</color> " + _dimColor + "(Nodes/Bio Caches only this close to a Nest)</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "3.</color> Alien Queens = " + _valueColor + FmtQueensMax(_alienQueensMax) + "</color> " + _dimColor + "(Queens per team; new Nests spawn one until reached)</color>");
				string text26 = (_alienNestFinishedAnchorOnly ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "4.</color> Alien Nest Finished-Anchor Only [" + text26 + "] " + _dimColor + "(a new Nest can't anchor to a Nest still under construction)</color>");
				string text27 = (_alienQueenLogisticsPerNest ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "5.</color> Queen Logistics per Nest [" + text27 + "] " + _dimColor + "(structures build only while their nearest Nest's Queen is docked; needs 3. = 0)</color>");
				break;
			}
			case MenuLevel.HTPHoverbike:
			{
				GetUnitParamGroups("Hover Bike", out string[] groupNames4, out string[][] _);
				SendChatToPlayer(player, _chatPrefix + _headerColor + "HTP > Hoverbike</color>");
				for (int num3 = 0; num3 < groupNames4.Length; num3++)
				{
					SendChatToPlayer(player, _chatPrefix + _itemColor + (num3 + 1) + ".</color> " + groupNames4[num3]);
				}
				break;
			}
			case MenuLevel.HTPHoverbikeParam:
			{
				GetUnitParamGroups("Hover Bike", out string[] groupNames3, out string[][] groupKeys3);
				if (state.HTPParamGroupIdx >= groupNames3.Length)
				{
					state.Level = MenuLevel.HTPHoverbike;
					ShowCurrentMenu(player, state);
					break;
				}
				string text18 = groupNames3[state.HTPParamGroupIdx];
				SendChatToPlayer(player, _chatPrefix + _headerColor + "Hover Bike > " + text18 + "</color>");
				string[] array3 = groupKeys3[state.HTPParamGroupIdx];
				JObject jObject2 = ReadUnitConfigFromJson("Hover Bike");
				Dictionary<string, string> baseValues2 = GetBaseValues("Hover Bike", array3);
				for (int l = 0; l < array3.Length; l++)
				{
					string text19 = "-";
					if (jObject2 != null && jObject2.ContainsKey(array3[l]))
					{
						text19 = jObject2[array3[l]].ToString();
					}
					if (IsBoolMenuParam(array3[l]))
					{
						text19 = FmtBoolMenuVal(array3[l], text19);
					}
					else if (array3[l] == "aim_compensation" && text19 != "-")
					{
						text19 = FmtAimCompVal(text19);
					}
					string text20 = "";
					if (baseValues2.TryGetValue(array3[l], out var value2))
					{
						text20 = " " + _dimColor + "(base: " + value2 + ")</color>";
					}
					string text21 = array3[l];
					if (text21.StartsWith("pri_") || text21.StartsWith("sec_"))
					{
						text21 = text21.Substring(4);
					}
					SendChatToPlayer(player, _chatPrefix + _itemColor + (l + 1) + ".</color> " + text21 + " = " + _valueColor + text19 + "</color>" + text20);
				}
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .1 1.5 (or !b 1 1.5)</color>");
				break;
			}
			case MenuLevel.HTPTier:
				SendChatToPlayer(player, _chatPrefix + _headerColor + "HTP > Tier</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "1.</color> Time " + _dimColor + "(seconds per tech-up)</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "2.</color> Cost " + _dimColor + "(resources per tech-up; -1 = vanilla)</color>");
				break;
			case MenuLevel.HTPTierTime:
			{
				SendChatToPlayer(player, _chatPrefix + _headerColor + "HTP > Tier > Time</color> " + _dimColor + "(seconds per tech-up)</color>");
				JObject jObject4 = null;
				try
				{
					if (File.Exists(_configPath))
					{
						jObject4 = JObject.Parse(File.ReadAllText(_configPath))["tech_time"] as JObject;
					}
				}
				catch
				{
				}
				for (int num4 = 1; num4 <= 8; num4++)
				{
					string text38 = "-";
					if (jObject4 != null)
					{
						float? num5 = jObject4[$"tier_{num4}"]?.Value<float>();
						if (num5.HasValue)
						{
							text38 = num5.Value.ToString("F0") + "s";
						}
					}
					SendChatToPlayer(player, _chatPrefix + _itemColor + num4 + ".</color> Tier " + num4 + " = " + _valueColor + text38 + "</color> " + _dimColor + "(default: 30s)</color>");
				}
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .N <seconds>  ·  /back to picker  ·  /0 close</color>");
				break;
			}
			case MenuLevel.HTPTierCost:
			{
				SendChatToPlayer(player, _chatPrefix + _headerColor + "HTP > Tier > Cost</color> " + _dimColor + "(resources per tech-up; -1 = vanilla)</color>");
				JObject jObject3 = null;
				try
				{
					if (File.Exists(_configPath))
					{
						jObject3 = JObject.Parse(File.ReadAllText(_configPath))["tech_cost"] as JObject;
					}
				}
				catch
				{
				}
				for (int num = 1; num <= 8; num++)
				{
					string text28 = "vanilla";
					if (jObject3 != null)
					{
						int? num2 = jObject3[$"tier_{num}"]?.Value<int>();
						if (num2.HasValue && num2.Value >= 0)
						{
							text28 = num2.Value.ToString();
						}
					}
					SendChatToPlayer(player, _chatPrefix + _itemColor + num + ".</color> Tier " + num + " = " + _valueColor + text28 + "</color>");
				}
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .N <resource>  ·  -1 = vanilla  ·  /back to picker  ·  /0 close</color>");
				break;
			}
			case MenuLevel.HTPTeleport:
			{
				SendChatToPlayer(player, _chatPrefix + _headerColor + "HTP > Teleportation</color>");
				JObject jObject5 = ReadUnitConfigFromJson("_teleport");
				string text39 = "-";
				string text40 = "-";
				if (jObject5 != null)
				{
					if (jObject5.ContainsKey("cooldown"))
					{
						text39 = jObject5["cooldown"].ToString() + "s";
					}
					if (jObject5.ContainsKey("duration"))
					{
						text40 = jObject5["duration"].ToString() + "s";
					}
				}
				SendChatToPlayer(player, _chatPrefix + _itemColor + "1.</color> Cooldown = " + _valueColor + text39 + "</color> " + _dimColor + "(base: 120s)</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "2.</color> Duration = " + _valueColor + text40 + "</color> " + _dimColor + "(base: 5s)</color>");
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .1 90 (or !b 1 90)</color>");
				break;
			}
			case MenuLevel.HTPDecay:
			{
				string text29 = (_decayHuman.Enabled ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
				string text30 = (_decayAlien.Enabled ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
				SendChatToPlayer(player, _chatPrefix + _headerColor + "Cap&Decay > Decay</color> " + _dimColor + "(structure decay when unanchored)</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "1.</color> Human (Sol/Centauri) [" + text29 + "]");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "2.</color> Alien [" + text30 + "]");
				break;
			}
			case MenuLevel.HTPDecayFaction:
			{
				bool flag2 = state.HTPDecayFactionIdx == 0;
				string text13 = (flag2 ? "Human" : "Alien");
				DecaySettings decaySettings = (flag2 ? _decayHuman : _decayAlien);
				string text14 = (decaySettings.Enabled ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
				SendChatToPlayer(player, _chatPrefix + _headerColor + "Decay > " + text13 + "</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "1.</color> Enabled [" + text14 + "]");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "2.</color> delay = " + _valueColor + ((decaySettings.Delay >= 0f) ? (decaySettings.Delay.ToString("F1") + "s") : "-") + "</color> " + _dimColor + "(base: 10s)</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "3.</color> tick = " + _valueColor + ((decaySettings.Tick >= 0f) ? (decaySettings.Tick.ToString("F1") + "s") : "-") + "</color> " + _dimColor + "(base: 5s)</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "4.</color> amount_pct = " + _valueColor + ((decaySettings.AmountPct >= 0f) ? decaySettings.AmountPct.ToString("F4") : "-") + "</color> " + _dimColor + "(base: 0.01)</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "5.</color> randomize_pct = " + _valueColor + ((decaySettings.RandomizePct >= 0f) ? decaySettings.RandomizePct.ToString("F2") : "-") + "</color> " + _dimColor + "(base: 0.2)</color>");
				string text15 = (decaySettings.KeepProduction ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "6.</color> Keep Production [" + text15 + "]");
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .2 30 (delay=30s) or .4 0.05 (5% per tick)</color>");
				break;
			}
			case MenuLevel.HTPUnitCap:
			{
				string text34 = "-";
				string text35 = "-";
				try
				{
					GameModeExt gameModeExt = GameMode.CurrentGameMode as GameModeExt;
					if (gameModeExt != null)
					{
						text34 = gameModeExt.TeamsVersus.ToString();
						text35 = FmtCapMult(gameModeExt.UnitCapMultiplier);
					}
				}
				catch
				{
				}
				bool flag3 = state.HTPUnitCapModeIdx == 1;
				string text36 = (flag3 ? "3-way" : "2-way");
				string text37 = (flag3 ? "HvHvA, H2vA" : "HvH, HvA");
				int m2 = (flag3 ? _unitCapMult3Way : _unitCapMult2Way);
				SendChatToPlayer(player, _chatPrefix + _headerColor + "Cap&Decay > Unit Cap Mult " + text36 + "</color> " + _dimColor + "(" + text37 + ")</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "1.</color> mult = " + _valueColor + FmtCapMult(m2) + "</color> " + _dimColor + "(CapBase x mult for every cap entry)</color>");
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Live: " + text34 + " -> " + text35 + " · -1 = server setting, 0 = unlimited</color>");
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .1 3 (x3) · .1 -1 (server) · .1 0 (unlimited)</color>");
				break;
			}
			case MenuLevel.CountCap:
			{
				bool flag = state.CountCapIdx == 1;
				string text11 = (flag ? "Harvester" : "Shrimp");
				string text12 = (flag ? "Sol + Centauri harvesters, per team" : "alien Shrimps, per team");
				int v = (flag ? _countCapHarvester : _countCapShrimp);
				SendChatToPlayer(player, _chatPrefix + _headerColor + "Cap&Decay > " + text11 + " Cap</color> " + _dimColor + "(" + text12 + ")</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "1.</color> max = " + _valueColor + FmtCountCap(v) + "</color> " + _dimColor + "(alive + queued + in production)</color>");
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Commander gets a chat warning when the cap blocks a build. Applies immediately on save.</color>");
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .1 12 (max 12) · .1 -1 (unlimited)</color>");
				break;
			}
			case MenuLevel.NestReach:
				SendChatToPlayer(player, _chatPrefix + _headerColor + "Strategystyle > Alien Nest Reach Radius</color> " + _dimColor + "(nest-centric expansion)</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "1.</color> radius = " + _valueColor + FmtNestReach(_alienNestReachRadius) + "</color> " + _dimColor + "(2D distance to nearest Nest, built or under construction)</color>");
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Nodes and Bio Caches further away are refused; the commander gets a chat warning + sound. Other alien buildings are unaffected. Applies immediately on save.</color>");
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .1 250 (250 m) · .1 -1 (vanilla, no limit)</color>");
				break;
			case MenuLevel.QueensMax:
				SendChatToPlayer(player, _chatPrefix + _headerColor + "Strategystyle > Alien Queens</color> " + _dimColor + "(a Queen per Nest)</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "1.</color> max = " + _valueColor + FmtQueensMax(_alienQueensMax) + "</color> " + _dimColor + "(Queens per team; every newly built Nest spawns a docked Queen while below max)</color>");
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Vanilla = 1 (only the starting Nest). Server-side, applies to Nests built after saving.</color>");
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .1 0 (every Nest) · .1 3 (at most 3) · .1 -1 (vanilla)</color>");
				break;
			case MenuLevel.DiscordMenu:
			{
				SendChatToPlayer(player, _chatPrefix + _headerColor + "Discord Webhook</color>");
				string text = (_discordAutoPost ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "1.</color> Auto-post on Rebalance [" + text + "]");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "2.</color> Push Balance Changes Now");
				break;
			}
			}
		}

		private static void HandleParamEdit(object player, BalanceMenuState state, int paramIdx, string valueStr)
		{
			GetUnitParamGroups(_unitNames[state.FactionIdx][state.CategoryIdx][state.UnitIdx], out string[] _, out string[][] groupKeys);
			if (state.ParamGroupIdx >= groupKeys.Length)
			{
				return;
			}
			string[] array = groupKeys[state.ParamGroupIdx];
			if (paramIdx < 1 || paramIdx > array.Length)
			{
				SendChatToPlayer(player, _chatPrefix + $"<color=#FF5555>Pick 1-{array.Length}.</color>");
				return;
			}
			if (!float.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Invalid number: " + valueStr + "</color>");
				return;
			}
			string text = array[paramIdx - 1];
			string text2 = _unitNames[state.FactionIdx][state.CategoryIdx][state.UnitIdx];
			JObject jObject = ReadUnitConfigFromJson(text2);
			string text3 = "-";
			if (jObject != null && jObject.ContainsKey(text))
			{
				text3 = jObject[text].ToString();
			}
			string raw = result.ToString(CultureInfo.InvariantCulture);
			state.PendingConfirm = true;
			state.PendingParamKey = text;
			state.PendingValue = result;
			state.PendingOldVal = text3;
			SendChatToPlayer(player, _chatPrefix + _headerColor + "Confirm:</color> " + text2 + " " + text + " " + _valueColor + FmtBoolCompact(text, text3) + "</color> -> " + _valueColor + FmtBoolCompact(text, raw) + "</color>");
			SendChatToPlayer(player, _chatPrefix + _itemColor + "/1</color>=Save  " + _itemColor + "/2</color>=Cancel  " + _itemColor + "/3</color>=Save+Rebalance");
		}

		private static void HandleHTPHoverbikeEdit(object player, BalanceMenuState state, int paramIdx, string valueStr)
		{
			GetUnitParamGroups("Hover Bike", out string[] _, out string[][] groupKeys);
			if (state.HTPParamGroupIdx >= groupKeys.Length)
			{
				return;
			}
			string[] array = groupKeys[state.HTPParamGroupIdx];
			if (paramIdx < 1 || paramIdx > array.Length)
			{
				SendChatToPlayer(player, _chatPrefix + $"<color=#FF5555>Pick 1-{array.Length}.</color>");
				return;
			}
			if (!float.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Invalid number: " + valueStr + "</color>");
				return;
			}
			string text = array[paramIdx - 1];
			JObject jObject = ReadUnitConfigFromJson("Hover Bike");
			string text2 = "-";
			if (jObject != null && jObject.ContainsKey(text))
			{
				text2 = jObject[text].ToString();
			}
			string text3 = result.ToString(CultureInfo.InvariantCulture);
			state.PendingConfirm = true;
			state.PendingParamKey = text;
			state.PendingValue = result;
			state.PendingOldVal = text2;
			state.PendingUnitName = "Hover Bike";
			SendChatToPlayer(player, _chatPrefix + _headerColor + "Confirm:</color> Hover Bike " + text + " " + _valueColor + text2 + "</color> -> " + _valueColor + text3 + "</color>");
			SendChatToPlayer(player, _chatPrefix + _itemColor + "/1</color>=Save  " + _itemColor + "/2</color>=Cancel  " + _itemColor + "/3</color>=Save+Rebalance");
		}

		private static void HandleHTPTierEdit(object player, BalanceMenuState state, int tierNum, string valueStr)
		{
			if (tierNum < 1 || tierNum > 8)
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick tier 1-8.</color>");
				return;
			}
			if (!float.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Invalid number: " + valueStr + "</color>");
				return;
			}
			string text = $"tier_{tierNum}";
			string text2 = "-";
			try
			{
				if (File.Exists(_configPath) && JObject.Parse(File.ReadAllText(_configPath))["tech_time"] is JObject jObject)
				{
					float? num = jObject[text]?.Value<float>();
					if (num.HasValue)
					{
						text2 = num.Value.ToString("F0");
					}
				}
			}
			catch
			{
			}
			string text3 = result.ToString(CultureInfo.InvariantCulture);
			state.PendingConfirm = true;
			state.PendingParamKey = text;
			state.PendingValue = result;
			state.PendingOldVal = text2;
			state.PendingTechTierKey = text;
			SendChatToPlayer(player, _chatPrefix + _headerColor + "Confirm:</color> Tier " + tierNum + " tech time " + _valueColor + text2 + "</color> -> " + _valueColor + text3 + "s</color>");
			SendChatToPlayer(player, _chatPrefix + _itemColor + "/1</color>=Save  " + _itemColor + "/2</color>=Cancel  " + _itemColor + "/3</color>=Save+Rebalance");
		}

		private static void HandleHTPTeleportEdit(object player, BalanceMenuState state, int paramIdx, string valueStr)
		{
			if (paramIdx < 1 || paramIdx > 2)
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick 1-2.</color>");
				return;
			}
			if (!float.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Invalid number: " + valueStr + "</color>");
				return;
			}
			string text = ((paramIdx == 1) ? "cooldown" : "duration");
			string text2 = ((paramIdx == 1) ? "Cooldown" : "Duration");
			JObject jObject = ReadUnitConfigFromJson("_teleport");
			string text3 = "-";
			if (jObject != null && jObject.ContainsKey(text))
			{
				text3 = jObject[text].ToString();
			}
			string text4 = result.ToString(CultureInfo.InvariantCulture);
			state.PendingConfirm = true;
			state.PendingParamKey = text;
			state.PendingValue = result;
			state.PendingOldVal = text3;
			state.PendingUnitName = "_teleport";
			SendChatToPlayer(player, _chatPrefix + _headerColor + "Confirm:</color> Teleport " + text2 + " " + _valueColor + text3 + "</color> -> " + _valueColor + text4 + "s</color>");
			SendChatToPlayer(player, _chatPrefix + _itemColor + "/1</color>=Save  " + _itemColor + "/2</color>=Cancel  " + _itemColor + "/3</color>=Save+Rebalance");
		}

		private static void HandleHTPDecayEdit(object player, BalanceMenuState state, int paramIdx, string valueStr)
		{
			if (paramIdx < 2 || paramIdx > 5)
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick 2-5 (use .1 to toggle enabled).</color>");
				return;
			}
			if (!float.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Invalid number: " + valueStr + "</color>");
				return;
			}
			bool flag = state.HTPDecayFactionIdx == 0;
			string text = (flag ? "human" : "alien");
			string text2 = (flag ? "Human" : "Alien");
			DecaySettings decaySettings = (flag ? _decayHuman : _decayAlien);
			string text3;
			string text4;
			switch (paramIdx)
			{
			default:
				return;
			case 2:
				text3 = "delay";
				text4 = ((decaySettings.Delay >= 0f) ? decaySettings.Delay.ToString("F1") : "-");
				break;
			case 3:
				text3 = "tick";
				text4 = ((decaySettings.Tick >= 0f) ? decaySettings.Tick.ToString("F1") : "-");
				break;
			case 4:
				text3 = "amount_pct";
				text4 = ((decaySettings.AmountPct >= 0f) ? decaySettings.AmountPct.ToString("F4") : "-");
				break;
			case 5:
				text3 = "randomize_pct";
				text4 = ((decaySettings.RandomizePct >= 0f) ? decaySettings.RandomizePct.ToString("F2") : "-");
				break;
			}
			string text5 = result.ToString(CultureInfo.InvariantCulture);
			state.PendingConfirm = true;
			state.PendingParamKey = "decay_" + text + "_" + text3;
			state.PendingValue = result;
			state.PendingOldVal = text4;
			state.PendingUnitName = "_decay_" + text;
			SendChatToPlayer(player, _chatPrefix + _headerColor + "Confirm:</color> Decay " + text2 + " " + text3 + " " + _valueColor + text4 + "</color> -> " + _valueColor + text5 + "</color>");
			SendChatToPlayer(player, _chatPrefix + _itemColor + "/1</color>=Save  " + _itemColor + "/2</color>=Cancel  " + _itemColor + "/3</color>=Save+Rebalance");
		}

		private static void HandleHTPUnitCapEdit(object player, BalanceMenuState state, int paramIdx, string valueStr)
		{
			if (paramIdx != 1)
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Use .1 <mult> on this page.</color>");
				return;
			}
			bool flag = state.HTPUnitCapModeIdx == 1;
			if (!int.TryParse(valueStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) || result < -1 || result > 255)
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Invalid value: " + valueStr + " (integer -1..255)</color>");
				return;
			}
			string text = (flag ? "three_way" : "two_way");
			string text2 = (flag ? "3-way" : "2-way");
			int m = (flag ? _unitCapMult3Way : _unitCapMult2Way);
			state.PendingConfirm = true;
			state.PendingParamKey = "unitcapmult_" + text;
			state.PendingValue = result;
			state.PendingOldVal = m.ToString(CultureInfo.InvariantCulture);
			state.PendingUnitName = "_unit_cap_mult";
			SendChatToPlayer(player, _chatPrefix + _headerColor + "Confirm:</color> Unit Cap Mult " + text2 + " " + _valueColor + FmtCapMult(m) + "</color> -> " + _valueColor + FmtCapMult(result) + "</color>");
			SendChatToPlayer(player, _chatPrefix + _itemColor + "/1</color>=Save  " + _itemColor + "/2</color>=Cancel  " + _itemColor + "/3</color>=Save+Rebalance");
		}

		private static void HandleCountCapEdit(object player, BalanceMenuState state, int paramIdx, string valueStr)
		{
			if (paramIdx != 1)
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Use .1 <max> on this page.</color>");
				return;
			}
			if (!int.TryParse(valueStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) || result < -1 || result > 1000)
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Invalid value: " + valueStr + " (integer -1..1000)</color>");
				return;
			}
			bool flag = state.CountCapIdx == 1;
			string text = (flag ? "harvester" : "shrimp");
			string text2 = (flag ? "Harvester" : "Shrimp");
			int v = (flag ? _countCapHarvester : _countCapShrimp);
			state.PendingConfirm = true;
			state.PendingParamKey = "countcap_" + text;
			state.PendingValue = result;
			state.PendingOldVal = v.ToString(CultureInfo.InvariantCulture);
			state.PendingUnitName = "_count_caps";
			SendChatToPlayer(player, _chatPrefix + _headerColor + "Confirm:</color> " + text2 + " Cap " + _valueColor + FmtCountCap(v) + "</color> -> " + _valueColor + FmtCountCap(result) + "</color>");
			SendChatToPlayer(player, _chatPrefix + _itemColor + "/1</color>=Save  " + _itemColor + "/2</color>=Cancel  " + _itemColor + "/3</color>=Save+Rebalance");
		}

		private static void HandleNestReachEdit(object player, BalanceMenuState state, int paramIdx, string valueStr)
		{
			if (paramIdx != 1)
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Use .1 <metres> on this page.</color>");
				return;
			}
			if (!float.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) || float.IsNaN(result) || (result != -1f && (!(result >= 1f) || !(result <= 100000f))))
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Invalid value: " + valueStr + " (metres 1..100000, or -1 = vanilla)</color>");
				return;
			}
			result = (float)Math.Round(result);
			state.PendingConfirm = true;
			state.PendingParamKey = "nestreach";
			state.PendingValue = result;
			state.PendingOldVal = _alienNestReachRadius.ToString(CultureInfo.InvariantCulture);
			state.PendingUnitName = "_alien_nest_reach";
			SendChatToPlayer(player, _chatPrefix + _headerColor + "Confirm:</color> Alien Nest Reach Radius " + _valueColor + FmtNestReach(_alienNestReachRadius) + "</color> -> " + _valueColor + FmtNestReach(result) + "</color>");
			SendChatToPlayer(player, _chatPrefix + _itemColor + "/1</color>=Save  " + _itemColor + "/2</color>=Cancel  " + _itemColor + "/3</color>=Save+Rebalance");
		}

		private static void HandleQueensMaxEdit(object player, BalanceMenuState state, int paramIdx, string valueStr)
		{
			if (paramIdx != 1)
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Use .1 <n> on this page.</color>");
				return;
			}
			if (!int.TryParse(valueStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) || result < -1 || result > 100)
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Invalid value: " + valueStr + " (integer -1..100; 0 = every Nest)</color>");
				return;
			}
			state.PendingConfirm = true;
			state.PendingParamKey = "queensmax";
			state.PendingValue = result;
			state.PendingOldVal = _alienQueensMax.ToString(CultureInfo.InvariantCulture);
			state.PendingUnitName = "_alien_queens";
			SendChatToPlayer(player, _chatPrefix + _headerColor + "Confirm:</color> Alien Queens " + _valueColor + FmtQueensMax(_alienQueensMax) + "</color> -> " + _valueColor + FmtQueensMax(result) + "</color>");
			SendChatToPlayer(player, _chatPrefix + _itemColor + "/1</color>=Save  " + _itemColor + "/2</color>=Cancel  " + _itemColor + "/3</color>=Save+Rebalance");
		}

		private static bool WriteNumberToJson(string key, float value)
		{
			try
			{
				if (!File.Exists(_configPath))
				{
					return false;
				}
				JObject jObject = JObject.Parse(File.ReadAllText(_configPath));
				jObject[key] = ((Math.Abs((double)value - Math.Round(value)) < 9.999999747378752E-05) ? ((JToken)(int)Math.Round(value)) : ((JToken)value));
				File.WriteAllText(_configPath, jObject.ToString());
				return true;
			}
			catch (Exception ex)
			{
				MelonLogger.Error("WriteNumberToJson: " + ex.Message);
				return false;
			}
		}

		private static bool WriteCountCapToJson(string paramKey, int value)
		{
			try
			{
				if (!File.Exists(_configPath))
				{
					return false;
				}
				JObject jObject = JObject.Parse(File.ReadAllText(_configPath));
				JObject jObject2 = jObject["count_caps"] as JObject;
				if (jObject2 == null)
				{
					jObject2 = (JObject)(jObject["count_caps"] = new JObject());
				}
				jObject2[paramKey] = value;
				File.WriteAllText(_configPath, jObject.ToString());
				return true;
			}
			catch (Exception ex)
			{
				MelonLogger.Error("WriteCountCapToJson: " + ex.Message);
				return false;
			}
		}

		private static bool WriteUnitCapMultToJson(string paramKey, int value)
		{
			try
			{
				if (!File.Exists(_configPath))
				{
					return false;
				}
				JObject jObject = JObject.Parse(File.ReadAllText(_configPath));
				JObject jObject2 = jObject["unit_cap_mult"] as JObject;
				if (jObject2 == null)
				{
					jObject2 = (JObject)(jObject["unit_cap_mult"] = new JObject());
				}
				jObject2[paramKey] = value;
				File.WriteAllText(_configPath, jObject.ToString());
				return true;
			}
			catch (Exception ex)
			{
				MelonLogger.Error("WriteUnitCapMultToJson: " + ex.Message);
				return false;
			}
		}

		private static bool WriteDecayToJson(string factionKey, string paramKey, float value)
		{
			try
			{
				if (!File.Exists(_configPath))
				{
					return false;
				}
				JObject jObject = JObject.Parse(File.ReadAllText(_configPath));
				JObject jObject2 = jObject["decay"] as JObject;
				if (jObject2 == null)
				{
					jObject2 = (JObject)(jObject["decay"] = new JObject());
				}
				JObject jObject3 = jObject2[factionKey] as JObject;
				if (jObject3 == null)
				{
					jObject3 = (JObject)(jObject2[factionKey] = new JObject());
				}
				if (paramKey == "enabled")
				{
					jObject3[paramKey] = value > 0.5f;
				}
				else
				{
					jObject3[paramKey] = Math.Round(value, 4);
				}
				File.WriteAllText(_configPath, jObject.ToString());
				return true;
			}
			catch (Exception ex)
			{
				MelonLogger.Error("WriteDecayToJson: " + ex.Message);
				return false;
			}
		}

		private static void HandleHTPTierCostEdit(object player, BalanceMenuState state, int tierNum, string valueStr)
		{
			if (tierNum < 1 || tierNum > 8)
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick tier 1-8.</color>");
				return;
			}
			if (!int.TryParse(valueStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Invalid cost (integer required): " + valueStr + "</color>");
				return;
			}
			if (result < -1)
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Cost must be -1 (vanilla) or >= 0.</color>");
				return;
			}
			string text = $"tier_{tierNum}";
			string text2 = "vanilla";
			try
			{
				if (File.Exists(_configPath) && JObject.Parse(File.ReadAllText(_configPath))["tech_cost"] is JObject jObject)
				{
					int? num = jObject[text]?.Value<int>();
					if (num.HasValue && num.Value >= 0)
					{
						text2 = num.Value.ToString();
					}
				}
			}
			catch
			{
			}
			string text3 = ((result < 0) ? "vanilla" : result.ToString());
			state.PendingConfirm = true;
			state.PendingParamKey = text;
			state.PendingValue = result;
			state.PendingOldVal = text2;
			state.PendingTechTierKey = text;
			state.PendingIsTechCost = true;
			SendChatToPlayer(player, _chatPrefix + _headerColor + "Confirm:</color> Tier " + tierNum + " tech cost " + _valueColor + text2 + "</color> -> " + _valueColor + text3 + "</color>");
			SendChatToPlayer(player, _chatPrefix + _dimColor + "1/yes save · 2/no cancel · 3 save + !rebalance</color>");
		}

		private static bool WriteTechCostToJson(string tierKey, int value)
		{
			try
			{
				if (!File.Exists(_configPath))
				{
					return false;
				}
				JObject jObject = JObject.Parse(File.ReadAllText(_configPath));
				JObject jObject2 = jObject["tech_cost"] as JObject;
				if (jObject2 == null)
				{
					jObject2 = (JObject)(jObject["tech_cost"] = new JObject());
				}
				jObject2[tierKey] = value;
				File.WriteAllText(_configPath, jObject.ToString());
				return true;
			}
			catch (Exception ex)
			{
				MelonLogger.Error("WriteTechCostToJson: " + ex.Message);
				return false;
			}
		}

		private static bool WriteTechTierToJson(string tierKey, float value)
		{
			try
			{
				if (!File.Exists(_configPath))
				{
					return false;
				}
				JObject jObject = JObject.Parse(File.ReadAllText(_configPath));
				JObject jObject2 = jObject["tech_time"] as JObject;
				if (jObject2 == null)
				{
					jObject2 = (JObject)(jObject["tech_time"] = new JObject());
				}
				jObject2[tierKey] = (int)Math.Round(value);
				File.WriteAllText(_configPath, jObject.ToString());
				return true;
			}
			catch (Exception ex)
			{
				MelonLogger.Error("WriteTechTierToJson: " + ex.Message);
				return false;
			}
		}

		private static bool WriteBoolToJson(string key, bool value)
		{
			try
			{
				if (!File.Exists(_configPath))
				{
					return false;
				}
				JObject jObject = JObject.Parse(File.ReadAllText(_configPath));
				jObject[key] = value;
				File.WriteAllText(_configPath, jObject.ToString());
				return true;
			}
			catch (Exception ex)
			{
				MelonLogger.Error("WriteBoolToJson: " + ex.Message);
				return false;
			}
		}

		private static string ReadProximityConfigValue(JObject unitConfig, string proxField)
		{
			if (unitConfig == null)
			{
				return "-";
			}
			try
			{
				if (!(unitConfig["projectiles"] is JObject jObject))
				{
					return "-";
				}
				foreach (KeyValuePair<string, JToken> item in jObject)
				{
					if (!(item.Value is JObject jObject2) || !(jObject2["proximity"] is JObject jObject3))
					{
						continue;
					}
					JToken jToken = jObject3[proxField];
					if (jToken != null)
					{
						if (jToken.Type == JTokenType.Boolean)
						{
							return jToken.Value<bool>() ? "True" : "False";
						}
						return jToken.ToString();
					}
				}
			}
			catch
			{
			}
			return "-";
		}

		private static JObject ReadUnitConfigFromJson(string unitName)
		{
			try
			{
				if (!File.Exists(_configPath))
				{
					return null;
				}
				if (!(JObject.Parse(File.ReadAllText(_configPath))["units"] is JObject jObject))
				{
					return null;
				}
				return jObject[unitName] as JObject;
			}
			catch
			{
				return null;
			}
		}

		private static bool WriteParamToJson(string unitName, string paramKey, float value)
		{
			try
			{
				if (!File.Exists(_configPath))
				{
					return false;
				}
				string text = File.ReadAllText(_configPath);
				string text2;
				switch (paramKey)
				{
				case "impact_scale_by_speed":
					text2 = ((value != 0f) ? "true" : "false");
					break;
				case "min_tier":
				case "build_radius":
				case "deposit_radius":
				case "extraction_radius":
				case "target_distance":
				case "fow_distance":
				case "unit_cap_value":
				case "aim_compensation":
					text2 = ((int)value).ToString();
					break;
				default:
					text2 = Math.Round(value, 4).ToString(CultureInfo.InvariantCulture);
					break;
				}
				if (!(JObject.Parse(text)["units"] is JObject jObject))
				{
					return false;
				}
				_ = jObject[unitName];
				int num = FindJsonKey(text, unitName);
				if (num < 0)
				{
					int num2 = FindClosingBrace(text, FindJsonKey(text, "units"));
					if (num2 < 0)
					{
						return false;
					}
					string text3 = "        ";
					string value2 = ",\n" + text3 + "\"" + EscapeJsonString(unitName) + "\": { \"" + EscapeJsonString(paramKey) + "\": " + text2 + ", \"_note\": \"modified via !b\" }";
					text = text.Insert(num2, value2);
				}
				else
				{
					int num3 = text.IndexOf('{', num);
					if (num3 < 0)
					{
						return false;
					}
					int num4 = FindClosingBrace(text, num3);
					if (num4 < 0)
					{
						return false;
					}
					string text4 = text.Substring(num3, num4 - num3 + 1);
					int num5 = FindJsonKey(text4, paramKey);
					if (num5 >= 0)
					{
						int num6 = text4.IndexOf(':', num5);
						if (num6 < 0)
						{
							return false;
						}
						int i;
						for (i = num6 + 1; i < text4.Length && text4[i] == ' '; i++)
						{
						}
						int j = i;
						bool flag = false;
						if (j < text4.Length && text4[j] == '"')
						{
							flag = true;
						}
						for (; j < text4.Length; j++)
						{
							char c = text4[j];
							if (flag)
							{
								if (c == '"' && j > i)
								{
									j++;
									break;
								}
							}
							else if (c == ',' || c == '}' || c == '\n')
							{
								break;
							}
						}
						string text5 = text4.Substring(i, j - i).TrimEnd();
						text4 = text4.Substring(0, i) + " " + text2 + text4.Substring(i + text5.Length);
					}
					else
					{
						int startIndex = text4.LastIndexOf('}');
						string value3 = ", \"" + EscapeJsonString(paramKey) + "\": " + text2;
						text4 = text4.Insert(startIndex, value3);
					}
					int num7 = FindJsonKey(text4, "_note");
					if (num7 >= 0)
					{
						int num8 = text4.IndexOf(':', num7);
						if (num8 >= 0)
						{
							int k;
							for (k = num8 + 1; k < text4.Length && text4[k] == ' '; k++)
							{
							}
							int l = k;
							if (l < text4.Length && text4[l] == '"')
							{
								for (l++; l < text4.Length && text4[l] != '"'; l++)
								{
								}
								if (l < text4.Length)
								{
									l++;
								}
							}
							text4 = text4.Substring(0, k) + " \"modified via !b\"" + text4.Substring(l);
						}
					}
					text = text.Substring(0, num3) + text4 + text.Substring(num4 + 1);
				}
				File.WriteAllText(_configPath, text);
				return true;
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[BAL] Failed to write JSON: " + ex.Message);
				return false;
			}
		}

		private static bool WriteProximityToJson(string unitName, string proxField, float value)
		{
			try
			{
				if (!File.Exists(_configPath))
				{
					return false;
				}
				JObject jObject = JObject.Parse(File.ReadAllText(_configPath));
				JObject jObject2 = jObject["units"] as JObject;
				if (jObject2 == null)
				{
					jObject2 = (JObject)(jObject["units"] = new JObject());
				}
				JObject jObject3 = jObject2[unitName] as JObject;
				if (jObject3 == null)
				{
					jObject3 = new JObject();
					jObject3["_note"] = "modified via !b";
					jObject2[unitName] = jObject3;
				}
				string text = FindUnitProjectileDataName(unitName);
				if (string.IsNullOrEmpty(text))
				{
					MelonLogger.Warning("[BAL] WriteProximityToJson: could not find ProjectileData for " + unitName);
					return false;
				}
				JObject jObject4 = jObject3["projectiles"] as JObject;
				if (jObject4 == null)
				{
					jObject4 = (JObject)(jObject3["projectiles"] = new JObject());
				}
				JObject jObject5 = jObject4[text] as JObject;
				if (jObject5 == null)
				{
					jObject5 = (JObject)(jObject4[text] = new JObject());
				}
				JObject jObject6 = jObject5["proximity"] as JObject;
				if (jObject6 == null)
				{
					jObject6 = (JObject)(jObject5["proximity"] = new JObject());
				}
				switch (proxField)
				{
				case "FlyingUnits":
				case "GroundUnits":
				case "Structures":
					jObject6[proxField] = value >= 0.5f;
					break;
				default:
					jObject6[proxField] = Math.Round(value, 4);
					break;
				}
				File.WriteAllText(_configPath, jObject.ToString());
				return true;
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[BAL] WriteProximityToJson failed: " + ex.Message);
				return false;
			}
		}

		private static string FindUnitProjectileDataName(string unitName)
		{
			try
			{
				ObjectInfo[] obj = _sharedAllInfos ?? Resources.FindObjectsOfTypeAll<ObjectInfo>();
				ObjectInfo objectInfo = null;
				ObjectInfo[] array = obj;
				foreach (ObjectInfo objectInfo2 in array)
				{
					if (!(objectInfo2 == null) && !(objectInfo2.Prefab == null) && MatchesUnitName(objectInfo2, unitName))
					{
						objectInfo = objectInfo2;
						break;
					}
				}
				if (objectInfo == null)
				{
					return null;
				}
				GameObject prefab = objectInfo.Prefab;
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				Component[] componentsInChildren = prefab.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component in componentsInChildren)
				{
					if (component == null || component.GetType().Name != "VehicleTurret")
					{
						continue;
					}
					string[] array2 = new string[2] { "Primary", "Secondary" };
					foreach (string text in array2)
					{
						string[] array3 = new string[2] { "Projectile", "ProjectileData" };
						foreach (string text2 in array3)
						{
							FieldInfo field = component.GetType().GetField(text + text2, bindingAttr);
							if (!(field == null))
							{
								object obj2 = null;
								try
								{
									obj2 = field.GetValue(component);
								}
								catch
								{
								}
								if (obj2 != null && GetProximityComponent(obj2, out string pdName) != null)
								{
									return pdName;
								}
							}
						}
					}
				}
				componentsInChildren = prefab.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component2 in componentsInChildren)
				{
					if (component2 == null || component2.GetType().Name != "CreatureDecapod")
					{
						continue;
					}
					string[] array2 = new string[2] { "AttackPrimary", "AttackSecondary" };
					foreach (string name in array2)
					{
						FieldInfo field2 = component2.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
						if (field2 == null)
						{
							continue;
						}
						object value = field2.GetValue(component2);
						if (value == null)
						{
							continue;
						}
						FieldInfo field3 = value.GetType().GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
						if (!(field3 == null))
						{
							object value2 = field3.GetValue(value);
							if (value2 != null && GetProximityComponent(value2, out string pdName2) != null)
							{
								return pdName2;
							}
						}
					}
				}
			}
			catch
			{
			}
			return null;
		}

		private static int FindJsonKey(string json, string key)
		{
			string text = "\"" + key + "\"";
			int num = 0;
			while (num < json.Length)
			{
				int num2 = json.IndexOf(text, num, StringComparison.Ordinal);
				if (num2 < 0)
				{
					return -1;
				}
				int i;
				for (i = num2 + text.Length; i < json.Length && (json[i] == ' ' || json[i] == '\t'); i++)
				{
				}
				if (i < json.Length && json[i] == ':')
				{
					return num2;
				}
				num = num2 + 1;
			}
			return -1;
		}

		private static int FindClosingBrace(string json, int startPos)
		{
			if (startPos < 0)
			{
				return -1;
			}
			int num = json.IndexOf('{', startPos);
			if (num < 0)
			{
				return -1;
			}
			int num2 = 0;
			bool flag = false;
			for (int i = num; i < json.Length; i++)
			{
				char c = json[i];
				if (flag)
				{
					switch (c)
					{
					case '\\':
						i++;
						break;
					case '"':
						flag = false;
						break;
					}
					continue;
				}
				switch (c)
				{
				case '"':
					flag = true;
					break;
				case '{':
					num2++;
					break;
				case '}':
					num2--;
					if (num2 == 0)
					{
						return i;
					}
					break;
				}
			}
			return -1;
		}

		private static string EscapeJsonString(string s)
		{
			return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
		}

		private static void WriteAuditLog(string playerName, string steamId, string unitName, string paramKey, string oldVal, string newVal)
		{
			try
			{
				if (string.IsNullOrEmpty(_auditLogPath))
				{
					_auditLogPath = Path.Combine(Path.GetDirectoryName(_configPath), "Si_UnitBalance_Audit.log");
				}
				string text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {playerName} ({steamId}): {unitName} {paramKey} {oldVal} -> {newVal}";
				File.AppendAllText(_auditLogPath, text + Environment.NewLine);
			}
			catch
			{
			}
		}

		private static void ApplyConstructionDataOverrides(bool useOM)
		{
			bool num = _costMultipliers.Count > 0 || _buildTimeMultipliers.Count > 0 || _minTierOverrides.Count > 0 || _buildRadiusOverrides.Count > 0;
			bool flag = _techTierTimes.Count > 0 || _techTierCosts.Count > 0;
			if (!num && !flag)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num2 = 0;
			int num3 = 0;
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.ConstructionData == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				ConstructionData constructionData = objectInfo.ConstructionData;
				string text2 = (useOM ? ("A:" + constructionData.name + ".asset") : null);
				if (flag && constructionData.TechnologyTier > 0)
				{
					if (_techTierTimes.TryGetValue(constructionData.TechnologyTier, out var value))
					{
						float buildUpTime = constructionData.BuildUpTime;
						float num4 = 0f;
						float num5 = 0f;
						try
						{
							Type type = constructionData.GetType();
							FieldInfo field = type.GetField("FinishedWaitTime", BindingFlags.Instance | BindingFlags.Public);
							FieldInfo field2 = type.GetField("CleanUpTime", BindingFlags.Instance | BindingFlags.Public);
							if (field != null)
							{
								num4 = (float)field.GetValue(constructionData);
							}
							if (field2 != null)
							{
								num5 = (float)field2.GetValue(constructionData);
							}
						}
						catch
						{
						}
						float num6 = buildUpTime + num4 + num5;
						float num7 = Math.Max(1f, value);
						float num8 = ((num6 > 0f) ? (num7 / num6) : 1f);
						float num9 = Math.Max(0.5f, buildUpTime * num8);
						bool flag2 = false;
						if (useOM)
						{
							flag2 = OMSetFloat(text2, "BuildUpTime", num9);
							if (!flag2)
							{
								MelonLogger.Warning($"[TECH] OM.Set failed for '{text2}' BuildUpTime={num9}, falling back to direct");
								constructionData.BuildUpTime = num9;
							}
						}
						else
						{
							constructionData.BuildUpTime = num9;
						}
						try
						{
							Type type2 = constructionData.GetType();
							if (num4 > 0f)
							{
								float num10 = num4 * num8;
								FieldInfo field3 = type2.GetField("FinishedWaitTime", BindingFlags.Instance | BindingFlags.Public);
								if (field3 != null)
								{
									if (useOM)
									{
										OMSetFloat(text2, "FinishedWaitTime", num10);
									}
									else
									{
										field3.SetValue(constructionData, num10);
									}
								}
							}
							if (num5 > 0f)
							{
								float num11 = num5 * num8;
								FieldInfo field4 = type2.GetField("CleanUpTime", BindingFlags.Instance | BindingFlags.Public);
								if (field4 != null)
								{
									if (useOM)
									{
										OMSetFloat(text2, "CleanUpTime", num11);
									}
									else
									{
										field4.SetValue(constructionData, num11);
									}
								}
							}
						}
						catch
						{
						}
						LogDebug(string.Format("[TECH] Tier {0} '{1}' (CD={2}): total {3:F0}s -> {4:F0}s (BU:{5:F0}->{6:F0} FWT:{7:F0} CUT:{8:F0}){9}", constructionData.TechnologyTier, text, constructionData.name, num6, num7, buildUpTime, num9, num4, num5, flag2 ? " (OM)" : ""));
						num3++;
					}
					if (_techTierCosts.TryGetValue(constructionData.TechnologyTier, out var value2))
					{
						int resourceCost = constructionData.ResourceCost;
						bool flag3 = false;
						if (useOM)
						{
							flag3 = OMSetInt(text2, "ResourceCost", value2);
							if (!flag3)
							{
								MelonLogger.Warning($"[TECH-COST] OM.Set failed for '{text2}' ResourceCost={value2}, falling back to direct");
								constructionData.ResourceCost = value2;
							}
						}
						else
						{
							constructionData.ResourceCost = value2;
						}
						LogDebug(string.Format("[TECH-COST] Tier {0} '{1}' (CD={2}): cost {3} -> {4}{5}", constructionData.TechnologyTier, text, constructionData.name, resourceCost, value2, flag3 ? " (OM)" : ""));
						num3++;
					}
					if (!_techTierTimes.ContainsKey(constructionData.TechnologyTier) && !_techTierCosts.ContainsKey(constructionData.TechnologyTier))
					{
						LogDebug($"[TECH-DIAG] Tier {constructionData.TechnologyTier} '{text}' (CD={constructionData.name}): no config for this tier, BuildUpTime={constructionData.BuildUpTime:F0}s cost={constructionData.ResourceCost}");
					}
					continue;
				}
				bool flag4 = _costMultipliers.TryGetValue(text, out var value3);
				bool flag5 = _buildTimeMultipliers.TryGetValue(text, out var value4);
				bool flag6 = _minTierOverrides.TryGetValue(text, out var value5);
				bool flag7 = _buildRadiusOverrides.TryGetValue(text, out var value6);
				if (!flag4 && !flag5 && !flag6 && !flag7)
				{
					continue;
				}
				if (flag4)
				{
					int resourceCost2 = constructionData.ResourceCost;
					int num12 = Math.Max(1, (int)Math.Round((float)resourceCost2 * value3));
					if (useOM)
					{
						OMSetInt(text2, "ResourceCost", num12);
					}
					else
					{
						constructionData.ResourceCost = num12;
					}
					LogDebug(string.Format("[COST] {0}: {1} -> {2} (x{3:F2}){4}", text, resourceCost2, num12, value3, useOM ? " (OM)" : ""));
				}
				if (flag5)
				{
					float buildUpTime2 = constructionData.BuildUpTime;
					float num13 = Math.Max(0.5f, buildUpTime2 * value4);
					if (useOM)
					{
						OMSetFloat(text2, "BuildUpTime", num13);
					}
					else
					{
						constructionData.BuildUpTime = num13;
					}
					LogDebug(string.Format("[BUILD] {0}: BuildUpTime {1:F1}s -> {2:F1}s (x{3:F2}){4}", text, buildUpTime2, num13, value4, useOM ? " (OM)" : ""));
					try
					{
						Type type3 = constructionData.GetType();
						FieldInfo field5 = type3.GetField("FinishedWaitTime", BindingFlags.Instance | BindingFlags.Public);
						FieldInfo field6 = type3.GetField("CleanUpTime", BindingFlags.Instance | BindingFlags.Public);
						if (field5 != null)
						{
							float num14 = (float)field5.GetValue(constructionData);
							if (num14 > 0f)
							{
								float num15 = num14 * value4;
								if (useOM)
								{
									OMSetFloat(text2, "FinishedWaitTime", num15);
								}
								else
								{
									field5.SetValue(constructionData, num15);
								}
								LogDebug($"[BUILD] {text}: FinishedWaitTime {num14:F1}s -> {num15:F1}s");
							}
						}
						if (field6 != null)
						{
							float num16 = (float)field6.GetValue(constructionData);
							if (num16 > 0f)
							{
								float num17 = num16 * value4;
								if (useOM)
								{
									OMSetFloat(text2, "CleanUpTime", num17);
								}
								else
								{
									field6.SetValue(constructionData, num17);
								}
								LogDebug($"[BUILD] {text}: CleanUpTime {num16:F1}s -> {num17:F1}s");
							}
						}
					}
					catch
					{
					}
				}
				if (flag6)
				{
					int minimumTeamTier = constructionData.MinimumTeamTier;
					if (useOM)
					{
						OMSetInt(text2, "MinimumTeamTier", value5);
					}
					else
					{
						constructionData.MinimumTeamTier = value5;
					}
					LogDebug(string.Format("[TIER] {0}: min tier {1} -> {2}{3}", text, minimumTeamTier, value5, useOM ? " (OM)" : ""));
				}
				if (flag7)
				{
					float maximumBaseStructureDistance = constructionData.MaximumBaseStructureDistance;
					if (useOM)
					{
						OMSetFloat(text2, "MaximumBaseStructureDistance", value6);
					}
					else
					{
						constructionData.MaximumBaseStructureDistance = value6;
					}
					LogDebug(string.Format("[BUILD_RADIUS] {0}: {1:F0} -> {2:F0}{3}", text, maximumBaseStructureDistance, value6, useOM ? " (OM)" : ""));
				}
				num2++;
			}
			if (num2 > 0 || num3 > 0)
			{
				MelonLogger.Msg($"Applied construction overrides: {num2} units/structures, {num3} tech tiers");
			}
		}

		private static void ApplyDepositRadiusOverrides(bool useOM)
		{
			if (_depositRadiusOverrides.Count == 0)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text) || !_depositRadiusOverrides.TryGetValue(text, out var value))
				{
					continue;
				}
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component in componentsInChildren)
				{
					if (!(component == null) && !(component.GetType().Name != "ResourceDepositPoint"))
					{
						FieldInfo field = component.GetType().GetField("DepositRadius", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (!(field == null) && !(field.FieldType != typeof(float)))
						{
							float num2 = (float)field.GetValue(component);
							field.SetValue(component, value);
							LogDebug($"[DEPOSIT_RADIUS] {text}: ResourceDepositPoint.DepositRadius {num2:F0} -> {value:F0} (direct)");
							num++;
						}
					}
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[DEPOSIT_RADIUS] Applied deposit radius overrides to {num} deposit points");
			}
		}

		private static void ApplyExtractionRadiusOverrides(bool useOM)
		{
			if (_extractionRadiusOverrides.Count == 0)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text) || !_extractionRadiusOverrides.TryGetValue(text, out var value))
				{
					continue;
				}
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component in componentsInChildren)
				{
					if (!(component == null))
					{
						FieldInfo field = component.GetType().GetField("ExtractionRadius", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (!(field == null) && !(field.FieldType != typeof(float)))
						{
							float num2 = (float)field.GetValue(component);
							field.SetValue(component, value);
							LogDebug($"[EXTRACTION_RADIUS] {text}: MiningArmsBase.ExtractionRadius {num2:F0} -> {value:F0} (direct)");
							num++;
						}
					}
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[EXTRACTION_RADIUS] Applied extraction radius overrides to {num} harvester(s)");
			}
		}

		private static void ApplyUnitCapOverrides(bool useOM)
		{
			if (_unitCapOverrides.Count == 0)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (!string.IsNullOrEmpty(text) && _unitCapOverrides.TryGetValue(text, out var value))
				{
					int unitCapValue = objectInfo.UnitCapValue;
					string target = (useOM ? ("A:" + objectInfo.name + ".asset") : null);
					if (useOM)
					{
						OMSetInt(target, "UnitCapValue", value);
					}
					else
					{
						objectInfo.UnitCapValue = value;
					}
					LogDebug(string.Format("[UNITCAP] {0}: {1} -> {2}{3}", text, unitCapValue, value, useOM ? " (OM)" : ""));
					num++;
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[UNITCAP] Applied unit cap overrides to {num} units");
			}
		}

		private static void ApplyHealthOverrides(bool useOM)
		{
			if (_healthMultipliers.Count == 0)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				DamageManager component = objectInfo.Prefab.GetComponent<DamageManager>();
				if (component == null)
				{
					continue;
				}
				try
				{
					FieldInfo field = component.GetType().GetField("Data", BindingFlags.Instance | BindingFlags.Public);
					if (field == null)
					{
						continue;
					}
					UnityEngine.Object obj = field.GetValue(component) as UnityEngine.Object;
					if (obj == null)
					{
						continue;
					}
					string name = obj.name;
					string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
					if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(text))
					{
						if (!dictionary.ContainsKey(name))
						{
							dictionary[name] = new List<string>();
						}
						dictionary[name].Add(text);
					}
				}
				catch
				{
				}
			}
			foreach (KeyValuePair<string, List<string>> item in dictionary)
			{
				if (item.Value.Count <= 1)
				{
					continue;
				}
				bool flag = false;
				foreach (string item2 in item.Value)
				{
					if (_healthMultipliers.ContainsKey(item2))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					MelonLogger.Warning("[HEALTH-DIAG] DamageManagerData '" + item.Key + "' shared by: " + string.Join(", ", item.Value));
				}
			}
			array2 = array;
			foreach (ObjectInfo objectInfo2 in array2)
			{
				if (objectInfo2 == null || objectInfo2.Prefab == null)
				{
					continue;
				}
				string text2 = ResolveConfigName(objectInfo2.DisplayName, objectInfo2.name);
				if (string.IsNullOrEmpty(text2) || !_healthMultipliers.TryGetValue(text2, out var value))
				{
					continue;
				}
				DamageManager component2 = objectInfo2.Prefab.GetComponent<DamageManager>();
				if (component2 == null)
				{
					continue;
				}
				bool flag2 = false;
				try
				{
					FieldInfo field2 = component2.GetType().GetField("Data", BindingFlags.Instance | BindingFlags.Public);
					if (field2 != null)
					{
						object value2 = field2.GetValue(component2);
						if (value2 != null)
						{
							string name2 = ((UnityEngine.Object)value2).name;
							if (!string.IsNullOrEmpty(name2) && hashSet.Contains(name2))
							{
								MelonLogger.Warning("[HEALTH] " + text2 + ": DamageManagerData '" + name2 + "' already modified — skipping to prevent compounding");
								continue;
							}
							float floatMember = GetFloatMember(value2, "Health");
							if (floatMember <= 0f)
							{
								floatMember = GetFloatMember(value2, "MaxHealth");
							}
							if (floatMember <= 0f)
							{
								floatMember = GetFloatMember(value2, "m_MaxHealth");
							}
							string key = name2 ?? text2;
							float num2;
							if (_originalHealth.ContainsKey(key))
							{
								num2 = _originalHealth[key];
							}
							else
							{
								num2 = floatMember;
								_originalHealth[key] = num2;
							}
							if (num2 > 0f)
							{
								float num3 = num2 * value;
								LogDebug($"[HEALTH-DIAG] {text2}: DMD='{name2}', vanilla={num2:F0}, current={floatMember:F0}, target={num3:F0} (x{value:F2})");
								if (useOM && !string.IsNullOrEmpty(name2))
								{
									string text3 = "A:DamageManagerData_" + name2 + ".asset";
									flag2 = OMSetFloat(text3, "Health", num3);
									if (flag2)
									{
										LogDebug($"[HEALTH] {text2}: Health {num2:F0} -> {num3:F0} (x{value:F2}) (OM: {name2})");
									}
									else
									{
										MelonLogger.Warning("[HEALTH] " + text2 + ": OM failed for " + text3 + ", falling back to direct mutation");
									}
								}
								if (!flag2)
								{
									flag2 = SetFloatMember(value2, "Health", num3);
									if (!flag2)
									{
										flag2 = SetFloatMember(value2, "MaxHealth", num3);
									}
									if (!flag2)
									{
										flag2 = SetFloatMember(value2, "m_MaxHealth", num3);
									}
									if (flag2)
									{
										LogDebug($"[HEALTH] {text2}: Health {num2:F0} -> {num3:F0} (x{value:F2}) (direct mutation — no client sync!)");
									}
								}
								if (flag2 && !string.IsNullOrEmpty(name2))
								{
									hashSet.Add(name2);
								}
							}
						}
					}
				}
				catch
				{
				}
				if (flag2)
				{
					num++;
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[HEALTH] Applied MaxHealth overrides to {num} units/structures");
			}
		}

		private static void ReclampLiveHealth()
		{
			if (_healthMultipliers.Count == 0)
			{
				return;
			}
			PropertyInfo property = typeof(DamageManager).GetProperty("Health", BindingFlags.Instance | BindingFlags.Public);
			if (property == null || property.GetSetMethod(nonPublic: true) == null)
			{
				MelonLogger.Warning("[HEALTH] Cannot find Health setter — skip live rescale");
				return;
			}
			MethodInfo setMethod = property.GetSetMethod(nonPublic: true);
			Dictionary<string, float> dictionary = new Dictionary<string, float>();
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			foreach (ObjectInfo objectInfo in array)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text) || !_healthMultipliers.TryGetValue(text, out var value))
				{
					continue;
				}
				DamageManager component = objectInfo.Prefab.GetComponent<DamageManager>();
				if (component == null || component.Data == null)
				{
					continue;
				}
				string name = component.Data.name;
				if (!string.IsNullOrEmpty(name) && !dictionary.ContainsKey(name))
				{
					string key = name;
					if (_originalHealth.TryGetValue(key, out var value2) && value2 > 0f)
					{
						dictionary[name] = value;
					}
					else
					{
						dictionary[name] = 1f;
					}
				}
			}
			if (dictionary.Count == 0)
			{
				return;
			}
			DamageManager[] array2 = UnityEngine.Object.FindObjectsOfType<DamageManager>();
			int num = 0;
			DamageManager[] array3 = array2;
			foreach (DamageManager damageManager in array3)
			{
				if (damageManager == null || damageManager.Data == null)
				{
					continue;
				}
				string name2 = damageManager.Data.name;
				if (!dictionary.TryGetValue(name2, out var value3))
				{
					continue;
				}
				float health = damageManager.Health;
				float maxHealth = damageManager.MaxHealth;
				if (health <= 0f || maxHealth <= 0f)
				{
					continue;
				}
				float val;
				if (Math.Abs(value3 - 1f) < 0.001f)
				{
					if (!_originalHealth.TryGetValue(name2, out var value4) || !(value4 > 0f))
					{
						continue;
					}
					val = ((!(health >= value4 - 1f)) ? (health / value4 * maxHealth) : maxHealth);
				}
				else
				{
					val = health * value3;
				}
				val = Math.Min(val, maxHealth);
				if (!(Math.Abs(val - health) < 1f))
				{
					setMethod.Invoke(damageManager, new object[1] { val });
					LogDebug($"[HEALTH] Rescaled live {name2}: {health:F0} -> {val:F0} (max={maxHealth:F0})");
					num++;
				}
			}
			if (num > 0)
			{
				LogDebug($"[HEALTH] Rescaled {num} live unit(s) to maintain health percentage");
			}
		}

		private static float GetDamageFieldMult(string unitName, string weapon, string fieldName)
		{
			string text = _damageFieldToSubtype[fieldName];
			if (!string.IsNullOrEmpty(weapon) && _damageMultipliers.TryGetValue(weapon + ":" + text + unitName, out var value))
			{
				return value;
			}
			if (!string.IsNullOrEmpty(weapon) && _damageMultipliers.TryGetValue(weapon + ":" + unitName, out var value2))
			{
				return value2;
			}
			if (_damageMultipliers.TryGetValue(text + unitName, out var value3))
			{
				return value3;
			}
			if (_damageMultipliers.TryGetValue(unitName, out var value4))
			{
				return value4;
			}
			return 1f;
		}

		private static bool HasAnyDamageMult(string unitName)
		{
			string[] array = new string[15]
			{
				"", "pri:", "sec:", "impact:", "ricochet:", "splash:", "penetrating:", "pri:impact:", "pri:ricochet:", "pri:splash:",
				"pri:penetrating:", "sec:impact:", "sec:ricochet:", "sec:splash:", "sec:penetrating:"
			};
			foreach (string text in array)
			{
				if (_damageMultipliers.ContainsKey(text + unitName))
				{
					return true;
				}
			}
			array = new string[9] { "max:", "min:", "pow:", "pri:max:", "pri:min:", "pri:pow:", "sec:max:", "sec:min:", "sec:pow:" };
			foreach (string text2 in array)
			{
				if (_splashRadiusMultipliers.ContainsKey(text2 + unitName))
				{
					return true;
				}
			}
			return false;
		}

		private static float GetSplashRadiusMult(string unitName, string weapon, string fieldName)
		{
			string text = _splashRadiusFieldToKey[fieldName];
			if (!string.IsNullOrEmpty(weapon) && _splashRadiusMultipliers.TryGetValue(weapon + ":" + text + unitName, out var value))
			{
				return value;
			}
			if (_splashRadiusMultipliers.TryGetValue(text + unitName, out var value2))
			{
				return value2;
			}
			return 1f;
		}

		private static void ApplyProjectileDamageOverrides(bool useOM)
		{
			if (_damageMultipliers.Count == 0 && _splashRadiusMultipliers.Count == 0 && _projectileOverrides.Count == 0)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			HashSet<string> hashSet = new HashSet<string>();
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				bool flag = HasAnyDamageMult(text);
				bool flag2 = _projectileOverrides.TryGetValue(text, out Dictionary<string, Dictionary<string, float>> value);
				if (!flag && !flag2)
				{
					continue;
				}
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				bool flag3 = false;
				int num2 = 0;
				Component[] array3 = componentsInChildren;
				foreach (Component component in array3)
				{
					if (component == null)
					{
						continue;
					}
					string name = component.GetType().Name;
					if (name == "VehicleTurret")
					{
						Type type = component.GetType();
						string[] array4 = new string[2] { "PrimaryProjectileData", "SecondaryProjectileData" };
						foreach (string text2 in array4)
						{
							FieldInfo field = type.GetField(text2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							if (field == null)
							{
								continue;
							}
							object value2;
							try
							{
								value2 = field.GetValue(component);
							}
							catch
							{
								continue;
							}
							if (value2 == null)
							{
								continue;
							}
							string text3 = "";
							try
							{
								text3 = ((UnityEngine.Object)value2).name;
							}
							catch
							{
								continue;
							}
							if (string.IsNullOrEmpty(text3) || hashSet.Contains(text3))
							{
								continue;
							}
							if (flag2 && value.TryGetValue(text3, out var value3))
							{
								string target = (useOM ? ("A:" + text3 + ".asset") : null);
								foreach (KeyValuePair<string, float> item in value3)
								{
									float floatMember = GetFloatMember(value2, item.Key);
									if (useOM)
									{
										OMSetFloat(target, item.Key, item.Value);
									}
									else
									{
										SetFloatMember(value2, item.Key, item.Value);
									}
									LogDebug(string.Format("[DMG] {0} -> {1}.{2}: {3:F1} -> {4:F1}{5}", text, text3, item.Key, floatMember, item.Value, useOM ? " (OM)" : ""));
								}
								hashSet.Add(text3);
								flag3 = true;
							}
							else if (flag)
							{
								int value4 = 0;
								_vtPriIndex.TryGetValue(text, out value4);
								string weapon;
								if (num2 != value4)
								{
									weapon = "sec";
								}
								else
								{
									weapon = (text2.StartsWith("Primary") ? "pri" : "sec");
								}
								ScaleProjectileDamage(value2, text3, text, weapon, hashSet, useOM);
								flag3 = true;
							}
						}
						num2++;
					}
					if (name == "CreatureDecapod")
					{
						Type type2 = component.GetType();
						string[] array4 = new string[2] { "AttackPrimary", "AttackSecondary" };
						foreach (string text4 in array4)
						{
							FieldInfo field2 = type2.GetField(text4, BindingFlags.Instance | BindingFlags.Public);
							if (field2 == null)
							{
								continue;
							}
							object value5;
							try
							{
								value5 = field2.GetValue(component);
							}
							catch
							{
								continue;
							}
							if (value5 == null)
							{
								continue;
							}
							FieldInfo field3 = value5.GetType().GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
							object obj4 = null;
							if (field3 != null)
							{
								try
								{
									obj4 = field3.GetValue(value5);
								}
								catch
								{
								}
							}
							if (obj4 != null)
							{
								string text5 = "";
								try
								{
									text5 = ((UnityEngine.Object)obj4).name;
								}
								catch
								{
								}
								if (string.IsNullOrEmpty(text5) || hashSet.Contains(text5))
								{
									continue;
								}
								if (flag2 && value.TryGetValue(text5, out var value6))
								{
									string target2 = (useOM ? ("A:" + text5 + ".asset") : null);
									foreach (KeyValuePair<string, float> item2 in value6)
									{
										float floatMember2 = GetFloatMember(obj4, item2.Key);
										if (useOM)
										{
											OMSetFloat(target2, item2.Key, item2.Value);
										}
										else
										{
											SetFloatMember(obj4, item2.Key, item2.Value);
										}
										LogDebug(string.Format("[DMG] {0} -> {1}.{2}: {3:F1} -> {4:F1}{5}", text, text5, item2.Key, floatMember2, item2.Value, useOM ? " (OM)" : ""));
									}
									hashSet.Add(text5);
									flag3 = true;
								}
								else if (flag)
								{
									string weapon2 = ((text4 == "AttackPrimary") ? "pri" : "sec");
									ScaleProjectileDamage(obj4, text5, text, weapon2, hashSet, useOM);
									flag3 = true;
								}
							}
							else
							{
								if (!flag)
								{
									continue;
								}
								string weapon3 = ((text4 == "AttackPrimary") ? "pri" : "sec");
								float weaponMult = GetWeaponMult(_damageMultipliers, text, weapon3);
								if (Math.Abs(weaponMult - 1f) > 0.001f)
								{
									string key = text + "_" + text4;
									float num3;
									if (_originalMeleeDamage.ContainsKey(key))
									{
										num3 = _originalMeleeDamage[key];
									}
									else
									{
										num3 = GetFloatMember(value5, "Damage");
										_originalMeleeDamage[key] = num3;
									}
									if (num3 > 0f)
									{
										float num4 = num3 * weaponMult;
										SetFloatMember(value5, "Damage", num4);
										LogDebug($"[DMG] {text} -> {text4}.Damage: {num3:F0} -> {num4:F0} (melee, direct)");
										flag3 = true;
									}
								}
							}
						}
					}
					if (!((name != "VehicleTurret" && name != "CreatureDecapod") & flag))
					{
						continue;
					}
					FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					foreach (FieldInfo fieldInfo in fields)
					{
						if (fieldInfo.FieldType.Name != "ProjectileData")
						{
							continue;
						}
						object value7;
						try
						{
							value7 = fieldInfo.GetValue(component);
						}
						catch
						{
							continue;
						}
						if (value7 != null)
						{
							string text6 = "";
							try
							{
								text6 = ((UnityEngine.Object)value7).name;
							}
							catch
							{
								continue;
							}
							if (!string.IsNullOrEmpty(text6) && !hashSet.Contains(text6))
							{
								ScaleProjectileDamage(value7, text6, text, "pri", hashSet, useOM);
								flag3 = true;
							}
						}
					}
				}
				if (flag3)
				{
					num++;
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[DMG] Applied projectile/melee damage overrides to {num} units");
			}
		}

		private static void ScaleProjectileDamage(object pdObj, string pdName, string unitName, string weapon, HashSet<string> modifiedPD, bool useOM)
		{
			string target = (useOM ? ("A:" + pdName + ".asset") : null);
			int num = 0;
			bool boolMember = GetBoolMember(pdObj, "m_bSplash");
			bool boolMember2 = GetBoolMember(pdObj, "m_bPenetrating");
			string[] damageFields = _damageFields;
			foreach (string text in damageFields)
			{
				if ((!boolMember && text == "m_fSplashDamageMax") || (!boolMember2 && text == "m_fPenetratingDamage"))
				{
					continue;
				}
				float floatMember = GetFloatMember(pdObj, text);
				if (floatMember <= 0f)
				{
					continue;
				}
				float damageFieldMult = GetDamageFieldMult(unitName, weapon, text);
				if (!(Math.Abs(damageFieldMult - 1f) <= 0.001f))
				{
					float num2 = floatMember * damageFieldMult;
					if (useOM)
					{
						OMSetFloat(target, text, num2);
					}
					else
					{
						SetFloatMember(pdObj, text, num2);
					}
					LogDebug(string.Format("[DMG] {0} -> {1}.{2}: {3:F1} -> {4:F1} (x{5:F2}){6}", unitName, pdName, text, floatMember, num2, damageFieldMult, useOM ? " (OM)" : ""));
					num++;
				}
			}
			float floatMember2 = GetFloatMember(pdObj, "m_fSplashDamageMax");
			if (boolMember && floatMember2 > 0f && _splashRadiusMultipliers.Count > 0)
			{
				damageFields = _splashRadiusFields;
				foreach (string text2 in damageFields)
				{
					float floatMember3 = GetFloatMember(pdObj, text2);
					if (floatMember3 <= 0f)
					{
						continue;
					}
					float splashRadiusMult = GetSplashRadiusMult(unitName, weapon, text2);
					if (!(Math.Abs(splashRadiusMult - 1f) <= 0.001f))
					{
						float num3 = floatMember3 * splashRadiusMult;
						if (useOM)
						{
							OMSetFloat(target, text2, num3);
						}
						else
						{
							SetFloatMember(pdObj, text2, num3);
						}
						LogDebug(string.Format("[DMG] {0} -> {1}.{2}: {3:F1} -> {4:F1} (x{5:F2}){6}", unitName, pdName, text2, floatMember3, num3, splashRadiusMult, useOM ? " (OM)" : ""));
						num++;
					}
				}
			}
			modifiedPD.Add(pdName);
		}

		private static void ApplyRangeOverrides(bool useOM)
		{
			if (_rangeMultipliers.Count == 0 && _speedMultipliers.Count == 0 && _reloadTimeMultipliers.Count == 0 && _accuracyMultipliers.Count == 0 && _magazineMultipliers.Count == 0 && _fireRateMultipliers.Count == 0 && _lifetimeMultipliers.Count == 0)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			ProjectileData[] array2 = Resources.FindObjectsOfTypeAll<ProjectileData>();
			int num = 0;
			HashSet<string> hashSet = new HashSet<string>();
			ObjectInfo[] array3 = array;
			foreach (ObjectInfo objectInfo in array3)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				bool flag = HasAnyWeaponMult(_rangeMultipliers, text);
				bool flag2 = HasAnyWeaponMult(_speedMultipliers, text);
				bool flag3 = HasAnyWeaponMult(_lifetimeMultipliers, text);
				bool flag4 = HasAnyWeaponMult(_reloadTimeMultipliers, text);
				bool flag5 = HasAnyWeaponMult(_accuracyMultipliers, text);
				bool flag6 = HasAnyWeaponMult(_magazineMultipliers, text);
				bool flag7 = HasAnyWeaponMult(_fireRateMultipliers, text);
				if (!flag && !flag2 && !flag3 && !flag4 && !flag5 && !flag6 && !flag7)
				{
					continue;
				}
				string target = (useOM ? ("A:" + objectInfo.name + ".asset") : null);
				LogDebug("[RANGE] Applying weapon overrides to '" + text + "' (internal: " + objectInfo.name + ")" + (useOM ? " (OM)" : ""));
				bool flag8 = false;
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				int num2 = 0;
				Component[] array4 = componentsInChildren;
				foreach (Component component in array4)
				{
					if (component == null)
					{
						continue;
					}
					string name = component.GetType().Name;
					if (flag)
					{
						if (name == "VehicleTurret")
						{
							int value = 0;
							_vtPriIndex.TryGetValue(text, out value);
							string weapon = ((num2 == value) ? "pri" : "sec");
							float weaponMult = GetWeaponMult(_rangeMultipliers, text, weapon);
							if (Math.Abs(weaponMult - 1f) > 0.001f)
							{
								FieldInfo field = component.GetType().GetField("AimDistance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
								if (field != null)
								{
									float num3 = (float)field.GetValue(component);
									float num4 = num3 * weaponMult;
									if (useOM)
									{
										OMSetFloat(target, "AimDistance", num4);
									}
									else
									{
										field.SetValue(component, num4);
									}
									LogDebug($"  VehicleTurret.AimDistance: {num3} -> {num4}");
								}
							}
						}
						if (name == "UnitAimAt")
						{
							float weaponMult2 = GetWeaponMult(_rangeMultipliers, text, "pri");
							if (Math.Abs(weaponMult2 - 1f) > 0.001f)
							{
								FieldInfo field2 = component.GetType().GetField("AimDistanceMax", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
								if (field2 != null)
								{
									float num5 = (float)field2.GetValue(component);
									float num6 = num5 * weaponMult2;
									if (useOM)
									{
										OMSetFloat(target, "AimDistanceMax", num6);
									}
									else
									{
										field2.SetValue(component, num6);
									}
									LogDebug($"  UnitAimAt.AimDistanceMax: {num5} -> {num6}");
								}
							}
						}
					}
					if (name == "CreatureDecapod")
					{
						Type type = component.GetType();
						string[] array5 = new string[2] { "AttackPrimary", "AttackSecondary" };
						foreach (string text2 in array5)
						{
							string weapon2 = ((text2 == "AttackPrimary") ? "pri" : "sec");
							float weaponMult3 = GetWeaponMult(_rangeMultipliers, text, weapon2);
							float weaponMult4 = GetWeaponMult(_accuracyMultipliers, text, weapon2);
							float weaponMult5 = GetWeaponMult(_speedMultipliers, text, weapon2);
							object obj = null;
							FieldInfo field3 = type.GetField(text2, BindingFlags.Instance | BindingFlags.Public);
							if (field3 != null)
							{
								try
								{
									obj = field3.GetValue(component);
								}
								catch
								{
								}
							}
							if (obj == null)
							{
								PropertyInfo property = type.GetProperty(text2, BindingFlags.Instance | BindingFlags.Public);
								if (property != null)
								{
									try
									{
										obj = property.GetValue(component);
									}
									catch
									{
									}
								}
							}
							if (obj == null)
							{
								continue;
							}
							Type type2 = obj.GetType();
							object obj4 = null;
							FieldInfo field4 = type2.GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
							if (field4 != null)
							{
								try
								{
									obj4 = field4.GetValue(obj);
								}
								catch
								{
								}
							}
							if (obj4 == null)
							{
								PropertyInfo property2 = type2.GetProperty("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
								if (property2 != null)
								{
									try
									{
										obj4 = property2.GetValue(obj);
									}
									catch
									{
									}
								}
							}
							bool flag9 = false;
							if (obj4 != null)
							{
								try
								{
									FieldInfo field5 = obj4.GetType().GetField("m_InstantHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
									if (field5 != null)
									{
										flag9 = (bool)field5.GetValue(obj4);
									}
									else
									{
										PropertyInfo property3 = obj4.GetType().GetProperty("m_InstantHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
										if (property3 != null && property3.CanRead)
										{
											flag9 = (bool)property3.GetValue(obj4);
										}
									}
								}
								catch
								{
								}
							}
							float num7 = weaponMult3 * (flag9 ? weaponMult5 : 1f);
							if (Math.Abs(num7 - 1f) > 0.001f)
							{
								float floatMember = GetFloatMember(obj, "AttackProjectileAimDistMax");
								if (!float.IsNaN(floatMember))
								{
									string key = text + "_ca_" + text2;
									float num8;
									if (_originalCreatureAttackAimDist.ContainsKey(key))
									{
										num8 = _originalCreatureAttackAimDist[key];
									}
									else
									{
										num8 = floatMember;
										_originalCreatureAttackAimDist[key] = num8;
									}
									float num9 = num8 * num7;
									SetFloatMember(obj, "AttackProjectileAimDistMax", num9);
									LogDebug(string.Format("  CreatureAttack.{0}.AimDistMax: {1} -> {2} (direct{3})", text2, num8, num9, flag9 ? ", instant-hit" : ""));
								}
							}
							if (Math.Abs(weaponMult4 - 1f) > 0.001f)
							{
								float floatMember2 = GetFloatMember(obj, "AttackProjectileSpread");
								if (!float.IsNaN(floatMember2))
								{
									string key2 = text + "_spread_" + text2;
									float num10;
									if (_originalSpread.ContainsKey(key2))
									{
										num10 = _originalSpread[key2];
									}
									else
									{
										num10 = floatMember2;
										_originalSpread[key2] = num10;
									}
									float num11 = num10 * weaponMult4;
									SetFloatMember(obj, "AttackProjectileSpread", num11);
									LogDebug($"  CreatureAttack.{text2}.Spread: {num10} -> {num11} (direct)");
								}
								if (flag9)
								{
									float floatMember3 = GetFloatMember(obj, "AttackProjectileAimAngle");
									if (!float.IsNaN(floatMember3) && floatMember3 > 0f)
									{
										string key3 = text + "_aimangle_" + text2;
										float num12;
										if (_originalAimAngle.ContainsKey(key3))
										{
											num12 = _originalAimAngle[key3];
										}
										else
										{
											num12 = floatMember3;
											_originalAimAngle[key3] = num12;
										}
										float num13 = num12 * weaponMult4;
										SetFloatMember(obj, "AttackProjectileAimAngle", num13);
										LogDebug($"  CreatureAttack.{text2}.AimAngle: {num12:F2} -> {num13:F2} (direct, instant-hit accuracy)");
									}
								}
							}
							float weaponMult6 = GetWeaponMult(_lifetimeMultipliers, text, weapon2);
							bool flag10 = Math.Abs(weaponMult3 - 1f) > 0.001f;
							bool flag11 = Math.Abs(weaponMult5 - 1f) > 0.001f;
							bool flag12 = Math.Abs(weaponMult6 - 1f) > 0.001f;
							if ((flag10 | flag11 | flag12) && obj4 != null)
							{
								string text3 = "";
								try
								{
									text3 = ((UnityEngine.Object)obj4).name;
								}
								catch
								{
								}
								if (!string.IsNullOrEmpty(text3) && !hashSet.Contains(text3))
								{
									ScaleProjectileData(obj4, text3, flag10 ? weaponMult3 : 1f, flag11 ? weaponMult5 : 1f, hashSet, useOM, flag12 ? weaponMult6 : 1f, text);
									flag8 = true;
								}
							}
						}
					}
					if (name == "VehicleTurret")
					{
						Type type3 = component.GetType();
						string[] array5 = new string[2] { "Primary", "Secondary" };
						foreach (string text4 in array5)
						{
							string weapon3;
							if (num2 != 0)
							{
								weapon3 = "sec";
							}
							else
							{
								weapon3 = ((text4 == "Primary") ? "pri" : "sec");
							}
							if (flag4)
							{
								float weaponMult7 = GetWeaponMult(_reloadTimeMultipliers, text, weapon3);
								if (Math.Abs(weaponMult7 - 1f) > 0.001f)
								{
									FieldInfo field6 = type3.GetField(text4 + "ReloadTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
									if (field6 != null)
									{
										float num14 = (float)field6.GetValue(component);
										float num15 = Math.Max(0.1f, num14 * weaponMult7);
										if (useOM)
										{
											OMSetFloat(target, text4 + "ReloadTime", num15);
										}
										else
										{
											field6.SetValue(component, num15);
										}
										LogDebug(string.Format("  VT[{0}].{1}ReloadTime: {2:F2} -> {3:F2}{4}", num2, text4, num14, num15, useOM ? " (OM)" : " (direct)"));
									}
								}
							}
							if (flag7)
							{
								float weaponMult8 = GetWeaponMult(_fireRateMultipliers, text, weapon3);
								if (Math.Abs(weaponMult8 - 1f) > 0.001f)
								{
									FieldInfo field7 = type3.GetField(text4 + "FireInterval", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
									if (field7 != null && field7.FieldType == typeof(float))
									{
										float num16 = (float)field7.GetValue(component);
										float num17 = Math.Max(0.01f, num16 / weaponMult8);
										if (useOM)
										{
											OMSetFloat(target, text4 + "FireInterval", num17);
										}
										else
										{
											field7.SetValue(component, num17);
										}
										LogDebug($"  VehicleTurret.{text4}FireInterval: {num16:F4} -> {num17:F4}");
									}
								}
							}
							if (flag6)
							{
								float weaponMult9 = GetWeaponMult(_magazineMultipliers, text, weapon3);
								if (Math.Abs(weaponMult9 - 1f) > 0.001f)
								{
									FieldInfo field8 = type3.GetField(text4 + "MagazineSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
									if (field8 != null && field8.FieldType == typeof(int))
									{
										int num18 = (int)field8.GetValue(component);
										int num19 = Math.Max(1, (int)Math.Round((float)num18 * weaponMult9));
										if (useOM)
										{
											OMSetInt(target, text4 + "MagazineSize", num19);
										}
										else
										{
											field8.SetValue(component, num19);
										}
										LogDebug(string.Format("  VT[{0}].{1}MagazineSize: {2} -> {3}{4}", num2, text4, num18, num19, useOM ? " (OM)" : " (direct)"));
									}
								}
							}
							if (!flag5)
							{
								continue;
							}
							float weaponMult10 = GetWeaponMult(_accuracyMultipliers, text, weapon3);
							if (!(Math.Abs(weaponMult10 - 1f) > 0.001f))
							{
								continue;
							}
							FieldInfo field9 = type3.GetField(text4 + "MuzzleSpread", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							if (field9 != null && field9.FieldType == typeof(float))
							{
								float num20 = (float)field9.GetValue(component);
								float num21 = num20 * weaponMult10;
								if (useOM)
								{
									OMSetFloat(target, text4 + "MuzzleSpread", num21);
								}
								else
								{
									field9.SetValue(component, num21);
								}
								LogDebug($"  VehicleTurret.{text4}MuzzleSpread: {num20:F4} -> {num21:F4}");
							}
						}
						num2++;
					}
					if (!(flag | flag2 | flag3))
					{
						continue;
					}
					FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					foreach (FieldInfo fieldInfo in fields)
					{
						if (fieldInfo.FieldType.Name != "ProjectileData")
						{
							continue;
						}
						object value2;
						try
						{
							value2 = fieldInfo.GetValue(component);
						}
						catch
						{
							continue;
						}
						if (value2 == null)
						{
							continue;
						}
						string text5 = "";
						try
						{
							text5 = ((UnityEngine.Object)value2).name;
						}
						catch
						{
							continue;
						}
						if (!string.IsNullOrEmpty(text5) && !hashSet.Contains(text5))
						{
							string weapon4;
							if (name == "VehicleTurret")
							{
								int value3 = 0;
								_vtPriIndex.TryGetValue(text, out value3);
								if (num2 - 1 != value3)
								{
									weapon4 = "sec";
								}
								else
								{
									weapon4 = (fieldInfo.Name.StartsWith("Secondary") ? "sec" : "pri");
								}
							}
							else
							{
								weapon4 = (fieldInfo.Name.StartsWith("Secondary") ? "sec" : "pri");
							}
							float weaponMult11 = GetWeaponMult(_rangeMultipliers, text, weapon4);
							float weaponMult12 = GetWeaponMult(_speedMultipliers, text, weapon4);
							float weaponMult13 = GetWeaponMult(_lifetimeMultipliers, text, weapon4);
							bool flag13 = Math.Abs(weaponMult11 - 1f) > 0.001f;
							bool flag14 = Math.Abs(weaponMult12 - 1f) > 0.001f;
							bool flag15 = Math.Abs(weaponMult13 - 1f) > 0.001f;
							if (flag13 || flag14 || flag15)
							{
								ScaleProjectileData(value2, text5, flag13 ? weaponMult11 : 1f, flag14 ? weaponMult12 : 1f, hashSet, useOM, flag15 ? weaponMult13 : 1f, text);
								flag8 = true;
							}
						}
					}
				}
				if (!flag8 && (flag | flag2 | flag3))
				{
					float weaponMult14 = GetWeaponMult(_rangeMultipliers, text, "pri");
					float weaponMult15 = GetWeaponMult(_speedMultipliers, text, "pri");
					float weaponMult16 = GetWeaponMult(_lifetimeMultipliers, text, "pri");
					bool flag16 = Math.Abs(weaponMult14 - 1f) > 0.001f;
					bool flag17 = Math.Abs(weaponMult15 - 1f) > 0.001f;
					bool flag18 = Math.Abs(weaponMult16 - 1f) > 0.001f;
					LogDebug("  No ProjectileData found on components, searching by name...");
					int num22 = 0;
					if (flag16 | flag17 | flag18)
					{
						ProjectileData[] array6 = array2;
						foreach (ProjectileData projectileData in array6)
						{
							if (!(projectileData == null) && !hashSet.Contains(projectileData.name) && projectileData.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
							{
								ScaleProjectileData(projectileData, projectileData.name, flag16 ? weaponMult14 : 1f, flag17 ? weaponMult15 : 1f, hashSet, useOM, flag18 ? weaponMult16 : 1f, text);
								num22++;
							}
						}
					}
					if (num22 == 0)
					{
						MelonLogger.Warning("  No ProjectileData asset found matching '" + text + "'");
					}
				}
				num++;
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[RANGE] Applied range overrides to {num} units");
			}
		}

		private static void BuildNetworkedPrefabIndex()
		{
			_networkedPrefabs.Clear();
			try
			{
				GameDatabase database = GameDatabase.Database;
				if (database == null)
				{
					return;
				}
				List<GameObject> allNetworkedPrefabs = database.AllNetworkedPrefabs;
				if (allNetworkedPrefabs == null)
				{
					return;
				}
				int num = 0;
				for (int i = 0; i < allNetworkedPrefabs.Count; i++)
				{
					GameObject gameObject = allNetworkedPrefabs[i];
					if (gameObject == null)
					{
						num++;
					}
					else
					{
						_networkedPrefabs[gameObject.name] = gameObject;
					}
				}
				if (num > 0)
				{
					MelonLogger.Warning($"[PREFAB] GameDatabase.AllNetworkedPrefabs holds {num} null entries — GetSpawnablePrefabIndex(string) throws on those");
				}
				LogDebug($"[PREFAB] indexed {_networkedPrefabs.Count} networked prefabs");
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[PREFAB] could not index networked prefabs: " + ex.Message);
			}
		}

		private static GameObject NetworkedTwin(GameObject prefab)
		{
			if (prefab == null)
			{
				return null;
			}
			if (!_networkedPrefabs.TryGetValue(prefab.name, out GameObject value) || value == null)
			{
				return null;
			}
			if ((object)value != prefab)
			{
				return value;
			}
			return null;
		}

		private static float SetSensorFloat(GameObject prefab, string field, float value)
		{
			if (prefab == null)
			{
				return -1f;
			}
			Component[] componentsInChildren = prefab.GetComponentsInChildren<Component>(includeInactive: true);
			foreach (Component component in componentsInChildren)
			{
				if (!(component == null) && !(component.GetType().Name != "Sensor"))
				{
					FieldInfo field2 = component.GetType().GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (!(field2 == null) && !(field2.FieldType != typeof(float)))
					{
						float result = (float)field2.GetValue(component);
						field2.SetValue(component, value);
						return result;
					}
				}
			}
			return -1f;
		}

		private static void ApplyTargetDistanceOverrides(bool useOM)
		{
			if (_targetDistanceOverrides.Count == 0)
			{
				return;
			}
			BuildNetworkedPrefabIndex();
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			int num2 = 0;
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text) || !_targetDistanceOverrides.TryGetValue(text, out var value))
				{
					continue;
				}
				string target = (useOM ? ("A:" + objectInfo.name + ".asset") : null);
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component in componentsInChildren)
				{
					if (component == null || component.GetType().Name != "Sensor")
					{
						continue;
					}
					FieldInfo field = component.GetType().GetField("TargetingDistance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (!(field == null) && !(field.FieldType != typeof(float)))
					{
						float num3 = (float)field.GetValue(component);
						field.SetValue(component, value);
						if (useOM)
						{
							OMSetFloat(target, "TargetingDistance", value);
						}
						GameObject gameObject = NetworkedTwin(objectInfo.Prefab);
						if (gameObject != null && SetSensorFloat(gameObject, "TargetingDistance", value) >= 0f)
						{
							num2++;
						}
						LogDebug(string.Format("[TARGETDIST] {0}: Sensor.TargetingDistance {1} -> {2}{3}", text, num3, value, useOM ? " (OM+direct)" : " (direct)"));
						num++;
						break;
					}
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[TARGETDIST] Applied target distance overrides to {num} units ({num2} networked prefab twins)");
			}
		}

		private static void ApplyFoWDistanceOverrides(bool useOM)
		{
			if (_fowDistanceOverrides.Count == 0)
			{
				return;
			}
			BuildNetworkedPrefabIndex();
			_fowPrefabsTouched.Clear();
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			int num2 = 0;
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text) || !_fowDistanceOverrides.TryGetValue(text, out var value))
				{
					continue;
				}
				string target = (useOM ? ("A:" + objectInfo.name + ".asset") : null);
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component in componentsInChildren)
				{
					if (component == null || component.GetType().Name != "Sensor")
					{
						continue;
					}
					FieldInfo field = component.GetType().GetField("FogOfWarViewDistance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (!(field == null) && !(field.FieldType != typeof(float)))
					{
						float num3 = (float)field.GetValue(component);
						field.SetValue(component, value);
						if (useOM)
						{
							OMSetFloat(target, "FogOfWarViewDistance", value);
						}
						_fowPrefabsTouched.Add(objectInfo.Prefab.GetInstanceID());
						GameObject gameObject = NetworkedTwin(objectInfo.Prefab);
						if (gameObject != null && SetSensorFloat(gameObject, "FogOfWarViewDistance", value) >= 0f)
						{
							_fowPrefabsTouched.Add(gameObject.GetInstanceID());
							num2++;
						}
						LogDebug(string.Format("[FOW] {0}: FogOfWarViewDistance {1} -> {2}{3}", text, num3, value, useOM ? " (OM+direct)" : " (direct)"));
						num++;
						break;
					}
				}
			}
			ApplyFoWToBaseStructures(useOM);
			if (num > 0)
			{
				MelonLogger.Msg($"[FOW] Applied FoW distance overrides to {num} units ({num2} networked prefab twins)");
			}
		}

		internal static void FlushPrefabPool(string reason)
		{
			try
			{
				Type typeFromHandle = typeof(PrefabPool);
				FieldInfo fieldInfo = typeFromHandle.GetField("Pools", BindingFlags.Static | BindingFlags.NonPublic) ?? typeFromHandle.GetField("Pools", BindingFlags.Static | BindingFlags.Public);
				if (fieldInfo == null)
				{
					MelonLogger.Warning("[POOL] PrefabPool.Pools not found — stale clones cannot be dropped");
					return;
				}
				if (!(fieldInfo.GetValue(null) is IDictionary dictionary))
				{
					MelonLogger.Warning("[POOL] PrefabPool.Pools is not a dictionary");
					return;
				}
				int num = 0;
				int num2 = 0;
				List<IEnumerable> list = new List<IEnumerable>();
				foreach (DictionaryEntry item2 in dictionary)
				{
					if (item2.Value is IEnumerable item)
					{
						list.Add(item);
					}
				}
				foreach (IEnumerable item3 in list)
				{
					num++;
					foreach (object item4 in item3)
					{
						GameObject gameObject = item4 as GameObject;
						if (!(gameObject == null))
						{
							num2++;
							try
							{
								UnityEngine.Object.Destroy(gameObject);
							}
							catch
							{
							}
						}
					}
				}
				dictionary.Clear();
				MelonLogger.Msg($"[POOL] {reason}: dropped {num2} pre-override clone(s) across {num} prefab(s) — the pool refills from the overridden prefabs");
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[POOL] flush threw: " + ex.Message);
			}
		}

		internal static void ProbeLiveFoW(string tag)
		{
			try
			{
				ProbeFoWPrefabs(tag);
				List<Team> teams = Team.Teams;
				if (teams == null)
				{
					return;
				}
				for (int i = 0; i < teams.Count; i++)
				{
					Team team = teams[i];
					if (team == null)
					{
						continue;
					}
					float num = 1f;
					try
					{
						PropertyInfo property = team.GetType().GetProperty("CurrentSensorScale", BindingFlags.Instance | BindingFlags.Public);
						if (property != null)
						{
							num = (float)property.GetValue(team);
						}
					}
					catch
					{
					}
					List<Structure> structures = team.Structures;
					if (structures == null)
					{
						continue;
					}
					Dictionary<string, int> dictionary = new Dictionary<string, int>();
					for (int j = 0; j < structures.Count; j++)
					{
						Structure structure = structures[j];
						if (structure == null)
						{
							continue;
						}
						string text = "?";
						string internalName = "";
						try
						{
							text = structure.ObjectInfo?.DisplayName ?? "?";
						}
						catch
						{
						}
						try
						{
							internalName = structure.ObjectInfo?.name ?? "";
						}
						catch
						{
						}
						string text2 = ResolveConfigName(text, internalName);
						if (string.IsNullOrEmpty(text2))
						{
							text2 = text;
						}
						if (!_fowDistanceOverrides.TryGetValue(text2, out var value))
						{
							value = -1f;
						}
						dictionary.TryGetValue(text2, out var value2);
						dictionary[text2] = value2 + 1;
						string text3 = "";
						int num2 = 0;
						try
						{
							Component[] componentsInChildren = structure.GetComponentsInChildren<Component>(includeInactive: true);
							foreach (Component component in componentsInChildren)
							{
								if (component == null || component.GetType().Name != "Sensor")
								{
									continue;
								}
								FieldInfo field = component.GetType().GetField("FogOfWarViewDistance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
								if (!(field == null) && !(field.FieldType != typeof(float)))
								{
									bool flag = true;
									try
									{
										flag = ((Behaviour)component).enabled;
									}
									catch
									{
									}
									text3 = text3 + ((num2 > 0) ? "," : "") + ((float)field.GetValue(component)).ToString("F0") + (flag ? "" : "(off)");
									num2++;
								}
							}
						}
						catch
						{
						}
						float num3 = -1f;
						try
						{
							PropertyInfo property2 = structure.GetType().GetProperty("FogOfWarViewDistance", BindingFlags.Instance | BindingFlags.Public);
							if (property2 != null)
							{
								num3 = (float)property2.GetValue(structure);
							}
						}
						catch
						{
						}
						Vector3 vector = Vector3.zero;
						try
						{
							vector = structure.transform.position;
						}
						catch
						{
						}
						if (num2 != 0)
						{
							MelonLogger.Msg($"[FOW/PROBE {tag}] {team.name} {text2}#{value2} at=({vector.x:F0},{vector.z:F0}) " + $"sensors=[{text3}] n={num2} computed={num3:F0} want={value:F0} sensorScale={num:F3}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[FOW/PROBE] threw: " + ex.Message);
			}
		}

		private static void ProbeFoWPrefabs(string tag)
		{
			try
			{
				BuildNetworkedPrefabIndex();
				ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
				foreach (ObjectInfo objectInfo in array)
				{
					if (!(objectInfo == null) && !(objectInfo.Prefab == null))
					{
						string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
						if (!string.IsNullOrEmpty(text) && _fowDistanceOverrides.TryGetValue(text, out var value))
						{
							string arg = SensorValues(objectInfo.Prefab);
							GameObject gameObject = NetworkedTwin(objectInfo.Prefab);
							string arg2 = ((gameObject == null) ? "none" : $"#{gameObject.GetInstanceID()} [{SensorValues(gameObject)}]");
							MelonLogger.Msg($"[FOW/PREFAB {tag}] {text} asset={objectInfo.name} prefab#{objectInfo.Prefab.GetInstanceID()} " + $"sensors=[{arg}] want={value:F0} twin={arg2}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[FOW/PREFAB] threw: " + ex.Message);
			}
		}

		private static string SensorValues(GameObject go)
		{
			string text = "";
			int num = 0;
			try
			{
				Component[] componentsInChildren = go.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component in componentsInChildren)
				{
					if (!(component == null) && !(component.GetType().Name != "Sensor"))
					{
						FieldInfo field = component.GetType().GetField("FogOfWarViewDistance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (!(field == null) && !(field.FieldType != typeof(float)))
						{
							text = text + ((num > 0) ? "," : "") + ((float)field.GetValue(component)).ToString("F0");
							num++;
						}
					}
				}
			}
			catch
			{
			}
			if (num != 0)
			{
				return text;
			}
			return "none";
		}

		private static void ApplyFoWToBaseStructures(bool useOM)
		{
			try
			{
				List<Team> teams = Team.Teams;
				if (teams == null)
				{
					return;
				}
				for (int i = 0; i < teams.Count; i++)
				{
					Team team = teams[i];
					if (team == null)
					{
						continue;
					}
					ObjectInfo objectInfo = null;
					try
					{
						objectInfo = team.BaseStructure;
					}
					catch
					{
					}
					if (objectInfo == null || objectInfo.Prefab == null)
					{
						continue;
					}
					string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
					if (!string.IsNullOrEmpty(text) && _fowDistanceOverrides.TryGetValue(text, out var value))
					{
						int instanceID = objectInfo.Prefab.GetInstanceID();
						bool flag = _fowPrefabsTouched.Contains(instanceID);
						float num = SetSensorFloat(objectInfo.Prefab, "FogOfWarViewDistance", value);
						if (useOM)
						{
							OMSetFloat("A:" + objectInfo.name + ".asset", "FogOfWarViewDistance", value);
						}
						MelonLogger.Msg($"[FOW/BASE] {team.name} BaseStructure {text}: prefab#{instanceID} sensor {num} -> {value} (covered by sweep: {flag}, om: {useOM})");
						GameObject gameObject = NetworkedTwin(objectInfo.Prefab);
						if (gameObject != null)
						{
							float num2 = SetSensorFloat(gameObject, "FogOfWarViewDistance", value);
							MelonLogger.Msg($"[FOW/BASE] {team.name} BaseStructure {text}: networked twin#{gameObject.GetInstanceID()} sensor {num2} -> {value}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[FOW/BASE] threw: " + ex.Message);
			}
		}

		private static void ApplyJumpSpeedOverrides(bool useOM)
		{
			if (_jumpSpeedMultipliers.Count == 0)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text) || !_jumpSpeedMultipliers.TryGetValue(text, out var value))
				{
					continue;
				}
				string target = (useOM ? ("A:" + objectInfo.name + ".asset") : null);
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component in componentsInChildren)
				{
					if (component == null)
					{
						continue;
					}
					string name = component.GetType().Name;
					if (name != "Soldier" && name != "PlayerMovement" && name != "FPSMovement")
					{
						continue;
					}
					FieldInfo field = component.GetType().GetField("JumpSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (!(field == null) && !(field.FieldType != typeof(float)))
					{
						float num2 = (float)field.GetValue(component);
						float num3 = num2 * value;
						if (useOM && OMSetFloat(target, "JumpSpeed", num3))
						{
							LogDebug($"[JUMP] {text}: JumpSpeed {num2:F1} -> {num3:F1} x{value:F2} (OM)");
							num++;
						}
						else if (!useOM)
						{
							field.SetValue(component, num3);
							LogDebug($"[JUMP] {text}: JumpSpeed {num2:F1} -> {num3:F1} x{value:F2} (direct)");
							num++;
						}
					}
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[JUMP] Applied jump speed overrides to {num} units");
			}
		}

		private static void ApplyVisibleEventRadiusOverrides(bool useOM)
		{
			if (_visibleEventRadiusMultipliers.Count == 0)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			HashSet<string> hashSet = new HashSet<string>();
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text) || !_visibleEventRadiusMultipliers.TryGetValue(text, out var value))
				{
					continue;
				}
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				bool flag = false;
				Component[] array3 = componentsInChildren;
				foreach (Component component in array3)
				{
					if (component == null)
					{
						continue;
					}
					string name = component.GetType().Name;
					if (name == "VehicleTurret")
					{
						string[] array4 = new string[2] { "PrimaryProjectile", "SecondaryProjectile" };
						foreach (string name2 in array4)
						{
							FieldInfo field = component.GetType().GetField(name2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							if (field == null)
							{
								continue;
							}
							object value2 = field.GetValue(component);
							if (value2 == null)
							{
								continue;
							}
							string text2 = (value2 as UnityEngine.Object)?.name ?? "";
							if (string.IsNullOrEmpty(text2) || hashSet.Contains(text2))
							{
								continue;
							}
							float floatMember = GetFloatMember(value2, "VisibleEventRadius");
							if (!(floatMember <= 0f))
							{
								float num2 = floatMember * value;
								string target = (useOM ? ("A:" + text2 + ".asset") : null);
								if (useOM)
								{
									OMSetFloat(target, "VisibleEventRadius", num2);
								}
								else
								{
									SetFloatMember(value2, "VisibleEventRadius", num2);
								}
								LogDebug($"[VER] {text}: {text2} VisibleEventRadius {floatMember:F0} -> {num2:F0} x{value:F2}");
								hashSet.Add(text2);
								flag = true;
							}
						}
					}
					if (name == "CreatureDecapod")
					{
						string[] array4 = new string[2] { "AttackPrimary", "AttackSecondary" };
						foreach (string name3 in array4)
						{
							FieldInfo field2 = component.GetType().GetField(name3, BindingFlags.Instance | BindingFlags.Public);
							if (field2 == null)
							{
								continue;
							}
							object value3;
							try
							{
								value3 = field2.GetValue(component);
							}
							catch
							{
								continue;
							}
							if (value3 == null)
							{
								continue;
							}
							FieldInfo field3 = value3.GetType().GetField("AttackProjectileData", BindingFlags.Instance | BindingFlags.Public);
							if (field3 == null)
							{
								continue;
							}
							object value4;
							try
							{
								value4 = field3.GetValue(value3);
							}
							catch
							{
								continue;
							}
							if (value4 == null)
							{
								continue;
							}
							string text3 = "";
							try
							{
								text3 = ((UnityEngine.Object)value4).name;
							}
							catch
							{
								continue;
							}
							if (string.IsNullOrEmpty(text3) || hashSet.Contains(text3))
							{
								continue;
							}
							float floatMember2 = GetFloatMember(value4, "VisibleEventRadius");
							if (!(floatMember2 <= 0f))
							{
								float num3 = floatMember2 * value;
								string target2 = (useOM ? ("A:" + text3 + ".asset") : null);
								if (useOM)
								{
									OMSetFloat(target2, "VisibleEventRadius", num3);
								}
								else
								{
									SetFloatMember(value4, "VisibleEventRadius", num3);
								}
								LogDebug($"[VER] {text}: {text3} VisibleEventRadius {floatMember2:F0} -> {num3:F0} x{value:F2}");
								hashSet.Add(text3);
								flag = true;
							}
						}
					}
					FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					foreach (FieldInfo fieldInfo in fields)
					{
						if (fieldInfo.FieldType.Name != "ProjectileData")
						{
							continue;
						}
						object value5;
						try
						{
							value5 = fieldInfo.GetValue(component);
						}
						catch
						{
							continue;
						}
						if (value5 == null)
						{
							continue;
						}
						string text4 = "";
						try
						{
							text4 = ((UnityEngine.Object)value5).name;
						}
						catch
						{
							continue;
						}
						if (string.IsNullOrEmpty(text4) || hashSet.Contains(text4))
						{
							continue;
						}
						float floatMember3 = GetFloatMember(value5, "VisibleEventRadius");
						if (!(floatMember3 <= 0f))
						{
							float num4 = floatMember3 * value;
							string target3 = (useOM ? ("A:" + text4 + ".asset") : null);
							if (useOM)
							{
								OMSetFloat(target3, "VisibleEventRadius", num4);
							}
							else
							{
								SetFloatMember(value5, "VisibleEventRadius", num4);
							}
							LogDebug($"[VER] {text}: {text4} VisibleEventRadius {floatMember3:F0} -> {num4:F0} x{value:F2}");
							hashSet.Add(text4);
							flag = true;
						}
					}
				}
				if (flag)
				{
					num++;
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[VER] Applied VisibleEventRadius overrides to {num} units");
			}
		}

		private static void ApplyMoveSpeedOverrides(bool useOM)
		{
			if (_moveSpeedMultipliers.Count == 0 && _turboSpeedMultipliers.Count == 0 && _flySpeedMultipliers.Count == 0 && _runSpeedMultipliers.Count == 0 && _sprintSpeedMultipliers.Count == 0)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				bool flag = _moveSpeedMultipliers.TryGetValue(text, out var value);
				bool flag2 = _turboSpeedMultipliers.TryGetValue(text, out var value2);
				bool flag3 = _flySpeedMultipliers.TryGetValue(text, out var value3);
				bool flag4 = _runSpeedMultipliers.TryGetValue(text, out var value4);
				bool flag5 = _sprintSpeedMultipliers.TryGetValue(text, out var value5);
				if (!flag && !flag2 && !flag3 && !flag4 && !flag5)
				{
					continue;
				}
				string target = (useOM ? ("A:" + objectInfo.name + ".asset") : null);
				if (flag)
				{
					LogDebug(string.Format("[MOVESPEED] Applying move_speed_mult x{0:F2} to '{1}'{2}", value, text, useOM ? " (OM)" : ""));
				}
				if (flag3)
				{
					LogDebug(string.Format("[MOVESPEED] Applying fly_speed_mult x{0:F2} to '{1}'{2}", value3, text, useOM ? " (OM)" : ""));
				}
				if (flag2)
				{
					LogDebug(string.Format("[MOVESPEED] Applying turbo_speed_mult x{0:F2} to '{1}'{2}", value2, text, useOM ? " (OM)" : ""));
				}
				if (flag4)
				{
					LogDebug(string.Format("[MOVESPEED] Applying run_speed_mult x{0:F2} to '{1}'{2}", value4, text, useOM ? " (OM)" : ""));
				}
				if (flag5)
				{
					LogDebug(string.Format("[MOVESPEED] Applying sprint_speed_mult x{0:F2} to '{1}'{2}", value5, text, useOM ? " (OM)" : ""));
				}
				if (useOM)
				{
					Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
					int num2 = 0;
					Component[] array3 = componentsInChildren;
					foreach (Component component in array3)
					{
						if (component == null)
						{
							continue;
						}
						Type type = component.GetType();
						string[] speedFieldNames = _speedFieldNames;
						foreach (string text2 in speedFieldNames)
						{
							float num3;
							switch (text2)
							{
							case "FlyMoveSpeed":
								if (flag3)
								{
									num3 = value3;
									break;
								}
								if (!flag)
								{
									continue;
								}
								num3 = value;
								break;
							case "TurboSpeed":
								if (flag2)
								{
									num3 = value2;
									break;
								}
								if (!flag)
								{
									continue;
								}
								num3 = value;
								break;
							case "RunSpeed":
								if (flag4)
								{
									num3 = value4;
									break;
								}
								if (!flag)
								{
									continue;
								}
								num3 = value;
								break;
							case "SprintSpeed":
								if (flag5)
								{
									num3 = value5;
									break;
								}
								if (!flag)
								{
									continue;
								}
								num3 = value;
								break;
							default:
								if (!flag)
								{
									continue;
								}
								num3 = value;
								break;
							}
							FieldInfo field = type.GetField(text2, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
							float num4;
							if (field != null && field.FieldType == typeof(float))
							{
								num4 = (float)field.GetValue(component);
							}
							else
							{
								PropertyInfo property = type.GetProperty(text2, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
								if (property == null || property.PropertyType != typeof(float))
								{
									continue;
								}
								num4 = (float)property.GetValue(component);
							}
							if (!(num4 <= 0f))
							{
								string key = text + "_" + type.Name + "_" + text2;
								if (!_originalMoveSpeeds.ContainsKey(key))
								{
									_originalMoveSpeeds[key] = num4;
								}
								else
								{
									num4 = _originalMoveSpeeds[key];
								}
								float num5 = num4 * num3;
								if (OMSetFloat(target, text2, num5))
								{
									LogDebug($"  {type.Name}.{text2}: {num4} -> {num5} (OM)");
									num2++;
								}
							}
						}
						if (num2 > 0)
						{
							break;
						}
					}
					if (num2 > 0)
					{
						num++;
					}
					else if (flag && ModifyPrefabSpeeds(objectInfo.Prefab, text, value) > 0)
					{
						num++;
					}
				}
				else if (flag && ModifyPrefabSpeeds(objectInfo.Prefab, text, value) > 0)
				{
					num++;
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[MOVESPEED] Applied move speed overrides to {num} units");
			}
		}

		private static void ApplyStrafeSpeedOverrides(bool useOM)
		{
			if (_strafeSpeedMultipliers.Count == 0)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text) || !_strafeSpeedMultipliers.TryGetValue(text, out var value))
				{
					continue;
				}
				string target = (useOM ? ("A:" + objectInfo.name + ".asset") : null);
				LogDebug(string.Format("[STRAFE] Applying strafe_speed_mult x{0:F2} to '{1}'{2}", value, text, useOM ? " (OM)" : ""));
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				bool flag = false;
				Component[] array3 = componentsInChildren;
				foreach (Component component in array3)
				{
					if (component == null)
					{
						continue;
					}
					Type type = component.GetType();
					string name = type.Name;
					if (name == "CreatureDecapod")
					{
						bool flag2 = false;
						FieldInfo field = type.GetField("FlyMoveScaleSide", BindingFlags.Instance | BindingFlags.Public);
						float num2;
						if (field != null && field.FieldType == typeof(float))
						{
							num2 = (float)field.GetValue(component);
							flag2 = true;
						}
						else
						{
							PropertyInfo property = type.GetProperty("FlyMoveScaleSide", BindingFlags.Instance | BindingFlags.Public);
							if (property != null && property.PropertyType == typeof(float))
							{
								num2 = (float)property.GetValue(component);
								flag2 = true;
							}
							else
							{
								num2 = 0f;
							}
						}
						if (!flag2)
						{
							break;
						}
						float num3 = num2 * value;
						if (useOM && OMSetFloat(target, "FlyMoveScaleSide", num3))
						{
							LogDebug($"  CreatureDecapod.FlyMoveScaleSide: {num2} -> {num3} (OM)");
						}
						else
						{
							if (field != null)
							{
								field.SetValue(component, num3);
							}
							else
							{
								type.GetProperty("FlyMoveScaleSide", BindingFlags.Instance | BindingFlags.Public)?.SetValue(component, num3);
							}
							LogDebug($"  CreatureDecapod.FlyMoveScaleSide: {num2} -> {num3} (direct)");
						}
						flag = true;
						break;
					}
					if (!(name == "VehicleAir"))
					{
						continue;
					}
					bool flag3 = false;
					FieldInfo field2 = type.GetField("StrafeSpeed", BindingFlags.Instance | BindingFlags.Public);
					float num4;
					if (field2 != null && field2.FieldType == typeof(float))
					{
						num4 = (float)field2.GetValue(component);
						flag3 = true;
					}
					else
					{
						PropertyInfo property2 = type.GetProperty("StrafeSpeed", BindingFlags.Instance | BindingFlags.Public);
						if (property2 != null && property2.PropertyType == typeof(float))
						{
							num4 = (float)property2.GetValue(component);
							flag3 = true;
						}
						else
						{
							num4 = 0f;
						}
					}
					if (!flag3)
					{
						break;
					}
					float num5 = num4 * value;
					if (useOM && OMSetFloat(target, "StrafeSpeed", num5))
					{
						LogDebug($"  VehicleAir.StrafeSpeed: {num4} -> {num5} (OM)");
					}
					else
					{
						if (field2 != null)
						{
							field2.SetValue(component, num5);
						}
						else
						{
							type.GetProperty("StrafeSpeed", BindingFlags.Instance | BindingFlags.Public)?.SetValue(component, num5);
						}
						LogDebug($"  VehicleAir.StrafeSpeed: {num4} -> {num5} (direct)");
					}
					flag = true;
					break;
				}
				if (flag)
				{
					num++;
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[STRAFE] Applied strafe speed overrides to {num} units");
			}
		}

		private static void ApplyTurnRadiusOverrides(bool useOM)
		{
			if (_turnRadiusMultipliers.Count == 0)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text) || !_turnRadiusMultipliers.TryGetValue(text, out var value))
				{
					continue;
				}
				string target = (useOM ? ("A:" + objectInfo.name + ".asset") : null);
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component in componentsInChildren)
				{
					if (component == null || component.GetType().Name != "VehicleWheeled")
					{
						continue;
					}
					FieldInfo field = component.GetType().GetField("TurningCircleRadius", BindingFlags.Instance | BindingFlags.Public);
					if (field == null || field.FieldType != typeof(float))
					{
						continue;
					}
					float num2 = (float)field.GetValue(component);
					if (!(num2 <= 0f))
					{
						float num3 = num2 * value;
						if (useOM && OMSetFloat(target, "TurningCircleRadius", num3))
						{
							LogDebug($"[TURNRADIUS] {text}: TurningCircleRadius {num2:F1} -> {num3:F1} (OM)");
							num++;
						}
						else
						{
							field.SetValue(component, num3);
							LogDebug($"[TURNRADIUS] {text}: TurningCircleRadius {num2:F1} -> {num3:F1} (direct)");
							num++;
						}
						break;
					}
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[TURNRADIUS] Applied turn radius overrides to {num} units");
			}
		}

		private static void ApplyTeleportOverrides(bool useOM)
		{
			if (_teleportCooldown < 0f && _teleportDuration < 0f)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public;
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component in componentsInChildren)
				{
					if (component == null || component.GetType().Name != "TeleportUI")
					{
						continue;
					}
					string arg = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
					string target = (useOM ? ("A:" + objectInfo.name + ".asset") : null);
					if (_teleportCooldown >= 0f)
					{
						FieldInfo field = component.GetType().GetField("TeleportCooldownTime", bindingAttr);
						if (field != null && field.FieldType == typeof(float))
						{
							float num2 = (float)field.GetValue(component);
							if (useOM && OMSetFloat(target, "TeleportCooldownTime", _teleportCooldown))
							{
								LogDebug($"[TELEPORT] {arg}: CooldownTime {num2:F1} -> {_teleportCooldown:F1} (OM)");
								num++;
							}
							else
							{
								field.SetValue(component, _teleportCooldown);
								LogDebug($"[TELEPORT] {arg}: CooldownTime {num2:F1} -> {_teleportCooldown:F1} (direct)");
								num++;
							}
						}
					}
					if (!(_teleportDuration >= 0f))
					{
						break;
					}
					FieldInfo field2 = component.GetType().GetField("TeleportTime", bindingAttr);
					if (field2 != null && field2.FieldType == typeof(float))
					{
						float num3 = (float)field2.GetValue(component);
						if (useOM && OMSetFloat(target, "TeleportTime", _teleportDuration))
						{
							LogDebug($"[TELEPORT] {arg}: TeleportTime {num3:F1} -> {_teleportDuration:F1} (OM)");
							num++;
						}
						else
						{
							field2.SetValue(component, _teleportDuration);
							LogDebug($"[TELEPORT] {arg}: TeleportTime {num3:F1} -> {_teleportDuration:F1} (direct)");
							num++;
						}
					}
					break;
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[TELEPORT] Applied teleport overrides ({num} fields)");
			}
		}

		private static void ApplyDispenserTimeoutOverrides(bool useOM)
		{
			if (_dispenseTimeout < 0f)
			{
				return;
			}
			int num = 0;
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public;
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			foreach (ObjectInfo objectInfo in array)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component in componentsInChildren)
				{
					if (component == null || component.GetType().Name != "VehicleDispenser")
					{
						continue;
					}
					string target = (useOM ? ("A:" + objectInfo.name + ".asset") : null);
					FieldInfo field = component.GetType().GetField("DispenseTimeout", bindingAttr);
					if (field != null && field.FieldType == typeof(float))
					{
						float num2 = (float)field.GetValue(component);
						if (useOM && OMSetFloat(target, "DispenseTimeout", _dispenseTimeout))
						{
							LogDebug($"[DISPENSER] {objectInfo.DisplayName}: DispenseTimeout {num2:F1} -> {_dispenseTimeout:F1} (OM)");
							num++;
						}
						else
						{
							field.SetValue(component, _dispenseTimeout);
							LogDebug($"[DISPENSER] {objectInfo.DisplayName}: DispenseTimeout {num2:F1} -> {_dispenseTimeout:F1} (direct)");
							num++;
						}
					}
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[DISPENSER] Applied dispense timeout overrides ({num} dispensers)");
			}
		}

		private static int ModifyPrefabSpeeds(GameObject prefab, string unitName, float mult)
		{
			int num = 0;
			Component[] componentsInChildren = prefab.GetComponentsInChildren<Component>(includeInactive: true);
			Component[] array = componentsInChildren;
			foreach (Component component in array)
			{
				if (component == null)
				{
					continue;
				}
				Type type = component.GetType();
				string name = type.Name;
				string text = name.ToLower();
				if (text == "transform" || text.Contains("mesh") || text.Contains("renderer") || text.Contains("animator") || text.Contains("lodgroup") || text.Contains("particle") || text.Contains("collider") || text.Contains("light") || text.Contains("audio") || text.Contains("canvas") || text.Contains("cloth") || text.Contains("rigidbody"))
				{
					continue;
				}
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				FieldInfo[] fields = type.GetFields(bindingAttr);
				foreach (FieldInfo fieldInfo in fields)
				{
					if (fieldInfo.FieldType != typeof(float))
					{
						continue;
					}
					string text2 = fieldInfo.Name.ToLower();
					if (!text2.Contains("speed") || text2.Contains("turn") || text2.Contains("rotation") || text2.Contains("projectile") || text2.Contains("anim") || text2.Contains("reload") || text2.Contains("fire") || text2.Contains("attack") || text2.Contains("cool") || text2.Contains("shutter") || text2.Contains("landing"))
					{
						continue;
					}
					try
					{
						float num2 = (float)fieldInfo.GetValue(component);
						if (!(Math.Abs(num2) < 0.001f))
						{
							string key = $"{unitName}_{component.GetInstanceID()}_{fieldInfo.Name}";
							if (!_originalMoveSpeeds.ContainsKey(key))
							{
								_originalMoveSpeeds[key] = num2;
							}
							else
							{
								num2 = _originalMoveSpeeds[key];
							}
							float num3 = num2 * mult;
							fieldInfo.SetValue(component, num3);
							LogDebug($"  [{name}] field {fieldInfo.Name}: {num2} -> {num3}");
							num++;
						}
					}
					catch
					{
					}
				}
				PropertyInfo[] properties = type.GetProperties(bindingAttr);
				foreach (PropertyInfo propertyInfo in properties)
				{
					if (propertyInfo.PropertyType != typeof(float) || !propertyInfo.CanRead || !propertyInfo.CanWrite)
					{
						continue;
					}
					string text3 = propertyInfo.Name.ToLower();
					if (!text3.Contains("speed") || text3.Contains("turn") || text3.Contains("rotation") || text3.Contains("projectile") || text3.Contains("anim") || text3.Contains("reload") || text3.Contains("fire") || text3.Contains("attack") || text3.Contains("cool") || text3.Contains("shutter") || text3.Contains("landing"))
					{
						continue;
					}
					try
					{
						float num4 = (float)propertyInfo.GetValue(component);
						if (!(Math.Abs(num4) < 0.001f))
						{
							string key2 = $"{unitName}_{component.GetInstanceID()}_{propertyInfo.Name}";
							if (!_originalMoveSpeeds.ContainsKey(key2))
							{
								_originalMoveSpeeds[key2] = num4;
							}
							else
							{
								num4 = _originalMoveSpeeds[key2];
							}
							float num5 = num4 * mult;
							propertyInfo.SetValue(component, num5);
							LogDebug($"  [{name}] prop {propertyInfo.Name}: {num4} -> {num5}");
							num++;
						}
					}
					catch
					{
					}
				}
			}
			if (num == 0)
			{
				MelonLogger.Warning("[MOVESPEED] No speed fields found on '" + unitName + "' — listing gameplay components:");
				array = componentsInChildren;
				foreach (Component component2 in array)
				{
					if (!(component2 == null))
					{
						string text4 = component2.GetType().Name.ToLower();
						if (!(text4 == "transform") && !text4.Contains("mesh") && !text4.Contains("renderer") && !text4.Contains("animator") && !text4.Contains("lodgroup") && !text4.Contains("particle") && !text4.Contains("collider") && !text4.Contains("light") && !text4.Contains("audio") && !text4.Contains("canvas") && !text4.Contains("cloth") && !text4.Contains("rigidbody"))
						{
							LogDebug("  Component: " + component2.GetType().Name + " on '" + component2.gameObject.name + "'");
						}
					}
				}
			}
			return num;
		}

		private static bool GetBoolMember(object obj, string name)
		{
			Type type = obj.GetType();
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				return (bool)field.GetValue(obj);
			}
			PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				return (bool)property.GetValue(obj);
			}
			return false;
		}

		private static void SetBoolMember(object obj, string name, bool value)
		{
			Type type = obj.GetType();
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				field.SetValue(obj, value);
				return;
			}
			PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.CanWrite)
			{
				property.SetValue(obj, value);
			}
		}

		private static void ApplyProjectileTuningOverrides(bool useOM)
		{
			if (_impactScaleBySpeedOverrides.Count == 0 && _dragMultipliers.Count == 0)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			HashSet<string> hashSet = new HashSet<string>();
			int num = 0;
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				bool flag = _impactScaleBySpeedOverrides.TryGetValue(text, out var value);
				bool flag2 = _dragMultipliers.TryGetValue(text, out var value2);
				if (!flag && !flag2)
				{
					continue;
				}
				num++;
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component in componentsInChildren)
				{
					if (component == null)
					{
						continue;
					}
					FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					foreach (FieldInfo fieldInfo in fields)
					{
						if (fieldInfo.FieldType.Name != "ProjectileData")
						{
							continue;
						}
						object value3;
						try
						{
							value3 = fieldInfo.GetValue(component);
						}
						catch
						{
							continue;
						}
						if (value3 == null)
						{
							continue;
						}
						string name;
						try
						{
							name = ((UnityEngine.Object)value3).name;
						}
						catch
						{
							continue;
						}
						if (string.IsNullOrEmpty(name) || hashSet.Contains(name))
						{
							continue;
						}
						hashSet.Add(name);
						string target = (useOM ? ("A:" + name + ".asset") : null);
						if (flag)
						{
							bool boolMember = GetBoolMember(value3, "m_bImpactScaleBySpeed");
							if (boolMember != value)
							{
								if (useOM)
								{
									OMSetBool(target, "m_bImpactScaleBySpeed", value);
								}
								else
								{
									SetBoolMember(value3, "m_bImpactScaleBySpeed", value);
								}
								LogDebug($"[PROJTUNE] {text} -> {name}.m_bImpactScaleBySpeed: {boolMember} -> {value}");
							}
						}
						if (flag2)
						{
							float floatMember = GetFloatMember(value3, "m_fDragCoefficient");
							float num2 = floatMember * value2;
							if (useOM)
							{
								OMSetFloat(target, "m_fDragCoefficient", num2);
							}
							else
							{
								SetFloatMember(value3, "m_fDragCoefficient", num2);
							}
							LogDebug($"[PROJTUNE] {text} -> {name}.m_fDragCoefficient: {floatMember:F3} -> {num2:F3} (x{value2})");
						}
					}
				}
			}
			if (hashSet.Count > 0)
			{
				MelonLogger.Msg($"[UnitBalance] projectile tuning: adjusted {hashSet.Count} projectile(s) across {num} unit(s)");
			}
		}

		private static void ApplyAimCompensationOverrides()
		{
			if (_aimCompensationOverrides.Count == 0 && _aimCompVanilla.Count == 0)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				bool flag = _aimCompensationOverrides.TryGetValue(text, out var value);
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component in componentsInChildren)
				{
					if (component == null || component.GetType().Name != "VehicleTurret")
					{
						continue;
					}
					FieldInfo field = component.GetType().GetField("AimCompensation", BindingFlags.Instance | BindingFlags.Public);
					if (field == null)
					{
						continue;
					}
					int num2;
					try
					{
						num2 = Convert.ToInt32(field.GetValue(component));
					}
					catch
					{
						continue;
					}
					string text2 = ((component.gameObject != null) ? component.gameObject.name : "?");
					string key = text + "|" + text2;
					if (!_aimCompVanilla.ContainsKey(key))
					{
						_aimCompVanilla[key] = num2;
					}
					int num3 = _aimCompVanilla[key];
					if (!flag && num3 == num2)
					{
						continue;
					}
					int num4 = (flag ? value : num3);
					if (num4 != num2)
					{
						try
						{
							field.SetValue(component, Enum.ToObject(field.FieldType, num4));
							num++;
							LogDebug($"[AIMCOMP] {text} -> {text2}.AimCompensation: {num2} -> {num4}");
						}
						catch (Exception ex)
						{
							MelonLogger.Warning("[AIMCOMP] " + text + "/" + text2 + " set failed: " + ex.Message);
						}
					}
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[UnitBalance] aim_compensation: set {num} turret(s) [prefab]");
			}
			if (_aimCompensationOverrides.Count == 0)
			{
				return;
			}
			int num5 = 0;
			try
			{
				UnityEngine.Object[] array3 = UnityEngine.Object.FindObjectsOfType(typeof(Unit));
				for (int i = 0; i < array3.Length; i++)
				{
					Unit unit = array3[i] as Unit;
					if (unit == null || unit.ObjectInfo == null)
					{
						continue;
					}
					string text3 = ResolveConfigName(unit.ObjectInfo.DisplayName, unit.ObjectInfo.name);
					if (string.IsNullOrEmpty(text3) || !_aimCompensationOverrides.TryGetValue(text3, out var value2))
					{
						continue;
					}
					Component[] componentsInChildren = unit.GetComponentsInChildren<Component>(includeInactive: true);
					foreach (Component component2 in componentsInChildren)
					{
						if (component2 == null || component2.GetType().Name != "VehicleTurret")
						{
							continue;
						}
						FieldInfo field2 = component2.GetType().GetField("AimCompensation", BindingFlags.Instance | BindingFlags.Public);
						if (field2 == null)
						{
							continue;
						}
						int num6;
						try
						{
							num6 = Convert.ToInt32(field2.GetValue(component2));
						}
						catch
						{
							continue;
						}
						if (num6 != value2)
						{
							try
							{
								field2.SetValue(component2, Enum.ToObject(field2.FieldType, value2));
								num5++;
								MelonLogger.Msg($"[AIMCOMP-LIVE] {text3}: AimCompensation {num6} -> {value2}");
							}
							catch
							{
							}
						}
					}
				}
			}
			catch (Exception ex2)
			{
				MelonLogger.Warning("[AIMCOMP] live pass failed: " + ex2.Message);
			}
			if (num5 > 0)
			{
				MelonLogger.Msg($"[UnitBalance] aim_compensation: set {num5} LIVE turret(s) in scene");
			}
		}

		private static void ApplyTurretPitchOverrides()
		{
			if (_pitchMaxUpOverrides.Count == 0 && _pitchMaxDownOverrides.Count == 0 && _pitchVanilla.Count == 0)
			{
				return;
			}
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			int num = 0;
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo objectInfo in array2)
			{
				if (objectInfo == null || objectInfo.Prefab == null)
				{
					continue;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component in componentsInChildren)
				{
					if (!(component == null) && !(component.GetType().Name != "VehicleTurret"))
					{
						num += SetTurretPitchField(component, text, "PitchMaxUp", _pitchMaxUpOverrides);
						num += SetTurretPitchField(component, text, "PitchMaxDown", _pitchMaxDownOverrides);
					}
				}
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[UnitBalance] pitch clamp: set {num} field(s) [prefab]");
			}
			if (_pitchMaxUpOverrides.Count == 0 && _pitchMaxDownOverrides.Count == 0)
			{
				return;
			}
			int num2 = 0;
			try
			{
				UnityEngine.Object[] array3 = UnityEngine.Object.FindObjectsOfType(typeof(Unit));
				for (int i = 0; i < array3.Length; i++)
				{
					Unit unit = array3[i] as Unit;
					if (unit == null || unit.ObjectInfo == null)
					{
						continue;
					}
					string text2 = ResolveConfigName(unit.ObjectInfo.DisplayName, unit.ObjectInfo.name);
					if (string.IsNullOrEmpty(text2))
					{
						continue;
					}
					bool flag = _pitchMaxUpOverrides.TryGetValue(text2, out var value);
					bool flag2 = _pitchMaxDownOverrides.TryGetValue(text2, out var value2);
					if (!flag && !flag2)
					{
						continue;
					}
					Component[] componentsInChildren = unit.GetComponentsInChildren<Component>(includeInactive: true);
					foreach (Component component2 in componentsInChildren)
					{
						if (!(component2 == null) && !(component2.GetType().Name != "VehicleTurret"))
						{
							if (flag)
							{
								num2 += SetTurretPitchLive(component2, "PitchMaxUp", value);
							}
							if (flag2)
							{
								num2 += SetTurretPitchLive(component2, "PitchMaxDown", value2);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[PITCH] live pass failed: " + ex.Message);
			}
			if (num2 > 0)
			{
				MelonLogger.Msg($"[UnitBalance] pitch clamp: set {num2} LIVE field(s) in scene");
			}
		}

		private static int SetTurretPitchField(Component comp, string unitName, string field, Dictionary<string, float> dict)
		{
			FieldInfo field2 = comp.GetType().GetField(field, BindingFlags.Instance | BindingFlags.Public);
			if (field2 == null)
			{
				return 0;
			}
			float num;
			try
			{
				num = (float)field2.GetValue(comp);
			}
			catch
			{
				return 0;
			}
			string text = ((comp.gameObject != null) ? comp.gameObject.name : "?");
			string key = unitName + "|" + text + "|" + field;
			if (!_pitchVanilla.ContainsKey(key))
			{
				_pitchVanilla[key] = num;
			}
			float num2 = _pitchVanilla[key];
			bool flag = dict.TryGetValue(unitName, out var value);
			if (!flag && Math.Abs(num2 - num) < 0.01f)
			{
				return 0;
			}
			float num3 = (flag ? value : num2);
			if (Math.Abs(num3 - num) < 0.01f)
			{
				return 0;
			}
			try
			{
				field2.SetValue(comp, num3);
				LogDebug($"[PITCH] {unitName} -> {text}.{field}: {num:F0} -> {num3:F0}");
				return 1;
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[PITCH] " + unitName + "/" + text + "." + field + " set failed: " + ex.Message);
				return 0;
			}
		}

		private static int SetTurretPitchLive(Component comp, string field, float ov)
		{
			FieldInfo field2 = comp.GetType().GetField(field, BindingFlags.Instance | BindingFlags.Public);
			if (field2 == null)
			{
				return 0;
			}
			float num;
			try
			{
				num = (float)field2.GetValue(comp);
			}
			catch
			{
				return 0;
			}
			if (Math.Abs(num - ov) < 0.01f)
			{
				return 0;
			}
			try
			{
				field2.SetValue(comp, ov);
				MelonLogger.Msg($"[PITCH-LIVE] {field} {num:F0} -> {ov:F0}");
				return 1;
			}
			catch
			{
				return 0;
			}
		}

		private static bool HasAnyLeadMult(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}
			if (!_aiLeadDragOverrides.ContainsKey(name) && !_aiLeadVelocityOverrides.ContainsKey(name))
			{
				return _aiLeadGravityOverrides.ContainsKey(name);
			}
			return true;
		}

		private static Team GetDefaultTeam(BaseGameObject obj)
		{
			Type type = obj.GetType();
			while (type != null)
			{
				FieldInfo field = type.GetField("DefaultTeam", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
				if (field != null)
				{
					return field.GetValue(obj) as Team;
				}
				PropertyInfo property = type.GetProperty("DefaultTeam", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
				if (property != null)
				{
					return property.GetValue(obj) as Team;
				}
				type = type.BaseType;
			}
			return null;
		}

		private static float GetFloatMember(object obj, string name)
		{
			Type type = obj.GetType();
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				return (float)field.GetValue(obj);
			}
			PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				return (float)property.GetValue(obj);
			}
			return 0f;
		}

		private static bool SetFloatMember(object obj, string name, float value)
		{
			Type type = obj.GetType();
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				field.SetValue(obj, value);
				return true;
			}
			PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.CanWrite)
			{
				property.SetValue(obj, value);
				return true;
			}
			return false;
		}

		private static void ScaleProjectileData(object pdObj, string pdName, float rangeMult, float speedMult, HashSet<string> modifiedPD, bool useOM = false, float lifetimeMult = 1f, string unitName = null)
		{
			string text = (useOM ? ("A:" + pdName + ".asset") : null);
			float num = GetFloatMember(pdObj, "m_fLifeTime");
			string key = pdName + "_lifetime";
			if (!_originalProjectileLifetimes.ContainsKey(key))
			{
				_originalProjectileLifetimes[key] = num;
			}
			else
			{
				num = _originalProjectileLifetimes[key];
			}
			float num2 = GetFloatMember(pdObj, "m_fBaseSpeed");
			string key2 = pdName + "_speed";
			if (!_originalProjectileSpeeds.ContainsKey(key2))
			{
				_originalProjectileSpeeds[key2] = num2;
			}
			else
			{
				num2 = _originalProjectileSpeeds[key2];
			}
			bool flag = false;
			try
			{
				FieldInfo field = pdObj.GetType().GetField("m_InstantHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null)
				{
					flag = (bool)field.GetValue(pdObj);
				}
				else
				{
					PropertyInfo property = pdObj.GetType().GetProperty("m_InstantHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (property != null && property.CanRead)
					{
						flag = (bool)property.GetValue(pdObj);
					}
				}
			}
			catch
			{
			}
			if (flag)
			{
				float num3 = num2 * rangeMult;
				if (Math.Abs(rangeMult - 1f) > 0.001f && num2 > 0f)
				{
					if (useOM)
					{
						OMSetFloat(text, "m_fBaseSpeed", num3);
					}
					else
					{
						SetFloatMember(pdObj, "m_fBaseSpeed", num3);
					}
				}
				if (Math.Abs(speedMult - 1f) > 0.001f && num2 > 0f)
				{
					float num4 = num3 * speedMult;
					if (useOM)
					{
						OMSetFloat(text, "m_fBaseSpeed", num4);
					}
					else
					{
						SetFloatMember(pdObj, "m_fBaseSpeed", num4);
					}
					num3 = num4;
				}
				LogDebug(string.Format("  ProjectileData '{0}' (instant-hit): Range {1:F0} -> {2:F0}{3}", pdName, num2, num3, useOM ? " (OM)" : ""));
			}
			else
			{
				float num5 = num * rangeMult * lifetimeMult;
				float num6 = num2 * speedMult;
				float num7 = num2 * num;
				float num8 = num6 * num5;
				if (Math.Abs(rangeMult * lifetimeMult - 1f) > 0.001f && num > 0f)
				{
					if (useOM)
					{
						OMSetFloat(text, "m_fLifeTime", num5);
					}
					else
					{
						SetFloatMember(pdObj, "m_fLifeTime", num5);
					}
					LogDebug($"  ProjectileData '{pdName}': Lifetime {num} -> {num5:F2}");
				}
				if (Math.Abs(speedMult - 1f) > 0.001f && num2 > 0f)
				{
					if (useOM)
					{
						OMSetFloat(text, "m_fBaseSpeed", num6);
					}
					else
					{
						SetFloatMember(pdObj, "m_fBaseSpeed", num6);
					}
					LogDebug($"  ProjectileData '{pdName}': Speed {num2} -> {num6:F1}");
				}
				LogDebug(string.Format("  ProjectileData '{0}': Range {1:F0} -> {2:F0}{3}", pdName, num7, num8, useOM ? " (OM)" : ""));
				bool flag2 = Math.Abs(speedMult - 1f) > 0.001f;
				if (flag2 || HasAnyLeadMult(unitName))
				{
					float scale = (flag2 ? (1f / speedMult) : 1f);
					ScaleAimLeadFactors(pdObj, pdName, text, scale, useOM, unitName);
				}
			}
			if (flag && Math.Abs(lifetimeMult - 1f) > 0.001f && num > 0f)
			{
				float num9 = num * lifetimeMult;
				if (useOM)
				{
					OMSetFloat(text, "m_fLifeTime", num9);
				}
				else
				{
					SetFloatMember(pdObj, "m_fLifeTime", num9);
				}
				LogDebug($"  ProjectileData '{pdName}' (instant-hit): Lifetime {num} -> {num9:F2}");
			}
			if ((Math.Abs(rangeMult - 1f) > 0.001f) & flag)
			{
				float floatMember = GetFloatMember(pdObj, "VisibleEventRadius");
				if (floatMember > 0f)
				{
					float num10 = floatMember * rangeMult;
					if (useOM)
					{
						OMSetFloat(text, "VisibleEventRadius", num10);
					}
					else
					{
						SetFloatMember(pdObj, "VisibleEventRadius", num10);
					}
					LogDebug($"  ProjectileData '{pdName}': VisibleEventRadius {floatMember:F0} -> {num10:F0}");
				}
			}
			modifiedPD.Add(pdName);
		}

		private static void ScaleAimLeadFactors(object pdObj, string pdName, string pdTarget, float scale, bool useOM, string unitName)
		{
			string[] array = new string[3] { "m_AIAimLeadFactor_Velocity", "m_AIAimLeadFactor_Gravity", "m_AIAimLeadFactor_Drag" };
			Dictionary<string, float>[] array2 = new Dictionary<string, float>[3] { _originalAimLeadVelocity, _originalAimLeadGravity, _originalAimLeadDrag };
			Dictionary<string, float>[] array3 = new Dictionary<string, float>[3] { _aiLeadVelocityOverrides, _aiLeadGravityOverrides, _aiLeadDragOverrides };
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				string key = pdName + "_" + text;
				float num = GetFloatMember(pdObj, text);
				if (float.IsNaN(num))
				{
					continue;
				}
				if (!array2[i].ContainsKey(key))
				{
					array2[i][key] = num;
				}
				else
				{
					num = array2[i][key];
				}
				float value = 1f;
				if (unitName != null)
				{
					array3[i].TryGetValue(unitName, out value);
				}
				if (value == 0f)
				{
					value = 1f;
				}
				float num2 = num * scale * value;
				if (!(Math.Abs(num2 - num) < 0.0001f))
				{
					if (useOM)
					{
						OMSetFloat(pdTarget, text, num2);
					}
					else
					{
						SetFloatMember(pdObj, text, num2);
					}
					if (Math.Abs(value - 1f) > 0.001f)
					{
						MelonLogger.Msg($"[LEADFACTOR] {unitName} '{pdName}': {text} {num:F3} -> {num2:F3} (speed x{scale:F3}, user x{value:F3})");
					}
					else
					{
						LogDebug($"  ProjectileData '{pdName}': {text} {num:F3} -> {num2:F3} (x{scale:F3} speed, x{value:F3} user)");
					}
				}
			}
		}

		private static void ApplyProximityDetonationOverrides()
		{
			if (_proximityOverrides.Count == 0)
			{
				return;
			}
			ProjectileData[] array = Resources.FindObjectsOfTypeAll<ProjectileData>();
			int num = 0;
			ProjectileData[] array2 = array;
			foreach (ProjectileData projectileData in array2)
			{
				if (projectileData == null)
				{
					continue;
				}
				string name = projectileData.name;
				if (string.IsNullOrEmpty(name) || !_proximityOverrides.TryGetValue(name, out Dictionary<string, float> value))
				{
					continue;
				}
				object obj = null;
				try
				{
					FieldInfo field = projectileData.GetType().GetField("m_ProjectilePrefab", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (field != null)
					{
						obj = field.GetValue(projectileData);
					}
					if (obj == null)
					{
						PropertyInfo property = projectileData.GetType().GetProperty("m_ProjectilePrefab", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (property != null)
						{
							obj = property.GetValue(projectileData);
						}
					}
				}
				catch
				{
				}
				if (obj == null)
				{
					MelonLogger.Warning("[PROXIMITY] " + name + ": m_ProjectilePrefab not found");
					continue;
				}
				GameObject gameObject = null;
				if (obj is GameObject gameObject2)
				{
					gameObject = gameObject2;
				}
				else if (obj is Component component)
				{
					gameObject = component.gameObject;
				}
				if (gameObject == null)
				{
					MelonLogger.Warning("[PROXIMITY] " + name + ": Could not resolve prefab GameObject (type: " + obj.GetType().Name + ")");
					continue;
				}
				Component component2 = null;
				Component[] componentsInChildren = gameObject.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component3 in componentsInChildren)
				{
					if (component3 != null && component3.GetType().Name == "ProjectileProximityDetonation")
					{
						component2 = component3;
						break;
					}
				}
				if (component2 == null)
				{
					MelonLogger.Warning("[PROXIMITY] " + name + ": No ProjectileProximityDetonation component on prefab '" + gameObject.name + "'");
					continue;
				}
				Type type = component2.GetType();
				LogDebug("[PROXIMITY] Applying overrides to '" + name + "' (prefab '" + gameObject.name + "'):");
				foreach (KeyValuePair<string, float> item in value)
				{
					string key = item.Key;
					float value2 = item.Value;
					if (key == "FlyingUnits" || key == "GroundUnits" || key == "Structures")
					{
						bool flag = value2 >= 0.5f;
						bool boolMember = GetBoolMember(component2, key);
						FieldInfo field2 = type.GetField(key, BindingFlags.Instance | BindingFlags.Public);
						if (field2 != null)
						{
							field2.SetValue(component2, flag);
							LogDebug($"  {key}: {boolMember} -> {flag}");
							continue;
						}
						PropertyInfo property2 = type.GetProperty(key, BindingFlags.Instance | BindingFlags.Public);
						if (property2 != null && property2.CanWrite)
						{
							property2.SetValue(component2, flag);
							LogDebug($"  {key}: {boolMember} -> {flag}");
						}
						else
						{
							MelonLogger.Warning("  " + key + ": field/property not found or read-only");
						}
					}
					else
					{
						float floatMember = GetFloatMember(component2, key);
						if (SetFloatMember(component2, key, value2))
						{
							LogDebug($"  {key}: {floatMember:F3} -> {value2:F3}");
						}
						else
						{
							MelonLogger.Warning("  " + key + ": SetFloatMember failed");
						}
					}
				}
				num++;
			}
			if (num > 0)
			{
				MelonLogger.Msg($"[PROXIMITY] Applied proximity detonation overrides to {num} projectile(s)");
			}
		}

		private static void ApplyAlienNestOnlyBuild(bool useOM)
		{
			if (!_alienNestOnlyBuild)
			{
				if (useOM || _nestOnlyMutated.Count <= 0)
				{
					return;
				}
				ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
				foreach (ObjectInfo objectInfo in array)
				{
					if (objectInfo != null && _nestOnlyMutated.Contains(objectInfo.name))
					{
						objectInfo.StructureConstructionAnchor = true;
					}
				}
				_nestOnlyMutated.Clear();
				return;
			}
			try
			{
				int num = 0;
				List<string> list = new List<string>();
				ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
				foreach (ObjectInfo objectInfo2 in array)
				{
					if (!(objectInfo2 == null) && !(objectInfo2.Prefab == null) && objectInfo2.name.StartsWith("ObjectInfo_Alien_", StringComparison.OrdinalIgnoreCase) && !objectInfo2.name.Equals("ObjectInfo_Alien_Nest", StringComparison.OrdinalIgnoreCase) && !(objectInfo2.Prefab.GetComponent<AlienStructure>() == null) && objectInfo2.StructureConstructionAnchor)
					{
						bool flag;
						if (useOM)
						{
							flag = OMSetBool("A:" + objectInfo2.name + ".asset", "StructureConstructionAnchor", value: false);
						}
						else
						{
							objectInfo2.StructureConstructionAnchor = false;
							_nestOnlyMutated.Add(objectInfo2.name);
							flag = true;
						}
						if (flag)
						{
							num++;
							list.Add(objectInfo2.DisplayName);
						}
					}
				}
				MelonLogger.Msg(string.Format("[NESTONLY] Alien nest-only build ON: anchor flag off on {0} structures ({1}){2}", num, string.Join(", ", list), useOM ? " (OM)" : ""));
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[NESTONLY] Failed: " + ex.Message);
			}
		}

		private static string FmtQueensMax(int v)
		{
			if (v >= 0)
			{
				if (v != 0)
				{
					return v.ToString();
				}
				return "every Nest";
			}
			return "vanilla";
		}

		private static void ApplyAlienQueensMax()
		{
			try
			{
				SpawnOnInit spawnOnInit = null;
				ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
				foreach (ObjectInfo objectInfo in array)
				{
					if (objectInfo == null || objectInfo.Prefab == null || !objectInfo.name.Equals("ObjectInfo_Alien_Nest", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					SpawnOnInit[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<SpawnOnInit>(includeInactive: true);
					foreach (SpawnOnInit spawnOnInit2 in componentsInChildren)
					{
						if (!(spawnOnInit2 == null) && !(spawnOnInit2.SpawnObject == null) && spawnOnInit2.SpawnObject.name.IndexOf("Queen", StringComparison.OrdinalIgnoreCase) >= 0)
						{
							spawnOnInit = spawnOnInit2;
							break;
						}
					}
					if (spawnOnInit != null)
					{
						break;
					}
				}
				if (spawnOnInit == null)
				{
					if (_alienQueensMax >= 0)
					{
						MelonLogger.Warning("[QUEENS] Nest prefab has no SpawnOnInit for the Queen — alien_queens_max inactive");
					}
					return;
				}
				if (!_originalQueenMaxPerTeam.HasValue)
				{
					_originalQueenMaxPerTeam = spawnOnInit.MaxPerTeam;
				}
				int num = ((_alienQueensMax < 0) ? _originalQueenMaxPerTeam.Value : _alienQueensMax);
				int maxPerTeam = spawnOnInit.MaxPerTeam;
				spawnOnInit.MaxPerTeam = num;
				if (maxPerTeam != num || _alienQueensMax >= 0)
				{
					MelonLogger.Msg($"[QUEENS] Nest SpawnOnInit.MaxPerTeam {maxPerTeam} -> {num} (vanilla {_originalQueenMaxPerTeam.Value}; config {FmtQueensMax(_alienQueensMax)})");
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[QUEENS] Failed: " + ex.Message);
			}
		}

		private static void ApplyShrimpAimDisable()
		{
			try
			{
				ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
				foreach (ObjectInfo objectInfo in array)
				{
					if (objectInfo == null || objectInfo.DisplayName != "Shrimp" || objectInfo.Prefab == null)
					{
						continue;
					}
					Component[] componentsInChildren = objectInfo.Prefab.GetComponentsInChildren<Component>(includeInactive: true);
					foreach (Component component in componentsInChildren)
					{
						if (component == null)
						{
							continue;
						}
						string name = component.GetType().Name;
						if (name == "CreatureDecapod")
						{
							FieldInfo field = component.GetType().GetField("AIMeleeDistance", BindingFlags.Instance | BindingFlags.Public);
							if (field != null)
							{
								float num = (float)field.GetValue(component);
								field.SetValue(component, 0f);
								LogDebug($"[SHRIMP] AIMeleeDistance: {num} -> 0");
							}
						}
						if (name == "Sensor")
						{
							FieldInfo field2 = component.GetType().GetField("TargetingDistance", BindingFlags.Instance | BindingFlags.Public);
							if (field2 != null)
							{
								float num2 = (float)field2.GetValue(component);
								field2.SetValue(component, 0f);
								LogDebug($"[SHRIMP] Sensor.TargetingDistance: {num2} -> 0");
							}
						}
						if (name == "AIAiming")
						{
							FieldInfo field3 = component.GetType().GetField("AimPaused", BindingFlags.Instance | BindingFlags.Public);
							if (field3 != null)
							{
								field3.SetValue(component, true);
								LogDebug("[SHRIMP] AIAiming.AimPaused -> True");
							}
						}
					}
					break;
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Error("[SHRIMP] ApplyShrimpAimDisable error: " + ex.Message);
			}
		}

		private static void ApplyDecayOverrides()
		{
			if (_decayHuman.IsVanilla && _decayAlien.IsVanilla)
			{
				return;
			}
			try
			{
				if (!_decayHuman.HasCustomParams && !_decayAlien.HasCustomParams)
				{
					return;
				}
				ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
				FieldInfo field = typeof(Decay).GetField("Data", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field == null)
				{
					MelonLogger.Warning("[DECAY] Decay.Data field not found via reflection");
					return;
				}
				ScriptableObject scriptableObject = null;
				ObjectInfo[] array2 = array;
				foreach (ObjectInfo objectInfo in array2)
				{
					if (objectInfo == null || objectInfo.Prefab == null)
					{
						continue;
					}
					Decay component = objectInfo.Prefab.GetComponent<Decay>();
					if (!(component == null))
					{
						ScriptableObject scriptableObject2 = field.GetValue(component) as ScriptableObject;
						if (scriptableObject2 != null)
						{
							scriptableObject = scriptableObject2;
							break;
						}
					}
				}
				if (scriptableObject == null)
				{
					MelonLogger.Warning("[DECAY] No DecayData asset found on any structure prefab");
					return;
				}
				Type type = scriptableObject.GetType();
				FieldInfo field2 = type.GetField("Tick", BindingFlags.Instance | BindingFlags.Public);
				FieldInfo field3 = type.GetField("AmountPct", BindingFlags.Instance | BindingFlags.Public);
				FieldInfo field4 = type.GetField("RandomizePct", BindingFlags.Instance | BindingFlags.Public);
				FieldInfo field5 = type.GetField("Delay", BindingFlags.Instance | BindingFlags.Public);
				float num = ((field2 != null) ? ((float)field2.GetValue(scriptableObject)) : 5f);
				float num2 = ((field3 != null) ? ((float)field3.GetValue(scriptableObject)) : 0.01f);
				float num3 = ((field4 != null) ? ((float)field4.GetValue(scriptableObject)) : 0.2f);
				float num4 = ((field5 != null) ? ((float)field5.GetValue(scriptableObject)) : 10f);
				if (_decayHuman.HasCustomParams && _decayDataHuman == null)
				{
					_decayDataHuman = ScriptableObject.CreateInstance(type);
					_decayDataHuman.name = "DecayData_Human_Override";
				}
				if (_decayAlien.HasCustomParams && _decayDataAlien == null)
				{
					_decayDataAlien = ScriptableObject.CreateInstance(type);
					_decayDataAlien.name = "DecayData_Alien_Override";
				}
				(DecaySettings, ScriptableObject)[] array3 = new (DecaySettings, ScriptableObject)[2]
				{
					(_decayHuman, _decayDataHuman),
					(_decayAlien, _decayDataAlien)
				};
				for (int i = 0; i < array3.Length; i++)
				{
					var (decaySettings, scriptableObject3) = array3[i];
					if (!(scriptableObject3 == null) && decaySettings.HasCustomParams)
					{
						if (field2 != null)
						{
							field2.SetValue(scriptableObject3, (decaySettings.Tick >= 0f) ? decaySettings.Tick : num);
						}
						if (field3 != null)
						{
							field3.SetValue(scriptableObject3, (decaySettings.AmountPct >= 0f) ? decaySettings.AmountPct : num2);
						}
						if (field4 != null)
						{
							field4.SetValue(scriptableObject3, (decaySettings.RandomizePct >= 0f) ? decaySettings.RandomizePct : num3);
						}
						if (field5 != null)
						{
							field5.SetValue(scriptableObject3, (decaySettings.Delay >= 0f) ? decaySettings.Delay : num4);
						}
					}
				}
				int num5 = 0;
				array2 = array;
				foreach (ObjectInfo objectInfo2 in array2)
				{
					if (objectInfo2 == null || objectInfo2.Prefab == null)
					{
						continue;
					}
					Decay component2 = objectInfo2.Prefab.GetComponent<Decay>();
					if (!(component2 == null))
					{
						bool flag = objectInfo2.Prefab.GetComponent<HumanStructure>() != null;
						bool flag2 = objectInfo2.Prefab.GetComponent<AlienStructure>() != null;
						ScriptableObject scriptableObject4 = null;
						if (flag && _decayHuman.HasCustomParams)
						{
							scriptableObject4 = _decayDataHuman;
						}
						else if (flag2 && _decayAlien.HasCustomParams)
						{
							scriptableObject4 = _decayDataAlien;
						}
						if (scriptableObject4 != null)
						{
							field.SetValue(component2, scriptableObject4);
							num5++;
						}
						else if (flag)
						{
						}
					}
				}
				if (num5 > 0)
				{
					MelonLogger.Msg($"[DECAY] Swapped DecayData on {num5} structure prefabs");
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Error("[DECAY] ApplyDecayOverrides error: " + ex.Message);
			}
		}

		private static void ApplyDecayTeamSettings()
		{
			if (_decayHuman.IsVanilla && _decayAlien.IsVanilla)
			{
				return;
			}
			try
			{
				PropertyInfo property = typeof(Team).GetProperty("Teams", BindingFlags.Static | BindingFlags.Public);
				if (property == null)
				{
					MelonLogger.Warning("[DECAY] Team.Teams property not found");
				}
				else
				{
					if (!(property.GetValue(null) is IList list))
					{
						return;
					}
					PropertyInfo property2 = typeof(Team).GetProperty("UnlinkedStructureStateEnabled", BindingFlags.Instance | BindingFlags.Public);
					if (property2 == null)
					{
						MelonLogger.Warning("[DECAY] Team.UnlinkedStructureStateEnabled property not found");
						return;
					}
					foreach (object item in list)
					{
						Team team = item as Team;
						if (!(team == null))
						{
							bool flag = false;
							if (team.BaseStructure != null && team.BaseStructure.Prefab != null)
							{
								flag = team.BaseStructure.Prefab.GetComponent<HumanStructure>() != null;
							}
							DecaySettings decaySettings = (flag ? _decayHuman : _decayAlien);
							if (!decaySettings.Enabled || decaySettings.KeepProduction)
							{
								property2.SetValue(team, false);
								string arg = ((!decaySettings.Enabled) ? "decay disabled" : "keep_production");
								LogDebug($"[DECAY] UnlinkedStructureStateEnabled=false for team {team.TeamShortName} (IsHuman={flag}, reason={arg})");
							}
						}
					}
					if (!_decayHuman.Enabled || !_decayAlien.Enabled)
					{
						MelonLogger.Msg("[DECAY] Team decay — Human: " + (_decayHuman.Enabled ? "ON" : "OFF") + ", Alien: " + (_decayAlien.Enabled ? "ON" : "OFF"));
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Error("[DECAY] ApplyDecayTeamSettings error: " + ex.Message);
			}
		}

		private static bool ShouldKeepProduction(Structure structure)
		{
			if (structure == null)
			{
				return false;
			}
			bool flag = structure is HumanStructure;
			bool flag2 = structure is AlienStructure;
			if (!flag && !flag2)
			{
				return false;
			}
			return (flag ? _decayHuman : _decayAlien).KeepProduction;
		}

		internal static void DecayCacheClear()
		{
			_dmCacheHuman.Clear();
			_dmCacheAlien.Clear();
		}

		private static string FmtCountCap(int v)
		{
			if (v >= 0)
			{
				return v.ToString();
			}
			return "unlimited";
		}

		private static int CountCapFor(ObjectInfo info, out string label)
		{
			label = null;
			if (info == null)
			{
				return -1;
			}
			string text = info.name ?? "";
			if (text.Equals("ObjectInfo_Alien_Shrimp", StringComparison.OrdinalIgnoreCase))
			{
				label = "Shrimp";
				return _countCapShrimp;
			}
			if (text.IndexOf("Harvester", StringComparison.OrdinalIgnoreCase) >= 0 && !text.Contains("_Intro"))
			{
				label = "Harvester";
				return _countCapHarvester;
			}
			return -1;
		}

		private static int CountTeamUnitsOf(Team team, ObjectInfo info)
		{
			int num = 0;
			List<Unit> units = team.Units;
			if (units != null)
			{
				for (int i = 0; i < units.Count; i++)
				{
					if (units[i] != null && units[i].ObjectInfo == info)
					{
						num++;
					}
				}
			}
			List<Structure> structures = team.Structures;
			if (structures != null)
			{
				for (int j = 0; j < structures.Count; j++)
				{
					List<ConstructionData> list = structures[j]?.ProductionQueue;
					if (list == null)
					{
						continue;
					}
					for (int k = 0; k < list.Count; k++)
					{
						if (list[k] != null && list[k].ObjectInfo == info)
						{
							num++;
						}
					}
				}
			}
			List<ConstructionSite> constructionSites = ConstructionSite.ConstructionSites;
			if (constructionSites != null)
			{
				for (int l = 0; l < constructionSites.Count; l++)
				{
					ConstructionSite constructionSite = constructionSites[l];
					if (constructionSite != null && constructionSite.Team == team && constructionSite.ObjectInfo == info && constructionSite.gameObject.activeSelf)
					{
						num++;
					}
				}
			}
			return num;
		}

		private static string FmtNestReach(float v)
		{
			if (!(v < 0f))
			{
				return v.ToString("F0", CultureInfo.InvariantCulture) + " m";
			}
			return "vanilla";
		}

		private static bool IsNestReachLimited(ObjectInfo info, out string label)
		{
			label = null;
			if (info == null)
			{
				return false;
			}
			string text = info.name ?? "";
			if (text.Equals("ObjectInfo_Alien_Node", StringComparison.OrdinalIgnoreCase))
			{
				label = "Node";
				return true;
			}
			if (text.Equals("ObjectInfo_Alien_BioCache", StringComparison.OrdinalIgnoreCase))
			{
				label = "Bio Cache";
				return true;
			}
			return false;
		}

		private static bool IsNestInfo(ObjectInfo info, Team team)
		{
			if (info == null)
			{
				return false;
			}
			if (team != null && team.BaseStructure != null && info == team.BaseStructure)
			{
				return true;
			}
			return string.Equals(info.name, "ObjectInfo_Alien_Nest", StringComparison.OrdinalIgnoreCase);
		}

		private static float NearestNestDistance2D(Team team, Vector3 position, out int nests)
		{
			float num = float.MaxValue;
			nests = 0;
			List<Structure> structures = team.Structures;
			if (structures != null)
			{
				for (int i = 0; i < structures.Count; i++)
				{
					Structure structure = structures[i];
					if (!(structure == null) && !structure.IsDestroyed && IsNestInfo(structure.ObjectInfo, team))
					{
						nests++;
						float num2 = GameMath.Distance2D(structure.transform.position, position);
						if (num2 < num)
						{
							num = num2;
						}
					}
				}
			}
			List<ConstructionSite> constructionSites = ConstructionSite.ConstructionSites;
			if (constructionSites != null)
			{
				for (int j = 0; j < constructionSites.Count; j++)
				{
					ConstructionSite constructionSite = constructionSites[j];
					if (!(constructionSite == null) && !(constructionSite.Team != team) && !constructionSite.IsDestroyed && constructionSite.gameObject.activeSelf && IsNestInfo(constructionSite.ObjectInfo, team))
					{
						nests++;
						float num3 = GameMath.Distance2D(constructionSite.transform.position, position);
						if (num3 < num)
						{
							num = num3;
						}
					}
				}
			}
			return num;
		}

		private static void PlaySoundToPlayer(object player, string relPath)
		{
			if (player == null || _playSoundFileMethod == null)
			{
				return;
			}
			if (!_nestReachSoundAvailable.HasValue)
			{
				string text = Path.Combine(MelonEnvironment.UserDataDirectory, relPath);
				_nestReachSoundAvailable = File.Exists(text);
				if (!_nestReachSoundAvailable.Value)
				{
					MelonLogger.Warning("[NestReach] sound file missing: " + text + " — chat warning only");
				}
			}
			if (!_nestReachSoundAvailable.Value)
			{
				return;
			}
			try
			{
				_playSoundFileMethod.Invoke(null, new object[2] { relPath, player });
			}
			catch (Exception ex)
			{
				LogDebug("[NestReach] PlaySoundFile failed: " + ex.Message);
			}
		}

		private static float NearestFinishedAnchorDistance2D(Team team, ConstructionData data, Vector3 position, Quaternion rotation, out Structure anchor)
		{
			anchor = null;
			float num = float.MaxValue;
			Bounds bounds = GameFuncs.TransformBounds(data.ObjectInfo.PhysicalBounds, position, rotation);
			bool maximumDistanceUseRadius = data.MaximumDistanceUseRadius;
			List<Structure> structures = Structure.Structures;
			if (structures == null)
			{
				return num;
			}
			for (int i = 0; i < structures.Count; i++)
			{
				Structure structure = structures[i];
				if (!(structure == null) && !(structure.Team != team) && !structure.IsDestroyed && structure.IsConstructionAnchor)
				{
					Bounds bounds2 = GameFuncs.TransformBounds(structure.PhysicalBounds, structure.transform, worldSpace: true);
					Vector3 vector = bounds.ClosestPoint(structure.transform.position);
					Vector3 to = bounds2.ClosestPoint(vector);
					float num2 = (maximumDistanceUseRadius ? GameMath.Distance2D(structure.transform.position, position - vector + position) : GameMath.Distance2D(vector, to));
					if (num2 < num)
					{
						num = num2;
						anchor = structure;
					}
				}
			}
			return num;
		}

		private static ConstructionSite FindNestUnderConstruction(Team team)
		{
			List<ConstructionSite> constructionSites = ConstructionSite.ConstructionSites;
			if (constructionSites == null)
			{
				return null;
			}
			for (int i = 0; i < constructionSites.Count; i++)
			{
				ConstructionSite constructionSite = constructionSites[i];
				if (!(constructionSite == null) && !(constructionSite.Team != team) && !constructionSite.IsDestroyed && constructionSite.gameObject.activeSelf && !constructionSite.IsDeconstructing && IsNestInfo(constructionSite.ObjectInfo, team))
				{
					return constructionSite;
				}
			}
			return null;
		}

		private static bool HasDockedLogisticsUnit(Structure st)
		{
			List<UnitCompartment> unitCompartments = st.UnitCompartments;
			if (unitCompartments == null)
			{
				return false;
			}
			for (int i = 0; i < unitCompartments.Count; i++)
			{
				UnitCompartment unitCompartment = unitCompartments[i];
				if (unitCompartment != null && unitCompartment.IsConstructionLogistics && unitCompartment.DriverUnit != null)
				{
					return true;
				}
			}
			return false;
		}

		private static Structure NearestNest(Team team, Vector3 position, out float dist)
		{
			Structure result = null;
			dist = float.MaxValue;
			List<Structure> structures = team.Structures;
			if (structures == null)
			{
				return null;
			}
			for (int i = 0; i < structures.Count; i++)
			{
				Structure structure = structures[i];
				if (!(structure == null) && !structure.IsDestroyed && IsNestInfo(structure.ObjectInfo, team))
				{
					float num = GameMath.Distance2D(structure.transform.position, position);
					if (num < dist)
					{
						dist = num;
						result = structure;
					}
				}
			}
			return result;
		}

		private static string FmtCapMult(int m)
		{
			if (m >= 0)
			{
				if (m != 0)
				{
					return "x" + m;
				}
				return "unlimited";
			}
			return "vanilla";
		}

		private static bool IsThreeWay(GameModeExt.ETeamsVersus mode)
		{
			if (mode != GameModeExt.ETeamsVersus.HUMANS_VS_HUMANS_VS_ALIENS)
			{
				return mode == GameModeExt.ETeamsVersus.HUMANS2_VS_ALIENS;
			}
			return true;
		}

		private static int GetConfiguredCapMult(GameModeExt.ETeamsVersus mode)
		{
			if (mode == GameModeExt.ETeamsVersus.NONE)
			{
				return -1;
			}
			if (!IsThreeWay(mode))
			{
				return _unitCapMult2Way;
			}
			return _unitCapMult3Way;
		}

		private static void ApplyUnitCapMultiplier(string reason)
		{
			try
			{
				GameModeExt gameModeExt = GameMode.CurrentGameMode as GameModeExt;
				if (gameModeExt == null || Game.GetIsClient())
				{
					return;
				}
				GameModeExt.ETeamsVersus teamsVersus = gameModeExt.TeamsVersus;
				int configuredCapMult = GetConfiguredCapMult(teamsVersus);
				if (configuredCapMult < 0)
				{
					if (teamsVersus != GameModeExt.ETeamsVersus.NONE && !_inVanillaCapMultRefresh)
					{
						_inVanillaCapMultRefresh = true;
						try
						{
							gameModeExt.UpdateUnitCapMultFromSetting();
						}
						finally
						{
							_inVanillaCapMultRefresh = false;
						}
						LogDebug($"[UNITCAPMULT] {teamsVersus}: not configured, vanilla value {gameModeExt.UnitCapMultiplier} kept ({reason})");
					}
					return;
				}
				int unitCapMultiplier = gameModeExt.UnitCapMultiplier;
				if (unitCapMultiplier != configuredCapMult)
				{
					gameModeExt.SetUnitCapMultiplier(configuredCapMult);
				}
				MelonLogger.Msg(string.Format("[UNITCAPMULT] {0} ({1}): cap multiplier {2} -> {3} ({4})", teamsVersus, IsThreeWay(teamsVersus) ? "3-way" : "2-way", FmtCapMult(unitCapMultiplier), FmtCapMult(configuredCapMult), reason));
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[UNITCAPMULT] apply error: " + ex.Message);
			}
		}

		private static void OnRebalanceCommand(object player, string args)
		{
			bool flag = args != null && args.IndexOf("default", StringComparison.OrdinalIgnoreCase) >= 0;
			if (flag)
			{
				MelonLogger.Msg("[Rebalance] Reverting to vanilla defaults (admin command)");
			}
			else
			{
				MelonLogger.Msg("[Rebalance] Hot-reload triggered by admin chat command");
			}
			try
			{
				if (_omInitialized && _overrideManagerType != null)
				{
					OMRevertAll();
					_overridesApplied = false;
					MelonLogger.Msg("[Rebalance] Reverted all overrides");
				}
				_originalCreatureAttackAimDist.Clear();
				_originalSpread.Clear();
				_originalProjectileLifetimes.Clear();
				_originalProjectileSpeeds.Clear();
				_originalAimLeadVelocity.Clear();
				_originalAimLeadGravity.Clear();
				_originalAimLeadDrag.Clear();
				_originalMoveSpeeds.Clear();
				_originalMeleeDamage.Clear();
				_originalHealth.Clear();
				_originalAimAngle.Clear();
				_nameCache.Clear();
				if (flag)
				{
					if (_omInitialized)
					{
						MelonCoroutines.Start(SyncOverridesToAllPlayers(0.5f));
					}
					_unitCapMult2Way = -1;
					_unitCapMult3Way = -1;
					ApplyUnitCapMultiplier("rebalance default");
					MelonLogger.Msg("[Rebalance] Vanilla defaults restored. Use !rebalance to reload config.");
					return;
				}
				LoadConfig();
				MelonLogger.Msg($"[Rebalance] Config reloaded: {_damageMultipliers.Count} damage, {_healthMultipliers.Count} health, " + $"{_costMultipliers.Count} cost, {_buildTimeMultipliers.Count} buildTime, " + $"{_moveSpeedMultipliers.Count} moveSpeed, {_projectileOverrides.Count} projOverrides");
				if (_enabled && _configLoaded)
				{
					bool flag2 = _omInitialized;
					if (!flag2)
					{
						flag2 = InitOverrideManager();
					}
					if (flag2 && _healthMultEnabled && _healthMultipliers.Count > 0)
					{
						RegisterDamageManagerDataInOM();
					}
					CacheVanillaBaseValues();
					ApplyConstructionDataOverrides(flag2);
					if (_healthMultEnabled)
					{
						ApplyHealthOverrides(flag2);
						ReclampLiveHealth();
					}
					ApplyProjectileDamageOverrides(flag2);
					ApplyProjectileTuningOverrides(flag2);
					ApplyAimCompensationOverrides();
					ApplyTurretPitchOverrides();
					ApplyRangeOverrides(flag2);
					ApplyTargetDistanceOverrides(flag2);
					ApplyFoWDistanceOverrides(flag2);
					ApplyJumpSpeedOverrides(flag2);
					ApplyVisibleEventRadiusOverrides(flag2);
					ApplyMoveSpeedOverrides(flag2);
					ApplyStrafeSpeedOverrides(flag2);
					ApplyTurnRadiusOverrides(flag2);
					ApplyTeleportOverrides(flag2);
					ApplyDispenserTimeoutOverrides(flag2);
					ApplyProximityDetonationOverrides();
					ApplyDepositRadiusOverrides(flag2);
					ApplyExtractionRadiusOverrides(flag2);
					if (_shrimpDisableAim)
					{
						ApplyShrimpAimDisable();
					}
					ApplyAlienNestOnlyBuild(flag2);
					ApplyAlienQueensMax();
					ApplyUnitCapMultiplier("rebalance");
					try
					{
						PropagateToLiveInstances();
					}
					catch (Exception ex)
					{
						MelonLogger.Warning("[Rebalance] Live propagation error: " + ex.Message);
					}
					_overridesApplied = true;
					FlushPrefabPool("after rebalance");
					MelonLogger.Msg("[Rebalance] All overrides re-applied");
					if (flag2)
					{
						MelonCoroutines.Start(SyncOverridesToAllPlayers(0.5f));
					}
					if (_discordAutoPost && !string.IsNullOrEmpty(_discordWebhookUrl))
					{
						PostBalanceChangesToDiscord();
					}
				}
			}
			catch (Exception ex2)
			{
				MelonLogger.Error("[Rebalance] Hot-reload failed: " + ex2.Message);
			}
		}

		private static void ProcessMenuInput(object player, string input)
		{
			long playerKey = GetPlayerKey(player);
			if (!_menuStates.ContainsKey(playerKey) && _statsStates.ContainsKey(playerKey))
			{
				string text = (input ?? "").Trim();
				string[] array = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length != 0)
				{
					text = array[0];
				}
				if (TryHandleStatsNavCommand(player, playerKey, text))
				{
					return;
				}
			}
			if (!_menuStates.TryGetValue(playerKey, out BalanceMenuState value))
			{
				return;
			}
			string text2 = input;
			string text3 = "";
			string[] array2 = input.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array2.Length != 0)
			{
				text2 = array2[0];
			}
			if (array2.Length > 1)
			{
				text3 = array2[1];
			}
			if (value.PendingConfirm)
			{
				bool num = text2.Equals("yes", StringComparison.OrdinalIgnoreCase) || text2.Equals("y", StringComparison.OrdinalIgnoreCase) || text2 == "1" || text2 == "3";
				bool flag = text2.Equals("no", StringComparison.OrdinalIgnoreCase) || text2.Equals("n", StringComparison.OrdinalIgnoreCase) || text2 == "2";
				bool flag2 = text2 == "3";
				if (num)
				{
					string text4 = value.PendingUnitName ?? _unitNames[value.FactionIdx][value.CategoryIdx][value.UnitIdx];
					bool flag3;
					if (value.PendingTechTierKey != null && value.PendingIsTechCost)
					{
						flag3 = WriteTechCostToJson(value.PendingTechTierKey, (int)Math.Round(value.PendingValue));
					}
					else if (value.PendingTechTierKey != null)
					{
						flag3 = WriteTechTierToJson(value.PendingTechTierKey, value.PendingValue);
					}
					else if (value.PendingParamKey.StartsWith("prox_"))
					{
						flag3 = WriteProximityToJson(text4, value.PendingParamKey.Substring(5), value.PendingValue);
					}
					else if (value.PendingParamKey.StartsWith("unitcapmult_"))
					{
						string text5 = value.PendingParamKey.Substring(12);
						int num2 = (int)Math.Round(value.PendingValue);
						flag3 = WriteUnitCapMultToJson(text5, num2);
						if (flag3)
						{
							if (text5 == "two_way")
							{
								_unitCapMult2Way = num2;
							}
							else
							{
								_unitCapMult3Way = num2;
							}
							ApplyUnitCapMultiplier("menu edit");
						}
					}
					else if (value.PendingParamKey.StartsWith("countcap_"))
					{
						string text6 = value.PendingParamKey.Substring(9);
						int num3 = (int)Math.Round(value.PendingValue);
						flag3 = WriteCountCapToJson(text6, num3);
						if (flag3)
						{
							if (text6 == "shrimp")
							{
								_countCapShrimp = num3;
							}
							else
							{
								_countCapHarvester = num3;
							}
						}
					}
					else if (value.PendingParamKey == "queensmax")
					{
						int num4 = (int)Math.Round(value.PendingValue);
						flag3 = WriteNumberToJson("alien_queens_max", num4);
						if (flag3)
						{
							_alienQueensMax = ((num4 < 0) ? (-1) : num4);
							ApplyAlienQueensMax();
							FlushPrefabPool("alien_queens_max edit");
						}
					}
					else if (value.PendingParamKey == "nestreach")
					{
						flag3 = WriteNumberToJson("alien_nest_reach_radius", value.PendingValue);
						if (flag3)
						{
							_alienNestReachRadius = ((value.PendingValue < 1f) ? (-1f) : value.PendingValue);
						}
					}
					else if (value.PendingParamKey.StartsWith("decay_"))
					{
						string text7 = value.PendingParamKey.Substring(6);
						int num5 = text7.IndexOf('_');
						string text8 = text7.Substring(0, num5);
						string text9 = text7.Substring(num5 + 1);
						flag3 = WriteDecayToJson(text8, text9, value.PendingValue);
						if (flag3)
						{
							DecaySettings decaySettings = ((text8 == "human") ? _decayHuman : _decayAlien);
							switch (text9)
							{
							case "delay":
								decaySettings.Delay = value.PendingValue;
								break;
							case "tick":
								decaySettings.Tick = value.PendingValue;
								break;
							case "amount_pct":
								decaySettings.AmountPct = value.PendingValue;
								break;
							case "randomize_pct":
								decaySettings.RandomizePct = value.PendingValue;
								break;
							}
						}
					}
					else
					{
						flag3 = WriteParamToJson(text4, value.PendingParamKey, value.PendingValue);
					}
					bool flag4 = value.PendingTechTierKey != null;
					bool pendingIsTechCost = value.PendingIsTechCost;
					value.PendingConfirm = false;
					value.PendingUnitName = null;
					value.PendingTechTierKey = null;
					value.PendingIsTechCost = false;
					if (flag3)
					{
						string playerName = GetPlayerName(player);
						string playerSteamId = GetPlayerSteamId(player);
						string text10 = value.PendingValue.ToString(CultureInfo.InvariantCulture);
						string text11 = (flag4 ? ((pendingIsTechCost ? "tech_cost." : "tech_time.") + value.PendingParamKey) : (text4 + " " + value.PendingParamKey));
						SendChatToPlayer(player, _chatPrefix + "<color=#55FF55>Saved:</color> " + text11 + " " + _valueColor + FmtBoolCompact(value.PendingParamKey, value.PendingOldVal) + "</color> -> " + _valueColor + FmtBoolCompact(value.PendingParamKey, text10) + "</color>");
						WriteAuditLog(playerName, playerSteamId, text4, value.PendingParamKey, value.PendingOldVal, text10);
						MelonLogger.Msg("[BAL] " + playerName + " (" + playerSteamId + "): " + text11 + " " + value.PendingOldVal + " -> " + text10);
						if (flag2)
						{
							SendChatToPlayer(player, _chatPrefix + _dimColor + "Applying rebalance...</color>");
							OnRebalanceCommand(player, "rebalance");
						}
						else
						{
							SendChatToPlayer(player, _chatPrefix + _dimColor + "Use !rebalance to apply.</color>");
						}
					}
					else
					{
						SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Failed to write config.</color>");
					}
					return;
				}
				if (flag)
				{
					value.PendingConfirm = false;
					value.PendingUnitName = null;
					value.PendingTechTierKey = null;
					value.PendingIsTechCost = false;
					SendChatToPlayer(player, _chatPrefix + _dimColor + "Cancelled.</color>");
					return;
				}
				value.PendingConfirm = false;
				value.PendingUnitName = null;
				value.PendingTechTierKey = null;
				value.PendingIsTechCost = false;
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Cancelled.</color>");
			}
			if (value.JsonPendingSave)
			{
				value.JsonPendingSave = false;
				string text12 = ((text2 == "1") ? "" : input.Trim());
				if (SaveConfigAs(text12))
				{
					string text13 = DateTime.Now.ToString("yyyyMMddHHmm");
					string text14 = (string.IsNullOrEmpty(text12) ? (text13 + ".json") : (text13 + "_" + SanitizeFileName(text12) + ".json"));
					SendChatToPlayer(player, _chatPrefix + "<color=#55FF55>Saved:</color> " + text14);
					string playerName2 = GetPlayerName(player);
					string playerSteamId2 = GetPlayerSteamId(player);
					MelonLogger.Msg("[JSON] " + playerName2 + " (" + playerSteamId2 + ") saved config as " + text14);
				}
				else
				{
					SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Failed to save config.</color>");
				}
				ShowCurrentMenu(player, value);
				return;
			}
			if (!string.IsNullOrEmpty(value.JsonPendingAction))
			{
				bool num6 = text2 == "1" || text2.Equals("yes", StringComparison.OrdinalIgnoreCase);
				bool flag5 = text2 == "2" || text2.Equals("no", StringComparison.OrdinalIgnoreCase);
				string jsonPendingAction = value.JsonPendingAction;
				string jsonPendingFile = value.JsonPendingFile;
				value.JsonPendingAction = null;
				value.JsonPendingFile = null;
				if (num6)
				{
					if (jsonPendingAction == "blank")
					{
						if (ResetToBlankConfig())
						{
							SendChatToPlayer(player, _chatPrefix + "<color=#55FF55>Config reset to vanilla.</color> Use !rebalance to apply.");
							string playerName3 = GetPlayerName(player);
							string playerSteamId3 = GetPlayerSteamId(player);
							MelonLogger.Msg("[JSON] " + playerName3 + " (" + playerSteamId3 + ") reset config to blank");
						}
						else
						{
							SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Failed to reset config.</color>");
						}
					}
					else if (jsonPendingAction == "load" && jsonPendingFile != null)
					{
						if (LoadSavedConfig(jsonPendingFile))
						{
							SendChatToPlayer(player, _chatPrefix + "<color=#55FF55>Loaded:</color> " + jsonPendingFile);
							SendChatToPlayer(player, _chatPrefix + _dimColor + "Use !rebalance to apply.</color>");
							string playerName4 = GetPlayerName(player);
							string playerSteamId4 = GetPlayerSteamId(player);
							MelonLogger.Msg("[JSON] " + playerName4 + " (" + playerSteamId4 + ") loaded config: " + jsonPendingFile);
						}
						else
						{
							SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Failed to load config.</color>");
						}
					}
					ShowCurrentMenu(player, value);
					return;
				}
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Cancelled.</color>");
				if (flag5)
				{
					ShowCurrentMenu(player, value);
					return;
				}
			}
			if (text2.Equals("exit", StringComparison.OrdinalIgnoreCase) || text2 == "0")
			{
				_menuStates.Remove(playerKey);
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Exited balance editor.</color>");
				return;
			}
			if (text2.Equals("back", StringComparison.OrdinalIgnoreCase))
			{
				value.JsonPendingSave = false;
				value.JsonPendingAction = null;
				value.JsonPendingFile = null;
				if (value.Level == MenuLevel.Root)
				{
					_menuStates.Remove(playerKey);
					SendChatToPlayer(player, _chatPrefix + _dimColor + "Exited balance editor.</color>");
					return;
				}
				if (value.Level == MenuLevel.JsonMenu)
				{
					value.Level = MenuLevel.Root;
				}
				else if (value.Level == MenuLevel.JsonLoad)
				{
					value.Level = MenuLevel.JsonMenu;
				}
				else if (value.Level == MenuLevel.HTPMenu)
				{
					value.Level = MenuLevel.Root;
				}
				else if (value.Level == MenuLevel.HTPHoverbike)
				{
					value.Level = MenuLevel.HTPMenu;
				}
				else if (value.Level == MenuLevel.HTPHoverbikeParam)
				{
					value.Level = MenuLevel.HTPHoverbike;
				}
				else if (value.Level == MenuLevel.HTPTier)
				{
					value.Level = MenuLevel.HTPMenu;
				}
				else if (value.Level == MenuLevel.HTPTierTime)
				{
					value.Level = MenuLevel.HTPTier;
				}
				else if (value.Level == MenuLevel.HTPTierCost)
				{
					value.Level = MenuLevel.HTPTier;
				}
				else if (value.Level == MenuLevel.HTPTeleport)
				{
					value.Level = MenuLevel.HTPMenu;
				}
				else if (value.Level == MenuLevel.CapDecayMenu || value.Level == MenuLevel.StrategyMenu)
				{
					value.Level = MenuLevel.Root;
				}
				else if (value.Level == MenuLevel.HTPDecay)
				{
					value.Level = MenuLevel.CapDecayMenu;
				}
				else if (value.Level == MenuLevel.HTPDecayFaction)
				{
					value.Level = MenuLevel.HTPDecay;
				}
				else if (value.Level == MenuLevel.HTPUnitCap)
				{
					value.Level = MenuLevel.CapDecayMenu;
				}
				else if (value.Level == MenuLevel.CountCap)
				{
					value.Level = MenuLevel.CapDecayMenu;
				}
				else if (value.Level == MenuLevel.NestReach || value.Level == MenuLevel.QueensMax)
				{
					value.Level = MenuLevel.StrategyMenu;
				}
				else
				{
					value.Level--;
				}
				ShowCurrentMenu(player, value);
				return;
			}
			if (!int.TryParse(text2, out var result) || result < 1)
			{
				SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Use /1-/20, /back, or /0 to exit</color>");
				return;
			}
			if (value.Level == MenuLevel.ParamGroup && !string.IsNullOrEmpty(text3))
			{
				HandleParamEdit(player, value, result, text3);
				return;
			}
			if (value.Level == MenuLevel.HTPHoverbikeParam && !string.IsNullOrEmpty(text3))
			{
				HandleHTPHoverbikeEdit(player, value, result, text3);
				return;
			}
			if (value.Level == MenuLevel.HTPTierTime && !string.IsNullOrEmpty(text3))
			{
				HandleHTPTierEdit(player, value, result, text3);
				return;
			}
			if (value.Level == MenuLevel.HTPTierCost && !string.IsNullOrEmpty(text3))
			{
				HandleHTPTierCostEdit(player, value, result, text3);
				return;
			}
			if (value.Level == MenuLevel.HTPTeleport && !string.IsNullOrEmpty(text3))
			{
				HandleHTPTeleportEdit(player, value, result, text3);
				return;
			}
			if (value.Level == MenuLevel.HTPDecayFaction && !string.IsNullOrEmpty(text3))
			{
				HandleHTPDecayEdit(player, value, result, text3);
				return;
			}
			if (value.Level == MenuLevel.HTPUnitCap && !string.IsNullOrEmpty(text3))
			{
				HandleHTPUnitCapEdit(player, value, result, text3);
				return;
			}
			if (value.Level == MenuLevel.CountCap && !string.IsNullOrEmpty(text3))
			{
				HandleCountCapEdit(player, value, result, text3);
				return;
			}
			if (value.Level == MenuLevel.NestReach && !string.IsNullOrEmpty(text3))
			{
				HandleNestReachEdit(player, value, result, text3);
				return;
			}
			if (value.Level == MenuLevel.QueensMax && !string.IsNullOrEmpty(text3))
			{
				HandleQueensMaxEdit(player, value, result, text3);
				return;
			}
			switch (value.Level)
			{
			case MenuLevel.Root:
				switch (result)
				{
				case 4:
					value.Level = MenuLevel.JsonMenu;
					break;
				case 5:
					value.Level = MenuLevel.HTPMenu;
					break;
				case 6:
					value.Level = MenuLevel.CapDecayMenu;
					break;
				case 7:
					value.Level = MenuLevel.StrategyMenu;
					break;
				default:
					SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick 1-7.</color>");
					return;
				case 1:
				case 2:
				case 3:
					value.FactionIdx = result - 1;
					value.Level = MenuLevel.Faction;
					break;
				}
				break;
			case MenuLevel.Faction:
			{
				string[] array3 = _categoryNames[value.FactionIdx];
				if (result < 1 || result > array3.Length)
				{
					SendChatToPlayer(player, _chatPrefix + $"<color=#FF5555>Pick 1-{array3.Length}.</color>");
					return;
				}
				value.CategoryIdx = result - 1;
				value.Level = MenuLevel.Category;
				break;
			}
			case MenuLevel.Category:
			{
				string[] array4 = _unitNames[value.FactionIdx][value.CategoryIdx];
				if (result < 1 || result > array4.Length)
				{
					SendChatToPlayer(player, _chatPrefix + $"<color=#FF5555>Pick 1-{array4.Length}.</color>");
					return;
				}
				value.UnitIdx = result - 1;
				value.Level = MenuLevel.Unit;
				break;
			}
			case MenuLevel.Unit:
			{
				GetUnitParamGroups(_unitNames[value.FactionIdx][value.CategoryIdx][value.UnitIdx], out string[] groupNames, out string[][] _);
				if (result < 1 || result > groupNames.Length)
				{
					SendChatToPlayer(player, _chatPrefix + $"<color=#FF5555>Pick 1-{groupNames.Length}.</color>");
					return;
				}
				value.ParamGroupIdx = result - 1;
				value.Level = MenuLevel.ParamGroup;
				break;
			}
			case MenuLevel.ParamGroup:
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: /1 1.5 (or /b 1 1.5)</color>");
				return;
			case MenuLevel.JsonMenu:
				switch (result)
				{
				default:
					SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick 1-3.</color>");
					return;
				case 1:
					value.JsonPendingAction = "blank";
					SendChatToPlayer(player, _chatPrefix + _headerColor + "Reset to blank?</color> All current settings will be lost!");
					SendChatToPlayer(player, _chatPrefix + _itemColor + "/1</color>=Confirm  " + _itemColor + "/2</color>=Cancel");
					return;
				case 2:
					value.JsonPendingSave = true;
					SendChatToPlayer(player, _chatPrefix + "Enter save name " + _dimColor + "(e.g. /b myconfig)</color> or " + _itemColor + "/1</color> for timestamp only");
					return;
				case 3:
					break;
				}
				value.JsonLoadFileList = GetSavedConfigs();
				value.Level = MenuLevel.JsonLoad;
				break;
			case MenuLevel.JsonLoad:
			{
				if (value.JsonLoadFileList == null || value.JsonLoadFileList.Length == 0)
				{
					SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>No saved configs.</color>");
					return;
				}
				if (result < 1 || result > value.JsonLoadFileList.Length)
				{
					SendChatToPlayer(player, _chatPrefix + $"<color=#FF5555>Pick 1-{value.JsonLoadFileList.Length}.</color>");
					return;
				}
				string text17 = value.JsonLoadFileList[result - 1];
				value.JsonPendingAction = "load";
				value.JsonPendingFile = text17;
				SendChatToPlayer(player, _chatPrefix + _headerColor + "Load " + text17 + "?</color>");
				SendChatToPlayer(player, _chatPrefix + _itemColor + "/1</color>=Confirm  " + _itemColor + "/2</color>=Cancel");
				return;
			}
			case MenuLevel.HTPMenu:
				switch (result)
				{
				default:
					SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick 1-7.</color>");
					return;
				case 1:
					value.Level = MenuLevel.HTPHoverbike;
					break;
				case 2:
					value.Level = MenuLevel.HTPTier;
					break;
				case 3:
					value.Level = MenuLevel.HTPTeleport;
					break;
				case 6:
					value.Level = MenuLevel.DiscordMenu;
					break;
				case 4:
				{
					_shrimpDisableAim = !_shrimpDisableAim;
					WriteBoolToJson("shrimp_disable_aim", _shrimpDisableAim);
					string text19 = (_shrimpDisableAim ? "<color=#FF5555>OFF</color>" : "<color=#55FF55>ON</color>");
					SendChatToPlayer(player, _chatPrefix + "Shrimp Aim: " + text19 + " " + _dimColor + "(use !rebalance to apply)</color>");
					string playerName8 = GetPlayerName(player);
					string playerSteamId8 = GetPlayerSteamId(player);
					WriteAuditLog(playerName8, playerSteamId8, "shrimp", "disable_aim", (!_shrimpDisableAim).ToString(), _shrimpDisableAim.ToString());
					MelonLogger.Msg($"[BAL] {playerName8} ({playerSteamId8}): shrimp_disable_aim -> {_shrimpDisableAim}");
					break;
				}
				case 5:
				{
					_additionalSpawn = !_additionalSpawn;
					WriteBoolToJson("additional_spawn", _additionalSpawn);
					string text20 = (_additionalSpawn ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
					SendChatToPlayer(player, _chatPrefix + "Additional Spawn: " + text20 + " " + _dimColor + "(applies next game)</color>");
					string playerName9 = GetPlayerName(player);
					string playerSteamId9 = GetPlayerSteamId(player);
					WriteAuditLog(playerName9, playerSteamId9, "htp", "additional_spawn", (!_additionalSpawn).ToString(), _additionalSpawn.ToString());
					MelonLogger.Msg($"[BAL] {playerName9} ({playerSteamId9}): additional_spawn -> {_additionalSpawn}");
					break;
				}
				case 7:
					if (result == 7)
					{
						_watchdogEnabled = !_watchdogEnabled;
						WriteBoolToJson("watchdog_enabled", _watchdogEnabled);
						string text18 = (_watchdogEnabled ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
						SendChatToPlayer(player, _chatPrefix + "Round Watchdog: " + text18);
						string playerName7 = GetPlayerName(player);
						string playerSteamId7 = GetPlayerSteamId(player);
						WriteAuditLog(playerName7, playerSteamId7, "htp", "watchdog_enabled", (!_watchdogEnabled).ToString(), _watchdogEnabled.ToString());
						MelonLogger.Msg($"[BAL] {playerName7} ({playerSteamId7}): watchdog_enabled -> {_watchdogEnabled}");
					}
					break;
				}
				break;
			case MenuLevel.CapDecayMenu:
				if (result < 1 || result > 6)
				{
					SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick 1-6.</color>");
					return;
				}
				switch (result)
				{
				case 1:
					value.Level = MenuLevel.HTPDecay;
					break;
				case 2:
				{
					_playerInfantryIgnoreCap = !_playerInfantryIgnoreCap;
					WriteBoolToJson("player_infantry_ignore_cap", _playerInfantryIgnoreCap);
					string text16 = (_playerInfantryIgnoreCap ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
					SendChatToPlayer(player, _chatPrefix + "Player Cap Exempt: " + text16 + " " + _dimColor + "(applies immediately)</color>");
					string playerName6 = GetPlayerName(player);
					string playerSteamId6 = GetPlayerSteamId(player);
					WriteAuditLog(playerName6, playerSteamId6, "capdecay", "player_infantry_ignore_cap", (!_playerInfantryIgnoreCap).ToString(), _playerInfantryIgnoreCap.ToString());
					MelonLogger.Msg($"[BAL] {playerName6} ({playerSteamId6}): player_infantry_ignore_cap -> {_playerInfantryIgnoreCap}");
					break;
				}
				case 3:
				case 4:
					value.HTPUnitCapModeIdx = result - 3;
					value.Level = MenuLevel.HTPUnitCap;
					break;
				case 5:
				case 6:
					value.CountCapIdx = result - 5;
					value.Level = MenuLevel.CountCap;
					break;
				}
				break;
			case MenuLevel.StrategyMenu:
				switch (result)
				{
				default:
					SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick 1-5.</color>");
					return;
				case 1:
				{
					_alienNestOnlyBuild = !_alienNestOnlyBuild;
					WriteBoolToJson("alien_nest_only_build", _alienNestOnlyBuild);
					string text27 = (_alienNestOnlyBuild ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
					SendChatToPlayer(player, _chatPrefix + "Alien Nest-Only Build: " + text27 + " " + _dimColor + "(use !rebalance to apply)</color>");
					string playerName14 = GetPlayerName(player);
					string playerSteamId14 = GetPlayerSteamId(player);
					WriteAuditLog(playerName14, playerSteamId14, "strategy", "alien_nest_only_build", (!_alienNestOnlyBuild).ToString(), _alienNestOnlyBuild.ToString());
					MelonLogger.Msg($"[BAL] {playerName14} ({playerSteamId14}): alien_nest_only_build -> {_alienNestOnlyBuild}");
					break;
				}
				case 2:
					value.Level = MenuLevel.NestReach;
					break;
				case 3:
					value.Level = MenuLevel.QueensMax;
					break;
				case 4:
				{
					_alienNestFinishedAnchorOnly = !_alienNestFinishedAnchorOnly;
					WriteBoolToJson("alien_nest_finished_anchor_only", _alienNestFinishedAnchorOnly);
					string text26 = (_alienNestFinishedAnchorOnly ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
					SendChatToPlayer(player, _chatPrefix + "Alien Nest Finished-Anchor Only: " + text26 + " " + _dimColor + "(applies immediately)</color>");
					string playerName13 = GetPlayerName(player);
					string playerSteamId13 = GetPlayerSteamId(player);
					WriteAuditLog(playerName13, playerSteamId13, "strategy", "alien_nest_finished_anchor_only", (!_alienNestFinishedAnchorOnly).ToString(), _alienNestFinishedAnchorOnly.ToString());
					MelonLogger.Msg($"[BAL] {playerName13} ({playerSteamId13}): alien_nest_finished_anchor_only -> {_alienNestFinishedAnchorOnly}");
					break;
				}
				case 5:
					if (result == 5)
					{
						_alienQueenLogisticsPerNest = !_alienQueenLogisticsPerNest;
						WriteBoolToJson("alien_queen_logistics_per_nest", _alienQueenLogisticsPerNest);
						string text25 = (_alienQueenLogisticsPerNest ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
						SendChatToPlayer(player, _chatPrefix + "Queen Logistics per Nest: " + text25 + " " + _dimColor + "(applies immediately)</color>");
						if (_alienQueenLogisticsPerNest && _alienQueensMax == -1)
						{
							SendChatToPlayer(player, _chatPrefix + "<color=#FFAA00>Note:</color> " + _dimColor + "Alien Queens is vanilla (-1) — expansion Nests get no Queen and could never build. Set 3. Alien Queens to 0.</color>");
						}
						string playerName12 = GetPlayerName(player);
						string playerSteamId12 = GetPlayerSteamId(player);
						WriteAuditLog(playerName12, playerSteamId12, "strategy", "alien_queen_logistics_per_nest", (!_alienQueenLogisticsPerNest).ToString(), _alienQueenLogisticsPerNest.ToString());
						MelonLogger.Msg($"[BAL] {playerName12} ({playerSteamId12}): alien_queen_logistics_per_nest -> {_alienQueenLogisticsPerNest}");
					}
					break;
				}
				break;
			case MenuLevel.HTPUnitCap:
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .1 <mult> (e.g. .1 3 = x3, .1 -1 = server, .1 0 = unlimited)</color>");
				return;
			case MenuLevel.CountCap:
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .1 <max> (e.g. .1 12 = at most 12, .1 -1 = unlimited)</color>");
				return;
			case MenuLevel.NestReach:
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .1 <metres> (e.g. .1 250 = 250 m around a Nest, .1 -1 = vanilla)</color>");
				return;
			case MenuLevel.QueensMax:
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .1 <n> (.1 0 = every Nest gets a Queen, .1 3 = at most 3, .1 -1 = vanilla)</color>");
				return;
			case MenuLevel.HTPDecay:
				if (result < 1 || result > 2)
				{
					SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick 1-2.</color>");
					return;
				}
				value.HTPDecayFactionIdx = result - 1;
				value.Level = MenuLevel.HTPDecayFaction;
				break;
			case MenuLevel.HTPDecayFaction:
			{
				bool flag6 = value.HTPDecayFactionIdx == 0;
				string text21 = (flag6 ? "human" : "alien");
				DecaySettings decaySettings2 = (flag6 ? _decayHuman : _decayAlien);
				string text22 = (flag6 ? "Human" : "Alien");
				switch (result)
				{
				case 1:
				{
					decaySettings2.Enabled = !decaySettings2.Enabled;
					WriteDecayToJson(text21, "enabled", decaySettings2.Enabled ? 1f : 0f);
					string text24 = (decaySettings2.Enabled ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
					SendChatToPlayer(player, _chatPrefix + text22 + " Decay: " + text24 + " " + _dimColor + "(use !rebalance to apply)</color>");
					string playerName11 = GetPlayerName(player);
					string playerSteamId11 = GetPlayerSteamId(player);
					WriteAuditLog(playerName11, playerSteamId11, "decay_" + text21, "enabled", (!decaySettings2.Enabled).ToString(), decaySettings2.Enabled.ToString());
					MelonLogger.Msg($"[BAL] {playerName11} ({playerSteamId11}): decay.{text21}.enabled -> {decaySettings2.Enabled}");
					break;
				}
				case 6:
				{
					decaySettings2.KeepProduction = !decaySettings2.KeepProduction;
					WriteDecayToJson(text21, "keep_production", decaySettings2.KeepProduction ? 1f : 0f);
					string text23 = (decaySettings2.KeepProduction ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
					SendChatToPlayer(player, _chatPrefix + text22 + " Keep Production: " + text23 + " " + _dimColor + "(applies immediately via Harmony)</color>");
					string playerName10 = GetPlayerName(player);
					string playerSteamId10 = GetPlayerSteamId(player);
					WriteAuditLog(playerName10, playerSteamId10, "decay_" + text21, "keep_production", (!decaySettings2.KeepProduction).ToString(), decaySettings2.KeepProduction.ToString());
					MelonLogger.Msg($"[BAL] {playerName10} ({playerSteamId10}): decay.{text21}.keep_production -> {decaySettings2.KeepProduction}");
					break;
				}
				default:
					SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: .2 30 (or !b 2 30)</color>");
					break;
				}
				return;
			}
			case MenuLevel.HTPHoverbike:
			{
				GetUnitParamGroups("Hover Bike", out string[] groupNames2, out string[][] _);
				if (result < 1 || result > groupNames2.Length)
				{
					SendChatToPlayer(player, _chatPrefix + $"<color=#FF5555>Pick 1-{groupNames2.Length}.</color>");
					return;
				}
				value.HTPParamGroupIdx = result - 1;
				value.Level = MenuLevel.HTPHoverbikeParam;
				break;
			}
			case MenuLevel.HTPHoverbikeParam:
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: /1 1.5 (or /b 1 1.5)</color>");
				return;
			case MenuLevel.HTPTier:
				switch (result)
				{
				case 1:
					value.Level = MenuLevel.HTPTierTime;
					ShowCurrentMenu(player, value);
					break;
				case 2:
					value.Level = MenuLevel.HTPTierCost;
					ShowCurrentMenu(player, value);
					break;
				default:
					SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick 1 (Time) or 2 (Cost).</color>");
					break;
				}
				return;
			case MenuLevel.HTPTierTime:
				if (result < 1 || result > 8)
				{
					SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick 1-8.</color>");
					return;
				}
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: ." + result + " <seconds> (or !b " + result + " <seconds>)</color>");
				return;
			case MenuLevel.HTPTierCost:
				if (result < 1 || result > 8)
				{
					SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick 1-8.</color>");
					return;
				}
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: ." + result + " <resource>  ·  -1 = vanilla  (or !b " + result + " <resource>)</color>");
				return;
			case MenuLevel.HTPTeleport:
				if (result < 1 || result > 2)
				{
					SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick 1-2.</color>");
					return;
				}
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Set: ." + result + " <value> (or !b " + result + " <value>)</color>");
				return;
			case MenuLevel.DiscordMenu:
				switch (result)
				{
				default:
					SendChatToPlayer(player, _chatPrefix + "<color=#FF5555>Pick 1-2.</color>");
					return;
				case 1:
				{
					_discordAutoPost = !_discordAutoPost;
					WriteBoolToJson("discord_auto_post", _discordAutoPost);
					string text15 = (_discordAutoPost ? "<color=#55FF55>ON</color>" : "<color=#FF5555>OFF</color>");
					SendChatToPlayer(player, _chatPrefix + "Auto-post on Rebalance: " + text15);
					string playerName5 = GetPlayerName(player);
					string playerSteamId5 = GetPlayerSteamId(player);
					WriteAuditLog(playerName5, playerSteamId5, "discord", "auto_post", (!_discordAutoPost).ToString(), _discordAutoPost.ToString());
					MelonLogger.Msg($"[BAL] {playerName5} ({playerSteamId5}): discord_auto_post -> {_discordAutoPost}");
					break;
				}
				case 2:
					SendChatToPlayer(player, _chatPrefix + "Posting balance changes to Discord...");
					PostBalanceChangesToDiscord();
					break;
				}
				break;
			}
			ShowCurrentMenu(player, value);
		}

		internal static void OnStatsCommand(object player, string args)
		{
			if (player == null)
			{
				return;
			}
			try
			{
				long playerKey = GetPlayerKey(player);
				OpenPlayerStats(player, playerKey);
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[Stats] /stats handler error: " + ex.Message);
			}
		}

		internal static bool TryHandleStatsNavCommand(object player, long key, string cmd)
		{
			if (!_statsStates.TryGetValue(key, out StatsViewState value))
			{
				return false;
			}
			if (cmd == null)
			{
				return false;
			}
			cmd = cmd.Trim();
			if (cmd.Length == 0)
			{
				return false;
			}
			if (cmd.Equals("exit", StringComparison.OrdinalIgnoreCase) || cmd == "0")
			{
				_statsStates.Remove(key);
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Closed unit stats.</color>");
				return true;
			}
			if (cmd.Equals("back", StringComparison.OrdinalIgnoreCase))
			{
				if (value.Level == 1)
				{
					value.Level = 0;
					ShowStatsCategories(player, value);
				}
				else
				{
					_statsStates.Remove(key);
					SendChatToPlayer(player, _chatPrefix + _dimColor + "Closed unit stats.</color>");
				}
				return true;
			}
			if (int.TryParse(cmd, out var result) && result >= 1)
			{
				if (result - 1 < value.GroupNames.Length)
				{
					value.Level = 1;
					value.GroupIdx = result - 1;
					ShowStatsGroup(player, value);
				}
				return true;
			}
			return false;
		}

		private static void OpenPlayerStats(object player, long key)
		{
			string text = ResolveControlledUnitName(player);
			if (text == null)
			{
				_statsStates.Remove(key);
				SendChatToPlayer(player, _chatPrefix + _dimColor + "Take control of a unit first, then type /stats.</color>");
				return;
			}
			GetUnitParamGroups(text, out string[] groupNames, out string[][] groupKeys);
			if (groupNames == null || groupNames.Length == 0)
			{
				_statsStates.Remove(key);
				SendChatToPlayer(player, _chatPrefix + _dimColor + "No inspectable parameters for " + text + ".</color>");
			}
			else
			{
				StatsViewState statsViewState = new StatsViewState
				{
					UnitName = text,
					GroupNames = groupNames,
					GroupKeys = groupKeys,
					Level = 0,
					GroupIdx = 0
				};
				_statsStates[key] = statsViewState;
				ShowStatsCategories(player, statsViewState);
			}
		}

		private static string ResolveControlledUnitName(object playerObj)
		{
			try
			{
				Player player = playerObj as Player;
				if (player == null)
				{
					return null;
				}
				Unit controlledUnit = player.ControlledUnit;
				if (controlledUnit == null)
				{
					return null;
				}
				ObjectInfo objectInfo = controlledUnit.ObjectInfo;
				if (objectInfo == null)
				{
					return null;
				}
				string text = ResolveConfigName(objectInfo.DisplayName, objectInfo.name);
				return string.IsNullOrEmpty(text) ? null : text;
			}
			catch
			{
				return null;
			}
		}

		private static void ShowStatsCategories(object player, StatsViewState state)
		{
			SendChatToPlayer(player, _chatPrefix + _headerColor + state.UnitName + " — Stats</color>");
			for (int i = 0; i < state.GroupNames.Length; i++)
			{
				SendChatToPlayer(player, _chatPrefix + _itemColor + (i + 1) + ".</color> " + state.GroupNames[i]);
			}
			SendChatToPlayer(player, _chatPrefix + _dimColor + "/1../" + state.GroupNames.Length + " to view · /0 to close</color>");
		}

		private static void ShowStatsGroup(object player, StatsViewState state)
		{
			if (state.GroupIdx < 0 || state.GroupIdx >= state.GroupNames.Length)
			{
				state.Level = 0;
				ShowStatsCategories(player, state);
				return;
			}
			string unitName = state.UnitName;
			string text = state.GroupNames[state.GroupIdx];
			string[] array = state.GroupKeys[state.GroupIdx];
			SendChatToPlayer(player, _chatPrefix + _headerColor + unitName + " > " + text + "</color>");
			Dictionary<string, string> baseValues = GetBaseValues(unitName, array);
			string[] array2;
			try
			{
				List<string> list = new List<string>();
				array2 = array;
				foreach (string text2 in array2)
				{
					if (!baseValues.ContainsKey(text2))
					{
						list.Add(text2);
					}
				}
				if (list.Count > 0)
				{
					foreach (KeyValuePair<string, string> item in GetBaseValuesLive(unitName, list.ToArray()))
					{
						if (!baseValues.ContainsKey(item.Key))
						{
							baseValues[item.Key] = item.Value;
						}
					}
				}
			}
			catch
			{
			}
			JObject unitConfig = null;
			try
			{
				unitConfig = ReadUnitConfigFromJson(unitName);
			}
			catch
			{
			}
			int num = 0;
			array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string text3 = FormatStatLine(array2[i], baseValues, unitConfig);
				if (text3 != null)
				{
					SendChatToPlayer(player, _chatPrefix + text3);
					num++;
				}
			}
			if (num == 0)
			{
				SendChatToPlayer(player, _chatPrefix + _dimColor + "(no readable values)</color>");
			}
			SendChatToPlayer(player, _chatPrefix + _dimColor + "/back to categories · /0 to close</color>");
		}

		private static string FormatStatLine(string key, Dictionary<string, string> baseVals, JObject unitConfig)
		{
			bool flag = key.EndsWith("_mult", StringComparison.OrdinalIgnoreCase);
			string text = PrettyStatKey(key);
			string value = null;
			bool flag2 = baseVals?.TryGetValue(key, out value) ?? false;
			bool flag3 = unitConfig != null && unitConfig[key] != null;
			if (flag)
			{
				float num = 1f;
				if (flag3)
				{
					try
					{
						num = unitConfig[key].Value<float>();
					}
					catch
					{
						num = 1f;
					}
				}
				if (flag2 && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
				{
					if (Math.Abs(num - 1f) < 0.0001f)
					{
						return _itemColor + text + ":</color> " + _valueColor + Fmt(result) + "</color> " + _dimColor + "(unchanged)</color>";
					}
					float v = result * num;
					return _itemColor + text + ":</color> " + _valueColor + Fmt(v) + "</color> " + _dimColor + "(vanilla " + Fmt(result) + ", ×" + Fmt(num) + ")</color>";
				}
				if (Math.Abs(num - 1f) < 0.0001f)
				{
					return null;
				}
				return _itemColor + text + ":</color> " + _valueColor + "×" + Fmt(num) + "</color>";
			}
			if (flag3)
			{
				string text2 = unitConfig[key].ToString();
				if (flag2 && !string.Equals(text2, value, StringComparison.OrdinalIgnoreCase))
				{
					return _itemColor + text + ":</color> " + _valueColor + text2 + "</color> " + _dimColor + "(vanilla " + value + ")</color>";
				}
				return _itemColor + text + ":</color> " + _valueColor + text2 + "</color>";
			}
			if (flag2)
			{
				return _itemColor + text + ":</color> " + _valueColor + value + "</color> " + _dimColor + "(unchanged)</color>";
			}
			return null;
		}

		private static string PrettyStatKey(string key)
		{
			string text = key;
			if (text.StartsWith("pri_") || text.StartsWith("sec_"))
			{
				text = text.Substring(4);
			}
			else if (text.StartsWith("prox_"))
			{
				text = text.Substring(5);
			}
			if (text.EndsWith("_mult", StringComparison.OrdinalIgnoreCase))
			{
				text = text.Substring(0, text.Length - 5);
			}
			text = text.Replace("_", " ").Trim();
			if (text.Length == 0)
			{
				return key;
			}
			string[] array = text.Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Length > 0)
				{
					array[i] = char.ToUpperInvariant(array[i][0]) + ((array[i].Length > 1) ? array[i].Substring(1) : "");
				}
			}
			return string.Join(" ", array);
		}

		private static string Fmt(float v)
		{
			return v.ToString("0.###", CultureInfo.InvariantCulture);
		}
	}
}
