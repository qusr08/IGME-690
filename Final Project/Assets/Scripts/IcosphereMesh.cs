using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class IcosphereMesh : MonoBehaviour
{
	[SerializeField] private Material material;
	[Space]
	[SerializeField, Range(0, 5)] private int resolution = 2;
	[SerializeField, Range(0f, 10f)] private float frequency = 5f;
	[SerializeField, Range(0f, 1f)] private float range = 0.2f;

	private Mesh _mesh;
	private MeshFilter _meshFilter;
	private MeshRenderer _meshRenderer;
	private Material _materialInstance;
	private IcosphereGenerator _generator;

	private int _lastResolution = -1;
	private float _lastFrequency = -1f;
	private float _lastRange = -1f;

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
		if (_lastResolution != resolution || _lastFrequency != frequency || _lastRange != range)
		{
			_lastResolution = resolution;
			_lastFrequency = frequency;
			_lastRange = range;
			GenerateMesh();
		}

		//_moveTimer += Time.deltaTime;
		//if (_moveTimer >= _moveSpeed)
		//{
		//	_moveTimer -= _moveSpeed;

		//	Color32[] colors32 = new Color32[_mesh.colors32.Length];
		//	_mesh.colors32.CopyTo(colors32, 0);
		//	colors32[_currentNode.Index * 3 + 0] = Color.green;
		//	colors32[_currentNode.Index * 3 + 1] = Color.green;
		//	colors32[_currentNode.Index * 3 + 2] = Color.green;
		//	_mesh.SetColors(colors32);

		//	_currentNode = _currentNode.Neighbors[Random.Range(0, 3)];
		//}

		transform.rotation *= Quaternion.Euler(0f, 25f * Time.deltaTime, 0f);
	}

	public void GenerateMesh()
	{
		// Create the triangles and vertices for the icosphere mesh
		_generator.Generate(resolution, frequency, range);

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
			Vector3 vertex1 = _generator.Vertices[triangle.A];
			Vector3 vertex2 = _generator.Vertices[triangle.B];
			Vector3 vertex3 = _generator.Vertices[triangle.C];

			indices[i * 3 + 0] = i * 3 + 0;
			indices[i * 3 + 1] = i * 3 + 1;
			indices[i * 3 + 2] = i * 3 + 2;

			vertices[i * 3 + 0] = vertex1;
			vertices[i * 3 + 1] = vertex2;
			vertices[i * 3 + 2] = vertex3;

			// Calculate the normal for the triangle plane, then set that to be each of the vertex normals
			// Each triangle of the mesh is separate, so this works to create appropriate shadows
			Vector3 triangleNormal = Vector3.Cross(vertex2 - vertex1, vertex3 - vertex1);
			normals[i * 3 + 0] = triangleNormal.normalized;
			normals[i * 3 + 1] = triangleNormal.normalized;
			normals[i * 3 + 2] = triangleNormal.normalized;

			// Use the height of the triangle to determine the color
			Vector3 triangleCenter = (vertex1 + vertex2 + vertex3) / 3f;
			Color color = (triangleCenter.magnitude > 1f ? Color.green : Color.cyan);
			colors32[i * 3 + 0] = color;
			colors32[i * 3 + 1] = color;
			colors32[i * 3 + 2] = color;
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
