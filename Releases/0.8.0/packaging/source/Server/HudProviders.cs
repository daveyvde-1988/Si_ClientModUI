using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace SiSquadHud
{
    internal static class HudProviders
    {
        private static MethodInfo ranks, limit, policy;
        private static PropertyInfo formationEnabled;
        private static MethodInfo walletBalance;
        private static PropertyInfo walletActive;
        private static float nextBind;
        private static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name, false)).FirstOrDefault(t => t != null);
        internal static HudStatus Read(Player player)
        {
            if (UnityEngine.Time.realtimeSinceStartup >= nextBind)
            {
                nextBind = UnityEngine.Time.realtimeSinceStartup + 10;
                ranks = Find("SiPlayerRanks.RankMod")?.GetMethod("GetSquadHudStatus", new[] { typeof(ulong) });
                var formation = Find("Si_Formation.FormationApi");
                limit = formation?.GetMethod("TryGetPlayerLimit"); policy = formation?.GetMethod("TryGetPlayerPolicy");
                formationEnabled = formation?.GetProperty("GlobalEnabled");
                var wallet=Find("Si_KingOfTheHill.KingOfTheHill");
                walletBalance=wallet?.GetMethod("GetCreditsBySteamId",new[]{typeof(long)});
                walletActive=wallet?.GetProperty("ModeBuyActive");
            }
            var status = new HudStatus { Team = player.Team.Index, Commander = player.IsCommander, Score=player.Score };
            ulong id = player.PlayerID.SteamID.m_SteamID;
            try
            {
                if(walletBalance!=null&&(walletActive?.GetValue(null) as bool? ?? false))
                {int balance=(int)walletBalance.Invoke(null,new object[]{unchecked((long)id)});if(balance>=0)status.Credits=balance;}
            }
            catch { status.Credits=null; }
            var rank = ranks?.Invoke(null, new object[] { id }) as IReadOnlyDictionary<string, string>;
            if (rank != null && rank["schema"] == "1" && rank["team"] == status.Team.ToString(CultureInfo.InvariantCulture))
            {
                status.Rank = Clip(rank["title"], 96); status.Round = rank["round"];
                status.RankCap = int.Parse(rank["cap"], CultureInfo.InvariantCulture);
            }
            // Cap comes from Formation's effective result, including admin overrides/native fallback.
            object[] capArgs = { id, 0 };
            if (limit != null && limit.Invoke(null, capArgs)?.ToString() == "Success") status.EffectiveCap = (int)capArgs[1];
            bool running = formationEnabled?.GetValue(null) as bool? ?? false;
            if (!running)
            {
                if (status.EffectiveCap < 1 && FPSCommanding.Instance) status.EffectiveCap = FPSCommanding.Instance.GroupUnitLimit;
                status.Source = status.EffectiveCap > 0 ? "native" : "unavailable";
                status.Recruitment = "Native game rules (rank categories not enforced)";
            }
            else
            {
                object[] args = { id, null, 0, null, null, null };
                string result = policy?.Invoke(null, args)?.ToString() ?? "Unavailable";
                if (result == "AdminOverride")
                {
                    status.Source = "admin-override"; status.Recruitment = "Any native-recruitable unit (admin override)";
                }
                else if (result == "Success")
                {
                    status.Source = (string)args[1] == "SilicaFieldRanks" ? "rank-policy" : "custom-policy";
                    var categories = args[3] as string[];
                    var denied = args[5] as string[] ?? Array.Empty<string>();
                    var allowed = (args[4] as string[] ?? Array.Empty<string>()).Where(n => !denied.Contains(n)).ToArray();
                    status.Recruitment = categories == null || categories.Contains("*") ? "Any native-recruitable category" :
                        categories.Length == 0 ? (allowed.Length == 0 ? "Player only; no AI recruits" : "Specific units") : string.Join(", ", categories.Select(Category));
                    if (allowed.Length > 0 && categories != null && !categories.Contains("*"))
                        status.Recruitment += (categories.Length == 0 ? ": " : "; plus ") + string.Join(", ", allowed.Select(AllowedUnitName).Distinct());
                    status.Exclusions = string.Join(", ", denied.Select(UnitName).Distinct());
                }
                else if (result == "NoPolicy")
                { status.Source = "custom-policy"; status.Recruitment = "Native-recruitable categories (no owner policy)"; }
            }
            // External recruitment rules remain active even when rank policy is shadowed.
            var rankType = Find("SiPlayerRanks.RankMod");
            bool ranksBan = rankType?.GetProperty("HarvesterRecruitmentBanEnabled")?.GetValue(null) as bool? ?? Find("SiPlayerRanks.HarvesterRestrictions") != null;
            var antiGrief = Find("Si_AntiGrief.AntiGrief");
            bool shrimpBan = ranksBan || Preference(antiGrief,"_BlockShrimpFPSCommanding");
            bool harvesterBan = ranksBan || Preference(antiGrief,"_BlockHarvesterFPSCommanding");
            string externalBan = shrimpBan && harvesterBan ? "Harvesters/Shrimp" : shrimpBan ? "Shrimp" : harvesterBan ? "Harvesters" : "";
            if (externalBan.Length > 0 && !status.Exclusions.Contains(externalBan))
                status.Exclusions = externalBan + (status.Exclusions.Length == 0 ? "" : ", " + status.Exclusions);
            if (status.Commander) status.Recruitment = "No FPS squad while commanding";
            status.Recruitment = Clip(status.Recruitment, 256); status.Exclusions = Clip(status.Exclusions, 192);
            if (status.EffectiveCap < 1) status.EffectiveCap = -1;
            return status;
        }
        private static string Category(string rule)
        {
            if (rule.EndsWith(":*")) rule = rule.Substring(0, rule.Length - 2);
            switch (rule) {
                case "Special:Light": return "Light special units";
                case "Tank": return "Tanks";
                case "Transport": return "Transports";
                case "Repair": return "Repair units";
                case "None": return "Unclassified units";
                default: return rule.Replace(":", " / ");
            }
        }
        private static bool Preference(Type type,string name)
        {
            var entry=type?.GetField(name,BindingFlags.NonPublic|BindingFlags.Static)?.GetValue(null);
            return entry?.GetType().GetProperty("Value")?.GetValue(entry) as bool? ?? false;
        }
        private static string UnitName(string name)
        {
            if (name.Contains("Harvester") || name.Contains("Shrimp")) return "Harvesters/Shrimp";
            if (name == "ObjectInfo_Sol_UltraHeavy_SiegeTank") return "Siege Tank";
            if (name == "ObjectInfo_Cent_UltraHeavy_CrimsonTank") return "Crimson Tank";
            if (name == "ObjectInfo_Sol_Air_Dropship") return "Dropship";
            if (name == "ObjectInfo_Cent_Air_Shuttle") return "Shuttle";
            return "Other configured unit exclusions";
        }
        private static string AllowedUnitName(string name)
        {
            var label = UnitName(name);
            return label == "Other configured unit exclusions" ? "other configured units" : label;
        }
        private static string Clip(string text, int length)
        {
            text = (text ?? "").Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
            return text.Length <= length ? text : text.Substring(0, length - 3) + "...";
        }
    }
}
