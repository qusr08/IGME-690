using Procedural.Generators;
using Procedural.Jobs;
using Procedural.Streams;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.InputManagerEntry;

namespace Planet
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlanetGeneratorComponent : MonoBehaviour
    {
        private static readonly MeshJobScheduleDelegate[] _meshJobs =
        {
            MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
            MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel,
            MeshJob<SharedTriangleGrid, SingleStream>.ScheduleParallel,
            MeshJob<PointyHexagonGrid, SingleStream>.ScheduleParallel,
            MeshJob<FlatHexagonGrid, SingleStream>.ScheduleParallel,
            MeshJob<UVSphere, SingleStream>.ScheduleParallel,
            MeshJob<CubeSphere, SingleStream>.ScheduleParallel,
            MeshJob<SharedCubeSphere, PositionStream>.ScheduleParallel,
            MeshJob<Octasphere, SingleStream>.ScheduleParallel,
            MeshJob<GeoOctasphere, SingleStream>.ScheduleParallel,
            MeshJob<Icosphere, PositionStream>.ScheduleParallel,
            MeshJob<GeoIcosphere, PositionStream>.ScheduleParallel
        };

        [SerializeField] private Material[] _materials;
        [Space]
        [SerializeField, Range(1, 50)] private int _resolution = 1;
        [SerializeField] private MeshType _meshType;
        [SerializeField] private GizmoMode _gizmoMode;
        [SerializeField] private MaterialMode _materialMode;
        [SerializeField] private MeshOptimizationMode _meshOptimizationMode;

        private bool _isDirty;
        private bool _isGeneratingMesh;
        private Mesh _mesh;
        [System.NonSerialized] private Vector3[] _vertices;
        [System.NonSerialized] private Vector3[] _normals;
        [System.NonSerialized] private Vector4[] _tangents;
        [System.NonSerialized] private int[] _triangles;

        private TriangleNode currentNode;
        private float moveTimer = 0f;
        private float moveSpeed = 0.1f;
        private NativeArray<TriangleNode> _triangleNodes;

        private void Awake()
        {
            _mesh = new Mesh
            {
                name = "Procedural Planet"
            };

            GetComponent<MeshFilter>().mesh = _mesh;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                _isDirty = true;
            }
        }

        private void Update()
        {
            if (_isDirty)
            {
                _isDirty = false;
                GenerateMesh();
            }

            if (_isGeneratingMesh)
            {
                return;
            }

            moveTimer += Time.deltaTime;
            if (moveTimer >= moveSpeed)
            {
                moveTimer -= moveSpeed;
                currentNode = _triangleNodes[currentNode.SelectRandomConnection()];

                Color32[] color32s = new Color32[_mesh.vertices.Length];
                for (int i = 0; i < color32s.Length; i++)
                {
                    if (currentNode.V1 == i || currentNode.V2 == i || currentNode.V3 == i)
                    {
                        color32s[i] = Color.red;
                    }
                    else
                    {
                        color32s[i] = Color.green;
                    }
                }
                _mesh.SetColors(color32s);
            }
        }

        private void OnDrawGizmos()
        {
            if (_gizmoMode == GizmoMode.Nothing || _mesh == null)
            {
                return;
            }

            bool drawVertices = (_gizmoMode & GizmoMode.Vertices) != 0;
            bool drawNormals = (_gizmoMode & GizmoMode.Normals) != 0;
            bool drawTangents = (_gizmoMode & GizmoMode.Tangents) != 0;
            bool drawTriangles = (_gizmoMode & GizmoMode.Triangles) != 0;

            if (_vertices == null)
            {
                _vertices = _mesh.vertices;
            }
            if (drawNormals && _normals == null)
            {
                drawNormals = _mesh.HasVertexAttribute(VertexAttribute.Normal);
                if (drawNormals)
                {
                    _normals = _mesh.normals;
                }
            }
            if (drawTangents && _tangents == null)
            {
                drawTangents = _mesh.HasVertexAttribute(VertexAttribute.Tangent);
                if (drawTangents)
                {
                    _tangents = _mesh.tangents;
                }
            }
            if (drawTriangles && _triangles == null)
            {
                _triangles = _mesh.triangles;
            }

            Transform t = transform;
            for (int i = 0; i < _vertices.Length; i++)
            {
                Vector3 position = t.TransformPoint(_vertices[i]);
                if (drawVertices)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(position, 0.02f);
                }
                if (drawNormals)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(position, t.TransformDirection(_normals[i]) * 0.2f);
                }
                if (drawTangents)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(position, t.TransformDirection(_tangents[i]) * 0.2f);
                }
            }

            if (drawTriangles)
            {
                float colorStep = 1f / (_triangles.Length - 3);
                for (int i = 0; i < _triangles.Length; i += 3)
                {
                    float c = i * colorStep;
                    Gizmos.color = new Color(c, 0f, c);
                    Gizmos.DrawSphere(
                        t.TransformPoint((_vertices[_triangles[i]] + _vertices[_triangles[i + 1]] + _vertices[_triangles[i + 2]]) * (1f / 3f)),
                        0.02f
                    );
                }
            }
        }

        private void GenerateMesh()
        {
            _isGeneratingMesh = true;

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];
            JobHandle meshJobHandle = _meshJobs[(int)_meshType](_mesh, meshData, _resolution, default);
            JobHandle gridJobHandle = GridJob.Schedule(_mesh, meshJobHandle, out GridJob gridJob);
            gridJobHandle.Complete();
            Debug.Log("End of job: " + gridJob.TriangleNodes.IsCreated);
            currentNode = gridJob.TriangleNodes[0];

            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _mesh);

            if (_meshOptimizationMode == MeshOptimizationMode.ReorderIndices)
            {
                _mesh.OptimizeIndexBuffers();
            }
            else if (_meshOptimizationMode == MeshOptimizationMode.ReorderVertices)
            {
                _mesh.OptimizeReorderVertexBuffer();
            }
            else if (_meshOptimizationMode != MeshOptimizationMode.Nothing)
            {
                _mesh.Optimize();
            }

            _vertices = null;
            _normals = null;
            _tangents = null;
            _triangles = null;
            GetComponent<MeshRenderer>().material = _materials[(int)_materialMode];

            _isGeneratingMesh = false;
        }

        public void SetMeshColors(Color[] colors)
        {
            if (_isGeneratingMesh)
            {
                return;
            }

            _mesh.SetColors(colors);
        }
    }
}