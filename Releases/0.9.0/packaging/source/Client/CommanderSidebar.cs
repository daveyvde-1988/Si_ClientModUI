using Il2Cpp;
using Il2CppSilica;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private bool sidebarMode,lastSidebarCapture;
    private Rect sidebarRect;
    private Vector2 sidebarScroll;
    private readonly Dictionary<string,(Texture2D texture,GUIStyle style)> sidebarIcons=new();
    private void SetSidebarMode(bool value)
    {EnsureWindowProfile();sidebarMode=layout.Sidebar=value;placementDirty=true;SaveWindowPosition();GameInput.UpdateInput();}
    private bool SidebarVisible=>sidebarMode&&Player.CurrentPlayer?.IsCommander==true;
    private bool SidebarPointerCaptured=>SidebarVisible&&OverlayContains(sidebarRect,"sidebar");
    private bool OverlayContains(Rect rect,string key)
    {
        key=RoleWindowKey(key);
        if(movingWindow==key)return true;if(movingWindow.Length>0)return false;
        var mouse=Mouse.current;if(mouse==null)return false;var point=mouse.position.ReadValue();
        return Cursor.visible&&rect.Contains(new Vector2(point.x,Screen.height-point.y));
    }
    private void RenderSidebar()
    {
        var entries=new List<CommandEntry>{
            new(){Mod="si_formation",Command="/formation",Description="Open the native formation chat menu."},
            new(){Mod="si_formation",Command="/formation status",Description="Request formation status once."},
            new(){Mod="si_formation",Command="/formation lock",Description="Arm the next locked commander move."},
            new(){Mod="si_formation",Command="/formation unlock",Description="Unlock commander formations."}};
        entries.AddRange(dynamicCommands.Where(c=>c.Mod=="si_formation"&&c.Source=="menu"&&activeExchange?.Mod==c.Mod&&clock.Elapsed.TotalSeconds-activeExchange.At<60));
        sidebarRect=LayoutRect("sidebar",140,320,0,.5f,110,160);var rect=sidebarRect;
        WindowSurface(rect);PlaceWindow("sidebar",rect,rect.width-28,110,160);
        GUI.Label(new Rect(rect.x+8,rect.y+5,rect.width-40,24),"FORMATION",smallStyle);
        if(Button(new Rect(rect.xMax-28,rect.y+3,24,24),"X",style:tabStyle)){SetSidebarMode(false);return;}
        var view=new Rect(rect.x+6,rect.y+34,rect.width-12,rect.height-76);
        sidebarScroll=GUI.BeginScrollView(view,sidebarScroll,new Rect(0,0,view.width-24,entries.Count*48),false,true);
        try
        {
            for(int i=0;i<entries.Count;i++)
            {
                var row=entries[i];string label=i switch{0=>"MENU",1=>"STATUS",2=>"LOCK",3=>"UNLOCK",_=>row.Command};
                if(Button(new Rect(0,i*48,view.width-26,42),label,description:row.Description)){ClickCommand(row);break;}
            }
        }
        finally{GUI.EndScrollView();}
        GUI.Label(new Rect(rect.x+6,rect.yMax-36,rect.width-12,32),"Chat controls",smallStyle);
    }

    private (Texture2D texture,GUIStyle style) CreateSidebarIcon(string name)
    {
        // Owned texture backgrounds drawn through the working GUI.Box path, never DrawTexture.
        var texture=new Texture2D(40,40,TextureFormat.RGBA32,false){hideFlags=HideFlags.HideAndDontSave,wrapMode=TextureWrapMode.Clamp};
        bool large=name.Contains("large"),open=name.Contains("open");
        float radius=large?15:10;
        for(int y=0;y<40;y++)for(int x=0;x<40;x++)
        {
            float dx=x-19.5f,dy=y-19.5f,ax=Math.Abs(dx),ay=Math.Abs(dy);
            float distance=MathF.Sqrt(dx*dx+dy*dy);bool ink;
            if(name=="padlock")ink=(ax<=11&&dy>=-13&&dy<=3&&!(ax<1.5f&&dy<0&&dy>-8))||
                (dy>=2&&dy<=14&&distance<=14&&distance>=10);
            else if(name.Contains("line"))ink=ax<=15&&ay<=(large?4:1.5f);
            else if(name.Contains("hexagon"))
            {
                float edge=Math.Max(ax*.8660254f+ay*.5f,ay);
                ink=edge<=radius&&(!open||edge>=radius-2);
            }
            else if(name.Contains("square"))ink=ax<=radius&&ay<=radius&&(!open||Math.Max(ax,ay)>=radius-2);
            else if(name.Contains("circle")||name.Contains("disc"))ink=distance<=radius&&(!open||distance>=radius-2);
            else ink=ay+ax<=radius&&(ay+ax>=radius-2);
            texture.SetPixel(x,y,new Color(1,1,1,ink?1:0));
        }
        texture.Apply();var style=new GUIStyle();style.normal.background=texture;
        return(texture,style);
    }
    private void ResetSidebarIcons()
    {
        foreach(var icon in sidebarIcons.Values)if(icon.texture!=null)UnityEngine.Object.Destroy(icon.texture);
        sidebarIcons.Clear();
    }
}


