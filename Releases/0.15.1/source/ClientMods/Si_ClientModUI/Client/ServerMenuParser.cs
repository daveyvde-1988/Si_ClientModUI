using System.Text.RegularExpressions;

namespace SiSquadHud;

// Legacy chat menus share a transport, not a text layout. No gameplay-mod names or catalogs here.
internal static class ServerMenuParser
{
    internal sealed class Choice
    {
        internal string Command,Label;
    }
    private static readonly Regex Tags=new(@"<[^>]*>",RegexOptions.Compiled);
    private static readonly Regex Prefix=new(@"^\s*(?:\[[^\]\r\n]{1,80}\]\s*)+",RegexOptions.Compiled);
    private static readonly Regex TextPrefix=new(@"^[\p{L}\p{N}_ -]{1,80}:\s*(?=(?:[/!.][0-9]{1,4}(?:[.)])?\s|[0-9]{1,4}[.)]\s))",RegexOptions.Compiled);
    private static readonly Regex Rows=new(@"(?:^|[ \t]{2,}|[|\u00b7\u2022]\s*)(?:(?<cmd>[/!.][0-9]{1,4})(?:[.)])?\s+(?![-/])|(?<num>[0-9]{1,4})[.)]\s+)",RegexOptions.Compiled);
    private static readonly Regex Navigation=new(@"(?<![\w:/!.])(?<cmd>[/!.](?:back|exit|close|0))(?=$|[\s;,.()])",RegexOptions.IgnoreCase|RegexOptions.Compiled);
    private static readonly Regex CloseCommand=new(@"(?<![\w:/!.])(?<cmd>[/!.][A-Za-z][A-Za-z0-9_-]*)(?:\s+to)?\s+(?:closes?|exits?)(?=$|[\s;,.])",RegexOptions.IgnoreCase|RegexOptions.Compiled);

    internal static IReadOnlyList<Choice> Parse(IEnumerable<string> lines,string prefix,IEnumerable<string> registeredCommands)
    {
        if(prefix is not ("/" or "!" or "."))return Array.Empty<Choice>();
        var result=new List<Choice>();var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var registered=new HashSet<string>(registeredCommands??Array.Empty<string>(),StringComparer.OrdinalIgnoreCase);
        void Add(string command,string label)
        {
            if(result.Count>=256||!Protocol.ValidCommandText(command)||!seen.Add(command))return;
            label=label.Trim();if(label.Length>512)label=label.Substring(0,512);
            result.Add(new Choice{Command=command,Label=label});
        }
        foreach(string raw in (lines??Array.Empty<string>()).Take(200))
        {
            string plain=Tags.Replace((raw??"").Length>8192?(raw??"").Substring(0,8192):raw??"","");
            foreach(string original in plain.Replace('\r','\n').Split('\n'))
            {
                string line=TextPrefix.Replace(Prefix.Replace(original,"").Trim(),"");
                var matches=Rows.Matches(line);
                for(int i=0;i<matches.Count;i++)
                {
                    var match=matches[i];int start=match.Index+match.Length;
                    int end=i+1<matches.Count?matches[i+1].Index:line.Length;
                    string label=line.Substring(start,end-start).Trim();
                    if(label.Length==0||Regex.IsMatch(label,@"^[+-]?[0-9]+(?:[.,][0-9]+)?(?:\s|$)"))continue;
                    string command=match.Groups["cmd"].Success?match.Groups["cmd"].Value:prefix+match.Groups["num"].Value;
                    Add(command,label);
                }
                // Footer commands are explicit advertisements. Never invent /0 for a toggle-close menu.
                foreach(Match match in Navigation.Matches(line))
                {
                    string command=match.Groups["cmd"].Value;
                    Add(command,command.Substring(1).Equals("back",StringComparison.OrdinalIgnoreCase)?"Back":"Close menu");
                }
                foreach(Match match in CloseCommand.Matches(line))
                {
                    string command=match.Groups["cmd"].Value;
                    if(registered.Contains(command))Add(command,"Close menu");
                }
            }
        }
        return result;
    }
}
