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
using System.Text.RegularExpressions;
using System.Threading;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using MelonLoader.Utils;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pathfinding;
using Si_Formation;
using Silica.AI;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(Formations), "Si_Formation", "2.15.1", "Local", null)]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_Formation")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("2.15.1.0")]
[assembly: AssemblyInformationalVersion("2.15.1+212b25716a90cc46cb1ab27d8c84901c01831354")]
[assembly: AssemblyProduct("Si_Formation")]
[assembly: AssemblyTitle("Si_Formation")]
[assembly: AssemblyVersion("2.15.1.0")]
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
namespace Si_Formation
{
	public sealed class Formations : MelonMod
	{
		private sealed class Selection
		{
			internal Team Team;

			internal bool PersonalEnabled;

			internal bool CommanderRole;

			internal bool FollowFacingLock;

			internal readonly Dictionary<string, FormationDefinition> Functions = new Dictionary<string, FormationDefinition>();
		}

		private sealed class Menu
		{
			internal Team Team;

			internal bool Commander;

			internal string Function;

			internal FormationDefinition[] Choices = Array.Empty<FormationDefinition>();
		}

		private sealed class RallyFactory
		{
			internal Structure Factory;

			internal Vector3 DefaultPoint;

			internal Vector3 RequestedPoint;

			internal Vector3 Marker;

			internal bool Explicit;

			internal bool OwnsMarker;

			internal RallyArea Area;
		}

		private sealed class RallyArea
		{
			internal RallyFactory State;

			internal Team Team;

			internal Vector3 Anchor;

			internal Vector3 Forward;

			internal string Shape;

			internal float Spacing;

			internal float Radius;

			internal bool Automatic;

			internal IEnumerator<Vector3> Positions;

			internal RallySpot Preview;
		}

		private sealed class RallyTicket
		{
			internal Unit Unit;

			internal RallyArea Area;

			internal RallySpot Spot;

			internal float Created;

			internal long Serial;

			internal bool Assigned;

			internal bool Arrived;
		}

		private sealed class RallySpot
		{
			internal Vector3 Point;
		}

		private sealed class PlayerPolicy
		{
			internal string Owner;

			internal int Limit;

			internal string[] Categories;

			internal string[] Allowed;

			internal string[] Denied;
		}

		private sealed class Placement
		{
			internal Unit Unit;

			internal int Slot;

			internal float Radius;

			internal Vector3 Point;

			internal uint OrderId;
		}

		private sealed class AttackOperation
		{
			internal Player Player;

			internal Team Team;

			internal AIGroup Group;

			internal Target Target;

			internal bool HadTarget;

			internal Vector3 OriginalPosition;

			internal float Deadline;

			internal readonly List<Placement> Members = new List<Placement>();
		}

		private sealed class RetryUnit
		{
			internal Unit Unit;

			internal AIGroup Group;

			internal uint[] Orders;
		}

		private sealed class PlacementRetry
		{
			internal Player Player;

			internal Team Team;

			internal AIGroup Group;

			internal bool Commander;

			internal string Function;

			internal FormationDefinition Definition;

			internal Vector3 Centre;

			internal Vector3 Forward;

			internal Vector3 TravelForward;

			internal AgentMoveSpeed Speed;

			internal int Attempt;

			internal float NextAttempt;

			internal AttackOperation Attack;

			internal SoftGroup Soft;

			internal bool TwoStage;

			internal PlacementCache Cache;

			internal IEnumerator<bool> Search;

			internal readonly List<RetryUnit> Pending = new List<RetryUnit>();

			internal readonly List<Placement> Reserved = new List<Placement>();
		}

		private sealed class PlacementCache
		{
			private sealed class UnitFacts
			{
				internal string Id;

				internal string Type;

				internal float Radius;

				internal readonly Dictionary<int, int> Ranks = new Dictionary<int, int>();

				internal readonly Dictionary<int, bool> Allowed = new Dictionary<int, bool>();
			}

			private readonly FormationDefinition definition;

			private readonly Vector3 centre;

			private readonly Vector3 forward;

			private readonly bool mirrored;

			private readonly Func<int, Vector3> pointOverride;

			private readonly Func<Unit, int, bool> candidateAllowed;

			internal readonly bool NearestFirst;

			private readonly Dictionary<int, Vector3> points = new Dictionary<int, Vector3>();

			private readonly Dictionary<int, int> colours = new Dictionary<int, int>();

			private readonly Dictionary<Unit, UnitFacts> units = new Dictionary<Unit, UnitFacts>();

			internal PlacementCache(FormationDefinition f, Vector3 c, Vector3 heading, bool mirror, Func<int, Vector3> pointOverride = null, Func<Unit, int, bool> candidateAllowed = null, bool nearestFirst = false)
			{
				definition = f;
				centre = c;
				forward = heading;
				mirrored = mirror;
				this.pointOverride = pointOverride;
				this.candidateAllowed = candidateAllowed;
				NearestFirst = nearestFirst;
			}

			private UnitFacts Facts(Unit u)
			{
				if (!units.TryGetValue(u, out var value))
				{
					Dictionary<Unit, UnitFacts> dictionary = units;
					UnitFacts unitFacts = new UnitFacts
					{
						Id = UnitId(u),
						Type = PreferenceType(u),
						Radius = Radius(u, definition, null)
					};
					value = unitFacts;
					dictionary[u] = unitFacts;
				}
				return value;
			}

			internal float UnitRadius(Unit u)
			{
				return Facts(u).Radius;
			}

			internal Vector3 Point(int slot)
			{
				if (!points.TryGetValue(slot, out var value))
				{
					value = (points[slot] = ((pointOverride != null) ? pointOverride(slot) : World(definition.Slots[slot], definition, centre, forward, mirrored)));
				}
				return value;
			}

			internal int Colour(int slot)
			{
				if (!colours.TryGetValue(slot, out var value))
				{
					value = (colours[slot] = FormationDefinition.Colour(definition.Slots[slot]));
				}
				return value;
			}

			internal int Rank(Unit u, int slot)
			{
				UnitFacts unitFacts = Facts(u);
				if (!unitFacts.Ranks.TryGetValue(slot, out var value))
				{
					value = (unitFacts.Ranks[slot] = FormationDefinition.Match(definition.Slots[slot], unitFacts.Id, unitFacts.Type));
				}
				return value;
			}

			internal bool Allows(Unit u, int slot)
			{
				if (candidateAllowed != null && !candidateAllowed(u, slot))
				{
					return false;
				}
				UnitFacts unitFacts = Facts(u);
				if (!unitFacts.Allowed.TryGetValue(slot, out var value))
				{
					value = (unitFacts.Allowed[slot] = FormationDefinition.Allows(definition.Slots[slot], unitFacts.Id, unitFacts.Type));
				}
				return value;
			}
		}

		private sealed class SoftGroup
		{
			internal Player Player;

			internal Team Team;

			internal bool Commander;

			internal bool Mirrored;

			internal Vector3 Forward;

			internal string Function;

			internal FormationDefinition Definition;

			internal readonly Dictionary<Unit, int> Slots = new Dictionary<Unit, int>();

			internal readonly Dictionary<Unit, AIGroup> Groups = new Dictionary<Unit, AIGroup>();
		}

		private sealed class SoftMotion
		{
			internal PlacementRetry Batch;

			internal Placement Placement;

			internal AIGroup Group;

			internal uint[] Previous;

			internal float Departure = float.PositiveInfinity;

			internal bool Initial;
		}

		private sealed class DepartureMember
		{
			internal SoftMotion Motion;

			internal int Stage;

			internal bool Departed;
		}

		private sealed class DepartureSpeed
		{
			internal PlacementRetry Batch;

			internal int LastStage;

			internal readonly List<DepartureMember> Members = new List<DepartureMember>();
		}

		private sealed class LockHeading
		{
			internal Player Owner;

			internal Team Team;

			internal AIGroup Group;

			internal Vector3 Forward;
		}

		private sealed class Follower
		{
			internal Unit Unit;

			internal Unit Leader;

			internal FormationDefinition Definition;

			internal int Slot = -1;

			internal int Attempts;

			internal bool Active;

			internal float NextAttempt;

			internal float Radius;

			internal Vector3 Destination;

			internal Vector3 Anchor;

			internal Vector3 RequestedAnchor;

			internal Vector3 RequestedForward;

			internal bool Refresh;

			internal bool RequestedMirrored;

			internal int RefreshAttempts;

			internal float RefreshAt;

			internal uint Order;

			internal float OutsideSince = -1f;

			internal bool Recovering;

			internal float DepartAt;

			internal int Phase;

			internal readonly HashSet<int> Rejected = new HashSet<int>();
		}

		private sealed class FollowFrame
		{
			internal Vector3 LastSample;

			internal Vector3 Anchor;

			internal Vector3 Forward;

			internal bool Holding;

			internal bool Mirrored;

			internal FormationDefinition HeadingDefinition;

			internal readonly Dictionary<Unit, int> Rows = new Dictionary<Unit, int>();

			internal float DepartureStart;

			internal float DepartureDelay;

			internal IEnumerator<bool> Search;
		}

		private sealed class FpsTravelMember
		{
			internal Placement Placement;

			internal AIGroup Group;

			internal float OutsideSince = -1f;

			internal bool Recovering;
		}

		private sealed class FpsTravel
		{
			internal PlacementRetry Batch;

			internal readonly List<FpsTravelMember> Members = new List<FpsTravelMember>();
		}

		private sealed class PaceSample
		{
			internal Unit Unit;

			internal float Position;

			internal float Offset;

			internal float Radius;

			internal float OutsideSince = -1f;

			internal bool Recovering;

			internal Action<bool, float> Save;
		}

		private sealed class FpsMoveTask
		{
			internal Player Owner;

			internal AITaskBase Task;

			internal readonly HashSet<Unit> Units = new HashSet<Unit>();
		}

		private sealed class LockedMember
		{
			internal Unit Unit;

			internal AIGroup OriginalGroup;

			internal Vector3 Offset;

			internal Vector3 LastDestination;

			internal float OriginalScale;

			internal float Maximum;

			internal float Radius;

			internal uint Order;

			internal bool HasDestination;

			internal float DepartAt;

			internal uint[] Previous;
		}

		private sealed class LockedGroup
		{
			internal Player Owner;

			internal Team Team;

			internal readonly List<LockedMember> Members = new List<LockedMember>();

			internal Vector3 Destination;

			internal Vector3 TravelForward;

			internal float Angle;

			internal float Baseline;

			internal float NextTick;

			internal bool Traveling;

			internal AgentMoveSpeed Speed;
		}

		private sealed class CommanderOptions
		{
			internal Team Team;

			internal float Tolerance;

			internal float Falloff;

			internal int Pending;
		}

		private sealed class ForeignMenu
		{
			internal string Name;

			internal FieldInfo States;

			internal FieldInfo Level;

			internal bool ProbeFailed;
		}

		internal const int GroupLimit = 50;

		internal const int ProjectionAttempts = 6;

		private static readonly FormationCatalog Catalog = new FormationCatalog();

		private static readonly Dictionary<Player, Selection> Selections = new Dictionary<Player, Selection>();

		private static readonly HashSet<Unit> Superseded = new HashSet<Unit>();

		private static bool Ready;

		private static bool Enabled;

		private static bool Faulted;

		private static bool debug;

		private static int generatedDepth;

		private static int traceBudget = 80;

		private static float nextCheck;

		private static MelonPreferences_Category prefs;

		private static Player networkSender;

		private readonly List<string> registered = new List<string>();

		private static readonly Dictionary<Player, Menu> Menus = new Dictionary<Player, Menu>();

		private static string panelEpoch = Guid.NewGuid().ToString("N");

		private static string preferenceLayout;

		private const int BuiltInLimit = 10;

		private const int MaximumLimit = 50;

		private static MelonPreferences_Entry<bool> enabledDefault;

		private static MelonPreferences_Entry<bool> debugDefault;

		private static MelonPreferences_Entry<int> groupLimitDefault;

		private static MelonPreferences_Entry<int> commanderOrderLimit;

		private static readonly Dictionary<string, MelonPreferences_Entry<int>> formationDefaults = new Dictionary<string, MelonPreferences_Entry<int>>();

		private static readonly Dictionary<string, FormationDefinition> matchFormationDefaults = new Dictionary<string, FormationDefinition>();

		private static int serverThread;

		private static int configuredLimit = 10;

		private static bool configuredEnabled = true;

		private static bool adminGlobal;

		private static int? adminMatchLimit;

		private static int? apiMatchLimit;

		private static readonly Dictionary<ulong, int> AdminLimits = new Dictionary<ulong, int>();

		private static readonly Dictionary<ulong, int> ApiLimits = new Dictionary<ulong, int>();

		private static MelonPreferences_Entry<bool> rallyFixDefault;

		private static Structure productionRallyFactory;

		private static Vehicle exitingRallyVehicle;

		private static bool issuingRally;

		private static bool movingRallyMarker;

		private static IEnumerator<bool> rallySearch;

		private static RallyTicket searchingRallyTicket;

		private static float nextRallyPrune;

		private static long rallySerial;

		private static readonly Dictionary<Unit, RallyTicket> RallyTickets = new Dictionary<Unit, RallyTicket>();

		private static readonly Dictionary<Structure, RallyFactory> RallyFactories = new Dictionary<Structure, RallyFactory>();

		private static bool rallyClearance;

		private static Vector3 rallyProjectedPoint;

		private static readonly Dictionary<ulong, PlayerPolicy> PlayerPolicies = new Dictionary<ulong, PlayerPolicy>();

		private static readonly List<AttackOperation> Operations = new List<AttackOperation>();

		private static readonly List<PlacementRetry> PlacementRetries = new List<PlacementRetry>();

		private static int placementCursor;

		private static readonly List<SoftGroup> SoftGroups = new List<SoftGroup>();

		private static readonly List<SoftMotion> SoftMotions = new List<SoftMotion>();

		private static int motionCursor;

		private static readonly List<DepartureSpeed> DepartureSpeeds = new List<DepartureSpeed>();

		private static readonly Dictionary<Unit, float> SoftSpeedCaps = new Dictionary<Unit, float>();

		private static readonly Dictionary<Unit, float> FollowDepartureCaps = new Dictionary<Unit, float>();

		private static readonly Dictionary<Unit, LockHeading> LockHeadings = new Dictionary<Unit, LockHeading>();

		private static MelonPreferences_Entry<float> softDelayDefault;

		private static MelonPreferences_Entry<float> lockDelayDefault;

		private static MelonPreferences_Entry<float> speedLockDelayDefault;

		private static float? softDelayOverride;

		private static float? lockDelayOverride;

		private static float? speedLockDelayOverride;

		private static readonly Dictionary<Unit, Follower> Followers = new Dictionary<Unit, Follower>();

		private static readonly Dictionary<Unit, FollowFrame> Frames = new Dictionary<Unit, FollowFrame>();

		private static int followSearchCursor;

		private static readonly List<FpsTravel> FpsTravels = new List<FpsTravel>();

		private static readonly Dictionary<Unit, float> FpsSpeedCaps = new Dictionary<Unit, float>();

		private static float nextFpsPace;

		private static readonly Dictionary<AIGroup, FpsMoveTask> FpsMoveTasks = new Dictionary<AIGroup, FpsMoveTask>();

		private static Action<AIGroup> poolArmyGroup;

		private static bool footprintsExported;

		private static readonly List<LockedGroup> Locks = new List<LockedGroup>();

		private static readonly Dictionary<Unit, LockedGroup> LockedUnits = new Dictionary<Unit, LockedGroup>();

		private static readonly Dictionary<Unit, float> LockedSpeedCaps = new Dictionary<Unit, float>();

		private static readonly Dictionary<Player, CommanderOptions> CommanderSettings = new Dictionary<Player, CommanderOptions>();

		private static MelonPreferences_Entry<int> travelDefault;

		private static MelonPreferences_Entry<int> toleranceDefault;

		private static MelonPreferences_Entry<int> falloffDefault;

		private static int? moderatorTolerance;

		private static int? moderatorFalloff;

		private static MelonPreferences_Entry<bool> speedCapDefault;

		private static bool? speedCapOverride;

		private static readonly List<ForeignMenu> ForeignMenus = new List<ForeignMenu>();

		private static bool menusProbed;

		private static bool Running
		{
			get
			{
				if (Ready && Enabled && !Faulted)
				{
					return Game.GetIsServer();
				}
				return false;
			}
		}

		private static string PreferencesPath => System.IO.Path.Combine(MelonEnvironment.UserDataDirectory, "Formations_cfg", "si_formation.cfg");

		internal static int CommanderOrderLimit
		{
			get
			{
				if (commanderOrderLimit == null || !ValidCommanderLimit(commanderOrderLimit.Value))
				{
					return 75;
				}
				return commanderOrderLimit.Value;
			}
		}

		internal static bool ApiReady
		{
			get
			{
				if (Ready)
				{
					return Game.GetIsServer();
				}
				return false;
			}
		}

		internal static bool ApiEnabled => Running;

		internal static bool GlobalRequested => Enabled;

		internal static int MatchLimit => adminMatchLimit ?? apiMatchLimit ?? configuredLimit;

		private static bool RallyEnabled
		{
			get
			{
				if (Running)
				{
					return rallyFixDefault?.Value ?? false;
				}
				return false;
			}
		}

		private static float SoftDelay
		{
			get
			{
				if (!ValidDelay(softDelayOverride ?? softDelayDefault.Value))
				{
					return 0.2f;
				}
				return softDelayOverride ?? softDelayDefault.Value;
			}
		}

		private static float SpeedLockDelay
		{
			get
			{
				if (!ValidDelay(speedLockDelayOverride ?? speedLockDelayDefault.Value))
				{
					return 0.3f;
				}
				return speedLockDelayOverride ?? speedLockDelayDefault.Value;
			}
		}

		private static float LockDelay
		{
			get
			{
				if (!(SpeedLockDelay > 0f))
				{
					if (!ValidDelay(lockDelayOverride ?? lockDelayDefault.Value))
					{
						return 0f;
					}
					return lockDelayOverride ?? lockDelayDefault.Value;
				}
				return 0f;
			}
		}

		private static int LockTravelMaximum => (int)ValidOption(travelDefault?.Value ?? 1000, 0, 10000, 1000);

		private static bool SpeedCapEnabled => speedCapOverride ?? speedCapDefault?.Value ?? true;

		private static void DebugMsg(string text)
		{
			if (debug)
			{
				MelonLogger.Msg(text);
			}
		}

		private static void DebugWarning(string text)
		{
			if (debug)
			{
				MelonLogger.Warning(text);
			}
		}

		private static void DebugError(string text)
		{
			if (debug)
			{
				MelonLogger.Error(text);
			}
		}

		public override void OnLateInitializeMelon()
		{
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0247: Expected Obj, but got Unknown
			try
			{
				if (AppDomain.CurrentDomain.GetAssemblies().Any((Assembly a) => a.GetName().Name == "Si_FollowFormation" || a.GetName().Name == "Si_Formations"))
				{
					throw new InvalidOperationException("Remove Mods/Si_FollowFormation.dll and Mods/Si_Formations.dll before loading Si_Formation.dll; both must not run together.");
				}
				if (AdminMethods.AdminCommands == null || PlayerMethods.PlayerCommands == null)
				{
					throw new InvalidOperationException("Si-AdminMod is unavailable.");
				}
				string[] array = new string[6] { "formation", "followformation", "formationsize", "formationsizeglobal", "formationsizecommander", "disband" };
				foreach (string text in array)
				{
					if (AdminMethods.FindAdminCommandFromString(text) != null || PlayerMethods.FindPlayerCommandFromString(text) != null)
					{
						throw new InvalidOperationException("Command already registered: " + text);
					}
				}
				serverThread = Thread.CurrentThread.ManagedThreadId;
				InstallPreferenceLayout();
				prefs = MelonPreferences.CreateCategory("Si_Formation");
				prefs.SetFilePath(System.IO.Path.Combine(MelonEnvironment.UserDataDirectory, "Formations_cfg", "si_formation.cfg"));
				enabledDefault = prefs.CreateEntry<bool>("Enabled", true, (string)null, "Default master state restored each match; /formation on|off is match-only", false, false, (ValueValidator)null, (string)null);
				groupLimitDefault = prefs.CreateEntry<int>("GroupLimit", 10, (string)null, "Default FPS group maximum including leader, 1..50; /formationsize overrides are match-only", false, false, (ValueValidator)null, (string)null);
				debugDefault = prefs.CreateEntry<bool>("Debug", false, (string)null, "Si_Formation bounded diagnostic logging", false, false, (ValueValidator)null, (string)null);
				debug = debugDefault.Value;
				EnsureBundledFormations();
				Catalog.Load(System.IO.Path.Combine(MelonEnvironment.UserDataDirectory, "Formations_cfg"), DebugMsg);
				RegisterFormationDefaults();
				RegisterCommanderOptions();
				InstallRallyFix();
				InstallCommanderHandoverFix();
				commanderOrderLimit = prefs.CreateEntry<int>("CommanderOrderLimit", 75, (string)null, "Commander formation capacity, 1..100; excess and unsupported units stay native; independent of FPS GroupLimit", false, false, (ValueValidator)null, (string)null);
				if (!ValidCommanderLimit(commanderOrderLimit.Value))
				{
					DebugWarning("Invalid CommanderOrderLimit; using 75.");
					commanderOrderLimit.Value = 75;
				}
				ResetMatchSettings();
				Patch(typeof(Event_Chat), "FireOnRequestPlayerChatEvent", "MenuChat");
				HarmonyMethod val = new HarmonyMethod(typeof(Formations), "HostMenuChat", (Type[])null)
				{
					priority = 800
				};
				((MelonBase)this).HarmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(Player), "SendChatMessage", (Type[])null, (Type[])null), val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				PatchAdminInviteLimit();
				Patch(typeof(FPSCommanding), "InviteToGroupServer", "RecruitAuthorized", null, "InviteLimit");
				Patch(typeof(AIGroup), "AddUnit", "GroupAdding");
				Patch(typeof(AIGroup), "EvaluateLeaderFollow", null, null, "PatchFollow");
				Patch(typeof(AIGroup), "EvaluateMovingTask", null, null, "PatchFpsMoveTask");
				Patch(typeof(AIOrderProcessor), "IssueOrder", "BeforeOtherOrder", "OtherOrder");
				Patch(typeof(Unit), "get_MoveSpeedLimit", null, "LockedSpeedLimit");
				Patch(typeof(AIGroup), "IgnoreTask", null, "FollowChanged");
				Patch(typeof(AIGroup), "RemoveUnit", "GroupRemoving");
				Patch(typeof(AIGroup), "set_Task", "GroupTaskChanging");
				Patch(typeof(FPSCommanding), "MoveServer", "FpsMove");
				Patch(typeof(FPSCommanding), "AttackServer", "FpsAttack");
				Patch(typeof(FPSCommanding), "StopAllServer", "FpsToggleGroup");
				Patch(typeof(FPSCommanding), "FollowAllServer", "FpsToggleGroup");
				array = new string[4] { "StopUnitServer", "UnitFollowServer", "ProtectServer", "KickFromGroupServer" };
				foreach (string method in array)
				{
					Patch(typeof(FPSCommanding), method, "FpsNewOrder");
				}
				Patch(typeof(StrategyMode), "PerformMoveAttack", "CommanderMove");
				Patch(typeof(NetworkLayer), "ProcessMessage", "NetworkBegin", null, null, "NetworkEnd");
				GameEvents.OnPlayerChangedUnit = (Action<Player, Unit, Unit>)Delegate.Combine(GameEvents.OnPlayerChangedUnit, new Action<Player, Unit, Unit>(PlayerChangedUnit));
				GameEvents.OnPlayerChangedTeam = (Action<Player, Team, Team>)Delegate.Combine(GameEvents.OnPlayerChangedTeam, new Action<Player, Team, Team>(PlayerChangedTeam));
				GameEvents.OnGameStarted = (Action<GameMode>)Delegate.Combine(GameEvents.OnGameStarted, new Action<GameMode>(RoundStarted));
				GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Combine(GameEvents.OnGameEnded, new Action<GameMode, Team>(CommanderRoundEnded));
				PlayerMethods.RegisterPlayerCommand("formation", MasterCommand, hideFromChat: true);
				PlayerMethods.RegisterPlayerCommand("followformation", FollowOrientationCommand, hideFromChat: true);
				PlayerMethods.RegisterPlayerCommand("formationsize", SizeCommand, hideFromChat: true);
				PlayerMethods.RegisterPlayerCommand("formationsizeglobal", GlobalSizeCommand, hideFromChat: true);
				PlayerMethods.RegisterPlayerCommand("disband", DisbandCommand, hideFromChat: true);
				PlayerMethods.RegisterPlayerCommand("formationsizecommander", CommanderSizeCommand, hideFromChat: true);
				registered.AddRange(new string[6] { "formation", "followformation", "formationsize", "formationsizeglobal", "formationsizecommander", "disband" });
				Ready = true;
				FormatPreferences();
				DebugMsg("Si_Formation ready. Master " + (Enabled ? "ON" : "OFF") + "; team/role formation defaults configured in preferences. Cached JSON is read only at startup.");
			}
			catch (Exception e)
			{
				Fault(e);
				((MelonBase)this).HarmonyInstance.UnpatchSelf();
			}
		}

		private void Patch(Type type, string method, string prefix = null, string postfix = null, string transpiler = null, string finalizer = null)
		{
			MethodInfo methodInfo = AccessTools.Method(type, method, (Type[])null, (Type[])null) ?? throw new MissingMethodException(type.FullName, method);
			((MelonBase)this).HarmonyInstance.Patch((MethodBase)methodInfo, H(prefix), H(postfix), H(transpiler), H(finalizer), (HarmonyMethod)null);
			static HarmonyMethod H(string n)
			{
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0015: Expected Obj, but got Unknown
				if (n != null)
				{
					return new HarmonyMethod(typeof(Formations), n, (Type[])null);
				}
				return null;
			}
		}

		private static void NetworkBegin(NetworkID remoteID, bool isServer, out Player __state)
		{
			__state = networkSender;
			networkSender = (isServer ? Player.FindPlayer(remoteID) : null);
		}

		private static Exception NetworkEnd(Exception __exception, Player __state)
		{
			networkSender = __state;
			return __exception;
		}

		private static string TeamName(Team team)
		{
			if (!team)
			{
				return null;
			}
			switch (team.name)
			{
			case "Team_Human_Sol":
				return "Sol";
			case "Team_Human_Centauri":
				return "Centauri";
			case "Team_Alien":
				return "Alien";
			default:
				if (!new string[3] { "Sol", "Centauri", "Alien" }.Contains(team.TeamShortName))
				{
					return null;
				}
				return team.TeamShortName;
			}
		}

		private static Selection For(Player player)
		{
			if (!Selections.TryGetValue(player, out var value) || value.Team != player.Team || value.CommanderRole != player.IsCommander)
			{
				ReleasePlayerControl(player);
				Menus.Remove(player);
				Dictionary<Player, Selection> selections = Selections;
				Selection selection = new Selection
				{
					Team = player.Team,
					CommanderRole = player.IsCommander
				};
				value = selection;
				selections[player] = selection;
				ApplyFormationDefaults(value);
			}
			return value;
		}

		private static bool FunctionActive(Player p, string function)
		{
			if ((bool)p && Running && (p.IsCommander || For(p).PersonalEnabled) && (p.IsCommander ? (function == "commander") : (function != "commander")))
			{
				return Selected(p, function) != null;
			}
			return false;
		}

		private static FormationDefinition Selected(Player p, string function)
		{
			if (!p)
			{
				return null;
			}
			if (!For(p).Functions.TryGetValue(function, out var value) || !(value.Team == TeamName(p.Team)) || !(value.Function == function))
			{
				return null;
			}
			return value;
		}

		private static void RoundStarted(GameMode mode)
		{
			panelEpoch = Guid.NewGuid().ToString("N");
			speedCapOverride = null;
			softDelayOverride = (lockDelayOverride = (speedLockDelayOverride = null));
			if (!Ready || !Game.GetIsServer())
			{
				return;
			}
			ClearFollow();
			ClearOrders();
			Selections.Clear();
			Menus.Clear();
			ResetMatchSettings();
			nextCheck = 0f;
			EnforceLimits();
			foreach (Player player in Player.Players)
			{
				if ((bool)player)
				{
					For(player);
				}
			}
		}

		private static void MoveReply(Player p, string text)
		{
			Trace(text);
		}

		private static void Reply(Player p, string text)
		{
			if ((bool)p)
			{
				HelperMethods.SendChatMessageToPlayer(p, "Formations: " + text);
			}
			else
			{
				DebugMsg("Formations: " + text);
			}
		}

		private static string SelectedName(Player p, string function)
		{
			return Selected(p, function)?.Name ?? "Default (native)";
		}

		private static void ReportMaster(Player p)
		{
			Reply(p, "Si_Formation global requested " + (Enabled ? "ON" : "OFF") + "; effective " + (Running ? "ON" : "OFF") + "; " + Catalog.Definitions.Count + " cached definitions" + ((!Ready) ? "; initialization failed" : (Faulted ? "; faulted (see server log)" : ((!Game.GetIsServer()) ? "; server unavailable" : ""))));
		}

		private static void MasterCommand(Player p, string args)
		{
			if (!CommandArgument.TryParse(args, out var name, out var quoted) | quoted)
			{
				Reply(p, "usage: /formation on|off|status");
				return;
			}
			if (name.Length == 0)
			{
				ToggleMenu(p);
				return;
			}
			if (name.Equals("help", StringComparison.OrdinalIgnoreCase))
			{
				FormationHelp(p);
				return;
			}
			if (name.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[0].Equals("rallyfix", StringComparison.OrdinalIgnoreCase))
			{
				RallyFixCommand(p, name);
				return;
			}
			if (name.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[0].Equals("debug", StringComparison.OrdinalIgnoreCase))
			{
				FormationDebugCommand(p, name);
				return;
			}
			if (name.Equals("lock", StringComparison.OrdinalIgnoreCase) || name.Equals("unlock", StringComparison.OrdinalIgnoreCase))
			{
				LockCommand(p, name);
				return;
			}
			if (name.Equals("status", StringComparison.OrdinalIgnoreCase))
			{
				PersonalStatus(p);
				return;
			}
			if (!name.Equals("on", StringComparison.OrdinalIgnoreCase) && !name.Equals("off", StringComparison.OrdinalIgnoreCase))
			{
				HandleShortcut(p, name);
				return;
			}
			if (!IsAdministrator(p))
			{
				Reply(p, "permission denied");
				return;
			}
			if (!Ready || !Game.GetIsServer())
			{
				Reply(p, "not initialized as server");
				return;
			}
			adminGlobal = true;
			SetGlobal(name.Equals("on", StringComparison.OrdinalIgnoreCase));
			ReportMaster(p);
		}

		private static IEnumerable<CodeInstruction> PatchFollow(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList();
			MethodInfo methodInfo = AccessTools.Method(typeof(AIOrderProcessor), "IssueResolvedOrder", (Type[])null, (Type[])null);
			MethodInfo operand = AccessTools.Method(typeof(Formations), "VanillaFollow", (Type[])null, (Type[])null);
			int num = 0;
			foreach (CodeInstruction item in list)
			{
				if (CodeInstructionExtensions.Calls(item, methodInfo))
				{
					item.opcode = OpCodes.Call;
					item.operand = operand;
					num++;
				}
			}
			if (num != 2)
			{
				throw new InvalidOperationException("Expected two vanilla Follow repath sites.");
			}
			return list;
		}

		private static bool VanillaFollow(AIOrderProcessor processor, Vector3 position, Target target, in OrderIssueParams parameters)
		{
			if (Running && Enabled && processor.Owner is Unit key && Followers.TryGetValue(key, out var value) && Current(value) && value.Active)
			{
				return true;
			}
			generatedDepth++;
			try
			{
				return processor.IssueResolvedOrder(position, target, in parameters);
			}
			finally
			{
				generatedDepth--;
			}
		}

		private static void BeforeOtherOrder(AIOrderProcessor __instance)
		{
			if (generatedDepth == 0 && __instance.Owner is Unit u)
			{
				ReleaseLockedUnit(u);
			}
		}

		private static void OtherOrder(AIOrderProcessor __instance, bool __result)
		{
			RallyOtherOrder(__instance, __result);
			if (generatedDepth <= 0 && __result && __instance.Owner is Unit unit)
			{
				CancelUnit(unit);
				Followers.Remove(unit);
				if (unit.AIGroup != null && unit.AIGroup.IgnoresTask(unit, out var reason) && reason == AIGroup.EIgnoreTaskReason.FOLLOWING_LEADER)
				{
					Superseded.Add(unit);
				}
			}
		}

		private static void FollowChanged(Unit unit)
		{
			if (generatedDepth <= 0 && (bool)unit)
			{
				CancelUnit(unit);
				Followers.Remove(unit);
				Superseded.Remove(unit);
			}
		}

		private static void GroupRemoving(Unit unit)
		{
			if ((bool)unit)
			{
				CancelUnit(unit);
				Followers.Remove(unit);
				Superseded.Remove(unit);
			}
		}

