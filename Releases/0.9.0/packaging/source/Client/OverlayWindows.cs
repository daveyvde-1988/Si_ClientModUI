using Il2Cpp;
using Il2CppSilica;
using UnityEngine;
using System.Text.RegularExpressions;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private sealed class StatusParameter {internal string Id,Mod,Label,Value;}
    private bool notificationsEnabled,statusPickerOpen;
    private Rect notificationRect,statusRect;
    private Vector2 notificationScroll,statusScroll,pickerScroll;
    private readonly List<string> notices=new();
    private int noticeRevision;
    private bool NotificationPointerCaptured=>notificationsEnabled&&OverlayContains(notificationRect,"notifications");
    private bool StatusPointerCaptured=>!panelOpen&&OverlayContains(statusRect,hudExpanded?"status-expanded":"status-header");
    private void SetNotifications(bool enabled)
    {EnsureWindowProfile();notificationsEnabled=layout.Notifications=enabled;placementDirty=true;SaveWindowPosition();GameInput.UpdateInput();}
    private void AddNotification(string mod,string text)
    {
        text=Plain(text);
        text=Regex.Replace(text,@"https?://[^\s]*discord(?:app)?\.com/api/webhooks/[^\s]+","[private webhook]");
        text=Regex.Replace(text,@"(?m)^[ \t]*[=\-_\u2500-\u257F]{3,}[ \t]*\r?$","");
        text=Regex.Replace(text,@"\n[ \t]*\n(?:[ \t]*\n)+","\n\n").Trim();
        if(text.Length==0)return;
        notices.Add(DateTime.Now.ToString("HH:mm:ss")+"  "+mod+"\n"+text);noticeRevision++;
        if(notices.Count>200)notices.RemoveAt(0);
        notificationScroll.y=float.MaxValue;
    }
    private StatusParameter[] availableCache=Array.Empty<StatusParameter>();
    private double availableAt;
    private StatusParameter[] AvailableParameters()
    {
        if(clock.Elapsed.TotalSeconds<availableAt)return availableCache;
        availableAt=clock.Elapsed.TotalSeconds+.25;
        var p=Player.CurrentPlayer;var rows=new List<StatusParameter>();
        void Add(string id,string label,string value)=>rows.Add(new StatusParameter{Id=id,Mod="silica / client",Label=label,Value=value});
        Add("local.role","Role",p?.IsCommander==true?"Commander":"FPS / spectator");
        Add("local.score","Score",p?.Score.ToString("N0")??"Unavailable");
        Add("local.squad","FPS squad",SquadQuantity(out var detail)+" / "+detail);
        Add("local.fps","FPS",Time.smoothDeltaTime>0?Math.Round(1/Time.smoothDeltaTime).ToString():"Unavailable");
        Add("local.kills","Kills",p?.Kills.ToString()??"Unavailable");
        Add("local.deaths","Deaths",p?.Deaths.ToString()??"Unavailable");
        Add("local.nativecap","Native squad limit",FPSCommanding.Instance?.GroupUnitLimit.ToString()??"Unavailable");
        Add("local.team","Faction",p?.Team?.TeamName??"Unavailable");
        Add("local.tech","Technology tier",p?.Team?.TechnologyTier.ToString()??"Unavailable");
        Add("local.resources","Faction stored resources",p?.Team?.StoredResources.ToString("N0")??"Unavailable");
        var damage=p?.ControlledUnit?.DamageManager;
        Add("local.health","Controlled unit health",damage==null?"Unavailable":damage.Health.ToString("N0")+" / "+damage.MaxHealth.ToString("N0"));
        foreach(var value in observedValues)
            rows.Add(new StatusParameter{Id=value.Key,Mod=value.Value.Mod,Label=value.Value.Label,Value=ObservedValue(value.Key)});
        // Preserve old positive selections; absent server providers are not silently reinterpreted.
        availableCache=rows.OrderBy(r=>r.Mod).ThenBy(r=>r.Label).ToArray();return availableCache;
    }

    private void RenderStatusPicker()
    {
        var rect=LayoutRect("status-picker",600,650,.5f,.5f,430,300);WindowSurface(rect);PlaceWindow("status-picker",rect,rect.width-120,430,300);
        GUI.Label(new Rect(rect.x+16,rect.y+8,rect.width-140,32),"STATUS PARAMETERS",titleStyle);
        if(Button(new Rect(rect.xMax-96,rect.y+8,76,30),"CLOSE",style:tabStyle)){statusPickerOpen=false;return;}
        var rows=AvailableParameters();
        if(Button(new Rect(rect.x+16,rect.y+48,110,30),"SHOW ALL",style:tabStyle))
        {layout.MigrateStatus=false;foreach(var r in rows)layout.SelectedStatus.Add(r.Id);placementDirty=true;SaveWindowPosition();}
        if(Button(new Rect(rect.x+138,rect.y+48,110,30),"HIDE ALL",style:tabStyle))
        {layout.MigrateStatus=false;layout.SelectedStatus.Clear();placementDirty=true;SaveWindowPosition();}
        GUI.Label(new Rect(rect.x+16,rect.y+86,rect.width-32,42),"Select parameters for the expanded top-right status. Local data and values observed in received chat. No automatic queries.",smallStyle);
        var view=new Rect(rect.x+16,rect.y+136,rect.width-32,rect.height-156);
        float width=view.width-24,y=0;
        var groups=new Dictionary<string,List<StatusParameter>>();
        foreach(var row in rows)
        {if(!groups.TryGetValue(row.Mod,out var group))groups[row.Mod]=group=new List<StatusParameter>();group.Add(row);}
        float full=groups.Sum(g=>38+(layout.ExpandedStatusGroups.Contains(g.Key)?g.Value.Count*62:0));
        pickerScroll=GUI.BeginScrollView(view,pickerScroll,new Rect(0,0,width,full),false,true);
        try
        {
            foreach(var group in groups)
            {
                bool expanded=layout.ExpandedStatusGroups.Contains(group.Key);
                int selectedCount=group.Value.Count(r=>layout.SelectedStatus.Contains(r.Id));
                if(Button(new Rect(0,y,width,32),(expanded?"-  ":"+  ")+group.Key+"  ("+selectedCount+"/"+group.Value.Count+")",description:"Expand or collapse this mod's available status parameters."))
                {if(expanded)layout.ExpandedStatusGroups.Remove(group.Key);else layout.ExpandedStatusGroups.Add(group.Key);placementDirty=true;SaveWindowPosition();}
                y+=38;if(!expanded)continue;
                foreach(var row in group.Value)
                {
                    bool selected=layout.SelectedStatus.Contains(row.Id);
                    if(Button(new Rect(14,y,width-14,54),(selected?"[x]  ":"[ ]  ")+row.Label+"\n"+row.Value,selected,description:"Show or hide this parameter in expanded status."))
                    {layout.MigrateStatus=false;if(selected)layout.SelectedStatus.Remove(row.Id);else layout.SelectedStatus.Add(row.Id);placementDirty=true;SaveWindowPosition();}
                    y+=62;
                }
            }
        }
        finally{GUI.EndScrollView();}
    }
    private void RenderNotifications()
    {
        notificationRect=LayoutRect("notifications",580,300,0,1,400,160);var rect=notificationRect;
        WindowSurface(rect);PlaceWindow("notifications",rect,rect.width-180,400,160);
        GUI.Label(new Rect(rect.x+14,rect.y+8,rect.width-190,30),"MOD NOTIFICATIONS",smallStyle);
        if(Button(new Rect(rect.xMax-168,rect.y+7,78,28),"CLEAR",style:tabStyle)){notices.Clear();noticeRevision++;}
        if(Button(new Rect(rect.xMax-82,rect.y+7,60,28),"X",description:"Disable the notification bar and restore normal mod chat delivery.",style:tabStyle)){SetNotifications(false);return;}
        var viewport=new Rect(rect.x+12,rect.y+44,rect.width-24,rect.height-64);float width=viewport.width-24;
        if(notices.Count==0){GUI.Label(viewport,"Mod and server notifications will appear here. Player chat stays in chat.",smallStyle);return;}
        // Cache by width/content rather than measure the whole history on every GUI event.
        MeasureNotices(width);
        notificationScroll=GUI.BeginScrollView(viewport,notificationScroll,new Rect(0,0,width,noticeHeight),false,true);
        try
        {
            float y=0;
            for(int i=0;i<notices.Count;i++)
            {
                float height=noticeHeights[i];
                if(y+height>=notificationScroll.y&&y<=notificationScroll.y+viewport.height)
                GUI.Label(new Rect(4,y+4,width-8,height-10),notices[i],smallStyle);
                y+=height;
            }
        }
        finally{GUI.EndScrollView();}
    }
    private readonly List<float> noticeHeights=new();
    private float noticeWidth,noticeHeight;
    private int noticeMeasureRevision=-1;
    private int noticeFont;
    private void MeasureNotices(float width)
    {
        if(width==noticeWidth&&noticeRevision==noticeMeasureRevision&&noticeFont==appearanceFont)return;
        noticeWidth=width;noticeMeasureRevision=noticeRevision;noticeFont=appearanceFont;noticeHeights.Clear();noticeHeight=0;
        foreach(var line in notices){float h=smallStyle.CalcHeight(new GUIContent(line),width-8)+14;noticeHeights.Add(h);noticeHeight+=h;}
    }
}


