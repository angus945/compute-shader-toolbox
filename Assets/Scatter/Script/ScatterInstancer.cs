using UnityEngine;

public class MeshBuffer
{
    public ComputeBuffer trianglesBuffer;
    public ComputeBuffer verticesBuffer;
    public ComputeBuffer normalsBuffer;

    public MeshBuffer(Mesh mesh)
    {
        trianglesBuffer = new ComputeBuffer(mesh.triangles.Length, sizeof(int), ComputeBufferType.Structured);
        verticesBuffer = new ComputeBuffer(mesh.vertices.Length, sizeof(float) * 3, ComputeBufferType.Structured);
        normalsBuffer = new ComputeBuffer(mesh.normals.Length, sizeof(float) * 3, ComputeBufferType.Structured);

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
    public ComputeBuffer buffer;

    public FilterBuffer(int filterCount)
    {
        buffer = new ComputeBuffer(filterCount, PointCloudFilter.Size, ComputeBufferType.Structured);
    }
    public void SetData(PointCloudFilter[] filters)
    {
        buffer.SetData(filters);
    }
    public void Release()
    {
        buffer.Release();
    }
}

[ExecuteInEditMode]
public class ScatterInstancer : MonoBehaviour
{
    [SerializeField] ScatterData scatterData;

    [SerializeField] MeshFilter[] sampleTargets;

    [SerializeField] bool drawWireframe, drawNormal, drawPointcloud, drawDirection;

    MeshBuffer[] meshBuffers;
    FilterBuffer filterBuffer;
    ScatterBuffer scatterBuffer;

    public int instanceCount;
    public Vector3[] pointCloud;
    public Vector3[] directions;
    public Vector3[] randomize;

    void Awake()
    {
        if (scatterData == null)
        {
            gameObject.SetActive(false);
        }
    }
    void OnEnable()
    {
        CreateBuffers();
        ScatteringPoints();

        pointCloud = new Vector3[scatterData.maxScattered];
        directions = new Vector3[scatterData.maxScattered];
        randomize = new Vector3[scatterData.maxScattered];
        scatterBuffer.GetDatas(ref pointCloud, ref directions, ref randomize);
    }
    void Update()
    {
        EditorPreview();
    }

    void EditorPreview()
    {
        if (Application.isPlaying) return;

        ScatteringPoints();

        pointCloud = new Vector3[scatterData.maxScattered];
        directions = new Vector3[scatterData.maxScattered];
        randomize = new Vector3[scatterData.maxScattered];
        scatterBuffer.GetDatas(ref pointCloud, ref directions, ref randomize);
    }

    void OnDisable()
    {
        ReleaseBuffers();
    }

    void CreateBuffers()
    {
        meshBuffers = new MeshBuffer[sampleTargets.Length];

        for (int i = 0; i < meshBuffers.Length; i++)
        {
            Mesh mesh = sampleTargets[i].sharedMesh;
            meshBuffers[i] = new MeshBuffer(mesh);
        }

        filterBuffer = new FilterBuffer(scatterData.filter.Length);
        scatterBuffer = new ScatterBuffer(scatterData.maxScattered);
    }

    //
    void ScatteringPoints()
    {
        filterBuffer.SetData(scatterData.filter);

        scatterBuffer.ResetCounter();

        for (int i = 0; i < meshBuffers.Length; i++)
        {
            PointCloudScatter.Option option = new PointCloudScatter.Option()
            {
                seed = scatterData.seed,
                density = scatterData.density,
                localToWorld = sampleTargets[i].transform.localToWorldMatrix,
            };
            PointCloudScatter.ScattingPoints(option, meshBuffers[i], scatterBuffer, filterBuffer);
        }

        instanceCount = scatterBuffer.count;

        //PointCloudFilter.FilteWithPlane(instanceCount, new Vector4(0, 1, 0, 0), scatterBuffer);

        instanceCount = scatterBuffer.count;
    }

    //Release
    void ReleaseBuffers()
    {
        for (int i = 0; i < meshBuffers.Length; i++)
        {
            meshBuffers[i].Release();
        }
        scatterBuffer.Release();
        filterBuffer.Release();
    }

    //Debug
    void OnDrawGizmos()
    {
        ScatterVisualizer.DrawSampleTraget(sampleTargets, drawWireframe, drawNormal);
        ScatterVisualizer.DrawPointCloud(pointCloud, directions, randomize, instanceCount, drawPointcloud, drawDirection);
    }
}
