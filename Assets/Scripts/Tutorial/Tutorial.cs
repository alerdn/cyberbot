using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private GameObject _tutorial;
    private void Start()
    {
        _tutorial.SetActive(false);
        transform.DOMoveY(.5f, 1f).SetRelative().SetLoops(-1, LoopType.Yoyo);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
            _tutorial.SetActive(true);

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
            _tutorial.SetActive(false);
    }
}
