using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class IcosphereMesh : MonoBehaviour
{
	[SerializeField] private bool isDirty = false;
	[Space]
	[SerializeField] private Gradient colorMap;
	[SerializeField, Range(0f, 1f)] private float colorVariation = 0.05f;
	[SerializeField, Range(0, 5)] private int resolution = 2;
	[SerializeField, Range(0, 1000)] private int iterations = 250;
	[SerializeField, Range(0, 5)] private int leaders = 2;
	[SerializeField, Range(0, 50)] private int backtrackPrevention = 20;
	[SerializeField] private Vector2 range = new Vector2(0f, 1f);
	[Space]
	[SerializeField, Range(0f, 60f)] private float rotationSpeed = 15f;
	[SerializeField, Range(0f, 60f)] private float orbitSpeed = 15f;
	[SerializeField, Range(0f, 30f)] private float orbitDistance = 0f;
	[SerializeField, Range(0f, 90f)] private float orbitAngleTilt = 15f;

	private Mesh _mesh;
	private MeshFilter _meshFilter;
	private MeshRenderer _meshRenderer;
	private Material _materialInstance;
	private IcosphereGenerator _generator;

	private float _orbitAngle;

	protected virtual void Awake()
	{
		_generator = new IcosphereGenerator();
		_meshFilter = GetComponent<MeshFilter>();
		_meshRenderer = GetComponent<MeshRenderer>();

		_mesh = new Mesh();
		_meshFilter.sharedMesh = _mesh;

		_materialInstance = new Material(_meshRenderer.material);
		_meshRenderer.material = _materialInstance;

		isDirty = true;
	}

	protected virtual void Update()
	{
		if (isDirty)
		{
			GenerateMesh();
			isDirty = false;
		}

		_orbitAngle += orbitSpeed * Time.deltaTime;
		if (_orbitAngle >= 360f)
			_orbitAngle -= 360f;

		float orbitX = Mathf.Cos(_orbitAngle * Mathf.Deg2Rad) * orbitDistance;
		float orbitY = Mathf.Sin(_orbitAngle * Mathf.Deg2Rad) * Mathf.Tan(orbitAngleTilt * Mathf.Deg2Rad) * orbitDistance;
		float orbitZ = Mathf.Sin(_orbitAngle * Mathf.Deg2Rad) * orbitDistance;
		transform.localPosition = orbitDistance * new Vector3(orbitX, orbitY, orbitZ);
		transform.localRotation *= Quaternion.Euler(0f, rotationSpeed * Time.deltaTime, 0f);
	}

	public void GenerateMesh()
	{
		// Create the triangles and vertices for the icosphere mesh
		_generator.Generate(resolution, iterations, leaders, backtrackPrevention, range);

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
			triangle.Normal = Vector3.Cross(vertex2 - vertex1, vertex3 - vertex1).normalized;
			normals[i * 3 + 0] = triangle.Normal;
			normals[i * 3 + 1] = triangle.Normal;
			normals[i * 3 + 2] = triangle.Normal;

			// Use the height of the triangle to determine the color
			triangle.Center = (vertex1 + vertex2 + vertex3) / 3f;
			Color color = GetOffsetColor(colorMap.Evaluate(Utils.Map(triangle.Center.magnitude, range.x, range.y, 0f, 1f)));
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
	}

	private Color GetOffsetColor(Color color)
	{
		Color.RGBToHSV(color, out float h, out float s, out float v);
		float newS = Mathf.Clamp01(Random.Range(-colorVariation, colorVariation) + s);
		float newV = Mathf.Clamp01(Random.Range(-colorVariation, colorVariation) + v);
		return Color.HSVToRGB(h, newS, newV);
	}
}
