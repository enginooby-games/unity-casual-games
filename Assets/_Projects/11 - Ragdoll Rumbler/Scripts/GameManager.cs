using UnityEngine;

namespace Devdy.RagdollTumbler
{
    /// <summary>
    /// Central manager for game state, level progression, and shared configuration data.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        #region Fields
        [SerializeField] private ConfigData config; // Shared configuration data
        
        private int currentLevel = 1; // Current level number
        private int currentScore = 0; // Total score accumulated
        private bool isGameActive = false; // Current game state
        
        public ConfigData Config => config;
        public int CurrentLevel => currentLevel;
        public int CurrentScore => currentScore;
        public bool IsGameActive => isGameActive;
        
        #endregion ==================================================================

        #region Unity Lifecycle
        
        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            SRDebug.Instance.PinAllOptions("RagdollTumbler");
            StartGame();
        }

        #endregion ==================================================================

        #region Game Flow

        /// <summary>
        /// Initializes a new game session.
        /// </summary>
        public void StartGame()
        {
            isGameActive = true;
            currentLevel = 1;
            currentScore = 0;
            
            UIManager.Instance.UpdateScore(currentScore);
            UIManager.Instance.UpdateLevel(currentLevel);
            UIManager.Instance.HideGameOverScreen();
            UIManager.Instance.HideLevelCompleteScreen();
            
            LevelManager.Instance.LoadLevel(currentLevel);
        }

        /// <summary>
        /// Advances to the next level or shows game complete if max level reached.
        /// </summary>
        public void CompleteLevel()
        {
            if (!isGameActive) return;
            
            isGameActive = false;
            
            if (currentLevel >= config.maxLevel)
            {
                UIManager.Instance.ShowGameCompleteScreen();
                return;
            }
            
            currentLevel++;
            UIManager.Instance.UpdateLevel(currentLevel);
            UIManager.Instance.ShowLevelCompleteScreen();
        }

        /// <summary>
        /// Loads the next level after level complete screen.
        /// </summary>
        public void LoadNextLevel()
        {
            isGameActive = true;
            UIManager.Instance.HideLevelCompleteScreen();
            LevelManager.Instance.LoadLevel(currentLevel);
        }

        /// <summary>
        /// Handles game over state.
        /// </summary>
        public void GameOver()
        {
            if (!isGameActive) return;
            
            isGameActive = false;
            UIManager.Instance.ShowGameOverScreen();
        }

        /// <summary>
        /// Adds score from collectibles.
        /// </summary>
        public void AddScore(int points)
        {
            if (!isGameActive) return;
            
            currentScore += points;
            UIManager.Instance.UpdateScore(currentScore);
        }

        /// <summary>
        /// Restarts the entire game without reloading the scene.
        /// Called when customizable parameters are changed via SRDebugger.
        /// </summary>
        public static void RestartGame()
        {
            if (Instance == null) return;
            
            Instance.StartGame();
        }

        #endregion ==================================================================
    }
}
