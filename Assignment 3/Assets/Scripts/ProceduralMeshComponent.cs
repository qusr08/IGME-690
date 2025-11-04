using ProceduralMesh;
using ProceduralMesh.Streams;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMeshComponent : MonoBehaviour
{
	private static readonly MeshJobScheduleDelegate[] _jobs =
	{
		MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
		MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel,
		MeshJob<SharedTriangleGrid, SingleStream>.ScheduleParallel,
		MeshJob<PointyHexagonGrid, SingleStream>.ScheduleParallel,
		MeshJob<FlatHexagonGrid, SingleStream>.ScheduleParallel,
		MeshJob<UVSphere, SingleStream>.ScheduleParallel,
		MeshJob<CubeSphere, SingleStream>.ScheduleParallel,
		MeshJob<SharedCubeSphere, PositionStream>.ScheduleParallel,
		MeshJob<Octasphere, SingleStream>.ScheduleParallel
	};

	[SerializeField] private Material[] _materials;
	[Space]
	[SerializeField, Range(1, 50)] private int _resolution = 1;
	[SerializeField] private MeshType _meshType;
	[SerializeField] private GizmoMode _gizmoMode;
	[SerializeField] private MaterialMode _materialMode;
	[SerializeField] private MeshOptimizationMode _meshOptimizationMode;

	private Mesh _mesh;
	[System.NonSerialized] private Vector3[] _vertices;
	[System.NonSerialized] private Vector3[] _normals;
	[System.NonSerialized] private Vector4[] _tangents;
	[System.NonSerialized] private int[] _triangles;

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

		_vertices = null;
		_normals = null;
		_tangents = null;
		_triangles = null;

		GetComponent<MeshRenderer>().material = _materials[(int)_materialMode];
	}

	private void OnDrawGizmos()
	{
		if (_gizmoMode == GizmoMode.Nothing || _mesh == null)
		{
			return;
		}

		bool drawVertices = (_gizmoMode & GizmoMode.Vertices) != 0;
		bool drawNormals = (_gizmoMode & GizmoMode.Normals) != 0;
		bool drawTangents = (_gizmoMode & GizmoMode.Tangents) != 0;
		bool drawTriangles = (_gizmoMode & GizmoMode.Triangles) != 0;

		if (_vertices == null)
		{
			_vertices = _mesh.vertices;
		}
		if (drawNormals && _normals == null)
		{
			drawNormals = _mesh.HasVertexAttribute(VertexAttribute.Normal);
			if (drawNormals)
			{
				_normals = _mesh.normals;
			}
		}
		if (drawTangents && _tangents == null)
		{
			drawTangents = _mesh.HasVertexAttribute(VertexAttribute.Tangent);
			if (drawTangents)
			{
				_tangents = _mesh.tangents;
			}
		}
		if (drawTriangles && _tangents == null)
		{
			_triangles = _mesh.triangles;
		}

		Transform t = transform;
		for (int i = 0; i < _vertices.Length; i++)
		{
			Vector3 position = t.TransformPoint(_vertices[i]);
			if (drawVertices)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawSphere(position, 0.02f);
			}
			if (drawNormals)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawRay(position, t.TransformDirection(_normals[i]) * 0.2f);
			}
			if (drawTangents)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawRay(position, t.TransformDirection(_tangents[i]) * 0.2f);
			}
		}

		if (drawTriangles)
		{
			float colorStep = 1f / (_triangles.Length - 3);
			for (int i = 0; i < _triangles.Length; i += 3)
			{
				float c = i * colorStep;
				Gizmos.color = new Color(c, 0f, c);
				Gizmos.DrawSphere(
					t.TransformPoint((_vertices[_triangles[i]] + _vertices[_triangles[i + 1]] + _vertices[_triangles[i + 2]]) * (1f / 3f)),
					0.02f
				);
			}
		}
	}

	private void GenerateMesh()
	{
		Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = meshDataArray[0];

		_jobs[(int)_meshType](_mesh, meshData, _resolution, default).Complete();

		Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _mesh);

		if (_meshOptimizationMode == MeshOptimizationMode.ReorderIndices)
		{
			_mesh.OptimizeIndexBuffers();
		}
		else if (_meshOptimizationMode == MeshOptimizationMode.ReorderVertices)
		{
			_mesh.OptimizeReorderVertexBuffer();
		}
		else if (_meshOptimizationMode != MeshOptimizationMode.Nothing)
		{
			_mesh.Optimize();
		}
	}
}
