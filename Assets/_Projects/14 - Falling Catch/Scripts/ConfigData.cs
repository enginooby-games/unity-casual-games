using UnityEngine;

namespace Devdy.FallingCatch
{
    /// <summary>
    /// Holds all configurable game settings for FallingCatch.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfigData", menuName = "Devdy/FallingCatch/Config")]
    public class ConfigData : ScriptableObject
    {
        [Header("Spawn Settings")]
        public float spawnInterval = 1f; // Time between spawns
        public float goodObjectRatio = 0.6f; // 60% good, 40% bad

        [Header("Object Settings")]
        public float fallSpeed = 3f; // How fast objects fall
        public float objectLifetime = 10f; // Auto-destroy after this time

        [Header("Game Settings")]
        public float gameDuration = 60f; // Game length in seconds
        public int startingHealth = 3; // Player starting health

        [Header("Scoring")]
        public int goodObjectScore = 10; // Points for catching good objects
        public int badObjectPenalty = -5; // Score penalty for bad objects
        public int healthLoss = 1; // Health lost per bad object

        [Header("Player Settings")]
        public float playerSpeed = 10f; // Movement speed
        public float minX = -8f; // Left boundary
        public float maxX = 8f; // Right boundary

        [Header("Visual Settings")]
        public Color goodColor = Color.green;
        public Color badColor = Color.red;
    }
}