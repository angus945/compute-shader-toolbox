using UnityEngine;

namespace SurfaceScatter
{
    public class ScatterInstancer : MonoBehaviour
    {
        [SerializeField] PointScatter scatter;
        [SerializeField] ComputeShader compute;

        public Mesh mesh;
        public Material material;
        public float globalScale;

        int kernel;
        ComputeBuffer argsBuffer;
        ComputeBuffer scatterBuffer;
        ComputeBuffer previewBuffer;

        void OnEnable()
        {
            scatterBuffer = new ComputeBuffer(1000000, sizeof(float) * 4 * 4, ComputeBufferType.Structured);
            previewBuffer = new ComputeBuffer(1000000, sizeof(float) * 4 * 4, ComputeBufferType.Structured);
            argsBuffer = new ComputeBuffer(6, sizeof(uint), ComputeBufferType.IndirectArguments);

            SetBufferDatas();
        }
        void OnDisable()
        {
            scatterBuffer.Dispose();
            previewBuffer.Dispose();
            argsBuffer.Dispose();
        }

        void SetBufferDatas()
        {
            scatterBuffer.SetData(scatter.pointCloudMatrix);
            argsBuffer.SetData(new uint[6]
            {
                (uint)mesh.GetIndexCount(0),
                (uint)scatter.pointCloudMatrix.Length,
                (uint)mesh.GetIndexStart(0),
                (uint)mesh.GetBaseVertex(0),
                0,
                0,
            });

            int kernel = compute.FindKernel("CSMain");
            compute.SetBuffer(kernel, "scatterBuffer", scatterBuffer);
            compute.SetBuffer(kernel, "previewBuffer", previewBuffer);

            material.SetBuffer("transformBuffer", previewBuffer);
        }

        void Update()
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100);

            compute.SetFloat("_UnitScale", globalScale);

            compute.Dispatch(kernel, (scatter.pointCloud.Length / 640 + 1), 1, 1);
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
        }
    }

}
