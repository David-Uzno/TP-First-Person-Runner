using UnityEngine;

namespace Runner3D.PoseTracking
{
    [DisallowMultipleComponent]
    public sealed class PoseDetector : MonoBehaviour
    {
        [SerializeField] private PoseTrackingController _trackingController;
        [SerializeField] private PoseCollisionZoneGraphic _collisionZone;

        [SerializeField, Range(0f, 1f)] private float _minimumHeadConfidence = 0.25f;

        [SerializeField, Min(0f)] private float _maximumPoseAge = 0.25f;

        [SerializeField, Tooltip("Margen normalizado aplicado a la zona visual. Valores positivos amplían la detección y valores negativos la vuelven más estricta.")]
        private float _detectionMargin = 0.01f;

        private bool _isColliding;

        private void Awake()
        {
            ResolveReferences();
        }

        private void LateUpdate()
        {
            if (_trackingController == null || _collisionZone == null)
            {
                ResolveReferences();
            }

            if (_trackingController == null || _collisionZone == null)
            {
                ResetCollisionState();
                return;
            }

            PoseSkeleton pose = _trackingController.CurrentPose;
            if (pose == null || !pose.TryGetPoint(PosePointId.Head, out PoseLandmark head) || !head.IsVisible(_minimumHeadConfidence))
            {
                ResetCollisionState();
                return;
            }

            if (Time.unscaledTime - _trackingController.LastPoseUpdateTime > _maximumPoseAge)
            {
                ResetCollisionState();
                return;
            }

            bool isColliding = _collisionZone.ContainsPoint(head.Position, _detectionMargin);
            if (isColliding && !_isColliding)
            {
                Debug.Log("Colisión detectada");
            }

            _isColliding = isColliding;
        }

        private void ResetCollisionState()
        {
            _isColliding = false;
        }

        private void ResolveReferences()
        {
            if (_trackingController == null)
            {
                _trackingController = FindFirstObjectByType<PoseTrackingController>();
            }

            if (_collisionZone == null)
            {
                _collisionZone = FindFirstObjectByType<PoseCollisionZoneGraphic>();
            }
        }
    }
}