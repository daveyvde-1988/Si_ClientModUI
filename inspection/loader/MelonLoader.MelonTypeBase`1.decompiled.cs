using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace MelonLoader;

public abstract class MelonTypeBase<T> : MelonBase where T : MelonTypeBase<T>
{
	internal new static List<T> _registeredMelons;

	/// <summary>
	/// List of registered <typeparamref name="T" />s.
	/// </summary>
	public new static ReadOnlyCollection<T> RegisteredMelons => _registeredMelons.AsReadOnly();

	/// <summary>
	/// A Human-Readable Name for <typeparamref name="T" />.
	/// </summary>
	public static string TypeName { get; protected internal set; }

	public sealed override string MelonTypeName => TypeName;

	static MelonTypeBase()
	{
		_registeredMelons = new List<T>();
		RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
	}

	private protected override bool RegisterInternal()
	{
		if (!base.RegisterInternal())
		{
			return false;
		}
		_registeredMelons.Add((T)this);
		return true;
	}

	private protected override void UnregisterInternal()
	{
		_registeredMelons.Remove((T)this);
	}

	public static void ExecuteAll(LemonAction<T> func, bool unregisterOnFail = false, string unregistrationReason = null)
	{
		MelonBase.ExecuteList(func, _registeredMelons, unregisterOnFail, unregistrationReason);
	}
}
