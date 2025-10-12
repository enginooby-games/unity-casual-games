using TMPro;
using UnityEngine;

namespace Project3
{
    public class Speedometer : MonoBehaviour
    {
        public TextMeshProUGUI speedText;
        public Rigidbody2D carRigidbody;
        
        void Update()
        {
            if (carRigidbody != null && speedText != null)
            {
                float speed = carRigidbody.linearVelocity.magnitude * 3.6f; // Convert to km/h
                speedText.text = "Speed: " + speed.ToString("F0") + " km/h";
            }
        }
    }
}