using System.Text.Json;
using System.Text.RegularExpressions;
using Il2Cpp;
using UnityEngine;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private sealed class FeedbackLine {internal string Mod,Text;internal bool AdminOnly;}
    private sealed class Exchange {internal CommandEntry Entry;internal double At;}
    private readonly List<CommandEntry> savedCommands=new(),dynamicCommands=new();
    private readonly List<FeedbackLine> feedback=new();
    private Exchange activeExchange;
    private double lastSend=-100;
    private bool controllerSending;
    private string menuTab="help",commandDraft="",controllerMessage="",commandSearch="",roleFilter="player";
    private CommandEntry selectedCommand;
    private CommandEntry pendingInputEntry;
    private string pendingInputPrefix="";
    private Vector2 moduleScroll,commandScroll,feedbackScroll;
    private bool addEntryOpen;
    private string addMod="",addCommand="",addDescription="",addRole="player";
    private bool addAdmin,addDebug;
    private static string Plain(string text)=>Regex.Replace(text??"",@"</?(?:b|i|u|s|color|size|font|material|sprite|align|indent|voffset|alpha|mark|style|noparse|nobr|link)(?:[=\s][^>]*)?>","",RegexOptions.IgnoreCase);
    private static string ModLabel(string name)=>Regex.Replace(Plain(name).Trim(),@"\bsi_","",RegexOptions.IgnoreCase);
    private static string RootCommand(string text)=>(text??"").Trim().Split(' ','\t')[0].ToLowerInvariant();
    private static bool CurrentAdmin=>Player.CurrentPlayer!=null&&(Game.GetIsSingleplayer()||Player.CurrentPlayer.AdminLevel!=EAdminLevel.NONE);
    private static string CurrentRole=>Player.CurrentPlayer?.IsCommander==true?"commander":"player";
    private static string ServerScope=>!HasSession?"":Game.GetIsSingleplayer()?"singleplayer":"server:"+NetworkGameClient.GetServerID().SteamID.m_SteamID;
    private string UserCommandsPath=>Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory,"Si_ModMenu","commands-"+placementProfile+".json");
    private static bool AdminCommand(CommandEntry c)=>c.AdminOnly||c.Role=="admin"||!c.Scope.StartsWith("local:")&&DebugCommand(c);
    private static bool DebugCommand(CommandEntry c)=>c.Debug||RootCommand(c.Command).TrimStart('/','!').Equals("debug",StringComparison.OrdinalIgnoreCase);
    private static bool RoleAllowed(string role)=>role=="shared"||role=="player"||role=="commander"&&(CurrentRole=="commander"||CurrentAdmin)||role=="admin"&&CurrentAdmin;
    private bool CommandAllowed(CommandEntry c)=>c!=null&&(!AdminCommand(c)||CurrentAdmin)&&RoleAllowed(c.Role)&&
        (c.Scope==ServerScope||c.Scope=="portable"&&HasSession||c.Scope.StartsWith("local:")&&localMods.Any(m=>LocalScope(m)==c.Scope));
    private IEnumerable<CommandEntry> AllCommands()=>CommandSnapshot();
    private IEnumerable<CommandEntry> BuildCommands()=>serverChoices.Concat(ServerCommands()).Concat(savedCommands).Concat(UserSidebarCommands()).Concat(portableCommands).Concat(dynamicCommands).Concat(ProviderCommands())
        .Where(c=>CommandAllowed(c))
        .GroupBy(c=>c.Mod+"\n"+c.Command+"\n"+c.Role+"\n"+(c.Source=="sidebar"?"sidebar":c.Source is "saved" or "imported"?"custom":"provided"),StringComparer.OrdinalIgnoreCase).Select(g=>g.First());

    private void LoadUserCommands()
    {
        savedCommands.Clear();
        try
        {
            if(File.Exists(UserCommandsPath))
                savedCommands.AddRange((JsonSerializer.Deserialize<CommandEntry[]>(File.ReadAllText(UserCommandsPath))??Array.Empty<CommandEntry>())
                    .Where(c=>c!=null&&ValidCommand(c.Command)&&!string.IsNullOrWhiteSpace(c.Mod)&&!string.IsNullOrEmpty(c.Scope)).Take(1000));
        }
        catch(Exception e){Error(e,"saved commands");}
        InvalidateUi();
    }
    private void SaveUserCommand()
    {
        var entry=new CommandEntry{Mod=ModLabel(addMod),Command=addCommand.Trim(),Description=addDescription.Trim(),
            Role=addRole,AdminOnly=addAdmin||addRole=="admin",Debug=addDebug,Scope=ServerScope,Source="saved"};
        if(entry.Mod.Length==0||!ValidCommand(entry.Command)){controllerMessage="Enter a mod name and one chat command.";return;}
        var next=savedCommands.Where(c=>!(c.Scope==entry.Scope&&c.Mod.Equals(entry.Mod,StringComparison.OrdinalIgnoreCase)&&
            c.Command.Equals(entry.Command,StringComparison.OrdinalIgnoreCase)&&c.Role==entry.Role)).ToList();
        next.Add(entry);
        if(next.Count>1000){controllerMessage="The saved command list is full.";return;}
        try
        {
            string path=UserCommandsPath;Directory.CreateDirectory(Path.GetDirectoryName(path));
            if(File.Exists(path))
            {
                string backup=path+"."+DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ")+".bak";
                File.Copy(path,backup,false);
                if(!System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path)).SequenceEqual(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(backup))))
                    throw new IOException("Command backup verification failed.");
            }
            string temp=path+".tmp";File.WriteAllText(temp,JsonSerializer.Serialize(next));File.Move(temp,path,true);
            savedCommands.Clear();savedCommands.AddRange(next);addEntryOpen=false;
            InvalidateUi();
            ExportCreatedCommands();
            controllerMessage="Saved locally for this server/session type. Availability remains controlled by the mod.";
        }
        catch(Exception e){controllerMessage="Could not save: "+e.GetBaseException().Message;}
    }
    private static bool ValidCommand(string text)=>!string.IsNullOrWhiteSpace(text)&&text.Length<=160&&
        (text[0]=='/'||text[0]=='!'||Regex.IsMatch(text,@"^\.\d"))&&!text.Any(char.IsControl);
    private void ClickCommand(CommandEntry entry)
    {
        if(!EntryAvailable(entry)){controllerMessage="This command changed or is no longer available.";return;}
        if(entry.Source=="provider")
        {
            var current=ProviderCommands().FirstOrDefault(c=>c.Scope==entry.Scope&&c.Command==entry.Command&&CommandAllowed(c));
            if(current==null)return;entry=current;
        }
        if(entry.Source=="server")
        {
            var current=ServerCommands().FirstOrDefault(c=>c.Source=="server"&&c.Group==entry.Group&&c.Command==entry.Command&&CommandAllowed(c));
            if(current==null)return;entry=current;
        }
        if(entry.Source=="server-menu"&&!serverChoices.Any(c=>ReferenceEquals(c,entry))){controllerMessage="This menu changed. Select a current option.";return;}
        selectedCommand=entry;commandDraft=entry.Command;
        pendingInputEntry=null;
        if(CommandNeedsInput(entry))
        {
            pendingInputEntry=entry;pendingInputPrefix=UiPolicies.InputBase(entry.Command);commandDraft=pendingInputPrefix+" ";
            focusCommandDraft=true;controllerMessage="INPUT REQUIRED: enter a value or argument after "+pendingInputPrefix+", then SEND or Enter.";return;
        }
        SendTypedChat(entry.Command);
    }
    private bool CommandNeedsInput(CommandEntry entry)=>UiPolicies.NeedsInput(entry.Command,entry.RequiresInput)||
        ServerCommands().Any(c=>c.Source=="server"&&c.Command==entry.Command&&c.RequiresInput&&CommandAllowed(c));
    private void SubmitDraft()
    {
        if(addEntryOpen||statusPickerOpen||sidebarMenuOpen||settingsMenuOpen||!panelOpen)return;
        if(pendingInputEntry!=null&&!UiPolicies.HasArguments(commandDraft,pendingInputPrefix))
        {controllerMessage="INPUT REQUIRED: append a value or argument before sending.";focusCommandDraft=true;return;}
        if(pendingInputEntry!=null&&!EntryAvailable(pendingInputEntry)){pendingInputEntry=null;controllerMessage="This command is no longer available.";return;}
        if(pendingInputEntry?.Source=="server-menu"&&!serverChoices.Any(c=>ReferenceEquals(c,pendingInputEntry)))
        {pendingInputEntry=null;controllerMessage="The menu expired or changed. Reopen it before submitting a value.";return;}
        pendingInputEntry=null;
        SendTypedChat(commandDraft);
    }
    private static bool ValidChat(string text)=>!string.IsNullOrWhiteSpace(text)&&text.Length<=160&&!text.Any(char.IsControl);
    private void SendTypedChat(string text)
    {
        if(!HasSession||!Application.isFocused||NativeMenu()||!ValidChat(text))return;
        double now=clock.Elapsed.TotalSeconds;if(now-lastSend<.1){controllerMessage="Please wait briefly before the next command.";return;}
        var entry=AllCommands().FirstOrDefault(c=>RootCommand(c.Command)==RootCommand(text));
        activeExchange=entry==null?null:new Exchange{Entry=entry,At=now};
        dynamicCommands.RemoveAll(c=>c.Source=="menu");lastSend=now;
        InvalidateUi();
        try
        {
            if(TrySendCompanionCommand(text,entry))return;
            serverChoices.Clear();serverRequest=null;
            controllerSending=true;
            bool accepted=Player.CurrentPlayer.SendChatMessage(text,false);
            debugChatSends++;AddDebugEvent("Chat submission: "+(accepted?"accepted":"handled or rejected")+"; length="+text.Length);
            controllerMessage=accepted?"Sent to chat.":"Handled or rejected locally; check feedback.";
        }
        catch(Exception e){activeExchange=null;controllerMessage=e.GetBaseException().Message;Error(e,"chat send");}
        finally{controllerSending=false;}
    }
    private void InvalidateMenuChoices(string text)
    {
        if(!HasSession||string.IsNullOrWhiteSpace(text))return;
        serverChoices.Clear();serverRequest=null;
        InvalidateUi();
        if(!ValidCommand(text.Trim()))return;
        dynamicCommands.RemoveAll(c=>c.Source=="menu");activeExchange=null;
        InvalidateUi();
    }
    private void ObserveFeedback(string raw,bool structured=false)
    {
        EnsureSessionContext();
        string text=Plain(raw).Trim();if(text.Length==0)return;
        if(text.Length>8192)text=text.Substring(0,8192);
        bool exchange=activeExchange!=null&&clock.Elapsed.TotalSeconds-activeExchange.At<=15&&CommandAllowed(activeExchange.Entry);
        var prefix=Regex.Match(text,@"^\s*\[([^\]\r\n]{1,80})\]");
        string mod=exchange?activeExchange.Entry.Mod:prefix.Success?ModLabel(prefix.Groups[1].Value):"Server";
        bool restricted=exchange?AdminCommand(activeExchange.Entry):Regex.IsMatch(text,@"\badmin(?:istrator)?(?:[- ]only|\s+commands?)\b",RegexOptions.IgnoreCase);
        feedback.Add(new FeedbackLine{Mod=mod,Text=text,AdminOnly=restricted});
        if(feedback.Count>200)feedback.RemoveAt(0);
        InvalidateUi();
        feedbackScroll.y=float.MaxValue;
        if(notificationsEnabled)AddNotification(ModLabel(mod),text);
        if(structured)return; // structured replies supply their own explicit navigation metadata
        foreach(string line in text.Split('\n'))
        {
            string role=exchange?activeExchange.Entry.Role:"unknown";
            if(Regex.IsMatch(line,@"\b(player|everyone|all players)\s+(commands?|controls?)\b",RegexOptions.IgnoreCase))role="shared";
            if(Regex.IsMatch(line,@"\bcommander\s+(commands?|controls?)\b",RegexOptions.IgnoreCase))role="commander";
            bool admin=restricted||Regex.IsMatch(line,@"\badmin(?:istrator)?\b",RegexOptions.IgnoreCase);
            if(admin)role="admin";
            foreach(Match match in Regex.Matches(line,@"(?<![\w:/])([/!][A-Za-z][A-Za-z0-9_-]*)\b"))
            {
                string command=match.Groups[1].Value;
                bool debug=RootCommand(command).TrimStart('/','!').Equals("debug",StringComparison.OrdinalIgnoreCase);
                // Unclassified server suggestions fail closed; no mod-specific permission guesses.
                bool onlyAdmin=admin||role=="unknown"||command.StartsWith("!")||debug;
                string advertisedRole=role=="unknown"&&debug?"admin":role;
                AddLearned(mod,command,line,advertisedRole,onlyAdmin,debug,"feedback");
                // Expand only choices actually advertised after a command, e.g. /debug [on|off].
                string tail=line.Substring(match.Index+match.Length);
                var choices=Regex.Match(tail,@"^\s*[\[<]?([A-Za-z0-9_-]+(?:\|[A-Za-z0-9_-]+)+)[\]>]?");
                if(choices.Success)foreach(string choice in choices.Groups[1].Value.Split('|'))
                    AddLearned(mod,command+" "+choice,line,advertisedRole,onlyAdmin,debug,"feedback");
            }
            if(!exchange)continue;
            // Generic explicit command rows only. Bare numbers are not assigned a guessed command prefix.
            foreach(Match row in Regex.Matches(line,@"(?:^|\s)([/!]\d{1,4}|/back|/close|/exit)(?=\s|[).:]|$)",RegexOptions.IgnoreCase))
                AddLearned(mod,row.Groups[1].Value,line,activeExchange.Entry.Role,AdminCommand(activeExchange.Entry),DebugCommand(activeExchange.Entry),"menu");
        }
        if(dynamicCommands.Count>1000)dynamicCommands.RemoveRange(0,dynamicCommands.Count-1000);
    }
    private void AddLearned(string mod,string command,string description,string role,bool admin,bool debug,string source)
    {
        string scope=ServerScope;
        var existing=dynamicCommands.FirstOrDefault(c=>c.Scope==scope&&c.Mod==mod&&c.Command==command);
        if(existing!=null){existing.AdminOnly|=admin;existing.Debug|=debug;return;}
        dynamicCommands.Add(new CommandEntry{Mod=mod,Command=command,Description=description,Role=role,AdminOnly=admin,Debug=debug,Scope=scope,Source=source});
    }
}


