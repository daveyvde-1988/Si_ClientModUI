using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SiSquadHud
{
    // Native enum values currently end at 42. Refuse to activate if 126 becomes defined.
    // Other mods using 126 are distinguished by the exact marker, not swallowed.
    public static partial class HudProtocol
    {
        public const byte PacketType = 126;
        public const string Marker = "SiSquadHUD/1";
        public const int MaxBody = 1800, MaxPacket = 2100;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        // Marker is ASCII with a single-byte native BinaryWriter string length.
        // Inspect raw bytes so another mod's packet 126 never triggers native type-error logging.
        public static bool ReadMarker(Func<int> readByte, byte stringTag)
        {
            if (readByte() != stringTag || readByte() != Marker.Length) return false;
            foreach (char c in Marker) if (readByte() != c) return false;
            return true;
        }
        public static bool ValidNonce(string value) => value != null && value.Length == 32 && value.All(c => c >= '0' && c <= '9' || c >= 'a' && c <= 'f');
        public static string Request(string nonce, int channel, int team)
        {
            if (!ValidNonce(nonce) || channel < 0 || channel > 255 || team < -1 || team > 255) throw new ArgumentException("Invalid HUD request.");
            return "Q4|" + nonce + "|" + channel.ToString(Inv) + "|" + team.ToString(Inv);
        }
        public static bool TryRequest(string text, out string nonce, out int channel, out int team)
        { int version; return TryRequest(text,out nonce,out channel,out team,out version); }
        public static bool TryRequest(string text, out string nonce, out int channel, out int team,out int version)
        {
            version=1;
            nonce = ""; channel = 0; team = -1;
            if (text == null || text.Length > 64) return false;
            var p = text.Split('|');
            if (p.Length != 4 || (p[0] != "Q"&&p[0]!="Q2"&&p[0]!="Q3"&&p[0]!="Q4") || !ValidNonce(p[1]) || !Number(p[2], 0, 255, out channel) || !Number(p[3], -1, 255, out team)) return false;
            nonce = p[1];version=p[0]=="Q4"?4:p[0]=="Q3"?3:p[0]=="Q2"?2:1; return true;
        }
        public static string Status(HudStatus value,bool extended=false)
        {
            if (!Valid(value)) throw new ArgumentException("Invalid HUD status.");
            var text = string.Join("|", extended?"S2":"S", value.Nonce, value.Round, value.Team.ToString(Inv), value.Sequence.ToString(Inv),
                value.RankCap.ToString(Inv), value.EffectiveCap.ToString(Inv), value.Commander ? "1" : "0",
                Pack(value.Rank), Pack(value.Recruitment), Pack(value.Exclusions), value.Source);
            if(extended)text+="|"+(value.Score?.ToString(Inv)??"?")+"|"+(value.Credits?.ToString(Inv)??"?");
            if (text.Length > MaxBody) throw new ArgumentException("HUD status exceeds packet budget.");
            return text;
        }
        public static bool TryStatus(string text, out HudStatus value)
        {
            value = null;
            if (text == null || text.Length > MaxBody) return false;
            var p = text.Split('|');
            if (!((p.Length==12&&p[0]=="S")||(p.Length==14&&p[0]=="S2"))) return false;
            try
            {
                int team, rankCap, effectiveCap;
                long sequence;
                if (!Number(p[3], -1, 255, out team) || !Number(p[5], -1, 1000, out rankCap) || !Number(p[6], -1, 1000, out effectiveCap) ||
                    !long.TryParse(p[4], NumberStyles.None, Inv, out sequence) || sequence < 0 || (p[7] != "0" && p[7] != "1")) return false;
                var candidate = new HudStatus {
                    Nonce = p[1], Round = p[2], Team = team, Sequence = sequence, RankCap = rankCap,
                    EffectiveCap = effectiveCap, Commander = p[7] == "1", Rank = Unpack(p[8]), Recruitment = Unpack(p[9]),
                    Exclusions = Unpack(p[10]), Source = p[11]
                };
                if(p[0]=="S2")
                {
                    int score,credits;
                    if(p[12]!="?"){if(!int.TryParse(p[12],NumberStyles.AllowLeadingSign,Inv,out score))return false;candidate.Score=score;}
                    if(p[13]!="?"){if(!Number(p[13],0,int.MaxValue,out credits))return false;candidate.Credits=credits;}
                }
                if (!Valid(candidate)) return false;
                value = candidate; return true;
            }
            catch (Exception error) when (error is FormatException || error is DecoderFallbackException || error is ArgumentException) { return false; }
        }
        private static bool Valid(HudStatus s) => s != null && ValidNonce(s.Nonce) && (s.Round == "-" || ValidNonce(s.Round)) &&
            s.Team >= -1 && s.Team <= 255 && s.Sequence >= 0 && Cap(s.RankCap) && Cap(s.EffectiveCap) &&
            Safe(s.Rank, 96) && Safe(s.Recruitment, 256) && Safe(s.Exclusions, 192) &&
            new[] { "rank-policy", "admin-override", "custom-policy", "native", "unavailable" }.Contains(s.Source);
        private static bool Cap(int n) => n == -1 || n >= 1 && n <= 1000;
        private static bool Safe(string s, int max) => s != null && s.Length <= max && !s.Any(char.IsControl);
        private static string Pack(string s) => Convert.ToBase64String(Utf8.GetBytes(s));
        private static string Unpack(string s) => Utf8.GetString(Convert.FromBase64String(s));
        private static bool Number(string s, int min, int max, out int value) => int.TryParse(s, NumberStyles.AllowLeadingSign, Inv, out value) && value >= min && value <= max;
    }

    public sealed class HudStatus
    {
        public string Nonce = "", Round = "-", Rank = "Rank unavailable", Recruitment = "Server rules unavailable", Exclusions = "", Source = "unavailable";
        public int Team = -1, RankCap = -1, EffectiveCap = -1;
        public long Sequence;
        public bool Commander;
        public int? Score,Credits;
    }

    // Cache scope is the connection, local Player instance, faction and scene lifecycle.
    public sealed class HudCache
    {
        public string Nonce { get; private set; } = Guid.NewGuid().ToString("N");
        public HudStatus Value { get; private set; }
        private string context = "";
        private double received;
        private long sequence = -1;
        public int Team { get; private set; } = -1;
        public bool SetContext(string peer, int player, int team)
        {
            var next = peer + ":" + player + ":" + team;
            if (context == next) return false;
            Clear(); context = next; Team = team; return true;
        }
        public void Clear() { context = ""; Nonce = Guid.NewGuid().ToString("N"); Value = null; sequence = -1; Team = -1; }
        public bool Accept(HudStatus value, double now)
        {
            if (value == null || value.Nonce != Nonce || value.Team != Team || value.Sequence <= sequence) return false;
            Value = value; sequence = value.Sequence; received = now; return true;
        }
        public HudStatus Fresh(double now) => Value != null && now - received <= 15 && now >= received ? Value : null;
    }
}
