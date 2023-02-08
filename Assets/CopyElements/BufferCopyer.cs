using UnityEngine;

public static class BufferCopyer
{

    class Copyer
    {
        string kernelName;
        string srcBuffer;
        string dstBuffer;

        public Copyer(string kernelName, string srcBuffer, string dstBuffer)
        {
            this.kernelName = kernelName;
            this.srcBuffer = srcBuffer;
            this.dstBuffer = dstBuffer;
        }
        public void CopyBuffer(ComputeShader compute, ComputeBuffer src, ComputeBuffer dst)
        {
            Init();

            int kernel = compute.FindKernel(kernelName);
            compute.SetBuffer(kernel, srcBuffer, src);
            compute.SetBuffer(kernel, dstBuffer, dst);
            compute.Dispatch(kernel, src.count / numthread + 1, 1, 1);
        }
    }

    const string shaderPath = "CopyCompute";
    const int numthread = 640;

    static ComputeShader compute;
    readonly static Copyer copyer_int = new Copyer("CopyBuffer_int", "sourceBuffer_int", "destinationBuffer_int");
    readonly static Copyer copyer_float = new Copyer("CopyBuffer_float", "sourceBuffer_float", "destinationBuffer_float");
    readonly static Copyer copyer_float3 = new Copyer("CopyBuffer_float3", "sourceBuffer_float3", "destinationBuffer_float3");

    static void Init()
    {
        if (compute != null) return;

        compute = Resources.Load<ComputeShader>(shaderPath);
    }

    public static void Copy_Integer(ComputeBuffer src, ComputeBuffer dst)
    {
        Init();

        copyer_int.CopyBuffer(compute, src, dst);
    }
    public static void Copy_Float(ComputeBuffer src, ComputeBuffer dst)
    {
        Init();

        copyer_float.CopyBuffer(compute, src, dst);
    }
    public static void Copy_Float3(ComputeBuffer src, ComputeBuffer dst)
    {
        Init();

        copyer_float3.CopyBuffer(compute, src, dst);
    }
}
