using UnityEngine;
using System.Collections.Generic;

namespace Devdy.RushHour
{
    /// <summary>
    /// Manages spawning of cars across different lanes.
    /// Controls spawn rate, initial car population, and assigns traffic lights.
    /// </summary>
    public class CarSpawner : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Spawn Settings")]
        [SerializeField] private GameObject carPrefab;
        [SerializeField] private int initialCarCount = 8;
        [SerializeField] private float gridSize = 1f;

        private LaneConfigData[] laneConfigs => GameManager.Instance.laneConfigs;

        [Header("Traffic Lights")]
        [SerializeField] private TrafficLight[] trafficLights;
        #endregion

        #region Private Fields
        private List<GameObject> activeCars = new List<GameObject>();
        private float spawnTimer;
        private float timeSinceLastSpawn;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            InitializeSpawner();
        }

        private void Update()
        {
            UpdateSpawning();
            CleanupDestroyedCars();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes spawner and spawns initial set of cars.
        /// </summary>
        private void InitializeSpawner()
        {
            if (carPrefab == null)
            {
                Debug.LogError("Car prefab not assigned to CarSpawner!");
                return;
            }

            if (laneConfigs == null || laneConfigs.Length == 0)
            {
                Debug.LogError("No lane configurations assigned to CarSpawner!");
                return;
            }

            for (int i = 0; i < initialCarCount; i++)
            {
                SpawnCar();
            }
        }

        /// <summary>
        /// Resets spawner state without destroying GameObject.
        /// Called by GameManager.RestartGame().
        /// </summary>
        public void ResetSpawner()
        {
            spawnTimer = 0f;
            timeSinceLastSpawn = 0f;
            for (int i = 0; i < initialCarCount; i++)
            {
                SpawnCar();
            }
        }
        #endregion

        #region Spawning
        /// <summary>
        /// Updates spawn timer and spawns cars based on spawn rate from SROptions.
        /// Spawn rate determines average time between spawns (lower = more frequent).
        /// </summary>
        private void UpdateSpawning()
        {
            int maxCars = SROptions.Current.RushHour_MaxCarsOnScreen;
            if (activeCars.Count >= maxCars) return;

            timeSinceLastSpawn += Time.deltaTime;
            float spawnRate = SROptions.Current.RushHour_CarSpawnRate;
            
            // Convert spawn rate to average spawn interval
            // SpawnRate 0.025 = spawn every ~2 seconds on average
            float spawnInterval = 1f / (spawnRate * 20f); // Multiply by 20 for reasonable frequency
            
            if (timeSinceLastSpawn >= spawnInterval)
            {
                SpawnCar();
                timeSinceLastSpawn = 0f;
            }
        }

        /// <summary>
        /// Spawns a new car on a random lane with configured properties.
        /// </summary>
        private void SpawnCar()
        {
            if (carPrefab == null || laneConfigs.Length == 0) return;

            LaneConfigData selectedLane = SelectRandomLane();
            if (selectedLane == null) return;

            GameObject newCar = Instantiate(carPrefab, transform);
            CarController carController = newCar.GetComponent<CarController>();

            if (carController == null)
            {
                Debug.LogError("Car prefab missing CarController component!");
                Destroy(newCar);
                return;
            }

            float carLength = Random.value > 0.6f ? 2.5f : 2f;
            Vector2 carSize = new Vector2(carLength, selectedLane.height);

            Vector3 spawnPosition = CalculateSpawnPosition(selectedLane, carLength);
            newCar.transform.position = spawnPosition;

            float adjustedSpeed = selectedLane.speed * SROptions.Current.RushHour_BaseCarSpeed / 2.5f;

            carController.Initialize(
                adjustedSpeed,
                selectedLane.direction,
                selectedLane.row,
                selectedLane.color,
                carSize
            );

            TrafficLight assignedLight = FindTrafficLightForLane(selectedLane.row);
            if (assignedLight != null)
            {
                carController.AssignTrafficLight(assignedLight);
            }

            activeCars.Add(newCar);
        }

        /// <summary>
        /// Selects a random lane from available configurations.
        /// </summary>
        private LaneConfigData SelectRandomLane()
        {
            if (laneConfigs.Length == 0) return null;
            return laneConfigs[Random.Range(0, laneConfigs.Length)];
        }

        /// <summary>
        /// Calculates spawn position based on lane and car properties.
        /// </summary>
        private Vector3 CalculateSpawnPosition(LaneConfigData lane, float carLength)
        {
            float xPos = (lane.direction > 0 ? -1f : 1f) * 6.5f;
            float yPos = (lane.row + lane.height / 2f - 7f) * gridSize;

            return new Vector3(xPos, yPos, 0f);
        }

        /// <summary>
        /// Finds traffic light assigned to specific lane row.
        /// </summary>
        private TrafficLight FindTrafficLightForLane(float laneRow)
        {
            if (trafficLights == null) return null;

            foreach (TrafficLight light in trafficLights)
            {
                if (light != null && Mathf.Abs(light.GetLaneRow() - laneRow) < 0.5f)
                {
                    return light;
                }
            }

            return null;
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// Removes destroyed cars from active list.
        /// </summary>
        private void CleanupDestroyedCars()
        {
            activeCars.RemoveAll(car => car == null);
        }

        /// <summary>
        /// Clears all active cars from scene.
        /// Called by GameManager.RestartGame().
        /// </summary>
        public void ClearAllCars()
        {
            foreach (GameObject car in activeCars)
            {
                if (car != null)
                {
                    Destroy(car);
                }
            }
            activeCars.Clear();
        }
        #endregion
    }

    /// <summary>
    /// Configuration data for a single traffic lane.
    /// Serializable struct for Inspector editing.
    /// </summary>
    [System.Serializable]
    public class LaneConfigData
    {
        [Tooltip("Row position in grid")]
        public float row;

        [Tooltip("Height of lane in grid units (1 or 2)")]
        public float height = 1f;

        [Tooltip("Speed multiplier for cars in this lane")]
        public float speed = 3f;

        [Tooltip("Direction: 1 for right, -1 for left")]
        public int direction = 1;

        [Tooltip("Color of cars in this lane")]
        public Color color = Color.red;

        [Tooltip("Danger level (affects bonus coin spawning)")]
        [Range(1, 5)]
        public int dangerLevel = 3;
    }
}