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
        private bool _canHit;
        private float dealtDamageTime;
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
        }

        private void FixedUpdate()
        {
            if (GameManager.instance.gameMode != GameManager.GameMode.Active)
            {
                return;
            }

            HandleMovement();
        }

        private void HandleMovement()
        {
            if (!player) return;

            if (Time.time - 1.5f >= dealtDamageTime)
            {
                _canHit = true;
            }

            _defaultPosition = transform.position;

            var direction = player.transform.position - transform.position;
            direction = direction.normalized;
            //todo figure out why it's there - a right place for y = 0
            direction.y = 0;

            var newPosition = _rb.position + direction * (speed * Time.fixedDeltaTime);
            newPosition.y = _defaultPosition.y;

            var transformRotation = Quaternion.LookRotation(direction);
            gameObject.transform.rotation = transformRotation;

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
            if (player != null && other.GetComponent<PlayerScript>() == player)
            {
                DealDamage();
            }
        }

        private void DealDamage()
        {
            player.IncreaseHealth(-damage);
            _canHit = false;
            dealtDamageTime = Time.time;
        }
    }
}