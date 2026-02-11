using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

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
        private int _health;

        [SerializeField] private float speed;

        [SerializeField] private int damage;
        [SerializeField] private int maxHealth;
        [SerializeField] private string displayName;


        [SerializeField] private int minGold = 1;
        [SerializeField] private int maxGold = 5;
        [SerializeField] private float goldTextLifetime = 2f;
        [SerializeField] private float goldTextHeight = 2f;

        [Header("Chest drop")] [SerializeField, Range(0f, 1f)]
        private float chestDropChance = 0.15f;

        [SerializeField] private List<GameObject> chestPrefabs;

        private void Awake()
        {
            _defaultPosition = transform.position;
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            _health = maxHealth;
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

        //todo fix random jumping, while not breaking falling from the start
        private void HandleMovement()
        {
            if (!player) return;

            if (Time.time - 1.5f >= _dealtDamageTime)
                _canHit = true;

            var direction = player.transform.position - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            direction.Normalize();

            // сохраняем текущую вертикальную скорость (гравитация!)
            var velocity = _rb.linearVelocity;
            velocity.x = direction.x * speed;
            velocity.z = direction.z * speed;

            _rb.linearVelocity = velocity;

            _rb.rotation = Quaternion.LookRotation(direction);

            _animator.SetFloat(Speed, 1f);
        }


        public void IncreaseHealth(int delta)
        {
            _health = Mathf.Clamp(_health + delta, 0, maxHealth);
        }

        protected virtual void HandleHealth()
        {
            if (_health > 0) return;

            var goldAmount = Random.Range(minGold, maxGold + 1);
            SpawnGoldText(goldAmount);

            TrySpawnChest();

            Destroy(gameObject);
            OnEnemyKilled?.Invoke(goldAmount);
        }
        
        protected int GetCurrentHealth()
        {
            return _health;
        }


        private void TrySpawnChest()
        {
            if (chestPrefabs.Count == 0)
                return;

            if (Random.value > chestDropChance)
                return;

            var prefab = chestPrefabs[Random.Range(0, chestPrefabs.Count)];

            var chest = Instantiate(
                prefab,
                transform.position,
                Quaternion.identity
            );

            if (chest.TryGetComponent<Collider>(out var col))
            {
                var offsetY = col.bounds.extents.y;
                chest.transform.position += Vector3.up * offsetY;
            }

            Destroy(chest, 10f);
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

            // textObject.AddComponent<BillBoardScript>();

            Destroy(textObject, goldTextLifetime);
        }

        public void HandleHealthChange(int delta)
        {
            IncreaseHealth(delta);

            var textObject = new GameObject("DamageText")
            {
                transform =
                {
                    position = transform.position + Vector3.up * 3f
                }
            };

            var text = textObject.AddComponent<TextMeshPro>();
            text.text = $"+{delta} hp";
            text.fontSize = 3;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.darkRed;

            // textObject.AddComponent<BillBoardScript>();

            Destroy(textObject, 1f);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (player != null && other.gameObject.GetComponent<PlayerScript>() == player)
            {
                DealDamage();
            }
        }

        private void DealDamage()
        {
            if (!_canHit)
                return;

            player.IncreaseHealth(-damage);
            player.ApplyKnockback((player.transform.position - transform.position).normalized, damage);
            _canHit = false;
            _dealtDamageTime = Time.time;
        }
    }
}