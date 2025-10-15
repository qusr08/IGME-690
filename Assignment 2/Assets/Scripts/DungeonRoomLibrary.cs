using System.Collections.Generic;
using UnityEngine;

public class DungeonRoomLibrary : MonoBehaviour
{
    [SerializeField] private DungeonRoomDictionary _dungeonRooms;
    [SerializeField, Min(0)] private int _roomSize;

    public DungeonRoomDictionary DungeonRooms => _dungeonRooms;
    public int RoomSize => _roomSize;
    public List<DungeonRoom> AllDungeonRooms { get; private set; }

    private void Awake()
    {
        AllDungeonRooms = new List<DungeonRoom>();

        // Get a list of all dungeon rooms in every possible rotation
        for (int i = 0; i < DungeonRooms.Count; i++)
        {
            RoomOrientation orientation = DungeonRooms[(DungeonRoomType)i].Orientation;
            for (int j = 0; j < 4; j++)
            {
                AllDungeonRooms.Add(new DungeonRoom(DungeonRooms[(DungeonRoomType)i].Data, orientation.RotateClockwise()));
            }
        }
    }
}