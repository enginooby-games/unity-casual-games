// ========== AudioVisualizer/UI/AudioUploadManager.cs ==========
/// <summary>
/// Manages audio file upload and playback controls.
/// Handles file loading, play/pause/stop functionality, and UI updates.
/// </summary>
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using AudioVisualizer.Core;
using TMPro;

namespace AudioVisualizer.UI
{
    public class AudioUploadManager : MonoBehaviour
    {
        /// <summary>Reference to the audio processor</summary>
        [SerializeField] private AudioProcessor audioProcessor;

        /// <summary>UI Text component for status messages</summary>
        [SerializeField] private TextMeshProUGUI statusText;

        /// <summary>Button to trigger file upload dialog</summary>
        [SerializeField] private Button uploadButton;

        /// <summary>Button to start audio playback</summary>
        [SerializeField] private Button playButton;

        /// <summary>Button to stop audio playback</summary>
        [SerializeField] private Button stopButton;

        /// <summary>Button to pause/resume audio playback</summary>
        [SerializeField] private Button pauseButton;

        /// <summary>Slider for playback progress control</summary>
        [SerializeField] private Slider progressSlider;

        /// <summary>Text showing current playback time</summary>
        [SerializeField] private TextMeshProUGUI timeText;

        /// <summary>Currently loaded audio clip</summary>
        private AudioClip currentClip;

        /// <summary>Whether audio is currently paused</summary>
        private bool isPaused = false;

        /// <summary>Maximum supported audio file size in MB</summary>
        private const int MAX_FILE_SIZE_MB = 100;

        private void Start()
        {
            InitializeUI();
            SubscribeToButtonEvents();
        }

        private void Update()
        {
            UpdatePlaybackProgress();
        }

        private void OnDestroy()
        {
            UnsubscribeFromButtonEvents();
        }

        /// <summary>
        /// Initializes UI components with default values.
        /// Sets initial status and button states.
        /// </summary>
        private void InitializeUI()
        {
            UpdateStatus("Ready for upload...");
            
            if (progressSlider != null)
            {
                progressSlider.value = 0f;
                progressSlider.interactable = false;
            }

            Debug.Log("[AudioUploadManager] UI initialized");
        }

        /// <summary>
        /// Subscribes all UI buttons to their click events.
        /// Removes default button behaviors.
        /// </summary>
        private void SubscribeToButtonEvents()
        {
            if (uploadButton != null)
            {
                uploadButton.onClick.AddListener(OnUploadClicked);
            }

            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
            }

            if (stopButton != null)
            {
                stopButton.onClick.AddListener(OnStopClicked);
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseClicked);
            }

