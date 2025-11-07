using System.Collections.Generic;
using UnityEngine;

public class CellGrid : MonoBehaviour
{
	[SerializeField] private GameObject cellPrefab;
	[SerializeField] private CellDictionary cellDictionary;
	[Space]
	[SerializeField, Range(3f, 50f)] private int width;
	[SerializeField, Range(3f, 50f)] private int height;
	[SerializeField, Range(0.01f, 1f)] private float updateSpeed;
	[SerializeField, Range(1f, 10f)] private float startingAreaRadius;

	private Cell[,] cellObjects;
	private CellType[,] cellBuffer;
	private float updateTimer;

	private void Start()
	{
		GenerateCells();
		ApplyBuffer();
	}

	private void Update()
	{
		updateTimer += Time.deltaTime;
		if (updateTimer >= updateSpeed)
		{
			updateTimer -= updateSpeed;
			UpdateCells();
			ApplyBuffer();
		}
	}

	private void GenerateCells()
	{
		// Initialize arrays and position the cell grid
		cellBuffer = new CellType[width, height];
		cellObjects = new Cell[width, height];
		transform.position = new Vector3(-width / 2f + 0.5f, 0f, -height / 2f + 0.5f);

		// Generate starting locations for trees
		List<Vector2Int> treeCenters = new List<Vector2Int>();
		for (int i = 0; i < width * height / 50f; i++)
		{
			treeCenters.Add(new Vector2Int(Random.Range(0, width), Random.Range(0, height)));
		}

		// Generate starting location for the roads
		List<Vector2Int> roadCenters = new List<Vector2Int>();
		for (int i = 0; i < 1; i++)
		{
			roadCenters.Add(new Vector2Int(Random.Range(0, width), Random.Range(0, height)));
		}

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				GameObject cellObject = Instantiate(cellPrefab, transform);
				cellObject.transform.localPosition = new Vector3(x, 0, y);
				cellObjects[x, y] = cellObject.GetComponent<Cell>();

				Vector2Int cellPosition = new Vector2Int(x, y);
				cellBuffer[x, y] = CellType.None;

				for (int k = 0; k < roadCenters.Count; k++)
				{
					if (Vector2Int.Distance(roadCenters[k], cellPosition) <= startingAreaRadius)
					{
						cellBuffer[x, y] = CellType.Road;
						break;
					}
				}

				if (cellBuffer[x, y] != CellType.None)
				{
					continue;
				}

				for (int k = 0; k < treeCenters.Count; k++)
				{
					if (Vector2Int.Distance(treeCenters[k], cellPosition) <= startingAreaRadius)
					{
						cellBuffer[x, y] = CellType.Tree;
						break;
					}
				}

				if (cellBuffer[x, y] != CellType.None)
				{
					continue;
				}

				cellBuffer[x, y] = CellType.Grass;
			}
		}
	}

	private void UpdateCells()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (cellObjects[x, y].CheckDecay(GetLikeNeighbors(x, y)))
				{
					cellBuffer[x, y] = CellType.Grass;
				}
				else if (cellObjects[x, y].CheckSpread(out Vector2Int spreadDirection))
				{
					int i = x + spreadDirection.x;
					int j = y + spreadDirection.y;

					if (IsValidPosition(i, j, cellObjects[x, y].CellData.SpreadIgnoreList))
					{
						cellBuffer[i, j] = Utils.Choose(cellObjects[x, y].CellData.SpreadAs);
					}
				}
			}
		}
	}

	private int GetLikeNeighbors(int x, int y)
	{
		int count = 0;
		CellType cellType = cellObjects[x, y].CellType;

		for (int i = 0; i < Utils.CardinalDirections.Length; i++)
		{
			Vector2Int position = new Vector2Int(x, y) + Utils.CardinalDirections[i];
			if (IsValidPosition(position, validCellTypeList: cellObjects[x, y].CellData.ValidNeighbors))
			{
				count++;
			}
		}

		return count;
	}

	private void ApplyBuffer()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				CellType cellType = cellBuffer[x, y];
				cellObjects[x, y].Set(cellType, cellDictionary[cellType]);
			}
		}
	}

	private bool IsValidPosition(int x, int y, CellType[] validCellTypeList = null)
	{
		if (x < 0 || x >= width || y < 0 || y >= height)
		{
			return false;
		}

		if (validCellTypeList != null)
		{
			for (int i = 0; i < validCellTypeList.Length; i++)
			{
				if (validCellTypeList[i] == cellObjects[x, y].CellType)
				{
					return true;
				}
			}

			return false;
		}

		return true;
	}

	private bool IsValidPosition(Vector2Int position, CellType[] validCellTypeList = null)
	{
		return IsValidPosition(position.x, position.y, validCellTypeList: validCellTypeList);
	}
}
