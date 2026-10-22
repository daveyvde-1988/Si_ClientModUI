using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using Microsoft.CodeAnalysis;
using Si_MatchSettings;
using SilicaAdminMod;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(MatchSettingsOverride), "Match Settings Override", "2.0.1", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_MatchSettings")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_MatchSettings")]
[assembly: AssemblyTitle("Si_MatchSettings")]
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
namespace Si_MatchSettings
{
	public class MatchSettingsOverride : MelonMod
	{
		[HarmonyPatch(typeof(GameModeExt), "UpdateUnitCapMultFromSetting")]
		private static class ApplyPatch_GameModeExt_UpdateUnitCapMultFromSetting
		{
			private static void Postfix(GameModeExt __instance)
			{
				try
				{
					if (!(__instance == null))
					{
						int num = _UnitCap.Value;
						if (num < 0)
						{
							num = 0;
						}
						typeof(GameModeExt).GetProperty("UnitCapMultiplier").SetValue(__instance, num);
						if (num == 0)
						{
							MelonLogger.Msg("Setting unit cap to unlimited.");
						}
						else
						{
							MelonLogger.Msg("Setting unit cap to: " + num);
						}
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run GameModeExt::UpdateUnitCapMultFromSetting");
				}
			}
		}

		[HarmonyPatch(typeof(MP_Deathmatch), "StartGameLoop")]
		private static class ApplyPatch_MP_Deathmatch_StartGameLoop
		{
			private static void Postfix(MP_Deathmatch __instance)
			{
				try
				{
					if (!(__instance == null))
					{
						int num = _BotCount.Value;
						if (num < 0)
						{
							num = 0;
						}
						typeof(MP_Deathmatch).GetField("BotCount").SetValue(__instance, num);
						MelonLogger.Msg("Setting bot count to: " + num);
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_Deathmatch::StartGameLoop");
				}
			}
		}

		[HarmonyPatch(typeof(MP_Strategy), "UpdateInitialStrategySettings")]
		private static class ApplyPatch_MP_Strategy_UpdateInitialStrategySettings
		{
			private static void Postfix(MP_Strategy __instance)
			{
				try
				{
					if (__instance == null)
					{
						return;
					}
					foreach (Team team in Team.Teams)
					{
						if (team.Index == 0)
						{
							team.TechnologyTierLimitMin = _Alien_TechMin.Value;
							team.TechnologyTierLimitMax = _Alien_TechMax.Value;
							MelonLogger.Msg("Setting Alien Tech Min/Max to: " + _Alien_TechMin.Value + "/" + _Alien_TechMax.Value);
						}
						else if (team.Index == 4 || team.Index == 3)
						{
							team.TechnologyTierLimitMin = _Human_TechMin.Value;
							team.TechnologyTierLimitMax = _Human_TechMax.Value;
							MelonLogger.Msg("Setting Human Tech Min/Max to: " + _Human_TechMin.Value + "/" + _Human_TechMax.Value);
						}
					}
					float num = Math.Clamp(_DistanceStart.Value, 0.25f, 1f);
					typeof(MP_Strategy).GetField("MultStartingDist", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, num);
					MelonLogger.Msg("Setting MultStartingDist to: " + num);
					float num2 = Math.Clamp(_ResourceLocations.Value, 1f / 32f, 4f);
					float num3 = Math.Clamp(_HiddenResourceLocations.Value, 0f, 1f);
					typeof(MP_Strategy).GetField("MultLocationRes", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, num2);
					typeof(MP_Strategy).GetField("MultLocationResHide", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, num3);
					MelonLogger.Msg("Setting MultLocationRes to: " + num2);
					MelonLogger.Msg("Setting MultLocationResHide to: " + num3);
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run MP_Strategy::UpdateInitialStrategySettings");
				}
			}
		}

		private static MelonPreferences_Category _modCategory;

		private static MelonPreferences_Entry<int> _UnitCap;

		private static MelonPreferences_Entry<int> _BotCount;

		private static MelonPreferences_Entry<int> _Alien_TechMin;

		private static MelonPreferences_Entry<int> _Alien_TechMax;

		private static MelonPreferences_Entry<int> _Human_TechMin;

		private static MelonPreferences_Entry<int> _Human_TechMax;

		private static MelonPreferences_Entry<float> _DistanceStart;

		private static MelonPreferences_Entry<float> _ResourceLocations;

		private static MelonPreferences_Entry<float> _HiddenResourceLocations;

		public override void OnInitializeMelon()
		{
			_modCategory = MelonPreferences.CreateCategory("Silica");
			_UnitCap = _modCategory.CreateEntry<int>("UnitCap", 0, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			_BotCount = _modCategory.CreateEntry<int>("BotCount", 16, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			_Alien_TechMin = _modCategory.CreateEntry<int>("TechMin_Alien", 0, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			_Alien_TechMax = _modCategory.CreateEntry<int>("TechMax_Alien", 8, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			_Human_TechMin = _modCategory.CreateEntry<int>("TechMin_Human", 0, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			_Human_TechMax = _modCategory.CreateEntry<int>("TechMax_Human", 8, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			_DistanceStart = _modCategory.CreateEntry<float>("DistanceStart", 0.5f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			_ResourceLocations = _modCategory.CreateEntry<float>("ResourceLocations", 0.8f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			_HiddenResourceLocations = _modCategory.CreateEntry<float>("HiddenResourceLocations", 0.5f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
		}
	}
}
