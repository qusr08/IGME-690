using Planet;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Procedural.Jobs
{
    public delegate JobHandle GridJobScheduleDelegate(Mesh mesh, JobHandle dependency);

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct GridJob : IJob
    {
        public NativeArray<TriangleNode> TriangleNodes;

        [DeallocateOnJobCompletion]
        private NativeArray<int> _meshTriangles;

        public void Execute()
        {
            for (int i = 0; i < _meshTriangles.Length; i += 3)
            {
                int index1 = _meshTriangles[i];
                int index2 = _meshTriangles[i + 1];
                int index3 = _meshTriangles[i + 2];

                TriangleNode triangle = new TriangleNode(index1, index2, index3, i / 3);
                TriangleNodes[i / 3] = triangle;

                new EdgeNode(index1, index2).AddNeighbor(triangle);
                new EdgeNode(index2, index3).AddNeighbor(triangle);
                new EdgeNode(index3, index1).AddNeighbor(triangle);
            }
        }

        public static JobHandle Schedule(Mesh mesh, JobHandle dependency, out GridJob job)
        {
            job = new GridJob()
            {
                _meshTriangles = new NativeArray<int>(mesh.triangles, Allocator.TempJob),
                TriangleNodes = new NativeArray<TriangleNode>(mesh.triangles.Length / 3, Allocator.Persistent)
            };
            Debug.Log("Start of job: " + job.TriangleNodes.Length);

            return job.Schedule(dependency);
        }
    }
}