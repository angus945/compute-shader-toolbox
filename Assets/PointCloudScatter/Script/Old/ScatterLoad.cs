using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScatterLoad : MonoBehaviour
{
    [SerializeField] ScatterBaker scatterData;
    [SerializeField] Texture2D texture;

    public Vector3[] scatters;

    ComputeBuffer buffer;

    void Start()
    {
        buffer = new ComputeBuffer(scatterData.instanceCount, sizeof(float) * 3, ComputeBufferType.Structured);
        scatters = new Vector3[scatterData.instanceCount];

        ScatterEncod.ReadRenderTexture(scatterData.boundMin, scatterData.boundMax, texture, buffer);
        buffer.GetData(scatters);
    }
    void OnDrawGizmos()
    {
        foreach (var item in scatters)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(item, Vector3.one * 0.01f);
        }
    }

}
