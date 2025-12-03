using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class IcosphereMesh : MonoBehaviour
{
	[SerializeField] private MeshFilter meshFilter;
	[Space]
	[SerializeField, Range(0, 5)] private int resolution = 1;

	private Mesh _mesh;
	private IcosphereGenerator _generator;

	private int _lastResolution = -1;

	private void Awake()
	{
		_generator = new IcosphereGenerator();
	}

	private void Update()
	{
		if (_lastResolution != resolution)
		{
			_lastResolution = resolution;

			GenerateMesh();
		}
	}

	public void GenerateMesh()
	{
		// Create the triangles and vertices for the icosphere mesh
		_generator.CreateIcosahedron();
		_generator.Subdivide(resolution);

		_mesh = new Mesh();

		// Create lists for storing mesh data
		int vertexCount = _generator.Triangles.Count * 3;
		int[] indices = new int[vertexCount];
		Vector3[] vertices = new Vector3[vertexCount];
		Vector3[] normals = new Vector3[vertexCount];

		// Get the vertices and indices for the mesh based on the generator
		for (int i = 0; i < _generator.Triangles.Count; i++)
		{
			Triangle triangle = _generator.Triangles[i];

			indices[i * 3 + 0] = i * 3 + 0;
			indices[i * 3 + 1] = i * 3 + 1;
			indices[i * 3 + 2] = i * 3 + 2;

			vertices[i * 3 + 0] = _generator.Vertices[triangle.A];
			vertices[i * 3 + 1] = _generator.Vertices[triangle.B];
			vertices[i * 3 + 2] = _generator.Vertices[triangle.C];

			normals[i * 3 + 0] = _generator.Vertices[triangle.A];
			normals[i * 3 + 1] = _generator.Vertices[triangle.B];
			normals[i * 3 + 2] = _generator.Vertices[triangle.C];
		}

		// Save the mesh data so it can be displayed on the mesh
		_mesh.vertices = vertices;
		_mesh.normals = normals;
		_mesh.SetTriangles(indices, 0);
		meshFilter.sharedMesh = _mesh;
	}
}
