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
        }
        internal static void Shutdown(){hooks?.UnpatchSelf();hooks=null;origins.Clear();}
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
