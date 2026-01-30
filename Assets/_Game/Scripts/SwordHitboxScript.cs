using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class SwordHitbox : MonoBehaviour
    {
        [SerializeField] private int damage = 10;
        public event Action<EnemyScript> OnHitDelegate;

        private readonly HashSet<Collider> _hitThisSwing = new();
        private Collider _col;

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _col.enabled = false;
        }

        public void BeginSwing()
        {
            _hitThisSwing.Clear();
            _col.enabled = true;
        }

        public void EndSwing()
        {
            _col.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hitThisSwing.Contains(other)) return;
            if (!other.TryGetComponent<EnemyScript>(out var enemy)) return;
            Debug.Log(other.name + " has hit something");
            _hitThisSwing.Add(other);

            if (enemy != null)
                OnHitDelegate?.Invoke(enemy);
        }
    }
}