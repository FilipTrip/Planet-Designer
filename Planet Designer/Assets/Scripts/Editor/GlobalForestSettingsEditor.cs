using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(GlobalForestSettings))]
public class GlobalForestSettingsEditor : Editor
{
    private GlobalForestSettings globalForestSettings;

    SerializedProperty numberOfObjects;
    SerializedProperty autoRegenerate;
    SerializedProperty seed;
    SerializedProperty density;
    SerializedProperty coverage;
    SerializedProperty clusters;
    SerializedProperty avoidOcean;
    SerializedProperty avoidObjects;
    SerializedProperty prefabs;

    private void OnEnable()
    {
        globalForestSettings = (GlobalForestSettings)target;

        numberOfObjects = serializedObject.FindProperty("numberOfObjects");
        autoRegenerate  = serializedObject.FindProperty("autoRegenerate");
        seed            = serializedObject.FindProperty("seed");
        density         = serializedObject.FindProperty("density");
        coverage        = serializedObject.FindProperty("coverage");
        clusters        = serializedObject.FindProperty("clusters");
        avoidOcean      = serializedObject.FindProperty("avoidOcean");
        avoidObjects    = serializedObject.FindProperty("avoidObjects");
        prefabs         = serializedObject.FindProperty("prefabs");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();

        if (GUILayout.Button("Regenerate"))
            globalForestSettings.forest.Regenerate();

        EditorGUILayout.Space();

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            //base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUILayout.PropertyField(numberOfObjects);
            EditorGUILayout.PropertyField(autoRegenerate);
            EditorGUILayout.PropertyField(seed);
            EditorGUILayout.PropertyField(density);
            EditorGUILayout.PropertyField(coverage);
            EditorGUILayout.PropertyField(clusters);
            EditorGUILayout.PropertyField(avoidOcean);
            EditorGUILayout.PropertyField(avoidObjects);
            EditorGUILayout.PropertyField(prefabs);
            serializedObject.ApplyModifiedProperties();

            if (check.changed && globalForestSettings)
            {
                globalForestSettings.forest.UpdateLayerMask();
                globalForestSettings.forest.AutoRegenerate();
            }  
        }
    }
}
