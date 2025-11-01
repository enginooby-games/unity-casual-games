using UnityEngine;
using System.Collections.Generic;
using Devdy.AudioProcessing;

namespace Devdy.VoiceModulator
{
    /// <summary>
    /// Manages voice modulation features including pitch shifting, reverb effects, and real-time audio processing.
    /// Handles recording, playback, real-time monitoring, and audio file import/export operations.
    /// </summary>
    public class VoiceModulatorManager : Singleton<VoiceModulatorManager>
    {
        #region Serialized Fields ==================================================================
        
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource; // Main audio output source
        [SerializeField] private int sampleRate = 44100; // Audio sample rate
        [SerializeField] private int maxRecordingLength = 300; // Maximum recording length in seconds
        
        #endregion ==================================================================
        
        #region Private Fields ==================================================================
        
        private AudioClip recordedClip; // Current recorded audio clip
        private AudioClip processedClip; // Audio clip with effects applied
        private AudioClip realtimeClip; // Looping clip for real-time monitoring
        private string microphoneDevice; // Selected microphone device
        private bool isRecording; // Recording state flag
        private bool isRealtimeMonitorEnabled; // Real-time monitoring state
        private List<float> recordedSamples; // Accumulated recorded samples
        
        // Effect parameters
        private float pitchShift; // Pitch shift in semitones
        private float reverbRoomSize; // Reverb room size (0-1)
        private float reverbMix; // Reverb wet/dry mix (0-1)
        private float inputGain; // Input audio gain multiplier
        
        // Real-time processing
        private int realtimeReadPosition; // Track position in microphone clip
        private float[] realtimeProcessBuffer; // Buffer for processing effects
        
        private const int RECORDING_FREQUENCY = 44100;
        private const int REALTIME_LATENCY_SAMPLES = 2048; // ~46ms latency at 44.1kHz
        
        #endregion ==================================================================
        
        #region Properties ==================================================================
        
        public bool IsRecording => isRecording;
        public bool IsRealtimeMonitorEnabled => isRealtimeMonitorEnabled;
        public bool HasRecording => recordedClip != null;
        public AudioClip ProcessedClip => processedClip;
        public float PitchShift => pitchShift;
        public float ReverbRoomSize => reverbRoomSize;
        public float ReverbMix => reverbMix;
        public float InputGain => inputGain;
        
        #endregion ==================================================================
        
        #region Unity Lifecycle ==================================================================
        
        protected override void Awake()
        {
            base.Awake();
            InitializeAudioSettings();
        }
        
        private void Start()
        {
            LoadParametersFromSROptions();
        }
        
        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!isRecording) return;
            
            // Store recorded samples for post-processing
            if (recordedSamples == null)
                recordedSamples = new List<float>();
            
            // Clone original data before processing
            float[] originalData = new float[data.Length];
            AudioProcessor.ApplyNoiseGate(data, 0.05f, 0.001f, 0.05f, 44100);
            System.Array.Copy(data, originalData, data.Length);
            
            // Process and store samples
            for (int i = 0; i < originalData.Length; i++)
            {
                // Apply input gain to original
                float sample = originalData[i] * inputGain;
                recordedSamples.Add(sample);
            }
            
