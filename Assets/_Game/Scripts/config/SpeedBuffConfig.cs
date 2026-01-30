using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(menuName = "Buffs/Speed Buff")]
    public class SpeedBuff : BuffConfig
    {
        [SerializeField] private float speedBonus = 1.5f;
        [SerializeField] private float duration = 10f;

        public override void Apply(PlayerScript player)
        {
            player.ApplySpeedBuff(speedBonus, duration);
        }
    }
}