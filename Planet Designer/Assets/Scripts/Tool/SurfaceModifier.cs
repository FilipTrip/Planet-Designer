using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SurfaceModifier
{
    [Tooltip("Enable or disable")]
    public bool enabled = true;

    public abstract void Run(Sphere sphere);

}
