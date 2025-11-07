using UnityEngine;

[CreateAssetMenu(fileName = "Cell", menuName = "Cell")]
public class CellData : ScriptableObject
{
	public Color Color;
	[Space]
	[Range(0f, 1f)] public float SpreadChance;
	public CellType[] SpreadAs;
	public CellType[] SpreadIgnoreList;
	[Space]
	[Range(0f, 1f)] public float DecayChance;
	public CellType[] ValidNeighbors;
	[Range(0, 8)] public int MaxLikeNeighbors;
	[Range(0, 8)] public int MinLikeNeighbors;
}