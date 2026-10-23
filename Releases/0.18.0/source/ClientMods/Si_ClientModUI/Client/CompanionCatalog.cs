using System.Text.Json;
using System.Text.Json.Serialization;

namespace SiSquadHud;

// Wire models are separate from layout, providers and user-created command files.
internal sealed class ServerCommand
{
    [JsonPropertyName("v")] public string Value {get;set;}
    [JsonPropertyName("m")] public bool Multi {get;set;}
    [JsonPropertyName("o")] public int Order {get;set;}=1000;
    [JsonPropertyName("role")] public string Role {get;set;}="shared";
    [JsonPropertyName("ui")] public ControlPresentation Ui {get;set;}
    [JsonPropertyName("t")] public string Text {get;set;}="";
    [JsonPropertyName("l")] public string Label {get;set;}="";
    [JsonPropertyName("d")] public string Description {get;set;}="";
    [JsonPropertyName("a")] public bool Admin {get;set;}
    [JsonPropertyName("p")] public string Power {get;set;}="";
    [JsonPropertyName("x")] public List<string> Aliases {get;set;}=new();
    [JsonPropertyName("arg")] public string Argument {get;set;}="";
    [JsonPropertyName("ch")] public List<string[]> Choices {get;set;}=new();
}
internal sealed class ServerModule
{
    [JsonIgnore] internal ServerPanelData Panel;
    [JsonPropertyName("data")] public JsonElement? Data {get;set;}
    [JsonPropertyName("cat")] public string Category {get;set;}="player";
    [JsonPropertyName("id")] public string Id {get;set;}="";
    [JsonPropertyName("name")] public string Name {get;set;}="";
    [JsonPropertyName("ver")] public string Version {get;set;}="";
    [JsonPropertyName("hash")] public string Hash {get;set;}="";
    [JsonPropertyName("nav")] public string Navigation {get;set;}="/";
    [JsonPropertyName("prio")] public int Priority {get;set;}
    [JsonPropertyName("cmds")] public List<ServerCommand> Commands {get;set;}=new();
}
internal sealed class ServerFrame
{
    [JsonPropertyName("v")] public int Version {get;set;}
    [JsonPropertyName("caps")] public string[] Capabilities {get;set;}=Array.Empty<string>();
    [JsonPropertyName("root")] public string Root {get;set;}="";
    [JsonPropertyName("same")] public bool Same {get;set;}
    [JsonPropertyName("full")] public bool Full {get;set;}
    [JsonPropertyName("mods")] public List<ServerModule> Modules {get;set;}=new();
    [JsonPropertyName("removed")] public List<string> Removed {get;set;}=new();
}
internal sealed class CompanionCatalog
{
    internal string Root {get;private set;}="";
    internal IReadOnlyDictionary<string,ServerModule> Modules=>modules;
    private Dictionary<string,ServerModule> modules=new(StringComparer.Ordinal);
    internal void Clear(){Root="";modules.Clear();}
    internal void Apply(ServerFrame doc)
    {
        if(doc==null||!Protocol.ValidHash(doc.Root))throw new InvalidDataException("Invalid frame hash.");
        if(doc.Same)
        {
            if(doc.Root!=Root)throw new InvalidDataException("Server confirmed an unknown catalog.");
            return;
        }
        if(doc.Modules==null||doc.Removed==null||doc.Modules.Count>256||doc.Removed.Count>256)
            throw new InvalidDataException("Invalid frame size.");
        var next=doc.Full?new Dictionary<string,ServerModule>(StringComparer.Ordinal):new(modules,StringComparer.Ordinal);
        foreach(string id in doc.Removed)if(Protocol.ValidId(id))next.Remove(id);else throw new InvalidDataException("Invalid removed module.");
        foreach(var mod in doc.Modules)
        {
            if(mod==null||!Protocol.ValidId(mod.Id)||!Protocol.ValidHash(mod.Hash)||string.IsNullOrWhiteSpace(mod.Name)||mod.Name.Length>160||
                mod.Navigation is not ("/" or "!" or ".")||mod.Commands==null||mod.Commands.Count>2048)
                throw new InvalidDataException("Invalid server module.");
            foreach(var cmd in mod.Commands)
            {
                if(cmd==null||!Protocol.ValidCommandText(cmd.Text)||cmd.Label==null||cmd.Label.Length>256||cmd.Description==null||cmd.Description.Length>8192||cmd.Argument==null||cmd.Argument.Length>32||
                    cmd.Aliases==null||cmd.Aliases.Count>128||cmd.Aliases.Any(a=>!Protocol.ValidCommandText(a))||cmd.Choices==null||cmd.Choices.Count>2048||
                    cmd.Choices.Any(c=>c==null||(c.Length<2||c.Length>3)||string.IsNullOrWhiteSpace(c[0])||!Protocol.ValidCommandText(cmd.Text+" "+c[0])||c[1]==null||c[1].Length>256))
                    throw new InvalidDataException("Invalid server command.");
                if(cmd.Value?.Length>2048||cmd.Role is not ("shared" or "player" or "commander" or "admin")||cmd.Ui!=null&&!cmd.Ui.Valid())
                    throw new InvalidDataException("Invalid control metadata.");
            }
            if(mod.Data.HasValue && (mod.Data.Value.ValueKind!=JsonValueKind.Object||mod.Data.Value.GetRawText().Length>3*1024*1024))
                throw new InvalidDataException("Invalid module data.");
            mod.Panel=ServerPanelData.Parse(mod.Data); // Validate known panel shapes before committing any part of a frame.
            next[mod.Id]=mod;
        }
        if(next.Count>256||next.Values.Sum(m=>m.Commands.Sum(c=>1+c.Aliases.Count+c.Choices.Count))>8192)throw new InvalidDataException("Catalog too large.");
        string root=Protocol.ShortHash(string.Join("\n",next.Values.OrderBy(m=>m.Id,StringComparer.Ordinal).Select(m=>m.Id+":"+m.Hash)));
        if(root!=doc.Root)throw new InvalidDataException("Incomplete frame diff.");
        modules=next;Root=doc.Root; // commit only after the entire diff validates
    }
    internal string Snapshot()=>JsonSerializer.Serialize(new ServerFrame{Full=true,Root=Root,Modules=modules.Values.ToList()});
}
