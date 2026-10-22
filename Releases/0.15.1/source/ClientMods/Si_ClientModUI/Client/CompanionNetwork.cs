using System.Text.Json;
using System.Text.RegularExpressions;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private readonly CompanionCatalog serverCatalog=new();
    private readonly Dictionary<string,IncomingTransfer> serverTransfers=new();
    private sealed class IncomingTransfer
    {internal bool Gzip;internal int Count;internal double At;internal readonly Dictionary<int,byte[]> Parts=new();}
    private sealed class ServerRequest
    {internal long Sequence;internal string Text;internal CommandEntry Entry;internal double At;}
    private ServerRequest serverRequest;
    private bool companionTransport,companionReady;
    private string companionContext="",companionNonce=Protocol.NewNonce(),companionCachePath="";
    private double companionHelloAt=-100;
    private int companionHelloTries;
    private long companionSequence;
    private double voteExpires;
    private string voteQuestion="";
    private readonly List<CommandEntry> serverChoices=new(),serverVotes=new();
    private string CompanionStatus=>Game.GetIsSingleplayer()?"Singleplayer: local mods":!HasSession?"Si_ServerModUI: offline":
        !companionTransport?"Si_ServerModUI: transport unavailable":companionReady?"Si_ServerModUI: detected":
        companionHelloTries>=3&&clock.Elapsed.TotalSeconds-companionHelloAt>5?"Si_ServerModUI: not detected":"Si_ServerModUI: checking...";

    private void InitializeCompanion()=>companionTransport=!Enum.GetValues(typeof(ENetworkPacketType)).Cast<object>().Any(v=>Convert.ToInt32(v)==Protocol.PacketType);
    private void ResetCompanion()
    {
        companionContext="";companionNonce=Protocol.NewNonce();companionReady=false;companionHelloTries=0;companionHelloAt=-100;
        serverCatalog.Clear();serverTransfers.Clear();serverRequest=null;serverChoices.Clear();serverVotes.Clear();voteQuestion="";companionCachePath="";
    }
    private void TickCompanion()
    {
        double now=clock.Elapsed.TotalSeconds;
        bool online=HasSession&&!Game.GetIsSingleplayer()&&Game.GetIsClient()&&!Game.GetIsServer();
        if(!online){if(companionContext.Length>0)ResetCompanion();return;}
        var player=Player.CurrentPlayer;
        // Team/commander/round changes reset the UI separately; only identity and admin-level changes restart discovery.
        string context=NetworkGameClient.GetServerID().SteamID.m_SteamID+":"+player.GetInstanceID()+":"+player.AdminLevel;
        if(context!=companionContext)
        {
            ResetCompanion();companionContext=context;
            string key=Protocol.ShortHash(NetworkGameClient.GetServerID().SteamID.m_SteamID+":"+player.PlayerID.SteamID.m_SteamID+":"+player.PlayerChannel);
            companionCachePath=Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory,"Si_ModMenu","server-frames",key+".json");
            // Cache is a diff hint only. Nothing from disk becomes available until an authenticated frame confirms it.
            try
            {
                if(File.Exists(companionCachePath)&&new FileInfo(companionCachePath).Length<=Protocol.MaxDocumentBytes)
                    serverCatalog.Apply(JsonSerializer.Deserialize<ServerFrame>(File.ReadAllText(companionCachePath)));
            }
            catch(Exception e){serverCatalog.Clear();Error(e,"companion cache");}
        }
        foreach(string id in serverTransfers.Where(p=>now-p.Value.At>20).Select(p=>p.Key).ToArray())serverTransfers.Remove(id);
        if(companionTransport&&!companionReady&&companionHelloTries<3&&now-companionHelloAt>=(companionHelloTries==0?0:4))
        {
            companionHelloAt=now;companionHelloTries++;
            SendCompanionBody(Protocol.Hello(companionNonce,player.PlayerChannel,serverCatalog.Root,serverCatalog.Modules.Select(m=>new KeyValuePair<string,string>(m.Key,m.Value.Hash))));
            Write("COMPANION HELLO "+companionHelloTries);
        }
        if(serverRequest!=null&&now-serverRequest.At>10)
        {
            // A timeout does not prove a command failed. Never resubmit it through native chat.
            controllerMessage="No server reply. The command may have run; it was not resent.";
            serverRequest=null;serverChoices.Clear();
            companionReady=false;companionHelloTries=0;companionHelloAt=now;serverVotes.Clear();voteQuestion="";
        }
        if(serverChoices.Count>0&&(activeExchange==null||now-activeExchange.At>60))serverChoices.Clear();
        if(serverVotes.Count>0&&now>=voteExpires){serverVotes.Clear();voteQuestion="";}
    }
    private void SendCompanionBody(string body)
    {
        var writer=GameByteStreamWriter.GetGameByteStreamWriter(0,"Si_ClientModUI::Send",true);
        writer.WriteByte(Protocol.PacketType);writer.WriteString(Protocol.Marker);writer.WriteString(body);
        NetworkLayer.SendPacket(writer,ENetworkPacketSend.Reliable);
    }
    [HarmonyPatch(typeof(NetworkLayer),"ProcessMessage")]
    private static class CompanionReceive
    {
        private static bool Prefix(GameByteStreamReader __0,uint __1,NetworkID __2,bool __3)
        {
            if(instance==null||!instance.companionTransport||__3||!HasSession||Game.GetIsSingleplayer()||__0==null||__1<2)return true;
            var stream=__0.m_MemoryStream;long position=stream.Position;bool ours=false;
            try
            {
                if(__0.ReadByte()!=Protocol.PacketType||__1>Protocol.MaxPacket)return true;
                if(__1-stream.Position<Protocol.Marker.Length+2||!Protocol.ReadMarker(stream.ReadByte,(byte)EGameByteStreamType.String))return true;
                ours=true;
                if(!NetworkGameClient.GetInServer()||__2.SteamID.m_SteamID!=NetworkGameClient.GetServerID().SteamID.m_SteamID)return false;
                string body=__0.ReadString();
                if(stream.Position>__1)return false;
                if(Protocol.TryTransfer(body,out var chunk))instance.ReceiveCompanionChunk(chunk);
                return false;
            }
            catch(Exception e){if(ours)instance.Error(e,"companion receive");return !ours;}
            finally{stream.Position=position;}
        }
    }
    private void ReceiveCompanionChunk(Chunk chunk)
    {
        if(chunk.Nonce!=companionNonce||companionContext.Length==0)return;
        if(!serverTransfers.TryGetValue(chunk.Id,out var transfer))
        {
            if(serverTransfers.Count>=8)return;
            serverTransfers[chunk.Id]=transfer=new IncomingTransfer{Gzip=chunk.Gzip,Count=chunk.Count,At=clock.Elapsed.TotalSeconds};
        }
        if(transfer.Count!=chunk.Count||transfer.Gzip!=chunk.Gzip){serverTransfers.Remove(chunk.Id);return;}
        transfer.Parts[chunk.Index]=chunk.Bytes;
        if(transfer.Parts.Count!=transfer.Count)return;
        serverTransfers.Remove(chunk.Id);
        string json=Protocol.Assemble(Enumerable.Range(0,transfer.Count).Select(i=>transfer.Parts[i]).ToList(),transfer.Gzip);
        using var document=JsonDocument.Parse(json,new JsonDocumentOptions{MaxDepth=16});var root=document.RootElement;
        switch(root.GetProperty("k").GetString())
        {
            case "frame":
                serverCatalog.Apply(JsonSerializer.Deserialize<ServerFrame>(json));companionReady=true;
                // Permission changes invalidate old menu buttons even if their owner is still present.
                serverChoices.Clear();
                try{Directory.CreateDirectory(Path.GetDirectoryName(companionCachePath));string tmp=companionCachePath+".tmp";File.WriteAllText(tmp,serverCatalog.Snapshot());File.Move(tmp,companionCachePath,true);}
                catch(Exception e){Error(e,"companion cache save");}
                Write("COMPANION FRAME: "+serverCatalog.Modules.Count+" server mods");break;
            case "reply":ReceiveCompanionReply(root);break;
            case "event":ReceiveCompanionEvent(root);break;
            case "rehello":ResetCompanion();break;
        }
    }
    private IEnumerable<CommandEntry> ServerCommands()
    {
        if(!companionReady)yield break;
        foreach(var mod in serverCatalog.Modules.Values.OrderBy(m=>m.Priority))foreach(var cmd in mod.Commands)
        {
            foreach(string text in new[]{cmd.Text}.Concat(cmd.Aliases).Distinct(StringComparer.OrdinalIgnoreCase))
                yield return new CommandEntry{Mod=ModLabel(mod.Name),Command=text,Description=cmd.Label+" "+cmd.Description+(cmd.Argument.Length>0?" Argument: "+cmd.Argument:""),
                    Role=cmd.Admin?"admin":"shared",AdminOnly=cmd.Admin,Scope=ServerScope,Source="server",Group=mod.Id};
            foreach(var choice in cmd.Choices)
                yield return new CommandEntry{Mod=ModLabel(mod.Name),Command=cmd.Text+" "+choice[0],Description=cmd.Label+" / "+choice[1],
                    Role=cmd.Admin?"admin":"shared",AdminOnly=cmd.Admin,Scope=ServerScope,Source="server",Group=mod.Id};
        }
        foreach(var choice in serverChoices.Concat(serverVotes))yield return choice;
    }
    private bool TrySendCompanionCommand(string text,CommandEntry entry)
    {
        if(!companionReady)return false;
        // Prefer the active menu's origin/access metadata over unrelated registry aliases.
        var supplied=serverChoices.FirstOrDefault(c=>CommandAllowed(c)&&RootCommand(c.Command)==RootCommand(text));
        supplied??=serverVotes.FirstOrDefault(c=>CommandAllowed(c)&&c.Command==text);
        supplied??=ServerCommands().FirstOrDefault(c=>CommandAllowed(c)&&c.Source=="server"&&RootCommand(c.Command)==RootCommand(text));
        if(supplied==null)return false;
        if(serverRequest!=null){controllerMessage="Wait for the current server reply.";return true;}
        var request=new ServerRequest{Sequence=++companionSequence,Text=text,Entry=supplied,At=clock.Elapsed.TotalSeconds};
        serverRequest=request;serverChoices.Clear();
        try
        {
            SendCompanionBody(Protocol.Command(companionNonce,request.Sequence,text));
            controllerMessage="Waiting for server reply...";debugChatSends++;AddDebugEvent("Companion command sent; sequence="+request.Sequence);
        }
        catch(Exception e){serverRequest=null;controllerMessage="Could not submit to server; it was not resent.";Error(e,"companion send");}
        return true;
    }
    private void ReceiveCompanionReply(JsonElement root)
    {
        if(serverRequest==null||root.GetProperty("seq").GetInt64()!=serverRequest.Sequence||root.GetProperty("cmd").GetString()!=serverRequest.Text)return;
        var request=serverRequest;serverRequest=null;
        if(!CommandAllowed(request.Entry))return;
        activeExchange=new Exchange{Entry=request.Entry,At=clock.Elapsed.TotalSeconds};
        string error=root.GetProperty("err").GetString()??"";
        var lines=root.GetProperty("lines").EnumerateArray().Take(200).Select(l=>l.GetString()??"").ToArray();
        foreach(string line in lines)ObserveFeedback(line,true);
        if(error.Length>0)ObserveFeedback(error,true);
        controllerMessage=error.Length>0?error:lines.Length==0?"The command completed without captured output.":"Server reply received.";
        if(error.Length==0&&serverCatalog.Modules.TryGetValue(request.Entry.Group,out var mod))
        {
            var registered=new List<string>();
            foreach(var module in serverCatalog.Modules.Values)foreach(var command in module.Commands)
            {registered.Add(command.Text);registered.AddRange(command.Aliases);}
            foreach(var choice in ServerMenuParser.Parse(lines,mod.Navigation,registered))
                AddServerChoice(request.Entry,choice.Command,choice.Label);
        }
    }
    private void AddServerChoice(CommandEntry origin,string command,string label)
    {
        if(serverChoices.Any(c=>c.Command==command))return;
        serverChoices.Add(new CommandEntry{Mod=origin.Mod,Command=command,Label=label,MenuOrder=serverChoices.Count,Description=label,Role=origin.Role,AdminOnly=origin.AdminOnly,
            Scope=ServerScope,Source="server-menu",Group=origin.Group});
    }
    private void ReceiveCompanionEvent(JsonElement root)
    {
        if(!companionReady)return;
        string name=root.GetProperty("e").GetString();
        if(name=="vote-end"){serverVotes.Clear();voteQuestion="";return;}
        if(name!="vote")return;
        var data=root.GetProperty("data");string question=Plain(data.GetProperty("q").GetString()??"Vote");
        var next=new List<CommandEntry>();
        foreach(var option in data.GetProperty("opts").EnumerateArray().Take(64))
        {
            string command=option.GetProperty("c").GetString();if(!Protocol.ValidCommandText(command))continue;
            next.Add(new CommandEntry{Mod="Vote",Command=command,Description=question+" / "+Plain(option.GetProperty("d").GetString()),
                Role="shared",Scope=ServerScope,Source="server-vote"});
        }
        int duration=data.GetProperty("dur").GetInt32();voteExpires=clock.Elapsed.TotalSeconds+Math.Clamp(duration,1,600);
        serverVotes.Clear();serverVotes.AddRange(next);voteQuestion=question;
        feedback.Add(new FeedbackLine{Mod="Vote",Text=question});if(feedback.Count>200)feedback.RemoveAt(0);
        if(notificationsEnabled)AddNotification("Vote",question);
    }
}
