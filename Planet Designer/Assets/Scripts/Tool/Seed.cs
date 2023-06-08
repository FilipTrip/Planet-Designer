using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Seed
{
    [Tooltip("The seed used by a certain feature or algorithm. Recommended range is within -10 000 and 10 000")]
    public int value;

    public int New()
    {
        return value = Random.Range(-10000, 10000);
    }

}
