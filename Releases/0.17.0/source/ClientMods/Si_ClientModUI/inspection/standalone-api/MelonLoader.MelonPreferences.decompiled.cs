using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader.Preferences;
using MelonLoader.Preferences.IO;
using MelonLoader.Utils;
using Tomlet.Exceptions;
using Tomlet.Models;

namespace MelonLoader;

public static class MelonPreferences
{
	public static readonly List<MelonPreferences_Category> Categories;

	public static readonly List<MelonPreferences_ReflectiveCategory> ReflectiveCategories;

	public static readonly TomlMapper Mapper;

	/// <summary>
	/// Occurs when a Preferences File has been loaded.
	/// <para>
	/// <see cref="T:System.String" />: Path of the Preferences File.
	/// </para>
	/// </summary>
	public static readonly MelonEvent<string> OnPreferencesLoaded;

	/// <summary>
	/// Occurs when a Preferences File has been saved.
	/// <para>
	/// <see cref="T:System.String" />: Path of the Preferences File.
	/// </para>
	/// </summary>
	public static readonly MelonEvent<string> OnPreferencesSaved;

	internal static List<MelonLoader.Preferences.IO.File> PrefFiles;

	internal static MelonLoader.Preferences.IO.File DefaultFile;

	static MelonPreferences()
	{
		Categories = new List<MelonPreferences_Category>();
		ReflectiveCategories = new List<MelonPreferences_ReflectiveCategory>();
		Mapper = new TomlMapper();
		OnPreferencesLoaded = new MelonEvent<string>();
		OnPreferencesSaved = new MelonEvent<string>();
		PrefFiles = new List<MelonLoader.Preferences.IO.File>();
		DefaultFile = null;
		DefaultFile = new MelonLoader.Preferences.IO.File(Path.Combine(MelonEnvironment.UserDataDirectory, "MelonPreferences.cfg"), Path.Combine(MelonEnvironment.UserDataDirectory, "modprefs.ini"));
	}

	public static void Load()
	{
		try
		{
			DefaultFile.LegacyLoad();
		}
		catch (Exception value)
		{
			MelonLogger.Error($"Error while Loading Legacy Preferences from {DefaultFile.LegacyFilePath}: {value}");
			DefaultFile.WasError = true;
		}
		try
		{
			DefaultFile.Load();
		}
		catch (Exception value2)
		{
			MelonLogger.Error($"Error while Loading Preferences from {DefaultFile.FilePath}: {value2}");
			DefaultFile.WasError = true;
		}
		if (PrefFiles.Count >= 0)
		{
			foreach (MelonLoader.Preferences.IO.File prefFile in PrefFiles)
			{
				try
				{
					prefFile.Load();
				}
				catch (Exception value3)
				{
					MelonLogger.Error($"Error while Loading Preferences from {prefFile.FilePath}: {value3}");
					prefFile.WasError = true;
				}
			}
		}
		if (Categories.Count > 0)
		{
			foreach (MelonPreferences_Category category in Categories)
			{
				if (category.Entries.Count <= 0)
				{
					continue;
				}
				MelonLoader.Preferences.IO.File file = category.File;
				if (file == null)
				{
					file = DefaultFile;
				}
				if (file.WasError)
				{
					continue;
				}
				foreach (MelonPreferences_Entry entry in category.Entries)
				{
					file.SetupEntryFromRawValue(entry);
				}
			}
		}
		if (ReflectiveCategories.Count > 0)
		{
			foreach (MelonPreferences_ReflectiveCategory reflectiveCategory in ReflectiveCategories)
			{
				MelonLoader.Preferences.IO.File file2 = reflectiveCategory.File;
				if (file2 == null)
				{
					file2 = DefaultFile;
				}
				if (!file2.WasError)
				{
					TomlTable tomlTable = file2.TryGetCategoryTable(reflectiveCategory.Identifier);
					if (tomlTable == null)
					{
						reflectiveCategory.LoadDefaults();
						continue;
					}
					reflectiveCategory.Load(tomlTable);
					OnPreferencesLoaded.Invoke(file2.FilePath);
				}
			}
		}
		MelonLogger.Msg("Preferences Loaded!");
	}

