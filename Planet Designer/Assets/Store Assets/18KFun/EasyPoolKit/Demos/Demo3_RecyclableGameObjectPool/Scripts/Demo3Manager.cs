using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EasyPoolKit.Demo
{
    public class Demo3Manager : MonoBehaviour
    {
        public GameObject PlayerTemplate;
        public GameObject HurtTemplate;
        public Transform BallRoot;
        public Transform BornPoint;
        
        List<Coroutine> _shootCoroutines = new List<Coroutine>();
        private void Awake()
        {
            RecyclableGOPoolKit.Instance.RegisterPrefab(PlayerTemplate);

            var hurtTextConfig = new RecyclablePoolConfig
            {
                ObjectType = RecycleObjectType.RecyclableGameObject,
                ReferenceType = typeof(DemoHitText),
                PoolId = "HurtTextPool",
                InitCreateCount = 20,
                ReachMaxLimitType = PoolReachMaxLimitType.RecycleOldest,
                MaxSpawnCount = 30,
                DespawnDestroyType = PoolDespawnDestroyType.DestroyToLimit,
                MaxDespawnCount = 25,
                ClearType = PoolClearType.ClearToLimit,
                AutoClearTime = 0.5f,
                IfIgnoreTimeScale = false,
            };
            RecyclableGOPoolKit.Instance.RegisterPrefab(HurtTemplate, hurtTextConfig);
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
                var newPlayer = RecyclableGOPoolKit.Instance.SimpleSpawn<DemoRecyclableBall>(PlayerTemplate);
                newPlayer.transform.SetParent(BallRoot);
                newPlayer.HitColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                newPlayer.Shoot(BornPoint.position, shootDir, moveSpeed, HurtTemplate.GetInstanceID());
                yield return waitTime;
            }
        }
    }
}
