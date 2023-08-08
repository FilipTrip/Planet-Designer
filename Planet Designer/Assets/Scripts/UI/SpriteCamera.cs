using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class SpriteCamera : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private GeographicTransform geographicTransform;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private float distance;
    [SerializeField] private float captureCooldown;
    [SerializeField] private Vector2Int resolution;
    
    private float cooldown;
    private bool tryCapture;

    private void Awake()
    {
        Planet.Loaded.AddListener(OnPlanetLoaded);
    }

    private void OnPlanetLoaded()
    {
        Planet.RegenerationCompleted.AddListener(TryCapture);
        TryCapture();
    }

    private void Update()
    {
        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;

            if (cooldown < 0f)
                cooldown = 0f;
        }

        if (tryCapture && cooldown == 0f)
        {
            Capture();
            cooldown = captureCooldown;
        }

        tryCapture = false;
    }

    public void TryCapture()
    {
        tryCapture = true;
    }

    [ContextMenu("Capture")]
    public void Capture()
    {
        if (!Planet.Instance)
            return;

        Stopwatch stopwatch = Stopwatch.StartNew();

        geographicTransform.magnitude = Planet.Instance.SurfaceSettings.terrainRadius * distance;
        geographicTransform.UpdateTransform();
        transform.LookAt(Vector3.zero);

        Rect sourceRect = new Rect(0, 0, resolution.x, resolution.y);

        RenderTexture renderTexture = new RenderTexture(resolution.x, resolution.y, 24);
        Texture2D texture = new Texture2D(resolution.x, resolution.y, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;

        camera.enabled = true;
        camera.targetTexture = renderTexture;
        camera.Render();
        camera.enabled = false;

        RenderTexture currentRenderTexture = RenderTexture.active;
        RenderTexture.active = renderTexture;
        texture.ReadPixels(sourceRect, 0, 0);
        texture.Apply();

        camera.targetTexture = null;
        RenderTexture.active = currentRenderTexture;
        Destroy(renderTexture);

        resourceManager.UpdatePlanetSprite(Planet.Instance.PlanetName, texture);

        stopwatch.Stop();
        Debug.Log("New thumbnail captured for " + Planet.Instance.PlanetName + " (" + stopwatch.ElapsedMilliseconds + "ms)");
    }
}
