using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Devdy.RagdollTumbler
{
    /// <summary>
    /// Manages all UI elements including score, level display, and game state screens.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        #region Fields
        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI scoreText; // Score display
        [SerializeField] private TextMeshProUGUI levelText; // Level display
        
        [Header("Game Over Screen")]
        [SerializeField] private GameObject gameOverScreen; // Game over UI panel
        [SerializeField] private Button restartButton; // Restart button
        
        [Header("Level Complete Screen")]
        [SerializeField] private GameObject levelCompleteScreen; // Level complete UI panel
        [SerializeField] private Button nextLevelButton; // Next level button
        
        [Header("Game Complete Screen")]
        [SerializeField] private GameObject gameCompleteScreen; // Game complete UI panel
        [SerializeField] private TextMeshProUGUI finalScoreText; // Final score display
        [SerializeField] private Button playAgainButton; // Play again button
        
        #endregion ==================================================================

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            
            restartButton.onClick.AddListener(OnRestartClicked);
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        }

        #endregion ==================================================================

        #region UI Updates

        public void UpdateScore(int score)
        {
            scoreText.text = $"Score: {score}";
        }

        public void UpdateLevel(int level)
        {
            levelText.text = $"Level: {level}";
        }

        public void ShowGameOverScreen()
        {
            gameOverScreen.SetActive(true);
        }

        public void HideGameOverScreen()
        {
            gameOverScreen.SetActive(false);
        }

        public void ShowLevelCompleteScreen()
        {
            levelCompleteScreen.SetActive(true);
        }

        public void HideLevelCompleteScreen()
        {
            levelCompleteScreen.SetActive(false);
        }

        public void ShowGameCompleteScreen()
        {
            gameCompleteScreen.SetActive(true);
            finalScoreText.text = $"Final Score: {GameManager.Instance.CurrentScore}";
        }

        public void HideGameCompleteScreen()
        {
            gameCompleteScreen.SetActive(false);
        }

        #endregion ==================================================================

        #region Button Handlers

        private void OnRestartClicked()
        {
            GameManager.Instance.StartGame();
        }

        private void OnNextLevelClicked()
        {
            GameManager.Instance.LoadNextLevel();
        }

        private void OnPlayAgainClicked()
        {
            GameManager.Instance.StartGame();
        }

        #endregion ==================================================================
    }
}
