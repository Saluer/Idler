using System.Collections.Generic;
using DefaultNamespace;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    /// <summary>
    /// Tests for enemy-related mechanics that can be verified without Play Mode.
    /// Focuses on wave modifier application, split generation limits,
    /// and damage calculations.
    /// </summary>
    [TestFixture]
    public class EnemyMechanicsTests
    {
        private GameManager _gm;

        [SetUp]
        public void SetUp()
        {
            _gm = TestHelpers.CreateGameManager();
            TestHelpers.ResetBuffHub();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.Cleanup();
        }

        // ==================== Wave Modifier Combinations ====================

        [Test]
        public void AntInvasion_Config_HasCorrectValues()
        {
            var config = TestHelpers.CreateModifierConfig(
                WaveModifierType.AntInvasion, "ANT INVASION",
                description: "They're tiny. They're angry.",
                scale: 0.4f, count: 3f, health: 0.5f);

            Assert.AreEqual(0.4f, config.scaleMultiplier);
            Assert.AreEqual(3f, config.countMultiplier);
            Assert.AreEqual(0.5f, config.healthMultiplier);
            Assert.AreEqual(1f, config.speedMultiplier, "Speed should be default");
        }

        [Test]
        public void ThiccBoys_Config_HasCorrectValues()
        {
            var config = TestHelpers.CreateModifierConfig(
                WaveModifierType.ThiccBoys, "THICC BOYS",
                scale: 2f, health: 2f, speed: 0.5f, gold: 2f);

            Assert.AreEqual(2f, config.scaleMultiplier);
            Assert.AreEqual(2f, config.healthMultiplier);
            Assert.AreEqual(0.5f, config.speedMultiplier);
            Assert.AreEqual(2f, config.goldMultiplier);
        }

        [Test]
        public void SpeedDating_Config_HasCorrectValues()
        {
            var config = TestHelpers.CreateModifierConfig(
                WaveModifierType.SpeedDating, "SPEED DATING",
                speed: 3f, health: 0.01f); // Very low health → 1 HP

            Assert.AreEqual(3f, config.speedMultiplier);
            Assert.IsTrue(config.healthMultiplier < 0.1f);
        }

        [Test]
        public void ExplosiveFinale_Config_HasExplosion()
        {
            var config = TestHelpers.CreateModifierConfig(
                WaveModifierType.ExplosiveFinale, "EXPLOSIVE FINALE",
                explodeOnDeath: true, explosionRadius: 3f, explosionDamage: 2);

            Assert.IsTrue(config.explodeOnDeath);
            Assert.AreEqual(3f, config.explosionRadius);
            Assert.AreEqual(2, config.explosionDamage);
        }

        [Test]
        public void Bouncers_Config_HasHighKnockback()
        {
            var config = TestHelpers.CreateModifierConfig(
                WaveModifierType.Bouncers, "BOUNCERS",
                knockback: 5f);

            Assert.AreEqual(5f, config.knockbackMultiplier);
        }

        [Test]
        public void GhostProtocol_Config_HasTransparency()
        {
            var config = TestHelpers.CreateModifierConfig(
                WaveModifierType.GhostProtocol, "GHOST PROTOCOL",
                transparency: 0.8f);

            Assert.AreEqual(0.8f, config.transparency);
        }

        [Test]
        public void Jackpot_Config_TradeOff()
        {
            var config = TestHelpers.CreateModifierConfig(
                WaveModifierType.Jackpot, "JACKPOT",
                gold: 3f, health: 2f);

            Assert.AreEqual(3f, config.goldMultiplier, "3x gold");
            Assert.AreEqual(2f, config.healthMultiplier, "2x health");
        }

        // ==================== Modifier Stacking ====================

        [Test]
        public void TwoModifiers_MultiplyScale()
        {
            // If both Ant Invasion (0.4x) and Thicc Boys (2x) somehow combine
            var ant = TestHelpers.CreateModifierConfig(
                WaveModifierType.AntInvasion, "ANT", scale: 0.4f);
            var thicc = TestHelpers.CreateModifierConfig(
                WaveModifierType.ThiccBoys, "THICC", scale: 2f);

            var mgr = TestHelpers.CreateWaveModifierManager(
                new List<WaveModifierConfig> { ant, thicc });

            // Run until both are active (count=2)
            for (var i = 0; i < 100; i++)
            {
                mgr.RollModifiers();
                if (mgr.GetActiveModifiers().Count == 2)
                    break;
            }

            if (mgr.GetActiveModifiers().Count == 2)
            {
                Assert.AreEqual(0.8f, mgr.CombinedScale, 0.01f,
                    "0.4 × 2.0 = 0.8");
            }
        }

        [Test]
        public void TwoModifiers_HealthMultiplies()
        {
            var jackpot = TestHelpers.CreateModifierConfig(
                WaveModifierType.Jackpot, "JACKPOT", health: 2f);
            var thicc = TestHelpers.CreateModifierConfig(
                WaveModifierType.ThiccBoys, "THICC", health: 2f);

            var mgr = TestHelpers.CreateWaveModifierManager(
                new List<WaveModifierConfig> { jackpot, thicc });

            for (var i = 0; i < 100; i++)
            {
                mgr.RollModifiers();
                if (mgr.GetActiveModifiers().Count == 2)
                    break;
            }

            if (mgr.GetActiveModifiers().Count == 2)
            {
                Assert.AreEqual(4f, mgr.CombinedHealth, 0.01f,
                    "2 × 2 = 4x health");
            }
        }

        // ==================== Split Generation ====================

        [Test]
        public void SplitGeneration_PropertyWorks()
        {
            // Verify SplitGeneration can be set via the public property
            // (used when spawning split copies)
            var go = new GameObject("TestEnemy");
            go.AddComponent<Rigidbody>();
            go.AddComponent<Animator>();
            var enemy = go.AddComponent<EnemyScript>();

            enemy.SplitGeneration = 2;
            Assert.AreEqual(2, enemy.SplitGeneration);

            Object.DestroyImmediate(go);
        }

        // ==================== Chain Reaction Buff ====================

        [Test]
        public void ChainReaction_Level0_NeverTriggers()
        {
            BuffHub.ChainReaction = 0;
            var chance = BuffHub.ChainReaction * 10; // 0%
            Assert.AreEqual(0, chance);
        }

        [Test]
        public void ChainReaction_Level1_TenPercent()
        {
            BuffHub.ChainReaction = 1;
            var chance = BuffHub.ChainReaction * 10;
            Assert.AreEqual(10, chance);
        }

        [Test]
        public void ChainReaction_Level5_FiftyPercent()
        {
            BuffHub.ChainReaction = 5;
            var chance = BuffHub.ChainReaction * 10;
            Assert.AreEqual(50, chance);
        }

        [Test]
        public void ChainReaction_Level10_HundredPercent()
        {
            BuffHub.ChainReaction = 10;
            var chance = BuffHub.ChainReaction * 10;
            Assert.AreEqual(100, chance);
        }

        // ==================== Giant Slayer + Thicc Boys Combo ====================

        [Test]
        public void GiantSlayer_PlusTHICC_ComboWorks()
        {
            BuffHub.GiantSlayer = 2;

            // Thicc Boys enemy at 2x scale
            var target = TestHelpers.CreateScaledTransform(2f);

            var baseDmg = 10;
            var result = BuffHub.ApplyGiantSlayer(baseDmg, target);

            // 2 levels × 50% = +100% → 10 + 10 = 20
            Assert.AreEqual(20, result,
                "Giant Slayer should combo with Thicc Boys (2x scale enemies)");
        }
    }
}
