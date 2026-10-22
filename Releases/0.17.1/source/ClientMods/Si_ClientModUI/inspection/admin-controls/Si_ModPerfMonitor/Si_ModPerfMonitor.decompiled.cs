using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using Microsoft.CodeAnalysis;
using Si_ModPerfMonitor;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(ModPerfMonitor), "Si_ModPerfMonitor", "1.0.1", "schwe", null)]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_ModPerfMonitor")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+f92c3dbe61bfa816c749fa90cf1a8dbdefa2829e")]
[assembly: AssemblyProduct("Si_ModPerfMonitor")]
[assembly: AssemblyTitle("Si_ModPerfMonitor")]
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
namespace Si_ModPerfMonitor
{
	public class ModPerfMonitor : MelonMod
	{
		internal static MelonPreferences_Category _prefs;

		internal static MelonPreferences_Entry<bool> _enable;

		internal static MelonPreferences_Entry<float> _logInterval;

		internal static MelonPreferences_Entry<int> _topN;

		internal static MelonPreferences_Entry<float> _minTotalMs;

		internal static MelonPreferences_Entry<float> _attachDelaySeconds;

		internal static MelonPreferences_Entry<float> _rescanIntervalSeconds;

		private Harmony _harmony;

		private bool _attached;

		private float _startTime;

		private float _lastReportTime;

		private float _lastRescanTime;

		public override void OnInitializeMelon()
		{
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Expected Obj, but got Unknown
			_prefs = MelonPreferences.CreateCategory("Silica");
			_prefs.SetFilePath("UserData/MelonPreferences.cfg", false);
			_enable = _prefs.CreateEntry<bool>("ModPerfMonitor_Enable", true, "Enable the mod performance monitor", (string)null, false, false, (ValueValidator)null, (string)null);
			_logInterval = _prefs.CreateEntry<float>("ModPerfMonitor_LogInterval", 60f, "Seconds between perf reports", (string)null, false, false, (ValueValidator)null, (string)null);
			_topN = _prefs.CreateEntry<int>("ModPerfMonitor_TopN", 20, "How many slowest methods to print per report", (string)null, false, false, (ValueValidator)null, (string)null);
			_minTotalMs = _prefs.CreateEntry<float>("ModPerfMonitor_MinTotalMs", 1f, "Ignore methods whose total time in an interval is below this (ms)", (string)null, false, false, (ValueValidator)null, (string)null);
			_attachDelaySeconds = _prefs.CreateEntry<float>("ModPerfMonitor_DelaySeconds", 15f, "Seconds after init to wait before attaching profiler wrappers", (string)null, false, false, (ValueValidator)null, (string)null);
			_rescanIntervalSeconds = _prefs.CreateEntry<float>("ModPerfMonitor_RescanSeconds", 120f, "Seconds between rescans that pick up newly-added Harmony patches", (string)null, false, false, (ValueValidator)null, (string)null);
			try
			{
				_prefs.SaveToFile(false);
			}
			catch
			{
			}
			_harmony = new Harmony("Si_ModPerfMonitor");
			if (!_enable.Value)
			{
				MelonLogger.Msg("[PerfMon] Disabled via MelonPreferences (ModPerfMonitor_Enable=false)");
				return;
			}
			_startTime = Time.realtimeSinceStartup;
			_lastReportTime = _startTime;
			_lastRescanTime = _startTime;
			MelonLogger.Msg($"[PerfMon] Initialized. Attaching in {_attachDelaySeconds.Value:F0}s via OnUpdate driver.");
		}

