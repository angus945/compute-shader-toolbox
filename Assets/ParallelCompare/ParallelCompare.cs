using UnityEngine;

public static class ParallelCompare
{

    // Compare And Copy min, max value to buffer index 0, 1.
    // this operator well destroy source content layout, please copy buffer before compare.
    // source [-4 , 47, 51, -71,  55, -72, -4, -2,  78, -77]
    // result [-77, 78, 51, -71, -72,  55, -4, -2, -7 ,  78]

    class Compare
    {
        string kernelName;
        string bufferName;

        public Compare(string kernelName, string bufferName)
        {
            this.kernelName = kernelName;
            this.bufferName = bufferName;
        }
        public void CompareElements(ComputeShader compute, ComputeBuffer compareBuffer)
        {
            int compareCount = compareBuffer.count;
            int kernel = compute.FindKernel(kernelName);

            compute.SetBuffer(kernel, bufferName, compareBuffer);

            int start = 1, offset = 1;
            while (compareCount > 1)
            {
                compareCount = Mathf.CeilToInt(compareCount / 4f);
                start = start * 4;

                compute.SetInt("_CompareCount", compareCount);
                compute.SetInt("_CompareStart", start);
                compute.SetInt("_CompareOffset", offset);
                compute.Dispatch(kernel, compareCount / numthread + 1, 1, 1);

                offset = offset * 4;
            }
        }
    }

    const string shaderPath = "CompareCompute";
    const int numthread = 8;

    static ComputeShader compute;
    static readonly Compare floatCompare = new Compare("Compare_Float", "compareBuffer_Float");

    static void Init()
    {
        if (compute != null) return;

        compute = Resources.Load<ComputeShader>(shaderPath);
    }
    public static void CompareElements(ComputeBuffer compareBuffer)
    {
        Init();

        floatCompare.CompareElements(compute, compareBuffer);
    }
}
