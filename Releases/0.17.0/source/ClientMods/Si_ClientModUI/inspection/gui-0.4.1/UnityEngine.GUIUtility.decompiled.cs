using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem;
using UnityEngine.Bindings;

namespace UnityEngine;

public class GUIUtility : Il2CppSystem.Object
{
	private delegate bool get_hasModalWindowDelegate();

	private delegate bool get_mouseUsedDelegate();

	private delegate bool get_manualTex2SRGBEnabledDelegate();

	private delegate void set_manualTex2SRGBEnabledDelegate(bool value);

	private delegate int GetPermanentControlIDDelegate();

	private delegate IMECompositionMode get_imeCompositionModeDelegate();

	private delegate bool GetChangedDelegate();

	private delegate void SetChangedDelegate(bool changed);

	private delegate void SetDidGUIWindowsEatLastEventDelegate(bool value);

	private delegate void get_s_EditorScreenPointOffset_InjectedDelegate([Out] System.IntPtr ret);

	private delegate void set_s_EditorScreenPointOffset_InjectedDelegate([In] System.IntPtr value);

	private delegate void get_compositionCursorPos_InjectedDelegate([Out] System.IntPtr ret);

	private delegate void Internal_MultiplyPoint_InjectedDelegate([In] System.IntPtr point, [In] System.IntPtr transform, [Out] System.IntPtr ret);

	private delegate System.IntPtr Internal_GetBuiltinSkin_InjectedDelegate(int skin);

	private delegate void InternalWindowToScreenPoint_InjectedDelegate([In] System.IntPtr windowPoint, [Out] System.IntPtr ret);

	private static readonly System.IntPtr NativeFieldInfoPtr_s_ControlCount;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_SkinMode;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_OriginalID;

	private static readonly System.IntPtr NativeFieldInfoPtr_takeCapture;

	private static readonly System.IntPtr NativeFieldInfoPtr_releaseCapture;

	private static readonly System.IntPtr NativeFieldInfoPtr_processEvent;

	private static readonly System.IntPtr NativeFieldInfoPtr_cleanupRoots;

	private static readonly System.IntPtr NativeFieldInfoPtr_endContainerGUIFromException;

	private static readonly System.IntPtr NativeFieldInfoPtr_guiChanged;

	private static readonly System.IntPtr NativeFieldInfoPtr_beforeEventProcessed;

	private static readonly System.IntPtr NativeFieldInfoPtr_m_Event;

	private static readonly System.IntPtr NativeFieldInfoPtr__guiIsExiting_k__BackingField;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_HasCurrentWindowKeyFocusFunc;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_pixelsPerPoint_Internal_Static_get_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_pixelsPerPoint_Internal_Static_set_Void_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_guiDepth_Internal_Static_get_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_mouseUsed_Internal_Static_set_Void_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_textFieldInput_Internal_Static_get_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_textFieldInput_Internal_Static_set_Void_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_systemCopyBuffer_Public_Static_get_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_systemCopyBuffer_Public_Static_set_Void_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Internal_GetControlID_Private_Static_Int32_Int32_FocusType_Rect_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetControlID_Public_Static_Int32_Int32_FocusType_Rect_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_BeginContainerFromOwner_Internal_Static_Void_ScriptableObject_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_BeginContainer_Internal_Static_Void_ObjectGUIState_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Internal_EndContainer_Internal_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CheckForTabEvent_Internal_Static_Int32_Event_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetKeyboardControlToFirstControlId_Internal_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetKeyboardControlToLastControlId_Internal_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HasFocusableControls_Internal_Static_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_OwnsId_Internal_Static_Boolean_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_AlignRectToDevice_Public_Static_Rect_Rect_byref_Int32_byref_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_compositionString_Internal_Static_get_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_imeCompositionMode_Internal_Static_set_Void_IMECompositionMode_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_compositionCursorPos_Internal_Static_set_Void_Vector2_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Internal_GetHotControl_Private_Static_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Internal_GetKeyboardControl_Private_Static_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Internal_SetHotControl_Private_Static_Void_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Internal_SetKeyboardControl_Private_Static_Void_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Internal_GetDefaultSkin_Private_Static_Object_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Internal_ExitGUI_Private_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_InternalScreenToWindowPoint_Private_Static_Vector2_Vector2_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_MarkGUIChanged_Private_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetControlID_Public_Static_Int32_FocusType_Rect_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetControlID_Public_Static_Int32_Int32_FocusType_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetStateObject_Public_Static_Object_Type_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_guiIsExiting_Internal_Static_set_Void_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_hotControl_Public_Static_get_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_hotControl_Public_Static_set_Void_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_TakeCapture_Internal_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_RemoveCapture_Internal_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_keyboardControl_Public_Static_get_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_keyboardControl_Public_Static_set_Void_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HasKeyFocus_Internal_Static_Boolean_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ExitGUI_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetDefaultSkin_Internal_Static_GUISkin_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ProcessEvent_Internal_Static_Void_Int32_IntPtr_byref_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_EndContainer_Internal_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_BeginGUI_Internal_Static_Void_Int32_Int32_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_DestroyGUI_Internal_Static_Void_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_EndGUI_Internal_Static_Void_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_EndGUIFromException_Internal_Static_Boolean_Exception_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_EndContainerGUIFromException_Internal_Static_Boolean_Exception_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ResetGlobalState_Internal_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsExitGUIException_Internal_Static_Boolean_Exception_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ShouldRethrowException_Internal_Static_Boolean_Exception_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CheckOnGUI_Internal_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_RoundToPixelGrid_Internal_Static_Single_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ScreenToGUIPoint_Public_Static_Vector2_Vector2_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_AlignRectToDevice_Public_Static_Rect_Rect_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HitTest_Internal_Static_Boolean_Rect_Vector2_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HitTest_Internal_Static_Boolean_Rect_Vector2_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HitTest_Internal_Static_Boolean_Rect_Event_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_systemCopyBuffer_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_systemCopyBuffer_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Internal_GetControlID_Injected_Private_Static_Int32_Int32_FocusType_byref_Rect_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_BeginContainerFromOwner_Injected_Private_Static_Void_IntPtr_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_BeginContainer_Injected_Private_Static_Void_IntPtr_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CheckForTabEvent_Injected_Private_Static_Int32_IntPtr_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_AlignRectToDevice_Injected_Private_Static_Void_byref_Rect_byref_Int32_byref_Int32_byref_Rect_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_compositionString_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_compositionCursorPos_Injected_Private_Static_Void_byref_Vector2_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_InternalScreenToWindowPoint_Injected_Private_Static_Void_byref_Vector2_byref_Vector2_0;

