using System.Globalization;
using SilicaCheats;
using SiSquadHud;
int checks=0;
void Check(bool valid,string name){checks++;if(!valid)throw new Exception(name);}
void Reject(Action action,string name){try{action();}catch(ArgumentException){checks++;return;}throw new Exception("Accepted invalid input: "+name);}
foreach(string culture in new[]{"en-US","nl-BE","fr-BE"})
{
    CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
    Check(CommandRules.Resources("centauri","5000",false)=="resources centauri 5000","add");
    Check(CommandRules.Resources("centauri","5000",true)=="resources centauri -5000","subtract");
    Check(CommandRules.Resources("alien","2147483647",true)=="resources alien -2147483647","integer boundary");
    Check(CommandRules.Spawn("Soldier","centauri","1.25,2,3,1,0,0,0",false)=="spawn Soldier centauri 1.25,2,3,1,0,0,0","camera quaternion order");
    Check(CommandRules.Spawn("Soldier","none","1, 2,3,1,0,0,0",true).EndsWith("1,2,3,1,0,0,0 true"),"precise rotation");
    Check(CommandRules.OffsetPose("10,20,30,1,0,0,0")=="10,20,130,1,0,0,0","forward along Z");
    var yaw=CommandRules.OffsetPose("10,20,30,0.70710677,0,0.70710677,0").Split(',').Select(s=>float.Parse(s,CultureInfo.InvariantCulture)).ToArray();
    Check(Math.Abs(yaw[0]-110)<.001&&Math.Abs(yaw[1]-20)<.001&&Math.Abs(yaw[2]-30)<.001,"90 degree yaw forward along X");
    var pitch=CommandRules.OffsetPose("0,0,0,0.70710677,-0.70710677,0,0").Split(',').Select(s=>float.Parse(s,CultureInfo.InvariantCulture)).ToArray();
    Check(Math.Abs(pitch[1]-100)<.001&&Math.Abs(pitch[2])<.001,"pitch retained for forward offset");
    string template=CommandRules.DynamicSpawn("Soldier","centauri",true);int captures=0;
    string Snapshot(){captures++;return captures==1?"10,20,30,1,0,0,0":"50,60,70,1,0,0,0";}
    Check(template=="spawn Soldier centauri {view+100} true","dynamic shortcut retains instruction");
    Check(CommandRules.ResolveDynamic(template,Snapshot)=="spawn Soldier centauri 10,20,130,1,0,0,0 true","first execution snapshot");
    Check(CommandRules.ResolveDynamic(template,Snapshot)=="spawn Soldier centauri 50,60,170,1,0,0,0 true"&&captures==2,"second execution recaptures repositioned view");
    string fixedLine="spawn Soldier none 10,20,30,1,0,0,0";
    Check(CommandRules.ResolveDynamic(fixedLine,()=>throw new Exception("Should not capture"))==fixedLine,"fixed shortcuts keep fixed coordinates");
    foreach(float value in new[]{0f,1.25f,-123.456f,0.70710677f,float.Epsilon,float.MaxValue,float.MinValue,1.234E-20f,12345678f})
    {
        string wire=CommandRules.NativeNumber(value);
        Check(!wire.Contains('.')&&!wire.Contains(','),"native numeric token has no locale-sensitive separator");
        Check(float.Parse(wire)==value,"native default-culture float.Parse round trip");
    }
    string normalized="1.25,-2.75,100.625,0.70710677,0,0.70710677,0";
    var nativeParts=CommandRules.ForNativeConsole("spawn Soldier centauri "+normalized+" true").Split(' ');
    Check(nativeParts.Length==5&&nativeParts[4]=="true","spawn arguments and precise flag preserved");
    var expectedPose=normalized.Split(',').Select(s=>float.Parse(s,CultureInfo.InvariantCulture)).ToArray();
    var nativePose=nativeParts[3].Split(',').Select(float.Parse).ToArray();
    Check(expectedPose.SequenceEqual(nativePose),"all seven spawn values survive native culture-sensitive parsing");
    Check(float.Parse(CommandRules.ForNativeConsole("nettickmult 1.1").Split(' ')[1])==1.1f,"fractional scalar reaches native parser correctly");
    foreach(string invalid in new[]{"resources centauri {view+100}","spawn Soldier none {view+100} extra","spawn Soldier none {view+50}","spawn {view+100} none {view+100}","spawn Soldier none {view+100} true extra"})
        Reject(()=>CommandRules.ResolveDynamic(invalid,Snapshot),"malformed dynamic template");
    foreach(string invalid in new[]{"0","-1","2147483648","1.5","NaN","5000\ncheats"})Reject(()=>CommandRules.Resources("centauri",invalid,false),"amount "+invalid);
    foreach(string invalid in new[]{"1,2,3","1,2,3,0,0,0,0","NaN,2,3,1,0,0,0","Infinity,2,3,1,0,0,0","1e99,2,3,1,0,0,0"})Reject(()=>CommandRules.Pose(invalid),"pose");
}
Reject(()=>CommandRules.Spawn("Soldier other","centauri","1,2,3,1,0,0,0",false),"prefab whitespace");
Reject(()=>CommandRules.Resources("centauri\ncheats","5",false),"team injection");
Check(!CommandRules.SingleLine("resources centauri 5\ncheats"),"newline rejected");
Check(!CommandRules.SingleLine(new string('x',1025)),"length capped");
Check(Catalog.All.Length==31,"catalog size including camera");
Check(Catalog.All.Select(c=>c.Key.ToLowerInvariant()).Distinct().Count()==31,"unique keys");
Check(Catalog.Find("ReSoUrCeS centauri 5000")?.Key=="Resources","case insensitive exact lookup");
Check(Catalog.Find("res centauri 5000")==null,"no partial execution");
Check(Catalog.Find("Run script")==null,"non-cheat script command excluded");
foreach(bool enabled in new[]{false,true})foreach(bool require in new[]{false,true})
    Check(CommandRules.AccessError(false,enabled,require)=="user is not an admin. access denied","non-admin denied regardless of cheats state");
