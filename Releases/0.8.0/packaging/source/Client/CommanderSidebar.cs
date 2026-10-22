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
    private PanelPage sidebarPage;
    private string sidebarContext="",sidebarPending="",sidebarBody="";
    private double sidebarAt,sidebarPoll;
    private bool sidebarRetried,sidebarAction;
    private readonly Dictionary<int,PanelPage> sidebarIncoming=new();
    private readonly Dictionary<string,(Texture2D texture,GUIStyle style)> sidebarIcons=new();
    private void SetSidebarMode(bool value)
    {
        EnsureWindowProfile();sidebarMode=layout.Sidebar=value;placementDirty=true;SaveWindowPosition();
        lastSidebarCapture=value&&SidebarPointerCaptured;GameInput.UpdateInput();
    }
    private bool SidebarVisible=>sidebarMode&&Player.CurrentPlayer?.IsCommander==true&&TabVisible("formations");
    private bool SidebarPointerCaptured=>SidebarVisible&&OverlayContains(sidebarRect,"sidebar");
    private bool OverlayContains(Rect rect,string key)
    {
        if(movingWindow==key)return true;
        if(movingWindow.Length>0)return false;
        var mouse=Mouse.current;if(mouse==null)return false;var point=mouse.position.ReadValue();
        return Cursor.visible&&rect.Contains(new Vector2(point.x,Screen.height-point.y));
    }
    private void EndSidebarDrag()=>EndWindowDrag();
    private void MaintainSidebar(Player player,double now)
    {
        string context=cache.Nonce+":"+(player.Team?.Index??-1)+":"+player.IsCommander;
        if(context!=sidebarContext){sidebarContext=context;sidebarPage=null;sidebarPending="";sidebarIncoming.Clear();sidebarPoll=0;}
        if(!SidebarVisible||!networkEnabled||player.Team==null)return;
        if(sidebarPending.Length>0)
        {
            if(now-sidebarAt>8){sidebarPending="";sidebarIncoming.Clear();sidebarPoll=now+3;}
            else if(!sidebarRetried&&now-sidebarAt>2){sidebarRetried=true;SendBody(sidebarBody);}
            return;
        }
        // Reuse the menu's authoritative snapshot; never double-poll the same formation page.
        if(panelOpen&&function=="commander"&&!BalanceSection&&page!=null){sidebarPage=page;sidebarPoll=now+1;return;}
        if(now>=sidebarPoll&&pendingId.Length==0&&!BalanceBusy&&now>=controlReadyAt)RequestSidebar();
    }
    private void RequestSidebar(string action="",int option=0)
    {
        var p=Player.CurrentPlayer;
        if(!SidebarVisible||p?.Team==null||sidebarPending.Length>0||pendingId.Length>0||BalanceBusy||clock.Elapsed.TotalSeconds<controlReadyAt)return;
        var r=new PanelRequest{Nonce=cache.Nonce,Id=Guid.NewGuid().ToString("N"),Channel=p.PlayerChannel,Team=p.Team.Index,Commander=true,
            Function="commander",Page=HudProtocol.FullCatalog,Action=action,Option=option,Revision=sidebarPage?.Revision??"",Known=action.Length==0&&sidebarPage!=null?HudProtocol.CatalogFingerprint(sidebarPage):""};
        sidebarPending=r.Id;sidebarAt=clock.Elapsed.TotalSeconds;sidebarRetried=false;sidebarAction=action.Length>0;sidebarIncoming.Clear();controlReadyAt=sidebarAt+.35;
        sidebarBody=HudProtocol.Control(r);SendBody(sidebarBody);
    }
    private bool SidebarScope(string nonce,string id,int team,bool commander,string section)=>
        nonce==cache.Nonce&&id==sidebarPending&&sidebarPending.Length>0&&team==Player.CurrentPlayer?.Team?.Index&&commander&&section=="commander";
    private bool ReceiveSidebarUnchanged(PanelRequest scope)
    {
        if(!SidebarScope(scope.Nonce,scope.Id,scope.Team,scope.Commander,scope.Function))return false;
        if(sidebarPage==null||HudProtocol.CatalogFingerprint(sidebarPage)!=scope.Known)sidebarPage=null;
        sidebarPending="";sidebarIncoming.Clear();sidebarPoll=clock.Elapsed.TotalSeconds+(sidebarPage==null?0:1);return true;
    }
    private bool ReceiveSidebar(PanelPage result)
    {
        if(!SidebarScope(result.Nonce,result.Id,result.Team,result.Commander,result.Function))return false;
        if(sidebarIncoming.Count>0)
        {
            var first=sidebarIncoming.Values.First();
            if(first.Revision!=result.Revision||first.Pages!=result.Pages||first.Selected!=result.Selected||first.Locked!=result.Locked||first.Enabled!=result.Enabled||first.Available!=result.Available)sidebarIncoming.Clear();
        }
        sidebarIncoming[result.Page]=result;
        if(sidebarIncoming.Values.Sum(p=>p.Numbers.Length)>HudProtocol.MaxCatalogRows){sidebarIncoming.Clear();return true;}
        if(sidebarIncoming.Count<result.Pages)return true;
        var parts=Enumerable.Range(0,result.Pages).Select(i=>sidebarIncoming[i]).ToArray();
        var numbers=new List<int>();var names=new List<string>();var groups=new List<string>();var details=new List<string>();var data=new List<string>();
        foreach(var part in parts){numbers.AddRange(part.Numbers);names.AddRange(part.Names);groups.AddRange(part.Groups);details.AddRange(part.Details);data.AddRange(part.Data);}
        result.Numbers=numbers.ToArray();result.Names=names.ToArray();result.Groups=groups.ToArray();result.Details=details.ToArray();result.Data=data.ToArray();
        if(result.Numbers.Distinct().Count()!=result.Numbers.Length){sidebarIncoming.Clear();return true;}
        result.Page=0;result.Pages=1;sidebarPage=result;sidebarIncoming.Clear();sidebarPending="";sidebarPoll=clock.Elapsed.TotalSeconds+1;
        // A sidebar action invalidates the full-menu cached selection too.
        if(function=="commander"){page=result;catalogCache[function]=result;nextPagePoll=sidebarPoll;}
        return true;
    }
    private void RenderSidebar()
    {
        var page=sidebarPage;
        int count=page?.Available==true?page.Numbers.Length:1;
        sidebarRect=LayoutRect("sidebar",82,76+count*48,0,.5f,72,160);
        var rect=sidebarRect;float width=rect.width,height=rect.height;
        WindowSurface(rect);PlaceWindow("sidebar",rect,width-26,72,160);
        GUI.Label(new Rect(rect.x+10,rect.y+5,36,24),"=",smallStyle);
        if(Button(new Rect(rect.x+width-26,rect.y+3,23,24),"X",false,true,"Hide the sidebar. Enable it again from the mod menu.",tabStyle)){SetSidebarMode(false);return;}
        var viewport=new Rect(rect.x+5,rect.y+32,width-10,height-80);
        if(page?.Available==true)
        {
            bool inside=viewport.Contains(Event.current.mousePosition);
            sidebarScroll=GUI.BeginScrollView(viewport,sidebarScroll,new Rect(0,0,width-30,count*48),false,false);
            try
            {
                for(int i=0;i<count;i++)
                {
                    bool locked=page.Groups[i]=="commander-lock";
                    bool selected=locked?page.Locked:page.Selected==page.Numbers[i];
                    string name=page.Names[i];
                    string hint=locked?"Lock formation / "+(page.Locked?"Locked":"Unlocked")+". units attempt to stay in formation trough travel, formation will move slow. Arms the next commander move; click again to unlock and cancel a pending lock.":name;
                    var button=new Rect(0,i*48,width-30,44);
                    if(Button(button,page.Numbers[i]==1&&!locked?"N":"",selected,sidebarPending.Length==0||!sidebarAction,inside?hint:"",tabStyle)&&pendingId.Length==0&&!BalanceBusy&&clock.Elapsed.TotalSeconds>=controlReadyAt)
                        RequestSidebar(locked?"lock":"select",locked?(page.Locked?0:1):page.Numbers[i]);
                    if(page.Numbers[i]!=1||locked)
                    {
                        string key=locked?"padlock":name.ToLowerInvariant();
                        if(!sidebarIcons.TryGetValue(key,out var icon)){icon=CreateSidebarIcon(key);sidebarIcons[key]=icon;}
                        var color=GUI.color;
                        try{GUI.color=selected?accent:Color.white;GUI.Box(new Rect((width-70)/2,i*48+2,40,40),"",icon.style);}
                        finally{GUI.color=color;}
                    }
                }
            }
            finally{GUI.EndScrollView();}
        }
        else GUI.Label(viewport,"...",smallStyle);
        if(Button(new Rect(rect.x+5,rect.y+height-43,width-10,26),"MENU",false,true,"Open the full mod menu. Sidebar stays enabled.",tabStyle))
        {if(!panelOpen){hudExpanded=true;TogglePanel();}}
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
