using UnityEngine;

namespace DefaultNamespace
{
    public class BuffHub : MonoBehaviour
    {
        public static int FireTouchDamage;
        public static int MoneyIsStrength;
        public static int ExtraLife;

        public void IncreaseFireTouchDamage()
        {
            if (GameManager.instance.goldAmount < 20)
            {
                return;
            }

            GameManager.instance.IncrementGold(-20);
            FireTouchDamage++;
        }

        public void IncreaseMoneyIsStrength()
        {
            if (GameManager.instance.goldAmount < 20)
            {
                return;
            }

            GameManager.instance.IncrementGold(-20);
            MoneyIsStrength++;
        }

        public void IncreaseExtraLife()
        {
            if (GameManager.instance.goldAmount < 20)
            {
                return;
            }

            GameManager.instance.IncrementGold(-20);
            ExtraLife++;
        }
    }
}