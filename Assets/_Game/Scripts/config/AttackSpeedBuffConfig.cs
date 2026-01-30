using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(menuName = "Buffs/Attack Speed Buff")]
    public class AttackSpeedBuff : BuffConfig
    {
        [SerializeField] private float speedBonus = 1.5f;
        [SerializeField] private float duration = 10f;

        public override void Apply(PlayerScript player)
        {
            player.ApplyAttackSpeedBuff(speedBonus, duration);
        }
    }
}