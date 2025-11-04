using ProceduralMesh.Streams;
using UnityEngine;

namespace ProceduralMesh.Generators
{
    public interface IMeshGenerator
	{
		int Resolution { get; set; }
		int VertexCount { get; }
        int IndexCount { get; }
        int JobLength { get; }
        Bounds Bounds { get; }

        void Execute<S>(int i, S streams) where S : struct, IMeshStream;
    }
}
