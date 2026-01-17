using System;
using DefaultNamespace;
using UnityEngine;

public class MeleeWeaponScript : MonoBehaviour
{
    public event Action<EnemyScript> OnHitDelegate;

    private void OnTriggerEnter(Collider other)
    {
        var enemyScript = other.GetComponent<EnemyScript>();
        if (!enemyScript)
        {
            return;
        }

        OnHitDelegate?.Invoke(enemyScript);
    }
}