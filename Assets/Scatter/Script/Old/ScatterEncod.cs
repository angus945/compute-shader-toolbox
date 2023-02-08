using UnityEngine;

public static class ScatterEncod
{
    const string shaderPath = "ScatterEncodeCompute";
    const int numthread = 8;

    const string encodeKernel = "EncodeToRenderTexture";
    const string loadKernel = "LoadFromRenderTexture";

    static ComputeShader compute;

    static void Init()
    {
        if (compute != null) return;

        compute = Resources.Load<ComputeShader>(shaderPath);
    }
    public static void WriteToRenderTexture(Vector3 boundMin, Vector3 boundMax, ComputeBuffer scatterBuffer, RenderTexture scatterTexture)
    {
        Init();

        int textureSize = scatterTexture.width;

        int kernel = compute.FindKernel(encodeKernel);
        compute.SetInt("_TextureSize", textureSize);
        compute.SetVector("_BoundMin", boundMin);
        compute.SetVector("_BoundMax", boundMax);
        compute.SetBuffer(kernel, "scatterBuffer", scatterBuffer);
        compute.SetTexture(kernel, "scatterRenderTexture", scatterTexture);

        compute.Dispatch(kernel, textureSize / numthread + 1, textureSize / numthread + 1, 1);
    }
    public static void ReadRenderTexture(Vector3 boundMin, Vector3 boundMax, Texture2D scatterTexture, ComputeBuffer scatterBuffer)
    {
        Init();

        int textureSize = scatterTexture.width;

        int kernel = compute.FindKernel(loadKernel);
        compute.SetInt("_TextureSize", textureSize);
        compute.SetVector("_BoundMin", boundMin);
        compute.SetVector("_BoundMax", boundMax);
        compute.SetTexture(kernel, "scatterTexture", scatterTexture);
        compute.SetBuffer(kernel, "scatterBuffer", scatterBuffer);

        compute.Dispatch(kernel, textureSize / numthread + 1, textureSize / numthread + 1, 1);
    }

}
