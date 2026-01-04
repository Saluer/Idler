using System;
using DefaultNamespace;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    public event Action<EnemyScript> OnHitDelegate;

    private void Start()
    {
    }


    private void Update()
    {
    }

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