		private static void GroupTaskChanging(AIGroup __instance)
		{
			if (generatedDepth <= 0)
			{
				FpsMoveTasks.Remove(__instance);
				Unit[] array = __instance.Units.ToArray();
				foreach (Unit unit in array)
				{
					CancelUnit(unit);
					Followers.Remove(unit);
				}
			}
		}

		private static bool FpsToggleGroup(Player player)
		{
			if (!FunctionActive(player, "follow") || generatedDepth > 0 || !player || player.IsCommander || player.Group == null || !player.ControlledUnit || player.Group.Leader != player.ControlledUnit)
			{
				return true;
			}
			if ((bool)networkSender && networkSender != player)
			{
				return true;
			}
			AIGroup aIGroup = player.Group;
			bool flag = aIGroup.AllStopped();
			CancelPlayer(player);
			ResetLeader(player.ControlledUnit);
			generatedDepth++;
			try
			{
				aIGroup.Task = null;
				Unit[] array = aIGroup.Units.ToArray();
				foreach (Unit unit in array)
				{
					if (!unit || unit.IsDestroyed || unit.PlayerControlled || ((bool)unit.Driver && (bool)unit.Driver.ControlledBy) || !unit.AIAgent)
					{
						continue;
					}
					Followers.Remove(unit);
					Superseded.Remove(unit);
					UnitCohesionGroup.Detach(unit);
					AIGroup.EIgnoreTaskReason eIgnoreTaskReason = (flag ? AIGroup.EIgnoreTaskReason.FOLLOWING_LEADER : AIGroup.EIgnoreTaskReason.NONE);
					bool flag2 = aIGroup.IgnoresTask(unit, out var reason) && reason == eIgnoreTaskReason;
					if (!flag && (bool)unit.AIAgent.AgentPathfinding)
					{
						unit.AIAgent.AgentPathfinding.PathfindingSeeker?.CancelCurrentPathRequest();
						unit.AIAgent.AgentPathfinding.CompleteMovementAndReset();
					}
					aIGroup.IgnoreTask(unit, eIgnoreTaskReason);
					if (flag2 && (bool)unit.OrderAgent)
					{
						OrderIssueParams issueParams = OrderIssueParams.Commanded(unit.AIAgent.MoveSpeed);
						if (flag)
						{
							unit.OrderAgent.IssueResolvedOrder(player.ControlledUnit.transform.position, null, in issueParams);
						}
						else
						{
							unit.OrderAgent.IssueOrder(OrderDefinitionRegistry.Stop, in OrderTarget.None, in issueParams);
						}
					}
				}
				if (flag)
				{
					Frames[player.ControlledUnit] = new FollowFrame
					{
						LastSample = player.ControlledUnit.transform.position
					};
				}
				Trace(flag ? "FPS all regroup: selected JSON follow armed" : "FPS all stop: paths and formation state cleared");
			}
			finally
			{
				generatedDepth--;
			}
			return false;
		}

		private static void FpsNewOrder(Player player, Target target)
		{
			if (generatedDepth == 0 && Game.GetIsServer() && (bool)player && (!networkSender || !(networkSender != player)) && (bool)target && target.Owner is Unit unit && unit.Team == player.Team)
			{
				CancelUnit(unit);
				Followers.Remove(unit);
			}
		}

		private static void PlayerChangedUnit(Player player, Unit oldUnit, Unit newUnit)
		{
			CancelPlayer(player);
			if ((bool)oldUnit)
			{
				ResetLeader(oldUnit);
			}
			if (!newUnit)
			{
				return;
			}
			CancelUnit(newUnit);
			Followers.Remove(newUnit);
			foreach (AttackOperation operation in Operations)
			{
				operation.Members.RemoveAll((Placement p) => (bool)p.Unit && p.Unit.Driver == newUnit);
			}
			Operations.RemoveAll((AttackOperation o) => o.Members.Count == 0 && !PlacementRetries.Any((PlacementRetry b) => b.Attack == o && b.Pending.Count > 0) && !SoftMotions.Any((SoftMotion m) => m.Batch.Attack == o));
			Follower[] array = Followers.Values.Where((Follower s) => (bool)s.Unit && s.Unit.Driver == newUnit).ToArray();
			foreach (Follower follower in array)
			{
				Followers.Remove(follower.Unit);
			}
		}

		private static void PlayerChangedTeam(Player player, Team oldTeam, Team newTeam)
		{
			ReleasePlayerControl(player);
			Menus.Remove(player);
			Selections.Remove(player);
		}

		private static bool Available(Unit u)
		{
			if ((bool)u && !u.IsDestroyed && u.AIControlled && !u.ControlledBy && !u.InCompartment && !u.IsFlying && !u.PlayerControlled && !u.IsFlyingType && (!u.Driver || !u.Driver.ControlledBy) && (bool)u.OrderAgent && (bool)u.AIAgent && (bool)u.AIAgent.AgentPathfinding)
			{
				return (UnityEngine.Object)(object)u.AIAgent.AgentPathfinding.PathfindingSeeker;
			}
			return false;
		}

		private static Vector3 Flat(Vector3 v)
		{
			v.y = 0f;
			return v;
		}

		private static Vector3 Heading(Vector3 v)
		{
			if (!(Flat(v).sqrMagnitude > 0.01f))
			{
				return Vector3.forward;
			}
			return Flat(v).normalized;
		}

		private static float DistanceSq(Vector3 a, Vector3 b)
		{
			return Flat(a - b).sqrMagnitude;
		}

		private static void Trace(string text)
		{
			if (debug && traceBudget-- > 0)
			{
				DebugMsg("[Formations] " + text);
			}
		}

		private static void Fault(Exception e)
		{
			Faulted = true;
			Enabled = false;
			ClearFollow();
			ClearOrders();
			DebugError("Si_Formation disabled after error: " + e);
		}

		public override void OnUpdate()
		{
			try
			{
				ExportEditorFootprints();
				TickLocks();
				TickRally();
				if (Running)
				{
					TickPlacementRetries();
					TickSoftMotions();
					RefreshDepartureSpeeds();
					TickFollowSearches();
					TickFollowDepartures();
					TickFpsCoordination();
				}
			}
			catch (Exception e)
			{
				Fault(e);
			}
			if (Time.unscaledTime < nextCheck)
			{
				return;
			}
			nextCheck = Time.unscaledTime + 1f;
			Player[] array = Menus.Keys.ToArray();
			foreach (Player player in array)
			{
				if (!player || !Player.Players.Contains(player))
				{
					Menus.Remove(player);
				}
			}
			ulong[] array2 = PlayerPolicies.Keys.ToArray();
			foreach (ulong num in array2)
			{
				if (!Connected(num))
				{
					PlayerPolicies.Remove(num);
				}
			}
			if (!Running)
			{
				return;
			}
			try
			{
				array = Selections.Keys.ToArray();
				foreach (Player player2 in array)
				{
					if (!player2 || !Player.Players.Contains(player2) || Selections[player2].Team != player2.Team || Selections[player2].CommanderRole != player2.IsCommander)
					{
						ReleasePlayerControl(player2);
						Selections.Remove(player2);
						Menus.Remove(player2);
					}
				}
				EnforceLimits();
				PruneLockHeadings();
				TickOperations();
				if (Enabled)
				{
					TickFollow();
				}
			}
			catch (Exception e2)
			{
				Fault(e2);
			}
		}

		public override void OnSceneWasInitialized(int buildIndex, string sceneName)
		{
			GameEvents.OnPlayerChangedUnit = (Action<Player, Unit, Unit>)Delegate.Remove(GameEvents.OnPlayerChangedUnit, new Action<Player, Unit, Unit>(PlayerChangedUnit));
			GameEvents.OnPlayerChangedUnit = (Action<Player, Unit, Unit>)Delegate.Combine(GameEvents.OnPlayerChangedUnit, new Action<Player, Unit, Unit>(PlayerChangedUnit));
			GameEvents.OnPlayerChangedTeam = (Action<Player, Team, Team>)Delegate.Remove(GameEvents.OnPlayerChangedTeam, new Action<Player, Team, Team>(PlayerChangedTeam));
			GameEvents.OnPlayerChangedTeam = (Action<Player, Team, Team>)Delegate.Combine(GameEvents.OnPlayerChangedTeam, new Action<Player, Team, Team>(PlayerChangedTeam));
			GameEvents.OnGameStarted = (Action<GameMode>)Delegate.Remove(GameEvents.OnGameStarted, new Action<GameMode>(RoundStarted));
			GameEvents.OnGameStarted = (Action<GameMode>)Delegate.Combine(GameEvents.OnGameStarted, new Action<GameMode>(RoundStarted));
			GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Remove(GameEvents.OnGameEnded, new Action<GameMode, Team>(CommanderRoundEnded));
			GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Combine(GameEvents.OnGameEnded, new Action<GameMode, Team>(CommanderRoundEnded));
		}

		public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
		{
			Followers.Clear();
			Frames.Clear();
			Superseded.Clear();
			Selections.Clear();
			ClearOrders();
			Menus.Clear();
			PlayerPolicies.Clear();
			networkSender = null;
		}

		public override void OnDeinitializeMelon()
		{
			ClearFollow();
			ClearOrders();
			Selections.Clear();
			Menus.Clear();
			PlayerPolicies.Clear();
			GameEvents.OnPlayerChangedUnit = (Action<Player, Unit, Unit>)Delegate.Remove(GameEvents.OnPlayerChangedUnit, new Action<Player, Unit, Unit>(PlayerChangedUnit));
			GameEvents.OnPlayerChangedTeam = (Action<Player, Team, Team>)Delegate.Remove(GameEvents.OnPlayerChangedTeam, new Action<Player, Team, Team>(PlayerChangedTeam));
			GameEvents.OnGameStarted = (Action<GameMode>)Delegate.Remove(GameEvents.OnGameStarted, new Action<GameMode>(RoundStarted));
			GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Remove(GameEvents.OnGameEnded, new Action<GameMode, Team>(CommanderRoundEnded));
			foreach (string item in registered)
			{
				PlayerMethods.UnregisterPlayerCommand(item);
			}
			((MelonBase)this).HarmonyInstance.UnpatchSelf();
			Ready = false;
		}

		private static bool IsAdministrator(Player p)
		{
			if ((bool)p && p.IsAdmin())
			{
				return AdminMethods.PowerInPowers(Power.Generic, p.GetAdminPowers());
			}
			return false;
		}

		private static void ToggleMenu(Player p)
		{
			if (Ready && Game.GetIsServer() && (bool)p)
			{
				if (Menus.Remove(p))
				{
					Reply(p, "menu closed.");
				}
				else
				{
					OpenMenu(p);
				}
			}
		}

		private static void OpenMenu(Player p)
		{
			if (MenuAvailable(p, explain: true))
			{
				if (TeamName(p.Team) == null)
				{
					Reply(p, "join a supported team first.");
					return;
				}
				For(p);
				Menu menu = new Menu
				{
					Team = p.Team,
					Commander = p.IsCommander,
					Function = (p.IsCommander ? "commander" : null)
				};
				Menus[p] = menu;
				ShowMenu(p, menu);
			}
		}

		private static void ShowMenu(Player p, Menu menu)
		{
			if (!Running)
			{
				Reply(p, "global formations OFF/unavailable; selections apply when enabled.");
			}
			if (menu.Function == null)
			{
				Reply(p, "Formation functions\n/1 Move\n/2 Follow\n/3 Attack\n/4 Disband all\n/5 " + (For(p).FollowFacingLock ? "Unlock formations (currently locked)" : "Lock formations (currently unlocked)"));
				return;
			}
			string value = SelectedName(p, menu.Function);
			menu.Choices = MenuChoices(TeamName(p.Team), menu.Function);
			Reply(p, menu.Function + " — Current formation: " + SafeChat(value));
			Reply(p, "/1 Default");
			FormationDefinition[] choices = menu.Choices;
			foreach (FormationDefinition formationDefinition in choices)
			{
				Reply(p, "/" + formationDefinition.MenuOrder + " " + SafeChat(formationDefinition.Name));
			}
			if (menu.Choices.Length == 0)
			{
				Reply(p, "no valid JSON formations for this team/order; native orders retained.");
			}
			Reply(p, "/back returns; /formation closes");
		}

		private static FormationDefinition[] MenuChoices(string team, string function)
		{
			return (from f in Catalog.Definitions.Values
				where f.Team == team && f.Function == function
				orderby f.MenuOrder
				select f).ToArray();
		}

		private static string SafeChat(string value)
		{
			return value.Replace("<", "‹").Replace(">", "›");
		}

		private static bool MenuChat(Player player, string message, bool teamOnly, ref OnRequestPlayerChatArgs __result)
		{
			if (!HandleMenuChat(player, message, teamOnly))
			{
				return true;
			}
			__result = new OnRequestPlayerChatArgs
			{
				Player = player,
				Text = message,
				TeamOnly = teamOnly,
				Block = true
			};
			return false;
		}

		private static bool HostMenuChat(Player __instance, string __0, bool __1, ref bool __result)
		{
			if (!HandleMenuChat(__instance, __0, __1))
			{
				return true;
			}
			__result = false;
			return false;
		}

		private static bool HandleMenuChat(Player p, string message, bool teamOnly)
		{
			if (!Ready || !Game.GetIsServer() || !p || !Menus.TryGetValue(p, out var value))
			{
				return false;
			}
			if (teamOnly && SiAdminMod.Pref_Admin_AcceptTeamChatCommands != null && !SiAdminMod.Pref_Admin_AcceptTeamChatCommands.Value)
			{
				return false;
			}
			string text = (message ?? "").Trim();
			if (!MenuAvailable(p, explain: false))
			{
				Menus.Remove(p);
				return false;
			}
			bool flag = text.Equals("/back", StringComparison.OrdinalIgnoreCase);
			bool flag2 = text.Length > 1 && text[0] == '/' && text.Skip(1).All((char c) => c >= '0' && c <= '9');
			if (!flag && !flag2)
			{
				if (text.Length > 0 && HelperMethods.IsValidCommandPrefix(text[0]) && !text.Split(' ', '\t')[0].Equals("/formation", StringComparison.OrdinalIgnoreCase))
				{
					Menus.Remove(p);
				}
				return false;
			}
			if (value.Team != p.Team || value.Commander != p.IsCommander)
			{
				Menus.Remove(p);
				Reply(p, "team/role changed; choose again.");
				OpenMenu(p);
				return true;
			}
			if (flag)
			{
				if (value.Commander || value.Function == null)
				{
					Menus.Remove(p);
					Reply(p, "menu closed.");
				}
				else
				{
					value.Function = null;
					ShowMenu(p, value);
				}
				return true;
			}
			if (!int.TryParse(text.Substring(1), out var result) || result < 1)
			{
				Reply(p, "invalid menu number; use /back.");
				return true;
			}
			if (value.Function == null)
			{
				if (result == 4)
				{
					DisbandAll(p);
					return true;
				}
				if (result == 5)
				{
					FollowOrientationCommand(p, "/followformation " + (For(p).FollowFacingLock ? "unlock" : "lock"));
					return true;
				}
				if (result > 3)
				{
					Reply(p, "choose /1, /2, /3, /4 or /5.");
					return true;
				}
				value.Function = (new string[3] { "move", "follow", "attack" })[result - 1];
				ShowMenu(p, value);
				return true;
			}
			ChooseNumber(p, value.Function, result);
			return true;
		}

		private static bool EffectivePersonal(Player p)
		{
			if ((bool)p)
			{
				if (!p.IsCommander)
				{
					return new string[3] { "move", "follow", "attack" }.Any((string f) => FunctionActive(p, f));
				}
				return FunctionActive(p, "commander");
			}
			return false;
		}

		private static void ReleasePlayerControl(Player p)
		{
			ReleaseCommander(p);
			if ((bool)p)
			{
				CancelPlayer(p);
				Unit[] array = (from s in Followers.Values
					where (bool)s.Leader && s.Leader.ControlledBy == p
					select s.Leader).Distinct().ToArray();
				for (int num = 0; num < array.Length; num++)
				{
					ResetLeader(array[num]);
				}
				if ((bool)p.ControlledUnit)
				{
					ResetLeader(p.ControlledUnit);
				}
			}
		}

		private static void DisablePersonal(Player p)
		{
			ReleasePlayerControl(p);
			For(p).PersonalEnabled = false;
			Menus.Remove(p);
		}

		private static void ChoiceMessage(Player p, string name)
		{
			Menus.Remove(p);
			HelperMethods.SendChatMessageToPlayer(p, "formation " + SafeChat(name) + " selected");
		}

		private static void ApplyCustom(Player p, string function, FormationDefinition f)
		{
			Selection selection = For(p);
			selection.Functions[function] = f;
			selection.PersonalEnabled = true;
			if (function == "follow")
			{
				ResetLeader(p.ControlledUnit);
			}
			if (function == "attack")
			{
				CancelPlayer(p);
			}
			ChoiceMessage(p, f.Name);
		}

		private static bool ChooseNumber(Player p, string function, int number)
		{
			if (!Ready || !Game.GetIsServer() || !p || TeamName(p.Team) == null || (p.IsCommander ? (function != "commander") : (!new string[3] { "move", "follow", "attack" }.Contains(function))))
			{
				Reply(p, "formation menu unavailable for your current team/role.");
				return false;
			}
			if (number == 1)
			{
				if (p.IsCommander)
				{
					ReleasePlayerControl(p);
					For(p).Functions.Remove("commander");
					For(p).PersonalEnabled = true;
				}
				else
				{
					DisablePersonal(p);
				}
				ChoiceMessage(p, "Default");
				return true;
			}
			FormationDefinition formationDefinition = MenuChoices(TeamName(p.Team), function).FirstOrDefault((FormationDefinition v) => v.MenuOrder == number);
			if (formationDefinition == null)
			{
				Reply(p, "number is not in this formation list; /1 is Default.");
				return false;
			}
			ApplyCustom(p, function, formationDefinition);
			return true;
		}

