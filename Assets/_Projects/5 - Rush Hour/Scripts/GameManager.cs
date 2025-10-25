using UnityEngine;
using TMPro;

namespace Devdy.RushHour
{
    /// <summary>
    /// Main game manager controlling game state, score, lives and game flow.
    /// Inherits from Singleton for global access.
    /// Manages shared data accessed by multiple systems.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        #region Shared Data - Accessed by Multiple Systems
        [Header("Game State")]
        public int CurrentCoins;
        public int CurrentScore;
        public int CurrentLives;
        public int ComboCount;
        public int MaxCombo;
        public int LevelsPassed;
        public bool IsGameOver;
        public LaneConfigData[] laneConfigs;
        #endregion

        #region Inspector Fields
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI finalStatsText;
        #endregion

        #region Private Fields
        private float comboTimer;
        #endregion

        #region Constants
        private const float COMBO_TIMEOUT = 3f;
        private const int COMBO_BONUS_THRESHOLD = 3;
        private const string HEART_ICON = "❤️";
        private const int COINS_PER_LEVEL = 5;
        private const int SCORE_PER_LEVEL = 20;
        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            SRDebug.Instance.PinAllOptions("RushHour");
            InitializeGame();
        }

        private void Update()
        {
            if (!IsGameOver)
            {
                UpdateComboTimer();
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes game state with default values.
        /// Called on game start.
        /// </summary>
        private void InitializeGame()
        {
            CurrentLives = SROptions.Current.RushHour_StartingLives;
            CurrentCoins = 0;
            CurrentScore = 0;
            ComboCount = 0;
            MaxCombo = 0;
            LevelsPassed = 0;
            comboTimer = 0f;
            IsGameOver = false;

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            UpdateUI();
        }
        #endregion

        #region Game Flow
        /// <summary>
        /// Restarts the game without reloading scene.
        /// Resets all runtime states and reinitializes gameplay.
        /// </summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            IsGameOver = false;

            InitializeGame();

            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.ResetPlayer();
            }

            CarSpawner carSpawner = FindObjectOfType<CarSpawner>();
            if (carSpawner != null)
            {
                carSpawner.ClearAllCars();
                carSpawner.ResetSpawner();
            }

            CoinSpawner coinSpawner = FindObjectOfType<CoinSpawner>();
            if (coinSpawner != null)
            {
                coinSpawner.ClearAllCoins();
                coinSpawner.ResetSpawner();
            }

            PowerupManager powerupManager = FindObjectOfType<PowerupManager>();
            if (powerupManager != null)
            {
                powerupManager.ResetPowerups();
            }
        }
        #endregion

        #region Coin Management
        /// <summary>
        /// Adds coins to player's total and updates combo system.
        /// Calculates bonus based on combo count.
        /// </summary>
        public void AddCoins(int amount)
        {
            if (IsGameOver) return;

            CurrentCoins += amount;
            ComboCount++;
            comboTimer = COMBO_TIMEOUT;

            if (ComboCount > MaxCombo)
            {
                MaxCombo = ComboCount;
            }

            int comboBonus = ComboCount / COMBO_BONUS_THRESHOLD;
            AddScore(amount + comboBonus);

            UpdateUI();
        }

        /// <summary>
        /// Attempts to spend coins for powerups or items.
        /// </summary>
        public bool SpendCoins(int cost)
        {
            if (CurrentCoins >= cost)
            {
                CurrentCoins -= cost;
                UpdateUI();
                return true;
            }
            return false;
        }
        #endregion

        #region Score Management
        /// <summary>
        /// Adds score points to player's total.
        /// </summary>
        public void AddScore(int points)
        {
            if (IsGameOver) return;

            CurrentScore += points;
            UpdateUI();
        }
        #endregion

        #region Life Management
        /// <summary>
        /// Reduces player lives by one and checks for game over.
        /// Resets combo on death.
        /// </summary>
        public void LoseLife()
        {
            if (IsGameOver) return;

            CurrentLives--;
            ComboCount = 0;
            comboTimer = 0f;

            if (CurrentLives <= 0)
            {
                TriggerGameOver();
            }

            UpdateUI();
        }
        #endregion

        #region Level Management
        /// <summary>
        /// Called when player successfully completes a level.
        /// Awards coins and score based on configuration.
        /// </summary>
        public void CompleteLevel()
        {
            if (IsGameOver) return;

            LevelsPassed++;
            CurrentCoins += COINS_PER_LEVEL;
            CurrentScore += SCORE_PER_LEVEL;

            UpdateUI();
        }
        #endregion

        #region Combo System
        /// <summary>
        /// Updates combo timer and resets combo if timer expires.
        /// </summary>
        private void UpdateComboTimer()
        {
            if (comboTimer > 0f)
            {
                comboTimer -= Time.deltaTime;

                if (comboTimer <= 0f)
                {
                    ComboCount = 0;
                }
            }
        }
        #endregion

        #region Game Over
        /// <summary>
        /// Triggers game over state and displays final statistics.
        /// Pauses game time.
        /// </summary>
        private void TriggerGameOver()
        {
            IsGameOver = true;

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (finalStatsText != null)
            {
                finalStatsText.text = $"🏆 Score: {CurrentScore}\n" +
                                     $"💰 Coins: {CurrentCoins}\n" +
                                     $"🔥 Max Combo: x{MaxCombo}\n" +
                                     $"✅ Levels: {LevelsPassed}";
            }

            Time.timeScale = 0f;
        }
        #endregion

        #region UI Updates
        /// <summary>
        /// Updates all UI text elements with current game state.
        /// </summary>
        private void UpdateUI()
        {
            if (coinsText != null)
            {
                coinsText.text = $"Coins: {CurrentCoins}";
            }

            if (scoreText != null)
            {
                scoreText.text = $"Score: {CurrentScore}";
            }

            if (livesText != null)
            {
                string hearts = "";
                for (int i = 0; i < CurrentLives; i++)
                {
                    hearts += HEART_ICON;
                }

                livesText.text = $"Lives: {CurrentLives}";
            }

            if (comboText != null)
            {
                comboText.text = ComboCount > 1 ? $"x{ComboCount} COMBO!" : "";
            }
        }
        #endregion
    }
}