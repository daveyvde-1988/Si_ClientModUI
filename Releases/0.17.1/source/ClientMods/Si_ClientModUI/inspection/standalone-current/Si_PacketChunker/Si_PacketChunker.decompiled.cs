using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using HarmonyLib;
using MelonLoader;
using Si_PacketChunker;
using Silica.AI;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(PacketChunker), "Packet Chunker", "1.1.0", "schwe", null)]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyVersion("0.0.0.0")]
namespace Si_PacketChunker;

public class PacketChunker : MelonMod
{
	private class Section
	{
		public int Count;

		public Func<int, int> Size;

		public Action<int, GameByteStreamWriter> Write;

		public string Name;

		public int Next;
	}

	[HarmonyPatch(typeof(AIGroup), "SendNetInitAll")]
	private static class Patch_AIGroup_SendNetInitAll
	{
		private static bool Prefix(Player player, ref bool __result)
		{
			try
			{
				if (!Game.GetIsServer())
				{
					__result = false;
					return false;
				}
				List<AIGroup> all = new List<AIGroup>(AIGroup.ActiveGroups.Values);
				int num = SendChunked(player, 37, "AIGroup::SendNetInit", 0, GroupSections(all, _noChanges, _noIgnores, _noIds), (int i) => all[i].ID);
				if (num > 1)
				{
					Log.Msg(string.Format("AIGroup init for {0}: {1} groups in {2} packets", player?.PlayerName ?? "all", all.Count, num));
				}
				__result = true;
				return false;
			}
			catch (Exception arg)
			{
				Log.Error($"AIGroup.SendNetInitAll chunking failed, falling back to vanilla: {arg}");
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(AIGroup), "SendNetUpdateAll")]
	private static class Patch_AIGroup_SendNetUpdateAll
	{
		private static bool Prefix(Player player)
		{
			try
			{
				if (!Game.GetIsServer())
				{
					return false;
				}
				if (_createdGroups.Count == 0 && _groupChanges.Count == 0 && _ignoreTaskChanges.Count == 0 && _removedGroupIDs.Count == 0)
				{
					return false;
				}
				List<AIGroup> created = new List<AIGroup>(_createdGroups);
				List<(AIGroup, AIGroup.EAIGroupChange, uint)> changes = new List<(AIGroup, AIGroup.EAIGroupChange, uint)>(_groupChanges);
				List<(AIGroup, uint, AIGroup.EIgnoreTaskReason)> ignores = new List<(AIGroup, uint, AIGroup.EIgnoreTaskReason)>(_ignoreTaskChanges);
				List<uint> removed = new List<uint>(_removedGroupIDs);
				_createdGroups.Clear();
				_groupChanges.Clear();
				_ignoreTaskChanges.Clear();
				_removedGroupIDs.Clear();
				SendChunked(player, 37, "AIGroup::SendNetInit", 1, GroupSections(created, changes, ignores, removed), (int i) => created[i].ID);
				return false;
			}
			catch (Exception arg)
			{
				Log.Error($"AIGroup.SendNetUpdateAll chunking failed, falling back to vanilla: {arg}");
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(AITaskBase), "SendNetInitAll")]
	private static class Patch_AITask_SendNetInitAll
	{
		private static bool Prefix(Player player, ref bool __result)
		{
			try
			{
				if (!Game.GetIsServer())
				{
					__result = false;
					return false;
				}
				List<AITaskBase> all = new List<AITaskBase>(AITaskBase.ActiveTasks);
				int num = SendChunked(player, 38, "AITaskBase::SendNetInitAll", 0, TaskSections(all, _noIds), (int i) => all[i].ID);
				if (num > 1)
				{
					Log.Msg(string.Format("AITask init for {0}: {1} tasks in {2} packets", player?.PlayerName ?? "all", all.Count, num));
				}
				__result = true;
				return false;
			}
			catch (Exception arg)
			{
				Log.Error($"AITaskBase.SendNetInitAll chunking failed, falling back to vanilla: {arg}");
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(AITaskBase), "SendNetUpdateAll")]
	private static class Patch_AITask_SendNetUpdateAll
	{
		private static bool Prefix(Player player, ref bool __result)
		{
			try
			{
				if (!Game.GetIsServer())
				{
					__result = false;
					return false;
				}
				if (_createdTasks.Count == 0 && _removedTaskIDs.Count == 0)
				{
					__result = false;
					return false;
				}
				List<AITaskBase> created = new List<AITaskBase>(_createdTasks);
				List<uint> removed = new List<uint>(_removedTaskIDs);
				_createdTasks.Clear();
				_removedTaskIDs.Clear();
				SendChunked(player, 38, "AITaskBase::SendNetInitAll", 1, TaskSections(created, removed), (int i) => created[i].ID);
				__result = true;
				return false;
			}
			catch (Exception arg)
			{
				Log.Error($"AITaskBase.SendNetUpdateAll chunking failed, falling back to vanilla: {arg}");
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(NetworkLayer), "SendTeamsInit")]
	private static class Patch_SendTeamsInit
	{
		private static readonly List<(Team team, List<Target> original)> _swapped = new List<(Team, List<Target>)>();

