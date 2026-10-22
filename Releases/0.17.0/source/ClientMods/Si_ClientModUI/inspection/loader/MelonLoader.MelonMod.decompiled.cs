using System;
using System.Collections.Generic;

namespace MelonLoader;

public abstract class MelonMod : MelonTypeBase<MelonMod>
{
	[Obsolete]
	private MelonModInfoAttribute _LegacyInfoAttribute;

	[Obsolete]
	private MelonModGameAttribute[] _LegacyGameAttributes;

	[Obsolete("Use MelonBase.Info instead. This will be removed in a future update.", true)]
	public MelonModInfoAttribute InfoAttribute
	{
		get
		{
			if (_LegacyInfoAttribute == null)
			{
				_LegacyInfoAttribute = new MelonModInfoAttribute(Info.SystemType, Info.Name, Info.Version, Info.Author, Info.DownloadLink);
			}
			return _LegacyInfoAttribute;
		}
	}

	[Obsolete("Use MelonBase.Games instead. This will be removed in a future update.", true)]
	public MelonModGameAttribute[] GameAttributes
	{
		get
		{
			if (_LegacyGameAttributes != null)
			{
				return _LegacyGameAttributes;
			}
			List<MelonModGameAttribute> list = new List<MelonModGameAttribute>();
			MelonGameAttribute[] games = Games;
			foreach (MelonGameAttribute melonGameAttribute in games)
			{
				list.Add(new MelonModGameAttribute(melonGameAttribute.Developer, melonGameAttribute.Name));
			}
			_LegacyGameAttributes = list.ToArray();
			return _LegacyGameAttributes;
		}
	}

	static MelonMod()
	{
		MelonTypeBase<MelonMod>.TypeName = "Mod";
	}

	private protected override bool RegisterInternal()
	{
		try
		{
			OnPreSupportModule();
		}
		catch (Exception ex)
		{
			MelonLogger.Error("Failed to register " + MelonTypeName + " '" + MelonAssembly.Location + "': Melon failed to initialize in the deprecated OnPreSupportModule callback!");
			MelonLogger.Error(ex.ToString());
			return false;
		}
		if (!base.RegisterInternal())
		{
			return false;
		}
		if (MelonEvents.MelonHarmonyInit.Disposed)
		{
			HarmonyInit();
		}
		else
		{
			MelonEvents.MelonHarmonyInit.Subscribe(HarmonyInit, Priority, unsubscribeOnFirstInvocation: true);
		}
		return true;
	}

	private void HarmonyInit()
	{
		if (!MelonAssembly.HarmonyDontPatchAll)
		{
			HarmonyInstance.PatchAll(MelonAssembly.Assembly);
		}
	}

	private protected override void RegisterCallbacks()
	{
		base.RegisterCallbacks();
		MelonEvents.OnSceneWasLoaded.Subscribe(OnSceneWasLoaded, Priority);
		MelonEvents.OnSceneWasInitialized.Subscribe(OnSceneWasInitialized, Priority);
		MelonEvents.OnSceneWasUnloaded.Subscribe(OnSceneWasUnloaded, Priority);
		RegisterObsoleteCallbacks();
	}

	[Obsolete]
	private void RegisterObsoleteCallbacks()
	{
		MelonEvents.OnSceneWasLoaded.Subscribe((int idx, string name) =>
		{
			OnLevelWasLoaded(idx);
		}, Priority);
		MelonEvents.OnSceneWasInitialized.Subscribe((int idx, string name) =>
		{
			OnLevelWasInitialized(idx);
		}, Priority);
		MelonEvents.OnApplicationStart.Subscribe(OnApplicationStart, Priority);
	}

	/// <summary>
	/// Runs when a new Scene is loaded.
	/// </summary>
	public virtual void OnSceneWasLoaded(int buildIndex, string sceneName)
	{
	}

	/// <summary>
	/// Runs once a Scene is initialized.
	/// </summary>
	public virtual void OnSceneWasInitialized(int buildIndex, string sceneName)
	{
	}

	/// <summary>
	/// Runs once a Scene unloads.
	/// </summary>
	public virtual void OnSceneWasUnloaded(int buildIndex, string sceneName)
	{
	}

	[Obsolete("Override OnSceneWasLoaded instead. This will be removed in a future update.", true)]
	public virtual void OnLevelWasLoaded(int level)
	{
	}

	[Obsolete("Override OnSceneWasInitialized instead. This will be removed in a future update.", true)]
	public virtual void OnLevelWasInitialized(int level)
	{
	}
}
