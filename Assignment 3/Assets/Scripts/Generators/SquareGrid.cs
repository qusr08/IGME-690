using ProceduralMesh.Generators;
using ProceduralMesh.Streams;

// https://catlikecoding.com/unity/tutorials/procedural-meshes/square-grid/

public class SquareGrid : IMeshGenerator
{
    public int VertexCount => 0;

    public int IndexCount => 0;

    public int JobLength => 0;

    public void Execute<S>(int i, S streams) where S : struct, IMeshStreams
    {

    }
}
