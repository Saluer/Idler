using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class EnemyChampionScript : EnemyScript
    {
        [Header("Champion spawn")]
        [SerializeField] private GameObject smallEnemyPrefab;

        [SerializeField, Range(2, 4)]
        private int minSpawnCount = 2;

        [SerializeField, Range(2, 4)]
        private int maxSpawnCount = 4;

        [SerializeField]
        private float spawnRadius = 1.5f;

        [Header("Spawn push")]
        [SerializeField]
        private float pushForceMin = 2f;

        [SerializeField]
        private float pushForceMax = 5f;

        private bool _spawnedChildren;

        protected override void HandleHealth()
        {
            if (_spawnedChildren)
            {
                base.HandleHealth();
                return;
            }

            if (GetCurrentHealth() > 0)
                return;

            SpawnChildren();
            _spawnedChildren = true;

            base.HandleHealth();
        }

        private void SpawnChildren()
        {
            if (!smallEnemyPrefab)
                return;

            var count = Random.Range(minSpawnCount, maxSpawnCount + 1);

            for (var i = 0; i < count; i++)
            {
                // позиционное разбрасывание
                var offset = Random.insideUnitSphere * spawnRadius;
                offset.y = 0f;

                var enemy = Instantiate(
                    smallEnemyPrefab,
                    transform.position + offset,
                    Quaternion.identity
                );

                if (enemy.TryGetComponent<EnemyScript>(out var enemyScript))
                {
                    enemyScript.player = player;
                }

                // физический толчок
                if (enemy.TryGetComponent<Rigidbody>(out var rb))
                {
                    var dir = Random.insideUnitSphere;
                    dir.y = 0f;
                    dir.Normalize();

                    var force = Random.Range(pushForceMin, pushForceMax);
                    rb.AddForce(dir * force, ForceMode.Impulse);
                }
            }
        }
    }
}
