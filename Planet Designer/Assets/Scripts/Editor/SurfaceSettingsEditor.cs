using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NUnit.Framework;

[CustomEditor(typeof(SurfaceSettings))]
public class SurfaceSettingsEditor : Editor
{
    private SurfaceSettings surfaceSettings;

    private SerializedProperty autoRegenerate;
    private SerializedProperty fixEdgeNormals;
    private SerializedProperty vertexDistributionBias;

    private SerializedProperty oceanResolution;
    private SerializedProperty oceanLevel;
    private SerializedProperty oceanMaterial;

    private SerializedProperty terrainResolution;
    private SerializedProperty terrainRadius;
    private SerializedProperty terrainMaterial;
    private SerializedProperty terrainNoiseLayers;

    private GUIStyle labelStyle;
    private GUIStyle listElementLabelStyle;
    private Color terrainLabelColor = new Color(0.12f, 0.95f, 0.39f);
    private Color oceanLabelColor = new Color(0.0f, 0.8f, 1.0f);

    private ReorderableList list;

    [SerializeField]
    private static NoiseLayerPresets noiseLayerPresets;

    private void OnEnable()
    {
        surfaceSettings = (SurfaceSettings)target;

        autoRegenerate         = serializedObject.FindProperty("autoRegenerate");
        fixEdgeNormals         = serializedObject.FindProperty("fixEdgeNormals");
        vertexDistributionBias = serializedObject.FindProperty("vertexDistributionBias");

        oceanResolution        = serializedObject.FindProperty("oceanResolution");
        oceanLevel             = serializedObject.FindProperty("oceanLevel");
        oceanMaterial          = serializedObject.FindProperty("oceanMaterial");

        terrainResolution      = serializedObject.FindProperty("terrainResolution");
        terrainRadius          = serializedObject.FindProperty("terrainRadius");
        terrainMaterial        = serializedObject.FindProperty("terrainMaterial");
        terrainNoiseLayers     = serializedObject.FindProperty("terrainNoiseLayers");

        labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;

        listElementLabelStyle = new GUIStyle();
        listElementLabelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        listElementLabelStyle.fontStyle = FontStyle.Bold;
     
        list = new ReorderableList(serializedObject, terrainNoiseLayers, true, true, false, false);
        list.drawElementCallback = DrawElement;
        list.drawHeaderCallback = DrawHeader;
        list.elementHeight = 140;
    }

    public override void OnInspectorGUI()
    {
        // Buttons

        EditorGUILayout.Space();

        if (GUILayout.Button("Regenerate"))
            Planet.Instance.Regenerate();

        // General section
        Label("General", Color.white);

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(autoRegenerate);

            if (SurfaceSettings.showHiddenProperties)
            {
                EditorGUILayout.PropertyField(fixEdgeNormals);
                EditorGUILayout.PropertyField(vertexDistributionBias);
            }

            serializedObject.ApplyModifiedProperties();
          
            if (check.changed)
            {
                if (Planet.Instance)
                    Planet.Instance.AutoRegenerate();
            }
        }

        // Ocean section
        Label("Ocean", oceanLabelColor);

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            serializedObject.Update();

            if (SurfaceSettings.showHiddenProperties)
            {
                EditorGUILayout.PropertyField(oceanMaterial);
            }

            EditorGUILayout.PropertyField(oceanResolution);
            EditorGUILayout.PropertyField(oceanLevel);
            serializedObject.ApplyModifiedProperties();

