using _Game.Scripts.Weapon;
using DefaultNamespace.weapon;
using UnityEngine;

namespace DefaultNamespace
{
    public class PistolScript : RangedWeaponBase
    {
        [SerializeField] private BulletScript bullet;

        public override void Fire(Transform target)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            var bulletInstance =
                Instantiate(bullet, transform.position, transform.rotation);

            if (!bulletInstance.TryGetComponent<Rigidbody>(out var rb))
            {
                Debug.LogError("Bullet prefab has no Rigidbody");
                Destroy(bulletInstance);
                return;
            }

            var direction = (target.position - transform.position).normalized;
            bullet.transform.rotation = Quaternion.LookRotation(direction);
            bullet.target = target;

            bulletInstance.GetComponent<Rigidbody>().AddForce(direction * damage, ForceMode.Impulse);
            bulletInstance.OnHitDelegate += param =>
            {
                var baseDmg = EffectiveDamage + BuffHub.MoneyIsStrength * GameManager.instance.goldAmount / 100;
                var finalDmg = BuffHub.ApplyGiantSlayer(baseDmg, param.transform);
                param.HandleHealthChange(-finalDmg);
            };

            Destroy(bulletInstance, 3f);
        }
    }
}
