using System.Collections.Generic;
using UnityEngine;

namespace Devdy.BirthdayCake
{
    /// <summary>
    /// Manages layer spawning, dropping, and cake completion logic.
    /// </summary>
    public class CakeBuilder : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform cakeBase; // Starting platform
        [SerializeField] private GameObject candlePrefab; // Special prefab for candles
        [SerializeField]  private List<LayerData> layerPool; // All available layer types
        private List<LayerController> spawnedLayers;
        private LayerController currentLayer;
        private int layersPlaced;

        private const float SPAWN_HEIGHT = 5f;
        private const float LAYER_SPAWN_DELAY = 1.5f;

        #region ==================================================================== Initialization

        private void Awake()
        {
            spawnedLayers = new List<LayerController>();
        }

        /// <summary>
        /// Initializes the cake builder and spawns the first layer.
        /// </summary>
        public void Initialize()
        {
            layersPlaced = 0;
            SpawnNextLayer();
        }

        #endregion ==================================================================

        #region ==================================================================== Layer Spawning

        /// <summary>
        /// Spawns a random layer at the spawn point.
        /// </summary>
        private void SpawnNextLayer()
        {
            if (layersPlaced >= GameManager.Instance.Config.TotalLayers)
            {
                SpawnCandles();
                return;
            }

            LayerData nextLayer = GetNextLayer();
            Vector3 spawnPosition = spawnPoint.position;

            GameObject layerObject = Instantiate(nextLayer.Prefab, spawnPosition, Quaternion.identity, transform);
            currentLayer = layerObject.AddComponent<LayerController>();
            
            // Calculate size multiplier: each layer gets progressively smaller
            float sizeReduction = GameManager.Instance.Config.SizeReduction;
            float sizeMultiplier = 1f - (layersPlaced * sizeReduction);
            currentLayer.Initialize(nextLayer, GameManager.Instance.Config.DropSpeed, sizeMultiplier * SROptions.Current.BirthdayCake_SizeScale);

            spawnedLayers.Add(currentLayer);
        }

        /// <summary>
        /// Returns a random LayerData from the pool.
        /// </summary>
        private LayerData GetNextLayer()
        {
            var nextId = GameManager.Instance.CurrentLayer % layerPool.Count;
            return layerPool[nextId];

            if (layerPool.Count == 0)
            {
                Debug.LogError("Layer pool is empty!");
                return null;
            }

            return layerPool[Random.Range(0, layerPool.Count)];
        }

        /// <summary>
        /// Spawns candles on top of the cake when all layers are placed.
        /// </summary>
        private void SpawnCandles()
        {
            if (candlePrefab == null)
            {
                Debug.LogWarning("Candle prefab not assigned!");
                GameManager.Instance.OnCakeComplete();
                return;
            }
            
            var topLayer = spawnedLayers[^1];
            var candles = Instantiate(candlePrefab, Vector3.zero, Quaternion.identity, topLayer.transform);
            candles.transform.localPosition = Vector3.zero;
            Invoke(nameof(LightCandles), 0.2f);
            return;
            int candleCount = GameManager.Instance.Config.CandleCount;
            Vector3 topPosition = GetTopLayerPosition();

            for (int i = 0; i < candleCount; i++)
            {
                float xOffset = (i - candleCount / 2f) * 0.4f;
                Vector3 candlePos = topPosition + new Vector3(xOffset, 0.7f, 0);
                Instantiate(candlePrefab, candlePos, Quaternion.identity, transform);
                Instantiate(candlePrefab, candlePos, Quaternion.identity, topLayer.transform);
            }

            Invoke(nameof(LightCandles), LAYER_SPAWN_DELAY);
        }

        /// <summary>
        /// Triggers candle lighting animation and completes the game.
        /// </summary>
        private void LightCandles()
        {
            // Find all candle objects and trigger their particle systems
            GameObject[] candles = GameObject.FindGameObjectsWithTag("Candle");
            foreach (GameObject candle in candles)
            {
                ParticleSystem flame = candle.GetComponentInChildren<ParticleSystem>();
                flame?.Play();
            }

            GameManager.Instance.OnCakeComplete();
        }

        /// <summary>
        /// Returns the position of the topmost placed layer.
        /// </summary>
        private Vector3 GetTopLayerPosition()
        {
            if (spawnedLayers.Count == 0) return cakeBase.position;
            
            LayerController topLayer = spawnedLayers[^1];
            return topLayer.transform.position;
        }

        #endregion ==================================================================

        #region ==================================================================== Input Handling

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;
            if (currentLayer == null || currentLayer.IsDropped) return;

            // Drop layer on click/tap
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                DropCurrentLayer();
            }
        }

        /// <summary>
        /// Drops the current layer and spawns the next one.
        /// </summary>
        private void DropCurrentLayer()
        {
            currentLayer.Drop();
            layersPlaced++;

            Invoke(nameof(SpawnNextLayer), LAYER_SPAWN_DELAY);
        }

        #endregion ==================================================================

        #region ==================================================================== Reset

        /// <summary>
        /// Clears all spawned layers and resets the cake.
        /// </summary>
        public void ResetCake()
        {
            foreach (LayerController layer in spawnedLayers)
            {
                if (layer != null) Destroy(layer.gameObject);
            }

            spawnedLayers.Clear();

            // Clear candles
            GameObject[] candles = GameObject.FindGameObjectsWithTag("Candle");
            foreach (GameObject candle in candles)
            {
                Destroy(candle);
            }

            currentLayer = null;
            layersPlaced = 0;
        }

        public void CollapseAllLayers()
        {
            // foreach (LayerController layer in spawnedLayers)
            // {
            //     var rb = layer.GetComponent<Rigidbody2D>();
            //     rb.AddForce(new Vector2(150,150) );
            // }
        }

        #endregion ==================================================================
    }
}