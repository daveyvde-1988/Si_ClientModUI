using System;
using System.Runtime.CompilerServices;
using Il2CppDebugTools;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSteamworks;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace Il2Cpp;

public static class NetworkLayer : Il2CppSystem.Object
{
	private static readonly System.IntPtr NativeFieldInfoPtr_MAX_PACKET_SIZE;

	private static readonly System.IntPtr NativeFieldInfoPtr_DROP_PACKET_SIZE;

	private static readonly System.IntPtr NativeFieldInfoPtr_MAX_QUEUEBUFFER_SIZE;

	private static readonly System.IntPtr NativeFieldInfoPtr_MAX_MESSAGES;

	private static readonly System.IntPtr NativeFieldInfoPtr_BUFFER_FULL_NO_SEND_UNRELIABLE_TIME;

	private static readonly System.IntPtr NativeFieldInfoPtr_BUFFER_NO_REPORT_EXCEEDED_TIME;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_PacketBuffer;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_MessageBuffer;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_DefaultConnectionOptions;

	private static readonly System.IntPtr NativeFieldInfoPtr__NetBitsAvgUpload_k__BackingField;

	private static readonly System.IntPtr NativeFieldInfoPtr__NetBitsAvgDownload_k__BackingField;

	private static readonly System.IntPtr NativeFieldInfoPtr_NetNoUnreliableTime;

	private static readonly System.IntPtr NativeFieldInfoPtr_NetNoReportBufferExceededTime;

	private static readonly System.IntPtr NativeFieldInfoPtr_NetBitsTime;

	private static readonly System.IntPtr NativeFieldInfoPtr_NetBitsSent;

	private static readonly System.IntPtr NativeFieldInfoPtr_NetBitsReceived;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_MessagesToSend;

	private static readonly System.IntPtr NativeFieldInfoPtr_s_MessageSendResults;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_DefaultConnectionOptions_Public_Static_get_Il2CppStructArray_1_SteamNetworkingConfigValue_t_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_NetBitsAvgUpload_Public_Static_get_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_NetBitsAvgUpload_Private_Static_set_Void_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_NetBitsAvgDownload_Public_Static_get_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_NetBitsAvgDownload_Private_Static_set_Void_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_IsHeavyLoad_Public_Static_get_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Update_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Clear_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CreateObjectPacketWriter_Public_Static_GameByteStreamWriter_NetworkComponent_ENetworkPacketType_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CreateTeamPacketWriter_Public_Static_GameByteStreamWriter_Team_ENetworkPacketType_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ConfigureDefaultLanes_Public_Static_Void_HSteamNetConnection_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_RetrieveNetworkData_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ProcessMessage_Private_Static_Void_GameByteStreamReader_UInt32_NetworkID_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_AcceptSessionWithUser_Public_Static_Boolean_NetworkID_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ServerCloseChannelWithUser_Public_Static_Boolean_NetworkID_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_OnInitClient_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_OnInitServer_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPacket_Private_Static_Boolean_HSteamNetConnection_Boolean_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendClientPacket_Private_Static_Boolean_NetworkID_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendServerPacket_Private_Static_Boolean_NetworkID_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendHeartbeat_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_RelayPacket_Public_Static_Void_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_Player_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_RelayVoiceStreamPacket_Public_Static_Void_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_Player_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPacket_Public_Static_Boolean_GameByteStreamWriter_ENetworkPacketSend_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPacketToPlayer_Public_Static_Boolean_GameByteStreamWriter_ENetworkPacketSend_Player_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPlayerConnect_Public_Static_Void_ENetworkPlayerConnectType_NetworkID_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPlayerConnectResponse_Public_Static_Void_ENetworkPlayerConnectType_NetworkID_Int32_NetworkID_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPlayerConnectRequest_Public_Static_Void_ENetworkPlayerConnectType_NetworkID_Int32_NetworkID_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPlayerList_Public_Static_Void_Player_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPlayerOverrides_Public_Static_Void_Player_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendOverride_Public_Static_Void_String_MemberOverride_Boolean_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPlayerUnitList_Public_Static_Void_Player_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPlayerSelectUnit_Public_Static_Void_Player_Unit_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPlayerSelectTeam_Public_Static_Void_Player_Team_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPlayerScore_Public_Static_Void_Player_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPlayerAdminLevel_Public_Static_Void_Player_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendVoteKick_Public_Static_Void_Player_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPlayerInitName_Public_Static_Void_Player_Player_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPlayerChangeName_Public_Static_Void_NetworkID_Int32_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendChatMessage_Public_Static_Void_NetworkID_Int32_String_Boolean_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendPlayerPing_Public_Static_Void_Player_Vector3_Target_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendTimeEvent_Public_Static_Void_Boolean_Single_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendTickMult_Public_Static_Void_Single_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendCheatsEvent_Public_Static_Void_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendTeamsInit_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendLoadLevelFinished_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendLoadLevel_Public_Static_Void_String_GameModeInfo_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendLoadLevel_Public_Static_Void_String_GameModeInfo_NetworkID_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendWorkshopIDs_Public_Static_Void_NetworkID_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendChecksum_Public_Static_Void_Il2CppStructArray_1_Byte_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendVoiceStream_Public_Static_Void_Il2CppStructArray_1_Single_Int32_Boolean_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendRemoteCommand_Public_Static_Void_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendRemoteCommandResult_Public_Static_Void_Player_List_1_String_List_1_LogLevel_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendSynchHealth_Public_Static_Void_List_1_DamageManager_List_1_GameObject_List_1_Byte_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendSynchHealth_Public_Static_Void_DamageManager_GameObject_Byte_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendSynchAIPathPacket_Public_Static_Void_GameByteStreamWriter_List_1_Player_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendSuicide_Public_Static_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SendSynchNavmeshRecalc_Public_Static_Void_Boolean_0;

