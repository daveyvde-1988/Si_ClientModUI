using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using Microsoft.CodeAnalysis;
using Si_TimeOfDay;
using SilicaAdminMod;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(ServerTime), "Time of Day", "0.9.9", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_TimeOfDay")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_TimeOfDay")]
[assembly: AssemblyTitle("Si_TimeOfDay")]
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
namespace Si_TimeOfDay
{
	public class ServerTime : MelonMod
	{
		[HarmonyPatch(typeof(WorldLight), "OnSendNetInit")]
		private static class TimeOfDay_Patch_WordLight_OnSendNetInit
		{
			public static void Postfix(WorldLight __instance, GameByteStreamWriter __0)
			{
				try
				{
					WorldLight.CurrentWorldLight.DayFraction = Pref_DayTime_Ratio.Value;
					float dayLength = (float)Pref_DayTime_Length.Value / 60f;
					WorldLight.CurrentWorldLight.DayLength = dayLength;
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run WorldLight::OnSendNetInit");
				}
			}
		}

		[HarmonyPatch(typeof(WorldLight), "SetDayTime")]
		private static class TimeOfDay_Patch_WorldLight_SetDayTime
		{
			public static void Prefix(ref float __0)
			{
				try
				{
					if ((double)Math.Abs(__0 - 11f) < 0.1)
					{
						float num = DateTime.Now.Hour;
						__0 = num;
					}
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run WorldLight::SetDayTime");
				}
			}
		}

		private const float startTime = 11f;

		private static MelonPreferences_Category _modCategory;

		private static MelonPreferences_Entry<float> Pref_DayTime_Ratio;

		private static MelonPreferences_Entry<int> Pref_DayTime_Length;

		public override void OnInitializeMelon()
		{
			if (_modCategory == null)
			{
				_modCategory = MelonPreferences.CreateCategory("Silica");
			}
			if (Pref_DayTime_Ratio == null)
			{
				Pref_DayTime_Ratio = _modCategory.CreateEntry<float>("TimeOfDay_DayLight_Ratio", 0.75f, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (Pref_DayTime_Length == null)
			{
				Pref_DayTime_Length = _modCategory.CreateEntry<int>("TimeOfDay_LengthOfDay_InRealMinutes", 120, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
		}

		public override void OnLateInitializeMelon()
		{
			HelperMethods.CommandCallback adminCallback = Command_SetTime;
			HelperMethods.RegisterAdminCommand("time", adminCallback, Power.Cheat, "Sets the current game time. Usage: !time [0.0 - 23.99]");
		}

		public static void Command_SetTime(Player? callerPlayer, string args)
		{
			string text = args.Split(' ')[0];
			int num = args.Split(' ').Length - 1;
			float result;
			if (num > 1)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Too many arguments");
			}
			else if (num < 1)
			{
				HelperMethods.ReplyToCommand("Current time is " + WorldLight.GetDayTime());
			}
			else if (!float.TryParse(args.Split(' ')[1], out result))
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Ambiguous time value provided");
			}
			else
			{
				WorldLight.SetDayTime(result);
				HelperMethods.AlertAdminAction(callerPlayer, "adjusted time to " + result);
			}
		}
	}
}
