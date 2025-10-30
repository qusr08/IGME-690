using Unity.Mathematics;
using UnityEngine;

namespace ProceduralMesh.Streams
{
    public interface IMeshStreams
    {
        void Setup(Mesh.MeshData data, int vertexCount, int indexCount);
        void SetVertex(int index, Vertex data);
        void SetTriangle(int index, int3 triangle);
    }
}