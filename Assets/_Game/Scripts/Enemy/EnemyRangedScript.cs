using UnityEngine;

namespace DefaultNamespace
{
    public class EnemyRangedScript : EnemyScript
    {
        [Header("Ranged")]
        [SerializeField] private float preferredDistance = 12f;
        [SerializeField] private float retreatDistance = 6f;
        [SerializeField] private float fireRate = 2f;
        [SerializeField] private EnemyProjectileScript projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private int projectileDamage = 1;

        private float _nextFireTime;

        protected override void HandleMovement()
        {
            if (!player) return;

            if (IsFrozen)
            {
                var vel = Rb.linearVelocity;
                vel.x = 0;
                vel.z = 0;
                Rb.linearVelocity = vel;
                EnemyAnimator.SetFloat(Animator.StringToHash("Speed"), 0f);
                return;
            }

            var toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0;
            var distance = toPlayer.magnitude;
            var direction = toPlayer.normalized;

            Vector3 moveDir;

            if (distance > preferredDistance)
            {
                // Approach
                moveDir = direction;
            }
            else if (distance < retreatDistance)
            {
                // Retreat
                moveDir = -direction;
            }
            else
            {
                // In sweet spot — hold position and shoot
                moveDir = Vector3.zero;
                TryFire(direction);
            }

            var velocity = Rb.linearVelocity;
            velocity.x = moveDir.x * MoveSpeed;
            velocity.z = moveDir.z * MoveSpeed;
            Rb.linearVelocity = velocity;

            if (direction.sqrMagnitude > 0.0001f)
                Rb.rotation = Quaternion.LookRotation(direction);

            EnemyAnimator.SetFloat(Animator.StringToHash("Speed"), moveDir.sqrMagnitude > 0.01f ? 1f : 0f);

            // Also try to fire while moving if in range
            if (distance <= preferredDistance && distance >= retreatDistance)
            {
                TryFire(direction);
            }
        }

        private void TryFire(Vector3 direction)
        {
            if (Time.time < _nextFireTime) return;
            _nextFireTime = Time.time + 1f / fireRate;

            var spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1f;
            var projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            projectile.Init(direction, projectileSpeed, projectileDamage);
        }
    }
}
