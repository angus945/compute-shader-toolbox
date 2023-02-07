using UnityEngine;

namespace SurfaceScatter
{
    public class ScatterInstancer : MonoBehaviour
    {
        [SerializeField] PointScatter scatter;
        [SerializeField] ComputeShader compute;

        public Matrix4x4[] checkTransforms;

        public Mesh mesh;
        public Material material;
        public float globalScale;

        int kernel;
        ComputeBuffer argsBuffer;
        ComputeBuffer scatterBuffer;
        ComputeBuffer previewBuffer;
        ComputeBuffer directionalBuffer;

        int instanceCount;

        void OnEnable()
        {
            instanceCount = scatter.pointCloud.Length;

            scatterBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 4 * 4, ComputeBufferType.Structured);
            previewBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 4 * 4, ComputeBufferType.Structured);
            directionalBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 4, ComputeBufferType.Structured);
            argsBuffer = new ComputeBuffer(6, sizeof(uint), ComputeBufferType.IndirectArguments);

            SetBufferDatas();
        }
        void OnDisable()
        {
            scatterBuffer.Dispose();
            previewBuffer.Dispose();
            directionalBuffer.Dispose();
            argsBuffer.Dispose();
        }

        void SetBufferDatas()
        {
            scatterBuffer.SetData(scatter.pointCloudMatrics);
            directionalBuffer.SetData(scatter.pointCloudDirectionals);

            argsBuffer.SetData(new uint[6]
            {
                (uint)mesh.GetIndexCount(0),
                (uint)scatter.pointCloudMatrics.Length,
                (uint)mesh.GetIndexStart(0),
                (uint)mesh.GetBaseVertex(0),
                0,
                0,
            });

            int kernel = compute.FindKernel("CSMain");
            compute.SetBuffer(kernel, "scatterBuffer", scatterBuffer);
            compute.SetBuffer(kernel, "previewBuffer", previewBuffer);

            material.SetBuffer("transformBuffer", previewBuffer);
            material.SetBuffer("directionBuffer", directionalBuffer);
        }

        void Update()
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100);

            compute.SetFloat("_UnitScale", globalScale);

            compute.Dispatch(kernel, (scatter.pointCloud.Length / 640 + 1), 1, 1);
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);

            checkTransforms = new Matrix4x4[instanceCount];
            previewBuffer.GetData(checkTransforms);
        }
    }

}
