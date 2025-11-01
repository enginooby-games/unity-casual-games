using UnityEngine;

namespace Devdy.FallingCatch
{
    /// <summary>
    /// Represents a falling object that can be good (collect) or bad (avoid).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class FallingObject : MonoBehaviour
    {
        #region Fields
        private bool isGood; // True = good object, False = bad object
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        #endregion

        #region Initialization
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(bool good)
        {
            isGood = good;
            spriteRenderer.color = isGood ? GameManager.Instance.Config.goodColor : GameManager.Instance.Config.badColor;
            rb.linearVelocity = Vector2.down * SROptions.Current.FallingCatch_FallSpeed;
        }
        #endregion

        #region Collision
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player")) return;

            if (isGood)
            {
                GameManager.Instance.AddScore(GameManager.Instance.Config.goodObjectScore);
            }
            else
            {
                GameManager.Instance.AddScore(GameManager.Instance.Config.badObjectPenalty);
                GameManager.Instance.TakeDamage(GameManager.Instance.Config.healthLoss);
            }

            SpawnManager.Instance.RemoveObject(gameObject);
            Destroy(gameObject);
        }
        #endregion
    }
}