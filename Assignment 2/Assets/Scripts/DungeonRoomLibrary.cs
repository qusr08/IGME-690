using System;
using System.Collections.Generic;
using UnityEngine;

public enum ConnectionType
{
    ANY, AIR, WALL
}

public class DungeonRoomLibrary : MonoBehaviour
{
    [SerializeField] private List<DungeonRoom> _dungeonRooms;
    [SerializeField, Min(0)] private int _roomSize;

    public List<DungeonRoom> DungeonRooms => _dungeonRooms;
    public int RoomSize => _roomSize;
    public List<DungeonRoom> AllDungeonRooms { get; private set; }

    private void Awake()
    {
        AllDungeonRooms = new List<DungeonRoom>();

        // Get a list of all dungeon rooms in every possible rotation
        foreach (DungeonRoom room in DungeonRooms)
        {
            RoomOrientation orientation = room.Orientation;
            for (int j = 0; j < 4; j++)
            {
                AllDungeonRooms.Add(new DungeonRoom(room.Prefab, room.Index, room.SpawnChance, orientation.RotateClockwise()));
            }
        }
    }
}

[Serializable]
public class DungeonRoom
{
    public GameObject Prefab;
    public int Index;
    public float SpawnChance;
    public RoomOrientation Orientation;

    public DungeonRoom(GameObject prefab, int index, float spawnChance, RoomOrientation orientation)
    {
        Prefab = prefab;
        Index = index;
        SpawnChance = spawnChance;
        Orientation = orientation;
    }

    public DungeonRoom(DungeonRoom parent, int rotation)
    {
        Prefab = parent.Prefab;
        Index = parent.Index;
        SpawnChance = parent.SpawnChance;
        Orientation = parent.Orientation;
        Orientation.Rotation = rotation;
    }
}

[Serializable]
public class RoomOrientation
{
    public ConnectionType PositiveX;
    public ConnectionType NegativeX;
    public ConnectionType PositiveY;
    public ConnectionType NegativeY;
    public ConnectionType PositiveZ;
    public ConnectionType NegativeZ;
    public int Rotation;

    private ConnectionType defaultType;

    public RoomOrientation(RoomOrientation parent, ConnectionType defaultType = ConnectionType.ANY)
    {
        this.defaultType = defaultType;
        SetValuesFromParent(parent);
    }

    public RoomOrientation(ConnectionType defaultType = ConnectionType.ANY) : this(null, defaultType) { }

    public RoomOrientation RotateClockwise()
    {
        RoomOrientation rotated = new RoomOrientation(this);

        rotated.PositiveX = NegativeZ;
        rotated.NegativeX = PositiveZ;
        rotated.PositiveY = PositiveY;
        rotated.NegativeY = NegativeY;
        rotated.PositiveZ = PositiveX;
        rotated.NegativeZ = NegativeX;
        rotated.Rotation = (Rotation + 1) % 4;

        SetValuesFromParent(rotated);
        return rotated;
    }

    public void SetValuesFromParent(RoomOrientation parent)
    {
        PositiveX = parent != null ? parent.PositiveX : defaultType;
        NegativeX = parent != null ? parent.NegativeX : defaultType;
        PositiveY = parent != null ? parent.PositiveY : defaultType;
        NegativeY = parent != null ? parent.NegativeY : defaultType;
        PositiveZ = parent != null ? parent.PositiveZ : defaultType;
        NegativeZ = parent != null ? parent.NegativeZ : defaultType;
        Rotation = parent != null ? parent.Rotation : 0;
    }

    public override string ToString()
    {
        return $"+X: {PositiveX} | -X: {NegativeX} | +Y: {PositiveY} | -Y: {NegativeY} | +Z: {PositiveZ} | -Z: {NegativeZ}";
    }
}