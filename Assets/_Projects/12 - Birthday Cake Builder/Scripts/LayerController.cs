using UnityEngine;

namespace Devdy.BirthdayCake
{
    /// <summary>
    /// Controls individual cake layer physics and drop behavior.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class LayerController : MonoBehaviour
    {
        public LayerData Data { get; private set; }
        public bool IsDropped { get; private set; }

        private Rigidbody2D rb;
        private BoxCollider2D boxCollider;
        private Vector3 moveDirection = Vector3.right;
        private float moveSpeed;

        private const float MOVE_RANGE = 3f; // Horizontal movement range
        private const float ANGULAR_DRAG = 5f;
        private const float COLLISION_CHECK_DELAY = 0.5f;

        private float dropTime;
        private bool hasCollided;

        #region ==================================================================== Initialization

        /// <summary>
        /// Initializes the layer with data and movement speed.
        /// </summary>
        public void Initialize(LayerData layerData, float speed, float sizeMultiplier = 1f)
        {
            Data = layerData;
            moveSpeed = speed;

            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            
            var renderer = GetComponent<SpriteRenderer>();
            renderer.sortingOrder = GameManager.Instance.CurrentLayer + 1;

            // Setup physics
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.angularDamping = ANGULAR_DRAG;
            rb.mass = layerData.Mass;

            // Setup collider with size multiplier
            boxCollider.size = layerData.Size;

            // Apply size multiplier to make layers progressively smaller
            Vector3 currentScale = transform.localScale;
            // transform.localScale = new Vector3(currentScale.x * sizeMultiplier, currentScale.y * 1.3f, currentScale.z);
            var scaleX = Mathf.Clamp(currentScale.x * sizeMultiplier, 1f, 5f);
            var scaleY = Mathf.Clamp(currentScale.y * sizeMultiplier, 1f, 2f);
            transform.localScale = new Vector3(scaleX, scaleY, currentScale.z);

            IsDropped = false;
            hasCollided = false;
        }

        #endregion ==================================================================

        #region ==================================================================== Movement & Drop

        private void Update()
        {
            if (IsDropped) return;

            // Horizontal movement before drop
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // Bounce at edges
            if (Mathf.Abs(transform.position.x) > MOVE_RANGE)
            {
                moveDirection = -moveDirection;
            }
        }

        /// <summary>
        /// Drops the layer and enables physics.
        /// </summary>
        public void Drop()
        {
            if (IsDropped) return;

            IsDropped = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            dropTime = Time.time;

            AudioManager.Instance.PlaySound("drop");
        }

        #endregion ==================================================================

        #region ==================================================================== Collision Detection

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsDropped || hasCollided) return;
            if (Time.time - dropTime < COLLISION_CHECK_DELAY) return;

            hasCollided = true;
            GameManager.Instance.OnCakeDropped();

            // Check if collision is with another layer
            LayerController otherLayer = collision.gameObject.GetComponent<LayerController>();
            if (otherLayer == null) return;

            // Calculate alignment offset
            float horizontalOffset = Mathf.Abs(transform.position.x - otherLayer.transform.position.x);
            float threshold = GameManager.Instance.Config.StabilityThreshold;

            // Check if misaligned beyond threshold
            if (horizontalOffset > threshold)
            {
                GameManager.Instance.OnCakeCollapsed();
                var x = Mathf.Sign(transform.position.x - otherLayer.transform.position.x) * 1;
                rb.AddForce(new Vector2(x, 1) * 100);
            }
        }

        #endregion ==================================================================
    }
}