            // Apply real-time effects if monitoring is enabled
            if (isRealtimeMonitorEnabled)
            {
                // Process the output data with effects
                ApplyRealtimeEffects(data, channels);
            }
        }
        
        #endregion ==================================================================
        
        #region Initialization ==================================================================
        
        private void InitializeAudioSettings()
        {
            // Setup main audio source
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            
            // Initialize realtime processing buffer
            realtimeProcessBuffer = new float[REALTIME_LATENCY_SAMPLES];
            
            // Get available microphone devices
            if (Microphone.devices.Length > 0)
            {
                microphoneDevice = Microphone.devices[0];
                Debug.Log($"Selected microphone: {microphoneDevice}");
            }
            else
            {
                Debug.LogWarning("No microphone devices found!");
            }
        }
        
        private void LoadParametersFromSROptions()
        {
            pitchShift = SROptions.Current.VoiceModulator_PitchShift;
            reverbRoomSize = SROptions.Current.VoiceModulator_ReverbRoomSize;
            reverbMix = SROptions.Current.VoiceModulator_ReverbMix;
            inputGain = SROptions.Current.VoiceModulator_InputGain;
        }
        
        #endregion ==================================================================
        
        #region Recording Controls ==================================================================
        
        /// <summary>
        /// Starts recording audio from the microphone with optional real-time monitoring.
        /// Real-time monitoring plays back your voice with effects as you speak.
        /// </summary>
        /// <param name="enableRealtimeMonitor">If true, hear effects while recording</param>
        public void StartRecording(bool enableRealtimeMonitor = false)
        {
            if (isRecording) return;
            if (string.IsNullOrEmpty(microphoneDevice))
            {
                Debug.LogError("No microphone device available!");
                return;
            }
            
            recordedSamples = new List<float>();
            isRealtimeMonitorEnabled = enableRealtimeMonitor;
            
            // Start microphone with looping for real-time playback
            recordedClip = Microphone.Start(microphoneDevice, true, 1, RECORDING_FREQUENCY);
            isRecording = true;
            realtimeReadPosition = 0;
            
            // If real-time monitoring enabled, setup playback
            if (isRealtimeMonitorEnabled)
            {
                // Wait for microphone to start
                StartCoroutine(StartRealtimeMonitoring());
            }
            
            Debug.Log($"Recording started (Real-time monitor: {enableRealtimeMonitor})");
        }
        
        /// <summary>
        /// Waits for microphone to initialize then starts real-time playback.
        /// </summary>
        private System.Collections.IEnumerator StartRealtimeMonitoring()
        {
            // Wait for microphone to have data
            while (Microphone.GetPosition(microphoneDevice) <= 0)
            {
                yield return null;
            }
            
            // Setup audio source to play the microphone clip with looping
            audioSource.clip = recordedClip;
            audioSource.loop = true;
            audioSource.Play();
            
            Debug.Log("Real-time monitoring started");
        }
        
        /// <summary>
        /// Stops recording and processes the recorded audio.
        /// </summary>
        public void StopRecording()
        {
            if (!isRecording) return;
            
            // Stop microphone
            int micPosition = Microphone.GetPosition(microphoneDevice);
            Microphone.End(microphoneDevice);
            
            // Stop real-time playback
            if (audioSource.isPlaying && isRealtimeMonitorEnabled)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }
            
            isRecording = false;
            isRealtimeMonitorEnabled = false;
            
            // Create audio clip from recorded samples
            if (recordedSamples != null && recordedSamples.Count > 0)
            {
                recordedClip = AudioClip.Create("RecordedAudio", recordedSamples.Count, 1, RECORDING_FREQUENCY, false);
                recordedClip.SetData(recordedSamples.ToArray(), 0);
                
                // Apply post-recording effects
                ProcessRecordedAudio();
            }
            
            Debug.Log("Recording stopped.");
        }
        
        /// <summary>
        /// Toggles real-time monitoring during recording.
        /// </summary>
        public void ToggleRealtimeMonitor()
        {
            if (!isRecording)
            {
                Debug.LogWarning("Cannot toggle monitor - not recording!");
                return;
            }
            
            isRealtimeMonitorEnabled = !isRealtimeMonitorEnabled;
            
            if (isRealtimeMonitorEnabled)
            {
                // Start playback
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = recordedClip;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            else
            {
                // Stop playback
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                    audioSource.loop = false;
                }
            }
            
            Debug.Log($"Real-time monitor: {(isRealtimeMonitorEnabled ? "Enabled" : "Disabled")}");
        }
        
        #endregion ==================================================================
        
        #region Playback Controls ==================================================================
        
        /// <summary>
        /// Plays the processed audio with applied effects.
        /// </summary>
        public void PlayProcessedAudio()
        {
            if (processedClip == null)
            {
                Debug.LogWarning("No processed audio available to play!");
                return;
            }
            
            // Make sure we're not in real-time monitoring mode
            if (isRealtimeMonitorEnabled)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }
            
            audioSource.clip = processedClip;
            audioSource.loop = false;
            audioSource.Play();
        }
        
        /// <summary>
        /// Stops audio playback.
        /// </summary>
        public void StopPlayback()
        {
            if (audioSource.isPlaying && !isRealtimeMonitorEnabled)
            {
                audioSource.Stop();
            }
        }
        
        #endregion ==================================================================
        
        #region Audio Processing ==================================================================
        
        /// <summary>
        /// Applies real-time effects during recording for monitoring.
        /// Uses efficient processing for low-latency feedback.
        /// </summary>
        private void ApplyRealtimeEffects(float[] data, int channels)
        {
            // Apply gain
            AudioProcessor.ApplyGain(data, inputGain);
            
            // Apply reverb effect in real-time
            AudioProcessor.ApplyReverb(data, sampleRate, reverbRoomSize, reverbMix);
            
            // Optional: Apply pitch shift if not too CPU-intensive for target platform
            // For mobile/WebGL, comment out the line below to avoid performance issues
            if (!Mathf.Approximately(pitchShift, 0f))
            {
                // Warning: This may cause audio dropouts on mobile/WebGL
                // Only enable if target hardware is powerful enough
                float[] pitched = AudioProcessor.ApplyPitchShift(data, pitchShift);
                if (pitched.Length <= data.Length)
                {
                    System.Array.Copy(pitched, data, pitched.Length);
                    // Fill remaining with silence if pitched is shorter
                    for (int i = pitched.Length; i < data.Length; i++)
                    {
                        data[i] = 0f;
                    }
                }
                else
                {
                    // If pitched is longer, truncate to fit
                    System.Array.Copy(pitched, data, data.Length);
                }
            }
        }
        
        /// <summary>
        /// Processes recorded audio with all effects (pitch shift, reverb, normalization).
        /// Creates a new processed clip ready for playback.
        /// </summary>
        private void ProcessRecordedAudio()
        {
            if (recordedClip == null) return;
            
            float[] samples = new float[recordedClip.samples * recordedClip.channels];
            recordedClip.GetData(samples, 0);
            
            // Apply pitch shift
            float[] pitchShiftedSamples = AudioProcessor.ApplyPitchShift(samples, pitchShift);
            
            // Apply reverb
            AudioProcessor.ApplyReverb(pitchShiftedSamples, recordedClip.frequency, reverbRoomSize, reverbMix);
            
            // Normalize to prevent clipping
            AudioProcessor.Normalize(pitchShiftedSamples, 0.95f);
            
            // Create processed clip
            processedClip = AudioClip.Create("ProcessedAudio", pitchShiftedSamples.Length / recordedClip.channels, 
                                            recordedClip.channels, recordedClip.frequency, false);
            processedClip.SetData(pitchShiftedSamples, 0);
            
            Debug.Log("Audio processing completed.");
        }
        
        #endregion ==================================================================
        
        #region Effect Presets ==================================================================
        
        /// <summary>
        /// Applies a preset effect configuration.
        /// </summary>
        public void ApplyPreset(VoicePreset preset)
        {
            switch (preset)
            {
                case VoicePreset.Robot:
                    SetEffectParameters(5f, 0.1f, 0.3f, 1.2f);
                    break;
                case VoicePreset.Monster:
                    SetEffectParameters(-8f, 0.8f, 0.6f, 1.5f);
                    break;
                case VoicePreset.Chipmunk:
                    SetEffectParameters(12f, 0.2f, 0.1f, 1.0f);
                    break;
                case VoicePreset.Echo:
                    SetEffectParameters(0f, 0.9f, 0.8f, 1.0f);
                    break;
                case VoicePreset.DeepVoice:
                    SetEffectParameters(-5f, 0.4f, 0.3f, 1.3f);
                    break;
                case VoicePreset.HighPitch:
                    SetEffectParameters(7f, 0.3f, 0.2f, 1.0f);
                    break;
                case VoicePreset.Normal:
                default:
                    SetEffectParameters(0f, 0.3f, 0.3f, 1.0f);
                    break;
            }
            
            // Update SROptions to reflect preset values
            UpdateSROptionsFromPreset();
            
            // Reprocess audio if available
            if (HasRecording)
            {
                ProcessRecordedAudio();
            }
        }
        
        private void SetEffectParameters(float pitch, float roomSize, float mix, float gain)
        {
            pitchShift = pitch;
            reverbRoomSize = roomSize;
            reverbMix = mix;
            inputGain = gain;
        }
        
        private void UpdateSROptionsFromPreset()
        {
            SROptions.Current.VoiceModulator_PitchShift = pitchShift;
            SROptions.Current.VoiceModulator_ReverbRoomSize = reverbRoomSize;
            SROptions.Current.VoiceModulator_ReverbMix = reverbMix;
            SROptions.Current.VoiceModulator_InputGain = inputGain;
        }
        
        #endregion ==================================================================
        
        #region Parameter Updates ==================================================================
        
        /// <summary>
        /// Updates effect parameters and reprocesses audio.
        /// </summary>
        public void UpdateEffectParameters(float pitch, float roomSize, float mix, float gain)
        {
            pitchShift = pitch;
            reverbRoomSize = roomSize;
            reverbMix = mix;
            inputGain = gain;
            
            if (HasRecording)
            {
                ProcessRecordedAudio();
            }
        }
        
        #endregion ==================================================================
        
        #region Import/Export ==================================================================
        
        /// <summary>
        /// Imports an audio file and sets it as the recorded clip.
        /// </summary>
        public void ImportAudioFile(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogError("Cannot import null audio clip!");
                return;
            }
            
            recordedClip = clip;
            recordedSamples = new List<float>();
            
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            recordedSamples.AddRange(samples);
            
            ProcessRecordedAudio();
            Debug.Log($"Audio file imported: {clip.name}");
        }
        
        /// <summary>
        /// Exports the processed audio clip (returns the clip for external save handling).
        /// </summary>
        public AudioClip ExportProcessedAudio()
        {
            if (processedClip == null)
            {
                Debug.LogWarning("No processed audio to export!");
                return null;
            }
            
            Debug.Log("Processed audio ready for export.");
            return processedClip;
        }
        
        #endregion ==================================================================
        
        #region Utility ==================================================================
        
        /// <summary>
        /// Clears all recorded and processed audio data.
        /// </summary>
        public void ClearAudio()
        {
            StopPlayback();
            
            if (isRecording)
            {
                StopRecording();
            }
            
            recordedClip = null;
            processedClip = null;
            recordedSamples = null;
            
            Debug.Log("Audio data cleared.");
        }
        
        #endregion ==================================================================
    }
    
    /// <summary>
    /// Preset voice effect configurations.
    /// </summary>
    public enum VoicePreset
    {
        Normal,
        Robot,
        Monster,
        Chipmunk,
        Echo,
        DeepVoice,
        HighPitch
    }
}
