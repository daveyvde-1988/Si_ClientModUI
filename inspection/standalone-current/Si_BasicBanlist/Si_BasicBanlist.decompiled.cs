using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using MelonLoader.Utils;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Si_BasicBanlist;
using SilicaAdminMod;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(BasicBanlist), "Basic Banlist", "1.5.9", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_BasicBanlist")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_BasicBanlist")]
[assembly: AssemblyTitle("Si_BasicBanlist")]
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
namespace Si_BasicBanlist
{
	public class BasicBanlist : MelonMod
	{
		public class BanEntry
		{
			public ulong OffenderSteamId { get; set; }

			public string? OffenderName { get; set; }

			public int UnixBanTime { get; set; }

			public string? Comments { get; set; }
		}

		[HarmonyPatch(typeof(NetworkGameServer), "KickPlayer")]
		private static class ApplyPatchKickPlayer
		{
			public static void Prefix(Player __0)
			{
				try
				{
					if (MasterBanList == null)
					{
						return;
					}
					if (!_Pref_Ban_KickButton_PermaBan.Value)
					{
						MelonLogger.Msg("Kicked player name (" + __0.PlayerName + ") SteamID (" + __0.PlayerID.SteamID.m_SteamID + ")");
						return;
					}
					BanEntry thisBan = GenerateBanEntry(__0, NetworkGameServer.GetServerPlayer());
					if (MasterBanList.Find((BanEntry i) => i.OffenderSteamId == thisBan.OffenderSteamId) != null)
					{
						MelonLogger.Msg("Player name (" + thisBan.OffenderName + ") SteamID (" + thisBan.OffenderSteamId + ") already on banlist.");
					}
					else
					{
						MelonLogger.Msg("Added player name (" + thisBan.OffenderName + ") SteamID (" + thisBan.OffenderSteamId + ") to the banlist.");
						MasterBanList.Add(thisBan);
						UpdateBanFile();
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run NetworkGameServer::KickPlayer");
				}
			}
		}

		[HarmonyPatch(typeof(GameMode), "OnPlayerJoinedBase")]
		private static class ApplyPatchOnPlayerJoinedBase
		{
			public static void Postfix(GameMode __instance, Player __0)
			{
				try
				{
					if (MasterBanList != null && __0 != null && MasterBanList.Find((BanEntry i) => i.OffenderSteamId == __0.PlayerID.SteamID.m_SteamID) != null)
					{
						MelonLogger.Msg("Kicking " + __0.ToString() + " for matching an entry in the banlist.");
						NetworkGameServer.KickPlayer(__0);
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run GameMode::OnPlayerJoinedBase");
				}
			}
		}

		private static List<BanEntry>? MasterBanList;

		private static readonly string banListFile = Path.Combine(MelonEnvironment.UserDataDirectory, "banned_users.json");

		private static MelonPreferences_Category _modCategory = null;

		private static MelonPreferences_Entry<bool> _Pref_Ban_KickButton_PermaBan = null;

		public static void UpdateBanFile()
		{
			string contents = JsonConvert.SerializeObject(MasterBanList, Formatting.Indented);
			File.WriteAllText(banListFile, contents);
		}

		public override void OnInitializeMelon()
		{
			try
			{
				if (_modCategory == null)
				{
					_modCategory = MelonPreferences.CreateCategory("Silica");
				}
				if (_Pref_Ban_KickButton_PermaBan == null)
				{
					_Pref_Ban_KickButton_PermaBan = _modCategory.CreateEntry<bool>("Ban_HostKickButton_Permabans", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (File.Exists(banListFile))
				{
					StreamReader streamReader = File.OpenText(banListFile);
					using (streamReader)
					{
						MasterBanList = JsonConvert.DeserializeObject<List<BanEntry>>(streamReader.ReadToEnd());
						if (MasterBanList == null)
						{
							MelonLogger.Msg("Unable to process banned_users.json file. Check syntax.");
						}
						else
						{
							MelonLogger.Msg("Loaded Silica banlist with " + MasterBanList.Count + " entries.");
						}
						return;
					}
				}
				MelonLogger.Msg("Did not find banned_users.json file. Will use ServerSettings.xml file.");
				MasterBanList = null;
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to load Silica banlist (OnInitializeMelon)");
			}
		}

		[MethodImpl(MethodImplOptions.NoOptimization)]
		public override void OnLateInitializeMelon()
		{
			HelperMethods.CommandCallback adminCallback = Command_Ban;
			HelperMethods.RegisterAdminCommand("ban", adminCallback, Power.Ban, "Bans target player. Usage: !ban <player>");
			HelperMethods.RegisterAdminCommand("kickban", adminCallback, Power.Ban, "Bans target player. Usage: !kickban <player>");
			HelperMethods.CommandCallback adminCallback2 = Command_OfflineBan;
			HelperMethods.RegisterAdminCommand("banid", adminCallback2, Power.Rcon, "Bans target player by SteamID. Usage !banid <Steam64ID> <player name>");
			HelperMethods.CommandCallback adminCallback3 = Command_Unban;
			HelperMethods.RegisterAdminCommand("unban", adminCallback3, Power.Unban, "Unbans target player. Usage: !unban <playername | Steam64ID>");
		}

		public static bool IsPlayerBanned(string name)
		{
			if (MasterBanList != null)
			{
				if (MasterBanList.Find((BanEntry i) => i.OffenderName == name) != null)
				{
					return true;
				}
				return false;
			}
			return NetworkServerSettings.PlayerIsBanned(name);
		}

		public static bool IsPlayerBanned(ulong steamID)
		{
			if (MasterBanList != null)
			{
				if (MasterBanList.Find((BanEntry i) => i.OffenderSteamId == steamID) != null)
				{
					return true;
				}
				return false;
			}
			return NetworkServerSettings.PlayerIsBanned(new NetworkID(steamID));
		}

		public static void Command_OfflineBan(Player? callerPlayer, string args)
		{
			string text = args.Split(' ')[0];
			if (args.Split(' ').Length - 1 < 2)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too few arguments");
				return;
			}
			string s = args.Split(' ')[1];
			int num = args.IndexOf(' ', text.Length + 1);
			string text2 = args.Substring(num + 1);
			if (!ulong.TryParse(s, out var result))
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Not a valid SteamID64");
				return;
			}
			if (IsPlayerBanned(result))
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": SteamID64 already found in the banlist");
				return;
			}
			if (MasterBanList == null)
			{
				NetworkServerSettings.PlayerAddBan(new NetworkID(result), text2, 0, (callerPlayer == null) ? "offline banned by SERVER CONSOLE" : ("offline banned by " + callerPlayer.PlayerName));
			}
			else
			{
				BanEntry item = new BanEntry
				{
					OffenderSteamId = result,
					OffenderName = text2,
					UnixBanTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
					Comments = ((callerPlayer == null) ? "offline banned by SERVER CONSOLE" : ("offline banned by " + callerPlayer.PlayerName))
				};
				MasterBanList.Add(item);
				UpdateBanFile();
			}
			MelonLogger.Msg("Added player name (" + text2 + ") SteamID (" + result + ") to the banlist (offline).");
			HelperMethods.AlertAdminAction(callerPlayer, "offline banned " + text2);
		}

