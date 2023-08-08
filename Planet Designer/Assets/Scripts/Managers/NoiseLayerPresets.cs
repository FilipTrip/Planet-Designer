using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Noise Layer Presets", menuName = "Planet Designer/Noise Layer Presets")]
public class NoiseLayerPresets : ScriptableObject
{
    [System.Serializable]
    public struct Preset
    {
        public string name;
        public NoiseLayer noiseLayer;
    }

    [SerializeField] public Preset[] presets;

    public NoiseLayer Get(string name)
    {
        foreach (Preset preset in presets)
        {
            if (preset.name == name)
                return preset.noiseLayer;
        }

        return null;
    }
}
