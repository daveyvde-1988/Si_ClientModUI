using System;
using System.Globalization;
using System.Linq;

namespace SiSquadHud
{
    public sealed class PanelRequest
    {
        public string Nonce="",Id="",Function="follow",Action="",Revision="",Known="";
        public int Channel,Team,Page,Option;
        public bool Commander;
        public bool IsAction=>Action.Length>0;
    }
    public sealed class PanelPage
    {
        public string Nonce="",Id="",Function="follow",Revision="-",Message="";
        public int Team,Page,Pages=1,Selected=1;
        public bool Commander,Locked,Enabled,Available,Leader;
        public int[] Numbers=Array.Empty<int>();
        public string[] Names=Array.Empty<string>();
        public string[] Details=Array.Empty<string>();
        public string[] Groups=Array.Empty<string>(),Data=Array.Empty<string>();
        public bool Admin;
    }
    public static partial class HudProtocol
    {
        public static bool IsFormation(string value)=>new[]{"follow","move","attack","commander"}.Contains(value);
        private static bool Function(string value)=>IsFormation(value)||new[]{"buy","ranks","help-fps","help-commander","help-admin","admin"}.Contains(value);
        private static bool Revision(string value)=>value!=null&&value.Length==64&&value.All(c=>c>='0'&&c<='9'||c>='a'&&c<='f');
        public static string Control(PanelRequest r)
        {
            if(r==null||!ValidNonce(r.Nonce)||!ValidNonce(r.Id)||!Function(r.Function)||r.Team<0||r.Team>255||r.Channel<0||r.Channel>255||r.Page<0||r.Page>2048||
                (IsFormation(r.Function)&&(r.Commander?r.Function!="commander":r.Function=="commander")))throw new ArgumentException("Invalid panel scope.");
            if(r.Known.Length>0&&!Revision(r.Known))throw new ArgumentException("Invalid cached catalog fingerprint.");
            string result=string.Join("|",r.IsAction?"A":r.Known.Length>0?"P4":"P",r.Nonce,r.Channel.ToString(Inv),r.Team.ToString(Inv),r.Commander?"1":"0",r.Id,r.Function,r.Page.ToString(Inv));
            if(!r.IsAction&&r.Known.Length>0)result+="|"+r.Known;
            if(r.IsAction)
            {
                if(!Revision(r.Revision)||!((IsFormation(r.Function)&&((r.Action=="select"&&r.Option>=1&&r.Option<=9999)||(r.Action=="lock"&&(r.Option==0||r.Option==1))||(r.Action=="disband"&&r.Option==0)))||(r.Function=="buy"&&r.Action=="purchase"&&r.Option>=1&&r.Option<=9999)||(r.Function=="admin"&&(r.Action=="enable"||r.Action=="disable")&&r.Option>=1&&r.Option<=9999)))throw new ArgumentException("Invalid panel action.");
                result+="|"+r.Action+"|"+r.Option.ToString(Inv)+"|"+r.Revision;
            }
            return result;
        }
        public static bool TryControl(string text,out PanelRequest result)
        {
            result=null;
            if(text==null||text.Length>256)return false;
            var p=text.Split('|');
            if(!((p.Length==8&&p[0]=="P")||(p.Length==9&&p[0]=="P4")||(p.Length==11&&p[0]=="A")))return false;
            int channel,team,page,option=0;
            if(!Number(p[2],0,255,out channel)||!Number(p[3],0,255,out team)||!Number(p[7],0,2048,out page)||(p[4]!="0"&&p[4]!="1"))return false;
            if(p.Length==11&&!Number(p[9],0,9999,out option))return false;
            var r=new PanelRequest{Nonce=p[1],Channel=channel,Team=team,Commander=p[4]=="1",Id=p[5],Function=p[6],Page=page,Option=option,
                Action=p.Length==11?p[8]:"",Revision=p.Length==11?p[10]:"",Known=p.Length==9?p[8]:""};
            try{Control(r);}catch(ArgumentException){return false;}result=r;return true;
        }
        public static string Page(PanelPage p)
        {
            if(p==null||!ValidNonce(p.Nonce)||!ValidNonce(p.Id)||!Function(p.Function)||p.Team<0||p.Team>255||p.Page<0||p.Pages<1||p.Pages>2048||p.Page>=p.Pages||
                p.Selected<1||p.Selected>9999||(!Revision(p.Revision)&&p.Revision!="-")||!Safe(p.Message,160)||p.Numbers.Length!=p.Names.Length||p.Numbers.Length>6||
                p.Numbers.Any(n=>n<1||n>9999)||p.Numbers.Distinct().Count()!=p.Numbers.Length||p.Names.Any(n=>!Safe(n,96)||Utf8.GetByteCount(n)>96)||
                (p.Details.Length>0&&(p.Numbers.Length>3||p.Details.Length!=p.Numbers.Length||p.Details.Any(n=>!Safe(n,200)||Utf8.GetByteCount(n)>200))))throw new ArgumentException("Invalid formation page.");
            var items=string.Join(",",p.Numbers.Select((n,i)=>n.ToString(Inv)+":"+Pack(p.Names[i])));
            var result=string.Join("|","F",p.Nonce,p.Team.ToString(Inv),p.Commander?"1":"0",p.Id,p.Function,p.Page.ToString(Inv),p.Pages.ToString(Inv),p.Selected.ToString(Inv),
                p.Locked?"1":"0",p.Enabled?"1":"0",p.Available?"1":"0",p.Leader?"1":"0",p.Revision,Pack(p.Message),items);
            if(p.Details.Length>0)result+="|"+string.Join(",",p.Details.Select(Pack));
            if(result.Length>MaxBody)throw new ArgumentException("Formation page exceeds packet budget.");
            return result;
        }
        public static bool TryPage(string text,out PanelPage result)
        {
            result=null;if(text==null||text.Length>MaxBody)return false;var p=text.Split('|');if((p.Length!=16&&p.Length!=17)||p[0]!="F")return false;
            try
            {
                int team,page,pages,selected;
                if(!Number(p[2],0,255,out team)||!Number(p[6],0,2048,out page)||!Number(p[7],1,2048,out pages)||!Number(p[8],1,9999,out selected)||
                    new[]{p[3],p[9],p[10],p[11],p[12]}.Any(v=>v!="0"&&v!="1"))return false;
                var rows=p[15].Length==0?Array.Empty<string>():p[15].Split(',');if(rows.Length>6)return false;
                var nums=new int[rows.Length];var names=new string[rows.Length];
                for(int i=0;i<rows.Length;i++){var row=rows[i].Split(':');if(row.Length!=2||!Number(row[0],1,9999,out nums[i]))return false;names[i]=Unpack(row[1]);}
                var r=new PanelPage{Nonce=p[1],Team=team,Commander=p[3]=="1",Id=p[4],Function=p[5],Page=page,Pages=pages,Selected=selected,Locked=p[9]=="1",
                    Enabled=p[10]=="1",Available=p[11]=="1",Leader=p[12]=="1",Revision=p[13],Message=Unpack(p[14]),Numbers=nums,Names=names,
                    Details=p.Length==17?p[16].Split(',').Select(Unpack).ToArray():Array.Empty<string>()};
                Page(r);result=r;return true;
            }
            catch(Exception e)when(e is FormatException||e is ArgumentException){return false;}
        }
        public static string ClipUtf8(string value,int budget)
        {
            value=(value??"").Replace('\r',' ').Replace('\n',' ').Replace('\t',' ');
            if(Utf8.GetByteCount(value)<=budget)return value;
            int length=value.Length;while(length>0&&(char.IsHighSurrogate(value[length-1])||Utf8.GetByteCount(value.Substring(0,length))+3>budget))length--;
            return value.Substring(0,length)+"...";
        }
    }
}
