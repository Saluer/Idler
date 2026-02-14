using System.Collections.Generic;
using _Game.Scripts.Weapon;
using UnityEngine;

public class WeaponUpgradeManager : MonoBehaviour
{
    public static WeaponUpgradeManager Instance { get; private set; }

    [SerializeField] private List<WeaponUpgradeConfig> upgradeConfigs;

    private readonly Dictionary<PlayerScript.WeaponType, int> _currentTiers = new();

    private void Awake()
    {
        Instance = this;
    }

    private WeaponUpgradeConfig GetConfig(PlayerScript.WeaponType type)
    {
        foreach (var cfg in upgradeConfigs)
        {
            if (cfg.weaponType == type) return cfg;
        }
        return null;
    }

    public int GetCurrentTier(PlayerScript.WeaponType type)
    {
        return _currentTiers.TryGetValue(type, out var tier) ? tier : 0;
    }

    public bool CanUpgrade(PlayerScript.WeaponType type)
    {
        var config = GetConfig(type);
        if (config == null) return false;

        var currentTier = GetCurrentTier(type);
        if (currentTier >= config.tiers.Count) return false;

        var nextTier = config.tiers[currentTier];
        return DefaultNamespace.GameManager.instance.diamondsAmount >= nextTier.diamondCost;
    }

    public void Upgrade(PlayerScript.WeaponType type, IWeapon weapon)
    {
        var config = GetConfig(type);
        if (config == null) return;

        var currentTier = GetCurrentTier(type);
        if (currentTier >= config.tiers.Count) return;

        var tier = config.tiers[currentTier];
        if (DefaultNamespace.GameManager.instance.diamondsAmount < tier.diamondCost) return;

        DefaultNamespace.GameManager.instance.IncrementDiamonds(-tier.diamondCost);
        weapon.ApplyUpgrade(tier);
        _currentTiers[type] = currentTier + 1;
    }
}
