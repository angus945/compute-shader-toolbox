using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Scatter Data", menuName = "ComputeShaderToolbox/Scatter Data")]
public class ScatterData : ScriptableObject
{
    [SerializeField] int _maxScattered;
    [SerializeField] int _seed = 3;
    [SerializeField] float _density;
    public int maxScattered { get => _maxScattered; }
    public int seed { get => _seed; }
    public float density { get => _density; }

    [SerializeField] PointCloudFilter[] _filter;
    public PointCloudFilter[] filter { get => _filter; }

}
