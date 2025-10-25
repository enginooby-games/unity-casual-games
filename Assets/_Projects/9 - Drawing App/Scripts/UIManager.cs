using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Devdy.DrawingApp
{
    /// <summary>
    /// Manages all UI elements and user interactions for the drawing app.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Tool Buttons")]
        [SerializeField] private Button brushButton; // Brush tool button
        [SerializeField] private Button eraserButton; // Eraser tool button
        [SerializeField] private Button clearButton; // Clear canvas button
        [SerializeField] private Button undoButton; // Undo button
        [SerializeField] private Button redoButton; // Redo button
        
        [Header("Color Palette")]
        [SerializeField] private Button[] colorButtons; // Array of color selection buttons
        [SerializeField] private Image currentColorDisplay; // Shows current selected color
        
        [Header("Brush Size")]
        [SerializeField] private Slider brushSizeSlider; // Brush size control
        [SerializeField] private TextMeshProUGUI brushSizeText; // Displays current brush size
        
        [Header("Tool Indicators")]
        [SerializeField] private GameObject brushIndicator; // Visual indicator for brush mode
        [SerializeField] private GameObject eraserIndicator; // Visual indicator for eraser mode
        

        private Color[] paletteColors = new Color[]
        {
            Color.black,
            Color.white,
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            new Color(1f, 0.5f, 0f), // Orange
            new Color(0.5f, 0f, 1f), // Purple
            new Color(0f, 1f, 1f), // Cyan
            new Color(1f, 0f, 1f) // Magenta
        };

        public void Initialize()
        {
            SetupButtons();
            SetupColorPalette();
            SetupBrushSizeSlider();
            UpdateToolDisplay(false);
            UpdateUndoRedoButtons();
        }

        private void Update()
        {
            UpdateUndoRedoButtons();
        }

        #region Setup Methods ==================================================================
        
        private void SetupButtons()
        {
            if (brushButton != null)
                brushButton.onClick.AddListener(() => OnBrushButtonClicked());
            
            if (eraserButton != null)
                eraserButton.onClick.AddListener(() => OnEraserButtonClicked());
            
            if (clearButton != null)
                clearButton.onClick.AddListener(() => OnClearButtonClicked());
            
            if (undoButton != null)
                undoButton.onClick.AddListener(() => OnUndoButtonClicked());
            
            if (redoButton != null)
                redoButton.onClick.AddListener(() => OnRedoButtonClicked());
        }

        private void SetupColorPalette()
        {
            if (colorButtons == null || colorButtons.Length == 0) return;
            
            int colorCount = Mathf.Min(colorButtons.Length, paletteColors.Length);
            
            for (int i = 0; i < colorCount; i++)
            {
                if (colorButtons[i] == null) continue;
                
                Color color = paletteColors[i];
                Image buttonImage = colorButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = color;
                }
                
                int index = i;
                colorButtons[i].onClick.AddListener(() => OnColorButtonClicked(paletteColors[index]));
            }
            
            if (currentColorDisplay != null)
            {
                currentColorDisplay.color = DrawingManager.Instance.GetCurrentColor();
            }
        }

        private void SetupBrushSizeSlider()
        {
            if (brushSizeSlider == null) return;
            
            brushSizeSlider.minValue = 1;
            brushSizeSlider.maxValue = 50;
            brushSizeSlider.value = DrawingManager.Instance.GetBrushSize();
            brushSizeSlider.onValueChanged.AddListener(OnBrushSizeChanged);
            
            UpdateBrushSizeText(DrawingManager.Instance.GetBrushSize());
        }

        #endregion ==================================================================

        #region Button Callbacks ==================================================================
        
        private void OnBrushButtonClicked()
        {
            DrawingManager.Instance.SetEraser(false);
        }

        private void OnEraserButtonClicked()
        {
            DrawingManager.Instance.SetEraser(true);
        }

        private void OnClearButtonClicked()
        {
            DrawingManager.Instance.ClearCanvas();
        }

        private void OnUndoButtonClicked()
        {
            DrawingManager.Instance.Undo();
        }

        private void OnRedoButtonClicked()
        {
            DrawingManager.Instance.Redo();
        }

        private void OnColorButtonClicked(Color color)
        {
            DrawingManager.Instance.SetColor(color);
            if (currentColorDisplay != null)
            {
                currentColorDisplay.color = color;
            }
        }

        private void OnBrushSizeChanged(float value)
        {
            int size = Mathf.RoundToInt(value);
            DrawingManager.Instance.SetBrushSize(size);
            UpdateBrushSizeText(size);
        }

        #endregion ==================================================================

        #region UI Updates ==================================================================
        
        public void UpdateToolDisplay(bool isEraserActive)
        {
            if (brushIndicator != null)
                brushIndicator.SetActive(!isEraserActive);
            
            if (eraserIndicator != null)
                eraserIndicator.SetActive(isEraserActive);
        }

        private void UpdateBrushSizeText(int size)
        {
            if (brushSizeText != null)
            {
                brushSizeText.text = $"{size}";
            }
        }

        private void UpdateUndoRedoButtons()
        {
            if (undoButton != null)
                undoButton.interactable = DrawingManager.Instance.CanUndo();
            
            if (redoButton != null)
                redoButton.interactable = DrawingManager.Instance.CanRedo();
        }

       

        #endregion ==================================================================
    }
}