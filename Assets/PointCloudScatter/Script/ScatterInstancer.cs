using UnityEngine;

public class MeshBuffer
{
    public ComputeBuffer trianglesBuffer;
    public ComputeBuffer verticesBuffer;
    public ComputeBuffer normalsBuffer;
    public Matrix4x4 localToWorld;

    public MeshBuffer(Mesh mesh, Matrix4x4 localToWorld)
    {
        trianglesBuffer = new ComputeBuffer(mesh.triangles.Length, sizeof(int), ComputeBufferType.Structured);
        verticesBuffer = new ComputeBuffer(mesh.vertices.Length, sizeof(float) * 3, ComputeBufferType.Structured);
        normalsBuffer = new ComputeBuffer(mesh.normals.Length, sizeof(float) * 3, ComputeBufferType.Structured);

        this.localToWorld = localToWorld;

        trianglesBuffer.SetData(mesh.triangles);
        verticesBuffer.SetData(mesh.vertices);
        normalsBuffer.SetData(mesh.normals);
    }
    public void Release()
    {
        trianglesBuffer.Release();
        verticesBuffer.Release();
        normalsBuffer.Release();
    }
}

public class FilterBuffer
{
    public bool activeFilter;
    public ComputeBuffer buffer;

    public FilterBuffer(int filterCount)
    {
        activeFilter = filterCount > 0;

        if (!activeFilter) return;

        buffer = new ComputeBuffer(filterCount, PointCloudFilter.Size, ComputeBufferType.Structured);
    }
    public void SetData(PointCloudFilter[] filters)
    {
        if (!activeFilter) return;

        buffer.SetData(filters);
    }
    public void Release()
    {
        if (!activeFilter) return;

        buffer.Release();
    }
}


[ExecuteInEditMode]
public class ScatterInstancer : MonoBehaviour
{
    [SerializeField] ScatterData scatterData;

    [SerializeField] MeshFilter[] sampleTargets;
    PointCloudScatter scatter;
    public GPUInstancer instancer;

    [Header("Debug")]
    public int instanceCount;
    public ScatterVisualizer.Options options;
    public bool instance;
    ScatterPoint[] pointCloud;

    void Awake()
    {
        if (scatterData == null)
        {
            gameObject.SetActive(false);
        }
    }
    void OnEnable()
    {
        scatter = new PointCloudScatter();
        scatter.CreateBuffers(sampleTargets, scatterData);
        scatter.ScatteringPoints();

        pointCloud = new ScatterPoint[scatterData.maxScattered];
        instanceCount = scatter.GetPointCloud(ref pointCloud);

        //
        //instancer = new GPUInstancer();
        instancer.Initial(instanceCount);
        instancer.SetBuffer("scatterBuffer", scatter.pointCloudBuffer);
    }

    void Update()
    {
        scatter.UpdateTransform(sampleTargets);
        scatter.ScatteringPoints();
        instanceCount = scatter.GetPointCloud(ref pointCloud);

        if(instance)
        {
            instancer.CopyCount(scatter.pointCloudBuffer);
            instancer.Render();
        }
    }

    void OnDisable()
    {
        scatter.ReleaseBuffers();
        instancer.Release();
    }

    [ContextMenu("Reload")]
    void Reload()
    {
        OnDisable();
        OnEnable();
    }

    //Debug
    void OnDrawGizmos()
    {
        ScatterVisualizer.DrawSampleTraget(sampleTargets, options);
        ScatterVisualizer.DrawPointCloud(pointCloud, instanceCount, options);
    }
}
