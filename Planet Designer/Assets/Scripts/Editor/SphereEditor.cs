using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Sphere))]
public class SphereEditor : Editor
{
    private Sphere sphere;
    private Editor sphereSettingsEditor;

    private void OnEnable()
    {
        sphere = (Sphere)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();

        if (GUILayout.Button("Regenerate"))
            sphere.Regenerate();

        EditorGUILayout.Space();

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();

            if (check.changed)
                sphere.AutoRegenerate();
        }

        //DrawScriptableObjectEditor(sphere.Settings, ref sphereSettingsEditor);
    }

    private void DrawScriptableObjectEditor(Object scriptableObject, ref Editor editor)
    {
        if (scriptableObject == null)
            return;

        EditorGUILayout.Space();
        EditorGUI.indentLevel++;
        EditorGUILayout.InspectorTitlebar(true, scriptableObject);

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            CreateCachedEditor(scriptableObject, null, ref editor);
            editor.OnInspectorGUI();

            if (check.changed)
                sphere.AutoRegenerate();
        }
        EditorGUI.indentLevel--;
    }
}
