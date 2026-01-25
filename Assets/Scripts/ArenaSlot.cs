using UnityEngine;

namespace DefaultNamespace
{
    public class ArenaSlot : MonoBehaviour
    {
        [SerializeField] private ArenaSlotType type;
        public ArenaSlotType Type => type;
    }
}