using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using Microsoft.CodeAnalysis;
using Si_FriendlyFireLimits;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(FriendlyFireLimits), "Friendly Fire Limits", "1.3.6", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_FriendlyFireLimits")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+17f1aa45fa1403d6b033f5c4b3839f68b78e8f34")]
[assembly: AssemblyProduct("Si_FriendlyFireLimits")]
[assembly: AssemblyTitle("Si_FriendlyFireLimits")]
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
namespace Si_FriendlyFireLimits
{
	public class FriendlyFireLimits : MelonMod
	{
		[HarmonyPatch(typeof(DamageManager), "ApplyDamage")]
		private static class ApplyPatchApplyDamage
		{
			public static bool Prefix(DamageManager __instance, float __result, Collider __0, ref float __1, EDamageType __2, GameObject __3, Vector3 __4)
			{
				try
				{
					if (__3 == null || __instance == null)
					{
						return true;
					}
					BaseGameObject baseGameObject = __3.GetBaseGameObject();
					if (baseGameObject == null || baseGameObject.NetworkComponent == null || baseGameObject.NetworkComponent.OwnerPlayer != null)
					{
						return true;
					}
					if (!ShouldProcessDamageModifier(__instance, __3, __1))
					{
						return true;
					}
					__1 = FindTeamDamage(__instance, __3, __1, __2);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run DamageManager::ApplyDamage");
				}
				return true;
			}
		}

		private static MelonPreferences_Category _modCategory;

		private static MelonPreferences_Entry<float> _FriendlyFireDamageCutoff;

		private static MelonPreferences_Entry<float> _UnitOnUnitNonExplosionDamageMultipler;

		private static MelonPreferences_Entry<float> _UnitOnUnitExplosionDamageMultiplier;

		private static MelonPreferences_Entry<float> _UnitOnStructureExplosionDamageMultiplier;

		private static MelonPreferences_Entry<float> _UnitOnStructureNonExplosionDamageMultiplier;

		private static MelonPreferences_Entry<float> _StructuresAttackingUnitsDamageRatio;

		private static MelonPreferences_Entry<float> _StructuresAttackingStructuresDamageRatio;

		private static MelonPreferences_Entry<bool> _HarvesterPassthrough;

		private const string ModCategory = "Silica";

