using System;
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
using Si_AntiGrief;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(AntiGrief), "Anti-Grief", "1.6.6", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_AntiGrief")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_AntiGrief")]
[assembly: AssemblyTitle("Si_AntiGrief")]
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
namespace Si_AntiGrief
{
	public class AntiGrief : MelonMod
	{
		[HarmonyPatch(typeof(Player), "OnTargetDestroyed")]
		private static class ApplyPatch_OnTargetDestroyed
		{
			public static void Postfix(Target __0, GameObject __1)
			{
				try
				{
					if (__0 == null || __1 == null)
					{
						return;
					}
					Team team = __0.Team;
					BaseGameObject baseGameObject = __1.GetBaseGameObject();
					if (baseGameObject == null || team == null)
					{
						return;
					}
					Team team2 = baseGameObject.Team;
					if (team2 == null || team2.Index != team.Index)
					{
						return;
					}
					NetworkComponent networkComponent = baseGameObject.NetworkComponent;
					if (networkComponent == null)
					{
						return;
					}
					Player ownerPlayer = networkComponent.OwnerPlayer;
					if (ownerPlayer == null)
					{
						return;
					}
					if (__0.Owner is Structure || __0.Owner is ConstructionSite)
					{
						string displayName = GetDisplayName(__0);
						if (!DisplayTeamKillForStructure(displayName))
						{
							return;
						}
						MelonLogger.Msg(ownerPlayer.PlayerName + " team killed a structure " + displayName);
						HelperMethods.ReplyToCommand_Player(ownerPlayer, "killed a friendly " + ((__0.OwnerConstructionSite == null) ? "structure" : "construction site") + " (" + HelperMethods.GetTeamColor(ownerPlayer) + displayName + "</color>)");
						HandleTeamKillerSoundEffect(ownerPlayer);
					}
					else if (__0.Owner is Unit)
					{
						Player controlledBy = __0.OwnerUnit.ControlledBy;
						if (controlledBy == ownerPlayer)
						{
							return;
						}
						MelonLogger.Msg(ownerPlayer.PlayerName + " destroyed a friendly unit with kill score of " + ownerPlayer.Kills);
						if (controlledBy != null)
						{
							MelonLogger.Msg(ownerPlayer.PlayerName + " team killed " + controlledBy.PlayerName);
							HelperMethods.ReplyToCommand_Player(ownerPlayer, "team killed " + HelperMethods.GetTeamColor(controlledBy) + controlledBy.PlayerName + "</color>");
							HandleTeamKillerSoundEffect(ownerPlayer);
						}
					}
					EvaluatePunishment(ownerPlayer);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run Player::OnTargetDestroyed");
				}
			}
		}

