using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using SiPlayerRanks;
using Silica.AI;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(RankMod), "Silica Field Ranks", "1.4.3", "Local", null)]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod", "Si_Formation" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_Ranks")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.4.3.0")]
[assembly: AssemblyInformationalVersion("1.4.3+fd8412f6219b457980e260c0349739e052af7db2")]
[assembly: AssemblyProduct("Si_Ranks")]
[assembly: AssemblyTitle("Si_Ranks")]
[assembly: AssemblyVersion("1.4.3.0")]
[module: RefSafetyRules(11)]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		public readonly byte[] NullableFlags;

		public NullableAttribute(byte P_0)
		{
			NullableFlags = new byte[1] { P_0 };
		}

		public NullableAttribute(byte[] P_0)
		{
			NullableFlags = P_0;
		}
	}
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableContextAttribute : Attribute
	{
		public readonly byte Flag;

		public NullableContextAttribute(byte P_0)
		{
			Flag = P_0;
		}
	}
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace SiPlayerRanks
{
	[Flags]
	public enum UnitCategories
	{
		None = 0,
		Infantry = 1,
		Light = 2,
		Heavy = 4,
		Air = 8,
		Siege = 0x10,
		Other = 0x20,
		Any = Infantry | Light | Heavy | Air | Siege | Other
	}
	public enum RewardKind
	{
		Kill,
		Structure,
		Logistics,
		Other
	}
	public enum Faction
	{
		Sol,
		Centauri,
		Alien
	}
	public sealed class RankedKillBonus
	{
		public string KillerName = "";

		public string KillerRank = "";

		public string VictimName = "";

		public string VictimRank = "";

		public int Multiplier;

		public long KillerId;

		public string RoundToken = "";

		public string Announcement(int credits)
		{
			return KillerRank + " " + KillerName + " killed higher ranking " + VictimRank + " " + VictimName + " and got a " + credits + "cr reward!!!";
		}
	}
	public sealed class RankSoundConfig
	{
		public bool Enabled = true;

		public string Tier1To4 = "Promotion_1.wav";

		public string Tier5To9 = "Promotion_2.wav";

		public string Tier10To14 = "Promotion_3.wav";

		public string Tier15To16 = "Promotion_4.wav";

		public string AlienTier1To4 = "Alien_Promotion_1.wav";

		public string AlienTier5To9 = "Alien_Promotion_2.wav";

		public string AlienTier10To14 = "Alien_Promotion_3.wav";

		public string AlienTier15To16 = "Alien_Promotion_4.wav";

		public string Demotion = "TK_1.wav";

		public string Traitor = "TK_2.wav";

		public string Promotion(int tier)
		{
			if (tier >= 1)
			{
				if (tier > 4)
				{
					if (tier > 9)
					{
						if (tier > 14)
						{
							return Tier15To16;
						}
						return Tier10To14;
					}
					return Tier5To9;
				}
				return Tier1To4;
			}
			return "";
		}

		public string Promotion(int tier, Faction faction)
		{
			if (faction == Faction.Alien)
			{
				if (tier >= 1)
				{
					if (tier > 4)
					{
						if (tier > 9)
						{
							if (tier > 14)
							{
								return AlienTier15To16;
							}
							return AlienTier10To14;
						}
						return AlienTier5To9;
					}
					return AlienTier1To4;
				}
				return "";
			}
			return Promotion(tier);
		}

		public IEnumerable<string> Files()
		{
			return new string[10] { Tier1To4, Tier5To9, Tier10To14, Tier15To16, AlienTier1To4, AlienTier5To9, AlienTier10To14, AlienTier15To16, Demotion, Traitor };
		}
	}
	public sealed class RankDefinition
	{
		public string Name = "";

		public string CentauriName = "";

		public string AlienName = "";

		public int Score;

		public int Squad;

		public int BonusPercent;

		public int Seats;

		public UnitCategories Categories;

		public string[] ExcludedObjectInfoNames = new string[0];

		public RankDefinition()
		{
		}

		public RankDefinition(string name, int score, int squad, int bonus, UnitCategories categories, int seats = 0)
		{
			Name = name;
			Score = score;
			Squad = squad;
			BonusPercent = bonus;
			Categories = categories;
			Seats = seats;
		}

		public string Display(Faction faction)
		{
			if (faction != Faction.Alien || !(AlienName != ""))
			{
				if (faction != Faction.Centauri || !(CentauriName != ""))
				{
					return Name;
				}
				return CentauriName;
			}
			return AlienName;
		}
	}
	public sealed class RankConfig
	{
		public bool Debug;

		public bool EnforceHarvesterRecruitmentBan = true;

		public double PollSeconds = 30.0;

		public double TeamkillCooldownSeconds = 10.0;

		public int StartingCommanderRank = 9;

		public int StartingCommanderScore = 3500;

		public int TraitorOtherBonusPercent = -25;

		public RankSoundConfig Sounds = new RankSoundConfig();

		public static readonly string[] ForbiddenRecruitObjects = new string[4] { "ObjectInfo_Sol_HoverHarvester", "ObjectInfo_Cent_HeavyHarvester", "ObjectInfo_Cent_HeavyHarvester_Intro", "ObjectInfo_Alien_Shrimp" };

		public Dictionary<string, string[]> NativeCategoryRules = DefaultNativeRules();

		public Dictionary<string, UnitCategories> NativeObjectCategoryOverrides = new Dictionary<string, UnitCategories>
		{
			{
				"ObjectInfo_Sol_Air_Dropship",
				UnitCategories.Air
			},
			{
				"ObjectInfo_Cent_Air_Shuttle",
				UnitCategories.Air
			}
		};

		public RankDefinition[] Ranks = Defaults();

		public string[] HarvesterRecruitmentExclusions()
		{
			if (!EnforceHarvesterRecruitmentBan)
			{
				return Array.Empty<string>();
			}
			return ForbiddenRecruitObjects;
		}

		public static Dictionary<string, string[]> DefaultNativeRules()
		{
			Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
			dictionary.Add("Infantry", new string[1] { "Infantry" });
			dictionary.Add("Light", new string[4] { "Cavalry", "Special:Light", "Transport", "Repair" });
			dictionary.Add("Heavy", new string[2] { "Tank", "Special" });
			dictionary.Add("Air", new string[1] { "Aircraft" });
			dictionary.Add("Siege", new string[1] { "Siege" });
			dictionary.Add("Other", new string[1] { "None" });
			return dictionary;
		}

		public static RankDefinition[] Defaults()
		{
			string[] array = new string[17]
			{
				"Private", "Specialist 1", "Specialist 2", "Specialist 3", "Specialist 4", "Sergeant", "Technical Sergeant", "Master Sergeant", "Senior Master Sergeant", "Chief Master Sergeant",
				"Second Lieutenant", "First Lieutenant", "Captain", "Major", "Lieutenant Colonel", "Colonel", "Field Marshal"
			};
			string[] array2 = new string[17]
			{
				"Recruit", "Private", "Private First Class", "Lance Corporal", "Corporal", "Sergeant", "Staff Sergeant", "Sergeant First Class", "Master Sergeant", "Sergeant Major",
				"Second Lieutenant", "First Lieutenant", "Captain", "Major", "Lieutenant Colonel", "Colonel", "Field Marshal"
			};
			string[] array3 = new string[17]
			{
				"Foetus", "Hatchling", "Stalker", "Hunter", "Ravager", "Predator", "Apex Predator", "Brood Warrior", "Brood Guardian", "Brood Warden",
				"Brood Champion", "Brood Leader", "Brood Master", "Brood Lord", "Hive Guardian", "Hive Lord", "Hive Mind"
			};
			int[] array4 = new int[17]
			{
				0, 300, 500, 700, 1000, 1500, 2000, 2500, 3000, 3500,
				4000, 4500, 5500, 7000, 9000, 12000, 16000
			};
			int[] array5 = new int[17]
			{
				0, 1, 2, 4, 6, 9, 12, 16, 20, 24,
				27, 30, 35, 40, 45, 50, 85
			};
			RankDefinition[] array6 = new RankDefinition[array4.Length];
			for (int i = 0; i < 17; i++)
			{
				UnitCategories categories = ((i != 0) ? ((i < 3) ? UnitCategories.Infantry : ((i < 5) ? (UnitCategories.Infantry | UnitCategories.Light) : ((i < 7) ? (UnitCategories.Infantry | UnitCategories.Light | UnitCategories.Heavy) : ((i == 7) ? (UnitCategories.Infantry | UnitCategories.Light | UnitCategories.Heavy | UnitCategories.Air) : UnitCategories.Any)))) : UnitCategories.None);
				array6[i] = new RankDefinition(array[i], array4[i], (i == 16) ? 20 : (i + 1), array5[i], categories, (i >= 10) ? 1 : 0)
				{
					CentauriName = array2[i],
					AlienName = array3[i]
				};
				if (i == 8 || i == 9)
				{
					array6[i].ExcludedObjectInfoNames = new string[2] { "ObjectInfo_Sol_UltraHeavy_SiegeTank", "ObjectInfo_Cent_UltraHeavy_CrimsonTank" };
				}
			}
			return array6;
		}

		public void Validate()
		{
			if (!Finite(PollSeconds) || PollSeconds < 1.0 || PollSeconds > 3600.0 || !Finite(TeamkillCooldownSeconds) || TeamkillCooldownSeconds < 0.0 || TeamkillCooldownSeconds > 3600.0)
			{
				throw new ArgumentException("Invalid polling or teamkill cooldown.");
			}
			if (TraitorOtherBonusPercent < -100 || TraitorOtherBonusPercent > 0)
			{
				throw new ArgumentException("Invalid reward modifiers.");
			}
			if (Sounds == null || Sounds.Files().Any((string s) => s == null || (s.Length > 0 && (s.IndexOfAny(new char[3] { '/', '\\', ':' }) >= 0 || s.Any(char.IsControl) || !s.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)))))
			{
				throw new ArgumentException("Invalid sound filenames; use WAV basenames in UserData/sounds.");
			}
			if (Ranks == null || Ranks.Length != 17)
			{
				throw new ArgumentException("Exactly seventeen rank tiers are required.");
			}
			if (NativeCategoryRules == null || new string[6] { "Infantry", "Light", "Heavy", "Air", "Siege", "Other" }.Any((string k) => !NativeCategoryRules.ContainsKey(k)) || NativeCategoryRules.Any((KeyValuePair<string, string[]> p) => p.Value == null || p.Value.Any((string r) => !ValidNativeRule(r))))
			{
				throw new ArgumentException("Invalid native category rules.");
			}
			if (NativeObjectCategoryOverrides == null || NativeObjectCategoryOverrides.Any((KeyValuePair<string, UnitCategories> p) => string.IsNullOrWhiteSpace(p.Key) || (p.Value & ~UnitCategories.Any) != 0))
			{
				throw new ArgumentException("Invalid native object category overrides.");
			}
			for (int num = 0; num < 17; num++)
			{
				RankDefinition rankDefinition = Ranks[num];
				if (rankDefinition == null || string.IsNullOrWhiteSpace(rankDefinition.Name) || rankDefinition.Score < 0 || (num > 0 && rankDefinition.Score <= Ranks[num - 1].Score) || rankDefinition.Squad < 0 || rankDefinition.Squad > 50 || rankDefinition.BonusPercent < 0 || rankDefinition.BonusPercent > 1000 || rankDefinition.Seats < 0 || (rankDefinition.Categories & ~UnitCategories.Any) != 0 || rankDefinition.ExcludedObjectInfoNames == null || rankDefinition.ExcludedObjectInfoNames.Any(string.IsNullOrWhiteSpace))
				{
					throw new ArgumentException("Invalid rank tier " + num);
				}
			}
			if (Ranks[0].Score != 0 || Ranks[0].Seats != 0)
			{
				throw new ArgumentException("Base tier must start at zero and have unlimited seats.");
			}
			if (StartingCommanderRank < 0 || StartingCommanderRank >= Ranks.Length || Ranks[StartingCommanderRank].Seats != 0 || StartingCommanderScore < 0)
			{
				throw new ArgumentException("Invalid starting commander tier or score award.");
			}
		}

		private static bool Finite(double v)
		{
			if (!double.IsNaN(v))
			{
				return !double.IsInfinity(v);
			}
			return false;
		}

		private static bool ValidNativeRule(string rule)
		{
			if (rule == "*")
			{
				return true;
			}
			if (string.IsNullOrWhiteSpace(rule))
			{
				return false;
			}
			string[] array = rule.Split(':');
			if (array.Length <= 2 && new string[10] { "None", "Infantry", "Cavalry", "Tank", "Siege", "Aircraft", "Transport", "Harvester", "Special", "Repair" }.Contains(array[0]))
			{
				if (array.Length != 1)
				{
					return new string[5] { "Light", "Medium", "Heavy", "Any", "*" }.Contains(array[1]);
				}
				return true;
			}
			return false;
		}
	}
	public sealed class RankState
	{
		public long Id;

		public string Name = "";

		public string TeamName = "";

		public string QueueNotice = "";

		public int Team = -1;

		public int Rank;

		public int Score;

		public int Strikes;

		public Faction Faction;

		public long ScoreOffset;

		public double? LastTeamkill;

		public bool Online;

		public bool HasRecovery;

		public bool StartingCommanderAwarded;

		public Dictionary<int, long> ThresholdOrder = new Dictionary<int, long>();
	}
	public sealed class RoundState
	{
		public string Token = Guid.NewGuid().ToString("N");

		public long Sequence;

		public Dictionary<long, RankState> Players = new Dictionary<long, RankState>();

		public Dictionary<int, long> Commanders = new Dictionary<int, long>();
	}
	public sealed class Privileges
	{
		public int Squad = 1;

		public int RankBonus;

		public int TraitorOtherBonus = -25;

		public UnitCategories Categories;

		public string[] ExcludedObjectInfoNames = new string[0];

		public bool Traitor;

		public bool Commander;

		public decimal Multiplier(RewardKind kind)
		{
			if (!Traitor)
			{
				return 1m + (decimal)RankBonus / 100m;
			}
			if (kind != RewardKind.Kill && kind != RewardKind.Structure)
			{
				return 1m + (decimal)TraitorOtherBonus / 100m;
			}
			return 0m;
		}
	}
	public sealed class RankEngine
	{
		public readonly RankConfig Config;

		public RoundState Round { get; private set; } = new RoundState();

		public event Action<long, string>? Message;

		public event Action<long, Privileges>? PolicyChanged;

		public event Action<long, string>? SoundRequested;

		public event Action<string>? GlobalMessage;

		public RankEngine(RankConfig config)
		{
			config.Validate();
			Config = config;
		}

		public void Reset()
		{
			long[] array = Round.Players.Keys.ToArray();
			Round = new RoundState();
			long[] array2 = array;
			foreach (long arg in array2)
			{
				PolicyChanged?.Invoke(arg, new Privileges
				{
					Squad = Config.Ranks[0].Squad,
					Categories = Config.Ranks[0].Categories
				});
			}
		}

		public void Restore(RoundState data)
		{
			if (data == null || string.IsNullOrWhiteSpace(data.Token) || data.Players == null || data.Commanders == null || data.Sequence < 0)
			{
				throw new ArgumentException("Invalid saved round.");
			}
			foreach (KeyValuePair<long, RankState> player in data.Players)
			{
				RankState value = player.Value;
				if (value == null || value.Id == 0L || value.Id != player.Key || value.Rank < -1 || value.Rank >= Config.Ranks.Length || value.Strikes < 0 || value.Name == null || value.TeamName == null || value.QueueNotice == null || value.ThresholdOrder == null || value.ScoreOffset < -1000000000000L || value.ScoreOffset > 1000000000000L || !Enum.IsDefined(typeof(Faction), value.Faction) || (value.LastTeamkill.HasValue && (double.IsNaN(value.LastTeamkill.Value) || double.IsInfinity(value.LastTeamkill.Value))) || value.ThresholdOrder.Any((KeyValuePair<int, long> p) => p.Key < 1 || p.Key >= Config.Ranks.Length || p.Value <= 0 || p.Value > data.Sequence))
				{
					throw new ArgumentException("Invalid saved player.");
				}
			}
			if (data.Commanders.Any((KeyValuePair<int, long> p) => !data.Players.ContainsKey(p.Value) || data.Players[p.Value].Team != p.Key))
			{
				throw new ArgumentException("Invalid saved commander.");
			}
			foreach (IGrouping<int, RankState> item in from s in data.Players.Values
				where s.Team >= 0
				group s by s.Team)
			{
				int rank;
				for (rank = 1; rank < Config.Ranks.Length; rank++)
				{
					if (Config.Ranks[rank].Seats > 0 && item.Count((RankState s) => s.Rank == rank) > Config.Ranks[rank].Seats)
					{
						throw new ArgumentException("Saved seats exceed limits.");
					}
				}
			}
			Round = data;
		}

		public RankState Observe(long id, string name, int team, string teamName, int score, bool online = true, bool promote = true, Faction? faction = null)
		{
			if (id == 0L)
			{
				throw new ArgumentException("Stable account identifier required.");
			}
			bool flag = !Round.Players.TryGetValue(id, out RankState value);
			if (flag)
			{
				value = new RankState
				{
					Id = id,
					Team = team
				};
				Round.Players.Add(id, value);
			}
			bool flag2 = !value.Online;
			value.Name = name ?? "";
			value.TeamName = teamName ?? "";
			value.Score = score;
			value.Online = online;
			value.Faction = faction ?? InferFaction(value.TeamName);
			if (value.Team != team)
			{
				bool num = value.Team >= 0 && team >= 0;
				RemoveAppointment(id);
				value.Team = team;
				value.ThresholdOrder.Clear();
				value.QueueNotice = "";
				if (num && value.Rank >= 0)
				{
					int rank = value.Rank;
					value.Rank = Math.Max(0, value.Rank - 5);
					FitDemotedSeat(value);
					Rebase(value);
					Message?.Invoke(id, "Team changed: demoted " + (rank - value.Rank) + " positions to " + Title(value) + ". Earn additional score to recover.");
					if (value.Rank < rank)
					{
						Cue(value, Config.Sounds.Demotion);
					}
				}
				Notify(value);
			}
			if (flag | flag2)
			{
				Notify(value);
			}
			if (promote)
			{
				Reconcile();
			}
			return value;
		}

		public static Faction InferFaction(string teamName)
		{
			string text = (teamName ?? "").ToLowerInvariant();
			if (!text.Contains("alien"))
			{
				if (!text.Contains("centauri"))
				{
					return Faction.Sol;
				}
				return Faction.Centauri;
			}
			return Faction.Alien;
		}

		public void Disconnect(long id)
		{
			if (Round.Players.TryGetValue(id, out RankState value))
			{
				value.Online = false;
				value.ThresholdOrder.Clear();
				value.QueueNotice = "";
				RemoveAppointment(id);
				Reconcile();
			}
		}

		private void RemoveAppointment(long id)
		{
			int[] array = (from p in Round.Commanders
				where p.Value == id
				select p.Key).ToArray();
			foreach (int key in array)
			{
				Round.Commanders.Remove(key);
			}
		}

		private long Effective(RankState s)
		{
			return s.Score - s.ScoreOffset;
		}

		private bool Eligible(RankState s, int next)
		{
			if (s.Online && s.Team >= 0 && s.Rank >= 0)
			{
				return Effective(s) >= Config.Ranks[next].Score;
			}
			return false;
		}

		private bool SeatFree(int team, int rank, long except = 0L)
		{
			if (Config.Ranks[rank].Seats != 0)
			{
				return Round.Players.Values.Count((RankState s) => s.Id != except && s.Team == team && s.Rank == rank) < Config.Ranks[rank].Seats;
			}
			return true;
		}

		public void Reconcile()
		{
			long value = ++Round.Sequence;
			foreach (RankState item in Round.Players.Values.Where((RankState rankState2) => rankState2.Online && rankState2.Team >= 0 && rankState2.Rank >= 0))
			{
				for (int num = item.Rank + 1; num < Config.Ranks.Length; num++)
				{
					if (Effective(item) >= Config.Ranks[num].Score && !item.ThresholdOrder.ContainsKey(num))
					{
						item.ThresholdOrder[num] = value;
					}
				}
			}
			Dictionary<long, RankState> dictionary = new Dictionary<long, RankState>();
			Dictionary<long, int> dictionary2 = new Dictionary<long, int>();
			bool flag;
			do
			{
				flag = false;
				int next;
				for (next = 1; next < Config.Ranks.Length; next++)
				{
					RankState[] array = (from s2 in Round.Players.Values
						where s2.Rank == next - 1 && Eligible(s2, next)
						orderby Order(s2, next)
						select s2).ThenBy((RankState rankState2) => rankState2.Id).ToArray();
					foreach (RankState rankState in array)
					{
						if (SeatFree(rankState.Team, next, rankState.Id))
						{
							if (!dictionary2.ContainsKey(rankState.Id))
							{
								dictionary2[rankState.Id] = rankState.Rank;
							}
							rankState.Rank = next;
							rankState.QueueNotice = "";
							dictionary[rankState.Id] = rankState;
							flag = true;
						}
					}
				}
			}
			while (flag);
			foreach (RankState value2 in dictionary.Values)
			{
				RankDefinition rankDefinition = Config.Ranks[value2.Rank];
				Message?.Invoke(value2.Id, "Promoted to " + Title(value2) + "! Personal credits: +" + rankDefinition.BonusPercent + "%." + (Policy(value2.Id).Commander ? "" : (" Squad size: " + Policy(value2.Id).Squad + ".")));
				Promoted(value2, dictionary2[value2.Id]);
				Notify(value2);
			}
			foreach (RankState s in Round.Players.Values)
			{
				string text = "";
				int next2 = s.Rank + 1;
				if (s.Online && s.Rank >= 0 && next2 < Config.Ranks.Length && Config.Ranks[next2].Seats > 0 && Eligible(s, next2))
				{
					List<RankState> list = (from p in Round.Players.Values
						where p.Team == s.Team && p.Online && p.Rank == next2 - 1 && Eligible(p, next2)
						orderby Order(p, next2), p.Id
						select p).ToList();
					text = "Waiting for " + Config.Ranks[next2].Display(s.Faction) + " seat: queue position " + (list.IndexOf(s) + 1) + ".";
				}
				if (s.QueueNotice != text)
				{
					if (text != "")
					{
						Message?.Invoke(s.Id, text);
					}
					else if (s.QueueNotice != "")
					{
						Message?.Invoke(s.Id, "No longer waiting for that rank seat.");
					}
					s.QueueNotice = text;
				}
			}
		}

		private static long Order(RankState s, int next)
		{
			if (!s.ThresholdOrder.TryGetValue(next, out var value))
			{
				return long.MaxValue;
			}
			return value;
		}

		public bool Teamkill(long id, int scoreAfter, double nowSeconds)
		{
			if (double.IsNaN(nowSeconds) || double.IsInfinity(nowSeconds))
			{
				throw new ArgumentException("Finite monotonic time required.");
			}
			if (!Round.Players.TryGetValue(id, out RankState value))
			{
				return false;
			}
			value.Score = scoreAfter;
			if (value.LastTeamkill.HasValue && nowSeconds - value.LastTeamkill.Value < Config.TeamkillCooldownSeconds)
			{
				return false;
			}
			value.LastTeamkill = nowSeconds;
			value.Strikes = ((value.Strikes == int.MaxValue) ? int.MaxValue : (value.Strikes + 1));
			if (value.Rank >= 0)
			{
				Demote(value, value.Strikes, "Teamkill #" + value.Strikes, value.Strikes >= 3);
			}
			return true;
		}

		private void FitDemotedSeat(RankState s)
		{
			while (s.Rank > 0 && !SeatFree(s.Team, s.Rank, s.Id))
			{
				s.Rank--;
			}
		}

		private void Rebase(RankState s)
		{
			s.ScoreOffset = (long)s.Score - (long)Config.Ranks[s.Rank].Score;
			s.HasRecovery = true;
			s.ThresholdOrder.Clear();
			s.QueueNotice = "";
		}

		public void RebindScore(long id, int freshScore)
		{
			if (Round.Players.TryGetValue(id, out RankState value) && value.HasRecovery && value.Rank >= 0)
			{
				value.ScoreOffset += (long)freshScore - (long)value.Score;
				value.Score = freshScore;
			}
		}

		private void Demote(RankState s, int amount, string reason, bool makeTraitor = false)
		{
			long num = (makeTraitor ? (-1) : Math.Max(0L, (long)s.Rank - (long)amount));
			if (num < 0)
			{
				s.Rank = -1;
				s.ScoreOffset = 0L;
				s.HasRecovery = false;
				s.ThresholdOrder.Clear();
				Message?.Invoke(s.Id, reason + ": " + Title(s) + ". Promotions locked until next round.");
				Cue(s, Config.Sounds.Traitor);
			}
			else
			{
				s.Rank = (int)num;
				FitDemotedSeat(s);
				Rebase(s);
				Message?.Invoke(s.Id, reason + ": demoted to " + Title(s) + ". Next promotion requires additional score.");
				Cue(s, Config.Sounds.Demotion);
			}
			Notify(s);
			Reconcile();
		}

		public bool AdminPromote(long id, out string error)
		{
			error = "";
			if (!Round.Players.TryGetValue(id, out RankState value))
			{
				error = "Player not found.";
				return false;
			}
			if (value.Rank < 0)
			{
				error = "Traitor promotions are locked for this round.";
				return false;
			}
			if (value.Rank >= Config.Ranks.Length - 1 || value.Team < 0)
			{
				error = "Player cannot be promoted in the current state.";
				return false;
			}
			int rank = value.Rank + 1;
			if (!SeatFree(value.Team, rank, id))
			{
				error = "The requested rank seat is occupied.";
				return false;
			}
			int rank2 = value.Rank;
			value.Rank = rank;
			Rebase(value);
			Message?.Invoke(id, "Admin promotion: " + Title(value) + ".");
			Promoted(value, rank2);
			Notify(value);
			Reconcile();
			return true;
		}

		public bool AdminDemote(long id, int scoreAfter, out string error)
		{
			error = "";
			if (!Round.Players.TryGetValue(id, out RankState value))
			{
				error = "Player not found.";
				return false;
			}
			if (value.Rank < 0)
			{
				error = "Player is already a Traitor.";
				return false;
			}
			if (value.Rank == 0)
			{
				error = "Player is already at the lowest normal rank; only the third teamkill strike assigns Traitor.";
				return false;
			}
			value.Score = scoreAfter;
			Demote(value, 1, "Admin demotion");
			return true;
		}

		public bool StartCommander(long id, out int scoreAfter)
		{
			scoreAfter = 0;
			if (!Round.Players.TryGetValue(id, out RankState value) || !value.Online || value.Team < 0 || value.Rank < 0 || value.StartingCommanderAwarded || value.HasRecovery || !Round.Commanders.Values.Contains(id))
			{
				return false;
			}
			value.StartingCommanderAwarded = true;
			value.Score = (int)Math.Min(2147483647L, (long)value.Score + (long)Config.StartingCommanderScore);
			int rank = value.Rank;
			value.Rank = Math.Max(value.Rank, Config.StartingCommanderRank);
			value.ThresholdOrder.Clear();
			scoreAfter = value.Score;
			Message?.Invoke(id, "Starting commander rank: " + Title(value) + ". Round score: +" + Config.StartingCommanderScore + ". Personal credits: +" + Config.Ranks[value.Rank].BonusPercent + "%.");
			if (value.Rank > rank)
			{
				Promoted(value, rank);
			}
			Notify(value);
			return true;
		}

		public void SetCommanders(IDictionary<int, long> active, bool announceChanges = true)
		{
			KeyValuePair<int, long>[] array = Round.Commanders.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<int, long> keyValuePair = array[i];
				if (!active.TryGetValue(keyValuePair.Key, out var value) || value != keyValuePair.Value || !Round.Players.TryGetValue(value, out RankState value2) || !value2.Online || value2.Team != keyValuePair.Key)
				{
					Round.Commanders.Remove(keyValuePair.Key);
					if (announceChanges)
					{
						Message?.Invoke(keyValuePair.Value, "Commander role ended; earned rank and reward bonus retained.");
					}
					if (Round.Players.TryGetValue(keyValuePair.Value, out RankState value3))
					{
						Notify(value3);
					}
				}
			}
			foreach (KeyValuePair<int, long> item in active)
			{
				if (Round.Players.TryGetValue(item.Value, out RankState value4) && value4.Online && value4.Team == item.Key && (!Round.Commanders.TryGetValue(item.Key, out var value5) || value5 != item.Value))
				{
					Round.Commanders[item.Key] = item.Value;
					if (announceChanges)
					{
						Message?.Invoke(item.Value, "Commander role active; normal rank: " + Title(value4) + ". Personal resources: +" + ((value4.Rank >= 0) ? (Config.Ranks[value4.Rank].BonusPercent + "%") : "Traitor restrictions") + "; no AI followers while commanding.");
					}
					Notify(value4);
				}
			}
		}

		public Privileges Policy(long id)
		{
			Privileges privileges = new Privileges
			{
				TraitorOtherBonus = Config.TraitorOtherBonusPercent
			};
			if (!Round.Players.TryGetValue(id, out RankState value))
			{
				return privileges;
			}
			privileges.Traitor = value.Rank < 0;
			privileges.Commander = Round.Commanders.Values.Contains(id);
			privileges.ExcludedObjectInfoNames = Config.HarvesterRecruitmentExclusions();
			if (value.Rank >= 0)
			{
				RankDefinition rankDefinition = Config.Ranks[value.Rank];
				privileges.RankBonus = rankDefinition.BonusPercent;
				if (!privileges.Commander)
				{
					privileges.Squad = rankDefinition.Squad;
					privileges.Categories = rankDefinition.Categories;
					privileges.ExcludedObjectInfoNames = rankDefinition.ExcludedObjectInfoNames.Concat(Config.HarvesterRecruitmentExclusions()).Distinct().ToArray();
				}
			}
			return privileges;
		}

		private string Title(RankState s)
		{
			if (s.Rank >= 0)
			{
				return Config.Ranks[s.Rank].Display(s.Faction);
			}
			if (s.Faction != Faction.Alien)
			{
				if (s.Faction != Faction.Centauri)
				{
					return "Traitor";
				}
				return "Deserter";
			}
			return "Outcast";
		}

		private void Cue(RankState s, string file)
		{
			if (Config.Sounds.Enabled && file != "")
			{
				SoundRequested?.Invoke(s.Id, file);
			}
		}

		private void Promoted(RankState s, int previousRank)
		{
			Cue(s, Config.Sounds.Promotion(s.Rank, s.Faction));
			for (int i = previousRank + 1; i <= s.Rank; i++)
			{
				if (i == 9 || i >= 15)
				{
					GlobalMessage?.Invoke(Plain(s.Name) + " just achieved " + Plain(Config.Ranks[i].Display(s.Faction)) + " !!");
				}
			}
		}

		public RankedKillBonus? HigherRankKill(long killerId, long victimId)
		{
			if (killerId == victimId || !Round.Players.TryGetValue(killerId, out RankState value) || !Round.Players.TryGetValue(victimId, out RankState value2) || !value.Online || !value2.Online || value.Team < 0 || value2.Team < 0 || value.Team == value2.Team || value.Rank < 0 || value2.Rank <= value.Rank)
			{
				return null;
			}
			return new RankedKillBonus
			{
				KillerId = killerId,
				RoundToken = Round.Token,
				KillerName = Plain(value.Name),
				KillerRank = Plain(Title(value)),
				VictimName = Plain(value2.Name),
				VictimRank = Plain(Title(value2)),
				Multiplier = Math.Max(2, value2.Rank - value.Rank)
			};
		}

		private void Notify(RankState s)
		{
			PolicyChanged?.Invoke(s.Id, Policy(s.Id));
		}

		public string Describe(long id)
		{
			RankState rankState = Round.Players[id];
			Privileges privileges = Policy(id);
			string text = (privileges.Traitor ? (Config.TraitorOtherBonusPercent + "% reward bonus (no combat rewards)") : ("+" + privileges.RankBonus + "% reward bonus"));
			string text2 = privileges.Squad + " squad " + ((privileges.Squad == 1) ? "unit" : "units");
			string text3 = (privileges.ExcludedObjectInfoNames.Contains("ObjectInfo_Sol_UltraHeavy_SiegeTank") ? "any unit except Siege/Crimson tanks" : "any unit");
			if (Config.EnforceHarvesterRecruitmentBan)
			{
				text3 += ((text3 == "any unit") ? " except harvesters/Shrimp" : " and harvesters/Shrimp");
			}
			string text4;
			if (privileges.Categories == UnitCategories.Any)
			{
				text4 = text3;
			}
			else if ((privileges.Categories & UnitCategories.Air) != 0)
			{
				text4 = "air units";
			}
			else if ((privileges.Categories & UnitCategories.Heavy) != 0)
			{
				text4 = "heavy units";
			}
			else
			{
				text4 = (((privileges.Categories & UnitCategories.Light) != 0) ? "light units" : "infantry");
			}
			return Plain(rankState.Name) + " is " + Title(rankState) + ", has " + text + ", gets " + text2 + ((privileges.Categories == UnitCategories.None) ? " (player only)." : (" up to " + text4 + "."));
		}

		private static string UnitLabel(string name)
		{
			if (!(name == "ObjectInfo_Sol_UltraHeavy_SiegeTank"))
			{
				if (!(name == "ObjectInfo_Cent_UltraHeavy_CrimsonTank"))
				{
					return Plain(name);
				}
				return "Crimson Tank";
			}
			return "Siege Tank";
		}

		public static string Plain(string text)
		{
			return (text ?? "").Replace("<", "[").Replace(">", "]").Replace("\r", " ")
				.Replace("\n", " ");
		}

		public static long? Resolve(string query, IEnumerable<RankState> players, out string error)
		{
			query = (query ?? "").Trim();
			if (query.Length >= 2 && query[0] == '"' && query[query.Length - 1] == '"')
			{
				query = query.Substring(1, query.Length - 2);
			}
			if (query == "")
			{
				error = "Specify a player name.";
				return null;
			}
			RankState[] source = players.Where((RankState s) => s.Online).ToArray();
			RankState[] array = source.Where((RankState s) => string.Equals(s.Name, query, StringComparison.OrdinalIgnoreCase)).ToArray();
			if (array.Length == 0)
			{
				array = source.Where((RankState s) => s.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
			}
			error = ((array.Length > 1) ? "Name is ambiguous; use a more specific player name." : "Player not found.");
			if (array.Length != 1)
			{
				return null;
			}
			error = "";
			return array[0].Id;
		}
	}
	internal sealed class FormationAdapter
	{
		private const string Owner = "SilicaFieldRanks";

		private MethodInfo? request;

		private MethodInfo? clearOwner;

		private MethodInfo? readPolicy;

		private MethodInfo? readLimit;

		private PropertyInfo? enabled;

		private RankConfig config = new RankConfig();

		private readonly Dictionary<long, string> applied = new Dictionary<long, string>();

		private readonly Dictionary<long, string> status = new Dictionary<long, string>();

		public void Configure(RankConfig value)
		{
			config = value;
		}

		public void Bind()
		{
			Type type = (from a in AppDomain.CurrentDomain.GetAssemblies()
				select a.GetType("Si_Formation.FormationApi", throwOnError: false)).FirstOrDefault((Type t) => t != null);
			request = type?.GetMethod("RequestPlayerPolicy", new Type[6]
			{
				typeof(ulong),
				typeof(string),
				typeof(int),
				typeof(string[]),
				typeof(string[]),
				typeof(string[])
			});
			clearOwner = type?.GetMethod("ClearOwnerPolicies", new Type[1] { typeof(string) });
			readPolicy = type?.GetMethod("TryGetPlayerPolicy");
			readLimit = type?.GetMethod("TryGetPlayerLimit");
			enabled = type?.GetProperty("GlobalEnabled");
		}

		private string[] Categories(Privileges policy)
		{
			if (policy.Categories == UnitCategories.Any)
			{
				return new string[1] { "*" };
			}
			return config.NativeCategoryRules.Where((KeyValuePair<string, string[]> p) => Enum.TryParse<UnitCategories>(p.Key, out var result) && (policy.Categories & result) != 0).SelectMany((KeyValuePair<string, string[]> p) => p.Value).Distinct(StringComparer.Ordinal)
				.OrderBy((string s) => s, StringComparer.Ordinal)
				.ToArray();
		}

		private static string Signature(int limit, string[] categories, string[] allowed, string[] denied)
		{
			return limit + "|" + string.Join(",", categories) + "|" + string.Join(",", allowed) + "|" + string.Join(",", denied);
		}

		public void Update(long id, Privileges policy)
		{
			int num = Math.Max(1, policy.Squad);
			string[] array = Categories(policy);
			string[] array2 = (from p in config.NativeObjectCategoryOverrides
				where (policy.Categories & p.Value) != 0
				select p.Key).OrderBy((string s) => s, StringComparer.Ordinal).ToArray();
			string[] array3 = policy.ExcludedObjectInfoNames.Concat(from p in config.NativeObjectCategoryOverrides
				where (policy.Categories & p.Value) == 0
				select p.Key).Distinct(StringComparer.Ordinal).OrderBy((string s) => s, StringComparer.Ordinal)
				.ToArray();
			string text = Signature(num, array, array2, array3);
			if (request == null)
			{
				Bind();
				if (request == null)
				{
					status[id] = "Formation 2.10.0 policy API unavailable";
					return;
				}
			}
			try
			{
				if (applied.TryGetValue(id, out string value) && value == text && readPolicy != null)
				{
					object[] array4 = new object[6]
					{
						(ulong)id,
						null,
						0,
						null,
						null,
						null
					};
					string text2 = readPolicy.Invoke(null, array4)?.ToString() ?? "Unknown";
					if ((text2 == "Success" || text2 == "AdminOverride") && (string)array4[1] == "SilicaFieldRanks" && Signature((int)array4[2], (array4[3] as string[]) ?? Array.Empty<string>(), (array4[4] as string[]) ?? Array.Empty<string>(), (array4[5] as string[]) ?? Array.Empty<string>()) == text)
					{
						status[id] = text2;
						return;
					}
				}
				string text3 = request.Invoke(null, new object[6]
				{
					(ulong)id,
					"SilicaFieldRanks",
					num,
					array,
					array2,
					array3
				})?.ToString() ?? "Unknown";
				status[id] = text3;
				if (text3 == "Success" || text3 == "AdminOverride")
				{
					applied[id] = text;
				}
			}
			catch (Exception ex)
			{
				status[id] = "Formation API error: " + ex.GetBaseException().Message;
			}
		}

		public string Describe(long id)
		{
			if (request == null || readLimit == null)
			{
				return "Formation policy API unavailable; rank squad permissions inactive.";
			}
			try
			{
				object[] array = new object[2]
				{
					(ulong)id,
					0
				};
				string text = readLimit.Invoke(null, array)?.ToString() ?? "Unknown";
				bool valueOrDefault = enabled?.GetValue(null) as bool? == true;
				string text2 = (status.TryGetValue(id, out string value) ? value : "pending");
				return (text == "Success") ? string.Format("Formation effective cap: {0} including player; {1}; rank policy: {2}.", array[1], valueOrDefault ? "enabled" : "disabled/native fallback", text2) : ("Formation status: " + text + "; rank policy: " + text2 + ".");
			}
			catch
			{
				return "Formation policy status unavailable.";
			}
		}

		public void ForgetApplications()
		{
			applied.Clear();
		}

		public void ClearAll()
		{
			try
			{
				clearOwner?.Invoke(null, new object[1] { "SilicaFieldRanks" });
			}
			catch
			{
			}
			applied.Clear();
			status.Clear();
		}
	}
	public sealed class RankMod : MelonMod
	{
		[HarmonyPatch(typeof(ClientChatHandler), "OnRequestPlayerChat")]
		private static class ChatPatch
		{
			[HarmonyPriority(800)]
			private static bool Prefix(OnRequestPlayerChatArgs args)
			{
				if (!Server || args == null || !args.Player || string.IsNullOrWhiteSpace(args.Text))
				{
					return true;
				}
				string text = args.Text.Trim().Split(new char[2] { ' ', '\t' }, 2)[0];
				if (!new string[4] { "/rank", "/promote", "/demote", "/debug" }.Contains(text, StringComparer.OrdinalIgnoreCase))
				{
					return true;
				}
				if (engine == null && !text.Equals("/debug", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				args.Block = true;
				if (args.TeamOnly && SiAdminMod.Pref_Admin_AcceptTeamChatCommands != null && !SiAdminMod.Pref_Admin_AcceptTeamChatCommands.Value)
				{
					return false;
				}
				try
				{
					Command(args.Player, args.Text);
				}
				catch (Exception ex)
				{
					Tell(Id(args.Player), "Rank command failed; see server log.");
					RankLog.Error(ex.ToString());
				}
				return false;
			}
		}

		private sealed class KillState
		{
			public Player? Attacker;

			public short Kills;

			public bool FriendlyUnit;
		}

		[HarmonyPatch(typeof(MP_Strategy), "ResolveCommanderLottery")]
		private static class StartingCommanderLotteryPatch
		{
			private static void Postfix(MP_Strategy __instance)
			{
				FinishStartingCommanders(__instance);
			}
		}

		[HarmonyPatch(typeof(Player), "OnTargetDestroyed")]
		private static class TeamkillPatch
		{
			private static void Prefix(Target __0, GameObject __1, out KillState? __state)
			{
				__state = null;
				if (Ready && (bool)__0 && (bool)__1 && (bool)__0.ObjectInfo)
				{
					Unit unit = __1.GetBaseGameObject() as Unit;
					if ((bool)unit && (bool)unit.ControlledBy)
					{
						__state = new KillState
						{
							Attacker = unit.ControlledBy,
							Kills = unit.ControlledBy.Kills,
							FriendlyUnit = ((bool)__0.OwnerUnit && __0.OwnerUnit != unit && __0.Team == unit.ControlledBy.Team)
						};
					}
				}
			}

			private static void Postfix(KillState? __state)
			{
				if (Ready && __state != null && (bool)__state.Attacker && __state.FriendlyUnit)
				{
					Player attacker = __state.Attacker;
					if (attacker.Kills == (short)(__state.Kills - 1))
					{
						Observe(attacker, promote: false, readScore: true);
						engine.Teamkill(Id(attacker), attacker.Score, Time.realtimeSinceStartup);
					}
				}
			}
		}

		private const string LifetimeKey = "SilicaFieldRanks.ProcessLifetime.v1";

		private static RankEngine? engine;

		private static RankConfig config = null;

		private static readonly FormationAdapter formation = new FormationAdapter();

		private static readonly Dictionary<long, Player> playerInstances = new Dictionary<long, Player>();

		private static bool active;

		private static bool awaitNewRound;

		private static bool confirmedRoundStart;

		private static bool startingCommandersPending;

		private static int modeId;

		private static float startTime;

		private static float lastGameTime;

		private static float nextPoll;

		private static float nextRoster;

		private static string initializationError = "";

		private static bool Server
		{
			get
			{
				if (Game.GetIsServer())
				{
					return !Game.GetIsClient();
				}
				return false;
			}
		}

		private static bool Ready
		{
			get
			{
				if (engine != null && active && !awaitNewRound)
				{
					return Server;
				}
				return false;
			}
		}

		public static bool HarvesterRecruitmentBanEnabled => HarvesterRestrictions.Enabled;

		public override void OnInitializeMelon()
		{
			RankLog.Enabled = false;
			initializationError = "";
			try
			{
				string text = Path.Combine(MelonEnvironment.UserDataDirectory, "Ranks_cfg");
				string path = Path.Combine(text, "Si_Ranks_Config.json");
				if (File.Exists(path))
				{
					config = JsonConvert.DeserializeObject<RankConfig>(File.ReadAllText(path)) ?? throw new InvalidDataException("Empty configuration.");
				}
				else
				{
					config = new RankConfig();
					config.Validate();
					Directory.CreateDirectory(text);
					using FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
					using StreamWriter streamWriter = new StreamWriter(stream);
					streamWriter.Write(JsonConvert.SerializeObject(config, Formatting.Indented));
				}
				RankLog.Enabled = config.Debug;
				config.Validate();
				HarvesterRestrictions.Enabled = config.EnforceHarvesterRecruitmentBan;
				formation.Configure(config);
				engine = new RankEngine(config);
				engine.Message += (long id, string message) =>
				{
					Tell(id, message);
				};
				engine.PolicyChanged += formation.Update;
				engine.SoundRequested += RankSounds.Play;
				engine.GlobalMessage += (string message) =>
				{
					HelperMethods.SendChatMessageToAll("[Ranks] ", message);
				};
				awaitNewRound = AppDomain.CurrentDomain.GetData("SilicaFieldRanks.ProcessLifetime.v1") != null;
				AppDomain.CurrentDomain.SetData("SilicaFieldRanks.ProcessLifetime.v1", true);
				if (awaitNewRound)
				{
					RankLog.Warning("Reload detected: ranks suspended until the next genuine round-start event; old round state will not leak.");
				}
				RankLog.Info("Personal rewards use temporary source-specific KoH patches. FPS limits include the player: 1 means no AI followers.");
			}
			catch (Exception ex)
			{
				engine = null;
				initializationError = "Ranks disabled: configuration load failed. Existing file preserved. " + ex.Message;
				RankLog.Error(initializationError);
			}
		}

		public override void OnLateInitializeMelon()
		{
			formation.Bind();
			if (engine != null)
			{
				RewardPatches.Bind(((MelonBase)this).HarmonyInstance);
				IEnumerable<string> filenames;
				if (!config.Sounds.Enabled)
				{
					IEnumerable<string> enumerable = Array.Empty<string>();
					filenames = enumerable;
				}
				else
				{
					filenames = config.Sounds.Files();
				}
				RankSounds.Initialize(filenames);
			}
			Bind();
		}

		internal static decimal RewardPolicy(long id, RewardKind kind)
		{
			if (!Ready)
			{
				return 1m;
			}
			return engine.Policy(id).Multiplier(kind);
		}

		internal static RankedKillBonus? HigherRankKill(long killerId, long victimId)
		{
			if (!Ready)
			{
				return null;
			}
			return engine.HigherRankKill(killerId, victimId);
		}

		internal static bool RankedKillValid(RankedKillBonus bonus)
		{
			if (Ready && bonus.RoundToken == engine.Round.Token)
			{
				return !engine.Policy(bonus.KillerId).Traitor;
			}
			return false;
		}

		internal static void AnnounceHigherRankKill(RankedKillBonus bonus, int paid)
		{
			if (Ready && paid > 0)
			{
				HelperMethods.SendChatMessageToAll("[Ranks] ", bonus.Announcement(paid));
			}
		}

		public override void OnSceneWasLoaded(int index, string name)
		{
			Bind();
		}

		public override void OnSceneWasInitialized(int index, string name)
		{
			Bind();
		}

		private static void Bind()
		{
			if (engine != null)
			{
				GameEvents.OnGameStarted = (Action<GameMode>)Delegate.Remove(GameEvents.OnGameStarted, new Action<GameMode>(Started));
				GameEvents.OnGameStarted = (Action<GameMode>)Delegate.Combine(GameEvents.OnGameStarted, new Action<GameMode>(Started));
				GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Remove(GameEvents.OnGameEnded, new Action<GameMode, Team>(Ended));
				GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Combine(GameEvents.OnGameEnded, new Action<GameMode, Team>(Ended));
				GameEvents.OnGameRestart = (Action<GameMode>)Delegate.Remove(GameEvents.OnGameRestart, new Action<GameMode>(Restarted));
				GameEvents.OnGameRestart = (Action<GameMode>)Delegate.Combine(GameEvents.OnGameRestart, new Action<GameMode>(Restarted));
				GameEvents.OnCommanderChanged = (Action<Team, Player, Player>)Delegate.Remove(GameEvents.OnCommanderChanged, new Action<Team, Player, Player>(CommanderChanged));
				GameEvents.OnCommanderChanged = (Action<Team, Player, Player>)Delegate.Combine(GameEvents.OnCommanderChanged, new Action<Team, Player, Player>(CommanderChanged));
				GameEvents.OnPlayerChangedTeam = (Action<Player, Team, Team>)Delegate.Remove(GameEvents.OnPlayerChangedTeam, new Action<Player, Team, Team>(TeamChanged));
				GameEvents.OnPlayerChangedTeam = (Action<Player, Team, Team>)Delegate.Combine(GameEvents.OnPlayerChangedTeam, new Action<Player, Team, Team>(TeamChanged));
				GameEvents.OnPlayerLeft = (Action<Player>)Delegate.Remove(GameEvents.OnPlayerLeft, new Action<Player>(Left));
				GameEvents.OnPlayerLeft = (Action<Player>)Delegate.Combine(GameEvents.OnPlayerLeft, new Action<Player>(Left));
				GameEvents.OnPlayerDeleted = (Action<Player>)Delegate.Remove(GameEvents.OnPlayerDeleted, new Action<Player>(Left));
				GameEvents.OnPlayerDeleted = (Action<Player>)Delegate.Combine(GameEvents.OnPlayerDeleted, new Action<Player>(Left));
			}
		}

		public static IReadOnlyDictionary<string, string>? GetSquadHudStatus(ulong steamId)
		{
			if (!Ready || !engine.Round.Players.TryGetValue((long)steamId, out RankState value) || !value.Online || value.Team < 0)
			{
				return null;
			}
			Privileges privileges = engine.Policy(value.Id);
			string value2;
			if (value.Rank >= 0)
			{
				value2 = config.Ranks[value.Rank].Display(value.Faction);
			}
			else if (value.Faction == Faction.Alien)
			{
				value2 = "Outcast";
			}
			else
			{
				value2 = ((value.Faction == Faction.Centauri) ? "Deserter" : "Traitor");
			}
			return new Dictionary<string, string>
			{
				{ "schema", "1" },
				{
					"round",
					engine.Round.Token
				},
				{
					"team",
					value.Team.ToString(CultureInfo.InvariantCulture)
				},
				{ "title", value2 },
				{
					"cap",
					privileges.Squad.ToString(CultureInfo.InvariantCulture)
				}
			};
		}

		private static long Id(Player p)
		{
			return (long)p.PlayerID.SteamID.m_SteamID;
		}

		private static void Tell(long id, string message)
		{
			Player player = Player.Players.FirstOrDefault((Player p) => (bool)p && Id(p) == id);
			if ((bool)player)
			{
				HelperMethods.SendChatMessageToPlayer(player, "[Ranks] ", message);
			}
		}

		private static string TeamLabel(Team t)
		{
			return t.name switch
			{
				"Team_Alien" => "Alien", 
				"Team_Human_Centauri" => "Centauri", 
				"Team_Human_Sol" => "Sol", 
				_ => t.TeamShortName, 
			};
		}

		private static void Observe(Player p, bool promote = false, bool readScore = false)
		{
			if (Ready && (bool)p && Id(p) != 0L)
			{
				long num = Id(p);
				engine.Round.Players.TryGetValue(num, out RankState value);
				if (value != null && playerInstances.TryGetValue(num, out Player value2) && (object)value2 != p)
				{
					engine.RebindScore(num, p.Score);
					readScore = true;
				}
				playerInstances[num] = p;
				int team = (p.Team ? p.Team.Index : (value?.Team ?? (-1)));
				string teamName = (p.Team ? TeamLabel(p.Team) : (value?.TeamName ?? "No team"));
				engine.Observe(num, p.PlayerName, team, teamName, readScore ? p.Score : (value?.Score ?? 0), online: true, promote);
			}
		}

		private static void Started(GameMode mode)
		{
			if (!confirmedRoundStart || !Ready || mode.GetInstanceID() != modeId || !(mode.GameTime + 0.1f >= lastGameTime))
			{
				StartRound(mode, announceStartingRank: true);
			}
		}

		private static void StartRound(GameMode mode, bool announceStartingRank)
		{
			if (engine == null || !Server)
			{
				return;
			}
			RankSounds.Reset();
			formation.ClearAll();
			engine.Reset();
			formation.ForgetApplications();
			playerInstances.Clear();
			awaitNewRound = false;
			active = true;
			modeId = mode.GetInstanceID();
			confirmedRoundStart = announceStartingRank;
			startingCommandersPending = announceStartingRank;
			startTime = Time.realtimeSinceStartup;
			lastGameTime = mode.GameTime;
			nextPoll = startTime + (float)config.PollSeconds;
			nextRoster = 0f;
			foreach (Player item in Player.Players.Where((Player p) => p).OrderBy(Id))
			{
				Observe(item);
			}
			RefreshCommanders(!announceStartingRank);
			if (announceStartingRank && !(mode is MP_Strategy))
			{
				FinishStartingCommanders(mode);
			}
			RankLog.Info("Round started: ranks, strikes, cooldowns, recovery, queues, seats and commander roles reset.");
		}

		private static void FinishStartingCommanders(GameMode mode)
		{
			if (!Ready || !startingCommandersPending || mode.GetInstanceID() != modeId)
			{
				return;
			}
			startingCommandersPending = false;
			RefreshRoster(readScores: false, reconcile: false);
			RefreshCommanders(announceChanges: false);
			long[] array = engine.Round.Commanders.Values.Distinct().ToArray();
			foreach (long id in array)
			{
				Player player = Player.Players.FirstOrDefault((Player p) => (bool)p && Id(p) == id);
				if ((bool)player)
				{
					Observe(player, promote: false, readScore: true);
					if (engine.StartCommander(id, out var scoreAfter))
					{
						player.Score = scoreAfter;
					}
				}
			}
			foreach (Player item in Player.Players.Where((Player p) => (bool)p && (bool)p.Team && !p.IsCommander).OrderBy(Id))
			{
				if (engine.Round.Players.TryGetValue(Id(item), out RankState value))
				{
					RankDefinition rankDefinition = config.Ranks[0];
					Tell(Id(item), $"Starting rank: {rankDefinition.Display(value.Faction)}. Personal credits: +{rankDefinition.BonusPercent}%. Squad size: {rankDefinition.Squad} (including you).");
				}
			}
		}

		private static void Ended(GameMode mode, Team winner)
		{
			StopRound();
		}

		private static void Restarted(GameMode mode)
		{
			StopRound();
		}

		private static void StopRound()
		{
			if (Server)
			{
				active = false;
				confirmedRoundStart = false;
				startingCommandersPending = false;
				RankSounds.Reset();
				engine?.Reset();
				formation.ClearAll();
				playerInstances.Clear();
			}
		}

		private static void Left(Player p)
		{
			if (Ready && (bool)p && Id(p) != 0L)
			{
				engine.Disconnect(Id(p));
				RefreshCommanders();
			}
		}

		private static void TeamChanged(Player p, Team oldTeam, Team newTeam)
		{
			if (Ready && (bool)p && (bool)newTeam)
			{
				Observe(p, promote: false, readScore: true);
				engine.Reconcile();
				RefreshCommanders();
			}
		}

		private static void CommanderChanged(Team team, Player oldPlayer, Player newPlayer)
		{
			if (Ready)
			{
				RefreshRoster(readScores: false, reconcile: false);
				RefreshCommanders(!startingCommandersPending);
			}
		}

		private static void RefreshRoster(bool readScores, bool reconcile)
		{
			if (!Ready)
			{
				return;
			}
			HashSet<long> hashSet = new HashSet<long>();
			foreach (Player item in Player.Players.Where((Player p) => p).OrderBy(Id))
			{
				if (Id(item) != 0L)
				{
					hashSet.Add(Id(item));
					Observe(item, promote: false, readScores);
				}
			}
			long[] array = engine.Round.Players.Keys.ToArray();
			foreach (long num2 in array)
			{
				if (!hashSet.Contains(num2))
				{
					engine.Disconnect(num2);
				}
			}
			if (reconcile)
			{
				engine.Reconcile();
			}
			foreach (long item2 in hashSet)
			{
				formation.Update(item2, engine.Policy(item2));
			}
		}

		private static void RefreshCommanders(bool announceChanges = true)
		{
			if (!Ready)
			{
				return;
			}
			Dictionary<int, long> dictionary = new Dictionary<int, long>();
			if (GameMode.CurrentGameMode is GameModeExt gameModeExt)
			{
				foreach (Team item in (from p in Player.Players
					where (bool)p && (bool)p.Team
					select p.Team).Distinct())
				{
					Player commanderForTeam = gameModeExt.GetCommanderForTeam(item);
					if ((bool)commanderForTeam && Id(commanderForTeam) != 0L && engine.Round.Players.ContainsKey(Id(commanderForTeam)))
					{
						dictionary[item.Index] = Id(commanderForTeam);
					}
				}
			}
			engine.SetCommanders(dictionary, announceChanges);
		}

		public override void OnUpdate()
		{
			if (engine == null || !Server || awaitNewRound)
			{
				return;
			}
			GameMode currentGameMode = GameMode.CurrentGameMode;
			if (!currentGameMode || !currentGameMode.GameBegun || currentGameMode.GameOver)
			{
				if (active)
				{
					StopRound();
				}
				return;
			}
			RankSounds.Tick();
			if (!active)
			{
				StartRound(currentGameMode, announceStartingRank: false);
				startTime = Time.realtimeSinceStartup - currentGameMode.GameTime;
				RankLog.Warning("Attached during an existing round: earlier penalties and eligibility order cannot be reconstructed. Begin a new round for complete history.");
				RefreshRoster(readScores: true, reconcile: true);
				RefreshCommanders();
				return;
			}
			if (currentGameMode.GetInstanceID() != modeId || currentGameMode.GameTime + 0.1f < lastGameTime)
			{
				Started(currentGameMode);
				return;
			}
			lastGameTime = currentGameMode.GameTime;
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (realtimeSinceStartup >= nextRoster)
			{
				nextRoster = realtimeSinceStartup + 1f;
				RefreshRoster(readScores: false, reconcile: false);
				RefreshCommanders();
			}
			if (realtimeSinceStartup >= nextPoll)
			{
				nextPoll = realtimeSinceStartup + (float)config.PollSeconds;
				RefreshRoster(readScores: true, reconcile: true);
			}
		}

		private static bool TryQuery(string text, out string command, out string query, out string error)
		{
			text = text.Trim();
			int num = text.IndexOfAny(new char[2] { ' ', '\t' });
			command = ((num < 0) ? text : text.Substring(0, num)).ToLowerInvariant();
			query = ((num < 0) ? "" : text.Substring(num + 1).Trim());
			error = "";
			if (query.StartsWith("\"", StringComparison.Ordinal))
			{
				try
				{
					query = JsonConvert.DeserializeObject<string>(query, new JsonSerializerSettings
					{
						CheckAdditionalContent = true
					}) ?? "";
				}
				catch
				{
					error = "Invalid quoted player name; use /rank \"Player Name\".";
					return false;
				}
			}
			else if (query.Contains('"'))
			{
				error = "Quote the entire player name.";
				return false;
			}
			if (query.Any(char.IsControl))
			{
				error = "Invalid player name.";
				return false;
			}
			return true;
		}

		private static void Command(Player caller, string text)
		{
			if (!TryQuery(text, out string command, out string query, out string error))
			{
				Tell(Id(caller), error);
				return;
			}
			if (command == "/debug")
			{
				if (!caller.CanAdminExecute(Power.Generic))
				{
					Tell(Id(caller), "Permission denied: admin Generic power required.");
					return;
				}
				string text2 = query.ToLowerInvariant();
				if (text2 == null || text2.Length != 0)
				{
					switch (text2)
					{
					case "on":
						RankLog.Enabled = true;
						break;
					case "off":
						RankLog.Enabled = false;
						break;
					default:
						Tell(Id(caller), "Usage: /debug [on|off|status] (Ranks logging).");
						return;
					case "status":
						break;
					}
				}
				else
				{
					RankLog.Enabled = !RankLog.Enabled;
				}
				Tell(Id(caller), "Ranks debug logging: " + (RankLog.Enabled ? "ON" : "OFF") + ".");
				if (query.Equals("status", StringComparison.OrdinalIgnoreCase))
				{
					Tell(Id(caller), RewardPatches.Status + " " + formation.Describe(Id(caller)));
				}
				if (RankLog.Enabled && query.ToLowerInvariant() != "status")
				{
					RankLog.Info("Ranks debug logging enabled. " + RewardPatches.Status);
					if (initializationError != "")
					{
						RankLog.Error(initializationError);
					}
				}
				return;
			}
			if (!Ready)
			{
				Tell(Id(caller), "Ranks inactive; wait for a valid new round.");
				return;
			}
			RefreshRoster(readScores: false, reconcile: false);
			bool flag = command != "/rank";
			if (flag && !caller.CanAdminExecute(Power.Generic))
			{
				Tell(Id(caller), "Permission denied: admin Generic power required.");
				return;
			}
			if (flag && query.Length == 0)
			{
				Tell(Id(caller), "Usage: " + command + " \"Player Name\".");
				return;
			}
			long? target = ((query.Length == 0) ? new long?(Id(caller)) : RankEngine.Resolve(query, engine.Round.Players.Values, out error));
			if (!target.HasValue)
			{
				Tell(Id(caller), error);
				return;
			}
			Player player = Player.Players.FirstOrDefault((Player p) => (bool)p && Id(p) == target.Value);
			if (!player)
			{
				Tell(Id(caller), "Player not found.");
				return;
			}
			if (flag && !caller.CanAdminExecute(Power.Generic, player))
			{
				Tell(Id(caller), "Permission denied for this target.");
				return;
			}
			if (command == "/rank")
			{
				Tell(Id(caller), engine.Describe(target.Value));
				return;
			}
			Observe(player, promote: false, readScore: true);
			bool flag2 = ((command == "/promote") ? engine.AdminPromote(target.Value, out error) : engine.AdminDemote(target.Value, player.Score, out error));
			Tell(Id(caller), flag2 ? ("Rank updated for " + RankEngine.Plain(player.PlayerName) + ".") : error);
		}

		public override void OnDeinitializeMelon()
		{
			active = false;
			confirmedRoundStart = false;
			startingCommandersPending = false;
			RankSounds.Reset();
			HarvesterRestrictions.Reset();
			formation.ClearAll();
			RewardPatches.Unbind(((MelonBase)this).HarmonyInstance);
			GameEvents.OnGameStarted = (Action<GameMode>)Delegate.Remove(GameEvents.OnGameStarted, new Action<GameMode>(Started));
			GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Remove(GameEvents.OnGameEnded, new Action<GameMode, Team>(Ended));
			GameEvents.OnGameRestart = (Action<GameMode>)Delegate.Remove(GameEvents.OnGameRestart, new Action<GameMode>(Restarted));
			GameEvents.OnCommanderChanged = (Action<Team, Player, Player>)Delegate.Remove(GameEvents.OnCommanderChanged, new Action<Team, Player, Player>(CommanderChanged));
			GameEvents.OnPlayerChangedTeam = (Action<Player, Team, Team>)Delegate.Remove(GameEvents.OnPlayerChangedTeam, new Action<Player, Team, Team>(TeamChanged));
			GameEvents.OnPlayerLeft = (Action<Player>)Delegate.Remove(GameEvents.OnPlayerLeft, new Action<Player>(Left));
			GameEvents.OnPlayerDeleted = (Action<Player>)Delegate.Remove(GameEvents.OnPlayerDeleted, new Action<Player>(Left));
		}
	}
	internal static class RewardPatches
	{
		private sealed class DeathCapture
		{
			public Unit Victim;

			public long KillerId;

			public RankedKillBonus Bonus;

			public DeathCapture? Previous;

			public int BeforeBalance;

			public int Paid = -1;

			public bool Announced;
		}

		private const string KohHash = "5377B71A034E065DF76D5A3F595DE733DAF24D9A875DEF9D0C092BCCAAABF8BC";

		private const string GameHash = "33969F93EBBE60C4182E18C590974BCE046BD0F440B7003D888F2E7640B1BCF5";

		private const string LogisticsHash = "C5799FD0CDB2D1B5330CD165FDC5F5088118565E4D51712A06B17C586BFF3329";

		private const string LogisticsReason = "Logistics rewards (batched)";

		private static MethodInfo? addCredits;

		private static MethodInfo? getCredits;

		private static MethodInfo? logisticsGrant;

		private static readonly HashSet<MethodBase> installed = new HashSet<MethodBase>();

		private static readonly Dictionary<string, string> statuses = new Dictionary<string, string>();

		[ThreadStatic]
		private static long lastLogisticsId;

		[ThreadStatic]
		private static int lastLogisticsBase;

		[ThreadStatic]
		private static int lastLogisticsPaid;

		[ThreadStatic]
		private static bool hasLogisticsReceipt;

		private static bool provenanceWarning;

		private static MethodInfo? deathHook;

		private static bool deathHookInstalled;

		private static readonly Dictionary<Unit, DeathCapture> deaths = new Dictionary<Unit, DeathCapture>();

		public static string Status
		{
			get
			{
				if (statuses.Count != 0)
				{
					return string.Join("; ", statuses.Select((KeyValuePair<string, string> p) => p.Key + ": " + p.Value));
				}
				return "Reward patches pending";
			}
		}

		public static void Unbind(Harmony harmony)
		{
			if (deathHookInstalled && deathHook != null)
			{
				try
				{
					harmony.Unpatch((MethodBase)deathHook, Helper("DeathPrefix"));
					harmony.Unpatch((MethodBase)deathHook, Helper("DeathFinalizer"));
					deathHookInstalled = false;
				}
				catch (Exception ex)
				{
					RankLog.Warning("Ranks death capture removal failed: " + ex.GetBaseException().Message);
				}
			}
			deaths.Clear();
			MethodBase[] array = installed.ToArray();
			foreach (MethodBase methodBase in array)
			{
				string name;
				if (methodBase.Name == "PayDueRewards")
				{
					name = "LogisticsChatTranspiler";
				}
				else
				{
					name = ((methodBase.Name == "GrantExternalCredits") ? "ExternalTranspiler" : "CombatTranspiler");
				}
				try
				{
					harmony.Unpatch(methodBase, Helper(name));
					installed.Remove(methodBase);
				}
				catch (Exception ex2)
				{
					RankLog.Warning("Ranks reward patch removal failed for " + methodBase.Name + ": " + ex2.GetBaseException().Message);
				}
			}
			hasLogisticsReceipt = false;
			lastLogisticsId = 0L;
			lastLogisticsBase = 0;
			lastLogisticsPaid = 0;
			statuses.Clear();
			provenanceWarning = false;
		}

		public static void Bind(Harmony harmony)
		{
			Type type = Find("Si_KingOfTheHill.KingOfTheHill");
			if (type == null)
			{
				statuses["KoH"] = "unavailable";
				return;
			}
			if (!Verified(type.Assembly, "5377B71A034E065DF76D5A3F595DE733DAF24D9A875DEF9D0C092BCCAAABF8BC"))
			{
				Warn("KoH", "DLL changed; reward patches disabled until reviewed");
				return;
			}
			addCredits = Method(type, "AddCredits", typeof(void), typeof(long), typeof(int));
			getCredits = Method(type, "GetCredits", typeof(int), typeof(long));
			if (addCredits == null || getCredits == null)
			{
				Warn("KoH", "credit signatures incompatible");
				return;
			}
			BindDeathCapture(harmony);
			Install(harmony, Method(type, "OnUnitDestroyed_Reward", typeof(void), typeof(Unit), typeof(GameObject)), "kills", "CombatTranspiler");
			Install(harmony, Method(type, "OnStructureDestroyed_Reward", typeof(void), typeof(Structure), typeof(GameObject)), "structures", "CombatTranspiler");
			Install(harmony, Method(type, "GrantZonePresenceReward", typeof(void), typeof(Player), typeof(int), typeof(string)), "zone presence", "CombatTranspiler");
			Type type2 = Find("Si_Logistics.LogisticsMod");
			if (type2 == null)
			{
				statuses["Logistics"] = "unavailable";
				return;
			}
			if (!Verified(type2.Assembly, "C5799FD0CDB2D1B5330CD165FDC5F5088118565E4D51712A06B17C586BFF3329"))
			{
				Warn("Logistics", "DLL changed; logistics reward patch disabled until reviewed");
				return;
			}
			logisticsGrant = type2.GetNestedType("KohCredits", BindingFlags.NonPublic)?.GetMethod("TryGrant", BindingFlags.Static | BindingFlags.Public, null, new Type[3]
			{
				typeof(long),
				typeof(int),
				typeof(string)
			}, null);
			if (logisticsGrant == null || !logisticsGrant.ReturnType.IsEnum)
			{
				Warn("Logistics", "credit bridge signature incompatible");
				return;
			}
			MethodInfo methodInfo = Method(type2, "PayDueRewards", typeof(void));
			if (methodInfo == null)
			{
				Warn("Logistics", "payout signature incompatible");
				return;
			}
			Install(harmony, methodInfo, "Logistics announcement", "LogisticsChatTranspiler");
			if (installed.Contains(methodInfo))
			{
				Install(harmony, Method(type, "GrantExternalCredits", typeof(bool), typeof(long), typeof(int), typeof(string)), "Logistics payment", "ExternalTranspiler");
			}
		}

		private static void BindDeathCapture(Harmony harmony)
		{
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Expected Obj, but got Unknown
			//IL_00fb: Expected Obj, but got Unknown
			if (deathHookInstalled)
			{
				return;
			}
			if (!Verified(typeof(DamageManager).Assembly, "33969F93EBBE60C4182E18C590974BCE046BD0F440B7003D888F2E7640B1BCF5"))
			{
				Warn("higher-rank kills", "game DLL changed; death capture disabled until reviewed");
				return;
			}
			deathHook = typeof(DamageManager).GetMethod("OnDamageReceived", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[4]
			{
				typeof(float),
				typeof(GameObject),
				typeof(byte),
				typeof(bool)
			}, null);
			if (deathHook == null || deathHook.ReturnType != typeof(void))
			{
				Warn("higher-rank kills", "death dispatcher signature incompatible");
				return;
			}
			try
			{
				harmony.Patch((MethodBase)deathHook, new HarmonyMethod(typeof(RewardPatches), "DeathPrefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(RewardPatches), "DeathFinalizer", (Type[])null), (HarmonyMethod)null);
				deathHookInstalled = true;
				statuses["higher-rank kills"] = "death capture active";
			}
			catch (Exception ex)
			{
				Warn("higher-rank kills", "death capture unavailable: " + ex.GetBaseException().Message);
			}
		}

		private static Player? DirectPlayer(Unit? unit)
		{
			if (unit == null)
			{
				return null;
			}
			Player controlledBy = unit.ControlledBy;
			if (controlledBy != null && controlledBy.ControlledUnit == unit)
			{
				return controlledBy;
			}
			Unit driver = unit.Driver;
			if (driver == null)
			{
				return null;
			}
			controlledBy = driver.ControlledBy;
			if (!(controlledBy != null) || !(controlledBy.ControlledUnit == driver))
			{
				return null;
			}
			return controlledBy;
		}

		private static void DeathPrefix(DamageManager __instance, float damage, GameObject instigator, out DeathCapture? __state)
		{
			__state = null;
			try
			{
				if (damage == 0f || __instance.Health > 0f || !Game.GetIsServer() || instigator == null)
				{
					return;
				}
				Unit unit = __instance.Owner as Unit;
				BaseGameObject baseGameObject = instigator.GetBaseGameObject();
				Player player = ((!(baseGameObject != null)) ? null : baseGameObject.NetworkComponent?.OwnerPlayer);
				Player player2 = DirectPlayer(unit);
				if (unit == null || baseGameObject == null || player == null || player2 == null || player == player2 || player.Team == null || player2.Team == null || unit.Team == null || !Team.GetTeamsAreEnemy(player.Team, unit.Team) || !Team.GetTeamsAreEnemy(player.Team, player2.Team))
				{
					return;
				}
				long steamID = (long)player.PlayerID.SteamID.m_SteamID;
				long steamID2 = (long)player2.PlayerID.SteamID.m_SteamID;
				if (steamID != 0L && steamID2 != 0L && steamID != steamID2)
				{
					RankedKillBonus rankedKillBonus = RankMod.HigherRankKill(steamID, steamID2);
					if (rankedKillBonus != null)
					{
						deaths.TryGetValue(unit, out DeathCapture value);
						__state = new DeathCapture
						{
							Victim = unit,
							KillerId = steamID,
							Bonus = rankedKillBonus,
							Previous = value
						};
						deaths[unit] = __state;
					}
				}
			}
			catch (Exception ex)
			{
				Warn("higher-rank kills", "snapshot failed: " + ex.GetBaseException().Message);
			}
		}

		private static void DeathFinalizer(DeathCapture? __state)
		{
			if (__state != null && deaths.TryGetValue(__state.Victim, out DeathCapture value) && value == __state)
			{
				if (__state.Previous == null)
				{
					deaths.Remove(__state.Victim);
				}
				else
				{
					deaths[__state.Victim] = __state.Previous;
				}
			}
		}

		private static Type? Find(string name)
		{
			return (from a in AppDomain.CurrentDomain.GetAssemblies()
				select a.GetType(name, throwOnError: false)).FirstOrDefault((Type t) => t != null);
		}

		private static MethodInfo? Method(Type type, string name, Type result, params Type[] args)
		{
			MethodInfo method = type.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);
			if (!(method?.ReturnType == result))
			{
				return null;
			}
			return method;
		}

		private static bool Verified(Assembly assembly, string hash)
		{
			try
			{
				using FileStream inputStream = File.OpenRead(assembly.Location);
				using SHA256 sHA = SHA256.Create();
				return BitConverter.ToString(sHA.ComputeHash(inputStream)).Replace("-", "") == hash;
			}
			catch
			{
				return false;
			}
		}

		private static void Warn(string site, string reason)
		{
			string text = (statuses.TryGetValue(site, out string value) ? value : "");
			statuses[site] = reason;
			if (text != reason)
			{
				RankLog.Warning("Ranks reward patch " + site + ": " + reason + ". Original rewards remain in use.");
			}
		}

		private static void Install(Harmony harmony, MethodInfo? target, string site, string transpiler)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected Obj, but got Unknown
			if (target == null)
			{
				Warn(site, "method missing");
			}
			else if (!installed.Contains(target))
			{
				try
				{
					harmony.Patch((MethodBase)target, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(RewardPatches), transpiler, (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
					installed.Add(target);
					statuses[site] = "active";
					RankLog.Info("Ranks reward patch active: " + site);
				}
				catch (Exception ex)
				{
					Warn(site, "incompatible IL: " + ex.GetBaseException().Message);
				}
			}
		}

		internal static int Calculate(int amount, decimal multiplier, int balance)
		{
			if (amount <= 0)
			{
				return amount;
			}
			decimal val = decimal.Floor((decimal)amount * Math.Max(0m, multiplier));
			decimal val2 = Math.Max(0L, 2147483647L - (long)Math.Max(0, balance));
			return (int)Math.Min(val, val2);
		}

		private static int Scale(long id, int amount, RewardKind kind)
		{
			if (amount <= 0)
			{
				return amount;
			}
			try
			{
				return Calculate(amount, RankMod.RewardPolicy(id, kind), (int)getCredits.Invoke(null, new object[1] { id }));
			}
			catch (Exception ex)
			{
				Warn("policy", "calculation unavailable: " + ex.GetBaseException().Message);
				return amount;
			}
		}

		private static int Kill(long id, int amount, Unit victim)
		{
			if (amount > 0 && victim != null && deaths.TryGetValue(victim, out DeathCapture value) && value.KillerId == id && value.Paid < 0 && RankMod.RankedKillValid(value.Bonus))
			{
				try
				{
					value.BeforeBalance = (int)getCredits.Invoke(null, new object[1] { id });
					value.Paid = Calculate(amount, value.Bonus.Multiplier, value.BeforeBalance);
					return value.Paid;
				}
				catch (Exception ex)
				{
					Warn("higher-rank kills", "override calculation failed: " + ex.GetBaseException().Message);
				}
			}
			return Scale(id, amount, RewardKind.Kill);
		}

		private static void KillPaid(long id, int paid, Unit victim)
		{
			if (paid <= 0 || victim == null || !deaths.TryGetValue(victim, out DeathCapture value) || value.KillerId != id || value.Paid != paid || value.Announced)
			{
				return;
			}
			try
			{
				if ((long)(int)getCredits.Invoke(null, new object[1] { id }) - (long)value.BeforeBalance == paid)
				{
					value.Announced = true;
					RankMod.AnnounceHigherRankKill(value.Bonus, paid);
				}
			}
			catch (Exception ex)
			{
				RankLog.Warning("Ranks higher-rank kill announcement failed: " + ex.GetBaseException().Message);
			}
		}

		private static int Structure(long id, int amount)
		{
			return Scale(id, amount, RewardKind.Structure);
		}

		private static int Zone(long id, int amount)
		{
			return Scale(id, amount, RewardKind.Other);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int External(long id, int amount, string reason)
		{
			if (reason != "Logistics rewards (batched)" || amount <= 0)
			{
				return amount;
			}
			hasLogisticsReceipt = false;
			if (!(logisticsGrant == null))
			{
				StackFrame[] frames = new StackTrace().GetFrames();
				if (frames != null && frames.Any((StackFrame f) => f.GetMethod() == logisticsGrant))
				{
					int result = Scale(id, amount, RewardKind.Logistics);
					statuses["Logistics payment"] = "active";
					lastLogisticsId = id;
					lastLogisticsBase = amount;
					lastLogisticsPaid = result;
					hasLogisticsReceipt = true;
					return result;
				}
			}
			statuses["Logistics payment"] = "matching reason without verified caller; payment unchanged";
			if (!provenanceWarning)
			{
				provenanceWarning = true;
				RankLog.Warning("Ranks: matching external reason without verified Logistics caller; payment left unchanged.");
			}
			return amount;
		}

		private static string LogisticsDisplay(long id, int amount)
		{
			int num = ((hasLogisticsReceipt && id == lastLogisticsId && amount == lastLogisticsBase) ? lastLogisticsPaid : amount);
			hasLogisticsReceipt = false;
			return num.ToString(CultureInfo.InvariantCulture);
		}

		private static MethodInfo Helper(string name)
		{
			return AccessTools.Method(typeof(RewardPatches), name, (Type[])null, (Type[])null);
		}

		private static int LocalIndex(CodeInstruction instruction)
		{
			if (instruction.opcode == OpCodes.Ldloc_0)
			{
				return 0;
			}
			if (instruction.opcode == OpCodes.Ldloc_1)
			{
				return 1;
			}
			if (instruction.opcode == OpCodes.Ldloc_2)
			{
				return 2;
			}
			if (instruction.opcode == OpCodes.Ldloc_3)
			{
				return 3;
			}
			if (instruction.opcode != OpCodes.Ldloc && instruction.opcode != OpCodes.Ldloc_S && instruction.opcode != OpCodes.Ldloca && instruction.opcode != OpCodes.Ldloca_S)
			{
				return -1;
			}
			if (!(instruction.operand is LocalBuilder localBuilder))
			{
				return Convert.ToInt32(instruction.operand, CultureInfo.InvariantCulture);
			}
			return localBuilder.LocalIndex;
		}

		private static void Require(bool valid, string message)
		{
			if (!valid)
			{
				throw new InvalidOperationException(message);
			}
		}

		private static CodeInstruction LocalInstruction(bool store, int index, object? operand)
		{
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Expected Obj, but got Unknown
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Expected Obj, but got Unknown
			if (index >= 4)
			{
				return new CodeInstruction(store ? OpCodes.Stloc : OpCodes.Ldloc, (operand is LocalBuilder) ? operand : ((object)(short)index));
			}
			return new CodeInstruction(((!store) ? new OpCode[4]
			{
				OpCodes.Ldloc_0,
				OpCodes.Ldloc_1,
				OpCodes.Ldloc_2,
				OpCodes.Ldloc_3
			} : new OpCode[4]
			{
				OpCodes.Stloc_0,
				OpCodes.Stloc_1,
				OpCodes.Stloc_2,
				OpCodes.Stloc_3
			})[index], (object)null);
		}

		private static int CreditSite(List<CodeInstruction> code)
		{
			int[] array = (from x in code.Select((CodeInstruction c, int i) => (c: c, i: i))
				where CodeInstructionExtensions.Calls(x.c, addCredits)
				select x.i).ToArray();
			Require(array.Length == 1, "expected exactly one direct AddCredits call");
			return array[0];
		}

		private static void InsertAt(List<CodeInstruction> code, int index, List<CodeInstruction> added)
		{
			Require(code[index].blocks.Count == 0, "unexpected exception boundary at payment operand");
			CodeInstructionExtensions.MoveLabelsFrom(added[0], code[index]);
			code.InsertRange(index, added);
		}

		internal static IEnumerable<CodeInstruction> CombatTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
		{
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Expected Obj, but got Unknown
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Expected Obj, but got Unknown
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Expected Obj, but got Unknown
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Expected Obj, but got Unknown
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Expected Obj, but got Unknown
			//IL_0273: Unknown result type (might be due to invalid IL or missing references)
			//IL_0279: Expected Obj, but got Unknown
			List<CodeInstruction> list = instructions.ToList();
			int num = CreditSite(list);
			Require(num >= 2, "missing payment operands");
			CodeInstruction val = list[num - 1];
			int num2 = LocalIndex(val);
			Require(num2 >= 0 && val.opcode != OpCodes.Ldloca && val.opcode != OpCodes.Ldloca_S, "amount must be an integer local load");
			Require(__originalMethod.GetMethodBody().LocalVariables[num2].LocalType == typeof(int), "amount local is not int");
			CodeInstruction val2 = list[num - 2];
			int num3 = LocalIndex(val2);
			Require((num3 >= 0) ? (__originalMethod.GetMethodBody().LocalVariables[num3].LocalType == typeof(long)) : (val2.opcode == OpCodes.Call && val2.operand is MethodInfo { Name: "GetPlayerSteamId" } methodInfo && methodInfo.ReturnType == typeof(long)), "unrecognized account-id operand");
			string text;
			if (!(__originalMethod.Name == "OnUnitDestroyed_Reward"))
			{
				if (!(__originalMethod.Name == "OnStructureDestroyed_Reward"))
				{
					if (!(__originalMethod.Name == "GrantZonePresenceReward"))
					{
						throw new InvalidOperationException("unrecognized reward handler");
					}
					text = "Zone";
				}
				else
				{
					text = "Structure";
				}
			}
			else
			{
				text = "Kill";
			}
			string text2 = text;
			List<CodeInstruction> list2 = new List<CodeInstruction>
			{
				new CodeInstruction(OpCodes.Dup, (object)null),
				new CodeInstruction(val.opcode, val.operand)
			};
			bool flag = text2 == "Kill";
			if (flag)
			{
				list2.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
			}
			list2.Add(new CodeInstruction(OpCodes.Call, (object)Helper(text2)));
			list2.Add(LocalInstruction(store: true, num2, val.operand));
			InsertAt(list, num - 1, list2);
			if (flag)
			{
				int num4 = CreditSite(list);
				Require(num3 >= 0, "kill account-id must remain a local");
				list.InsertRange(num4 + 1, new CodeInstruction[4]
				{
					LocalInstruction(store: false, num3, val2.operand),
					LocalInstruction(store: false, num2, val.operand),
					new CodeInstruction(OpCodes.Ldarg_0, (object)null),
					new CodeInstruction(OpCodes.Call, (object)Helper("KillPaid"))
				});
			}
			return list;
		}

		internal static IEnumerable<CodeInstruction> ExternalTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Expected Obj, but got Unknown
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Expected Obj, but got Unknown
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Expected Obj, but got Unknown
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Expected Obj, but got Unknown
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Expected Obj, but got Unknown
			List<CodeInstruction> list = instructions.ToList();
			int num = CreditSite(list);
			Require(num >= 2 && list[num - 2].opcode == OpCodes.Ldarg_0 && list[num - 1].opcode == OpCodes.Ldarg_1, "external payment arguments changed");
			InsertAt(list, num - 1, new List<CodeInstruction>
			{
				new CodeInstruction(OpCodes.Dup, (object)null),
				new CodeInstruction(OpCodes.Ldarg_1, (object)null),
				new CodeInstruction(OpCodes.Ldarg_2, (object)null),
				new CodeInstruction(OpCodes.Call, (object)Helper("External")),
				new CodeInstruction(OpCodes.Starg_S, (object)(byte)1)
			});
			return list;
		}

		internal static IEnumerable<CodeInstruction> LogisticsChatTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
		{
			//IL_0360: Unknown result type (might be due to invalid IL or missing references)
			//IL_0366: Expected Obj, but got Unknown
			List<CodeInstruction> list = instructions.ToList();
			int[] array = (from x in list.Select((CodeInstruction c, int i) => (c: c, i: i))
				where CodeInstructionExtensions.Calls(x.c, logisticsGrant)
				select x.i).ToArray();
			Require(array.Length == 1, "expected one Logistics grant call");
			int num = array[0];
			Require(num >= 3 && list[num - 1].opcode == OpCodes.Ldstr && (string)list[num - 1].operand == "Logistics rewards (batched)", "Logistics reason changed");
			int num2 = LocalIndex(list[num - 3]);
			int num3 = LocalIndex(list[num - 2]);
			Require(num2 >= 0 && num3 >= 0 && __originalMethod.GetMethodBody().LocalVariables[num2].LocalType == typeof(long) && __originalMethod.GetMethodBody().LocalVariables[num3].LocalType == typeof(int), "Logistics grant operands changed");
			int[] array2 = (from x in list.Select((CodeInstruction c, int i) => (c: c, i: i))
				where x.c.opcode == OpCodes.Ldstr && object.Equals(x.c.operand, " credits for logistics")
				select x.i).ToArray();
			Require(array2.Length == 1, "Logistics announcement changed");
			int num4 = array2[0];
			Require(num4 > num && num4 >= 2 && LocalIndex(list[num4 - 2]) == num3 && (list[num4 - 2].opcode == OpCodes.Ldloca || list[num4 - 2].opcode == OpCodes.Ldloca_S) && list[num4 - 1].operand is MethodInfo methodInfo && methodInfo.DeclaringType == typeof(int) && methodInfo.Name == "ToString", "Logistics amount display changed");
			int count = ((ICollection)typeof(CodeInstruction).GetField("labels").GetValue(list[num4 - 1])).Count;
			Require(list[num4 - 2].blocks.Count == 0 && list[num4 - 1].blocks.Count == 0 && count == 0, "unexpected announcement control flow");
			CodeInstruction val = LocalInstruction(store: false, num2, list[num - 3].operand);
			CodeInstructionExtensions.MoveLabelsFrom(val, list[num4 - 2]);
			CodeInstruction val2 = LocalInstruction(store: false, num3, list[num4 - 2].operand);
			list.RemoveRange(num4 - 2, 2);
			list.InsertRange(num4 - 2, new CodeInstruction[3]
			{
				val,
				val2,
				new CodeInstruction(OpCodes.Call, (object)Helper("LogisticsDisplay"))
			});
			return list;
		}
	}
	internal static class RankLog
	{
		internal static bool Enabled;

		internal static void Info(string text)
		{
			if (Enabled)
			{
				MelonLogger.Msg(text);
			}
		}

		internal static void Warning(string text)
		{
			if (Enabled)
			{
				MelonLogger.Warning(text);
			}
		}

		internal static void Error(string text)
		{
			if (Enabled)
			{
				MelonLogger.Error(text);
			}
		}
	}
	internal static class RankSounds
	{
		private sealed class Playback
		{
			internal readonly Player Player;

			internal readonly Queue<byte[][]> Pending = new Queue<byte[][]>();

			internal byte[][] Clip;

			internal int Packet;

			internal float Next;

			internal Playback(Player player, byte[][] clip)
			{
				Player = player;
				Clip = clip;
			}
		}

		private const int Frequency = 12000;

		private const int BlockSize = 1024;

		private static readonly Dictionary<string, byte[][]> clips = new Dictionary<string, byte[][]>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<long, Playback> playing = new Dictionary<long, Playback>();

		internal static string Status => "Rank sounds: " + clips.Count + " cached clips.";

		internal static void Initialize(IEnumerable<string> filenames)
		{
			Reset();
			clips.Clear();
			foreach (string item in filenames.Distinct(StringComparer.OrdinalIgnoreCase))
			{
				try
				{
					if (string.IsNullOrWhiteSpace(item) || Path.GetFileName(item) != item)
					{
						throw new InvalidDataException("Expected a filename inside UserData/sounds.");
					}
					string path = Path.Combine(MelonEnvironment.UserDataDirectory, "sounds", item);
					clips[item] = Decode(File.ReadAllBytes(path));
				}
				catch (Exception ex)
				{
					RankLog.Warning("Rank sound disabled: " + item + "; " + ex.Message);
				}
			}
			RankLog.Info(Status);
		}

		internal static void Play(long id, string filename)
		{
			if (!Game.GetIsServer() || Game.GetIsClient() || !clips.TryGetValue(filename, out byte[][] value))
			{
				return;
			}
			Player player = Player.Players.FirstOrDefault((Player p) => (bool)p && p.PlayerID.SteamID.m_SteamID == (ulong)id);
			if (!(player == null) && (bool)player)
			{
				if (playing.TryGetValue(id, out Playback value2) && value2.Player == player)
				{
					value2.Pending.Enqueue(value);
				}
				else
				{
					playing[id] = new Playback(player, value);
				}
			}
		}

		internal static void Tick()
		{
			if (playing.Count == 0)
			{
				return;
			}
			if (!Game.GetIsServer() || Game.GetIsClient())
			{
				Reset();
				return;
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			long[] array = playing.Keys.ToArray();
			foreach (long key in array)
			{
				Playback playback = playing[key];
				if (!playback.Player || !Player.Players.Contains(playback.Player))
				{
					playing.Remove(key);
				}
				else
				{
					if (realtimeSinceStartup < playback.Next)
					{
						continue;
					}
					try
					{
						AudioHelper.SendAudioPacketToPlayer(playback.Player, playback.Clip[playback.Packet], playback.Packet * 1024 + 1);
						playback.Next = realtimeSinceStartup + 0.07f;
						if (++playback.Packet == playback.Clip.Length)
						{
							if (playback.Pending.Count == 0)
							{
								playing.Remove(key);
								continue;
							}
							playback.Clip = playback.Pending.Dequeue();
							playback.Packet = 0;
						}
					}
					catch (Exception ex)
					{
						playing.Remove(key);
						RankLog.Warning("Rank sound delivery failed: " + ex.GetBaseException().Message);
					}
				}
			}
		}

		internal static void Reset()
		{
			playing.Clear();
		}

		private static byte[][] Decode(byte[] bytes)
		{
			if (bytes.Length < 12 || bytes.Length > 16777216 || Tag(bytes, 0) != "RIFF" || Tag(bytes, 8) != "WAVE")
			{
				throw new InvalidDataException("Invalid or oversized WAV file.");
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = -1;
			int num7 = 0;
			int num8 = 12;
			while (num8 <= bytes.Length - 8)
			{
				int num9 = BitConverter.ToInt32(bytes, num8 + 4);
				int num10 = num8 + 8;
				if (num9 < 0 || num9 > bytes.Length - num10)
				{
					throw new InvalidDataException("Truncated WAV chunk.");
				}
				string text = Tag(bytes, num8);
				if (text == "fmt ")
				{
					if (num9 < 16)
					{
						throw new InvalidDataException("Invalid WAV format chunk.");
					}
					num = BitConverter.ToUInt16(bytes, num10);
					num2 = BitConverter.ToUInt16(bytes, num10 + 2);
					num3 = BitConverter.ToInt32(bytes, num10 + 4);
					num5 = BitConverter.ToUInt16(bytes, num10 + 12);
					num4 = BitConverter.ToUInt16(bytes, num10 + 14);
				}
				else if (text == "data" && num6 < 0)
				{
					num6 = num10;
					num7 = num9;
				}
				num8 = checked(num10 + num9 + (num9 & 1));
			}
			if (num6 < 0 || num2 < 1 || num2 > 8 || num3 < 1000 || num3 > 192000 || ((num != 1 || (num4 != 8 && num4 != 16 && num4 != 24 && num4 != 32)) && (num != 3 || num4 != 32)) || num5 != num2 * (num4 / 8) || num7 == 0 || num7 % num5 != 0)
			{
				throw new InvalidDataException("Unsupported WAV: use PCM 8/16/24/32-bit or float 32-bit.");
			}
			int num11 = num7 / num5;
			if ((double)num11 / (double)num3 > 30.0)
			{
				throw new InvalidDataException("Rank cues must be at most 30 seconds.");
			}
			float[] array = new float[num11];
			for (int i = 0; i < num11; i++)
			{
				double num12 = 0.0;
				for (int j = 0; j < num2; j++)
				{
					int num13 = num6 + i * num5 + j * (num4 / 8);
					double num14;
					if (num == 3)
					{
						num14 = BitConverter.ToSingle(bytes, num13);
					}
					else
					{
						switch (num4)
						{
						case 8:
							num14 = (double)(bytes[num13] - 128) / 128.0;
							break;
						case 16:
							num14 = (double)BitConverter.ToInt16(bytes, num13) / 32768.0;
							break;
						case 24:
						{
							int num15 = bytes[num13] | (bytes[num13 + 1] << 8) | (bytes[num13 + 2] << 16);
							if ((num15 & 0x800000) != 0)
							{
								num15 |= -16777216;
							}
							num14 = (double)num15 / 8388608.0;
							break;
						}
						default:
							num14 = (double)BitConverter.ToInt32(bytes, num13) / 2147483648.0;
							break;
						}
					}
					if (double.IsNaN(num14) || double.IsInfinity(num14))
					{
						throw new InvalidDataException("Invalid WAV sample.");
					}
					num12 += num14;
				}
				array[i] = (float)(num12 / (double)num2);
			}
			int num16 = Math.Max(1, (int)((double)num11 * 12000.0 / (double)num3));
			byte[][] array2 = new byte[(num16 + 1024 - 1) / 1024][];
			for (int k = 0; k < array2.Length; k++)
			{
				byte[] array3 = new byte[1024];
				for (int l = 0; l < 1024; l++)
				{
					int num17 = k * 1024 + l;
					if (num17 >= num16)
					{
						array3[l] = 128;
						continue;
					}
					double num18 = (double)num17 * (double)num3 / 12000.0;
					int num19 = Math.Min((int)num18, num11 - 1);
					int num20 = Math.Min(num19 + 1, num11 - 1);
					double num21 = (double)array[num19] + (double)(array[num20] - array[num19]) * (num18 - (double)num19);
					array3[l] = (byte)Math.Max(0, Math.Min(255, (int)(num21 * 128.0 + 128.0)));
				}
				array2[k] = array3;
			}
			return array2;
		}

		private static string Tag(byte[] bytes, int offset)
		{
			return Encoding.ASCII.GetString(bytes, offset, 4);
		}
	}
	internal static class HarvesterRestrictions
	{
		[HarmonyPatch(typeof(FPSCommanding), "InviteToGroupServer", new Type[]
		{
			typeof(Player),
			typeof(Target)
		})]
		private static class Invite
		{
			[HarmonyPrefix]
			[HarmonyPriority(800)]
			private static bool Prefix(Player __0, Target __1)
			{
				if (!Enabled || !Server || !__0 || !__1 || !IsForbidden(__1.OwnerUnit))
				{
					return true;
				}
				Deny(__0);
				return false;
			}
		}

		[HarmonyPatch(typeof(AIGroup), "AddUnit", new Type[] { typeof(Unit) })]
		private static class Recruit
		{
			[HarmonyPrefix]
			[HarmonyPriority(800)]
			private static bool Prefix(AIGroup __instance, Unit __0, ref bool __result)
			{
				if (!Enabled || !Server || !IsForbidden(__0) || __0.PlayerControlled || ((bool)__0.Driver && (bool)__0.Driver.ControlledBy) || !__instance.Leader || !__instance.Leader.ControlledBy || __instance.Units.Contains(__0))
				{
					return true;
				}
				Player controlledBy = __instance.Leader.ControlledBy;
				if (controlledBy.Group != __instance || controlledBy.ControlledUnit == __0)
				{
					return true;
				}
				__result = false;
				Deny(controlledBy);
				return false;
			}
		}

		internal static bool Enabled = true;

		private static readonly Dictionary<ulong, float> nextNotice = new Dictionary<ulong, float>();

		private static bool Server
		{
			get
			{
				if (Game.GetIsServer())
				{
					return !Game.GetIsClient();
				}
				return false;
			}
		}

		internal static bool IsForbidden(ObjectInfo info)
		{
			if ((bool)info)
			{
				return info.UnitType == UnitType.Harvester;
			}
			return false;
		}

		private static bool IsForbidden(Unit unit)
		{
			if ((bool)unit)
			{
				return IsForbidden(unit.ObjectInfo);
			}
			return false;
		}

		internal static void Reset()
		{
			nextNotice.Clear();
		}

		private static void Deny(Player player)
		{
			if (!player)
			{
				return;
			}
			ulong steamID = player.PlayerID.SteamID.m_SteamID;
			if (steamID == 0L)
			{
				return;
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			try
			{
				if (!nextNotice.TryGetValue(steamID, out var value) || realtimeSinceStartup >= value)
				{
					nextNotice[steamID] = realtimeSinceStartup + 2f;
					HelperMethods.SendChatMessageToPlayer(player, "[Ranks] ", "Harvesters and Shrimp cannot be recruited into an FPS squad. Direct control is allowed.");
				}
			}
			catch (Exception ex)
			{
				RankLog.Error("Harvester restriction notification failed: " + ex.Message);
			}
		}
	}
}
