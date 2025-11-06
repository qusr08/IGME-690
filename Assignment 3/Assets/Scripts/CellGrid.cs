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

    private Cell[,] cellObjects;
    private CellType[,] cellBuffer;
    private float updateTimer;

    private void Start()
    {
        GenerateCells();
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateSpeed)
        {
            updateTimer -= updateSpeed;
            UpdateCells();
        }
    }

    private void GenerateCells()
    {
        cellBuffer = new CellType[width, height];
        cellObjects = new Cell[width, height];
        transform.position = new Vector3(-width / 2f + 0.5f, 0f, -height / 2f + 0.5f);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject cellObject = Instantiate(cellPrefab, transform);
                cellObject.transform.localPosition = new Vector3(i, 0, j);
                cellObjects[i, j] = cellObject.GetComponent<Cell>();
                cellBuffer[i, j] = (i == 4 && j == 4 ? CellType.Road : CellType.Grass);
            }
        }

        ApplyBuffer();
    }

    private void UpdateCells()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (cellObjects[i, j].CheckDecay())
                {
                    cellBuffer[i, j] = CellType.Grass;
                }
                else if (cellObjects[i, j].CheckSpread(out List<Vector2Int> spreadDirections))
                {
                    foreach (Vector2Int direction in spreadDirections)
                    {
                        int x = i + direction.x;
                        int y = j + direction.y;

                        if (!IsOnGrid(x, y))
                        {
                            continue;
                        }

                        cellBuffer[x, y] = cellObjects[i, j].CellType;
                    }
                }
            }
        }

        ApplyBuffer();
    }

    private bool IsOnGrid(int i, int j)
    {
        return i >= 0 && i < width && j >= 0 && j < height;
    }

    private void ApplyBuffer()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                CellType cellType = cellBuffer[i, j];
                cellObjects[i, j].Set(cellType, cellDictionary[cellType]);
            }
        }
    }
}
