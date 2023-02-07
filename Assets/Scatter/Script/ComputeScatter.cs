using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComputeScatter
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

    static ComputeShader compute;

    static void Init()
    {
        if (compute != null) return;

        compute = Resources.Load<ComputeShader>(shaderPath);
    }
    public static void ScattingPoints(Option option, ComputeBuffer trianglesBuffer, ComputeBuffer verticesBuffer, ComputeBuffer scatterBuffer, ComputeBuffer normalsBuffer)
    {
        Init();

        int faceCount = trianglesBuffer.count / 3;
        int kernel = compute.FindKernel(kernelName);

        scatterBuffer.SetCounterValue(0);

        compute.SetInt("_FaceCount", faceCount);
        compute.SetBuffer(kernel, "trianglesBuffer", trianglesBuffer);
        compute.SetBuffer(kernel, "verticesBuffer", verticesBuffer);
        compute.SetBuffer(kernel, "scatterBuffer", scatterBuffer);

        compute.SetFloat("_Seed", option.seed);
        compute.SetFloat("_Density", option.density);
        compute.SetMatrix("_TransformMatrix", option.localToWorld);

        compute.Dispatch(kernel, (faceCount / numthread + 1), 1, 1);
    }
}
