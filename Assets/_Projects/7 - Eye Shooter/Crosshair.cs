using UnityEngine;

namespace EyeShooter
{
    /// <summary>
    /// Controls the crosshair visual element and position based on eye tracking.
    /// Handles smooth movement, visual feedback, and shoot effects.
    /// </summary>
    public class Crosshair : MonoBehaviour
    {
        /// <summary>
        /// Duration of shoot flash effect in seconds
        /// </summary>
        private const float SHOOT_FLASH_DURATION = 0.1f;

        /// <summary>
        /// Scale multiplier for shoot animation
        /// </summary>
        private const float SHOOT_SCALE_MULTIPLIER = 1.3f;

        /// <summary>
        /// Speed of smooth movement interpolation
        /// </summary>
        private const float MOVEMENT_SMOOTHING = 0.15f;

        [Header("Appearance")]
        [SerializeField]
        [Tooltip("Default color of the crosshair")]
        private Color normalColor = Color.white;

        [SerializeField]
        [Tooltip("Color when shooting")]
        private Color shootColor = Color.red;

        [SerializeField]
        [Tooltip("Size of the crosshair")]
        private float crosshairSize = 50f;

        [Header("Movement")]
        [SerializeField]
        [Tooltip("Percentage of screen boundaries to use (0-1)")]
        private Vector2 screenBoundaryPercent = new Vector2(0.8f, 0.8f);

        public AudioSource shootAudioSource;

        /// <summary>
        /// SpriteRenderer for crosshair visual
        /// </summary>
        private SpriteRenderer spriteRenderer;

        /// <summary>
        /// Camera used for screen-to-world conversion
        /// </summary>
        private Camera mainCamera;

        /// <summary>
        /// Target position for smooth movement
        /// </summary>
        private Vector3 targetPosition;

        /// <summary>
        /// Original scale of the crosshair
        /// </summary>
        private Vector3 originalScale;

        /// <summary>
        /// Remaining time for shoot flash effect
        /// </summary>
        private float shootFlashTimer;

        /// <summary>
        /// Initializes crosshair components and appearance
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
            
            if (spriteRenderer == null)
            {
                Debug.LogError("Crosshair requires a SpriteRenderer component!");
            }

            mainCamera = Camera.main;
            
            if (mainCamera == null)
            {
                Debug.LogError("No main camera found in scene!");
            }

            targetPosition = transform.position;
        }

        /// <summary>
        /// Sets up initial visual appearance
        /// </summary>
        private void SetupAppearance()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }

            transform.localScale = Vector3.one * crosshairSize * 0.01f;
            originalScale = transform.localScale;
        }

        /// <summary>
        /// Updates crosshair position and effects each frame
        /// </summary>
        private void Update()
        {
            SmoothMoveToTarget();
            UpdateShootEffect();
        }

        /// <summary>
        /// Smoothly interpolates crosshair position to target
        /// </summary>
        private void SmoothMoveToTarget()
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                MOVEMENT_SMOOTHING
            );
        }

        /// <summary>
        /// Updates visual effects for shooting animation
        /// </summary>
        private void UpdateShootEffect()
        {
            if (shootFlashTimer > 0)
            {
                shootFlashTimer -= Time.deltaTime;

                if (shootFlashTimer <= 0)
                {
                    ResetToNormalAppearance();
                }
            }
        }

        /// <summary>
        /// Updates crosshair position based on normalized eye position
        /// </summary>
        /// <param name="normalizedPosition">Eye position in 0-1 range</param>
        public void UpdatePosition(Vector2 normalizedPosition)
        {
            if (mainCamera == null) return;

            Vector3 worldPosition = ConvertNormalizedToWorldPosition(normalizedPosition);
            Vector3 clampedPosition = ClampToScreenBoundaries(worldPosition);
            targetPosition = clampedPosition;
        }

        /// <summary>
        /// Converts normalized position (0-1) to world space position
        /// </summary>
        /// <param name="normalized">Normalized position</param>
        /// <returns>World space position</returns>
        private Vector3 ConvertNormalizedToWorldPosition(Vector2 normalized)
        {
            float screenX = normalized.x * Screen.width;
            float screenY = (1f - normalized.y) * Screen.height;

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(screenX, screenY, mainCamera.nearClipPlane + 10f)
            );

            return new Vector3(worldPos.x, worldPos.y, 0f);
        }

        /// <summary>
        /// Clamps position to stay within screen boundaries
        /// </summary>
        /// <param name="position">Unclamped world position</param>
        /// <returns>Clamped world position</returns>
        private Vector3 ClampToScreenBoundaries(Vector3 position)
        {
            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(
                (1f - screenBoundaryPercent.x) * 0.5f,
                (1f - screenBoundaryPercent.y) * 0.5f,
                mainCamera.nearClipPlane + 10f
            ));

            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(
                1f - (1f - screenBoundaryPercent.x) * 0.5f,
                1f - (1f - screenBoundaryPercent.y) * 0.5f,
                mainCamera.nearClipPlane + 10f
            ));

            return new Vector3(
                Mathf.Clamp(position.x, bottomLeft.x, topRight.x),
                Mathf.Clamp(position.y, bottomLeft.y, topRight.y),
                position.z
            );
        }

        /// <summary>
        /// Triggers visual feedback for shooting action
        /// </summary>
        public void PlayShootEffect()
        {
            shootFlashTimer = SHOOT_FLASH_DURATION;
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = shootColor;
            }
            
            transform.localScale = originalScale * SHOOT_SCALE_MULTIPLIER;
            shootAudioSource.Play();
        }

        /// <summary>
        /// Resets crosshair to normal appearance after shoot effect
        /// </summary>
        private void ResetToNormalAppearance()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
            
            transform.localScale = originalScale;
        }

        /// <summary>
        /// Gets current world position of crosshair
        /// </summary>
        /// <returns>World space position</returns>
        public Vector2 GetWorldPosition()
        {
            return transform.position;
        }

        /// <summary>
        /// Visualizes screen boundaries in Scene view
        /// </summary>
        private void OnDrawGizmos()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            Gizmos.color = Color.yellow;

            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(
                (1f - screenBoundaryPercent.x) * 0.5f,
                (1f - screenBoundaryPercent.y) * 0.5f,
                10f
            ));

            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(
                1f - (1f - screenBoundaryPercent.x) * 0.5f,
                1f - (1f - screenBoundaryPercent.y) * 0.5f,
                10f
            ));

            Vector3 bottomLeftFlat = new Vector3(bottomLeft.x, bottomLeft.y, 0);
            Vector3 bottomRightFlat = new Vector3(topRight.x, bottomLeft.y, 0);
            Vector3 topRightFlat = new Vector3(topRight.x, topRight.y, 0);
            Vector3 topLeftFlat = new Vector3(bottomLeft.x, topRight.y, 0);

            Gizmos.DrawLine(bottomLeftFlat, bottomRightFlat);
            Gizmos.DrawLine(bottomRightFlat, topRightFlat);
            Gizmos.DrawLine(topRightFlat, topLeftFlat);
            Gizmos.DrawLine(topLeftFlat, bottomLeftFlat);
        }
    }
}