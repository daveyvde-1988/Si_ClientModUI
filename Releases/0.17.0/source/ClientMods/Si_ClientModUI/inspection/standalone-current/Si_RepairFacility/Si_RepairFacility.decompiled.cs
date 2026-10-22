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
using Si_RepairFacility;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(RepairFacility), "Repair Facility", "1.4.0", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_RepairFacility")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_RepairFacility")]
[assembly: AssemblyTitle("Si_RepairFacility")]
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
namespace Si_RepairFacility
{
	public class RepairFacility : MelonMod
	{
		[HarmonyPatch(typeof(OpenableBase), "OnUnitEnterZone")]
		private static class RepairFacility_Patch_OpenableBase_OnUnitEnterZone
		{
			public static void Postfix(OpenableBase __instance, Zone __0, Unit __1)
			{
				try
				{
					if (!(__instance == null) && !(__instance.NetworkComponent == null) && !(__1 == null) && !(__instance.NetworkComponent.Owner == null) && !(__instance.NetworkComponent.Owner.Team == null) && __1.Team.Index == 4 && !(__instance.NetworkComponent.Owner.Team != __1.Team) && !(__1.DriverCompartment == null) && !(__1.NetworkComponent == null) && !(__1.ControlledBy == null) && __instance.NetworkComponent.ToString().Contains("LightFactory"))
					{
						MelonLogger.Msg("Found player's " + (__1.IsFlyingType ? "aircraft" : "vehicle") + " entering LVF repair zone: " + __1.ControlledBy.PlayerName + " with vehicle " + __1.ObjectInfo.DisplayName);
						vehiclesAtRepairShop.Add(__1);
						if (_Pref_Repair_Notification.Value)
						{
							HelperMethods.SendChatMessageToPlayer(__1.ControlledBy, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Entered vehicle repair zone.");
						}
						HandleRepairSoundEffects(__1.ControlledBy, startingRepair: true);
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run OpenableBase::OnUnitEnterZone");
				}
			}
		}

		[HarmonyPatch(typeof(OpenableBase), "OnUnitExitZone")]
		private static class RepairFacility_Patch_OpenableBase_OnUnitExitZone
		{
			public static void Postfix(OpenableBase __instance, Zone __0, Unit __1)
			{
				try
				{
					if (__instance == null || __instance.NetworkComponent == null || __1 == null || __instance.NetworkComponent.Owner == null || __instance.NetworkComponent.Owner.Team == null || __1.Team.Index != 4 || __instance.NetworkComponent.Owner.Team != __1.Team || !__instance.NetworkComponent.ToString().Contains("LightFactory"))
					{
						return;
					}
					if (__1.DriverCompartment == null || __1.NetworkComponent == null || __1.ControlledBy == null)
					{
						if (vehiclesAtRepairShop.Contains(__1))
						{
							MelonLogger.Msg("Found " + (__1.IsFlyingType ? "aircraft" : "vehicle") + " exiting LVF repair zone: " + __1.ObjectInfo.DisplayName);
						}
					}
					else
					{
						MelonLogger.Msg("Found player's " + (__1.IsFlyingType ? "aircraft" : "vehicle") + " exiting LVF repair zone: " + __1.ControlledBy.PlayerName + " with vehicle " + __1.ObjectInfo.DisplayName);
					}
					vehiclesAtRepairShop.RemoveAll((Unit vehicle) => vehicle == __1);
					if (__1.ControlledBy != null)
					{
						if (_Pref_Repair_Notification.Value)
						{
							HelperMethods.SendChatMessageToPlayer(__1.ControlledBy, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Left vehicle repair zone.");
						}
						HandleRepairSoundEffects(__1.ControlledBy, startingRepair: false);
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run OpenableBase::OnUnitExitZone");
				}
			}
		}

		[HarmonyPatch(typeof(AutoHeal), "OnEnable")]
		private static class RepairFacility_Patch_AutoHeal_OnEnable
		{
			public static void Postfix(AutoHeal __instance)
			{
				if (!(__instance == null) && !(__instance.Data == null))
				{
					string name = __instance.Data.name;
					switch (name.Substring(name.LastIndexOf("_") + 1))
					{
					case "Infantry":
						__instance.Data.HealAmountPct = _Pref_Humans_Infantry_HealRate.Value;
						break;
					case "AlienSmall":
						__instance.Data.HealAmountPct = _Pref_Aliens_SmallUnit_HealRate.Value;
						break;
					case "AlienStructure":
						__instance.Data.HealAmountPct = _Pref_Aliens_Structure_HealRate.Value;
						break;
					case "AlienMedium":
						__instance.Data.HealAmountPct = _Pref_Aliens_MediumUnit_HealRate.Value;
						break;
					case "AlienLarge":
						__instance.Data.HealAmountPct = _Pref_Aliens_LargeUnit_HealRate.Value;
						break;
					case "Queen":
						__instance.Data.HealAmountPct = _Pref_Aliens_Queen_HealRate.Value;
						break;
					default:
						MelonLogger.Warning("Received unknown AutoHealData type: " + __instance.name);
						__instance.Data.HealAmountPct = _Pref_Aliens_LargeUnit_HealRate.Value;
						break;
					}
				}
			}
		}

		private static List<Unit> vehiclesAtRepairShop = null;

		private static float Timer_HealVehicles = 0f;

		private static MelonPreferences_Category _modCategory = null;

		private static MelonPreferences_Entry<float> _Pref_Humans_Vehicle_HealRate = null;

		private static MelonPreferences_Entry<float> _Pref_Humans_Aircraft_HealRate = null;

		private static MelonPreferences_Entry<float> _Pref_Aliens_SmallUnit_HealRate = null;

		private static MelonPreferences_Entry<float> _Pref_Aliens_MediumUnit_HealRate = null;

		private static MelonPreferences_Entry<float> _Pref_Aliens_LargeUnit_HealRate = null;

		private static MelonPreferences_Entry<float> _Pref_Aliens_Structure_HealRate = null;

		private static MelonPreferences_Entry<float> _Pref_Aliens_Queen_HealRate = null;

		private static MelonPreferences_Entry<float> _Pref_Humans_Infantry_HealRate = null;

		private static MelonPreferences_Entry<float> _Pref_SiegeDefenders_Structure_HealRate = null;

		private static MelonPreferences_Entry<bool> _Pref_Repair_Notification = null;

		private static MelonPreferences_Entry<bool> _Pref_Repair_SoundEffects = null;

		private static Vector3 halfExtentsRepairCheck = new Vector3
		{
			x = 12f,
			y = 12f,
			z = 12f
		};

		public override void OnInitializeMelon()
		{
			if (_modCategory == null)
			{
				_modCategory = MelonPreferences.CreateCategory("Silica");
			}
			if (_Pref_Humans_Vehicle_HealRate == null)
			{
				_Pref_Humans_Vehicle_HealRate = _modCategory.CreateEntry<float>("RepairFacility_HumanVehicle_HealRate", 0.035f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Humans_Aircraft_HealRate == null)
			{
				_Pref_Humans_Aircraft_HealRate = _modCategory.CreateEntry<float>("RepairFacility_HumanAircraft_HealRate", 0.015f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Humans_Infantry_HealRate == null)
			{
				_Pref_Humans_Infantry_HealRate = _modCategory.CreateEntry<float>("RepairFacility_HumanInfantry_HealRate", 0.015f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Aliens_SmallUnit_HealRate == null)
			{
				_Pref_Aliens_SmallUnit_HealRate = _modCategory.CreateEntry<float>("RepairFacility_Alien_SmallUnit_HealRate", 0.02f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Aliens_MediumUnit_HealRate == null)
			{
				_Pref_Aliens_MediumUnit_HealRate = _modCategory.CreateEntry<float>("RepairFacility_Alien_MediumUnit_HealRate", 0.012f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Aliens_LargeUnit_HealRate == null)
			{
				_Pref_Aliens_LargeUnit_HealRate = _modCategory.CreateEntry<float>("RepairFacility_Alien_LargeUnit_HealRate", 0.01f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Aliens_Queen_HealRate == null)
			{
				_Pref_Aliens_Queen_HealRate = _modCategory.CreateEntry<float>("RepairFacility_Alien_Queen_HealRate", 0.01f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Aliens_Structure_HealRate == null)
			{
				_Pref_Aliens_Structure_HealRate = _modCategory.CreateEntry<float>("RepairFacility_Alien_Structure_HealRate", 0.01f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_SiegeDefenders_Structure_HealRate == null)
			{
				_Pref_SiegeDefenders_Structure_HealRate = _modCategory.CreateEntry<float>("RepairFacility_SiegeDefender_Structure_HealRate", 0.025f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Repair_Notification == null)
			{
				_Pref_Repair_Notification = _modCategory.CreateEntry<bool>("RepairFacility_ChatNotifications", false, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Repair_SoundEffects == null)
			{
				_Pref_Repair_SoundEffects = _modCategory.CreateEntry<bool>("RepairFacility_SoundEffects", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			vehiclesAtRepairShop = new List<Unit>();
		}

		public override void OnUpdate()
		{
			try
			{
				Timer_HealVehicles += Time.deltaTime;
				if (Timer_HealVehicles > 5f)
				{
					Timer_HealVehicles = 0f;
					CleanRepairList();
					Repair_HandleCentauriUnits(Repair_GetCentauriUnits());
					Repair_UnitsAtRepairShop();
					Repair_DefenderStructures();
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run OnUpdate");
			}
		}

		public static List<Unit> Repair_GetCentauriUnits()
		{
			List<Unit> list = new List<Unit>();
			foreach (Structure structure in Structure.Structures)
			{
				if (structure == null || structure.Team == null || structure.Team.Index != 3 || structure.ObjectInfo == null || structure.ObjectInfo.StructureType != StructureType.Production || structure.ObjectInfo.StructureSelectionType != StructureSelectionType.Units2)
				{
					continue;
				}
				Collider[] array = Physics.OverlapBox(structure.transform.position, halfExtentsRepairCheck, structure.transform.rotation);
				for (int i = 0; i < array.Length; i++)
				{
					BaseGameObject baseGameObject = array[i].gameObject.GetBaseGameObject();
					if (baseGameObject == null)
					{
						continue;
					}
					NetworkComponent networkComponent = baseGameObject.NetworkComponent;
					if (networkComponent == null)
					{
						continue;
					}
					Player ownerPlayer = networkComponent.OwnerPlayer;
					if (!(ownerPlayer == null) && !(ownerPlayer.Team != structure.Team))
					{
						Unit controlledUnit = ownerPlayer.ControlledUnit;
						if (controlledUnit == null)
						{
							MelonLogger.Warning("Found invalid unit in LVF repair zone.");
						}
						else if (!(controlledUnit.DriverCompartment == null) && !controlledUnit.DamageManager.IsDestroyed && !list.Contains(controlledUnit))
						{
							list.Add(controlledUnit);
							MelonLogger.Msg("Found alive player-controlled unit on LVF collider: " + ownerPlayer.PlayerName + " " + controlledUnit.ObjectInfo.DisplayName);
						}
					}
				}
			}
			return list;
		}

		public static List<Unit> Repair_GetStaleCentauriUnits(List<Unit> centauriUnitsAtRepairShop)
		{
			List<Unit> list = new List<Unit>();
			foreach (Unit item in vehiclesAtRepairShop)
			{
				if (item.Team.Index == 3 && !centauriUnitsAtRepairShop.Contains(item))
				{
					list.Add(item);
				}
			}
			return list;
		}

		public static void Repair_HandleCentauriUnits(List<Unit> centauriUnitsAtRepairShop)
		{
			foreach (Unit item in centauriUnitsAtRepairShop)
			{
				if (!vehiclesAtRepairShop.Contains(item))
				{
					vehiclesAtRepairShop.Add(item);
					MelonLogger.Msg("Found player's " + (item.IsFlyingType ? "aircraft" : "vehicle") + " entering LVF repair zone: " + item.ControlledBy.PlayerName + " with vehicle " + item.ObjectInfo.DisplayName);
					if (_Pref_Repair_Notification.Value)
					{
						HelperMethods.SendChatMessageToPlayer(item.ControlledBy, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Entered vehicle repair zone.");
					}
					HandleRepairSoundEffects(item.ControlledBy, startingRepair: true);
				}
			}
			foreach (Unit centauriUnitToRemove in Repair_GetStaleCentauriUnits(centauriUnitsAtRepairShop))
			{
				vehiclesAtRepairShop.RemoveAll((Unit vehicle) => vehicle == centauriUnitToRemove);
				MelonLogger.Msg("Found player's " + (centauriUnitToRemove.IsFlyingType ? "aircraft" : "vehicle") + " exiting LVF repair zone: " + centauriUnitToRemove.ControlledBy.PlayerName + " with vehicle " + centauriUnitToRemove.ObjectInfo.DisplayName);
				if (_Pref_Repair_Notification.Value)
				{
					HelperMethods.SendChatMessageToPlayer(centauriUnitToRemove.ControlledBy, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Left vehicle repair zone.");
				}
				HandleRepairSoundEffects(centauriUnitToRemove.ControlledBy, startingRepair: false);
			}
		}

		public static void Repair_UnitsAtRepairShop()
		{
			foreach (Unit item in vehiclesAtRepairShop)
			{
				float num = item.DamageManager.MaxHealth * (item.IsFlyingType ? _Pref_Humans_Aircraft_HealRate.Value : _Pref_Humans_Vehicle_HealRate.Value);
				if (item.DamageManager.Health >= item.DamageManager.MaxHealth)
				{
					if (_Pref_Repair_Notification.Value && item.ControlledBy != null)
					{
						HelperMethods.SendConsoleMessageToPlayer(item.ControlledBy, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Debug Info: (Skipping Repairs) Health[" + item.DamageManager.Health + "] MaxHP[" + item.DamageManager.MaxHealth + "] HealAmt[" + num + "]");
					}
					continue;
				}
				float health = Mathf.Clamp(item.DamageManager.Health + num, 0.1f, item.DamageManager.MaxHealth);
				item.DamageManager.SetHealth(health);
				if (_Pref_Repair_Notification.Value && item.ControlledBy != null)
				{
					HelperMethods.SendConsoleMessageToPlayer(item.ControlledBy, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", " Debug Info: (Repair) Health[" + item.DamageManager.Health + "] MaxHP[" + item.DamageManager.MaxHealth + "] HealAmt[" + num + "]");
				}
			}
		}

		private static void HandleRepairSoundEffects(Player player, bool startingRepair)
		{
			if (_Pref_Repair_SoundEffects.Value)
			{
				AudioHelper.PlaySoundFile(startingRepair ? Path.Combine(MelonEnvironment.UserDataDirectory, "sounds\\repair_start.wav") : Path.Combine(MelonEnvironment.UserDataDirectory, "sounds\\repair_end.wav"), player);
			}
		}

		public static void Repair_DefenderStructures()
		{
			if (!(GameMode.CurrentGameMode is MP_TowerDefense))
			{
				return;
			}
			foreach (Structure structure in Team.Teams[4].Structures)
			{
				if (!structure.DamageManager || structure.DamageManager.IsDestroyed)
				{
					continue;
				}
				float num = structure.DamageManager.MaxHealth * _Pref_SiegeDefenders_Structure_HealRate.Value;
				if (structure.DamageManager.Health != structure.DamageManager.MaxHealth)
				{
					float num2 = structure.DamageManager.Health + num;
					if (num2 >= structure.DamageManager.MaxHealth)
					{
						structure.DamageManager.SetHealth01(1f);
						continue;
					}
					float health = Mathf.Clamp(num2, 0f, structure.DamageManager.MaxHealth);
					structure.DamageManager.SetHealth(health);
				}
			}
		}

		public static void CleanRepairList()
		{
			if (vehiclesAtRepairShop.RemoveAll((Unit vehicle) => vehicle == null) > 0)
			{
				MelonLogger.Warning("Removed null element(s) from Repair List");
			}
			if (vehiclesAtRepairShop.RemoveAll((Unit vehicle) => vehicle.DamageManager.IsDestroyed) > 0)
			{
				MelonLogger.Warning("Removed destroyed units from Repair List");
			}
			if (vehiclesAtRepairShop.RemoveAll((Unit vehicle) => vehicle.NetworkComponent == null) > 0)
			{
				MelonLogger.Warning("Removed units with null network components from Repair List");
			}
		}
	}
}
