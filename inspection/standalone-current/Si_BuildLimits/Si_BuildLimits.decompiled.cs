using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using MelonLoader;
using MelonLoader.Preferences;
using MelonLoader.Utils;
using Microsoft.CodeAnalysis;
using Si_BuildLimits;
using SilicaAdminMod;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(BuildLimits), "Build Limits", "1.2.1", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_BuildLimits")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_BuildLimits")]
[assembly: AssemblyTitle("Si_BuildLimits")]
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
namespace Si_BuildLimits
{
	public class BuildLimits : MelonMod
	{
		public enum EObjectComparison
		{
			ObjectInfoExact,
			StructureTypeComparison,
			StructureSelectionTypeComparison,
			UnitTypeComparison,
			StructureSelectionTypeComparisonSimilarDefense
		}

		public enum EDefenseRating
		{
			RadarStation = 0,
			Human_AntiAir = 1,
			HiveSpire = 2,
			Human_Turret = 3,
			Human_HeavyTurret = 4,
			ThornSpire = Human_HeavyTurret
		}

		private const int nolimit = -1;

		private static MelonPreferences_Category _modCategory;

		private static MelonPreferences_Entry<bool> _Pref_Block_BotCommanders;

		private static MelonPreferences_Entry<bool> _Pref_Check_UnderConstruction;

		private static MelonPreferences_Entry<int> _Pref_Limit_Bases_Humans;

		private static MelonPreferences_Entry<int> _Pref_Limit_Bases_Aliens;

		private static MelonPreferences_Entry<int> _Pref_Limit_Turrets_Humans;

		private static MelonPreferences_Entry<int> _Pref_Limit_Turrets_Aliens;

		private static MelonPreferences_Entry<int> _Pref_Limit_Research_Humans;

		private static MelonPreferences_Entry<int> _Pref_Limit_Research_Alien;

		private static MelonPreferences_Entry<int> _Pref_Limit_Radar;

		private static MelonPreferences_Entry<int> _Pref_Limit_Nodes;

		private static MelonPreferences_Entry<int> _Pref_Limit_Silos;

		private static MelonPreferences_Entry<int> _Pref_Limit_Deposits_Humans;

		private static MelonPreferences_Entry<int> _Pref_Limit_Deposits_Aliens;

		private static MelonPreferences_Entry<int> _Pref_Limit_Prod1_Humans;

		private static MelonPreferences_Entry<int> _Pref_Limit_Prod1_Aliens;

		private static MelonPreferences_Entry<int> _Pref_Limit_Prod2_Humans;

		private static MelonPreferences_Entry<int> _Pref_Limit_Prod2_Aliens;

		private static MelonPreferences_Entry<int> _Pref_Limit_Prod3_Humans;

		private static MelonPreferences_Entry<int> _Pref_Limit_Prod3_Aliens;

		private static MelonPreferences_Entry<int> _Pref_Limit_Prod4_Humans;

		private static MelonPreferences_Entry<int> _Pref_Limit_Prod4_Aliens;

		private static MelonPreferences_Entry<int> _Pref_Limit_Prod5_Humans;

		private static MelonPreferences_Entry<int> _Pref_Limit_Prod5_Aliens;

		private static MelonPreferences_Entry<int> _Pref_Limit_Aircraft_Siege_Alien;

		private static MelonPreferences_Entry<int> _Pref_Limit_Aircraft_Siege_Human;

		private static MelonPreferences_Entry<int> _Pref_Limit_Heavy_Siege_Human;

		private static MelonPreferences_Entry<int> _Pref_Limit_Heavy_Tank_Human;

