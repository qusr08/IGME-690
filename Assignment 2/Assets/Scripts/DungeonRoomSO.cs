using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Dungeon Room", menuName = "Dungeon Room")]
[Serializable]
public class DungeonRoomSO : ScriptableObject
{
	[SerializeField] private DungeonRoom _data;

	public DungeonRoom Data => _data;
	public GameObject Prefab => Data.Prefab;
	public float SpawnChance => Data.SpawnChance;
	public RoomOrientation Orientation => Data.Orientation;
}