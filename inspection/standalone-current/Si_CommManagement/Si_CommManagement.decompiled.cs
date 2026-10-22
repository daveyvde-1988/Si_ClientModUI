using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using MelonLoader.Utils;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Si_CommanderManagement;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(CommanderManager), "Commander Management", "1.9.15", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_CommManagement")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+29c72fafdade58e425361c71fdf0f9e62e86e637")]
[assembly: AssemblyProduct("Si_CommManagement")]
[assembly: AssemblyTitle("Si_CommManagement")]
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
namespace Si_CommanderManagement
{
	public class CommanderApplications
	{
		[HarmonyPatch(typeof(MP_Strategy), "OnMissionStateChanged")]
		private static class ApplyPatch_Strategy_OnMissionStateChanged
		{
			public static void Postfix(MP_Strategy __instance, GameModeExt.EMissionState __0)
			{
				try
				{
					if (__0 == GameModeExt.EMissionState.STARTED)
					{
						HoldCommanderLottery();
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_Strategy::OnMissionStateChanged");
				}
			}
		}

		[HarmonyPatch(typeof(MP_TowerDefense), "OnMissionStateChanged")]
		private static class ApplyPatch_TowerDefense_OnMissionStateChanged
		{
			public static void Postfix(MP_TowerDefense __instance, GameModeExt.EMissionState __0)
			{
				try
				{
					if (__0 == GameModeExt.EMissionState.STARTED)
					{
						HoldCommanderLottery();
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_TowerDefense::OnMissionStateChanged");
				}
			}
		}

		[HarmonyPatch(typeof(MP_TowerDefense), "RPC_TimerUpdate")]
		private static class CommanderManager_Patch_TowerDefense_TimerUpdate
		{
			private static void Postfix(MP_TowerDefense __instance)
			{
				Process_RPC_TimerUpdate(__instance);
			}
		}

		[HarmonyPatch(typeof(MP_Strategy), "RPC_TimerUpdate")]
		private static class CommanderManager_Patch_Strategy_TimerUpdate
		{
			private static void Postfix(MP_Strategy __instance)
			{
				Process_RPC_TimerUpdate(__instance);
			}
		}

		public static List<Player>[] commanderApplicants;

		public static List<PreviousCommander>? previousCommanders;

		public static Player?[]? teamswapCommanderChecks;

		public static Player[]? promotedCommanders;

		private static float GameModeTimer
		{
			get
			{
				if (GameMode.CurrentGameMode is GameModeExt gameModeExt)
				{
					return gameModeExt.Timer;
				}
				throw new ArgumentException("Cannot access gamemode timer");
			}
			set
			{
				if (GameMode.CurrentGameMode is GameModeExt gameModeExt)
				{
					gameModeExt.SetTimer(value);
					return;
				}
				throw new ArgumentException("Cannot set gamemode timer");
			}
		}

		public static void InitializeApplications()
		{
			commanderApplicants = new List<Player>[9];
			for (int i = 0; i < 8; i++)
			{
				commanderApplicants[i] = new List<Player>();
			}
			previousCommanders = new List<PreviousCommander>();
			teamswapCommanderChecks = new Player[9];
			promotedCommanders = new Player[9];
		}

		public static bool IsApplicant(Player player)
		{
			if (player.Team == null)
			{
				return false;
			}
			return commanderApplicants[player.Team.Index].Any((Player k) => k == player);
		}

		public static void Command_Commander(Player? callerPlayer, string args)
		{
			if (callerPlayer == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Console not supported.");
			}
			else if (commanderApplicants == null)
			{
				MelonLogger.Warning("Commander applicant pool missing.");
			}
			else if (callerPlayer.Team == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " On invalid team.");
			}
			else if (!GameMode.CurrentGameMode.Started || GameMode.CurrentGameMode.GameBegun)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Cannot apply for commander during the game.");
			}
			else if (!commanderApplicants[callerPlayer.Team.Index].Any((Player k) => k == callerPlayer))
			{
				commanderApplicants[callerPlayer.Team.Index].Add(callerPlayer);
				HelperMethods.ReplyToCommand_Player(callerPlayer, "applied for commander");
			}
			else
			{
				commanderApplicants[callerPlayer.Team.Index].Remove(callerPlayer);
				GameMode.CurrentGameMode.TrySpawnPlayerAtSpawnPoint(callerPlayer, callerPlayer.Team);
				HelperMethods.ReplyToCommand_Player(callerPlayer, "removed themselves from commander lottery");
			}
		}

