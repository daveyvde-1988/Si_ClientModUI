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
    private string mainMenu="HELP",settingsPage="GENERAL",commandSelection="all",modSelection="";
    private void SetModCategory(bool admin)
    {modAdmin=admin;sectionControls=true;serverPanelMode="";panelRowsKey="";serverPanelScroll=localScroll=Vector2.zero;pickerCommand=null;dropdownKey="";InvalidateUi(true);}
    private bool modAdmin;
    private bool commandAdmin,clientNavOpen=true,serverNavOpen=true;
    private Vector2 navigationScroll,helpScroll,settingsScroll;
    private string navigationKey=>mainMenu=="COMMANDS"?commandSelection:modSelection;
    private void RestoreMenuSelection()
    {
        if(layout.RememberLastMenu)
        {
            mainMenu=new[]{"HELP","COMMANDS","MODS","SETTINGS"}.Contains(layout.LastMainMenu)?layout.LastMainMenu:"HELP";
            settingsPage=layout.LastSettingsPage??"GENERAL";commandSelection=layout.LastCommandSelection??"all";modSelection=layout.LastModSelection??"";
        }
        else {mainMenu="HELP";settingsPage="GENERAL";commandSelection="all";modSelection="";}
        SetNavigationContext();
    }
    private void SaveMenuSelection()
    {
        if(!panelOpen)return;
        layout.LastMainMenu=mainMenu;layout.LastSettingsPage=settingsPage;layout.LastCommandSelection=commandSelection;layout.LastModSelection=modSelection;
        placementDirty=true;
    }
    private void SetNavigationContext()
    {
        modAdmin=false;serverPanelMode="";
        sidebarMenuOpen=settingsMenuOpen=false;sectionControls=mainMenu=="MODS";commandScroll=localScroll=Vector2.zero;
        var owner=localMods.FirstOrDefault(m=>LocalScope(m)==navigationKey);
        menuTab=owner!=null?"mod:"+LocalTitle(owner):"help";
        selectedCommand=null;pendingInputEntry=null;InvalidateUi(true);
    }
    private void RenderModMenu()
    {
        var rect=WindowRect(Math.Min(1200,UiWidth-24),Math.Min(900,UiHeight-24));
        WindowSurface(rect);DragWindow(rect);
        GUI.Label(new Rect(rect.x+16,rect.y+12,180,32),"MOD UI 0.17.1",titleStyle);
        if(Button(new Rect(rect.x+210,rect.y+14,Math.Max(100,rect.width-330),28),CompanionStatus,
            allowed:serverRequest==null&&!Game.GetIsSingleplayer(),description:"Recheck the optional server companion.",style:smallStyle))ResetCompanion();
        if(Button(new Rect(rect.xMax-96,rect.y+12,80,32),"CLOSE",style:tabStyle)){ClosePanel();return;}
        string[] pages={"HELP","COMMANDS","MODS","SETTINGS"};
        float tw=(rect.width-44)/4;
        for(int i=0;i<pages.Length;i++) if(Tab(new Rect(rect.x+16+i*(tw+4),rect.y+54,tw,36),pages[i],mainMenu==pages[i]))
        {mainMenu=pages[i];addEntryOpen=false;SetNavigationContext();}
        var body=new Rect(rect.x+16,rect.y+104,rect.width-32,rect.height-120);
        if(mainMenu=="HELP"){RenderHelpPage(body);return;}
        float navWidth=Math.Clamp(body.width*.24f,175,240);
        RenderNavigation(new Rect(body.x,body.y,navWidth,body.height));
        var right=new Rect(body.x+navWidth+12,body.y,body.width-navWidth-12,body.height);
        if(mainMenu=="SETTINGS"){RenderSettingsPage(right);return;}
        if(mainMenu=="COMMANDS")
        {
            if(addEntryOpen){RenderAddEntry(right);return;}
            if(!CurrentAdmin)commandAdmin=false;
            if(commandSelection=="all")GUI.Label(new Rect(right.x,right.y,200,30),"ALL COMMANDS",titleStyle);
            else
            {
                if(Tab(new Rect(right.x,right.y,110,32),"PLAYER",!commandAdmin)) {commandAdmin=false;InvalidateUi();}
                if(CurrentAdmin&&Tab(new Rect(right.x+118,right.y,110,32),"ADMIN",commandAdmin)){commandAdmin=true;InvalidateUi();}
            }
            if(Button(new Rect(right.xMax-122,right.y,122,32),"ADD ENTRY",style:tabStyle))
            {addEntryOpen=true;addMod=selectedCommand?.Mod??"";addCommand=commandDraft;addDescription="";addRole=CurrentRole;addAdmin=addDebug=false;}
            float split=FeedbackDivider(right.x,right.y+116,right.yMax-100,right.width,"command-console",100,100);
            commandSearch=GUI.TextField(new Rect(right.x,right.y+42,right.width,30),commandSearch,160,inputStyle);
            RenderCommandRows(new Rect(right.x,right.y+82,right.width,Math.Max(30,split-right.y-92)));
            RenderCommandDock(new Rect(right.x,split+8,right.width,right.yMax-split-8));
            return;
        }
        if(!CurrentAdmin)modAdmin=false;
        if(Tab(new Rect(right.x,right.y,110,32),"PLAYER",!modAdmin)){SetModCategory(false);}
        if(CurrentAdmin&&Tab(new Rect(right.x+118,right.y,110,32),"ADMIN",modAdmin)){SetModCategory(true);}
        right=new Rect(right.x,right.y+44,right.width,right.height-44);
        var local=localMods.FirstOrDefault(m=>LocalScope(m)==modSelection);
        if(local!=null)
        {
            var def=ClientModControls.For(local);
            if(def==null&&!ModCategories(local).Any()){GUI.Label(right,"This client mod has not supplied a panel or visible settings. Known commands are available under COMMANDS.",bodyStyle);return;}
            var providerView=right;
            if(def?.DrawCommands!=null&&def.PanelAdminOnly==modAdmin)
            {
                if(Tab(new Rect(right.x,right.y,150,32),"PANEL",sectionControls))sectionControls=true;
                if(Tab(new Rect(right.x+158,right.y,150,32),"EXTRA PANEL",!sectionControls))sectionControls=false;
                providerView=new Rect(right.x,right.y+44,right.width,right.height-44);
            }
            if(!RenderProviderPanel(new[]{local},providerView,!sectionControls))RenderLocalControls(new[]{local},providerView,modAdmin,false);
        }
        else if(companionReady&&modSelection.StartsWith("server:")&&serverCatalog.Modules.TryGetValue(modSelection.Substring(7),out var remote))
            RenderServerPanel(remote,right);
        else GUI.Label(right,"Select a mod on the left. Panels are built from information provided by that mod.",bodyStyle);
    }
    private void RenderHelpPage(Rect view)
    {
        const string help="Si_ClientModUI 0.17.1\nOptional server addon: Si_ServerModUI 2.0.0\n\nOne place for your mods' commands, settings and information. Each mod decides which features it provides and who may use them.\n\nCOMMANDS: find known commands by client or server mod, or use ALL. Click a complete command to run it; commands needing values prompt first. The chat box accepts normal chat too.\n\nMODS: open interactive panels supplied by client mods or the server. Switches, dropdowns and value controls use the mod's real commands.\n\nSETTINGS: adjust appearance, status values, sidebar, notifications and diagnostics.\n\nPress F6 to open or close. FPS players can hold F7 to use sidebar buttons with the mouse. Release F7 to return to the game.\n\nLocal features and ordinary chat work without a server addon. The header shows whether the addon was detected. Some internal commands remain unknown unless a mod describes them. Your server permissions always apply.";
        float height=MeasureText(bodyStyle,help,view.width-30);
        helpScroll=GUI.BeginScrollView(view,helpScroll,new Rect(0,0,view.width-24,height+16),false,true);
        try{GUI.Label(new Rect(4,4,view.width-32,height),help,bodyStyle);}finally{GUI.EndScrollView();}
    }
    private (string key,string label,bool local)[] navEntries=Array.Empty<(string,string,bool)>();
    private long navRevision=-1;
    private void RenderNavigation(Rect view)
    {
        CommandSnapshot();
        if(navRevision!=uiRevision)
        {
            var entries=new List<(string,string,bool)>();
            entries.AddRange(localMods.Select(m=>(LocalScope(m),LocalTitle(m),true)));
            if(companionReady)entries.AddRange(serverCatalog.Modules.Values.Where(m=>m.Id!="_match").OrderBy(m=>m.Priority).Select(m=>("server:"+m.Id,ModLabel(m.Name),false)));
            foreach(var c in AllCommands().Where(c=>!c.Scope.StartsWith("local:")))
                if(!entries.Any(e=>!e.Item3&&e.Item2.Equals(ModLabel(c.Mod),StringComparison.OrdinalIgnoreCase)))entries.Add(("known:"+ModLabel(c.Mod),ModLabel(c.Mod),false));
            navEntries=entries.ToArray();navRevision=uiRevision;
        }
        var settings=new[]{"GENERAL","STATUS","SIDEBAR","NOTIFICATIONS","DEBUG"};
        int count=mainMenu=="SETTINGS"?settings.Length:2+(mainMenu=="COMMANDS"?1:0)+navEntries.Count(e=>e.local?clientNavOpen:serverNavOpen);
        navigationScroll=GUI.BeginScrollView(view,navigationScroll,new Rect(0,0,view.width-22,count*40+10),false,true);
        try
        {
            float y=0,w=view.width-26;
            if(mainMenu=="SETTINGS")
            {
                foreach(string page in settings){if(Button(new Rect(0,y,w,34),page,settingsPage==page)){settingsPage=page;editingSidebar=false;sidebarMenuScroll=Vector2.zero;}y+=40;}
                return;
            }
            if(mainMenu=="COMMANDS"){if(Button(new Rect(0,y,w,34),"ALL",navigationKey=="all")){commandSelection="all";SetNavigationContext();}y+=40;}
            for(int group=0;group<2;group++)
            {
                bool local=group==0,expanded=local?clientNavOpen:serverNavOpen;
                if(Button(new Rect(0,y,w,34),(expanded?"− ":"+ ")+(local?"CLIENT MODS":"SERVER MODS")))
                {if(local)clientNavOpen=!clientNavOpen;else serverNavOpen=!serverNavOpen;}
                y+=40;if(!expanded)continue;
                foreach(var entry in navEntries.Where(e=>e.local==local))
                {
                    if(Button(new Rect(10,y,w-10,34),entry.label,navigationKey==entry.key))
                    {if(mainMenu=="COMMANDS")commandSelection=entry.key;else modSelection=entry.key;SetNavigationContext();}
                    y+=40;
                }
            }
        }
        finally{GUI.EndScrollView();}
    }
    private void RenderCommandDock(Rect area)
    {
        GUI.Label(new Rect(area.x,area.y,area.width,24),"CHAT / MESSAGES",smallStyle);
        RenderFeedback(new Rect(area.x,area.y+28,area.width,Math.Max(20,area.height-110)));
        GUI.SetNextControlName("mod-menu-command");
        commandDraft=GUI.TextField(new Rect(area.x,area.yMax-76,area.width-(pendingInputEntry==null?100:190),32),commandDraft,160,inputStyle);
        if(focusCommandDraft){GUI.FocusControl("mod-menu-command");focusCommandDraft=false;}
        if(Button(new Rect(area.xMax-90,area.yMax-76,90,32),"SEND",allowed:ValidChat(commandDraft),style:tabStyle))SubmitDraft();
        if(pendingInputEntry!=null&&Button(new Rect(area.xMax-184,area.yMax-76,88,32),"CANCEL")){pendingInputEntry=null;commandDraft="";}
        GUI.Label(new Rect(area.x,area.yMax-38,area.width,38),controllerMessage.Length==0?"Type a chat message or command; Send or Enter submits.":controllerMessage,smallStyle);
    }
    private void MelonModArray(out MelonLoader.MelonMod[] mods)
    {
        mods=menuTab.StartsWith("mod:")?localMods.Where(m=>"mod:"+LocalTitle(m)==menuTab).ToArray():localMods.ToArray();
    }
    private bool InCommandView(CommandEntry c)
    {
        if(!CommandAllowed(c))return false;
        if(commandSelection=="all")return true;
        if(commandSelection.StartsWith("local:")) {if(c.Scope!=commandSelection)return false;}
        else if(commandSelection.StartsWith("server:")) {if(c.Scope.StartsWith("local:")||c.Group!=commandSelection.Substring(7))return false;}
        else if(commandSelection.StartsWith("known:")&&!ModLabel(c.Mod).Equals(commandSelection.Substring(6),StringComparison.OrdinalIgnoreCase))return false;
        return commandAdmin?AdminCommand(c):!AdminCommand(c);
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
                    GUI.Label(new Rect(258,top+4,width-264,24),ModLabel(row.Mod),smallStyle);
                    GUI.Label(new Rect(258,top+30,Math.Max(60,width-364),height-36),row.Description,smallStyle);
                    if(CustomCommand(row)&&Button(new Rect(width-94,top+6,90,30),"REMOVE",style:tabStyle)){RemoveCustomCommand(row);break;}
                }
                top+=height;
            }
            if(rows.Length==0)GUI.Label(new Rect(0,0,width,100),menuTab=="debug"?"No debug options have been supplied for your current access.":"No commands are known for this selection. Mods can describe commands, or you can add a custom entry.",smallStyle);
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


