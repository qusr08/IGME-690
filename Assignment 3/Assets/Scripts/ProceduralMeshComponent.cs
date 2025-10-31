using ProceduralMesh;
using ProceduralMesh.Streams;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMeshComponent : MonoBehaviour
{
	private static MeshJobScheduleDelegate[] _jobs =
	{
		MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
		MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel
	};

	[SerializeField, Range(1, 50)] private int _resolution = 1;
	[SerializeField] private MeshType _meshType;

	private Mesh _mesh;

	private void Awake()
	{
		_mesh = new Mesh
		{
			name = "Procedural Mesh"
		};

		GetComponent<MeshFilter>().mesh = _mesh;
	}

	private void OnValidate()
	{
		enabled = true;
	}

	private void Update()
	{
		GenerateMesh();
		enabled = false;
	}

	private void GenerateMesh()
	{
		Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = meshDataArray[0];

		_jobs[(int)_meshType](_mesh, meshData, _resolution, default).Complete();

		Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _mesh);
	}
}
