using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private Light sceneLight;
    [SerializeField] private Light cameraLight;
    [SerializeField] private Camera camera;
    [SerializeField] private LineRenderer lineRenderer;

    [Range(1.1f, 16f)]
    [SerializeField] private float distance;

    [Range(0.01f, 2f)]
    [SerializeField] private float scrollSpeed;

    [SerializeField] private AnimationCurve lightIntensityOverDistance;
    [SerializeField] private AnimationCurve fieldOfViewOverDistance;

    [SerializeField] private bool requireMouseButtonDown;

    private Vector3 previousPosition;
    private bool beingControlled;

    public bool BeingControlled => beingControlled;

    private void Awake()
    {
        Instance = this;
        Planet.Loaded.AddListener(OnPlanetLoaded);
        UpdateLightAndFOV();
    }

    private void OnPlanetLoaded()
    {
        UpdateMagnitude();
        Planet.Instance.TerrainSphere.RegenerationCompleted.AddListener(UpdateMagnitude);
    }

    private void OnValidate()
    {
        UpdateMagnitude();
        UpdateLightAndFOV();
    }

    private void UpdateMagnitude()
    {
        if (!Planet.Instance)
            return;

        transform.position = transform.position.normalized * Planet.Instance.SurfaceSettings.terrainRadius * distance;
    }

    private void UpdateLightAndFOV()
    {
        cameraLight.intensity = lightIntensityOverDistance.Evaluate(distance);
        cameraLight.intensity *= Vector3.Dot(sceneLight.transform.forward, cameraLight.transform.forward).Remapped(-1f, 1f, 1f, 0f).Smoothstep();
        camera.fieldOfView = fieldOfViewOverDistance.Evaluate(distance);

        camera.farClipPlane = transform.position.magnitude;

        if (Planet.Instance)
            camera.farClipPlane += Planet.Instance.SurfaceSettings.terrainRadius * 0.5f;
    }

    private void Update()
    {
        if (!Planet.Instance)
            return;

        UpdateLightAndFOV();

        if (!beingControlled && !CanvasManager.Instance.OverridingPlanetControl)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                beingControlled = true;
                lineRenderer.enabled = true;
                EditorMenu.Instance.ShowCameraControlText(true);

                if (!requireMouseButtonDown)
                {
                    previousPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                }
            }
        }

        else
        {
            if (!Input.GetKey(KeyCode.Space) || CanvasManager.Instance.OverridingPlanetControl)
            {
                if (!requireMouseButtonDown || (!Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0)) || CanvasManager.Instance.OverridingPlanetControl)
                {
                    beingControlled = false;
                    lineRenderer.enabled = false;
                    EditorMenu.Instance.ShowCameraControlText(false);
                }
            } 
        }

        if (!beingControlled)
            return;

        // If scrolling
        if (Input.mouseScrollDelta != Vector2.zero)
        {
            distance -= Input.mouseScrollDelta.y * distance * scrollSpeed;
            distance = Mathf.Clamp(distance, 1.1f, 16f);

            UpdateMagnitude();
        }

        // If left mouse button pressed
        if (requireMouseButtonDown && Input.GetMouseButtonDown(0))
        {
            previousPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        }

        // If left mouse button held down
        if (!requireMouseButtonDown || Input.GetMouseButton(0))
        {
            Vector3 viewportPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            Vector3 direction = previousPosition - viewportPoint;
            previousPosition = viewportPoint;

            transform.position = Vector3.zero;
            transform.Rotate(new Vector3(1f, 0f, 0f), direction.y * 180f);
            transform.Rotate(new Vector3(0f, 1f, 0f), direction.x * -180f, Space.World);
            transform.Translate(new Vector3(0f, 0f, Planet.Instance.SurfaceSettings.terrainRadius * -distance));
            
        }
    }

}
