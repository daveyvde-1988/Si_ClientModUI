using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace SiSquadHud;

public static class ClientModControls
{
    public sealed class Control
    {
        public string Id="",Label="",Description="",Group="",Role="shared";
        public bool AdminOnly,Debug;
        public Func<string> Read;
        public Action<string> Apply;
        public string[] Choices=Array.Empty<string>();
        public string Kind="",Unit="",DefaultValue,VanillaValue;
        public double? Minimum,Maximum,Step;
    }
    public sealed class Status
    {
        public string Id="",Label="",Group="",Role="shared";
        public bool AdminOnly;
        public Func<string> Read;
    }
    public sealed class ChatCommand
    {
        public string Command="",Description="",Group="",Role="shared";
        public bool AdminOnly,Debug,RequiresInput;
    }
    public sealed class Definition
    {
        public string Title="",Description="";
        // Runs inside the menu's scaled, input-isolated drawing pass.
        public bool PanelAdminOnly;
        // Optionally expose the same panel in PLAYER while a provider-owned condition holds.
        // The provider must also recheck this condition when executing an action.
        public Func<bool> PlayerPanelAvailable;
        public Action<PanelContext> DrawPanel;
        // Command reference content shown under COMMANDS -> CLIENT MODS -> this mod.
        public Action<PanelContext> DrawCommands;
        public IReadOnlyList<Control> Controls=Array.Empty<Control>();
        public IReadOnlyList<Status> Statuses=Array.Empty<Status>();
        public IReadOnlyList<ChatCommand> Commands=Array.Empty<ChatCommand>();
        public IReadOnlyList<SidebarAction> Sidebar=Array.Empty<SidebarAction>();
        public IReadOnlyList<SidebarSymbol> SidebarSymbols=Array.Empty<SidebarSymbol>();
    }
    public sealed class PanelContext
    {
        public Rect View;
        public GUIStyle LabelStyle,InputStyle;
        public Action<Rect,string> Label;
        public Func<Rect,string,bool,bool> Button;
        // Returns a draggable divider's Y coordinate, persisted per team and role.
        public Func<string,Rect,float,float,float> Divider;
    }
    public static void EditSidebar(MelonMod owner,string actionId)=>SquadHud.EditProviderSidebar(owner,actionId);
    public sealed class SidebarAction
    {
        public string Id="",Label="",Description="",Symbol="shape:circle-filled",Color="#FFFFFF",Role="shared",Command="";
        public bool AdminOnly;
        public Action Invoke;
        public Func<bool> Available;
    }
    public sealed class SidebarSymbol
    {
        public string Id="",Label="";
        public Texture2D Texture;
    }
    private static readonly Dictionary<MelonMod,Definition> registrations=new();
    internal static long Revision {get;private set;}
    public static void Register(MelonMod owner,Definition definition)
    {if(owner==null||definition==null)throw new ArgumentNullException();registrations[owner]=definition;Revision++;SquadHud.ProviderChanged();}
    public static void Register(MelonMod owner,IReadOnlyList<Control> controls)=>Register(owner,new Definition{Controls=controls});
    public static void Unregister(MelonMod owner){if(registrations.Remove(owner)){Revision++;SquadHud.ProviderChanged();}}
    internal static Definition For(MelonMod owner)=>registrations.TryGetValue(owner,out var value)?value:null;
    public static void Notify(MelonMod owner,string text)=>SquadHud.LocalNotification(owner,text);
}

