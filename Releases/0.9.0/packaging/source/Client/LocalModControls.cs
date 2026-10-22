using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace SiSquadHud;

// Optional local-only API. Mods register their own truthful actions; the HUD never unloads hooks.
public static class ClientModControls
{
    public sealed class Control
    {
        public string Id,Label,Description;
        public Func<string> Read;
        public Action<string> Apply;
        public string[] Choices=Array.Empty<string>();
    }
    public sealed class Registration
    {
        public MelonMod Owner;
        public IReadOnlyList<Control> Controls;
    }
    private static readonly Dictionary<MelonMod,Registration> registrations=new();
    public static void Register(MelonMod owner,IReadOnlyList<Control> controls)
    {
        if(owner==null||controls==null)throw new ArgumentNullException();
        registrations[owner]=new Registration{Owner=owner,Controls=controls};
    }
    public static void Unregister(MelonMod owner)=>registrations.Remove(owner);
    internal static Registration For(MelonMod owner)=>registrations.TryGetValue(owner,out var value)?value:null;
}

public sealed partial class SquadHud
{
    private readonly List<MelonMod> localMods=new();
    private MelonMod selectedLocalMod;
    private Vector2 localScroll;
    private readonly Dictionary<string,string> localDrafts=new();
    private string localMessage="";
    private void RefreshLocalMods()
    {
        localMods.Clear();
        localMods.AddRange(MelonMod.RegisteredMelons.Where(m=>m!=this));
        if(selectedLocalMod==null||!localMods.Contains(selectedLocalMod))selectedLocalMod=localMods.FirstOrDefault();
    }
    private static string Normalized(string value)=>new string((value??"").Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    private static IEnumerable<MelonPreferences_Category> ModCategories(MelonMod mod)
    {
        string assembly=Normalized(mod.GetType().Assembly.GetName().Name),name=Normalized(mod.Info.Name);
        return MelonPreferences.Categories.Where(c=>!c.IsHidden&&
            (Normalized(c.Identifier)==assembly||Normalized(c.Identifier)==name));
    }
    private void RenderLocalMods(Rect view)
    {
        if(!Game.GetIsSingleplayer()){GUI.Label(view,"Local mod controls are available in singleplayer.",bodyStyle);return;}
        float leftWidth=Math.Min(230,view.width*.27f);
        float y=view.y;
        foreach(var mod in localMods)
        {
            if(Button(new Rect(view.x,y,leftWidth,40),mod.Info.Name,mod==selectedLocalMod))
            {selectedLocalMod=mod;localScroll=Vector2.zero;}
            y+=46;
        }
        var area=new Rect(view.x+leftWidth+16,view.y,view.width-leftWidth-16,view.height-50);
        if(selectedLocalMod==null)return;
        var categories=ModCategories(selectedLocalMod).ToArray();
        var controls=ClientModControls.For(selectedLocalMod)?.Controls??Array.Empty<ClientModControls.Control>();
        var preferenceList=new List<MelonPreferences_Entry>();
        foreach(var category in categories)foreach(var entry in category.Entries)if(!entry.IsHidden)preferenceList.Add(entry);
        var preferences=preferenceList.ToArray();
        float width=area.width-26,top=0;
        localScroll=GUI.BeginScrollView(area,localScroll,new Rect(0,0,width,100+preferences.Length*110+controls.Count*110),false,true);
        try
        {
            GUI.Label(new Rect(0,0,width,68),selectedLocalMod.Info.Name+" "+selectedLocalMod.Info.Version+
                "\nLocal preferences and controls supplied by the mod. A preference may require a scene change or restart.",smallStyle);top=76;
            foreach(var control in controls)
            {
                string key="api:"+selectedLocalMod.GetType().Assembly.FullName+":"+control.Id;
                string current;
                try{current=control.Read?.Invoke()??"";}catch(Exception e){current="Unavailable: "+e.GetBaseException().Message;}
                GUI.Label(new Rect(0,top,width,28),control.Label+": "+current,bodyStyle);
                GUI.Label(new Rect(0,top+30,width,28),control.Description??"",smallStyle);
                localDrafts.TryGetValue(key,out var draft);draft??=current;
                draft=GUI.TextField(new Rect(0,top+62,width-110,32),draft,512,inputStyle);localDrafts[key]=draft;
                if(Button(new Rect(width-102,top+62,102,32),"APPLY",allowed:control.Apply!=null))
                {
                    try
                    {
                        if(control.Choices?.Length>0&&!control.Choices.Contains(draft))throw new InvalidOperationException("Use one of: "+string.Join(", ",control.Choices));
                        control.Apply(draft);localMessage="Applied through the mod's own control.";
                    }
                    catch(Exception e){localMessage=e.GetBaseException().Message;}
                }
                top+=110;
            }
            foreach(var entry in preferences)
            {
                string key="pref:"+entry.Category.Identifier+":"+entry.Identifier;
                var type=entry.GetReflectedType();object current=entry.BoxedValue;
                bool writable=type==typeof(bool)||type.IsEnum||type==typeof(string)||type.IsPrimitive&&type!=typeof(char);
                GUI.Label(new Rect(0,top,width,28),entry.DisplayName+": "+current,bodyStyle);
                GUI.Label(new Rect(0,top+30,width,28),entry.Description??entry.Identifier,smallStyle);
                if(!localDrafts.TryGetValue(key,out string draft))draft=Convert.ToString(current,CultureInfo.InvariantCulture)??"";
                if(type==typeof(bool))
                {
                    if(Button(new Rect(0,top+62,width-110,32),(bool)current?"SET FALSE":"SET TRUE"))
                        ApplyLocalPreference(entry,!((bool)current));
                }
                else
                {
                    localDrafts[key]=GUI.TextField(new Rect(0,top+62,width-110,32),draft,512,inputStyle);
                    if(Button(new Rect(width-102,top+62,102,32),"APPLY",allowed:writable))
                    {
                        try
                        {
                            object value=type.IsEnum?Enum.Parse(type,localDrafts[key],true):Convert.ChangeType(localDrafts[key],type,CultureInfo.InvariantCulture);
                            ApplyLocalPreference(entry,value);
                        }
                        catch(Exception e){localMessage=e.GetBaseException().Message;}
                    }
                }
                top+=110;
            }
            if(controls.Count==0&&preferences.Length==0)
                GUI.Label(new Rect(0,top,width,100),"This mod exposes no supported local controls. It remains loaded; no master switch is assumed.",bodyStyle);
        }
        finally{GUI.EndScrollView();}
        if(Button(new Rect(area.x,view.yMax-42,140,32),"SAVE PREFS",allowed:categories.Length>0))
            SaveLocalPreferences(categories);
        GUI.Label(new Rect(area.x+150,view.yMax-42,area.width-150,80),localMessage,smallStyle);
    }
    private void ApplyLocalPreference(MelonPreferences_Entry entry,object value)
    {
        try
        {
            if(!Game.GetIsSingleplayer())return;
            // Use the loader's own validation/event path. No private gameplay fields or hook changes.
            entry.BoxedValue=value;
            localMessage="Updated "+entry.DisplayName+" locally. The mod decides when the value takes effect.";
        }
        catch(Exception e){localMessage=e.GetBaseException().Message;}
    }
    private void SaveLocalPreferences(MelonPreferences_Category[] categories)
    {
        if(!Game.GetIsSingleplayer())return;
        const BindingFlags flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
        try
        {
            string folder=Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory,"Si_SquadHUD","PreferenceBackups",DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ"));
            Directory.CreateDirectory(folder);
            string Digest(string file)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file)));
            var backed=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var receipts=new List<object>();
            foreach(var category in categories)
            {
                object file=typeof(MelonPreferences_Category).GetField("File",flags)?.GetValue(category)??
                    typeof(MelonPreferences).GetField("DefaultFile",flags)?.GetValue(null);
                if(file==null)throw new InvalidOperationException("Loader preference file could not be resolved; save cancelled.");
                string path=file.GetType().GetProperty("FilePath",flags)?.GetValue(file) as string??file.GetType().GetField("FilePath",flags)?.GetValue(file) as string;
                if(string.IsNullOrWhiteSpace(path))throw new InvalidOperationException("Preference path unavailable; save cancelled.");
                if(backed.Add(path))
                {
                    string hash=File.Exists(path)?Digest(path):null;
                    string backup=Path.Combine(folder,backed.Count+"-"+Path.GetFileName(path));
                    if(hash!=null){File.Copy(path,backup,false);if(Digest(backup)!=hash)throw new IOException("Preference backup verification failed.");}
                    receipts.Add(new {path,backup=hash==null?null:backup,sha256=hash});
                }
                File.WriteAllText(Path.Combine(folder,"backups.json"),System.Text.Json.JsonSerializer.Serialize(receipts));
                category.SaveToFile(false);
                bool error=file.GetType().GetField("WasError",flags)?.GetValue(file) as bool? ??
                    file.GetType().GetProperty("WasError",flags)?.GetValue(file) as bool? ?? false;
                if(error||!File.Exists(path))throw new IOException("Loader reported a preference save failure. Backup: "+folder);
            }
            localMessage="Preferences saved. Verified backups: "+folder;
        }
        catch(Exception e){localMessage=e.GetBaseException().Message;}
    }
}

