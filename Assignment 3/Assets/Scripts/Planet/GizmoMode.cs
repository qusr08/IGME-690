using System.Collections.Generic;

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
        }

        public override string ToString()
        {
            return $"({V1}, {V2})";
        }
    }

    public struct TriangleNode
    {
        public int V1, V2, V3;
        public List<TriangleNode> Connections;

        public TriangleNode(int v1, int v2, int v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            Connections = new List<TriangleNode>();
        }

        public override string ToString()
        {
            return $"({V1}, {V2}, {V3})";
        }
    }
}