using System.Collections;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Collider))]
    //todo add changing of name, enemy preview, enemy configs dynamically based on list of configs
    public class SpawnerScript : MonoBehaviour
    {
        private PlayerScript _player;
        private Collider _targetCollider;
        private Collider _collider;

        public bool _triggerActivated;
        
        public EnemyLevelConfig config;
        public Vector3 spawnPosition;

        private void Awake()
        {
            _player = FindFirstObjectByType<PlayerScript>();
            _targetCollider = _player.GetComponent<Collider>();
        }

        private void Start()
        {
            var displayName = gameObject.AddComponent<TextMeshPro>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject != _targetCollider.gameObject || _triggerActivated) return;

            _triggerActivated = true;
            StartCoroutine(Spawn(config.enemyCount));
        }

        private IEnumerator Spawn(int times)
        {
            for (var i = 0; i < times; i++)
            {
                var spawnPositionPosition = spawnPosition;
                spawnPositionPosition = new Vector3(spawnPositionPosition.x + Random.Range(-5, 5),
                    spawnPositionPosition.y, spawnPositionPosition.z);
                var enemy = Instantiate(config.enemyPrefab, spawnPositionPosition, Quaternion.identity)
                    .GetComponent<EnemyScript>();
                enemy.player = _player;
                yield return new WaitForSeconds(0.1f);
            }

            gameObject.SetActive(false);
        }
    }
}