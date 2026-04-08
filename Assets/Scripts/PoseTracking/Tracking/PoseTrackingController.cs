using UnityEngine;
using UnityEngine.UI;

namespace TPRunner3D.PoseTracking
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PoseWebcamSource))]
    [RequireComponent(typeof(RawImage))]
    [RequireComponent(typeof(AspectRatioFitter))]
    public sealed class PoseTrackingController : MonoBehaviour
    {
        private const float InferenceIntervalSeconds = 1f / 20f;

        [SerializeField] private PoseSkeletonGraphic _skeletonGraphic;

        private readonly PoseSkeleton _pose = new();

        public PoseSkeleton CurrentPose => _pose;

        public float LastPoseUpdateTime { get; private set; } = float.NegativeInfinity;

        private RawImage _cameraImage;
        private AspectRatioFitter _aspectRatioFitter;
        private PoseWebcamSource _webcamSource;
        private MoveNetPoseEstimator _estimator;
        private float _nextInferenceTime;

        private void Start()
        {
            _cameraImage = GetComponent<RawImage>();
            _aspectRatioFitter = GetComponent<AspectRatioFitter>();
            _webcamSource = GetComponent<PoseWebcamSource>();

            if (_cameraImage == null || _aspectRatioFitter == null || _webcamSource == null || _skeletonGraphic == null)
            {
                Debug.LogError("PoseTrackingController requiere RawImage, AspectRatioFitter, PoseWebcamSource y PoseSkeletonGraphic.");
                enabled = false;
                return;
            }

            _skeletonGraphic.SetPose(_pose);

            if (!MoveNetPoseEstimator.TryCreateDefault(out _estimator, out string startupError))
            {
                Debug.LogError(startupError ?? "No se pudo inicializar el estimador de pose.");
            }
        }

        private void Update()
        {
            UpdateView();

            if (_estimator == null || !_webcamSource.HasFreshFrame)
            {
                return;
            }

            if (Time.unscaledTime < _nextInferenceTime)
            {
                return;
            }

            _nextInferenceTime = Time.unscaledTime + InferenceIntervalSeconds;
            if (_estimator.TryEstimate(_webcamSource.Texture, _pose))
            {
                LastPoseUpdateTime = Time.unscaledTime;
                _skeletonGraphic.Refresh();
            }
        }

        private void OnDestroy()
        {
            _estimator?.Dispose();
        }

        private void UpdateView()
        {
            if (_webcamSource == null || _cameraImage == null || _aspectRatioFitter == null)
            {
                return;
            }

            if (_cameraImage.texture != _webcamSource.Texture)
            {
                _cameraImage.texture = _webcamSource.Texture;
            }

            if (!_webcamSource.IsReady)
            {
                return;
            }

            RectTransform cameraTransform = _cameraImage.rectTransform;
            _aspectRatioFitter.aspectRatio = _webcamSource.DisplayAspectRatio;
            cameraTransform.localEulerAngles = new Vector3(0f, 0f, -_webcamSource.RotationAngle);

            Vector3 scale = Vector3.one;
            if (_webcamSource.IsVerticallyMirrored)
            {
                scale.y *= -1f;
            }

            if (_webcamSource.IsFrontFacing)
            {
                scale.x *= -1f;
            }

            cameraTransform.localScale = scale;
        }
    }
}
