using HarmonyLib;
using Il2Cpp;
using Il2CppSilica;
using Il2CppSilica.UI;
using Il2CppSilica.UI.Menus;
using Il2CppDebugTools;
using MelonLoader;
using UnityEngine;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private bool panelOpen,panelCommander;
    private bool lastOverlayCapture;
    private double releaseUntil;
    private CursorLockMode priorLock;
    private bool priorCursor;
    private bool sidebarHeld,holdPriorCursor,holdNeedsRelease;
    private CursorLockMode holdPriorLock;
    private bool BlocksGameplay=>PresentationAllowed&&(panelOpen||sidebarHeld||IsolatingInput||!NativeMenu()&&!NativeTextInput()&&
        (SidebarPointerCaptured||NotificationPointerCaptured||StatusPointerCaptured||statusPickerOpen)||clock.Elapsed.TotalSeconds<releaseUntil);
    private static bool NativeTextInput()=>UIManager.TextInputActive||DebugConsole.IsOpen;
    private static bool NativeMenu()=>Game.IsPauseMenuActive||Game.InMainMenu||(MenuManager.Instance!=null&&MenuManager.Instance.GetCurrent()!=null);
    private void UpdateSidebarHold()
    {
        var keyboard=UnityEngine.InputSystem.Keyboard.current;
        bool held=keyboard?.f7Key.isPressed==true;
        if(!held)holdNeedsRelease=false;
        bool wanted=UiPolicies.HoldWanted(held,layout.HoldSidebarF7,Player.CurrentPlayer?.IsCommander!=true,
            PresentationAllowed,Application.isFocused,NativeMenu(),NativeTextInput(),panelOpen,statusPickerOpen,holdNeedsRelease);
        if(!wanted){EndSidebarHold();return;}
        if(!sidebarHeld)
        {
            EnsureWindowProfile();holdPriorCursor=Cursor.visible;holdPriorLock=Cursor.lockState;
            CaptureChatInput();sidebarHeld=true;GameInput.UpdateInput();
        }
        Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
    }
    private void EndSidebarHold()
    {
        if(!sidebarHeld)return;
        sidebarHeld=false;holdNeedsRelease=true;releaseUntil=clock.Elapsed.TotalSeconds+.2;EndWindowDrag();
        if(!Application.isFocused){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
        else if(!panelOpen&&!NativeMenu()&&!NativeTextInput()){Cursor.visible=holdPriorCursor;Cursor.lockState=holdPriorLock;}
        GameInput.UpdateInput();
    }
    private void TogglePanel()
    {
        if(panelOpen){ClosePanel();return;}
        if(!PresentationAllowed||NativeMenu()||NativeTextInput())return;
        if(sidebarHeld){holdNeedsRelease=true;EndSidebarHold();}
        EnsureSessionContext();
        if(!drawingEnabled){ResetAppearance();drawingEnabled=true;}
        RestoreMenuSelection();
        CaptureChatInput();
        priorCursor=Cursor.visible;priorLock=Cursor.lockState;panelCommander=Player.CurrentPlayer.IsCommander;
        panelOpen=true;RefreshLocalMods();Cursor.lockState=CursorLockMode.None;Cursor.visible=true;GameInput.UpdateInput();
        Write("Menu opened; isolated native input actions: "+guardedChatActions.Count);
    }
    private void ClosePanel()
    {
        SaveMenuSelection();dropdownKey="";pickerCommand=null;statusPickerOpen=false;addEntryOpen=false;sidebarMenuOpen=false;settingsMenuOpen=false;EndWindowDrag();EndFeedbackDrag();SaveWindowPosition();
        if(!panelOpen)return;
        panelOpen=false;releaseUntil=clock.Elapsed.TotalSeconds+.2;
        try
        {
            if(!Application.isFocused){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
            else if(!NativeMenu()&&!NativeTextInput())
            {Cursor.visible=GameMode.CurrentGameMode==null?priorCursor:GameMode.CurrentGameMode.GetShowCursor();Cursor.lockState=Cursor.visible?CursorLockMode.None:priorLock;}
            GameInput.UpdateInput();
        }
        catch{}
    }
    public override void OnLateUpdate()
    {
        try
        {
            UpdateSidebarHold();
            bool capture=sidebarHeld||SidebarPointerCaptured||NotificationPointerCaptured||StatusPointerCaptured||statusPickerOpen;
            if(lastOverlayCapture!=capture){lastOverlayCapture=capture;GameInput.UpdateInput();}
            if(!PresentationAllowed){if(panelOpen)ClosePanel();releaseUntil=0;if(guardedChatActions.Count>0)RestoreChatInput();return;}
            if(!panelOpen)return;
            if(!Application.isFocused||NativeMenu()||NativeTextInput())
            {Write("Menu context closed: focused="+Application.isFocused+"; nativeMenu="+NativeMenu()+"; nativeText="+NativeTextInput());ClosePanel();return;}
            Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
        }
        catch(Exception e){ClosePanel();Error(e);}
    }
    [HarmonyPatch(typeof(MenuManager),nameof(MenuManager.IsAnyMenuOpened))]
    private static class MenuFocus{private static void Postfix(ref bool __result){if(instance?.BlocksGameplay==true)__result=true;}}
    [HarmonyPatch(typeof(GameInput),nameof(GameInput.GetPreferredInput))]
    private static class InputFocus{private static bool Prefix(ref GameInput.InputMap __result){if(instance?.BlocksGameplay!=true)return true;__result=GameInput.InputMap.UI;return false;}}
    [HarmonyPatch(typeof(Game),nameof(Game.GetControlsEnabled))]
    private static class ControlFocus{private static bool Prefix(ref bool __result){if(instance?.BlocksGameplay!=true)return true;__result=false;return false;}}
    [HarmonyPatch(typeof(Game),nameof(Game.GetCameraEnabled))]
    private static class CameraFocus{private static bool Prefix(ref bool __result){if(instance?.BlocksGameplay!=true)return true;__result=false;return false;}}
    [HarmonyPatch(typeof(GameManager),nameof(GameManager.OnEscape))]
    private static class EscapeFocus{private static bool Prefix(){if(instance?.panelOpen!=true)return true;instance.ClosePanel();return false;}}
}

