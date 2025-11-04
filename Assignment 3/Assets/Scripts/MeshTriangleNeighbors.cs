using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

// https://discussions.unity.com/t/how-to-find-connected-mesh-triangles/221772

public class MeshTriangleNeighbors
{
    private struct Edge
    {
        public float3 V1;
        public float3 V2;

        public Edge(float3 v1, float3 v2)
        {
            // ensure the same order to guarantee equality
            if (v1.GetHashCode() > v2.GetHashCode())
            {
                V1 = v1; V2 = v2;
            }
            else
            {
                V1 = v2; V2 = v1;
            }
        }
    }

    private struct TrianglePair
    {
        public int T1;
        public int T2;

        public TrianglePair(int defaultValue = -1)
        {
            T1 = defaultValue;
            T2 = defaultValue;
        }

        public bool Add(int triangleIndex)
        {
            if (T1 == -1)
            {
                T1 = triangleIndex;
            }
            else if (T2 == -1)
            {
                T2 = triangleIndex;
            }
            else
            {
                return false;
            }

            return true;
        }

    }

    public class Neighbors
    {
        public int t1 = -1;
        public int t2 = -1;
        public int t3 = -1;
    }

    private static Dictionary<Edge, TrianglePair> CreateEdgeList(List<float3> triangles)
    {
        var result = new Dictionary<Edge, TrianglePair>();
        int count = triangles.Count / 3;
        for (int i = 0; i < count; i++)
        {
            float3 v1 = triangles[i * 3];
            float3 v2 = triangles[i * 3 + 1];
            float3 v3 = triangles[i * 3 + 2];
            TrianglePair p;
            Edge e;

            e = new Edge(v1, v2);
            if (!result.TryGetValue(e, out p))
            {
                p = new TrianglePair();
                result.Add(e, p);
            }
            p.Add(i);

            e = new Edge(v2, v3);
            if (!result.TryGetValue(e, out p))
            {
                p = new TrianglePair();
                result.Add(e, p);
            }
            p.Add(i);

            e = new Edge(v3, v1);
            if (!result.TryGetValue(e, out p))
            {
                p = new TrianglePair();
                result.Add(e, p);
            }
            p.Add(i);
        }

        return result;
    }

    private static List<int> GetTriangleNeighbors(Dictionary<Edge, TrianglePair> edgeDictionary, List<float3> triangles)
    {
        var result = new List<int>();
        int count = triangles.Count / 3;
        for (int i = 0; i < count; i++)
        {
            float3 v1 = triangles[i * 3];
            float3 v2 = triangles[i * 3 + 1];
            float3 v3 = triangles[i * 3 + 2];
            TrianglePair p;

            if (edgeDictionary.TryGetValue(new Edge(v1, v2), out p))
            {
                if (p.T1 == i)
                {
                    result.Add(p.T2);
                }
                else
                {
                    result.Add(p.T1);
                }
            }
            else
            {
                result.Add(-1);
            }

            if (edgeDictionary.TryGetValue(new Edge(v2, v3), out p))
            {
                if (p.T1 == i)
                {
                    result.Add(p.T2);
                }
                else
                {
                    result.Add(p.T1);
                }
            }
            else
            {
                result.Add(-1);
            }

            if (edgeDictionary.TryGetValue(new Edge(v3, v1), out p))
            {
                if (p.T1 == i)
                {
                    result.Add(p.T2);
                }
                else
                {
                    result.Add(p.T1);
                }
            }
            else
            {
                result.Add(-1);
            }
        }

        return result;
    }

    public static List<int> GetNeighbors(Mesh mesh)
    {
        int[] tris = mesh.triangles;
        List<float3> triangles = new List<float3>(tris.Length);
        foreach (int t in tris)
        {
            triangles.Add(mesh.vertices[t]);
        }

        return GetTriangleNeighbors(CreateEdgeList(triangles), triangles);
    }
}