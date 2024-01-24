using System.Collections.Generic;
using UnityEngine;

namespace Project1
{
    public class BallManager : MonoBehaviour
    {
        public static BallManager Instance => FindObjectOfType<BallManager>();
        
        [field: SerializeReference] public List<Ball> PrefabBalls { get; private set; } = new();

        public void Merge(Ball ball1, Ball ball2)
        {
            var ballPrefab = PrefabBalls[ball1.Level + 1];
            Destroy(ball1.gameObject);
            Destroy(ball2.gameObject);
            var newBall = Instantiate(ballPrefab, ball2.transform.position, Quaternion.identity);
            newBall.Level = ball1.Level + 1;
        }
    }
}