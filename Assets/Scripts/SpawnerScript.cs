using System.Collections;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Collider))]
    public class SpawnerScript : MonoBehaviour
    {
        private PlayerScript _player;
        private Collider _targetCollider;
        private Collider _collider;

        public bool triggerActivated { private set; get; }

        public EnemyLevelConfig config;

        public Vector3 spawnPosition;

        private void Awake()
        {
            _player = FindFirstObjectByType<PlayerScript>();
            _targetCollider = _player.GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject != _targetCollider.gameObject) return;

            StartCoroutine(SpawnAll(config));
        }

        public IEnumerator SpawnAll(EnemyLevelConfig enemyLevelConfig)
        {
            if (triggerActivated)
            {
                yield break;
            }

            triggerActivated = true;

            foreach (var enemyWrapper in enemyLevelConfig.enemies)
            {
                for (var i = 0; i < enemyWrapper.count; i++)
                {
                    var spawnPositionPosition = spawnPosition;
                    spawnPositionPosition = new Vector3(spawnPositionPosition.x + Random.Range(-5, 5),
                        spawnPositionPosition.y, spawnPositionPosition.z);
                    var enemy = Instantiate(enemyWrapper.enemyPrefab, spawnPositionPosition, Quaternion.identity)
                        .GetComponent<EnemyScript>();
                    enemy.player = _player;
                    yield return new WaitForSeconds(0.5f);
                }
            }

            gameObject.SetActive(false);
        }
    }
}