	public static void Save()
	{
		if (Categories.Count > 0)
		{
			foreach (MelonPreferences_Category category in Categories)
			{
				MelonLoader.Preferences.IO.File file = category.File;
				if (file == null)
				{
					file = DefaultFile;
				}
				foreach (MelonPreferences_Entry entry in category.Entries)
				{
					if ((!entry.DontSaveDefault || !(entry.GetValueAsString() == entry.GetDefaultValueAsString())) && entry.GetValueAsString() != null)
					{
						file.InsertIntoDocument(category.Identifier, entry.Identifier, entry.Save());
					}
				}
			}
		}
		if (ReflectiveCategories.Count > 0)
		{
			foreach (MelonPreferences_ReflectiveCategory reflectiveCategory in ReflectiveCategories)
			{
				MelonLoader.Preferences.IO.File file2 = reflectiveCategory.File;
				if (file2 == null)
				{
					file2 = DefaultFile;
				}
				file2.document.PutValue(reflectiveCategory.Identifier, reflectiveCategory.Save());
			}
		}
		try
		{
			DefaultFile.Save();
		}
		catch (Exception value)
		{
			MelonLogger.Error($"Error while Saving Preferences to {DefaultFile.FilePath}: {value}");
			DefaultFile.WasError = true;
		}
		if (PrefFiles.Count >= 0)
		{
			foreach (MelonLoader.Preferences.IO.File prefFile in PrefFiles)
			{
				try
				{
					prefFile.Save();
				}
				catch (Exception value2)
				{
					MelonLogger.Error($"Error while Saving Preferences to {prefFile.FilePath}: {value2}");
					prefFile.WasError = true;
					continue;
				}
				OnPreferencesSaved.Invoke(prefFile.FilePath);
			}
		}
		MelonLogger.Msg("Preferences Saved!");
	}

	public static MelonPreferences_Category CreateCategory(string identifier)
	{
		return CreateCategory(identifier, null, false, true);
	}

	public static MelonPreferences_Category CreateCategory(string identifier, string display_name = null)
	{
		return CreateCategory(identifier, display_name, false, true);
	}

	public static MelonPreferences_Category CreateCategory(string identifier, string display_name = null, bool is_hidden = false, bool should_save = true)
	{
		if (string.IsNullOrEmpty(identifier))
		{
			throw new Exception("identifier is null or empty when calling CreateCategory");
		}
		if (display_name == null)
		{
			display_name = identifier;
		}
		MelonPreferences_Category category = GetCategory(identifier);
		if (category != null)
		{
			return category;
		}
		return new MelonPreferences_Category(identifier, display_name, is_hidden);
	}

	public static MelonPreferences_ReflectiveCategory CreateCategory<T>(string identifier, string display_name = null) where T : new()
	{
		return MelonPreferences_ReflectiveCategory.Create<T>(identifier, display_name);
	}

	[Obsolete]
	public static MelonPreferences_Entry CreateEntry<T>(string category_identifier, string entry_identifier, T default_value, string display_name, bool is_hidden)
	{
		return CreateEntry(category_identifier, entry_identifier, default_value, display_name, null, is_hidden);
	}

	public static MelonPreferences_Entry<T> CreateEntry<T>(string category_identifier, string entry_identifier, T default_value, string display_name = null, string description = null, bool is_hidden = false, bool dont_save_default = false, ValueValidator validator = null)
	{
		if (string.IsNullOrEmpty(category_identifier))
		{
			throw new Exception("category_identifier is null or empty when calling CreateEntry");
		}
		if (string.IsNullOrEmpty(entry_identifier))
		{
			throw new Exception("entry_identifier is null or empty when calling CreateEntry");
		}
		MelonPreferences_Category melonPreferences_Category = GetCategory(entry_identifier);
		if (melonPreferences_Category == null)
		{
			melonPreferences_Category = CreateCategory(category_identifier);
		}
		return melonPreferences_Category.CreateEntry(entry_identifier, default_value, display_name, description, is_hidden, dont_save_default, validator);
	}

	public static MelonPreferences_Category GetCategory(string identifier)
	{
		if (string.IsNullOrEmpty(identifier))
		{
			throw new Exception("identifier is null or empty when calling GetCategory");
		}
		if (Categories.Count <= 0)
		{
			return null;
		}
		return Categories.Find((MelonPreferences_Category x) => x.Identifier.Equals(identifier));
	}

	public static T GetCategory<T>(string identifier) where T : new()
	{
		if (string.IsNullOrEmpty(identifier))
		{
			throw new Exception("identifier is null or empty when calling GetCategory");
		}
		if (ReflectiveCategories.Count <= 0)
		{
			return default;
		}
		MelonPreferences_ReflectiveCategory melonPreferences_ReflectiveCategory = ReflectiveCategories.Find((MelonPreferences_ReflectiveCategory x) => x.Identifier.Equals(identifier));
		if (melonPreferences_ReflectiveCategory != null)
		{
			return melonPreferences_ReflectiveCategory.GetValue<T>();
		}
		return default;
	}

	public static void SaveCategory<T>(string identifier, bool printmsg = true)
	{
		if (string.IsNullOrEmpty(identifier))
		{
			throw new Exception("identifier is null or empty when calling GetCategory");
		}
		if (ReflectiveCategories.Count > 0)
		{
			ReflectiveCategories.Find((MelonPreferences_ReflectiveCategory x) => x.Identifier.Equals(identifier))?.SaveToFile(printmsg);
		}
	}

