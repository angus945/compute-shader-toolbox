using System;
using UnityEditor;
using UnityEngine;

namespace SurfaceScatter
{
    [CustomEditor(typeof(ScatterData))]
    public class ScatterDataEditor : Editor
    {
        ScatterData scatter;
        Action onSampleChanged;
        Action onOptionChanged;

        ComputeBuffer trianglesBuffer;
        ComputeBuffer verticesBuffer;

        ComputeBuffer scatterBuffer;
        ComputeBuffer boundBuffer;
        ComputeBuffer argsBuffer;

        RenderTexture bakeTexture;

        void OnEnable()
        {
            scatter = (ScatterData)target;

            SceneView.duringSceneGui += OnSceneGUI;

            onSampleChanged += DisposeBuffer;
            onSampleChanged += CreateDataTemporary;
            onSampleChanged += CreateBuffer;
            onSampleChanged += CreateTexture;
            onSampleChanged?.Invoke();

            onOptionChanged += GeneratePointCloud;
            onOptionChanged += CalculateBound;
            onOptionChanged += BakeTexture;
        }
        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;

            DisposeBuffer();
        }
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck() || GUILayout.Button("Generate"))
            {
                onOptionChanged?.Invoke();
            }

            if (GUILayout.Button("UpdateMesh"))
            {
                onSampleChanged?.Invoke();
            }
            GUILayout.Label($"count: {scatter.instanceCount}");

            if (GUILayout.Button("SaveTexture"))
            {
                SaveRenderTexture(bakeTexture, $"{Application.dataPath}/texture.png");
            }

        }

        Vector3[] pointCloud;
        uint[] args;
        void CreateDataTemporary()
        {
            pointCloud = new Vector3[(int)scatter.maxInstance];
            args = new uint[1];
        }
        void CreateBuffer()
        {
            trianglesBuffer = new ComputeBuffer(scatter.sampleMesh.triangles.Length, sizeof(int), ComputeBufferType.Structured);
            verticesBuffer = new ComputeBuffer(scatter.sampleMesh.vertices.Length, sizeof(float) * 3, ComputeBufferType.Structured);

            scatterBuffer = new ComputeBuffer((int)scatter.maxInstance, sizeof(float) * 3, ComputeBufferType.Append);
            boundBuffer = new ComputeBuffer((int)scatter.maxInstance, sizeof(float) * 3, ComputeBufferType.Structured);

            argsBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.IndirectArguments);

            trianglesBuffer.SetData(scatter.sampleMesh.triangles);
            verticesBuffer.SetData(scatter.sampleMesh.vertices);
        }
        void CreateTexture()
        {
            int size = (int)Mathf.Sqrt((int)scatter.maxInstance);

            bakeTexture = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32);
            bakeTexture.filterMode = FilterMode.Point;
            bakeTexture.enableRandomWrite = true;
        }
        void DisposeBuffer()
        {
            if (trianglesBuffer != null) trianglesBuffer.Dispose();
            if (verticesBuffer != null) verticesBuffer.Dispose();

            if (scatterBuffer != null) scatterBuffer.Dispose();
            if (boundBuffer != null) boundBuffer.Dispose();

            if (argsBuffer != null) argsBuffer.Dispose();

            if (bakeTexture != null) bakeTexture.Release();
        }

        void GeneratePointCloud()
        {
            //scatter.seed = UnityEngine.Random.value * 1000;
            ComputeScatter.Option option = new ComputeScatter.Option()
            {
                seed = scatter.seed,
                density = scatter.density,
                localToWorld = scatter.localToWorldMatrix,
            };
            ComputeScatter.ScattingPoints(option, trianglesBuffer, verticesBuffer, scatterBuffer, null);

            ComputeBuffer.CopyCount(scatterBuffer, argsBuffer, 0);
            scatterBuffer.GetData(pointCloud);
            argsBuffer.GetData(args);

            scatter.instanceCount = Mathf.Min((int)args[0], (int)scatter.maxInstance);
        }
        void CalculateBound()
        {
            int[] triangles = scatter.sampleMesh.triangles;
            Vector3[] vertices = scatter.sampleMesh.vertices;

            Matrix4x4 transformMat = scatter.localToWorldMatrix;

            Vector3 boundMin = Vector3.one * +10000; 
            Vector3 boundMax = Vector3.one * -10000; 

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int triangle_0 = triangles[i + 0];
                int triangle_1 = triangles[i + 1];
                int triangle_2 = triangles[i + 2];

                Vector3 vertex_0 = transformMat.MultiplyPoint(vertices[triangle_0]);
                Vector3 vertex_1 = transformMat.MultiplyPoint(vertices[triangle_1]);
                Vector3 vertex_2 = transformMat.MultiplyPoint(vertices[triangle_2]);

                boundMin = Vector3.Min(boundMin, vertex_0);
                boundMin = Vector3.Min(boundMin, vertex_1);
                boundMin = Vector3.Min(boundMin, vertex_2);

                boundMax = Vector3.Max(boundMax, vertex_0);
                boundMax = Vector3.Max(boundMax, vertex_1);
                boundMax = Vector3.Max(boundMax, vertex_2);
            }

            scatter.boundMin = boundMin;
            scatter.boundMax = boundMax;
        }
        void BakeTexture()
        {
            ScatterEncod.WriteToRenderTexture(scatter.boundMin, scatter.boundMax, scatterBuffer, bakeTexture);
        }

        //
        void OnSceneGUI(SceneView sceneView)
        {
            DrawHandles();

            DrawWireframe();
            DrawPointCloud();
        }
        void DrawHandles()
        {
            EditorGUI.BeginChangeCheck();
            switch (Tools.current)
            {
                case Tool.Move:
                    scatter.position = Handles.PositionHandle(scatter.position, Quaternion.identity);
                    break;

                case Tool.Rotate:
                    scatter.quaternion = Handles.RotationHandle(scatter.quaternion, scatter.position);
                    break;

                case Tool.Scale:
                    scatter.localScale = Handles.ScaleHandle(scatter.localScale, scatter.position, scatter.quaternion);
                    break;
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                Undo.RecordObject(target, "Transform Object");

                onOptionChanged?.Invoke();
            }
        }
        void DrawWireframe()
        {
            if (scatter.sampleMesh == null) return;

            int[] triangles = scatter.sampleMesh.triangles;
            Vector3[] vertices = scatter.sampleMesh.vertices;
            Vector3[] normals = scatter.sampleMesh.normals;

            Matrix4x4 transformMat = scatter.localToWorldMatrix;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int triangle_0 = triangles[i + 0];
                int triangle_1 = triangles[i + 1];
                int triangle_2 = triangles[i + 2];

                Vector3 vertex_0 = transformMat.MultiplyPoint(vertices[triangle_0]);
                Vector3 vertex_1 = transformMat.MultiplyPoint(vertices[triangle_1]);
                Vector3 vertex_2 = transformMat.MultiplyPoint(vertices[triangle_2]);

                Vector3 normal_0 = transformMat.MultiplyVector(normals[triangle_0]).normalized;
                Vector3 normal_1 = transformMat.MultiplyVector(normals[triangle_1]).normalized;
                Vector3 normal_2 = transformMat.MultiplyVector(normals[triangle_2]).normalized;

                if (scatter.showWireframe)
                {
                    Handles.color = Color.green;
                    Handles.DrawLine(vertex_0, vertex_1);
                    Handles.DrawLine(vertex_1, vertex_2);
                    Handles.DrawLine(vertex_2, vertex_0);
                }

                if (scatter.showNormal)
                {
                    Handles.color = Color.blue;
                    Handles.DrawLine(vertex_0, vertex_0 + normal_0);
                    Handles.DrawLine(vertex_1, vertex_1 + normal_1);
                    Handles.DrawLine(vertex_2, vertex_2 + normal_2);
                }
            }

            if(scatter.showBound)
            {
                Vector3 center = (scatter.boundMin + scatter.boundMax) / 2;
                Vector3 size = (scatter.boundMax - scatter.boundMin);

                Handles.color = Color.gray;
                Handles.DrawWireCube(center, size);
            }
        }
        void DrawPointCloud()
        {
            if (scatter.showPointCloud && pointCloud != null)
            {
                for (int i = 0; i < scatter.instanceCount; i++)
                {
                    Vector3 point = pointCloud[i];

                    Handles.color = Color.red;
                    Handles.DrawWireCube(point, Vector3.one * 0.01f);
                }
            }
            //if (scatter.preview.displayNormal)
            //{
            //    for (int i = 0; i < scatter.pointCloud.Length; i++)
            //    {
            //        PointData point = scatter.pointCloud[i];

            //        Handles.color = Color.blue;
            //        Handles.DrawLine(point.pos, point.pos + point.normal * scatter.preview.normalLength);
            //    }
            //}
        }

        //
        public override bool HasPreviewGUI()
        {
            return true;
        }
        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("BakeTexture");
        }
        public override void OnPreviewSettings()
        {
            base.OnPreviewSettings();
        }
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            //base.OnPreviewGUI(r, background);
            GUI.DrawTexture(r, bakeTexture, ScaleMode.ScaleToFit);
        }

        //
        static void SaveRenderTexture(RenderTexture rt, string path)
        {
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            RenderTexture.active = null;
            var bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            Debug.Log($"Saved texture: {rt.width}x{rt.height} - " + path);
            AssetDatabase.Refresh();
        }

    }


}
