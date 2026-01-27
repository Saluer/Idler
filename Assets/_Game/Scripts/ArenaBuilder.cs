using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class ArenaBuilder : MonoBehaviour
{
    [SerializeField] private List<ArenaModuleConfig> modules;
    [SerializeField] private List<ArenaSlot> slots;

    public void Build()
    {
        foreach (var slot in slots)
        {
            var candidates = modules
                .Where(m => m.slotType == slot.Type)
                .ToList();

            foreach (var module in candidates.Where(module => Random.value <= module.chance))
            {
                Instantiate(
                    module.prefab,
                    slot.transform.position,
                    slot.transform.rotation,
                    slot.transform
                );
                break;
            }
        }
    }
}