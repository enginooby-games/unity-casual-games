using UnityEngine;

namespace Devdy.RushHour
{
    /// <summary>
    /// Controls player movement in a grid-based system with snap movement.
    /// Handles input, collision detection, and respawn logic.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 10f; // Visual movement speed

        [Header("Grid Settings")]
        [SerializeField] private float gridSize = 1f;
        [SerializeField] private Vector2Int startGridPosition = new Vector2Int(8, 0); // Bottom of screen
        [SerializeField] private Vector2Int goalGridPosition = new Vector2Int(8, 13); // Top of screen

        [Header("Visual Settings")]
        [SerializeField] private Color playerColor = Color.blue;
        [SerializeField] private GameObject shieldEffect; // Visual effect for shield
        [SerializeField] private GameObject magnetEffect; // Visual effect for magnet
        #endregion

        #region Private Fields
        private Vector2Int currentGridPosition;
        private Vector3 targetWorldPosition;
        private float lastMoveTime;
        private bool isMoving;
        private SpriteRenderer spriteRenderer;
        private bool hasShield;
        private bool hasMagnet;
        #endregion

        #region Constants
        private const int GRID_MIN_X = 0;
        private const int GRID_MAX_X = 12;
        private const int GRID_MIN_Y = 0;
        private const int GRID_MAX_Y = 14;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = playerColor;
            }
        }

        private void Start()
        {
            InitializePlayer();
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return;
            }

            HandleInput();
            UpdateMovement();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes player position and state.
        /// </summary>
        private void InitializePlayer()
        {
            currentGridPosition = startGridPosition;
            targetWorldPosition = GridToWorldPosition(currentGridPosition);
            transform.position = targetWorldPosition;
            lastMoveTime = -SROptions.Current.RushHour_MoveDelay;
            isMoving = false;

            if (shieldEffect != null)
            {
                shieldEffect.SetActive(false);
            }

            if (magnetEffect != null)
            {
                magnetEffect.SetActive(false);
            }
        }

        /// <summary>
        /// Resets player to initial state without destroying GameObject.
        /// Called by GameManager.RestartGame().
        /// </summary>
        public void ResetPlayer()
        {
            hasShield = false;
            hasMagnet = false;
            InitializePlayer();
        }
        #endregion

        #region Input Handling
        /// <summary>
        /// Handles keyboard input for player movement using early return pattern.
        /// </summary>
        private void HandleInput()
        {
            float moveDelay = SROptions.Current.RushHour_MoveDelay;
            if (Time.time - lastMoveTime < moveDelay || isMoving) return;

            Vector2Int moveDirection = Vector2Int.zero;

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                moveDirection = Vector2Int.up;
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                moveDirection = Vector2Int.down;
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                moveDirection = Vector2Int.left;
            }
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                moveDirection = Vector2Int.right;
            }

            if (moveDirection == Vector2Int.zero) return;

            TryMove(moveDirection);
        }
        #endregion

        #region Movement
        /// <summary>
        /// Attempts to move player in specified direction if valid.
        /// Uses early return to reduce nesting.
        /// </summary>
        private void TryMove(Vector2Int direction)
        {
            Vector2Int newGridPosition = currentGridPosition + direction;

            if (!IsValidGridPosition(newGridPosition)) return;

            currentGridPosition = newGridPosition;
            targetWorldPosition = GridToWorldPosition(currentGridPosition);
            lastMoveTime = Time.time;
            isMoving = true;

            CheckGoalReached();
        }

        /// <summary>
        /// Smoothly moves player towards target position.
        /// </summary>
        private void UpdateMovement()
        {
            if (!isMoving) return;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetWorldPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetWorldPosition) < 0.01f)
            {
                transform.position = targetWorldPosition;
                isMoving = false;
            }
        }

        /// <summary>
        /// Checks if grid position is within valid bounds.
        /// </summary>
        private bool IsValidGridPosition(Vector2Int gridPos)
        {
            return gridPos.x >= GRID_MIN_X && gridPos.x <= GRID_MAX_X &&
                   gridPos.y >= GRID_MIN_Y && gridPos.y <= GRID_MAX_Y;
        }

        /// <summary>
        /// Converts grid coordinates to world position.
        /// </summary>
        private Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            return new Vector3(
                (gridPos.x - GRID_MAX_X / 2f) * gridSize,
                (gridPos.y - GRID_MAX_Y / 2f) * gridSize,
                0f
            );
        }
        #endregion

        #region Goal & Respawn
        /// <summary>
        /// Checks if player reached the goal and triggers level completion.
        /// </summary>
        private void CheckGoalReached()
        {
            if (currentGridPosition.y != goalGridPosition.y) return;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel();
            }
            RespawnPlayer();
        }

        /// <summary>
        /// Respawns player at starting position.
        /// </summary>
        public void RespawnPlayer()
        {
            currentGridPosition = startGridPosition;
            targetWorldPosition = GridToWorldPosition(currentGridPosition);
            transform.position = targetWorldPosition;
            isMoving = false;
        }

        /// <summary>
        /// Teleports player to goal position for teleport powerup.
        /// </summary>
        public void TeleportToGoal()
        {
            currentGridPosition = goalGridPosition;
            targetWorldPosition = GridToWorldPosition(currentGridPosition);
            transform.position = targetWorldPosition;
            isMoving = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel();
            }

            Invoke(nameof(RespawnPlayer), 0.1f);
        }
        #endregion

        #region Collision
        /// <summary>
        /// Handles collision with cars and obstacles.
        /// Uses early return pattern.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasShield) return;

            if (other.CompareTag("Car") || other.CompareTag("Obstacle"))
            {
                HandleCollision();
            }
            else if (other.CompareTag("Coin"))
            {
                HandleCoinCollection(other.gameObject);
            }
        }

        /// <summary>
        /// Handles player collision with hazards.
        /// </summary>
        private void HandleCollision()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoseLife();
            }
            RespawnPlayer();
        }

        /// <summary>
        /// Handles coin collection logic.
        /// </summary>
        private void HandleCoinCollection(GameObject coinObject)
        {
            Coin coin = coinObject.GetComponent<Coin>();
            if (coin == null) return;

            coin.Collect();
        }
        #endregion

        #region Powerups
        /// <summary>
        /// Activates shield powerup effect.
        /// </summary>
        public void ActivateShield(float duration)
        {
            hasShield = true;
            if (shieldEffect != null)
            {
                shieldEffect.SetActive(true);
            }
            Invoke(nameof(DeactivateShield), duration);
        }

        private void DeactivateShield()
        {
            hasShield = false;
            if (shieldEffect != null)
            {
                shieldEffect.SetActive(false);
            }
        }

        /// <summary>
        /// Activates magnet powerup effect.
        /// </summary>
        public void ActivateMagnet(float duration)
        {
            hasMagnet = true;
            if (magnetEffect != null)
            {
                magnetEffect.SetActive(true);
            }
            Invoke(nameof(DeactivateMagnet), duration);
        }

        private void DeactivateMagnet()
        {
            hasMagnet = false;
            if (magnetEffect != null)
            {
                magnetEffect.SetActive(false);
            }
        }

        public bool HasShield() => hasShield;
        public bool HasMagnet() => hasMagnet;
        public Vector2Int GetGridPosition() => currentGridPosition;
        public float GetMagnetRange() => hasMagnet ? 2f : 0.7f;
        #endregion
    }
}