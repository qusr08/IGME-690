using System;
using System.Collections.Generic;
using UnityEngine;

public enum ConnectionType
{
    AIR, WALL
}

public class DungeonRoomLibrary : MonoBehaviour
{
    [SerializeField] private List<DungeonRoom> _dungeonRooms;

    public List<DungeonRoom> DungeonRooms => _dungeonRooms;
}

[Serializable]
public class DungeonRoom
{
    public GameObject RoomPrefab;
    public ConnectionType PositiveXID;
    public ConnectionType NegativeXID;
    public ConnectionType PositiveYID;
    public ConnectionType NegativeYID;
    public ConnectionType PositiveZID;
    public ConnectionType NegativeZID;
}