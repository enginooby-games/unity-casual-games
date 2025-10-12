using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Shared;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project1
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Transform _rotator;
        [SerializeField] private DOTweenAnimation _rotatorTween;

        public float _shootForce = 1;
        public ForceMode _shootForceMode = ForceMode.VelocityChange;

        private Ball _currentBall;
        private TransformConfiner _transformConfiner;
        private bool _enableControl = true;

        private void Start()
        {
            MessageBroker.Default.Receive<OnGameOver>().Subscribe(_ => OnGameOver()).AddTo(this);
            MessageBroker.Default.Receive<OnGameRestart>().Subscribe(_ => OnGameRestart()).AddTo(this);

            SpawnBall();

            // confiner for manual rotation
            // _transformConfiner = _rotator.gameObject.AddComponent<TransformConfiner>();
            // _transformConfiner.ZRange = new(-70, 70);
        }

        private void OnGameOver()
        {
            _rotatorTween.DOPause();
            _enableControl = false;
        }

        private void OnGameRestart()
        {
            // UTILS
            foreach (var ball in FindObjectsOfType<Ball>())
            {
                Destroy(ball.gameObject);
            }

            _rotatorTween.DOPlay();
            _enableControl = true;
            _currentBall = null;
            SpawnBall();
        }

        private async Task Shoot()
        {
            if (!_enableControl) return;

            if (!_currentBall) return;

            var shootDir = -_rotator.transform.up;
            _currentBall.transform.SetParent(null);
            _currentBall.CanMerge = true;
            _currentBall.GetComponent<Collider>().enabled = true;
            _currentBall.GetComponent<Rigidbody>().useGravity = true;
            _currentBall.GetComponent<Rigidbody>().AddForce(shootDir * _shootForce, _shootForceMode);
            _currentBall = null;
            await UniTask.WaitForSeconds(1);
            SpawnBall();
        }

        private void SpawnBall()
        {
            if (_currentBall) return;

            var level = Random.Range(0, SROptions.Current.P1_MaxLevel - 1);
            var prefabBall = GameController.Instance.PrefabBall;
            _currentBall = Instantiate(prefabBall, _shootPoint.transform.position, Quaternion.identity);
            _currentBall.SetLevel(level);
            _currentBall.transform.SetParent(_shootPoint);
            _currentBall.transform.ResetLocal();
            _currentBall.GetComponent<Rigidbody>().useGravity = false;
            _currentBall.GetComponent<Collider>().enabled = false;
        }

        private void Update()
        {
            if (!_enableControl) return;

            if (Input.GetMouseButtonDown(0))
            {
                _rotatorTween.DOPause();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _rotatorTween.DOPlay();
                Shoot();
            }
        }

        // manual rotation
        private void RotateToPointer()
        {
            if (!_enableControl) return;

            var pointerPos = Input.mousePosition;
            if (Camera.main.orthographic)
            {
                pointerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            pointerPos.z = 0;
            var dir = _rotator.transform.position - pointerPos;
            _rotator.up = dir;
        }
    }
}