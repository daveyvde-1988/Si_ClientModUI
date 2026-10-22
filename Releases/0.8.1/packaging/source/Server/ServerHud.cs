using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;

[assembly: MelonInfo(typeof(SiSquadHud.ServerHud), "Silica Squad HUD Server", "0.8.1", "Davey")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.MONO)]

namespace SiSquadHud
{
    public sealed class ServerHud : MelonMod
    {
        private sealed class Subscription
        {
            public NetworkID Peer; public int Channel, Team; public string Nonce, Signature = "";
            public float Hello, LastRequest, LastSent = -100; public long Sequence;
            public int Version=1;
            public int OverlayFlags;
            public float OverlayAt=-100,StatsAt=-100,NotificationAt;
            public int NotificationCount;
            public long StatsSequence;
            public string StatsSignature="";
            public float StatsFullAt=-100;
            public readonly Dictionary<string,string> StatValues=new Dictionary<string,string>();
            public float LastControl=-100;
            public PanelRequest Pending;
            public BalanceCommand PendingBalance;
            public readonly BalanceDraft BalanceDraft=new BalanceDraft();
            public readonly Dictionary<string,string> BalanceResults=new Dictionary<string,string>();
            public readonly Queue<string> BalanceResultOrder=new Queue<string>();
            public Queue<string> BalanceOutbox=new Queue<string>();
            public string BalanceStamp="",MenuSignature="";
            public long MenuSequence;
            public bool LastAdmin;
            public bool SnapshotAdmin;
            public readonly AdminMenuSession AdminMenu=new AdminMenuSession();
            public readonly Dictionary<string,string[]> MenuResults=new Dictionary<string,string[]>();
            public readonly Queue<string> MenuResultOrder=new Queue<string>();
            public readonly Dictionary<string,string[]> Results=new Dictionary<string,string[]>();
            public readonly Queue<string> ResultOrder=new Queue<string>();
        }
        private static readonly Dictionary<string, Subscription> subscribers = new Dictionary<string, Subscription>();
        private static readonly AccessTools.FieldRef<GameByteStreamReader, MemoryStream> Stream = AccessTools.FieldRefAccess<GameByteStreamReader, MemoryStream>("m_MemoryStream");
        private static bool enabled;
        private static float nextTick, nextError,nextBalanceSend;
        private static ServerHud instance;
        private static long sentBodyBytes,sentPackets,lastLoggedBytes,lastLoggedPackets;
        private static float nextTrafficLog;

        public override void OnInitializeMelon()
        {
            instance = this;
            enabled = !Enum.IsDefined(typeof(ENetworkPacketType), HudProtocol.PacketType);
            if (!enabled) { LoggerInstance.Error("HUD transport disabled: packet value 126 is now used by the game."); return; }
            LoggerInstance.Msg("READY - opt-in status on the authenticated game connection; no chat transport.");
        }
        public override void OnLateInitializeMelon()
        {
            GameEvents.OnGameStarted+=Boundary;GameEvents.OnGameEnded+=Boundary;
            try{UnitBalanceBackend.Initialize();NativeAdminMenus.Initialize();}catch(Exception error){Report(error);}
            try{StarterUnitsMenus.Initialize();}catch(Exception error){Report(error);}
            try{ModNotifications.Initialize();StatusProviders.Initialize();}catch(Exception error){Report(error);}
        }
        private static void Boundary(GameMode mode){subscribers.Clear();MenuCatalog.Boundary();UnitBalanceBackend.Boundary();StarterUnitsMenus.Boundary();}
        private static void Boundary(GameMode mode,Team team){subscribers.Clear();MenuCatalog.Boundary();UnitBalanceBackend.Boundary();StarterUnitsMenus.Boundary();}
        public override void OnSceneWasLoaded(int index,string name){subscribers.Clear();MenuCatalog.Boundary();UnitBalanceBackend.Boundary();StarterUnitsMenus.Boundary();}

