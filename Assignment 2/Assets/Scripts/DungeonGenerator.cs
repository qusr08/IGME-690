using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int size;

    private DungeonRoomLibrary roomLibrary;
    private DungeonRoom[,] map;
    private DungeonPossibility[,] possibilities;

    private void Awake()
    {
        roomLibrary = GetComponent<DungeonRoomLibrary>();
        map = new DungeonRoom[size.x, size.y];
        possibilities = new DungeonPossibility[size.x, size.y];
    }

    private void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        // Create a list of all the dungeon possibility entries for the map
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                DungeonPossibility possibility = new DungeonPossibility(x, y);
                possibility.PossibleRooms.AddRange(roomLibrary.DungeonRoomIndices);
                possibilities[x, y] = possibility;
            }
        }

        int collapseCount = 0;
        while (collapseCount < size.x * size.y)
        {
            // Get the positions of the highest entropy on the map
            List<Vector2Int> highestEntropyPositions = FindHighestEntropyPositions();

            if (highestEntropyPositions.Count == 0)
            {
                break;
            }

            // Randomly select possibility to resolve
            Vector2Int collapsePosition = GetRandomListValue(highestEntropyPositions);

            // Randomly select possible room within that possibility
            possibilities[collapsePosition.x, collapsePosition.y].CollapsePossibleRoom();

            // Update possible rooms for surrounding possibilities


            collapseCount++;
        }
    }

    private List<Vector2Int> FindHighestEntropyPositions()
    {
        List<Vector2Int> highestEntropyPositions = new List<Vector2Int>();
        int highestEntropyValue = 0;

        // Loop through all possibilities
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                // Get the current entropy at the position
                int entropy = possibilities[x, y].Entropy;

                // If there is a new highest entropy, clear the list and set it as the new highest
                if (entropy > highestEntropyValue && entropy > 0)
                {
                    highestEntropyPositions.Clear();
                    highestEntropyValue = entropy;
                }

                // Add to the highest entropy list if there is a tie for the highest entropy
                if (entropy == highestEntropyValue)
                {
                    highestEntropyPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        return highestEntropyPositions;
    }

    private T GetRandomListValue<T>(List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }
}

public class DungeonPossibility
{
    public List<int> PossibleRooms { get; private set; }
    public int CollapsedRoom { get; private set; }
    public bool IsCollapsed => CollapsedRoom > 0;
    public int Entropy => PossibleRooms.Count;
    public Vector2Int MapPosition { get; private set; }

    public DungeonPossibility(int x, int y)
    {
        PossibleRooms = new List<int>();
        CollapsedRoom = -1;
        MapPosition = new Vector2Int(x, y);
    }

    public void CollapsePossibleRoom()
    {
        if (IsCollapsed)
        {
            return;
        }

        CollapsedRoom = PossibleRooms[Random.Range(0, Entropy)];
        PossibleRooms.Clear();
    }
}