using System.Collections.Generic;
using _Game.Scripts.Weapon;
using DefaultNamespace;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    [TestFixture]
    public class WeaponUpgradeTests
    {
        private GameManager _gm;

        [SetUp]
        public void SetUp()
        {
            _gm = TestHelpers.CreateGameManager(diamonds: 200);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.Cleanup();
        }

        private WeaponUpgradeConfig CreatePistolConfig(params WeaponUpgradeTier[] tiers)
        {
            var config = ScriptableObject.CreateInstance<WeaponUpgradeConfig>();
            config.weaponType = PlayerScript.WeaponType.Pistol;
            config.tiers = new List<WeaponUpgradeTier>(tiers);
            return config;
        }

        // ==================== Tier Tracking ====================

        [Test]
        public void GetCurrentTier_Default_ReturnsZero()
        {
            var mgr = TestHelpers.CreateWeaponUpgradeManager(
                new List<WeaponUpgradeConfig>());

            Assert.AreEqual(0, mgr.GetCurrentTier(PlayerScript.WeaponType.Pistol));
            Assert.AreEqual(0, mgr.GetCurrentTier(PlayerScript.WeaponType.Sword));
        }

        // ==================== CanUpgrade ====================

        [Test]
        public void CanUpgrade_NoConfig_ReturnsFalse()
        {
            var mgr = TestHelpers.CreateWeaponUpgradeManager(
                new List<WeaponUpgradeConfig>());

            Assert.IsFalse(mgr.CanUpgrade(PlayerScript.WeaponType.Pistol));
        }

        [Test]
        public void CanUpgrade_WithEnoughDiamonds_ReturnsTrue()
        {
            var config = CreatePistolConfig(new WeaponUpgradeTier
            {
                diamondCost = 50,
                damageMultiplier = 1.5f,
                cooldownMultiplier = 0.9f
            });

            var mgr = TestHelpers.CreateWeaponUpgradeManager(
                new List<WeaponUpgradeConfig> { config });

            _gm.diamondsAmount = 50;
            Assert.IsTrue(mgr.CanUpgrade(PlayerScript.WeaponType.Pistol));
        }

        [Test]
        public void CanUpgrade_NotEnoughDiamonds_ReturnsFalse()
        {
            var config = CreatePistolConfig(new WeaponUpgradeTier
            {
                diamondCost = 50,
                damageMultiplier = 1.5f,
                cooldownMultiplier = 0.9f
            });

            var mgr = TestHelpers.CreateWeaponUpgradeManager(
                new List<WeaponUpgradeConfig> { config });

            _gm.diamondsAmount = 49;
            Assert.IsFalse(mgr.CanUpgrade(PlayerScript.WeaponType.Pistol));
        }

        [Test]
        public void CanUpgrade_AllTiersUsed_ReturnsFalse()
        {
            var config = CreatePistolConfig(new WeaponUpgradeTier
            {
                diamondCost = 10,
                damageMultiplier = 1.5f,
                cooldownMultiplier = 0.9f
            });

            var mgr = TestHelpers.CreateWeaponUpgradeManager(
                new List<WeaponUpgradeConfig> { config });

            // Create a mock weapon to receive upgrade
            var weaponGo = new GameObject("TestWeapon");
            var weapon = weaponGo.AddComponent<TestWeapon>();

            mgr.Upgrade(PlayerScript.WeaponType.Pistol, weapon);

            // Now tier 0 is used, no tier 1 exists
            Assert.IsFalse(mgr.CanUpgrade(PlayerScript.WeaponType.Pistol));

            Object.DestroyImmediate(weaponGo);
        }

        // ==================== Upgrade ====================

        [Test]
        public void Upgrade_IncrementsTier()
        {
            var config = CreatePistolConfig(
                new WeaponUpgradeTier
                {
                    diamondCost = 20,
                    damageMultiplier = 1.5f,
                    cooldownMultiplier = 0.9f
                },
                new WeaponUpgradeTier
                {
                    diamondCost = 40,
                    damageMultiplier = 2f,
                    cooldownMultiplier = 0.8f
                });

            var mgr = TestHelpers.CreateWeaponUpgradeManager(
                new List<WeaponUpgradeConfig> { config });

            var weaponGo = new GameObject("TestWeapon");
            var weapon = weaponGo.AddComponent<TestWeapon>();

            mgr.Upgrade(PlayerScript.WeaponType.Pistol, weapon);

            Assert.AreEqual(1, mgr.GetCurrentTier(PlayerScript.WeaponType.Pistol));

            mgr.Upgrade(PlayerScript.WeaponType.Pistol, weapon);

            Assert.AreEqual(2, mgr.GetCurrentTier(PlayerScript.WeaponType.Pistol));

            Object.DestroyImmediate(weaponGo);
        }

        [Test]
        public void Upgrade_DeductsDiamonds()
        {
            var config = CreatePistolConfig(new WeaponUpgradeTier
            {
                diamondCost = 30,
                damageMultiplier = 1.5f,
                cooldownMultiplier = 0.9f
            });

            var mgr = TestHelpers.CreateWeaponUpgradeManager(
                new List<WeaponUpgradeConfig> { config });

            _gm.diamondsAmount = 100;

            var weaponGo = new GameObject("TestWeapon");
            var weapon = weaponGo.AddComponent<TestWeapon>();

            mgr.Upgrade(PlayerScript.WeaponType.Pistol, weapon);

            Assert.AreEqual(70, _gm.diamondsAmount);

            Object.DestroyImmediate(weaponGo);
        }

        [Test]
        public void Upgrade_CallsApplyUpgradeOnWeapon()
        {
            var tier = new WeaponUpgradeTier
            {
                diamondCost = 10,
                damageMultiplier = 2f,
                cooldownMultiplier = 0.5f,
                bonusPellets = 3
            };
            var config = CreatePistolConfig(tier);

            var mgr = TestHelpers.CreateWeaponUpgradeManager(
                new List<WeaponUpgradeConfig> { config });

            var weaponGo = new GameObject("TestWeapon");
            var weapon = weaponGo.AddComponent<TestWeapon>();

            mgr.Upgrade(PlayerScript.WeaponType.Pistol, weapon);

            Assert.IsTrue(weapon.UpgradeApplied, "ApplyUpgrade should have been called");
            Assert.AreEqual(2f, weapon.LastDamageMultiplier);
            Assert.AreEqual(0.5f, weapon.LastCooldownMultiplier);
            Assert.AreEqual(3, weapon.LastBonusPellets);

            Object.DestroyImmediate(weaponGo);
        }

        [Test]
        public void Upgrade_InsufficientDiamonds_DoesNothing()
        {
            var config = CreatePistolConfig(new WeaponUpgradeTier
            {
                diamondCost = 999,
                damageMultiplier = 2f,
                cooldownMultiplier = 0.5f
            });

            var mgr = TestHelpers.CreateWeaponUpgradeManager(
                new List<WeaponUpgradeConfig> { config });

            _gm.diamondsAmount = 50;

            var weaponGo = new GameObject("TestWeapon");
            var weapon = weaponGo.AddComponent<TestWeapon>();

            mgr.Upgrade(PlayerScript.WeaponType.Pistol, weapon);

            Assert.AreEqual(0, mgr.GetCurrentTier(PlayerScript.WeaponType.Pistol));
            Assert.IsFalse(weapon.UpgradeApplied);
            Assert.AreEqual(50, _gm.diamondsAmount);

            Object.DestroyImmediate(weaponGo);
        }

        // ==================== Multi-Weapon ====================

        [Test]
        public void DifferentWeapons_TrackTiersSeparately()
        {
            var pistolConfig = CreatePistolConfig(new WeaponUpgradeTier
            {
                diamondCost = 10,
                damageMultiplier = 1.5f,
                cooldownMultiplier = 0.9f
            });

            var swordConfig = ScriptableObject.CreateInstance<WeaponUpgradeConfig>();
            swordConfig.weaponType = PlayerScript.WeaponType.Sword;
            swordConfig.tiers = new List<WeaponUpgradeTier>
            {
                new() { diamondCost = 15, damageMultiplier = 2f, cooldownMultiplier = 0.8f }
            };

            var mgr = TestHelpers.CreateWeaponUpgradeManager(
                new List<WeaponUpgradeConfig> { pistolConfig, swordConfig });

            var weaponGo = new GameObject("TestWeapon");
            var weapon = weaponGo.AddComponent<TestWeapon>();

            mgr.Upgrade(PlayerScript.WeaponType.Pistol, weapon);

            Assert.AreEqual(1, mgr.GetCurrentTier(PlayerScript.WeaponType.Pistol));
            Assert.AreEqual(0, mgr.GetCurrentTier(PlayerScript.WeaponType.Sword));

            Object.DestroyImmediate(weaponGo);
        }
    }

    /// <summary>
    /// Minimal IWeapon implementation for testing upgrades without real weapon MonoBehaviours.
    /// </summary>
    public class TestWeapon : MonoBehaviour, IWeapon
    {
        public bool UpgradeApplied;
        public float LastDamageMultiplier;
        public float LastCooldownMultiplier;
        public int LastBonusPellets;
        private int _tier;

        public void Enable() { }
        public void Disable() { }
        public void TryAttack(Transform target) { }

        public void ApplyUpgrade(WeaponUpgradeTier tier)
        {
            UpgradeApplied = true;
            LastDamageMultiplier = tier.damageMultiplier;
            LastCooldownMultiplier = tier.cooldownMultiplier;
            LastBonusPellets = tier.bonusPellets;
            _tier++;
        }

        public int CurrentUpgradeTier => _tier;
    }
}