		private static void Prefix()
		{
			_swapped.Clear();
			try
			{
				if (!Game.GetIsMultiplayer())
				{
					return;
				}
				List<Team> teams = Team.Teams;
				int num = 2356 - 32 * teams.Count;
				int num2 = 0;
				for (int i = 0; i < teams.Count; i++)
				{
					List<Target> detectedTargets = teams[i].DetectedTargets;
					for (int j = 0; j < detectedTargets.Count; j++)
					{
						num2 += TargetSize(detectedTargets[j]);
					}
				}
				if (num2 <= num)
				{
					return;
				}
				int[] array = new int[teams.Count];
				int num3 = num;
				for (int k = 0; k < 2; k++)
				{
					for (int l = 0; l < teams.Count; l++)
					{
						bool flag = teams[l].GetNumPlayers() > 0;
						if (k == 0 != flag)
						{
							continue;
						}
						List<Target> detectedTargets2 = teams[l].DetectedTargets;
						int m = 0;
						int num4 = 0;
						for (; m < detectedTargets2.Count; m++)
						{
							int num5 = TargetSize(detectedTargets2[m]);
							if (num4 + num5 > num3)
							{
								break;
							}
							num4 += num5;
						}
						array[l] = m;
						num3 -= num4;
					}
				}
				StringBuilder stringBuilder = new StringBuilder();
				for (int n = 0; n < teams.Count; n++)
				{
					List<Target> detectedTargets3 = teams[n].DetectedTargets;
					if (array[n] < detectedTargets3.Count)
					{
						_swapped.Add((teams[n], detectedTargets3));
						_setDetectedTargets.Invoke(teams[n], new object[1] { detectedTargets3.GetRange(0, array[n]) });
						stringBuilder.Append($" {teams[n].TeamShortName}:{array[n]}/{detectedTargets3.Count}");
					}
				}
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				if (realtimeSinceStartup - _lastTeamTrimLog > 10f)
				{
					_lastTeamTrimLog = realtimeSinceStartup;
					Log.Msg($"TeamInit trimmed ({num2}b of detected targets > {num}b budget):{stringBuilder}");
				}
			}
			catch (Exception arg)
			{
				Log.Error($"SendTeamsInit trim failed: {arg}");
				Restore();
			}
		}

		private static void Postfix()
		{
			Restore();
		}

		private static void Restore()
		{
			for (int i = 0; i < _swapped.Count; i++)
			{
				try
				{
					_setDetectedTargets.Invoke(_swapped[i].team, new object[1] { _swapped[i].original });
				}
				catch (Exception arg)
				{
					Log.Error($"restore DetectedTargets failed: {arg}");
				}
			}
			_swapped.Clear();
		}
	}

	private const int MAX_PACKET = 2400;

	private const int BUDGET = 2360;

	private const int SZ_PU32 = 6;

	private const int SZ_BYTE = 2;

	private const int SZ_BOOL = 2;

	private const byte PKT_AIGROUP = 37;

	private const byte PKT_AITASK = 38;

	private static Instance Log;

	private static GameByteStreamWriter _scratch;

	private static Action<AIGroup, GameByteStreamWriter> _groupWriteData;

	private static List<AIGroup> _createdGroups;

	private static List<(AIGroup group, AIGroup.EAIGroupChange changeType, uint unitID)> _groupChanges;

	private static List<(AIGroup group, uint unitID, AIGroup.EIgnoreTaskReason reason)> _ignoreTaskChanges;

	private static List<uint> _removedGroupIDs;

	private static Action<AITaskBase, GameByteStreamWriter> _taskWriteGroups;

	private static List<AITaskBase> _createdTasks;

	private static List<uint> _removedTaskIDs;

	private static MethodInfo _setDetectedTargets;

	private static readonly HashSet<uint> _oversizedWarned = new HashSet<uint>();

	private static float _lastTeamTrimLog = -999f;

	private static readonly List<(AIGroup group, AIGroup.EAIGroupChange changeType, uint unitID)> _noChanges = new List<(AIGroup, AIGroup.EAIGroupChange, uint)>();

	private static readonly List<(AIGroup group, uint unitID, AIGroup.EIgnoreTaskReason reason)> _noIgnores = new List<(AIGroup, uint, AIGroup.EIgnoreTaskReason)>();

	private static readonly List<uint> _noIds = new List<uint>();

