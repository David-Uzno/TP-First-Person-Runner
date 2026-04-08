using System;
using UnityEngine;
using Unity.InferenceEngine;

namespace TPRunner3D.PoseTracking
{
    public sealed class MoveNetPoseEstimator : IDisposable
    {
        public const string ModelResourcePath = "PoseModels/movenet_lightning_v4";

        private readonly Tensor<float> _inputTensor;
        private readonly TextureTransform _textureTransform;
        private readonly Worker _worker;
        private readonly BackendType _backendType;

        private MoveNetPoseEstimator(ModelAsset sourceModelAsset, BackendType backendType)
        {
            _backendType = backendType;

            Model sourceModel = ModelLoader.Load(sourceModelAsset);

            // TextureConverter genera valores en el intervalo [0, 1], mientras que el ONNX de MoveNet importado
            // espera píxeles RGB de tipo float32 en el intervalo [0, 255]. El wrapper graph soluciona ese problema.
            FunctionalGraph graph = new();
            graph.AddOutputs(Functional.Forward(sourceModel, graph.AddInput(sourceModel, 0) * 255f));

            Model runtimeModel = graph.Compile();
            TensorShape inputShape = ResolveInputShape(runtimeModel);
            _worker = new Worker(runtimeModel, backendType);
            _inputTensor = new Tensor<float>(inputShape);
            _textureTransform = new TextureTransform().SetTensorLayout(TensorLayout.NHWC);
        }

        public string BackendLabel
        {
            get
            {
                if (_backendType == BackendType.GPUCompute)
                {
                    return "GPU Compute";
                }
                return "CPU";
            }
        }

        public void Dispose()
        {
            _worker?.Dispose();
            _inputTensor?.Dispose();
        }

        public bool TryEstimate(Texture texture, PoseSkeleton skeleton)
        {
            if (texture == null || skeleton == null)
            {
                return false;
            }

            TextureConverter.ToTensor(texture, _inputTensor, _textureTransform);
            _worker.Schedule(_inputTensor);

            if (_worker.PeekOutput() is not Tensor<float> outputTensor)
            {
                return false;
            }

            float[] data = outputTensor.DownloadToArray();
            if (data == null || data.Length < PoseSkeleton.BasePointCount * 3)
            {
                return false;
            }

            skeleton.Clear();
            for (int index = 0; index < PoseSkeleton.BasePointCount; index++)
            {
                int baseOffset = index * 3;
                float y = Mathf.Clamp01(data[baseOffset + 0]);
                float x = Mathf.Clamp01(data[baseOffset + 1]);
                float score = Mathf.Clamp01(data[baseOffset + 2]);
                skeleton.SetBasePoint(index, new PoseLandmark(new Vector2(x, y), score));
            }

            return true;
        }

        public static bool TryCreateDefault(out MoveNetPoseEstimator estimator, out string error)
        {
            ModelAsset modelAsset = Resources.Load<ModelAsset>(ModelResourcePath);
            if (modelAsset == null)
            {
                estimator = null;
                error = "No se pudo cargar el modelo MoveNet desde Resources/PoseModels.";
                return false;
            }
            BackendType backendType;
            if (SystemInfo.supportsComputeShaders)
            {
                backendType = BackendType.GPUCompute;
            }
            else
            {
                backendType = BackendType.CPU;
            }
            estimator = new MoveNetPoseEstimator(modelAsset, backendType);
            error = null;
            return true;
        }

        private static TensorShape ResolveInputShape(Model runtimeModel)
        {
            if (runtimeModel == null || runtimeModel.inputs == null || runtimeModel.inputs.Count == 0)
            {
                throw new InvalidOperationException("El modelo MoveNet no expone ninguna entrada de inferencia.");
            }

            if (!runtimeModel.inputs[0].shape.IsStatic())
            {
                throw new InvalidOperationException("El modelo MoveNet debe exponer un shape de entrada estático.");
            }
            TensorShape inputShape = runtimeModel.inputs[0].shape.ToTensorShape();
            if (inputShape.rank != 4)
            {
                throw new InvalidOperationException($"Se esperaba una entrada rank-4 para MoveNet y se recibió {inputShape}.");
            }

            if (inputShape[0] != 1 || inputShape[3] != 3)
            {
                throw new InvalidOperationException($"Se esperaba una entrada NHWC con shape (1, alto, ancho, 3) y se recibió {inputShape}.");
            }

            return inputShape;
        }
    }
}
