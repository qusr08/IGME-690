using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int size;
    [SerializeField] private Vector2Int roomMinSize;
    [SerializeField] private Vector2Int roomMaxSize;
    [SerializeField] private int roomCount;
    [SerializeField, Min(0f)] private float generationSpeed;

    private DungeonRoomLibrary roomLibrary;
    private DungeonEntry[,] map;
    private List<GameObject> dungeonRoomObjects;

    private void Awake()
    {
        roomLibrary = GetComponent<DungeonRoomLibrary>();
        dungeonRoomObjects = new List<GameObject>();
    }

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        StopAllCoroutines();

        // Destroy all rooms so they can be regenerated
        for (int i = dungeonRoomObjects.Count - 1; i >= 0; i--)
        {
            Destroy(dungeonRoomObjects[i]);
        }
        dungeonRoomObjects.Clear();

        StartCoroutine(GenerateMapCoroutine());
    }

    private IEnumerator GenerateMapCoroutine()
    {
        // Create a list of all the dungeon possibility entries for the map
        InitializeRoomPossibilities();

        // Before doing wave function collapse, spawn some starting rooms in the dungeon
        // SpawnStartingRooms();

        // Update all of the possible rooms of the map before it starts to generate
        UpdateEntropyFromOrigin(new Vector2Int(size.x / 2, size.y / 2), forceEntireMap: true);

        int collapseCount = 0;
        while (collapseCount < size.x * size.y)
        {
            // Get the positions of the lowest entropy on the map
            List<Vector2Int> lowestEntropyPositions = FindLowestEntropyPositions();

            // If there are no more positions to collapse, then break from the loop
            if (lowestEntropyPositions.Count == 0)
            {
                break;
            }

            // Randomly select possibility to resolve
            Vector2Int collapsePosition = lowestEntropyPositions[Random.Range(0, lowestEntropyPositions.Count)];

            // Randomly select possible room within that possibility
            map[collapsePosition.x, collapsePosition.y].CollapsePossibleRooms();
            collapseCount++;

            // Update possible rooms for surrounding possibilities
            UpdateEntropyFromOrigin(collapsePosition);

            // Get the currently collapsed room at the map location
            DungeonRoom room = map[collapsePosition.x, collapsePosition.y].CollapsedRoom;

            // If there is no collapsed room there, continue to the next position
            if (room == null)
            {
                continue;
            }

            Vector3 position = roomLibrary.RoomSize * new Vector3(collapsePosition.x, 0f, collapsePosition.y);
            Quaternion rotation = Quaternion.Euler(0, room.Orientation.Rotation * -90, 0);
            dungeonRoomObjects.Add(Instantiate(room.Prefab, position, rotation, transform));

            yield return new WaitForSeconds(generationSpeed);
        }
    }

    private void InitializeRoomPossibilities()
    {
        map = new DungeonEntry[size.x, size.y];

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                // At the start, each room can be any possibility
                DungeonEntry possibility = new DungeonEntry(x, y);
                possibility.PossibleRooms.AddRange(roomLibrary.AllDungeonRooms);
                map[x, y] = possibility;
            }
        }
    }

    private void SpawnStartingRooms()
    {
        // If the size of the dungeon is not big enough to fit all the rooms side-by-side, then do not spawn any rooms
        if (size.x > roomMaxSize.x * roomCount && size.y > roomMaxSize.y * roomCount)
        {
            return;
        }

        // Generate a list of available positions for the rooms to spawn at
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        for (int x = 0; x < size.x - roomMinSize.x; x++)
        {
            for (int y = 0; y < size.y - roomMinSize.y; y++)
            {
                availablePositions.Add(new Vector2Int(x, y));
            }
        }

        // Generate each of the rooms
        for (int i = 0; i < roomCount; i++)
        {
            // Get a random position for the top-left corner of the room
            Vector2Int spawnPosition = availablePositions[Random.Range(0, availablePositions.Count)];

            // Get a random size for the room
            int randomWidth = Mathf.Max(size.x - spawnPosition.x, Random.Range(roomMinSize.x, roomMaxSize.x));
            int randomHeight = Mathf.Max(size.y - spawnPosition.y, Random.Range(roomMinSize.y, roomMaxSize.y));

            // Collapse all the rooms within the area
            for (int x = spawnPosition.x; x < randomWidth; x++)
            {
                for (int y = spawnPosition.y; y < randomHeight; y++)
                {
                    if (x == spawnPosition.x || x == randomWidth - 1)
                    {

                    }
                    else if (y == spawnPosition.y || y == randomHeight - 1)
                    {

                    }

                    map[x, y].ForceCollapseRoom(roomLibrary.DungeonRooms[DungeonRoomType.ROOM_MIDDLE]);
                }
            }
        }
    }

    private List<Vector2Int> FindLowestEntropyPositions()
    {
        List<Vector2Int> lowestEntropyPositions = new List<Vector2Int>();
        int lowestEntropyValue = int.MaxValue;

        // Loop through all possibilities
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                // Get the current entropy at the position
                int entropy = map[x, y].PossibleRooms.Count;

                // If there is a new lowest entropy, clear the list and set it as the new lowest
                if (entropy < lowestEntropyValue && entropy > 0)
                {
                    lowestEntropyPositions.Clear();
                    lowestEntropyValue = entropy;
                }

                // Add to the lowest entropy list if there is a tie for the lowest entropy
                if (entropy == lowestEntropyValue)
                {
                    lowestEntropyPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        return lowestEntropyPositions;
    }

    private void UpdateEntropyFromOrigin(Vector2Int origin, bool forceEntireMap = false)
    {
        // Get a list of the current positions to check
        List<Vector2Int> updatePositions = new List<Vector2Int>() {
            origin + Vector2Int.right,
            origin + Vector2Int.left,
            origin + Vector2Int.up,
            origin + Vector2Int.down
        };
        List<Vector2Int> clearedPositions = new List<Vector2Int>()
        {
            origin
        };

        // Keep updating until there are no more positions to update
        Vector2Int currentPosition;
        RoomOrientation requirements = new RoomOrientation();
        int counter = 0;
        while (updatePositions.Count > 0 && counter < 100)
        {
            // Get the next position to update
            currentPosition = updatePositions[0];
            updatePositions.RemoveAt(0);

            // If the current position is still in the update position list or has already been cleared, then continue
            // If the position is outside the bounds of the map, then also continue to the next position
            if (!IsInsideMap(currentPosition) || updatePositions.Contains(currentPosition) || clearedPositions.Contains(currentPosition))
            {
                continue;
            }

            // At the current position, get data pertaining to the requirements for the room based on surrounding positions
            requirements.PositiveX = GetOrientationAtPosition(currentPosition + Vector2Int.right).NegativeX;
            requirements.NegativeX = GetOrientationAtPosition(currentPosition + Vector2Int.left).PositiveX;
            requirements.PositiveZ = GetOrientationAtPosition(currentPosition + Vector2Int.up).NegativeZ;
            requirements.NegativeZ = GetOrientationAtPosition(currentPosition + Vector2Int.down).PositiveZ;

            // Remove all rooms that do not fit the requirements
            int removedRooms = map[currentPosition.x, currentPosition.y].RemoveUnfitRooms(requirements);

            // If some possible rooms were removed, then add the surrounding positions to the update list
            if (forceEntireMap || (!forceEntireMap && removedRooms > 0))
            {
                updatePositions.Add(currentPosition + Vector2Int.right);
                updatePositions.Add(currentPosition + Vector2Int.left);
                updatePositions.Add(currentPosition + Vector2Int.up);
                updatePositions.Add(currentPosition + Vector2Int.down);
            }

            clearedPositions.Add(currentPosition);
        }
    }

    private RoomOrientation GetOrientationAtPosition(Vector2Int position)
    {
        // If the position is outside the bounds of the map, return a default orientation will all of the values set to be walls
        // This will ensure that any room being collapsed at the edge of the map will have walls surrounding it
        if (!IsInsideMap(position))
        {
            return new RoomOrientation(ConnectionType.WALL);
        }

        return map[position.x, position.y].CollapsedRoom.Orientation;
    }

    private bool IsInsideMap(Vector2Int position)
    {
        return (position.x >= 0 && position.x < size.x && position.y >= 0 && position.y < size.y);
    }
}