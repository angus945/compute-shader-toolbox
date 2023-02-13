using System;
using UnityEngine;

public static class ScatterVisualizer
{
    [System.Serializable]
    public struct Options
    {
        public bool wireframe;
        public bool normal;
        public bool pointCloud;
        public bool pointCloudDirection;
    }

    readonly static Color wireframeColor = Color.green;
    readonly static Color normalColor = Color.blue;
    readonly static Color pointCloudColor = Color.red;

    public static void DrawSampleTraget(MeshFilter[] sampleTargets, Options options)
    {
        for (int i = 0; i < sampleTargets.Length; i++)
        {
            DrawSampleTraget(sampleTargets[i], options);
        }
    }
    public static void DrawSampleTraget(MeshFilter sampleTarget, Options options)
    {
        Mesh mesh = sampleTarget.sharedMesh;
        Matrix4x4 localToWorld = sampleTarget.transform.localToWorldMatrix;

        DrawSampleTraget(mesh, localToWorld, options);
    }

    public static void DrawSampleTraget(Mesh mesh, Matrix4x4 matrix, Options options)
    {
        if (mesh == null) return;

        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int triangle_0 = triangles[i + 0];
            int triangle_1 = triangles[i + 1];
            int triangle_2 = triangles[i + 2];

            Vector3 vertex_0 = matrix.MultiplyPoint(vertices[triangle_0]);
            Vector3 vertex_1 = matrix.MultiplyPoint(vertices[triangle_1]);
            Vector3 vertex_2 = matrix.MultiplyPoint(vertices[triangle_2]);

            Vector3 normal_0 = matrix.MultiplyVector(normals[triangle_0]).normalized;
            Vector3 normal_1 = matrix.MultiplyVector(normals[triangle_1]).normalized;
            Vector3 normal_2 = matrix.MultiplyVector(normals[triangle_2]).normalized;

            if (options.wireframe)
            {
                Gizmos.color = wireframeColor;
                Gizmos.DrawLine(vertex_0, vertex_1);
                Gizmos.DrawLine(vertex_1, vertex_2);
                Gizmos.DrawLine(vertex_2, vertex_0);
            }

            if (options.normal)
            {
                Gizmos.color = normalColor;
                Gizmos.DrawLine(vertex_0, vertex_0 + normal_0);
                Gizmos.DrawLine(vertex_1, vertex_1 + normal_1);
                Gizmos.DrawLine(vertex_2, vertex_2 + normal_2);
            }
        }

        //if (scatter.showBound)
        //{
        //    Vector3 center = (scatter.boundMin + scatter.boundMax) / 2;
        //    Vector3 size = (scatter.boundMax - scatter.boundMin);

        //    Handles.color = Color.gray;
        //    Handles.DrawWireCube(center, size);
        //}


    }

    public static void DrawPointCloud(ScatterPoint[] pointCloud, int instanceCount, Options options)
    {
        int count = Mathf.Min(pointCloud.Length, instanceCount);

        for (int i = 0; i < count; i++)
        {
            Vector3 point = pointCloud[i].transform.GetPosition();
            Vector3 normal = pointCloud[i].direction;
            Vector3 random = pointCloud[i].randomize;
            //Quaternion.eu

            if (options.pointCloud)
            {
                //Gizmos.color = new Color(random.x, random.y, random.z, 1);
                //Gizmos.DrawCube(point, Vector3.one * 0.05f);
                Gizmos.color = Color.red;
                Gizmos.DrawCube(point, Vector3.one * 0.05f);
            }
            if(options.pointCloudDirection)
            {
                Gizmos.color = normalColor;
                Gizmos.DrawRay(point, normal * 0.1f);
            }

            //Vector3 direction = pointCloud[i].transform.MultiplyVector(Vector3.up);
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawRay(point, direction * 0.1f);
        }
    }
}
