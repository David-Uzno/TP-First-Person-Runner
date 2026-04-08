using UnityEngine;

namespace TPRunner3D.PoseTracking
{
    [DisallowMultipleComponent]
    public sealed class PoseDetectorVectorY : MonoBehaviour
    {
        private const float MinimumHeadConfidence = 0.25f;
        private const float MaximumPoseAgeSeconds = 0.25f;

        [SerializeField, Min(0f)] private float _detectionThreshold = 0.08f;
        [SerializeField, Range(0.01f, 1f)] private float _smoothingFactor = 0.2f;

        private PoseTrackingController _trackingController;
        private bool _hasFilteredValue;
        private float _filteredY;
        private bool _isInSpikeState;

        private void Awake()
        {
            ResolveTrackingController();
            ResetFilter();
        }

        private void LateUpdate()
        {
            if (_trackingController == null)
            {
                ResolveTrackingController();
            }

            if (_trackingController == null)
            {
                ResetFilter();
                return;
            }

            if (Time.unscaledTime - _trackingController.LastPoseUpdateTime > MaximumPoseAgeSeconds)
            {
                ResetFilter();
                return;
            }

            PoseSkeleton pose = _trackingController.CurrentPose;
            if (pose == null || !pose.TryGetPoint(PosePointId.Head, out PoseLandmark head) || !head.IsVisible(MinimumHeadConfidence))
            {
                ResetFilter();
                return;
            }

            float currentY = head.Position.y;
            if (!_hasFilteredValue)
            {
                _filteredY = currentY;
                _hasFilteredValue = true;
                return;
            }

            _filteredY = Mathf.Lerp(_filteredY, currentY, _smoothingFactor);
            float deviation = Mathf.Abs(currentY - _filteredY);

            if (!_isInSpikeState && deviation >= _detectionThreshold)
            {
                Debug.Log("Cambio brusco en Y detectado");
                _isInSpikeState = true;
            }
            else if (_isInSpikeState && deviation <= _detectionThreshold * 0.5f)
            {
                _isInSpikeState = false;
            }
        }

        private void ResolveTrackingController()
        {
            if (_trackingController != null)
            {
                return;
            }

            _trackingController = GetComponentInParent<PoseTrackingController>();
            if (_trackingController == null)
            {
                _trackingController = FindFirstObjectByType<PoseTrackingController>();
            }
        }

        private void ResetFilter()
        {
            _hasFilteredValue = false;
            _filteredY = 0f;
            _isInSpikeState = false;
        }
    }
}