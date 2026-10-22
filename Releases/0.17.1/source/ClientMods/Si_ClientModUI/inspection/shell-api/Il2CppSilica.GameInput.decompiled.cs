using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Il2Cpp;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Il2CppSilica;

public static class GameInput : Il2CppSystem.Object
{
	[System.Serializable]
	[OriginalName("SilicaCore.dll", "", "InputMap")]
	[System.Flags]
	public enum InputMap
	{
		None = 1,
		Global = 2,
		UI = 4,
		Unit = 8,
		Commander = 0x10,
		Gamepad = 0x20,
		Freecam = 0x40,
		Cheats = 0x80,
		Sandbox = 0x100,
		Voting = 0x200
	}

	public sealed class ActionLabelProvider : Il2CppSystem.MulticastDelegate
	{
		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_Object_IntPtr_0;

		private static readonly System.IntPtr NativeMethodInfoPtr_Invoke_Public_Virtual_New_Boolean_InputAction_byref_String_byref_String_0;

		private static readonly System.IntPtr NativeMethodInfoPtr_BeginInvoke_Public_Virtual_New_IAsyncResult_InputAction_byref_String_byref_String_AsyncCallback_Object_0;

		private static readonly System.IntPtr NativeMethodInfoPtr_EndInvoke_Public_Virtual_New_Boolean_byref_String_byref_String_IAsyncResult_0;

