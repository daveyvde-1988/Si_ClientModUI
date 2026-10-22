using System.Diagnostics;
using System.Text;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(SiSquadHud.SquadHud), "Si_ClientModUI", "0.15.0", "Davey, DrMuck")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonPlatform(MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace SiSquadHud;
public sealed partial class SquadHud : MelonMod
{
    private static SquadHud instance;
    private readonly Stopwatch clock=Stopwatch.StartNew();
    private bool keyboardEnabled=true,drawingEnabled=true,visible=true;
    private double nextPoll,nextError,nextDrawRetry;
    private string sessionContext="",panel="";
    private string sceneName="";
    private StreamWriter log;
    private static bool HasSession=>!Game.InMainMenu&&Player.CurrentPlayer!=null&&
        (Game.GetIsSingleplayer()||Game.GetIsClient()&&NetworkGameClient.GetInServer());
    private static bool PresentationAllowed=>HasSession&&ShellRules.ShowOverlay(true,Game.IsLoading||GameLevelLoader.IsLoading,
        GameVideo.IsPlaying||CinematicCamera.IsPlaying||GameMode.InCinematicMoment,
        instance?.sceneName is "Intro" or "Loading" or "MainMenu");

    public override void OnInitializeMelon()
    {
        instance=this;
        try
        {
            string folder=Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory,"Si_ModMenu");
            Directory.CreateDirectory(folder);
            log=new StreamWriter(new FileStream(Path.Combine(folder,$"session-{DateTime.UtcNow:yyyyMMddTHHmmssfffZ}.log"),FileMode.CreateNew,FileAccess.Write,FileShare.Read),new UTF8Encoding(false)){AutoFlush=true};
        }
        catch(Exception error){LoggerInstance.Warning("Session log unavailable: "+error.Message);}
        RefreshLocalMods();
        InitializeCompanion();
        Write("READY 0.15.0 - Si_ClientModUI. Optional Si_ServerModUI transport; existing Mod menu preferences and provider API retained.");
    }
    public override void OnSceneWasLoaded(int index,string name)
    {sceneName=name;ClosePanel();ResetSession();ResetCompanion();ResetAppearance();drawingEnabled=true;nextDrawRetry=nextPoll=0;Write("Scene: "+Clean(name));}

    // Observe the native presentation path after game recipient/team validation.
    [HarmonyPatch(typeof(Player),nameof(Player.LogChatMessage))]
    private static class NativeChat
    {
        private static void Prefix(string __1,bool __2,bool __3)
        {
            if(instance==null||!__3||__2||!HasSession||!Game.GetIsSingleplayer())return;
            try{instance.ObserveFeedback(__1);}catch(Exception e){instance.Error(e,"chat feedback");}
        }
    }
    [HarmonyPatch(typeof(NetworkLayer),"ProcessMessage")]
    private static class ReceivedChat
    {
        private static bool Prefix(GameByteStreamReader __0,uint __1,NetworkID __2,bool __3)
        {
            if(instance==null||__3||!HasSession||Game.GetIsSingleplayer()||__0==null||__1<2)return true;
            var stream=__0.m_MemoryStream;long position=stream.Position;
            try
            {
                if(__0.ReadByte()!=(byte)ENetworkPacketType.ChatMessage||__1>65536)return true;
                if(__2.SteamID.m_SteamID!=NetworkGameClient.GetServerID().SteamID.m_SteamID)return true;
                __0.ReadNetworkID();__0.ReadByte();string message=__0.ReadString();
                bool teamOnly=__0.ReadBool(),serverMessage=__0.ReadBool();
                if(stream.Position>__1||!serverMessage||teamOnly)return true;
                instance.ObserveFeedback(message);
                return !instance.notificationsEnabled||!instance.drawingEnabled;
            }
            catch(Exception error){instance.Error(error,"received chat");return true;}
            finally{stream.Position=position;}
        }
    }
    [HarmonyPatch(typeof(Player),nameof(Player.SendChatMessage))]
    private static class NativeCommandInput
    {
        private static void Prefix(Player __instance,string __0)
        {
            if(instance==null||instance.controllerSending||__instance!=Player.CurrentPlayer)return;
            instance.InvalidateMenuChoices(__0);
        }
    }
    [HarmonyPatch(typeof(NetworkLayer),nameof(NetworkLayer.SendChatMessage))]
    private static class NativeNetworkInput
    {
        private static void Prefix(string __2)
        {if(instance!=null&&!instance.controllerSending)instance.InvalidateMenuChoices(__2);}
    }
    public override void OnUpdate()
    {
        debugUpdates++;
        try{EnsureSessionContext();}catch(Exception e){Error(e);return;}
        try{TickCompanion();}catch(Exception e){Error(e,"companion");}
        if(!PresentationAllowed)
        {
            if(panelOpen||statusPickerOpen)ClosePanel();
            releaseUntil=0;
            if(guardedChatActions.Count>0)RestoreChatInput();
            return;
        }
        MaintainChatInputGuard();
        bool snapshot=false;
        try
        {
            if(keyboardEnabled&&Application.isFocused)
            {
                var k=Keyboard.current;
                if(k!=null)
                {
                    if(k.f6Key.wasPressedThisFrame&&!NativeTextInput())TogglePanel();
                    if(panelOpen&&k.escapeKey.wasPressedThisFrame)ClosePanel();
                    if(panelOpen&&!ProviderPanelActive&&(k.enterKey.wasPressedThisFrame||k.numpadEnterKey.wasPressedThisFrame))SubmitDraft();
                    snapshot=!panelOpen&&k.f7Key.wasPressedThisFrame&&!NativeTextInput()&&!NativeMenu();
                }
            }
        }
        catch(Exception error){keyboardEnabled=false;Error(error);}
        double now=clock.Elapsed.TotalSeconds;
        if(!snapshot&&now<nextPoll)return;
        nextPoll=now+.25;
        debugPolls++;
        try
        {
            EnsureSessionContext();
            if(!HasSession)return;
            EnsureWindowProfile();
            if(now>=nextLocalDiscovery)RefreshLocalMods();
            DrainLocalNotices();
            var p=Player.CurrentPlayer;
            if(panelOpen&&p.IsCommander!=panelCommander)ClosePanel();
            panel=$"Role: {CurrentRole}; admin: {CurrentAdmin}; loaded client mods: {localMods.Count}; known commands: {AllCommands().Count()}";
            if(snapshot)Write("SNAPSHOT "+panel);
        }
        catch(Exception error){ClosePanel();Error(error);}
    }
    private void EnsureSessionContext()
    {
        string context=CurrentContext();
        if(context!=sessionContext){ClosePanel();ResetSession();sessionContext=context;}
    }
    private static string CurrentContext()
    {
        if(!HasSession)return "";
        var p=Player.CurrentPlayer;var mode=GameMode.CurrentGameMode;
        string peer=Game.GetIsSingleplayer()?"SP":NetworkGameClient.GetServerID().SteamID.m_SteamID.ToString();
        return peer+":"+p.GetInstanceID()+":"+(p.Team?.Index??-1)+":"+p.IsCommander+":"+(mode?.GetInstanceID()??0)+":"+(mode?.GameBegun??false)+":"+(mode?.GameOver??false);
    }
    private void ResetSession()
    {
        serverChoices.Clear();serverRequest=null;
        feedback.Clear();dynamicCommands.Clear();activeExchange=null;notices.Clear();noticeRevision++;
        availableAt=0;selectedCommand=null;commandDraft="";lastSend=-100;addEntryOpen=false;
        roleFilter=CurrentRole;controllerMessage="";
    }
    public override void OnGUI()
    {
        if(!visible||!PresentationAllowed||Screen.width<200||Screen.height<100)return;
        debugDraws++;
        if(!drawingEnabled)
        {if(clock.Elapsed.TotalSeconds<nextDrawRetry)return;ResetAppearance();drawingEnabled=true;}
        var matrix=GUI.matrix;
        try{EnsureWindowProfile();GUI.matrix=Matrix4x4.Scale(new Vector3(UiScale,UiScale,1));RenderInterface();}
        catch(Exception error){ClosePanel();drawingEnabled=false;nextDrawRetry=clock.Elapsed.TotalSeconds+2;Error(error,"OnGUI; retry in 2 seconds");}
        finally{GUI.matrix=matrix;}
    }
    private void Error(Exception error,string phase="runtime")
    {debugErrors++;if(clock.Elapsed.TotalSeconds<nextError){AddDebugEvent("ERROR ["+phase+"] "+error.Message);return;}nextError=clock.Elapsed.TotalSeconds+10;Write("ERROR ["+phase+"] "+error);}
    private void Write(string text)
    {
        AddDebugEvent(text);
        string line=DateTime.UtcNow.ToString("o")+" "+Clean(text);LoggerInstance.Msg(line);
        try{log?.WriteLine(line);}catch{log?.Dispose();log=null;}
    }
    private static string Clean(string value)=>(value??"").Replace('\r',' ').Replace('\n',' ').Replace('\t',' ');
    public override void OnDeinitializeMelon(){ClosePanel();RestoreChatInput();ResetAppearance();DisposeSymbols();log?.Dispose();log=null;instance=null;}
}

