namespace SiSquadHud;
public sealed partial class SquadHud
{
    public sealed class CommandEntry
    {
        public string Mod {get;set;}="";
        public string Command {get;set;}="";
        public string Description {get;set;}="";
        public string Role {get;set;}="player";
        public bool AdminOnly {get;set;}
        public bool Debug {get;set;}
        public string Scope {get;set;}="";
        public string Source {get;set;}="saved";
        public string Group {get;set;}="";
        public string Label {get;set;}="";
        public int MenuOrder {get;set;}
        public bool RequiresInput {get;set;}
    }
}
