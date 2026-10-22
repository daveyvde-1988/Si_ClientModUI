using UnityEngine;

namespace Silica.UI;

public static class UITools
{
	public static readonly Color32 HostileRed = new Color32(byte.MaxValue, 73, 73, byte.MaxValue);

	public static readonly Color32 AllyBlue = new Color32(180, 244, byte.MaxValue, byte.MaxValue);

	public static readonly Color32 Neutral = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static string ToColor(this string value, Color color)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + value + "</color>";
	}

	public static string ToBold(this string value)
	{
		return "<b>" + value + "</b>";
	}

	public static string ToItalics(this string value)
	{
		return "<i>" + value + "</i>";
	}

	public static string ToSize(this string value, float size)
	{
		return $"<size={size}>{value}</size>";
	}

	public static string ToSizePercent(this string value, float percent)
	{
		return $"<size={percent}%>{value}</size>";
	}

	public static string AddKeybindArrows(this string value)
	{
		return "«" + value + "»";
	}

	public static string ToAlignment(this string value, TextAlignment textAlignment)
	{
		string text = textAlignment.ToString().ToLower();
		return "<align=" + text + ">" + value + "</align>";
	}
}