	public override void OnInitializeMelon()
	{
		Log = ((MelonBase)this).LoggerInstance;
		try
		{
			_scratch = new GameByteStreamWriter();
			AccessTools.FieldRefAccess<GameByteStreamWriter, bool>("m_WriteDebug").Invoke(_scratch) = true;
			AccessTools.FieldRefAccess<GameByteStreamWriter, string>("ForFunction").Invoke(_scratch) = "PacketChunker::Measure";
			MethodInfo method = AccessTools.Method(typeof(AIGroup), "WriteData", new Type[1] { typeof(GameByteStreamWriter) }, (Type[])null);
			_groupWriteData = (Action<AIGroup, GameByteStreamWriter>)Delegate.CreateDelegate(typeof(Action<AIGroup, GameByteStreamWriter>), method);
			_createdGroups = (List<AIGroup>)AccessTools.Property(typeof(AIGroup), "CreatedGroups").GetValue(null);
			_groupChanges = (List<(AIGroup, AIGroup.EAIGroupChange, uint)>)AccessTools.Property(typeof(AIGroup), "GroupChanges").GetValue(null);
			_ignoreTaskChanges = (List<(AIGroup, uint, AIGroup.EIgnoreTaskReason)>)AccessTools.Property(typeof(AIGroup), "IgnoreTaskChanges").GetValue(null);
			_removedGroupIDs = (List<uint>)AccessTools.Property(typeof(AIGroup), "RemovedGroupIDs").GetValue(null);
			MethodInfo method2 = AccessTools.Method(typeof(AITaskBase), "WriteGroups", new Type[1] { typeof(GameByteStreamWriter) }, (Type[])null);
			_taskWriteGroups = (Action<AITaskBase, GameByteStreamWriter>)Delegate.CreateDelegate(typeof(Action<AITaskBase, GameByteStreamWriter>), method2);
			_createdTasks = (List<AITaskBase>)AccessTools.Property(typeof(AITaskBase), "CreatedTasks").GetValue(null);
			_removedTaskIDs = (List<uint>)AccessTools.Property(typeof(AITaskBase), "RemovedTaskIDs").GetValue(null);
			_setDetectedTargets = AccessTools.PropertySetter(typeof(Team), "DetectedTargets");
			if (_createdGroups == null || _groupChanges == null || _ignoreTaskChanges == null || _removedGroupIDs == null || _createdTasks == null || _removedTaskIDs == null || _setDetectedTargets == null)
			{
				throw new Exception("private member lookup failed");
			}
			((MelonBase)this).HarmonyInstance.PatchAll(typeof(PacketChunker).Assembly);
			Log.Msg($"Packet Chunker v1.1.0 active: AIGroup + AITask init/update chunked, TeamInit trimmed to {2360}b");
		}
		catch (Exception arg)
		{
			Log.Error($"Init failed, mod inactive (game build changed?): {arg}");
		}
	}

	private static int Measure(Action<GameByteStreamWriter> write)
	{
		_scratch.Reset();
		write(_scratch);
		return _scratch.GetByteDataSize();
	}

	private static GameByteStreamWriter NewPacket(byte packetId, string name, byte type)
	{
		GameByteStreamWriter gameByteStreamWriter = GameByteStreamWriter.GetGameByteStreamWriter(0u, name);
		if (gameByteStreamWriter == null)
		{
			return null;
		}
		gameByteStreamWriter.WriteByte(packetId);
		gameByteStreamWriter.WriteByte(type);
		return gameByteStreamWriter;
	}

	private static bool Send(GameByteStreamWriter w, Player player, string name)
	{
		int byteDataSize = w.GetByteDataSize();
		if (byteDataSize > 2400)
		{
			Log.Warning($"{name}: packet {byteDataSize}b > {2400}b despite chunking");
		}
		if (!(player == null))
		{
			return NetworkLayer.SendPacketToPlayer(w, ENetworkPacketSend.Reliable, player);
		}
		return NetworkLayer.SendPacket(w, ENetworkPacketSend.Reliable);
	}

