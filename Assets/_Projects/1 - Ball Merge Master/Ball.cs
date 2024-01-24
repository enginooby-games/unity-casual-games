using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Shared;
using UnityEngine;

namespace Project1
{
    public class Ball : MonoBehaviour
    {
        [HideInInspector] public int Level;
        [HideInInspector] public bool CanMerge;

        private async void Start()
        {
            var scale = new Vector3(0.3f * (Level + 1), 0.3f * (Level + 1), 1);
            transform.SetGlobalScale(scale);
            
            // continuous merging delay
            await UniTask.Delay(1500);
            if(!this) return;
            CanMerge = true;
            GetComponent<Rigidbody>().WakeUp();
        }
        
        private void HandleCollision(Collision collision)
        {
            // Debug.Log($"{name}-{collision.gameObject.name}");
            if(!CanMerge) return;
            if(!collision.gameObject.TryGetComponent(out Ball otherBall)) return;
            if(otherBall.Level != Level) return;

            // prevent 2-way collision
            if(transform.position.y < otherBall.transform.position.y) return;
            BallManager.Instance.Merge(this, otherBall);
        }

        private void OnCollisionEnter(Collision collision) => HandleCollision(collision);

        private void OnCollisionStay(Collision collision) => HandleCollision(collision);
    }
}