	private static readonly get_hasModalWindowDelegate get_hasModalWindowDelegateField;

	private static readonly get_mouseUsedDelegate get_mouseUsedDelegateField;

	private static readonly get_manualTex2SRGBEnabledDelegate get_manualTex2SRGBEnabledDelegateField;

	private static readonly set_manualTex2SRGBEnabledDelegate set_manualTex2SRGBEnabledDelegateField;

	private static readonly GetPermanentControlIDDelegate GetPermanentControlIDDelegateField;

	private static readonly get_imeCompositionModeDelegate get_imeCompositionModeDelegateField;

	private static readonly GetChangedDelegate GetChangedDelegateField;

	private static readonly SetChangedDelegate SetChangedDelegateField;

	private static readonly SetDidGUIWindowsEatLastEventDelegate SetDidGUIWindowsEatLastEventDelegateField;

	private static readonly get_s_EditorScreenPointOffset_InjectedDelegate get_s_EditorScreenPointOffset_InjectedDelegateField;

	private static readonly set_s_EditorScreenPointOffset_InjectedDelegate set_s_EditorScreenPointOffset_InjectedDelegateField;

	private static readonly get_compositionCursorPos_InjectedDelegate get_compositionCursorPos_InjectedDelegateField;

	private static readonly Internal_MultiplyPoint_InjectedDelegate Internal_MultiplyPoint_InjectedDelegateField;

	private static readonly Internal_GetBuiltinSkin_InjectedDelegate Internal_GetBuiltinSkin_InjectedDelegateField;

	private static readonly InternalWindowToScreenPoint_InjectedDelegate InternalWindowToScreenPoint_InjectedDelegateField;

