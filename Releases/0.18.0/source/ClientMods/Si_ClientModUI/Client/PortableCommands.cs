using System.Text.Json;
using System.Security.Cryptography;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    public sealed class PortableCommand
    {
        public string Mod {get;set;}="";
        public string Command {get;set;}="";
        public string Description {get;set;}="";
        public string Role {get;set;}="shared";
        public bool AdminOnly {get;set;}
        public bool Debug {get;set;}
        public string Group {get;set;}="";
    }
    public sealed class PortableCommandFile
    {
        public int Schema {get;set;}=1;
        public List<PortableCommand> Commands {get;set;}=new();
    }
    private readonly List<CommandEntry> portableCommands=new();
    private bool portableReadFailed;
    private static string PortableCommandsPath=>Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory,"Si_ModMenu","created-commands.json");
    private void LoadPortableCommands()
    {
        portableCommands.Clear();portableReadFailed=false;
        if(!File.Exists(PortableCommandsPath)){InvalidateUi();return;}
        try
        {
            var data=JsonSerializer.Deserialize<PortableCommandFile>(File.ReadAllText(PortableCommandsPath));
            if(data?.Schema!=1)throw new InvalidDataException("Unknown shared-command JSON schema.");
            foreach(var c in (data.Commands??new()).Take(1000))
                if(c!=null&&ValidChat(c.Command)&&!string.IsNullOrWhiteSpace(c.Mod)&&new[]{"player","commander","shared","admin"}.Contains(c.Role))
                    portableCommands.Add(new CommandEntry{Mod=ModLabel(c.Mod),Command=c.Command,Description=c.Description??"",Role=c.Role,
                        AdminOnly=c.AdminOnly,Debug=c.Debug,Group=c.Group??"",Scope="portable",Source="imported"});
        }
        catch(Exception e){portableReadFailed=true;controllerMessage="Shared commands were not imported: "+e.GetBaseException().Message;Error(e,"shared command JSON");}
        InvalidateUi();
    }
    private void ExportCreatedCommands()
    {
        if(portableReadFailed)return;
        var entries=savedCommands.Where(c=>c.Source=="saved").Concat(portableCommands).Where(c=>!CustomCommandRemoved(c)).Select(c=>new PortableCommand{
            Mod=ModLabel(c.Mod),Command=c.Command,Description=c.Description,Role=c.Role,AdminOnly=c.AdminOnly,Debug=c.Debug,Group=c.Group}).ToList();
        foreach(var binding in layout.SidebarButtons.Where(b=>b!=null&&b.Kind=="chat"&&ValidChat(b.Command)&&!layout.RemovedCustomCommands.Contains(CustomCommandKey(b.Mod,b.Command,b.Role))))
            entries.Add(new PortableCommand{Mod=ModLabel(binding.Mod),Command=binding.Command,Description=binding.Label,Role=binding.Role,AdminOnly=binding.AdminOnly});
        var created=entries.GroupBy(c=>c.Mod+"\n"+c.Command+"\n"+c.Role,StringComparer.OrdinalIgnoreCase).Select(g=>g.First())
            .OrderBy(c=>c.Mod).ThenBy(c=>c.Command).ThenBy(c=>c.Role).ToList();
        if(created.Count==0&&!File.Exists(PortableCommandsPath))return; // Existing exports must be cleared after deletion.
        try
        {
            string json=JsonSerializer.Serialize(new PortableCommandFile{Commands=created},new JsonSerializerOptions{WriteIndented=true});
            if(File.Exists(PortableCommandsPath)&&File.ReadAllText(PortableCommandsPath)==json)return;
            WriteVerifiedJson(PortableCommandsPath,json);
        }
        catch(Exception e){controllerMessage="Shared command export failed: "+e.GetBaseException().Message;Error(e,"shared command export");}
    }
    private static void WriteVerifiedJson(string path,string json)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        if(File.Exists(path))
        {
            string backup=path+"."+DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ")+".bak";
            File.Copy(path,backup,false);
            if(!SHA256.HashData(File.ReadAllBytes(path)).SequenceEqual(SHA256.HashData(File.ReadAllBytes(backup))))
                throw new IOException("JSON backup verification failed.");
        }
        string temp=path+".tmp";File.WriteAllText(temp,json);
        if(File.ReadAllText(temp)!=json)throw new IOException("JSON write verification failed.");
        File.Move(temp,path,true);
    }
}

