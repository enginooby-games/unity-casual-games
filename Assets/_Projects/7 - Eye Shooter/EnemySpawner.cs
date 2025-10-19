using UnityEngine;
using System.Collections.Generic;

namespace EyeShooter
{
    /// <summary>
    /// Manages enemy spawning, positioning, and hit detection.
    /// Spawns enemies at random positions with increasing difficulty.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        /// <summary>
        /// Initial time between enemy spawns (seconds)
        /// </summary>
        private const float INITIAL_SPAWN_INTERVAL = 2f;

        /// <summary>
        /// Minimum spawn interval at maximum difficulty (seconds)
        /// </summary>
        private const float MIN_SPAWN_INTERVAL = 0.5f;

        /// <summary>
        /// Rate at which spawn interval decreases per spawn
        /// </summary>
        private const float DIFFICULTY_INCREASE_RATE = 0.05f;

        /// <summary>
        /// Percentage of screen boundaries to use for spawning (0-1)
        /// </summary>
        private const float SPAWN_BOUNDARY_PERCENT = 0.85f;

        /// <summary>
        /// Minimum distance between newly spawned enemies
        /// </summary>
        private const float MIN_SPAWN_DISTANCE = 2f;

        [Header("Enemy Settings")]
        [SerializeField]
        [Tooltip("Prefab for enemy objects")]
        private GameObject enemyPrefab;

        [SerializeField]
        [Tooltip("Maximum number of active enemies")]
        private int maxActiveEnemies = 5;

        /// <summary>
        /// Event triggered when an enemy is destroyed
        /// </summary>
        public event System.Action<Enemy> OnEnemyDestroyed;

        /// <summary>
        /// Camera used for spawn position calculations
        /// </summary>
        private Camera mainCamera;

        /// <summary>
        /// List of currently active enemies
        /// </summary>
        private List<Enemy> activeEnemies;

        /// <summary>
        /// Timer for spawn interval
        /// </summary>
        private float spawnTimer;

        /// <summary>
        /// Current time between spawns
        /// </summary>
        private float currentSpawnInterval;

        /// <summary>
        /// Flag indicating if spawning is active
        /// </summary>
        private bool isSpawning;

        /// <summary>
        /// Initializes spawner and starts spawning enemies
        /// </summary>
        private void Start()
        {
            InitializeComponents();
            StartSpawning();
        }

        /// <summary>
        /// Sets up initial state and component references
        /// </summary>
        private void InitializeComponents()
        {
            mainCamera = Camera.main;
            activeEnemies = new List<Enemy>();
            currentSpawnInterval = INITIAL_SPAWN_INTERVAL;
        }

        /// <summary>
        /// Begins enemy spawning
        /// </summary>
        public void StartSpawning()
        {
            isSpawning = true;
            spawnTimer = 0f;
            currentSpawnInterval = INITIAL_SPAWN_INTERVAL;
        }

        /// <summary>
        /// Stops enemy spawning
        /// </summary>
        public void StopSpawning()
        {
            isSpawning = false;
        }

        /// <summary>
        /// Updates spawn timer and spawns enemies when ready
        /// </summary>
        private void Update()
        {
            if (!isSpawning) return;

            UpdateSpawnTimer();
            RemoveDestroyedEnemies();
        }

