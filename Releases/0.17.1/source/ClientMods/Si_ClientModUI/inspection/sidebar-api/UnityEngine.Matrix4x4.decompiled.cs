using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppSystem;

namespace UnityEngine;

[StructLayout(LayoutKind.Explicit)]
public struct Matrix4x4
{
	private delegate bool ValidTRSDelegate(System.IntPtr @this);

	private delegate bool CompareApproximately_InjectedDelegate([In] System.IntPtr a, [In] System.IntPtr b, float threshold);

	private static readonly System.IntPtr NativeFieldInfoPtr_m00;

	private static readonly System.IntPtr NativeFieldInfoPtr_m10;

	private static readonly System.IntPtr NativeFieldInfoPtr_m20;

	private static readonly System.IntPtr NativeFieldInfoPtr_m30;

	private static readonly System.IntPtr NativeFieldInfoPtr_m01;

	private static readonly System.IntPtr NativeFieldInfoPtr_m11;

	private static readonly System.IntPtr NativeFieldInfoPtr_m21;

	private static readonly System.IntPtr NativeFieldInfoPtr_m31;

	private static readonly System.IntPtr NativeFieldInfoPtr_m02;

	private static readonly System.IntPtr NativeFieldInfoPtr_m12;

	private static readonly System.IntPtr NativeFieldInfoPtr_m22;

	private static readonly System.IntPtr NativeFieldInfoPtr_m32;

	private static readonly System.IntPtr NativeFieldInfoPtr_m03;

	private static readonly System.IntPtr NativeFieldInfoPtr_m13;

	private static readonly System.IntPtr NativeFieldInfoPtr_m23;

	private static readonly System.IntPtr NativeFieldInfoPtr_m33;

	private static readonly System.IntPtr NativeFieldInfoPtr_zeroMatrix;

	private static readonly System.IntPtr NativeFieldInfoPtr_identityMatrix;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetRotation_Private_Quaternion_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetLossyScale_Private_Vector3_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsIdentity_Private_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetDeterminant_Private_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_DecomposeProjection_Private_FrustumPlanes_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_rotation_Public_get_Quaternion_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_lossyScale_Public_get_Vector3_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_isIdentity_Public_get_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_determinant_Public_get_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_decomposeProjection_Public_get_FrustumPlanes_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_TRS_Public_Static_Matrix4x4_Vector3_Quaternion_Vector3_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Inverse3DAffine_Public_Static_Boolean_Matrix4x4_byref_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Inverse_Public_Static_Matrix4x4_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_inverse_Public_get_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Transpose_Public_Static_Matrix4x4_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_transpose_Public_get_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Ortho_Public_Static_Matrix4x4_Single_Single_Single_Single_Single_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Perspective_Public_Static_Matrix4x4_Single_Single_Single_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_LookAt_Public_Static_Matrix4x4_Vector3_Vector3_Vector3_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Frustum_Public_Static_Matrix4x4_Single_Single_Single_Single_Single_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Frustum_Public_Static_Matrix4x4_FrustumPlanes_0;

	private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_Vector4_Vector4_Vector4_Vector4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_Item_Public_get_Single_Int32_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_Item_Public_set_Void_Int32_Int32_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_Item_Public_get_Single_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_Item_Public_set_Void_Int32_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetHashCode_Public_Virtual_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Equals_Public_Virtual_Boolean_Object_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Equals_Public_Virtual_Final_New_Boolean_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_op_Multiply_Public_Static_Matrix4x4_Matrix4x4_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_op_Multiply_Public_Static_Vector4_Matrix4x4_Vector4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_op_Equality_Public_Static_Boolean_Matrix4x4_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_op_Inequality_Public_Static_Boolean_Matrix4x4_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetColumn_Public_Vector4_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetRow_Public_Vector4_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetPosition_Public_Vector3_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetColumn_Public_Void_Int32_Vector4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetRow_Public_Void_Int32_Vector4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_MultiplyPoint_Public_Vector3_Vector3_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_MultiplyPoint3x4_Public_Vector3_Vector3_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_MultiplyVector_Public_Vector3_Vector3_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Scale_Public_Static_Matrix4x4_Vector3_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Translate_Public_Static_Matrix4x4_Vector3_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Rotate_Public_Static_Matrix4x4_Quaternion_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_zero_Public_Static_get_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_identity_Public_Static_get_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ToString_Public_Virtual_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ToString_Public_Virtual_Final_New_String_String_IFormatProvider_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetRotation_Injected_Private_Static_Void_byref_Matrix4x4_byref_Quaternion_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetLossyScale_Injected_Private_Static_Void_byref_Matrix4x4_byref_Vector3_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_DecomposeProjection_Injected_Private_Static_Void_byref_Matrix4x4_byref_FrustumPlanes_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_TRS_Injected_Private_Static_Void_byref_Vector3_byref_Quaternion_byref_Vector3_byref_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Inverse3DAffine_Injected_Private_Static_Boolean_byref_Matrix4x4_byref_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Inverse_Injected_Private_Static_Void_byref_Matrix4x4_byref_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Transpose_Injected_Private_Static_Void_byref_Matrix4x4_byref_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Ortho_Injected_Private_Static_Void_Single_Single_Single_Single_Single_Single_byref_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Perspective_Injected_Private_Static_Void_Single_Single_Single_Single_byref_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_LookAt_Injected_Private_Static_Void_byref_Vector3_byref_Vector3_byref_Vector3_byref_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Frustum_Injected_Private_Static_Void_Single_Single_Single_Single_Single_Single_byref_Matrix4x4_0;

