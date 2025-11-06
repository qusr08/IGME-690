using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum CellType
{
    Grass, Road, Tree, House
}

[Serializable]
public struct CellData
{
    public Color Color;
    public float SpreadChance;
    public float DecayChance;

    public CellData(Color color, float spreadChance, float decayChance)
    {
        Color = color;
        SpreadChance = spreadChance;
        DecayChance = decayChance;
    }
}

public class Cell : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    public CellType CellType { get; private set; }

    private CellData cellData;
    private Material material;

    private void Awake()
    {
        material = new Material(meshRenderer.material);
        meshRenderer.material = material;
    }

    public bool CheckSpread(out List<Vector2Int> spreadDirections)
    {
        spreadDirections = new List<Vector2Int>()
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        for (int i = spreadDirections.Count - 1; i >= 0; i--)
        {
            if (Random.Range(0f, 1f) >= cellData.SpreadChance)
            {
                spreadDirections.RemoveAt(i);
            }
        }

        return spreadDirections.Count > 0;
    }

    public bool CheckDecay()
    {
        return Random.Range(0f, 1f) < cellData.DecayChance;
    }

    public void Set(CellType cellType, CellData cellData)
    {
        CellType = cellType;
        this.cellData = cellData;

        material.color = cellData.Color;
    }
}