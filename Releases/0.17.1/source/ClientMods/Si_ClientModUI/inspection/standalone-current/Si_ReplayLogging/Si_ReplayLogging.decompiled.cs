using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using Microsoft.CodeAnalysis;
using Si_ReplayLogging;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(ReplayLogging), "Replay Logging", "0.6.0", "schwe", null)]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_ReplayLogging")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+088605c43d1219717d3ea3b3c68e5aa23fac5c67")]
[assembly: AssemblyProduct("Si_ReplayLogging")]
[assembly: AssemblyTitle("Si_ReplayLogging")]
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
namespace Si_ReplayLogging
{
	public class ReplayWriter : IDisposable
	{
		private BinaryWriter _bw;

		private bool _disposed;

		private readonly Dictionary<string, byte> _typeIds = new Dictionary<string, byte>();

		private byte _nextTypeId;

		private const byte REC_TYPE_REGISTER = 1;

		private const byte REC_ENTITY_REGISTER = 2;

		private const byte REC_PLAYER_REGISTER = 3;

		private const byte REC_PLAYER_CONTROL = 4;

		private const byte REC_TICK_FRAME = 16;

		private const byte REC_UNIT_DESTROYED = 32;

		private const byte REC_BUILDING_DESTROYED = 48;

		private const byte REC_END = byte.MaxValue;

		public ReplayWriter(string filePath)
		{
			FileStream output = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read, 8192);
			_bw = new BinaryWriter(output, Encoding.UTF8);
		}

		public void WriteHeader(string mapName, string gameType, long unixTimestamp, ushort tickIntervalMs)
		{
			_bw.Write((byte)83);
			_bw.Write((byte)82);
			_bw.Write((byte)80);
			_bw.Write((byte)76);
			_bw.Write((byte)3);
			_bw.Write(tickIntervalMs);
			WriteString(mapName);
			WriteString(gameType);
			_bw.Write(unixTimestamp);
			_bw.Flush();
		}

		public byte GetOrRegisterType(string typeName)
		{
			if (_typeIds.TryGetValue(typeName, out var value))
			{
				return value;
			}
			byte b = _nextTypeId++;
			_typeIds[typeName] = b;
			_bw.Write((byte)1);
			_bw.Write(b);
			WriteString(typeName);
			return b;
		}

		public void WritePlayerRegister(byte playerId, byte teamIndex, string playerName)
		{
			_bw.Write((byte)3);
			_bw.Write(playerId);
			_bw.Write(teamIndex);
			WriteString(playerName);
		}

		public void WriteEntityRegister(ushort entityId, byte teamIndex, byte typeId, bool isUnit, byte controllerId, short x, short y, short z)
		{
			_bw.Write((byte)2);
			_bw.Write(entityId);
			_bw.Write(teamIndex);
			_bw.Write(typeId);
			_bw.Write(isUnit ? ((byte)1) : ((byte)0));
			_bw.Write(controllerId);
			_bw.Write(x);
			_bw.Write(y);
			_bw.Write(z);
		}

		public void WriteTickFrame(ushort tickNumber, ushort count, ushort[] entityIds, short[] xs, short[] ys, short[] zs)
		{
			_bw.Write((byte)16);
			_bw.Write(tickNumber);
			_bw.Write(count);
			for (int i = 0; i < count; i++)
			{
				_bw.Write(entityIds[i]);
				_bw.Write(xs[i]);
				_bw.Write(ys[i]);
				_bw.Write(zs[i]);
			}
			_bw.Flush();
		}

		public void WriteUnitDestroyed(ushort tickNumber, ushort victimId, ushort attackerId)
		{
			_bw.Write((byte)32);
			_bw.Write(tickNumber);
			_bw.Write(victimId);
			_bw.Write(attackerId);
		}

		public void WritePlayerControl(ushort tickNumber, ushort entityId, byte playerId)
		{
			_bw.Write((byte)4);
			_bw.Write(tickNumber);
			_bw.Write(entityId);
			_bw.Write(playerId);
		}

