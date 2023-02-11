using System;
using System.Collections.Generic;
using UnityEngine;
using SurfaceScatter;

[ExecuteInEditMode]
public class ScatterTest : MonoBehaviour
{
    [System.Serializable]
    struct PointData
    {
        public Vector3 pos;
        public Vector3 normal;
        public Vector2 uv;

        public PointData(Vector3 point, Vector3 normal, Vector2 uv)
        {
            this.pos = point;
            this.normal = normal;
            this.uv = uv;
        }
        public PointData ApplyMatrix(Matrix4x4 matrix)
        {
            this.pos = matrix.MultiplyPoint(this.pos);
            this.normal = matrix.MultiplyPoint(this.pos).normalized;

            return this;
        }
    }

    [System.Serializable]
    struct TriangleData
    {
        public PointData vertexA;
        public PointData vertexB;
        public PointData vertexC;

        public Vector3 normal;
        public Vector3 upward;

        public TriangleData(PointData vertexA, PointData vertexB, PointData vertexC, Vector3 normal, Vector3 upward)
        {
            this.vertexA = vertexA;
            this.vertexB = vertexB;
            this.vertexC = vertexC;
            this.normal = normal;
            this.upward = upward;
        }
        public float FaceArea()
        {
            float width = MathF.Abs(Vector3.Distance(vertexA.pos, vertexB.pos));
            float height = MathF.Abs(Vector3.Dot(vertexA.pos - vertexC.pos, upward));
            return (height * width) / 2;
        }
        public PointData LerpOnTriangle(float acValue, float bcValue)
        {
            //Vector3 rndBottom = Vector3.Lerp(a, b, UnityEngine.Random.value);
            //return Vector3.Lerp(rndBottom, c, UnityEngine.Random.value);

            if (acValue + bcValue > 1)
            {
                acValue = (1 - acValue);
                bcValue = (1 - bcValue);
            }

            Vector3 abShift = Vector3.Lerp(Vector3.zero, vertexB.pos - vertexA.pos, acValue);
            Vector3 acShift = Vector3.Lerp(Vector3.zero, vertexC.pos - vertexA.pos, bcValue);
            Vector3 position = vertexA.pos + abShift + acShift;

            return new PointData(position, Vector3.zero, Vector2.zero);
        }
    }

    [Header("Scatter")]
    public Mesh surfaceMesh;
    public int density;
    public bool randomScatter;
    public bool applyTransform;

    [Header("Debug")]
    [Range(0, 0.3f)] public float pointSize = 1;
    public bool wireframe;

    //[Header("Process")]
    [SerializeField] PointScatter scatter;

    void OnEnable()
    {
        scatter.LoadMeshTriangles();
        scatter.ScatterOnSurface();
    }

    //void OnDrawGizmos()
    //{
    //    DrawWireframe();
    //    DrawScatterPoints();
    //}
    //void DrawWireframe()
    //{
    //    if (Application.isPlaying) return;

    //    scatter.LoadMeshTriangles();

    //    for (int i = 0; i < processingFaces.Length; i++)
    //    {
    //        TriangleData face = processingFaces[i];
    //        Gizmos.DrawLine(face.vertexA.pos, face.vertexB.pos);
    //        Gizmos.DrawLine(face.vertexB.pos, face.vertexC.pos);
    //        Gizmos.DrawLine(face.vertexC.pos, face.vertexA.pos);
    //    }
    //}
    //void DrawScatterPoints()
    //{
    //    if (scatterPoints == null) return;

    //    Matrix4x4 transformMatrix = transform.localToWorldMatrix;

    //    Gizmos.color = Color.blue;
    //    for (int i = 0; i < scatterPoints.Length; i++)
    //    {
    //        Vector3 position = transformMatrix.MultiplyPoint(scatterPoints[i].pos);
    //        Gizmos.DrawWireSphere(position, pointSize);
    //    }
    //}

}
