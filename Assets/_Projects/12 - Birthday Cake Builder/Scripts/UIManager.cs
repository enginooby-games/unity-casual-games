using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Devdy.BirthdayCake
{
    /// <summary>
    /// Manages all UI elements including game UI, victory, and fail screens.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Game UI")]
        [SerializeField] private GameObject gameUI;
        [SerializeField] private TextMeshProUGUI layerCountText; // Shows "Layer 3/5"

        [Header("Victory Screen")]
        [SerializeField] private GameObject victoryScreen;
        [SerializeField] private TextMeshProUGUI victoryText; // "🎉 Happy Birthday! 🎉"
        [SerializeField] private GameObject failScreen;
        [SerializeField] private Button screenshotButton;
        [SerializeField] private Button restartButton;

        [Header("Fail Screen")]
        [SerializeField] private TextMeshProUGUI failText;
        [SerializeField] private Button retryButton;
        [Header("Screenshot Settings")]
        [SerializeField] private bool hideUIInScreenshot = true; // Hide UI when taking screenshot
        private GameObject debugger; // Hide UI when taking screenshot
        
        private const string BIRTHDAY_MESSAGE = "🎉 Happy Birthday! 🎉";
        private const string FAIL_MESSAGE = "Oops! The cake collapsed!";

        #region ==================================================================== Initialization

        private void Start()
        {
            SetupButtons();
            HideAllScreens();
        }

        private void SetupButtons()
        {
            if (screenshotButton != null)
                screenshotButton.onClick.AddListener(TakeScreenshot);

            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);

            if (retryButton != null)
                retryButton.onClick.AddListener(RestartGame);
        }

        #endregion ==================================================================

        #region ==================================================================== Screen Management

        public void ShowGameUI()
        {
            HideAllScreens();
            if (gameUI != null) gameUI.SetActive(true);
            UpdateLayerCount(0);
        }

        public void ShowVictoryScreen()
        {
            HideAllScreens();
            if (victoryScreen != null)
            {
                victoryScreen.SetActive(true);
                if (victoryText != null)
                    victoryText.text = BIRTHDAY_MESSAGE;
            }
        }

        public void ShowFailScreen()
        {
            HideAllScreens();
            if (failScreen != null)
            {
                failScreen.SetActive(true);
                if (failText != null)
                    failText.text = FAIL_MESSAGE;
            }
        }

        private void HideAllScreens()
        {
            if (gameUI != null) gameUI.SetActive(false);
            if (victoryScreen != null) victoryScreen.SetActive(false);
            if (failScreen != null) failScreen.SetActive(false);
        }

        #endregion ==================================================================

        #region ==================================================================== UI Updates

        /// <summary>
        /// Updates the layer progress text during gameplay.
        /// </summary>
        public void UpdateLayerCount(int currentLayer)
        {
            if (layerCountText == null) return;

            int totalLayers = GameManager.Instance.Config.TotalLayers;
            layerCountText.text = $"{currentLayer}/{totalLayers}";
        }

        #endregion ==================================================================

        #region ==================================================================== Button Actions

        private void TakeScreenshot()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL: Use canvas download method
            StartCoroutine(CaptureScreenshotWebGL());
#else
            // Standalone/Mobile: Use ScreenCapture
            StartCoroutine(CaptureScreenshotStandalone());
#endif
        }

        private System.Collections.IEnumerator CaptureScreenshotStandalone()
        {
            if (hideUIInScreenshot)
            {
                debugger = GameObject.Find("SRDebugger");
                // Hide all UI temporarily
                gameUI.SetActive(false);
                victoryScreen.SetActive(false);
                failScreen.SetActive(false);
                debugger.SetActive(false);
            }

            // Wait one frame for UI to hide
            yield return null;

            // Capture screenshot
            string filename = $"BirthdayCake_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            string path = System.IO.Path.Combine(Application.persistentDataPath, filename);
            
            ScreenCapture.CaptureScreenshot(filename);
            Debug.Log($"Screenshot saved to: {path}");

            AudioManager.Instance.PlaySound("screenshot");

            if (hideUIInScreenshot)
            {
                // Wait another frame for screenshot to capture
                yield return null;
                
                // Restore UI
                ShowVictoryScreen();
                debugger.SetActive(true);
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private System.Collections.IEnumerator CaptureScreenshotWebGL()
        {
            if (hideUIInScreenshot)
            {
                // Hide all UI temporarily
                gameUI.SetActive(false);
                victoryScreen.SetActive(false);
                failScreen.SetActive(false);
                debugger = GameObject.Find("SRDebugger");
                debugger.SetActive(false);
                
                // Wait for UI to hide
                yield return null;
            }

            // Wait for end of frame to capture
            yield return new WaitForEndOfFrame();

            // Create texture from screen
            Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            // Convert to PNG
            byte[] bytes = screenshot.EncodeToPNG();
            Destroy(screenshot);

            // Trigger browser download
            string filename = $"BirthdayCake_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            DownloadFile(filename, bytes);

            AudioManager.Instance.PlaySound("screenshot");
            Debug.Log($"Screenshot downloaded: {filename}");

            if (hideUIInScreenshot)
            {
                // Restore UI
                yield return null;
                ShowVictoryScreen();
                debugger.SetActive(true);
            }
        }

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void DownloadFile(string filename, byte[] data, int size);

        private void DownloadFile(string filename, byte[] data)
        {
            DownloadFile(filename, data, data.Length);
        }
#endif

        private void RestartGame()
        {
            AudioManager.Instance.PlaySound("click");
            GameManager.Instance.RestartGame();
        }

        #endregion ==================================================================
    }
}