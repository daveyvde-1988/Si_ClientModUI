using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using MelonLoader.InternalUtils;
using MelonLoader.Preferences;
using MelonLoader.Utils;
using Microsoft.CodeAnalysis;
using Si_Logging;
using Silica;
using SilicaAdminMod;
using Steamworks;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(HL_Logging), "Half-Life Logger", "1.9.14", "databomb&zawedcvg", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_Logging")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+29c72fafdade58e425361c71fdf0f9e62e86e637")]
[assembly: AssemblyProduct("Si_Logging")]
[assembly: AssemblyTitle("Si_Logging")]
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
namespace Si_Logging
{
	public class HL_Logging : MelonMod
	{
		public static class DamageDatabase
		{
			public static void AddDamage(Player victim, Player attacker, float amount)
			{
				int index = victim.GetIndex();
				if (index < 0 || VictimDamage.GetLength(0) <= index)
				{
					MelonLogger.Warning($"Could not add player damage. Attempted to VictimDamage[{index}] with array size: {VictimDamage.GetLength(0)}");
					return;
				}
				if (VictimDamage[index] == null)
				{
					MelonLogger.Msg("Creating new damage database entry for " + victim.PlayerName);
					VictimDamage[index] = new List<AttackerInfo>();
				}
				int num = VictimDamage[index].FindIndex((AttackerInfo info) => info.SteamId == attacker.PlayerID.SteamID.m_SteamID);
				if (num < 0)
				{
					AttackerInfo attackerInfo = new AttackerInfo
					{
						AttackerName = attacker.PlayerName,
						SteamId = attacker.PlayerID.SteamID.m_SteamID,
						TeamIndex = ((attacker.Team == null) ? (-1) : attacker.Team.Index),
						Quantity = 1,
						TotalDamage = amount
					};
					VictimDamage[index].Add(attackerInfo);
					MelonLogger.Msg($"Added new attacker {attackerInfo.AttackerName} to victim index {index}");
				}
				else
				{
					VictimDamage[index][num].TotalDamage += amount;
					VictimDamage[index][num].Quantity++;
				}
			}

			public static void OnPlayerDeath(Player victim)
			{
				int index = victim.GetIndex();
				if (index < 0 || VictimDamage.GetLength(0) <= index)
				{
					MelonLogger.Warning($"Could not print player death stats. Attempted to VictimDamage[{index}] with array size: {VictimDamage.GetLength(0)}");
				}
				else if (VictimDamage[index] != null && VictimDamage[index].Count > 0 && !(victim.Team == null))
				{
					string[] messages = GenerateStats(index, victim.Team.Index);
					HelperMethods.SendConsoleMessageToPlayer(victim, messages);
					ClearIndex(index);
				}
			}

			public static string[] GenerateStats(int index, int teamIndex)
			{
				List<string> list = new List<string>();
				list.Add(Header());
				foreach (AttackerInfo item in VictimDamage[index])
				{
					string consoleColorCode = GetConsoleColorCode(teamIndex, item.TeamIndex);
					list.Add(string.Format("{0}{1} caused {2} dmg in {3} {4}</color>", consoleColorCode, item.AttackerName, item.TotalDamage.ToString("#.#"), item.Quantity, (item.Quantity > 1) ? "hits" : "hit"));
				}
				list.Add(Footer());
				return list.ToArray();
			}

			public static string GetConsoleColorCode(int victimTeamIndex, int attackerTeamIndex)
			{
				if (victimTeamIndex == attackerTeamIndex)
				{
					return "<color=#FFABAB>";
				}
				return "<color=#FFFFFF>";
			}

			public static string Header()
			{
				return "<color=#FFFFFF>---------- <b>PvP Damage Report</b> ----------</color>";
			}

			public static string Footer()
			{
				return "<color=#FFFFFF>-----------------------------------------------------</color>";
			}

			public static void ClearIndex(int i)
			{
				if (VictimDamage[i] != null)
				{
					VictimDamage[i].Clear();
				}
			}

			public static void ResetRound()
			{
				for (int i = 0; i < VictimDamage.Length; i++)
				{
					ClearIndex(i);
				}
			}

			static DamageDatabase()
			{
				VictimDamage = new List<AttackerInfo>[66];
			}
		}

		public class AttackerInfo
		{
			private string _attackerName;

			public string AttackerName
			{
				get
				{
					return _attackerName;
				}
				set
				{
					_attackerName = value ?? throw new ArgumentNullException("Attacker name is required.");
				}
			}

			public ulong SteamId { get; set; }

			public int TeamIndex { get; set; }

			public int Quantity { get; set; }

			public float TotalDamage { get; set; }
		}

		[HarmonyPatch(typeof(NetworkGameServer), "OnP2PSessionRequest")]
		private static class ApplyPatchOnP2PSessionRequest
		{
			public static void Postfix(NetworkGameServer __instance, P2PSessionRequest_t __0)
			{
				try
				{
					PrintLogLine(GetConnectionString(__0) + " connected, address \"127.0.0.1:27015\"");
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run OnP2PSessionRequest");
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
					if (!(__0 == null))
					{
						PrintLogLine(AddPlayerLogEntry(__0) + " entered the game");
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run OnPlayerJoinedBase");
				}
			}
		}

