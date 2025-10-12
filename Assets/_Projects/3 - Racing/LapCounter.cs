// Lap Counter Script

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project3
{

    public class LapCounter : MonoBehaviour
    {
        public TextMeshProUGUI lapText;
        public TextMeshProUGUI winText;
        public int totalLaps = 3;

        private int currentLap = 0;
        private bool hasPassedCheckpoint = false;

        void Start()
        {
            UpdateLapUI();
            if (winText != null) winText.enabled = false;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Checkpoint"))
            {
                hasPassedCheckpoint = true;
            }

            if (other.CompareTag("FinishLine") && hasPassedCheckpoint)
            {
                currentLap++;
                hasPassedCheckpoint = false;
                UpdateLapUI();

                if (currentLap >= totalLaps)
                {
                    WinRace();
                }
            }
        }

        void UpdateLapUI()
        {
            if (lapText != null)
            {
                lapText.text = "Lap: " + currentLap + "/" + totalLaps;
            }
        }

        void WinRace()
        {
            if (winText != null)
            {
                winText.enabled = true;
                winText.text = "YOU WIN!";
            }

            Time.timeScale = 0.5f;
        }
    }
}