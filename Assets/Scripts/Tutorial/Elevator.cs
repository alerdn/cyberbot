using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour
{
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
