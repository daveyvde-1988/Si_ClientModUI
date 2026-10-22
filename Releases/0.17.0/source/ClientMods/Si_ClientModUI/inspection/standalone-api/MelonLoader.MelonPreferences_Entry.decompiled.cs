using System;
using MelonLoader.Preferences;
using Tomlet.Models;

namespace MelonLoader;

public abstract class MelonPreferences_Entry
{
	public readonly MelonEvent<object, object> OnEntryValueChangedUntyped = new MelonEvent<object, object>();

	public string Identifier { get; internal set; }

	public string DisplayName { get; set; }

	public string Description { get; set; }

	public string Comment { get; set; }

	public bool IsHidden { get; set; }

	public bool DontSaveDefault { get; set; }

	public MelonPreferences_Category Category { get; internal set; }

	public abstract object BoxedValue { get; set; }

	public abstract object BoxedEditedValue { get; set; }

	public ValueValidator Validator { get; internal set; }

	[Obsolete("Please use the OnEntryValueChangedUntyped MelonEvent instead. This will be removed in a future update.", true)]
	public event Action OnValueChangedUntyped;

	public string GetExceptionMessage(string submsg)
	{
		return $"Attempted to {submsg} {DisplayName} when it is a {GetReflectedType().FullName}!";
	}

	public abstract Type GetReflectedType();

	public abstract void ResetToDefault();

	public abstract string GetEditedValueAsString();

	public abstract string GetDefaultValueAsString();

	public abstract string GetValueAsString();

	public abstract void Load(TomlValue obj);

	public abstract TomlValue Save();

	protected void FireUntypedValueChanged(object old, object neew)
	{
		OnEntryValueChangedUntyped.Invoke(old, neew);
		OnValueChangedUntyped?.Invoke();
	}
}
