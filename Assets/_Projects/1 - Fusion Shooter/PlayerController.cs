using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Shared;
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
        private Vector2 _rotationRange = new (-70, 70);
        private TransformConfiner _transformConfiner;
        
        private void Start()
        {
            SpawnBall();
            _transformConfiner = _rotator.gameObject.AddComponent<TransformConfiner>();
            _transformConfiner.ZRange = _rotationRange;
            // RotateAuto();
        }

        private async Task Shoot()
        {
            if (!_currentBall)
            {
                SpawnBall();
            }
            
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
            if(_currentBall) return;
            
            var level = Random.Range(0, 3);
            var prefabBall = GameController.Instance.PrefabBalls[level];
            _currentBall = Instantiate(prefabBall, _shootPoint.transform.position, Quaternion.identity);
            _currentBall.Level = level;
            _currentBall.transform.SetParent(_shootPoint);
            _currentBall.transform.ResetLocal(); 
            _currentBall.GetComponent<Rigidbody>().useGravity = false;
            _currentBall.GetComponent<Collider>().enabled = false;
        }

        private void Update()
        {
            // RotateToPointer();
             // _transformConfiner.UpdateConfine();
            
            if (Input.GetMouseButtonDown(0))
            {
                _rotatorTween.DOPause();
            }else if (Input.GetMouseButtonUp(0))
            {
                _rotatorTween.DOPlay();
                Shoot();
            }
            
        }

        private void RotateToPointer()
        {
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