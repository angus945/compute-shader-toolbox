using UnityEngine;

public class ScatterBuffer
{
    // float4 x y z random
    public readonly int length;
    public int count { get; private set; }

    uint[] args;
    public ComputeBuffer argsBuffer;
    public ComputeBuffer scatteredBuffer;
    public ComputeBuffer directionBuffer;
    public ComputeBuffer randomizeBuffer;

    public ScatterBuffer(int maxScattered)
    {
        length = maxScattered;

        args = new uint[1];

        argsBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.IndirectArguments);
        scatteredBuffer = new ComputeBuffer(maxScattered, sizeof(float) * 3, ComputeBufferType.Append);
        directionBuffer = new ComputeBuffer(maxScattered, sizeof(float) * 3, ComputeBufferType.Append);
        randomizeBuffer = new ComputeBuffer(maxScattered, sizeof(float) * 3, ComputeBufferType.Append);
    }
    public void ResetCounter()
    {
        scatteredBuffer.SetCounterValue(0);
        directionBuffer.SetCounterValue(0);
        randomizeBuffer.SetCounterValue(0);
    }
    public void GetDatas(ref Vector3[] pointCloud, ref Vector3[] direcitons, ref Vector3[] randomize)
    {
        scatteredBuffer.GetData(pointCloud);
        directionBuffer.GetData(direcitons);
        randomizeBuffer.GetData(randomize);
    }
    public void CopyCount()
    {
        ComputeBuffer.CopyCount(scatteredBuffer, argsBuffer, 0);
        argsBuffer.GetData(args);

        count = (int)args[0];
    }
    public void Release()
    {
        argsBuffer.Release();
        scatteredBuffer.Release();
        directionBuffer.Release();
        randomizeBuffer.Release();
    }
}
