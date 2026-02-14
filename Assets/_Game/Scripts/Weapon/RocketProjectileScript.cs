using UnityEngine;


namespace DefaultNamespace.weapon
{
    public class RocketProjectileScript : MonoBehaviour
    {
        [SerializeField] private float explosionRadius = 4f;
        [SerializeField] private int damage = 5;
        [SerializeField] private LayerMask enemyLayer;

        private bool _damageOverridden;

        public void SetDamage(int overrideDamage)
        {
            damage = overrideDamage;
            _damageOverridden = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var hits = Physics.OverlapSphere(
                transform.position,
                explosionRadius,
                enemyLayer
            );

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<EnemyScript>(out var enemy))
                {
                    var dmg = BuffHub.ApplyGiantSlayer(damage, enemy.transform);
                    enemy.IncreaseHealth(-dmg);
                }
            }

            Destroy(gameObject);
        }
    }
}
