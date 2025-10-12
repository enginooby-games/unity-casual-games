using TMPro;
using UnityEngine;

namespace Project2
{

// Quản lý các nút piano rơi
public class FallingNote : MonoBehaviour
{
    public float fallSpeed => SROptions.Current.P2_NoteSpeed;
    public int noteIndex;
    public bool isPerfect = false;
    private CircleCollider2D col;
    [SerializeField] private TextMeshPro keyText;

    void Start()
    {
        col = GetComponent<CircleCollider2D>();
        keyText.text = GetKeyNumberForLane(noteIndex);
    }

    string GetKeyNumberForLane(int lane)
    {
        // Map lane index to key number (0->1, 1->2, ..., 8->9, 9->0)
        if (lane == 9)
            return "0";
        else if (lane >= 0 && lane < 9)
            return (lane + 1).ToString();
        else
            return "";
    }

    void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        
        // Hủy nếu vượt quá màn hình
        if (transform.position.y < -6)
        {
            GameManager.instance.ComboReset();
            Destroy(gameObject);
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("HitZone"))
        {
            isPerfect = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("HitZone"))
        {
            isPerfect = false;
        }
    }
}}