using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GPUInstancer
{
    //static int transformBufferNameID = Shader.PropertyToID("transformsBuffer");
    static Bounds bounds = new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f));

    public int maxInstance { get; private set; }

    uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    ComputeBuffer argsBuffer;
    //ComputeBuffer transformBuffer;
    //ComputeBuffer cullingBuffer;

    [SerializeField] Mesh mesh;
    [SerializeField] Material material;

    //public void Initial(ComputeBuffer transformsBuffer, Mesh mesh, Material material)
    //{
    //    this.maxInstance = transformsBuffer.count;

    //    CreateBuffer(transformsBuffer);
    //    SetMesh(mesh);
    //    SetMaterial(material);
    //}
    //public void Initial(ComputeBuffer transformsBuffer)
    //{
    //    this.maxInstance = transformsBuffer.count;

    //    CreateBuffer(transformsBuffer);
    //    SetMesh(this.mesh);
    //    SetMaterial(this.material);
    //}
    public void Initial(int maxInstance, Mesh mesh, Material material)
    {
        this.maxInstance = maxInstance;

        CreateBuffer();
        SetMesh(mesh);
        SetMaterial(material);
    }
    public void Initial(int maxInstance)
    {
        this.maxInstance = maxInstance;

        CreateBuffer();
        SetMesh(this.mesh);
        SetMaterial(this.material);
    }

    void CreateBuffer(ComputeBuffer sourceBuffer = null)
    {
        //if (sourceBuffer == null)
        //{
        //    transformBuffer = new ComputeBuffer(maxInstance, sizeof(float), ComputeBufferType.Structured);
        //}
        //else transformBuffer = sourceBuffer;

        argsBuffer = new ComputeBuffer(args.Length, sizeof(uint), ComputeBufferType.IndirectArguments);
        //cullingBuffer = new ComputeBuffer(maxInstance, sizeof(float), ComputeBufferType.Append);
    }
    public void SetMesh(Mesh mesh)
    {
        this.mesh = mesh;

        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)maxInstance;
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);

        argsBuffer.SetData(args);
    }
    public void SetMaterial(Material material)
    {
        this.material = material;
    }
    public void SetBuffer(string name, ComputeBuffer buffer)
    {
        material.SetBuffer(name, buffer);
    }

    public void Render()
    {
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }

    public void Release()
    {
        //argsBuffer.Release();
        //transformBuffer.Release();
        //argsBuffer.Release();
    }
}
