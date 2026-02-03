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

            nextAttackTime = Time.time + cooldown;
            Fire(target);
        }
    }
}