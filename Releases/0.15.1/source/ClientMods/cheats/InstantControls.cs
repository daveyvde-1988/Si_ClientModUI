using System.Globalization;
using System.Text.RegularExpressions;
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
    private string commandsSearch="";
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
        ui.Label(new Rect(view.x,view.y,view.width-164,26),"USED CONSOLE ENTRY (editable)");
        if(ui.Button(new Rect(view.xMax-156,view.y,156,26),"History",true))Navigate("history");
        preview=GUI.TextField(new Rect(view.x,view.y+32,view.width,34),preview,1024,ui.InputStyle);
        float half=(view.width-8)/2;
        if(ui.Button(new Rect(view.x,view.y+74,half,34),"Run console entry",Valid(preview)&&(Enabled||Catalog.Find(preview)?.Key=="Cheats")))
            Try(()=>Run(preview,Catalog.Find(preview)?.Key=="Cheats"));
        if(ui.Button(new Rect(view.x+half+8,view.y+74,half,34),"Create sidebar button",Valid(preview)))Try(CreateSidebar);
        ui.Label(new Rect(view.x,view.y+112,view.width,28),CommandRules.Dynamic(preview)?"Dynamic current view: fresh Camera snapshot + 100m forward on every click.":"Commands run immediately. Select a history entry to reuse it or create a sidebar button.");
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
            case "AllowVisionModes":return WorldLight.AllowVisionModes;
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
        if(reportedNumbers.TryGetValue(key,out float value))return value;
        return key switch
        {
            "DayScale"=>WorldLight.DayScale,
            "SunScale"=>WorldLight.SunScale,
            "MoonScale"=>WorldLight.MoonScale,
            "TimeScale" when Game.GetIsSingleplayer()=>Il2CppSilica.UI.TimeManager.Instance?.TargetSpeed,
            "NetTickMult" when Game.GetIsSingleplayer()=>GameManager.Instance?.Net_TickMult,
            _=>null
        };
    }
    private void ObserveValues(string text)
    {
        int colon=text.IndexOf(':');
        if(colon>0)
        {
            string key=text[..colon],body=text[(colon+1)..].Trim();
            if(!body.Contains("cheats",StringComparison.OrdinalIgnoreCase)&&!body.Contains("error",StringComparison.OrdinalIgnoreCase))
            {
                var match=Regex.Match(body,@"(?:\bto\s+|\s)(Enabled|Disabled|True|False)$",RegexOptions.IgnoreCase);
                if(match.Success&&key!="UnlinkedStructures")
                    reportedToggles[ToggleKey(key,"")]=match.Groups[1].Value.Equals("Enabled",StringComparison.OrdinalIgnoreCase)||match.Groups[1].Value.Equals("True",StringComparison.OrdinalIgnoreCase);
                var number=Regex.Match(body,@"(?:set|reset) to\s+([-+0-9.eE]+)$",RegexOptions.IgnoreCase);
                if(number.Success&&float.TryParse(number.Groups[1].Value,System.Globalization.NumberStyles.Float,CultureInfo.InvariantCulture,out float n)&&float.IsFinite(n))reportedNumbers[key]=n;
            }
        }
        var teamState=Regex.Match(text,@"(?:Commander for team|unlinked structure state for Team) '([^']+)' (?:is now|to) (enabled|disabled)",RegexOptions.IgnoreCase);
        if(teamState.Success)
        {
            string key=text.Contains("unlinked",StringComparison.OrdinalIgnoreCase)?"UnlinkedStructures":"AICommander";
            reportedToggles[ToggleKey(key,teamState.Groups[1].Value)]=teamState.Groups[2].Value.Equals("enabled",StringComparison.OrdinalIgnoreCase);
        }
    }
    private void DrawCommands(ClientModControls.PanelContext panel)
    {
        var view=panel.View;
        panel.Label(new Rect(view.x,view.y,view.width,28),"NATIVE CONSOLE COMMANDS - same registry as Help");
        commandsSearch=GUI.TextField(new Rect(view.x,view.y+34,view.width,34),commandsSearch,160,panel.InputStyle);
        var entries=new List<(string key,string description)>();
        var registry=DebugConsole.s_Commands;
        if(registry!=null)foreach(var entry in registry)
            if(entry.Value!=null&&(entry.Key.Contains(commandsSearch,StringComparison.OrdinalIgnoreCase)||entry.Value.Description.Contains(commandsSearch,StringComparison.OrdinalIgnoreCase)))
                entries.Add((entry.Key,entry.Value.Description));
        var area=new Rect(view.x,view.y+78,view.width,view.height-78);float w=area.width-28,top=0;
        var rows=entries.OrderBy(e=>e.key,StringComparer.OrdinalIgnoreCase).Select(e=>e.key+" : "+e.description).ToArray();
        float height=rows.Sum(row=>panel.LabelStyle.CalcHeight(new GUIContent(row),w)+16);
        commandsScroll=GUI.BeginScrollView(area,commandsScroll,new Rect(0,0,w,Math.Max(area.height,height)),false,true);
        try
        {
            foreach(string row in rows)
            {
                float h=panel.LabelStyle.CalcHeight(new GUIContent(row),w);
                panel.Label(new Rect(0,top,w,h),row);top+=h+16;
            }
            if(rows.Length==0)panel.Label(new Rect(0,0,w,60),"No console commands match, or the native console registry is not initialized yet.");
        }
        finally{GUI.EndScrollView();}
    }
}
