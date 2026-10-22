using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using Microsoft.CodeAnalysis;
using Si_AFKManager;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(AwayFromKeyboard), "AFK Manager", "1.3.7", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_AFKManager")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_AFKManager")]
[assembly: AssemblyTitle("Si_AFKManager")]
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
namespace Si_AFKManager
{
	public class AwayFromKeyboard : MelonMod
	{
		public class AFKCount
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

			public uint Minutes { get; set; }
		}

		[HarmonyPatch(typeof(MusicJukeboxHandler), "OnGameStarted")]
		private static class ApplyPatch_MusicJukeboxHandler_OnGameStarted
		{
			public static void Postfix(MusicJukeboxHandler __instance, GameMode __0)
			{
				try
				{
					skippedFirstCheck = false;
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MusicJukeboxHandler::OnGameStarted");
				}
			}
		}

		[HarmonyPatch(typeof(MusicJukeboxHandler), "OnGameEnded")]
		private static class ApplyPatch_MusicJukeboxHandler_OnGameEnded
		{
			public static void Postfix(MusicJukeboxHandler __instance, GameMode __0, Team __1)
			{
				try
				{
					skippedFirstCheck = false;
					AFKTracker.Clear();
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MusicJukeboxHandler::OnGameEnded");
				}
			}
		}

		private static float Timer_AFKCheck = HelperMethods.Timer_Inactive;

		private static bool AdminModAvailable = false;

		private static List<AFKCount> AFKTracker = null;

		private static bool skippedFirstCheck;

		private static MelonPreferences_Category _modCategory = null;

		private static MelonPreferences_Entry<bool> Pref_AFK_KickIfServerNotFull = null;

		private static MelonPreferences_Entry<int> Pref_AFK_MinutesBeforeKick = null;

