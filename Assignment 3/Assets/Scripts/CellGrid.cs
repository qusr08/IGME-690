using System.Collections.Generic;
using UnityEngine;

public class CellGrid : MonoBehaviour
{
	[SerializeField] private GameObject cellPrefab;
	[SerializeField] private CellDataDictionary cellDictionary;
	[Space]
	[SerializeField, Range(10f, 100f)] public int Size;
	[SerializeField, Range(0.01f, 1f)] private float updateSpeed;
	[SerializeField] private bool isPaused;

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
		if (isPaused)
		{
			return;
		}

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
		cellBuffer = new CellType[Size, Size];
		cellObjects = new Cell[Size, Size];
		transform.position = new Vector3(-Size / 2f + 0.5f, 0f, -Size / 2f + 0.5f);

		// Generate starting locations for trees
		List<Vector2Int> treeCenters = new List<Vector2Int>();
		for (int i = 0; i < Size * Size / 50; i++)
		{
			treeCenters.Add(new Vector2Int(Random.Range(0, Size), Random.Range(0, Size)));
		}

		// Generate starting location for the roads
		List<Vector2Int> roadCenters = new List<Vector2Int>();
		for (int i = 0; i < Size / 10; i++)
		{
			roadCenters.Add(new Vector2Int(Random.Range(0, Size), Random.Range(0, Size)));
		}

		for (int x = 0; x < Size; x++)
		{
			for (int y = 0; y < Size; y++)
			{
				GameObject cellObject = Instantiate(cellPrefab, transform);
				cellObject.transform.localPosition = new Vector3(x, 0, y);
				cellObjects[x, y] = cellObject.GetComponent<Cell>();
				cellBuffer[x, y] = CellType.Grass;

				// Random chance to just skip and set the cell to grass
				if (Random.Range(0f, 1f) < 0.1f)
				{
					continue;
				}

				Vector2Int cellPosition = new Vector2Int(x, y);
				float radius = Size / 10;

				// Check for tree areas
				for (int k = 0; k < treeCenters.Count; k++)
				{
					if (Vector2Int.Distance(treeCenters[k], cellPosition) <= radius)
					{
						cellBuffer[x, y] = CellType.Tree;
						break;
					}
				}

				// Check for road areas
				for (int k = 0; k < roadCenters.Count; k++)
				{
					if (Vector2Int.Distance(roadCenters[k], cellPosition) <= radius)
					{
						cellBuffer[x, y] = CellType.Road;
						break;
					}
				}
			}
		}
	}

	private void UpdateCells()
	{
		for (int x = 0; x < Size; x++)
		{
			for (int y = 0; y < Size; y++)
			{
				// Try to decay the cell first
				// If that fails, try to spread the tile
				if (cellObjects[x, y].CheckDecay(GetLikeNeighbors(x, y)))
				{
					cellBuffer[x, y] = CellType.Grass;
				}
				else if (cellObjects[x, y].CheckSpread(out Vector2Int spreadDirection))
				{
					int i = x + spreadDirection.x;
					int j = y + spreadDirection.y;

					// Make sure the cell can spread to the cell
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

		// Count all of the neighbors around a specific position that match the valid neighbors of the cell
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
		// Apply all the changes in the buffer to the cell objects to have the changes show
		for (int x = 0; x < Size; x++)
		{
			for (int y = 0; y < Size; y++)
			{
				CellType cellType = cellBuffer[x, y];
				cellObjects[x, y].Set(cellType, cellDictionary[cellType]);
			}
		}
	}

	private bool IsValidPosition(int x, int y, CellType[] validCellTypeList = null)
	{
		if (x < 0 || x >= Size || y < 0 || y >= Size)
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