	public static MelonPreferences_Entry GetEntry(string category_identifier, string entry_identifier)
	{
		return GetCategory(category_identifier)?.GetEntry(entry_identifier);
	}

	public static MelonPreferences_Entry<T> GetEntry<T>(string category_identifier, string entry_identifier)
	{
		return GetCategory(category_identifier)?.GetEntry<T>(entry_identifier);
	}

	public static bool HasEntry(string category_identifier, string entry_identifier)
	{
		return GetEntry(category_identifier, entry_identifier) != null;
	}

	public static void SetEntryValue<T>(string category_identifier, string entry_identifier, T value)
	{
		MelonPreferences_Entry<T> melonPreferences_Entry = GetCategory(category_identifier)?.GetEntry<T>(entry_identifier);
		if (melonPreferences_Entry != null)
		{
			melonPreferences_Entry.Value = value;
		}
	}

	public static T GetEntryValue<T>(string category_identifier, string entry_identifier)
	{
		MelonPreferences_Category category = GetCategory(category_identifier);
		if (category == null)
		{
			return default;
		}
		MelonPreferences_Entry<T> entry = category.GetEntry<T>(entry_identifier);
		if (entry == null)
		{
			return default;
		}
		return entry.Value;
	}

	internal static MelonLoader.Preferences.IO.File GetPrefFileFromFilePath(string filepath)
	{
		if (PrefFiles.Count <= 0)
		{
			return null;
		}
		FileInfo fileInfo = new FileInfo(filepath);
		foreach (MelonLoader.Preferences.IO.File prefFile in PrefFiles)
		{
			FileInfo fileInfo2 = new FileInfo(prefFile.FilePath);
			if (fileInfo.FullName.Equals(fileInfo2.FullName))
			{
				return prefFile;
			}
		}
		return null;
	}

	internal static bool IsFileInUse(MelonLoader.Preferences.IO.File file)
	{
		if (Categories.Count <= 0)
		{
			return false;
		}
		if (file == null)
		{
			file = DefaultFile;
		}
		if (file == DefaultFile)
		{
			return true;
		}
		foreach (MelonPreferences_Category category in Categories)
		{
			MelonLoader.Preferences.IO.File file2 = category.File;
			if (file2 == null)
			{
				file2 = DefaultFile;
			}
			if (file2 == file)
			{
				return true;
			}
		}
		foreach (MelonPreferences_ReflectiveCategory reflectiveCategory in ReflectiveCategories)
		{
			MelonLoader.Preferences.IO.File file3 = reflectiveCategory.File;
			if (file3 == null)
			{
				file3 = DefaultFile;
			}
			if (file3 == file)
			{
				return true;
			}
		}
		return false;
	}

	internal static void LoadFileAndRefreshCategories(MelonLoader.Preferences.IO.File file, bool printmsg = true)
	{
		try
		{
			file.Load();
		}
		catch (TomlUnescapedUnicodeControlCharException value)
		{
			MelonLogger.Error($"Error while Loading Preferences from {file.FilePath}: {value}");
			return;
		}
		catch (Exception value2)
		{
			MelonLogger.Error($"Error while Loading Preferences from {file.FilePath}: {value2}");
			file.WasError = true;
			return;
		}
		if (Categories.Count > 0)
		{
			foreach (MelonPreferences_Category category in Categories)
			{
				MelonLoader.Preferences.IO.File file2 = category.File;
				if (file2 == null)
				{
					file2 = DefaultFile;
				}
				if (file2 != file || category.Entries.Count <= 0)
				{
					continue;
				}
				foreach (MelonPreferences_Entry entry in category.Entries)
				{
					file2.SetupEntryFromRawValue(entry);
				}
			}
		}
		if (ReflectiveCategories.Count > 0)
		{
			foreach (MelonPreferences_ReflectiveCategory reflectiveCategory in ReflectiveCategories)
			{
				MelonLoader.Preferences.IO.File file3 = reflectiveCategory.File;
				if (file3 == null)
				{
					file3 = DefaultFile;
				}
				if (file3 == file)
				{
					TomlTable tomlTable = file.TryGetCategoryTable(reflectiveCategory.Identifier);
					if (tomlTable == null)
					{
						reflectiveCategory.LoadDefaults();
					}
					else
					{
						reflectiveCategory.Load(tomlTable);
					}
				}
			}
		}
		if (printmsg)
		{
			MelonLogger.MsgDirect("MelonPreferences Loaded from " + file.FilePath);
		}
		OnPreferencesLoaded.Invoke(file.FilePath);
	}

	public static void RemoveCategoryFromFile(string filePath, string categoryName)
	{
		GetPrefFileFromFilePath(filePath)?.RemoveCategoryFromDocument(categoryName);
	}

	internal static bool IsFilePathDefault(string filepath)
	{
		return new FileInfo(filepath).FullName.Equals(new FileInfo(DefaultFile.FilePath).FullName);
	}
}
