using System;
using System.Collections;
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
        private bool _isFrozen;

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

        // --- Protected accessors for subclasses ---
        protected Rigidbody Rb => _rb;
        protected Animator EnemyAnimator => _animator;
        protected bool IsFrozen => _isFrozen;
        protected float MoveSpeed => speed * _auraSpeedMultiplier * _waveSpeedMultiplier;

        // --- Aura buff support ---
        private float _auraSpeedMultiplier = 1f;
        private float _auraExpireTime;

        public void ApplyAuraBuff(float multiplier)
        {
            _auraSpeedMultiplier = multiplier;
            _auraExpireTime = Time.time + 1f;
        }

        // --- Wave modifier fields ---
        protected float _waveSpeedMultiplier = 1f;
        private float _waveKnockbackMultiplier = 1f;
        private float _waveGoldMultiplier = 1f;
        private bool _splitOnDeath;
        private int _splitGeneration;
        private bool _explodeOnDeath;
        private float _explosionRadius = 3f;
        private int _explosionDamage = 2;

        public int SplitGeneration
        {
            get => _splitGeneration;
            set => _splitGeneration = value;
        }

        private void Awake()
        {
            _defaultPosition = transform.position;
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
        }

        protected virtual void Start()
        {
            _health = maxHealth;
            GameManager.instance.enemies.Add(gameObject);
            ApplyWaveModifiers();
        }

        private void ApplyWaveModifiers()
        {
            var modMgr = WaveModifierManager.Instance;
            if (modMgr == null) return;

            transform.localScale *= modMgr.CombinedScale;
            _health = Mathf.Max(1, (int)(_health * modMgr.CombinedHealth));
            _waveSpeedMultiplier = modMgr.CombinedSpeed;
            _waveKnockbackMultiplier = modMgr.CombinedKnockback;
            _waveGoldMultiplier = modMgr.CombinedGold;

            if (modMgr.SplitOnDeath)
            {
                _splitOnDeath = true;
            }

            if (modMgr.ExplodeOnDeath)
            {
                _explodeOnDeath = true;
                _explosionRadius = modMgr.ExplosionRadius;
                _explosionDamage = modMgr.ExplosionDamage;
            }

            if (modMgr.Transparency > 0f)
            {
                foreach (var rend in GetComponentsInChildren<Renderer>())
                {
                    foreach (var mat in rend.materials)
                    {
                        var c = mat.color;
                        c.a = 1f - modMgr.Transparency;
                        mat.color = c;

                        // Enable transparency rendering
                        mat.SetFloat("_Mode", 3); // Transparent
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;
                    }
                }
            }
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

            // Expire aura buff
            if (_auraSpeedMultiplier > 1f && Time.time >= _auraExpireTime)
            {
                _auraSpeedMultiplier = 1f;
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

        protected virtual void HandleMovement()
        {
            if (!player) return;

            if (_isFrozen)
            {
                var vel = _rb.linearVelocity;
                vel.x = 0;
                vel.z = 0;
                _rb.linearVelocity = vel;
                _animator.SetFloat(Speed, 0f);
                return;
            }

            if (Time.time - 1.5f >= _dealtDamageTime)
                _canHit = true;

            var direction = player.transform.position - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            direction.Normalize();

            var velocity = _rb.linearVelocity;
            velocity.x = direction.x * MoveSpeed;
            velocity.z = direction.z * MoveSpeed;

            _rb.linearVelocity = velocity;

            _rb.rotation = Quaternion.LookRotation(direction);

            _animator.SetFloat(Speed, 1f);
        }


        public void IncreaseHealth(int delta)
        {
            _health = Mathf.Clamp(_health + delta, 0, maxHealth);
        }

        public void Freeze(float duration)
        {
            StartCoroutine(FreezeCoroutine(duration));
        }

        private IEnumerator FreezeCoroutine(float duration)
        {
            _isFrozen = true;
            yield return new WaitForSeconds(duration);
            _isFrozen = false;
        }

        protected virtual void HandleHealth()
        {
            if (_health > 0) return;

            var goldAmount = (int)(Random.Range(minGold, maxGold + 1) * _waveGoldMultiplier);
            SpawnGoldText(goldAmount);

            TrySpawnChest();

            // Explode on death (wave modifier)
            if (_explodeOnDeath)
            {
                ExplodeOnDeath();
            }

            // Split on death (wave modifier - Matryoshka)
            if (_splitOnDeath && _splitGeneration < 2)
            {
                SpawnSplitCopies();
            }

            // Chain Reaction buff
            if (BuffHub.ChainReaction > 0)
            {
                var chance = BuffHub.ChainReaction * 10;
                if (Random.Range(0, 100) < chance)
                {
                    ChainReactionExplosion();
                }
            }

            Destroy(gameObject);
            OnEnemyKilled?.Invoke(goldAmount);
        }

        private void ExplodeOnDeath()
        {
            var hits = Physics.OverlapSphere(transform.position, _explosionRadius);
            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;
                if (hit.TryGetComponent<EnemyScript>(out var enemy))
                {
                    enemy.IncreaseHealth(-_explosionDamage);
                }
            }
        }

        private void SpawnSplitCopies()
        {
            for (var i = 0; i < 2; i++)
            {
                var offset = Random.insideUnitSphere * 1.5f;
                offset.y = 0;
                var pos = transform.position + offset;

                var copy = Instantiate(gameObject, pos, transform.rotation);
                copy.transform.localScale = transform.localScale * 0.6f;

                if (copy.TryGetComponent<EnemyScript>(out var copyEnemy))
                {
                    copyEnemy._splitGeneration = _splitGeneration + 1;
                    copyEnemy._splitOnDeath = true;
                    copyEnemy.player = player;
                }
            }
        }

        private void ChainReactionExplosion()
        {
            var hits = Physics.OverlapSphere(transform.position, 4f);
            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;
                if (hit.TryGetComponent<EnemyScript>(out var enemy))
                {
                    enemy.IncreaseHealth(-2);
                }
            }
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

            Destroy(textObject, 1f);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (player != null && other.gameObject.GetComponent<PlayerScript>() == player)
            {
                DealDamage(other.gameObject.GetComponent<PlayerScript>());
            }
        }

        private void DealDamage(PlayerScript targetPlayer)
        {
            if (!_canHit)
                return;

            targetPlayer.IncreaseHealth(-damage);

            var knockbackDir = (targetPlayer.transform.position - transform.position).normalized;
            targetPlayer.ApplyKnockback(knockbackDir, damage * _waveKnockbackMultiplier);

            // Thorns buff
            if (BuffHub.Thorns > 0)
            {
                IncreaseHealth(-BuffHub.Thorns);
            }

            _canHit = false;
            _dealtDamageTime = Time.time;
        }
    }
}
