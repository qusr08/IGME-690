using ProceduralMesh.Generators;
using ProceduralMesh.Streams;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProceduralMesh
{
	public delegate JobHandle MeshJobScheduleDelegate(Mesh mesh, Mesh.MeshData meshData, int resolution, JobHandle dependency);

	[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
	public struct MeshJob<G, S> : IJobFor where G : struct, IMeshGenerator where S : struct, IMeshStreams
	{

		private G _generator;

		[WriteOnly]
		private S _streams;

		public void Execute(int index)
		{
			_generator.Execute(index, _streams);
		}

		public static JobHandle ScheduleParallel(Mesh mesh, Mesh.MeshData meshData, int resolution, JobHandle dependency)
		{
			MeshJob<G, S> job = new MeshJob<G, S>();
			job._generator.Resolution = resolution;
			job._streams.Setup(meshData, mesh.bounds = job._generator.Bounds, job._generator.VertexCount, job._generator.IndexCount);

			return job.ScheduleParallel(job._generator.JobLength, 1, dependency);
		}
	}
}