using System.Text.Json;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private bool EntryAvailable(CommandEntry entry)
    {
        if(!CommandAllowed(entry))return false;
        bool Matches(CommandEntry c)=>c.Command==entry.Command&&c.Mod==entry.Mod&&CommandAllowed(c);
        return entry.Source switch
        {
            "server-menu"=>serverChoices.Any(c=>ReferenceEquals(c,entry)),
            "server-vote"=>serverVotes.Any(c=>ReferenceEquals(c,entry)),
            "server"=>ServerCommands().Any(c=>c.Source=="server"&&c.Group==entry.Group&&Matches(c)),
            "provider"=>ProviderCommands().Any(c=>Matches(c)),
            "saved"=>savedCommands.Contains(entry),
            "imported"=>portableCommands.Contains(entry),
            "sidebar"=>UserSidebarCommands().Any(c=>Matches(c)),
            "menu"=>dynamicCommands.Contains(entry)&&activeExchange!=null&&clock.Elapsed.TotalSeconds-activeExchange.At<60,
            _=>true
        };
    }
    private void ForgetPortableSidebar(SidebarBinding binding)=>portableCommands.RemoveAll(c=>c.Command==binding.Command&&c.Role==binding.Role&&c.Mod==ModLabel(binding.Mod));
    private static bool CustomCommand(CommandEntry entry)=>entry.Source is "saved" or "imported" or "sidebar";
    private static string CustomCommandKey(string mod,string command,string role)=>ModLabel(mod).ToLowerInvariant()+"\n"+(command??"").Trim().ToLowerInvariant()+"\n"+(role??"player").ToLowerInvariant();
    private bool CustomCommandRemoved(CommandEntry entry)=>CustomCommand(entry)&&layout.RemovedCustomCommands.Contains(CustomCommandKey(entry.Mod,entry.Command,entry.Role));
    private void RemoveCustomCommand(CommandEntry entry)
    {
        if(!CustomCommand(entry)||!CommandAllowed(entry))return;
        bool Same(CommandEntry c)=>c.Mod.Equals(entry.Mod,StringComparison.OrdinalIgnoreCase)&&c.Command.Equals(entry.Command,StringComparison.OrdinalIgnoreCase)&&c.Role==entry.Role;
        try
        {
            // Saved server copies and the portable export are the same user definition.
            // Remember deletion before rewriting either file so an older copy cannot revive it.
            layout.RemovedCustomCommands.Add(CustomCommandKey(entry.Mod,entry.Command,entry.Role));
            WriteVerifiedJson(placementPath,JsonSerializer.Serialize(layout));placementDirty=false;
            var next=savedCommands.Where(c=>!Same(c)).ToList();
            if(next.Count!=savedCommands.Count)WriteVerifiedJson(UserCommandsPath,JsonSerializer.Serialize(next));
            savedCommands.Clear();savedCommands.AddRange(next);portableCommands.RemoveAll(c=>Same(c));
            // Sidebar buttons have their own Remove control; forgetting a command-list
            // entry should not erase a deliberately saved button on another team.
            if(selectedCommand==entry){selectedCommand=null;pendingInputEntry=null;commandDraft="";}
            ExportCreatedCommands();InvalidateUi();controllerMessage="Custom chat control removed locally.";
        }
        catch(Exception e){InvalidateUi();controllerMessage="Could not remove the custom control: "+e.GetBaseException().Message;Error(e,"remove custom control");}
    }
}
