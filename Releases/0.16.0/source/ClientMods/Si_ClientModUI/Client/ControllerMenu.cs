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
        pendingInputEntry=null;
        sectionControls=key=="debug"||key.StartsWith("mod:")&&localMods.Any(m=>"mod:"+LocalTitle(m)==key);
    }
    private void RenderModMenu()
    {
        var e=Event.current;
        if(!addEntryOpen&&e.type==EventType.KeyDown&&(e.keyCode==KeyCode.Return||e.keyCode==KeyCode.KeypadEnter))
        {e.Use();}
        var rect=WindowRect(Math.Min(1100,UiWidth-24),Math.Min(900,UiHeight-24));
        float x=rect.x,y=rect.y,w=rect.width,h=rect.height,pad=16;
        WindowSurface(rect);DragWindow(rect);Surface(new Rect(x+12,y,w-24,3),accent);
        GUI.Label(new Rect(x+pad,y+12,144,32),"MOD MENU",titleStyle);
        if(Button(new Rect(x+174,y+14,w-294,28),CompanionStatus,allowed:serverRequest==null&&!Game.GetIsSingleplayer(),
            description:"Check for Si_ServerModUI again. This sends discovery metadata only.",style:smallStyle))ResetCompanion();
        if(Button(new Rect(x+w-100,y+12,84,32),"CLOSE",style:tabStyle)){ClosePanel();return;}
        float toolWidth=(w-2*pad-32)/5;
        if(Button(new Rect(x+pad,y+54,toolWidth,34),"status",style:tabStyle))statusPickerOpen=!statusPickerOpen;
        if(Button(new Rect(x+pad+(toolWidth+8),y+54,toolWidth,34),"sidebar",sidebarMenuOpen,description:"hold F7 to open sidebar",style:tabStyle))
        {sidebarMenuOpen=!sidebarMenuOpen;settingsMenuOpen=false;addEntryOpen=false;}
        if(Button(new Rect(x+pad+2*(toolWidth+8),y+54,toolWidth,34),"notifications",notificationsEnabled,style:tabStyle))SetNotifications(!notificationsEnabled);
        if(Button(new Rect(x+pad+3*(toolWidth+8),y+54,toolWidth,34),"debug",menuTab=="debug",style:tabStyle))
        {SelectTab("debug");sidebarMenuOpen=settingsMenuOpen=false;}
        if(Button(new Rect(x+pad+4*(toolWidth+8),y+54,toolWidth,34),"settings",settingsMenuOpen,style:tabStyle))
        {settingsMenuOpen=!settingsMenuOpen;sidebarMenuOpen=false;addEntryOpen=false;}
        if(sidebarMenuOpen){RenderSidebarMenu(new Rect(x+pad,y+108,w-2*pad,h-128));return;}
        if(settingsMenuOpen){RenderSettings(new Rect(x+pad,y+108,w-2*pad,h-128));return;}

        var tabs=CachedMenuTabs(out bool hasAdminCommands,out bool showAdmin);
        if(menuTab=="admin"&&!showAdmin)SelectTab("help");
        if(menuTab=="admin"&&!hasAdminCommands)sectionControls=true;
        if(menuTab!="debug"&&!tabs.Any(t=>t.key==menuTab))SelectTab("help");
        var tabsRect=new Rect(x+pad,y+102,w-2*pad,60);
        float tabTotal=menuTabTotal;
        moduleScroll=GUI.BeginScrollView(tabsRect,moduleScroll,new Rect(0,0,tabTotal,42),true,false);
        try
        {
            float left=0;
            for(int tabIndex=0;tabIndex<tabs.Count;tabIndex++)
            {
                var tab=tabs[tabIndex];float width=menuTabWidths[tabIndex];
                if(Tab(new Rect(left,0,width,38),tab.title,menuTab==tab.key)&&menuTab!=tab.key)SelectTab(tab.key);
                left+=width+8;
            }
        }
        finally{GUI.EndScrollView();}
        if(addEntryOpen){RenderAddEntry(new Rect(x+pad,y+172,w-2*pad,h-240));return;}
        bool fullProvider=ProviderPanelActive;
        float split=fullProvider?y+h-232:FeedbackDivider(x+pad,y+218,y+h-114,w-2*pad);
        var content=new Rect(x+pad,y+218,w-2*pad,Math.Max(72,split-y-234));

        string title=menuTab=="debug"?"DEBUG":menuTab=="admin"?"ADMIN":menuTab=="help"?"HELP":menuTab.Substring(4);
        GUI.Label(new Rect(x+pad,y+170,118,32),title,smallStyle);
        if(menuTab=="help")
        {
            if(!RoleAllowed(roleFilter))roleFilter=CurrentRole;
            if((!showAdmin||!hasAdminCommands)&&roleFilter=="admin")roleFilter=CurrentRole;
            float left=x+pad+126;
            if(Tab(new Rect(left,y+166,96,34),"PLAYER",roleFilter=="player"))roleFilter="player";
            if(Tab(new Rect(left+102,y+166,112,34),"COMMANDER",roleFilter=="commander",RoleAllowed("commander")))roleFilter="commander";
            if(showAdmin&&hasAdminCommands&&Tab(new Rect(left+220,y+166,88,34),"ADMIN",roleFilter=="admin"))roleFilter="admin";
            if(Button(new Rect(x+w-pad-146,y+166,146,34),"ADD ENTRY",style:tabStyle))
            {addEntryOpen=true;addMod=selectedCommand?.Mod??"";addCommand=commandDraft;addDescription="";addRole=CurrentRole;addAdmin=addDebug=false;}
        }
        else
        {
            if(Tab(new Rect(x+pad+126,y+166,116,34),"COMMANDS",!sectionControls))sectionControls=false;
            if(Tab(new Rect(x+pad+250,y+166,116,34),"CONTROLS",sectionControls))sectionControls=true;
        }

        if(menuTab.StartsWith("mod:"))
        {
            MelonModArray(out var providers);
            if(RenderProviderPanel(providers,new Rect(x+pad,y+210,w-2*pad,h-230),!sectionControls))return;
        }

        if(sectionControls&&menuTab!="help")
        {
            MelonModArray(out var mods);
            if(menuTab=="debug")RenderClientDebug(content);
            else RenderLocalControls(mods,content,Game.GetIsSingleplayer()?null:menuTab=="admin"?(bool?)true:false,false);
        }
        else
        {
            commandSearch=GUI.TextField(new Rect(content.x,content.y,content.width,30),commandSearch,160,inputStyle);
            RenderCommandRows(new Rect(content.x,content.y+40,content.width,Math.Max(40,content.height-40)));
        }
        float feedbackY=split+10;
        GUI.Label(new Rect(x+pad,feedbackY,w-2*pad,24),"RECEIVED FEEDBACK",smallStyle);
        RenderFeedback(new Rect(x+pad,feedbackY+28,w-2*pad,Math.Max(22,y+h-112-feedbackY-28)));
        GUI.SetNextControlName("mod-menu-command");
        commandDraft=GUI.TextField(new Rect(x+pad,y+h-100,w-2*pad-(pendingInputEntry==null?104:204),34),commandDraft,160,inputStyle);
        if(focusCommandDraft){GUI.FocusControl("mod-menu-command");focusCommandDraft=false;}
        if(Button(new Rect(x+w-pad-96,y+h-100,96,34),"SEND",allowed:ValidChat(commandDraft),style:tabStyle))SubmitDraft();
        if(pendingInputEntry!=null&&Button(new Rect(x+w-pad-196,y+h-100,92,34),"CANCEL",style:tabStyle))
        {pendingInputEntry=null;commandDraft="";controllerMessage="Input cancelled.";}
        GUI.Label(new Rect(x+pad,y+h-56,w-2*pad,46),controllerMessage.Length==0?"Type any chat message or command. SEND or Enter submits it.":controllerMessage,smallStyle);
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
        if(menuTab=="help")return roleFilter=="admin"?AdminCommand(c):(!AdminCommand(c)||Game.GetIsSingleplayer())&&
            (c.Role=="shared"||c.Role==roleFilter||Game.GetIsSingleplayer()&&c.Role=="admin"&&roleFilter=="player");
        return (!AdminCommand(c)||Game.GetIsSingleplayer())&&menuTab=="mod:"+ModLabel(c.Mod);
    }
    private void RenderCommandRows(Rect view)
    {
        string search=commandSearch.Trim();
        float width=view.width-26,top=0;
        CachedCommandRows(width,out var rows,out var heights);
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
                    string label=row.Source=="server-menu"&&!string.IsNullOrWhiteSpace(row.Label)?row.Label:row.Command;
                    if(Button(new Rect(4,top+4,242,height-14),label,selectedCommand==row,scoped,"Run "+row.Command+". Commands needing arguments prompt for input."))
                    {ClickCommand(row);break;}
                    GUI.Label(new Rect(258,top+4,width-264,24),ModLabel(row.Mod)+(string.IsNullOrWhiteSpace(row.Group)?"":" / "+ModLabel(row.Group))+" / "+row.Source,smallStyle);
                    GUI.Label(new Rect(258,top+30,Math.Max(60,width-364),height-36),row.Description,smallStyle);
                    if(CustomCommand(row)&&Button(new Rect(width-94,top+6,90,30),"REMOVE",style:tabStyle)){RemoveCustomCommand(row);break;}
                }
                top+=height;
            }
            if(rows.Length==0)GUI.Label(new Rect(0,0,width,100),menuTab=="debug"?"No debug options have been supplied for your current access.":"No commands are known for this role yet. Mods can supply controls; Help lets you remember your own entries.",smallStyle);
        }
        finally{GUI.EndScrollView();}
    }
    private void RenderFeedback(Rect view)
    {
        float width=view.width-26;CachedFeedback(width,out string text,out float height);
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