		public static bool AllTeamsHaveCommanderApplicants()
		{
			if (commanderApplicants == null)
			{
				return true;
			}
			for (int i = 0; i < 8; i++)
			{
				if (!(Team.Teams[i] == null) && Team.Teams[i].GetNumPlayers() >= 1 && commanderApplicants[i].Count == 0)
				{
					return false;
				}
			}
			return true;
		}

		public static void HoldCommanderLottery()
		{
			Mutineer.ClearMutineerList();
			if (commanderApplicants == null || previousCommanders == null || promotedCommanders == null)
			{
				return;
			}
			System.Random random = new System.Random();
			Player player = null;
			for (int i = 0; i < 8; i++)
			{
				if (teamswapCommanderChecks != null)
				{
					teamswapCommanderChecks[i] = null;
				}
				if (Team.Teams[i] == null)
				{
					commanderApplicants[i].Clear();
				}
				else
				{
					if (commanderApplicants[i].Count == 0)
					{
						continue;
					}
					if (commanderApplicants[i].Count > 1)
					{
						int j;
						for (j = 0; j < previousCommanders.Count; j++)
						{
							player = commanderApplicants[i].Find((Player k) => k == previousCommanders[j].Commander);
							if (player != null)
							{
								MelonLogger.Msg("Removing applicant from 2 rounds ago from random selection: " + player.PlayerName);
								GameMode.CurrentGameMode.TrySpawnPlayerAtSpawnPoint(player, player.Team);
								commanderApplicants[i].Remove(player);
							}
						}
						if (commanderApplicants[i].Count == 0)
						{
							continue;
						}
					}
					int index = random.Next(0, commanderApplicants[i].Count - 1);
					Player player2 = commanderApplicants[i][index];
					if (player2 != null && player2.Team.Index == i)
					{
						HelperMethods.ReplyToCommand("Promoted " + HelperMethods.GetTeamColor(player2) + player2.PlayerName + "</color> to commander for " + HelperMethods.GetTeamColor(player2) + player2.Team.TeamShortName + "</color>");
						promotedCommanders[player2.Team.Index] = player2;
						CommanderPrimitives.PromoteToCommander(player2);
						PreviousCommander item = new PreviousCommander
						{
							Commander = player2,
							RoundsLeft = 2
						};
						previousCommanders.Add(item);
						commanderApplicants[i].RemoveAt(index);
					}
					else
					{
						MelonLogger.Warning("Can't find lottery winner. Not promoting for team " + Team.Teams[i].TeamShortName);
					}
					foreach (Player item2 in commanderApplicants[i])
					{
						if (!(item2 == null))
						{
							MelonLogger.Msg("Player " + item2.PlayerName + " lost commander lottery. Spawning as infantry.");
							GameMode.CurrentGameMode.TrySpawnPlayerAtSpawnPoint(item2, item2.Team);
						}
					}
					commanderApplicants[i].Clear();
					MelonLogger.Msg("Clearing commander lottery for team " + Team.Teams[i].TeamShortName);
				}
			}
			if (previousCommanders == null || previousCommanders.Count == 0)
			{
				MelonLogger.Msg("Skipping clearing previous commanders. Nothing to do.");
				return;
			}
			List<PreviousCommander> list = new List<PreviousCommander>();
			foreach (PreviousCommander previousCommander in previousCommanders)
			{
				previousCommander.RoundsLeft--;
				if (previousCommander.Commander == null)
				{
					list.Add(previousCommander);
					MelonLogger.Msg("Adding stale, invalid commander.");
				}
				else if (previousCommander.RoundsLeft <= 0)
				{
					list.Add(previousCommander);
					MelonLogger.Msg("Adding stale commander with 0 rounds left: " + previousCommander.Commander.PlayerName);
				}
			}
			foreach (PreviousCommander item3 in list)
			{
				previousCommanders.Remove(item3);
				MelonLogger.Msg("Removed stale commander.");
			}
		}

		private static void RemovePlayerFromRespawnTracker<T>(T gameModeInstance, Player player) where T : GameModeExt
		{
			if (gameModeInstance is MP_Strategy obj)
			{
				FieldInfo field = typeof(MP_Strategy).GetField("PlayerRespawnTracker", BindingFlags.Instance | BindingFlags.NonPublic);
				Dictionary<Player, float> dictionary = (Dictionary<Player, float>)field.GetValue(obj);
				if (dictionary.ContainsKey(player))
				{
					dictionary.Remove(player);
					field.SetValue(obj, dictionary);
				}
			}
			else if (gameModeInstance is MP_TowerDefense obj2)
			{
				FieldInfo field2 = typeof(MP_TowerDefense).GetField("PlayerRespawnTracker", BindingFlags.Instance | BindingFlags.NonPublic);
				Dictionary<Player, float> dictionary2 = (Dictionary<Player, float>)field2.GetValue(obj2);
				if (dictionary2.ContainsKey(player))
				{
					dictionary2.Remove(player);
					field2.SetValue(obj2, dictionary2);
				}
			}
		}

