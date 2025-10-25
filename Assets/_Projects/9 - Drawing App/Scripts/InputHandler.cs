using UnityEngine;
using UnityEngine.EventSystems;

namespace Devdy.DrawingApp
{
    /// <summary>
    /// Handles mouse and touch input for drawing on the canvas.
    /// </summary>
    public class InputHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private bool isDrawing;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (DrawingManager.Instance == null) return;
            
            isDrawing = true;
            DrawingManager.Instance.StartDrawing(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDrawing || DrawingManager.Instance == null) return;
            
            DrawingManager.Instance.ContinueDrawing(eventData.position);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isDrawing || DrawingManager.Instance == null) return;
            
            isDrawing = false;
            DrawingManager.Instance.EndDrawing();
        }
    }
}