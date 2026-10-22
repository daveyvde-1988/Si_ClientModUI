using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;

namespace SiSquadHud
{
    // Explicit read adapters: add a provider here without changing rendering or packet handling.
    internal static class StatusProviders
    {
        private sealed class Provider
        {
            internal string Id,Mod,Label;
            internal Func<Player,string> Read;
            internal bool AdminOnly;
        }
        private static readonly List<Provider> extra=new List<Provider>();
        internal static void Initialize()
        {
            extra.Clear();
            RegisterFields("Si_Logistics.LogisticsMod","si_logistics",new[]{"Cfg.AirCargoCapacity","Cfg.GroundCargoCapacity","Cfg.LoadUnitsPerSecond","Cfg.UnloadUnitsPerSecond","Cfg.AirLoadUnitsPerSecond","Cfg.AirUnloadUnitsPerSecond","Cfg.LoadRadius","Cfg.UnloadRadius","Cfg.DeliveryCooldownSeconds","Cfg.PersonalBonus.Share","Cfg.PersonalBonus.TransferShare","Cfg.CommanderScore.Enabled"});
            RegisterFields("Si_CrabCannon.CrabCannon","si_crabcannon",new[]{"LaunchSpeed","LaunchAngle","CooldownSeconds","TriggerRadius","MinTier","SuperMaxCharges","SuperRechargeTime","_superCharges"},true);
            RegisterFields("Si_HeavyTeleporter.HeavyTeleporter","si_heavyteleporter",new[]{"TechTier","MaxCharges","RechargeTime","Countdown","TeleportRadius","MinEnemyBaseDistance"});
            var heavy=Find("Si_HeavyTeleporter.HeavyTeleporter");
            if(heavy!=null&&AdminControls.Verified(heavy.Assembly))
            {
                foreach(string field in new[]{"Charges","RechargeTimer"})
                {
                    string name=field;
                    extra.Add(new Provider{Id="si_heavyteleporter.team."+name,Mod="si_heavyteleporter",Label="Your team / "+name,Read=p=>
                    {
                        var states=MenuCatalog.Read(heavy,"_teamStates") as IDictionary;
                        object state=p.Team&&states?.Contains(p.Team.Index)==true?states[p.Team.Index]:null;
                        if(!(MenuCatalog.Read(state,"Announced") as bool? ?? false))return "Not active";
                        return Format(MenuCatalog.Read(state,name));
                    }});
                }
            }
        }
        private static Type Find(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name,false)).FirstOrDefault(t=>t!=null);
        private static string Format(object value)=>value is bool flag?(flag?"ON":"OFF"):value is IFormattable number?number.ToString("0.##",CultureInfo.InvariantCulture):"Unavailable";
        private static void RegisterFields(string typeName,string mod,string[] fields,bool adminOnly=false)
        {
            var type=Find(typeName);if(type==null||!AdminControls.Verified(type.Assembly))return;
            foreach(string field in fields)
            {
                string name=field;
                extra.Add(new Provider{Id=mod+"."+name,Mod=mod,AdminOnly=adminOnly,Label=Regex.Replace(name.Replace("Cfg.","").Replace('.', '/').TrimStart('_'),"(?<=[a-z])(?=[A-Z])"," "),Read=p=>
                {
                    object value=type;foreach(string part in name.Split('.'))value=MenuCatalog.Read(value,part);
                    return Format(value);
                }});
            }
        }
        internal static List<StatusParameter> Read(Player player,HudStatus status,List<AdminControls.Item> mods)
        {
            var rows=new List<StatusParameter>();
            Action<string,string,string,string> add=(id,mod,label,value)=>rows.Add(new StatusParameter{Id=id,Mod=mod,Label=label,Value=value??"Unavailable"});
            add("ranks.title","si_ranks","Rank",status.Rank);
            add("ranks.cap","si_ranks","Rank squad cap",status.RankCap<0?"Unavailable":status.RankCap.ToString());
            add("formation.cap","si_formation","Effective squad cap",status.EffectiveCap<0?"Unavailable":status.EffectiveCap.ToString());
            add("formation.source","si_formation","Squad policy",status.Source);
            add("antigrief.recruitment","si_antigrief","Recruitment",status.Recruitment);
            add("antigrief.exclusions","si_antigrief","AI exclusions",status.Exclusions.Length==0?"None":status.Exclusions);
            add("koh.credits","si_kingofthehill","Personal credits",status.Credits?.ToString("N0")??"Unavailable");
            foreach(var mod in mods)
                add("module."+mod.Key,mod.Parent.Length>0?mod.Parent:mod.Name,mod.Parent.Length>0?mod.Name:"State",mod.State);
            bool admin=MenuCatalog.IsAdmin(player);
            foreach(var provider in extra)
            {
                if(provider.AdminOnly&&!admin)continue;
                string value;
                try{value=provider.Read(player);}catch{value="Unavailable";}
                add(provider.Id,provider.Mod,provider.Label,value);
            }
            return rows;
        }
    }
}
