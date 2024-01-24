using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project1
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance => FindObjectOfType<GameController>();
        
        [field: SerializeReference] public List<Ball> PrefabBalls { get; private set; } = new();
        [field: SerializeReference] private TMPro.TMP_Text _labelCoin;

        private int _coin;
        private bool _destroyMaxLevelBall;

        private void Start()
        {
            UpdateCoin(0);
        }

        public void Merge(Ball ball1, Ball ball2)
        {
            if (ball1.Level < PrefabBalls.Count - 1)
            {
                var ballPrefab = PrefabBalls[ball1.Level + 1];
                var newBall = Instantiate(ballPrefab, ball2.transform.position, Quaternion.identity);
                newBall.Level = ball1.Level + 1;
                newBall.DelayMerge();
            } else if (!_destroyMaxLevelBall)
            {
                return;
            }
            
            UpdateCoin(ball1.CoinReward);
            Destroy(ball1.gameObject);
            Destroy(ball2.gameObject);
        }

        private void UpdateCoin(int coinToAdd)
        {
            _coin += coinToAdd;
            _labelCoin.text = _coin.ToString();
        }
    }
}