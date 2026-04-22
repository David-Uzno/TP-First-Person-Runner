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
    private float _defaultMoveSpeed;
    private EnemySpeedProgression _speedProgression;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _defaultMoveSpeed = _moveSpeed;
    }

    private void Start()
    {
        ObjectPool.TryGetInstance(out _objectPool);
    }

    public void SetMoveSpeed(float moveSpeed)
    {
        _speedProgression = null;
        _moveSpeed = Mathf.Max(0f, moveSpeed);
    }

    public void SetSpeedProgression(EnemySpeedProgression speedProgression)
    {
        _speedProgression = speedProgression;
    }

    public void ResetMoveSpeed()
    {
        _speedProgression = null;
        _moveSpeed = _defaultMoveSpeed;
    }

    public float GetConfiguredMoveSpeed()
    {
        return Mathf.Max(0f, _defaultMoveSpeed);
    }

    public Vector3 GetTravelDirectionFrom(Vector3 startPosition)
    {
        Vector3 toTarget = _targetPosition - startPosition;
        return toTarget.sqrMagnitude > 0f ? toTarget.normalized : Vector3.back;
    }

    private void FixedUpdate()
    {
        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 currentPosition = _rigidbody.position;
        Vector3 toTarget = _targetPosition - currentPosition;
        float activeMoveSpeed = _speedProgression != null ? _speedProgression.GetCurrentMoveSpeed() : _moveSpeed;
        float distThisFrame = activeMoveSpeed * Time.fixedDeltaTime;

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
