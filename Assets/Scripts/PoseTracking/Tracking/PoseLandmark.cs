using UnityEngine;

namespace Runner3D.PoseTracking
{
    public readonly struct PoseLandmark
    {
        public PoseLandmark(Vector2 position, float confidence)
        {
            Position = position;
            Confidence = confidence;
        }

        public Vector2 Position { get; }
        public float Confidence { get; }

        public bool IsVisible(float minConfidence)
        {
            return Confidence >= minConfidence;
        }
    }
}
