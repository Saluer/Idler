using UnityEngine;

namespace DefaultNamespace
{
    public class RangedWeaponScript : MonoBehaviour
    {
        [SerializeField] private BulletScript bullet;
        [SerializeField] private float damage;
        [SerializeField] private float range;

        public void Fire(Transform target)
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
                var damageToEnemy = -(int)damage + -BuffHub.MoneyIsStrength * GameManager.instance.goldAmount / 100;
                param.HandleHealthChange(damageToEnemy);
            };

            Destroy(bulletInstance, 3f);
        }
    }
}