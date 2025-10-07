using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private Vector3Int size;

    private DungeonRoomLibrary roomLibrary;
    private DungeonEntry[,] map;

    private void Awake()
    {
        roomLibrary = GetComponent<DungeonRoomLibrary>();
        map = new DungeonEntry[size.x, size.z];
    }

    private void Start()
    {
        StartCoroutine(GenerateMap());
    }

    private IEnumerator GenerateMap()
    {
        // Create a list of all the dungeon possibility entries for the map
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                DungeonEntry possibility = new DungeonEntry(x, 0, z);
                possibility.PossibleRooms.AddRange(roomLibrary.AllDungeonRooms);
                map[x, z] = possibility;
                yield return null;
            }
        }

        // Update all of the possible rooms of the map before it starts to generate
        UpdateEntropyFromOrigin(Vector3Int.zero);

        int collapseCount = 0;
        while (collapseCount < size.x * size.z)
        {
            // Get the positions of the lowest entropy on the map
            List<Vector3Int> lowestEntropyPositions = FindLowestEntropyPositions();

            // If there are no more positions to collapse, then break from the loop
            if (lowestEntropyPositions.Count == 0)
            {
                break;
            }

            // Randomly select possibility to resolve
            Vector3Int collapsePosition = lowestEntropyPositions[Random.Range(0, lowestEntropyPositions.Count)];

            // Randomly select possible room within that possibility
            map[collapsePosition.x, collapsePosition.z].CollapsePossibleRooms();
            collapseCount++;

            // Update possible rooms for surrounding possibilities
            UpdateEntropyFromOrigin(collapsePosition);

            // Get the currently collapsed room at the map location
            DungeonRoom room = map[collapsePosition.x, collapsePosition.z].CollapsedRoom;

            // If there is no collapsed room there, continue to the next position
            if (room == null)
            {
                continue;
            }

            Vector3 position = roomLibrary.RoomSize * collapsePosition;
            Quaternion rotation = Quaternion.Euler(0, room.Orientation.Rotation * -90, 0);
            Instantiate(room.Prefab, position, rotation, transform);

            yield return new WaitForSeconds(0.1f);
        }

        //// Spawn all room prefabs based on the collapsed rooms
        //for (int x = 0; x < size.x; x++)
        //{
        //    for (int z = 0; z < size.z; z++)
        //    {
        //        // Get the currently collapsed room at the map location
        //        DungeonRoom room = map[x, z].CollapsedRoom;

        //        // If there is no collapsed room there, continue to the next position
        //        if (room.Prefab == null)
        //        {
        //            continue;
        //        }

        //        Vector3 position = roomLibrary.RoomSize * new Vector3(x, 0, z);
        //        Quaternion rotation = Quaternion.Euler(0, room.Orientation.Rotation * 90, 0);
        //        Instantiate(room.Prefab, position, rotation, transform);
        //    }
        //}
    }

    private List<Vector3Int> FindLowestEntropyPositions()
    {
        List<Vector3Int> lowestEntropyPositions = new List<Vector3Int>();
        int lowestEntropyValue = int.MaxValue;

        // Loop through all possibilities
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                // Get the current entropy at the position
                int entropy = map[x, z].PossibleRooms.Count;

                // If there is a new lowest entropy, clear the list and set it as the new lowest
                if (entropy < lowestEntropyValue && entropy > 0)
                {
                    lowestEntropyPositions.Clear();
                    lowestEntropyValue = entropy;
                }

                // Add to the lowest entropy list if there is a tie for the lowest entropy
                if (entropy == lowestEntropyValue)
                {
                    lowestEntropyPositions.Add(new Vector3Int(x, 0, z));
                }
            }
        }

        return lowestEntropyPositions;
    }

    private void UpdateEntropyFromOrigin(Vector3Int origin)
    {
        // Get a list of the current positions to check
        List<Vector3Int> updatePositions = new List<Vector3Int>() {
            origin + Vector3Int.right,
            origin + Vector3Int.left,
            origin + Vector3Int.forward,
            origin + Vector3Int.back
        };
        List<Vector3Int> clearedPositions = new List<Vector3Int>()
        {
            origin
        };

        // Keep updating until there are no more positions to update
        Vector3Int currentPosition;
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
            requirements.PositiveX = GetOrientation(currentPosition + Vector3Int.right).NegativeX;
            requirements.NegativeX = GetOrientation(currentPosition + Vector3Int.left).PositiveX;
            requirements.PositiveZ = GetOrientation(currentPosition + Vector3Int.forward).NegativeZ;
            requirements.NegativeZ = GetOrientation(currentPosition + Vector3Int.back).PositiveZ;

            // Remove all rooms that do not fit the requirements
            int removedRooms = map[currentPosition.x, currentPosition.z].RemoveUnfitRooms(requirements);

            // If some possible rooms were removed, then add the surrounding positions to the update list
            if (removedRooms > 0)
            {
                updatePositions.Add(currentPosition + Vector3Int.right);
                updatePositions.Add(currentPosition + Vector3Int.left);
                updatePositions.Add(currentPosition + Vector3Int.forward);
                updatePositions.Add(currentPosition + Vector3Int.back);
            }

            clearedPositions.Add(currentPosition);
        }
    }

    private RoomOrientation GetOrientation(Vector3Int position)
    {
        // If the position is outside the bounds of the map, return a default orientation will all of the values set to be walls
        // This will ensure that any room being collapsed at the edge of the map will have walls surrounding it
        if (!IsInsideMap(position))
        {
            return new RoomOrientation(ConnectionType.WALL);
        }

        return map[position.x, position.z].CollapsedRoom.Orientation;
    }

    private bool IsInsideMap(Vector3Int position)
    {
        return (position.x >= 0 && position.x < size.x && position.z >= 0 && position.z < size.z);
    }
}

