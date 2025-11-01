using UnityEngine;

namespace Devdy.VoiceModulator
{
    /// <summary>
    /// Main game manager for Voice Modulator application.
    /// Initializes core systems and manages application lifecycle.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        #region Serialized Fields ==================================================================
        
        [Header("Manager References")]
        [SerializeField] private VoiceModulatorManager voiceModulatorManager; // Voice modulator system manager
        
        #endregion ==================================================================
        
        #region Unity Lifecycle ==================================================================
        
        protected override void Awake()
        {
            base.Awake();
            InitializeManagers();
        }
        
        private void Start()
        {
            // Pin all VoiceModulator options in SRDebugger
            SRDebug.Instance.PinAllOptions("VoiceModulator");
            Debug.Log("Voice Modulator initialized successfully.");
        }
        
        #endregion ==================================================================
        
        #region Initialization ==================================================================
        
        private void InitializeManagers()
        {
            // Ensure VoiceModulatorManager is initialized
            if (voiceModulatorManager == null)
            {
                voiceModulatorManager = VoiceModulatorManager.Instance;
            }
        }
        
        #endregion ==================================================================
        
        #region Application Lifecycle ==================================================================
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Stop recording when app is paused (important for mobile)
                if (voiceModulatorManager != null && voiceModulatorManager.IsRecording)
                {
                    voiceModulatorManager.StopRecording();
                    Debug.Log("Recording stopped due to application pause.");
                }
            }
        }
        
        private void OnApplicationQuit()
        {
            // Clean up audio resources
            if (voiceModulatorManager != null)
            {
                voiceModulatorManager.ClearAudio();
            }
        }
        
        #endregion ==================================================================
    }
}