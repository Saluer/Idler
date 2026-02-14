using UnityEngine;

namespace _Game.Scripts.Weapon
{
    public interface IWeapon
    {
        void Enable();
        void Disable();
        void TryAttack(Transform target);
        void ApplyUpgrade(WeaponUpgradeTier tier);
        int CurrentUpgradeTier { get; }
    }
}
