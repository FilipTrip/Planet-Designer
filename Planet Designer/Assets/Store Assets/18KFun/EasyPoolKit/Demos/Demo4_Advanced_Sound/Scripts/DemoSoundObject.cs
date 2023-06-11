using UnityEngine;

namespace EasyPoolKit.Demo
{
    public class DemoSoundObject : RecyclableMonoBehaviour
    {
        public AudioSource Audio;
        public bool _ifAutoDespawn = false;
        private AudioClip _clip;
        private float? _autoDespawnTime = null;
        private float _playedTime;
        
        public void Play(AudioClip clip, bool ifAutoDespawn)
        {
            _clip = clip;
            _ifAutoDespawn = ifAutoDespawn;
            Audio.clip = _clip;
            Audio.Play();

            _playedTime = 0;
            
            if (_ifAutoDespawn)
            {
                _autoDespawnTime = _clip.length;
            }
            else
            {
                _autoDespawnTime = null;
            }
        }

        public override void OnObjectDespawn()
        {
            base.OnObjectDespawn();
            Audio.Stop();
            
            Audio.clip = null;
            _clip = null;
            _playedTime = 0;
            _ifAutoDespawn = true;
            _autoDespawnTime = null;
        }

        public override void OnObjectUpdate(float deltaTime)
        {
            base.OnObjectUpdate(deltaTime);

            if (!_ifAutoDespawn)
            {
                return;   
            }

            if (!_autoDespawnTime.HasValue)
            {
                return;
            }

            _playedTime += deltaTime;
            
            if (_playedTime >= _autoDespawnTime.Value)
            {
                DespawnSelf();
            }
        }
    }
}
