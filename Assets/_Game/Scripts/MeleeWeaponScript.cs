using System;
using DefaultNamespace;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MeleeWeaponScript : MonoBehaviour
{
    public event Action<EnemyScript> OnHitDelegate;

    private Renderer _renderer;
    private TrailRenderer _trailRenderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _trailRenderer = GetComponentInChildren<TrailRenderer>();
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

    public void ToggleVisibility(bool show)
    {
        _renderer.enabled = show;
        _trailRenderer.emitting = show;
        _trailRenderer.Clear();
    }
}