using System;
using UnityEngine;

[Serializable]
public class DungeonRoom
{
    public GameObject Prefab;
    public float SpawnChance;
    public RoomOrientation Orientation;

    public DungeonRoom(GameObject prefab, float spawnChance, RoomOrientation orientation)
    {
        Prefab = prefab;
        SpawnChance = spawnChance;
        Orientation = orientation;
    }

    public DungeonRoom(DungeonRoom parent, RoomOrientation orientation) : this(parent.Prefab, parent.SpawnChance, orientation)
    {
    }
}