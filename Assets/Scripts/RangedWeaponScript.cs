using UnityEngine;

namespace DefaultNamespace
{
    public class RangedWeaponScript : MonoBehaviour
    {
        [SerializeField] private MeleeWeaponScript bullet;
        [SerializeField] private float damage;
        [SerializeField] private float range;

        public void Fire(Vector3 target)
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

            var direction = (target - transform.position).normalized;
            bullet.transform.rotation = Quaternion.LookRotation(direction);
            bulletInstance.OnHitDelegate += param => { param.HandleHealthChange(-(int)damage); };

            rb.AddForce(direction * range, ForceMode.Impulse);

            Destroy(bulletInstance, 3f);
        }
    }
}