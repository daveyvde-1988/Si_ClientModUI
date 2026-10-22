using System;
using System.Collections.Generic;
using System.Linq;

namespace SiSquadHud
{
    public static partial class HudProtocol
    {
        public const int FullCatalog=2048,MaxCatalogRows=512,MaxCatalogChunks=512;
        public static string CatalogFingerprint(PanelPage p)
        {
            var fields=new List<string>{p.Function,p.Team.ToString(Inv),p.Commander.ToString(),p.Revision,p.Selected.ToString(Inv),p.Locked.ToString(),p.Enabled.ToString(),p.Available.ToString(),p.Leader.ToString(),p.Admin.ToString(),p.Message};
            for(int i=0;i<p.Numbers.Length;i++)fields.AddRange(new[]{p.Numbers[i].ToString(Inv),p.Names[i],i<p.Details.Length?p.Details[i]:"",i<p.Groups.Length?p.Groups[i]:"",i<p.Data.Length?p.Data[i]:""});
            return Digest(Utf8.GetBytes(string.Join("\u001e",fields)));
        }
        public static string CatalogUnchanged(PanelRequest r,string hash)=>string.Join("|","GU",r.Nonce,r.Team.ToString(Inv),r.Commander?"1":"0",r.Id,r.Function,hash);
        public static bool TryCatalogUnchanged(string body,out PanelRequest scope)
        {
            scope=null;if(body==null||body.Length>256)return false;var f=body.Split('|');int team;
            if(f.Length!=7||f[0]!="GU"||!ValidNonce(f[1])||!Number(f[2],0,255,out team)||(f[3]!="0"&&f[3]!="1")||!ValidNonce(f[4])||!Function(f[5])||!Revision(f[6]))return false;
            scope=new PanelRequest{Nonce=f[1],Team=team,Commander=f[3]=="1",Id=f[4],Function=f[5],Known=f[6]};return true;
        }
        // Each complete snapshot is split into bounded native packets; these chunks are never UI pages.
        public static string Catalog(PanelPage p)
        {
            int n=p.Numbers.Length;
            if(!ValidNonce(p.Nonce)||!ValidNonce(p.Id)||!Function(p.Function)||p.Team<0||p.Team>255||p.Page<0||p.Pages<1||p.Pages>MaxCatalogChunks||p.Page>=p.Pages||n>6||
                p.Selected<1||p.Selected>9999||(!Revision(p.Revision)&&p.Revision!="-")||!Safe(p.Message,160)||Utf8.GetByteCount(p.Message)>160||
                p.Names.Length!=n||p.Details.Length!=n||p.Groups.Length!=n||p.Data.Length!=n||p.Numbers.Any(v=>v<1||v>9999)||p.Numbers.Distinct().Count()!=n||
                p.Names.Any(v=>!Safe(v,96)||Utf8.GetByteCount(v)>96)||p.Details.Any(v=>!Safe(v,400)||Utf8.GetByteCount(v)>400)||
                p.Groups.Any(v=>!Safe(v,96)||Utf8.GetByteCount(v)>96)||p.Data.Any(v=>!Safe(v,400)||Utf8.GetByteCount(v)>400))throw new ArgumentException("Invalid catalog chunk.");
            string rows=string.Join(",",p.Numbers.Select((v,i)=>v.ToString(Inv)+":"+Pack(p.Names[i])+":"+Pack(p.Details[i])+":"+Pack(p.Groups[i])+":"+Pack(p.Data[i])));
            string body=string.Join("|","G",p.Nonce,p.Team.ToString(Inv),p.Commander?"1":"0",p.Id,p.Function,p.Page.ToString(Inv),p.Pages.ToString(Inv),p.Selected.ToString(Inv),
                p.Locked?"1":"0",p.Enabled?"1":"0",p.Available?"1":"0",p.Leader?"1":"0",p.Revision,Pack(p.Message),p.Admin?"1":"0",rows);
            if(body.Length>MaxBody)throw new ArgumentException("Catalog chunk exceeds packet budget.");
            return body;
        }
        public static bool TryCatalog(string body,out PanelPage result)
        {
            result=null;if(body==null||body.Length>MaxBody)return false;
            var f=body.Split('|');if(f.Length!=17||f[0]!="G")return false;
            try
            {
                int team,index,total,selected;
                if(!Number(f[2],0,255,out team)||!Number(f[6],0,MaxCatalogChunks-1,out index)||!Number(f[7],1,MaxCatalogChunks,out total)||!Number(f[8],1,9999,out selected)||
                    new[]{f[3],f[9],f[10],f[11],f[12],f[15]}.Any(v=>v!="0"&&v!="1"))return false;
                var rows=f[16].Length==0?Array.Empty<string>():f[16].Split(',');if(rows.Length>6)return false;
                var p=new PanelPage{Nonce=f[1],Team=team,Commander=f[3]=="1",Id=f[4],Function=f[5],Page=index,Pages=total,Selected=selected,Locked=f[9]=="1",Enabled=f[10]=="1",
                    Available=f[11]=="1",Leader=f[12]=="1",Revision=f[13],Message=Unpack(f[14]),Admin=f[15]=="1",Numbers=new int[rows.Length],Names=new string[rows.Length],Details=new string[rows.Length],Groups=new string[rows.Length],Data=new string[rows.Length]};
                for(int i=0;i<rows.Length;i++)
                {var r=rows[i].Split(':');if(r.Length!=5||!Number(r[0],1,9999,out p.Numbers[i]))return false;p.Names[i]=Unpack(r[1]);p.Details[i]=Unpack(r[2]);p.Groups[i]=Unpack(r[3]);p.Data[i]=Unpack(r[4]);}
                Catalog(p);result=p;return true;
            }
            catch(ArgumentException){return false;}catch(FormatException){return false;}
        }
        private static PanelPage Slice(PanelPage source,int start,int count)=>new PanelPage{Nonce=source.Nonce,Id=source.Id,Team=source.Team,Commander=source.Commander,Function=source.Function,
            Revision=source.Revision,Selected=source.Selected,Locked=source.Locked,Enabled=source.Enabled,Available=source.Available,Leader=source.Leader,Admin=source.Admin,Message=source.Message,
            Numbers=source.Numbers.Skip(start).Take(count).ToArray(),Names=source.Names.Skip(start).Take(count).ToArray(),
            Details=Enumerable.Range(start,count).Select(i=>i<source.Details.Length?source.Details[i]:"").ToArray(),
            Groups=Enumerable.Range(start,count).Select(i=>i<source.Groups.Length?source.Groups[i]:"").ToArray(),
            Data=Enumerable.Range(start,count).Select(i=>i<source.Data.Length?source.Data[i]:"").ToArray()};
        public static string[] CatalogChunks(PanelPage source)
        {
            if(source.Numbers.Length>MaxCatalogRows)throw new ArgumentException("Catalog too large.");
            var chunks=new List<PanelPage>();int at=0;
            do
            {
                int take=Math.Min(6,source.Numbers.Length-at);PanelPage chunk;
                while(true)
                {
                    chunk=Slice(source,at,take);chunk.Pages=MaxCatalogChunks;chunk.Page=MaxCatalogChunks-1;
                    try{Catalog(chunk);break;}catch(ArgumentException){if(take<=1)throw;take--;}
                }
                chunks.Add(chunk);at+=take;
            }while(at<source.Numbers.Length);
            for(int i=0;i<chunks.Count;i++){chunks[i].Page=i;chunks[i].Pages=chunks.Count;}
            return chunks.Select(Catalog).ToArray();
        }
    }
}
