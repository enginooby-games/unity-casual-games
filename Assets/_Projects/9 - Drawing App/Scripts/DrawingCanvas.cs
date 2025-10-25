using UnityEngine;
using UnityEngine.UI;

namespace Devdy.DrawingApp
{
    /// <summary>
    /// Handles the drawing canvas rendering and pixel manipulation.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class DrawingCanvas : MonoBehaviour
    {
        private RawImage rawImage; // UI element to display the canvas
        private Texture2D canvasTexture;
        private Vector2 lastDrawPosition;
        private bool isDrawing;
        
        private int textureWidth = 1024;
        private int textureHeight = 1024;
        
        private Color backgroundColor = Color.white;

        public void Initialize()
        {
            rawImage = GetComponent<RawImage>();
            CreateCanvas();
        }

        private void CreateCanvas()
        {
            canvasTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            canvasTexture.filterMode = FilterMode.Point;
            
            Color[] pixels = new Color[textureWidth * textureHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = backgroundColor;
            }
            canvasTexture.SetPixels(pixels);
            canvasTexture.Apply();
            
            rawImage.texture = canvasTexture;
        }

        #region Drawing Methods ==================================================================
        
        public void StartDrawing(Vector2 screenPosition, Color color, int brushSize, bool isEraser)
        {
            Vector2 localPos = ScreenToTexturePosition(screenPosition);
            if (!IsValidPosition(localPos)) return;
            
            isDrawing = true;
            lastDrawPosition = localPos;
            DrawPoint(localPos, color, brushSize, isEraser);
        }

        public void ContinueDrawing(Vector2 screenPosition, Color color, int brushSize, bool isEraser)
        {
            if (!isDrawing) return;
            
            Vector2 localPos = ScreenToTexturePosition(screenPosition);
            if (!IsValidPosition(localPos)) return;
            
            DrawLine(lastDrawPosition, localPos, color, brushSize, isEraser);
            lastDrawPosition = localPos;
        }

        public void EndDrawing()
        {
            isDrawing = false;
        }

        /// <summary>
        /// Draws a single point on the canvas.
        /// </summary>
        private void DrawPoint(Vector2 position, Color color, int brushSize, bool isEraser)
        {
            Color drawColor = isEraser ? backgroundColor : color;
            int halfSize = brushSize / 2;
            
            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    if (x * x + y * y > halfSize * halfSize) continue;
                    
                    int px = Mathf.RoundToInt(position.x) + x;
                    int py = Mathf.RoundToInt(position.y) + y;
                    
                    if (px >= 0 && px < textureWidth && py >= 0 && py < textureHeight)
                    {
                        canvasTexture.SetPixel(px, py, drawColor);
                    }
                }
            }
            
            canvasTexture.Apply();
        }

        /// <summary>
        /// Draws a line between two points using interpolation.
        /// </summary>
        private void DrawLine(Vector2 start, Vector2 end, Color color, int brushSize, bool isEraser)
        {
            float distance = Vector2.Distance(start, end);
            int steps = Mathf.CeilToInt(distance);
            
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2 point = Vector2.Lerp(start, end, t);
                DrawPoint(point, color, brushSize, isEraser);
            }
        }

        #endregion ==================================================================

        #region Canvas Operations ==================================================================
        
        public void Clear()
        {
            Color[] pixels = new Color[textureWidth * textureHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = backgroundColor;
            }
            canvasTexture.SetPixels(pixels);
            canvasTexture.Apply();
        }

        public Texture2D GetCanvasSnapshot()
        {
            Texture2D snapshot = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            snapshot.SetPixels(canvasTexture.GetPixels());
            snapshot.Apply();
            return snapshot;
        }

        public void RestoreFromSnapshot(Texture2D snapshot)
        {
            if (snapshot.width != textureWidth || snapshot.height != textureHeight)
            {
                Debug.LogError("Snapshot dimensions don't match canvas!");
                return;
            }
            
            canvasTexture.SetPixels(snapshot.GetPixels());
            canvasTexture.Apply();
        }

        #endregion ==================================================================

        #region Helper Methods ==================================================================
        
        private Vector2 ScreenToTexturePosition(Vector2 screenPosition)
        {
            RectTransform rectTransform = rawImage.rectTransform;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, 
                screenPosition, 
                null, 
                out localPoint
            );
            
            Rect rect = rectTransform.rect;
            float normalizedX = (localPoint.x - rect.x) / rect.width;
            float normalizedY = (localPoint.y - rect.y) / rect.height;
            
            return new Vector2(
                normalizedX * textureWidth,
                normalizedY * textureHeight
            );
        }

        private bool IsValidPosition(Vector2 position)
        {
            return position.x >= 0 && position.x < textureWidth &&
                   position.y >= 0 && position.y < textureHeight;
        }

        #endregion ==================================================================
    }
}