using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private UnityEvent _onInteract;
    [SerializeField] private GameObject _tutorial;

    private bool _entered;

    private void Start()
    {
        _tutorial.SetActive(false);
        transform.DOMoveY(.5f, 1f).SetRelative().SetLoops(-1, LoopType.Yoyo);
    }

    private void Update()
    {
        if (_entered && Keyboard.current.fKey.wasPressedThisFrame)
        {
            _onInteract?.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            _tutorial.SetActive(true);
            _entered = true;
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            _tutorial.SetActive(false);
            _entered = false;
        }
    }
}
