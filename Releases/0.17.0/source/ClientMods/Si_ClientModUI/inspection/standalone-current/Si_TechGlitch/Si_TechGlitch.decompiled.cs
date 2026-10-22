using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using MelonLoader;
using Microsoft.CodeAnalysis;
using Si_TechGlitch;
using SilicaAdminMod;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(TechGlitch), "Tech Glitch Command", "1.1.0", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_TechGlitch")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_TechGlitch")]
[assembly: AssemblyTitle("Si_TechGlitch")]
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
namespace Si_TechGlitch
{
	public class TechGlitch : MelonMod
	{
		public override void OnLateInitializeMelon()
		{
			HelperMethods.CommandCallback commandCallback = Command_TechGlitch;
			HelperMethods.RegisterPlayerCommand("techglitch", commandCallback, hideFromChat: false);
		}

		public static void Command_TechGlitch(Player? callerPlayer, string args)
		{
			if (!(callerPlayer == null) && !(callerPlayer.Team == null))
			{
				GameModeExt gameModeExt = GameMode.CurrentGameMode as GameModeExt;
				if (gameModeExt == null)
				{
					MelonLogger.Warning("Could not find GameModeExt instance from CurrentGameMode.");
					return;
				}
				if (gameModeExt.GameOngoing)
				{
					HelperMethods.ReplyToCommand_Player(callerPlayer, ": game must be active to use !techglitch");
					return;
				}
				if (gameModeExt.GetCommanderForTeam(callerPlayer.Team) != callerPlayer)
				{
					HelperMethods.ReplyToCommand_Player(callerPlayer, ": only the commander can use !techglitch");
					return;
				}
				HelperMethods.SendChatMessageToTeam(callerPlayer.Team, "Server has you at Tech Tier " + callerPlayer.Team.TechnologyTier + "/" + callerPlayer.Team.TechnologyTierLimitMax + " with a watermark tech tier of " + callerPlayer.Team.TechnologyTierHighestReached);
				callerPlayer.Team.UpdateTechnologyTier(validate: false, forceUpdate: true);
				HelperMethods.ReplyToCommand_Player(callerPlayer, "forced tech sync.");
			}
		}
	}
}
