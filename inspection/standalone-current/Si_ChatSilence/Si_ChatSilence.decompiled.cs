using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using Microsoft.CodeAnalysis;
using Si_ChatSilence;
using SilicaAdminMod;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(ChatSilence), "Chat Silence", "2.0.3", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_ChatSilence")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_ChatSilence")]
[assembly: AssemblyTitle("Si_ChatSilence")]
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
namespace Si_ChatSilence
{
	public class ChatSilence : MelonMod
	{
		[HarmonyPatch(typeof(GameMode), "OnPlayerLeftBase")]
		private static class Silence_Patch_GameMode_OnPlayerLeftBase
		{
			public static void Prefix(GameMode __instance, Player __0)
			{
				try
				{
					if (__0 != null && IsPlayerSilenced(__0))
					{
						silencedPlayers.Remove(__0.PlayerID);
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run GameMode::OnPlayerLeftBase");
				}
			}
		}

		[HarmonyPatch(typeof(NetworkLayer), "RelayVoiceStreamPacket")]
		private static class Mute_Patch_GameMode_RelayVoiceStreamPacket
		{
			public static bool Prefix(byte[] __0, uint __1, ENetworkPacketSend __2, Player __3, bool __4)
			{
				if (IsPlayerMuted(__3))
				{
					return false;
				}
				return true;
			}
		}

		private static List<NetworkID> silencedPlayers;

		private static List<NetworkID> mutedPlayers;

		public override void OnInitializeMelon()
		{
			silencedPlayers = new List<NetworkID>();
			mutedPlayers = new List<NetworkID>();
		}

		public override void OnLateInitializeMelon()
		{
			HelperMethods.CommandCallback adminCallback = Command_Silence;
			HelperMethods.RegisterAdminCommand("silence", adminCallback, Power.Mute, "Prevents target player from sending chat messages. Usage: !silence <player>");
			HelperMethods.CommandCallback adminCallback2 = Command_UnSilence;
			HelperMethods.RegisterAdminCommand("unsilence", adminCallback2, Power.Mute, "Allows target player to send chat messages. Usage: !unsilence <player>");
			HelperMethods.CommandCallback adminCallback3 = Command_Mute;
			HelperMethods.RegisterAdminCommand("mute", adminCallback3, Power.Mute, "Prevents target player from sending voice messages. Usage: !mute <player>");
			HelperMethods.CommandCallback adminCallback4 = Command_UnMute;
			HelperMethods.RegisterAdminCommand("unmute", adminCallback4, Power.Mute, "Allows target player to send voice messages. Usage: !unmute <player>");
			Event_Chat.OnRequestPlayerChat += OnRequestPlayerChat;
		}

		public static void Command_Silence(Player? callerPlayer, string args)
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
			Player player = HelperMethods.FindTargetPlayer(args.Split(' ')[1]);
			if (player == null)
			{
				HelperMethods.ReplyToCommand(text + ": Ambiguous or invalid target");
			}
			else if (callerPlayer != null && !callerPlayer.CanAdminTarget(player))
			{
				HelperMethods.ReplyToCommand_Player(player, "is immune due to level");
			}
			else if (IsPlayerSilenced(player))
			{
				HelperMethods.ReplyToCommand(text + ": Target player already silenced");
			}
			else
			{
				SilencePlayer(player);
				HelperMethods.AlertAdminActivity(callerPlayer, player, "silenced");
			}
		}

		public static void Command_UnSilence(Player? callerPlayer, string args)
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
			Player player = HelperMethods.FindTargetPlayer(args.Split(' ')[1]);
			if (player == null)
			{
				HelperMethods.ReplyToCommand(text + ": Ambiguous or invalid target");
			}
			else if (callerPlayer != null && !callerPlayer.CanAdminTarget(player))
			{
				HelperMethods.ReplyToCommand_Player(player, "is immune due to level");
			}
			else if (IsPlayerSilenced(player))
			{
				UnSilencePlayer(player);
				HelperMethods.AlertAdminActivity(callerPlayer, player, "unsilenced");
			}
			else
			{
				HelperMethods.ReplyToCommand(text + ": Target player not silenced");
			}
		}

		public static bool IsPlayerSilenced(Player player)
		{
			return silencedPlayers.Any((NetworkID s) => s == player.PlayerID);
		}

		public static bool IsSteamSilenced(NetworkID steamID)
		{
			return silencedPlayers.Any((NetworkID s) => s == steamID);
		}

		public static void SilencePlayer(Player playerTarget)
		{
			silencedPlayers.Add(playerTarget.PlayerID);
		}

		public static void UnSilencePlayer(Player playerTarget)
		{
			silencedPlayers.Remove(playerTarget.PlayerID);
		}

		public void OnRequestPlayerChat(object? sender, OnRequestPlayerChatArgs args)
		{
			if (!(args.Player == null))
			{
				MelonLogger.Msg("Checking if player is silenced.");
				if (IsSteamSilenced(args.Player.PlayerID))
				{
					MelonLogger.Msg("Preventing " + args.Player.PlayerName + " from talking in chat.");
					args.Block = true;
				}
			}
		}

		public static void Command_Mute(Player? callerPlayer, string args)
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
			Player player = HelperMethods.FindTargetPlayer(args.Split(' ')[1]);
			if (player == null)
			{
				HelperMethods.ReplyToCommand(text + ": Ambiguous or invalid target");
			}
			else if (callerPlayer != null && !callerPlayer.CanAdminTarget(player))
			{
				HelperMethods.ReplyToCommand_Player(player, "is immune due to level");
			}
			else if (IsPlayerMuted(player))
			{
				HelperMethods.ReplyToCommand(text + ": Target player already muted");
			}
			else
			{
				MutePlayer(player);
				HelperMethods.AlertAdminActivity(callerPlayer, player, "muted");
			}
		}

		public static void Command_UnMute(Player? callerPlayer, string args)
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
			Player player = HelperMethods.FindTargetPlayer(args.Split(' ')[1]);
			if (player == null)
			{
				HelperMethods.ReplyToCommand(text + ": Ambiguous or invalid target");
			}
			else if (callerPlayer != null && !callerPlayer.CanAdminTarget(player))
			{
				HelperMethods.ReplyToCommand_Player(player, "is immune due to level");
			}
			else if (IsPlayerMuted(player))
			{
				UnMutePlayer(player);
				HelperMethods.AlertAdminActivity(callerPlayer, player, "unmuted");
			}
			else
			{
				HelperMethods.ReplyToCommand(text + ": Target player not muted");
			}
		}

		public static bool IsPlayerMuted(Player player)
		{
			return mutedPlayers.Any((NetworkID s) => s == player.PlayerID);
		}

		public static bool IsSteamMuted(NetworkID steamID)
		{
			return mutedPlayers.Any((NetworkID s) => s == steamID);
		}

		public static void MutePlayer(Player playerTarget)
		{
			mutedPlayers.Add(playerTarget.PlayerID);
		}

		public static void UnMutePlayer(Player playerTarget)
		{
			mutedPlayers.Remove(playerTarget.PlayerID);
		}
	}
}
