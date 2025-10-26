using UnityEngine;
using System;
using System.Runtime.InteropServices;
using AOT;

namespace Devdy.VideoChat
{
    /// <summary>
    /// WebGL-compatible audio capture using JavaScript Web Audio API.
    /// </summary>
    public class WebGLAudioCapture : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void WebGLAudio_Init(int sampleRate, int chunkSize);

        [DllImport("__Internal")]
        private static extern int WebGLAudio_StartMicrophone(Action<IntPtr, int> callback);

        [DllImport("__Internal")]
        private static extern void WebGLAudio_StopMicrophone();

        [DllImport("__Internal")]
        private static extern int WebGLAudio_IsMicrophoneAvailable();

        [DllImport("__Internal")]
        private static extern int WebGLAudio_GetPermissionStatus();
#endif

        private static Action<byte[]> onAudioDataCallback;
        private bool isInitialized;
        private bool isCapturing;

        private int sampleRate = 16000;
        private int chunkSize = 1024;

        /// <summary>
        /// Initialize WebGL audio capture.
        /// </summary>
        public void Initialize(int sampleRate, int chunkSize)
        {
            this.sampleRate = sampleRate;
            this.chunkSize = chunkSize;

#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLAudio_Init(sampleRate, chunkSize);
            isInitialized = true;
            Debug.Log("WebGL Audio initialized");
#else
            Debug.LogWarning("WebGLAudioCapture only works in WebGL builds");
#endif
        }

        /// <summary>
        /// Start capturing audio from microphone.
        /// </summary>
        public void StartCapture(Action<byte[]> onAudioData)
        {
            if (!isInitialized)
            {
                Debug.LogError("WebGL Audio not initialized. Call Initialize() first.");
                return;
            }

            if (isCapturing)
            {
                Debug.LogWarning("Already capturing audio");
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            onAudioDataCallback = onAudioData;
            int result = WebGLAudio_StartMicrophone(OnAudioDataReceived);
            
            if (result == 1)
            {
                isCapturing = true;
                Debug.Log("WebGL Audio capture started");
            }
            else
            {
                Debug.LogError("Failed to start WebGL audio capture. Check browser console.");
            }
#else
            Debug.LogWarning("WebGLAudioCapture only works in WebGL builds");
#endif
        }

        /// <summary>
        /// Stop capturing audio.
        /// </summary>
        public void StopCapture()
        {
            if (!isCapturing) return;

#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLAudio_StopMicrophone();
            isCapturing = false;
            onAudioDataCallback = null;
            Debug.Log("WebGL Audio capture stopped");
#endif
        }

        /// <summary>
        /// Check if microphone is available in browser.
        /// </summary>
        public bool IsMicrophoneAvailable()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return WebGLAudio_IsMicrophoneAvailable() == 1;
#else
            return false;
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [MonoPInvokeCallback(typeof(Action<IntPtr, int>))]
        private static void OnAudioDataReceived(IntPtr dataPtr, int length)
        {
            if (onAudioDataCallback == null) return;

            byte[] audioData = new byte[length];
            Marshal.Copy(dataPtr, audioData, 0, length);
            
            onAudioDataCallback?.Invoke(audioData);
        }
#endif

        private void OnDestroy()
        {
            StopCapture();
        }

        public bool IsCapturing => isCapturing;
    }
}