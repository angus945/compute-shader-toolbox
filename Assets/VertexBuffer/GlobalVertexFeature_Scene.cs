using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GlobalVertexFeature_Scene : MonoBehaviour
{
    public class VertexPass
    {

        class MeshData
        {
            public Mesh mesh;
            public Matrix4x4 transform;

            public int vertexCount;
            public int stride;
            public GraphicsBuffer sourceBuffer;
            public GraphicsBuffer vertexBuffer;

            public MeshData(Mesh mesh, Matrix4x4 transform)
            {
                this.mesh = mesh;
                this.transform = transform;

                mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;

                stride = mesh.GetVertexBufferStride(0);
                vertexCount = mesh.vertexCount;

                sourceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw | GraphicsBuffer.Target.CopyDestination, vertexCount, stride);

                Graphics.CopyBuffer(mesh.GetVertexBuffer(0), sourceBuffer);
                vertexBuffer = mesh.GetVertexBuffer(0);
            }
            public void Release()
            {
                sourceBuffer.Release();
                vertexBuffer.Release();
            }
        }



        ComputeShader compute;
        MeshData[] targetBuffers;

        public VertexPass(ComputeShader vertexCompute) : base()
        {
            compute = vertexCompute;
        }

        public void Update()
        {
            for (int i = 0; i < targetBuffers.Length; i++)
            {
                MeshData meshData = targetBuffers[i];
                compute.SetInt("_VertexCount", meshData.vertexCount);
                compute.SetInt("_BufferStride", meshData.stride);

                compute.SetMatrix("_LocalToWorldMatrix", meshData.transform);
                compute.SetMatrix("_WorldToLocalMatrix", meshData.transform.inverse);

                compute.SetBuffer(0, "sourceBuffer", meshData.sourceBuffer);
                compute.SetBuffer(0, "vertexBuffer", meshData.vertexBuffer);
                compute.Dispatch(0, (meshData.vertexCount / 64) + 1, 1, 1);
            }
        }
        public void SetTargets(List<MeshFilter> effectTargets)
        {
            targetBuffers = new MeshData[effectTargets.Count];
            for (int i = 0; i < effectTargets.Count; i++)
            {
                targetBuffers[i] = new MeshData(effectTargets[i].mesh, effectTargets[i].transform.localToWorldMatrix);
            }
        }

        internal void Dispose()
        {
            if (targetBuffers == null) return;

            for (int i = 0; i < targetBuffers.Length; i++)
            {
                targetBuffers[i].Release();
            }
        }
    }

    [SerializeField] ComputeShader vertexCompute;
    [SerializeField] List<MeshFilter> targets;
    [SerializeField] float _Value;
    VertexPass pass;

    public void Awake()
    {
        pass = new VertexPass(vertexCompute);
        pass.SetTargets(targets);
    }
    public void Update()
    {
        if (!Application.isPlaying) return;

        pass.Update();
    }
    void OnDisable()
    {
        pass.Dispose();

    }



}