		public override void OnInitializeMelon()
		{
			if (_modCategory == null)
			{
				_modCategory = MelonPreferences.CreateCategory("Silica");
			}
			if (_FriendlyFireDamageCutoff == null)
			{
				_FriendlyFireDamageCutoff = _modCategory.CreateEntry<float>("FriendlyFire_Allow_Damage_Below_Cutoff", 5f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_UnitOnUnitNonExplosionDamageMultipler == null)
			{
				_UnitOnUnitNonExplosionDamageMultipler = _modCategory.CreateEntry<float>("FriendlyFire_Unit_ATKs_Unit_DamageRatio", 0.5f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_UnitOnUnitExplosionDamageMultiplier == null)
			{
				_UnitOnUnitExplosionDamageMultiplier = _modCategory.CreateEntry<float>("FriendlyFire_Unit_ATKs_Unit_DamageRatio_Explosion", 0.875f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_UnitOnStructureExplosionDamageMultiplier == null)
			{
				_UnitOnStructureExplosionDamageMultiplier = _modCategory.CreateEntry<float>("FriendlyFire_Unit_ATKs_Structure_DamageRatio_Explosion", 0.625f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_UnitOnStructureNonExplosionDamageMultiplier == null)
			{
				_UnitOnStructureNonExplosionDamageMultiplier = _modCategory.CreateEntry<float>("FriendlyFire_Unit_ATKs_Structure_DamageRatio", 0f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_StructuresAttackingUnitsDamageRatio == null)
			{
				_StructuresAttackingUnitsDamageRatio = _modCategory.CreateEntry<float>("FriendlyFire_Structue_ATKs_Unit_DamageRatio", 0.25f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_StructuresAttackingStructuresDamageRatio == null)
			{
				_StructuresAttackingStructuresDamageRatio = _modCategory.CreateEntry<float>("FriendlyFire_Structue_ATKs_Structure_DamageRatio", 0.25f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_HarvesterPassthrough == null)
			{
				_HarvesterPassthrough = _modCategory.CreateEntry<bool>("FriendlyFire_Allow_Harvester_Damage", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
		}

		[MethodImpl(MethodImplOptions.NoOptimization)]
		public override void OnLateInitializeMelon()
		{
			Event_Damage.OnPrePlayerDamageReceived += OnPreDamageReceived_FriendlyFireCheck;
		}

		public void OnPreDamageReceived_FriendlyFireCheck(object? sender, OnPreDamageReceivedArgs args)
		{
			try
			{
				if (args != null && ShouldProcessDamageModifier(args.DamageManager, args.Instigator, args.Damage))
				{
					args.Damage = FindTeamDamage(args.DamageManager, args.Instigator, args.Damage, EDamageType.None, playerControlled: true);
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run OnPreDamageReceived_FriendlyFireCheck");
			}
		}

		public static bool IsDamageSelfInflicted(DamageManager victimDamageManager, GameObject instigatorObject)
		{
			return instigatorObject.GetBaseGameObject() == victimDamageManager.Owner;
		}

		public static bool IsDamageFriendlyFire(DamageManager victimDamageManager, GameObject instigatorObject)
		{
			BaseGameObject baseGameObject = instigatorObject.GetBaseGameObject();
			if (baseGameObject == null || baseGameObject.Team == null || victimDamageManager.Owner == null || victimDamageManager.Owner.Team == null)
			{
				return false;
			}
			return baseGameObject.Team.Index == victimDamageManager.Owner.Team.Index;
		}

		public static float FindTeamDamage(DamageManager victimDamageManager, GameObject instigatorObject, float originalDamage, EDamageType damageType = EDamageType.None, bool playerControlled = false)
		{
			ObjectInfo objectInfo = victimDamageManager.Owner.ObjectInfo;
			ObjectInfo objectInfo2 = instigatorObject.GetBaseGameObject().ObjectInfo;
			ObjectInfoType objectType = objectInfo.ObjectType;
			ObjectInfoType objectType2 = objectInfo2.ObjectType;
			if (objectType == ObjectInfoType.Unit && objectType2 == ObjectInfoType.Unit)
			{
				if (_HarvesterPassthrough.Value && objectInfo.UnitType == UnitType.Harvester)
				{
					return originalDamage;
				}
				if (damageType == EDamageType.Explosion)
				{
					return originalDamage * _UnitOnUnitExplosionDamageMultiplier.Value;
				}
				return originalDamage * _UnitOnUnitNonExplosionDamageMultipler.Value;
			}
			if (objectType == ObjectInfoType.Structure && objectType2 == ObjectInfoType.Unit)
			{
				if (damageType == EDamageType.Explosion)
				{
					return originalDamage * _UnitOnStructureExplosionDamageMultiplier.Value;
				}
				return originalDamage * _UnitOnStructureNonExplosionDamageMultiplier.Value;
			}
			if (objectType == ObjectInfoType.Unit && objectType2 == ObjectInfoType.Structure)
			{
				if (_HarvesterPassthrough.Value && objectInfo.UnitType == UnitType.Harvester)
				{
					return originalDamage;
				}
				return originalDamage * _StructuresAttackingUnitsDamageRatio.Value;
			}
			if (objectType == ObjectInfoType.Structure && objectType2 == ObjectInfoType.Structure)
			{
				return originalDamage * _StructuresAttackingStructuresDamageRatio.Value;
			}
			MelonLogger.Warning("Hit unexpected statement in FindTeamDamage");
			return originalDamage;
		}

		public static bool ShouldProcessDamageModifier(DamageManager damageManager, GameObject gameObject, float damageAmount)
		{
			if (!IsDamageFriendlyFire(damageManager, gameObject))
			{
				return false;
			}
			if (IsDamageSelfInflicted(damageManager, gameObject))
			{
				return false;
			}
			if (damageAmount < _FriendlyFireDamageCutoff.Value)
			{
				return false;
			}
			return true;
		}
	}
}
