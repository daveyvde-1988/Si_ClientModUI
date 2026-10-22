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
using Si_Mapcycle;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(MapCycleMod), "Mapcycle", "1.8.2", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_Mapcycle")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_Mapcycle")]
[assembly: AssemblyTitle("Si_Mapcycle")]
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
namespace Si_Mapcycle
{
	public class MapCycleMod : MelonMod
	{
		[HarmonyPatch(typeof(MusicJukeboxHandler), "OnGameEnded")]
		private static class ApplyPatch_OnGameEnded
		{
			public static void Postfix(MusicJukeboxHandler __instance, GameMode __0, Team __1)
			{
				try
				{
					if (firedRoundEndOnce)
					{
						return;
					}
					firedRoundEndOnce = true;
					if (!IsMapCycleValid())
					{
						MelonLogger.Warning("sMapCycle is null. Skipping end-round routines.");
						return;
					}
					roundsOnSameMap++;
					if (Pref_Mapcycle_RoundsBeforeChange.Value > roundsOnSameMap)
					{
						int num = Pref_Mapcycle_RoundsBeforeChange.Value - roundsOnSameMap;
						HelperMethods.ReplyToCommand("Current map will change after " + num + " more round" + ((num == 1) ? "." : "s."));
						return;
					}
					if (HelperMethods.IsTimerActive(Timer_EndRoundDelay))
					{
						MelonLogger.Warning("End round delay timer already started.");
						return;
					}
					if (HelperMethods.IsTimerActive(Timer_FinalPostVoteDelay))
					{
						Timer_FinalPostVoteDelay = HelperMethods.Timer_Inactive;
						MelonLogger.Warning("Game ended while final RTV timer was in progress. Forcing timer to expire.");
					}
					if (HelperMethods.IsTimerActive(Timer_InitialPostVoteDelay))
					{
						Timer_InitialPostVoteDelay = HelperMethods.Timer_Inactive;
						MelonLogger.Warning("Game ended while initial RTV timer was in progress. Forcing timer to expire.");
					}
					HelperMethods.ReplyToCommand("Preparing to change map to " + GetNextMap() + "....");
					HelperMethods.StartTimer(ref Timer_EndRoundDelay);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MusicJukeboxHandler::OnGameEnded");
				}
			}
		}

		[HarmonyPatch(typeof(MusicJukeboxHandler), "OnGameStarted")]
		private static class ApplyPatch_OnGameStarted
		{
			public static void Prefix(MusicJukeboxHandler __instance, GameMode __0)
			{
				try
				{
					firedRoundEndOnce = false;
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MusicJukeboxHandler::OnGameStarted");
				}
			}
		}

		private static string mapName = "";

		private static int iMapLoadCount;

		private static bool firedRoundEndOnce;

		private static int roundsOnSameMap;

		private static string rockthevoteWinningMap = "";

		private static readonly string mapCycleFile = Path.Combine(MelonEnvironment.UserDataDirectory, "mapcycle.txt");

		private static float Timer_EndRoundDelay = HelperMethods.Timer_Inactive;

		private static float Timer_InitialPostVoteDelay = HelperMethods.Timer_Inactive;

		private static float Timer_FinalPostVoteDelay = HelperMethods.Timer_Inactive;

		private static MelonPreferences_Category _modCategory = null;

		private static MelonPreferences_Entry<int> Pref_Mapcycle_RoundsBeforeChange = null;

		private static MelonPreferences_Entry<int> Pref_Mapcycle_EndgameDelay = null;

		private static MelonPreferences_Entry<float> Pref_Mapcycle_RockTheVote_Percent = null;

		private static MelonPreferences_Entry<int> Pref_Mapcycle_RockTheVote_MaximumMaps = null;

		private static List<Player> rockers = new List<Player>();

		private static List<VotingOption> mapNominations = new List<VotingOption>();

		private static List<VotingOption> defaultRtvStrategyList = new List<VotingOption>();

		private static List<string> gameModes = new List<string>();

		private static List<VotingOption> mapCycleEntries = new List<VotingOption>();

