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
using Si_SurrenderCommand;
using SilicaAdminMod;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(SurrenderCommand), "Surrender Command", "1.6.4", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_SurrenderCommand")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_SurrenderCommand")]
[assembly: AssemblyTitle("Si_SurrenderCommand")]
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
namespace Si_SurrenderCommand
{
	public class SurrenderCommand : MelonMod
	{
		[HarmonyPatch(typeof(MusicJukeboxHandler), "OnGameStarted")]
		private static class ApplyPatch_MusicJukeboxHandler_OnGameStarted
		{
			public static void Postfix(MusicJukeboxHandler __instance, GameMode __0)
			{
				try
				{
					ClearSurrenderVotes();
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MusicJukeboxHandler::OnGameStarted");
				}
			}
		}

		private static List<Player>[] votesToSurrender;

		private static MelonPreferences_Category _modCategory;

		private static MelonPreferences_Entry<bool> Pref_Surrender_CommanderImmediate;

		private static MelonPreferences_Entry<bool> Pref_Surrender_ShowNames;

		private static MelonPreferences_Entry<float> Pref_Surrender_Vote_Percent;

		public override void OnInitializeMelon()
		{
			if (_modCategory == null)
			{
				_modCategory = MelonPreferences.CreateCategory("Silica");
			}
			if (Pref_Surrender_CommanderImmediate == null)
			{
				Pref_Surrender_CommanderImmediate = _modCategory.CreateEntry<bool>("Surrender_CommanderImmediate", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_Surrender_ShowNames == null)
			{
				Pref_Surrender_ShowNames = _modCategory.CreateEntry<bool>("Surrender_ShowPlayerNames", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_Surrender_Vote_Percent == null)
			{
				Pref_Surrender_Vote_Percent = _modCategory.CreateEntry<float>("Surrender_Vote_PercentNeeded", 0.35f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			votesToSurrender = new List<Player>[6];
			for (int i = 0; i < 5; i++)
			{
				votesToSurrender[i] = new List<Player>();
			}
		}

		public static bool IsCommander(Player player)
		{
			if (player == null)
			{
				return false;
			}
			if (player.Team == null)
			{
				return false;
			}
			return GameMode.CurrentGameMode.GetPlayerIsCommander(player);
		}

		public override void OnLateInitializeMelon()
		{
			HelperMethods.CommandCallback commandCallback = Command_Surrender;
			HelperMethods.RegisterPlayerCommand("surrender", commandCallback, hideFromChat: true);
			GameEvents.OnPlayerLeft = (Action<Player>)Delegate.Combine(GameEvents.OnPlayerLeft, new Action<Player>(OnPlayerLeft_SurrenderCheck));
		}

		private void OnPlayerLeft_SurrenderCheck(Player player)
		{
			if (!(player == null) && !(player.Team == null) && votesToSurrender[player.Team.Index].Remove(player))
			{
				MelonLogger.Msg("Removed surrender vote from disconnected player.");
			}
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			ClearSurrenderVotes();
		}

		public static void Command_Surrender(Player? callerPlayer, string args)
		{
			if (callerPlayer == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Console not supported.");
				return;
			}
			Team team = callerPlayer.Team;
			if (team == null || team.Index == 1)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " This team does not support surrender.");
				return;
			}
			if (!GameMode.CurrentGameMode.GameOngoing)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Can't surrender. Game not started.");
				return;
			}
			if (IsCommander(callerPlayer) && Pref_Surrender_CommanderImmediate.Value)
			{
				Surrender(team, callerPlayer);
				return;
			}
			if (votesToSurrender[team.Index].Contains(callerPlayer))
			{
				int num = MoreSurrenderVotesNeeded(team);
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Already voted for surrender. ", num.ToString(), " more player" + ((num == 1) ? " " : "s ") + (Pref_Surrender_CommanderImmediate.Value ? "or 1 commander" : "") + " needed.");
				return;
			}
			votesToSurrender[team.Index].Add(callerPlayer);
			if (votesToSurrender[team.Index].Count < TeammatesNeededForSurrender(team))
			{
				int num2 = MoreSurrenderVotesNeeded(team);
				HelperMethods.SendChatMessageToTeam(team, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", HelperMethods.GetTeamColor(team), " ", Pref_Surrender_ShowNames.Value ? callerPlayer.PlayerName : "A teammate", "</color> votes to surrender. ", num2.ToString(), " more player" + ((num2 == 1) ? " " : "s ") + (Pref_Surrender_CommanderImmediate.Value ? "or 1 commander" : "") + " needed.");
				if (Pref_Surrender_ShowNames.Value)
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Your surrender vote was recorded.");
				}
			}
			else
			{
				Surrender(team, callerPlayer);
			}
		}

		public static int TeammatesNeededForSurrender(Team team)
		{
			int num = team.GetNumPlayers();
			if ((bool)GameMode.CurrentGameMode && Pref_Surrender_CommanderImmediate.Value)
			{
				if (GameMode.CurrentGameMode is MP_Strategy mP_Strategy)
				{
					num -= ((mP_Strategy.GetCommanderForTeam(team) != null) ? 1 : 0);
				}
				else if (GameMode.CurrentGameMode is MP_TowerDefense mP_TowerDefense)
				{
					num -= ((mP_TowerDefense.GetCommanderForTeam(team) != null) ? 1 : 0);
				}
			}
			int num2 = (int)Math.Ceiling((float)num * Pref_Surrender_Vote_Percent.Value);
			if (num2 < 1)
			{
				return 1;
			}
			return num2;
		}

		public static int MoreSurrenderVotesNeeded(Team team)
		{
			int num = TeammatesNeededForSurrender(team) - votesToSurrender[team.Index].Count;
			if (num < 1)
			{
				return 1;
			}
			return num;
		}

		public static void ClearSurrenderVotes()
		{
			for (int i = 0; i < 5; i++)
			{
				votesToSurrender[i].Clear();
			}
		}

		public static void Surrender(Team team, Player player)
		{
			if (Pref_Surrender_ShowNames.Value)
			{
				HelperMethods.ReplyToCommand_Player(player, "used !surrender to end");
			}
			else
			{
				HelperMethods.SendChatMessageToAll("<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", HelperMethods.GetTeamColor(team), team.TeamShortName, "</color> surrendered.");
			}
			if (GameMode.CurrentGameMode is MP_Strategy)
			{
				RemoveConstructionSites(team, criticalOnly: true);
				RemoveStructures(team, criticalOnly: true);
				RemoveUnits(team, criticalOnly: true);
			}
			else if (GameMode.CurrentGameMode is MP_TowerDefense mP_TowerDefense)
			{
				mP_TowerDefense.EndRound();
			}
			ClearSurrenderVotes();
		}

		public static void RemoveConstructionSites(Team team, bool criticalOnly)
		{
			List<ConstructionSite> list = new List<ConstructionSite>();
			foreach (ConstructionSite constructionSite in ConstructionSite.ConstructionSites)
			{
				if (!(constructionSite == null) && !(constructionSite.Team == null) && !(constructionSite.Team != team) && (!criticalOnly || !constructionSite.ObjectInfo || constructionSite.ObjectInfo.Critical))
				{
					MelonLogger.Msg("Found critical construction site: " + constructionSite.ToString());
					list.Add(constructionSite);
				}
			}
			foreach (ConstructionSite item in list)
			{
				item.Deinit(wasCompleted: false);
			}
		}

		public static void RemoveStructures(Team team, bool criticalOnly)
		{
			for (int i = 0; i < team.Structures.Count; i++)
			{
				if (team.Structures[i] == null)
				{
					MelonLogger.Warning("Found null structure during surrender command.");
				}
				else if (!team.Structures[i].IsDestroyed && (!criticalOnly || !team.Structures[i].ObjectInfo || team.Structures[i].ObjectInfo.Critical))
				{
					MelonLogger.Msg("Found critical structure: " + team.Structures[i].ToString());
					team.Structures[i].DamageManager.SetHealth01(0f);
				}
			}
		}

		public static void RemoveUnits(Team team, bool criticalOnly)
		{
			for (int i = 0; i < team.Units.Count; i++)
			{
				if (team.Units[i] == null)
				{
					MelonLogger.Warning("Found null unit during surrender command.");
				}
				else if (!team.Units[i].IsDestroyed && (!criticalOnly || !team.Units[i].ObjectInfo || team.Units[i].ObjectInfo.Critical))
				{
					MelonLogger.Msg("Found critical unit: " + team.Units[i].ToString());
					team.Units[i].DamageManager.SetHealth01(0f);
				}
			}
		}
	}
}
