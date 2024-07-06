using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XRToolkit
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RectTransform))]
    public class RectTransformEditor : Editor
    {
        private Editor instance;
        private MethodInfo onSceneGUI;
        private static readonly object[] emptyArray = new object[0];

        private float aspectRatio;
        private Vector2 size = Vector2.zero;

        private RectTransform _target;

        protected virtual void OnEnable()
        {
            _target = target as RectTransform;

            var editorType = Assembly.GetAssembly(typeof(Editor)).GetTypes().FirstOrDefault(m => m.Name == "RectTransformEditor");
            instance = CreateEditor(targets, editorType);
            onSceneGUI = editorType.GetMethod("OnSceneGUI", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            size = new Vector2(_target.sizeDelta.x, _target.sizeDelta.y);
        }

        private void OnDisable()
        {
            if (instance)
            {
                DestroyImmediate(instance);
            }
        }

        public override void OnInspectorGUI()
        {
            instance.OnInspectorGUI();

            GUILayout.Space(10f);
            

            GUILayout.BeginHorizontal();
            GUILayout.Label("Size");
            size = EditorGUILayout.Vector2Field("", size);
            GUILayout.Label("Ratio");
            aspectRatio = EditorGUILayout.FloatField(_target.sizeDelta.x / _target.sizeDelta.y);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Reset Size"))
            {
                SetSizeWithAspectRatio(_target, aspectRatio, size.x, size.y);
                size = new Vector2(_target.sizeDelta.x, _target.sizeDelta.y);
            }
            
            if (GUILayout.Button("Copy Full Path"))
            {
                GameObject go = Selection.activeGameObject;
                string path = GetFullPath(go);
                EditorGUIUtility.systemCopyBuffer = path;
                Debug.Log("Full Path copied to clipboard: " + path);
            }
        }

        private string GetFullPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform.parent;

            while (current != null)
            {
                if (current.GetComponent<Canvas>() == null)
                {
                    path = current.name + "/" + path;
                }
                current = current.parent;
            }

            return path;
        }

        private void OnSceneGUI()
        {
            if (instance)
            {
                onSceneGUI.Invoke(instance, emptyArray);
            }
        }

        private void SetSizeWithAspectRatio(RectTransform rectTransform, float aspectRatio, float width, float height)
        {
            if (width != rectTransform.sizeDelta.x)
            {
                height = width / aspectRatio;
            }
            else if (height != rectTransform.sizeDelta.y)
            {
                width = height * aspectRatio;
            }

            rectTransform.sizeDelta = new Vector2(width, height);
        }
    }
}
