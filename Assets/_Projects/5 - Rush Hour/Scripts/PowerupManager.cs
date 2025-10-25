using UnityEngine;
using UnityEngine.UI;

namespace Devdy.RushHour
{
    /// <summary>
    /// Manages all powerup purchases and activations.
    /// Handles shield, slow-motion, magnet, and teleport powerups with UI updates.
    /// Stores shared powerup state accessed by multiple systems.
    /// </summary>
    public class PowerupManager : MonoBehaviour
    {
        #region Singleton-like Access
        public static PowerupManager Instance { get; private set; }
        #endregion

        #region Shared Data - Accessed by Multiple Systems
        public bool IsShieldActive;
        public bool IsSlowMoActive;
        public bool IsMagnetActive;
        #endregion

        #region Inspector Fields
        [Header("Player Reference")]
        [SerializeField] private PlayerController player;

        [Header("UI Buttons")]
        [SerializeField] private Button shieldButton;
        [SerializeField] private Button slowmoButton;
        [SerializeField] private Button magnetButton;
        [SerializeField] private Button teleportButton;

        [Header("Cooldown Bars")]
        [SerializeField] private Image shieldCooldownBar;
        [SerializeField] private Image slowmoCooldownBar;
        [SerializeField] private Image magnetCooldownBar;

        [Header("Powerup Costs")]
        [SerializeField] private int shieldCost = 15;
        [SerializeField] private int slowmoCost = 10;
        [SerializeField] private int magnetCost = 8;
        [SerializeField] private int teleportCost = 12;
        #endregion

        #region Private Fields
        private float shieldTimeRemaining;
        private float slowmoTimeRemaining;
        private float magnetTimeRemaining;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializePowerups();
            SetupButtons();
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

            UpdatePowerups();
            UpdateUI();
            HandleKeyboardInput();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes powerup states and finds player if not assigned.
        /// </summary>
        private void InitializePowerups()
        {
            if (player == null)
            {
                player = FindObjectOfType<PlayerController>();
            }

            IsShieldActive = false;
            IsSlowMoActive = false;
            IsMagnetActive = false;

            shieldTimeRemaining = 0f;
            slowmoTimeRemaining = 0f;
            magnetTimeRemaining = 0f;
        }

        /// <summary>
        /// Sets up button click listeners.
        /// </summary>
        private void SetupButtons()
        {
            if (shieldButton != null)
            {
                shieldButton.onClick.AddListener(() => BuyPowerup(PowerupType.Shield));
            }

            if (slowmoButton != null)
            {
                slowmoButton.onClick.AddListener(() => BuyPowerup(PowerupType.SlowMo));
            }

            if (magnetButton != null)
            {
                magnetButton.onClick.AddListener(() => BuyPowerup(PowerupType.Magnet));
            }

            if (teleportButton != null)
            {
                teleportButton.onClick.AddListener(() => BuyPowerup(PowerupType.Teleport));
            }
        }

        /// <summary>
        /// Resets all powerup states without destroying GameObject.
        /// Called by GameManager.RestartGame().
        /// </summary>
        public void ResetPowerups()
        {
            IsShieldActive = false;
            IsSlowMoActive = false;
            IsMagnetActive = false;

            shieldTimeRemaining = 0f;
            slowmoTimeRemaining = 0f;
            magnetTimeRemaining = 0f;

            if (player == null)
            {
                player = FindObjectOfType<PlayerController>();
            }
        }
        #endregion

        #region Powerup Purchase
        /// <summary>
        /// Attempts to purchase and activate a powerup.
        /// Uses early return pattern.
        /// </summary>
        public void BuyPowerup(PowerupType type)
        {
            if (GameManager.Instance == null || player == null) return;

            int cost = GetPowerupCost(type);

            if (!GameManager.Instance.SpendCoins(cost)) return;

            ActivatePowerup(type);
        }

        /// <summary>
        /// Gets cost for specific powerup type.
        /// </summary>
        private int GetPowerupCost(PowerupType type)
        {
            switch (type)
            {
                case PowerupType.Shield: return shieldCost;
                case PowerupType.SlowMo: return slowmoCost;
                case PowerupType.Magnet: return magnetCost;
                case PowerupType.Teleport: return teleportCost;
                default: return 0;
            }
        }
        #endregion

        #region Powerup Activation
        /// <summary>
        /// Activates specified powerup with effects.
        /// </summary>
        private void ActivatePowerup(PowerupType type)
        {
            switch (type)
            {
                case PowerupType.Shield:
                    ActivateShield();
                    break;
                case PowerupType.SlowMo:
                    ActivateSlowMo();
                    break;
                case PowerupType.Magnet:
                    ActivateMagnet();
                    break;
                case PowerupType.Teleport:
                    ActivateTeleport();
                    break;
            }
        }

        /// <summary>
        /// Activates shield powerup with duration from SROptions.
        /// </summary>
        private void ActivateShield()
        {
            if (IsShieldActive) return;

            float duration = SROptions.Current.RushHour_ShieldDuration;
            IsShieldActive = true;
            shieldTimeRemaining = duration;

            if (player != null)
            {
                player.ActivateShield(duration);
            }
        }

