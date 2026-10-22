using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using HarmonyLib;
using MelonLoader;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Si_MapBalance;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(MapBalance), "Map Balance", "3.2.0", "schwe", null)]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_MapBalance")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+7e2359a5a99aa435fe44a0dea0f7ad6505215569")]
[assembly: AssemblyProduct("Si_MapBalance")]
[assembly: AssemblyTitle("Si_MapBalance")]
[assembly: AssemblyVersion("1.0.0.0")]
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
}
namespace Si_MapBalance
{
	public class MapConfig
	{
		[JsonProperty("map")]
		public string Map = "";

		[JsonProperty("description")]
		public string Description = "";

		[JsonProperty("resources")]
		public ResourceConfig Resources = new ResourceConfig();

		[JsonProperty("spawns")]
		public JToken? RawSpawns;

		[JsonProperty("positions")]
		public List<PositionEntry>? Positions;

		[JsonProperty("settings")]
		public SettingsConfig Settings = new SettingsConfig();

		[JsonProperty("game_modes")]
		public JToken? GameModes;

		[JsonProperty("koh")]
		public KohEntry? Koh;

		[JsonProperty("fusion_reactors")]
		public List<FusionReactorEntry>? FusionReactors;

		public SpawnPos? GetFactionSpawn(string faction, Instance log)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Invalid comparison between Unknown and I4
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected Obj, but got Unknown
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Invalid comparison between Unknown and I4
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected Obj, but got Unknown
			if (RawSpawns == null || (int)RawSpawns.Type != 1)
			{
				return null;
			}
			JObject val = (JObject)RawSpawns;
			JToken val2 = val[faction];
			if (val2 != null && (int)val2.Type == 2)
			{
				JArray val3 = (JArray)val2;
				if (((JContainer)val3).Count == 0)
				{
					return null;
				}
				JToken val4 = val3[0];
				SpawnPos spawnPos = new SpawnPos();
				JToken val5 = val4[(object)"x"];
				spawnPos.X = ((val5 != null) ? Extensions.Value<float>((IEnumerable<JToken>)val5) : 0f);
				JToken val6 = val4[(object)"z"];
				spawnPos.Z = ((val6 != null) ? Extensions.Value<float>((IEnumerable<JToken>)val6) : 0f);
				return spawnPos;
			}
			List<JProperty> list = val.Properties().Where((JProperty p) =>
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Invalid comparison between Unknown and I4
				return (int)p.Value.Type == 1 && p.Value[(object)faction] != null;
			}).ToList();
			if (list.Count == 0)
			{
				return null;
			}
			JProperty val7 = list[Random.Range(0, list.Count)];
			log.Msg("Selected spawn variant: " + val7.Name);
			JToken val8 = val7.Value[(object)faction];
			if (val8 == null)
			{
				return null;
			}
			SpawnPos spawnPos2 = new SpawnPos();
			JToken val9 = val8[(object)"x"];
			spawnPos2.X = ((val9 != null) ? Extensions.Value<float>((IEnumerable<JToken>)val9) : 0f);
			JToken val10 = val8[(object)"z"];
			spawnPos2.Z = ((val10 != null) ? Extensions.Value<float>((IEnumerable<JToken>)val10) : 0f);
			return spawnPos2;
		}

		public bool HasSpawns()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected Obj, but got Unknown
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Invalid comparison between Unknown and I4
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			if (RawSpawns == null || (int)RawSpawns.Type != 1)
			{
				return false;
			}
			JObject val = (JObject)RawSpawns;
			string[] array = new string[3] { "Sol", "Cent", "Alien" };
			foreach (string text in array)
			{
				JToken val2 = val[text];
				if (val2 != null && (int)val2.Type == 2 && ((JContainer)(JArray)val2).Count > 0)
				{
					return true;
				}
			}
			return val.Properties().Any((JProperty p) =>
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Invalid comparison between Unknown and I4
				return (int)p.Value.Type == 1;
			});
		}
	}
	public class ResourceConfig
	{
		[JsonProperty("balterium_amount")]
		public int BalteriumAmount;

		[JsonProperty("biotics_amount")]
		public int BioticsAmount;

		[JsonProperty("removed_patches")]
		public List<int> RemovedPatches = new List<int>();

		[JsonProperty("patch_overrides")]
		public Dictionary<string, int> PatchOverrides = new Dictionary<string, int>();
	}
	public class SpawnPos
	{
		[JsonProperty("x")]
		public float X;

		[JsonProperty("z")]
		public float Z;
	}
	public class PositionEntry
	{
		[JsonProperty("x")]
		public float X;

		[JsonProperty("z")]
		public float Z;

		[JsonProperty("type")]
		public string Type = "balterium";
	}
	public class KohEntry
	{
		[JsonProperty("x")]
		public float X;

		[JsonProperty("z")]
		public float Z;

		[JsonProperty("capture_radius")]
		public float CaptureRadius = 50f;

		[JsonProperty("exclusion_radius")]
		public float ExclusionRadius = 75f;

		[JsonProperty("prefab")]
		public string PrefabName = "Sol_UltraHeavyFactory";
	}
	public class FusionReactorEntry
	{
		[JsonProperty("x")]
		public float X;

		[JsonProperty("z")]
		public float Z;

		[JsonProperty("capture_radius")]
		public float CaptureRadius = 40f;

		[JsonProperty("prefab")]
		public string PrefabName = "FusionReactor_01";
	}
	public class SettingsConfig
	{
		[JsonProperty("buildRadius")]
		public float BuildRadius = 550f;

		[JsonProperty("chainRange")]
		public float ChainRange = 1300f;

		[JsonProperty("solBuildRadius")]
		public float SolBuildRadius;

		[JsonProperty("centBuildRadius")]
		public float CentBuildRadius;

		[JsonProperty("solChainRange")]
		public float SolChainRange;

		[JsonProperty("centChainRange")]
		public float CentChainRange;

		[JsonProperty("alienNodeChainRange")]
		public float AlienNodeChainRange;

		[JsonProperty("alienBiocacheChainRange")]
		public float AlienBiocacheChainRange;

		[JsonProperty("wildlifeBuildRadius")]
		public float WildlifeBuildRadius;

		[JsonProperty("wildlifeNodeChainRange")]
		public float WildlifeNodeChainRange;

		[JsonProperty("wildlifeBiocacheChainRange")]
		public float WildlifeBiocacheChainRange;

		public float GetBuildRadius(string faction)
		{
			if (faction.Contains("Sol") && SolBuildRadius > 0f)
			{
				return SolBuildRadius;
			}
			if (faction.Contains("Cent") && CentBuildRadius > 0f)
			{
				return CentBuildRadius;
			}
			if (faction.Contains("Wildlife") && WildlifeBuildRadius > 0f)
			{
				return WildlifeBuildRadius;
			}
			return BuildRadius;
		}

		public float GetChainRange(string faction)
		{
			if (faction.Contains("Sol") && SolChainRange > 0f)
			{
				return SolChainRange;
			}
			if (faction.Contains("Cent") && CentChainRange > 0f)
			{
				return CentChainRange;
			}
			if (faction.Contains("Wildlife") && WildlifeNodeChainRange > 0f)
			{
				return WildlifeNodeChainRange;
			}
			return ChainRange;
		}
	}
	public class MapBalance : MelonMod
	{
		[HarmonyPatch(typeof(MusicJukeboxHandler), "OnGameStarted")]
		private static class Patch_OnGameStarted
		{
			private static void Postfix()
			{
				try
				{
					_dumpedThisRound = false;
					if (_activeConfig != null)
					{
						string text = (_swappedSolCent ? " (swapped)" : "");
						string text2 = "Map Balance: " + _activeConfigName + text;
						_sendServerChatMethod?.Invoke(null, new object[1] { text2 });
						Melon<MapBalance>.Logger.Msg("Allchat: " + text2);
						if (_printLogLineMethod == null)
						{
							ResolvePrintLogLine();
						}
						string text3 = "World triggered \"map_layout\" (layout \"" + _activeConfigName + text + "\")";
						_printLogLineMethod?.Invoke(null, new object[2] { text3, true });
						try
						{
							SpawnSpecials(_activeConfig, Melon<MapBalance>.Logger);
							return;
						}
						catch (Exception arg)
						{
							Melon<MapBalance>.Logger.Error($"SpawnSpecials failed: {arg}");
							return;
						}
					}
				}
				catch (Exception arg2)
				{
					Melon<MapBalance>.Logger.Error($"OnGameStarted failed: {arg2}");
				}
			}
		}

		[HarmonyPatch(typeof(MusicJukeboxHandler), "OnGameEnded")]
		private static class Patch_OnGameEnded
		{
			private static void Postfix()
			{
				_dumpedThisRound = false;
				_distributedThisRound = false;
				_activeConfig = null;
				_activeConfigName = "";
				_swappedSolCent = false;
				CleanupSpawnedAreas();
				MapBalanceSpecials.Clear();
			}
		}

		private class RefineryCheckData
		{
			public string Name = "";

			public float GridSnapXZ = 10f;

			public float GridSnapY = 5f;

			public bool RotateToSurface;

			public float MaxAngle = 30f;

			public List<Vector3> AboveGroundLocal = new List<Vector3>();

			public List<Vector3> BelowGroundLocal = new List<Vector3>();

			public List<EntryCheckData> PreviewEntries = new List<EntryCheckData>();

			public List<RampCheckData> Ramps = new List<RampCheckData>();
		}

		private class EntryCheckData
		{
			public List<Vector3> AboveLocal = new List<Vector3>();

			public List<Vector3> BelowLocal = new List<Vector3>();

			public int MustAboveCount;

			public int MustBelowCount;
		}

		private class RampCheckData
		{
			public Vector3 CheckPointLocal;

			public Vector3 EntryLocal;

			public bool IsAboveType;

			public bool HasDepositPoint;
		}

		private static bool _dumpedThisRound;

		private static List<GameObject> _spawnedAreas = new List<GameObject>();

		private static MapConfig? _activeConfig;

		private static string _activeConfigName = "";

		private static bool _swappedSolCent;

		public static float[] WildlifeSpawnPos = null;

		private static MethodInfo? _sendServerChatMethod;

		private static MethodInfo? _printLogLineMethod;

		private static Type? _adminCallbackType;

		private static Type? _adminPowerType;

		private static MethodInfo? _registerAdminCmdMethod;

		private static MethodInfo? _sendChatToPlayerMethod;

		private static Type? _resourceAreaType;

		private static Type? _gameType;

		private static Type? _gameDatabaseType;

		private static Type? _resourceType;

		private static Type? _networkComponentType;

		private static Type? _playerType;

		private static Type? _constructionDataType;

		private static Type? _constructionPreviewType;

		private static Type? _enableAboveGroundType;

		private static Type? _constructionBoundsType;

		private static Type? _mpStrategyType;

		private static Type? _strategyTeamSetupType;

		private static Type? _teamType;

		private static bool _typesResolved;

		private static MethodInfo? _distributeResourcesMethod;

		private static MethodInfo? _sendNetInitMethod;

		private static MethodInfo? _spawnPrefabMethod;

		private static MethodInfo? _getPrefabIndexMethod;

		private static MethodInfo? _getPrefabMethod;

		private static FieldInfo? _allAreasField;

		private static PropertyInfo? _allAreasProp;

		private static FieldInfo? _enabledAreasField;

		private static PropertyInfo? _enabledAreasProp;

		private const float HQ_SPAWN_HEIGHT_OFFSET = 5f;

		private static bool _distributedThisRound;

		private const float FALLBACK_RESOURCE_MULT = 0.8f;

		private static readonly float[][] DesignedPositions = new float[64][]
		{
			new float[2] { -2153f, -2153f },
			new float[2] { -2400f, -2046f },
			new float[2] { -2046f, -2400f },
			new float[2] { -1200f, -2050f },
			new float[2] { -1450f, -2150f },
			new float[2] { -950f, -2150f },
			new float[2] { 0f, -2050f },
			new float[2] { -250f, -2150f },
			new float[2] { 250f, -2150f },
			new float[2] { 1200f, -2050f },
			new float[2] { 950f, -2150f },
			new float[2] { 1450f, -2150f },
			new float[2] { 2153f, -2153f },
			new float[2] { 2046f, -2400f },
			new float[2] { 2400f, -2046f },
			new float[2] { -2153f, 2153f },
			new float[2] { -2046f, 2400f },
			new float[2] { -2400f, 2046f },
			new float[2] { -1200f, 2050f },
			new float[2] { -950f, 2150f },
			new float[2] { -1450f, 2150f },
			new float[2] { 0f, 2050f },
			new float[2] { 250f, 2150f },
			new float[2] { -250f, 2150f },
			new float[2] { 1200f, 2050f },
			new float[2] { 1450f, 2150f },
			new float[2] { 950f, 2150f },
			new float[2] { 2153f, 2153f },
			new float[2] { 2400f, 2046f },
			new float[2] { 2046f, 2400f },
			new float[2] { -2050f, -1200f },
			new float[2] { -2150f, -950f },
			new float[2] { -2150f, -1450f },
			new float[2] { -2050f, 0f },
			new float[2] { -2150f, 250f },
			new float[2] { -2150f, -250f },
			new float[2] { -2050f, 1200f },
			new float[2] { -2150f, 1450f },
			new float[2] { -2150f, 950f },
			new float[2] { 2050f, -1200f },
			new float[2] { 2150f, -1450f },
			new float[2] { 2150f, -950f },
			new float[2] { 2050f, 0f },
			new float[2] { 2150f, -250f },
			new float[2] { 2150f, 250f },
			new float[2] { 2050f, 1200f },
			new float[2] { 2150f, 950f },
			new float[2] { 2150f, 1450f },
			new float[2] { 400f, 0f },
			new float[2] { 0f, 400f },
			new float[2] { -400f, 0f },
			new float[2] { 0f, -400f },
			new float[2] { 600f, 600f },
			new float[2] { -600f, 600f },
			new float[2] { -600f, -600f },
			new float[2] { 600f, -600f },
			new float[2] { 1100f, 0f },
			new float[2] { 0f, 1100f },
			new float[2] { -1100f, 0f },
			new float[2] { 0f, -1100f },
			new float[2] { 1000f, 1000f },
			new float[2] { -1000f, 1000f },
			new float[2] { -1000f, -1000f },
			new float[2] { 1000f, -1000f }
		};

		private static readonly string SpawnsDir = Path.Combine("UserData", "Spawns");

		private static readonly string LegacyConfigDir = Path.Combine("UserData", "MapBalance", "configs");

		private static bool _teamsLogged;

		public override void OnInitializeMelon()
		{
			((MelonBase)this).LoggerInstance.Msg("Si_MapBalance v3.2.0 loaded");
		}

		public override void OnLateInitializeMelon()
		{
			ResolveTypes();
			CacheReflection();
			PatchDistributeAllResources();
			PatchSpawnBaseStructures();
			RegisterAdminCommands();
		}

		private static void ResolveTypes()
		{
			if (_typesResolved)
			{
				return;
			}
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				if (!(assembly.GetName().Name != "SilicaCore"))
				{
					_resourceAreaType = assembly.GetType("ResourceArea");
					_gameType = assembly.GetType("Game");
					_gameDatabaseType = assembly.GetType("GameDatabase");
					_resourceType = assembly.GetType("Resource");
					_networkComponentType = assembly.GetType("NetworkComponent");
					_playerType = assembly.GetType("Player");
					_constructionDataType = assembly.GetType("ConstructionData");
					_constructionPreviewType = assembly.GetType("ConstructionPreview");
					_enableAboveGroundType = assembly.GetType("EnableAboveGround");
					_constructionBoundsType = assembly.GetType("ConstructionBounds");
					_mpStrategyType = assembly.GetType("MP_Strategy");
					_strategyTeamSetupType = assembly.GetType("StrategyTeamSetup");
					_teamType = assembly.GetType("Team");
					break;
				}
			}
			_typesResolved = true;
		}

		private static void CacheReflection()
		{
			if (_resourceAreaType == null)
			{
				return;
			}
			Instance logger = Melon<MapBalance>.Logger;
			_distributeResourcesMethod = _resourceAreaType.GetMethod("DistributeResources", BindingFlags.Instance | BindingFlags.Public);
			logger.Msg($"DistributeResources: {_distributeResourcesMethod}");
			_allAreasField = _resourceAreaType.GetField("AllResourceAreas", BindingFlags.Static | BindingFlags.Public);
			if (_allAreasField == null)
			{
				_allAreasProp = _resourceAreaType.GetProperty("AllResourceAreas", BindingFlags.Static | BindingFlags.Public);
			}
			_enabledAreasField = _resourceAreaType.GetField("ResourceAreas", BindingFlags.Static | BindingFlags.Public);
			if (_enabledAreasField == null)
			{
				_enabledAreasProp = _resourceAreaType.GetProperty("ResourceAreas", BindingFlags.Static | BindingFlags.Public);
			}
			if (_networkComponentType != null)
			{
				_sendNetInitMethod = _networkComponentType.GetMethod("SendNetInit", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { _playerType }, null);
				if (_sendNetInitMethod == null)
				{
					_sendNetInitMethod = _networkComponentType.GetMethod("SendNetInit", BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
				}
				if (_sendNetInitMethod == null)
				{
					MethodInfo[] array = (from m in _networkComponentType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
						where m.Name == "SendNetInit"
						select m).ToArray();
					logger.Msg($"SendNetInit overloads found: {array.Length}");
					MethodInfo[] array2 = array;
					foreach (MethodInfo methodInfo in array2)
					{
						ParameterInfo[] parameters = methodInfo.GetParameters();
						logger.Msg("  " + methodInfo.Name + "(" + string.Join(", ", parameters.Select((ParameterInfo p) => p.ParameterType.Name + " " + p.Name)) + ")");
					}
					if (array.Length != 0)
					{
						_sendNetInitMethod = array[0];
					}
				}
				logger.Msg($"SendNetInit: {_sendNetInitMethod}");
			}
			try
			{
				if (_gameDatabaseType != null)
				{
					MethodInfo[] array3 = (from m in _gameDatabaseType.GetMethods(BindingFlags.Static | BindingFlags.Public)
						where m.Name == "GetSpawnablePrefabIndex"
						select m).ToArray();
					logger.Msg($"GetSpawnablePrefabIndex overloads: {array3.Length}");
					MethodInfo[] array2 = array3;
					for (int num = 0; num < array2.Length; num++)
					{
						ParameterInfo[] parameters2 = array2[num].GetParameters();
						logger.Msg("  (" + string.Join(", ", parameters2.Select((ParameterInfo p) => p.ParameterType.Name + " " + p.Name)) + ")");
					}
					_getPrefabIndexMethod = array3.FirstOrDefault((MethodInfo m) => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string)) ?? array3.FirstOrDefault();
					MethodInfo[] array4 = (from m in _gameDatabaseType.GetMethods(BindingFlags.Static | BindingFlags.Public)
						where m.Name == "GetSpawnablePrefab"
						select m).ToArray();
					logger.Msg($"GetSpawnablePrefab overloads: {array4.Length}");
					array2 = array4;
					for (int num = 0; num < array2.Length; num++)
					{
						ParameterInfo[] parameters3 = array2[num].GetParameters();
						logger.Msg("  (" + string.Join(", ", parameters3.Select((ParameterInfo p) => p.ParameterType.Name + " " + p.Name)) + ")");
					}
					_getPrefabMethod = array4.FirstOrDefault((MethodInfo m) => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(int)) ?? array4.FirstOrDefault();
					logger.Msg($"Using GetSpawnablePrefabIndex: {_getPrefabIndexMethod}");
					logger.Msg($"Using GetSpawnablePrefab: {_getPrefabMethod}");
				}
				if (_gameType != null)
				{
					MethodInfo[] array5 = (from m in _gameType.GetMethods(BindingFlags.Static | BindingFlags.Public)
						where m.Name == "SpawnPrefab"
						select m).ToArray();
					logger.Msg($"SpawnPrefab overloads: {array5.Length}");
					MethodInfo[] array2 = array5;
					for (int num = 0; num < array2.Length; num++)
					{
						ParameterInfo[] parameters4 = array2[num].GetParameters();
						logger.Msg("  SpawnPrefab(" + string.Join(", ", parameters4.Select((ParameterInfo p) => p.ParameterType.Name + " " + p.Name)) + ")");
					}
					_spawnPrefabMethod = array5.OrderByDescending((MethodInfo m) => m.GetParameters().Length).FirstOrDefault();
					logger.Msg($"Using SpawnPrefab: {_spawnPrefabMethod} ({_spawnPrefabMethod?.GetParameters().Length ?? 0} params)");
				}
			}
			catch (Exception ex)
			{
				logger.Error("Failed to resolve spawn methods: " + ex.Message);
			}
			if (_playerType != null)
			{
				_sendServerChatMethod = _playerType.GetMethod("SendServerChatMessage", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(string) }, null);
				logger.Msg($"SendServerChatMessage: {_sendServerChatMethod}");
			}
			try
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (int num = 0; num < assemblies.Length; num++)
				{
					Type type = assemblies[num].GetType("Si_Logging.HL_Logging");
					if (type != null)
					{
						_printLogLineMethod = type.GetMethod("PrintLogLine", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
						{
							typeof(string),
							typeof(bool)
						}, null);
						logger.Msg($"PrintLogLine: {_printLogLineMethod}");
						break;
					}
				}
			}
			catch (Exception ex2)
			{
				logger.Warning("Could not resolve PrintLogLine: " + ex2.Message);
			}
		}

		private static void ResolvePrintLogLine()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Type type = assemblies[i].GetType("Si_Logging.HL_Logging");
				if (type != null)
				{
					_printLogLineMethod = type.GetMethod("PrintLogLine", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
					{
						typeof(string),
						typeof(bool)
					}, null);
					Melon<MapBalance>.Logger.Msg($"PrintLogLine (lazy resolve): {_printLogLineMethod}");
					break;
				}
			}
		}

		private void PatchDistributeAllResources()
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Expected Obj, but got Unknown
			if (_resourceAreaType == null)
			{
				((MelonBase)this).LoggerInstance.Error("Cannot patch — ResourceArea type not found");
				return;
			}
			MethodInfo method = _resourceAreaType.GetMethod("DistributeAllResources", BindingFlags.Static | BindingFlags.Public);
			if (method == null)
			{
				((MelonBase)this).LoggerInstance.Error("DistributeAllResources not found");
				return;
			}
			MethodInfo method2 = typeof(MapBalance).GetMethod("Prefix_DistributeAllResources", BindingFlags.Static | BindingFlags.NonPublic);
			((MelonBase)this).HarmonyInstance.Patch((MethodBase)method, new HarmonyMethod(method2), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			((MelonBase)this).LoggerInstance.Msg("Patched DistributeAllResources (prefix, skips original)");
		}

		private void PatchSpawnBaseStructures()
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Expected Obj, but got Unknown
			if (_mpStrategyType == null)
			{
				((MelonBase)this).LoggerInstance.Warning("MP_Strategy type not found — cannot patch spawn positions");
				return;
			}
			MethodInfo method = _mpStrategyType.GetMethod("SpawnBaseStructures", BindingFlags.Instance | BindingFlags.NonPublic);
			if (method == null)
			{
				((MelonBase)this).LoggerInstance.Warning("SpawnBaseStructures not found — cannot patch spawn positions");
				return;
			}
			MethodInfo method2 = typeof(MapBalance).GetMethod("Prefix_SpawnBaseStructures", BindingFlags.Static | BindingFlags.NonPublic);
			((MelonBase)this).HarmonyInstance.Patch((MethodBase)method, new HarmonyMethod(method2), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			((MelonBase)this).LoggerInstance.Msg("Patched SpawnBaseStructures (prefix, overrides HQ positions)");
		}

		private void RegisterAdminCommands()
		{
			try
			{
				Type type = null;
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (int i = 0; i < assemblies.Length; i++)
				{
					type = assemblies[i].GetType("SilicaAdminMod.HelperMethods");
					if (type != null)
					{
						break;
					}
				}
				if (type == null)
				{
					((MelonBase)this).LoggerInstance.Warning("AdminMod not found — !mapdump command disabled");
					return;
				}
				_adminCallbackType = type.GetNestedType("CommandCallback", BindingFlags.Public);
				if (_adminCallbackType == null)
				{
					_adminCallbackType = type.Assembly.GetType("SilicaAdminMod.HelperMethods+CommandCallback");
				}
				_adminPowerType = type.Assembly.GetType("SilicaAdminMod.Power");
				_registerAdminCmdMethod = type.GetMethod("RegisterAdminCommand", BindingFlags.Static | BindingFlags.Public);
				_sendChatToPlayerMethod = type.GetMethod("SendChatMessageToPlayer", BindingFlags.Static | BindingFlags.Public);
				if (_adminCallbackType == null || _registerAdminCmdMethod == null)
				{
					((MelonBase)this).LoggerInstance.Warning("Could not find CommandCallback or RegisterAdminCommand in AdminMod");
					return;
				}
				MethodInfo method = typeof(MapBalance).GetMethod("OnMapdumpCommand", BindingFlags.Static | BindingFlags.NonPublic);
				DynamicMethod dynamicMethod = new DynamicMethod("MapdumpShim", typeof(void), new Type[2]
				{
					typeof(Player),
					typeof(string)
				}, typeof(MapBalance).Module, skipVisibility: true);
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldarg_1);
				iLGenerator.Emit(OpCodes.Call, method);
				iLGenerator.Emit(OpCodes.Ret);
				object obj = Enum.ToObject(_adminPowerType, 16384);
				_registerAdminCmdMethod.Invoke(null, new object[4]
				{
					"mapdump",
					dynamicMethod.CreateDelegate(_adminCallbackType),
					obj,
					"Dump map info, refinery scans, and HQ scans to UserData/MapBalance/"
				});
				((MelonBase)this).LoggerInstance.Msg("Registered !mapdump admin command via AdminMod");
			}
			catch (Exception ex)
			{
				((MelonBase)this).LoggerInstance.Warning("Failed to register admin commands: " + ex.Message);
			}
		}

		private static void OnMapdumpCommand(object player, string args)
		{
			Instance logger = Melon<MapBalance>.Logger;
			string text = GetCurrentMapName() ?? "unknown";
			try
			{
				string text2 = "[MapBalance] Starting full dump for " + text + "...";
				_sendServerChatMethod?.Invoke(null, new object[1] { text2 });
				logger.Msg(text2);
				_dumpedThisRound = false;
				DumpMapInfo();
				logger.Msg("DumpMapInfo complete");
				DumpConstructionData();
				logger.Msg("DumpConstructionData complete");
				ScanRefineryPlacements();
				logger.Msg("ScanRefineryPlacements complete");
				ScanHQPlacements();
				logger.Msg("ScanHQPlacements complete");
				string text3 = "[MapBalance] Dump complete for " + text + " — files in UserData/MapBalance/";
				_sendServerChatMethod?.Invoke(null, new object[1] { text3 });
				logger.Msg(text3);
			}
			catch (Exception ex)
			{
				string text4 = "[MapBalance] Dump failed: " + ex.Message;
				_sendServerChatMethod?.Invoke(null, new object[1] { text4 });
				logger.Error($"!mapdump failed: {ex}");
			}
		}

		private static bool Prefix_SpawnBaseStructures(object __instance)
		{
			//IL_045c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0461: Unknown result type (might be due to invalid IL or missing references)
			//IL_0482: Unknown result type (might be due to invalid IL or missing references)
			//IL_0487: Unknown result type (might be due to invalid IL or missing references)
			//IL_048b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0490: Unknown result type (might be due to invalid IL or missing references)
			//IL_0499: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_06fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0705: Unknown result type (might be due to invalid IL or missing references)
			//IL_068e: Unknown result type (might be due to invalid IL or missing references)
			//IL_069b: Unknown result type (might be due to invalid IL or missing references)
			Instance logger = Melon<MapBalance>.Logger;
			bool flag = false;
			try
			{
				FieldInfo field = _mpStrategyType.GetField("TeamSetups", BindingFlags.Instance | BindingFlags.Public);
				if (field == null)
				{
					logger.Error("TeamSetups field not found");
					return true;
				}
				object value = field.GetValue(__instance);
				if (value == null)
				{
					logger.Error("TeamSetups is null");
					return true;
				}
				MethodInfo methodInfo = FindMethod(_mpStrategyType, "GetTeamSetupActive");
				int num = (int)(value.GetType().GetProperty("Count")?.GetValue(value) ?? ((object)0));
				PropertyInfo property = value.GetType().GetProperty("Item");
				int num2 = 0;
				bool flag2 = false;
				for (int i = 0; i < num; i++)
				{
					object obj = property?.GetValue(value, new object[1] { i });
					if (obj != null && methodInfo != null && (bool)(methodInfo.Invoke(__instance, new object[1] { obj }) ?? ((object)false)))
					{
						num2++;
						object teamFromSetup = GetTeamFromSetup(obj);
						if (teamFromSetup != null && GetTeamName(teamFromSetup).Contains("Alien"))
						{
							flag2 = true;
						}
					}
				}
				bool flag3 = false;
				try
				{
					Type type = AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly a) =>
					{
						try
						{
							return a.GetTypes();
						}
						catch
						{
							return Array.Empty<Type>();
						}
					}).FirstOrDefault((Type t) => t.FullName == "Si_4way.Si_4way");
					if (type != null)
					{
						FieldInfo field2 = type.GetField("Is4WayEnabled", BindingFlags.Static | BindingFlags.Public);
						if (field2 != null)
						{
							flag3 = (bool)(field2.GetValue(null) ?? ((object)false));
						}
					}
				}
				catch
				{
				}
				string text;
				if (flag3)
				{
					text = "4way";
				}
				else if (num2 >= 3)
				{
					text = "hvhva";
				}
				else
				{
					text = ((!((num2 == 2) & flag2)) ? "hvh" : "hva");
				}
				logger.Msg($"Detected game mode: {text} ({num2} active teams, alien={flag2}, 4way={flag3})");
				MapConfig mapConfig = LoadMapConfig(text);
				if (mapConfig == null)
				{
					logger.Msg("SpawnBaseStructures: No config — letting game handle spawn positions");
					return true;
				}
				if (!mapConfig.HasSpawns())
				{
					logger.Msg("SpawnBaseStructures: No spawn positions in config — letting game handle");
					_activeConfig = mapConfig;
					return true;
				}
				_activeConfig = mapConfig;
				WildlifeSpawnPos = null;
				if (flag3)
				{
					SpawnPos factionSpawn = mapConfig.GetFactionSpawn("Wildlife", logger);
					if (factionSpawn != null)
					{
						WildlifeSpawnPos = new float[2] { factionSpawn.X, factionSpawn.Z };
						logger.Msg($"  Wildlife spawn from config: ({factionSpawn.X:F0}, {factionSpawn.Z:F0})");
					}
					else
					{
						logger.Msg("  No Wildlife spawn in config — Si_4way will use map center");
					}
				}
				_swappedSolCent = false;
				if ((text == "hvh" || text == "hvhva") && Random.Range(0, 2) == 1)
				{
					SpawnPos factionSpawn2 = mapConfig.GetFactionSpawn("Sol", logger);
					SpawnPos factionSpawn3 = mapConfig.GetFactionSpawn("Cent", logger);
					bool flag4 = true;
					if (factionSpawn2 != null && factionSpawn3 != null)
					{
						bool flag5 = IsPositionSafeForSwap(factionSpawn3.X, factionSpawn3.Z, 30f, 15f, logger);
						bool flag6 = IsPositionSafeForSwap(factionSpawn2.X, factionSpawn2.Z, 30f, 15f, logger);
						if (!flag5 || !flag6)
						{
							flag4 = false;
							logger.Msg($"{text}: Swap SKIPPED — terrain unsafe (Sol pos safe={flag6}, Cent pos safe={flag5})");
						}
					}
					if (flag4)
					{
						_swappedSolCent = true;
						logger.Msg(text + ": Swapping Sol ↔ Cent spawn positions");
					}
				}
				logger.Msg("SpawnBaseStructures: Overriding HQ positions from config");
				List<Transform> usedPoints = new List<Transform>();
				FieldInfo idxField = _strategyTeamSetupType?.GetField("BaseStructureStartPointIndex", BindingFlags.Instance | BindingFlags.Public);
				FieldInfo fieldInfo = _strategyTeamSetupType?.GetField("BaseStructureStartPoints", BindingFlags.Instance | BindingFlags.Public);
				FieldInfo baseStructureField = _teamType?.GetField("BaseStructure", BindingFlags.Instance | BindingFlags.Public);
				PropertyInfo baseStructureProp = _teamType?.GetProperty("BaseStructure", BindingFlags.Instance | BindingFlags.Public);
				Vector3 val = Vector3.zero;
				float num3 = 1000f;
				Terrain activeTerrain = Terrain.activeTerrain;
				if ((Object)(object)activeTerrain != (Object)null)
				{
					Bounds bounds = activeTerrain.terrainData.bounds;
					Vector3 extents = bounds.extents;
					Vector3 position = ((Component)activeTerrain).transform.position;
					bounds = activeTerrain.terrainData.bounds;
					val = position + bounds.center;
					num3 = (extents.x + extents.z) * 0.5f * 0.5f;
				}
				float num4 = Random.Range(0f, 360f);
				float num5 = ((num2 > 0) ? (360f / (float)num2) : 0f);
				int num6 = 0;
				for (int num7 = 0; num7 < num; num7++)
				{
					object obj3 = property?.GetValue(value, new object[1] { num7 });
					if (obj3 == null || !(methodInfo != null) || !(bool)(methodInfo.Invoke(__instance, new object[1] { obj3 }) ?? ((object)false)))
					{
						continue;
					}
					object teamFromSetup2 = GetTeamFromSetup(obj3);
					if (teamFromSetup2 == null)
					{
						num6++;
						continue;
					}
					string teamName = GetTeamName(teamFromSetup2);
					string text2;
					if (teamName.Contains("Sol"))
					{
						text2 = "Sol";
					}
					else if (teamName.Contains("Cent"))
					{
						text2 = "Cent";
					}
					else if (teamName.Contains("Alien") && !teamName.Contains("Worm"))
					{
						text2 = "Alien";
					}
					else
					{
						text2 = ((teamName.Contains("Worm") || teamName.Contains("Wildlife")) ? "Wildlife" : "");
					}
					string text3 = text2;
					if (_swappedSolCent)
					{
						if (text2 == "Sol")
						{
							text3 = "Cent";
						}
						else if (text2 == "Cent")
						{
							text3 = "Sol";
						}
					}
					SpawnPos spawnPos = ((!string.IsNullOrEmpty(text3)) ? mapConfig.GetFactionSpawn(text3, logger) : null);
					List<Transform> startPoints = fieldInfo?.GetValue(obj3) as List<Transform>;
					if (spawnPos != null)
					{
						float x = spawnPos.X;
						float z = spawnPos.Z;
						float num8 = SampleSurfaceHeight(x, z, logger);
						Vector3 target = new Vector3(x, num8, z);
						flag = true;
						SpawnAtBestStartPoint(obj3, teamFromSetup2, startPoints, target, usedPoints, idxField, baseStructureField, baseStructureProp, logger, teamName, useExactPosition: true);
					}
					else
					{
						float num9 = (num4 + num5 * (float)num6) * (MathF.PI / 180f);
						float num10 = num3 * Random.Range(0.8f, 1.2f);
						Vector3 target2 = val + new Vector3(Mathf.Sin(num9), 0f, Mathf.Cos(num9)) * num10;
						flag = true;
						SpawnAtBestStartPoint(obj3, teamFromSetup2, startPoints, target2, usedPoints, idxField, baseStructureField, baseStructureProp, logger, teamName, useExactPosition: false);
						logger.Msg("  " + teamName + ": Using default placement (no config position)");
					}
					num6++;
				}
				try
				{
					EnsureResourcesDistributed(0.8f, "spawn-time fallback");
				}
				catch (Exception arg)
				{
					logger.Error($"Resource fallback failed: {arg}");
				}
				return false;
			}
			catch (Exception arg2)
			{
				logger.Error($"SpawnBaseStructures override failed: {arg2}");
				if (flag)
				{
					logger.Warning("HQs were already spawned before the error — skipping original to prevent duplicates");
					return false;
				}
				return true;
			}
		}

		private static void SpawnAtBestStartPoint(object setup, object team, List<Transform>? startPoints, Vector3 target, List<Transform> usedPoints, FieldInfo? idxField, FieldInfo? baseStructureField, PropertyInfo? baseStructureProp, Instance log, string teamName, bool useExactPosition)
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_0198: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Unknown result type (might be due to invalid IL or missing references)
			//IL_0246: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_0286: Unknown result type (might be due to invalid IL or missing references)
			//IL_0295: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
			if (startPoints == null || startPoints.Count == 0)
			{
				log.Warning("  " + teamName + ": No BaseStructureStartPoints — cannot spawn");
				return;
			}
			int num = 0;
			float num2 = float.MaxValue;
			Transform val = null;
			for (int i = 0; i < startPoints.Count; i++)
			{
				Transform val2 = startPoints[i];
				if (!usedPoints.Contains(val2))
				{
					float num3 = val2.position.x - target.x;
					float num4 = val2.position.z - target.z;
					float num5 = num3 * num3 + num4 * num4;
					if ((Object)(object)val == (Object)null || num5 < num2)
					{
						num2 = num5;
						num = i;
						val = val2;
					}
				}
			}
			if ((Object)(object)val != (Object)null)
			{
				usedPoints.Add(val);
			}
			idxField?.SetValue(setup, num);
			object obj = baseStructureProp?.GetValue(team) ?? baseStructureField?.GetValue(team);
			if (obj == null)
			{
				log.Error("  " + teamName + ": BaseStructure is null");
				return;
			}
			PropertyInfo? property = obj.GetType().GetProperty("Prefab", BindingFlags.Instance | BindingFlags.Public);
			FieldInfo field = obj.GetType().GetField("Prefab", BindingFlags.Instance | BindingFlags.Public);
			object? obj2 = property?.GetValue(obj) ?? field?.GetValue(obj);
			GameObject val3 = (GameObject)((obj2 is GameObject) ? obj2 : null);
			if ((Object)(object)val3 == (Object)null)
			{
				log.Error("  " + teamName + ": BaseStructure.Prefab is null");
				return;
			}
			Quaternion rotation = ((val != null) ? val.rotation : Quaternion.identity);
			Vector3 val4;
			if (useExactPosition)
			{
				val4 = target;
			}
			else
			{
				val4 = ((val != null) ? val.position : target);
			}
			if (useExactPosition && (Object)(object)val != (Object)null)
			{
				log.Msg($"  {teamName}: Relocating start point from ({val.position.x:F0},{val.position.z:F0}) to ({val4.x:F0},{val4.z:F0})");
				val.position = val4;
			}
			bool flag = (Object)(object)val3.GetComponent<HumanStructure>() != (Object)null;
			Vector3 val5 = (flag ? (val4 + new Vector3(0f, 5f, 0f)) : val4);
			SpawnHQPrefab(val3, team, val5, rotation, log);
			string text = (flag ? $" [+{5f:F0}m]" : " [ground, alien]");
			log.Msg($"  {teamName}: Spawned HQ at ({val5.x:F0}, {val5.y:F0}, {val5.z:F0}){text} — startPointIdx={num}");
		}

		private static object? GetTeamFromSetup(object setup)
		{
			Type type = setup.GetType();
			while (type != null)
			{
				PropertyInfo property = type.GetProperty("Team", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
				if (property != null)
				{
					return property.GetValue(setup);
				}
				FieldInfo field = type.GetField("Team", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
				if (field != null)
				{
					return field.GetValue(setup);
				}
				type = type.BaseType;
			}
			return null;
		}

		private static string GetTeamName(object team)
		{
			PropertyInfo? propertyInfo = _teamType?.GetProperty("TeamName", BindingFlags.Instance | BindingFlags.Public);
			FieldInfo fieldInfo = _teamType?.GetField("TeamName", BindingFlags.Instance | BindingFlags.Public);
			return (string)(propertyInfo?.GetValue(team) ?? fieldInfo?.GetValue(team) ?? "");
		}

		private static MethodInfo? FindMethod(Type type, string name)
		{
			MethodInfo method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
			if (method != null)
			{
				return method;
			}
			Type baseType = type.BaseType;
			while (baseType != null)
			{
				method = baseType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
				if (method != null)
				{
					return method;
				}
				baseType = baseType.BaseType;
			}
			return null;
		}

		private static void SpawnHQPrefab(GameObject prefab, object team, Vector3 position, Quaternion rotation, Instance log)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			if (_spawnPrefabMethod == null)
			{
				log.Error("SpawnPrefab method not resolved");
				return;
			}
			int num = _spawnPrefabMethod.GetParameters().Length;
			object[] parameters;
			if (num >= 7)
			{
				parameters = new object[7] { prefab, null, team, position, rotation, true, true };
			}
			else
			{
				parameters = ((num < 5) ? new object[5] { prefab, null, team, position, rotation } : new object[5] { prefab, null, team, position, rotation });
			}
			try
			{
				_spawnPrefabMethod.Invoke(null, parameters);
			}
			catch (TargetInvocationException ex) when (ex.InnerException is ArgumentOutOfRangeException)
			{
				log.Warning("SpawnPrefab network error (player disconnect during spawn): " + ex.InnerException.Message);
			}
		}

		private static bool EnsureResourcesDistributed(float resourceMultiplier, string why)
		{
			Instance logger = Melon<MapBalance>.Logger;
			if (_distributedThisRound)
			{
				return true;
			}
			if (_activeConfig == null)
			{
				_activeConfig = LoadMapConfig();
			}
			if (_activeConfig == null)
			{
				return false;
			}
			if (GetAllResourceAreas().Count == 0)
			{
				logger.Msg("Resource distribution deferred (" + why + ") — no ResourceAreas registered yet");
				return false;
			}
			CustomDistribution(resourceMultiplier);
			_distributedThisRound = true;
			logger.Msg("Resource amounts applied (" + why + ")");
			return true;
		}

		private static bool Prefix_DistributeAllResources(float resourceMultiplier, float disableAmount01)
		{
			Instance logger = Melon<MapBalance>.Logger;
			logger.Msg($"=== DistributeAllResources intercepted (mult={resourceMultiplier}, hide={disableAmount01}) ===");
			try
			{
				if (_activeConfig == null)
				{
					_activeConfig = LoadMapConfig();
				}
				if (_activeConfig == null)
				{
					logger.Msg("No map config — letting game handle resource distribution (vanilla)");
					return true;
				}
				if (_distributedThisRound)
				{
					logger.Msg("Already distributed this round — skipping original only");
					return false;
				}
				EnsureResourcesDistributed(resourceMultiplier, "game call");
			}
			catch (Exception arg)
			{
				logger.Error($"Custom distribution failed: {arg}");
				return true;
			}
			return false;
		}

		private static void CustomDistribution(float resourceMultiplier)
		{
			Instance logger = Melon<MapBalance>.Logger;
			if (_resourceAreaType == null)
			{
				return;
			}
			List<Component> allResourceAreas = GetAllResourceAreas();
			if (allResourceAreas.Count == 0)
			{
				logger.Warning("No ResourceAreas found");
				return;
			}
			int num = 0;
			int num2 = 0;
			foreach (Component item in allResourceAreas)
			{
				string resourceName = GetResourceName(item);
				if (resourceName == "Balterium")
				{
					num++;
				}
				else if (resourceName == "Biotics")
				{
					num2++;
				}
				ZeroOutResourceArea(item);
			}
			logger.Msg($"Zeroed {allResourceAreas.Count} existing areas (Balterium: {num}, Biotics: {num2})");
			SyncAllToClients(allResourceAreas);
			CleanupSpawnedAreas();
			SpawnConfiguredResources(_activeConfig, resourceMultiplier, logger);
			SyncAllToClients(GetAllResourceAreas());
			int num3 = CountEnabledAreas();
			logger.Msg($"=== Distribution complete: {num3} total enabled areas ===");
		}

		private static GameObject? GetResourcePrefab(string prefabName, Instance log)
		{
			int num = (int)_getPrefabIndexMethod.Invoke(null, new object[1] { prefabName });
			if (num < 0)
			{
				log.Warning("Prefab '" + prefabName + "' not found");
				return null;
			}
			object? obj = _getPrefabMethod.Invoke(null, new object[1] { num });
			GameObject val = (GameObject)((obj is GameObject) ? obj : null);
			if ((Object)(object)val != (Object)null)
			{
				log.Msg($"Found prefab: {((Object)val).name} (index={num})");
			}
			return val;
		}

		private static void SpawnConfiguredResources(MapConfig config, float resourceMultiplier, Instance log)
		{
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0280: Unknown result type (might be due to invalid IL or missing references)
			//IL_0274: Unknown result type (might be due to invalid IL or missing references)
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_022d: Unknown result type (might be due to invalid IL or missing references)
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0285: Unknown result type (might be due to invalid IL or missing references)
			//IL_029f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
			HashSet<int> hashSet = new HashSet<int>(config.Resources.RemovedPatches);
			Dictionary<string, int> patchOverrides = config.Resources.PatchOverrides;
			List<Component> allResourceAreas = GetAllResourceAreas();
			log.Msg($"Found {allResourceAreas.Count} existing resource areas in scene");
			if (hashSet.Count > 0)
			{
				log.Msg("  Removed patches: [" + string.Join(", ", hashSet) + "]");
			}
			if (patchOverrides.Count > 0)
			{
				log.Msg($"  Patch overrides: {patchOverrides.Count} entries");
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < allResourceAreas.Count; i++)
			{
				Component val = allResourceAreas[i];
				string resourceName = GetResourceName(val);
				bool flag = resourceName == "Balterium";
				bool flag2 = resourceName == "Biotics";
				if (hashSet.Contains(i))
				{
					num3++;
					continue;
				}
				string key = i.ToString();
				int num5 = 0;
				bool flag3 = false;
				if (patchOverrides.ContainsKey(key))
				{
					num5 = patchOverrides[key];
					flag3 = true;
				}
				else if (flag && config.Resources.BalteriumAmount > 0)
				{
					num5 = config.Resources.BalteriumAmount;
				}
				else if (flag2 && config.Resources.BioticsAmount > 0)
				{
					num5 = config.Resources.BioticsAmount;
				}
				if (num5 <= 0)
				{
					continue;
				}
				SetEnabled(val, enabled: true);
				SetMemberValue(val, "ResourceDistributionAmountMult", resourceMultiplier);
				CallDistributeResources(val);
				SetResourceAmount(val, num5, log);
				if (flag3)
				{
					num4++;
					Vector3 val2 = ((val != null) ? val.transform.position : Vector3.zero);
					log.Msg($"OVERRIDE [{i}] {resourceName}: ({val2.x:F0},{val2.z:F0}) amount={num5}");
				}
				else if (flag)
				{
					num++;
					if (num <= 3)
					{
						Vector3 val3 = ((val != null) ? val.transform.position : Vector3.zero);
						log.Msg($"BALT [{i}]: ({val3.x:F0},{val3.z:F0}) amount={num5}");
					}
				}
				else if (flag2)
				{
					num2++;
					if (num2 <= 3)
					{
						Vector3 val4 = ((val != null) ? val.transform.position : Vector3.zero);
						log.Msg($"BIO  [{i}]: ({val4.x:F0},{val4.z:F0}) amount={num5}");
					}
				}
			}
			log.Msg($"Enabled {num} Balterium, {num2} Biotics areas ({num4} overridden, {num3} removed)");
		}

		private static void SpawnLegacyPositions(GameObject baltPrefab, Terrain? terrain, float terrainY0, float resourceMultiplier, Instance log)
		{
			int paramCount = _spawnPrefabMethod.GetParameters().Length;
			int num = 0;
			for (int i = 0; i < DesignedPositions.Length; i++)
			{
				float num2 = DesignedPositions[i][0];
				float num3 = DesignedPositions[i][1];
				GameObject val = SpawnResourceArea(baltPrefab, num2, num3, terrain, terrainY0, paramCount, log);
				if (!((Object)(object)val == (Object)null))
				{
					_spawnedAreas.Add(val);
					Component component = val.GetComponent(_resourceAreaType);
					if ((Object)(object)component != (Object)null)
					{
						SetMemberValue(component, "ResourceDistributionAmountMult", resourceMultiplier);
						CallDistributeResources(component);
					}
					num++;
					if (i < 3 || i >= DesignedPositions.Length - 2)
					{
						log.Msg($"SPAWN [{i}]: ({num2:F0},{num3:F0}) -> {((Object)val).name}");
					}
				}
			}
			log.Msg($"Legacy: spawned {num}/{DesignedPositions.Length} Balterium areas");
		}

		private static GameObject? SpawnResourceArea(GameObject prefab, float tx, float tz, Terrain? terrain, float terrainY0, int paramCount, Instance log)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			float num = terrainY0;
			if ((Object)(object)terrain != (Object)null)
			{
				try
				{
					num = terrain.SampleHeight(new Vector3(tx, 0f, tz)) + terrainY0;
				}
				catch
				{
				}
			}
			Vector3 val = new Vector3(tx, num, tz);
			Quaternion identity = Quaternion.identity;
			object[] parameters = ((paramCount < 7) ? new object[5] { prefab, null, null, val, identity } : new object[7] { prefab, null, null, val, identity, true, true });
			try
			{
				object? obj2 = _spawnPrefabMethod.Invoke(null, parameters);
				return (GameObject?)((obj2 is GameObject) ? obj2 : null);
			}
			catch (Exception ex)
			{
				log.Error($"SpawnPrefab failed at ({tx:F0},{tz:F0}): {ex.Message}");
				return null;
			}
		}

		private static void SetResourceAmount(Component area, int targetAmount, Instance log)
		{
			int memberValueInt = GetMemberValueInt(area, "ResourceAmountMax");
			if (memberValueInt > 0 && memberValueInt != targetAmount)
			{
				float memberValue = GetMemberValue<float>(area, "ResourceDistributionAmountMult");
				float num = (float)targetAmount / (float)memberValueInt;
				float num2 = memberValue * num;
				SetMemberValue(area, "ResourceDistributionAmountMult", num2);
				CallDistributeResources(area);
				int memberValueInt2 = GetMemberValueInt(area, "ResourceAmountMax");
				if ((float)Math.Abs(memberValueInt2 - targetAmount) > (float)targetAmount * 0.05f)
				{
					log.Warning($"  Amount mismatch: target={targetAmount}, got={memberValueInt2} (mult={num2:F4})");
				}
			}
			else if (memberValueInt == 0)
			{
				SetMemberValueInt(area, "ResourceAmountMax", targetAmount);
				SetMemberValueInt(area, "ResourceAmountCurrent", targetAmount);
			}
		}

		private static void CleanupSpawnedAreas()
		{
			Instance logger = Melon<MapBalance>.Logger;
			int num = 0;
			foreach (GameObject spawnedArea in _spawnedAreas)
			{
				if ((Object)(object)spawnedArea != (Object)null)
				{
					Object.Destroy((Object)(object)spawnedArea);
					num++;
				}
			}
			_spawnedAreas.Clear();
			if (num > 0)
			{
				logger.Msg($"Cleaned up {num} previously spawned areas");
			}
		}

		private static bool HasKohBlock(JObject obj)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Invalid comparison between Unknown and I4
			JToken val = obj["koh"];
			if (val != null)
			{
				return (int)val.Type == 1;
			}
			return false;
		}

		private static string SelectLayout(List<string> all, List<string> withKoh, List<string> noKoh, Instance log)
		{
			List<string> list = all;
			if (IsKohModeActive())
			{
				if (withKoh.Count > 0)
				{
					log.Msg($"KGT KoH mode active (Mode 1) — preferring {withKoh.Count}/{all.Count} layouts with 'koh' field");
					list = withKoh;
				}
			}
			else if (withKoh.Count > 0)
			{
				if (noKoh.Count > 0)
				{
					log.Msg($"KGT KoH mode inactive — excluding {withKoh.Count} KoH layout(s), picking from {noKoh.Count}");
					list = noKoh;
				}
				else
				{
					log.Warning($"KGT KoH mode inactive but all {all.Count} candidate layout(s) carry 'koh' — " + "using one anyway; the KoH tower will not be spawned");
				}
			}
			int index = ((list.Count != 1) ? Random.Range(0, list.Count) : 0);
			return list[index];
		}

		private static MapConfig? LoadMapConfig(string gameMode = "hvh")
		{
			Instance logger = Melon<MapBalance>.Logger;
			string currentMapName = GetCurrentMapName();
			if (string.IsNullOrEmpty(currentMapName))
			{
				logger.Warning("Cannot detect map name");
				return null;
			}
			string text = null;
			string path = Path.Combine(SpawnsDir, currentMapName);
			if (Directory.Exists(path))
			{
				string[] files = Directory.GetFiles(path, "*.json");
				List<string> list = new List<string>();
				List<string> list2 = new List<string>();
				List<string> list3 = new List<string>();
				string[] array = files;
				foreach (string text2 in array)
				{
					try
					{
						JObject val = JObject.Parse(File.ReadAllText(text2));
						JToken val2 = val["game_modes"];
						if (val2 == null)
						{
							goto IL_00a7;
						}
						JToken val3 = val2[(object)gameMode];
						if (val3 != null && Extensions.Value<bool>((IEnumerable<JToken>)val3))
						{
							goto IL_00a7;
						}
						goto end_IL_006c;
						IL_00a7:
						list.Add(text2);
						if (HasKohBlock(val))
						{
							list2.Add(text2);
						}
						else
						{
							list3.Add(text2);
						}
						end_IL_006c:;
					}
					catch
					{
					}
				}
				logger.Msg($"Found {files.Length} layouts for {currentMapName}, {list.Count} match game mode '{gameMode}' ({list2.Count} with KoH)");
				if (list.Count > 0)
				{
					text = SelectLayout(list, list2, list3, logger);
				}
				else if (files.Length != 0)
				{
					List<string> list4 = new List<string>();
					List<string> list5 = new List<string>();
					List<string> list6 = new List<string>();
					array = files;
					foreach (string text3 in array)
					{
						try
						{
							JObject val4 = JObject.Parse(File.ReadAllText(text3));
							if (val4["game_modes"] == null)
							{
								list4.Add(text3);
								if (HasKohBlock(val4))
								{
									list5.Add(text3);
								}
								else
								{
									list6.Add(text3);
								}
							}
						}
						catch
						{
						}
					}
					if (list4.Count > 0)
					{
						logger.Warning($"No layouts match game mode '{gameMode}' — picking from {list4.Count} mode-agnostic layout(s)");
						text = SelectLayout(list4, list5, list6, logger);
					}
					else
					{
						logger.Warning("No layouts match game mode '" + gameMode + "' and no mode-agnostic layouts — vanilla mode");
					}
				}
			}
			if (text == null)
			{
				string text4 = Path.Combine(LegacyConfigDir, currentMapName + ".json");
				if (File.Exists(text4))
				{
					text = text4;
				}
			}
			if (text == null && Directory.Exists(LegacyConfigDir))
			{
				string[] array = Directory.GetFiles(LegacyConfigDir, "*.json");
				foreach (string text5 in array)
				{
					try
					{
						if (((object)JObject.Parse(File.ReadAllText(text5))["map"])?.ToString() == currentMapName)
						{
							text = text5;
							break;
						}
					}
					catch
					{
					}
				}
			}
			if (text == null)
			{
				logger.Msg("No config found for map '" + currentMapName + "' — vanilla mode");
				return null;
			}
			try
			{
				MapConfig mapConfig = JsonConvert.DeserializeObject<MapConfig>(File.ReadAllText(text));
				if (mapConfig == null)
				{
					logger.Warning("Failed to parse config: " + text);
					return null;
				}
				_activeConfigName = Path.GetFileNameWithoutExtension(text);
				logger.Msg("=== Loaded map config: " + text + " ===");
				logger.Msg("  Map: " + mapConfig.Map);
				if (!string.IsNullOrEmpty(mapConfig.Description))
				{
					logger.Msg("  Description: " + mapConfig.Description);
				}
				logger.Msg($"  Balterium: {mapConfig.Resources.BalteriumAmount}, Biotics: {mapConfig.Resources.BioticsAmount}");
				if (mapConfig.Resources.RemovedPatches.Count > 0)
				{
					logger.Msg("  Removed patches: [" + string.Join(", ", mapConfig.Resources.RemovedPatches) + "]");
				}
				if (mapConfig.Resources.PatchOverrides.Count > 0)
				{
					logger.Msg($"  Patch overrides: {mapConfig.Resources.PatchOverrides.Count} entries");
				}
				logger.Msg($"  Has spawns: {mapConfig.HasSpawns()}");
				return mapConfig;
			}
			catch (Exception ex)
			{
				logger.Error("Error loading config " + text + ": " + ex.Message);
				return null;
			}
		}

		private static void ZeroOutResourceArea(Component area)
		{
			SetEnabled(area, enabled: false);
			FieldInfo? field = _resourceAreaType.GetField("ResourceAmounts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			PropertyInfo property = _resourceAreaType.GetProperty("ResourceAmounts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if ((field?.GetValue(area) ?? property?.GetValue(area)) is int[,] array)
			{
				int length = array.GetLength(0);
				int length2 = array.GetLength(1);
				for (int i = 0; i < length; i++)
				{
					for (int j = 0; j < length2; j++)
					{
						array[i, j] = 0;
					}
				}
				FieldInfo? field2 = _resourceAreaType.GetField("Net_ResourceChanged", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				PropertyInfo property2 = _resourceAreaType.GetProperty("Net_ResourceChanged", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if ((field2?.GetValue(area) ?? property2?.GetValue(area)) is bool[,] array2)
				{
					for (int k = 0; k < length; k++)
					{
						for (int l = 0; l < length2; l++)
						{
							array2[k, l] = true;
						}
					}
				}
			}
			SetMemberValueInt(area, "ResourceAmountCurrent", 0);
			SetMemberValueInt(area, "ResourceAmountMax", 0);
		}

		private static void SyncAllToClients(List<Component> allAreas)
		{
			Instance logger = Melon<MapBalance>.Logger;
			if (_sendNetInitMethod == null)
			{
				logger.Warning("SendNetInit method not found — cannot sync to clients");
				return;
			}
			int num = 0;
			int num2 = 0;
			foreach (Component allArea in allAreas)
			{
				try
				{
					object networkComponent = GetNetworkComponent(allArea);
					if (networkComponent == null)
					{
						num2++;
						continue;
					}
					int num3 = _sendNetInitMethod.GetParameters().Length;
					switch (num3)
					{
					case 0:
						_sendNetInitMethod.Invoke(networkComponent, null);
						break;
					case 1:
						_sendNetInitMethod.Invoke(networkComponent, new object[1]);
						break;
					default:
						_sendNetInitMethod.Invoke(networkComponent, new object[num3]);
						break;
					}
					num++;
				}
				catch (Exception ex)
				{
					if (num2 == 0)
					{
						logger.Warning("SendNetInit failed: " + ex.Message);
					}
					num2++;
				}
			}
			logger.Msg($"Synced {num} areas to clients ({num2} failed)");
		}

		private static object? GetNetworkComponent(Component area)
		{
			FieldInfo field = _resourceAreaType.GetField("NetworkComponent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				return field.GetValue(area);
			}
			PropertyInfo property = _resourceAreaType.GetProperty("NetworkComponent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				return property.GetValue(area);
			}
			if (_networkComponentType != null)
			{
				return typeof(GameObject).GetMethod("GetComponent", new Type[1] { typeof(Type) })?.Invoke(area.gameObject, new object[1] { _networkComponentType });
			}
			return null;
		}

		private static RefineryCheckData? ExtractBuildingData(Func<string, bool> nameFilter, string label)
		{
			//IL_04e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0505: Unknown result type (might be due to invalid IL or missing references)
			//IL_050a: Unknown result type (might be due to invalid IL or missing references)
			//IL_051f: Unknown result type (might be due to invalid IL or missing references)
			//IL_052e: Unknown result type (might be due to invalid IL or missing references)
			//IL_053d: Unknown result type (might be due to invalid IL or missing references)
			//IL_054c: Unknown result type (might be due to invalid IL or missing references)
			//IL_055b: Unknown result type (might be due to invalid IL or missing references)
			//IL_056a: Unknown result type (might be due to invalid IL or missing references)
			Instance logger = Melon<MapBalance>.Logger;
			if (_constructionDataType == null)
			{
				logger.Error("ConstructionData type not found");
				return null;
			}
			if (_constructionPreviewType == null)
			{
				logger.Error("ConstructionPreview type not found");
				return null;
			}
			Object[] array = Resources.FindObjectsOfTypeAll(_constructionDataType);
			Object val = null;
			Object[] array2 = array;
			foreach (Object val2 in array2)
			{
				if (val2 != (Object)null && nameFilter(val2.name))
				{
					val = val2;
					logger.Msg("Found " + label + ": " + val2.name);
					break;
				}
			}
			if (val == (Object)null)
			{
				logger.Warning($"{label} not found in {array.Length} ConstructionData assets");
				return null;
			}
			RefineryCheckData refineryCheckData = new RefineryCheckData();
			refineryCheckData.Name = val.name;
			refineryCheckData.GridSnapXZ = GetMemberValue<float>(val, "GridSnapXZ");
			refineryCheckData.GridSnapY = GetMemberValue<float>(val, "GridSnapY");
			if (refineryCheckData.GridSnapXZ <= 0f)
			{
				refineryCheckData.GridSnapXZ = 10f;
			}
			if (refineryCheckData.GridSnapY <= 0f)
			{
				refineryCheckData.GridSnapY = 5f;
			}
			logger.Msg($"[{label}] Grid: XZ={refineryCheckData.GridSnapXZ}, Y={refineryCheckData.GridSnapY}");
			GameObject memberValue = GetMemberValue<GameObject>(val, "ObjectPreviewPrefab");
			if ((Object)(object)memberValue == (Object)null)
			{
				logger.Error("[" + label + "] ObjectPreviewPrefab is null");
				return null;
			}
			Component component = memberValue.GetComponent(_constructionPreviewType);
			if ((Object)(object)component == (Object)null)
			{
				logger.Error("[" + label + "] ConstructionPreview not found on preview prefab");
				return null;
			}
			refineryCheckData.RotateToSurface = GetMemberValue<bool>(component, "StructureRotateToSurface");
			refineryCheckData.MaxAngle = GetMemberValue<float>(component, "StructureRotateMaxAngle");
			logger.Msg($"[{label}] RotateToSurface={refineryCheckData.RotateToSurface}, MaxAngle={refineryCheckData.MaxAngle}");
			refineryCheckData.AboveGroundLocal = ExtractTransformLocalPositions(component, "AboveGroundPoints");
			logger.Msg($"[{label}] AboveGroundPoints: {refineryCheckData.AboveGroundLocal.Count}");
			refineryCheckData.BelowGroundLocal = ExtractTransformLocalPositions(component, "BelowGroundPoints");
			logger.Msg($"[{label}] BelowGroundPoints: {refineryCheckData.BelowGroundLocal.Count}");
			if (GetRawMember(component, "PreviewEntries") is IList list)
			{
				logger.Msg($"[{label}] PreviewEntries: {list.Count}");
				foreach (object item in list)
				{
					if (item != null)
					{
						EntryCheckData entryCheckData = new EntryCheckData();
						entryCheckData.AboveLocal = ExtractTransformLocalPositions(item, "AboveGroundPoints");
						entryCheckData.BelowLocal = ExtractTransformLocalPositions(item, "BelowGroundPoints");
						entryCheckData.MustAboveCount = GetMemberValue<int>(item, "MustBeAboveGroundCount");
						entryCheckData.MustBelowCount = GetMemberValue<int>(item, "MustBeBelowGroundCount");
						refineryCheckData.PreviewEntries.Add(entryCheckData);
						logger.Msg($"  Entry: above={entryCheckData.AboveLocal.Count}(need>={entryCheckData.MustAboveCount}), below={entryCheckData.BelowLocal.Count}(need>={entryCheckData.MustBelowCount})");
					}
				}
			}
			GameObject memberValue2 = GetMemberValue<GameObject>(val, "ObjectToBuild");
			if ((Object)(object)memberValue2 == (Object)null)
			{
				object rawMember = GetRawMember(val, "ObjectInfo");
				if (rawMember != null)
				{
					memberValue2 = GetMemberValue<GameObject>(rawMember, "Prefab");
				}
			}
			if ((Object)(object)memberValue2 != (Object)null && _enableAboveGroundType != null)
			{
				logger.Msg("[" + label + "] Structure prefab: " + ((Object)memberValue2).name);
				Component componentInChildren = memberValue2.GetComponentInChildren(_enableAboveGroundType, true);
				if ((Object)(object)componentInChildren != (Object)null)
				{
					Transform transform = memberValue2.transform;
					ExtractRampPoints(componentInChildren, "EnableObjectIfPointAboveGround", isAboveType: true, transform, refineryCheckData.Ramps, logger);
					ExtractRampPoints(componentInChildren, "EnableObjectIfPointUnderGround", isAboveType: false, transform, refineryCheckData.Ramps, logger);
					logger.Msg($"[{label}] Terrain check points: {refineryCheckData.Ramps.Count}");
				}
				else
				{
					logger.Msg("[" + label + "] No EnableAboveGround component (OK for HQ)");
				}
			}
			if (_constructionBoundsType != null)
			{
				Component[] componentsInChildren = memberValue.GetComponentsInChildren(_constructionBoundsType, true);
				logger.Msg($"[{label}] ConstructionBounds: {componentsInChildren.Length}");
				Component[] array3 = componentsInChildren;
				foreach (Component val3 in array3)
				{
					if (!((Object)(object)val3 == (Object)null))
					{
						Vector3 memberValue3 = GetMemberValue<Vector3>(val3, "Size");
						object rawMember2 = GetRawMember(val3, "BoundsType");
						Vector3 localPosition = val3.transform.localPosition;
						logger.Msg($"  {rawMember2} size=({memberValue3.x:F1},{memberValue3.y:F1},{memberValue3.z:F1}) localPos=({localPosition.x:F1},{localPosition.y:F1},{localPosition.z:F1})");
					}
				}
			}
			return refineryCheckData;
		}

		private static RefineryCheckData? ExtractRefineryData()
		{
			return ExtractBuildingData((string name) => name.Contains("Sol") && name.Contains("Refinery") && !name.Contains("Turret") && !name.Contains("Upgrade"), "Sol_Refinery");
		}

		private static void ExtractRampPoints(object enableAboveGround, string listName, bool isAboveType, Transform structRoot, List<RampCheckData> ramps, Instance log)
		{
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			if (!(GetRawMember(enableAboveGround, listName) is IList list))
			{
				return;
			}
			log.Msg($"  {listName}: {list.Count} entries");
			foreach (object item in list)
			{
				if (item == null)
				{
					continue;
				}
				RampCheckData rampCheckData = new RampCheckData();
				rampCheckData.IsAboveType = isAboveType;
				GameObject memberValue = GetMemberValue<GameObject>(item, "Point");
				if ((Object)(object)memberValue != (Object)null)
				{
					rampCheckData.CheckPointLocal = structRoot.InverseTransformPoint(memberValue.transform.position);
					log.Msg($"    CheckPoint: ({rampCheckData.CheckPointLocal.x:F2}, {rampCheckData.CheckPointLocal.y:F2}, {rampCheckData.CheckPointLocal.z:F2})");
				}
				object rawMember = GetRawMember(item, "DepositPoint");
				rampCheckData.HasDepositPoint = rawMember != null;
				object? rawMember2 = GetRawMember(item, "DepositPointEntryPoint");
				Transform val = (Transform)((rawMember2 is Transform) ? rawMember2 : null);
				if ((Object)(object)val != (Object)null)
				{
					rampCheckData.EntryLocal = structRoot.InverseTransformPoint(val.position);
					log.Msg($"    EntryPoint: ({rampCheckData.EntryLocal.x:F2}, {rampCheckData.EntryLocal.y:F2}, {rampCheckData.EntryLocal.z:F2})");
				}
				if (rampCheckData.EntryLocal == Vector3.zero && rawMember != null && GetRawMember(rawMember, "AIEntryPoints") is IList { Count: >0 } list2)
				{
					object? obj = list2[0];
					Transform val2 = (Transform)((obj is Transform) ? obj : null);
					if ((Object)(object)val2 != (Object)null)
					{
						rampCheckData.EntryLocal = structRoot.InverseTransformPoint(val2.position);
						log.Msg($"    EntryPoint(AI): ({rampCheckData.EntryLocal.x:F2}, {rampCheckData.EntryLocal.y:F2}, {rampCheckData.EntryLocal.z:F2})");
					}
				}
				ramps.Add(rampCheckData);
			}
		}

		private static void ScanRefineryPlacements()
		{
			Melon<MapBalance>.Logger.Msg("=== Starting Refinery Placement Scan ===");
			ScanSingleRefinery("Sol", (string n) => n.Contains("Sol") && n.Contains("Refinery") && !n.Contains("Turret") && !n.Contains("Upgrade"));
			ScanSingleRefinery("Cent", (string n) => n.Contains("Cent") && n.Contains("Refinery") && !n.Contains("Turret") && !n.Contains("Upgrade"));
		}

		private static void ScanSingleRefinery(string faction, Func<string, bool> nameFilter)
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0304: Unknown result type (might be due to invalid IL or missing references)
			//IL_0310: Unknown result type (might be due to invalid IL or missing references)
			//IL_031c: Unknown result type (might be due to invalid IL or missing references)
			//IL_037f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0384: Unknown result type (might be due to invalid IL or missing references)
			//IL_038d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0399: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0678: Unknown result type (might be due to invalid IL or missing references)
			//IL_0687: Unknown result type (might be due to invalid IL or missing references)
			//IL_0696: Unknown result type (might be due to invalid IL or missing references)
			//IL_06a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_06c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0488: Unknown result type (might be due to invalid IL or missing references)
			//IL_048d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0496: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_0826: Unknown result type (might be due to invalid IL or missing references)
			//IL_082b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0841: Unknown result type (might be due to invalid IL or missing references)
			//IL_0846: Unknown result type (might be due to invalid IL or missing references)
			//IL_085c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0861: Unknown result type (might be due to invalid IL or missing references)
			//IL_0877: Unknown result type (might be due to invalid IL or missing references)
			//IL_087c: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_050a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0516: Unknown result type (might be due to invalid IL or missing references)
			//IL_08b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_08dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_08e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0908: Unknown result type (might be due to invalid IL or missing references)
			//IL_090d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0915: Unknown result type (might be due to invalid IL or missing references)
			//IL_091a: Unknown result type (might be due to invalid IL or missing references)
			//IL_091c: Unknown result type (might be due to invalid IL or missing references)
			//IL_091e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0923: Unknown result type (might be due to invalid IL or missing references)
			//IL_0928: Unknown result type (might be due to invalid IL or missing references)
			//IL_092a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0931: Unknown result type (might be due to invalid IL or missing references)
			//IL_093a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0941: Unknown result type (might be due to invalid IL or missing references)
			//IL_0948: Unknown result type (might be due to invalid IL or missing references)
			//IL_0952: Unknown result type (might be due to invalid IL or missing references)
			//IL_0959: Unknown result type (might be due to invalid IL or missing references)
			//IL_0962: Unknown result type (might be due to invalid IL or missing references)
			//IL_0969: Unknown result type (might be due to invalid IL or missing references)
			//IL_0970: Unknown result type (might be due to invalid IL or missing references)
			//IL_0981: Unknown result type (might be due to invalid IL or missing references)
			//IL_098b: Unknown result type (might be due to invalid IL or missing references)
			//IL_09e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_09e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_09ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_09f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_09f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_09f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_09fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a01: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a03: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a0a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a13: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a1a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a21: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a2b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a32: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a3b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a42: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a49: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a5a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a64: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ac5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aca: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ad2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ad7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ad9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0adb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ae0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ae5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ae7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0af7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0afe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b05: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c75: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c7a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c7e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c83: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c88: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c8d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c8f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c96: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b0f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b16: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c9f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ca6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cad: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b1f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b26: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b2d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cb7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cbe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b71: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b76: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b7e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b83: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b85: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b87: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b8c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b91: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b93: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b9a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cc7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cce: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cd5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d7b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d80: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d84: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d89: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d8e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d93: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d95: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d9c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ba3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0baa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bb1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0da5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dac: Unknown result type (might be due to invalid IL or missing references)
			//IL_0db3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bbb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bc2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d13: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d03: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e2a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e2f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e33: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e38: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e3d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e42: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dbd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dc4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bcb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bd2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bd9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dcd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dd4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ddb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0be5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0df2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e19: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e09: Unknown result type (might be due to invalid IL or missing references)
			Instance logger = Melon<MapBalance>.Logger;
			Stopwatch stopwatch = Stopwatch.StartNew();
			string text = faction + "_Refinery";
			RefineryCheckData refineryCheckData = ExtractBuildingData(nameFilter, text);
			if (refineryCheckData == null)
			{
				logger.Error("Failed to extract " + text + " data");
				return;
			}
			Terrain mainTerrain = GetMainTerrain();
			if ((Object)(object)mainTerrain == (Object)null)
			{
				logger.Error("No terrain found for scan");
				return;
			}
			float y = ((Component)mainTerrain).transform.position.y;
			Vector3 position = ((Component)mainTerrain).transform.position;
			Vector3 size = mainTerrain.terrainData.size;
			float gridSnapXZ = refineryCheckData.GridSnapXZ;
			float gridSnapY = refineryCheckData.GridSnapY;
			float num = Mathf.Ceil(position.x / gridSnapXZ) * gridSnapXZ;
			float num2 = Mathf.Ceil(position.z / gridSnapXZ) * gridSnapXZ;
			float num3 = Mathf.Floor((position.x + size.x) / gridSnapXZ) * gridSnapXZ;
			float num4 = Mathf.Floor((position.z + size.z) / gridSnapXZ) * gridSnapXZ;
			int num5 = Mathf.RoundToInt((num3 - num) / gridSnapXZ) + 1;
			int num6 = Mathf.RoundToInt((num4 - num2) / gridSnapXZ) + 1;
			logger.Msg($"Terrain: pos=({position.x:F0},{position.y:F0},{position.z:F0}) size=({size.x:F0},{size.y:F0},{size.z:F0})");
			logger.Msg($"Scan: X=[{num:F0},{num3:F0}] Z=[{num2:F0},{num4:F0}] steps={num5}x{num6} x4rot = {num5 * num6 * 4} checks");
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder.AppendLine("=== Refinery Placement Scan v1.0 ===");
			stringBuilder.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
			stringBuilder.AppendLine("Map: " + (GetCurrentMapName() ?? "unknown"));
			stringBuilder.AppendLine("Faction: " + faction);
			stringBuilder.AppendLine("Asset: " + refineryCheckData.Name);
			stringBuilder.AppendLine($"GridSnapXZ: {refineryCheckData.GridSnapXZ}, GridSnapY: {refineryCheckData.GridSnapY}");
			stringBuilder.AppendLine($"RotateToSurface: {refineryCheckData.RotateToSurface}, MaxAngle: {refineryCheckData.MaxAngle}");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine($"--- AboveGroundPoints ({refineryCheckData.AboveGroundLocal.Count}) ---");
			foreach (Vector3 item in refineryCheckData.AboveGroundLocal)
			{
				stringBuilder.AppendLine($"  ({item.x:F2}, {item.y:F2}, {item.z:F2})");
			}
			stringBuilder.AppendLine($"--- BelowGroundPoints ({refineryCheckData.BelowGroundLocal.Count}) ---");
			foreach (Vector3 item2 in refineryCheckData.BelowGroundLocal)
			{
				stringBuilder.AppendLine($"  ({item2.x:F2}, {item2.y:F2}, {item2.z:F2})");
			}
			stringBuilder.AppendLine($"--- PreviewEntries ({refineryCheckData.PreviewEntries.Count}) ---");
			for (int i = 0; i < refineryCheckData.PreviewEntries.Count; i++)
			{
				EntryCheckData entryCheckData = refineryCheckData.PreviewEntries[i];
				stringBuilder.AppendLine($"  [{i}] above={entryCheckData.AboveLocal.Count}(need>={entryCheckData.MustAboveCount}), below={entryCheckData.BelowLocal.Count}(need>={entryCheckData.MustBelowCount})");
				foreach (Vector3 item3 in entryCheckData.AboveLocal)
				{
					stringBuilder.AppendLine($"    A: ({item3.x:F2}, {item3.y:F2}, {item3.z:F2})");
				}
				foreach (Vector3 item4 in entryCheckData.BelowLocal)
				{
					stringBuilder.AppendLine($"    B: ({item4.x:F2}, {item4.y:F2}, {item4.z:F2})");
				}
			}
			stringBuilder.AppendLine($"--- Ramps ({refineryCheckData.Ramps.Count}) ---");
			for (int j = 0; j < refineryCheckData.Ramps.Count; j++)
			{
				RampCheckData rampCheckData = refineryCheckData.Ramps[j];
				stringBuilder.AppendLine(string.Format("  [{0}] type={1} check=({2:F2},{3:F2},{4:F2}) entry=({5:F2},{6:F2},{7:F2})", j, rampCheckData.IsAboveType ? "above" : "below", rampCheckData.CheckPointLocal.x, rampCheckData.CheckPointLocal.y, rampCheckData.CheckPointLocal.z, rampCheckData.EntryLocal.x, rampCheckData.EntryLocal.y, rampCheckData.EntryLocal.z));
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine($"Terrain: ({position.x:F0},{position.y:F0},{position.z:F0}) size ({size.x:F0},{size.y:F0},{size.z:F0})");
			stringBuilder.AppendLine($"Scan: X=[{num:F0},{num3:F0}] Z=[{num2:F0},{num4:F0}] step={gridSnapXZ}");
			stringBuilder.AppendLine();
			List<int> list = new List<int>();
			for (int k = 0; k < refineryCheckData.Ramps.Count; k++)
			{
				if (refineryCheckData.Ramps[k].HasDepositPoint)
				{
					list.Add(k);
				}
			}
			logger.Msg($"Harvester ramps (with DepositPoint): {list.Count} of {refineryCheckData.Ramps.Count} total");
			StringBuilder stringBuilder3 = new StringBuilder();
			for (int l = 0; l < list.Count; l++)
			{
				char c = (char)(65 + l);
				stringBuilder3.Append($",ramp{c}_ok,ramp{c}_dirX,ramp{c}_dirZ");
			}
			stringBuilder2.AppendLine($"x,z,rot,terrainY,adjustY,finalY{stringBuilder3}");
			int num7 = 0;
			int num8 = 0;
			int[] array = new int[refineryCheckData.Ramps.Count];
			Quaternion[] array2 = new Quaternion[4]
			{
				Quaternion.Euler(0f, 0f, 0f),
				Quaternion.Euler(0f, 90f, 0f),
				Quaternion.Euler(0f, 180f, 0f),
				Quaternion.Euler(0f, 270f, 0f)
			};
			float[] array3 = new float[4] { 0f, 90f, 180f, 270f };
			for (float num9 = num; num9 <= num3; num9 += gridSnapXZ)
			{
				for (float num10 = num2; num10 <= num4; num10 += gridSnapXZ)
				{
					float num11 = mainTerrain.SampleHeight(new Vector3(num9, 0f, num10)) + y;
					float num12 = Mathf.Round(num11 / gridSnapY) * gridSnapY;
					for (int m = 0; m < 4; m++)
					{
						Quaternion val = array2[m];
						num7++;
						bool flag = true;
						float num13 = -30f;
						foreach (Vector3 item5 in refineryCheckData.AboveGroundLocal)
						{
							Vector3 val2 = new Vector3(num9, num12, num10) + val * item5;
							if (val2.x < position.x || val2.x > position.x + size.x || val2.z < position.z || val2.z > position.z + size.z)
							{
								flag = false;
								break;
							}
							float num14 = mainTerrain.SampleHeight(val2) + y - val2.y;
							if (num14 > num13)
							{
								num13 = num14;
							}
						}
						if (!flag)
						{
							continue;
						}
						float num15 = num13;
						float num16 = num12 + num15;
						foreach (Vector3 item6 in refineryCheckData.BelowGroundLocal)
						{
							Vector3 val3 = new Vector3(num9, num16, num10) + val * item6;
							if (val3.x < position.x || val3.x > position.x + size.x || val3.z < position.z || val3.z > position.z + size.z)
							{
								flag = false;
								break;
							}
							if (mainTerrain.SampleHeight(val3) + y < val3.y)
							{
								flag = false;
								break;
							}
						}
						if (!flag)
						{
							continue;
						}
						foreach (EntryCheckData previewEntry in refineryCheckData.PreviewEntries)
						{
							int num17 = 0;
							foreach (Vector3 item7 in previewEntry.AboveLocal)
							{
								Vector3 val4 = new Vector3(num9, num16, num10) + val * item7;
								if (val4.x >= position.x && val4.x <= position.x + size.x && val4.z >= position.z && val4.z <= position.z + size.z)
								{
									num17++;
								}
							}
							int num18 = 0;
							foreach (Vector3 item8 in previewEntry.BelowLocal)
							{
								Vector3 val5 = new Vector3(num9, num16, num10) + val * item8;
								if (val5.x >= position.x && val5.x <= position.x + size.x && val5.z >= position.z && val5.z <= position.z + size.z && mainTerrain.SampleHeight(val5) + y >= val5.y)
								{
									num18++;
								}
							}
							if (num17 < previewEntry.MustAboveCount || num18 < previewEntry.MustBelowCount)
							{
								flag = false;
								break;
							}
						}
						if (!flag)
						{
							continue;
						}
						for (int n = 0; n < refineryCheckData.Ramps.Count; n++)
						{
							RampCheckData rampCheckData2 = refineryCheckData.Ramps[n];
							Vector3 val6 = new Vector3(num9, num16, num10) + val * rampCheckData2.CheckPointLocal;
							bool flag2;
							if (val6.x < position.x || val6.x > position.x + size.x || val6.z < position.z || val6.z > position.z + size.z)
							{
								flag2 = rampCheckData2.IsAboveType;
							}
							else
							{
								float num19 = mainTerrain.SampleHeight(val6) + y;
								flag2 = (rampCheckData2.IsAboveType ? (num19 < val6.y) : (num19 >= val6.y));
							}
							if (flag2)
							{
								array[n]++;
							}
						}
						StringBuilder stringBuilder4 = new StringBuilder();
						foreach (int item9 in list)
						{
							RampCheckData rampCheckData3 = refineryCheckData.Ramps[item9];
							Vector3 val7 = new Vector3(num9, num16, num10) + val * rampCheckData3.CheckPointLocal;
							bool flag3;
							if (val7.x < position.x || val7.x > position.x + size.x || val7.z < position.z || val7.z > position.z + size.z)
							{
								flag3 = rampCheckData3.IsAboveType;
							}
							else
							{
								float num20 = mainTerrain.SampleHeight(val7) + y;
								flag3 = (rampCheckData3.IsAboveType ? (num20 < val7.y) : (num20 >= val7.y));
							}
							Vector3 val8 = new Vector3(num9, num16, num10) + val * rampCheckData3.EntryLocal;
							float num21 = val8.x - num9;
							float num22 = val8.z - num10;
							float num23 = Mathf.Sqrt(num21 * num21 + num22 * num22);
							if (num23 > 0.01f)
							{
								num21 /= num23;
								num22 /= num23;
							}
							stringBuilder4.Append($",{(flag3 ? 1 : 0)},{num21:F3},{num22:F3}");
						}
						num8++;
						stringBuilder2.AppendLine($"{num9:F0},{num10:F0},{array3[m]:F0},{num11:F1},{num15:F2},{num16:F1}{stringBuilder4}");
					}
				}
			}
			stopwatch.Stop();
			stringBuilder.AppendLine("=== Scan Results ===");
			stringBuilder.AppendLine($"Total checks: {num7}");
			stringBuilder.AppendLine($"Valid placements: {num8} ({((num7 > 0) ? (100.0 * (double)num8 / (double)num7) : 0.0):F1}%)");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Harvester ramps (with DepositPoint):");
			for (int num24 = 0; num24 < list.Count; num24++)
			{
				int num25 = list[num24];
				char c2 = (char)(65 + num24);
				RampCheckData rampCheckData4 = refineryCheckData.Ramps[num25];
				stringBuilder.AppendLine($"  Ramp {c2} (idx {num25}): {array[num25]} accessible ({((num8 > 0) ? (100.0 * (double)array[num25] / (double)num8) : 0.0):F1}% of valid), check=({rampCheckData4.CheckPointLocal.x:F1},{rampCheckData4.CheckPointLocal.y:F1},{rampCheckData4.CheckPointLocal.z:F1}), entry=({rampCheckData4.EntryLocal.x:F1},{rampCheckData4.EntryLocal.y:F1},{rampCheckData4.EntryLocal.z:F1})");
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("All terrain-dependent elements (for reference):");
			for (int num26 = 0; num26 < refineryCheckData.Ramps.Count; num26++)
			{
				string text2 = (refineryCheckData.Ramps[num26].HasDepositPoint ? " [HARVESTER]" : "");
				stringBuilder.AppendLine($"  Ramp {num26}: {array[num26]} accessible ({((num8 > 0) ? (100.0 * (double)array[num26] / (double)num8) : 0.0):F1}% of valid){text2}");
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine($"Scan time: {stopwatch.ElapsedMilliseconds}ms");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("=== CSV DATA (harvester ramps only) ===");
			string text3 = Path.Combine("UserData", "MapBalance");
			Directory.CreateDirectory(text3);
			string arg = GetCurrentMapName() ?? "unknown";
			string path = $"refinery_scan_{faction}_{arg}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
			string text4 = Path.Combine(text3, path);
			using (StreamWriter streamWriter = new StreamWriter(text4))
			{
				streamWriter.Write(stringBuilder.ToString());
				streamWriter.Write(stringBuilder2.ToString());
			}
			logger.Msg("=== Refinery scan complete ===");
			logger.Msg($"Valid: {num8}/{num7} ({((num7 > 0) ? (100.0 * (double)num8 / (double)num7) : 0.0):F1}%)");
			logger.Msg($"Scan time: {stopwatch.ElapsedMilliseconds}ms");
			logger.Msg("Output: " + text4);
		}

		private static void ScanHQPlacements()
		{
			Melon<MapBalance>.Logger.Msg("=== Starting HQ Placement Scan ===");
			(string, Func<string, bool>)[] array = new (string, Func<string, bool>)[2]
			{
				("Sol", (string n) => n.Contains("Sol") && n.Contains("Headquarters")),
				("Cent", (string n) => n.Contains("Cent") && n.Contains("Headquarters"))
			};
			for (int num = 0; num < array.Length; num++)
			{
				var (faction, nameFilter) = array[num];
				ScanSingleHQ(faction, nameFilter);
			}
		}

		private static void ScanSingleHQ(string faction, Func<string, bool> nameFilter)
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_028f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0298: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0313: Unknown result type (might be due to invalid IL or missing references)
			//IL_0318: Unknown result type (might be due to invalid IL or missing references)
			//IL_0321: Unknown result type (might be due to invalid IL or missing references)
			//IL_032d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0339: Unknown result type (might be due to invalid IL or missing references)
			//IL_0436: Unknown result type (might be due to invalid IL or missing references)
			//IL_0445: Unknown result type (might be due to invalid IL or missing references)
			//IL_0454: Unknown result type (might be due to invalid IL or missing references)
			//IL_0463: Unknown result type (might be due to invalid IL or missing references)
			//IL_0472: Unknown result type (might be due to invalid IL or missing references)
			//IL_0481: Unknown result type (might be due to invalid IL or missing references)
			//IL_0515: Unknown result type (might be due to invalid IL or missing references)
			//IL_051a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0530: Unknown result type (might be due to invalid IL or missing references)
			//IL_0535: Unknown result type (might be due to invalid IL or missing references)
			//IL_054b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0550: Unknown result type (might be due to invalid IL or missing references)
			//IL_0566: Unknown result type (might be due to invalid IL or missing references)
			//IL_056b: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_05fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0604: Unknown result type (might be due to invalid IL or missing references)
			//IL_0609: Unknown result type (might be due to invalid IL or missing references)
			//IL_060b: Unknown result type (might be due to invalid IL or missing references)
			//IL_060d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0612: Unknown result type (might be due to invalid IL or missing references)
			//IL_0617: Unknown result type (might be due to invalid IL or missing references)
			//IL_0619: Unknown result type (might be due to invalid IL or missing references)
			//IL_0620: Unknown result type (might be due to invalid IL or missing references)
			//IL_0629: Unknown result type (might be due to invalid IL or missing references)
			//IL_0630: Unknown result type (might be due to invalid IL or missing references)
			//IL_0637: Unknown result type (might be due to invalid IL or missing references)
			//IL_0641: Unknown result type (might be due to invalid IL or missing references)
			//IL_0648: Unknown result type (might be due to invalid IL or missing references)
			//IL_0651: Unknown result type (might be due to invalid IL or missing references)
			//IL_0658: Unknown result type (might be due to invalid IL or missing references)
			//IL_065f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0670: Unknown result type (might be due to invalid IL or missing references)
			//IL_067a: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_06dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_06e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_06e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_06e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_06eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0702: Unknown result type (might be due to invalid IL or missing references)
			//IL_0709: Unknown result type (might be due to invalid IL or missing references)
			//IL_0710: Unknown result type (might be due to invalid IL or missing references)
			//IL_071a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0721: Unknown result type (might be due to invalid IL or missing references)
			//IL_072a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0731: Unknown result type (might be due to invalid IL or missing references)
			//IL_0738: Unknown result type (might be due to invalid IL or missing references)
			//IL_0749: Unknown result type (might be due to invalid IL or missing references)
			//IL_0753: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_07c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_07c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_07c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_07cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_07d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_07d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_07dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_07e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_07f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_07fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0805: Unknown result type (might be due to invalid IL or missing references)
			//IL_080e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0815: Unknown result type (might be due to invalid IL or missing references)
			//IL_081c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0860: Unknown result type (might be due to invalid IL or missing references)
			//IL_0865: Unknown result type (might be due to invalid IL or missing references)
			//IL_086d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0872: Unknown result type (might be due to invalid IL or missing references)
			//IL_0874: Unknown result type (might be due to invalid IL or missing references)
			//IL_0876: Unknown result type (might be due to invalid IL or missing references)
			//IL_087b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0880: Unknown result type (might be due to invalid IL or missing references)
			//IL_0882: Unknown result type (might be due to invalid IL or missing references)
			//IL_0889: Unknown result type (might be due to invalid IL or missing references)
			//IL_0892: Unknown result type (might be due to invalid IL or missing references)
			//IL_0899: Unknown result type (might be due to invalid IL or missing references)
			//IL_08a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_08aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_08b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_08ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_08c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_08c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_08d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_08de: Unknown result type (might be due to invalid IL or missing references)
			Instance logger = Melon<MapBalance>.Logger;
			Stopwatch stopwatch = Stopwatch.StartNew();
			string text = faction + "_Headquarters";
			RefineryCheckData refineryCheckData = ExtractBuildingData(nameFilter, text);
			if (refineryCheckData == null)
			{
				logger.Error("Failed to extract " + text + " data");
				return;
			}
			Terrain mainTerrain = GetMainTerrain();
			if ((Object)(object)mainTerrain == (Object)null)
			{
				logger.Error("No terrain found for HQ scan");
				return;
			}
			float y = ((Component)mainTerrain).transform.position.y;
			Vector3 position = ((Component)mainTerrain).transform.position;
			Vector3 size = mainTerrain.terrainData.size;
			float gridSnapXZ = refineryCheckData.GridSnapXZ;
			float gridSnapY = refineryCheckData.GridSnapY;
			float num = Mathf.Ceil(position.x / gridSnapXZ) * gridSnapXZ;
			float num2 = Mathf.Ceil(position.z / gridSnapXZ) * gridSnapXZ;
			float num3 = Mathf.Floor((position.x + size.x) / gridSnapXZ) * gridSnapXZ;
			float num4 = Mathf.Floor((position.z + size.z) / gridSnapXZ) * gridSnapXZ;
			int num5 = Mathf.RoundToInt((num3 - num) / gridSnapXZ) + 1;
			int num6 = Mathf.RoundToInt((num4 - num2) / gridSnapXZ) + 1;
			logger.Msg($"[{text}] Scan: X=[{num:F0},{num3:F0}] Z=[{num2:F0},{num4:F0}] steps={num5}x{num6} x4rot = {num5 * num6 * 4} checks");
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder.AppendLine("=== HQ Placement Scan v1.0 ===");
			stringBuilder.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
			stringBuilder.AppendLine("Map: " + (GetCurrentMapName() ?? "unknown"));
			stringBuilder.AppendLine("Faction: " + faction);
			stringBuilder.AppendLine("Asset: " + refineryCheckData.Name);
			stringBuilder.AppendLine($"GridSnapXZ: {refineryCheckData.GridSnapXZ}, GridSnapY: {refineryCheckData.GridSnapY}");
			stringBuilder.AppendLine($"RotateToSurface: {refineryCheckData.RotateToSurface}, MaxAngle: {refineryCheckData.MaxAngle}");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine($"--- AboveGroundPoints ({refineryCheckData.AboveGroundLocal.Count}) ---");
			foreach (Vector3 item in refineryCheckData.AboveGroundLocal)
			{
				stringBuilder.AppendLine($"  ({item.x:F2}, {item.y:F2}, {item.z:F2})");
			}
			stringBuilder.AppendLine($"--- BelowGroundPoints ({refineryCheckData.BelowGroundLocal.Count}) ---");
			foreach (Vector3 item2 in refineryCheckData.BelowGroundLocal)
			{
				stringBuilder.AppendLine($"  ({item2.x:F2}, {item2.y:F2}, {item2.z:F2})");
			}
			stringBuilder.AppendLine($"--- PreviewEntries ({refineryCheckData.PreviewEntries.Count}) ---");
			for (int i = 0; i < refineryCheckData.PreviewEntries.Count; i++)
			{
				EntryCheckData entryCheckData = refineryCheckData.PreviewEntries[i];
				stringBuilder.AppendLine($"  [{i}] above={entryCheckData.AboveLocal.Count}(need>={entryCheckData.MustAboveCount}), below={entryCheckData.BelowLocal.Count}(need>={entryCheckData.MustBelowCount})");
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine($"Terrain: ({position.x:F0},{position.y:F0},{position.z:F0}) size ({size.x:F0},{size.y:F0},{size.z:F0})");
			stringBuilder.AppendLine($"Scan: X=[{num:F0},{num3:F0}] Z=[{num2:F0},{num4:F0}] step={gridSnapXZ}");
			stringBuilder.AppendLine();
			stringBuilder2.AppendLine("x,z,rot,terrainY,adjustY,finalY");
			int num7 = 0;
			int num8 = 0;
			Quaternion[] array = new Quaternion[4]
			{
				Quaternion.Euler(0f, 0f, 0f),
				Quaternion.Euler(0f, 90f, 0f),
				Quaternion.Euler(0f, 180f, 0f),
				Quaternion.Euler(0f, 270f, 0f)
			};
			float[] array2 = new float[4] { 0f, 90f, 180f, 270f };
			for (float num9 = num; num9 <= num3; num9 += gridSnapXZ)
			{
				for (float num10 = num2; num10 <= num4; num10 += gridSnapXZ)
				{
					float num11 = mainTerrain.SampleHeight(new Vector3(num9, 0f, num10)) + y;
					float num12 = Mathf.Round(num11 / gridSnapY) * gridSnapY;
					for (int j = 0; j < 4; j++)
					{
						Quaternion val = array[j];
						num7++;
						bool flag = true;
						float num13 = -30f;
						foreach (Vector3 item3 in refineryCheckData.AboveGroundLocal)
						{
							Vector3 val2 = new Vector3(num9, num12, num10) + val * item3;
							if (val2.x < position.x || val2.x > position.x + size.x || val2.z < position.z || val2.z > position.z + size.z)
							{
								flag = false;
								break;
							}
							float num14 = mainTerrain.SampleHeight(val2) + y - val2.y;
							if (num14 > num13)
							{
								num13 = num14;
							}
						}
						if (!flag)
						{
							continue;
						}
						float num15 = num13;
						float num16 = num12 + num15;
						foreach (Vector3 item4 in refineryCheckData.BelowGroundLocal)
						{
							Vector3 val3 = new Vector3(num9, num16, num10) + val * item4;
							if (val3.x < position.x || val3.x > position.x + size.x || val3.z < position.z || val3.z > position.z + size.z)
							{
								flag = false;
								break;
							}
							if (mainTerrain.SampleHeight(val3) + y < val3.y)
							{
								flag = false;
								break;
							}
						}
						if (!flag)
						{
							continue;
						}
						foreach (EntryCheckData previewEntry in refineryCheckData.PreviewEntries)
						{
							int num17 = 0;
							foreach (Vector3 item5 in previewEntry.AboveLocal)
							{
								Vector3 val4 = new Vector3(num9, num16, num10) + val * item5;
								if (val4.x >= position.x && val4.x <= position.x + size.x && val4.z >= position.z && val4.z <= position.z + size.z)
								{
									num17++;
								}
							}
							int num18 = 0;
							foreach (Vector3 item6 in previewEntry.BelowLocal)
							{
								Vector3 val5 = new Vector3(num9, num16, num10) + val * item6;
								if (val5.x >= position.x && val5.x <= position.x + size.x && val5.z >= position.z && val5.z <= position.z + size.z && mainTerrain.SampleHeight(val5) + y >= val5.y)
								{
									num18++;
								}
							}
							if (num17 < previewEntry.MustAboveCount || num18 < previewEntry.MustBelowCount)
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							num8++;
							stringBuilder2.AppendLine($"{num9:F0},{num10:F0},{array2[j]:F0},{num11:F1},{num15:F2},{num16:F1}");
						}
					}
				}
			}
			stopwatch.Stop();
			stringBuilder.AppendLine("=== Scan Results ===");
			stringBuilder.AppendLine($"Total checks: {num7}");
			stringBuilder.AppendLine($"Valid placements: {num8} ({((num7 > 0) ? (100.0 * (double)num8 / (double)num7) : 0.0):F1}%)");
			stringBuilder.AppendLine($"Scan time: {stopwatch.ElapsedMilliseconds}ms");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("=== CSV DATA ===");
			string text2 = Path.Combine("UserData", "MapBalance");
			Directory.CreateDirectory(text2);
			string arg = GetCurrentMapName() ?? "unknown";
			string path = $"hq_scan_{faction}_{arg}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
			string text3 = Path.Combine(text2, path);
			using (StreamWriter streamWriter = new StreamWriter(text3))
			{
				streamWriter.Write(stringBuilder.ToString());
				streamWriter.Write(stringBuilder2.ToString());
			}
			logger.Msg("=== " + text + " scan complete ===");
			logger.Msg($"Valid: {num8}/{num7} ({((num7 > 0) ? (100.0 * (double)num8 / (double)num7) : 0.0):F1}%)");
			logger.Msg($"Scan time: {stopwatch.ElapsedMilliseconds}ms");
			logger.Msg("Output: " + text3);
		}

		private static List<Component> GetAllResourceAreas()
		{
			List<Component> list = new List<Component>();
			object obj = _allAreasField?.GetValue(null) ?? _allAreasProp?.GetValue(null);
			if (obj == null)
			{
				return list;
			}
			int num = (int)(obj.GetType().GetProperty("Count")?.GetValue(obj) ?? ((object)0));
			PropertyInfo property = obj.GetType().GetProperty("Item");
			for (int i = 0; i < num; i++)
			{
				object? obj2 = property?.GetValue(obj, new object[1] { i });
				Component val = (Component)((obj2 is Component) ? obj2 : null);
				if ((Object)(object)val != (Object)null)
				{
					list.Add(val);
				}
			}
			return list;
		}

		private static int CountEnabledAreas()
		{
			object obj = _enabledAreasField?.GetValue(null) ?? _enabledAreasProp?.GetValue(null);
			if (obj == null)
			{
				return 0;
			}
			return (int)(obj.GetType().GetProperty("Count")?.GetValue(obj) ?? ((object)0));
		}

		private static void SetEnabled(Component area, bool enabled)
		{
			Behaviour val = (Behaviour)(object)((area is Behaviour) ? area : null);
			if (val != null)
			{
				val.enabled = enabled;
			}
		}

		private static void CallDistributeResources(Component area)
		{
			if (_distributeResourcesMethod == null)
			{
				return;
			}
			try
			{
				if (_distributeResourcesMethod.GetParameters().Length == 0)
				{
					_distributeResourcesMethod.Invoke(area, null);
					return;
				}
				_distributeResourcesMethod.Invoke(area, new object[1] { false });
			}
			catch (Exception ex)
			{
				Melon<MapBalance>.Logger.Warning("DistributeResources failed: " + ex.Message);
			}
		}

		private static void SetMemberValue(object obj, string name, float value)
		{
			Type type = obj.GetType();
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				field.SetValue(obj, value);
				return;
			}
			PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.CanWrite)
			{
				property.SetValue(obj, value);
			}
		}

		private static int GetMemberValueInt(object obj, string name)
		{
			if (obj == null)
			{
				return 0;
			}
			Type type = obj.GetType();
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				object value = field.GetValue(obj);
				if (value is int)
				{
					return (int)value;
				}
			}
			PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.CanRead)
			{
				object value2 = property.GetValue(obj);
				if (value2 is int)
				{
					return (int)value2;
				}
			}
			return 0;
		}

		private static void SetMemberValueInt(object obj, string name, int value)
		{
			Type type = obj.GetType();
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				field.SetValue(obj, value);
				return;
			}
			PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.CanWrite)
			{
				property.SetValue(obj, value);
			}
		}

		private static void DumpMapInfo()
		{
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0281: Unknown result type (might be due to invalid IL or missing references)
			//IL_0286: Unknown result type (might be due to invalid IL or missing references)
			//IL_0398: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
			if (_dumpedThisRound)
			{
				return;
			}
			_dumpedThisRound = true;
			Instance logger = Melon<MapBalance>.Logger;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("=== Si_MapBalance Dump v1.0 ===");
			stringBuilder.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
			string text = GetCurrentMapName() ?? "unknown";
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("--- TERRAIN ---");
			stringBuilder.AppendLine("Map: " + text);
			Terrain mainTerrain = GetMainTerrain();
			if ((Object)(object)mainTerrain != (Object)null)
			{
				Vector3 position = ((Component)mainTerrain).transform.position;
				Vector3 size = mainTerrain.terrainData.size;
				stringBuilder.AppendLine($"Terrain Position: ({position.x:F1}, {position.y:F1}, {position.z:F1})");
				stringBuilder.AppendLine($"Terrain Size: ({size.x:F1}, {size.y:F1}, {size.z:F1})");
				stringBuilder.AppendLine($"Heightmap Resolution: {mainTerrain.terrainData.heightmapResolution}");
				float num = float.MaxValue;
				float num2 = float.MinValue;
				int num3 = 21;
				for (int i = 0; i < num3; i++)
				{
					for (int j = 0; j < num3; j++)
					{
						float num4 = position.x + size.x * (float)i / (float)(num3 - 1);
						float num5 = position.z + size.z * (float)j / (float)(num3 - 1);
						float num6 = mainTerrain.SampleHeight(new Vector3(num4, 0f, num5)) + position.y;
						if (num6 < num)
						{
							num = num6;
						}
						if (num6 > num2)
						{
							num2 = num6;
						}
					}
				}
				stringBuilder.AppendLine($"Elevation Range: {num:F1} to {num2:F1} (sampled {num3 * num3} points)");
			}
			else
			{
				stringBuilder.AppendLine("Terrain: not found");
			}
			stringBuilder.AppendLine();
			List<Component> allResourceAreas = GetAllResourceAreas();
			int num7 = CountEnabledAreas();
			stringBuilder.AppendLine("--- RESOURCE AREAS ---");
			stringBuilder.AppendLine($"Total ResourceAreas: {allResourceAreas.Count} (Enabled: {num7})");
			stringBuilder.AppendLine();
			for (int k = 0; k < allResourceAreas.Count; k++)
			{
				Component val = allResourceAreas[k];
				Behaviour val2 = (Behaviour)(object)((val is Behaviour) ? val : null);
				bool flag = val2 != null && val2.enabled;
				Vector3 position2 = val.transform.position;
				string resourceName = GetResourceName(val);
				string name = ((Object)val.gameObject).name;
				int memberValue = GetMemberValue<int>(val, "ResourceAmountCurrent");
				int memberValue2 = GetMemberValue<int>(val, "ResourceAmountMax");
				float memberValue3 = GetMemberValue<float>(val, "ResourceDistributionAmountMult");
				float memberValue4 = GetMemberValue<float>(val, "ResourceDistributionDistanceMult");
				int memberValue5 = GetMemberValue<int>(val, "MaxResourceAmountPerCell");
				float memberValue6 = GetMemberValue<float>(val, "CellSize");
				int num8 = 0;
				int num9 = 0;
				FieldInfo? field = _resourceAreaType.GetField("ResourceAmounts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				PropertyInfo property = _resourceAreaType.GetProperty("ResourceAmounts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if ((field?.GetValue(val) ?? property?.GetValue(val)) is int[,] array)
				{
					num8 = array.GetLength(0);
					num9 = array.GetLength(1);
				}
				stringBuilder.AppendLine($"[{k}] \"{name}\" ({resourceName})");
				stringBuilder.AppendLine($"     Active: {flag}");
				stringBuilder.AppendLine($"     Position: ({position2.x:F1}, {position2.y:F1}, {position2.z:F1})");
				if (num8 > 0 && memberValue6 > 0f)
				{
					stringBuilder.AppendLine($"     Grid: {num8}x{num9} cells, CellSize={memberValue6:F1}m");
					stringBuilder.AppendLine($"     World extent: {(float)num8 * memberValue6:F0}x{(float)num9 * memberValue6:F0}m");
				}
				stringBuilder.AppendLine($"     MaxPerCell: {memberValue5}, AmountMult: {memberValue3:F2}, DistMult: {memberValue4:F2}");
				stringBuilder.AppendLine($"     Resources: {memberValue} / {memberValue2}");
				stringBuilder.AppendLine();
			}
			string text2 = Path.Combine("UserData", "MapBalance");
			Directory.CreateDirectory(text2);
			string text3 = Path.Combine(text2, $"dump_{text}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
			File.WriteAllText(text3, stringBuilder.ToString());
			logger.Msg($"Dump: {text3} ({allResourceAreas.Count} areas)");
		}

		private static void DumpConstructionData()
		{
			Instance logger = Melon<MapBalance>.Logger;
			if (_constructionDataType == null)
			{
				return;
			}
			Object[] array = Resources.FindObjectsOfTypeAll(_constructionDataType);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("=== ConstructionData Dump ===");
			stringBuilder.AppendLine($"Total: {array.Length}");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(string.Format("{0,-50} {1,8} {2,8} {3,7} {4,10} {5,6} {6,7}", "Name", "MaxDist", "MinDist", "Anchor", "Placeable", "Cost", "GridXZ"));
			stringBuilder.AppendLine(new string('-', 100));
			Object[] array2 = array;
			foreach (Object val in array2)
			{
				Object val2 = val;
				if (!(val2 == (Object)null))
				{
					float memberValue = GetMemberValue<float>(val, "MaximumBaseStructureDistance");
					float memberValue2 = GetMemberValue<float>(val, "MinimumBaseStructureDistance");
					bool memberValue3 = GetMemberValue<bool>(val, "Placeable");
					int memberValue4 = GetMemberValue<int>(val, "ResourceCost");
					float memberValue5 = GetMemberValue<float>(val, "GridSnapXZ");
					bool flag = false;
					object rawMember = GetRawMember(val, "ObjectInfo");
					if (rawMember != null)
					{
						flag = GetMemberValue<bool>(rawMember, "StructureConstructionAnchor");
					}
					stringBuilder.AppendLine($"{val2.name,-50} {memberValue,8:F0} {memberValue2,8:F0} {flag,7} {memberValue3,10} {memberValue4,6} {memberValue5,7:F0}");
				}
			}
			string text = Path.Combine("UserData", "MapBalance");
			Directory.CreateDirectory(text);
			string arg = GetCurrentMapName() ?? "unknown";
			string text2 = Path.Combine(text, $"construction_data_{arg}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
			File.WriteAllText(text2, stringBuilder.ToString());
			logger.Msg("ConstructionData dump: " + text2);
		}

		private static string GetResourceName(object area)
		{
			object obj = (_resourceAreaType?.GetField("ResourceType", BindingFlags.Instance | BindingFlags.Public))?.GetValue(area);
			if (obj == null)
			{
				obj = (_resourceAreaType?.GetProperty("ResourceType", BindingFlags.Instance | BindingFlags.Public))?.GetValue(area);
			}
			if (obj != null)
			{
				return GetMemberValue<string>(obj, "ResourceName") ?? "?";
			}
			return "?";
		}

		private static bool IsPositionSafeForSwap(float worldX, float worldZ, float checkRadius, float maxHeightDiff, Instance log)
		{
			float num = SampleTerrainHeightQuiet(worldX, worldZ);
			if (float.IsNaN(num))
			{
				return true;
			}
			float num2 = num;
			float num3 = num;
			for (int i = 0; i < 8; i++)
			{
				float num4 = (float)i * 45f * (MathF.PI / 180f);
				float worldX2 = worldX + Mathf.Cos(num4) * checkRadius;
				float worldZ2 = worldZ + Mathf.Sin(num4) * checkRadius;
				float num5 = SampleTerrainHeightQuiet(worldX2, worldZ2);
				if (!float.IsNaN(num5))
				{
					if (num5 < num2)
					{
						num2 = num5;
					}
					if (num5 > num3)
					{
						num3 = num5;
					}
				}
			}
			float num6 = num3 - num2;
			bool flag = num6 <= maxHeightDiff;
			if (!flag)
			{
				log.Msg($"  Swap check FAILED at ({worldX:F0},{worldZ:F0}): height range {num6:F1}m (max allowed {maxHeightDiff:F0}m)");
			}
			return flag;
		}

		private static float SampleTerrainHeightQuiet(float worldX, float worldZ)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			Terrain[] activeTerrains = Terrain.activeTerrains;
			foreach (Terrain val in activeTerrains)
			{
				Vector3 position = ((Component)val).transform.position;
				Vector3 size = val.terrainData.size;
				if (worldX >= position.x && worldX <= position.x + size.x && worldZ >= position.z && worldZ <= position.z + size.z)
				{
					return val.SampleHeight(new Vector3(worldX, 0f, worldZ)) + position.y;
				}
			}
			Terrain activeTerrain = Terrain.activeTerrain;
			if ((Object)(object)activeTerrain != (Object)null)
			{
				return activeTerrain.SampleHeight(new Vector3(worldX, 0f, worldZ)) + ((Component)activeTerrain).transform.position.y;
			}
			return float.NaN;
		}

		private static float SampleTerrainHeight(float worldX, float worldZ, Instance log)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			Terrain val = null;
			Terrain[] activeTerrains = Terrain.activeTerrains;
			foreach (Terrain val2 in activeTerrains)
			{
				Vector3 position = ((Component)val2).transform.position;
				Vector3 size = val2.terrainData.size;
				if (worldX >= position.x && worldX <= position.x + size.x && worldZ >= position.z && worldZ <= position.z + size.z)
				{
					val = val2;
					break;
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				val = Terrain.activeTerrain;
			}
			if ((Object)(object)val != (Object)null)
			{
				float num = val.SampleHeight(new Vector3(worldX, 0f, worldZ)) + ((Component)val).transform.position.y;
				log.Msg($"  TerrainHeight at ({worldX:F0},{worldZ:F0}): {num:F1} (terrain: {((Object)val).name})");
				return num;
			}
			log.Warning($"  No terrain found for ({worldX:F0},{worldZ:F0}) — using Y=0");
			return 0f;
		}

		private static float SampleSurfaceHeight(float worldX, float worldZ, Instance log)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			float num = SampleTerrainHeight(worldX, worldZ, log);
			float num2 = num + 200f;
			RaycastHit val = default;
			if (Physics.Raycast(new Vector3(worldX, num2, worldZ), Vector3.down, ref val, 400f, -1, (QueryTriggerInteraction)1))
			{
				float num3 = val.point.y - num;
				string text = (((Object)(object)val.collider != (Object)null && (Object)(object)((Component)val.collider).gameObject != (Object)null) ? ((Object)((Component)val.collider).gameObject).name : "?");
				log.Msg($"  SurfaceHeight at ({worldX:F0},{worldZ:F0}): {val.point.y:F1} (collider: {text}, terrain={num:F1}, Δ={num3:+0.0;-0.0;0})");
				return val.point.y;
			}
			log.Msg($"  SurfaceHeight at ({worldX:F0},{worldZ:F0}): raycast missed, using terrain Y={num:F1}");
			return num;
		}

		private static Terrain? GetMainTerrain()
		{
			try
			{
				object? obj = (_gameType?.GetProperty("MainTerrain", BindingFlags.Static | BindingFlags.Public))?.GetValue(null);
				return (Terrain?)((obj is Terrain) ? obj : null);
			}
			catch
			{
				return null;
			}
		}

		private static string? GetCurrentMapName()
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				PropertyInfo propertyInfo = _gameType?.GetProperty("ServerMapName", BindingFlags.Static | BindingFlags.Public);
				if (propertyInfo != null)
				{
					return propertyInfo.GetValue(null) as string;
				}
				FieldInfo fieldInfo = _gameType?.GetField("ServerMapName", BindingFlags.Static | BindingFlags.Public);
				if (fieldInfo != null)
				{
					return fieldInfo.GetValue(null) as string;
				}
				Scene activeScene = SceneManager.GetActiveScene();
				return activeScene.name;
			}
			catch
			{
				return null;
			}
		}

		private static T GetMemberValue<T>(object obj, string name)
		{
			if (obj == null)
			{
				return default;
			}
			Type type = obj.GetType();
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				object value = field.GetValue(obj);
				if (value is T)
				{
					return (T)value;
				}
			}
			PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.CanRead)
			{
				object value2 = property.GetValue(obj);
				if (value2 is T)
				{
					return (T)value2;
				}
			}
			return default;
		}

		private static object? GetRawMember(object obj, string name)
		{
			if (obj == null)
			{
				return null;
			}
			Type type = obj.GetType();
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				return field.GetValue(obj);
			}
			PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.CanRead)
			{
				return property.GetValue(obj);
			}
			return null;
		}

		private static List<Vector3> ExtractTransformLocalPositions(object obj, string fieldName)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			List<Vector3> list = new List<Vector3>();
			if (GetRawMember(obj, fieldName) is IList list2)
			{
				foreach (object item in list2)
				{
					Transform val = (Transform)((item is Transform) ? item : null);
					if (val != null)
					{
						list.Add(val.localPosition);
					}
				}
			}
			return list;
		}

		private static void SpawnSpecials(MapConfig config, Instance log)
		{
			MapBalanceSpecials.Clear();
			int num = _spawnPrefabMethod?.GetParameters().Length ?? 0;
			if (num == 0)
			{
				log.Warning("[Specials] Game.SpawnPrefab not resolved — skipping KoH/FusionReactor spawn");
				return;
			}
			if (config.Koh != null && !IsKohModeActive())
			{
				log.Msg("[Specials] KGT KoH mode inactive — skipping KoH tower from layout");
			}
			else if (config.Koh != null)
			{
				object team = FindNeutralTeam(log);
				GameObject val = SpawnSpecialAt(config.Koh.PrefabName, config.Koh.X, config.Koh.Z, num, log, "KoH", team);
				if ((Object)(object)val != (Object)null)
				{
					MapBalanceSpecials.Koh = new MapBalanceSpecials.KohEntry
					{
						Obj = val,
						CaptureRadius = config.Koh.CaptureRadius,
						ExclusionRadius = config.Koh.ExclusionRadius
					};
					log.Msg($"[Specials] KoH spawned: {((Object)val).name} at ({config.Koh.X:F0},{config.Koh.Z:F0}) " + $"R_capture={config.Koh.CaptureRadius} R_excl={config.Koh.ExclusionRadius}");
				}
			}
			if (config.FusionReactors != null)
			{
				int num2 = 0;
				foreach (FusionReactorEntry fusionReactor in config.FusionReactors)
				{
					GameObject val2 = SpawnSpecialAt(fusionReactor.PrefabName, fusionReactor.X, fusionReactor.Z, num, log, $"FR#{num2}");
					if ((Object)(object)val2 != (Object)null)
					{
						MapBalanceSpecials.FusionReactors.Add(new MapBalanceSpecials.FusionReactorEntry
						{
							Obj = val2,
							CaptureRadius = fusionReactor.CaptureRadius
						});
						log.Msg($"[Specials] FusionReactor#{num2} spawned: {((Object)val2).name} at ({fusionReactor.X:F0},{fusionReactor.Z:F0}) R_capture={fusionReactor.CaptureRadius}");
					}
					num2++;
				}
			}
			try
			{
				MapBalanceSpecials.RaiseReady();
			}
			catch (Exception ex)
			{
				log.Warning("[Specials] OnSpecialsReady subscriber threw: " + ex.Message);
			}
		}

		private static GameObject? SpawnSpecialAt(string prefabName, float tx, float tz, int paramCount, Instance log, string label, object? team = null)
		{
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = GetResourcePrefab(prefabName, log);
			if ((Object)(object)val == (Object)null)
			{
				val = FindPrefabByName(prefabName, log);
			}
			if ((Object)(object)val == (Object)null)
			{
				log.Warning("[Specials/" + label + "] prefab '" + prefabName + "' not found — cannot spawn");
				return null;
			}
			float num = SampleSurfaceHeight(tx, tz, log);
			Vector3 val2 = new Vector3(tx, num, tz);
			Quaternion identity = Quaternion.identity;
			GameObject val3 = null;
			try
			{
				object[] parameters = ((paramCount < 7) ? new object[5] { val, null, team, val2, identity } : new object[7] { val, null, team, val2, identity, true, true });
				object? obj = _spawnPrefabMethod.Invoke(null, parameters);
				val3 = (GameObject)((obj is GameObject) ? obj : null);
			}
			catch (Exception ex)
			{
				log.Warning($"[Specials/{label}] SpawnPrefab threw at ({tx:F0},{tz:F0}): {ex.Message} — trying direct Instantiate");
			}
			if ((Object)(object)val3 != (Object)null)
			{
				log.Msg("[Specials/" + label + "] networked spawn OK (clients should see).");
				return val3;
			}
			try
			{
				val3 = Object.Instantiate<GameObject>(val, val2, identity);
				log.Msg($"[Specials/{label}] direct Instantiate at ({tx:F0},{tz:F0}) — server-only marker (clients won't see tower).");
				return val3;
			}
			catch (Exception ex2)
			{
				log.Error($"[Specials/{label}] Instantiate threw at ({tx:F0},{tz:F0}): {ex2.Message}");
				return null;
			}
		}

		private static bool ContainsCI(string haystack, string needle)
		{
			if (!string.IsNullOrEmpty(haystack))
			{
				return haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
			}
			return false;
		}

		private static object? FindNeutralTeam(Instance log)
		{
			try
			{
				if (_teamType == null)
				{
					log.Warning("[Specials/KoH] Team type not resolved — cannot find neutral team");
					return null;
				}
				PropertyInfo? property = _teamType.GetProperty("Teams", BindingFlags.Static | BindingFlags.Public);
				FieldInfo field = _teamType.GetField("Teams", BindingFlags.Static | BindingFlags.Public);
				if (!((property?.GetValue(null) ?? field?.GetValue(null)) is IEnumerable enumerable))
				{
					return null;
				}
				FieldInfo field2 = _teamType.GetField("TeamShortName", BindingFlags.Instance | BindingFlags.Public);
				FieldInfo field3 = _teamType.GetField("TeamName", BindingFlags.Instance | BindingFlags.Public);
				object obj = null;
				object obj2 = null;
				int num = 0;
				foreach (object item in enumerable)
				{
					if (item == null)
					{
						log.Msg($"[Specials/KoH] Team[{num++}] = <null>");
						continue;
					}
					string text = (field2?.GetValue(item) as string) ?? "";
					string text2 = (field3?.GetValue(item) as string) ?? "";
					object obj3 = ((item is Component) ? item : null);
					string text3 = ((obj3 != null) ? ((Object)obj3).name : null) ?? "";
					log.Msg($"[Specials/KoH] Team[{num++}]: short='{text}', name='{text2}', go='{text3}'");
					bool num2 = ContainsCI(text, "Master") || ContainsCI(text2, "Master") || ContainsCI(text3, "Master");
					bool flag = ContainsCI(text, "Worm") || ContainsCI(text, "Wildlife") || ContainsCI(text2, "Worm") || ContainsCI(text2, "Wildlife") || ContainsCI(text3, "Worm") || ContainsCI(text3, "Wildlife");
					if (num2 && obj == null)
					{
						obj = item;
					}
					else if (flag && obj2 == null)
					{
						obj2 = item;
					}
				}
				if (obj2 != null)
				{
					log.Msg("[Specials/KoH] Picked: Wildlife");
					return obj2;
				}
				if (obj != null)
				{
					log.Msg("[Specials/KoH] Picked: Gamemaster (fallback)");
					return obj;
				}
				log.Warning("[Specials/KoH] No wildlife/gamemaster team found in Teams collection");
				return null;
			}
			catch (Exception ex)
			{
				log.Warning("[Specials/KoH] FindNeutralTeam threw: " + ex.Message);
				return null;
			}
		}

		private static void LogAvailableTeams(Instance log)
		{
			if (_teamsLogged || _teamType == null)
			{
				return;
			}
			try
			{
				string[] array = new string[3] { "Teams", "AllTeams", "TeamsList" };
				foreach (string text in array)
				{
					PropertyInfo? property = _teamType.GetProperty(text, BindingFlags.Static | BindingFlags.Public);
					FieldInfo field = _teamType.GetField(text, BindingFlags.Static | BindingFlags.Public);
					if (!((property?.GetValue(null) ?? field?.GetValue(null)) is IEnumerable enumerable))
					{
						continue;
					}
					List<string> list = new List<string>();
					foreach (object item in enumerable)
					{
						if (item == null)
						{
							list.Add("<null>");
							continue;
						}
						object obj = item.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public)?.GetValue(item)?.ToString();
						if (obj == null)
						{
							object obj2 = ((item is Component) ? item : null);
							obj = ((obj2 != null) ? ((Object)obj2).name : null) ?? item.ToString();
						}
						string text2 = (string)obj;
						list.Add(text2 ?? "?");
					}
					log.Msg("[Specials/KoH] Teams (via " + text + "): [" + string.Join(", ", list) + "]");
					_teamsLogged = true;
					return;
				}
				log.Msg("[Specials/KoH] Could not enumerate Teams list (no Teams/AllTeams/TeamsList static)");
				_teamsLogged = true;
			}
			catch (Exception ex)
			{
				log.Warning("[Specials/KoH] LogAvailableTeams threw: " + ex.Message);
				_teamsLogged = true;
			}
		}

		private static void TryMakeNeutral(GameObject go, Instance log)
		{
			try
			{
				if (_teamType == null)
				{
					log.Warning("[Specials/KoH] Team type not resolved — leaving default team");
					return;
				}
				Type type = _teamType.Assembly.GetType("BaseGameObject");
				if (type == null)
				{
					log.Warning("[Specials/KoH] BaseGameObject type not found — leaving default team");
					return;
				}
				Component val = go.GetComponent(type);
				if ((Object)(object)val == (Object)null)
				{
					val = go.GetComponentInChildren(type, true);
				}
				if ((Object)(object)val == (Object)null)
				{
					log.Warning("[Specials/KoH] BaseGameObject not on building — leaving default team");
					return;
				}
				PropertyInfo property = type.GetProperty("Team", BindingFlags.Instance | BindingFlags.Public);
				if (property == null)
				{
					log.Warning("[Specials/KoH] BaseGameObject.Team property not found");
					return;
				}
				_teamType.GetMethod("GetTeamByName", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(string) }, null);
				LogAvailableTeams(log);
				object obj = null;
				string text = "";
				PropertyInfo? property2 = _teamType.GetProperty("Teams", BindingFlags.Static | BindingFlags.Public);
				FieldInfo field = _teamType.GetField("Teams", BindingFlags.Static | BindingFlags.Public);
				object? obj2 = property2?.GetValue(null) ?? field?.GetValue(null);
				FieldInfo field2 = _teamType.GetField("TeamShortName", BindingFlags.Instance | BindingFlags.Public);
				FieldInfo field3 = _teamType.GetField("TeamName", BindingFlags.Instance | BindingFlags.Public);
				if (obj2 is IEnumerable enumerable && field2 != null)
				{
					foreach (object item in enumerable)
					{
						if (item != null)
						{
							string text2 = (field2.GetValue(item) as string) ?? "";
							string text3 = (field3?.GetValue(item) as string) ?? "";
							object obj3 = ((item is Component) ? item : null);
							string text4 = ((obj3 != null) ? ((Object)obj3).name : null) ?? "";
							log.Msg("[Specials/KoH] Team candidate: short='" + text2 + "', name='" + text3 + "', go='" + text4 + "'");
							bool num = ContainsCI(text2, "Master") || ContainsCI(text4, "Master");
							bool flag = ContainsCI(text2, "Worm") || ContainsCI(text2, "Wildlife") || ContainsCI(text4, "Worm") || ContainsCI(text4, "Wildlife");
							if (num && obj == null)
							{
								obj = item;
								text = text2 + "|" + text4;
							}
							else if (flag && obj == null)
							{
								obj = item;
								text = text2 + "|" + text4;
							}
						}
					}
				}
				if (obj != null)
				{
					try
					{
						property.SetValue(val, obj);
						log.Msg("[Specials/KoH] Team set to " + text + " (via Teams iteration)");
						return;
					}
					catch (Exception ex)
					{
						log.Warning("[Specials/KoH] Team set via iteration failed: " + ex.Message);
					}
				}
				else
				{
					log.Warning("[Specials/KoH] No neutral team found in Teams collection");
				}
				try
				{
					property.SetValue(val, null);
					log.Msg("[Specials/KoH] Team set to null (last-resort)");
				}
				catch (Exception ex2)
				{
					log.Warning("[Specials/KoH] All team attempts failed: " + ex2.Message);
				}
			}
			catch (Exception ex3)
			{
				log.Warning("[Specials/KoH] TryMakeNeutral threw: " + ex3.Message);
			}
		}

		private static bool IsKohModeActive()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Type type = assemblies[i].GetType("Si_KingOfTheHill.KingOfTheHill");
				if (type == null)
				{
					continue;
				}
				try
				{
					PropertyInfo property = type.GetProperty("ModeKohActive", BindingFlags.Static | BindingFlags.Public);
					if (property != null)
					{
						object value = property.GetValue(null);
						if (value is bool)
						{
							return (bool)value;
						}
					}
				}
				catch
				{
				}
				return true;
			}
			return false;
		}

		private static GameObject? FindPrefabByName(string name, Instance log)
		{
			try
			{
				GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
				foreach (GameObject val in array)
				{
					if ((Object)(object)val != (Object)null && ((Object)val).name == name)
					{
						log.Msg("[Specials] FindPrefabByName: matched '" + name + "' (via Resources.FindObjectsOfTypeAll)");
						return val;
					}
				}
			}
			catch (Exception ex)
			{
				log.Warning("[Specials] FindPrefabByName threw: " + ex.Message);
			}
			return null;
		}
	}
	public static class MapBalanceSpecials
	{
		public class KohEntry
		{
			public GameObject? Obj;

			public float CaptureRadius;

			public float ExclusionRadius;
		}

		public class FusionReactorEntry
		{
			public GameObject? Obj;

			public float CaptureRadius;
		}

		public static KohEntry? Koh;

		public static readonly List<FusionReactorEntry> FusionReactors = new List<FusionReactorEntry>();

		public static event Action? OnSpecialsReady;

		internal static void RaiseReady()
		{
			OnSpecialsReady?.Invoke();
		}

		public static void Clear()
		{
			Koh = null;
			FusionReactors.Clear();
		}
	}
}
