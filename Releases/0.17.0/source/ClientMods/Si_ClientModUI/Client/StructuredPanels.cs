using System.Globalization;
using System.Text.Json;
using Il2Cpp;
using UnityEngine;
namespace SiSquadHud;
public sealed partial class SquadHud
{
    private static string JsonText(JsonElement e,string key)=>e.ValueKind==JsonValueKind.Object&&e.TryGetProperty(key,out var v)&&v.ValueKind==JsonValueKind.String?v.GetString()??"":"";
    private static bool JsonArray(JsonElement e,string key,out JsonElement a){a=default;return e.ValueKind==JsonValueKind.Object&&e.TryGetProperty(key,out a)&&a.ValueKind==JsonValueKind.Array;}
    private sealed class PanelRow {internal string Heading,Text,Key;internal ServerCommand Command;internal float Height;}
    private readonly Dictionary<string,string> controlDrafts=new();
    private readonly Dictionary<string,string> controlConfirmed=new();
    private readonly HashSet<string> controlDirty=new(),expandedControlDetails=new();
    private string dropdownKey="",serverPanelMode="",panelRowsKey="",panelModule="",browseSection="",browseEntry="";
    private bool changesOnly;
    private readonly HashSet<string> collapsedServerGroups=new();
    private PanelRow[] preparedPanelRows=Array.Empty<PanelRow>();
    private Vector2 serverPanelScroll,pickScroll;
    private static bool RemoteAllowed(ServerCommand c)=>c!=null&&(!c.Admin||CurrentAdmin)&&RoleAllowed(c.Role);
    private void RenderServerPanel(ServerModule mod,Rect view)
    {
        if(panelModule!=mod.Id){panelModule=mod.Id;serverPanelMode="";browseSection=browseEntry="";panelRowsKey="";serverPanelScroll=Vector2.zero;pickerCommand=null;}
        if(pickerCommand!=null){RenderArgumentPicker(view);return;}
        float split=FeedbackDivider(view.x,view.y+120,view.yMax-30,view.width,"server-panel",100,70);
        var body=new Rect(view.x,view.y,view.width,Math.Max(60,split-view.y-10));
        float x=body.x;
        bool catalog=mod.Data.HasValue&&mod.Data.Value.TryGetProperty("catalog",out _),browse=mod.Data.HasValue&&mod.Data.Value.TryGetProperty("browse",out _);
        if(serverPanelMode.Length==0)serverPanelMode=catalog?"CATALOG":browse?"BROWSE":"CONTROLS";
        foreach(var mode in new[]{"CONTROLS","CATALOG","BROWSE"})
        {
            if(mode=="CATALOG"&&!catalog||mode=="BROWSE"&&!browse)continue;
            if(Tab(new Rect(x,body.y,122,32),mode,serverPanelMode==mode)){serverPanelMode=mode;panelRowsKey="";serverPanelScroll=Vector2.zero;}x+=128;
        }
        if(serverPanelMode=="BROWSE")
        {
            if(Button(new Rect(body.x,body.y+40,100,30),"BACK")){if(browseEntry.Length>0)browseEntry="";else browseSection="";panelRowsKey="";}
            if(Button(new Rect(body.x+108,body.y+40,220,30),(changesOnly?"[x] ":"[ ] ")+"Changes only",changesOnly)){changesOnly=!changesOnly;panelRowsKey="";}
        }
        PreparePanelRows(mod,body.width-26);
        var region=new Rect(body.x,body.y+82,body.width,Math.Max(24,body.height-82));
        serverPanelScroll=GUI.BeginScrollView(region,serverPanelScroll,new Rect(0,0,body.width-26,preparedPanelRows.Sum(r=>r.Height)+12),false,true);
        try
        {
            float y=0,w=body.width-30;
            foreach(var row in preparedPanelRows)
            {
                if(y+row.Height>=serverPanelScroll.y&&y<=serverPanelScroll.y+region.height)
                {
                    var rect=new Rect(0,y,w,row.Height-6);
                    if(row.Command!=null)RenderTypedControl(rect,mod.Id+":"+row.Command.Text,row.Command,v=>SubmitServerControl(mod,row.Command,v));
                    else if(row.Key!=null)
                    {
                        if(Button(rect,row.Text))
                        {
                            if(row.Key.StartsWith("section:")){browseSection=row.Key.Substring(8);browseEntry="";}
                            else if(row.Key.StartsWith("entry:"))browseEntry=row.Key.Substring(6);
                            else if(row.Key.StartsWith("group:")){string g=mod.Id+":"+row.Key.Substring(6);if(!collapsedServerGroups.Add(g))collapsedServerGroups.Remove(g);}
                            panelRowsKey="";serverPanelScroll=Vector2.zero;
                        }
                    }
                    else GUI.Label(rect,row.Heading??row.Text,row.Heading!=null?titleStyle:smallStyle);
                }
                y+=row.Height;
            }
        }
        finally{GUI.EndScrollView();}
        RenderFeedback(new Rect(view.x,split+10,view.width,Math.Max(24,view.yMax-split-10)));
    }
    private void PreparePanelRows(ServerModule mod,float width)
    {
        string key=mod.Id+":"+mod.Hash+":"+serverPanelMode+":"+CurrentRole+":"+CurrentAdmin+":"+CurrentSidebarTeam+":"+browseSection+":"+browseEntry+":"+changesOnly+":"+width+":"+smallStyle.fontSize+":"+serverChoices.Count+":"+uiRevision;
        if(key==panelRowsKey)return;panelRowsKey=key;
        var rows=new List<PanelRow>();
        void Text(string t,bool heading=false){if(rows.Count<4096)rows.Add(new PanelRow{Heading=heading?t:null,Text=t,Height=Math.Max(34,MeasureText(heading?titleStyle:smallStyle,t,width)+12)});}
        if(mod.Data.HasValue)
        {
            var data=mod.Data.Value;
            if(JsonArray(data,"info",out var info))foreach(var line in info.EnumerateArray().Take(8))if(line.ValueKind==JsonValueKind.String)Text(Plain(line.GetString()));
            if(serverPanelMode=="CATALOG"&&data.TryGetProperty("catalog",out var catalog)&&catalog.ValueKind==JsonValueKind.Object)
            {
                string faction=Player.CurrentPlayer?.Team?.TeamShortName??"";
                if(catalog.TryGetProperty(faction,out var categories)&&categories.ValueKind==JsonValueKind.Array)
                foreach(var category in categories.EnumerateArray().Take(64))
                {
                    Text(JsonText(category,"c"),true);
                    if(JsonArray(category,"u",out var units))foreach(var unit in units.EnumerateArray().Take(256))
                    {
                        if(unit.ValueKind!=JsonValueKind.Array||unit.GetArrayLength()<2||unit[0].ValueKind!=JsonValueKind.String)continue;
                        string name=unit[0].GetString()??"",command=JsonText(data,"cmd")+" "+name;
                        if(Protocol.ValidCommandText(command))rows.Add(new PanelRow{Command=new ServerCommand{Text=command,Label=name+" — "+unit[1],Description="Purchase through this mod's normal command handler."},Height=150});
                    }
                }
                else Text("No catalog supplied for your current faction.");
            }
            else if(serverPanelMode=="BROWSE"&&data.TryGetProperty("browse",out var browser)&&JsonArray(browser,"sections",out var sections))
            foreach(var section in sections.EnumerateArray().Take(128))
            {
                string sn=JsonText(section,"n");
                if(browseSection.Length==0){rows.Add(new PanelRow{Text=sn,Key="section:"+sn,Height=44});continue;}
                if(sn!=browseSection||!JsonArray(section,"u",out var entries))continue;
                foreach(var entry in entries.EnumerateArray().Take(512))
                {
                    string en=JsonText(entry,"n");
                    if(changesOnly&&entry.TryGetProperty("c",out var changed)&&changed.TryGetInt32(out int count)&&count==0)continue;
                    if(browseEntry.Length==0){rows.Add(new PanelRow{Text=en,Key="entry:"+en,Height=44});continue;}
                    if(en!=browseEntry||!JsonArray(entry,"g",out var groups))continue;
                    Text(sn+" / "+en,true);
                    foreach(var group in groups.EnumerateArray().Take(64))
                    {
                        Text(JsonText(group,"n"),true);
                        if(JsonArray(group,"r",out var stats))foreach(var stat in stats.EnumerateArray().Take(512))
                        {
                            if(stat.ValueKind!=JsonValueKind.Array||stat.GetArrayLength()<2||changesOnly&&stat.GetArrayLength()<3)continue;
                            Text(stat[0]+": "+stat[1]+(stat.GetArrayLength()>2?" | Vanilla: "+stat[2]:"")+(stat.GetArrayLength()>3?" | "+stat[3]:""));
                        }
                    }
                }
            }
        }
        if(serverPanelMode=="CONTROLS")
        {
            string group=null;
            foreach(var cmd in mod.Commands.Where(RemoteAllowed).OrderBy(c=>c.Order).ThenBy(c=>c.Admin))
            {
                string next=cmd.Ui?.Group??(cmd.Admin?"Admin":"Player");
                if(next!=group){group=next;if(group.Length>0)rows.Add(new PanelRow{Text=(collapsedServerGroups.Contains(mod.Id+":"+group)?"+ ":"− ")+group,Key="group:"+group,Height=44});}
                if(group.Length>0&&collapsedServerGroups.Contains(mod.Id+":"+group))continue;
                rows.Add(new PanelRow{Command=cmd,Height=150});
            }
            foreach(var c in serverChoices.Where(c=>c.Group==mod.Id&&CommandAllowed(c)))rows.Add(new PanelRow{Command=new ServerCommand{Text=c.Command,Label=c.Label,Description=c.Description,Argument=c.RequiresInput?"text":"",Admin=c.AdminOnly},Height=150});
            if(rows.Count==0)Text("No panel information supplied. Known commands remain available under COMMANDS.");
        }
        preparedPanelRows=rows.Take(4096).ToArray();
    }
    private void RenderTypedControl(Rect rect,string key,ServerCommand c,Action<string> submit)
    {
        var ui=c.Ui;
        if(controlDrafts.Count>2048){controlDrafts.Clear();controlConfirmed.Clear();controlDirty.Clear();}
        if(ui?.Kind is "header" or "explanation"){GUI.Label(rect,c.Label+"\n"+c.Description,ui.Kind=="header"?titleStyle:smallStyle);return;}
        Surface(rect,Color.Lerp(surface,accent,.04f));
        GUI.Label(new Rect(rect.x+6,rect.y+3,rect.width-12,28),c.Label+(c.Value!=null?": "+c.Value:""),bodyStyle);
        GUI.Label(new Rect(rect.x+6,rect.y+33,rect.width-12,32),c.Description,smallStyle);
        string refs=(ui?.Default!=null?"Default: "+ui.Default+"   ":"")+(ui?.Vanilla!=null?"Vanilla: "+ui.Vanilla:"");
        if(refs.Length>0)
        {
            bool expanded=expandedControlDetails.Contains(key);
            if(Button(new Rect(rect.x+6,rect.y+112,84,24),expanded?"− DETAILS":"+ DETAILS",style:smallStyle))
            {if(expanded)expandedControlDetails.Remove(key);else expandedControlDetails.Add(key);}
            if(expanded)GUI.Label(new Rect(rect.x+96,rect.y+112,rect.width-102,24),refs,smallStyle);
        }
        float x=rect.x+6,y=rect.y+72,w=rect.width-12;
        if(!controlDrafts.ContainsKey(key)||!controlDirty.Contains(key)&&(!controlConfirmed.TryGetValue(key,out var old)||old!=c.Value))
        {controlDrafts[key]=c.Value??"";controlConfirmed[key]=c.Value??"";}
        string draft=controlDrafts[key];
        bool allowed=RemoteAllowed(c)&&(key.StartsWith("local:")||serverRequest==null);
        if(ui?.Kind=="readonly"){GUI.Label(new Rect(x,y,w,32),c.Value??"Unavailable",bodyStyle);return;}
        if(c.Value==null&&c.Argument.Length>0&&(c.Argument!="choice"||c.Choices.Count==0))
        {if(Button(new Rect(x,y,w,34),"CHOOSE…",allowed:allowed))submit(null);return;}
        if(c.Value==null&&c.Choices.Count==0)
        {if(Button(new Rect(x,y,w,34),c.Argument.Length>0?"CHOOSE…":"RUN",allowed:allowed))submit(null);return;}
        if(ui?.Kind=="checkbox"&&c.Choices.Count>=2)
        {
            bool on=c.Value is "on" or "true" or "1";
            if(Button(new Rect(x,y,w,34),(on?"[x] ":"[ ] ")+c.Label,on,allowed))submit(c.Choices[on?1:0][0]);return;
        }
        if(c.Value!=null&&c.Choices.Count>0&&ui?.Kind!="dropdown"&&c.Choices.Count<=5)
        {
            float bw=w/Math.Max(1,c.Choices.Count);
            for(int i=0;i<c.Choices.Count;i++)if(Button(new Rect(x+i*bw,y,bw-4,34),c.Choices[i][1],string.Equals(c.Value,c.Choices[i][0],StringComparison.OrdinalIgnoreCase),allowed))submit(c.Choices[i][0]);return;
        }
        if(c.Choices.Count>0)
        {
            if(Button(new Rect(x,y,w,34),(c.Value??"Select")+"  ▾",allowed:allowed))
            {dropdownKey=key;dropdownChoices=c.Choices.Select(a=>a.ToArray()).ToArray();dropdownSubmit=submit;dropdownTitle=c.Label;}
            return;
        }
        if(ui?.Kind=="slider"&&ui.Min.HasValue&&ui.Max.HasValue&&ui.Max>ui.Min)
        {
            double.TryParse(draft,NumberStyles.Float,CultureInfo.InvariantCulture,out double n);
            float oldValue=(float)Math.Clamp(n,ui.Min.Value,ui.Max.Value);
            float next=GUI.HorizontalSlider(new Rect(x,y,w-170,28),oldValue,(float)ui.Min.Value,(float)ui.Max.Value);
            if(next!=oldValue){double step=ui.Step??.01;controlDrafts[key]=(Math.Clamp(ui.Min.Value+Math.Round((next-ui.Min.Value)/step)*step,ui.Min.Value,ui.Max.Value)).ToString("0.######",CultureInfo.InvariantCulture);controlDirty.Add(key);}
            GUI.Label(new Rect(x+w-162,y,76,30),controlDrafts[key]+(ui.Unit??""),smallStyle);
        }
        else
        {
            string next=GUI.TextField(new Rect(x,y,w-92,34),draft,160,inputStyle);
            if(next!=draft){controlDrafts[key]=next;controlDirty.Add(key);}
        }
        if(Button(new Rect(x+w-84,y,84,34),"SET",allowed:allowed&&controlDrafts[key].Trim().Length>0))
        {
            string value=controlDrafts[key].Trim();
            if(ui?.Min!=null||ui?.Max!=null)
            {
                if(!double.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out double n)||!double.IsFinite(n)||ui.Min.HasValue&&n<ui.Min||ui.Max.HasValue&&n>ui.Max)
                {controllerMessage="Enter a value within the supplied range.";return;}
            }
            submit(value);controlDirty.Remove(key);
        }
    }
    private string[][] dropdownChoices=Array.Empty<string[]>();
    private Action<string> dropdownSubmit;
    private string dropdownTitle="";
    private Vector2 dropdownScroll;
    private void RenderDropdown()
    {
        if(dropdownKey.Length==0)return;
        var r=new Rect(Math.Max(12,(UiWidth-480)/2),Math.Max(12,(UiHeight-460)/2),Math.Min(480,UiWidth-24),Math.Min(460,UiHeight-24));
        WindowSurface(r);GUI.Label(new Rect(r.x+14,r.y+10,r.width-110,30),dropdownTitle,titleStyle);
        if(Button(new Rect(r.xMax-86,r.y+10,72,30),"CLOSE")){dropdownKey="";return;}
        var area=new Rect(r.x+14,r.y+52,r.width-28,r.height-66);
        dropdownScroll=GUI.BeginScrollView(area,dropdownScroll,new Rect(0,0,area.width-24,dropdownChoices.Length*42),false,true);
        try
        {
            for(int i=Math.Max(0,(int)(dropdownScroll.y/42)-1);i<Math.Min(dropdownChoices.Length,(int)((dropdownScroll.y+area.height)/42)+2);i++)if(Button(new Rect(0,i*42,area.width-28,34),dropdownChoices[i][1]))
            {var callback=dropdownSubmit;string token=dropdownChoices[i][0];dropdownKey="";callback?.Invoke(token);break;}
        }
        finally{GUI.EndScrollView();}
    }
    private void SubmitServerControl(ServerModule mod,ServerCommand cmd,string value)
    {
        if(!companionReady||!RemoteAllowed(cmd)||serverRequest!=null||!serverCatalog.Modules.TryGetValue(mod.Id,out var currentModule))return;
        if(currentModule.Hash!=mod.Hash){controllerMessage="This mod changed; select the current control again.";return;}
        if(value==null&&(cmd.Argument.Length>0||cmd.Choices.Count>0)){StartPicker(mod,cmd);return;}
        string text=cmd.Text+(value!=null?" "+value:"");
        if(!Protocol.ValidCommandText(text)){controllerMessage="Command exceeds the supported length.";return;}
        activeExchange=new Exchange{Entry=new CommandEntry{Mod=ModLabel(mod.Name),Group=mod.Id,Command=cmd.Text,Role=cmd.Admin?"admin":cmd.Role,AdminOnly=cmd.Admin,Source="server",Scope=ServerScope},At=clock.Elapsed.TotalSeconds};
        // The proxy validates the root and gameplay handler validates the complete action.
        SendTypedChat(text);
    }
    private ServerCommand pickerCommand;
    private ServerModule pickerModule;
    private readonly List<string> pickerTokens=new();
    private string pickerText="";
    private string[] PickerStages()=>pickerCommand.Argument.Length>0?pickerCommand.Argument.Split(',').Select(x=>x.Trim()).ToArray():new[]{"choice"};
    private void StartPicker(ServerModule mod,ServerCommand command){pickerModule=mod;pickerCommand=command;pickerTokens.Clear();pickerText="";pickScroll=Vector2.zero;}
    private void RenderArgumentPicker(Rect view)
    {
        if(!companionReady||!serverCatalog.Modules.TryGetValue(pickerModule.Id,out var currentPickerModule)||currentPickerModule.Hash!=pickerModule.Hash||!RemoteAllowed(pickerCommand)){pickerCommand=null;return;}
        var stages=PickerStages();int index=pickerTokens.Count;
        if(index>=stages.Length){var cmd=pickerCommand;var mod=pickerModule;string value=string.Join(" ",pickerTokens);pickerCommand=null;SubmitServerControl(mod,cmd,value);return;}
        string stage=stages[index].TrimEnd('?').ToLowerInvariant();bool optional=stages[index].EndsWith('?');
        GUI.Label(new Rect(view.x,view.y,view.width-110,38),pickerCommand.Label+" / "+stage+" ("+(index+1)+"/"+stages.Length+")",titleStyle);
        if(Button(new Rect(view.xMax-100,view.y,100,32),index>0?"BACK":"CANCEL")){if(index>0)pickerTokens.RemoveAt(index-1);else pickerCommand=null;return;}
        if(optional&&Button(new Rect(view.x,view.y+44,160,32),"SKIP / DEFAULT")){pickerTokens.Add("");return;}
        if(stage=="text"||stage is not ("player" or "team" or "map" or "mode" or "ban" or "choice"))
        {
            pickerText=GUI.TextField(new Rect(view.x,view.y+90,view.width-100,34),pickerText,120,inputStyle);
            if(Button(new Rect(view.xMax-90,view.y+90,90,34),"OK",allowed:pickerText.Trim().Length>0)){pickerTokens.Add(pickerText.Trim());pickerText="";}return;
        }
        var options=PickerOptions(stage,stages);
        var area=new Rect(view.x,view.y+90,view.width,view.height-90);
        pickScroll=GUI.BeginScrollView(area,pickScroll,new Rect(0,0,view.width-26,options.Count*42+60),false,true);
        try
        {
            for(int i=Math.Max(0,(int)(pickScroll.y/42)-1);i<Math.Min(options.Count,(int)((pickScroll.y+area.height)/42)+2);i++)if(Button(new Rect(0,i*42,view.width-30,34),options[i].label)){pickerTokens.Add(options[i].value);pickScroll=Vector2.zero;break;}
            if(options.Count==0)GUI.Label(new Rect(0,0,view.width-30,60),"No unambiguous options available. Use COMMANDS to enter arguments manually.",smallStyle);
        }
        finally{GUI.EndScrollView();}
    }
    private List<(string value,string label)> pickerOptionsCache=new();
    private string pickerOptionsKey="";
    private double pickerOptionsAt;
    private List<(string value,string label)> PickerOptions(string stage,string[] stages)
    {
        string key=pickerModule.Id+":"+pickerCommand.Text+":"+stage+":"+string.Join("|",pickerTokens);
        if(key==pickerOptionsKey&&clock.Elapsed.TotalSeconds<pickerOptionsAt)return pickerOptionsCache;
        pickerOptionsKey=key;pickerOptionsAt=clock.Elapsed.TotalSeconds+UiPolicies.UiRefreshSeconds;
        var options=new List<(string value,string label)>();
        if(stage=="player")
        {
            var names=new List<string>();foreach(var p in Player.Players)if(p!=null)names.Add(p.PlayerName??"");
            foreach(var name in names)
            {
                string token=name.Split(' ').Where(t=>t.Length>0&&names.Count(n=>n.Contains(t,StringComparison.OrdinalIgnoreCase))==1).OrderByDescending(t=>t.Length).FirstOrDefault();
                if(token!=null)options.Add((token,name));
            }
        }
        else if(stage=="team")
        {
            var strategy=GameMode.CurrentGameMode?.TryCast<MP_Strategy>();
            if(strategy?.TeamSetups!=null)foreach(var setup in strategy.TeamSetups)
                if(setup?.Team!=null&&strategy.GetTeamSetupActive(setup))options.Add((setup.Team.TeamShortName,setup.Team.TeamShortName));
            if(options.Count==0)foreach(var team in Team.Teams)if(team!=null)options.Add((team.TeamShortName,team.TeamShortName));
        }
        else if(stage=="mode")
        {
            int at=Array.FindIndex(stages,s=>s.TrimEnd('?').Equals("map",StringComparison.OrdinalIgnoreCase));
            if(at>=0&&at<pickerTokens.Count)foreach(var choice in pickerCommand.Choices.Where(c=>c.Length==3&&c[0]==pickerTokens[at]))foreach(string mode in choice[2].Split(','))if(mode.Length>0)options.Add((mode,mode));
        }
        else options.AddRange(pickerCommand.Choices.Select(c=>(c[0],c[1])));
        return pickerOptionsCache=options;
    }

}
