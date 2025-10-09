using System.Collections.Generic;
using UnityEngine;

public class DungeonEntry
{
    public List<DungeonRoom> PossibleRooms { get; private set; }
    public DungeonRoom CollapsedRoom { get; private set; }
    public Vector2Int MapPosition { get; private set; }
    public bool IsCollapsed { get; private set; }

    public DungeonEntry(int x, int y)
    {
        PossibleRooms = new List<DungeonRoom>();
        CollapsedRoom = new DungeonRoom(null, 0, new RoomOrientation());
        MapPosition = new Vector2Int(x, y);
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

    public void ForceCollapseRoom(DungeonRoom room)
    {
        if (IsCollapsed)
        {
            return;
        }

        CollapsedRoom = room;
        IsCollapsed = true;
        PossibleRooms.Clear();
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