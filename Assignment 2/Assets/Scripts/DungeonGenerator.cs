using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    private DungeonRoomLibrary roomLibrary;

    private void Awake()
    {
        roomLibrary = GetComponent<DungeonRoomLibrary>();
    }
}
