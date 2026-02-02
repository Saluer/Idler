using UnityEngine;

using System;
using DefaultNamespace;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ShotgunPelletScript : MonoBehaviour
{
    public event Action<EnemyScript> OnHitDelegate;

    [SerializeField] private float lifeTime = 1.5f;

    private void Start()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<EnemyScript>(out var enemy))
            return;

        OnHitDelegate?.Invoke(enemy);
        Destroy(gameObject);
    }

    private void OnEnable()
    {
        Destroy(gameObject, lifeTime);
    }
}
