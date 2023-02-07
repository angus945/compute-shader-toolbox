using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Scatter Data", menuName = "ComputeShaderToolbox/Scatter")]
public class ScatterData : ScriptableObject
{
    public enum MaxScatter
    {
        _16 = 4 * 4,
        _64 = 8 * 8,
        _256 = 16 * 16,
        _1024 = 32 * 32,
        _4096 = 64 * 64,
        _16384 = 128 * 128,
        //_65539 = 256 * 256,
        //_262144 = 512 * 512,
        //_1048576 = 1024 * 1024,
    }

    public Mesh sampleMesh;
    public Vector3 position = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    public Vector3 localScale = Vector3.one;

    public Quaternion quaternion 
    { 
        get => Quaternion.Euler(rotation);
        set => rotation = value.eulerAngles;
    }
    public Matrix4x4 localToWorldMatrix { get => Matrix4x4.TRS(position, quaternion, localScale); }

    [Space]
    public MaxScatter maxInstance = MaxScatter._4096;
    public float density = 3;
    public float seed = 3;

    [Header("Result")]
    public int instanceCount;
    public Vector3 boundMin;
    public Vector3 boundMax;

    [Header("Debug")]
    public bool showWireframe;
    public bool showNormal;
    public bool showPointCloud;
    public bool showBound;
}
