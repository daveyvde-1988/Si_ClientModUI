using UnityEngine;

using Il2Cpp;
namespace SiSquadHud;
public sealed partial class SquadHud
{
    private GUIStyle bodyStyle,titleStyle,smallStyle,buttonStyle,surfaceStyle,tabStyle,inputStyle;
    private Texture2D panelMask,inputBackground;
    private Color accent=new(.5f,.85f,1f,1f),surface=new(.075f,.095f,.12f,.94f);
    private int appearanceTeam=int.MinValue;
    private float appearanceTransparency=-1;
    private string hoverCandidate="",hoverText="",hoverKey="";
    private double hoverSince;

    private void ResetAppearance()
    {
        if(panelMask!=null)UnityEngine.Object.Destroy(panelMask);
        if(inputBackground!=null)UnityEngine.Object.Destroy(inputBackground);
        panelMask=inputBackground=null;
        bodyStyle=titleStyle=smallStyle=buttonStyle=surfaceStyle=tabStyle=inputStyle=null;hoverKey="";appearanceTeam=int.MinValue;
    }
    private void Appearance()
    {
        var team=Player.CurrentPlayer?.Team;int teamId=team?.GetInstanceID()??-1;
        if(bodyStyle!=null&&appearanceTeam==teamId&&Math.Abs(appearanceTransparency-layout.Transparency)<.01f)return;
        ResetAppearance();appearanceTeam=teamId;appearanceTransparency=layout.Transparency;
        var native=team==null?new Color(.5f,.85f,1f,1):team.TeamColor;native.a=1;
        accent=Color.Lerp(native,Color.white,.12f);
        surface=Color.Lerp(new Color(.045f,.055f,.07f,1),native,.17f);surface.a=1;
        panelMask=new Texture2D(1,1,TextureFormat.RGBA32,false){hideFlags=HideFlags.HideAndDontSave};
        panelMask.SetPixel(0,0,Color.white);panelMask.Apply();
        bodyStyle=Text(18);titleStyle=Text(21);titleStyle.fontStyle=FontStyle.Bold;
        smallStyle=Text(16);
        surfaceStyle=new GUIStyle();surfaceStyle.normal.background=panelMask;
        buttonStyle=Text(18);buttonStyle.alignment=TextAnchor.MiddleLeft;buttonStyle.padding=new RectOffset(12,8,4,4);
        buttonStyle.normal.background=buttonStyle.hover.background=buttonStyle.active.background=buttonStyle.focused.background=panelMask;
        inputBackground=new Texture2D(1,1,TextureFormat.RGBA32,false){hideFlags=HideFlags.HideAndDontSave};
        inputBackground.SetPixel(0,0,new Color(surface.r*.6f,surface.g*.6f,surface.b*.6f,1-layout.Transparency/100));inputBackground.Apply();
        inputStyle=Text(18);inputStyle.alignment=TextAnchor.MiddleLeft;inputStyle.padding=new RectOffset(10,8,4,4);inputStyle.wordWrap=false;
        inputStyle.normal.background=inputStyle.hover.background=inputStyle.active.background=inputStyle.focused.background=inputBackground;
        inputStyle.normal.textColor=inputStyle.hover.textColor=inputStyle.active.textColor=inputStyle.focused.textColor=Color.white;
        tabStyle=Text(16);tabStyle.fontStyle=FontStyle.Bold;tabStyle.alignment=TextAnchor.MiddleCenter;tabStyle.padding=new RectOffset(6,6,4,4);
        tabStyle.normal.background=tabStyle.hover.background=tabStyle.active.background=tabStyle.focused.background=panelMask;
    }
    private GUIStyle Text(int size)
    {
        var style=new GUIStyle{font=GUI.skin.font,fontSize=size,wordWrap=true,richText=false};
        style.normal.textColor=style.hover.textColor=style.active.textColor=style.focused.textColor=new Color(.98f,.98f,.95f,1);return style;
    }
    private void Surface(Rect rect,Color color)
    {var old=GUI.color;try{color.a*=1-layout.Transparency/100;GUI.color=color;GUI.Box(rect,"",surfaceStyle);}finally{GUI.color=old;}}
    private void WindowSurface(Rect rect)=>Surface(rect,surface);
    private bool Button(Rect rect,string text,bool selected=false,bool allowed=true,string description=null,GUIStyle style=null,bool keepAppearance=false)
    {
        var oldBackground=GUI.backgroundColor;bool oldEnabled=GUI.enabled;
        try
        {
            bool hover=rect.Contains(Event.current.mousePosition);
            string detail=description??"";
            if(hover&&detail.Length>0){hoverCandidate=text+"|"+detail;hoverText=detail;}
            GUI.enabled=oldEnabled&&(allowed||keepAppearance);
            var background=Color.Lerp(surface,accent,selected ? .29f : hover ? .18f : .07f);background.a*=1-layout.Transparency/100;
            GUI.backgroundColor=background;
            return GUI.Button(rect,text,style??buttonStyle)&&allowed;
        }
        finally{GUI.backgroundColor=oldBackground;GUI.enabled=oldEnabled;}
    }
    private void DrawTooltip()
    {
        if(hoverCandidate!=hoverKey){hoverKey=hoverCandidate;hoverSince=clock.Elapsed.TotalSeconds;}
        if(hoverKey.Length==0||!Cursor.visible||!Application.isFocused||clock.Elapsed.TotalSeconds-hoverSince<=2)return;
        float width=Math.Min(440,UiWidth-32),height=bodyStyle.CalcHeight(new GUIContent(hoverText),width-28)+24;
        var mouse=Event.current.mousePosition;
        float x=Math.Clamp(mouse.x+18,12,UiWidth-width-12),y=Math.Clamp(mouse.y+22,12,Math.Max(12,UiHeight-height-12));
        Surface(new Rect(x,y,width,height),new Color(.04f,.05f,.07f,.99f));
        GUI.Label(new Rect(x+14,y+12,width-28,height-24),hoverText,bodyStyle);
    }
    private void RenderInterface()
    {
        Appearance();hoverCandidate="";EnsureWindowProfile();
        bool interactive=Application.isFocused&&!NativeMenu()&&!NativeTextInput();
        bool overMenu=panelOpen&&OverlayContains(WindowRect(Math.Min(1100,UiWidth-24),Math.Min(900,UiHeight-24)),"menu");
        bool before=GUI.enabled;
        GUI.enabled=before&&interactive&&Cursor.visible&&!overMenu&&!statusPickerOpen;
        try{RenderStatus();if(SidebarVisible)RenderSidebar();if(notificationsEnabled)RenderNotifications();}
        finally{GUI.enabled=before;}
        GUI.enabled=before&&!statusPickerOpen;
        try{if(panelOpen)RenderModMenu();}finally{GUI.enabled=before;}
        if(statusPickerOpen)RenderStatusPicker();
        DrawTooltip();
    }
    private void RenderStatus()
    {
        var values=AvailableParameters().Where(r=>layout.SelectedStatus.Contains(r.Id)).ToArray();
        string text=string.Join("\n",values.Select(r=>r.Label+": "+r.Value));
        if(text.Length==0)text="No status values selected.";
        float width=hudExpanded?620:340,height=hudExpanded?Math.Min(520,bodyStyle.CalcHeight(new GUIContent(text),572)+110):44;
        string key=hudExpanded?"status-expanded":"status-header";
        var rect=LayoutRect(key,width,height,1,.15f,hudExpanded?380:300,hudExpanded?180:44);
        statusRect=rect;WindowSurface(rect);Surface(new Rect(rect.x+12,rect.y,rect.width-24,2),accent);
        PlaceWindow(key,rect,hudExpanded?rect.width-24:130,hudExpanded?380:300,hudExpanded?180:44);
        bool clickable=Cursor.visible&&!NativeMenu()&&!NativeTextInput();
        if(!hudExpanded)
        {
            GUI.Label(new Rect(rect.x+14,rect.y+10,128,26),"Mod menu",bodyStyle);
            if(Button(new Rect(rect.x+142,rect.y+5,rect.width-166,32),"F6 / OPEN",allowed:clickable,style:tabStyle))TogglePanel();
            return;
        }
        GUI.Label(new Rect(rect.x+18,rect.y+8,rect.width-36,32),"Mod status",titleStyle);
        var view=new Rect(rect.x+14,rect.y+48,rect.width-28,rect.height-106);
        float h=bodyStyle.CalcHeight(new GUIContent(text),view.width-28);
        statusScroll=GUI.BeginScrollView(view,statusScroll,new Rect(0,0,view.width-24,h),false,true);
        try{GUI.Label(new Rect(0,0,view.width-28,h),text,bodyStyle);}finally{GUI.EndScrollView();}
        if(Button(new Rect(rect.x+14,rect.yMax-50,rect.width-152,32),"MOD MENU [F6]",allowed:clickable,style:tabStyle))TogglePanel();
        if(Button(new Rect(rect.xMax-130,rect.yMax-50,110,32),"COLLAPSE",allowed:clickable,style:tabStyle))hudExpanded=false;
    }
}