        /// <summary>
        /// Activates slow motion powerup with duration from SROptions.
        /// </summary>
        private void ActivateSlowMo()
        {
            if (IsSlowMoActive) return;

            float duration = SROptions.Current.RushHour_SlowMoDuration;
            IsSlowMoActive = true;
            slowmoTimeRemaining = duration;
        }

        /// <summary>
        /// Activates magnet powerup with duration from SROptions.
        /// </summary>
        private void ActivateMagnet()
        {
            if (IsMagnetActive) return;

            float duration = SROptions.Current.RushHour_MagnetDuration;
            IsMagnetActive = true;
            magnetTimeRemaining = duration;

            if (player != null)
            {
                player.ActivateMagnet(duration);
            }
        }

        /// <summary>
        /// Activates teleport powerup (instant effect).
        /// </summary>
        private void ActivateTeleport()
        {
            if (player != null)
            {
                player.TeleportToGoal();
            }
        }
        #endregion

        #region Powerup Updates
        /// <summary>
        /// Updates all active powerup timers.
        /// </summary>
        private void UpdatePowerups()
        {
            UpdateShieldTimer();
            UpdateSlowMoTimer();
            UpdateMagnetTimer();
        }

        /// <summary>
        /// Updates shield timer and deactivates when expired.
        /// </summary>
        private void UpdateShieldTimer()
        {
            if (!IsShieldActive || shieldTimeRemaining <= 0f) return;

            shieldTimeRemaining -= Time.deltaTime;

            if (shieldTimeRemaining <= 0f)
            {
                IsShieldActive = false;
                shieldTimeRemaining = 0f;
            }
        }

        /// <summary>
        /// Updates slow motion timer and deactivates when expired.
        /// </summary>
        private void UpdateSlowMoTimer()
        {
            if (!IsSlowMoActive || slowmoTimeRemaining <= 0f) return;

            slowmoTimeRemaining -= Time.deltaTime;

            if (slowmoTimeRemaining <= 0f)
            {
                IsSlowMoActive = false;
                slowmoTimeRemaining = 0f;
            }
        }

        /// <summary>
        /// Updates magnet timer and deactivates when expired.
        /// </summary>
        private void UpdateMagnetTimer()
        {
            if (!IsMagnetActive || magnetTimeRemaining <= 0f) return;

            magnetTimeRemaining -= Time.deltaTime;

            if (magnetTimeRemaining <= 0f)
            {
                IsMagnetActive = false;
                magnetTimeRemaining = 0f;
            }
        }
        #endregion

        #region UI Updates
        /// <summary>
        /// Updates all powerup UI elements.
        /// </summary>
        private void UpdateUI()
        {
            UpdateButtonStates();
            UpdateCooldownBars();
        }

        /// <summary>
        /// Updates button interactability based on cost and active state.
        /// </summary>
        private void UpdateButtonStates()
        {
            if (GameManager.Instance == null) return;

            int currentCoins = GameManager.Instance.CurrentCoins;

            if (shieldButton != null)
            {
                shieldButton.interactable = currentCoins >= shieldCost && !IsShieldActive;
            }

            if (slowmoButton != null)
            {
                slowmoButton.interactable = currentCoins >= slowmoCost && !IsSlowMoActive;
            }

            if (magnetButton != null)
            {
                magnetButton.interactable = currentCoins >= magnetCost && !IsMagnetActive;
            }

            if (teleportButton != null)
            {
                teleportButton.interactable = currentCoins >= teleportCost;
            }
        }

        /// <summary>
        /// Updates cooldown bar fill amounts.
        /// </summary>
        private void UpdateCooldownBars()
        {
            if (shieldCooldownBar != null)
            {
                float fillAmount = IsShieldActive ? 
                    (shieldTimeRemaining / SROptions.Current.RushHour_ShieldDuration) : 0f;
                shieldCooldownBar.fillAmount = fillAmount;
            }

            if (slowmoCooldownBar != null)
            {
                float fillAmount = IsSlowMoActive ? 
                    (slowmoTimeRemaining / SROptions.Current.RushHour_SlowMoDuration) : 0f;
                slowmoCooldownBar.fillAmount = fillAmount;
            }

            if (magnetCooldownBar != null)
            {
                float fillAmount = IsMagnetActive ? 
                    (magnetTimeRemaining / SROptions.Current.RushHour_MagnetDuration) : 0f;
                magnetCooldownBar.fillAmount = fillAmount;
            }
        }
        #endregion

        #region Input Handling
        /// <summary>
        /// Handles keyboard shortcuts for powerup activation (1-4 keys).
        /// </summary>
        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                BuyPowerup(PowerupType.Shield);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                BuyPowerup(PowerupType.SlowMo);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                BuyPowerup(PowerupType.Magnet);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                BuyPowerup(PowerupType.Teleport);
            }
        }
        #endregion
    }

    /// <summary>
    /// Enum defining available powerup types.
    /// </summary>
    public enum PowerupType
    {
        Shield,
        SlowMo,
        Magnet,
        Teleport
    }
}