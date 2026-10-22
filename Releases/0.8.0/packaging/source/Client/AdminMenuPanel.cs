using System.Text.Json;
using UnityEngine;

namespace SiSquadHud;

public sealed partial class SquadHud
{
    private AdminMenuPage adminMenu;
    private string adminMenuService="",adminMenuMessage="";
    private bool adminReloadConfirm;
    private Vector2 adminMenuScroll;
    private readonly Dictionary<string,string> adminMenuInputs=new();
    private bool AdminMenuSection=>menuTab=="balance-admin"||menuTab=="starter-admin";
    private void ResetAdminMenu()
    {adminMenu=null;adminMenuService="";adminMenuMessage="";adminMenuInputs.Clear();adminReloadConfirm=false;adminMenuScroll=Vector2.zero;}
    private void OpenAdminMenu(string service)
    {
        if(!serverAdmin||BalanceBusy)return;
        ClearBalanceEdits();adminMenuService=service;menuTab=service=="starter"?"starter-admin":"balance-admin";
        function="";page=null;pendingId="";confirmPurchase=0;SendAdminChoice("open");
    }
    private void SendAdminChoice(string choice,string value="")
    {
        if(!serverAdmin||BalanceBusy||!AdminMenuSection)return;
        adminReloadConfirm=false;
        string service=adminMenuService.Length>0?adminMenuService:menuTab=="starter-admin"?"starter":"balance";
        SendBalance("menu",JsonSerializer.Serialize(new{service,choice,value}));
        adminMenuMessage="Waiting for server menu...";
    }
    private bool ReceiveAdminMenu(string json)
    {
        using var document=JsonDocument.Parse(json);
        var root=document.RootElement;
        if(!root.TryGetProperty("Kind",out var kind)&&!root.TryGetProperty("kind",out kind))return false;
        if(kind.GetString()!="admin-menu")return false;
        var received=JsonSerializer.Deserialize<AdminMenuPage>(json,new JsonSerializerOptions{PropertyNameCaseInsensitive=true,MaxDepth=32});
        if(received==null||received.Rows.Count>256||received.Lines.Count>30||received.Rows.Any(r=>r.Id.Length>64||r.Label.Length>600))throw new InvalidDataException("Invalid admin menu snapshot.");
        if(balancePending?.Operation!="menu"||received.Request!=balancePending.Id||!AdminMenuSection)return true;
        if(received.Service.Length>0&&received.Service!=adminMenuService)return true;
        bool sameEntries=adminMenu!=null&&adminMenu.Title==received.Title&&adminMenu.Rows.Select(r=>r.Id).SequenceEqual(received.Rows.Select(r=>r.Id));
        balancePending=null;adminMenu=received;adminMenuMessage=received.Message.Length>0?received.Message:"Server menu ready. Changes follow the original mod's confirmation rules.";
        adminMenuInputs.Clear();adminReloadConfirm=false;if(!sameEntries)adminMenuScroll=Vector2.zero;
        Write("ADMIN MENU "+received.Service+"; entries="+received.Rows.Count+"; confirmation="+received.Confirmation);
        return true;
    }
    private void RenderAdminMenu(Rect viewport)
    {
        GUI.Label(new Rect(viewport.x,viewport.y,viewport.width,36),adminMenu?.Title??(adminMenuService=="starter"?"Starter Units":"Unit Balance / native menu"),titleStyle);
        if(adminMenu==null)
        {GUI.Label(new Rect(viewport.x,viewport.y+48,viewport.width,100),adminMenuMessage.Length>0?adminMenuMessage:"Choose Root to open the server menu.",bodyStyle);return;}
        float width=viewport.width-24,total=0;
        var lineHeights=adminMenu.Lines.Select(line=>Math.Max(25,smallStyle.CalcHeight(new GUIContent(line),width-16)+6)).ToArray();
        var rowHeights=adminMenu.Rows.Select(row=>Math.Max(46,bodyStyle.CalcHeight(new GUIContent(row.Label),Math.Max(100,row.Input?width*.53f:width-24))+14)).ToArray();
        total=lineHeights.Sum()+rowHeights.Sum()+12;
        var view=new Rect(viewport.x,viewport.y+44,viewport.width,Math.Max(32,viewport.height-44));
        bool inside=view.Contains(Event.current.mousePosition);
        adminMenuScroll=GUI.BeginScrollView(view,adminMenuScroll,new Rect(0,0,width,total),false,true);
        try
        {
            float y=0;
            for(int i=0;i<adminMenu.Lines.Count;i++)
            {GUI.Label(new Rect(8,y,width-16,lineHeights[i]),adminMenu.Lines[i],smallStyle);y+=lineHeights[i];}
            y+=10;
            for(int i=0;i<adminMenu.Rows.Count;i++)
            {
                var row=adminMenu.Rows[i];float height=rowHeights[i];bool allowed=serverAdmin&&row.Enabled&&!BalanceBusy;
                if(row.Input)
                {
                    Surface(new Rect(0,y,width,height-5),Color.Lerp(surface,accent,.055f));
                    GUI.Label(new Rect(12,y+7,width*.53f-20,height-10),row.Label,bodyStyle);
                    float inputX=width*.54f,inputWidth=Math.Max(48,width*.27f);
                    string value=adminMenuInputs.TryGetValue(row.Id,out var text)?text:"";
                    bool old=GUI.enabled;GUI.enabled=old&&allowed;
                    try{adminMenuInputs[row.Id]=GUI.TextField(new Rect(inputX,y+5,inputWidth,34),value,64,inputStyle);}
                    finally{GUI.enabled=old;}
                    if(Button(new Rect(inputX+inputWidth+8,y+5,width-inputX-inputWidth-14,34),"SET",false,allowed,inside?row.Hint:"",tabStyle))
                    {SendAdminChoice(row.Id,adminMenuInputs[row.Id]);return;}
                }
                else if(Button(new Rect(0,y,width,height-5),row.Label,false,allowed,inside?row.Hint:""))
                {SendAdminChoice(row.Id);return;}
                y+=height;
            }
        }
        finally{GUI.EndScrollView();}
    }
    private void RenderAdminMenuActions(Rect rect)
    {
        float width=(rect.width-20)/3;
        if(Button(new Rect(rect.x,rect.y,width,40),"BACK",false,!BalanceBusy&&adminMenu?.CanBack==true))SendAdminChoice("back");
        if(Button(new Rect(rect.x+width+10,rect.y,width,40),"ROOT",false,!BalanceBusy,"Return to the root menu and cancel this UI's pending selection."))SendAdminChoice("open");
        if(adminMenuService=="balance")
        {
            if(Button(new Rect(rect.x+2*(width+10),rect.y,width,40),adminReloadConfirm?"CONFIRM RELOAD":"RELOAD SAVED JSON",false,!BalanceBusy&&adminMenu?.Confirmation==false,
                "Calls Unit Balance's existing rebalance handler. Its configured Discord auto-post behavior is retained."))
            {if(adminReloadConfirm)SendAdminChoice("reload");else adminReloadConfirm=true;}
        }
        else GUI.Label(new Rect(rect.x+2*(width+10),rect.y+6,width,34),"Applies next round",smallStyle);
    }
}
