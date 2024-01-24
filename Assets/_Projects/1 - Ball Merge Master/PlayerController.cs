using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Shared;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project1
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Transform _rotator;
        public float _shootForce = 1;
        public ForceMode _shootForceMode = ForceMode.VelocityChange;

        private Ball _currentBall;

        private void Start()
        {
            SpawnBall();
        }

        private async Task Shoot()
        {
            if(!_currentBall) return;
            
            var shootDir = -_rotator.transform.up;
            _currentBall.transform.SetParent(null);
            _currentBall.CanMerge = true;
            _currentBall.GetComponent<Rigidbody>().useGravity = true;
            _currentBall.GetComponent<Rigidbody>().AddForce(shootDir * _shootForce, _shootForceMode);
            _currentBall = null;
            await UniTask.WaitForSeconds(1);
            SpawnBall();
        }

        private void SpawnBall()
        {
            var level = Random.Range(0, 3);
            var prefabBall = GameController.Instance.PrefabBalls[level];
            _currentBall = Instantiate(prefabBall, _shootPoint.transform.position, Quaternion.identity);
            _currentBall.Level = level;
            _currentBall.transform.SetParent(_shootPoint);
            _currentBall.transform.ResetLocal(); 
            _currentBall.GetComponent<Rigidbody>().useGravity = false;
        }

        private void Update()
        {
            RotateToPointer();
            
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        }

        private void RotateToPointer()
        {
            var pointerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pointerPos.z = 0;
            var dir = _rotator.transform.position - pointerPos;
            _rotator.up = dir;
        }
    }
}