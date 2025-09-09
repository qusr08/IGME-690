using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;

public class TerrainGeneration : MonoBehaviour
{
    public int RandomSeed;
    [Min(0)] public int Width;
    [Min(0)] public int Depth;
    [Min(0)] public int MaxHeight;
    [Range(0f, 1f)] public float Step;
    [Min(0)] public int Iterations;
    [Min(0)] public float IterationStepGain;
    public Material TerrainMaterial;

    private GameObject mRealTerrain;

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
        mRealTerrain = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mRealTerrain.transform.position = new Vector3(0, 0, 0);
        MeshRenderer meshRenderer = mRealTerrain.GetComponent<MeshRenderer>();
        MeshFilter meshFilter = mRealTerrain.GetComponent<MeshFilter>();
        meshRenderer.material = TerrainMaterial;
        meshFilter.mesh = GenerateTerrainMesh();

    }

    private void Update()
    {

    }

    // create a new mesh with
    // perlin noise done blankly from Mathf.PerlinNoise in Unity
    // without any other features
    // makes a quad and connects it with the next quad
    // uses whatever texture the material is given
    public Mesh GenerateTerrainMesh()
    {
        int width = Width + 1, depth = Depth + 1;
        int height = MaxHeight;
        int indicesIndex = 0;
        int vertexIndex = 0;
        int vertexMultiplier = 4; // create quads to fit uv's to so we can use more than one uv (4 vertices to a quad)
        float randomSeed = RandomSeed;

        Mesh terrainMesh = new Mesh();
        List<Vector3> vert = new List<Vector3>(width * depth * vertexMultiplier);
        List<int> indices = new List<int>(width * depth * 6);
        List<Vector2> uvs = new List<Vector2>(width * depth);

        float[] heights = new float[width * depth];
        float modStep = Step;

        // Generate the terrain over multiple iterations of perlin noise
        for (int i = 0; i < Iterations; i++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    float realX = (x * modStep) + RandomSeed + 0.5f;
                    float realZ = (z * modStep) + RandomSeed + 0.5f;

                    if (x < width - 1 && z < depth - 1)
                    {
                        heights[x + (width * z)] += Mathf.PerlinNoise(realX, realZ) * (height / (i + 1));
                    }
                }
            }

            modStep += IterationStepGain;
        }

        // Create all of the vertices, uv, and tris for the mesh
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (x < width - 1 && z < depth - 1)
                {
                    vert.Add(new float3(x, heights[x + (width * z)], z));
                    vert.Add(new float3(x, heights[x + (width * (z + 1))], z + 1)); // 
                    vert.Add(new float3(x + 1, heights[(x + 1) + (width * z)], z)); // 
                    vert.Add(new float3(x + 1, heights[(x + 1) + (width * (z + 1))], z + 1)); // 

                    // add uv's
                    // remember to give it all 4 sides of the image coords
                    uvs.Add(new Vector2(0.0f, 0.0f));
                    uvs.Add(new Vector2(0.0f, 1.0f));
                    uvs.Add(new Vector2(1.0f, 1.0f));
                    uvs.Add(new Vector2(1.0f, 0.0f));

                    // front or top face indices for a quad
                    // NOTE: depends on order of vertex adds
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

}
