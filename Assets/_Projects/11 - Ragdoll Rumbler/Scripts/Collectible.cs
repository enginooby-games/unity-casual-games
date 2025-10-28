using UnityEngine;

namespace Devdy.RagdollTumbler
{
    /// <summary>
    /// Handles coin collection logic and score addition.
    /// </summary>
    public class Collectible : MonoBehaviour
    {
        #region Collision Detection

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Check if collision object or its parent has RagdollController
            RagdollController ragdoll = collision.GetComponent<RagdollController>();
            if (ragdoll == null)
            {
                ragdoll = collision.GetComponentInParent<RagdollController>();
            }
            
            if (ragdoll == null) return;
            
            int coinValue = GameManager.Instance.Config.coinValue;
            GameManager.Instance.AddScore(coinValue);
            
            Destroy(gameObject);
        }

        #endregion ==================================================================
    }
}