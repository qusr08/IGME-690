using ProceduralMesh;
using ProceduralMesh.Generators;
using ProceduralMesh.Streams;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

// https://catlikecoding.com/unity/tutorials/procedural-meshes/square-grid/

public struct SquareGrid : IMeshGenerator
{
	public int Resolution { get; set; }
	public int VertexCount => 4 * Resolution * Resolution;
	public int IndexCount => 6 * Resolution * Resolution;
	public int JobLength => Resolution;
	public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1f, 0f, 1f));

	public void Execute<S>(int z, S streams) where S : struct, IMeshStream
	{
		int vi = 4 * Resolution * z;
		int ti = 2 * Resolution * z;

		for (int x = 0; x < Resolution; x++, vi += 4, ti += 2)
		{
			float2 xCoordinates = float2(x, x + 1f) / Resolution - 0.5f;
			float2 zCoordinates = float2(z, z + 1f) / Resolution - 0.5f;

			Vertex vertex = new Vertex();
			vertex.normal.y = 1f;
			vertex.tangent.xw = float2(1f, -1f);
			vertex.position.x = xCoordinates.x;
			vertex.position.z = zCoordinates.x;
			streams.SetVertex(vi + 0, vertex);

			vertex.position.x = xCoordinates.y;
			vertex.texCoord0 = float2(1f, 0f);
			streams.SetVertex(vi + 1, vertex);

			vertex.position.x = xCoordinates.x;
			vertex.position.z = zCoordinates.y;
			vertex.texCoord0 = float2(0f, 1f);
			streams.SetVertex(vi + 2, vertex);

			vertex.position.x = xCoordinates.y;
			vertex.texCoord0 = 1f;
			streams.SetVertex(vi + 3, vertex);

			streams.SetTriangle(ti + 0, vi + int3(0, 2, 1));
			streams.SetTriangle(ti + 1, vi + int3(1, 2, 3));
		}
	}
}
