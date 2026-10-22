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
        "HELP"=>"Click a command to send it through normal chat. Commands with arguments open an editable draft.",
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
    private bool Button(Rect rect,string text,bool selected=false,bool allowed=true,string description=null,GUIStyle style=null,bool keepAppearance=false)
    {
        var oldBackground=GUI.backgroundColor;bool oldEnabled=GUI.enabled;
        try
        {
            bool hover=rect.Contains(Event.current.mousePosition);
            string detail=description??Description(text);
            if(hover&&detail.Length>0){hoverCandidate=text+"|"+detail;hoverText=detail;}
            // Polling retains the same appearance while clicks wait for the complete reply.
            GUI.enabled=oldEnabled&&(allowed||keepAppearance);
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
        var p=Player.CurrentPlayer;
        if(p==null||p.IsCommander){detail="No FPS squad while commanding";return "Inactive";}
        var unit=p.ControlledUnit;var group=unit==null?null:unit.AIGroup;
        if(group==null){detail="No active FPS squad";return "n/a";}
        int count=group.UnitCount;
        detail=group.Leader==unit?$"{Math.Max(0,count-1)} AI followers; count includes you":"Group count; count includes you";
        return count.ToString();
    }
    private void RenderInterface()
    {
        Appearance();hoverCandidate="";

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
        var values=AvailableParameters().Where(r=>layout.SelectedStatus.Contains(r.Id)).ToArray();
        string text=string.Join("\n",values.Select(r=>r.Label+": "+r.Value));
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
            GUI.Label(new Rect(x+14,y+10,128,26),"Mod status",bodyStyle);
            if(Button(new Rect(x+142,y+5,width-166,32),"F6 / OPEN",false,clickable,style:tabStyle))TogglePanel();
            return;
        }
        GUI.Label(new Rect(x+18,y+8,width-36,32),"Mod status (F6)",titleStyle);
        var viewport=new Rect(x+14,y+48,width-28,height-106);
        float h=bodyStyle.CalcHeight(new GUIContent(text),viewport.width-28);
        statusScroll=GUI.BeginScrollView(viewport,statusScroll,new Rect(0,0,viewport.width-24,h),false,true);
        try{GUI.Label(new Rect(0,0,viewport.width-28,h),text,bodyStyle);}finally{GUI.EndScrollView();}
        if(Button(new Rect(x+14,y+height-50,width-152,32),"MOD MENU [F6]",false,clickable,style:tabStyle))TogglePanel();
        if(Button(new Rect(x+width-130,y+height-50,110,32),"COLLAPSE",false,clickable,style:tabStyle))hudExpanded=false;
    }
}