		private static void Process_RPC_TimerUpdate<T>(T gameModeInstance) where T : GameModeExt
		{
			if (gameModeInstance.GameOver)
			{
				return;
			}
			float gameModeTimer = GameModeTimer;
			if (gameModeTimer <= 1f && gameModeTimer > 0f)
			{
				for (int i = 0; i < 8; i++)
				{
					if (commanderApplicants[i].Count == 0)
					{
						continue;
					}
					foreach (Player item in commanderApplicants[i])
					{
						if (!(item == null))
						{
							CommanderPrimitives.SendToRole(item, GameModeExt.ETeamRole.NONE);
							MelonLogger.Msg("Player " + item.PlayerName + " is being sent to role NONE.");
							gameModeInstance.DestroyAllUnitsForPlayer(item);
							RemovePlayerFromRespawnTracker(gameModeInstance, item);
						}
					}
				}
			}
			if (CommanderManager._BlockRoundStartUntilEnoughApplicants.Value && !AllTeamsHaveCommanderApplicants() && gameModeTimer <= 5f && gameModeTimer > 4f)
			{
				HelperMethods.ReplyToCommand("Round cannot start because all teams don't have a commander. Chat !commander to apply.");
				GameModeTimer = 26f;
			}
		}
	}
	public class CommanderAdminCommands
	{
		public static void Command_CommanderBan(Player? callerPlayer, string args)
		{
			if (CommanderBans.BanList == null)
			{
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
			Player player = HelperMethods.FindTargetPlayer(args.Split(' ')[1]);
			if (player == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Ambiguous or invalid target");
			}
			else if (callerPlayer != null && !callerPlayer.CanAdminTarget(player))
			{
				HelperMethods.ReplyToCommand_Player(player, "is immune due to level");
			}
			else
			{
				CommanderBans.AddBan(player);
				HelperMethods.AlertAdminAction(callerPlayer, "restricted " + HelperMethods.GetTeamColor(player) + player.PlayerName + "</color> to play as infantry only");
			}
		}

		public static void Command_CommanderDemote(Player? callerPlayer, string args)
		{
			string text = args.Split(' ')[0];
			int num = args.Split(' ').Length - 1;
			if (num > 1)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too many arguments");
				return;
			}
			int num2 = -1;
			if (num == 0)
			{
				if (callerPlayer == null || callerPlayer.Team == null)
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too few arguments");
					return;
				}
				num2 = callerPlayer.Team.Index;
			}
			else
			{
				string text2 = args.Split(' ')[1];
				if (string.Equals(text2, "Alien", StringComparison.OrdinalIgnoreCase))
				{
					num2 = 0;
				}
				else if (text2.Contains("Cent", StringComparison.OrdinalIgnoreCase))
				{
					num2 = 3;
				}
				else if (string.Equals(text2, "Sol", StringComparison.OrdinalIgnoreCase))
				{
					num2 = 4;
				}
				if (num2 < 0)
				{
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Valid targets are Alien, Centauri, or Sol");
					return;
				}
			}
			Team teamByIndex = Team.GetTeamByIndex(num2);
			if (teamByIndex == null)
			{
				HelperMethods.ReplyToCommand(text + ": No valid team found");
				return;
			}
			Player commander = CommanderPrimitives.GetCommander(teamByIndex);
			if (commander == null)
			{
				HelperMethods.ReplyToCommand(text + ": No commander found on specified team");
			}
			else if (callerPlayer != null && !callerPlayer.CanAdminTarget(commander))
			{
				HelperMethods.ReplyToCommand_Player(commander, "is immune due to level");
			}
			else
			{
				CommanderPrimitives.DemoteTeamsCommander(teamByIndex);
				HelperMethods.AlertAdminActivity(callerPlayer, commander, "demoted");
			}
		}

		public static void Command_CommanderUnban(Player? callerPlayer, string args)
		{
			if (CommanderBans.BanList == null)
			{
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
			Player player = HelperMethods.FindTargetPlayer(args.Split(' ')[1]);
			if (player == null)
			{
				HelperMethods.ReplyToCommand(text + ": Ambiguous or invalid target");
			}
			else if (callerPlayer != null && !callerPlayer.CanAdminTarget(player))
			{
				HelperMethods.ReplyToCommand_Player(player, "is immune due to level");
			}
			else if (CommanderBans.RemoveBan(player))
			{
				HelperMethods.AlertAdminAction(callerPlayer, "permitted " + HelperMethods.GetTeamColor(player) + player.PlayerName + "</color> to play as commander");
			}
			else
			{
				HelperMethods.ReplyToCommand_Player(player, "not commander banned");
			}
		}
	}
	public class BanEntry
	{
		public long OffenderSteamId { get; set; }

