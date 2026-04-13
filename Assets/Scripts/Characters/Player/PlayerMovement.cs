using UnityEngine;
using Runner3D.PoseTracking;

public class PlayerMovement : MonoBehaviour
{
    [Header ("Jump")]
    [SerializeField] private float _jumpForce = 4f;
    private bool _isJumping;
    private PoseDetectorVectorY _poseDetectorVectorY;
    [SerializeField] private AudioClip _audioClip;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    [Header("References")]
    [SerializeField] private Rigidbody _rigidbody;

    private void Awake()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    private void OnEnable()
    {
        ResolvePoseDetector();

        if (_poseDetectorVectorY != null)
        {
            _poseDetectorVectorY.SpikeDetected += Jump;
        }
    }

    private void OnDisable()
    {
        if (_poseDetectorVectorY != null)
        {
            _poseDetectorVectorY.SpikeDetected -= Jump;
        }
    }

    private void Jump()
    {
        if (_isJumping == false)
        {
            _rigidbody.AddForce(_jumpForce * 10 * Vector3.up);
            if (_audioClip != null)
            {
                AudioSource.PlayClipAtPoint(_audioClip, transform.position);
            }

            _isJumping = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plataform"))
        {
            _isJumping = false;
        }
    }

    private void ResolvePoseDetector()
    {
        if (_poseDetectorVectorY != null)
        {
            return;
        }

        _poseDetectorVectorY = GetComponentInParent<PoseDetectorVectorY>();
        if (_poseDetectorVectorY == null)
        {
            _poseDetectorVectorY = FindFirstObjectByType<PoseDetectorVectorY>();
        }
    }

    public void ResetMovementState()
    {
        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.position = _initialPosition;
            _rigidbody.rotation = _initialRotation;
            _rigidbody.Sleep();
        }
        else
        {
            transform.SetPositionAndRotation(_initialPosition, _initialRotation);
        }

        _isJumping = false;
    }
}
