using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Selectable selectable;
    private bool selectionUpdatedThisFrame;
    private Reticle reticle;

    public static SelectionManager Instance { get; private set; }
    public Selectable Selectable => selectable;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // Deselect if clicking outside the planet
        if (selectable && !Reticle.Instance.OnPlanetSurface && !CameraController.Instance.BeingControlled && Input.GetMouseButtonUp(0))
            Select(null);

        // Deselect if pressing ESC
        if (selectable && Input.GetKeyDown(KeyCode.Escape))
            Select(null);

        // Delete if pressing Backspace
        if (selectable && Input.GetKeyDown(KeyCode.Backspace))
        {
            Selectable toBeDeleted = selectable;
            Select(null);
            toBeDeleted.Delete();
            FeatureOverview.Instance.Refresh();
        }

        // Try to select if pressing on planet
        if (!selectable && Reticle.Instance.OnPlanetSurface && !CameraController.Instance.BeingControlled && Input.GetMouseButtonUp(0))
        {
            TrySelect();
        }

        // Only change selection once per frame
        if (selectionUpdatedThisFrame)
            selectionUpdatedThisFrame = false;

        // Update selected selectable
        if (selectable)
            selectable.WhileSelected();
    }

    public void Select(Selectable selectable)
    {
        // Only select one selectable each frame
        if (selectionUpdatedThisFrame)
            return;

        selectionUpdatedThisFrame = true;

        // Deselect current selectable
        if (this.selectable)
        {
            this.selectable.Selected = false;
            this.selectable.OnDeselected();

            // Return if trying to select the already selected selectable
            if (this.selectable == selectable)
            {
                this.selectable = null;
                return;
            }
            else
            {
                this.selectable = null;
            }
        }

        // Select new selectable
        if (selectable)
        {
            this.selectable = selectable;
            selectable.Selected = true;
            selectable.OnSelected();
        }
    }

    private void TrySelect()
    {
        Debug.Log("Trying to select");

        Selectable s;
        foreach (Transform feature in Planet.Instance.FeaturesParent)
        {
            if (s = feature.GetComponent<Selectable>())
            {
                if (s.CheckClickedOn())
                {
                    Select(s);
                    return;
                }
                else
                {
                    Debug.Log(s.name + " was not selected");
                }
            }
        }
    }
}
