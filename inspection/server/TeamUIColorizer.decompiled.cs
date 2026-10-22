using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TeamUIColorizer : TeamUIColorizer<Graphic>
{
	public TeamColorType ColorType;

	protected override void OnColorChanged()
	{
		if (Source != null && !(Target == null))
		{
			Color color = Source.GetColor(ColorType);
			Target.color = color;
		}
	}
}