		public string? OffenderName { get; set; }

		public int UnixBanTime { get; set; }

		public string? Comments { get; set; }
	}
	public class CommanderBans
	{
		public static readonly string banListFile = Path.Combine(MelonEnvironment.UserDataDirectory, "commander_bans.json");

		public static List<BanEntry>? BanList;

		public static void UpdateCommanderBanFile()
		{
			string contents = JsonConvert.SerializeObject(BanList, Formatting.Indented);
			File.WriteAllText(banListFile, contents);
		}

		public static void InitializeList()
		{
			if (File.Exists(banListFile))
			{
				StreamReader streamReader = File.OpenText(banListFile);
				using (streamReader)
				{
					string text = streamReader.ReadToEnd();
					if (text == null)
					{
						MelonLogger.Warning("The commander_bans.json read as empty. No commander ban entries loaded.");
						BanList = new List<BanEntry>();
						return;
					}
					BanList = JsonConvert.DeserializeObject<List<BanEntry>>(text);
					if (BanList == null)
					{
						MelonLogger.Error("Encountered deserialization error in commander_bans.json file. Ensure file is in valid format (e.g. https://jsonlint.com/)");
						BanList = new List<BanEntry>();
					}
					else
					{
						MelonLogger.Msg("Loaded Silica commander banlist with " + BanList.Count + " entries.");
					}
					return;
				}
			}
			MelonLogger.Warning("Did not find commander_bans.json file. No commander ban entries loaded.");
			BanList = new List<BanEntry>();
		}

		public static bool IsBanned(Player player)
		{
			if (BanList == null)
			{
				return false;
			}
			long playerSteamId = (long)player.PlayerID.SteamID.m_SteamID;
			if (BanList.Find((BanEntry i) => i.OffenderSteamId == playerSteamId) != null)
			{
				return true;
			}
			return false;
		}

		public static bool RemoveBan(Player playerToCmdrUnban)
		{
			if (BanList == null)
			{
				return false;
			}
			BanEntry banEntry = BanList.Find((BanEntry i) => i.OffenderSteamId == (long)playerToCmdrUnban.PlayerID.SteamID.m_SteamID);
			if (banEntry == null)
			{
				return false;
			}
			MelonLogger.Msg("Removed player name (" + banEntry.OffenderName + ") SteamID (" + banEntry.OffenderSteamId + ") from the commander banlist.");
			BanList.Remove(banEntry);
			UpdateCommanderBanFile();
			return true;
		}

