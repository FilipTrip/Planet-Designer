using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalForestSettings))]
public class LocalForestSettingsEditor : Editor
{
    private LocalForestSettings globalForestSettings;

    private void OnEnable()
    {
        globalForestSettings = (LocalForestSettings)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();

        if (GUILayout.Button("Regenerate"))
            globalForestSettings.forest.Regenerate();

        EditorGUILayout.Space();

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();

            if (check.changed && globalForestSettings)
            {
                globalForestSettings.forest.UpdateLayerMask();
                globalForestSettings.forest.AutoRegenerate();
            }
        }
    }
}
