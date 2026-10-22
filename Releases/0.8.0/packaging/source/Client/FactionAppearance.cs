using Il2Cpp;
using UnityEngine;

namespace SiSquadHud;

public sealed partial class SquadHud
{
    private GUIStyle bodyStyle,titleStyle,smallStyle,buttonStyle,surfaceStyle,tabStyle,tableStyle,tableHeaderStyle,weaveStyle,inputStyle;
    private bool weaveDisabled;
    private Texture2D panelMask,panelWeave,inputBackground;
    private Color accent=new(0.5f,0.85f,1f,1f),surface=new(0.08f,0.07f,0.06f,0.90f);
    private string faction="FIELD COMMAND";
    private int appearanceTeam=int.MinValue,appearanceFont;
    private Vector2 optionScroll;
    private string hoverCandidate="",hoverText="",hoverKey="";
    private double hoverSince;

    private void ResetAppearance()
    {
        if(panelMask!=null)UnityEngine.Object.Destroy(panelMask);
        if(panelWeave!=null)UnityEngine.Object.Destroy(panelWeave);
        if(inputBackground!=null)UnityEngine.Object.Destroy(inputBackground);
        panelMask=panelWeave=inputBackground=null;bodyStyle=titleStyle=smallStyle=buttonStyle=surfaceStyle=tabStyle=tableStyle=tableHeaderStyle=weaveStyle=inputStyle=null;appearanceTeam=int.MinValue;
        ResetSidebarIcons();hoverKey="";
    }
    private void Appearance()
    {
        var p=Player.CurrentPlayer;var team=p==null?null:p.Team;
        int id=team==null?-1:team.Index;
        int font=Math.Clamp(fontPreference?.Value??18,16,28);
        if(bodyStyle!=null&&id==appearanceTeam&&font==appearanceFont)return;
        ResetAppearance();appearanceTeam=id;appearanceFont=font;
        string name=team==null?"":team.name;
        bool alien=name=="Team_Alien";
        faction=alien?"ALIEN":name=="Team_Human_Centauri"?"CENTAURI":name=="Team_Human_Sol"?"SOL":"FIELD COMMAND";
        accent=team==null?new Color(0.5f,0.85f,1f,1):Color.Lerp(team.TeamColor,Color.white,0.12f);accent.a=1;
        // Neutral-light references: SOL slate, restrained burgundy/olive for Centauri/Alien.
        surface=alien?new Color(0.075f,0.08f,0.025f,0.94f):name=="Team_Human_Centauri"?new Color(0.13f,0.035f,0.04f,0.94f):new Color(0.075f,0.095f,0.12f,0.94f);
        if(name=="Team_Human_Centauri"&&!weaveDisabled)
        {
            panelWeave=new Texture2D(1,1024,TextureFormat.RGBA32,false){hideFlags=HideFlags.HideAndDontSave,wrapMode=TextureWrapMode.Clamp};
            for(int wy=0;wy<1024;wy++)panelWeave.SetPixel(0,wy,new Color(.7f,.24f,.25f,wy%4==0?.10f:wy%4==1?.035f:0));
            panelWeave.Apply();
            weaveStyle=new GUIStyle();weaveStyle.normal.background=panelWeave;
        }
        panelMask=new Texture2D(32,32,TextureFormat.RGBA32,false){hideFlags=HideFlags.HideAndDontSave,wrapMode=TextureWrapMode.Clamp};
        for(int y=0;y<32;y++)for(int x=0;x<32;x++)
        {
            float dx=Math.Max(0,8-Math.Min(x,31-x)),dy=Math.Max(0,8-Math.Min(y,31-y));
            bool cut=alien?dx*dx+dy*dy>64:dx+dy>8;
            panelMask.SetPixel(x,y,new Color(1,1,1,cut?0:1));
        }
        panelMask.Apply();
        bodyStyle=Text(font);titleStyle=Text(font+3);titleStyle.fontStyle=FontStyle.Bold;
        titleStyle.normal.textColor=new Color(1f,.97f,.87f,1);
        smallStyle=Text(Math.Max(16,font-2));smallStyle.normal.textColor=new Color(.93f,.94f,.91f,1);
        surfaceStyle=new GUIStyle();surfaceStyle.normal.background=panelMask;surfaceStyle.border=new RectOffset(8,8,8,8);
        buttonStyle=Text(font);buttonStyle.alignment=TextAnchor.MiddleLeft;buttonStyle.padding=new RectOffset(12,8,4,4);buttonStyle.border=new RectOffset(8,8,8,8);
        buttonStyle.normal.background=panelMask;buttonStyle.hover.background=panelMask;buttonStyle.active.background=panelMask;
        buttonStyle.hover.textColor=Color.white;buttonStyle.active.textColor=Color.white;buttonStyle.focused.textColor=Color.white;
        buttonStyle.focused.background=panelMask;
        inputBackground=new Texture2D(1,1,TextureFormat.RGBA32,false){hideFlags=HideFlags.HideAndDontSave};
        inputBackground.SetPixel(0,0,new Color(surface.r*.55f,surface.g*.55f,surface.b*.55f,1));inputBackground.Apply();
        inputStyle=Text(font);inputStyle.alignment=TextAnchor.MiddleLeft;inputStyle.padding=new RectOffset(12,8,4,4);inputStyle.wordWrap=false;
        inputStyle.border=new RectOffset(0,0,0,0);
        inputStyle.normal.background=inputStyle.hover.background=inputStyle.active.background=inputStyle.focused.background=inputBackground;
        inputStyle.normal.textColor=inputStyle.hover.textColor=inputStyle.active.textColor=inputStyle.focused.textColor=Color.white;
        tabStyle=Text(Math.Min(font,17));tabStyle.fontStyle=FontStyle.Bold;tabStyle.alignment=TextAnchor.MiddleCenter;tabStyle.padding=new RectOffset(6,6,4,4);tabStyle.border=new RectOffset(8,8,8,8);
        tabStyle.normal.background=tabStyle.hover.background=tabStyle.active.background=tabStyle.focused.background=panelMask;
        tabStyle.hover.textColor=tabStyle.active.textColor=tabStyle.focused.textColor=Color.white;
        tableStyle=Text(Math.Max(16,font-1));tableHeaderStyle=Text(Math.Max(16,font-1));tableHeaderStyle.fontStyle=FontStyle.Bold;
    }
    private GUIStyle Text(int size)
    {
        var result=new GUIStyle{font=GUI.skin.font,fontSize=size,wordWrap=true,richText=false};
        result.normal.textColor=new Color(.98f,.98f,.95f,1);return result;
    }
    private void Surface(Rect rect,Color color)
    {var old=GUI.color;try{GUI.color=color;GUI.Box(rect,"",surfaceStyle);}finally{GUI.color=old;}}
    private void WindowSurface(Rect rect)
    {
        Surface(rect,surface);
        if(weaveStyle!=null&&!weaveDisabled)
        {
            // DrawTexture/DrawTextureWithTexCoords are throwing stubs in this IL2CPP build.
            // Use the same native GUI.Box background path as the working menu surfaces.
            try{GUI.Box(new Rect(rect.x+8,rect.y+8,rect.width-16,rect.height-16),"",weaveStyle);}
            catch(Exception error){weaveDisabled=true;Error(error,"Centauri decoration disabled; base menu retained");}
        }
    }
    private static string Description(string label)=>label switch
    {
        "CLOSE"=>"Close the mod menu and return to the collapsed faction header. Gameplay controls and the expected cursor mode are restored.",
        "COLLAPSE"=>"Reduce the status panel to its faction header. F6 expands it again; a second F6 opens the full mod menu.",
        "OPEN MOD MENU  [F6]"=>"Open the full mod menu with Formations, KoH Buy, Ranks and Help tabs. The game continues running while you use it.",
        "FORMATIONS"=>"Choose personal formation layouts for your current faction and role. Existing native validation and formation rules apply.",
        "BUY"=>"Choose a production facility, then browse its units and current KoH prices. A second confirmation is required to spend personal credits; all KoH purchase rules apply.",
        "RANKS"=>"Read this faction's rank titles, score requirements, squad allowances, personal reward bonuses and seat limits from the active server configuration.",
        "HELP"=>"Browse the server's mod chat commands under FPS player, Commander and Admin. This tab lists commands without executing them.",
        "UNITBALANCE"=>"Browse the server's published Unit Balance configuration. All values in this tab are read only.",
        "FOLLOW"=>"Choose the layout your AI followers use while following your FPS unit. Selecting Native turns off all personal FPS formations.",
        "MOVE"=>"Choose the layout used for FPS squad move orders. This is separate from Follow and Attack staging.",
        "ATTACK"=>"Choose the layout used for FPS squad attack staging. Native targeting, movement and manual-order precedence remain in effect.",
        "COMMANDER"=>"Choose the layout used for your commander move orders. This does not expose FPS squad actions.",
        "FPS PLAYER"=>"Commands for an FPS player, including shared commands usable in either role. Some commands have additional server requirements.",
        "ADMIN"=>"Administrative commands and their required powers. Listing a command does not grant permission or execute it.",
        "PREVIOUS"=>"Show the previous page of entries in this tab. Your selected formation and personal credits are unchanged.",
        "NEXT"=>"Show the next page of entries in this tab. Your selected formation and personal credits are unchanged.",
        "REFRESH"=>"Request a fresh page and authoritative state from the server. Refreshing alone never buys units or changes your selections.",
        "CANCEL PURCHASE"=>"Cancel the pending purchase confirmation without sending a purchase request or spending credits.",
        _ when label.StartsWith("LOCKED FORMATION")=>"Units will try to keep formation at all times, even when moving. Formation will move slow. Toggles the existing FPS formation lock and speed coordination; pathfinding and obstacles still affect cohesion.",
        _ when label.Contains("DISMISS ALL AI")=>"Dismiss AI followers from your FPS squad. Click once to request confirmation and again to dismiss. Your controlled unit and other player-controlled units are retained.",
        "Mod menu (F6)"=>"F6 first expands squad status, then opens the full menu. Mouse clicks also work when the game already shows a cursor.",
        _=>""
    };
    private bool Button(Rect rect,string text,bool selected=false,bool allowed=true,string description=null,GUIStyle style=null)
    {
        var oldBackground=GUI.backgroundColor;bool oldEnabled=GUI.enabled;
        try
        {
            bool hover=rect.Contains(Event.current.mousePosition);
            string detail=description??Description(text);
            if(hover&&detail.Length>0){hoverCandidate=text+"|"+detail;hoverText=detail;}
            // Polling retains the same appearance while clicks wait for the complete reply.
            GUI.enabled=oldEnabled&&(allowed||BackgroundRefresh);
            GUI.backgroundColor=Color.Lerp(surface,accent,selected?0.29f:hover?0.18f:0.07f);
            return GUI.Button(rect,text,style??buttonStyle)&&allowed;
        }
        finally{GUI.backgroundColor=oldBackground;GUI.enabled=oldEnabled;}
    }
    private void DrawTooltip()
    {
        if(hoverCandidate!=hoverKey){hoverKey=hoverCandidate;hoverSince=clock.Elapsed.TotalSeconds;}
        if(hoverKey.Length==0||!Cursor.visible||!Application.isFocused||clock.Elapsed.TotalSeconds-hoverSince<=2)return;
        float width=Math.Min(440,Screen.width-32),height=bodyStyle.CalcHeight(new GUIContent(hoverText),width-28)+24;
        var mouse=Event.current.mousePosition;
        float x=Math.Clamp(mouse.x+18,12,Screen.width-width-12),y=Math.Clamp(mouse.y+22,12,Math.Max(12,Screen.height-height-12));
        Surface(new Rect(x,y,width,height),new Color(surface.r,surface.g,surface.b,0.99f));
        GUI.Label(new Rect(x+14,y+12,width-28,height-24),hoverText,bodyStyle);
    }
    private string SquadQuantity(out string detail)
    {
        var p=Player.CurrentPlayer;var status=cache.Fresh(clock.Elapsed.TotalSeconds);
        if(p==null||p.IsCommander){detail="No FPS squad while commanding";return "Inactive";}
        var unit=p.ControlledUnit;var group=unit==null?null:unit.AIGroup;
        if(group==null){detail="No active FPS squad";return "n/a";}
        int count=group.UnitCount;
        detail=group.Leader==unit?$"{Math.Max(0,count-1)} AI followers; count includes you":"Group count; count includes you";
        return count+((status!=null&&!status.Commander&&status.EffectiveCap>0)?" / "+status.EffectiveCap:"");
    }
    private void RenderInterface()
    {
        Appearance();hoverCandidate="";
        SyncBalanceContext();
        EnsureWindowProfile();
        bool interactive=Application.isFocused&&!NativeMenu()&&!NativeTextInput();
        bool overMenu=panelOpen&&OverlayContains(WindowRect(Math.Min(1100,Screen.width-24),Math.Min(940,Screen.height-24)),"menu");
        bool enabledBefore=GUI.enabled;GUI.enabled=enabledBefore&&interactive&&Cursor.visible&&!overMenu&&!statusPickerOpen;
        try
        {
            // Keep status visible while F6 provides an FPS cursor for arranging overlays.
            bool overlayEnabled=GUI.enabled;
            GUI.enabled=overlayEnabled&&!SidebarPointerCaptured&&!NotificationPointerCaptured;
            RenderStatus();
            GUI.enabled=overlayEnabled&&!NotificationPointerCaptured;
            if(SidebarVisible)RenderSidebar();
            GUI.enabled=overlayEnabled;
            if(notificationsEnabled)RenderNotifications();
        }
        finally{GUI.enabled=enabledBefore;}
        GUI.enabled=enabledBefore&&!statusPickerOpen;
        try{if(panelOpen)RenderModMenu();}finally{GUI.enabled=enabledBefore;}
        if(statusPickerOpen)RenderStatusPicker();
        DrawTooltip();
    }
    private void RenderStatus()
    {
        var values=AvailableParameters().Where(r=>!layout.HiddenStatus.Contains(r.Id)).ToArray();
        string text=string.Join("\n",values.Select(r=>r.Mod+" / "+r.Label+": "+r.Value));
        if(text.Length==0)text="Choose status parameters in the mod menu.";
        float defaultWidth=hudExpanded?620:340;
        float defaultHeight=hudExpanded?Math.Min(520,bodyStyle.CalcHeight(new GUIContent(text),defaultWidth-48)+110):44;
        string key=hudExpanded?"status-expanded":"status-header";
        var rect=LayoutRect(key,defaultWidth,defaultHeight,1,84f/Math.Max(1,Screen.height-defaultHeight-24),hudExpanded?380:300,hudExpanded?180:44);
        statusRect=rect;
        float x=rect.x,y=rect.y,width=rect.width,height=rect.height;
        WindowSurface(rect);Surface(new Rect(x+12,y,width-24,2),accent);
        PlaceWindow(key,rect,hudExpanded?width-24:130,hudExpanded?380:300,hudExpanded?180:44);
        bool clickable=Cursor.visible&&!NativeMenu()&&!NativeTextInput();
        if(!hudExpanded)
        {
            GUI.Label(new Rect(x+14,y+10,128,26),"Mod menu",bodyStyle);
            if(Button(new Rect(x+142,y+5,width-166,32),"F6 / OPEN",false,clickable,style:tabStyle))TogglePanel();
            return;
        }
        GUI.Label(new Rect(x+18,y+8,width-36,32),"Mod menu (F6)",titleStyle);
        var viewport=new Rect(x+14,y+48,width-28,height-106);
        float h=bodyStyle.CalcHeight(new GUIContent(text),viewport.width-28);
        statusScroll=GUI.BeginScrollView(viewport,statusScroll,new Rect(0,0,viewport.width-24,h),false,true);
        try{GUI.Label(new Rect(0,0,viewport.width-28,h),text,bodyStyle);}finally{GUI.EndScrollView();}
        if(Button(new Rect(x+14,y+height-50,width-152,32),"MOD MENU [F6]",false,clickable,style:tabStyle))TogglePanel();
        if(Button(new Rect(x+width-130,y+height-50,110,32),"COLLAPSE",false,clickable,style:tabStyle))hudExpanded=false;
    }
    private bool Waiting=>pendingId.Length>0||clock.Elapsed.TotalSeconds<controlReadyAt;
    private bool BackgroundRefresh=>Waiting&&page?.Available==true&&!pendingAction;
    private string buyFacility="";
    private readonly HashSet<string> expandedAdminMods=new();
    private void ChangeSection(string tab,string section)
    {
        if(AdminMenuSection&&tab!=menuTab)ClearBalanceEdits();
        menuTab=tab;function=section;page=null;confirmDismiss=false;confirmPurchase=0;buyFacility="";optionScroll=Vector2.zero;hoverKey="";
        if(tab=="unitbalance"){function="";balanceFieldScroll=Vector2.zero;return;}RequestPage(0);
    }
    private bool Tab(Rect rect,string label,bool selected,bool allowed=true,string description=null)
    {
        bool clicked=Button(rect,label,selected,allowed,description,tabStyle);
        if(selected)Surface(new Rect(rect.x+6,rect.y+rect.height-3,rect.width-12,3),accent);
        return clicked;
    }
    private void RenderModMenu()
    {
        float width=Math.Min(1100,Screen.width-24),height=Math.Min(940,Screen.height-24);
        var window=WindowRect(width,height);width=window.width;height=window.height;float x=window.x,y=window.y,pad=20;
        WindowSurface(window);DragWindow(window);Surface(new Rect(x+12,y,width-24,3),accent);
        if(height<560||width<600)
        {
            GUI.Label(new Rect(x+20,y+20,width-40,100),"Enlarge the game window to use mod controls. F6 or Escape closes this panel.",bodyStyle);
            if(Button(new Rect(x+20,y+140,width-40,42),"CLOSE"))ClosePanel();return;
        }
        GUI.Label(new Rect(x+pad,y+13,190,34),"MOD MENU",titleStyle);
        if(Button(new Rect(x+220,y+12,100,32),"status",false,true,"Choose the available mod parameters shown in expanded status.",tabStyle))statusPickerOpen=!statusPickerOpen;
        if(panelCommander&&Button(new Rect(x+328,y+12,100,32),"sidebar",sidebarMode,true,"Keep a commander formation sidebar on screen.",tabStyle))SetSidebarMode(!sidebarMode);
        if(Button(new Rect(x+436,y+12,166,32),"notification bar",notificationsEnabled,true,"Route mod chat notifications into a separate persistent window.",tabStyle))SetNotifications(!notificationsEnabled);
        if(Button(new Rect(x+width-116,y+12,96,36),"CLOSE")){ClosePanel();return;}
        var p=Player.CurrentPlayer;var remote=cache.Fresh(clock.Elapsed.TotalSeconds);
        if(remote!=null&&remote.Commander!=panelCommander)remote=null;
        float metricWidth=(width-2*pad-24)/3;
        Metric(new Rect(x+pad,y+57,metricWidth,62),"SCORE",(remote?.Score??p?.Score)?.ToString("N0")??"Unavailable");
        Metric(new Rect(x+pad+metricWidth+12,y+57,metricWidth,62),"PERSONAL CREDITS",remote?.Credits?.ToString("N0")??"Unavailable");
        string squad=SquadQuantity(out string squadDetail);
        Metric(new Rect(x+pad+2*(metricWidth+12),y+57,metricWidth,62),"FPS SQUAD",squad);
        GUI.Label(new Rect(x+pad,y+126,width-2*pad,40),"Rank: "+(remote?.Rank??"Unavailable")+"  |  "+(panelCommander?"Commander":squadDetail),smallStyle);
        var tabs=new List<(string key,string title)>{("formations","FORMATIONS"),("buy","BUY"),("ranks","RANKS"),("unitbalance","UNITBALANCE"),("help","HELP")};
        tabs.RemoveAll(t=>!TabVisible(t.key));
        if(serverAdmin)tabs.Add(("admin","ADMIN"));
        Surface(new Rect(x+pad-4,y+168,width-2*pad+8,55),new Color(0.025f,0.03f,0.04f,0.97f));
        float tabWidth=(width-2*pad-8*(tabs.Count-1))/tabs.Count;
        for(int i=0;i<tabs.Count;i++)
        {
            var tab=tabs[i];
            string hint=tab.key=="admin"?"Administrator mod controls. Shows all loaded server mods and their reviewed live master switches; no mod is unloaded from this menu.":null;
            if(Tab(new Rect(x+pad+i*(tabWidth+8),y+172,tabWidth,46),tab.title,menuTab==tab.key||tab.key=="admin"&&AdminMenuSection,!Waiting&&!BalanceBusy,hint)&&menuTab!=tab.key)
                ChangeSection(tab.key,tab.key=="formations"?(panelCommander?"commander":"follow"):tab.key=="help"?(panelCommander?"help-commander":"help-fps"):tab.key);
        }
        float rowsY=y+236;
        if(AdminMenuSection)
        {
            if(Button(new Rect(x+pad,rowsY,196,32),"BACK TO ADMIN",false,!BalanceBusy)){ChangeSection("admin","admin");return;}
            rowsY+=42;
        }
        if(menuTab=="formations"||menuTab=="help")
        {
            string[] sections=menuTab=="help"?new[]{"help-fps","help-commander","help-admin"}:panelCommander?new[]{"commander"}:new[]{"follow","move","attack"};
            float subWidth=(width-2*pad-8*(sections.Length-1))/sections.Length;
            for(int i=0;i<sections.Length;i++)
            {
                var section=sections[i];string label=section=="help-fps"?"FPS PLAYER":section=="help-commander"?"COMMANDER":section=="help-admin"?"ADMIN":section.ToUpperInvariant();
                string hint=section=="help-commander"?"Commands for commanders, including shared commands. Administrative controls are listed separately.":null;
                if(Tab(new Rect(x+pad+i*(subWidth+8),rowsY,subWidth,36),label,function==section,!Waiting,hint)&&function!=section)ChangeSection(menuTab,section);
            }
            rowsY+=46;
        }
        if(menuTab=="formations")
        {
            GUI.Label(new Rect(x+pad,rowsY,width-2*pad,26),page?.Available==true?(page.Enabled?"Custom formation active":"Native behavior / custom formation inactive"):"Loading server formations...",smallStyle);rowsY+=32;
        }
        if(menuTab=="buy")
        {
            if(buyFacility.Length>0)
            {
                if(Button(new Rect(x+pad,rowsY,220,36),"PRODUCTION FACILITIES",false,!Waiting,"Return to the faction's production-facility list. No purchase is sent."))
                {buyFacility="";confirmPurchase=0;optionScroll=Vector2.zero;}
                GUI.Label(new Rect(x+pad+234,rowsY+3,width-2*pad-234,32),buyFacility,titleStyle);
            }
            else GUI.Label(new Rect(x+pad,rowsY,width-2*pad,32),"Choose a production facility",titleStyle);
            rowsY+=46;
        }
        var entries=new Rect(x+pad,rowsY,width-2*pad,Math.Max(40,y+height-132-rowsY));
        if(AdminMenuSection)RenderAdminMenu(entries);else if(BalanceSection)RenderBalance(entries,false);else RenderEntries(entries);
        float bottom=y+height-120;
        if(AdminMenuSection)RenderAdminMenuActions(new Rect(x+pad,bottom,width-2*pad,42));
        else if(BalanceSection)RenderBalanceActions(new Rect(x+pad,bottom,width-2*pad,42),false);
        else if(menuTab=="formations"&&!panelCommander)
        {
            bool leader=page?.Available==true&&page.Leader;
            if(Button(new Rect(x+pad,bottom,(width-2*pad-10)/2,40),page?.Locked==true?"LOCKED FORMATION: ON":"LOCKED FORMATION: OFF",page?.Locked==true,!Waiting&&leader))
            {confirmDismiss=false;RequestPage(0,"lock",page.Locked?0:1);}
            if(Button(new Rect(x+pad+(width-2*pad+10)/2,bottom,(width-2*pad-10)/2,40),confirmDismiss?"CONFIRM DISMISS ALL AI":"DISMISS ALL AI",false,!Waiting&&leader))
            {if(confirmDismiss){confirmDismiss=false;RequestPage(0,"disband",0);}else{confirmDismiss=true;panelMessage="Click again to dismiss AI followers. Your controlled unit is retained.";}}
        }
        else if(menuTab=="buy"&&confirmPurchase>0)
        {if(Button(new Rect(x+pad,bottom,width-2*pad,40),"CANCEL PURCHASE",false,!Waiting)){confirmPurchase=0;panelMessage="Purchase cancelled; no request sent.";}}
        else GUI.Label(new Rect(x+pad,bottom,width-2*pad,42),menuTab=="help"?"Command reference only. Scroll to read; entries never execute commands.":menuTab=="ranks"?"Squad caps include you. Effective score, recovery and seat availability govern promotions.":menuTab=="admin"?"ON/OFF controls apply server-wide using each mod's own save/cleanup behavior.":menuTab=="buy"?"Unit purchases use personal credits; faction treasury is separate.":"Commander formation selection; FPS squad actions are unavailable.",smallStyle);
        string message=AdminMenuSection?adminMenuMessage:BalanceSection?balanceMessage:panelMessage;
        GUI.Label(new Rect(x+pad,y+height-66,width-190,56),message.Length==0?"Drag the title bar to move this window. Hover for 2 seconds for details.":message,smallStyle);
        if(Button(new Rect(x+width-144,y+height-60,124,40),"REFRESH",false,!Waiting&&!BalanceBusy)){if(AdminMenuSection)SendAdminChoice("refresh");else if(BalanceSection)RefreshBalance();else{confirmPurchase=0;RequestPage(0);}}
    }
    private float[] TableWidths(float width)
        =>menuTab=="ranks"?new[]{width*.055f,width*.10f,width*.20f,width*.075f,width*.10f,width*.32f,width*.15f}:new[]{width*.29f,width*.15f,width*.56f};
    private string[] TableCells(int i)
    {
        var data=page.Data[i].Split('|');
        if(menuTab!="ranks")return new[]{page.Names[i],page.Data[i],page.Details[i]};
        return data.Length==6?new[]{data[0],data[1],page.Names[i],data[2],data[3],data[4],data[5]}:new[]{"","",page.Names[i],"","","",""};
    }
    private void DrawCells(Rect rect,string[] cells,float[] widths,bool heading)
    {
        var style=heading?tableHeaderStyle:tableStyle;float left=rect.x;
        for(int c=0;c<cells.Length;c++)
        {
            if(c>0)Surface(new Rect(left,rect.y,.5f,rect.height),Color.Lerp(surface,accent,.22f));
            GUI.Label(new Rect(left+5,rect.y+5,widths[c]-10,rect.height-10),cells[c],style);left+=widths[c];
        }
        Surface(new Rect(rect.x,rect.y+rect.height-.5f,rect.width,.5f),Color.Lerp(surface,accent,.25f));
    }
    private void RenderEntries(Rect viewport)
    {
        if(page?.Available!=true){GUI.Label(new Rect(viewport.x,viewport.y,viewport.width,90),pendingId.Length>0?"Loading complete server list...":panelMessage,bodyStyle);return;}
        bool formation=menuTab=="formations",buy=menuTab=="buy",table=menuTab=="ranks"||menuTab=="help",admin=menuTab=="admin";
        if(buy&&buyFacility.Length==0){RenderFacilities(viewport);return;}
        float contentWidth=viewport.width-26;
        float[] widths=table?TableWidths(contentWidth):Array.Empty<float>();
        if(table)
        {
            float headHeight=64;
            Surface(new Rect(viewport.x,viewport.y,contentWidth,headHeight),Color.Lerp(surface,accent,.12f));
            DrawCells(new Rect(viewport.x,viewport.y,contentWidth,headHeight),menuTab=="ranks"?new[]{"Tier","Required score","Rank","FPS cap","Reward bonus","Allowed recruited units","Seats / team"}:new[]{"Command","Mod","Explanation"},widths,true);
            viewport=new Rect(viewport.x,viewport.y+headHeight,viewport.width,Math.Max(32,viewport.height-headHeight));
        }
        var indices=Enumerable.Range(0,page.Numbers.Length).Where(i=>(!buy||page.Groups[i]==buyFacility)&&
            (!admin||!page.Groups[i].StartsWith("child:")||expandedAdminMods.Contains(page.Groups[i].Substring(6)))).ToArray();
        var heights=new float[indices.Length];var cellHeights=new float[indices.Length];float fullHeight=0;
        for(int k=0;k<indices.Length;k++)
        {
            int i=indices[k];float h=formation?48:admin?96:108;
            if(table)
            {
                var cells=TableCells(i);h=48;
                for(int c=0;c<cells.Length;c++)h=Math.Max(h,tableStyle.CalcHeight(new GUIContent(cells[c]),widths[c]-16)+16);
                cellHeights[k]=h;
                if(menuTab=="ranks"&&page.Details[i].Length>0)h+=tableStyle.CalcHeight(new GUIContent(page.Details[i]),contentWidth-20)+20;
            }
            else if(!formation)h=Math.Max(h,smallStyle.CalcHeight(new GUIContent(page.Details[i]),contentWidth-24)+48);
            heights[k]=h+(table?0:8);fullHeight+=heights[k];
        }
        if(indices.Length==0){GUI.Label(new Rect(viewport.x+8,viewport.y+8,contentWidth-16,70),"No units currently available for this facility. Return to facilities or refresh.",bodyStyle);return;}
        bool mouseInside=viewport.Contains(Event.current.mousePosition);
        optionScroll=GUI.BeginScrollView(viewport,optionScroll,new Rect(0,0,contentWidth,fullHeight),false,true);
        try
        {
            float top=0;
            for(int k=0;k<indices.Length;k++)
            {
                int i=indices[k],number=page.Numbers[i];float h=heights[k];
                if(top+h<optionScroll.y||top>optionScroll.y+viewport.height){top+=h;continue;}
                string name=page.Names[i],detail=page.Details[i];
                if(formation)
                {
                    if(panelCommander&&page.Groups[i]=="commander-lock")
                    {
                        string hint="units attempt to stay in formation trough travel, formation will move slow. Arms the next commander move; click again to unlock active formations and cancel a pending lock.";
                        if(Button(new Rect(0,top,contentWidth,h-8),"Lock formation  /  "+(page.Locked?"Locked":"Unlocked"),page.Locked,!Waiting,mouseInside?hint:""))
                            RequestPage(0,"lock",page.Locked?0:1);
                    }
                    else if(Button(new Rect(0,top,contentWidth,h-8),(number==page.Selected?"SELECTED  /  ":"")+name,number==page.Selected,!Waiting,""))
                    {confirmDismiss=false;RequestPage(0,"select",number);}
                }
                else if(table)
                {
                    Surface(new Rect(0,top,contentWidth,h),Color.Lerp(surface,accent,k%2==0?.025f:.065f));
                    DrawCells(new Rect(0,top,contentWidth,cellHeights[k]),TableCells(i),widths,false);
                    if(menuTab=="ranks"&&detail.Length>0)GUI.Label(new Rect(10,top+cellHeights[k]+8,contentWidth-20,h-cellHeights[k]-12),detail,tableStyle);
                }
                else
                {
                    Surface(new Rect(0,top,contentWidth,h-8),Color.Lerp(surface,accent,.035f));
                    bool configure=admin&&(page.Groups[i]=="unitbalance-config"||page.Groups[i]=="starter-config");
                    bool child=admin&&page.Groups[i].StartsWith("child:");
                    bool children=admin&&page.Groups.Contains("child:"+name);
                    float nameX=child?32:children?48:12;
                    if(children&&Button(new Rect(8,top+7,30,30),expandedAdminMods.Contains(name)?"−":"+",style:tabStyle))
                    {if(!expandedAdminMods.Add(name))expandedAdminMods.Remove(name);}
                    GUI.Label(new Rect(nameX,top+7,contentWidth-nameX-(configure?478:admin?344:220),36),child?"/ "+name:name,bodyStyle);
                    GUI.Label(new Rect(12,top+42,contentWidth-24,h-50),detail,smallStyle);
                    if(buy)
                    {
                        string label=confirmPurchase==number?"CONFIRM PURCHASE":"BUY UNIT";
                        if(Button(new Rect(contentWidth-204,top+6,192,34),label,confirmPurchase==number,!Waiting,mouseInside?"Purchase "+name+". "+detail+" Click once to review, then Confirm Purchase to send it to KoH.":""))
                        {
                            if(confirmPurchase==number){confirmPurchase=0;RequestPage(0,"purchase",number);}
                            else{confirmPurchase=number;panelMessage="Confirm "+name+" to spend personal credits, or cancel.";}
                        }
                    }
                    if(admin)
                    {
                        if(configure&&Button(new Rect(contentWidth-470,top+6,132,34),"CONFIGURE",false,!Waiting&&serverAdmin,"Open this mod's server-validated configuration menu."))
                        {OpenAdminMenu(page.Groups[i]=="starter-config"?"starter":"balance");return;}
                        var data=page.Data[i].Split('|');string state=data.Length>0?data[0]:"Unavailable";bool can=data.Length==2&&data[1]=="1";
                        GUI.Label(new Rect(contentWidth-330,top+10,110,26),state,tableHeaderStyle);
                        string label=can?(state=="ON"?"TURN OFF":"TURN ON"):"NO LIVE SWITCH";
                        if(Button(new Rect(contentWidth-204,top+6,192,34),label,false,!Waiting&&serverAdmin&&can,mouseInside?detail:""))
                            RequestPage(0,state=="ON"?"disable":"enable",number);
                    }
                }
                top+=h;
            }
        }
        finally{GUI.EndScrollView();}
    }
    private void RenderFacilities(Rect viewport)
    {
        var groups=page.Groups.Where(g=>g.Length>0).Distinct().ToArray();float w=viewport.width-26;
        optionScroll=GUI.BeginScrollView(viewport,optionScroll,new Rect(0,0,w,groups.Length*60),false,true);
        try
        {
            for(int i=0;i<groups.Length;i++)
            {
                string facility=groups[i];int count=page.Groups.Count(g=>g==facility);
                if(Button(new Rect(0,i*60,w,52),facility+"  /  "+count+" units",false,!Waiting,"Show the faction's available units produced by "+facility+". Selecting a facility does not spend credits."))
                {buyFacility=facility;confirmPurchase=0;optionScroll=Vector2.zero;}
            }
        }
        finally{GUI.EndScrollView();}
    }
    private void Metric(Rect rect,string label,string value)
    {
        Surface(rect,Color.Lerp(surface,accent,0.06f));
        GUI.Label(new Rect(rect.x+12,rect.y+4,rect.width-24,22),label,smallStyle);
        GUI.Label(new Rect(rect.x+12,rect.y+27,rect.width-24,31),value,titleStyle);
    }
}