	[FieldOffset(0)]
	public float m00;

	[FieldOffset(4)]
	public float m10;

	[FieldOffset(8)]
	public float m20;

	[FieldOffset(12)]
	public float m30;

	[FieldOffset(16)]
	public float m01;

	[FieldOffset(20)]
	public float m11;

	[FieldOffset(24)]
	public float m21;

	[FieldOffset(28)]
	public float m31;

	[FieldOffset(32)]
	public float m02;

	[FieldOffset(36)]
	public float m12;

	[FieldOffset(40)]
	public float m22;

	[FieldOffset(44)]
	public float m32;

	[FieldOffset(48)]
	public float m03;

	[FieldOffset(52)]
	public float m13;

	[FieldOffset(56)]
	public float m23;

	[FieldOffset(60)]
	public float m33;

	private static readonly ValidTRSDelegate ValidTRSDelegateField;

	private static readonly CompareApproximately_InjectedDelegate CompareApproximately_InjectedDelegateField;

	public unsafe static Matrix4x4 zeroMatrix
	{
		get
		{
			Unsafe.SkipInit(out Matrix4x4 result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_zeroMatrix, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_zeroMatrix, (void*)(&matrix4x));
		}
	}

	public unsafe static Matrix4x4 identityMatrix
	{
		get
		{
			Unsafe.SkipInit(out Matrix4x4 result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_identityMatrix, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_identityMatrix, (void*)(&matrix4x));
		}
	}

