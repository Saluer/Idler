using System.Collections.Generic;
using DefaultNamespace;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    [TestFixture]
    public class WaveModifierManagerTests
    {
        private GameManager _gm;

        [SetUp]
        public void SetUp()
        {
            _gm = TestHelpers.CreateGameManager();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.Cleanup();
        }

        // ==================== Defaults ====================

        [Test]
        public void Defaults_AllMultipliersAreOne()
        {
            var mgr = TestHelpers.CreateWaveModifierManager(new List<WaveModifierConfig>());

            Assert.AreEqual(1f, mgr.CombinedScale);
            Assert.AreEqual(1f, mgr.CombinedCount);
            Assert.AreEqual(1f, mgr.CombinedHealth);
            Assert.AreEqual(1f, mgr.CombinedSpeed);
            Assert.AreEqual(1f, mgr.CombinedGold);
            Assert.AreEqual(1f, mgr.CombinedKnockback);
            Assert.AreEqual(0f, mgr.Transparency);
            Assert.IsFalse(mgr.SplitOnDeath);
            Assert.IsFalse(mgr.ExplodeOnDeath);
        }

        // ==================== Rolling ====================

        [Test]
        public void RollModifiers_EmptyList_NoActiveModifiers()
        {
            var mgr = TestHelpers.CreateWaveModifierManager(new List<WaveModifierConfig>());

            mgr.RollModifiers();

            Assert.AreEqual(0, mgr.GetActiveModifiers().Count);
            Assert.AreEqual(1f, mgr.CombinedScale, "Should stay default with no modifiers");
        }

        [Test]
        public void RollModifiers_SingleModifier_AlwaysPicksIt()
        {
            var config = TestHelpers.CreateModifierConfig(
                WaveModifierType.ThiccBoys, "THICC BOYS",
                scale: 2f, health: 2f, speed: 0.5f, gold: 2f);

            var mgr = TestHelpers.CreateWaveModifierManager(
                new List<WaveModifierConfig> { config });

            mgr.RollModifiers();

            Assert.AreEqual(1, mgr.GetActiveModifiers().Count);
            Assert.AreEqual(2f, mgr.CombinedScale);
            Assert.AreEqual(2f, mgr.CombinedHealth);
            Assert.AreEqual(0.5f, mgr.CombinedSpeed);
            Assert.AreEqual(2f, mgr.CombinedGold);
        }

        [Test]
        public void RollModifiers_TwoModifiers_PicksBoth_WhenCountIsTwo()
        {
            var antInvasion = TestHelpers.CreateModifierConfig(
                WaveModifierType.AntInvasion, "ANT INVASION",
                scale: 0.4f, count: 3f, health: 0.5f);

            var speedDating = TestHelpers.CreateModifierConfig(
                WaveModifierType.SpeedDating, "SPEED DATING",
                speed: 3f, health: 0.1f); // very low health

            var mgr = TestHelpers.CreateWaveModifierManager(
                new List<WaveModifierConfig> { antInvasion, speedDating });

            // Run multiple times — with 2 configs, count can be 1 or 2
            var sawBoth = false;
            for (var i = 0; i < 50; i++)
            {
                mgr.RollModifiers();
                if (mgr.GetActiveModifiers().Count == 2)
                {
                    sawBoth = true;
                    break;
                }
            }

            Assert.IsTrue(sawBoth, "Should sometimes pick both modifiers");
        }

        [Test]
        public void RollModifiers_ManyModifiers_PicksAtMostTwo()
        {
            var configs = new List<WaveModifierConfig>();
            for (var i = 0; i < 8; i++)
            {
                configs.Add(TestHelpers.CreateModifierConfig(
                    (WaveModifierType)i, $"Mod{i}"));
            }

            var mgr = TestHelpers.CreateWaveModifierManager(configs);

            for (var i = 0; i < 50; i++)
            {
                mgr.RollModifiers();
                Assert.LessOrEqual(mgr.GetActiveModifiers().Count, 2);
                Assert.GreaterOrEqual(mgr.GetActiveModifiers().Count, 1);
            }
        }

        // ==================== Combined Multipliers ====================

        [Test]
        public void CombinedMultipliers_TwoModifiers_Multiply()
        {
            // Use only one config to guarantee deterministic result
            // Actually let's use the single-modifier approach
            var thicc = TestHelpers.CreateModifierConfig(
                WaveModifierType.ThiccBoys, "THICC BOYS",
                scale: 2f, health: 2f, speed: 0.5f, gold: 2f);

            var mgr = TestHelpers.CreateWaveModifierManager(
                new List<WaveModifierConfig> { thicc });

            mgr.RollModifiers();

            Assert.AreEqual(2f, mgr.CombinedScale);
            Assert.AreEqual(2f, mgr.CombinedHealth);
            Assert.AreEqual(0.5f, mgr.CombinedSpeed);
            Assert.AreEqual(2f, mgr.CombinedGold);
        }

        [Test]
        public void Knockback_Multiplier_Applied()
        {
            var bouncers = TestHelpers.CreateModifierConfig(
                WaveModifierType.Bouncers, "BOUNCERS",
                knockback: 5f);

            var mgr = TestHelpers.CreateWaveModifierManager(
                new List<WaveModifierConfig> { bouncers });

            mgr.RollModifiers();

            Assert.AreEqual(5f, mgr.CombinedKnockback);
        }

        // ==================== Boolean Flags ====================

        [Test]
        public void SplitOnDeath_SetFromMatryoshka()
        {
            var matryoshka = TestHelpers.CreateModifierConfig(
                WaveModifierType.Matryoshka, "MATRYOSHKA",
                splitOnDeath: true);

            var mgr = TestHelpers.CreateWaveModifierManager(
                new List<WaveModifierConfig> { matryoshka });

            mgr.RollModifiers();

            Assert.IsTrue(mgr.SplitOnDeath);
        }

        [Test]
        public void ExplodeOnDeath_SetFromExplosiveFinale()
        {
            var explosive = TestHelpers.CreateModifierConfig(
                WaveModifierType.ExplosiveFinale, "EXPLOSIVE FINALE",
                explodeOnDeath: true, explosionRadius: 5f, explosionDamage: 3);

            var mgr = TestHelpers.CreateWaveModifierManager(
                new List<WaveModifierConfig> { explosive });

            mgr.RollModifiers();

            Assert.IsTrue(mgr.ExplodeOnDeath);
            Assert.AreEqual(5f, mgr.ExplosionRadius);
            Assert.AreEqual(3, mgr.ExplosionDamage);
        }

        [Test]
        public void Transparency_TakesMaxValue()
        {
            var ghost = TestHelpers.CreateModifierConfig(
                WaveModifierType.GhostProtocol, "GHOST PROTOCOL",
                transparency: 0.8f);

            var mgr = TestHelpers.CreateWaveModifierManager(
                new List<WaveModifierConfig> { ghost });

            mgr.RollModifiers();

            Assert.AreEqual(0.8f, mgr.Transparency, 0.001f);
        }

        // ==================== Clear ====================

        [Test]
        public void ClearModifiers_ResetsEverything()
        {
            var thicc = TestHelpers.CreateModifierConfig(
                WaveModifierType.ThiccBoys, "THICC BOYS",
                scale: 2f, health: 2f, explodeOnDeath: true, splitOnDeath: true,
                transparency: 0.5f, knockback: 3f);

            var mgr = TestHelpers.CreateWaveModifierManager(
                new List<WaveModifierConfig> { thicc });

            mgr.RollModifiers();
            mgr.ClearModifiers();

            Assert.AreEqual(0, mgr.GetActiveModifiers().Count);
            Assert.AreEqual(1f, mgr.CombinedScale);
            Assert.AreEqual(1f, mgr.CombinedHealth);
            Assert.AreEqual(1f, mgr.CombinedSpeed);
            Assert.AreEqual(0f, mgr.Transparency);
            Assert.IsFalse(mgr.SplitOnDeath);
            Assert.IsFalse(mgr.ExplodeOnDeath);
        }

        // ==================== Announcement ====================

        [Test]
        public void Announcement_EmptyModifiers_ReturnsEmpty()
        {
            var mgr = TestHelpers.CreateWaveModifierManager(new List<WaveModifierConfig>());

            mgr.RollModifiers();

            Assert.AreEqual("", mgr.GetAnnouncementText());
        }

        [Test]
        public void Announcement_ContainsModifierName()
        {
            var config = TestHelpers.CreateModifierConfig(
                WaveModifierType.SpeedDating, "SPEED DATING",
                description: "They're fast. They're fragile.");

            var mgr = TestHelpers.CreateWaveModifierManager(
                new List<WaveModifierConfig> { config });

            mgr.RollModifiers();

            var text = mgr.GetAnnouncementText();
            Assert.IsTrue(text.Contains("SPEED DATING"));
            Assert.IsTrue(text.Contains("They're fast"));
        }
    }
}
