using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public struct PlatformState
{
    public bool Active;
    public Transform Platform;
}

public class Boss : MonoBehaviour, IHealth
{
    public float CurrentHealthPercentage => ((float)_currentHealth / (float)_maxHealth) * 100f;
    public Animator Animator => _animator;
    public BossHand LeftHand => _leftHand;
    public BossHand RightHand => _rightHand;
    public Collider2D Collider => _collider;

    [Header("UI")]
    [SerializeField] private Image _lifeBarImage;

    [Header("Health")]
    [SerializeField] private int _maxHealth = 300;
    [SerializeField] private FlashOnHit _flashEffect;
    [SerializeField] private EnergyDrop _energyDropPrefab;
    [SerializeField] private float _energyDropChance;

    [Header("Components")]
    [SerializeField] private BossHand _leftHand;
    [SerializeField] private BossHand _rightHand;
    [SerializeField] private EnemyGenerator _enemyGenerator;

    [Header("Platforms")]
    [SerializeField] private List<PlatformState> _platforms;

    [Header("Debug")]
    [SerializeField] private int _currentHealth;

    private BossStateBase _currentState;
    private Animator _animator;
    private Collider2D _collider;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        _currentHealth = _maxHealth;

        SwitchState(new BossBattleState(this));
    }

    private void Update()
    {
        _currentState?.OnTick(Time.deltaTime);
    }

    public void SwitchState(BossStateBase state)
    {
        _currentState?.OnExit();
        _currentState = state;
        _currentState?.OnEnter();
    }

    public void TakeDamage(int damage)
    {
        _flashEffect.Flash();
        _currentHealth = Mathf.Max(_currentHealth - damage, 0);
        UpdateUI();

        if (Random.Range(0f, 10f) < _energyDropChance)
        {
            Instantiate(_energyDropPrefab, transform.position, Quaternion.identity);
        }

        if (_currentHealth == 0)
        {
            OnDeath();
        }
    }

    public void OnDeath()
    {
        SwitchState(new BossDeathState(this));
    }

    public void ClearEnemies()
    {
        _enemyGenerator.Enemies.FindAll(enemy => enemy != null).ForEach(enemy => Destroy(enemy.gameObject));
        _enemyGenerator.Stop();
    }

    public void DropPlatform(params int[] platformsIndex)
    {
        foreach (int index in platformsIndex)
        {
            PlatformState state = _platforms[index];
            if (!state.Active) continue;
            state.Active = false;

            StartCoroutine(DropPlatformRoutine(state.Platform));
        }
    }

    private IEnumerator DropPlatformRoutine(Transform platform)
    {
        yield return platform.DOShakePosition(3f, .5f, 10).WaitForCompletion();
        platform.DOMoveY(-10f, 1f).SetRelative();
    }

    private void UpdateUI()
    {
        _lifeBarImage.fillAmount = CurrentHealthPercentage / 100f;
    }
}

#region State Base

public interface IState
{
    void OnEnter();
    void OnTick(float deltaTime);
    void OnExit();
}

public abstract class BossStateBase : IState
{
    protected Boss stateMachine;

    public BossStateBase(Boss stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public abstract void OnEnter();
    public abstract void OnTick(float deltaTime);
    public abstract void OnExit();

}

#endregion

#region States

public class BossBattleState : BossStateBase
{
    private float _attackDelay = 5f;
    private List<Ability> _abilities;
    private Coroutine _attackRountine;
    private int _phaseIndex;

    public BossBattleState(Boss stateMachine) : base(stateMachine)
    {
        _phaseIndex = 1;
        _abilities = new List<Ability>()
        {
            new BossHandAbility(stateMachine, 3f, 1, stateMachine.LeftHand, stateMachine.RightHand),
           // new BossLaserAbility(3),
        };
    }

    public override void OnEnter()
    {
        _attackRountine = stateMachine.StartCoroutine(AttackRoutine());
    }

    public override void OnTick(float deltaTime)
    {
        // Phase Control
        if (_phaseIndex == 1 && stateMachine.CurrentHealthPercentage < 50f)
        {
            _phaseIndex = 2;

            _abilities = new List<Ability>()
            {
                new BossHandAbility(stateMachine, 3f, 2, stateMachine.LeftHand, stateMachine.RightHand),
                // new BossLaserAbility(6),
            };

            stateMachine.DropPlatform(0, 1);
            _attackDelay = 3f;
        }
        if (_phaseIndex == 2 && stateMachine.CurrentHealthPercentage < 25f)
        {
            _phaseIndex = 3;

            stateMachine.DropPlatform(2, 3);
            _attackDelay = 1f;
        }
    }

    public override void OnExit()
    {
        stateMachine.StopCoroutine(_attackRountine);
    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_attackDelay);

            Ability ability = _abilities.GetRandom();
            Debug.Log("Usando ability");
            yield return ability.Use();
            Debug.Log("Usou ability");
        }
    }
}

public class BossDeathState : BossStateBase
{
    public BossDeathState(Boss stateMachine) : base(stateMachine) { }

    public override void OnEnter()
    {
        stateMachine.Collider.enabled = false;

        stateMachine.LeftHand.Animator.SetTrigger("Idle");
        stateMachine.LeftHand.Collider.enabled = false;
        stateMachine.RightHand.Animator.SetTrigger("Idle");
        stateMachine.RightHand.Collider.enabled = false;

        stateMachine.Animator.SetTrigger("Death");
        stateMachine.ClearEnemies();
    }

    public override void OnTick(float deltaTime) { }

    public override void OnExit() { }
}

#endregion

#region Ability

public abstract class Ability
{
    public abstract IEnumerator Use();
}

public class BossHandAbility : Ability
{
    private Boss _stateMachine;
    private float _abilityDuration;
    private int _hitTimes;
    private BossHand _leftHand;
    private BossHand _rightHand;

    public BossHandAbility(Boss stateMachine, float abilityDuration, int hitTimes, BossHand leftHand, BossHand rightHand)
    {
        _stateMachine = stateMachine;
        _abilityDuration = abilityDuration;
        _hitTimes = hitTimes;
        _leftHand = leftHand;
        _rightHand = rightHand;
    }

    public override IEnumerator Use()
    {
        if (_hitTimes == 1)
        {
            int hand = Random.Range(1, 3);
            if (hand == 1)
            {
                LeftHandAttack();
            }
            else
            {
                RightHandAttack();
            }
        }
        else
        {
            LeftHandAttack();
            RightHandAttack();
        }

        yield return new WaitForSeconds(_abilityDuration);
        _leftHand.Collider.enabled = false;
        _rightHand.Collider.enabled = false;
    }

    private void LeftHandAttack()
    {
        string trigger = Random.Range(1, 3) == 1 ? "UpperAttack" : "BottomAttack";
        _stateMachine.LeftHand.Animator.SetTrigger(trigger);
        _leftHand.Collider.enabled = true;
    }

    private void RightHandAttack()
    {
        string trigger = Random.Range(1, 3) == 1 ? "UpperAttack" : "BottomAttack";
        _stateMachine.RightHand.Animator.SetTrigger(trigger);
        _rightHand.Collider.enabled = true;
    }
}

public class BossLaserAbility : Ability
{
    public BossLaserAbility(params object[] args) { }

    public override IEnumerator Use()
    {
        Debug.Log($"Attacking with lasers");
        yield return new WaitForSeconds(1f);
    }
}

#endregion