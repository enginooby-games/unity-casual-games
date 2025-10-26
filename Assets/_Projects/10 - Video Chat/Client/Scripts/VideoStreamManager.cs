using UnityEngine;
using System.Collections.Generic;

namespace Devdy.VideoChat
{
    /// <summary>
    /// Manages local camera capture, video encoding, and remote video playback.
    /// Note: WebCamTexture has limited support in WebGL builds.
    /// </summary>
    public class VideoStreamManager : Singleton<VideoStreamManager>
    {
        #region Fields

        [SerializeField] private int targetFrameRate = 15;
        [SerializeField] private int jpegQuality = 75;
        [SerializeField] private int videoWidth = 640;
        [SerializeField] private int videoHeight = 480;

        private WebCamTexture localCamera;
        private Texture2D encodingTexture;
        private bool isStreaming;
        private float frameInterval;
        private float lastFrameTime;

        // Remote user videos
        private Dictionary<string, Texture2D> remoteVideoTextures = new Dictionary<string, Texture2D>();

        public RenderTexture LocalCameraTexture { get; private set; }

        #endregion ==================================================================

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            frameInterval = 1f / targetFrameRate;
            LocalCameraTexture = new RenderTexture(videoWidth, videoHeight, 0);
        }

        private void Start()
        {
            NetworkManager.Instance.OnVideoFrameReceived += HandleRemoteVideoFrame;
            NetworkManager.Instance.OnUserLeft += HandleUserLeft;
        }

        private void Update()
        {
            if (!isStreaming || localCamera == null || !localCamera.isPlaying) return;

            if (Time.time - lastFrameTime >= frameInterval)
            {
                CaptureAndSendFrame();
                lastFrameTime = Time.time;
            }
        }

        private void OnDestroy()
        {
            StopStreaming();
            
            if (LocalCameraTexture != null)
            {
                LocalCameraTexture.Release();
                Destroy(LocalCameraTexture);
            }

            foreach (var texture in remoteVideoTextures.Values)
            {
                if (texture != null) Destroy(texture);
            }
        }

        #endregion ==================================================================

        #region Local Camera

        /// <summary>
        /// Initializes and starts the local camera.
        /// </summary>
        public void StartCamera()
        {
            if (localCamera != null) return;

            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.LogError("No camera devices found");
#if UNITY_WEBGL && !UNITY_EDITOR
                Debug.LogWarning("WebGL: Camera access requires user permission. Make sure your page is served over HTTPS.");
#endif
                return;
            }

            localCamera = new WebCamTexture(devices[0].name, videoWidth, videoHeight, targetFrameRate);
            encodingTexture = new Texture2D(videoWidth, videoHeight, TextureFormat.RGB24, false);
            
            localCamera.Play();
            Debug.Log($"Camera started: {devices[0].name}");
        }

        public void StopCamera()
        {
            if (localCamera == null) return;

            localCamera.Stop();
            Destroy(localCamera);
            localCamera = null;

            if (encodingTexture != null)
            {
                Destroy(encodingTexture);
                encodingTexture = null;
            }
        }

        public void StartStreaming()
        {
            if (!NetworkManager.Instance.IsConnected)
            {
                Debug.LogWarning("Cannot start streaming: not connected to server");
                return;
            }

            if (localCamera == null) StartCamera();
            isStreaming = true;
            Debug.Log("Video streaming started");
        }

        public void StopStreaming()
        {
            isStreaming = false;
            Debug.Log("Video streaming stopped");
        }

        #endregion ==================================================================

        #region Frame Capture & Encoding

        private void CaptureAndSendFrame()
        {
            if (localCamera == null || !localCamera.isPlaying) return;

            // Copy camera texture to encoding texture
            Graphics.Blit(localCamera, LocalCameraTexture);
            
            RenderTexture.active = LocalCameraTexture;
            encodingTexture.ReadPixels(new Rect(0, 0, videoWidth, videoHeight), 0, 0);
            encodingTexture.Apply();
            RenderTexture.active = null;

            // Encode to JPEG
            byte[] jpegData = encodingTexture.EncodeToJPG(jpegQuality);

            // Send via network
            NetworkManager.Instance.SendVideoFrame(jpegData);
        }

        #endregion ==================================================================

        #region Remote Video

        private void HandleRemoteVideoFrame(string userId, byte[] frameData)
        {
            if (!remoteVideoTextures.ContainsKey(userId))
            {
                remoteVideoTextures[userId] = new Texture2D(2, 2);
            }

            Texture2D texture = remoteVideoTextures[userId];
            texture.LoadImage(frameData);
        }

        private void HandleUserLeft(string userId)
        {
            if (remoteVideoTextures.TryGetValue(userId, out Texture2D texture))
            {
                Destroy(texture);
                remoteVideoTextures.Remove(userId);
            }
        }

        public Texture2D GetRemoteVideoTexture(string userId)
        {
            return remoteVideoTextures.TryGetValue(userId, out Texture2D texture) ? texture : null;
        }

        #endregion ==================================================================

        #region Properties

        public bool IsStreaming => isStreaming;
        public bool IsCameraActive => localCamera != null && localCamera.isPlaying;

        #endregion ==================================================================
    }
}