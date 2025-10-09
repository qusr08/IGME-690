using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Dungeon Room", menuName = "Dungeon Room")]
[Serializable]
public class DungeonRoom : ScriptableObject
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