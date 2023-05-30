using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance { get; private set; }

    [SerializeField] private GameObject margin;
    [SerializeField] private GameObject startMenu;
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private EditorMenu editorMenu;
    [SerializeField] private List<Object> planetControlOverriders;

    public MainMenu MainMenu => mainMenu;
    public bool OverridingPlanetControl => planetControlOverriders.Count > 0;

    private void Awake()
    {
        Instance = this;
        startMenu.SetActive(false);
        editorMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }

    public void AddPlanetControlOverrider(Object obj)
    {
        planetControlOverriders.Add(obj);
    }

    public void RemovePlanetControlOverrider(Object obj)
    {
        planetControlOverriders.Remove(obj);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && Input.GetKey(KeyCode.LeftControl))
            margin.SetActive(!margin.activeSelf);
    }

}
