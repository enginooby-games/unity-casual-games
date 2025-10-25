using UnityEngine;
using TMPro;

namespace Devdy.RushHour
{
    /// <summary>
    /// Represents a collectible coin that awards points and currency.
    /// Supports both regular and bonus coins with different values.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class Coin : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Coin Properties")]
        [SerializeField] private int coinValue = 1;
        [SerializeField] private bool isBonusCoin = false;

        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer coinSprite;
        [SerializeField] private TextMeshPro valueText;
        [SerializeField] private Color normalCoinColor = Color.yellow;
        [SerializeField] private Color bonusCoinColor = Color.red;

        [Header("Animation")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseAmount = 0.2f;
        #endregion

        #region Private Fields
        private bool isCollected;
        private float baseScale;
        private CircleCollider2D coinCollider;
        private PlayerController player;
        #endregion

        #region Constants
        private const float COLLECTION_RANGE = 0.7f;
        private const float MAGNET_RANGE = 2f;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            coinCollider = GetComponent<CircleCollider2D>();
            coinCollider.isTrigger = true;
            baseScale = transform.localScale.x;
        }

        private void Start()
        {
            InitializeCoin();
            FindPlayer();
        }

        private void Update()
        {
            if (isCollected) return;

            AnimateCoin();
            CheckMagnetCollection();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes coin visual appearance based on type.
        /// </summary>
        private void InitializeCoin()
        {
            if (coinSprite != null)
            {
                coinSprite.color = isBonusCoin ? bonusCoinColor : normalCoinColor;
            }

            if (valueText != null)
            {
                if (isBonusCoin && coinValue > 1)
                {
                    valueText.text = $"x{coinValue}";
                    valueText.gameObject.SetActive(true);
                }
                else
                {
                    valueText.gameObject.SetActive(false);
                }
            }

            gameObject.tag = "Coin";
        }

        /// <summary>
        /// Finds player reference for magnet calculations.
        /// </summary>
        private void FindPlayer()
        {
            player = FindObjectOfType<PlayerController>();
        }
        #endregion

        #region Animation
        /// <summary>
        /// Animates coin with rotation and pulsing scale.
        /// </summary>
        private void AnimateCoin()
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

            if (isBonusCoin)
            {
                float scale = baseScale + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                transform.localScale = Vector3.one * scale;
            }
        }
        #endregion

        #region Collection
        /// <summary>
        /// Checks if player with magnet is close enough to collect coin.
        /// Uses early return pattern.
        /// </summary>
        private void CheckMagnetCollection()
        {
            if (player == null) return;

            float collectionRange = player.HasMagnet() ? MAGNET_RANGE : COLLECTION_RANGE;
            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance < collectionRange)
            {
                Collect();
            }
        }

        /// <summary>
        /// Handles coin collection and awards value to player.
        /// </summary>
        public void Collect()
        {
            if (isCollected) return;

            isCollected = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(coinValue);
            }

            Destroy(gameObject);
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Configures coin as a bonus coin with specific value.
        /// </summary>
        public void SetBonusCoin(int value)
        {
            isBonusCoin = true;
            coinValue = value;
            InitializeCoin();
        }
        #endregion
    }
}