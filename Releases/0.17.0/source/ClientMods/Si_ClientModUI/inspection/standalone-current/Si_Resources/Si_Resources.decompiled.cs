using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using Microsoft.CodeAnalysis;
using Si_Resources;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(ResourceConfig), "Resource Configuration", "1.4.7", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_Resources")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+36907e09975799a75c5924a4c8e9274a024180af")]
[assembly: AssemblyProduct("Si_Resources")]
[assembly: AssemblyTitle("Si_Resources")]
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
namespace Si_Resources
{
	public class ResourceConfig : MelonMod
	{
		private enum EResource
		{
			Balterium,
			Biotics
		}

		[HarmonyPatch(typeof(MP_TowerDefense), "SetTeamVersusMode")]
		private static class Resources_Patch_MPTowerDefense_SetTeamVersusMode
		{
			public static void Prefix(MP_TowerDefense __instance, GameModeExt.ETeamsVersus __0)
			{
				try
				{
					foreach (TowerDefenseTeamSetup teamSetup in __instance.TeamSetups)
					{
						if (__instance.GetTeamSetupActive((BaseTeamSetup)teamSetup))
						{
							teamSetup.StartingResources = GetTeamStartingResources(__instance, teamSetup.Team);
							MelonLogger.Msg("Set starting resources for Team (" + teamSetup.Team.TeamShortName + ") to " + teamSetup.StartingResources);
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_TowerDefense::SetTeamVersusMode");
				}
			}
		}

		[HarmonyPatch(typeof(MP_Strategy), "SpawnStructuresAndStartingResources")]
		private static class Resources_Patch_MPStrategy_SpawnStructuresAndStartingResources
		{
			public static void Postfix(MP_Strategy __instance)
			{
				try
				{
					if (__instance.TeamsVersus == GameModeExt.ETeamsVersus.NONE)
					{
						return;
					}
					foreach (StrategyTeamSetup teamSetup in __instance.TeamSetups)
					{
						if (__instance.GetTeamSetupActive(teamSetup) && !(teamSetup.Team == null))
						{
							int teamStartingResources = GetTeamStartingResources(__instance, teamSetup.Team);
							if (teamSetup.Team.StartingResources != teamStartingResources)
							{
								MelonLogger.Msg("Re-set starting resources for Team (" + teamSetup.Team.TeamShortName + ") after base spawn: " + teamSetup.Team.StartingResources + " -> " + teamStartingResources);
								teamSetup.Team.StartingResources = teamStartingResources;
							}
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_Strategy::SpawnStructuresAndStartingResources");
				}
			}
		}

		[HarmonyPatch(typeof(MP_Strategy), "SetTeamVersusMode")]
		private static class Resources_Patch_MPStrategy_SetTeamVersusMode
		{
			public static void Postfix(MP_Strategy __instance, GameModeExt.ETeamsVersus __0)
			{
				try
				{
					foreach (StrategyTeamSetup teamSetup in __instance.TeamSetups)
					{
						if (!__instance.GetTeamSetupActive(teamSetup))
						{
							continue;
						}
						teamSetup.Team.StartingResources = GetTeamStartingResources(__instance, teamSetup.Team);
						MelonLogger.Msg("Set starting resources for Team (" + teamSetup.Team.TeamShortName + ") to " + teamSetup.Team.StartingResources);
						if (!MakeClosestResearchAreaVisible(teamSetup.Team))
						{
							continue;
						}
						Resource teamResourceType = GetTeamResourceType(teamSetup.Team);
						if (teamResourceType == null)
						{
							MelonLogger.Warning("Could not find default resource type for team: " + teamSetup.Team.TeamShortName);
							continue;
						}
						if (ResourceArea.GetNumKnownResourcesAreas(teamSetup.Team, teamResourceType) > 0)
						{
							MelonLogger.Msg("Already a visible resource type for team: " + teamSetup.Team.TeamShortName);
							continue;
						}
						ResourceArea teamClosestResourceArea = GetTeamClosestResourceArea(teamSetup.Team, teamResourceType);
						if (teamClosestResourceArea == null)
						{
							MelonLogger.Warning("Could not find closest resource area for team: " + teamSetup.Team.TeamShortName);
							continue;
						}
						Vector3 position = teamClosestResourceArea.transform.position;
						position[1] += 7f;
						HelperMethods.SpawnAtLocation((teamSetup.Team.Index == 0) ? "Crab" : "Soldier_Scout", position, Quaternion.identity, teamSetup.Team.Index);
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_Strategy::SetTeamVersusMode");
				}
			}
		}

		private const int defaultStrategyStartingResources = 8000;

		private const int defaultSiegeAttackingStartingResources = 45000;

		private const int defaultSiegeDefendingStartingResources = 20000;

		private static MelonPreferences_Category _modCategory;

		private static MelonPreferences_Entry<bool> Pref_Resources_Enable_Structure_Refunds;

		private static MelonPreferences_Entry<float> Pref_Resources_Refund_TopRate;

		private static MelonPreferences_Entry<float> Pref_Resources_Refund_MidRatePenalty;

		private static MelonPreferences_Entry<float> Pref_Resources_Refund_JunkRatePenalty;

		private static MelonPreferences_Entry<float> Pref_Resources_Refund_DisfunctionalPenalty;

		private static MelonPreferences_Entry<int> Pref_Resources_Refund_MinimumAmount;

		private static MelonPreferences_Entry<int> Pref_Resources_Centauri_StartingAmount;

		private static MelonPreferences_Entry<int> Pref_Resources_Sol_StartingAmount;

		private static MelonPreferences_Entry<int> Pref_Resources_Aliens_StartingAmount;

		private static MelonPreferences_Entry<bool> Pref_Resources_Aliens_RevealClosestArea;

		private static MelonPreferences_Entry<bool> Pref_Resources_Humans_RevealClosestArea;

		private static MelonPreferences_Entry<int> Pref_Resources_Attackers_StartingAmount;

		private static MelonPreferences_Entry<int> Pref_Resources_Defenders_StartingAmount;

		public override void OnInitializeMelon()
		{
			if (_modCategory == null)
			{
				_modCategory = MelonPreferences.CreateCategory("Silica");
			}
			if (Pref_Resources_Enable_Structure_Refunds == null)
			{
				Pref_Resources_Enable_Structure_Refunds = _modCategory.CreateEntry<bool>("Resources_AllowStructureRefunds", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_Resources_Refund_TopRate == null)
			{
				Pref_Resources_Refund_TopRate = _modCategory.CreateEntry<float>("Resources_Refund_TopRate", 0.875f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_Resources_Refund_MidRatePenalty == null)
			{
				Pref_Resources_Refund_MidRatePenalty = _modCategory.CreateEntry<float>("Resources_Refund_Midline_PenaltyPct", 3f / 32f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_Resources_Refund_JunkRatePenalty == null)
			{
				Pref_Resources_Refund_JunkRatePenalty = _modCategory.CreateEntry<float>("Resources_Refund_Junk_PenaltyPct", 0.1875f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_Resources_Refund_DisfunctionalPenalty == null)
			{
				Pref_Resources_Refund_DisfunctionalPenalty = _modCategory.CreateEntry<float>("Resources_Refund_Disfunctional_PenaltyPct", 0.5f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_Resources_Refund_MinimumAmount == null)
			{
				Pref_Resources_Refund_MinimumAmount = _modCategory.CreateEntry<int>("Resources_Refund_MinimumRefundAmount", 100, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_Resources_Centauri_StartingAmount == null)
			{
				Pref_Resources_Centauri_StartingAmount = _modCategory.CreateEntry<int>("Resources_Centauri_StartingAmount", 8000, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_Resources_Sol_StartingAmount == null)
			{
				Pref_Resources_Sol_StartingAmount = _modCategory.CreateEntry<int>("Resources_Sol_StartingAmount", 8000, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_Resources_Aliens_StartingAmount == null)
			{
				Pref_Resources_Aliens_StartingAmount = _modCategory.CreateEntry<int>("Resources_Aliens_StartingAmount", 8000, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_Resources_Aliens_RevealClosestArea == null)
			{
				Pref_Resources_Aliens_RevealClosestArea = _modCategory.CreateEntry<bool>("Resources_Aliens_RevealClosestAreaOnStart", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_Resources_Humans_RevealClosestArea == null)
			{
				Pref_Resources_Humans_RevealClosestArea = _modCategory.CreateEntry<bool>("Resources_Humans_RevealClosestAreaOnStart", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_Resources_Attackers_StartingAmount == null)
			{
				Pref_Resources_Attackers_StartingAmount = _modCategory.CreateEntry<int>("Resources_Attackers_StartingAmount", 45000, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_Resources_Defenders_StartingAmount == null)
			{
				Pref_Resources_Defenders_StartingAmount = _modCategory.CreateEntry<int>("Resources_Defenders_StartingAmount", 20000, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
		}

		[MethodImpl(MethodImplOptions.NoOptimization)]
		public override void OnLateInitializeMelon()
		{
			HelperMethods.CommandCallback adminCallback = Command_Resources;
			HelperMethods.RegisterAdminCommand("resources", adminCallback, Power.Cheat, "Provides resources to a team. Usage: !resources <amount> [optional:<teamname>]");
		}

		public static void Command_Resources(Player? callerPlayer, string args)
		{
			string text = args.Split(' ')[0];
			int num = args.Split(' ').Length - 1;
			if (num > 2)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too many arguments");
				return;
			}
			if (num < 1)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too few arguments");
				return;
			}
			Team team = null;
			if (num == 1)
			{
				if (callerPlayer != null)
				{
					team = callerPlayer.Team;
				}
			}
			else
			{
				team = Team.GetTeamByName(args.Split(' ')[2]);
			}
			if (team == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Ambiguous or invalid team name");
				return;
			}
			string text2 = args.Split(' ')[1];
			if (!int.TryParse(text2, out var result))
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Invalid amount specified");
			}
			else if (result == 0)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": No amount specified");
			}
			else if (result > team.RemainingResourceCapacity && team.RemainingResourceCapacity > 0)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Amount specified is higher than team resource capacity");
			}
			else if (result + team.StoredResources < 0)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Amount specified would give team negative resources");
			}
			else if (result >= 0)
			{
				int num2 = team.StoreResource(result);
				if (num2 > 0)
				{
					team.StartingResources += num2;
				}
				HelperMethods.AlertAdminAction(callerPlayer, "granted " + text2 + " resources to " + HelperMethods.GetTeamColor(team) + team.TeamShortName + "</color>");
			}
			else
			{
				team.RetrieveResource(-result);
				HelperMethods.AlertAdminAction(callerPlayer, "took " + text2 + " resources from " + HelperMethods.GetTeamColor(team) + team.TeamShortName + "</color>");
			}
		}

		private static bool MakeClosestResearchAreaVisible(Team team)
		{
			if (team.Index == 0)
			{
				return Pref_Resources_Aliens_RevealClosestArea.Value;
			}
			if (team.Index == 4 || team.Index == 3)
			{
				return Pref_Resources_Humans_RevealClosestArea.Value;
			}
			return false;
		}

		private static ResourceArea? GetTeamClosestResourceArea(Team team, Resource type)
		{
			Vector3 teamStartingPosition = GetTeamStartingPosition(team);
			GameListCache.QueryResourceAreas.Clear();
			foreach (ResourceArea resourceArea in ResourceArea.ResourceAreas)
			{
				if (!(resourceArea.ResourceType != type) && resourceArea.ResourceAmountCurrent > 0)
				{
					GameListCache.QueryResourceAreas.Add(resourceArea);
				}
			}
			ResourceArea result = null;
			float num = float.MaxValue;
			foreach (ResourceArea queryResourceArea in GameListCache.QueryResourceAreas)
			{
				float sqrMagnitude = (queryResourceArea.SignalCenter - teamStartingPosition).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = queryResourceArea;
					num = sqrMagnitude;
				}
			}
			return result;
		}

		private static Vector3 GetTeamStartingPosition(Team team)
		{
			if (team.Structures.Count <= 0)
			{
				MelonLogger.Warning("Could not determine starting position for team: " + team.TeamShortName);
				return Vector3.zero;
			}
			return team.Structures[0].transform.position;
		}

		private static Resource? GetTeamResourceType(Team team)
		{
			switch (team.Index)
			{
			case 0:
				return Resource.Resources.Find((Resource r) => r.ResourceName.StartsWith("Bi"));
			case 3:
			case 4:
				return Resource.Resources.Find((Resource r) => r.ResourceName.StartsWith("Ba"));
			default:
				MelonLogger.Warning("Could not determine default resource type for team: " + team.TeamShortName);
				return null;
			}
		}

		private static int GetTeamStartingResources<T>(T gameModeInstance, Team team) where T : GameModeExt
		{
			if (gameModeInstance is MP_Strategy)
			{
				switch (team.Index)
				{
				case 0:
					return Pref_Resources_Aliens_StartingAmount.Value;
				case 3:
					return Pref_Resources_Centauri_StartingAmount.Value;
				case 4:
					return Pref_Resources_Sol_StartingAmount.Value;
				default:
					MelonLogger.Warning("Could not determine starting resources for strategy team: " + team.TeamShortName);
					return 8000;
				}
			}
			if (gameModeInstance is MP_TowerDefense)
			{
				switch (team.Index)
				{
				case 3:
					return Pref_Resources_Attackers_StartingAmount.Value;
				case 4:
					return Pref_Resources_Defenders_StartingAmount.Value;
				default:
					MelonLogger.Warning("Could not determine starting resources for siege team: " + team.TeamShortName);
					return 45000;
				}
			}
			MelonLogger.Error("Could not determine gamemode. Returning strategy default resources for team: " + team.TeamShortName);
			return 8000;
		}
	}
}
