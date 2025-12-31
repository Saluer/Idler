using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyScript : MonoBehaviour
    {
        public PlayerScript player { get; set; }

        private Rigidbody _rb;
        private Vector3 _defaultPosition;

        [SerializeField] private float speed;

        [SerializeField] private int damage;

        private void Awake()
        {
            _defaultPosition = transform.position;
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (!player) return;

            _defaultPosition = transform.position;

            var direction = player.transform.position - transform.position;
            direction = direction.normalized;

            var newPosition = _rb.position + direction * (speed * Time.fixedDeltaTime);
            newPosition.y = _defaultPosition.y;
            _rb.MovePosition(newPosition);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerScript>() == player)
            {
                Debug.Log("Hit player");
            }
        }
    }
}