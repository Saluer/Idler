using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerScript : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [SerializeField] [Range(min: 1, max: 100)]
    private float mouseSensitivity;

    [SerializeField] private Transform cameraTransform;
    private CharacterController _controller;

    private Vector3 _velocity;

    private Vector2 _moveInput;
    private bool _jumpPressed;
    private Vector2 _lookInput;
    private float _xRotation;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleCameraMove();
        HandleMove();
    }

    private void HandleCameraMove()
    {
        var vec = _lookInput * (mouseSensitivity * Time.deltaTime);
        _xRotation -= vec.y;
        _xRotation = Mathf.Clamp(_xRotation, -30f, 10f);
        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * vec.x);
    }

    private void HandleMove()
    {
        var move = transform.right * _moveInput.x + transform.forward * _moveInput.y;

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

    public void OnLook(InputValue value)
    {
        _lookInput = value.Get<Vector2>();
    }
}