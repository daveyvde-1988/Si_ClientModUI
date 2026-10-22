using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MelonLoader;
using Newtonsoft.Json;
using SilicaAdminMod;

namespace SiSquadHud
{
    // The static "mod frame": every chat command the Admin Mod registry knows, grouped by the mod that registered it,
    // filtered by what THIS player may execute. Admin commands never leave the server for players without the power.
    internal static class Frame
    {
        internal sealed class Cmd
        {
            [JsonProperty("t")] public string Text;          // what the client sends, e.g. "/buy", "rtv", "!kick"
            [JsonProperty("l")] public string Label;
            [JsonProperty("d")] public string Desc;
            [JsonProperty("a")] public bool Admin;
            [JsonProperty("p")] public string Power;         // required admin power name, "" for player commands
            [JsonProperty("x")] public List<string> Aliases = new List<string>();
            [JsonProperty("arg")] public string Arg = "";                      // "player" | "map" | "" : what the client offers as a picker
            [JsonProperty("ch")] public List<string[]> Choices = new List<string[]>();   // [value, label] pairs for arg == "map"
            [JsonIgnore] public MethodInfo Callback;
        }
        internal sealed class Mod
        {
            [JsonProperty("id")] public string Id;
            [JsonProperty("name")] public string Name;
            [JsonProperty("ver")] public string Version;
            [JsonProperty("cat")] public string Category;
            [JsonProperty("prio")] public int Priority;
            [JsonProperty("nav")] public string Nav;         // prefix for numbered menu options, e.g. "/" -> "/1"
            [JsonProperty("hash")] public string Hash;
            [JsonProperty("cmds")] public List<Cmd> Cmds = new List<Cmd>();
        }
        internal sealed class Result { public string Root; public Dictionary<string, Mod> Mods; }

