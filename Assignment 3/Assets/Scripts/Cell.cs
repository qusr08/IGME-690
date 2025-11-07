using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cell : MonoBehaviour
{
	[SerializeField] private CellFeatureDictionary features;
	[SerializeField, Range(0f, 1f)] private float colorVariation;

	public CellType CellType { get; private set; } = CellType.None;
	public CellData CellData { get; private set; }

	private Material material;

	private void Awake()
	{
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		material = new Material(meshRenderer.material);
		meshRenderer.material = material;
	}

	public bool CheckSpread(out Vector2Int spreadDirection)
	{
		if (Random.Range(0f, 1f) < CellData.SpreadChance)
		{
			spreadDirection = Utils.Choose(Utils.CardinalDirections);
			return true;
		}

		spreadDirection = Vector2Int.zero;
		return false;
	}

	public bool CheckDecay(int neighborCount)
	{
		if (neighborCount < CellData.MinLikeNeighbors || neighborCount > CellData.MaxLikeNeighbors)
		{
			return true;
		}

		return Random.Range(0f, 1f) < CellData.DecayChance;
	}

	public void Set(CellType cellType, CellData cellData)
	{
		if (CellType == cellType)
		{
			return;
		}

		if (features.TryGetValue(CellType, out GameObject oldFeature))
		{
			oldFeature.SetActive(false);
		}

		CellType = cellType;
		CellData = cellData;

		if (features.TryGetValue(CellType, out GameObject newFeature))
		{
			newFeature.SetActive(true);
		}

		Color.RGBToHSV(cellData.Color, out float h, out float s, out float v);
		float newS = Mathf.Clamp01(Random.Range(-colorVariation, colorVariation) + s);
		float newV = Mathf.Clamp01(Random.Range(-colorVariation, colorVariation) + v);
		material.color = Color.HSVToRGB(h, newS, newV);
	}
}