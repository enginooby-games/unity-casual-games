using UnityEngine;

namespace Devdy.RagdollTumbler
{
    /// <summary>
    /// Handles hazard collision detection and triggers game over.
    /// </summary>
    public class Hazard : MonoBehaviour
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
            
            GameManager.Instance.GameOver();
        }

        #endregion ==================================================================
    }
}