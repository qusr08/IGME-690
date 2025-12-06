using System.Collections.Generic;
using UnityEngine;

public struct Triangle
{
	public readonly int A, B, C;
	public readonly int Index;
	public readonly List<Triangle> Neighbors;

	public Vector3 Center;
	public Vector3 Normal;

	public Triangle(int a, int b, int c, int index)
	{
		A = a;
		B = b;
		C = c;
		Index = index;
		Neighbors = new List<Triangle>();

		Center = Vector3.zero;
		Normal = Vector3.zero;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Triangle triangle))
			return false;

		bool hasA = triangle.A == A || triangle.A == B || triangle.A == C;
		bool hasB = triangle.B == A || triangle.B == B || triangle.B == C;
		bool hasC = triangle.C == A || triangle.C == B || triangle.C == C;
		return hasA && hasB && hasC;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return $"({A}, {B}, {C})";
	}

	public static bool operator ==(Triangle a, Triangle b) => a.Equals(b);
	public static bool operator !=(Triangle a, Triangle b) => !a.Equals(b);
}
