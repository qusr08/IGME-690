using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;
using TMPro;
using UnityEngine.UI;

public class TerrainGeneration : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI aText;
    [SerializeField] private Slider aSlider;
    [SerializeField] private TextMeshProUGUI bText;
    [SerializeField] private Slider bSlider;
    [SerializeField] private TMP_Dropdown equationDropdown;
    [Space]
    public int Width;
    public int Depth;
    public int NoiseHeightMultiplier;
    public Material TerrainMaterial;
    public float Frequency = 1.0f;
    public float Amplitude = 0.5f;
    public float Lacunarity = 2.0f;
    public float Gain = 0.5f;
    public int Octaves = 8;
    public float Scale = 0.01f;
    public float NormalizeBias = 1.0f;

    private GameObject terrainObject;
    private MeshFilter terrainMeshFilter;
    private MeshRenderer terrainMeshRenderer;
    private NoiseAlgorithm terrainNoise;
    private NativeArray<float> terrainHeightMap;
    private List<MapFunction> mapFunctions;

    public float A
    {
        get => _a;
        set
        {
            _a = value;
            Vector2 aRange = mapFunctions[equationDropdown.value].ARange;
            aText.text = $"<b>A</b> = {_a:0.0000} <i>[{aRange.x} to {aRange.y}]</i>";
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
            bText.text = $"<b>B</b> = {_b:0.0000} <i>[{bRange.x} to {bRange.y}]</i>";
            UpdateTerrainMesh();
        }
    }
    private float _b;

    // code to get rid of fog from: https://forum.unity.com/threads/how-do-i-turn-off-fog-on-a-specific-camera-using-urp.1373826/
    // Unity calls this method automatically when it enables this component
    private void OnEnable()
    {
        // Add WriteLogMessage as a delegate of the RenderPipelineManager.beginCameraRendering event
        RenderPipelineManager.beginCameraRendering += BeginRender;
        RenderPipelineManager.endCameraRendering += EndRender;
    }

    // Unity calls this method automatically when it disables this component
    private void OnDisable()
    {
        // Remove WriteLogMessage as a delegate of the  RenderPipelineManager.beginCameraRendering event
        RenderPipelineManager.beginCameraRendering -= BeginRender;
        RenderPipelineManager.endCameraRendering -= EndRender;
    }

    // When this method is a delegate of RenderPipeline.beginCameraRendering event, Unity calls this method every time it raises the beginCameraRendering event
    void BeginRender(ScriptableRenderContext context, Camera camera)
    {
        if (camera.name == "Main Camera No Fog")
        {
            //Debug.Log("Turn fog off");
            RenderSettings.fog = false;
        }

    }

    void EndRender(ScriptableRenderContext context, Camera camera)
    {
        if (camera.name == "Main Camera No Fog")
        {
            //Debug.Log("Turn fog on");
            RenderSettings.fog = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // create a height map using perlin noise and fractal brownian motion
        terrainNoise = new NoiseAlgorithm();
        terrainNoise.InitializeNoise(Width + 1, Depth + 1, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        terrainNoise.InitializePerlinNoise(Frequency, Amplitude, Octaves, Lacunarity, Gain, Scale, NormalizeBias);
        terrainHeightMap = new NativeArray<float>((Width + 1) * (Depth + 1), Allocator.Persistent);
        terrainNoise.setNoise(terrainHeightMap, 0, 0);
        NoiseAlgorithm.OnExit();

        // create the mesh and set it to the terrain variable
        terrainObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        terrainObject.transform.position = new Vector3(0, 0, 0);
        terrainMeshRenderer = terrainObject.GetComponent<MeshRenderer>();
        terrainMeshFilter = terrainObject.GetComponent<MeshFilter>();

        mapFunctions = new List<MapFunction>()
        {
            new MapFunction("x<i>cos</i>(<b>A</b>x)<i>sin</i>(<b>B</b>z)",
                new Vector2(0f, 0.2f), new Vector2(0f, 0.2f),
                (float x, float z) => { return x * Mathf.Cos(A * x) * Mathf.Sin(B * z); }),
            new MapFunction("<b>A</b><i>sin</i>(<b>B</b><i>sqrt</i>(x^2 + z^2))",
                new Vector2(0f, 5f), new Vector2(0f, 1f),
                (float x, float z) => { return A * Mathf.Sin(B * Mathf.Sqrt((x * x) + (z * z))); }),
            new MapFunction("-<b>A</b>(<b>B</b> - 0.2<i>sqrt</i>(x^2 + z^2))^2",
                new Vector2(-1f, 5f), new Vector2(0f, 5f),
                (float x, float z) => { return -A * Mathf.Pow(B - (0.2f * Mathf.Sqrt((x * x) + (z * z))), 2); }),
            new MapFunction("<b>A</b>z<i>sin</i>(<b>B</b>xy)",
                new Vector2(0f, 1f), new Vector2(0f, 0.05f),
                (float x, float z) => { return A * z * Mathf.Sin(B * x * z); }),
            new MapFunction("<b>A</b>cos(<b>B</b>(|x| + |z|))",
                new Vector2(0f, 5f), new Vector2(0f, 0.5f),
                (float x, float z) => { return A * Mathf.Cos(B * (Mathf.Abs(x) + Mathf.Abs(z))); }),
        };

        // z * Mathf.Sin(x * x);
        // 0.001f * ((A * x * z * z * z) - (B * z * x * x * x));

        List<TMP_Dropdown.OptionData> optionsList = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < mapFunctions.Count; i++)
        {
            optionsList.Add(new TMP_Dropdown.OptionData(mapFunctions[i].FunctionText));
        }
        equationDropdown.AddOptions(optionsList);

        // Add UI value change listeners
        aSlider.onValueChanged.AddListener((v) => { A = v; });
        bSlider.onValueChanged.AddListener((v) => { B = v; });
        equationDropdown.onValueChanged.AddListener(SetCurrentMapFunction);

        SetCurrentMapFunction(0);
    }

    public void UpdateTerrainMesh()
    {
        // Update the shape of the terrain mesh
        terrainMeshFilter.mesh = GenerateTerrainMesh();

        // Update the material of the terrain mesh
        terrainMeshRenderer.material = TerrainMaterial;
    }

    // create a new mesh with
    // perlin noise
    // makes a quad and connects it with the next quad
    // uses whatever texture the material is given
    public Mesh GenerateTerrainMesh()
    {
        int width = Width + 1, depth = Depth + 1;
        int indicesIndex = 0;
        int vertexIndex = 0;
        int vertexMultiplier = 4; // create quads to fit uv's to so we can use more than one uv (4 vertices to a quad)

        Mesh terrainMesh = new Mesh();
        List<Vector3> vert = new List<Vector3>(width * depth * vertexMultiplier);
        List<int> indices = new List<int>(width * depth * 6);
        List<Vector2> uvs = new List<Vector2>(width * depth);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (x >= width - 1 || z >= depth - 1)
                {
                    continue;
                }

                // note: since perlin goes up to 1.0 multiplying by a height will tend to set
                // the average around maxheight/2. We remove most of that extra by subtracting maxheight/2
                // so our ground isn't always way up in the air
                float y = EvaluateMapFunctionAt(x, z, width, depth) + terrainHeightMap[(x) * (depth) + (z)] * NoiseHeightMultiplier;
                float useAltXPlusY = EvaluateMapFunctionAt(x + 1, z, width, depth) + terrainHeightMap[(x + 1) * (depth) + (z)] * NoiseHeightMultiplier;
                float useAltZPlusY = EvaluateMapFunctionAt(x, z + 1, width, depth) + terrainHeightMap[(x) * (depth) + (z + 1)] * NoiseHeightMultiplier;
                float useAltXAndZPlusY = EvaluateMapFunctionAt(x + 1, z + 1, width, depth) + terrainHeightMap[(x + 1) * (depth) + (z + 1)] * NoiseHeightMultiplier;

                vert.Add(new float3(x, y, z));
                vert.Add(new float3(x, useAltZPlusY, z + 1));
                vert.Add(new float3(x + 1, useAltXPlusY, z));
                vert.Add(new float3(x + 1, useAltXAndZPlusY, z + 1));

                // add uv's
                // remember to give it all 4 sides of the image coords
                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 1.0f));
                uvs.Add(new Vector2(1.0f, 0.0f));
                uvs.Add(new Vector2(1.0f, 1.0f));

                // front or top face indices for a quad
                //0,2,1,0,3,2
                indices.Add(vertexIndex);
                indices.Add(vertexIndex + 1);
                indices.Add(vertexIndex + 2);
                indices.Add(vertexIndex + 3);
                indices.Add(vertexIndex + 2);
                indices.Add(vertexIndex + 1);
                indicesIndex += 6;
                vertexIndex += vertexMultiplier;
            }
        }

        // set the terrain var's for the mesh
        terrainMesh.vertices = vert.ToArray();
        terrainMesh.triangles = indices.ToArray();
        terrainMesh.SetUVs(0, uvs);

        // reset the mesh
        terrainMesh.RecalculateNormals();
        terrainMesh.RecalculateBounds();

        return terrainMesh;
    }

    private float EvaluateMapFunctionAt(float x, float z, float width, float depth)
    {
        return mapFunctions[equationDropdown.value].Evaluate(x - width / 2f, z - depth / 2f);
    }

    private void SetCurrentMapFunction(int index)
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