		public static void AddBan(Player playerToCmdrBan)
		{
			if (BanList != null)
			{
				Player serverPlayer = NetworkGameServer.GetServerPlayer();
				BanEntry thisBan = new BanEntry
				{
					OffenderSteamId = (long)playerToCmdrBan.PlayerID.SteamID.m_SteamID,
					OffenderName = playerToCmdrBan.PlayerName,
					UnixBanTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
					Comments = "banned from playing commander by " + serverPlayer.PlayerName
				};
				if (playerToCmdrBan.IsCommander)
				{
					CommanderPrimitives.DemoteTeamsCommander(playerToCmdrBan.Team);
					HelperMethods.ReplyToCommand_Player(playerToCmdrBan, "was demoted");
				}
				if (BanList.Find((BanEntry i) => i.OffenderSteamId == thisBan.OffenderSteamId) != null)
				{
					MelonLogger.Warning("Player name (" + thisBan.OffenderName + ") SteamID (" + thisBan.OffenderSteamId + ") already on commander banlist.");
				}
				else
				{
					MelonLogger.Msg("Added player name (" + thisBan.OffenderName + ") SteamID (" + thisBan.OffenderSteamId + ") to the commander banlist.");
					BanList.Add(thisBan);
					UpdateCommanderBanFile();
				}
			}
		}
	}
	public class CommanderTransits
	{
		[HarmonyPatch(typeof(GameMode), "SpawnUnitForPlayer", new Type[]
		{
			typeof(Player),
			typeof(GameObject),
			typeof(Vector3),
			typeof(Quaternion)
		})]
		private static class CommanderManager_Patch_GameMode_SpawnUnitForPlayer
		{
			public static void Postfix(GameMode __instance, Unit __result, Player __0, GameObject __1, Vector3 __2, Quaternion __3)
			{
				try
				{
					if (CommanderApplications.teamswapCommanderChecks == null || __0 == null || __0.Team == null || !GameMode.CurrentGameMode.Started || !GameMode.CurrentGameMode.GameBegun)
					{
						return;
					}
					int num = -1;
					for (int i = 0; i < 8; i++)
					{
						if (CommanderApplications.teamswapCommanderChecks[i] == __0)
						{
							num = i;
							break;
						}
					}
					if (num == -1)
					{
						return;
					}
					Team team = Team.Teams[num];
					CommanderApplications.teamswapCommanderChecks[num] = null;
					if (team.NumMajorStructures > 0)
					{
						if (CommanderManager._TeamOnlyResponses.Value)
						{
							HelperMethods.SendChatMessageToTeam(__0.Team, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", HelperMethods.GetTeamColor(__0.Team), __0.PlayerName, "</color> left commander position vacant for " + HelperMethods.GetTeamColor(team) + team.TeamShortName + "</color> by switching to infantry");
						}
						else
						{
							HelperMethods.ReplyToCommand_Player(__0, "left commander position vacant for " + HelperMethods.GetTeamColor(team) + team.TeamShortName + "</color> by switching to infantry");
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run GameMode::SpawnUnitForPlayer");
				}
			}
		}

		[HarmonyPatch(typeof(GameMode), "OnPlayerLeftBase")]
		private static class CommanderManager_Patch_GameMode_OnPlayerLeftBase
		{
			public static void Prefix(GameMode __instance, Player __0)
			{
				try
				{
					if (CommanderApplications.commanderApplicants == null || __0 == null || __0.Team == null)
					{
						return;
					}
					if (GameMode.CurrentGameMode.Started && !GameMode.CurrentGameMode.GameBegun && CommanderApplications.IsApplicant(__0))
					{
						CommanderApplications.commanderApplicants[__0.Team.Index].Remove(__0);
						if (CommanderManager._TeamOnlyResponses.Value)
						{
							HelperMethods.SendChatMessageToTeam(__0.Team, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", HelperMethods.GetTeamColor(__0.Team), __0.PlayerName, "</color> was removed from consideration due to disconnect");
						}
						else
						{
							HelperMethods.ReplyToCommand_Player(__0, "was removed from consideration due to disconnect");
						}
					}
					if (GameMode.CurrentGameMode.Started && GameMode.CurrentGameMode.GameBegun && __0.IsCommander && __0.Team.NumMajorStructures > 0)
					{
						if (CommanderManager._TeamOnlyResponses.Value)
						{
							HelperMethods.SendChatMessageToTeam(__0.Team, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", HelperMethods.GetTeamColor(__0.Team), __0.PlayerName, "</color> left commander position vacant by disconnecting");
						}
						else
						{
							HelperMethods.ReplyToCommand_Player(__0, "left commander position vacant by disconnecting");
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run GameMode::OnPlayerLeftBase");
				}
			}
		}

		[HarmonyPatch(typeof(MP_TowerDefense), "SetCommander")]
		private static class ApplyPatch_MPTowerDefense_SetCommander
		{
			public static bool Prefix(MP_TowerDefense __instance, Team __0, Player? __1)
			{
				try
				{
					return SetCommanderPatch(__instance, __0, __1);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_TowerDefense::SetCommander");
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(MP_Strategy), "SetCommander")]
		private static class ApplyPatch_MPStrategy_SetCommander
		{
			public static bool Prefix(MP_Strategy __instance, Team __0, Player? __1)
			{
				try
				{
					return SetCommanderPatch(__instance, __0, __1);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_Strategy::SetCommander");
				}
				return true;
			}
		}

		private static bool SetCommanderPatch<T>(T gameModeInstance, Team team, Player? player) where T : GameModeExt
		{
			MelonLogger.Msg("Reached SetCommander Patch for Team " + team.TeamShortName);
			if (gameModeInstance == null || team == null || CommanderBans.BanList == null || CommanderApplications.commanderApplicants == null || CommanderApplications.teamswapCommanderChecks == null || CommanderApplications.promotedCommanders == null)
			{
				return true;
			}
			if (player != null)
			{
				if (GameMode.CurrentGameMode.Started && GameMode.CurrentGameMode.GameBegun)
				{
					int num = -1;
					for (int i = 0; i < 8; i++)
					{
						if (i != team.Index && CommanderApplications.teamswapCommanderChecks[i] == player)
						{
							num = i;
							break;
						}
					}
					if (num != -1)
					{
						Team team2 = Team.Teams[num];
						HelperMethods.ReplyToCommand_Player(player, "has left command of " + HelperMethods.GetTeamColor(team2) + team2.TeamShortName + "</color> and taken command of " + HelperMethods.GetTeamColor(team) + team.TeamShortName + "</color>");
						CommanderApplications.teamswapCommanderChecks[num] = null;
					}
					else if (CommanderApplications.teamswapCommanderChecks[team.Index] != player && CommanderApplications.promotedCommanders[team.Index] != player)
					{
						CommanderApplications.promotedCommanders[team.Index] = player;
						if (CommanderManager._TeamOnlyResponses.Value)
						{
							HelperMethods.SendChatMessageToTeam(player.Team, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", HelperMethods.GetTeamColor(player.Team), player.PlayerName, "</color> has taken command of " + HelperMethods.GetTeamColor(team) + team.TeamShortName + "</color>");
						}
						else
						{
							HelperMethods.ReplyToCommand_Player(player, "has taken command of " + HelperMethods.GetTeamColor(team) + team.TeamShortName + "</color>");
						}
					}
				}
			}
			else
			{
				Player commanderForTeam = gameModeInstance.GetCommanderForTeam(team);
				if (commanderForTeam != null)
				{
					CommanderApplications.teamswapCommanderChecks[team.Index] = commanderForTeam;
				}
			}
			return true;
		}
	}
	public class CommanderPrimitives
	{
		public static Player? GetCommander(Team team)
		{
			if ((bool)GameMode.CurrentGameMode)
			{
				if (GameMode.CurrentGameMode is MP_Strategy mP_Strategy)
				{
					return mP_Strategy.GetCommanderForTeam(team);
				}
				if (GameMode.CurrentGameMode is MP_TowerDefense mP_TowerDefense)
				{
					return mP_TowerDefense.GetCommanderForTeam(team);
				}
			}
			return null;
		}

		public static void DemoteTeamsCommander(Team TargetTeam)
		{
			Player commander = GetCommander(TargetTeam);
			if (commander == null)
			{
				throw new ArgumentException("Team has no commander");
			}
			SetCommander(TargetTeam, null);
			SendToRole(commander, GameModeExt.ETeamRole.UNIT);
			GameMode.CurrentGameMode.TrySpawnPlayerAtSpawnPoint(commander, TargetTeam);
		}

		private static object?[] FillOptionalParameters(MethodInfo method, params object?[] args)
		{
			ParameterInfo[] parameters = method.GetParameters();
			object[] array = new object[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				if (i < args.Length)
				{
					array[i] = args[i];
				}
				else
				{
					array[i] = (parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null);
				}
			}
			return array;
		}

		private static void SetCommander(Team team, Player? player)
		{
			if (GameMode.CurrentGameMode is MP_Strategy mP_Strategy)
			{
				Type type = mP_Strategy.GetType();
				type.GetMethod("SetCommander", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(mP_Strategy, new object[2] { team, player });
				MethodInfo method = type.GetMethod("RPC_SynchCommander", BindingFlags.Instance | BindingFlags.NonPublic);
				method.Invoke(mP_Strategy, FillOptionalParameters(method, team));
			}
			else if (GameMode.CurrentGameMode is MP_TowerDefense mP_TowerDefense)
			{
				Type type2 = mP_TowerDefense.GetType();
				type2.GetMethod("SetCommander", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(mP_TowerDefense, new object[2] { team, player });
				MethodInfo method2 = type2.GetMethod("RPC_SynchCommander", BindingFlags.Instance | BindingFlags.NonPublic);
				method2.Invoke(mP_TowerDefense, FillOptionalParameters(method2, team));
			}
		}

		public static void PromoteToCommander(Player commander)
		{
			MelonLogger.Msg("Trying to promote " + commander.PlayerName + " on team " + commander.Team.TeamShortName);
			SetCommander(commander.Team, commander);
			Event_Roles.FireOnRoleChangedEvent(commander, GameModeExt.ETeamRole.COMMANDER);
		}

		public static void SendToRole(Player FormerCommander, GameModeExt.ETeamRole role)
		{
			GameByteStreamWriter gameByteStreamWriter = GameMode.CurrentGameMode.CreateRPCPacket(2);
			if (gameByteStreamWriter != null)
			{
				gameByteStreamWriter.WriteUInt64((ulong)FormerCommander.PlayerID);
				gameByteStreamWriter.WriteByte((byte)FormerCommander.PlayerChannel);
				gameByteStreamWriter.WriteByte((byte)role);
				GameMode.CurrentGameMode.SendRPCPacket(gameByteStreamWriter);
			}
		}
	}
	public class Mutineer
	{
		public static void InitializeMutineerList()
		{
			CommanderManager.mutineerPlayers = new List<Player>[9];
			for (int i = 0; i < 8; i++)
			{
				CommanderManager.mutineerPlayers[i] = new List<Player>();
			}
		}

		public static void ClearMutineerList()
		{
			for (int i = 0; i < 8; i++)
			{
				CommanderManager.mutineerPlayers[i].Clear();
			}
		}

		public static void Command_Mutiny(Player? callerPlayer, string args)
		{
			if (callerPlayer == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Console not supported.");
				return;
			}
			if (callerPlayer.Team == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " On invalid team.");
				return;
			}
			if (!GameMode.CurrentGameMode || !GameMode.CurrentGameMode.GameOngoing)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Cannot mutiny at this time.");
				return;
			}
			if (callerPlayer.IsCommander)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Cannot mutiny when commader.");
				return;
			}
			if (CommanderPrimitives.GetCommander(callerPlayer.Team) == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Cannot mutiny when no one is commander.");
				return;
			}
			if (CommanderManager.mutineerPlayers[callerPlayer.Team.Index].Contains(callerPlayer))
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Already voted to mutiny. ", MoreMutinyVotesNeeded(callerPlayer.Team).ToString(), " more players needed.");
				return;
			}
			CommanderManager.mutineerPlayers[callerPlayer.Team.Index].Add(callerPlayer);
			if (CommanderManager.mutineerPlayers[callerPlayer.Team.Index].Count < TeammatesNeededForMutiny(callerPlayer.Team))
			{
				HelperMethods.SendChatMessageToTeamNoCommander(callerPlayer.Team, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", HelperMethods.GetTeamColor(callerPlayer), " ", callerPlayer.PlayerName, "</color> wants to mutiny. ", MoreMutinyVotesNeeded(callerPlayer.Team).ToString(), " more players needed.");
			}
			else
			{
				Mutiny(callerPlayer.Team);
			}
		}

		public static int TeammatesNeededForMutiny(Team team)
		{
			int num = (int)Math.Ceiling((float)(team.GetNumPlayers() - ((CommanderPrimitives.GetCommander(team) != null) ? 1 : 0)) * CommanderManager._MutinyVotePercent.Value);
			if (num < 1)
			{
				return 1;
			}
			return num;
		}

		public static int MoreMutinyVotesNeeded(Team team)
		{
			int num = TeammatesNeededForMutiny(team) - CommanderManager.mutineerPlayers[team.Index].Count;
			if (num < 1)
			{
				return 1;
			}
			return num;
		}

		public static void Mutiny(Team team)
		{
			HelperMethods.ReplyToCommand(HelperMethods.GetTeamColor(team) + team.TeamShortName + "</color> had a mutiny against the current commander.");
			CommanderPrimitives.DemoteTeamsCommander(team);
			ClearMutineerList();
		}
	}
	public class PreviousCommander
	{
		private Player _commander;

		public Player Commander
		{
			get
			{
				return _commander;
			}
			set
			{
				_commander = value ?? throw new ArgumentNullException("Player is required.");
			}
		}

		public int RoundsLeft { get; set; }
	}
	public class CommanderManager : MelonMod
	{
		private static MelonPreferences_Category _modCategory;

		public static MelonPreferences_Entry<bool> _BlockRoundStartUntilEnoughApplicants;

		public static MelonPreferences_Entry<bool> _TeamOnlyResponses;

		public static MelonPreferences_Entry<float> _MutinyVotePercent;

		public static MelonPreferences_Entry<bool> _SuppressRoundStartCommanderChat;

		public static MelonPreferences_Entry<bool> _SuppressCountdownChat;

		public static List<Player>[] mutineerPlayers;

		public override void OnInitializeMelon()
		{
			if (_modCategory == null)
			{
				_modCategory = MelonPreferences.CreateCategory("Silica");
			}
			if (_BlockRoundStartUntilEnoughApplicants == null)
			{
				_BlockRoundStartUntilEnoughApplicants = _modCategory.CreateEntry<bool>("BlockRoundStartUntilCommandersApplied", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_TeamOnlyResponses == null)
			{
				_TeamOnlyResponses = _modCategory.CreateEntry<bool>("CmdrMgr_CommanderResponses_TeamOnly", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_MutinyVotePercent == null)
			{
				_MutinyVotePercent = _modCategory.CreateEntry<float>("CmdrMgr_Mutiny_Vote_PercentNeeded", 0.54f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_SuppressRoundStartCommanderChat == null)
			{
				_SuppressRoundStartCommanderChat = _modCategory.CreateEntry<bool>("CmdrMgr_SuppressChatSpam_RoundStart", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_SuppressCountdownChat == null)
			{
				_SuppressCountdownChat = _modCategory.CreateEntry<bool>("CmdrMgr_SuppressChatSpam_AICountdown", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			try
			{
				CommanderBans.InitializeList();
				CommanderApplications.InitializeApplications();
				Mutineer.InitializeMutineerList();
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to load Silica commander banlist (OnInitializeMelon)");
			}
		}

		[MethodImpl(MethodImplOptions.NoOptimization)]
		public override void OnLateInitializeMelon()
		{
			HelperMethods.CommandCallback adminCallback = CommanderAdminCommands.Command_CommanderBan;
			HelperMethods.RegisterAdminCommand("cmdrban", adminCallback, Power.Commander, "Prevents target player from applying or playing as a Commander. Usage: !cmdrban <player>");
			HelperMethods.RegisterAdminCommand("commanderban", adminCallback, Power.Commander, "Prevents target player from applying or playing as a Commander. Usage: !commanderban <player>");
			HelperMethods.RegisterAdminCommand("cban", adminCallback, Power.Commander, "Prevents target player from applying or playing as a Commander. Usage: !cban <player>");
			HelperMethods.CommandCallback adminCallback2 = CommanderAdminCommands.Command_CommanderUnban;
			HelperMethods.RegisterAdminCommand("removecommanderban", adminCallback2, Power.Commander, "Allows target player to apply or play as a Commander. Usage: !removecommanderban <player>");
			HelperMethods.RegisterAdminCommand("uncban", adminCallback2, Power.Commander, "Allows target player to apply or play as a Commander. Usage: !uncban <player>");
			HelperMethods.CommandCallback adminCallback3 = CommanderAdminCommands.Command_CommanderDemote;
			HelperMethods.RegisterAdminCommand("demote", adminCallback3, Power.Commander, "Demotes target player from their current Commander role. Usage: !demote <player>");
			HelperMethods.CommandCallback commandCallback = CommanderApplications.Command_Commander;
			HelperMethods.RegisterPlayerCommand("commander", commandCallback, hideFromChat: true);
			HelperMethods.CommandCallback commandCallback2 = Mutineer.Command_Mutiny;
			HelperMethods.RegisterPlayerCommand("mutiny", commandCallback2, hideFromChat: true);
			Event_Roles.OnRequestCommander += OnRequestCommander;
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			Mutineer.ClearMutineerList();
		}

		public void OnRequestCommander(object? sender, OnRequestCommanderArgs args)
		{
			if (args.Requester == null || CommanderBans.BanList == null)
			{
				return;
			}
			if (CommanderBans.IsBanned(args.Requester))
			{
				MelonLogger.Msg("Preventing " + args.Requester.PlayerName + " from playing as commander.");
				args.Block = true;
			}
			else
			{
				if (!GameMode.CurrentGameMode.Started || GameMode.CurrentGameMode.GameBegun)
				{
					return;
				}
				if (!CommanderApplications.IsApplicant(args.Requester))
				{
					if (_TeamOnlyResponses.Value)
					{
						HelperMethods.SendChatMessageToTeam(args.Requester.Team, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", HelperMethods.GetTeamColor(args.Requester.Team), args.Requester.PlayerName, "</color> has applied for commander.");
					}
					else
					{
						HelperMethods.ReplyToCommand_Player(args.Requester, "has applied for commander");
					}
					CommanderApplications.commanderApplicants[args.Requester.Team.Index].Add(args.Requester);
				}
				MelonLogger.Msg("Denied early game commander join for " + args.Requester.PlayerName);
				args.Block = true;
				args.PreventSpawnWhenBlocked = false;
			}
		}
	}
	public class SuppressChat
	{
		[HarmonyPatch(typeof(Player), "SendServerChatMessage")]
		private static class CommanderManager_Patch_Player_SendServerChatMessage
		{
			private static bool Prefix(bool __result, string __0)
			{
				try
				{
					string name = new StackFrame(2, fNeedFileInfo: true).GetMethod().Name;
					if (CommanderManager._SuppressRoundStartCommanderChat.Value && name.Contains("OnMissionStateChanged"))
					{
						return false;
					}
					if (name.Contains("SetCommander"))
					{
						return false;
					}
					if (CommanderManager._SuppressCountdownChat.Value && string.Equals(name, "UpdateGameLoop"))
					{
						return false;
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run Player::SendServerChatMessage");
				}
				return true;
			}
		}
	}
}
