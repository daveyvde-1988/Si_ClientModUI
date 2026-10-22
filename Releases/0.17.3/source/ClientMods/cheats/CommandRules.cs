using System.Globalization;

namespace SilicaCheats;
public static class CommandRules
{
    // The native Spawn handler uses culture-sensitive float.Parse, although Camera
    // exports dot decimals separated by commas. Integer mantissas with exponents
    // avoid decimal/group separator ambiguity on both client and server.
    public static string NativeNumber(float value)
    {
        if(!float.IsFinite(value))throw new ArgumentException("Value must be finite.");
        string text=value.ToString("R",CultureInfo.InvariantCulture);
        int e=text.IndexOf('E');string mantissa=e<0?text:text[..e];
        int exponent=e<0?0:int.Parse(text[(e+1)..],CultureInfo.InvariantCulture);
        int dot=mantissa.IndexOf('.');
        if(dot>=0){exponent-=mantissa.Length-dot-1;mantissa=mantissa.Remove(dot,1);}
        return mantissa+(exponent==0?"":"E"+exponent.ToString(CultureInfo.InvariantCulture));
    }
    public static string ForNativeConsole(string line)
    {
        var tokens=line.Split(' ',StringSplitOptions.RemoveEmptyEntries);
        if(tokens.Length>=4&&tokens[0].Equals("spawn",StringComparison.OrdinalIgnoreCase))
            tokens[3]=string.Join(",",Pose(tokens[3]).Split(',').Select(v=>NativeNumber(float.Parse(v,CultureInfo.InvariantCulture))));
        else if(tokens.Length==2&&new[]{"sunscale","timescale","dayscale","moonscale","nettickmult"}.Contains(tokens[0].ToLowerInvariant())&&float.TryParse(tokens[1],NumberStyles.Float,CultureInfo.InvariantCulture,out float n))
            tokens[1]=NativeNumber(n);
        return string.Join(" ",tokens);
    }
    public const float DynamicDistance=50;
    public const string DynamicView="{view+50}";
    private const string LegacyDynamicView="{view+100}";
    public static string UpgradeDynamic(string line)=>line?.Replace(LegacyDynamicView,DynamicView,StringComparison.Ordinal);
    public static bool Dynamic(string line)=>UpgradeDynamic(line)?.Contains(DynamicView,StringComparison.Ordinal)==true;
    public static bool ValidTemplate(string line)
    {
        if(!SingleLine(line))return false;
        if(!line.Contains('{')&&!line.Contains('}'))return true;
        var tokens=UpgradeDynamic(line).Split(' ',StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length is 4 or 5&&tokens[0].Equals("spawn",StringComparison.OrdinalIgnoreCase)&&
            Token(tokens[1])&&Token(tokens[2])&&tokens[3]==DynamicView&&
            (tokens.Length==4||tokens[4].Equals("true",StringComparison.OrdinalIgnoreCase)||tokens[4].Equals("false",StringComparison.OrdinalIgnoreCase))&&
            line.Count(c=>c=='{')==1&&line.Count(c=>c=='}')==1;
    }
    public static string DynamicSpawn(string prefab,string team,bool precise)
    {
        if(!Token(prefab)||!Token(team))throw new ArgumentException("Select a prefab and team.");
        return "spawn "+prefab+" "+team+" "+DynamicView+(precise?" true":"");
    }
    public static string OffsetPose(string pose,float metres=DynamicDistance)
    {
        if(!float.IsFinite(metres))throw new ArgumentException("Distance must be finite.");
        var v=Pose(pose).Split(',').Select(s=>float.Parse(s,CultureInfo.InvariantCulture)).ToArray();
        double norm=Math.Sqrt(v.Skip(3).Sum(f=>(double)f*f));
        double w=v[3]/norm,x=v[4]/norm,y=v[5]/norm,z=v[6]/norm;
        v[0]+=(float)(metres*2*(x*z+w*y));
        v[1]+=(float)(metres*2*(y*z-w*x));
        v[2]+=(float)(metres*(1-2*(x*x+y*y)));
        return Pose(string.Join(",",v.Select(f=>f.ToString("R",CultureInfo.InvariantCulture))));
    }
    public static string ResolveDynamic(string template,Func<string> snapshot)
    {
        if(!ValidTemplate(template))throw new ArgumentException("Invalid dynamic spawn command.");
        return Dynamic(template)?UpgradeDynamic(template).Replace(DynamicView,OffsetPose(snapshot())):template;
    }
    public static string AccessError(bool admin,bool enabled,bool requireCheats)=>!admin?"user is not an admin. access denied":requireCheats&&!enabled?"Cheats are disabled. Enable cheats first.":"";
    public static bool SingleLine(string value)=>!string.IsNullOrWhiteSpace(value)&&value.Length<=1024&&!value.Any(char.IsControl);
    public static string Resources(string team,string amount,bool subtract)
    {
        if(!Token(team)||!int.TryParse(amount,NumberStyles.None,CultureInfo.InvariantCulture,out int n)||n<=0)
            throw new ArgumentException("Enter a positive whole amount up to 2147483647.");
        return "resources "+team+" "+(subtract?-n:n).ToString(CultureInfo.InvariantCulture);
    }
    public static bool Token(string value)=>!string.IsNullOrWhiteSpace(value)&&!value.Any(c=>char.IsWhiteSpace(c)||char.IsControl(c));
    public static string Pose(string text)
    {
        var parts=(text??"").Trim().Split(',');
        if(parts.Length!=7)throw new ArgumentException("Paste Camera's seven comma-separated values: x,y,z,w,qx,qy,qz.");
        var values=parts.Select(p=>float.TryParse(p.Trim(),NumberStyles.Float,CultureInfo.InvariantCulture,out var f)&&float.IsFinite(f)?f:throw new ArgumentException("Coordinates must be finite numbers using decimal points.")).ToArray();
        if(values.Skip(3).Sum(v=>(double)v*v)<0.000001)throw new ArgumentException("Rotation quaternion cannot be zero.");
        return string.Join(",",values.Select(v=>v.ToString("R",CultureInfo.InvariantCulture)));
    }
    public static string Spawn(string prefab,string team,string pose,bool precise)
    {
        if(!Token(prefab)||!Token(team))throw new ArgumentException("Select a valid prefab and team.");
        return "spawn "+prefab+" "+team+" "+Pose(pose)+(precise?" true":"");
    }
}
