using System.Globalization;
using Il2Cpp;
using Il2CppDebugTools;
using SiSquadHud;
using UnityEngine;

namespace SilicaCheats;
public sealed partial class CheatsMod
{
    private readonly List<(string template,string resolved)> usedEntries=new();
    private readonly Dictionary<string,bool> reportedToggles=new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string,float> reportedNumbers=new(StringComparer.OrdinalIgnoreCase);
    private Vector2 commandsScroll;
    private Vector2 consoleEntryScroll;
    private string commandsSearch="";
    private readonly System.Diagnostics.Stopwatch nativeUiClock=System.Diagnostics.Stopwatch.StartNew();
    private double nextNativeCatalog;
    private int nativeCatalogHash;
    private int nativeCatalogCount=-1;
    private string nativeRowsKey="";
    private string[] nativeRows=Array.Empty<string>();
    private float[] nativeHeights=Array.Empty<float>();
    private readonly List<(string key,string description)> nativeCatalog=new();
    private static string ToggleKey(string key,string selectedTeam)=>key+":"+selectedTeam;
    private string SpawnCommand()=>dynamicView?CommandRules.DynamicSpawn(prefab,team,precise):CommandRules.Spawn(prefab,team,pose,precise);
    private void CreateSidebar()
    {
        if(!Valid(preview))throw new ArgumentException("Choose or enter a valid console command first.");
        shortcutLine=preview;shortcutLabel=preview;editingId="";
        // Reuse an existing action for the same template so an edited icon retains its binding.
        var shortcut=shortcuts.FirstOrDefault(s=>s.Command==preview);
        if(shortcut==null)shortcut=SaveShortcut();
        ClientModControls.EditSidebar(this,shortcut.Id);
    }
    private void DrawCommandBar(Rect view)
    {
        var outer=view;
        consoleEntryScroll=GUI.BeginScrollView(outer,consoleEntryScroll,new Rect(0,0,Math.Max(80,outer.width-24),Math.Max(142,182+feedback.Count*60)),false,true);
        try
        {
        view=new Rect(0,0,Math.Max(80,outer.width-24),142);
        ui.Label(new Rect(view.x,view.y,view.width-164,26),"USED CONSOLE ENTRY (editable)");
        if(ui.Button(new Rect(view.xMax-156,view.y,156,26),"History",true))Navigate("history");
        preview=GUI.TextField(new Rect(view.x,view.y+32,view.width,34),preview,1024,ui.InputStyle);
        float half=(view.width-8)/2;
        if(ui.Button(new Rect(view.x,view.y+74,half,34),"Run console entry",Valid(preview)&&(Catalog.Find(preview)?.Key=="Cheats"?Admin:Enabled)))
            Try(()=>Run(preview,Catalog.Find(preview)?.Key=="Cheats"));
        if(ui.Button(new Rect(view.x+half+8,view.y+74,half,34),"Create sidebar button",Valid(preview)))Try(CreateSidebar);
        ui.Label(new Rect(view.x,view.y+112,view.width,28),CommandRules.Dynamic(preview)?"Dynamic current view: fresh Camera snapshot + 50m forward on every click.":"Commands run immediately. Select a history entry to reuse it or create a sidebar button.");
        if(feedback.Count>0)
        {
            ui.Label(new Rect(0,148,view.width,26),"NATIVE CONSOLE FEEDBACK");int index=0;
            foreach(string line in feedback)ui.Label(new Rect(0,178+index++*60,view.width,58),line);
        }
        }
        finally{GUI.EndScrollView();}
    }
    private void History()
    {
        Label("USED CONSOLE ENTRIES",28);
        if(usedEntries.Count==0)Label("No commands used in this session yet.");
        foreach(var entry in usedEntries.ToArray())
        {
            if(Button(entry.template)){preview=entry.template;shortcutLine=preview;shortcutLabel=preview;}
            if(entry.resolved!=entry.template)Label("Last execution: "+entry.resolved,56);
        }
    }
    private bool? ToggleState(string key,string selectedTeam="")
    {
        switch(key)
        {
            case "Fog":return !FogOfWar.FogOfWarDisabled;
            case "Units":return Unit.UnitDebugging;
            case "DamageDebug":return DamageManager.ShowDebug;
            case "Lights":return !BaseGameObject.NoLights;
            case "NetList":return NetworkComponent.ShowDebugList;
            case "AllowVisionModes":
                if(Game.GetIsSingleplayer())return WorldLight.AllowVisionModes;
                break;
            case "UnlinkedStructures":
                if(Game.GetIsSingleplayer())return Team.GetTeamByName(selectedTeam)?.UnlinkedStructureStateEnabled;
                break;
        }
        if(Game.GetIsSingleplayer())
        {
            if(key=="AI")return !Il2CppSilica.AI.AIBaseAgent.AIDisabled;
            if(key=="Damage")return !DamageManager.DamageDisabled;
            if(key=="Build")return ConstructionSite.LimitationsDisabled;
        }
        return reportedToggles.TryGetValue(ToggleKey(key,selectedTeam),out bool state)?state:null;
    }
    private void ToggleButton(string key,string selectedTeam="")
    {
        bool? state=ToggleState(key,selectedTeam);
        bool toggleOnly=new[]{"DamageDebug","Lights","NetList","AICommander"}.Contains(key);
        if(!state.HasValue)Label(toggleOnly?"Server status has not been reported yet. This command toggles the current state.":"Server status has not been reported yet. Enable sends an explicit enable command.",48);
        if(!Button(!state.HasValue&&toggleOnly?"Toggle":state==true?"Disable":"Enable",Enabled))return;
        string command=key+(selectedTeam.Length>0?" "+selectedTeam:"");
        // These native handlers only support a toggle, rather than boolean arguments.
        if(!toggleOnly)command+=" "+(state==true?"false":"true");
        Try(()=>Run(command));
    }
    private float? NumberValue(string key)
    {
        // Multiplayer WorldLight statics do not establish the server's current value.
        if(!Game.GetIsSingleplayer())return reportedNumbers.TryGetValue(key,out float reported)?reported:null;
        float? live=key switch
        {
            "DayScale"=>WorldLight.DayScale,
            "SunScale"=>WorldLight.SunScale,
            "MoonScale"=>WorldLight.MoonScale,
            "TimeScale" when Game.GetIsSingleplayer()=>Il2CppSilica.UI.TimeManager.Instance?.TargetSpeed,
            "NetTickMult" when Game.GetIsSingleplayer()=>GameManager.Instance?.Net_TickMult,
            _=>null
        };
        return live??(reportedNumbers.TryGetValue(key,out float value)?value:null);
    }
    private void ObserveValues(string text)
    {
        CheatStatusRules.Observe(text,reportedToggles,reportedNumbers);
    }
    private void DrawCommands(ClientModControls.PanelContext panel)
    {
        var view=panel.View;
        panel.Label(new Rect(view.x,view.y,view.width,28),"NATIVE CONSOLE COMMANDS - same registry as Help");
        commandsSearch=GUI.TextField(new Rect(view.x,view.y+34,view.width,34),commandsSearch,160,panel.InputStyle);
        var registry=DebugConsole.s_Commands;
        var area=new Rect(view.x,view.y+78,view.width,view.height-78);float w=area.width-28,top=0;
        if(nativeUiClock.Elapsed.TotalSeconds>=nextNativeCatalog||nativeCatalogCount!=(registry?.Count??0))
        {
            nativeCatalogCount=registry?.Count??0;nextNativeCatalog=nativeUiClock.Elapsed.TotalSeconds+1;var hash=new HashCode();
            if(registry!=null)foreach(var entry in registry)if(entry.Value!=null){hash.Add(entry.Key);hash.Add(entry.Value.Description);}
            int signature=hash.ToHashCode();
            if(signature!=nativeCatalogHash)
            {
                nativeCatalogHash=signature;nativeCatalog.Clear();
                if(registry!=null)foreach(var entry in registry)if(entry.Value!=null)nativeCatalog.Add((entry.Key,entry.Value.Description??""));
                nativeRowsKey="";
            }
        }
        string rowsKey=commandsSearch+":"+w+":"+panel.LabelStyle.fontSize;
        if(rowsKey!=nativeRowsKey)
        {
            nativeRowsKey=rowsKey;nativeRows=nativeCatalog.Where(e=>e.key.Contains(commandsSearch,StringComparison.OrdinalIgnoreCase)||e.description.Contains(commandsSearch,StringComparison.OrdinalIgnoreCase))
                .OrderBy(e=>e.key,StringComparer.OrdinalIgnoreCase).Select(e=>e.key+" : "+e.description).ToArray();
            nativeHeights=nativeRows.Select(row=>panel.LabelStyle.CalcHeight(new GUIContent(row),w)).ToArray();
        }
        var rows=nativeRows;float height=nativeHeights.Sum()+rows.Length*16;
        commandsScroll=GUI.BeginScrollView(area,commandsScroll,new Rect(0,0,w,Math.Max(area.height,height)),false,true);
        try
        {
            for(int index=0;index<rows.Length;index++)
            {
                float h=nativeHeights[index];
                if(top+h>=commandsScroll.y&&top<=commandsScroll.y+area.height)panel.Label(new Rect(0,top,w,h),rows[index]);top+=h+16;
            }
            if(rows.Length==0)panel.Label(new Rect(0,0,w,60),"No console commands match, or the native console registry is not initialized yet.");
        }
        finally{GUI.EndScrollView();}
    }
}