		public void WriteBuildingDestroyed(ushort tickNumber, ushort buildingId, ushort attackerId)
		{
			_bw.Write((byte)48);
			_bw.Write(tickNumber);
			_bw.Write(buildingId);
			_bw.Write(attackerId);
		}

		public void WriteEndOfReplay()
		{
			_bw.Write(byte.MaxValue);
			_bw.Flush();
		}

		public void Close()
		{
			if (!_disposed)
			{
				_bw.Flush();
				_bw.Close();
				_disposed = true;
			}
		}

		public void Dispose()
		{
			Close();
		}

		private void WriteString(string s)
		{
			if (s == null)
			{
				s = "";
			}
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			int num = Math.Min(bytes.Length, 255);
			_bw.Write((byte)num);
			_bw.Write(bytes, 0, num);
		}
	}
	public class ReplayLogging : MelonMod
	{
		[HarmonyPatch(typeof(MusicJukeboxHandler), "OnGameStarted")]
		private static class Patch_GameStarted
		{
			public static void Postfix(GameMode __0)
			{
				try
				{
					StartRecording(__0);
				}
				catch (Exception ex)
				{
					MelonLogger.Error("StartRecording error: " + ex.Message);
				}
			}
		}

		[HarmonyPatch(typeof(MusicJukeboxHandler), "OnGameEnded")]
		private static class Patch_GameEnded
		{
			public static void Postfix(GameMode __0, Team __1)
			{
				try
				{
					StopRecording();
				}
				catch (Exception ex)
				{
					MelonLogger.Error("StopRecording error: " + ex.Message);
				}
			}
		}

		[HarmonyPatch(typeof(MP_Strategy), "OnUnitDestroyed")]
		private static class Patch_UnitDestroyed
		{
			public static void Postfix(Unit __0, GameObject __1)
			{
				try
				{
					OnUnitDestroyed(__0, __1);
				}
				catch (Exception ex)
				{
					MelonLogger.Error("OnUnitDestroyed error: " + ex.Message);
				}
			}
		}

		[HarmonyPatch(typeof(MP_Strategy), "OnStructureDestroyed")]
		private static class Patch_StructureDestroyed
		{
			public static void Postfix(Structure __0, GameObject __1)
			{
				try
				{
					OnStructureDestroyed(__0, __1);
				}
				catch (Exception ex)
				{
					MelonLogger.Error("OnStructureDestroyed error: " + ex.Message);
				}
			}
		}

		private const string PREFS_CATEGORY = "Si_ReplayLogging";

		private const float TICK_INTERVAL_DEFAULT = 1f;

		private const float TICK_INTERVAL_MIN = 0.05f;

		private const float TICK_INTERVAL_MAX = 10f;

		private static MelonPreferences_Category? _prefs;

		private static MelonPreferences_Entry<float>? _prefTickInterval;

		private static MelonPreferences_Entry<bool>? _prefLogAIvsAIUnitKills;

		private static float _tickInterval = 1f;

		private static ushort _tickIntervalMs = 1000;

		private static bool _logAIvsAIUnitKills = true;

		private static bool _isRecording;

		private static ReplayWriter? _writer;

		private static ushort _tickNumber;

		private static readonly Stopwatch _tickClock = new Stopwatch();

		private static bool _tickOverflowLogged;

		private static int _writtenTickCount;

		private static string _currentMapName = "";

		private static string _replayDir = "";

		private static bool _firstTick = true;

		private static int _unitDestroyedCount;

		private static int _structureDestroyedCount;

		private static int _selfAttributedCount;

		private static int _openingDespawnCount;

		private static int _aiVsAiKillCount;

		private static int _aiVsAiSkippedCount;

		private const float OPENING_DESPAWN_WINDOW_S = 10f;

		private static readonly Dictionary<int, ushort> _entityIds = new Dictionary<int, ushort>();

		private static ushort _nextEntityId;

		private static readonly Dictionary<int, byte> _playerIds = new Dictionary<int, byte>();

		private static byte _nextPlayerId;

		private static readonly Dictionary<ushort, byte> _entityController = new Dictionary<ushort, byte>();

		private static ushort[] _idBuf = new ushort[2048];

		private static short[] _xBuf = new short[2048];

		private static short[] _yBuf = new short[2048];

		private static short[] _zBuf = new short[2048];

		public override void OnInitializeMelon()
		{
			_replayDir = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)), "UserData", "ReplayLogs");
			if (!Directory.Exists(_replayDir))
			{
				Directory.CreateDirectory(_replayDir);
			}
			try
			{
				_prefs = MelonPreferences.CreateCategory("Si_ReplayLogging", "Si_ReplayLogging");
				_prefTickInterval = _prefs.CreateEntry<float>("TickIntervalSeconds", 1f, (string)null, "Seconds between recorded position snapshots. Lower = smoother replays and larger files; higher = coarser and smaller. " + $"Clamped to {0.05f}-{10f}s. " + "Applied at the start of the next round.", false, false, (ValueValidator)null, (string)null);
				_prefLogAIvsAIUnitKills = _prefs.CreateEntry<bool>("LogAIvsAIUnitKills", true, (string)null, "Record unit kills where neither the victim nor the killer was piloted by a player. On by default - this is most of the kill data in bot rounds. Set false to keep only kills a player was involved in. Applied at the start of the next round.", false, false, (ValueValidator)null, (string)null);
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("Could not create preferences: " + ex.Message);
				MelonLogger.Warning($"Falling back to {1f}s ticks.");
			}
			MelonLogger.Msg("Replay Logging v0.6.0 initialized. Output: " + _replayDir);
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			switch (sceneName)
			{
			case "MainMenu":
				return;
			case "Loading":
				return;
			}
			if (!string.IsNullOrEmpty(sceneName))
			{
				_currentMapName = sceneName;
			}
		}

		public override void OnUpdate()
		{
			if (!_isRecording || _writer == null)
			{
				return;
			}
			long num = (long)(_tickClock.Elapsed.TotalSeconds / (double)_tickInterval);
			if (num <= _tickNumber)
			{
				return;
			}
			GameMode currentGameMode = GameMode.CurrentGameMode;
			if (currentGameMode == null || !currentGameMode.GameOngoing)
			{
				StopRecording();
				return;
			}
			if (num > 65535)
			{
				if (!_tickOverflowLogged)
				{
					_tickOverflowLogged = true;
					MelonLogger.Warning($"Round exceeded {ushort.MaxValue} ticks at {_tickInterval:0.##}s - " + "no further position frames will be recorded this round.");
				}
				return;
			}
			_tickNumber = (ushort)num;
			if (_firstTick)
			{
				_firstTick = false;
				RegisterAllPlayers();
			}
			CheckForNewPlayers();
			try
			{
				WriteTickFrame();
				_writtenTickCount++;
			}
			catch (Exception ex)
			{
				MelonLogger.Error("Tick error: " + ex.Message + "\n" + ex.StackTrace);
				try
				{
					StopRecording();
				}
				catch
				{
				}
			}
		}

		private static BaseGameObject? ResolveAttackerBase(GameObject? attackerObj)
		{
			if (attackerObj == null)
			{
				return null;
			}
			try
			{
				return attackerObj.GetBaseGameObject();
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("Attacker resolution failed: " + ex.Message);
				return null;
			}
		}

		private static ushort RegisterAttackerId(BaseGameObject? attackerBase)
		{
			if (attackerBase == null)
			{
				return 0;
			}
			try
			{
				if (attackerBase is Unit unit)
				{
					return GetOrRegisterUnit(unit);
				}
				if (attackerBase is Structure structure)
				{
					return GetOrRegisterStructure(structure);
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Warning("Attacker registration failed: " + ex.Message);
			}
			return 0;
		}

		private static ushort ResolveAttackerId(GameObject? attackerObj)
		{
			return RegisterAttackerId(ResolveAttackerBase(attackerObj));
		}

		private static bool IsPlayerControlled(Unit unit)
		{
			try
			{
				if (unit.ControlledBy != null)
				{
					return true;
				}
			}
			catch
			{
			}
			try
			{
				if (_entityIds.TryGetValue(unit.GetInstanceID(), out var value) && _entityController.TryGetValue(value, out var value2) && value2 != 0)
				{
					return true;
				}
			}
			catch
			{
			}
			return false;
		}

		private static bool IsPlayerInvolvedKill(Unit victim, BaseGameObject? attackerBase)
		{
			if (IsPlayerControlled(victim))
			{
				return true;
			}
			if (attackerBase is Unit unit && IsPlayerControlled(unit))
			{
				return true;
			}
			return false;
		}

		private static void OnUnitDestroyed(Unit victim, GameObject attackerObj)
		{
			if (!_isRecording || _writer == null)
			{
				return;
			}
			BaseGameObject attackerBase = ResolveAttackerBase(attackerObj);
			bool flag = !IsPlayerInvolvedKill(victim, attackerBase);
			if (flag && !_logAIvsAIUnitKills)
			{
				_aiVsAiSkippedCount++;
				return;
			}
			ushort orRegisterUnit = GetOrRegisterUnit(victim);
			ushort num = RegisterAttackerId(attackerBase);
			_writer.WriteUnitDestroyed(_tickNumber, orRegisterUnit, num);
			_unitDestroyedCount++;
			if (flag)
			{
				_aiVsAiKillCount++;
			}
			if (num != 0 && num == orRegisterUnit)
			{
				if ((float)(int)_tickNumber * ((float)(int)_tickIntervalMs / 1000f) <= 10f)
				{
					_openingDespawnCount++;
				}
				else
				{
					_selfAttributedCount++;
				}
			}
		}

		private static void OnStructureDestroyed(Structure victim, GameObject attackerObj)
		{
			if (_isRecording && _writer != null)
			{
				ushort orRegisterStructure = GetOrRegisterStructure(victim);
				ushort attackerId = ResolveAttackerId(attackerObj);
				_writer.WriteBuildingDestroyed(_tickNumber, orRegisterStructure, attackerId);
				_structureDestroyedCount++;
			}
		}

		private static byte GetOrRegisterPlayer(Player player)
		{
			int instanceID = player.GetInstanceID();
			if (_playerIds.TryGetValue(instanceID, out var value))
			{
				return value;
			}
			value = _nextPlayerId++;
			_playerIds[instanceID] = value;
			string playerName = "Unknown";
			try
			{
				playerName = player.PlayerName;
			}
			catch
			{
			}
			byte teamIndex = byte.MaxValue;
			try
			{
				teamIndex = GetTeamIndex(player.Team);
			}
			catch
			{
			}
			_writer.WritePlayerRegister(value, teamIndex, playerName);
			return value;
		}

		private static byte GetControllingPlayerId(Unit unit)
		{
			try
			{
				Player controlledBy = unit.ControlledBy;
				if (controlledBy != null)
				{
					return GetOrRegisterPlayer(controlledBy);
				}
			}
			catch
			{
			}
			return 0;
		}

		private static void RegisterAllPlayers()
		{
			try
			{
				List<Player> players = Player.Players;
				for (int i = 0; i < players.Count; i++)
				{
					try
					{
						Player player = players[i];
						if (player != null)
						{
							GetOrRegisterPlayer(player);
						}
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
		}

		private static void CheckForNewPlayers()
		{
			try
			{
				List<Player> players = Player.Players;
				for (int i = 0; i < players.Count; i++)
				{
					try
					{
						Player player = players[i];
						if (player != null)
						{
							int instanceID = player.GetInstanceID();
							if (!_playerIds.ContainsKey(instanceID))
							{
								GetOrRegisterPlayer(player);
							}
						}
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
		}

		private static ushort GetOrRegisterUnit(Unit unit)
		{
			int instanceID = unit.GetInstanceID();
			if (_entityIds.TryGetValue(instanceID, out var value))
			{
				return value;
			}
			value = _nextEntityId++;
			_entityIds[instanceID] = value;
			string typeName = "Unknown";
			try
			{
				typeName = ((unit.ObjectInfo != null) ? unit.ObjectInfo.DisplayName : unit.ToString());
			}
			catch
			{
				try
				{
					typeName = unit.ToString();
				}
				catch
				{
				}
			}
			Team team = null;
			try
			{
				team = unit.Team;
			}
			catch
			{
			}
			byte teamIndex = GetTeamIndex(team);
			byte orRegisterType = _writer.GetOrRegisterType(typeName);
			byte controllingPlayerId = GetControllingPlayerId(unit);
			Vector3 position = unit.transform.position;
			_writer.WriteEntityRegister(value, teamIndex, orRegisterType, isUnit: true, controllingPlayerId, (short)position.x, (short)position.z, (short)position.y);
			_entityController[value] = controllingPlayerId;
			return value;
		}

		private static ushort GetOrRegisterStructure(Structure structure)
		{
			int instanceID = structure.GetInstanceID();
			if (_entityIds.TryGetValue(instanceID, out var value))
			{
				return value;
			}
			value = _nextEntityId++;
			_entityIds[instanceID] = value;
			string typeName = "Unknown";
			try
			{
				typeName = ((structure.ObjectInfo != null) ? structure.ObjectInfo.DisplayName : structure.ToString());
			}
			catch
			{
				try
				{
					typeName = structure.ToString();
				}
				catch
				{
				}
			}
			Team team = null;
			try
			{
				team = structure.Team;
			}
			catch
			{
			}
			byte teamIndex = GetTeamIndex(team);
			byte orRegisterType = _writer.GetOrRegisterType(typeName);
			Vector3 position = structure.transform.position;
			_writer.WriteEntityRegister(value, teamIndex, orRegisterType, isUnit: false, 0, (short)position.x, (short)position.z, (short)position.y);
			return value;
		}

		private static void TrackControllerChange(ushort entityId, Unit unit)
		{
			byte controllingPlayerId = GetControllingPlayerId(unit);
			_entityController.TryGetValue(entityId, out var value);
			if (controllingPlayerId != value)
			{
				_entityController[entityId] = controllingPlayerId;
				_writer.WritePlayerControl(_tickNumber, entityId, controllingPlayerId);
			}
		}

		private static void ResolveTickInterval()
		{
			float num = 1f;
			try
			{
				if (_prefTickInterval != null)
				{
					num = _prefTickInterval.Value;
				}
			}
			catch
			{
			}
			float num2 = num;
			if (num2 < 0.05f)
			{
				num2 = 0.05f;
			}
			if (num2 > 10f)
			{
				num2 = 10f;
			}
			if (num2 != num)
			{
				MelonLogger.Warning($"TickIntervalSeconds {num}s out of range - using {num2}s");
			}
			_tickInterval = num2;
			_tickIntervalMs = (ushort)Math.Round(num2 * 1000f);
		}

		private static void ResolveKillLogging()
		{
			bool logAIvsAIUnitKills = true;
			try
			{
				if (_prefLogAIvsAIUnitKills != null)
				{
					logAIvsAIUnitKills = _prefLogAIvsAIUnitKills.Value;
				}
			}
			catch
			{
			}
			_logAIvsAIUnitKills = logAIvsAIUnitKills;
		}

		private static void StartRecording(GameMode? gameMode)
		{
			if (_isRecording)
			{
				StopRecording();
			}
			_entityIds.Clear();
			_nextEntityId = 1;
			_playerIds.Clear();
			_nextPlayerId = 1;
			_entityController.Clear();
			_tickNumber = 0;
			_tickClock.Reset();
			_tickOverflowLogged = false;
			_writtenTickCount = 0;
			_firstTick = true;
			_unitDestroyedCount = 0;
			_structureDestroyedCount = 0;
			_selfAttributedCount = 0;
			_openingDespawnCount = 0;
			_aiVsAiKillCount = 0;
			_aiVsAiSkippedCount = 0;
			ResolveTickInterval();
			ResolveKillLogging();
			string gameType = "";
			try
			{
				if (gameMode != null)
				{
					gameType = gameMode.ToString();
				}
			}
			catch
			{
			}
			string text = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + _currentMapName + ".srpl";
			string filePath = Path.Combine(_replayDir, text);
			try
			{
				_writer = new ReplayWriter(filePath);
				_writer.WriteHeader(_currentMapName, gameType, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), _tickIntervalMs);
				_tickClock.Restart();
				_isRecording = true;
				MelonLogger.Msg($"Recording started: {text} ({_tickIntervalMs}ms ticks)");
				if (!_logAIvsAIUnitKills)
				{
					MelonLogger.Msg("  LogAIvsAIUnitKills=false - only kills involving a player are recorded.");
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Error("Failed to start recording: " + ex.Message);
				_writer = null;
			}
		}

		private static void StopRecording()
		{
			if (!_isRecording || _writer == null)
			{
				return;
			}
			try
			{
				_writer.WriteEndOfReplay();
				_writer.Close();
				_tickClock.Stop();
				double totalSeconds = _tickClock.Elapsed.TotalSeconds;
				double num = (float)(int)_tickNumber * _tickInterval;
				MelonLogger.Msg($"Recording stopped. Ticks: {_tickNumber}, Entities: {_nextEntityId - 1}, Players: {_nextPlayerId - 1}");
				long num2 = (long)(totalSeconds / (double)_tickInterval);
				long num3 = num2 - _writtenTickCount;
				if (num3 > 0 && num2 > 0)
				{
					MelonLogger.Msg($"  {num3} of {num2} ticks skipped ({100.0 * (double)num3 / (double)num2:0.#}% - server could not keep up); " + $"round wall time {totalSeconds:0}s, last tick at {num:0}s");
				}
				MelonLogger.Msg($"  Destroyed recorded - units: {_unitDestroyedCount} ({_aiVsAiKillCount} AI-vs-AI), structures: {_structureDestroyedCount}");
				if (_aiVsAiSkippedCount > 0)
				{
					MelonLogger.Msg($"  {_aiVsAiSkippedCount} AI-vs-AI kill(s) not recorded (LogAIvsAIUnitKills=false)");
				}
				if (_unitDestroyedCount == 0 && _aiVsAiSkippedCount == 0 && _tickNumber > 100)
				{
					MelonLogger.Warning("  No unit kills recorded this round - MP_Strategy.OnUnitDestroyed may have stopped firing.");
				}
				if (_openingDespawnCount > 0)
				{
					MelonLogger.Msg($"  ({_openingDespawnCount} opening despawn(s) in the first {10f:0}s - not kills)");
				}
				if (_selfAttributedCount > 0)
				{
					MelonLogger.Warning($"  {_selfAttributedCount} mid-round kill(s) attributed to the victim itself - attacker lookup may be broken.");
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Error("Error closing replay: " + ex.Message);
			}
			_writer = null;
			_isRecording = false;
		}

		private static void WriteTickFrame()
		{
			ushort num = 0;
			for (int i = 0; i < Team.Teams.Count; i++)
			{
				Team team = Team.Teams[i];
				if (team == null)
				{
					continue;
				}
				try
				{
					List<Unit> units = team.Units;
					if (units != null)
					{
						for (int j = 0; j < units.Count; j++)
						{
							try
							{
								Unit unit = units[j];
								if (unit == null)
								{
									continue;
								}
								bool flag = false;
								try
								{
									flag = unit.IsDestroyed;
								}
								catch
								{
									try
									{
										flag = unit.DamageManager != null && unit.DamageManager.IsDestroyed;
									}
									catch
									{
									}
								}
								if (flag)
								{
									continue;
								}
								Vector3 position = unit.transform.position;
								int instanceID = unit.GetInstanceID();
								if (!_entityIds.TryGetValue(instanceID, out var value))
								{
									value = _nextEntityId++;
									_entityIds[instanceID] = value;
									string typeName = "Unknown";
									try
									{
										typeName = ((unit.ObjectInfo != null) ? unit.ObjectInfo.DisplayName : unit.ToString());
									}
									catch
									{
										try
										{
											typeName = unit.ToString();
										}
										catch
										{
										}
									}
									byte teamIndex = GetTeamIndex(team);
									byte orRegisterType = _writer.GetOrRegisterType(typeName);
									byte controllingPlayerId = GetControllingPlayerId(unit);
									_writer.WriteEntityRegister(value, teamIndex, orRegisterType, isUnit: true, controllingPlayerId, (short)position.x, (short)position.z, (short)position.y);
									_entityController[value] = controllingPlayerId;
								}
								else
								{
									TrackControllerChange(value, unit);
								}
								if (num >= _idBuf.Length)
								{
									GrowBuffers();
								}
								_idBuf[num] = value;
								_xBuf[num] = (short)position.x;
								_yBuf[num] = (short)position.z;
								_zBuf[num] = (short)position.y;
								num++;
							}
							catch (Exception ex)
							{
								MelonLogger.Warning($"Unit[{j}] error: {ex.Message}");
							}
						}
					}
				}
				catch (Exception ex2)
				{
					MelonLogger.Warning($"Team[{i}] units error: {ex2.Message}");
				}
				try
				{
					List<Structure> structures = team.Structures;
					if (structures == null)
					{
						continue;
					}
					for (int k = 0; k < structures.Count; k++)
					{
						try
						{
							Structure structure = structures[k];
							if (structure == null)
							{
								continue;
							}
							bool flag2 = false;
							try
							{
								flag2 = structure.IsDestroyed;
							}
							catch
							{
								try
								{
									flag2 = structure.DamageManager != null && structure.DamageManager.IsDestroyed;
								}
								catch
								{
								}
							}
							if (flag2)
							{
								continue;
							}
							int instanceID2 = structure.GetInstanceID();
							if (_entityIds.ContainsKey(instanceID2))
							{
								continue;
							}
							ushort num2 = _nextEntityId++;
							_entityIds[instanceID2] = num2;
							string typeName2 = "Unknown";
							try
							{
								typeName2 = ((structure.ObjectInfo != null) ? structure.ObjectInfo.DisplayName : structure.ToString());
							}
							catch
							{
								try
								{
									typeName2 = structure.ToString();
								}
								catch
								{
								}
							}
							byte teamIndex2 = GetTeamIndex(team);
							byte orRegisterType2 = _writer.GetOrRegisterType(typeName2);
							Vector3 position2 = structure.transform.position;
							_writer.WriteEntityRegister(num2, teamIndex2, orRegisterType2, isUnit: false, 0, (short)position2.x, (short)position2.z, (short)position2.y);
						}
						catch (Exception ex3)
						{
							MelonLogger.Warning($"Structure[{k}] error: {ex3.Message}");
						}
					}
				}
				catch (Exception ex4)
				{
					MelonLogger.Warning($"Team[{i}] structures error: {ex4.Message}");
				}
			}
			_writer.WriteTickFrame(_tickNumber, num, _idBuf, _xBuf, _yBuf, _zBuf);
		}

		private static byte GetTeamIndex(Team? team)
		{
			if (team == null)
			{
				return byte.MaxValue;
			}
			try
			{
				return (byte)team.Index;
			}
			catch
			{
				return byte.MaxValue;
			}
		}

		private static void GrowBuffers()
		{
			int newSize = _idBuf.Length * 2;
			Array.Resize(ref _idBuf, newSize);
			Array.Resize(ref _xBuf, newSize);
			Array.Resize(ref _yBuf, newSize);
			Array.Resize(ref _zBuf, newSize);
		}
	}
}
