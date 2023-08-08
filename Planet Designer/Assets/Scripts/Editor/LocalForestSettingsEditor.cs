using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(LocalForestSettings))]
public class LocalForestSettingsEditor : ForestEditor
{
    private LocalForestSettings localForestSettings;

    private new void OnEnable()
    {
        base.OnEnable();
        localForestSettings = (LocalForestSettings)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        base.DeleteButton();
        base.RegenerateButton();
        ClearButton();
        EditorGUILayout.EndHorizontal();
        base.HorizontalLine();

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(autoRegenerate);
            EditorGUILayout.PropertyField(seed);
            EditorGUILayout.PropertyField(coverage);
            EditorGUILayout.PropertyField(clusters);
            EditorGUILayout.PropertyField(avoidOcean);
            EditorGUILayout.PropertyField(avoidObjects);
            base.DrawSmartPrefabList();
            base.DrawInfo();
            serializedObject.ApplyModifiedProperties();

            if (check.changed && Planet.Instance)
            {
                localForestSettings.forest.UpdateLayerMask();
                localForestSettings.forest.AutoRegenerate();
            }
        }
    }

    private void ClearButton()
    {
        GUI.enabled = BelongsToLoadedPlanet();

        if (GUILayout.Button("Clear"))
        {
            localForestSettings.forest.GetComponent<Zone>().Clear();
        }

        GUI.enabled = true;
    }
}
