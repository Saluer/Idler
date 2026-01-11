using System.Collections;
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
        
        [SerializeField] private EnemyLevelConfig config;
        [SerializeField] private Transform spawnPosition;

        private void Awake()
        {
            _player = FindFirstObjectByType<PlayerScript>();
            _targetCollider = _player.GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject != _targetCollider.gameObject) return;

            StartCoroutine(Spawn(config.enemyCount));
        }

        private IEnumerator Spawn(int times)
        {
            for (var i = 0; i < times; i++)
            {
                var spawnPositionPosition = spawnPosition.position;
                spawnPositionPosition = new Vector3(spawnPositionPosition.x + Random.Range(-5, 5),
                    spawnPositionPosition.y, spawnPositionPosition.z);
                var enemy = Instantiate(config.prefab, spawnPositionPosition, Quaternion.identity)
                    .GetComponent<EnemyScript>();
                enemy.player = _player;
                yield return new WaitForSeconds(0.1f);
            }

            gameObject.SetActive(false);
        }
    }
}