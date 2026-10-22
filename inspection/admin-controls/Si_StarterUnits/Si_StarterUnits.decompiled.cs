using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Si_StarterUnits;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(StarterUnits), "Starter Units", "1.0.0", "DrMuck", null)]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_StarterUnits")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+26b98eab5ccc4a2536ed3110b392b6c30591b723")]
[assembly: AssemblyProduct("Si_StarterUnits")]
[assembly: AssemblyTitle("Si_StarterUnits")]
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
namespace Si_StarterUnits
{
	public class StarterUnitsConfig
	{
		[JsonProperty("units")]
		public List<UnitEntry> Units = new List<UnitEntry>();
	}
	public class UnitEntry
	{
		[JsonProperty("name")]
		public string Name;

		[JsonProperty("count")]
		public int Count = 1;
	}
	public class CrossSpawnConfig
	{
		[JsonProperty("cross_spawns")]
		public List<CrossSpawnEntry> CrossSpawns = new List<CrossSpawnEntry>();
	}
	public class CrossSpawnEntry
	{
		[JsonProperty("from_faction")]
		public string FromFaction;

		[JsonProperty("at_faction")]
		public string AtFaction;

		[JsonProperty("units")]
		public List<UnitEntry> Units = new List<UnitEntry>();
	}
	public class StarterUnits : MelonMod
	{
		private static bool _is4WayRound;

		private static bool _spawnScheduled;

		private static string ConfigDir => Path.Combine(MelonEnvironment.UserDataDirectory, "StarterUnits");

		public override void OnInitializeMelon()
		{
			Directory.CreateDirectory(ConfigDir);
			MelonLogger.Msg("Si_StarterUnits v1.0.0 loaded. Configs: " + ConfigDir);
		}

