using System;
using System.Collections.Generic;
using UnityEngine;

public class ScatterTest : MonoBehaviour
{

    [Header("Scatter")]
    public Mesh surface;
    public int scatterByCount;
    public int scatterBySurface;
    public bool randomScatter;

    [Header("Debug")]
    [Range(0, 0.3f)] public float pointSize = 1;
    public bool wireframe;

    List<Vector3> scatterPoints;

    void Start()
    {
        scatterPoints = new List<Vector3>();

        scatterPoints.Clear();

        //ScatterByCount();
        ScatterByDensity();
    }

    int ScatterByCount()
    {
        ForeachFace((int index, Vector3 a, Vector3 b, Vector3 c, Vector3 n, Vector3 u) =>
        {
            for (int i = 0; i < scatterByCount; i++)
            {
                scatterPoints.Add(MapToTriangle(a, b, c, UnityEngine.Random.value, UnityEngine.Random.value));
            }
        });

        return scatterByCount;
    }
    int ScatterByDensity()
    {
        int totalCount = 0;

        ForeachFace((int index, Vector3 a, Vector3 b, Vector3 c, Vector3 normal, Vector3 up) =>
        {
            float surface = CalculateSufrace(a, b, c, normal, up);

            int count = Mathf.CeilToInt(surface * scatterBySurface);

            float width = Mathf.Sqrt(count);

            for (int i = 0; i < count; i++)
            {
                float ab, ac;

                if (randomScatter)
                {
                    ab = UnityEngine.Random.value;
                    ac = UnityEngine.Random.value;
                }
                else
                {
                    ab = (i / width) / width;
                    ac = (i % width) / width;
                }

                scatterPoints.Add(MapToTriangle(a, b, c, ab, ac));
            }


            totalCount += count;
        });

        return totalCount;
    }

    void OnDrawGizmos()
    {
        if (wireframe)
        {
            ForeachFace((int i, Vector3 a, Vector3 b, Vector3 c, Vector3 n, Vector3 u) =>
            {
                Gizmos.DrawLine(a, b);
                Gizmos.DrawLine(b, c);
                Gizmos.DrawLine(c, a);
            });
        }

        if (scatterPoints == null) return;
        Gizmos.color = Color.blue;
        for (int i = 0; i < scatterPoints.Count; i++)
        {
            Gizmos.DrawWireSphere(scatterPoints[i], pointSize);
        }
    }

    delegate void FaceHandler(int index, Vector3 a, Vector3 b, Vector3 c, Vector3 normal, Vector3 up);
    void ForeachFace(FaceHandler triangleHandler)
    {
        Vector3[] vertices = surface.vertices;
        int[] triangles = surface.triangles;

        Matrix4x4 transformMatrix = transform.localToWorldMatrix;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 a = transformMatrix.MultiplyPoint(vertices[triangles[i + 0]]);
            Vector3 b = transformMatrix.MultiplyPoint(vertices[triangles[i + 1]]);
            Vector3 c = transformMatrix.MultiplyPoint(vertices[triangles[i + 2]]);

            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            Vector3 upward = Vector3.Cross(a - b, normal).normalized;

            triangleHandler.Invoke(i / 3, a, b, c, normal, upward);
        }
    }

    float CalculateSufrace(Vector3 a, Vector3 b, Vector3 c, Vector3 normal, Vector3 up)
    {
        //Debug.DrawRay(a, normal, Color.blue);
        //Debug.DrawRay(a, upward, Color.green);

        float width = MathF.Abs(Vector3.Distance(a, b));
        float height = MathF.Abs(Vector3.Dot(a - c, up));
        return (height * width) / 2;

        //Debug.DrawRay(c, upward.normalized * -height, Color.red);
    }
    Vector3 MapToTriangle(Vector3 a, Vector3 b, Vector3 c, float abValue, float acValue)
    {
        //Vector3 rndBottom = Vector3.Lerp(a, b, UnityEngine.Random.value);
        //return Vector3.Lerp(rndBottom, c, UnityEngine.Random.value);

        if (abValue + acValue > 1)
        {
            abValue = (1 - abValue);
            acValue = (1 - acValue);
        }

        Vector3 abShift = Vector3.Lerp(Vector3.zero, b - a, abValue);
        Vector3 acShift = Vector3.Lerp(Vector3.zero, c - a, acValue);
        return a + abShift + acShift;
    }


}
