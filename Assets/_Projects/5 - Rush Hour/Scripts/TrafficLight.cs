using UnityEngine;
using TMPro;

namespace Devdy.RushHour
{
    /// <summary>
    /// Controls traffic light behavior with red/green states and countdown timer.
    /// Displays time remaining and warning effects when about to change.
    /// </summary>
    public class TrafficLight : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Traffic Light Settings")]
        [SerializeField] private float laneRow = 2f;
        [SerializeField] private float laneHeight = 1f;
        [SerializeField] private bool startAsGreen = true;

        [Header("Timing")]
        [SerializeField] private float minDuration = 4f;
        [SerializeField] private float maxDuration = 6f;

        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer lightSprite;
        [SerializeField] private SpriteRenderer backgroundSprite;
        [SerializeField] private SpriteRenderer warningRing;
        [SerializeField] private TextMeshPro countdownText;

        [Header("Colors")]
        [SerializeField] private Color redColor = Color.red;
        [SerializeField] private Color greenColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color backgroundColor = new Color(0.17f, 0.24f, 0.31f);
        #endregion

        #region Private Fields
        private bool isRed;
        private float currentTimer;
        private float maxTimer;
        #endregion

        #region Constants
        private const float WARNING_THRESHOLD = 1f;
        private const float WARNING_PULSE_SPEED = 10f;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            InitializeTrafficLight();
        }

        private void Update()
        {
            UpdateTimer();
            UpdateVisuals();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes traffic light state and timer.
        /// </summary>
        private void InitializeTrafficLight()
        {
            isRed = !startAsGreen;
            maxTimer = Random.Range(minDuration, maxDuration);
            currentTimer = maxTimer;

            if (backgroundSprite != null)
            {
                backgroundSprite.color = backgroundColor;
            }

            if (warningRing != null)
            {
                warningRing.enabled = false;
            }

            UpdateVisuals();
        }
        #endregion

        #region Timer
        /// <summary>
        /// Updates countdown timer and switches state when timer expires.
        /// </summary>
        private void UpdateTimer()
        {
            currentTimer -= Time.deltaTime;

            if (currentTimer <= 0f)
            {
                SwitchState();
            }
        }

        /// <summary>
        /// Switches traffic light between red and green states.
        /// </summary>
        private void SwitchState()
        {
            isRed = !isRed;
            maxTimer = Random.Range(minDuration, maxDuration);
            currentTimer = maxTimer;
        }
        #endregion

        #region Visuals
        /// <summary>
        /// Updates all visual components based on current state.
        /// </summary>
        private void UpdateVisuals()
        {
            UpdateLightColor();
            UpdateCountdownDisplay();
            UpdateWarningEffect();
        }

        /// <summary>
        /// Updates the main light color based on state.
        /// </summary>
        private void UpdateLightColor()
        {
            if (lightSprite == null) return;

            lightSprite.color = isRed ? redColor : greenColor;
        }

        /// <summary>
        /// Updates countdown text display.
        /// </summary>
        private void UpdateCountdownDisplay()
        {
            if (countdownText == null) return;

            int secondsRemaining = Mathf.CeilToInt(currentTimer);
            countdownText.text = secondsRemaining.ToString();
        }

        /// <summary>
        /// Updates warning effect when about to change state.
        /// </summary>
        private void UpdateWarningEffect()
        {
            if (warningRing == null) return;

            if (currentTimer < WARNING_THRESHOLD)
            {
                warningRing.enabled = true;
                warningRing.color = warningColor;

                float alpha = 0.5f + Mathf.Sin(Time.time * WARNING_PULSE_SPEED) * 0.5f;
                Color pulsedColor = warningColor;
                pulsedColor.a = alpha;
                warningRing.color = pulsedColor;
            }
            else
            {
                warningRing.enabled = false;
            }
        }
        #endregion

        #region State Queries
        public bool IsRed() => isRed;
        public bool IsGreen() => !isRed;
        public bool IsAboutToChange() => currentTimer < WARNING_THRESHOLD;
        public float GetLaneRow() => laneRow;
        public float GetLaneHeight() => laneHeight;
        #endregion

        #region Configuration
        /// <summary>
        /// Sets the lane row and height for this traffic light.
        /// </summary>
        public void SetLanePosition(float row, float height)
        {
            laneRow = row;
            laneHeight = height;
        }

        /// <summary>
        /// Forces traffic light to specific state.
        /// </summary>
        public void ForceState(bool shouldBeRed)
        {
            isRed = shouldBeRed;
            currentTimer = maxTimer;
            UpdateVisuals();
        }
        #endregion
    }
}