		public override void OnUpdate()
		{
			if (!_enable.Value)
			{
				return;
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (!_attached)
			{
				if (realtimeSinceStartup - _startTime >= _attachDelaySeconds.Value)
				{
					try
					{
						Profiler.AttachToAllPatchedMethods(_harmony);
						MelonLogger.Msg($"[PerfMon] Attached to {Profiler.WrappedCount} patched methods across {Profiler.WrappedOwnerCount} mods.");
					}
					catch (Exception ex)
					{
						MelonLogger.Error("[PerfMon] Attach failed: " + ex.Message + "\n" + ex.StackTrace);
					}
					_attached = true;
					_lastReportTime = realtimeSinceStartup;
					_lastRescanTime = realtimeSinceStartup;
				}
				return;
			}
			if (realtimeSinceStartup - _lastReportTime >= _logInterval.Value)
			{
				_lastReportTime = realtimeSinceStartup;
				try
				{
					Profiler.Report(_topN.Value, _minTotalMs.Value);
				}
				catch (Exception ex2)
				{
					MelonLogger.Warning("[PerfMon] Report error: " + ex2.Message);
				}
			}
			if (!(realtimeSinceStartup - _lastRescanTime >= _rescanIntervalSeconds.Value))
			{
				return;
			}
			_lastRescanTime = realtimeSinceStartup;
			try
			{
				int num = Profiler.AttachToAllPatchedMethods(_harmony);
				if (num > 0)
				{
					MelonLogger.Msg($"[PerfMon] Rescan: wrapped {num} newly-patched methods (total {Profiler.WrappedCount}).");
				}
			}
			catch (Exception ex3)
			{
				MelonLogger.Warning("[PerfMon] Rescan error: " + ex3.Message);
			}
		}

		public override void OnApplicationQuit()
		{
			try
			{
				Profiler.Report(_topN.Value, 0f, final: true);
			}
			catch
			{
			}
		}
	}
	internal static class Profiler
	{
		private class Stats
		{
			public long Calls;

			public long TotalTicks;

			public long MaxTicks;

			public long IntervalCalls;

			public long IntervalTicks;

			public long IntervalMaxTicks;

			public void ResetInterval()
			{
				IntervalCalls = 0L;
				IntervalTicks = 0L;
				IntervalMaxTicks = 0L;
			}
		}

		private static readonly Dictionary<MethodBase, Stats> _stats = new Dictionary<MethodBase, Stats>();

		private static readonly Dictionary<MethodBase, string> _owners = new Dictionary<MethodBase, string>();

		private static readonly object _lock = new object();

		private static readonly double _tickToMs = 1000.0 / (double)Stopwatch.Frequency;

		private static readonly HashSet<string> _knownOwners = new HashSet<string>();

		public static int WrappedCount { get; private set; }

		public static int WrappedOwnerCount { get; private set; }

		public static int AttachToAllPatchedMethods(Harmony selfHarmony)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected Obj, but got Unknown
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Expected Obj, but got Unknown
			MethodBase[] array = Harmony.GetAllPatchedMethods().ToArray();
			HashSet<string> hashSet = new HashSet<string>();
			HarmonyMethod val = new HarmonyMethod(typeof(Profiler).GetMethod("PrefixTimer", BindingFlags.Static | BindingFlags.NonPublic));
			HarmonyMethod val2 = new HarmonyMethod(typeof(Profiler).GetMethod("PostfixTimer", BindingFlags.Static | BindingFlags.NonPublic));
			val.priority = 800;
			val2.priority = 0;
			int num = 0;
			MethodBase[] array2 = array;
			foreach (MethodBase methodBase in array2)
			{
				if (methodBase == null || methodBase.DeclaringType?.Assembly == typeof(Profiler).Assembly)
				{
					continue;
				}
				lock (_lock)
				{
					if (_stats.ContainsKey(methodBase))
					{
						continue;
					}
				}
				string value;
				try
				{
					Patches patchInfo = Harmony.GetPatchInfo(methodBase);
					HashSet<string> hashSet2 = new HashSet<string>();
					if (patchInfo != null)
					{
						foreach (Patch prefix in patchInfo.Prefixes)
						{
							if (!string.IsNullOrEmpty(prefix.owner))
							{
								hashSet2.Add(prefix.owner);
							}
						}
						foreach (Patch postfix in patchInfo.Postfixes)
						{
							if (!string.IsNullOrEmpty(postfix.owner))
							{
								hashSet2.Add(postfix.owner);
							}
						}
						foreach (Patch transpiler in patchInfo.Transpilers)
						{
							if (!string.IsNullOrEmpty(transpiler.owner))
							{
								hashSet2.Add(transpiler.owner);
							}
						}
						foreach (Patch finalizer in patchInfo.Finalizers)
						{
							if (!string.IsNullOrEmpty(finalizer.owner))
							{
								hashSet2.Add(finalizer.owner);
							}
						}
					}
					hashSet2.Remove("Si_ModPerfMonitor");
					if (hashSet2.Count == 0)
					{
						continue;
					}
					value = string.Join(",", hashSet2);
					foreach (string item in hashSet2)
					{
						hashSet.Add(item);
					}
					goto IL_028b;
				}
				catch
				{
				}
				continue;
				IL_028b:
				try
				{
					selfHarmony.Patch(methodBase, val, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					lock (_lock)
					{
						_stats[methodBase] = new Stats();
						_owners[methodBase] = value;
					}
					num++;
				}
				catch (Exception ex)
				{
					MelonLogger.Warning("[PerfMon] Could not wrap " + FormatMethod(methodBase) + ": " + ex.Message);
				}
			}
			WrappedCount += num;
			foreach (string item2 in hashSet)
			{
				_knownOwners.Add(item2);
			}
			WrappedOwnerCount = _knownOwners.Count;
			return num;
		}

