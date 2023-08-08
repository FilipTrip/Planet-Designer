using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditorInternal;

[CustomEditor(typeof(GlobalForestSettings))]
public class GlobalForestSettingsEditor : ForestEditor
{
    private GlobalForestSettings globalForestSettings;

    private SerializedProperty density;

    private new void OnEnable()
    {
        base.OnEnable();
        globalForestSettings = (GlobalForestSettings)target;

        density = serializedObject.FindProperty("density");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        base.DeleteButton();
        base.RegenerateButton();
        EditorGUILayout.EndHorizontal();
        base.HorizontalLine();

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(autoRegenerate);
            EditorGUILayout.PropertyField(seed);
            EditorGUILayout.PropertyField(density);
            EditorGUILayout.PropertyField(coverage);
            EditorGUILayout.PropertyField(clusters);
            EditorGUILayout.PropertyField(avoidOcean);
            EditorGUILayout.PropertyField(avoidObjects);
            base.DrawSmartPrefabList();
            base.DrawInfo();
            serializedObject.ApplyModifiedProperties();

            if (check.changed && Planet.Instance)
            {
                globalForestSettings.forest.UpdateLayerMask();
                globalForestSettings.forest.AutoRegenerate();
            }  
        }
    }

}