        [HarmonyPatch(typeof(NetworkLayer), "ProcessMessage")]
        private static class Receive
        {
            private static bool Prefix(GameByteStreamReader __0, uint __1, NetworkID __2, bool __3)
            {
                if (!enabled || !__3 || !Game.GetIsServer() || Game.GetIsClient() || __0 == null || __1 < 2) return true;
                var stream = Stream(__0);
                long position = stream.Position;
                bool ours = false;
                try
                {
                    if (__0.ReadByte() != HudProtocol.PacketType) return true;
                    if (__1 > HudProtocol.MaxPacket) return true;
                    if (__1 - stream.Position < HudProtocol.Marker.Length + 2 || !HudProtocol.ReadMarker(stream.ReadByte, (byte)EGameByteStreamType.String)) return true;
                    ours = true;
                    string nonce; int channel, team,version;
                    string body = __0.ReadString();
                    if (stream.Position > __1) return false;
                    if(HudProtocol.TryOverlayOptions(body,out var overlayNonce,out var overlayChannel,out var overlayTeam,out var overlayFlags))
                    {
                        var actor=Player.FindPlayer(__2,overlayChannel);
                        string actorKey=__2.SteamID.m_SteamID+":"+overlayChannel;
                        if(actor&&actor.Team&&actor.Team.Index==overlayTeam&&actor.PlayerChannel==overlayChannel&&subscribers.TryGetValue(actorKey,out var target)&&
                            target.Version>=4&&target.Nonce==overlayNonce&&Time.realtimeSinceStartup-target.Hello<=20)
                        {target.OverlayFlags=overlayFlags;target.OverlayAt=Time.realtimeSinceStartup;}
                        return false;
                    }
                    if(HudProtocol.TryBalanceControl(body,out var balance))
                    {
                        var actor=Player.FindPlayer(__2,balance.Channel);
                        string actorKey=__2.SteamID.m_SteamID+":"+balance.Channel;float at=Time.realtimeSinceStartup;
                        if(!actor||!actor.Team||actor.PlayerChannel!=balance.Channel||actor.Team.Index!=balance.Team||actor.IsCommander!=balance.Commander||
                            !subscribers.TryGetValue(actorKey,out var sub)||sub.Version<3||sub.Nonce!=balance.Nonce||at-sub.Hello>20||
                            at-sub.LastControl<0.1f||sub.PendingBalance!=null||sub.Pending!=null)return false;
                        sub.LastControl=sub.Hello=at;sub.PendingBalance=balance;return false;
                    }
                    if(!HudProtocol.TryRequest(body,out nonce,out channel,out team,out version))
                    {
                        if(!HudProtocol.TryControl(body,out var control))return false;
                        var actor=Player.FindPlayer(__2,control.Channel);
                        if(!actor||actor.PlayerChannel!=control.Channel||!actor.Team||actor.Team.Index!=control.Team||actor.IsCommander!=control.Commander)return false;
                        string actorKey=__2.SteamID.m_SteamID.ToString()+":"+control.Channel;
                        float at=Time.realtimeSinceStartup;
                        if(!subscribers.TryGetValue(actorKey,out var subscription)||subscription.Version<2||subscription.Nonce!=control.Nonce||
                            at-subscription.Hello>20||at-subscription.LastControl<0.3f||subscription.Pending!=null||subscription.PendingBalance!=null)return false;
                        subscription.LastControl=subscription.Hello=at;subscription.Pending=control;
                        return false;
                    }
                    // remoteID is validated by native RetrieveNetworkData, not supplied by the request.
                    var player = Player.FindPlayer(__2, channel);
                    if (!player || player.PlayerChannel != channel || !player.Team || player.Team.Index != team) return false;
                    var key = __2.SteamID.m_SteamID.ToString() + ":" + channel;
                    float now = Time.realtimeSinceStartup;
                    if (subscribers.TryGetValue(key, out var prior) && now - prior.LastRequest < 1) return false;
                    if (prior == null || prior.Nonce != nonce || prior.Team != team)
                    {
                        subscribers[key] = prior = new Subscription { Peer = __2, Channel = channel, Team = team, Nonce = nonce, Version=version };
                        instance?.LoggerInstance.Msg("HUD client subscribed: channel " + channel + ", team " + team + ".");
                    }
                    prior.Hello = prior.LastRequest = now;
                    prior.Version=version;
                    return false;
                }
                catch (Exception error) { if (ours) Report(error); return !ours; }
                finally { stream.Position = position; }
            }
        }