		public override void OnInitializeMelon()
		{
			if (_modCategory == null)
			{
				_modCategory = MelonPreferences.CreateCategory("Silica");
			}
			if (_Pref_Block_BotCommanders == null)
			{
				_Pref_Block_BotCommanders = _modCategory.CreateEntry<bool>("BuildLimits_EnforceLimitsOnAI", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Check_UnderConstruction == null)
			{
				_Pref_Check_UnderConstruction = _modCategory.CreateEntry<bool>("BuildLimits_CheckUnderConstruction", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Bases_Humans == null)
			{
				_Pref_Limit_Bases_Humans = _modCategory.CreateEntry<int>("BuildLimits_Humans_Bases", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Bases_Aliens == null)
			{
				_Pref_Limit_Bases_Aliens = _modCategory.CreateEntry<int>("BuildLimits_Aliens_Bases", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Turrets_Humans == null)
			{
				_Pref_Limit_Turrets_Humans = _modCategory.CreateEntry<int>("BuildLimits_Humans_Turrets", 15, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Radar == null)
			{
				_Pref_Limit_Radar = _modCategory.CreateEntry<int>("BuildLimits_Humans_RadarStations", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Turrets_Aliens == null)
			{
				_Pref_Limit_Turrets_Aliens = _modCategory.CreateEntry<int>("BuildLimits_Aliens_Turrets", 25, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Research_Humans == null)
			{
				_Pref_Limit_Research_Humans = _modCategory.CreateEntry<int>("BuildLimits_Humans_Research", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Research_Alien == null)
			{
				_Pref_Limit_Research_Alien = _modCategory.CreateEntry<int>("BuildLimits_Aliens_Research", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Nodes == null)
			{
				_Pref_Limit_Nodes = _modCategory.CreateEntry<int>("BuildLimits_Nodes", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Silos == null)
			{
				_Pref_Limit_Silos = _modCategory.CreateEntry<int>("BuildLimits_Silos", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Deposits_Humans == null)
			{
				_Pref_Limit_Deposits_Humans = _modCategory.CreateEntry<int>("BuildLimits_Humans_Deposits", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Deposits_Aliens == null)
			{
				_Pref_Limit_Deposits_Aliens = _modCategory.CreateEntry<int>("BuildLimits_Aliens_Deposits", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Prod1_Humans == null)
			{
				_Pref_Limit_Prod1_Humans = _modCategory.CreateEntry<int>("BuildLimits_Humans_Production_L1", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Prod1_Aliens == null)
			{
				_Pref_Limit_Prod1_Aliens = _modCategory.CreateEntry<int>("BuildLimits_Aliens_Production_L1", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Prod2_Humans == null)
			{
				_Pref_Limit_Prod2_Humans = _modCategory.CreateEntry<int>("BuildLimits_Humans_Production_L2", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Prod2_Aliens == null)
			{
				_Pref_Limit_Prod2_Aliens = _modCategory.CreateEntry<int>("BuildLimits_Aliens_Production_L2", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Prod3_Humans == null)
			{
				_Pref_Limit_Prod3_Humans = _modCategory.CreateEntry<int>("BuildLimits_Humans_Production_L3", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Prod3_Aliens == null)
			{
				_Pref_Limit_Prod3_Aliens = _modCategory.CreateEntry<int>("BuildLimits_Aliens_Production_L3", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Prod4_Humans == null)
			{
				_Pref_Limit_Prod4_Humans = _modCategory.CreateEntry<int>("BuildLimits_Humans_Production_L4", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Prod4_Aliens == null)
			{
				_Pref_Limit_Prod4_Aliens = _modCategory.CreateEntry<int>("BuildLimits_Aliens_Production_L4", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Prod5_Humans == null)
			{
				_Pref_Limit_Prod5_Humans = _modCategory.CreateEntry<int>("BuildLimits_Humans_Production_L5", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Prod5_Aliens == null)
			{
				_Pref_Limit_Prod5_Aliens = _modCategory.CreateEntry<int>("BuildLimits_Aliens_Production_L5", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Aircraft_Siege_Alien == null)
			{
				_Pref_Limit_Aircraft_Siege_Alien = _modCategory.CreateEntry<int>("BuildLimits_Aliens_SiegeAircraft", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Aircraft_Siege_Human == null)
			{
				_Pref_Limit_Aircraft_Siege_Human = _modCategory.CreateEntry<int>("BuildLimits_Humans_SiegeAircraft", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Heavy_Siege_Human == null)
			{
				_Pref_Limit_Heavy_Siege_Human = _modCategory.CreateEntry<int>("BuildLimits_Humans_HeavySiege", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Pref_Limit_Heavy_Tank_Human == null)
			{
				_Pref_Limit_Heavy_Tank_Human = _modCategory.CreateEntry<int>("BuildLimits_Humans_HeavyTank", -1, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
		}

		public override void OnLateInitializeMelon()
		{
			Event_Construction.OnRequestBuildStructure += OnRequestBuildStructure_LimitCheck;
			Event_Construction.OnRequestBuildUnit += OnRequestBuildUnit_LimitCheck;
		}

		public void NotifyLimitsEnforced(Team team, int maxAmount, string type, bool structure = true)
		{
			Player player = null;
			if (GameMode.CurrentGameMode is GameModeExt gameModeExt)
			{
				player = gameModeExt.GetCommanderForTeam(team);
			}
			if (player != null)
			{
				string arg = (structure ? "structure" : "unit");
				string text = $"{type} {arg} limit ({maxAmount}) exceeded." + ((maxAmount > 0) ? ((structure ? "Sell" : "Destroy") + " more before purchasing.") : "");
				HelperMethods.SendConsoleMessageToPlayer(player, text);
				HelperMethods.SendChatMessageToTeam(team, text);
				AudioHelper.PlaySoundFile(Path.Combine(MelonEnvironment.UserDataDirectory, "sounds\\access_denied.wav"), player);
			}
		}

		public void OnRequestBuildStructure_LimitCheck(object? sender, OnRequestBuildArgs args)
		{
			try
			{
				if (args == null || (!_Pref_Block_BotCommanders.Value && !args.PlayerInitiated))
				{
					return;
				}
				ConstructionData constructionData = args.ConstructionData;
				Structure parentStructure = args.ParentStructure;
				if (!constructionData || !constructionData.IsStructure || !constructionData.ObjectToBuild || !parentStructure || !parentStructure.Team)
				{
					return;
				}
				if (_Pref_Limit_Nodes.Value > -1 && constructionData.ObjectInfo.StructureType == StructureType.Resource && !constructionData.ObjectInfo.HasResourceDeposit && !constructionData.ObjectInfo.HasResourceStorage)
				{
					if (GetStructureCount(parentStructure.Team, constructionData) >= _Pref_Limit_Nodes.Value)
					{
						NotifyLimitsEnforced(parentStructure.Team, _Pref_Limit_Nodes.Value, "Node");
						args.Block = true;
					}
				}
				else if (constructionData.ObjectInfo.StructureType == StructureType.Defense)
				{
					if (constructionData.ObjectInfo.DefenseRating > 0)
					{
						int num = ((parentStructure.Team.Index == 0) ? _Pref_Limit_Turrets_Aliens.Value : _Pref_Limit_Turrets_Humans.Value);
						if (num > -1 && GetStructureCount(parentStructure.Team, constructionData, EObjectComparison.StructureSelectionTypeComparisonSimilarDefense) >= num)
						{
							NotifyLimitsEnforced(parentStructure.Team, num, "Turret");
							args.Block = true;
						}
					}
					else
					{
						int value = _Pref_Limit_Radar.Value;
						if (value > -1 && GetStructureCount(parentStructure.Team, constructionData, EObjectComparison.StructureSelectionTypeComparisonSimilarDefense) >= value)
						{
							NotifyLimitsEnforced(parentStructure.Team, value, "Radar Station");
							args.Block = true;
						}
					}
				}
				else if (constructionData.ObjectInfo.StructureType == StructureType.Production && constructionData.ObjectInfo.StructureSelectionType != StructureSelectionType.HQ)
				{
					if (constructionData.ObjectInfo.StructureSelectionType == StructureSelectionType.Units1)
					{
						int num2 = ((parentStructure.Team.Index == 0) ? _Pref_Limit_Prod1_Aliens.Value : _Pref_Limit_Prod1_Humans.Value);
						if (num2 > -1 && GetStructureCount(parentStructure.Team, constructionData, EObjectComparison.StructureSelectionTypeComparison) >= num2)
						{
							NotifyLimitsEnforced(parentStructure.Team, num2, (parentStructure.Team.Index == 0) ? "Lesser Spawning Cyst" : "Barracks");
							args.Block = true;
						}
					}
					else if (constructionData.ObjectInfo.StructureSelectionType == StructureSelectionType.Units2)
					{
						int num3 = ((parentStructure.Team.Index == 0) ? _Pref_Limit_Prod2_Aliens.Value : _Pref_Limit_Prod2_Humans.Value);
						if (num3 > -1 && GetStructureCount(parentStructure.Team, constructionData, EObjectComparison.StructureSelectionTypeComparison) >= num3)
						{
							NotifyLimitsEnforced(parentStructure.Team, num3, (parentStructure.Team.Index == 0) ? "Greater Spawning Cyst" : "Light Vehicle Factory");
							args.Block = true;
						}
					}
					else if (constructionData.ObjectInfo.StructureSelectionType == StructureSelectionType.Units3)
					{
						int num4 = ((parentStructure.Team.Index == 0) ? _Pref_Limit_Prod3_Aliens.Value : _Pref_Limit_Prod3_Humans.Value);
						if (num4 > -1 && GetStructureCount(parentStructure.Team, constructionData, EObjectComparison.StructureSelectionTypeComparison) >= num4)
						{
							NotifyLimitsEnforced(parentStructure.Team, num4, (parentStructure.Team.Index == 0) ? "Grand Spawning Cyst" : "Heavy Vehicle Factory");
							args.Block = true;
						}
					}
					else if (constructionData.ObjectInfo.StructureSelectionType == StructureSelectionType.Units4)
					{
						int num5 = ((parentStructure.Team.Index == 0) ? _Pref_Limit_Prod4_Aliens.Value : _Pref_Limit_Prod4_Humans.Value);
						if (num5 > -1 && GetStructureCount(parentStructure.Team, constructionData, EObjectComparison.StructureSelectionTypeComparison) >= num5)
						{
							NotifyLimitsEnforced(parentStructure.Team, num5, (parentStructure.Team.Index == 0) ? "Colossal Spawning Cyst" : "Ultra Heavy Vehicle Factory");
							args.Block = true;
						}
					}
					else if (constructionData.ObjectInfo.StructureSelectionType == StructureSelectionType.Units5)
					{
						int num6 = ((parentStructure.Team.Index == 0) ? _Pref_Limit_Prod5_Aliens.Value : _Pref_Limit_Prod5_Humans.Value);
						if (num6 > -1 && GetStructureCount(parentStructure.Team, constructionData, EObjectComparison.StructureSelectionTypeComparison) >= num6)
						{
							NotifyLimitsEnforced(parentStructure.Team, num6, (parentStructure.Team.Index == 0) ? "tbd" : "Air Factory");
							args.Block = true;
						}
					}
				}
				else if (constructionData.ObjectInfo.StructureType == StructureType.Resource && constructionData.ObjectInfo.HasResourceDeposit && constructionData.ObjectInfo.HasResourceStorage)
				{
					int num7 = ((parentStructure.Team.Index == 0) ? _Pref_Limit_Deposits_Aliens.Value : _Pref_Limit_Deposits_Humans.Value);
					if (num7 > -1 && GetStructureCount(parentStructure.Team, constructionData) >= num7)
					{
						NotifyLimitsEnforced(parentStructure.Team, num7, (parentStructure.Team.Index == 0) ? "BioCache" : "Refinery");
						args.Block = true;
					}
				}
				else if (constructionData.ObjectInfo.StructureType == StructureType.Research)
				{
					int num8 = ((parentStructure.Team.Index == 0) ? _Pref_Limit_Research_Alien.Value : _Pref_Limit_Research_Humans.Value);
					if (num8 > -1 && GetStructureCount(parentStructure.Team, constructionData, EObjectComparison.StructureTypeComparison) >= num8)
					{
						NotifyLimitsEnforced(parentStructure.Team, num8, "Research");
						args.Block = true;
					}
				}
				else if (_Pref_Limit_Silos.Value > -1 && constructionData.ObjectInfo.StructureType == StructureType.Resource && !constructionData.ObjectInfo.HasResourceDeposit && constructionData.ObjectInfo.HasResourceStorage)
				{
					if (GetStructureCount(parentStructure.Team, constructionData) >= _Pref_Limit_Silos.Value)
					{
						NotifyLimitsEnforced(parentStructure.Team, _Pref_Limit_Silos.Value, "Silo");
						args.Block = true;
					}
				}
				else if ((bool)parentStructure.Team.BaseStructure && parentStructure.Team.BaseStructure == constructionData.ObjectInfo)
				{
					int num9 = ((parentStructure.Team.Index == 0) ? _Pref_Limit_Bases_Aliens.Value : _Pref_Limit_Bases_Humans.Value);
					if (num9 > -1 && GetStructureCount(parentStructure.Team, constructionData) >= num9)
					{
						NotifyLimitsEnforced(parentStructure.Team, num9, (parentStructure.Team.Index == 0) ? "Nest" : "Headquarters");
						args.Block = true;
					}
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run OnRequestBuildStructure_LimitCheck");
			}
		}

		public void OnRequestBuildUnit_LimitCheck(object? sender, OnRequestBuildArgs args)
		{
			try
			{
				if (args == null || (!_Pref_Block_BotCommanders.Value && !args.PlayerInitiated))
				{
					return;
				}
				ConstructionData constructionData = args.ConstructionData;
				Structure parentStructure = args.ParentStructure;
				if (!constructionData || !constructionData.IsUnit || !constructionData.ObjectToBuild || !parentStructure || !parentStructure.Team)
				{
					return;
				}
				if (constructionData.ObjectInfo.UnitType == UnitType.Aircraft && constructionData.ObjectInfo.UnitWeight == UnitWeight.Heavy && constructionData.ObjectInfo.OffenseRating == 5)
				{
					int num = ((parentStructure.Team.Index == 0) ? _Pref_Limit_Aircraft_Siege_Alien.Value : _Pref_Limit_Aircraft_Siege_Human.Value);
					if (num > -1 && GetUnitCount(parentStructure.Team, constructionData, EObjectComparison.UnitTypeComparison) >= num)
					{
						NotifyLimitsEnforced(parentStructure.Team, num, "Siege Aircraft");
						args.Block = true;
					}
				}
				else if (constructionData.ObjectInfo.UnitType == UnitType.Siege && constructionData.ObjectInfo.UnitWeight == UnitWeight.Heavy && constructionData.ObjectInfo.OffenseRating == 5)
				{
					if (parentStructure.Team.Index != 0)
					{
						int value = _Pref_Limit_Heavy_Siege_Human.Value;
						if (value > -1 && GetUnitCount(parentStructure.Team, constructionData, EObjectComparison.UnitTypeComparison) >= value)
						{
							NotifyLimitsEnforced(parentStructure.Team, value, "Heavy Siege");
							args.Block = true;
						}
					}
				}
				else if (constructionData.ObjectInfo.UnitType == UnitType.Tank && constructionData.ObjectInfo.UnitWeight == UnitWeight.Heavy && constructionData.ObjectInfo.OffenseRating >= 4 && parentStructure.Team.Index != 0)
				{
					int value2 = _Pref_Limit_Heavy_Tank_Human.Value;
					if (value2 > -1 && GetUnitCount(parentStructure.Team, constructionData, EObjectComparison.UnitTypeComparison) >= value2)
					{
						NotifyLimitsEnforced(parentStructure.Team, value2, "Heavy Tank");
						args.Block = true;
					}
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run OnRequestBuildUnit_LimitCheck");
			}
		}

		public int GetStructureCount(Team team, ConstructionData constructionData, EObjectComparison objectComparison = EObjectComparison.ObjectInfoExact)
		{
			int num = 0;
			foreach (Structure structure in team.Structures)
			{
				if (!structure)
				{
					MelonLogger.Error("GetStructureCount: A structure is NULL for team '" + team.TeamShortName + "', skipping it....");
				}
				else
				{
					if (structure.IsDestroyed)
					{
						continue;
					}
					switch (objectComparison)
					{
					case EObjectComparison.ObjectInfoExact:
						if (structure.ObjectInfo == constructionData.ObjectInfo)
						{
							num++;
						}
						break;
					case EObjectComparison.StructureTypeComparison:
						if (structure.ObjectInfo.StructureType == constructionData.ObjectInfo.StructureType)
						{
							num++;
						}
						break;
					case EObjectComparison.StructureSelectionTypeComparison:
						if (structure.ObjectInfo.StructureSelectionType == constructionData.ObjectInfo.StructureSelectionType)
						{
							num++;
						}
						break;
					case EObjectComparison.StructureSelectionTypeComparisonSimilarDefense:
						if (structure.ObjectInfo.StructureSelectionType != constructionData.ObjectInfo.StructureSelectionType)
						{
							break;
						}
						if (constructionData.ObjectInfo.DefenseRating == 0)
						{
							if (structure.ObjectInfo.DefenseRating == 0)
							{
								num++;
							}
						}
						else if (structure.ObjectInfo.DefenseRating > 0)
						{
							num++;
						}
						break;
					}
				}
			}
			if (_Pref_Check_UnderConstruction.Value)
			{
				num += GetObjectUnderConstructionCount(team, constructionData, checkStructures: true, objectComparison);
			}
			return num;
		}

		public int GetUnitCount(Team team, ConstructionData constructionData, EObjectComparison objectComparison)
		{
			int num = 0;
			foreach (Unit unit in team.Units)
			{
				if (!unit)
				{
					MelonLogger.Error("GetUnitTypeCount: A unit is NULL for team '" + team.TeamShortName + "', skipping it...");
				}
				else
				{
					if (unit.IsDestroyed)
					{
						continue;
					}
					switch (objectComparison)
					{
					case EObjectComparison.ObjectInfoExact:
						if (unit.ObjectInfo == constructionData.ObjectInfo)
						{
							num++;
						}
						break;
					case EObjectComparison.UnitTypeComparison:
						if (unit.ObjectInfo.UnitType == constructionData.ObjectInfo.UnitType)
						{
							num++;
						}
						break;
					}
				}
			}
			if (_Pref_Check_UnderConstruction.Value)
			{
				num += GetObjectUnderConstructionCount(team, constructionData, checkStructures: false, objectComparison);
			}
			return num;
		}

		public int GetObjectUnderConstructionCount(Team team, ConstructionData constructionData, bool checkStructures, EObjectComparison objectComparison)
		{
			int num = 0;
			foreach (ConstructionSite constructionSite in ConstructionSite.ConstructionSites)
			{
				if (!constructionSite)
				{
					MelonLogger.Error("GetObjectUnderConstructionCount: A construction site is NULL for team '" + team.TeamShortName + "', skipping it...");
				}
				else
				{
					if (constructionSite.Team.Index != team.Index || (checkStructures && !constructionSite.ConstructionData.IsStructure) || (!checkStructures && !constructionSite.ConstructionData.IsUnit))
					{
						continue;
					}
					switch (objectComparison)
					{
					case EObjectComparison.ObjectInfoExact:
						if (constructionSite.ConstructionData.ObjectInfo == constructionData.ObjectInfo)
						{
							num++;
						}
						break;
					case EObjectComparison.StructureTypeComparison:
						if (constructionSite.ConstructionData.ObjectInfo.StructureType == constructionData.ObjectInfo.StructureType)
						{
							num++;
						}
						break;
					case EObjectComparison.StructureSelectionTypeComparison:
						if (constructionSite.ConstructionData.ObjectInfo.StructureSelectionType == constructionData.ObjectInfo.StructureSelectionType)
						{
							num++;
						}
						break;
					case EObjectComparison.UnitTypeComparison:
						if (constructionSite.ConstructionData.ObjectInfo.UnitType == constructionData.ObjectInfo.UnitType)
						{
							num++;
						}
						break;
					case EObjectComparison.StructureSelectionTypeComparisonSimilarDefense:
						if (constructionSite.ConstructionData.ObjectInfo.StructureSelectionType != constructionData.ObjectInfo.StructureSelectionType)
						{
							break;
						}
						if (constructionData.ObjectInfo.DefenseRating == 0)
						{
							if (constructionSite.ConstructionData.ObjectInfo.DefenseRating == 0)
							{
								num++;
							}
						}
						else if (constructionSite.ConstructionData.ObjectInfo.DefenseRating > 0)
						{
							num++;
						}
						break;
					}
				}
			}
			return num;
		}
	}
}