		[HarmonyPatch(typeof(GameMode), "OnPlayerLeftBase")]
		private static class ApplyPatchOnPlayerLeftBase
		{
			public static void Prefix(GameMode __instance, Player __0)
			{
				try
				{
					if (!(__0 == null))
					{
						PrintLogLine(AddPlayerLogEntry(__0) + " disconnected");
						if (Pref_Log_PlayerConsole_Enable.Value)
						{
							string text = AddPlayerConsoleEntry(__0);
							HelperMethods.SendConsoleMessage(text + " disconnected.");
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run OnPlayerLeftBase");
				}
			}
		}

		[HarmonyPatch(typeof(NetworkGameServer), "KickPlayer")]
		private static class ApplyPatchKickPlayer
		{
			public static void Postfix(Player __0)
			{
				try
				{
					string text = AddPlayerLogEntry(__0);
					PrintLogLine("Kick: " + text + " was kicked by \"Console\" (message \"\")");
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run KickPlayer");
				}
			}
		}

		[HarmonyPatch(typeof(MP_Strategy), "OnUnitDestroyed")]
		private static class ApplyPatch_MP_Strategy_OnUnitDestroyed
		{
			public static void Postfix(MP_Strategy __instance, Unit __0, GameObject __1)
			{
				try
				{
					if (__0 == null || __1 == null)
					{
						return;
					}
					BaseGameObject baseGameObject = __1.GetBaseGameObject();
					if (baseGameObject == null)
					{
						return;
					}
					NetworkComponent networkComponent = baseGameObject.NetworkComponent;
					if (networkComponent == null)
					{
						MelonLogger.Warning("Found null attackerNetComp in ApplyPatch_MP_Strategy_OnUnitDestroyed");
						return;
					}
					Player ownerPlayer = networkComponent.OwnerPlayer;
					Player controlledBy = __0.ControlledBy;
					bool flag = controlledBy != null;
					bool flag2 = ownerPlayer != null;
					if (flag)
					{
						DamageDatabase.OnPlayerDeath(controlledBy);
						if (flag2)
						{
							if (ownerPlayer == controlledBy)
							{
								string text = AddPlayerLogEntry(controlledBy);
								string nameFromObject = GetNameFromObject(__1);
								string playerPosition = GetPlayerPosition(__0);
								PrintLogLine(text + " committed suicide with \"" + nameFromObject + "\" (dmgtype \"\") " + playerPosition);
								if (Pref_Log_PlayerConsole_Enable.Value)
								{
									string text2 = AddPlayerConsoleEntry(controlledBy);
									HelperMethods.SendConsoleMessage(text2 + " (" + nameFromObject + ") committed suicide");
								}
								return;
							}
							string text3 = AddPlayerLogEntry(ownerPlayer);
							string text4 = AddPlayerLogEntry(controlledBy);
							string text5 = AddKilledWithEntry(__0, __1);
							string playerPosition2 = GetPlayerPosition(__0, __1);
							PrintLogLine(text3 + " killed " + text4 + " with " + text5 + " " + playerPosition2);
							if (Pref_Log_PlayerConsole_Enable.Value)
							{
								string text6 = AddPlayerConsoleEntry(ownerPlayer);
								string text7 = AddPlayerConsoleEntry(controlledBy);
								string nameFromObject2 = GetNameFromObject(__1);
								string nameFromUnit = GetNameFromUnit(__0);
								HelperMethods.SendConsoleMessage(text6 + " (" + nameFromObject2 + ") killed " + text7 + " (" + nameFromUnit + ")");
							}
						}
						else if (Pref_Log_Kills_Include_AI_vs_Player.Value)
						{
							string text8 = AddAIAttackerLogEntry(__1, baseGameObject.Team);
							string text9 = AddPlayerLogEntry(controlledBy);
							string text10 = AddKilledWithEntry(__0, __1);
							string nameFromUnit2 = GetNameFromUnit(__0);
							string playerPosition3 = GetPlayerPosition(__0, __1);
							PrintLogLine(text8 + " killed " + text9 + " with " + text10 + " " + playerPosition3);
							if (Pref_Log_PlayerConsole_Enable.Value)
							{
								string text11 = AddAIConsoleEntry();
								string nameFromObject3 = GetNameFromObject(__1);
								string text12 = AddPlayerConsoleEntry(controlledBy);
								HelperMethods.SendConsoleMessage(text11 + " (" + nameFromObject3 + ") killed " + text12 + " (" + nameFromUnit2 + ")");
							}
						}
					}
					else if (flag2 && Pref_Log_Kills_Include_AI_vs_Player.Value)
					{
						string text13 = AddPlayerLogEntry(ownerPlayer);
						string text14 = AddAIVictimLogEntry(__0);
						string text15 = AddKilledWithEntry(__0, __1);
						string playerPosition4 = GetPlayerPosition(__0, __1);
						PrintLogLine(text13 + " killed " + text14 + " with " + text15 + " " + playerPosition4);
						if (Pref_Log_PlayerConsole_Enable.Value)
						{
							string text16 = AddAIConsoleEntry();
							string text17 = AddPlayerConsoleEntry(ownerPlayer);
							string nameFromObject4 = GetNameFromObject(__1);
							string nameFromUnit3 = GetNameFromUnit(__0);
							HelperMethods.SendConsoleMessage(text17 + " (" + nameFromObject4 + ") killed " + text16 + " (" + nameFromUnit3 + ")");
						}
					}
					else if (__0.ObjectInfo.Critical)
					{
						string text18 = AddAIAttackerLogEntry(__1, baseGameObject.Team);
						string text19 = AddAIVictimLogEntry(__0);
						string text20 = AddKilledWithEntry(__0, __1);
						string playerPosition5 = GetPlayerPosition(__0, __1);
						PrintLogLine(text18 + " killed " + text19 + " with " + text20 + " " + playerPosition5);
						if (Pref_Log_PlayerConsole_Enable.Value)
						{
							string text21 = AddAIConsoleEntry();
							string nameFromObject5 = GetNameFromObject(__1);
							string nameFromUnit4 = GetNameFromUnit(__0);
							HelperMethods.SendConsoleMessage(text21 + " (" + nameFromObject5 + ") killed " + text21 + " (" + nameFromUnit4 + ")");
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run OnUnitDestroyed");
				}
			}
		}

		[HarmonyPatch(typeof(MP_TowerDefense), "OnPlayerChangedTeam")]
		private static class ApplyPatch_MPTowerDefense_OnPlayerChangedTeam
		{
			public static void Postfix(MP_TowerDefense __instance, Player __0, Team __1, Team __2)
			{
				try
				{
					ProcessOnPlayerChangedTeam(__0, __1, __2);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_TowerDefense::OnPlayerChangedTeam");
				}
			}
		}

		[HarmonyPatch(typeof(MP_Strategy), "OnPlayerChangedTeam")]
		private static class ApplyPatch_MPStrategy_OnPlayerChangedTeam
		{
			public static void Postfix(MP_Strategy __instance, Player __0, Team __1, Team __2)
			{
				try
				{
					ProcessOnPlayerChangedTeam(__0, __1, __2);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_Strategy::OnPlayerChangedTeam");
				}
			}
		}

		[HarmonyPatch(typeof(NetworkLayer), "SendPlayerChangeName")]
		private static class ApplyPatchSendPlayerChangeName
		{
			public static void Postfix(NetworkID __0, int __1, string __2)
			{
				try
				{
					string playerID = GetPlayerID(__0);
					PrintLogLine("\"...<><" + playerID + "><>\" changed name to \"" + __2 + "\"");
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run SendPlayerChangeName");
				}
			}
		}

		[HarmonyPatch(typeof(DamageManager), "OnDamageReceived")]
		private static class ApplyPatchOnDamageReceived
		{
			public static void Postfix(DamageManager __instance, float __0, GameObject __1, byte __2, bool __3)
			{
				if ((!Pref_Log_Damage.Value && !Pref_Display_Damage.Value) || !ShouldHandleDamage(__0))
				{
					return;
				}
				Player player = GetPlayer(__1);
				Player player2 = GetPlayer(__instance);
				if (!(player == null) && !(player2 == null))
				{
					if (Pref_Display_Damage.Value)
					{
						DamageDatabase.AddDamage(player2, player, __0);
					}
					if (Pref_Log_Damage.Value)
					{
						AddPlayerLogEntry(player);
						AddPlayerLogEntry(player2);
						string nameFromObject = GetNameFromObject(__1);
						PrintLogLine(string.Format("{0} attacked {1} with \"{2}\" (damage \"{3}\")", player, player2, nameFromObject, __0.ToString("#.#")), suppressConsoleOutput: true);
					}
				}
			}
		}

		[HarmonyPatch(typeof(Player), "OnTargetDestroyed")]
		private static class ApplyPatchOnTargetDestroyed
		{
			public static void Postfix(Target __0, GameObject __1)
			{
				try
				{
					if (__0 == null || __1 == null)
					{
						return;
					}
					BaseGameObject baseGameObject = __1.GetBaseGameObject();
					if (baseGameObject == null)
					{
						return;
					}
					NetworkComponent networkComponent = baseGameObject.NetworkComponent;
					if (networkComponent == null)
					{
						return;
					}
					Player ownerPlayer = networkComponent.OwnerPlayer;
					bool flag = ownerPlayer != null;
					if (!(__0.Team == null) && !(baseGameObject.Team == null) && !(__0.ObjectInfo == null) && __0.ObjectInfo.ObjectType == ObjectInfoType.Structure)
					{
						string text = (flag ? AddPlayerLogEntry(ownerPlayer) : AddAIAttackerLogEntry(__1, baseGameObject.Team));
						string structureName = GetStructureName(__0);
						string teamShortName = __0.Team.TeamShortName;
						string nameFromObject = GetNameFromObject(__1);
						string text2 = ((__0.OwnerConstructionSite == null) ? "no" : "yes");
						string logPosition = GetLogPosition(__0.gameObject.transform.position);
						string logPosition2 = GetLogPosition(__1.transform.position);
						PrintLogLine(text + " triggered \"structure_kill\" (structure \"" + structureName + "\") (weapon \"" + nameFromObject + "\") (struct_team \"" + teamShortName + "\") (construction \"" + text2 + "\") (attacker_position \"" + logPosition2 + "\") (building_position \"" + logPosition + "\")");
						if (Pref_Log_PlayerConsole_Enable.Value)
						{
							string text3 = (flag ? AddPlayerConsoleEntry(ownerPlayer) : AddAIConsoleEntry());
							string text4 = ((__0.OwnerConstructionSite == null) ? "structure" : "construction site");
							HelperMethods.SendConsoleMessageToTeam(baseGameObject.Team, text3 + " (" + nameFromObject + ") destroyed a " + text4 + " (" + HelperMethods.GetTeamColor(__0.Team) + structureName + "</color>)");
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run OnTargetDestroyed");
				}
			}
		}

		[HarmonyPatch(typeof(MP_Strategy), "OnStructureDestroyed")]
		private static class ApplyPatchOnStructureDestroyed
		{
			public static void Postfix(MP_Strategy __instance, Structure __0, GameObject __1)
			{
				try
				{
					if (!(__0 == null) && __0.Team != null)
					{
						Team team = __0.Team;
						int highestTechTier = getHighestTechTier(team);
						if (highestTechTier != currentTechTier[team.Index])
						{
							currentTechTier[team.Index] = highestTechTier;
							LogTierChange(team, highestTechTier);
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run OnStructureDestroyed");
				}
			}
		}

		[HarmonyPatch(typeof(Team), "OnResourcesChanged")]
		private static class ApplyPatchOnResourcesChanged
		{
			public static void Postfix(Team __instance, Structure __0, ResourceHolder __1, int __2)
			{
				try
				{
					if (!(__instance == null))
					{
						if (__2 <= 0)
						{
							teamResourcesSpent[__instance.Index] -= __2;
						}
						else
						{
							teamResourcesCollected[__instance.Index] += __2;
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run OnResourcesChanged");
				}
			}
		}

		[HarmonyPatch(typeof(ConstructionData), "RequestConstructionSite")]
		private static class ApplyPatchRequestConstructionSite
		{
			public static void Postfix(ConstructionData __instance, ConstructionSite __result, Structure __0, Vector3 __1, Quaternion __2, Team __3, Transform __4, float __5)
			{
				if (!(__instance == null) && !(__result == null) && __instance.ObjectInfo.ObjectType == ObjectInfoType.Structure)
				{
					string teamName = GetTeamName(__3);
					string structureName = GetStructureName(__result);
					string logPosition = GetLogPosition(__1);
					PrintLogLine("Team \"" + teamName + "\" triggered \"construction_start\" (building_name \"" + structureName + "\") (building_position \"" + logPosition + "\")");
				}
			}
		}

		[HarmonyPatch(typeof(ConstructionSite), "SpawnObject")]
		private static class ApplyPatchConstructionSpawned
		{
			public static void Postfix(ConstructionSite __instance)
			{
				if (!(__instance == null) && __instance.ObjectInfo.ObjectType == ObjectInfoType.Structure)
				{
					string teamName = GetTeamName(__instance.Team);
					string structureName = GetStructureName(__instance);
					string logPosition = GetLogPosition(__instance.transform.position);
					PrintLogLine("Team \"" + teamName + "\" triggered \"construction_complete\" (building_name \"" + structureName + "\") (building_position \"" + logPosition + "\")");
				}
			}
		}

		[HarmonyPatch(typeof(MusicJukeboxHandler), "OnGameEnded")]
		private static class ApplyPatchOnGameEnded
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
					if (VictimDamage != null)
					{
						DamageDatabase.ResetRound();
					}
					GameModeExt gameModeExt = UnityEngine.Object.FindFirstObjectByType<GameModeExt>();
					if (gameModeExt == null)
					{
						return;
					}
					string gameType = GetGameType(gameModeExt);
					if (__1 == null)
					{
						string gameMode = GetGameMode();
						PrintLogLine("World triggered \"Round_Win\" (gamemode \"" + gameMode + "\") (gametype \"" + gameType + "\")");
						if (Pref_Log_PlayerConsole_Enable.Value)
						{
							HelperMethods.SendConsoleMessage("<b>Round is over. Game is a draw.</b>");
						}
						return;
					}
					string teamName = GetTeamName(__1);
					PrintLogLine("Team \"" + teamName + "\" triggered \"Victory\"");
					if (Pref_Log_PlayerConsole_Enable.Value)
					{
						HelperMethods.SendConsoleMessage("<b>Team " + HelperMethods.GetTeamColor(__1) + teamName + "</color> is victorious!</b>");
					}
					GameModeExt.ETeamsVersus teamsVersus = gameModeExt.TeamsVersus;
					for (int i = 0; i < 8; i++)
					{
						if (i != 1 && i != 2)
						{
							Team team = Team.Teams[i];
							if (!(team == null) && !team.IsSpecial && (teamsVersus != GameModeExt.ETeamsVersus.HUMANS_VS_HUMANS || i != 0) && (teamsVersus != GameModeExt.ETeamsVersus.HUMANS_VS_ALIENS || i != 3))
							{
								string text = teamResourcesCollected[team.Index].ToString();
								string text2 = team.GetNumPlayers().ToString();
								teamName = GetTeamName(i);
								PrintLogLine("Team \"" + teamName + "\" scored \"" + text + "\" with \"" + text2 + "\" players");
							}
						}
					}
					int num = 0;
					for (int j = 0; j < Player.Players.Count; j++)
					{
						if (!(Player.Players[j] == null))
						{
							Player player = Player.Players[j];
							string text3 = AddPlayerLogEntry(player);
							PrintLogLine($"Player {text3} scored \"{player.Score}\" (kills \"{player.Kills}\") (deaths \"{player.Deaths}\")");
							num++;
						}
					}
					PrintLogLine("World triggered \"Round_Win\" (gametype \"" + gameType + "\")");
					if (ParserExePresent())
					{
						MelonLogger.Msg("Launching parser.");
						ProcessStartInfo processStartInfo = new ProcessStartInfo
						{
							FileName = Path.GetFullPath(GetParserPath())
						};
						string arguments = $"\"{GetLogFileDirectory()}";
						processStartInfo.Arguments = arguments;
						processStartInfo.UseShellExecute = false;
						processStartInfo.RedirectStandardOutput = false;
						using (Process.Start(processStartInfo))
						{
						}
					}
					else
					{
						MelonLogger.Msg("Parser file not present.");
					}
					if (VideoGeneratorExePresent())
					{
						if (num >= Pref_Log_VideoPlayerThreshold.Value)
						{
							MelonLogger.Msg("Launching video generator.");
							ProcessStartInfo processStartInfo2 = new ProcessStartInfo
							{
								FileName = Path.GetFullPath(GetVideoGeneratorPath())
							};
							string arguments2 = $"\"{GetLogFileDirectory()}";
							processStartInfo2.Arguments = arguments2;
							processStartInfo2.UseShellExecute = false;
							processStartInfo2.RedirectStandardOutput = false;
							using (Process.Start(processStartInfo2))
							{
								return;
							}
						}
						MelonLogger.Msg("Video generator not called due to low player count.");
					}
					else
					{
						MelonLogger.Msg("Video generator file not present.");
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run OnGameEnded");
				}
			}
		}

		[HarmonyPatch(typeof(GameplayEventsHandler), "OnConstructionCompleted")]
		private static class ApplyPatchOnSetTechnologyTier
		{
			public static void Postfix(ConstructionSite constructionSite, bool wasCompleted)
			{
				try
				{
					Team team = constructionSite.Team;
					int highestTechTier = getHighestTechTier(team);
					if (highestTechTier != currentTechTier[team.Index])
					{
						currentTechTier[team.Index] = highestTechTier;
						LogTierChange(team, highestTechTier);
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run GameplayEventsHandler::OnConstructionCompleted");
				}
			}
		}

		[HarmonyPatch(typeof(ResourceArea), "ExtractResource", new Type[]
		{
			typeof(int),
			typeof(int),
			typeof(int)
		})]
		private static class ApplyPatchExtractResource
		{
			public static void Postfix(ResourceArea __instance, int __result, int __0, int __1, int __2)
			{
				try
				{
					if (__instance.ResourceAmountCurrent <= 0)
					{
						string logPosition = GetLogPosition(__instance.transform.position);
						string logResourceType = GetLogResourceType(__instance.ResourceType);
						PrintLogLine("World triggered \"Resource_Depleted\" (type \"" + logResourceType + "\") (position \"" + logPosition + "\")");
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run ResourceArea::ExtractResource");
				}
			}
		}

		[HarmonyPatch(typeof(MusicJukeboxHandler), "OnGameStarted")]
		private static class ApplyPatchOnGameStarted
		{
			public static void Prefix(MusicJukeboxHandler __instance, GameMode __0)
			{
				try
				{
					GameModeExt gameModeInstance = UnityEngine.Object.FindFirstObjectByType<GameModeExt>();
					firedRoundEndOnce = false;
					string gameMode = GetGameMode();
					string gameType = GetGameType(gameModeInstance);
					PrintLogLine("World triggered \"Round_Start\" (gamemode \"" + gameMode + "\") (gametype \"" + gameType + "\")");
					LogStartingResources();
					LogStartingStructures();
					initializeRound(ref currentTechTier);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run OnGameStarted");
				}
			}
		}

		public static List<AttackerInfo>[] VictimDamage = null;

		private static string CurrentLogFile = GetLogFilePath();

		public static float Timer_PerfMonitorLog = HelperMethods.Timer_Inactive;

		private static int[] teamResourcesCollected = new int[9];

		private static int[] teamResourcesSpent = new int[9];

		private static int[] currentTechTier = new int[9];

		private static Player?[]? lastCommander;

		private static MelonPreferences_Category _modCategory = null;

		public static MelonPreferences_Entry<bool> Pref_Log_Damage = null;

		public static MelonPreferences_Entry<bool> Pref_Display_Damage = null;

		public static MelonPreferences_Entry<bool> Pref_Log_Kills_Include_AI_vs_Player = null;

		public static MelonPreferences_Entry<string> Pref_Log_ParserExe = null;

		public static MelonPreferences_Entry<string> Pref_Log_VideoExe = null;

		public static MelonPreferences_Entry<int> Pref_Log_VideoPlayerThreshold = null;

		public static MelonPreferences_Entry<float> Pref_Log_PerfMonitor_Interval = null;

		public static MelonPreferences_Entry<bool> Pref_Log_PerfMonitor_Enable = null;

		public static MelonPreferences_Entry<bool> Pref_Log_PlayerConsole_Enable = null;

		public static MelonPreferences_Entry<int> Pref_Log_MinDamageCutoff = null;

		public static float Timer_ResourceLogEvent = HelperMethods.Timer_Inactive;

		private static bool firedRoundEndOnce;

		public static string GetLogPrefix()
		{
			return "L " + DateTime.Now.ToString("MM/dd/yyyy - HH:mm:ss: ");
		}

		public static int GetUserId(Player? player)
		{
			if (player == null)
			{
				return -1;
			}
			return Math.Abs(player.GetInstanceID());
		}

		public static string GetPlayerID(Player player)
		{
			return player.PlayerID.SteamID.m_SteamID.ToString();
		}

		public static string GetPlayerID(NetworkID networkID)
		{
			return networkID.SteamID.ToString();
		}

		public static string GetGameMode()
		{
			return GameMode.CurrentGameMode.ToString().Split(' ')[0];
		}

		public static string GetGameType(GameModeExt gameModeInstance)
		{
			return gameModeInstance.TeamsVersus.ToString();
		}

		public static string GetStructureName(Structure structure)
		{
			if (structure == null || structure.ObjectInfo == null)
			{
				return "";
			}
			return GetLogNameFromDisplayName(structure.ObjectInfo.DisplayName);
		}

		public static string GetStructureName(Target target)
		{
			if (target == null || target.ObjectInfo == null)
			{
				return "";
			}
			return GetLogNameFromDisplayName(target.ObjectInfo.DisplayName);
		}

		public static string GetStructureName(ConstructionSite site)
		{
			if (site == null || site.ObjectInfo == null)
			{
				return "";
			}
			return GetLogNameFromDisplayName(site.ObjectInfo.DisplayName);
		}

		public static string GetLogPosition(Vector3 position)
		{
			string text = position.x.ToString("F0");
			string text2 = position.z.ToString("F0");
			string text3 = position.y.ToString("F0");
			return text + " " + text2 + " " + text3;
		}

		public static string GetLogResourceType(Resource resourceType)
		{
			return resourceType.ToString().Split('(')[0].Split('_')[1].TrimEnd();
		}

		public static string GetPlayerPosition(Unit victim)
		{
			string logPosition = GetLogPosition(victim.transform.position);
			return "(victim_position \"" + logPosition + "\")";
		}

		public static string GetPlayerPosition(Unit victim, GameObject attacker)
		{
			string logPosition = GetLogPosition(victim.transform.position);
			string logPosition2 = GetLogPosition(attacker.transform.position);
			return "(attacker_position \"" + logPosition2 + "\") (victim_position \"" + logPosition + "\")";
		}

		public static string GetLogNameFromDisplayName(string displayName)
		{
			return displayName.Replace(" ", "").Replace("-", "");
		}

		public static string GetPlayerID(P2PSessionRequest_t session)
		{
			return session.m_steamIDRemote.ToString();
		}

		public static string GetNameFromObject(GameObject gameObject)
		{
			return GetLogNameFromDisplayName(gameObject.GetBaseGameObject().ObjectInfo.DisplayName);
		}

		public static string GetNameFromUnit(Unit unit)
		{
			return unit.ToString().Split('(')[0];
		}

		public static string GetConnectionString(P2PSessionRequest_t session)
		{
			string playerID = GetPlayerID(session);
			return "\"...<><" + playerID + "><>\"";
		}

		public static string AddKilledWithEntry(Unit unit, GameObject killerObject)
		{
			string nameFromObject = GetNameFromObject(killerObject);
			string nameFromUnit = GetNameFromUnit(unit);
			return "\"" + nameFromObject + "\" (dmgtype \"\") (victim \"" + nameFromUnit + "\")";
		}

		public static string AddAIVictimLogEntry(Unit unit)
		{
			string nameFromUnit = GetNameFromUnit(unit);
			string teamName = GetTeamName(unit.Team);
			return "\"" + nameFromUnit + "<><><" + teamName + ">\"";
		}

		public static string AddAIAttackerLogEntry(GameObject gameObject, Team team)
		{
			string nameFromObject = GetNameFromObject(gameObject);
			string teamName = GetTeamName(team);
			return "\"" + nameFromObject + "<><><" + teamName + ">\"";
		}

		public static string GetTeamName(Player player)
		{
			if (player.Team == null)
			{
				return string.Empty;
			}
			return player.Team.TeamShortName;
		}

		public static string GetTeamName(Team team)
		{
			if (team == null)
			{
				return string.Empty;
			}
			return team.TeamShortName;
		}

		public static string GetTeamName(int index)
		{
			switch (index)
			{
			default:
				return string.Empty;
			case 0:
				return "Alien";
			case 1:
				return "Wildlife";
			case 2:
				return "Gamemaster";
			case 3:
				return "Centauri";
			case 4:
				if (index == 4)
				{
					return "Sol";
				}
				return string.Empty;
			}
		}

		public static string AddAIConsoleEntry()
		{
			return "<b>AI</b>";
		}

		public static string AddPlayerLogEntry(Player? player)
		{
			if (player == null)
			{
				return string.Empty;
			}
			int userId = GetUserId(player);
			string playerID = GetPlayerID(player);
			string teamName = GetTeamName(player);
			return $"\"{player.PlayerName}<{userId}><{playerID}><{teamName}>\"";
		}

		public static string AddPlayerLogEntry(Player? player, string teamOverride)
		{
			if (player == null)
			{
				return string.Empty;
			}
			int userId = GetUserId(player);
			string playerID = GetPlayerID(player);
			return $"\"{player.PlayerName}<{userId}><{playerID}><{teamOverride}>\"";
		}

		public static string AddPlayerConsoleEntry(Player? player)
		{
			if (player == null)
			{
				return string.Empty;
			}
			return "<b>" + HelperMethods.GetTeamColor(player) + player.PlayerName + "</color></b>";
		}

		public static string GetRoleName(GameModeExt.ETeamRole role)
		{
			return role switch
			{
				GameModeExt.ETeamRole.COMMANDER => "Commander", 
				GameModeExt.ETeamRole.UNIT => "Infantry", 
				_ => "None", 
			};
		}

		public static bool ParserExePresent()
		{
			return File.Exists(GetParserPath());
		}

		public static string GetParserPath()
		{
			return Path.Combine(MelonEnvironment.UserDataDirectory, Pref_Log_ParserExe.Value);
		}

		public static bool VideoGeneratorExePresent()
		{
			return File.Exists(GetVideoGeneratorPath());
		}

		public static string GetVideoGeneratorPath()
		{
			return Path.Combine(MelonEnvironment.UserDataDirectory, Pref_Log_VideoExe.Value);
		}

		public static void PrintLogLine(string LogMessage, bool suppressConsoleOutput = false)
		{
			if (LogMessage != null)
			{
				string logFilePath = GetLogFilePath();
				if (logFilePath != CurrentLogFile)
				{
					File.AppendAllText(CurrentLogFile, GetLogPrefix() + "Log file closed" + Environment.NewLine);
					CurrentLogFile = logFilePath;
					AddFirstLogLine();
				}
				string text = GetLogPrefix() + LogMessage;
				if (!suppressConsoleOutput)
				{
					MelonLogger.Msg(text);
				}
				File.AppendAllText(CurrentLogFile, text + Environment.NewLine);
			}
		}

		public static void AddFirstLogLine()
		{
			string text = "Log file started (file \"" + GetLogSubPath() + "\") (game \"" + MelonEnvironment.GameExecutablePath + "\") (version \"" + UnityInformationHandler.GameVersion + "\") (hostid \"" + NetworkGameServer.GetServerID().ToString() + "\")";
			File.AppendAllText(CurrentLogFile, GetLogPrefix() + text + Environment.NewLine);
		}

		public static string GetLogFileDirectory()
		{
			return Path.Combine(MelonEnvironment.UserDataDirectory, "logs\\");
		}

		public static string GetLogSubPath()
		{
			return "logs\\" + GetLogName();
		}

		public static string GetLogFilePath()
		{
			string logSubPath = GetLogSubPath();
			return Path.Combine(MelonEnvironment.UserDataDirectory, logSubPath);
		}

		public static string GetLogName()
		{
			return "L" + DateTime.Now.ToString("yyyyMMdd") + ".log";
		}

		public static void TrackPerformanceMonitor()
		{
			if (!Pref_Log_PerfMonitor_Enable.Value)
			{
				return;
			}
			Timer_PerfMonitorLog += Time.deltaTime;
			if (Timer_PerfMonitorLog >= Pref_Log_PerfMonitor_Interval.Value)
			{
				Timer_PerfMonitorLog = 0f;
				if (Player.Players.Count > 0)
				{
					ServerPerfLogger.CapturePerformancePoint();
				}
			}
		}

		public override void OnInitializeMelon()
		{
			try
			{
				if (_modCategory == null)
				{
					_modCategory = MelonPreferences.CreateCategory("Silica");
				}
				if (Pref_Log_Damage == null)
				{
					Pref_Log_Damage = _modCategory.CreateEntry<bool>("Logging_LogDamage", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Display_Damage == null)
				{
					Pref_Display_Damage = _modCategory.CreateEntry<bool>("Logging_DisplayDamage", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Log_MinDamageCutoff == null)
				{
					Pref_Log_MinDamageCutoff = _modCategory.CreateEntry<int>("Logging_LogDamage_MinDmgCutoff", 1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Log_Kills_Include_AI_vs_Player == null)
				{
					Pref_Log_Kills_Include_AI_vs_Player = _modCategory.CreateEntry<bool>("Logging_LogKills_IncludeAIvsPlayer", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Log_ParserExe == null)
				{
					Pref_Log_ParserExe = _modCategory.CreateEntry<string>("Logging_ParserExePath", "parser.exe", (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Log_VideoExe == null)
				{
					Pref_Log_VideoExe = _modCategory.CreateEntry<string>("Logging_Video_ExePath", "video-generator.exe", (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Log_VideoPlayerThreshold == null)
				{
					Pref_Log_VideoPlayerThreshold = _modCategory.CreateEntry<int>("Logging_Video_MinimumPlayersNeeded", 2, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Log_PerfMonitor_Interval == null)
				{
					Pref_Log_PerfMonitor_Interval = _modCategory.CreateEntry<float>("Logging_PerfMonitor_LogInterval", 60f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Log_PerfMonitor_Enable == null)
				{
					Pref_Log_PerfMonitor_Enable = _modCategory.CreateEntry<bool>("Logging_PerfMonitor_Enable", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (Pref_Log_PlayerConsole_Enable == null)
				{
					Pref_Log_PlayerConsole_Enable = _modCategory.CreateEntry<bool>("Logging_PlayerConsole_Enable", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
				}
				if (!Directory.Exists(GetLogFileDirectory()))
				{
					MelonLogger.Msg("Creating log file directory at: " + GetLogFileDirectory());
					Directory.CreateDirectory(GetLogFileDirectory());
				}
				if (!File.Exists(CurrentLogFile))
				{
					AddFirstLogLine();
				}
				lastCommander = new Player[8];
				for (int i = 0; i < 8; i++)
				{
					lastCommander[i] = null;
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to initialize log directories or files");
			}
		}

		[MethodImpl(MethodImplOptions.NoOptimization)]
		public override void OnLateInitializeMelon()
		{
			HelperMethods.StartTimer(ref Timer_PerfMonitorLog);
			HelperMethods.StartTimer(ref Timer_ResourceLogEvent);
			Event_Roles.OnRoleChanged += OnRoleChanged;
			Event_Chat.OnRequestPlayerChat += OnRequestPlayerChat;
			Event_Structures.OnCommanderSoldStructure += OnCommanderSoldStructure_Log;
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			try
			{
				switch (sceneName)
				{
				case "MainMenu":
					return;
				case "Loading":
					return;
				}
				PrintLogLine("Loading map \"" + sceneName + "\"");
				DamageDatabase.ResetRound();
				if (Pref_Log_PlayerConsole_Enable.Value)
				{
					HelperMethods.SendConsoleMessage("<b>Loading map \"" + sceneName + "\"</b>");
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception);
			}
		}

		public static void ProcessOnPlayerChangedTeam(Player player, Team oldTeam, Team newTeam)
		{
			string teamOverride = ((!(oldTeam == null)) ? oldTeam.TeamShortName : string.Empty);
			if (!(newTeam == null) && !(player == null))
			{
				PrintLogLine(AddPlayerLogEntry(player, teamOverride) + " joined team \"" + newTeam.TeamShortName + "\"");
				if (Pref_Log_PlayerConsole_Enable.Value)
				{
					string text = AddPlayerConsoleEntry(player);
					string text2 = ((oldTeam == null) ? "joined team" : "changed to team");
					HelperMethods.SendConsoleMessage(text + " " + text2 + " " + HelperMethods.GetTeamColor(newTeam) + newTeam.TeamShortName + "</color>");
				}
			}
		}

		public void OnRoleChanged(object? sender, OnRoleChangedArgs args)
		{
			try
			{
				if (args != null)
				{
					Player player = args.Player;
					if (!(player == null) && !(player.Team == null))
					{
						string roleName = GetRoleName(args.Role);
						PrintLogLine(AddPlayerLogEntry(player) + " changed role to \"" + roleName + "\"");
					}
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run OnRoleChanged");
			}
		}

		public static Player? GetPlayer(BaseGameObject? baseGameObject)
		{
			if (baseGameObject == null)
			{
				return null;
			}
			NetworkComponent networkComponent = baseGameObject.NetworkComponent;
			if (networkComponent == null)
			{
				return null;
			}
			Player ownerPlayer = networkComponent.OwnerPlayer;
			if (ownerPlayer == null)
			{
				return null;
			}
			return ownerPlayer;
		}

		public static Player? GetPlayer(DamageManager? damageManager)
		{
			if (damageManager == null)
			{
				return null;
			}
			BaseGameObject owner = damageManager.Owner;
			if (owner == null)
			{
				return null;
			}
			return GetPlayer(owner);
		}

		public static Player? GetPlayer(GameObject? gameObject)
		{
			if (gameObject == null)
			{
				return null;
			}
			BaseGameObject baseGameObject = gameObject.GetBaseGameObject();
			if (baseGameObject == null)
			{
				return null;
			}
			return GetPlayer(baseGameObject);
		}

		public static bool ShouldHandleDamage(float damage)
		{
			if ((int)Math.Ceiling(damage) <= Pref_Log_MinDamageCutoff.Value)
			{
				return false;
			}
			return true;
		}

		public void OnCommanderSoldStructure_Log(object? sender, OnCommanderSoldStructureArgs args)
		{
			try
			{
				if (args == null)
				{
					return;
				}
				Structure structure = args.Structure;
				Team team = args.Team;
				if (!(structure == null) && !(team == null))
				{
					string teamName = GetTeamName(team);
					string structureName = GetStructureName(structure);
					string logPosition = GetLogPosition(structure.transform.position);
					PrintLogLine("Team \"" + teamName + "\" triggered \"structure_sold\" (building_name \"" + structureName + "\") (building_position \"" + logPosition + "\")");
					if (Pref_Log_PlayerConsole_Enable.Value)
					{
						string teamName2 = GetTeamName(team);
						HelperMethods.SendConsoleMessageToTeam(team, teamName2 + " sold a structure (" + HelperMethods.GetTeamColor(team) + structureName + "</color>)");
					}
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run OnCommanderDestroyedStructure_Log");
			}
		}

		public override void OnUpdate()
		{
			try
			{
				TrackPerformanceMonitor();
				TrackPeriodicResourceEvent();
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed in OnUpdate");
			}
		}

		public static void TrackPeriodicResourceEvent()
		{
			Timer_ResourceLogEvent += Time.deltaTime;
			if (Timer_ResourceLogEvent >= 60f)
			{
				Timer_ResourceLogEvent = 0f;
				if (!(GameMode.CurrentGameMode == null) && GameMode.CurrentGameMode.GameOngoing && !GameMode.CurrentGameMode.GameOver)
				{
					LogPeriodicResources();
				}
			}
		}

		public static void LogPeriodicResources()
		{
			GameModeExt gameModeExt = GameMode.CurrentGameMode as GameModeExt;
			if (gameModeExt == null)
			{
				return;
			}
			foreach (BaseTeamSetup baseTeamSetup in gameModeExt.BaseTeamSetups)
			{
				if (gameModeExt.GetTeamSetupActive(baseTeamSetup))
				{
					string text = teamResourcesCollected[baseTeamSetup.Team.Index].ToString();
					string text2 = teamResourcesSpent[baseTeamSetup.Team.Index].ToString();
					string teamName = GetTeamName(baseTeamSetup.Team);
					PrintLogLine("Team \"" + teamName + "\" triggered \"resource_status\" (collected \"" + text + "\") (spent \"" + text2 + "\")");
				}
			}
		}

		public static int getHighestTechTier(Team team)
		{
			return team.TechnologyTier;
		}

		public static void initializeRound(ref int[] tiers)
		{
			for (int i = 0; i < 8; i++)
			{
				tiers[Team.Teams[i].Index] = 0;
				teamResourcesCollected[i] = 0;
				teamResourcesSpent[i] = 0;
			}
		}

		public static void LogTierChange(Team team, int tier)
		{
			string teamName = GetTeamName(team);
			PrintLogLine($"Team \"{teamName}\" triggered \"technology_change\" (tier \"{tier}\")");
		}

		public static void LogStartingResources()
		{
			foreach (ResourceArea allResourceArea in ResourceArea.AllResourceAreas)
			{
				if (!(allResourceArea == null) && allResourceArea.enabled)
				{
					string logPosition = GetLogPosition(allResourceArea.transform.position);
					string text = allResourceArea.ResourceAmountCurrent.ToString();
					string logResourceType = GetLogResourceType(allResourceArea.ResourceType);
					PrintLogLine("World triggered \"Resource_Spawned\" (type \"" + logResourceType + "\") (amount \"" + text + "\") (position \"" + logPosition + "\")");
				}
			}
		}

		public static void LogStartingStructures()
		{
			if (Structure.Structures == null || Structure.Structures.Count == 0)
			{
				MelonLogger.Warning("Could not find starting structures.");
				return;
			}
			foreach (Structure structure in Structure.Structures)
			{
				if (structure == null || structure.Team == null || structure.ObjectInfo == null)
				{
					MelonLogger.Warning("Skipping invalid structure.");
				}
				else if (structure.Team.BaseStructure == structure.ObjectInfo)
				{
					string teamName = GetTeamName(structure.Team);
					string structureName = GetStructureName(structure);
					string logPosition = GetLogPosition(structure.transform.position);
					PrintLogLine("Team \"" + teamName + "\" triggered \"construction_complete\" (building_name \"" + structureName + "\") (building_position \"" + logPosition + "\")");
				}
			}
		}

		public void OnRequestPlayerChat(object? sender, OnRequestPlayerChatArgs args)
		{
			if (!(args.Player == null))
			{
				string text = AddPlayerLogEntry(args.Player);
				string text2 = ((!args.TeamOnly) ? "say" : "say_team");
				PrintLogLine(text + " " + text2 + " \"" + args.Text + "\"");
			}
		}
	}
	public class ServerPerfLogger
	{
		public class PerfMonitorData
		{
			public int UnixTime { get; set; }

			public int ServerFPS { get; set; }

			public int Players { get; set; }

			public int Structures { get; set; }

			public int ConstructionSites { get; set; }

			public int NetworkComponents { get; set; }

			public int Units { get; set; }

			public int LightsOn { get; set; }

			public float UploadRate { get; set; }

			public float DownloadRate { get; set; }

			public PerfMonitorData()
			{
				UnixTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
				ServerFPS = Game.FPS;
				Players = ((Player.Players != null) ? Player.Players.Count : (-1));
				Structures = ((Structure.Structures != null) ? Structure.Structures.Count : (-1));
				ConstructionSites = ((ConstructionSite.ConstructionSites != null) ? ConstructionSite.ConstructionSites.Count : (-1));
				NetworkComponents = ((NetworkComponent.NetworkComponents != null) ? NetworkComponent.NetworkComponents.Count : (-1));
				Units = ((Unit.Units != null) ? Unit.Units.Count : (-1));
				LightsOn = GetNumberOfLightsOn();
				UploadRate = (float)Mathf.RoundToInt(NetworkLayer.NetBitsAvgUpload * 0.01f) * 0.1f;
				DownloadRate = (float)Mathf.RoundToInt(NetworkLayer.NetBitsAvgDownload * 0.01f) * 0.1f;
			}
		}

		private static readonly string perfMonitorLogFile = Path.Combine(MelonEnvironment.UserDataDirectory, "PerfMonitorData.csv");

		private static int GetNumberOfLightsOn()
		{
			if (Unit.Units == null)
			{
				return -1;
			}
			int num = 0;
			foreach (Unit unit in Unit.Units)
			{
				if (!(unit.UnitLights == null) && unit.UnitLights.LightsOn)
				{
					num++;
				}
			}
			return num;
		}

		public static void CapturePerformancePoint()
		{
			if (!File.Exists(perfMonitorLogFile))
			{
				AddColumnInfo();
			}
			AddPerformanceEntry();
		}

		public static void AddColumnInfo()
		{
			string text = "Unix Time,Server FPS,Players,Structures,Construction Sites,Network Components,Units,Lights On,Upload Rate,Download Rate";
			File.AppendAllText(perfMonitorLogFile, text + Environment.NewLine);
		}

		public static void AddPerformanceEntry()
		{
			PerfMonitorData perfMonitorData = new PerfMonitorData();
			string text = $"{perfMonitorData.UnixTime}," + $"{perfMonitorData.ServerFPS}," + $"{perfMonitorData.Players}," + $"{perfMonitorData.Structures}," + $"{perfMonitorData.ConstructionSites}," + $"{perfMonitorData.NetworkComponents}," + $"{perfMonitorData.Units}," + $"{perfMonitorData.LightsOn}," + $"{perfMonitorData.UploadRate}," + $"{perfMonitorData.DownloadRate}";
			File.AppendAllText(perfMonitorLogFile, text + Environment.NewLine);
		}
	}
}
