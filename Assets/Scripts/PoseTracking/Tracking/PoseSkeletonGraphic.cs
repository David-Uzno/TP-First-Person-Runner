using System.Collections.Generic;
using UnityEngine;

namespace TPRunner3D.PoseTracking
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
    public sealed class PoseSkeletonGraphic : MonoBehaviour
    {
        private readonly struct BoneSegment
        {
            public BoneSegment(PosePointId from, PosePointId to, Color color)
            {
                From = from;
                To = to;
                Color = color;
            }

            public readonly PosePointId From;
            public readonly PosePointId To;
            public readonly Color Color;
        }

        private static readonly Color LeftColor = new Color32(71, 199, 255, 255);
        private static readonly Color RightColor = new Color32(255, 170, 73, 255);
        private static readonly Color CenterColor = new Color32(101, 233, 162, 255);

        private static readonly PosePointId[] JointOrder =
        {
            PosePointId.Head,
            PosePointId.Neck,
            PosePointId.LeftShoulder,
            PosePointId.RightShoulder,
            PosePointId.LeftElbow,
            PosePointId.RightElbow,
            PosePointId.LeftWrist,
            PosePointId.RightWrist,
            PosePointId.HipCenter,
            PosePointId.LeftHip,
            PosePointId.RightHip,
            PosePointId.LeftKnee,
            PosePointId.RightKnee,
            PosePointId.LeftAnkle,
            PosePointId.RightAnkle,
        };

        private static readonly BoneSegment[] BoneSegments =
        {
            new(PosePointId.Head, PosePointId.Neck, CenterColor),
            new(PosePointId.Neck, PosePointId.LeftShoulder, CenterColor),
            new(PosePointId.Neck, PosePointId.RightShoulder, CenterColor),
            new(PosePointId.LeftShoulder, PosePointId.RightShoulder, CenterColor),
            new(PosePointId.LeftShoulder, PosePointId.LeftElbow, LeftColor),
            new(PosePointId.LeftElbow, PosePointId.LeftWrist, LeftColor),
            new(PosePointId.RightShoulder, PosePointId.RightElbow, RightColor),
            new(PosePointId.RightElbow, PosePointId.RightWrist, RightColor),
            new(PosePointId.Neck, PosePointId.HipCenter, CenterColor),
            new(PosePointId.LeftShoulder, PosePointId.LeftHip, CenterColor),
            new(PosePointId.RightShoulder, PosePointId.RightHip, CenterColor),
            new(PosePointId.LeftHip, PosePointId.RightHip, CenterColor),
            new(PosePointId.LeftHip, PosePointId.LeftKnee, LeftColor),
            new(PosePointId.LeftKnee, PosePointId.LeftAnkle, LeftColor),
            new(PosePointId.RightHip, PosePointId.RightKnee, RightColor),
            new(PosePointId.RightKnee, PosePointId.RightAnkle, RightColor),
        };

        [SerializeField] private float _jointSize = 10f;
        [SerializeField] private float _lineThickness = 6f;
        [SerializeField] [Range(0f, 1f)] private float _minimumConfidence = 0.25f;

        private PoseSkeleton _pose;

        private RectTransform _rectTransform;
        private CanvasRenderer _canvasRenderer;
        private Mesh _mesh;
        private Material _material;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasRenderer = GetComponent<CanvasRenderer>();

            _mesh = new Mesh { name = "PoseSkeletonMesh" };
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null) _material = new Material(shader);
            _canvasRenderer.SetMaterial(_material, null);
        }

        private void OnDestroy()
        {
            if (TryGetComponent(out CanvasRenderer canvasRender))
            {
                canvasRender.SetMesh(null);
            }
            if (_mesh != null)
            {
                Destroy(_mesh);
                _mesh = null;
            }

            if (_material != null)
            {
                Destroy(_material);
                _material = null;
            }
        }

        public void Refresh() => BuildMesh();

        public void SetPose(PoseSkeleton pose)
        {
            _pose = pose;
            BuildMesh();
        }

        private void BuildMesh()
        {
            if (_canvasRenderer == null) _canvasRenderer = GetComponent<CanvasRenderer>();
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

            if (_pose == null)
            {
                _canvasRenderer.SetMesh(null);
                return;
            }

            List<Vector3> poseVertices = new();
            List<int> poseTriangle = new();
            List<Color> poseJoints = new();
            List<Vector2> textureCoordinates = new();

            foreach (BoneSegment segment in BoneSegments)
            {
                if (!_pose.TryGetPoint(segment.From, out PoseLandmark from) || !_pose.TryGetPoint(segment.To, out PoseLandmark to))
                    continue;

                if (!from.IsVisible(_minimumConfidence) || !to.IsVisible(_minimumConfidence))
                    continue;

                AddLineMesh(poseVertices, poseTriangle, poseJoints, textureCoordinates, ToLocalPoint(from.Position), ToLocalPoint(to.Position), _lineThickness, segment.Color);
            }

            foreach (PosePointId jointId in JointOrder)
            {
                if (!_pose.TryGetPoint(jointId, out PoseLandmark point) || !point.IsVisible(_minimumConfidence))
                    continue;

                AddJointMesh(poseVertices, poseTriangle, poseJoints, textureCoordinates, ToLocalPoint(point.Position), _jointSize, Color.white);
            }

            _mesh.Clear();
            _mesh.SetVertices(poseVertices);
            _mesh.SetColors(poseJoints);
            if (textureCoordinates.Count == poseVertices.Count) _mesh.SetUVs(0, textureCoordinates);
            _mesh.SetTriangles(poseTriangle, 0);

            _canvasRenderer.SetMesh(_mesh);
        }

        private static void AddJointMesh(List<Vector3> vertexPositions, List<int> triangleIndices, List<Color> vertexColors, List<Vector2> textureCoordinates, Vector2 center, float size, Color jointColor)
        {
            float halfSize = size * 0.5f;

            Vector2 bottomLeftCorner = center + new Vector2(-halfSize, -halfSize);
            Vector2 topLeftCorner = center + new Vector2(-halfSize, halfSize);
            Vector2 topRightCorner = center + new Vector2(halfSize, halfSize);
            Vector2 bottomRightCorner = center + new Vector2(halfSize, -halfSize);

            AddQuadMesh(vertexPositions, triangleIndices, vertexColors, textureCoordinates, bottomLeftCorner, topLeftCorner, topRightCorner, bottomRightCorner, jointColor);
        }

        private static void AddLineMesh(List<Vector3> vertexPositions, List<int> triangleIndices, List<Color> vertexColors, List<Vector2> textureCoordinates, Vector2 start, Vector2 end, float thickness, Color lineColor)
        {
            Vector2 directionVector = end - start;
            if (directionVector.sqrMagnitude < 0.0001f)
                return;

            Vector2 normal = new Vector2(-directionVector.y, directionVector.x).normalized * (thickness * 0.5f);
            AddQuadMesh(vertexPositions, triangleIndices, vertexColors, textureCoordinates, start - normal, start + normal, end + normal, end - normal, lineColor);
        }

        private static void AddQuadMesh(List<Vector3> vertexPositions, List<int> triangleIndices, List<Color> vertexColors, List<Vector2> textureCoordinates, Vector2 bottomLeft, Vector2 topLeft, Vector2 topRight, Vector2 bottomRight, Color color)
        {
            int baseIndex = vertexPositions.Count;
            vertexPositions.Add(bottomLeft);
            vertexPositions.Add(topLeft);
            vertexPositions.Add(topRight);
            vertexPositions.Add(bottomRight);

            for (int i = 0; i < 4; i++)
            {
                vertexColors.Add(color);
                textureCoordinates.Add(Vector2.zero);
            }

            triangleIndices.AddRange(new[] { baseIndex, baseIndex + 1, baseIndex + 2, baseIndex + 2, baseIndex + 3, baseIndex });
        }

        private Vector2 ToLocalPoint(Vector2 normalizedPosition)
        {
            Rect rect = _rectTransform.rect;
            float x = Mathf.LerpUnclamped(rect.xMin, rect.xMax, normalizedPosition.x);
            float y = Mathf.LerpUnclamped(rect.yMax, rect.yMin, normalizedPosition.y);
            return new Vector2(x, y);
        }
    }
}
