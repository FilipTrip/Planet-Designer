using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EasyPoolKit.Demo
{
    public class Demo1Manager : MonoBehaviour
    {
        public GameObject Player;

        public GameObject[] TileTemplates;

        public Transform SceneRoot;

        private List<GameObject> _sceneTiles = new List<GameObject>(256);

        private int _sceneSize = 15;

        private int _lastPlayerZ = 0;

        private float _speed = 3f;
        
        private void Start()
        {
            for (int i = 0; i < _sceneSize; i++)
            {
                for (int j = 0; j < _sceneSize; j++)
                {
                    var newTile = SimpleGOPoolKit.Instance.SimpleSpawn(TileTemplates[GetRandomTileId()]);
                    _sceneTiles.Add(newTile);
                    
                    newTile.transform.SetParent(SceneRoot);
                    newTile.transform.localPosition = new Vector3(i - _sceneSize * 0.5f, 0, j - _sceneSize * 0.5f);
                }
            }

            Player.transform.position = new Vector3(0, 0.6f, 0);
            _lastPlayerZ = 0;
        }

        private int GetRandomTileId()
        {
            return Random.Range(0, TileTemplates.Length);
        }

        private void Update()
        {
            var lastPlayerPos = Player.transform.position;
            _speed = Mathf.Min(20, _speed + 0.1f);
            var newPlayerPos = lastPlayerPos + Vector3.forward * (Time.deltaTime * _speed);
            Player.transform.position = newPlayerPos;
            
            TryRemoveTiles(newPlayerPos.z);
            TryCreateNewTiles(newPlayerPos.z);
        }

        private void TryRemoveTiles(float playerPosZ)
        {
            for (int i = _sceneTiles.Count - 1; i >= 0; i--)
            {
                var tile = _sceneTiles[i];
                var tilePosZ = tile.transform.position.z;

                if (playerPosZ - tilePosZ > _sceneSize * 0.5f)
                {
                    _sceneTiles.RemoveAt(i);
                    SimpleGOPoolKit.Instance.Despawn(tile);
                }
            }
        }

        private void TryCreateNewTiles(float playerPosZ)
        {
            var curPosZ = Mathf.CeilToInt(playerPosZ);
            if (curPosZ > _lastPlayerZ)
            {
                for (int i = 0; i < _sceneSize; i++)
                {
                    for (int j = _sceneSize + _lastPlayerZ ; j < _sceneSize + curPosZ; j++)
                    {
                        var newTile = SimpleGOPoolKit.Instance.SimpleSpawn(TileTemplates[GetRandomTileId()]);
                        _sceneTiles.Add(newTile);
                    
                        newTile.transform.SetParent(SceneRoot);
                        newTile.transform.localPosition = new Vector3(i - _sceneSize * 0.5f, 0, j - _sceneSize * 0.5f);
                    }
                }

                _lastPlayerZ = curPosZ;
            }
        }
    }
}