        public override void OnUpdate()
        {
            if (!enabled || !Game.GetIsServer() || Game.GetIsClient()) return;
            float now = Time.realtimeSinceStartup;
            foreach(var sub in subscribers.Values.ToArray())
            {
                if(sub.PendingBalance==null)continue;
                var request=sub.PendingBalance;sub.PendingBalance=null;
                var actor=Player.FindPlayer(sub.Peer,sub.Channel);
                if(now-sub.Hello>20||!actor||!actor.Team||actor.PlayerChannel!=sub.Channel||actor.Team.Index!=request.Team||actor.IsCommander!=request.Commander||request.Nonce!=sub.Nonce){sub.BalanceDraft.Clear();continue;}
                try
                {
                    bool admin=MenuCatalog.IsAdmin(actor);
                    if(!admin&&request.Operation!="refresh"){sub.BalanceDraft.Clear();Send(actor,HudProtocol.BalanceReply(sub.Nonce,request.Id,false,UnitBalanceBackend.Revision,"Administrator Generic power required."));continue;}
                    if(request.Operation=="menu")
                    {
                        if(sub.Version<4)continue;
                        if(!sub.MenuResults.TryGetValue(request.Id,out var menuBodies))
                        {
                            AdminMenuPage menu;
                            try{menu=NativeAdminMenus.Handle(actor,request,sub.AdminMenu);}
                            catch(Exception error)
                            {
                                menu=sub.AdminMenu.Page??new AdminMenuPage{Service=sub.AdminMenu.Service,Revision=new string('0',64)};
                                menu.Message=HudProtocol.ClipUtf8(error.GetBaseException().Message,600);
                            }
                            menu.Request=request.Id;
                            menuBodies=HudProtocol.BalanceChunks(sub.Nonce,JsonConvert.SerializeObject(menu));
                            sub.MenuResults[request.Id]=menuBodies;sub.MenuResultOrder.Enqueue(request.Id);
                            while(sub.MenuResultOrder.Count>32)sub.MenuResults.Remove(sub.MenuResultOrder.Dequeue());
                            Traffic("admin-menu "+menu.Service,menuBodies);
                        }
                        foreach(string body in menuBodies)Send(actor,body);continue;
                    }
                    if(!sub.BalanceResults.TryGetValue(request.Id,out string reply))
                    {
                        string message=UnitBalanceBackend.Handle(actor,request,sub.BalanceDraft,out bool ok);
                        reply=HudProtocol.BalanceReply(sub.Nonce,request.Id,ok,UnitBalanceBackend.Revision,message);
                        sub.BalanceResults[request.Id]=reply;sub.BalanceResultOrder.Enqueue(request.Id);
                        while(sub.BalanceResultOrder.Count>32)sub.BalanceResults.Remove(sub.BalanceResultOrder.Dequeue());
                    }
                    Send(actor,reply);
                    if(request.Operation!="stage")sub.BalanceStamp="";
                }
                catch(Exception error){Report(error);}
            }
            // Execute authenticated actions after packet dispatch has unwound, on the game thread.
            foreach(var sub in subscribers.Values.ToArray())
            {
                if(sub.Pending==null)continue;
                var request=sub.Pending;sub.Pending=null;
                var actor=Player.FindPlayer(sub.Peer,sub.Channel);
                if(now-sub.Hello>20||!actor||actor.PlayerChannel!=sub.Channel||!actor.Team||actor.Team.Index!=request.Team||actor.IsCommander!=request.Commander||request.Nonce!=sub.Nonce)continue;
                try
                {
                    string[] bodies;
                    if(request.IsAction&&sub.Results.TryGetValue(request.Id,out bodies)){foreach(var cached in bodies)Send(actor,cached);continue;}
                    var result=PanelBackend.Handle(actor,request);
                    bodies=!request.IsAction&&request.Known.Length>0&&request.Known==HudProtocol.CatalogFingerprint(result)?
                        new[]{HudProtocol.CatalogUnchanged(request,request.Known)}:request.Page==HudProtocol.FullCatalog?HudProtocol.CatalogChunks(result):new[]{HudProtocol.Page(result)};
                    if(request.IsAction)
                    {
                        sub.Results[request.Id]=bodies;sub.ResultOrder.Enqueue(request.Id);
                        while(sub.ResultOrder.Count>32)sub.Results.Remove(sub.ResultOrder.Dequeue());
                    }
                    foreach(var body in bodies)Send(actor,body);
                    sub.Signature="";
                }
                catch(Exception error){Report(error);}
            }
            if(now>=nextBalanceSend)
            {
                nextBalanceSend=now+0.05f;int budget=32;
                foreach(var sub in subscribers.Values.ToArray())
                {
                    if(budget<=0)break;if(sub.BalanceOutbox.Count==0)continue;
                    var actor=Player.FindPlayer(sub.Peer,sub.Channel);
                    if(!actor||!actor.Team||actor.Team.Index!=sub.Team||now-sub.Hello>20){sub.BalanceOutbox.Clear();continue;}
                    if(sub.BalanceStamp!=UnitBalanceBackend.Stamp(sub.SnapshotAdmin)){sub.BalanceOutbox.Clear();continue;}
                    // Never continue sending an admin snapshot after authority has been revoked.
                    if(sub.SnapshotAdmin&&!MenuCatalog.IsAdmin(actor)){sub.BalanceOutbox.Clear();sub.BalanceStamp="";continue;}
                    for(int i=0;i<4&&budget>0&&sub.BalanceOutbox.Count>0;i++,budget--)
                    {if(!Send(actor,sub.BalanceOutbox.Peek()))break;sub.BalanceOutbox.Dequeue();}
                }
            }
            if (now < nextTick) return;
            nextTick = now + 1;
            if(now>=nextTrafficLog)
            {
                nextTrafficLog=now+60;
                if(sentPackets>lastLoggedPackets)instance?.LoggerInstance.Msg("HUD upload interval: packets="+(sentPackets-lastLoggedPackets)+", acceptedBodyBytes="+(sentBodyBytes-lastLoggedBytes)+"; totals="+sentBodyBytes+" bytes. Native/Steam framing and retransmissions excluded.");
                lastLoggedBytes=sentBodyBytes;lastLoggedPackets=sentPackets;
            }
            try{UnitBalanceBackend.RefreshDisk();}catch(Exception error){Report(error);}
            if(subscribers.Count==0)return;
            var modStates=AdminControls.List();
            int currentMask=MenuMask(modStates);
            foreach (var pair in subscribers.ToArray())
            {
                var sub = pair.Value;
                var player = Player.FindPlayer(sub.Peer, sub.Channel);
                if (now - sub.Hello > 20 || !player || !player.Team || player.Team.Index != sub.Team)
                { subscribers.Remove(pair.Key); continue; }
                try
                {
                    if(sub.Version>=3)
                    {
                        bool admin=MenuCatalog.IsAdmin(player);int mask=currentMask;string menuSignature=mask+":"+admin;
                        if(menuSignature!=sub.MenuSignature)
                        {if(Send(player,HudProtocol.MenuState(sub.Nonce,++sub.MenuSequence,mask,admin)))sub.MenuSignature=menuSignature;}
                        bool privateSnapshot=admin&&sub.Version<4;
                        string balanceStamp=UnitBalanceBackend.Stamp(privateSnapshot);
                        if(balanceStamp!=sub.BalanceStamp||privateSnapshot!=sub.SnapshotAdmin)
                        {
                            var bodies=HudProtocol.BalanceChunks(sub.Nonce,UnitBalanceBackend.Snapshot(privateSnapshot));
                            sub.BalanceOutbox=new Queue<string>(bodies);Traffic("public-balance",bodies);
                            sub.BalanceStamp=balanceStamp;sub.SnapshotAdmin=privateSnapshot;
                            if(!admin)sub.BalanceDraft.Clear();
                        }
                        sub.LastAdmin=admin;
                    }
                    var status = HudProviders.Read(player);
                    status.Nonce = sub.Nonce;
                    if((sub.OverlayFlags&2)!=0&&now-sub.OverlayAt<=12)
                    {
                        var rows=StatusProviders.Read(player,status,modStates).Take(128).ToList();
                        string stamp=string.Join("\n",rows.Select(r=>r.Id+"|"+r.Value));
                        if(stamp!=sub.StatsSignature||now-sub.StatsAt>=10)
                        {
                            bool replace=now-sub.StatsFullAt>=30||rows.Count!=sub.StatValues.Count||rows.Any(r=>!sub.StatValues.ContainsKey(r.Id));
                            var changed=replace?rows:rows.Where(r=>!sub.StatValues.TryGetValue(r.Id,out var value)||value!=r.Mod+"|"+r.Label+"|"+r.Value).ToList();
                            bool sent=true;
                            foreach(var b in HudProtocol.StatusParameters(sub.Nonce,++sub.StatsSequence,changed,replace))sent=Send(player,b)&&sent;
                            if(sent)
                            {
                                sub.StatValues.Clear();foreach(var row in rows)sub.StatValues[row.Id]=row.Mod+"|"+row.Label+"|"+row.Value;
                                sub.StatsSignature=stamp;sub.StatsAt=now;if(replace)sub.StatsFullAt=now;
                            }
                        }
                    }
                    var signature = HudProtocol.Status(status,sub.Version>=2);
                    if (signature == sub.Signature && now - sub.LastSent < 5) continue;
                    status.Sequence = sub.Sequence + 1;
                    var body = HudProtocol.Status(status,sub.Version>=2);
                    if (Send(player,body))
                    { sub.Sequence = status.Sequence; sub.Signature = signature; sub.LastSent = now; }
                }
                catch (Exception error) { Report(error); }
            }
        }
        private static int MenuMask(List<AdminControls.Item> mods)
        {
            int mask=0;
            if(mods.Any(i=>i.Key=="Si_Formation.Formations"&&i.State!="OFF"))mask|=1;
            if(mods.Any(i=>i.Key=="Si_KingOfTheHill.KingOfTheHill"&&i.State!="OFF"))mask|=2;
            if(mods.Any(i=>i.Key=="SiPlayerRanks.RankMod"&&i.State!="OFF"))mask|=4;
            if(UnitBalanceBackend.Enabled)mask|=8;return mask;
        }
        private static void Traffic(string kind,string[] bodies)
        {
            long bytes=bodies.Sum(b=>Encoding.UTF8.GetByteCount(b));
            instance?.LoggerInstance.Msg("HUD transfer scheduled "+kind+": packets="+bodies.Length+", bodyBytes="+bytes+" (excludes native/Steam framing and retransmission).");
        }
        private static bool Send(Player player,string body)
        {
            var writer=GameByteStreamWriter.GetGameByteStreamWriter(0,"Si_SquadHUD::Panel",true);
            writer.WriteByte(HudProtocol.PacketType);writer.WriteString(HudProtocol.Marker);writer.WriteString(body);
            bool sent=NetworkLayer.SendPacketToPlayer(writer,ENetworkPacketSend.Reliable,player);
            if(sent){sentPackets++;sentBodyBytes+=Encoding.UTF8.GetByteCount(body);}return sent;
        }
        internal static bool WantsNotifications(Player player)=>TryNotificationSubscription(player,out _);
        private static bool TryNotificationSubscription(Player player,out Subscription sub)
        {
            sub=null;
            return enabled&&Game.GetIsServer()&&!Game.GetIsClient()&&player&&player.Team&&
                subscribers.TryGetValue(player.PlayerID.SteamID.m_SteamID+":"+player.PlayerChannel,out sub)&&
                sub.Team==player.Team.Index&&(sub.OverlayFlags&1)!=0&&Time.realtimeSinceStartup-sub.Hello<=20&&Time.realtimeSinceStartup-sub.OverlayAt<=12;
        }
        internal static bool RedirectNotification(Player player,string mod,string text)
        {
            if(!TryNotificationSubscription(player,out var sub)||string.IsNullOrEmpty(text))return false;
            float now=Time.realtimeSinceStartup;
            if(now-sub.NotificationAt>=1){sub.NotificationAt=now;sub.NotificationCount=0;}
            // A flood falls back to native chat rather than silently dropping a notice.
            if(sub.NotificationCount>=60||Encoding.UTF8.GetByteCount(text)>1100)return false;
            if(!Send(player,HudProtocol.Notification(sub.Nonce,mod,text)))return false;
            sub.NotificationCount++;return true;
        }
        private static void Report(Exception error)
        {
            float now = Time.realtimeSinceStartup;
            if (now < nextError) return;
            nextError = now + 10;
            instance?.LoggerInstance.Warning("HUD status unavailable: " + error.GetBaseException().Message);
        }
        public override void OnDeinitializeMelon() { enabled = false; subscribers.Clear();ModNotifications.Shutdown();NativeAdminMenus.Shutdown();UnitBalanceBackend.Shutdown();GameEvents.OnGameStarted-=Boundary;GameEvents.OnGameEnded-=Boundary; }
    }
}
