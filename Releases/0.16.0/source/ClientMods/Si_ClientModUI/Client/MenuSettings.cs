using Il2Cpp;
using UnityEngine;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private bool settingsMenuOpen;
    private float UiScale=>Math.Clamp(layout.TextSize,75,150)/100f;
    private int UiWidth=>(int)(Screen.width/UiScale);
    private int UiHeight=>(int)(Screen.height/UiScale);
    private void RenderSettings(Rect view)
    {
        float x=view.x,y=view.y,w=view.width;
        GUI.Label(new Rect(x,y,w,34),"SETTINGS",titleStyle);
        GUI.Label(new Rect(x,y+46,w,30),"Transparency: "+Math.Round(layout.Transparency)+"%",bodyStyle);
        float transparency=GUI.HorizontalSlider(new Rect(x,y+84,w,28),layout.Transparency,0,100);
        GUI.Label(new Rect(x,y+128,w,30),"Text size: "+Math.Round(layout.TextSize)+"%",bodyStyle);
        float size=GUI.HorizontalSlider(new Rect(x,y+166,w,28),layout.TextSize,75,150);
        if(Math.Abs(transparency-layout.Transparency)>.05f||Math.Abs(size-layout.TextSize)>.05f)
        {
            layout.Transparency=transparency;layout.TextSize=size;placementDirty=true;
            InvalidateUi(true);
            // Persist when the slider is released, instead of writing on every drag event.
        }
        if(placementDirty&&UnityEngine.InputSystem.Mouse.current?.leftButton.isPressed!=true)SaveWindowPosition();
        GUI.Label(new Rect(x,y+216,w,74),"Text size scales controls and spacing together. Team colors come from the local player; no server companion is needed.",smallStyle);
        if(Button(new Rect(x,y+304,180,36),"IMPORT SHARED JSON",description:"Read created-commands.json from the local Mod menu folder. Imported commands are references, never executed automatically."))
        {LoadPortableCommands();if(!portableReadFailed)controllerMessage="Read the local shared command file.";}
        GUI.Label(new Rect(x,y+354,w,100),"Share UserData/Si_ModMenu/created-commands.json. Only explicitly saved Help entries and custom sidebar commands are exported. An empty shell creates no command file.",smallStyle);
        if(Button(new Rect(x,view.yMax-42,140,34),"BACK",style:tabStyle))settingsMenuOpen=false;
    }
}