            if (check.changed)
            {
                surfaceSettings.UpdateOceanRadius();

                if (Planet.Instance)
                    Planet.Instance.OceanSphere.AutoRegenerate();
            }
        }

        // Terrain section
        Label("Terrain", terrainLabelColor);

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            serializedObject.Update();

            if (SurfaceSettings.showHiddenProperties)
            {
                EditorGUILayout.PropertyField(terrainMaterial);
            }

            EditorGUILayout.PropertyField(terrainResolution);
            EditorGUILayout.PropertyField(terrainRadius);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Adjust labelWidth and draw list
            float originalWidth = EditorGUIUtility.labelWidth;
            list.DoLayoutList();
            EditorGUIUtility.labelWidth = originalWidth;

            serializedObject.ApplyModifiedProperties();

            if (check.changed)
            {
                surfaceSettings.UpdateOceanRadius();

                if (Planet.Instance)
                    Planet.Instance.AutoRegenerate();
            }
        }

        // Add and Remove button (Noise Layers)

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add"))
        {
            if (noiseLayerPresets == null)
            {
                noiseLayerPresets = Resources.Load<NoiseLayerPresets>("Other/Noise Layer Presets");
            }

            GenericMenu menu = new GenericMenu();

            foreach (NoiseLayerPresets.Preset preset in noiseLayerPresets.presets)
            {
                menu.AddItem(new GUIContent(preset.name), false, AddNoiseLayerPreset, preset.noiseLayer);
            }

            menu.ShowAsContext();
        }

        GUI.enabled = list.selectedIndices.Count > 0;

        if (GUILayout.Button("Remove"))
        {
            List<NoiseLayer> toBeRemoved = new List<NoiseLayer>();

            foreach (int index in list.selectedIndices)
                toBeRemoved.Add(surfaceSettings.terrainNoiseLayers[index]);

            foreach (NoiseLayer noiseLayer in toBeRemoved)
                surfaceSettings.terrainNoiseLayers.Remove(noiseLayer);

            list.ClearSelection();

            if (Planet.Instance)
                Planet.Instance.AutoRegenerate();
        }

        if (GUILayout.Button("Duplicate"))
        {
            NoiseLayer selected = surfaceSettings.terrainNoiseLayers[list.selectedIndices[0]];
            NoiseLayer duplicate = new NoiseLayer();
            duplicate.amplitude = selected.amplitude;
            duplicate.scale = selected.scale;
            duplicate.seed = new Seed();
            duplicate.seed.value = selected.seed.value;
            duplicate.curve = new AnimationCurve(selected.curve.keys);
            surfaceSettings.terrainNoiseLayers.Add(duplicate);

            if (Planet.Instance)
                Planet.Instance.AutoRegenerate();
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    private void Label(string text, Color color)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        GUI.contentColor = color;
        GUILayout.Label(text, labelStyle);
        GUI.contentColor = Color.white;
    }

    private void AddNoiseLayerPreset(object noiseLayerObj)
    {
        NoiseLayer noiseLayer = (NoiseLayer)noiseLayerObj;
        NoiseLayer duplicate = new NoiseLayer();
        duplicate.seed = new Seed();
        duplicate.seed.New();
        duplicate.scale = noiseLayer.scale;
        duplicate.amplitude = noiseLayer.amplitude;
        duplicate.curve = new AnimationCurve(noiseLayer.curve.keys);

        surfaceSettings.terrainNoiseLayers.Add(duplicate);
        list.Select(surfaceSettings.terrainNoiseLayers.Count - 1);

        if (Planet.Instance)
            Planet.Instance.AutoRegenerate();
    }

    // ReorderableList Delegates

    // Is called for each element in the list
    private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);

        float y = 0;
        float lineHeight = EditorGUIUtility.singleLineHeight;
        bool wide = rect.width >= 200;
        float halfWidth = rect.width * (wide ? 0.5f : 0.3f);

        // Label

        EditorGUI.LabelField(new Rect(rect.x, rect.y + 3, 100, lineHeight), "Layer " + (index + 1), listElementLabelStyle);
        EditorGUI.LabelField(new Rect(rect.x, rect.y + 12, rect.width, lineHeight), "", GUI.skin.horizontalSlider);

        // 'Enabled' property

        y += lineHeight + 12;
        EditorGUIUtility.labelWidth = 55;

        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y + y, halfWidth, lineHeight),
            element.FindPropertyRelative("enabled"),
            new GUIContent(wide ? "Enabled" : ""));

        // 'Scale' property

        EditorGUIUtility.labelWidth = 65;

        EditorGUI.PropertyField(
            new Rect(rect.x + halfWidth, rect.y + y, rect.width - halfWidth, lineHeight),
            element.FindPropertyRelative("scale"),
            new GUIContent("Scale"));

        // 'Seed' property

        y += lineHeight + 2;
        EditorGUIUtility.labelWidth = 55;

        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y + y, halfWidth - 15, lineHeight),
            element.FindPropertyRelative("seed").FindPropertyRelative("value"),
            new GUIContent(wide ? "Seed" : ""));

        // 'Amplitude' property

        EditorGUIUtility.labelWidth = 65;

        EditorGUI.PropertyField(
            new Rect(rect.x + halfWidth, rect.y + y, rect.width - halfWidth, lineHeight),
            element.FindPropertyRelative("amplitude"),
            new GUIContent("Amplitude"));

        // 'Curve' property

        y += lineHeight + 10;

        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y + y, rect.width, lineHeight * 2.5f),
            element.FindPropertyRelative("curve"),
            GUIContent.none);
    }

    private void DrawHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "Terrain Noise Layers");
    }

}
