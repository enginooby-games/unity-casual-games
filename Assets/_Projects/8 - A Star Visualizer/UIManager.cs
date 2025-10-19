using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Devdy.AStarVisualizer
{
    /// <summary>
    /// Manages UI interactions and controls for the A* visualizer.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button findPathButton;
        [SerializeField] private Button resetButton;
        
        [Header("Text Labels")]
        [SerializeField] private TextMeshProUGUI widthText;
        [SerializeField] private TextMeshProUGUI heightText;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        
        private const string INSTRUCTIONS = 
            "<b>Controls:</b>\n" +
            "• <b>Left Click:</b> Toggle Obstacle\n" +
            "• <b>Shift + Click:</b> Set Start (Green)\n" +
            "• <b>Ctrl + Click:</b> Set End (Red)\n" +
            "• <b>Right Click:</b> Clear Cell";

        private void Start()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            if (findPathButton != null)
            {
                findPathButton.onClick.AddListener(OnFindPathClicked);
            }
            
            if (resetButton != null)
            {
                resetButton.onClick.AddListener(OnResetClicked);
            }
            
            if (instructionsText != null)
            {
                instructionsText.text = INSTRUCTIONS;
            }
        }

        private void OnFindPathClicked()
        {
            if (AStarPathfinder.Instance.IsPathfinding())
            {
                Debug.Log("Pathfinding already in progress!");
                return;
            }
            
            AStarPathfinder.Instance.StartPathfinding();
        }

        private void OnResetClicked()
        {
            if (AStarPathfinder.Instance.IsPathfinding())
            {
                Debug.Log("Cannot reset while pathfinding!");
                return;
            }
            
            GridManager.Instance.ResetGrid();
        }
        
        private void UpdateWidthLabel(int width)
        {
            if (widthText != null)
            {
                widthText.text = $"Width: {width}";
            }
        }

        private void UpdateHeightLabel(int height)
        {
            if (heightText != null)
            {
                heightText.text = $"Height: {height}";
            }
        }

        private void UpdateSpeedLabel(float value)
        {
            if (speedText != null)
            {
                speedText.text = $"Speed: {value:F2}";
            }
        }
    }
}