	public unsafe static int MAX_PACKET_SIZE
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_MAX_PACKET_SIZE, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_MAX_PACKET_SIZE, (void*)(&num));
		}
	}

	public unsafe static int DROP_PACKET_SIZE
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_DROP_PACKET_SIZE, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_DROP_PACKET_SIZE, (void*)(&num));
		}
	}

	public unsafe static int MAX_QUEUEBUFFER_SIZE
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_MAX_QUEUEBUFFER_SIZE, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_MAX_QUEUEBUFFER_SIZE, (void*)(&num));
		}
	}

	public unsafe static int MAX_MESSAGES
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_MAX_MESSAGES, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_MAX_MESSAGES, (void*)(&num));
		}
	}

	public unsafe static float BUFFER_FULL_NO_SEND_UNRELIABLE_TIME
	{
		get
		{
			Unsafe.SkipInit(out float result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_BUFFER_FULL_NO_SEND_UNRELIABLE_TIME, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_BUFFER_FULL_NO_SEND_UNRELIABLE_TIME, (void*)(&num));
		}
	}

	public unsafe static float BUFFER_NO_REPORT_EXCEEDED_TIME
	{
		get
		{
			Unsafe.SkipInit(out float result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_BUFFER_NO_REPORT_EXCEEDED_TIME, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_BUFFER_NO_REPORT_EXCEEDED_TIME, (void*)(&num));
		}
	}

	public unsafe static Il2CppStructArray<byte> s_PacketBuffer
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_PacketBuffer, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppStructArray<byte>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_PacketBuffer, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)val));
		}
	}

	public unsafe static Il2CppStructArray<System.IntPtr> s_MessageBuffer
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_MessageBuffer, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppStructArray<System.IntPtr>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_MessageBuffer, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)val));
		}
	}

	public unsafe static Il2CppStructArray<SteamNetworkingConfigValue_t> s_DefaultConnectionOptions
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_DefaultConnectionOptions, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppStructArray<SteamNetworkingConfigValue_t>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_DefaultConnectionOptions, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)val));
		}
	}

	public unsafe static float _NetBitsAvgUpload_k__BackingField
	{
		get
		{
			Unsafe.SkipInit(out float result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__NetBitsAvgUpload_k__BackingField, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__NetBitsAvgUpload_k__BackingField, (void*)(&num));
		}
	}

	public unsafe static float _NetBitsAvgDownload_k__BackingField
	{
		get
		{
			Unsafe.SkipInit(out float result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr__NetBitsAvgDownload_k__BackingField, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr__NetBitsAvgDownload_k__BackingField, (void*)(&num));
		}
	}

	public unsafe static float NetNoUnreliableTime
	{
		get
		{
			Unsafe.SkipInit(out float result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_NetNoUnreliableTime, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_NetNoUnreliableTime, (void*)(&num));
		}
	}

	public unsafe static float NetNoReportBufferExceededTime
	{
		get
		{
			Unsafe.SkipInit(out float result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_NetNoReportBufferExceededTime, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_NetNoReportBufferExceededTime, (void*)(&num));
		}
	}

	public unsafe static float NetBitsTime
	{
		get
		{
			Unsafe.SkipInit(out float result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_NetBitsTime, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_NetBitsTime, (void*)(&num));
		}
	}

	public unsafe static uint NetBitsSent
	{
		get
		{
			Unsafe.SkipInit(out uint result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_NetBitsSent, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_NetBitsSent, (void*)(&num));
		}
	}

	public unsafe static uint NetBitsReceived
	{
		get
		{
			Unsafe.SkipInit(out uint result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_NetBitsReceived, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_NetBitsReceived, (void*)(&num));
		}
	}

	public unsafe static Il2CppStructArray<System.IntPtr> s_MessagesToSend
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_MessagesToSend, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppStructArray<System.IntPtr>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_MessagesToSend, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)val));
		}
	}

	public unsafe static Il2CppStructArray<long> s_MessageSendResults
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_s_MessageSendResults, (void*)(&intPtr));
			System.IntPtr intPtr2 = intPtr;
			return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppStructArray<long>>(intPtr2) : null;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_s_MessageSendResults, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)val));
		}
	}

	public unsafe static Il2CppStructArray<SteamNetworkingConfigValue_t> DefaultConnectionOptions
	{
		[CallerCount(4)]
		[CachedScanResults(RefRangeStart = 488095, RefRangeEnd = 488099, XrefRangeStart = 488077, XrefRangeEnd = 488095, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_DefaultConnectionOptions_Public_Static_get_Il2CppStructArray_1_SteamNetworkingConfigValue_t_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppStructArray<SteamNetworkingConfigValue_t>>(intPtr) : null;
		}
	}

	public unsafe static float NetBitsAvgUpload
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 488099, XrefRangeEnd = 488103, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_NetBitsAvgUpload_Public_Static_get_Single_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 488103, XrefRangeEnd = 488107, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_NetBitsAvgUpload_Private_Static_set_Void_Single_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static float NetBitsAvgDownload
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 488107, XrefRangeEnd = 488111, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_NetBitsAvgDownload_Public_Static_get_Single_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 488111, XrefRangeEnd = 488115, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_NetBitsAvgDownload_Private_Static_set_Void_Single_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe static bool IsHeavyLoad
	{
		[CallerCount(3)]
		[CachedScanResults(RefRangeStart = 488119, RefRangeEnd = 488122, XrefRangeStart = 488115, XrefRangeEnd = 488119, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_IsHeavyLoad_Public_Static_get_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}
	}

	static NetworkLayer()
	{
		Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr = IL2CPP.GetIl2CppClass("SilicaCore.dll", "", "NetworkLayer");
		IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr);
		NativeFieldInfoPtr_MAX_PACKET_SIZE = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "MAX_PACKET_SIZE");
		NativeFieldInfoPtr_DROP_PACKET_SIZE = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "DROP_PACKET_SIZE");
		NativeFieldInfoPtr_MAX_QUEUEBUFFER_SIZE = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "MAX_QUEUEBUFFER_SIZE");
		NativeFieldInfoPtr_MAX_MESSAGES = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "MAX_MESSAGES");
		NativeFieldInfoPtr_BUFFER_FULL_NO_SEND_UNRELIABLE_TIME = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "BUFFER_FULL_NO_SEND_UNRELIABLE_TIME");
		NativeFieldInfoPtr_BUFFER_NO_REPORT_EXCEEDED_TIME = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "BUFFER_NO_REPORT_EXCEEDED_TIME");
		NativeFieldInfoPtr_s_PacketBuffer = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "s_PacketBuffer");
		NativeFieldInfoPtr_s_MessageBuffer = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "s_MessageBuffer");
		NativeFieldInfoPtr_s_DefaultConnectionOptions = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "s_DefaultConnectionOptions");
		NativeFieldInfoPtr__NetBitsAvgUpload_k__BackingField = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "<NetBitsAvgUpload>k__BackingField");
		NativeFieldInfoPtr__NetBitsAvgDownload_k__BackingField = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "<NetBitsAvgDownload>k__BackingField");
		NativeFieldInfoPtr_NetNoUnreliableTime = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "NetNoUnreliableTime");
		NativeFieldInfoPtr_NetNoReportBufferExceededTime = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "NetNoReportBufferExceededTime");
		NativeFieldInfoPtr_NetBitsTime = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "NetBitsTime");
		NativeFieldInfoPtr_NetBitsSent = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "NetBitsSent");
		NativeFieldInfoPtr_NetBitsReceived = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "NetBitsReceived");
		NativeFieldInfoPtr_s_MessagesToSend = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "s_MessagesToSend");
		NativeFieldInfoPtr_s_MessageSendResults = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, "s_MessageSendResults");
		NativeMethodInfoPtr_get_DefaultConnectionOptions_Public_Static_get_Il2CppStructArray_1_SteamNetworkingConfigValue_t_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671417);
		NativeMethodInfoPtr_get_NetBitsAvgUpload_Public_Static_get_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671418);
		NativeMethodInfoPtr_set_NetBitsAvgUpload_Private_Static_set_Void_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671419);
		NativeMethodInfoPtr_get_NetBitsAvgDownload_Public_Static_get_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671420);
		NativeMethodInfoPtr_set_NetBitsAvgDownload_Private_Static_set_Void_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671421);
		NativeMethodInfoPtr_get_IsHeavyLoad_Public_Static_get_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671422);
		NativeMethodInfoPtr_Update_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671423);
		NativeMethodInfoPtr_Clear_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671424);
		NativeMethodInfoPtr_CreateObjectPacketWriter_Public_Static_GameByteStreamWriter_NetworkComponent_ENetworkPacketType_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671425);
		NativeMethodInfoPtr_CreateTeamPacketWriter_Public_Static_GameByteStreamWriter_Team_ENetworkPacketType_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671426);
		NativeMethodInfoPtr_ConfigureDefaultLanes_Public_Static_Void_HSteamNetConnection_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671427);
		NativeMethodInfoPtr_RetrieveNetworkData_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671428);
		NativeMethodInfoPtr_ProcessMessage_Private_Static_Void_GameByteStreamReader_UInt32_NetworkID_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671429);
		NativeMethodInfoPtr_AcceptSessionWithUser_Public_Static_Boolean_NetworkID_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671430);
		NativeMethodInfoPtr_ServerCloseChannelWithUser_Public_Static_Boolean_NetworkID_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671431);
		NativeMethodInfoPtr_OnInitClient_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671432);
		NativeMethodInfoPtr_OnInitServer_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671433);
		NativeMethodInfoPtr_SendPacket_Private_Static_Boolean_HSteamNetConnection_Boolean_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671434);
		NativeMethodInfoPtr_SendClientPacket_Private_Static_Boolean_NetworkID_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671435);
		NativeMethodInfoPtr_SendServerPacket_Private_Static_Boolean_NetworkID_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671436);
		NativeMethodInfoPtr_SendHeartbeat_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671437);
		NativeMethodInfoPtr_RelayPacket_Public_Static_Void_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_Player_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671438);
		NativeMethodInfoPtr_RelayVoiceStreamPacket_Public_Static_Void_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_Player_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671439);
		NativeMethodInfoPtr_SendPacket_Public_Static_Boolean_GameByteStreamWriter_ENetworkPacketSend_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671440);
		NativeMethodInfoPtr_SendPacketToPlayer_Public_Static_Boolean_GameByteStreamWriter_ENetworkPacketSend_Player_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671441);
		NativeMethodInfoPtr_SendPlayerConnect_Public_Static_Void_ENetworkPlayerConnectType_NetworkID_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671442);
		NativeMethodInfoPtr_SendPlayerConnectResponse_Public_Static_Void_ENetworkPlayerConnectType_NetworkID_Int32_NetworkID_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671443);
		NativeMethodInfoPtr_SendPlayerConnectRequest_Public_Static_Void_ENetworkPlayerConnectType_NetworkID_Int32_NetworkID_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671444);
		NativeMethodInfoPtr_SendPlayerList_Public_Static_Void_Player_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671445);
		NativeMethodInfoPtr_SendPlayerOverrides_Public_Static_Void_Player_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671446);
		NativeMethodInfoPtr_SendOverride_Public_Static_Void_String_MemberOverride_Boolean_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671447);
		NativeMethodInfoPtr_SendPlayerUnitList_Public_Static_Void_Player_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671448);
		NativeMethodInfoPtr_SendPlayerSelectUnit_Public_Static_Void_Player_Unit_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671449);
		NativeMethodInfoPtr_SendPlayerSelectTeam_Public_Static_Void_Player_Team_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671450);
		NativeMethodInfoPtr_SendPlayerScore_Public_Static_Void_Player_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671451);
		NativeMethodInfoPtr_SendPlayerAdminLevel_Public_Static_Void_Player_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671452);
		NativeMethodInfoPtr_SendVoteKick_Public_Static_Void_Player_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671453);
		NativeMethodInfoPtr_SendPlayerInitName_Public_Static_Void_Player_Player_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671454);
		NativeMethodInfoPtr_SendPlayerChangeName_Public_Static_Void_NetworkID_Int32_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671455);
		NativeMethodInfoPtr_SendChatMessage_Public_Static_Void_NetworkID_Int32_String_Boolean_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671456);
		NativeMethodInfoPtr_SendPlayerPing_Public_Static_Void_Player_Vector3_Target_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671457);
		NativeMethodInfoPtr_SendTimeEvent_Public_Static_Void_Boolean_Single_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671458);
		NativeMethodInfoPtr_SendTickMult_Public_Static_Void_Single_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671459);
		NativeMethodInfoPtr_SendCheatsEvent_Public_Static_Void_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671460);
		NativeMethodInfoPtr_SendTeamsInit_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671461);
		NativeMethodInfoPtr_SendLoadLevelFinished_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671462);
		NativeMethodInfoPtr_SendLoadLevel_Public_Static_Void_String_GameModeInfo_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671463);
		NativeMethodInfoPtr_SendLoadLevel_Public_Static_Void_String_GameModeInfo_NetworkID_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671464);
		NativeMethodInfoPtr_SendWorkshopIDs_Public_Static_Void_NetworkID_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671465);
		NativeMethodInfoPtr_SendChecksum_Public_Static_Void_Il2CppStructArray_1_Byte_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671466);
		NativeMethodInfoPtr_SendVoiceStream_Public_Static_Void_Il2CppStructArray_1_Single_Int32_Boolean_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671467);
		NativeMethodInfoPtr_SendRemoteCommand_Public_Static_Void_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671468);
		NativeMethodInfoPtr_SendRemoteCommandResult_Public_Static_Void_Player_List_1_String_List_1_LogLevel_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671469);
		NativeMethodInfoPtr_SendSynchHealth_Public_Static_Void_List_1_DamageManager_List_1_GameObject_List_1_Byte_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671470);
		NativeMethodInfoPtr_SendSynchHealth_Public_Static_Void_DamageManager_GameObject_Byte_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671471);
		NativeMethodInfoPtr_SendSynchAIPathPacket_Public_Static_Void_GameByteStreamWriter_List_1_Player_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671472);
		NativeMethodInfoPtr_SendSuicide_Public_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671473);
		NativeMethodInfoPtr_SendSynchNavmeshRecalc_Public_Static_Void_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<NetworkLayer>.NativeClassPtr, 100671474);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 488155, RefRangeEnd = 488156, XrefRangeStart = 488122, XrefRangeEnd = 488155, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Update()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Update_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 488164, RefRangeEnd = 488165, XrefRangeStart = 488156, XrefRangeEnd = 488164, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void Clear()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Clear_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(11)]
	[CachedScanResults(RefRangeStart = 488173, RefRangeEnd = 488184, XrefRangeStart = 488165, XrefRangeEnd = 488173, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static GameByteStreamWriter CreateObjectPacketWriter(NetworkComponent callerObject, ENetworkPacketType packetType)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)callerObject);
		*(ENetworkPacketType**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &packetType;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CreateObjectPacketWriter_Public_Static_GameByteStreamWriter_NetworkComponent_ENetworkPacketType_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<GameByteStreamWriter>(intPtr) : null;
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 488193, RefRangeEnd = 488194, XrefRangeStart = 488184, XrefRangeEnd = 488193, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static GameByteStreamWriter CreateTeamPacketWriter(Team callerTeam, ENetworkPacketType packetType)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)callerTeam);
		*(ENetworkPacketType**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &packetType;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CreateTeamPacketWriter_Public_Static_GameByteStreamWriter_Team_ENetworkPacketType_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<GameByteStreamWriter>(intPtr) : null;
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 488225, RefRangeEnd = 488226, XrefRangeStart = 488194, XrefRangeEnd = 488225, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void ConfigureDefaultLanes(HSteamNetConnection connection, bool server)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&connection);
		*(bool**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &server;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ConfigureDefaultLanes_Public_Static_Void_HSteamNetConnection_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 488317, RefRangeEnd = 488318, XrefRangeStart = 488226, XrefRangeEnd = 488317, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void RetrieveNetworkData()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_RetrieveNetworkData_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 488414, RefRangeEnd = 488415, XrefRangeStart = 488318, XrefRangeEnd = 488414, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void ProcessMessage(GameByteStreamReader packetReader, uint packetLength, NetworkID remoteID, bool isServer)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)packetReader);
		*(uint**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &packetLength;
		*(NetworkID**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &remoteID;
		*(bool**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &isServer;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ProcessMessage_Private_Static_Void_GameByteStreamReader_UInt32_NetworkID_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(211)]
	[CachedScanResults(RefRangeStart = 7685, RefRangeEnd = 7896, XrefRangeStart = 7685, XrefRangeEnd = 7896, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool AcceptSessionWithUser(NetworkID userID)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&userID);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_AcceptSessionWithUser_Public_Static_Boolean_NetworkID_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 488416, RefRangeEnd = 488418, XrefRangeStart = 488415, XrefRangeEnd = 488416, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool ServerCloseChannelWithUser(NetworkID userID, int channel)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&userID);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &channel;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ServerCloseChannelWithUser_Public_Static_Boolean_NetworkID_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 488424, RefRangeEnd = 488425, XrefRangeStart = 488418, XrefRangeEnd = 488424, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void OnInitClient()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_OnInitClient_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 488431, RefRangeEnd = 488432, XrefRangeStart = 488425, XrefRangeEnd = 488431, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void OnInitServer()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_OnInitServer_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 488572, RefRangeEnd = 488574, XrefRangeStart = 488432, XrefRangeEnd = 488572, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool SendPacket(HSteamNetConnection connection, bool server, Il2CppStructArray<byte> pubData, uint cubData, ENetworkPacketSend sendType)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[5];
		*ptr = (nint)(&connection);
		*(bool**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &server;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)pubData);
		*(uint**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &cubData;
		*(ENetworkPacketSend**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &sendType;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPacket_Private_Static_Boolean_HSteamNetConnection_Boolean_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(13)]
	[CachedScanResults(RefRangeStart = 488588, RefRangeEnd = 488601, XrefRangeStart = 488574, XrefRangeEnd = 488588, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool SendClientPacket(NetworkID IDRemote, Il2CppStructArray<byte> pubData, uint cubData, ENetworkPacketSend sendType, int nChannel = 0)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[5];
		*ptr = (nint)(&IDRemote);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)pubData);
		*(uint**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &cubData;
		*(ENetworkPacketSend**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &sendType;
		*(int**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &nChannel;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendClientPacket_Private_Static_Boolean_NetworkID_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(25)]
	[CachedScanResults(RefRangeStart = 488615, RefRangeEnd = 488640, XrefRangeStart = 488601, XrefRangeEnd = 488615, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool SendServerPacket(NetworkID IDRemote, Il2CppStructArray<byte> pubData, uint cubData, ENetworkPacketSend sendType, int nChannel = 0)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[5];
		*ptr = (nint)(&IDRemote);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)pubData);
		*(uint**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &cubData;
		*(ENetworkPacketSend**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &sendType;
		*(int**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &nChannel;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendServerPacket_Private_Static_Boolean_NetworkID_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 488640, XrefRangeEnd = 488666, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendHeartbeat()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendHeartbeat_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 488666, XrefRangeEnd = 488692, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void RelayPacket(Il2CppStructArray<byte> packet, uint packetBytesSize, ENetworkPacketSend sendType, Player ignorePlayer)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)packet);
		*(uint**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &packetBytesSize;
		*(ENetworkPacketSend**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &sendType;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)ignorePlayer);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_RelayPacket_Public_Static_Void_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_Player_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 488692, XrefRangeEnd = 488719, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void RelayVoiceStreamPacket(Il2CppStructArray<byte> packet, uint packetBytesSize, ENetworkPacketSend sendType, Player senderPlayer, bool proximityOnly)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[5];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)packet);
		*(uint**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &packetBytesSize;
		*(ENetworkPacketSend**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &sendType;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)senderPlayer);
		*(bool**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &proximityOnly;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_RelayVoiceStreamPacket_Public_Static_Void_Il2CppStructArray_1_Byte_UInt32_ENetworkPacketSend_Player_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(39)]
	[CachedScanResults(RefRangeStart = 488738, RefRangeEnd = 488777, XrefRangeStart = 488719, XrefRangeEnd = 488738, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool SendPacket(GameByteStreamWriter packetWriter, ENetworkPacketSend sendType)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)packetWriter);
		*(ENetworkPacketSend**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &sendType;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPacket_Public_Static_Boolean_GameByteStreamWriter_ENetworkPacketSend_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(10)]
	[CachedScanResults(RefRangeStart = 488792, RefRangeEnd = 488802, XrefRangeStart = 488777, XrefRangeEnd = 488792, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool SendPacketToPlayer(GameByteStreamWriter packetWriter, ENetworkPacketSend sendType, Player player)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)packetWriter);
		*(ENetworkPacketSend**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &sendType;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)player);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPacketToPlayer_Public_Static_Boolean_GameByteStreamWriter_ENetworkPacketSend_Player_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 488837, RefRangeEnd = 488841, XrefRangeStart = 488802, XrefRangeEnd = 488837, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendPlayerConnect(ENetworkPlayerConnectType connectType, NetworkID playerID, int playerChannel)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&connectType);
		*(NetworkID**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &playerID;
		*(int**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &playerChannel;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPlayerConnect_Public_Static_Void_ENetworkPlayerConnectType_NetworkID_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(9)]
	[CachedScanResults(RefRangeStart = 488862, RefRangeEnd = 488871, XrefRangeStart = 488841, XrefRangeEnd = 488862, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendPlayerConnectResponse(ENetworkPlayerConnectType connectType, NetworkID playerID, int playerChannel, NetworkID serverID)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = (nint)(&connectType);
		*(NetworkID**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &playerID;
		*(int**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &playerChannel;
		*(NetworkID**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &serverID;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPlayerConnectResponse_Public_Static_Void_ENetworkPlayerConnectType_NetworkID_Int32_NetworkID_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 488891, RefRangeEnd = 488895, XrefRangeStart = 488871, XrefRangeEnd = 488891, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendPlayerConnectRequest(ENetworkPlayerConnectType connectType, NetworkID playerID, int playerChannel, NetworkID serverID, string password = null)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[5];
		*ptr = (nint)(&connectType);
		*(NetworkID**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &playerID;
		*(int**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &playerChannel;
		*(NetworkID**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &serverID;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(password);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPlayerConnectRequest_Public_Static_Void_ENetworkPlayerConnectType_NetworkID_Int32_NetworkID_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 488895, XrefRangeEnd = 488933, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendPlayerList(Player player)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)player);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPlayerList_Public_Static_Void_Player_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 488933, XrefRangeEnd = 489002, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendPlayerOverrides(Player player)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)player);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPlayerOverrides_Public_Static_Void_Player_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(5)]
	[CachedScanResults(RefRangeStart = 489045, RefRangeEnd = 489050, XrefRangeStart = 489002, XrefRangeEnd = 489045, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendOverride(string target, OverrideManager.MemberOverride memberOverride, bool revert = false, bool revertAll = false)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(target);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)memberOverride);
		*(bool**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &revert;
		*(bool**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &revertAll;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendOverride_Public_Static_Void_String_MemberOverride_Boolean_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 489050, XrefRangeEnd = 489086, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendPlayerUnitList(Player player)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)player);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPlayerUnitList_Public_Static_Void_Player_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(13)]
	[CachedScanResults(RefRangeStart = 489138, RefRangeEnd = 489151, XrefRangeStart = 489086, XrefRangeEnd = 489138, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendPlayerSelectUnit(Player playerSelect, Unit unit)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)playerSelect);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)unit);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPlayerSelectUnit_Public_Static_Void_Player_Unit_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(15)]
	[CachedScanResults(RefRangeStart = 489184, RefRangeEnd = 489199, XrefRangeStart = 489151, XrefRangeEnd = 489184, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendPlayerSelectTeam(Player playerSelect, Team team)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)playerSelect);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)team);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPlayerSelectTeam_Public_Static_Void_Player_Team_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 489244, RefRangeEnd = 489245, XrefRangeStart = 489199, XrefRangeEnd = 489244, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendPlayerScore(Player forPlayer)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)forPlayer);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPlayerScore_Public_Static_Void_Player_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 489288, RefRangeEnd = 489289, XrefRangeStart = 489245, XrefRangeEnd = 489288, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendPlayerAdminLevel(Player forPlayer)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)forPlayer);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPlayerAdminLevel_Public_Static_Void_Player_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 489322, RefRangeEnd = 489323, XrefRangeStart = 489289, XrefRangeEnd = 489322, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendVoteKick(Player kickPlayer, bool kick)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)kickPlayer);
		*(bool**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &kick;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendVoteKick_Public_Static_Void_Player_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 489323, XrefRangeEnd = 489357, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendPlayerInitName(Player toPlayer, Player playerGetName)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)toPlayer);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)playerGetName);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPlayerInitName_Public_Static_Void_Player_Player_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 489387, RefRangeEnd = 489388, XrefRangeStart = 489357, XrefRangeEnd = 489387, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendPlayerChangeName(NetworkID playerID, int playerChannel, string newName)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&playerID);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &playerChannel;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(newName);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPlayerChangeName_Public_Static_Void_NetworkID_Int32_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 489419, RefRangeEnd = 489421, XrefRangeStart = 489388, XrefRangeEnd = 489419, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendChatMessage(NetworkID playerID, int playerChannel, string message, bool teamOnly, bool isServerMessage = false)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[5];
		*ptr = (nint)(&playerID);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &playerChannel;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(message);
		*(bool**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &teamOnly;
		*(bool**)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = &isServerMessage;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendChatMessage_Public_Static_Void_NetworkID_Int32_String_Boolean_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 489470, RefRangeEnd = 489471, XrefRangeStart = 489421, XrefRangeEnd = 489470, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendPlayerPing(Player pingPlayer, Vector3 position, Target target)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)pingPlayer);
		*(Vector3**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &position;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)target);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendPlayerPing_Public_Static_Void_Player_Vector3_Target_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 489510, RefRangeEnd = 489511, XrefRangeStart = 489471, XrefRangeEnd = 489510, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendTimeEvent(bool slowTime, float timeScaleTarget, float timeScaleCurrent)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&slowTime);
		*(float**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &timeScaleTarget;
		*(float**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &timeScaleCurrent;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendTimeEvent_Public_Static_Void_Boolean_Single_Single_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 489548, RefRangeEnd = 489549, XrefRangeStart = 489511, XrefRangeEnd = 489548, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendTickMult(float tickMult)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&tickMult);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendTickMult_Public_Static_Void_Single_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 489586, RefRangeEnd = 489587, XrefRangeStart = 489549, XrefRangeEnd = 489586, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendCheatsEvent(bool enableCheats)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&enableCheats);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendCheatsEvent_Public_Static_Void_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 489587, XrefRangeEnd = 489660, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendTeamsInit()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendTeamsInit_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 489660, XrefRangeEnd = 489703, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendLoadLevelFinished()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendLoadLevelFinished_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 489746, RefRangeEnd = 489747, XrefRangeStart = 489703, XrefRangeEnd = 489746, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendLoadLevel(string levelName, GameModeInfo gameMode)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(levelName);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gameMode);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendLoadLevel_Public_Static_Void_String_GameModeInfo_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 489780, RefRangeEnd = 489782, XrefRangeStart = 489747, XrefRangeEnd = 489780, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendLoadLevel(string levelName, GameModeInfo gameMode, NetworkID playerID, int playerChannel)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(levelName);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gameMode);
		*(NetworkID**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &playerID;
		*(int**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &playerChannel;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendLoadLevel_Public_Static_Void_String_GameModeInfo_NetworkID_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 489819, RefRangeEnd = 489820, XrefRangeStart = 489782, XrefRangeEnd = 489819, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendWorkshopIDs(NetworkID playerID, int playerChannel)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&playerID);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &playerChannel;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendWorkshopIDs_Public_Static_Void_NetworkID_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 489844, RefRangeEnd = 489845, XrefRangeStart = 489820, XrefRangeEnd = 489844, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendChecksum(Il2CppStructArray<byte> checksum)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)checksum);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendChecksum_Public_Static_Void_Il2CppStructArray_1_Byte_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 489888, RefRangeEnd = 489890, XrefRangeStart = 489845, XrefRangeEnd = 489888, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendVoiceStream(Il2CppStructArray<float> voicePacketBuffer, int startOffset, bool silence, bool proximityOnly)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)voicePacketBuffer);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &startOffset;
		*(bool**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &silence;
		*(bool**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &proximityOnly;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendVoiceStream_Public_Static_Void_Il2CppStructArray_1_Single_Int32_Boolean_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 489890, XrefRangeEnd = 489921, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendRemoteCommand(string command)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(command);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendRemoteCommand_Public_Static_Void_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 489921, XrefRangeEnd = 489957, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendRemoteCommandResult(Player remotePlayer, List<string> commandResult, List<DebugConsole.LogLevel> logLevel)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)remotePlayer);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)commandResult);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)logLevel);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendRemoteCommandResult_Public_Static_Void_Player_List_1_String_List_1_LogLevel_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 490027, RefRangeEnd = 490028, XrefRangeStart = 489957, XrefRangeEnd = 490027, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendSynchHealth(List<DamageManager> synchHealthList, List<GameObject> synchInstigatorsList, List<byte> synchHitAngList, int sendMax)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)synchHealthList);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)synchInstigatorsList);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)synchHitAngList);
		*(int**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &sendMax;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendSynchHealth_Public_Static_Void_List_1_DamageManager_List_1_GameObject_List_1_Byte_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 490079, RefRangeEnd = 490081, XrefRangeStart = 490028, XrefRangeEnd = 490079, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendSynchHealth(DamageManager synchHealthDM, GameObject synchInstigator, byte synchHitAng)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)synchHealthDM);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)synchInstigator);
		*(byte**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &synchHitAng;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendSynchHealth_Public_Static_Void_DamageManager_GameObject_Byte_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 490098, RefRangeEnd = 490100, XrefRangeStart = 490081, XrefRangeEnd = 490098, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendSynchAIPathPacket(GameByteStreamWriter packetWriter, List<Player> toPlayers)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)packetWriter);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)toPlayers);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendSynchAIPathPacket_Public_Static_Void_GameByteStreamWriter_List_1_Player_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 490155, RefRangeEnd = 490156, XrefRangeStart = 490100, XrefRangeEnd = 490155, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendSuicide()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendSuicide_Public_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 490193, RefRangeEnd = 490195, XrefRangeStart = 490156, XrefRangeEnd = 490193, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static void SendSynchNavmeshRecalc(bool isAnyGraphUpdateInProgress)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&isAnyGraphUpdateInProgress);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SendSynchNavmeshRecalc_Public_Static_Void_Boolean_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	public NetworkLayer(System.IntPtr pointer)
		: base(pointer)
	{
	}
}
