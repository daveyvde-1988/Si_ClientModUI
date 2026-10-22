using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SiSquadHud
{
    public sealed class BalanceCommand
    {
        public string Nonce="",Id="",Operation="",Revision="",Draft="",Payload="";
        public int Channel,Team;
        public bool Commander;
    }
    public sealed class BalanceChunk
    {
        public string Nonce,Id,Hash;
        public int Index,Count;
        public byte[] Bytes;
    }
    public static partial class HudProtocol
    {
        public const int MaxBalanceBytes=2*1024*1024,MaxBalanceChunks=1024;
        private static string packedBalanceJson;
        private static byte[] packedBalanceBytes;
        public static string Digest(byte[] bytes)
        {using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
        public static string BalanceControl(BalanceCommand c)
        {
            if(c==null||!ValidNonce(c.Nonce)||!ValidNonce(c.Id)||!ValidNonce(c.Draft)||!Revision(c.Revision)||
                c.Channel<0||c.Channel>255||c.Team<0||c.Team>255||!new[]{"stage","save","reload","refresh","menu"}.Contains(c.Operation)||
                c.Payload==null||Utf8.GetByteCount(c.Payload)>900)throw new ArgumentException("Invalid balance request.");
            string body=string.Join("|","UC",c.Nonce,c.Id,c.Channel.ToString(Inv),c.Team.ToString(Inv),c.Commander?"1":"0",c.Operation,c.Revision,c.Draft,Pack(c.Payload));
            if(body.Length>MaxBody)throw new ArgumentException("Balance request too large.");return body;
        }
        public static bool TryBalanceControl(string body,out BalanceCommand command)
        {
            command=null;if(body==null||body.Length>MaxBody)return false;
            var f=body.Split('|');int channel,team;
            if(f.Length!=10||f[0]!="UC"||!Number(f[3],0,255,out channel)||!Number(f[4],0,255,out team)||(f[5]!="0"&&f[5]!="1"))return false;
            try
            {
                var c=new BalanceCommand{Nonce=f[1],Id=f[2],Channel=channel,Team=team,Commander=f[5]=="1",Operation=f[6],Revision=f[7],Draft=f[8],Payload=Unpack(f[9])};
                BalanceControl(c);command=c;return true;
            }
            catch(Exception e)when(e is ArgumentException||e is FormatException){return false;}
        }
        public static string BalanceReply(string nonce,string id,bool success,string revision,string message)
        {
            if(!ValidNonce(nonce)||!ValidNonce(id)||!Revision(revision))throw new ArgumentException("Invalid balance reply.");
            return string.Join("|","UR",nonce,id,success?"1":"0",revision,Pack(ClipUtf8(message,350)));
        }
        public static bool TryBalanceReply(string body,out string nonce,out string id,out bool success,out string revision,out string message)
        {
            nonce=id=revision=message="";success=false;if(body==null||body.Length>MaxBody)return false;var f=body.Split('|');
            if(f.Length!=6||f[0]!="UR"||!ValidNonce(f[1])||!ValidNonce(f[2])||!Revision(f[4])||(f[3]!="0"&&f[3]!="1"))return false;
            try{nonce=f[1];id=f[2];success=f[3]=="1";revision=f[4];message=Unpack(f[5]);return Safe(message,350);}catch(FormatException){return false;}
        }
        public static string[] BalanceChunks(string nonce,string json)
        {
            byte[] raw=Utf8.GetBytes(json);if(raw.Length>MaxBalanceBytes)throw new ArgumentException("Balance snapshot too large.");
            byte[] bytes;
            if(ReferenceEquals(json,packedBalanceJson)&&packedBalanceBytes!=null)bytes=packedBalanceBytes;
            else
            {
                using(var output=new MemoryStream())
                {using(var zip=new GZipStream(output,CompressionLevel.Optimal,true))zip.Write(raw,0,raw.Length);bytes=output.ToArray();}
                if(raw.Length>32768){packedBalanceJson=json;packedBalanceBytes=bytes;}
            }
            int count=Math.Max(1,(bytes.Length+1049)/1050);if(count>MaxBalanceChunks)throw new ArgumentException("Balance transfer too large.");
            string id=Guid.NewGuid().ToString("N"),hash=Digest(bytes);
            return Enumerable.Range(0,count).Select(i=>string.Join("|","UB",nonce,id,hash,i.ToString(Inv),count.ToString(Inv),Convert.ToBase64String(bytes.Skip(i*1050).Take(1050).ToArray()))).ToArray();
        }
        public static bool TryBalanceChunk(string body,out BalanceChunk chunk)
        {
            chunk=null;if(body==null||body.Length>MaxBody)return false;var f=body.Split('|');int index,count;
            if(f.Length!=7||f[0]!="UB"||!ValidNonce(f[1])||!ValidNonce(f[2])||!Revision(f[3])||
                !Number(f[4],0,MaxBalanceChunks-1,out index)||!Number(f[5],1,MaxBalanceChunks,out count)||index>=count)return false;
            try{byte[] bytes=Convert.FromBase64String(f[6]);if(bytes.Length<1||bytes.Length>1050)return false;chunk=new BalanceChunk{Nonce=f[1],Id=f[2],Hash=f[3],Index=index,Count=count,Bytes=bytes};return true;}catch(FormatException){return false;}
        }
        public static string DecodeBalance(byte[] bytes,string hash)
        {
            if(bytes.Length>MaxBalanceChunks*1050||Digest(bytes)!=hash)throw new ArgumentException("Balance snapshot hash mismatch.");
            using(var input=new MemoryStream(bytes))using(var zip=new GZipStream(input,CompressionMode.Decompress))using(var output=new MemoryStream())
            {
                byte[] buffer=new byte[8192];int n;
                while((n=zip.Read(buffer,0,buffer.Length))>0){if(output.Length+n>MaxBalanceBytes)throw new ArgumentException("Balance snapshot exceeds limit.");output.Write(buffer,0,n);}
                return Utf8.GetString(output.ToArray());
            }
        }
        public static string MenuState(string nonce,long sequence,int mask,bool admin)
            =>string.Join("|","UM",nonce,sequence.ToString(Inv),mask.ToString(Inv),admin?"1":"0");
        public static bool TryMenuState(string body,out string nonce,out long sequence,out int mask,out bool admin)
        {
            nonce="";sequence=-1;mask=0;admin=false;if(body==null||body.Length>120)return false;var f=body.Split('|');
            if(f.Length!=5||f[0]!="UM"||!ValidNonce(f[1])||!long.TryParse(f[2],out sequence)||sequence<0||!Number(f[3],0,15,out mask)||(f[4]!="0"&&f[4]!="1"))return false;
            nonce=f[1];admin=f[4]=="1";return true;
        }
    }
}