		public static void Command_Ban(Player? callerPlayer, string args)
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
			else
			{
				BanPlayer(player, callerPlayer);
			}
		}

		public static void Command_Unban(Player? callerPlayer, string args)
		{
			string text = args.Split(' ')[0];
			if (args.Split(' ').Length - 1 < 1)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too few arguments");
				return;
			}
			string text2 = args.Split(' ', 2)[1];
			if (!UnbanPlayer(text2, callerPlayer))
			{
				HelperMethods.ReplyToCommand(text + ": Unable to find " + text2 + " on banlist");
			}
		}

		public static bool UnbanPlayer(string target, Player? adminPlayer)
		{
			bool flag = ulong.TryParse(target, out var steamid);
			if (MasterBanList == null)
			{
				NetworkBannedPlayer networkBannedPlayer = (flag ? NetworkServerSettings.GetPlayerBan(new NetworkID(steamid)) : NetworkServerSettings.GetPlayerBan(target));
				if (networkBannedPlayer == null)
				{
					return false;
				}
				NetworkServerSettings.PlayerRemoveBan(new NetworkID(steamid));
				MelonLogger.Msg("Removed player name (" + networkBannedPlayer.m_Name + ") SteamID (" + networkBannedPlayer.m_ID + ") from the banlist.");
				HelperMethods.AlertAdminAction(adminPlayer, "unbanned " + networkBannedPlayer.m_Name);
				return true;
			}
			BanEntry banEntry = (flag ? MasterBanList.Find((BanEntry i) => i.OffenderSteamId == steamid) : MasterBanList.Find((BanEntry i) => i.OffenderName == target));
			if (banEntry == null)
			{
				return false;
			}
			MasterBanList.Remove(banEntry);
			UpdateBanFile();
			MelonLogger.Msg("Removed player name (" + banEntry.OffenderName + ") SteamID (" + banEntry.OffenderSteamId + ") from the banlist.");
			HelperMethods.AlertAdminAction(adminPlayer, "unbanned " + banEntry.OffenderName);
			return true;
		}

		public static void BanPlayer(Player playerToBan, Player? adminPlayer)
		{
			if (IsPlayerBanned(playerToBan.PlayerID.SteamID.m_SteamID))
			{
				MelonLogger.Warning("Player name (" + playerToBan.PlayerName + ") SteamID (" + playerToBan.PlayerID.SteamID.m_SteamID + ") already on banlist.");
				return;
			}
			if (MasterBanList == null)
			{
				NetworkServerSettings.PlayerAddBan(playerToBan.PlayerID, playerToBan.PlayerName, 0, (adminPlayer == null) ? "banned by SERVER CONSOLE" : ("banned by " + adminPlayer.PlayerName));
			}
			else
			{
				BanEntry item = GenerateBanEntry(playerToBan, adminPlayer);
				MasterBanList.Add(item);
				UpdateBanFile();
			}
			MelonLogger.Msg("Added player name (" + playerToBan.PlayerName + ") SteamID (" + playerToBan.PlayerID.SteamID.m_SteamID + ") to the banlist.");
			NetworkGameServer.KickPlayer(playerToBan);
			HelperMethods.AlertAdminActivity(adminPlayer, playerToBan, "banned");
		}

		public static BanEntry GenerateBanEntry(Player player, Player? admin)
		{
			BanEntry banEntry = new BanEntry
			{
				OffenderSteamId = player.PlayerID.SteamID.m_SteamID,
				OffenderName = player.PlayerName,
				UnixBanTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds
			};
			if (admin == null)
			{
				banEntry.Comments = "banned by SERVER CONSOLE";
			}
			else
			{
				banEntry.Comments = "banned by " + admin.PlayerName;
			}
			return banEntry;
		}
	}
}
