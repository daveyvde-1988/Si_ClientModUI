using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace SiSquadHud
{
    internal static class PanelBackend
    {
        private static Type Find()=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Si_Formation.FormationApi",false)).FirstOrDefault(t=>t!=null);
        internal static PanelPage Handle(Player player,PanelRequest request)
        {
            if(!HudProtocol.IsFormation(request.Function))return MenuCatalog.Handle(player,request);
            var response=new PanelPage{Nonce=request.Nonce,Id=request.Id,Function=request.Function,Team=request.Team,Commander=request.Commander,Admin=MenuCatalog.IsAdmin(player),
                Message="Formation service unavailable. Update the server Formation mod."};
            var type=Find();var read=type?.GetMethod("GetPlayerPanelPage");var apply=type?.GetMethod("RequestPlayerPanelAction");
            if(read==null||apply==null)return response;
            ulong id=player.PlayerID.SteamID.m_SteamID;
            string message="";
            if(request.IsAction)
                message=apply.Invoke(null,new object[]{id,request.Function,request.Option,request.Action,request.Revision}) as string??"No action result.";
            if(request.Action=="lock")message=message.Replace("Facing lock","Locked formation");
            var data=read.Invoke(null,new object[]{id,request.Function,request.Page==HudProtocol.FullCatalog?0:request.Page}) as IReadOnlyDictionary<string,object>;
            if(data==null)return response;
            if(data.ContainsKey("error")){response.Message=HudProtocol.ClipUtf8(data["error"] as string,120);return response;}
            response.Page=(int)data["page"];response.Pages=(int)data["pages"];response.Selected=(int)data["selected"];
            response.Locked=(bool)data["locked"];response.Enabled=(bool)data["enabled"];response.Leader=(bool)data["leader"];
            response.Revision=(string)data["revision"];response.Numbers=(int[])data["numbers"];
            response.Names=((string[])data["names"]).Select(n=>HudProtocol.ClipUtf8(n,96)).ToArray();
            if(request.Page==HudProtocol.FullCatalog)
            {
                var numbers=response.Numbers.ToList();var names=response.Names.ToList();
                for(int i=1;i<response.Pages;i++)
                {
                    var more=read.Invoke(null,new object[]{id,request.Function,i}) as IReadOnlyDictionary<string,object>;
                    if(more==null||more.ContainsKey("error")||(string)more["revision"]!=response.Revision)throw new InvalidOperationException("Formation list changed during transfer.");
                    numbers.AddRange((int[])more["numbers"]);names.AddRange(((string[])more["names"]).Select(n=>HudProtocol.ClipUtf8(n,96)));
                    if(numbers.Count>HudProtocol.MaxCatalogRows)throw new InvalidOperationException("Formation list exceeds catalog limit.");
                }
                response.Numbers=numbers.ToArray();response.Names=names.ToArray();response.Page=0;response.Pages=1;
                if(request.Commander&&data.TryGetValue("commanderLock",out var supported)&&supported is bool available&&available)
                {
                    if(response.Numbers.Length>=HudProtocol.MaxCatalogRows)throw new InvalidOperationException("Formation list has no room for its lock control.");
                    // A distinct UI row, not a catalog selection or reserved formation number.
                    int rowNumber=Enumerable.Range(1,9999).Reverse().First(n=>!numbers.Contains(n));
                    response.Groups=new string[response.Numbers.Length+1];
                    for(int i=0;i<response.Groups.Length;i++)response.Groups[i]=i==response.Groups.Length-1?"commander-lock":"";
                    response.Numbers=response.Numbers.Concat(new[]{rowNumber}).ToArray();
                    response.Names=response.Names.Concat(new[]{"Lock formation"}).ToArray();
                }
            }
            response.Available=true;response.Message=HudProtocol.ClipUtf8(message,120);
            return response;
        }
    }
}
