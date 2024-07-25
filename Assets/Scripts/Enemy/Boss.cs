using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Boss : MonoBehaviour, IHealth
{
    public float CurrentHealthPercentage => ((float)_currentHealth / (float)_maxHealth) * 100f;
    public Animator Animator => _animator;
    public BossHand LeftHand => _leftHand;
    public BossHand RightHand => _rightHand;

    [Header("UI")]
    [SerializeField] private Image _lifeBarImage;
    [Header("Health")]
    [SerializeField] private int _maxHealth = 1000;

    [Header("Components")]
    [SerializeField] private BossHand _leftHand;
    [SerializeField] private BossHand _rightHand;

    [Header("Debug")]
    [SerializeField] private int _currentHealth;

    private BossStateBase _currentState;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _currentHealth = _maxHealth;

        SwitchState(new BossStartPhaseState(this));
    }

    private void Update()
    {
        _currentState?.OnTick(Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(LeftHand.transform.position, 4f);
    }

    public void SwitchState(BossStateBase state)
    {
        _currentState?.OnExit();
        _currentState = state;
        _currentState?.OnEnter();
    }

    public void TakeDamage(int damage)
    {
        _currentHealth = Mathf.Max(_currentHealth - damage, 0);
        UpdateUI();

        if (_currentHealth == 0)
        {
            OnDeath();
        }
    }

    public void OnDeath()
    {
        SwitchState(new BossDeathState(this));
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

public class BossStartPhaseState : BossStateBase
{
    private float _attackDelay = 2f;
    private List<Ability> _abilities;
    private Coroutine _attackRountine;

    public BossStartPhaseState(Boss stateMachine) : base(stateMachine)
    {
        _abilities = new List<Ability>()
        {
            new BossHandAbility(stateMachine, 3f, 1, stateMachine.LeftHand, stateMachine.RightHand),
           // new BossLaserAbility(3),
           // new BossPawnAbility(5)
        };
    }

    public override void OnEnter()
    {
        _attackRountine = stateMachine.StartCoroutine(AttackRoutine());
    }

    public override void OnTick(float deltaTime)
    {
        if (stateMachine.CurrentHealthPercentage < 50f)
        {
            _abilities = new List<Ability>()
            {
                new BossHandAbility(stateMachine, 3f, 2, stateMachine.LeftHand, stateMachine.RightHand),
                // new BossLaserAbility(6),
                // new BossPawnAbility(10)
            };
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
            Ability ability = _abilities.GetRandom();
            Debug.Log("Usando ability");
            yield return ability.Use();
            Debug.Log("Usou ability");
            yield return new WaitForSeconds(_attackDelay);
        }
    }
}

public class BossDeathState : BossStateBase
{
    public BossDeathState(Boss stateMachine) : base(stateMachine) { }

    public override void OnEnter()
    {
        Debug.Log("Boss derrotado");
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

public class BossPawnAbility : Ability
{
    public BossPawnAbility(params object[] args) { }

    public override IEnumerator Use()
    {
        Debug.Log($"Spawning Pawns");
        yield return new WaitForSeconds(1f);
    }
}

#endregion