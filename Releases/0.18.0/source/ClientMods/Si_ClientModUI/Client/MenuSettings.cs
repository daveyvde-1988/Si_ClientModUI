using UnityEngine;
namespace SiSquadHud;
public sealed partial class SquadHud
{
    private bool settingsMenuOpen;
    private float UiScale=>Math.Clamp(layout.TextSize,75,150)/100f;
    private int UiWidth=>(int)(Screen.width/UiScale);
    private int UiHeight=>(int)(Screen.height/UiScale);
    private void PersistSetting(){placementDirty=true;SaveWindowPosition();InvalidateUi(true);}
    private bool Check(Rect r,string label,bool value)
    {
        if(Button(r,(value?"[x] ":"[ ] ")+label,value))return !value;return value;
    }
    private void RenderSettingsPage(Rect view)
    {
        switch(settingsPage)
        {
            case "STATUS":RenderStatusOptions(view);break;
            case "SIDEBAR":RenderSidebarMenu(view);break;
            case "NOTIFICATIONS":RenderNotificationSettings(view);break;
            case "DEBUG":RenderClientDebug(view);break;
            default:RenderSettings(view);break;
        }
    }
    private void RenderSettings(Rect view)
    {
        GUI.Label(new Rect(view.x,view.y,view.width,32),"GENERAL",titleStyle);
        GUI.Label(new Rect(view.x,view.y+46,view.width,26),"Transparency: "+Math.Round(layout.Transparency)+"%",bodyStyle);
        float t=GUI.HorizontalSlider(new Rect(view.x,view.y+84,view.width,26),layout.Transparency,0,100);
        GUI.Label(new Rect(view.x,view.y+126,view.width,26),"Text size: "+Math.Round(layout.TextSize)+"%",bodyStyle);
        float size=GUI.HorizontalSlider(new Rect(view.x,view.y+164,view.width,26),layout.TextSize,75,150);
        if(Math.Abs(t-layout.Transparency)>.05f||Math.Abs(size-layout.TextSize)>.05f){layout.Transparency=t;layout.TextSize=size;placementDirty=true;InvalidateUi(true);}
        if(placementDirty&&UnityEngine.InputSystem.Mouse.current?.leftButton.isPressed!=true)SaveWindowPosition();
        bool remember=Check(new Rect(view.x,view.y+216,view.width,36),"Remember last menu",layout.RememberLastMenu);
        if(remember!=layout.RememberLastMenu){layout.RememberLastMenu=remember;PersistSetting();}
        bool showMenu=Check(new Rect(view.x,view.y+260,view.width,36),"Show Mod menu button",layout.ShowModMenuButton);
        if(showMenu!=layout.ShowModMenuButton){layout.ShowModMenuButton=showMenu;PersistSetting();}
        GUI.Label(new Rect(view.x,view.y+308,view.width,68),"F6 opens HELP by default. Remember last menu restores the last tab and selected page. Appearance and positions are saved locally.",smallStyle);
        if(Button(new Rect(view.x,view.y+388,220,36),"IMPORT SHARED JSON")){LoadPortableCommands();controllerMessage=portableReadFailed?"Import failed.":"Shared commands read.";}
        GUI.Label(new Rect(view.x,view.y+436,view.width,90),"Share UserData/Si_ModMenu/created-commands.json for saved chat controls. Provider code and server settings remain with their owning mods.",smallStyle);
    }
    private void RenderStatusOptions(Rect view)
    {
        bool show=Check(new Rect(view.x,view.y,view.width,36),"Enable separate status window",layout.StatusEnabled);
        if(show!=layout.StatusEnabled){layout.StatusEnabled=show;PersistSetting();}
        var rows=AvailableParameters();
        var groups=rows.GroupBy(r=>r.Mod).ToArray();
        float height=groups.Sum(g=>42+g.Count()*42);
        var area=new Rect(view.x,view.y+52,view.width,Math.Max(30,view.height-52));
        pickerScroll=GUI.BeginScrollView(area,pickerScroll,new Rect(0,0,view.width-26,height+60),false,true);
        try
        {
            float y=0,w=view.width-30;
            foreach(var group in groups)
            {
                GUI.Label(new Rect(0,y,w,30),group.Key,titleStyle);y+=42;
                foreach(var row in group)
                {
                    bool selected=layout.SelectedStatus.Contains(row.Id),next=Check(new Rect(8,y,w-8,34),row.Label+" : "+row.Value,selected);
                    if(next!=selected){if(next)layout.SelectedStatus.Add(row.Id);else layout.SelectedStatus.Remove(row.Id);PersistSetting();}
                    y+=42;
                }
            }
            if(rows.Length==0)GUI.Label(new Rect(0,0,w,80),"No mod has supplied status values yet.",smallStyle);
        }
        finally{GUI.EndScrollView();}
    }
    private void RenderNotificationSettings(Rect view)
    {
        bool enabled=Check(new Rect(view.x,view.y,view.width,36),"Enable notification window / interception",notificationsEnabled);
        if(enabled!=notificationsEnabled)SetNotifications(enabled);
        GUI.Label(new Rect(view.x,view.y+44,view.width,66),"Selected mods can use the notification window. All observed messages also remain in COMMANDS chat history. Player chat stays in native chat.",smallStyle);
        var mods=localMods.Select(LocalTitle).Concat(companionReady?serverCatalog.Modules.Values.Select(m=>ModLabel(m.Name)):Array.Empty<string>()).Concat(feedback.Select(f=>ModLabel(f.Mod))).Append("Server").Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x=>x).ToArray();
        var area=new Rect(view.x,view.y+120,view.width,Math.Max(30,view.height-120));
        settingsScroll=GUI.BeginScrollView(area,settingsScroll,new Rect(0,0,view.width-26,mods.Length*42+10),false,true);
        try
        {
            for(int i=0;i<mods.Length;i++)
            {
                bool selected=!layout.MutedNotificationMods.Contains(mods[i]),next=Check(new Rect(0,i*42,view.width-30,34),mods[i],selected);
                if(next!=selected){if(next)layout.MutedNotificationMods.Remove(mods[i]);else layout.MutedNotificationMods.Add(mods[i]);PersistSetting();}
            }
        }
        finally{GUI.EndScrollView();}
    }
}
