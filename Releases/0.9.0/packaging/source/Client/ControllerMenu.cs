using Il2Cpp;
using UnityEngine;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private bool Tab(Rect rect,string label,bool selected,bool allowed=true,string description=null)
    {
        bool clicked=Button(rect,label,selected,allowed,description,tabStyle,true);
        if(selected)Surface(new Rect(rect.x+6,rect.y+rect.height-3,rect.width-12,3),accent);
        return clicked;
    }
    private void RenderModMenu()
    {
        var rect=WindowRect(Math.Min(1100,Screen.width-24),Math.Min(940,Screen.height-24));
        float x=rect.x,y=rect.y,w=rect.width,h=rect.height,pad=20;
        WindowSurface(rect);DragWindow(rect);Surface(new Rect(x+12,y,w-24,3),accent);
        GUI.Label(new Rect(x+pad,y+12,180,32),"MOD CONTROLLER",titleStyle);
        if(Button(new Rect(x+210,y+12,90,32),"status",style:tabStyle))statusPickerOpen=!statusPickerOpen;
        if(panelCommander&&Button(new Rect(x+308,y+12,90,32),"sidebar",sidebarMode,style:tabStyle))SetSidebarMode(!sidebarMode);
        if(Button(new Rect(x+406,y+12,154,32),"notifications",notificationsEnabled,style:tabStyle))SetNotifications(!notificationsEnabled);
        if(Button(new Rect(x+w-116,y+12,96,32),"CLOSE",style:tabStyle)){ClosePanel();return;}
        var p=Player.CurrentPlayer;
        float mw=(w-2*pad-24)/3;
        Metric(new Rect(x+pad,y+54,mw,62),"SCORE",p.Score.ToString("N0"));
        Metric(new Rect(x+pad+mw+12,y+54,mw,62),"PERSONAL CREDITS",ObservedValue("koh.credits"));
        Metric(new Rect(x+pad+2*(mw+12),y+54,mw,62),"FPS SQUAD",SquadQuantity(out var detail));
        GUI.Label(new Rect(x+pad,y+122,w-2*pad,40),"Rank: "+ObservedValue("ranks.rank")+"  |  "+(panelCommander?"Commander":detail),smallStyle);

        var tabs=new List<(string key,string title)>{("help","HELP")};
        if(Game.GetIsSingleplayer()&&localMods.Count>0)tabs.Add(("sp","SP MODS"));
        tabs.AddRange(AllCommands().Select(c=>c.Mod).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(m=>m).Select(m=>(m,m)));
        if(!tabs.Any(t=>t.key==menuTab))menuTab="help";
        var tabsRect=new Rect(x+pad,y+166,w-2*pad,62);
        float tabTotal=tabs.Sum(t=>Math.Max(112,tabStyle.CalcSize(new GUIContent(t.title)).x+26)+8);
        moduleScroll=GUI.BeginScrollView(tabsRect,moduleScroll,new Rect(0,0,tabTotal,44),true,false);
        try
        {
            float left=0;
            foreach(var tab in tabs)
            {
                float tw=Math.Max(112,tabStyle.CalcSize(new GUIContent(tab.title)).x+26);
                if(Tab(new Rect(left,0,tw,40),tab.title,menuTab==tab.key)&&menuTab!=tab.key)
                {menuTab=tab.key;commandScroll=Vector2.zero;selectedCommand=null;commandDraft="";commandSearch="";}
                left+=tw+8;
            }
        }
        finally{GUI.EndScrollView();}
        if(menuTab=="sp"){RenderLocalMods(new Rect(x+pad,y+242,w-2*pad,h-326));return;}
        GUI.Label(new Rect(x+pad,y+236,w-2*pad,40),
            menuTab=="help"?"Click a command to send it. The server validates permissions. Catalog entries do not prove a mod is installed.":"Chat controls. Values and menu choices come from received feedback; nothing refreshes automatically.",smallStyle);
        commandSearch=GUI.TextField(new Rect(x+pad,y+278,w-2*pad-264,32),commandSearch,160,inputStyle);
        foreach(var (key,label,offset) in new[]{("all","ALL",0),("player","PLAYER",88),("admin","ADMIN",176)})
            if(Tab(new Rect(x+w-pad-256+offset,y+278,80,32),label,roleFilter==key))roleFilter=key;
        float contentTop=y+320,contentBottom=y+h-244;
        RenderCommandRows(new Rect(x+pad,contentTop,w-2*pad,Math.Max(60,contentBottom-contentTop)));
        GUI.Label(new Rect(x+pad,contentBottom+6,w-2*pad,26),"RECEIVED FEEDBACK",smallStyle);
        RenderFeedback(new Rect(x+pad,contentBottom+34,w-2*pad,100));
        float draftY=y+h-102;
        commandDraft=GUI.TextField(new Rect(x+pad,draftY,w-2*pad-104,34),commandDraft,160,inputStyle);
        if(Button(new Rect(x+w-pad-96,draftY,96,34),"SEND",allowed:ValidCommand(commandDraft.Trim()),style:tabStyle))
        {
            var entry=selectedCommand??AllCommands().FirstOrDefault(c=>RootCommand(c.Command)==RootCommand(commandDraft))??
                new CommandEntry{Mod=menuTab=="help"?"server":menuTab,Command=commandDraft};
            SendCommand(entry,commandDraft);
        }
        GUI.Label(new Rect(x+pad,y+h-60,w-2*pad,46),controllerMessage,smallStyle);
    }
    private void RenderCommandRows(Rect view)
    {
        string search=commandSearch.Trim();
        var rows=AllCommands().Where(c=>(menuTab=="help"||c.Mod==menuTab)&&
            (roleFilter=="all"||(roleFilter=="admin"?(c.Role=="admin"||c.Command.StartsWith("!")):c.Role!="admin"&&!c.Command.StartsWith("!")))&&
            (search.Length==0||(c.Command+" "+c.Mod+" "+c.Description).Contains(search,StringComparison.OrdinalIgnoreCase)))
            .OrderBy(c=>c.Source=="menu"?0:1).ThenBy(c=>c.Mod).ThenBy(c=>c.Command,StringComparer.OrdinalIgnoreCase).ToArray();
        float width=view.width-26,top=0;
        var heights=rows.Select(r=>Math.Max(66,smallStyle.CalcHeight(new GUIContent(r.Description),Math.Max(80,width-260))+34)).ToArray();
        commandScroll=GUI.BeginScrollView(view,commandScroll,new Rect(0,0,width,heights.Sum()),false,true);
        try
        {
            for(int i=0;i<rows.Length;i++)
            {
                var row=rows[i];float height=heights[i];
                if(top+height>=commandScroll.y&&top<=commandScroll.y+view.height)
                {
                    Surface(new Rect(0,top,width,height-6),Color.Lerp(surface,accent,.035f));
                    bool scoped=row.Source!="menu"||activeExchange?.Mod==row.Mod&&clock.Elapsed.TotalSeconds-activeExchange.At<60;
                    if(Button(new Rect(4,top+4,176,height-14),row.Command,false,scoped,row.Description))
                    {ClickCommand(row);break;}
                    if(Button(new Rect(184,top+4,64,height-14),"EDIT",false,scoped,"Enter arguments before sending this command.",tabStyle))
                    {selectedCommand=row;commandDraft=row.Command;}
                    GUI.Label(new Rect(258,top+4,width-264,22),row.Mod+(row.Role=="admin"?" / admin":"")+(row.Source=="menu"?" / received choice":""),smallStyle);
                    GUI.Label(new Rect(258,top+28,width-264,height-34),row.Description,smallStyle);
                }
                top+=height;
            }
            if(rows.Length==0)GUI.Label(new Rect(0,0,width,65),"No matching commands. Local custom catalogs can add commands for other servers.",smallStyle);
        }
        finally{GUI.EndScrollView();}
    }
    private void RenderFeedback(Rect view)
    {
        var lines=feedback.Where(f=>menuTab=="help"||f.Mod==menuTab).TakeLast(80).ToArray();
        string text=string.Join("\n\n",lines.Select(f=>"["+f.Mod+"] "+f.Text));
        if(text.Length==0)text="No feedback received yet. Click a mod command to request its native menu or status.";
        float width=view.width-26,height=smallStyle.CalcHeight(new GUIContent(text),width);
        feedbackScroll=GUI.BeginScrollView(view,feedbackScroll,new Rect(0,0,width,height),false,true);
        try{GUI.Label(new Rect(0,0,width,height),text,smallStyle);}finally{GUI.EndScrollView();}
    }
    private void Metric(Rect rect,string label,string value)
    {
        Surface(rect,Color.Lerp(surface,accent,.06f));
        GUI.Label(new Rect(rect.x+12,rect.y+4,rect.width-24,22),label,smallStyle);
        GUI.Label(new Rect(rect.x+12,rect.y+27,rect.width-24,31),value,bodyStyle);
    }
}

