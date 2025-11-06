using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Procedural.Streams
{
	[StructLayout(LayoutKind.Sequential)]
	public struct TriangleUInt16
	{
		public ushort A, B, C;

		public static implicit operator TriangleUInt16(int3 t) => new TriangleUInt16
		{
			A = (ushort)t.x,
			B = (ushort)t.y,
			C = (ushort)t.z
		};
	}
}