using System;
using UnityEngine;

namespace Project1
{
    public class Ball : MonoBehaviour
    {
        [HideInInspector] public int Level;

        private void Start()
        {
            transform.localScale = new(0.3f * (Level + 1), 0.3f * (Level + 1), 1);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(!collision.gameObject.TryGetComponent(out Ball otherBall)) return;
            if(otherBall.Level != Level) return;

            // prevent 2-way collision
            if(transform.position.y < otherBall.transform.position.y) return;
            BallManager.Instance.Merge(this, otherBall);
        }
    }
}