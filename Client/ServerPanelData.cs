using System.Text.Json;

namespace SiSquadHud;

// The friend's legacy panel contract, independent of the shell and native GUI.
internal sealed class ServerPanelData
{
    internal sealed class Table
    {
        internal string Name="";
        internal List<string> Headers=new();
        internal List<string[]> Rows=new();
    }
    internal sealed class Item {internal string Token,Label;internal int Cost;}
    internal sealed class Category {internal string Name;internal List<Item> Items=new();}
    internal bool Enabled=true,CatalogAdmin,AllFactions,Credits=true;
    internal string Command="",OffText="This feature is switched off on the server.";
    internal List<string> Info=new();
    internal List<Table> Tables=new();
    internal Dictionary<string,List<Category>> Factions=new(StringComparer.OrdinalIgnoreCase);
    internal bool HasCatalog=>Factions.Count>0;

    internal static bool ChoiceSelected(ServerCommand command,string token)=>command.Multi?
        (command.Value??"").Split(new[]{',',' '},StringSplitOptions.RemoveEmptyEntries).Contains(token,StringComparer.OrdinalIgnoreCase):
        string.Equals(command.Value,token,StringComparison.OrdinalIgnoreCase);

    internal bool CatalogVisible(bool adminView,bool admin,IEnumerable<ServerCommand> commands)=>
        HasCatalog&&CatalogAdmin==adminView&&(!CatalogAdmin||admin)&&commands.Any(c=>
            (c.Text.Equals(Command,StringComparison.OrdinalIgnoreCase)||c.Aliases.Contains(Command,StringComparer.OrdinalIgnoreCase))&&(!c.Admin||admin));

    internal static ServerPanelData Parse(JsonElement? source)
    {
        var result=new ServerPanelData();
        if(!source.HasValue)return result;
        var data=source.Value;
        if(data.ValueKind!=JsonValueKind.Object)throw new InvalidDataException("Invalid panel document.");
        string Text(JsonElement value,int max=2048)
        {
            if(value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)throw new InvalidDataException("Invalid panel text.");
            string text=value.ValueKind==JsonValueKind.String?value.GetString()??"":value.ToString();
            if(text.Length>max)throw new InvalidDataException("Panel text too long.");
            return text;
        }
        bool Flag(string name,bool fallback)=>data.TryGetProperty(name,out var value)&&value.ValueKind is JsonValueKind.True or JsonValueKind.False?value.GetBoolean():fallback;
        result.Enabled=Flag("enabled",true);result.CatalogAdmin=Flag("admin",false);
        result.AllFactions=Flag("all",false);result.Credits=Flag("credits",true);
        if(data.TryGetProperty("cmd",out var cmd))result.Command=Text(cmd,160);
        if(data.TryGetProperty("offText",out var off))result.OffText=Text(off);
        if(data.TryGetProperty("info",out var info))
        {
            if(info.ValueKind!=JsonValueKind.Array||info.GetArrayLength()>256)throw new InvalidDataException("Invalid panel information.");
            foreach(var line in info.EnumerateArray())result.Info.Add(Text(line));
        }
        int cells=0;
        if(data.TryGetProperty("tables",out var tables))
        {
            if(tables.ValueKind!=JsonValueKind.Array||tables.GetArrayLength()>32)throw new InvalidDataException("Too many tables.");
            foreach(var entry in tables.EnumerateArray())
            {
                if(entry.ValueKind!=JsonValueKind.Object)throw new InvalidDataException("Invalid table.");
                var table=new Table();
                if(entry.TryGetProperty("n",out var name))table.Name=Text(name,256);
                if(entry.TryGetProperty("h",out var headers))
                {
                    if(headers.ValueKind!=JsonValueKind.Array||headers.GetArrayLength()>32)throw new InvalidDataException("Invalid table columns.");
                    foreach(var header in headers.EnumerateArray())table.Headers.Add(Text(header,256));
                }
                if(entry.TryGetProperty("r",out var rows))
                {
                    if(rows.ValueKind!=JsonValueKind.Array||rows.GetArrayLength()>512)throw new InvalidDataException("Too many table rows.");
                    foreach(var row in rows.EnumerateArray())
                    {
                        if(row.ValueKind!=JsonValueKind.Array||row.GetArrayLength()>32||(cells+=row.GetArrayLength())>32768)throw new InvalidDataException("Invalid table cells.");
                        table.Rows.Add(row.EnumerateArray().Select(v=>Text(v)).ToArray());
                    }
                }
                result.Tables.Add(table);
            }
        }
        int items=0;
        if(data.TryGetProperty("catalog",out var catalog))
        {
            if(catalog.ValueKind!=JsonValueKind.Object||!Protocol.ValidCommandText(result.Command))throw new InvalidDataException("Invalid catalog command.");
            foreach(var faction in catalog.EnumerateObject())
            {
                if(result.Factions.Count>=32||faction.Name.Length>160||faction.Value.ValueKind!=JsonValueKind.Array||faction.Value.GetArrayLength()>128)throw new InvalidDataException("Invalid catalog faction.");
                var categories=new List<Category>();
                foreach(var category in faction.Value.EnumerateArray())
                {
                    if(category.ValueKind!=JsonValueKind.Object||!category.TryGetProperty("c",out var label)||!category.TryGetProperty("u",out var units)||units.ValueKind!=JsonValueKind.Array)throw new InvalidDataException("Invalid catalog category.");
                    var group=new Category{Name=Text(label,256)};
                    foreach(var unit in units.EnumerateArray())
                    {
                        if(++items>8192||unit.ValueKind!=JsonValueKind.Array||unit.GetArrayLength()<2||unit.GetArrayLength()>3||unit[0].ValueKind!=JsonValueKind.String||!unit[1].TryGetInt32(out int cost))throw new InvalidDataException("Invalid catalog item.");
                        string token=Text(unit[0],160);
                        if(!Protocol.ValidCommandText(result.Command+" "+token))throw new InvalidDataException("Invalid catalog action.");
                        group.Items.Add(new Item{Token=token,Cost=cost,Label=unit.GetArrayLength()==3?Text(unit[2],256):token});
                    }
                    categories.Add(group);
                }
                result.Factions.Add(faction.Name,categories);
            }
        }
        return result;
    }
}
