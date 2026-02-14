using System.Collections.Generic;
using System.Reflection;
using DefaultNamespace;
using UnityEngine;

namespace Tests.EditMode
{
    /// <summary>
    /// Utilities for setting up minimal game infrastructure in Edit Mode tests.
    /// </summary>
    public static class TestHelpers
    {
        private static readonly List<GameObject> _created = new();

        /// <summary>
        /// Creates a minimal GameManager with instance set, so code that reads
        /// GameManager.instance.diamondsAmount etc. doesn't NRE.
        /// </summary>
        public static GameManager CreateGameManager(int diamonds = 100, int gold = 0)
        {
            var go = new GameObject("TestGameManager");
            _created.Add(go);

            var gm = go.AddComponent<GameManager>();
            // Awake sets instance automatically
            gm.diamondsAmount = diamonds;
            gm.goldAmount = gold;
            return gm;
        }

        /// <summary>
        /// Creates a WaveModifierManager with the given configs.
        /// </summary>
        public static WaveModifierManager CreateWaveModifierManager(
            List<WaveModifierConfig> configs)
        {
            var go = new GameObject("TestWaveModifierManager");
            _created.Add(go);

            var mgr = go.AddComponent<WaveModifierManager>();
            // Set the private serialized field via reflection
            var field = typeof(WaveModifierManager).GetField(
                "allModifiers",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(mgr, configs);

            return mgr;
        }

        /// <summary>
        /// Creates a WeaponUpgradeManager with the given configs.
        /// </summary>
        public static WeaponUpgradeManager CreateWeaponUpgradeManager(
            List<_Game.Scripts.Weapon.WeaponUpgradeConfig> configs)
        {
            var go = new GameObject("TestWeaponUpgradeManager");
            _created.Add(go);

            var mgr = go.AddComponent<WeaponUpgradeManager>();
            var field = typeof(WeaponUpgradeManager).GetField(
                "upgradeConfigs",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(mgr, configs);

            return mgr;
        }

        /// <summary>
        /// Creates a bare Transform at the given scale (for GiantSlayer tests etc.)
        /// </summary>
        public static Transform CreateScaledTransform(float scale)
        {
            var go = new GameObject("ScaledTarget");
            _created.Add(go);
            go.transform.localScale = Vector3.one * scale;
            return go.transform;
        }

        /// <summary>
        /// Resets all static buff fields on BuffHub.
        /// </summary>
        public static void ResetBuffHub()
        {
            BuffHub.FireTouchDamage = 0;
            BuffHub.MoneyIsStrength = 0;
            BuffHub.ExtraLife = 0;
            BuffHub.Vampire = 0;
            BuffHub.Thorns = 0;
            BuffHub.ChainReaction = 0;
            BuffHub.GiantSlayer = 0;
        }

        /// <summary>
        /// Destroys all GameObjects created by helpers. Call in [TearDown].
        /// </summary>
        public static void Cleanup()
        {
            foreach (var go in _created)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }

            _created.Clear();
            ResetBuffHub();
        }

        /// <summary>
        /// Creates a WaveModifierConfig ScriptableObject with given parameters.
        /// </summary>
        public static WaveModifierConfig CreateModifierConfig(
            WaveModifierType type,
            string displayName,
            string description = "",
            float scale = 1f,
            float count = 1f,
            float health = 1f,
            float speed = 1f,
            float gold = 1f,
            float knockback = 1f,
            float transparency = 0f,
            bool splitOnDeath = false,
            bool explodeOnDeath = false,
            float explosionRadius = 3f,
            int explosionDamage = 2)
        {
            var config = ScriptableObject.CreateInstance<WaveModifierConfig>();
            config.type = type;
            config.displayName = displayName;
            config.description = description;
            config.scaleMultiplier = scale;
            config.countMultiplier = count;
            config.healthMultiplier = health;
            config.speedMultiplier = speed;
            config.goldMultiplier = gold;
            config.knockbackMultiplier = knockback;
            config.transparency = transparency;
            config.splitOnDeath = splitOnDeath;
            config.explodeOnDeath = explodeOnDeath;
            config.explosionRadius = explosionRadius;
            config.explosionDamage = explosionDamage;
            return config;
        }
    }
}
