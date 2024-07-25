using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public EnergyComponent EnergyComp => _energyComp;

    [Header("Layers")]
    [Tooltip("Set this to the layer your player is on")]
    [SerializeField] private LayerMask _playerLayer;

    [Header("Input")]
    [SerializeField] private InputReader _input;

    [Header("Components")]
    [SerializeField] private EnergyComponent _energyComp;
    [SerializeField] private Gun _gun;
    [SerializeField] private float _gunFollowSpeed = 25f;
    [SerializeField] private Transform _holderPosition;
    [SerializeField] private Animator _animator;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _acceleration = 100f;
    [SerializeField] private float _deceleration = 60f;
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _coyoteJumpTime = .15f;

    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private bool _cachedQueryStartInColliders;

    private float _time;
    private float _timeLeftGround;

    [Header("Debug")]
    [SerializeField] private bool _grounded;
    [SerializeField] private bool _canCoyoteJump;
    [SerializeField] private float _gravityScale;

    public bool CanCoyoteJump => _canCoyoteJump && _time < _timeLeftGround + _coyoteJumpTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Start()
    {
        _input.ShootEvent += OnShoot;
        _input.JumpEvent += OnJump;
        _input.HealEvent += OnHeal;
        _input.ActivateShieldEvent += OnActivateShield;

        _gravityScale = _rb.gravityScale;
    }

    private void OnDestroy()
    {
        _input.ShootEvent -= OnShoot;
        _input.JumpEvent -= OnJump;
        _input.HealEvent -= OnHeal;
        _input.ActivateShieldEvent -= OnActivateShield;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        Aim();
    }

    private void FixedUpdate()
    {
        CheckCollisions();
        Move();

        _gun.transform.position = Vector3.Lerp(_gun.transform.position, _holderPosition.position, _gunFollowSpeed * Time.fixedDeltaTime);
    }

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, 0.05f, ~_playerLayer);

        // Landed on the Ground
        if (!_grounded && groundHit)
        {
            _grounded = true;
            _canCoyoteJump = true;
        }
        // Left the Ground
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _timeLeftGround = _time;
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    private void Move()
    {
        _rb.gravityScale = _gravityScale;
        if (!_grounded && _rb.velocity.y <= 0)
        {
            _rb.gravityScale = _gravityScale * 1.5f;
        }

        float horizontalVelocity;
        if (_input.MovementValue.x == 0)
        {
            horizontalVelocity = Mathf.MoveTowards(_rb.velocity.x, 0f, _deceleration * Time.fixedDeltaTime);
        }
        else
        {
            horizontalVelocity = Mathf.MoveTowards(_rb.velocity.x, _input.MovementValue.x * _moveSpeed, _acceleration * Time.fixedDeltaTime);
        }

        _animator.SetFloat("MovementBlend", Mathf.Abs(_input.MovementValue.x));

        _rb.velocity = new Vector2(horizontalVelocity, _rb.velocity.y);
    }

    private void OnJump()
    {
        if (!_grounded && !CanCoyoteJump) return;
        _canCoyoteJump = false;

        _animator.SetTrigger("Jump");

        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
    }

    private void Aim()
    {
        Vector2 aimPosition = _input.AimValue;
        Vector2 playerPosition = Camera.main.WorldToScreenPoint(transform.position);

        // Gamepad
        if (aimPosition.magnitude <= 2f)
        {
            aimPosition += playerPosition;
        }

        Vector3 gunEulerAngles;
        Vector2 aimDirection = aimPosition - playerPosition;
        if (aimPosition.x > playerPosition.x)
        {
            transform.localRotation = Quaternion.identity;
            gunEulerAngles = Vector3.forward * Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            gunEulerAngles.y = 0f;
        }
        else
        {
            transform.localRotation = Quaternion.Euler(Vector3.up * 180f);
            gunEulerAngles = Vector3.forward * Mathf.Atan2(aimDirection.y, -aimDirection.x) * Mathf.Rad2Deg;
            gunEulerAngles.y = 180f;
        }

        _gun.Aim(aimPosition, gunEulerAngles);
    }

    private void OnShoot(bool isShooting)
    {
        _gun.Shoot(isShooting, _energyComp);
    }

    private void OnHeal(bool isHealing)
    {
        _energyComp.Heal(isHealing);
    }

    private void OnActivateShield()
    {
        EnergyComp.ActivateShield();
    }
}