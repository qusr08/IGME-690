using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class PerlinNoiseTest : MonoBehaviour
{

    public RenderTexture renderTexture;

    public GameObject plane;

    public int mWidth = 256;
    public int mDepth = 256;
    public int mMaxHeight = 1000;
    public Material mTerrainMaterial;
    public Material GrassMaterial;
    private GameObject realTerrain;
    private NoiseAlgorithm terrainNoise;

    // perlin noise var's to set up
    public int seed = 523;
    public float Frequency = 1.0f;
    public float Amplitude = 0.5f;
    public int Octaves = 8;
    public float Lacunarity = 2.0f;
    public float Gain = 0.5f;
    public float Scale = 0.01f;
    public float NormalizeBias = 1.0f;
    public int cx = 0;
    public int cy = 0;

    // compute shader info and buffers
    public ComputeShader computeShader;
    private ComputeBuffer PermutationArray;
    private ComputeBuffer RandomArray;
    private ComputeBuffer PerlinNoiseArray;
    private float[] perlinNoiseArray;
    private bool shaderIsDone = false;
    float xOffset;
    float yOffset;
    GraphicsFence graphicsFence;

    // Start is called before the first frame update
    void Start()
    {
        perlinNoiseArray = new float[mWidth * mDepth];

        // create a height map using perlin noise and fractal brownian motion
        UnityEngine.Random.InitState(seed);
        xOffset = UnityEngine.Random.Range(0.0f, 0.9999f);
        yOffset = UnityEngine.Random.Range(0.0f, 0.9999f);
        terrainNoise = new NoiseAlgorithm();
        terrainNoise.InitializeNoise(mWidth + 1, mDepth + 1, seed);
        
        // set up a gazillion var's for perlin on the gpu
        computeShader.SetFloat("frequency", Frequency);
        computeShader.SetFloat("amplitude", Amplitude);
        computeShader.SetInt("octaves", Octaves);
        computeShader.SetFloat("lacunarity", Lacunarity);
        computeShader.SetFloat("gain", Gain);
        computeShader.SetFloat("scale", Scale);
        computeShader.SetFloat("xOffset", xOffset);
        computeShader.SetFloat("yOffset", yOffset);
        computeShader.SetFloat("zOffset", 0.0f);
        computeShader.SetInt("cx", cx);
        computeShader.SetInt("cy", cy);
        computeShader.SetInt("permSize", NoiseAlgorithm.PERM_SIZE);
        computeShader.SetInt("dimensions", 3);
        computeShader.SetInt("maxWidth", mWidth);
        PermutationArray = new ComputeBuffer(terrainNoise.permutation.Length, Marshal.SizeOf(typeof(System.Single)), ComputeBufferType.Default);
        PermutationArray.SetData(terrainNoise.permutation);
        computeShader.SetBuffer(0, "PermutationArray", PermutationArray);
        RandomArray = new ComputeBuffer(terrainNoise.randomArray.Length, Marshal.SizeOf(typeof(System.Single)), ComputeBufferType.Default);
        RandomArray.SetData(terrainNoise.randomArray);
        computeShader.SetBuffer(0, "RandomArray", RandomArray);
        PerlinNoiseArray = new ComputeBuffer(perlinNoiseArray.Length, Marshal.SizeOf(typeof(System.Single)), ComputeBufferType.Default);
        PerlinNoiseArray.SetData(perlinNoiseArray);
        computeShader.SetBuffer(0, "PerlinNoise", PerlinNoiseArray);

        // dispatch!
        computeShader.Dispatch(0, mWidth / 8, mDepth / 8, 1);
        graphicsFence = Graphics.CreateGraphicsFence(GraphicsFenceType.CPUSynchronisation, SynchronisationStageFlags.ComputeProcessing);
    }

    // Update is called once per frame
    void Update()
    {
        if (!shaderIsDone && graphicsFence.passed)
        {
            Debug.Log("perlin is done!");
            PerlinNoiseArray.GetData(perlinNoiseArray);
            shaderIsDone = true;
            MeshRenderer meshRenderer = plane.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = plane.GetComponent<MeshFilter>();
            meshRenderer.material = mTerrainMaterial;
            meshFilter.mesh = GenerateTerrainMesh(perlinNoiseArray);
        }
       
    }

    // create a new mesh with
    // perlin noise 
    // without any other features
    // makes a quad and connects it with the next quad
    // uses whatever texture the material is given
    public Mesh GenerateTerrainMesh(float[] perlinNoise)
    {
        int width = mWidth, depth = mDepth;
        int height = mMaxHeight;
        int indicesIndex = 0;
        int vertexIndex = 0;
        int vertexMultiplier = 4; // create quads to fit uv's to so we can use more than one uv (4 vertices to a quad)
        float randomSeed = seed;

        Mesh terrainMesh = new Mesh();
        List<Vector3> vert = new List<Vector3>(width * depth * vertexMultiplier);
        List<int> indices = new List<int>(width * depth * 6);
        List<Vector2> uvs = new List<Vector2>(width * depth);
        for (int x = 0; x < width - 1; x++)
        {
            for (int z = 0; z < depth - 1; z++)
            {
                float y = 0;
                float realX = x + seed + 0.5f;
                float realZ = z + seed + 0.5f;
                // create an array of quads at given noise heights
                if (x < width - 1 && z < depth - 1)
                {
                    y = height * perlinNoise[x * mWidth + z];
                    float useAltXPlusY = perlinNoise[(x + 1) * mWidth + z] * height;
                    float useAltZPlusY = perlinNoise[x * mWidth + (z + 1)] * height;
                    float useAltXAndZPlusY = perlinNoise[(x + 1) * mWidth + (z + 1)] * height;
                    vert.Add(new float3(x, y, z));
                    vert.Add(new float3(x, useAltZPlusY, z + 1)); // 
                    vert.Add(new float3(x + 1, useAltXPlusY, z)); // 
                    vert.Add(new float3(x + 1, useAltXAndZPlusY, z + 1)); // 

                    // add uv's
                    // remember to give it all 4 sides of the image coords
                    uvs.Add(new Vector2(0.0f, 0.0f));
                    uvs.Add(new Vector2(0.0f, 1.0f));
                    uvs.Add(new Vector2(1.0f, 1.0f));
                    uvs.Add(new Vector2(1.0f, 0.0f));

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
