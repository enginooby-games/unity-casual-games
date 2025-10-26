using UnityEngine;
using System.Collections.Generic;

namespace Devdy.VideoChat
{
    /// <summary>
    /// Manages microphone capture, audio streaming, and remote audio playback.
    /// Supports both native platforms (via Microphone API) and WebGL (via Web Audio API).
    /// </summary>
    public class AudioStreamManager : Singleton<AudioStreamManager>
    {
        #region Fields

        [SerializeField] private int sampleRate = 16000;
        [SerializeField] private int chunkSize = 1024;

#if !UNITY_WEBGL || UNITY_EDITOR
        private AudioClip microphoneClip;
        private string microphoneDevice;
        private int lastSamplePosition;
#else
        private WebGLAudioCapture webglAudioCapture;
#endif

        private bool isStreaming;

        // Remote audio sources
        private Dictionary<string, AudioSource> remoteAudioSources = new Dictionary<string, AudioSource>();
        private Dictionary<string, Queue<float[]>> remoteAudioBuffers = new Dictionary<string, Queue<float[]>>();

        #endregion ==================================================================

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            
#if UNITY_WEBGL && !UNITY_EDITOR
            // Create WebGL audio capture component
            GameObject webglAudioObj = new GameObject("WebGLAudioCapture");
            webglAudioObj.transform.SetParent(transform);
            webglAudioCapture = webglAudioObj.AddComponent<WebGLAudioCapture>();
            webglAudioCapture.Initialize(sampleRate, chunkSize);
#endif
        }

        private void Start()
        {
            NetworkManager.Instance.OnAudioChunkReceived += HandleRemoteAudioChunk;
            NetworkManager.Instance.OnUserLeft += HandleUserLeft;
        }

        private void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (isStreaming && microphoneClip != null)
            {
                CaptureAndSendAudio();
            }
#endif

            PlayRemoteAudio();
        }

        private void OnDestroy()
        {
            StopStreaming();

            foreach (var audioSource in remoteAudioSources.Values)
            {
                if (audioSource != null) Destroy(audioSource.gameObject);
            }
        }

        #endregion ==================================================================

        #region Local Microphone

        /// <summary>
        /// Starts microphone capture.
        /// </summary>
        public void StartMicrophone()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (microphoneClip != null) return;

            if (Microphone.devices.Length == 0)
            {
                Debug.LogError("No microphone devices found");
                return;
            }

            microphoneDevice = Microphone.devices[0];
            microphoneClip = Microphone.Start(microphoneDevice, true, 1, sampleRate);
            lastSamplePosition = 0;

            Debug.Log($"Microphone started: {microphoneDevice}");
#else
            if (webglAudioCapture == null)
            {
                Debug.LogError("WebGL Audio Capture not initialized");
                return;
            }

            if (!webglAudioCapture.IsMicrophoneAvailable())
            {
                Debug.LogError("Microphone not available in browser");
                return;
            }

            // WebGL uses callback-based capture
            Debug.Log("WebGL Microphone will start with streaming");
#endif
        }

        public void StopMicrophone()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (microphoneClip == null) return;

            Microphone.End(microphoneDevice);
            Destroy(microphoneClip);
            microphoneClip = null;
#else
            if (webglAudioCapture != null)
            {
                webglAudioCapture.StopCapture();
            }
#endif
        }

        public void StartStreaming()
        {
            if (!NetworkManager.Instance.IsConnected)
            {
                Debug.LogWarning("Cannot start audio streaming: not connected to server");
                return;
            }

#if !UNITY_WEBGL || UNITY_EDITOR
            if (microphoneClip == null) StartMicrophone();
            isStreaming = true;
            Debug.Log("Audio streaming started");
#else
            if (webglAudioCapture == null)
            {
                Debug.LogError("WebGL Audio Capture not initialized");
                return;
            }

            webglAudioCapture.StartCapture(OnWebGLAudioData);
            isStreaming = true;
            Debug.Log("WebGL Audio streaming started");
#endif
        }

        public void StopStreaming()
        {
            isStreaming = false;
            
#if UNITY_WEBGL && !UNITY_EDITOR
            if (webglAudioCapture != null)
            {
                webglAudioCapture.StopCapture();
            }
#endif
            
            Debug.Log("Audio streaming stopped");
        }

        #endregion ==================================================================

        #region Audio Capture & Encoding

