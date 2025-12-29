using System.Collections;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Collider))]
    public class SpawnerScript : MonoBehaviour
    {
        private Collider _targetCollider;
        private Collider _collider;
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private Transform _spawnPosition;

        private void Awake()
        {
            _targetCollider = FindFirstObjectByType<PlayerScript>().GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject != _targetCollider.gameObject) return;


            StartCoroutine(Spawn(10));
        }

        private IEnumerator Spawn(int times)
        {
            for (var i = 0; i < times; i++)
            {
                var spawnPositionPosition = _spawnPosition.position;
                spawnPositionPosition = new Vector3(spawnPositionPosition.x + Random.Range(-5, 5),
                    spawnPositionPosition.y, spawnPositionPosition.z);
                Instantiate(_enemyPrefab, spawnPositionPosition, Quaternion.identity);
                yield return new WaitForSeconds(0.1f);
            }
            gameObject.SetActive(false);
        }
    }
}