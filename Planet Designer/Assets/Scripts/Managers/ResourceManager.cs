using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [SerializeField] private GameObject planetPrefab;
    private char slash;

    private void Awake()
    {
        Instance = this;

        if (Application.platform == RuntimePlatform.OSXEditor)
            slash = '/';
        else
            slash = '\\';
    }

    /// <summary>
    /// Creats a new planet folder by duplicating the a preset
    /// </summary>
    public void CreatePlanet(string presetName, string planetName)
    {
#if UNITY_EDITOR
        AssetDatabase.CopyAsset(
        "Assets" + slash + "Resources" + slash + "Presets" + slash + presetName,
        "Assets" + slash + "Resources" + slash + "Planets" + slash + planetName);
        LoadPlanet(planetName, true);
#endif
    }

    /// <summary>
    /// Returns the names of all subdirectories in the provided folder
    /// </summary>
    public string[] GetSubdirectoryNames(string folder)
    {
        string[] names = Directory.GetDirectories(Application.dataPath + slash + "Resources" + slash + folder);

        for (int i = 0; i < names.Length; ++i)
        {
            names[i] = names[i].Substring(names[i].LastIndexOf(slash) + 1);
        }

        return names;
    }

    /// <summary>
    /// Returns a sprite created using a stored Texture2D
    /// </summary>
    public Sprite GetPlanetSprite(string folder, string planetName)
    {
        Texture2D texture = Resources.Load<Texture2D>(folder + slash + planetName + slash + "Sprite");

        if (texture == null)
            return null;
        else
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// Replaces a planet sprite
    /// </summary>
    public void UpdatePlanetSprite(string planetName, Texture2D texture)
    {
#if UNITY_EDITOR
        string spritePath = "Assets" + slash + "Resources" + slash + "Planets" + slash + planetName + slash + "Sprite.asset";
        AssetDatabase.DeleteAsset(spritePath);
        AssetDatabase.CreateAsset(texture, spritePath);
#endif
    }

    /// <summary>
    /// Instantiates a planet prefab, initializes it, and generates the planet
    /// </summary>
    public void LoadPlanet(string planetName, bool newPlanet)
    {
        string planetFolder = "Planets" + slash + planetName + slash;

        // Instantiate and initialize planet
        Planet planet = Instantiate(planetPrefab).GetComponent<Planet>();
        planet.gameObject.name = "Planet";
        planet.PlanetName = planetName;
        planet.Initialize();

        // Link settings and material resources
        planet.OceanSphere.Settings = Resources.Load<SphereSettings>(planetFolder + "Ocean Settings");
        planet.TerrainSphere.Settings = Resources.Load<SphereSettings>(planetFolder + "Terrain Settings");

        planet.OceanSphere.Settings.material = Resources.Load<Material>(planetFolder + "Ocean Material");
        planet.TerrainSphere.Settings.material = Resources.Load<Material>(planetFolder + "Terrain Material");

        planet.OceanSphere.Settings.SetSphere(planet.OceanSphere);
        planet.TerrainSphere.Settings.SetSphere(planet.TerrainSphere);

        // Load features
#if UNITY_EDITOR
        string[] featurePaths = AssetDatabase.GetSubFolders("Assets" + slash + "Resources" + slash + "Planets" + slash + planetName + slash + "Features");

        foreach (string featurePath in featurePaths)
        {
            string featureName = RemovePath(featurePath);
            string[] featureContents = Directory.GetFiles(featurePath);

            for (int i = 0; i < featureContents.Length; ++i)
            {
                featureContents[i] = RemoveExtension(RemovePath(featureContents[i]));
            }

            if (featureContents.Contains("Local Forest Settings"))
            {
                LocalForestSettings forestSettings = Resources.Load<LocalForestSettings>(featurePath.Replace("Assets/Resources/", "") + "/Local Forest Settings");
                ZoneSettings zoneSettings = Resources.Load<ZoneSettings>(featurePath.Replace("Assets/Resources/", "") + "/Zone Settings");
                FeatureManager.Instance.AddLocalForest(featureName, forestSettings, zoneSettings);
            }

            else if (featureContents.Contains("Global Forest Settings"))
            {
                GlobalForestSettings forestSettings = Resources.Load<GlobalForestSettings>(featurePath.Replace("Assets/Resources/", "") + "/Global Forest Settings");
                FeatureManager.Instance.AddGlobalForest(featureName, forestSettings);
            }

            else
            {
                Debug.LogWarning("Feature could not be classified: " + featurePath);
            }
        }
#endif

        // Randomize seeds
        if (newPlanet)
            planet.RandomizeSeeds();

        // Generate planet
        planet.Regenerate();
        Planet.Loaded.Invoke();

        #if UNITY_EDITOR
        Selection.activeObject = planet.TerrainSphere.Settings;
        //EditorGUIUtility.PingObject(planet.TerrainSphere.gameObject);
        #endif
    }

    /// <summary>
    /// Deletes a planet folder
    /// </summary>
    public void DeletePlanet(string planetName)
    {
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset("Assets" + slash + "Resources" + slash + "Planets" + slash + planetName);
#endif
    }

    /// <summary>
    /// Renames a planet folder
    /// </summary>
    public void RenamePlanet(string planetName, string newPlanetName)
    {
#if UNITY_EDITOR
        AssetDatabase.RenameAsset("Assets" + slash + "Resources" + slash + "Planets" + slash + planetName, newPlanetName);
#endif
    }

    /// <summary>
    /// Duplicates a planet folder and returns the duplicate's name
    /// </summary>
    public string DuplicatePlanet(string planetName)
    {
        // CopyAsset does not work if duplicate already exists !!!

#if UNITY_EDITOR
        AssetDatabase.CopyAsset("Assets" + slash + "Resources" + slash + "Planets" + slash + planetName, "Assets" + slash + "Resources" + slash + "Planets" + slash + planetName + " (copy)");
#endif
        return planetName + " (copy)";
    }

    /// <summary>
    /// Renames a feature folder
    /// </summary>
    public void RenameFeature(string planetName, string featureName, string newFeatureName)
    {
#if UNITY_EDITOR
        AssetDatabase.RenameAsset("Assets" + slash + "Resources" + slash + "Planets" + slash + planetName + slash + "Features" + slash + featureName, newFeatureName);
#endif
    }

    /// <summary>
    /// Deletes a feature of a planet
    /// </summary>
    public void DeleteFeature(string planetName, string featureName)
    {
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset("Assets" + slash + "Resources" + slash + "Planets" + slash + planetName + slash + "Features" + slash + featureName);
#endif
    }

    /// <summary>
    /// Returns the file name and extension without the path
    /// </summary>
    private string RemovePath(string filePath)
    {
        return filePath.Substring(filePath.LastIndexOf('/') + 1);
    }

    /// <summary>
    /// Returns the file name without the extension
    /// </summary>
    private string RemoveExtension(string fileName)
    {
        return fileName.Remove(fileName.LastIndexOf('.'));
    }

    /// <summary>
    /// Creates a folder with data for a local forest and returns the feature's name
    /// </summary>
    public string CreateLocalForest(string planetName, out LocalForestSettings forestSettings, out ZoneSettings zoneSettings)
    {
        zoneSettings = ScriptableObject.CreateInstance<ZoneSettings>();
        forestSettings = ScriptableObject.CreateInstance<LocalForestSettings>();

#if UNITY_EDITOR
        string folderGUID = AssetDatabase.CreateFolder("Assets" + slash + "Resources" + slash + "Planets" + slash + planetName + slash + "Features", "Local Forest");
        string folderPath = AssetDatabase.GUIDToAssetPath(folderGUID);

        AssetDatabase.CreateAsset(zoneSettings, folderPath + slash + "Zone Settings.asset");
        AssetDatabase.CreateAsset(forestSettings, folderPath + slash + "Local Forest Settings.asset");
        forestSettings.seed.New();
        return folderPath.Substring(folderPath.LastIndexOf('/') + 1);
#endif
        return null;
    }

    /// <summary>
    /// Creates a folder with data for a global forest and returns the feature's name
    /// </summary>
    public string CreateGlobalForest(string planetName, out GlobalForestSettings forestSettings)
    {
        forestSettings = ScriptableObject.CreateInstance<GlobalForestSettings>();

#if UNITY_EDITOR
        string folderGUID = AssetDatabase.CreateFolder("Assets" + slash + "Resources" + slash + "Planets" + slash + planetName + slash + "Features", "Global Forest");
        string folderPath = AssetDatabase.GUIDToAssetPath(folderGUID);

        AssetDatabase.CreateAsset(forestSettings, folderPath + slash + "Global Forest Settings.asset");
        forestSettings.seed.New();
        return folderPath.Substring(folderPath.LastIndexOf('/') + 1);
#endif
        return null;
    }

}
