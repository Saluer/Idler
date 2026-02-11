using System.Collections;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Collider))]
    public class SpawnerScript : MonoBehaviour
    {
        private PlayerScript _player;
        private Collider _targetCollider;

        // состояние
        public bool triggerActivated { get; private set; }
        public bool IsSpawning { get; private set; }

        // конфигурация (инициализируется вручную)
        private EnemyLevelConfig _config;
        private Vector3 _spawnPosition;
        private float _spawnRadius;

        // тайминги
        private float _baseSpawnDelay;
        private float _minSpawnDelay;
        private float _killAccelerationMultiplier;
        private float _currentSpawnDelay;

        // счётчики
        public int TotalEnemies { get; private set; }
        public int SpawnedEnemies { get; private set; }
        public bool CanStart { get; private set; }

        private void Awake()
        {
            _player = FindFirstObjectByType<PlayerScript>();
            _targetCollider = _player.GetComponent<Collider>();
        }

        /// <summary>
        /// Обязательная инициализация. Без вызова — спавнер не работает.
        /// </summary>
        public void Init(
            EnemyLevelConfig config,
            Vector3 spawnPosition,
            float spawnRadius,
            float baseSpawnDelay = 0.5f,
            float minSpawnDelay = 0.1f,
            float killAccelerationMultiplier = 0.85f
        )
        {
            _config = config;
            _spawnPosition = spawnPosition;
            _spawnRadius = spawnRadius;

            _baseSpawnDelay = baseSpawnDelay;
            _minSpawnDelay = minSpawnDelay;
            _killAccelerationMultiplier = killAccelerationMultiplier;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject != _targetCollider.gameObject)
                return;

            Begin();
        }

        /// <summary>
        /// Старт волны (по триггеру или принудительно)
        /// </summary>
        public void Begin()
        {
            if (triggerActivated || _config == null)
                return;

            triggerActivated = true;
            foreach (var rend in GetComponentsInChildren<Renderer>())
            {
                rend.enabled = false;
            }

            StopAllCoroutines();
            StartCoroutine(SpawnAll());
        }
        
        public void NotifyEnemyKilled()
        {
            if (!IsSpawning)
                return;

            _currentSpawnDelay = Mathf.Max(
                _minSpawnDelay,
                _currentSpawnDelay * _killAccelerationMultiplier
            );
        }

        public void AllowStart()
        {
            CanStart = true;
        }


        public IEnumerator SpawnAll()
        {
            IsSpawning = true;

            TotalEnemies = _config.enemies.Sum(e => e.count);
            SpawnedEnemies = 0;
            _currentSpawnDelay = _baseSpawnDelay;

            foreach (var enemyWrapper in _config.enemies)
            {
                for (var i = 0; i < enemyWrapper.count; i++)
                {
                    var angle = Random.Range(0f, 2f * Mathf.PI);

                    var position = new Vector3(
                        _spawnPosition.x + Mathf.Cos(angle) * _spawnRadius,
                        _spawnPosition.y,
                        _spawnPosition.z + Mathf.Sin(angle) * _spawnRadius
                    );

                    var enemyGo = Instantiate(
                        enemyWrapper.enemyPrefab,
                        position,
                        Quaternion.identity
                    );

                    var enemy = enemyGo.GetComponent<EnemyScript>();
                    if (enemy != null)
                        enemy.player = _player;

                    // if (GameManager.instance != null)
                    //     GameManager.instance.enemies.Add(enemyGo);

                    SpawnedEnemies++;

                    yield return new WaitForSeconds(_currentSpawnDelay);
                }

                // пауза между типами врагов — сохраняем поведение
                yield return new WaitForSeconds(3f);
            }

            IsSpawning = false;
            gameObject.SetActive(false);
        }
    }
}