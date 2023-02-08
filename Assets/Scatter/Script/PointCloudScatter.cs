using UnityEngine;

[System.Serializable]
public struct PointCloudFilter
{
    public static int Size 
    { 
        get => sizeof(int) + sizeof(float) * 3 + sizeof(float) * 3 + sizeof(float); 
    }

    [Min(-1)]
    public int type;
    public Vector3 v1;
    public Vector3 v2;
    public float intensity;
}

public static class PointCloudScatter
{
    public struct Option
    {
        public float seed;
        public float density;
        public Matrix4x4 localToWorld;
    }

    const string shaderPath = "ScatterCompute";
    const string kernelName = "ScatterKernel";
    const int numthread = 640;

    static int triangleBufferNameID = Shader.PropertyToID("trianglesBuffer"),
               verticesBufferNameID = Shader.PropertyToID("verticesBuffer"),
               normalBufferNameID = Shader.PropertyToID("normalsBuffer");

    static int scatterBufferNameID = Shader.PropertyToID("scatterBuffer"),
               directionBufferNameID = Shader.PropertyToID("directionBuffer"),
               randomizeBufferNameID = Shader.PropertyToID("randomizeBuffer");


    static int filterCountNameID = Shader.PropertyToID("_FilterCount"),
               filtersBufferNameID = Shader.PropertyToID("filtersBuffer");

    static ComputeShader compute;

    static void Init()
    {
        if (compute != null) return;

        compute = Resources.Load<ComputeShader>(shaderPath);
    }
    public static void ScattingPoints(Option option, MeshBuffer mesh, ScatterBuffer buffer, FilterBuffer filter)
    {
        Init();

        int faceCount = mesh.trianglesBuffer.count / 3;
        int kernel = compute.FindKernel(kernelName);

        //Mesh
        compute.SetInt("_FaceCount", faceCount);
        compute.SetBuffer(kernel, triangleBufferNameID, mesh.trianglesBuffer);
        compute.SetBuffer(kernel, verticesBufferNameID, mesh.verticesBuffer);
        compute.SetBuffer(kernel, normalBufferNameID, mesh.normalsBuffer);

        //Scatter
        compute.SetBuffer(kernel, scatterBufferNameID, buffer.scatteredBuffer);
        compute.SetBuffer(kernel, directionBufferNameID, buffer.directionBuffer);
        compute.SetBuffer(kernel, randomizeBufferNameID, buffer.randomizeBuffer);

        //Filter
        compute.SetInt(filterCountNameID, filter.buffer.count);
        compute.SetBuffer(kernel, filtersBufferNameID, filter.buffer);

        //
        compute.SetFloat("_Seed", option.seed);
        compute.SetFloat("_Density", option.density);
        compute.SetMatrix("_TransformMatrix", option.localToWorld);

        compute.Dispatch(kernel, (faceCount / numthread + 1), 1, 1);

        buffer.CopyCount();
    }
}
