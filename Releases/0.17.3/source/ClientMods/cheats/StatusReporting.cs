using System.Globalization;
using Il2Cpp;
using SiSquadHud;

namespace SilicaCheats;
public sealed partial class CheatsMod
{
    private ClientModControls.Status[] CreateStatuses()
    {
        var statuses=new List<ClientModControls.Status>{
            new(){Id="access",Label="Cheats",Read=()=>Enabled?"enabled (game reports)":"disabled (game reports)"}
        };
        void Add(string id,string label,string group,Func<string> read)=>statuses.Add(new(){Id=id,Label=label,Group=group,AdminOnly=!Enabled,Read=()=>CanUse?read():"Unavailable: cheats disabled"});
        Add("active","Activated cheats","Summary",ActiveCheats);
        Add("altered-values","Values away from native reset (1)","Summary",AlteredValues);
        foreach(var toggle in CheatStatusRules.Toggles)
        {
            string key=toggle.Key;
            Add("toggle:"+key,key=="Build"?"Unlimited construction":key,"Cheat states",()=>ToggleStatus(key));
        }
        Add("health","Health display mode","Cheat states",()=>DamageManager.ShowDebugHealth+" (live local)");
        foreach(string key in CheatStatusRules.Numbers)
            Add("value:"+key,key,"Values",()=>NumberStatus(key));
        Add("teams:AICommander","AI commanders by team","Team states",()=>TeamStatuses("AICommander"));
        Add("teams:UnlinkedStructures","Unlinked structure state / decay by team","Team states",()=>TeamStatuses("UnlinkedStructures"));
        Add("teams:resources","Stored team resources","Team states",()=>string.Join("; ",AvailableTeams().Select(t=>t.TeamShortName+": "+t.StoredResources+" (game reports)")));
        return statuses.ToArray();
    }
    private static bool LiveToggle(string key)=>Game.GetIsSingleplayer()||new[]{"Fog","Units","DamageDebug","Lights","NetList"}.Contains(key);
    private static bool LiveNumber(string key)=>Game.GetIsSingleplayer();
    private string ToggleStatus(string key,string selectedTeam="")
    {
        bool? state=ToggleState(key,selectedTeam);
        return state.HasValue?(state.Value?"enabled":"disabled")+(LiveToggle(key)?" (live game value)":" (last native feedback)"):"Unknown: server has not reported a value";
    }
    private string NumberStatus(string key)
    {
        float? value=NumberValue(key);
        return value.HasValue?value.Value.ToString("G9",CultureInfo.InvariantCulture)+(LiveNumber(key)?" (live game value)":" (last native feedback)"):"Unknown: server has not reported a value";
    }
    private string TeamStatuses(string key)
    {
        var rows=AvailableTeams().Select(t=>t.TeamShortName+": "+ToggleStatus(key,t.TeamShortName)).ToArray();
        return rows.Length>0?string.Join("; ",rows):"No teams available";
    }
    private string ActiveCheats()
    {
        var active=CheatStatusRules.Toggles.Where(t=>ToggleState(t.Key)==t.ActiveValue)
            .Select(t=>t.Label+(LiveToggle(t.Key)?" (live)":" (last native feedback)")).ToArray();
        int unknown=CheatStatusRules.Toggles.Count(t=>!ToggleState(t.Key).HasValue);
        return (active.Length>0?string.Join("; ",active):"None confirmed")+(unknown>0?"; "+unknown+" server states unknown":"");
    }
    private string AlteredValues()
    {
        var altered=CheatStatusRules.Numbers.Where(key=>NumberValue(key).HasValue&&NumberValue(key).Value!=1)
            .Select(key=>key+" = "+NumberStatus(key)).ToArray();
        int unknown=CheatStatusRules.Numbers.Count(key=>!NumberValue(key).HasValue);
        return (altered.Length>0?string.Join("; ",altered):"None confirmed away from reset value 1")+(unknown>0?"; "+unknown+" server values unknown":"");
    }
}
