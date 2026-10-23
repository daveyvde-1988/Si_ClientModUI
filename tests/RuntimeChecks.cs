using System.Text.Json;

namespace SiSquadHud;
internal static class RuntimeChecks
{
    internal static void Run(Action<bool,string> check)
    {
        check(UiPolicies.HousekeepingSeconds==1&&UiPolicies.UiRefreshSeconds==.05,"Housekeeping and UI refresh policy");
        foreach(bool isAdmin in new[]{false,true})
        {
            check(UiPolicies.ModCategory(false,false,isAdmin),"Player controls available in player view");
            check(!UiPolicies.ModCategory(true,false,isAdmin),"Admin controls excluded from player view");
            check(!UiPolicies.ModCategory(false,true,isAdmin),"Player controls excluded from admin view");
            check(UiPolicies.ModCategory(true,true,isAdmin)==isAdmin,"Admin controls require current admin access");
        }
        var defaults=new SquadHud.LocalLayout();
        check(!defaults.ClientDebug&&defaults.ShowModMenuButton,"Diagnostics default off and menu button defaults visible");
        var hidden=JsonSerializer.Deserialize<SquadHud.LocalLayout>("{\"ShowModMenuButton\":false,\"TextSize\":125}");
        check(!hidden.ShowModMenuButton&&hidden.TextSize==125,"Hidden menu setting loads without changing appearance");
        check(JsonSerializer.Deserialize<SquadHud.LocalLayout>(JsonSerializer.Serialize(hidden)).ShowModMenuButton==false,"Hidden menu setting persists");
        var keys=new HashSet<string>();
        foreach(string team in new[]{"team:sol:0","team:sol:1","team:alien:2","spectator:-1"})foreach(bool commander in new[]{false,true})
            keys.Add(UiPolicies.WindowKey("sidebar",team,commander));
        check(keys.Count==8,"Individual teams, same-name teams, spectator and roles have independent window keys");
        check(UiPolicies.SidebarColumns(78,550,12)==1,"Narrow sidebar remains vertical");
        check(UiPolicies.SidebarColumns(550,60,8)==8,"Horizontal sidebar places buttons side by side");
        int square=UiPolicies.SidebarColumns(280,280,12);
        check(square>1&&square<12,"Square sidebar uses a grid");
        check(UiPolicies.SidebarColumns(1,1,0)==1,"Empty/minimum sidebar geometry stays valid");
        check(UiPolicies.NeedsInput("!kick",true)&&UiPolicies.NeedsInput("/set <value>",false)&&!UiPolicies.NeedsInput("/buy",false),"Only declared/template commands require a prompt");
        check(UiPolicies.InputBase("/set <value>")=="/set"&&UiPolicies.InputBase("/buy")=="/buy","Input prompt separates the editable value");
        check(!UiPolicies.HasArguments("/set","/set")&&!UiPolicies.HasArguments("/set ","/set")&&!UiPolicies.HasArguments("/setting 1","/set")&&UiPolicies.HasArguments("/set 1.5","/set"),"Required input cannot send an empty value or a different command");
        foreach(float fraction in new[]{-5f,0f,.3f,1f,5f,float.NaN,float.PositiveInfinity})
            check(UiPolicies.Split(fraction)>=.18f&&UiPolicies.Split(fraction)<=.65f,"Divider proportions are finite and bounded");
        check(UiPolicies.HoldWanted(true,true,true,true,true,false,false,false,false,false),"F7 acquires the FPS sidebar while held");
        foreach(int failure in Enumerable.Range(0,10))
        {
            bool wanted=UiPolicies.HoldWanted(failure!=0,failure!=1,failure!=2,failure!=3,failure!=4,failure==5,failure==6,failure==7,failure==8,failure==9);
            check(!wanted,"F7 exits on release, disabled option, commander, loading, focus loss, native menu/text, panel, picker or release latch");
        }
        int builds=0;var cache=new RevisionCache<int>();
        check(cache.Get(0,1,()=>++builds)==1,"UI cache initial build");
        for(int i=2;i<1002;i++)cache.Get(.01,i,()=>++builds);
        check(builds==1,"Repeated GUI passes and invalidations do not rebuild above 20 Hz");
        check(cache.Get(.05,1002,()=>++builds)==2,"Dirty snapshot refreshes at the next permitted interval");
        check(cache.Get(5,1002,()=>++builds)==2&&builds==2,"Unchanged command data does not rebuild merely because time passed");
        cache.Force();check(cache.Get(5.001,1003,()=>++builds)==3,"Role/session changes can invalidate immediately");
        var traffic=new TrafficMeter();traffic.Sent(100,true);traffic.Sent(50,false);traffic.Received(250);traffic.CompanionSent(20,true);traffic.CompanionSent(20,false);traffic.CompanionReceived(40);traffic.Tick(1);
        check(traffic.TxPackets==1&&traffic.TxBytes==100&&traffic.Failures==1&&traffic.RxPackets==1&&traffic.RxBytes==250,"Traffic separates successful bytes from failed submissions");
        check(traffic.TxBytesPerSecond==100&&traffic.RxBytesPerSecond==250,"Traffic rate accounts for elapsed time");
        check(traffic.CompanionTxBytes==20&&traffic.CompanionRxBytes==40&&traffic.CompanionTxPackets==1,"Companion payload counters remain separate");
        traffic.Tick(2);check(traffic.TxBytesPerSecond==0&&traffic.RxBytesPerSecond==0,"Traffic rates return to zero when idle");
        var old=JsonSerializer.Deserialize<SquadHud.LocalLayout>("{\"Windows\":{\"sidebar-player\":{\"X\":0.1,\"Y\":0.4,\"Width\":88,\"Height\":400}},\"SidebarEnabled\":true,\"TextSize\":125}");
        check(old.HoldSidebarF7&&old.SidebarEnabled&&old.TextSize==125&&old.Windows["sidebar-player"].Width==88,"Legacy preferences and windows deserialize with the new hold option");
        string fps=UiPolicies.WindowKey("menu","team:sol:0",false),commanderKey=UiPolicies.WindowKey("menu","team:sol:0",true);
        old.Windows[fps]=new(){X=.2f,Y=.3f,Width=900,Height=700};old.Windows[commanderKey]=new(){X=.7f,Y=.8f,Width=1000,Height=800};old.FeedbackSplit[fps]=.25f;old.FeedbackSplit[commanderKey]=.6f;
        var restored=JsonSerializer.Deserialize<SquadHud.LocalLayout>(JsonSerializer.Serialize(old));
        check(restored.Windows[fps].X==.2f&&restored.Windows[commanderKey].X==.7f&&restored.FeedbackSplit[fps]==.25f&&restored.FeedbackSplit[commanderKey]==.6f,"Role/team window and divider positions survive JSON round trips independently");
        var editor=ServerMenuParser.Parse(new[]{"[Editor] 1. health_mult = 1.5","[Editor] Set: .1 1.5 · /back to return · /0 close"},"/",Array.Empty<string>());
        check(editor.First(x=>x.Command=="/1").RequiresInput&&!editor.First(x=>x.Command=="/back").RequiresInput&&!editor.First(x=>x.Command=="/0").RequiresInput,"Editor value choices prompt while navigation remains immediate");
    }
}

