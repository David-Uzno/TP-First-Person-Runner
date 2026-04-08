using TPRunner3D.PoseTracking;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField] private PoseDetectorVectorY _poseDetector;
    [SerializeField] private Vector3 _jumpImpulse = new Vector3(0f, 5f, 0f);
    [SerializeField] private ForceMode _forceMode = ForceMode.Impulse;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        ResolveDetector();
    }

    private void OnEnable()
    {
        ResolveDetector();
        SubscribeToDetector();
    }

    private void OnDisable()
    {
        UnsubscribeFromDetector();
    }

    private void ResolveDetector()
    {
        if (_poseDetector != null)
        {
            return;
        }

        _poseDetector = FindFirstObjectByType<PoseDetectorVectorY>();
    }

    private void SubscribeToDetector()
    {
        if (_poseDetector == null)
        {
            return;
        }

        _poseDetector.YSpikeDetected -= HandleYSpikeDetected;
        _poseDetector.YSpikeDetected += HandleYSpikeDetected;
    }

    private void UnsubscribeFromDetector()
    {
        if (_poseDetector == null)
        {
            return;
        }

        _poseDetector.YSpikeDetected -= HandleYSpikeDetected;
    }

    private void HandleYSpikeDetected()
    {
        if (_rigidbody == null)
        {
            return;
        }

        _rigidbody.AddForce(_jumpImpulse, _forceMode);
    }
}
