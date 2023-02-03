using UnityEditor;
using UnityEngine;

namespace SurfaceScatter
{
    [CustomEditor(typeof(PointScatter))]
    public class ScatterEditor : Editor
    {
        PointScatter scatter;

        void OnEnable()
        {
            scatter = (PointScatter)target;
        }
        void OnDisable()
        {

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

                if (scatter.preview.displayWireframe)
                {
                    Handles.color = Color.green;
                    Handles.DrawLine(face.vertexA.pos, face.vertexB.pos);
                    Handles.DrawLine(face.vertexB.pos, face.vertexC.pos);
                    Handles.DrawLine(face.vertexC.pos, face.vertexA.pos);
                }

                if(scatter.preview.displayWireframeNormal)
                {
                    Handles.color = Color.blue;
                    Handles.DrawLine(face.vertexA.pos, face.vertexA.pos + face.vertexA.normal);
                    Handles.DrawLine(face.vertexB.pos, face.vertexB.pos + face.vertexB.normal);
                    Handles.DrawLine(face.vertexC.pos, face.vertexC.pos + face.vertexC.normal);
                }
            }
        }
        void DrawPointCloud()
        {
            if (scatter.preview.displayPointCloud)
            {
                for (int i = 0; i < scatter.pointCloud.Length; i++)
                {
                    PointData point = scatter.pointCloud[i];

                    Handles.color = Color.red;
                    Handles.DrawWireCube(point.pos, Vector3.one * scatter.preview.pointSize);
                }
            }
            if (scatter.preview.displayNormal)
            {
                for (int i = 0; i < scatter.pointCloud.Length; i++)
                {
                    PointData point = scatter.pointCloud[i];

                    Handles.color = Color.blue;
                    Handles.DrawLine(point.pos, point.pos + point.normal * scatter.preview.normalLength);
                }
            }
        }
    }

}
