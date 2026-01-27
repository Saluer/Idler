using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossScript : MonoBehaviour
{
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float jumpCooldown = 3f;
    [SerializeField] private float fallMultiplier = 3f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        StartCoroutine(JumpRoutine());
    }

    private void FixedUpdate()
    {
        // ускоряем падение
        if (_rb.linearVelocity.y < 0)
        {
            _rb.AddForce(
                Physics.gravity * (fallMultiplier - 1),
                ForceMode.Acceleration
            );
        }
    }

    private IEnumerator JumpRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(jumpCooldown);

            // сбрасываем вертикальную скорость
            var velocity = _rb.linearVelocity;
            velocity.y = 0;
            _rb.linearVelocity = velocity;

            // прыжок
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}