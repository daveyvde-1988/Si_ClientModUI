namespace SiSquadHud;
public static class ShellRules
{
    public static bool ShowOverlay(bool session,bool loading,bool cinematic,bool shellScene)=>session&&!loading&&!cinematic&&!shellScene;
    public static bool ShowAdmin(bool singleplayer,bool admin,bool commands,bool controls)=>!singleplayer&&admin&&(commands||controls);
    public static string TeamScope(string shortName)=>string.IsNullOrWhiteSpace(shortName)?"spectator":"team:"+shortName.Trim().ToLowerInvariant();
    public static bool MatchesTeam(string bindingTeam,string currentTeam)=>!string.IsNullOrEmpty(bindingTeam)&&string.Equals(bindingTeam,currentTeam,StringComparison.OrdinalIgnoreCase);
    public static int AssignLegacy<T>(IEnumerable<T> bindings,string team,Func<T,string> read,Action<T,string> assign)
    {
        if(string.IsNullOrEmpty(team)||team=="spectator")return 0;
        int count=0;foreach(var binding in bindings)if(binding!=null&&string.IsNullOrEmpty(read(binding))){assign(binding,team);count++;}
        return count;
    }
}
