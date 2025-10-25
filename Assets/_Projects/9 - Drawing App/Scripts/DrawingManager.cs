using UnityEngine;
using System.Collections.Generic;

namespace Devdy.DrawingApp
{
    /// <summary>
    /// Main manager for the drawing application. Handles canvas state, drawing operations, and undo/redo.
    /// </summary>
    public class DrawingManager : Singleton<DrawingManager>
    {
        [SerializeField] private DrawingCanvas canvas; // Reference to the drawing canvas
        [SerializeField] private UIManager uiManager; // Reference to UI manager
        [SerializeField] private PromptGenerator promptGenerator;
        
        private Stack<Texture2D> undoStack = new Stack<Texture2D>();
        private Stack<Texture2D> redoStack = new Stack<Texture2D>();
        
        private Color currentColor = Color.black;
        private int brushSize = 5;
        private bool isEraser = false;
        
        private const int MAX_UNDO_STEPS = 20;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            if (canvas == null || uiManager == null)
            {
                Debug.LogError("DrawingManager: Missing required references!");
                return;
            }
            
            uiManager.Initialize();
            canvas.Initialize();
        }

        #region Drawing Operations ==================================================================
        
        public void StartDrawing(Vector2 position)
        {
            SaveStateForUndo();
            canvas.StartDrawing(position, currentColor, brushSize, isEraser);
        }

        public void ContinueDrawing(Vector2 position)
        {
            canvas.ContinueDrawing(position, currentColor, brushSize, isEraser);
        }

        public void EndDrawing()
        {
            canvas.EndDrawing();
        }

        #endregion ==================================================================

        #region Tool Settings ==================================================================
        
        public void SetColor(Color color)
        {
            currentColor = color;
            isEraser = false;
            uiManager.UpdateToolDisplay(false);
        }

        public void SetBrushSize(int size)
        {
            brushSize = Mathf.Clamp(size, 1, 50);
        }

        public void SetEraser(bool active)
        {
            isEraser = active;
            uiManager.UpdateToolDisplay(active);
        }

        public Color GetCurrentColor() => currentColor;
        public int GetBrushSize() => brushSize;
        public bool IsEraserActive() => isEraser;

        #endregion ==================================================================

        #region Canvas Operations ==================================================================
        
        public void ClearCanvas()
        {
            SaveStateForUndo();
            canvas.Clear();
            redoStack.Clear();
        }

        #endregion ==================================================================

        #region Undo/Redo ==================================================================
        
        private void SaveStateForUndo()
        {
            Texture2D snapshot = canvas.GetCanvasSnapshot();
            undoStack.Push(snapshot);
            
            if (undoStack.Count > MAX_UNDO_STEPS)
            {
                var oldest = undoStack.ToArray()[undoStack.Count - 1];
                Destroy(oldest);
                Stack<Texture2D> temp = new Stack<Texture2D>();
                int count = undoStack.Count - 1;
                for (int i = 0; i < count; i++)
                {
                    temp.Push(undoStack.Pop());
                }
                undoStack.Pop();
                while (temp.Count > 0)
                {
                    undoStack.Push(temp.Pop());
                }
            }
            
            redoStack.Clear();
        }

        public void Undo()
        {
            if (undoStack.Count == 0) return;
            
            Texture2D currentState = canvas.GetCanvasSnapshot();
            redoStack.Push(currentState);
            
            Texture2D previousState = undoStack.Pop();
            canvas.RestoreFromSnapshot(previousState);
            Destroy(previousState);
        }

        public void Redo()
        {
            if (redoStack.Count == 0) return;
            
            Texture2D currentState = canvas.GetCanvasSnapshot();
            undoStack.Push(currentState);
            
            Texture2D nextState = redoStack.Pop();
            canvas.RestoreFromSnapshot(nextState);
            Destroy(nextState);
        }

        public bool CanUndo() => undoStack.Count > 0;
        public bool CanRedo() => redoStack.Count > 0;

        #endregion ==================================================================

        #region AI Generation ==================================================================
        
        public void GenerateAIImage(string prompt, string apiKey)
        {
            Texture2D currentDrawing = canvas.GetCanvasSnapshot();
            
            AIGeneratorManager.Instance.GenerateFromDrawing(
                currentDrawing,
                prompt,
                apiKey,
                OnAIImageGenerated,
                OnAIImageError
            );
            
            Destroy(currentDrawing);
        }

        private void OnAIImageGenerated(Texture2D generatedImage)
        {
            SaveStateForUndo();
            canvas.RestoreFromSnapshot(generatedImage);
            promptGenerator.OnAIGenerationComplete(true, "AI image generated successfully!");
            Destroy(generatedImage);
        }

        private void OnAIImageError(string error)
        {
            Debug.LogError($"AI Generation Error: {error}");
            promptGenerator.OnAIGenerationComplete(false, error);
        }

        public bool IsGeneratingAI() => AIGeneratorManager.Instance.IsGenerating();

        #endregion ==================================================================
    }
}