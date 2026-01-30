using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Renderer))]
    public class SwordScript : MonoBehaviour
    {
        public SwordHitbox hitbox { get; private set; }
        private Renderer _renderer;
        private TrailRenderer _trailRenderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _trailRenderer = GetComponentInChildren<TrailRenderer>();
            hitbox = GetComponentInChildren<SwordHitbox>();
        }

        public void ToggleVisibility(bool show)
        {
            _renderer.enabled = show;
            _trailRenderer.emitting = show;
            _trailRenderer.Clear();
            if (show) hitbox.BeginSwing();
            else hitbox.EndSwing();
        }
    }
}