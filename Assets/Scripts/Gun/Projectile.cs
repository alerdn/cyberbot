using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _lifeTime = 5f;

    private float _currentLifeTime;
    private Action<Projectile> _releaseCallback;

    public void Init(Action<Projectile> onRelease)
    {
        _releaseCallback = onRelease;
    }

    public void Launch(Vector3 shootPosition, Quaternion rotation)
    {
        transform.SetPositionAndRotation(shootPosition, rotation);
        _currentLifeTime = _lifeTime;
    }

    private void Update()
    {
        transform.Translate(Vector3.right * _speed * Time.deltaTime);
        _currentLifeTime -= Time.deltaTime;

        if (_currentLifeTime < 0)
        {
            _releaseCallback(this);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        _releaseCallback?.Invoke(this);
        Debug.Log($"Collided with {other.gameObject.name}.");
    }
}
