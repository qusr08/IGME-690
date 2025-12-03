public readonly struct Edge
{
	public readonly int V1, V2;

	public Edge(int v1, int v2)
	{
		// Make sure the order of the indices is always the same
		if (v1.GetHashCode() > v2.GetHashCode())
		{
			V1 = v1;
			V2 = v2;
		}
		else
		{
			V1 = v2;
			V2 = v1;
		}
	}

	public override bool Equals(object obj)
	{
		return obj is Edge edge && edge.V1 == V1 && edge.V2 == V2;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return $"({V1}, {V2})";
	}
}
