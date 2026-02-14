using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class WaveModifierManager : MonoBehaviour
    {
        public static WaveModifierManager Instance { get; private set; }

        [SerializeField] private List<WaveModifierConfig> allModifiers;

        private readonly List<WaveModifierConfig> _activeModifiers = new();

        // Combined values read by enemies at spawn
        public float CombinedScale { get; private set; } = 1f;
        public float CombinedCount { get; private set; } = 1f;
        public float CombinedHealth { get; private set; } = 1f;
        public float CombinedSpeed { get; private set; } = 1f;
        public float CombinedGold { get; private set; } = 1f;
        public float CombinedKnockback { get; private set; } = 1f;
        public float Transparency { get; private set; }
        public bool SplitOnDeath { get; private set; }
        public bool ExplodeOnDeath { get; private set; }
        public float ExplosionRadius { get; private set; } = 3f;
        public int ExplosionDamage { get; private set; } = 2;

        private void Awake()
        {
            Instance = this;
        }

        public void RollModifiers()
        {
            _activeModifiers.Clear();

            if (allModifiers == null || allModifiers.Count == 0)
            {
                ResetCombined();
                return;
            }

            var count = Random.Range(1, 3); // 1 or 2
            var available = new List<WaveModifierConfig>(allModifiers);

            for (var i = 0; i < count && available.Count > 0; i++)
            {
                var idx = Random.Range(0, available.Count);
                _activeModifiers.Add(available[idx]);
                available.RemoveAt(idx);
            }

            ComputeCombined();
        }

        public void ClearModifiers()
        {
            _activeModifiers.Clear();
            ResetCombined();
        }

        private void ResetCombined()
        {
            CombinedScale = 1f;
            CombinedCount = 1f;
            CombinedHealth = 1f;
            CombinedSpeed = 1f;
            CombinedGold = 1f;
            CombinedKnockback = 1f;
            Transparency = 0f;
            SplitOnDeath = false;
            ExplodeOnDeath = false;
            ExplosionRadius = 3f;
            ExplosionDamage = 2;
        }

        private void ComputeCombined()
        {
            ResetCombined();

            foreach (var mod in _activeModifiers)
            {
                CombinedScale *= mod.scaleMultiplier;
                CombinedCount *= mod.countMultiplier;
                CombinedHealth *= mod.healthMultiplier;
                CombinedSpeed *= mod.speedMultiplier;
                CombinedGold *= mod.goldMultiplier;
                CombinedKnockback *= mod.knockbackMultiplier;

                if (mod.transparency > Transparency)
                    Transparency = mod.transparency;

                if (mod.splitOnDeath)
                    SplitOnDeath = true;

                if (mod.explodeOnDeath)
                {
                    ExplodeOnDeath = true;
                    ExplosionRadius = Mathf.Max(ExplosionRadius, mod.explosionRadius);
                    ExplosionDamage = Mathf.Max(ExplosionDamage, mod.explosionDamage);
                }
            }
        }

        public string GetAnnouncementText()
        {
            if (_activeModifiers.Count == 0)
                return "";

            var text = "";
            foreach (var mod in _activeModifiers)
            {
                text += $"<size=150%><b>{mod.displayName}</b></size>\n{mod.description}\n\n";
            }

            return text.TrimEnd();
        }

        public List<WaveModifierConfig> GetActiveModifiers()
        {
            return _activeModifiers;
        }
    }
}
