using UnityEngine;

namespace EyeShooter
{
    /// <summary>
    /// Represents an enemy target that can be shot by the player.
    /// Tracks lifetime for reaction time scoring and handles destruction.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        /// <summary>
        /// Duration enemy remains active before auto-destruction (seconds)
        /// </summary>
        private const float MAX_LIFETIME = 5f;

        /// <summary>
        /// Fade duration during destruction animation (seconds)
        /// </summary>
        private const float FADE_OUT_DURATION = 0.2f;

        /// <summary>
        /// Scale increase per second for pulse animation
        /// </summary>
        private const float PULSE_SPEED = 2f;

        /// <summary>
        /// Maximum scale multiplier for pulse animation
        /// </summary>
        private const float PULSE_MAX_SCALE = 1.2f;

        [Header("Visuals")]
        [SerializeField]
        [Tooltip("Color of the enemy sprite")]
        private Color enemyColor = Color.red;

        /// <summary>
        /// SpriteRenderer component for visual representation
        /// </summary>
        private SpriteRenderer spriteRenderer;

        /// <summary>
        /// Time elapsed since enemy spawned (milliseconds)
        /// </summary>
        private float lifetimeMs;

        /// <summary>
        /// Original scale of the enemy
        /// </summary>
        private Vector3 originalScale;

        /// <summary>
        /// Flag indicating if enemy is being destroyed
        /// </summary>
        private bool isDestroying;

        /// <summary>
        /// Timer for fade out animation
        /// </summary>
        private float fadeTimer;

        /// <summary>
        /// Initializes enemy components and appearance
        /// </summary>
        private void Start()
        {
            InitializeComponents();
            SetupAppearance();
        }

        /// <summary>
        /// Gets required component references
        /// </summary>
        private void InitializeComponents()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalScale = transform.localScale;
        }

        /// <summary>
        /// Sets up initial visual appearance
        /// </summary>
        private void SetupAppearance()
        {
            spriteRenderer.color = enemyColor;
        }

        /// <summary>
        /// Updates enemy state, lifetime, and animations
        /// </summary>
        private void Update()
        {
            if (isDestroying)
            {
                UpdateFadeOutAnimation();
            }
            else
            {
                UpdateLifetime();
                UpdatePulseAnimation();
                CheckAutoDestroy();
            }
        }

        /// <summary>
        /// Increments lifetime counter
        /// </summary>
        private void UpdateLifetime()
        {
            lifetimeMs += Time.deltaTime * 1000f;
        }

        /// <summary>
        /// Updates pulsing scale animation
        /// </summary>
        private void UpdatePulseAnimation()
        {
            float pulse = 1f + Mathf.Sin(Time.time * PULSE_SPEED) * (PULSE_MAX_SCALE - 1f) * 0.5f;
            transform.localScale = originalScale * pulse;
        }

        /// <summary>
        /// Checks if enemy should be auto-destroyed due to timeout
        /// </summary>
        private void CheckAutoDestroy()
        {
            if (lifetimeMs >= MAX_LIFETIME * 1000f)
            {
                Destroy();
            }
        }

        /// <summary>
        /// Updates fade out animation during destruction
        /// </summary>
        private void UpdateFadeOutAnimation()
        {
            fadeTimer += Time.deltaTime;
            float alpha = 1f - (fadeTimer / FADE_OUT_DURATION);

            Color currentColor = spriteRenderer.color;
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);

            float scale = Mathf.Lerp(1f, 1.5f, fadeTimer / FADE_OUT_DURATION);
            transform.localScale = originalScale * scale;

            if (fadeTimer >= FADE_OUT_DURATION)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Gets the time this enemy has been alive
        /// </summary>
        /// <returns>Lifetime in milliseconds</returns>
        public float GetLifetime()
        {
            return lifetimeMs;
        }

        /// <summary>
        /// Initiates destruction sequence with fade animation
        /// </summary>
        public void Destroy()
        {
            if (isDestroying) return;

            isDestroying = true;
            fadeTimer = 0f;
        }

        /// <summary>
        /// Checks if a point hits this enemy's collider
        /// </summary>
        /// <param name="point">World space position to check</param>
        /// <returns>True if point is inside enemy bounds</returns>
        public bool CheckHit(Vector2 point)
        {
            if (isDestroying) return false;

            Collider2D collider = GetComponent<Collider2D>();
            if (collider == null) return false;

            return collider.OverlapPoint(point);
        }
    }
}