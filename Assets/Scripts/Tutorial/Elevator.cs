using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour
{
    private bool _entered;

    private void Update()
    {
        if (_entered && Keyboard.current.fKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(2);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            _entered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            _entered = false;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene(2);
            }
        }
    }
}
