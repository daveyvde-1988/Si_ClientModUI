using System.Globalization;
using System.Text.Json;
using Il2Cpp;
using UnityEngine;
namespace SiSquadHud;
public sealed partial class SquadHud
{
    private static string JsonText(JsonElement e,string key)=>e.ValueKind==JsonValueKind.Object&&e.TryGetProperty(key,out var v)&&v.ValueKind==JsonValueKind.String?v.GetString()??"":"";
    private static bool JsonArray(JsonElement e,string key,out JsonElement a){a=default;return e.ValueKind==JsonValueKind.Object&&e.TryGetProperty(key,out a)&&a.ValueKind==JsonValueKind.Array;}
    private sealed class PanelRow
    {
        internal string Heading,Text,Key;internal ServerCommand Command;internal float Height;
        internal ServerCommand[] Buttons;internal float[] Widths;internal string[] Cells,Navigation;internal bool Header,Allowed=true;
    }
    private readonly Dictionary<string,string> controlDrafts=new();
    private readonly Dictionary<string,string> controlConfirmed=new();
    private readonly HashSet<string> controlDirty=new(),expandedControlDetails=new();
    private string dropdownKey="",serverPanelMode="",panelRowsKey="",panelModule="",browseSection="",browseEntry="";
    private bool changesOnly;
    private readonly HashSet<string> collapsedServerGroups=new();
    private PanelRow[] preparedPanelRows=Array.Empty<PanelRow>();
    private Vector2 serverPanelScroll,pickScroll;
    private string catalogFaction="";
    private ServerPanelData PanelData(ServerModule mod)=>mod.Panel??=ServerPanelData.Parse(mod.Data);
    private bool HasCatalog(ServerModule mod)=>PanelData(mod).CatalogVisible(modAdmin,CurrentAdmin,mod.Commands);
    private static string LocalCatalogFaction()
    {
        var team=Player.CurrentPlayer?.Team;
        string name=team?.TeamShortName??"";
        if(name.Contains("Cent",StringComparison.OrdinalIgnoreCase))return "Centauri";
        if(name.Contains("Alien",StringComparison.OrdinalIgnoreCase))return "Alien";
        if(name.Contains("Sol",StringComparison.OrdinalIgnoreCase))return "Sol";
        string objectName=team?.name??"";
        if(objectName.Contains("Centauri",StringComparison.OrdinalIgnoreCase))return "Centauri";
        if(objectName.Contains("Alien",StringComparison.OrdinalIgnoreCase))return "Alien";
        if(objectName.Contains("Sol",StringComparison.OrdinalIgnoreCase))return "Sol";
        return name;
    }
    private static bool RemoteAllowed(ServerCommand c)=>c!=null&&(!c.Admin||CurrentAdmin)&&RoleAllowed(c.Role);
    private void RenderServerPanel(ServerModule mod,Rect view)
    {
        if(panelModule!=mod.Id){panelModule=mod.Id;serverPanelMode="";browseSection=browseEntry=catalogFaction="";panelRowsKey="";serverPanelScroll=Vector2.zero;pickerCommand=null;}
        if(pickerCommand!=null){RenderArgumentPicker(view);return;}
        float split=FeedbackDivider(view.x,view.y+120,view.yMax-30,view.width,"server-panel",100,70);
        var body=new Rect(view.x,view.y,view.width,Math.Max(60,split-view.y-10));
        float x=body.x;
        var panelData=PanelData(mod);
        bool catalog=HasCatalog(mod),browse=!modAdmin&&mod.Data.HasValue&&mod.Data.Value.TryGetProperty("browse",out _);
        if(serverPanelMode=="CATALOG"&&!catalog||serverPanelMode=="BROWSE"&&!browse)serverPanelMode="";
        if(serverPanelMode.Length==0)serverPanelMode=catalog?"CATALOG":browse?"BROWSE":"CONTROLS";
        foreach(var mode in new[]{"CONTROLS","CATALOG","BROWSE"})
        {
            if(mode=="CONTROLS"&&!catalog&&!browse||mode=="CATALOG"&&!catalog||mode=="BROWSE"&&!browse)continue;
            if(Tab(new Rect(x,body.y,122,32),mode=="CONTROLS"?"COMMANDS":mode,serverPanelMode==mode)){serverPanelMode=mode;panelRowsKey="";serverPanelScroll=Vector2.zero;}x+=128;
        }
        if(catalog&&serverPanelMode=="CATALOG")
        {
            var root=mod.Commands.FirstOrDefault(c=>c.Text.Equals(panelData.Command,StringComparison.OrdinalIgnoreCase)||c.Aliases.Contains(panelData.Command,StringComparer.OrdinalIgnoreCase));
            if(root!=null&&Button(new Rect(body.xMax-122,body.y,122,32),"CHAT MENU",allowed:serverRequest==null))
            {serverPanelMode="CONTROLS";panelRowsKey="";serverPanelScroll=Vector2.zero;SubmitServerControl(mod,root,null,true);}
        }
        if(serverPanelMode=="BROWSE")
        {
            if(Button(new Rect(body.x,body.y+40,100,30),"BACK")){if(browseEntry.Length>0)browseEntry="";else browseSection="";panelRowsKey="";}
            if(Button(new Rect(body.x+108,body.y+40,220,30),(changesOnly?"[x] ":"[ ] ")+"Changes only",changesOnly)){changesOnly=!changesOnly;panelRowsKey="";}
        }
        PreparePanelRows(mod,body.width-26);
        float contentTop=serverPanelMode=="BROWSE"?82:catalog||browse?44:0;
        var region=new Rect(body.x,body.y+contentTop,body.width,Math.Max(24,body.height-contentTop));
        serverPanelScroll=GUI.BeginScrollView(region,serverPanelScroll,new Rect(0,0,body.width-26,preparedPanelRows.Sum(r=>r.Height)+12),false,true);
        try
        {
            float y=0,w=body.width-30;
            foreach(var row in preparedPanelRows)
            {
                if(y+row.Height>=serverPanelScroll.y&&y<=serverPanelScroll.y+region.height)
                {
                    var rect=new Rect(0,y,w,row.Height-6);
                    if(row.Buttons!=null)
                    {
                        float bx=0;
                        for(int i=0;i<row.Buttons.Length;i++)
                        {
                            var command=row.Buttons[i];
                            if(Button(new Rect(bx,rect.y,row.Widths[i]-6,rect.height),command.Label,allowed:row.Allowed&&serverRequest==null&&RemoteAllowed(command)))SubmitServerControl(mod,command,null);
                            bx+=row.Widths[i];
                        }
                    }
                    else if(row.Navigation!=null)
                    {
                        float nx=0;
                        for(int i=0;i<row.Navigation.Length;i++)
                        {
                            string faction=row.Navigation[i];
                            if(Tab(new Rect(nx,rect.y,row.Widths[i]-6,rect.height),faction,faction.Equals(catalogFaction,StringComparison.OrdinalIgnoreCase)))
                            {catalogFaction=faction;panelRowsKey="";serverPanelScroll=Vector2.zero;}
                            nx+=row.Widths[i];
                        }
                    }
                    else if(row.Cells!=null)
                    {
                        float cx=4;
                        for(int i=0;i<row.Widths.Length;i++)
                        {
                            if(i<row.Cells.Length)GUI.Label(new Rect(cx,rect.y+3,row.Widths[i]-8,rect.height-4),Plain(row.Cells[i]),row.Header?smallStyle:bodyStyle);
                            cx+=row.Widths[i];
                        }
                    }
                    else if(row.Command!=null)RenderRemoteControl(rect,mod.Id+":"+row.Command.Text,row.Command,v=>SubmitServerControl(mod,row.Command,v));
                    else if(row.Key!=null)
                    {
                        if(Button(rect,row.Text))
                        {
                            if(row.Key.StartsWith("section:")){browseSection=row.Key.Substring(8);browseEntry="";}
                            else if(row.Key.StartsWith("entry:"))browseEntry=row.Key.Substring(6);
                            else if(row.Key.StartsWith("faction:"))catalogFaction=row.Key.Substring(8);
                            else if(row.Key=="balance")RequestCatalogBalance(mod);
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
        string key=mod.Id+":"+mod.Hash+":"+serverPanelMode+":"+modAdmin+":"+CurrentRole+":"+CurrentAdmin+":"+CurrentSidebarTeam+":"+browseSection+":"+browseEntry+":"+changesOnly+":"+catalogFaction+":"+width+":"+smallStyle.fontSize+":"+serverChoices.Count+":"+uiRevision;
        if(key==panelRowsKey)return;panelRowsKey=key;
        var rows=new List<PanelRow>();
        void Text(string t,bool heading=false){if(rows.Count<4096)rows.Add(new PanelRow{Heading=heading?t:null,Text=t,Height=Math.Max(34,MeasureText(heading?titleStyle:smallStyle,t,width)+12)});}
        var panelData=PanelData(mod);
        Text(Plain(mod.Name)+(mod.Version.Length>0?"  "+mod.Version:""),true);
        foreach(string line in panelData.Info)Text(Plain(line));
        if(serverPanelMode=="CONTROLS")AddPanelTables(rows,panelData,width);
        if(mod.Data.HasValue)
        {
            var data=mod.Data.Value;
            if(serverPanelMode=="CATALOG"&&HasCatalog(mod))
            {
                string faction=LocalCatalogFaction();
                if(panelData.AllFactions)
                {
                    if(!panelData.Factions.ContainsKey(catalogFaction))catalogFaction=panelData.Factions.ContainsKey(faction)?faction:panelData.Factions.Keys.FirstOrDefault()??"";
                    faction=catalogFaction;
                    var names=new List<string>();var sizes=new List<float>();float used=0;
                    void FlushFactions(){if(names.Count==0)return;rows.Add(new PanelRow{Navigation=names.ToArray(),Widths=sizes.ToArray(),Height=40});names.Clear();sizes.Clear();used=0;}
                    foreach(string name in panelData.Factions.Keys)
                    {
                        float size=Math.Min(width,Math.Max(90,tabStyle.CalcSize(new GUIContent(name)).x+28));
                        if(used>0&&used+size>width)FlushFactions();
                        names.Add(name);sizes.Add(size);used+=size;
                    }
                    FlushFactions();
                }
                if(panelData.Credits)
                    rows.Add(new PanelRow{Text="Credits: "+(catalogBalances.TryGetValue(mod.Id,out var balance)?balance:"?")+"   / REFRESH",Key="balance",Height=40});
                if(!panelData.Enabled)Text(panelData.OffText);
                if(panelData.Factions.TryGetValue(faction,out var categories))
                foreach(var category in categories)
                {
                    Text(category.Name,true);
                    var buttons=new List<ServerCommand>();var widths=new List<float>();float used=0;
                    void Flush(){if(buttons.Count==0)return;rows.Add(new PanelRow{Buttons=buttons.ToArray(),Widths=widths.ToArray(),Allowed=panelData.Enabled,Height=42});buttons.Clear();widths.Clear();used=0;}
                    foreach(var item in category.Items)
                    {
                        string label=Plain(item.Label)+(panelData.Credits&&item.Cost>0?"  "+item.Cost+"cr":"");
                        float size=Math.Min(width,Math.Max(120,buttonStyle.CalcSize(new GUIContent(label)).x+30));
                        if(used>0&&used+size>width)Flush();
                        buttons.Add(new ServerCommand{Text=panelData.Command+" "+item.Token,Label=label,Admin=panelData.CatalogAdmin,Role=panelData.CatalogAdmin?"admin":"shared"});widths.Add(size);used+=size;
                    }
                    Flush();
                }
                else Text(panelData.AllFactions?"The catalog is empty.":"Join a team to see its catalog.");
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
            var menuChoices=serverChoices.Where(c=>c.Group==mod.Id&&CommandAllowed(c)&&UiPolicies.ModCategory(c.AdminOnly||c.Role=="admin",modAdmin,CurrentAdmin)).ToArray();
            if(menuChoices.Length>0)Text("Chat menu",true);
            foreach(var c in menuChoices)
            {
                var choice=new ServerCommand{Text=c.Command,Label=c.Label,Description=c.Description,Argument=c.RequiresInput?"text":"",Admin=c.AdminOnly};
                rows.Add(new PanelRow{Command=choice,Height=RemoteControlHeight(mod.Id+":"+choice.Text,choice,width-4)+6});
            }
            if(menuChoices.Length>0)Text("Commands",true);
            string group=null;
            var commands=mod.Commands.Where(c=>RemoteAllowed(c)&&UiPolicies.ModCategory(c.Admin||c.Role=="admin",modAdmin,CurrentAdmin)).OrderBy(c=>c.Order).ThenBy(c=>c.Admin).ToArray();
            foreach(var cmd in commands)
            {
                string next=cmd.Ui?.Group??"";
                if(next!=group){group=next;if(group.Length>0)rows.Add(new PanelRow{Text=(collapsedServerGroups.Contains(mod.Id+":"+group)?"+ ":"− ")+group,Key="group:"+group,Height=44});}
                if(group.Length>0&&collapsedServerGroups.Contains(mod.Id+":"+group))continue;
                rows.Add(new PanelRow{Command=cmd,Height=RemoteControlHeight(mod.Id+":"+cmd.Text,cmd,width-4)+6});
            }
            if(commands.Length==0&&menuChoices.Length==0&&panelData.Tables.Count==0)Text("No controls supplied for this view. Known commands remain available under COMMANDS.");
        }
        preparedPanelRows=rows.Take(4096).ToArray();
    }
    private void AddPanelTables(List<PanelRow> rows,ServerPanelData data,float width)
    {
        foreach(var table in data.Tables)
        {
            if(table.Name.Length>0)rows.Add(new PanelRow{Heading=Plain(table.Name),Height=Math.Max(34,MeasureText(titleStyle,Plain(table.Name),width)+10)});
            int columns=Math.Max(table.Headers.Count,table.Rows.Count>0?table.Rows.Max(r=>r.Length):0);
            if(columns==0)continue;
            float[] widths=new float[columns];
            for(int i=0;i<columns;i++)
            {
                widths[i]=i<table.Headers.Count?smallStyle.CalcSize(new GUIContent(Plain(table.Headers[i]))).x+16:40;
                foreach(var row in table.Rows)if(i<row.Length)widths[i]=Math.Max(widths[i],Math.Min(260,bodyStyle.CalcSize(new GUIContent(Plain(row[i]))).x+16));
                widths[i]=Math.Max(40,widths[i]);
            }
            float total=widths.Sum();if(total>width)for(int i=0;i<columns;i++)widths[i]*=width/total;
            void Add(string[] cells,bool header)
            {
                float height=28;
                for(int i=0;i<cells.Length;i++)height=Math.Max(height,MeasureText(header?smallStyle:bodyStyle,Plain(cells[i]),Math.Max(12,widths[i]-8))+10);
                rows.Add(new PanelRow{Cells=cells,Widths=widths,Header=header,Height=height+4});
            }
            if(table.Headers.Count>0)Add(table.Headers.ToArray(),true);
            foreach(var row in table.Rows)Add(row,false);
        }
    }
    private void RequestCatalogBalance(ServerModule mod)
    {
        var data=PanelData(mod);
        var status=mod.Commands.FirstOrDefault(c=>c.Text.Equals(data.Command+" status",StringComparison.OrdinalIgnoreCase)&&RemoteAllowed(c));
        if(status!=null)SubmitServerControl(mod,status,null);
    }
    private List<Rect> RemoteChoiceLayout(ServerCommand command,float width,out float height)
    {
        float labelWidth=Math.Min(width,bodyStyle.CalcSize(new GUIContent(Plain(command.Label))).x+20);
        float labelHeight=Math.Max(32,MeasureText(bodyStyle,Plain(command.Label),width-8)+8);
        var widths=command.Choices.Select(c=>Math.Min(width,Math.Max(64,buttonStyle.CalcSize(new GUIContent(Plain(c[1]))).x+28))).ToArray();
        bool inline=labelWidth+widths.Sum()+widths.Length*6<=width;
        float x=inline?labelWidth:0,y=inline?0:labelHeight;
        var result=new List<Rect>();
        foreach(float w in widths)
        {
            if(x>0&&x+w>width){x=0;y+=40;}
            result.Add(new Rect(x,y,w,34));x+=w+6;
        }
        height=Math.Max(labelHeight,y+38);return result;
    }
    private float RemoteControlHeight(string key,ServerCommand command,float width)
    {
        if(command.Ui!=null)return TypedControlHeight(key,command,width);
        float height=Math.Max(34,MeasureText(bodyStyle,Plain(command.Label),width-8)+8);
        if(command.Value!=null&&command.Choices.Count>0)RemoteChoiceLayout(command,width,out height);
        else if(command.Value!=null)height=Math.Max(38,MeasureText(bodyStyle,Plain(command.Label),Math.Max(60,width*.42f-10))+8);
        return height+ControlDescriptionHeight(command.Description,width)+8;
    }
    private void RenderRemoteControl(Rect rect,string key,ServerCommand command,Action<string> submit)
    {
        if(command.Ui!=null){RenderTypedControl(rect,key,command,submit);return;}
        bool allowed=RemoteAllowed(command)&&serverRequest==null;
        float height=Math.Max(34,MeasureText(bodyStyle,Plain(command.Label),rect.width-8)+8);
        if(command.Value!=null&&command.Choices.Count>0)
        {
            var choices=RemoteChoiceLayout(command,rect.width,out height);
            float labelWidth=choices.Count>0&&choices[0].y==0?choices[0].x:rect.width;
            GUI.Label(new Rect(rect.x,rect.y+4,labelWidth,Math.Max(32,choices.Count>0?choices[0].y:32)),Plain(command.Label),bodyStyle);
            for(int i=0;i<choices.Count;i++)
            {
                Rect r=choices[i];r.x+=rect.x;r.y+=rect.y;
                bool selected=ServerPanelData.ChoiceSelected(command,command.Choices[i][0]);
                if(Button(r,command.Choices[i][1],selected,allowed&&(command.Multi||!selected)))submit(command.Choices[i][0]);
            }
        }
        else if(command.Value!=null)
        {
            float labelWidth=rect.width*.42f,inputWidth=Math.Max(40,rect.width-labelWidth-90);
            height=Math.Max(38,MeasureText(bodyStyle,Plain(command.Label),Math.Max(60,labelWidth-10))+8);
            GUI.Label(new Rect(rect.x,rect.y+4,labelWidth-8,height),Plain(command.Label),bodyStyle);
            if(!controlDrafts.ContainsKey(key)||!controlDirty.Contains(key)&&(!controlConfirmed.TryGetValue(key,out var prior)||prior!=command.Value))
            {controlDrafts[key]=command.Value;controlConfirmed[key]=command.Value;}
            string next=GUI.TextField(new Rect(rect.x+labelWidth,rect.y,inputWidth,34),controlDrafts[key],160,inputStyle);
            if(next!=controlDrafts[key]){controlDrafts[key]=next;controlDirty.Add(key);}
            if(Button(new Rect(rect.xMax-82,rect.y,82,34),"SET",allowed:allowed&&next.Trim().Length>0)){submit(next.Trim());controlDirty.Remove(key);}
        }
        else if(Button(new Rect(rect.x,rect.y,rect.width,height),command.Label,allowed:allowed,description:command.Description))submit(null);
        float descriptionHeight=ControlDescriptionHeight(command.Description,rect.width);
        if(descriptionHeight>0)GUI.Label(new Rect(rect.x+4,rect.y+height+2,rect.width-8,descriptionHeight),Plain(command.Description),smallStyle);
    }
    private static bool TypedAction(ServerCommand c)=>c.Value==null&&c.Choices.Count==0&&c.Argument.Length==0&&c.Ui?.Kind!="readonly";
    private static bool TypedEditor(ServerCommand c)=>c.Ui?.Kind!="readonly"&&!TypedAction(c)&&(c.Value!=null||c.Choices.Count>0||c.Argument.Length>0);
    private static string ControlReferences(ServerCommand c)=>(c.Ui?.Default!=null?"Default: "+c.Ui.Default+"   ":"")+(c.Ui?.Vanilla!=null?"Vanilla: "+c.Ui.Vanilla:"");
    private float TypedLabelHeight(ServerCommand c,float width)=>Math.Max(32,MeasureText(TypedAction(c)?buttonStyle:bodyStyle,c.Label+(c.Value!=null?": "+c.Value:""),Math.Max(40,width-32)));
    private float ControlDescriptionHeight(string description,float width)=>string.IsNullOrWhiteSpace(description)?0:MeasureText(smallStyle,description,Math.Max(40,width-12))+4;
    private float TypedControlHeight(string key,ServerCommand c,float width)
    {
        if(c.Ui?.Kind is "header" or "explanation")return MeasureText(c.Ui.Kind=="header"?titleStyle:smallStyle,c.Label+"\n"+c.Description,width)+12;
        string refs=ControlReferences(c);
        return 12+TypedLabelHeight(c,width)+ControlDescriptionHeight(c.Description,width)+(TypedEditor(c)?40:0)+
            (refs.Length>0?30+(expandedControlDetails.Contains(key)?MeasureText(smallStyle,refs,width-12)+4:0):0);
    }
    private void RenderTypedControl(Rect rect,string key,ServerCommand c,Action<string> submit)
    {
        var ui=c.Ui;
        if(controlDrafts.Count>2048){controlDrafts.Clear();controlConfirmed.Clear();controlDirty.Clear();}
        if(ui?.Kind is "header" or "explanation"){GUI.Label(rect,c.Label+"\n"+c.Description,ui.Kind=="header"?titleStyle:smallStyle);return;}
        Surface(rect,Color.Lerp(surface,accent,.04f));
        bool allowed=RemoteAllowed(c)&&(key.StartsWith("local:")||serverRequest==null);
        bool action=TypedAction(c);
        float labelHeight=TypedLabelHeight(c,rect.width),descriptionHeight=ControlDescriptionHeight(c.Description,rect.width);
        var optionRect=new Rect(rect.x+6,rect.y+6,rect.width-12,labelHeight);
        if(action){if(Button(optionRect,c.Label,allowed:allowed,description:c.Description))submit(null);}
        else GUI.Label(optionRect,c.Label+(c.Value!=null?": "+c.Value:""),bodyStyle);
        if(descriptionHeight>0)GUI.Label(new Rect(rect.x+6,optionRect.yMax+2,rect.width-12,descriptionHeight),c.Description,smallStyle);
        float x=rect.x+6,y=optionRect.yMax+descriptionHeight+4,w=rect.width-12;
        string refs=ControlReferences(c);
        if(refs.Length>0)
        {
            bool expanded=expandedControlDetails.Contains(key);
            float detailsY=y+(TypedEditor(c)?40:0);
            if(Button(new Rect(x,detailsY,104,26),expanded?"− DETAILS":"+ DETAILS",style:smallStyle))
            {if(expanded)expandedControlDetails.Remove(key);else expandedControlDetails.Add(key);panelRowsKey="";}
            if(expanded)GUI.Label(new Rect(x,detailsY+30,w,MeasureText(smallStyle,refs,w)),refs,smallStyle);
        }
        if(!controlDrafts.ContainsKey(key)||!controlDirty.Contains(key)&&(!controlConfirmed.TryGetValue(key,out var old)||old!=c.Value))
        {controlDrafts[key]=c.Value??"";controlConfirmed[key]=c.Value??"";}
        string draft=controlDrafts[key];
        if(ui?.Kind=="readonly")return;
        if(c.Value==null&&c.Argument.Length>0&&(c.Argument!="choice"||c.Choices.Count==0))
        {if(Button(new Rect(x,y,w,34),"CHOOSE…",allowed:allowed))submit(null);return;}
        if(c.Value==null&&c.Choices.Count==0)
        {return;}
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
    private void SubmitServerControl(ServerModule mod,ServerCommand cmd,string value,bool direct=false)
    {
        if(!companionReady||!RemoteAllowed(cmd)||serverRequest!=null||!serverCatalog.Modules.TryGetValue(mod.Id,out var currentModule))return;
        if(currentModule.Hash!=mod.Hash){controllerMessage="This mod changed; select the current control again.";return;}
        if(!direct&&value==null&&(cmd.Argument.Length>0||cmd.Choices.Count>0)){StartPicker(mod,cmd);return;}
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
        if(index==0&&stages.Length==2&&stages.Contains("player",StringComparer.OrdinalIgnoreCase)&&stages.Contains("team",StringComparer.OrdinalIgnoreCase))
        {RenderPlayerTeamPicker(view,stages);return;}
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
    private void RenderPlayerTeamPicker(Rect view,string[] stages)
    {
        GUI.Label(new Rect(view.x,view.y,view.width-110,38),pickerCommand.Label+" / PLAYER + TEAM",titleStyle);
        if(Button(new Rect(view.xMax-100,view.y,100,32),"CANCEL")){pickerCommand=null;return;}
        var players=PickerOptions("player",stages).ToArray();
        var teams=PickerOptions("team",stages).ToArray();
        bool stacked=teams.Length>0&&(view.width*.6f)/teams.Length<68;
        float rowHeight=stacked?78:42;
        var area=new Rect(view.x,view.y+48,view.width,view.height-48);
        pickScroll=GUI.BeginScrollView(area,pickScroll,new Rect(0,0,view.width-26,Math.Max(60,players.Length*rowHeight)),false,true);
        try
        {
            float w=view.width-30,labelWidth=stacked?w:w*.4f;
            for(int i=Math.Max(0,(int)(pickScroll.y/rowHeight)-1);i<Math.Min(players.Length,(int)((pickScroll.y+area.height)/rowHeight)+2);i++)
            {
                float y=i*rowHeight;
                GUI.Label(new Rect(0,y+4,labelWidth-6,32),players[i].label,bodyStyle);
                float x=stacked?0:labelWidth,bw=(stacked?w:w-labelWidth)/Math.Max(1,teams.Length);
                for(int t=0;t<teams.Length;t++)if(Button(new Rect(x+t*bw,y+(stacked?36:0),bw-6,34),teams[t].label,allowed:serverRequest==null))
                {
                    var command=pickerCommand;var module=pickerModule;
                    string args=stages[0].Equals("team",StringComparison.OrdinalIgnoreCase)?teams[t].value+" "+players[i].value:players[i].value+" "+teams[t].value;
                    pickerCommand=null;SubmitServerControl(module,command,args);return;
                }
            }
            if(players.Length==0||teams.Length==0)GUI.Label(new Rect(0,0,w,60),"No unambiguous players or active teams available.",smallStyle);
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
