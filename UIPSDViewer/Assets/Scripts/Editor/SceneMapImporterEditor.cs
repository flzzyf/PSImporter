using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneMapImporter))]
public class SceneMapImporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SceneMapImporter main = target as SceneMapImporter;

        if (GUILayout.Button("生成"))
        {
            main.GenerateLayers();
        }
    }
}
