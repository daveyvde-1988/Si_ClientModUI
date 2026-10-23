using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SiSquadHud;
using SilicaAdminMod;

internal static class ServerChecks
{
    private static int count;
    static void Check(bool condition,string label){count++;if(!condition)throw new Exception(label);}
    static string Root(Frame.Result result)=>Protocol.ShortHash(string.Join("\n",result.Mods.Values.OrderBy(m=>m.Id,StringComparer.Ordinal).Select(m=>m.Id+":"+m.Hash)));
    static void ValidateHash(Frame.Result result)
    {
        foreach(var mod in result.Mods.Values)
        {
            string expected=mod.Hash;mod.Hash="";string computed=Protocol.ShortHash(JsonConvert.SerializeObject(mod,Formatting.None));mod.Hash=expected;
            Check(expected==computed,"Module hash must match the final player-visible document");
        }
        Check(Root(result)==result.Root,"Root hash describes transmitted modules");
    }
    static void Main()
    {
        string temp=Path.Combine(Path.GetTempPath(),"modui-server-export-"+Guid.NewGuid().ToString("N"));
        MelonLoader.Utils.MelonEnvironment.UserDataDirectory=temp;
        string id=Assembly.GetExecutingAssembly().GetName().Name;
        MelonLoader.MelonMod.RegisteredMelons.Add(new(){MelonAssembly=new(){Assembly=Assembly.GetExecutingAssembly()},Info=new(){Name="Demo",Version="1"}});
        PlayerMethods.PlayerCommands.Add(new(){CommandName="demo",PlayerCommandCallback=DemoProvider.Root});
        PlayerMethods.PlayerCommands.Add(new(){CommandName="tuning",PlayerCommandCallback=DemoProvider.Tuning});
        AdminMethods.AdminCommands.Add(new(){AdminCommandText="secret",AdminPower=Power.Commander,AdminCallback=DemoProvider.Secret});
        var user=new Player{Id=11,Admin=false,Power=Power.None};
        try
        {
            Frame.ResetProviders();var publicFrame=Frame.Build(user);var mod=publicFrame.Mods[id];
            Check(mod.Cmds.Any(c=>c.Text=="/demo"),"Registered root retained");
            Check(mod.Cmds.All(c=>!c.Admin&&c.Text!="/tuning"&&!c.Text.Contains("admin")),"Exported admin rows and player-registered tuning hidden");
            string publicJson=JsonConvert.SerializeObject(mod);
            Check(!publicJson.Contains("76561198000000000"),"Public structured data scrubs numeric and string Steam IDs");
            Check(!publicJson.Contains("commands\""),"Raw exported commands removed from data");
            Check(mod.Cmds.Any(c=>c.Text=="/demo pick"&&c.Choices[0][0]=="11"),"Player-scoped choices merged under published root");
            ValidateHash(publicFrame);
            var other=Frame.Build(new Player{Id=22});
            Check(other.Mods[id].Cmds.Single(c=>c.Text=="/demo pick").Choices[0][0]=="22","Per-player values are isolated");
            Check(mod.Cmds.Single(c=>c.Text=="/demo pick").Choices[0][0]=="11","Shared/player snapshots do not mutate each other");
            var limited=Frame.Build(new Player{Id=33,Admin=true,Power=Power.Generic});
            Check(limited.Mods[id].Cmds.Any(c=>c.Text=="/demo admin"),"Generic exported admin action visible to authorized admin");
            Check(limited.Mods[id].Cmds.All(c=>c.Text!="!secret"),"Registry-specific Commander power remains enforced");
            Check(limited.Mods[id].Cmds.Single(c=>c.Text=="/tuning").Admin,"Export can tighten player-registered tuning visibility");
            ValidateHash(limited);
            var full=Frame.Build(new Player{Id=44,Admin=true,Power=Power.Generic|Power.Commander});
            Check(full.Mods[id].Cmds.Any(c=>c.Text=="!secret"&&c.Power==Power.Commander.ToString()),"Registry powers cannot be weakened by export decoration");
            DemoProvider.Malformed=true;Frame.Invalidate();var malformed=Frame.Build(user);
            Check(malformed.Mods[id].Data==null,"Malformed export cannot escape as unfiltered data");
            Check(!JsonConvert.SerializeObject(malformed).Contains("private marker"),"Malformed admin metadata not leaked");
            DemoProvider.Malformed=false;Frame.Invalidate();var stableA=Frame.Build(user);Frame.Invalidate();var stableB=Frame.Build(user);
            Check(stableA.Root==stableB.Root,"Unchanged exports have deterministic hashes");
            Console.WriteLine($"PASS: {count} server export permission, personalization, privacy and hash checks. No gameplay mod loaded.");
        }
        finally{if(Directory.Exists(temp))Directory.Delete(temp,true);}
    }
}
internal static class DemoProvider
{
    internal static bool Malformed;
    public static void Root(Player p,string text){}
    public static void Tuning(Player p,string text){}
    public static void Secret(Player p,string text){}
    public static string ModUIData()=>Malformed?
        "{\"commands\":[{\"t\":\"/demo admin\",\"a\":true,\"d\":\"private marker\"},{\"a\":\"not-a-bool\"}]}":
        "{\"schema\":2,\"info\":[\"76561198000000000\"],\"numeric\":76561198000000000,\"commands\":[{\"t\":\"/demo admin\",\"a\":true},{\"t\":\"/tuning\",\"a\":true,\"v\":\"on\",\"ch\":[[\"on\",\"ON\"],[\"off\",\"OFF\"]]},{\"t\":\"!secret\",\"a\":true,\"p\":\"Generic\"}]}";
    public static string ModUIPlayerData(ulong steamId)=>"{\"commands\":[{\"t\":\"/demo pick\",\"l\":\"My choice\",\"ch\":[[\""+steamId+"\",\"Option\"]]}]}";
}
public class Player
{
    public ulong Id;public bool Admin,IsCommander;public Power Power;
    public bool IsAdmin()=>Admin;public Power GetAdminPowers()=>Power;
    public PlayerIdentity PlayerID=>new(){SteamID=new(){m_SteamID=Id}};
}
public class PlayerIdentity{public SteamIdentity SteamID;}
public class SteamIdentity{public ulong m_SteamID;}
public class NetworkServerSettings
{
    public static NetworkServerSettings Instance=>null;public List<Ban> BannedPlayers;
    public class Ban{public ulong m_ID;public string m_Name;}
}
public class GameDatabase{public static GameDatabase Database=>null;public List<Level> AllLevels;}
public class Level{public bool Enabled,IsMultiplayer,IsDeveloperOnly;public string FileName,DisplayName;public List<Mode> GameModes;}
public class Mode{public bool Enabled;public string ObjectName;}
namespace UnityEngine{public static class Time{public static float realtimeSinceStartup=>10;}}
namespace MelonLoader
{
    public class MelonMod{public static List<MelonMod> RegisteredMelons=new();public MelonAssembly MelonAssembly;public Info Info;}
    public class MelonAssembly{public Assembly Assembly;}
    public class Info{public string Name,Version;}
    public static class MelonLogger{public static void Warning(string text){}}
}
namespace MelonLoader.Utils{public static class MelonEnvironment{public static string UserDataDirectory;}}
namespace SilicaAdminMod
{
    [Flags] public enum Power{None=0,Generic=1,Commander=2}
    public static class HelperMethods{public delegate void CommandCallback(Player p,string text);}
    public class PlayerCommand{public string CommandName;public HelperMethods.CommandCallback PlayerCommandCallback;}
    public class AdminCommand{public string AdminCommandText,AdminCommandDescription;public Power AdminPower;public HelperMethods.CommandCallback AdminCallback;}
    public static class PlayerMethods{public static List<PlayerCommand> PlayerCommands=new(),PlayerPhrases=new();}
    public static class AdminMethods{public static List<AdminCommand> AdminCommands=new();public static bool PowerInPowers(Power required,Power actual)=>(required&actual)==required;}
}
