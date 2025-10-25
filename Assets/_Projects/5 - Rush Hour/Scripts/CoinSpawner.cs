using UnityEngine;
using System.Collections.Generic;

namespace Devdy.RushHour
{
    /// <summary>
    /// Manages spawning of regular and bonus coins across lanes.
    /// Ensures coins don't spawn on traffic lights and maintains minimum coin count.
    /// </summary>
    public class CoinSpawner : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Prefabs")]
        [SerializeField] private GameObject coinPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private int minCoinsOnScreen = 8;
        [SerializeField] private float spawnCheckInterval = 1f;
        [SerializeField] private float gridSize = 1f;

       private LaneConfigData[] laneConfigs => GameManager.Instance.laneConfigs;

        [Header("Bonus Coin Settings")]
        [SerializeField] private float bonusCoinChance = 0.4f;
        [SerializeField] private int minDangerLevelForBonus = 4;

        [Header("Safe Zones")]
        [SerializeField] private float safeZoneTopRow = 0.5f;
        [SerializeField] private float safeZoneBottomRow = 13f;
        [SerializeField] private float safeZoneSpawnChance = 0.3f;
        #endregion

        #region Private Fields
        private List<GameObject> activeCoins = new List<GameObject>();
        private float spawnTimer;
        #endregion

