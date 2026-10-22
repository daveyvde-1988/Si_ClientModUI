using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SiSquadHud
{
    internal sealed class AdminMenuSession
    {
        internal string Service="",Revision="",Fingerprint="";
        internal object NativeState;
        internal AdminMenuPage Page;
        internal StarterMenuState Starter=new StarterMenuState();
    }
    internal static class NativeAdminMenus
    {
        private const BindingFlags Flags=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
        private static Type unitBalance;
        private static MethodInfo open,process,show,key;
        private static IDictionary states,stats;
        private static HarmonyLib.Harmony hooks;
        private static object captureActor;
        private static readonly List<string> captured=new List<string>();
        private static bool backedUp;
        private static string ConfigPath=>Path.Combine(MelonEnvironment.UserDataDirectory,"UnitBalance_cfg","Si_UnitBalance_Config.json");
        internal static bool Available {get;private set;}
        internal static void Initialize()
        {
            if(!UnitBalanceBackend.Available)return;
            unitBalance=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Si_UnitBalance.UnitBalance",false)).FirstOrDefault(t=>t!=null);
            open=unitBalance?.GetMethod("OnBalanceCommand",Flags);process=unitBalance?.GetMethod("ProcessMenuInput",Flags);
            show=unitBalance?.GetMethod("ShowCurrentMenu",Flags);key=unitBalance?.GetMethod("GetPlayerKey",Flags);
            states=MenuCatalog.Read(unitBalance,"_menuStates") as IDictionary;stats=MenuCatalog.Read(unitBalance,"_statsStates") as IDictionary;
            if(open==null||process==null||show==null||key==null||states==null)return;
            hooks=new HarmonyLib.Harmony("SiSquadHUD.NativeAdminMenus");
            foreach(string name in new[]{"SendChatToPlayer","SendConsoleToPlayer"})
            {
                var method=unitBalance.GetMethod(name,Flags);if(method==null)throw new InvalidOperationException("Native menu output method unavailable.");
                hooks.Patch(method,prefix:new HarmonyMethod(typeof(NativeAdminMenus).GetMethod(nameof(Capture),Flags)));
            }
            foreach(var method in unitBalance.GetMethods(Flags).Where(m=>m.Name.StartsWith("Write",StringComparison.Ordinal)&&m.Name.EndsWith("ToJson",StringComparison.Ordinal)||new[]{"ResetToBlankConfig","LoadSavedConfig"}.Contains(m.Name)))
                hooks.Patch(method,prefix:new HarmonyMethod(typeof(NativeAdminMenus).GetMethod(nameof(BeforeWrite),Flags)));
            Available=true;
        }
        internal static void Shutdown(){hooks?.UnpatchSelf();hooks=null;Available=false;captureActor=null;}
        private static bool Capture(object __0,string __1)
        {
            if(captureActor==null||!ReferenceEquals(captureActor,__0))return true;
            if(captured.Count<160&&captured.Sum(s=>s.Length)<32000)captured.Add(__1??"");return false;
        }
        private static void BeforeWrite()
        {
            if(captureActor==null||backedUp)return;
            BackupFile(ConfigPath,"SquadHUD_MenuBackups");backedUp=true;
        }
        internal static string BackupFile(string path,string folder)
        {
            if(!File.Exists(path))return "";
            byte[] bytes=File.ReadAllBytes(path);if(bytes.Length>HudProtocol.MaxBalanceBytes)throw new InvalidOperationException("Configuration exceeds backup limit.");
            string directory=Path.Combine(Path.GetDirectoryName(path),folder);Directory.CreateDirectory(directory);
            string backup=Path.Combine(directory,Path.GetFileNameWithoutExtension(path)+"-"+DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ")+"-"+Guid.NewGuid().ToString("N")+".json");
            File.WriteAllBytes(backup,bytes);if(HudProtocol.Digest(File.ReadAllBytes(backup))!=HudProtocol.Digest(bytes))throw new IOException("Backup verification failed; edit cancelled.");
            MelonLogger.Msg("SquadHUD verified configuration backup: "+backup);return backup;
        }
        private static string Clean(string text)
        {
            text=Regex.Replace(text??"","<[^>]*>","");
            text=Regex.Replace(text,@"https?://[^\s]*discord(?:app)?\.com/api/webhooks/[^\s]+","[private webhook]");
            text=Regex.Replace(text,@"^\s*\[[^\]]+\]\s*","");
            return text.Replace('\r',' ').Replace('\n',' ').Trim();
        }
        private static string Fingerprint(AdminMenuSession session)
        {
            if(File.Exists(ConfigPath)&&new FileInfo(ConfigPath).Length>HudProtocol.MaxBalanceBytes)throw new InvalidOperationException("Balance configuration exceeds the menu limit.");
            string file=File.Exists(ConfigPath)?HudProtocol.Digest(File.ReadAllBytes(ConfigPath)):"missing";
            return HudProtocol.Digest(Encoding.UTF8.GetBytes(file+"|"+JsonConvert.SerializeObject(session.NativeState)));
        }
        private static bool Flag(object state,string name)=>MenuCatalog.Read(state,name) as bool? ?? false;
        private static string Text(object state,string name)=>MenuCatalog.Read(state,name)?.ToString()??"";
        private static void InNativeScope(Player actor,AdminMenuSession session,Action<object> action)
        {
            object playerKey=key.Invoke(null,new object[]{actor});
            bool hadState=states.Contains(playerKey),hadStats=stats?.Contains(playerKey)??false;
            object old=hadState?states[playerKey]:null,oldStats=hadStats?stats[playerKey]:null;
            if(session.NativeState==null)states.Remove(playerKey);else states[playerKey]=session.NativeState;
            captureActor=actor;captured.Clear();backedUp=false;
            try{action(playerKey);session.NativeState=states.Contains(playerKey)?states[playerKey]:null;}
            finally
            {
                captureActor=null;
                if(hadState)states[playerKey]=old;else states.Remove(playerKey);
                if(stats!=null){if(hadStats)stats[playerKey]=oldStats;else stats.Remove(playerKey);}
            }
        }
        private static AdminMenuPage Build(AdminMenuSession session,string message)
        {
            var page=new AdminMenuPage{Service="balance",Title="Unit Balance / "+Text(session.NativeState,"Level"),Message=message};
            bool pending=Flag(session.NativeState,"PendingConfirm"),jsonPending=Text(session.NativeState,"JsonPendingAction").Length>0,
                naming=Flag(session.NativeState,"JsonPendingSave");
            page.Confirmation=pending||jsonPending||naming;
            page.CanBack=session.NativeState!=null&&Text(session.NativeState,"Level")!="Root"&&!page.Confirmation;
            var entries=new Dictionary<string,AdminMenuRow>();
            foreach(string raw in captured)
            {
                string line=Clean(raw);if(line.Length==0)continue;
                var matches=Regex.Matches(line,@"(?<![\w.])(?<id>\d{1,3})\.\s+");
                if(matches.Count==0){string info=HudProtocol.ClipUtf8(line,1500);if(page.Lines.Count<30&&!page.Lines.Contains(info))page.Lines.Add(info);continue;}
                for(int i=0;i<matches.Count;i++)
                {
                    var match=matches[i];int start=match.Index+match.Length,end=i+1<matches.Count?matches[i+1].Index:line.Length;
                    string label=line.Substring(start,end-start).Trim();if(label.Length==0)continue;
                    string id=match.Groups["id"].Value;
                    bool privateMenu=label.IndexOf("Discord",StringComparison.OrdinalIgnoreCase)>=0;
                    entries[id]=new AdminMenuRow{Id=id,Label=HudProtocol.ClipUtf8(label,600),Input=label.Contains("="),Enabled=!privateMenu,
                        Hint=privateMenu?"Private Discord configuration remains in the native chat editor.":"Uses the existing Unit Balance menu handler."};
                }
            }
            if(pending)
            {
                entries.Clear();entries["1"]=new AdminMenuRow{Id="1",Label="Save JSON"};entries["2"]=new AdminMenuRow{Id="2",Label="Cancel"};
                entries["3"]=new AdminMenuRow{Id="3",Label="Save JSON and rebalance"};
            }
            else if(jsonPending)
            {entries.Clear();entries["1"]=new AdminMenuRow{Id="1",Label="Confirm "+Text(session.NativeState,"JsonPendingAction")};entries["2"]=new AdminMenuRow{Id="2",Label="Cancel"};}
            else if(naming)
            {entries.Clear();entries["name"]=new AdminMenuRow{Id="name",Label="Save named configuration (blank uses timestamp)",Input=true};}
            page.Rows=entries.Values.Take(256).ToList();
            if(session.NativeState==null)page.Message="Native editor closed. Choose Root to reopen.";
            session.Fingerprint=Fingerprint(session);session.Revision=HudProtocol.Digest(Encoding.UTF8.GetBytes(session.Fingerprint+Guid.NewGuid().ToString("N")));
            page.Revision=session.Revision;session.Page=page;return page;
        }
        internal static AdminMenuPage Handle(Player actor,BalanceCommand request,AdminMenuSession session)
        {
            if(!MenuCatalog.IsAdmin(actor))throw new InvalidOperationException("Administrator Generic power required.");
            var payload=JObject.Parse(request.Payload);string service=payload.Value<string>("service")??"",choice=payload.Value<string>("choice")??"",value=payload.Value<string>("value")??"";
            if(service=="starter")return StarterUnitsMenus.Handle(actor,request,session,choice,value);
            if(service!="balance"||!Available)throw new InvalidOperationException("Reviewed Unit Balance menu adapter unavailable.");
            if(value.Length>64||value.Any(char.IsControl))throw new InvalidOperationException("Menu input is too long or invalid.");
            bool root=choice=="open"||session.Service!="balance";
            if(!root&&request.Revision!=session.Revision)throw new InvalidOperationException("Menu changed; refresh before choosing.");
            string message="";
            if(!root&&session.Fingerprint!=Fingerprint(session))
            {root=true;message="Server JSON changed. Reopened Root without applying the stale choice.";}
            if(root){session.Service="balance";session.NativeState=null;}
            if(choice=="close"){session.NativeState=null;return Build(session,"Menu closed.");}
            InNativeScope(actor,session,playerKey=>
            {
                if(root){open.Invoke(null,new object[]{actor,"!b"});return;}
                if(choice=="refresh"){if(session.NativeState!=null)show.Invoke(null,new[]{(object)actor,session.NativeState});return;}
                if(choice=="reload")
                {
                    if(session.Page?.Confirmation==true)throw new InvalidOperationException("Finish or cancel the current confirmation before reload.");
                    UnitBalanceBackend.RefreshDisk(true);
                    var reloadRequest=new BalanceCommand{Operation="reload",Revision=UnitBalanceBackend.Revision};
                    message=UnitBalanceBackend.Handle(actor,reloadRequest,new BalanceDraft(),out bool ok);if(!ok)throw new InvalidOperationException(message);
                    if(session.NativeState!=null)show.Invoke(null,new[]{(object)actor,session.NativeState});return;
                }
                if(choice=="back")
                {if(session.Page?.CanBack!=true)throw new InvalidOperationException("Back is unavailable for this menu.");process.Invoke(null,new object[]{actor,"back"});return;}
                var row=session.Page?.Rows.FirstOrDefault(r=>r.Id==choice&&r.Enabled);
                if(row==null)throw new InvalidOperationException("Unknown or disabled menu entry.");
                if(value.Length>0&&!row.Input)throw new InvalidOperationException("This entry accepts no value.");
                string input;
                if(choice=="name")
                {if(!Regex.IsMatch(value,@"^[A-Za-z0-9_. -]{0,64}$"))throw new InvalidOperationException("Use a simple configuration name without path characters.");input=value.Length==0?"1":value;}
                else
                {
                    if(value.Length>0&&!new[]{"on","off","true","false","default","vanilla","reset","-"}.Contains(value.ToLowerInvariant()))
                    {
                        if(!double.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out double number)||double.IsNaN(number)||double.IsInfinity(number)||Math.Abs(number)>100000000)
                            throw new InvalidOperationException("Enter one finite numeric value (decimal point), or a supported native default/on/off value.");
                    }
                    input=choice+(value.Length>0?" "+value:"");
                }
                process.Invoke(null,new object[]{actor,input});
            });
            // A setter may only print a receipt; append the current entries for a usable mouse view.
            if(session.NativeState!=null&&!Flag(session.NativeState,"PendingConfirm")&&!Flag(session.NativeState,"JsonPendingSave")&&Text(session.NativeState,"JsonPendingAction").Length==0)
            {
                var output=captured.ToArray();InNativeScope(actor,session,k=>show.Invoke(null,new[]{(object)actor,session.NativeState}));
                captured.InsertRange(0,output);
            }
            return Build(session,message);
        }
    }
}
