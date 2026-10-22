using System.Globalization;
using System.Text.RegularExpressions;

namespace SilicaCheats;
// Parse confirmed native replies, never submitted command text or guessed toggle outcomes.
public static class CheatStatusRules
{
    public static readonly (string Key,string Label,bool ActiveValue)[] Toggles={
        ("Build","Unlimited construction",true),("Damage","Damage disabled",false),
        ("AI","AI disabled",false),("Fog","Fog of war disabled",false),
        ("Units","Unit debugging",true),("DamageDebug","Damage debugging",true),
        ("Lights","Object lights hidden",false),("NetList","Network debug list",true),
        ("AllowVisionModes","Vision modes disabled",false)
    };
    public static readonly string[] Numbers={"TimeScale","DayScale","SunScale","MoonScale","NetTickMult"};
    public static void Observe(string text,IDictionary<string,bool> toggles,IDictionary<string,float> numbers)
    {
        if(string.IsNullOrWhiteSpace(text))return;
        var team=Regex.Match(text,@"^(?:Commander for team '([^']+)' is now|UnlinkedStructures: Setting unlinked structure state for Team '([^']+)' to) (Enabled|Disabled)$",RegexOptions.IgnoreCase);
        if(team.Success)
        {
            bool commander=team.Groups[1].Success;
            toggles[(commander?"AICommander:":"UnlinkedStructures:")+team.Groups[commander?1:2].Value]=team.Groups[3].Value.Equals("Enabled",StringComparison.OrdinalIgnoreCase);
            return;
        }
        int colon=text.IndexOf(':');if(colon<=0)return;
        string key=Catalog.Find(text[..colon])?.Key,body=text[(colon+1)..].Trim();
        if(key==null||!text[..colon].Trim().Equals(key,StringComparison.OrdinalIgnoreCase))return;
        string prefix=key switch{
            "AI"=>@"AI(?: toggled to)?", "Damage"=>@"Damage toggled to",
            "Build"=>@"Unlimited construction(?: toggled to)?",
            "AllowVisionModes"=>@"Allow vision modes (?:set|reset) to",_=>null};
        if(prefix!=null)
        {
            var state=Regex.Match(body,"^"+prefix+@" (Enabled|Disabled|True|False)$",RegexOptions.IgnoreCase);
            if(state.Success)toggles[key+":"]=state.Groups[1].Value.Equals("Enabled",StringComparison.OrdinalIgnoreCase)||state.Groups[1].Value.Equals("True",StringComparison.OrdinalIgnoreCase);
        }
        string numberPrefix=key switch{
            "TimeScale"=>"Time scale", "DayScale"=>"Day scale", "SunScale"=>"Sun brightness scale",
            "MoonScale"=>"Moon brightness scale", "NetTickMult"=>"Network tick rate multiplier",_=>null};
        if(numberPrefix==null)return;
        var number=Regex.Match(body,"^"+numberPrefix+@" (?:set|reset) to ([-+0-9.,eE]+)$",RegexOptions.IgnoreCase);
        if(number.Success&&float.TryParse(number.Groups[1].Value.Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out float n)&&float.IsFinite(n))numbers[key]=n;
    }
}
