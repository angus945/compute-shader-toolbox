using UnityEngine;

public static class CopyElements
{
    const string shaderPath = "CopyCompute";
    static ComputeShader compute;

    static void Init()
    {
        if (compute == null)
        {
            compute = Resources.Load<ComputeShader>(shaderPath);
        }
    }

    //public void CopyBuffer()
}
