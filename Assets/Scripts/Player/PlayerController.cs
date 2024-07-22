using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader _input;

    [Header("Components")]
    [SerializeField] private Gun _gun;

    [SerializeField] private float _moveSpeed = 10f;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _input.ShootEvent += OnShoot;
    }


    private void Update()
    {
        Aim();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector2 direction = Vector2.right * _input.MovementValue * _moveSpeed;
        _rb.velocity = new Vector2(direction.x, _rb.velocity.y);
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
        }
        else
        {
            transform.localRotation = Quaternion.Euler(Vector3.up * 180f);
            gunEulerAngles = Vector3.forward * Mathf.Atan2(aimDirection.y, -aimDirection.x) * Mathf.Rad2Deg;
        }

        _gun.Aim(aimPosition, gunEulerAngles);
    }

    private void OnShoot(bool isShooting)
    {
        _gun.Shoot(isShooting);
    }
}
