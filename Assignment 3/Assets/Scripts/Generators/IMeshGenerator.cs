using ProceduralMesh.Streams;

namespace ProceduralMesh.Generators
{
    public interface IMeshGenerator
    {
        int VertexCount { get; }
        int IndexCount { get; }
        int JobLength { get; }

        void Execute<S>(int i, S streams) where S : struct, IMeshStreams;
    }
}
