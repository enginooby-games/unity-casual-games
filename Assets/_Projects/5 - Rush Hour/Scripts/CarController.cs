using UnityEngine;

namespace Devdy.RushHour
{
    /// <summary>
    /// Controls individual car movement, stopping at traffic lights,
    /// and destruction when off-screen.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class CarController : MonoBehaviour
    {
        #region Private Fields
        private float baseSpeed;
        private int direction; // 1 for right, -1 for left
        private float laneRow;
        private Vector2 carSize;
        private Color carColor;
        private float currentSpeed;
        private bool isStopped;
        private SpriteRenderer spriteRenderer;
        private BoxCollider2D boxCollider;
        private TrafficLight assignedTrafficLight;
        #endregion

        #region Constants
        private const float DESTROY_DISTANCE = 6.5f;
        private const float SLOWMO_MULTIPLIER = 0.4f;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            UpdateMovement();
            CheckDestruction();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes car with specific properties.
        /// </summary>
        public void Initialize(float speed, int dir, float row, Color color, Vector2 size)
        {
            baseSpeed = speed;
            direction = dir;
            laneRow = row;
            carColor = color;
            carSize = size;
            currentSpeed = baseSpeed * direction;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = carColor;
            }

            if (boxCollider != null)
            {
                // boxCollider.size = carSize;
                boxCollider.isTrigger = true;
            }

            gameObject.tag = "Car";
            var scale = transform.localScale;
            transform.localScale = new Vector3(scale.x * dir, scale.y, scale.z);
        }
        #endregion

        #region Movement
        /// <summary>
        /// Updates car position based on current speed and traffic light state.
        /// Uses early return pattern for cleaner code.
        /// </summary>
        private void UpdateMovement()
        {
            UpdateStoppedState();

            if (isStopped) return;

            float speedMultiplier = PowerupManager.Instance != null && 
                                  PowerupManager.Instance.IsSlowMoActive 
                                  ? SLOWMO_MULTIPLIER : 1f;

            transform.Translate(Vector3.right * currentSpeed * speedMultiplier * Time.deltaTime);
        }

        /// <summary>
        /// Updates whether car should be stopped based on traffic light.
        /// </summary>
        private void UpdateStoppedState()
        {
            if (assignedTrafficLight == null) return;

            isStopped = assignedTrafficLight.IsRed();
        }

        /// <summary>
        /// Checks if car is far enough off-screen to be destroyed.
        /// </summary>
        private void CheckDestruction()
        {
            if (Mathf.Abs(transform.position.x) > DESTROY_DISTANCE)
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Traffic Light
        /// <summary>
        /// Assigns a traffic light for this car to obey.
        /// </summary>
        public void AssignTrafficLight(TrafficLight trafficLight)
        {
            assignedTrafficLight = trafficLight;
        }

        public float GetLaneRow() => laneRow;
        #endregion
    }
}