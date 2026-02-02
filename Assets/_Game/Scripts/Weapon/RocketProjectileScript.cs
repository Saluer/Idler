using UnityEngine;


namespace DefaultNamespace.weapon
{
    public class RocketProjectileScript : MonoBehaviour
    {
        [SerializeField] private float explosionRadius = 4f;
        [SerializeField] private int damage = 5;
        [SerializeField] private LayerMask enemyLayer;

        private void OnTriggerEnter(Collider other)
        {
            var hits = Physics.OverlapSphere(
                transform.position,
                explosionRadius,
                enemyLayer
            );
            
            Debug.Log(hits);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<EnemyScript>(out var enemy))
                {
                    enemy.IncreaseHealth(-damage);
                }
            }

            Destroy(gameObject);
        }
    }
}
