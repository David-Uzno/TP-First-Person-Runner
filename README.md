# TP-Runner-3D: seguimiento corporal minimalista

Este proyecto muestra la webcam y dibuja un esqueleto 2D encima con MoveNet y Unity Inference Engine.

## Qué se conserva

- Captura de webcam en tiempo real.
- Inferencia de pose 2D con MoveNet SinglePose Lightning v4.
- Renderizado del esqueleto en UI.
- Una sola escena de ejemplo clara y manual.

## Dependencias

- Unity 6000.3.5f2
- com.unity.ai.inference 2.4.1
- Assets/Resources/PoseModels/movenet_lightning_v4.onnx

## Cómo usarlo

1. Revisa que la escena tenga esta estructura:
   - Canvas
   - Camera View con PoseTrackingController, PoseWebcamSource, RawImage y AspectRatioFitter
   - Skeleton Overlay como hijo de Camera View con PoseSkeletonGraphic
2. Pulsa Play.
3. Permite el acceso a la webcam cuando Unity lo pida.

## Scripts esenciales

- [Assets/Scripts/PoseTracking/PoseLandmark.cs](Assets/Scripts/PoseTracking/PoseLandmark.cs): dato simple de un punto de pose.
- [Assets/Scripts/PoseTracking/PoseSkeleton.cs](Assets/Scripts/PoseTracking/PoseSkeleton.cs): contenedor de los 17 keypoints y puntos derivados.
- [Assets/Scripts/PoseTracking/PoseWebcamSource.cs](Assets/Scripts/PoseTracking/PoseWebcamSource.cs): captura de webcam.
- [Assets/Scripts/PoseTracking/MoveNetPoseEstimator.cs](Assets/Scripts/PoseTracking/MoveNetPoseEstimator.cs): carga del modelo y ejecución de inferencia.
- [Assets/Scripts/PoseTracking/PoseSkeletonGraphic.cs](Assets/Scripts/PoseTracking/PoseSkeletonGraphic.cs): dibujo del esqueleto.
- [Assets/Scripts/PoseTracking/PoseTrackingController.cs](Assets/Scripts/PoseTracking/PoseTrackingController.cs): orquesta webcam, inferencia y renderizado.

## Qué se quitó del flujo principal

- Bootstrap automático en runtime.
- UI de estado y otros adornos no esenciales.
- Cámara 3D, luz y EventSystem de la escena de ejemplo.

## Notas técnicas

- MoveNet devuelve 17 keypoints base. El renderer sintetiza cuello, centro de cadera y cabeza para que la silueta sea más legible.
- El paquete de inferencia recibe texturas en rango [0, 1]. El modelo se envuelve con la Functional API para escalar la entrada al rango [0, 255] que espera este ONNX.
- El tamaño de entrada se lee del metadata real del ONNX en tiempo de carga, así que el tensor siempre coincide con el modelo incluido.

## Limitaciones

- La solución es 2D, no 3D.
- El modelo es single-pose, así que sigue a una sola persona.
- La calidad depende de la iluminación y del encuadre de la webcam.
