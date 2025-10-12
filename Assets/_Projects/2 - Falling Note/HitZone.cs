using TMPro;
using UnityEngine;

namespace Project2
{
// Quản lý vùng hit để phát hiện khi nhấn nút
public class HitZone : MonoBehaviour
{
    public int noteIndex;
    public TextMeshProUGUI feedback;
    public bool enableKeyboardInput = true; // Toggle keyboard input
    private CircleCollider2D col;
    private bool isPressed = false;

    void Start()
    {
        col = GetComponent<CircleCollider2D>();
    }

    void Update()
    {
        DetectInput();
    }

    void DetectInput()
    {
        bool touchDetected = false;

        // Keyboard input - number keys 1-9 and 0
        if (enableKeyboardInput && Input.anyKeyDown)
        {
            if (CheckKeyboardInput())
            {
                touchDetected = true;
            }
        }

        // Desktop - mouse click
        if (Input.GetMouseButtonDown(0))
        {
            touchDetected = CheckTouchOnZone(Input.mousePosition);
        }

        // Mobile - touch
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touchDetected = CheckTouchOnZone(touch.position);
            }
        }

        if (touchDetected && !isPressed)
        {
            isPressed = true;
            CheckHit();
        }

        if (Input.GetMouseButtonUp(0) || (Input.touchCount == 0))
        {
            isPressed = false;
        }
    }

    bool CheckKeyboardInput()
    {
        // Map number keys 1-9, 0 to lane indices 0-9
        KeyCode[] numberKeys = new KeyCode[]
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
            KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0
        };

        for (int i = 0; i < numberKeys.Length; i++)
        {
            if (Input.GetKeyDown(numberKeys[i]))
            {
                // Alpha1 = lane 0, Alpha2 = lane 1, etc.
                // Alpha0 = lane 9
                int targetLane = (i == 9) ? 9 : i;
                return targetLane == noteIndex;
            }
        }

        return false;
    }

    bool CheckTouchOnZone(Vector3 screenPos)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        Collider2D hit = Physics2D.OverlapPoint(new Vector2(worldPos.x, worldPos.y));
        return hit != null && hit.gameObject == gameObject;
    }

    void CheckHit()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, col.radius);
        
        bool hitNote = false;
        foreach (Collider2D hit in hits)
        {
            FallingNote note = hit.GetComponent<FallingNote>();
            if (note != null && note.noteIndex == noteIndex)
            {
                GameManager.instance.AddScore(note.isPerfect ? 100 : 50);
                GameManager.instance.IncreaseCombo();
                PlayHitEffect();
                Destroy(hit.gameObject);
                hitNote = true;
                break;
            }
        }

        if (!hitNote)
        {
            GameManager.instance.ComboReset();
        }
    }

    void PlayHitEffect()
    {
        if (feedback != null)
        {
            feedback.text = "Perfect!";
            feedback.gameObject.SetActive(true);
            Invoke("HideFeedback", 0.3f);
        }
    }

    void HideFeedback()
    {
        if (feedback != null)
            feedback.gameObject.SetActive(false);
    }
}
}