#if !UNITY_WEBGL || UNITY_EDITOR
        private void CaptureAndSendAudio()
        {
            int currentPosition = Microphone.GetPosition(microphoneDevice);
            if (currentPosition < 0 || currentPosition == lastSamplePosition) return;

            int samplesAvailable = currentPosition - lastSamplePosition;
            if (samplesAvailable < 0)
            {
                samplesAvailable += microphoneClip.samples;
            }

            if (samplesAvailable < chunkSize) return;

            float[] samples = new float[chunkSize];
            microphoneClip.GetData(samples, lastSamplePosition);

            lastSamplePosition = (lastSamplePosition + chunkSize) % microphoneClip.samples;

            // Convert float samples to byte array (16-bit PCM)
            byte[] audioData = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short sample16 = (short)(samples[i] * short.MaxValue);
                audioData[i * 2] = (byte)(sample16 & 0xFF);
                audioData[i * 2 + 1] = (byte)((sample16 >> 8) & 0xFF);
            }

            NetworkManager.Instance.SendAudioChunk(audioData);
        }
#else
        private void OnWebGLAudioData(byte[] audioData)
        {
            if (!isStreaming) return;
            
            // WebGL plugin already provides 16-bit PCM data
            NetworkManager.Instance.SendAudioChunk(audioData);
        }
#endif

        #endregion ==================================================================

        #region Remote Audio

        private void HandleRemoteAudioChunk(string userId, byte[] audioData)
        {
            // Convert byte array back to float samples
            float[] samples = new float[audioData.Length / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short sample16 = (short)(audioData[i * 2] | (audioData[i * 2 + 1] << 8));
                samples[i] = sample16 / (float)short.MaxValue;
            }

            if (!remoteAudioBuffers.ContainsKey(userId))
            {
                remoteAudioBuffers[userId] = new Queue<float[]>();
                CreateRemoteAudioSource(userId);
            }

            remoteAudioBuffers[userId].Enqueue(samples);
        }

        private void CreateRemoteAudioSource(string userId)
        {
            GameObject audioObject = new GameObject($"RemoteAudio_{userId}");
            audioObject.transform.SetParent(transform);

            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.volume = 1f;

            AudioClip clip = AudioClip.Create($"RemoteClip_{userId}", sampleRate * 2, 1, sampleRate, false);
            audioSource.clip = clip;
            audioSource.Play();

            remoteAudioSources[userId] = audioSource;
        }

        private void PlayRemoteAudio()
        {
            foreach (var kvp in remoteAudioBuffers)
            {
                string userId = kvp.Key;
                Queue<float[]> buffer = kvp.Value;

                if (buffer.Count == 0) continue;
                if (!remoteAudioSources.TryGetValue(userId, out AudioSource audioSource)) continue;

                float[] samples = buffer.Dequeue();
                audioSource.clip.SetData(samples, 0);
            }
        }

        private void HandleUserLeft(string userId)
        {
            if (remoteAudioSources.TryGetValue(userId, out AudioSource audioSource))
            {
                Destroy(audioSource.gameObject);
                remoteAudioSources.Remove(userId);
            }

            remoteAudioBuffers.Remove(userId);
        }

        #endregion ==================================================================

        #region Properties

        public bool IsStreaming => isStreaming;
        
        public bool IsMicrophoneActive
        {
            get
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                return microphoneClip != null;
#else
                return webglAudioCapture != null && webglAudioCapture.IsCapturing;
#endif
            }
        }

        #endregion ==================================================================
    }
}