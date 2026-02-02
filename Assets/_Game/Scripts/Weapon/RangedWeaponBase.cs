using UnityEngine;

namespace DefaultNamespace.weapon
{
    public abstract class RangedWeaponBase : MonoBehaviour
    {
        [SerializeField] protected float damage;
        [SerializeField] protected float range;

        public abstract void Fire(Transform target);
    }
}
