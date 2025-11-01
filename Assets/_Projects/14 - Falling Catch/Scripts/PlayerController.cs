using UnityEngine;

namespace Devdy.FallingCatch
{
    /// <summary>
    /// Controls player basket movement via mouse, touch, and keyboard input.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        #region Fields
        private Camera mainCamera;
        private float targetX; // Target X position
        private bool isDragging; // Mouse/touch drag state
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            mainCamera = Camera.main;
            targetX = transform.position.x;
        }

        private void Update()
        {
            if (!GameManager.Instance.IsGameActive) return;

            HandleKeyboardInput();
            HandleMouseInput();
            HandleTouchInput();
            MoveToTarget();
        }
        #endregion

        #region Input Handling
        private void HandleKeyboardInput()
        {
            float horizontal = Input.GetAxis("Horizontal"); // A/D or Arrow keys
            if (Mathf.Abs(horizontal) > 0.01f)
            {
                targetX += horizontal * SROptions.Current.FallingCatch_PlayerSpeed * Time.deltaTime;
                ClampTargetX();
            }
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                targetX = mouseWorldPos.x;
                ClampTargetX();
            }
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector3 touchWorldPos = mainCamera.ScreenToWorldPoint(touch.position);
                targetX = touchWorldPos.x;
                ClampTargetX();
            }
        }
        #endregion

        #region Movement
        private void MoveToTarget()
        {
            float currentX = transform.position.x;
            float newX = Mathf.MoveTowards(currentX, targetX, SROptions.Current.FallingCatch_PlayerSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        private void ClampTargetX()
        {
            targetX = Mathf.Clamp(targetX, GameManager.Instance.Config.minX, GameManager.Instance.Config.maxX);
        }
        #endregion
    }
}