using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using MelonLoader;
using Microsoft.CodeAnalysis;
using Si_EndRound;
using SilicaAdminMod;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(EndRound), "End Round", "1.1.0", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_EndRound")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_EndRound")]
[assembly: AssemblyTitle("Si_EndRound")]
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
namespace Si_EndRound
{
	public class EndRound : MelonMod
	{
		public override void OnLateInitializeMelon()
		{
			HelperMethods.CommandCallback adminCallback = Command_EndRound;
			HelperMethods.RegisterAdminCommand("endround", adminCallback, Power.End, "Ends the current round of the game. Usage: !endround");
			HelperMethods.RegisterAdminCommand("endgame", adminCallback, Power.End, "Ends the current round of the game. Usage: !endgame");
		}

		public static void Command_EndRound(Player? callerPlayer, string args)
		{
			string text = args.Split(' ')[0];
			if (!GameMode.CurrentGameMode.GameOngoing)
			{
				HelperMethods.SendChatMessageToPlayer(callerPlayer, "<b><color=#DDE98C>[</color><color=#DFA725>SAM</color><color=#DDE98C>]</color></b> ", text, ": Cannot end round until round starts");
			}
			else
			{
				Force_EndRound();
				HelperMethods.AlertAdminAction(callerPlayer, "ended the round");
			}
		}

		public static void Force_EndRound()
		{
			GameMode.CurrentGameMode.EndRound();
		}
	}
}
