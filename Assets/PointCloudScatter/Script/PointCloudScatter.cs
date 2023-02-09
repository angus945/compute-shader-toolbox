using UnityEngine;

[System.Serializable]
public struct PointCloudFilter
{
    public static int Size 
    { 
        get => sizeof(int) + sizeof(float) * 3 + sizeof(float) * 3 + sizeof(float) * 2; 
    }

    [Min(0)] public int type;
    public Vector3 v1;
    public Vector3 v2;

    [Min(0)] public float fade;
    public float filte;
}

[System.Serializable]
public struct ScatterPoint
{
    public static int Size { get => sizeof(float) * 4 * 4 + sizeof(float) * 3 * 2; }

    public Matrix4x4 transform;
    public Vector3 direction;
    public Vector3 randomize;
}

public class PointCloudScatter
{
    const string shaderPath = "ScatterCompute";
    const string kernelName = "ScatterKernel";
    const int numthread = 640;

    static int triangleBufferNameID = Shader.PropertyToID("trianglesBuffer"),
               verticesBufferNameID = Shader.PropertyToID("verticesBuffer"),
               normalBufferNameID = Shader.PropertyToID("normalsBuffer");

    static int _FaceCoundNameID = Shader.PropertyToID("_FaceCount"),
               _SeedNameID = Shader.PropertyToID("_Seed"),
               _DensityNameID = Shader.PropertyToID("_Density"),
               _LocalToWorldMatNameID = Shader.PropertyToID("_LocalToWorldMat");

    static int filterCountNameID = Shader.PropertyToID("_FilterCount"),
               filtersBufferNameID = Shader.PropertyToID("filtersBuffer");

    static int _AlignDirectionNameID = Shader.PropertyToID("_AlignDirection"),
               _BaseOffsetNameID = Shader.PropertyToID("_BaseOffset"),
               _BaseRotateNameID = Shader.PropertyToID("_BaseRotate"),
               _BaseSizeNameID = Shader.PropertyToID("_BaseSize"),
               _BaseExtrudeNameID = Shader.PropertyToID("_BaseExtrude");

    static int _RndOffsetNameID = Shader.PropertyToID("_RndOffset"),
               _RndRotateNameID = Shader.PropertyToID("_RndRotate"),
               _RndScaleNameID = Shader.PropertyToID("_RndScale"),
               _RndExtrudeNameID = Shader.PropertyToID("_RndExtrude");

    static int scatterBufferNameID = Shader.PropertyToID("scatterBuffer");

    static ComputeShader compute;

    static void Init()
    {
        if (compute != null) return;

        compute = Resources.Load<ComputeShader>(shaderPath);
    }
    public static void ScattingPoints(ScatterData data, MeshBuffer mesh, ScatterBuffer buffer, FilterBuffer filter)
    {
        Init();

        int faceCount = mesh.trianglesBuffer.count / 3;
        int kernel = compute.FindKernel(kernelName);

        //Instance
        compute.SetInt(_FaceCoundNameID, faceCount);
        compute.SetFloat(_SeedNameID, data.seed);
        compute.SetFloat(_DensityNameID, data.density);
        compute.SetMatrix(_LocalToWorldMatNameID, mesh.localToWorld);

        //Mesh
        compute.SetBuffer(kernel, triangleBufferNameID, mesh.trianglesBuffer);
        compute.SetBuffer(kernel, verticesBufferNameID, mesh.verticesBuffer);
        compute.SetBuffer(kernel, normalBufferNameID, mesh.normalsBuffer);

        //Filtering
        if(filter.activeFilter)
        {
            compute.SetInt(filterCountNameID, filter.buffer.count);
            compute.SetBuffer(kernel, filtersBufferNameID, filter.buffer);
        }
        else compute.SetInt(filterCountNameID, 0);

        //Transform
        compute.SetBool(_AlignDirectionNameID, data.alignDirection);
        compute.SetVector(_BaseOffsetNameID, data.baseOffset);
        compute.SetVector(_BaseRotateNameID, data.baseRotate * Mathf.Deg2Rad);
        compute.SetVector(_BaseSizeNameID, data.baseSize);
        compute.SetVector(_BaseExtrudeNameID, data.baseExtrude);

        //Randomize
        compute.SetVector(_RndOffsetNameID, data.rndOffset);
        compute.SetVector(_RndRotateNameID, data.rndRotate * Mathf.Deg2Rad);
        compute.SetVector(_RndScaleNameID, data.rndScale);
        compute.SetVector(_RndExtrudeNameID, data.rndExtrude);
        
        //Result
        compute.SetBuffer(kernel, scatterBufferNameID, buffer.pointCloudBuffer);

        compute.Dispatch(kernel, (faceCount / numthread + 1), 1, 1);

        buffer.CopyCount();
    }

    //
    int instanceCount;
    ScatterData data;
    MeshBuffer[] meshBuffers;
    FilterBuffer filterBuffer;
    ScatterBuffer scatterBuffer;
    public ComputeBuffer pointCloudBuffer { get => scatterBuffer.pointCloudBuffer; }
    public void CreateBuffers(MeshFilter[] sampleTargets, ScatterData scatterData)
    {
        this.data = scatterData;
        meshBuffers = new MeshBuffer[sampleTargets.Length];

        for (int i = 0; i < meshBuffers.Length; i++)
        {
            Mesh mesh = sampleTargets[i].sharedMesh;
            Matrix4x4 localToWorld = sampleTargets[i].transform.localToWorldMatrix;
            meshBuffers[i] = new MeshBuffer(mesh, localToWorld);
        }

        filterBuffer = new FilterBuffer(scatterData.filter.Length);
        scatterBuffer = new ScatterBuffer(scatterData.maxScattered);
    }
    public void ScatteringPoints()
    {
        filterBuffer.SetData(data.filter);

        scatterBuffer.ResetCounter();

        for (int i = 0; i < meshBuffers.Length; i++)
        {
            PointCloudScatter.ScattingPoints(data, meshBuffers[i], scatterBuffer, filterBuffer);
        }

        instanceCount = scatterBuffer.count;
    }
    public void UpdateTransform(MeshFilter[] sampleTargets)
    {
        for (int i = 0; i < sampleTargets.Length; i++)
        {
            meshBuffers[i].localToWorld = sampleTargets[i].transform.localToWorldMatrix;
        }
    }
    public int GetPointCloud(ref ScatterPoint[] pointCloud)
    {
        scatterBuffer.GetDatas(ref pointCloud);

        return scatterBuffer.count;
    }
    public void ReleaseBuffers()
    {
        for (int i = 0; i < meshBuffers.Length; i++)
        {
            meshBuffers[i].Release();
        }
        scatterBuffer.Release();
        filterBuffer.Release();
    }
}
