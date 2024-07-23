using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IHealth
{
    [Header("Behaviour")]
    [SerializeField] private float _detectionDistance = 10f;
    [SerializeField] private float _attackDistance = 5f;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private Transform _forwardCollisionDetector;
    [SerializeField] private Transform _downwardCollisionDetector;

    [Header("Health")]
    [SerializeField] private int _maxHealth = 10;
    [Header("Debug")]
    [SerializeField] private int _currentHealth = 0;

    private Rigidbody2D _rb;
    private PlayerController _player;
    private bool _hasNoticedPlayer;
    private int direction = 1;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _player = PlayerController.Instance;
        _currentHealth = _maxHealth;
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        RaycastHit2D hitForward = Physics2D.Raycast(_forwardCollisionDetector.position, transform.right, 1f, LayerMask.GetMask("Wall"));
        Debug.DrawRay(_forwardCollisionDetector.position, transform.right, Color.red);
        if (hitForward)
        {
            // Bateu na parede
            direction *= -1;
        }

        RaycastHit2D hitDownward = Physics2D.Raycast(_downwardCollisionDetector.position, Vector2.down, 1f, LayerMask.GetMask("Ground"));
        Debug.DrawRay(_downwardCollisionDetector.position, Vector2.down, Color.red);
        if (!hitDownward)
        {
            // Prestes a sair da plataforma
            direction *= -1;
        }

        if (direction == 1)
        {
            transform.eulerAngles = Vector3.zero;
        }
        else
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }

        _rb.MovePosition(_rb.position + Vector2.right * direction * Time.fixedDeltaTime * _moveSpeed);
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
        Destroy(gameObject);
    }

    #endregion

}