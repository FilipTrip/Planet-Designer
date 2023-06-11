using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EasyPoolKit.Demo
{
    public class Demo4Manager : MonoBehaviour
    {
        public GameObject PlayerTemplate;
        public GameObject HurtTemplate;
        public GameObject SoundTemplate;
        public AudioClip CollideAudioClip;
        
        public Transform BallRoot;
        public Transform SoundRoot;
        public Transform BornPoint;

        private RecyclableGOPoolManagerBase _poolManager;
        List<Coroutine> _shootCoroutines = new List<Coroutine>();
        
        private void Awake()
        {
            _poolManager = RecyclableGOPoolKit.Instance;
            _poolManager.RegisterPrefab(PlayerTemplate);

            var hurtTextConfig = new RecyclablePoolConfig
            {
                ObjectType = RecycleObjectType.RecyclableGameObject,
                ReferenceType = typeof(Demo4HitText),
                PoolId = "HurtTextPool",
                InitCreateCount = 20,
                ReachMaxLimitType = PoolReachMaxLimitType.RejectNull,
                MaxSpawnCount = 30,
                DespawnDestroyType = PoolDespawnDestroyType.DestroyToLimit,
                MaxDespawnCount = 25,
                ClearType = PoolClearType.ClearToLimit,
                AutoClearTime = 0.5f,
                IfIgnoreTimeScale = false,
            };
            _poolManager.RegisterPrefab(HurtTemplate, hurtTextConfig);
            
            //Register sound pool
            var soundPoolConfig = new RecyclablePoolConfig
            {
                ObjectType = RecycleObjectType.RecyclableGameObject,
                ReferenceType = typeof(DemoSoundObject),
                PoolId = "DemoSoundPool",
                InitCreateCount = 10,
                ReachMaxLimitType = PoolReachMaxLimitType.RecycleOldest,
                MaxSpawnCount = 10,
            };
            _poolManager.RegisterPrefab(SoundTemplate, soundPoolConfig);
        }

        private void Start()
        {
            CreateShootCoroutine();
        }

        private void CreateShootCoroutine()
        {
            for (int i = 0; i < 5; i++)
            {
                var shootCor = StartCoroutine(ShootBall(Random.Range(0.08f,0.1f), Random.Range(70f,80f)));
                _shootCoroutines.Add(shootCor);
            }
        }

        private void StopShootCoroutine()
        {
            foreach (var shootCor in _shootCoroutines)
            {
                StopCoroutine(shootCor);
            }
                
            _shootCoroutines.Clear();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_shootCoroutines.Count == 0)
                {
                    CreateShootCoroutine();
                }
                else
                {
                    StopShootCoroutine();
                }
            }
        }

        private IEnumerator ShootBall(float shootDeltaTime, float moveSpeed)
        {
            var waitTime = new WaitForSeconds(shootDeltaTime);
            
            while (true)
            {
                var shootDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1, 1f)).normalized;
                var newPlayer = _poolManager.SimpleSpawn<Demo4RecyclableBall>(PlayerTemplate);
                newPlayer.transform.SetParent(BallRoot);
                newPlayer.HitColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                newPlayer.Shoot(BornPoint.position, shootDir, moveSpeed, HurtTemplate.GetInstanceID(), (player) =>
                {
                    if (player)
                    {
                        var hitText = RecyclableGOPoolKit.Instance.SimpleSpawn<Demo4HitText>(HurtTemplate);
                        if(hitText)
                        {
                            hitText.transform.SetParent(BallRoot);
                            hitText.transform.position = player.HeadRoot.position;
                            hitText.Text.color = player.HitColor;
                        }

                        //Play sound from sound pool
                        if (RecyclableGOPoolKit.Instance.TrySpawn<DemoSoundObject>(SoundTemplate.GetInstanceID(), out var soundObj))
                        {
                            soundObj.transform.SetParent(SoundRoot);
                            soundObj.transform.position = player.transform.position;
                            soundObj.Play(CollideAudioClip, true);
                        }
                    }
                });
                yield return waitTime;
            }
        }
    }
}
