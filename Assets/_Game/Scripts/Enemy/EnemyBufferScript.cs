using System.Collections;
using UnityEngine;

namespace DefaultNamespace
{
    public class EnemyBufferScript : EnemyScript
    {
        [Header("Buffer Aura")]
        [SerializeField] private float auraRadius = 8f;
        [SerializeField] private float auraSpeedMultiplier = 1.5f;
        [SerializeField] private float auraTick = 0.5f;

        private Coroutine _auraCoroutine;

        protected override void Start()
        {
            base.Start();
            _auraCoroutine = StartCoroutine(AuraCoroutine());
        }

        private IEnumerator AuraCoroutine()
        {
            while (true)
            {
                var hits = Physics.OverlapSphere(transform.position, auraRadius);
                foreach (var hit in hits)
                {
                    if (hit.gameObject == gameObject) continue;
                    if (hit.TryGetComponent<EnemyScript>(out var enemy))
                    {
                        enemy.ApplyAuraBuff(auraSpeedMultiplier);
                    }
                }

                yield return new WaitForSeconds(auraTick);
            }
        }
    }
}
