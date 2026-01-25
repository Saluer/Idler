using System;
using System.Collections;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerScript : MonoBehaviour
{
    //todo app optioon for dual pistols
    private static readonly int Grounded = Animator.StringToHash("grounded");
    public Action OnDeath;

    [SerializeField] private int maxHealth;

    [Header("Movement")] [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Weapon")] [SerializeField] [Range(min: 1, max: 20)]
    private float swingSpeed = 5f;

    [SerializeField] [Range(min: 10, max: 90)]
    private float swingRange = 5f;

    [Header("Camera")] [SerializeField] [Range(min: 1, max: 100)]
    private float mouseSensitivity;

    [SerializeField] [Range(min: 1, max: 50)]
    private float xMaxRotationAngle;

    private Vector3 _knockbackVelocity;
    [SerializeField] private float knockbackDamping = 8f;

    [SerializeField] private Transform cameraPivot;

    [SerializeField] private HealthBar healthBar;
    private CharacterController _controller;
    private Animator _animator;
    private MeleeWeaponScript _meleeWeapon;
    private RangedWeaponScript _rangedWeapon;

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
        _meleeWeapon = GetComponentInChildren<MeleeWeaponScript>();
        _rangedWeapon = GetComponentInChildren<RangedWeaponScript>();

        _meleeWeapon.gameObject.SetActive(false);
        _rangedWeapon.gameObject.SetActive(false);

        _health = maxHealth;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        healthBar.Init(maxHealth);
        if (_meleeWeapon) _meleeWeapon.OnHitDelegate += param => { param.HandleHealthChange(-1); };
    }

    private void Update()
    {
        if (GameManager.instance.gameMode != GameManager.GameMode.Active)
        {
            return;
        }

        HandleCameraMove();
        HandleHealth();
    }

    private void FixedUpdate()
    {
        if (GameManager.instance.gameMode != GameManager.GameMode.Active)
        {
            return;
        }

        HandleMove();
    }

    private void HandleWeapon()
    {
        if (_meleeWeapon && _meleeWeapon.gameObject.activeSelf)
            StartCoroutine(HandleMeleeWeapon());

        if (_rangedWeapon && _rangedWeapon.gameObject.activeSelf)
            StartCoroutine(HandleRangeWeapon());
    }

    public void EquipMelee()
    {
        if (_meleeWeapon.gameObject.activeSelf || GameManager.instance.goldAmount < 5)
        {
            return;
        }

        _meleeWeapon.gameObject.SetActive(true);
        _meleeWeapon.ToggleVisibility(false);
        StartCoroutine(HandleMeleeWeapon());

        GameManager.instance.IncrementGold(-5);
    }

    public void EquipRanged()
    {
        if (_rangedWeapon.gameObject.activeSelf || GameManager.instance.goldAmount < 10)
        {
            return;
        }

        _rangedWeapon.gameObject.SetActive(true);
        GameManager.instance.IncrementGold(-10);
        StartCoroutine(HandleRangeWeapon());
    }

    private IEnumerator HandleMeleeWeapon()
    {
        while (true)
        {
            if (GameManager.instance.gameMode != GameManager.GameMode.Active)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }

            _meleeWeapon.ToggleVisibility(true);
            for (var angle = 90.0f; angle > -270.0f; angle -= 15f)
            {
                _meleeWeapon.transform.localRotation = Quaternion.Euler(0, angle, 0);
                yield return new WaitForFixedUpdate();
            }

            _meleeWeapon.ToggleVisibility(false);
            yield return new WaitForSeconds(3f);
            yield return null;
        }
    }

    private IEnumerator HandleRangeWeapon()
    {
        while (true)
        {
            if (GameManager.instance.gameMode != GameManager.GameMode.Active)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }

            var closestEnemy = GameManager.instance.GetClosestEnemyTo(transform);

            if (!(closestEnemy && closestEnemy.TryGetComponent<Renderer>(out var component)))
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            var center = component.bounds.center;
            _rangedWeapon.Fire(center);
            yield return new WaitForSeconds(1f);
        }
    }

    private void HandleCameraMove()
    {
        var look = _lookInput * (mouseSensitivity * Time.deltaTime);

        transform.Rotate(Vector3.up * look.x);

        var prevPitch = _xRotation;
        _xRotation = Mathf.Clamp(_xRotation - look.y, -10, xMaxRotationAngle);

        var deltaPitch = _xRotation - prevPitch;
        if (Mathf.Abs(deltaPitch) < 0.0001f)
            return;

        cameraPivot.RotateAround(
            transform.position,
            transform.right,
            deltaPitch
        );
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

        var displacement = ((move + _knockbackVelocity) * moveSpeed + Vector3.up * _velocity.y) * Time.deltaTime;
        _controller.Move(displacement);

        _knockbackVelocity = Vector3.Lerp(
            _knockbackVelocity,
            Vector3.zero,
            knockbackDamping * Time.deltaTime
        );
    }

    private void HandleHealth()
    {
        if (_health > 0) return;

        if (BuffHub.ExtraLife > 0)
        {
            _health = maxHealth;
            BuffHub.ExtraLife--;
            return;
        }

        gameObject.SetActive(false);
        OnDeath();
    }

    public void IncreaseHealth(int delta)
    {
        _health = Mathf.Clamp(_health + delta, 0, maxHealth);
        healthBar.SetHealth(_health);
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.y = 0;
        direction.Normalize();

        _knockbackVelocity = direction * force;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (BuffHub.FireTouchDamage != 0)
        {
            if (collision.gameObject.TryGetComponent<EnemyScript>(out var enemy))
            {
                enemy.IncreaseHealth(-BuffHub.FireTouchDamage);
            }
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