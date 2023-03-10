using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Scatter Data", menuName = "ComputeShaderToolbox/Scatter Data")]
public class ScatterData : ScriptableObject
{
    [SerializeField] int _maxScattered;
    [SerializeField] int _seed = 3;
    [SerializeField] [Min(0.01f)] float _density;
    [SerializeField] [Range(0,1)] float _noising;
    public int maxScattered { get => _maxScattered; }
    public int seed { get => _seed; }
    public float density { get => _density; }
    public float noising { get => _noising; }

    [Header("Transform")]
    [SerializeField] bool _alignDirection;
    [SerializeField] Vector3 _baseOffset;
    [SerializeField] Vector3 _baseRotate;
    [SerializeField] Vector3 _baseSize;
    [SerializeField] Vector3 _baseExtrude;
    public bool alignDirection { get => _alignDirection; }
    public Vector3 baseOffset { get => _baseOffset; }
    public Vector3 baseRotate { get => _baseRotate; }
    public Vector3 baseSize { get => _baseSize; }
    public Vector3 baseExtrude { get => _baseExtrude; }

    [Header("Randomize")]
    [SerializeField] Vector3 _rndOffset;
    [SerializeField] Vector3 _rndRotate;
    [SerializeField] Vector3 _rndScale;
    [SerializeField] Vector3 _rndExtrude;
    public Vector3 rndOffset { get => _rndOffset; }
    public Vector3 rndRotate { get => _rndRotate; }
    public Vector3 rndScale { get => _rndScale; }
    public Vector3 rndExtrude { get => _rndExtrude; }

    [Space]
    [SerializeField] PointCloudFilter[] _filter;
    public PointCloudFilter[] filter { get => _filter; }

}
