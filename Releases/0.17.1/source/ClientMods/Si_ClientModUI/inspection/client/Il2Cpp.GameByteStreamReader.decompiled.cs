using System;
using System.Runtime.CompilerServices;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem;
using Il2CppSystem.IO;
using UnityEngine;

namespace Il2Cpp;

public class GameByteStreamReader : Il2CppSystem.Object
{
	private static readonly System.IntPtr NativeFieldInfoPtr_m_MemoryStream;

	private static readonly System.IntPtr NativeFieldInfoPtr_m_BinaryReader;

	private static readonly System.IntPtr NativeFieldInfoPtr_m_ReadDebug;

	private static readonly System.IntPtr NativeFieldInfoPtr_ForNetID;

	private static readonly System.IntPtr NativeFieldInfoPtr_ForFunction;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_Instance;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetGameByteStreamReader_Public_Static_GameByteStreamReader_Il2CppStructArray_1_Byte_Int32_UInt32_String_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadFromFile_Public_Static_GameByteStreamReader_Stream_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_FromRawByteArray_Public_Static_GameByteStreamReader_Il2CppStructArray_1_Byte_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ToRawByteArray_Public_Static_Il2CppStructArray_1_Byte_GameByteStreamReader_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetDataSize_Public_Int64_0;

	private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_WriteTo_Public_Void_GameByteStreamWriter_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Finalize_Protected_Virtual_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetNetID_Public_Void_UInt32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetFunction_Public_Void_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetLogName_Public_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Reset_Public_Void_Il2CppStructArray_1_Byte_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ResetToStart_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadFloatNonAlloc_Private_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadDoubleNonAlloc_Private_Double_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadType_Public_Void_EGameByteStreamType_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadByteArray_Public_Int32_Il2CppStructArray_1_Byte_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadLargeByteArray_Public_Int32_Il2CppStructArray_1_Byte_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadLargeBoolArray_Public_Il2CppStructArray_1_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadBool_Public_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadByte_Public_Byte_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadSByte_Public_SByte_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadInt16_Public_Int16_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadUInt16_Public_UInt16_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadInt32_Public_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadUInt32_Public_UInt32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadPackedUInt32_Public_UInt32_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadInt64_Public_Int64_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadUInt64_Public_UInt64_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadUInt64Array_Public_Int32_Il2CppStructArray_1_UInt64_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadPackedUInt64_Public_UInt64_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadInputAxis_Public_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadFloatAsByte_Public_Single_Single_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadFloatAsUShort_Public_Single_Single_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadFloat_Public_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadDouble_Public_Double_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadDecimal_Public_Decimal_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadChar_Public_Char_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadString_Public_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadQuaternion_Public_Quaternion_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadPackedQuaternion_Public_Quaternion_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadVector2_Public_Vector2_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadVector3_Public_Vector3_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadPackedVector3_Public_Vector3_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadPackedPosition_Public_Vector3_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadVector4_Public_Vector4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadColor_Public_Color_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadColor32_Public_Color32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadRect_Public_Rect_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadPlane_Public_Plane_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadRay_Public_Ray_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadMatrix4x4_Public_Matrix4x4_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadType_Public_Type_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadTeam_Public_Team_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadBaseGameObject_Public_BaseGameObject_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ReadNetworkID_Public_NetworkID_0;

