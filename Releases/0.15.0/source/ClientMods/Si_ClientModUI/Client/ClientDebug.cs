using Il2Cpp;
using UnityEngine;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private readonly Queue<string> debugEvents=new();
    private long debugUpdates,debugDraws,debugPolls,debugErrors,debugChatSends;
    private Vector2 debugScroll;
    private string debugFilter="";
    private bool debugProviders;
    private void AddDebugEvent(string text)
    {
        if(!layout.ClientDebug)return;
        text=Clean(text);if(text.Length>1600)text=text[..1600]+"... (full details in session log)";
        debugEvents.Enqueue(DateTime.UtcNow.ToString("HH:mm:ss.fff")+" UTC  "+text);
        while(debugEvents.Count>200)debugEvents.Dequeue();
    }
    private void RenderClientDebug(Rect view)
    {
        if(Button(new Rect(view.x,view.y,view.width-184,34),(layout.ClientDebug?"[x] ":"[ ] ")+"Enable client UI debug",layout.ClientDebug))
        {
            layout.ClientDebug=!layout.ClientDebug;placementDirty=true;SaveWindowPosition();
            AddDebugEvent("Client UI debug enabled. Capturing own events and errors; history is bounded to 200 entries.");
        }
        if(Button(new Rect(view.xMax-176,view.y,176,34),debugProviders?"UI ANALYTICS":"MOD DEBUG CONTROLS",style:tabStyle))debugProviders=!debugProviders;
        var area=new Rect(view.x,view.y+44,view.width,view.height-44);
        if(debugProviders){RenderLocalControls(localMods.ToArray(),area,null,true);return;}
        if(!layout.ClientDebug){GUI.Label(area,"Enable debug to collect and view client UI events, errors and analytics. Detailed error logs remain available in the session log.",bodyStyle);return;}
        string stats="Uptime: "+clock.Elapsed.ToString(@"hh\:mm\:ss")+" | Native FPS: "+Game.FPS+" | Scene: "+sceneName+
            "\nUpdates: "+debugUpdates+" | GUI passes: "+debugDraws+" | Polls: "+debugPolls+" | Errors: "+debugErrors+" | Chat sends: "+debugChatSends+
            "\nTeam: "+CurrentSidebarTeam+" | Sidebar buttons: "+CurrentSidebarBindings.Count()+" | Client mods: "+localMods.Count+
            " | Commands: "+AllCommands().Count()+" | Captured input actions: "+guardedChatActions.Count;
        GUI.Label(new Rect(area.x,area.y,area.width,98),stats,smallStyle);
        debugFilter=GUI.TextField(new Rect(area.x,area.y+104,Math.Max(100,area.width-210),30),debugFilter,160,inputStyle);
        if(Button(new Rect(area.xMax-202,area.y+104,94,30),"CLEAR",style:tabStyle))debugEvents.Clear();
        if(Button(new Rect(area.xMax-100,area.y+104,100,30),"COPY",style:tabStyle))GUIUtility.systemCopyBuffer=stats+"\n\n"+string.Join("\n",debugEvents);
        var region=new Rect(area.x,area.y+144,area.width,Math.Max(40,area.height-144));float width=region.width-26;
        var rows=debugEvents.Reverse().Where(s=>debugFilter.Length==0||s.Contains(debugFilter,StringComparison.OrdinalIgnoreCase)).ToArray();
        var heights=rows.Select(row=>Math.Max(28,smallStyle.CalcHeight(new GUIContent(row),width))+10).ToArray();
        debugScroll=GUI.BeginScrollView(region,debugScroll,new Rect(0,0,width,Math.Max(region.height,heights.Sum())),false,true);
        try
        {
            float top=0;
            for(int i=0;i<rows.Length;i++)
            {
                if(top+heights[i]>=debugScroll.y&&top<=debugScroll.y+region.height)GUI.Label(new Rect(0,top,width,heights[i]-6),rows[i],smallStyle);
                top+=heights[i];
            }
            if(rows.Length==0)GUI.Label(new Rect(0,0,width,60),"No matching events yet. New menu events and errors appear here while debug is enabled.",smallStyle);
        }
        finally{GUI.EndScrollView();}
    }
}
