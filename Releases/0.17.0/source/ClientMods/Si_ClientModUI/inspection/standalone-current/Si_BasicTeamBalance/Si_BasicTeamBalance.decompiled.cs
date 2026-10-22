using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using Microsoft.CodeAnalysis;
using Si_BasicTeamBalance;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(BasicTeamBalance), "Basic Team Balance", "1.4.6", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_BasicTeamBalance")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_BasicTeamBalance")]
[assembly: AssemblyTitle("Si_BasicTeamBalance")]
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
namespace Si_BasicTeamBalance
{
	public class BasicTeamBalance : MelonMod
	{
		[HarmonyPatch(typeof(MP_TowerDefense), "ProcessNetRPC")]
		private static class ApplyPatch_MPTowerDefense_JoinTeam
		{
			public static bool Prefix(MP_TowerDefense __instance, GameByteStreamReader __0, byte __1)
			{
				try
				{
					return ProcessJoinTeam(__instance, __0, __1);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_TowerDefense::ProcessNetRPC");
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(MP_Strategy), "ProcessNetRPC")]
		private static class ApplyPatch_MPStrategy_JoinTeam
		{
			public static bool Prefix(MP_Strategy __instance, GameByteStreamReader __0, byte __1)
			{
				try
				{
					return ProcessJoinTeam(__instance, __0, __1);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_Strategy::ProcessNetRPC");
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(MusicJukeboxHandler), "OnGameStarted")]
		private static class ApplyPatch_MusicJukeboxHandler_OnGameStarted
		{
			public static void Postfix(MusicJukeboxHandler __instance, GameMode __0)
			{
				try
				{
					if (_PreventEarlyTeamSwitches.Value)
					{
						preventTeamSwitches = true;
						HelperMethods.StartTimer(ref Timer_AllowTeamSwitches);
						MelonLogger.Msg("Early game team lock set for " + _AllowTeamSwitchAfterTime.Value + " seconds.");
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MusicJukeboxHandler::OnGameStarted");
				}
			}
		}

		[HarmonyPatch(typeof(MusicJukeboxHandler), "OnGameEnded")]
		private static class ApplyPatch_OnGameEnded
		{
			public static void Postfix(MusicJukeboxHandler __instance, GameMode __0, Team __1)
			{
				try
				{
					if (_PreventEarlyTeamSwitches.Value && HelperMethods.IsTimerActive(Timer_AllowTeamSwitches))
					{
						Timer_AllowTeamSwitches = HelperMethods.Timer_Inactive;
						MelonLogger.Msg("Game ended before early game team lock expired. Forcing timer to expire.");
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MusicJukeboxHandler::OnGameStarted");
				}
			}
		}

		private static MelonPreferences_Category _modCategory = null;

		private static MelonPreferences_Entry<float> _TwoTeamBalanceDivisor = null;

		private static MelonPreferences_Entry<float> _TwoTeamBalanceAddend = null;

		private static MelonPreferences_Entry<float> _ThreeTeamBalanceDivisor = null;

		private static MelonPreferences_Entry<float> _ThreeTeamBalanceAddend = null;

		private static MelonPreferences_Entry<bool> _PreventEarlyTeamSwitches = null;

		private static MelonPreferences_Entry<int> _AllowTeamSwitchAfterTime = null;

		private static byte ERPC_Strategy_ClearRequest = HelperMethods.FindByteValueInEnum(typeof(MP_Strategy), "ERPCs", "CLEAR_REQUEST");

		private static byte ERPC_Strategy_RequestJoinTeam = HelperMethods.FindByteValueInEnum(typeof(MP_Strategy), "ERPCs", "REQUEST_JOIN_TEAM");

		private static byte ERPC_TowerDefense_ClearRequest = HelperMethods.FindByteValueInEnum(typeof(MP_TowerDefense), "ERPCs", "CLEAR_REQUEST");

		private static byte ERPC_TowerDefense_RequestJoinTeam = HelperMethods.FindByteValueInEnum(typeof(MP_TowerDefense), "ERPCs", "REQUEST_JOIN_TEAM");

		private static Player? LastPlayerChatMessage;

		private static bool preventTeamSwitches;

		private static float Timer_AllowTeamSwitches = HelperMethods.Timer_Inactive;

		private const string ModCategory = "Silica";

