using UnityEngine;
using TPRunner3D.PoseTracking;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _jumpForce = 4f;
    private bool _isJumping;
    private PoseDetectorVectorY _poseDetectorVectorY;

    [Header("References")]
    [SerializeField] private Rigidbody _rigidbody;

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

    /*private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Enemy"))
        {
            FindAnyObjectByType<GameManager>().LoseLife();
        }
    }*/
}
