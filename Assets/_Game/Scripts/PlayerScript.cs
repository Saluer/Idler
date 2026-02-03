using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Weapon;
using DefaultNamespace;
using DefaultNamespace.weapon;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CharacterController))]
public class PlayerScript : MonoBehaviour
{
    public Action OnDeath;

    [SerializeField] private int maxHealth;

    [Header("Movement")] [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Camera")] [SerializeField] private float mouseSensitivity;
    [SerializeField] private float xMaxRotationAngle;
    [SerializeField] private Transform cameraPivot;

    [Header("Weapons")] [SerializeField] private SwordScript meleeWeapon;
    [SerializeField] private List<MonoBehaviour> ownedWeaponBehaviours;

    private readonly List<IWeapon> _ownedWeapons = new();

    private CharacterController _controller;
    private Vector3 _velocity;
    private int _health;

    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _jumpPressed;
    private float _xRotation;
    private Vector3 _knockbackVelocity;

    [SerializeField] private float knockbackDamping = 8f;

    [SerializeField] private HealthBar healthBar;

    [Header("Weapon Prefabs")] [SerializeField]
    private GameObject swordPrefab;

    [SerializeField] private GameObject pistolPrefab;
    [SerializeField] private GameObject shotgunPrefab;
    [SerializeField] private GameObject rocketLauncherPrefab;


    public enum WeaponType
    {
        Sword = 0,
        Pistol = 1,
        Shotgun = 2,
        RocketLauncher = 3
    }

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _health = maxHealth;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Собираем IWeapon из инспектора
        foreach (var mb in ownedWeaponBehaviours)
        {
            if (mb is IWeapon weapon)
            {
                _ownedWeapons.Add(weapon);
                weapon.Disable();
            }
        }
    }

    private void Update()
    {
        if (GameManager.instance.gameMode != GameManager.GameMode.Active)
            return;

        HandleCameraMove();
        HandleHealth();
        HandleWeapons();
    }

    private void FixedUpdate()
    {
        HandleMove();
    }

    private void HandleWeapons()
    {
        var enemy = GameManager.instance.GetClosestEnemyTo(transform);
        if (!enemy)
            return;

        var target = enemy.transform;

        foreach (var weapon in _ownedWeapons)
        {
            weapon.TryAttack(target);
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

        cameraPivot.RotateAround(transform.position, transform.right, deltaPitch);
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
        _controller.Move((move * moveSpeed + Vector3.up * _velocity.y) * Time.deltaTime);
    }

    private void HandleHealth()
    {
        if (_health > 0) return;

        gameObject.SetActive(false);
        OnDeath?.Invoke();
    }

    public void BuyWeapon(int weaponType)
    {
        var type = (WeaponType)weaponType;

        if (!CanBuy(type))
            return;

        var weaponGO = Instantiate(GetPrefab(type), transform);
        if (!weaponGO.TryGetComponent<IWeapon>(out var weapon))
        {
            Destroy(weaponGO);
            Debug.LogError("Prefab has no IWeapon");
            return;
        }

        weapon.Enable();
        _ownedWeapons.Add(weapon);

        SpendGold(type);
    }

    private GameObject GetPrefab(WeaponType type)
    {
        return type switch
        {
            WeaponType.Sword => swordPrefab,
            WeaponType.Pistol => pistolPrefab,
            WeaponType.Shotgun => shotgunPrefab,
            WeaponType.RocketLauncher => rocketLauncherPrefab,
            _ => null
        };
    }

    private bool CanBuy(WeaponType type)
    {
        var cost = GetCost(type);
        return GameManager.instance.goldAmount >= cost;
    }

    private void SpendGold(WeaponType type)
    {
        GameManager.instance.IncrementGold(-GetCost(type));
    }

    private int GetCost(WeaponType type)
    {
        return type switch
        {
            WeaponType.Sword => 5,
            WeaponType.Pistol => 10,
            WeaponType.Shotgun => 20,
            WeaponType.RocketLauncher => 40,
            _ => int.MaxValue
        };
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

    public void ApplySpeedBuff(float bonus, float duration)
    {
        StartCoroutine(SpeedBuffCoroutine(bonus, duration));
    }

    private IEnumerator SpeedBuffCoroutine(float bonus, float duration)
    {
        moveSpeed += bonus;
        yield return new WaitForSeconds(duration);
        moveSpeed -= bonus;
    }

    public void ApplyAttackSpeedBuff(float bonus, float duration)
    {
        // StartCoroutine(SpeedAttackBuffCoroutine(bonus, duration));
    }


    // Input
    public void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();

    public void OnJump()
    {
        if (_controller.isGrounded) _jumpPressed = true;
    }

    public void OnLook(InputValue value) => _lookInput = value.Get<Vector2>();
}