using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using Microsoft.CodeAnalysis;
using Si_HQlessHumansLose;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(HQlessHumansLose), "HQless Humans Lose", "1.3.11", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_HQlessHumansLose")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+36907e09975799a75c5924a4c8e9274a024180af")]
[assembly: AssemblyProduct("Si_HQlessHumansLose")]
[assembly: AssemblyTitle("Si_HQlessHumansLose")]
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
namespace Si_HQlessHumansLose
{
	public class HQlessHumansLose : MelonMod
	{
		[HarmonyPatch(typeof(ConstructionSite), "Deinit")]
		private static class ApplyPatch_ConstructionSiteDeinit
		{
			private static void Postfix(ConstructionSite __instance, bool __0)
			{
				try
				{
					if (!__0 && __instance.gameObject.GetBaseGameObject().ObjectInfo.ObjectType == ObjectInfoType.Structure)
					{
						MelonLogger.Msg("Structure construction destroyed: " + __instance.name);
						Team team = __instance.Team;
						string rootCriticalPrefix = GetRootCriticalPrefix(team);
						if (__instance.ToString().Contains(rootCriticalPrefix) && !HasRootCriticalsRemaining(team, onDestroyedEventTrigger: false) && !HasRootStructureUnderConstruction(team))
						{
							EliminateTeam(team);
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run ConstructionSite::Deinit");
				}
			}
		}

		[HarmonyPatch(typeof(ConstructionSite), "Update")]
		public static class ApplyPatch_ConstructionSite_Update
		{
			public static void Postfix(ConstructionSite __instance)
			{
				try
				{
					if (!HelperMethods.IsTimerActive(Timer_SendTeamLostMessage))
					{
						return;
					}
					Timer_SendTeamLostMessage += Time.deltaTime;
					if (!(Timer_SendTeamLostMessage >= 1f))
					{
						return;
					}
					Timer_SendTeamLostMessage = HelperMethods.Timer_Inactive;
					if (losingTeam != null)
					{
						if (destroyerOfWorlds == null)
						{
							TeamLostMessage(losingTeam);
						}
						else
						{
							TeamLostByPlayerMessage(losingTeam, destroyerOfWorlds);
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run ConstructionSite::Update");
				}
			}
		}

		[HarmonyPatch(typeof(MP_Strategy), "OnStructureDestroyed")]
		private static class ApplyPatch_OnStructureDestroyed
		{
			private static void Postfix(MP_Strategy __instance, Structure __0, GameObject __1)
			{
				try
				{
					if (GameMode.CurrentGameMode == null || !GameMode.CurrentGameMode.GameOngoing || __0 == null)
					{
						return;
					}
					Team team = __0.Team;
					if (team == null)
					{
						return;
					}
					string rootCriticalPrefix = GetRootCriticalPrefix(team);
					if (!__0.ToString().Contains(rootCriticalPrefix) || HasRootCriticalsRemaining(team) || HasRootStructureUnderConstruction(team))
					{
						return;
					}
					destroyerOfWorlds = null;
					if (__1 != null)
					{
						BaseGameObject baseGameObject = __1.GetBaseGameObject();
						if (baseGameObject != null)
						{
							NetworkComponent networkComponent = baseGameObject.NetworkComponent;
							if (networkComponent != null && networkComponent.OwnerPlayer != null)
							{
								destroyerOfWorlds = networkComponent.OwnerPlayer;
							}
						}
					}
					EliminateTeam(team);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_Strategy::OnStructureDestroyed");
				}
			}
		}

		[HarmonyPatch(typeof(StrategyMode), "OnUnitDestroyed")]
		private static class ApplyPatch_StrategyMode_OnUnitDestroyed
		{
			public static void Postfix(StrategyMode __instance, Unit __0, GameObject __1)
			{
				try
				{
					if (GameMode.CurrentGameMode == null || !GameMode.CurrentGameMode.GameOngoing || __0 == null)
					{
						return;
					}
					Team team = __0.Team;
					if (team == null)
					{
						return;
					}
					string rootCriticalPrefix = GetRootCriticalPrefix(team);
					if (!__0.ToString().Contains(rootCriticalPrefix) || HasRootCriticalsRemaining(team))
					{
						return;
					}
					destroyerOfWorlds = null;
					if (__1 != null)
					{
						BaseGameObject baseGameObject = __1.GetBaseGameObject();
						if (baseGameObject != null)
						{
							NetworkComponent networkComponent = baseGameObject.NetworkComponent;
							if (networkComponent != null && networkComponent.OwnerPlayer != null)
							{
								destroyerOfWorlds = networkComponent.OwnerPlayer;
							}
						}
					}
					EliminateTeam(team);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run StrategyMode::OnUnitDestroyed");
				}
			}
		}

		private static MelonPreferences_Category _modCategory = null;

		public static MelonPreferences_Entry<bool> Pref_HQ_RemoveStructuresOnElimination = null;

		private static Team? losingTeam;

		private static Player? destroyerOfWorlds;

		private static float Timer_SendTeamLostMessage = HelperMethods.Timer_Inactive;

		public override void OnInitializeMelon()
		{
			if (_modCategory == null)
			{
				_modCategory = MelonPreferences.CreateCategory("Silica");
			}
			if (Pref_HQ_RemoveStructuresOnElimination == null)
			{
				Pref_HQ_RemoveStructuresOnElimination = _modCategory.CreateEntry<bool>("HQ_RemoveStructuresOnElimination", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
		}

		public static void TeamLostMessage(Team team)
		{
			string rootCriticalFullName = GetRootCriticalFullName(team);
			HelperMethods.ReplyToCommand(HelperMethods.GetTeamColor(team) + team.TeamShortName + "</color> lost their " + rootCriticalFullName);
		}

		public static void TeamLostByPlayerMessage(Team team, Player player)
		{
			string rootCriticalFullName = GetRootCriticalFullName(team);
			HelperMethods.ReplyToCommand(HelperMethods.GetTeamColor(player) + player.PlayerName + "</color> destroyed " + HelperMethods.GetTeamColor(team) + team.TeamShortName + "</color>'s " + rootCriticalFullName);
		}

		private static string GetRootCriticalPrefix(Team team)
		{
			if (!team.TeamName.Contains("Human"))
			{
				return "Que";
			}
			return "Headq";
		}

		private static string GetRootCriticalFullName(Team team)
		{
			if (!team.TeamName.Contains("Human"))
			{
				return "Queen";
			}
			return "Headquarters";
		}

		public static bool OneFactionAlreadyEliminated()
		{
			int num = 0;
			for (int i = 0; i < 5; i++)
			{
				Team team = Team.Teams[i];
				if (!(team == null) && team.GetHasAnyCritical())
				{
					num++;
				}
			}
			if (num < 2)
			{
				MelonLogger.Msg("OneFactionAlreadyEliminated: true");
				return true;
			}
			MelonLogger.Msg("OneFactionAlreadyEliminated: false");
			return false;
		}

		public static void EliminateTeam(Team team)
		{
			GameModeExt.ETeamsVersus teamsVersus = UnityEngine.Object.FindFirstObjectByType<MP_Strategy>().TeamsVersus;
			MelonLogger.Msg("Eliminating team " + team.TeamShortName + " on versus mode " + teamsVersus);
			if (teamsVersus == GameModeExt.ETeamsVersus.HUMANS_VS_HUMANS_VS_ALIENS && !OneFactionAlreadyEliminated())
			{
				if (Pref_HQ_RemoveStructuresOnElimination.Value)
				{
					for (int i = 0; i < team.Structures.Count; i++)
					{
						if (!team.Structures[i].ToString().StartsWith("Bunk"))
						{
							team.Structures[i].DamageManager.SetHealth01(0f);
						}
					}
				}
				DestroyAllUnits(team);
			}
			else
			{
				HelperMethods.DestroyAllStructures(team);
			}
			TeamLostMessage(team);
		}

		private static void DestroyAllUnits(Team team)
		{
			for (int i = 0; i < team.Units.Count; i++)
			{
				if (!team.Units[i].IsDestroyed)
				{
					team.Units[i].DamageManager.SetHealth(0f);
				}
			}
		}

		private static void DelayTeamLostMessage(Team team)
		{
			MelonLogger.Msg("Starting delay lost timer for team " + team.TeamShortName);
			HelperMethods.StartTimer(ref Timer_SendTeamLostMessage);
			losingTeam = team;
		}

		public static bool HasRootCriticalsRemaining(Team team, bool onDestroyedEventTrigger = true)
		{
			return team.GetHasAnyCritical();
		}

		public static bool HasRootStructureUnderConstruction(Team team)
		{
			string rootCriticalPrefix = GetRootCriticalPrefix(team);
			int num = 0;
			for (int i = 0; i < ConstructionSite.ConstructionSites.Count; i++)
			{
				if (ConstructionSite.ConstructionSites[i].Team.Index == team.Index && ConstructionSite.ConstructionSites[i].gameObject.GetBaseGameObject().ObjectInfo.ObjectType == ObjectInfoType.Structure && ConstructionSite.ConstructionSites[i].ToString().Contains(rootCriticalPrefix))
				{
					num++;
				}
			}
			if (num <= 0)
			{
				return false;
			}
			return true;
		}
	}
}
