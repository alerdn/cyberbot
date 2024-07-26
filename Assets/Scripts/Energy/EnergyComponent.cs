using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] private GameObject _deathScreen;
    [SerializeField] private Image _lifeBarImage;
    [SerializeField] private Image _energyBarImage;
    [SerializeField] private Image _shieldBarImage;
    [SerializeField] private Image _shieldIcon;
    [SerializeField] private TMP_Text _shieldCooldownText;

    [Header("Stats")]
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _maxEnergy;
    [SerializeField] private int _maxShield;
    [SerializeField] private float _healingRate = 10f;
    [SerializeField] private FlashOnHit _flashEffect;

    [Header("Shield")]
    [SerializeField] private SpriteRenderer _shieldRenderer;
    [SerializeField] private int _shieldRestorationMultiplier = 10;
    [SerializeField] private float _shieldDuration;
    [SerializeField] private float _shieldCoolDown;

    [Header("Audio")]
    [SerializeField] private AudioSource _shieldAudio;
    [SerializeField] private AudioSource _hitAudio;

    [Header("Debug")]
    [SerializeField] private int _currentHealth;
    [SerializeField] private int _currentEnergy;
    [SerializeField] private int _currentShield;

    private Coroutine _healRoutine;
    private bool _canActivateShield = true;
    private Coroutine _shieldRoutine;

    private void Start()
    {
        CurrentHealth = _maxHealth;
        CurrentEnergy = _maxEnergy;
        CurrentShield = 0;
        _deathScreen.SetActive(false);
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
        if (CurrentHealth == 0) return;

        if (IsShieldActivated)
        {
            CurrentShield = Mathf.Max(CurrentShield - damage, 0);
            RestoreEnergy(damage * _shieldRestorationMultiplier);

            // Shield entra em cooldown
            if (CurrentShield == 0)
            {
                DeactiveShield();
            }

            return;
        }

        _flashEffect.Flash();
        _hitAudio.Play();
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        UpdateUI();

        if (CurrentHealth == 0)
        {
            OnDeath();
        }
    }

    public void OnDeath()
    {
        _deathScreen.SetActive(true);
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            _shieldRoutine = StartCoroutine(ShieldRoutine());
            CurrentShield = _maxShield;
            _shieldRenderer.enabled = true;
            _shieldAudio.Play();
            _shieldIcon.enabled = true;
        }
    }

    private void DeactiveShield()
    {
        CurrentShield = 0;
        if (_shieldRoutine != null)
        {
            StopCoroutine(_shieldRoutine);
            _shieldRoutine = null;
        }
        _shieldAudio.Stop();
        _shieldRenderer.enabled = false;
        _shieldIcon.enabled = false;
        StartCoroutine(ShieldCooldownRoutine());
    }

    private IEnumerator ShieldRoutine()
    {
        yield return new WaitForSeconds(_shieldDuration);
        if (IsShieldActivated)
            DeactiveShield();
    }

    private IEnumerator ShieldCooldownRoutine()
    {
        _shieldCooldownText.text = _shieldCoolDown.ToString();
        yield return DOTween.To(() => int.Parse(_shieldCooldownText.text), (v) => _shieldCooldownText.text = v.ToString(), 0, _shieldCoolDown).WaitForCompletion();
        _shieldCooldownText.text = "Q";
        _canActivateShield = true;
    }

    #endregion
}