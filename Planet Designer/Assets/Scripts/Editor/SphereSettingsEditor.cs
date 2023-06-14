using System.Collections;
using System.Collections.Generic;
using Codice.CM.Common;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SphereSettings))]
public class SphereSettingsEditor : Editor
{
    private SphereSettings sphereSettings;

    private SerializedProperty autoRegenerate;
    private SerializedProperty fixEdgeNormals;
    private SerializedProperty vertexDistributionBias;
    private SerializedProperty resolution;
    private SerializedProperty radius;
    private SerializedProperty material;
    private SerializedProperty noiseLayers;

    private void OnEnable()
    {
        sphereSettings = (SphereSettings)target;

        autoRegenerate         = serializedObject.FindProperty("autoRegenerate");
        fixEdgeNormals         = serializedObject.FindProperty("fixEdgeNormals");
        vertexDistributionBias = serializedObject.FindProperty("vertexDistributionBias");
        resolution             = serializedObject.FindProperty("resolution");
        radius                 = serializedObject.FindProperty("radius");
        material               = serializedObject.FindProperty("material");
        noiseLayers            = serializedObject.FindProperty("noiseLayers");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();

        if (GUILayout.Button("Regenerate"))
            sphereSettings.sphere.Regenerate();

        EditorGUILayout.Space();

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(autoRegenerate);

            if (sphereSettings.showFixEdgeNormals)
                EditorGUILayout.PropertyField(fixEdgeNormals);

            if (sphereSettings.showVertexDistributionBias)
                EditorGUILayout.PropertyField(vertexDistributionBias);

            EditorGUILayout.PropertyField(resolution);
            EditorGUILayout.PropertyField(radius);

            if (sphereSettings.showMaterial)
                EditorGUILayout.PropertyField(material);

            if (sphereSettings.showNoiseLayers)
                EditorGUILayout.PropertyField(noiseLayers);

            serializedObject.ApplyModifiedProperties();

            if (check.changed && sphereSettings)
            {
                sphereSettings.sphere.AutoRegenerate();
            }
        }
    }
}
