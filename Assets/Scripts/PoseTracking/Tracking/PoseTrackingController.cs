using UnityEngine;
using UnityEngine.UI;

namespace Runner3D.PoseTracking
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PoseWebcamSource))]
    public sealed class PoseTrackingController : MonoBehaviour
    {
        private const float InferenceIntervalSeconds = 1f / 20f;

        [SerializeField] private RawImage _cameraImage;
        [SerializeField] private PoseSkeletonGraphic _skeletonGraphic;

        private readonly PoseSkeleton _pose = new();

        public PoseSkeleton CurrentPose => _pose;

        public float LastPoseUpdateTime { get; private set; } = float.NegativeInfinity;

        private PoseWebcamSource _webcamSource;
        private MoveNetPoseEstimator _estimator;
        private float _nextInferenceTime;

        private void Start()
        {
            _webcamSource = GetComponent<PoseWebcamSource>();

            if (_cameraImage == null || _webcamSource == null || _skeletonGraphic == null)
            {
                Debug.LogError("PoseTrackingController requiere RawImage, PoseWebcamSource y PoseSkeletonGraphic.");
                enabled = false;
                return;
            }

            if (_cameraImage.TryGetComponent(out AspectRatioFitter aspectRatioFitter))
            {
                aspectRatioFitter.enabled = false;
            }

            _cameraImage.uvRect = new Rect(0f, 0f, 1f, 1f);
            _skeletonGraphic.SetPose(_pose);

            if (!MoveNetPoseEstimator.TryCreateDefault(out _estimator, out string startupError))
            {
                Debug.LogError(startupError ?? "No se pudo inicializar el estimador de pose.");
            }
        }

        private void OnDisable()
        {
            _estimator?.Dispose();
            _estimator = null;
            _nextInferenceTime = 0f;

            if (_cameraImage != null)
            {
                _cameraImage.texture = null;
                _cameraImage.uvRect = new Rect(0f, 0f, 1f, 1f);
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

        private void UpdateView()
        {
            if (_webcamSource == null || _cameraImage == null)
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

            float width = _webcamSource.IsFrontFacing ? -1f : 1f;
            float height = _webcamSource.IsVerticallyMirrored ? -1f : 1f;
            float x = width < 0f ? 1f : 0f;
            float y = height < 0f ? 1f : 0f;

            Rect targetUv = new Rect(x, y, width, height);
            if (_cameraImage.uvRect != targetUv)
            {
                _cameraImage.uvRect = targetUv;
            }
        }
    }
}
