using System;
using System.Collections.Generic;
using System.Linq;

namespace SiSquadHud
{
    // Small optional overlay messages on the existing authenticated transport.
    public sealed class StatusParameter
    {
        public string Id="",Mod="",Label="",Value="";
    }
    public static partial class HudProtocol
    {
        public static string OverlayOptions(string nonce,int channel,int team,int flags)=>"UO|"+nonce+"|"+channel+"|"+team+"|"+flags;
        public static bool TryOverlayOptions(string text,out string nonce,out int channel,out int team,out int flags)
        {
            nonce="";channel=team=flags=0;if(text==null||text.Length>80)return false;var p=text.Split('|');
            if(p.Length!=5||p[0]!="UO"||!ValidNonce(p[1])||!Number(p[2],0,255,out channel)||!Number(p[3],0,255,out team)||!Number(p[4],0,3,out flags))return false;
            nonce=p[1];return true;
        }
        public static string Notification(string nonce,string mod,string text)=>"UN|"+nonce+"|"+Pack(ClipUtf8(mod,64))+"|"+Pack(ClipUtf8(text,1100));
        public static bool TryNotification(string body,out string nonce,out string mod,out string text)
        {
            nonce=mod=text="";if(body==null||body.Length>MaxBody)return false;var p=body.Split('|');
            if(p.Length!=4||p[0]!="UN"||!ValidNonce(p[1]))return false;
            try{nonce=p[1];mod=Unpack(p[2]);text=Unpack(p[3]);return mod.Length<=64&&text.Length<=1100;}catch(ArgumentException){return false;}catch(FormatException){return false;}
        }
        public static string[] StatusParameters(string nonce,long sequence,List<StatusParameter> rows,bool replace=true)
        {
            var batches=new List<string>();string batch="";
            foreach(var r in rows.Take(128))
            {
                string row=Pack(ClipUtf8(r.Id,96))+":"+Pack(ClipUtf8(r.Mod,64))+":"+Pack(ClipUtf8(r.Label,96))+":"+Pack(ClipUtf8(r.Value,320));
                if(batch.Length+row.Length>1550){batches.Add(batch);batch="";}
                batch+=(batch.Length==0?"":",")+row;
            }
            batches.Add(batch);
            return batches.Select((b,i)=>"US|"+nonce+"|"+sequence+"|"+i+"|"+batches.Count+"|"+(replace?"1":"0")+"|"+b).ToArray();
        }
        public static bool TryStatusParameters(string body,out string nonce,out long sequence,out int index,out int count,out bool replace,out StatusParameter[] rows)
        {
            nonce="";sequence=0;index=count=0;replace=false;rows=null;if(body==null||body.Length>MaxBody)return false;var p=body.Split('|');
            if(p.Length!=7||p[0]!="US"||!ValidNonce(p[1])||!long.TryParse(p[2],out sequence)||sequence<1||!Number(p[3],0,127,out index)||!Number(p[4],1,128,out count)||index>=count||(p[5]!="0"&&p[5]!="1"))return false;
            replace=p[5]=="1";
            try
            {
                var result=new List<StatusParameter>();
                foreach(var encoded in p[6].Split(',').Where(x=>x.Length>0))
                {
                    var r=encoded.Split(':');if(r.Length!=4||result.Count>=128)return false;
                    var s=new StatusParameter{Id=Unpack(r[0]),Mod=Unpack(r[1]),Label=Unpack(r[2]),Value=Unpack(r[3])};
                    if(!Safe(s.Id,96)||!Safe(s.Mod,64)||!Safe(s.Label,96)||!Safe(s.Value,320))return false;result.Add(s);
                }
                nonce=p[1];rows=result.ToArray();return true;
            }
            catch(ArgumentException){return false;}catch(FormatException){return false;}
        }
    }
}
