using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Silica.UI.Config;

[CreateAssetMenu(fileName = "NewUIStyle", menuName = "Silica/UI/UIStyle")]
public class UIStyle : ScriptableObject
{
	[Serializable]
	public struct ObjectiveMarker
	{
		public ObjectiveType Type;

		public Sprite Sprite;
	}

	[SerializeField]
	public Sprite FriendlyUnitMarker;

	[SerializeField]
	private SerializedDictionary<ObjectiveType, Sprite> _markersList = new SerializedDictionary<ObjectiveType, Sprite>();

	[field: SerializeField]
	public Sprite UnitMarkerIcon { get; private set; }

	[field: SerializeField]
	public Color AllyColor { get; private set; } = UITools.AllyBlue;

	[field: SerializeField]
	public Color HostileColor { get; private set; } = UITools.HostileRed;

	[field: SerializeField]
	public Color NeutralColor { get; private set; } = UITools.Neutral;

	public Sprite GetFriendlyUnitMarkerSprite()
	{
		return FriendlyUnitMarker;
	}

	public Sprite GetObjectiveMarkerSprite(ObjectiveType objectiveType)
	{
		return _markersList.GetValueOrDefault(objectiveType);
	}
}
