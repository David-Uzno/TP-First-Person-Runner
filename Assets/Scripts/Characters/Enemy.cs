using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private Vector3 _targetPosition = Vector3.zero;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Vector3 _resetPosition = Vector3.zero;

    [Header("Refences")]
    [SerializeField] private Rigidbody _rigidbody;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 currentPos = _rigidbody.position;
        Vector3 toTarget = _targetPosition - currentPos;
        float distThisFrame = _moveSpeed * Time.fixedDeltaTime;

        if (toTarget.sqrMagnitude <= distThisFrame * distThisFrame)
        {
            _rigidbody.MovePosition(_targetPosition);
        }
        else
        {
            Vector3 newPos = currentPos + toTarget.normalized * distThisFrame;
            _rigidbody.MovePosition(newPos);
        }
    }

    public void ResetPosition()
    {
        transform.position = _resetPosition;
        if (_rigidbody != null)
        {
            _rigidbody.position = _resetPosition;
            _rigidbody.Sleep();
        }
    }
}
