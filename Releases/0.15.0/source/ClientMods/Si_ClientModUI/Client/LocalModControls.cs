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
        public bool AdminOnly,Debug;
    }
    public sealed class Definition
    {
        public string Title="";
        // Runs inside the menu's scaled, input-isolated drawing pass.
        public Action<PanelContext> DrawPanel;
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
    public static void Register(MelonMod owner,Definition definition)
    {if(owner==null||definition==null)throw new ArgumentNullException();registrations[owner]=definition;}
    public static void Register(MelonMod owner,IReadOnlyList<Control> controls)=>Register(owner,new Definition{Controls=controls});
    public static void Unregister(MelonMod owner)=>registrations.Remove(owner);
    internal static Definition For(MelonMod owner)=>registrations.TryGetValue(owner,out var value)?value:null;
    public static void Notify(MelonMod owner,string text)=>SquadHud.LocalNotification(owner,text);
}

public sealed partial class SquadHud
{
    private bool ProviderPanelActive=>menuTab.StartsWith("mod:")&&!sidebarMenuOpen&&!settingsMenuOpen&&
        localMods.Any(m=>"mod:"+LocalTitle(m)==menuTab&&(sectionControls?ClientModControls.For(m)?.DrawPanel:ClientModControls.For(m)?.DrawCommands)!=null);
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
        var draw=commands?definition?.DrawCommands:definition?.DrawPanel;if(draw==null)return false;
        try{draw(new ClientModControls.PanelContext{View=view,LabelStyle=bodyStyle,InputStyle=inputStyle,
            Label=(r,t)=>GUI.Label(r,t,bodyStyle),Button=(r,t,a)=>Button(r,t,allowed:a)});}
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
        localMods.Clear();localMods.AddRange(MelonMod.RegisteredMelons.Where(m=>m!=this));
        nextLocalDiscovery=clock.Elapsed.TotalSeconds+1;
    }
    private IEnumerable<CommandEntry> ProviderCommands()
    {
        foreach(var mod in localMods)
        {
            var definition=ClientModControls.For(mod);
            if(definition?.Commands==null)continue;
            foreach(var command in definition.Commands)
                if(command!=null&&ValidCommand(command.Command))
                    yield return new CommandEntry{Mod=LocalTitle(mod),Command=command.Command,Description=command.Description,
                        Role=command.Role,AdminOnly=command.AdminOnly,Debug=command.Debug,Group=command.Group,Scope=LocalScope(mod),Source="provider"};
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
    private void RenderLocalControls(MelonMod[] mods,Rect view,bool? adminOnly,bool debugOnly)
    {
        var rows=new List<(MelonMod owner,ClientModControls.Control control,MelonPreferences_Entry preference)>();
        foreach(var mod in mods)
        {
            var definition=ClientModControls.For(mod);
            if(definition?.Controls!=null)foreach(var c in definition.Controls)
                if(ControlAllowed(c)&&(!adminOnly.HasValue||ControlAdmin(c)==adminOnly.Value)&&(!debugOnly||c.Debug))rows.Add((mod,c,null));
            if(adminOnly==true||debugOnly)continue;
            foreach(var category in ModCategories(mod))foreach(var p in category.Entries)
                if(!p.IsHidden)rows.Add((mod,new ClientModControls.Control{Id=p.Identifier,Label=p.DisplayName,Description=p.Description??"",
                    Group=category.DisplayName,Read=()=>p.BoxedValue?.ToString()??""},p));
        }
        float width=view.width-26,top=0;
        // Compute group heights before drawing; callbacks are read only and supplied by the owning mod.
        var groups=rows.GroupBy(r=>LocalTitle(r.owner)+" / "+ModLabel(string.IsNullOrWhiteSpace(r.control.Group)?"Controls":r.control.Group)).ToArray();
        float full=groups.Sum(g=>42+(openControlGroups.Contains(g.Key)?g.Count()*112:0))+64;
        localScroll=GUI.BeginScrollView(view,localScroll,new Rect(0,0,width,full),false,true);
        try
        {
            foreach(var group in groups)
            {
                bool open=openControlGroups.Contains(group.Key);
                if(Button(new Rect(0,top,width,34),(open?"- ":"+ ")+group.Key))
                {if(open)openControlGroups.Remove(group.Key);else openControlGroups.Add(group.Key);}
                top+=42;if(!open)continue;
                foreach(var row in group)
                {
                    var c=row.control;string value="";
                    try{value=c.Read?.Invoke()??"";}catch(Exception e){value="Unavailable: "+e.GetBaseException().Message;}
                    string key=LocalScope(row.owner)+":"+c.Group+":"+c.Id;
                    GUI.Label(new Rect(8,top,width-16,28),c.Label+(value.Length>0?": "+value:""),bodyStyle);
                    GUI.Label(new Rect(8,top+30,width-16,28),c.Description??"",smallStyle);
                    if(!localDrafts.TryGetValue(key,out string draft))draft=value;
                    bool writable=c.Apply!=null||row.preference!=null&&PreferenceWritable(row.preference.GetReflectedType());
                    if(c.Choices?.Length>0)
                    {
                        float buttonWidth=(width-16)/c.Choices.Length;
                        for(int i=0;i<c.Choices.Length;i++)if(Button(new Rect(8+i*buttonWidth,top+64,buttonWidth-6,32),c.Choices[i],value==c.Choices[i],writable))
                            ApplyLocalControl(row.owner,c,row.preference,c.Choices[i]);
                    }
                    else
                    {
                        localDrafts[key]=GUI.TextField(new Rect(8,top+64,width-128,32),draft,512,inputStyle);
                        if(Button(new Rect(width-112,top+64,104,32),c.Read==null?"RUN":"APPLY",allowed:writable))
                            ApplyLocalControl(row.owner,c,row.preference,localDrafts[key]);
                    }
                    top+=112;
                }
            }
            if(rows.Count==0)GUI.Label(new Rect(0,0,width,90),"No controls exposed for this view. Mods can register their own groups, commands and status values.",smallStyle);
            else GUI.Label(new Rect(0,top,width,60),localMessage,smallStyle);
        }
        finally{GUI.EndScrollView();}
    }
    private static bool PreferenceWritable(Type type)=>type==typeof(bool)||type.IsEnum||type==typeof(string)||type.IsPrimitive&&type!=typeof(char);
    private void ApplyLocalControl(MelonMod owner,ClientModControls.Control control,MelonPreferences_Entry preference,string draft)
    {
        try
        {
            if(!localMods.Contains(owner)||!ControlAllowed(control))return;
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
                if(notificationsEnabled&&localMods.Contains(notice.owner))AddNotification(LocalTitle(notice.owner),notice.text);
            }
    }
}

