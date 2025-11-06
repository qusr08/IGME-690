
using System;
using Unity.Collections;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Planet
{
    [System.Flags]
    public enum GizmoMode
    {
        Nothing = 0, Vertices = 1, Normals = 0b10, Tangents = 0b100, Triangles = 0b1000
    }

    [System.Flags]
    public enum MeshOptimizationMode
    {
        Nothing = 0, ReorderIndices = 1, ReorderVertices = 0b10
    }

    public enum MeshType
    {
        SquareGrid, SharedSquareGrid, SharedTriangleGrid, PointyHexagonGrid, FlatHexagonGrid, UVSphere, CubeSphere, SharedCubeSphere, Octasphere, GeoOctasphere, Icosphere, GeoIcosphere
    }

    public enum MaterialMode
    {
        VertexColor, Flat, Ripple, LatLonMap, LatLonCubeMap
    }

    public struct EdgeNode
    {
        public int V1, V2;
        public TriangleNode? N1, N2;
        private int _neighborCount;

        public EdgeNode(int v1, int v2)
        {
            // Make sure the order of the indices is always the same
            if (v1.GetHashCode() > v2.GetHashCode())
            {
                V1 = v1;
                V2 = v2;
            }
            else
            {
                V1 = v2;
                V2 = v1;
            }

            N1 = null;
            N2 = null;
            _neighborCount = 0;
        }

        public void AddNeighbor(TriangleNode neighbor)
        {
            if (N1 == neighbor || N2 == neighbor)
            {
                return;
            }

            if (N1 == null)
            {
                N1 = neighbor;
                _neighborCount++;
            }
            else if (N2 == null)
            {
                N2 = neighbor;
                neighbor.AddConnection(((TriangleNode)N1).Index);
                ((TriangleNode)N1).AddConnection(neighbor.Index);
                _neighborCount++;
            }
        }

        public override string ToString() => $"({V1}, {V2})";
    }

    public struct TriangleNode
    {
        public int V1, V2, V3;
        public int Index;
        public int ConnectionIndex1, ConnectionIndex2, ConnectionIndex3;
        private int _connectionCount;

        public TriangleNode(int v1, int v2, int v3, int index)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            Index = index;
            ConnectionIndex1 = -1;
            ConnectionIndex2 = -1;
            ConnectionIndex3 = -1;
            _connectionCount = 0;
        }

        public void AddConnection(int index)
        {
            if (ConnectionIndex1 == index || ConnectionIndex2 == index || ConnectionIndex3 == index || index == Index)
            {
                return;
            }

            if (ConnectionIndex1 == -1)
            {
                ConnectionIndex1 = index;
                _connectionCount++;
            }
            else if (ConnectionIndex2 == -1)
            {
                ConnectionIndex2 = index;
                _connectionCount++;
            }
            else if (ConnectionIndex3 == -1)
            {
                ConnectionIndex3 = index;
                _connectionCount++;
            }
        }

        public int SelectRandomConnection()
        {
            return Random.Range(0, _connectionCount) switch
            {
                0 => ConnectionIndex1,
                1 => ConnectionIndex2,
                _ => ConnectionIndex3
            };
        }

        public static bool operator ==(TriangleNode left, TriangleNode right) => left.Equals(right);
        public static bool operator !=(TriangleNode left, TriangleNode right) => !(left == right);

        public override string ToString() => $"({V1}, {V2}, {V3})";
        public override bool Equals(object obj) => obj is TriangleNode node && V1 == node.V1 && V2 == node.V2 && V3 == node.V3;
        public override int GetHashCode() => HashCode.Combine(V1, V2, V3);
    }
}