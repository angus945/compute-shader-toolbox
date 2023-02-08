//using UnityEngine;

//public class ScatterByCompute : MonoBehaviour
//{
//    [SerializeField] GameObject sampleTarget;
//    Mesh surface { get => sampleTarget.GetComponent<MeshFilter>().mesh; }

//    [Space]
//    [SerializeField] ComputeShader compute;
//    [SerializeField] int _MaxInstance = 16 * 16;
//    [SerializeField] float _Density = 3;
//    [SerializeField] float _Seed = 3;
//    int kernel;

//    [Header("Instance")]
//    [SerializeField] ComputeShader computeTransform;
//    [SerializeField] Mesh instanceMesh;
//    [SerializeField] Material material;
//    [SerializeField][Range(0.001f, 0.5f)] float _UnitScale;
//    ComputeBuffer transformBuffer;
//    public Matrix4x4[] transformBufferCheck;
//    int kernelTrans;


//    [Space]
//    [SerializeField] Vector3[] resultArray;
//    public uint[] args;
//    int instanceCount { get => (int) args[1]; }

//    ComputeBuffer trianglesBuffer;
//    ComputeBuffer verticesBuffer;

//    ComputeBuffer argsBuffer;
//    ComputeBuffer scatterBuffer;
//    int faceCount;

//    Matrix4x4 transformMat;

//    //
//    ComputeBuffer instanceBuffer;

//    void Awake()
//    {
//        Application.targetFrameRate = 120;
//    }
//    void Start()
//    {
//        args = new uint[6]
//        {
//            (uint)instanceMesh.GetIndexCount(0),
//            (uint)_MaxInstance,
//            (uint)instanceMesh.GetIndexStart(0),
//            (uint)instanceMesh.GetBaseVertex(0),
//            0,
//            0,
//        };

//        trianglesBuffer = new ComputeBuffer(surface.triangles.Length, sizeof(int), ComputeBufferType.Structured);
//        verticesBuffer = new ComputeBuffer(surface.vertices.Length, sizeof(float) * 3, ComputeBufferType.Structured);

//        trianglesBuffer.SetData(surface.triangles);
//        verticesBuffer.SetData(surface.vertices);

//        argsBuffer = new ComputeBuffer(6, sizeof(uint), ComputeBufferType.IndirectArguments);
//        scatterBuffer = new ComputeBuffer(_MaxInstance, sizeof(float) * 4, ComputeBufferType.Append);
//        argsBuffer.SetData(args);

//        faceCount = surface.triangles.Length / 3;

//        //int kernel = compute.FindKernel("CSMain");
//        //compute.SetInt("_FaceCount", faceCount);
//        //compute.SetBuffer(kernel, "trianglesBuffer", trianglesBuffer);
//        //compute.SetBuffer(kernel, "verticesBuffer", verticesBuffer);
//        //compute.SetBuffer(kernel, "scatterBuffer", scatterBuffer);

//        //
//        transformBuffer = new ComputeBuffer(_MaxInstance, sizeof(float) * 4 * 4, ComputeBufferType.Structured);
//        kernelTrans = computeTransform.FindKernel("GetTransform");
//        computeTransform.SetBuffer(kernelTrans, "positionBuffer", scatterBuffer);
//        computeTransform.SetBuffer(kernelTrans, "transformBuffer", transformBuffer);
//        material.SetBuffer("transformBuffer", transformBuffer);
//    }

//    float t = 0;
//    void Update()
//    {
//        t += Time.deltaTime;

//        if (transformMat != sampleTarget.transform.localToWorldMatrix || Input.GetKeyDown(KeyCode.Space) || t >= 1)
//        {
//            ScatterPoints();

//            t = 0;
//        }

//        computeTransform.SetFloat("_UnitScale", _UnitScale);
//        computeTransform.Dispatch(kernelTrans, instanceCount / 640 + 1, 1, 1);
//        //transformBufferCheck = new Matrix4x4[_MaxInstance];
//        //transformBuffer.GetData(transformBufferCheck);

//        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100);
//        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, material, bounds, argsBuffer);
//    }
//    void OnDestroy()
//    {
//        trianglesBuffer.Release();
//        verticesBuffer.Release();
//        argsBuffer.Release();
//        scatterBuffer.Release();
//    }

//    void ScatterPoints()
//    {
//        transformMat = sampleTarget.transform.localToWorldMatrix;

//        scatterBuffer.SetCounterValue(0);

//        _Seed = Random.value * 1000;
//        ComputeScatter.Option option = new ComputeScatter.Option()
//        {
//            seed = _Seed,
//            density = _Density,
//            localToWorld = transformMat
//        };

//        scatterBuffer.SetCounterValue(0);

//        ComputeScatter.ScattingPoints(option, trianglesBuffer, verticesBuffer, scatterBuffer, null);
//        //compute.SetFloat("_Seed", _Seed);
//        //compute.SetFloat("_Density", _Density);
//        //compute.SetMatrix("_TransformMatrix", transformMat);

//        //compute.Dispatch(kernel, (faceCount / 640 + 1), 1, 1);

//        resultArray = new Vector3[_MaxInstance];
//        scatterBuffer.GetData(resultArray);

//        ComputeBuffer.CopyCount(scatterBuffer, argsBuffer, sizeof(uint));
//        argsBuffer.GetData(args);
//    }



//    void OnDrawGizmos()
//    {
//        Gizmos.color = Color.red;
//        for (int i = 0; i < resultArray.Length; i++)
//        {
//            if (i >= args[1]) return;

//            Vector3 item = resultArray[i];

//            Gizmos.DrawWireCube(item, Vector3.one * 0.01f);
//        }
//    }


//}
