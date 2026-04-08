using UnityEngine;
using UnityEngine.UI;

namespace TPRunner3D.PoseTracking
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(RawImage))]
    public sealed class PoseCollisionZoneGraphic : MonoBehaviour
    {
        [SerializeField, Tooltip("Centro normalizado de la zona de colisión. X crece hacia la derecha y Y hacia abajo.")]
        private Vector2 _normalizedCenter = new(0.5f, 0.35f);

        [SerializeField, Tooltip("Tamaño normalizado de la zona de colisión.")]
        private Vector2 _normalizedSize = new(0.25f, 0.2f);

        [SerializeField, Tooltip("Color visual del rectángulo.")]
        private Color _fillColor = new(1f, 0.92f, 0.18f, 0.22f);

        private RectTransform _rectTransform;
        private RawImage _rawImage;

        public Rect NormalizedRect
        {
            get
            {
                Vector2 halfSize = _normalizedSize * 0.5f;
                return new Rect(_normalizedCenter.x - halfSize.x, _normalizedCenter.y - halfSize.y, _normalizedSize.x, _normalizedSize.y);
            }
        }

        private void Awake()
        {
            CacheComponents();
            ApplyVisualState();
        }

        private void OnEnable()
        {
            ApplyVisualState();
        }

        private void OnValidate()
        {
            SanitizeValues();
            ApplyVisualState();
        }

        private void LateUpdate()
        {
            ApplyLayout();
        }

        public bool ContainsPoint(Vector2 normalizedPoint, float margin = 0f)
        {
            Rect rect = NormalizedRect;
            rect.xMin -= margin;
            rect.yMin -= margin;
            rect.xMax += margin;
            rect.yMax += margin;
            return rect.Contains(normalizedPoint);
        }

        private void ApplyVisualState()
        {
            CacheComponents();
            SanitizeValues();

            if (_rawImage != null)
            {
                _rawImage.texture = Texture2D.whiteTexture;
                _rawImage.color = _fillColor;
                _rawImage.raycastTarget = false;
            }

            ApplyLayout();
        }

        private void ApplyLayout()
        {
            if (_rectTransform == null)
            {
                CacheComponents();
            }

            if (_rectTransform == null)
            {
                return;
            }

            RectTransform parentRect = _rectTransform.parent as RectTransform;
            if (parentRect == null)
            {
                return;
            }

            Vector2 parentSize = parentRect.rect.size;
            if (parentSize.sqrMagnitude <= 0f)
            {
                return;
            }

            Vector2 size = new(_normalizedSize.x * parentSize.x, _normalizedSize.y * parentSize.y);
            Vector2 anchoredPosition = new(
                (_normalizedCenter.x - 0.5f) * parentSize.x,
                (0.5f - _normalizedCenter.y) * parentSize.y);

            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.sizeDelta = size;
            _rectTransform.anchoredPosition = anchoredPosition;
        }

        private void CacheComponents()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            if (_rawImage == null)
            {
                _rawImage = GetComponent<RawImage>();
            }
        }

        private void SanitizeValues()
        {
            _normalizedCenter.x = Mathf.Clamp01(_normalizedCenter.x);
            _normalizedCenter.y = Mathf.Clamp01(_normalizedCenter.y);
            _normalizedSize.x = Mathf.Clamp(_normalizedSize.x, 0.01f, 1f);
            _normalizedSize.y = Mathf.Clamp(_normalizedSize.y, 0.01f, 1f);

            float halfWidth = _normalizedSize.x * 0.5f;
            float halfHeight = _normalizedSize.y * 0.5f;

            _normalizedCenter.x = Mathf.Clamp(_normalizedCenter.x, halfWidth, 1f - halfWidth);
            _normalizedCenter.y = Mathf.Clamp(_normalizedCenter.y, halfHeight, 1f - halfHeight);
        }
    }
}