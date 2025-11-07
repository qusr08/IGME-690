using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
	public static Vector2Int[] CardinalDirections = new Vector2Int[]
	{
		new(1, 0), new(-1, 0), new(0, -1), new(0, 1),
		new(1, 1), new(-1, -1), new(1, -1), new(-1, 1)
	};

	public static T Choose<T>(T[] list)
	{
		return list[Random.Range(0, list.Length)];
	}
}
