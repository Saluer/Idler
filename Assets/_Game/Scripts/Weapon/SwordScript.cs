using System.Collections;
using _Game.Scripts.Weapon;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Renderer))]
    public class SwordScript : MonoBehaviour, IWeapon
    {
        public SwordHitbox hitbox { get; private set; }

        [SerializeField] private float swingCooldown = 3f;
        [SerializeField] private int baseDamage = 1;

        private Renderer _renderer;
        private TrailRenderer _trailRenderer;

        private bool _isAttacking;
        private float _nextAttackTime;
        private Coroutine _swingCoroutine;

        // Upgrade support
        private float _damageMultiplier = 1f;
        private float _cooldownMultiplier = 1f;
        private int _currentUpgradeTier;

        private int EffectiveDamage => Mathf.RoundToInt(baseDamage * _damageMultiplier);
        private float EffectiveCooldown => swingCooldown * _cooldownMultiplier;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _trailRenderer = GetComponentInChildren<TrailRenderer>();
            hitbox = GetComponentInChildren<SwordHitbox>();
            hitbox.OnHitDelegate += enemy => { enemy.HandleHealthChange(-EffectiveDamage); };
        }

        // ======================
        // IWeapon
        // ======================

        public void Enable()
        {
            gameObject.SetActive(true);
            hitbox.gameObject.SetActive(true);
            ToggleVisibility(false);
        }

        public void Disable()
        {
            if (_swingCoroutine != null)
            {
                StopCoroutine(_swingCoroutine);
                _swingCoroutine = null;
            }

            _isAttacking = false;
            ToggleVisibility(false);
            gameObject.SetActive(false);
        }

        public void TryAttack(Transform target)
        {
            if (!gameObject.activeSelf)
                return;

            if (_isAttacking)
                return;

            if (Time.time < _nextAttackTime)
                return;

            _nextAttackTime = Time.time + EffectiveCooldown;
            _swingCoroutine = StartCoroutine(SwingCoroutine());
        }

        public void ApplyUpgrade(WeaponUpgradeTier tier)
        {
            _damageMultiplier = tier.damageMultiplier;
            _cooldownMultiplier = tier.cooldownMultiplier;
            _currentUpgradeTier++;
        }

        public int CurrentUpgradeTier => _currentUpgradeTier;

        // ======================
        // Attack logic
        // ======================

        private IEnumerator SwingCoroutine()
        {
            _isAttacking = true;

            ToggleVisibility(true);

            for (var angle = 90.0f; angle > -270.0f; angle -= 15f)
            {
                transform.localRotation = Quaternion.Euler(0, angle, 0);
                yield return new WaitForFixedUpdate();
            }

            ToggleVisibility(false);

            _isAttacking = false;
            _swingCoroutine = null;
        }

        // ======================
        // Visuals / hitbox
        // ======================

        private void ToggleVisibility(bool show)
        {
            _renderer.enabled = show;

            if (_trailRenderer)
            {
                _trailRenderer.emitting = show;
                _trailRenderer.Clear();
            }

            if (hitbox)
            {
                if (show) hitbox.BeginSwing();
                else hitbox.EndSwing();
            }
        }
    }
}
