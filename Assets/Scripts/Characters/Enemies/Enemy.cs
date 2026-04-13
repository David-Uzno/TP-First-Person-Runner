using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private Vector3 _targetPosition = Vector3.zero;
    [SerializeField] private float _moveSpeed = 5f;

    [Header("References")]
    [SerializeField] private Rigidbody _rigidbody;

    private ObjectPool _objectPool;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        ObjectPool.TryGetInstance(out _objectPool);
    }

    private void FixedUpdate()
    {
        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 currentPosition = _rigidbody.position;
        Vector3 toTarget = _targetPosition - currentPosition;
        float distThisFrame = _moveSpeed * Time.fixedDeltaTime;

        if (toTarget.sqrMagnitude <= distThisFrame * distThisFrame)
        {
            _rigidbody.MovePosition(_targetPosition);
            if (_objectPool == null)
            {
                ObjectPool.TryGetInstance(out _objectPool);
            }
            if (_objectPool != null)
            {
                _objectPool.ReturnToPool(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            Vector3 newPosition = currentPosition + toTarget.normalized * distThisFrame;
            _rigidbody.MovePosition(newPosition);
        }
    }
}
