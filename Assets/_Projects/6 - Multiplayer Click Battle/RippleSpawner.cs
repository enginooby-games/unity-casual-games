namespace SimpleSignalRGame
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class RippleSpawner : MonoBehaviour
    {
        public GameObject ripplePrefab;
        public Canvas canvas;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 pos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    Input.mousePosition,
                    canvas.worldCamera,
                    out pos
                );
                var ripple = Instantiate(ripplePrefab, canvas.transform);
                ripple.GetComponent<RectTransform>().anchoredPosition = pos;
            }
        }
    }
}