using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnergyComponent : MonoBehaviour, IHealth
{
    public int CurrentHealth
    {
        get => _currentHealth;
        private set
        {
            _currentHealth = value;
            UpdateUI();
        }
    }

    public int CurrentEnergy
    {
        get => _currentEnergy;
        private set
        {
            _currentEnergy = value;
            UpdateUI();
        }
    }

    [Header("UI")]
    [SerializeField] private Image _lifeBarImage;
    [SerializeField] private Image _energyBarImage;

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
        CurrentHealth = _maxHealth;
        CurrentEnergy = _maxEnergy;
    }

    private void UpdateUI()
    {
        _lifeBarImage.fillAmount = (float)CurrentHealth / (float)_maxHealth;
        _energyBarImage.fillAmount = (float)CurrentEnergy / (float)_maxEnergy;
    }

    #region Health

    public void TakeDamage(int damage)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        UpdateUI();

        if (CurrentHealth == 0)
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
            if (CurrentEnergy <= 0)
            {
                _healRoutine = null;
                yield break;
            }

            CurrentEnergy--;
            CurrentHealth = Mathf.Min(CurrentHealth + 1, _maxHealth);

            yield return new WaitForSeconds(1 / _healingRate);
        }
    }

    #endregion

    #region Energy
    public bool UseEnergy(int amount = 1)
    {
        if (CurrentEnergy < amount)
        {
            return false;
        }

        CurrentEnergy -= amount;
        return true;
    }

    public void RestoreEnergy(int amount)
    {
        CurrentEnergy = Mathf.Max(CurrentEnergy + amount, _maxEnergy);
    }

    #endregion
}