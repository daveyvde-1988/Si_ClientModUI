using HarmonyLib;
using Il2CppSilica;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private sealed class GuardedAction {internal InputAction Action;internal bool Enabled;}
    private readonly List<GuardedAction> guardedChatActions=new();
    private bool updatingChatGuard;
    private void CaptureChatInput()
    {
        foreach(string name in new[]{"Chat","ChatChannel"})
        {
            var action=GameInput.GetAction(GameInput.InputMap.Global,name);if(action==null)continue;
            if(!guardedChatActions.Any(g=>g.Action.Pointer==action.Pointer))
                guardedChatActions.Add(new GuardedAction{Action=action,Enabled=action.enabled});
        }
    }
    private void MaintainChatInputGuard()
    {
        if(updatingChatGuard)return;
        updatingChatGuard=true;
        try
        {
            if(panelOpen)
            {
                CaptureChatInput();
                foreach(string name in new[]{"Chat","ChatChannel"})
                {
                    var action=GameInput.GetAction(GameInput.InputMap.Global,name);if(action==null)continue;
                    if(!guardedChatActions.Any(g=>g.Action.Pointer==action.Pointer))
                        guardedChatActions.Add(new GuardedAction{Action=action,Enabled=action.enabled});
                    if(action.enabled)action.Disable();
                }
            }
            else if(guardedChatActions.Count>0)
            {
                var keyboard=Keyboard.current;
                // Re-enabling an action while Enter is held can open native chat immediately.
                if(keyboard!=null&&(keyboard.enterKey.isPressed||keyboard.numpadEnterKey.isPressed))return;
                RestoreChatInput();
            }
        }
        catch(Exception e){Error(e,"chat input guard");}
        finally{updatingChatGuard=false;}
    }
    private void RestoreChatInput()
    {
        foreach(var entry in guardedChatActions)
            try{if(entry.Enabled)entry.Action.Enable();else entry.Action.Disable();}catch{}
        guardedChatActions.Clear();
    }
    [HarmonyPatch(typeof(GameInput),nameof(GameInput.UpdateInput))]
    private static class ChatInputGuard
    {
        private static void Postfix(){instance?.MaintainChatInputGuard();}
    }
}

