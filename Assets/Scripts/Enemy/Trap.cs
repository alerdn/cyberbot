using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour, IHealth
{
    [SerializeField] private bool _isDamageable = false;

    public void TakeDamage(int damage)
    {
        if (!_isDamageable) return;

        OnDeath();
    }

    public void OnDeath()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent<IHealth>(out IHealth PlayerHealth))
        {
            PlayerHealth.TakeDamage(10);
        }
    }
}
