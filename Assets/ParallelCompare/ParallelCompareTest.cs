using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallelCompareTest : MonoBehaviour
{

    [SerializeField] ComputeShader compute;
    [SerializeField] int count;
    [SerializeField] float[] datas;
    [SerializeField] float[] result;

    ComputeBuffer compareBuffer;
    //ComputeBuffer compareBuffer;

    public float min = 0, max = 0;

    void Start()
    {
        compareBuffer = new ComputeBuffer(count, sizeof(float), ComputeBufferType.Structured);

        datas = new float[count];
        result = new float[count];
        for (int i = 0; i < count; i++)
        {
            datas[i] = UnityEngine.Random.Range(-100f, 100f);
            min = Mathf.Min(min, datas[i]);
            max = Mathf.Max(max, datas[i]);
        }

        compareBuffer.SetData(datas);

        int kernel = compute.FindKernel("CSMain");
        //compute.SetBuffer(kernel, "sourceBuffer", sourceBuffer);
        compute.SetBuffer(kernel, "compareBuffer", compareBuffer);

        int start = 1, offset = 1;
        int compareCount = 0;
        while (count > 1)
        {
            count = count / 4;
            start = start * 4;

            compute.SetInt("_CompareCount", count);
            compute.SetInt("_CompareStart", start);
            compute.SetInt("_CompareOffset", offset);
            compute.Dispatch(kernel, count / 4 + 1, 1, 1);

            offset = offset * 4;
            compareCount++;
            Debug.Log(count);
        }

        Debug.Log(compareCount);
        compareBuffer.GetData(result);
    }
    void OnDestroy()
    {
        //sourceBuffer.Release();
        //compareBuffer.Release();
    }

    void CompareByCSharp()
    {
        Array.Copy(datas, result, count);

        int start = 1, offset = 1;
        while (count > 1)
        {
            count = count / 4;
            start = start * 4;

            for (int i = 0; i < count; i++)
            {
                int compareIndex = i * start;
                float minValue_0 = result[compareIndex + offset * 0];
                float minValue_1 = result[compareIndex + offset * 1];
                float minValue_2 = result[compareIndex + offset * 2];
                float minValue_3 = result[compareIndex + offset * 3];

                int checkOffset = offset == 1 ? 0 : 1;
                float maxValue_0 = result[compareIndex + offset * 0 + checkOffset];
                float maxValue_1 = result[compareIndex + offset * 1 + checkOffset];
                float maxValue_2 = result[compareIndex + offset * 2 + checkOffset];
                float maxValue_3 = result[compareIndex + offset * 3 + checkOffset];

                //
                float min = UnityEngine.Mathf.Min(minValue_0, minValue_1, minValue_2, minValue_3);
                result[compareIndex] = min;

                float max = UnityEngine.Mathf.Max(maxValue_0, maxValue_1, maxValue_2, maxValue_3);
                result[compareIndex + 1] = max;

                //Debug.Log(compareIndex);
                //Debug.LogError(min);
            }

            offset = offset * 4;
            Debug.Log("-------------------------" + count);
        }
    }
}
