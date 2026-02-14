using _Game.Scripts.Weapon;
using UnityEngine;

namespace DefaultNamespace.weapon
{
    public class ShotgunWeaponScript : RangedWeaponBase
    {
        [SerializeField] private int pellets = 6;
        [SerializeField] private float spreadAngle = 10f;
        [SerializeField] private float force = 8f;
        [SerializeField] private ShotgunPelletScript pelletPrefab;

        private int _bonusPellets;

        public override void Fire(Transform target)
        {
            if (!gameObject.activeSelf)
                return;

            var totalPellets = pellets + _bonusPellets;

            for (var i = 0; i < totalPellets; i++)
            {
                var pellet = Instantiate(
                    pelletPrefab,
                    transform.position,
                    Quaternion.identity
                );

                var rb = pellet.GetComponent<Rigidbody>();
                if (!rb)
                {
                    Destroy(pellet.gameObject);
                    continue;
                }

                var direction = transform.forward;
                direction = Quaternion.Euler(
                    Random.Range(-spreadAngle, spreadAngle),
                    Random.Range(-spreadAngle, spreadAngle),
                    0
                ) * direction;

                pellet.transform.rotation = Quaternion.LookRotation(direction);
                rb.AddForce(direction * force, ForceMode.Impulse);

                pellet.OnHitDelegate += enemy =>
                {
                    var dmg = BuffHub.ApplyGiantSlayer(EffectiveDamage, enemy.transform);
                    enemy.HandleHealthChange(-dmg);
                };
            }
        }

        public override void ApplyUpgrade(WeaponUpgradeTier tier)
        {
            base.ApplyUpgrade(tier);
            _bonusPellets = tier.bonusPellets;
        }
    }
}
