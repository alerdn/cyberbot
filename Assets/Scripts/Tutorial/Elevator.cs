using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool _entered;

    public void LoadBossLevel()
    {
        SceneManager.LoadScene(2);
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
}
