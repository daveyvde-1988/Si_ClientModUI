using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using MelonLoader;

namespace SiSquadHud
{
    internal static class AdminControls
    {
        internal sealed class Item
        {
            internal string Key,Name,State,Description,Parent="";
            internal bool CanSwitch,Enabled;
            internal Action<Player,bool> Set;
        }
        private const BindingFlags Flags=BindingFlags.Static|BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        private static readonly Dictionary<Assembly,bool> verified=new Dictionary<Assembly,bool>();
        private static readonly Dictionary<string,string> Hashes=new Dictionary<string,string>{
            {"Si_KingOfTheHill","5377B71A034E065DF76D5A3F595DE733DAF24D9A875DEF9D0C092BCCAAABF8BC"},
            {"Si_Logistics","9BA61FE9ACF1E9A34C6773BC9DE9C4D0B7E23CE44C1E99E48D34F27741EFE20E"},
            {"Si_CrabCannon","803BFAB9601DF2C9C648AF7C3D2FEE675914FFE0966A7E862D8CA2B63E5BB62F"},
            {"Si_HeavyTeleporter","2E94428CE783DB6FFE618265F3CCA32784830AD6FAED742E7DACF3E67640F6D3"}
        };
        internal static bool Verified(Assembly assembly)
        {
            if(verified.TryGetValue(assembly,out bool ok))return ok;
            if(!Hashes.TryGetValue(assembly.GetName().Name,out var expected))return false;
            using(var hash=SHA256.Create())using(var stream=System.IO.File.OpenRead(assembly.Location))ok=BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","")==expected;
            verified[assembly]=ok;return ok;
        }
        private static object Read(object value,string name)=>MenuCatalog.Read(value,name);
        private static Type Find(Assembly a,string name)=>a.GetType(name,false);
        internal static List<Item> List()
        {
            var result=new List<Item>();
            foreach(var mod in MelonMod.RegisteredMelons.OrderBy(m=>m.Info.Name,StringComparer.OrdinalIgnoreCase))
            {
                var type=mod.GetType();var assembly=type.Assembly;string key=type.FullName;
                var row=new Item{Key=key,Name=System.IO.Path.GetFileNameWithoutExtension(assembly.Location).ToLowerInvariant(),State="Loaded",Description="Loaded server mod. No reviewed live master switch; changes require the mod's own configuration and a planned restart."};
                try
                {
                    if(key=="Si_Formation.Formations")
                    {
                        var api=Find(assembly,"Si_Formation.FormationApi");
                        row.Enabled=(bool)Read(api,"GlobalRequestedEnabled");row.CanSwitch=api.GetMethod("RequestGlobalEnabled")!=null;
                        row.Set=(p,on)=>{var answer=api.GetMethod("RequestGlobalEnabled").Invoke(null,new object[]{on});if(answer.ToString()!="Success")throw new InvalidOperationException("Formation: "+answer);};
                        row.Description="Formation match-wide master switch. Uses its public API with native cleanup; next round restores configured defaults.";
                    }
                    else if(key=="Si_UnitBalance.UnitBalance")
                    {
                        row.Enabled=UnitBalanceBackend.Enabled;row.CanSwitch=UnitBalanceBackend.Available;
                        row.State=row.CanSwitch?(row.Enabled?"ON":"OFF"):"Unavailable";
                        row.Set=(p,on)=>UnitBalanceBackend.SetEnabled(p,on);
                        row.Description="Configure mirrors the native /b menu, including its save/apply confirmations, with verified backups. Switch uses the native enabled flag/reload; independent rules may remain active while OFF.";
                    }
                    else if(key=="Si_StarterUnits.StarterUnits")
                    {
                        row.Description="Configure faction starter armies and 4-way cross-spawns. Saves use verified JSON backups and take effect next round; this does not respawn live units.";
                    }
                    else
                    {
                        string method=null,command=null,state=null;
                        switch(key)
                        {
                            case "Si_KingOfTheHill.KingOfTheHill":method="OnKohCommand";command="/koh";state="Cfg";break;
                            case "Si_Logistics.LogisticsMod":method="OnLogisticsCommand";command="/logistics";state="Cfg";break;
                            case "Si_CrabCannon.CrabCannon":method="OnCcCommand";command="/cc";state="_enabled";break;
                            case "Si_HeavyTeleporter.HeavyTeleporter":method="OnStCommand";command="/st";state="_enabled";break;
                        }
                        if(method!=null)
                        {
                            object value=Read(type,state);bool? enabled=value as bool? ?? Read(value,"Enabled") as bool?;
                            if(enabled.HasValue){row.Enabled=enabled.Value;row.State=enabled.Value?"ON":"OFF";}
                            row.CanSwitch=enabled.HasValue&&Verified(assembly)&&type.GetMethod(method,Flags)!=null;
                            string handler=method,prefix=command;
                            row.Set=(p,on)=>type.GetMethod(handler,Flags).Invoke(null,new object[]{p,prefix+(on?" on":" off")});
                            row.Description="Uses "+command+" on/off and the mod's existing cleanup/save behavior. Persists exactly as that command does.";
                            if(!row.CanSwitch)row.Description="Configured state shown; live adapter needs review for this installed binary.";
                        }
                    }
                    if(row.CanSwitch)row.State=row.Enabled?"ON":"OFF";
                }
                catch{row.CanSwitch=false;row.State="Unavailable";row.Description="Master state unavailable; no change will be made.";}
                result.Add(row);
                if(key=="Si_Logistics.LogisticsMod"&&Verified(assembly))
                {
                    foreach(var pair in new[]{new[]{"harvesterfix","HarvesterFix"},new[]{"aitransport","AITransport"}})
                    {
                        string command=pair[0],property=pair[1];
                        bool? value=Read(Read(Read(type,"Cfg"),property),"Enabled") as bool?;
                        result.Add(new Item{Key=key+":"+command,Parent=row.Name,Name=command,Enabled=value??false,
                            State=value.HasValue?(value.Value?"ON":"OFF"):"Unavailable",CanSwitch=value.HasValue&&row.CanSwitch,
                            Description="Uses /logistics "+command+" on/off with native cleanup and save. Master Logistics must also be enabled.",
                            Set=(p,on)=>type.GetMethod("OnLogisticsCommand",Flags).Invoke(null,new object[]{p,"/logistics "+command+(on?" on":" off")})});
                    }
                }
                if(key=="Si_CrabCannon.CrabCannon"&&Verified(assembly))
                {
                    foreach(var pair in new[]{new[]{"crab","CrabEnabled"},new[]{"goliath","GoliathEnabled"},new[]{"behemoth","BehemothEnabled"},new[]{"scorpion","ScorpionEnabled"},new[]{"hunter","HunterEnabled"},new[]{"super","SuperEnabled"},new[]{"playeraim","PlayerAimAllowed"},new[]{"commanderaim","CommanderAimAllowed"}})
                    {
                        string command=pair[0],property=pair[1];bool? value=Read(type,property) as bool?;
                        result.Add(new Item{Key=key+":"+command,Parent=row.Name,Name=command,Enabled=value??false,
                            State=value.HasValue?(value.Value?"ON":"OFF"):"Unavailable",CanSwitch=value.HasValue&&row.CanSwitch,
                            Description="Uses /cc "+command+" with the native toggle/save behavior. The parent master switch still applies.",
                            Set=(p,on)=>{if((Read(type,property) as bool?)!=on)type.GetMethod("OnCcCommand",Flags).Invoke(null,new object[]{p,"/cc "+command});}});
                    }
                }
            }
            return result;
        }
    }
}
