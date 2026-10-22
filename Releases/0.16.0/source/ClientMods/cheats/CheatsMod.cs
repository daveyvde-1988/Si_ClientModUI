using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using HarmonyLib;
using Il2Cpp;
using Il2CppDebugTools;
using MelonLoader;
using SiSquadHud;
using UnityEngine;

[assembly: MelonInfo(typeof(SilicaCheats.CheatsMod), "cheats", "0.3.2", "Davey")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonPlatform(MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
[assembly: MelonAdditionalDependencies("Si_ClientModUI")]

namespace SilicaCheats;
public sealed partial class CheatsMod : MelonMod
{
    public sealed class Shortcut
    {
        public string Id {get;set;}=Guid.NewGuid().ToString("N");
        public string Label {get;set;}="";
        public string Command {get;set;}="";
    }
    private sealed class Spawnable
    {
        public readonly string Token,Name,Category;
        public Spawnable(string token,string name,string category){Token=token;Name=name;Category=category;}
    }
    private static CheatsMod instance;
    private readonly List<Shortcut> shortcuts=new();
    private readonly List<Spawnable> spawnables=new();
    private readonly Queue<string> feedback=new();
    private readonly Dictionary<string,string> observed=new(StringComparer.OrdinalIgnoreCase);
    private string page="",category="",spawnCategory="",search="",team="",prefab="",pose="",amount="5000",args="",preview="";
    private string shortcutLabel="",shortcutLine="",editingId="",message="",context="";
    private bool precise,dynamicView=true;
    private Vector2 scroll;
    private DateTime feedbackUntil=DateTime.MinValue;
    private ClientModControls.PanelContext ui;
    private float y,width,contentHeight=900;
    private string ProfilePath=>Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory,"cheats","shortcuts-"+(Player.CurrentPlayer?.PlayerID.SteamID.m_SteamID.ToString()??"offline")+".json");
    private static bool Session=>!Game.InMainMenu&&Player.CurrentPlayer!=null&&(Game.GetIsSingleplayer()||Game.GetIsClient()&&NetworkGameClient.GetInServer());
    private static bool Admin=>Session&&(Game.GetIsSingleplayer()||Player.CurrentPlayer.AdminLevel>=EAdminLevel.STANDARD);
    private static bool Enabled=>Session&&Game.CheatsEnabled;
    public override void OnInitializeMelon(){instance=this;RegisterControls();LoggerInstance.Msg("cheats ready. Open Mod Menu -> cheats -> Controls.");}
    public override void OnDeinitializeMelon(){ClientModControls.Unregister(this);instance=null;}
    public override void OnSceneWasLoaded(int index,string name){Reset();context="";}
    private void Reset(){page=category=team=prefab=pose=preview=search="";spawnables.Clear();observed.Clear();feedback.Clear();usedEntries.Clear();reportedToggles.Clear();reportedNumbers.Clear();scroll=Vector2.zero;feedbackUntil=DateTime.MinValue;}
    public override void OnUpdate()
    {
        string next=Session?Player.CurrentPlayer.PlayerID.SteamID.m_SteamID+":"+(Game.GetIsSingleplayer()?"singleplayer":NetworkGameClient.GetServerID().SteamID.m_SteamID.ToString()):"";
        if(context==next)return;
        context=next;Reset();shortcuts.Clear();
        if(Session)Load();RegisterControls();
    }
    private void Load()
    {
        try
        {
            if(!File.Exists(ProfilePath))return;
            var saved=JsonSerializer.Deserialize<List<Shortcut>>(File.ReadAllText(ProfilePath));
            foreach(var s in saved??new())if(s!=null&&Guid.TryParseExact(s.Id,"N",out _)&&CommandRules.SingleLine(s.Label)&&Valid(s.Command)&&!shortcuts.Any(v=>v.Id==s.Id))shortcuts.Add(s);
        }
        catch(Exception e){Notice("Could not load shortcuts: "+e.Message);}
    }
    private void Save()
    {
        string path=ProfilePath;Directory.CreateDirectory(Path.GetDirectoryName(path));
        string json=JsonSerializer.Serialize(shortcuts,new JsonSerializerOptions{WriteIndented=true});
        File.WriteAllText(path+".tmp",json);
        // Parse before replacing; retain the previous definitions as a backup.
        JsonSerializer.Deserialize<List<Shortcut>>(File.ReadAllText(path+".tmp"));
        if(File.Exists(path))File.Replace(path+".tmp",path,path+".bak",true);else File.Move(path+".tmp",path);
    }
    private static bool Valid(string line)=>CommandRules.ValidTemplate(line)&&Catalog.Find(line)!=null;
    private void RegisterControls()=>ClientModControls.Register(this,new ClientModControls.Definition
    {
        Title="cheats",DrawPanel=Draw,DrawCommands=DrawCommands,
        Statuses=new[]{new ClientModControls.Status{Id="access",Label="Cheats",Read=()=>!Admin?"access denied":Enabled?"enabled":"disabled"}},
        Sidebar=shortcuts.Select(s=>new ClientModControls.SidebarAction{Id=s.Id,Label=s.Label,Description=s.Command,AdminOnly=true,
            Symbol="text:$",Color="#64DD86",Available=()=>Admin&&Enabled&&Valid(s.Command),Invoke=()=>Run(s.Command)}).ToArray()
    });
    private void Notice(string text){message=text;ClientModControls.Notify(this,text);}
    private bool Access(bool requireCheats=true)
    {
        string error=CommandRules.AccessError(Admin,Enabled,requireCheats);
        if(error.Length>0){Notice(error);return false;}
        return true;
    }
    private void Run(string line,bool bypassCheats=false)
    {
        if(!Access(!bypassCheats)||!Valid(line))return;
        feedbackUntil=DateTime.UtcNow.AddSeconds(8);
        string template=line;
        line=CommandRules.ResolveDynamic(line,()=>
        {
            if(Game.CurrentCamera==null)throw new InvalidOperationException("No camera available. Dynamic spawn was cancelled.");
            DebugConsole.TryExecuteCommand("camera",false,null);
            return GUIUtility.systemCopyBuffer;
        });
        // Exact native console dispatch preserves per-command server authorization.
        // Never set game flags, invoke handler Execute directly, or impersonate a remote caller.
        try{DebugConsole.TryExecuteCommand(CommandRules.ForNativeConsole(line),false,null);}
        catch(Exception error)
        {
            LoggerInstance.Error("Console command failed: "+line+"\n"+error);
            Notice("Command failed: "+(Catalog.Find(line)?.Key=="Spawn"?"the game could not spawn at the supplied coordinates. ":"")+"See the client log for details.");
            return;
        }
        preview=template;shortcutLine=template;shortcutLabel=template;
        usedEntries.Insert(0,(template,line));if(usedEntries.Count>30)usedEntries.RemoveAt(usedEntries.Count-1);
        message="Submitted: "+line+". See native feedback below.";
    }
    [HarmonyPatch(typeof(DebugConsole),nameof(DebugConsole.DisplayLog))]
    private static class ConsoleFeedback
    {
        private static void Postfix(string __0){instance?.Observe(__0);}
    }
    [HarmonyPatch(typeof(DebugConsole),nameof(DebugConsole.Log))]
    private static class LocalConsoleFeedback
    {
        private static void Postfix(string __0){instance?.Observe(__0);}
    }
    private void Observe(string text)
    {
        if(DateTime.UtcNow>feedbackUntil||string.IsNullOrWhiteSpace(text))return;
        text=Regex.Replace(text,"<[^>]+>","");
        if(text.Length>800)text=text[..800];
        if(feedback.LastOrDefault()!=text){feedback.Enqueue(text);while(feedback.Count>6)feedback.Dequeue();}
        int colon=text.IndexOf(':');
        if(colon>0&&Catalog.Find(text[..colon])!=null)observed[text[..colon]]=text[(colon+1)..].Trim();
        ObserveValues(text);
    }
    private void Navigate(string value){page=value;scroll=Vector2.zero;search="";contentHeight=900;}
    private Rect Row(float height=34){var r=new Rect(0,y,width,height);y+=height+8;return r;}
    private void Label(string value,float height=36)=>ui.Label(Row(height),value);
    private bool Button(string label,bool allowed=true)=>ui.Button(Row(),label,allowed);
    private string Field(string label,string value,int max=1024){Label(label,26);return GUI.TextField(Row(),value,max,ui.InputStyle);}
    private void Pair(string a,Action left,string b,Action right,bool allowed=true)
    {
        var r=Row();float half=(r.width-8)/2;
        if(ui.Button(new Rect(r.x,r.y,half,r.height),a,allowed))Try(left);
        if(ui.Button(new Rect(r.x+half+8,r.y,half,r.height),b,allowed))Try(right);
    }
    private void Try(Action action){try{action();}catch(Exception e){Notice(e.GetBaseException().Message);}}
    private void Draw(ClientModControls.PanelContext panel)
    {
        ui=panel;var view=panel.View;
        bool enabled=Enabled;
        if(panel.Button(new Rect(view.x,view.y,view.width-120,34),(enabled?"[x] ":"[ ] ")+"Enable cheats (reported by game)",true))
            Run("cheats",true);
        if(panel.Button(new Rect(view.xMax-112,view.y,112,34),"HOME",true))Navigate("");
        if(!Admin)
        {
            panel.Label(new Rect(view.x,view.y+46,view.width,100),Session?"user is not an admin. access denied":"Join a game to use cheats.");return;
        }
        float split=panel.Divider?.Invoke("console",new Rect(view.x,view.y+100,view.width,view.height-100),80,150)??view.yMax-150;
        DrawCommandBar(new Rect(view.x,split+8,view.width,Math.Max(40,view.yMax-split-8)));
        var area=new Rect(view.x,view.y+44,view.width,Math.Max(50,split-view.y-54));
        width=area.width-26;y=0;
        // Room for the complete live prefab catalog, without imposing a fixed item limit.
        float height=Math.Max(area.height,contentHeight);
        scroll=GUI.BeginScrollView(area,scroll,new Rect(0,0,width,height),false,true);
        try
        {
            if(!enabled)Label("Enable cheats to execute commands. You can browse and edit shortcuts while disabled.");
            if(message.Length>0)Label(message,64);
            if(page.Length>0&&Button("< Back")){Back();return;}
            switch(page)
            {
                case "":Home();break;
                case "category":Category();break;
                case "resources-team":Teams("resources-value");break;
                case "resources-value":Resources();break;
                case "spawn-categories":SpawnCategories();break;
                case "spawn-list":SpawnList();break;
                case "spawn-team":Teams("spawn-position",true);break;
                case "spawn-position":SpawnPosition();break;
                case "shortcuts":Shortcuts();break;
                case "history":History();break;
                default:Generic();break;
            }
        }
        finally{contentHeight=y+20;GUI.EndScrollView();}
    }
    private void Back()
    {
        string target=page switch{"category"=>"","resources-team"=>"category","resources-value"=>"resources-team","spawn-categories"=>"category","spawn-list"=>"spawn-categories","spawn-team"=>"spawn-list","spawn-position"=>"spawn-team","shortcuts"=>"",_=>"category"};
        Navigate(target);
    }
    private void Home()
    {
        Label("CHEATS - native console controls",32);
        if(Button("Saved commands and sidebar shortcuts"))Navigate("shortcuts");
        if(Button("Used console entries"))Navigate("history");
        foreach(string group in Catalog.All.Select(c=>c.Category).Distinct())
            if(Button(group)){category=group;Navigate("category");}
    }
    private void Category()
    {
        Label(category,30);
        foreach(var command in Catalog.All.Where(c=>c.Category==category))
            if(Button(command.Key+" - "+command.Description))
            {
                args=NumberValue(command.Key)?.ToString("0.###",CultureInfo.InvariantCulture)??"";
                if(command.Key=="Resources")Navigate("resources-team");
                else if(command.Key=="Spawn"){RefreshSpawnables();Navigate("spawn-categories");}
                else Navigate(command.Key);
            }
    }
    private IEnumerable<Team> AvailableTeams()
    {
        var database=GameDatabase.Database;if(database==null)yield break;
        for(int i=0;i<database.AllTeams.Count;i++){var t=database.AllTeams[i];if(t!=null&&CommandRules.Token(t.TeamShortName))yield return t;}
    }
    private void Teams(string next,bool none=false)
    {
        Label("Select team",28);
        foreach(var t in AvailableTeams().OrderBy(t=>t.TeamShortName))
            if(Button(t.TeamShortName)){team=t.TeamShortName;Navigate(next);}
        if(none&&Button("none (no team)")){team="none";Navigate(next);}
    }
    private void Prepare(string line){preview=line;shortcutLine=line;shortcutLabel=line;}
    private void Resources()
    {
        var t=Team.GetTeamByName(team);
        Label("Resources / "+team,28);
        if(t!=null)Label("Game reports: stored "+t.StoredResources+"; starting "+t.StartingResources+"; total "+t.TotalResources,40);
        amount=Field("Amount (positive whole number)",amount,10);
        Pair("Subtract amount",()=>Run(CommandRules.Resources(team,amount,true)),"Add amount",()=>Run(CommandRules.Resources(team,amount,false)),Enabled);
        Pair("- 100",()=>Run(CommandRules.Resources(team,"100",true)),"+ 100",()=>Run(CommandRules.Resources(team,"100",false)),Enabled);
        Pair("- 1000",()=>Run(CommandRules.Resources(team,"1000",true)),"+ 1000",()=>Run(CommandRules.Resources(team,"1000",false)),Enabled);
    }
    private void RefreshSpawnables()
    {
        spawnables.Clear();var db=GameDatabase.Database;if(db==null)return;
        for(int i=0;i<db.AllNetworkedPrefabs.Count;i++)
        {
            var p=db.AllNetworkedPrefabs[i];if(p==null)continue;
            var unit=p.GetComponent<Unit>();var structure=p.GetComponent<Structure>();
            string kind=unit!=null?"Units":structure!=null?"Structures":"Other network prefabs";
            var obj=p.GetComponent<BaseGameObject>();
            string faction=obj?.DefaultTeam?.TeamShortName??"Unassigned";
            // The native lookup matches prefab names case-insensitively and returns the first match.
            // P:index is a log marker, not a valid Spawn argument in this build.
            string token=p.name;
            if(CommandRules.Token(token)&&!spawnables.Any(s=>s.Token.Equals(token,StringComparison.OrdinalIgnoreCase)))
                spawnables.Add(new Spawnable(token,p.name,faction+" / "+kind));
        }
        spawnables.Sort((a,b)=>StringComparer.OrdinalIgnoreCase.Compare(a.Name,b.Name));
    }
    private void SpawnCategories()
    {
        Label("Spawn / category",28);
        if(Button("Refresh live spawn catalog"))Try(RefreshSpawnables);
        if(spawnables.Count==0)Label("No spawnables resolved yet. Enter a loaded match and refresh.",50);
        foreach(string group in spawnables.Select(s=>s.Category).Distinct().OrderBy(s=>s))
            if(Button(group)){spawnCategory=group;Navigate("spawn-list");}
    }
    private void SpawnList()
    {
        Label("Spawn / "+spawnCategory,28);search=Field("Filter spawnables",search,80);
        foreach(var s in spawnables.Where(s=>s.Category==spawnCategory&&s.Name.Contains(search,StringComparison.OrdinalIgnoreCase)))
            if(Button(s.Name)){prefab=s.Token;Navigate("spawn-team");}
    }
    private static string CameraPose()
    {
        var camera=Game.CurrentCamera;if(camera==null)throw new InvalidOperationException("No game camera available. Use Camera and paste the coordinates.");
        var p=camera.transform.position;var q=camera.transform.rotation;
        return string.Join(",",new[]{p.x,p.y,p.z,q.w,q.x,q.y,q.z}.Select(f=>f.ToString("R",CultureInfo.InvariantCulture)));
    }
    private void SpawnPosition()
    {
        Label("Spawn / "+prefab+" / "+team,40);
        if(Button((dynamicView?"[x] ":"[ ] ")+"Dynamic current view - 100 metres forward each time"))dynamicView=!dynamicView;
        Label(dynamicView?"Each execution captures Camera again and spawns 100 metres along the current viewing direction, including pitch.":"Fixed position. Capture or paste coordinates below.",55);
        Label("Free placement is not verified for arbitrary units, buildings or other prefabs.",36);
        if(!dynamicView)
        {
        Pair("Use current camera coordinates",()=>pose=CameraPose(),"Run Camera (copies coordinates)",()=>Run("camera"));
        Pair("Paste coordinates",()=>pose=GUIUtility.systemCopyBuffer,"Copy coordinates",()=>GUIUtility.systemCopyBuffer=pose);
        pose=Field("Position / rotation: x,y,z,w,qx,qy,qz",pose,240);
        }
        if(Button((precise?"[x] ":"[ ] ")+"Preserve full rotation (otherwise yaw only)"))precise=!precise;
        var buttons=Row();float half=(buttons.width-8)/2;
        if(ui.Button(new Rect(0,buttons.y,half,34),"Spawn now",Enabled))Try(()=>Run(SpawnCommand()));
        if(ui.Button(new Rect(half+8,buttons.y,half,34),"Create sidebar button",true))Try(()=>{Prepare(SpawnCommand());CreateSidebar();});
    }
    private string State(string key)
    {
        if(observed.TryGetValue(key,out string value))return "Last native feedback: "+value;
        if(Game.GetIsSingleplayer())
        {
            if(key=="Damage")return DamageManager.DamageDisabled?"Damage disabled":"Damage enabled";
            if(key=="AI")return Il2CppSilica.AI.AIBaseAgent.AIDisabled?"AI disabled":"AI enabled";
            if(key=="Build")return ConstructionSite.LimitationsDisabled?"Unlimited construction enabled":"Normal construction limits";
        }
        // Only local effects have locally readable authoritative values. Server state is never inferred from client statics.
        return key switch
        {
            "Cheats"=>Enabled?"Enabled (game reports)":"Disabled (game reports)",
            "Fog"=>FogOfWar.FogOfWarDisabled?"Local fog disabled":"Local fog enabled",
            "Units"=>Unit.UnitDebugging?"Local unit debugging enabled":"Local unit debugging disabled",
            "Health"=>"Local display mode: "+DamageManager.ShowDebugHealth,
            "DamageDebug"=>"Local debug: "+DamageManager.ShowDebug,
            "Lights"=>"Local lights hidden: "+BaseGameObject.NoLights,
            "NetList"=>"Local debug list: "+NetworkComponent.ShowDebugList,
            _=>"Server state: use native feedback after execution."
        };
    }
    private void Generic()
    {
        var c=Catalog.Find(page);if(c==null)return;
        Label(c.Key+" / "+(c.Server?"server command":"local command"),30);
        Label(c.Description,52);Label("Syntax: "+c.Key+" "+c.Syntax,48);Label(State(c.Key),58);
        if(c.Destructive)Label("This command can remove objects or end/change the current match. Review the command before executing.",55);
        if(c.Key=="Cheats")
        {
            if(Button(Enabled?"Disable":"Enable"))Run("cheats",true);return;
        }
        if(new[]{"AI","Damage","Fog","Build","Units","AllowVisionModes","DamageDebug","Lights","NetList"}.Contains(c.Key))
        {ToggleButton(c.Key);return;}
        if(new[]{"Team","AICommander","UnlinkedStructures","Destroy","Resources"}.Contains(c.Key))
        {
            foreach(var t in AvailableTeams())if(Button(t.TeamShortName)){team=t.TeamShortName;args=team;}
            if(c.Key=="UnlinkedStructures"){if(team.Length>0)ToggleButton(c.Key,team);return;}
            if(c.Key=="AICommander"){if(team.Length>0)ToggleButton(c.Key,team);return;}
        }
        if(new[]{"TimeScale","DayScale","SunScale","MoonScale","NetTickMult"}.Contains(c.Key))
        {
            float step=c.Key=="NetTickMult"?.1f:1;
            Pair("- "+step.ToString("0.#",CultureInfo.InvariantCulture),()=>StepNumber(-step),"+ "+step.ToString("0.#",CultureInfo.InvariantCulture),()=>StepNumber(step),Enabled);
            if(c.Key=="TimeScale")Label("Vanilla normal game speed: 1. Native accepted range: 0..10.",36);
        }
        args=Field("Arguments (leave empty where optional)",args,800);
        if(Button(c.Destructive?"Execute destructive command":"Apply",Enabled))Try(()=>Run(c.Key+(args.Trim().Length>0?" "+args.Trim():"")));
    }
    private void StepNumber(float step)
    {
        if(!Access())return;
        float n=float.TryParse(args,NumberStyles.Float,CultureInfo.InvariantCulture,out var draft)&&float.IsFinite(draft)?draft:NumberValue(page)??1;
        args=Math.Clamp(n+step,page=="NetTickMult"?.1f:0,page=="TimeScale"?10:page=="NetTickMult"?30:1000).ToString("0.###",CultureInfo.InvariantCulture);
        Run(page+" "+args);
    }
    private void Shortcuts()
    {
        Label("CHEATS / saved console commands",30);
        Label("Save a command, then choose its sidebar symbol/color. Sidebar settings include the persistent sidebar checkbox.",52);
        shortcutLabel=Field("Button label",shortcutLabel,100);
        shortcutLine=Field("Exact console command (for example: resources centauri 5000)",shortcutLine);
        if(Button(editingId.Length==0?"SAVE NEW COMMAND":"SAVE CHANGES"))Try(()=>SaveShortcut());
        if(editingId.Length>0&&Button("Cancel editing")){editingId=shortcutLabel=shortcutLine="";}
        foreach(var s in shortcuts.ToArray())
        {
            Label(s.Label+" - "+s.Command,48);
            Pair("Edit command",()=>{editingId=s.Id;shortcutLabel=s.Label;shortcutLine=s.Command;scroll=Vector2.zero;},
                "Create sidebar button",()=>ClientModControls.EditSidebar(this,s.Id));
            Pair("Run",()=>Run(s.Command),"Remove saved command",()=>RemoveShortcut(s));
        }
    }
    private Shortcut SaveShortcut()
    {
        if(!CommandRules.SingleLine(shortcutLabel)||!Valid(shortcutLine))throw new ArgumentException("Enter a label and a complete command from the cheat catalog. Commands must occupy one line.");
        string id=editingId;var old=shortcuts.FirstOrDefault(s=>s.Id==id);
        var saved=new Shortcut{Id=old?.Id??Guid.NewGuid().ToString("N"),Label=shortcutLabel.Trim(),Command=shortcutLine.Trim()};
        int index=old==null?shortcuts.Count:shortcuts.IndexOf(old);
        if(old==null)shortcuts.Add(saved);else shortcuts[index]=saved;
        try{Save();}catch{if(old==null)shortcuts.Remove(saved);else shortcuts[index]=old;throw;}
        RegisterControls();editingId="";Notice("Saved. Choose a sidebar symbol to add or edit its button.");
        return saved;
    }
    private void RemoveShortcut(Shortcut shortcut)
    {
        int index=shortcuts.IndexOf(shortcut);shortcuts.Remove(shortcut);
        try{Save();}catch{shortcuts.Insert(index,shortcut);throw;}
        RegisterControls();Notice("Saved command removed. Its existing sidebar binding is now unavailable and can be removed in sidebar settings.");
    }
}
