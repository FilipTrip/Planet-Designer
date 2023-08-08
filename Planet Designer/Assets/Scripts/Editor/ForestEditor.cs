using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NUnit.Framework;
using Codice.CM.Common;
using UnityEngine.TestTools;
using static Codice.Client.BaseCommands.EventTracking.TrackFeatureUseEvent;

[CustomEditor(typeof(Forest))]
public class ForestEditor : Editor
{
    private ForestSettings forestSettings;
    private ReorderableList smartPrefabList;

    protected SerializedProperty autoRegenerate;
    protected SerializedProperty seed;
    protected SerializedProperty coverage;
    protected SerializedProperty clusters;
    protected SerializedProperty avoidOcean;
    protected SerializedProperty avoidObjects;

    public void OnEnable()
    {
        forestSettings = (ForestSettings)target;

        autoRegenerate = serializedObject.FindProperty("autoRegenerate");
        seed = serializedObject.FindProperty("seed");
        coverage = serializedObject.FindProperty("coverage");
        clusters = serializedObject.FindProperty("clusters");
        avoidOcean = serializedObject.FindProperty("avoidOcean");
        avoidObjects = serializedObject.FindProperty("avoidObjects");

        smartPrefabList = new ReorderableList(serializedObject, serializedObject.FindProperty("prefabs"), true, true, false, false);
        smartPrefabList.drawElementCallback = DrawSmartPrefabElement;
        smartPrefabList.drawHeaderCallback = DrawSmartPrefabHeader;
        smartPrefabList.elementHeight = (EditorGUIUtility.singleLineHeight + 3) * 3.2f;
    }

    protected void DeleteButton()
    {
        if (GUILayout.Button("Delete"))
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddDisabledItem(new GUIContent("Are you sure?"));
            genericMenu.AddItem(new GUIContent("Cancel"), false, () => { });
            genericMenu.AddItem(new GUIContent("Delete"), false, DeleteFeature);
            genericMenu.ShowAsContext();

            void DeleteFeature()
            {
                if (BelongsToLoadedPlanet())
                    FeatureManager.Instance.Remove(forestSettings.forest);
                else
                {
                    string featureFolderPath = AssetDatabase.GetAssetPath(forestSettings);
                    featureFolderPath = featureFolderPath.Remove(featureFolderPath.LastIndexOf('/'));
                    AssetDatabase.DeleteAsset(featureFolderPath);
                }
            }
        }
    }

    protected void RegenerateButton()
    {
        GUI.enabled = BelongsToLoadedPlanet();

        if (GUILayout.Button("Regenerate"))
        {
            forestSettings.forest.Regenerate();
        }

        GUI.enabled = true;
    }

    protected void HorizontalLine()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(GUIContent.none, GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
    }

    protected bool BelongsToLoadedPlanet()
    {
        return Planet.Instance && Planet.Instance.Features.Contains(forestSettings.forest);
    }

    protected void DrawSmartPrefabList()
    {
        EditorGUILayout.Space();

        float originalWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 55;
        smartPrefabList.DoLayoutList();
        EditorGUIUtility.labelWidth = originalWidth;

        // Add and Remove button

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add"))
        {
            SmartPrefab smartPrefab = new SmartPrefab();
            smartPrefab.scale = 1;
            smartPrefab.enabled = true;
            smartPrefab.prefab = null;
            forestSettings.prefabs.Add(smartPrefab);
        }

        GUI.enabled = smartPrefabList.selectedIndices.Count > 0;

        if (GUILayout.Button("Remove"))
        {
            forestSettings.prefabs.RemoveAt(smartPrefabList.selectedIndices[0]);
            smartPrefabList.ClearSelection();
        }

        if (GUILayout.Button("Duplicate"))
        {
            SmartPrefab selected = forestSettings.prefabs[smartPrefabList.selectedIndices[0]];
            SmartPrefab duplicate = new SmartPrefab();
            duplicate.scale = selected.scale;
            duplicate.enabled = selected.enabled;
            duplicate.prefab = selected.prefab;
            forestSettings.prefabs.Add(duplicate);
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    protected void DrawInfo()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(GUIContent.none, GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(new GUIContent("Game Object Count: " + forestSettings.numberOfObjects));
    }

    // Is called for each element in the list
    private void DrawSmartPrefabElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = smartPrefabList.serializedProperty.GetArrayElementAtIndex(index);

        float width = Mathf.Max(rect.width, 120);
        float y = 2;
        float lineHeight = EditorGUIUtility.singleLineHeight;

        // 'Enabled' property

        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y + y, width, lineHeight),
            element.FindPropertyRelative("enabled"),
            new GUIContent("Enabled"));

        // 'Prefab' property

        y += lineHeight + 3;

        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y + y, width, lineHeight),
            element.FindPropertyRelative("prefab"),
            new GUIContent("Prefab"));

        // 'Scale' property

        y += lineHeight + 3;

        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y + y, width, lineHeight),
            element.FindPropertyRelative("scale"),
            new GUIContent("Scale"));
    }

    private void DrawSmartPrefabHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "Prefabs");
    }
}
