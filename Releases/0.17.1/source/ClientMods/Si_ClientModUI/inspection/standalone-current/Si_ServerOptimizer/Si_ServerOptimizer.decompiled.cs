using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Baltarus.Blueprints;
using DebugTools;
using HarmonyLib;
using InstancedLODBatching;
using MelonLoader;
using Microsoft.CodeAnalysis;
using Sandbox.Flow.Gizmos;
using Si_ServerOptimizer;
using Silica;
using Silica.UI;
using Silica.UI.Loading;
using Silica.UI.Menus;
using Silica.Voting;
using TMPro;
using UnityEngine.Rendering.HighDefinition;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(Optimizer), "Server Optimizer", "0.7.2", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_ServerOptimizer")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+17f1aa45fa1403d6b033f5c4b3839f68b78e8f34")]
[assembly: AssemblyProduct("Si_ServerOptimizer")]
[assembly: AssemblyTitle("Si_ServerOptimizer")]
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
namespace Si_ServerOptimizer
{
	public class Optimizer : MelonMod
	{
		[HarmonyPatch(typeof(GameSettings), "Load", new Type[] { })]
		internal class Patch_Disable_GameSettings
		{
			private static bool Prefix(GameSettings? __instance)
			{
				MelonLogger.Msg("Disable user settings...");
				__instance = null;
				return false;
			}
		}

