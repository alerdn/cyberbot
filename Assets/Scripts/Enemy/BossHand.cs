using UnityEngine;

public class BossHand : MonoBehaviour
{
    public Animator Animator => _animator;
    public Collider2D Collider => _collider;

    [SerializeField] private int _damage;

    private Animator _animator;
    private Collider2D _collider;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out IHealth playerHealth))
        {
            playerHealth.TakeDamage(_damage);
        }
    }
}