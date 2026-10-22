using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader;
using Newtonsoft.Json;

namespace SiSquadHud
{
    // Sidecar the server admin edits: display names, categories, priorities, labels, hidden commands, match-control list.
    // Discovered mods are appended with defaults on every frame build so the file is always complete; user edits are kept.
    internal sealed class MenuConfig
    {
        public sealed class ModConfig { public string Name; public string Category = "player"; public int Priority = 100; public string Nav = "/"; }
        // Mod: force the owning mod id. Arg: what the client offers as a picker for the first argument ("player", "map", "" = free text).
        public sealed class CommandConfig { public string Label = null; public string Description = null; public string Mod; public string Arg; }
        private static readonly Dictionary<string, string> SeedArg = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "!kick", "player" }, { "!kickban", "player" }, { "!ban", "player" }, { "!mute", "player" }, { "!unmute", "player" }, { "!silence", "player" }, { "!unsilence", "player" },
            { "!afk", "player" }, { "!eject", "player" }, { "!demote", "player" }, { "!cban", "player" }, { "!commanderban", "player" }, { "!cmdrban", "player" }, { "!removecommanderban", "player" },
            { "!uncban", "player" }, { "!commander", "player" }, { "!team", "player" }, { "!slay", "player" }, { "!promote", "player" }, { "!transfer", "player" }, { "!map", "map" }, { "!unban", "ban" }
        };
        internal string ArgType(string commandText)
        {
            Commands.TryGetValue(commandText, out var cc);
            if (cc != null && cc.Arg != null) return cc.Arg;
            if (!SeedArg.TryGetValue(commandText, out var seeded)) return "";
            if (cc == null) { cc = new CommandConfig(); Commands[commandText] = cc; }
            cc.Arg = seeded; dirty = true; return seeded;
        }

        public bool Debug = false;
        public Dictionary<string, string> Categories = new Dictionary<string, string> { { "player", "Player" }, { "info", "Info" }, { "admin", "Admin" }, { "match", "Match control" } };
        public List<string> MatchControl = new List<string> { "map", "nextmap", "endround", "endgame", "kick", "kickban", "ban", "banid", "unban", "mute", "unmute", "silence", "unsilence", "demote", "commander", "cban", "removecommanderban", "mutiny", "afk", "eject" };
        // Formation registers its admin tuning as player commands and checks power inside the handler; keep them out of the player menu.
        public List<string> Hidden = new List<string> { "/formationsize", "/formationsizeglobal", "/formationsizecommander", "/Formationspeedcap", "/Formationtraveldistance", "/formationTravel", "/formationTolerance", "/FormationFallof", "/startdelayspeedlock", "/startdelaysoftlock", "/startdelaylock", "/startdelay", "/hudtest" };
        public Dictionary<string, ModConfig> Mods = new Dictionary<string, ModConfig>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, CommandConfig> Commands = new Dictionary<string, CommandConfig>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, int> SeedPriority = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        { { "Si_Buy", 10 }, { "Si_KingOfTheHill", 11 }, { "Si_Mapcycle", 20 }, { "Si_Leaderboard", 30 }, { "Si_EloMatchmaking", 31 }, { "Si_HeavyTeleporter", 80 }, { "Si_CrabCannon", 81 } };
        private static readonly Dictionary<string, string> SeedName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        { { "Si_Buy", "Buy" }, { "Si_KingOfTheHill", "King of the Hill" }, { "Si_Mapcycle", "Map voting" }, { "Si_Leaderboard", "Leaderboard" }, { "Si_EloMatchmaking", "Elo matchmaking" }, { "Si_HeavyTeleporter", "Heavy teleporter" }, { "Si_CrabCannon", "Crab cannon" }, { "Si_AdminMod", "Admin mod" } };

        [JsonIgnore] private bool dirty;
        [JsonIgnore] private static DateTime loadedWrite;
        internal static MenuConfig Current { get; private set; } = new MenuConfig();
        internal static string Path => System.IO.Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, "cfg", "Si_ServerModUI", "menu.json");

        internal static void Load()
        {
            try
            {
                if (File.Exists(Path))
                {
                    var loaded = JsonConvert.DeserializeObject<MenuConfig>(File.ReadAllText(Path));
                    if (loaded != null) { Current = loaded; loadedWrite = File.GetLastWriteTimeUtc(Path); return; }
                }
                Current = new MenuConfig { dirty = true }; Current.SaveIfDirty();
            }
            catch (Exception error) { MelonLogger.Warning("menu.json unreadable, using defaults: " + error.Message); Current = new MenuConfig(); }
        }
        internal static bool ReloadIfChanged()
        {
            try
            {
                if (!File.Exists(Path) || File.GetLastWriteTimeUtc(Path) == loadedWrite) return false;
                Load(); return true;
            }
            catch { return false; }
        }
        // Known commands registered through DynamicMethod shims (no owner visible to reflection); recorded in menu.json so it can be edited.
        private static readonly Dictionary<string, string> SeedOwner = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        { { "/stats", "Si_UnitBalance" }, { "!b", "Si_UnitBalance" }, { "!rebalance", "Si_UnitBalance" } };
        internal string Attribute(string commandText, string fallback)
        {
            if (!SeedOwner.TryGetValue(commandText, out var owner)) return fallback;
            if (!Commands.TryGetValue(commandText, out var cc)) { cc = new CommandConfig(); Commands[commandText] = cc; }
            if (string.IsNullOrEmpty(cc.Mod)) { cc.Mod = owner; dirty = true; }
            return cc.Mod;
        }
        internal ModConfig Mod(string id, string defaultName)
        {
            if (Mods.TryGetValue(id, out var mod)) return mod;
            mod = new ModConfig { Name = SeedName.TryGetValue(id, out var seeded) ? seeded : defaultName, Priority = SeedPriority.TryGetValue(id, out int prio) ? prio : 100 };
            Mods[id] = mod; dirty = true; return mod;
        }
        internal void SaveIfDirty()
        {
            if (!dirty) return;
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
                File.WriteAllText(Path, JsonConvert.SerializeObject(this, Formatting.Indented));
                loadedWrite = File.GetLastWriteTimeUtc(Path); dirty = false;
            }
            catch (Exception error) { MelonLogger.Warning("menu.json not saved: " + error.Message); }
        }
    }
}
