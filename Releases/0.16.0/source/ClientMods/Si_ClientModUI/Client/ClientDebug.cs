using Il2Cpp;
using UnityEngine;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private readonly Queue<string> debugEvents=new();
    private long debugUpdates,debugDraws,debugPolls,debugErrors,debugChatSends,debugRevision;
    private Vector2 debugScroll;
    private string debugFilter="",debugStats="",debugRowsKey="";
    private string[] debugRows=Array.Empty<string>();
    private float[] debugHeights=Array.Empty<float>();
    private double debugStatsAt;
    private bool debugProviders;
    private void AddDebugEvent(string text)
    {
        if(!layout.ClientDebug)return;
        text=Clean(text);if(text.Length>1600)text=text[..1600]+"... (full details in session log)";
        debugEvents.Enqueue(DateTime.UtcNow.ToString("HH:mm:ss.fff")+" UTC  "+text);debugRevision++;
        while(debugEvents.Count>200)debugEvents.Dequeue();
    }
    private string DebugStats()
    {
        if(clock.Elapsed.TotalSeconds<debugStatsAt)return debugStats;
        debugStatsAt=clock.Elapsed.TotalSeconds+UiPolicies.UiRefreshSeconds;
        debugStats="Uptime: "+clock.Elapsed.ToString(@"hh\:mm\:ss")+" | Native FPS: "+Game.FPS+" | Scene: "+sceneName+
            "\nUpdates: "+debugUpdates+" | GUI passes: "+debugDraws+" | Housekeeping: "+debugPolls+" | Errors: "+debugErrors+" | Chat sends: "+debugChatSends+
            "\nTeam: "+CurrentSidebarTeam+" | Role: "+CurrentRole+" | Sidebar buttons: "+CurrentSidebarBindings.Count()+" | Client mods: "+localMods.Count+
            " | Commands: "+AllCommands().Count()+" | Captured actions: "+guardedChatActions.Count+
            "\nCache work: command builds "+uiCommandBuilds+" | Text measurements "+uiTextMeasures+" | Provider reads "+uiProviderReads+" | Status refreshes "+uiStatusRefreshes+
            "\nObserved native TX: "+traffic.TxPackets+" packets / "+traffic.TxBytes+" B ("+Math.Round(traffic.TxBytesPerSecond)+" B/s); RX: "+traffic.RxPackets+" / "+traffic.RxBytes+" B ("+Math.Round(traffic.RxBytesPerSecond)+" B/s)"+
            "\nCompanion TX/RX: "+traffic.CompanionTxPackets+" / "+traffic.CompanionRxPackets+" packets; "+traffic.CompanionTxBytes+" / "+traffic.CompanionRxBytes+" payload B"+
            "\nCatalogs: "+traffic.Frames+" | Replies: "+traffic.Replies+" | Events: "+traffic.Events+" | Last reply: "+Math.Round(traffic.LastReplyMs)+" ms"+
            "\nSend failures: "+traffic.Failures+" | Rejected/stale companion packets: "+traffic.Rejected+" | Reply timeouts: "+traffic.Timeouts+" | Transfers assembling: "+serverTransfers.Count+
            "\nSession totals; native application bytes exclude Steam/UDP overhead. Companion bytes count message bodies. No analytics requests are sent.";
        return debugStats;
    }
    private void RenderClientDebug(Rect view)
    {
        if(Button(new Rect(view.x,view.y,view.width-184,34),(layout.ClientDebug?"[x] ":"[ ] ")+"Enable client UI debug",layout.ClientDebug))
        {
            layout.ClientDebug=!layout.ClientDebug;placementDirty=true;SaveWindowPosition();debugStatsAt=0;
            AddDebugEvent("Client UI debug enabled. Event history is bounded to 200 entries.");
        }
        if(Button(new Rect(view.xMax-176,view.y,176,34),debugProviders?"UI ANALYTICS":"MOD DEBUG CONTROLS",style:tabStyle))debugProviders=!debugProviders;
        var area=new Rect(view.x,view.y+44,view.width,Math.Max(30,view.height-44));
        if(debugProviders){RenderLocalControls(localMods.ToArray(),area,null,true);return;}
        if(!layout.ClientDebug){GUI.Label(area,"Enable debug to collect client UI events and view traffic analytics. Detailed errors remain available in the session log.",bodyStyle);return;}
        string stats=DebugStats();float width=area.width-26,statsHeight=MeasureText(smallStyle,stats,width)+8;
        string key=debugRevision+":"+debugFilter+":"+width+":"+smallStyle.fontSize;
        if(key!=debugRowsKey)
        {
            debugRowsKey=key;debugRows=debugEvents.Reverse().Where(s=>debugFilter.Length==0||s.Contains(debugFilter,StringComparison.OrdinalIgnoreCase)).ToArray();
            debugHeights=debugRows.Select(row=>Math.Max(28,smallStyle.CalcHeight(new GUIContent(row),width))+10).ToArray();
        }
        debugScroll=GUI.BeginScrollView(area,debugScroll,new Rect(0,0,width,Math.Max(area.height,statsHeight+94+debugHeights.Sum())),false,true);
        try
        {
            GUI.Label(new Rect(0,0,width,statsHeight),stats,smallStyle);
            debugFilter=GUI.TextField(new Rect(0,statsHeight+8,Math.Max(100,width-210),30),debugFilter,160,inputStyle);
            if(Button(new Rect(width-202,statsHeight+8,94,30),"CLEAR",style:tabStyle)){debugEvents.Clear();debugRevision++;}
            if(Button(new Rect(width-100,statsHeight+8,100,30),"COPY",style:tabStyle))GUIUtility.systemCopyBuffer=stats+"\n\n"+string.Join("\n",debugEvents);
            float top=statsHeight+52;
            for(int i=0;i<debugRows.Length;i++)
            {
                if(top+debugHeights[i]>=debugScroll.y&&top<=debugScroll.y+area.height)GUI.Label(new Rect(0,top,width,debugHeights[i]-6),debugRows[i],smallStyle);
                top+=debugHeights[i];
            }
            if(debugRows.Length==0)GUI.Label(new Rect(0,top,width,40),"No matching events yet.",smallStyle);
        }
        finally{GUI.EndScrollView();}
    }
}
