using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Rendering;
using static UnityEngine.Mesh;

namespace ProceduralMesh.Streams
{

    public class SingleStream : IMeshStreams
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Stream0
        {
            public float3 position, normal;
            public float4 tangent;
            public float2 texCoord0;
        }

        NativeArray<Stream0> stream0;
        NativeArray<int3> triangles;

        public void Setup(MeshData data, int vertexCount, int indexCount)
        {
            var descriptor = new NativeArray<VertexAttributeDescriptor>(
                4, Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );
            descriptor[0] = new VertexAttributeDescriptor(dimension: 3);
            descriptor[1] = new VertexAttributeDescriptor(
                VertexAttribute.Normal, dimension: 3
            );
            descriptor[2] = new VertexAttributeDescriptor(
                VertexAttribute.Tangent, dimension: 4
            );
            descriptor[3] = new VertexAttributeDescriptor(
                VertexAttribute.TexCoord0, dimension: 2
            );
            data.SetVertexBufferParams(vertexCount, descriptor);
            descriptor.Dispose();

            data.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

            data.subMeshCount = 1;
            data.SetSubMesh(0, new SubMeshDescriptor(0, indexCount));

            stream0 = data.GetVertexData<Stream0>();
            triangles = data.GetIndexData<int>().Reinterpret<int3>(4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertex(int index, Vertex data)
        {
            stream0[index] = new Stream0
            {
                position = data.position,
                normal = data.normal,
                tangent = data.tangent,
                texCoord0 = data.texCoord0
            };
        }

        public void SetTriangle(int index, int3 triangle)
        {
            triangles[index] = triangle;
        }
    }
}
