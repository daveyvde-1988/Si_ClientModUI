using Il2Cpp;
using UnityEngine;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private long uiRevision;
    private long uiCommandBuilds,uiTextMeasures,uiProviderReads,uiStatusRefreshes;
    private string uiContext="";
    private double nextProviderProbe;
    private int providerFingerprint;
    private readonly RevisionCache<CommandEntry[]> commandSnapshots=new();
    private Func<CommandEntry[]> commandBuilder;
    private long commandCacheRevision=-1;
    private SidebarBinding[] sidebarCache;
    private CommandEntry[] commandRowCache;
    private float[] commandHeightCache;
    private string commandRowKey="",feedbackCacheKey="",statusTextKey="";
    private string feedbackTextCache="",statusTextCache="";
    private float feedbackHeightCache;
    private double statusTextAt;
    private readonly Dictionary<string,(double at,string value)> uiValues=new();
    private readonly Dictionary<(int font,int width,string text),float> textMeasurements=new();
    private float MeasureText(GUIStyle style,string text,float width)
    {
        var key=(style.fontSize,(int)width,text);
        if(textMeasurements.TryGetValue(key,out float height))return height;
        if(textMeasurements.Count>=512)textMeasurements.Clear();
        uiTextMeasures++;height=style.CalcHeight(new GUIContent(text),width);textMeasurements[key]=height;return height;
    }
    private void InvalidateUi(bool immediate=false)
    {
        uiRevision++;sidebarCache=null;statusTextAt=0;feedbackCacheKey="";
        if(immediate){commandSnapshots.Force();availableAt=0;uiValues.Clear();}
    }
    internal static void ProviderChanged()=>instance?.InvalidateUi();
    private void RefreshUiContext()
    {
        if(HasSession)EnsureWindowProfile();
        string key=sessionContext+":"+CurrentAdmin+":"+layout.TextSize+":"+layout.Transparency+":"+ClientModControls.Revision;
        if(key!=uiContext){uiContext=key;InvalidateUi(true);}
        if((!panelOpen||settingsMenuOpen||sidebarMenuOpen)&&!statusPickerOpen)return;
        double now=clock.Elapsed.TotalSeconds;if(now<nextProviderProbe)return;nextProviderProbe=now+UiPolicies.UiRefreshSeconds;
        var hash=new HashCode();
        foreach(var mod in localMods)
        {
            var d=ClientModControls.For(mod);hash.Add(d);if(d==null)continue;
            hash.Add(d.Title);hash.Add(d.Controls?.Count);hash.Add(d.Statuses?.Count);hash.Add(d.Sidebar?.Count);
            if(d.Commands!=null)foreach(var c in d.Commands)if(c!=null)
            {hash.Add(c.Command);hash.Add(c.Description);hash.Add(c.Group);hash.Add(c.Role);hash.Add(c.AdminOnly);hash.Add(c.Debug);hash.Add(c.RequiresInput);}
            if(d.Controls!=null)foreach(var c in d.Controls)if(c!=null)
            {hash.Add(c.Id);hash.Add(c.Label);hash.Add(c.Role);hash.Add(c.AdminOnly);hash.Add(c.Group);}
        }
        int value=hash.ToHashCode();if(value!=providerFingerprint){providerFingerprint=value;InvalidateUi();}
    }
    private CommandEntry[] CommandSnapshot()
    {
        commandBuilder??=()=>{uiCommandBuilds++;return BuildCommands().ToArray();};
        var result=commandSnapshots.Get(clock.Elapsed.TotalSeconds,uiRevision,commandBuilder);
        if(commandCacheRevision!=commandSnapshots.Revision)
        {
            commandCacheRevision=commandSnapshots.Revision;commandRowCache=null;menuTabsRevision=-1;
        }
        return result;
    }
    private SidebarBinding[] CachedSidebarButtons()=>sidebarCache??=CurrentSidebarBindings.Where(b=>b.Enabled&&BindingAllowed(b)).ToArray();
    private string ReadUiValue(string key,Func<string> read)
    {
        double now=clock.Elapsed.TotalSeconds;
        if(uiValues.TryGetValue(key,out var old)&&now<old.at)return old.value;
        uiProviderReads++;string value;try{value=read?.Invoke()??"";}catch(Exception e){value="Unavailable: "+e.GetBaseException().Message;}
        if(uiValues.Count>1024)uiValues.Clear();uiValues[key]=(now+UiPolicies.UiRefreshSeconds,value);return value;
    }
    private void CachedCommandRows(float width,out CommandEntry[] rows,out float[] heights)
    {
        CommandSnapshot();
        string key=commandCacheRevision+":"+menuTab+":"+roleFilter+":"+commandSearch+":"+width+":"+smallStyle.fontSize;
        if(commandRowCache==null||key!=commandRowKey)
        {
            commandRowKey=key;string search=commandSearch.Trim();
            commandRowCache=AllCommands().Where(InCommandView).Where(c=>search.Length==0||(c.Command+" "+ModLabel(c.Mod)+" "+c.Description).Contains(search,StringComparison.OrdinalIgnoreCase))
                .OrderBy(c=>c.Source=="server-menu"?0:1).ThenBy(c=>ModLabel(c.Mod)).ThenBy(c=>c.Group).ThenBy(c=>c.MenuOrder)
                .ThenBy(c=>c.Command,StringComparer.OrdinalIgnoreCase).ToArray();
            commandHeightCache=commandRowCache.Select(r=>Math.Max(68,MeasureText(smallStyle,r.Description,Math.Max(80,width-360))+36)).ToArray();
        }
        rows=commandRowCache;heights=commandHeightCache;
    }
    private void CachedFeedback(float width,out string text,out float height)
    {
        string key=uiRevision+":"+menuTab+":"+width+":"+smallStyle.fontSize+":"+CompanionStatus;
        if(key!=feedbackCacheKey)
        {
            feedbackCacheKey=key;
            var lines=feedback.Where(f=>(!f.AdminOnly||CurrentAdmin)&&(menuTab=="help"||menuTab=="admin"||menuTab=="debug"||menuTab=="mod:"+ModLabel(f.Mod))).TakeLast(80);
            feedbackTextCache=string.Join("\n\n",lines.Select(f=>"["+ModLabel(f.Mod)+"] "+f.Text));
            if(feedbackTextCache.Length==0)feedbackTextCache="No feedback received. "+CompanionStatus+".";
            feedbackHeightCache=MeasureText(smallStyle,feedbackTextCache,width);
        }
        text=feedbackTextCache;height=feedbackHeightCache;
    }
    private string CachedStatusText()
    {
        string key=uiRevision+":"+CompanionStatus+":"+voteQuestion;
        if(key!=statusTextKey||clock.Elapsed.TotalSeconds>=statusTextAt)
        {
            statusTextKey=key;statusTextAt=clock.Elapsed.TotalSeconds+UiPolicies.HousekeepingSeconds;
            var values=layout.SelectedStatus.Count==0?Array.Empty<StatusParameter>():AvailableParameters().Where(r=>layout.SelectedStatus.Contains(r.Id)).ToArray();
            string text=string.Join("\n",values.Select(r=>r.Label+": "+r.Value));
            statusTextCache=CompanionStatus+"\n"+(serverVotes.Count>0?"Vote: "+voteQuestion+"\n":"")+(text.Length==0?"No status values selected.":text);
        }
        return statusTextCache;
    }
    private long menuTabsRevision=-1;
    private List<(string key,string title)> menuTabsCache;
    private bool cachedHasAdmin,cachedShowAdmin;
    private float[] menuTabWidths=Array.Empty<float>();
    private float menuTabTotal;
    private List<(string key,string title)> CachedMenuTabs(out bool hasAdmin,out bool showAdmin)
    {
        CommandSnapshot();
        if(menuTabsRevision!=commandCacheRevision)
        {
            var known=CommandSnapshot();menuTabsCache=new(){("help","HELP")};
            menuTabsCache.AddRange(localMods.Select(m=>LocalTitle(m)).Concat(known.Where(c=>!AdminCommand(c)||Game.GetIsSingleplayer()).Select(c=>ModLabel(c.Mod)))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(m=>m).Select(m=>("mod:"+m,m)));
            cachedHasAdmin=known.Any(c=>AdminCommand(c)&&CommandAllowed(c));
            cachedShowAdmin=ShellRules.ShowAdmin(Game.GetIsSingleplayer(),CurrentAdmin,cachedHasAdmin,
                localMods.Any(m=>ClientModControls.For(m)?.Controls?.Any(c=>ControlAllowed(c)&&ControlAdmin(c))==true));
            if(cachedShowAdmin)menuTabsCache.Add(("admin","ADMIN"));
            menuTabWidths=menuTabsCache.Select(t=>Math.Max(106,tabStyle.CalcSize(new GUIContent(t.title)).x+24)).ToArray();
            menuTabTotal=menuTabWidths.Sum()+menuTabWidths.Length*8;menuTabsRevision=commandCacheRevision;
        }
        hasAdmin=cachedHasAdmin;showAdmin=cachedShowAdmin;return menuTabsCache;
    }
}
