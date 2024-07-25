using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashOnHit : MonoBehaviour
{
    [SerializeField] private float _flashSpeed = .05f;

    private SpriteRenderer _renderer;
    private Coroutine _flashRoutine;

    private void Awake()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Flash()
    {
        if (!_renderer) return;

        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
        }

        _flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        _renderer.material.SetFloat("_FlashAmount", 1f);
        _renderer.material.SetColor("_FlashColor", Color.white);
        yield return new WaitForSeconds(_flashSpeed);
        _renderer.material.SetColor("_FlashColor", Color.red);
        yield return new WaitForSeconds(_flashSpeed);

        _renderer.material.SetFloat("_FlashAmount", 0f);
        _flashRoutine = null;
    }
}