		public override void OnLateInitializeMelon()
		{
			try
			{
				if (!File.Exists(mapCycleFile))
				{
					CreateDefaultMapcycle(mapCycleFile);
				}
				ParseMapcycle(mapCycleFile);
				defaultRtvStrategyList = CreateVotingOptions();
				foreach (VotingOption mapCycleEntry in mapCycleEntries)
				{
					if (!IsMapNameValid(mapCycleEntry.MapName))
					{
						MelonLogger.Error("Invalid map found in mapcycle.txt: " + mapCycleEntry.MapName);
					}
				}
				HelperMethods.CommandCallback adminCallback = Command_ChangeMap;
				HelperMethods.RegisterAdminCommand("map", adminCallback, Power.Map);
				HelperMethods.CommandCallback commandCallback = Command_RockTheVote;
				HelperMethods.RegisterPlayerPhrase("rtv", commandCallback, hideFromChat: true);
				HelperMethods.RegisterPlayerCommand("rtv", commandCallback, hideFromChat: true);
				HelperMethods.RegisterPlayerPhrase("rockthevote", commandCallback, hideFromChat: true);
				HelperMethods.RegisterPlayerCommand("rockthevote", commandCallback, hideFromChat: true);
				HelperMethods.CommandCallback commandCallback2 = Command_Nominate;
				HelperMethods.RegisterPlayerCommand("nominate", commandCallback2, hideFromChat: true);
				HelperMethods.CommandCallback commandCallback3 = Command_CurrentMap;
				HelperMethods.RegisterPlayerPhrase("currentmap", commandCallback3, hideFromChat: true);
				HelperMethods.CommandCallback commandCallback4 = Command_NextMap;
				HelperMethods.RegisterPlayerPhrase("nextmap", commandCallback4, hideFromChat: true);
				HelperMethods.CommandCallback commandCallback5 = Command_showmapcycle;
				HelperMethods.RegisterPlayerPhrase("mapcycle", commandCallback5, hideFromChat: true);
			}
			catch (Exception ex)
			{
				MelonLogger.Error(ex.ToString());
			}
		}

		public static List<string> InitializeGameModeList(bool enabledModesOnly = false)
		{
			List<string> list = new List<string>();
			if (GameDatabase.Database == null || GameDatabase.Database.AllGameModes == null)
			{
				MelonLogger.Error("InitializeGameModeList: GameDatabase.Database or .AllGameModes is null");
				return list;
			}
			foreach (GameModeInfo allGameMode in GameDatabase.Database.AllGameModes)
			{
				if (!enabledModesOnly || allGameMode.Enabled)
				{
					list.Add(allGameMode.ObjectName);
				}
			}
			return list;
		}

		public static string NormalizeGameMode(string gameMode)
		{
			List<string> source = InitializeGameModeList();
			if (string.IsNullOrWhiteSpace(gameMode))
			{
				return gameMode;
			}
			string standardizedInput = gameMode.Replace(" ", "_");
			string text = source.FirstOrDefault((string gm) => string.Equals(gm, standardizedInput, StringComparison.OrdinalIgnoreCase));
			if (text != null)
			{
				return text;
			}
			MelonLogger.Warning("Gamemode: " + gameMode + " not found. Setting gamemode to Default");
			return "";
		}

