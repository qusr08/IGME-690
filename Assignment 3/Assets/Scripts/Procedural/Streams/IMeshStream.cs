using Unity.Mathematics;
using UnityEngine;

namespace Procedural.Streams
{
    public interface IMeshStream
    {
        void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount);
        void SetVertex(int index, Vertex vertex);
        void SetTriangle(int index, int3 triangle);
    }
}