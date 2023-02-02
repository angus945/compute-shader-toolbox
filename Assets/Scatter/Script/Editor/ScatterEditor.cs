using UnityEditor;
using UnityEngine;

namespace SurfaceScatter
{
    [CustomEditor(typeof(PointScatter))]
    public class ScatterEditor : Editor
    {
        PointScatter scatter;

        int kernel;
        ComputeShader compute;
        ComputeBuffer argsBuffer;
        ComputeBuffer scatterBuffer;
        ComputeBuffer previewBuffer;

        void OnEnable()
        {
            scatter = (PointScatter)target;

            compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scatter/ComputeShader/PreviewCompute.compute");
            scatterBuffer = new ComputeBuffer(1000000, sizeof(float) * 4 * 4, ComputeBufferType.Structured);
            previewBuffer = new ComputeBuffer(1000000, sizeof(float) * 4 * 4, ComputeBufferType.Structured);
            argsBuffer = new ComputeBuffer(6, sizeof(uint), ComputeBufferType.IndirectArguments);
        }
        void OnDisable()
        {
            scatterBuffer.Dispose();
            previewBuffer.Dispose();
            argsBuffer.Dispose();
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSourceSelector();
            DrawScatterOptions();
            DrawPreviewOptions();
            DrawGenerateDatas();

            if (GUILayout.Button("Generate"))
            {
                scatter.LoadMeshTriangles();
                scatter.ScatterOnSurface();

                SetBufferDatas();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawSourceSelector()
        {
            SerializedProperty source = serializedObject.FindProperty("source");
            SerializedProperty type = source.FindPropertyRelative("type");
            SerializedProperty mesh = source.FindPropertyRelative("mesh");
            SerializedProperty gameObject = source.FindPropertyRelative("gameObject");

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(type);
            switch (scatter.source.type)
            {
                case SampleType.Mesh:
                    EditorGUILayout.PropertyField(mesh);
                    break;

                case SampleType.Object:
                    EditorGUILayout.PropertyField(gameObject);
                    break;
            }
        }
        void DrawScatterOptions()
        {
            EditorGUILayout.Space();

            SerializedProperty option = serializedObject.FindProperty("option");
            EditorGUILayout.PropertyField(option);
        }
        void DrawPreviewOptions()
        {
            EditorGUI.BeginChangeCheck();

            SerializedProperty preview = serializedObject.FindProperty("preview");
            EditorGUILayout.PropertyField(preview);
        }
        void DrawGenerateDatas()
        {
            int triangleCount = scatter.meshTriangles.Length;
            int pointCoundLength = scatter.pointCloud.Length;
            GUILayout.Label($"Triangle: {triangleCount}, Generated Points: {pointCoundLength}");
        }

        void SetBufferDatas()
        {
            Mesh mesh = scatter.preview.mesh;
            Material material = scatter.preview.material;

            scatterBuffer.SetData(scatter.pointCloudMatrix);
            argsBuffer.SetData(new uint[6]
            {
                (uint)mesh.GetIndexCount(0),
                (uint)scatter.pointCloudMatrix.Length,
                (uint)mesh.GetIndexStart(0),
                (uint)mesh.GetBaseVertex(0),
                0,
                0,
            });

            int kernel = compute.FindKernel("CSMain");
            compute.SetBuffer(kernel, "scatterBuffer", scatterBuffer);
            compute.SetBuffer(kernel, "previewBuffer", previewBuffer);

            material.SetBuffer("transformBuffer", previewBuffer);
        }

        void OnSceneGUI()
        {
            if (Application.isPlaying) return;

            DrawWireframe();
            DrawPointCloud();
        }
        void DrawWireframe()
        {
            if (scatter.meshTriangles == null) return;

            for (int i = 0; i < scatter.meshTriangles.Length; i++)
            {
                TriangleData face = scatter.meshTriangles[i];

                Handles.color = Color.green;
                Handles.DrawLine(face.vertexA.pos, face.vertexB.pos);
                Handles.DrawLine(face.vertexB.pos, face.vertexC.pos);
                Handles.DrawLine(face.vertexC.pos, face.vertexA.pos);

                Handles.color = Color.blue;
                Handles.DrawLine(face.vertexA.pos, face.vertexA.pos + face.vertexA.normal);
                Handles.DrawLine(face.vertexB.pos, face.vertexB.pos + face.vertexB.normal);
                Handles.DrawLine(face.vertexC.pos, face.vertexC.pos + face.vertexC.normal);
            }
        }
        void DrawPointCloud()
        {
            if (scatter.preview.displayInstance)
            {
                Mesh mesh = scatter.preview.mesh;
                Material material = scatter.preview.material;
                Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100);

                compute.SetFloat("_UnitScale", scatter.preview.globalScale);

                compute.Dispatch(kernel, (scatter.pointCloud.Length / 640 + 1), 1, 1);
                Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
            }
            if (scatter.preview.displayNormal)
            {
                for (int i = 0; i < scatter.pointCloud.Length; i++)
                {
                    PointData point = scatter.pointCloud[i];

                    Handles.color = Color.blue;
                    Handles.DrawLine(point.pos, point.pos + point.normal);
                }
            }



        }
    }

}
