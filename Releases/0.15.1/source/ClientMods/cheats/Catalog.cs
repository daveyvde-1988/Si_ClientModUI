namespace SilicaCheats;
public sealed class CheatCommand
{
    public readonly string Key,Category,Description,Syntax;
    public readonly bool Server,Destructive;
    public CheatCommand(string key,string category,string description,string syntax,bool server=true,bool destructive=false)
    {Key=key;Category=category;Description=description;Syntax=syntax;Server=server;Destructive=destructive;}
}
public static class Catalog
{
    // Every command whose inspected vanilla handler checks CheatsEnabled/Local, plus Camera.
    public static readonly CheatCommand[] All={
        new("Resources","Economy and construction","Add or remove team resources.","<team> <signed amount>"),
        new("Spawn","Spawning","Spawn any network prefab, with a team and explicit camera pose.","<prefab> <team|none> <x,y,z,w,qx,qy,qz> [true]"),
        new("Build","Economy and construction","Unlimited construction: ignores cost and distance.","[true|false]"),
        new("UnlinkedStructures","Economy and construction","Enable or disable unlinked structure state and decay for a team.","<team> [true|false]"),
        new("Damage","Combat and AI","Enable or disable damage (false disables damage).","[true|false]"),
        new("AI","Combat and AI","Enable or disable AI.","[true|false]"),
        new("AICommander","Combat and AI","Toggle the selected team's AI commander.","[team]"),
        new("Team","Player and camera","Join a team or leave to spectator.","<team|none>",false),
        new("Camera","Player and camera","Copy current camera position and quaternion to clipboard.","",false),
        new("Suicide","Combat and AI","Destroy your currently controlled unit.","",false,true),
        new("Destroy","Combat and AI","Apply full damage to ALL objects, optionally limited to a team.","[team]",true,true),
        new("Delete","Combat and AI","Delete units, structures, or everything directly.","<unit|structure|all> [current|all]",true,true),
        new("TimeScale","Time and environment","Set game speed from 0 to 10; no argument resets to 1.","[0..10]"),
        new("DayScale","Time and environment","Set day/night cycle speed.","<multiplier>"),
        new("SunScale","Time and environment","Set sunlight brightness scale.","<multiplier>"),
        new("MoonScale","Time and environment","Set moonlight brightness scale.","<multiplier>"),
        new("AllowVisionModes","Vision and debugging","Allow vision modes.","<true|false>"),
        new("Fog","Vision and debugging","Enable or disable fog of war.","[true|false]",false),
        new("Units","Vision and debugging","Enable or disable unit debugging.","[true|false]",false),
        new("Health","Vision and debugging","Cycle health display: hidden, health, resource amounts.","",false),
        new("DamageDebug","Vision and debugging","Toggle projectile and attack debug display.","",false),
        new("Lights","Vision and debugging","Toggle game object lights.","",false),
        new("NetList","Vision and debugging","Toggle network object debug list.","",false),
        new("NetTickMult","Vision and debugging","Network tick multiplier; native range 0.1..30.","<0.1..30>"),
        new("Get","Match and scenario","Read a game mode parameter; no argument lists parameters.","[parameter]"),
        new("Set","Match and scenario","Set a game mode parameter for this match.","<parameter> <value>"),
        new("Map","Match and scenario","Load a map and optional game mode.","<map> [game mode]",true,true),
        new("Mission","Match and scenario","Load a campaign mission (requires the game's campaign password).","<password> [mission]",true,true),
        new("EndRound","Match and scenario","End the current round.","",true,true),
        new("EndSegment","Match and scenario","End the current scenario segment.","",true,true),
        new("Cheats","Access","Toggle native cheats. This build inverts explicit boolean arguments.","[toggle]")
    };
    public static CheatCommand Find(string line)=>All.FirstOrDefault(c=>c.Key.Equals((line??"").Trim().Split(' ')[0],StringComparison.OrdinalIgnoreCase));
}
