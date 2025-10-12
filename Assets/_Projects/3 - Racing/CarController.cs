using UnityEngine;
using UnityEngine.UI;

namespace Project3
{
    // Main Car Controller Script
    public class CarController : MonoBehaviour
    {
        [Header("Car Settings")] public float acceleration = 30f;
        public float maxSpeed = 50f;
        public float turnSpeed = 200f;
        public float drag = 3f;

        [Header("AI Settings")] public bool isAI = false;
        public Transform[] waypoints;
        private int currentWaypoint = 0;

        private Rigidbody2D rb;
        private float currentSpeed = 0f;
        private float turnInput = 0f;
        private float accelerationInput = 0f;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.linearDamping = drag;
        }

        void Update()
        {
            if (isAI)
            {
                HandleAI();
            }
            else
            {
                HandleInput();
            }
        }

        void FixedUpdate()
        {
            // Apply acceleration
            currentSpeed += accelerationInput * acceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * 0.5f, maxSpeed);

            // Apply natural deceleration
            currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.fixedDeltaTime * drag);

            // Move forward
            rb.linearVelocity = transform.up * currentSpeed;

            // Turn the car
            if (Mathf.Abs(currentSpeed) > 0.1f)
            {
                float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
                transform.Rotate(0, 0, -turn * (currentSpeed / maxSpeed));
            }
        }

        void HandleInput()
        {
            accelerationInput = Input.GetAxis("Vertical");
            turnInput = Input.GetAxis("Horizontal");
        }

        void HandleAI()
        {
            if (waypoints.Length == 0) return;

            // Navigate to waypoint
            Vector2 direction = (waypoints[currentWaypoint].position - transform.position).normalized;
            float angle = Vector2.SignedAngle(transform.up, direction);

            // Steering
            turnInput = Mathf.Clamp(angle / 45f, -1f, 1f);

            // Acceleration
            accelerationInput = 1f;

            // Check if reached waypoint
            if (Vector2.Distance(transform.position, waypoints[currentWaypoint].position) < 5f)
            {
                currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            }
        }
    }
}