            if (progressSlider != null)
            {
                progressSlider.onValueChanged.AddListener(OnProgressSliderChanged);
            }
        }

        /// <summary>
        /// Unsubscribes all UI buttons from their click events.
        /// Prevents memory leaks from dangling references.
        /// </summary>
        private void UnsubscribeFromButtonEvents()
        {
            if (uploadButton != null)
                uploadButton.onClick.RemoveListener(OnUploadClicked);

            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayClicked);

            if (stopButton != null)
                stopButton.onClick.RemoveListener(OnStopClicked);

            if (pauseButton != null)
                pauseButton.onClick.RemoveListener(OnPauseClicked);

            if (progressSlider != null)
                progressSlider.onValueChanged.RemoveListener(OnProgressSliderChanged);
        }

        /// <summary>
        /// Handles upload button click event.
        /// Opens file dialog and loads selected audio file.
        /// </summary>
        private void OnUploadClicked()
        {
            #if UNITY_EDITOR
            string filePath = UnityEditor.EditorUtility.OpenFilePanel(
                "Load Audio File", "", "wav,mp3,ogg");
            
            if (!string.IsNullOrEmpty(filePath))
            {
                LoadAudioFile(filePath);
            }
            #elif UNITY_STANDALONE_WIN
            string filePath = OpenWindowsFileDialog();
            if (!string.IsNullOrEmpty(filePath))
            {
                LoadAudioFile(filePath);
            }
            #else
            UpdateStatus("Upload not supported on this platform");
            #endif
        }

        /// <summary>
        /// Opens Windows file dialog for audio file selection.
        /// Only available on Windows platform.
        /// </summary>
        /// <returns>Selected file path or empty string if cancelled</returns>
        private string OpenWindowsFileDialog()
        {
            #if UNITY_STANDALONE_WIN
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Audio Files (*.wav;*.mp3;*.ogg)|*.wav;*.mp3;*.ogg|All Files (*.*)|*.*";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return dialog.FileName;
            }
            #endif
            return null;
        }

        /// <summary>
        /// Validates and loads an audio file from disk.
        /// Checks file existence, format, and size before loading.
        /// </summary>
        /// <param name="filePath">Full path to audio file</param>
        private void LoadAudioFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                UpdateStatus("Invalid file path");
                return;
            }

            if (!File.Exists(filePath))
            {
                UpdateStatus("File not found!");
                return;
            }

            if (!IsValidAudioFile(filePath))
            {
                UpdateStatus("Unsupported format! Use WAV, MP3, or OGG");
                return;
            }

            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > MAX_FILE_SIZE_MB * 1024 * 1024)
            {
                UpdateStatus($"File too large! Maximum {MAX_FILE_SIZE_MB}MB");
                return;
            }

            StartCoroutine(LoadAudioCoroutine(filePath));
        }

        /// <summary>
        /// Validates audio file by checking extension.
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <returns>True if file is supported audio format</returns>
        private bool IsValidAudioFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension == ".wav" || extension == ".mp3" || extension == ".ogg";
        }

        /// <summary>
        /// Loads audio file asynchronously using WWW.
        /// Handles audio loading and error checking.
        /// </summary>
        /// <param name="filePath">Full path to audio file</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator LoadAudioCoroutine(string filePath)
        {
            UpdateStatus("Loading...");

            string fileUrl = $"file:///{filePath}";
            AudioType audioType = GetAudioTypeFromPath(filePath);

            #pragma warning disable CS0618
            using (WWW www = new WWW(fileUrl))
            {
                yield return www;

                if (!string.IsNullOrEmpty(www.error))
                {
                    UpdateStatus($"Load error: {www.error}");
                    yield break;
                }

                string fileName = Path.GetFileNameWithoutExtension(filePath);
                currentClip = www.GetAudioClip(false, false, audioType);
                currentClip.name = fileName;

                audioProcessor?.SetAudioClip(currentClip);
                progressSlider.interactable = true;
                progressSlider.maxValue = currentClip.length;
                UpdateStatus($"Loaded: {fileName} ({currentClip.length:F1}s)");
                isPaused = false;

                #pragma warning restore CS0618
            }
        }

        /// <summary>
        /// Determines AudioType from file extension.
        /// </summary>
        /// <param name="filePath">Path to audio file</param>
        /// <returns>Appropriate AudioType for the file</returns>
        private AudioType GetAudioTypeFromPath(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".wav" => AudioType.WAV,
                ".mp3" => AudioType.MPEG,
                ".ogg" => AudioType.OGGVORBIS,
                _ => AudioType.UNKNOWN
            };
        }

        /// <summary>
        /// Handles play button click event.
        /// Starts audio playback from current position.
        /// </summary>
        private void OnPlayClicked()
        {
            if (currentClip == null)
            {
                UpdateStatus("No audio loaded!");
                return;
            }

            if (isPaused)
            {
                audioProcessor?.Resume();
                isPaused = false;
            }
            else
            {
                audioProcessor?.Play();
            }

            UpdateStatus("Playing...");
        }

        /// <summary>
        /// Handles stop button click event.
        /// Stops audio and resets playback position.
        /// </summary>
        private void OnStopClicked()
        {
            audioProcessor?.Stop();
            isPaused = false;
            progressSlider.value = 0f;
            UpdateStatus("Stopped");
        }

        /// <summary>
        /// Handles pause button click event.
        /// Pauses or resumes audio playback.
        /// </summary>
        private void OnPauseClicked()
        {
            if (isPaused)
            {
                audioProcessor?.Resume();
                UpdateStatus("Resumed");
                isPaused = false;
            }
            else
            {
                audioProcessor?.Pause();
                UpdateStatus("Paused");
                isPaused = true;
            }
        }

        /// <summary>
        /// Handles progress slider value change.
        /// Allows seeking to different points in audio.
        /// </summary>
        /// <param name="value">New slider value (time in seconds)</param>
        private void OnProgressSliderChanged(float value)
        {
            if (audioProcessor != null && audioProcessor.IsPlaying)
            {
                AudioSource audioSource = audioProcessor.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.time = value;
                }
            }
        }

        /// <summary>
        /// Updates progress slider and time display each frame.
        /// Shows current playback position.
        /// </summary>
        private void UpdatePlaybackProgress()
        {
            if (audioProcessor == null || currentClip == null)
                return;

            AudioSource audioSource = audioProcessor.GetComponent<AudioSource>();
            if (audioSource == null)
                return;

            // Update slider without triggering event
            progressSlider.SetValueWithoutNotify(audioSource.time);

            // Update time display
            if (timeText != null)
            {
                string currentTime = FormatTime(audioSource.time);
                string totalTime = FormatTime(currentClip.length);
                timeText.text = $"{currentTime} / {totalTime}";
            }
        }

        /// <summary>
        /// Formats time value into MM:SS format.
        /// </summary>
        /// <param name="time">Time in seconds</param>
        /// <returns>Formatted time string</returns>
        private string FormatTime(float time)
        {
            int minutes = (int)(time / 60f);
            int seconds = (int)(time % 60f);
            return $"{minutes:D2}:{seconds:D2}";
        }

        /// <summary>
        /// Updates status text display and logs message.
        /// </summary>
        /// <param name="message">Status message to display</param>
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            Debug.Log($"[AudioUploadManager] {message}");
        }
    }
}