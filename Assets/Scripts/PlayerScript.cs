using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerScript : MonoBehaviour
{
    private static readonly int Grounded = Animator.StringToHash("grounded");

    [Header("Movement")] [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private int maxHealth;

    [SerializeField] [Range(min: 1, max: 100)]
    private float mouseSensitivity;

    [SerializeField] private Transform cameraPivot;
    private CharacterController _controller;
    private Animator _animator;
    private WeaponScript _weapon;

    private int _health;
    private Vector3 _velocity;

    private Vector2 _moveInput;
    private bool _jumpPressed;
    private Vector2 _lookInput;
    private float _xRotation;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _weapon = GetComponentInChildren<WeaponScript>();

        _health = maxHealth;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(HandleWeapon());
        _weapon.OnHitDelegate += param => { param.HandleHealthChange(-1); };
    }

    private void Update()
    {
        HandleCameraMove();
        HandleMove();
        HandleHealth();
    }

    private IEnumerator HandleWeapon()
    {
        while (true)
        {
            //todo add parametrization
            var angle = Mathf.Sin(Time.time * 2) * 60f;
            _weapon.transform.localRotation = Quaternion.Euler(0, angle, 0);
            yield return null;
        }
    }

    private void HandleCameraMove()
    {
        var look = _lookInput * (mouseSensitivity * Time.deltaTime);

        _xRotation -= look.y;
        _xRotation = Mathf.Clamp(_xRotation, -10f, 10f);
        cameraPivot.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * look.x);
    }

    private void HandleMove()
    {
        var move =
            transform.right * _moveInput.x +
            transform.forward * _moveInput.y;

        _animator.SetBool(Grounded, _controller.isGrounded);

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

        var displacement = (move * moveSpeed + Vector3.up * _velocity.y) * Time.deltaTime;
        _controller.Move(displacement);
    }

    private void HandleHealth()
    {
        if (_health <= 0)
        {
            gameObject.SetActive(false);
        }
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