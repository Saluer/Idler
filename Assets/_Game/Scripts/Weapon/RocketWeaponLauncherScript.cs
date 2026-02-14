using UnityEngine;

namespace DefaultNamespace.weapon
{
    public class RocketLauncherWeaponScript : RangedWeaponBase
    {
        [SerializeField] private RocketProjectileScript rocket;
        [SerializeField] private float force = 12f;

        public override void Fire(Transform target)
        {
            if (!gameObject.activeSelf || target == null)
                return;

            var rocketInstance =
                Instantiate(rocket, transform.position, Quaternion.identity);

            rocketInstance.SetDamage(EffectiveDamage);

            if (!rocketInstance.TryGetComponent<Rigidbody>(out var rb))
            {
                Destroy(rocketInstance.gameObject);
                return;
            }

            var direction = (target.position - transform.position).normalized;
            rocketInstance.transform.rotation = Quaternion.LookRotation(direction);

            rb.AddForce(direction * force, ForceMode.Impulse);

            Destroy(rocketInstance.gameObject, 5f);
        }
    }
}
