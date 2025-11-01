using UnityEngine;

namespace Devdy.FallingCatch
{
    /// <summary>
    /// Main game manager controlling game state, score, health, and timer.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        #region Fields
        [SerializeField] private ConfigData config; // Game configuration
        
        private int score;
        private int health;
        private float timeRemaining;
        private bool isGameActive;
        #endregion

        #region Properties
        public ConfigData Config => config;
        public int Score => score;
        public int Health => health;
        public float TimeRemaining => timeRemaining;
        public bool IsGameActive => isGameActive;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            InitializeGame();
            SRDebug.Instance.PinAllOptions("FallingCatch");
        }

        private void Update()
        {
            if (!isGameActive) return;

            UpdateTimer();
        }
        #endregion

        #region Game Flow
        /// <summary>
        /// Initializes or resets the game to starting state.
        /// </summary>
        private void InitializeGame()
        {
            score = 0;
            health = SROptions.Current.FallingCatch_StartingHealth;
            timeRemaining = SROptions.Current.FallingCatch_GameDuration;
            isGameActive = true;

            UIManager.Instance.UpdateScore(score);
            UIManager.Instance.UpdateHealth(health);
            UIManager.Instance.UpdateTimer(timeRemaining);
            UIManager.Instance.HideGameOver();

            SpawnManager.Instance.StartSpawning();
        }

        /// <summary>
        /// Restarts the game without reloading the scene.
        /// </summary>
        public void RestartGame()
        {
            SpawnManager.Instance.ClearAllObjects();
            InitializeGame();
        }

        private void UpdateTimer()
        {
            timeRemaining -= Time.deltaTime;
            UIManager.Instance.UpdateTimer(timeRemaining);

            if (timeRemaining <= 0)
            {
                EndGame();
            }
        }

        private void EndGame()
        {
            isGameActive = false;
            SpawnManager.Instance.StopSpawning();
            UIManager.Instance.ShowGameOver(score);
        }
        #endregion

        #region Score & Health
        public void AddScore(int points)
        {
            if (!isGameActive) return;

            score += points;
            UIManager.Instance.UpdateScore(score);
        }

        public void TakeDamage(int damage)
        {
            if (!isGameActive) return;

            health -= damage;
            UIManager.Instance.UpdateHealth(health);

            if (health <= 0)
            {
                health = 0;
                EndGame();
            }
        }
        #endregion
    }
}