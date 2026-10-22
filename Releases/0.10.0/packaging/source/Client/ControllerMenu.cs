using Il2Cpp;
using UnityEngine;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private bool sectionControls;
    private bool focusCommandDraft;
    private bool Tab(Rect rect,string label,bool selected,bool allowed=true,string description=null)
    {
        bool clicked=Button(rect,label,selected,allowed,description,tabStyle,true);
        if(selected)Surface(new Rect(rect.x+6,rect.y+rect.height-3,rect.width-12,3),accent);
        return clicked;
    }
    private void SelectTab(string key)
    {
        menuTab=key;commandScroll=localScroll=Vector2.zero;selectedCommand=null;commandDraft="";commandSearch="";
        sectionControls=key.StartsWith("mod:")&&localMods.Any(m=>"mod:"+LocalTitle(m)==key);
    }
    private void RenderModMenu()
    {
        var e=Event.current;
        if(!addEntryOpen&&e.type==EventType.KeyDown&&(e.keyCode==KeyCode.Return||e.keyCode==KeyCode.KeypadEnter))
        {e.Use();}
        var rect=WindowRect(Math.Min(1100,Screen.width-24),Math.Min(900,Screen.height-24));
        float x=rect.x,y=rect.y,w=rect.width,h=rect.height,pad=16;
        WindowSurface(rect);DragWindow(rect);Surface(new Rect(x+12,y,w-24,3),accent);
        GUI.Label(new Rect(x+pad,y+12,144,32),"MOD MENU",titleStyle);
        if(Button(new Rect(x+166,y+12,78,32),"status",style:tabStyle))statusPickerOpen=!statusPickerOpen;
        Button(new Rect(x+250,y+12,84,32),"sidebar",description:"Sidebar controls will be added later.",style:tabStyle);
        if(Button(new Rect(x+340,y+12,112,32),"notifications",notificationsEnabled,style:tabStyle))SetNotifications(!notificationsEnabled);
        if(Button(new Rect(x+458,y+12,76,32),"debug",menuTab=="debug",style:tabStyle))SelectTab("debug");
        if(Button(new Rect(x+w-100,y+12,84,32),"CLOSE",style:tabStyle)){ClosePanel();return;}

        var known=AllCommands().ToArray();
        var tabs=new List<(string key,string title)>{("help","HELP")};
        tabs.AddRange(localMods.Select(m=>LocalTitle(m)).Concat(known.Where(c=>!AdminCommand(c)).Select(c=>ModLabel(c.Mod)))
            .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(m=>m).Select(m=>("mod:"+m,m)));
        if(CurrentAdmin)tabs.Add(("admin","ADMIN"));
        if(menuTab=="admin"&&!CurrentAdmin)SelectTab("help");
        if(menuTab!="debug"&&!tabs.Any(t=>t.key==menuTab))SelectTab("help");
        var tabsRect=new Rect(x+pad,y+58,w-2*pad,60);
        float tabTotal=tabs.Sum(t=>Math.Max(106,tabStyle.CalcSize(new GUIContent(t.title)).x+24)+8);
        moduleScroll=GUI.BeginScrollView(tabsRect,moduleScroll,new Rect(0,0,tabTotal,42),true,false);
        try
        {
            float left=0;
            foreach(var tab in tabs)
            {
                float width=Math.Max(106,tabStyle.CalcSize(new GUIContent(tab.title)).x+24);
                if(Tab(new Rect(left,0,width,38),tab.title,menuTab==tab.key)&&menuTab!=tab.key)SelectTab(tab.key);
                left+=width+8;
            }
        }
        finally{GUI.EndScrollView();}
        var content=new Rect(x+pad,y+174,w-2*pad,h-418);
        if(addEntryOpen){RenderAddEntry(new Rect(x+pad,y+128,w-2*pad,h-196));return;}

        string title=menuTab=="debug"?"DEBUG":menuTab=="admin"?"ADMIN":menuTab=="help"?"HELP":menuTab.Substring(4);
        GUI.Label(new Rect(x+pad,y+126,118,32),title,smallStyle);
        if(menuTab=="help")
        {
            if(roleFilter!="admin"&&roleFilter!=CurrentRole)roleFilter=CurrentRole;
            if(!CurrentAdmin&&roleFilter=="admin")roleFilter=CurrentRole;
            float left=x+pad+126;
            if(Tab(new Rect(left,y+122,96,34),"PLAYER",roleFilter=="player",CurrentRole=="player"))roleFilter="player";
            if(Tab(new Rect(left+102,y+122,112,34),"COMMANDER",roleFilter=="commander",CurrentRole=="commander"))roleFilter="commander";
            if(CurrentAdmin&&Tab(new Rect(left+220,y+122,88,34),"ADMIN",roleFilter=="admin"))roleFilter="admin";
            if(Button(new Rect(x+w-pad-146,y+122,146,34),"ADD ENTRY",style:tabStyle))
            {addEntryOpen=true;addMod=selectedCommand?.Mod??"";addCommand=commandDraft;addDescription="";addRole=CurrentRole;addAdmin=addDebug=false;}
        }
        else
        {
            if(Tab(new Rect(x+pad+126,y+122,116,34),"COMMANDS",!sectionControls))sectionControls=false;
            if(Tab(new Rect(x+pad+250,y+122,116,34),"CONTROLS",sectionControls))sectionControls=true;
        }

        if(sectionControls&&menuTab!="help")
        {
            MelonModArray(out var mods);
            RenderLocalControls(mods,content,menuTab=="admin"?(bool?)true:menuTab=="debug"?null:false,menuTab=="debug");
        }
        else
        {
            commandSearch=GUI.TextField(new Rect(content.x,content.y,content.width,30),commandSearch,160,inputStyle);
            RenderCommandRows(new Rect(content.x,content.y+40,content.width,Math.Max(40,content.height-40)));
        }
        float feedbackY=y+h-232;
        GUI.Label(new Rect(x+pad,feedbackY,w-2*pad,24),"RECEIVED FEEDBACK",smallStyle);
        RenderFeedback(new Rect(x+pad,feedbackY+28,w-2*pad,92));
        GUI.SetNextControlName("mod-menu-command");
        commandDraft=GUI.TextField(new Rect(x+pad,y+h-100,w-2*pad-104,34),commandDraft,160,inputStyle);
        if(focusCommandDraft){GUI.FocusControl("mod-menu-command");focusCommandDraft=false;}
        if(Button(new Rect(x+w-pad-96,y+h-100,96,34),"SEND",allowed:ValidCommand(commandDraft.Trim()),style:tabStyle))SubmitDraft();
        GUI.Label(new Rect(x+pad,y+h-56,w-2*pad,46),controllerMessage.Length==0?"Click a command to prepare it. SEND or Enter submits it.":controllerMessage,smallStyle);
    }
    private void MelonModArray(out MelonLoader.MelonMod[] mods)
    {
        mods=menuTab.StartsWith("mod:")?localMods.Where(m=>"mod:"+LocalTitle(m)==menuTab).ToArray():localMods.ToArray();
    }
    private bool InCommandView(CommandEntry c)
    {
        if(!CommandAllowed(c))return false;
        if(menuTab=="debug")return DebugCommand(c);
        if(menuTab=="admin")return AdminCommand(c);
        if(menuTab=="help")return roleFilter=="admin"?AdminCommand(c):!AdminCommand(c)&&(c.Role=="shared"||c.Role==roleFilter);
        return !AdminCommand(c)&&menuTab=="mod:"+ModLabel(c.Mod);
    }
    private void RenderCommandRows(Rect view)
    {
        string search=commandSearch.Trim();
        var rows=AllCommands().Where(InCommandView).Where(c=>search.Length==0||(c.Command+" "+ModLabel(c.Mod)+" "+c.Description).Contains(search,StringComparison.OrdinalIgnoreCase))
            .OrderBy(c=>ModLabel(c.Mod)).ThenBy(c=>c.Group).ThenBy(c=>c.Command,StringComparer.OrdinalIgnoreCase).ToArray();
        float width=view.width-26,top=0;
        var heights=rows.Select(r=>Math.Max(68,smallStyle.CalcHeight(new GUIContent(r.Description),Math.Max(80,width-260))+36)).ToArray();
        commandScroll=GUI.BeginScrollView(view,commandScroll,new Rect(0,0,width,Math.Max(90,heights.Sum())),false,true);
        try
        {
            for(int i=0;i<rows.Length;i++)
            {
                var row=rows[i];float height=heights[i];
                if(top+height>=commandScroll.y&&top<=commandScroll.y+view.height)
                {
                    Surface(new Rect(0,top,width,height-6),Color.Lerp(surface,accent,.035f));
                    bool scoped=row.Source!="menu"||activeExchange?.Entry.Mod==row.Mod&&clock.Elapsed.TotalSeconds-activeExchange.At<60;
                    if(Button(new Rect(4,top+4,242,height-14),row.Command,selectedCommand==row,scoped,"Prepare this command; sending requires SEND or Enter."))
                        ClickCommand(row);
                    GUI.Label(new Rect(258,top+4,width-264,24),ModLabel(row.Mod)+(string.IsNullOrWhiteSpace(row.Group)?"":" / "+ModLabel(row.Group))+" / "+row.Source,smallStyle);
                    GUI.Label(new Rect(258,top+30,width-264,height-36),row.Description,smallStyle);
                }
                top+=height;
            }
            if(rows.Length==0)GUI.Label(new Rect(0,0,width,100),menuTab=="debug"?"No debug options have been supplied for your current access.":"No commands are known for this role yet. Mods can supply controls; Help lets you remember your own entries.",smallStyle);
        }
        finally{GUI.EndScrollView();}
    }
    private void RenderFeedback(Rect view)
    {
        var lines=feedback.Where(f=>(!f.AdminOnly||CurrentAdmin)&&(menuTab=="help"||menuTab=="admin"||menuTab=="debug"||menuTab=="mod:"+ModLabel(f.Mod))).TakeLast(80);
        string text=string.Join("\n\n",lines.Select(f=>"["+ModLabel(f.Mod)+"] "+f.Text));
        if(text.Length==0)text="No feedback received. The menu does not send discovery queries.";
        float width=view.width-26,height=smallStyle.CalcHeight(new GUIContent(text),width);
        feedbackScroll=GUI.BeginScrollView(view,feedbackScroll,new Rect(0,0,width,height),false,true);
        try{GUI.Label(new Rect(0,0,width,height),text,smallStyle);}finally{GUI.EndScrollView();}
    }
    private void RenderAddEntry(Rect view)
    {
        float x=view.x,y=view.y,w=view.width;
        GUI.Label(new Rect(x,y,w,32),"REMEMBER A CHAT CONTROL",titleStyle);
        GUI.Label(new Rect(x,y+40,w,44),"Stored for this server, or singleplayer. Saving never sends the command.",smallStyle);
        GUI.Label(new Rect(x,y+94,120,30),"Mod",bodyStyle);
        addMod=GUI.TextField(new Rect(x+130,y+90,w-130,34),addMod,80,inputStyle);
        GUI.Label(new Rect(x,y+140,120,30),"Command",bodyStyle);
        addCommand=GUI.TextField(new Rect(x+130,y+136,w-130,34),addCommand,160,inputStyle);
        GUI.Label(new Rect(x,y+186,120,30),"Description",bodyStyle);
        addDescription=GUI.TextField(new Rect(x+130,y+182,w-130,34),addDescription,300,inputStyle);
        float left=x;
        foreach(string role in new[]{"player","commander","shared","admin"})
        {
            if(role=="admin"&&!CurrentAdmin)continue;
            if(Tab(new Rect(left,y+234,126,34),role.ToUpperInvariant(),addRole==role))addRole=role;
            left+=134;
        }
        if(CurrentAdmin&&Button(new Rect(x,y+284,210,34),addAdmin?"ADMIN ONLY: YES":"ADMIN ONLY: NO",addAdmin))addAdmin=!addAdmin;
        if(Button(new Rect(x+220,y+284,180,34),addDebug?"DEBUG: YES":"DEBUG: NO",addDebug))addDebug=!addDebug;
        if(Button(new Rect(x,y+342,160,36),"SAVE ENTRY",allowed:ValidCommand(addCommand.Trim())&&!string.IsNullOrWhiteSpace(addMod)))SaveUserCommand();
        if(Button(new Rect(x+172,y+342,130,36),"CANCEL"))addEntryOpen=false;
        GUI.Label(new Rect(x,y+390,w,70),controllerMessage,smallStyle);
    }
}

