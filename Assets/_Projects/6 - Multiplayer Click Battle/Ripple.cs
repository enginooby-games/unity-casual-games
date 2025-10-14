namespace SimpleSignalRGame
{
    using UnityEngine;
    using UnityEngine.UI;

    public class Ripple : MonoBehaviour
    {
        public float duration = 0.6f;
        public float maxScale = 2.5f;
        private Image image;
        private float timer;

        void Start()
        {
            image = GetComponent<Image>();
            transform.localScale = Vector3.zero;
        }

        void Update()
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * maxScale, t);
            var c = image.color;
            c.a = Mathf.Lerp(1, 0, t);
            image.color = c;

            if (timer >= duration)
                Destroy(gameObject);
        }
    }
}