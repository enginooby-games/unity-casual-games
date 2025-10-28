using UnityEngine;

namespace Devdy.RagdollTumbler
{
    /// <summary>
    /// ScriptableObject holding all customizable game parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfigData", menuName = "RagdollTumbler/Config Data")]
    public class ConfigData : ScriptableObject
    {
        [Header("Physics Settings")]
        [Range(1f, 20f)]
        public float tumbleForce = 10f; // Strength of player's nudge impulse
        
        [Range(1f, 5f)]
        public float gravityScale = 2f; // Ragdoll gravity multiplier
        
        [Range(1f, 10f)]
        public float ragdollMass = 3f; // Physics mass of ragdoll
        
        [Header("Level Settings")]
        [Range(1, 10)]
        public int maxLevel = 5; // Number of playable levels
        
        [Header("Score Settings")]
        public int coinValue = 10; // Points per coin (fixed)
    }
}
