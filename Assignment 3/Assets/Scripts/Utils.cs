using UnityEngine;

public static class Utils
{
	public static Vector2Int[] CardinalDirections = new Vector2Int[]
	{
		new(1, 0), new(-1, 0), new(0, -1), new(0, 1),
		new(1, 1), new(-1, -1), new(1, -1), new(-1, 1)
	};
	public static float ColorVariation = 0.05f;

	public static T Choose<T>(T[] list)
	{
		return list[Random.Range(0, list.Length)];
	}

	public static Color GetOffsetColor (Color color)
	{
		Color.RGBToHSV(color, out float h, out float s, out float v);
		float newS = Mathf.Clamp01(Random.Range(-ColorVariation, ColorVariation) + s);
		float newV = Mathf.Clamp01(Random.Range(-ColorVariation, ColorVariation) + v);
		return Color.HSVToRGB(h, newS, newV);
	}
}
