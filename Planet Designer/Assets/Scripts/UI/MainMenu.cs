using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("My Planets References")]
    [SerializeField] private GameObject myPlanetsSelection;
    [SerializeField] private GameObject myPlanetsButtons;
    [SerializeField] private GridLayoutGroup myPlanetsGrid;
    [SerializeField] private ToggleGroup myPlanetsToggleGroup;

    [Header("Presets References")]
    [SerializeField] private GameObject presetsSelection;
    [SerializeField] private GameObject presetsButtons;
    [SerializeField] private GridLayoutGroup presetsGrid;
    [SerializeField] private ToggleGroup presetsToggleGroup;

    [Header("Button References")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button newButton;
    [SerializeField] private Button openButton;
    [SerializeField] private Button renameButton;
    [SerializeField] private Button duplicateButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button createButton;
    [SerializeField] private Button cancelButton;

    [Header("Other References")]
    [SerializeField] private TextMeshProUGUI newPlanetName;
    [SerializeField] private TextMeshProUGUI renamePlanetName;
    [SerializeField] private CanvasManager canvasManager;
    [SerializeField] private GameObject newPlanetPrompt;
    [SerializeField] private GameObject editorMenu;
    [SerializeField] private GameObject planetCardPrefab;

    [Header("Values")]
    [SerializeField] private PlanetCard selectedPlanetCard;


    private void OnEnable()
    {
        RefreshMyPlanets();
        myPlanetsSelection.SetActive(true);
        myPlanetsButtons.SetActive(true);
        presetsSelection.SetActive(false);
        presetsButtons.SetActive(false);
        backButton.gameObject.SetActive(Planet.Instance);
        canvasManager.AddPlanetControlOverrider(this);
    }

    private void OnDisable()
    {
        canvasManager.RemovePlanetControlOverrider(this);
    }

    private void Update()
    {
        // Enable/disable My Planet buttons
        if (myPlanetsGrid.gameObject.activeInHierarchy)
        {
            if (myPlanetsToggleGroup.AnyTogglesOn())
            {
                if (!openButton.interactable)
                    EnableMyPlanetsButtons(true);
            }
            else
            {
                if (openButton.interactable)
                    EnableMyPlanetsButtons(false);
            }
        }

        // Enable/disable Preset buttons
        else
        {
            if (presetsToggleGroup.AnyTogglesOn())
            {
                if (!createButton.interactable)
                    EnablePresetButtons(true);
            }
            else
            {
                if (createButton.interactable)
                    EnablePresetButtons(false);
            }
        }
    }

    public void RefreshMyPlanets()
    {
        selectedPlanetCard = null;

        foreach (Transform planetCard in myPlanetsGrid.transform)
            Destroy(planetCard.gameObject);

        foreach (string planetName in ResourceManager.Instance.GetSubdirectoryNames("Planets"))
        {
            PlanetCard planetCard = Instantiate(planetCardPrefab, myPlanetsGrid.transform).GetComponent<PlanetCard>();
            planetCard.SetName(planetName);
            planetCard.SetSprite(ResourceManager.Instance.GetPlanetSprite("Planets", planetName));
        }

        Debug.Log("My Planets refreshed");
    }

    public void RefreshPresets()
    {
        selectedPlanetCard = null;

        foreach (Transform planetCard in presetsGrid.transform)
            Destroy(planetCard.gameObject);

        foreach (string planetName in ResourceManager.Instance.GetSubdirectoryNames("Presets"))
        {
            PlanetCard planetCard = Instantiate(planetCardPrefab, presetsGrid.transform).GetComponent<PlanetCard>();
            planetCard.SetName(planetName);
            planetCard.SetSprite(ResourceManager.Instance.GetPlanetSprite("Presets", planetName));
        }

        Debug.Log("Presets refreshed");
    }

    public void SelectPlanetCard(PlanetCard planetCard)
    {
        selectedPlanetCard = planetCard;
    }

    public void DeselectPlanetCard()
    {
        selectedPlanetCard = null;
    }

    public void EnableMyPlanetsButtons(bool enabled)
    {
        openButton.SetEnabled(enabled);
        renameButton.SetEnabled(enabled);
        duplicateButton.SetEnabled(enabled);
        deleteButton.SetEnabled(enabled);
    }

    public void EnablePresetButtons(bool enabled)
    {
        createButton.SetEnabled(enabled);
    }

    // Button actions

    public void NewPlanetNameOnEndEdit()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (newPlanetName.text.Length > 1)
                CreatePlanet();
        }  
    }

    public void CreatePlanet()
    {
        if (newPlanetName.text == "")
            throw new System.Exception("No planet name entered");

        Destroy(GameObject.Find("Planet"));
        StartCoroutine(Coroutine_CreatePlanet());
    }

    private IEnumerator Coroutine_CreatePlanet()
    {
        Debug.Log("Creating new planet with name \"" + newPlanetName.text + "\"");
        string presetName = presetsToggleGroup.GetFirstActiveToggle().GetComponent<PlanetCard>().name;
        ResourceManager.Instance.CreatePlanet(presetName, newPlanetName.text);

        myPlanetsSelection.SetActive(true);
        presetsSelection.SetActive(false);
        newPlanetPrompt.SetActive(false);
        editorMenu.SetActive(true);
        gameObject.SetActive(false);
        yield return 0;
    }

    public void OpenSelectedPlanet()
    {
        Destroy(GameObject.Find("Planet"));
        StartCoroutine(Coroutine_OpenSelectedPlanet());
    }

    private IEnumerator Coroutine_OpenSelectedPlanet()
    {
        yield return null;
        ResourceManager.Instance.LoadPlanet(selectedPlanetCard.name, false);
        editorMenu.SetActive(true);
        gameObject.SetActive(false);
        yield return 0;
    }

    public void RenameSelectedPlanet()
    {
        string newName = renamePlanetName.text;
        ResourceManager.Instance.RenamePlanet(selectedPlanetCard.name, newName);
        selectedPlanetCard.SetName(newName);
    }

    public void DuplicateSelectedPlanet()
    {
        string newPlanetName = ResourceManager.Instance.DuplicatePlanet(selectedPlanetCard.name);
        RefreshMyPlanets();
        myPlanetsGrid.transform.Find(newPlanetName).GetComponent<Toggle>().isOn = true;
    }

    public void DeleteSelectedPlanet()
    {
        if (Planet.Instance && Planet.Instance.PlanetName == selectedPlanetCard.name)
        {
            Destroy(Planet.Instance.gameObject);
            backButton.gameObject.SetActive(false);
        }

        ResourceManager.Instance.DeletePlanet(selectedPlanetCard.name);
        RefreshMyPlanets();
    }

}
