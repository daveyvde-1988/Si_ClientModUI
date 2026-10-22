using System.Globalization;
using System.Text.Json;
using Il2Cpp;
using MelonLoader.Utils;
using UnityEngine;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private LocalLayout layout=new();
    private string placementProfile="",placementPath="",movingWindow="";
    private bool placementDirty,resizingWindow;
    private int windowDragControl;
    private bool legacySidebarMigrationComplete;
    private void EnsureWindowProfile()
    {
        var p=Player.CurrentPlayer;if(p==null)return;
        string id=p.PlayerID.SteamID.m_SteamID.ToString(CultureInfo.InvariantCulture);
        if(id=="0")id=NetworkID.CurrentUserID.SteamID.m_SteamID.ToString(CultureInfo.InvariantCulture);
        if(id==placementProfile){MigrateSidebarTeam();return;}
        EndWindowDrag();placementProfile=id;layout=new();legacySidebarMigrationComplete=false;
        // A clean shell namespace: old HUD layouts, catalogs and preferences are never imported.
        placementPath=Path.Combine(MelonEnvironment.UserDataDirectory,"Si_ModMenu","layout-"+id+".json");
        try
        {
            if(File.Exists(placementPath))layout=JsonSerializer.Deserialize<LocalLayout>(File.ReadAllText(placementPath))??new();
            layout.Windows??=new();layout.SelectedStatus??=new();layout.ExpandedStatusGroups??=new();
            layout.SidebarButtons??=new();
            layout.FeedbackSplit??=new();layout.MutedNotificationMods??=new();
            layout.RemovedCustomCommands??=new();
            layout.Transparency=float.IsFinite(layout.Transparency)?Math.Clamp(layout.Transparency,0,100):6;
            layout.TextSize=float.IsFinite(layout.TextSize)?Math.Clamp(layout.TextSize,75,150):100;
        }
        catch(Exception e){Error(e);layout=new();}
        layout.ClientDebug=false; // Diagnostics are opt-in for each client session.
        notificationsEnabled=layout.Notifications;
        MigrateSidebarTeam();
        LoadUserCommands();
        LoadPortableCommands();ExportCreatedCommands();
        InvalidateUi();
    }
    private void MigrateSidebarTeam()
    {
        if(legacySidebarMigrationComplete)return;
        if(!layout.SidebarButtons.Any(b=>b!=null&&string.IsNullOrEmpty(b.TeamScope))){legacySidebarMigrationComplete=true;return;}
        if(Player.CurrentPlayer?.Team==null)return;
        try
        {
            if(File.Exists(placementPath))File.Copy(placementPath,placementPath+".before-team-sidebar-"+DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ")+".bak",false);
            int count=ShellRules.AssignLegacy(layout.SidebarButtons,CurrentSidebarTeam,b=>b.TeamScope,(b,t)=>b.TeamScope=t);
            if(count>0){placementDirty=true;SaveWindowPosition();Write("Assigned "+count+" existing sidebar buttons to "+CurrentSidebarTeam+"; previous layout backed up.");}
        }
        catch(Exception e){Error(e,"sidebar team migration");}
    }

    private string RoleWindowKey(string key)=>UiPolicies.WindowKey(key,CurrentSidebarTeam+":"+(Player.CurrentPlayer?.Team?.Index??-1),Player.CurrentPlayer?.IsCommander==true);
    private Rect LayoutRect(string key,float width,float height,float x=.5f,float y=.5f,float minWidth=240,float minHeight=120,bool fixedHeight=false)
    {
        EnsureWindowProfile();
        string oldKey=key+"-"+CurrentRole;
        key=RoleWindowKey(key);
        if(!layout.Windows.TryGetValue(key,out var w)||w==null)
        {
            // Copy legacy role-only geometry into each team's independent slot on first use.
            if(layout.Windows.TryGetValue(oldKey,out var old)&&old!=null)
                w=new LocalWindow{X=old.X,Y=old.Y,Width=old.Width,Height=old.Height};
            else w=new LocalWindow{X=x,Y=y};
            layout.Windows[key]=w;placementDirty=true;
        }
        if(!float.IsFinite(w.X)||!float.IsFinite(w.Y)||!float.IsFinite(w.Width)||!float.IsFinite(w.Height))w=layout.Windows[key]=new LocalWindow{X=x,Y=y};
        width=Math.Clamp(w.Width>0?w.Width:width,Math.Min(minWidth,UiWidth-24),UiWidth-24);
        height=Math.Clamp(!fixedHeight&&w.Height>0?w.Height:height,Math.Min(minHeight,UiHeight-24),UiHeight-24);
        return new Rect(12+Math.Max(0,UiWidth-width-24)*Math.Clamp(w.X,0,1),12+Math.Max(0,UiHeight-height-24)*Math.Clamp(w.Y,0,1),width,height);
    }
    private Rect WindowRect(float width,float height)=>LayoutRect("menu",width,height,minWidth:760,minHeight:600);
    private void SaveWindowPosition()
    {
        if(!placementDirty||placementPath.Length==0)return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(placementPath));string temporary=placementPath+".tmp";
            File.WriteAllText(temporary,JsonSerializer.Serialize(layout));File.Move(temporary,placementPath,true);placementDirty=false;
        }
        catch(Exception error){Error(error);}
    }
    private void EndWindowDrag()
    {
        if(movingWindow.Length>0&&GUIUtility.hotControl==windowDragControl)GUIUtility.hotControl=0;
        movingWindow="";SaveWindowPosition();
    }
    private void DragWindow(Rect rect)=>PlaceWindow("menu",rect,200,760,600);
    private void PlaceWindow(string key,Rect rect,float dragWidth,float minWidth=240,float minHeight=120,bool resize=true)
    {
        key=RoleWindowKey(key);
        int id=GUIUtility.GetControlID(key.GetHashCode(),FocusType.Passive);var e=Event.current;
        bool interactive=GUI.enabled&&Application.isFocused&&Cursor.visible&&!NativeMenu()&&!NativeTextInput();
        var grip=new Rect(rect.xMax-18,rect.yMax-18,18,18);
        if(resize)GUI.Label(grip,"//",smallStyle);
        if(interactive&&e.type==EventType.MouseDown&&e.button==0&&
            (resize&&grip.Contains(e.mousePosition)||new Rect(rect.x+5,rect.y+3,Math.Max(0,dragWidth-5),28).Contains(e.mousePosition)))
        {GUIUtility.hotControl=id;windowDragControl=id;movingWindow=key;resizingWindow=resize&&grip.Contains(e.mousePosition);hoverKey="";e.Use();}
        if(movingWindow!=key)return;
        if(!interactive){EndWindowDrag();return;}
        if(e.type==EventType.MouseDrag&&GUIUtility.hotControl==windowDragControl)
        {
            var w=layout.Windows[key];float width=rect.width,height=rect.height,x=rect.x,y=rect.y;
            if(resizingWindow)
            {
                width=Math.Clamp(width+e.delta.x,Math.Min(minWidth,UiWidth-24),UiWidth-24);
                height=Math.Clamp(height+e.delta.y,Math.Min(minHeight,UiHeight-24),UiHeight-24);
                w.Width=width;w.Height=height;
            }
            else{x+=e.delta.x;y+=e.delta.y;}
            w.X=UiWidth-width-24>0?Math.Clamp((x-12)/(UiWidth-width-24),0,1):0;
            w.Y=UiHeight-height-24>0?Math.Clamp((y-12)/(UiHeight-height-24),0,1):0;
            placementDirty=true;e.Use();
        }
        if(e.type==EventType.MouseUp&&e.button==0){EndWindowDrag();e.Use();}
    }
}




