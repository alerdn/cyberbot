using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour, IHealth
{
    public void TakeDamage(int damage)
    {
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
