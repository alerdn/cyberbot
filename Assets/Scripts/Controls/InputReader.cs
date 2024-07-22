using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(fileName = "InputReader", menuName = "Controls/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event Action<bool> ShootEvent;

    public Vector2 MovementValue { get; private set; }
    public Vector2 AimValue { get; private set; }

    private Controls _controls;

    private void OnEnable()
    {
        _controls ??= new Controls();
        _controls.Player.SetCallbacks(this);
        _controls.Player.Enable();
    }

    private void OnDisable()
    {
        _controls.Player.Disable();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        MovementValue = context.ReadValue<Vector2>();
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        AimValue = context.ReadValue<Vector2>();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ShootEvent?.Invoke(true);
        }
        if (context.canceled)
        {
            ShootEvent?.Invoke(false);
        }
    }
}