        #region Constants
        private const int GRID_COLS = 16;
        private const float MIN_COIN_SPACING = 2f;
        private const float TRAFFIC_LIGHT_ZONE = 15f;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            InitializeSpawner();
        }

        private void Update()
        {
            UpdateSpawning();
            CleanupDestroyedCoins();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes spawner and spawns initial set of coins.
        /// </summary>
        private void InitializeSpawner()
        {
            if (coinPrefab == null)
            {
                Debug.LogError("Coin prefab not assigned to CoinSpawner!");
                return;
            }

            if (laneConfigs == null || laneConfigs.Length == 0)
            {
                Debug.LogWarning("No lane configurations assigned to CoinSpawner! Coins will only spawn in safe zones.");
            }

            SpawnInitialCoins();
        }

        /// <summary>
        /// Spawns initial batch of coins.
        /// </summary>
        private void SpawnInitialCoins()
        {
            for (int i = 0; i < minCoinsOnScreen; i++)
            {
                SpawnCoin();
            }
        }

        /// <summary>
        /// Resets spawner state without destroying GameObject.
        /// Called by GameManager.RestartGame().
        /// </summary>
        public void ResetSpawner()
        {
            spawnTimer = 0f;
            SpawnInitialCoins();
        }
        #endregion

        #region Spawning
        /// <summary>
        /// Checks spawn conditions and spawns new coins when needed.
        /// </summary>
        private void UpdateSpawning()
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer < spawnCheckInterval) return;

            spawnTimer = 0f;

            if (activeCoins.Count >= minCoinsOnScreen) return;

            int coinsToSpawn = minCoinsOnScreen - activeCoins.Count;
            for (int i = 0; i < coinsToSpawn; i++)
            {
                SpawnCoin();
            }
        }

        /// <summary>
        /// Spawns a single coin on a valid lane position.
        /// Avoids traffic light zones.
        /// </summary>
        private void SpawnCoin()
        {
            if (coinPrefab == null) return;

            bool shouldSpawnInSafeZone = Random.value < safeZoneSpawnChance;

            Vector3 spawnPosition;
            bool isBonus = false;
            int coinValue = 1;

            if (shouldSpawnInSafeZone || laneConfigs == null || laneConfigs.Length == 0)
            {
                spawnPosition = GetSafeZoneSpawnPosition();
            }
            else
            {
                LaneConfigData selectedLane = SelectValidLane();
                if (selectedLane == null)
                {
                    // Fallback to safe zone if no valid lane
                    spawnPosition = GetSafeZoneSpawnPosition();
                }
                else
                {
                    spawnPosition = GetLaneSpawnPosition(selectedLane);

                    if (selectedLane.dangerLevel >= minDangerLevelForBonus && 
                        Random.value < bonusCoinChance)
                    {
                        isBonus = true;
                        coinValue = selectedLane.dangerLevel * 2;
                    }
                }
            }

            if (IsTooCloseToOtherCoins(spawnPosition)) return;

            CreateCoin(spawnPosition, isBonus, coinValue);
        }

        /// <summary>
        /// Selects a valid lane for coin spawning.
        /// Always returns a lane if laneConfigs is populated.
        /// </summary>
        private LaneConfigData SelectValidLane()
        {
            if (laneConfigs == null || laneConfigs.Length == 0) return null;

            // Try to select from 70% probability lanes first
            LaneConfigData[] validLanes = System.Array.FindAll(laneConfigs, 
                lane => Random.value < 0.7f);

            // If no lanes selected, just pick any random lane
            if (validLanes.Length == 0)
            {
                return laneConfigs[Random.Range(0, laneConfigs.Length)];
            }

            return validLanes[Random.Range(0, validLanes.Length)];
        }

        /// <summary>
        /// Gets spawn position within a lane, avoiding traffic light zone.
        /// </summary>
        private Vector3 GetLaneSpawnPosition(LaneConfigData lane)
        {
            int col = Random.Range(1, GRID_COLS - 1);
            
            while (col >= TRAFFIC_LIGHT_ZONE - 1)
            {
                col = Random.Range(1, GRID_COLS - 1);
            }

            float xPos = (col - GRID_COLS / 2f) * gridSize;
            float yPos = (lane.row + (lane.height == 2 ? 0.5f : 0f) - 7f) * gridSize;

            Debug.Log($"Spawning lane coin at row {lane.row}, col {col} → position ({xPos}, {yPos})");

            return new Vector3(xPos, yPos, 0f);
        }

        /// <summary>
        /// Gets spawn position in safe zone (top or bottom).
        /// </summary>
        private Vector3 GetSafeZoneSpawnPosition()
        {
            float row = Random.value < 0.5f ? safeZoneTopRow : safeZoneBottomRow;
            int col = Random.Range(0, GRID_COLS);

            float xPos = (col - GRID_COLS / 2f) * gridSize;
            float yPos = (row - 7f) * gridSize;

            Debug.Log($"Spawning safe zone coin at row {row}, col {col} → position ({xPos}, {yPos})");

            return new Vector3(xPos, yPos, 0f);
        }

        /// <summary>
        /// Checks if position is too close to existing coins.
        /// </summary>
        private bool IsTooCloseToOtherCoins(Vector3 position)
        {
            foreach (GameObject coin in activeCoins)
            {
                if (coin == null) continue;

                float distance = Vector3.Distance(position, coin.transform.position);
                if (distance < MIN_COIN_SPACING * gridSize)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates and configures a coin at specified position.
        /// </summary>
        private void CreateCoin(Vector3 position, bool isBonus, int value)
        {
            position.y = Mathf.Floor(position.y);
            position.x++;
            GameObject newCoin = Instantiate(coinPrefab, position, Quaternion.identity, transform);
            Coin coinComponent = newCoin.GetComponent<Coin>();

            if (coinComponent != null && isBonus)
            {
                coinComponent.SetBonusCoin(value);
            }

            activeCoins.Add(newCoin);
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// Removes destroyed coins from active list.
        /// </summary>
        private void CleanupDestroyedCoins()
        {
            activeCoins.RemoveAll(coin => coin == null);
        }

        /// <summary>
        /// Clears all active coins from scene.
        /// Called by GameManager.RestartGame().
        /// </summary>
        public void ClearAllCoins()
        {
            foreach (GameObject coin in activeCoins)
            {
                if (coin != null)
                {
                    Destroy(coin);
                }
            }
            activeCoins.Clear();
        }
        #endregion
    }
}