public class DungeonEntry
{
    public List<DungeonRoom> PossibleRooms { get; private set; }
    public DungeonRoom CollapsedRoom { get; private set; }
    public Vector3Int MapPosition { get; private set; }
    public bool IsCollapsed { get; private set; }

    public DungeonEntry(int x, int y, int z)
    {
        PossibleRooms = new List<DungeonRoom>();
        CollapsedRoom = new DungeonRoom(null, -1, 0, new RoomOrientation());
        MapPosition = new Vector3Int(x, y, z);
        IsCollapsed = false;
    }

    public int RemoveUnfitRooms(RoomOrientation requirements)
    {
        int removedRooms = 0;

        // Loop through possible rooms for the current position and remove ones that do not match the requirements
        for (int i = PossibleRooms.Count - 1; i >= 0; i--)
        {
            bool matchPositiveX = requirements.PositiveX == ConnectionType.ANY || PossibleRooms[i].Orientation.PositiveX == requirements.PositiveX;
            bool matchNegativeX = requirements.NegativeX == ConnectionType.ANY || PossibleRooms[i].Orientation.NegativeX == requirements.NegativeX;
            bool matchPositiveZ = requirements.PositiveZ == ConnectionType.ANY || PossibleRooms[i].Orientation.PositiveZ == requirements.PositiveZ;
            bool matchNegativeZ = requirements.NegativeZ == ConnectionType.ANY || PossibleRooms[i].Orientation.NegativeZ == requirements.NegativeZ;

            // If the current room does not match any one of the directions, then 
            if (!matchPositiveX || !matchNegativeX || !matchPositiveZ || !matchNegativeZ)
            {
                PossibleRooms.RemoveAt(i);
                removedRooms++;
            }
        }

        return removedRooms;
    }

    public DungeonRoom CollapsePossibleRooms()
    {
        if (IsCollapsed || PossibleRooms.Count == 0)
        {
            return null;
        }

        // Get a list containing all spawn chances at their corresponding indices
        List<float> roomSpawnChances = new List<float>();
        float totalSpawnChanceValue = 0;
        foreach (DungeonRoom room in PossibleRooms)
        {
            roomSpawnChances.Add(room.SpawnChance);
            totalSpawnChanceValue += room.SpawnChance;
        }

        // Based on a random value, select a certain room index to be collapsed to
        float randomValue = Random.Range(0f, 1f);
        float spawnChanceSum = 0f;
        for (int i = 0; i < roomSpawnChances.Count; i++)
        {
            // Adjust all spawn chances so they add up to 100%
            spawnChanceSum += roomSpawnChances[i] * (1f / totalSpawnChanceValue);

            if (spawnChanceSum >= randomValue || i == roomSpawnChances.Count - 1)
            {
                CollapsedRoom = PossibleRooms[i];
                IsCollapsed = true;
                PossibleRooms.Clear();

                return CollapsedRoom;
            }
        }

        return null;
    }
}