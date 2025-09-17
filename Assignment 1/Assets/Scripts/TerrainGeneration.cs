using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TerrainGeneration : MonoBehaviour
{
	[Header("Scene Objects")]
	[SerializeField] private TextMeshProUGUI aText;
	[SerializeField] private Slider aSlider;
	[SerializeField] private TextMeshProUGUI bText;
	[SerializeField] private Slider bSlider;
	[SerializeField] private TMP_Dropdown equationDropdown;
    [Header("Terrain Variables")]
	[SerializeField] private int terrainWidth;
	[SerializeField] private int terrainDepth;
	[SerializeField] private int noiseHeightMultiplier;
    [SerializeField] private Material terrainMaterial;
    [SerializeField, Range(0f, 1f)] private float treeSpawnPercentage;
    [SerializeField] private TreeObjectPool treeObjectPool;
    [Header("Perlin Noise Variables")]
	[SerializeField] private float frequency = 1.0f;
	[SerializeField] private float amplitude = 0.5f;
	[SerializeField] private float lacunarity = 2.0f;
	[SerializeField] private float gain = 0.5f;
	[SerializeField] private int octaves = 8;
	[SerializeField] private float scale = 0.01f;
	[SerializeField] private float normalizeBias = 1.0f;

	private GameObject terrainObject;
	private MeshFilter terrainMeshFilter;
	private MeshRenderer terrainMeshRenderer;
	private NativeArray<float> terrainHeightMap;
	private float[] terrainFullHeightMap;
	private List<MapFunction> mapFunctions;

	private List<Vector2Int> treePositions;

	public float A
	{
		get => _a;
		set
		{
			_a = value;
			Vector2 aRange = mapFunctions[equationDropdown.value].ARange;
			aText.text = $"<b>A</b> = {_a:0.0000} [{aRange.x} to {aRange.y}]";
			UpdateTerrainMesh();
        }
	}
	private float _a;

	public float B
	{
		get => _b;
		set
		{
			_b = value;
			Vector2 bRange = mapFunctions[equationDropdown.value].BRange;
			bText.text = $"<b>B</b> = {_b:0.0000} [{bRange.x} to {bRange.y}]";
			UpdateTerrainMesh();
        }
	}
	private float _b;

	private void Start ( )
	{
        // Create a height map using perlin noise and fractal brownian motion
        NoiseAlgorithm terrainNoise = new NoiseAlgorithm( );
		terrainNoise.InitializeNoise(terrainWidth + 1, terrainDepth + 1, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
		terrainNoise.InitializePerlinNoise(frequency, amplitude, octaves, lacunarity, gain, scale, normalizeBias);
		terrainHeightMap = new NativeArray<float>((terrainWidth + 1) * (terrainDepth + 1), Allocator.Persistent);
		terrainFullHeightMap = new float[(terrainWidth + 1) * (terrainDepth + 1)];
        terrainNoise.setNoise(terrainHeightMap, 0, 0);
		NoiseAlgorithm.OnExit( );

		// Create the mesh and set it to the terrain variable
		terrainObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        terrainObject.transform.position = new Vector3(-terrainWidth / 2f, -noiseHeightMultiplier / 2f, -terrainDepth / 2f);
		treeObjectPool.transform.position = new Vector3(-terrainWidth / 2f + 1, -noiseHeightMultiplier / 2f, -terrainDepth / 2f);
        terrainMeshFilter = terrainObject.GetComponent<MeshFilter>( );
        terrainMeshRenderer = terrainObject.GetComponent<MeshRenderer>();
        terrainMeshRenderer.material = terrainMaterial;

        treePositions = new List<Vector2Int>();

        mapFunctions = new List<MapFunction>( )
		{
			new MapFunction("xcos(<b>A</b>x)sin(<b>B</b>z)",
				new Vector2(0f, 0.2f), new Vector2(0f, 0.2f),
				(float x, float z) => { return x * Mathf.Cos(A * x) * Mathf.Sin(B * z); }),
			new MapFunction("<b>A</b>sin(<b>B</b>sqrt(x^2 + z^2))",
				new Vector2(0f, 5f), new Vector2(0f, 1f),
				(float x, float z) => { return A * Mathf.Sin(B * Mathf.Sqrt((x * x) + (z * z))); }),
			new MapFunction("-<b>A</b>(<b>B</b> - 0.2sqrt(x^2 + z^2))^2",
				new Vector2(-1f, 5f), new Vector2(0f, 5f),
				(float x, float z) => { return -A * Mathf.Pow(B - (0.2f * Mathf.Sqrt((x * x) + (z * z))), 2); }),
			new MapFunction("<b>A</b>zsin(<b>B</b>xy)",
				new Vector2(0f, 1f), new Vector2(0f, 0.05f),
				(float x, float z) => { return A * z * Mathf.Sin(B * x * z); }),
			new MapFunction("<b>A</b>cos(<b>B</b>(|x| + |z|))",
				new Vector2(0f, 5f), new Vector2(0f, 0.5f),
				(float x, float z) => { return A * Mathf.Cos(B * (Mathf.Abs(x) + Mathf.Abs(z))); }),
		};

		// z * Mathf.Sin(x * x);
		// 0.001f * ((A * x * z * z * z) - (B * z * x * x * x));

		List<TMP_Dropdown.OptionData> optionsList = new List<TMP_Dropdown.OptionData>( );
		for (int i = 0; i < mapFunctions.Count; i++)
		{
			optionsList.Add(new TMP_Dropdown.OptionData(mapFunctions[i].FunctionText));
		}
		equationDropdown.AddOptions(optionsList);

		// Add UI value change listeners
		aSlider.onValueChanged.AddListener((v) => { A = v; });
		bSlider.onValueChanged.AddListener((v) => { B = v; });
		equationDropdown.onValueChanged.AddListener(SetCurrentMapFunction);

		// Set the default map function and update the terrain mesh
		SetCurrentMapFunction(0);
	}

	public Mesh GenerateTerrainMesh ( )
	{
		int width = terrainWidth + 1, depth = terrainDepth + 1;
		int indicesIndex = 0;
		int vertexIndex = 0;
		int vertexMultiplier = 4; // Create quads to fit uv's to so we can use more than one uv (4 vertices to a quad)

		Mesh terrainMesh = new Mesh( );
		terrainMesh.indexFormat = IndexFormat.UInt32;
		List<Vector3> vert = new List<Vector3>(width * depth * vertexMultiplier);
		List<int> indices = new List<int>(width * depth * 6);
		List<Vector2> uvs = new List<Vector2>(width * depth);

        treePositions.Clear();

        for (int x = 0; x < width; x++)
		{
			for (int z = 0; z < depth; z++)
			{
				if (x >= width - 1 || z >= depth - 1)
				{
					continue;
				}

				// Note: since perlin goes up to 1.0 multiplying by a height will tend to set
				// The average around maxheight/2. We remove most of that extra by subtracting maxheight/2
				// So our ground isn't always way up in the air
				float y = EvaluateMapFunctionAt(x, z, width, depth) + terrainHeightMap[(x) * (depth) + (z)] * noiseHeightMultiplier;
				float useAltXPlusY = EvaluateMapFunctionAt(x + 1, z, width, depth) + terrainHeightMap[(x + 1) * (depth) + (z)] * noiseHeightMultiplier;
				float useAltZPlusY = EvaluateMapFunctionAt(x, z + 1, width, depth) + terrainHeightMap[(x) * (depth) + (z + 1)] * noiseHeightMultiplier;
				float useAltXAndZPlusY = EvaluateMapFunctionAt(x + 1, z + 1, width, depth) + terrainHeightMap[(x + 1) * (depth) + (z + 1)] * noiseHeightMultiplier;
				terrainFullHeightMap[(x) * (depth) + (z)] = y;
				terrainFullHeightMap[(x + 1) * (depth) + (z + 1)] = useAltXAndZPlusY;

                vert.Add(new float3(x, y, z));
				vert.Add(new float3(x, useAltZPlusY, z + 1));
				vert.Add(new float3(x + 1, useAltXPlusY, z));
				vert.Add(new float3(x + 1, useAltXAndZPlusY, z + 1));

				// Front or top face indices for a quad
				// 0, 2, 1, 0, 3, 2
				indices.Add(vertexIndex);
				indices.Add(vertexIndex + 1);
				indices.Add(vertexIndex + 2);
				indices.Add(vertexIndex + 3);
				indices.Add(vertexIndex + 2);
				indices.Add(vertexIndex + 1);
				indicesIndex += 6;
				vertexIndex += vertexMultiplier;

				float maxY = Mathf.Max(y, useAltXPlusY, useAltZPlusY, useAltXAndZPlusY);
				float minY = Mathf.Min(y, useAltXPlusY, useAltZPlusY, useAltXAndZPlusY);
				float heightDifference = (maxY - minY);

				// UV positioning is from the bottom-left of the texture atlas
				Rect uvRect;
				if (maxY > 200)
				{
					uvRect = GetTextureUVRect(2, 3); // Snow
				}
				else if (maxY < -50)
				{
					uvRect = GetTextureUVRect(3, 0); // Deepslate
				}
				else
				{
					if (heightDifference < 1.5)
					{
						uvRect = GetTextureUVRect(0, 2); // Grass
						
						// See if a tree should spawn on this grass block
						if (UnityEngine.Random.Range(0f, 1f) < treeSpawnPercentage)
						{
							treePositions.Add(new Vector2Int(x, z));
						}
					}
					else if (heightDifference < 4)
					{
						uvRect = GetTextureUVRect(1, 1); // Dirt
					}
					else
					{
						uvRect = GetTextureUVRect(3, 2); // Stone
					}
				}

				// Add uv's
				// Remember to give it all 4 sides of the image coords
				uvs.Add(new Vector2(uvRect.xMin, uvRect.yMin));
				uvs.Add(new Vector2(uvRect.xMin, uvRect.yMax));
				uvs.Add(new Vector2(uvRect.xMax, uvRect.yMin));
				uvs.Add(new Vector2(uvRect.xMax, uvRect.yMax));
			}
		}

		// Set the terrain var's for the mesh
		terrainMesh.vertices = vert.ToArray( );
		terrainMesh.triangles = indices.ToArray( );
		terrainMesh.SetUVs(0, uvs);

		// Reset the mesh
		terrainMesh.RecalculateNormals( );
		terrainMesh.RecalculateBounds( );

		return terrainMesh;
	}

	private void UpdateTerrainMesh ()
	{
		treeObjectPool.DisableAllTrees();
        terrainMeshFilter.sharedMesh = GenerateTerrainMesh();

		for (int i = 0; i < treeObjectPool.MaxTreeObjects; i++)
		{
			if (treePositions.Count == 0)
			{
				break;
			}

			// Get a random tree position and spawn a tree there
			int treeIndex = UnityEngine.Random.Range(0, treePositions.Count);
            Vector2Int treePosition = treePositions[treeIndex];
            treeObjectPool.PlaceTreeAt(new Vector3(treePosition.x, terrainFullHeightMap[(treePosition.x) * (terrainDepth + 1) + (treePosition.y)] - 1.5f, treePosition.y));
			treePositions.RemoveAt(treeIndex);
        }
    }

	private float EvaluateMapFunctionAt (float x, float z, float width, float depth)
	{
		return mapFunctions[equationDropdown.value].Evaluate(x - width / 2f, z - depth / 2f);
	}

	private Rect GetTextureUVRect (int x, int y)
	{
		float uvX = (float) x / TextureAtlas.TextureRows;
		float uvY = (float) y / TextureAtlas.TextureRows;
		float uvSize = 1f / TextureAtlas.TextureRows;
		return new Rect(uvX, uvY, uvSize, uvSize);
	}

	private void SetCurrentMapFunction (int index)
	{
		MapFunction currentFunction = mapFunctions[index];

		aSlider.minValue = currentFunction.ARange.x;
		aSlider.maxValue = currentFunction.ARange.y;
		aSlider.value = currentFunction.ARangeCenter;

		bSlider.minValue = currentFunction.BRange.x;
		bSlider.maxValue = currentFunction.BRange.y;
		bSlider.value = currentFunction.BRangeCenter;
	}
}