		[HarmonyPatch(typeof(GameMode), "SpawnUnitForPlayer", new Type[]
		{
			typeof(Player),
			typeof(GameObject),
			typeof(Vector3),
			typeof(Quaternion)
		})]
		private static class AntiGrief_Patch_GameMode_SpawnUnitForPlayer
		{
			public static void Postfix(GameMode __instance, Unit __result, Player __0, GameObject __1, Vector3 __2, Quaternion __3)
			{
				try
				{
					if (!(__0 == null))
					{
						int index = __0.GetIndex();
						if (!IsPlayerIndexValid(index))
						{
							MelonLogger.Error("Player Index found outside of bounds (SpawnUnitForPlayer): " + index);
						}
						else
						{
							playerTransferCount[index] = 0u;
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run GameMode::SpawnUnitForPlayer");
				}
			}
		}

		private static MelonPreferences_Category _modCategory = null;

		private static MelonPreferences_Entry<int> _NegativeKillsThreshold = null;

		private static MelonPreferences_Entry<bool> _PlayTeamKillerSound = null;

		private static MelonPreferences_Entry<bool> _NegativeKills_Penalty_Ban = null;

		private static MelonPreferences_Entry<bool> _StructureAntiGrief_IgnoreNodes = null;

		private static MelonPreferences_Entry<bool> _BlockShrimpControllers = null;

		private static MelonPreferences_Entry<bool> _BlockCommanderRemovingLastSpawn = null;

		private static MelonPreferences_Entry<bool> _BlockCommanderRemovingLastResearch = null;

		private static MelonPreferences_Entry<bool> _BlockShrimpFPSCommanding = null;

		private static MelonPreferences_Entry<bool> _BlockHarvesterFPSCommanding = null;

		private static MelonPreferences_Entry<bool> _BlockQueenFPSCommanding = null;

		private static uint[] playerTransferCount = new uint[0];

		private const string ModCategory = "Silica";

		public override void OnInitializeMelon()
		{
			if (_modCategory == null)
			{
				_modCategory = MelonPreferences.CreateCategory("Silica");
			}
			if (_NegativeKillsThreshold == null)
			{
				_NegativeKillsThreshold = _modCategory.CreateEntry<int>("Grief_NegativeKills_Threshold", -9, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_PlayTeamKillerSound == null)
			{
				_PlayTeamKillerSound = _modCategory.CreateEntry<bool>("Grief_PlayTeamkillerSound", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_NegativeKills_Penalty_Ban == null)
			{
				_NegativeKills_Penalty_Ban = _modCategory.CreateEntry<bool>("Grief_NegativeKills_Penalty_Ban", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_StructureAntiGrief_IgnoreNodes == null)
			{
				_StructureAntiGrief_IgnoreNodes = _modCategory.CreateEntry<bool>("Grief_IgnoreFriendlyNodesDestroyed", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_BlockShrimpControllers == null)
			{
				_BlockShrimpControllers = _modCategory.CreateEntry<bool>("Grief_BlockShrimpTakeOver", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_BlockCommanderRemovingLastSpawn == null)
			{
				_BlockCommanderRemovingLastSpawn = _modCategory.CreateEntry<bool>("Grief_BlockRemoveLastSpawn", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_BlockCommanderRemovingLastResearch == null)
			{
				_BlockCommanderRemovingLastResearch = _modCategory.CreateEntry<bool>("Grief_BlockRemoveLastResearch", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_BlockShrimpFPSCommanding == null)
			{
				_BlockShrimpFPSCommanding = _modCategory.CreateEntry<bool>("Grief_BlockShrimpFPSCommanding", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_BlockHarvesterFPSCommanding == null)
			{
				_BlockHarvesterFPSCommanding = _modCategory.CreateEntry<bool>("Grief_BlockHarvesterFPSCommanding", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_BlockQueenFPSCommanding == null)
			{
				_BlockQueenFPSCommanding = _modCategory.CreateEntry<bool>("Grief_BlockQueenFPSCommanding", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			InitializeTransferCountArray();
		}

		public void InitializeTransferCountArray()
		{
			int num = NetworkGameServer.GetPlayersMax() + 1;
			playerTransferCount = new uint[num];
			MelonLogger.Msg("Setting Anti-Grief transfer count array using max array index of: " + num);
			Array.Fill(playerTransferCount, 0u);
		}

		[MethodImpl(MethodImplOptions.NoOptimization)]
		public override void OnLateInitializeMelon()
		{
			Event_Roles.OnRoleChanged += OnRoleChanged;
			Event_Units.OnRequestEnterUnit += OnRequestEnterUnit;
			Event_Units.OnRequestInviteToGroup += OnRequestInviteToGroup_GriefCheck;
			Event_Structures.OnRequestSellStructure += OnRequestSellStructure_GriefCheck;
		}

		public void OnRequestSellStructure_GriefCheck(object? sender, OnRequestSellStructureArgs args)
		{
			try
			{
				if (args == null)
				{
					return;
				}
				Structure structure = args.Structure;
				Team team = args.Team;
				if (structure == null || team == null)
				{
					return;
				}
				MelonLogger.Msg("OnRequestSellStructure StructureType: " + structure.ObjectInfo.StructureType.ToString() + " SelectionType: " + structure.ObjectInfo.StructureSelectionType);
				if ((!_BlockCommanderRemovingLastSpawn.Value || structure.ObjectInfo.StructureSelectionType != StructureSelectionType.Units1) && (!_BlockCommanderRemovingLastResearch.Value || structure.ObjectInfo.StructureType != StructureType.Research))
				{
					return;
				}
				int structureCount = team.GetStructureCount(structure.ObjectInfo);
				MelonLogger.Msg("OnRequestSellStructure Remaining Structures: " + structureCount);
				if (structureCount <= 1)
				{
					MelonLogger.Msg(team.TeamShortName + "'s commander tried to sell the last structure: ", new object[1] { structure.ObjectInfo.DisplayName });
					Player player = null;
					if (GameMode.CurrentGameMode is GameModeExt gameModeExt)
					{
						player = gameModeExt.GetCommanderForTeam(team);
					}
					if (player != null)
					{
						HelperMethods.ReplyToCommand_Player(player, "tried to destroy the last ", structure.ObjectInfo.DisplayName);
					}
					if (!args.GameDecision)
					{
						args.Override = true;
						MelonLogger.Msg("Anti-Grief is disallowing structure selling command.");
					}
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run OnRequestSellStructure_GriefCheck");
			}
		}

		public void OnRequestInviteToGroup_GriefCheck(object? sender, OnRequestInviteToGroupArgs args)
		{
			try
			{
				if (args == null)
				{
					return;
				}
				Player player = args.Player;
				Target target = args.Target;
				if (player == null || target == null || target.OwnerUnit == null)
				{
					return;
				}
				Unit ownerUnit = target.OwnerUnit;
				if (player.Team.Index == 0)
				{
					if (_BlockShrimpFPSCommanding.Value && ownerUnit.IsResourceHolder)
					{
						args.Block = true;
						MelonLogger.Msg("Found " + player.PlayerName + " trying to invite an Alien shrimp to their FPS Command group.");
						HelperMethods.SendChatMessageToPlayer(player, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " inviting shrimp is not permitted on this server.");
					}
					else if (_BlockQueenFPSCommanding.Value && ownerUnit.ObjectInfo.Critical)
					{
						MelonLogger.Msg("Found " + player.PlayerName + " trying to invite the Alien Queen to their FPS Command group.");
						HelperMethods.SendChatMessageToPlayer(player, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " inviting the Queen is not permitted on this server.");
						args.Block = true;
					}
				}
				else if (_BlockHarvesterFPSCommanding.Value && ownerUnit.IsResourceHolder)
				{
					MelonLogger.Msg("Found " + player.PlayerName + " trying to invite a Harvester to their FPS Command group.");
					HelperMethods.SendChatMessageToPlayer(player, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " inviting a Harvester is not permitted on this server.");
					args.Block = true;
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run OnRequestInviteToGroup_GriefCheck");
			}
		}

		public void OnRequestEnterUnit(object? sender, OnRequestEnterUnitArgs args)
		{
			try
			{
				if (args == null)
				{
					return;
				}
				Player player = args.Player;
				Unit unit = args.Unit;
				if (player == null || player.Team == null || unit == null || player.Team.Index != 0)
				{
					return;
				}
				if (_BlockShrimpControllers.Value && unit.IsResourceHolder)
				{
					MelonLogger.Msg("Found " + player.PlayerName + " trying to use an Alien shrimp.");
					HelperMethods.SendChatMessageToPlayer(player, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " use of shrimp is not permitted on this server.");
					args.Block = true;
					return;
				}
				if (ShouldDeletePriorUnit(player))
				{
					MelonLogger.Msg("Player " + player.PlayerName + " found switching from spawn unit. Taking action to prevent inf creatures.");
					DeletePriorUnit(player, player.ControlledUnit);
				}
				int index = player.GetIndex();
				if (!IsPlayerIndexValid(index))
				{
					MelonLogger.Error("Player Index found outside of bounds (OnRequestEnterUnit): " + index);
				}
				else
				{
					playerTransferCount[index]++;
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run OnRequestEnterUnit");
			}
		}

		public static void DeletePriorUnit(Player player, Unit unit)
		{
			for (int num = NetworkComponent.NetworkComponents.Count - 1; num >= 0; num--)
			{
				NetworkComponent networkComponent = NetworkComponent.NetworkComponents[num];
				if (networkComponent != null && networkComponent.OwnerPlayer == player && networkComponent.IsValid && networkComponent == unit.NetworkComponent)
				{
					MelonLogger.Msg("Removing old unit (" + unit.name + ") from " + player.PlayerName + " before allowing transfer.");
					UnityEngine.Object.Destroy(networkComponent.gameObject);
				}
			}
		}

		public static bool ShouldDeletePriorUnit(Player player)
		{
			Unit controlledUnit = player.ControlledUnit;
			Team team = player.Team;
			if (controlledUnit == null || team == null)
			{
				return false;
			}
			if (GameMode.CurrentGameMode.GetCanDeletePlayerControlledUnit(player, controlledUnit))
			{
				int index = player.GetIndex();
				if (!IsPlayerIndexValid(index))
				{
					MelonLogger.Error("Player Index found outside of bounds (ShouldDeletePriorUnit): " + index);
					return false;
				}
				if (playerTransferCount[index] >= 1)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public void OnRoleChanged(object? sender, OnRoleChangedArgs args)
		{
			try
			{
				if (args == null)
				{
					return;
				}
				Player player = args.Player;
				if (!(player == null) && !(player.Team == null) && player.Team.Index == 0 && !(GameMode.CurrentGameMode == null) && !GameMode.CurrentGameMode.GameOver && GameMode.CurrentGameMode.GameOngoing && args.Role != GameModeExt.ETeamRole.UNIT)
				{
					Unit controlledUnit = player.ControlledUnit;
					if (!(controlledUnit == null) && !controlledUnit.DamageManager.IsDestroyed && IsHighValueAlienUnit(controlledUnit, player))
					{
						string displayName = GetDisplayName(controlledUnit.name);
						MelonLogger.Msg(player.PlayerName + " tried to despawn a non-trivial Alien unit (" + displayName + ")");
						HelperMethods.ReplyToCommand_Player(player, "tried to despawn a unit (" + HelperMethods.GetTeamColor(player) + displayName + "</color>)");
						GameMode.CurrentGameMode.TrySpawnPlayerAtSpawnPoint(player, player.Team);
					}
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run OnRoleChanged");
			}
		}

		public static void EvaluatePunishment(Player player)
		{
			if (player.Kills < _NegativeKillsThreshold.Value)
			{
				string playerName = player.PlayerName;
				if (_NegativeKills_Penalty_Ban.Value)
				{
					MelonLogger.Msg("Banned " + playerName + " (" + player.ToString() + ") for griefing (negative kills)");
					HelperMethods.ReplyToCommand_Player(player, "was banned for griefing (negative kills)");
					NetworkGameServer.KickPlayer(player);
				}
				else
				{
					MelonLogger.Msg("Kicked " + playerName + " (" + player.ToString() + ") for griefing (negative kills)");
					HelperMethods.ReplyToCommand_Player(player, "was kicked for griefing (negative kills)");
					HelperMethods.KickPlayer(player);
				}
			}
		}

		private static void HandleTeamKillerSoundEffect(Player player)
		{
			if (_PlayTeamKillerSound.Value)
			{
				AudioHelper.PlaySoundFile(Path.Combine(MelonEnvironment.UserDataDirectory, "sounds\\teamkiller.wav"), player);
			}
		}

		public static bool IsHighValueAlienUnit(Unit unit, Player player)
		{
			BaseGameObject baseGameObject = unit.gameObject.GetBaseGameObject();
			if (baseGameObject == null)
			{
				return false;
			}
			ConstructionData constructionData = baseGameObject.ConstructionData;
			if (constructionData == null)
			{
				return false;
			}
			if (GameMode.CurrentGameMode.GetCanDeletePlayerControlledUnit(player, unit))
			{
				return false;
			}
			if (constructionData.ResourceCost >= 500)
			{
				return true;
			}
			return false;
		}

		private static bool IsPlayerIndexValid(int playerIndex)
		{
			if (playerIndex == -1)
			{
				return false;
			}
			return true;
		}

		private static bool DisplayTeamKillForStructure(string structureName)
		{
			if (_StructureAntiGrief_IgnoreNodes.Value && structureName == "Node")
			{
				return false;
			}
			return true;
		}

		private static string GetDisplayName(Target target)
		{
			return target.ObjectInfo.DisplayName.Replace(" ", "").Replace("-", "");
		}

		private static string GetDisplayName(string fullName)
		{
			if (fullName.Contains('_'))
			{
				return fullName.Split('_')[0];
			}
			if (fullName.Contains('('))
			{
				return fullName.Split('(')[0];
			}
			return fullName;
		}
	}
}
