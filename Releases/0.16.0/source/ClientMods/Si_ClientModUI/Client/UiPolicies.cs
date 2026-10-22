using System.Text.RegularExpressions;

namespace SiSquadHud;
// Pure policies shared with offline checks; input/render callbacks themselves are never throttled.
internal static class UiPolicies
{
    internal const double HousekeepingSeconds=1,UiRefreshSeconds=.05;
    internal static string WindowKey(string window,string team,bool commander)=>window+"-"+team+"-"+(commander?"commander":"player");
    internal static int SidebarColumns(float width,float height,int count)
    {
        int max=Math.Max(1,(int)((width-10)/52));
        return Math.Max(1,Math.Min(Math.Max(1,count),width>=height*1.5f?max:Math.Min(max,Math.Max(1,(int)Math.Ceiling(Math.Sqrt(Math.Max(1,count)*Math.Max(1,width)/Math.Max(1,height)))))));
    }
    internal static bool NeedsInput(string command,bool declared)=>declared||Regex.IsMatch(command??"",@"<[^>]+>|\{[^}]+\}|\[[^\]]+\]");
    internal static string InputBase(string command)=>Regex.Replace(command??"",@"\s*[<\[{].*$","").TrimEnd();
    internal static bool HasArguments(string text,string prefix)=>text!=null&&text.StartsWith(prefix,StringComparison.Ordinal)&&text.Length>prefix.Length&&char.IsWhiteSpace(text[prefix.Length])&&!string.IsNullOrWhiteSpace(text.Substring(prefix.Length));
    internal static float Split(float fraction)=>float.IsFinite(fraction)?Math.Clamp(fraction,.18f,.65f):.30f;
    internal static bool HoldWanted(bool held,bool enabled,bool fps,bool presented,bool focused,bool nativeMenu,bool nativeText,bool panel,bool picker,bool needsRelease)
        =>held&&enabled&&fps&&presented&&focused&&!nativeMenu&&!nativeText&&!panel&&!picker&&!needsRelease;
}