		public override void OnLateInitializeMelon()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected Obj, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Expected Obj, but got Unknown
			SubscribeTo4WayEvents();
			try
			{
				Harmony val = new Harmony("com.drmuck.starterunits");
				Type type = typeof(GameMode).Assembly.GetType("MP_Strategy");
				if (type != null)
				{
					MethodInfo method = type.GetMethod("OnMissionStateChanged", BindingFlags.Instance | BindingFlags.NonPublic);
					if (method != null)
					{
						val.Patch((MethodBase)method, (HarmonyMethod)null, new HarmonyMethod(typeof(StarterUnits), "Postfix_OnMissionStateChanged", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
						MelonLogger.Msg("[STARTER] Patched OnMissionStateChanged");
					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("[STARTER] Patch failed: " + ex.Message);
			}
		}

		public static void Postfix_OnMissionStateChanged(object __instance, EMissionState __0)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Invalid comparison between Unknown and I4
			if ((int)__0 == 1 && Game.GetIsServer())
			{
				MelonCoroutines.Start(SpawnAllStarters(5f));
			}
		}

		private static void SubscribeTo4WayEvents()
		{
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
				if (type == null)
				{
					return;
				}
				EventInfo eventInfo = type.GetEvent("On4WayRoundStarted", BindingFlags.Static | BindingFlags.Public);
				if (eventInfo != null)
				{
					Action<Team, Team> handler = (Team a, Team w) =>
					{
						_is4WayRound = true;
					};
					eventInfo.AddEventHandler(null, handler);
				}
				EventInfo eventInfo2 = type.GetEvent("On4WayRoundEnded", BindingFlags.Static | BindingFlags.Public);
				if (eventInfo2 != null)
				{
					Action handler2 = () =>
					{
						_is4WayRound = false;
					};
					eventInfo2.AddEventHandler(null, handler2);
				}
				MelonLogger.Msg("[STARTER] Subscribed to Si_4way events");
			}
			catch
			{
			}
		}

		private static IEnumerator SpawnAllStarters(float delay)
		{
			if (delay > 0f)
			{
				yield return (object)new WaitForSeconds(delay);
			}
			MelonLogger.Msg("[STARTER] Spawning starter units...");
			ObjectInfo[] array = Resources.FindObjectsOfTypeAll<ObjectInfo>();
			Dictionary<string, ObjectInfo> dictionary = new Dictionary<string, ObjectInfo>(StringComparer.OrdinalIgnoreCase);
			ObjectInfo[] array2 = array;
			foreach (ObjectInfo val in array2)
			{
				if (!((Object)(object)val == (Object)null) && !((Object)(object)val.Prefab == (Object)null))
				{
					string displayName = val.DisplayName;
					if (!string.IsNullOrEmpty(displayName) && !dictionary.ContainsKey(displayName))
					{
						dictionary[displayName] = val;
					}
				}
			}
			int num = 0;
			string[] array3 = new string[4] { "sol.json", "centauri.json", "alien.json", "wildlife.json" };
			foreach (string text in array3)
			{
				string path = Path.Combine(ConfigDir, text);
				if (!File.Exists(path))
				{
					continue;
				}
				try
				{
					StarterUnitsConfig starterUnitsConfig = JsonConvert.DeserializeObject<StarterUnitsConfig>(File.ReadAllText(path));
					if (starterUnitsConfig?.Units == null || starterUnitsConfig.Units.Count == 0)
					{
						continue;
					}
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
					Team val2 = FindTeamByFaction(fileNameWithoutExtension);
					if ((Object)(object)val2 == (Object)null)
					{
						MelonLogger.Warning("[STARTER] Team not found for '" + fileNameWithoutExtension + "'");
						continue;
					}
					Structure val3 = ((val2.Structures.Count > 0) ? val2.Structures[0] : null);
					if ((Object)(object)val3 == (Object)null)
					{
						MelonLogger.Warning("[STARTER] No base structure for '" + fileNameWithoutExtension + "'");
						continue;
					}
					int num2 = SpawnUnitsAroundStructure(starterUnitsConfig.Units, val2, val3, dictionary);
					num += num2;
					MelonLogger.Msg($"[STARTER] {fileNameWithoutExtension}: spawned {num2} units");
				}
				catch (Exception ex)
				{
					MelonLogger.Error("[STARTER] Error loading " + text + ": " + ex.Message);
				}
			}
			if (_is4WayRound)
			{
				string path2 = Path.Combine(ConfigDir, "4way_cross_spawns.json");
				if (File.Exists(path2))
				{
					try
					{
						CrossSpawnConfig crossSpawnConfig = JsonConvert.DeserializeObject<CrossSpawnConfig>(File.ReadAllText(path2));
						if (crossSpawnConfig?.CrossSpawns != null)
						{
							foreach (CrossSpawnEntry crossSpawn in crossSpawnConfig.CrossSpawns)
							{
								Team val4 = FindTeamByFaction(crossSpawn.FromFaction);
								Team val5 = FindTeamByFaction(crossSpawn.AtFaction);
								if (!((Object)(object)val4 == (Object)null) && !((Object)(object)val5 == (Object)null))
								{
									Structure val6 = ((val5.Structures.Count > 0) ? val5.Structures[0] : null);
									if (!((Object)(object)val6 == (Object)null))
									{
										int num3 = SpawnUnitsAroundStructure(crossSpawn.Units, val4, val6, dictionary);
										num += num3;
										MelonLogger.Msg($"[STARTER] Cross-spawn: {crossSpawn.FromFaction} units at {crossSpawn.AtFaction} base: {num3} units");
									}
								}
							}
						}
					}
					catch (Exception ex2)
					{
						MelonLogger.Error("[STARTER] Cross-spawn error: " + ex2.Message);
					}
				}
			}
			MelonLogger.Msg($"[STARTER] Complete: {num} total units spawned");
		}

		private static int SpawnUnitsAroundStructure(List<UnitEntry> units, Team team, Structure baseStruct, Dictionary<string, ObjectInfo> infoByName)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position = ((Component)baseStruct).transform.position;
			Vector3 forward = ((Component)baseStruct).transform.forward;
			float num = 120f;
			List<(string, ObjectInfo)> list = new List<(string, ObjectInfo)>();
			foreach (UnitEntry unit in units)
			{
				if (!infoByName.TryGetValue(unit.Name, out ObjectInfo value))
				{
					MelonLogger.Warning("[STARTER] Unit not found: '" + unit.Name + "'");
					continue;
				}
				for (int i = 0; i < unit.Count; i++)
				{
					list.Add((unit.Name, value));
				}
			}
			int count = list.Count;
			int num2 = 0;
			for (int j = 0; j < count; j++)
			{
				try
				{
					float num3 = 360f / (float)count * (float)j;
					Vector3 val = Quaternion.Euler(0f, num3, 0f) * forward;
					Vector3 val2 = position + val * num;
					float num4 = val2.y;
					Terrain[] activeTerrains = Terrain.activeTerrains;
					foreach (Terrain val3 in activeTerrains)
					{
						if (!((Object)(object)val3 == (Object)null))
						{
							Vector3 position2 = ((Component)val3).transform.position;
							TerrainData terrainData = val3.terrainData;
							if (!((Object)(object)terrainData == (Object)null) && val2.x >= position2.x && val2.x <= position2.x + terrainData.size.x && val2.z >= position2.z && val2.z <= position2.z + terrainData.size.z)
							{
								num4 = val3.SampleHeight(val2) + position2.y;
								break;
							}
						}
					}
					val2.y = num4 + 1f;
					Quaternion val4 = Quaternion.LookRotation(val, Vector3.up);
					Game.SpawnPrefab(list[j].Item2.Prefab, (Player)null, team, val2, val4, true, true);
					num2++;
				}
				catch (Exception ex)
				{
					MelonLogger.Warning("[STARTER] Spawn failed '" + list[j].Item1 + "': " + ex.Message);
				}
			}
			return num2;
		}

		private static Team FindTeamByFaction(string faction)
		{
			if (string.IsNullOrEmpty(faction))
			{
				return null;
			}
			faction = faction.ToLowerInvariant();
			foreach (Team team in Team.Teams)
			{
				if (!((Object)(object)team == (Object)null))
				{
					string text = (team.TeamName ?? "").ToLowerInvariant();
					if (faction == "sol" && text.Contains("sol"))
					{
						return team;
					}
					if (faction == "centauri" && text.Contains("centauri"))
					{
						return team;
					}
					if (faction == "alien" && text.Contains("alien") && !text.Contains("worm"))
					{
						return team;
					}
					if (faction == "wildlife" && (text.Contains("worm") || text.Contains("wildlife")))
					{
						return team;
					}
				}
			}
			return null;
		}
	}
}
