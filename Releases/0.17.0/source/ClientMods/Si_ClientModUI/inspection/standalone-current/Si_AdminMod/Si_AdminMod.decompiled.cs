using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using DebugTools;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using MelonLoader.Utils;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using SilicaAdminMod;
using Tomlet;
using Tomlet.Exceptions;
using Tomlet.Models;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(SiAdminMod), "Admin Mod", "2.14.18", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonColor(255, 0, 255, 255)]
[assembly: MelonAuthorColor(255, 0, 128, 0)]
[assembly: MelonPriority(-100)]
[assembly: MelonPlatform(/*Could not decode attribute arguments.*/)]
[assembly: VerifyLoaderVersion(0, 6, 1, true)]
[assembly: MelonPlatformDomain(/*Could not decode attribute arguments.*/)]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("databomb")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCopyright("Copyright (C) 2023-2025 by databomb")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+17f1aa45fa1403d6b033f5c4b3839f68b78e8f34")]
[assembly: AssemblyProduct("Si_AdminMod")]
[assembly: AssemblyTitle("Si_AdminMod")]
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
namespace SilicaAdminMod
{
	public class AdminCommand
	{
		private string _admincommandtext;

		private string _admindescriptiontext;

		private HelperMethods.CommandCallback _commandCallback;

		public string AdminCommandText
		{
			get
			{
				return _admincommandtext;
			}
			set
			{
				_admincommandtext = value ?? throw new ArgumentNullException("Command name is required.");
			}
		}

		public HelperMethods.CommandCallback AdminCallback
		{
			get
			{
				return _commandCallback;
			}
			set
			{
				_commandCallback = value ?? throw new ArgumentNullException("Command callback is required.");
			}
		}

		public Power AdminPower { get; set; }

		public string AdminCommandDescription
		{
			get
			{
				return _admindescriptiontext;
			}
			set
			{
				_admindescriptiontext = value ?? "No description provided.";
			}
		}
	}
	public class AdminFile
	{
		private static readonly string adminFilePath = Path.Combine(MelonEnvironment.UserDataDirectory, "admins.json");

		public static void Update(List<Admin> theAdminList)
		{
			string contents = JsonConvert.SerializeObject(theAdminList, Formatting.Indented);
			File.WriteAllText(adminFilePath, contents);
		}

		public static List<Admin> Initialize()
		{
			if (!File.Exists(adminFilePath))
			{
				MelonLogger.Warning("Did not find admins.json file. No admin entries loaded.");
				return new List<Admin>();
			}
			StreamReader streamReader = File.OpenText(adminFilePath);
			using (streamReader)
			{
				string text = streamReader.ReadToEnd();
				if (text == null)
				{
					MelonLogger.Warning("The admins.json read as empty. No admins loaded.");
					return new List<Admin>();
				}
				List<Admin> list = JsonConvert.DeserializeObject<List<Admin>>(text);
				if (list == null)
				{
					MelonLogger.Warning("Encountered deserialization error in admins.json file. Ensure file is in valid format (e.g., https://jsonlint.com/)");
					return new List<Admin>();
				}
				MelonLogger.Msg("Loaded admins.json with " + list.Count + " admin entries.");
				return list;
			}
		}
	}
	public static class AdminMethods
	{
		public static List<AdminCommand> AdminCommands;

		public static void RegisterAdminCommand(string adminCommand, HelperMethods.CommandCallback adminCallback, Power adminPower, string? adminDescription = null)
		{
			AdminCommand adminCommand2 = new AdminCommand
			{
				AdminCommandText = adminCommand,
				AdminCallback = adminCallback,
				AdminPower = adminPower
			};
			if (adminDescription != null)
			{
				adminCommand2.AdminCommandDescription = adminDescription;
			}
			AdminCommands.Add(adminCommand2);
			FieldInfo field = typeof(DebugConsole).GetField("s_Commands", BindingFlags.Static | BindingFlags.NonPublic);
			string text = "sam_" + adminCommand2.AdminCommandText;
			DebugConsole.ICommand command = (DebugConsole.ICommand)Activator.CreateInstance(typeof(CSAM_ConsoleCommand), text, adminCommand2.AdminCommandDescription, EAdminLevel.STANDARD);
			if (command != null)
			{
				Dictionary<string, DebugConsole.ICommand> dictionary = (Dictionary<string, DebugConsole.ICommand>)field.GetValue(null);
				if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
				{
					MelonLogger.Msg(command.Key.ToLower() + " console command registered.");
				}
				dictionary.Add(command.Key.ToLower(), command);
				field.SetValue(null, dictionary);
			}
		}

		public static bool UnregisterAdminCommand(string adminCommand)
		{
			AdminCommand adminCommand2 = FindAdminCommandFromString(adminCommand);
			if (adminCommand2 == null)
			{
				return false;
			}
			return AdminCommands.Remove(adminCommand2);
		}

		public static Admin? FindAdminFromSteamId(long steamId, bool modAdminOnly = false)
		{
			foreach (Admin admin in SiAdminMod.AdminList)
			{
				if (admin.SteamId == steamId)
				{
					return admin;
				}
			}
			if (modAdminOnly)
			{
				return null;
			}
			NetworkAdminPlayer playerAdmin = NetworkServerSettings.GetPlayerAdmin(new NetworkID((ulong)steamId));
			if (playerAdmin != null)
			{
				return new Admin
				{
					SteamId = steamId,
					Level = (byte)(playerAdmin.m_Root ? 100 : 10),
					Name = playerAdmin.m_Name,
					CreatedOn = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
					LastModifiedOn = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
					Powers = (playerAdmin.m_Root ? Power.Root : ((Power)56676))
				};
			}
			return null;
		}

		public static AdminCommand? FindAdminCommandFromString(string commandText)
		{
			foreach (AdminCommand adminCommand in AdminCommands)
			{
				if (string.Equals(adminCommand.AdminCommandText, commandText, StringComparison.OrdinalIgnoreCase))
				{
					return adminCommand;
				}
			}
			return null;
		}

		public static bool PowerInPowers(Power power, Power powers)
		{
			if ((powers & (power | Power.Root)) != 0)
			{
				return true;
			}
			return false;
		}
	}
	public enum Power
	{
		None = 0,
		Slot = 1,
		Vote = 2,
		Kick = 4,
		Ban = 8,
		Unban = 0x10,
		Slay = 0x20,
		Map = 0x40,
		Cheat = 0x80,
		Commander = 0x100,
		Skip = 0x200,
		End = 0x400,
		Eject = 0x800,
		Mute = 0x1000,
		MuteForever = 0x2000,
		Generic = 0x4000,
		Teams = 0x8000,
		Custom1 = 0x10000,
		Custom2 = 0x20000,
		Custom3 = 0x40000,
		Custom4 = 0x80000,
		Reserved1 = 0x200000,
		Reserved2 = 0x400000,
		Reserved3 = 0x800000,
		Reserved4 = 0x1000000,
		Rcon = 0x2000000,
		Root = 0x4000000
	}
	public class Admin
	{
		public string? Name { get; set; }

		public long SteamId { get; set; }

		public Power Powers { get; set; }

		public byte Level { get; set; }

		public int CreatedOn { get; set; }

		public int LastModifiedOn { get; set; }
	}
	public class ChatVoteBallot
	{
		private string _question;

		private ChatVotes.VoteHandler _voteHandler;

		private OptionPair[] _options;

		public string Question
		{
			get
			{
				return _question;
			}
			set
			{
				_question = value ?? throw new ArgumentNullException("Question is required.");
			}
		}

		public ChatVotes.VoteHandler VoteHandler
		{
			get
			{
				return _voteHandler;
			}
			set
			{
				_voteHandler = value ?? throw new ArgumentNullException("Vote handler is required.");
			}
		}

		public OptionPair[] Options
		{
			get
			{
				return _options;
			}
			set
			{
				_options = value ?? throw new ArgumentNullException("Options are required.");
			}
		}
	}
	public class OptionPair
	{
		private string _command;

		private string _description;

