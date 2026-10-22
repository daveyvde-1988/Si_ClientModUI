using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using HarmonyLib;
using MelonLoader;

namespace SiSquadHud
{
    // Recipient-scoped interception: no message-prefix guessing and no player-chat hook.
    internal static class ModNotifications
    {
        private static HarmonyLib.Harmony hooks;
        private sealed class ChatScope { internal string Origin,Text; }
        [ThreadStatic] private static ChatScope nativeScope;
        [ThreadStatic] private static bool redirecting;
        private static readonly Dictionary<Assembly,string> origins=new Dictionary<Assembly,string>();
        internal static void Initialize()
        {
            foreach(var mod in MelonMod.RegisteredMelons)origins[mod.GetType().Assembly]=Path.GetFileNameWithoutExtension(mod.GetType().Assembly.Location).ToLowerInvariant();
            var assembly=origins.Keys.FirstOrDefault(a=>a.GetName().Name=="Si_AdminMod");if(assembly==null)return;
            using(var hash=SHA256.Create())using(var stream=File.OpenRead(assembly.Location))
                if(BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","")!="51F113348FA0FA30C200451EA4BDEDDFD0D7F65E40D15F13A9D007297C041432")return;
            var method=assembly.GetType("SilicaAdminMod.HelperMethods")?.GetMethod("NetworkSendChat",BindingFlags.NonPublic|BindingFlags.Static);
            if(method==null)return;
            hooks=new HarmonyLib.Harmony("SiSquadHUD.ModNotifications");
            hooks.Patch(method,prefix:new HarmonyMethod(typeof(ModNotifications).GetMethod(nameof(Route),BindingFlags.NonPublic|BindingFlags.Static)));
            using(var hash=SHA256.Create())using(var stream=File.OpenRead(typeof(NetworkLayer).Assembly.Location))
                if(BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","")!="33969F93EBBE60C4182E18C590974BCE046BD0F440B7003D888F2E7640B1BCF5")return;
            var flags=BindingFlags.NonPublic|BindingFlags.Static;
            hooks.Patch(AccessTools.Method(typeof(NetworkLayer),"SendChatMessage"),
                prefix:new HarmonyMethod(typeof(ModNotifications).GetMethod(nameof(BeginNativeChat),flags)),
                finalizer:new HarmonyMethod(typeof(ModNotifications).GetMethod(nameof(EndNativeChat),flags)));
            hooks.Patch(AccessTools.Method(typeof(NetworkLayer),"SendServerPacket"),
                prefix:new HarmonyMethod(typeof(ModNotifications).GetMethod(nameof(RouteNativeRecipient),flags)));
        }
        internal static void Shutdown(){hooks?.UnpatchSelf();hooks=null;origins.Clear();nativeScope=null;}
        private static string Origin()
        {
            foreach(var frame in new StackTrace(false).GetFrames()??Array.Empty<StackFrame>())
            {
                var assembly=frame.GetMethod()?.DeclaringType?.Assembly;
                if(assembly==null||assembly==typeof(ServerHud).Assembly||!origins.TryGetValue(assembly,out var candidate)||candidate=="si_adminmod")continue;
                return candidate;
            }
            return null;
        }
        private static void BeginNativeChat(string __2,bool __4,out ChatScope __state)
        {
            __state=nativeScope;nativeScope=null;
            try
            {
                if(!__4||!Game.GetIsServer()||Game.GetIsClient())return;
                string origin=Origin();if(origin!=null)nativeScope=new ChatScope{Origin=origin,Text=__2};
            }
            catch{nativeScope=null;}
        }
        private static Exception EndNativeChat(Exception __exception,ChatScope __state){nativeScope=__state;return __exception;}
        private static bool RouteNativeRecipient(NetworkID __0,int __4,ref bool __result)
        {
            if(nativeScope==null||redirecting)return true;
            try
            {
                redirecting=true;var player=Player.FindPlayer(__0,__4);
                if(!ServerHud.RedirectNotification(player,nativeScope.Origin,nativeScope.Text))return true;
                __result=true;return false;
            }
            catch{return true;}
            finally{redirecting=false;}
        }
        private static bool Route(Player __0,string[] __2)
        {
            try
            {
                if(!ServerHud.WantsNotifications(__0))return true;
                string origin=Origin()??"si_adminmod";
                return !ServerHud.RedirectNotification(__0,origin,string.Concat(__2??Array.Empty<string>()));
            }
            catch{return true;} // Native chat remains the fallback on any routing failure.
        }
    }
}
