using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUInstance : MonoBehaviour
{
    [SerializeField] ComputeShader compute;

    [Header("Render")]
    [SerializeField][Range(0, 1000)] int instanceCount = 10;
    [SerializeField] Mesh instanceMesh;
    [SerializeField] Material material;

    //Compute
    int kernel;
    ComputeBuffer argsBuffer;
    ComputeBuffer positionBuffer;
    ComputeBuffer resultBuffer;

    //Datas
    uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    void Start()
    {
        CreateArgs();
        Generate();
        SetCompute();
    }
    void Update()
    {
        Compute();
        Render();
    }
    void OnDestroy()
    {
        Release();
    }

    void CreateArgs()
    {
        args[0] = (uint)instanceMesh.GetIndexCount(0);
        args[1] = (uint)instanceCount;
        args[2] = (uint)instanceMesh.GetIndexStart(0);
        args[3] = (uint)instanceMesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }
    void Generate()
    {
        positionBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 16, ComputeBufferType.Structured);
        resultBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 16, ComputeBufferType.Append);

        Matrix4x4[] matrices = new Matrix4x4[instanceCount];
        for (int i = 0; i < matrices.Length; i++)
        {
            matrices[i] = Matrix4x4.Translate(Random.insideUnitSphere * 20);
        }

        positionBuffer.SetData(matrices);
        material.SetBuffer("transformBuffer", positionBuffer);
    }
    void SetCompute()
    {
        kernel = compute.FindKernel("CSMin");
        compute.SetInt("instanceCount", instanceCount);

        compute.SetBuffer(kernel, "positionBuffer", positionBuffer);
        compute.SetBuffer(kernel, "resultBuffer", resultBuffer);
    }

    void Compute()
    {
        resultBuffer.SetCounterValue(0);

        compute.Dispatch(kernel, (instanceCount / 640 + 1), 1, 1);

        ComputeBuffer.CopyCount(resultBuffer, argsBuffer, sizeof(uint));
    }
    void Render()
    {
        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, material, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
    }

    public void Release()
    {
        argsBuffer.Dispose();
        positionBuffer.Dispose();
        resultBuffer.Dispose();
    }

}
