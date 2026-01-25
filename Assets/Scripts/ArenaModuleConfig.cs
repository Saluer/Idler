using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu]
    public class ArenaModuleConfig : ScriptableObject
    {
        public ArenaSlotType slotType;
        public GameObject prefab;
        [Range(0f, 1f)] public float chance;
    }

    public enum ArenaSlotType
    {
        Obstacle,
        Pit,
        Platform,
        Decoration
    }

}