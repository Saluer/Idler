using UnityEngine;

namespace DefaultNamespace
{
    public enum WaveModifierType
    {
        AntInvasion,
        Matryoshka,
        ThiccBoys,
        ExplosiveFinale,
        SpeedDating,
        Bouncers,
        Jackpot,
        GhostProtocol
    }

    [CreateAssetMenu(fileName = "WaveModifier_", menuName = "Wave Modifier", order = 2)]
    public class WaveModifierConfig : ScriptableObject
    {
        public WaveModifierType type;
        public string displayName;
        [TextArea] public string description;

        public float scaleMultiplier = 1f;
        public float countMultiplier = 1f;
        public float healthMultiplier = 1f;
        public float speedMultiplier = 1f;
        public float goldMultiplier = 1f;
        public float knockbackMultiplier = 1f;
        [Range(0f, 1f)] public float transparency;
        public bool splitOnDeath;
        public bool explodeOnDeath;
        public float explosionRadius = 3f;
        public int explosionDamage = 2;
    }
}
