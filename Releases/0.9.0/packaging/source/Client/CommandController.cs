using System.Text.Json;
using System.Text.RegularExpressions;
using Il2Cpp;
using UnityEngine;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    public sealed class CommandEntry
    {
        public string Mod {get;set;}="";
        public string Command {get;set;}="";
        public string Description {get;set;}="";
        public string Role {get;set;}="shared";
        public bool Arguments {get;set;}
        public string Source {get;set;}="catalog";
    }
    private sealed class Observation
    {
        internal string Label,Value,Mod;
        internal double At;
    }
    private sealed class FeedbackLine {internal string Mod,Text;internal double At;}
    private sealed class Exchange {internal string Mod,Command;internal double At;}
    private readonly List<CommandEntry> commands=new(),dynamicCommands=new();
    private readonly HashSet<string> sessionCommands=new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string,Observation> observedValues=new();
    private readonly List<FeedbackLine> feedback=new();
    private Exchange activeExchange;
    private int feedbackRevision;
    private double lastSend=-100;
    private bool controllerSending;
    private string menuTab="help",commandDraft="",controllerMessage="",commandSearch="",roleFilter="all";
    private CommandEntry selectedCommand;
    private Vector2 moduleScroll,commandScroll,feedbackScroll;
    private static string Plain(string text)=>Regex.Replace(text??"","<[^>]*>","");
    private static string RootCommand(string text)=>text.Trim().Split(' ','\t')[0].ToLowerInvariant();
    private IEnumerable<CommandEntry> AllCommands()=>commands.Concat(dynamicCommands)
        .GroupBy(c=>c.Mod+"\n"+c.Command,StringComparer.OrdinalIgnoreCase).Select(g=>g.First());

    private void LoadCommandCatalog()
    {
        try
        {
            using var stream=typeof(SquadHud).Assembly.GetManifestResourceStream("SiSquadHud.commands.json");
            using var reader=new StreamReader(stream);
            commands.AddRange(JsonSerializer.Deserialize<CommandEntry[]>(reader.ReadToEnd())??Array.Empty<CommandEntry>());
            string custom=Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory,"Si_SquadHUD","commands.json");
            if(File.Exists(custom))
                commands.AddRange((JsonSerializer.Deserialize<CommandEntry[]>(File.ReadAllText(custom))??Array.Empty<CommandEntry>())
                    .Where(c=>c!=null&&ValidCommand(c.Command)&&c.Mod?.Length>0).Take(1000));
        }
        catch(Exception error){Error(error,"command catalog");}
    }
    private static bool ValidCommand(string text)=>!string.IsNullOrWhiteSpace(text)&&text.Length<=160&&
        (text[0]=='/'||text[0]=='!'||Regex.IsMatch(text,@"^\.\d"))&&!text.Any(char.IsControl);
    private void ClickCommand(CommandEntry entry)
    {
        if(entry.Arguments)
        {
            selectedCommand=entry;commandDraft=entry.Command;
            controllerMessage="Enter arguments, then click SEND. Only the displayed command is sent.";return;
        }
        SendCommand(entry,entry.Command);
    }
    private void SendCommand(CommandEntry entry,string command)
    {
        if(!HasSession||!Application.isFocused||NativeMenu())return;
        EnsureSessionContext();
        if(entry.Source=="menu"&&(activeExchange?.Mod!=entry.Mod||clock.Elapsed.TotalSeconds-activeExchange.At>=60))
        {controllerMessage="This menu choice has expired. Open the native menu again.";return;}
        command=command.Trim();
        if(!ValidCommand(command)||Regex.IsMatch(command,@"<[^>]+>|\[[^\]]+\]")||command.Contains('|'))
        {controllerMessage="Replace argument placeholders with a single complete command (160 characters maximum).";return;}
        double now=clock.Elapsed.TotalSeconds;
        if(now-lastSend<.35){controllerMessage="Wait a moment before sending another command.";return;}
        // No retries: chat commands may have side effects and no idempotency receipt.
        activeExchange=new Exchange{Mod=entry.Mod,Command=command,At=now};
        dynamicCommands.RemoveAll(c=>c.Source=="menu");
        lastSend=now;
        try
        {
            // Native player entrypoint also allows locally installed mods to intercept SP commands.
            controllerSending=true;
            bool accepted=Player.CurrentPlayer.SendChatMessage(command,false);
            controllerMessage=accepted?"Sent "+command+". Check the mod response below.":"Command handled or rejected locally. Check the mod response.";
            sessionCommands.Add(entry.Mod);selectedCommand=null;commandDraft="";
            // Do not claim success or change ON/OFF/selection based on a click.
        }
        catch(Exception e){activeExchange=null;controllerMessage="Command could not be sent: "+e.GetBaseException().Message;Error(e,"chat send");}
        finally{controllerSending=false;}
    }
    private void InvalidateMenuChoices(string text)
    {
        if(!HasSession||string.IsNullOrWhiteSpace(text)||!ValidCommand(text.Trim()))return;
        dynamicCommands.RemoveAll(c=>c.Source=="menu");activeExchange=null;
    }
    private string IdentifyMod(string text)
    {
        if(text.Contains("[Ranks]",StringComparison.OrdinalIgnoreCase))return "si_ranks";
        if(text.Contains("[KGT",StringComparison.OrdinalIgnoreCase))return "si_kingofthehill";
        if(text.Contains("[Formation]",StringComparison.OrdinalIgnoreCase)||text.StartsWith("Si_Formation",StringComparison.OrdinalIgnoreCase))return "si_formation";
        if(text.Contains("[TELEPORTER]",StringComparison.OrdinalIgnoreCase))return "si_heavyteleporter";
        if(text.Contains("[Logistics]",StringComparison.OrdinalIgnoreCase))return "si_logistics";
        if(text.Contains("[UnitBalance]",StringComparison.OrdinalIgnoreCase)||text.Contains("[BAL]",StringComparison.OrdinalIgnoreCase))return "si_unitbalance";
        if(activeExchange!=null&&clock.Elapsed.TotalSeconds-activeExchange.At<=15)return activeExchange.Mod;
        return "server";
    }
    private void ObserveFeedback(string raw)
    {
        EnsureSessionContext();
        string text=Plain(raw).Trim();
        if(text.Length==0)return;
        if(text.Length>8192)text=text.Substring(0,8192);
        string mod=IdentifyMod(text);
        feedback.Add(new FeedbackLine{Mod=mod,Text=text,At=clock.Elapsed.TotalSeconds});
        if(feedback.Count>200)feedback.RemoveAt(0);
        feedbackRevision++;feedbackScroll.y=float.MaxValue;
        if(notificationsEnabled)AddNotification(mod,text);
        ReadObservedStatus(mod,text);
        bool inExchange=activeExchange!=null&&mod==activeExchange.Mod&&clock.Elapsed.TotalSeconds-activeExchange.At<=15;
        foreach(string line in text.Split('\n'))
        {
            // Only server feedback, never player messages. Descriptions remain inert until clicked.
            foreach(Match match in Regex.Matches(line,@"(?<![\w:/])([/!][A-Za-z][A-Za-z0-9_-]*)\b"))
            {
                string command=match.Groups[1].Value;
                string owner=AllCommands().FirstOrDefault(c=>RootCommand(c.Command)==command.ToLowerInvariant())?.Mod??mod;
                if(!AllCommands().Any(c=>c.Command.Equals(command,StringComparison.OrdinalIgnoreCase)))
                    dynamicCommands.Add(new CommandEntry{Mod=owner,Command=command,Description="Mentioned in received server feedback. Arguments may be required.",Source="feedback"});
            }
            if(!inExchange)continue;
            // Explicit numeric rows from a currently requested native menu. Global numeric ownership stays with the mod.
            string menuLine=Regex.Replace(line.Trim(),@"^(?:\[[^\]]+\]\s*)+","");
            var menuRows=Regex.Matches(menuLine,@"(?:^|\s{2,})/?(\d{1,4})[).:\-]\s+(.+?)(?=\s{2,}/?\d{1,4}[).:\-]\s+|$)").Cast<Match>().ToList();
            var slashRow=Regex.Match(menuLine,@"^/(\d{1,4})\s+(.+)$");
            if(slashRow.Success)menuRows.Add(slashRow);
            bool knownNumberMenu=mod=="si_unitbalance"||mod=="si_formation"||mod=="si_kingofthehill"||mod=="si_heavyteleporter";
            foreach(var row in menuRows)
            {
                if(!knownNumberMenu&&!menuLine.StartsWith("/"))continue;
                string command=(mod=="si_heavyteleporter"?"/st ":"/")+row.Groups[1].Value;
                if(!dynamicCommands.Any(c=>c.Source=="menu"&&c.Command==command))
                    dynamicCommands.Add(new CommandEntry{Mod=mod,Command=command,Description=Plain(row.Groups[2].Value).Trim(),Source="menu"});
            }
            var edit=Regex.Match(menuLine,@"Set:\s*([/.]\d{1,4})(?:\s|$)",RegexOptions.IgnoreCase);
            if(edit.Success)
            {
                string command=edit.Groups[1].Value;
                var existing=dynamicCommands.FirstOrDefault(c=>c.Source=="menu"&&c.Command==command);
                if(existing!=null)existing.Arguments=true;
                else dynamicCommands.Add(new CommandEntry{Mod=mod,Command=command,Description=menuLine,Source="menu",Arguments=true});
            }
            foreach(string navigation in new[]{"/back","/0"})
                if(line.Contains(navigation,StringComparison.OrdinalIgnoreCase)&&!dynamicCommands.Any(c=>c.Source=="menu"&&c.Command==navigation))
                    dynamicCommands.Add(new CommandEntry{Mod=mod,Command=navigation,Description="Native menu navigation",Source="menu"});
        }
        if(dynamicCommands.Count>1000)dynamicCommands.RemoveRange(0,dynamicCommands.Count-1000);
    }
    private void ReadObservedStatus(string mod,string text)
    {
        void Save(string id,string label,string value)=>observedValues[id]=new Observation{Mod=mod,Label=label,Value=value,At=clock.Elapsed.TotalSeconds};
        if(mod=="si_kingofthehill")
        {
            var m=Regex.Match(text,@"(?:Credits:\s*|Remaining:\s*)(\d[\d,]*)",RegexOptions.IgnoreCase);
            if(m.Success)Save("koh.credits","Personal credits",m.Groups[1].Value);
        }
        if(mod=="si_ranks")
        {
            string name=Regex.Escape(Plain(Player.CurrentPlayer?.PlayerName??"").Replace("<","[").Replace(">","]"));
            var m=Regex.Match(text,@"(?:Starting rank:\s*|"+name+@" is )(.+?)(?:\. Personal credits:|, has )");
            if(m.Success)Save("ranks.rank","Rank",m.Groups[1].Value);
            if(m.Success)
            {
                var cap=Regex.Match(text,@"(?:Squad size:\s*|gets\s+)(\d+)(?:\s+squad| \(including)");
                if(cap.Success)Save("ranks.cap","Rank squad allowance",cap.Groups[1].Value);
            }
        }
        if(mod=="si_formation")
        {
            foreach(string function in new[]{"move","follow","attack","commander"})
            {
                var m=Regex.Match(text,@"(?:^|\n)(?:\[Formation\]\s*)?"+function+@":\s*(.+?)\s*\(effective (ON|native)\)",RegexOptions.IgnoreCase);
                if(m.Success)Save("formation."+function,function+" formation",m.Groups[1].Value+" ("+m.Groups[2].Value+")");
            }
            var max=Regex.Match(text,@"effective FPS group maximum:\s*(\d+)",RegexOptions.IgnoreCase);
            if(max.Success)Save("formation.cap","Server squad allowance",max.Groups[1].Value);
        }
        availableAt=0;
    }
    private string ObservedValue(string id)=>observedValues.TryGetValue(id,out var value)
        ?value.Value+" (seen "+Math.Max(0,(int)(clock.Elapsed.TotalSeconds-value.At))+"s ago)":"Unknown";
}

