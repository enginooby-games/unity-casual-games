using UnityEngine;

namespace Project1
{
    public class Ball : MonoBehaviour
    {
        [HideInInspector] public int Level;

        private void OnCollisionEnter(Collision collision)
        {
            if(!collision.gameObject.TryGetComponent(out Ball otherBall)) return;
            if(otherBall.Level != Level) return;

            // prevent 2-way collision
            if(transform.position.y < otherBall.transform.position.y) return;
            // Debug.Log($"{name} hits {otherBall.name}");
            // GetComponent<Collider>().enabled = false;
            // otherBall.GetComponent<Collider>().enabled = false;
            BallManager.Instance.Merge(this, otherBall);
        }
    }
}