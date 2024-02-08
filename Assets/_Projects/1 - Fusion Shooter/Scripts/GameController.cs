using System.Collections.Generic;
using Shared;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Project1
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance => FindObjectOfType<GameController>();

        [field: SerializeReference] public List<Ball> PrefabBalls { get; private set; } = new();
        [field: SerializeReference] private TMPro.TMP_Text _labelCoin;
        [field: SerializeReference] private Button _buttonRestart;

        private int _coin;
        private bool _destroyMaxLevelBall;
        public bool IsGameOver { get; private set; }

        private void Awake()
        {
            MessageBroker.Default.Receive<OnGameOver>().Subscribe(_ =>
            {
                IsGameOver = true;
                _buttonRestart.gameObject.SetActive(true);
            }).AddTo(this);
            
            _buttonRestart.OnClickAsObservable().Subscribe(_ =>
            {
                IsGameOver = false;
                _buttonRestart.gameObject.SetActive(false);
                MessageBroker.Default.Publish(new OnGameRestart());
                ResetCoin();
            }).AddTo(this);
            
            _buttonRestart.gameObject.SetActive(false);
        }

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
            }
            else if (!_destroyMaxLevelBall)
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

        private void ResetCoin() => UpdateCoin(-_coin);
    }
}