using System.Collections.Generic;
using UnityEngine;
using WorldNavigation;

namespace WorldNavigation
{
    public class MeshTriangleNeighbors
    {
        private static Dictionary<Edge, List<Triangle>> edges;

        public static List<Triangle> CalculateTriangleNeighbors(Mesh mesh)
        {
            List<Triangle> triangles = new List<Triangle>();
            edges = new Dictionary<Edge, List<Triangle>>();

            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                int index1 = mesh.triangles[i];
                int index2 = mesh.triangles[i + 1];
                int index3 = mesh.triangles[i + 2];

                Triangle triangle = new Triangle(index1, index2, index3);
                triangles.Add(triangle);

                TryAddConnection(new Edge(index1, index2), triangle);
                TryAddConnection(new Edge(index2, index3), triangle);
                TryAddConnection(new Edge(index3, index1), triangle);
            }

            return triangles;
        }

        private static void TryAddConnection(Edge edge, Triangle triangle)
        {
            if (edges.TryGetValue(edge, out List<Triangle> connected))
            {
                connected[0].Neighbors.Add(triangle);
                triangle.Neighbors.Add(connected[0]);
                connected.Add(triangle);
            }
            else
            {
                edges.Add(edge, new List<Triangle>() { triangle });
            }
        }
    }
}