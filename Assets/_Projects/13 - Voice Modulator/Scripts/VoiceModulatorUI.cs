using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Devdy.VoiceModulator
{
    /// <summary>
    /// Manages the UI for the Voice Modulator including recording controls, real-time monitoring toggle,
    /// effect presets, and parameter displays.
    /// </summary>
    public class VoiceModulatorUI : MonoBehaviour
    {
        #region Serialized Fields ==================================================================
        
        [Header("Control Buttons")]
        [SerializeField] private Button recordButton; // Start/Stop recording button
        [SerializeField] private Button playButton; // Play processed audio button
        [SerializeField] private Button stopButton; // Stop playback button
        [SerializeField] private Button clearButton; // Clear audio data button
        [SerializeField] private Button importButton; // Import audio file button
        [SerializeField] private Button exportButton; // Export processed audio button
        [SerializeField] private Toggle realtimeMonitorToggle; // Toggle for real-time monitoring
        
        [Header("Button Text References")]
        [SerializeField] private TextMeshProUGUI recordButtonText; // Text component of record button
        [SerializeField] private TextMeshProUGUI playButtonText; // Text component of play button
        
        [Header("Preset Buttons")]
        [SerializeField] private Button normalPresetButton;
        [SerializeField] private Button robotPresetButton;
        [SerializeField] private Button monsterPresetButton;
        [SerializeField] private Button chipmunkPresetButton;
        [SerializeField] private Button echoPresetButton;
        [SerializeField] private Button deepVoicePresetButton;
        [SerializeField] private Button highPitchPresetButton;
        
        [Header("Status Display")]
        [SerializeField] private TextMeshProUGUI statusText; // Display recording/playback status
        [SerializeField] private Image recordingIndicator; // Visual indicator for recording state
        [SerializeField] private Image monitorIndicator; // Visual indicator for real-time monitoring
        [SerializeField] private Color recordingColor = Color.red;
        [SerializeField] private Color monitoringColor = Color.green;
        [SerializeField] private Color idleColor = Color.gray;
        
        [Header("Parameter Display")]
        [SerializeField] private TextMeshProUGUI pitchValueText; // Display current pitch value
        [SerializeField] private TextMeshProUGUI reverbRoomSizeText; // Display reverb room size
        [SerializeField] private TextMeshProUGUI reverbMixText; // Display reverb mix level
        [SerializeField] private TextMeshProUGUI inputGainText; // Display input gain value
        
        #endregion ==================================================================
        
        #region Private Fields ==================================================================
        
        private VoiceModulatorManager manager; // Reference to voice modulator manager
        private bool isRecording; // Local recording state
        
        private const string RECORD_TEXT = "Record";
        private const string STOP_RECORDING_TEXT = "Stop Recording";
        private const string PLAY_TEXT = "Play";
        private const string PLAYING_TEXT = "Playing...";
        
        #endregion ==================================================================
        
        #region Unity Lifecycle ==================================================================
        
        private void Start()
        {
            manager = VoiceModulatorManager.Instance;
            InitializeUI();
            SetupButtonListeners();
            UpdateUI();
        }
        
        private void Update()
        {
            UpdateRealTimeDisplay();
        }
        
        #endregion ==================================================================
        
        #region Initialization ==================================================================
        
        private void InitializeUI()
        {
            if (recordButtonText != null)
                recordButtonText.text = RECORD_TEXT;
            
            if (playButtonText != null)
                playButtonText.text = PLAY_TEXT;
            
            if (recordingIndicator != null)
                recordingIndicator.color = idleColor;
            
            if (monitorIndicator != null)
                monitorIndicator.color = idleColor;
            
            if (realtimeMonitorToggle != null)
            {
                realtimeMonitorToggle.isOn = false;
                realtimeMonitorToggle.interactable = false; // Only enable during recording
            }
            
            UpdateStatusText("Ready - Enable real-time monitor to hear effects while recording");
            UpdateParameterDisplays();
        }
        
        private void SetupButtonListeners()
        {
            if (recordButton != null)
                recordButton.onClick.AddListener(OnRecordButtonClicked);
            
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonClicked);
            
            if (stopButton != null)
                stopButton.onClick.AddListener(OnStopButtonClicked);
            
            if (clearButton != null)
                clearButton.onClick.AddListener(OnClearButtonClicked);
            
            if (importButton != null)
                importButton.onClick.AddListener(OnImportButtonClicked);
            
            if (exportButton != null)
                exportButton.onClick.AddListener(OnExportButtonClicked);
            
            if (realtimeMonitorToggle != null)
                realtimeMonitorToggle.onValueChanged.AddListener(OnRealtimeMonitorToggled);
            
            // Setup preset buttons
            SetupPresetButton(normalPresetButton, VoicePreset.Normal);
            SetupPresetButton(robotPresetButton, VoicePreset.Robot);
            SetupPresetButton(monsterPresetButton, VoicePreset.Monster);
            SetupPresetButton(chipmunkPresetButton, VoicePreset.Chipmunk);
            SetupPresetButton(echoPresetButton, VoicePreset.Echo);
            SetupPresetButton(deepVoicePresetButton, VoicePreset.DeepVoice);
            SetupPresetButton(highPitchPresetButton, VoicePreset.HighPitch);
        }
        
        private void SetupPresetButton(Button button, VoicePreset preset)
        {
            if (button == null) return;
            button.onClick.AddListener(() => OnPresetButtonClicked(preset));
        }
        
        #endregion ==================================================================
        
        #region Button Callbacks ==================================================================
        
        private void OnRecordButtonClicked()
        {
            if (isRecording)
            {
                manager.StopRecording();
                isRecording = false;
                
                if (recordButtonText != null)
                    recordButtonText.text = RECORD_TEXT;
                
                if (recordingIndicator != null)
                    recordingIndicator.color = idleColor;
                
                if (monitorIndicator != null)
                    monitorIndicator.color = idleColor;
                
                if (realtimeMonitorToggle != null)
                {
                    realtimeMonitorToggle.interactable = false;
                    realtimeMonitorToggle.isOn = false;
                }
                
                UpdateStatusText("Recording stopped. Processing audio...");
            }
            else
            {
                bool enableMonitor = realtimeMonitorToggle != null && realtimeMonitorToggle.isOn;
                manager.StartRecording(enableMonitor);
                isRecording = true;
                
                if (recordButtonText != null)
                    recordButtonText.text = STOP_RECORDING_TEXT;
                
                if (recordingIndicator != null)
                    recordingIndicator.color = recordingColor;
                
                if (realtimeMonitorToggle != null)
                    realtimeMonitorToggle.interactable = true;
                
                UpdateMonitorIndicator();
                UpdateStatusText(enableMonitor ? "Recording with real-time monitoring..." : "Recording...");
            }
            
            UpdateUI();
        }
        
        private void OnRealtimeMonitorToggled(bool enabled)
        {
            if (!isRecording) return;
            
            manager.ToggleRealtimeMonitor();
            UpdateMonitorIndicator();
            UpdateStatusText(enabled ? "Real-time monitoring enabled" : "Real-time monitoring disabled");
        }
        
        private void UpdateMonitorIndicator()
        {
            if (monitorIndicator == null) return;
            
            if (isRecording && manager.IsRealtimeMonitorEnabled)
            {
                monitorIndicator.color = monitoringColor;
            }
            else
            {
                monitorIndicator.color = idleColor;
            }
        }
        
        private void OnPlayButtonClicked()
        {
            if (!manager.HasRecording)
            {
                UpdateStatusText("No audio to play!");
                return;
            }
            
            manager.PlayProcessedAudio();
            
            if (playButtonText != null)
                playButtonText.text = PLAYING_TEXT;
            
            UpdateStatusText("Playing processed audio...");
        }
        
        private void OnStopButtonClicked()
        {
            manager.StopPlayback();
            
            if (playButtonText != null)
                playButtonText.text = PLAY_TEXT;
            
            UpdateStatusText("Playback stopped.");
        }
        
        private void OnClearButtonClicked()
        {
            manager.ClearAudio();
            isRecording = false;
            
            if (recordButtonText != null)
                recordButtonText.text = RECORD_TEXT;
            
            if (recordingIndicator != null)
                recordingIndicator.color = idleColor;
            
            if (monitorIndicator != null)
                monitorIndicator.color = idleColor;
            
            if (realtimeMonitorToggle != null)
            {
                realtimeMonitorToggle.isOn = false;
                realtimeMonitorToggle.interactable = false;
            }
            
            UpdateStatusText("Audio data cleared.");
            UpdateUI();
        }
        
        private void OnImportButtonClicked()
        {
            // Note: Actual file import implementation depends on platform
            UpdateStatusText("Import feature requires platform-specific implementation.");
            Debug.Log("Import audio file - implement platform-specific file picker");
        }
        
        private void OnExportButtonClicked()
        {
            AudioClip exportClip = manager.ExportProcessedAudio();
            
            if (exportClip == null)
            {
                UpdateStatusText("No audio to export!");
                return;
            }
            
            // Note: Actual file export implementation depends on platform
            UpdateStatusText("Export feature requires platform-specific implementation.");
            Debug.Log("Export processed audio - implement WAV conversion and save");
        }
        
        private void OnPresetButtonClicked(VoicePreset preset)
        {
            manager.ApplyPreset(preset);
            UpdateStatusText($"Applied preset: {preset}");
            UpdateParameterDisplays();
        }
        
        #endregion ==================================================================
        
        #region UI Updates ==================================================================
        
        private void UpdateUI()
        {
            if (playButton != null)
                playButton.interactable = manager.HasRecording;
            
            if (exportButton != null)
                exportButton.interactable = manager.HasRecording;
            
            // Disable preset buttons while recording
            bool enablePresets = !isRecording;
            if (normalPresetButton != null) normalPresetButton.interactable = enablePresets;
            if (robotPresetButton != null) robotPresetButton.interactable = enablePresets;
            if (monsterPresetButton != null) monsterPresetButton.interactable = enablePresets;
            if (chipmunkPresetButton != null) chipmunkPresetButton.interactable = enablePresets;
            if (echoPresetButton != null) echoPresetButton.interactable = enablePresets;
            if (deepVoicePresetButton != null) deepVoicePresetButton.interactable = enablePresets;
            if (highPitchPresetButton != null) highPitchPresetButton.interactable = enablePresets;
        }
        
        private void UpdateRealTimeDisplay()
        {
            // Reset play button text when audio finishes playing
            if (playButton != null && playButtonText != null)
            {
                AudioSource audioSource = manager.GetComponent<AudioSource>();
                if (audioSource != null && !audioSource.isPlaying && playButtonText.text == PLAYING_TEXT)
                {
                    playButtonText.text = PLAY_TEXT;
                    UpdateStatusText("Playback finished.");
                }
            }
            
            // Update parameter displays from SROptions
            UpdateParameterDisplays();
            
            // Update monitor indicator state
            UpdateMonitorIndicator();
        }
        
        private void UpdateParameterDisplays()
        {
            if (pitchValueText != null)
                pitchValueText.text = $"Pitch: {manager.PitchShift:F1} semitones";
            
            if (reverbRoomSizeText != null)
                reverbRoomSizeText.text = $"Room Size: {manager.ReverbRoomSize:F2}";
            
            if (reverbMixText != null)
                reverbMixText.text = $"Reverb Mix: {manager.ReverbMix:F2}";
            
            if (inputGainText != null)
                inputGainText.text = $"Input Gain: {manager.InputGain:F2}x";
        }
        
        private void UpdateStatusText(string message)
        {
            if (statusText != null)
                statusText.text = message;
            
            Debug.Log($"[VoiceModulator] {message}");
        }
        
        #endregion ==================================================================
    }
}