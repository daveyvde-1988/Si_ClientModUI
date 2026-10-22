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
    private bool panelOpen,confirmDismiss,hudExpanded;
    private string menuTab="formations";
    private int confirmPurchase;
    private int panelTeam=-1;
    private bool panelCommander;
    private string function="follow",pendingId="",panelMessage="";
    private string pendingBody="";
    private bool retried,pendingAction;
    private PanelPage page;
    private bool serverAdmin;
    private readonly System.Collections.Generic.Dictionary<int,PanelPage> incoming=new();
    private readonly System.Collections.Generic.Dictionary<string,PanelPage> catalogCache=new();
    private string catalogContext="";
    private void EnsureCatalogCache()
    {
        string context=cache.Nonce+":"+panelTeam+":"+panelCommander;
        if(context==catalogContext)return;catalogContext=context;catalogCache.Clear();
    }
    private void SchedulePagePoll()=>nextPagePoll=clock.Elapsed.TotalSeconds+(function=="commander"?1:function=="admin"?5:HudProtocol.IsFormation(function)?10:30);
    private bool ReceiveCatalogUnchanged(string body)
    {
        if(!HudProtocol.TryCatalogUnchanged(body,out var scope))return false;
        if(ReceiveSidebarUnchanged(scope))return true;
        if(!panelOpen||scope.Nonce!=cache.Nonce||scope.Id!=pendingId||scope.Team!=panelTeam||scope.Commander!=panelCommander||scope.Function!=function)return true;
        if(page==null||HudProtocol.CatalogFingerprint(page)!=scope.Known)
        {page=null;pendingId="";incoming.Clear();nextPagePoll=clock.Elapsed.TotalSeconds;return true;}
        pendingId="";incoming.Clear();SchedulePagePoll();return true;
    }
    private double pendingAt,nextPagePoll,releaseUntil,controlReadyAt;
    private CursorLockMode priorLock;
    private bool priorCursor;
    private MelonPreferences_Entry<int> fontPreference;
    private bool BlocksGameplay=>panelOpen||!NativeMenu()&&!NativeTextInput()&&(SidebarPointerCaptured||NotificationPointerCaptured||StatusPointerCaptured||statusPickerOpen)||clock.Elapsed.TotalSeconds<releaseUntil;

    private static bool NativeTextInput()=>UIManager.TextInputActive||DebugConsole.IsOpen;
    private static bool NativeMenu()=>Game.IsPauseMenuActive||Game.InMainMenu||(MenuManager.Instance!=null&&MenuManager.Instance.GetCurrent()!=null);
    private void TogglePanel()
    {
        if(panelOpen){ClosePanel();return;}
        if(!drawingEnabled){ResetAppearance();drawingEnabled=true;nextDrawRetry=0;Write("Renderer recovery requested with F6.");}
        var p=Player.CurrentPlayer;
        if(!drawingEnabled||NativeMenu()||NativeTextInput()||p==null||p.Team==null||!NetworkGameClient.GetInServer()||Game.GetIsServer())return;
        if(!hudExpanded){hudExpanded=true;return;}
        // Establish the same context used by the regular status query before first open.
        string peer=NetworkGameClient.GetServerID().SteamID.m_SteamID.ToString();
        cache.SetContext(peer,p.GetInstanceID(),p.Team.Index);
        SyncBalanceContext();
        priorCursor=Cursor.visible;priorLock=Cursor.lockState;
        panelTeam=p.Team.Index;panelCommander=p.IsCommander;menuTab="formations";function=panelCommander?"commander":"follow";confirmPurchase=0;
        var mode=GameMode.CurrentGameMode;
        lastMode=mode==null?0:mode.GetInstanceID();lastGameTime=mode==null?0:mode.GameTime;roundPlaying=mode!=null&&mode.GameBegun&&!mode.GameOver;
        panelOpen=true;page=null;incoming.Clear();pendingId="";confirmDismiss=false;panelMessage="Loading server formations...";
        nextPagePoll=clock.Elapsed.TotalSeconds;
        Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
        GameInput.UpdateInput();
        SendHello(p);RequestPage(0);
        Write("Mod menu opened.");
    }
    private void ClosePanel()
    {
        statusPickerOpen=false;EndWindowDrag();
        hudExpanded=false;confirmPurchase=0;
        if(!panelOpen)return;
        EndWindowDrag();
        panelOpen=false;page=null;incoming.Clear();pendingId="";confirmDismiss=false;releaseUntil=clock.Elapsed.TotalSeconds+0.2;
        ClearBalanceEdits();
        try
        {
            if(!Application.isFocused){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
            else if(!NativeMenu()&&!NativeTextInput())
            {
                bool show=GameMode.CurrentGameMode==null?priorCursor:GameMode.CurrentGameMode.GetShowCursor();
                Cursor.visible=show;Cursor.lockState=show?CursorLockMode.None:priorLock;
            }
            GameInput.UpdateInput();
        }
        catch { }
    }
    public override void OnLateUpdate()
    {
        try
        {
            bool capture=SidebarPointerCaptured||NotificationPointerCaptured||StatusPointerCaptured||statusPickerOpen;
            if(lastSidebarCapture!=capture){lastSidebarCapture=capture;GameInput.UpdateInput();}
            if(!panelOpen)return;
            if(!Application.isFocused||NativeMenu()||NativeTextInput()){ClosePanel();return;}
            Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
        }
        catch(Exception error){ClosePanel();Error(error);}
    }
    private void SendHello(Player p)
    {
        if(!networkEnabled)return;
        SendBody(HudProtocol.Request(cache.Nonce,p.PlayerChannel,p.Team.Index));nextRequest=clock.Elapsed.TotalSeconds+5;
    }
    private void SendBody(string body)
    {
        var writer=GameByteStreamWriter.GetGameByteStreamWriter(0,"Si_SquadHUD::PanelRequest",true);
        writer.WriteByte(HudProtocol.PacketType);writer.WriteString(HudProtocol.Marker);writer.WriteString(body);
        NetworkLayer.SendPacket(writer,ENetworkPacketSend.Reliable);
    }
    private void RequestPage(int number,string action="",int option=0)
    {
        if(BalanceSection)return;
        if(!panelOpen||pendingId.Length>0||sidebarPending.Length>0)return;
        if(clock.Elapsed.TotalSeconds<controlReadyAt)return;
        if(!networkEnabled){panelMessage="Panel transport unavailable on this game build.";return;}
        var p=Player.CurrentPlayer;
        if(p==null||p.Team==null||p.Team.Index!=panelTeam||p.IsCommander!=panelCommander){ClosePanel();return;}
        EnsureCatalogCache();
        if(page==null&&action.Length==0&&catalogCache.TryGetValue(function,out var cached))page=cached;
        var r=new PanelRequest{Nonce=cache.Nonce,Id=Guid.NewGuid().ToString("N"),Channel=p.PlayerChannel,Team=panelTeam,
            Commander=panelCommander,Function=function,Page=HudProtocol.FullCatalog,Action=action,Option=option,Revision=page?.Revision??"",Known=action.Length==0&&page!=null?HudProtocol.CatalogFingerprint(page):""};
        pendingAction=action.Length>0;pendingId=r.Id;pendingAt=clock.Elapsed.TotalSeconds;
        incoming.Clear();
        retried=false;
        controlReadyAt=pendingAt+0.35;
        try{pendingBody=HudProtocol.Control(r);SendBody(pendingBody);if(action.Length>0)panelMessage="Applying selection on server...";else if(page==null)panelMessage="Loading formations...";}
        catch(Exception error){pendingId="";panelMessage="Request unavailable; refresh the panel.";Error(error);}
    }
    private void ReceivePage(PanelPage result)
    {
        if(ReceiveSidebar(result))return;
        if(!panelOpen||result.Nonce!=cache.Nonce||result.Id!=pendingId||result.Team!=panelTeam||result.Commander!=panelCommander||result.Function!=function)return;
        if(incoming.Count>0)
        {
            var first=incoming.Values.First();
            if(first.Revision!=result.Revision||first.Pages!=result.Pages||first.Admin!=result.Admin||first.Selected!=result.Selected||first.Enabled!=result.Enabled||first.Locked!=result.Locked||first.Leader!=result.Leader||first.Available!=result.Available)incoming.Clear();
        }
        incoming[result.Page]=result;
        if(incoming.Values.Sum(p=>p.Numbers.Length)>HudProtocol.MaxCatalogRows){incoming.Clear();return;}
        if(incoming.Count<result.Pages){if(page==null)panelMessage="Loading list... "+incoming.Count+" / "+result.Pages+" packets";return;}
        var parts=Enumerable.Range(0,result.Pages).Select(i=>incoming[i]).ToArray();
        var numbers=new List<int>();var names=new List<string>();var details=new List<string>();var groups=new List<string>();var data=new List<string>();
        foreach(var part in parts){numbers.AddRange(part.Numbers);names.AddRange(part.Names);details.AddRange(part.Details);groups.AddRange(part.Groups);data.AddRange(part.Data);}
        result.Numbers=numbers.ToArray();result.Names=names.ToArray();result.Details=details.ToArray();result.Groups=groups.ToArray();result.Data=data.ToArray();
        if(result.Numbers.Distinct().Count()!=result.Numbers.Length){incoming.Clear();return;}
        result.Page=0;result.Pages=1;incoming.Clear();if(!menuStateKnown)serverAdmin=result.Admin;
        if(page?.Revision!=result.Revision)confirmPurchase=0;
        page=result;pendingId="";panelMessage=result.Message;
        EnsureCatalogCache();catalogCache[function]=result;SchedulePagePoll();
        Write("PANEL reply function="+result.Function+"; selected="+result.Selected+"; available="+result.Available+"; "+result.Message);
    }
    private void MaintainPanel(Player player,double now)
    {
        if(!panelOpen)return;
        if(player.Team==null||player.Team.Index!=panelTeam||player.IsCommander!=panelCommander){ClosePanel();return;}
        if(BalanceSection)return;
        if(pendingId.Length>0&&!retried&&now-pendingAt>2)
        {retried=true;SendHello(player);SendBody(pendingBody);}
        if(pendingId.Length>0&&now-pendingAt>8)
        {pendingId="";page=null;incoming.Clear();panelMessage="Server list did not finish. Refresh to retry; no success assumed.";nextPagePoll=now+10;}
        if(pendingId.Length==0&&now>=nextPagePoll&&confirmPurchase==0)RequestPage(page?.Page??0);
    }

    [HarmonyPatch(typeof(MenuManager),nameof(MenuManager.IsAnyMenuOpened))]
    private static class MenuFocus
    {private static void Postfix(ref bool __result){if(instance?.BlocksGameplay==true)__result=true;}}
    [HarmonyPatch(typeof(GameInput),nameof(GameInput.GetPreferredInput))]
    private static class InputFocus
    {
        private static bool Prefix(ref GameInput.InputMap __result)
        {if(instance?.BlocksGameplay!=true)return true;__result=GameInput.InputMap.UI;return false;}
    }
    [HarmonyPatch(typeof(Game),nameof(Game.GetControlsEnabled))]
    private static class ControlFocus
    {private static bool Prefix(ref bool __result){if(instance?.BlocksGameplay!=true)return true;__result=false;return false;}}
    [HarmonyPatch(typeof(Game),nameof(Game.GetCameraEnabled))]
    private static class CameraFocus
    {private static bool Prefix(ref bool __result){if(instance?.BlocksGameplay!=true)return true;__result=false;return false;}}
    [HarmonyPatch(typeof(GameManager),nameof(GameManager.OnEscape))]
    private static class EscapeFocus
    {private static bool Prefix(){if(instance?.panelOpen!=true)return true;instance.ClosePanel();return false;}}
}
