using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SiSquadHud
{
    internal sealed class StarterMenuState
    {
        internal string Node="root",File="sol",Pending="",Return="",Name="",From="",At="";
        internal int Rule=-1,Entry=-1,Count=1,Page;
    }
    internal static class StarterUnitsMenus
    {
        private static readonly string[] Factions={"Sol","Centauri","Alien"};
        private static readonly string[] Files={"sol","centauri","alien","wildlife","4way_cross_spawns"};
        private static string[] unitNames;
        internal static bool Available {get;private set;}
        private static string DirectoryPath=>Path.Combine(MelonEnvironment.UserDataDirectory,"StarterUnits");
        internal static void Initialize()
        {
            var mod=AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a=>a.GetType("Si_StarterUnits.StarterUnits",false)!=null);
            if(mod==null)return;
            Available=HudProtocol.Digest(File.ReadAllBytes(mod.Location))=="048c449449f89955e4e2af6e21d75fe6ee466cfdfce523954affad48113412c2";
        }
        internal static void Boundary(){unitNames=null;}
        private static string PathFor(StarterMenuState s)
        {if(!Files.Contains(s.File))throw new InvalidOperationException("Unknown starter configuration.");return Path.Combine(DirectoryPath,s.File+".json");}
        private static string Hash(StarterMenuState s)
        {
            string path=PathFor(s);if(!File.Exists(path))return "missing";
            if(new FileInfo(path).Length>128*1024)throw new InvalidOperationException("Starter configuration exceeds the editor limit.");
            return HudProtocol.Digest(File.ReadAllBytes(path));
        }
        private static JObject Read(StarterMenuState s)
        {
            string path=PathFor(s);if(!File.Exists(path))return new JObject{[s.File=="4way_cross_spawns"?"cross_spawns":"units"]=new JArray()};
            if(new FileInfo(path).Length>128*1024)throw new InvalidOperationException("Starter configuration exceeds the editor limit.");
            using(var input=File.OpenText(path))using(var reader=new JsonTextReader(input){MaxDepth=16})return JObject.Load(reader);
        }
        private static JArray Rules(JObject root)
        {if(!(root["cross_spawns"] is JArray array)){array=new JArray();root["cross_spawns"]=array;}return array;}
        private static JArray Units(JObject root,StarterMenuState s)
        {
            JObject owner=root;
            if(s.File=="4way_cross_spawns")
            {var rules=Rules(root);if(s.Rule<0||s.Rule>=rules.Count||!(rules[s.Rule] is JObject rule))throw new InvalidOperationException("Cross-spawn rule changed. Reopen the menu.");owner=rule;}
            if(!(owner["units"] is JArray array)){array=new JArray();owner["units"]=array;}return array;
        }
        private static string Fingerprint(StarterMenuState state)=>HudProtocol.Digest(Encoding.UTF8.GetBytes(Hash(state)+"|"+state.Node+"|"+state.Rule+"|"+state.Entry+"|"+state.Pending+"|"+state.Count+"|"+state.Name+"|"+state.From+"|"+state.At+"|"+state.Page));
        private static string[] Catalog()
        {
            if(unitNames!=null)return unitNames;
            unitNames=Resources.FindObjectsOfTypeAll<ObjectInfo>().Where(i=>i&&i.Prefab&&i.Prefab.GetComponent<Unit>()!=null&&!string.IsNullOrWhiteSpace(i.DisplayName))
                .Select(i=>i.DisplayName).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n=>n,StringComparer.OrdinalIgnoreCase).ToArray();
            return unitNames;
        }
        private static void Row(AdminMenuPage page,string id,string label,bool input=false)
            =>page.Rows.Add(new AdminMenuRow{Id=id,Label=HudProtocol.ClipUtf8(label,600),Input=input});
        private static AdminMenuPage Build(AdminMenuSession session,string message="")
        {
            var s=session.Starter;var root=Read(s);var p=new AdminMenuPage{Service="starter",Title="Starter Units / "+(s.Node=="root"?"Root":s.File+" / "+s.Node),Message=message,
                CanBack=s.Node!="root"&&s.Node!="confirm",Confirmation=s.Node=="confirm"};
            p.Lines.Add("Changes are saved for the next round. This editor does not spawn or remove live units.");
            switch(s.Node)
            {
                case "root":
                    foreach(string f in Files)Row(p,f,f=="4way_cross_spawns"?"4-way cross-spawns":char.ToUpperInvariant(f[0])+f.Substring(1));break;
                case "cross":
                    var rules=Rules(root);for(int i=0;i<rules.Count;i++)Row(p,"r"+i,(i+1)+". "+(string)rules[i]["from_faction"]+" at "+(string)rules[i]["at_faction"]);
                    Row(p,"add-rule","Add cross-spawn rule");break;
                case "rule":
                    var rule=Rules(root)[s.Rule];Row(p,"from","From faction: "+(string)rule["from_faction"]);Row(p,"at","At faction: "+(string)rule["at_faction"]);
                    Row(p,"units","Units");Row(p,"remove-rule","Remove rule");break;
                case "units":
                    var units=Units(root,s);for(int i=0;i<units.Count;i++)Row(p,"e"+i,(i+1)+". "+(string)units[i]["name"]+" × "+(units[i]["count"]?.ToString()??"1"));
                    Row(p,"add","Add unit");break;
                case "entry":
                    var entry=Units(root,s)[s.Entry];p.Lines.Add((string)entry["name"]+" · current count "+(entry["count"]?.ToString()??"1"));
                    Row(p,"count","Set count (0–1000)",true);Row(p,"remove","Remove unit entry");break;
                case "catalog":
                    var names=Catalog();int pages=Math.Max(1,(names.Length+39)/40);s.Page=Math.Max(0,Math.Min(s.Page,pages-1));
                    for(int i=s.Page*40;i<Math.Min(names.Length,s.Page*40+40);i++)Row(p,"u"+i,names[i]);
                    if(s.Page>0)Row(p,"previous","Previous units");if(s.Page+1<pages)Row(p,"next","More units");
                    if(names.Length==0)p.Lines.Add("Unit prefabs are not available yet. Refresh after the match has loaded.");break;
                case "from":case "at":case "addfrom":case "addat":
                    foreach(string f in Factions)Row(p,f,f);break;
                case "confirm":
                    p.Lines.Add(s.Pending=="count"?"Set count to "+s.Count+"?":s.Pending=="add"?"Add "+s.Name+" × 1?":s.Pending=="add-rule"?"Add "+s.From+" at "+s.At+"?":"Confirm "+s.Pending+"?");
                    Row(p,"yes","Confirm save with backup");Row(p,"no","Cancel");break;
            }
            if(p.Rows.Count>256)throw new InvalidOperationException("Too many entries in this configuration.");
            session.Fingerprint=Fingerprint(s);session.Revision=HudProtocol.Digest(Encoding.UTF8.GetBytes(session.Fingerprint+Guid.NewGuid().ToString("N")));
            p.Revision=session.Revision;session.Page=p;return p;
        }
        private static void Pending(StarterMenuState s,string action,string back)
        {s.Pending=action;s.Return=back;s.Node="confirm";}
        private static void Save(Player actor,StarterMenuState s,JObject root,string expected)
        {
            if(!MenuCatalog.IsAdmin(actor))throw new InvalidOperationException("Administrator Generic power required.");
            if(Hash(s)!=expected)throw new InvalidOperationException("Starter JSON changed. Refresh before saving.");
            IEnumerable<JToken> arrays=s.File=="4way_cross_spawns"?Rules(root).Select(r=>r["units"]):new[]{root["units"]};
            long total=0;foreach(var array in arrays)if(array is JArray entries)foreach(var entry in entries)
            {int count=entry.Value<int?>("count")??1;if(count<0||count>1000)throw new InvalidOperationException("Unit counts must be 0..1000.");total+=count;}
            if(total>5000)throw new InvalidOperationException("The editor limits one configuration to 5000 starter units.");
            string path=PathFor(s);Directory.CreateDirectory(DirectoryPath);bool existed=File.Exists(path);
            string backup=NativeAdminMenus.BackupFile(path,"SquadHUD_Backups");
            if(Hash(s)!=expected||existed&&HudProtocol.Digest(File.ReadAllBytes(backup))!=expected)throw new InvalidOperationException("JSON changed while preparing its backup.");
            byte[] bytes=new UTF8Encoding(false).GetBytes(root.ToString(Formatting.Indented));if(bytes.Length>128*1024)throw new InvalidOperationException("Starter JSON exceeds the editor limit.");
            string temp=path+".squadhud-"+Guid.NewGuid().ToString("N")+".tmp";
            try
            {
                File.WriteAllBytes(temp,bytes);
                if(Hash(s)!=expected)throw new InvalidOperationException("JSON changed; no replacement made.");
                if(existed)File.Replace(temp,path,null);else File.Move(temp,path);
                if(HudProtocol.Digest(File.ReadAllBytes(path))!=HudProtocol.Digest(bytes))throw new IOException("Starter JSON verification failed.");
                MelonLogger.Msg("SquadHUD starter configuration saved by "+actor.PlayerID.SteamID.m_SteamID+": "+s.File+" (next round).");
            }
            finally{if(File.Exists(temp))File.Delete(temp);}
        }
        internal static AdminMenuPage Handle(Player actor,BalanceCommand request,AdminMenuSession session,string choice,string value)
        {
            if(!Available)throw new InvalidOperationException("Reviewed Starter Units adapter unavailable.");
            if(!MenuCatalog.IsAdmin(actor))throw new InvalidOperationException("Administrator Generic power required.");
            if(choice=="open"||session.Service!="starter")
            {session.Service="starter";session.NativeState=null;session.Starter=new StarterMenuState();return Build(session);}
            if(request.Revision!=session.Revision)throw new InvalidOperationException("Menu changed. Refresh before choosing.");
            var s=session.Starter;
            if(session.Fingerprint!=Fingerprint(s)){session.Starter=new StarterMenuState();return Build(session,"JSON changed. Reopened Root without applying the stale choice.");}
            if(choice=="refresh")return Build(session);
            if(choice=="back")
            {
                if(!session.Page.CanBack)throw new InvalidOperationException("Back is unavailable.");
                s.Node=s.Node=="cross"||s.Node=="units"&&s.File!="4way_cross_spawns"?"root":s.Node=="units"||s.Node=="from"||s.Node=="at"?"rule":s.Node=="rule"||s.Node=="addfrom"?"cross":s.Node=="addat"?"addfrom":"units";
                return Build(session);
            }
            var row=session.Page.Rows.FirstOrDefault(r=>r.Id==choice&&r.Enabled);if(row==null)throw new InvalidOperationException("Unknown starter menu entry.");
            if(value.Length>32||value.Length>0&&!row.Input)throw new InvalidOperationException("Invalid menu value.");
            var root=Read(s);string hash=Hash(s),message="";
            switch(s.Node)
            {
                case "root":s.File=choice;s.Rule=s.Entry=-1;s.Node=choice=="4way_cross_spawns"?"cross":"units";break;
                case "cross":
                    if(choice=="add-rule")s.Node="addfrom";else{s.Rule=int.Parse(choice.Substring(1),CultureInfo.InvariantCulture);s.Node="rule";}break;
                case "rule":
                    if(choice=="remove-rule")Pending(s,"remove-rule","rule");else s.Node=choice;break;
                case "units":
                    if(choice=="add"){s.Page=0;s.Node="catalog";}else{s.Entry=int.Parse(choice.Substring(1),CultureInfo.InvariantCulture);s.Node="entry";}break;
                case "entry":
                    if(choice=="remove")Pending(s,"remove","entry");
                    else{if(!int.TryParse(value,NumberStyles.Integer,CultureInfo.InvariantCulture,out int count)||count<0||count>1000)throw new InvalidOperationException("Enter an integer count from 0 to 1000.");s.Count=count;Pending(s,"count","entry");}break;
                case "catalog":
                    if(choice=="next")s.Page++;else if(choice=="previous")s.Page--;
                    else{s.Name=Catalog()[int.Parse(choice.Substring(1),CultureInfo.InvariantCulture)];Pending(s,"add","catalog");}break;
                case "from":s.From=choice;Pending(s,"from","rule");break;
                case "at":s.At=choice;Pending(s,"at","rule");break;
                case "addfrom":s.From=choice;s.Node="addat";break;
                case "addat":s.At=choice;Pending(s,"add-rule","cross");break;
                case "confirm":
                    if(choice=="no"){s.Node=s.Return;s.Pending="";break;}
                    switch(s.Pending)
                    {
                        case "count":Units(root,s)[s.Entry]["count"]=s.Count;s.Node="entry";break;
                        case "remove":Units(root,s).RemoveAt(s.Entry);s.Entry=-1;s.Node="units";break;
                        case "add":Units(root,s).Add(new JObject{["name"]=s.Name,["count"]=1});s.Node="units";break;
                        case "remove-rule":Rules(root).RemoveAt(s.Rule);s.Rule=-1;s.Node="cross";break;
                        case "from":Rules(root)[s.Rule]["from_faction"]=s.From;s.Node="rule";break;
                        case "at":Rules(root)[s.Rule]["at_faction"]=s.At;s.Node="rule";break;
                        case "add-rule":Rules(root).Add(new JObject{["from_faction"]=s.From,["at_faction"]=s.At,["units"]=new JArray()});s.Node="cross";break;
                        default:throw new InvalidOperationException("Unknown pending starter edit.");
                    }
                    Save(actor,s,root,hash);s.Pending="";message="Saved with verified backup. Takes effect next round.";break;
            }
            return Build(session,message);
        }
    }
}
