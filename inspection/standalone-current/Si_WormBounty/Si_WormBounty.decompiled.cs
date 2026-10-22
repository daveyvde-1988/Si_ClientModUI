using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using Microsoft.CodeAnalysis;
using Si_WormBounty;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(WormBounty), "Worm Bounty", "1.1.4", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_WormBounty")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_WormBounty")]
[assembly: AssemblyTitle("Si_WormBounty")]
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
namespace Si_WormBounty
{
	public class WormBounty : MelonMod
	{
		[HarmonyPatch(typeof(AmbientLife), "OnEnable")]
		private static class WormBounty_Patch_AmbientLife_OnEnable
		{
			public static void Postfix(AmbientLife __instance)
			{
				try
				{
					__instance.NumBossMax = _Pref_GreatWorms_MaxNumber.Value;
					__instance.SpawnBossChance = _Pref_GreatWorms_SpawnChance.Value;
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run AmbientLife::OnEnable");
				}
			}
		}

		[HarmonyPatch(typeof(AmbientLife), "OnUnitDestroyed")]
		private static class WormBounty_Patch_AmbientLife_OnUnitDestroyed
		{
			public static void Prefix(AmbientLife __instance, Unit __0, GameObject __1)
			{
				try
				{
					if (!GameMode.CurrentGameMode || !GameMode.CurrentGameMode.GetAmbientLifeEnabled() || __1 == null || __instance.Boss != __0.ObjectInfo)
					{
						return;
					}
					if (__instance.Team.Index != 1)
					{
						MelonLogger.Warning("Ignoring bounty for GreatWorm due to worm being on team " + __instance.Team.TeamShortName);
						return;
					}
					BaseGameObject baseGameObject = __1.GetBaseGameObject();
					if (baseGameObject == null)
					{
						return;
					}
					NetworkComponent networkComponent = baseGameObject.NetworkComponent;
					if (!(networkComponent == null))
					{
						Player ownerPlayer = networkComponent.OwnerPlayer;
						if (!(ownerPlayer == null) && !(ownerPlayer.Team == null))
						{
							int bounty = FindBounty(ownerPlayer.Team);
							AwardBounty(ownerPlayer, bounty);
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run AmbientLife::OnUnitDestroyed");
				}
			}
		}

		private static MelonPreferences_Category _modCategory;

		private static MelonPreferences_Entry<int> _Pref_GreatWorms_MaxNumber;

		private static MelonPreferences_Entry<float> _Pref_GreatWorms_SpawnChance;

		private static MelonPreferences_Entry<int> _Pref_GreatWorms_Bounty_BaseAmount;

		private static MelonPreferences_Entry<int> _Pref_GreatWorms_Bounty_RandomBonusMax;

		private static MelonPreferences_Entry<bool> _Pref_GreatWorms_Bounty_TechScaling;

		public override void OnInitializeMelon()
		{
			if (_modCategory == null)
			{
				_modCategory = MelonPreferences.CreateCategory("Silica");
			}
			if (_Pref_GreatWorms_MaxNumber == null)
			{
				_Pref_GreatWorms_MaxNumber = _modCategory.CreateEntry<int>("GreatWorms_MaxNumber", 2, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_GreatWorms_Bounty_BaseAmount == null)
			{
				_Pref_GreatWorms_Bounty_BaseAmount = _modCategory.CreateEntry<int>("GreatWorms_Bounty_BaseAmount", 1250, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_GreatWorms_Bounty_RandomBonusMax == null)
			{
				_Pref_GreatWorms_Bounty_RandomBonusMax = _modCategory.CreateEntry<int>("GreatWorms_Bounty_Bonus_RandomMax", 750, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_GreatWorms_Bounty_TechScaling == null)
			{
				_Pref_GreatWorms_Bounty_TechScaling = _modCategory.CreateEntry<bool>("GreatWorms_Bounty_TechScaling", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_GreatWorms_SpawnChance == null)
			{
				_Pref_GreatWorms_SpawnChance = _modCategory.CreateEntry<float>("GreatWorms_SpawnChance", 0.25f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
		}

		public override void OnLateInitializeMelon()
		{
			HelperMethods.CommandCallback adminCallback = Command_Devour;
			HelperMethods.RegisterAdminCommand("devour", adminCallback, Power.Slay, "Spawns a Great Worm at the location of the player. Usage: !devour <player>");
		}

		public void Command_Devour(Player? callerPlayer, string args)
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
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Ambiguous or invalid target");
				return;
			}
			if (callerPlayer != null && !callerPlayer.CanAdminTarget(player))
			{
				HelperMethods.ReplyToCommand_Player(player, "is immune due to level");
				return;
			}
			AmbientLife ambientLife = UnityEngine.Object.FindFirstObjectByType<AmbientLife>();
			Vector3 worldPhysicalCenter = player.ControlledUnit.WorldPhysicalCenter;
			Quaternion rotatedQuaternion = GameMath.GetRotatedQuaternion(Quaternion.identity, Vector3.up * UnityEngine.Random.Range(-180f, 180f));
			Target.GetTargetByNetID(player.ControlledUnit.NetworkComponent.NetID);
			_ = worldPhysicalCenter + rotatedQuaternion * Vector3.forward * UnityEngine.Random.Range(10f, 25f);
			if (!(Game.SpawnPrefab(ambientLife.Boss.Prefab, null, ambientLife.Team, worldPhysicalCenter, rotatedQuaternion).GetBaseGameObject() as Unit == null))
			{
				MelonLogger.Msg("Spawning Great Worm to devour player (" + player.PlayerName + ")");
				HelperMethods.AlertAdminActivity(callerPlayer, player, "devoured");
			}
		}

		private static int FindBounty(Team team)
		{
			int value = _Pref_GreatWorms_Bounty_BaseAmount.Value;
			int num = 0;
			if (_Pref_GreatWorms_Bounty_RandomBonusMax.Value > 0)
			{
				num = new System.Random().Next(0, _Pref_GreatWorms_Bounty_RandomBonusMax.Value);
			}
			int num2 = value + num;
			if (_Pref_GreatWorms_Bounty_TechScaling.Value)
			{
				num2 = (int)Math.Round((float)num2 * FindTechScalingFactor(team));
			}
			return (int)Math.Round((double)num2 / 10.0) * 10;
		}

		private static float FindTechScalingFactor(Team team)
		{
			float num = (float)team.TechnologyTier / (float)team.MaximumTechnologyTier;
			return 1f + num * 2f;
		}

		private static void AwardBounty(Player player, int bounty)
		{
			Team team = player.Team;
			team.StoreResource(bounty);
			HelperMethods.ReplyToCommand_Player(player, "defeated a Great Worm. ", bounty.ToString(), " awarded to ", team.TeamShortName);
		}
	}
}
