using UnityEngine;

namespace EasyPoolKit
{
    public class RecyclableObjectPoolViewer : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
