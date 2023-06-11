using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Planet Designer/Zone Settings")]
public class ZoneSettings : ScriptableObject
{
    [Range(0f, 5f)]
    [SerializeField] public float pointAngle = 1;
    [SerializeField] public List<Vector3> points = new List<Vector3>();

}
