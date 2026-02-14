using UnityEngine;

namespace DefaultNamespace
{
    public class BuffHub : MonoBehaviour
    {
        public static int FireTouchDamage;
        public static int MoneyIsStrength;
        public static int ExtraLife;
        public static int Vampire;
        public static int Thorns;
        public static int ChainReaction;
        public static int GiantSlayer;

        public void IncreaseFireTouchDamage()
        {
            if (GameManager.instance.diamondsAmount < 20)
            {
                return;
            }

            GameManager.instance.IncrementDiamonds(-20);
            FireTouchDamage++;
        }

        public void IncreaseMoneyIsStrength()
        {
            if (GameManager.instance.diamondsAmount < 20)
            {
                return;
            }

            GameManager.instance.IncrementDiamonds(-20);
            MoneyIsStrength++;
        }

        public void IncreaseExtraLife()
        {
            if (GameManager.instance.diamondsAmount < 20)
            {
                return;
            }

            GameManager.instance.IncrementDiamonds(-20);
            ExtraLife++;
        }

        public void IncreaseVampire()
        {
            if (GameManager.instance.diamondsAmount < 20)
                return;

            GameManager.instance.IncrementDiamonds(-20);
            Vampire++;
        }

        public void IncreaseThorns()
        {
            if (GameManager.instance.diamondsAmount < 20)
                return;

            GameManager.instance.IncrementDiamonds(-20);
            Thorns++;
        }

        public void IncreaseChainReaction()
        {
            if (GameManager.instance.diamondsAmount < 25)
                return;

            GameManager.instance.IncrementDiamonds(-25);
            ChainReaction++;
        }

        public void IncreaseGiantSlayer()
        {
            if (GameManager.instance.diamondsAmount < 20)
                return;

            GameManager.instance.IncrementDiamonds(-20);
            GiantSlayer++;
        }

        /// <summary>
        /// Giant Slayer: +50% damage per level to enemies scaled above 1x.
        /// </summary>
        public static int ApplyGiantSlayer(int baseDamage, Transform enemy)
        {
            if (GiantSlayer <= 0 || enemy == null)
                return baseDamage;

            var scale = enemy.localScale.x;
            if (scale <= 1.05f)
                return baseDamage;

            var bonus = baseDamage * 0.5f * GiantSlayer;
            return baseDamage + (int)bonus;
        }
    }
}