		static ActionLabelProvider()
		{
			Il2CppClassPointerStore<ActionLabelProvider>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "ActionLabelProvider");
			NativeMethodInfoPtr__ctor_Public_Void_Object_IntPtr_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ActionLabelProvider>.NativeClassPtr, 100682804);
			NativeMethodInfoPtr_Invoke_Public_Virtual_New_Boolean_InputAction_byref_String_byref_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ActionLabelProvider>.NativeClassPtr, 100682805);
			NativeMethodInfoPtr_BeginInvoke_Public_Virtual_New_IAsyncResult_InputAction_byref_String_byref_String_AsyncCallback_Object_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ActionLabelProvider>.NativeClassPtr, 100682806);
			NativeMethodInfoPtr_EndInvoke_Public_Virtual_New_Boolean_byref_String_byref_String_IAsyncResult_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ActionLabelProvider>.NativeClassPtr, 100682807);
		}

		[CallerCount(1)]
		[CachedScanResults(RefRangeStart = 607770, RefRangeEnd = 607771, XrefRangeStart = 607766, XrefRangeEnd = 607770, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe ActionLabelProvider(Il2CppSystem.Object @object, System.IntPtr method)
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ActionLabelProvider>.NativeClassPtr))
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[2];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)@object);
			*(System.IntPtr**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &method;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_Object_IntPtr_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		public unsafe virtual bool Invoke(InputAction action, out string mapLabel, out string actionLabel)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[3];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
			byte* num = (byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)));
			nint num2 = 0;
			*(nint**)num = &num2;
			byte* num3 = (byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)));
			nint num4 = 0;
			*(nint**)num3 = &num4;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Invoke_Public_Virtual_New_Boolean_InputAction_byref_String_byref_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			mapLabel = IL2CPP.Il2CppStringToManaged((System.IntPtr)num2);
			actionLabel = IL2CPP.Il2CppStringToManaged((System.IntPtr)num4);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 607771, XrefRangeEnd = 607772, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe virtual Il2CppSystem.IAsyncResult BeginInvoke(InputAction action, out string mapLabel, out string actionLabel, Il2CppSystem.AsyncCallback callback, Il2CppSystem.Object @object)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[5];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
			byte* num = (byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)));
			nint num2 = 0;
			*(nint**)num = &num2;
			byte* num3 = (byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)));
			nint num4 = 0;
			*(nint**)num3 = &num4;
			*(System.IntPtr*)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)callback);
			*(System.IntPtr*)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)@object);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_BeginInvoke_Public_Virtual_New_IAsyncResult_InputAction_byref_String_byref_String_AsyncCallback_Object_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			mapLabel = IL2CPP.Il2CppStringToManaged((System.IntPtr)num2);
			actionLabel = IL2CPP.Il2CppStringToManaged((System.IntPtr)num4);
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.IAsyncResult>(intPtr) : null;
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 607772, XrefRangeEnd = 607774, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe virtual bool EndInvoke(out string mapLabel, out string actionLabel, Il2CppSystem.IAsyncResult result)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[3];
			nint num = 0;
			*ptr = (nint)(&num);
			byte* num2 = (byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)));
			nint num3 = 0;
			*(nint**)num2 = &num3;
			*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)result);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_EndInvoke_Public_Virtual_New_Boolean_byref_String_byref_String_IAsyncResult_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			mapLabel = IL2CPP.Il2CppStringToManaged((System.IntPtr)num);
			actionLabel = IL2CPP.Il2CppStringToManaged((System.IntPtr)num3);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		public ActionLabelProvider(System.IntPtr pointer)
			: base(pointer)
		{
		}
	}

	private sealed class MethodInfoStoreGeneric_ReadValue_Public_Static_TValue_String_InputMap_0<TValue>
	{
		internal static System.IntPtr Pointer = IL2CPP.il2cpp_method_get_from_reflection(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)new MethodInfo(IL2CPP.il2cpp_method_get_object(NativeMethodInfoPtr_ReadValue_Public_Static_TValue_String_InputMap_0, Il2CppClassPointerStore<GameInput>.NativeClassPtr)).MakeGenericMethod(new Il2CppReferenceArray<Il2CppSystem.Type>(new Il2CppSystem.Type[1] { Il2CppSystem.Type.internal_from_handle(IL2CPP.il2cpp_class_get_type(Il2CppClassPointerStore<TValue>.NativeClassPtr)) }))));
	}

	private static readonly System.IntPtr NativeFieldInfoPtr_KBM_SCHEME;

	private static readonly System.IntPtr NativeFieldInfoPtr_GAMEPAD_SCHEME;

	private static readonly System.IntPtr NativeFieldInfoPtr_GENERATED_MAP_PREFIX;

	private static readonly System.IntPtr NativeFieldInfoPtr_LabelProvider;

	private static readonly System.IntPtr NativeFieldInfoPtr_OnRebind;

	private static readonly System.IntPtr NativeFieldInfoPtr_OnControlsUpdated;

	private static readonly System.IntPtr NativeFieldInfoPtr_OnSwitchedInput;

	private static readonly System.IntPtr NativeFieldInfoPtr__CurrentControlScheme_k__BackingField;

	private static readonly System.IntPtr NativeFieldInfoPtr__AimDelta_k__BackingField;

	private static readonly System.IntPtr NativeFieldInfoPtr__ScrollDelta_k__BackingField;

	private static readonly System.IntPtr NativeFieldInfoPtr__actionMaps;

	private static readonly System.IntPtr NativeFieldInfoPtr__concurrentMaps;

	private static readonly System.IntPtr NativeFieldInfoPtr__toggleActions;

	private static readonly System.IntPtr NativeFieldInfoPtr__currentAsset;

	private static readonly System.IntPtr NativeFieldInfoPtr__currentPlayerInput;

	private static readonly System.IntPtr NativeFieldInfoPtr__pointerAction;

	private static readonly System.IntPtr NativeFieldInfoPtr__aimAction;

	private static readonly System.IntPtr NativeFieldInfoPtr__scrollAction;

	private static readonly System.IntPtr NativeFieldInfoPtr__currentInput;

	private static readonly System.IntPtr NativeFieldInfoPtr__rebindingOperation;

	private static readonly System.IntPtr NativeFieldInfoPtr__pointerPosition;

	private static readonly System.IntPtr NativeFieldInfoPtr__rightStickPathMasks;

	private static readonly System.IntPtr NativeFieldInfoPtr__leftStickPathMasks;

	private static readonly System.IntPtr NativeFieldInfoPtr__mouseDeltaMask;

	private static readonly System.IntPtr NativeMethodInfoPtr_TryGetActionLabel_Public_Static_Boolean_InputAction_byref_String_byref_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_CurrentAsset_Public_Static_get_GameInputAsset_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_CurrentPlayerInput_Public_Static_get_PlayerInput_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_CurrentControlScheme_Public_Static_get_EControlScheme_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_CurrentControlScheme_Private_Static_set_Void_EControlScheme_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_CurrentInput_Public_Static_get_InputMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_AimDelta_Public_Static_get_Vector2_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_AimDelta_Private_Static_set_Void_Vector2_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_ScrollDelta_Public_Static_get_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_ScrollDelta_Private_Static_set_Void_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_PointerPosition_Public_Static_get_Vector2_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_RegisteredActionMaps_Public_Static_get_IReadOnlyDictionary_2_InputMap_ActionMapContainer_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadValue_Public_Static_TValue_String_InputMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetButtonUp_Public_Static_Boolean_String_InputMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetButtonDown_Public_Static_Boolean_String_InputMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_FindActionInCurrentlyActiveMaps_Private_Static_ActionContainer_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetButton_Public_Static_Boolean_String_InputMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_UsingGamepad_Public_Static_Boolean_String_InputMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetToggle_Public_Static_Boolean_String_InputMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_InterruptToggle_Public_Static_Void_String_InputMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_FindInputMapByName_Public_Static_InputMap_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetPreferredInput_Public_Static_InputMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_UpdateInput_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetToggleMode_Public_Static_Void_String_Boolean_InputMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetToggleMode_Public_Static_Boolean_String_InputMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetPlayerInput_Public_Static_Void_PlayerInput_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetInputModule_Public_Static_Void_InputSystemUIInputModule_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetAsset_Public_Static_Void_GameInputAsset_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_LoadSettings_Public_Static_Void_InputSettings_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ApplyAimSensitivity_Private_Static_Void_InputSettings_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ApplyGamepadDeadzone_Private_Static_Void_InputSettings_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ProcessInput_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetPointerAction_Public_Static_Void_InputMap_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetAimAction_Public_Static_Void_InputMap_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetScrollAction_Public_Static_Void_InputMap_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_RegisterActionMap_Public_Static_Void_InputActionMap_InputMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_RegisterConcurrentMap_Public_Static_Void_InputActionMap_Il2CppStringArray_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_ConcurrentMaps_Public_Static_get_IReadOnlyDictionary_2_InputActionMap_Il2CppStringArray_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_AreConcurrent_Public_Static_Boolean_InputActionMap_InputActionMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SwitchInput_Private_Static_Void_InputMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetActionEnableOverride_Public_Static_Void_InputMap_String_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetCurrentInputValue_Private_Static_Void_InputMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_DebugPrintInputMode_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ProcessInputToggle_Private_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ClearAllInput_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ForceDisableAll_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_EnableAllWithOverride_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetAction_Public_Static_InputAction_InputMap_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetAction_Public_Static_InputAction_InputActionReference_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetActionContainer_Private_Static_ActionContainer_InputMap_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_TryGetValidBinding_Public_Static_Boolean_InputAction_byref_InputBinding_byref_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetDisplayString_Public_Static_String_InputAction_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetDisplayString_Public_Static_String_InputMap_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_OnControlsChanged_Private_Static_Void_PlayerInput_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_OnDeviceChange_Public_Static_Void_InputDevice_InputDeviceChange_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_OnFocusChanged_Public_Static_Void_Boolean_0;

	public unsafe static string KBM_SCHEME
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_KBM_SCHEME, (void*)(&intPtr));
			return IL2CPP.Il2CppStringToManaged(intPtr);
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_KBM_SCHEME, (void*)IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe static string GAMEPAD_SCHEME
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_GAMEPAD_SCHEME, (void*)(&intPtr));
			return IL2CPP.Il2CppStringToManaged(intPtr);
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_GAMEPAD_SCHEME, (void*)IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe static string GENERATED_MAP_PREFIX
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_GENERATED_MAP_PREFIX, (void*)(&intPtr));
			return IL2CPP.Il2CppStringToManaged(intPtr);
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_GENERATED_MAP_PREFIX, (void*)IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe static ActionLabelProvider LabelProvider
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_LabelProvider, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<ActionLabelProvider>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_LabelProvider, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)actionLabelProvider));
		}
	}

	public unsafe static Il2CppSystem.Action<InputAction, InputBinding, int, bool> OnRebind
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_OnRebind, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<InputAction, InputBinding, int, bool>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_OnRebind, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe static Il2CppSystem.Action<string, EControlScheme> OnControlsUpdated
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_OnControlsUpdated, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<string, EControlScheme>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_OnControlsUpdated, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe static Il2CppSystem.Action OnSwitchedInput
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_OnSwitchedInput, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_OnSwitchedInput, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe static EControlScheme _CurrentControlScheme_k__BackingField
	{
		get
		{
			Unsafe.SkipInit(out EControlScheme result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__CurrentControlScheme_k__BackingField, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__CurrentControlScheme_k__BackingField, (void*)(&eControlScheme));
		}
	}

	public unsafe static Vector2 _AimDelta_k__BackingField
	{
		get
		{
			Unsafe.SkipInit(out Vector2 result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__AimDelta_k__BackingField, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__AimDelta_k__BackingField, (void*)(&vector));
		}
	}

	public unsafe static float _ScrollDelta_k__BackingField
	{
		get
		{
			Unsafe.SkipInit(out float result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__ScrollDelta_k__BackingField, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__ScrollDelta_k__BackingField, (void*)(&num));
		}
	}

	public unsafe static Dictionary<InputMap, ActionMapContainer> _actionMaps
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__actionMaps, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dictionary<InputMap, ActionMapContainer>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__actionMaps, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dictionary));
		}
	}

	public unsafe static Dictionary<InputActionMap, Il2CppStringArray> _concurrentMaps
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__concurrentMaps, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dictionary<InputActionMap, Il2CppStringArray>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__concurrentMaps, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dictionary));
		}
	}

	public unsafe static Dictionary<InputMap, Dictionary<string, ActionMapContainer.ToggleContainer>> _toggleActions
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__toggleActions, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dictionary<InputMap, Dictionary<string, ActionMapContainer.ToggleContainer>>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__toggleActions, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dictionary));
		}
	}

	public unsafe static GameInputAsset _currentAsset
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__currentAsset, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<GameInputAsset>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__currentAsset, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gameInputAsset));
		}
	}

	public unsafe static PlayerInput _currentPlayerInput
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__currentPlayerInput, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<PlayerInput>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__currentPlayerInput, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)playerInput));
		}
	}

	public unsafe static ActionMapContainer.ActionContainer _pointerAction
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__pointerAction, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<ActionMapContainer.ActionContainer>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__pointerAction, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)actionContainer));
		}
	}

	public unsafe static ActionMapContainer.ActionContainer _aimAction
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__aimAction, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<ActionMapContainer.ActionContainer>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__aimAction, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)actionContainer));
		}
	}

	public unsafe static ActionMapContainer.ActionContainer _scrollAction
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__scrollAction, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<ActionMapContainer.ActionContainer>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__scrollAction, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)actionContainer));
		}
	}

	public unsafe static InputMap _currentInput
	{
		get
		{
			Unsafe.SkipInit(out InputMap result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__currentInput, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__currentInput, (void*)(&inputMap));
		}
	}

	public unsafe static InputActionRebindingExtensions.RebindingOperation _rebindingOperation
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__rebindingOperation, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<InputActionRebindingExtensions.RebindingOperation>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__rebindingOperation, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)rebindingOperation));
		}
	}

	public unsafe static Vector2 _pointerPosition
	{
		get
		{
			Unsafe.SkipInit(out Vector2 result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__pointerPosition, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__pointerPosition, (void*)(&vector));
		}
	}

	public unsafe static Il2CppReferenceArray<InputBinding> _rightStickPathMasks
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__rightStickPathMasks, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppReferenceArray<InputBinding>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__rightStickPathMasks, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)val));
		}
	}

	public unsafe static Il2CppReferenceArray<InputBinding> _leftStickPathMasks
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__leftStickPathMasks, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppReferenceArray<InputBinding>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__leftStickPathMasks, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)val));
		}
	}

	public unsafe static InputBinding _mouseDeltaMask
	{
		get
		{
			System.IntPtr intPtr = (nint)stackalloc byte[(int)(uint)IL2CPP.il2cpp_class_value_size(Il2CppClassPointerStore<InputBinding>.NativeClassPtr, ref *(uint*)null)];
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__mouseDeltaMask, (void*)intPtr);
			return new InputBinding(IL2CPP.il2cpp_value_box(Il2CppClassPointerStore<InputBinding>.NativeClassPtr, intPtr));
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__mouseDeltaMask, (void*)IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)inputBinding)));
		}
	}

	public unsafe static GameInputAsset CurrentAsset
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 607783, XrefRangeEnd = 607787, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_CurrentAsset_Public_Static_get_GameInputAsset_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<GameInputAsset>(intPtr) : null;
		}
	}

	public unsafe static PlayerInput CurrentPlayerInput
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 607787, XrefRangeEnd = 607791, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_CurrentPlayerInput_Public_Static_get_PlayerInput_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<PlayerInput>(intPtr) : null;
		}
	}

	public unsafe static EControlScheme CurrentControlScheme
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 607791, XrefRangeEnd = 607795, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_CurrentControlScheme_Public_Static_get_EControlScheme_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(EControlScheme*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 607795, XrefRangeEnd = 607799, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_CurrentControlScheme_Private_Static_set_Void_EControlScheme_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static InputMap CurrentInput
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 607799, XrefRangeEnd = 607803, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_CurrentInput_Public_Static_get_InputMap_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(InputMap*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
	}

	public unsafe static Vector2 AimDelta
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 607803, XrefRangeEnd = 607807, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_AimDelta_Public_Static_get_Vector2_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(Vector2*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 607807, XrefRangeEnd = 607811, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_AimDelta_Private_Static_set_Void_Vector2_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static float ScrollDelta
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 607811, XrefRangeEnd = 607815, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_ScrollDelta_Public_Static_get_Single_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 607815, XrefRangeEnd = 607819, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_ScrollDelta_Private_Static_set_Void_Single_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static Vector2 PointerPosition
	{
		[CallerCount(42)]
		[CachedScanResults(RefRangeStart = 607829, RefRangeEnd = 607871, XrefRangeStart = 607819, XrefRangeEnd = 607829, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_PointerPosition_Public_Static_get_Vector2_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(Vector2*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
	}

	public unsafe static IReadOnlyDictionary<InputMap, ActionMapContainer> RegisteredActionMaps
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 607871, XrefRangeEnd = 607875, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_RegisteredActionMaps_Public_Static_get_IReadOnlyDictionary_2_InputMap_ActionMapContainer_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<IReadOnlyDictionary<InputMap, ActionMapContainer>>(intPtr) : null;
		}
	}

	public unsafe static IReadOnlyDictionary<InputActionMap, Il2CppStringArray> ConcurrentMaps
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 608776, XrefRangeEnd = 608780, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_ConcurrentMaps_Public_Static_get_IReadOnlyDictionary_2_InputActionMap_Il2CppStringArray_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<IReadOnlyDictionary<InputActionMap, Il2CppStringArray>>(intPtr) : null;
		}
	}

	static GameInput()
	{
		Il2CppClassPointerStore<GameInput>.NativeClassPtr = IL2CPP.GetIl2CppClass("SilicaCore.dll", "Silica", "GameInput");
		IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<GameInput>.NativeClassPtr);
		NativeFieldInfoPtr_KBM_SCHEME = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "KBM_SCHEME");
		NativeFieldInfoPtr_GAMEPAD_SCHEME = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "GAMEPAD_SCHEME");
		NativeFieldInfoPtr_GENERATED_MAP_PREFIX = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "GENERATED_MAP_PREFIX");
		NativeFieldInfoPtr_LabelProvider = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "LabelProvider");
		NativeFieldInfoPtr_OnRebind = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "OnRebind");
		NativeFieldInfoPtr_OnControlsUpdated = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "OnControlsUpdated");
		NativeFieldInfoPtr_OnSwitchedInput = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "OnSwitchedInput");
		NativeFieldInfoPtr__CurrentControlScheme_k__BackingField = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "<CurrentControlScheme>k__BackingField");
		NativeFieldInfoPtr__AimDelta_k__BackingField = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "<AimDelta>k__BackingField");
		NativeFieldInfoPtr__ScrollDelta_k__BackingField = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "<ScrollDelta>k__BackingField");
		NativeFieldInfoPtr__actionMaps = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "_actionMaps");
		NativeFieldInfoPtr__concurrentMaps = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "_concurrentMaps");
		NativeFieldInfoPtr__toggleActions = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "_toggleActions");
		NativeFieldInfoPtr__currentAsset = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "_currentAsset");
		NativeFieldInfoPtr__currentPlayerInput = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "_currentPlayerInput");
		NativeFieldInfoPtr__pointerAction = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "_pointerAction");
		NativeFieldInfoPtr__aimAction = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "_aimAction");
		NativeFieldInfoPtr__scrollAction = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "_scrollAction");
		NativeFieldInfoPtr__currentInput = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "_currentInput");
		NativeFieldInfoPtr__rebindingOperation = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "_rebindingOperation");
		NativeFieldInfoPtr__pointerPosition = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "_pointerPosition");
		NativeFieldInfoPtr__rightStickPathMasks = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "_rightStickPathMasks");
		NativeFieldInfoPtr__leftStickPathMasks = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "_leftStickPathMasks");
		NativeFieldInfoPtr__mouseDeltaMask = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInput>.NativeClassPtr, "_mouseDeltaMask");
		NativeMethodInfoPtr_TryGetActionLabel_Public_Static_Boolean_InputAction_byref_String_byref_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682747);
		NativeMethodInfoPtr_get_CurrentAsset_Public_Static_get_GameInputAsset_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682748);
		NativeMethodInfoPtr_get_CurrentPlayerInput_Public_Static_get_PlayerInput_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682749);
		NativeMethodInfoPtr_get_CurrentControlScheme_Public_Static_get_EControlScheme_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682750);
		NativeMethodInfoPtr_set_CurrentControlScheme_Private_Static_set_Void_EControlScheme_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682751);
		NativeMethodInfoPtr_get_CurrentInput_Public_Static_get_InputMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682752);
		NativeMethodInfoPtr_get_AimDelta_Public_Static_get_Vector2_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682753);
		NativeMethodInfoPtr_set_AimDelta_Private_Static_set_Void_Vector2_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682754);
		NativeMethodInfoPtr_get_ScrollDelta_Public_Static_get_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682755);
		NativeMethodInfoPtr_set_ScrollDelta_Private_Static_set_Void_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682756);
		NativeMethodInfoPtr_get_PointerPosition_Public_Static_get_Vector2_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682757);
		NativeMethodInfoPtr_get_RegisteredActionMaps_Public_Static_get_IReadOnlyDictionary_2_InputMap_ActionMapContainer_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682758);
		NativeMethodInfoPtr_ReadValue_Public_Static_TValue_String_InputMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682759);
		NativeMethodInfoPtr_GetButtonUp_Public_Static_Boolean_String_InputMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682760);
		NativeMethodInfoPtr_GetButtonDown_Public_Static_Boolean_String_InputMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682761);
		NativeMethodInfoPtr_FindActionInCurrentlyActiveMaps_Private_Static_ActionContainer_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682762);
		NativeMethodInfoPtr_GetButton_Public_Static_Boolean_String_InputMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682763);
		NativeMethodInfoPtr_UsingGamepad_Public_Static_Boolean_String_InputMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682764);
		NativeMethodInfoPtr_GetToggle_Public_Static_Boolean_String_InputMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682765);
		NativeMethodInfoPtr_InterruptToggle_Public_Static_Void_String_InputMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682766);
		NativeMethodInfoPtr_FindInputMapByName_Public_Static_InputMap_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682767);
		NativeMethodInfoPtr_GetPreferredInput_Public_Static_InputMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682768);
		NativeMethodInfoPtr_UpdateInput_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682769);
		NativeMethodInfoPtr_SetToggleMode_Public_Static_Void_String_Boolean_InputMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682770);
		NativeMethodInfoPtr_GetToggleMode_Public_Static_Boolean_String_InputMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682771);
		NativeMethodInfoPtr_SetPlayerInput_Public_Static_Void_PlayerInput_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682772);
		NativeMethodInfoPtr_SetInputModule_Public_Static_Void_InputSystemUIInputModule_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682773);
		NativeMethodInfoPtr_SetAsset_Public_Static_Void_GameInputAsset_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682774);
		NativeMethodInfoPtr_LoadSettings_Public_Static_Void_InputSettings_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682775);
		NativeMethodInfoPtr_ApplyAimSensitivity_Private_Static_Void_InputSettings_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682776);
		NativeMethodInfoPtr_ApplyGamepadDeadzone_Private_Static_Void_InputSettings_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682777);
		NativeMethodInfoPtr_ProcessInput_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682778);
		NativeMethodInfoPtr_SetPointerAction_Public_Static_Void_InputMap_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682779);
		NativeMethodInfoPtr_SetAimAction_Public_Static_Void_InputMap_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682780);
		NativeMethodInfoPtr_SetScrollAction_Public_Static_Void_InputMap_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682781);
		NativeMethodInfoPtr_RegisterActionMap_Public_Static_Void_InputActionMap_InputMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682782);
		NativeMethodInfoPtr_RegisterConcurrentMap_Public_Static_Void_InputActionMap_Il2CppStringArray_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682783);
		NativeMethodInfoPtr_get_ConcurrentMaps_Public_Static_get_IReadOnlyDictionary_2_InputActionMap_Il2CppStringArray_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682784);
		NativeMethodInfoPtr_AreConcurrent_Public_Static_Boolean_InputActionMap_InputActionMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682785);
		NativeMethodInfoPtr_SwitchInput_Private_Static_Void_InputMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682786);
		NativeMethodInfoPtr_SetActionEnableOverride_Public_Static_Void_InputMap_String_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682787);
		NativeMethodInfoPtr_SetCurrentInputValue_Private_Static_Void_InputMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682788);
		NativeMethodInfoPtr_DebugPrintInputMode_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682789);
		NativeMethodInfoPtr_ProcessInputToggle_Private_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682790);
		NativeMethodInfoPtr_ClearAllInput_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682791);
		NativeMethodInfoPtr_ForceDisableAll_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682792);
		NativeMethodInfoPtr_EnableAllWithOverride_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682793);
		NativeMethodInfoPtr_GetAction_Public_Static_InputAction_InputMap_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682794);
		NativeMethodInfoPtr_GetAction_Public_Static_InputAction_InputActionReference_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682795);
		NativeMethodInfoPtr_GetActionContainer_Private_Static_ActionContainer_InputMap_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682796);
		NativeMethodInfoPtr_TryGetValidBinding_Public_Static_Boolean_InputAction_byref_InputBinding_byref_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682797);
		NativeMethodInfoPtr_GetDisplayString_Public_Static_String_InputAction_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682798);
		NativeMethodInfoPtr_GetDisplayString_Public_Static_String_InputMap_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682799);
		NativeMethodInfoPtr_OnControlsChanged_Private_Static_Void_PlayerInput_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682800);
		NativeMethodInfoPtr_OnDeviceChange_Public_Static_Void_InputDevice_InputDeviceChange_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682801);
		NativeMethodInfoPtr_OnFocusChanged_Public_Static_Void_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInput>.NativeClassPtr, 100682802);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 607782, RefRangeEnd = 607783, XrefRangeStart = 607774, XrefRangeEnd = 607782, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool TryGetActionLabel(InputAction action, out string mapLabel, out string actionLabel)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
		byte* num = (byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)));
		nint num2 = 0;
		*(nint**)num = &num2;
		byte* num3 = (byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)));
		nint num4 = 0;
		*(nint**)num3 = &num4;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_TryGetActionLabel_Public_Static_Boolean_InputAction_byref_String_byref_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		mapLabel = IL2CPP.Il2CppStringToManaged((System.IntPtr)num2);
		actionLabel = IL2CPP.Il2CppStringToManaged((System.IntPtr)num4);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(14)]
	[CachedScanResults(RefRangeStart = 607886, RefRangeEnd = 607900, XrefRangeStart = 607875, XrefRangeEnd = 607886, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static TValue ReadValue<TValue>(string action, InputMap actionMap) where TValue : new()
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(action);
		*(InputMap**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &actionMap;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(MethodInfoStoreGeneric_ReadValue_Public_Static_TValue_String_InputMap_0<TValue>.Pointer, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return IL2CPP.PointerToValueGeneric<TValue>(intPtr, false, true);
	}

	[CallerCount(16)]
	[CachedScanResults(RefRangeStart = 607908, RefRangeEnd = 607924, XrefRangeStart = 607900, XrefRangeEnd = 607908, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool GetButtonUp(string action, InputMap actionMap)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(action);
		*(InputMap**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &actionMap;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetButtonUp_Public_Static_Boolean_String_InputMap_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(104)]
	[CachedScanResults(RefRangeStart = 607932, RefRangeEnd = 608036, XrefRangeStart = 607924, XrefRangeEnd = 607932, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool GetButtonDown(string action, InputMap actionMap)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(action);
		*(InputMap**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &actionMap;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetButtonDown_Public_Static_Boolean_String_InputMap_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(5)]
	[CachedScanResults(RefRangeStart = 608056, RefRangeEnd = 608061, XrefRangeStart = 608036, XrefRangeEnd = 608056, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static ActionMapContainer.ActionContainer FindActionInCurrentlyActiveMaps(string action)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(action);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_FindActionInCurrentlyActiveMaps_Private_Static_ActionContainer_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<ActionMapContainer.ActionContainer>(intPtr) : null;
	}

	[CallerCount(39)]
	[CachedScanResults(RefRangeStart = 608069, RefRangeEnd = 608108, XrefRangeStart = 608061, XrefRangeEnd = 608069, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool GetButton(string action, InputMap actionMap)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(action);
		*(InputMap**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &actionMap;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetButton_Public_Static_Boolean_String_InputMap_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 608122, RefRangeEnd = 608123, XrefRangeStart = 608108, XrefRangeEnd = 608122, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool UsingGamepad(string action, InputMap actionMap)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(action);
		*(InputMap**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &actionMap;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_UsingGamepad_Public_Static_Boolean_String_InputMap_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(5)]
	[CachedScanResults(RefRangeStart = 608133, RefRangeEnd = 608138, XrefRangeStart = 608123, XrefRangeEnd = 608133, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool GetToggle(string action, InputMap actionMap)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(action);
		*(InputMap**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &actionMap;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetToggle_Public_Static_Boolean_String_InputMap_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 608148, RefRangeEnd = 608150, XrefRangeStart = 608138, XrefRangeEnd = 608148, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void InterruptToggle(string action, InputMap actionMap)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(action);
		*(InputMap**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &actionMap;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_InterruptToggle_Public_Static_Void_String_InputMap_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 608167, RefRangeEnd = 608171, XrefRangeStart = 608150, XrefRangeEnd = 608167, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static InputMap FindInputMapByName(string name)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(name);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_FindInputMapByName_Public_Static_InputMap_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(InputMap*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 608218, RefRangeEnd = 608219, XrefRangeStart = 608171, XrefRangeEnd = 608218, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static InputMap GetPreferredInput()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetPreferredInput_Public_Static_InputMap_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(InputMap*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 608262, RefRangeEnd = 608263, XrefRangeStart = 608219, XrefRangeEnd = 608262, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void UpdateInput()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_UpdateInput_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 608302, RefRangeEnd = 608306, XrefRangeStart = 608263, XrefRangeEnd = 608302, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SetToggleMode(string action, bool toggle, InputMap actionMap = InputMap.None)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(action);
		*(bool**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &toggle;
		*(InputMap**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &actionMap;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetToggleMode_Public_Static_Void_String_Boolean_InputMap_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 608306, XrefRangeEnd = 608322, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool GetToggleMode(string action, InputMap actionMap)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(action);
		*(InputMap**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &actionMap;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetToggleMode_Public_Static_Boolean_String_InputMap_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 608385, RefRangeEnd = 608386, XrefRangeStart = 608322, XrefRangeEnd = 608385, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SetPlayerInput(PlayerInput playerInput)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)playerInput);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetPlayerInput_Public_Static_Void_PlayerInput_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 608446, RefRangeEnd = 608448, XrefRangeStart = 608386, XrefRangeEnd = 608446, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SetInputModule(InputSystemUIInputModule inputModule)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)inputModule);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetInputModule_Public_Static_Void_InputSystemUIInputModule_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 608448, XrefRangeEnd = 608464, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SetAsset(GameInputAsset asset)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)asset);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetAsset_Public_Static_Void_GameInputAsset_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 608499, RefRangeEnd = 608501, XrefRangeStart = 608464, XrefRangeEnd = 608499, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void LoadSettings(Il2Cpp.InputSettings settings)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)settings);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_LoadSettings_Public_Static_Void_InputSettings_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 608546, RefRangeEnd = 608548, XrefRangeStart = 608501, XrefRangeEnd = 608546, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void ApplyAimSensitivity(Il2Cpp.InputSettings settings)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)settings);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ApplyAimSensitivity_Private_Static_Void_InputSettings_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 608623, RefRangeEnd = 608624, XrefRangeStart = 608548, XrefRangeEnd = 608623, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void ApplyGamepadDeadzone(Il2Cpp.InputSettings settings)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)settings);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ApplyGamepadDeadzone_Private_Static_Void_InputSettings_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 608709, RefRangeEnd = 608710, XrefRangeStart = 608624, XrefRangeEnd = 608709, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void ProcessInput()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ProcessInput_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 608710, XrefRangeEnd = 608719, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SetPointerAction(InputMap actionMap, string action)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&actionMap);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(action);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetPointerAction_Public_Static_Void_InputMap_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 608719, XrefRangeEnd = 608728, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SetAimAction(InputMap actionMap, string action)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&actionMap);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(action);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetAimAction_Public_Static_Void_InputMap_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 608728, XrefRangeEnd = 608737, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SetScrollAction(InputMap actionMap, string action)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&actionMap);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(action);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetScrollAction_Public_Static_Void_InputMap_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(9)]
	[CachedScanResults(RefRangeStart = 608758, RefRangeEnd = 608767, XrefRangeStart = 608737, XrefRangeEnd = 608758, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void RegisterActionMap(InputActionMap actionMap, InputMap inputMap)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)actionMap);
		*(InputMap**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &inputMap;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_RegisterActionMap_Public_Static_Void_InputActionMap_InputMap_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 608774, RefRangeEnd = 608776, XrefRangeStart = 608767, XrefRangeEnd = 608774, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void RegisterConcurrentMap(InputActionMap map, [Optional] Il2CppStringArray peerMapNames)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected Obj, but got Unknown
		if (peerMapNames == null)
		{
			peerMapNames = new Il2CppStringArray(0L);
		}
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)map);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)peerMapNames);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_RegisterConcurrentMap_Public_Static_Void_InputActionMap_Il2CppStringArray_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 608790, RefRangeEnd = 608791, XrefRangeStart = 608780, XrefRangeEnd = 608790, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool AreConcurrent(InputActionMap a, InputActionMap b)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)a);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)b);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_AreConcurrent_Public_Static_Boolean_InputActionMap_InputActionMap_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 608822, RefRangeEnd = 608823, XrefRangeStart = 608791, XrefRangeEnd = 608822, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SwitchInput(InputMap actionMap)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&actionMap);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SwitchInput_Private_Static_Void_InputMap_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 608840, RefRangeEnd = 608843, XrefRangeStart = 608823, XrefRangeEnd = 608840, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SetActionEnableOverride(InputMap actionMap, string action, bool state)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&actionMap);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(action);
		*(bool**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &state;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetActionEnableOverride_Public_Static_Void_InputMap_String_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 608843, XrefRangeEnd = 608847, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SetCurrentInputValue(InputMap newCurrentInput)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&newCurrentInput);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetCurrentInputValue_Private_Static_Void_InputMap_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 608864, RefRangeEnd = 608865, XrefRangeStart = 608847, XrefRangeEnd = 608864, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void DebugPrintInputMode()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_DebugPrintInputMode_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 608925, RefRangeEnd = 608926, XrefRangeStart = 608865, XrefRangeEnd = 608925, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void ProcessInputToggle()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ProcessInputToggle_Private_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 609008, RefRangeEnd = 609011, XrefRangeStart = 608926, XrefRangeEnd = 609008, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void ClearAllInput()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ClearAllInput_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 609045, RefRangeEnd = 609048, XrefRangeStart = 609011, XrefRangeEnd = 609045, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void ForceDisableAll()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ForceDisableAll_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 609074, RefRangeEnd = 609077, XrefRangeStart = 609048, XrefRangeEnd = 609074, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void EnableAllWithOverride()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_EnableAllWithOverride_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(23)]
	[CachedScanResults(RefRangeStart = 609081, RefRangeEnd = 609104, XrefRangeStart = 609077, XrefRangeEnd = 609081, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static InputAction GetAction(InputMap actionMap, string action)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&actionMap);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(action);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetAction_Public_Static_InputAction_InputMap_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<InputAction>(intPtr) : null;
	}

	[CallerCount(71)]
	[CachedScanResults(RefRangeStart = 609116, RefRangeEnd = 609187, XrefRangeStart = 609104, XrefRangeEnd = 609116, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static InputAction GetAction(InputActionReference inputActionReference)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)inputActionReference);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetAction_Public_Static_InputAction_InputActionReference_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<InputAction>(intPtr) : null;
	}

	[CallerCount(15)]
	[CachedScanResults(RefRangeStart = 609197, RefRangeEnd = 609212, XrefRangeStart = 609187, XrefRangeEnd = 609197, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static ActionMapContainer.ActionContainer GetActionContainer(InputMap actionMap, string action)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&actionMap);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(action);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetActionContainer_Private_Static_ActionContainer_InputMap_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<ActionMapContainer.ActionContainer>(intPtr) : null;
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 609236, RefRangeEnd = 609239, XrefRangeStart = 609212, XrefRangeEnd = 609236, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool TryGetValidBinding(InputAction inputAction, out InputBinding validbinding, out int bindingIndex)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)inputAction);
		byte* num = (byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)));
		nint num2 = 0;
		*(nint**)num = &num2;
		*(void**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref bindingIndex);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_TryGetValidBinding_Public_Static_Boolean_InputAction_byref_InputBinding_byref_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		nint num3 = num2;
		validbinding = ((num3 == 0) ? null : new InputBinding(num3));
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(7)]
	[CachedScanResults(RefRangeStart = 609274, RefRangeEnd = 609281, XrefRangeStart = 609239, XrefRangeEnd = 609274, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static string GetDisplayString(InputAction inputAction, bool plusInComposites = true)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)inputAction);
		*(bool**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &plusInComposites;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetDisplayString_Public_Static_String_InputAction_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return IL2CPP.Il2CppStringToManaged(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 609289, RefRangeEnd = 609291, XrefRangeStart = 609281, XrefRangeEnd = 609289, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static string GetDisplayString(InputMap actionMap, string action)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&actionMap);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(action);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetDisplayString_Public_Static_String_InputMap_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return IL2CPP.Il2CppStringToManaged(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609291, XrefRangeEnd = 609313, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void OnControlsChanged(PlayerInput playerInput)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)playerInput);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_OnControlsChanged_Private_Static_Void_PlayerInput_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609313, XrefRangeEnd = 609323, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void OnDeviceChange(InputDevice device, InputDeviceChange change)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)device);
		*(InputDeviceChange**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &change;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_OnDeviceChange_Public_Static_Void_InputDevice_InputDeviceChange_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609323, XrefRangeEnd = 609327, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void OnFocusChanged(bool state)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&state);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_OnFocusChanged_Public_Static_Void_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	public static void RegisterConcurrentMap(InputActionMap map, params string[] peerMapNames)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected Obj, but got Unknown
		RegisterConcurrentMap(map, new Il2CppStringArray(peerMapNames));
	}

	public GameInput(System.IntPtr pointer)
		: base(pointer)
	{
	}
}
