using UnityEngine;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private bool dividerDragging;
    private int dividerControl;
    private string dividerKey="";
    private float FeedbackDivider(float x,float top,float bottom,float width,string layoutId="menu",float minimumAbove=90,float minimumBelow=80)
    {
        string key=RoleWindowKey(layoutId);
        if(dividerDragging&&dividerKey!=key)EndFeedbackDrag();
        float span=Math.Max(90,bottom-top);
        float minFraction=Math.Min(.65f,minimumBelow/span),maxFraction=Math.Max(minFraction,Math.Min(.65f,1-minimumAbove/span));
        float fraction=Math.Clamp(layout.FeedbackSplit.TryGetValue(key,out float saved)?UiPolicies.Split(saved):.30f,minFraction,maxFraction);
        float position=top+span*(1-fraction);
        var grip=new Rect(x,position-6,width,12);int id=GUIUtility.GetControlID((key+"-split").GetHashCode(),FocusType.Passive);var e=Event.current;
        Surface(grip,Color.Lerp(surface,accent,grip.Contains(e.mousePosition)?.24f:.10f));
        GUI.Label(new Rect(x+width/2-22,position-10,44,20),"=",tabStyle);
        if(GUI.enabled&&Application.isFocused&&e.type==EventType.MouseDown&&e.button==0&&grip.Contains(e.mousePosition))
        {dividerDragging=true;dividerKey=key;dividerControl=id;GUIUtility.hotControl=id;e.Use();}
        if(dividerDragging)
        {
            if(!Application.isFocused||!panelOpen){EndFeedbackDrag();return position;}
            if(e.type==EventType.MouseDrag&&GUIUtility.hotControl==dividerControl)
            {
                fraction=Math.Clamp(UiPolicies.Split((bottom-e.mousePosition.y)/span),minFraction,maxFraction);layout.FeedbackSplit[key]=fraction;placementDirty=true;
                position=top+span*(1-fraction);e.Use();
            }
            if(e.type==EventType.MouseUp&&e.button==0){EndFeedbackDrag();e.Use();}
        }
        return position;
    }
    private void EndFeedbackDrag()
    {
        if(!dividerDragging)return;
        if(GUIUtility.hotControl==dividerControl)GUIUtility.hotControl=0;
        dividerDragging=false;dividerKey="";SaveWindowPosition();
    }
}