public sealed partial class SquadHud
{
    internal static void RunPersistenceChecks(Action<bool,string> check)
    {
        var c=new SquadHud();Directory.CreateDirectory(MelonLoader.Utils.MelonEnvironment.UserDataDirectory);
        c.ExportCreatedCommands();check(!File.Exists(PortableCommandsPath),"A fresh empty shell creates no command export");
        var saved=new CommandEntry{Mod="Custom",Command="/example",Role="player",Scope=ServerScope,Source="saved"};
        var imported=new CommandEntry{Mod="Custom",Command="/example",Role="player",Scope="portable",Source="imported"};
        c.savedCommands.Add(saved);c.portableCommands.Add(imported);c.selectedCommand=c.pendingInputEntry=saved;c.commandDraft="/example value";
        WriteVerifiedJson(c.UserCommandsPath,JsonSerializer.Serialize(c.savedCommands));c.ExportCreatedCommands();
        check(c.EntryAvailable(saved)&&JsonSerializer.Deserialize<PortableCommandFile>(File.ReadAllText(PortableCommandsPath)).Commands.Count==1,"Saved and imported copies deduplicate in the shared export");
        c.RemoveCustomCommand(saved);
        check(!c.EntryAvailable(saved)&&c.savedCommands.Count==0&&c.portableCommands.Count==0&&c.pendingInputEntry==null&&c.commandDraft=="","Removed custom entries cannot execute through a stale UI reference");
        check(JsonSerializer.Deserialize<CommandEntry[]>(File.ReadAllText(c.UserCommandsPath)).Length==0,"Removal persists the private command file");
        check(JsonSerializer.Deserialize<PortableCommandFile>(File.ReadAllText(PortableCommandsPath)).Commands.Count==0,"Deleting the last definition clears an existing export");
        c.LoadPortableCommands();check(c.portableCommands.Count==0,"Deleted definitions do not return on import/restart");
        c.layout.SidebarButtons.Add(new(){Mod="Custom",Label="Example",Command="/example",Role="player",TeamScope="team:sol"});c.ExportCreatedCommands();
        c.RemoveCustomCommand(c.UserSidebarCommands().First());
        check(c.layout.SidebarButtons.Count==1&&JsonSerializer.Deserialize<LocalLayout>(File.ReadAllText(Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory,"layout.json"))).SidebarButtons.Count==1,"Removing a command-list entry preserves deliberately saved sidebar bindings");
        check(c.layout.RemovedCustomCommands.Count>0&&JsonSerializer.Deserialize<PortableCommandFile>(File.ReadAllText(PortableCommandsPath)).Commands.Count==0,"Deletion markers prevent retained sidebar definitions from returning in exports");
        check(Directory.GetFiles(MelonLoader.Utils.MelonEnvironment.UserDataDirectory,"*.bak").Length>0,"JSON replacements retain verified backups");
        check(c.Errors.Count==0,"Custom command persistence checks finish without hidden errors");
        string temporary=Path.GetFullPath(MelonLoader.Utils.MelonEnvironment.UserDataDirectory);
        if(!temporary.StartsWith(Path.Combine(Path.GetFullPath(Path.GetTempPath()),"ModUI-network-checks-"),StringComparison.OrdinalIgnoreCase))throw new Exception("Unexpected cleanup path");
        Directory.Delete(temporary,true);
    }
}
