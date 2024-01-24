using System;
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
        
        private void Shoot()
        {
            var level = Random.Range(0, 3);
            var prefabBall = BallManager.Instance.PrefabBalls[level];
            var ball = Instantiate(prefabBall, _shootPoint.transform.position, Quaternion.identity);
            var shootDir = -_rotator.transform.up;
            ball.GetComponent<Rigidbody>().AddForce(shootDir * _shootForce, _shootForceMode);
            ball.Level = level;
            ball.transform.localScale = new(0.3f * (level + 1), 0.3f * (level + 1), 1);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Shoot();
            }
        }
    }
}