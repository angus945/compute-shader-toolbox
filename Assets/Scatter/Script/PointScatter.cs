using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurfaceScatter
{
    public enum SampleType
    {
        Mesh,
        Object,
    }

    [System.Serializable]
    public struct PointData
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
            this.normal = matrix.MultiplyPoint(this.normal).normalized;

            return this;
        }
        public Matrix4x4 GetTransformMatrix()
        {
            return Matrix4x4.TRS(pos, Quaternion.LookRotation(normal), Vector3.one);
        }

        public void OffsetByNormal(float expand)
        {
            this.pos += normal * expand;
        }
    }

    [System.Serializable]
    public struct TriangleData
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
            float width = Mathf.Abs(Vector3.Distance(vertexA.pos, vertexB.pos));
            float height = Mathf.Abs(Vector3.Dot(vertexA.pos - vertexC.pos, upward));
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

            Vector3 abNormalShift = Vector3.Lerp(Vector3.zero, vertexB.normal - vertexA.normal, acValue);
            Vector3 acNormalShift = Vector3.Lerp(Vector3.zero, vertexC.normal - vertexA.normal, bcValue);
            Vector3 normal = Vector3.Normalize(vertexA.normal + abNormalShift + acNormalShift);

            return new PointData(position, normal, Vector2.zero);
        }
    }

    [System.Serializable]
    public struct ScatterSource
    {
        public SampleType type;
        public Mesh mesh;
        public GameObject gameObject;
    }

    [System.Serializable]
    public struct ScatterOption
    {
        public float density;
        public float expand;
        public bool fullRandom;
    }

    [System.Serializable]
    public struct PreviewOptions
    {
        public bool displayWireframe;
        public bool displayWireframeNormal;

        public bool displayPointCloud;
        public float pointSize;

        public bool displayNormal;
        public float normalLength;
    }

    [System.Serializable]
    public class PointScatter : MonoBehaviour
    {
        public ScatterSource source;
        public ScatterOption option;
        public PreviewOptions preview;

        public TriangleData[] meshTriangles;
        public PointData[] pointCloud;
        public Matrix4x4[] pointCloudMatrics;
        public Vector4[] pointCloudDirectionals;

        //public Vector

        public void LoadMeshTriangles()
        {
            switch (source.type)
            {
                case SampleType.Mesh:
                    LoadMeshTriangles(source.mesh, Matrix4x4.identity);
                    break;

                case SampleType.Object:
                    Mesh mesh = source.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Matrix4x4 matrix = source.gameObject.transform.localToWorldMatrix;
                    LoadMeshTriangles(mesh, matrix);
                    break;
            }
        }
        public void LoadMeshTriangles(Mesh meshData, Matrix4x4 matrix)
        {
            List<TriangleData> faces = new List<TriangleData>();

            int[] triangles = meshData.triangles;
            Vector3[] vertices = meshData.vertices;
            Vector3[] normals = meshData.normals;
            Vector2[] uvs = meshData.uv;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                PointData vertA = GetVertex(i + 0, matrix);
                PointData vertB = GetVertex(i + 1, matrix);
                PointData vertC = GetVertex(i + 2, matrix);

                Vector3 normal = Vector3.Cross(vertB.pos - vertA.pos, vertC.pos - vertA.pos).normalized;
                Vector3 upward = Vector3.Cross(vertA.pos - vertB.pos, normal).normalized;

                TriangleData face = new TriangleData(vertA, vertB, vertC, normal, upward);

                faces.Add(face);
            }
            PointData GetVertex(int index, Matrix4x4 matrix)
            {
                int triangleIndex = triangles[index];
                PointData point = new PointData(vertices[triangleIndex], normals[triangleIndex], Vector2.zero);

                point = point.ApplyMatrix((Matrix4x4)matrix);

                return point;
            }

            meshTriangles = faces.ToArray();
            //return faces.ToArray();
        }
        public void ScatterOnSurface()
        {
            List<PointData> scatterPoints = new List<PointData>();
            List<Matrix4x4> transforms = new List<Matrix4x4>();
            List<Vector4> directionals = new List<Vector4>();

            for (int faceIndex = 0; faceIndex < meshTriangles.Length; faceIndex++)
            {
                TriangleData face = meshTriangles[faceIndex];

                float surface = face.FaceArea();

                int scatterCount = Mathf.CeilToInt(surface * option.density);
                float sqrtCount = Mathf.Sqrt(scatterCount);

                for (int scatterIndex = 0; scatterIndex < scatterCount; scatterIndex++)
                {
                    float acMapping, bcMapping;

                    if (option.fullRandom)
                    {
                        acMapping = UnityEngine.Random.value;
                        bcMapping = UnityEngine.Random.value;
                    }
                    else
                    {
                        bcMapping = (scatterIndex / sqrtCount) / sqrtCount;
                        acMapping = (scatterIndex % sqrtCount) / sqrtCount;
                    }

                    PointData scatterPoint = face.LerpOnTriangle(acMapping, bcMapping);
                    scatterPoint.OffsetByNormal(option.expand);

                    scatterPoints.Add(scatterPoint);
                    transforms.Add(scatterPoint.GetTransformMatrix());
                    directionals.Add(scatterPoint.normal);
                }
            }

            pointCloud = scatterPoints.ToArray();
            pointCloudMatrics = transforms.ToArray();
            pointCloudDirectionals = directionals.ToArray();
        }     
    }
}

