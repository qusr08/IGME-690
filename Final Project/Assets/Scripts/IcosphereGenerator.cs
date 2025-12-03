using System.Collections.Generic;
using UnityEngine;

public sealed class IcosphereGenerator
{
	public List<Triangle> Triangles { get; private set; }
	public List<Vector3> Vertices { get; private set; }

	private Dictionary<int, int> _midpointCache;
	private Dictionary<Edge, Triangle> _edgeCache;

	public IcosphereGenerator() { }

	public void Generate (int resolution)
	{
		CreateIcosahedron();
		Subdivide(resolution);
		CalculateTriangleNeighbors();
	}

	private void CreateIcosahedron()
	{
		Triangles = new List<Triangle>();
		Vertices = new List<Vector3>();

		// Manually create an icosahedron that can be subdivided later

		float t = (1f + Mathf.Sqrt(5f)) / 2f;

		Vertices.Add(new Vector3(-1, t, 0).normalized);
		Vertices.Add(new Vector3(1, t, 0).normalized);
		Vertices.Add(new Vector3(-1, -t, 0).normalized);
		Vertices.Add(new Vector3(1, -t, 0).normalized);
		Vertices.Add(new Vector3(0, -1, t).normalized);
		Vertices.Add(new Vector3(0, 1, t).normalized);
		Vertices.Add(new Vector3(0, -1, -t).normalized);
		Vertices.Add(new Vector3(0, 1, -t).normalized);
		Vertices.Add(new Vector3(t, 0, -1).normalized);
		Vertices.Add(new Vector3(t, 0, 1).normalized);
		Vertices.Add(new Vector3(-t, 0, -1).normalized);
		Vertices.Add(new Vector3(-t, 0, 1).normalized);

		Triangles.Add(new Triangle(0, 11, 5, Triangles.Count));
		Triangles.Add(new Triangle(0, 5, 1, Triangles.Count));
		Triangles.Add(new Triangle(0, 1, 7, Triangles.Count));
		Triangles.Add(new Triangle(0, 7, 10, Triangles.Count));
		Triangles.Add(new Triangle(0, 10, 11, Triangles.Count));
		Triangles.Add(new Triangle(1, 5, 9, Triangles.Count));
		Triangles.Add(new Triangle(5, 11, 4, Triangles.Count));
		Triangles.Add(new Triangle(11, 10, 2, Triangles.Count));
		Triangles.Add(new Triangle(10, 7, 6, Triangles.Count));
		Triangles.Add(new Triangle(7, 1, 8, Triangles.Count));
		Triangles.Add(new Triangle(3, 9, 4, Triangles.Count));
		Triangles.Add(new Triangle(3, 4, 2, Triangles.Count));
		Triangles.Add(new Triangle(3, 2, 6, Triangles.Count));
		Triangles.Add(new Triangle(3, 6, 8, Triangles.Count));
		Triangles.Add(new Triangle(3, 8, 9, Triangles.Count));
		Triangles.Add(new Triangle(4, 9, 5, Triangles.Count));
		Triangles.Add(new Triangle(2, 4, 11, Triangles.Count));
		Triangles.Add(new Triangle(6, 2, 10, Triangles.Count));
		Triangles.Add(new Triangle(8, 6, 7, Triangles.Count));
		Triangles.Add(new Triangle(9, 8, 1, Triangles.Count));
	}

	private void Subdivide(int resolution)
	{
		_midpointCache = new Dictionary<int, int>();

		for (int i = 0; i < resolution; i++)
		{
			List<Triangle> newTriangles = new List<Triangle>();
			foreach (Triangle triangle in Triangles)
			{
				// Get all midpoints of the triangle
				int ab = GetMidpointIndex(triangle.A, triangle.B);
				int bc = GetMidpointIndex(triangle.B, triangle.C);
				int ca = GetMidpointIndex(triangle.C, triangle.A);

				// Add new subdivided triangles
				newTriangles.Add(new Triangle(triangle.A, ab, ca, newTriangles.Count));
				newTriangles.Add(new Triangle(triangle.B, bc, ab, newTriangles.Count));
				newTriangles.Add(new Triangle(triangle.C, ca, bc, newTriangles.Count));
				newTriangles.Add(new Triangle(ab, bc, ca, newTriangles.Count));
			}

			Triangles = newTriangles;
		}

		// Clear the midpoint generation cache
		_midpointCache.Clear();
	}

	private int GetMidpointIndex(int indexA, int indexB)
	{
		// Create a key out of the indices
		int smallerIndex = Mathf.Min(indexA, indexB);
		int largerIndex = Mathf.Max(indexA, indexB);
		int key = (smallerIndex << 16) + largerIndex;
		int vertexIndex;

		// If the midpoint has already been found, return that index
		if (_midpointCache.TryGetValue(key, out vertexIndex))
			return vertexIndex;

		// Find the midpoint between the two vertices
		Vector3 vertex1 = Vertices[indexA];
		Vector3 vertex2 = Vertices[indexB];
		Vector3 midpoint = Vector3.Lerp(vertex1, vertex2, 0.5f).normalized;

		// Add the new midpoint as a vertex
		vertexIndex = Vertices.Count;
		Vertices.Add(midpoint);
		_midpointCache.Add(key, vertexIndex);

		return vertexIndex;
	}

	private void CalculateTriangleNeighbors()
	{
		_edgeCache = new Dictionary<Edge, Triangle>();

		foreach (Triangle triangle in Triangles)
		{
			// Add neighboring triangles based on the edges between them
			TryAddConnection(new Edge(triangle.A, triangle.B), triangle);
			TryAddConnection(new Edge(triangle.B, triangle.C), triangle);
			TryAddConnection(new Edge(triangle.C, triangle.A), triangle);
		}

		// Clear the edge cache
		_edgeCache.Clear();
	}

	private void TryAddConnection(Edge edge, Triangle triangle)
	{
		// If the edge already exists in the dictionary, then both triangles touching the edge have been found
		// If the edge is not in the dictionary, add it
		if (_edgeCache.TryGetValue(edge, out Triangle initialTriangle))
		{
			// This is safe here since an edge will only ever have 2 triangles connected to it
			initialTriangle.Neighbors.Add(triangle);
			triangle.Neighbors.Add(initialTriangle);
		}
		else
		{
			_edgeCache.Add(edge, triangle);
		}
	}
}
