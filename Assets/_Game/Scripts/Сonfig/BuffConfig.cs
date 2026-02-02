using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu]
    public abstract class BuffConfig : ScriptableObject
    {
        public abstract void Apply(PlayerScript player);
    }
}