using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SiSquadHud
{
    // Optional adapters: never execute arbitrary client-supplied commands or load config from disk.
    internal static class MenuCatalog
    {
        private const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance;
        private const string KohHash="5377B71A034E065DF76D5A3F595DE733DAF24D9A875DEF9D0C092BCCAAABF8BC";
        private static Type verifiedKoh;
        private static bool kohVerified;
        private static string epoch=Guid.NewGuid().ToString("N");
        private sealed class Row { internal string Key,Name,Detail,Group="",Data=""; internal object Choice; }
        internal static void Boundary(){epoch=Guid.NewGuid().ToString("N");}
        private static Type Find(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name,false)).FirstOrDefault(t=>t!=null);
        internal static object Read(object instance,string name)
        {
            if(instance==null)return null;
            var type=instance as Type??instance.GetType();var target=instance is Type?null:instance;
            return type.GetField(name,Flags)?.GetValue(target)??type.GetProperty(name,Flags)?.GetValue(target);
        }
        private static string Str(object instance,string name)=>Read(instance,name)?.ToString()??"";
        private static object Call(Type type,string name,params object[] args)=>type.GetMethod(name,Flags).Invoke(null,args);
        private static string Plain(string text)=>Regex.Replace(text??"","<[^>]*>","");
        internal static bool IsAdmin(Player p)
        {
            var type=Find("SilicaAdminMod.PlayerAdmin");var power=Find("SilicaAdminMod.Power");
            return type!=null&&power!=null&&(bool)Call(type,"CanAdminExecute",p,Enum.Parse(power,"Generic"),null);
        }
        private static bool CheckKoh(Type type)
        {
            if(type==verifiedKoh)return kohVerified;
            verifiedKoh=type;kohVerified=false;
            if(type==null)return false;
            using(var hash=SHA256.Create())using(var stream=System.IO.File.OpenRead(type.Assembly.Location))
                kohVerified=BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","")==KohHash;
            return kohVerified;
        }
        private static string BuyGate(Player p,Type type)
        {
            if(!CheckKoh(type))return "KoH buy adapter unavailable: installed DLL needs review.";
            if(!(Read(type,"ModeBuyActive") as bool? ?? false))return "KoH purchases are disabled on this server.";
            if(GameMode.CurrentGameMode==null||!GameMode.CurrentGameMode.GameBegun||GameMode.CurrentGameMode.GameOver)return "Purchases require an active round.";
            bool admin=IsAdmin(p);
            if((Read(Read(type,"Cfg"),"BuyAdminOnly") as bool? ?? false)&&!admin)return "KoH purchases are admin-only on this server.";
            if(admin&&(bool)Call(type,"IsUnitBalanceEditorOpen",unchecked((long)p.PlayerID.SteamID.m_SteamID)))return "Close the Unit Balance editor (/b) before purchasing.";
            return "";
        }
        internal static PanelPage Handle(Player player,PanelRequest request)
        {
            var response=new PanelPage{Nonce=request.Nonce,Id=request.Id,Function=request.Function,Team=request.Team,Commander=request.Commander,Admin=IsAdmin(player)};
            var rows=new List<Row>();string note="";Type koh=null;
            try
            {
                if(request.Function=="buy")
                {
                    koh=Find("Si_KingOfTheHill.KingOfTheHill");note=BuyGate(player,koh);
                    if(note.Length>0){response.Message=note;return response;}
                    rows=BuyRows(player,koh);note="Confirm a unit to buy. KoH checks cooldown, unlocks, credits and spawn distance.";
                }
                else if(request.Function=="ranks")
                {rows=RankRows(player);note="Score thresholds use effective rank score; limited seats can delay promotion. Commanders have no FPS squad.";}
                else if(request.Function=="admin")
                {
                    if(!IsAdmin(player)){response.Message="Administrator Generic power required.";return response;}
                    rows=AdminRows();note="Live switches use the mod's own handler. Other mods show Loaded / no live switch.";
                }
                else rows=HelpRows(request.Function,out note);
                if(rows.Count==0){response.Message="No entries available from the active server mod.";return response;}
                string revision;
                revision=RevisionFor(rows,request);
                if(request.IsAction)
                {
                    if(request.Revision!=revision)note="List, state or round changed. Refresh and confirm again.";
                    else if(request.Option<1||request.Option>rows.Count)note="Unit no longer available.";
                    else if(request.Function=="admin"&&(request.Action=="enable"||request.Action=="disable"))
                    {
                        if(!IsAdmin(player))throw new InvalidOperationException("Administrator Generic power required.");
                        var item=rows[request.Option-1].Choice as AdminControls.Item;
                        if(item==null||!item.CanSwitch)note="This mod has no reviewed live switch.";
                        else
                        {
                            bool desired=request.Action=="enable";item.Set(player,desired);
                            rows=AdminRows();revision=RevisionFor(rows,request);
                            var actual=rows.Select(r=>r.Choice as AdminControls.Item).FirstOrDefault(i=>i?.Key==item.Key);
                            note=actual?.Enabled==desired?item.Name+" is "+(desired?"ON.":"OFF."):"Mod did not reach the requested state; inspect its chat/log response.";
                        }
                    }
                    else if(request.Function!="buy"||request.Action!="purchase")note="Unsupported menu action.";
                    else
                    {
                        // Constructed exclusively from the current server catalog; preserve KoH's actual purchase path.
                        var row=rows[request.Option-1];
                        long id=unchecked((long)player.PlayerID.SteamID.m_SteamID);
                        int before=(int)Call(koh,"GetCreditsBySteamId",id);
                        Call(koh,"TryPurchase",player,row.Choice);
                        int after=(int)Call(koh,"GetCreditsBySteamId",id);
                        note=after<before?"KoH processed the purchase. See chat for its receipt.":"No credit charge observed. Check KoH chat for the purchase result.";
                    }
                }
                response.Available=true;response.Revision=revision;response.Pages=request.Page==HudProtocol.FullCatalog?1:(rows.Count+2)/3;
                response.Page=request.Page==HudProtocol.FullCatalog?0:Math.Min(request.Page,response.Pages-1);
                int start=response.Page*3;
                var slice=rows.Skip(start).Take(request.Page==HudProtocol.FullCatalog?HudProtocol.MaxCatalogRows:3).ToArray();
                response.Numbers=Enumerable.Range(start+1,slice.Length).ToArray();
                response.Names=slice.Select(r=>HudProtocol.ClipUtf8(r.Name,96)).ToArray();
                response.Details=slice.Select(r=>HudProtocol.ClipUtf8(r.Detail,request.Page==HudProtocol.FullCatalog?400:200)).ToArray();
                response.Groups=slice.Select(r=>HudProtocol.ClipUtf8(r.Group,96)).ToArray();response.Data=slice.Select(r=>HudProtocol.ClipUtf8(r.Data,400)).ToArray();
                response.Message=HudProtocol.ClipUtf8(note,120);
                return response;
            }
            catch(Exception error)
            {
                response.Available=false;response.Message=HudProtocol.ClipUtf8("Menu unavailable: "+error.GetBaseException().Message,120);
                return response;
            }
        }
        private static string RevisionFor(List<Row> rows,PanelRequest request)
        {
            using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(epoch+"|"+request.Nonce+"|"+request.Team+"|"+request.Commander+"|"+request.Function+"|"+string.Join("\n",rows.Select(r=>r.Key+":"+r.Name+":"+r.Data))))).Replace("-","").ToLowerInvariant();
        }
        private static List<Row> AdminRows()=>AdminControls.List().Select(i=>new Row{Key=i.Key,Name=i.Name,Detail=i.Description,Group=i.Parent.Length>0?"child:"+i.Parent:i.Key=="Si_UnitBalance.UnitBalance"&&NativeAdminMenus.Available?"unitbalance-config":i.Key=="Si_StarterUnits.StarterUnits"&&StarterUnitsMenus.Available?"starter-config":"",Data=i.State+"|"+(i.CanSwitch?"1":"0"),Choice=i}).ToList();
        private static List<Row> BuyRows(Player player,Type type)
        {
            var factions=(Dictionary<string,string>)Read(type,"_unitFaction");
            var buildings=(Dictionary<string,string>)Read(type,"_unitToBuilding");
            var costs=(Dictionary<string,int>)Read(type,"_unitCost");
            var prefabs=(Dictionary<string,string>)Read(type,"_unitPrefab");
            var internals=(Dictionary<string,string>)Read(type,"_internalByDisplay");
            var structures=(HashSet<string>)Read(type,"_isStructure");
            string faction=(string)Call(type,"DumpFactionForTeam",player.Team);
            var choiceType=type.GetNestedType("BuyChoice",BindingFlags.NonPublic);
            var result=new List<Row>();
            foreach(var pair in factions.OrderBy(p=>buildings.TryGetValue(p.Key,out var b)?b:"").ThenBy(p=>costs.TryGetValue(p.Key,out var c)?c:0).ThenBy(p=>p.Key,StringComparer.Ordinal))
            {
                string name=pair.Key;
                if(!string.Equals(pair.Value,faction,StringComparison.OrdinalIgnoreCase)||structures.Contains(name)||
                    !buildings.TryGetValue(name,out var building)||string.IsNullOrEmpty(building)||!costs.TryGetValue(name,out int cost)||cost<=0||!(bool)Call(type,"CanResolveUnit",name))continue;
                var choice=Activator.CreateInstance(choiceType,true);
                choiceType.GetField("Display",Flags).SetValue(choice,name);
                choiceType.GetField("Cost",Flags).SetValue(choice,cost);
                choiceType.GetField("Prefab",Flags).SetValue(choice,prefabs.TryGetValue(name,out var prefab)?prefab:"");
                choiceType.GetField("ObjectInfoName",Flags).SetValue(choice,internals.TryGetValue(name,out var info)?info:"");
                choiceType.GetField("IsStructure",Flags).SetValue(choice,false);
                var cfg=Read(type,"Cfg");
                string detail=building+" | "+cost+" personal credits. Spawns near your controlled unit. Cooldown: "+Str(cfg,"BuyCooldownSeconds")+"s; own HQ/Nest range: "+Str(cfg,"BuyMaxDistanceFromOwnCritical")+"m.";
                if(name.IndexOf("shrimp",StringComparison.OrdinalIgnoreCase)>=0)detail+=" Shrimp unlock: "+Str(cfg,"ShrimpBuyUnlockSeconds")+"s into round.";
                result.Add(new Row{Key=name+":"+prefab+":"+info,Name=name+"  /  "+cost+" cr",Detail=detail,Group=building,Choice=choice});
            }
            return result;
        }
        private static List<Row> RankRows(Player player)
        {
            var engine=Read(Find("SiPlayerRanks.RankMod"),"engine");
            var cfg=Read(engine,"Config");var ranks=Read(cfg,"Ranks") as IEnumerable;
            var result=new List<Row>();if(ranks==null)return result;
            foreach(var tier in ranks)
            {
                string title=Str(tier,player.Team.name=="Team_Alien"?"AlienName":player.Team.name=="Team_Human_Centauri"?"CentauriName":"Name");
                if(title.Length==0)title=Str(tier,"Name");
                string seats=Str(tier,"Seats");
                string allowed=Str(tier,"Categories");
                var excluded=Read(tier,"ExcludedObjectInfoNames") as string[]??Array.Empty<string>();
                if(excluded.Length>0)allowed+=" except "+string.Join(", ",excluded.Select(n=>n.IndexOf("Crimson",StringComparison.OrdinalIgnoreCase)>=0?"Crimson Tank":n.IndexOf("Siege",StringComparison.OrdinalIgnoreCase)>=0?"Siege Tank":n));
                result.Add(new Row{Key=result.Count.ToString(),Name=title,Detail="",Data=string.Join("|",result.Count,Str(tier,"Score"),Str(tier,"Squad"),Str(tier,"BonusPercent")+"%",allowed,seats=="0"?"Unlimited":seats)});
            }
            string traitor=player.Team.name=="Team_Alien"?"Outcast":player.Team.name=="Team_Human_Centauri"?"Deserter":"Traitor";
            result.Add(new Row{Key="traitor",Name=traitor+" (traitor)",Data="-1|3 counted teamkills|1|See rules|None|N/A",
                Detail="Third counted teamkill enters this tier until next round; teamkills within "+Str(cfg,"TeamkillCooldownSeconds")+"s of a counted strike do not add another strike. First/second strikes demote 1/2 tiers. No AI recruits; combat/structure rewards are zero; other rewards "+Str(cfg,"TraitorOtherBonusPercent")+"%. Promotions locked until next round."});
            return result;
        }
        private static readonly Dictionary<string,string> Usage=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase){
            {"formation","/formation toggles menu; status reads state; FPS: /formation <function 1-3> <option>; commander: /formation <option>, or lock|unlock for snapshot orders."},
            {"followformation","/followformation lock|unlock|status - moving formation lock and speed coordination."},
            {"moveformation","/moveformation [number] - choose FPS move layout."},
            {"attackformation","/attackformation [number] - choose FPS attack staging layout."},
            {"commanderformation","/commanderformation [number] - choose commander move layout."},
            {"rank","/rank - current rank, score progress, squad allowance and rewards."},
            {"ranks","/ranks - rank information; permissions depend on the server."},
            {"buy","/buy [number] - category/unit shop; /back goes up, /0 closes. Spending uses personal credits."},
            {"koh","/koh [status] - King of the Hill capture and economy status."},
            {"stats","/stats - unit stats inspector; /1-N selects, /back goes up, /0 closes."},
            {"st","/st [number|status] - human heavy teleport menu; team tech, charges and cooldown apply."},
            {"ccaim","/ccaim [angle speed|range metres] - personal or commander cannon aim, when permitted."},
            {"disband","/disband all - dismiss AI squad followers; player-controlled units are retained."},
            {"logistics","/logistics - status; configuration/reload operations require admin permissions."},
            {"logi","Alias for /logistics - logistics status and permitted configuration operations."},
            {"scout","/scout - arm the commander's next scout order under Formation rules."}
        };
        private static readonly HashSet<string> CommanderOnly=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"scout"};
        private static readonly HashSet<string> FpsOnly=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"followformation","disband"};
        private static readonly HashSet<string> AdminPlayerCommands=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"formationsize","formationsizeglobal","formationsizecommander","Formationspeedcap","Formationtraveldistance","formationTravel","formationTolerance","FormationFallof","startdelayspeedlock","startdelaysoftlock","startdelaylock","startdelay"};
        private static List<Row> HelpRows(string function,out string note)
        {
            note="Registered server mod chat commands. Shared commands appear in both role lists. Admin permissions still apply.";
            var result=new Dictionary<string,Row>(StringComparer.OrdinalIgnoreCase);
            bool admin=function=="help-admin",commander=function=="help-commander";
            var registry=Read(Find(admin?"SilicaAdminMod.AdminMethods":"SilicaAdminMod.PlayerMethods"),admin?"AdminCommands":"PlayerCommands") as IEnumerable;
            if(registry==null)return new List<Row>();
            foreach(var command in registry)
            {
                string name=Str(command,admin?"AdminCommandText":"CommandName").TrimStart('/','!');
                if(name.Length==0||(!admin&&(AdminPlayerCommands.Contains(name)||(commander?FpsOnly:CommanderOnly).Contains(name))))continue;
                var callback=Read(command,admin?"AdminCallback":"PlayerCommandCallback") as Delegate;
                string origin=callback?.Method.DeclaringType?.Assembly.GetName().Name??"server mod";
                string detail=admin?Plain(Str(command,"AdminCommandDescription")):Usage.TryGetValue(name,out var usage)?usage:"Registered by "+origin+". No argument description is published by this command.";
                if(admin)detail+=" Required power: "+Str(command,"AdminPower")+".";
                result[name]=new Row{Key=name,Name=(admin?"!":"/")+name,Data=origin,Detail=detail};
            }
            if(admin)
            {
                // Formation registers some Generic-admin controls as player commands and checks inside the handler.
                var playerRegistry=Read(Find("SilicaAdminMod.PlayerMethods"),"PlayerCommands") as IEnumerable;
                if(playerRegistry!=null)foreach(var command in playerRegistry)
                {
                    string name=Str(command,"CommandName");if(!AdminPlayerCommands.Contains(name))continue;
                    result[name]=new Row{Key=name,Name="/"+name,Data="Si_Formation",Detail="Formation server configuration/control. Requires administrator Generic power; current handler validates arguments and scope."};
                }
            }
            // Ranks routes exact slash commands before AdminMod; these are not in its registry.
            if(Find("SiPlayerRanks.RankMod")!=null)
            {
                if(!admin)result["rank"]=new Row{Key="rank",Name="/rank [Player Name]",Data="Si_Ranks",Detail="Read rank, effective score progress, squad privileges and reward bonus. Quote player names containing spaces."};
                else foreach(var pair in new[]{("promote","/promote \"Player Name\" - promote one tier; Generic admin and target permissions required."),("demote","/demote \"Player Name\" - demote rank; Generic admin and target permissions required. Exact slash form is Ranks; !demote belongs to Commander Management."),("debug","/debug [on|off|status] - Ranks debug logging and adapter status; requires Generic admin power.")})
                    result["ranks-"+pair.Item1]=new Row{Key="ranks-"+pair.Item1,Name="/"+pair.Item1+" (Ranks)",Data="Si_Ranks",Detail=pair.Item2};
            }
            if(Find("Si_Formation.FormationApi")!=null)
            {
                void Add(string command,string explanation){result[command]=new Row{Key=command,Name=command,Data="Si_Formation",Detail=explanation};}
                if(!admin)
                {
                    Add("/formation help","Show commands for your current role and permissions.");
                    Add("/formation status","Global status and personal selections; commanders also see order capacity and pending lock/scout action.");
                    if(commander)
                    {
                        Add("/formation <number>","Select the commander formation by its displayed option number.");
                        Add("/formation lock","Arm one snapshot move for at least three eligible AI ground units; the next move supplies the selection and destination.");
                        Add("/formation unlock","Release your explicit locks and cancel a pending lock/scout action.");
                        Add("/formationTolerance <10..100>","Legacy compatibility setting; does not change current movement.");
                        Add("/FormationFallof <10..90>","Legacy compatibility setting; does not change current movement.");
                    }
                    else Add("/formation <function> <number>","Select a formation directly: function 1 = Move, 2 = Follow, 3 = Attack. Option 1 restores native behavior across personal FPS formations.");
                }
                else
                {
                    Add("/formation on | off","Match-wide master switch; next round restores configuration. Requires Generic power.");
                    Add("/formation rallyfix on|off|status","Automatic production parking; saved setting. Requires Generic power.");
                    Add("/formation debug on|off|status","Formation diagnostic logging for this match; requires Generic power.");
                    Add("/formationsizeglobal <1..50>","Match-wide FPS group allowance, including the controlled leader; Generic power.");
                    Add("/formationsize \"Player Name\" <1..50>","Specific player's FPS group allowance, including the leader; match-only, Generic power.");
                    Add("/formationsizecommander <1..100>","Saved eligible ground-unit cap per commander formation selection; no argument or status displays it.");
                    Add("/Formationtraveldistance <0..10000>","Saved maximum locked-formation centre travel in metres; 0 keeps its centre in place. Alias: /formationTravel.");
                    Add("/Formationspeedcap on|off","Final-arrival speed ceiling for this match; centre-based travel pacing remains active.");
                    Add("/startdelaysoftlock <seconds>","Soft-lock batch departure interval; 0 disables, otherwise 0.1..5 seconds, match-only.");
                    Add("/startdelayspeedlock <seconds>","Hard-lock speed-tier departure interval; 0 disables, otherwise 0.1..5. Takes priority over positional delay.");
                    Add("/startdelaylock <seconds>","Hard-lock positional row interval; 0 disables, otherwise 0.1..5. Disable speed-tier delay first. Alias: /startdelay lock.");
                }
            }
            foreach(var row in result.Values)if(row.Data.Length==0)row.Data="Unknown mod";
            return result.Values.Where(r=>!Regex.IsMatch(r.Name,@"^[/!](?:\d+|back|exit|close)(?:\s|$)",RegexOptions.IgnoreCase)).OrderBy(r=>r.Name,StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
