// Offline native/loader stand-ins. CompanionNetwork.cs is the production implementation.
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace HarmonyLib
{
    [AttributeUsage(AttributeTargets.Class)] public sealed class HarmonyPatch : Attribute {public HarmonyPatch(Type type,string method){}}
}
namespace MelonLoader.Utils
{
    public static class MelonEnvironment {public static string UserDataDirectory=Path.Combine(Path.GetTempPath(),"ModUI-network-checks-"+Guid.NewGuid().ToString("N"));}
}
namespace UnityEngine {}
namespace Il2Cpp
{
    public enum ENetworkPacketType {Heartbeat=0}
    public enum ENetworkPacketSend {Reliable}
    public enum EGameByteStreamType : byte {Byte=1,String=2}
    public struct SteamID {public ulong m_SteamID;}
    public struct NetworkID {public SteamID SteamID;public static NetworkID Of(ulong id)=>new(){SteamID=new(){m_SteamID=id}};}
    public sealed class Player
    {
        public static Player CurrentPlayer=new();public int PlayerChannel,AdminLevel;
        public NetworkID PlayerID=NetworkID.Of(111);
        public int GetInstanceID()=>7;
    }
    public static class Game {public static bool Singleplayer;public static bool GetIsSingleplayer()=>Singleplayer;public static bool GetIsClient()=>true;public static bool GetIsServer()=>false;}
    public static class NetworkGameClient {public static bool GetInServer()=>true;public static NetworkID GetServerID()=>NetworkID.Of(222);}
    public sealed class GameByteStreamWriter
    {
        internal MemoryStream Stream=new();internal BinaryWriter Writer;
        public GameByteStreamWriter(){Writer=new(Stream,Encoding.UTF8,true);}
        public static GameByteStreamWriter GetGameByteStreamWriter(int channel,string source,bool types)=>new();
        public void WriteByte(byte value){Writer.Write((byte)EGameByteStreamType.Byte);Writer.Write(value);}
        public void WriteString(string value){Writer.Write((byte)EGameByteStreamType.String);Writer.Write(value);}
    }
    public sealed class GameByteStreamReader
    {
        public MemoryStream m_MemoryStream;private BinaryReader reader;
        public GameByteStreamReader(byte[] bytes){m_MemoryStream=new(bytes);reader=new(m_MemoryStream,Encoding.UTF8,true);}
        public byte ReadByte(){if(reader.ReadByte()!=(byte)EGameByteStreamType.Byte)throw new InvalidDataException();return reader.ReadByte();}
        public string ReadString(){if(reader.ReadByte()!=(byte)EGameByteStreamType.String)throw new InvalidDataException();return reader.ReadString();}
    }
    public static class NetworkLayer
    {
        public static readonly List<byte[]> Sent=new();
        public static void SendPacket(GameByteStreamWriter writer,ENetworkPacketSend mode)=>Sent.Add(writer.Stream.ToArray());
    }
}
namespace SiSquadHud
{
    public sealed partial class SquadHud
    {
        private static SquadHud instance;private readonly Stopwatch clock=Stopwatch.StartNew();
        private static bool HasSession=true;private bool notificationsEnabled=false;private int debugChatSends;
        private string controllerMessage="";private readonly List<FeedbackLine> feedback=new();private Exchange activeExchange;
        private static string ServerScope=>"server:222";
        public sealed class CommandEntry {public string Mod="",Command="",Description="",Role="",Scope="",Source="",Group="";public bool AdminOnly;}
        private sealed class FeedbackLine {internal string Mod,Text;}
        private sealed class Exchange {internal CommandEntry Entry;internal double At;}
        private static string Plain(string text)=>Regex.Replace(text??"","<[^>]*>","");
        private static string ModLabel(string value)=>value;
        private static string RootCommand(string value)=>(value??"").Trim().Split(' ')[0].ToLowerInvariant();
        private bool CommandAllowed(CommandEntry c)=>HasSession&&c!=null&&c.Scope==ServerScope&&(!c.AdminOnly||Il2Cpp.Player.CurrentPlayer.AdminLevel>0);
        private void Error(Exception error,string phase=""){Errors.Add(phase+": "+error.Message);}
        private void Write(string text){}private void AddDebugEvent(string text){}private void AddNotification(string mod,string text){}
        private void ObserveFeedback(string text,bool structured=false)=>feedback.Add(new(){Mod=activeExchange?.Entry.Mod??"",Text=text});
        internal readonly List<string> Errors=new();
        internal SquadHud(){instance=this;InitializeCompanion();}
        internal static void RunNetworkChecks(Action<bool,string> check)
        {
            var c=new SquadHud();Il2Cpp.Game.Singleplayer=true;Il2Cpp.NetworkLayer.Sent.Clear();c.TickCompanion();
            check(Il2Cpp.NetworkLayer.Sent.Count==0&&c.CompanionStatus.StartsWith("Singleplayer"),"Singleplayer sends no packets");
            Il2Cpp.Game.Singleplayer=false;c.TickCompanion();check(Il2Cpp.NetworkLayer.Sent.Count==1&&!c.companionReady,"Join sends metadata hello only");
            c.TickCompanion();check(Il2Cpp.NetworkLayer.Sent.Count==1,"No repeated hello on UI/update ticks");
            c.companionHelloTries=3;c.companionHelloAt=-100;c.TickCompanion();
            check(c.CompanionStatus.Contains("not detected")&&!c.TrySendCompanionCommand("/buy",null),"Absent server reports status and leaves native fallback available");
            var m=new ServerModule{Id="Buy",Name="Buy",Hash=Protocol.ShortHash("buy"),Commands=new(){new(){Text="/buy"}}};
            string json=JsonSerializer.Serialize(new{k="frame",root=Protocol.ShortHash(m.Id+":"+m.Hash),full=true,mods=new[]{m},removed=Array.Empty<string>()});
            foreach(string packet in Protocol.Transfer(Protocol.NewNonce(),json)){Protocol.TryTransfer(packet,out var chunk);c.ReceiveCompanionChunk(chunk);}
            check(!c.companionReady,"Stale connection nonce ignored");
            byte[] Packet(string body,string marker=Protocol.Marker){var w=new Il2Cpp.GameByteStreamWriter();w.WriteByte(Protocol.PacketType);w.WriteString(marker);w.WriteString(body);return w.Stream.ToArray();}
            bool Receive(Il2Cpp.GameByteStreamReader reader,Il2Cpp.NetworkID peer)=>(bool)typeof(CompanionReceive).GetMethod("Prefix",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,new object[]{reader,(uint)reader.m_MemoryStream.Length,peer,false});
            var body=Protocol.Transfer(c.companionNonce,json)[0];var fake=new Il2Cpp.GameByteStreamReader(Packet(body));
            check(!Receive(fake,Il2Cpp.NetworkID.Of(999))&&!c.companionReady&&fake.m_MemoryStream.Position==0,"Non-server peer ignored and reader restored");
            fake=new(Packet(body,"OtherMod/2"));
            check(Receive(fake,Il2Cpp.NetworkID.Of(222))&&fake.m_MemoryStream.Position==0,"Other packet owner retains native processing");
            fake=new(Packet(body));Receive(fake,Il2Cpp.NetworkID.Of(222));
            check(c.companionReady&&c.CompanionStatus.Contains("detected")&&c.ServerCommands().Count()==1&&fake.m_MemoryStream.Position==0,"Authenticated frame enables catalog");
            check(!c.TrySendCompanionCommand("ordinary chat",null),"Free text remains native");
            check(c.TrySendCompanionCommand("/buy",null)&&c.serverRequest!=null,"Confirmed command uses proxy");
            int count=Il2Cpp.NetworkLayer.Sent.Count;check(c.TrySendCompanionCommand("/buy",null)&&Il2Cpp.NetworkLayer.Sent.Count==count,"Busy proxy never duplicates submission");
            long seq=c.serverRequest.Sequence;
            void Deliver(string doc){foreach(string p in Protocol.Transfer(c.companionNonce,doc)){Protocol.TryTransfer(p,out var chunk);c.ReceiveCompanionChunk(chunk);}}
            Deliver(JsonSerializer.Serialize(new{k="reply",seq=seq+1,cmd="/buy",lines=new[]{"1. Wrong"},err=""}));
            check(c.serverRequest!=null&&c.serverChoices.Count==0,"Uncorrelated reply ignored");
            Deliver(JsonSerializer.Serialize(new{k="reply",seq,cmd="/buy",lines=new[]{"1. Buy vehicle","2) Buy infantry"},err=""}));
            check(c.serverRequest==null&&c.serverChoices.Any(x=>x.Command=="/1")&&c.serverChoices.Any(x=>x.Command=="/back")&&c.serverChoices.Any(x=>x.Command=="/0"),"Reply produces explicit server navigation");
            Deliver(JsonSerializer.Serialize(new{k="event",e="vote",data=new{q="Next map?",dur=20,opts=new[]{new{c="1",d="Map A"}}}}));
            check(c.serverVotes.Count==1&&c.ServerCommands().Any(x=>x.Source=="server-vote"),"Vote appears in existing command list");
            Deliver("{\"k\":\"event\",\"e\":\"vote-end\",\"data\":null}");check(c.serverVotes.Count==0,"Vote end removes choices");
            c.TrySendCompanionCommand("/1",null);check(c.serverRequest?.Text=="/1","Advertised navigation uses proxy");
            c.serverRequest.At=-100;count=Il2Cpp.NetworkLayer.Sent.Count;c.TickCompanion();
            check(c.serverRequest==null&&Il2Cpp.NetworkLayer.Sent.Count==count&&c.controllerMessage.Contains("not resent"),"Timeout does not replay command or send native fallback");
            c.TickCompanion();check(Il2Cpp.NetworkLayer.Sent.Count==count+1&&!c.companionReady,"Timeout recovery sends discovery only");
            c.ResetCompanion();check(!c.companionReady&&!c.ServerCommands().Any()&&c.serverVotes.Count==0,"Scene/reset clears confirmed data");
            c.TickCompanion();check(!c.companionReady&&c.serverCatalog.Modules.Count==1&&!c.ServerCommands().Any(),"Cached frame remains unavailable before confirmation");
            Deliver(JsonSerializer.Serialize(new{k="frame",root=c.serverCatalog.Root,same=true}));
            check(c.companionReady,"Fresh unchanged confirmation enables cached catalog");
            m.Commands.Add(new(){Text="!map",Admin=true,Argument="map",Choices=new(){new[]{"desert","Desert"}}});m.Hash=Protocol.ShortHash("buy and map");
            Deliver(JsonSerializer.Serialize(new{k="frame",root=Protocol.ShortHash(m.Id+":"+m.Hash),full=true,mods=new[]{m},removed=Array.Empty<string>()}));
            check(c.ServerCommands().Any(x=>x.Command=="!map desert"&&x.AdminOnly),"Argument choices enter current command list with admin metadata");
            check(!c.TrySendCompanionCommand("!map desert",null),"Local admin filter blocks advertised admin actions for ordinary users");
            Il2Cpp.Player.CurrentPlayer.AdminLevel=1;c.TickCompanion();
            check(!c.companionReady&&!c.ServerCommands().Any(),"Admin-level change requires fresh server confirmation");
            Deliver(JsonSerializer.Serialize(new{k="frame",root=c.serverCatalog.Root,same=true}));
            check(c.TrySendCompanionCommand("!map desert",null),"Authorized server choice uses proxy after confirmation");
            HasSession=false;c.TickCompanion();check(c.serverCatalog.Modules.Count==0&&c.CompanionStatus.Contains("offline"),"Disconnect clears server data");
            HasSession=true;
            Il2Cpp.Player.CurrentPlayer.AdminLevel=0;
            string temporary=Path.GetFullPath(MelonLoader.Utils.MelonEnvironment.UserDataDirectory);
            if(!temporary.StartsWith(Path.Combine(Path.GetFullPath(Path.GetTempPath()),"ModUI-network-checks-"),StringComparison.OrdinalIgnoreCase))throw new Exception("Unexpected test cleanup path");
            if(Directory.Exists(temporary))Directory.Delete(temporary,true);
        }
    }
}
