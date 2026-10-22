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
    private bool StatusPointerCaptured=>!panelOpen&&(layout.ShowModMenuButton&&OverlayContains(statusRect,"status-header")||layout.StatusEnabled&&OverlayContains(statusWindowRect,"mod-status-window"));
    private bool NotificationSelected(string mod)=>!layout.MutedNotificationMods.Contains(ModLabel(mod));
    private bool ShouldInterceptNotification()=>notificationsEnabled&&drawingEnabled&&feedback.Count>0&&NotificationSelected(feedback[^1].Mod);
    private void SetNotifications(bool enabled)
    {EnsureWindowProfile();notificationsEnabled=layout.Notifications=enabled;placementDirty=true;SaveWindowPosition();GameInput.UpdateInput();}
    private void AddNotification(string mod,string text)
    {
        if(!NotificationSelected(mod))return;
        text=Plain(text);
        text=Regex.Replace(text,@"https?://[^\s]*discord(?:app)?\.com/api/webhooks/[^\s]+","[private webhook]");
        text=Regex.Replace(text,@"(?m)^[ \t]*[=\-_\u2500-\u257F]{3,}[ \t]*\r?$","");
        text=Regex.Replace(text,@"\n[ \t]*\n(?:[ \t]*\n)+","\n\n").Trim();
        if(text.Length==0)return;if(text.Length>8192)text=text.Substring(0,8192);
        notices.Add(DateTime.Now.ToString("HH:mm:ss")+"  "+ModLabel(mod)+"\n"+text);noticeRevision++;
        if(notices.Count>200)notices.RemoveAt(0);notificationScroll.y=float.MaxValue;
    }
    private StatusParameter[] availableCache=Array.Empty<StatusParameter>();
    private double availableAt;
    private string availableContext="";
    private StatusParameter[] AvailableParameters()
    {
        string context=ServerScope+":"+CurrentAdmin+":"+CurrentRole+":"+localMods.Count+":"+ClientModControls.Revision+":"+serverCatalog.Root;
        if(availableContext==context&&clock.Elapsed.TotalSeconds<availableAt)return availableCache;
        availableContext=context;availableAt=clock.Elapsed.TotalSeconds+UiPolicies.HousekeepingSeconds;
        uiStatusRefreshes++;
        var rows=new List<StatusParameter>();
        foreach(var owner in localMods)
        {
            if(!MelonLoader.MelonMod.RegisteredMelons.Contains(owner))continue;
            var definition=ClientModControls.For(owner);if(definition?.Statuses==null)continue;
            foreach(var status in definition.Statuses)
            {
                if(status==null||string.IsNullOrWhiteSpace(status.Id)||status.AdminOnly&&!CurrentAdmin||!RoleAllowed(status.Role))continue;
                string value;
                try{value=status.Read?.Invoke()??"Unavailable";}catch{value="Unavailable";}
                rows.Add(new StatusParameter{Id=LocalScope(owner)+":"+status.Id,Mod=LocalTitle(owner)+(string.IsNullOrWhiteSpace(status.Group)?"":" / "+ModLabel(status.Group)),Label=status.Label,Value=value});
                if(rows.Count>=256)break;
            }
            if(rows.Count>=256)break;
        }
        if(companionReady)foreach(var module in serverCatalog.Modules.Values)
        {
            if(!module.Data.HasValue)continue;
            var data=module.Data.Value;
            if(data.TryGetProperty("stats",out var stats)&&stats.ValueKind==System.Text.Json.JsonValueKind.Array)
            foreach(var stat in stats.EnumerateArray().Take(128))
            {
                string id=JsonText(stat,"id"),label=JsonText(stat,"label"),value=JsonText(stat,"value"),group=JsonText(stat,"group");
                if(id.Length>0&&id.Length<=80&&label.Length<=160&&value.Length<=2048)
                    rows.Add(new StatusParameter{Id="server:"+module.Id+":"+id,Mod=ModLabel(module.Name)+(group.Length>0?" / "+group:""),Label=label,Value=value});
            }
            if(data.TryGetProperty("info",out var info)&&info.ValueKind==System.Text.Json.JsonValueKind.Array)
            {int i=0;foreach(var line in info.EnumerateArray().Take(8))if(line.ValueKind==System.Text.Json.JsonValueKind.String)rows.Add(new StatusParameter{Id="server:"+module.Id+":info:"+(i++),Mod=ModLabel(module.Name),Label="Summary "+i,Value=Plain(line.GetString())});}
        }
        availableCache=rows.Take(512).ToArray();return availableCache;
    }

    private void RenderStatusPicker()
    {
        var rect=LayoutRect("status-picker",600,650,.5f,.5f,430,300);WindowSurface(rect);PlaceWindow("status-picker",rect,rect.width-120,430,300);
        GUI.Label(new Rect(rect.x+16,rect.y+8,rect.width-140,32),"STATUS PARAMETERS",titleStyle);
        if(Button(new Rect(rect.xMax-96,rect.y+8,76,30),"CLOSE",style:tabStyle)){statusPickerOpen=false;return;}
        var rows=AvailableParameters();
        if(Button(new Rect(rect.x+16,rect.y+48,110,30),"SHOW ALL",style:tabStyle))
        {foreach(var r in rows)layout.SelectedStatus.Add(r.Id);placementDirty=true;SaveWindowPosition();InvalidateUi();}
        if(Button(new Rect(rect.x+138,rect.y+48,110,30),"HIDE ALL",style:tabStyle))
        {layout.SelectedStatus.Clear();placementDirty=true;SaveWindowPosition();InvalidateUi();}
        GUI.Label(new Rect(rect.x+16,rect.y+86,rect.width-32,42),"Select values supplied by loaded mods. Nothing is selected by default.",smallStyle);
        if(rows.Length==0)
        {GUI.Label(new Rect(rect.x+16,rect.y+144,rect.width-32,80),"No mod has supplied status values yet.",bodyStyle);return;}
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
                    {if(selected)layout.SelectedStatus.Remove(row.Id);else layout.SelectedStatus.Add(row.Id);placementDirty=true;SaveWindowPosition();InvalidateUi();}
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
        if(width==noticeWidth&&noticeRevision==noticeMeasureRevision&&noticeFont==smallStyle.fontSize)return;
        noticeWidth=width;noticeMeasureRevision=noticeRevision;noticeFont=smallStyle.fontSize;noticeHeights.Clear();noticeHeight=0;
        foreach(var line in notices){float h=smallStyle.CalcHeight(new GUIContent(line),width-8)+14;noticeHeights.Add(h);noticeHeight+=h;}
    }
}