	private static int SendChunked(Player player, byte packetId, string name, byte firstType, Section[] sections, Func<int, uint> idOfFirstSectionItem)
	{
		int num = 0;
		byte b = firstType;
		bool flag = true;
		while (flag || AnyLeft(sections))
		{
			flag = false;
			int num2 = 4 + 6 * sections.Length;
			int num3 = ((b == 0) ? 1 : sections.Length);
			int[] array = new int[sections.Length];
			for (int i = 0; i < num3; i++)
			{
				Section section = sections[i];
				while (section.Next + array[i] < section.Count)
				{
					int arg = section.Next + array[i];
					int num4 = section.Size(arg);
					if (num4 > 2356 - 6 * sections.Length)
					{
						if (array[i] > 0)
						{
							break;
						}
						uint num5 = ((i == 0 && idOfFirstSectionItem != null) ? idOfFirstSectionItem(arg) : uint.MaxValue);
						if (_oversizedWarned.Add(num5 ^ (uint)(packetId << 24)))
						{
							Log.Warning($"{name}: {section.Name} item id {num5} is {num4}b and cannot fit a {2400}b packet, not replicated");
						}
						section.Next++;
					}
					else
					{
						if (num2 + num4 > 2360)
						{
							break;
						}
						num2 += num4;
						array[i]++;
					}
				}
				if (section.Next + array[i] < section.Count)
				{
					break;
				}
			}
			GameByteStreamWriter gameByteStreamWriter = NewPacket(packetId, name, b);
			if (gameByteStreamWriter == null)
			{
				return num;
			}
			for (int j = 0; j < num3; j++)
			{
				Section section2 = sections[j];
				gameByteStreamWriter.WritePackedUInt32((uint)array[j]);
				for (int k = 0; k < array[j]; k++)
				{
					section2.Write(section2.Next + k, gameByteStreamWriter);
				}
				section2.Next += array[j];
			}
			Send(gameByteStreamWriter, player, name);
			num++;
			b = 1;
			if (num > 1024)
			{
				Log.Error(name + ": chunking runaway, aborting");
				break;
			}
		}
		return num;
	}

	private static bool AnyLeft(Section[] sections)
	{
		for (int i = 0; i < sections.Length; i++)
		{
			if (sections[i].Next < sections[i].Count)
			{
				return true;
			}
		}
		return false;
	}

	private static Section[] GroupSections(List<AIGroup> created, List<(AIGroup group, AIGroup.EAIGroupChange changeType, uint unitID)> changes, List<(AIGroup group, uint unitID, AIGroup.EIgnoreTaskReason reason)> ignores, List<uint> removed)
	{
		return new Section[4]
		{
			new Section
			{
				Name = "group",
				Count = created.Count,
				Size = (int i) => Measure((GameByteStreamWriter w) =>
				{
					_groupWriteData(created[i], w);
				}),
				Write = (int i, GameByteStreamWriter w) =>
				{
					_groupWriteData(created[i], w);
				}
			},
			new Section
			{
				Name = "change",
				Count = changes.Count,
				Size = (int i) => 14,
				Write = (int i, GameByteStreamWriter w) =>
				{
					(AIGroup, AIGroup.EAIGroupChange, uint) tuple = changes[i];
					w.WritePackedUInt32(tuple.Item1.ID);
					w.WriteByte((byte)tuple.Item2);
					w.WritePackedUInt32(tuple.Item3);
				}
			},
			new Section
			{
				Name = "ignoreTask",
				Count = ignores.Count,
				Size = (int i) => 14,
				Write = (int i, GameByteStreamWriter w) =>
				{
					(AIGroup, uint, AIGroup.EIgnoreTaskReason) tuple = ignores[i];
					w.WritePackedUInt32(tuple.Item1.ID);
					w.WritePackedUInt32(tuple.Item2);
					w.WriteByte((byte)tuple.Item3);
				}
			},
			new Section
			{
				Name = "removed",
				Count = removed.Count,
				Size = (int i) => 6,
				Write = (int i, GameByteStreamWriter w) =>
				{
					w.WritePackedUInt32(removed[i]);
				}
			}
		};
	}

	private static void WriteTask(AITaskBase t, GameByteStreamWriter w)
	{
		t.WriteData(w);
		_taskWriteGroups(t, w);
	}

	private static Section[] TaskSections(List<AITaskBase> created, List<uint> removed)
	{
		return new Section[2]
		{
			new Section
			{
				Name = "task",
				Count = created.Count,
				Size = (int i) => Measure((GameByteStreamWriter w) =>
				{
					WriteTask(created[i], w);
				}),
				Write = (int i, GameByteStreamWriter w) =>
				{
					WriteTask(created[i], w);
				}
			},
			new Section
			{
				Name = "removed",
				Count = removed.Count,
				Size = (int i) => 6,
				Write = (int i, GameByteStreamWriter w) =>
				{
					w.WritePackedUInt32(removed[i]);
				}
			}
		};
	}

	private static int PackedSize(uint v)
	{
		if (v <= 240)
		{
			return 2;
		}
		if (v <= 2287)
		{
			return 3;
		}
		if (v <= 67823)
		{
			return 4;
		}
		if (v <= 16777215)
		{
			return 5;
		}
		return 6;
	}

	private static int TargetSize(Target t)
	{
		uint v = 0u;
		try
		{
			if ((bool)t && (bool)t.NetworkComponent && t.NetworkComponent.IsValid)
			{
				v = t.NetworkComponent.NetID;
			}
		}
		catch
		{
		}
		return PackedSize(v);
	}
}
