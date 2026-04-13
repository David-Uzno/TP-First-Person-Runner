using UnityEngine;

namespace Runner3D.PoseTracking
{
    public enum PosePointId
    {
        Nose = 0,
        LeftEye = 1,
        RightEye = 2,
        LeftEar = 3,
        RightEar = 4,
        LeftShoulder = 5,
        RightShoulder = 6,
        LeftElbow = 7,
        RightElbow = 8,
        LeftWrist = 9,
        RightWrist = 10,
        LeftHip = 11,
        RightHip = 12,
        LeftKnee = 13,
        RightKnee = 14,
        LeftAnkle = 15,
        RightAnkle = 16,
        Neck = 17,
        HipCenter = 18,
        Head = 19,
    }

    public sealed class PoseSkeleton
    {
        public const int BasePointCount = 17;

        private readonly PoseLandmark[] _basePoints = new PoseLandmark[BasePointCount];

        public void Clear()
        {
            for (int index = 0; index < _basePoints.Length; index++)
            {
                _basePoints[index] = default;
            }
        }

        public void SetBasePoint(int index, PoseLandmark point)
        {
            if ((uint)index >= BasePointCount)
            {
                return;
            }

            _basePoints[index] = point;
        }

        public bool TryGetPoint(PosePointId pointId, out PoseLandmark point)
        {
            return pointId switch
            {
                PosePointId.Neck => TryGetMidpoint(PosePointId.LeftShoulder, PosePointId.RightShoulder, out point),
                PosePointId.HipCenter => TryGetMidpoint(PosePointId.LeftHip, PosePointId.RightHip, out point),
                PosePointId.Head => TryGetHead(out point),
                _ => TryGetBasePoint(pointId, out point),
            };
        }

        private bool TryGetBasePoint(PosePointId pointId, out PoseLandmark point)
        {
            int index = (int)pointId;
            if ((uint)index < BasePointCount)
            {
                point = _basePoints[index];
                return point.Confidence > 0f;
            }

            point = default;
            return false;
        }

        private bool TryGetHead(out PoseLandmark point)
        {
            if (TryGetBasePoint(PosePointId.Nose, out point))
            {
                return true;
            }

            if (TryGetMidpoint(PosePointId.LeftEye, PosePointId.RightEye, out point))
            {
                return true;
            }

            return TryGetMidpoint(PosePointId.LeftEar, PosePointId.RightEar, out point);
        }

        private bool TryGetMidpoint(PosePointId leftId, PosePointId rightId, out PoseLandmark point)
        {
            if (TryGetBasePoint(leftId, out PoseLandmark left) && TryGetBasePoint(rightId, out PoseLandmark right))
            {
                point = new PoseLandmark((left.Position + right.Position) * 0.5f, Mathf.Min(left.Confidence, right.Confidence));
                return true;
            }

            point = default;
            return false;
        }
    }
}
