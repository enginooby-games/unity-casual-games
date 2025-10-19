using UnityEngine;
using TMPro;

namespace EyeShooter
{
    /// <summary>
    /// Main game controller that manages game state, score, and UI updates.
    /// Coordinates between eye tracking, crosshair movement, and enemy spawning.
    /// </summary>
    public class EyeShooterGame : MonoBehaviour
    {
        /// <summary>
        /// Maximum score achievable for instant reaction (milliseconds)
        /// </summary>
        private const int PERFECT_REACTION_MS = 500;

        /// <summary>
        /// Minimum score for slowest valid reaction (milliseconds)
        /// </summary>
        private const int MIN_REACTION_MS = 2000;

        /// <summary>
        /// Maximum score points awarded for perfect reaction
        /// </summary>
        private const int MAX_REACTION_SCORE = 100;

        /// <summary>
        /// Minimum score points awarded for slowest reaction
        /// </summary>
        private const int MIN_REACTION_SCORE = 10;

        [Header("References")]
        [SerializeField]
        [Tooltip("Reference to the eye tracking component")]
        private EyeTracker eyeTracker;

        [SerializeField]
        [Tooltip("Reference to the crosshair controller")]
        private Crosshair crosshair;

        [SerializeField]
        [Tooltip("Reference to the enemy spawner")]
        private EnemySpawner enemySpawner;

        [Header("UI")]
        [SerializeField]
        [Tooltip("Text component displaying current score")]
        private TextMeshProUGUI scoreText;

        [SerializeField]
        [Tooltip("Text component displaying game over message")]
        private TextMeshProUGUI gameOverText;

        [SerializeField]
        [Tooltip("Text component displaying instructions")]
        private TextMeshProUGUI instructionsText;

        /// <summary>
        /// Current player score
        /// </summary>
        private int currentScore;

        /// <summary>
        /// Flag indicating if the game is currently active
        /// </summary>
        private bool isGameActive;

        /// <summary>
        /// Initializes game components and starts the game
        /// </summary>
        private void Start()
        {
            InitializeGame();
        }

        /// <summary>
        /// Updates game state each frame, processing eye tracking and blink detection
        /// </summary>
        private void Update()
        {
            if (!isGameActive) 
            {
                CheckGameRestart();
                return;
            }

            ProcessEyeTracking();
            ProcessBlinkDetection();
        }

        /// <summary>
        /// Sets up initial game state and subscribes to events
        /// </summary>
        private void InitializeGame()
        {
            currentScore = 0;
            isGameActive = true;

            UpdateScoreDisplay();
            HideGameOver();
            ShowInstructions();

            enemySpawner.OnEnemyDestroyed += HandleEnemyDestroyed;
        }

        /// <summary>
        /// Processes eye position data and updates crosshair position
        /// </summary>
        private void ProcessEyeTracking()
        {
            Vector2 eyePosition = eyeTracker.GetNormalizedEyePosition();
            crosshair.UpdatePosition(eyePosition);
        }

        /// <summary>
        /// Detects blinks and triggers shooting action
        /// </summary>
        private void ProcessBlinkDetection()
        {
            if (eyeTracker.DetectBlink())
            {
                TryShootAtCrosshair();
            }
        }

        /// <summary>
        /// Attempts to shoot at the current crosshair position
        /// Checks for enemy hits and calculates score
        /// </summary>
        private void TryShootAtCrosshair()
        {
            Vector2 shootPosition = crosshair.GetWorldPosition();
            Enemy hitEnemy = enemySpawner.CheckHit(shootPosition);

            if (hitEnemy != null)
            {
                int reactionScore = CalculateReactionScore(hitEnemy.GetLifetime());
                AddScore(reactionScore);
                hitEnemy.Destroy();
            }

            crosshair.PlayShootEffect();
        }

        /// <summary>
        /// Calculates score based on reaction time
        /// Faster reactions yield higher scores
        /// </summary>
        /// <param name="reactionTimeMs">Reaction time in milliseconds</param>
        /// <returns>Score points awarded (between MIN_REACTION_SCORE and MAX_REACTION_SCORE)</returns>
        private int CalculateReactionScore(float reactionTimeMs)
        {
            if (reactionTimeMs <= PERFECT_REACTION_MS)
            {
                return MAX_REACTION_SCORE;
            }

            if (reactionTimeMs >= MIN_REACTION_MS)
            {
                return MIN_REACTION_SCORE;
            }

            float normalizedTime = (reactionTimeMs - PERFECT_REACTION_MS) / 
                                   (MIN_REACTION_MS - PERFECT_REACTION_MS);
            
            return Mathf.RoundToInt(
                Mathf.Lerp(MAX_REACTION_SCORE, MIN_REACTION_SCORE, normalizedTime)
            );
        }

        /// <summary>
        /// Adds points to the current score and updates UI
        /// </summary>
        /// <param name="points">Number of points to add</param>
        private void AddScore(int points)
        {
            currentScore += points;
            UpdateScoreDisplay();
        }

        /// <summary>
        /// Updates the score text UI element
        /// </summary>
        private void UpdateScoreDisplay()
        {
            scoreText.text = $"Score: {currentScore}";
        }

        /// <summary>
        /// Handles enemy destruction event
        /// Currently used for future extensions (sound effects, particles, etc.)
        /// </summary>
        /// <param name="enemy">The enemy that was destroyed</param>
        private void HandleEnemyDestroyed(Enemy enemy)
        {
            // Future: Add particle effects, sound effects, etc.
        }

        /// <summary>
        /// Ends the game and displays game over screen
        /// </summary>
        private void GameOver()
        {
            isGameActive = false;
            ShowGameOver();
            enemySpawner.StopSpawning();
        }

        /// <summary>
        /// Displays the game over UI with final score
        /// </summary>
        private void ShowGameOver()
        {
            gameOverText.text = $"Game Over!\nFinal Score: {currentScore}\nBlink to Restart";
            gameOverText.gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides the game over UI
        /// </summary>
        private void HideGameOver()
        {
            gameOverText.gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows initial game instructions
        /// </summary>
        private void ShowInstructions()
        {
            instructionsText.text = "Move eyes to aim\nBlink to shoot";
            // Invoke(nameof(HideInstructions), 3f);
        }

        /// <summary>
        /// Hides the instructions UI
        /// </summary>
        private void HideInstructions()
        {
            instructionsText.gameObject.SetActive(false);
        }

        /// <summary>
        /// Checks for restart input when game is not active
        /// </summary>
        private void CheckGameRestart()
        {
            if (eyeTracker.DetectBlink())
            {
                RestartGame();
            }
        }

        /// <summary>
        /// Resets game state and starts a new game
        /// </summary>
        private void RestartGame()
        {
            currentScore = 0;
            isGameActive = true;
            
            UpdateScoreDisplay();
            HideGameOver();
            
            enemySpawner.ClearAllEnemies();
            enemySpawner.StartSpawning();
        }

        /// <summary>
        /// Cleanup when component is destroyed
        /// </summary>
        private void OnDestroy()
        {
            if (enemySpawner != null)
            {
                enemySpawner.OnEnemyDestroyed -= HandleEnemyDestroyed;
            }
        }
    }
}