        /// <summary>
        /// Updates spawn timer and triggers spawning when interval is reached
        /// </summary>
        private void UpdateSpawnTimer()
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= currentSpawnInterval && CanSpawnEnemy())
            {
                SpawnEnemy();
                ResetSpawnTimer();
                IncreaseDifficulty();
            }
        }

        /// <summary>
        /// Checks if a new enemy can be spawned
        /// </summary>
        /// <returns>True if spawn is allowed</returns>
        private bool CanSpawnEnemy()
        {
            return activeEnemies.Count < maxActiveEnemies;
        }

        /// <summary>
        /// Spawns a new enemy at a random valid position
        /// </summary>
        private void SpawnEnemy()
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, transform);
            Enemy enemy = enemyObject.GetComponent<Enemy>();

            activeEnemies.Add(enemy);
        }

        /// <summary>
        /// Calculates a random spawn position within screen boundaries
        /// Ensures minimum distance from existing enemies
        /// </summary>
        /// <returns>Valid world space spawn position</returns>
        private Vector3 GetRandomSpawnPosition()
        {
            const int maxAttempts = 10;
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 position = GenerateRandomScreenPosition();

                if (IsValidSpawnPosition(position))
                {
                    return position;
                }
            }

            return GenerateRandomScreenPosition();
        }

        /// <summary>
        /// Generates a random position within screen boundaries
        /// </summary>
        /// <returns>Random world space position</returns>
        private Vector3 GenerateRandomScreenPosition()
        {
            float marginX = (1f - SPAWN_BOUNDARY_PERCENT) * 0.5f;
            float marginY = (1f - SPAWN_BOUNDARY_PERCENT) * 0.5f;

            float randomX = Random.Range(marginX, 1f - marginX);
            float randomY = Random.Range(marginY, 1f - marginY);

            Vector3 worldPos = mainCamera.ViewportToWorldPoint(
                new Vector3(randomX, randomY, mainCamera.nearClipPlane + 10f)
            );

            return new Vector3(worldPos.x, worldPos.y, 0f);
        }

        /// <summary>
        /// Checks if a position is valid for spawning
        /// Validates minimum distance from existing enemies
        /// </summary>
        /// <param name="position">Position to validate</param>
        /// <returns>True if position is valid</returns>
        private bool IsValidSpawnPosition(Vector3 position)
        {
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy == null) continue;

                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < MIN_SPAWN_DISTANCE)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Resets the spawn timer
        /// </summary>
        private void ResetSpawnTimer()
        {
            spawnTimer = 0f;
        }

        /// <summary>
        /// Increases difficulty by reducing spawn interval
        /// </summary>
        private void IncreaseDifficulty()
        {
            currentSpawnInterval = Mathf.Max(
                MIN_SPAWN_INTERVAL,
                currentSpawnInterval - DIFFICULTY_INCREASE_RATE
            );
        }

        /// <summary>
        /// Removes null references from active enemies list
        /// </summary>
        private void RemoveDestroyedEnemies()
        {
            activeEnemies.RemoveAll(enemy => enemy == null);
        }

        /// <summary>
        /// Checks if a position hits any active enemy
        /// </summary>
        /// <param name="position">World space position to check</param>
        /// <returns>Hit enemy or null if no hit</returns>
        public Enemy CheckHit(Vector2 position)
        {
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy == null) continue;

                if (enemy.CheckHit(position))
                {
                    OnEnemyDestroyed?.Invoke(enemy);
                    return enemy;
                }
            }

            return null;
        }

        /// <summary>
        /// Removes and destroys all active enemies
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }

            activeEnemies.Clear();
        }

        /// <summary>
        /// Visualizes spawn boundaries in Scene view
        /// </summary>
        private void OnDrawGizmos()
        {
            if (mainCamera == null) return;

            Gizmos.color = Color.cyan;

            float marginX = (1f - SPAWN_BOUNDARY_PERCENT) * 0.5f;
            float marginY = (1f - SPAWN_BOUNDARY_PERCENT) * 0.5f;

            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(
                new Vector3(marginX, marginY, 10f)
            );

            Vector3 topRight = mainCamera.ViewportToWorldPoint(
                new Vector3(1f - marginX, 1f - marginY, 10f)
            );

            Gizmos.DrawLine(new Vector3(bottomLeft.x, bottomLeft.y, 0), new Vector3(topRight.x, bottomLeft.y, 0));
            Gizmos.DrawLine(new Vector3(topRight.x, bottomLeft.y, 0), new Vector3(topRight.x, topRight.y, 0));
            Gizmos.DrawLine(new Vector3(topRight.x, topRight.y, 0), new Vector3(bottomLeft.x, topRight.y, 0));
            Gizmos.DrawLine(new Vector3(bottomLeft.x, topRight.y, 0), new Vector3(bottomLeft.x, bottomLeft.y, 0));
        }
    }
}