	public unsafe Quaternion rotation
	{
		[CallerCount(10)]
		[CachedScanResults(RefRangeStart = 1217338, RefRangeEnd = 1217348, XrefRangeStart = 1217336, XrefRangeEnd = 1217338, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_rotation_Public_get_Quaternion_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(Quaternion*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
	}

	public unsafe Vector3 lossyScale
	{
		[CallerCount(8)]
		[CachedScanResults(RefRangeStart = 1217350, RefRangeEnd = 1217358, XrefRangeStart = 1217348, XrefRangeEnd = 1217350, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_lossyScale_Public_get_Vector3_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(Vector3*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
	}

	public unsafe bool isIdentity
	{
		[CallerCount(1)]
		[CachedScanResults(RefRangeStart = 1217329, RefRangeEnd = 1217330, XrefRangeStart = 1217329, XrefRangeEnd = 1217330, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_isIdentity_Public_get_Boolean_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
	}

	public unsafe float determinant
	{
		[CallerCount(2)]
		[CachedScanResults(RefRangeStart = 1217332, RefRangeEnd = 1217334, XrefRangeStart = 1217332, XrefRangeEnd = 1217334, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_determinant_Public_get_Single_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
	}

	public unsafe FrustumPlanes decomposeProjection
	{
		[CallerCount(2)]
		[CachedScanResults(RefRangeStart = 1217360, RefRangeEnd = 1217362, XrefRangeStart = 1217358, XrefRangeEnd = 1217360, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_decomposeProjection_Public_get_FrustumPlanes_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(FrustumPlanes*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
	}

	public unsafe Matrix4x4 inverse
	{
		[CallerCount(40)]
		[CachedScanResults(RefRangeStart = 1217442, RefRangeEnd = 1217482, XrefRangeStart = 1217440, XrefRangeEnd = 1217442, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_inverse_Public_get_Matrix4x4_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
	}

	public unsafe Matrix4x4 transpose
	{
		[CallerCount(4)]
		[CachedScanResults(RefRangeStart = 1217487, RefRangeEnd = 1217491, XrefRangeStart = 1217485, XrefRangeEnd = 1217487, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_transpose_Public_get_Matrix4x4_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
	}

	public unsafe float this[int row, int column]
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1217519, XrefRangeEnd = 1217520, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[2];
			*ptr = (nint)(&row);
			*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &column;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_Item_Public_get_Single_Int32_Int32_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1217520, XrefRangeEnd = 1217521, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[3];
			*ptr = (nint)(&row);
			*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &column;
			*(float**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &value;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_Item_Public_set_Void_Int32_Int32_Single_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe float this[int index]
	{
		[CallerCount(84)]
		[CachedScanResults(RefRangeStart = 1217521, RefRangeEnd = 1217605, XrefRangeStart = 1217521, XrefRangeEnd = 1217521, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&index);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_Item_Public_get_Single_Int32_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(36)]
		[CachedScanResults(RefRangeStart = 1217605, RefRangeEnd = 1217641, XrefRangeStart = 1217605, XrefRangeEnd = 1217605, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[2];
			*ptr = (nint)(&index);
			*(float**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &value;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_Item_Public_set_Void_Int32_Single_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static Matrix4x4 zero
	{
		[CallerCount(3)]
		[CachedScanResults(RefRangeStart = 1218188, RefRangeEnd = 1218191, XrefRangeStart = 1218186, XrefRangeEnd = 1218188, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_zero_Public_Static_get_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
	}

	public unsafe static Matrix4x4 identity
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1218191, XrefRangeEnd = 1218193, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_identity_Public_Static_get_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
	}

	static Matrix4x4()
	{
		Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr = IL2CPP.GetIl2CppClass("UnityEngine.CoreModule.dll", "UnityEngine", "Matrix4x4");
		IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr);
		NativeFieldInfoPtr_m00 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m00");
		NativeFieldInfoPtr_m10 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m10");
		NativeFieldInfoPtr_m20 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m20");
		NativeFieldInfoPtr_m30 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m30");
		NativeFieldInfoPtr_m01 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m01");
		NativeFieldInfoPtr_m11 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m11");
		NativeFieldInfoPtr_m21 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m21");
		NativeFieldInfoPtr_m31 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m31");
		NativeFieldInfoPtr_m02 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m02");
		NativeFieldInfoPtr_m12 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m12");
		NativeFieldInfoPtr_m22 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m22");
		NativeFieldInfoPtr_m32 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m32");
		NativeFieldInfoPtr_m03 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m03");
		NativeFieldInfoPtr_m13 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m13");
		NativeFieldInfoPtr_m23 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m23");
		NativeFieldInfoPtr_m33 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "m33");
		NativeFieldInfoPtr_zeroMatrix = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "zeroMatrix");
		NativeFieldInfoPtr_identityMatrix = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, "identityMatrix");
		NativeMethodInfoPtr_GetRotation_Private_Quaternion_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666316);
		NativeMethodInfoPtr_GetLossyScale_Private_Vector3_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666317);
		NativeMethodInfoPtr_IsIdentity_Private_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666318);
		NativeMethodInfoPtr_GetDeterminant_Private_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666319);
		NativeMethodInfoPtr_DecomposeProjection_Private_FrustumPlanes_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666320);
		NativeMethodInfoPtr_get_rotation_Public_get_Quaternion_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666321);
		NativeMethodInfoPtr_get_lossyScale_Public_get_Vector3_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666322);
		NativeMethodInfoPtr_get_isIdentity_Public_get_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666323);
		NativeMethodInfoPtr_get_determinant_Public_get_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666324);
		NativeMethodInfoPtr_get_decomposeProjection_Public_get_FrustumPlanes_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666325);
		NativeMethodInfoPtr_TRS_Public_Static_Matrix4x4_Vector3_Quaternion_Vector3_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666326);
		NativeMethodInfoPtr_Inverse3DAffine_Public_Static_Boolean_Matrix4x4_byref_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666327);
		NativeMethodInfoPtr_Inverse_Public_Static_Matrix4x4_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666328);
		NativeMethodInfoPtr_get_inverse_Public_get_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666329);
		NativeMethodInfoPtr_Transpose_Public_Static_Matrix4x4_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666330);
		NativeMethodInfoPtr_get_transpose_Public_get_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666331);
		NativeMethodInfoPtr_Ortho_Public_Static_Matrix4x4_Single_Single_Single_Single_Single_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666332);
		NativeMethodInfoPtr_Perspective_Public_Static_Matrix4x4_Single_Single_Single_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666333);
		NativeMethodInfoPtr_LookAt_Public_Static_Matrix4x4_Vector3_Vector3_Vector3_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666334);
		NativeMethodInfoPtr_Frustum_Public_Static_Matrix4x4_Single_Single_Single_Single_Single_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666335);
		NativeMethodInfoPtr_Frustum_Public_Static_Matrix4x4_FrustumPlanes_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666336);
		NativeMethodInfoPtr__ctor_Public_Void_Vector4_Vector4_Vector4_Vector4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666337);
		NativeMethodInfoPtr_get_Item_Public_get_Single_Int32_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666338);
		NativeMethodInfoPtr_set_Item_Public_set_Void_Int32_Int32_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666339);
		NativeMethodInfoPtr_get_Item_Public_get_Single_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666340);
		NativeMethodInfoPtr_set_Item_Public_set_Void_Int32_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666341);
		NativeMethodInfoPtr_GetHashCode_Public_Virtual_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666342);
		NativeMethodInfoPtr_Equals_Public_Virtual_Boolean_Object_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666343);
		NativeMethodInfoPtr_Equals_Public_Virtual_Final_New_Boolean_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666344);
		NativeMethodInfoPtr_op_Multiply_Public_Static_Matrix4x4_Matrix4x4_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666345);
		NativeMethodInfoPtr_op_Multiply_Public_Static_Vector4_Matrix4x4_Vector4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666346);
		NativeMethodInfoPtr_op_Equality_Public_Static_Boolean_Matrix4x4_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666347);
		NativeMethodInfoPtr_op_Inequality_Public_Static_Boolean_Matrix4x4_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666348);
		NativeMethodInfoPtr_GetColumn_Public_Vector4_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666349);
		NativeMethodInfoPtr_GetRow_Public_Vector4_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666350);
		NativeMethodInfoPtr_GetPosition_Public_Vector3_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666351);
		NativeMethodInfoPtr_SetColumn_Public_Void_Int32_Vector4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666352);
		NativeMethodInfoPtr_SetRow_Public_Void_Int32_Vector4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666353);
		NativeMethodInfoPtr_MultiplyPoint_Public_Vector3_Vector3_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666354);
		NativeMethodInfoPtr_MultiplyPoint3x4_Public_Vector3_Vector3_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666355);
		NativeMethodInfoPtr_MultiplyVector_Public_Vector3_Vector3_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666356);
		NativeMethodInfoPtr_Scale_Public_Static_Matrix4x4_Vector3_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666357);
		NativeMethodInfoPtr_Translate_Public_Static_Matrix4x4_Vector3_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666358);
		NativeMethodInfoPtr_Rotate_Public_Static_Matrix4x4_Quaternion_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666359);
		NativeMethodInfoPtr_get_zero_Public_Static_get_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666360);
		NativeMethodInfoPtr_get_identity_Public_Static_get_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666361);
		NativeMethodInfoPtr_ToString_Public_Virtual_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666362);
		NativeMethodInfoPtr_ToString_Public_Virtual_Final_New_String_String_IFormatProvider_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666363);
		NativeMethodInfoPtr_GetRotation_Injected_Private_Static_Void_byref_Matrix4x4_byref_Quaternion_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666365);
		NativeMethodInfoPtr_GetLossyScale_Injected_Private_Static_Void_byref_Matrix4x4_byref_Vector3_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666366);
		NativeMethodInfoPtr_DecomposeProjection_Injected_Private_Static_Void_byref_Matrix4x4_byref_FrustumPlanes_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666367);
		NativeMethodInfoPtr_TRS_Injected_Private_Static_Void_byref_Vector3_byref_Quaternion_byref_Vector3_byref_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666368);
		NativeMethodInfoPtr_Inverse3DAffine_Injected_Private_Static_Boolean_byref_Matrix4x4_byref_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666369);
		NativeMethodInfoPtr_Inverse_Injected_Private_Static_Void_byref_Matrix4x4_byref_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666370);
		NativeMethodInfoPtr_Transpose_Injected_Private_Static_Void_byref_Matrix4x4_byref_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666371);
		NativeMethodInfoPtr_Ortho_Injected_Private_Static_Void_Single_Single_Single_Single_Single_Single_byref_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666372);
		NativeMethodInfoPtr_Perspective_Injected_Private_Static_Void_Single_Single_Single_Single_byref_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666373);
		NativeMethodInfoPtr_LookAt_Injected_Private_Static_Void_byref_Vector3_byref_Vector3_byref_Vector3_byref_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666374);
		NativeMethodInfoPtr_Frustum_Injected_Private_Static_Void_Single_Single_Single_Single_Single_Single_byref_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, 100666375);
		ValidTRSDelegateField = IL2CPP.ResolveICall<ValidTRSDelegate>("UnityEngine.Matrix4x4::ValidTRS");
		CompareApproximately_InjectedDelegateField = IL2CPP.ResolveICall<CompareApproximately_InjectedDelegate>("UnityEngine.Matrix4x4::CompareApproximately_Injected");
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1217323, XrefRangeEnd = 1217325, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Quaternion GetRotation()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetRotation_Private_Quaternion_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Quaternion*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1217325, XrefRangeEnd = 1217327, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Vector3 GetLossyScale()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetLossyScale_Private_Vector3_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector3*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1217329, RefRangeEnd = 1217330, XrefRangeStart = 1217327, XrefRangeEnd = 1217329, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsIdentity()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsIdentity_Private_Boolean_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 1217332, RefRangeEnd = 1217334, XrefRangeStart = 1217330, XrefRangeEnd = 1217332, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe float GetDeterminant()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetDeterminant_Private_Single_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1217334, XrefRangeEnd = 1217336, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe FrustumPlanes DecomposeProjection()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_DecomposeProjection_Private_FrustumPlanes_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(FrustumPlanes*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(69)]
	[CachedScanResults(RefRangeStart = 1217364, RefRangeEnd = 1217433, XrefRangeStart = 1217362, XrefRangeEnd = 1217364, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Matrix4x4 TRS(Vector3 pos, Quaternion q, Vector3 s)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&pos);
		*(Quaternion**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &q;
		*(Vector3**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &s;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_TRS_Public_Static_Matrix4x4_Vector3_Quaternion_Vector3_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 1217435, RefRangeEnd = 1217437, XrefRangeStart = 1217433, XrefRangeEnd = 1217435, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool Inverse3DAffine(Matrix4x4 input, ref Matrix4x4 result)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&input);
		*(void**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref result);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Inverse3DAffine_Public_Static_Boolean_Matrix4x4_byref_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1217439, RefRangeEnd = 1217440, XrefRangeStart = 1217437, XrefRangeEnd = 1217439, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Matrix4x4 Inverse(Matrix4x4 m)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&m);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Inverse_Public_Static_Matrix4x4_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 1217484, RefRangeEnd = 1217485, XrefRangeStart = 1217482, XrefRangeEnd = 1217484, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Matrix4x4 Transpose(Matrix4x4 m)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&m);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Transpose_Public_Static_Matrix4x4_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(5)]
	[CachedScanResults(RefRangeStart = 1217493, RefRangeEnd = 1217498, XrefRangeStart = 1217491, XrefRangeEnd = 1217493, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Matrix4x4 Ortho(float left, float right, float bottom, float top, float zNear, float zFar)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[6];
		*ptr = (nint)(&left);
		*(float**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &right;
		*(float**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &bottom;
		*(float**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &top;
		*(float**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &zNear;
		*(float**)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = &zFar;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Ortho_Public_Static_Matrix4x4_Single_Single_Single_Single_Single_Single_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 1217500, RefRangeEnd = 1217504, XrefRangeStart = 1217498, XrefRangeEnd = 1217500, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Matrix4x4 Perspective(float fov, float aspect, float zNear, float zFar)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = (nint)(&fov);
		*(float**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &aspect;
		*(float**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &zNear;
		*(float**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &zFar;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Perspective_Public_Static_Matrix4x4_Single_Single_Single_Single_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 1217506, RefRangeEnd = 1217510, XrefRangeStart = 1217504, XrefRangeEnd = 1217506, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Matrix4x4 LookAt(Vector3 from, Vector3 to, Vector3 up)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&from);
		*(Vector3**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &to;
		*(Vector3**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &up;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_LookAt_Public_Static_Matrix4x4_Vector3_Vector3_Vector3_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1217510, XrefRangeEnd = 1217512, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Matrix4x4 Frustum(float left, float right, float bottom, float top, float zNear, float zFar)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[6];
		*ptr = (nint)(&left);
		*(float**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &right;
		*(float**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &bottom;
		*(float**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &top;
		*(float**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &zNear;
		*(float**)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = &zFar;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Frustum_Public_Static_Matrix4x4_Single_Single_Single_Single_Single_Single_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1217512, XrefRangeEnd = 1217514, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Matrix4x4 Frustum(FrustumPlanes fp)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&fp);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Frustum_Public_Static_Matrix4x4_FrustumPlanes_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(5)]
	[CachedScanResults(RefRangeStart = 1217514, RefRangeEnd = 1217519, XrefRangeStart = 1217514, XrefRangeEnd = 1217514, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Matrix4x4(Vector4 column0, Vector4 column1, Vector4 column2, Vector4 column3)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = (nint)(&column0);
		*(Vector4**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &column1;
		*(Vector4**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &column2;
		*(Vector4**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &column3;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_Vector4_Vector4_Vector4_Vector4_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1217641, XrefRangeEnd = 1217657, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe override int GetHashCode()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetHashCode_Public_Virtual_Int32_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1217657, XrefRangeEnd = 1217661, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool Equals(Il2CppSystem.Object other)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)other);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Equals_Public_Virtual_Boolean_Object_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	public unsafe virtual bool Equals(Matrix4x4 other)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&other);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Equals_Public_Virtual_Final_New_Boolean_Matrix4x4_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(121)]
	[CachedScanResults(RefRangeStart = 1217661, RefRangeEnd = 1217782, XrefRangeStart = 1217661, XrefRangeEnd = 1217661, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Matrix4x4 operator *(Matrix4x4 lhs, Matrix4x4 rhs)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&lhs);
		*(Matrix4x4**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &rhs;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_op_Multiply_Public_Static_Matrix4x4_Matrix4x4_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(10)]
	[CachedScanResults(RefRangeStart = 1217782, RefRangeEnd = 1217792, XrefRangeStart = 1217782, XrefRangeEnd = 1217782, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Vector4 operator *(Matrix4x4 lhs, Vector4 vector)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&lhs);
		*(Vector4**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &vector;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_op_Multiply_Public_Static_Vector4_Matrix4x4_Vector4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(8)]
	[CachedScanResults(RefRangeStart = 1217792, RefRangeEnd = 1217800, XrefRangeStart = 1217792, XrefRangeEnd = 1217792, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool operator ==(Matrix4x4 lhs, Matrix4x4 rhs)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&lhs);
		*(Matrix4x4**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &rhs;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_op_Equality_Public_Static_Boolean_Matrix4x4_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1217800, XrefRangeEnd = 1217801, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool operator !=(Matrix4x4 lhs, Matrix4x4 rhs)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&lhs);
		*(Matrix4x4**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &rhs;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_op_Inequality_Public_Static_Boolean_Matrix4x4_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(73)]
	[CachedScanResults(RefRangeStart = 1217801, RefRangeEnd = 1217874, XrefRangeStart = 1217801, XrefRangeEnd = 1217801, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Vector4 GetColumn(int index)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&index);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetColumn_Public_Vector4_Int32_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(13)]
	[CachedScanResults(RefRangeStart = 1217877, RefRangeEnd = 1217890, XrefRangeStart = 1217874, XrefRangeEnd = 1217877, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Vector4 GetRow(int index)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&index);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetRow_Public_Vector4_Int32_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(8)]
	[CachedScanResults(RefRangeStart = 1217890, RefRangeEnd = 1217898, XrefRangeStart = 1217890, XrefRangeEnd = 1217890, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Vector3 GetPosition()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetPosition_Public_Vector3_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector3*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(18)]
	[CachedScanResults(RefRangeStart = 1217902, RefRangeEnd = 1217920, XrefRangeStart = 1217898, XrefRangeEnd = 1217902, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void SetColumn(int index, Vector4 column)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&index);
		*(Vector4**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &column;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetColumn_Public_Void_Int32_Vector4_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(10)]
	[CachedScanResults(RefRangeStart = 1217924, RefRangeEnd = 1217934, XrefRangeStart = 1217920, XrefRangeEnd = 1217924, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void SetRow(int index, Vector4 row)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&index);
		*(Vector4**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &row;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetRow_Public_Void_Int32_Vector4_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(67)]
	[CachedScanResults(RefRangeStart = 1217934, RefRangeEnd = 1218001, XrefRangeStart = 1217934, XrefRangeEnd = 1217934, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Vector3 MultiplyPoint(Vector3 point)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&point);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_MultiplyPoint_Public_Vector3_Vector3_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector3*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(83)]
	[CachedScanResults(RefRangeStart = 1218001, RefRangeEnd = 1218084, XrefRangeStart = 1218001, XrefRangeEnd = 1218001, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Vector3 MultiplyPoint3x4(Vector3 point)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&point);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_MultiplyPoint3x4_Public_Vector3_Vector3_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector3*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(31)]
	[CachedScanResults(RefRangeStart = 1218084, RefRangeEnd = 1218115, XrefRangeStart = 1218084, XrefRangeEnd = 1218084, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Vector3 MultiplyVector(Vector3 vector)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&vector);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_MultiplyVector_Public_Vector3_Vector3_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector3*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(20)]
	[CachedScanResults(RefRangeStart = 1218115, RefRangeEnd = 1218135, XrefRangeStart = 1218115, XrefRangeEnd = 1218115, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Matrix4x4 Scale(Vector3 vector)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&vector);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Scale_Public_Static_Matrix4x4_Vector3_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(32)]
	[CachedScanResults(RefRangeStart = 1218135, RefRangeEnd = 1218167, XrefRangeStart = 1218135, XrefRangeEnd = 1218135, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Matrix4x4 Translate(Vector3 vector)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&vector);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Translate_Public_Static_Matrix4x4_Vector3_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(19)]
	[CachedScanResults(RefRangeStart = 1218167, RefRangeEnd = 1218186, XrefRangeStart = 1218167, XrefRangeEnd = 1218167, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Matrix4x4 Rotate(Quaternion q)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&q);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Rotate_Public_Static_Matrix4x4_Quaternion_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1218193, XrefRangeEnd = 1218194, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe override string ToString()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ToString_Public_Virtual_String_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return IL2CPP.Il2CppStringToManaged(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1218194, XrefRangeEnd = 1218289, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe virtual string ToString(string format, Il2CppSystem.IFormatProvider formatProvider)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(format);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)formatProvider);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ToString_Public_Virtual_Final_New_String_String_IFormatProvider_0, (System.IntPtr)(nint)Unsafe.AsPointer(ref this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return IL2CPP.Il2CppStringToManaged(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1218289, XrefRangeEnd = 1218291, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void GetRotation_Injected(ref Matrix4x4 _unity_self, out Quaternion ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)Unsafe.AsPointer(ref _unity_self);
		*(void**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetRotation_Injected_Private_Static_Void_byref_Matrix4x4_byref_Quaternion_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1218291, XrefRangeEnd = 1218293, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void GetLossyScale_Injected(ref Matrix4x4 _unity_self, out Vector3 ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)Unsafe.AsPointer(ref _unity_self);
		*(void**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetLossyScale_Injected_Private_Static_Void_byref_Matrix4x4_byref_Vector3_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1218293, XrefRangeEnd = 1218295, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void DecomposeProjection_Injected(ref Matrix4x4 _unity_self, out FrustumPlanes ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)Unsafe.AsPointer(ref _unity_self);
		*(void**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_DecomposeProjection_Injected_Private_Static_Void_byref_Matrix4x4_byref_FrustumPlanes_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1218295, XrefRangeEnd = 1218297, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void TRS_Injected([In] ref Vector3 pos, [In] ref Quaternion q, [In] ref Vector3 s, out Matrix4x4 ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = (nint)Unsafe.AsPointer(ref pos);
		*(void**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref q);
		*(void**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref s);
		*(void**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_TRS_Injected_Private_Static_Void_byref_Vector3_byref_Quaternion_byref_Vector3_byref_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1218297, XrefRangeEnd = 1218299, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool Inverse3DAffine_Injected([In] ref Matrix4x4 input, ref Matrix4x4 result)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)Unsafe.AsPointer(ref input);
		*(void**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref result);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Inverse3DAffine_Injected_Private_Static_Boolean_byref_Matrix4x4_byref_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1218299, XrefRangeEnd = 1218301, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Inverse_Injected([In] ref Matrix4x4 m, out Matrix4x4 ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)Unsafe.AsPointer(ref m);
		*(void**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Inverse_Injected_Private_Static_Void_byref_Matrix4x4_byref_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1218301, XrefRangeEnd = 1218303, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Transpose_Injected([In] ref Matrix4x4 m, out Matrix4x4 ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)Unsafe.AsPointer(ref m);
		*(void**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Transpose_Injected_Private_Static_Void_byref_Matrix4x4_byref_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1218303, XrefRangeEnd = 1218305, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Ortho_Injected(float left, float right, float bottom, float top, float zNear, float zFar, out Matrix4x4 ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[7];
		*ptr = (nint)(&left);
		*(float**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &right;
		*(float**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &bottom;
		*(float**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &top;
		*(float**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &zNear;
		*(float**)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = &zFar;
		*(void**)((byte*)ptr + checked((nuint)6u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Ortho_Injected_Private_Static_Void_Single_Single_Single_Single_Single_Single_byref_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1218305, XrefRangeEnd = 1218307, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Perspective_Injected(float fov, float aspect, float zNear, float zFar, out Matrix4x4 ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[5];
		*ptr = (nint)(&fov);
		*(float**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &aspect;
		*(float**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &zNear;
		*(float**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &zFar;
		*(void**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Perspective_Injected_Private_Static_Void_Single_Single_Single_Single_byref_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1218307, XrefRangeEnd = 1218309, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void LookAt_Injected([In] ref Vector3 from, [In] ref Vector3 to, [In] ref Vector3 up, out Matrix4x4 ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = (nint)Unsafe.AsPointer(ref from);
		*(void**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref to);
		*(void**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref up);
		*(void**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_LookAt_Injected_Private_Static_Void_byref_Vector3_byref_Vector3_byref_Vector3_byref_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 1218309, XrefRangeEnd = 1218311, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Frustum_Injected(float left, float right, float bottom, float top, float zNear, float zFar, out Matrix4x4 ret)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[7];
		*ptr = (nint)(&left);
		*(float**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &right;
		*(float**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &bottom;
		*(float**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &top;
		*(float**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &zNear;
		*(float**)((byte*)ptr + checked((nuint)5u * unchecked((nuint)sizeof(System.IntPtr)))) = &zFar;
		*(void**)((byte*)ptr + checked((nuint)6u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref ret);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Frustum_Injected_Private_Static_Void_Single_Single_Single_Single_Single_Single_byref_Matrix4x4_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	public unsafe Il2CppSystem.Object BoxIl2CppObject()
	{
		return new Il2CppSystem.Object(IL2CPP.il2cpp_value_box(Il2CppClassPointerStore<Matrix4x4>.NativeClassPtr, (System.IntPtr)(nint)Unsafe.AsPointer(ref this)));
	}

	public unsafe bool ValidTRS()
	{
		//IL_000b: Expected Obj, but got Ref
		return ValidTRSDelegateField(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)Unsafe.AsPointer(ref this)));
	}

	public static float Determinant(Matrix4x4 m)
	{
		return m.determinant;
	}

	public void SetTRS(Vector3 pos, Quaternion q, Vector3 s)
	{
		this = TRS(pos, q, s);
	}

	public static bool CompareApproximately(Matrix4x4 a, Matrix4x4 b, float threshold)
	{
		return CompareApproximately_Injected(ref a, ref b, threshold);
	}

	public Plane TransformPlane(Plane plane)
	{
		Matrix4x4 matrix4x = inverse;
		float x = plane.normal.x;
		float y = plane.normal.y;
		float z = plane.normal.z;
		float distance = plane.distance;
		float x2 = matrix4x.m00 * x + matrix4x.m10 * y + matrix4x.m20 * z + matrix4x.m30 * distance;
		float y2 = matrix4x.m01 * x + matrix4x.m11 * y + matrix4x.m21 * z + matrix4x.m31 * distance;
		float z2 = matrix4x.m02 * x + matrix4x.m12 * y + matrix4x.m22 * z + matrix4x.m32 * distance;
		float d = matrix4x.m03 * x + matrix4x.m13 * y + matrix4x.m23 * z + matrix4x.m33 * distance;
		return new Plane(new Vector3(x2, y2, z2), d);
	}

	public string ToString(string format)
	{
		return ToString(format, null);
	}

	public unsafe static bool CompareApproximately_Injected([In] ref Matrix4x4 a, [In] ref Matrix4x4 b, float threshold)
	{
		return CompareApproximately_InjectedDelegateField((nint)Unsafe.AsPointer(ref a), (nint)Unsafe.AsPointer(ref b), threshold);
	}
}
