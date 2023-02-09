using UnityEngine;

public class ScatterBuffer
{
    // float4 x y z random
    public readonly int length;
    public int count { get; private set; }

    uint[] args;
    public ComputeBuffer argsBuffer;
    public ComputeBuffer pointCloudBuffer;

    public ScatterBuffer(int maxScattered)
    {
        length = maxScattered;

        args = new uint[1];

        argsBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.IndirectArguments);
        pointCloudBuffer = new ComputeBuffer(maxScattered, ScatterPoint.Size, ComputeBufferType.Append);
    }
    public void ResetCounter()
    {
        pointCloudBuffer.SetCounterValue(0);
    }
    public void GetDatas(ref ScatterPoint[] pointCloud)
    {
        pointCloudBuffer.GetData(pointCloud);
    }
    public void CopyCount()
    {
        ComputeBuffer.CopyCount(pointCloudBuffer, argsBuffer, 0);
        argsBuffer.GetData(args);

        count = (int)args[0];
    }
    public void Release()
    {
        argsBuffer.Release();
        pointCloudBuffer.Release();
    }
}