		public override void OnInitializeMelon()
		{
			try
			{
				if (_modCategory == null)
				{
					_modCategory = MelonPreferences.CreateCategory("Silica");
				}
				if (Pref_Mapcycle_RoundsBeforeChange == null)
				{
					Pref_Mapcycle_RoundsBeforeChange = _modCategory.CreateEntry<int>("Mapcycle_RoundsBeforeMapChange", 2, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Mapcycle_EndgameDelay == null)
				{
					Pref_Mapcycle_EndgameDelay = _modCategory.CreateEntry<int>("Mapcycle_DelayBeforeEndgameMapChange_Seconds", 9, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Mapcycle_RockTheVote_Percent == null)
				{
					Pref_Mapcycle_RockTheVote_Percent = _modCategory.CreateEntry<float>("Mapcycle_RockTheVote_PercentNeeded", 0.31f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Mapcycle_RockTheVote_MaximumMaps == null)
				{
					Pref_Mapcycle_RockTheVote_MaximumMaps = _modCategory.CreateEntry<int>("Mapcycle_RockTheVote_MaximumMaps", 5, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Msg(ex.ToString());
			}
		}

		public static void Command_showmapcycle(Player? callerplayer, string args)
		{
			string text = "Current Mapcycle: \n";
			foreach (VotingOption mapCycleEntry in mapCycleEntries)
			{
				text += (mapCycleEntry.GameMode.Equals("MP_Strategy", StringComparison.OrdinalIgnoreCase) ? (" - " + mapCycleEntry.MapName + "\n") : (" - " + mapCycleEntry.MapName + " " + mapCycleEntry.GameMode + "\n"));
			}
			HelperMethods.SendChatMessageToPlayer(callerplayer, text);
		}

		public static void Command_RockTheVote(Player? callerPlayer, string args)
		{
			if (callerPlayer == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Console not supported.");
				return;
			}
			if (!GameMode.CurrentGameMode.GameOngoing)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Can't rock the vote. Game not started.");
				return;
			}
			if (HelperMethods.IsTimerActive(Timer_EndRoundDelay))
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Can't rock the vote. Map change already in progress.");
				return;
			}
			if (rockers.Contains(callerPlayer))
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Already used rtv. ", MoreRocksNeededForVote().ToString(), " more needed.");
				return;
			}
			rockers.Add(callerPlayer);
			if (rockers.Count < RocksNeededForVote())
			{
				HelperMethods.ReplyToCommand_Player(callerPlayer, "wants to rock the vote. ", MoreRocksNeededForVote().ToString(), " more needed.");
				return;
			}
			if (ChatVotes.IsVoteInProgress())
			{
				HelperMethods.ReplyToCommand_Player(callerPlayer, "wants to rock the vote. Another vote is in progress. Wait before trying again.");
				return;
			}
			ChatVoteBallot chatVoteBallot = CreateRTVBallot();
			if (chatVoteBallot == null)
			{
				HelperMethods.ReplyToCommand_Player(callerPlayer, "rocked the vote. Currently unavailable. Wait before trying again.");
			}
			else
			{
				HelperMethods.ReplyToCommand_Player(callerPlayer, "rocked the vote.");
				ChatVotes.HoldVote(chatVoteBallot);
			}
		}

		public static List<VotingOption> CreateVotingOptions()
		{
			List<VotingOption> list = new List<VotingOption>();
			System.Random rnd = new System.Random();
			foreach (VotingOption nominatedMap in mapNominations)
			{
				if (!list.Any((VotingOption option) => option.MapName.Equals(nominatedMap.MapName, StringComparison.OrdinalIgnoreCase) && option.GameMode.Equals(nominatedMap.GameMode, StringComparison.OrdinalIgnoreCase)))
				{
					list.Add(new VotingOption(nominatedMap.MapName, nominatedMap.GameMode));
				}
			}
			foreach (VotingOption entry in (from x in mapCycleEntries.GroupBy((VotingOption s) => new { s.MapName, s.GameMode }, (key, g) => g.First()).ToList()
				orderby rnd.Next()
				select x).ToList())
			{
				if (list.Count >= Pref_Mapcycle_RockTheVote_MaximumMaps.Value - 1)
				{
					break;
				}
				if (!list.Any((VotingOption option) => option.MapName.Equals(entry.MapName, StringComparison.OrdinalIgnoreCase) && option.GameMode.Equals(entry.GameMode, StringComparison.OrdinalIgnoreCase)))
				{
					list.Add(new VotingOption(entry.MapName, entry.GameMode));
				}
			}
			if (list.Count > Pref_Mapcycle_RockTheVote_MaximumMaps.Value - 1)
			{
				int count = list.Count - (Pref_Mapcycle_RockTheVote_MaximumMaps.Value - 1);
				foreach (VotingOption item in list.Where((VotingOption option) => !mapNominations.Any((VotingOption nom) => nom.MapName.Equals(option.MapName, StringComparison.OrdinalIgnoreCase) && nom.GameMode.Equals(option.GameMode, StringComparison.OrdinalIgnoreCase))).Take(count).ToList())
				{
					list.Remove(item);
				}
			}
			return list;
		}

		public static ChatVoteBallot? CreateRTVBallot()
		{
			List<VotingOption> currentVotingOptions = CreateVotingOptions();
			if (currentVotingOptions == null || currentVotingOptions.Count == 0)
			{
				MelonLogger.Warning("No valid voting options available.");
				return null;
			}
			OptionPair[] array = new OptionPair[currentVotingOptions.Count + 1];
			array[0] = new OptionPair
			{
				Command = "1",
				Description = "Keep Current Map"
			};
			for (int i = 0; i < currentVotingOptions.Count; i++)
			{
				VotingOption votingOption = currentVotingOptions[i];
				string description = (votingOption.GameMode.Equals("MP_Strategy", StringComparison.OrdinalIgnoreCase) ? votingOption.MapName : (votingOption.MapName + " " + votingOption.GameMode));
				array[i + 1] = new OptionPair
				{
					Command = (i + 2).ToString(),
					Description = description
				};
			}
			MelonLogger.Msg("RTV Ballot Maps:");
			MelonLogger.Msg("1 - Keep Current Map");
			foreach (OptionPair item in array.Skip(1))
			{
				MelonLogger.Msg(item.Command + " " + item.Description);
			}
			return new ChatVoteBallot
			{
				Question = "Vote for a map:",
				VoteHandler = (ChatVoteResults results) =>
				{
					RockTheVote_Handler(results, currentVotingOptions);
				},
				Options = array
			};
		}

		public static void RockTheVote_Handler(ChatVoteResults results, List<VotingOption> currentVotingOptions)
		{
			int num = int.Parse(results.WinningCommand) - 1;
			if (num == 0)
			{
				HelperMethods.SendChatMessageToAll("Vote concluded: The current map will be kept. RTV is now reset and available.");
				ResetRTVStatus();
				mapNominations.Clear();
				currentVotingOptions.Clear();
				return;
			}
			num--;
			if (num < 0 || num >= currentVotingOptions.Count)
			{
				MelonLogger.Error("Invalid map selection index from RTV.");
				return;
			}
			VotingOption votingOption = currentVotingOptions[num];
			string text = votingOption.MapName;
			string gameMode = votingOption.GameMode;
			HelperMethods.SendChatMessageToAll($"Winning RTV Map Option: {num + 2} - {text} ({gameMode})");
			MelonLogger.Msg("Selected map name: " + text + ", Game Mode: " + gameMode);
			QueueChangeMap(text, fromRTV: true, gameMode);
			currentVotingOptions.Clear();
			mapNominations.Clear();
			ResetRTVStatus();
			MelonLogger.Msg("RTV Ballot has been reset.");
		}

		public static void ResetRTVStatus()
		{
			rockers.Clear();
		}

		public static int MoreRocksNeededForVote()
		{
			int num = RocksNeededForVote() - rockers.Count;
			if (num < 1)
			{
				return 1;
			}
			return num;
		}

		public static int RocksNeededForVote()
		{
			int num = (int)Math.Ceiling((float)Player.Players.Count * Pref_Mapcycle_RockTheVote_Percent.Value);
			if (num < 1)
			{
				return 1;
			}
			return num;
		}

		public static void Command_Nominate(Player? callerPlayer, string args)
		{
			if (callerPlayer == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", "Console not supported.");
				return;
			}
			Tuple<string, string> tuple = ArgsToMapAndGamemode(args);
			if (tuple == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", "Too few arguments.");
				return;
			}
			string mapName = tuple.Item1;
			string gameMode = tuple.Item2;
			if (!IsMapNameValid(mapName))
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", "Invalid map name.");
				return;
			}
			if (string.IsNullOrEmpty(gameMode))
			{
				gameMode = GetGameModeInfo(mapName)?.ObjectName;
				if (gameMode == null)
				{
					MelonLogger.Warning("No (highest priority) Gamemode found for map. Input Gamemode was null or empty");
					return;
				}
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", "Gamemode was not specified or rubbish and is set to highest priority Gamemode: " + gameMode);
			}
			if (mapCycleEntries.Count == 0)
			{
				MelonLogger.Warning("Map cycle is invalid or not loaded.");
				return;
			}
			if (mapNominations.Any((VotingOption entry) => entry.MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase) && entry.GameMode.Equals(gameMode, StringComparison.OrdinalIgnoreCase)))
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", mapName + ": Map already nominated");
				return;
			}
			if (defaultRtvStrategyList.FirstOrDefault((VotingOption option) => option.MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase) && option.GameMode.Equals(gameMode, StringComparison.OrdinalIgnoreCase)) != null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", "Map already nominated.");
				return;
			}
			if (mapNominations.Count >= 3)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", "Too many map nominations already received.");
				return;
			}
			if (ChatVotes.IsVoteInProgress())
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", "Can't nominate a map because a vote is in progress.");
				return;
			}
			if (!GameMode.CurrentGameMode.GameOngoing)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", "Can't nominate a map yet. Game not started.");
				return;
			}
			mapNominations.Add(new VotingOption(mapName, gameMode));
			HelperMethods.ReplyToCommand_Player(callerPlayer, "Nominated " + mapName + " with game mode " + gameMode + ".");
		}

		public static void Command_CurrentMap(Player? callerPlayer, string args)
		{
			if (callerPlayer == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Console not supported.");
			}
			else
			{
				HelperMethods.ReplyToCommand_Player(callerPlayer, ": The current map is " + GetDisplayName(mapName));
			}
		}

		public static void Command_NextMap(Player? callerPlayer, string args)
		{
			if (callerPlayer == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Console not supported.");
				return;
			}
			if (mapCycleEntries == null)
			{
				MelonLogger.Warning("sMapCycle is null. Skipping nextmap handling.");
				return;
			}
			int num = Pref_Mapcycle_RoundsBeforeChange.Value - roundsOnSameMap;
			HelperMethods.ReplyToCommand_Player(callerPlayer, ": The next map is " + GetDisplayName(GetNextMap()) + ". " + num + " more round" + ((num == 1) ? "" : "s") + " before map changes.");
		}

		public static string GetNextMap()
		{
			return mapCycleEntries[(iMapLoadCount + 1) % mapCycleEntries.Count].MapName;
		}

		public static bool IsMapCycleValid()
		{
			if (mapCycleEntries == null || !mapCycleEntries.Any())
			{
				return false;
			}
			return true;
		}

		private static Tuple<string, string>? ArgsToMapAndGamemode(string args)
		{
			string[] array = args.Split(new char[1] { ' ' }, 3);
			if (array == null || array.Length < 2)
			{
				return null;
			}
			string item = array[1].Trim();
			string gameMode = ((array.Length > 2) ? array[2].Trim() : string.Empty);
			string item2 = ((array.Length > 2) ? NormalizeGameMode(gameMode) : string.Empty);
			return Tuple.Create(item, item2);
		}

		public static void Command_ChangeMap(Player? callerPlayer, string args)
		{
			Tuple<string, string> tuple = ArgsToMapAndGamemode(args);
			if (tuple == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", "Too few arguments.");
				return;
			}
			string item = tuple.Item1;
			string text = tuple.Item2;
			if (!IsMapNameValid(item))
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", "Invalid map name.");
				return;
			}
			if (string.IsNullOrEmpty(text))
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", "Gamemode " + text + " cannot be found or empty. Using highest priority gamemode");
				text = GetGameModeInfo(item)?.ObjectName;
			}
			if (text != null && IsMapCycleValid())
			{
				if (HelperMethods.IsTimerActive(Timer_FinalPostVoteDelay))
				{
					Timer_FinalPostVoteDelay = HelperMethods.Timer_Inactive;
					MelonLogger.Warning("Admin changed map while final RTV timer was in progress. Forcing timer to expire.");
				}
				if (HelperMethods.IsTimerActive(Timer_InitialPostVoteDelay))
				{
					Timer_InitialPostVoteDelay = HelperMethods.Timer_Inactive;
					MelonLogger.Warning("Admin changed map while initial RTV timer was in progress. Forcing timer to expire.");
				}
				HelperMethods.AlertAdminAction(callerPlayer, "changing map to " + GetDisplayName(item) + "...");
				MelonLogger.Msg("Changing map to " + item + " with Gamemode " + text + "...");
				QueueChangeMap(item, fromRTV: true, text);
			}
		}

		public static void QueueChangeMap(string mapName, bool fromRTV = false, string gamemodertv = "MP_Strategy")
		{
			if (!IsMapCycleValid())
			{
				return;
			}
			LevelInfo levelInfo = GetLevelInfo(mapName);
			if (levelInfo == null)
			{
				MelonLogger.Error("Could not find LevelInfo for map name: " + mapName);
				return;
			}
			GameModeInfo gameModeInfo = null;
			if (fromRTV)
			{
				gameModeInfo = GameModeInfo.GetByName(gamemodertv);
				HelperMethods.SendChatMessageToAll($"Changing to Winning map: {mapName} with Gamemode {gameModeInfo}.");
			}
			else
			{
				iMapLoadCount = (iMapLoadCount + 1) % mapCycleEntries.Count;
				MelonLogger.Msg($"Post-RTV: Resuming map cycle from index {iMapLoadCount}, Map: {mapCycleEntries[iMapLoadCount].MapName}");
				gameModeInfo = GameModeInfo.GetByName(mapCycleEntries[iMapLoadCount].GameMode);
				if (gameModeInfo == null)
				{
					gameModeInfo = GetGameModeInfo(mapName);
				}
				MelonLogger.Msg("Queueing next map: " + mapCycleEntries[iMapLoadCount % mapCycleEntries.Count]?.MapName);
			}
			FieldInfo field = typeof(NetworkGameServer).GetField("m_QueueMap", BindingFlags.Instance | BindingFlags.NonPublic);
			typeof(NetworkGameServer).GetField("m_QueueGameMode", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(NetworkGameServer.Instance, gameModeInfo);
			field.SetValue(NetworkGameServer.Instance, levelInfo?.FileName);
			MelonLogger.Msg("[NET Framework] Queued map: " + levelInfo?.FileName + " with game mode: " + gameModeInfo?.ObjectName);
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			rockers.Clear();
			mapNominations.Clear();
			roundsOnSameMap = 0;
			mapName = sceneName;
			switch (sceneName)
			{
			case "MainMenu":
				return;
			case "Loading":
				return;
			}
			_ = sceneName.Length;
			_ = 2;
		}

		public override void OnUpdate()
		{
			try
			{
				if (HelperMethods.IsTimerActive(Timer_EndRoundDelay))
				{
					Timer_EndRoundDelay += Time.deltaTime;
					if (Timer_EndRoundDelay > (float)Pref_Mapcycle_EndgameDelay.Value)
					{
						Timer_EndRoundDelay = HelperMethods.Timer_Inactive;
						string nextMap = GetNextMap();
						MelonLogger.Msg("Changing map to " + nextMap + " .....");
						QueueChangeMap(nextMap);
						return;
					}
				}
				if (HelperMethods.IsTimerActive(Timer_InitialPostVoteDelay))
				{
					Timer_InitialPostVoteDelay += Time.deltaTime;
					if (Timer_InitialPostVoteDelay > 2f)
					{
						Timer_InitialPostVoteDelay = HelperMethods.Timer_Inactive;
						HelperMethods.SendChatMessageToAll("Rock the vote finished.");
						if (rockthevoteWinningMap == "")
						{
							HelperMethods.SendChatMessageToAll("Staying on current map.");
						}
						else
						{
							HelperMethods.SendChatMessageToAll("Preparing to change map to " + GetDisplayName(rockthevoteWinningMap) + " ...");
							HelperMethods.StartTimer(ref Timer_FinalPostVoteDelay);
						}
						return;
					}
				}
				if (HelperMethods.IsTimerActive(Timer_FinalPostVoteDelay))
				{
					Timer_FinalPostVoteDelay += Time.deltaTime;
					if (Timer_FinalPostVoteDelay > 6f)
					{
						Timer_FinalPostVoteDelay = HelperMethods.Timer_Inactive;
						MelonLogger.Msg("Changing map to " + rockthevoteWinningMap + " ...");
						QueueChangeMap(rockthevoteWinningMap);
					}
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run OnUpdate");
			}
		}

		private static GameModeInfo? GetGameModeInfo(string mapName)
		{
			LevelInfo levelInfo = GetLevelInfo(mapName);
			if (levelInfo == null)
			{
				return null;
			}
			int num = -1;
			GameModeInfo result = null;
			foreach (GameModeInfo gameMode in levelInfo.GameModes)
			{
				if (!(gameMode == null) && gameMode.Enabled && num < gameMode.Priority)
				{
					num = gameMode.Priority;
					result = gameMode;
				}
			}
			return result;
		}

		private static bool IsMapNameValid(string mapName)
		{
			if (GetLevelInfo(mapName) == null)
			{
				MelonLogger.Warning("IsMapNameValid: levelinfo is null.");
				return false;
			}
			return true;
		}

		private static LevelInfo? GetLevelInfo(string mapName)
		{
			if (GameDatabase.Database == null || GameDatabase.Database.AllLevels == null)
			{
				MelonLogger.Warning("Found game database null.");
				return null;
			}
			foreach (LevelInfo allLevel in GameDatabase.Database.AllLevels)
			{
				if (!(allLevel == null) && allLevel.Enabled && allLevel.IsMultiplayer && string.Equals(mapName, allLevel.FileName, StringComparison.OrdinalIgnoreCase))
				{
					return allLevel;
				}
			}
			return null;
		}

		private static string? GetDisplayName(string? mapName)
		{
			string text;
			if (mapName != null)
			{
				text = GetLevelInfo(mapName)?.DisplayName;
				if (text == null)
				{
					return mapName;
				}
			}
			else
			{
				text = string.Empty;
			}
			return text;
		}

		private static void CreateDefaultMapcycle(string mapCycleFile)
		{
			using FileStream fileStream = File.Create(mapCycleFile);
			fileStream.Close();
			File.WriteAllText(mapCycleFile, "RiftBasin MP_Strategy\nNorthPolarCap MP_Strategy\nCrimsonPeak MP_Strategy\nTheMaw MP_Strategy\nGreatErg MP_Strategy\nBadlands MP_Strategy\nNarakaCity MP_Strategy\n");
		}

		private static void ParseMapcycle(string mapCycleFile)
		{
			try
			{
				using StreamReader streamReader = File.OpenText(mapCycleFile);
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					string[] array = text.Split(new char[1] { ' ' }, 2);
					string text2 = array[0].Trim();
					string text3 = ((array.Length > 1) ? NormalizeGameMode(array[1].Trim()) : "");
					if (text3 == string.Empty)
					{
						GameModeInfo gameModeInfo = GetGameModeInfo(text2);
						if (gameModeInfo != null)
						{
							text3 = gameModeInfo.ObjectName;
							mapCycleEntries.Add(new VotingOption(text2, text3 ?? string.Empty));
						}
						else
						{
							MelonLogger.Warning("GameMode is null for map " + text2 + ".");
						}
					}
					else
					{
						mapCycleEntries.Add(new VotingOption(text2, text3 ?? string.Empty));
					}
				}
				MelonLogger.Msg("Current Mapcycle:");
				foreach (VotingOption mapCycleEntry in mapCycleEntries)
				{
					MelonLogger.Msg("Map: " + mapCycleEntry.MapName + ", Game Mode: " + (string.IsNullOrWhiteSpace(mapCycleEntry.GameMode) ? "Default" : mapCycleEntry.GameMode));
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Error(ex.ToString());
			}
		}
	}
	public class VotingOption
	{
		public string MapName { get; set; }

		public string GameMode { get; set; }

		public VotingOption(string mapName, string gameMode)
		{
			MapName = mapName;
			GameMode = gameMode;
		}
	}
}
