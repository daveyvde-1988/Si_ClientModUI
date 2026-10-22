using System.Diagnostics;
using System.Text;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(SiSquadHud.SquadHud), "Silica Squad HUD", "0.8.0", "Davey")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonPlatform(MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace SiSquadHud;

public sealed partial class SquadHud : MelonMod
{
    private static SquadHud instance;
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly HudCache cache = new();
    private bool networkEnabled, keyboardEnabled = true, drawingEnabled = true, visible = true, wasConnected;
    private double nextPoll, nextRequest, nextError, nextDrawRetry;
    private int lastMode;
    private float lastGameTime;
    private bool roundPlaying;
    private string panel = "Squad HUD\nJoin a server to see your squad.";
    private string lastServerSummary = "";
    private StreamWriter log;

    public override void OnInitializeMelon()
    {
        instance = this;
        networkEnabled = !Enum.IsDefined(typeof(ENetworkPacketType), HudProtocol.PacketType);
        try
        {
            var folder = Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, "Si_SquadHUD");
            Directory.CreateDirectory(folder);
            log = new StreamWriter(new FileStream(Path.Combine(folder, $"session-{DateTime.UtcNow:yyyyMMddTHHmmssfffZ}.log"),
                FileMode.CreateNew, FileAccess.Write, FileShare.Read), new UTF8Encoding(false)) { AutoFlush = true };
        }
        catch (Exception error) { LoggerInstance.Warning("Session log unavailable: " + error.Message); }
        var appearancePreferences=MelonPreferences.CreateCategory("Si_SquadHUD");
        fontPreference=appearancePreferences.CreateEntry("FontSize",18);
        var styleRevision=appearancePreferences.CreateEntry("StyleRevision",0);
        if(styleRevision.Value==0){if(fontPreference.Value==20)fontPreference.Value=18;styleRevision.Value=1;}
        Write("READY - F6 expands status, then opens the mod menu; F7 logs status. Mouse-only actions.");
        if (!networkEnabled) Write("Transport disabled: packet 126 is defined by this game build. Local HUD remains available.");
    }

    public override void OnSceneWasLoaded(int buildIndex, string name)
    { ClosePanel();ResetAppearance();drawingEnabled=true;nextDrawRetry=0;cache.Clear(); nextRequest = nextPoll = 0; Write("Scene: " + Clean(name)); }

    [HarmonyPatch(typeof(NetworkLayer), "ProcessMessage")]
    private static class Receive
    {
        private static bool Prefix(GameByteStreamReader __0, uint __1, NetworkID __2, bool __3)
        {
            if (instance == null || !instance.networkEnabled || __3 || !Game.GetIsClient() || __0 == null || __1 < 2) return true;
            var stream = __0.m_MemoryStream;
            long position = stream.Position;
            bool ours = false;
            try
            {
                if (__0.ReadByte() != HudProtocol.PacketType || __1 > HudProtocol.MaxPacket) return true;
                if (__1 - stream.Position < HudProtocol.Marker.Length + 2 || !HudProtocol.ReadMarker(stream.ReadByte, (byte)EGameByteStreamType.String)) return true;
                ours = true;
                if (!NetworkGameClient.GetInServer() || __2.SteamID.m_SteamID != NetworkGameClient.GetServerID().SteamID.m_SteamID) return false;
                string body = __0.ReadString();
                if (stream.Position > __1) return false;
                if(instance.ReceiveBalanceBody(body))return false;
                if(instance.ReceiveOverlayBody(body))return false;
                if(instance.ReceiveCatalogUnchanged(body))return false;
                if(HudProtocol.TryCatalog(body,out var page)){instance.ReceivePage(page);return false;}
                if(HudProtocol.TryPage(body,out _)){instance.panelMessage="Update the server companion to 0.4.0 for complete lists.";instance.pendingId="";return false;}
                if (HudProtocol.TryStatus(body, out var status) && instance.cache.Accept(status, instance.clock.Elapsed.TotalSeconds))
                {
                    instance.nextPoll = 0;
                    string summary = $"SERVER team={status.Team}; rank={status.Rank}; cap={status.EffectiveCap}; source={status.Source}; commander={status.Commander}; round={status.Round}";
                    if (summary != instance.lastServerSummary) { instance.Write(summary); instance.lastServerSummary = summary; }
                }
                return false;
            }
            catch (Exception error) { if (ours) instance.Error(error); return !ours; }
            finally { stream.Position = position; }
        }
    }

    public override void OnUpdate()
    {
        bool snapshot = false;
        if (keyboardEnabled)
        {
            try
            {
                var keyboard = Keyboard.current;
                if (keyboard != null && Application.isFocused)
                {
                    if(keyboard.f6Key.wasPressedThisFrame&&!NativeTextInput())TogglePanel();
                    snapshot = !NativeTextInput()&&!NativeMenu()&&keyboard.f7Key.wasPressedThisFrame;
                }
            }
            catch (Exception error) { keyboardEnabled = false; Error(error); }
        }
        double now = clock.Elapsed.TotalSeconds;
        if (!snapshot && now < nextPoll) return;
        nextPoll = now + 0.25;
        try
        {
            var player = Player.CurrentPlayer;
            bool connected = Game.GetIsClient() && !Game.GetIsServer() && NetworkGameClient.GetInServer() && player != null;
            if (!connected)
            {
                if (wasConnected) {ClosePanel();cache.Clear();}
                wasConnected = false;
                panel = "Squad HUD\nJoin a dedicated server to see your squad.\nF6: toggle HUD   F7: status snapshot";
            }
            else
            {
                wasConnected = true;
                var mode = GameMode.CurrentGameMode;
                bool playing = mode != null && mode.GameBegun && !mode.GameOver;
                int modeId = mode == null ? 0 : mode.GetInstanceID();
                float gameTime = mode == null ? 0 : mode.GameTime;
                if (modeId != lastMode || playing != roundPlaying || gameTime + 0.5f < lastGameTime)
                { ClosePanel();cache.Clear(); nextRequest = 0; }
                lastMode = modeId; roundPlaying = playing; lastGameTime = gameTime;
                int team = player.Team == null ? -1 : player.Team.Index;
                string peer = NetworkGameClient.GetServerID().SteamID.m_SteamID.ToString();
                if (cache.SetContext(peer, player.GetInstanceID(), team)) { ClosePanel();nextRequest = 0; Write("Connection/player/team changed; cleared server status."); }
                if (networkEnabled && team >= 0 && now >= nextRequest)
                {
                    nextRequest = now + 5;
                    var writer = GameByteStreamWriter.GetGameByteStreamWriter(0, "Si_SquadHUD::Request", true);
                    writer.WriteByte(HudProtocol.PacketType); writer.WriteString(HudProtocol.Marker);
                    writer.WriteString(HudProtocol.Request(cache.Nonce, player.PlayerChannel, team));
                    NetworkLayer.SendPacket(writer, ENetworkPacketSend.Reliable);
                }
                panel = BuildPanel(player, playing ? cache.Fresh(now) : null);
                MaintainBalance(now);
                MaintainPanel(player,now);
                EnsureWindowProfile();MaintainSidebar(player,now);MaintainNotifications(player,now);
            }
            if (snapshot) { Write("SNAPSHOT " + panel.Replace('\n', ';')); nextRequest = Math.Min(nextRequest, now + 1); }
        }
        catch (Exception error)
        {
            ClosePanel();cache.Clear(); panel = "Squad HUD\nGame state changing; waiting for a fresh update."; Error(error);
        }
    }

    private static string BuildPanel(Player player, HudStatus remote)
    {
        var unit = player.ControlledUnit;
        bool commander = player.IsCommander;
        int? count = null, followers = null;
        if (unit != null)
        {
            var group = unit.AIGroup;
            if (group != null)
            {
                count = group.UnitCount;
                if (group.Leader == unit) followers = Math.Max(0, count.Value - 1);
            }
        }
        string heading = "Squad HUD | " + (commander ? "Commander" : unit == null ? "Unspawned / spectating" : "FPS");
        heading+="\nScore: "+(remote?.Score??player.Score).ToString("N0")+"   |   Personal credits: "+(remote?.Credits?.ToString("N0")??"Unavailable");
        string squad = commander ? "FPS squad inactive while commanding" :
            $"Group: {Number(count)} units including you | AI followers: {Number(followers)}";
        if (remote == null)
        {
            var native = FPSCommanding.Instance;
            return heading + "\n" + squad + "\nServer rank/status unavailable or stale\n" +
                "Local native limit: " + (native == null ? "n/a" : native.GroupUnitLimit.ToString()) +
                " (server allowance unknown)\nF6: toggle HUD   F7: status snapshot";
        }
        if (remote.Commander != commander) return heading + "\n" + squad + "\nRole changing; waiting for server status.\nF6: toggle HUD   F7: status snapshot";
        string source = remote.Source switch {
            "rank-policy" => "rank policy", "admin-override" => "admin override", "custom-policy" => "server policy",
            "native" => "native fallback", _ => "policy unavailable"
        };
        string cap = remote.EffectiveCap < 0 ? "Server squad allowance: unavailable" : $"Server squad allowance: {remote.EffectiveCap} including you ({source})";
        string rankCap = remote.RankCap > 0 && remote.RankCap != remote.EffectiveCap ? $" | rank cap {remote.RankCap}" : "";
        return heading + "\n" + squad + "\nRank: " + remote.Rank + "\n" + cap + rankCap +
            "\nRecruits: " + remote.Recruitment + (remote.Exclusions.Length == 0 ? "" : "\nExcluded as AI followers: " + remote.Exclusions) +
            "\nF6: toggle HUD   F7: status snapshot";
    }

    public override void OnGUI()
    {
        if (!visible||Game.InMainMenu||!Game.GetIsClient()||Game.GetIsServer()||!NetworkGameClient.GetInServer()||Player.CurrentPlayer==null||GameMode.CurrentGameMode==null) return;
        if(!drawingEnabled)
        {
            if(clock.Elapsed.TotalSeconds<nextDrawRetry)return;
            ResetAppearance();drawingEnabled=true;
            Write("Renderer recovery: rebuilding UI after a drawing error.");
        }
        if (Screen.width < 200 || Screen.height < 100) return;
        try
        {
            RenderInterface();
        }
        catch (Exception error) { ClosePanel();drawingEnabled = false;nextDrawRetry=clock.Elapsed.TotalSeconds+2;Error(error,"OnGUI; retry in 2 seconds"); }
    }
    private void Error(Exception error,string phase="runtime")
    {
        if (clock.Elapsed.TotalSeconds < nextError) return;
        nextError = clock.Elapsed.TotalSeconds + 10;
        Write("ERROR ["+phase+"] " + error.ToString());
    }
    private void Write(string text)
    {
        var line = DateTime.UtcNow.ToString("o") + " " + Clean(text);
        LoggerInstance.Msg(line);
        if (log == null) return;
        try { log.WriteLine(line); }
        catch { try { log.Dispose(); } catch { } log = null; }
    }
    private static string Clean(string value) => value.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
    private static string Number(int? value) => value?.ToString() ?? "n/a";
    public override void OnDeinitializeMelon() { ClosePanel();ResetAppearance();networkEnabled = false; cache.Clear(); log?.Dispose(); log = null; instance = null; }
}
