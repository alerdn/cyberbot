using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Enemy : MonoBehaviour, IHealth
{
    [Header("Behaviour")]
    [SerializeField] private float _detectionDistance = 10f;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private Transform _forwardCollisionDetector;
    [SerializeField] private Transform _downwardCollisionDetector;

    [Header("Gun")]
    [SerializeField] private Transform _shootPosition;
    [SerializeField][Tooltip("Tiros por segundo")] private float _fireRate = 2;
    [SerializeField] private Projectile _projectile;
    [SerializeField] private AudioSource _shootAudio;

    [Header("Health")]
    [SerializeField] private int _maxHealth = 10;
    [SerializeField] private FlashOnHit _flashEffect;

    [Header("Debug")]
    [SerializeField] private int _currentHealth = 0;

    private IObjectPool<Projectile> _projectilePool;
    private Rigidbody2D _rb;
    private PlayerController _player;
    private bool _hasNoticedPlayer;
    private int direction = 1;
    private Coroutine _shootRoutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _player = PlayerController.Instance;
        _currentHealth = _maxHealth;

        _projectilePool = new ObjectPool<Projectile>(
            createFunc: () =>
            {
                Projectile projectile = Instantiate(_projectile);
                projectile.Init(onRelease: (Projectile projectile) =>
                {
                    if (projectile.isActiveAndEnabled)
                        _projectilePool.Release(projectile);
                });

                return projectile;
            }, actionOnGet: (Projectile projectile) =>
            {
                projectile.gameObject.SetActive(true);
                projectile.Launch(_shootPosition.position, _shootPosition.rotation);

            }, actionOnRelease: (Projectile projectile) =>
            {
                projectile.gameObject.SetActive(false);
            }, actionOnDestroy: (Projectile projectile) =>
            {
                Destroy(projectile.gameObject);
            }, collectionCheck: true, defaultCapacity: 50, maxSize: 100
        );
    }

    private void Update()
    {
        if (!_hasNoticedPlayer)
        {
            _hasNoticedPlayer = Vector2.Distance(_player.transform.position, _rb.position) < _detectionDistance;
        }
    }

    private void FixedUpdate()
    {
        if (!_hasNoticedPlayer)
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

            _rb.velocity = new Vector2(direction * _moveSpeed, _rb.velocity.y);
        }
        else
        {
            _shootRoutine ??= StartCoroutine(ShootRoutine());
        }
    }

    private IEnumerator ShootRoutine()
    {
        while (true)
        {
            Vector2 deltaPosition = _player.transform.position - transform.position;
            Vector3 gunEulerAngles;
            if (deltaPosition.x >= 0)
            {
                transform.localRotation = Quaternion.identity;
                gunEulerAngles = Vector3.forward * Mathf.Atan2(deltaPosition.y, deltaPosition.x) * Mathf.Rad2Deg;
                gunEulerAngles.y = 0f;
            }
            else
            {
                transform.localRotation = Quaternion.Euler(Vector3.up * 180f);
                gunEulerAngles = Vector3.forward * Mathf.Atan2(deltaPosition.y, -deltaPosition.x) * Mathf.Rad2Deg;
                gunEulerAngles.y = 180f;
            }

            _shootPosition.eulerAngles = gunEulerAngles;
            _projectilePool.Get();
            _shootAudio.Play();
            yield return new WaitForSeconds(1 / _fireRate);
        }
    }

    #region Health

    public void TakeDamage(int damage)
    {
        _flashEffect.Flash();
        _currentHealth = Mathf.Max(_currentHealth - damage, 0);

        if (_currentHealth == 0)
        {
            OnDeath();
        }
    }

    public void OnDeath()
    {
        _player.EnergyComp.RestoreEnergy(5);
        Destroy(gameObject);
    }

    #endregion

}