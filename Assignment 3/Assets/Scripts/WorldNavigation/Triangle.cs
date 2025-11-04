using System.Collections.Generic;

namespace WorldNavigation
{
    public struct Triangle
    {
        public int V1, V2, V3;
        public List<Triangle> Neighbors;

        public Triangle(int v1, int v2, int v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            Neighbors = new List<Triangle>();
        }

        public override string ToString()
        {
            return $"({V1}, {V2}, {V3})";
        }
    }
}