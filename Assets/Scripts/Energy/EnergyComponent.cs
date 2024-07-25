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

    public int CurrentShield
    {
        get => _currentShield;
        private set
        {
            _currentShield = value;
            UpdateUI();
        }
    }

    public bool IsShieldActivated => CurrentShield > 0;

    [Header("UI")]
    [SerializeField] private Image _lifeBarImage;
    [SerializeField] private Image _energyBarImage;
    [SerializeField] private Image _shieldBarImage;

    [Header("Stats")]
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _maxEnergy;
    [SerializeField] private int _maxShield;
    [SerializeField] private float _healingRate = 10f;

    [Header("Shield")]
    [SerializeField] private int _shieldRestorationMultiplier = 10;
    [SerializeField] private float _shieldCoolDown;

    [Header("Debug")]
    [SerializeField] private int _currentHealth;
    [SerializeField] private int _currentEnergy;
    [SerializeField] private int _currentShield;

    private Coroutine _healRoutine;
    private Coroutine _shieldRoutine;
    private bool _canActivateShield = true;

    private void Start()
    {
        CurrentHealth = _maxHealth;
        CurrentEnergy = _maxEnergy;
        CurrentShield = 0;
    }

    private void UpdateUI()
    {
        _lifeBarImage.fillAmount = (float)CurrentHealth / (float)_maxHealth;
        _energyBarImage.fillAmount = (float)CurrentEnergy / (float)_maxEnergy;
        _shieldBarImage.fillAmount = (float)CurrentShield / (float)_maxShield;
    }

    #region Health

    public void TakeDamage(int damage)
    {
        if (IsShieldActivated)
        {
            CurrentShield = Mathf.Max(CurrentShield - damage, 0);
            RestoreEnergy(damage * _shieldRestorationMultiplier);

            // Shield entra em cooldown
            if (CurrentShield == 0)
            {
                StartCoroutine(ShieldCooldownRoutine());
            }

            return;
        }

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
        CurrentEnergy = Mathf.Clamp(CurrentEnergy + amount, 0, _maxEnergy);
    }

    #endregion

    #region Shield

    public void ActivateShield()
    {
        if (_canActivateShield)
        {
            _canActivateShield = false;
            CurrentShield = _maxShield;
        }
    }

    private IEnumerator ShieldCooldownRoutine()
    {
        yield return new WaitForSeconds(_shieldCoolDown);
        _canActivateShield = true;
    }

    #endregion
}