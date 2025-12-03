using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class IcosphereMesh : MonoBehaviour
{
	[SerializeField] private Material material;
	[Space]
	[SerializeField, Range(0, 5)] private int resolution = 1;

	private Mesh _mesh;
	private MeshFilter _meshFilter;
	private MeshRenderer _meshRenderer;
	private Material _materialInstance;
	private IcosphereGenerator _generator;

	private int _lastResolution = -1;

	private float _moveTimer = 0f;
	private float _moveSpeed = 0.1f;
	private Triangle _currentNode;

	private void Awake()
	{
		_generator = new IcosphereGenerator();
		_meshFilter = GetComponent<MeshFilter>();
		_meshRenderer = GetComponent<MeshRenderer>();

		_mesh = new Mesh();
		_meshFilter.sharedMesh = _mesh;

		_materialInstance = new Material(material);
		_meshRenderer.material = _materialInstance;
	}

	private void Update()
	{
		if (_lastResolution != resolution)
		{
			_lastResolution = resolution;
			GenerateMesh();
		}

		_moveTimer += Time.deltaTime;
		if (_moveTimer >= _moveSpeed)
		{
			_moveTimer -= _moveSpeed;

			Color32[] colors32 = new Color32[_mesh.colors32.Length];
			_mesh.colors32.CopyTo(colors32, 0);
			colors32[_currentNode.Index * 3 + 0] = Color.green;
			colors32[_currentNode.Index * 3 + 1] = Color.green;
			colors32[_currentNode.Index * 3 + 2] = Color.green;
			_mesh.SetColors(colors32);

			_currentNode = _currentNode.Neighbors[Random.Range(0, 3)];
		}
	}

	public void GenerateMesh()
	{
		// Create the triangles and vertices for the icosphere mesh
		_generator.Generate(resolution);

		// Create lists for storing mesh data
		int vertexCount = _generator.Triangles.Count * 3;
		int[] indices = new int[vertexCount];
		Vector3[] vertices = new Vector3[vertexCount];
		Vector3[] normals = new Vector3[vertexCount];
		Color32[] colors32 = new Color32[vertexCount];

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

			colors32[i * 3 + 0] = Color.cyan;
			colors32[i * 3 + 1] = Color.cyan;
			colors32[i * 3 + 2] = Color.cyan;
		}

		// Save the mesh data so it can be displayed on the mesh
		_mesh.Clear();
		_mesh.vertices = vertices;
		_mesh.normals = normals;
		_mesh.SetTriangles(indices, 0);
		_mesh.SetColors(colors32);

		_currentNode = _generator.Triangles[0];
	}
}
