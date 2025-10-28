using UnityEngine;

namespace Devdy.RagdollTumbler
{
    /// <summary>
    /// Handles player input and applies physics impulses to nudge the ragdoll left or right.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class RagdollController : MonoBehaviour
    {
        #region Fields
        [SerializeField] private Rigidbody2D torsoRb; // Torso rigidbody (for multi-part ragdoll)
        private Rigidbody2D rb; // Main rigidbody component
        
        #endregion ==================================================================

        #region Unity Lifecycle

        private void Awake()
        {
            // Use torso rigidbody if assigned (multi-part), otherwise use own rigidbody (simple)
            rb = torsoRb != null ? torsoRb : GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (!GameManager.Instance.IsGameActive) return;
            
            HandleInput();
        }

        #endregion ==================================================================

        #region Input Handling

        private void HandleInput()
        {
            // Keyboard input
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                ApplyNudge(Vector2.left);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                ApplyNudge(Vector2.right);
            }
            
            // Touch/Mouse input
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = touchPos.x < transform.position.x ? Vector2.left : Vector2.right;
                ApplyNudge(direction);
            }
        }

        private void ApplyNudge(Vector2 direction)
        {
            float force = GameManager.Instance.Config.tumbleForce;
            rb.AddForce(direction * force, ForceMode2D.Impulse);
        }

        #endregion ==================================================================
    }
}