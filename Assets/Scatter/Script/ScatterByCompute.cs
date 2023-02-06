using UnityEngine;

public class ScatterByCompute : MonoBehaviour
{
    [SerializeField] GameObject sampleTarget;
    [SerializeField] Material material;
    Mesh surface { get => sampleTarget.GetComponent<MeshFilter>().mesh; }
    

    [SerializeField] ComputeShader compute;
    [SerializeField] int _MaxInstance = 16 * 16;
    [SerializeField] float _Density = 3;
    [SerializeField] float _Seed = 3;

    [SerializeField] Vector3[] resultArray;
    public uint[] args;

    int kernel;
    ComputeBuffer trianglesBuffer;
    ComputeBuffer verticesBuffer;

    ComputeBuffer argsBuffer;
    ComputeBuffer scatterBuffer;
    int faceCount;

    Matrix4x4 transformMat;

    void Awake()
    {
        Application.targetFrameRate = 120;
    }
    void Start()
    {
        args = new uint[6]
        {
            (uint)surface.GetIndexCount(0),
            (uint)_MaxInstance,
            (uint)surface.GetIndexStart(0),
            (uint)surface.GetBaseVertex(0),
            0,
            0,
        };

        trianglesBuffer = new ComputeBuffer(surface.triangles.Length, sizeof(int), ComputeBufferType.Structured);
        verticesBuffer = new ComputeBuffer(surface.vertices.Length, sizeof(float) * 3, ComputeBufferType.Structured);

        trianglesBuffer.SetData(surface.triangles);
        verticesBuffer.SetData(surface.vertices);

        argsBuffer = new ComputeBuffer(6, sizeof(uint), ComputeBufferType.IndirectArguments);
        scatterBuffer = new ComputeBuffer(_MaxInstance, sizeof(float) * 3, ComputeBufferType.Append);
        argsBuffer.SetData(args);

        faceCount = surface.triangles.Length / 3;

        int kernel = compute.FindKernel("CSMain");
        compute.SetInt("_FaceCount", faceCount);
        compute.SetBuffer(kernel, "trianglesBuffer", trianglesBuffer);
        compute.SetBuffer(kernel, "verticesBuffer", verticesBuffer);
        compute.SetBuffer(kernel, "scatterBuffer", scatterBuffer);
    }
    void Update()
    {
        if(transformMat != sampleTarget.transform.localToWorldMatrix || Input.GetKeyDown(KeyCode.Space))
        {
            ScatterPoints();
        }
    }
    void OnDestroy()
    {
        trianglesBuffer.Release();
        verticesBuffer.Release();
        argsBuffer.Release();
        scatterBuffer.Release();
    }

    void ScatterPoints()
    {
        transformMat = sampleTarget.transform.localToWorldMatrix;

        scatterBuffer.SetCounterValue(0);

        _Seed = Random.value * 1000;
        compute.SetFloat("_Seed", _Seed);
        compute.SetFloat("_Density", _Density);
        compute.SetMatrix("_TransformMatrix", transformMat);

        compute.Dispatch(kernel, (faceCount / 640 + 1), 1, 1);

        resultArray = new Vector3[_MaxInstance];
        scatterBuffer.GetData(resultArray);

        ComputeBuffer.CopyCount(scatterBuffer, argsBuffer, sizeof(uint));
        argsBuffer.GetData(args);
    }

 

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < resultArray.Length; i++)
        {
            if (i >= args[1]) return;

            Vector3 item = resultArray[i];

            Gizmos.DrawWireCube(item, Vector3.one * 0.01f);
        }
    }


}
