using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerScript : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController _controller;
    private Vector3 _velocity;

    private Vector2 _moveInput;
    private bool _jumpPressed;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        var move =
            transform.right * _moveInput.x +
            transform.forward * _moveInput.y;

        if (_controller.isGrounded)
        {
            if (_velocity.y < 0)
                _velocity.y = -2f;

            if (_jumpPressed)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _jumpPressed = false;
            }
        }

        _velocity.y += gravity * Time.deltaTime;

        var displacement = (move * moveSpeed + _velocity) * Time.deltaTime;
        _controller.Move(displacement);
    }

    // === Send Messages callbacks ===

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void OnJump()
    {
        if (_controller.isGrounded)
            _jumpPressed = true;
    }
}