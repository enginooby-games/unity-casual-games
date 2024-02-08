using System.Collections.Generic;
using Shared;
using UniRx;
using UnityEngine;

namespace Project1
{
    // TODO: breakable object with health to game over
    public class GameOverTrigger : MonoBehaviour
    {
        private Dictionary<Ball, float> _collisions = new();

        private void OnCollisionExit(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Ball ball))
            {
                if (_collisions.ContainsKey(ball)) _collisions.Remove(ball);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Ball ball))
            {
                _collisions.Add(ball, Time.timeSinceLevelLoad);
            }
        }

        private void Update()
        {
            foreach (var (ball, collisionTimestamp) in _collisions)
            {
                // ball maybe merged/destroyed without exiting collision
                if (ball && Time.timeSinceLevelLoad - collisionTimestamp > 1f)
                {
                    MessageBroker.Default.Publish(new OnGameOver());
                }
            }
        }
    }
}