		private static void HandleShortcut(Player p, string text)
		{
			if (!Ready || !Game.GetIsServer() || !p)
			{
				Reply(p, "requires an initialized server and issuing player.");
				return;
			}
			string[] array = text.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			int n2;
			int n3;
			if (p.IsCommander)
			{
				if (array.Length == 1 && Number(array[0], out var n))
				{
					ChooseNumber(p, "commander", n);
				}
				else
				{
					Reply(p, "usage: /formation <commander option number> or /formation on|off|status");
				}
			}
			else if (array.Length == 2 && Number(array[0], out n2) && n2 <= 3 && Number(array[1], out n3))
			{
				ChooseNumber(p, (new string[3] { "move", "follow", "attack" })[n2 - 1], n3);
			}
			else
			{
				Reply(p, "usage: /formation <function 1..3> <formation option> or /formation on|off|status");
			}
			static bool Number(string value, out int reference)
			{
				if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out reference) && reference >= 1)
				{
					return reference <= 9999;
				}
				return false;
			}
		}

		private static void DisbandCommand(Player p, string args)
		{
			if ((!CommandArgument.TryParse(args, out var name, out var quoted) | quoted) || !name.Equals("all", StringComparison.OrdinalIgnoreCase))
			{
				Reply(p, "usage: /disband all");
			}
			else
			{
				DisbandAll(p);
			}
		}

		private static void DisbandAll(Player p)
		{
			if (!Ready || !Game.GetIsServer() || !p || p.IsCommander)
			{
				Reply(p, "disband all is only available to FPS players.");
				return;
			}
			if ((bool)networkSender && networkSender != p)
			{
				Reply(p, "issuing player mismatch.");
				return;
			}
			AIGroup aIGroup = p.Group;
			if (aIGroup == null)
			{
				Menus.Remove(p);
				Reply(p, "no FPS group to disband.");
				return;
			}
			if (!p.ControlledUnit || aIGroup.Leader != p.ControlledUnit)
			{
				Reply(p, "you must lead your current FPS group.");
				return;
			}
			ReleasePlayerControl(p);
			Menus.Remove(p);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			generatedDepth++;
			try
			{
				Unit[] array = aIGroup.Units.ToArray();
				foreach (Unit unit in array)
				{
					if (!unit || unit == p.ControlledUnit)
					{
						continue;
					}
					if (unit.PlayerControlled || (bool)unit.ControlledBy || ((bool)unit.Driver && (bool)unit.Driver.ControlledBy))
					{
						num2++;
						continue;
					}
					CancelUnit(unit);
					Followers.Remove(unit);
					Superseded.Remove(unit);
					try
					{
						aIGroup.RemoveUnit(unit);
						if (aIGroup.Units.Contains(unit))
						{
							num3++;
						}
						else
						{
							num++;
						}
					}
					catch (Exception ex)
					{
						num3++;
						DebugWarning("Disband removal failed: " + ex.Message);
					}
				}
			}
			finally
			{
				generatedDepth--;
			}
			Reply(p, "disbanded " + num + " units; leader retained." + ((num2 > 0) ? (" " + num2 + " player/driver-controlled units retained.") : "") + ((num3 > 0) ? (" " + num3 + " removals failed; see server log.") : ""));
		}

		private static void PersonalStatus(Player p)
		{
			ReportMaster(p);
			if ((bool)p)
			{
				Reply(p, "personal formations " + (For(p).PersonalEnabled ? "ON" : "OFF") + "; effective " + (EffectivePersonal(p) ? "ON" : "OFF"));
				string[] array = ((!p.IsCommander) ? new string[3] { "move", "follow", "attack" } : new string[1] { "commander" });
				foreach (string text in array)
				{
					Reply(p, text + ": " + SafeChat(SelectedName(p, text)) + " (effective " + (FunctionActive(p, text) ? "ON" : "native") + ")");
				}
				if (p.IsCommander)
				{
					Reply(p, "commander formation maximum: " + CommanderOrderLimit + " eligible ground units per selection; locked travel maximum: " + LockTravelMaximum + " metres; pending " + ((Options(p).Pending == 1) ? "lock" : ((Options(p).Pending == 2) ? "scout" : "none")));
				}
				else
				{
					Reply(p, "effective FPS group maximum: " + EffectiveLimit(p) + " (including leader)" + ((!Running) ? "; formation limits inactive while global OFF/unavailable" : ""));
				}
				if (IsAdministrator(p))
				{
					Reply(p, "match-wide group limit: " + MatchLimit + "; commander order maximum: " + CommanderOrderLimit);
				}
			}
		}

		private static bool PanelFunction(Player p, string function)
		{
			if ((bool)p && (bool)p.Team && TeamName(p.Team) != null)
			{
				if (!p.IsCommander)
				{
					return new string[3] { "follow", "move", "attack" }.Contains(function);
				}
				return function == "commander";
			}
			return false;
		}

		private static string PanelRevision(Player p, string function)
		{
			string s = panelEpoch + "|" + TeamName(p.Team) + "|" + function + "|" + string.Join("\n", from f in MenuChoices(TeamName(p.Team), function)
				select f.MenuOrder + ":" + f.Name);
			using SHA256 sHA = SHA256.Create();
			return BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-", "").ToLowerInvariant();
		}

		internal static IReadOnlyDictionary<string, object> ApiPanelPage(ulong id, string function, int page)
		{
			Dictionary<string, object> result = new Dictionary<string, object> { { "error", "Formation menu unavailable for your current team or role." } };
			if (ApiCheck() != ChangeResult.Success)
			{
				return result;
			}
			Player p = Connected(id);
			if (!PanelFunction(p, function) || page < 0 || page > 2048)
			{
				return result;
			}
			Selection selection = For(p);
			FormationDefinition[] array = MenuChoices(TeamName(p.Team), function);
			int num = Math.Max(1, (array.Length + 1 + 5) / 6);
			page = Math.Min(page, num - 1);
			int[] value = new int[1] { 1 }.Concat(array.Select((FormationDefinition f) => f.MenuOrder)).Skip(page * 6).Take(6)
				.ToArray();
			string[] value2 = new string[1] { p.IsCommander ? "Native commander formation" : "Native (all FPS formations off)" }.Concat(array.Select((FormationDefinition f) => f.Name)).Skip(page * 6).Take(6)
				.ToArray();
			return new Dictionary<string, object>
			{
				{ "page", page },
				{ "pages", num },
				{ "numbers", value },
				{ "names", value2 },
				{
					"selected",
					(!selection.PersonalEnabled) ? 1 : (Selected(p, function)?.MenuOrder ?? 1)
				},
				{
					"locked",
					(!p.IsCommander) ? selection.FollowFacingLock : (Options(p).Pending == 1 || Locks.Any((LockedGroup g) => g.Owner == p && g.Team == p.Team && g.Members.Count > 0))
				},
				{ "commanderLock", p.IsCommander },
				{
					"enabled",
					FunctionActive(p, function)
				},
				{
					"revision",
					PanelRevision(p, function)
				},
				{
					"leader",
					!p.IsCommander && (bool)p.ControlledUnit && p.Group != null && p.Group.Leader == p.ControlledUnit
				}
			};
		}

		internal static string ApiPanelAction(ulong id, string function, int option, string action, string revision)
		{
			if (ApiCheck() != ChangeResult.Success)
			{
				return "Formation service unavailable.";
			}
			Player player = Connected(id);
			if (!PanelFunction(player, function))
			{
				return "Your team or role changed; reopen the panel.";
			}
			if (revision != PanelRevision(player, function))
			{
				return "Formation list or round changed; refresh before choosing.";
			}
			if (action == "select")
			{
				if (!ChooseNumber(player, function, option))
				{
					return "Formation selection rejected.";
				}
				return "Formation selection applied.";
			}
			if (player.IsCommander && action == "lock" && (option == 0 || option == 1))
			{
				if (option == 1 && !CommanderAuthorized(player))
				{
					return "Commander formations unavailable; lock was not armed.";
				}
				LockCommand(player, (option == 1) ? "lock" : "unlock");
				if (option != 1)
				{
					return "Commander formations unlocked; pending lock cancelled.";
				}
				return "Formation lock armed for your next commander move.";
			}
			if (player.IsCommander || !player.ControlledUnit || player.Group == null || player.Group.Leader != player.ControlledUnit)
			{
				return "This action requires an FPS squad leader.";
			}
			if (action == "lock" && (option == 0 || option == 1))
			{
				FollowOrientationCommand(player, (option == 1) ? "/followformation lock" : "/followformation unlock");
				if (option != 1)
				{
					return "Facing lock disabled.";
				}
				return "Facing lock enabled.";
			}
			if (action == "disband" && option == 0)
			{
				DisbandAll(player);
				return "Squad dismissal processed; controlled units retained.";
			}
			return "Unsupported formation action.";
		}

		private static void FormationHelp(Player p)
		{
			if ((bool)p)
			{
				Reply(p, "/formation - menu; /formation help - commands; /formation status - status and selections.");
				Reply(p, "In the menu: /<number> selects; /back returns. Option 1 in a formation list is vanilla.");
				if (p.IsCommander)
				{
					Reply(p, "/formation <number> - commander formation; /formation lock - next move only; /formation unlock - release locks.");
					Reply(p, "/scout - next move selection scouts (at least five eligible units). Aircraft and excess units use native movement on normal moves.");
				}
				else
				{
					Reply(p, "/formation <function> <number> - 1 Move, 2 Follow, 3 Attack; /disband all - disband your AI squad (leader only).");
					Reply(p, "/followformation lock|unlock|status - coordinate Move, Follow and Attack staging; menu /5 toggles. Follow uses moving formation slots; player speed stays unrestricted.");
				}
				if (p.IsCommander || IsAdministrator(p))
				{
					Reply(p, "Legacy compatibility only: /formationTolerance <10..100>, /FormationFallof <10..90>; neither changes current movement.");
				}
				if (IsAdministrator(p))
				{
					Reply(p, "/formation rallyfix on|off|status - saved automatic production parking; /formation debug on|off|status - match diagnostic logging.");
					Reply(p, "Admin: /formation on|off - global match switch; /formationsizeglobal <1..50>; /formationsize \"Player Name\" <1..50> (FPS, including leader).");
					Reply(p, "/formationsizecommander <1..100> | status - saved per-selection cap (default 75); /Formationtraveldistance <0..10000> | status - saved lock maximum metres (default 1000).");
					Reply(p, "Farther lock clicks stop the formation centre at that maximum; 0 keeps the centre in place. Soft-lock moves are not range-limited.");
					Reply(p, "/formationTravel <metres> - alias for /Formationtraveldistance; /Formationspeedcap on|off - final-arrival ceiling (centre pacing remains).");
					Reply(p, "/startdelaysoftlock <seconds>; /startdelayspeedlock <seconds>; /startdelaylock <seconds> (alias /startdelay lock). 0 disables, otherwise 0.1..5; match-only.");
					Reply(p, "Speed-based lock delay takes priority. For positional rows: /startdelayspeedlock 0 then /startdelaylock <seconds>.");
				}
			}
		}

		private static void EnsureBundledFormations()
		{
			string text = System.IO.Path.Combine(MelonEnvironment.UserDataDirectory, "Formations_cfg");
			Directory.CreateDirectory(text);
			if (Directory.GetFiles(text, "*.json").Length != 0)
			{
				return;
			}
			string text2 = System.IO.Path.Combine(text, "silica-formations.json");
			using (Stream stream = typeof(Formations).Assembly.GetManifestResourceStream("Si_Formation.DefaultFormations.json"))
			{
				if (stream == null)
				{
					throw new InvalidOperationException("Bundled default formations resource is missing.");
				}
				using FileStream destination = new FileStream(text2, FileMode.CreateNew, FileAccess.Write, FileShare.None);
				stream.CopyTo(destination);
			}
			DebugMsg("Installed bundled default formations: " + text2);
		}

		private void InstallPreferenceLayout()
		{
			if (File.Exists(PreferencesPath))
			{
				preferenceLayout = File.ReadAllText(PreferencesPath);
			}
			Type type = typeof(MelonPreferences).Assembly.GetType("MelonLoader.Preferences.IO.File", throwOnError: true);
			Patch(type, "Save", null, null, "PreferenceSaveLayout");
		}

		private static IEnumerable<CodeInstruction> PreferenceSaveLayout(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo write = AccessTools.Method(typeof(File), "WriteAllText", new Type[2]
			{
				typeof(string),
				typeof(string)
			}, (Type[])null);
			MethodInfo replacement = AccessTools.Method(typeof(Formations), "WritePreferenceFile", (Type[])null, (Type[])null);
			int count = 0;
			foreach (CodeInstruction instruction in instructions)
			{
				if (CodeInstructionExtensions.Calls(instruction, write))
				{
					instruction.opcode = OpCodes.Call;
					instruction.operand = replacement;
					count++;
				}
				yield return instruction;
			}
			if (count != 1)
			{
				throw new InvalidOperationException("Expected one preferences file write.");
			}
		}

		private static void WritePreferenceFile(string path, string contents)
		{
			if (string.Equals(System.IO.Path.GetFullPath(path), System.IO.Path.GetFullPath(PreferencesPath), StringComparison.OrdinalIgnoreCase))
			{
				contents = FormatPreferenceText(contents);
			}
			File.WriteAllText(path, contents);
		}

		private static void FormatPreferences()
		{
			try
			{
				if (File.Exists(PreferencesPath))
				{
					string text = File.ReadAllText(PreferencesPath);
					string text2 = FormatPreferenceText(text);
					if (text2 != text)
					{
						File.WriteAllText(PreferencesPath, text2, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
					}
				}
			}
			catch (Exception ex)
			{
				DebugWarning("Could not format si_formation.cfg: " + ex.Message);
			}
		}

		private static string FormatPreferenceText(string original)
		{
			try
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
				Regex regex = new Regex("^([A-Za-z][A-Za-z0-9_]*)\\s*=\\s*(.*)$");
				bool flag = false;
				string[] array = original.Split('\n');
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i].Trim();
					if (text.Length == 0 || text.StartsWith("#"))
					{
						continue;
					}
					if (Regex.IsMatch(text, "^\\[\\s*[\"']?Si_Formation[\"']?\\s*\\]\\s*(#.*)?$") && !flag)
					{
						flag = true;
						continue;
					}
					Match match = regex.Match(text);
					if (!flag || !match.Success || dictionary.ContainsKey(match.Groups[1].Value))
					{
						return original;
					}
					string value = match.Groups[1].Value;
					string text2 = match.Groups[2].Value;
					if (value.StartsWith("StartDelay", StringComparison.Ordinal) && float.TryParse(text2, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
					{
						text2 = result.ToString("G7", CultureInfo.InvariantCulture);
					}
					dictionary.Add(value, text2);
				}
				if (!flag)
				{
					return original;
				}
				string text3 = preferenceLayout;
				if (string.IsNullOrEmpty(text3))
				{
					using Stream stream = typeof(Formations).Assembly.GetManifestResourceStream("Si_Formation.Preferences.cfg");
					using StreamReader streamReader = new StreamReader(stream);
					text3 = streamReader.ReadToEnd();
				}
				StringBuilder stringBuilder = new StringBuilder();
				array = text3.Split('\n');
				for (int i = 0; i < array.Length; i++)
				{
					string text4 = array[i].TrimEnd('\r');
					Match match2 = regex.Match(text4);
					if (!match2.Success)
					{
						stringBuilder.AppendLine(text4);
						continue;
					}
					string value2 = match2.Groups[1].Value;
					if (dictionary.TryGetValue(value2, out var value3))
					{
						stringBuilder.AppendLine(value2 + " = " + value3);
						dictionary.Remove(value2);
					}
				}
				if (dictionary.Count > 0)
				{
					stringBuilder.AppendLine("# === Additional preserved settings ===");
					foreach (KeyValuePair<string, string> item in dictionary)
					{
						stringBuilder.AppendLine(item.Key + " = " + item.Value);
					}
				}
				return stringBuilder.ToString().TrimEnd() + Environment.NewLine;
			}
			catch (Exception ex)
			{
				DebugWarning("Could not format si_formation.cfg: " + ex.Message);
				return original;
			}
		}

		private static string DefaultKey(string team, string function)
		{
			return "Default" + team + char.ToUpperInvariant(function[0]) + function.Substring(1);
		}

		private static void RegisterFormationDefaults()
		{
			formationDefaults.Clear();
			string[] array = new string[3] { "Sol", "Centauri", "Alien" };
			foreach (string text in array)
			{
				string[] array2 = new string[4] { "move", "follow", "attack", "commander" };
				foreach (string text2 in array2)
				{
					string text3 = DefaultKey(text, text2);
					formationDefaults[text3] = prefs.CreateEntry<int>(text3, 1, (string)null, text + " " + text2 + " initial menu option: 1=native, 2..9999=exact menu number; applied each round and on fresh team/role state", false, false, (ValueValidator)null, (string)null);
				}
			}
		}

		private static void LoadFormationDefaults()
		{
			matchFormationDefaults.Clear();
			string[] array = new string[3] { "Sol", "Centauri", "Alien" };
			foreach (string team in array)
			{
				string[] array2 = new string[4] { "move", "follow", "attack", "commander" };
				foreach (string function in array2)
				{
					string text = DefaultKey(team, function);
					int number = formationDefaults[text].Value;
					if (number != 1)
					{
						FormationDefinition formationDefinition = Catalog.Definitions.Values.FirstOrDefault((FormationDefinition f) => f.Team == team && f.Function == function && f.MenuOrder == number);
						if (number < 2 || number > 9999 || formationDefinition == null)
						{
							DebugWarning("[Si_Formation] " + text + "=" + number + " has no valid formation menu option; using native (1).");
						}
						else
						{
							matchFormationDefaults[text] = formationDefinition;
						}
					}
				}
			}
		}

		private static void ApplyFormationDefaults(Selection selection)
		{
			string text = TeamName(selection.Team);
			if (text == null)
			{
				return;
			}
			string[] array = ((!selection.CommanderRole) ? new string[3] { "move", "follow", "attack" } : new string[1] { "commander" });
			foreach (string text2 in array)
			{
				if (matchFormationDefaults.TryGetValue(DefaultKey(text, text2), out var value))
				{
					selection.Functions[text2] = value;
				}
			}
			selection.PersonalEnabled = selection.CommanderRole || selection.Functions.Count > 0;
		}

		private static bool ValidCommanderLimit(int value)
		{
			if (value >= 1)
			{
				return value <= 100;
			}
			return false;
		}

		private static void CommanderSizeCommand(Player p, string args)
		{
			if (!IsAdministrator(p))
			{
				Reply(p, "permission denied: admin Generic power required.");
				return;
			}
			if (!ApiReady)
			{
				Reply(p, "server not initialized.");
				return;
			}
			if (!CommandArgument.TryParse(args, out var name, out var quoted) | quoted)
			{
				Reply(p, "usage: /formationsizecommander <1..100> | status");
				return;
			}
			if (name.Length == 0 || name.Equals("status", StringComparison.OrdinalIgnoreCase))
			{
				Reply(p, "commander order maximum: " + CommanderOrderLimit);
				return;
			}
			if (!int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var result) || !ValidCommanderLimit(result))
			{
				Reply(p, "usage: /formationsizecommander <1..100> | status");
				return;
			}
			commanderOrderLimit.Value = result;
			try
			{
				prefs.SaveToFile(false);
				FormatPreferences();
				Reply(p, "commander order maximum: " + result + " (saved)");
			}
			catch (Exception ex)
			{
				DebugError("Saving CommanderOrderLimit: " + ex);
				Reply(p, "commander order maximum: " + result + "; saving failed, see server log.");
			}
		}

		internal static ChangeResult ApiActivation(ulong id, out bool personal, out bool effective)
		{
			personal = false;
			effective = false;
			ChangeResult changeResult = ApiCheck();
			if (changeResult != ChangeResult.Success)
			{
				return changeResult;
			}
			Player player = Connected(id);
			if (!player)
			{
				return ChangeResult.InvalidPlayer;
			}
			personal = For(player).PersonalEnabled;
			effective = EffectivePersonal(player);
			return ChangeResult.Success;
		}

		internal static ChangeResult ApiFunctionEnabled(ulong id, string function, out bool enabled)
		{
			enabled = false;
			ChangeResult changeResult = ApiCheck();
			if (changeResult != ChangeResult.Success)
			{
				return changeResult;
			}
			if (!new string[4] { "move", "follow", "attack", "commander" }.Contains(function))
			{
				return ChangeResult.InvalidFunction;
			}
			Player player = Connected(id);
			if (!player)
			{
				return ChangeResult.InvalidPlayer;
			}
			enabled = FunctionActive(player, function);
			return ChangeResult.Success;
		}

		private static bool ValidLimit(int value)
		{
			if (value >= 1)
			{
				return value <= 50;
			}
			return false;
		}

		private static ulong PlayerId(Player p)
		{
			if (!p)
			{
				return 0uL;
			}
			return p.PlayerID.SteamID.m_SteamID;
		}

		private static Player Connected(ulong id)
		{
			if (id != 0L)
			{
				return Player.Players.FirstOrDefault((Player p) => (bool)p && PlayerId(p) == id);
			}
			return null;
		}

		private static ChangeResult ApiCheck()
		{
			if (Thread.CurrentThread.ManagedThreadId != serverThread)
			{
				return ChangeResult.WrongThread;
			}
			if (!ApiReady)
			{
				return ChangeResult.NotReady;
			}
			return ChangeResult.Success;
		}

		private static int ConfiguredPlayerLimit(Player p)
		{
			ulong key = PlayerId(p);
			if (AdminLimits.TryGetValue(key, out var value))
			{
				return value;
			}
			if (adminMatchLimit.HasValue)
			{
				return adminMatchLimit.Value;
			}
			if (PlayerPolicies.TryGetValue(key, out var value2))
			{
				return value2.Limit;
			}
			if (!ApiLimits.TryGetValue(key, out value))
			{
				return MatchLimit;
			}
			return value;
		}

		private static int EffectiveLimit(Player p)
		{
			if (!Running)
			{
				if (!FPSCommanding.Instance)
				{
					return 8;
				}
				return FPSCommanding.Instance.GroupUnitLimit;
			}
			return ConfiguredPlayerLimit(p);
		}

		internal static ChangeResult ApiPlayerLimit(ulong id, out int limit)
		{
			limit = 0;
			ChangeResult changeResult = ApiCheck();
			if (changeResult != ChangeResult.Success)
			{
				return changeResult;
			}
			Player player = Connected(id);
			if (!player)
			{
				return ChangeResult.InvalidPlayer;
			}
			limit = EffectiveLimit(player);
			return ChangeResult.Success;
		}

		internal static ChangeResult ApiCommanderEnabled(ulong id, out bool enabled)
		{
			enabled = false;
			ChangeResult changeResult = ApiCheck();
			if (changeResult != ChangeResult.Success)
			{
				return changeResult;
			}
			Player player = Connected(id);
			if (!player)
			{
				return ChangeResult.InvalidPlayer;
			}
			enabled = FunctionActive(player, "commander");
			return ChangeResult.Success;
		}

		internal static ChangeResult ApiGlobal(bool enabled)
		{
			ChangeResult changeResult = ApiCheck();
			if (changeResult != ChangeResult.Success)
			{
				return changeResult;
			}
			if (adminGlobal)
			{
				return ChangeResult.AdminOverride;
			}
			SetGlobal(enabled);
			return ChangeResult.Success;
		}

		internal static ChangeResult ApiMatchLimit(int limit)
		{
			ChangeResult changeResult = ApiCheck();
			if (changeResult != ChangeResult.Success)
			{
				return changeResult;
			}
			if (!ValidLimit(limit))
			{
				return ChangeResult.InvalidLimit;
			}
			if (adminMatchLimit.HasValue)
			{
				return ChangeResult.AdminOverride;
			}
			apiMatchLimit = limit;
			LimitsChanged();
			return ChangeResult.Success;
		}

		internal static ChangeResult ApiSetPlayerLimit(ulong id, int limit, bool clear)
		{
			ChangeResult changeResult = ApiCheck();
			if (changeResult != ChangeResult.Success)
			{
				return changeResult;
			}
			if (!Connected(id))
			{
				return ChangeResult.InvalidPlayer;
			}
			if (!clear && !ValidLimit(limit))
			{
				return ChangeResult.InvalidLimit;
			}
			if (adminMatchLimit.HasValue || AdminLimits.ContainsKey(id))
			{
				return ChangeResult.AdminOverride;
			}
			if (clear)
			{
				ApiLimits.Remove(id);
			}
			else
			{
				ApiLimits[id] = limit;
			}
			LimitsChanged();
			return ChangeResult.Success;
		}

		private static void SetGlobal(bool enabled)
		{
			ClearFollow();
			ClearOrders();
			Enabled = enabled;
			if (enabled)
			{
				Faulted = false;
			}
			nextCheck = 0f;
			EnforceLimits();
		}

		private static void LoadDefaults()
		{
			configuredEnabled = enabledDefault?.Value ?? true;
			configuredLimit = groupLimitDefault?.Value ?? 10;
			debug = debugDefault?.Value ?? false;
			if (!ValidLimit(configuredLimit))
			{
				DebugWarning("[Si_Formation] GroupLimit must be 1..50; using 10.");
				configuredLimit = 10;
			}
		}

		private static void ResetMatchSettings()
		{
			LoadDefaults();
			LoadFormationDefaults();
			adminGlobal = false;
			adminMatchLimit = null;
			apiMatchLimit = null;
			AdminLimits.Clear();
			ApiLimits.Clear();
			PlayerPolicies.Clear();
			Menus.Clear();
			Enabled = configuredEnabled;
			Faulted = false;
		}

		private static void GlobalSizeCommand(Player p, string args)
		{
			if (!IsAdministrator(p))
			{
				Reply(p, "permission denied: admin Generic power required.");
				return;
			}
			if (!ApiReady)
			{
				Reply(p, "server not initialized.");
				return;
			}
			if ((!CommandArgument.TryParse(args, out var name, out var quoted) | quoted) || !int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var result) || !ValidLimit(result))
			{
				Reply(p, "usage: /formationsizeglobal <1..50>; includes the controlled leader.");
				return;
			}
			adminMatchLimit = result;
			LimitsChanged();
			Reply(p, "match-wide FPS group maximum: " + result + " (including leader)");
		}

		private static void SizeCommand(Player p, string args)
		{
			if (!IsAdministrator(p))
			{
				Reply(p, "permission denied: admin Generic power required.");
				return;
			}
			if (!ApiReady)
			{
				Reply(p, "server not initialized.");
				return;
			}
			string text = (args ?? "").Trim();
			int num = text.IndexOfAny(new char[2] { ' ', '\t' });
			text = ((num < 0) ? "" : text.Substring(num).Trim());
			int num2 = text.LastIndexOfAny(new char[2] { ' ', '\t' });
			if (num2 < 0)
			{
				Reply(p, "For the match-wide limit use /formationsizeglobal <1..50>. For one player use /formationsize \"Player Name\" <1..50>. Limits include the leader.");
				return;
			}
			if (!int.TryParse(text.Substring(num2 + 1), NumberStyles.None, CultureInfo.InvariantCulture, out var result) || !ValidLimit(result))
			{
				Reply(p, "usage: /formationsize <player name> <1..50>; includes leader.");
				return;
			}
			string name = text.Substring(0, num2).Trim();
			if (name.StartsWith("\""))
			{
				try
				{
					name = JsonConvert.DeserializeObject<string>(name);
				}
				catch
				{
					Reply(p, "invalid quoted player name.");
					return;
				}
			}
			if (string.IsNullOrWhiteSpace(name) || name.Any(char.IsControl))
			{
				Reply(p, "invalid player name.");
				return;
			}
			Player[] array = Player.Players.Where((Player player) => (bool)player && string.Equals(player.PlayerName, name, StringComparison.OrdinalIgnoreCase)).ToArray();
			if (array.Length != 1)
			{
				Reply(p, "use an exact, unique connected player name (spaces allowed); matches: " + array.Length);
				return;
			}
			ulong num3 = PlayerId(array[0]);
			if (num3 == 0L)
			{
				Reply(p, "player has no stable Steam identifier.");
				return;
			}
			AdminLimits[num3] = result;
			LimitsChanged();
			Reply(p, SafeChat(array[0].PlayerName) + " group maximum: " + result);
		}

		private static void LimitsChanged()
		{
			ClearFollow();
			ClearOrders();
			EnforceLimits();
			nextCheck = 0f;
		}

		private static void EnforceLimits()
		{
			if (!Running)
			{
				return;
			}
			Player[] array = Player.Players.ToArray();
			foreach (Player p in array)
			{
				if (!p || p.IsCommander || !p.ControlledUnit || p.Group == null || p.Group.Leader != p.ControlledUnit)
				{
					continue;
				}
				AIGroup aIGroup = p.Group;
				int num = ConfiguredPlayerLimit(p);
				Unit[] array2 = (from u in aIGroup.Units.Where((Unit u) => (bool)u && u != p.ControlledUnit && !u.PlayerControlled && (!u.Driver || !u.Driver.ControlledBy)).Reverse()
					orderby PolicyAllows(p, u)
					select u).ToArray();
				foreach (Unit unit in array2)
				{
					if (aIGroup.UnitCount > num || !PolicyAllows(p, unit))
					{
						CancelPlayer(p);
						ResetLeader(p.ControlledUnit);
						aIGroup.RemoveUnit(unit);
					}
				}
			}
		}

		private void PatchAdminInviteLimit()
		{
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Expected Obj, but got Unknown
			MethodInfo methodInfo = typeof(Event_Units).Assembly.GetType("SilicaAdminMod.Event_Units+ApplyPatch_FPSCommanding_InviteToGroupServer")?.GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[3]
			{
				typeof(FPSCommanding),
				typeof(Player),
				typeof(Target)
			}, null);
			if (methodInfo == null || methodInfo.ReturnType != typeof(bool))
			{
				throw new InvalidOperationException("Unsupported Admin Mod recruitment prefix: expected static bool SilicaAdminMod.Event_Units+ApplyPatch_FPSCommanding_InviteToGroupServer.Prefix(FPSCommanding, Player, Target).");
			}
			((MelonBase)this).HarmonyInstance.Patch((MethodBase)methodInfo, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(Formations), "InviteLimit", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
		}

		private static IEnumerable<CodeInstruction> InviteLimit(IEnumerable<CodeInstruction> instructions, MethodBase original)
		{
			int count = 0;
			FieldInfo field = AccessTools.Field(typeof(FPSCommanding), "GroupUnitLimit");
			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldfld && object.Equals(instruction.operand, field))
				{
					instruction.opcode = OpCodes.Ldarg_1;
					instruction.operand = null;
					yield return instruction;
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Formations), "RecruitLimit", (Type[])null, (Type[])null));
					count++;
				}
				else
				{
					yield return instruction;
				}
			}
			if (count != 1)
			{
				throw new InvalidOperationException("Expected one FPS recruitment limit field read in " + original.DeclaringType.FullName + "." + original.Name + "; found " + count + ".");
			}
		}

		private static int RecruitLimit(FPSCommanding instance, Player player)
		{
			if (!Running || !player)
			{
				return instance.GroupUnitLimit;
			}
			return ConfiguredPlayerLimit(player);
		}

		private static bool GroupAdding(AIGroup __instance, Unit unit, ref bool __result)
		{
			if (!Running || !unit || unit.PlayerControlled || ((bool)unit.Driver && (bool)unit.Driver.ControlledBy) || !__instance.Leader || !__instance.Leader.ControlledBy)
			{
				return true;
			}
			Player controlledBy = __instance.Leader.ControlledBy;
			if (controlledBy.IsCommander || controlledBy.Group != __instance || __instance.Units.Contains(unit))
			{
				return true;
			}
			if (PolicyAllows(controlledBy, unit) && __instance.UnitCount < ConfiguredPlayerLimit(controlledBy))
			{
				return true;
			}
			__result = false;
			return false;
		}

		private static bool RecruitAuthorized(Player player, Target target)
		{
			if (!Running)
			{
				return true;
			}
			if ((bool)player && !player.IsCommander && (!networkSender || networkSender == player) && (bool)target && (bool)target.OwnerUnit && target.OwnerUnit.Team == player.Team && !target.OwnerUnit.PlayerControlled && (!target.OwnerUnit.Driver || !target.OwnerUnit.Driver.ControlledBy) && PolicyAllows(player, target.OwnerUnit))
			{
				if (target.OwnerUnit.AIGroup != null)
				{
					return !target.OwnerUnit.AIGroup.PlayerControlled;
				}
				return true;
			}
			return false;
		}

		private void InstallRallyFix()
		{
			rallyFixDefault = prefs.CreateEntry<bool>("Rallyfix", true, (string)null, "Growing faction-shaped production parking; /formation rallyfix on|off saves this setting", false, false, (ValueValidator)null, (string)null);
			Patch(typeof(Structure), "OnInit", null, "RallyFactoryInitialized");
			Patch(typeof(Structure), "MoveRallyPoint", null, "RallyFactoryChanged");
			Patch(typeof(ConstructionSite), "SpawnObject", "RallySiteBegin", "RallyProducedSite", "RallySiteSpawn", "RallyProductionEnd");
			Patch(typeof(Structure), "ConstructNonPlaceableInstant", "RallyInstantBegin", null, "RallyInstantSpawn", "RallyProductionEnd");
			Patch(typeof(Unit), "RallyTo", "RallyNativeDestination");
			Patch(typeof(Vehicle), "RallyTo", "RallyNativeDestination");
			Patch(typeof(Vehicle), "ClearForceMove", "RallyExitBegin", null, null, "RallyExitEnd");
		}

		private static void RallyFactoryInitialized(Structure __instance)
		{
			if (Game.GetIsServer())
			{
				RallyFactories[__instance] = new RallyFactory
				{
					Factory = __instance,
					DefaultPoint = __instance.RallyPoint,
					RequestedPoint = __instance.RallyPoint
				};
			}
		}

		private static RallyFactory RallyState(Structure factory)
		{
			if (RallyFactories.TryGetValue(factory, out var value))
			{
				return value;
			}
			Vector3 vector = (factory.ProductionUnitMoveToTransform ? factory.ProductionUnitMoveToTransform.position : Vector3.zero);
			bool flag = factory.RallyPoint != Vector3.zero && DistanceSq(factory.RallyPoint, vector) > 1f;
			value = new RallyFactory
			{
				Factory = factory,
				DefaultPoint = vector,
				RequestedPoint = factory.RallyPoint,
				Explicit = flag
			};
			RallyFactories.Add(factory, value);
			return value;
		}

		private static void RallyFactoryChanged(Structure __instance, Vector3 position)
		{
			if (!movingRallyMarker && Game.GetIsServer())
			{
				RallyFactory rallyFactory = RallyState(__instance);
				rallyFactory.RequestedPoint = position;
				rallyFactory.Explicit = position != Vector3.zero && DistanceSq(position, rallyFactory.DefaultPoint) > 1f;
				rallyFactory.OwnsMarker = false;
				if (rallyFactory.Area != null)
				{
					rallyFactory.Area.Preview = null;
					rallyFactory.Area.Positions.Dispose();
					rallyFactory.Area = null;
				}
				string name = __instance.name;
				Vector3 vector = position;
				Trace("Rallyfix new commander centre: " + name + " at " + vector.ToString());
			}
		}

		private static void RallySiteBegin(ConstructionSite __instance, out Structure __state)
		{
			__state = productionRallyFactory;
			productionRallyFactory = __instance.ParentStructure;
		}

		private static void RallyInstantBegin(Structure __instance, out Structure __state)
		{
			__state = productionRallyFactory;
			productionRallyFactory = __instance;
		}

		private static Exception RallyProductionEnd(Exception __exception, Structure __state)
		{
			productionRallyFactory = __state;
			return __exception;
		}

		private static IEnumerable<CodeInstruction> RallySiteSpawn(IEnumerable<CodeInstruction> instructions)
		{
			return ReplaceRallyProduced(instructions, "RallySiteProduced");
		}

		private static IEnumerable<CodeInstruction> RallyInstantSpawn(IEnumerable<CodeInstruction> instructions)
		{
			return ReplaceRallyProduced(instructions, "RallyInstantProduced");
		}

		private static IEnumerable<CodeInstruction> ReplaceRallyProduced(IEnumerable<CodeInstruction> instructions, string replacement)
		{
			MethodInfo produced = AccessTools.Method(typeof(Unit), "OnProduced", (Type[])null, (Type[])null);
			int count = 0;
			foreach (CodeInstruction instruction in instructions)
			{
				if (CodeInstructionExtensions.Calls(instruction, produced))
				{
					instruction.opcode = OpCodes.Call;
					instruction.operand = AccessTools.Method(typeof(Formations), replacement, (Type[])null, (Type[])null);
					count++;
				}
				yield return instruction;
			}
			if (count != 1)
			{
				throw new InvalidOperationException("Expected one production OnProduced call: " + replacement);
			}
		}

		private static void RallySiteProduced(Unit unit, ConstructionSite site)
		{
			unit.OnProduced(site);
			QueueRally(unit, site ? site.ParentStructure : productionRallyFactory);
		}

		private static void RallyInstantProduced(Unit unit, ConstructionSite site)
		{
			unit.OnProduced(site);
			QueueRally(unit, productionRallyFactory);
		}

		private static void RallyProducedSite(ConstructionSite __instance)
		{
			if (__instance.SpawnedGameObject is Unit unit && !RallyTickets.ContainsKey(unit))
			{
				QueueRally(unit, __instance.ParentStructure);
			}
		}

		private static bool RallyNativeDestination(Unit __instance)
		{
			if (RallyEnabled && !issuingRally && (bool)productionRallyFactory)
			{
				return !RallyTickets.ContainsKey(__instance);
			}
			return true;
		}

		private static void RallyExitBegin(Vehicle __instance, out Vehicle __state)
		{
			__state = exitingRallyVehicle;
			exitingRallyVehicle = __instance;
		}

		private static Exception RallyExitEnd(Exception __exception, Vehicle __state)
		{
			exitingRallyVehicle = __state;
			return __exception;
		}

		private static bool RallyTierSpacing(Structure factory, out float spacing)
		{
			spacing = 0f;
			if (!factory || !factory.ObjectInfo || factory.ObjectInfo.StructureType != StructureType.Production)
			{
				return false;
			}
			switch (factory.ObjectInfo.StructureSelectionType)
			{
			case StructureSelectionType.Units1:
				spacing = 5f;
				break;
			case StructureSelectionType.Units2:
				spacing = 15f;
				break;
			case StructureSelectionType.Units3:
				spacing = Mathf.Max(20f, Catalog.Definitions.Values.FirstOrDefault((FormationDefinition f) => f.Team == TeamName(factory.Team) && f.Function == "commander" && f.MenuOrder == 2)?.Scale ?? 30f);
				break;
			case StructureSelectionType.Units4:
				spacing = 30f;
				break;
			default:
				return false;
			}
			return true;
		}

		private static float RallyRadius(Unit unit)
		{
			return RallyBoundsRadius(unit.PhysicalBounds.size);
		}

		private static float RallyBoundsRadius(Vector3 size)
		{
			return Mathf.Max(0.5f, Mathf.Sqrt(size.x * size.x + size.z * size.z) * 0.5f) + 0.5f;
		}

		private static RallyArea CreateRallyArea(RallyFactory state, Unit unit, float spacing)
		{
			Structure factory = state.Factory;
			float num = RallyRadius(unit);
			foreach (ConstructionData constructionOption in factory.ConstructionOptions)
			{
				if ((bool)constructionOption && (bool)constructionOption.ObjectInfo && constructionOption.ObjectInfo.ObjectType == ObjectInfoType.Unit)
				{
					num = Mathf.Max(num, RallyBoundsRadius(constructionOption.ObjectInfo.PhysicalBounds.size));
				}
			}
			Vector3 vector = (factory.ProductionUnitMoveToTransform ? factory.ProductionUnitMoveToTransform.position : factory.transform.position);
			Vector3 vector2 = Heading(vector - factory.transform.position);
			if (DistanceSq(vector, factory.transform.position) < 1f)
			{
				vector2 = Heading(factory.transform.forward);
			}
			RallyArea rallyArea = new RallyArea
			{
				State = state,
				Team = unit.Team,
				Forward = vector2,
				Shape = TeamName(unit.Team),
				Automatic = !state.Explicit,
				Radius = num,
				Spacing = Mathf.Max(spacing, num * 2f)
			};
			rallyArea.Anchor = (rallyArea.Automatic ? (factory.transform.position + vector2 * (75f + factory.PhysicalRadius + num)) : state.RequestedPoint);
			rallyArea.Positions = RallyOffsets(rallyArea.Shape).GetEnumerator();
			return rallyArea;
		}

		private static void QueueRally(Unit unit, Structure factory)
		{
			try
			{
				if (RallyEnabled && (bool)unit && !unit.IsFlyingType && TeamName(unit.Team) != null && RallyTierSpacing(factory, out var spacing))
				{
					ReleaseRally(unit);
					RallyFactory rallyFactory = RallyState(factory);
					if (rallyFactory.Area == null || rallyFactory.Area.Team != unit.Team)
					{
						rallyFactory.Area = CreateRallyArea(rallyFactory, unit, spacing);
					}
					RallyTicket rallyTicket = new RallyTicket
					{
						Unit = unit,
						Area = rallyFactory.Area,
						Created = Time.unscaledTime,
						Serial = ++rallySerial
					};
					rallyTicket.Spot = rallyFactory.Area.Preview;
					rallyFactory.Area.Preview = null;
					RallyTickets[unit] = rallyTicket;
					string[] array = new string[8]
					{
						"Rallyfix queued ",
						unit.name,
						"; ",
						rallyTicket.Area.Shape,
						" spacing ",
						rallyTicket.Area.Spacing.ToString(),
						"m; centre ",
						null
					};
					Vector3 anchor = rallyTicket.Area.Anchor;
					array[7] = anchor.ToString();
					Trace(string.Concat(array));
				}
			}
			catch (Exception ex)
			{
				DebugWarning("Rallyfix production observation: " + ex.Message);
			}
		}

		private static bool RallyCurrent(RallyTicket ticket)
		{
			if (RallyEnabled && (bool)ticket.Unit && (bool)ticket.Area.State.Factory && !ticket.Area.State.Factory.IsDestroyed && ticket.Unit.Team == ticket.Area.Team && ticket.Area.State.Factory.Team == ticket.Area.Team && Available(ticket.Unit) && RallyTickets.TryGetValue(ticket.Unit, out var value))
			{
				return value == ticket;
			}
			return false;
		}

		private static void ReleaseRally(Unit unit, string reason = null)
		{
			if ((object)unit != null && RallyTickets.TryGetValue(unit, out var _))
			{
				if (reason != null)
				{
					Trace("Rallyfix released " + (unit ? unit.name : "destroyed unit") + ": " + reason);
				}
				RallyTickets.Remove(unit);
			}
		}

		private static void RallyOtherOrder(AIOrderProcessor processor, bool success)
		{
			if (success && !issuingRally && processor.Owner is Unit unit && !(exitingRallyVehicle == unit) && (!productionRallyFactory || !RallyTickets.TryGetValue(unit, out var value) || !(value.Area.State.Factory == productionRallyFactory)))
			{
				ReleaseRally(unit, "replacement order");
			}
		}

		private static void SetRallyMarker(RallyFactory state, Vector3 point)
		{
			if (!state.Factory || state.Factory.IsDestroyed)
			{
				return;
			}
			movingRallyMarker = true;
			try
			{
				state.Factory.MoveRallyPoint(point, playerRequested: false);
				state.Marker = point;
				state.OwnsMarker = true;
			}
			finally
			{
				movingRallyMarker = false;
			}
		}

		private static void ClearRally()
		{
			rallySearch?.Dispose();
			rallySearch = null;
			searchingRallyTicket = null;
			foreach (RallyFactory value in RallyFactories.Values)
			{
				if (Game.GetIsServer() && value.OwnsMarker && (bool)value.Factory && DistanceSq(value.Factory.RallyPoint, value.Marker) < 0.01f)
				{
					SetRallyMarker(value, value.RequestedPoint);
					value.OwnsMarker = false;
				}
				value.Area?.Positions.Dispose();
				value.Area = null;
			}
			RallyTickets.Clear();
			nextRallyPrune = 0f;
			Structure[] array = RallyFactories.Keys.Where((Structure f) => !f || f.IsDestroyed).ToArray();
			foreach (Structure key in array)
			{
				RallyFactories.Remove(key);
			}
		}

		private static void RallyFixCommand(Player player, string action)
		{
			if (!IsAdministrator(player))
			{
				Reply(player, "permission denied: admin Generic power required.");
				return;
			}
			if (!ApiReady)
			{
				Reply(player, "server not initialized.");
				return;
			}
			string[] array = action.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 1 || (array.Length == 2 && array[1].Equals("status", StringComparison.OrdinalIgnoreCase)))
			{
				Reply(player, "Rallyfix requested " + (rallyFixDefault.Value ? "ON" : "OFF") + "; effective " + (RallyEnabled ? "ON" : "OFF") + "; assigned " + RallyTickets.Values.Count((RallyTicket t) => t.Assigned) + "; pending " + RallyTickets.Values.Count((RallyTicket t) => !t.Assigned));
				{
					foreach (RallyTicket item in (from t in RallyTickets.Values
						where !t.Assigned
						orderby t.Serial
						select t).Take(3))
					{
						Reply(player, (item.Unit ? item.Unit.name : "removed unit") + ": " + ((item == searchingRallyTicket) ? "searching parking" : "queued for parking") + "; " + (int)(Time.unscaledTime - item.Created) + "s; " + item.Area.Shape + " " + item.Area.Spacing + "m");
					}
					return;
				}
			}
			if (array.Length != 2 || (!array[1].Equals("on", StringComparison.OrdinalIgnoreCase) && !array[1].Equals("off", StringComparison.OrdinalIgnoreCase)))
			{
				Reply(player, "usage: /formation rallyfix on|off|status");
				return;
			}
			rallyFixDefault.Value = array[1].Equals("on", StringComparison.OrdinalIgnoreCase);
			ClearRally();
			try
			{
				prefs.SaveToFile(false);
				FormatPreferences();
				Reply(player, "Rallyfix " + (rallyFixDefault.Value ? "ON" : "OFF") + " (saved; applies to new production)");
			}
			catch (Exception ex)
			{
				DebugError("Saving Rallyfix: " + ex);
				Reply(player, "Rallyfix updated; CFG save failed.");
			}
		}

		private static void FormationDebugCommand(Player player, string action)
		{
			if (!IsAdministrator(player))
			{
				Reply(player, "permission denied: admin Generic power required.");
				return;
			}
			string[] array = action.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 1 || (array.Length == 2 && array[1].Equals("status", StringComparison.OrdinalIgnoreCase)))
			{
				Reply(player, "Debug " + (debug ? "ON" : "OFF"));
				return;
			}
			if (array.Length != 2 || (!array[1].Equals("on", StringComparison.OrdinalIgnoreCase) && !array[1].Equals("off", StringComparison.OrdinalIgnoreCase)))
			{
				Reply(player, "usage: /formation debug on|off|status");
				return;
			}
			debug = array[1].Equals("on", StringComparison.OrdinalIgnoreCase);
			traceBudget = 80;
			Reply(player, "Debug " + (debug ? "ON" : "OFF") + " (until next round; CFG Debug supplies the default)");
		}

		private static void TickRally()
		{
			if (!RallyEnabled)
			{
				if (RallyTickets.Count > 0 || rallySearch != null || RallyFactories.Values.Any((RallyFactory s) => s.Area != null))
				{
					ClearRally();
				}
				return;
			}
			if (Time.unscaledTime >= nextRallyPrune)
			{
				nextRallyPrune = Time.unscaledTime + 1f;
				RallyTicket[] array = RallyTickets.Values.ToArray();
				foreach (RallyTicket rallyTicket in array)
				{
					if (!RallyCurrent(rallyTicket))
					{
						ReleaseRally(rallyTicket.Unit, "no longer eligible or factory/team changed");
					}
					else if (rallyTicket.Assigned)
					{
						float num2 = DistanceSq(rallyTicket.Unit.transform.position, rallyTicket.Spot.Point);
						if (num2 <= Mathf.Pow(Mathf.Max(3f, rallyTicket.Area.Spacing * 0.25f), 2f))
						{
							rallyTicket.Arrived = true;
						}
						if (rallyTicket.Arrived && num2 > rallyTicket.Area.Spacing * rallyTicket.Area.Spacing)
						{
							ReleaseRally(rallyTicket.Unit, "left parking position");
						}
					}
				}
				Structure[] array2 = RallyFactories.Keys.Where((Structure f) => !f || f.IsDestroyed).ToArray();
				foreach (Structure key in array2)
				{
					RallyFactories[key].Area?.Positions.Dispose();
					RallyFactories.Remove(key);
				}
			}
			if (rallySearch == null)
			{
				RallyTicket rallyTicket2 = (from t in RallyTickets.Values
					where !t.Assigned
					orderby t.Serial
					select t).FirstOrDefault();
				if (rallyTicket2 != null)
				{
					searchingRallyTicket = rallyTicket2;
					rallySearch = FindRallyParking(rallyTicket2).GetEnumerator();
				}
			}
			if (rallySearch == null)
			{
				return;
			}
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				for (int num3 = 0; num3 < 8; num3++)
				{
					if (!(stopwatch.Elapsed.TotalMilliseconds < 2.0))
					{
						break;
					}
					if (!rallySearch.MoveNext())
					{
						rallySearch.Dispose();
						rallySearch = null;
						searchingRallyTicket = null;
						break;
					}
				}
			}
			catch (Exception ex)
			{
				DebugWarning("Rallyfix search: " + ex.Message);
				if (searchingRallyTicket != null && !searchingRallyTicket.Assigned)
				{
					ReleaseRally(searchingRallyTicket.Unit);
				}
				rallySearch?.Dispose();
				rallySearch = null;
				searchingRallyTicket = null;
			}
		}

		private static IEnumerable<bool> FindRallyParking(RallyTicket ticket)
		{
			RallyArea area = ticket.Area;
			if (ticket.Spot == null && area.State.Area == area && area.Preview != null)
			{
				ticket.Spot = area.Preview;
				area.Preview = null;
			}
			int attempt = 0;
			while (true)
			{
				if (attempt < 2048 && RallyCurrent(ticket))
				{
					Vector3 desired;
					if (ticket.Spot != null)
					{
						desired = ticket.Spot.Point;
						ticket.Spot = null;
					}
					else if (!NextRallyPosition(area, out desired))
					{
						goto IL_0436;
					}
					foreach (bool item in ValidateRallyPosition(ticket, desired, preview: false))
					{
						yield return item;
					}
					if (rallyClearance)
					{
						Vector3 vector = rallyProjectedPoint;
						if (!RallyCurrent(ticket))
						{
							break;
						}
						OrderTarget target = new OrderTarget(vector);
						OrderIssueParams issueParams = OrderIssueParams.Ai(AgentMoveSpeed.Fast);
						bool flag = false;
						issuingRally = true;
						generatedDepth++;
						try
						{
							if (ticket.Unit is Vehicle { HasForceMove: not false } vehicle)
							{
								vehicle.RallyTo(vector);
								flag = vehicle.AI_QueueRallyToPoint == vector;
							}
							else
							{
								flag = ticket.Unit.OrderAgent.IssueOrder(OrderDefinitionRegistry.Move, in target, in issueParams);
							}
						}
						finally
						{
							generatedDepth--;
							issuingRally = false;
						}
						if (flag)
						{
							ticket.Spot = new RallySpot
							{
								Point = vector
							};
							ticket.Assigned = true;
							string[] array = new string[5]
							{
								"Rallyfix assigned ",
								ticket.Unit.name,
								" to ",
								null,
								null
							};
							Vector3 vector2 = vector;
							array[3] = vector2.ToString();
							array[4] = " (previous units retained)";
							Trace(string.Concat(array));
							if (area.State.Area != area)
							{
								break;
							}
							SetRallyMarker(area.State, vector);
							for (int next = 0; next < 2048; next++)
							{
								if (!RallyCurrent(ticket))
								{
									break;
								}
								if (area.State.Area != area)
								{
									break;
								}
								if (!NextRallyPosition(area, out desired))
								{
									break;
								}
								foreach (bool item2 in ValidateRallyPosition(ticket, desired, preview: true))
								{
									yield return item2;
								}
								if (rallyClearance)
								{
									if (RallyCurrent(ticket) && area.State.Area == area)
									{
										area.Preview = new RallySpot
										{
											Point = rallyProjectedPoint
										};
										SetRallyMarker(area.State, rallyProjectedPoint);
									}
									break;
								}
							}
							break;
						}
						yield return true;
					}
					attempt++;
					continue;
				}
				goto IL_0436;
				IL_0436:
				if (RallyCurrent(ticket))
				{
					issuingRally = true;
					generatedDepth++;
					try
					{
						ticket.Unit.RallyTo(area.Automatic ? area.State.DefaultPoint : area.Anchor);
					}
					finally
					{
						generatedDepth--;
						issuingRally = false;
					}
					Trace("Rallyfix no free destination within search limit; native rally restored.");
				}
				ReleaseRally(ticket.Unit);
				break;
			}
		}

		private static bool NextRallyPosition(RallyArea area, out Vector3 desired)
		{
			desired = Vector3.zero;
			if (!area.Positions.MoveNext())
			{
				return false;
			}
			Vector3 vector = area.Positions.Current * area.Spacing;
			if (vector.sqrMagnitude > 250000f)
			{
				return false;
			}
			desired = area.Anchor + Vector3.Cross(Vector3.up, area.Forward) * vector.x + area.Forward * vector.z;
			return true;
		}

		private static IEnumerable<Vector3> RallyOffsets(string shape)
		{
			yield return Vector3.zero;
			int side;
			if (shape == "Sol")
			{
				side = 2;
				while (true)
				{
					int low = -(side - 1) / 2;
					int high = side / 2;
					if (side % 2 == 0)
					{
						for (int z = low; z <= high; z++)
						{
							yield return new Vector3(high, 0f, z);
						}
						for (int z = high - 1; z >= low; z--)
						{
							yield return new Vector3(z, 0f, high);
						}
					}
					else
					{
						for (int z = high; z >= low; z--)
						{
							yield return new Vector3(low, 0f, z);
						}
						for (int z = low + 1; z <= high; z++)
						{
							yield return new Vector3(z, 0f, low);
						}
					}
					side++;
				}
			}
			if (shape == "Centauri")
			{
				int[] dq = new int[6] { 0, -1, -1, 0, 1, 1 };
				int[] dr = new int[6] { -1, 0, 1, 1, 0, -1 };
				side = 1;
				while (true)
				{
					int high = side;
					int low = 0;
					for (int z = 0; z < 6; z++)
					{
						for (int step = 0; step < side; step++)
						{
							yield return new Vector3((float)high + (float)low * 0.5f, 0f, (float)low * Mathf.Sqrt(3f) * 0.5f);
							high += dq[z];
							low += dr[z];
						}
					}
					side++;
				}
			}
			side = 1;
			while (true)
			{
				int low = (int)Math.Floor(Math.PI / Math.Asin(1.0 / (double)(2 * side)) + 1E-09);
				for (int high = 0; high < low; high++)
				{
					float f = (float)high * MathF.PI * 2f / (float)low;
					yield return new Vector3(Mathf.Sin(f) * (float)side, 0f, Mathf.Cos(f) * (float)side);
				}
				side++;
			}
		}

		private static IEnumerable<bool> ValidateRallyPosition(RallyTicket ticket, Vector3 desired, bool preview)
		{
			rallyClearance = false;
			if (!RallyCurrent(ticket))
			{
				yield break;
			}
			bool valid = TerrainProjection.TryProject(ticket.Unit, desired, out var point) && DistanceSq(desired, point) <= 1f;
			yield return true;
			if (!valid || !RallyCurrent(ticket))
			{
				yield break;
			}
			OrderTarget target = new OrderTarget(point);
			valid = ticket.Unit.OrderAgent.CanIssueOrder(OrderDefinitionRegistry.Move, in target);
			yield return true;
			if (!valid || !RallyCurrent(ticket))
			{
				yield break;
			}
			RallyArea area = ticket.Area;
			int work = 0;
			if (area.Automatic && RallyOverlaps(area.State.Factory, point, 75f + area.Radius))
			{
				yield break;
			}
			RallyTicket[] array = RallyTickets.Values.ToArray();
			foreach (RallyTicket rallyTicket in array)
			{
				if ((preview || rallyTicket != ticket) && rallyTicket.Spot != null && RallyTooClose(point, rallyTicket.Spot.Point, Mathf.Max(Mathf.Max(area.Spacing, rallyTicket.Area.Spacing), area.Radius + rallyTicket.Area.Radius)))
				{
					yield break;
				}
				int num = work + 1;
				work = num;
				if (num % 32 == 0)
				{
					yield return true;
					if (!RallyCurrent(ticket))
					{
						yield break;
					}
				}
			}
			RallyFactory[] array2 = RallyFactories.Values.ToArray();
			for (int i = 0; i < array2.Length; i++)
			{
				RallyArea area2 = array2[i].Area;
				if (area2?.Preview != null && RallyTooClose(point, area2.Preview.Point, Mathf.Max(Mathf.Max(area.Spacing, area2.Spacing), area.Radius + area2.Radius)))
				{
					yield break;
				}
				int num = work + 1;
				work = num;
				if (num % 32 == 0)
				{
					yield return true;
					if (!RallyCurrent(ticket))
					{
						yield break;
					}
				}
			}
			Structure[] array3 = Structure.Structures.ToArray();
			foreach (Structure structure in array3)
			{
				if ((bool)structure && !structure.IsDestroyed && RallyOverlaps(structure, point, area.Radius))
				{
					yield break;
				}
				int num = work + 1;
				work = num;
				if (num % 32 == 0)
				{
					yield return true;
					if (!RallyCurrent(ticket))
					{
						yield break;
					}
				}
			}
			ConstructionSite[] array4 = ConstructionSite.ConstructionSites.ToArray();
			foreach (ConstructionSite constructionSite in array4)
			{
				if ((bool)constructionSite && !constructionSite.IsDestroyed && RallyOverlaps(constructionSite, point, area.Radius))
				{
					yield break;
				}
				int num = work + 1;
				work = num;
				if (num % 32 == 0)
				{
					yield return true;
					if (!RallyCurrent(ticket))
					{
						yield break;
					}
				}
			}
			Unit[] array5 = Unit.Units.ToArray();
			foreach (Unit unit in array5)
			{
				if ((bool)unit && unit != ticket.Unit && !unit.IsDestroyed && !unit.IsFlyingType && !unit.InCompartment && RallyTooClose(point, unit.transform.position, area.Radius + RallyRadius(unit)))
				{
					yield break;
				}
				int num = work + 1;
				work = num;
				if (num % 32 == 0)
				{
					yield return true;
					if (!RallyCurrent(ticket))
					{
						yield break;
					}
				}
			}
			rallyProjectedPoint = point;
			rallyClearance = true;
		}

		private static bool RallyTooClose(Vector3 first, Vector3 second, float minimum)
		{
			return DistanceSq(first, second) < Mathf.Pow(Mathf.Max(0f, minimum - 0.05f), 2f);
		}

		private static bool RallyOverlaps(BaseGameObject obj, Vector3 point, float radius)
		{
			Bounds physicalBounds = obj.PhysicalBounds;
			if (physicalBounds.size == Vector3.zero)
			{
				return DistanceSq(point, obj.transform.position) < radius * radius;
			}
			Vector3 vector = obj.transform.InverseTransformPoint(point);
			float num = Mathf.Max(0f, Mathf.Abs(vector.x - physicalBounds.center.x) - physicalBounds.extents.x);
			float num2 = Mathf.Max(0f, Mathf.Abs(vector.z - physicalBounds.center.z) - physicalBounds.extents.z);
			return num * num + num2 * num2 < radius * radius;
		}

		private static bool AdminPolicyOverride(ulong id)
		{
			if (!adminMatchLimit.HasValue)
			{
				return AdminLimits.ContainsKey(id);
			}
			return true;
		}

		private static bool ValidOwner(string owner)
		{
			if (!string.IsNullOrWhiteSpace(owner) && owner.Length <= 128)
			{
				return !owner.Any(char.IsControl);
			}
			return false;
		}

		private static bool ValidNames(string[] names)
		{
			return names?.All((string n) => !string.IsNullOrWhiteSpace(n) && !n.Any(char.IsControl)) ?? true;
		}

		private static bool ValidCategory(string rule)
		{
			if (rule == "*")
			{
				return true;
			}
			if (rule == null)
			{
				return false;
			}
			string[] array = rule.Split(':');
			if (array.Length >= 1 && array.Length <= 2 && Enum.GetNames(typeof(UnitType)).Contains(array[0]))
			{
				if (array.Length != 1 && !(array[1] == "*"))
				{
					return Enum.GetNames(typeof(UnitWeight)).Contains(array[1]);
				}
				return true;
			}
			return false;
		}

		private static string[] Copy(string[] values)
		{
			if (values != null)
			{
				return (string[])values.Clone();
			}
			return null;
		}

		private static bool Same(string[] first, string[] second)
		{
			if (first != null)
			{
				if (second != null)
				{
					return first.SequenceEqual(second);
				}
				return false;
			}
			return second == null;
		}

		internal static ChangeResult ApiPolicy(ulong id, string owner, int limit, string[] categories, string[] allowed, string[] denied)
		{
			ChangeResult changeResult = ApiCheck();
			if (changeResult != ChangeResult.Success)
			{
				return changeResult;
			}
			if (!Connected(id))
			{
				return ChangeResult.InvalidPlayer;
			}
			if (!ValidLimit(limit))
			{
				return ChangeResult.InvalidLimit;
			}
			if (!ValidOwner(owner) || !ValidNames(allowed) || !ValidNames(denied) || (categories != null && !categories.All(ValidCategory)))
			{
				return ChangeResult.InvalidPolicy;
			}
			if (PlayerPolicies.TryGetValue(id, out var value) && value.Owner != owner)
			{
				return ChangeResult.OwnerConflict;
			}
			if (value != null && value.Limit == limit && Same(value.Categories, categories) && Same(value.Allowed, allowed) && Same(value.Denied, denied))
			{
				if (!AdminPolicyOverride(id))
				{
					return ChangeResult.Success;
				}
				return ChangeResult.AdminOverride;
			}
			PlayerPolicies[id] = new PlayerPolicy
			{
				Owner = owner,
				Limit = limit,
				Categories = Copy(categories),
				Allowed = Copy(allowed),
				Denied = Copy(denied)
			};
			LimitsChanged();
			if (!AdminPolicyOverride(id))
			{
				return ChangeResult.Success;
			}
			return ChangeResult.AdminOverride;
		}

		internal static ChangeResult ApiReadPolicy(ulong id, out string owner, out int limit, out string[] categories, out string[] allowed, out string[] denied)
		{
			owner = null;
			limit = 0;
			categories = (allowed = (denied = null));
			ChangeResult changeResult = ApiCheck();
			if (changeResult != ChangeResult.Success)
			{
				return changeResult;
			}
			if (!PlayerPolicies.TryGetValue(id, out var value))
			{
				return ChangeResult.NoPolicy;
			}
			owner = value.Owner;
			limit = value.Limit;
			categories = Copy(value.Categories);
			allowed = Copy(value.Allowed);
			denied = Copy(value.Denied);
			if (!AdminPolicyOverride(id))
			{
				return ChangeResult.Success;
			}
			return ChangeResult.AdminOverride;
		}

		internal static ChangeResult ApiClearPolicy(ulong id, string owner)
		{
			ChangeResult changeResult = ApiCheck();
			if (changeResult != ChangeResult.Success)
			{
				return changeResult;
			}
			if (!ValidOwner(owner))
			{
				return ChangeResult.InvalidPolicy;
			}
			if (!PlayerPolicies.TryGetValue(id, out var value))
			{
				return ChangeResult.NoPolicy;
			}
			if (value.Owner != owner)
			{
				return ChangeResult.OwnerConflict;
			}
			PlayerPolicies.Remove(id);
			LimitsChanged();
			return ChangeResult.Success;
		}

		internal static ChangeResult ApiClearOwner(string owner)
		{
			ChangeResult changeResult = ApiCheck();
			if (changeResult != ChangeResult.Success)
			{
				return changeResult;
			}
			if (!ValidOwner(owner))
			{
				return ChangeResult.InvalidPolicy;
			}
			ulong[] array = (from e in PlayerPolicies
				where e.Value.Owner == owner
				select e.Key).ToArray();
			foreach (ulong key in array)
			{
				PlayerPolicies.Remove(key);
			}
			LimitsChanged();
			return ChangeResult.Success;
		}

		internal static ChangeResult ApiDefinitions(out string[] names, out string[] types, out string[] weights)
		{
			names = (types = (weights = Array.Empty<string>()));
			ChangeResult changeResult = ApiCheck();
			if (changeResult != ChangeResult.Success)
			{
				return changeResult;
			}
			if (!ObjectInfo.AllInitialized)
			{
				return ChangeResult.NotReady;
			}
			ObjectInfo[] source = ObjectInfo.ObjectDatas.Where((ObjectInfo i) => (bool)i && (bool)i.Unit).OrderBy((ObjectInfo i) => i.name, StringComparer.Ordinal).ToArray();
			names = source.Select((ObjectInfo i) => i.name).ToArray();
			types = source.Select((ObjectInfo i) => i.UnitType.ToString()).ToArray();
			weights = source.Select((ObjectInfo i) => i.UnitWeight.ToString()).ToArray();
			return ChangeResult.Success;
		}

		private static bool PolicyAllows(Player player, Unit unit)
		{
			ulong num = PlayerId(player);
			if (AdminPolicyOverride(num) || !PlayerPolicies.TryGetValue(num, out var value))
			{
				return true;
			}
			ObjectInfo objectInfo = unit.ObjectInfo;
			if (!objectInfo)
			{
				return false;
			}
			if (value.Denied != null && value.Denied.Contains(objectInfo.name))
			{
				return false;
			}
			if (value.Allowed != null && value.Allowed.Contains(objectInfo.name))
			{
				return true;
			}
			if (value.Categories == null)
			{
				return true;
			}
			string type = objectInfo.UnitType.ToString();
			string weight = objectInfo.UnitWeight.ToString();
			return value.Categories.Any((string rule) => rule == "*" || rule == type || rule == type + ":*" || rule == type + ":" + weight);
		}

		private static bool FpsMove(Player player, Vector3 position)
		{
			if (!Running || generatedDepth > 0)
			{
				return true;
			}
			CancelPlayer(player, keepSoft: true);
			try
			{
				return !TryFps(player, position, null, attack: false);
			}
			catch (Exception ex)
			{
				DebugError("FPS move formation: " + ex);
				return true;
			}
		}

		private static bool FpsAttack(Player player, Target target)
		{
			if (!Running || generatedDepth > 0)
			{
				return true;
			}
			CancelPlayer(player, keepSoft: true);
			if (!ValidTarget(target))
			{
				return true;
			}
			try
			{
				return !TryFps(player, target.transform.position, target, attack: true);
			}
			catch (Exception ex)
			{
				DebugError("FPS attack formation: " + ex);
				return true;
			}
		}

		private static bool ValidTarget(Target t)
		{
			if ((bool)t && (bool)t.Owner && !t.IsDestroyed && (bool)t.NetworkComponent)
			{
				return t.NetworkComponent.IsValid;
			}
			return false;
		}

		private static bool TryFps(Player player, Vector3 centre, Target target, bool attack)
		{
			if (!player || player.IsCommander || !player.ControlledUnit || player.Group == null || player.Group.Leader != player.ControlledUnit)
			{
				return false;
			}
			if ((bool)networkSender && networkSender != player)
			{
				return false;
			}
			if (!FunctionActive(player, attack ? "attack" : "move"))
			{
				return false;
			}
			FormationDefinition formationDefinition = Selected(player, attack ? "attack" : "move");
			if (formationDefinition == null)
			{
				return false;
			}
			AIGroup group = player.Group;
			bool allStopped = group.AllStopped();
			Unit[] array = group.Units.Where((Unit u) => (bool)u && !u.IsDestroyed && !u.PlayerControlled && (allStopped || !group.IgnoresTask(u, out var reason) || reason == AIGroup.EIgnoreTaskReason.FOLLOWING_LEADER)).ToArray();
			Unit[] array2 = array.Where((Unit u) => Available(u) && u.Team == player.Team && u.CanMove).ToArray();
			Unit[] array3 = array.Except(array2).ToArray();
			if (array2.Length <= 2)
			{
				ClearFormationSelection(array);
				return false;
			}
			PrepareFormationSelection(player, array2);
			if (array.Length > ConfiguredPlayerLimit(player) - 1 || array.Length > 50 || array.Any((Unit u) => u.Team != player.Team))
			{
				MoveReply(player, $"formation not applied: FPS limit is {ConfiguredPlayerLimit(player) - 1} eligible ground units; original order retained.");
				return false;
			}
			Vector3 vector = array2.Aggregate(Vector3.zero, (Vector3 sum, Unit u) => sum + u.transform.position) / array2.Length;
			Vector3 forward = Heading(centre - (attack ? vector : player.ControlledUnit.transform.position));
			if (attack && formationDefinition.DirectionSensitive)
			{
				int bestColour = ((IEnumerable<FormationSlot>)formationDefinition.Slots).Min((Func<FormationSlot, int>)FormationDefinition.Colour);
				FormationSlot[] array4 = formationDefinition.Slots.Where((FormationSlot s) => FormationDefinition.Colour(s) == bestColour).ToArray();
				Vector3 vector2 = array4.Aggregate(Vector3.zero, (Vector3 sum, FormationSlot s) => sum + new Vector3(s.X, 0f, s.Z)) / array4.Length;
				if (vector2.sqrMagnitude > 1f)
				{
					forward = Quaternion.Euler(0f, (0f - Mathf.Atan2(vector2.x, vector2.z)) * 57.29578f, 0f) * Heading(vector - centre);
				}
			}
			if (attack)
			{
				OrderTarget intent = new OrderTarget(centre, target);
				if (!array2.Any((Unit u) => u.OrderAgent.CanIssueOrder(OrderDefinitionRegistry.Attack, in intent)))
				{
					return false;
				}
			}
			generatedDepth++;
			try
			{
				group.Task = null;
				Unit[] array5 = array;
				foreach (Unit unit in array5)
				{
					group.StopIgnoringTask(unit);
					Followers.Remove(unit);
					Superseded.Remove(unit);
				}
				if (!attack)
				{
					AITaskBase task = AITaskBase.CreateTask(EAITaskType.Explore, player.Team, 10, centre);
					FpsMoveTask fpsMoveTask = new FpsMoveTask
					{
						Owner = player,
						Task = task
					};
					fpsMoveTask.Units.UnionWith(array2);
					FpsMoveTasks[group] = fpsMoveTask;
					group.Task = task;
				}
				if (array3.Length != 0)
				{
					ClearFormationSelection(array3);
					StrategyMode.PerformMoveAttack(array3.Cast<BaseGameObject>().ToList(), centre, target, AgentMoveSpeed.Normal, attack);
				}
				List<Placement> list = new List<Placement>();
				AttackOperation attackOperation = null;
				if (attack)
				{
					attackOperation = new AttackOperation
					{
						Player = player,
						Team = player.Team,
						Group = group,
						Target = target,
						HadTarget = true,
						OriginalPosition = centre,
						Deadline = Time.unscaledTime + 30f
					};
					attackOperation.Members.AddRange(list);
					Operations.Add(attackOperation);
				}
				QueuePlacementRetries(player, attack ? "attack" : "move", formationDefinition, array2, list, centre, forward, AgentMoveSpeed.Normal, attackOperation);
				Trace((attack ? "attack staging" : "FPS move") + " group=" + array2.Length);
				return true;
			}
			catch (Exception ex)
			{
				DebugError("FPS formation application: " + ex);
				return true;
			}
			finally
			{
				generatedDepth--;
			}
		}

		private static bool CommanderMove(ref List<BaseGameObject> objects, Vector3 worldPosition, Target target, AgentMoveSpeed moveSpeed, bool isAttack, bool remote, bool queueOrder)
		{
			if (!Running || generatedDepth > 0)
			{
				return true;
			}
			Player issuer = (remote ? networkSender : Player.CurrentPlayer);
			if (!issuer || !issuer.IsCommander || objects == null)
			{
				return true;
			}
			try
			{
				if ((queueOrder | isAttack) || (bool)target || Options(issuer).Pending == 2)
				{
					return !HandleLockedOrder(issuer, ref objects, worldPosition, target, moveSpeed, isAttack, queueOrder);
				}
				Unit[] array = (from u in objects.OfType<Unit>()
					where Available(u) && u.Team == issuer.Team && u.CanMove
					select u).Distinct().Take(CommanderOrderLimit).ToArray();
				HashSet<Unit> chosen = new HashSet<Unit>(array);
				PrepareFormationSelection(issuer, array);
				List<BaseGameObject> list = objects.Where((BaseGameObject obj) => !(obj is Unit item) || !chosen.Contains(item)).ToList();
				ClearFormationSelection(from u in list.OfType<Unit>()
					where (bool)u && u.Team == issuer.Team
					select u);
				if (array.Length <= 2)
				{
					if (Options(issuer).Pending == 1)
					{
						Options(issuer).Pending = 0;
					}
					ClearFormationSelection(array);
					return true;
				}
				List<BaseGameObject> objects2 = array.Cast<BaseGameObject>().ToList();
				if (HandleLockedOrder(issuer, ref objects2, worldPosition, target, moveSpeed, attack: false, queued: false))
				{
					objects = list;
					return objects.Count > 0;
				}
				FormationDefinition formationDefinition = Selected(issuer, "commander");
				Unit[] array2 = objects2.OfType<Unit>().ToArray();
				if (formationDefinition == null || !FunctionActive(issuer, "commander") || array2.Length <= 2)
				{
					ClearFormationSelection(array2);
					objects = list.Concat(objects2).ToList();
					return objects.Count > 0;
				}
				Unit[] array3 = array2;
				foreach (Unit unit in array3)
				{
					CancelUnit(unit, keepSoft: true);
					Followers.Remove(unit);
					Superseded.Remove(unit);
				}
				Vector3 zero = Vector3.zero;
				array3 = array2;
				foreach (Unit unit2 in array3)
				{
					zero += unit2.transform.position;
				}
				zero /= (float)array2.Length;
				generatedDepth++;
				try
				{
					QueuePlacementRetries(issuer, "commander", formationDefinition, array2, new List<Placement>(), worldPosition, Heading(worldPosition - zero), moveSpeed);
				}
				finally
				{
					generatedDepth--;
				}
				objects = list;
				return objects.Count > 0;
			}
			catch (Exception e)
			{
				Fault(e);
				return false;
			}
		}

		private static float Radius(Unit u, FormationDefinition f, FormationSlot s)
		{
			Vector3 vector = (u.ObjectInfo ? u.ObjectInfo.PhysicalBounds.size : Vector3.zero);
			float val = ((vector.x > 0f && vector.z > 0f) ? (0.5f * (float)Math.Sqrt(vector.x * vector.x + vector.z * vector.z)) : u.PhysicalRadius);
			return Math.Max(0.5f, val) + 0.5f;
		}

		private static bool Separated(Vector3 point, float radius, IEnumerable<Placement> reserved, Unit self = null)
		{
			return !reserved.Any((Placement p) => p.Unit != self && DistanceSq(point, p.Point) < (radius + p.Radius) * (radius + p.Radius));
		}

		private static Vector3 World(FormationSlot slot, FormationDefinition f, Vector3 centre, Vector3 forward, bool mirrored = false)
		{
			if (!f.DirectionSensitive)
			{
				return centre + new Vector3(slot.X, 0f, slot.Z);
			}
			return centre + Vector3.Cross(Vector3.up, forward) * (mirrored ? (0f - slot.X) : slot.X) + forward * slot.Z;
		}

		private static List<Placement> Commit(List<Placement> plan, AgentMoveSpeed speed)
		{
			List<Placement> list = new List<Placement>();
			foreach (Placement item in plan)
			{
				try
				{
					OrderTarget target = new OrderTarget(item.Point);
					OrderIssueParams issueParams = OrderIssueParams.Commanded(speed);
					if (Available(item.Unit) && item.Unit.OrderAgent.IssueOrder(OrderDefinitionRegistry.Move, in target, in issueParams))
					{
						item.OrderId = ((item.Unit.OrderAgent.Orders.Count > 0) ? item.Unit.OrderAgent.Orders[0].Id : 0u);
						list.Add(item);
					}
				}
				catch (Exception ex)
				{
					DebugWarning("Formation unit order skipped: " + ex.Message);
				}
			}
			return list;
		}

		private static void CancelPlayer(Player player, bool keepSoft = false)
		{
			ClearFpsTravel(player);
			ClearFpsMoveTask(player);
			Operations.RemoveAll((AttackOperation o) => o.Player == player);
			PlacementRetries.RemoveAll((PlacementRetry o) => o.Player == player);
			SoftMotions.RemoveAll((SoftMotion m) => m.Batch.Player == player);
			DepartureSpeeds.RemoveAll((DepartureSpeed g) => g.Batch.Player == player);
			RefreshDepartureSpeeds();
			if (!keepSoft)
			{
				SoftGroups.RemoveAll((SoftGroup g) => g.Player == player);
			}
		}

		private static void CancelUnit(Unit unit, bool keepSoft = false)
		{
			RemoveFpsTravelUnit(unit);
			LockHeadings.Remove(unit);
			SoftMotions.RemoveAll((SoftMotion m) => m.Placement.Unit == unit);
			RemoveDepartureUnit(unit);
			if (!keepSoft)
			{
				ForgetSoftUnit(unit);
			}
			ReleaseLockedUnit(unit);
			foreach (PlacementRetry placementRetry in PlacementRetries)
			{
				placementRetry.Pending.RemoveAll((RetryUnit p) => p.Unit == unit);
				if (placementRetry.Reserved.RemoveAll((Placement p) => p.Unit == unit) > 0)
				{
					placementRetry.Search?.Dispose();
					placementRetry.Search = null;
					placementRetry.Attempt = Math.Max(0, placementRetry.Attempt - 1);
				}
			}
			if (Followers.TryGetValue(unit, out var value) && Frames.TryGetValue(value.Leader, out var value2))
			{
				value2.Search?.Dispose();
				value2.Search = null;
			}
			PlacementRetry[] array = PlacementRetries.Where((PlacementRetry b) => b.Pending.Count == 0).ToArray();
			for (int num = 0; num < array.Length; num++)
			{
				FinishPlacement(array[num]);
			}
			foreach (AttackOperation operation in Operations)
			{
				operation.Members.RemoveAll((Placement p) => p.Unit == unit);
			}
			Operations.RemoveAll((AttackOperation o) => o.Members.Count == 0 && !PlacementRetries.Any((PlacementRetry b) => b.Attack == o && b.Pending.Count > 0) && !SoftMotions.Any((SoftMotion m) => m.Batch.Attack == o));
		}

		private static void TickOperations()
		{
			AttackOperation[] array = Operations.ToArray();
			foreach (AttackOperation o in array)
			{
				if (!o.Player || !Player.Players.Contains(o.Player) || !FunctionActive(o.Player, "attack") || o.Player.Team != o.Team || o.Player.Group != o.Group || o.Player.IsCommander || (o.HadTarget && !ValidTarget(o.Target)))
				{
					Operations.Remove(o);
					continue;
				}
				o.Members.RemoveAll((Placement p) => !Available(p.Unit) || p.Unit.Team != o.Team || p.Unit.AIGroup != o.Group || p.Unit.OrderAgent.Orders.Count > 1 || (p.Unit.OrderAgent.Orders.Count == 1 && (!(p.Unit.OrderAgent.Orders[0] is AIMoveOrder) || p.Unit.OrderAgent.Orders[0].Id != p.OrderId)));
				if (o.Members.Count == 0)
				{
					if (!PlacementRetries.Any((PlacementRetry b) => b.Attack == o && b.Pending.Count > 0) && !SoftMotions.Any((SoftMotion m) => m.Batch.Attack == o))
					{
						Operations.Remove(o);
					}
				}
				else
				{
					if (PlacementRetries.Any((PlacementRetry b) => b.Attack == o && b.Pending.Count > 0) || SoftMotions.Any((SoftMotion m) => m.Batch.Attack == o))
					{
						continue;
					}
					bool flag = o.Members.All((Placement p) => DistanceSq(p.Unit.transform.position, p.Point) <= 25f && Math.Abs(p.Unit.transform.position.y - p.Point.y) <= 5f);
					if (!flag && Time.unscaledTime < o.Deadline)
					{
						continue;
					}
					Operations.Remove(o);
					generatedDepth++;
					try
					{
						foreach (Placement member in o.Members)
						{
							if (Available(member.Unit) && (!o.HadTarget || ValidTarget(o.Target)))
							{
								RemoveFpsTravelUnit(member.Unit);
								OrderTarget target = new OrderTarget(o.OriginalPosition, o.HadTarget ? o.Target : null);
								OrderIssueParams issueParams = OrderIssueParams.Commanded(member.Unit.AIAgent.MoveSpeed);
								if (member.Unit.OrderAgent.CanIssueOrder(OrderDefinitionRegistry.Attack, in target))
								{
									member.Unit.OrderAgent.IssueOrder(OrderDefinitionRegistry.Attack, in target, in issueParams);
								}
							}
						}
					}
					finally
					{
						generatedDepth--;
					}
					Trace("attack restored (" + (flag ? "arrived" : "30s timeout") + ") members=" + o.Members.Count);
				}
			}
		}

		private static string UnitId(Unit u)
		{
			if (!u || !u.ObjectInfo)
			{
				return "";
			}
			return u.ObjectInfo.name;
		}

		private static string PreferenceType(Unit u)
		{
			if (!u)
			{
				return "None";
			}
			return EditorCategories.For(u.ObjectInfo);
		}

		private static IEnumerable<Unit> PlacementOrder(FormationDefinition f, IEnumerable<Unit> units)
		{
			return from u in units
				select new
				{
					Unit = u,
					Match = f.Specificity(UnitId(u), PreferenceType(u))
				} into v
				orderby v.Match.Item1, v.Match.Item2, v.Unit.PhysicalRadius descending, v.Unit.GetInstanceID()
				select v.Unit;
		}

		private static void QueuePlacementRetries(Player player, string function, FormationDefinition f, Unit[] units, List<Placement> issued, Vector3 centre, Vector3 forward, AgentMoveSpeed speed, AttackOperation attack = null)
		{
			foreach (Unit u in units)
			{
				foreach (PlacementRetry placementRetry2 in PlacementRetries)
				{
					placementRetry2.Pending.RemoveAll((RetryUnit p) => p.Unit == u);
				}
			}
			PlacementRetry placementRetry = new PlacementRetry
			{
				Player = player,
				Team = player.Team,
				Group = player.Group,
				Commander = player.IsCommander,
				Function = function,
				Definition = f,
				Centre = centre,
				Forward = forward,
				Speed = speed,
				Attack = attack,
				TravelForward = Heading(centre - units.Aggregate(Vector3.zero, (Vector3 sum, Unit unit) => sum + unit.transform.position) / units.Length)
			};
			placementRetry.Soft = RememberSelection(player, function, f, units, forward);
			if (attack != null)
			{
				placementRetry.Soft.Slots.Clear();
			}
			placementRetry.Reserved.AddRange(issued);
			foreach (Unit item in units.Where((Unit unit) => !issued.Any((Placement p) => p.Unit == unit)))
			{
				placementRetry.Pending.Add(new RetryUnit
				{
					Unit = item,
					Group = item.AIGroup,
					Orders = item.OrderAgent.Orders.Select((AIOrder o) => o.Id).ToArray()
				});
			}
			if (placementRetry.Pending.Count > 0)
			{
				PlacementRetries.Add(placementRetry);
				PrepareInitialMove(placementRetry);
			}
		}

		private static void ClearOrders()
		{
			ClearFpsTravel();
			ClearFpsMoveTasks();
			ClearRally();
			ClearLocks();
			Operations.Clear();
			PlacementRetries.Clear();
			SoftGroups.Clear();
			SoftMotions.Clear();
			DepartureSpeeds.Clear();
			SoftSpeedCaps.Clear();
		}

		private static bool BatchCurrent(PlacementRetry b)
		{
			if ((bool)b.Player && Player.Players.Contains(b.Player) && b.Player.Team == b.Team && b.Player.IsCommander == b.Commander && (b.Commander || b.Player.Group == b.Group) && FunctionActive(b.Player, b.Function) && Selected(b.Player, b.Function) == b.Definition)
			{
				if (b.Attack != null)
				{
					return Operations.Contains(b.Attack);
				}
				return true;
			}
			return false;
		}

		private static bool PendingCurrent(PlacementRetry b, Unit unit)
		{
			RetryUnit retryUnit = b.Pending.FirstOrDefault((RetryUnit v) => v.Unit == unit);
			if (retryUnit != null && Available(unit) && unit.Team == b.Team && unit.AIGroup == retryUnit.Group)
			{
				if (unit.OrderAgent.Orders.Count != 0)
				{
					return retryUnit.Orders.SequenceEqual(unit.OrderAgent.Orders.Select((AIOrder o) => o.Id));
				}
				return true;
			}
			return false;
		}

		private static void TickPlacementRetries()
		{
			if (PlacementRetries.Count == 0)
			{
				return;
			}
			Stopwatch stopwatch = Stopwatch.StartNew();
			int num = 0;
			for (int i = 0; num < 12 && i < 1024 && stopwatch.ElapsedMilliseconds < 2 && PlacementRetries.Count > 0; i++)
			{
				PlacementRetry batch = PlacementRetries[(placementCursor++ & 0x7FFFFFFF) % PlacementRetries.Count];
				if (!BatchCurrent(batch))
				{
					batch.Search?.Dispose();
					PlacementRetries.Remove(batch);
					SoftMotions.RemoveAll((SoftMotion m) => m.Batch == batch);
					continue;
				}
				if (batch.Reserved.RemoveAll((Placement p) => !Available(p.Unit) || p.Unit.Team != batch.Team) > 0)
				{
					batch.Search?.Dispose();
					batch.Search = null;
					batch.Attempt = Math.Max(0, batch.Attempt - 1);
				}
				batch.Pending.RemoveAll((RetryUnit p) => !PendingCurrent(batch, p.Unit));
				if (batch.Pending.Count == 0)
				{
					FinishPlacement(batch);
					continue;
				}
				if (batch.TwoStage)
				{
					int num2 = TickInitialMove(batch);
					if (num2 != 0)
					{
						if (num2 > 0)
						{
							num += num2;
						}
						continue;
					}
				}
				if (Time.unscaledTime < batch.NextAttempt)
				{
					continue;
				}
				if (batch.Search == null)
				{
					batch.Attempt++;
					if (batch.Cache == null)
					{
						batch.Cache = new PlacementCache(batch.Definition, batch.Centre, batch.Forward, batch.Soft.Mirrored, null, (batch.Attack == null) ? null : ((Func<Unit, int, bool>)((Unit u, int index) => Vector3.Dot(Flat(World(batch.Definition.Slots[index], batch.Definition, batch.Centre, batch.Forward, batch.Soft.Mirrored) - batch.Centre), batch.TravelForward) <= 0.01f)), batch.Attack != null);
					}
					batch.Search = SearchRemembered(batch.Soft.Slots, batch.Definition, batch.Pending.Select((RetryUnit p) => p.Unit).ToArray(), batch.Centre, batch.Forward, batch.Reserved, (Unit u) => PendingCurrent(batch, u), (Placement p) => IssuePlacement(batch, p), batch.Soft.Mirrored, batch.Cache).GetEnumerator();
				}
				if (batch.Search.MoveNext())
				{
					if (batch.Search.Current)
					{
						num++;
					}
					continue;
				}
				batch.Search.Dispose();
				batch.Search = null;
				if (batch.Pending.Count == 0 || batch.Attempt >= 3)
				{
					FinishPlacement(batch);
				}
				else
				{
					batch.NextAttempt = Time.unscaledTime + 1.5f;
				}
			}
		}

		private static bool IssuePlacement(PlacementRetry batch, Placement p)
		{
			if (!PendingCurrent(batch, p.Unit))
			{
				return false;
			}
			batch.Soft.Slots[p.Unit] = p.Slot;
			SoftMotions.Add(new SoftMotion
			{
				Batch = batch,
				Placement = p,
				Group = p.Unit.AIGroup,
				Previous = p.Unit.OrderAgent.Orders.Select((AIOrder o) => o.Id).ToArray(),
				Departure = (batch.TwoStage ? Time.unscaledTime : float.PositiveInfinity)
			});
			batch.Pending.RemoveAll((RetryUnit v) => v.Unit == p.Unit);
			return true;
		}

		private static void FinishPlacement(PlacementRetry batch)
		{
			PlacementRetries.Remove(batch);
			batch.Search?.Dispose();
			batch.Search = null;
			SoftMotion[] array = SoftMotions.Where((SoftMotion m) => m.Batch == batch && m.Initial).ToArray();
			foreach (SoftMotion softMotion in array)
			{
				SoftMotions.Remove(softMotion);
				RemoveDepartureUnit(softMotion.Placement.Unit);
			}
			StartSoftDepartures(batch);
			if (batch.Attack != null && BatchCurrent(batch))
			{
				RetryUnit[] array2 = batch.Pending.Where((RetryUnit p) => PendingCurrent(batch, p.Unit)).ToArray();
				for (int num = 0; num < array2.Length; num++)
				{
					Unit unit = array2[num].Unit;
					if (ValidTarget(batch.Attack.Target))
					{
						OrderTarget target = new OrderTarget(batch.Attack.Target.transform.position, batch.Attack.Target);
						OrderIssueParams issueParams = OrderIssueParams.Commanded(batch.Speed);
						generatedDepth++;
						try
						{
							unit.OrderAgent.IssueOrder(OrderDefinitionRegistry.Attack, in target, in issueParams);
						}
						finally
						{
							generatedDepth--;
						}
						batch.Soft.Slots.Remove(unit);
						RemoveFpsTravelUnit(unit);
					}
				}
			}
			if (batch.Pending.Count > 0)
			{
				Trace(batch.Reserved.Count + " units placed; " + batch.Pending.Count + " remain on their previous/native orders after three passes.");
			}
		}

		private static void PrepareInitialMove(PlacementRetry batch)
		{
			if (batch.Attack == null && (batch.Commander || !For(batch.Player).FollowFacingLock) && batch.Soft.Slots.Count == 0 && batch.Pending.Count != 0)
			{
				batch.TwoStage = true;
				Dictionary<Unit, int> rows = SoftDepartureBatches(batch.Pending.Select((RetryUnit p) => p.Unit), batch.Forward);
				SoftMotion[] array = batch.Pending.Select((RetryUnit p) => new SoftMotion
				{
					Batch = batch,
					Group = p.Group,
					Previous = p.Orders,
					Initial = true,
					Departure = Time.unscaledTime + (float)rows[p.Unit] * SoftDelay,
					Placement = new Placement
					{
						Unit = p.Unit,
						Slot = -1,
						Point = batch.Centre
					}
				}).ToArray();
				SoftMotions.AddRange(array);
				BeginDepartureSpeed(batch, array, rows);
			}
		}

		private static int TickInitialMove(PlacementRetry batch)
		{
			SoftMotion[] source = SoftMotions.Where((SoftMotion m) => m.Batch == batch && m.Initial).ToArray();
			foreach (SoftMotion item in source.Where((SoftMotion m) => !PendingCurrent(batch, m.Placement.Unit)))
			{
				SoftMotions.Remove(item);
				RemoveDepartureUnit(item.Placement.Unit);
			}
			source = source.Where((SoftMotion m) => SoftMotions.Contains(m)).ToArray();
			if (source.Length == 0)
			{
				return 0;
			}
			float due = source.Min((SoftMotion m) => m.Departure);
			if (Time.unscaledTime < due)
			{
				return -1;
			}
			SoftMotion[] array = source.Where((SoftMotion m) => m.Departure == due).ToArray();
			List<BaseGameObject> objects = ((IEnumerable<SoftMotion>)array).Select((Func<SoftMotion, BaseGameObject>)((SoftMotion m) => m.Placement.Unit)).ToList();
			generatedDepth++;
			try
			{
				StrategyMode.PerformMoveAttack(objects, batch.Centre, null, batch.Speed, isAttack: false);
			}
			finally
			{
				generatedDepth--;
			}
			SoftMotion[] array2 = array;
			foreach (SoftMotion softMotion in array2)
			{
				Unit unit = softMotion.Placement.Unit;
				RetryUnit retryUnit = batch.Pending.FirstOrDefault((RetryUnit p) => p.Unit == unit);
				if (retryUnit != null && Available(unit))
				{
					retryUnit.Orders = unit.OrderAgent.Orders.Select((AIOrder o) => o.Id).ToArray();
					softMotion.Placement.OrderId = ((retryUnit.Orders.Length == 1) ? retryUnit.Orders[0] : 0u);
					DepartedSoftMotion(softMotion);
				}
				else
				{
					RemoveDepartureUnit(unit);
				}
				SoftMotions.Remove(softMotion);
			}
			return array.Length;
		}

		private static void ClearFormationSelection(IEnumerable<Unit> units)
		{
			foreach (Unit unit in units)
			{
				CancelUnit(unit);
				Followers.Remove(unit);
				Superseded.Remove(unit);
			}
		}

		private static void PrepareFormationSelection(Player player, Unit[] units)
		{
			HashSet<Unit> selected = new HashSet<Unit>(units);
			if (SoftGroups.Where((SoftGroup g) => g.Player == player).Any((SoftGroup g) => g.Groups.Keys.Any(selected.Contains) && g.Groups.Keys.Any((Unit u) => Available(u) && u.Team == player.Team && !selected.Contains(u))) || Locks.Where((LockedGroup g) => g.Owner == player).Any((LockedGroup g) => g.Members.Any((LockedMember m) => selected.Contains(m.Unit)) && g.Members.Any((LockedMember m) => LockEligible(m.Unit, player) && !selected.Contains(m.Unit))))
			{
				for (int num = 0; num < units.Length; num++)
				{
					ForgetSoftUnit(units[num]);
				}
			}
		}

		private static void ForgetSoftUnit(Unit u)
		{
			foreach (SoftGroup softGroup in SoftGroups)
			{
				softGroup.Slots.Remove(u);
				softGroup.Groups.Remove(u);
			}
			SoftGroups.RemoveAll((SoftGroup g) => g.Slots.Count == 0 && !PlacementRetries.Any((PlacementRetry b) => b.Soft == g) && !SoftMotions.Any((SoftMotion m) => m.Batch.Soft == g));
		}

		private static SoftGroup RememberSelection(Player player, string function, FormationDefinition f, Unit[] units, Vector3 forward)
		{
			HashSet<Unit> selected = new HashSet<Unit>(units);
			SoftGroup softGroup = (from g in SoftGroups
				where g.Player == player && g.Definition == f && g.Function == function && g.Groups.Keys.All((Unit u) => !Available(u) || selected.Contains(u))
				orderby units.Count((Unit u) => g.Slots.ContainsKey(u)) descending
				select g).FirstOrDefault();
			SoftGroup softGroup2 = new SoftGroup
			{
				Player = player,
				Team = player.Team,
				Commander = player.IsCommander,
				Function = function,
				Definition = f,
				Forward = forward,
				Mirrored = (function != "attack" && f.DirectionSensitive && softGroup != null && (softGroup.Mirrored ^ Reversed(softGroup.Forward, forward)))
			};
			Unit[] array = units;
			foreach (Unit unit in array)
			{
				if (softGroup != null && softGroup.Groups.TryGetValue(unit, out var value) && unit.AIGroup == value && softGroup.Slots.TryGetValue(unit, out var value2))
				{
					softGroup2.Slots[unit] = value2;
				}
				softGroup2.Groups[unit] = unit.AIGroup;
			}
			array = units;
			for (int num = 0; num < array.Length; num++)
			{
				ForgetSoftUnit(array[num]);
			}
			SoftGroups.Add(softGroup2);
			return softGroup2;
		}

		private static IEnumerable<bool> SearchRemembered(Dictionary<Unit, int> saved, FormationDefinition f, Unit[] units, Vector3 centre, Vector3 forward, List<Placement> reserved, Func<Unit, bool> current, Func<Placement, bool> issue, bool mirrored = false, PlacementCache cache = null)
		{
			if (cache == null)
			{
				cache = new PlacementCache(f, centre, forward, mirrored);
			}
			foreach (Unit u in units)
			{
				if (!current(u) || reserved.Any((Placement p) => p.Unit == u) || !saved.TryGetValue(u, out var index))
				{
					continue;
				}
				if (index < 0 || index >= f.Slots.Length || reserved.Any((Placement p) => p.Slot == index))
				{
					saved.Remove(u);
					continue;
				}
				_ = f.Slots[index];
				if (!cache.Allows(u, index))
				{
					saved.Remove(u);
					continue;
				}
				float radius = cache.UnitRadius(u);
				bool valid = TerrainProjection.TryProject(u, cache.Point(index), out var point) && Separated(point, radius, reserved);
				yield return true;
				if (current(u))
				{
					OrderTarget target = new OrderTarget(point);
					valid = valid && u.OrderAgent.CanIssueOrder(OrderDefinitionRegistry.Move, in target);
					Placement placement = new Placement
					{
						Unit = u,
						Slot = index,
						Point = point,
						Radius = radius
					};
					if (valid && issue(placement))
					{
						reserved.Add(placement);
					}
					else
					{
						saved.Remove(u);
					}
					yield return true;
				}
			}
			foreach (bool item in SearchPositions(f, units, centre, forward, reserved, current, issue, mirrored, cache))
			{
				yield return item;
			}
		}

		private static void StartSoftDepartures(PlacementRetry batch)
		{
			SoftMotion[] array = SoftMotions.Where((SoftMotion m) => m.Batch == batch && !m.Initial && float.IsPositiveInfinity(m.Departure)).ToArray();
			Dictionary<Unit, int> dictionary = SoftDepartureBatches(array.Select((SoftMotion m) => m.Placement.Unit), batch.Forward);
			SoftMotion[] array2 = array;
			foreach (SoftMotion softMotion in array2)
			{
				softMotion.Departure = Time.unscaledTime + (float)dictionary[softMotion.Placement.Unit] * SoftDelay;
			}
			BeginDepartureSpeed(batch, array, dictionary);
			if (batch.Attack != null)
			{
				batch.Attack.Deadline = Time.unscaledTime + 30f;
			}
		}

		private static void RetrySoftMotion(SoftMotion m)
		{
			SoftMotions.Remove(m);
			RemoveDepartureUnit(m.Placement.Unit);
			PlacementRetry b = m.Batch;
			Unit u = m.Placement.Unit;
			b.Soft.Slots.Remove(u);
			b.Reserved.Remove(m.Placement);
			b.Reserved.RemoveAll((Placement p) => !Available(p.Unit) || !b.Soft.Slots.ContainsKey(p.Unit));
			b.Search?.Dispose();
			b.Search = null;
			if (b.Attempt >= 3 || !Available(u))
			{
				return;
			}
			if (!b.Pending.Any((RetryUnit p) => p.Unit == u))
			{
				b.Pending.Add(new RetryUnit
				{
					Unit = u,
					Group = u.AIGroup,
					Orders = u.OrderAgent.Orders.Select((AIOrder o) => o.Id).ToArray()
				});
			}
			b.NextAttempt = Time.unscaledTime + 1.5f;
			if (!PlacementRetries.Contains(b))
			{
				PlacementRetries.Add(b);
			}
		}

		private static void TickSoftMotions()
		{
			SoftGroup[] array = SoftGroups.ToArray();
			foreach (SoftGroup softGroup in array)
			{
				if (!softGroup.Player || !Player.Players.Contains(softGroup.Player) || softGroup.Player.Team != softGroup.Team || softGroup.Player.IsCommander != softGroup.Commander || !FunctionActive(softGroup.Player, softGroup.Function) || Selected(softGroup.Player, softGroup.Function) != softGroup.Definition)
				{
					SoftGroups.Remove(softGroup);
					continue;
				}
				Unit[] array2 = softGroup.Slots.Keys.ToArray();
				foreach (Unit unit in array2)
				{
					if (!Available(unit) || unit.PlayerControlled || unit.IsFlyingType || unit.Team != softGroup.Team || !softGroup.Groups.TryGetValue(unit, out var value) || unit.AIGroup != value)
					{
						softGroup.Slots.Remove(unit);
						softGroup.Groups.Remove(unit);
					}
				}
			}
			Stopwatch stopwatch = Stopwatch.StartNew();
			int count = SoftMotions.Count;
			int k = 0;
			int num = 0;
			for (; k < count; k++)
			{
				if (num >= 12)
				{
					break;
				}
				if (stopwatch.ElapsedMilliseconds >= 2)
				{
					break;
				}
				if (SoftMotions.Count <= 0)
				{
					break;
				}
				SoftMotion softMotion = SoftMotions[(motionCursor++ & 0x7FFFFFFF) % SoftMotions.Count];
				Placement placement = softMotion.Placement;
				Unit unit2 = placement.Unit;
				if (!SoftGroups.Contains(softMotion.Batch.Soft) || !BatchCurrent(softMotion.Batch) || !Available(unit2) || unit2.PlayerControlled || unit2.IsFlyingType || unit2.Team != softMotion.Batch.Team || unit2.AIGroup != softMotion.Group || (unit2.OrderAgent.Orders.Count > 0 && !softMotion.Previous.SequenceEqual(unit2.OrderAgent.Orders.Select((AIOrder o) => o.Id))))
				{
					SoftMotions.Remove(softMotion);
					softMotion.Batch.Reserved.Remove(placement);
					softMotion.Batch.Soft.Slots.Remove(unit2);
				}
				else
				{
					if (softMotion.Initial || Time.unscaledTime < softMotion.Departure)
					{
						continue;
					}
					num++;
					OrderTarget target = new OrderTarget(placement.Point);
					OrderIssueParams issueParams = OrderIssueParams.Commanded(softMotion.Batch.Speed);
					generatedDepth++;
					bool flag;
					try
					{
						UnitCohesionGroup.Detach(unit2);
						flag = unit2.OrderAgent.IssueOrder(OrderDefinitionRegistry.Move, in target, in issueParams);
					}
					finally
					{
						generatedDepth--;
					}
					if (!flag)
					{
						RetrySoftMotion(softMotion);
						continue;
					}
					placement.OrderId = ((unit2.OrderAgent.Orders.Count > 0) ? unit2.OrderAgent.Orders[0].Id : 0u);
					RegisterFpsTravel(softMotion.Batch, placement);
					DepartedSoftMotion(softMotion);
					SoftMotions.Remove(softMotion);
					if (softMotion.Batch.Attack != null)
					{
						softMotion.Batch.Attack.Members.Add(placement);
						softMotion.Batch.Attack.Deadline = Time.unscaledTime + 30f;
					}
				}
			}
		}

		private static void RefreshFollowDepartureCaps()
		{
			FollowDepartureCaps.Clear();
			foreach (FollowFrame frame in Frames.Values)
			{
				if (!frame.Holding || frame.DepartureDelay <= 0f)
				{
					continue;
				}
				Follower[] array = Followers.Values.Where((Follower s) => s.Active && Current(s) && Frames.TryGetValue(s.Leader, out var value) && value == frame).ToArray();
				if (array.Length < 2 || array.All((Follower s) => s.Phase > 0))
				{
					continue;
				}
				int num = array.Max((Follower s) => frame.Rows[s.Unit]);
				if (num == 0)
				{
					continue;
				}
				int stage;
				for (stage = 0; stage < num && array.Where((Follower s) => frame.Rows[s.Unit] <= stage + 1).All((Follower s) => s.Phase > 0); stage++)
				{
				}
				float num2 = array.Min((Follower s) => Math.Max(0f, s.Unit.TopSpeed));
				foreach (Follower item in array.Where((Follower s) => s.Phase > 0))
				{
					float num3 = num2 + Math.Max(0f, item.Unit.TopSpeed - num2) * (float)stage / (float)num;
					if (num3 < item.Unit.TopSpeed)
					{
						FollowDepartureCaps[item.Unit] = num3;
					}
				}
			}
		}

		private static void BeginDepartureSpeed(PlacementRetry batch, SoftMotion[] motions, Dictionary<Unit, int> rows)
		{
			if (SoftDelay <= 0f || motions.Length < 2)
			{
				return;
			}
			DepartureSpeed departureSpeed = new DepartureSpeed
			{
				Batch = batch,
				LastStage = rows.Values.Max()
			};
			if (departureSpeed.LastStage != 0)
			{
				foreach (SoftMotion softMotion in motions)
				{
					departureSpeed.Members.Add(new DepartureMember
					{
						Motion = softMotion,
						Stage = rows[softMotion.Placement.Unit]
					});
				}
				DepartureSpeeds.Add(departureSpeed);
			}
		}

		private static void DepartedSoftMotion(SoftMotion motion)
		{
			foreach (DepartureSpeed departureSpeed in DepartureSpeeds)
			{
				foreach (DepartureMember member in departureSpeed.Members)
				{
					if (member.Motion == motion)
					{
						member.Departed = true;
					}
				}
			}
			RefreshDepartureSpeeds();
		}

		private static void RemoveDepartureUnit(Unit unit)
		{
			foreach (DepartureSpeed departureSpeed in DepartureSpeeds)
			{
				departureSpeed.Members.RemoveAll((DepartureMember m) => m.Motion.Placement.Unit == unit);
			}
			SoftSpeedCaps.Remove(unit);
			FollowDepartureCaps.Remove(unit);
			RefreshDepartureSpeeds();
		}

		private static void RefreshDepartureSpeeds()
		{
			SoftSpeedCaps.Clear();
			DepartureSpeed[] array = DepartureSpeeds.ToArray();
			foreach (DepartureSpeed group in array)
			{
				if (!BatchCurrent(group.Batch) || !SoftGroups.Contains(group.Batch.Soft))
				{
					DepartureSpeeds.Remove(group);
					continue;
				}
				group.Members.RemoveAll((DepartureMember member) =>
				{
					SoftMotion motion = member.Motion;
					Unit unit2 = motion.Placement.Unit;
					if (!Available(unit2) || unit2.Team != group.Batch.Team || unit2.AIGroup != motion.Group)
					{
						return true;
					}
					if (!member.Departed)
					{
						return !SoftMotions.Contains(motion);
					}
					return unit2.OrderAgent.Orders.Count != 1 || unit2.OrderAgent.Orders[0].Id != motion.Placement.OrderId;
				});
				if (group.Members.Count == 0 || group.Members.All((DepartureMember m) => m.Departed))
				{
					DepartureSpeeds.Remove(group);
					continue;
				}
				int stage;
				for (stage = 0; stage < group.LastStage && group.Members.Where((DepartureMember m) => m.Stage <= stage + 1).All((DepartureMember m) => m.Departed); stage++)
				{
				}
				float num = group.Members.Min((DepartureMember m) => Math.Max(0f, m.Motion.Placement.Unit.TopSpeed));
				float num2 = (float)stage / (float)group.LastStage;
				foreach (DepartureMember item in group.Members.Where((DepartureMember m) => m.Departed))
				{
					Unit unit = item.Motion.Placement.Unit;
					float num3 = num + Math.Max(0f, unit.TopSpeed - num) * num2;
					if (num3 < unit.TopSpeed)
					{
						SoftSpeedCaps[unit] = num3;
					}
				}
			}
		}

		private static void ApplyCentreCaps(LockedGroup g)
		{
			if (!g.Traveling || g.Members.Count < 2)
			{
				return;
			}
			Vector3 zero = Vector3.zero;
			foreach (LockedMember member in g.Members)
			{
				zero += Flat(member.Unit.transform.position) - Slot(g, member);
			}
			zero /= (float)g.Members.Count;
			foreach (LockedMember member2 in g.Members)
			{
				if (member2.HasDestination)
				{
					float num = Mathf.Clamp(member2.Radius * 0.5f, 3f, 8f);
					if (!(DistanceSq(member2.Unit.transform.position, member2.LastDestination) <= num * num))
					{
						float num2 = Vector3.Dot(zero + Slot(g, member2) - Flat(member2.Unit.transform.position), g.TravelForward);
						float num3 = Math.Max(4f, member2.Radius * 1.5f);
						float num4;
						if (num2 > num3)
						{
							num4 = (num2 - num3) * 0.5f;
						}
						else
						{
							num4 = ((num2 < 0f - num3) ? ((num2 + num3) * 0.5f) : 0f);
						}
						float value = Mathf.Clamp(g.Baseline + num4, 0f, member2.Maximum);
						LockedSpeedCaps[member2.Unit] = value;
					}
				}
			}
		}

		private static bool Reversed(Vector3 before, Vector3 after)
		{
			if (Flat(before).sqrMagnitude > 0.01f && Flat(after).sqrMagnitude > 0.01f)
			{
				return Vector3.Dot(Heading(before), Heading(after)) < -0.8660254f;
			}
			return false;
		}

		private static void PruneLockHeadings()
		{
			KeyValuePair<Unit, LockHeading>[] array = LockHeadings.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<Unit, LockHeading> keyValuePair = array[i];
				if (!keyValuePair.Value.Owner || !Player.Players.Contains(keyValuePair.Value.Owner) || !keyValuePair.Value.Owner.IsCommander || keyValuePair.Value.Owner.Team != keyValuePair.Value.Team || !LockEligible(keyValuePair.Key, keyValuePair.Value.Owner) || keyValuePair.Key.AIGroup != keyValuePair.Value.Group)
				{
					LockHeadings.Remove(keyValuePair.Key);
				}
			}
		}

		private static Vector3 PreviousLockHeading(Player owner, Unit[] units)
		{
			Vector3 zero = Vector3.zero;
			foreach (Unit unit in units)
			{
				zero += ((LockHeadings.TryGetValue(unit, out var value) && value.Owner == owner && value.Team == unit.Team && value.Group == unit.AIGroup) ? value.Forward : Heading(unit.transform.forward));
			}
			return Heading(zero);
		}

		private static bool ValidDelay(float n)
		{
			if (n != 0f)
			{
				if (!float.IsNaN(n) && n >= 0.1f)
				{
					return n <= 5f;
				}
				return false;
			}
			return true;
		}

		private void RegisterDepartureSettings()
		{
			softDelayDefault = prefs.CreateEntry<float>("StartDelaySoftLock", 0.2f, (string)null, "Seconds between custom formation 20-percent departure batches (maximum five): 0 disables, otherwise 0.1..5", false, false, (ValueValidator)null, (string)null);
			speedLockDelayDefault = prefs.CreateEntry<float>("StartDelaySpeedLock", 0.3f, (string)null, "Seconds between hard-lock speed tiers, slowest TopSpeed first; overrides positional delay. 0 disables, otherwise 0.1..5", false, false, (ValueValidator)null, (string)null);
			lockDelayDefault = prefs.CreateEntry<float>("StartDelayLock", 0f, (string)null, "Seconds between explicit formation lock departure rows: 0 disables, otherwise 0.1..5", false, false, (ValueValidator)null, (string)null);
			RegisterCommanderCommand("startdelayspeedlock", (Player p, string a) =>
			{
				DepartureCommand(p, a, locked: true, bySpeed: true);
			});
			RegisterCommanderCommand("startdelaysoftlock", (Player p, string a) =>
			{
				DepartureCommand(p, a, locked: false);
			});
			RegisterCommanderCommand("startdelaylock", (Player p, string a) =>
			{
				DepartureCommand(p, a, locked: true);
			});
			RegisterCommanderCommand("startdelay", (Player p, string a) =>
			{
				if (!CommandArgument.TryParse(a, out var name, out var quoted) | quoted)
				{
					Reply(p, "usage: /startdelay lock <seconds>");
				}
				else if (!name.StartsWith("lock ", StringComparison.OrdinalIgnoreCase))
				{
					Reply(p, "usage: /startdelay lock <seconds>");
				}
				else
				{
					DepartureCommand(p, "startdelaylock " + name.Substring(5), locked: true);
				}
			});
		}

		private static void DepartureCommand(Player p, string args, bool locked, bool bySpeed = false)
		{
			if (!ApiReady || !IsAdministrator(p))
			{
				Reply(p, "admin Generic power required.");
				return;
			}
			if ((!CommandArgument.TryParse(args, out var name, out var quoted) | quoted) || !float.TryParse(name, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) || !ValidDelay(result))
			{
				Reply(p, "delay: 0 to disable, or 0.1..5 seconds (decimal point).");
				return;
			}
			if (bySpeed)
			{
				if (SpeedLockDelay > 0f || result > 0f)
				{
					lockDelayOverride = 0f;
				}
				speedLockDelayOverride = result;
			}
			else if (locked)
			{
				lockDelayOverride = ((SpeedLockDelay > 0f) ? 0f : result);
			}
			else
			{
				softDelayOverride = result;
			}
			if (locked && !bySpeed && result > 0f && SpeedLockDelay > 0f)
			{
				Reply(p, "Speed-ordered departure is enabled; positional delay remains disabled. Use /startdelayspeedlock 0 first.");
			}
			else
			{
				Player p2 = p;
				string text;
				if (bySpeed)
				{
					text = "Speed-ordered hard-lock";
				}
				else
				{
					text = (locked ? "Lock" : "Soft-lock");
				}
				Reply(p2, text + " departure interval: " + result.ToString("0.###", CultureInfo.InvariantCulture) + " seconds. Applies to new moves this match; configuration unchanged.");
			}
		}

		private static Dictionary<Unit, int> SpeedDepartureRows(IEnumerable<Unit> units)
		{
			Dictionary<Unit, int> dictionary = new Dictionary<Unit, int>();
			float num = 0f;
			int num2 = -1;
			foreach (Unit item in from u in units.Where((Unit u) => u).Distinct()
				orderby u.TopSpeed, u.GetInstanceID()
				select u)
			{
				if (num2 < 0 || item.TopSpeed > num + 0.001f)
				{
					num2++;
					num = item.TopSpeed;
				}
				dictionary[item] = num2;
			}
			return dictionary;
		}

		private static Dictionary<Unit, int> SoftDepartureBatches(IEnumerable<Unit> units, Vector3 forward)
		{
			Unit[] array = (from u in units.Where((Unit u) => u).Distinct()
				orderby Vector3.Dot(Flat(u.transform.position), forward) descending, u.GetInstanceID()
				select u).ToArray();
			int num = Math.Max(1, (array.Length + 4) / 5);
			Dictionary<Unit, int> dictionary = new Dictionary<Unit, int>();
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				dictionary[array[num2]] = num2 / num;
			}
			return dictionary;
		}

		private static Dictionary<Unit, int> DepartureRows(IEnumerable<Unit> units, Vector3 forward)
		{
			Dictionary<Unit, int> dictionary = new Dictionary<Unit, int>();
			float num = 0f;
			float val = 0f;
			int num2 = -1;
			foreach (Unit item in from u in units.Where((Unit u) => u).Distinct()
				orderby Vector3.Dot(Flat(u.transform.position), forward) descending, u.GetInstanceID()
				select u)
			{
				float num3 = Vector3.Dot(Flat(item.transform.position), forward);
				float num4 = Math.Max(3f, 2f * item.PhysicalRadius);
				if (num2 < 0 || num - num3 > Math.Max(val, num4) * 0.6f)
				{
					num2++;
					num = num3;
					val = num4;
				}
				dictionary[item] = num2;
			}
			return dictionary;
		}

		private static float FollowRefreshDistance(Unit unit, bool facingLocked)
		{
			if ((bool)unit.ObjectInfo)
			{
				switch (unit.ObjectInfo.UnitType)
				{
				case UnitType.Infantry:
					if (!facingLocked)
					{
						return 5f;
					}
					return 2f;
				case UnitType.Siege:
					if (!facingLocked)
					{
						return 30f;
					}
					return 15f;
				}
			}
			if (!facingLocked)
			{
				return 20f;
			}
			return 10f;
		}

		private static bool Eligible(Unit u, out Unit leader)
		{
			leader = null;
			if (!Available(u) || Superseded.Contains(u) || u.AIGroup == null || !u.AIGroup.IgnoresTask(u, out var reason) || reason != AIGroup.EIgnoreTaskReason.FOLLOWING_LEADER)
			{
				return false;
			}
			leader = u.AIGroup.Leader;
			if (!leader || leader == u || leader.IsDestroyed || !leader.ControlledBy || !Player.Players.Contains(leader.ControlledBy) || leader.ControlledBy.ControlledUnit != leader || leader.ControlledBy.IsCommander || u.Team != leader.Team)
			{
				return false;
			}
			List<AIOrder> orders = u.OrderAgent.Orders;
			if (orders.Count <= 1)
			{
				if (orders.Count != 0)
				{
					return orders[0] is AIMoveOrder;
				}
				return true;
			}
			return false;
		}

		private static bool Current(Follower s)
		{
			if (Running && Eligible(s.Unit, out var leader) && leader == s.Leader)
			{
				return FunctionActive(leader.ControlledBy, "follow");
			}
			return false;
		}

		private static void ResetLeader(Unit leader)
		{
			if ((bool)leader)
			{
				Follower[] array = Followers.Values.Where((Follower s) => s.Leader == leader).ToArray();
				foreach (Follower follower in array)
				{
					Release(follower);
					Followers.Remove(follower.Unit);
				}
				Frames.Remove(leader);
			}
		}

		private static void ClearFollow()
		{
			Follower[] array = Followers.Values.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				Release(array[i]);
			}
			Followers.Clear();
			Frames.Clear();
			Superseded.Clear();
			FollowDepartureCaps.Clear();
		}

		private static void Release(Follower s)
		{
			FpsSpeedCaps.Remove(s.Unit);
			FollowDepartureCaps.Remove(s.Unit);
			if (s.Active && Available(s.Unit) && Eligible(s.Unit, out var leader) && leader == s.Leader)
			{
				s.Unit.AIAgent.AgentPathfinding.PathfindingSeeker.CancelCurrentPathRequest();
				s.Unit.AIAgent.AgentPathfinding.CompleteMovementAndReset();
			}
			s.Active = false;
		}

		private static void TickFollow()
		{
			Superseded.RemoveWhere((Unit u) => !u || u.IsDestroyed || u.AIGroup == null || !u.AIGroup.IgnoresTask(u, out var reason) || reason != AIGroup.EIgnoreTaskReason.FOLLOWING_LEADER);
			Follower[] array = Followers.Values.ToArray();
			foreach (Follower follower in array)
			{
				if (!Current(follower))
				{
					if (Frames.TryGetValue(follower.Leader, out var value))
					{
						value.Search?.Dispose();
						value.Search = null;
					}
					Followers.Remove(follower.Unit);
				}
			}
			Player[] array2 = Player.Players.ToArray();
			foreach (Player player in array2)
			{
				if (!player || player.IsCommander || !player.ControlledUnit || player.Group == null || player.Group.Leader != player.ControlledUnit || !FunctionActive(player, "follow"))
				{
					continue;
				}
				FormationDefinition formationDefinition = Selected(player, "follow");
				if (formationDefinition == null)
				{
					continue;
				}
				Unit leader = player.ControlledUnit;
				Unit[] array3 = player.Group.Units.Where((Unit u) => Eligible(u, out var leader3) && leader3 == leader).ToArray();
				if (array3.Length == 0)
				{
					ResetLeader(leader);
					continue;
				}
				Vector3 position = leader.transform.position;
				if (!Frames.TryGetValue(leader, out var value2))
				{
					Dictionary<Unit, FollowFrame> frames = Frames;
					Unit key = leader;
					FollowFrame followFrame = new FollowFrame
					{
						LastSample = position
					};
					value2 = followFrame;
					frames[key] = followFrame;
				}
				float num2 = DistanceSq(position, value2.LastSample);
				value2.LastSample = position;
				bool flag = num2 <= 0.04f;
				Vector3 vector = array3.Aggregate(Vector3.zero, (Vector3 sum, Unit u) => sum + u.transform.position) / array3.Length;
				Vector3 v = Flat(position - vector);
				bool facingLocked = For(player).FollowFacingLock;
				Vector3 vector2;
				if (facingLocked)
				{
					vector2 = Heading(leader.transform.forward);
				}
				else if (v.sqrMagnitude > 1f)
				{
					vector2 = Heading(v);
				}
				else
				{
					vector2 = (value2.Holding ? value2.Forward : Vector3.forward);
				}
				float num3 = array3.Min((Unit u) => FollowRefreshDistance(u, facingLocked));
				if (!value2.Holding || DistanceSq(position, value2.Anchor) >= num3 * num3 || (flag && DistanceSq(position, value2.Anchor) > 1f))
				{
					value2.Mirrored = formationDefinition.DirectionSensitive && value2.HeadingDefinition == formationDefinition && (value2.Mirrored ^ Reversed(value2.Forward, vector2));
					value2.HeadingDefinition = formationDefinition;
					value2.Anchor = position;
					value2.Forward = vector2;
					value2.Holding = true;
					value2.Search?.Dispose();
					value2.Search = null;
					foreach (Follower item in Followers.Values.Where((Follower s) => s.Leader == leader && !s.Active))
					{
						item.Attempts = 0;
						item.NextAttempt = 0f;
					}
					if (value2.Rows.Count == 0)
					{
						foreach (KeyValuePair<Unit, int> item2 in SoftDepartureBatches(array3, Heading(position - vector)))
						{
							value2.Rows[item2.Key] = item2.Value;
						}
						value2.DepartureStart = Time.unscaledTime;
						value2.DepartureDelay = SoftDelay;
					}
				}
				foreach (Unit item3 in from u in PlacementOrder(formationDefinition, array3)
					orderby Followers.ContainsKey(u) && Followers[u].Active descending
					select u)
				{
					if (!Followers.TryGetValue(item3, out var value3))
					{
						if (Followers.Values.Count((Follower follower3) => follower3.Leader == leader) >= ConfiguredPlayerLimit(player) - 1)
						{
							continue;
						}
						value3 = (Followers[item3] = new Follower
						{
							Unit = item3,
							Leader = leader,
							Definition = formationDefinition
						});
					}
					if (value3.Definition != formationDefinition)
					{
						Release(value3);
						value3.Definition = formationDefinition;
						value3.Slot = -1;
						value3.Attempts = 0;
						value3.NextAttempt = 0f;
						value3.Rejected.Clear();
						value2.Search?.Dispose();
						value2.Search = null;
					}
					if (value3.Active)
					{
						float num4 = FollowRefreshDistance(item3, facingLocked);
						if (DistanceSq(position, value3.Anchor) >= num4 * num4 || (flag && DistanceSq(position, value3.Anchor) > 1f))
						{
							value3.Anchor = position;
							value3.RequestedAnchor = position;
							value3.RequestedMirrored = value3.Definition.DirectionSensitive && (value3.RequestedMirrored ^ Reversed(value3.RequestedForward, vector2));
							value3.RequestedForward = vector2;
							value3.Refresh = true;
							value3.RefreshAttempts = 0;
							value3.RefreshAt = 0f;
						}
						if (!value3.Refresh)
						{
							SettleFollower(value3, value2);
						}
					}
				}
				if (value2.Search == null)
				{
					StartFollowSearch(leader, formationDefinition, value2);
				}
			}
			Unit[] array4 = Frames.Keys.ToArray();
			foreach (Unit leader2 in array4)
			{
				if (!leader2 || !leader2.ControlledBy || !Player.Players.Contains(leader2.ControlledBy) || leader2.AIGroup == null || !leader2.AIGroup.Units.Any((Unit u) => Eligible(u, out var leader3) && leader3 == leader2))
				{
					Frames.Remove(leader2);
				}
			}
		}

		private static void SettleFollower(Follower s, FollowFrame frame)
		{
			if (s.Phase >= 2 && s.Attempts < 3 && !(Time.unscaledTime < s.NextAttempt) && (!s.Active || !(DistanceSq(s.Unit.transform.position, s.Destination) <= 9f) || !(Math.Abs(s.Unit.transform.position.y - s.Destination.y) <= 3f)))
			{
				s.Attempts++;
				s.NextAttempt = Time.unscaledTime + 1.5f;
				if (s.Active)
				{
					IssueFixedMove(s, s.Destination);
				}
			}
		}

		private static void StartFollowSearch(Unit leader, FormationDefinition definition, FollowFrame frame)
		{
			Follower[] array = Followers.Values.Where((Follower s) => s.Leader == leader && !s.Active && s.Attempts < 3 && Time.unscaledTime >= s.NextAttempt).ToArray();
			if (array.Length != 0)
			{
				Follower[] array2 = array;
				foreach (Follower obj in array2)
				{
					obj.Attempts++;
					obj.NextAttempt = Time.unscaledTime + 1.5f;
				}
				List<Placement> reserved = (from s in Followers.Values
					where s.Leader == leader && s.Active
					select new Placement
					{
						Unit = s.Unit,
						Slot = s.Slot,
						Point = s.Destination,
						Radius = s.Radius
					}).ToList();
				frame.Search = SearchRemembered(array.Where((Follower s) => s.Slot >= 0).ToDictionary((Follower s) => s.Unit, (Follower s) => s.Slot), definition, array.Select((Follower s) => s.Unit).ToArray(), frame.Anchor, frame.Forward, reserved, Valid, Issue, frame.Mirrored).GetEnumerator();
			}
			bool Issue(Placement p)
			{
				if (!Valid(p.Unit))
				{
					return false;
				}
				Follower follower = Followers[p.Unit];
				follower.Phase = 0;
				if (!frame.Rows.ContainsKey(p.Unit))
				{
					frame.Rows[p.Unit] = ((frame.Rows.Count != 0) ? Math.Min(4, frame.Rows.Values.Max() + 1) : 0);
				}
				follower.DepartAt = frame.DepartureStart + (float)frame.Rows[p.Unit] * frame.DepartureDelay;
				follower.Slot = p.Slot;
				follower.Destination = p.Point;
				follower.Radius = p.Radius;
				follower.Active = true;
				follower.NextAttempt = Time.unscaledTime + 1.5f;
				follower.Anchor = frame.Anchor;
				follower.RequestedForward = frame.Forward;
				follower.RequestedMirrored = frame.Mirrored;
				follower.Refresh = false;
				return true;
			}
			bool Valid(Unit u)
			{
				if (frame.Holding && (bool)leader && Followers.TryGetValue(u, out var value) && !value.Active && value.Definition == definition && Current(value))
				{
					return Selected(leader.ControlledBy, "follow") == definition;
				}
				return false;
			}
		}

		private static void TickFollowSearches()
		{
			FollowFrame[] array = Frames.Values.Where((FollowFrame f) => f.Search != null).ToArray();
			if (array.Length == 0)
			{
				return;
			}
			Stopwatch stopwatch = Stopwatch.StartNew();
			int num = 0;
			int num2 = 0;
			while (num < 12 && num2 < 1024 && stopwatch.ElapsedMilliseconds < 2)
			{
				FollowFrame frame = array[(followSearchCursor++ & 0x7FFFFFFF) % array.Length];
				if (frame.Search != null)
				{
					if (!frame.Search.MoveNext())
					{
						frame.Search.Dispose();
						frame.Search = null;
						if (float.IsPositiveInfinity(frame.DepartureStart))
						{
							frame.DepartureStart = Time.unscaledTime;
						}
						foreach (Follower item in Followers.Values.Where((Follower s) => s.Active && s.Phase == 0 && Frames.TryGetValue(s.Leader, out var value) && value == frame))
						{
							item.DepartAt = frame.DepartureStart + (float)frame.Rows[item.Unit] * frame.DepartureDelay;
						}
					}
					else if (frame.Search.Current)
					{
						num++;
					}
				}
				num2++;
			}
		}

		private static void TickFollowDepartures()
		{
			int num = 0;
			Follower[] array = Followers.Values.ToArray();
			foreach (Follower s in array)
			{
				if (!s.Active || !Current(s))
				{
					continue;
				}
				if (s.Refresh && s.RefreshAttempts >= 3)
				{
					s.Refresh = false;
				}
				if (s.Refresh)
				{
					if (Time.unscaledTime < s.RefreshAt)
					{
						continue;
					}
					if (++num > 12)
					{
						break;
					}
					s.RefreshAttempts++;
					s.RefreshAt = Time.unscaledTime + 1.5f;
					Vector3 desired = World(s.Definition.Slots[s.Slot], s.Definition, s.RequestedAnchor, s.RequestedForward, s.RequestedMirrored);
					if (!TerrainProjection.TryProject(s.Unit, desired, out var point))
					{
						continue;
					}
					IEnumerable<Placement> reserved = from o in Followers.Values
						where o != s && o.Leader == s.Leader && o.Active
						select new Placement
						{
							Unit = o.Unit,
							Radius = o.Radius,
							Point = (o.Refresh ? World(o.Definition.Slots[o.Slot], o.Definition, o.RequestedAnchor, o.RequestedForward, o.RequestedMirrored) : o.Destination)
						};
					if (Separated(point, s.Radius, reserved, s.Unit))
					{
						OrderTarget target = new OrderTarget(point);
						if (s.Unit.OrderAgent.CanIssueOrder(OrderDefinitionRegistry.Move, in target) && (s.Phase < 2 || IssueFixedMove(s, point)))
						{
							s.Destination = point;
							s.Refresh = false;
							s.Attempts = 0;
							s.NextAttempt = Time.unscaledTime + 1.5f;
						}
					}
				}
				else
				{
					if (s.Phase >= 2 || Time.unscaledTime < s.DepartAt)
					{
						continue;
					}
					if (++num > 12)
					{
						break;
					}
					if (IssueFixedMove(s, s.Destination))
					{
						s.Phase = 2;
						RefreshFollowDepartureCaps();
						continue;
					}
					s.Active = false;
					s.Slot = -1;
					s.NextAttempt = Time.unscaledTime + 1.5f;
					if (Frames.TryGetValue(s.Leader, out var value))
					{
						value.Search?.Dispose();
						value.Search = null;
					}
				}
			}
			RefreshFollowDepartureCaps();
		}

		private static bool IssueFixedMove(Follower s, Vector3 point)
		{
			OrderTarget target = new OrderTarget(point);
			OrderIssueParams issueParams = OrderIssueParams.Ai(s.Unit.AIAgent.MoveSpeed, broadcastEvent: false);
			generatedDepth++;
			try
			{
				UnitCohesionGroup.Detach(s.Unit);
				bool flag = s.Unit.OrderAgent.IssueOrder(OrderDefinitionRegistry.Move, in target, in issueParams);
				if (flag)
				{
					s.Order = ((s.Unit.OrderAgent.Orders.Count > 0) ? s.Unit.OrderAgent.Orders[0].Id : 0u);
				}
				return flag;
			}
			finally
			{
				generatedDepth--;
			}
		}

		private static void RegisterFpsTravel(PlacementRetry batch, Placement placement)
		{
			if (!batch.Commander)
			{
				FpsTravel fpsTravel = FpsTravels.FirstOrDefault((FpsTravel g) => g.Batch == batch);
				if (fpsTravel == null)
				{
					fpsTravel = new FpsTravel
					{
						Batch = batch
					};
					FpsTravels.Add(fpsTravel);
				}
				fpsTravel.Members.RemoveAll((FpsTravelMember m) => m.Placement.Unit == placement.Unit);
				fpsTravel.Members.Add(new FpsTravelMember
				{
					Placement = placement,
					Group = placement.Unit.AIGroup
				});
			}
		}

		private static void RemoveFpsTravelUnit(Unit unit)
		{
			FpsSpeedCaps.Remove(unit);
			foreach (FpsTravel fpsTravel in FpsTravels)
			{
				fpsTravel.Members.RemoveAll((FpsTravelMember m) => m.Placement.Unit == unit);
			}
		}

		private static void ClearFpsSpeedCaps(Player owner)
		{
			foreach (FpsTravel item in FpsTravels.Where((FpsTravel g) => g.Batch.Player == owner))
			{
				foreach (FpsTravelMember member in item.Members)
				{
					FpsSpeedCaps.Remove(member.Placement.Unit);
				}
			}
			foreach (Follower item2 in Followers.Values.Where((Follower s) => (bool)s.Leader && s.Leader.ControlledBy == owner))
			{
				FpsSpeedCaps.Remove(item2.Unit);
			}
		}

		private static void ClearFpsTravel(Player owner = null)
		{
			if ((object)owner == null)
			{
				FpsTravels.Clear();
				FpsSpeedCaps.Clear();
				return;
			}
			ClearFpsSpeedCaps(owner);
			FpsTravels.RemoveAll((FpsTravel g) => g.Batch.Player == owner);
		}

		private static bool OwnedMove(Unit unit, uint order)
		{
			if (Available(unit) && order != 0 && unit.OrderAgent.Orders.Count == 1 && unit.OrderAgent.Orders[0] is AIMoveOrder)
			{
				return unit.OrderAgent.Orders[0].Id == order;
			}
			return false;
		}

		private static void TickFpsCoordination()
		{
			if (Time.unscaledTime < nextFpsPace)
			{
				return;
			}
			nextFpsPace = Time.unscaledTime + 0.2f;
			FpsSpeedCaps.Clear();
			FpsTravel[] array = FpsTravels.ToArray();
			foreach (FpsTravel fpsTravel in array)
			{
				PlacementRetry b = fpsTravel.Batch;
				if (!BatchCurrent(b))
				{
					FpsTravels.Remove(fpsTravel);
					continue;
				}
				fpsTravel.Members.RemoveAll((FpsTravelMember m) => !OwnedMove(m.Placement.Unit, m.Placement.OrderId) || m.Placement.Unit.AIGroup != m.Group);
				if (fpsTravel.Members.Count == 0)
				{
					FpsTravels.Remove(fpsTravel);
				}
				else if (!PlacementRetries.Contains(b) && !SoftMotions.Any((SoftMotion m) => m.Batch == b) && fpsTravel.Members.All((FpsTravelMember m) => DistanceSq(m.Placement.Unit.transform.position, m.Placement.Point) <= 9f))
				{
					FpsTravels.Remove(fpsTravel);
				}
				else
				{
					if (!For(b.Player).FollowFacingLock)
					{
						continue;
					}
					ApplyFpsPace(fpsTravel.Members.Select((FpsTravelMember m) => new PaceSample
					{
						Unit = m.Placement.Unit,
						Radius = m.Placement.Radius,
						Position = Vector3.Dot(Flat(m.Placement.Unit.transform.position), b.TravelForward),
						Offset = Vector3.Dot(Flat(m.Placement.Point - b.Centre), b.TravelForward),
						OutsideSince = m.OutsideSince,
						Recovering = m.Recovering,
						Save = (bool recovering, float since) =>
						{
							m.Recovering = recovering;
							m.OutsideSince = since;
						}
					}).ToArray());
				}
			}
			KeyValuePair<Unit, FollowFrame>[] array2 = Frames.ToArray();
			for (int i = 0; i < array2.Length; i++)
			{
				KeyValuePair<Unit, FollowFrame> keyValuePair = array2[i];
				Unit leader = keyValuePair.Key;
				FollowFrame frame = keyValuePair.Value;
				if (!leader || !leader.ControlledBy || !For(leader.ControlledBy).FollowFacingLock)
				{
					continue;
				}
				Follower[] array3 = Followers.Values.Where((Follower s) => s.Leader == leader && s.Active && s.Phase >= 2 && Current(s) && OwnedMove(s.Unit, s.Order)).ToArray();
				if (array3.Length == 0)
				{
					continue;
				}
				ApplyFpsPace(array3.Select((Follower s) => new PaceSample
				{
					Unit = s.Unit,
					Radius = s.Radius,
					Position = Vector3.Dot(Flat(s.Unit.transform.position), frame.Forward),
					Offset = Vector3.Dot(Flat(World(s.Definition.Slots[s.Slot], s.Definition, Vector3.zero, frame.Forward, frame.Mirrored)), frame.Forward),
					OutsideSince = s.OutsideSince,
					Recovering = s.Recovering,
					Save = (bool recovering, float since) =>
					{
						s.Recovering = recovering;
						s.OutsideSince = since;
					}
				}).ToArray());
			}
		}

		private static void ApplyFpsPace(PaceSample[] members)
		{
			if (members.Length == 0)
			{
				return;
			}
			PaceSample[] array = members.Where((PaceSample m) => !m.Recovering).ToArray();
			if (array.Length == 0)
			{
				array = members;
			}
			float[] array2 = (from m in array
				select m.Position - m.Offset into v
				orderby v
				select v).ToArray();
			float num = array2[array2.Length / 2];
			float num2 = array.Min((PaceSample m) => Math.Max(0.1f, m.Unit.TopSpeed));
			foreach (PaceSample paceSample in members)
			{
				float num4 = num + paceSample.Offset - paceSample.Position;
				float num5 = Math.Max(3f, paceSample.Radius * 1.5f);
				float num6 = Math.Max(25f, num5 * 4f);
				if (num4 > num6)
				{
					if (paceSample.OutsideSince < 0f)
					{
						paceSample.OutsideSince = Time.unscaledTime;
					}
					if (Time.unscaledTime - paceSample.OutsideSince >= 5f)
					{
						paceSample.Recovering = true;
					}
				}
				else if (num4 < num6 * 0.6f)
				{
					paceSample.Recovering = false;
					paceSample.OutsideSince = -1f;
				}
				paceSample.Save(paceSample.Recovering, paceSample.OutsideSince);
				if (!paceSample.Recovering)
				{
					float num7;
					if (num4 > num5)
					{
						num7 = (num4 - num5) * 0.5f;
					}
					else
					{
						num7 = ((num4 < 0f - num5) ? ((num4 + num5) * 0.5f) : 0f);
					}
					FpsSpeedCaps[paceSample.Unit] = Mathf.Clamp(num2 + num7, 0f, paceSample.Unit.TopSpeed);
				}
			}
		}

		private static void ClearFpsMoveTask(Player player)
		{
			KeyValuePair<AIGroup, FpsMoveTask>[] array = FpsMoveTasks.Where((KeyValuePair<AIGroup, FpsMoveTask> e) => e.Value.Owner == player).ToArray();
			for (int num = 0; num < array.Length; num++)
			{
				KeyValuePair<AIGroup, FpsMoveTask> keyValuePair = array[num];
				FpsMoveTasks.Remove(keyValuePair.Key);
				if (keyValuePair.Key.Task == keyValuePair.Value.Task)
				{
					generatedDepth++;
					try
					{
						keyValuePair.Key.Task = null;
					}
					finally
					{
						generatedDepth--;
					}
				}
			}
		}

		private static void ClearFpsMoveTasks()
		{
			Player[] array = FpsMoveTasks.Values.Select((FpsMoveTask v) => v.Owner).Distinct().ToArray();
			for (int num = 0; num < array.Length; num++)
			{
				ClearFpsMoveTask(array[num]);
			}
		}

		private static IEnumerable<CodeInstruction> PatchFpsMoveTask(IEnumerable<CodeInstruction> instructions)
		{
			int count = 0;
			MethodInfo original = AccessTools.Method(typeof(AIOrderProcessor), "IssueResolvedOrder", (Type[])null, (Type[])null);
			foreach (CodeInstruction instruction in instructions)
			{
				if (CodeInstructionExtensions.Calls(instruction, original))
				{
					instruction.opcode = OpCodes.Call;
					instruction.operand = AccessTools.Method(typeof(Formations), "FpsTaskMove", (Type[])null, (Type[])null);
					count++;
				}
				yield return instruction;
			}
			if (count != 2)
			{
				throw new InvalidOperationException("Expected two FPS group-task repath sites.");
			}
		}

		private static bool FpsTaskMove(AIOrderProcessor processor, Vector3 position, Target target, in OrderIssueParams parameters)
		{
			if (Running && processor.Owner is Unit { AIGroup: not null } unit && FpsMoveTasks.TryGetValue(unit.AIGroup, out var value) && value.Task == unit.AIGroup.Task && (bool)value.Owner && value.Owner.Group == unit.AIGroup && value.Units.Contains(unit))
			{
				return true;
			}
			return processor.IssueResolvedOrder(position, target, in parameters);
		}

		private static void FollowOrientationCommand(Player p, string args)
		{
			if (!Ready || !Game.GetIsServer() || !p || p.IsCommander || !p.ControlledUnit || p.Group == null || p.Group.Leader != p.ControlledUnit)
			{
				Reply(p, "followformation orientation is only available to FPS squad leaders.");
			}
			else
			{
				if ((bool)networkSender && networkSender != p)
				{
					return;
				}
				if ((!CommandArgument.TryParse(args, out var name, out var quoted) | quoted) || !new string[3] { "lock", "unlock", "status" }.Contains(name, StringComparer.OrdinalIgnoreCase))
				{
					Reply(p, "usage: /followformation lock|unlock|status");
					return;
				}
				Selection selection = For(p);
				if (!name.Equals("status", StringComparison.OrdinalIgnoreCase))
				{
					selection.FollowFacingLock = name.Equals("lock", StringComparison.OrdinalIgnoreCase);
					if (!selection.FollowFacingLock)
					{
						ClearFpsSpeedCaps(p);
					}
					Menus.Remove(p);
				}
				Reply(p, selection.FollowFacingLock ? "Formation lock ON: Move, Follow and Attack staging coordinate speed. Follow captures your facing at movement updates." : "Formation lock OFF: Follow uses squad travel direction and the normal moving-slot system.");
			}
		}

		private void InstallCommanderHandoverFix()
		{
			poolArmyGroup = (Action<AIGroup>)Delegate.CreateDelegate(typeof(Action<AIGroup>), AccessTools.Method(typeof(AIGroup), "ReturnToPool", Type.EmptyTypes, (Type[])null));
			Patch(typeof(AIUnitHandler), "Init", null, null, "PatchCommanderHandover");
			Patch(typeof(AIUnitHandler), "Deinit", null, null, "PatchCommanderHandover");
		}

		private static IEnumerable<CodeInstruction> PatchCommanderHandover(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo tasks = AccessTools.Method(typeof(AITaskBase), "ReturnToPool", new Type[1] { typeof(Team) }, (Type[])null);
			MethodInfo groups = AccessTools.Method(typeof(AIGroup), "ReturnToPool", new Type[1] { typeof(Team) }, (Type[])null);
			int taskCalls = 0;
			int groupCalls = 0;
			foreach (CodeInstruction instruction in instructions)
			{
				if (CodeInstructionExtensions.Calls(instruction, tasks))
				{
					instruction.operand = AccessTools.Method(typeof(Formations), "PoolCommanderTasks", (Type[])null, (Type[])null);
					taskCalls++;
				}
				else if (CodeInstructionExtensions.Calls(instruction, groups))
				{
					instruction.operand = AccessTools.Method(typeof(Formations), "PoolCommanderGroups", (Type[])null, (Type[])null);
					groupCalls++;
				}
				yield return instruction;
			}
			if (taskCalls != 1 || groupCalls != 1)
			{
				throw new InvalidOperationException("Expected one task and one group cleanup in AI commander unit-handler handover.");
			}
		}

		private static bool FpsOwnedGroup(AIGroup group)
		{
			if (group != null && (bool)group.Leader && !group.Leader.IsDestroyed)
			{
				return group.PlayerControlled;
			}
			return false;
		}

		private static void PoolCommanderTasks(Team team)
		{
			if (!Ready || !Game.GetIsServer() || !team)
			{
				AITaskBase.ReturnToPool(team);
				return;
			}
			AITaskBase[] array = team.Tasks.ToArray();
			foreach (AITaskBase aITaskBase in array)
			{
				if (aITaskBase != null && aITaskBase.Team == team && aITaskBase.Type != EAITaskType.Sentry && !aITaskBase.Groups.Any(FpsOwnedGroup))
				{
					aITaskBase.ReturnToPool();
				}
			}
		}

		private static void PoolCommanderGroups(Team team)
		{
			if (!Ready || !Game.GetIsServer() || !team)
			{
				AIGroup.ReturnToPool(team);
				return;
			}
			AIGroup[] array = team.Groups.ToArray();
			foreach (AIGroup aIGroup in array)
			{
				if (aIGroup != null && aIGroup.Team == team && !FpsOwnedGroup(aIGroup) && (aIGroup.Task == null || aIGroup.Task.Type != EAITaskType.Sentry))
				{
					poolArmyGroup(aIGroup);
				}
			}
		}

		private static IEnumerable<bool> SearchPositions(FormationDefinition f, Unit[] units, Vector3 centre, Vector3 forward, List<Placement> reserved, Func<Unit, bool> current, Func<Placement, bool> issue, bool mirrored = false, PlacementCache cache = null)
		{
			if (cache == null)
			{
				cache = new PlacementCache(f, centre, forward, mirrored);
			}
			HashSet<Unit> assigned = new HashSet<Unit>(reserved.Select((Placement p) => p.Unit));
			HashSet<int> used = new HashSet<int>(reserved.Select((Placement p) => p.Slot));
			for (int colour = 0; colour < 4; colour++)
			{
				Dictionary<Unit, List<Placement>> edges = new Dictionary<Unit, List<Placement>>();
				Unit[] array = units;
				foreach (Unit u in array)
				{
					if (assigned.Contains(u) || !current(u))
					{
						continue;
					}
					List<Placement> candidates = new List<Placement>();
					edges[u] = candidates;
					for (int index = 0; index < f.Slots.Length; index++)
					{
						_ = f.Slots[index];
						if (cache.Colour(index) != colour || used.Contains(index) || !cache.Allows(u, index))
						{
							continue;
						}
						if (!current(u))
						{
							break;
						}
						float radius = cache.UnitRadius(u);
						Vector3 vector = cache.Point(index);
						if (Separated(vector, radius, reserved) && TerrainProjection.TryProject(u, vector, out var point) && Separated(point, radius, reserved))
						{
							OrderTarget target = new OrderTarget(point);
							if (u.OrderAgent.CanIssueOrder(OrderDefinitionRegistry.Move, in target))
							{
								candidates.Add(new Placement
								{
									Unit = u,
									Slot = index,
									Point = point,
									Radius = radius
								});
							}
						}
						yield return true;
					}
					candidates.Sort((Placement a, Placement b) =>
					{
						int num2 = cache.Rank(u, a.Slot).CompareTo(cache.Rank(u, b.Slot));
						if (num2 == 0 && cache.NearestFirst)
						{
							num2 = DistanceSq(u.transform.position, a.Point).CompareTo(DistanceSq(u.transform.position, b.Point));
						}
						return (num2 == 0) ? a.Slot.CompareTo(b.Slot) : num2;
					});
				}
				while (true)
				{
					array = edges.Keys.ToArray();
					foreach (Unit unit in array)
					{
						if (assigned.Contains(unit) || !current(unit))
						{
							edges.Remove(unit);
							continue;
						}
						edges[unit].RemoveAll((Placement p) => used.Contains(p.Slot) || !Separated(p.Point, p.Radius, reserved));
						yield return false;
					}
					Dictionary<int, Placement> owners = new Dictionary<int, Placement>();
					foreach (bool item in MatchPlacements(f, edges, owners, cache))
					{
						yield return item;
					}
					if (owners.Count == 0)
					{
						break;
					}
					foreach (Placement item2 in from p in owners.Values
						orderby cache.Rank(p.Unit, p.Slot), edges[p.Unit].Count, p.Slot
						select p)
					{
						if (current(item2.Unit) && Separated(item2.Point, item2.Radius, reserved) && issue(item2))
						{
							reserved.Add(item2);
							used.Add(item2.Slot);
							assigned.Add(item2.Unit);
						}
						else
						{
							edges[item2.Unit].Remove(item2);
						}
						yield return true;
					}
				}
			}
		}

		private static IEnumerable<bool> MatchPlacements(FormationDefinition f, Dictionary<Unit, List<Placement>> edges, Dictionary<int, Placement> owners, PlacementCache cache)
		{
			int basis = edges.Count + 1;
			while (true)
			{
				HashSet<Unit> matched = new HashSet<Unit>(owners.Values.Select((Placement p) => p.Unit));
				Dictionary<Unit, long> distance = edges.Keys.ToDictionary((Unit u) => u, (Unit u) => (!matched.Contains(u)) ? 0 : 2305843009213693951L);
				Dictionary<Unit, Placement> previous = new Dictionary<Unit, Placement>();
				for (int pass = 0; pass < edges.Count; pass++)
				{
					bool changed = false;
					int checks = 0;
					foreach (KeyValuePair<Unit, List<Placement>> edge in edges)
					{
						if (distance[edge.Key] == 2305843009213693951L)
						{
							continue;
						}
						foreach (Placement item in edge.Value)
						{
							if (owners.TryGetValue(item.Slot, out var value) && value.Unit != item.Unit)
							{
								long num = distance[item.Unit] + Cost(item) - Cost(value);
								if (num < distance[value.Unit])
								{
									distance[value.Unit] = num;
									previous[value.Unit] = item;
									changed = true;
								}
							}
							int num2 = checks + 1;
							checks = num2;
							if (num2 % 64 == 0)
							{
								yield return false;
							}
						}
					}
					yield return false;
					if (!changed)
					{
						break;
					}
				}
				Placement end = null;
				long best = 2305843009213693951L;
				foreach (KeyValuePair<Unit, List<Placement>> edge2 in edges)
				{
					if (distance[edge2.Key] == 2305843009213693951L)
					{
						continue;
					}
					foreach (Placement item2 in edge2.Value)
					{
						if (!owners.ContainsKey(item2.Slot) && distance[item2.Unit] + Cost(item2) < best)
						{
							best = distance[item2.Unit] + Cost(item2);
							end = item2;
						}
					}
					yield return false;
				}
				if (end != null)
				{
					do
					{
						owners[end.Slot] = end;
					}
					while (previous.TryGetValue(end.Unit, out end));
					yield return false;
					continue;
				}
				break;
			}
			long Cost(Placement p)
			{
				long num3 = cache.Rank(p.Unit, p.Slot) switch
				{
					2 => basis * basis + basis, 
					1 => basis * basis, 
					0 => 0, 
					_ => basis * basis + basis + 1, 
				};
				if (!cache.NearestFirst)
				{
					return num3;
				}
				return num3 * 1000000 + (long)Math.Min(9999f, Flat(p.Unit.transform.position - p.Point).magnitude * 10f);
			}
		}

		private static void ExportEditorFootprints()
		{
			if (footprintsExported || !Game.GetIsServer() || !ObjectInfo.AllInitialized)
			{
				return;
			}
			footprintsExported = true;
			try
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				List<ObjectInfo> objectDatas = ObjectInfo.ObjectDatas;
				foreach (KeyValuePair<string, string> entry in FormationCatalog.UnitIds)
				{
					ObjectInfo objectInfo = objectDatas.FirstOrDefault((ObjectInfo i) => (bool)i && i.name == entry.Value);
					if ((bool)objectInfo)
					{
						Vector3 size = objectInfo.PhysicalBounds.size;
						if (!(size.x <= 0f) && !(size.z <= 0f) && !float.IsNaN(size.x) && !float.IsNaN(size.z) && !float.IsInfinity(size.x) && !float.IsInfinity(size.z))
						{
							dictionary[entry.Key] = new
							{
								width = size.x,
								length = size.z,
								category = EditorCategories.For(objectInfo)
							};
						}
					}
				}
				if (dictionary.Count == 0)
				{
					DebugWarning("No initialized physical footprints available for editor export.");
					return;
				}
				string text = System.IO.Path.Combine(MelonEnvironment.UserDataDirectory, "Formations_cfg", "metadata");
				Directory.CreateDirectory(text);
				string text2 = System.IO.Path.Combine(text, "footprints.json");
				File.WriteAllText(text2, JsonConvert.SerializeObject(new
				{
					format = "silica-unit-footprints",
					version = 1,
					source = "ObjectInfo.PhysicalBounds (local prefab bounds, metres)",
					units = dictionary
				}, Formatting.Indented));
				DebugMsg("Exported " + dictionary.Count + " physical footprints for formation editor: " + text2);
			}
			catch (Exception ex)
			{
				DebugWarning("Editor footprint export: " + ex.Message);
			}
		}

		private static void LockedSpeedLimit(Unit __instance, ref float __result)
		{
			if (Running && Available(__instance))
			{
				if (LockedSpeedCaps.TryGetValue(__instance, out var value))
				{
					__result = ((__result < 0f) ? value : Math.Min(__result, value));
				}
				if (SoftSpeedCaps.TryGetValue(__instance, out var value2))
				{
					__result = ((__result < 0f) ? value2 : Math.Min(__result, value2));
				}
				if (FollowDepartureCaps.TryGetValue(__instance, out var value3))
				{
					__result = ((__result < 0f) ? value3 : Math.Min(__result, value3));
				}
				if (FpsSpeedCaps.TryGetValue(__instance, out var value4))
				{
					__result = ((__result < 0f) ? value4 : Math.Min(__result, value4));
				}
			}
		}

		private static bool LockEligible(Unit u, Player p)
		{
			if (Available(u) && !u.IsFlyingType && !u.PlayerControlled && u.CanMove && u.Team == p.Team && u.NormalSpeed > 0.01f)
			{
				return u.TopSpeed > 0.01f;
			}
			return false;
		}

		private static void ReleaseLockedUnit(Unit u)
		{
			if ((object)u == null || !LockedUnits.TryGetValue(u, out var value))
			{
				return;
			}
			for (int num = value.Members.Count - 1; num >= 0; num--)
			{
				if (value.Members[num].Unit == u)
				{
					RestoreMember(value.Members[num]);
					value.Members.RemoveAt(num);
				}
			}
			LockedUnits.Remove(u);
			RecalculateBaseline(value);
		}

		private static void RestoreMember(LockedMember m)
		{
			LockedSpeedCaps.Remove(m.Unit);
			if (!m.Unit)
			{
				return;
			}
			if ((bool)m.Unit.OrderAgent)
			{
				foreach (AIOrder order in m.Unit.OrderAgent.Orders)
				{
					if (order.Id == m.Order)
					{
						order.SpeedScale = m.OriginalScale;
					}
				}
			}
			m.Unit.MoveSpeedScale = m.OriginalScale;
		}

		private static void ReleaseCommander(Player p)
		{
			Unit[] array = (from e in LockHeadings
				where e.Value.Owner == p
				select e.Key).ToArray();
			foreach (Unit key in array)
			{
				LockHeadings.Remove(key);
			}
			for (int num2 = Locks.Count - 1; num2 >= 0; num2--)
			{
				if (Locks[num2].Owner == p)
				{
					ReleaseLockAt(num2);
				}
			}
			if ((object)p != null)
			{
				CommanderSettings.Remove(p);
			}
		}

		private static void ReleaseLockAt(int index)
		{
			foreach (LockedMember member in Locks[index].Members)
			{
				RestoreMember(member);
				LockedUnits.Remove(member.Unit);
			}
			Locks.RemoveAt(index);
		}

		private static void ClearLocks()
		{
			for (int num = Locks.Count - 1; num >= 0; num--)
			{
				ReleaseLockAt(num);
			}
			LockedSpeedCaps.Clear();
			LockHeadings.Clear();
			CommanderSettings.Clear();
			Menus.Clear();
			moderatorTolerance = (moderatorFalloff = null);
		}

		private static void CommanderRoundEnded(GameMode mode, Team winner)
		{
			ClearOrders();
			PlayerPolicies.Clear();
		}

		private static void RecalculateBaseline(LockedGroup g)
		{
			g.Baseline = float.MaxValue;
			foreach (LockedMember member in g.Members)
			{
				g.Baseline = Math.Min(g.Baseline, member.Maximum);
			}
			if (g.Members.Count == 0)
			{
				g.Baseline = 0f;
			}
		}

		private static Vector3 Centre(LockedGroup g)
		{
			Vector3 zero = Vector3.zero;
			foreach (LockedMember member in g.Members)
			{
				zero += member.Unit.transform.position;
			}
			return zero / Math.Max(1, g.Members.Count);
		}

		private static Vector3 Slot(LockedGroup g, LockedMember m)
		{
			return Quaternion.Euler(0f, g.Angle, 0f) * m.Offset;
		}

		private static bool HandleLockedOrder(Player p, ref List<BaseGameObject> objects, Vector3 destination, Target target, AgentMoveSpeed speed, bool attack, bool queued)
		{
			if ((bool)target || attack || queued)
			{
				foreach (BaseGameObject @object in objects)
				{
					if (@object is Unit unit && unit.Team == p.Team)
					{
						ReleaseLockedUnit(unit);
					}
				}
				return false;
			}
			CommanderOptions commanderOptions = Options(p);
			if (commanderOptions.Pending == 2)
			{
				commanderOptions.Pending = 0;
				return ScoutSelection(p, ref objects, destination, speed);
			}
			Unit[] array = (from u in objects.OfType<Unit>()
				where (bool)u && u.Team == p.Team
				select u).Distinct().ToArray();
			Unit[] array2 = array;
			for (int num = 0; num < array2.Length; num++)
			{
				ReleaseLockedUnit(array2[num]);
			}
			if (commanderOptions.Pending != 1)
			{
				array2 = array;
				foreach (Unit key in array2)
				{
					LockHeadings.Remove(key);
				}
				return false;
			}
			commanderOptions.Pending = 0;
			Unit[] array3 = array.Where((Unit u) => LockEligible(u, p)).ToArray();
			if (array3.Length < 3)
			{
				MoveReply(p, "Lock denied: select at least three AI-controlled movable ground units. Infantry, vehicles and alien ground units are supported; flying and player-controlled units are excluded.");
				return false;
			}
			LockedGroup lockedGroup = new LockedGroup
			{
				Owner = p,
				Team = p.Team,
				NextTick = Time.unscaledTime
			};
			Vector3 zero = Vector3.zero;
			array2 = array3;
			foreach (Unit unit2 in array2)
			{
				zero += unit2.transform.position;
			}
			zero /= (float)array3.Length;
			if (DistanceSq(destination, zero) > (float)LockTravelMaximum * (float)LockTravelMaximum)
			{
				destination = zero + Heading(destination - zero) * LockTravelMaximum;
			}
			Vector3 vector = PreviousLockHeading(p, array3);
			float num2 = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
			Vector3 after = ((DistanceSq(destination, zero) > 0.01f) ? Heading(destination - zero) : vector);
			bool flag = Reversed(vector, after);
			lockedGroup.Angle = Mathf.Atan2(after.x, after.z) * 57.29578f;
			array2 = array3;
			foreach (Unit unit3 in array2)
			{
				CancelUnit(unit3, keepSoft: true);
				Followers.Remove(unit3);
				Superseded.Remove(unit3);
				float originalScale = ((unit3.AIAgent.CohesionGroup != null) ? 1f : unit3.MoveSpeedScale);
				UnitCohesionGroup.Detach(unit3);
				LockedMember lockedMember = new LockedMember
				{
					Unit = unit3,
					OriginalGroup = unit3.AIGroup,
					OriginalScale = originalScale,
					Maximum = unit3.TopSpeed,
					Radius = Math.Max(1f, unit3.PhysicalRadius),
					Offset = Quaternion.Euler(0f, 0f - num2, 0f) * Flat(unit3.transform.position - zero)
				};
				if (flag)
				{
					lockedMember.Offset.x = 0f - lockedMember.Offset.x;
				}
				lockedGroup.Members.Add(lockedMember);
				LockedUnits.Add(unit3, lockedGroup);
			}
			RecalculateBaseline(lockedGroup);
			Locks.Add(lockedGroup);
			StartLockedMove(lockedGroup, destination, speed);
			HashSet<Unit> handled = new HashSet<Unit>(lockedGroup.Members.Select((LockedMember m) => m.Unit));
			MoveReply(p, "Formation snapshot applied to this move only: " + handled.Count + " units. The next move uses normal formation handling.");
			if (handled.Count == 0)
			{
				return false;
			}
			objects = objects.Where((BaseGameObject obj) => !(obj is Unit item) || !handled.Contains(item)).ToList();
			return objects.Count == 0;
		}

		private static void RefreshSpeedCaps(LockedGroup g)
		{
			foreach (LockedMember member in g.Members)
			{
				if ((bool)member.Unit)
				{
					member.Maximum = member.Unit.TopSpeed;
				}
			}
			RecalculateBaseline(g);
			foreach (LockedMember member2 in g.Members)
			{
				if (g.Traveling && member2.HasDestination && SpeedCapEnabled && CoordinationPolicy.NeedsSpeedCap(member2.Maximum, g.Baseline))
				{
					LockedSpeedCaps[member2.Unit] = g.Baseline;
				}
				else
				{
					LockedSpeedCaps.Remove(member2.Unit);
				}
			}
			ApplyCentreCaps(g);
		}

		private static void StartLockedMove(LockedGroup g, Vector3 destination, AgentMoveSpeed speed)
		{
			g.Destination = destination;
			g.TravelForward = Heading(destination - Centre(g));
			g.Speed = speed;
			g.Traveling = true;
			float num = ((SpeedLockDelay > 0f) ? SpeedLockDelay : LockDelay);
			Dictionary<Unit, int> dictionary = ((SpeedLockDelay > 0f) ? SpeedDepartureRows(g.Members.Select((LockedMember m) => m.Unit)) : DepartureRows(g.Members.Select((LockedMember m) => m.Unit), g.TravelForward));
			foreach (LockedMember member in g.Members)
			{
				member.HasDestination = false;
				member.DepartAt = Time.unscaledTime + (float)dictionary[member.Unit] * num;
				member.Previous = member.Unit.OrderAgent.Orders.Select((AIOrder o) => o.Id).ToArray();
			}
			RefreshSpeedCaps(g);
			IssueLockedDestinations(g);
			int num2 = 0;
			LockedMember[] array = g.Members.ToArray();
			foreach (LockedMember lockedMember in array)
			{
				if (!lockedMember.HasDestination && Time.unscaledTime >= lockedMember.DepartAt)
				{
					ReleaseLockedUnit(lockedMember.Unit);
					num2++;
				}
			}
			RefreshSpeedCaps(g);
			if (num2 > 0)
			{
				MoveReply(g.Owner, num2 + " units released: no valid formation destination; original request remains available to native movement.");
			}
			Player owner = g.Owner;
			string text = $"Locked move: centre-based pacing, baseline {g.Baseline:0.0} m/s; ";
			string text2;
			if (SpeedLockDelay > 0f)
			{
				text2 = "slowest-first departures.";
			}
			else
			{
				text2 = ((LockDelay > 0f) ? "positional departures." : "simultaneous departure.");
			}
			MoveReply(owner, text + text2);
		}

		private static void TickLocks()
		{
			if (!Running)
			{
				if (Locks.Count > 0)
				{
					ClearLocks();
				}
				return;
			}
			float now = Time.unscaledTime;
			for (int num = Locks.Count - 1; num >= 0; num--)
			{
				LockedGroup lockedGroup = Locks[num];
				bool flag = lockedGroup.Traveling && lockedGroup.Members.Any((LockedMember m) => !m.HasDestination && now >= m.DepartAt);
				if (!(now < lockedGroup.NextTick) || flag)
				{
					lockedGroup.NextTick = now + 0.2f;
					if (!lockedGroup.Owner || !Player.Players.Contains(lockedGroup.Owner) || !lockedGroup.Owner.IsCommander || lockedGroup.Owner.Team != lockedGroup.Team)
					{
						ReleaseLockAt(num);
					}
					else
					{
						for (int num2 = lockedGroup.Members.Count - 1; num2 >= 0; num2--)
						{
							LockedMember lockedMember = lockedGroup.Members[num2];
							if (LockEligible(lockedMember.Unit, lockedGroup.Owner) && lockedMember.Unit.AIGroup == lockedMember.OriginalGroup)
							{
								if (!lockedGroup.Traveling || lockedMember.Unit.OrderAgent.Orders.Count <= 0)
								{
									continue;
								}
								bool num3;
								if (!lockedMember.HasDestination)
								{
									num3 = !lockedMember.Previous.SequenceEqual(lockedMember.Unit.OrderAgent.Orders.Select((AIOrder o) => o.Id));
								}
								else
								{
									if (lockedMember.Unit.OrderAgent.Orders.Count != 1)
									{
										goto IL_0204;
									}
									num3 = lockedMember.Unit.OrderAgent.Orders[0].Id != lockedMember.Order;
								}
								if (!num3)
								{
									continue;
								}
							}
							goto IL_0204;
							IL_0204:
							ReleaseLockedUnit(lockedMember.Unit);
						}
						if (lockedGroup.Members.Count == 0)
						{
							Locks.RemoveAt(num);
						}
						else if (lockedGroup.Traveling)
						{
							IssueLockedDestinations(lockedGroup);
							LockedMember[] array = lockedGroup.Members.ToArray();
							foreach (LockedMember lockedMember2 in array)
							{
								if (!lockedMember2.HasDestination && now >= lockedMember2.DepartAt)
								{
									ReleaseLockedUnit(lockedMember2.Unit);
								}
							}
							bool flag2 = true;
							foreach (LockedMember member in lockedGroup.Members)
							{
								float num5 = Mathf.Clamp(member.Radius * 0.5f, 3f, 8f);
								if (!member.HasDestination || DistanceSq(member.Unit.transform.position, member.LastDestination) > num5 * num5)
								{
									flag2 = false;
								}
							}
							if (flag2)
							{
								ReleaseLockAt(num);
							}
							else
							{
								RefreshSpeedCaps(lockedGroup);
							}
						}
					}
				}
			}
		}

		private static void IssueLockedDestinations(LockedGroup g)
		{
			generatedDepth++;
			try
			{
				foreach (LockedMember member in g.Members)
				{
					if (member.HasDestination || Time.unscaledTime < member.DepartAt)
					{
						continue;
					}
					Vector3 desired = g.Destination + Slot(g, member);
					if (TerrainProjection.TryProject(member.Unit, desired, out var point))
					{
						UnitCohesionGroup.Detach(member.Unit);
						OrderTarget target = new OrderTarget(point);
						OrderIssueParams issueParams = OrderIssueParams.Commanded(g.Speed, queueOrder: false, member.OriginalScale);
						if (member.Unit.OrderAgent.IssueOrder(OrderDefinitionRegistry.Move, in target, in issueParams))
						{
							member.Order = ((member.Unit.OrderAgent.Orders.Count > 0) ? member.Unit.OrderAgent.Orders[0].Id : 0u);
							member.LastDestination = point;
							member.HasDestination = true;
							LockHeadings[member.Unit] = new LockHeading
							{
								Owner = g.Owner,
								Team = g.Team,
								Group = member.Unit.AIGroup,
								Forward = g.TravelForward
							};
						}
					}
				}
			}
			finally
			{
				generatedDepth--;
			}
		}

		private static bool ScoutEligible(Unit u, Player p)
		{
			if ((bool)u && !u.IsDestroyed && u.Team == p.Team && u.AIControlled && !u.PlayerControlled && !u.ControlledBy && !u.InCompartment && (!u.Driver || !u.Driver.ControlledBy) && u.CanMove && (bool)u.OrderAgent && (bool)u.AIAgent && (bool)u.AIAgent.AgentPathfinding && (bool)(UnityEngine.Object)(object)u.AIAgent.AgentPathfinding.PathfindingSeeker && u.NormalSpeed > 0.01f && (!u.IsFlyingType || u.AIAgent is AIAirVehicleAgent))
			{
				return u.AIAgent.AgentPathfinding.PathfindingSeeker.traversableTags == -1;
			}
			return false;
		}

		private static bool ScoutSelection(Player p, ref List<BaseGameObject> objects, Vector3 clicked, AgentMoveSpeed speed)
		{
			int num = objects.OfType<Unit>().Distinct().Count((Unit u) => (bool)u && LockedUnits.ContainsKey(u));
			List<Unit> list = (from u in objects.OfType<Unit>().Distinct()
				where ScoutEligible(u, p) && !LockedUnits.ContainsKey(u)
				select u).ToList();
			objects = list.Cast<BaseGameObject>().ToList();
			if (list.Count < 5)
			{
				MoveReply(p, "Scout denied: fewer than 5 eligible units. Locked formation units are excluded.");
				NativeScoutFallback(objects, clicked, speed);
				objects.Clear();
				return true;
			}
			int num2 = Math.Max(0, list.Count - CommanderOrderLimit);
			if (num2 > 0)
			{
				list.RemoveRange(CommanderOrderLimit, num2);
			}
			if (list.Count < 5)
			{
				MoveReply(p, "Scout denied: commander capacity is below 5.");
				NativeScoutFallback(objects, clicked, speed);
				objects.Clear();
				return true;
			}
			Terrain mainTerrain = Game.MainTerrain;
			if (!mainTerrain || !mainTerrain.terrainData || !(UnityEngine.Object)(object)AstarPath.active)
			{
				MoveReply(p, "Scouting started: 0 units deployed; map navigation unavailable; " + list.Count + " failures; " + num + " locked units skipped.");
				return true;
			}
			Bounds bounds = new Bounds(mainTerrain.transform.position + mainTerrain.terrainData.bounds.center, mainTerrain.terrainData.bounds.size);
			int count = list.Count;
			int val = Math.Max(1, Mathf.CeilToInt(Mathf.Sqrt((float)count * bounds.size.x / Math.Max(1f, bounds.size.z))));
			val = Math.Min(count, val);
			int num3 = Mathf.CeilToInt((float)count / (float)val);
			List<Vector3> list2 = new List<Vector3>(count);
			int num4 = 0;
			int num5 = 0;
			generatedDepth++;
			try
			{
				for (int num6 = 0; num6 < count; num6++)
				{
					int num7 = num6 / val;
					int num8 = num6 % val;
					int num9 = Math.Min(val, count - num7 * val);
					Vector3 vector = new Vector3(bounds.min.x + ((float)num8 + 0.5f) * bounds.size.x / (float)num9, 0f, bounds.min.z + ((float)num7 + 0.5f) * bounds.size.z / (float)num3);
					int index = 0;
					float num10 = float.MaxValue;
					for (int num11 = 0; num11 < list.Count; num11++)
					{
						float num12 = DistanceSq(list[num11].transform.position, vector);
						if (num12 < num10)
						{
							num10 = num12;
							index = num11;
						}
					}
					Unit unit = list[index];
					list.RemoveAt(index);
					bool flag = false;
					for (int num13 = 0; num13 < 13; num13++)
					{
						float num14 = ((num13 != 0) ? ((num13 <= 6) ? 150 : 295) : 0);
						float f = (float)((num13 - 1) % 6) * MathF.PI / 3f;
						Vector3 candidate = vector + new Vector3(Mathf.Cos(f) * num14, 0f, Mathf.Sin(f) * num14);
						if (!ScoutDestination(unit, candidate, vector, bounds, out var destination))
						{
							continue;
						}
						float separation = Math.Max(30f, Math.Min(bounds.size.x / (float)val, bounds.size.z / (float)num3) * 0.35f);
						if (list2.Any((Vector3 d) => DistanceSq(d, destination) < separation * separation))
						{
							continue;
						}
						OrderTarget target = new OrderTarget(destination);
						OrderIssueParams issueParams = OrderIssueParams.Commanded(speed);
						if (unit.OrderAgent.CanIssueOrder(OrderDefinitionRegistry.Move, in target))
						{
							CancelUnit(unit);
							Followers.Remove(unit);
							Superseded.Remove(unit);
							UnitCohesionGroup.Detach(unit);
							if (unit.OrderAgent.IssueOrder(OrderDefinitionRegistry.Move, in target, in issueParams))
							{
								num4++;
								list2.Add(destination);
								flag = true;
							}
							break;
						}
					}
					if (!flag)
					{
						num5++;
					}
				}
			}
			finally
			{
				generatedDepth--;
			}
			MoveReply(p, $"Scouting started: {num4} units deployed; {num} locked units skipped; {num5} destination/order failures; {num2} over capacity left untouched.");
			return true;
		}

		private static void NativeScoutFallback(List<BaseGameObject> objects, Vector3 clicked, AgentMoveSpeed speed)
		{
			foreach (BaseGameObject @object in objects)
			{
				if (@object is Unit unit)
				{
					CancelUnit(unit);
					Followers.Remove(unit);
					Superseded.Remove(unit);
				}
			}
			generatedDepth++;
			try
			{
				StrategyMode.PerformMoveAttack(objects, clicked, null, speed, isAttack: false);
			}
			finally
			{
				generatedDepth--;
			}
		}

		private static bool ScoutDestination(Unit u, Vector3 candidate, Vector3 nominal, Bounds bounds, out Vector3 point)
		{
			point = Vector3.zero;
			if (candidate.x < bounds.min.x || candidate.x > bounds.max.x || candidate.z < bounds.min.z || candidate.z > bounds.max.z)
			{
				return false;
			}
			if (!TerrainProjection.TryProject(u, candidate, out point) || DistanceSq(point, nominal) > 90000f)
			{
				return false;
			}
			NearestNodeConstraint constraint = new NearestNodeConstraint
			{
				graphMask = u.AIAgent.AgentPathfinding.PathfindingSeeker.graphMask,
				area = -1,
				tags = -1,
				maxDistance = 75f
			};
			Vector3 position = (u.IsFlyingType ? GamePhysics.GetTerrainPosition(u.transform.position, 0f, Game.MainTerrain) : u.transform.position);
			GraphNode node = AstarPath.active.GetNearest(position, constraint).node;
			GraphNode node2 = AstarPath.active.GetNearest(point, constraint).node;
			if (node != null && node2 != null && PathUtilities.IsPathPossible(node, node2) && point.x >= bounds.min.x && point.x <= bounds.max.x && point.z >= bounds.min.z)
			{
				return point.z <= bounds.max.z;
			}
			return false;
		}

		private static void TravelDistanceCommand(Player p, string args)
		{
			if (!ApiReady || !IsAdministrator(p))
			{
				Reply(p, "admin Generic power required.");
				return;
			}
			if (!CommandArgument.TryParse(args, out var name, out var quoted) | quoted)
			{
				Reply(p, "usage: /Formationtraveldistance <0..10000> | status");
				return;
			}
			if (name.Length == 0 || name.Equals("status", StringComparison.OrdinalIgnoreCase))
			{
				Reply(p, "Locked formation maximum " + LockTravelMaximum + " metres.");
				return;
			}
			if (!int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var result) || result < 0 || result > 10000)
			{
				Reply(p, "usage: /Formationtraveldistance <0..10000> | status");
				return;
			}
			travelDefault.Value = result;
			try
			{
				prefs.SaveToFile(false);
				FormatPreferences();
				Reply(p, "Locked formation maximum " + result + " metres (saved; new moves only).");
			}
			catch (Exception ex)
			{
				DebugError("Saving FormationTravelDistance: " + ex);
				Reply(p, "Locked formation maximum " + result + " metres; saving failed, see server log.");
			}
		}

		private static void SpeedCapCommand(Player p, string args)
		{
			if (!ApiReady || !IsAdministrator(p))
			{
				Reply(p, "admin Generic power required.");
				return;
			}
			if ((!CommandArgument.TryParse(args, out var name, out var quoted) | quoted) || (!name.Equals("on", StringComparison.OrdinalIgnoreCase) && !name.Equals("off", StringComparison.OrdinalIgnoreCase)))
			{
				Reply(p, "usage: /Formationspeedcap on|off");
				return;
			}
			speedCapOverride = name.Equals("on", StringComparison.OrdinalIgnoreCase);
			foreach (LockedGroup @lock in Locks)
			{
				RefreshSpeedCaps(@lock);
			}
			Reply(p, "Formation speed cap " + (SpeedCapEnabled ? "ON" : "OFF") + " for final arrival handling this match. Centre-based travel pacing remains active.");
		}

		private void RegisterCommanderOptions()
		{
			speedCapDefault = prefs.CreateEntry<bool>("FormationSpeedCap", true, (string)null, "Optional faster-unit ceiling near final arrival; centre-based travel pacing is always active. Admin /Formationspeedcap on|off overrides for this match", false, false, (ValueValidator)null, (string)null);
			RegisterCommanderCommand("Formationspeedcap", SpeedCapCommand);
			RegisterDepartureSettings();
			travelDefault = prefs.CreateEntry<int>("FormationTravelDistance", 1000, (string)null, "Locked formation maximum travel distance in metres, 0..10000; farther clicks stop the snapshot centre at this distance", false, false, (ValueValidator)null, (string)null);
			toleranceDefault = prefs.CreateEntry<int>("FormationPositionTolerance", 25, (string)null, "Slot tolerance as percent of snapshot nearest-neighbour spacing, 10..100; vehicle-size minimum also applies", false, false, (ValueValidator)null, (string)null);
			falloffDefault = prefs.CreateEntry<int>("FormationFalloffSlowdown", 25, (string)null, "Percent outside slot tolerance to trigger slowdown, 10..90; recovery threshold is 60% of this value (25 -> 15)", false, false, (ValueValidator)null, (string)null);
			RegisterCommanderCommand("scout", (Player p, string a) =>
			{
				ArmCommander(p, 2);
			});
			RegisterCommanderCommand("Formationtraveldistance", TravelDistanceCommand);
			RegisterCommanderCommand("formationTravel", TravelDistanceCommand);
			RegisterCommanderCommand("formationTolerance", (Player p, string a) =>
			{
				SetCommanderOption(p, a, 1);
			});
			RegisterCommanderCommand("FormationFallof", (Player p, string a) =>
			{
				SetCommanderOption(p, a, 2);
			});
		}

		private void RegisterCommanderCommand(string name, Action<Player, string> callback)
		{
			if (AdminMethods.FindAdminCommandFromString(name) != null || PlayerMethods.FindPlayerCommandFromString(name) != null)
			{
				throw new InvalidOperationException("Command already registered: " + name);
			}
			PlayerMethods.RegisterPlayerCommand(name, (Player? p, string a) =>
			{
				callback(p, a);
			}, hideFromChat: true);
			registered.Add(name);
		}

		private static float ValidOption(int value, int min, int max, int fallback)
		{
			return (value >= min && value <= max) ? value : fallback;
		}

		private static CommanderOptions Options(Player p)
		{
			if (!CommanderSettings.TryGetValue(p, out var value) || value.Team != p.Team)
			{
				Dictionary<Player, CommanderOptions> commanderSettings = CommanderSettings;
				CommanderOptions commanderOptions = new CommanderOptions
				{
					Team = p.Team,
					Tolerance = ValidOption(moderatorTolerance ?? toleranceDefault.Value, 10, 100, 25),
					Falloff = ValidOption(moderatorFalloff ?? falloffDefault.Value, 10, 90, 25)
				};
				value = commanderOptions;
				commanderSettings[p] = commanderOptions;
			}
			return value;
		}

		private static bool CommanderAuthorized(Player p)
		{
			if (!Running || !p || !p.IsCommander || !p.Team || ((bool)networkSender && networkSender != p))
			{
				Reply(p, "requires an active commander and enabled server formations.");
				return false;
			}
			For(p);
			return true;
		}

		private static void LockCommand(Player p, string action)
		{
			if (action.Equals("lock", StringComparison.OrdinalIgnoreCase))
			{
				ArmCommander(p, 1);
				return;
			}
			if (!p || !Game.GetIsServer() || !p.IsCommander)
			{
				Reply(p, "commander only.");
				return;
			}
			CommanderOptions commanderOptions = Options(p);
			ReleaseCommander(p);
			commanderOptions.Pending = 0;
			CommanderSettings[p] = commanderOptions;
			Reply(p, "All your locked formations released; pending action cancelled; movement settings restored.");
		}

		private static void ArmCommander(Player p, int action)
		{
			if (CommanderAuthorized(p))
			{
				Options(p).Pending = action;
				if (action == 1)
				{
					Reply(p, "formation lock armed for one move; maximum " + LockTravelMaximum + " metres");
				}
				else
				{
					Reply(p, "Scouting armed. Select units and issue a move order; the clicked destination will be ignored.");
				}
			}
		}

		private static void SetCommanderOption(Player p, string text, int index)
		{
			if (!ApiReady || !p || (!p.IsCommander && !IsAdministrator(p)))
			{
				Reply(p, "commander or moderator Generic power required.");
				return;
			}
			if ((!CommandArgument.TryParse(text, out var name, out var quoted) | quoted) || !int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var result) || result < 10 || result > ((index == 1) ? 100 : 90))
			{
				Reply(p, (index == 1) ? "position tolerance range: 10..100%." : "fall-off slowdown range: 10..90%.");
				return;
			}
			CommanderOptions commanderOptions = Options(p);
			if (index == 1)
			{
				commanderOptions.Tolerance = result;
			}
			else
			{
				commanderOptions.Falloff = result;
			}
			if (!p.IsCommander)
			{
				if (index == 1)
				{
					moderatorTolerance = result;
				}
				else
				{
					moderatorFalloff = result;
				}
			}
			Reply(p, ((index == 1) ? "Position tolerance: " : "Fall-off group slowdown: ") + result + "%." + (p.IsCommander ? " Personal setting." : " Default for new commander sessions.") + ((index == 2) ? (" Recovery below " + ((float)result * 0.6f).ToString("0.#", CultureInfo.InvariantCulture) + "%.") : ""));
		}

		private static void ProbeMenus()
		{
			menusProbed = true;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly obj in assemblies)
			{
				Type type = obj.GetType("Si_KingOfTheHill.KingOfTheHill");
				if (type != null)
				{
					ProbeMenu(type, "_buyStates", "King of the Hill /buy (close with /buy close)", hasClosedLevel: true);
				}
				Type type2 = obj.GetType("Si_UnitBalance.UnitBalance");
				if (type2 != null)
				{
					ProbeMenu(type2, "_menuStates", "Unit Balance /b (close with /b)", hasClosedLevel: false);
					ProbeMenu(type2, "_statsStates", "Unit Balance stats (close with /exit)", hasClosedLevel: false);
				}
			}
		}

		private static void ProbeMenu(Type type, string field, string name, bool hasClosedLevel)
		{
			ForeignMenu foreignMenu = new ForeignMenu
			{
				Name = name,
				States = type.GetField(field, BindingFlags.Static | BindingFlags.NonPublic)
			};
			if (foreignMenu.States == null || !typeof(IDictionary).IsAssignableFrom(foreignMenu.States.FieldType))
			{
				foreignMenu.ProbeFailed = true;
			}
			else if (hasClosedLevel)
			{
				Type type2 = foreignMenu.States.FieldType.GetGenericArguments()[1];
				foreignMenu.Level = type2.GetField("Level");
				foreignMenu.ProbeFailed = foreignMenu.Level == null || !foreignMenu.Level.FieldType.IsEnum || Enum.GetName(foreignMenu.Level.FieldType, 0) != "Closed";
			}
			ForeignMenus.Add(foreignMenu);
		}

		private static bool MenuAvailable(Player p, bool explain)
		{
			if (!menusProbed)
			{
				ProbeMenus();
			}
			foreach (ForeignMenu foreignMenu in ForeignMenus)
			{
				bool flag = false;
				bool flag2 = foreignMenu.ProbeFailed;
				try
				{
					if (!flag2)
					{
						if (!(foreignMenu.States.GetValue(null) is IDictionary dictionary))
						{
							flag2 = true;
						}
						else
						{
							object obj = dictionary[(long)p.PlayerID.m_ID];
							flag = obj != null && (foreignMenu.Level == null || Convert.ToInt32(foreignMenu.Level.GetValue(obj)) != 0);
						}
					}
				}
				catch
				{
					flag2 = true;
				}
				if (flag || flag2)
				{
					if (explain)
					{
						Reply(p, flag2 ? ("Formation menu unavailable: cannot safely detect " + foreignMenu.Name + " menu ownership.") : ("Close " + foreignMenu.Name + " first. No shared menu cancellation API is available."));
					}
					return false;
				}
			}
			return true;
		}
	}
	public static class FormationApi
	{
		public static bool IsReady => Formations.ApiReady;

		public static bool GlobalEnabled => Formations.ApiEnabled;

		public static bool GlobalRequestedEnabled => Formations.GlobalRequested;

		public static int CommanderFormationOrderLimit => Formations.CommanderOrderLimit;

		public static int MatchGroupLimit => Formations.MatchLimit;

		public static IReadOnlyDictionary<string, object> GetPlayerPanelPage(ulong steamId, string function, int page)
		{
			return Formations.ApiPanelPage(steamId, function, page);
		}

		public static string RequestPlayerPanelAction(ulong steamId, string function, int option, string action, string revision)
		{
			return Formations.ApiPanelAction(steamId, function, option, action, revision);
		}

		public static ChangeResult TryGetPlayerActivation(ulong steamId, out bool personalEnabled, out bool effectiveEnabled)
		{
			return Formations.ApiActivation(steamId, out personalEnabled, out effectiveEnabled);
		}

		public static ChangeResult TryGetFunctionEnabled(ulong steamId, string function, out bool enabled)
		{
			return Formations.ApiFunctionEnabled(steamId, function, out enabled);
		}

		public static ChangeResult TryGetPlayerLimit(ulong steamId, out int limit)
		{
			return Formations.ApiPlayerLimit(steamId, out limit);
		}

		public static ChangeResult RequestGlobalEnabled(bool enabled)
		{
			return Formations.ApiGlobal(enabled);
		}

		public static ChangeResult RequestMatchGroupLimit(int limit)
		{
			return Formations.ApiMatchLimit(limit);
		}

		public static ChangeResult RequestPlayerGroupLimit(ulong steamId, int limit)
		{
			return Formations.ApiSetPlayerLimit(steamId, limit, clear: false);
		}

		public static ChangeResult ClearPlayerGroupLimit(ulong steamId)
		{
			return Formations.ApiSetPlayerLimit(steamId, 0, clear: true);
		}

		public static ChangeResult TryGetCommanderEnabled(ulong steamId, out bool enabled)
		{
			return Formations.ApiCommanderEnabled(steamId, out enabled);
		}

		public static ChangeResult RequestPlayerPolicy(ulong steamId, string owner, int limit, string[] allowedCategories, string[] allowedObjectNames, string[] deniedObjectNames)
		{
			return Formations.ApiPolicy(steamId, owner, limit, allowedCategories, allowedObjectNames, deniedObjectNames);
		}

		public static ChangeResult TryGetPlayerPolicy(ulong steamId, out string owner, out int limit, out string[] allowedCategories, out string[] allowedObjectNames, out string[] deniedObjectNames)
		{
			return Formations.ApiReadPolicy(steamId, out owner, out limit, out allowedCategories, out allowedObjectNames, out deniedObjectNames);
		}

		public static ChangeResult ClearPlayerPolicy(ulong steamId, string owner)
		{
			return Formations.ApiClearPolicy(steamId, owner);
		}

		public static ChangeResult ClearOwnerPolicies(string owner)
		{
			return Formations.ApiClearOwner(owner);
		}

		public static ChangeResult TryGetUnitDefinitions(out string[] objectNames, out string[] unitTypes, out string[] unitWeights)
		{
			return Formations.ApiDefinitions(out objectNames, out unitTypes, out unitWeights);
		}

		public static string[] GetNativeUnitCategories()
		{
			return Enum.GetNames(typeof(UnitType)).SelectMany((string t) => from w in Enum.GetNames(typeof(UnitWeight))
				select t + ":" + w).ToArray();
		}
	}
	public enum ChangeResult
	{
		Success,
		NotReady,
		WrongThread,
		InvalidPlayer,
		InvalidLimit,
		AdminOverride,
		InvalidFunction,
		InvalidPolicy,
		OwnerConflict,
		NoPolicy
	}
	internal static class TerrainProjection
	{
		internal static bool TryProject(Unit unit, Vector3 desired, out Vector3 point)
		{
			point = Vector3.zero;
			if (!unit)
			{
				return false;
			}
			Terrain mainTerrain = Game.MainTerrain;
			if (!mainTerrain || !mainTerrain.terrainData)
			{
				return false;
			}
			Bounds bounds = new Bounds(mainTerrain.transform.position + mainTerrain.terrainData.bounds.center, mainTerrain.terrainData.bounds.size);
			if (desired.x < bounds.min.x || desired.x > bounds.max.x || desired.z < bounds.min.z || desired.z > bounds.max.z)
			{
				return false;
			}
			Vector3 terrainPosition = GamePhysics.GetTerrainPosition(desired, 0f, mainTerrain);
			if (float.IsNaN(terrainPosition.y) || float.IsInfinity(terrainPosition.y))
			{
				return false;
			}
			return GameAI.GetPointValidForAI(terrainPosition, unit.AIAgent.AgentPathfinding.PathfindingSeeker.graphMask, out point, 75f, 5f, 30f);
		}
	}
	internal sealed class FormationSlot
	{
		internal readonly float X;

		internal readonly float Z;

		internal readonly float Size;

		internal readonly string Role;

		internal readonly int? Preference;

		internal readonly bool ForceExclusive;

		internal readonly HashSet<string> Preferred;

		internal readonly HashSet<string> PreferredTypes;

		internal FormationSlot(float x, float z, string role, float size, IEnumerable<string> preferred, int? preference = null, IEnumerable<string> preferredTypes = null, bool forceExclusive = false)
		{
			ForceExclusive = forceExclusive;
			X = x;
			Z = z;
			Role = role;
			Size = size;
			Preference = preference;
			Preferred = new HashSet<string>(preferred, StringComparer.OrdinalIgnoreCase);
			PreferredTypes = new HashSet<string>(preferredTypes ?? Array.Empty<string>(), StringComparer.Ordinal);
		}
	}
	internal sealed class FormationDefinition
	{
		internal readonly string Name;

		internal readonly string Team;

		internal readonly string Function;

		internal readonly bool DirectionSensitive;

		internal readonly float Scale;

		internal readonly FormationSlot[] Slots;

		internal int MenuOrder;

		internal string Source;

		private readonly Dictionary<string, int[]> ranks = new Dictionary<string, int[]>(StringComparer.Ordinal);

		private readonly Dictionary<string, Tuple<int, int>> specificity = new Dictionary<string, Tuple<int, int>>(StringComparer.Ordinal);

		internal Tuple<int, int> Specificity(string id, string type)
		{
			string key = id + "|" + type;
			if (!specificity.TryGetValue(key, out var value))
			{
				int num = 4;
				int num2 = 0;
				FormationSlot[] slots = Slots;
				for (int i = 0; i < slots.Length; i++)
				{
					int num3 = Match(slots[i], id, type);
					if (num3 < num)
					{
						num = num3;
						num2 = 1;
					}
					else if (num3 == num)
					{
						num2++;
					}
				}
				value = (specificity[key] = Tuple.Create(num, num2));
			}
			return value;
		}

		internal FormationDefinition(string name, string team, string function, bool sensitive, float scale, FormationSlot[] slots, int menuOrder = 0)
		{
			Name = name;
			Team = team;
			Function = function;
			DirectionSensitive = sensitive;
			Scale = scale;
			Slots = slots;
			MenuOrder = menuOrder;
		}

		internal int[] Ranked(string unitId, string unitType)
		{
			string key = unitId + "|" + unitType;
			if (!ranks.TryGetValue(key, out var value))
			{
				value = (from i in Enumerable.Range(0, Slots.Length)
					where Allows(Slots[i], unitId, unitType)
					orderby Rank(Slots[i], unitId, unitType), i
					select i).ToArray();
				ranks.Add(key, value);
			}
			return value;
		}

		internal static bool Allows(FormationSlot s, string id, string unitType)
		{
			if (s.ForceExclusive && !s.Preferred.Contains(id))
			{
				return s.PreferredTypes.Contains(unitType);
			}
			return true;
		}

		internal static int Match(FormationSlot s, string id, string unitType)
		{
			if (!s.Preferred.Contains(id))
			{
				if (!s.PreferredTypes.Contains(unitType))
				{
					if (s.Preferred.Count != 0 || s.PreferredTypes.Count != 0)
					{
						return 3;
					}
					return 2;
				}
				return 1;
			}
			return 0;
		}

		internal static int Colour(FormationSlot s)
		{
			return Math.Min(3, s.Preference ?? ((!(s.Role == "top")) ? ((s.Role == "backup") ? 1 : 2) : 0));
		}

		private static int Rank(FormationSlot s, string id, string unitType)
		{
			return Colour(s) * 4 + Match(s, id, unitType);
		}
	}
	internal sealed class FormationCatalog
	{
		internal readonly Dictionary<string, FormationDefinition> Definitions = new Dictionary<string, FormationDefinition>(StringComparer.OrdinalIgnoreCase);

		private readonly HashSet<string> duplicates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		internal static readonly Dictionary<string, string> UnitIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ "Sol|Scout", "ObjectInfo_Sol_Soldier_Scout" },
			{ "Sol|Rifleman", "ObjectInfo_Sol_Soldier_Rifleman" },
			{ "Sol|Heavy", "ObjectInfo_Sol_Soldier_Heavy" },
			{ "Sol|Sniper", "ObjectInfo_Sol_Soldier_Sniper" },
			{ "Sol|Commando", "ObjectInfo_Sol_Soldier_Commando" },
			{ "Sol|Light Quad", "ObjectInfo_Sol_Light_LightQuad" },
			{ "Sol|Heavy Quad", "ObjectInfo_Sol_Light_HeavyQuad" },
			{ "Sol|Light Striker", "ObjectInfo_Sol_Light_LightStriker" },
			{ "Sol|Heavy Striker", "ObjectInfo_Sol_Light_HeavyStriker" },
			{ "Sol|AA Truck", "ObjectInfo_Sol_Light_AATruck" },
			{ "Sol|Repair Rig", "ObjectInfo_Sol_Light_RepairRig" },
			{ "Sol|Platoon Hauler", "ObjectInfo_Sol_Light_PlatoonHauler" },
			{ "Sol|Hover Tank", "ObjectInfo_Sol_Heavy_HoverTank" },
			{ "Sol|Barrage Truck", "ObjectInfo_Sol_Heavy_BarrageTruck" },
			{ "Sol|Railgun Tank", "ObjectInfo_Sol_Heavy_RailgunTank" },
			{ "Sol|Pulse Truck", "ObjectInfo_Sol_Heavy_PulseTruck" },
			{ "Sol|Siege Tank", "ObjectInfo_Sol_UltraHeavy_SiegeTank" },
			{ "Sol|Gunship", "ObjectInfo_Sol_Air_Gunship" },
			{ "Sol|Dropship", "ObjectInfo_Sol_Air_Dropship" },
			{ "Sol|Fighter", "ObjectInfo_Sol_Air_Fighter" },
			{ "Sol|Bomber", "ObjectInfo_Sol_Air_Bomber" },
			{ "Centauri|Militia", "ObjectInfo_Cent_Soldier_Militia" },
			{ "Centauri|Trooper", "ObjectInfo_Cent_Soldier_Trooper" },
			{ "Centauri|Marksman", "ObjectInfo_Cent_Soldier_Marksman" },
			{ "Centauri|Juggernaut", "ObjectInfo_Cent_Soldier_Juggernaut" },
			{ "Centauri|Templar", "ObjectInfo_Cent_Soldier_Templar" },
			{ "Centauri|Light Raider", "ObjectInfo_Cent_Light_LightRaider" },
			{ "Centauri|Heavy Raider", "ObjectInfo_Cent_Light_HeavyRaider" },
			{ "Centauri|Assault Car", "ObjectInfo_Cent_Light_AssaultCar" },
			{ "Centauri|Repair Truck", "ObjectInfo_Cent_Light_RepairTruck" },
			{ "Centauri|Strike Tank", "ObjectInfo_Cent_Light_StrikeTank" },
			{ "Centauri|Squad Transport", "ObjectInfo_Cent_Light_SquadTransport" },
			{ "Centauri|Combat Tank", "ObjectInfo_Cent_Heavy_CombatTank" },
			{ "Centauri|Heavy Tank", "ObjectInfo_Cent_Heavy_HeavyTank" },
			{ "Centauri|Pyro Tank", "ObjectInfo_Cent_Heavy_PyroTank" },
			{ "Centauri|Crimson Tank", "ObjectInfo_Cent_UltraHeavy_CrimsonTank" },
			{ "Centauri|Dreadnought", "ObjectInfo_Cent_Air_Dreadnought" },
			{ "Centauri|Interceptor", "ObjectInfo_Cent_Air_Interceptor" },
			{ "Centauri|Shuttle", "ObjectInfo_Cent_Air_Shuttle" },
			{ "Centauri|Freighter", "ObjectInfo_Cent_Air_Freighter" },
			{ "Alien|Crab", "ObjectInfo_Alien_Crab" },
			{ "Alien|Horned Crab", "ObjectInfo_Alien_CrabHorned" },
			{ "Alien|Shocker", "ObjectInfo_Alien_Shocker" },
			{ "Alien|Wasp", "ObjectInfo_Alien_Wasp" },
			{ "Alien|Dragonfly", "ObjectInfo_Alien_Dragonfly" },
			{ "Alien|Squid", "ObjectInfo_Alien_Squid" },
			{ "Alien|Hunter", "ObjectInfo_Alien_Hunter" },
			{ "Alien|Behemoth", "ObjectInfo_Alien_Behemoth" },
			{ "Alien|Scorpion", "ObjectInfo_Alien_Scorpion" },
			{ "Alien|Firebug", "ObjectInfo_Alien_Firebug" },
			{ "Alien|Goliath", "ObjectInfo_Alien_Goliath" },
			{ "Alien|Defiler", "ObjectInfo_Alien_Defiler" },
			{ "Alien|Colossus", "ObjectInfo_Alien_Colossus" },
			{ "Centauri|Flak Truck", "ObjectInfo_Cent_Light_FlakCar" },
			{ "Centauri|Rocket Truck", "ObjectInfo_Cent_Heavy_RocketTank" }
		};

		internal static string Key(string team, string function, string name)
		{
			return team + "|" + function + "|" + name.Trim();
		}

		internal bool TryGet(string team, string function, string name, out FormationDefinition definition)
		{
			return Definitions.TryGetValue(Key(team, function, name), out definition);
		}

		internal void Load(string directory, Action<string> report)
		{
			Definitions.Clear();
			duplicates.Clear();
			if (!Directory.Exists(directory))
			{
				report("No JSON directory: " + directory);
				return;
			}
			string[] array = Directory.GetFiles(directory, "*.json").OrderBy((string p) => p, StringComparer.OrdinalIgnoreCase).ThenBy((string p) => p, StringComparer.Ordinal)
				.ToArray();
			if (array.Length > 128)
			{
				report("Too many JSON files (maximum 128); no formations loaded.");
				return;
			}
			List<FormationDefinition> list = new List<FormationDefinition>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				try
				{
					if (new FileInfo(text).Length > 2097152)
					{
						throw new FormatException("file exceeds 2 MiB");
					}
					FormationDefinition[] array3 = Parse(File.ReadAllText(text));
					foreach (FormationDefinition formationDefinition in array3)
					{
						formationDefinition.Source = text;
						list.Add(formationDefinition);
						string text2 = Key(formationDefinition.Team, formationDefinition.Function, formationDefinition.Name);
						if (duplicates.Contains(text2) || Definitions.ContainsKey(text2))
						{
							Definitions.Remove(text2);
							duplicates.Add(text2);
							report(text + ": duplicate " + text2 + "; ALL copies disabled");
						}
						else
						{
							Definitions.Add(text2, formationDefinition);
						}
					}
				}
				catch (Exception ex)
				{
					report(text + ": rejected: " + ex.Message);
				}
			}
			Dictionary<string, HashSet<int>> dictionary = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
			foreach (IGrouping<string, FormationDefinition> item in from f in list
				group f by f.Team + "|" + f.Function)
			{
				dictionary[item.Key] = new HashSet<int>(from f in item
					where f.MenuOrder != 0
					select f.MenuOrder);
			}
			foreach (IGrouping<string, FormationDefinition> item2 in from f in list
				where f.MenuOrder != 0
				group f by f.Team + "|" + f.Function + "|" + f.MenuOrder)
			{
				if (item2.Count() <= 1)
				{
					continue;
				}
				report("Duplicate menu number " + item2.Key + ": " + string.Join("; ", item2.Select((FormationDefinition f) => f.Source + " [" + f.Name + "]")) + "; ALL conflicting definitions disabled; number remains reserved.");
				foreach (FormationDefinition item3 in item2)
				{
					Definitions.Remove(Key(item3.Team, item3.Function, item3.Name));
				}
			}
			foreach (FormationDefinition item4 in list.Where((FormationDefinition f) => f.MenuOrder == 0))
			{
				string text3 = Key(item4.Team, item4.Function, item4.Name);
				if (Definitions.TryGetValue(text3, out var value) && value == item4)
				{
					HashSet<int> hashSet = dictionary[item4.Team + "|" + item4.Function];
					int num3;
					for (num3 = 2; num3 <= 9999 && hashSet.Contains(num3); num3++)
					{
					}
					if (num3 > 9999)
					{
						Definitions.Remove(text3);
						report(item4.Source + ": no free menu number for " + text3);
					}
					else
					{
						item4.MenuOrder = num3;
						hashSet.Add(num3);
					}
				}
			}
			report("Cached " + Definitions.Count + " definitions from " + array.Length + " JSON files (startup only).");
		}

		internal static FormationDefinition[] Parse(string json)
		{
			using JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json))
			{
				MaxDepth = 32,
				DateParseHandling = DateParseHandling.None
			};
			JObject jObject = JObject.Load(jsonTextReader, new JsonLoadSettings
			{
				DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error
			});
			if (jsonTextReader.Read())
			{
				throw new FormatException("trailing JSON data");
			}
			if (Text(jObject, "format") != "silica-formations" || Number(jObject, "version") != 3f)
			{
				throw new FormatException("requires silica-formations version 3; re-export older files in editor");
			}
			if (jObject.TryGetValue("menu", out JToken value))
			{
				if (value is JObject jObject2)
				{
					JToken? jToken = jObject2["defaultOption"];
					if (jToken != null && jToken.Type == JTokenType.Integer && !(jObject2["defaultOption"].ToString() != "1"))
					{
						JToken? jToken2 = jObject2["defaultLabel"];
						if (jToken2 != null && jToken2.Type == JTokenType.String && !((string?)jObject2["defaultLabel"] != "default"))
						{
							goto IL_011d;
						}
					}
				}
				throw new FormatException("menu must reserve defaultOption 1 with defaultLabel default");
			}
			goto IL_011d;
			IL_011d:
			if (!(jObject["formations"] is JArray { Count: not 0, Count: <=128 } jArray))
			{
				throw new FormatException("formations must contain 1..128 entries");
			}
			List<FormationDefinition> list = new List<FormationDefinition>();
			foreach (JToken item in jArray)
			{
				if (!(item is JObject jObject3))
				{
					throw new FormatException("formation must be an object");
				}
				string text = Text(jObject3, "name");
				string text2 = Text(jObject3, "team");
				string text3 = Text(jObject3, "function");
				if (text != text.Trim() || text.Length > 128 || text.Contains("|") || text.Any(char.IsControl))
				{
					throw new FormatException("invalid formation name");
				}
				if (!new string[3] { "Sol", "Centauri", "Alien" }.Contains(text2) || !new string[4] { "follow", "move", "commander", "attack" }.Contains(text3))
				{
					throw new FormatException(text + ": invalid team/function");
				}
				int result = 0;
				if (jObject3.TryGetValue("menuOrder", out JToken value2) && (value2.Type != JTokenType.Integer || !int.TryParse(value2.ToString(), out result) || result < 2 || result > 9999))
				{
					throw new FormatException(text + ": menuOrder must be an integer 2..9999; /1 is virtual Default");
				}
				float num = Number(jObject3, "metresPerNode");
				if (num <= 0f || num > 1000f)
				{
					throw new FormatException(text + ": scale must be >0 and <=1000");
				}
				JToken? jToken3 = jObject3["directionSensitive"];
				if (jToken3 == null || jToken3.Type != JTokenType.Boolean)
				{
					throw new FormatException(text + ": missing directionSensitive");
				}
				bool flag = (bool)jObject3["directionSensitive"];
				Number(jObject3, "directionDegrees");
				if (Text(jObject3, "coordinateSpace") != (flag ? "local-right-forward" : "world-xz"))
				{
					throw new FormatException(text + ": coordinateSpace mismatch");
				}
				if (!(jObject3["origin"] is JObject o) || Number(o, "x") != 0f || Number(o, "z") != 0f)
				{
					throw new FormatException(text + ": origin must be zero");
				}
				if (!(jObject3["slots"] is JArray { Count: not 0, Count: <=4096 } jArray2))
				{
					throw new FormatException(text + ": slots must contain 1..4096 candidates");
				}
				List<FormationSlot> list2 = new List<FormationSlot>();
				foreach (JToken item2 in jArray2)
				{
					if (!(item2 is JObject jObject4))
					{
						throw new FormatException("slot must be object");
					}
					float num2 = Number(jObject4, "x");
					float num3 = Number(jObject4, "z");
					float num4 = Number(jObject4, "sizeScale");
					if (Math.Abs(num2) > 10000f || Math.Abs(num3) > 10000f)
					{
						throw new FormatException(text + ": offset exceeds 10000 metres");
					}
					string text4 = Text(jObject4, "role");
					if (!new string[5] { "top", "repair", "purple", "backup", "last" }.Contains(text4))
					{
						throw new FormatException(text + ": unknown priority " + text4);
					}
					if (num4 != 0.5f && num4 != 1f && num4 != 2f)
					{
						throw new FormatException(text + ": invalid sizeScale");
					}
					JArray obj = (jObject4["preferredUnits"] as JArray) ?? throw new FormatException(text + ": missing preferredUnits");
					List<string> list3 = new List<string>();
					foreach (JToken item3 in obj)
					{
						if (item3.Type != JTokenType.String || !UnitIds.TryGetValue(text2 + "|" + (string?)item3, out var value3))
						{
							throw new FormatException(text + ": unknown preferred unit " + item3?.ToString() + " for " + text2);
						}
						list3.Add(value3);
					}
					int? preference = null;
					List<string> list4 = new List<string>();
					if (jObject4.TryGetValue("preferredTypes", out JToken value4))
					{
						foreach (JToken item4 in (value4 as JArray) ?? throw new FormatException(text + ": preferredTypes must be an array"))
						{
							if (item4.Type != JTokenType.String || !EditorCategories.Names.Contains((string?)item4))
							{
								throw new FormatException(text + ": unknown preferred type " + item4);
							}
							if ((string?)item4 != "Harvester")
							{
								list4.Add((string?)item4);
							}
						}
					}
					if (text4 == "repair" || text4 == "purple")
					{
						list4.Add("Repair");
						text4 = ((text4 == "repair") ? "top" : "backup");
					}
					if (jObject4.TryGetValue("preference", out JToken value5))
					{
						if (value5.Type != JTokenType.Integer || !int.TryParse(value5.ToString(), out var result2) || result2 < 0 || result2 > 4)
						{
							throw new FormatException(text + ": preference must be an integer from 0 to 4");
						}
						preference = result2;
					}
					bool forceExclusive = false;
					if (jObject4.TryGetValue("forceExclusive", out JToken value6))
					{
						if (value6.Type != JTokenType.Boolean)
						{
							throw new FormatException(text + ": forceExclusive must be boolean");
						}
						forceExclusive = (bool)value6;
					}
					list2.Add(new FormationSlot(num2, num3, text4, num4, list3, preference, list4, forceExclusive));
				}
				list.Add(new FormationDefinition(text, text2, text3, flag, num, list2.ToArray(), result));
			}
			return list.ToArray();
		}

		private static string Text(JObject o, string key)
		{
			JToken? jToken = o[key];
			if (jToken == null || jToken.Type != JTokenType.String || string.IsNullOrWhiteSpace((string?)o[key]))
			{
				throw new FormatException("missing/invalid " + key);
			}
			return (string?)o[key];
		}

		private static float Number(JObject o, string key)
		{
			JToken? jToken = o[key];
			if (jToken == null || jToken.Type != JTokenType.Integer)
			{
				JToken? jToken2 = o[key];
				if (jToken2 == null || jToken2.Type != JTokenType.Float)
				{
					throw new FormatException("missing/invalid " + key);
				}
			}
			float num = (float)o[key];
			if (float.IsNaN(num) || float.IsInfinity(num))
			{
				throw new FormatException("nonfinite " + key);
			}
			return num;
		}
	}
	internal static class CommandArgument
	{
		internal static bool TryParse(string args, out string name, out bool quoted)
		{
			name = "";
			quoted = false;
			string text = (args ?? "").Trim();
			int num = text.IndexOfAny(new char[2] { ' ', '\t' });
			if (num < 0)
			{
				return true;
			}
			text = text.Substring(num).Trim();
			if (text.StartsWith("\""))
			{
				quoted = true;
				try
				{
					name = JsonConvert.DeserializeObject<string>(text);
					return !string.IsNullOrWhiteSpace(name);
				}
				catch
				{
					return false;
				}
			}
			if (text.Contains("\""))
			{
				return false;
			}
			name = text;
			return true;
		}
	}
	internal static class EditorCategories
	{
		internal static readonly HashSet<string> Names;

		private static readonly Dictionary<string, string> Overrides;

		static EditorCategories()
		{
			Names = new HashSet<string>(new string[12]
			{
				"Infantry", "Cavalry", "Tank", "Siege", "Aircraft", "Transport", "Special", "Repair", "Anti-air", "Scout",
				"Artillery", "Harvester"
			}, StringComparer.Ordinal);
			Overrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			Add("Anti-air", "AA Truck", "Flak Truck");
			Add("Scout", "Light Quad", "Heavy Quad", "Light Raider", "Heavy Raider");
			Add("Cavalry", "Light Striker", "Heavy Striker", "Strike Tank", "Assault Car", "Scorpion");
			Add("Tank", "Hover Tank", "Railgun Tank", "Combat Tank", "Heavy Tank", "Behemoth");
			Add("Siege", "Siege Tank", "Crimson Tank", "Goliath");
			Add("Artillery", "Barrage Truck", "Rocket Truck");
			Add("Special", "Pulse Truck", "Pyro Tank");
			Add("Transport", "Platoon Hauler", "Squad Transport");
			Add("Repair", "Repair Rig", "Repair Truck");
		}

		private static void Add(string category, params string[] labels)
		{
			foreach (KeyValuePair<string, string> unitId in FormationCatalog.UnitIds)
			{
				if (labels.Contains(unitId.Key.Substring(unitId.Key.IndexOf('|') + 1)))
				{
					Overrides[unitId.Value] = category;
				}
			}
		}

		internal static string For(ObjectInfo info)
		{
			if (!info)
			{
				return "None";
			}
			if (!Overrides.TryGetValue(info.name, out var value))
			{
				return info.UnitType.ToString();
			}
			return value;
		}
	}
	internal static class CoordinationPolicy
	{
		internal static bool NeedsSpeedCap(float maximum, float slowest)
		{
			if (slowest > 0f)
			{
				return maximum > slowest + 0.001f;
			}
			return false;
		}

		internal static float Tolerance(float radius, float spacing, float percent)
		{
			return Math.Max(Math.Max(4f, radius * 1.5f), spacing * percent / 100f);
		}

		internal static bool Coordinated(float distance, float limit)
		{
			return distance <= limit;
		}

		internal static bool Arrived(float issuedTargetToFinalSquared, float distanceToIssuedSquared, float arrivalRadius)
		{
			if (issuedTargetToFinalSquared <= 25f)
			{
				return distanceToIssuedSquared <= arrivalRadius * arrivalRadius;
			}
			return false;
		}

		internal static float AccelerationPace(float previous, float requested, float slowestObserved, float elapsed)
		{
			return Math.Min(Math.Min(requested, Math.Max(0f, slowestObserved) + 0.75f), Math.Max(0f, previous) + Math.Max(0f, elapsed) * 1.5f);
		}

		internal static float MemberPace(float groupPace, float maximum, bool recoveringStraggler, float boost, float longitudinal, float tolerance)
		{
			if (recoveringStraggler)
			{
				return maximum * Math.Min(1.1f, Math.Max(1f, boost));
			}
			float num = Math.Min(maximum, groupPace);
			if (longitudinal < (0f - tolerance) * 0.25f)
			{
				num *= Math.Max(0f, 1f + (longitudinal + tolerance * 0.25f) / Math.Max(1f, tolerance));
			}
			return num;
		}

		internal static float RecoveryThreshold(float trigger)
		{
			return trigger * 3f / 5f;
		}

		internal static bool UpdateRecovery(float outside, float trigger, float now, ref bool recovering, ref float started)
		{
			if (!recovering && outside > trigger)
			{
				recovering = true;
				started = now;
			}
			bool flag = recovering && now - started >= 10f;
			if (recovering && ((outside < RecoveryThreshold(trigger)) | flag))
			{
				recovering = false;
			}
			return flag;
		}

		internal static float NextPace(float pace, bool recovering, float elapsed)
		{
			float num = (recovering ? 0.5f : 1f);
			float num2 = Math.Max(0f, elapsed) * 0.25f;
			if (!(pace < num))
			{
				return Math.Max(num, pace - num2);
			}
			return Math.Min(num, pace + num2);
		}
	}
}
