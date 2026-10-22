using HarmonyLib;
using Il2CppSilica;
using UnityEngine.InputSystem;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private sealed class GuardedAction {internal InputAction Action;internal bool Enabled;}
    private readonly List<GuardedAction> guardedChatActions=new();
    private readonly HashSet<IntPtr> guardedPointers=new();
    private readonly HashSet<IntPtr> guardedMaps=new();
    private readonly Dictionary<IntPtr,int> capturedMapCounts=new();
    private bool updatingChatGuard;
    private bool restoringChatInput;
    private bool IsolatingInput=>panelOpen||sidebarHeld||guardedChatActions.Count>0&&!restoringChatInput;
    private void CaptureAsset(InputActionAsset asset)
    {
        if(asset==null)return;
        foreach(var map in asset.actionMaps)
        {
            guardedMaps.Add(map.Pointer);
            int count=map.actions.Count;
            if(capturedMapCounts.TryGetValue(map.Pointer,out int previous)&&previous==count)continue;
            capturedMapCounts[map.Pointer]=count;
            foreach(var action in map.actions)
            if(action!=null&&guardedPointers.Add(action.Pointer))
                guardedChatActions.Add(new GuardedAction{Action=action,Enabled=action.enabled});
        }
    }
    private void CaptureChatInput()
    {
        CaptureAsset(GameInput.CurrentPlayerInput?.actions);
        CaptureAsset(GameInput.CurrentAsset?.currentActions?.asset);
    }
    private void MaintainChatInputGuard()
    {
        if(updatingChatGuard)return;updatingChatGuard=true;
        try
        {
            if(panelOpen||sidebarHeld)
            {
                CaptureChatInput();
                foreach(var entry in guardedChatActions)if(entry.Action.enabled)entry.Action.Disable();
            }
            else if(guardedChatActions.Count>0)
            {
                var keyboard=Keyboard.current;
                // Wait for held keys to release before restoring native bindings.
                var mouse=Mouse.current;
                if(keyboard!=null&&keyboard.anyKey.isPressed||mouse!=null&&(mouse.leftButton.isPressed||mouse.rightButton.isPressed||mouse.middleButton.isPressed))return;
                RestoreChatInput();
            }
        }
        catch(Exception e){Error(e,"menu input isolation");}
        finally{updatingChatGuard=false;}
    }
    private void RestoreChatInput()
    {
        restoringChatInput=true;
        try
        {
        foreach(var entry in guardedChatActions)
            try{if(entry.Enabled)entry.Action.Enable();else entry.Action.Disable();}catch{}
        guardedChatActions.Clear();guardedPointers.Clear();guardedMaps.Clear();capturedMapCounts.Clear();
        }
        finally{restoringChatInput=false;}
    }
    [HarmonyPatch(typeof(GameInput),nameof(GameInput.UpdateInput))]
    private static class MenuInputGuard
    {private static void Postfix(){if(instance==null)return;if(instance.panelOpen||instance.sidebarHeld)instance.capturedMapCounts.Clear();instance.MaintainChatInputGuard();}}
    [HarmonyPatch(typeof(InputAction),nameof(InputAction.Enable))]
    private static class PreventNativeActionEnable
    {private static bool Prefix(InputAction __instance)=>instance?.IsolatingInput!=true||!instance.guardedPointers.Contains(__instance.Pointer);}
    [HarmonyPatch(typeof(InputActionMap),nameof(InputActionMap.Enable))]
    private static class PreventNativeMapEnable
    {private static bool Prefix(InputActionMap __instance)=>instance?.IsolatingInput!=true||!instance.guardedMaps.Contains(__instance.Pointer);}
    [HarmonyPatch(typeof(GameInput),nameof(GameInput.ProcessInput))]
    private static class PreventNativeProcessing
    {private static bool Prefix()=>instance?.IsolatingInput!=true;}
    [HarmonyPatch]
    private static class PreventNativeButtons
    {
        private static IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            foreach(string name in new[]{nameof(GameInput.GetButtonDown),nameof(GameInput.GetButtonUp),nameof(GameInput.GetButton),nameof(GameInput.GetToggle)})
                yield return AccessTools.Method(typeof(GameInput),name);
        }
        private static bool Prefix(ref bool __result)
        {if(instance?.IsolatingInput!=true)return true;__result=false;return false;}
    }
}

