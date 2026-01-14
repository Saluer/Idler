using System;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Rigidbody))]
    //todo add new enemies
    public class EnemyScript : MonoBehaviour
    {
        public static event Action OnEnemyKilled;
        public PlayerScript player { get; set; }

        private Rigidbody _rb;
        private Animator _animator;
        private Vector3 _defaultPosition;

        [SerializeField] private float speed;

        [SerializeField] private int damage;
        [SerializeField] private int health;
        [SerializeField] private string displayName;

        private void Awake()
        {
            _defaultPosition = transform.position;
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            GameManager.instance.enemies.Add(gameObject);
        }

        private void OnDestroy()
        {
            GameManager.instance.enemies.Remove(gameObject);
        }

        private void Update()
        {
            if (GameManager.instance.gameMode != GameManager.GameMode.Active)
            {
                return;
            }

            HandleHealth();
            HandleMovement();
        }

        private void HandleMovement()
        {
            if (!player) return;

            _defaultPosition = transform.position;

            var direction = player.transform.position - transform.position;
            direction = direction.normalized;

            var newPosition = _rb.position + direction * (speed * Time.fixedDeltaTime);
            newPosition.y = _defaultPosition.y;
            gameObject.transform.rotation = Quaternion.LookRotation(direction);

            _rb.MovePosition(newPosition);
            _animator.SetFloat("Speed", 0.5f);
        }

        private void HandleHealth()
        {
            if (health > 0) return;

            Debug.Log("Enemy is dead");

            Destroy(gameObject);
            OnEnemyKilled?.Invoke();
        }

        public void HandleHealthChange(int delta)
        {
            health += delta;
        }

        private void OnTriggerEnter(Collider other)
        {
            if ( player != null && other.GetComponent<PlayerScript>() == player)
            {
                DealDamage();
            }
        }

        private void DealDamage()
        {
            player.IncreaseHealth(-damage);
        }
    }
}