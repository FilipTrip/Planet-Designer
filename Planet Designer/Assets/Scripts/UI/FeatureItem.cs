using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FeatureItem : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button featureButton;
    [SerializeField] private Button renameButton;

    private Feature feature;

    public void Initialize(Feature feature)
    {
        this.feature = feature;
        gameObject.name = feature.name;
    }

    private void Start()
    {
        inputField.interactable = false;

        if (!feature)
            return;

        inputField.SetTextWithoutNotify(feature.name);
        inputField.transform.Find("Text Area").Find("Caret").GetComponent<TMP_SelectionCaret>().raycastTarget = false;
    }

    // Called by feature button
    public void SelectFeature()
    {
        FeatureManager.Instance.Select(feature);
    }

    // Called by rename button
    public void StartRenameFeature()
    {
        CanvasManager.Instance.AddPlanetControlOverrider(this);
        inputField.interactable = true;
        inputField.Select();
        inputField.transform.Find("Text Area").Find("Text").GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Underline;
    }

    // Called when input field value changed
    public void EndRenameFeature(string newFeatureName)
    {
        CanvasManager.Instance.RemovePlanetControlOverrider(this);
        inputField.interactable = false;
        inputField.transform.Find("Text Area").Find("Text").GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
        ResourceManager.Instance.RenameFeature(Planet.Instance.PlanetName, feature.name, newFeatureName);
        Debug.Log(feature.name + " renamed to " + newFeatureName);
        feature.name = newFeatureName;
    }
}
