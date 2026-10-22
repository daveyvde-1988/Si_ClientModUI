using System.Collections.Generic;

namespace SiSquadHud
{
    public sealed class AdminMenuRow
    {
        public string Id {get;set;}="";
        public string Label {get;set;}="";
        public bool Input {get;set;}
        public bool Enabled {get;set;}=true;
        public string Hint {get;set;}="";
    }
    public sealed class AdminMenuPage
    {
        public string Kind {get;set;}="admin-menu";
        public string Request {get;set;}="";
        public string Service {get;set;}="";
        public string Revision {get;set;}="";
        public string Title {get;set;}="";
        public string Message {get;set;}="";
        public bool CanBack {get;set;}
        public bool Confirmation {get;set;}
        public List<string> Lines {get;set;}=new List<string>();
        public List<AdminMenuRow> Rows {get;set;}=new List<AdminMenuRow>();
    }
}
