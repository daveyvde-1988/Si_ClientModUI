using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SiSquadHud
{
    internal sealed class BalanceDraft
    {
        internal string Id="",Revision="";
        internal readonly Dictionary<int,JToken> Values=new Dictionary<int,JToken>();
        internal readonly HashSet<int> Removed=new HashSet<int>();
        internal void Clear(){Id=Revision="";Values.Clear();Removed.Clear();}
    }
    // Adapter for the installed 7.7.1 binary. This never modifies or replaces UnitBalance.dll.
    internal static class UnitBalanceBackend
    {
        private const string ReviewedHash="23e5f58f3337e081b784d4d940f46ec81451c630c900cc3610fac11f68609c66";
        private const BindingFlags Flags=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
        private static Type mod;
        private static MethodInfo reload,groups,bases;
        private static HarmonyLib.Harmony adapter;
        private static bool reviewed,initialized,lastReloadConfirmed;
        private static string path="",diskRevision=new string('0',64),publishedRevision="",schemaRevision="",publicJson="",adminJson="";
        private static long generation,snapshotSequence;
        private static JObject disk,published,defaults;
        private static JArray schema=new JArray();
        private static string lastState="Unavailable",diskSignature="";
        private static readonly string[] Globals={"enabled","description","game_version","dump_fields","health_mult_enabled","shrimp_disable_aim","additional_spawn","watchdog_enabled",
            "player_infantry_ignore_cap","alien_nest_only_build","alien_nest_reach_radius","alien_nest_finished_anchor_only","alien_queen_logistics_per_nest","alien_queens_max","count_caps","unit_cap_mult","tech_time","tech_cost","decay"};
        internal static bool Available=>reviewed&&initialized;
        internal static bool Enabled=>Available&&(MenuCatalog.Read(mod,"_enabled") as bool? ?? false);
        internal static string Revision=>HudProtocol.Digest(Encoding.UTF8.GetBytes(diskRevision+"|"+generation));
        internal static void Initialize()
        {
            mod=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Si_UnitBalance.UnitBalance",false)).FirstOrDefault(t=>t!=null);
            if(mod==null)return;
            using(var stream=File.OpenRead(mod.Assembly.Location))using(var bytes=new MemoryStream()){stream.CopyTo(bytes);reviewed=HudProtocol.Digest(bytes.ToArray())==ReviewedHash;}
            if(!reviewed){MelonLogger.Warning("SquadHUD: Unit Balance adapter disabled; DLL hash requires review.");return;}
            path=Path.Combine(MelonEnvironment.UserDataDirectory,"UnitBalance_cfg","Si_UnitBalance_Config.json");
            if(!string.Equals(Path.GetFullPath(MenuCatalog.Read(mod,"_configPath") as string??""),Path.GetFullPath(path),StringComparison.OrdinalIgnoreCase))
            {reviewed=false;return;}
            reload=mod.GetMethod("OnRebalanceCommand",Flags);groups=mod.GetMethod("GetUnitParamGroups",Flags);bases=mod.GetMethod("GetBaseValues",Flags);
            if(reload==null||groups==null||bases==null){reviewed=false;return;}
            adapter=new HarmonyLib.Harmony("SiSquadHUD.UnitBalanceAdapter");
            adapter.Patch(reload,postfix:new HarmonyMethod(typeof(UnitBalanceBackend).GetMethod(nameof(AfterReload),Flags)));
            RefreshDisk(true);published=(JObject)disk.DeepClone();publishedRevision=diskRevision;lastState="Loaded configuration; native runtime values may differ";
            initialized=true;Invalidate();
        }
        private static JObject ReadJson(string file)
        {
            var info=new FileInfo(file);if(!info.Exists||info.Length>HudProtocol.MaxBalanceBytes)throw new InvalidOperationException("Balance configuration missing or too large.");
            using(var input=File.OpenText(file))using(var reader=new JsonTextReader(input){MaxDepth=32,DateParseHandling=DateParseHandling.None})return JObject.Load(reader);
        }
        internal static void RefreshDisk(bool force=false)
        {
            if(!reviewed)return;
            var info=new FileInfo(path);string signature=info.Length+":"+info.LastWriteTimeUtc.Ticks;
            if(!force&&signature==diskSignature)return;
            byte[] bytes=File.ReadAllBytes(path);if(bytes.Length>HudProtocol.MaxBalanceBytes)throw new InvalidOperationException("Balance configuration too large.");
            var next=ReadJson(path);if(!(next["units"] is JObject))throw new InvalidOperationException("Balance configuration has no units object.");
            string hash=HudProtocol.Digest(bytes);
            disk=next;diskSignature=signature;
            if(defaults==null)defaults=ReadJson(Path.Combine(Path.GetDirectoryName(path),"Si_UnitBalance_Config_Default.json"));
            if(hash!=diskRevision){diskRevision=hash;schemaRevision="";adminJson="";}
        }
        private static void Invalidate(){generation++;publicJson=adminJson="";schemaRevision="";}
        internal static void Boundary(){if(Available)Invalidate();}
        internal static void Shutdown(){adapter?.UnpatchSelf();adapter=null;initialized=reviewed=false;}
        // Observes both HUD requests and the original chat command. No Discord operation is invoked here.
        private static void AfterReload(string __1)
        {
            if(!reviewed)return;
            lastReloadConfirmed=false;
            try
            {
                RefreshDisk(true);
                defaults=ReadJson(Path.Combine(Path.GetDirectoryName(path),"Si_UnitBalance_Config_Default.json"));
                bool vanilla=(__1??"").IndexOf("default",StringComparison.OrdinalIgnoreCase)>=0;
                bool loaded=MenuCatalog.Read(mod,"_configLoaded") as bool? ?? false;
                bool applied=MenuCatalog.Read(mod,"_overridesApplied") as bool? ?? false;
                if(!vanilla&&(!loaded||(Enabled&&!applied))){lastState="Reload did not confirm applied overrides; inspect server log";Invalidate();return;}
                published=(JObject)(vanilla?defaults:disk).DeepClone();publishedRevision=vanilla?"vanilla":diskRevision;
                lastReloadConfirmed=true;
                lastState=vanilla?"Vanilla defaults restored":Enabled?"Configuration reloaded; consult native log for individual override results":"Unit Balance disabled";
                Invalidate();
            }
            catch(Exception){lastState="Balance snapshot unavailable after reload; inspect server log";Invalidate();}
        }
        private static bool PrivateKey(string key)=>new[]{"discord","webhook","password","secret","token","credential"}.Any(s=>key.IndexOf(s,StringComparison.OrdinalIgnoreCase)>=0);
        private static JToken Sanitize(JToken value)
        {
            if(value is JObject obj)
            {
                var copy=new JObject();foreach(var p in obj.Properties())if(!PrivateKey(p.Name)&&!p.Name.StartsWith("_comment",StringComparison.OrdinalIgnoreCase))copy[p.Name]=Sanitize(p.Value);return copy;
            }
            if(value is JArray array)return new JArray(array.Select(Sanitize));return value.DeepClone();
        }
        private static JObject PublicConfig(JObject source)
        {
            var result=new JObject();if(source==null)return result;
            foreach(string name in Globals)if(source[name]!=null)result[name]=Sanitize(source[name]);
            if(source["units"] is JObject units)result["units"]=Sanitize(units);return result;
        }
        private static string Key(IEnumerable<string> p)=>string.Join("\u001f",p);
        private static JToken At(JToken root,IEnumerable<string> pathParts)
        {foreach(string part in pathParts){root=(root as JObject)?[part];if(root==null)return null;}return root;}
        private static string GroupFor(string key)
        {
            if(key.StartsWith("pri_"))return "Primary Weapon";if(key.StartsWith("sec_"))return "Secondary Weapon";
            if(new[]{"health","cost","build_time","min_tier","unit_cap"}.Any(key.StartsWith))return "Health & Production";
            if(new[]{"move_","run_","sprint_","strafe_","fly_","jump_","turbo_","turn_"}.Any(key.StartsWith))return "Movement";
            if(new[]{"fow_","target_","visible_"}.Any(key.StartsWith))return "Vision & Sense";
            if(key.Contains("radius")&&!key.Contains("splash"))return "Placement";
            return "Weapon & Other Settings";
        }
        private static string FieldType(string key,JToken value)
        {
            if(value?.Type==JTokenType.Boolean||key=="impact_scale_by_speed"||new[]{"FlyingUnits","GroundUnits","Structures"}.Contains(key))return "bool";
            if(value!=null&&value.Type!=JTokenType.Float&&value.Type!=JTokenType.Integer&&value.Type!=JTokenType.Null)return "info";
            return new[]{"min_tier","unit_cap_value","aim_compensation","alien_queens_max","shrimp","harvester","two_way","three_way"}.Contains(key)?"int":"number";
        }
        private static void AddField(Dictionary<string,JObject> fields,string[] parts,string group,JToken value,string baseline="")
        {
            if(parts.Length>9||parts.Any(p=>p.Length>160||PrivateKey(p)||p.Any(char.IsControl)))return;
            string key=Key(parts),name=parts.Last(),entity=parts[0]=="units"?parts[1]:"Global Settings";
            string type=parts[0]=="tech_cost"?"int":FieldType(name,value);bool editable=type!="info"&&!name.StartsWith("_")&&parts[0]!="enabled"&&parts[0]!="game_version";
            if(fields.TryGetValue(key,out var existing)){if(baseline.Length>0)existing["baseline"]=baseline;return;}
            fields[key]=new JObject{["path"]=new JArray(parts),["entity"]=entity,["group"]=group,["name"]=name,["type"]=type,["editable"]=editable,["baseline"]=baseline};
        }
        private static void Flatten(Dictionary<string,JObject> fields,JToken node,string[] parts)
        {
            if(node is JObject obj)
            {
                foreach(var p in obj.Properties())Flatten(fields,p.Value,parts.Concat(new[]{p.Name}).ToArray());return;
            }
            if(parts.Length<1||parts[0]=="units"&&parts.Length<3)return;
            string group=parts[0]=="units"?(parts.Length>3?string.Join(" / ",parts.Skip(2).Take(parts.Length-3)):GroupFor(parts.Last())):
                parts.Length>1?parts[0].Replace('_',' '):"General Rules";
            AddField(fields,parts,group,node);
        }
        private static void EnsureSchema()
        {
            if(schemaRevision==diskRevision)return;
            var fields=new Dictionary<string,JObject>(StringComparer.Ordinal);var clean=PublicConfig(disk);var def=PublicConfig(defaults);
            Flatten(fields,def,Array.Empty<string>());Flatten(fields,clean,Array.Empty<string>());
            var units=((JObject)clean["units"]).Properties().Select(p=>p.Name).Concat(((JObject)def["units"]).Properties().Select(p=>p.Name)).Distinct().Where(n=>!n.StartsWith("_")).ToArray();
            foreach(string unit in units)
            {
                try
                {
                    object[] args={unit,null,null};groups.Invoke(null,args);var names=(string[])args[1];var keys=(string[][])args[2];
                    // Only a captured vanilla cache is a baseline; a live fallback may already contain overrides.
                    var baseCache=MenuCatalog.Read(mod,"_vanillaBaseCache") as Dictionary<string,Dictionary<string,string>>;
                    Dictionary<string,string> baseValues=null;baseCache?.TryGetValue(unit,out baseValues);
                    for(int g=0;g<names.Length;g++)foreach(string param in keys[g])
                    {
                        string[] parts;
                        if(param.StartsWith("prox_"))
                        {
                            var find=mod.GetMethod("FindUnitProjectileDataName",Flags);string projectile=find?.Invoke(null,new object[]{unit}) as string;
                            if(string.IsNullOrEmpty(projectile))continue;
                            parts=new[]{"units",unit,"projectiles",projectile,"proximity",param.Substring(5)};
                        }
                        else parts=new[]{"units",unit,param};
                        JToken val=At(clean,parts)??At(def,parts);string baseValue="";baseValues?.TryGetValue(param,out baseValue);
                        AddField(fields,parts,names[g],val,baseValue??"");
                        if(fields.TryGetValue(Key(parts),out var field))field["group"]=names[g];
                    }
                }
                catch(Exception){/* Missing prefab: retain the JSON fields and label absent baselines as unavailable. */}
            }
            // Reviewed optional global parameters absent from older default files.
            foreach(var pair in new Dictionary<string,JToken>{{"player_infantry_ignore_cap",false},{"alien_nest_only_build",false},{"alien_nest_reach_radius",-1f},{"alien_nest_finished_anchor_only",false},{"alien_queen_logistics_per_nest",false},{"alien_queens_max",-1}})
                AddField(fields,new[]{pair.Key},"General Rules",pair.Value);
            foreach(string name in new[]{"shrimp","harvester"})AddField(fields,new[]{"count_caps",name},"Count Caps",new JValue(-1));
            foreach(string name in new[]{"two_way","three_way"})AddField(fields,new[]{"unit_cap_mult",name},"Unit Cap Multipliers",new JValue(-1));
            for(int tier=1;tier<=8;tier++){AddField(fields,new[]{"tech_cost","tier_"+tier},"Tech Cost",new JValue(0));AddField(fields,new[]{"tech_time","tier_"+tier},"Tech Time",new JValue(0f));}
            if(fields.Count>10000)throw new InvalidOperationException("Balance schema too large.");
            schema=new JArray(fields.OrderBy(p=>p.Key,StringComparer.Ordinal).Select((p,i)=>{p.Value["id"]=i;return p.Value;}));schemaRevision=diskRevision;
        }
        internal static string Stamp(bool admin)
            =>Available?generation+":"+Enabled+":"+(admin?diskRevision:"public"):(reviewed?"not-ready":"unavailable");
        internal static string Snapshot(bool admin)
        {
            if(!Available)return new JObject{["available"]=false,["admin"]=admin,["revision"]=Revision,["generation"]=generation,["state"]="Unit Balance adapter unavailable for this installed binary"}.ToString(Formatting.None);
            if(admin&&adminJson.Length>0)return adminJson;if(!admin&&publicJson.Length>0)return publicJson;
            EnsureSchema();
            var obj=new JObject{["available"]=true,["enabled"]=Enabled,["admin"]=admin,["revision"]=Revision,["generation"]=generation,
                ["serial"]=++snapshotSequence,["pending"]=publishedRevision!=diskRevision,["state"]=lastState,["config"]=PublicConfig(published),["defaults"]=PublicConfig(defaults),["fields"]=schema.DeepClone()};
            if(admin)obj["saved"]=PublicConfig(disk);
            string text=obj.ToString(Formatting.None);if(Encoding.UTF8.GetByteCount(text)>HudProtocol.MaxBalanceBytes)throw new InvalidOperationException("Balance snapshot too large.");
            if(admin)adminJson=text;else publicJson=text;return text;
        }
        private static void SetPath(JObject root,string[] parts,JToken value,bool remove)
        {
            JObject parent=root;
            for(int i=0;i<parts.Length-1;i++)
            {if(!(parent[parts[i]] is JObject child)){child=new JObject();parent[parts[i]]=child;}parent=child;}
            if(remove)parent.Remove(parts.Last());else parent[parts.Last()]=value.DeepClone();
        }
        private static JToken Validate(JObject field,JToken value)
        {
            string type=(string)field["type"],key=(string)field["name"];if(!(bool)field["editable"])throw new InvalidOperationException("Field is read-only.");
            if(type=="bool"){if(value?.Type!=JTokenType.Boolean)throw new InvalidOperationException("A boolean value is required.");return value;}
            if(value==null||(value.Type!=JTokenType.Float&&value.Type!=JTokenType.Integer))throw new InvalidOperationException("A numeric value is required.");
            double n=value.Value<double>();if(double.IsNaN(n)||double.IsInfinity(n)||Math.Abs(n)>100000000||n < -1)throw new InvalidOperationException("Value must be finite and between -1 and 100000000.");
            if(type=="int"&&Math.Truncate(n)!=n)throw new InvalidOperationException("An integer is required.");
            if(key.EndsWith("_mult",StringComparison.Ordinal)&&(n<=0||n>1000))throw new InvalidOperationException("Multipliers must be greater than zero and at most 1000.");
            if(key=="min_tier"&&(n>8||Math.Truncate(n)!=n)||key=="aim_compensation"&&(n>5||Math.Truncate(n)!=n))throw new InvalidOperationException("Value outside the supported selection range.");
            if(key=="two_way"||key=="three_way"){if(n>255||Math.Truncate(n)!=n)throw new InvalidOperationException("Unit cap multiplier must be -1 or 0..255.");}
            return type=="int"?new JValue((long)n):new JValue(n);
        }
        private static string Save(JObject next,Player actor,string expected)
        {
            byte[] old=File.ReadAllBytes(path);if(HudProtocol.Digest(old)!=expected)throw new InvalidOperationException("JSON changed; refresh before saving.");
            string directory=Path.Combine(Path.GetDirectoryName(path),"SquadHUD_Backups");Directory.CreateDirectory(directory);
            string stamp=DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ",CultureInfo.InvariantCulture)+"-"+Guid.NewGuid().ToString("N");
            string backup=Path.Combine(directory,"Si_UnitBalance_Config-"+stamp+".json"),temp=path+".squadhud-"+stamp+".tmp";
            byte[] bytes=new UTF8Encoding(false).GetBytes(next.ToString(Formatting.Indented));
            if(bytes.Length>HudProtocol.MaxBalanceBytes)throw new InvalidOperationException("New JSON exceeds size limit.");
            try
            {
                File.WriteAllBytes(temp,bytes);ReadJson(temp);
                if(HudProtocol.Digest(File.ReadAllBytes(path))!=expected)throw new InvalidOperationException("JSON changed during save; retry after refreshing.");
                File.Replace(temp,path,backup);
                if(HudProtocol.Digest(File.ReadAllBytes(backup))!=expected||HudProtocol.Digest(File.ReadAllBytes(path))!=HudProtocol.Digest(bytes))throw new IOException("JSON/backup verification failed; inspect backups before proceeding.");
                MelonLogger.Msg("SquadHUD Unit Balance JSON saved by "+actor.PlayerID.SteamID.m_SteamID+"; backup: "+backup);
                RefreshDisk(true);return "JSON saved with verified backup. Reload to apply it.";
            }
            finally{if(File.Exists(temp))File.Delete(temp);}
        }
        internal static string Handle(Player actor,BalanceCommand request,BalanceDraft draft,out bool success)
        {
            success=false;
            try
            {
                if(request.Operation=="refresh"){success=true;return "Balance snapshot refreshed.";}
                if(!MenuCatalog.IsAdmin(actor))throw new InvalidOperationException("Administrator Generic power required.");
                if(!Available)throw new InvalidOperationException("Unit Balance adapter unavailable.");
                RefreshDisk(true);
                if(request.Revision!=Revision)throw new InvalidOperationException("JSON or reload state changed. Refresh and review your edits again.");
                if(request.Operation=="reload")
                {
                    draft.Clear();lastReloadConfirmed=false;reload.Invoke(null,new object[]{actor,"rebalance"});
                    success=lastReloadConfirmed&&publishedRevision==diskRevision;return success?lastState:"Reload did not confirm the new configuration; inspect server log.";
                }
                if(request.Operation=="stage")
                {
                    EnsureSchema();var change=JObject.Parse(request.Payload);if(change["field"]?.Type!=JTokenType.Integer)throw new InvalidOperationException("A field number is required.");int id=change.Value<int>("field");
                    if(id<0||id>=schema.Count)throw new InvalidOperationException("Unknown balance field.");
                    var field=(JObject)schema[id];if(!(bool)field["editable"])throw new InvalidOperationException("Read-only balance field.");
                    bool remove=change.Value<bool?>("remove")??false;var value=remove?JValue.CreateNull():Validate(field,change["value"]);
                    if(draft.Id!=request.Draft||draft.Revision!=Revision){draft.Clear();draft.Id=request.Draft;draft.Revision=Revision;}
                    if(draft.Values.Count>=256&&!draft.Values.ContainsKey(id))throw new InvalidOperationException("Save at most 256 edited fields at a time.");
                    draft.Values[id]=value;if(remove)draft.Removed.Add(id);else draft.Removed.Remove(id);
                    success=true;return "Edit staged; JSON has not changed.";
                }
                if(request.Operation=="save")
                {
                    if(draft.Id!=request.Draft||draft.Revision!=Revision||draft.Values.Count==0)throw new InvalidOperationException("No matching complete edit batch.");
                    // The client declares its edit count so a partial staging transfer cannot be committed.
                    if(!int.TryParse(request.Payload,out int count)||count!=draft.Values.Count)throw new InvalidOperationException("Edit batch incomplete; no JSON was changed.");
                    EnsureSchema();var next=(JObject)disk.DeepClone();
                    foreach(var change in draft.Values)
                    {
                        var field=(JObject)schema[change.Key];if(!draft.Removed.Contains(change.Key))Validate(field,change.Value);
                        SetPath(next,field["path"].Values<string>().ToArray(),change.Value,draft.Removed.Contains(change.Key));
                    }
                    string message=Save(next,actor,diskRevision);draft.Clear();success=true;return message;
                }
                throw new InvalidOperationException("Unsupported balance operation.");
            }
            catch(Exception error){draft.Clear();return HudProtocol.ClipUtf8(error.GetBaseException().Message,300);}
        }
        internal static void SetEnabled(Player actor,bool on)
        {
            if(!MenuCatalog.IsAdmin(actor)||!Available)throw new InvalidOperationException("Unit Balance switch unavailable.");
            RefreshDisk(true);var next=(JObject)disk.DeepClone();next["enabled"]=on;Save(next,actor,diskRevision);
            lastReloadConfirmed=false;reload.Invoke(null,new object[]{actor,"rebalance"});
            if(!lastReloadConfirmed||Enabled!=on)throw new InvalidOperationException("Unit Balance did not confirm the requested state; inspect server log.");
        }
    }
}
