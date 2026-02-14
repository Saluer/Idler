using DefaultNamespace;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    [TestFixture]
    public class BuffHubTests
    {
        private GameManager _gm;

        [SetUp]
        public void SetUp()
        {
            _gm = TestHelpers.CreateGameManager(diamonds: 100, gold: 500);
            TestHelpers.ResetBuffHub();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.Cleanup();
        }

        // ==================== Giant Slayer ====================

        [Test]
        public void GiantSlayer_NoBuff_ReturnsBaseDamage()
        {
            BuffHub.GiantSlayer = 0;
            var target = TestHelpers.CreateScaledTransform(2f);

            var result = BuffHub.ApplyGiantSlayer(10, target);

            Assert.AreEqual(10, result);
        }

        [Test]
        public void GiantSlayer_SmallEnemy_ReturnsBaseDamage()
        {
            BuffHub.GiantSlayer = 1;
            var target = TestHelpers.CreateScaledTransform(1f);

            var result = BuffHub.ApplyGiantSlayer(10, target);

            Assert.AreEqual(10, result, "Enemies at 1x scale should not get bonus damage");
        }

        [Test]
        public void GiantSlayer_LargeEnemy_AppliesBonus()
        {
            BuffHub.GiantSlayer = 1;
            var target = TestHelpers.CreateScaledTransform(2f);

            var result = BuffHub.ApplyGiantSlayer(10, target);

            // +50% of 10 = 15
            Assert.AreEqual(15, result);
        }

        [Test]
        public void GiantSlayer_MultipleLevels_Stacks()
        {
            BuffHub.GiantSlayer = 3;
            var target = TestHelpers.CreateScaledTransform(2f);

            var result = BuffHub.ApplyGiantSlayer(10, target);

            // 3 levels × 50% = +150% → 10 + 15 = 25
            Assert.AreEqual(25, result);
        }

        [Test]
        public void GiantSlayer_NullEnemy_ReturnsBaseDamage()
        {
            BuffHub.GiantSlayer = 5;

            var result = BuffHub.ApplyGiantSlayer(10, null);

            Assert.AreEqual(10, result);
        }

        [Test]
        public void GiantSlayer_SlightlyAboveThreshold_AppliesBonus()
        {
            BuffHub.GiantSlayer = 1;
            var target = TestHelpers.CreateScaledTransform(1.1f);

            var result = BuffHub.ApplyGiantSlayer(10, target);

            Assert.Greater(result, 10, "Enemy at 1.1x should trigger Giant Slayer");
        }

        [Test]
        public void GiantSlayer_AtThreshold_NoBonus()
        {
            BuffHub.GiantSlayer = 1;
            var target = TestHelpers.CreateScaledTransform(1.04f);

            var result = BuffHub.ApplyGiantSlayer(10, target);

            Assert.AreEqual(10, result, "Enemy at 1.04x should NOT trigger (threshold is 1.05)");
        }

        // ==================== Buff Purchases ====================

        [Test]
        public void PurchaseVampire_WithDiamonds_Succeeds()
        {
            var hub = _gm.gameObject.AddComponent<BuffHub>();
            _gm.diamondsAmount = 50;

            hub.IncreaseVampire();

            Assert.AreEqual(1, BuffHub.Vampire);
            Assert.AreEqual(30, _gm.diamondsAmount, "Should spend 20 diamonds");
        }

        [Test]
        public void PurchaseVampire_InsufficientDiamonds_Fails()
        {
            var hub = _gm.gameObject.AddComponent<BuffHub>();
            _gm.diamondsAmount = 10;

            hub.IncreaseVampire();

            Assert.AreEqual(0, BuffHub.Vampire);
            Assert.AreEqual(10, _gm.diamondsAmount, "Diamonds should be unchanged");
        }

        [Test]
        public void PurchaseThorns_DeductsDiamonds()
        {
            var hub = _gm.gameObject.AddComponent<BuffHub>();
            _gm.diamondsAmount = 40;

            hub.IncreaseThorns();

            Assert.AreEqual(1, BuffHub.Thorns);
            Assert.AreEqual(20, _gm.diamondsAmount);
        }

        [Test]
        public void PurchaseChainReaction_Costs25Diamonds()
        {
            var hub = _gm.gameObject.AddComponent<BuffHub>();
            _gm.diamondsAmount = 25;

            hub.IncreaseChainReaction();

            Assert.AreEqual(1, BuffHub.ChainReaction);
            Assert.AreEqual(0, _gm.diamondsAmount, "Chain Reaction costs 25");
        }

        [Test]
        public void PurchaseChainReaction_24Diamonds_Fails()
        {
            var hub = _gm.gameObject.AddComponent<BuffHub>();
            _gm.diamondsAmount = 24;

            hub.IncreaseChainReaction();

            Assert.AreEqual(0, BuffHub.ChainReaction);
            Assert.AreEqual(24, _gm.diamondsAmount);
        }

        [Test]
        public void PurchaseGiantSlayer_DeductsDiamonds()
        {
            var hub = _gm.gameObject.AddComponent<BuffHub>();
            _gm.diamondsAmount = 100;

            hub.IncreaseGiantSlayer();
            hub.IncreaseGiantSlayer();

            Assert.AreEqual(2, BuffHub.GiantSlayer);
            Assert.AreEqual(60, _gm.diamondsAmount);
        }

        [Test]
        public void MultiplePurchases_StackCorrectly()
        {
            var hub = _gm.gameObject.AddComponent<BuffHub>();
            _gm.diamondsAmount = 200;

            hub.IncreaseVampire();
            hub.IncreaseVampire();
            hub.IncreaseVampire();

            Assert.AreEqual(3, BuffHub.Vampire);
            Assert.AreEqual(140, _gm.diamondsAmount);
        }

        [Test]
        public void PurchaseStopsWhenBroke()
        {
            var hub = _gm.gameObject.AddComponent<BuffHub>();
            _gm.diamondsAmount = 35;

            hub.IncreaseVampire(); // -20 → 15 left
            hub.IncreaseVampire(); // fails, need 20

            Assert.AreEqual(1, BuffHub.Vampire);
            Assert.AreEqual(15, _gm.diamondsAmount);
        }
    }
}
