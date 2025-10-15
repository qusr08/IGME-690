using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
	[SerializeField] private Vector2Int dungeonSize;
	[SerializeField, Min(0f)] private float generationSpeed;
	[Space]
	[SerializeField, Range(3, 15)] private int areaMinSize;
	[SerializeField, Range(3, 15)] private int areaMaxSize;
	[SerializeField, Range(0f, 10f)] private int areaCount;

	private DungeonRoomLibrary roomLibrary;
	private DungeonEntry[,] map;

	private void Awake()
	{
		roomLibrary = GetComponent<DungeonRoomLibrary>();
	}

	private void Start()
	{
		GenerateMap();
	}

	public void GenerateMap()
	{
		StopAllCoroutines();

		// Destroy all rooms so they can be regenerated
		if (map != null)
		{
			for (int x = 0; x < dungeonSize.x; x++)
			{
				for (int y = 0; y < dungeonSize.y; y++)
				{
					if (map[x, y].RoomGameObject != null)
					{
						Destroy(map[x, y].RoomGameObject);
					}
				}
			}
		}

		StartCoroutine(GenerateMapCoroutine());
	}

	private IEnumerator GenerateMapCoroutine()
	{
		// Create a list of all the dungeon possibility entries for the map
		InitializeRoomPossibilities();

		// Before doing wave function collapse, spawn some starting rooms in the dungeon
		yield return StartCoroutine(SpawnStartingAreas());

		// Update all of the possible rooms of the map before it starts to generate
		UpdateEntropyFromOrigin(Vector2Int.zero, forceEntireMap: true);

		// Use wave function collapse algorithm to create the rest of the dungeon
		yield return StartCoroutine(CollapseDungeon());

		// Find the largest path through the dungeon and remove all of the rooms that are not part of it
		yield return StartCoroutine(IsolateLargestPath());
	}

	private IEnumerator SpawnStartingAreas()
	{
		List<Vector2Int> availableAreaSizes = new List<Vector2Int>();
		List<Vector2Int> availableAreaPositions = new List<Vector2Int>();
		List<Rect> areaRects = new List<Rect>();

		// Get a list of all the possible area sizes
		for (int x = areaMinSize; x <= areaMaxSize; x++)
		{
			for (int y = areaMinSize; y <= areaMaxSize; y++)
			{
				availableAreaSizes.Add(new Vector2Int(x, y));
			}
		}

		// Spawn each of areas into the dungeon
		while (areaRects.Count < areaCount)
		{
			// Select an area size
			int sizeIndex = Random.Range(0, availableAreaSizes.Count);
			Vector2Int areaSize = availableAreaSizes[sizeIndex];

			// Get all of the positions that the area can spawn at
			availableAreaPositions.Clear();
			for (int x = 1; x < dungeonSize.x - areaSize.x - 1; x++)
			{
				for (int y = 1; y < dungeonSize.y - areaSize.y - 1; y++)
				{
					// Make sure the current positions is not already within another area
					Vector2Int position = new Vector2Int(x, y);
					Rect newAreaRect = new Rect(position, areaSize);
					if (!areaRects.Where(area => area.Overlaps(newAreaRect)).Any())
					{
						availableAreaPositions.Add(position);
					}
				}
			}

			// If there are no available positions for the area to spawn at, then remove that size from the list
			if (availableAreaPositions.Count == 0)
			{
				availableAreaSizes.RemoveAt(sizeIndex);
				if (availableAreaSizes.Count == 0)
				{
					break;
				}

				continue;
			}

			// Get a random position for the bottom-left corner of the area
			Vector2Int areaPosition = availableAreaPositions[Random.Range(0, availableAreaPositions.Count)];

			// Add the area to the area rect list
			areaRects.Add(new Rect(areaPosition, areaSize));

			// Get the positions of doors around the area
			// There can be between 1 and 4 doors around the area
			List<Vector2Int> doorPositions = new List<Vector2Int>();
			List<int> availableDoorSides = new List<int>() { 0, 1, 2, 3 };
			int doorCount = Random.Range(1, 5);
			for (int i = 0; i < doorCount; i++)
			{
				// Get a random side for the door to spawn on
				int sideIndex = Random.Range(0, availableDoorSides.Count);
				int doorSide = availableDoorSides[sideIndex];
				availableDoorSides.RemoveAt(sideIndex);

				// Get a position for the door to spawn at
				Vector2Int sidePosition = Vector2Int.zero;
				switch (doorSide)
				{
					case 0: // Right
						sidePosition = new Vector2Int(areaSize.x - 1, Random.Range(1, areaSize.y - 2));
						break;
					case 1: // Down
						sidePosition = new Vector2Int(Random.Range(1, areaSize.x - 2), 0);
						break;
					case 2: // Left
						sidePosition = new Vector2Int(0, Random.Range(1, areaSize.y - 2));
						break;
					case 3: // Up
						sidePosition = new Vector2Int(Random.Range(1, areaSize.x - 2), areaSize.y - 1);
						break;
				}

				doorPositions.Add(sidePosition + areaPosition);
			}

			// Collapse all the rooms within the area
			RoomOrientation requirements = new RoomOrientation();
			for (int x = areaPosition.x; x < areaPosition.x + areaSize.x; x++)
			{
				for (int y = areaPosition.y; y < areaPosition.y + areaSize.y; y++)
				{
					Vector2Int currentPosition = new Vector2Int(x, y);
					bool doorSpawn = doorPositions.Contains(currentPosition);

					// Check the surrounding rooms to make sure that all rooms can lead into each other
					requirements.PositiveX = GetOrientationAtPosition(currentPosition + Vector2Int.right).NegativeX;
					if (requirements.PositiveX == ConnectionType.ANY)
					{
						requirements.PositiveX = (x == areaPosition.x + areaSize.x - 1 && !doorSpawn ? ConnectionType.WALL : ConnectionType.AIR);
					}

					requirements.NegativeX = GetOrientationAtPosition(currentPosition + Vector2Int.left).PositiveX;
					if (requirements.NegativeX == ConnectionType.ANY)
					{
						requirements.NegativeX = (x == areaPosition.x && !doorSpawn ? ConnectionType.WALL : ConnectionType.AIR);
					}

					requirements.PositiveZ = GetOrientationAtPosition(currentPosition + Vector2Int.up).NegativeZ;
					if (requirements.PositiveZ == ConnectionType.ANY)
					{
						requirements.PositiveZ = (y == areaPosition.y + areaSize.y - 1 && !doorSpawn ? ConnectionType.WALL : ConnectionType.AIR);
					}

					requirements.NegativeZ = GetOrientationAtPosition(currentPosition + Vector2Int.down).PositiveZ;
					if (requirements.NegativeZ == ConnectionType.ANY)
					{
						requirements.NegativeZ = (y == areaPosition.y && !doorSpawn ? ConnectionType.WALL : ConnectionType.AIR);
					}

					// With the above requirements, there should only be one room that can be placed
					map[x, y].RemoveUnfitRooms(requirements);
					map[x, y].CollapsePossibleRooms();

					yield return new WaitForSeconds(generationSpeed);
				}
			}
		}
	}

	private IEnumerator CollapseDungeon()
	{
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
			map[collapsePosition.x, collapsePosition.y].CollapsePossibleRooms();
			collapseCount++;

			// Update possible rooms for surrounding possibilities
			UpdateEntropyFromOrigin(collapsePosition);

			yield return new WaitForSeconds(generationSpeed);
		}
	}

	private IEnumerator IsolateLargestPath()
	{
		// Get a list of all the positions in the dungeon
		// These positions will slowly deminish 
		List<Vector2Int> unsearchedPositions = new List<Vector2Int>();
		for (int x = 0; x < dungeonSize.x; x++)
		{
			for (int y = 0; y < dungeonSize.y; y++)
			{
				unsearchedPositions.Add(new Vector2Int(x, y));
			}
		}

		// A list of all the paths through the dungeon
		List<List<Vector2Int>> dungeonPaths = new List<List<Vector2Int>>();
		int currentPathIndex = -1;
		int largestPathLength = 0;
		int largestPathIndex = -1;

		// A list of all the positions that were just searched in the previous loop
		List<Vector2Int> searchPositions = new List<Vector2Int>();

		// Keep looping until every position is searched
		while (unsearchedPositions.Count > 0)
		{
			if (searchPositions.Count == 0)
			{
				currentPathIndex++;
				dungeonPaths.Add(new List<Vector2Int>() { unsearchedPositions[0] });
				searchPositions.Add(unsearchedPositions[0]);
				unsearchedPositions.RemoveAt(0);
				continue;
			}

			// Based on the last searched positions, add adjacent positions to
			List<Vector2Int> newPositions = new List<Vector2Int>();
			foreach (Vector2Int lastPosition in searchPositions)
			{
				RoomOrientation orientation = GetOrientationAtPosition(lastPosition);

				// If the current room connects to air on a specific side, then add that adjacent position to the list of new positions
				// This will allow for the dungeon path to grow over time
				if (orientation.PositiveX == ConnectionType.AIR)
				{
					newPositions.Add(lastPosition + Vector2Int.right);
				}
				if (orientation.NegativeX == ConnectionType.AIR)
				{
					newPositions.Add(lastPosition + Vector2Int.left);
				}
				if (orientation.PositiveZ == ConnectionType.AIR)
				{
					newPositions.Add(lastPosition + Vector2Int.up);
				}
				if (orientation.NegativeZ == ConnectionType.AIR)
				{
					newPositions.Add(lastPosition + Vector2Int.down);
				}
			}

			// Add the newly searched positions to the last searched position list
			searchPositions.Clear();
			foreach (Vector2Int newPosition in newPositions)
			{
				// Make sure there are no duplicate positions, only take from the unsearched positions
				if (!unsearchedPositions.Contains(newPosition))
				{
					continue;
				}

				searchPositions.Add(newPosition);
				dungeonPaths[currentPathIndex].Add(newPosition);
				unsearchedPositions.Remove(newPosition);

				// Track the largest path length/index as the paths are discovered
				if (dungeonPaths[currentPathIndex].Count > largestPathLength)
				{
					largestPathLength = dungeonPaths[currentPathIndex].Count;
					largestPathIndex = currentPathIndex;
				}
			}
		}

		// Destroy all dungeon rooms not part of the largest path
		for (int i = 0; i < dungeonPaths.Count; i++)
		{
			// Make sure not to destroy the largest path
			if (i == largestPathIndex)
			{
				continue;
			}

			for (int j = 0; j < dungeonPaths[i].Count; j++)
			{
				Vector2Int position = dungeonPaths[i][j];
				Destroy(map[position.x, position.y].RoomGameObject);

				yield return new WaitForSeconds(generationSpeed);
			}
		}
	}

	private void InitializeRoomPossibilities()
	{
		map = new DungeonEntry[dungeonSize.x, dungeonSize.y];

		for (int x = 0; x < dungeonSize.x; x++)
		{
			for (int y = 0; y < dungeonSize.y; y++)
			{
				// At the start, each room can be any possibility
				DungeonEntry possibility = new DungeonEntry(this, x, y);
				possibility.PossibleRooms.AddRange(roomLibrary.AllDungeonRooms);
				map[x, y] = possibility;
			}
		}
	}

	private List<Vector2Int> FindLowestEntropyPositions()
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

	private void UpdateEntropyFromOrigin(Vector2Int origin, bool forceEntireMap = false)
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

	private RoomOrientation GetOrientationAtPosition(Vector2Int position)
	{
		// If the position is outside the bounds of the map, return a default orientation will all of the values set to be walls
		// This will ensure that any room being collapsed at the edge of the map will have walls surrounding it
		if (!IsInsideMap(position))
		{
			return new RoomOrientation(ConnectionType.WALL);
		}

		return map[position.x, position.y].CollapsedRoom.Orientation;
	}

	private bool IsInsideMap(Vector2Int position)
	{
		return (position.x >= 0 && position.x < dungeonSize.x && position.y >= 0 && position.y < dungeonSize.y);
	}

	public void SpawnDungeonRoomPrefab(DungeonEntry entry)
	{
		Vector3 spawnPosition = roomLibrary.RoomSize * new Vector3(entry.MapPosition.x, 0f, entry.MapPosition.y);
		Quaternion spawnRotation = Quaternion.Euler(0, entry.CollapsedRoom.Orientation.Rotation * 90, 0);
		entry.RoomGameObject = Instantiate(entry.CollapsedRoom.Prefab, spawnPosition, spawnRotation, transform);
	}
}