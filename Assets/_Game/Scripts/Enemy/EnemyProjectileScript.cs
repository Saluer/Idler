using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyProjectileScript : MonoBehaviour
    {
        private Vector3 _direction;
        private float _speed;
        private int _damage;
        private bool _initialized;

        public void Init(Vector3 direction, float speed, int damage)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _initialized = true;

            transform.rotation = Quaternion.LookRotation(_direction);
            Destroy(gameObject, 5f);
        }

        private void FixedUpdate()
        {
            if (!_initialized) return;

            var rb = GetComponent<Rigidbody>();
            rb.linearVelocity = _direction * _speed;
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerScript>();
            if (player != null)
            {
                player.IncreaseHealth(-_damage);
                Destroy(gameObject);
            }
        }
    }
}
