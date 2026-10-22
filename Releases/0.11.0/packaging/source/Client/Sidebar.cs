using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    public sealed class SidebarBinding
    {
        public string Id {get;set;}=Guid.NewGuid().ToString("N");
        public string Kind {get;set;}="chat";
        public string Mod {get;set;}="Custom";
        public string Label {get;set;}="";
        public string Command {get;set;}="";
        public string Provider {get;set;}="";
        public string Symbol {get;set;}="shape:circle-filled";
        public string Color {get;set;}="#FFFFFF";
        public string Role {get;set;}="shared";
        public bool AdminOnly {get;set;}
        public bool Enabled {get;set;}=true;
    }
    private bool sidebarMenuOpen,editingSidebar;
    private string editingSidebarId="",sideLabel="",sideCommand="",sideSymbol="shape:circle-filled",sideColor="#FFFFFF",sideRole="shared",sideProvider="",sideMod="Custom";
    private bool sideAdmin;
    private Vector2 sidebarScroll,sidebarMenuScroll;
    private Rect sidebarRect;
    private double lastSidebarAction=-100;
    private sealed class ProviderSidebar {internal MelonMod Owner;internal ClientModControls.SidebarAction Action;internal string Key;}
    private IEnumerable<CommandEntry> UserSidebarCommands()
    {
        foreach(var b in layout.SidebarButtons)
            if(b!=null&&b.Kind=="chat"&&ValidChat(b.Command))
                yield return new CommandEntry{Mod=ModLabel(b.Mod),Command=b.Command,Description=b.Label,Role=b.Role,AdminOnly=b.AdminOnly,Scope="portable",Source="sidebar",Group="Sidebar"};
    }
    private IEnumerable<ProviderSidebar> ProviderSidebarActions()
    {
        foreach(var owner in localMods)
        {
            var actions=ClientModControls.For(owner)?.Sidebar;if(actions==null)continue;
            foreach(var action in actions)
                if(action!=null&&!string.IsNullOrWhiteSpace(action.Id)&&(!action.AdminOnly||CurrentAdmin)&&RoleAllowed(action.Role)&&
                    (action.Invoke!=null||ValidChat(action.Command)))
                    yield return new ProviderSidebar{Owner=owner,Action=action,Key=LocalScope(owner)+":sidebar:"+action.Id};
        }
    }
    private bool BindingAllowed(SidebarBinding b)=>b!=null&&(!b.AdminOnly||CurrentAdmin)&&RoleAllowed(b.Role);
    private bool CanUseSidebar(SidebarBinding binding)
    {
        if(!BindingAllowed(binding)||!binding.Enabled)return false;
        if(binding.Kind=="chat")return ValidChat(binding.Command);
        var provider=ProviderSidebarActions().FirstOrDefault(p=>p.Key==binding.Provider);
        if(provider==null)return false;
        try{return provider.Action.Available?.Invoke()??true;}catch{return false;}
    }
    private bool SidebarVisible=>HasSession&&layout.SidebarEnabled;
    private bool SidebarPointerCaptured=>SidebarVisible&&OverlayContains(sidebarRect,"sidebar");
    private bool OverlayContains(Rect rect,string key)
    {
        key=RoleWindowKey(key);
        if(movingWindow==key)return true;if(movingWindow.Length>0)return false;
        var mouse=Mouse.current;if(mouse==null)return false;var point=mouse.position.ReadValue();
        return Cursor.visible&&rect.Contains(new Vector2(point.x/UiScale,(Screen.height-point.y)/UiScale));
    }
    private void OpenSidebarMenu()
    {
        if(!panelOpen){hudExpanded=true;TogglePanel();}
        if(panelOpen){sidebarMenuOpen=true;settingsMenuOpen=false;statusPickerOpen=false;addEntryOpen=false;}
    }
    private void RunSidebar(SidebarBinding binding)
    {
        if(!HasSession||!Application.isFocused||NativeMenu()||!CanUseSidebar(binding)||clock.Elapsed.TotalSeconds-lastSidebarAction<.2)return;
        lastSidebarAction=clock.Elapsed.TotalSeconds;
        if(binding.Kind=="chat"){SendTypedChat(binding.Command);return;}
        var provider=ProviderSidebarActions().FirstOrDefault(p=>p.Key==binding.Provider);if(provider==null)return;
        try
        {
            if(provider.Action.Invoke!=null)provider.Action.Invoke();
            else SendTypedChat(provider.Action.Command);
        }
        catch(Exception e){controllerMessage=e.GetBaseException().Message;Error(e,"sidebar action");}
    }
    private void RenderSidebar()
    {
        var buttons=layout.SidebarButtons.Where(b=>b!=null&&b.Enabled&&BindingAllowed(b)).ToArray();
        sidebarRect=LayoutRect("sidebar",88,Math.Min(600,48+buttons.Length*52),0,.45f,76,104);
        var rect=sidebarRect;WindowSurface(rect);PlaceWindow("sidebar",rect,rect.width-30,76,104);
        GUI.Label(new Rect(rect.x+7,rect.y+4,rect.width-36,24),"=",smallStyle);
        if(Button(new Rect(rect.xMax-29,rect.y+3,24,25),"+",description:"Configure sidebar buttons.",style:tabStyle))OpenSidebarMenu();
        var view=new Rect(rect.x+5,rect.y+34,rect.width-10,rect.height-44);
        sidebarScroll=GUI.BeginScrollView(view,sidebarScroll,new Rect(0,0,view.width-20,Math.Max(48,buttons.Length*52)),false,true);
        try
        {
            for(int i=0;i<buttons.Length;i++)
            {
                var b=buttons[i];var button=new Rect(0,i*52,view.width-22,46);bool usable=CanUseSidebar(b);
                if(Button(button,"",allowed:usable,description:b.Label+(usable?"":" (unavailable)"))){RunSidebar(b);break;}
                float size=Math.Min(38,button.width-4);
                DrawSymbol(new Rect((button.width-size)/2,button.y+(46-size)/2,size,size),b.Symbol,usable?ReadColor(b.Color):new Color(.4f,.4f,.4f,1));
            }
            if(buttons.Length==0)GUI.Label(new Rect(0,0,view.width-22,48),"+",tabStyle);
        }
        finally{GUI.EndScrollView();}
    }
    private void BeginSidebarEdit(SidebarBinding b=null,ProviderSidebar provider=null)
    {
        editingSidebar=true;editingSidebarId=b?.Id??"";
        sideProvider=b?.Provider??provider?.Key??"";sideMod=b?.Mod??(provider==null?"Custom":LocalTitle(provider.Owner));
        sideLabel=b?.Label??provider?.Action.Label??"";sideCommand=b?.Command??"";
        sideSymbol=b?.Symbol??provider?.Action.Symbol??"shape:circle-filled";
        if(provider!=null&&!SymbolCatalog().Any(s=>s.Id==sideSymbol))sideSymbol=LocalScope(provider.Owner)+":symbol:"+sideSymbol;
        sideColor=b?.Color??provider?.Action.Color??"#FFFFFF";sideRole=b?.Role??provider?.Action.Role??"shared";
        sideAdmin=b?.AdminOnly??provider?.Action.AdminOnly??false;sidebarMenuScroll=Vector2.zero;
    }
    private void SaveSidebarBinding()
    {
        if(sideLabel.Trim().Length==0||sideProvider.Length==0&&!ValidChat(sideCommand)||!ValidSidebarColor(sideColor))return;
        sideColor="#"+sideColor.TrimStart('#').ToUpperInvariant();
        var existing=layout.SidebarButtons.FirstOrDefault(b=>b.Id==editingSidebarId);
        var value=new SidebarBinding{Id=existing?.Id??Guid.NewGuid().ToString("N"),Kind=sideProvider.Length==0?"chat":"provider",
            Provider=sideProvider,Mod=sideMod,Label=sideLabel.Trim(),Command=sideCommand,Symbol=sideSymbol,Color=sideColor,Role=sideRole,AdminOnly=sideAdmin,Enabled=existing?.Enabled??true};
        if(existing!=null)layout.SidebarButtons.Remove(existing);
        layout.SidebarButtons.Add(value);placementDirty=true;SaveWindowPosition();ExportCreatedCommands();editingSidebar=false;
    }
    private void RenderSidebarMenu(Rect view)
    {
        GUI.Label(new Rect(view.x,view.y,view.width-120,32),"SIDEBAR",titleStyle);
        if(Button(new Rect(view.xMax-112,view.y,112,32),"BACK",style:tabStyle)){sidebarMenuOpen=false;editingSidebar=false;return;}
        if(editingSidebar){RenderSidebarEditor(new Rect(view.x,view.y+44,view.width,view.height-44));return;}
        if(Button(new Rect(view.x,view.y+44,view.width,36),(layout.SidebarEnabled?"[x] ":"[ ] ")+"Enable persistent sidebar",layout.SidebarEnabled))
        {layout.SidebarEnabled=!layout.SidebarEnabled;placementDirty=true;SaveWindowPosition();}
        if(Button(new Rect(view.x,view.y+92,view.width,36),"ADD CHAT BUTTON"))BeginSidebarEdit();
        var region=new Rect(view.x,view.y+140,view.width,view.height-140);
        var bindings=layout.SidebarButtons.Where(BindingAllowed).ToArray();var providers=ProviderSidebarActions().ToArray();
        float width=region.width-24,top=0;
        sidebarMenuScroll=GUI.BeginScrollView(region,sidebarMenuScroll,new Rect(0,0,width,bindings.Length*74+providers.Length*58+90),false,true);
        try
        {
            foreach(var b in bindings)
            {
                DrawSymbol(new Rect(4,top+6,40,40),b.Symbol,ReadColor(b.Color));
                GUI.Label(new Rect(54,top+4,width-340,56),b.Label+"\n"+(b.Kind=="chat"?b.Command:ModLabel(b.Mod)),smallStyle);
                if(Button(new Rect(width-278,top+6,84,32),b.Enabled?"[x] ON":"[ ] OFF",b.Enabled))
                {b.Enabled=!b.Enabled;placementDirty=true;SaveWindowPosition();}
                if(Button(new Rect(width-188,top+6,84,32),"EDIT",style:tabStyle)){BeginSidebarEdit(b);break;}
                if(Button(new Rect(width-98,top+6,92,32),"REMOVE",style:tabStyle))
                {layout.SidebarButtons.Remove(b);placementDirty=true;SaveWindowPosition();break;}
                top+=74;
            }
            GUI.Label(new Rect(4,top,width-8,38),"MOD SUPPORT",titleStyle);top+=46;
            foreach(var p in providers)
            {
                if(Button(new Rect(0,top,width,46),LocalTitle(p.Owner)+" / "+p.Action.Label,description:p.Action.Description))
                {BeginSidebarEdit(provider:p);break;}top+=58;
            }
            if(providers.Length==0)GUI.Label(new Rect(4,top,width-8,40),"No loaded mod has supplied sidebar actions.",smallStyle);
        }
        finally{GUI.EndScrollView();}
    }
    private void RenderSidebarEditor(Rect view)
    {
        var symbols=SymbolCatalog().ToArray();int columns=Math.Max(3,(int)((view.width-24)/56));
        float width=view.width-24,top=0;
        int symbolRows=(symbols.Length+columns-1)/columns;
        sidebarMenuScroll=GUI.BeginScrollView(view,sidebarMenuScroll,new Rect(0,0,width,510+symbolRows*52),false,true);
        try
        {
            GUI.Label(new Rect(0,top,100,30),"Label",smallStyle);
            sideLabel=GUI.TextField(new Rect(106,top,width-106,34),sideLabel,80,inputStyle);top+=44;
            GUI.Label(new Rect(0,top,100,30),sideProvider.Length==0?"Chat command":"Mod action",smallStyle);
            if(sideProvider.Length==0)sideCommand=GUI.TextField(new Rect(106,top,width-106,34),sideCommand,160,inputStyle);
            else GUI.Label(new Rect(106,top,width-106,38),ModLabel(sideMod),bodyStyle);top+=48;
            float roleX=0;
            foreach(string role in new[]{"shared","player","commander","admin"})
            {
                if(role=="admin"&&!CurrentAdmin)continue;
                if(Tab(new Rect(roleX,top,112,32),role.ToUpperInvariant(),sideRole==role))sideRole=role;roleX+=120;
            }
            top+=44;
            if(CurrentAdmin&&Button(new Rect(0,top,width,32),sideAdmin?"[x] Admin only":"[ ] Admin only",sideAdmin))sideAdmin=!sideAdmin;
            top+=42;
            GUI.Label(new Rect(0,top,width,28),"COLOR",smallStyle);top+=32;
            string[] colors={"#FFFFFF","#FF4040","#FF9000","#FFD840","#80EE50","#20B070","#40DFFF","#4080FF","#A060FF","#FF60BD","#A0A0A0","#202020"};
            float swatchWidth=Math.Min(52,(width-11*4)/12);
            for(int i=0;i<colors.Length;i++)
            {
                var r=new Rect(i*(swatchWidth+4),top,swatchWidth,32);
                if(Button(r,sideColor==colors[i]?"x":"",sideColor==colors[i]))sideColor=colors[i];
                Surface(new Rect(r.x+7,r.y+8,Math.Max(5,r.width-14),16),ReadColor(colors[i]));
            }
            top+=44;
            GUI.Label(new Rect(0,top,104,30),"Hex color",smallStyle);
            sideColor=GUI.TextField(new Rect(106,top,150,32),sideColor,7,inputStyle);
            DrawSymbol(new Rect(width-48,top-4,40,40),sideSymbol,ReadColor(sideColor));top+=48;
            GUI.Label(new Rect(0,top,width,28),"SYMBOL CATALOG",smallStyle);top+=34;
            for(int i=0;i<symbols.Length;i++)
            {
                int col=i%columns,row=i/columns;var r=new Rect(col*56,top+row*52,50,46);
                if(Button(r,"",sideSymbol==symbols[i].Id,description:symbols[i].Label))sideSymbol=symbols[i].Id;
                DrawSymbol(new Rect(r.x+5,r.y+3,40,40),symbols[i].Id,ReadColor(sideColor));
            }
            top+=symbolRows*52+16;
            if(Button(new Rect(0,top,180,38),"SAVE BUTTON",allowed:sideLabel.Trim().Length>0&&ValidSidebarColor(sideColor)&&(sideProvider.Length>0||ValidChat(sideCommand))))SaveSidebarBinding();
            if(Button(new Rect(192,top,130,38),"CANCEL"))editingSidebar=false;
        }
        finally{GUI.EndScrollView();}
    }
    private static bool ValidSidebarColor(string text)=>System.Text.RegularExpressions.Regex.IsMatch(text??"",@"^#?[0-9A-Fa-f]{6}$");
}

