using System.Text;
using System.Text.Json;

namespace SiSquadHud;
internal static class FriendPanelChecks
{
    internal static void Run(Action<bool,string> check)
    {
        JsonElement Data(string json)=>JsonDocument.Parse(json).RootElement.Clone();
        void Reject(Action action,string label){bool rejected=false;try{action();}catch(InvalidDataException){rejected=true;}check(rejected,label);}
        check(Protocol.Marker=="SiModUI/2"&&Protocol.MaxDocumentBytes==4*1024*1024,"Friend marker and bounded four MiB document envelope");
        var command=new ServerCommand{Text="/flex",Value="sol,alien",Multi=true,Choices=new(){new[]{"sol","Sol"},new[]{"cent","Centauri"},new[]{"alien","Alien"}}};
        check(ServerPanelData.ChoiceSelected(command,"sol")&&ServerPanelData.ChoiceSelected(command,"ALIEN")&&!ServerPanelData.ChoiceSelected(command,"cent"),"Multi-choice highlights complete set case-insensitively");
        command.Value="sol alien";check(ServerPanelData.ChoiceSelected(command,"alien"),"Legacy space-delimited personal choice set");
        command.Multi=false;check(!ServerPanelData.ChoiceSelected(command,"sol"),"Single choice never treats a set as one active token");
        string spawn="{\"cmd\":\"!spawn\",\"admin\":true,\"all\":true,\"credits\":false,\"enabled\":false,\"offText\":\"Spawning off\",\"catalog\":{\"Sol\":[{\"c\":\"Units\",\"u\":[[\"Sol_Infantry_Prefab\",0,\"Rifleman\"]]}],\"Wildlife\":[{\"c\":\"Creatures\",\"u\":[[\"Wild_Beast\",0,\"Beast\"]]}]}}";
        var panel=ServerPanelData.Parse(Data(spawn));
        var roots=new[]{new ServerCommand{Text="!spawn",Admin=true}};
        check(panel.CatalogVisible(true,true,roots)&&!panel.CatalogVisible(false,true,roots)&&!panel.CatalogVisible(true,false,roots),"Spawn catalog restricted to authenticated ADMIN view");
        check(!panel.CatalogVisible(true,true,Array.Empty<ServerCommand>()),"Catalog unavailable when server did not publish required root");
        check(panel.AllFactions&&!panel.Credits&&!panel.Enabled&&panel.OffText=="Spawning off","Spawn faction, currency and disabled metadata retained");
        var item=panel.Factions["sol"][0].Items[0];
        check(item.Label=="Rifleman"&&item.Token=="Sol_Infantry_Prefab"&&panel.Factions.ContainsKey("wildlife"),"Friendly catalog labels never replace command tokens");
        var buy=ServerPanelData.Parse(Data("{\"cmd\":\"/buy\",\"catalog\":{\"Alien\":[{\"c\":\"Units\",\"u\":[[\"Shrimp\",125]]}]}}"));
        check(buy.Enabled&&buy.Credits&&!buy.AllFactions&&!buy.CatalogAdmin&&buy.Factions["alien"][0].Items[0].Label=="Shrimp","Legacy Buy defaults and two-column catalog rows remain supported");
        check(buy.CatalogVisible(false,false,new[]{new ServerCommand{Text="/buy"}}),"Player purchase catalog available to ordinary player");
        var table=ServerPanelData.Parse(Data("{\"info\":[\"Ready\"],\"tables\":[{\"n\":\"Commander\",\"h\":[\"Faction\",\"Rating\"],\"r\":[[\"Sol\",1234],[\"Alien\",\"1180\"]]}]}"));
        check(table.Info[0]=="Ready"&&table.Tables[0].Rows[0][1]=="1234"&&table.Tables[0].Headers.Count==2,"Personal tables retain titles, headers, rows and scalar cells");
        var module=new ServerModule{Id="Friend",Name="Friend",Version="1.5.0",Hash=Protocol.ShortHash("friend"),Commands=new(){command},Data=Data(spawn)};
        var legacy=new ServerFrame{Full=true,Root=Protocol.ShortHash(module.Id+":"+module.Hash),Modules=new(){module}};
        string json=JsonSerializer.Serialize(legacy);
        var catalog=new CompanionCatalog();catalog.Apply(JsonSerializer.Deserialize<ServerFrame>(json));
        var restored=JsonSerializer.Deserialize<ServerFrame>(catalog.Snapshot());
        check(restored.Version==0&&restored.Modules[0].Version=="1.5.0"&&restored.Modules[0].Data.HasValue,"Legacy unversioned frames and module versions survive cache");
        module.Commands[0].Multi=true;
        catalog.Apply(legacy);check(JsonSerializer.Deserialize<ServerFrame>(catalog.Snapshot()).Modules[0].Commands[0].Multi,"Multi-select bit survives serialized frame cache");
        string before=catalog.Snapshot();
        var invalidLegacy=JsonSerializer.Deserialize<ServerFrame>(before);
        invalidLegacy.Modules[0].Data=Data("{\"tables\":[{\"h\":[\"A\"],\"r\":[\"bad row\"]}]}");
        Reject(()=>catalog.Apply(invalidLegacy),"Malformed table rejects whole frame");check(catalog.Snapshot()==before,"Rejected table leaves active frame intact");
        Reject(()=>ServerPanelData.Parse(Data("{\"cmd\":\"!spawn\",\"catalog\":{\"Sol\":[{\"c\":\"Units\",\"u\":[[\"bad\\n!kick\",0]]}]}}")),"Catalog command injection rejected");
        Reject(()=>ServerPanelData.Parse(Data(JsonSerializer.Serialize(new{tables=Enumerable.Range(0,33).Select(_=>new{n="x"})}))),"Table count bounded");
        Reject(()=>ServerPanelData.Parse(Data(JsonSerializer.Serialize(new{tables=new[]{new{r=Enumerable.Range(0,513).Select(_=>new[]{"x"})}}}))),"Table row count bounded");
        Reject(()=>ServerPanelData.Parse(Data(JsonSerializer.Serialize(new{tables=new[]{new{h=Enumerable.Repeat("x",33)}}}))),"Table column count bounded");
        var packets=Protocol.Transfer(Protocol.NewNonce(),JsonSerializer.Serialize(new{k="frame",full=true,root="abcdef123456",mods=new[]{new{data=spawn}},removed=Array.Empty<string>()}));
        check(packets.All(p=>p.Length<=Protocol.MaxBody),"Friend panel payload stays inside packet chunk bounds");
    }
}
