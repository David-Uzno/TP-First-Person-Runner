using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
#endif

namespace Runner3D.PoseTracking
{
    [DisallowMultipleComponent]
    public sealed class PoseWebcamSource : MonoBehaviour
    {
        private const int StartupAttempts = 4;
        private const float StartupRetryDelaySeconds = 0.2f;
        private const float StartupReadyTimeoutSeconds = 2f;
        private const int MaxDeviceCandidates = 5;

        [SerializeField] private int _requestedWidth = 1280;
        [SerializeField] private int _requestedHeight = 720;
        [SerializeField] private int _requestedFPS = 30;

        private WebCamTexture _texture;
        private Coroutine _startCameraRoutine;
        private bool _isFrontFacing;

        public bool HasFreshFrame => IsReady && _texture.didUpdateThisFrame;
        public bool IsFrontFacing => _isFrontFacing;
        public bool IsReady => _texture != null && _texture.isPlaying && _texture.width > 16 && _texture.height > 16;
        public bool IsVerticallyMirrored => _texture != null && _texture.videoVerticallyMirrored;
        public float DisplayAspectRatio
        {
            get
            {
                if (!IsReady)
                {
                    return 16f / 9f;
                }

                bool rotate = RotationAngle == 90 || RotationAngle == 270;
                
                if (rotate)
                {
                    return _texture.height / (float)_texture.width;
                }

                return _texture.width / (float)_texture.height;
            }
        }
        public int RotationAngle
        {
            get
            {
                if (_texture != null)
                {
                    return _texture.videoRotationAngle;
                }

                return 0;
            }
        }
        public Texture Texture => _texture;

        private void OnEnable()
        {
            if (_startCameraRoutine != null)
            {
                return;
            }

            _startCameraRoutine = StartCoroutine(StartCameraRoutine());
        }

        private IEnumerator StartCameraRoutine()
        {
            yield return null;

            if (!isActiveAndEnabled)
            {
                _startCameraRoutine = null;
                yield break;
            }

            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            }

            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.LogError("Permiso de webcam denegado.");
                _startCameraRoutine = null;
                yield break;
            }

            for (int attempt = 0; attempt < StartupAttempts; attempt++)
            {
                StopCamera(immediate: false);

                WebCamDevice[] devices = WebCamTexture.devices;

                List<string> candidateNames = new();

                if (devices.Length > 0)
                {
                    int limit = Mathf.Min(devices.Length, MaxDeviceCandidates);
                    int frontIndex = -1;
                    for (int i = 0; i < limit; i++)
                    {
                        if (devices[i].isFrontFacing)
                        {
                            frontIndex = i;
                            break;
                        }
                    }

                    if (frontIndex >= 0)
                    {
                        candidateNames.Add(devices[frontIndex].name);
                    }

                    for (int i = 0; i < limit; i++)
                    {
                        if (i == frontIndex) continue;
                        candidateNames.Add(devices[i].name);
                    }
                }
                else
                {
                    for (int i = 0; i < MaxDeviceCandidates; i++)
                    {
                        candidateNames.Add(null);
                    }
                }

                foreach (string candidate in candidateNames)
                {
                    if (candidate != null)
                    {
                        bool isFront = false;
                        for (int j = 0; j < devices.Length; j++)
                        {
                            if (devices[j].name == candidate)
                            {
                                isFront = devices[j].isFrontFacing;
                                break;
                            }
                        }

                        _isFrontFacing = isFront;
                        _texture = new WebCamTexture(candidate, _requestedWidth, _requestedHeight, _requestedFPS);
                    }
                    else
                    {
                        _isFrontFacing = false;
                        _texture = new WebCamTexture(_requestedWidth, _requestedHeight, _requestedFPS);
                    }

                    _texture.Play();

                    float timeoutAt = Time.realtimeSinceStartup + StartupReadyTimeoutSeconds;
                    while (_texture != null && Time.realtimeSinceStartup < timeoutAt)
                    {
                        if (IsReady)
                        {
                            _startCameraRoutine = null;
                            yield break;
                        }

                        yield return null;
                    }

                    StopCamera(immediate: false);
                    yield return new WaitForSecondsRealtime(StartupRetryDelaySeconds);
                }
            }

            Debug.LogError("PoseWebcamSource: No se pudo iniciar la webcam tras varios intentos.");
            _startCameraRoutine = null;
        }

        private void OnDisable()
        {
            StopPendingStart();
            StopCamera(immediate: !Application.isPlaying);
        }

        private void OnDestroy()
        {
            StopPendingStart();
            StopCamera(immediate: !Application.isPlaying);
        }

        private void OnApplicationQuit()
        {
            StopPendingStart();
            StopCamera(immediate: false);
        }

        private static WebCamDevice SelectDevice(WebCamDevice[] devices)
        {
            for (int index = 0; index < devices.Length; index++)
            {
                if (devices[index].isFrontFacing)
                {
                    return devices[index];
                }
            }

            return devices[0];
        }

        private void StopCamera(bool immediate)
        {
            if (_texture == null)
            {
                _isFrontFacing = false;
                return;
            }

            WebCamTexture textureToRelease = _texture;
            _texture = null;
            _isFrontFacing = false;

            if (textureToRelease.isPlaying)
            {
                textureToRelease.Stop();
            }

            if (immediate || !Application.isPlaying)
            {
                DestroyImmediate(textureToRelease);
                return;
            }

            Destroy(textureToRelease);
        }

        private void StopPendingStart()
        {
            if (_startCameraRoutine == null)
            {
                return;
            }

            StopCoroutine(_startCameraRoutine);
            _startCameraRoutine = null;
        }

        internal static void ForceReleaseAllCameras(bool immediate)
        {
            WebCamTexture[] textures = Resources.FindObjectsOfTypeAll<WebCamTexture>();
            for (int index = 0; index < textures.Length; index++)
            {
                WebCamTexture texture = textures[index];
                if (texture == null)
                {
                    continue;
                }

                if (texture.isPlaying)
                {
                    texture.Stop();
                }

                if (immediate || !Application.isPlaying)
                {
                    DestroyImmediate(texture);
                }
                else
                {
                    Destroy(texture);
                }
            }
        }
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
    internal static class PoseWebcamSourceEditorLifecycle
    {
        static PoseWebcamSourceEditorLifecycle()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;

            AssemblyReloadEvents.beforeAssemblyReload -= HandleBeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += HandleBeforeAssemblyReload;

            CompilationPipeline.compilationStarted -= HandleCompilationStarted;
            CompilationPipeline.compilationStarted += HandleCompilationStarted;
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode && state != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            PoseWebcamSource.ForceReleaseAllCameras(immediate: true);
        }

        private static void HandleBeforeAssemblyReload()
        {
            PoseWebcamSource.ForceReleaseAllCameras(immediate: true);
        }

        private static void HandleCompilationStarted(object context)
        {
            PoseWebcamSource.ForceReleaseAllCameras(immediate: true);
        }
    }
#endif
}
