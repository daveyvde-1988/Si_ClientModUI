using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem;
using UnityEngine.Bindings;
using UnityEngineInternal;

namespace UnityEngine;

public class GUI : Il2CppSystem.Object
{
	public sealed class WindowFunction : Il2CppSystem.MulticastDelegate
	{
		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_Object_IntPtr_0;

		private static readonly System.IntPtr NativeMethodInfoPtr_Invoke_Public_Virtual_New_Void_Int32_0;

		static WindowFunction()
		{
			Il2CppClassPointerStore<WindowFunction>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<GUI>.NativeClassPtr, "WindowFunction");
			NativeMethodInfoPtr__ctor_Public_Void_Object_IntPtr_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<WindowFunction>.NativeClassPtr, 100663463);
			NativeMethodInfoPtr_Invoke_Public_Virtual_New_Void_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<WindowFunction>.NativeClassPtr, 100663464);
		}

		[CallerCount(167)]
		[CachedScanResults(RefRangeStart = 627757, RefRangeEnd = 627924, XrefRangeStart = 627757, XrefRangeEnd = 627924, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe WindowFunction(Il2CppSystem.Object @object, System.IntPtr method)
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<WindowFunction>.NativeClassPtr))
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[2];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)@object);
			*(System.IntPtr**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &method;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_Object_IntPtr_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		public unsafe virtual void Invoke(int id)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&id);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Invoke_Public_Virtual_New_Void_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		public WindowFunction(System.IntPtr pointer)
			: base(pointer)
		{
		}

		public static implicit operator WindowFunction(System.Action<int> P_0)
		{
			return DelegateSupport.ConvertDelegate<WindowFunction>((System.Delegate)P_0);
		}

		public static WindowFunction operator +(WindowFunction P_0, WindowFunction P_1)
		{
			return ((Il2CppObjectBase)Il2CppSystem.Delegate.Combine(P_0, P_1)).Cast<WindowFunction>();
		}

		public static WindowFunction operator -(WindowFunction P_0, WindowFunction P_1)
		{
			object obj = Il2CppSystem.Delegate.Remove(P_0, P_1);
			if (obj != null)
			{
				obj = ((Il2CppObjectBase)obj).Cast<WindowFunction>();
			}
			return (WindowFunction)obj;
		}
	}

	public enum ToolbarButtonSize
	{
		Fixed,
		FitToContents
	}

	public abstract class Scope
	{
	}

	public class GroupScope : Scope
	{
	}

	public class ScrollViewScope : Scope
	{
	}

	public class ClipScope : Scope
	{
	}

	private delegate int get_depthDelegate();

	private delegate void set_depthDelegate(int value);

	private delegate bool get_isInsideListDelegate();

	private delegate void set_isInsideListDelegate(bool value);

	private delegate void BringWindowToFrontDelegate(int windowID);

	private delegate void BringWindowToBackDelegate(int windowID);

	private delegate void FocusWindowDelegate(int windowID);

	private delegate void UnfocusWindowDelegate();

	private delegate void Internal_BeginWindowsDelegate();

	private delegate void Internal_EndWindowsDelegate();

	private delegate System.IntPtr get_blendMaterial_InjectedDelegate();

	private delegate System.IntPtr get_blitMaterial_InjectedDelegate();

	private delegate System.IntPtr get_roundedRectMaterial_InjectedDelegate();

	private delegate System.IntPtr get_roundedRectWithColorPerBorderMaterial_InjectedDelegate();

	private delegate void GetNameOfFocusedControl_InjectedDelegate([Out] System.IntPtr ret);

	private delegate void Internal_GetTooltip_InjectedDelegate([Out] System.IntPtr ret);

	private delegate void Internal_SetTooltip_InjectedDelegate(System.IntPtr value);

	private delegate void Internal_GetMouseTooltip_InjectedDelegate([Out] System.IntPtr ret);

	private delegate void Internal_DoModalWindow_InjectedDelegate(int id, int instanceID, [In] System.IntPtr clientRect, System.IntPtr func, System.IntPtr content, System.IntPtr style, System.IntPtr skin, [Out] System.IntPtr ret);

	private delegate void Internal_DoWindow_InjectedDelegate(int id, int instanceID, [In] System.IntPtr clientRect, System.IntPtr func, System.IntPtr title, System.IntPtr style, System.IntPtr skin, bool forceRectOnLayout, [Out] System.IntPtr ret);

	private delegate void DragWindow_InjectedDelegate([In] System.IntPtr position);

	private delegate void Internal_Concatenate_InjectedDelegate(System.IntPtr first, System.IntPtr second, [Out] System.IntPtr ret);

	private static readonly System.IntPtr NativeFieldInfoPtr_s_ScrollControlId;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_HotTextField;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_BoxHash;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_ButonHash;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_RepeatButtonHash;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_ToggleHash;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_ButtonGridHash;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_SliderHash;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_BeginGroupHash;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_ScrollviewHash;

	private static readonly System.IntPtr NativeFieldInfoPtr__scrollTroughSide_k__BackingField;

	private static readonly System.IntPtr NativeFieldInfoPtr__nextScrollStepTime_k__BackingField;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_Skin;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_ToolTipRect;

	private static readonly System.IntPtr NativeFieldInfoPtr__scrollViewStates_k__BackingField;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_color_Public_Static_get_Color_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_color_Public_Static_set_Void_Color_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_backgroundColor_Public_Static_get_Color_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_backgroundColor_Public_Static_set_Void_Color_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_contentColor_Public_Static_get_Color_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_contentColor_Public_Static_set_Void_Color_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_changed_Public_Static_get_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_changed_Public_Static_set_Void_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_enabled_Public_Static_get_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_enabled_Public_Static_set_Void_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_usePageScrollbars_Internal_Static_get_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GrabMouseControl_Internal_Static_Void_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HasMouseControl_Internal_Static_Boolean_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReleaseMouseControl_Internal_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetNextControlName_Public_Static_Void_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_FocusControl_Public_Static_Void_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_InternalRepaintEditorWindow_Internal_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_scrollTroughSide_Internal_Static_get_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_scrollTroughSide_Internal_Static_set_Void_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_nextScrollStepTime_Internal_Static_get_DateTime_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_nextScrollStepTime_Internal_Static_set_Void_DateTime_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_skin_Public_Static_set_Void_GUISkin_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_skin_Public_Static_get_GUISkin_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_DoSetSkin_Internal_Static_Void_GUISkin_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_matrix_Public_Static_get_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_matrix_Public_Static_set_Void_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Label_Public_Static_Void_Rect_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Label_Public_Static_Void_Rect_String_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Label_Public_Static_Void_Rect_GUIContent_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Box_Public_Static_Void_Rect_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Box_Public_Static_Void_Rect_GUIContent_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Box_Public_Static_Void_Rect_String_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Box_Public_Static_Void_Rect_GUIContent_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Button_Public_Static_Boolean_Rect_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Button_Public_Static_Boolean_Rect_GUIContent_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Button_Internal_Static_Boolean_Rect_Int32_GUIContent_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_DoRepeatButton_Private_Static_Boolean_Rect_GUIContent_GUIStyle_FocusType_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_TextField_Public_Static_String_Rect_String_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_PasswordFieldGetStrToShow_Internal_Static_String_String_Char_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_DoTextField_Internal_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_DoTextField_Internal_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_DoTextField_Internal_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_String_Char_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleTextFieldEventForTouchscreen_Private_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_String_Char_TextEditor_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleTextFieldEventForDesktop_Private_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_TextEditor_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_DoControl_Internal_Static_Boolean_Rect_Int32_Boolean_Boolean_GUIContent_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_DoLabel_Private_Static_Void_Rect_GUIContent_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_DoButton_Internal_Static_Boolean_Rect_Int32_GUIContent_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Slider_Public_Static_Single_Rect_Single_Single_Single_Single_GUIStyle_GUIStyle_Boolean_Int32_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HorizontalScrollbar_Public_Static_Single_Rect_Single_Single_Single_Single_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ScrollerRepeatButton_Internal_Static_Boolean_Int32_Rect_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_VerticalScrollbar_Public_Static_Single_Rect_Single_Single_Single_Single_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Scroller_Internal_Static_Single_Rect_Single_Single_Single_Single_GUIStyle_GUIStyle_GUIStyle_GUIStyle_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_BeginGroup_Public_Static_Void_Rect_GUIContent_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_BeginGroup_Internal_Static_Void_Rect_GUIContent_GUIStyle_Vector2_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_EndGroup_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_scrollViewStates_Internal_Static_get_GenericStack_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_BeginScrollView_Public_Static_Vector2_Rect_Vector2_Rect_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_BeginScrollView_Internal_Static_Vector2_Rect_Vector2_Rect_Boolean_Boolean_GUIStyle_GUIStyle_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_EndScrollView_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_EndScrollView_Public_Static_Void_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CallWindowDelegate_Internal_Static_Void_WindowFunction_Int32_Int32_GUISkin_Int32_Single_Single_GUIStyle_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_color_Injected_Private_Static_Void_byref_Color_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_color_Injected_Private_Static_Void_byref_Color_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_backgroundColor_Injected_Private_Static_Void_byref_Color_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_backgroundColor_Injected_Private_Static_Void_byref_Color_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_contentColor_Injected_Private_Static_Void_byref_Color_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_contentColor_Injected_Private_Static_Void_byref_Color_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetNextControlName_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_FocusControl_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0;

	public const float s_ScrollStepSize = 10f;

	private static readonly get_depthDelegate get_depthDelegateField;

	private static readonly set_depthDelegate set_depthDelegateField;

	private static readonly get_isInsideListDelegate get_isInsideListDelegateField;

	private static readonly set_isInsideListDelegate set_isInsideListDelegateField;

	private static readonly BringWindowToFrontDelegate BringWindowToFrontDelegateField;

	private static readonly BringWindowToBackDelegate BringWindowToBackDelegateField;

	private static readonly FocusWindowDelegate FocusWindowDelegateField;

	private static readonly UnfocusWindowDelegate UnfocusWindowDelegateField;

	private static readonly Internal_BeginWindowsDelegate Internal_BeginWindowsDelegateField;

	private static readonly Internal_EndWindowsDelegate Internal_EndWindowsDelegateField;

	private static readonly get_blendMaterial_InjectedDelegate get_blendMaterial_InjectedDelegateField;

	private static readonly get_blitMaterial_InjectedDelegate get_blitMaterial_InjectedDelegateField;

	private static readonly get_roundedRectMaterial_InjectedDelegate get_roundedRectMaterial_InjectedDelegateField;

	private static readonly get_roundedRectWithColorPerBorderMaterial_InjectedDelegate get_roundedRectWithColorPerBorderMaterial_InjectedDelegateField;

	private static readonly GetNameOfFocusedControl_InjectedDelegate GetNameOfFocusedControl_InjectedDelegateField;

	private static readonly Internal_GetTooltip_InjectedDelegate Internal_GetTooltip_InjectedDelegateField;

	private static readonly Internal_SetTooltip_InjectedDelegate Internal_SetTooltip_InjectedDelegateField;

	private static readonly Internal_GetMouseTooltip_InjectedDelegate Internal_GetMouseTooltip_InjectedDelegateField;

	private static readonly Internal_DoModalWindow_InjectedDelegate Internal_DoModalWindow_InjectedDelegateField;

	private static readonly Internal_DoWindow_InjectedDelegate Internal_DoWindow_InjectedDelegateField;

	private static readonly DragWindow_InjectedDelegate DragWindow_InjectedDelegateField;

	private static readonly Internal_Concatenate_InjectedDelegate Internal_Concatenate_InjectedDelegateField;

	public unsafe static int s_ScrollControlId
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_ScrollControlId, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_ScrollControlId, (void*)(&num));
		}
	}

	public unsafe static int s_HotTextField
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_HotTextField, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_HotTextField, (void*)(&num));
		}
	}

	public unsafe static int s_BoxHash
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_BoxHash, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_BoxHash, (void*)(&num));
		}
	}

	public unsafe static int s_ButonHash
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_ButonHash, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_ButonHash, (void*)(&num));
		}
	}

	public unsafe static int s_RepeatButtonHash
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_RepeatButtonHash, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_RepeatButtonHash, (void*)(&num));
		}
	}

	public unsafe static int s_ToggleHash
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_ToggleHash, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_ToggleHash, (void*)(&num));
		}
	}

	public unsafe static int s_ButtonGridHash
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_ButtonGridHash, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_ButtonGridHash, (void*)(&num));
		}
	}

	public unsafe static int s_SliderHash
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_SliderHash, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_SliderHash, (void*)(&num));
		}
	}

	public unsafe static int s_BeginGroupHash
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_BeginGroupHash, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_BeginGroupHash, (void*)(&num));
		}
	}

	public unsafe static int s_ScrollviewHash
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_ScrollviewHash, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_ScrollviewHash, (void*)(&num));
		}
	}

	public unsafe static int _scrollTroughSide_k__BackingField
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__scrollTroughSide_k__BackingField, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__scrollTroughSide_k__BackingField, (void*)(&num));
		}
	}

	public unsafe static Il2CppSystem.DateTime _nextScrollStepTime_k__BackingField
	{
		get
		{
			Unsafe.SkipInit(out Il2CppSystem.DateTime result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__nextScrollStepTime_k__BackingField, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__nextScrollStepTime_k__BackingField, (void*)(&dateTime));
		}
	}

	public unsafe static GUISkin s_Skin
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_Skin, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<GUISkin>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_Skin, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gUISkin));
		}
	}

	public unsafe static Rect s_ToolTipRect
	{
		get
		{
			Unsafe.SkipInit(out Rect result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_ToolTipRect, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_ToolTipRect, (void*)(&rect));
		}
	}

	public unsafe static GenericStack _scrollViewStates_k__BackingField
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__scrollViewStates_k__BackingField, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<GenericStack>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__scrollViewStates_k__BackingField, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)genericStack));
		}
	}

	public unsafe static Color color
	{
		[CallerCount(2)]
		[CachedScanResults(RefRangeStart = 1286373, RefRangeEnd = 1286375, XrefRangeStart = 1286368, XrefRangeEnd = 1286373, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_color_Public_Static_get_Color_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(Color*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(4)]
		[CachedScanResults(RefRangeStart = 1286380, RefRangeEnd = 1286384, XrefRangeStart = 1286375, XrefRangeEnd = 1286380, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_color_Public_Static_set_Void_Color_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static Color backgroundColor
	{
		[CallerCount(2)]
		[CachedScanResults(RefRangeStart = 1286389, RefRangeEnd = 1286391, XrefRangeStart = 1286384, XrefRangeEnd = 1286389, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_backgroundColor_Public_Static_get_Color_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(Color*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(1)]
		[CachedScanResults(RefRangeStart = 1286396, RefRangeEnd = 1286397, XrefRangeStart = 1286391, XrefRangeEnd = 1286396, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_backgroundColor_Public_Static_set_Void_Color_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static Color contentColor
	{
		[CallerCount(2)]
		[CachedScanResults(RefRangeStart = 1286402, RefRangeEnd = 1286404, XrefRangeStart = 1286397, XrefRangeEnd = 1286402, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_contentColor_Public_Static_get_Color_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(Color*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(1)]
		[CachedScanResults(RefRangeStart = 1286409, RefRangeEnd = 1286410, XrefRangeStart = 1286404, XrefRangeEnd = 1286409, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_contentColor_Public_Static_set_Void_Color_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static bool changed
	{
		[CallerCount(2)]
		[CachedScanResults(RefRangeStart = 1286412, RefRangeEnd = 1286414, XrefRangeStart = 1286410, XrefRangeEnd = 1286412, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_changed_Public_Static_get_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(3)]
		[CachedScanResults(RefRangeStart = 1286416, RefRangeEnd = 1286419, XrefRangeStart = 1286414, XrefRangeEnd = 1286416, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_changed_Public_Static_set_Void_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static bool enabled
	{
		[CallerCount(2)]
		[CachedScanResults(RefRangeStart = 1286421, RefRangeEnd = 1286423, XrefRangeStart = 1286419, XrefRangeEnd = 1286421, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_enabled_Public_Static_get_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(2)]
		[CachedScanResults(RefRangeStart = 1286425, RefRangeEnd = 1286427, XrefRangeStart = 1286423, XrefRangeEnd = 1286425, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_enabled_Public_Static_set_Void_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static bool usePageScrollbars
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1286427, XrefRangeEnd = 1286429, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_usePageScrollbars_Internal_Static_get_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
	}

	public unsafe static int scrollTroughSide
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1286474, XrefRangeEnd = 1286478, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_scrollTroughSide_Internal_Static_get_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1286478, XrefRangeEnd = 1286482, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_scrollTroughSide_Internal_Static_set_Void_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static Il2CppSystem.DateTime nextScrollStepTime
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1286482, XrefRangeEnd = 1286486, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_nextScrollStepTime_Internal_Static_get_DateTime_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(Il2CppSystem.DateTime*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1286486, XrefRangeEnd = 1286490, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_nextScrollStepTime_Internal_Static_set_Void_DateTime_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static GUISkin skin
	{
		[CallerCount(28)]
		[CachedScanResults(RefRangeStart = 1286508, RefRangeEnd = 1286536, XrefRangeStart = 1286500, XrefRangeEnd = 1286508, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_skin_Public_Static_get_GUISkin_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<GUISkin>(intPtr) : null;
		}
		[CallerCount(2)]
		[CachedScanResults(RefRangeStart = 1286498, RefRangeEnd = 1286500, XrefRangeStart = 1286490, XrefRangeEnd = 1286498, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_skin_Public_Static_set_Void_GUISkin_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static Matrix4x4 matrix
	{
		[CallerCount(2)]
		[CachedScanResults(RefRangeStart = 1286578, RefRangeEnd = 1286580, XrefRangeStart = 1286576, XrefRangeEnd = 1286578, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_matrix_Public_Static_get_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(1)]
		[CachedScanResults(RefRangeStart = 1286582, RefRangeEnd = 1286583, XrefRangeStart = 1286580, XrefRangeEnd = 1286582, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_matrix_Public_Static_set_Void_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static GenericStack scrollViewStates
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1287595, XrefRangeEnd = 1287599, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_scrollViewStates_Internal_Static_get_GenericStack_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<GenericStack>(intPtr) : null;
		}
		set
		{
			_scrollViewStates_k__BackingField = value;
		}
	}

	public static int depth
	{
		get
		{
			return get_depthDelegateField();
		}
		set
		{
			set_depthDelegateField(value);
		}
	}

	public static bool isInsideList
	{
		get
		{
			return get_isInsideListDelegateField();
		}
		set
		{
			set_isInsideListDelegateField(value);
		}
	}

	public static Material blendMaterial => Unmarshal.UnmarshalUnityObject<Material>(get_blendMaterial_Injected());

	public static Material blitMaterial => Unmarshal.UnmarshalUnityObject<Material>(get_blitMaterial_Injected());

	public static Material roundedRectMaterial => Unmarshal.UnmarshalUnityObject<Material>(get_roundedRectMaterial_Injected());

	public static Material roundedRectWithColorPerBorderMaterial => Unmarshal.UnmarshalUnityObject<Material>(get_roundedRectWithColorPerBorderMaterial_Injected());

	public static string tooltip
	{
		get
		{
			return Internal_GetTooltip();
		}
		set
		{
			Internal_SetTooltip(value);
		}
	}

	public static string mouseTooltip => Internal_GetMouseTooltip();

	public static Rect tooltipRect
	{
		get
		{
			return s_ToolTipRect;
		}
		set
		{
			s_ToolTipRect = value;
		}
	}

	static GUI()
	{
		Il2CppClassPointerStore<GUI>.NativeClassPtr = IL2CPP.GetIl2CppClass("UnityEngine.IMGUIModule.dll", "UnityEngine", "GUI");
		NativeFieldInfoPtr_s_ScrollControlId = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "s_ScrollControlId");
		NativeFieldInfoPtr_s_HotTextField = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "s_HotTextField");
		NativeFieldInfoPtr_s_BoxHash = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "s_BoxHash");
		NativeFieldInfoPtr_s_ButonHash = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "s_ButonHash");
		NativeFieldInfoPtr_s_RepeatButtonHash = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "s_RepeatButtonHash");
		NativeFieldInfoPtr_s_ToggleHash = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "s_ToggleHash");
		NativeFieldInfoPtr_s_ButtonGridHash = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "s_ButtonGridHash");
		NativeFieldInfoPtr_s_SliderHash = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "s_SliderHash");
		NativeFieldInfoPtr_s_BeginGroupHash = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "s_BeginGroupHash");
		NativeFieldInfoPtr_s_ScrollviewHash = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "s_ScrollviewHash");
		NativeFieldInfoPtr__scrollTroughSide_k__BackingField = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "<scrollTroughSide>k__BackingField");
		NativeFieldInfoPtr__nextScrollStepTime_k__BackingField = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "<nextScrollStepTime>k__BackingField");
		NativeFieldInfoPtr_s_Skin = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "s_Skin");
		NativeFieldInfoPtr_s_ToolTipRect = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "s_ToolTipRect");
		NativeFieldInfoPtr__scrollViewStates_k__BackingField = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GUI>.NativeClassPtr, "<scrollViewStates>k__BackingField");
		NativeMethodInfoPtr_get_color_Public_Static_get_Color_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663393);
		NativeMethodInfoPtr_set_color_Public_Static_set_Void_Color_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663394);
		NativeMethodInfoPtr_get_backgroundColor_Public_Static_get_Color_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663395);
		NativeMethodInfoPtr_set_backgroundColor_Public_Static_set_Void_Color_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663396);
		NativeMethodInfoPtr_get_contentColor_Public_Static_get_Color_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663397);
		NativeMethodInfoPtr_set_contentColor_Public_Static_set_Void_Color_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663398);
		NativeMethodInfoPtr_get_changed_Public_Static_get_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663399);
		NativeMethodInfoPtr_set_changed_Public_Static_set_Void_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663400);
		NativeMethodInfoPtr_get_enabled_Public_Static_get_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663401);
		NativeMethodInfoPtr_set_enabled_Public_Static_set_Void_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663402);
		NativeMethodInfoPtr_get_usePageScrollbars_Internal_Static_get_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663403);
		NativeMethodInfoPtr_GrabMouseControl_Internal_Static_Void_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663404);
		NativeMethodInfoPtr_HasMouseControl_Internal_Static_Boolean_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663405);
		NativeMethodInfoPtr_ReleaseMouseControl_Internal_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663406);
		NativeMethodInfoPtr_SetNextControlName_Public_Static_Void_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663407);
		NativeMethodInfoPtr_FocusControl_Public_Static_Void_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663408);
		NativeMethodInfoPtr_InternalRepaintEditorWindow_Internal_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663409);
		NativeMethodInfoPtr_get_scrollTroughSide_Internal_Static_get_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663411);
		NativeMethodInfoPtr_set_scrollTroughSide_Internal_Static_set_Void_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663412);
		NativeMethodInfoPtr_get_nextScrollStepTime_Internal_Static_get_DateTime_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663413);
		NativeMethodInfoPtr_set_nextScrollStepTime_Internal_Static_set_Void_DateTime_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663414);
		NativeMethodInfoPtr_set_skin_Public_Static_set_Void_GUISkin_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663415);
		NativeMethodInfoPtr_get_skin_Public_Static_get_GUISkin_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663416);
		NativeMethodInfoPtr_DoSetSkin_Internal_Static_Void_GUISkin_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663417);
		NativeMethodInfoPtr_get_matrix_Public_Static_get_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663418);
		NativeMethodInfoPtr_set_matrix_Public_Static_set_Void_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663419);
		NativeMethodInfoPtr_Label_Public_Static_Void_Rect_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663420);
		NativeMethodInfoPtr_Label_Public_Static_Void_Rect_String_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663421);
		NativeMethodInfoPtr_Label_Public_Static_Void_Rect_GUIContent_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663422);
		NativeMethodInfoPtr_Box_Public_Static_Void_Rect_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663423);
		NativeMethodInfoPtr_Box_Public_Static_Void_Rect_GUIContent_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663424);
		NativeMethodInfoPtr_Box_Public_Static_Void_Rect_String_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663425);
		NativeMethodInfoPtr_Box_Public_Static_Void_Rect_GUIContent_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663426);
		NativeMethodInfoPtr_Button_Public_Static_Boolean_Rect_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663427);
		NativeMethodInfoPtr_Button_Public_Static_Boolean_Rect_GUIContent_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663428);
		NativeMethodInfoPtr_Button_Internal_Static_Boolean_Rect_Int32_GUIContent_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663429);
		NativeMethodInfoPtr_DoRepeatButton_Private_Static_Boolean_Rect_GUIContent_GUIStyle_FocusType_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663430);
		NativeMethodInfoPtr_TextField_Public_Static_String_Rect_String_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663431);
		NativeMethodInfoPtr_PasswordFieldGetStrToShow_Internal_Static_String_String_Char_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663432);
		NativeMethodInfoPtr_DoTextField_Internal_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663433);
		NativeMethodInfoPtr_DoTextField_Internal_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663434);
		NativeMethodInfoPtr_DoTextField_Internal_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_String_Char_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663435);
		NativeMethodInfoPtr_HandleTextFieldEventForTouchscreen_Private_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_String_Char_TextEditor_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663436);
		NativeMethodInfoPtr_HandleTextFieldEventForDesktop_Private_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_TextEditor_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663437);
		NativeMethodInfoPtr_DoControl_Internal_Static_Boolean_Rect_Int32_Boolean_Boolean_GUIContent_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663438);
		NativeMethodInfoPtr_DoLabel_Private_Static_Void_Rect_GUIContent_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663439);
		NativeMethodInfoPtr_DoButton_Internal_Static_Boolean_Rect_Int32_GUIContent_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663440);
		NativeMethodInfoPtr_Slider_Public_Static_Single_Rect_Single_Single_Single_Single_GUIStyle_GUIStyle_Boolean_Int32_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663441);
		NativeMethodInfoPtr_HorizontalScrollbar_Public_Static_Single_Rect_Single_Single_Single_Single_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663442);
		NativeMethodInfoPtr_ScrollerRepeatButton_Internal_Static_Boolean_Int32_Rect_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663443);
		NativeMethodInfoPtr_VerticalScrollbar_Public_Static_Single_Rect_Single_Single_Single_Single_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663444);
		NativeMethodInfoPtr_Scroller_Internal_Static_Single_Rect_Single_Single_Single_Single_GUIStyle_GUIStyle_GUIStyle_GUIStyle_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663445);
		NativeMethodInfoPtr_BeginGroup_Public_Static_Void_Rect_GUIContent_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663446);
		NativeMethodInfoPtr_BeginGroup_Internal_Static_Void_Rect_GUIContent_GUIStyle_Vector2_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663447);
		NativeMethodInfoPtr_EndGroup_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663448);
		NativeMethodInfoPtr_get_scrollViewStates_Internal_Static_get_GenericStack_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663449);
		NativeMethodInfoPtr_BeginScrollView_Public_Static_Vector2_Rect_Vector2_Rect_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663450);
		NativeMethodInfoPtr_BeginScrollView_Internal_Static_Vector2_Rect_Vector2_Rect_Boolean_Boolean_GUIStyle_GUIStyle_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663451);
		NativeMethodInfoPtr_EndScrollView_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663452);
		NativeMethodInfoPtr_EndScrollView_Public_Static_Void_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663453);
		NativeMethodInfoPtr_CallWindowDelegate_Internal_Static_Void_WindowFunction_Int32_Int32_GUISkin_Int32_Single_Single_GUIStyle_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663454);
		NativeMethodInfoPtr_get_color_Injected_Private_Static_Void_byref_Color_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663455);
		NativeMethodInfoPtr_set_color_Injected_Private_Static_Void_byref_Color_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663456);
		NativeMethodInfoPtr_get_backgroundColor_Injected_Private_Static_Void_byref_Color_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663457);
		NativeMethodInfoPtr_set_backgroundColor_Injected_Private_Static_Void_byref_Color_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663458);
		NativeMethodInfoPtr_get_contentColor_Injected_Private_Static_Void_byref_Color_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663459);
		NativeMethodInfoPtr_set_contentColor_Injected_Private_Static_Void_byref_Color_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663460);
		NativeMethodInfoPtr_SetNextControlName_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663461);
		NativeMethodInfoPtr_FocusControl_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GUI>.NativeClassPtr, 100663462);
		get_depthDelegateField = IL2CPP.ResolveICall<get_depthDelegate>("UnityEngine.GUI::get_depth");
		set_depthDelegateField = IL2CPP.ResolveICall<set_depthDelegate>("UnityEngine.GUI::set_depth");
		get_isInsideListDelegateField = IL2CPP.ResolveICall<get_isInsideListDelegate>("UnityEngine.GUI::get_isInsideList");
		set_isInsideListDelegateField = IL2CPP.ResolveICall<set_isInsideListDelegate>("UnityEngine.GUI::set_isInsideList");
		BringWindowToFrontDelegateField = IL2CPP.ResolveICall<BringWindowToFrontDelegate>("UnityEngine.GUI::BringWindowToFront");
		BringWindowToBackDelegateField = IL2CPP.ResolveICall<BringWindowToBackDelegate>("UnityEngine.GUI::BringWindowToBack");
		FocusWindowDelegateField = IL2CPP.ResolveICall<FocusWindowDelegate>("UnityEngine.GUI::FocusWindow");
		UnfocusWindowDelegateField = IL2CPP.ResolveICall<UnfocusWindowDelegate>("UnityEngine.GUI::UnfocusWindow");
		Internal_BeginWindowsDelegateField = IL2CPP.ResolveICall<Internal_BeginWindowsDelegate>("UnityEngine.GUI::Internal_BeginWindows");
		Internal_EndWindowsDelegateField = IL2CPP.ResolveICall<Internal_EndWindowsDelegate>("UnityEngine.GUI::Internal_EndWindows");
		get_blendMaterial_InjectedDelegateField = IL2CPP.ResolveICall<get_blendMaterial_InjectedDelegate>("UnityEngine.GUI::get_blendMaterial_Injected");
		get_blitMaterial_InjectedDelegateField = IL2CPP.ResolveICall<get_blitMaterial_InjectedDelegate>("UnityEngine.GUI::get_blitMaterial_Injected");
		get_roundedRectMaterial_InjectedDelegateField = IL2CPP.ResolveICall<get_roundedRectMaterial_InjectedDelegate>("UnityEngine.GUI::get_roundedRectMaterial_Injected");
		get_roundedRectWithColorPerBorderMaterial_InjectedDelegateField = IL2CPP.ResolveICall<get_roundedRectWithColorPerBorderMaterial_InjectedDelegate>("UnityEngine.GUI::get_roundedRectWithColorPerBorderMaterial_Injected");
		GetNameOfFocusedControl_InjectedDelegateField = IL2CPP.ResolveICall<GetNameOfFocusedControl_InjectedDelegate>("UnityEngine.GUI::GetNameOfFocusedControl_Injected");
		Internal_GetTooltip_InjectedDelegateField = IL2CPP.ResolveICall<Internal_GetTooltip_InjectedDelegate>("UnityEngine.GUI::Internal_GetTooltip_Injected");
		Internal_SetTooltip_InjectedDelegateField = IL2CPP.ResolveICall<Internal_SetTooltip_InjectedDelegate>("UnityEngine.GUI::Internal_SetTooltip_Injected");
		Internal_GetMouseTooltip_InjectedDelegateField = IL2CPP.ResolveICall<Internal_GetMouseTooltip_InjectedDelegate>("UnityEngine.GUI::Internal_GetMouseTooltip_Injected");
		Internal_DoModalWindow_InjectedDelegateField = IL2CPP.ResolveICall<Internal_DoModalWindow_InjectedDelegate>("UnityEngine.GUI::Internal_DoModalWindow_Injected");
		Internal_DoWindow_InjectedDelegateField = IL2CPP.ResolveICall<Internal_DoWindow_InjectedDelegate>("UnityEngine.GUI::Internal_DoWindow_Injected");
		DragWindow_InjectedDelegateField = IL2CPP.ResolveICall<DragWindow_InjectedDelegate>("UnityEngine.GUI::DragWindow_Injected");
		Internal_Concatenate_InjectedDelegateField = IL2CPP.ResolveICall<Internal_Concatenate_InjectedDelegate>("UnityEngine.GUI::Internal_Concatenate_Injected");
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1286429, XrefRangeEnd = 1286431, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void GrabMouseControl(int id)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&id);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GrabMouseControl_Internal_Static_Void_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1286431, XrefRangeEnd = 1286433, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool HasMouseControl(int id)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&id);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HasMouseControl_Internal_Static_Boolean_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1286433, XrefRangeEnd = 1286435, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void ReleaseMouseControl()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReleaseMouseControl_Internal_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1286452, RefRangeEnd = 1286453, XrefRangeStart = 1286435, XrefRangeEnd = 1286452, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SetNextControlName(string name)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(name);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetNextControlName_Public_Static_Void_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1286470, RefRangeEnd = 1286471, XrefRangeStart = 1286453, XrefRangeEnd = 1286470, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void FocusControl(string name)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(name);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_FocusControl_Public_Static_Void_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1286473, RefRangeEnd = 1286474, XrefRangeStart = 1286471, XrefRangeEnd = 1286473, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void InternalRepaintEditorWindow()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_InternalRepaintEditorWindow_Internal_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 1286573, RefRangeEnd = 1286576, XrefRangeStart = 1286536, XrefRangeEnd = 1286573, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void DoSetSkin(GUISkin newSkin)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)newSkin);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_DoSetSkin_Internal_Static_Void_GUISkin_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(36)]
	[CachedScanResults(RefRangeStart = 1286599, RefRangeEnd = 1286635, XrefRangeStart = 1286583, XrefRangeEnd = 1286599, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Label(Rect position, string text)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&position);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(text);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Label_Public_Static_Void_Rect_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(64)]
	[CachedScanResults(RefRangeStart = 1286650, RefRangeEnd = 1286714, XrefRangeStart = 1286635, XrefRangeEnd = 1286650, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Label(Rect position, string text, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&position);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(text);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Label_Public_Static_Void_Rect_String_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1286714, XrefRangeEnd = 1286722, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Label(Rect position, GUIContent content, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&position);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Label_Public_Static_Void_Rect_GUIContent_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 1286731, RefRangeEnd = 1286733, XrefRangeStart = 1286722, XrefRangeEnd = 1286731, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Box(Rect position, string text)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&position);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(text);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Box_Public_Static_Void_Rect_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1286738, RefRangeEnd = 1286739, XrefRangeStart = 1286733, XrefRangeEnd = 1286738, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Box(Rect position, GUIContent content)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&position);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Box_Public_Static_Void_Rect_GUIContent_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 1286747, RefRangeEnd = 1286749, XrefRangeStart = 1286739, XrefRangeEnd = 1286747, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Box(Rect position, string text, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&position);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(text);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Box_Public_Static_Void_Rect_String_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(8)]
	[CachedScanResults(RefRangeStart = 1286770, RefRangeEnd = 1286778, XrefRangeStart = 1286749, XrefRangeEnd = 1286770, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Box(Rect position, GUIContent content, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&position);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Box_Public_Static_Void_Rect_GUIContent_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 1286787, RefRangeEnd = 1286789, XrefRangeStart = 1286778, XrefRangeEnd = 1286787, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool Button(Rect position, string text)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&position);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(text);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Button_Public_Static_Boolean_Rect_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 1286814, RefRangeEnd = 1286817, XrefRangeStart = 1286789, XrefRangeEnd = 1286814, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool Button(Rect position, GUIContent content, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&position);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Button_Public_Static_Boolean_Rect_GUIContent_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1286817, XrefRangeEnd = 1286834, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool Button(Rect position, int id, GUIContent content, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = (nint)(&position);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &id;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Button_Internal_Static_Boolean_Rect_Int32_GUIContent_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1286899, RefRangeEnd = 1286900, XrefRangeStart = 1286834, XrefRangeEnd = 1286899, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool DoRepeatButton(Rect position, GUIContent content, GUIStyle style, FocusType focusType)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = (nint)(&position);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		*(FocusType**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &focusType;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_DoRepeatButton_Private_Static_Boolean_Rect_GUIContent_GUIStyle_FocusType_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1286922, RefRangeEnd = 1286923, XrefRangeStart = 1286900, XrefRangeEnd = 1286922, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static string TextField(Rect position, string text, int maxLength)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&position);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(text);
		*(int**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &maxLength;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_TextField_Public_Static_String_Rect_String_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return IL2CPP.Il2CppStringToManaged(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1286923, XrefRangeEnd = 1286936, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static string PasswordFieldGetStrToShow(string password, char maskChar)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(password);
		*(char**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &maskChar;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_PasswordFieldGetStrToShow_Internal_Static_String_String_Char_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return IL2CPP.Il2CppStringToManaged(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1286936, XrefRangeEnd = 1286943, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void DoTextField(Rect position, int id, GUIContent content, bool multiline, int maxLength, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[6];
		*ptr = (nint)(&position);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &id;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(bool**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &multiline;
		*(int**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &maxLength;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_DoTextField_Internal_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1286943, XrefRangeEnd = 1286947, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void DoTextField(Rect position, int id, GUIContent content, bool multiline, int maxLength, GUIStyle style, string secureText)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[7];
		*ptr = (nint)(&position);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &id;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(bool**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &multiline;
		*(int**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &maxLength;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)6u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(secureText);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_DoTextField_Internal_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 1286986, RefRangeEnd = 1286989, XrefRangeStart = 1286947, XrefRangeEnd = 1286986, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void DoTextField(Rect position, int id, GUIContent content, bool multiline, int maxLength, GUIStyle style, string secureText, char maskChar)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[8];
		*ptr = (nint)(&position);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &id;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(bool**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &multiline;
		*(int**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &maxLength;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)6u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(secureText);
		*(char**)((byte*)ptr + checked((nuint)7u * unchecked((nuint)sizeof(System.IntPtr)))) = &maskChar;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_DoTextField_Internal_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_String_Char_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1287034, RefRangeEnd = 1287035, XrefRangeStart = 1286989, XrefRangeEnd = 1287034, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void HandleTextFieldEventForTouchscreen(Rect position, int id, GUIContent content, bool multiline, int maxLength, GUIStyle style, string secureText, char maskChar, TextEditor editor)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[9];
		*ptr = (nint)(&position);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &id;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(bool**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &multiline;
		*(int**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &maxLength;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)6u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(secureText);
		*(char**)((byte*)ptr + checked((nuint)7u * unchecked((nuint)sizeof(System.IntPtr)))) = &maskChar;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)8u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)editor);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleTextFieldEventForTouchscreen_Private_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_String_Char_TextEditor_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1287184, RefRangeEnd = 1287185, XrefRangeStart = 1287035, XrefRangeEnd = 1287184, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void HandleTextFieldEventForDesktop(Rect position, int id, GUIContent content, bool multiline, int maxLength, GUIStyle style, TextEditor editor)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[7];
		*ptr = (nint)(&position);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &id;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(bool**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &multiline;
		*(int**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &maxLength;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)6u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)editor);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleTextFieldEventForDesktop_Private_Static_Void_Rect_Int32_GUIContent_Boolean_Int32_GUIStyle_TextEditor_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 1287247, RefRangeEnd = 1287250, XrefRangeStart = 1287185, XrefRangeEnd = 1287247, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool DoControl(Rect position, int id, bool on, bool hover, GUIContent content, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[6];
		*ptr = (nint)(&position);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &id;
		*(bool**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &on;
		*(bool**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &hover;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_DoControl_Internal_Static_Boolean_Rect_Int32_Boolean_Boolean_GUIContent_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 1287290, RefRangeEnd = 1287294, XrefRangeStart = 1287250, XrefRangeEnd = 1287290, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void DoLabel(Rect position, GUIContent content, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&position);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_DoLabel_Private_Static_Void_Rect_GUIContent_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1287294, XrefRangeEnd = 1287304, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool DoButton(Rect position, int id, GUIContent content, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = (nint)(&position);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &id;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_DoButton_Internal_Static_Boolean_Rect_Int32_GUIContent_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1287304, XrefRangeEnd = 1287317, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static float Slider(Rect position, float value, float size, float start, float end, GUIStyle slider, GUIStyle thumb, bool horiz, int id, GUIStyle thumbExtent = null)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[10];
		*ptr = (nint)(&position);
		*(float**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &value;
		*(float**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &size;
		*(float**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &start;
		*(float**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &end;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)slider);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)6u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)thumb);
		*(bool**)((byte*)ptr + checked((nuint)7u * unchecked((nuint)sizeof(System.IntPtr)))) = &horiz;
		*(int**)((byte*)ptr + checked((nuint)8u * unchecked((nuint)sizeof(System.IntPtr)))) = &id;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)9u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)thumbExtent);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Slider_Public_Static_Single_Rect_Single_Single_Single_Single_GUIStyle_GUIStyle_Boolean_Int32_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1287342, RefRangeEnd = 1287343, XrefRangeStart = 1287317, XrefRangeEnd = 1287342, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static float HorizontalScrollbar(Rect position, float value, float size, float leftValue, float rightValue, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[6];
		*ptr = (nint)(&position);
		*(float**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &value;
		*(float**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &size;
		*(float**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &leftValue;
		*(float**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &rightValue;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HorizontalScrollbar_Public_Static_Single_Rect_Single_Single_Single_Single_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 1287391, RefRangeEnd = 1287393, XrefRangeStart = 1287343, XrefRangeEnd = 1287391, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool ScrollerRepeatButton(int scrollerID, Rect rect, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&scrollerID);
		*(Rect**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &rect;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ScrollerRepeatButton_Internal_Static_Boolean_Int32_Rect_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1287418, RefRangeEnd = 1287419, XrefRangeStart = 1287393, XrefRangeEnd = 1287418, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static float VerticalScrollbar(Rect position, float value, float size, float topValue, float bottomValue, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[6];
		*ptr = (nint)(&position);
		*(float**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &value;
		*(float**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &size;
		*(float**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &topValue;
		*(float**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &bottomValue;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_VerticalScrollbar_Public_Static_Single_Rect_Single_Single_Single_Single_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 1287542, RefRangeEnd = 1287544, XrefRangeStart = 1287419, XrefRangeEnd = 1287542, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static float Scroller(Rect position, float value, float size, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb, GUIStyle leftButton, GUIStyle rightButton, bool horiz)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[10];
		*ptr = (nint)(&position);
		*(float**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &value;
		*(float**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &size;
		*(float**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &leftValue;
		*(float**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &rightValue;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)slider);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)6u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)thumb);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)7u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)leftButton);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)8u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)rightButton);
		*(bool**)((byte*)ptr + checked((nuint)9u * unchecked((nuint)sizeof(System.IntPtr)))) = &horiz;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Scroller_Internal_Static_Single_Rect_Single_Single_Single_Single_GUIStyle_GUIStyle_GUIStyle_GUIStyle_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1287544, XrefRangeEnd = 1287550, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void BeginGroup(Rect position, GUIContent content, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&position);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_BeginGroup_Public_Static_Void_Rect_GUIContent_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 1287587, RefRangeEnd = 1287589, XrefRangeStart = 1287550, XrefRangeEnd = 1287587, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void BeginGroup(Rect position, GUIContent content, GUIStyle style, Vector2 scrollOffset)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = (nint)(&position);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		*(Vector2**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &scrollOffset;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_BeginGroup_Internal_Static_Void_Rect_GUIContent_GUIStyle_Vector2_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1287589, XrefRangeEnd = 1287595, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void EndGroup()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_EndGroup_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1287606, RefRangeEnd = 1287607, XrefRangeStart = 1287599, XrefRangeEnd = 1287606, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&position);
		*(Vector2**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &scrollPosition;
		*(Rect**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &viewRect;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_BeginScrollView_Public_Static_Vector2_Rect_Vector2_Rect_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector2*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 1287748, RefRangeEnd = 1287750, XrefRangeStart = 1287607, XrefRangeEnd = 1287748, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[8];
		*ptr = (nint)(&position);
		*(Vector2**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &scrollPosition;
		*(Rect**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &viewRect;
		*(bool**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &alwaysShowHorizontal;
		*(bool**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &alwaysShowVertical;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)horizontalScrollbar);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)6u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)verticalScrollbar);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)7u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)background);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_BeginScrollView_Internal_Static_Vector2_Rect_Vector2_Rect_Boolean_Boolean_GUIStyle_GUIStyle_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector2*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1287754, RefRangeEnd = 1287755, XrefRangeStart = 1287750, XrefRangeEnd = 1287754, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void EndScrollView()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_EndScrollView_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 1287922, RefRangeEnd = 1287925, XrefRangeStart = 1287755, XrefRangeEnd = 1287922, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void EndScrollView(bool handleScrollWheel)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&handleScrollWheel);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_EndScrollView_Public_Static_Void_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1287925, XrefRangeEnd = 1287974, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void CallWindowDelegate(WindowFunction func, int id, int instanceID, GUISkin _skin, int forceRect, float width, float height, GUIStyle style)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[8];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)func);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &id;
		*(int**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &instanceID;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)_skin);
		*(int**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &forceRect;
		*(float**)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = &width;
		*(float**)((byte*)ptr + checked((nuint)6u * unchecked((nuint)sizeof(System.IntPtr)))) = &height;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)7u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CallWindowDelegate_Internal_Static_Void_WindowFunction_Int32_Int32_GUISkin_Int32_Single_Single_GUIStyle_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1287974, XrefRangeEnd = 1287976, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void get_color_Injected(out Color ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_color_Injected_Private_Static_Void_byref_Color_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1287976, XrefRangeEnd = 1287978, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void set_color_Injected([In] ref Color value)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)Unsafe.AsPointer(ref value);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_color_Injected_Private_Static_Void_byref_Color_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1287978, XrefRangeEnd = 1287980, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void get_backgroundColor_Injected(out Color ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_backgroundColor_Injected_Private_Static_Void_byref_Color_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1287980, XrefRangeEnd = 1287982, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void set_backgroundColor_Injected([In] ref Color value)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)Unsafe.AsPointer(ref value);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_backgroundColor_Injected_Private_Static_Void_byref_Color_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1287982, XrefRangeEnd = 1287984, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void get_contentColor_Injected(out Color ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_contentColor_Injected_Private_Static_Void_byref_Color_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1287984, XrefRangeEnd = 1287986, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void set_contentColor_Injected([In] ref Color value)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)Unsafe.AsPointer(ref value);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_contentColor_Injected_Private_Static_Void_byref_Color_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1287986, XrefRangeEnd = 1287988, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SetNextControlName_Injected(ref ManagedSpanWrapper name)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)Unsafe.AsPointer(ref name);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetNextControlName_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1287988, XrefRangeEnd = 1287990, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void FocusControl_Injected(ref ManagedSpanWrapper name)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)Unsafe.AsPointer(ref name);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_FocusControl_Injected_Private_Static_Void_byref_ManagedSpanWrapper_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	public GUI(System.IntPtr pointer)
		: base(pointer)
	{
	}

	public static string GetNameOfFocusedControl()
	{
		Unsafe.SkipInit(out ManagedSpanWrapper ret);
		string stringAndDispose;
		try
		{
			GetNameOfFocusedControl_Injected(out ret);
		}
		finally
		{
			stringAndDispose = OutStringMarshaller.GetStringAndDispose(ret);
		}
		return stringAndDispose;
	}

	public static string Internal_GetTooltip()
	{
		Unsafe.SkipInit(out ManagedSpanWrapper ret);
		string stringAndDispose;
		try
		{
			Internal_GetTooltip_Injected(out ret);
		}
		finally
		{
			stringAndDispose = OutStringMarshaller.GetStringAndDispose(ret);
		}
		return stringAndDispose;
	}

	public unsafe static void Internal_SetTooltip(string value)
	{
		//IL_0018: Expected Obj, but got Ref
		//IL_0024: Expected Obj, but got Ref
		//The blocks IL_0029 are reachable both inside and outside the pinned region starting at IL_0018. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		try
		{
			Unsafe.SkipInit(out ManagedSpanWrapper managedSpanWrapper);
			if (!StringMarshaller.TryMarshalEmptyOrNullString(value, ref managedSpanWrapper))
			{
				Il2CppSystem.ReadOnlySpan<char> readOnlySpan = Il2CppSystem.MemoryExtensions.AsSpan(value);
				fixed (char* begin = &((Il2CppSystem.ReadOnlySpan<char>)(&readOnlySpan)).GetPinnableReference())
				{
					managedSpanWrapper = new ManagedSpanWrapper(begin, ((Il2CppSystem.ReadOnlySpan<char>)(&readOnlySpan)).Length);
					Internal_SetTooltip_Injected(ref managedSpanWrapper);
					return;
				}
			}
			Internal_SetTooltip_Injected(ref managedSpanWrapper);
		}
		finally
		{
		}
	}

	public static string Internal_GetMouseTooltip()
	{
		Unsafe.SkipInit(out ManagedSpanWrapper ret);
		string stringAndDispose;
		try
		{
			Internal_GetMouseTooltip_Injected(out ret);
		}
		finally
		{
			stringAndDispose = OutStringMarshaller.GetStringAndDispose(ret);
		}
		return stringAndDispose;
	}

	public static Rect Internal_DoModalWindow(int id, int instanceID, Rect clientRect, WindowFunction func, GUIContent content, GUIStyle style, Il2CppSystem.Object skin)
	{
		Internal_DoModalWindow_Injected(id, instanceID, ref clientRect, func, content, style, skin, out var ret);
		return ret;
	}

	public static Rect Internal_DoWindow(int id, int instanceID, Rect clientRect, WindowFunction func, GUIContent title, GUIStyle style, Il2CppSystem.Object skin, bool forceRectOnLayout)
	{
		Internal_DoWindow_Injected(id, instanceID, ref clientRect, func, title, style, skin, forceRectOnLayout, out var ret);
		return ret;
	}

	public static void DragWindow(Rect position)
	{
		DragWindow_Injected(ref position);
	}

	public static void BringWindowToFront(int windowID)
	{
		BringWindowToFrontDelegateField(windowID);
	}

	public static void BringWindowToBack(int windowID)
	{
		BringWindowToBackDelegateField(windowID);
	}

	public static void FocusWindow(int windowID)
	{
		FocusWindowDelegateField(windowID);
	}

	public static void UnfocusWindow()
	{
		UnfocusWindowDelegateField();
	}

	public static void Internal_BeginWindows()
	{
		Internal_BeginWindowsDelegateField();
	}

	public static void Internal_EndWindows()
	{
		Internal_EndWindowsDelegateField();
	}

	public static string Internal_Concatenate(GUIContent first, GUIContent second)
	{
		Unsafe.SkipInit(out ManagedSpanWrapper ret);
		string stringAndDispose;
		try
		{
			Internal_Concatenate_Injected(first, second, out ret);
		}
		finally
		{
			stringAndDispose = OutStringMarshaller.GetStringAndDispose(ret);
		}
		return stringAndDispose;
	}

	public static void CleanupRoots()
	{
		s_Skin = null;
		GUIUtility.CleanupRoots();
		GUILayoutUtility.CleanupRoots();
		GUISkin.CleanupRoots();
		GUIStyle.CleanupRoots();
	}

	public static void Label(Rect position, Texture image)
	{
		Label(position, GUIContent.Temp(image), s_Skin.label);
	}

	public static void Label(Rect position, GUIContent content)
	{
		Label(position, content, s_Skin.label);
	}

	public static void Label(Rect position, Texture image, GUIStyle style)
	{
		Label(position, GUIContent.Temp(image), style);
	}

	public static void DrawTexture(Rect position, Texture image)
	{
		DrawTexture(position, image, ScaleMode.StretchToFill);
	}

	public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode)
	{
		DrawTexture(position, image, scaleMode, alphaBlend: true);
	}

	public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend)
	{
		DrawTexture(position, image, scaleMode, alphaBlend, 0f);
	}

	public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect)
	{
		DrawTexture(position, image, scaleMode, alphaBlend, imageAspect, color, 0f, 0f);
	}

	public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect, Color color, float borderWidth, float borderRadius)
	{
		Vector4 borderWidths = Vector4.one * borderWidth;
		DrawTexture(position, image, scaleMode, alphaBlend, imageAspect, color, borderWidths, borderRadius);
	}

	public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect, Color color, Vector4 borderWidths, float borderRadius)
	{
		Vector4 borderRadiuses = Vector4.one * borderRadius;
		DrawTexture(position, image, scaleMode, alphaBlend, imageAspect, color, borderWidths, borderRadiuses);
	}

	public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect, Color color, Vector4 borderWidths, Vector4 borderRadiuses)
	{
		DrawTexture(position, image, scaleMode, alphaBlend, imageAspect, color, borderWidths, borderRadiuses, drawSmoothCorners: true);
	}

	public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect, Color color, Vector4 borderWidths, Vector4 borderRadiuses, bool drawSmoothCorners)
	{
		DrawTexture(position, image, scaleMode, alphaBlend, imageAspect, color, color, color, color, borderWidths, borderRadiuses, drawSmoothCorners);
	}

	public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect, Color leftColor, Color topColor, Color rightColor, Color bottomColor, Vector4 borderWidths, Vector4 borderRadiuses)
	{
		DrawTexture(position, image, scaleMode, alphaBlend, imageAspect, leftColor, topColor, rightColor, bottomColor, borderWidths, borderRadiuses, drawSmoothCorners: true);
	}

	public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect, Color leftColor, Color topColor, Color rightColor, Color bottomColor, Vector4 borderWidths, Vector4 borderRadiuses, bool drawSmoothCorners)
	{
		throw new System.NotSupportedException("Method unstripping failed");
	}

	public static bool CalculateScaledTextureRects(Rect position, ScaleMode scaleMode, float imageAspect, ref Rect outScreenRect, ref Rect outSourceRect)
	{
		float num = position.width / position.height;
		bool result = false;
		switch (scaleMode)
		{
		case ScaleMode.StretchToFill:
			outScreenRect = position;
			outSourceRect = new Rect(0f, 0f, 1f, 1f);
			result = true;
			break;
		case ScaleMode.ScaleAndCrop:
			if (num > imageAspect)
			{
				float num4 = imageAspect / num;
				outScreenRect = position;
				outSourceRect = new Rect(0f, (1f - num4) * 0.5f, 1f, num4);
				result = true;
			}
			else
			{
				float num5 = num / imageAspect;
				outScreenRect = position;
				outSourceRect = new Rect(0.5f - num5 * 0.5f, 0f, num5, 1f);
				result = true;
			}
			break;
		case ScaleMode.ScaleToFit:
			if (num > imageAspect)
			{
				float num2 = imageAspect / num;
				outScreenRect = new Rect(position.xMin + position.width * (1f - num2) * 0.5f, position.yMin, num2 * position.width, position.height);
				outSourceRect = new Rect(0f, 0f, 1f, 1f);
				result = true;
			}
			else
			{
				float num3 = num / imageAspect;
				outScreenRect = new Rect(position.xMin, position.yMin + position.height * (1f - num3) * 0.5f, position.width, num3 * position.height);
				outSourceRect = new Rect(0f, 0f, 1f, 1f);
				result = true;
			}
			break;
		}
		return result;
	}

	public static void DrawTextureWithTexCoords(Rect position, Texture image, Rect texCoords)
	{
		DrawTextureWithTexCoords(position, image, texCoords, alphaBlend: true);
	}

	public static void DrawTextureWithTexCoords(Rect position, Texture image, Rect texCoords, bool alphaBlend)
	{
		throw new System.NotSupportedException("Method unstripping failed");
	}

	public static void Box(Rect position, Texture image)
	{
		Box(position, GUIContent.Temp(image), s_Skin.box);
	}

	public static void Box(Rect position, Texture image, GUIStyle style)
	{
		Box(position, GUIContent.Temp(image), style);
	}

	public static bool Button(Rect position, Texture image)
	{
		return Button(position, GUIContent.Temp(image), s_Skin.button);
	}

	public static bool Button(Rect position, GUIContent content)
	{
		return Button(position, content, s_Skin.button);
	}

	public static bool Button(Rect position, string text, GUIStyle style)
	{
		return Button(position, GUIContent.Temp(text), style);
	}

	public static bool Button(Rect position, Texture image, GUIStyle style)
	{
		return Button(position, GUIContent.Temp(image), style);
	}

	public static bool RepeatButton(Rect position, string text)
	{
		return DoRepeatButton(position, GUIContent.Temp(text), s_Skin.button, FocusType.Passive);
	}

	public static bool RepeatButton(Rect position, Texture image)
	{
		return DoRepeatButton(position, GUIContent.Temp(image), s_Skin.button, FocusType.Passive);
	}

	public static bool RepeatButton(Rect position, GUIContent content)
	{
		return DoRepeatButton(position, content, s_Skin.button, FocusType.Passive);
	}

	public static bool RepeatButton(Rect position, string text, GUIStyle style)
	{
		return DoRepeatButton(position, GUIContent.Temp(text), style, FocusType.Passive);
	}

	public static bool RepeatButton(Rect position, Texture image, GUIStyle style)
	{
		return DoRepeatButton(position, GUIContent.Temp(image), style, FocusType.Passive);
	}

	public static bool RepeatButton(Rect position, GUIContent content, GUIStyle style)
	{
		return DoRepeatButton(position, content, style, FocusType.Passive);
	}

	public static string TextField(Rect position, string text)
	{
		GUIContent gUIContent = GUIContent.Temp(text);
		DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: false, -1, skin.textField);
		return gUIContent.text;
	}

	public static string TextField(Rect position, string text, GUIStyle style)
	{
		GUIContent gUIContent = GUIContent.Temp(text);
		DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: false, -1, style);
		return gUIContent.text;
	}

	public static string TextField(Rect position, string text, int maxLength, GUIStyle style)
	{
		GUIContent gUIContent = GUIContent.Temp(text);
		DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: false, maxLength, style);
		return gUIContent.text;
	}

	public static string PasswordField(Rect position, string password, char maskChar)
	{
		return PasswordField(position, password, maskChar, -1, skin.textField);
	}

	public static string PasswordField(Rect position, string password, char maskChar, int maxLength)
	{
		return PasswordField(position, password, maskChar, maxLength, skin.textField);
	}

	public static string PasswordField(Rect position, string password, char maskChar, GUIStyle style)
	{
		return PasswordField(position, password, maskChar, -1, style);
	}

	public static string PasswordField(Rect position, string password, char maskChar, int maxLength, GUIStyle style)
	{
		GUIUtility.CheckOnGUI();
		string t = PasswordFieldGetStrToShow(password, maskChar);
		GUIContent gUIContent = GUIContent.Temp(t);
		bool flag = changed;
		changed = false;
		if (TouchScreenKeyboard.isSupported && !TouchScreenKeyboard.isInPlaceEditingAllowed)
		{
			DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard), gUIContent, multiline: false, maxLength, style, password, maskChar);
		}
		else
		{
			DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: false, maxLength, style);
		}
		t = (changed ? gUIContent.text : password);
		changed |= flag;
		return t;
	}

	public static string TextArea(Rect position, string text)
	{
		GUIContent gUIContent = GUIContent.Temp(text);
		DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: true, -1, skin.textArea);
		return gUIContent.text;
	}

	public static string TextArea(Rect position, string text, int maxLength)
	{
		GUIContent gUIContent = GUIContent.Temp(text);
		DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: true, maxLength, skin.textArea);
		return gUIContent.text;
	}

	public static string TextArea(Rect position, string text, GUIStyle style)
	{
		GUIContent gUIContent = GUIContent.Temp(text);
		DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: true, -1, style);
		return gUIContent.text;
	}

	public static string TextArea(Rect position, string text, int maxLength, GUIStyle style)
	{
		GUIContent gUIContent = GUIContent.Temp(text);
		DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: true, maxLength, style);
		return gUIContent.text;
	}

	public static bool Toggle(Rect position, bool value, string text)
	{
		return Toggle(position, value, GUIContent.Temp(text), s_Skin.toggle);
	}

	public static bool Toggle(Rect position, bool value, Texture image)
	{
		return Toggle(position, value, GUIContent.Temp(image), s_Skin.toggle);
	}

	public static bool Toggle(Rect position, bool value, GUIContent content)
	{
		return Toggle(position, value, content, s_Skin.toggle);
	}

	public static bool Toggle(Rect position, bool value, string text, GUIStyle style)
	{
		return Toggle(position, value, GUIContent.Temp(text), style);
	}

	public static bool Toggle(Rect position, bool value, Texture image, GUIStyle style)
	{
		return Toggle(position, value, GUIContent.Temp(image), style);
	}

	public static bool Toggle(Rect position, bool value, GUIContent content, GUIStyle style)
	{
		GUIUtility.CheckOnGUI();
		return DoToggle(position, GUIUtility.GetControlID(s_ToggleHash, FocusType.Passive, position), value, content, style);
	}

	public static bool Toggle(Rect position, int id, bool value, GUIContent content, GUIStyle style)
	{
		GUIUtility.CheckOnGUI();
		return DoToggle(position, id, value, content, style);
	}

	public static int Toolbar(Rect position, int selected, Il2CppStringArray texts)
	{
		return Toolbar(position, selected, GUIContent.Temp(texts), s_Skin.button);
	}

	public static int Toolbar(Rect position, int selected, Il2CppReferenceArray<Texture> images)
	{
		return Toolbar(position, selected, GUIContent.Temp(images), s_Skin.button);
	}

	public static int Toolbar(Rect position, int selected, Il2CppReferenceArray<GUIContent> contents)
	{
		return Toolbar(position, selected, contents, s_Skin.button);
	}

	public static int Toolbar(Rect position, int selected, Il2CppStringArray texts, GUIStyle style)
	{
		return Toolbar(position, selected, GUIContent.Temp(texts), style);
	}

	public static int Toolbar(Rect position, int selected, Il2CppReferenceArray<Texture> images, GUIStyle style)
	{
		return Toolbar(position, selected, GUIContent.Temp(images), style);
	}

	public static int Toolbar(Rect position, int selected, Il2CppReferenceArray<GUIContent> contents, GUIStyle style)
	{
		return Toolbar(position, selected, contents, null, style, ToolbarButtonSize.Fixed);
	}

	public static int Toolbar(Rect position, int selected, Il2CppReferenceArray<GUIContent> contents, GUIStyle style, ToolbarButtonSize buttonSize)
	{
		return Toolbar(position, selected, contents, null, style, buttonSize);
	}

	public static int Toolbar(Rect position, int selected, Il2CppReferenceArray<GUIContent> contents, Il2CppStringArray controlNames, GUIStyle style, ToolbarButtonSize buttonSize, [Optional] Il2CppStructArray<bool> contentsEnabled)
	{
		FindStyles(ref style, out var firstStyle, out var midStyle, out var lastStyle, "left", "mid", "right");
		return Toolbar(position, selected, contents, controlNames, style, firstStyle, midStyle, lastStyle, buttonSize, contentsEnabled);
	}

	public static int Toolbar(Rect position, int selected, Il2CppReferenceArray<GUIContent> contents, Il2CppStringArray controlNames, GUIStyle style, GUIStyle firstStyle, GUIStyle midStyle, GUIStyle lastStyle, ToolbarButtonSize buttonSize, [Optional] Il2CppStructArray<bool> contentsEnabled)
	{
		GUIUtility.CheckOnGUI();
		return DoButtonGrid(position, selected, contents, controlNames, ((Il2CppArrayBase)contents).Length, style, firstStyle, midStyle, lastStyle, buttonSize, contentsEnabled);
	}

	public static int SelectionGrid(Rect position, int selected, Il2CppStringArray texts, int xCount)
	{
		return SelectionGrid(position, selected, GUIContent.Temp(texts), xCount, null);
	}

	public static int SelectionGrid(Rect position, int selected, Il2CppReferenceArray<Texture> images, int xCount)
	{
		return SelectionGrid(position, selected, GUIContent.Temp(images), xCount, null);
	}

	public static int SelectionGrid(Rect position, int selected, Il2CppReferenceArray<GUIContent> content, int xCount)
	{
		return SelectionGrid(position, selected, content, xCount, null);
	}

	public static int SelectionGrid(Rect position, int selected, Il2CppStringArray texts, int xCount, GUIStyle style)
	{
		return SelectionGrid(position, selected, GUIContent.Temp(texts), xCount, style);
	}

	public static int SelectionGrid(Rect position, int selected, Il2CppReferenceArray<Texture> images, int xCount, GUIStyle style)
	{
		return SelectionGrid(position, selected, GUIContent.Temp(images), xCount, style);
	}

	public static int SelectionGrid(Rect position, int selected, Il2CppReferenceArray<GUIContent> contents, int xCount, GUIStyle style)
	{
		if (style == null)
		{
			style = s_Skin.button;
		}
		return DoButtonGrid(position, selected, contents, null, xCount, style, style, style, style, ToolbarButtonSize.Fixed);
	}

	public static void FindStyles(ref GUIStyle style, out GUIStyle firstStyle, out GUIStyle midStyle, out GUIStyle lastStyle, string first, string mid, string last)
	{
		if (style == null)
		{
			style = skin.button;
		}
		string name = style.name;
		midStyle = skin.FindStyle(Il2CppSystem.String.Concat(name, mid)) ?? style;
		firstStyle = skin.FindStyle(Il2CppSystem.String.Concat(name, first)) ?? midStyle;
		lastStyle = skin.FindStyle(Il2CppSystem.String.Concat(name, last)) ?? midStyle;
	}

	public static int CalcTotalHorizSpacing(int xCount, GUIStyle style, GUIStyle firstStyle, GUIStyle midStyle, GUIStyle lastStyle)
	{
		if (xCount < 2)
		{
			return 0;
		}
		if (xCount == 2)
		{
			return Mathf.Max(firstStyle.margin.right, lastStyle.margin.left);
		}
		int num = Mathf.Max(midStyle.margin.left, midStyle.margin.right);
		return Mathf.Max(firstStyle.margin.right, midStyle.margin.left) + Mathf.Max(midStyle.margin.right, lastStyle.margin.left) + num * (xCount - 3);
	}

	public static bool DoToggle(Rect position, int id, bool value, GUIContent content, GUIStyle style)
	{
		return DoControl(position, id, value, position.Contains(Event.current.mousePosition), content, style);
	}

	public static Il2CppStructArray<Rect> CalcGridRectsFixedWidthFixedMargin(Rect position, int itemCount, int itemsPerRow, float elemWidth, float elemHeight, float spacingHorizontal, float spacingVertical)
	{
		throw new System.NotSupportedException("Method unstripping failed");
	}

	public static int DoButtonGrid(Rect position, int selected, Il2CppReferenceArray<GUIContent> contents, Il2CppStringArray controlNames, int itemsPerRow, GUIStyle style, GUIStyle firstStyle, GUIStyle midStyle, GUIStyle lastStyle, ToolbarButtonSize buttonSize, [Optional] Il2CppStructArray<bool> contentsEnabled)
	{
		throw new System.NotSupportedException("Method unstripping failed");
	}

	public static Il2CppStructArray<Rect> CalcGridRects(Rect position, Il2CppReferenceArray<GUIContent> contents, int xCount, float elemWidth, float elemHeight, GUIStyle style, GUIStyle firstStyle, GUIStyle midStyle, GUIStyle lastStyle, ToolbarButtonSize buttonSize)
	{
		throw new System.NotSupportedException("Method unstripping failed");
	}

	public static float HorizontalSlider(Rect position, float value, float leftValue, float rightValue)
	{
		return Slider(position, value, 0f, leftValue, rightValue, skin.horizontalSlider, skin.horizontalSliderThumb, horiz: true, 0, skin.horizontalSliderThumbExtent);
	}

	public static float HorizontalSlider(Rect position, float value, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb)
	{
		return Slider(position, value, 0f, leftValue, rightValue, slider, thumb, horiz: true, 0);
	}

	public static float HorizontalSlider(Rect position, float value, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb, GUIStyle thumbExtent)
	{
		return Slider(position, value, 0f, leftValue, rightValue, slider, thumb, horiz: true, 0, (thumbExtent == null && thumb == skin.horizontalSliderThumb) ? skin.horizontalSliderThumbExtent : thumbExtent);
	}

	public static float VerticalSlider(Rect position, float value, float topValue, float bottomValue)
	{
		return Slider(position, value, 0f, topValue, bottomValue, skin.verticalSlider, skin.verticalSliderThumb, horiz: false, 0, skin.verticalSliderThumbExtent);
	}

	public static float VerticalSlider(Rect position, float value, float topValue, float bottomValue, GUIStyle slider, GUIStyle thumb)
	{
		return Slider(position, value, 0f, topValue, bottomValue, slider, thumb, horiz: false, 0);
	}

	public static float VerticalSlider(Rect position, float value, float topValue, float bottomValue, GUIStyle slider, GUIStyle thumb, GUIStyle thumbExtent)
	{
		return Slider(position, value, 0f, topValue, bottomValue, slider, thumb, horiz: false, 0, (thumbExtent == null && thumb == skin.verticalSliderThumb) ? skin.verticalSliderThumbExtent : thumbExtent);
	}

	public static float HorizontalScrollbar(Rect position, float value, float size, float leftValue, float rightValue)
	{
		return Scroller(position, value, size, leftValue, rightValue, skin.horizontalScrollbar, skin.horizontalScrollbarThumb, skin.horizontalScrollbarLeftButton, skin.horizontalScrollbarRightButton, horiz: true);
	}

	public static float VerticalScrollbar(Rect position, float value, float size, float topValue, float bottomValue)
	{
		return Scroller(position, value, size, topValue, bottomValue, skin.verticalScrollbar, skin.verticalScrollbarThumb, skin.verticalScrollbarUpButton, skin.verticalScrollbarDownButton, horiz: false);
	}

	public static void BeginClip(Rect position, Vector2 scrollOffset, Vector2 renderOffset, bool resetOffset)
	{
		GUIUtility.CheckOnGUI();
		GUIClip.Push(position, scrollOffset, renderOffset, resetOffset);
	}

	public static void BeginGroup(Rect position)
	{
		BeginGroup(position, GUIContent.none, GUIStyle.none);
	}

	public static void BeginGroup(Rect position, string text)
	{
		BeginGroup(position, GUIContent.Temp(text), GUIStyle.none);
	}

	public static void BeginGroup(Rect position, Texture image)
	{
		BeginGroup(position, GUIContent.Temp(image), GUIStyle.none);
	}

	public static void BeginGroup(Rect position, GUIContent content)
	{
		BeginGroup(position, content, GUIStyle.none);
	}

	public static void BeginGroup(Rect position, GUIStyle style)
	{
		BeginGroup(position, GUIContent.none, style);
	}

	public static void BeginGroup(Rect position, string text, GUIStyle style)
	{
		BeginGroup(position, GUIContent.Temp(text), style);
	}

	public static void BeginGroup(Rect position, Texture image, GUIStyle style)
	{
		BeginGroup(position, GUIContent.Temp(image), style);
	}

	public static void BeginClip(Rect position)
	{
		GUIUtility.CheckOnGUI();
		GUIClip.Push(position, Vector2.zero, Vector2.zero, resetOffset: false);
	}

	public static void EndClip()
	{
		GUIUtility.CheckOnGUI();
		GUIClip.Pop();
	}

	public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical)
	{
		return BeginScrollView(position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, skin.horizontalScrollbar, skin.verticalScrollbar, skin.scrollView);
	}

	public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar)
	{
		return BeginScrollView(position, scrollPosition, viewRect, alwaysShowHorizontal: false, alwaysShowVertical: false, horizontalScrollbar, verticalScrollbar, skin.scrollView);
	}

	public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar)
	{
		return BeginScrollView(position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, skin.scrollView);
	}

	public static Vector2 DoBeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background)
	{
		return BeginScrollView(position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, background);
	}

	public static ScrollViewState GetTopScrollView()
	{
		if (scrollViewStates.Count != 0)
		{
			return ((Il2CppObjectBase)scrollViewStates.Peek()).Cast<ScrollViewState>();
		}
		return null;
	}

	public static void ScrollTo(Rect position)
	{
		GetTopScrollView()?.ScrollTo(position);
	}

	public static bool ScrollTowards(Rect position, float maxDelta)
	{
		return GetTopScrollView()?.ScrollTowards(position, maxDelta) ?? false;
	}

	public static Rect Window(int id, Rect clientRect, WindowFunction func, string text)
	{
		GUIUtility.CheckOnGUI();
		return DoWindow(id, clientRect, func, GUIContent.Temp(text), skin.window, skin, forceRectOnLayout: true);
	}

	public static Rect Window(int id, Rect clientRect, WindowFunction func, Texture image)
	{
		GUIUtility.CheckOnGUI();
		return DoWindow(id, clientRect, func, GUIContent.Temp(image), skin.window, skin, forceRectOnLayout: true);
	}

	public static Rect Window(int id, Rect clientRect, WindowFunction func, GUIContent content)
	{
		GUIUtility.CheckOnGUI();
		return DoWindow(id, clientRect, func, content, skin.window, skin, forceRectOnLayout: true);
	}

	public static Rect Window(int id, Rect clientRect, WindowFunction func, string text, GUIStyle style)
	{
		GUIUtility.CheckOnGUI();
		return DoWindow(id, clientRect, func, GUIContent.Temp(text), style, skin, forceRectOnLayout: true);
	}

	public static Rect Window(int id, Rect clientRect, WindowFunction func, Texture image, GUIStyle style)
	{
		GUIUtility.CheckOnGUI();
		return DoWindow(id, clientRect, func, GUIContent.Temp(image), style, skin, forceRectOnLayout: true);
	}

	public static Rect Window(int id, Rect clientRect, WindowFunction func, GUIContent title, GUIStyle style)
	{
		GUIUtility.CheckOnGUI();
		return DoWindow(id, clientRect, func, title, style, skin, forceRectOnLayout: true);
	}

	public static Rect ModalWindow(int id, Rect clientRect, WindowFunction func, string text)
	{
		GUIUtility.CheckOnGUI();
		return DoModalWindow(id, clientRect, func, GUIContent.Temp(text), skin.window, skin);
	}

	public static Rect ModalWindow(int id, Rect clientRect, WindowFunction func, Texture image)
	{
		GUIUtility.CheckOnGUI();
		return DoModalWindow(id, clientRect, func, GUIContent.Temp(image), skin.window, skin);
	}

	public static Rect ModalWindow(int id, Rect clientRect, WindowFunction func, GUIContent content)
	{
		GUIUtility.CheckOnGUI();
		return DoModalWindow(id, clientRect, func, content, skin.window, skin);
	}

	public static Rect ModalWindow(int id, Rect clientRect, WindowFunction func, string text, GUIStyle style)
	{
		GUIUtility.CheckOnGUI();
		return DoModalWindow(id, clientRect, func, GUIContent.Temp(text), style, skin);
	}

	public static Rect ModalWindow(int id, Rect clientRect, WindowFunction func, Texture image, GUIStyle style)
	{
		GUIUtility.CheckOnGUI();
		return DoModalWindow(id, clientRect, func, GUIContent.Temp(image), style, skin);
	}

	public static Rect ModalWindow(int id, Rect clientRect, WindowFunction func, GUIContent content, GUIStyle style)
	{
		GUIUtility.CheckOnGUI();
		return DoModalWindow(id, clientRect, func, content, style, skin);
	}

	public static Rect DoWindow(int id, Rect clientRect, WindowFunction func, GUIContent title, GUIStyle style, GUISkin skin, bool forceRectOnLayout)
	{
		return Internal_DoWindow(id, GUIUtility.s_OriginalID, clientRect, func, title, style, skin, forceRectOnLayout);
	}

	public static Rect DoModalWindow(int id, Rect clientRect, WindowFunction func, GUIContent content, GUIStyle style, GUISkin skin)
	{
		return Internal_DoModalWindow(id, GUIUtility.s_OriginalID, clientRect, func, content, style, skin);
	}

	public static void DragWindow()
	{
		DragWindow(new Rect(0f, 0f, 10000f, 10000f));
	}

	public static void BeginWindows(int skinMode, int editorWindowInstanceID)
	{
		GUILayoutGroup topLevel = GUILayoutUtility.current.topLevel;
		GenericStack layoutGroups = GUILayoutUtility.current.layoutGroups;
		GUILayoutGroup windows = GUILayoutUtility.current.windows;
		Matrix4x4 matrix4x = matrix;
		Internal_BeginWindows();
		matrix = matrix4x;
		GUILayoutUtility.current.topLevel = topLevel;
		GUILayoutUtility.current.layoutGroups = layoutGroups;
		GUILayoutUtility.current.windows = windows;
	}

	public static void EndWindows()
	{
		GUILayoutGroup topLevel = GUILayoutUtility.current.topLevel;
		GenericStack layoutGroups = GUILayoutUtility.current.layoutGroups;
		GUILayoutGroup windows = GUILayoutUtility.current.windows;
		Internal_EndWindows();
		GUILayoutUtility.current.topLevel = topLevel;
		GUILayoutUtility.current.layoutGroups = layoutGroups;
		GUILayoutUtility.current.windows = windows;
	}

	public static System.IntPtr get_blendMaterial_Injected()
	{
		return get_blendMaterial_InjectedDelegateField();
	}

	public static System.IntPtr get_blitMaterial_Injected()
	{
		return get_blitMaterial_InjectedDelegateField();
	}

	public static System.IntPtr get_roundedRectMaterial_Injected()
	{
		return get_roundedRectMaterial_InjectedDelegateField();
	}

	public static System.IntPtr get_roundedRectWithColorPerBorderMaterial_Injected()
	{
		return get_roundedRectWithColorPerBorderMaterial_InjectedDelegateField();
	}

	public unsafe static void GetNameOfFocusedControl_Injected(out ManagedSpanWrapper ret)
	{
		GetNameOfFocusedControl_InjectedDelegateField((nint)Unsafe.AsPointer(ref ret));
	}

	public unsafe static void Internal_GetTooltip_Injected(out ManagedSpanWrapper ret)
	{
		Internal_GetTooltip_InjectedDelegateField((nint)Unsafe.AsPointer(ref ret));
	}

	public unsafe static void Internal_SetTooltip_Injected(ref ManagedSpanWrapper value)
	{
		Internal_SetTooltip_InjectedDelegateField((nint)Unsafe.AsPointer(ref value));
	}

	public unsafe static void Internal_GetMouseTooltip_Injected(out ManagedSpanWrapper ret)
	{
		Internal_GetMouseTooltip_InjectedDelegateField((nint)Unsafe.AsPointer(ref ret));
	}

	public unsafe static void Internal_DoModalWindow_Injected(int id, int instanceID, [In] ref Rect clientRect, WindowFunction func, GUIContent content, GUIStyle style, Il2CppSystem.Object skin, out Rect ret)
	{
		Internal_DoModalWindow_InjectedDelegateField(id, instanceID, (nint)Unsafe.AsPointer(ref clientRect), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)func), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)content), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)skin), (nint)Unsafe.AsPointer(ref ret));
	}

	public unsafe static void Internal_DoWindow_Injected(int id, int instanceID, [In] ref Rect clientRect, WindowFunction func, GUIContent title, GUIStyle style, Il2CppSystem.Object skin, bool forceRectOnLayout, out Rect ret)
	{
		Internal_DoWindow_InjectedDelegateField(id, instanceID, (nint)Unsafe.AsPointer(ref clientRect), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)func), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)title), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)style), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)skin), forceRectOnLayout, (nint)Unsafe.AsPointer(ref ret));
	}

	public unsafe static void DragWindow_Injected([In] ref Rect position)
	{
		DragWindow_InjectedDelegateField((nint)Unsafe.AsPointer(ref position));
	}

	public unsafe static void Internal_Concatenate_Injected(GUIContent first, GUIContent second, out ManagedSpanWrapper ret)
	{
		Internal_Concatenate_InjectedDelegateField(IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)first), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)second), (nint)Unsafe.AsPointer(ref ret));
	}
}
