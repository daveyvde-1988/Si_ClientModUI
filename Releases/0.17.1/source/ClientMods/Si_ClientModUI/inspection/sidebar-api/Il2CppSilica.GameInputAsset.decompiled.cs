using System;
using System.Runtime.CompilerServices;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Il2CppSilica;

public class GameInputAsset : ScriptableObject
{
	public static class Rebinding : Il2CppSystem.Object
	{
		public sealed class ProposalData : Il2CppSystem.ValueType
		{
			private static readonly System.IntPtr NativeFieldInfoPtr_modifications;

			private static readonly System.IntPtr NativeFieldInfoPtr_action;

			private static readonly System.IntPtr NativeFieldInfoPtr_proposedBinding;

			private static readonly System.IntPtr NativeFieldInfoPtr_bindingIndex;

			private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_InputAction_InputBinding_Int32_List_1_ActionModification_0;

			public unsafe List<ActionModification> modifications
			{
				get
				{
					nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_modifications);
					System.IntPtr intPtr = *(System.IntPtr*)num;
					return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<ActionModification>>(intPtr) : null;
				}
				set
				{
					System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
					IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_modifications)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
				}
			}

			public unsafe InputAction action
			{
				get
				{
					nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_action);
					System.IntPtr intPtr = *(System.IntPtr*)num;
					return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<InputAction>(intPtr) : null;
				}
				set
				{
					System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
					IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_action)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)inputAction));
				}
			}

			public unsafe InputBinding proposedBinding
			{
				get
				{
					nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_proposedBinding);
					return new InputBinding(IL2CPP.il2cpp_value_box(Il2CppClassPointerStore<InputBinding>.NativeClassPtr, (System.IntPtr)num));
				}
				set
				{
					// IL cpblk instruction
					Unsafe.CopyBlock((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_proposedBinding), IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)inputBinding)), IL2CPP.il2cpp_class_value_size(Il2CppClassPointerStore<InputBinding>.NativeClassPtr, ref *(uint*)null));
				}
			}

			public unsafe int bindingIndex
			{
				get
				{
					nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_bindingIndex);
					return *(int*)num;
				}
				set
				{
					*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_bindingIndex)) = num;
				}
			}

			static ProposalData()
			{
				Il2CppClassPointerStore<ProposalData>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<Rebinding>.NativeClassPtr, "ProposalData");
				IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<ProposalData>.NativeClassPtr);
				NativeFieldInfoPtr_modifications = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ProposalData>.NativeClassPtr, "modifications");
				NativeFieldInfoPtr_action = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ProposalData>.NativeClassPtr, "action");
				NativeFieldInfoPtr_proposedBinding = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ProposalData>.NativeClassPtr, "proposedBinding");
				NativeFieldInfoPtr_bindingIndex = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ProposalData>.NativeClassPtr, "bindingIndex");
				NativeMethodInfoPtr__ctor_Public_Void_InputAction_InputBinding_Int32_List_1_ActionModification_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ProposalData>.NativeClassPtr, 100682832);
			}

			[CallerCount(1)]
			[CachedScanResults(RefRangeStart = 609330, RefRangeEnd = 609331, XrefRangeStart = 609327, XrefRangeEnd = 609330, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
			public unsafe ProposalData(InputAction action, InputBinding binding, int bindingIndex, List<ActionModification> modifications)
				: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ProposalData>.NativeClassPtr))
			{
				System.IntPtr* ptr = stackalloc System.IntPtr[4];
				*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
				*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)binding));
				*(int**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &bindingIndex;
				*(System.IntPtr*)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)modifications);
				Unsafe.SkipInit(out System.IntPtr intPtr2);
				System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_InputAction_InputBinding_Int32_List_1_ActionModification_0, IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this)), (void**)ptr, ref intPtr2);
				Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			}

			public ProposalData(System.IntPtr pointer)
				: base(pointer)
			{
			}

			public ProposalData()
				: base(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ProposalData>.NativeClassPtr))
			{
			}
		}

		[ObfuscatedName("Silica.GameInputAsset+Rebinding+<>c__DisplayClass2_0")]
		public sealed class __c__DisplayClass2_0 : Il2CppSystem.Object
		{
			private static readonly System.IntPtr NativeFieldInfoPtr_action;

			private static readonly System.IntPtr NativeFieldInfoPtr_bindingIndex;

			private static readonly System.IntPtr NativeFieldInfoPtr_binding;

			private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

			private static readonly System.IntPtr NativeMethodInfoPtr__BeginRebind_b__1_Internal_Void_RebindingOperation_0;

			private static readonly System.IntPtr NativeMethodInfoPtr__BeginRebind_b__2_Internal_Void_RebindingOperation_0;

			public unsafe InputAction action
			{
				get
				{
					nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_action);
					System.IntPtr intPtr = *(System.IntPtr*)num;
					return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<InputAction>(intPtr) : null;
				}
				set
				{
					System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
					IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_action)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)inputAction));
				}
			}

			public unsafe int bindingIndex
			{
				get
				{
					nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_bindingIndex);
					return *(int*)num;
				}
				set
				{
					*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_bindingIndex)) = num;
				}
			}

			public unsafe InputBinding binding
			{
				get
				{
					nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_binding);
					return new InputBinding(IL2CPP.il2cpp_value_box(Il2CppClassPointerStore<InputBinding>.NativeClassPtr, (System.IntPtr)num));
				}
				set
				{
					// IL cpblk instruction
					Unsafe.CopyBlock((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_binding), IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)inputBinding)), IL2CPP.il2cpp_class_value_size(Il2CppClassPointerStore<InputBinding>.NativeClassPtr, ref *(uint*)null));
				}
			}

			static __c__DisplayClass2_0()
			{
				Il2CppClassPointerStore<__c__DisplayClass2_0>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<Rebinding>.NativeClassPtr, "<>c__DisplayClass2_0");
				IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<__c__DisplayClass2_0>.NativeClassPtr);
				NativeFieldInfoPtr_action = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c__DisplayClass2_0>.NativeClassPtr, "action");
				NativeFieldInfoPtr_bindingIndex = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c__DisplayClass2_0>.NativeClassPtr, "bindingIndex");
				NativeFieldInfoPtr_binding = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c__DisplayClass2_0>.NativeClassPtr, "binding");
				NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass2_0>.NativeClassPtr, 100682833);
				NativeMethodInfoPtr__BeginRebind_b__1_Internal_Void_RebindingOperation_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass2_0>.NativeClassPtr, 100682834);
				NativeMethodInfoPtr__BeginRebind_b__2_Internal_Void_RebindingOperation_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass2_0>.NativeClassPtr, 100682835);
			}

			[CallerCount(2477)]
			[CachedScanResults(RefRangeStart = 412, RefRangeEnd = 2889, XrefRangeStart = 412, XrefRangeEnd = 2889, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
			public unsafe __c__DisplayClass2_0()
				: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<__c__DisplayClass2_0>.NativeClassPtr))
			{
				System.IntPtr* ptr = null;
				Unsafe.SkipInit(out System.IntPtr intPtr2);
				System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
				Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			}

			[CallerCount(0)]
			[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609331, XrefRangeEnd = 609374, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
			public unsafe void _BeginRebind_b__1(InputActionRebindingExtensions.RebindingOperation operation)
			{
				IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				System.IntPtr* ptr = stackalloc System.IntPtr[1];
				*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)operation);
				Unsafe.SkipInit(out System.IntPtr intPtr2);
				System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__BeginRebind_b__1_Internal_Void_RebindingOperation_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
				Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			}

			[CallerCount(0)]
			[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609374, XrefRangeEnd = 609426, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
			public unsafe void _BeginRebind_b__2(InputActionRebindingExtensions.RebindingOperation operation)
			{
				IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				System.IntPtr* ptr = stackalloc System.IntPtr[1];
				*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)operation);
				Unsafe.SkipInit(out System.IntPtr intPtr2);
				System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__BeginRebind_b__2_Internal_Void_RebindingOperation_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
				Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			}

			public __c__DisplayClass2_0(System.IntPtr pointer)
				: base(pointer)
			{
			}
		}

		private static readonly System.IntPtr NativeFieldInfoPtr_OnProposal;

		private static readonly System.IntPtr NativeFieldInfoPtr_rebindingOperation;

		private static readonly System.IntPtr NativeMethodInfoPtr_BeginRebind_Public_Static_Boolean_InputAction_InputBinding_Int32_0;

		private static readonly System.IntPtr NativeMethodInfoPtr_Unbind_Public_Static_Void_InputAction_Int32_0;

		private static readonly System.IntPtr NativeMethodInfoPtr_RemoveOverride_Public_Static_Void_InputAction_Int32_0;

		private static readonly System.IntPtr NativeMethodInfoPtr_Bind_Public_Static_Void_InputAction_Int32_String_0;

		private static readonly System.IntPtr NativeMethodInfoPtr_Method_Internal_Static_Void_0;

		public unsafe static Il2CppSystem.Action<ProposalData> OnProposal
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_OnProposal, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<ProposalData>>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_OnProposal, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
			}
		}

		public unsafe static InputActionRebindingExtensions.RebindingOperation rebindingOperation
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_rebindingOperation, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<InputActionRebindingExtensions.RebindingOperation>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_rebindingOperation, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)rebindingOperation));
			}
		}

		static Rebinding()
		{
			Il2CppClassPointerStore<Rebinding>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "Rebinding");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<Rebinding>.NativeClassPtr);
			NativeFieldInfoPtr_OnProposal = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Rebinding>.NativeClassPtr, "OnProposal");
			NativeFieldInfoPtr_rebindingOperation = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<Rebinding>.NativeClassPtr, "rebindingOperation");
			NativeMethodInfoPtr_BeginRebind_Public_Static_Boolean_InputAction_InputBinding_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Rebinding>.NativeClassPtr, 100682827);
			NativeMethodInfoPtr_Unbind_Public_Static_Void_InputAction_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Rebinding>.NativeClassPtr, 100682828);
			NativeMethodInfoPtr_RemoveOverride_Public_Static_Void_InputAction_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Rebinding>.NativeClassPtr, 100682829);
			NativeMethodInfoPtr_Bind_Public_Static_Void_InputAction_Int32_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Rebinding>.NativeClassPtr, 100682830);
			NativeMethodInfoPtr_Method_Internal_Static_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<Rebinding>.NativeClassPtr, 100682831);
		}

		[CallerCount(1)]
		[CachedScanResults(RefRangeStart = 609500, RefRangeEnd = 609501, XrefRangeStart = 609426, XrefRangeEnd = 609500, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe static bool BeginRebind(InputAction action, InputBinding binding, int bindingIndex)
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[3];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
			*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)binding));
			*(int**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &bindingIndex;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_BeginRebind_Public_Static_Boolean_InputAction_InputBinding_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		[CallerCount(3)]
		[CachedScanResults(RefRangeStart = 609517, RefRangeEnd = 609520, XrefRangeStart = 609501, XrefRangeEnd = 609517, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe static void Unbind(InputAction action, int bindingIndex)
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[2];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
			*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &bindingIndex;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Unbind_Public_Static_Void_InputAction_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609520, XrefRangeEnd = 609534, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe static void RemoveOverride(InputAction action, int bindingIndex)
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[2];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
			*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &bindingIndex;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_RemoveOverride_Public_Static_Void_InputAction_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(4)]
		[CachedScanResults(RefRangeStart = 609553, RefRangeEnd = 609557, XrefRangeStart = 609534, XrefRangeEnd = 609553, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe static void Bind(InputAction action, int bindingIndex, string overridePath)
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[3];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
			*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &bindingIndex;
			*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(overridePath);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Bind_Public_Static_Void_InputAction_Int32_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(2)]
		[CachedScanResults(RefRangeStart = 609569, RefRangeEnd = 609571, XrefRangeStart = 609557, XrefRangeEnd = 609569, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe static void Method_Internal_Static_Void_0()
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Method_Internal_Static_Void_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		public Rebinding(System.IntPtr pointer)
			: base(pointer)
		{
		}
	}

	public class ActionComparer : Il2CppSystem.Object
	{
		private static readonly System.IntPtr NativeMethodInfoPtr_Equals_Public_Virtual_Final_New_Boolean_ActionModification_ActionModification_0;

		private static readonly System.IntPtr NativeMethodInfoPtr_GetHashCode_Public_Virtual_Final_New_Int32_ActionModification_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

		static ActionComparer()
		{
			Il2CppClassPointerStore<ActionComparer>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "ActionComparer");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<ActionComparer>.NativeClassPtr);
			NativeMethodInfoPtr_Equals_Public_Virtual_Final_New_Boolean_ActionModification_ActionModification_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ActionComparer>.NativeClassPtr, 100682836);
			NativeMethodInfoPtr_GetHashCode_Public_Virtual_Final_New_Int32_ActionModification_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ActionComparer>.NativeClassPtr, 100682837);
			NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ActionComparer>.NativeClassPtr, 100682838);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609571, XrefRangeEnd = 609574, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe virtual bool Equals(ActionModification x, ActionModification y)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[2];
			*ptr = IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)x));
			*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)y));
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Equals_Public_Virtual_Final_New_Boolean_ActionModification_ActionModification_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609574, XrefRangeEnd = 609576, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe virtual int GetHashCode(ActionModification action)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)action));
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetHashCode_Public_Virtual_Final_New_Int32_ActionModification_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		[CallerCount(2477)]
		[CachedScanResults(RefRangeStart = 412, RefRangeEnd = 2889, XrefRangeStart = 412, XrefRangeEnd = 2889, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe ActionComparer()
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ActionComparer>.NativeClassPtr))
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		public ActionComparer(System.IntPtr pointer)
			: base(pointer)
		{
		}
	}

	public sealed class ActionConflict : Il2CppSystem.ValueType
	{
		private static readonly System.IntPtr NativeFieldInfoPtr_configuration;

		private static readonly System.IntPtr NativeFieldInfoPtr_bindingIndex;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_ActionData_Int32_0;

		public unsafe ActionData configuration
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_configuration);
				return new ActionData(IL2CPP.il2cpp_value_box(Il2CppClassPointerStore<ActionData>.NativeClassPtr, (System.IntPtr)num));
			}
			set
			{
				// IL cpblk instruction
				Unsafe.CopyBlock((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_configuration), IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)actionData)), IL2CPP.il2cpp_class_value_size(Il2CppClassPointerStore<ActionData>.NativeClassPtr, ref *(uint*)null));
			}
		}

		public unsafe int bindingIndex
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_bindingIndex);
				return *(int*)num;
			}
			set
			{
				*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_bindingIndex)) = num;
			}
		}

		static ActionConflict()
		{
			Il2CppClassPointerStore<ActionConflict>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "ActionConflict");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<ActionConflict>.NativeClassPtr);
			NativeFieldInfoPtr_configuration = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionConflict>.NativeClassPtr, "configuration");
			NativeFieldInfoPtr_bindingIndex = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionConflict>.NativeClassPtr, "bindingIndex");
			NativeMethodInfoPtr__ctor_Public_Void_ActionData_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ActionConflict>.NativeClassPtr, 100682839);
		}

		[CallerCount(5)]
		[CachedScanResults(RefRangeStart = 609577, RefRangeEnd = 609582, XrefRangeStart = 609576, XrefRangeEnd = 609577, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe ActionConflict(ActionData configuration, int bindingIndex)
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ActionConflict>.NativeClassPtr))
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[2];
			*ptr = IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)configuration));
			*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &bindingIndex;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_ActionData_Int32_0, IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this)), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		public ActionConflict(System.IntPtr pointer)
			: base(pointer)
		{
		}

		public ActionConflict()
			: base(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ActionConflict>.NativeClassPtr))
		{
		}
	}

	[OriginalName("SilicaCore.dll", "", "ModificationFlag")]
	public enum ModificationFlag
	{
		None,
		Bind,
		Unbind,
		Conflict,
		HardConflict
	}

	[System.Serializable]
	[OriginalName("SilicaCore.dll", "", "ActionFlags")]
	[System.Flags]
	public enum ActionFlags
	{
		None = 1,
		DisallowRebind = 2,
		NonExclusive = 4,
		GlobalExclusive = 8,
		EnableOverride = 0x10,
		GlobalParent = 0x20,
		GlobalNonExclusive = 0x40,
		DependentChild = 0x80,
		Toggle = 0x100,
		HiddenInRebindMenu = 0x200
	}

	public sealed class ActionData : Il2CppSystem.ValueType
	{
		private static readonly System.IntPtr NativeFieldInfoPtr_action;

		private static readonly System.IntPtr NativeFieldInfoPtr_configuration;

		private static readonly System.IntPtr NativeFieldInfoPtr_allowedConflicts;

		private static readonly System.IntPtr NativeFieldInfoPtr_groupId;

		private static readonly System.IntPtr NativeFieldInfoPtr_groupName;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_InputAction_ActionFlags_Il2CppReferenceArray_1_InputAction_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_InputAction_ActionFlags_Il2CppReferenceArray_1_InputAction_Int32_String_0;

		private static readonly System.IntPtr NativeMethodInfoPtr_HasFlag_Public_Boolean_ActionFlags_0;

		private static readonly System.IntPtr NativeMethodInfoPtr_ToString_Public_Virtual_String_0;

		public unsafe InputAction action
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_action);
				System.IntPtr intPtr = *(System.IntPtr*)num;
				return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<InputAction>(intPtr) : null;
			}
			set
			{
				System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_action)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)inputAction));
			}
		}

		public unsafe ActionFlags configuration
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_configuration);
				return *(ActionFlags*)num;
			}
			set
			{
				*(ActionFlags*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_configuration)) = actionFlags;
			}
		}

		public unsafe Il2CppReferenceArray<InputAction> allowedConflicts
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_allowedConflicts);
				System.IntPtr intPtr = *(System.IntPtr*)num;
				return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppReferenceArray<InputAction>>(intPtr) : null;
			}
			set
			{
				System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_allowedConflicts)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)val));
			}
		}

		public unsafe int groupId
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_groupId);
				return *(int*)num;
			}
			set
			{
				*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_groupId)) = num;
			}
		}

		public unsafe string groupName
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_groupName);
				return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
			}
			set
			{
				System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_groupName)), IL2CPP.ManagedStringToIl2Cpp(text));
			}
		}

		static ActionData()
		{
			Il2CppClassPointerStore<ActionData>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "ActionData");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<ActionData>.NativeClassPtr);
			NativeFieldInfoPtr_action = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionData>.NativeClassPtr, "action");
			NativeFieldInfoPtr_configuration = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionData>.NativeClassPtr, "configuration");
			NativeFieldInfoPtr_allowedConflicts = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionData>.NativeClassPtr, "allowedConflicts");
			NativeFieldInfoPtr_groupId = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionData>.NativeClassPtr, "groupId");
			NativeFieldInfoPtr_groupName = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionData>.NativeClassPtr, "groupName");
			NativeMethodInfoPtr__ctor_Public_Void_InputAction_ActionFlags_Il2CppReferenceArray_1_InputAction_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ActionData>.NativeClassPtr, 100682840);
			NativeMethodInfoPtr__ctor_Public_Void_InputAction_ActionFlags_Il2CppReferenceArray_1_InputAction_Int32_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ActionData>.NativeClassPtr, 100682841);
			NativeMethodInfoPtr_HasFlag_Public_Boolean_ActionFlags_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ActionData>.NativeClassPtr, 100682842);
			NativeMethodInfoPtr_ToString_Public_Virtual_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ActionData>.NativeClassPtr, 100682843);
		}

		[CallerCount(3)]
		[CachedScanResults(RefRangeStart = 609585, RefRangeEnd = 609588, XrefRangeStart = 609582, XrefRangeEnd = 609585, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe ActionData(InputAction action, ActionFlags configuration, Il2CppReferenceArray<InputAction> allowedConflicts)
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ActionData>.NativeClassPtr))
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[3];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
			*(ActionFlags**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &configuration;
			*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)allowedConflicts);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_InputAction_ActionFlags_Il2CppReferenceArray_1_InputAction_0, IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this)), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(1)]
		[CachedScanResults(RefRangeStart = 609591, RefRangeEnd = 609592, XrefRangeStart = 609588, XrefRangeEnd = 609591, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe ActionData(InputAction action, ActionFlags configuration, Il2CppReferenceArray<InputAction> allowedConflicts, int groupId, string groupName)
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ActionData>.NativeClassPtr))
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[5];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
			*(ActionFlags**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &configuration;
			*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)allowedConflicts);
			*(int**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &groupId;
			*(System.IntPtr*)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(groupName);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_InputAction_ActionFlags_Il2CppReferenceArray_1_InputAction_Int32_String_0, IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this)), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(13)]
		[CachedScanResults(RefRangeStart = 609592, RefRangeEnd = 609605, XrefRangeStart = 609592, XrefRangeEnd = 609592, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe bool HasFlag(ActionFlags flag)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = (nint)(&flag);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HasFlag_Public_Boolean_ActionFlags_0, IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this)), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609605, XrefRangeEnd = 609613, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe override string ToString()
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ToString_Public_Virtual_String_0, IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this)), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return IL2CPP.Il2CppStringToManaged(intPtr);
		}

		public ActionData(System.IntPtr pointer)
			: base(pointer)
		{
		}

		public ActionData()
			: base(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ActionData>.NativeClassPtr))
		{
		}
	}

	public sealed class ActionModification : Il2CppSystem.ValueType
	{
		private static readonly System.IntPtr NativeFieldInfoPtr_action;

		private static readonly System.IntPtr NativeFieldInfoPtr_modification;

		private static readonly System.IntPtr NativeFieldInfoPtr_bindingIndex;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_InputAction_ModificationFlag_Int32_0;

		public unsafe InputAction action
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_action);
				System.IntPtr intPtr = *(System.IntPtr*)num;
				return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<InputAction>(intPtr) : null;
			}
			set
			{
				System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_action)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)inputAction));
			}
		}

		public unsafe ModificationFlag modification
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_modification);
				return *(ModificationFlag*)num;
			}
			set
			{
				*(ModificationFlag*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_modification)) = modificationFlag;
			}
		}

		public unsafe int bindingIndex
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_bindingIndex);
				return *(int*)num;
			}
			set
			{
				*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_bindingIndex)) = num;
			}
		}

		static ActionModification()
		{
			Il2CppClassPointerStore<ActionModification>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "ActionModification");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<ActionModification>.NativeClassPtr);
			NativeFieldInfoPtr_action = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionModification>.NativeClassPtr, "action");
			NativeFieldInfoPtr_modification = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionModification>.NativeClassPtr, "modification");
			NativeFieldInfoPtr_bindingIndex = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionModification>.NativeClassPtr, "bindingIndex");
			NativeMethodInfoPtr__ctor_Public_Void_InputAction_ModificationFlag_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ActionModification>.NativeClassPtr, 100682844);
		}

		[CallerCount(98)]
		[CachedScanResults(RefRangeStart = 187377, RefRangeEnd = 187475, XrefRangeStart = 187377, XrefRangeEnd = 187475, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe ActionModification(InputAction action, ModificationFlag modification, int bindingIndex)
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ActionModification>.NativeClassPtr))
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[3];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
			*(ModificationFlag**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &modification;
			*(int**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &bindingIndex;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_InputAction_ModificationFlag_Int32_0, IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this)), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		public ActionModification(System.IntPtr pointer)
			: base(pointer)
		{
		}

		public ActionModification()
			: base(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ActionModification>.NativeClassPtr))
		{
		}
	}

	[System.Serializable]
	public sealed class ActionConfiguration : Il2CppSystem.ValueType
	{
		private static readonly System.IntPtr NativeFieldInfoPtr_name;

		private static readonly System.IntPtr NativeFieldInfoPtr_actionReference;

		private static readonly System.IntPtr NativeFieldInfoPtr_configuration;

		private static readonly System.IntPtr NativeFieldInfoPtr_allowedConflicts;

		private static readonly System.IntPtr NativeFieldInfoPtr_groupId;

		private static readonly System.IntPtr NativeFieldInfoPtr_groupName;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_InputActionReference_ActionFlags_Il2CppReferenceArray_1_InputActionReference_Int32_String_0;

		public unsafe string name
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_name);
				return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
			}
			set
			{
				System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_name)), IL2CPP.ManagedStringToIl2Cpp(text));
			}
		}

		public unsafe InputActionReference actionReference
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_actionReference);
				System.IntPtr intPtr = *(System.IntPtr*)num;
				return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<InputActionReference>(intPtr) : null;
			}
			set
			{
				System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_actionReference)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)inputActionReference));
			}
		}

		public unsafe ActionFlags configuration
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_configuration);
				return *(ActionFlags*)num;
			}
			set
			{
				*(ActionFlags*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_configuration)) = actionFlags;
			}
		}

		public unsafe Il2CppReferenceArray<InputActionReference> allowedConflicts
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_allowedConflicts);
				System.IntPtr intPtr = *(System.IntPtr*)num;
				return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppReferenceArray<InputActionReference>>(intPtr) : null;
			}
			set
			{
				System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_allowedConflicts)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)val));
			}
		}

		public unsafe int groupId
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_groupId);
				return *(int*)num;
			}
			set
			{
				*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_groupId)) = num;
			}
		}

		public unsafe string groupName
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_groupName);
				return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
			}
			set
			{
				System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_groupName)), IL2CPP.ManagedStringToIl2Cpp(text));
			}
		}

		static ActionConfiguration()
		{
			Il2CppClassPointerStore<ActionConfiguration>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "ActionConfiguration");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<ActionConfiguration>.NativeClassPtr);
			NativeFieldInfoPtr_name = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionConfiguration>.NativeClassPtr, "name");
			NativeFieldInfoPtr_actionReference = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionConfiguration>.NativeClassPtr, "actionReference");
			NativeFieldInfoPtr_configuration = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionConfiguration>.NativeClassPtr, "configuration");
			NativeFieldInfoPtr_allowedConflicts = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionConfiguration>.NativeClassPtr, "allowedConflicts");
			NativeFieldInfoPtr_groupId = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionConfiguration>.NativeClassPtr, "groupId");
			NativeFieldInfoPtr_groupName = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ActionConfiguration>.NativeClassPtr, "groupName");
			NativeMethodInfoPtr__ctor_Public_Void_InputActionReference_ActionFlags_Il2CppReferenceArray_1_InputActionReference_Int32_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ActionConfiguration>.NativeClassPtr, 100682845);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609613, XrefRangeEnd = 609619, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe ActionConfiguration(InputActionReference action, ActionFlags configuration, Il2CppReferenceArray<InputActionReference> allowedConflicts, int groupId = 0, string groupName = null)
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ActionConfiguration>.NativeClassPtr))
		{
			System.IntPtr* ptr = stackalloc System.IntPtr[5];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
			*(ActionFlags**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &configuration;
			*(System.IntPtr*)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)allowedConflicts);
			*(int**)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = &groupId;
			*(System.IntPtr*)((byte*)ptr + checked((nuint)4u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(groupName);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_InputActionReference_ActionFlags_Il2CppReferenceArray_1_InputActionReference_Int32_String_0, IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this)), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		public ActionConfiguration(System.IntPtr pointer)
			: base(pointer)
		{
		}

		public ActionConfiguration()
			: base(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ActionConfiguration>.NativeClassPtr))
		{
		}
	}

	[System.Serializable]
	[ObfuscatedName("Silica.GameInputAsset+<>c")]
	public sealed class __c : Il2CppSystem.Object
	{
		private static readonly System.IntPtr NativeFieldInfoPtr___9;

		private static readonly System.IntPtr NativeFieldInfoPtr___9__23_0;

		private static readonly System.IntPtr NativeFieldInfoPtr___9__23_1;

		private static readonly System.IntPtr NativeFieldInfoPtr___9__27_1;

		private static readonly System.IntPtr NativeFieldInfoPtr___9__27_3;

		private static readonly System.IntPtr NativeFieldInfoPtr___9__27_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__Initialize_b__23_0_Internal_Boolean_ActionConfiguration_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__Initialize_b__23_1_Internal_Boolean_ActionConfiguration_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__GenerateConfiguration_b__27_1_Internal_Boolean_InputActionReference_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__GenerateConfiguration_b__27_3_Internal_Boolean_InputAction_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__GenerateConfiguration_b__27_0_Internal_Boolean_InputActionMap_0;

		public unsafe static __c __9
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr___9, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<__c>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr___9, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)_c));
			}
		}

		public unsafe static Il2CppSystem.Func<ActionConfiguration, bool> __9__23_0
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr___9__23_0, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Func<ActionConfiguration, bool>>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr___9__23_0, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)func));
			}
		}

		public unsafe static Il2CppSystem.Func<ActionConfiguration, bool> __9__23_1
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr___9__23_1, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Func<ActionConfiguration, bool>>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr___9__23_1, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)func));
			}
		}

		public unsafe static Il2CppSystem.Func<InputActionReference, bool> __9__27_1
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr___9__27_1, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Func<InputActionReference, bool>>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr___9__27_1, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)func));
			}
		}

		public unsafe static Il2CppSystem.Func<InputAction, bool> __9__27_3
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr___9__27_3, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Func<InputAction, bool>>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr___9__27_3, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)func));
			}
		}

		public unsafe static Il2CppSystem.Func<InputActionMap, bool> __9__27_0
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr___9__27_0, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Func<InputActionMap, bool>>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr___9__27_0, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)func));
			}
		}

		static __c()
		{
			Il2CppClassPointerStore<__c>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "<>c");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<__c>.NativeClassPtr);
			NativeFieldInfoPtr___9 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c>.NativeClassPtr, "<>9");
			NativeFieldInfoPtr___9__23_0 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c>.NativeClassPtr, "<>9__23_0");
			NativeFieldInfoPtr___9__23_1 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c>.NativeClassPtr, "<>9__23_1");
			NativeFieldInfoPtr___9__27_1 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c>.NativeClassPtr, "<>9__27_1");
			NativeFieldInfoPtr___9__27_3 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c>.NativeClassPtr, "<>9__27_3");
			NativeFieldInfoPtr___9__27_0 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c>.NativeClassPtr, "<>9__27_0");
			NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c>.NativeClassPtr, 100682847);
			NativeMethodInfoPtr__Initialize_b__23_0_Internal_Boolean_ActionConfiguration_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c>.NativeClassPtr, 100682848);
			NativeMethodInfoPtr__Initialize_b__23_1_Internal_Boolean_ActionConfiguration_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c>.NativeClassPtr, 100682849);
			NativeMethodInfoPtr__GenerateConfiguration_b__27_1_Internal_Boolean_InputActionReference_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c>.NativeClassPtr, 100682850);
			NativeMethodInfoPtr__GenerateConfiguration_b__27_3_Internal_Boolean_InputAction_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c>.NativeClassPtr, 100682851);
			NativeMethodInfoPtr__GenerateConfiguration_b__27_0_Internal_Boolean_InputActionMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c>.NativeClassPtr, 100682852);
		}

		[CallerCount(2477)]
		[CachedScanResults(RefRangeStart = 412, RefRangeEnd = 2889, XrefRangeStart = 412, XrefRangeEnd = 2889, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe __c()
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<__c>.NativeClassPtr))
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609619, XrefRangeEnd = 609624, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe bool _Initialize_b__23_0(ActionConfiguration x)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)x));
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__Initialize_b__23_0_Internal_Boolean_ActionConfiguration_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609624, XrefRangeEnd = 609629, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe bool _Initialize_b__23_1(ActionConfiguration x)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)x));
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__Initialize_b__23_1_Internal_Boolean_ActionConfiguration_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609629, XrefRangeEnd = 609633, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe bool _GenerateConfiguration_b__27_1(InputActionReference x)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)x);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__GenerateConfiguration_b__27_1_Internal_Boolean_InputActionReference_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		[CallerCount(0)]
		public unsafe bool _GenerateConfiguration_b__27_3(InputAction x)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)x);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__GenerateConfiguration_b__27_3_Internal_Boolean_InputAction_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609633, XrefRangeEnd = 609637, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe bool _GenerateConfiguration_b__27_0(InputActionMap x)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)x);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__GenerateConfiguration_b__27_0_Internal_Boolean_InputActionMap_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		public __c(System.IntPtr pointer)
			: base(pointer)
		{
		}
	}

	[ObfuscatedName("Silica.GameInputAsset+<>c__DisplayClass25_0")]
	public sealed class __c__DisplayClass25_0 : Il2CppSystem.Object
	{
		private static readonly System.IntPtr NativeFieldInfoPtr_conflict;

		private static readonly System.IntPtr NativeFieldInfoPtr_conflictIndex;

		private static readonly System.IntPtr NativeFieldInfoPtr___9__0;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__GetModifications_b__0_Internal_Boolean_ActionConflict_0;

		public unsafe KeyValuePair<Il2CppSystem.Guid, List<ActionConflict>> conflict
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_conflict);
				return new KeyValuePair<Il2CppSystem.Guid, List<ActionConflict>>(IL2CPP.il2cpp_value_box(Il2CppClassPointerStore<KeyValuePair<Il2CppSystem.Guid, List<ActionConflict>>>.NativeClassPtr, (System.IntPtr)num));
			}
			set
			{
				// IL cpblk instruction
				Unsafe.CopyBlock((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_conflict), IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)keyValuePair)), IL2CPP.il2cpp_class_value_size(Il2CppClassPointerStore<KeyValuePair<Il2CppSystem.Guid, List<ActionConflict>>>.NativeClassPtr, ref *(uint*)null));
			}
		}

		public unsafe int conflictIndex
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_conflictIndex);
				return *(int*)num;
			}
			set
			{
				*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_conflictIndex)) = num;
			}
		}

		public unsafe Il2CppSystem.Predicate<ActionConflict> __9__0
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr___9__0);
				System.IntPtr intPtr = *(System.IntPtr*)num;
				return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Predicate<ActionConflict>>(intPtr) : null;
			}
			set
			{
				System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr___9__0)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)predicate));
			}
		}

		static __c__DisplayClass25_0()
		{
			Il2CppClassPointerStore<__c__DisplayClass25_0>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "<>c__DisplayClass25_0");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<__c__DisplayClass25_0>.NativeClassPtr);
			NativeFieldInfoPtr_conflict = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c__DisplayClass25_0>.NativeClassPtr, "conflict");
			NativeFieldInfoPtr_conflictIndex = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c__DisplayClass25_0>.NativeClassPtr, "conflictIndex");
			NativeFieldInfoPtr___9__0 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c__DisplayClass25_0>.NativeClassPtr, "<>9__0");
			NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass25_0>.NativeClassPtr, 100682853);
			NativeMethodInfoPtr__GetModifications_b__0_Internal_Boolean_ActionConflict_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass25_0>.NativeClassPtr, 100682854);
		}

		[CallerCount(2477)]
		[CachedScanResults(RefRangeStart = 412, RefRangeEnd = 2889, XrefRangeStart = 412, XrefRangeEnd = 2889, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe __c__DisplayClass25_0()
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<__c__DisplayClass25_0>.NativeClassPtr))
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609637, XrefRangeEnd = 609641, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe bool _GetModifications_b__0(ActionConflict x)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)x));
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__GetModifications_b__0_Internal_Boolean_ActionConflict_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		public __c__DisplayClass25_0(System.IntPtr pointer)
			: base(pointer)
		{
		}
	}

	private static readonly System.IntPtr NativeFieldInfoPtr_schemeNameOverrides;

	private static readonly System.IntPtr NativeFieldInfoPtr_actionMapNameOverrides;

	private static readonly System.IntPtr NativeFieldInfoPtr_nameOverrideByPath;

	private static readonly System.IntPtr NativeFieldInfoPtr_actionData;

	private static readonly System.IntPtr NativeFieldInfoPtr_globalExclusiveActions;

	private static readonly System.IntPtr NativeFieldInfoPtr_actionConfigurations;

	private static readonly System.IntPtr NativeFieldInfoPtr_mouseSensitivityMax;

	private static readonly System.IntPtr NativeFieldInfoPtr_gamepadSentivityMax;

	private static readonly System.IntPtr NativeFieldInfoPtr_debugOutput;

	private static readonly System.IntPtr NativeFieldInfoPtr__DisplayStringOverrides_k__BackingField;

	private static readonly System.IntPtr NativeFieldInfoPtr__currentActions_k__BackingField;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_actionMaps_Public_get_ReadOnlyArray_1_InputActionMap_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_controlSchemes_Public_get_ReadOnlyArray_1_InputControlScheme_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_ActionConfigurations_Public_get_Il2CppReferenceArray_1_ActionConfiguration_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_DisplayStringOverrides_Public_get_Dictionary_2_String_Dictionary_2_String_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_DisplayStringOverrides_Private_set_Void_Dictionary_2_String_Dictionary_2_String_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_currentActions_Public_get_SilicaInputActions_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_currentActions_Private_set_Void_SilicaInputActions_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Initialize_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetModifications_Public_List_1_ActionModification_InputAction_InputBinding_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetModifications_Private_List_1_ActionModification_InputAction_InputBinding_Int32_HashSet_1_Guid_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_LoadBindings_Public_Boolean_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GenerateConfiguration_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ClearConfiguration_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ClearNameOverrides_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_Cleanup_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetConfiguration_Public_ActionData_InputAction_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetParent_Public_ActionData_InputAction_0;

	private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr__GenerateConfiguration_b__27_2_Private_InputAction_InputActionReference_0;

	public unsafe Dictionary<string, string> schemeNameOverrides
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_schemeNameOverrides);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dictionary<string, string>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_schemeNameOverrides)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dictionary));
		}
	}

	public unsafe Dictionary<string, string> actionMapNameOverrides
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_actionMapNameOverrides);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dictionary<string, string>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_actionMapNameOverrides)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dictionary));
		}
	}

	public unsafe Dictionary<string, Dictionary<string, Dictionary<string, string>>> nameOverrideByPath
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_nameOverrideByPath);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_nameOverrideByPath)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dictionary));
		}
	}

	public unsafe Dictionary<Il2CppSystem.Guid, Dictionary<Il2CppSystem.Guid, ActionData>> actionData
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_actionData);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dictionary<Il2CppSystem.Guid, Dictionary<Il2CppSystem.Guid, ActionData>>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_actionData)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dictionary));
		}
	}

	public unsafe Dictionary<Il2CppSystem.Guid, ActionData> globalExclusiveActions
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_globalExclusiveActions);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dictionary<Il2CppSystem.Guid, ActionData>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_globalExclusiveActions)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dictionary));
		}
	}

	public unsafe List<ActionConfiguration> actionConfigurations
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_actionConfigurations);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<ActionConfiguration>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_actionConfigurations)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe float mouseSensitivityMax
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_mouseSensitivityMax);
			return *(float*)num;
		}
		set
		{
			*(float*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_mouseSensitivityMax)) = num;
		}
	}

	public unsafe float gamepadSentivityMax
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_gamepadSentivityMax);
			return *(float*)num;
		}
		set
		{
			*(float*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_gamepadSentivityMax)) = num;
		}
	}

	public unsafe bool debugOutput
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_debugOutput);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_debugOutput)) = flag;
		}
	}

	public unsafe Dictionary<string, Dictionary<string, string>> _DisplayStringOverrides_k__BackingField
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr__DisplayStringOverrides_k__BackingField);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dictionary<string, Dictionary<string, string>>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr__DisplayStringOverrides_k__BackingField)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dictionary));
		}
	}

	public unsafe SilicaInputActions _currentActions_k__BackingField
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr__currentActions_k__BackingField);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<SilicaInputActions>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr__currentActions_k__BackingField)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)silicaInputActions));
		}
	}

	public unsafe ReadOnlyArray<InputActionMap> actionMaps
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609641, XrefRangeEnd = 609642, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr);
			System.IntPtr pointer = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_actionMaps_Public_get_ReadOnlyArray_1_InputActionMap_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr);
			Il2CppException.RaiseExceptionIfNecessary(intPtr);
			return new ReadOnlyArray<InputActionMap>(pointer);
		}
	}

	public unsafe ReadOnlyArray<InputControlScheme> controlSchemes
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609642, XrefRangeEnd = 609643, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr);
			System.IntPtr pointer = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_controlSchemes_Public_get_ReadOnlyArray_1_InputControlScheme_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr);
			Il2CppException.RaiseExceptionIfNecessary(intPtr);
			return new ReadOnlyArray<InputControlScheme>(pointer);
		}
	}

	public unsafe Il2CppReferenceArray<ActionConfiguration> ActionConfigurations
	{
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 609643, XrefRangeEnd = 609647, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_ActionConfigurations_Public_get_Il2CppReferenceArray_1_ActionConfiguration_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppReferenceArray<ActionConfiguration>>(intPtr) : null;
		}
	}

	public unsafe Dictionary<string, Dictionary<string, string>> DisplayStringOverrides
	{
		[CallerCount(1)]
		[CachedScanResults(RefRangeStart = 32772, RefRangeEnd = 32773, XrefRangeStart = 32772, XrefRangeEnd = 32773, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_DisplayStringOverrides_Public_get_Dictionary_2_String_Dictionary_2_String_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dictionary<string, Dictionary<string, string>>>(intPtr) : null;
		}
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 0, XrefRangeEnd = 0, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_DisplayStringOverrides_Private_set_Void_Dictionary_2_String_Dictionary_2_String_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	public unsafe SilicaInputActions currentActions
	{
		[CallerCount(45)]
		[CachedScanResults(RefRangeStart = 32500, RefRangeEnd = 32545, XrefRangeStart = 32500, XrefRangeEnd = 32545, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_currentActions_Public_get_SilicaInputActions_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<SilicaInputActions>(intPtr) : null;
		}
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 0, XrefRangeEnd = 0, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_currentActions_Private_set_Void_SilicaInputActions_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	static GameInputAsset()
	{
		Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr = IL2CPP.GetIl2CppClass("SilicaCore.dll", "Silica", "GameInputAsset");
		IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr);
		NativeFieldInfoPtr_schemeNameOverrides = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "schemeNameOverrides");
		NativeFieldInfoPtr_actionMapNameOverrides = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "actionMapNameOverrides");
		NativeFieldInfoPtr_nameOverrideByPath = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "nameOverrideByPath");
		NativeFieldInfoPtr_actionData = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "actionData");
		NativeFieldInfoPtr_globalExclusiveActions = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "globalExclusiveActions");
		NativeFieldInfoPtr_actionConfigurations = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "actionConfigurations");
		NativeFieldInfoPtr_mouseSensitivityMax = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "mouseSensitivityMax");
		NativeFieldInfoPtr_gamepadSentivityMax = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "gamepadSentivityMax");
		NativeFieldInfoPtr_debugOutput = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "debugOutput");
		NativeFieldInfoPtr__DisplayStringOverrides_k__BackingField = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "<DisplayStringOverrides>k__BackingField");
		NativeFieldInfoPtr__currentActions_k__BackingField = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, "<currentActions>k__BackingField");
		NativeMethodInfoPtr_get_actionMaps_Public_get_ReadOnlyArray_1_InputActionMap_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682808);
		NativeMethodInfoPtr_get_controlSchemes_Public_get_ReadOnlyArray_1_InputControlScheme_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682809);
		NativeMethodInfoPtr_get_ActionConfigurations_Public_get_Il2CppReferenceArray_1_ActionConfiguration_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682810);
		NativeMethodInfoPtr_get_DisplayStringOverrides_Public_get_Dictionary_2_String_Dictionary_2_String_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682811);
		NativeMethodInfoPtr_set_DisplayStringOverrides_Private_set_Void_Dictionary_2_String_Dictionary_2_String_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682812);
		NativeMethodInfoPtr_get_currentActions_Public_get_SilicaInputActions_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682813);
		NativeMethodInfoPtr_set_currentActions_Private_set_Void_SilicaInputActions_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682814);
		NativeMethodInfoPtr_Initialize_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682815);
		NativeMethodInfoPtr_GetModifications_Public_List_1_ActionModification_InputAction_InputBinding_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682816);
		NativeMethodInfoPtr_GetModifications_Private_List_1_ActionModification_InputAction_InputBinding_Int32_HashSet_1_Guid_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682817);
		NativeMethodInfoPtr_LoadBindings_Public_Boolean_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682818);
		NativeMethodInfoPtr_GenerateConfiguration_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682819);
		NativeMethodInfoPtr_ClearConfiguration_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682820);
		NativeMethodInfoPtr_ClearNameOverrides_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682821);
		NativeMethodInfoPtr_Cleanup_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682822);
		NativeMethodInfoPtr_GetConfiguration_Public_ActionData_InputAction_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682823);
		NativeMethodInfoPtr_GetParent_Public_ActionData_InputAction_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682824);
		NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682825);
		NativeMethodInfoPtr__GenerateConfiguration_b__27_2_Private_InputAction_InputActionReference_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr, 100682826);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 609878, RefRangeEnd = 609879, XrefRangeStart = 609647, XrefRangeEnd = 609878, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void Initialize()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Initialize_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 609890, RefRangeEnd = 609894, XrefRangeStart = 609879, XrefRangeEnd = 609890, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe List<ActionModification> GetModifications(InputAction action, InputBinding binding, int bindingIndex)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)binding));
		*(int**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &bindingIndex;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetModifications_Public_List_1_ActionModification_InputAction_InputBinding_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<ActionModification>>(intPtr) : null;
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 610427, RefRangeEnd = 610429, XrefRangeStart = 609894, XrefRangeEnd = 610427, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe List<ActionModification> GetModifications(InputAction action, InputBinding binding, int bindingIndex, HashSet<Il2CppSystem.Guid> expanded)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[4];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)binding));
		*(int**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &bindingIndex;
		*(System.IntPtr*)((byte*)ptr + checked((nuint)3u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)expanded);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetModifications_Private_List_1_ActionModification_InputAction_InputBinding_Int32_HashSet_1_Guid_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<ActionModification>>(intPtr) : null;
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 610429, XrefRangeEnd = 610431, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool LoadBindings(string bindings)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(bindings);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_LoadBindings_Public_Boolean_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 610596, RefRangeEnd = 610598, XrefRangeStart = 610431, XrefRangeEnd = 610596, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void GenerateConfiguration()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GenerateConfiguration_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 610598, XrefRangeEnd = 610600, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void ClearConfiguration()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ClearConfiguration_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 610600, XrefRangeEnd = 610603, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void ClearNameOverrides()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ClearNameOverrides_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 610608, RefRangeEnd = 610609, XrefRangeStart = 610603, XrefRangeEnd = 610608, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void Cleanup()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_Cleanup_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(11)]
	[CachedScanResults(RefRangeStart = 610632, RefRangeEnd = 610643, XrefRangeStart = 610609, XrefRangeEnd = 610632, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe ActionData GetConfiguration(InputAction action)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
		Unsafe.SkipInit(out System.IntPtr intPtr);
		System.IntPtr pointer = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetConfiguration_Public_ActionData_InputAction_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr);
		Il2CppException.RaiseExceptionIfNecessary(intPtr);
		return new ActionData(pointer);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 610675, RefRangeEnd = 610677, XrefRangeStart = 610643, XrefRangeEnd = 610675, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe ActionData GetParent(InputAction action)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action);
		Unsafe.SkipInit(out System.IntPtr intPtr);
		System.IntPtr pointer = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetParent_Public_ActionData_InputAction_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr);
		Il2CppException.RaiseExceptionIfNecessary(intPtr);
		return new ActionData(pointer);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 610677, XrefRangeEnd = 610685, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe GameInputAsset()
		: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<GameInputAsset>.NativeClassPtr))
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 610685, XrefRangeEnd = 610689, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe InputAction _GenerateConfiguration_b__27_2(InputActionReference x)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)x);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__GenerateConfiguration_b__27_2_Private_InputAction_InputActionReference_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<InputAction>(intPtr) : null;
	}

	public GameInputAsset(System.IntPtr pointer)
		: base(pointer)
	{
	}
}
