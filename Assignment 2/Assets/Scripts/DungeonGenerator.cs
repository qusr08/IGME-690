using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonGenerator : MonoBehaviour
{
	[SerializeField] private Vector2Int dungeonSize;
	[SerializeField] private Vector2Int roomMinSize;
	[SerializeField] private Vector2Int roomMaxSize;
	[SerializeField] private int roomCount;
	[SerializeField, Min(0f)] private float generationSpeed;

	private DungeonRoomLibrary roomLibrary;
	private DungeonEntry[,] map;
	private List<GameObject> dungeonRoomObjects;

	private void Awake ()
	{
		roomLibrary = GetComponent<DungeonRoomLibrary>();
		dungeonRoomObjects = new List<GameObject>();
	}

	private void Start ()
	{
		GenerateMap();
	}

	public void GenerateMap ()
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

	private IEnumerator GenerateMapCoroutine ()
	{
		// Create a list of all the dungeon possibility entries for the map
		InitializeRoomPossibilities();

		// Before doing wave function collapse, spawn some starting rooms in the dungeon
		yield return StartCoroutine(SpawnStartingRooms());

		// Update all of the possible rooms of the map before it starts to generate
		UpdateEntropyFromOrigin(Vector2Int.zero, forceEntireMap: true);

		int collapseCount = 0;
		while (collapseCount < dungeonSize.x * dungeonSize.y)
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
			SpawnDungeonRoomPrefab(map[collapsePosition.x, collapsePosition.y].CollapsePossibleRooms(), collapsePosition);
			collapseCount++;

			// Update possible rooms for surrounding possibilities
			UpdateEntropyFromOrigin(collapsePosition);

			yield return new WaitForSeconds(generationSpeed);
		}
	}

	private void InitializeRoomPossibilities ()
	{
		map = new DungeonEntry[dungeonSize.x, dungeonSize.y];

		for (int x = 0; x < dungeonSize.x; x++)
		{
			for (int y = 0; y < dungeonSize.y; y++)
			{
				// At the start, each room can be any possibility
				DungeonEntry possibility = new DungeonEntry(x, y);
				possibility.PossibleRooms.AddRange(roomLibrary.AllDungeonRooms);
				map[x, y] = possibility;
			}
		}
	}

	private IEnumerator SpawnStartingRooms ()
	{
		List<Vector2Int> availableRoomSizes = new List<Vector2Int>();
		List<Vector2Int> availableRoomPositions = new List<Vector2Int>();
		List<Rect> roomRects = new List<Rect>();

		// Get a list of all the possible room sizes
		for (int x = roomMinSize.x; x <= roomMaxSize.x; x++)
		{
			for (int y = roomMinSize.y; y <= roomMaxSize.y; y++)
			{
				availableRoomSizes.Add(new Vector2Int(x, y));
			}
		}

		// Spawn each of rooms into the dungeon
		while (roomRects.Count < roomCount)
		{
			// Select a room size
			int sizeIndex = Random.Range(0, availableRoomSizes.Count);
			Vector2Int roomSize = availableRoomSizes[sizeIndex];

			// Get all of the positions that the room can spawn at
			availableRoomPositions.Clear();
			for (int x = 0; x < dungeonSize.x - roomSize.x; x++)
			{
				for (int y = 0; y < dungeonSize.y - roomSize.y; y++)
				{
					// Make sure the current positions is not already within another room
					Vector2Int position = new Vector2Int(x, y);
					Rect newRoomRect = new Rect(position, roomSize);
					if (!roomRects.Where(room => room.Overlaps(newRoomRect)).Any())
					{
						availableRoomPositions.Add(position);
					}
				}
			}

			// If there are no available positions for the room to spawn at, then remove that size from the equation
			if (availableRoomPositions.Count == 0)
			{
				availableRoomSizes.RemoveAt(sizeIndex);
				if (availableRoomSizes.Count == 0)
				{
					break;
				}

				continue;
			}

			// Get a random position for the top-left corner of the room
			Vector2Int roomPosition = availableRoomPositions[Random.Range(0, availableRoomPositions.Count)];

			// Add the room to the room rect list
			roomRects.Add(new Rect(roomPosition, roomSize));

			// Collapse all the rooms within the area
			RoomOrientation requirements = new RoomOrientation();
			for (int x = roomPosition.x; x < roomPosition.x + roomSize.x; x++)
			{
				requirements.NegativeX = (x == roomPosition.x ? ConnectionType.WALL : ConnectionType.AIR);
				requirements.PositiveX = (x == roomPosition.x + roomSize.x - 1 ? ConnectionType.WALL : ConnectionType.AIR);

				for (int y = roomPosition.y; y < roomPosition.y + roomSize.y; y++)
				{
					requirements.NegativeZ = (y == roomPosition.y ? ConnectionType.WALL : ConnectionType.AIR);
					requirements.PositiveZ = (y == roomPosition.y + roomSize.y - 1 ? ConnectionType.WALL : ConnectionType.AIR);

					map[x, y].RemoveUnfitRooms(requirements);
					SpawnDungeonRoomPrefab(map[x, y].CollapsePossibleRooms(), new Vector2Int(x, y));

					yield return new WaitForSeconds(generationSpeed);
				}
			}
		}
	}

	private List<Vector2Int> FindLowestEntropyPositions ()
	{
		List<Vector2Int> lowestEntropyPositions = new List<Vector2Int>();
		int lowestEntropyValue = int.MaxValue;

		// Loop through all possibilities
		for (int x = 0; x < dungeonSize.x; x++)
		{
			for (int y = 0; y < dungeonSize.y; y++)
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

	private void UpdateEntropyFromOrigin (Vector2Int origin, bool forceEntireMap = false)
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
		while (updatePositions.Count > 0)
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

	private RoomOrientation GetOrientationAtPosition (Vector2Int position)
	{
		// If the position is outside the bounds of the map, return a default orientation will all of the values set to be walls
		// This will ensure that any room being collapsed at the edge of the map will have walls surrounding it
		if (!IsInsideMap(position))
		{
			return new RoomOrientation(ConnectionType.WALL);
		}

		return map[position.x, position.y].CollapsedRoom.Orientation;
	}

	private bool IsInsideMap (Vector2Int position)
	{
		return (position.x >= 0 && position.x < dungeonSize.x && position.y >= 0 && position.y < dungeonSize.y);
	}

	private void SpawnDungeonRoomPrefab (DungeonRoom room, Vector2Int position)
	{
		if (room == null)
		{
			return;
		}

		Vector3 spawnPosition = roomLibrary.RoomSize * new Vector3(position.x, 0f, position.y);
		Quaternion spawnRotation = Quaternion.Euler(0, room.Orientation.Rotation * 90, 0);
		dungeonRoomObjects.Add(Instantiate(room.Prefab, spawnPosition, spawnRotation, transform));
	}
}