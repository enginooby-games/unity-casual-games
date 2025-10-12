using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project2
{

// Quản lý trò chơi
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Button ButtonStart;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public GameObject notePrefab;
    public GameObject hitZonePrefab;
    public int numberOfLanes => SROptions.Current.P2_LaneAmount;
    public int bpm => SROptions.Current.P2_Pbm; // Beats per minute
    public int maxNotesPerBeat => SROptions.Current.P2_MaxConcurrentNotes; // 1-2 notes per beat
    
    private int score = 0;
    private int combo = 0;
    private float beatInterval; // Time between beats
    private float nextBeatTime;
    private Transform[] spawnPositions;
    private bool gameStarted;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        
        ButtonStart.onClick.AddListener(() =>
        {
            ButtonStart.gameObject.SetActive(false);
            RestartGame();
        });
    }

    void GenerateLanes()
    {
        float laneSpacing = 6f / (numberOfLanes - 1);
        
        for (int i = 0; i < numberOfLanes; i++)
        {
            GameObject lane = new GameObject($"Lane_{i}");
            lane.transform.parent = transform;
            lane.transform.position = new Vector3(-3 + (i * laneSpacing), 4, 0);
            
            spawnPositions[i] = lane.transform;
        }
    }

    void GenerateHitZones()
    {
        float laneSpacing = numberOfLanes > 1 ? 6f / (numberOfLanes - 1) : 0;
        
        for (int i = 0; i < numberOfLanes; i++)
        {
            var hitZone = Instantiate(hitZonePrefab);
            
            hitZone.name = $"HitZone_{i}";
            hitZone.transform.parent = transform;
            
            float xPos = numberOfLanes > 1 ? -3 + (i * laneSpacing) : 0;
            hitZone.transform.position = new Vector3(xPos, -2.4f, 0);
            hitZone.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            
            if (!hitZone.CompareTag("HitZone"))
            {
                hitZone.tag = "HitZone";
            }
            
            // Add Collider if not present
            if (hitZone.GetComponent<CircleCollider2D>() == null)
            {
                CircleCollider2D col = hitZone.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.3f;
            }
            
            // Add Rigidbody if not present
            if (hitZone.GetComponent<Rigidbody2D>() == null)
            {
                Rigidbody2D rb = hitZone.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0;
            }
            
            // Add HitZone script if not present
            HitZone hzScript = hitZone.GetComponent<HitZone>();
            if (hzScript == null)
            {
                hzScript = hitZone.AddComponent<HitZone>();
            }
            hzScript.noteIndex = i;
            
            Debug.Log($"Generated HitZone_{i} at position {hitZone.transform.position}");
        }
    }
    
    public AudioSource AudioSource;
    
    void Update()
    {
        if(!gameStarted) return;
        
        // Check if it's time for the next beat
        if (Time.time >= nextBeatTime)
        {
            SpawnNotesOnBeat();
            nextBeatTime += beatInterval;
        }
    }

    public void RestartGame()
    {
        gameStarted = true;
        AudioSource.Stop();
        AudioSource.time = 0;
        AudioSource.pitch = SROptions.Current.P2_Pbm / 70f;
        AudioSource.Play();

        spawnPositions = new Transform[numberOfLanes];

        // Clear all existing notes
        foreach (var note in FindObjectsOfType<FallingNote>())
        {
            Destroy(note.gameObject);
        }
        foreach (var hitzone in FindObjectsOfType<HitZone>())
        {
            Destroy(hitzone.gameObject);
        }

        // Reset score and combo
        score = 0;
        combo = 0;
        UpdateScoreUI();
        UpdateComboUI();

        // Reset beat timing
        beatInterval = 60f / bpm;
        nextBeatTime = Time.time + beatInterval;
        
        GenerateLanes();
        GenerateHitZones();
        SRDebug.Instance.PinAllOptions("Project2");

        Debug.Log("Game Restarted!");
    }


    void SpawnNotesOnBeat()
    {
        // Randomly spawn 1 to maxNotesPerBeat notes
        int notesToSpawn = Random.Range(1, maxNotesPerBeat + 1);

        if (Random.Range(0, 100) < 70)
        {
            notesToSpawn = 1;
        }
        
        // Track which lanes are used to avoid duplicates
        System.Collections.Generic.List<int> usedLanes = new System.Collections.Generic.List<int>();
        
        for (int i = 0; i < notesToSpawn; i++)
        {
            int laneIndex;
            int attempts = 0;
            
            // Find an unused lane (max 10 attempts to avoid infinite loop)
            do
            {
                laneIndex = Random.Range(0, numberOfLanes);
                attempts++;
            } while (usedLanes.Contains(laneIndex) && attempts < 10);
            
            if (attempts < 10)
            {
                usedLanes.Add(laneIndex);
                SpawnNote(laneIndex);
            }
        }
    }

    void SpawnNote(int laneIndex)
    {
        if (laneIndex >= spawnPositions.Length || spawnPositions[laneIndex] == null) return;

        GameObject note = Instantiate(
            notePrefab,
            spawnPositions[laneIndex].position,
            Quaternion.identity
        );
        
        FallingNote fn = note.GetComponent<FallingNote>();
        if (fn != null)
        {
            fn.noteIndex = laneIndex;
        }
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();
    }

    public void IncreaseCombo()
    {
        combo++;
        UpdateComboUI();
    }

    public void ComboReset()
    {
        combo = 0;
        UpdateComboUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    void UpdateComboUI()
    {
        if (comboText != null)
            comboText.text = $"Combo: {combo}";
    }
}

}