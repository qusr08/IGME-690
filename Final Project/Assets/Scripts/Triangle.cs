using System.Collections.Generic;

public readonly struct Triangle
{
	public readonly int A, B, C;
	public readonly int Index;
	public readonly List<Triangle> Neighbors;

	public Triangle(int a, int b, int c, int index)
	{
		A = a;
		B = b;
		C = c;
		Index = index;
		Neighbors = new List<Triangle>();
	}

	public override string ToString()
	{
		return $"({A}, {B}, {C})";
	}
}
