using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Armor : MonoBehaviour, IHealth
{
    public event Action OnDeathEvent;

    [field: SerializeField] public Transform SocketOffset { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject _hud;
    [SerializeField] private Image _lifeBarImage;
    [SerializeField] private Image _energyBarImage;

    [Header("Stats")]
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _maxEnergy;

    [Header("Debug")]
    [SerializeField] private int _currentHealth;
    [SerializeField] private int _currentEnergy;

    private Collider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        _hud.SetActive(false);
        _currentHealth = _maxHealth;
        _currentEnergy = _maxEnergy;
    }

    public void UpdateUI()
    {
        _lifeBarImage.fillAmount = (float)_currentHealth / (float)_maxHealth;
        _energyBarImage.fillAmount = (float)_currentEnergy / (float)_maxEnergy;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            if (player.EquipArmor(this))
            {
                _hud.SetActive(true);
                _collider.enabled = false;
            }
        }
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
        OnDeathEvent?.Invoke();
        Destroy(gameObject);
    }

    #endregion
}
