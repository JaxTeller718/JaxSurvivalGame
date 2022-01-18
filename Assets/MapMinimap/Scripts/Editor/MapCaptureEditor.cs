using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MapMinimap;

namespace MapMinimap.EditorTool
{
    [CustomEditor(typeof(MapCapture))]
    public class MapCaptureEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MapCapture myScript = target as MapCapture;

            DrawDefaultInspector();

            EditorGUILayout.Space();

            if (myScript.mode == MapCaptureMode.Runtime || myScript.mode == MapCaptureMode.RuntimeUpdate)
                myScript.save_mode = MapSaveMode.RenderTexture;

            if (myScript.save_mode == MapSaveMode.File)
            {
                myScript.save_folder = AddTextField("Save Folder", myScript.save_folder);
                myScript.file_size = AddIntField("File Pixel Size", myScript.file_size);
            }

            if (myScript.save_mode == MapSaveMode.RenderTexture)
            {
                myScript.render_texture = AddObjectField<RenderTexture>("Render Texture", myScript.render_texture);
            }

            EditorGUILayout.Space();

            if (myScript.capture_camera == null)
            {
                if (GUILayout.Button("Create Camera"))
                {
                    myScript.CreateCamera();
                }
            }
            else
            {
                if (GUILayout.Button("Reset Camera"))
                {
                    myScript.ResetCamera();
                }
            }

            if (myScript.mode == MapCaptureMode.Editor)
            {
                if (GUILayout.Button("Capture Map"))
                {
                    myScript.CaptureMapAndSave();
                    EditorUtility.SetDirty(myScript);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }

            EditorGUILayout.Space();
        }

        private string AddTextField(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GetLabelWidth());
            GUILayout.FlexibleSpace();
            string outval = EditorGUILayout.TextField(value, GetWidth());
            GUILayout.EndHorizontal();
            return outval;
        }

        private int AddIntField(string label, int value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GetLabelWidth());
            GUILayout.FlexibleSpace();
            int outval = EditorGUILayout.IntField(value, GetWidth());
            GUILayout.EndHorizontal();
            return outval;
        }

        private T AddObjectField<T>(string label, Object value) where T : Object
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GetLabelWidth());
            GUILayout.FlexibleSpace();
            T outval = (T)EditorGUILayout.ObjectField(value, typeof(T), true, GetWidth());
            GUILayout.EndHorizontal();
            return outval;
        }

        private GUILayoutOption GetLabelWidth()
        {
            return GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.4f);
        }

        private GUILayoutOption GetWidth()
        {
            return GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.45f);
        }
    }

}