		public override void OnInitializeMelon()
		{
			if (_modCategory == null)
			{
				_modCategory = MelonPreferences.CreateCategory("Silica");
			}
			if (Pref_AFK_KickIfServerNotFull == null)
			{
				Pref_AFK_KickIfServerNotFull = _modCategory.CreateEntry<bool>("AFK_KickIfServerNotFull", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_AFK_MinutesBeforeKick == null)
			{
				Pref_AFK_MinutesBeforeKick = _modCategory.CreateEntry<int>("AFK_MinutesBeforeKick", 7, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
		}

		[MethodImpl(MethodImplOptions.NoOptimization)]
		public override void OnLateInitializeMelon()
		{
			HelperMethods.StartTimer(ref Timer_AFKCheck);
			AFKTracker = new List<AFKCount>();
			AdminModAvailable = MelonTypeBase<MelonMod>.RegisteredMelons.Any((MelonMod m) => ((MelonBase)m).Info.Name == "Admin Mod");
			HelperMethods.CommandCallback adminCallback = Command_Kick;
			HelperMethods.CommandCallback adminCallback2 = Command_AFK;
			HelperMethods.RegisterAdminCommand("kick", adminCallback, Power.Kick, "Kicks target player. Usage: !kick <player>");
			HelperMethods.RegisterAdminCommand("afk", adminCallback2, Power.Kick, "Kicks any AFK players immediately. Usage: !afk");
		}

		public static bool ServerAlmostFull()
		{
			if (Player.Players.Count + 2 >= NetworkGameServer.GetPlayersMax())
			{
				return true;
			}
			return false;
		}

		public static void Command_Kick(Player? callerPlayer, string args)
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
			else if (HelperMethods.KickPlayer(player))
			{
				HelperMethods.AlertAdminActivity(callerPlayer, player, "kicked");
			}
			else
			{
				HelperMethods.ReplyToCommand_Player(player, "is the host and cannot be targeted");
			}
		}

		public static void Command_AFK(Player? callerPlayer, string args)
		{
			string text = args.Split(' ')[0];
			if (args.Split(' ').Length - 1 > 0)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too many arguments");
			}
			else
			{
				if (Pref_AFK_KickIfServerNotFull == null || Pref_AFK_MinutesBeforeKick == null)
				{
					return;
				}
				if (Pref_AFK_KickIfServerNotFull.Value)
				{
					List<Player> list = new List<Player>();
					foreach (Player player in Player.Players)
					{
						if (player == null || player.Team != null)
						{
							continue;
						}
						int num = AFKTracker.FindIndex((AFKCount p) => p.Player == player);
						if (num >= 0)
						{
							MelonLogger.Msg("Found player " + AFKTracker[num].Player.PlayerName + " AFK for " + AFKTracker[num].Minutes);
							if (AFKTracker[num].Minutes >= Pref_AFK_MinutesBeforeKick.Value)
							{
								list.Add(player);
							}
						}
					}
					if (list.Count > 0)
					{
						foreach (Player playerToKick in list)
						{
							if (!(playerToKick == null))
							{
								HelperMethods.KickPlayer(playerToKick);
								HelperMethods.ReplyToCommand_Player(playerToKick, "was kicked for being AFK");
								int num2 = AFKTracker.FindIndex((AFKCount p) => p.Player == playerToKick);
								if (num2 >= 0)
								{
									AFKTracker.RemoveAt(num2);
								}
							}
						}
						return;
					}
					HelperMethods.ReplyToCommand(text + ": no players were AFK for too long");
				}
				else
				{
					HelperMethods.ReplyToCommand(text + ": server already immediately kicks AFK players");
				}
			}
		}

		public override void OnUpdate()
		{
			try
			{
				if (Pref_AFK_MinutesBeforeKick == null || Pref_AFK_KickIfServerNotFull == null)
				{
					return;
				}
				Timer_AFKCheck += Time.deltaTime;
				if (!(Timer_AFKCheck >= 60f))
				{
					return;
				}
				Timer_AFKCheck = 0f;
				if ((bool)GameMode.CurrentGameMode && !GameMode.CurrentGameMode.GameOngoing)
				{
					return;
				}
				if (!skippedFirstCheck)
				{
					skippedFirstCheck = true;
					return;
				}
				List<Player> list = new List<Player>();
				foreach (Player player in Player.Players)
				{
					if (!(player == null) && !(player.Team == null))
					{
						int num = AFKTracker.FindIndex((AFKCount p) => p.Player == player);
						if (num >= 0)
						{
							MelonLogger.Msg("Removing " + player.PlayerName + " from AFK list for being on a team.");
							AFKTracker.RemoveAt(num);
						}
					}
				}
				foreach (Player player2 in Player.Players)
				{
					if (player2 == null || player2.Team != null)
					{
						continue;
					}
					int num2 = AFKTracker.FindIndex((AFKCount p) => p.Player == player2);
					Player serverPlayer = NetworkGameServer.GetServerPlayer();
					if (player2 == serverPlayer)
					{
						continue;
					}
					if (num2 >= 0)
					{
						AFKTracker[num2].Minutes++;
						if (AFKTracker[num2].Minutes >= Pref_AFK_MinutesBeforeKick.Value)
						{
							if (Pref_AFK_KickIfServerNotFull.Value)
							{
								list.Add(player2);
							}
							else if (ServerAlmostFull())
							{
								list.Add(player2);
							}
						}
					}
					else
					{
						AFKCount aFKCount = new AFKCount();
						aFKCount.Player = player2;
						aFKCount.Minutes = 1u;
						AFKTracker.Add(aFKCount);
					}
				}
				foreach (Player playerToKick in list)
				{
					if (!(playerToKick == null))
					{
						HelperMethods.KickPlayer(playerToKick);
						HelperMethods.ReplyToCommand_Player(playerToKick, "was kicked for being AFK");
						int num3 = AFKTracker.FindIndex((AFKCount p) => p.Player == playerToKick);
						if (num3 >= 0)
						{
							AFKTracker.RemoveAt(num3);
						}
					}
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed in OnUpdate");
			}
		}
	}
}
