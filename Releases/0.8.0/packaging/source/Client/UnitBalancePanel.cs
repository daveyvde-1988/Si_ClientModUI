using System.Globalization;
using System.Text.Json;
using Il2Cpp;
using UnityEngine;

namespace SiSquadHud;

public sealed partial class SquadHud
{
    private sealed class BalanceField
    {
        public BalanceField() { }
        public int Id {get;set;}
        public string[] Path {get;set;}=Array.Empty<string>();
        public string Entity {get;set;}="";
        public string Group {get;set;}="";
        public string Name {get;set;}="";
        public string Type {get;set;}="";
        public string Baseline {get;set;}="";
        public bool Editable {get;set;}
    }
    private sealed class BalanceView
    {
        public BalanceView() { }
        public bool Available {get;set;}
        public bool Enabled {get;set;}
        public bool Admin {get;set;}
        public bool Pending {get;set;}
        public long Generation {get;set;}
        public long Serial {get;set;}
        public string Revision {get;set;}="";
        public string State {get;set;}="";
        public JsonElement Config {get;set;}
        public JsonElement Saved {get;set;}
        public JsonElement Defaults {get;set;}
        public List<BalanceField> Fields {get;set;}=new();
    }
    private sealed class BalanceEdit
    {
        public int Field {get;set;}
        public bool Remove {get;set;}
        public object Value {get;set;}
    }
    private BalanceView balanceView;
    private string balanceNonce="",balanceTransfer="",balanceHash="",balanceEntity="Global Settings",balanceSearch="",balanceMessage="",balanceEditRevision="";
    private long menuSequence=-1;
    private int menuMask=7,balanceChunkCount;
    private bool menuStateKnown,balanceAwaitSnapshot,balanceSaveConfirm,balanceReloadConfirm;
    private readonly Dictionary<int,byte[]> balanceParts=new();
    private readonly Dictionary<int,string> balanceInputs=new();
    private readonly Dictionary<int,BalanceEdit> balanceEdits=new();
    private readonly HashSet<string> balanceExpanded=new(){"Structures (Shared)"};
    private readonly Queue<BalanceEdit> balanceUpload=new();
    private BalanceCommand balancePending;
    private string balanceDraftId="",balanceWire="";
    private int balanceUploadCount;
    private double balanceSent,balanceTransferAt,balanceLastReceived,balanceNextRefresh;
    private bool balanceRetried;
    private Vector2 balanceTreeScroll,balanceFieldScroll;
    private bool BalanceBusy=>balancePending!=null||balanceUpload.Count>0||balanceUploadCount>0||balanceAwaitSnapshot;
    private bool BalanceSection=>menuTab=="unitbalance"||AdminMenuSection;
    private bool TabVisible(string key)
    {
        if(!menuStateKnown)return key!="unitbalance";
        int bit=key=="formations"?1:key=="buy"?2:key=="ranks"?4:key=="unitbalance"?8:0;
        return bit==0||(menuMask&bit)!=0;
    }
    private void ClearBalanceEdits()
    {
        balanceEdits.Clear();balanceInputs.Clear();balanceUpload.Clear();balancePending=null;balanceUploadCount=0;
        balanceSaveConfirm=balanceReloadConfirm=false;balanceEditRevision="";
        ResetAdminMenu();
    }
    private void SyncBalanceContext()
    {
        if(balanceNonce==cache.Nonce)return;
        balanceNonce=cache.Nonce;balanceView=null;menuStateKnown=false;serverAdmin=false;menuMask=7;menuSequence=-1;
        balanceParts.Clear();balanceTransfer=balanceHash="";balanceChunkCount=0;balanceAwaitSnapshot=false;
        balanceMessage="Waiting for the server balance snapshot...";balanceNextRefresh=clock.Elapsed.TotalSeconds+15;balanceLastReceived=0;
        ClearBalanceEdits();
    }
    private bool ReceiveBalanceBody(string body)
    {
        SyncBalanceContext();
        if(HudProtocol.TryMenuState(body,out var nonce,out long seq,out int mask,out bool admin))
        {
            if(nonce!=cache.Nonce||seq<=menuSequence)return true;
            menuSequence=seq;menuStateKnown=true;menuMask=mask;serverAdmin=admin;
            if(!admin){ClearBalanceEdits();if(balanceView?.Admin==true)balanceView=null;}return true;
        }
        if(HudProtocol.TryBalanceReply(body,out nonce,out var id,out bool ok,out var revision,out var message))
        {
            if(nonce!=cache.Nonce||balancePending==null||id!=balancePending.Id)return true;
            string operation=balancePending.Operation;balancePending=null;balanceMessage=message;
            if(operation=="menu")adminMenuMessage=message;
            if(!ok){balanceUpload.Clear();balanceUploadCount=0;balanceSaveConfirm=balanceReloadConfirm=false;return true;}
            if(operation=="save")
            {ClearBalanceEdits();balanceAwaitSnapshot=true;balanceNextRefresh=clock.Elapsed.TotalSeconds+15;}
            else if(operation=="reload"||operation=="refresh")
            {balanceAwaitSnapshot=true;balanceNextRefresh=clock.Elapsed.TotalSeconds+15;}
            return true;
        }
        if(!HudProtocol.TryBalanceChunk(body,out var chunk))return false;
        if(chunk.Nonce!=cache.Nonce)return true;
        if(chunk.Id!=balanceTransfer)
        {balanceParts.Clear();balanceTransfer=chunk.Id;balanceHash=chunk.Hash;balanceChunkCount=chunk.Count;balanceTransferAt=clock.Elapsed.TotalSeconds;}
        if(chunk.Hash!=balanceHash||chunk.Count!=balanceChunkCount){balanceParts.Clear();return true;}
        balanceParts[chunk.Index]=chunk.Bytes;
        balanceTransferAt=clock.Elapsed.TotalSeconds;
        if(balanceParts.Count<balanceChunkCount)return true;
        try
        {
            byte[] bytes;
            using(var output=new MemoryStream())
            {for(int i=0;i<balanceChunkCount;i++){var part=balanceParts[i];output.Write(part,0,part.Length);}bytes=output.ToArray();}
            string json=HudProtocol.DecodeBalance(bytes,balanceHash);
            if(ReceiveAdminMenu(json))return true;
            var snapshot=JsonSerializer.Deserialize<BalanceView>(json,new JsonSerializerOptions{PropertyNameCaseInsensitive=true,MaxDepth=32});
            if(snapshot==null||snapshot.Fields.Count>10000||snapshot.Fields.Any(f=>f.Path.Length<1||f.Path.Length>9)||snapshot.Fields.Select(f=>f.Id).Distinct().Count()!=snapshot.Fields.Count)
                throw new InvalidDataException("Invalid balance snapshot.");
            if(balanceView!=null&&snapshot.Serial<balanceView.Serial&&snapshot.Available)return true;
            if(balanceEdits.Count>0&&balanceEditRevision!=snapshot.Revision)
            {ClearBalanceEdits();balanceMessage="Server JSON or reload state changed. Unsaved edits were discarded; review the new snapshot.";}
            balanceView=snapshot;balanceAwaitSnapshot=false;balanceLastReceived=clock.Elapsed.TotalSeconds;balanceNextRefresh=balanceLastReceived+30;
            if(balanceMessage.StartsWith("Waiting for",StringComparison.Ordinal)||balanceMessage.StartsWith("Refreshing",StringComparison.Ordinal))balanceMessage="Server snapshot cached. Hover over controls for details.";
            if(!serverAdmin)ClearBalanceEdits();
            Write("UNITBALANCE cached fields="+snapshot.Fields.Count+"; generation="+snapshot.Generation+"; pending="+snapshot.Pending+"; enabled="+snapshot.Enabled);
        }
        catch(Exception error){balanceMessage="Balance snapshot could not be read. Refresh to retry.";Error(error);}
        finally{balanceParts.Clear();balanceTransfer="";}
        return true;
    }
    private void SendBalance(string operation,string payload="",string draft=null)
    {
        if(balancePending!=null||!networkEnabled)return;
        var player=Player.CurrentPlayer;if(player?.Team==null)return;
        balancePending=new BalanceCommand{Nonce=cache.Nonce,Id=Guid.NewGuid().ToString("N"),Channel=player.PlayerChannel,Team=player.Team.Index,
            Commander=player.IsCommander,Operation=operation,Revision=operation=="menu"?adminMenu?.Revision??new string('0',64):operation=="stage"||operation=="save"?balanceEditRevision:balanceView?.Revision??new string('0',64),Draft=draft??Guid.NewGuid().ToString("N"),Payload=payload};
        try
        {balanceWire=HudProtocol.BalanceControl(balancePending);SendBody(balanceWire);balanceSent=clock.Elapsed.TotalSeconds;balanceRetried=false;}
        catch(Exception error){balancePending=null;balanceUpload.Clear();balanceUploadCount=0;balanceMessage="Request could not be sent.";Error(error);}
    }
    private void RefreshBalance()
    {if(!BalanceBusy){SendBalance("refresh");balanceMessage="Refreshing the server snapshot...";}}
    private void MaintainBalance(double now)
    {
        SyncBalanceContext();
        if(panelOpen&&(!TabVisible(menuTab)||AdminMenuSection&&!serverAdmin))
        {
            EndSidebarDrag();ClearBalanceEdits();pendingId="";controlReadyAt=0;
            ChangeSection("help",panelCommander?"help-commander":"help-fps");
        }
        if(balancePending!=null)
        {
            var player=Player.CurrentPlayer;
            if(player?.Team==null||player.Team.Index!=balancePending.Team||player.IsCommander!=balancePending.Commander){ClearBalanceEdits();return;}
            if(!balanceRetried&&now-balanceSent>2){balanceRetried=true;SendBody(balanceWire);}
            if(now-balanceSent>8)
            {balancePending=null;balanceUpload.Clear();balanceUploadCount=0;balanceMessage=adminMenuMessage="Request timed out. Refresh before retrying; no success assumed.";}
        }
        else if(balanceUploadCount>0)
        {
            if(!panelOpen||menuTab!="balance-admin"||!serverAdmin){ClearBalanceEdits();return;}
            if(balanceUpload.Count>0)
            {var edit=balanceUpload.Dequeue();SendBalance("stage",JsonSerializer.Serialize(edit,new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase}),balanceDraftId);}
            else{int count=balanceUploadCount;balanceUploadCount=0;SendBalance("save",count.ToString(CultureInfo.InvariantCulture),balanceDraftId);}
        }
        if(now>=balanceNextRefresh&&((balanceView==null||balanceAwaitSnapshot)&&balanceParts.Count==0||balanceParts.Count>0&&now-balanceTransferAt>30)&&balancePending==null)
        {
            balanceParts.Clear();balanceAwaitSnapshot=false;balanceNextRefresh=now+30;SendBalance("refresh");
        }
    }
    private static JsonElement BalanceAt(JsonElement root,IEnumerable<string> parts)
    {foreach(string part in parts){if(root.ValueKind!=JsonValueKind.Object||!root.TryGetProperty(part,out root))return default;}return root;}
    private static string BalanceText(JsonElement value)
        =>value.ValueKind==JsonValueKind.Undefined||value.ValueKind==JsonValueKind.Null?"Default":value.ValueKind==JsonValueKind.String?value.GetString():value.GetRawText();
    private static string BalanceCategory(string entity)
    {
        if(entity=="Global Settings"||entity.StartsWith("_"))return "Global Settings";
        if(new[]{"Sol Headquarters","Cent Headquarters","Refinery","Research Facility","Barracks","Light Factory","Air Factory","Heavy Factory","Ultra Heavy Factory","Silo","Radar Station","Turret","Heavy Turret","Anti-Air Rocket Turret"}.Contains(entity))return "Structures (Shared)";
        if(new[]{"Nest","Node","Bio Cache","Lesser Spawning Cyst","Greater Spawning Cyst","Grand Spawning Cyst","Colossal Spawning Cyst","Quantum Cortex","Hive Spire","Thorn Spire"}.Contains(entity))return "Alien Structures";
        if(new[]{"Crab","Shrimp","Shocker","Wasp","Dragonfly","Squid","Horned Crab","Hunter","Behemoth","Scorpion","Firebug","Goliath","Defiler","Colossus","Queen"}.Contains(entity))return "Alien Units";
        if(new[]{"Militia","Trooper","Marksman","Juggernaut","Templar","Light Raider","Squad Transport","Heavy Raider","Assault Car","Strike Tank","Flak Car","Combat Tank","Rocket Tank","Heavy Tank","Pyro Tank","Cent Harvester","Crimson Tank","Shuttle","Dreadnought","Interceptor","Freighter"}.Contains(entity))return "Centauri Units";
        if(new[]{"Scout","Rifleman","Sniper","Heavy","Commando","Light Quad","Platoon Hauler","Heavy Quad","Light Striker","Heavy Striker","AA Truck","Hover Tank","Barrage Truck","Railgun Tank","Pulse Truck","Sol Harvester","Siege Tank","Gunship","Dropship","Fighter","Bomber"}.Contains(entity))return "Sol Units";
        return entity=="Hover Bike"?"Shared Vehicles":"Other";
    }
    private void OpenBalanceEditor()
    {
        if(!serverAdmin)return;
        menuTab="balance-admin";function="";pendingId="";confirmPurchase=0;ClearBalanceEdits();balanceFieldScroll=Vector2.zero;
        RefreshBalance();
    }
    private void StageBalanceText(BalanceField field,string text)
    {
        if(balanceEdits.Count==0)balanceEditRevision=balanceView.Revision;
        balanceInputs[field.Id]=text;
        balanceEdits[field.Id]=new BalanceEdit{Field=field.Id};balanceSaveConfirm=balanceReloadConfirm=false;
    }
    private void BeginBalanceSave()
    {
        if(BalanceBusy||balanceView==null||!serverAdmin||balanceEditRevision!=balanceView.Revision)return;
        if(balanceEdits.Count>256){balanceMessage="Save at most 256 fields at a time.";return;}
        var changes=new List<BalanceEdit>();
        foreach(var pair in balanceEdits)
        {
            var edit=pair.Value;var field=balanceView.Fields.First(f=>f.Id==pair.Key);
            if(!edit.Remove)
            {
                string text=balanceInputs[pair.Key];
                if(field.Type=="bool")
                {if(!bool.TryParse(text,out bool b)){balanceMessage="Invalid boolean: "+field.Name;return;}edit.Value=b;}
                else
                {
                    if(!double.TryParse(text,NumberStyles.Float,CultureInfo.InvariantCulture,out double value)||!double.IsFinite(value))
                    {balanceMessage="Invalid number for "+field.Name+". Use a decimal point.";return;}
                    edit.Value=value;
                }
            }
            changes.Add(edit);
        }
        balanceUpload.Clear();foreach(var change in changes)balanceUpload.Enqueue(change);
        balanceUploadCount=changes.Count;balanceDraftId=Guid.NewGuid().ToString("N");balanceSaveConfirm=false;balanceMessage="Validating and saving "+changes.Count+" edits on the server...";
    }
    private void RenderBalance(Rect viewport,bool editing)
    {
        if(editing&&!serverAdmin)return;
        if(balanceView?.Available!=true)
        {GUI.Label(viewport,balanceView?.State??"Waiting for the server balance snapshot...",bodyStyle);return;}
        editing=editing&&balanceView.Admin;
        float treeWidth=Math.Min(252,viewport.width*.30f),gap=14,rightX=viewport.x+treeWidth+gap,rightWidth=viewport.width-treeWidth-gap;
        GUI.Label(new Rect(viewport.x,viewport.y,viewport.width,30),editing?"UNIT BALANCE / ADMIN EDITOR":"UNIT BALANCE / READ ONLY",titleStyle);
        string state=editing?(balanceView.Pending?"Saved JSON has changes awaiting reload.":"Saved JSON matches the last observed reload."):"Published configuration overrides; vanilla base values are shown where available.";
        state+=" "+balanceView.State;
        GUI.Label(new Rect(viewport.x,viewport.y+34,viewport.width,48),state,smallStyle);
        float top=viewport.y+86,bottom=viewport.y+viewport.height;
        Surface(new Rect(viewport.x,top,treeWidth,bottom-top),Color.Lerp(surface,accent,.035f));
        balanceSearch=GUI.TextField(new Rect(viewport.x+6,top+6,treeWidth-12,34),balanceSearch,80,inputStyle);
        if(balanceSearch.Length==0)GUI.Label(new Rect(viewport.x+18,top+12,treeWidth-36,25),"Search units...",smallStyle);
        var entities=balanceView.Fields.Select(f=>f.Entity).Distinct().ToArray();
        var categoryOrder=new[]{"Global Settings","Structures (Shared)","Sol Units","Centauri Units","Shared Vehicles","Alien Structures","Alien Units","Other"};
        var treeRows=new List<(string entity,string label,bool heading)>();
        foreach(string category in categoryOrder)
        {
            var list=entities.Where(e=>BalanceCategory(e)==category).OrderBy(e=>e=="Global Settings"?"":e,StringComparer.OrdinalIgnoreCase)
                .Where(e=>balanceSearch.Length==0||e.Contains(balanceSearch,StringComparison.OrdinalIgnoreCase)||category.Contains(balanceSearch,StringComparison.OrdinalIgnoreCase)).ToArray();
            if(list.Length==0)continue;
            treeRows.Add((category,category,true));
            if(balanceExpanded.Contains(category)||balanceSearch.Length>0||category=="Global Settings")
                foreach(string entity in list)treeRows.Add((entity,entity=="_teleport"?"Teleportation":entity,false));
        }
        var treeView=new Rect(viewport.x+4,top+46,treeWidth-8,Math.Max(40,bottom-top-50));
        float treeContentWidth=treeView.width-22;
        var rowHeights=treeRows.Select(r=>Math.Max(38,smallStyle.CalcHeight(new GUIContent(r.label),treeContentWidth-16)+12)).ToArray();
        balanceTreeScroll=GUI.BeginScrollView(treeView,balanceTreeScroll,new Rect(0,0,treeContentWidth,rowHeights.Sum()),false,true);
        try
        {
            float y=0;for(int i=0;i<treeRows.Count;i++)
            {
                var row=treeRows[i];string label=row.heading?(balanceExpanded.Contains(row.entity)?"- ":"+ ")+row.label:row.label;
                if(Button(new Rect(0,y,treeContentWidth,rowHeights[i]-3),label,!row.heading&&balanceEntity==row.entity,true,"",row.heading?tableHeaderStyle:buttonStyle))
                {
                    if(row.heading){if(!balanceExpanded.Add(row.entity))balanceExpanded.Remove(row.entity);}
                    else{balanceEntity=row.entity;balanceFieldScroll=Vector2.zero;}
                }
                y+=rowHeights[i];
            }
        }
        finally{GUI.EndScrollView();}
        var fields=balanceView.Fields.Where(f=>f.Entity==balanceEntity).OrderBy(f=>f.Group,StringComparer.Ordinal).ThenBy(f=>f.Name,StringComparer.Ordinal).ToArray();
        GUI.Label(new Rect(rightX,top,rightWidth,34),balanceEntity=="_teleport"?"Teleportation":balanceEntity,titleStyle);
        var fieldView=new Rect(rightX,top+40,rightWidth,Math.Max(30,bottom-top-40));float contentWidth=rightWidth-24;
        JsonElement source=editing?balanceView.Saved:balanceView.Config;
        var heights=fields.Select(f=>Math.Max(82,Math.Max(bodyStyle.CalcHeight(new GUIContent(f.Name.Replace('_',' ')),Math.Max(80,contentWidth*.48f)),
            f.Type=="info"?smallStyle.CalcHeight(new GUIContent(BalanceText(BalanceAt(source,f.Path))),Math.Max(40,contentWidth*.49f)):32)+48)).ToArray();
        float total=heights.Sum()+fields.Select(f=>f.Group).Distinct().Count()*36;
        balanceFieldScroll=GUI.BeginScrollView(fieldView,balanceFieldScroll,new Rect(0,0,contentWidth,total),false,true);
        try
        {
            float y=0;string group="";
            for(int i=0;i<fields.Length;i++)
            {
                var field=fields[i];if(group!=field.Group){group=field.Group;Surface(new Rect(0,y,contentWidth,30),Color.Lerp(surface,accent,.12f));GUI.Label(new Rect(8,y+3,contentWidth-16,27),group,tableHeaderStyle);y+=36;}
                float h=heights[i];Surface(new Rect(0,y,contentWidth,h-4),Color.Lerp(surface,accent,i%2==0?.02f:.055f));
                bool changed=balanceEdits.TryGetValue(field.Id,out var edit);
                GUI.Label(new Rect(8,y+5,contentWidth*.48f-12,h-42),(changed?"* ":"")+field.Name.Replace('_',' '),bodyStyle);
                var current=BalanceAt(source,field.Path);var def=BalanceAt(balanceView.Defaults,field.Path);
                string text=changed?(edit.Remove?"Default":balanceInputs.GetValueOrDefault(field.Id,"")):BalanceText(current);
                float inputX=contentWidth*.49f,inputWidth=Math.Max(40,contentWidth-inputX-12);
                bool canEdit=editing&&field.Editable&&!BalanceBusy&&(balanceEditRevision.Length==0||balanceEditRevision==balanceView.Revision);
                if(canEdit&&field.Type=="bool")
                {if(Button(new Rect(inputX,y+5,inputWidth,32),text,false,true,"Toggle this override. Default removes it from the JSON.")){StageBalanceText(field,text.Equals("true",StringComparison.OrdinalIgnoreCase)?"false":"true");}}
                else if(canEdit&&field.Type!="info")
                {
                    string input=changed&&edit.Remove?"":balanceInputs.TryGetValue(field.Id,out var stored)?stored:current.ValueKind==JsonValueKind.Undefined?"":BalanceText(current);
                    string next=GUI.TextField(new Rect(inputX,y+5,inputWidth,32),input,32,inputStyle);
                    if(next!=input)StageBalanceText(field,next);
                    if(input.Length==0)GUI.Label(new Rect(inputX+12,y+9,inputWidth-20,25),"Default",smallStyle);
                }
                else GUI.Label(new Rect(inputX,y+7,inputWidth,Math.Max(30,h-44)),text,smallStyle);
                string baseline=field.Baseline.Length>0?"Vanilla: "+field.Baseline:"Default: "+BalanceText(def);
                GUI.Label(new Rect(8,y+h-33,contentWidth-(editing?112:16),29),baseline,smallStyle);
                if(editing&&field.Editable&&Button(new Rect(contentWidth-102,y+h-36,94,28),"DEFAULT",false,canEdit,"Remove this override. Unit Balance supplies its normal fallback.",tabStyle))
                {
                    if(balanceEdits.Count==0)balanceEditRevision=balanceView.Revision;
                    balanceEdits[field.Id]=new BalanceEdit{Field=field.Id,Remove=true};balanceInputs.Remove(field.Id);balanceSaveConfirm=balanceReloadConfirm=false;
                }
                y+=h;
            }
        }
        finally{GUI.EndScrollView();}
    }
    private void RenderBalanceActions(Rect rect,bool editing)
    {
        if(!editing){GUI.Label(rect,"Read only. Values update when the server reloads Unit Balance.",smallStyle);return;}
        float w=(rect.width-20)/3;
        if(Button(new Rect(rect.x,rect.y,w,40),balanceSaveConfirm?"CONFIRM SAVE JSON":"SAVE JSON ("+balanceEdits.Count+")",false,
            !BalanceBusy&&balanceEdits.Count>0&&balanceView?.Admin==true&&balanceEditRevision==balanceView.Revision,"Save the edited fields with a server-side JSON backup. Reload is a separate action."))
        {if(balanceSaveConfirm)BeginBalanceSave();else{balanceSaveConfirm=true;balanceReloadConfirm=false;}}
        if(Button(new Rect(rect.x+w+10,rect.y,w,40),balanceReloadConfirm?"CONFIRM RELOAD":"RELOAD SAVED JSON",false,!BalanceBusy&&balanceEdits.Count==0&&balanceView?.Admin==true,
            "Apply the saved JSON using Unit Balance's existing reload handler and refresh all client caches. The mod's configured Discord auto-post behavior is retained."))
        {if(balanceReloadConfirm){balanceReloadConfirm=false;SendBalance("reload");balanceMessage="Reload requested; waiting for the server.";}else{balanceReloadConfirm=true;balanceSaveConfirm=false;}}
        if(Button(new Rect(rect.x+2*(w+10),rect.y,w,40),"DISCARD EDITS",false,!BalanceBusy&&balanceEdits.Count>0,"Discard only your unsaved local edits."))ClearBalanceEdits();
    }
}
