using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalForestSettings : ForestSettings
{
    [Range(0f, 200f)]
    [Tooltip("Determines the amount of attempts to place trees. More attemps mean more trees")]
    public float density = 100;

}