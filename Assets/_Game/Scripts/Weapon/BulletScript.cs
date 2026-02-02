using System;
using DefaultNamespace;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class BulletScript : MonoBehaviour
{
    public event Action<EnemyScript> OnHitDelegate;
    public Transform target;
    public float speed;
    public float damage;

    private void Start()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void Update()
    {
        Vector3 center;
        if (target && target.TryGetComponent<Renderer>(out var component))
        {
            center = component.bounds.center;
        }
        else if (target && target.TryGetComponent<Collider>(out var component2))
        {
            center = component2.bounds.center;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        var direction = center - transform.position;

        transform.Translate(direction.normalized * (speed * Time.deltaTime), Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        var enemyScript = other.GetComponent<EnemyScript>();
        if (!enemyScript)
        {
            return;
        }

        OnHitDelegate?.Invoke(enemyScript);
        Destroy(gameObject);
    }
}