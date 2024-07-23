using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnergyComponent : MonoBehaviour, IHealth
{
    [Header("Stats")]
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _maxEnergy;
    [SerializeField] private float _healingRate = 10f;

    [Header("Debug")]
    [SerializeField] private int _currentHealth;
    [SerializeField] private int _currentEnergy;

    private Coroutine _healRoutine;

    private void Start()
    {
        _currentHealth = _maxHealth;
        _currentEnergy = _maxEnergy;
    }

    #region Health

    public void TakeDamage(int damage)
    {
        _currentHealth = Mathf.Max(_currentHealth - damage, 0);

        if (_currentHealth == 0)
        {
            OnDeath();
        }
    }

    public void OnDeath()
    {
        Debug.Log("Player morrey");
    }

    public void Heal(bool isHealing)
    {
        if (isHealing)
        {
            _healRoutine ??= StartCoroutine(HealRoutine());
        }
        else
        {
            if (_healRoutine == null) return;

            StopCoroutine(_healRoutine);
            _healRoutine = null;
        }
    }

    private IEnumerator HealRoutine()
    {
        while (true)
        {
            if (_currentEnergy <= 0)
            {
                _healRoutine = null;
                yield break;
            }

            _currentEnergy--;
            _currentHealth = Mathf.Min(_currentHealth + 1, _maxHealth);

            yield return new WaitForSeconds(1 / _healingRate);
        }
    }

    #endregion

    #region Energy
    public bool UseEnergy(int amount = 1)
    {
        if (_currentEnergy < amount)
        {
            return false;
        }

        _currentEnergy -= amount;
        return true;
    }

    public void RestoreEnergy(int amount)
    {
        _currentEnergy = Mathf.Max(_currentEnergy + amount, _maxEnergy);
    }

    #endregion
}