	public unsafe static int s_ControlCount
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_ControlCount, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_ControlCount, (void*)(&num));
		}
	}

	public unsafe static int s_SkinMode
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_SkinMode, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_SkinMode, (void*)(&num));
		}
	}

	public unsafe static int s_OriginalID
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_OriginalID, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_OriginalID, (void*)(&num));
		}
	}

	public unsafe static Il2CppSystem.Action takeCapture
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_takeCapture, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_takeCapture, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe static Il2CppSystem.Action releaseCapture
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_releaseCapture, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_releaseCapture, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe static Il2CppSystem.Func<int, System.IntPtr, bool> processEvent
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_processEvent, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Func<int, System.IntPtr, bool>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_processEvent, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)func));
		}
	}

	public unsafe static Il2CppSystem.Action cleanupRoots
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_cleanupRoots, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_cleanupRoots, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe static Il2CppSystem.Func<Il2CppSystem.Exception, bool> endContainerGUIFromException
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_endContainerGUIFromException, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Func<Il2CppSystem.Exception, bool>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_endContainerGUIFromException, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)func));
		}
	}

	public unsafe static Il2CppSystem.Action guiChanged
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_guiChanged, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_guiChanged, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe static Il2CppSystem.Action<EventType, KeyCode, EventModifiers> beforeEventProcessed
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_beforeEventProcessed, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<EventType, KeyCode, EventModifiers>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_beforeEventProcessed, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe static Event m_Event
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_m_Event, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Event>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_m_Event, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)obj));
		}
	}

	public unsafe static bool _guiIsExiting_k__BackingField
	{
		get
		{
			Unsafe.SkipInit(out bool result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__guiIsExiting_k__BackingField, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__guiIsExiting_k__BackingField, (void*)(&flag));
		}
	}

	public unsafe static Il2CppSystem.Func<bool> s_HasCurrentWindowKeyFocusFunc
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_HasCurrentWindowKeyFocusFunc, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Func<bool>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_HasCurrentWindowKeyFocusFunc, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)func));
		}
	}

	public unsafe static float pixelsPerPoint
	{
		[CallerCount(8)]
		[CachedScanResults(RefRangeStart = 1291154, RefRangeEnd = 1291162, XrefRangeStart = 1291152, XrefRangeEnd = 1291154, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_pixelsPerPoint_Internal_Static_get_Single_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(2)]
		[CachedScanResults(RefRangeStart = 1291164, RefRangeEnd = 1291166, XrefRangeStart = 1291162, XrefRangeEnd = 1291164, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_pixelsPerPoint_Internal_Static_set_Void_Single_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static int guiDepth
	{
		[CallerCount(1)]
		[CachedScanResults(RefRangeStart = 1291168, RefRangeEnd = 1291169, XrefRangeStart = 1291166, XrefRangeEnd = 1291168, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_guiDepth_Internal_Static_get_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
	}

	public unsafe static bool mouseUsed
	{
		get
		{
			return get_mouseUsedDelegateField();
		}
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291169, XrefRangeEnd = 1291171, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_mouseUsed_Internal_Static_set_Void_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static bool textFieldInput
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291171, XrefRangeEnd = 1291173, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_textFieldInput_Internal_Static_get_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291173, XrefRangeEnd = 1291175, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_textFieldInput_Internal_Static_set_Void_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static string systemCopyBuffer
	{
		[CallerCount(5)]
		[CachedScanResults(RefRangeStart = 1291183, RefRangeEnd = 1291188, XrefRangeStart = 1291175, XrefRangeEnd = 1291183, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_systemCopyBuffer_Public_Static_get_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return IL2CPP.Il2CppStringToManaged(intPtr);
		}
		[CallerCount(8)]
		[CachedScanResults(RefRangeStart = 1291205, RefRangeEnd = 1291213, XrefRangeStart = 1291188, XrefRangeEnd = 1291205, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.ManagedStringToIl2Cpp(value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_systemCopyBuffer_Public_Static_set_Void_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static string compositionString
	{
		[CallerCount(8)]
		[CachedScanResults(RefRangeStart = 1291283, RefRangeEnd = 1291291, XrefRangeStart = 1291275, XrefRangeEnd = 1291283, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_compositionString_Internal_Static_get_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return IL2CPP.Il2CppStringToManaged(intPtr);
		}
	}

	public unsafe static IMECompositionMode imeCompositionMode
	{
		get
		{
			return get_imeCompositionModeDelegateField();
		}
		[CallerCount(4)]
		[CachedScanResults(RefRangeStart = 1291293, RefRangeEnd = 1291297, XrefRangeStart = 1291291, XrefRangeEnd = 1291293, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_imeCompositionMode_Internal_Static_set_Void_IMECompositionMode_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static Vector2 compositionCursorPos
	{
		get
		{
			get_compositionCursorPos_Injected(out var ret);
			return ret;
		}
		[CallerCount(2)]
		[CachedScanResults(RefRangeStart = 1291302, RefRangeEnd = 1291304, XrefRangeStart = 1291297, XrefRangeEnd = 1291302, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_compositionCursorPos_Internal_Static_set_Void_Vector2_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static bool guiIsExiting
	{
		get
		{
			return _guiIsExiting_k__BackingField;
		}
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291374, XrefRangeEnd = 1291378, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_guiIsExiting_Internal_Static_set_Void_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static int hotControl
	{
		[CallerCount(9)]
		[CachedScanResults(RefRangeStart = 1291383, RefRangeEnd = 1291392, XrefRangeStart = 1291378, XrefRangeEnd = 1291383, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_hotControl_Public_Static_get_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(10)]
		[CachedScanResults(RefRangeStart = 1291397, RefRangeEnd = 1291407, XrefRangeStart = 1291392, XrefRangeEnd = 1291397, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_hotControl_Public_Static_set_Void_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static int keyboardControl
	{
		[CallerCount(11)]
		[CachedScanResults(RefRangeStart = 1291420, RefRangeEnd = 1291431, XrefRangeStart = 1291415, XrefRangeEnd = 1291420, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_keyboardControl_Public_Static_get_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(3)]
		[CachedScanResults(RefRangeStart = 1291436, RefRangeEnd = 1291439, XrefRangeStart = 1291431, XrefRangeEnd = 1291436, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_keyboardControl_Public_Static_set_Void_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public static bool hasModalWindow => get_hasModalWindowDelegateField();

	public static Vector2 s_EditorScreenPointOffset
	{
		get
		{
			get_s_EditorScreenPointOffset_Injected(out var ret);
			return ret;
		}
		set
		{
			set_s_EditorScreenPointOffset_Injected(ref value);
		}
	}

	public static bool manualTex2SRGBEnabled
	{
		get
		{
			return get_manualTex2SRGBEnabledDelegateField();
		}
		set
		{
			set_manualTex2SRGBEnabledDelegateField(value);
		}
	}

	static GUIUtility()
	{
		Il2CppClassPointerStore<GUIUtility>.NativeClassPtr = IL2CPP.GetIl2CppClass("UnityEngine.IMGUIModule.dll", "UnityEngine", "GUIUtility");
		IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr);
		NativeFieldInfoPtr_s_ControlCount = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, "s_ControlCount");
		NativeFieldInfoPtr_s_SkinMode = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, "s_SkinMode");
		NativeFieldInfoPtr_s_OriginalID = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, "s_OriginalID");
		NativeFieldInfoPtr_takeCapture = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, "takeCapture");
		NativeFieldInfoPtr_releaseCapture = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, "releaseCapture");
		NativeFieldInfoPtr_processEvent = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, "processEvent");
		NativeFieldInfoPtr_cleanupRoots = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, "cleanupRoots");
		NativeFieldInfoPtr_endContainerGUIFromException = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, "endContainerGUIFromException");
		NativeFieldInfoPtr_guiChanged = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, "guiChanged");
		NativeFieldInfoPtr_beforeEventProcessed = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, "beforeEventProcessed");
		NativeFieldInfoPtr_m_Event = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, "m_Event");
		NativeFieldInfoPtr__guiIsExiting_k__BackingField = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, "<guiIsExiting>k__BackingField");
		NativeFieldInfoPtr_s_HasCurrentWindowKeyFocusFunc = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, "s_HasCurrentWindowKeyFocusFunc");
		NativeMethodInfoPtr_get_pixelsPerPoint_Internal_Static_get_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663771);
		NativeMethodInfoPtr_set_pixelsPerPoint_Internal_Static_set_Void_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663772);
		NativeMethodInfoPtr_get_guiDepth_Internal_Static_get_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663773);
		NativeMethodInfoPtr_set_mouseUsed_Internal_Static_set_Void_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663774);
		NativeMethodInfoPtr_get_textFieldInput_Internal_Static_get_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663775);
		NativeMethodInfoPtr_set_textFieldInput_Internal_Static_set_Void_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663776);
		NativeMethodInfoPtr_get_systemCopyBuffer_Public_Static_get_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663777);
		NativeMethodInfoPtr_set_systemCopyBuffer_Public_Static_set_Void_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663778);
		NativeMethodInfoPtr_Internal_GetControlID_Private_Static_Int32_Int32_FocusType_Rect_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663779);
		NativeMethodInfoPtr_GetControlID_Public_Static_Int32_Int32_FocusType_Rect_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663780);
		NativeMethodInfoPtr_BeginContainerFromOwner_Internal_Static_Void_ScriptableObject_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663781);
		NativeMethodInfoPtr_BeginContainer_Internal_Static_Void_ObjectGUIState_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663782);
		NativeMethodInfoPtr_Internal_EndContainer_Internal_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663783);
		NativeMethodInfoPtr_CheckForTabEvent_Internal_Static_Int32_Event_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663784);
		NativeMethodInfoPtr_SetKeyboardControlToFirstControlId_Internal_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663785);
		NativeMethodInfoPtr_SetKeyboardControlToLastControlId_Internal_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663786);
		NativeMethodInfoPtr_HasFocusableControls_Internal_Static_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663787);
		NativeMethodInfoPtr_OwnsId_Internal_Static_Boolean_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663788);
		NativeMethodInfoPtr_AlignRectToDevice_Public_Static_Rect_Rect_byref_Int32_byref_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663789);
		NativeMethodInfoPtr_get_compositionString_Internal_Static_get_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663790);
		NativeMethodInfoPtr_set_imeCompositionMode_Internal_Static_set_Void_IMECompositionMode_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663791);
		NativeMethodInfoPtr_set_compositionCursorPos_Internal_Static_set_Void_Vector2_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663792);
		NativeMethodInfoPtr_Internal_GetHotControl_Private_Static_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663793);
		NativeMethodInfoPtr_Internal_GetKeyboardControl_Private_Static_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663794);
		NativeMethodInfoPtr_Internal_SetHotControl_Private_Static_Void_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663795);
		NativeMethodInfoPtr_Internal_SetKeyboardControl_Private_Static_Void_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663796);
		NativeMethodInfoPtr_Internal_GetDefaultSkin_Private_Static_Object_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663797);
		NativeMethodInfoPtr_Internal_ExitGUI_Private_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663798);
		NativeMethodInfoPtr_InternalScreenToWindowPoint_Private_Static_Vector2_Vector2_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663799);
		NativeMethodInfoPtr_MarkGUIChanged_Private_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663800);
		NativeMethodInfoPtr_GetControlID_Public_Static_Int32_FocusType_Rect_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663801);
		NativeMethodInfoPtr_GetControlID_Public_Static_Int32_Int32_FocusType_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663802);
		NativeMethodInfoPtr_GetStateObject_Public_Static_Object_Type_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663803);
		NativeMethodInfoPtr_set_guiIsExiting_Internal_Static_set_Void_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663804);
		NativeMethodInfoPtr_get_hotControl_Public_Static_get_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663805);
		NativeMethodInfoPtr_set_hotControl_Public_Static_set_Void_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663806);
		NativeMethodInfoPtr_TakeCapture_Internal_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663807);
		NativeMethodInfoPtr_RemoveCapture_Internal_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663808);
		NativeMethodInfoPtr_get_keyboardControl_Public_Static_get_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663809);
		NativeMethodInfoPtr_set_keyboardControl_Public_Static_set_Void_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663810);
		NativeMethodInfoPtr_HasKeyFocus_Internal_Static_Boolean_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663811);
		NativeMethodInfoPtr_ExitGUI_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663812);
		NativeMethodInfoPtr_GetDefaultSkin_Internal_Static_GUISkin_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663813);
		NativeMethodInfoPtr_ProcessEvent_Internal_Static_Void_Int32_IntPtr_byref_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663814);
		NativeMethodInfoPtr_EndContainer_Internal_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663815);
		NativeMethodInfoPtr_BeginGUI_Internal_Static_Void_Int32_Int32_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663816);
		NativeMethodInfoPtr_DestroyGUI_Internal_Static_Void_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663817);
		NativeMethodInfoPtr_EndGUI_Internal_Static_Void_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663818);
		NativeMethodInfoPtr_EndGUIFromException_Internal_Static_Boolean_Exception_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663819);
		NativeMethodInfoPtr_EndContainerGUIFromException_Internal_Static_Boolean_Exception_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663820);
		NativeMethodInfoPtr_ResetGlobalState_Internal_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663821);
		NativeMethodInfoPtr_IsExitGUIException_Internal_Static_Boolean_Exception_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663822);
		NativeMethodInfoPtr_ShouldRethrowException_Internal_Static_Boolean_Exception_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663823);
		NativeMethodInfoPtr_CheckOnGUI_Internal_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663824);
		NativeMethodInfoPtr_RoundToPixelGrid_Internal_Static_Single_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663825);
		NativeMethodInfoPtr_ScreenToGUIPoint_Public_Static_Vector2_Vector2_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663826);
		NativeMethodInfoPtr_AlignRectToDevice_Public_Static_Rect_Rect_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663827);
		NativeMethodInfoPtr_HitTest_Internal_Static_Boolean_Rect_Vector2_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663828);
		NativeMethodInfoPtr_HitTest_Internal_Static_Boolean_Rect_Vector2_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663829);
		NativeMethodInfoPtr_HitTest_Internal_Static_Boolean_Rect_Event_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663830);
		NativeMethodInfoPtr_get_systemCopyBuffer_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663832);
		NativeMethodInfoPtr_set_systemCopyBuffer_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663833);
		NativeMethodInfoPtr_Internal_GetControlID_Injected_Private_Static_Int32_Int32_FocusType_byref_Rect_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663834);
		NativeMethodInfoPtr_BeginContainerFromOwner_Injected_Private_Static_Void_IntPtr_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663835);
		NativeMethodInfoPtr_BeginContainer_Injected_Private_Static_Void_IntPtr_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663836);
		NativeMethodInfoPtr_CheckForTabEvent_Injected_Private_Static_Int32_IntPtr_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663837);
		NativeMethodInfoPtr_AlignRectToDevice_Injected_Private_Static_Void_byref_Rect_byref_Int32_byref_Int32_byref_Rect_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663838);
		NativeMethodInfoPtr_get_compositionString_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663839);
		NativeMethodInfoPtr_set_compositionCursorPos_Injected_Private_Static_Void_byref_Vector2_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663840);
		NativeMethodInfoPtr_InternalScreenToWindowPoint_Injected_Private_Static_Void_byref_Vector2_byref_Vector2_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUIUtility>.NativeClassPtr, 100663841);
		get_hasModalWindowDelegateField = IL2CPP.ResolveICall<get_hasModalWindowDelegate>("UnityEngine.GUIUtility::get_hasModalWindow");
		get_mouseUsedDelegateField = IL2CPP.ResolveICall<get_mouseUsedDelegate>("UnityEngine.GUIUtility::get_mouseUsed");
		get_manualTex2SRGBEnabledDelegateField = IL2CPP.ResolveICall<get_manualTex2SRGBEnabledDelegate>("UnityEngine.GUIUtility::get_manualTex2SRGBEnabled");
		set_manualTex2SRGBEnabledDelegateField = IL2CPP.ResolveICall<set_manualTex2SRGBEnabledDelegate>("UnityEngine.GUIUtility::set_manualTex2SRGBEnabled");
		GetPermanentControlIDDelegateField = IL2CPP.ResolveICall<GetPermanentControlIDDelegate>("UnityEngine.GUIUtility::GetPermanentControlID");
		get_imeCompositionModeDelegateField = IL2CPP.ResolveICall<get_imeCompositionModeDelegate>("UnityEngine.GUIUtility::get_imeCompositionMode");
		GetChangedDelegateField = IL2CPP.ResolveICall<GetChangedDelegate>("UnityEngine.GUIUtility::GetChanged");
		SetChangedDelegateField = IL2CPP.ResolveICall<SetChangedDelegate>("UnityEngine.GUIUtility::SetChanged");
		SetDidGUIWindowsEatLastEventDelegateField = IL2CPP.ResolveICall<SetDidGUIWindowsEatLastEventDelegate>("UnityEngine.GUIUtility::SetDidGUIWindowsEatLastEvent");
		get_s_EditorScreenPointOffset_InjectedDelegateField = IL2CPP.ResolveICall<get_s_EditorScreenPointOffset_InjectedDelegate>("UnityEngine.GUIUtility::get_s_EditorScreenPointOffset_Injected");
		set_s_EditorScreenPointOffset_InjectedDelegateField = IL2CPP.ResolveICall<set_s_EditorScreenPointOffset_InjectedDelegate>("UnityEngine.GUIUtility::set_s_EditorScreenPointOffset_Injected");
		get_compositionCursorPos_InjectedDelegateField = IL2CPP.ResolveICall<get_compositionCursorPos_InjectedDelegate>("UnityEngine.GUIUtility::get_compositionCursorPos_Injected");
		Internal_MultiplyPoint_InjectedDelegateField = IL2CPP.ResolveICall<Internal_MultiplyPoint_InjectedDelegate>("UnityEngine.GUIUtility::Internal_MultiplyPoint_Injected");
		Internal_GetBuiltinSkin_InjectedDelegateField = IL2CPP.ResolveICall<Internal_GetBuiltinSkin_InjectedDelegate>("UnityEngine.GUIUtility::Internal_GetBuiltinSkin_Injected");
		InternalWindowToScreenPoint_InjectedDelegateField = IL2CPP.ResolveICall<InternalWindowToScreenPoint_InjectedDelegate>("UnityEngine.GUIUtility::InternalWindowToScreenPoint_Injected");
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291213, XrefRangeEnd = 1291218, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static int Internal_GetControlID(int hint, FocusType focusType, Rect rect)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&hint);
		*(FocusType**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &focusType;
		*(Rect**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &rect;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Internal_GetControlID_Private_Static_Int32_Int32_FocusType_Rect_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(7)]
	[CachedScanResults(RefRangeStart = 1291227, RefRangeEnd = 1291234, XrefRangeStart = 1291218, XrefRangeEnd = 1291227, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static int GetControlID(int hint, FocusType focusType, Rect rect)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&hint);
		*(FocusType**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &focusType;
		*(Rect**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &rect;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetControlID_Public_Static_Int32_Int32_FocusType_Rect_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1291242, RefRangeEnd = 1291243, XrefRangeStart = 1291234, XrefRangeEnd = 1291242, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void BeginContainerFromOwner(ScriptableObject owner)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)owner);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_BeginContainerFromOwner_Internal_Static_Void_ScriptableObject_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1291249, RefRangeEnd = 1291250, XrefRangeStart = 1291243, XrefRangeEnd = 1291249, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void BeginContainer(ObjectGUIState objectGUIState)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)objectGUIState);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_BeginContainer_Internal_Static_Void_ObjectGUIState_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291250, XrefRangeEnd = 1291252, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Internal_EndContainer()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Internal_EndContainer_Internal_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291252, XrefRangeEnd = 1291258, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static int CheckForTabEvent(Event evt)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)evt);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CheckForTabEvent_Internal_Static_Int32_Event_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 1291260, RefRangeEnd = 1291262, XrefRangeStart = 1291258, XrefRangeEnd = 1291260, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SetKeyboardControlToFirstControlId()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetKeyboardControlToFirstControlId_Internal_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1291264, RefRangeEnd = 1291265, XrefRangeStart = 1291262, XrefRangeEnd = 1291264, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SetKeyboardControlToLastControlId()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetKeyboardControlToLastControlId_Internal_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291265, XrefRangeEnd = 1291267, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool HasFocusableControls()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HasFocusableControls_Internal_Static_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1291269, RefRangeEnd = 1291270, XrefRangeStart = 1291267, XrefRangeEnd = 1291269, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool OwnsId(int id)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&id);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_OwnsId_Internal_Static_Boolean_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291270, XrefRangeEnd = 1291275, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Rect AlignRectToDevice(Rect rect, out int widthInPixels, out int heightInPixels)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&rect);
		*(void**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref widthInPixels);
		*(void**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref heightInPixels);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_AlignRectToDevice_Public_Static_Rect_Rect_byref_Int32_byref_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Rect*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291304, XrefRangeEnd = 1291306, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static int Internal_GetHotControl()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Internal_GetHotControl_Private_Static_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291306, XrefRangeEnd = 1291308, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static int Internal_GetKeyboardControl()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Internal_GetKeyboardControl_Private_Static_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291308, XrefRangeEnd = 1291310, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Internal_SetHotControl(int value)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&value);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Internal_SetHotControl_Private_Static_Void_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291310, XrefRangeEnd = 1291312, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Internal_SetKeyboardControl(int value)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&value);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Internal_SetKeyboardControl_Private_Static_Void_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291312, XrefRangeEnd = 1291314, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Il2CppSystem.Object Internal_GetDefaultSkin(int skinMode)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&skinMode);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Internal_GetDefaultSkin_Private_Static_Object_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Object>(intPtr) : null;
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291314, XrefRangeEnd = 1291316, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Internal_ExitGUI()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Internal_ExitGUI_Private_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291316, XrefRangeEnd = 1291321, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Vector2 InternalScreenToWindowPoint(Vector2 screenPoint)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&screenPoint);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_InternalScreenToWindowPoint_Private_Static_Vector2_Vector2_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector2*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291321, XrefRangeEnd = 1291325, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void MarkGUIChanged()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_MarkGUIChanged_Private_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291325, XrefRangeEnd = 1291329, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static int GetControlID(FocusType focus, Rect position)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&focus);
		*(Rect**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &position;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetControlID_Public_Static_Int32_FocusType_Rect_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(15)]
	[CachedScanResults(RefRangeStart = 1291334, RefRangeEnd = 1291349, XrefRangeStart = 1291329, XrefRangeEnd = 1291334, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static int GetControlID(int hint, FocusType focus)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&hint);
		*(FocusType**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &focus;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetControlID_Public_Static_Int32_Int32_FocusType_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 1291370, RefRangeEnd = 1291374, XrefRangeStart = 1291349, XrefRangeEnd = 1291370, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Il2CppSystem.Object GetStateObject(Il2CppSystem.Type t, int controlID)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)t);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &controlID;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetStateObject_Public_Static_Object_Type_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Object>(intPtr) : null;
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291407, XrefRangeEnd = 1291411, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void TakeCapture()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_TakeCapture_Internal_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291411, XrefRangeEnd = 1291415, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void RemoveCapture()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_RemoveCapture_Internal_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291439, XrefRangeEnd = 1291447, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool HasKeyFocus(int controlID)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&controlID);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HasKeyFocus_Internal_Static_Boolean_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1291452, RefRangeEnd = 1291453, XrefRangeStart = 1291447, XrefRangeEnd = 1291452, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void ExitGUI()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ExitGUI_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291453, XrefRangeEnd = 1291460, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static GUISkin GetDefaultSkin()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetDefaultSkin_Internal_Static_GUISkin_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<GUISkin>(intPtr) : null;
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291460, XrefRangeEnd = 1291488, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void ProcessEvent(int instanceID, System.IntPtr nativeEventPtr, out bool result)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&instanceID);
		*(System.IntPtr**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &nativeEventPtr;
		*(void**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref result);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ProcessEvent_Internal_Static_Void_Int32_IntPtr_byref_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 1291495, RefRangeEnd = 1291497, XrefRangeStart = 1291488, XrefRangeEnd = 1291495, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void EndContainer()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_EndContainer_Internal_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291497, XrefRangeEnd = 1291530, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void BeginGUI(int skinMode, int instanceID, int useGUILayout)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&skinMode);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &instanceID;
		*(int**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &useGUILayout;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_BeginGUI_Internal_Static_Void_Int32_Int32_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291530, XrefRangeEnd = 1291543, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void DestroyGUI(int instanceID)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&instanceID);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_DestroyGUI_Internal_Static_Void_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291543, XrefRangeEnd = 1291598, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void EndGUI(int layoutType)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&layoutType);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_EndGUI_Internal_Static_Void_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291598, XrefRangeEnd = 1291611, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool EndGUIFromException(Il2CppSystem.Exception exception)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)exception);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_EndGUIFromException_Internal_Static_Boolean_Exception_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291611, XrefRangeEnd = 1291615, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool EndContainerGUIFromException(Il2CppSystem.Exception exception)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)exception);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_EndContainerGUIFromException_Internal_Static_Boolean_Exception_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1291640, RefRangeEnd = 1291641, XrefRangeStart = 1291615, XrefRangeEnd = 1291640, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void ResetGlobalState()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ResetGlobalState_Internal_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 1291646, RefRangeEnd = 1291648, XrefRangeStart = 1291641, XrefRangeEnd = 1291646, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool IsExitGUIException(Il2CppSystem.Exception exception)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)exception);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsExitGUIException_Internal_Static_Boolean_Exception_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1291656, RefRangeEnd = 1291657, XrefRangeStart = 1291648, XrefRangeEnd = 1291656, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool ShouldRethrowException(Il2CppSystem.Exception exception)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)exception);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ShouldRethrowException_Internal_Static_Boolean_Exception_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(26)]
	[CachedScanResults(RefRangeStart = 1291662, RefRangeEnd = 1291688, XrefRangeStart = 1291657, XrefRangeEnd = 1291662, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void CheckOnGUI()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CheckOnGUI_Internal_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 1291696, RefRangeEnd = 1291698, XrefRangeStart = 1291688, XrefRangeEnd = 1291696, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static float RoundToPixelGrid(float v)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&v);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_RoundToPixelGrid_Internal_Static_Single_Single_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1291708, RefRangeEnd = 1291709, XrefRangeStart = 1291698, XrefRangeEnd = 1291708, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Vector2 ScreenToGUIPoint(Vector2 screenPoint)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&screenPoint);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ScreenToGUIPoint_Public_Static_Vector2_Vector2_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector2*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1291717, RefRangeEnd = 1291718, XrefRangeStart = 1291709, XrefRangeEnd = 1291717, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Rect AlignRectToDevice(Rect rect)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&rect);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_AlignRectToDevice_Public_Static_Rect_Rect_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Rect*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	public unsafe static bool HitTest(Rect rect, Vector2 point, int offset)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&rect);
		*(Vector2**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &point;
		*(int**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &offset;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HitTest_Internal_Static_Boolean_Rect_Vector2_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1291721, RefRangeEnd = 1291722, XrefRangeStart = 1291718, XrefRangeEnd = 1291721, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool HitTest(Rect rect, Vector2 point, bool isDirectManipulationDevice)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&rect);
		*(Vector2**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &point;
		*(bool**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &isDirectManipulationDevice;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HitTest_Internal_Static_Boolean_Rect_Vector2_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(6)]
	[CachedScanResults(RefRangeStart = 1291737, RefRangeEnd = 1291743, XrefRangeStart = 1291722, XrefRangeEnd = 1291737, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool HitTest(Rect rect, Event evt)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&rect);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)evt);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HitTest_Internal_Static_Boolean_Rect_Event_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291743, XrefRangeEnd = 1291745, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void get_systemCopyBuffer_Injected(out ManagedSpanWrapper ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_systemCopyBuffer_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291745, XrefRangeEnd = 1291747, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void set_systemCopyBuffer_Injected(ref ManagedSpanWrapper value)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)Unsafe.AsPointer(ref value);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_systemCopyBuffer_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291747, XrefRangeEnd = 1291749, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static int Internal_GetControlID_Injected(int hint, FocusType focusType, [In] ref Rect rect)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&hint);
		*(FocusType**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &focusType;
		*(void**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref rect);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Internal_GetControlID_Injected_Private_Static_Int32_Int32_FocusType_byref_Rect_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291749, XrefRangeEnd = 1291751, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void BeginContainerFromOwner_Injected(System.IntPtr owner)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&owner);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_BeginContainerFromOwner_Injected_Private_Static_Void_IntPtr_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291751, XrefRangeEnd = 1291753, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void BeginContainer_Injected(System.IntPtr objectGUIState)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&objectGUIState);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_BeginContainer_Injected_Private_Static_Void_IntPtr_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291753, XrefRangeEnd = 1291755, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static int CheckForTabEvent_Injected(System.IntPtr evt)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&evt);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CheckForTabEvent_Injected_Private_Static_Int32_IntPtr_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291755, XrefRangeEnd = 1291757, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void AlignRectToDevice_Injected([In] ref Rect rect, out int widthInPixels, out int heightInPixels, out Rect ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = (nint)Unsafe.AsPointer(ref rect);
		*(void**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref widthInPixels);
		*(void**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref heightInPixels);
		*(void**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_AlignRectToDevice_Injected_Private_Static_Void_byref_Rect_byref_Int32_byref_Int32_byref_Rect_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291757, XrefRangeEnd = 1291759, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void get_compositionString_Injected(out ManagedSpanWrapper ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_compositionString_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291759, XrefRangeEnd = 1291761, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void set_compositionCursorPos_Injected([In] ref Vector2 value)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)Unsafe.AsPointer(ref value);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_compositionCursorPos_Injected_Private_Static_Void_byref_Vector2_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1291761, XrefRangeEnd = 1291763, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void InternalScreenToWindowPoint_Injected([In] ref Vector2 screenPoint, out Vector2 ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)Unsafe.AsPointer(ref screenPoint);
		*(void**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_InternalScreenToWindowPoint_Injected_Private_Static_Void_byref_Vector2_byref_Vector2_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	public GUIUtility(System.IntPtr pointer)
		: base(pointer)
	{
	}

	public static int GetPermanentControlID()
	{
		return GetPermanentControlIDDelegateField();
	}

	public static Vector3 Internal_MultiplyPoint(Vector3 point, Matrix4x4 transform)
	{
		Internal_MultiplyPoint_Injected(ref point, ref transform, out var ret);
		return ret;
	}

	public static bool GetChanged()
	{
		return GetChangedDelegateField();
	}

	public static void SetChanged(bool changed)
	{
		SetChangedDelegateField(changed);
	}

	public static void SetDidGUIWindowsEatLastEvent(bool value)
	{
		SetDidGUIWindowsEatLastEventDelegateField(value);
	}

	public static Object Internal_GetBuiltinSkin(int skin)
	{
		return Unmarshal.UnmarshalUnityObject<Object>(Internal_GetBuiltinSkin_Injected(skin));
	}

	public static Vector2 InternalWindowToScreenPoint(Vector2 windowPoint)
	{
		InternalWindowToScreenPoint_Injected(ref windowPoint, out var ret);
		return ret;
	}

	public static int GetControlID(FocusType focus)
	{
		return GetControlID(0, focus);
	}

	public static int GetControlID(GUIContent contents, FocusType focus)
	{
		return GetControlID(contents.hash, focus);
	}

	public static int GetControlID(GUIContent contents, FocusType focus, Rect position)
	{
		return GetControlID(contents.hash, focus, position);
	}

	public static Il2CppSystem.Object QueryStateObject(Il2CppSystem.Type t, int controlID)
	{
		return GUIStateObjects.QueryStateObject(t, controlID);
	}

	public static GUISkin GetDefaultSkin(int skinMode)
	{
		return ((Il2CppObjectBase)Internal_GetDefaultSkin(skinMode)).TryCast<GUISkin>();
	}

	public static GUISkin GetBuiltinSkin(int skin)
	{
		return ((Il2CppObjectBase)Internal_GetBuiltinSkin(skin)).TryCast<GUISkin>();
	}

	public static void CleanupRoots()
	{
		cleanupRoots?.Invoke();
	}

	public static float RoundToPixelGrid(float v, float scale)
	{
		return Mathf.Floor(v * scale + 0.48f) / scale;
	}

	public static Vector2 GUIToScreenPoint(Vector2 guiPoint)
	{
		return InternalWindowToScreenPoint(GUIClip.UnclipToWindow(guiPoint));
	}

	public static Rect GUIToScreenRect(Rect guiRect)
	{
		Vector2 vector = GUIToScreenPoint(new Vector2(guiRect.x, guiRect.y));
		guiRect.x = vector.x;
		guiRect.y = vector.y;
		return guiRect;
	}

	public static Rect ScreenToGUIRect(Rect screenRect)
	{
		Vector2 vector = ScreenToGUIPoint(new Vector2(screenRect.x, screenRect.y));
		screenRect.x = vector.x;
		screenRect.y = vector.y;
		return screenRect;
	}

	public static void RotateAroundPivot(float angle, Vector2 pivotPoint)
	{
		Matrix4x4 matrix = GUI.matrix;
		GUI.matrix = Matrix4x4.identity;
		Vector2 vector = GUIClip.Unclip(pivotPoint);
		Matrix4x4 matrix4x = Matrix4x4.TRS(vector, Quaternion.Euler(0f, 0f, angle), Vector3.one) * Matrix4x4.TRS(-vector, Quaternion.identity, Vector3.one);
		GUI.matrix = matrix4x * matrix;
	}

	public static void ScaleAroundPivot(Vector2 scale, Vector2 pivotPoint)
	{
		Matrix4x4 matrix = GUI.matrix;
		Vector2 vector = GUIClip.Unclip(pivotPoint);
		Matrix4x4 matrix4x = Matrix4x4.TRS(vector, Quaternion.identity, new Vector3(scale.x, scale.y, 1f)) * Matrix4x4.TRS(-vector, Quaternion.identity, Vector3.one);
		GUI.matrix = matrix4x * matrix;
	}

	public unsafe static void get_s_EditorScreenPointOffset_Injected(out Vector2 ret)
	{
		get_s_EditorScreenPointOffset_InjectedDelegateField((nint)Unsafe.AsPointer(ref ret));
	}

	public unsafe static void set_s_EditorScreenPointOffset_Injected([In] ref Vector2 value)
	{
		set_s_EditorScreenPointOffset_InjectedDelegateField((nint)Unsafe.AsPointer(ref value));
	}

	public unsafe static void get_compositionCursorPos_Injected(out Vector2 ret)
	{
		get_compositionCursorPos_InjectedDelegateField((nint)Unsafe.AsPointer(ref ret));
	}

	public unsafe static void Internal_MultiplyPoint_Injected([In] ref Vector3 point, [In] ref Matrix4x4 transform, out Vector3 ret)
	{
		Internal_MultiplyPoint_InjectedDelegateField((nint)Unsafe.AsPointer(ref point), (nint)Unsafe.AsPointer(ref transform), (nint)Unsafe.AsPointer(ref ret));
	}

	public static System.IntPtr Internal_GetBuiltinSkin_Injected(int skin)
	{
		return Internal_GetBuiltinSkin_InjectedDelegateField(skin);
	}

	public unsafe static void InternalWindowToScreenPoint_Injected([In] ref Vector2 windowPoint, out Vector2 ret)
	{
		InternalWindowToScreenPoint_InjectedDelegateField((nint)Unsafe.AsPointer(ref windowPoint), (nint)Unsafe.AsPointer(ref ret));
	}
}
