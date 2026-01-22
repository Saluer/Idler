using System;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyScript : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        public static event Action<int> OnEnemyKilled;
        public PlayerScript player { get; set; }

        private Rigidbody _rb;
        private Animator _animator;
        private Vector3 _defaultPosition;

        private bool _canHit;
        private float _dealtDamageTime;

        [SerializeField] private float speed;

        [SerializeField] private int damage;
        [SerializeField] private int health;
        [SerializeField] private string displayName;


        [SerializeField] private int minGold = 1;
        [SerializeField] private int maxGold = 5;
        [SerializeField] private float goldTextLifetime = 2f;
        [SerializeField] private float goldTextHeight = 2f;

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

            if (Time.time - 1.5f >= _dealtDamageTime)
            {
                _canHit = true;
            }

            _defaultPosition = transform.position;

            var direction = player.transform.position - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            direction = direction.normalized;


            var newPosition = _rb.position + direction * (speed * Time.fixedDeltaTime);
            newPosition.y = _defaultPosition.y;

            var transformRotation = Quaternion.LookRotation(direction);
            _rb.MoveRotation(transformRotation);

            _rb.MovePosition(newPosition);

            _animator.SetFloat(Speed, 1f);
        }

        private void HandleHealth()
        {
            if (health > 0) return;
            var goldAmount = UnityEngine.Random.Range(minGold, maxGold + 1);
            SpawnGoldText(goldAmount);
            Destroy(gameObject);
            OnEnemyKilled?.Invoke(goldAmount);
        }

        private void SpawnGoldText(int goldAmount)
        {
            var textObject = new GameObject("GoldText")
            {
                transform =
                {
                    position = transform.position + Vector3.up * goldTextHeight
                }
            };

            var text = textObject.AddComponent<TextMeshPro>();
            text.text = $"+{goldAmount} gold";
            text.fontSize = 3;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.yellow;

            // Чтобы текст всегда смотрел на камеру (опционально, но обычно нужно)
            // textObject.AddComponent<BillboardToCamera>();

            Destroy(textObject, goldTextLifetime);
        }

        public void HandleHealthChange(int delta)
        {
            health += delta;
        }

        private void OnCollisionEnter(Collision other)
        {
            Debug.Log(other.gameObject.name);
            if (player != null && other.gameObject.GetComponent<PlayerScript>() == player)
            {
                DealDamage();
            }
        }

        private void DealDamage()
        {
            if (!_canHit)
                return;

            Debug.Log("Dealing damage");
            player.IncreaseHealth(-damage);
            player.ApplyKnockback((player.transform.position - transform.position).normalized, damage);
            _canHit = false;
            _dealtDamageTime = Time.time;
        }
    }
}