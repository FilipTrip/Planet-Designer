using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyPoolKit.Demo
{
    public class Demo4RecyclableBall : RecyclableMonoBehaviour
    {
        public Transform HeadRoot;
        public Color HitColor { get; set; } = Color.black;
        private bool _ifActive = false;
        private float _moveSpeed = 0f;
        private int _hitTextHash = 0;
        private Action<Demo4RecyclableBall> _onCollide = null;
        
        public override void OnObjectDespawn()
        {
            base.OnObjectDespawn();

            HitColor = Color.black;
            _ifActive = false;
            transform.localPosition = Vector3.zero;
            transform.forward = Vector3.forward;
            _hitTextHash = 0;
            _onCollide = null;
        }

        public void Shoot(Vector3 position, Vector3 forwardDir, float speed, int hitTextHash, Action<Demo4RecyclableBall> onCollide)
        {
            transform.position = position;
            transform.forward = forwardDir;
            _moveSpeed = speed;
            _hitTextHash = hitTextHash;
            _ifActive = true;
            _onCollide = onCollide;
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
                _onCollide?.Invoke(this);
                DespawnSelf();
            }
        }
    }
}