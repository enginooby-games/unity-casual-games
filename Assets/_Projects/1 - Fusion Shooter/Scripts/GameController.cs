using Shared;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Project1
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance => FindObjectOfType<GameController>();

        [field: SerializeReference] public Ball PrefabBall { get; private set; } = new();
        [field: SerializeReference] private TMPro.TMP_Text _labelCoin;
        [field: SerializeReference] private Button _buttonRestart;

        private int _coin;
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
              RestartGame();
            }).AddTo(this);

            _buttonRestart.gameObject.SetActive(false);
        }

        public void RestartGame()
        {
            IsGameOver = false;
            _buttonRestart.gameObject.SetActive(false);
            MessageBroker.Default.Publish(new OnGameRestart());
            ResetCoin();
        }

        private void Start()
        {
            UpdateCoin(0);
            SRDebug.Instance.PinAllOptions("Project1");
        }

        public void Merge(Ball ball1, Ball ball2)
        {
            if (ball1.Level >= SROptions.Current.P1_MaxLevel - 1) return; // max level dont merge
            
            var ballPrefab = PrefabBall;
            var newBall = Instantiate(ballPrefab, ball2.transform.position, Quaternion.identity);
            newBall.SetLevel(ball1.Level + 1);
            newBall.DelayMerge();

            UpdateCoin(ball1.CoinReward);
            Destroy(ball1.gameObject);
            Destroy(ball2.gameObject);
        }

        private void UpdateCoin(int coinToAdd)
        {
            _coin += coinToAdd;
            _labelCoin.text = $"Score: {_coin}";
        }

        private void ResetCoin() => UpdateCoin(-_coin);
    }
}