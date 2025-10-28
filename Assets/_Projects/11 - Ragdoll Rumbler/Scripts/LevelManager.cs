using UnityEngine;

namespace Devdy.RagdollTumbler
{
    /// <summary>
    /// Manages level loading, spawning platforms, coins, hazards, and ragdoll initialization.
    /// </summary>
    public class LevelManager : Singleton<LevelManager>
    {
        #region Fields
        [Header("Prefabs")]
        [SerializeField] private GameObject ragdollPrefab; // Ragdoll character prefab
        [SerializeField] private GameObject coinPrefab; // Coin collectible prefab
        [SerializeField] private GameObject hazardPrefab; // Hazard (spike) prefab
        [SerializeField] private GameObject goalPrefab; // Goal zone prefab
        [SerializeField] private GameObject platformPrefab; // Platform prefab
        
        [Header("Spawn Points")]
        [SerializeField] private Transform ragdollSpawnPoint; // Starting position for ragdoll
        [SerializeField] private Transform levelContainer; // Parent container for level objects
        
        private GameObject currentRagdoll; // Active ragdoll instance
        
        #endregion ==================================================================

        #region Level Loading

        /// <summary>
        /// Loads a level by clearing previous level objects and spawning new elements.
        /// Procedurally generates platforms, coins, hazards, and goal based on level number.
        /// </summary>
        public void LoadLevel(int levelNumber)
        {
            ClearLevel();
            
            SpawnRagdoll();
            GenerateLevelElements(levelNumber);
        }

        private void ClearLevel()
        {
            if (currentRagdoll != null)
            {
                Destroy(currentRagdoll);
            }
            
            foreach (Transform child in levelContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void SpawnRagdoll()
        {
            currentRagdoll = Instantiate(ragdollPrefab, ragdollSpawnPoint.position, Quaternion.identity);
            
            Rigidbody2D rb = currentRagdoll.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = GameManager.Instance.Config.gravityScale;
                rb.mass = GameManager.Instance.Config.ragdollMass;
            }
        }

        /// <summary>
        /// Procedurally generates level elements based on level difficulty.
        /// Higher levels have more platforms, coins, and hazards.
        /// </summary>
        private void GenerateLevelElements(int levelNumber)
        {
            float yOffset = -3f; // Starting Y position below spawn
            int platformCount = 3 + levelNumber; // More platforms per level
            int coinCount = 5 + (levelNumber * 2); // More coins per level
            int hazardCount = 1 + levelNumber; // More hazards per level
            
            // Spawn platforms
            for (int i = 0; i < platformCount; i++)
            {
                float xPos = Random.Range(-5f, 5f);
                float yPos = yOffset - (i * 4f);
                
                Vector3 platformPos = new Vector3(xPos, yPos, 0f);
                Instantiate(platformPrefab, platformPos, Quaternion.identity, levelContainer);
            }
            
            // Spawn coins
            for (int i = 0; i < coinCount; i++)
            {
                float xPos = Random.Range(-6f, 6f);
                float yPos = Random.Range(yOffset, yOffset - (platformCount * 4f));
                
                Vector3 coinPos = new Vector3(xPos, yPos, 0f);
                Instantiate(coinPrefab, coinPos, Quaternion.identity, levelContainer);
            }
            
            // Spawn hazards
            for (int i = 0; i < hazardCount; i++)
            {
                float xPos = Random.Range(-5f, 5f);
                float yPos = Random.Range(yOffset - 4f, yOffset - (platformCount * 4f) + 4f);
                
                Vector3 hazardPos = new Vector3(xPos, yPos, 0f);
                Instantiate(hazardPrefab, hazardPos, Quaternion.identity, levelContainer);
            }
            
            // Spawn goal at bottom
            float goalY = yOffset - (platformCount * 4f) - 2f;
            Vector3 goalPos = new Vector3(0f, goalY, 0f);
            Instantiate(goalPrefab, goalPos, Quaternion.identity, levelContainer);
        }

        #endregion ==================================================================
    }
}