Check(CommandRules.AccessError(true,false,true).Length>0,"disabled execution denied");
Check(CommandRules.AccessError(true,false,false)=="","admin can enable disabled cheats");
Check(CommandRules.AccessError(true,true,true)=="","admin with enabled cheats permitted");
foreach(bool session in new[]{false,true})foreach(bool loading in new[]{false,true})foreach(bool cinematic in new[]{false,true})foreach(bool shell in new[]{false,true})
    Check(ShellRules.ShowOverlay(session,loading,cinematic,shell)==(session&&!loading&&!cinematic&&!shell),"overlay presentation policy");
Check(!ShellRules.ShowAdmin(true,true,true,true),"singleplayer never has separate admin section");
Check(!ShellRules.ShowAdmin(false,true,false,false),"empty admin section hidden");
Check(!ShellRules.ShowAdmin(false,false,true,true),"non-admin section hidden");
Check(ShellRules.ShowAdmin(false,true,true,false)&&ShellRules.ShowAdmin(false,true,false,true),"real admin commands or controls reveal section");
Check(ShellRules.TeamScope("Centauri")=="team:centauri"&&ShellRules.TeamScope(null)=="spectator","stable team scope");
var scopes=new Dictionary<string,string>{{"legacy",""},{"sol","team:sol"}};
Check(ShellRules.AssignLegacy(scopes.Keys.ToArray(),"spectator",k=>scopes[k],(k,v)=>scopes[k]=v)==0,"spectator does not claim legacy buttons");
Check(ShellRules.AssignLegacy(scopes.Keys.ToArray(),"team:centauri",k=>scopes[k],(k,v)=>scopes[k]=v)==1,"legacy buttons migrate once");
Check(ShellRules.AssignLegacy(scopes.Keys.ToArray(),"team:sol",k=>scopes[k],(k,v)=>scopes[k]=v)==0,"switching teams does not reassign existing buttons");
var restored=System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,string>>(System.Text.Json.JsonSerializer.Serialize(scopes));
Check(ShellRules.MatchesTeam(restored["legacy"],"team:centauri")&&!ShellRules.MatchesTeam(restored["legacy"],"team:sol"),"saved shortcut remains exclusive to its team");
Check(!ShellRules.MatchesTeam("", "team:sol"),"unassigned legacy buttons never leak across teams");
Console.WriteLine($"Passed {checks} command checks across three cultures.");
