using UnityEngine;

namespace EasyPoolKit.Demo
{
    public class DemoRecyclableBall : RecyclableMonoBehaviour
    {
        public Transform HeadRoot;
        public Color HitColor { get; set; } = Color.black;
        private bool _ifActive = false;
        private float _moveSpeed = 0f;
        private int _hitTextHash = 0;
        
        public override void OnObjectDespawn()
        {
            base.OnObjectDespawn();

            HitColor = Color.black;
            _ifActive = false;
            transform.localPosition = Vector3.zero;
            transform.forward = Vector3.forward;
            _hitTextHash = 0;
        }

        public void Shoot(Vector3 position, Vector3 forwardDir, float speed, int hitTextHash)
        {
            transform.position = position;
            transform.forward = forwardDir;
            _moveSpeed = speed;
            _hitTextHash = hitTextHash;
            _ifActive = true;
        }

        public void Update()
        {
            if (_ifActive)
            {
                transform.position += transform.forward * Time.deltaTime * _moveSpeed;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_ifActive)
            {
                if (RecyclableGOPoolKit.Instance.TrySpawn<DemoHitText>(_hitTextHash, out var hitText))
                {
                    hitText.transform.SetParent(transform.parent);
                    hitText.transform.position = HeadRoot.position;
                    hitText.Text.color = HitColor;
                }
                DespawnSelf();
            }
        }
    }
}