		[HarmonyPatch(typeof(Game), "UpdateCameraFOV")]
		internal class Patch_Disable_Game_UpdateGameraFOV
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(FreeFlyCameraController), "Awake")]
		internal class Patch_Disable_FreeFlyCameraController1
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(FreeFlyCameraController), "OnEnable")]
		internal class Patch_Disable_FreeFlyCameraController2
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(UIManager), "LateUpdate")]
		internal class Patch_Disable_UIUpdates
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(DecalProjector), "OnEnable")]
		internal class Patch_Disable_DecalProjector1
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(DecalProjector), "Awake")]
		internal class Patch_Disable_DecalProjector2
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(DisableBasedOnDistance), "Awake")]
		internal class Patch_Disable_DistanceCulling1
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(CullingComponent), "OnEnable")]
		internal class Patch_Disable_DistanceCulling2
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(CullingComponent), "OnDisable")]
		internal class Patch_Disable_DistanceCulling3
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(DisableBasedOnDistance), "UpdateBasedOnDistance")]
		internal class Patch_Disable_DistanceCulling4
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(LoadingScreenController), "Awake")]
		internal class Patch_Disable_LoadingScreenController1
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(LoadingScreenController), "Update")]
		internal class Patch_Disable_LoadingScreenController2
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(InstancedLODBatch), "AddObject")]
		internal class Patch_Disable_LOD1
		{
			private static bool Prefix(bool __result)
			{
				__result = false;
				return false;
			}
		}

		[HarmonyPatch(typeof(InstancedLODBatcher), "Update")]
		internal class Patch_Disable_LOD2
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(InstancedLODBatcher), "ResetAll")]
		internal class Patch_Disable_LOD3
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(InstancedLODBatcher), "Start")]
		internal class Patch_Disable_LOD4
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(InstancedLODBatcher), "Init")]
		internal class Patch_Disable_LOD5
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(InstancedLODBatch), "Init")]
		internal class Patch_Disable_LOD6
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(InstancedLODBatcher), "EnableAll")]
		internal class Patch_Disable_LOD7
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(InstancedLODBatcher), "EnableInstanced")]
		internal class Patch_Disable_LOD8
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(UnitAnimator), "OnAnimatorUpdate")]
		internal class Patch_Disable_Animator_Unit2
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(StructureTaskUI), "OnEnable")]
		internal class Patch_Disable_UI_StructureTask1
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(StructureTaskUI), "Awake")]
		internal class Patch_Disable_UI_StructureTask2
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(AudioEffectHandler), "OnEnable")]
		internal class Patch_Disable_Audio_EffectHandler1
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(AudioEffectHandler), "OrderedUpdate")]
		internal class Patch_Disable_Audio_EffectHandler2
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(CameraCentric), "LateUpdate")]
		internal class Patch_Disable_Camera1
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(Game), "GetControlsEnabled")]
		internal class Patch_Disable_Input_Controls
		{
			private static bool Prefix(bool __result)
			{
				__result = false;
				return false;
			}
		}

		[HarmonyPatch(typeof(GameInputAsset), "Initialize")]
		internal class Patch_Disable_Input_Keyboard1
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(ActionMapContainer.ActionContainer), "Process")]
		internal class Patch_Disable_Input_Keyboard2
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(ActionMapContainer), "ProcessInput")]
		internal class Patch_Disable_Input_Keyboard3
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(JiggleBone), "Awake")]
		internal class Patch_Disable_Jiggling1
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(JiggleBone), "OnEnable")]
		internal class Patch_Disable_Jiggling2
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(JiggleBone), "Init")]
		internal class Patch_Disable_Jiggling3
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(JiggleBone), "OrderedLateUpdate")]
		internal class Patch_Disable_Jiggling4
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(VoteSystem), "OnEnable")]
		internal class Patch_Modify_VoteSystem1
		{
			private static bool Prefix(VoteSystem __instance)
			{
				MethodInfo method = AccessTools.Method(typeof(VoteSystem), "OnPlayerLeft", (Type[])null, (Type[])null);
				Action<Player> b = (Action<Player>)Delegate.CreateDelegate(typeof(Action<Player>), __instance, method);
				GameEvents.OnPlayerLeft = (Action<Player>)Delegate.Combine(GameEvents.OnPlayerLeft, b);
				return false;
			}
		}

		[HarmonyPatch(typeof(VoteSystem), "OnDisable")]
		internal class Patch_Modify_VoteSystem2
		{
			private static bool Prefix(VoteSystem __instance)
			{
				MethodInfo method = AccessTools.Method(typeof(VoteSystem), "OnPlayerLeft", (Type[])null, (Type[])null);
				Action<Player> value = (Action<Player>)Delegate.CreateDelegate(typeof(Action<Player>), __instance, method);
				GameEvents.OnPlayerLeft = (Action<Player>)Delegate.Remove(GameEvents.OnPlayerLeft, value);
				return false;
			}
		}

		[HarmonyPatch(typeof(TimeManager), "OnEnable")]
		internal class Patch_Modify_TimeManager1
		{
			private static bool Prefix(TimeManager __instance)
			{
				MethodInfo method = AccessTools.Method(typeof(TimeManager), "OnGameSpeedChanged", (Type[])null, (Type[])null);
				Action b = (Action)Delegate.CreateDelegate(typeof(Action), __instance, method);
				TimeManager.GameSpeedChanged = (Action)Delegate.Combine(TimeManager.GameSpeedChanged, b);
				return false;
			}
		}

		[HarmonyPatch(typeof(TimeManager), "OnDisable")]
		internal class Patch_Modify_TimeManager2
		{
			private static bool Prefix(TimeManager __instance)
			{
				MethodInfo method = AccessTools.Method(typeof(TimeManager), "OnGameSpeedChanged", (Type[])null, (Type[])null);
				Action value = (Action)Delegate.CreateDelegate(typeof(Action), __instance, method);
				TimeManager.GameSpeedChanged = (Action)Delegate.Remove(TimeManager.GameSpeedChanged, value);
				return false;
			}
		}

		[HarmonyPatch(typeof(TextMeshProUGUI), "Awake")]
		internal class Patch_Disable_TextMeshProUGUI
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(GameInput), "ProcessInput")]
		internal class Patch_Disable_GameManager_ProcessInput
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(GameInput), "GetButtonDown")]
		internal class Patch_Disable_GameManager_Input_GetButtonDown
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(Game), "UpdateFading")]
		internal class Patch_Disable_GameManager_UpdateFading
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(DebugConsole), "HandleToggling")]
		internal class Patch_Disable_GameManager_DebugHandleToggling
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(GameInput), "UpdateInput")]
		internal class Patch_Disable_GameManager_UpdateInput
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(Game), "UpdateCameraFOV")]
		internal class Patch_Disable_GameManager_UpdateCameraFOV
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(VisionModeObject), "UpdateNewlyAddedObjects")]
		internal class Patch_Disable_GameManager_VisionUpdateNewlyAddedObjects
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(CinematicCamera), "Update")]
		internal class Patch_Disable_GameManager_CinematicCameraUpdate
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(Unit), "UpdateRigidBodyInterpolationAll")]
		internal class Patch_Disable_GameManager_UnitUpdateRigidBodyInterpolationAll
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(VehicleParticleFX), "TickVehicleParticleFX")]
		internal class Patch_Disable_GameManager_TickVehicleParticleFX
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(DisableBasedOnDistance), "TickDisableBasedOnDistance")]
		internal class Patch_Disable_GameManager_DistanceCullingTick
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(AudioAmbientSource), "UpdateAmbients")]
		internal class Patch_Disable_GameManager_AudioUpdateAmbients
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(CursorWaiting), "UpdateShow")]
		internal class Patch_Disable_GameManager_CursorWaitingUpdateShow
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(LightGrid), "Update")]
		internal class Patch_Disable_GameManager_LightGridUpdate
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(MenuManager), "Awake")]
		internal class Patch_Disable_GameManager_MenuManagerAwake
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(ScreenPauseHint), "Update")]
		internal class Patch_Disable_GameManager_ScreenPauseHintUpdate
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(GizmoManager), "OnEnable")]
		internal class Patch_Disable_GameManager_GizmoManager_OnEnable
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(GizmoManager), "OnDisable")]
		internal class Patch_Disable_GameManager_GizmoManager_OnDisable
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(BlueprintEditor), "OnEnable")]
		internal class Patch_Disable_GameManager_BlueprintEditor_OnEnable
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(BlueprintEditor), "OnDisable")]
		internal class Patch_Disable_GameManager_BlueprintEditor_OnDisable
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(BlueprintSpawner), "OnEnable")]
		internal class Patch_Disable_GameManager_BlueprintSpawner_OnEnable
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(BlueprintSpawner), "OnDisable")]
		internal class Patch_Disable_GameManager_BlueprintSpawner_OnDisable
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(FPSCommanding), "Start")]
		internal class Patch_Modify_FPSCommandingStart
		{
			private static bool Prefix(FPSCommanding __instance)
			{
				MethodInfo method = AccessTools.Method(typeof(FPSCommanding), "OnPlayerChangedTeam", (Type[])null, (Type[])null);
				MethodInfo method2 = AccessTools.Method(typeof(FPSCommanding), "OnPlayerChangedUnit", (Type[])null, (Type[])null);
				Action<Player, Unit, Unit> b = (Action<Player, Unit, Unit>)Delegate.CreateDelegate(typeof(Action<Player, Unit, Unit>), __instance, method2);
				Action<Player, Team, Team> b2 = (Action<Player, Team, Team>)Delegate.CreateDelegate(typeof(Action<Player, Team, Team>), __instance, method);
				GameEvents.OnPlayerChangedTeam = (Action<Player, Team, Team>)Delegate.Combine(GameEvents.OnPlayerChangedTeam, b2);
				GameEvents.OnPlayerChangedUnit = (Action<Player, Unit, Unit>)Delegate.Combine(GameEvents.OnPlayerChangedUnit, b);
				AccessTools.Field(typeof(FPSCommanding), "networkComponent").SetValue(__instance, __instance.gameObject.GetComponent<NetworkComponent>());
				return false;
			}
		}

		[HarmonyPatch(typeof(FPSCommanding), "OnDestroy")]
		internal class Patch_Modify_FPSCommandingOnDestroy
		{
			private static bool Prefix(FPSCommanding __instance)
			{
				MethodInfo method = AccessTools.Method(typeof(FPSCommanding), "OnPlayerChangedTeam", (Type[])null, (Type[])null);
				MethodInfo method2 = AccessTools.Method(typeof(FPSCommanding), "OnPlayerChangedUnit", (Type[])null, (Type[])null);
				Action<Player, Unit, Unit> value = (Action<Player, Unit, Unit>)Delegate.CreateDelegate(typeof(Action<Player, Unit, Unit>), __instance, method2);
				Action<Player, Team, Team> value2 = (Action<Player, Team, Team>)Delegate.CreateDelegate(typeof(Action<Player, Team, Team>), __instance, method);
				GameEvents.OnPlayerChangedTeam = (Action<Player, Team, Team>)Delegate.Remove(GameEvents.OnPlayerChangedTeam, value2);
				GameEvents.OnPlayerChangedUnit = (Action<Player, Unit, Unit>)Delegate.Remove(GameEvents.OnPlayerChangedUnit, value);
				return false;
			}
		}
	}
}
