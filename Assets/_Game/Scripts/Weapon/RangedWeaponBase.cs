using _Game.Scripts.Weapon;
using UnityEngine;

namespace DefaultNamespace.weapon
{
    public abstract class RangedWeaponBase : MonoBehaviour, IWeapon
    {
        [SerializeField] protected float damage;
        [SerializeField] protected float range;
        [SerializeField] private float cooldown = 1f;
        protected float nextAttackTime;

        // Upgrade multipliers
        protected float _damageMultiplier = 1f;
        protected float _cooldownMultiplier = 1f;
        private int _currentUpgradeTier;

        public int EffectiveDamage => Mathf.RoundToInt(damage * _damageMultiplier);
        protected float EffectiveCooldown => cooldown * _cooldownMultiplier;

        public abstract void Fire(Transform target);

        public virtual void Enable()
        {
            gameObject.SetActive(true);
        }

        public virtual void Disable()
        {
            gameObject.SetActive(false);
        }

        public void TryAttack(Transform target)
        {
            if (!gameObject.activeSelf)
                return;

            if (Time.time < nextAttackTime)
                return;

            nextAttackTime = Time.time + EffectiveCooldown;
            Fire(target);
        }

        public virtual void ApplyUpgrade(WeaponUpgradeTier tier)
        {
            _damageMultiplier = tier.damageMultiplier;
            _cooldownMultiplier = tier.cooldownMultiplier;
            _currentUpgradeTier++;
        }

        public int CurrentUpgradeTier => _currentUpgradeTier;
    }
}
