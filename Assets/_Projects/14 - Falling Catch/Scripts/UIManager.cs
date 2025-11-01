using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Devdy.FallingCatch
{
    /// <summary>
    /// Manages all UI elements including score, health, timer, and game over screen.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        #region Fields
        [Header("Gameplay UI")]
        [SerializeField] private TextMeshProUGUI scoreText; // Score display
        [SerializeField] private TextMeshProUGUI healthText; // Health display
        [SerializeField] private TextMeshProUGUI timerText; // Timer display

        [Header("Game Over UI")]
        [SerializeField] private GameObject gameOverPanel; // Game over screen
        [SerializeField] private TextMeshProUGUI finalScoreText; // Final score display
        [SerializeField] private Button restartButton; // Restart button
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            restartButton.onClick.AddListener(OnRestartClicked);
            HideGameOver();
        }
        #endregion

        #region UI Updates
        public void UpdateScore(int score)
        {
            scoreText.text = $"Score: {score}";
        }

        public void UpdateHealth(int health)
        {
            healthText.text = $"Health: {health}";
        }

        public void UpdateTimer(float timeRemaining)
        {
            int seconds = Mathf.CeilToInt(timeRemaining);
            timerText.text = $"Time: {seconds}s";
        }

        public void ShowGameOver(int finalScore)
        {
            gameOverPanel.SetActive(true);
            finalScoreText.text = $"Final Score: {finalScore}";
        }

        public void HideGameOver()
        {
            gameOverPanel.SetActive(false);
        }
        #endregion

        #region Button Handlers
        private void OnRestartClicked()
        {
            GameManager.Instance.RestartGame();
        }
        #endregion
    }
}