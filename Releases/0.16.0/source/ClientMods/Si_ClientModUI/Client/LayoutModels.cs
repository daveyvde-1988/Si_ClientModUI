namespace SiSquadHud;
public sealed partial class SquadHud
{
    public sealed class LocalWindow
    {
        public float X {get;set;}=.5f;
        public float Y {get;set;}=.5f;
        public float Width {get;set;}
        public float Height {get;set;}
    }
    public sealed class LocalLayout
    {
        public Dictionary<string,LocalWindow> Windows {get;set;}=new();
        public HashSet<string> SelectedStatus {get;set;}=new();
        public HashSet<string> ExpandedStatusGroups {get;set;}=new();
        public bool Notifications {get;set;}
        public bool SidebarEnabled {get;set;}
        public bool ClientDebug {get;set;}
        public bool HoldSidebarF7 {get;set;}=true;
        public Dictionary<string,float> FeedbackSplit {get;set;}=new();
        public List<SidebarBinding> SidebarButtons {get;set;}=new();
        public float Transparency {get;set;}=6;
        public float TextSize {get;set;}=100;
    }
    public sealed class SidebarBinding
    {
        public string Id {get;set;}=Guid.NewGuid().ToString("N");
        public string Kind {get;set;}="chat";
        public string Mod {get;set;}="Custom";
        public string Label {get;set;}="";
        public string Command {get;set;}="";
        public string Provider {get;set;}="";
        public string Symbol {get;set;}="shape:circle-filled";
        public string Color {get;set;}="#FFFFFF";
        public string Role {get;set;}="shared";
        public string TeamScope {get;set;}="";
        public bool AdminOnly {get;set;}
        public bool Enabled {get;set;}=true;
    }
}
