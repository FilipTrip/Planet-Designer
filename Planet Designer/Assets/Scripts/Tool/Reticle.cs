using System.Collections;
using System.Collections.Generic;
using KevinCastejon.ConeMesh;
using UnityEngine;

public class Reticle : MonoBehaviour
{
    [SerializeField] private GeographicTransform geographicTransform;
    [SerializeField] private LayerMask raycastLayerMask;
    [SerializeField] private GameObject pin;
    [SerializeField] private GameObject brush;
    [SerializeField] private Range brushAngleRange;
    [SerializeField] private float brushAngleSpeed;

    private Vector3 originalPinScale;
    private float brushAngle;
    private Material brushMaterial;

    public static Reticle Instance { get; private set; }
    public GeographicTransform GeographicTransform => geographicTransform;
    public bool OnPlanetSurface => pin.activeSelf;
    public float BrushAngle => brushAngle;

    private void Awake()
    {
        Instance = this;
        originalPinScale = pin.transform.localScale;
        brushAngle = brushAngleRange.Mid();
        brushMaterial = brush.GetComponent<MeshRenderer>().material;
        SetBrushColor(Color.white);
    }

    private void Update()
    {
        // Disable if controlling camera or overridden by canvas

        if (CameraController.Instance.BeingControlled || CanvasManager.Instance.OverridingPlanetControl)
        {
            SetPinVisibility(false);
            SetBrushVisibility(false);
            return;
        }

        // Update visibility

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;

        if (Physics.Raycast(ray, out raycastHit, 3000f, raycastLayerMask))
        {
            SetPinVisibility(true);
            SetBrushVisibility(FeatureManager.Instance.SelectedFeature);
        }
        else
        {
            SetPinVisibility(false);
            SetBrushVisibility(false);
            return;
        }

        // Update position

        geographicTransform.Coordinates = GeographicCoordinates.FromPosition(raycastHit.point);
        geographicTransform.UpdateTransform();

        // Update pin scale

        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        pin.transform.localScale = originalPinScale * distance;

        // If brush is visible
        if (FeatureManager.Instance.SelectedFeature && FeatureManager.Instance.SelectedFeature.EnableBrush)
        {
            // Update brush angle

            if (!CameraController.Instance.BeingControlled && Input.mouseScrollDelta != Vector2.zero)
            {
                brushAngle -= Input.mouseScrollDelta.y * brushAngle * brushAngleSpeed;
                brushAngle = brushAngleRange.Clamp(brushAngle);
            }

            // Update brush scale

            float planetRadius = Planet.Instance.SurfaceSettings.terrainRadius;
            float brushRadius = (new Vector2(0, planetRadius) - new Vector2(0, planetRadius).RotateAroundZero(brushAngle * Mathf.Deg2Rad)).x;
            brush.transform.localScale = new Vector3(brushRadius * 2f, planetRadius, brushRadius * 2f);

            // Update brush color

            if (!FeatureManager.Instance.SelectedFeature)
                SetBrushColor(new Color(1f, 1f, 1f, 2f));

            else if (Input.GetKey(KeyCode.LeftShift))
                SetBrushColor(new Color(1f, 0f, 0f, 2f));

            else
                SetBrushColor(new Color(0f, 0.9f, 0f, 2f));
        }
    }

    private void SetPinVisibility(bool visible)
    {
        if (pin.activeSelf == !visible)
        {
            pin.SetActive(visible);
            Cursor.visible = !visible;
        }
    }

    private void SetBrushVisibility(bool visible)
    {
        if (brush.activeSelf == !visible)
        {
            brush.SetActive(visible);
        }
    }

    private void SetBrushColor(Color color)
    {
        brushMaterial.SetColor("_Emission", color);
    }

}
