using UnityEngine;

namespace DefaultNamespace
{
    public class EnemyDodgerScript : EnemyScript
    {
        [Header("Dodger")]
        [SerializeField] private float strafeAmplitude = 4f;
        [SerializeField] private float strafeFrequency = 2f;
        [SerializeField] private float dodgeSpeedMultiplier = 1.5f;

        private float _strafeDirection = 1f;
        private float _nextStrafeFlipTime;

        protected override void Start()
        {
            base.Start();
            _nextStrafeFlipTime = Time.time + 1f / strafeFrequency;
        }

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

            // Flip strafe direction periodically
            if (Time.time >= _nextStrafeFlipTime)
            {
                _strafeDirection *= -1f;
                _nextStrafeFlipTime = Time.time + 1f / strafeFrequency;
            }

            var toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0;

            if (toPlayer.sqrMagnitude < 0.0001f) return;

            var forward = toPlayer.normalized;
            var strafe = Vector3.Cross(Vector3.up, forward) * (_strafeDirection * strafeAmplitude);

            var moveDir = (forward + strafe).normalized;
            var effectiveSpeed = MoveSpeed * dodgeSpeedMultiplier;

            var velocity = Rb.linearVelocity;
            velocity.x = moveDir.x * effectiveSpeed;
            velocity.z = moveDir.z * effectiveSpeed;
            Rb.linearVelocity = velocity;

            Rb.rotation = Quaternion.LookRotation(forward);
            EnemyAnimator.SetFloat(Animator.StringToHash("Speed"), 1f);
        }
    }
}