		private static void PrefixTimer(MethodBase __originalMethod, out long __state)
		{
			__state = Stopwatch.GetTimestamp();
		}

		private static void PostfixTimer(MethodBase __originalMethod, long __state)
		{
			long num = Stopwatch.GetTimestamp() - __state;
			if (num < 0)
			{
				return;
			}
			lock (_lock)
			{
				if (_stats.TryGetValue(__originalMethod, out Stats value))
				{
					value.Calls++;
					value.TotalTicks += num;
					if (num > value.MaxTicks)
					{
						value.MaxTicks = num;
					}
					value.IntervalCalls++;
					value.IntervalTicks += num;
					if (num > value.IntervalMaxTicks)
					{
						value.IntervalMaxTicks = num;
					}
				}
			}
		}

		public static void Report(int topN, float minTotalMs, bool final = false)
		{
			List<(MethodBase, string, Stats)> source;
			lock (_lock)
			{
				source = _stats.Select((KeyValuePair<MethodBase, Stats> kv) => (Key: kv.Key, _owners.TryGetValue(kv.Key, out string value) ? value : "?", Value: kv.Value)).ToList();
			}
			var list = (from r in source.Select(((MethodBase method, string owners, Stats s) t) => new
				{
					method = t.method,
					owners = t.owners,
					calls = (final ? t.s.Calls : t.s.IntervalCalls),
					totalMs = (double)(final ? t.s.TotalTicks : t.s.IntervalTicks) * _tickToMs,
					maxMs = (double)(final ? t.s.MaxTicks : t.s.IntervalMaxTicks) * _tickToMs
				})
				where r.calls > 0 && r.totalMs >= (double)minTotalMs
				orderby r.totalMs descending
				select r).Take(Math.Max(1, topN)).ToList();
			string arg = (final ? "CUMULATIVE" : "INTERVAL");
			MelonLogger.Msg($"================ [PerfMon] {arg} report (top {list.Count}) ================");
			MelonLogger.Msg(string.Format("{0,10} {1,9} {2,9} {3,9}  owner | method", "totalMs", "avgMs", "maxMs", "calls"));
			foreach (var item in list)
			{
				double num = item.totalMs / (double)Math.Max(1L, item.calls);
				MelonLogger.Msg($"{item.totalMs,10:F2} {num,9:F3} {item.maxMs,9:F2} {item.calls,9}  {item.owners} | {FormatMethod(item.method)}");
			}
			var list2 = (from r in list
				group r by r.owners into g
				select new
				{
					owner = g.Key,
					totalMs = g.Sum(x => x.totalMs),
					calls = g.Sum(x => x.calls)
				} into x
				orderby x.totalMs descending
				select x).ToList();
			if (list2.Count > 0)
			{
				MelonLogger.Msg("---- per-owner ----");
				foreach (var item2 in list2)
				{
					MelonLogger.Msg($"{item2.totalMs,10:F2} ms  {item2.calls,9} calls  {item2.owner}");
				}
			}
			MelonLogger.Msg("=====================================================");
			if (final)
			{
				return;
			}
			lock (_lock)
			{
				foreach (Stats value2 in _stats.Values)
				{
					value2.ResetInterval();
				}
			}
		}

		private static string FormatMethod(MethodBase m)
		{
			try
			{
				return m.DeclaringType?.FullName + "::" + m.Name;
			}
			catch
			{
				return m?.Name ?? "?";
			}
		}
	}
}