		public string Command
		{
			get
			{
				return _command;
			}
			set
			{
				_command = value ?? throw new ArgumentNullException("Command is required.");
			}
		}

		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				_description = value ?? throw new ArgumentNullException("Description is required.");
			}
		}
	}
	public class ChatVoteResults
	{
		private string _winningCommand;

		private OptionVoteResult[] _detailedResults;

		private ChatVotes.VoteHandler _voteHandler;

		public string WinningCommand
		{
			get
			{
				return _winningCommand;
			}
			set
			{
				_winningCommand = value;
			}
		}

		public OptionVoteResult[] DetailedResults
		{
			get
			{
				return _detailedResults;
			}
			set
			{
				_detailedResults = value;
			}
		}

		public ChatVotes.VoteHandler VoteHandler
		{
			get
			{
				return _voteHandler;
			}
			set
			{
				_voteHandler = value;
			}
		}
	}
	public class OptionVoteResult
	{
		private string _command;

		public string Command
		{
			get
			{
				return _command;
			}
			set
			{
				_command = value ?? throw new ArgumentNullException("Command is required.");
			}
		}

		public int Votes { get; set; }
	}
	public static class ChatVotes
	{
		public delegate void VoteHandler(ChatVoteResults results);

		public static bool voteInProgress = false;

		public static ChatVoteResults currentVoteResults = null;

		private static HelperMethods.CommandCallback voteChatCallback = Command_VoteChat;

		private static List<Player> voters = null;

		public static void Command_VoteChat(Player? callerPlayer, string args)
		{
			if (currentVoteResults == null || voters == null)
			{
				MelonLogger.Warning("Current vote results unavailable.");
				return;
			}
			if (callerPlayer == null)
			{
				MelonLogger.Warning("Console not supported.");
				return;
			}
			if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
			{
				MelonLogger.Msg("Reached vote callback for: " + args);
			}
			OptionVoteResult[] detailedResults = currentVoteResults.DetailedResults;
			foreach (OptionVoteResult optionVoteResult in detailedResults)
			{
				if (optionVoteResult.Command == args)
				{
					if (voters.Contains(callerPlayer))
					{
						HelperMethods.SendChatMessageToPlayer(callerPlayer, "Vote already cast.");
						break;
					}
					optionVoteResult.Votes++;
					voters.Add(callerPlayer);
					HelperMethods.SendChatMessageToPlayer(callerPlayer, "Vote cast for " + args);
					break;
				}
			}
		}

		public static bool IsVoteInProgress()
		{
			return voteInProgress;
		}

		public static void HoldVote(ChatVoteBallot ballot)
		{
			if (IsVoteInProgress())
			{
				throw new InvalidOperationException("Cannot start a new vote when one is in progress.");
			}
			HelperMethods.ReplyToCommand(ballot.Question);
			voters = new List<Player>();
			currentVoteResults = new ChatVoteResults
			{
				DetailedResults = new OptionVoteResult[ballot.Options.Length],
				VoteHandler = ballot.VoteHandler
			};
			int num = 0;
			OptionPair[] options = ballot.Options;
			foreach (OptionPair optionPair in options)
			{
				OptionVoteResult optionVoteResult = new OptionVoteResult
				{
					Command = optionPair.Command,
					Votes = 0
				};
				currentVoteResults.DetailedResults[num] = optionVoteResult;
				num++;
				PlayerMethods.RegisterPlayerPhrase(optionPair.Command, voteChatCallback, hideFromChat: true);
				HelperMethods.ReplyToCommand(optionPair.Command, " -- ", optionPair.Description);
			}
			StartVoteDurationTimer();
		}

		private static void StartVoteDurationTimer()
		{
			if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
			{
				MelonLogger.Msg("Starting vote timer with duration " + SiAdminMod.Pre_Admin_VoteDuration.Value);
			}
			voteInProgress = true;
			HelperMethods.StartTimer(ref SiAdminMod.Timer_TallyVote);
		}
	}
	public class SiAdminMod : MelonMod
	{
		[HarmonyPatch(typeof(MusicJukeboxHandler), "OnGameStarted")]
		private static class ApplyPatch_MusicJukeboxHandler_OnGameStarted
		{
			public static void Prefix(MusicJukeboxHandler __instance, GameMode __0)
			{
				try
				{
					string text = UnityEngine.Object.FindFirstObjectByType<GameModeExt>().TeamsVersus.ToString();
					if (!Directory.Exists(GetGameTypeConfigFileFullDirectory()))
					{
						MelonLogger.Msg("Creating gametype config file directory at: " + GetMapConfigFileFullDirectory());
						Directory.CreateDirectory(GetGameTypeConfigFileFullDirectory());
					}
					string gameTypeConfigFilePath = GetGameTypeConfigFilePath(text);
					if (!File.Exists(gameTypeConfigFilePath))
					{
						MelonLogger.Msg("Map config file [" + GetGameTypeConfigFileSubDirectory() + text + ".cfg] not found. Skipping.");
						return;
					}
					LoadConfigEntries(TomlParser.ParseFile(gameTypeConfigFilePath));
					MelonLogger.Msg("MelonPreferences Loaded from " + gameTypeConfigFilePath);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run OnGameStarted");
				}
			}
		}

		[HarmonyPatch(typeof(GameMode), "OnPlayerJoinedBase")]
		private static class ApplyPatch_GameMode_OnPlayerJoinedBase
		{
			public static void Postfix(GameMode __instance, Player __0)
			{
				try
				{
					if (!(__0 == null))
					{
						if (!bannerGenerated)
						{
							welcomeBanner = GenerateWelcomeBanner(__0);
							bannerGenerated = true;
						}
						HelperMethods.SendConsoleMessageToPlayer(__0, welcomeBanner);
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run GameMode::OnPlayerJoinedBase");
				}
			}
		}

		public static float Timer_TallyVote = HelperMethods.Timer_Inactive;

		public static MelonPreferences_Category _modCategory = null;

		public static MelonPreferences_Entry<bool> Pref_Admin_AcceptTeamChatCommands = null;

		public static MelonPreferences_Entry<int> Pre_Admin_VoteDuration = null;

		public static MelonPreferences_Entry<bool> Pref_Admin_StopNonAdminCheats = null;

		public static MelonPreferences_Entry<bool> Pref_Admin_ReplicateCheatsForPasswordedServers = null;

		public static MelonPreferences_Entry<bool> Pref_Admin_DebugLogMessages = null;

		public static List<Admin> AdminList = null;

		private static string[] welcomeBanner = Array.Empty<string>();

		private static bool bannerGenerated = false;

		public override void OnUpdate()
		{
			try
			{
				if (!HelperMethods.IsTimerActive(Timer_TallyVote))
				{
					return;
				}
				Timer_TallyVote += Time.deltaTime;
				if (!(Timer_TallyVote >= (float)Pre_Admin_VoteDuration.Value))
				{
					return;
				}
				Timer_TallyVote = HelperMethods.Timer_Inactive;
				if (ChatVotes.currentVoteResults == null)
				{
					MelonLogger.Warning("Current vote results unavailable for timer expiration.");
					ChatVotes.voteInProgress = false;
					return;
				}
				OptionVoteResult optionVoteResult = new OptionVoteResult
				{
					Votes = -1,
					Command = "invalid"
				};
				OptionVoteResult[] detailedResults = ChatVotes.currentVoteResults.DetailedResults;
				foreach (OptionVoteResult optionVoteResult2 in detailedResults)
				{
					if (optionVoteResult2.Votes > optionVoteResult.Votes)
					{
						optionVoteResult = optionVoteResult2;
					}
					if (Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Found command " + optionVoteResult2.Command + " with votes " + optionVoteResult2.Votes);
					}
					PlayerMethods.UnregisterPlayerPhrase(optionVoteResult2.Command);
				}
				ChatVotes.currentVoteResults.WinningCommand = optionVoteResult.Command;
				ChatVotes.voteInProgress = false;
				ChatVotes.currentVoteResults.VoteHandler(ChatVotes.currentVoteResults);
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed in OnUpdate");
			}
		}

		private static void LoadConfigEntries(TomlDocument configToml)
		{
			if (MelonPreferences.Categories.Count <= 0)
			{
				return;
			}
			foreach (MelonPreferences_Category category in MelonPreferences.Categories)
			{
				if (category.Entries.Count <= 0)
				{
					continue;
				}
				foreach (MelonPreferences_Entry entry in category.Entries)
				{
					SetupEntryFromRawValue(configToml, entry);
				}
			}
		}

		private static string QuoteKey(string key)
		{
			if (!key.Contains('"'))
			{
				return "\"" + key + "\"";
			}
			return "'" + key + "'";
		}

		public static void SetupEntryFromRawValue(TomlDocument config, MelonPreferences_Entry entry)
		{
			lock (config)
			{
				try
				{
					TomlValue value = ((TomlTable)config).GetSubTable(entry.Category.Identifier).GetValue(QuoteKey(entry.Identifier));
					entry.Load(value);
				}
				catch (TomlTypeMismatchException)
				{
				}
				catch (TomlNoSuchValueException)
				{
				}
			}
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			try
			{
				if (IsMapNameValid(sceneName))
				{
					if (!Directory.Exists(GetMapConfigFileFullDirectory()))
					{
						MelonLogger.Msg("Creating map config file directory at: " + GetMapConfigFileFullDirectory());
						Directory.CreateDirectory(GetMapConfigFileFullDirectory());
					}
					string mapConfigFilePath = GetMapConfigFilePath(sceneName);
					if (!File.Exists(mapConfigFilePath))
					{
						MelonLogger.Msg("Map config file [" + GetMapConfigFileSubDirectory() + sceneName + ".cfg] not found. Skipping.");
						return;
					}
					LoadConfigEntries(TomlParser.ParseFile(mapConfigFilePath));
					MelonLogger.Msg("MelonPreferences Loaded from " + mapConfigFilePath);
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run map config file.");
			}
		}

		public static bool IsMapNameValid(string sceneName)
		{
			switch (sceneName)
			{
			default:
				if (sceneName.Length >= 2)
				{
					return true;
				}
				goto case "Intro";
			case "Intro":
			case "MainMenu":
			case "Loading":
				return false;
			}
		}

		public static string GetMapConfigFileSubDirectory()
		{
			return "cfg\\maps\\";
		}

		public static string GetMapConfigFileFullDirectory()
		{
			return Path.Combine(MelonEnvironment.UserDataDirectory, GetMapConfigFileSubDirectory());
		}

		public static string GetMapConfigFilePath(string sceneName)
		{
			string path = sceneName + ".cfg";
			return Path.Combine(GetMapConfigFileFullDirectory(), path);
		}

		public static string GetGameTypeConfigFileSubDirectory()
		{
			return "cfg\\gametype\\";
		}

		public static string GetGameTypeConfigFileFullDirectory()
		{
			return Path.Combine(MelonEnvironment.UserDataDirectory, GetGameTypeConfigFileSubDirectory());
		}

		public static string GetGameTypeConfigFilePath(string gameTypeName)
		{
			string path = gameTypeName + ".cfg";
			return Path.Combine(GetGameTypeConfigFileSubDirectory(), path);
		}

		public override void OnInitializeMelon()
		{
			try
			{
				AdminMethods.AdminCommands = new List<AdminCommand>();
				PlayerMethods.PlayerCommands = new List<PlayerCommand>();
				PlayerMethods.PlayerPhrases = new List<PlayerCommand>();
				AdminList = AdminFile.Initialize();
				if (_modCategory == null)
				{
					_modCategory = MelonPreferences.CreateCategory("Silica");
				}
				if (Pref_Admin_AcceptTeamChatCommands == null)
				{
					Pref_Admin_AcceptTeamChatCommands = _modCategory.CreateEntry<bool>("Admin_AllowTeamChatCommands", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pre_Admin_VoteDuration == null)
				{
					Pre_Admin_VoteDuration = _modCategory.CreateEntry<int>("Admin_VoteDuration_Seconds", 30, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Admin_StopNonAdminCheats == null)
				{
					Pref_Admin_StopNonAdminCheats = _modCategory.CreateEntry<bool>("Admin_PreventNonAdminCheats", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Admin_ReplicateCheatsForPasswordedServers == null)
				{
					Pref_Admin_ReplicateCheatsForPasswordedServers = _modCategory.CreateEntry<bool>("Admin_ReplicateCheatsForPrivateServers", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Admin_DebugLogMessages == null)
				{
					Pref_Admin_DebugLogMessages = _modCategory.CreateEntry<bool>("Admin_EnableDebugLogging", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				MelonLogger.Msg("Registering host console commands...");
				FieldInfo field = typeof(DebugConsole).GetField("s_Commands", BindingFlags.Static | BindingFlags.NonPublic);
				Dictionary<string, DebugConsole.ICommand> dictionary = (Dictionary<string, DebugConsole.ICommand>)field.GetValue(null);
				DebugConsole.ICommand command = (DebugConsole.ICommand)Activator.CreateInstance(typeof(CSAM_AddAdmin));
				if (command != null)
				{
					if (Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg(command.Key.ToLower() + " registered.");
					}
					dictionary.Add(command.Key.ToLower(), command);
				}
				DebugConsole.ICommand command2 = (DebugConsole.ICommand)Activator.CreateInstance(typeof(CSAM_Say));
				if (command2 != null)
				{
					if (Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg(command2.Key.ToLower() + " registered.");
					}
					dictionary.Add(command2.Key.ToLower(), command2);
				}
				DebugConsole.ICommand command3 = (DebugConsole.ICommand)Activator.CreateInstance(typeof(CSAM_Cvar));
				if (command3 != null)
				{
					if (Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg(command3.Key.ToLower() + " registered.");
					}
					dictionary.Add(command3.Key.ToLower(), command3);
				}
				field.SetValue(null, dictionary);
				Event_Chat.OnRequestPlayerChat += ClientChatHandler.OnRequestPlayerChat;
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to load admins");
			}
		}

		public static string[] GenerateWelcomeBanner(Player player)
		{
			List<string> list = new List<string>();
			list.Add(Divider());
			list.Add(CommandsHeader());
			int num = 1;
			foreach (PlayerCommand playerCommand in PlayerMethods.PlayerCommands)
			{
				list.Add("<color=#FFFFFF>" + num + ". <indent=6.3em>!" + playerCommand.CommandName + "</indent></color>");
				num++;
			}
			foreach (PlayerCommand playerPhrase in PlayerMethods.PlayerPhrases)
			{
				list.Add("<color=#FFFFFF>" + num + ". <indent=6.2em>" + playerPhrase.CommandName + "</indent></color>");
				num++;
			}
			list.Add(CommandsFooter());
			return list.ToArray();
		}

		public static string Divider()
		{
			return " ";
		}

		public static string CommandsHeader()
		{
			return "<mspace=0.66em><color=#FFFFFF>------------- <i>Player Commands</i> -------------</color></mspace>";
		}

		public static string CommandsFooter()
		{
			return "<mspace=0.66em><color=#FFFFFF>-------------------------------------------</color></mspace>";
		}
	}
	public class CheatsEventLimiter
	{
		[HarmonyPatch(/*Could not decode attribute arguments.*/)]
		private static class ApplyPatch_Game_CheatsEnabled_Setter_Pre
		{
			public static bool Prefix(bool __0)
			{
				try
				{
					if (!__0)
					{
						return true;
					}
					if (!SiAdminMod.Pref_Admin_StopNonAdminCheats.Value)
					{
						return true;
					}
					typeof(Game).GetField("m_CheatsEnabled", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, true);
					SendLimitedCheatsEvent(cheatsStatus: true);
					return false;
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run Game::CheatsEnabled::Setter(Pre)");
				}
				return true;
			}
		}

		public static void SendLimitedCheatsEvent(bool cheatsStatus)
		{
			GameByteStreamWriter gameByteStreamWriter = GameByteStreamWriter.GetGameByteStreamWriter(0u, "NetworkLayer::SendLimitedCheatsEvent");
			gameByteStreamWriter.WriteByte(26);
			gameByteStreamWriter.WriteBool(cheatsStatus);
			gameByteStreamWriter.GetByteData();
			uint byteDataSize = (uint)gameByteStreamWriter.GetByteDataSize();
			for (int i = 0; i < Player.Players.Count; i++)
			{
				Player player = Player.Players[i];
				if (player == null || player == NetworkGameServer.GetServerPlayer())
				{
					continue;
				}
				Power adminPowers = player.GetAdminPowers();
				if ((int)player.AdminLevel >= 1 || AdminMethods.PowerInPowers(Power.Cheat, adminPowers))
				{
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Permitting cheats access to admin: " + player.PlayerName + " (server level: " + player.AdminLevel.ToString() + ") (mod powers: " + adminPowers.ToString() + ")");
					}
					FieldInfo field = typeof(NetworkLayer).GetField("NetBitsSent", BindingFlags.Static | BindingFlags.NonPublic);
					uint num = (uint)field.GetValue(null);
					num += byteDataSize * 8;
					field.SetValue(null, num);
					typeof(NetworkLayer).GetMethod("SendServerPacket", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[5]
					{
						player.PlayerID,
						gameByteStreamWriter.GetByteData(),
						byteDataSize,
						ENetworkPacketSend.Reliable,
						player.PlayerChannel
					});
				}
			}
		}
	}
	public class CheatsNotification
	{
		[HarmonyPatch(/*Could not decode attribute arguments.*/)]
		private static class ApplyPatch_Game_CheatsEnabled_Setter
		{
			public static void Postfix(bool __0)
			{
				try
				{
					if (!NetworkGameServer.GetServerPasswordProtected() || SiAdminMod.Pref_Admin_ReplicateCheatsForPasswordedServers.Value)
					{
						string name = new StackFrame(2, fNeedFileInfo: true).GetMethod().Name;
						if (!string.Equals(name, "OnEnable") && !string.Equals(name, "ClearAll"))
						{
							HelperMethods.AlertAdminAction(null, (Game.CheatsEnabled ? "ENABLED CHEATS" : "disabled cheats") + ((Game.CheatsEnabled && SiAdminMod.Pref_Admin_StopNonAdminCheats.Value) ? " (admins only)" : ""));
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run Game::CheatsEnabled::Setter");
				}
			}
		}
	}
	public class CSAM_AddAdmin : DebugConsole.ICommand
	{
		public string Key => "sam_addadmin";

		public string Description => "Allows a host to add an admin to SAM from the console";

		public EAdminLevel RequiredAdminLevel => EAdminLevel.HOST;

		public bool ServerSide => true;

		public void Execute(params string[] parameters)
		{
			if (parameters == null || parameters.Length < 3)
			{
				DebugConsole.Log(Key + " usage: <player> <powers> <level>");
				return;
			}
			if (parameters.Length > 3)
			{
				DebugConsole.Log(Key + " usage: <player> <powers> <level>");
				return;
			}
			Player player = HelperMethods.FindTargetPlayer(parameters[0]);
			if (player == null)
			{
				DebugConsole.Log(Key + ": Ambiguous or invalid target");
				return;
			}
			string text = parameters[1];
			if (text.Any(char.IsDigit))
			{
				DebugConsole.Log(Key + ": Powers invalid");
				return;
			}
			string text2 = parameters[2];
			int num = int.Parse(text2);
			if (num < 0)
			{
				DebugConsole.Log(Key + ": Level too low");
				return;
			}
			if (num > 255)
			{
				DebugConsole.Log(Key + ": Level too high");
				return;
			}
			if (HostAction.AddAdmin(player, text, (byte)num))
			{
				DebugConsole.Log(Key + ": Added " + player.PlayerName + " as an admin (Level " + text2 + ")");
			}
			else
			{
				DebugConsole.Log(Key + ": " + player.PlayerName + " is already an admin");
			}
			DebugConsole.Log("");
		}
	}
	public class CSAM_Say : DebugConsole.ICommand
	{
		public string Key => "sam_say";

		public string Description => "Allows a host to send a SAM message from server console";

		public EAdminLevel RequiredAdminLevel => EAdminLevel.HOST;

		public bool ServerSide => true;

		public void Execute(params string[] parameters)
		{
			if (parameters == null)
			{
				DebugConsole.Log(Key + " usage: <message>");
				DebugConsole.Log("");
			}
			else
			{
				HelperMethods.AlertAdminAction(null, string.Join(" ", parameters));
			}
		}
	}
	public class CSAM_Cvar : DebugConsole.ICommand
	{
		public string Key => "sam_cvar";

		public string Description => "Allows a host to change a preference value without a restart";

		public EAdminLevel RequiredAdminLevel => EAdminLevel.HOST;

		public bool ServerSide => true;

		public void Execute(params string[] parameters)
		{
			if (parameters == null || parameters.Length < 1)
			{
				DebugConsole.Log(Key + " usage: <preference_string> [optional: new_value]");
				DebugConsole.Log("");
				return;
			}
			MelonPreferences_Entry entry = SiAdminMod._modCategory.GetEntry(parameters[0]);
			if (entry == null)
			{
				DebugConsole.Log("Could not find specified preference in the preference list: " + parameters[0], DebugConsole.LogLevel.Warning);
				DebugConsole.Log("");
			}
			else if (parameters.Length > 1)
			{
				Type reflectedType = entry.GetReflectedType();
				if (reflectedType == typeof(string))
				{
					entry.BoxedValue = string.Join(" ", parameters[1..]);
					DebugConsole.Log(parameters[0] + " changed to: " + entry.GetValueAsString());
					DebugConsole.Log("");
				}
				else if (reflectedType == typeof(bool))
				{
					if (bool.TryParse(parameters[1], out var result))
					{
						entry.BoxedValue = result;
						DebugConsole.Log(parameters[0] + " changed to: " + entry.GetValueAsString());
						DebugConsole.Log("");
						return;
					}
					DebugConsole.Log("Could not convert argument (" + parameters[1] + ") into preference type (" + reflectedType.ToString() + ")", DebugConsole.LogLevel.Warning);
					DebugConsole.Log("");
				}
				else if (reflectedType == typeof(int))
				{
					if (int.TryParse(parameters[1], out var result2))
					{
						entry.BoxedValue = result2;
						DebugConsole.Log(parameters[0] + " changed to: " + entry.GetValueAsString());
						DebugConsole.Log("");
						return;
					}
					DebugConsole.Log("Could not convert argument (" + parameters[1] + ") into preference type (" + reflectedType.ToString() + ")", DebugConsole.LogLevel.Warning);
					DebugConsole.Log("");
				}
				else if (reflectedType == typeof(float))
				{
					if (float.TryParse(parameters[1], out var result3))
					{
						entry.BoxedValue = result3;
						DebugConsole.Log(parameters[0] + " changed to: " + entry.GetValueAsString());
						DebugConsole.Log("");
						return;
					}
					DebugConsole.Log("Could not convert argument (" + parameters[1] + ") into preference type (" + reflectedType.ToString() + ")", DebugConsole.LogLevel.Warning);
					DebugConsole.Log("");
				}
				else
				{
					DebugConsole.Log("Could not handle preference type (" + reflectedType.ToString() + ")", DebugConsole.LogLevel.Warning);
					DebugConsole.Log("");
				}
			}
			else
			{
				DebugConsole.Log(parameters[0] + " is set to: " + entry.GetValueAsString());
				DebugConsole.Log("");
			}
		}
	}
	public class CSAM_ConsoleCommand : DebugConsole.ICommand
	{
		private string _key;

		private string _description;

		private EAdminLevel? _level;

		public string Key => _key;

		public string Description => _description;

		public EAdminLevel RequiredAdminLevel
		{
			get
			{
				if (!_level.HasValue)
				{
					return EAdminLevel.STANDARD;
				}
				return _level.Value;
			}
		}

		public bool ServerSide => true;

		public CSAM_ConsoleCommand(string keyName, string description, EAdminLevel level)
		{
			_key = keyName;
			_description = description;
			_level = level;
		}

		public void Execute(params string[] parameters)
		{
			string text = Key + " " + string.Join(" ", parameters);
			DebugConsole.Log("Processing command: " + text);
			AdminCommand adminCommand = AdminMethods.FindAdminCommandFromString(Key.Substring(4));
			if (adminCommand != null)
			{
				DebugConsole.Log("Valid admin command found. Running callback.");
				adminCommand.AdminCallback(null, text);
			}
			DebugConsole.Log("");
		}
	}
	[HarmonyPatch(typeof(DebugConsole), "TryExecuteCommand")]
	internal static class Patch_DebugConsole_TryExecuteCommand
	{
		public static bool Prefix(string __0, bool __1, Player __2)
		{
			try
			{
				if (__2 == null)
				{
					return true;
				}
				MelonLogger.Msg("Received remote command string: " + __0 + " from " + __2.PlayerName);
				if (IsValidConsoleCommand(__0))
				{
					string text = __0.Split(' ')[0];
					text = text.Substring(4);
					AdminCommand adminCommand = AdminMethods.FindAdminCommandFromString(text);
					if (adminCommand != null)
					{
						if (!__2.IsAdmin())
						{
							SendNetworkResponseWarning(__2, $"Caller \"{__2.PlayerName}\" is not an admin and cannot remotely execute \"{text}\"");
							return false;
						}
						Power adminPowers = __2.GetAdminPowers();
						if (!AdminMethods.PowerInPowers(adminCommand.AdminPower, adminPowers))
						{
							SendNetworkResponseWarning(__2, $"Caller \"{__2.PlayerName}\" does not have sufficient admin privileges to remotely execute \"{text}\"");
							return false;
						}
						adminCommand.AdminCallback(__2, __0);
						return false;
					}
					SendNetworkResponseWarning(__2, $"Caller \"{__2.PlayerName}\" has tried to use an invalid remote SAM command: \"{text}\"");
					return false;
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run DebugConsole::TryExecuteCommand");
			}
			return true;
		}

		private static void SendNetworkResponseWarning(Player callerPlayer, string responseLine)
		{
			DebugConsole.Log(responseLine, DebugConsole.LogLevel.Warning);
			List<string> commandResult = new List<string> { responseLine };
			List<DebugConsole.LogLevel> logLevel = new List<DebugConsole.LogLevel> { DebugConsole.LogLevel.Warning };
			NetworkLayer.SendRemoteCommandResult(callerPlayer, commandResult, logLevel);
		}

		private static bool IsValidConsoleCommand(string consoleCommand)
		{
			if (consoleCommand.Length < 5)
			{
				return false;
			}
			return consoleCommand.StartsWith("sam_");
		}
	}
	public static class SiConstants
	{
		public enum ETeam
		{
			Alien,
			Wildlife,
			Gamemaster,
			Centauri,
			Sol,
			Centauri_Allied,
			Sol_Allied,
			Alien_Allied
		}

		public const string SAM_AddAdmin_Usage = " usage: <player> <powers> <level>";

		public const int MaxPlayableTeams = 8;
	}
	public class OnRequestEnterUnitArgs : EventArgs
	{
		private Player _player;

		private Unit _unit;

		public Player Player
		{
			get
			{
				return _player;
			}
			set
			{
				_player = value ?? throw new ArgumentNullException("Player is required.");
			}
		}

		public Unit Unit
		{
			get
			{
				return _unit;
			}
			set
			{
				_unit = value ?? throw new ArgumentNullException("Unit is required.");
			}
		}

		public bool AsDriver { get; set; }

		public bool Block { get; set; }
	}
	public class OnRequestTeleportUnitArgs : EventArgs
	{
		private Unit _unit;

		private Structure _structure;

		public Unit Unit
		{
			get
			{
				return _unit;
			}
			set
			{
				_unit = value ?? throw new ArgumentNullException("Unit is required.");
			}
		}

		public Structure TeleportStructure
		{
			get
			{
				return _structure;
			}
			set
			{
				_structure = value ?? throw new ArgumentNullException("Structure is required.");
			}
		}

		public Vector3 TargetPosition { get; set; }

		public Vector3 EffectsPosition { get; set; }
	}
	public class OnRequestInviteToGroupArgs : EventArgs
	{
		private Player _player;

		private Target _target;

		public Player Player
		{
			get
			{
				return _player;
			}
			set
			{
				_player = value ?? throw new ArgumentNullException("Player is required.");
			}
		}

		public Target Target
		{
			get
			{
				return _target;
			}
			set
			{
				_target = value ?? throw new ArgumentNullException("Target is required.");
			}
		}

		public bool Block { get; set; }
	}
	public class OnCommanderSoldStructureArgs : EventArgs
	{
		private Structure _structure;

		private Team _team;

		public Structure Structure
		{
			get
			{
				return _structure;
			}
			set
			{
				_structure = value ?? throw new ArgumentNullException("Structure is required.");
			}
		}

		public Team Team
		{
			get
			{
				return _team;
			}
			set
			{
				_team = value ?? throw new ArgumentNullException("Team is required.");
			}
		}
	}
	public class OnRequestSellStructureArgs : EventArgs
	{
		private Structure _structure;

		private Team _team;

		public Structure Structure
		{
			get
			{
				return _structure;
			}
			set
			{
				_structure = value ?? throw new ArgumentNullException("Structure is required.");
			}
		}

		public Team Team
		{
			get
			{
				return _team;
			}
			set
			{
				_team = value ?? throw new ArgumentNullException("Team is required.");
			}
		}

		public bool GameDecision { get; set; }

		public bool Override { get; set; }
	}
	public class OnRequestBuildArgs : EventArgs
	{
		private ConstructionData _constructionData;

		private Structure _structure;

		public Structure ParentStructure
		{
			get
			{
				return _structure;
			}
			set
			{
				_structure = value ?? throw new ArgumentNullException("Parent Structure is required.");
			}
		}

		public ConstructionData ConstructionData
		{
			get
			{
				return _constructionData;
			}
			set
			{
				_constructionData = value ?? throw new ArgumentNullException("ConstructionData is required.");
			}
		}

		public Vector3 Position { get; set; }

		public Quaternion Rotation { get; set; }

		public bool PlayerInitiated { get; set; }

		public bool Block { get; set; }
	}
	public class OnPreDamageReceivedArgs : EventArgs
	{
		private DamageManager _damageManager;

		private GameObject _instigator;

		public DamageManager DamageManager
		{
			get
			{
				return _damageManager;
			}
			set
			{
				_damageManager = value ?? throw new ArgumentNullException("DamageManager is required.");
			}
		}

		public float Damage { get; set; }

		public GameObject Instigator
		{
			get
			{
				return _instigator;
			}
			set
			{
				_instigator = value ?? throw new ArgumentNullException("Instigator GameObject is required.");
			}
		}

		public byte HitAngle { get; set; }
	}
	public class OnRequestCommanderArgs : EventArgs
	{
		private Player _requester;

		public Player Requester
		{
			get
			{
				return _requester;
			}
			set
			{
				_requester = value ?? throw new ArgumentNullException("Requesting player is required.");
			}
		}

		public bool Block { get; set; }

		public bool PreventSpawnWhenBlocked { get; set; }
	}
	public class OnRoleChangedArgs : EventArgs
	{
		private Player _player;

		public Player Player
		{
			get
			{
				return _player;
			}
			set
			{
				_player = value ?? throw new ArgumentNullException("Player is required.");
			}
		}

		public GameModeExt.ETeamRole Role { get; set; }
	}
	public class OnRequestPlayerChatArgs : EventArgs
	{
		private Player _player;

		private string _text;

		public Player Player
		{
			get
			{
				return _player;
			}
			set
			{
				_player = value ?? throw new ArgumentNullException("Player is required.");
			}
		}

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value ?? throw new ArgumentNullException("Text cannot be empty.");
			}
		}

		public bool TeamOnly { get; set; }

		public bool Block { get; set; }
	}
	public static class Event_Chat
	{
		[HarmonyPatch(typeof(GameByteStreamReader), "GetGameByteStreamReader")]
		private static class ApplyPatch_GameByteStreamReader_Events
		{
			private static void Prefix(GameByteStreamReader __result, ref byte[] __0, int __1, uint __2, string __3, bool __4)
			{
				try
				{
					if (__1 < 12)
					{
						return;
					}
					GameByteStreamReader gameByteStreamReader = new GameByteStreamReader();
					typeof(GameByteStreamReader).GetField("m_ReadDebug", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(gameByteStreamReader, __4);
					gameByteStreamReader.Reset(__0, __1);
					if (gameByteStreamReader.ReadByte() != 11)
					{
						return;
					}
					NetworkID playerID = (NetworkID)gameByteStreamReader.ReadUInt64();
					int channel = gameByteStreamReader.ReadByte();
					string message = gameByteStreamReader.ReadString();
					bool teamOnly = gameByteStreamReader.ReadBool();
					Player player = Player.FindPlayer(playerID, channel);
					if (!player)
					{
						return;
					}
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Firing OnRequestPlayerChatEvent for player (size=" + __1 + "): " + player.PlayerName);
					}
					if (FireOnRequestPlayerChatEvent(player, message, teamOnly).Block)
					{
						if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
						{
							MelonLogger.Msg("Blocking chat message for player: " + player.PlayerName);
						}
						__0[1] = 127;
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run GameByteStreamReader::GetGameByteStreamReader");
				}
			}
		}

		public static event EventHandler<OnRequestPlayerChatArgs> OnRequestPlayerChat = delegate
		{
		};

		public static OnRequestPlayerChatArgs FireOnRequestPlayerChatEvent(Player player, string message, bool teamOnly)
		{
			OnRequestPlayerChatArgs onRequestPlayerChatArgs = new OnRequestPlayerChatArgs
			{
				Player = player,
				Text = message,
				TeamOnly = teamOnly,
				Block = false
			};
			OnRequestPlayerChat?.Invoke(null, onRequestPlayerChatArgs);
			return onRequestPlayerChatArgs;
		}
	}
	public static class Event_Construction
	{
		[HarmonyPatch(typeof(Structure), "Construct")]
		private static class ApplyPatch_Structure_Construct
		{
			private static bool Detour_BeforeRequestConstructionSite(ConstructionData constructionData, Structure parentStructure, Vector3 worldPosition, Quaternion rotation, bool isClientRequest)
			{
				if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
				{
					MelonLogger.Msg("Structure::Construct detour hit");
				}
				bool flag = constructionData.ObjectInfo.ObjectType != ObjectInfoType.Unit;
				if (FireOnRequestBuildEvent(constructionData, parentStructure, worldPosition, rotation, isClientRequest, flag).Block)
				{
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Blocking construction of " + (flag ? "structure" : "unit") + " (" + constructionData.ObjectInfo.DisplayName + ") on Team " + parentStructure.Team.TeamShortName);
					}
					return true;
				}
				return false;
			}

			private static int FindMethodCallInCode(List<CodeInstruction> opCodes, MethodInfo methodCall, bool firstMatch)
			{
				int result = -1;
				for (int i = 0; i < opCodes.Count; i++)
				{
					if (CodeInstructionExtensions.Calls(opCodes[i], methodCall))
					{
						result = i;
						if (firstMatch)
						{
							return result;
						}
					}
				}
				return result;
			}

			private static int FindEntryPoint_Structure_Construct_Unit(List<CodeInstruction> opCodes)
			{
				MethodInfo methodInfo = AccessTools.Method(typeof(Structure), "RPC_Enqueue", (Type[])null, (Type[])null);
				if (methodInfo == null)
				{
					MelonLogger.Warning("Transpiler failure of Structure::Construct. Cannot locate RPC_Enqueue method. Unit construction events will not function correctly.");
					return -1;
				}
				int num = FindMethodCallInCode(opCodes, methodInfo, firstMatch: false);
				MelonLogger.Msg("(Structure::Construct Transpiler_Unit) Found last call of RPCEnqueue at opCode[" + num + "]");
				if (num < 0)
				{
					MelonLogger.Warning("Transpiler failure of Structure::Construct. Cannot locate RPCEnqueue call. Unit construction events will not function correctly.");
					return -1;
				}
				int num2 = num;
				for (int num3 = num; num3 >= 0; num3--)
				{
					if (CodeInstructionExtensions.IsLdarg(opCodes[num3], (int?)0))
					{
						num2 = num3;
						break;
					}
				}
				if (num2 == 0)
				{
					MelonLogger.Warning("Transpiler failure of Structure::Construct. Cannot locate ldarg0. Unit construction events will not function correctly.");
					return -1;
				}
				MelonLogger.Msg("(Structure::Construct Transpiler_Unit) Found insertion point at opCode[" + num2 + "]");
				return num2;
			}

			private static int FindEntryPoint_Structure_Construct_Structure(List<CodeInstruction> opCodes)
			{
				MethodInfo methodInfo = AccessTools.Method(typeof(ConstructionData), "RequestConstructionSite", (Type[])null, (Type[])null);
				if (methodInfo == null)
				{
					MelonLogger.Warning("Transpiler failure of Structure::Construct. Cannot locate RequestConstructionSite method. Structure construction events will not function correctly.");
					return -1;
				}
				int num = FindMethodCallInCode(opCodes, methodInfo, firstMatch: false);
				MelonLogger.Msg("(Structure::Construct Transpiler_Structure) Found last call of ReqConSite at opCode[" + num + "]");
				if (num < 0)
				{
					MelonLogger.Warning("Transpiler failure of Structure::Construct. Cannot locate RequestConstructionSite call. Structure events will not function correctly.");
					return -1;
				}
				int num2 = num;
				for (int num3 = num; num3 >= 0; num3--)
				{
					if (CodeInstructionExtensions.IsLdarg(opCodes[num3], (int?)1))
					{
						num2 = num3;
						break;
					}
				}
				if (num2 == 0)
				{
					MelonLogger.Warning("Transpiler failure of Structure::Construct. Cannot locate ldarg1. Structure events will not function correctly.");
					return -1;
				}
				MelonLogger.Msg("(Structure::Construct Transpiler_Structure) Found insertion point at opCode[" + num2 + "]");
				return num2;
			}

			private static List<CodeInstruction> GenerateILPatch_Structure_Construct_Structure(ILGenerator generator)
			{
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Expected Obj, but got Unknown
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Expected Obj, but got Unknown
				//IL_004c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0056: Expected Obj, but got Unknown
				//IL_005d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0067: Expected Obj, but got Unknown
				//IL_0073: Unknown result type (might be due to invalid IL or missing references)
				//IL_007d: Expected Obj, but got Unknown
				//IL_0084: Unknown result type (might be due to invalid IL or missing references)
				//IL_008e: Expected Obj, but got Unknown
				//IL_009a: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a4: Expected Obj, but got Unknown
				//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b5: Expected Obj, but got Unknown
				//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c6: Expected Obj, but got Unknown
				//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e9: Expected Obj, but got Unknown
				Label label = generator.DefineLabel();
				MethodInfo methodInfo = AccessTools.Method(typeof(ApplyPatch_Structure_Construct), "Detour_BeforeRequestConstructionSite", (Type[])null, (Type[])null);
				return new List<CodeInstruction>
				{
					new CodeInstruction(OpCodes.Ldarg_1, (object)null),
					new CodeInstruction(OpCodes.Ldarg_0, (object)null),
					new CodeInstruction(OpCodes.Ldarg_2, (object)null),
					new CodeInstruction(OpCodes.Ldarg_3, (object)null),
					new CodeInstruction(OpCodes.Ldarg_S, (object)(byte)4),
					new CodeInstruction(OpCodes.Call, (object)methodInfo),
					new CodeInstruction(OpCodes.Brfalse_S, (object)label),
					new CodeInstruction(OpCodes.Ldc_I4_7, (object)null),
					new CodeInstruction(OpCodes.Ret, (object)null),
					new CodeInstruction(OpCodes.Nop, (object)null)
					{
						labels = new List<Label> { label }
					}
				};
			}

			private static List<CodeInstruction> GenerateILPatch_Structure_Construct_Unit(ILGenerator generator)
			{
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Expected Obj, but got Unknown
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Expected Obj, but got Unknown
				//IL_004c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0056: Expected Obj, but got Unknown
				//IL_005d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0067: Expected Obj, but got Unknown
				//IL_0073: Unknown result type (might be due to invalid IL or missing references)
				//IL_007d: Expected Obj, but got Unknown
				//IL_0084: Unknown result type (might be due to invalid IL or missing references)
				//IL_008e: Expected Obj, but got Unknown
				//IL_009a: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a4: Expected Obj, but got Unknown
				//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b5: Expected Obj, but got Unknown
				//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c6: Expected Obj, but got Unknown
				//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e9: Expected Obj, but got Unknown
				Label label = generator.DefineLabel();
				MethodInfo methodInfo = AccessTools.Method(typeof(ApplyPatch_Structure_Construct), "Detour_BeforeRequestConstructionSite", (Type[])null, (Type[])null);
				return new List<CodeInstruction>
				{
					new CodeInstruction(OpCodes.Ldarg_1, (object)null),
					new CodeInstruction(OpCodes.Ldarg_0, (object)null),
					new CodeInstruction(OpCodes.Ldarg_2, (object)null),
					new CodeInstruction(OpCodes.Ldarg_3, (object)null),
					new CodeInstruction(OpCodes.Ldarg_S, (object)(byte)4),
					new CodeInstruction(OpCodes.Call, (object)methodInfo),
					new CodeInstruction(OpCodes.Brfalse_S, (object)label),
					new CodeInstruction(OpCodes.Ldc_I4_7, (object)null),
					new CodeInstruction(OpCodes.Ret, (object)null),
					new CodeInstruction(OpCodes.Nop, (object)null)
					{
						labels = new List<Label> { label }
					}
				};
			}

			[HarmonyTranspiler]
			private static IEnumerable<CodeInstruction> Transpiler_Structure(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
			{
				List<CodeInstruction> list = instructions.ToList();
				int num = FindEntryPoint_Structure_Construct_Structure(list);
				if (num < 0)
				{
					return instructions;
				}
				List<CodeInstruction> collection = GenerateILPatch_Structure_Construct_Structure(generator);
				list.InsertRange(num, collection);
				return list.AsEnumerable();
			}

			[HarmonyTranspiler]
			private static IEnumerable<CodeInstruction> Transpiler_Unit(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
			{
				List<CodeInstruction> list = instructions.ToList();
				int num = FindEntryPoint_Structure_Construct_Unit(list);
				if (num < 0)
				{
					return instructions;
				}
				List<CodeInstruction> collection = GenerateILPatch_Structure_Construct_Unit(generator);
				list.InsertRange(num, collection);
				return list.AsEnumerable();
			}
		}

		public static event EventHandler<OnRequestBuildArgs> OnRequestBuildStructure = delegate
		{
		};

		public static event EventHandler<OnRequestBuildArgs> OnRequestBuildUnit = delegate
		{
		};

		public static OnRequestBuildArgs FireOnRequestBuildEvent(ConstructionData constructionData, Structure parentStructure, Vector3 position, Quaternion rotation, bool playerInitiated, bool isStructure)
		{
			OnRequestBuildArgs onRequestBuildArgs = new OnRequestBuildArgs();
			onRequestBuildArgs.ConstructionData = constructionData;
			onRequestBuildArgs.ParentStructure = parentStructure;
			onRequestBuildArgs.Position = position;
			onRequestBuildArgs.Rotation = rotation;
			onRequestBuildArgs.PlayerInitiated = playerInitiated;
			((!isStructure) ? OnRequestBuildUnit : OnRequestBuildStructure)?.Invoke(null, onRequestBuildArgs);
			return onRequestBuildArgs;
		}
	}
	public static class Event_Damage
	{
		[HarmonyPatch(typeof(DamageManager), "OnReceiveClientDamageHitPacket")]
		private static class ApplyPatch_DamageManager_OnReceiveClientDamageHitPacket
		{
			private static float Detour_BeforeOnDamageReceived(DamageManager damageManager, float damage, GameObject instigator, byte hitAngle)
			{
				OnPreDamageReceivedArgs onPreDamageReceivedArgs = FireOnPrePlayerDamageReceivedEvent(damageManager, damage, instigator, hitAngle);
				if (onPreDamageReceivedArgs.Damage != damage)
				{
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Adjusting damage of (" + damageManager.Owner.ObjectInfo.DisplayName + ") from (" + damage + ") to (" + onPreDamageReceivedArgs.Damage + ")");
					}
					return onPreDamageReceivedArgs.Damage;
				}
				return damage;
			}

			private static int FindEntryPoint_DamageManager_OnReceiveClientDamageHitPacket(List<CodeInstruction> opCodes)
			{
				MethodInfo methodInfo = AccessTools.Method(typeof(DamageManager), "OnDamageReceived", (Type[])null, (Type[])null);
				if (methodInfo == null)
				{
					MelonLogger.Warning("Transpiler failure of DamageManager::OnReceiveClientDamageHitPacket. Cannot locate OnDamageReceived method. Damage events will not function correctly.");
					return -1;
				}
				int num = -1;
				for (int i = 0; i < opCodes.Count; i++)
				{
					if (CodeInstructionExtensions.Calls(opCodes[i], methodInfo))
					{
						num = i;
					}
				}
				MelonLogger.Msg("(DamageManager::OnReceiveClientDamageHitPacket Transpiler) Found last call of OnDamageReceived at opCode[" + num + "]");
				if (num < 0)
				{
					MelonLogger.Warning("Transpiler failure of DamageManager::OnReceiveClientDamageHitPacket. Cannot locate OnDamageReceived call. Damage events will not function correctly.");
					return -1;
				}
				int num2 = num;
				for (int num3 = num; num3 >= 0; num3--)
				{
					if (opCodes[num3].opcode == OpCodes.Ret)
					{
						num2 = num3 + 1;
						break;
					}
				}
				if (num2 == 0)
				{
					MelonLogger.Warning("Transpiler failure of DamageManager::OnReceiveClientDamageHitPacket. Cannot locate ret. Damage events will not function correctly.");
					return -1;
				}
				MelonLogger.Msg("(DamageManager::OnReceiveClientDamageHitPacket Transpiler) Found insertion point at opCode[" + num2 + "]");
				return num2;
			}

			private static List<CodeInstruction> GenerateILPatch_DamageManager_OnReceiveClientDamageHitPacket(ILGenerator generator)
			{
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Expected Obj, but got Unknown
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				//IL_003e: Expected Obj, but got Unknown
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				//IL_004f: Expected Obj, but got Unknown
				//IL_0056: Unknown result type (might be due to invalid IL or missing references)
				//IL_0060: Expected Obj, but got Unknown
				//IL_0067: Unknown result type (might be due to invalid IL or missing references)
				//IL_0071: Expected Obj, but got Unknown
				//IL_0078: Unknown result type (might be due to invalid IL or missing references)
				//IL_0082: Expected Obj, but got Unknown
				MethodInfo methodInfo = AccessTools.Method(typeof(ApplyPatch_DamageManager_OnReceiveClientDamageHitPacket), "Detour_BeforeOnDamageReceived", (Type[])null, (Type[])null);
				return new List<CodeInstruction>
				{
					new CodeInstruction(OpCodes.Ldarg_0, (object)null),
					new CodeInstruction(OpCodes.Ldloc_0, (object)null),
					new CodeInstruction(OpCodes.Ldloc_3, (object)null),
					new CodeInstruction(OpCodes.Ldloc_1, (object)null),
					new CodeInstruction(OpCodes.Call, (object)methodInfo),
					new CodeInstruction(OpCodes.Stloc_0, (object)null)
				};
			}

			[HarmonyTranspiler]
			private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
			{
				List<CodeInstruction> list = instructions.ToList();
				int num = FindEntryPoint_DamageManager_OnReceiveClientDamageHitPacket(list);
				if (num < 0)
				{
					return instructions;
				}
				List<CodeInstruction> list2 = GenerateILPatch_DamageManager_OnReceiveClientDamageHitPacket(generator);
				list2[0].labels.AddRange(list[num].labels);
				list[num].labels.Clear();
				list.InsertRange(num, list2);
				return list.AsEnumerable();
			}
		}

		public static event EventHandler<OnPreDamageReceivedArgs> OnPrePlayerDamageReceived = delegate
		{
		};

		public static OnPreDamageReceivedArgs FireOnPrePlayerDamageReceivedEvent(DamageManager damageManager, float damage, GameObject instigator, byte hitAngle)
		{
			OnPreDamageReceivedArgs onPreDamageReceivedArgs = new OnPreDamageReceivedArgs();
			onPreDamageReceivedArgs.DamageManager = damageManager;
			onPreDamageReceivedArgs.Damage = damage;
			onPreDamageReceivedArgs.Instigator = instigator;
			onPreDamageReceivedArgs.HitAngle = hitAngle;
			OnPrePlayerDamageReceived?.Invoke(null, onPreDamageReceivedArgs);
			return onPreDamageReceivedArgs;
		}
	}
	public static class Event_Roles
	{
		[HarmonyPatch(typeof(MP_Strategy), "ProcessNetRPC")]
		private static class ApplyPatch_MPStrategy_ProcessNetRPC
		{
			public static bool Prefix(MP_Strategy __instance, ref GameByteStreamReader __0, byte __1)
			{
				try
				{
					if (__instance == null || __0 == null)
					{
						return true;
					}
					if (__1 == ERPC_Strategy_RequestRole)
					{
						return ProcessRequestRole(__instance, ref __0, __1);
					}
					if (__1 == ERPC_Strategy_RequestCommanderLotto)
					{
						return ProcessRequestCommanderLotto(__instance, __0, __1);
					}
					return true;
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_Strategy::ProcessNetRPC");
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(MP_TowerDefense), "ProcessNetRPC")]
		private static class ApplyPatch_MPTowerDefense_RequestRole
		{
			public static bool Prefix(MP_TowerDefense __instance, ref GameByteStreamReader __0, byte __1)
			{
				try
				{
					if (__instance == null || __0 == null)
					{
						return true;
					}
					if (__1 != ERPC_TowerDefense_RequestRole)
					{
						return true;
					}
					return ProcessRequestRole(__instance, ref __0, __1);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_TowerDefense::ProcessNetRPC");
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(MP_Strategy), "ResolveCommanderLottery")]
		private static class ApplyPatch_MPStrategy_ResolveCommanderLottery
		{
			public static void Prefix(MP_Strategy __instance, out Player?[]? __state)
			{
				__state = null;
				try
				{
					if (!(__instance == null))
					{
						__state = new Player[8];
						for (int i = 0; i < 8; i++)
						{
							__state[i] = __instance.GetCommanderForTeam(Team.Teams[i]);
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_Strategy::ResolveCommanderLottery");
				}
			}

			public static void Postfix(MP_Strategy __instance, Player?[]? __state)
			{
				try
				{
					if (__instance == null || __state == null)
					{
						return;
					}
					for (int i = 0; i < __state.Length && i < 8; i++)
					{
						Player commanderForTeam = __instance.GetCommanderForTeam(Team.Teams[i]);
						if (commanderForTeam != null && commanderForTeam != __state[i])
						{
							FireOnRoleChangedEvent(commanderForTeam, GameModeExt.ETeamRole.COMMANDER);
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_Strategy::ResolveCommanderLottery");
				}
			}
		}

		public static byte ERPC_Strategy_RequestRole = HelperMethods.FindByteValueInEnum(typeof(MP_Strategy), "ERPCs", "REQUEST_ROLE");

		public static byte ERPC_Strategy_RequestCommanderLotto = HelperMethods.FindByteValueInEnum(typeof(MP_Strategy), "ERPCs", "REQUEST_COMMANDER_LOTTERY_ENTRY");

		public static byte ERPC_TowerDefense_RequestRole = HelperMethods.FindByteValueInEnum(typeof(MP_TowerDefense), "ERPCs", "REQUEST_ROLE");

		public static event EventHandler<OnRequestCommanderArgs> OnRequestCommander = delegate
		{
		};

		public static event EventHandler<OnRoleChangedArgs> OnRoleChanged = delegate
		{
		};

		public static bool ProcessRequestCommanderLotto<T>(T gameModeInstance, GameByteStreamReader reader, byte rpcIndex) where T : GameModeExt
		{
			Player player = Player.FindPlayer((NetworkID)reader.ReadUInt64());
			Team team = reader.ReadTeam();
			if (player == null)
			{
				MelonLogger.Warning("Cannot find player in commander lotto request.");
				return false;
			}
			OnRequestCommanderArgs onRequestCommanderArgs = FireOnRequestCommanderEvent(player);
			if (onRequestCommanderArgs.Block)
			{
				if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
				{
					MelonLogger.Msg("Blocking commander lottery request for " + onRequestCommanderArgs.Requester.PlayerName);
				}
				return false;
			}
			if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
			{
				MelonLogger.Msg("Allowing commander lottery request for player " + onRequestCommanderArgs.Requester.PlayerName);
			}
			MethodInfo method = gameModeInstance.GetType().GetMethod("Sync_AddPlayerToCommanderLottery", BindingFlags.Instance | BindingFlags.NonPublic);
			if (gameModeInstance is MP_Strategy)
			{
				method.Invoke(gameModeInstance, new object[2] { team, player });
			}
			return false;
		}

		public static bool ProcessRequestRole<T>(T gameModeInstance, ref GameByteStreamReader reader, byte rpcIndex) where T : GameModeExt
		{
			Player player = Player.FindPlayer((NetworkID)reader.ReadUInt64(), reader.ReadByte());
			GameModeExt.ETeamRole eTeamRole = (GameModeExt.ETeamRole)reader.ReadByte();
			if (player == null)
			{
				MelonLogger.Warning("Cannot find player in role request.");
				return false;
			}
			if (eTeamRole != GameModeExt.ETeamRole.COMMANDER || (bool)gameModeInstance.GetCommanderForTeam(player.Team))
			{
				reader = RestoreRPC_RequestRoleReader(gameModeInstance, player, eTeamRole);
				FireOnRoleChangedEvent(player, eTeamRole);
				return true;
			}
			BaseTeamSetup teamSetup = GetTeamSetup(gameModeInstance, player.Team);
			if (teamSetup == null)
			{
				return false;
			}
			OnRequestCommanderArgs onRequestCommanderArgs = FireOnRequestCommanderEvent(player);
			if (onRequestCommanderArgs.Block)
			{
				if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
				{
					MelonLogger.Msg("Blocking commander role request for " + onRequestCommanderArgs.Requester.PlayerName);
				}
				if (!onRequestCommanderArgs.PreventSpawnWhenBlocked)
				{
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Preventing Spawn");
					}
					gameModeInstance.TrySpawnPlayerAtSpawnPoint(player, player.Team);
					FireOnRoleChangedEvent(player, GameModeExt.ETeamRole.UNIT);
				}
				return false;
			}
			if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
			{
				MelonLogger.Msg("Allowing to join commander");
			}
			Type type = gameModeInstance.GetType();
			type.GetMethod("SetCommander", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(gameModeInstance, new object[2] { teamSetup.Team, player });
			MethodInfo method = type.GetMethod("RPC_SynchCommander", BindingFlags.Instance | BindingFlags.NonPublic);
			if (gameModeInstance is MP_Strategy)
			{
				method.Invoke(gameModeInstance, new object[2] { teamSetup.Team, false });
			}
			else
			{
				method.Invoke(gameModeInstance, new object[1] { teamSetup.Team });
			}
			FireOnRoleChangedEvent(player, GameModeExt.ETeamRole.COMMANDER);
			return false;
		}

		public static BaseTeamSetup? GetTeamSetup<T>(T gameModeInstance, Team team) where T : GameModeExt
		{
			foreach (BaseTeamSetup baseTeamSetup in gameModeInstance.BaseTeamSetups)
			{
				if (gameModeInstance.GetTeamSetupActive(baseTeamSetup) && baseTeamSetup.Team == team)
				{
					return baseTeamSetup;
				}
			}
			return null;
		}

		public static GameByteStreamReader RestoreRPC_RequestRoleReader<T>(T gameModeInstance, Player requestingPlayer, GameModeExt.ETeamRole role) where T : GameModeExt
		{
			GameByteStreamWriter gameByteStreamWriter = GameByteStreamWriter.GetGameByteStreamWriter(0u, "Si_AdminMod::RestoreRPC_RequestRoleReader");
			gameByteStreamWriter.WriteByte(23);
			gameByteStreamWriter.WriteByte(0);
			if (gameModeInstance is MP_Strategy)
			{
				gameByteStreamWriter.WriteByte(ERPC_Strategy_RequestRole);
			}
			else if (gameModeInstance is MP_TowerDefense)
			{
				gameByteStreamWriter.WriteByte(ERPC_TowerDefense_RequestRole);
			}
			gameByteStreamWriter.WriteUInt64((ulong)requestingPlayer.PlayerID);
			gameByteStreamWriter.WriteByte((byte)requestingPlayer.PlayerChannel);
			gameByteStreamWriter.WriteByte((byte)role);
			GameByteStreamReader gameByteStreamReader = GameByteStreamReader.GetGameByteStreamReader(gameByteStreamWriter.GetByteData(), gameByteStreamWriter.GetByteDataSize(), 0u, "Si_AdminMod::RestoreRPC_RequestRoleReader");
			gameByteStreamReader.ReadByte();
			gameByteStreamReader.ReadByte();
			gameByteStreamReader.ReadByte();
			return gameByteStreamReader;
		}

		public static void FireOnRoleChangedEvent(Player player, GameModeExt.ETeamRole role)
		{
			if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
			{
				MelonLogger.Msg("Firing Role Change Event for " + player.PlayerName + " to role " + role);
			}
			OnRoleChangedArgs e = new OnRoleChangedArgs
			{
				Player = player,
				Role = role
			};
			OnRoleChanged?.Invoke(null, e);
		}

		public static OnRequestCommanderArgs FireOnRequestCommanderEvent(Player player)
		{
			OnRequestCommanderArgs onRequestCommanderArgs = new OnRequestCommanderArgs
			{
				Requester = player,
				Block = false,
				PreventSpawnWhenBlocked = false
			};
			OnRequestCommander?.Invoke(null, onRequestCommanderArgs);
			return onRequestCommanderArgs;
		}
	}
	public static class Event_Structures
	{
		[HarmonyPatch(typeof(StructureSellComponent), "GetCanSell")]
		private static class ApplyPatch_StructureSellComponent_GetCanSell
		{
			public static void Postfix(StructureSellComponent __instance, ref bool __result, Structure __0)
			{
				try
				{
					if (FireOnRequestSellStructureEvent(__0, __0.Team, __result).Override)
					{
						if (__result)
						{
							if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
							{
								MelonLogger.Msg("Blocking structure (" + __0.name + ") from being sold on team " + __0.Team.TeamShortName);
							}
							__result = false;
							return;
						}
						__result = true;
					}
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Allowing structure (" + __0.name + ") to be sold on team " + __0.Team.TeamShortName);
					}
					FireOnCommanderSoldStructure(__0, __0.Team);
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Structure (" + __0.name + ") sold by commander on team " + __0.Team.TeamShortName);
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run StructureSellComponent::GetCanSell");
				}
			}
		}

		public static event EventHandler<OnRequestSellStructureArgs> OnRequestSellStructure = delegate
		{
		};

		public static event EventHandler<OnCommanderSoldStructureArgs> OnCommanderSoldStructure = delegate
		{
		};

		public static OnRequestSellStructureArgs FireOnRequestSellStructureEvent(Structure structure, Team team, bool gameDecision)
		{
			OnRequestSellStructureArgs onRequestSellStructureArgs = new OnRequestSellStructureArgs();
			onRequestSellStructureArgs.Structure = structure;
			onRequestSellStructureArgs.Team = team;
			onRequestSellStructureArgs.GameDecision = gameDecision;
			OnRequestSellStructure?.Invoke(null, onRequestSellStructureArgs);
			return onRequestSellStructureArgs;
		}

		public static OnCommanderSoldStructureArgs FireOnCommanderSoldStructure(Structure structure, Team team)
		{
			OnCommanderSoldStructureArgs onCommanderSoldStructureArgs = new OnCommanderSoldStructureArgs();
			onCommanderSoldStructureArgs.Structure = structure;
			onCommanderSoldStructureArgs.Team = team;
			OnCommanderSoldStructure?.Invoke(null, onCommanderSoldStructureArgs);
			return onCommanderSoldStructureArgs;
		}
	}
	public static class Event_Units
	{
		[HarmonyPatch(typeof(UseAlienTakeOver), "OnUse")]
		private static class ApplyPatch_UseAlienTakeOver_OnUse
		{
			public static bool Prefix(UseAlienTakeOver __instance, Unit __0)
			{
				try
				{
					if (__instance == null || __0 == null)
					{
						return true;
					}
					if (__0.ControlledBy == null || __instance.OwnerUnit == null)
					{
						return true;
					}
					if (FireOnRequestEnterUnitEvent(__0.ControlledBy, __instance.OwnerUnit, asDriver: true).Block)
					{
						if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
						{
							MelonLogger.Msg("Blocking player " + __0.ControlledBy.PlayerName + " from entering alien " + __instance.OwnerUnit.ToString());
						}
						return false;
					}
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Allowing player to enter alien");
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run UseAlienTakeOver::OnUse");
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(Player), "GetCanSwitchToUnit")]
		private static class ApplyPatch_Player_GetCanSwitchToUnit
		{
			public static bool Prefix(Player __instance, Unit __0)
			{
				try
				{
					if (__instance == null || __0 == null)
					{
						return true;
					}
					if (!__instance.IsCommander)
					{
						return true;
					}
					if (FireOnRequestEnterUnitEvent(__instance, __0, asDriver: true).Block)
					{
						NetworkLayer.SendPlayerSelectUnit(__instance, null);
						if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
						{
							MelonLogger.Msg("Blocking player " + __instance.PlayerName + " from entering unit " + __0.ToString());
						}
						HelperMethods.SetCommander(__instance.Team, null);
						HelperMethods.SetCommander(__instance.Team, __instance);
						return false;
					}
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Allowing player to direct control unit");
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run NetworkLayer::SendPlayerSelectUnit");
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(UnitCompartment), "AddUnit")]
		private static class ApplyPatch_UnitCompartment_AddUnit
		{
			public static bool Prefix(UnitCompartment __instance, Unit __0)
			{
				try
				{
					if (__instance == null || __0 == null)
					{
						return true;
					}
					if (__0.ControlledBy == null || __instance.OwnerUnit == null)
					{
						return true;
					}
					if (FireOnRequestEnterUnitEvent(__0.ControlledBy, __instance.OwnerUnit, __instance.IsDriver).Block)
					{
						if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
						{
							MelonLogger.Msg("Blocking player " + __0.ControlledBy.PlayerName + " from entering unit " + __instance.OwnerUnit.ToString());
						}
						return false;
					}
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Allowing player to enter unit's compartment");
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run UnitCompartment::AddUnit");
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(FPSCommanding), "InviteToGroupServer")]
		private static class ApplyPatch_FPSCommanding_InviteToGroupServer
		{
			public static bool Prefix(FPSCommanding __instance, Player __0, Target __1)
			{
				try
				{
					if (__1 == null || __0 == null || !__1.OwnerUnit || __0.Group == null || __0.Group.UnitCount >= __instance.GroupUnitLimit)
					{
						return false;
					}
					if (FireOnRequestInviteToGroupEvent(__0, __1).Block)
					{
						if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
						{
							MelonLogger.Msg("Blocking player " + __0.PlayerName + " from inviting target to group " + __1.ObjectInfo.DisplayName);
						}
						return false;
					}
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Allowing player to invite target to join FPS commanding group");
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run FPSCommanding::InviteToGroupServer");
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(TeleportUI), "RPC_Teleport")]
		private static class ApplyPatch_TeleportUI_RPC_Teleport
		{
			public static bool Prefix(TeleportUI __instance, Unit __0, ref Vector3 __1, ref Vector3 __2)
			{
				try
				{
					if (__instance == null || __0 == null || __instance.Structure == null)
					{
						return true;
					}
					if (__0.ControlledBy == null)
					{
						return true;
					}
					OnRequestTeleportUnitArgs onRequestTeleportUnitArgs = FireOnRequestTeleportUnitEvent(__0, __instance.Structure, __1, __2);
					__1 = onRequestTeleportUnitArgs.TargetPosition;
					__2 = onRequestTeleportUnitArgs.EffectsPosition;
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Allowing player " + __0.ControlledBy.PlayerName + " to teleport unit " + __0.ObjectInfo.DisplayName);
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run TeleportUI::RPC_Teleport");
				}
				return true;
			}
		}

		public static event EventHandler<OnRequestEnterUnitArgs> OnRequestEnterUnit = delegate
		{
		};

		public static event EventHandler<OnRequestInviteToGroupArgs> OnRequestInviteToGroup = delegate
		{
		};

		public static event EventHandler<OnRequestTeleportUnitArgs> OnRequestTeleportUnit = delegate
		{
		};

		public static OnRequestEnterUnitArgs FireOnRequestEnterUnitEvent(Player player, Unit unit, bool asDriver)
		{
			OnRequestEnterUnitArgs onRequestEnterUnitArgs = new OnRequestEnterUnitArgs();
			onRequestEnterUnitArgs.Player = player;
			onRequestEnterUnitArgs.Unit = unit;
			onRequestEnterUnitArgs.AsDriver = asDriver;
			OnRequestEnterUnit?.Invoke(null, onRequestEnterUnitArgs);
			return onRequestEnterUnitArgs;
		}

		public static OnRequestInviteToGroupArgs FireOnRequestInviteToGroupEvent(Player player, Target target)
		{
			OnRequestInviteToGroupArgs onRequestInviteToGroupArgs = new OnRequestInviteToGroupArgs();
			onRequestInviteToGroupArgs.Player = player;
			onRequestInviteToGroupArgs.Target = target;
			OnRequestInviteToGroup?.Invoke(null, onRequestInviteToGroupArgs);
			return onRequestInviteToGroupArgs;
		}

		public static OnRequestTeleportUnitArgs FireOnRequestTeleportUnitEvent(Unit unit, Structure structure, Vector3 targetPosition, Vector3 fxPosition)
		{
			OnRequestTeleportUnitArgs onRequestTeleportUnitArgs = new OnRequestTeleportUnitArgs();
			onRequestTeleportUnitArgs.Unit = unit;
			onRequestTeleportUnitArgs.TeleportStructure = structure;
			onRequestTeleportUnitArgs.TargetPosition = targetPosition;
			onRequestTeleportUnitArgs.EffectsPosition = fxPosition;
			OnRequestTeleportUnit?.Invoke(null, onRequestTeleportUnitArgs);
			return onRequestTeleportUnitArgs;
		}
	}
	public static class HelperMethods
	{
		public delegate void CommandCallback(Player? callerPlayer, string args);

		public static readonly float Timer_Inactive = -123f;

		public const string defaultColor = "<color=#DDE98C>";

		public const string chatPrefix = "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ";

		public static void RegisterAdminCommand(string adminCommand, CommandCallback adminCallback, Power adminPower, string? adminDescription = null)
		{
			AdminMethods.RegisterAdminCommand(adminCommand, adminCallback, adminPower, adminDescription);
		}

		public static void RegisterPlayerCommand(string playerCommand, CommandCallback commandCallback, bool hideFromChat)
		{
			PlayerMethods.RegisterPlayerCommand(playerCommand, commandCallback, hideFromChat);
		}

		public static void RegisterPlayerPhrase(string playerCommand, CommandCallback commandCallback, bool hideFromChat)
		{
			PlayerMethods.RegisterPlayerPhrase(playerCommand, commandCallback, hideFromChat);
		}

		public static bool UnregisterAdminCommand(string adminCommand)
		{
			return AdminMethods.UnregisterAdminCommand(adminCommand);
		}

		public static bool UnregisterPlayerCommand(string playerCommand)
		{
			return PlayerMethods.UnregisterPlayerCommand(playerCommand);
		}

		public static bool UnregisterPlayerPhrase(string playerPhrase)
		{
			return PlayerMethods.UnregisterPlayerPhrase(playerPhrase);
		}

		public static bool IsValidCommandPrefix(char commandFirstCharacter)
		{
			if (commandFirstCharacter == '!' || commandFirstCharacter == '/' || commandFirstCharacter == '.')
			{
				return true;
			}
			return false;
		}

		public static AdminCommand? GetAdminCommand(string commandString)
		{
			return AdminMethods.FindAdminCommandFromString(commandString.Split(' ')[0].Substring(1));
		}

		public static PlayerCommand? GetPlayerCommand(string commandString)
		{
			return PlayerMethods.FindPlayerCommandFromString(commandString.Split(' ')[0].Substring(1));
		}

		public static PlayerCommand? GetPlayerPhrase(string phraseString)
		{
			return PlayerMethods.FindPlayerPhraseFromString(phraseString.Split(' ')[0]);
		}

		public static void ReplyToCommand(params string[] messages)
		{
			SendChatMessageToAll(string.Concat("<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", string.Concat(messages)));
		}

		public static void ReplyToCommand_Player(Player player, params string[] messages)
		{
			SendChatMessageToAll(string.Concat("<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", GetTeamColor(player), player.PlayerName, "</color> ", string.Concat(messages)));
		}

		public static void AlertAdminActivity(Player? adminPlayer, Player targetPlayer, string action)
		{
			string text = ((adminPlayer == null) ? "CONSOLE" : adminPlayer.PlayerName);
			SendChatMessageToAll("<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> " + GetAdminColor() + text + "</color> " + action + " " + GetTeamColor(targetPlayer) + targetPlayer.PlayerName + "</color>");
		}

		public static void AlertAdminAction(Player? adminPlayer, string action)
		{
			string text = ((adminPlayer == null) ? "CONSOLE" : adminPlayer.PlayerName);
			SendChatMessageToAll("<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> " + GetAdminColor() + text + "</color> " + action);
		}

		public static void SendChatMessageToAll(params string[] messages)
		{
			for (int i = 0; i < Player.Players.Count; i++)
			{
				Player player = Player.Players[i];
				if (!(player == null))
				{
					NetworkSendChat(player, teamOnly: false, messages);
				}
			}
		}

		public static void SendChatMessageToTeam(Team team, params string[] messages)
		{
			for (int i = 0; i < Player.Players.Count; i++)
			{
				Player player = Player.Players[i];
				if (!(player == null) && player.Team == team)
				{
					NetworkSendChat(player, teamOnly: false, messages);
				}
			}
		}

		public static void SendChatMessageToTeamNoCommander(Team team, params string[] messages)
		{
			for (int i = 0; i < Player.Players.Count; i++)
			{
				Player player = Player.Players[i];
				if (!(player == null) && player.Team == team && !player.IsCommander)
				{
					NetworkSendChat(player, teamOnly: false, messages);
				}
			}
		}

		public static void SendConsoleMessage(params string[] messages)
		{
			for (int i = 0; i < Player.Players.Count; i++)
			{
				Player player = Player.Players[i];
				if (!(player == null))
				{
					NetworkSendConsole(player, messages);
				}
			}
		}

		public static void SendConsoleMessageToTeam(Team team, params string[] messages)
		{
			for (int i = 0; i < Player.Players.Count; i++)
			{
				Player player = Player.Players[i];
				if (!(player == null) && player.Team == team)
				{
					NetworkSendConsole(player, messages);
				}
			}
		}

		public static void SendChatMessageToPlayer(Player? player, params string[] messages)
		{
			if (player == null)
			{
				DebugConsole.Log(string.Concat(messages));
				DebugConsole.Log("");
			}
			else
			{
				NetworkSendChat(player, teamOnly: false, messages);
			}
		}

		public static void SendConsoleMessageToPlayer(Player? player, params string[] messages)
		{
			if (player == null)
			{
				DebugConsole.Log(string.Concat(messages));
				DebugConsole.Log("");
			}
			else
			{
				NetworkSendConsole(player, messages);
			}
		}

		private static void NetworkSendChat(Player recipient, bool teamOnly, params string[] messages)
		{
			GameByteStreamWriter gameByteStreamWriter = GameByteStreamWriter.GetGameByteStreamWriter(0u, "Si_AdminMod::NetworkSendChat");
			gameByteStreamWriter.WriteByte(11);
			gameByteStreamWriter.WriteUInt64((ulong)NetworkID.CurrentUserID);
			gameByteStreamWriter.WriteByte(0);
			gameByteStreamWriter.WriteString(string.Concat(messages));
			gameByteStreamWriter.WriteBool(teamOnly);
			gameByteStreamWriter.WriteBool(value: true);
			uint byteDataSize = (uint)gameByteStreamWriter.GetByteDataSize();
			FieldInfo field = typeof(NetworkLayer).GetField("NetBitsSent", BindingFlags.Static | BindingFlags.NonPublic);
			uint num = (uint)field.GetValue(null);
			num += byteDataSize * 8;
			field.SetValue(null, num);
			typeof(NetworkLayer).GetMethod("SendServerPacket", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[5]
			{
				recipient.PlayerID,
				gameByteStreamWriter.GetByteData(),
				byteDataSize,
				ENetworkPacketSend.Reliable,
				recipient.PlayerChannel
			});
		}

		private static void NetworkSendConsole(Player recipient, params string[] messages)
		{
			List<List<string>> messageChunks = GetMessageChunks(messages);
			if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
			{
				MelonLogger.Msg("Entering loop for NetworkSendConsole_Netcode with chunk size " + messageChunks.Count);
			}
			foreach (List<string> item in messageChunks)
			{
				NetworkSendConsole_Netcode(recipient, item);
			}
		}

		private static List<List<string>> GetMessageChunks(string[] messages)
		{
			List<List<string>> list = new List<List<string>>();
			List<string> list2 = new List<string>();
			uint num = 16u;
			foreach (string text in messages)
			{
				uint num2 = (uint)(Encoding.UTF8.GetByteCount(text) + 4);
				if (num + num2 >= 2400)
				{
					MelonLogger.Warning("Encountered large console message. Rolling over..");
					list.Add(list2);
					list2 = new List<string>();
					num = 0u;
				}
				list2.Add(text);
				num += num2;
			}
			if (list2.Count > 0)
			{
				list.Add(list2);
			}
			return list;
		}

		private static void NetworkSendConsole_Netcode(Player recipient, List<string> messages)
		{
			GameByteStreamWriter gameByteStreamWriter = GameByteStreamWriter.GetGameByteStreamWriter(0u, "Si_AdminMod::NetworkSendConsole");
			gameByteStreamWriter.WriteByte(32);
			gameByteStreamWriter.WriteUInt64((ulong)recipient.PlayerID);
			gameByteStreamWriter.WriteByte((byte)recipient.PlayerChannel);
			short num = (short)messages.Count;
			gameByteStreamWriter.WriteInt16(num);
			for (int i = 0; i < num; i++)
			{
				gameByteStreamWriter.WriteString(messages[i]);
				gameByteStreamWriter.WriteByte(99);
			}
			uint byteDataSize = (uint)gameByteStreamWriter.GetByteDataSize();
			FieldInfo field = typeof(NetworkLayer).GetField("NetBitsSent", BindingFlags.Static | BindingFlags.NonPublic);
			uint num2 = (uint)field.GetValue(null);
			num2 += byteDataSize * 8;
			field.SetValue(null, num2);
			typeof(NetworkLayer).GetMethod("SendServerPacket", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[5]
			{
				recipient.PlayerID,
				gameByteStreamWriter.GetByteData(),
				byteDataSize,
				ENetworkPacketSend.Reliable,
				recipient.PlayerChannel
			});
		}

		public static void PrintError(Exception exception, string? message = null)
		{
			if (message != null)
			{
				MelonLogger.Msg(message);
			}
			MelonLogger.Error(string.Concat(exception.Message + "\n" + exception.TargetSite, "\n", exception.StackTrace));
		}

		public static void SetCommander(Team team, Player? player)
		{
			if (GameMode.CurrentGameMode is MP_Strategy mP_Strategy)
			{
				Type type = mP_Strategy.GetType();
				type.GetMethod("SetCommander", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(mP_Strategy, new object[2] { team, player });
				type.GetMethod("RPC_SynchCommander", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(mP_Strategy, new object[1] { team });
			}
			else if (GameMode.CurrentGameMode is MP_TowerDefense mP_TowerDefense)
			{
				Type type2 = mP_TowerDefense.GetType();
				type2.GetMethod("SetCommander", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(mP_TowerDefense, new object[2] { team, player });
				type2.GetMethod("RPC_SynchCommander", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(mP_TowerDefense, new object[1] { team });
			}
		}

		public static Player? FindTargetPlayer(string sTarget)
		{
			Player result = null;
			int num = 0;
			List<Player> players = Player.Players;
			int count = players.Count;
			for (int i = 0; i < count; i++)
			{
				if (players[i].PlayerName.Contains(sTarget))
				{
					num++;
					result = players[i];
				}
			}
			if (num != 1)
			{
				result = null;
			}
			return result;
		}

		public static string GetAdminColor()
		{
			return "<color=#ff54ff>";
		}

		private static string TeamColorTextFromIndex(SiConstants.ETeam teamIndex)
		{
			return teamIndex switch
			{
				SiConstants.ETeam.Alien => "<color=#54CC54>", 
				SiConstants.ETeam.Centauri => "<color=#FF5454>", 
				SiConstants.ETeam.Sol => "<color=#548DFF>", 
				SiConstants.ETeam.Wildlife => "<color=#cc8800>", 
				_ => "<color=#FFFFFF>", 
			};
		}

		public static string GetTeamColor(Team team)
		{
			if (team == null)
			{
				return "<color=#FFFFFF>";
			}
			return TeamColorTextFromIndex((SiConstants.ETeam)team.Index);
		}

		public static string GetTeamColor(Player player)
		{
			if (player == null)
			{
				return "<color=#FFFFFF>";
			}
			Team team = player.Team;
			if (team == null)
			{
				return "<color=#FFFFFF>";
			}
			return TeamColorTextFromIndex((SiConstants.ETeam)team.Index);
		}

		public static Power PowerTextToPower(string powerText)
		{
			Power power = Power.None;
			if (powerText.Contains('a'))
			{
				power |= Power.Slot;
			}
			if (powerText.Contains('b'))
			{
				power |= Power.Vote;
			}
			if (powerText.Contains('c'))
			{
				power |= Power.Kick;
			}
			if (powerText.Contains('d'))
			{
				power |= Power.Ban;
			}
			if (powerText.Contains('e'))
			{
				power |= Power.Unban;
			}
			if (powerText.Contains('f'))
			{
				power |= Power.Slay;
			}
			if (powerText.Contains('g'))
			{
				power |= Power.Map;
			}
			if (powerText.Contains('h'))
			{
				power |= Power.Cheat;
			}
			if (powerText.Contains('i'))
			{
				power |= Power.Commander;
			}
			if (powerText.Contains('j'))
			{
				power |= Power.Skip;
			}
			if (powerText.Contains('k'))
			{
				power |= Power.End;
			}
			if (powerText.Contains('l'))
			{
				power |= Power.Eject;
			}
			if (powerText.Contains('m'))
			{
				power |= Power.Mute;
			}
			if (powerText.Contains('n'))
			{
				power |= Power.MuteForever;
			}
			if (powerText.Contains('o'))
			{
				power |= Power.Generic;
			}
			if (powerText.Contains('p'))
			{
				power |= Power.Teams;
			}
			if (powerText.Contains('q'))
			{
				power |= Power.Custom1;
			}
			if (powerText.Contains('r'))
			{
				power |= Power.Custom2;
			}
			if (powerText.Contains('s'))
			{
				power |= Power.Custom3;
			}
			if (powerText.Contains('t'))
			{
				power |= Power.Custom4;
			}
			if (powerText.Contains('u'))
			{
				power |= Power.Reserved1;
			}
			if (powerText.Contains('v'))
			{
				power |= Power.Reserved2;
			}
			if (powerText.Contains('w'))
			{
				power |= Power.Reserved3;
			}
			if (powerText.Contains('x'))
			{
				power |= Power.Reserved4;
			}
			if (powerText.Contains('y'))
			{
				power |= Power.Rcon;
			}
			if (powerText.Contains('z'))
			{
				power |= Power.Root;
			}
			return power;
		}

		public static void DestroyAllStructures(Team team)
		{
			if (!(team == null))
			{
				for (int i = 0; i < team.Structures.Count; i++)
				{
					team.Structures[i].DamageManager.SetHealth01(0f);
				}
			}
		}

		public static bool KickPlayer(Player playerToKick)
		{
			Player serverPlayer = NetworkGameServer.GetServerPlayer();
			if (playerToKick == serverPlayer)
			{
				return false;
			}
			NetworkID serverID = NetworkGameServer.GetServerID();
			NetworkID playerID = playerToKick.PlayerID;
			int playerChannel = playerToKick.PlayerChannel;
			NetworkLayer.SendPlayerConnectResponse(ENetworkPlayerConnectType.Kicked, playerID, playerChannel, serverID);
			Player.RemovePlayer(playerID);
			NetworkLayer.SendPlayerConnect(ENetworkPlayerConnectType.Disconnected, playerID, playerChannel);
			return true;
		}

		public static GameObject? SpawnAtLocation(string name, Vector3 position, Quaternion rotation, int teamIndex = -1)
		{
			int spawnablePrefabIndex = GameDatabase.GetSpawnablePrefabIndex(name);
			if (spawnablePrefabIndex <= -1)
			{
				DebugConsole.Log("SpawnAtLocation: Could not find prefab '" + name + "' in network spawn lists!", DebugConsole.LogLevel.Warning);
				return null;
			}
			Team team = ((teamIndex < 0) ? null : Team.Teams[teamIndex]);
			GameObject spawnablePrefab = GameDatabase.GetSpawnablePrefab(spawnablePrefabIndex);
			if (spawnablePrefab == null)
			{
				DebugConsole.Log("SpawnAtLocation: Could not obtain spawnable prefab for '" + name + "'!", DebugConsole.LogLevel.Warning);
			}
			return Game.SpawnPrefab(spawnablePrefab, null, team, position, rotation);
		}

		public static byte FindByteValueInEnum(Type parentType, string enumName, string byteName)
		{
			foreach (object enumValue in parentType.GetNestedType(enumName, BindingFlags.NonPublic).GetEnumValues())
			{
				if (string.Compare(enumValue.ToString(), byteName) == 0)
				{
					return (byte)enumValue;
				}
			}
			return byte.MaxValue;
		}

		public static bool IsTimerActive(float time)
		{
			if (time >= 0f)
			{
				return true;
			}
			return false;
		}

		public static void StartTimer(ref float timer)
		{
			timer = 0f;
		}
	}
	public class HostAction
	{
		public static bool AddAdmin(Player player, string powerText, byte level)
		{
			long steamID = (long)player.PlayerID.SteamID.m_SteamID;
			if (SiAdminMod.AdminList == null)
			{
				return false;
			}
			if (AdminMethods.FindAdminFromSteamId(steamID, modAdminOnly: true) != null)
			{
				return false;
			}
			Admin admin = new Admin
			{
				Name = player.PlayerName,
				SteamId = steamID,
				Level = level,
				Powers = HelperMethods.PowerTextToPower(powerText),
				CreatedOn = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds
			};
			admin.LastModifiedOn = admin.CreatedOn;
			SiAdminMod.AdminList.Add(admin);
			AdminFile.Update(SiAdminMod.AdminList);
			return true;
		}
	}
	public static class ClientChatHandler
	{
		public static void OnRequestPlayerChat(object? sender, OnRequestPlayerChatArgs args)
		{
			try
			{
				if (args.Player == null)
				{
					return;
				}
				if (!HelperMethods.IsValidCommandPrefix(args.Text[0]))
				{
					PlayerCommand playerPhrase = HelperMethods.GetPlayerPhrase(args.Text);
					if (playerPhrase != null)
					{
						playerPhrase.PlayerCommandCallback(args.Player, args.Text);
						args.Block = playerPhrase.HideChatMessage;
					}
					return;
				}
				if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
				{
					MelonLogger.Msg("Processing admin or player command.");
				}
				if (args.TeamOnly && SiAdminMod.Pref_Admin_AcceptTeamChatCommands != null && !SiAdminMod.Pref_Admin_AcceptTeamChatCommands.Value)
				{
					return;
				}
				AdminCommand adminCommand = HelperMethods.GetAdminCommand(args.Text);
				if (adminCommand != null)
				{
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Processing admin command: " + adminCommand.AdminCommandText);
					}
					if (!args.Player.IsAdmin())
					{
						HelperMethods.ReplyToCommand_Player(args.Player, "is not an admin");
						return;
					}
					Power adminPowers = args.Player.GetAdminPowers();
					if (!AdminMethods.PowerInPowers(adminCommand.AdminPower, adminPowers))
					{
						HelperMethods.ReplyToCommand_Player(args.Player, "unauthorized command");
					}
					else
					{
						adminCommand.AdminCallback(args.Player, args.Text);
						args.Block = args.Text[0] == '/';
					}
					return;
				}
				PlayerCommand playerCommand = HelperMethods.GetPlayerCommand(args.Text);
				if (playerCommand != null)
				{
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Processing player command: " + playerCommand.CommandName);
					}
					playerCommand.PlayerCommandCallback(args.Player, args.Text);
					args.Block = playerCommand.HideChatMessage;
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run AdminMod::OnRequestPlayerChat");
			}
		}
	}
	[HarmonyPatch(typeof(Player), "SendChatMessage")]
	internal static class Patch_SendChatMessage_AdminCommands
	{
		public static bool Prefix(Player __instance, bool __result, string __0, bool __1)
		{
			try
			{
				if (__1 && SiAdminMod.Pref_Admin_AcceptTeamChatCommands != null && !SiAdminMod.Pref_Admin_AcceptTeamChatCommands.Value)
				{
					return true;
				}
				if (string.Equals(__0.Split(' ')[0], "!addadmin", StringComparison.OrdinalIgnoreCase))
				{
					Player serverPlayer = NetworkGameServer.GetServerPlayer();
					if (__instance != serverPlayer)
					{
						HelperMethods.ReplyToCommand_Player(__instance, "cannot use " + __0.Split(' ')[0]);
						return false;
					}
					int num = __0.Split(' ').Length - 1;
					if (num > 3)
					{
						HelperMethods.ReplyToCommand(__0.Split(' ')[0] + " usage: <player> <powers> <level>");
						return false;
					}
					if (num < 3)
					{
						HelperMethods.ReplyToCommand(__0.Split(' ')[0] + " usage: <player> <powers> <level>");
						return false;
					}
					Player player = HelperMethods.FindTargetPlayer(__0.Split(' ')[1]);
					if (player == null)
					{
						HelperMethods.ReplyToCommand(__0.Split(' ')[0] + ": Ambiguous or invalid target");
						return false;
					}
					string text = __0.Split(' ')[2];
					if (text.Any(char.IsDigit))
					{
						HelperMethods.ReplyToCommand(__0.Split(' ')[0] + ": Powers invalid");
						return false;
					}
					string text2 = __0.Split(' ')[3];
					int num2 = int.Parse(text2);
					if (num2 < 0)
					{
						HelperMethods.ReplyToCommand(__0.Split(' ')[0] + ": Level too low");
						return false;
					}
					if (num2 > 255)
					{
						HelperMethods.ReplyToCommand(__0.Split(' ')[0] + ": Level too high");
						return false;
					}
					if (HostAction.AddAdmin(player, text, (byte)num2))
					{
						HelperMethods.ReplyToCommand_Player(player, "added as admin (Level " + text2 + ")");
					}
					else
					{
						HelperMethods.ReplyToCommand_Player(player, "is already an admin");
					}
					return false;
				}
				if (string.Equals(__0.Split(' ')[0], "!removeadmin", StringComparison.OrdinalIgnoreCase) || string.Equals(__0.Split(' ')[0], "!deladmin", StringComparison.OrdinalIgnoreCase))
				{
					Player serverPlayer2 = NetworkGameServer.GetServerPlayer();
					if (__instance != serverPlayer2)
					{
						HelperMethods.ReplyToCommand_Player(__instance, "cannot use " + __0.Split(' ')[0]);
						return false;
					}
					return false;
				}
				if (!HelperMethods.IsValidCommandPrefix(__0[0]))
				{
					PlayerCommand playerPhrase = HelperMethods.GetPlayerPhrase(__0);
					if (playerPhrase == null)
					{
						return true;
					}
					playerPhrase.PlayerCommandCallback(__instance, __0);
					return false;
				}
				if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
				{
					MelonLogger.Msg("Processing host admin or player command.");
				}
				AdminCommand adminCommand = HelperMethods.GetAdminCommand(__0);
				if (adminCommand != null)
				{
					if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
					{
						MelonLogger.Msg("Processing host admin command: " + adminCommand.AdminCommandText);
					}
					if (!__instance.IsAdmin())
					{
						HelperMethods.ReplyToCommand_Player(__instance, "is not an admin");
						return false;
					}
					Power adminPowers = __instance.GetAdminPowers();
					if (!AdminMethods.PowerInPowers(adminCommand.AdminPower, adminPowers))
					{
						HelperMethods.ReplyToCommand_Player(__instance, "unauthorized command");
						return false;
					}
					adminCommand.AdminCallback(__instance, __0);
					return false;
				}
				PlayerCommand playerCommand = HelperMethods.GetPlayerCommand(__0);
				if (playerCommand == null)
				{
					return true;
				}
				if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
				{
					MelonLogger.Msg("Processing host command: " + playerCommand.CommandName);
				}
				playerCommand.PlayerCommandCallback(__instance, __0);
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run SendChatMessage");
			}
			return true;
		}
	}
	public class PlayerCommand
	{
		private string _commandName;

		private HelperMethods.CommandCallback _commandCallback;

		public string CommandName
		{
			get
			{
				return _commandName;
			}
			set
			{
				_commandName = value ?? throw new ArgumentNullException("Command name is required.");
			}
		}

		public HelperMethods.CommandCallback PlayerCommandCallback
		{
			get
			{
				return _commandCallback;
			}
			set
			{
				_commandCallback = value ?? throw new ArgumentNullException("Command callback is required.");
			}
		}

		public bool HideChatMessage { get; set; }
	}
	public class PlayerAdmin : Player
	{
		public static bool CanAdminExecute(Player callerPlayer, Power power, Player? targetPlayer = null)
		{
			Admin admin = AdminMethods.FindAdminFromSteamId((long)callerPlayer.PlayerID.SteamID.m_SteamID);
			Power powers = Power.None;
			if (admin != null)
			{
				powers = admin.Powers;
			}
			if (targetPlayer != null)
			{
				if (CanAdminTarget(callerPlayer, targetPlayer))
				{
					if (AdminMethods.PowerInPowers(power, powers))
					{
						return true;
					}
					return false;
				}
				return false;
			}
			if (AdminMethods.PowerInPowers(power, powers))
			{
				return true;
			}
			return false;
		}

		public static bool CanAdminTarget(Player callerPlayer, Player targetPlayer)
		{
			Admin admin = AdminMethods.FindAdminFromSteamId((long)callerPlayer.PlayerID.SteamID.m_SteamID);
			byte b = 0;
			if (admin != null)
			{
				b = admin.Level;
			}
			Admin admin2 = AdminMethods.FindAdminFromSteamId((long)targetPlayer.PlayerID.SteamID.m_SteamID);
			byte b2 = 0;
			if (admin2 != null)
			{
				b2 = admin2.Level;
			}
			if (b > 0 && b >= b2)
			{
				return true;
			}
			return false;
		}

		public static Power GetAdminPowers(Player callerPlayer)
		{
			return AdminMethods.FindAdminFromSteamId((long)callerPlayer.PlayerID.SteamID.m_SteamID)?.Powers ?? Power.None;
		}

		public static byte GetAdminLevel(Player callerPlayer)
		{
			return AdminMethods.FindAdminFromSteamId((long)callerPlayer.PlayerID.SteamID.m_SteamID)?.Level ?? 0;
		}

		public static bool IsAdmin(Player callerPlayer)
		{
			Admin admin = AdminMethods.FindAdminFromSteamId((long)callerPlayer.PlayerID.SteamID.m_SteamID);
			if (admin != null && (admin.Powers & (Power)(-983556)) != 0)
			{
				return true;
			}
			return false;
		}
	}
	public static class PlayerInterface
	{
		public static bool CanAdminExecute(this Player player, Power power, Player? targetPlayer = null)
		{
			return PlayerAdmin.CanAdminExecute(player, power, targetPlayer);
		}

		public static bool CanAdminTarget(this Player player, Player targetPlayer)
		{
			return PlayerAdmin.CanAdminTarget(player, targetPlayer);
		}

		public static Power GetAdminPowers(this Player player)
		{
			return PlayerAdmin.GetAdminPowers(player);
		}

		public static byte GetAdminLevel(this Player player)
		{
			return PlayerAdmin.GetAdminLevel(player);
		}

		public static bool IsAdmin(this Player player)
		{
			return PlayerAdmin.IsAdmin(player);
		}
	}
	public static class PlayerMethods
	{
		public static List<PlayerCommand> PlayerCommands;

		public static List<PlayerCommand> PlayerPhrases;

		public static PlayerCommand? FindPlayerCommandFromString(string commandText)
		{
			foreach (PlayerCommand playerCommand in PlayerCommands)
			{
				if (string.Equals(playerCommand.CommandName, commandText, StringComparison.OrdinalIgnoreCase))
				{
					return playerCommand;
				}
			}
			return null;
		}

		public static PlayerCommand? FindPlayerPhraseFromString(string phraseText)
		{
			foreach (PlayerCommand playerPhrase in PlayerPhrases)
			{
				if (string.Equals(playerPhrase.CommandName, phraseText, StringComparison.OrdinalIgnoreCase))
				{
					return playerPhrase;
				}
			}
			return null;
		}

		public static void RegisterPlayerCommand(string playerCommand, HelperMethods.CommandCallback playerCallback, bool hideFromChat)
		{
			PlayerCommand item = new PlayerCommand
			{
				CommandName = playerCommand,
				PlayerCommandCallback = playerCallback,
				HideChatMessage = hideFromChat
			};
			PlayerCommands.Add(item);
		}

		public static void RegisterPlayerPhrase(string playerPhrase, HelperMethods.CommandCallback playerCallback, bool hideFromChat)
		{
			PlayerCommand item = new PlayerCommand
			{
				CommandName = playerPhrase,
				PlayerCommandCallback = playerCallback,
				HideChatMessage = hideFromChat
			};
			PlayerPhrases.Add(item);
		}

		public static bool UnregisterPlayerCommand(string playerCommand)
		{
			PlayerCommand playerCommand2 = FindPlayerCommandFromString(playerCommand);
			if (playerCommand2 == null)
			{
				return false;
			}
			return PlayerCommands.Remove(playerCommand2);
		}

		public static bool UnregisterPlayerPhrase(string playerPhrase)
		{
			PlayerCommand playerCommand = FindPlayerPhraseFromString(playerPhrase);
			if (playerCommand == null)
			{
				return false;
			}
			return PlayerPhrases.Remove(playerCommand);
		}
	}
	public class WaveReader
	{
		public const int WAVE_FORMAT_PCM = 1;

		public const int WAVE_FORMAT_IEEE_FLOAT = 3;

		private float[] _samples;

		public int Frequency { get; set; }

		public int Channels { get; set; }

		public int BitsPerSample { get; set; }

		public int AudioFormat { get; set; }

		public float[] Samples
		{
			get
			{
				return _samples;
			}
			set
			{
				_samples = value ?? throw new ArgumentNullException("Samples are required.");
			}
		}

		public static WaveReader? Load(string filePath)
		{
			try
			{
				byte[] array = File.ReadAllBytes(filePath);
				if (Encoding.ASCII.GetString(array, 0, 4) != "RIFF")
				{
					throw new Exception("Invalid .WAV file. Missing 'RIFF'.");
				}
				if (Encoding.ASCII.GetString(array, 8, 4) != "WAVE")
				{
					throw new Exception("Invalid .WAV file. Missing 'WAVE'.");
				}
				int num = 12;
				int channels = 0;
				int frequency = 0;
				int num2 = 0;
				int num3 = 0;
				byte[] array2 = null;
				while (num < array.Length)
				{
					string text = Encoding.ASCII.GetString(array, num, 4);
					num += 4;
					int num4 = BitConverter.ToInt32(array, num);
					num += 4;
					if (text == "fmt ")
					{
						num3 = BitConverter.ToInt16(array, num);
						channels = BitConverter.ToInt16(array, num + 2);
						frequency = BitConverter.ToInt32(array, num + 4);
						num2 = BitConverter.ToInt16(array, num + 14);
						num += num4;
						continue;
					}
					if (text == "data")
					{
						array2 = new byte[num4];
						Array.Copy(array, num, array2, 0, num4);
						if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
						{
							MelonLogger.Msg("Found .WAV data region with size: " + num4);
						}
						break;
					}
					num += num4;
				}
				if (array2 == null)
				{
					throw new Exception("Invalid .WAV file. Missing 'DATA' section.");
				}
				int num5 = num2 / 8;
				int num6 = array2.Length / num5;
				float[] array3 = new float[num6];
				for (int i = 0; i < num6; i++)
				{
					int num7 = i * num5;
					switch (num3)
					{
					case 1:
						switch (num2)
						{
						case 8:
							array3[i] = (float)(array2[num7] - 128) / 128f;
							break;
						case 16:
						{
							short num8 = BitConverter.ToInt16(array2, num7);
							array3[i] = ((float)num8 - 32768f) / 32768f;
							break;
						}
						default:
							throw new Exception(num2 + " is an unsupported PCM bit depth. Use 8 or 16.");
						}
						break;
					case 3:
						if (num2 != 32)
						{
							throw new Exception("Unsupported float format using bits-per-sample of: " + num2);
						}
						array3[i] = BitConverter.ToSingle(array2, num7);
						break;
					default:
						throw new Exception("Unsupported WAV encoding format of: " + num3);
					}
				}
				return new WaveReader
				{
					Frequency = frequency,
					Channels = channels,
					BitsPerSample = num2,
					AudioFormat = num3,
					Samples = array3
				};
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Error processing .WAV file: " + filePath);
			}
			return null;
		}
	}
	public class AudioHelper
	{
		private const int targetFrequency = 12000;

		private static GameByteStreamWriter GenerateAudioStreamWriter(Player player, byte[] audioBytes, int offset)
		{
			GameByteStreamWriter gameByteStreamWriter = GameByteStreamWriter.GetGameByteStreamWriter(0u, "NetworkLayer::SendCustomAudio");
			gameByteStreamWriter.WriteByte(33);
			gameByteStreamWriter.WriteUInt64((ulong)player.PlayerID);
			gameByteStreamWriter.WriteByte((byte)player.PlayerChannel);
			gameByteStreamWriter.WriteBool(value: false);
			gameByteStreamWriter.WriteBool(value: false);
			gameByteStreamWriter.WriteUInt16((ushort)offset);
			gameByteStreamWriter.WriteByteArray(audioBytes);
			return gameByteStreamWriter;
		}

		public static float[] StereoToMono(float[] inputSamples, int channels)
		{
			int num = inputSamples.Length / channels;
			float[] array = new float[num];
			for (int i = 0; i < num; i++)
			{
				float num2 = 0f;
				for (int j = 0; j < channels; j++)
				{
					num2 += inputSamples[i * channels + j];
				}
				array[i] = num2 / (float)channels;
			}
			return array;
		}

		public static float[] ResampleToTargetFrequency(float[] inputSamples, int inputFrequency, int targetFrequency)
		{
			int num = (int)((float)inputSamples.Length * ((float)targetFrequency / (float)inputFrequency));
			float[] array = new float[num];
			for (int i = 0; i < num; i++)
			{
				float num2 = (float)i * ((float)inputFrequency / (float)targetFrequency);
				int num3 = (int)Math.Floor(num2);
				int num4 = Math.Min(num3 + 1, inputSamples.Length - 1);
				float num5 = num2 - (float)num3;
				array[i] = inputSamples[num3] * (1f - num5) + inputSamples[num4] * num5;
			}
			return array;
		}

		public static async Task PlaySoundFile(string audioFilePath, Player? targetPlayer = null)
		{
			string text = Path.Combine(MelonEnvironment.UserDataDirectory, audioFilePath);
			if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
			{
				MelonLogger.Msg("Trying to load audio file: " + text);
			}
			WaveReader waveReader = WaveReader.Load(text);
			if (waveReader == null)
			{
				MelonLogger.Warning("Could not load wave file: " + text);
				return;
			}
			int frequency = waveReader.Frequency;
			int audioFormat = waveReader.AudioFormat;
			int bitsPerSample = waveReader.BitsPerSample;
			int channels = waveReader.Channels;
			float[] samples = waveReader.Samples;
			int num = samples.Length;
			if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
			{
				MelonLogger.Msg($"Loaded wave with frequency: {frequency} samples: {num} channels: {channels} format: {audioFormat} bps: {bitsPerSample}");
			}
			if ((float)Math.Abs(frequency - 12000) / 12000f > 0.1f)
			{
				if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
				{
					MelonLogger.Msg("Frequency too far away from 12KHz. Performing Resampling.");
				}
				samples = ResampleToTargetFrequency(samples, frequency, 12000);
			}
			if (channels > 1)
			{
				if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
				{
					MelonLogger.Msg("More than one channel present. Converting to mono.");
				}
				samples = StereoToMono(samples, channels);
			}
			int offset = 0;
			int packetsSent = 0;
			while (offset < samples.Length)
			{
				int num2 = Math.Min(1024, samples.Length - offset);
				float[] array = new float[1024];
				Array.Clear(array, 0, 1024);
				Array.Copy(samples, offset, array, 0, num2);
				byte[] array2 = new byte[1024];
				for (int i = 0; i < 1024; i++)
				{
					float num3 = Math.Clamp(array[i], -1f, 1f);
					array2[i] = (byte)(num3 * 128f + 128f);
				}
				if (targetPlayer == null)
				{
					SendAudioPacket(array2, offset + 1);
				}
				else
				{
					SendAudioPacketToPlayer(targetPlayer, array2, offset + 1);
				}
				packetsSent++;
				offset += num2;
				await Task.Delay(TimeSpan.FromMilliseconds(70.0));
			}
			if (SiAdminMod.Pref_Admin_DebugLogMessages.Value)
			{
				MelonLogger.Msg("Sent " + packetsSent + " audio packets.");
			}
		}

		public static void SendAudioPacketToPlayer(Player targetPlayer, byte[] audioBytes, int offset, Player? originPlayer = null)
		{
			GameByteStreamWriter gameByteStreamWriter = ((!(originPlayer != null)) ? GenerateAudioStreamWriter(targetPlayer, audioBytes, offset) : GenerateAudioStreamWriter(originPlayer, audioBytes, offset));
			uint byteDataSize = (uint)gameByteStreamWriter.GetByteDataSize();
			FieldInfo field = typeof(NetworkLayer).GetField("NetBitsSent", BindingFlags.Static | BindingFlags.NonPublic);
			uint num = (uint)field.GetValue(null);
			num += byteDataSize * 8;
			field.SetValue(null, num);
			typeof(NetworkLayer).GetMethod("SendServerPacket", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[5]
			{
				targetPlayer.PlayerID,
				gameByteStreamWriter.GetByteData(),
				byteDataSize,
				ENetworkPacketSend.Reliable,
				targetPlayer.PlayerChannel
			});
		}

		public static void SendAudioPacket(byte[] audioBytes, int offset, Player? originPlayer = null)
		{
			for (int i = 0; i < Player.Players.Count; i++)
			{
				Player player = Player.Players[i];
				if (!(player == null) && !(player == NetworkGameServer.GetServerPlayer()))
				{
					SendAudioPacketToPlayer(player, audioBytes, offset, originPlayer);
				}
			}
		}
	}
}
