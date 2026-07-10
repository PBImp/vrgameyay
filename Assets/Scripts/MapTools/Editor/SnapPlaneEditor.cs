using UnityEditor;
using UnityEngine;

namespace MapTools.Editor
{
    [CustomEditor(typeof(SnapPlane))]
    [CanEditMultipleObjects]
    public class SnapPlaneEditor : UnityEditor.Editor
    {
        private static readonly PlaneEdge[] AllEdges =
        {
            PlaneEdge.Left, PlaneEdge.Right, PlaneEdge.Top, PlaneEdge.Bottom
        };

        private static readonly Color[] EdgeColors =
        {
            Color.red, Color.green, Color.cyan, Color.yellow
        };

        private PlaneEdge newChildEdge = PlaneEdge.Top;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "hingeAngle");
            serializedObject.ApplyModifiedProperties();

            var plane = (SnapPlane)target;

            if (plane.parentPlane != null)
            {
                EditorGUI.BeginChangeCheck();
                float newAngle = EditorGUILayout.Slider("Hinge Angle", plane.hingeAngle, -180f, 180f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(plane, "Change Hinge Angle");
                    plane.hingeAngle = newAngle;
                    plane.Apply();
                    EditorUtility.SetDirty(plane);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Build Onward", EditorStyles.boldLabel);
            newChildEdge = (PlaneEdge)EditorGUILayout.EnumPopup("From Edge", newChildEdge);
            if (GUILayout.Button($"Create Plane Snapped to {newChildEdge} Edge"))
            {
                CreateChild(plane, newChildEdge);
            }
        }

        private static void CreateChild(SnapPlane parent, PlaneEdge edge)
        {
            var go = new GameObject($"{parent.name} - {edge} Plane");
            Undo.RegisterCreatedObjectUndo(go, "Create Snap Plane");
            var child = go.AddComponent<SnapPlane>();
            child.parentPlane = parent;
            child.parentEdge = edge;
            child.hingeAngle = 90f;
            child.extrusion = 2f;
            child.thickness = parent.thickness;
            child.Apply();

            Selection.activeGameObject = go;
        }

        private void OnSceneGUI()
        {
            var plane = (SnapPlane)target;
            DrawEdges(plane);

            if (plane.parentPlane != null)
            {
                DrawExtrusionHandle(plane);
            }
        }

        private static void DrawEdges(SnapPlane plane)
        {
            for (int i = 0; i < AllEdges.Length; i++)
            {
                var (a, b) = plane.GetEdgeWorld(AllEdges[i]);
                Handles.color = EdgeColors[i];
                Handles.DrawLine(a, b, 3f);
            }
        }

        private static void DrawExtrusionHandle(SnapPlane plane)
        {
            var (a, b) = plane.GetEdgeWorld(PlaneEdge.Top);
            Vector3 mid = (a + b) * 0.5f;
            Vector3 dir = plane.transform.up;

            EditorGUI.BeginChangeCheck();
            Handles.color = Color.white;
            Vector3 newPos = Handles.Slider(mid, dir, HandleUtility.GetHandleSize(mid) * 0.6f, Handles.ArrowHandleCap, 0f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(plane, "Resize Plane");
                float delta = Vector3.Dot(newPos - mid, dir);
                plane.extrusion = Mathf.Max(0.1f, plane.extrusion + delta);
                plane.Apply();
                EditorUtility.SetDirty(plane);
            }
        }

        [MenuItem("GameObject/3D Object/Snap Plane", false, 10)]
        private static void CreateRootPlane(MenuCommand menuCommand)
        {
            var go = new GameObject("Snap Plane");
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            var plane = go.AddComponent<SnapPlane>();
            plane.Apply();

            Undo.RegisterCreatedObjectUndo(go, "Create Snap Plane");
            Selection.activeGameObject = go;
        }
    }
}