		public override void OnInitializeMelon()
		{
			if (_modCategory == null)
			{
				_modCategory = MelonPreferences.CreateCategory("Silica");
			}
			if (_TwoTeamBalanceDivisor == null)
			{
				_TwoTeamBalanceDivisor = _modCategory.CreateEntry<float>("TeamBalance_TwoTeam_Divisor", 8f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_TwoTeamBalanceAddend == null)
			{
				_TwoTeamBalanceAddend = _modCategory.CreateEntry<float>("TeamBalance_TwoTeam_Addend", 1f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_ThreeTeamBalanceDivisor == null)
			{
				_ThreeTeamBalanceDivisor = _modCategory.CreateEntry<float>("TeamBalance_ThreeTeam_Divisor", 10f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_ThreeTeamBalanceAddend == null)
			{
				_ThreeTeamBalanceAddend = _modCategory.CreateEntry<float>("TeamBalance_ThreeTeam_Addend", 0f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_PreventEarlyTeamSwitches == null)
			{
				_PreventEarlyTeamSwitches = _modCategory.CreateEntry<bool>("TeamBalance_Prevent_EarlySwitching", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_AllowTeamSwitchAfterTime == null)
			{
				_AllowTeamSwitchAfterTime = _modCategory.CreateEntry<int>("TeamBalance_Prevent_EarlySwitching_For_Seconds", 75, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			preventTeamSwitches = false;
		}

		[MethodImpl(MethodImplOptions.NoOptimization)]
		public override void OnLateInitializeMelon()
		{
			HelperMethods.CommandCallback adminCallback = Command_Team;
			HelperMethods.RegisterAdminCommand("team", adminCallback, Power.Teams, "Moves target player to target team. Usage: !team [optional:<player>] <teamname>");
		}

		private static byte GetClearRequestValue(GameMode gameMode)
		{
			if (GameMode.CurrentGameMode is MP_TowerDefense)
			{
				return ERPC_TowerDefense_ClearRequest;
			}
			if (GameMode.CurrentGameMode is MP_Strategy)
			{
				return ERPC_Strategy_ClearRequest;
			}
			return 0;
		}

		public static void SendClearRequest(ulong thisPlayerSteam64, int thisPlayerChannel)
		{
			byte clearRequestValue = GetClearRequestValue(GameMode.CurrentGameMode);
			GameByteStreamWriter gameByteStreamWriter = GameMode.CurrentGameMode.CreateRPCPacket(clearRequestValue);
			if (gameByteStreamWriter != null)
			{
				gameByteStreamWriter.WriteUInt64(thisPlayerSteam64);
				gameByteStreamWriter.WriteByte((byte)thisPlayerChannel);
				GameMode.CurrentGameMode.SendRPCPacket(gameByteStreamWriter);
			}
		}

		public static int GetNumberOfActiveTeams<T>(T gameModeInstance) where T : GameModeExt
		{
			int num = 0;
			foreach (BaseTeamSetup baseTeamSetup in gameModeInstance.BaseTeamSetups)
			{
				if (gameModeInstance.GetTeamSetupActive(baseTeamSetup) && !baseTeamSetup.GetHasLost())
				{
					num++;
				}
			}
			return num;
		}

		public static Team? FindLowestPopulationTeam<T>(T gameModeInstance) where T : GameModeExt
		{
			int num = NetworkGameServer.GetPlayersMax() + 1;
			Team result = null;
			foreach (BaseTeamSetup baseTeamSetup in gameModeInstance.BaseTeamSetups)
			{
				if (gameModeInstance.GetTeamSetupActive(baseTeamSetup) && !baseTeamSetup.GetHasLost())
				{
					int numPlayers = baseTeamSetup.Team.GetNumPlayers();
					if (numPlayers < num)
					{
						num = numPlayers;
						result = baseTeamSetup.Team;
					}
				}
			}
			return result;
		}

		public static bool JoinCausesImbalance<T>(Team? targetTeam, T gameModeInstance) where T : GameModeExt
		{
			if (targetTeam == null)
			{
				return false;
			}
			_ = gameModeInstance.TeamsVersus;
			Team team = null;
			int numberOfActiveTeams = GetNumberOfActiveTeams(gameModeInstance);
			team = FindLowestPopulationTeam(gameModeInstance);
			if (team == null || team == targetTeam)
			{
				return false;
			}
			int num = targetTeam.GetNumPlayers() - team.GetNumPlayers();
			if (num < 0)
			{
				num = -num;
			}
			int count = Player.Players.Count;
			int num2 = 5;
			switch (numberOfActiveTeams)
			{
			case 2:
				num2 = (int)Math.Ceiling((float)count / _TwoTeamBalanceDivisor.Value + _TwoTeamBalanceAddend.Value);
				break;
			case 3:
				num2 = (int)Math.Ceiling((float)count / _ThreeTeamBalanceDivisor.Value + _ThreeTeamBalanceAddend.Value);
				break;
			default:
				MelonLogger.Warning("Found unhandled number of active teams: " + numberOfActiveTeams);
				break;
			}
			if (num > num2)
			{
				return true;
			}
			return false;
		}

		public static void Command_Team(Player? callerPlayer, string args)
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
			Team teamByName = Team.GetTeamByName(args.Split(' ')[(num == 1) ? 1 : 2]);
			if (teamByName == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Ambiguous or invalid team name");
				return;
			}
			Player player = null;
			player = ((num <= 1) ? callerPlayer : HelperMethods.FindTargetPlayer(args.Split(' ')[1]));
			if (player == null)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Ambiguous or invalid target");
				return;
			}
			if (callerPlayer != null && !callerPlayer.CanAdminTarget(player))
			{
				HelperMethods.ReplyToCommand_Player(player, "is immune due to level");
				return;
			}
			if (player.Team == teamByName)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Player already on target team");
				return;
			}
			SwapTeam(player, teamByName);
			HelperMethods.AlertAdminAction(callerPlayer, "swapped " + player.PlayerName + " to " + HelperMethods.GetTeamColor(player) + teamByName.TeamShortName + "</color>");
		}

		public static void SwapTeam(Player player, Team team)
		{
			MelonLogger.Msg("Swapping player (" + player.PlayerName + ") to team: " + team.TeamShortName);
			GameMode.CurrentGameMode.DestroyAllUnitsForPlayer(player);
			player.Team = team;
			NetworkLayer.SendPlayerSelectTeam(player, team);
			GameMode.CurrentGameMode.TrySpawnPlayerAtSpawnPoint(player, team);
		}

		public static bool ProcessJoinTeam<T>(T gameModeInstance, GameByteStreamReader reader, byte rpcIndex) where T : GameModeExt
		{
			if (gameModeInstance == null || reader == null)
			{
				return true;
			}
			if (GameMode.CurrentGameMode.GameOver)
			{
				return true;
			}
			if (GameMode.CurrentGameMode is MP_TowerDefense)
			{
				if (rpcIndex != ERPC_TowerDefense_RequestJoinTeam)
				{
					return true;
				}
			}
			else if (GameMode.CurrentGameMode is MP_Strategy && rpcIndex != ERPC_Strategy_RequestJoinTeam)
			{
				return true;
			}
			ulong num = reader.ReadUInt64();
			NetworkID playerID = new NetworkID(num);
			int num2 = reader.ReadByte();
			Player player = Player.FindPlayer(playerID, num2);
			Team team = reader.ReadTeam();
			if (player == null)
			{
				UnityEngine.Debug.LogError("MP_Strategy::ProcessNetRPC - Player was NULL for REQUEST_JOIN_TEAM, ID: " + num + ", channel: " + num2);
				return false;
			}
			Team team2 = player.Team;
			if (object.Equals(team2, team))
			{
				SendClearRequest(num, num2);
				return false;
			}
			if (_PreventEarlyTeamSwitches.Value && GameMode.CurrentGameMode.GameOngoing && preventTeamSwitches && team2 != null && !JoinCausesImbalance(team2, gameModeInstance))
			{
				if (LastPlayerChatMessage != player)
				{
					HelperMethods.ReplyToCommand_Player(player, "'s switch was denied due to early game team lock");
					LastPlayerChatMessage = player;
				}
				MelonLogger.Msg(player.PlayerName + "'s team switch was denied due to early game team lock");
				SendClearRequest(num, num2);
				return false;
			}
			if (player.Team != null && Event_Roles.GetTeamSetup(gameModeInstance, player.Team) == null)
			{
				MelonLogger.Warning("Found player on invalid team. Allowing role change.");
				player.Team = team;
				NetworkLayer.SendPlayerSelectTeam(player, team);
				return false;
			}
			if (!JoinCausesImbalance(team, gameModeInstance))
			{
				player.Team = team;
				NetworkLayer.SendPlayerSelectTeam(player, team);
				return false;
			}
			if (player.Team == null)
			{
				if (gameModeInstance.GetCommanderForTeam(team) == null && GameMode.CurrentGameMode.GameOngoing)
				{
					if (LastPlayerChatMessage != player)
					{
						HelperMethods.ReplyToCommand_Player(player, "'s switch was denied due to team imbalance");
						LastPlayerChatMessage = player;
					}
					MelonLogger.Msg(player.PlayerName + "'s team switch was denied due to team imbalance");
					SendClearRequest(num, num2);
					return false;
				}
				_ = gameModeInstance.TeamsVersus;
				Team team3 = FindLowestPopulationTeam(gameModeInstance);
				if (team3 != null)
				{
					if (LastPlayerChatMessage != player)
					{
						HelperMethods.ReplyToCommand_Player(player, " was forced to " + HelperMethods.GetTeamColor(team3) + team3.TeamShortName + "</color> to fix imbalance");
						LastPlayerChatMessage = player;
					}
					MelonLogger.Msg(player.PlayerName + " was forced to " + team3.TeamShortName + " to fix imbalance");
					player.Team = team3;
					NetworkLayer.SendPlayerSelectTeam(player, team3);
					return false;
				}
				MelonLogger.Warning("Error in FindLowestPopulationTeam(). Could not find a valid team.");
				SendClearRequest(num, num2);
				return false;
			}
			if (LastPlayerChatMessage != player)
			{
				HelperMethods.ReplyToCommand_Player(player, "'s switch was denied due to imbalance");
				LastPlayerChatMessage = player;
			}
			MelonLogger.Msg(player.PlayerName + "'s team switch was denied due to team imbalance");
			SendClearRequest(num, num2);
			return false;
		}

		public override void OnUpdate()
		{
			try
			{
				if (HelperMethods.IsTimerActive(Timer_AllowTeamSwitches))
				{
					Timer_AllowTeamSwitches += Time.deltaTime;
					if (Timer_AllowTeamSwitches >= (float)_AllowTeamSwitchAfterTime.Value)
					{
						Timer_AllowTeamSwitches = HelperMethods.Timer_Inactive;
						preventTeamSwitches = false;
					}
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run OnUpdate");
			}
		}
	}
}