	public unsafe MemoryStream m_MemoryStream
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_m_MemoryStream);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<MemoryStream>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_m_MemoryStream)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)memoryStream));
		}
	}

	public unsafe BinaryReader m_BinaryReader
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_m_BinaryReader);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<BinaryReader>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_m_BinaryReader)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)binaryReader));
		}
	}

	public unsafe bool m_ReadDebug
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_m_ReadDebug);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_m_ReadDebug)) = flag;
		}
	}

	public unsafe uint ForNetID
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_ForNetID);
			return *(uint*)num;
		}
		set
		{
			*(uint*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_ForNetID)) = num;
		}
	}

	public unsafe string ForFunction
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_ForFunction);
			return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_ForFunction)), IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe static GameByteStreamReader s_Instance
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_Instance, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<GameByteStreamReader>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_Instance, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gameByteStreamReader));
		}
	}

	static GameByteStreamReader()
	{
		Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr = IL2CPP.GetIl2CppClass("SilicaCore.dll", "", "GameByteStreamReader");
		IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr);
		NativeFieldInfoPtr_m_MemoryStream = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, "m_MemoryStream");
		NativeFieldInfoPtr_m_BinaryReader = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, "m_BinaryReader");
		NativeFieldInfoPtr_m_ReadDebug = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, "m_ReadDebug");
		NativeFieldInfoPtr_ForNetID = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, "ForNetID");
		NativeFieldInfoPtr_ForFunction = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, "ForFunction");
		NativeFieldInfoPtr_s_Instance = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, "s_Instance");
		NativeMethodInfoPtr_GetGameByteStreamReader_Public_Static_GameByteStreamReader_Il2CppStructArray_1_Byte_Int32_UInt32_String_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670639);
		NativeMethodInfoPtr_ReadFromFile_Public_Static_GameByteStreamReader_Stream_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670640);
		NativeMethodInfoPtr_FromRawByteArray_Public_Static_GameByteStreamReader_Il2CppStructArray_1_Byte_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670641);
		NativeMethodInfoPtr_ToRawByteArray_Public_Static_Il2CppStructArray_1_Byte_GameByteStreamReader_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670642);
		NativeMethodInfoPtr_GetDataSize_Public_Int64_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670643);
		NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670644);
		NativeMethodInfoPtr_WriteTo_Public_Void_GameByteStreamWriter_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670645);
		NativeMethodInfoPtr_Finalize_Protected_Virtual_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670646);
		NativeMethodInfoPtr_SetNetID_Public_Void_UInt32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670647);
		NativeMethodInfoPtr_SetFunction_Public_Void_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670648);
		NativeMethodInfoPtr_GetLogName_Public_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670649);
		NativeMethodInfoPtr_Reset_Public_Void_Il2CppStructArray_1_Byte_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670650);
		NativeMethodInfoPtr_ResetToStart_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670651);
		NativeMethodInfoPtr_ReadFloatNonAlloc_Private_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670652);
		NativeMethodInfoPtr_ReadDoubleNonAlloc_Private_Double_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670653);
		NativeMethodInfoPtr_ReadType_Public_Void_EGameByteStreamType_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670654);
		NativeMethodInfoPtr_ReadByteArray_Public_Int32_Il2CppStructArray_1_Byte_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670655);
		NativeMethodInfoPtr_ReadLargeByteArray_Public_Int32_Il2CppStructArray_1_Byte_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670656);
		NativeMethodInfoPtr_ReadLargeBoolArray_Public_Il2CppStructArray_1_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670657);
		NativeMethodInfoPtr_ReadBool_Public_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670658);
		NativeMethodInfoPtr_ReadByte_Public_Byte_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670659);
		NativeMethodInfoPtr_ReadSByte_Public_SByte_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670660);
		NativeMethodInfoPtr_ReadInt16_Public_Int16_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670661);
		NativeMethodInfoPtr_ReadUInt16_Public_UInt16_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670662);
		NativeMethodInfoPtr_ReadInt32_Public_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670663);
		NativeMethodInfoPtr_ReadUInt32_Public_UInt32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670664);
		NativeMethodInfoPtr_ReadPackedUInt32_Public_UInt32_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670665);
		NativeMethodInfoPtr_ReadInt64_Public_Int64_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670666);
		NativeMethodInfoPtr_ReadUInt64_Public_UInt64_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670667);
		NativeMethodInfoPtr_ReadUInt64Array_Public_Int32_Il2CppStructArray_1_UInt64_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670668);
		NativeMethodInfoPtr_ReadPackedUInt64_Public_UInt64_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670669);
		NativeMethodInfoPtr_ReadInputAxis_Public_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670670);
		NativeMethodInfoPtr_ReadFloatAsByte_Public_Single_Single_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670671);
		NativeMethodInfoPtr_ReadFloatAsUShort_Public_Single_Single_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670672);
		NativeMethodInfoPtr_ReadFloat_Public_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670673);
		NativeMethodInfoPtr_ReadDouble_Public_Double_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670674);
		NativeMethodInfoPtr_ReadDecimal_Public_Decimal_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670675);
		NativeMethodInfoPtr_ReadChar_Public_Char_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670676);
		NativeMethodInfoPtr_ReadString_Public_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670677);
		NativeMethodInfoPtr_ReadQuaternion_Public_Quaternion_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670678);
		NativeMethodInfoPtr_ReadPackedQuaternion_Public_Quaternion_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670679);
		NativeMethodInfoPtr_ReadVector2_Public_Vector2_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670680);
		NativeMethodInfoPtr_ReadVector3_Public_Vector3_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670681);
		NativeMethodInfoPtr_ReadPackedVector3_Public_Vector3_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670682);
		NativeMethodInfoPtr_ReadPackedPosition_Public_Vector3_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670683);
		NativeMethodInfoPtr_ReadVector4_Public_Vector4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670684);
		NativeMethodInfoPtr_ReadColor_Public_Color_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670685);
		NativeMethodInfoPtr_ReadColor32_Public_Color32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670686);
		NativeMethodInfoPtr_ReadRect_Public_Rect_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670687);
		NativeMethodInfoPtr_ReadPlane_Public_Plane_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670688);
		NativeMethodInfoPtr_ReadRay_Public_Ray_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670689);
		NativeMethodInfoPtr_ReadMatrix4x4_Public_Matrix4x4_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670690);
		NativeMethodInfoPtr_ReadType_Public_Type_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670691);
		NativeMethodInfoPtr_ReadTeam_Public_Team_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670692);
		NativeMethodInfoPtr_ReadBaseGameObject_Public_BaseGameObject_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670693);
		NativeMethodInfoPtr_ReadNetworkID_Public_NetworkID_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr, 100670694);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 468716, RefRangeEnd = 468718, XrefRangeStart = 468706, XrefRangeEnd = 468716, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static GameByteStreamReader GetGameByteStreamReader(Il2CppStructArray<byte> data, int dataSizeBytes, uint netID, string funcName, bool readDebug = true)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[5];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)data);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &dataSizeBytes;
		*(uint**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &netID;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(funcName);
		*(bool**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &readDebug;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetGameByteStreamReader_Public_Static_GameByteStreamReader_Il2CppStructArray_1_Byte_Int32_UInt32_String_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<GameByteStreamReader>(intPtr) : null;
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 468725, RefRangeEnd = 468729, XrefRangeStart = 468718, XrefRangeEnd = 468725, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static GameByteStreamReader ReadFromFile(Stream stream)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)stream);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadFromFile_Public_Static_GameByteStreamReader_Stream_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<GameByteStreamReader>(intPtr) : null;
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 468734, RefRangeEnd = 468737, XrefRangeStart = 468729, XrefRangeEnd = 468734, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static GameByteStreamReader FromRawByteArray(Il2CppStructArray<byte> data)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)data);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_FromRawByteArray_Public_Static_GameByteStreamReader_Il2CppStructArray_1_Byte_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<GameByteStreamReader>(intPtr) : null;
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 468737, XrefRangeEnd = 468754, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static Il2CppStructArray<byte> ToRawByteArray(GameByteStreamReader dataReader)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dataReader);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ToRawByteArray_Public_Static_Il2CppStructArray_1_Byte_GameByteStreamReader_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppStructArray<byte>>(intPtr) : null;
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 468754, XrefRangeEnd = 468755, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe long GetDataSize()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetDataSize_Public_Int64_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(long*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 468768, RefRangeEnd = 468771, XrefRangeStart = 468755, XrefRangeEnd = 468768, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe GameByteStreamReader()
		: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<GameByteStreamReader>.NativeClassPtr))
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 468771, XrefRangeEnd = 468780, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void WriteTo(GameByteStreamWriter writer)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)writer);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_WriteTo_Public_Void_GameByteStreamWriter_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 468780, XrefRangeEnd = 468785, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public new unsafe void Finalize()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(IL2CPP.il2cpp_object_get_virtual_method(IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)this), NativeMethodInfoPtr_Finalize_Protected_Virtual_Void_0), IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(13)]
	[CachedScanResults(RefRangeStart = 381, RefRangeEnd = 394, XrefRangeStart = 381, XrefRangeEnd = 394, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void SetNetID(uint netID)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&netID);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetNetID_Public_Void_UInt32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 0, XrefRangeEnd = 0, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void SetFunction(string funcName)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(funcName);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetFunction_Public_Void_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 468881, RefRangeEnd = 468882, XrefRangeStart = 468785, XrefRangeEnd = 468881, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe string GetLogName()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetLogName_Public_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return IL2CPP.Il2CppStringToManaged(intPtr);
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 468883, RefRangeEnd = 468887, XrefRangeStart = 468882, XrefRangeEnd = 468883, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void Reset(Il2CppStructArray<byte> data, int dataSizeBytes)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)data);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &dataSizeBytes;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Reset_Public_Void_Il2CppStructArray_1_Byte_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 468888, RefRangeEnd = 468889, XrefRangeStart = 468887, XrefRangeEnd = 468888, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void ResetToStart()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ResetToStart_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(96)]
	[CachedScanResults(RefRangeStart = 468892, RefRangeEnd = 468988, XrefRangeStart = 468889, XrefRangeEnd = 468892, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe float ReadFloatNonAlloc()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadFloatNonAlloc_Private_Single_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 468988, XrefRangeEnd = 468991, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe double ReadDoubleNonAlloc()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadDoubleNonAlloc_Private_Double_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(double*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(68)]
	[CachedScanResults(RefRangeStart = 469014, RefRangeEnd = 469082, XrefRangeStart = 468991, XrefRangeEnd = 469014, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void ReadType(EGameByteStreamType expectedDataType)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&expectedDataType);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadType_Public_Void_EGameByteStreamType_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 469083, RefRangeEnd = 469087, XrefRangeStart = 469082, XrefRangeEnd = 469083, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe int ReadByteArray(Il2CppStructArray<byte> byteData)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)byteData);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadByteArray_Public_Int32_Il2CppStructArray_1_Byte_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 469088, RefRangeEnd = 469089, XrefRangeStart = 469087, XrefRangeEnd = 469088, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe int ReadLargeByteArray(Il2CppStructArray<byte> byteData)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)byteData);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadLargeByteArray_Public_Int32_Il2CppStructArray_1_Byte_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 469094, RefRangeEnd = 469095, XrefRangeStart = 469089, XrefRangeEnd = 469094, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Il2CppStructArray<bool> ReadLargeBoolArray()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadLargeBoolArray_Public_Il2CppStructArray_1_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppStructArray<bool>>(intPtr) : null;
	}

	[CallerCount(93)]
	[CachedScanResults(RefRangeStart = 469097, RefRangeEnd = 469190, XrefRangeStart = 469095, XrefRangeEnd = 469097, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool ReadBool()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadBool_Public_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(124)]
	[CachedScanResults(RefRangeStart = 469192, RefRangeEnd = 469316, XrefRangeStart = 469190, XrefRangeEnd = 469192, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe byte ReadByte()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadByte_Public_Byte_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(byte*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 469318, RefRangeEnd = 469321, XrefRangeStart = 469316, XrefRangeEnd = 469318, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe sbyte ReadSByte()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadSByte_Public_SByte_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(sbyte*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(10)]
	[CachedScanResults(RefRangeStart = 469323, RefRangeEnd = 469333, XrefRangeStart = 469321, XrefRangeEnd = 469323, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe short ReadInt16()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadInt16_Public_Int16_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(short*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(10)]
	[CachedScanResults(RefRangeStart = 469335, RefRangeEnd = 469345, XrefRangeStart = 469333, XrefRangeEnd = 469335, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe ushort ReadUInt16()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadUInt16_Public_UInt16_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(ushort*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(34)]
	[CachedScanResults(RefRangeStart = 469347, RefRangeEnd = 469381, XrefRangeStart = 469345, XrefRangeEnd = 469347, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe int ReadInt32()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadInt32_Public_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(12)]
	[CachedScanResults(RefRangeStart = 469383, RefRangeEnd = 469395, XrefRangeStart = 469381, XrefRangeEnd = 469383, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe uint ReadUInt32()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadUInt32_Public_UInt32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(uint*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(92)]
	[CachedScanResults(RefRangeStart = 469396, RefRangeEnd = 469488, XrefRangeStart = 469395, XrefRangeEnd = 469396, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe uint ReadPackedUInt32(bool isBGO = false)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&isBGO);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadPackedUInt32_Public_UInt32_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(uint*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 469490, RefRangeEnd = 469493, XrefRangeStart = 469488, XrefRangeEnd = 469490, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe long ReadInt64()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadInt64_Public_Int64_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(long*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(41)]
	[CachedScanResults(RefRangeStart = 469495, RefRangeEnd = 469536, XrefRangeStart = 469493, XrefRangeEnd = 469495, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe ulong ReadUInt64()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadUInt64_Public_UInt64_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(ulong*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 469537, RefRangeEnd = 469538, XrefRangeStart = 469536, XrefRangeEnd = 469537, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe int ReadUInt64Array(Il2CppStructArray<ulong> ids)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)ids);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadUInt64Array_Public_Int32_Il2CppStructArray_1_UInt64_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 469543, RefRangeEnd = 469544, XrefRangeStart = 469538, XrefRangeEnd = 469543, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe ulong ReadPackedUInt64()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadPackedUInt64_Public_UInt64_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(ulong*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(25)]
	[CachedScanResults(RefRangeStart = 469545, RefRangeEnd = 469570, XrefRangeStart = 469544, XrefRangeEnd = 469545, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe float ReadInputAxis()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadInputAxis_Public_Single_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(37)]
	[CachedScanResults(RefRangeStart = 469571, RefRangeEnd = 469608, XrefRangeStart = 469570, XrefRangeEnd = 469571, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe float ReadFloatAsByte(float min, float max)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&min);
		*(float**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &max;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadFloatAsByte_Public_Single_Single_Single_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 469609, RefRangeEnd = 469613, XrefRangeStart = 469608, XrefRangeEnd = 469609, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe float ReadFloatAsUShort(float min, float max)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&min);
		*(float**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &max;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadFloatAsUShort_Public_Single_Single_Single_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(36)]
	[CachedScanResults(RefRangeStart = 469615, RefRangeEnd = 469651, XrefRangeStart = 469613, XrefRangeEnd = 469615, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe float ReadFloat()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadFloat_Public_Single_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 469655, RefRangeEnd = 469656, XrefRangeStart = 469651, XrefRangeEnd = 469655, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe double ReadDouble()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadDouble_Public_Double_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(double*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 469657, RefRangeEnd = 469658, XrefRangeStart = 469656, XrefRangeEnd = 469657, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Il2CppSystem.Decimal ReadDecimal()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadDecimal_Public_Decimal_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Il2CppSystem.Decimal*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 469660, RefRangeEnd = 469661, XrefRangeStart = 469658, XrefRangeEnd = 469660, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe char ReadChar()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadChar_Public_Char_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(char*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(33)]
	[CachedScanResults(RefRangeStart = 469663, RefRangeEnd = 469696, XrefRangeStart = 469661, XrefRangeEnd = 469663, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe string ReadString()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadString_Public_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return IL2CPP.Il2CppStringToManaged(intPtr);
	}

	[CallerCount(9)]
	[CachedScanResults(RefRangeStart = 469701, RefRangeEnd = 469710, XrefRangeStart = 469696, XrefRangeEnd = 469701, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Quaternion ReadQuaternion()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadQuaternion_Public_Quaternion_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Quaternion*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(9)]
	[CachedScanResults(RefRangeStart = 469719, RefRangeEnd = 469728, XrefRangeStart = 469710, XrefRangeEnd = 469719, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Quaternion ReadPackedQuaternion()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadPackedQuaternion_Public_Quaternion_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Quaternion*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 469731, RefRangeEnd = 469733, XrefRangeStart = 469728, XrefRangeEnd = 469731, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Vector2 ReadVector2()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadVector2_Public_Vector2_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector2*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(59)]
	[CachedScanResults(RefRangeStart = 469737, RefRangeEnd = 469796, XrefRangeStart = 469733, XrefRangeEnd = 469737, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Vector3 ReadVector3()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadVector3_Public_Vector3_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector3*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 469799, RefRangeEnd = 469802, XrefRangeStart = 469796, XrefRangeEnd = 469799, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Vector3 ReadPackedVector3(float maxLength)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&maxLength);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadPackedVector3_Public_Vector3_Single_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector3*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 469802, XrefRangeEnd = 469805, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Vector3 ReadPackedPosition()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadPackedPosition_Public_Vector3_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector3*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 469810, RefRangeEnd = 469811, XrefRangeStart = 469805, XrefRangeEnd = 469810, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Vector4 ReadVector4()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadVector4_Public_Vector4_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Vector4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 469816, RefRangeEnd = 469817, XrefRangeStart = 469811, XrefRangeEnd = 469816, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Color ReadColor()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadColor_Public_Color_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Color*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 469818, RefRangeEnd = 469819, XrefRangeStart = 469817, XrefRangeEnd = 469818, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Color32 ReadColor32()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadColor32_Public_Color32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Color32*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 469824, RefRangeEnd = 469825, XrefRangeStart = 469819, XrefRangeEnd = 469824, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Rect ReadRect()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadRect_Public_Rect_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Rect*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 469831, RefRangeEnd = 469832, XrefRangeStart = 469825, XrefRangeEnd = 469831, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Plane ReadPlane()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadPlane_Public_Plane_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Plane*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 469840, RefRangeEnd = 469841, XrefRangeStart = 469832, XrefRangeEnd = 469840, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Ray ReadRay()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadRay_Public_Ray_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Ray*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 469841, XrefRangeEnd = 469858, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Matrix4x4 ReadMatrix4x4()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadMatrix4x4_Public_Matrix4x4_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(Matrix4x4*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 469872, RefRangeEnd = 469875, XrefRangeStart = 469858, XrefRangeEnd = 469872, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Il2CppSystem.Type ReadType()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadType_Public_Type_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Type>(intPtr) : null;
	}

	[CallerCount(25)]
	[CachedScanResults(RefRangeStart = 469878, RefRangeEnd = 469903, XrefRangeStart = 469875, XrefRangeEnd = 469878, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe Team ReadTeam()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadTeam_Public_Team_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Team>(intPtr) : null;
	}

	[CallerCount(14)]
	[CachedScanResults(RefRangeStart = 469913, RefRangeEnd = 469927, XrefRangeStart = 469903, XrefRangeEnd = 469913, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe BaseGameObject ReadBaseGameObject()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadBaseGameObject_Public_BaseGameObject_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<BaseGameObject>(intPtr) : null;
	}

	[CallerCount(41)]
	[CachedScanResults(RefRangeStart = 469495, RefRangeEnd = 469536, XrefRangeStart = 469495, XrefRangeEnd = 469536, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe NetworkID ReadNetworkID()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ReadNetworkID_Public_NetworkID_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(NetworkID*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	public GameByteStreamReader(System.IntPtr pointer)
		: base(pointer)
	{
	}
}
