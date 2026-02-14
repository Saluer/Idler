using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.Weapon
{
    [CreateAssetMenu(fileName = "WeaponUpgrade_", menuName = "Weapon Upgrade Config", order = 1)]
    public class WeaponUpgradeConfig : ScriptableObject
    {
        public PlayerScript.WeaponType weaponType;
        public List<WeaponUpgradeTier> tiers;
    }

    [System.Serializable]
    public class WeaponUpgradeTier
    {
        public int diamondCost;
        public float damageMultiplier = 1f;
        public float cooldownMultiplier = 1f;
        public int bonusPellets;
        public string description;
    }
}
