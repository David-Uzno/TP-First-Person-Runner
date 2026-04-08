using System.Collections;
using UnityEngine;

namespace TPRunner3D.PoseTracking
{
    [DisallowMultipleComponent]
    public sealed class PoseWebcamSource : MonoBehaviour
    {
        [SerializeField] private int _requestedWidth = 1280;
        [SerializeField] private int _requestedHeight = 720;
        [SerializeField] private int _requestedFPS = 30;

        private WebCamTexture _texture;
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

        private IEnumerator Start()
        {
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            }

            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.LogError("Permiso de webcam denegado.");
                yield break;
            }

            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.LogError("No se encontró ninguna webcam.");
                yield break;
            }
            WebCamDevice selectedDevice = SelectDevice(devices);
            _isFrontFacing = selectedDevice.isFrontFacing;
            _texture = new WebCamTexture(selectedDevice.name, _requestedWidth, _requestedHeight, _requestedFPS);
            _texture.Play();
        }

        private void OnDisable()
        {
            StopCamera();
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

        private void StopCamera()
        {
            if (_texture == null)
            {
                return;
            }

            if (_texture.isPlaying)
            {
                _texture.Stop();
            }

            _texture = null;
        }
    }
}
