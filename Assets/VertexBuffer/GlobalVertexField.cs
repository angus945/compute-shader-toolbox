using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVertexField : MonoBehaviour
{
    public Transform cullingSphere;
    public bool cullIn;

    void Update()
    {
        Vector4 sphereSDF = cullingSphere.position;
        sphereSDF.w = cullingSphere.localScale.x / 2;

        Shader.SetGlobalVector("_SDFSphere", sphereSDF);
        Shader.SetGlobalFloat("_SDFDirection", cullIn ? 1 : -1);
    }
}
