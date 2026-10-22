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
    private void RemoveCustomCommand(CommandEntry entry)
    {
        if(!CustomCommand(entry)||!CommandAllowed(entry))return;
        bool Same(CommandEntry c)=>c.Mod.Equals(entry.Mod,StringComparison.OrdinalIgnoreCase)&&c.Command.Equals(entry.Command,StringComparison.OrdinalIgnoreCase)&&c.Role==entry.Role;
        try
        {
            var next=savedCommands.Where(c=>!(Same(c)&&c.Scope==entry.Scope)).ToList();
            if(next.Count!=savedCommands.Count)WriteVerifiedJson(UserCommandsPath,JsonSerializer.Serialize(next));
            savedCommands.Clear();savedCommands.AddRange(next);portableCommands.RemoveAll(c=>Same(c));
            if(entry.Source=="sidebar")
            {
                var bindings=CurrentSidebarBindings.Where(b=>b.Kind=="chat"&&b.Command==entry.Command&&ModLabel(b.Mod)==entry.Mod&&b.Role==entry.Role).ToArray();
                foreach(var binding in bindings)layout.SidebarButtons.Remove(binding);
                placementDirty=true;SaveWindowPosition();
            }
            if(selectedCommand==entry){selectedCommand=null;pendingInputEntry=null;commandDraft="";}
            ExportCreatedCommands();InvalidateUi();controllerMessage="Custom chat control removed locally.";
        }
        catch(Exception e){controllerMessage="Could not remove the custom control: "+e.GetBaseException().Message;Error(e,"remove custom control");}
    }
}