public sealed partial class SquadHud
{
    private bool ProviderPanelActive
    {
        get
        {
            if(sidebarMenuOpen||settingsMenuOpen||addEntryOpen||mainMenu is not ("MODS" or "COMMANDS"))return false;
            bool commands=mainMenu=="COMMANDS",admin=commands?commandAdmin:modAdmin;
            return localMods.Any(m=>LocalScope(m)==navigationKey&&ProviderPanelAllowed(ClientModControls.For(m),admin)&&
                (commands?ClientModControls.For(m)?.DrawCommands:ClientModControls.For(m)?.DrawPanel)!=null);
        }
    }
    private static bool ProviderPanelAllowed(ClientModControls.Definition definition,bool admin)
    {
        if(definition==null)return false;
        if(admin)return CurrentAdmin&&definition.PanelAdminOnly;
        if(!definition.PanelAdminOnly)return true;
        try{return definition.PlayerPanelAvailable?.Invoke()==true;}catch{return false;}
    }
    internal static void EditProviderSidebar(MelonMod owner,string actionId)
    {
        if(instance==null)return;
        var provider=instance.ProviderSidebarActions().FirstOrDefault(p=>p.Owner==owner&&p.Action.Id==actionId);
        if(provider==null)return;
        instance.OpenSidebarMenu();
        var binding=instance.CurrentSidebarBindings.FirstOrDefault(b=>b.Kind=="provider"&&b.Provider==provider.Key);
        instance.BeginSidebarEdit(binding,provider);
    }
    private bool RenderProviderPanel(MelonMod[] mods,Rect view,bool commands=false)
    {
        if(mods.Length!=1)return false;
        var definition=ClientModControls.For(mods[0]);
        bool admin=commands?commandAdmin:modAdmin;
        var draw=commands?definition?.DrawCommands:definition?.DrawPanel;if(draw==null||!ProviderPanelAllowed(definition,admin))return false;
        try{draw(new ClientModControls.PanelContext{View=view,LabelStyle=bodyStyle,InputStyle=inputStyle,
            Label=(r,t)=>GUI.Label(r,t,bodyStyle),Button=(r,t,a)=>Button(r,t,allowed:a),
            Divider=(id,r,above,below)=>FeedbackDivider(r.x,r.y,r.yMax,r.width,LocalScope(mods[0])+":"+id,above,below)});}
        catch(Exception e){Error(e,"provider panel");GUI.Label(view,"Provider panel unavailable. See Debug for details.",bodyStyle);}
        return true;
    }
    private readonly List<MelonMod> localMods=new();
    private Vector2 localScroll;
    private readonly Dictionary<string,string> localDrafts=new();
    private readonly HashSet<string> openControlGroups=new();
    private string localMessage="";
    private double nextLocalDiscovery;
    private static string LocalScope(MelonMod mod)=>"local:"+mod.GetType().Assembly.GetName().Name;
    private string LocalTitle(MelonMod mod)
    {
        string title=ClientModControls.For(mod)?.Title;
        return ModLabel(string.IsNullOrWhiteSpace(title)?mod.Info.Name:title);
    }
    private void RefreshLocalMods()
    {
        var next=MelonMod.RegisteredMelons.Where(m=>m!=this).ToArray();
        if(!localMods.SequenceEqual(next)){localMods.Clear();localMods.AddRange(next);InvalidateUi();}
        nextLocalDiscovery=clock.Elapsed.TotalSeconds+1;
    }
    private IEnumerable<CommandEntry> ProviderCommands()
    {
        foreach(var mod in localMods)
        {
            if(!MelonMod.RegisteredMelons.Contains(mod))continue;
            var definition=ClientModControls.For(mod);
            if(definition?.Commands==null)continue;
            foreach(var command in definition.Commands)
                if(command!=null&&ValidCommand(command.Command))
                    yield return new CommandEntry{Mod=LocalTitle(mod),Command=command.Command,Description=command.Description,
                        Role=command.Role,AdminOnly=command.AdminOnly,Debug=command.Debug,RequiresInput=command.RequiresInput,Group=command.Group,Scope=LocalScope(mod),Source="provider"};
        }
    }
    private static string Normalized(string value)=>new string((value??"").Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    private static IEnumerable<MelonPreferences_Category> ModCategories(MelonMod mod)
    {
        string assembly=Normalized(mod.GetType().Assembly.GetName().Name),name=Normalized(mod.Info.Name);
        return MelonPreferences.Categories.Where(c=>!c.IsHidden&&(Normalized(c.Identifier)==assembly||Normalized(c.Identifier)==name));
    }
    private static bool ControlAllowed(ClientModControls.Control c)=>c!=null&&(!c.AdminOnly||CurrentAdmin)&&RoleAllowed(c.Role);
    private static bool ControlAdmin(ClientModControls.Control c)=>c.AdminOnly||c.Role=="admin";
    private sealed class ControlGroup
    {
        internal string Title;
        internal List<(MelonMod owner,ClientModControls.Control control,MelonPreferences_Entry preference)> Rows;
    }
    private string localRowsKey="";
    private ControlGroup[] localGroups=Array.Empty<ControlGroup>();
    private static string LocalControlKey(MelonMod owner,ClientModControls.Control c)=>LocalScope(owner)+":"+c.Group+":"+c.Id;
    private ServerCommand LocalControlModel(ClientModControls.Control c,string value)=>new(){Text=c.Id,Label=c.Label,Description=c.Description,Value=c.Read==null?null:value,Role=c.Role,Admin=c.AdminOnly,
        Choices=(c.Choices??Array.Empty<string>()).Select(v=>new[]{v,v}).ToList(),
        Ui=new ControlPresentation{Kind=c.Kind,Group=c.Group,Min=c.Minimum,Max=c.Maximum,Step=c.Step,Unit=c.Unit,Default=c.DefaultValue,Vanilla=c.VanillaValue}};
    private List<(MelonMod owner,ClientModControls.Control control,MelonPreferences_Entry preference)> BuildLocalRows(MelonMod[] mods,bool? adminOnly,bool debugOnly)
    {
        var rows=new List<(MelonMod owner,ClientModControls.Control control,MelonPreferences_Entry preference)>();
        foreach(var mod in mods)
        {
            var definition=ClientModControls.For(mod);
            if(definition?.Controls!=null)foreach(var c in definition.Controls)
                if(ControlAllowed(c)&&(!adminOnly.HasValue||UiPolicies.ModCategory(ControlAdmin(c),adminOnly.Value,CurrentAdmin))&&(!debugOnly||c.Debug))rows.Add((mod,c,null));
            if(adminOnly==true||debugOnly)continue;
            foreach(var category in ModCategories(mod))foreach(var p in category.Entries)
                if(!p.IsHidden)rows.Add((mod,new ClientModControls.Control{Id=p.Identifier,Label=p.DisplayName,Description=p.Description??"",
                    Group=category.DisplayName,Read=()=>p.BoxedValue?.ToString()??""},p));
        }
        return rows;
    }
    private void RenderLocalControls(MelonMod[] mods,Rect view,bool? adminOnly,bool debugOnly)
    {
        CommandSnapshot();string cacheKey=commandCacheRevision+":"+menuTab+":"+adminOnly+":"+debugOnly;
        if(cacheKey!=localRowsKey)
        {
            localRowsKey=cacheKey;var rows=BuildLocalRows(mods,adminOnly,debugOnly);
            localGroups=rows.GroupBy(r=>LocalTitle(r.owner)+" / "+ModLabel(string.IsNullOrWhiteSpace(r.control.Group)?"Controls":r.control.Group))
                .Select(g=>new ControlGroup{Title=g.Key,Rows=g.ToList()}).ToArray();
        }
        float width=view.width-26,top=0;
        // Compute group heights before drawing; callbacks are read only and supplied by the owning mod.
        var groups=localGroups;
        float RowHeight((MelonMod owner,ClientModControls.Control control,MelonPreferences_Entry preference) row)
        {
            var c=row.control;string key=LocalControlKey(row.owner,c),value=ReadUiValue(key,c.Read);
            if(!string.IsNullOrEmpty(c.Kind))return TypedControlHeight(key,LocalControlModel(c,value),width-16)+6;
            bool writable=c.Apply!=null||row.preference!=null&&PreferenceWritable(row.preference.GetReflectedType());
            bool action=c.Read==null&&(c.Choices?.Length??0)==0&&row.preference==null;
            float label=Math.Max(32,MeasureText(action?buttonStyle:bodyStyle,c.Label+(value.Length>0?": "+value:""),Math.Max(40,width-36)));
            return label+ControlDescriptionHeight(c.Description,width-16)+(!action&&writable?40:0)+12;
        }
        float full=groups.Sum(g=>42+(openControlGroups.Contains(g.Title)?g.Rows.Sum(RowHeight):0))+40;
        localScroll=GUI.BeginScrollView(view,localScroll,new Rect(0,0,width,full),false,true);
        try
        {
            foreach(var group in groups)
            {
                bool open=openControlGroups.Contains(group.Title);
                if(Button(new Rect(0,top,width,34),(open?"- ":"+ ")+group.Title))
                {if(open)openControlGroups.Remove(group.Title);else openControlGroups.Add(group.Title);}
                top+=42;if(!open)continue;
                foreach(var row in group.Rows)
                {
                    var c=row.control;float rowHeight=RowHeight(row);
                    if(top+rowHeight<localScroll.y||top>localScroll.y+view.height){top+=rowHeight;continue;}
                    string value="";
                    string key=LocalControlKey(row.owner,c);
                    value=ReadUiValue(key,c.Read);
                    if(!string.IsNullOrEmpty(c.Kind))
                    {
                        var model=LocalControlModel(c,value);
                        RenderTypedControl(new Rect(8,top,width-16,rowHeight-6),key,model,v=>ApplyLocalControl(row.owner,c,row.preference,v));
                        top+=rowHeight;continue;
                    }
                    bool writable=c.Apply!=null||row.preference!=null&&PreferenceWritable(row.preference.GetReflectedType());
                    bool action=c.Read==null&&(c.Choices?.Length??0)==0&&row.preference==null;
                    float labelHeight=Math.Max(32,MeasureText(action?buttonStyle:bodyStyle,c.Label+(value.Length>0?": "+value:""),Math.Max(40,width-36)));
                    if(action){if(Button(new Rect(8,top,width-16,labelHeight),c.Label,allowed:writable,description:c.Description))ApplyLocalControl(row.owner,c,row.preference,"");}
                    else GUI.Label(new Rect(8,top,width-16,labelHeight),c.Label+(value.Length>0?": "+value:""),bodyStyle);
                    float descriptionHeight=ControlDescriptionHeight(c.Description,width-16),editorY=top+labelHeight+descriptionHeight+4;
                    if(descriptionHeight>0)GUI.Label(new Rect(8,top+labelHeight+2,width-16,descriptionHeight),c.Description,smallStyle);
                    if(!localDrafts.TryGetValue(key,out string draft))draft=value;
                    if(action||!writable){top+=rowHeight;continue;}
                    if(c.Choices?.Length>0)
                    {
                        float buttonWidth=(width-16)/c.Choices.Length;
                        for(int i=0;i<c.Choices.Length;i++)if(Button(new Rect(8+i*buttonWidth,editorY,buttonWidth-6,32),c.Choices[i],value==c.Choices[i],writable))
                            ApplyLocalControl(row.owner,c,row.preference,c.Choices[i]);
                    }
                    else
                    {
                        localDrafts[key]=GUI.TextField(new Rect(8,editorY,width-128,32),draft,512,inputStyle);
                        if(Button(new Rect(width-112,editorY,104,32),"APPLY",allowed:writable))
                            ApplyLocalControl(row.owner,c,row.preference,localDrafts[key]);
                    }
                    top+=rowHeight;
                }
            }
            if(groups.Length==0)GUI.Label(new Rect(0,0,width,90),"No controls exposed for this view. Mods can register their own groups, commands and status values.",smallStyle);
            else GUI.Label(new Rect(0,top,width,60),localMessage,smallStyle);
        }
        finally{GUI.EndScrollView();}
    }
    private static bool PreferenceWritable(Type type)=>type==typeof(bool)||type.IsEnum||type==typeof(string)||type.IsPrimitive&&type!=typeof(char);
    private void ApplyLocalControl(MelonMod owner,ClientModControls.Control control,MelonPreferences_Entry preference,string draft)
    {
        try
        {
            if(!localMods.Contains(owner)||!MelonMod.RegisteredMelons.Contains(owner)||!ControlAllowed(control))return;
            if(preference==null)
            {
                var current=ClientModControls.For(owner)?.Controls?.FirstOrDefault(c=>c.Id==control.Id&&c.Group==control.Group);
                if(current==null||!ControlAllowed(current))return;control=current;
            }
            if(preference!=null)
            {
                var type=preference.GetReflectedType();
                preference.BoxedValue=type.IsEnum?Enum.Parse(type,draft,true):Convert.ChangeType(draft,type,CultureInfo.InvariantCulture);
                localMessage="Preference updated in memory. The mod controls when it takes effect.";
            }
            else
            {
                if(control.Choices?.Count()>0&&!control.Choices.Contains(draft))throw new InvalidOperationException("Invalid choice.");
                control.Apply?.Invoke(draft);localMessage="Handled by "+LocalTitle(owner)+".";
            }
        }
        catch(Exception e){localMessage=e.GetBaseException().Message;}
        InvalidateUi(true);
    }
    internal static void LocalNotification(MelonMod owner,string text)
    {
        if(instance==null||owner==null||!instance.localMods.Contains(owner))return;
        lock(instance.pendingLocalNotices)
        {
            instance.pendingLocalNotices.Enqueue((owner,text??""));
            while(instance.pendingLocalNotices.Count>200)instance.pendingLocalNotices.Dequeue();
        }
    }
    private readonly Queue<(MelonMod owner,string text)> pendingLocalNotices=new();
    private void DrainLocalNotices()
    {
        lock(pendingLocalNotices)
            while(pendingLocalNotices.Count>0)
            {
                var notice=pendingLocalNotices.Dequeue();
                AddDebugEvent("Provider notice ["+LocalTitle(notice.owner)+"] "+notice.text);
                if(localMods.Contains(notice.owner)){AddNotification(LocalTitle(notice.owner),notice.text);feedback.Add(new FeedbackLine{Mod=LocalTitle(notice.owner),Text=Plain(notice.text)});if(feedback.Count>200)feedback.RemoveAt(0);InvalidateUi();}
            }
    }
}