        private static readonly Regex NavCommand = new Regex(@"^(?:\d{1,4}|back|exit|close|0)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Dictionary<string, Result> cache = new Dictionary<string, Result>();
        internal static void Invalidate() { cache.Clear(); cacheAt = -100; }
        private static float cacheAt = -100;

        internal static Result Build(Player player)
        {
            float now = UnityEngine.Time.realtimeSinceStartup;
            if (now - cacheAt > 2) { cache.Clear(); cacheAt = now; }
            bool admin = player.IsAdmin();
            Power powers = admin ? player.GetAdminPowers() : Power.None;
            string key = admin + ":" + powers;
            if (cache.TryGetValue(key, out var cached)) return cached;
            var result = BuildFor(admin, powers);
            cache[key] = result; return result;
        }

        internal static Result BuildPublic() => BuildFor(false, Power.None);
        private static Result BuildFor(bool admin, Power powers)
        {
            var cfg = MenuConfig.Current;
            var mods = new Dictionary<string, Mod>(StringComparer.OrdinalIgnoreCase);
            var byCallback = new Dictionary<MethodInfo, Cmd>();
            try
            {
                foreach (var c in PlayerMethods.PlayerCommands.ToArray()) Add(mods, byCallback, cfg, c.PlayerCommandCallback, "/" + c.CommandName, false, Power.None, "");
                foreach (var c in PlayerMethods.PlayerPhrases.ToArray()) Add(mods, byCallback, cfg, c.PlayerCommandCallback, c.CommandName, false, Power.None, "");
                if (admin)
                    foreach (var a in AdminMethods.AdminCommands.ToArray())
                        if (AdminMethods.PowerInPowers(a.AdminPower, powers))
                            Add(mods, byCallback, cfg, a.AdminCallback, "!" + a.AdminCommandText, true, a.AdminPower, a.AdminCommandDescription);
            }
            catch (Exception error) { MelonLogger.Warning("Frame build incomplete: " + error.GetBaseException().Message); }

            // Match control: a fixed admin section that gathers selected admin commands from any mod.
            if (admin)
            {
                var match = new Mod { Id = "_match", Name = cfg.Categories.TryGetValue("match", out var mn) ? mn : "Match control", Version = "", Category = "match", Priority = 0, Nav = "!" };
                foreach (string name in cfg.MatchControl)
                {
                    var found = mods.Values.SelectMany(m => m.Cmds).FirstOrDefault(c => c.Admin && (c.Text.Equals("!" + name, StringComparison.OrdinalIgnoreCase) || c.Aliases.Any(x => x.Equals("!" + name, StringComparison.OrdinalIgnoreCase))));
                    if (found != null && !match.Cmds.Contains(found)) match.Cmds.Add(found);
                }
                if (match.Cmds.Count > 0) mods[match.Id] = match;
            }
            foreach (var mod in mods.Values.ToArray())
            {
                mod.Cmds = mod.Cmds.OrderBy(c => c.Admin).ThenBy(c => c.Label, StringComparer.OrdinalIgnoreCase).ToList();
                if (mod.Cmds.Count == 0) { mods.Remove(mod.Id); continue; }
                mod.Hash = ""; mod.Hash = Protocol.ShortHash(JsonConvert.SerializeObject(mod, Formatting.None));
            }
            cfg.SaveIfDirty();
            string root = Protocol.ShortHash(string.Join("\n", mods.Values.OrderBy(m => m.Id, StringComparer.Ordinal).Select(m => m.Id + ":" + m.Hash)));
            return new Result { Root = root, Mods = mods };
        }

        private static void Add(Dictionary<string, Mod> mods, Dictionary<MethodInfo, Cmd> byCallback, MenuConfig cfg, HelperMethods.CommandCallback callback, string text, bool admin, Power power, string description)
        {
            if (callback == null || string.IsNullOrEmpty(text)) return;
            string bare = text.TrimStart('/', '!', '.');
            if (bare.Length == 0 || NavCommand.IsMatch(bare)) return;                       // numbered menu navigation / vote options are not catalog entries
            if (cfg.Hidden.Any(h => h.Equals(text, StringComparison.OrdinalIgnoreCase) || h.Equals(bare, StringComparison.OrdinalIgnoreCase))) return;
            var method = callback.Method;
            if (byCallback.TryGetValue(method, out var existing))
            {
                // same handler registered under another name (rtv / rockthevote, command + phrase): keep one entry, list aliases
                if (!existing.Text.Equals(text, StringComparison.OrdinalIgnoreCase) && !existing.Aliases.Contains(text)) existing.Aliases.Add(text);
                return;
            }
            var assembly = OwningAssembly(callback);
            string id = assembly?.GetName().Name ?? "unknown";
            if (id == "SilicaAdminMod") id = "Si_AdminMod";
            cfg.Commands.TryGetValue(text, out var cc);
            if (!string.IsNullOrEmpty(cc?.Mod)) id = cc.Mod;                                   // sidecar override
            else if (assembly == null || IsFramework(assembly)) id = cfg.Attribute(text, id);   // DynamicMethod shims cannot be attributed by reflection on Mono
            if (!mods.TryGetValue(id, out var mod))
            {
                var info = assembly != null && assembly.GetName().Name == id ? ModInfo(assembly) : ModInfo(id);
                var mc = cfg.Mod(id, info.Key ?? id);
                mod = new Mod { Id = id, Name = mc.Name ?? id, Version = info.Value ?? "", Category = string.IsNullOrEmpty(mc.Category) ? "player" : mc.Category, Priority = mc.Priority, Nav = string.IsNullOrEmpty(mc.Nav) ? "/" : mc.Nav };
                mods[id] = mod;
            }
            var cmd = new Cmd
            {
                Text = text, Admin = admin, Power = admin ? power.ToString() : "", Callback = method,
                Label = cc?.Label ?? text,
                Desc = cc?.Description ?? Plain(description ?? "")
            };
            if (admin && cmd.Desc.Length == 0) cmd.Desc = "Requires admin power " + power + ".";
            cmd.Arg = cfg.ArgType(text) ?? "";
            if (cmd.Arg == "map") cmd.Choices = MapChoices();
            else if (cmd.Arg == "ban") cmd.Choices = BanChoices();
            mod.Cmds.Add(cmd); byCallback[method] = cmd;
        }
        // The game's own ban list (serversettings.xml); the value is the Steam64 id that !unban accepts.
        private static List<string[]> BanChoices()
        {
            var result = new List<string[]>();
            try
            {
                var bans = NetworkServerSettings.Instance?.BannedPlayers;
                if (bans == null) return result;
                foreach (var ban in bans)
                {
                    if (ban == null) continue;
                    string id = ban.m_ID.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    result.Add(new[] { id, Plain(ban.m_Name ?? "?") + "  (" + id + ")" });
                }
            }
            catch (Exception error) { MelonLogger.Warning("Ban list unavailable: " + error.GetBaseException().Message); }
            return result.OrderBy(c => c[1], StringComparer.OrdinalIgnoreCase).ToList();
        }
        // Multiplayer maps the game knows; the value is the file name the map commands expect.
        private static List<string[]> MapChoices()
        {
            var result = new List<string[]>();
            try
            {
                var levels = GameDatabase.Database?.AllLevels;
                if (levels == null) return result;
                foreach (var level in levels)
                {
                    if (level == null || !level.Enabled || !level.IsMultiplayer || level.IsDeveloperOnly || string.IsNullOrEmpty(level.FileName)) continue;
                    string label = Plain(level.DisplayName);
                    result.Add(new[] { level.FileName, label.Length > 0 && label != level.FileName ? label + "  (" + level.FileName + ")" : level.FileName });
                }
            }
            catch (Exception error) { MelonLogger.Warning("Map list unavailable: " + error.GetBaseException().Message); }
            return result.OrderBy(c => c[1], StringComparer.OrdinalIgnoreCase).ToList();
        }

        // A callback created by reflection (Delegate.CreateDelegate on another delegate's Invoke) reports mscorlib
        // as the declaring assembly; the mod is the delegate's target, unwrapped as deep as needed.
        private static Assembly OwningAssembly(Delegate callback)
        {
            for (int depth = 0; callback != null && depth < 8; depth++)
            {
                var assembly = callback.Method?.DeclaringType?.Assembly;
                if (assembly != null && !IsFramework(assembly)) return assembly;
                // DynamicMethod shims (Unit Balance) carry no declaring type, but their owner module is the mod's.
                Assembly module = null;
                try { module = callback.Method?.Module?.Assembly; } catch { }
                if (module != null && !IsFramework(module)) return module;
                var target = callback.Target;
                if (target is Delegate inner) { callback = inner; continue; }
                if (target != null && !IsFramework(target.GetType().Assembly)) return target.GetType().Assembly;
                return assembly;
            }
            return null;
        }
        private static bool IsFramework(Assembly assembly)
        {
            string name = assembly.GetName().Name;
            return name == "mscorlib" || name == "netstandard" || name.StartsWith("System", StringComparison.Ordinal);
        }
        private static KeyValuePair<string, string> ModInfo(string assemblyName)
        {
            try
            {
                foreach (var melon in MelonMod.RegisteredMelons)
                    if (melon.MelonAssembly?.Assembly?.GetName().Name == assemblyName) return new KeyValuePair<string, string>(melon.Info.Name, melon.Info.Version);
            }
            catch { }
            return new KeyValuePair<string, string>(null, "");
        }
        private static KeyValuePair<string, string> ModInfo(Assembly assembly)
        {
            try
            {
                foreach (var melon in MelonMod.RegisteredMelons)
                    if (melon.MelonAssembly?.Assembly == assembly) return new KeyValuePair<string, string>(melon.Info.Name, melon.Info.Version);
            }
            catch { }
            return new KeyValuePair<string, string>(null, assembly?.GetName().Version?.ToString());
        }
        private static string Plain(string text) => Regex.Replace(text ?? "", "<[^>]*>", "").Replace('\n', ' ').Trim();

        // Everything the client does not have yet (by per-mod hash), plus ids it should drop.
        internal static string Diff(Result frame, Dictionary<string, string> known, bool same)
        {
            var cfg = MenuConfig.Current;
            if (same) return JsonConvert.SerializeObject(new { k = "frame", root = frame.Root, same = true }, Formatting.None);
            var changed = frame.Mods.Values.Where(m => !known.TryGetValue(m.Id, out var h) || h != m.Hash).OrderBy(m => m.Priority).ThenBy(m => m.Name, StringComparer.OrdinalIgnoreCase).ToList();
            var removed = known.Keys.Where(id => !frame.Mods.Values.Any(m => m.Id == id)).ToList();
            return JsonConvert.SerializeObject(new { k = "frame", root = frame.Root, full = known.Count == 0, cats = cfg.Categories, mods = changed, removed }, Formatting.None);
        }
    }
}
