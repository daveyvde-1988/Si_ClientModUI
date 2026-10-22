using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using Microsoft.CodeAnalysis;
using Si_ChipmunkVoices;
using SilicaAdminMod;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(ChipmunkVoices), "Chipmunk Voices", "0.9.2", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_ChipmunkVoices")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_ChipmunkVoices")]
[assembly: AssemblyTitle("Si_ChipmunkVoices")]
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
namespace Si_ChipmunkVoices
{
	public class ChipmunkVoices : MelonMod
	{
		[HarmonyPatch(typeof(NetworkLayer), "RelayVoiceStreamPacket")]
		public class ApplyPatch_NetworkLayer_RelayVoiceStreamPacket
		{
			private static bool Prefix(ref byte[] __0, uint __1, ENetworkPacketSend __2, Player __3, bool __4)
			{
				try
				{
					if (!chipmunkMode || __0 == null)
					{
						return true;
					}
					if (__0[16] > 0 || __3 == null)
					{
						return true;
					}
					int index = __3.GetIndex();
					if (!playerSamplesStaged[index])
					{
						playerSamplesStaged[index] = true;
						int num = 0;
						for (int i = 23; i < __1; i += 2)
						{
							__0[num] = __0[i];
							playerVoiceSamples[index, num++] = __0[i];
						}
						MelonLogger.Msg("Stored voice with sample count: " + num);
						return false;
					}
					byte[] array = new byte[512];
					int num2 = 0;
					for (int j = 23; j < __1; j += 2)
					{
						array[num2++] = __0[j];
					}
					int num3 = 23;
					for (int k = 0; k < 512; k++)
					{
						__0[num3++] = playerVoiceSamples[index, k];
					}
					for (num2 = 0; num2 < 512; num2++)
					{
						__0[num3++] = array[num2];
					}
					MelonLogger.Msg("Finished writing buffer of sample count: " + num3);
					playerSamplesStaged[index] = false;
				}
				catch (Exception exception)
				{
					HelperMethods.PrintError(exception, "Failed to run NetworkLayer::RelayVoiceStreamPacket");
				}
				return true;
			}
		}

		private static bool chipmunkMode = ShouldHaveChipmunks();

		private static byte[,] playerVoiceSamples = new byte[68, 512];

		private static bool[] playerSamplesStaged = new bool[68];

		public override void OnLateInitializeMelon()
		{
			HelperMethods.CommandCallback adminCallback = Command_Chipmunk;
			HelperMethods.RegisterAdminCommand("chipmunk", adminCallback, Power.Slay, "Toggles chipmunk voices. Usage: !chipmunk");
		}

		public static bool ShouldHaveChipmunks()
		{
			DateTime today = DateTime.Today;
			if (today.Month == 4 && today.Day == 1)
			{
				return true;
			}
			return false;
		}

		public static void Command_Chipmunk(Player? callerPlayer, string args)
		{
			string text = args.Split(' ')[0];
			chipmunkMode = !chipmunkMode;
			HelperMethods.ReplyToCommand(text + ": Chipmunk mode " + (chipmunkMode ? "ENABLED" : "DISABLED"));
		}
	}
}
