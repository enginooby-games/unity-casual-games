using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Shared;
using TMPro;
using UnityEngine;

namespace Project1
{
    public class Ball : MonoBehaviour
    {
        public TextMeshPro LabelLevel;
        public MeshRenderer MeshRenderer;
        
        [HideInInspector] public int Level = 1;
        [HideInInspector] public int CoinReward;
        [HideInInspector] public bool CanMerge;

        private void Start()
        {
            CoinReward = (Level + 1) * 2;
            var scale = new Vector3(0.3f * (Level + 1), 0.3f * (Level + 1), 1);
            transform.SetGlobalScale(scale);
        }

        public void SetLevel(int level)
        {
            Level = level;
            LabelLevel.SetText((level + 1).ToString());
            MeshRenderer.material.color = GetColorByLevel(level);
        }

        public async Task DelayMerge()
        {
            // continuous merging delay
            await UniTask.Delay(1000);
            if(!this) return;
            CanMerge = true;
            GetComponent<Rigidbody>().WakeUp();
            if (Level >= SROptions.Current.P1_MaxLevel - 1 && SROptions.Current.P1_DestroyMaxLevel)
            {
                Destroy(gameObject);
            }
        }
        
        Color GetColorByLevel(int lvl)
        {
            // giảm độ sáng dần (clamp để không tối quá)
            var baseColor = Color.cyan;
            var darkenStep = 0.1f;
            float factor = Mathf.Clamp01(1f - lvl * darkenStep);
            return new Color(baseColor.r * factor, baseColor.g * factor, baseColor.b * factor);
        }
        
        private void HandleCollision(Collision collision)
        {
            // Debug.Log($"{name}-{collision.gameObject.name}");
            if(!CanMerge) return;
            if(!collision.gameObject.TryGetComponent(out Ball otherBall)) return;
            if(otherBall.Level != Level) return;

            // prevent 2-way collision
            if(transform.position.y < otherBall.transform.position.y) return;
            GameController.Instance.Merge(this, otherBall);
        }

        private void OnCollisionEnter(Collision collision) => HandleCollision(collision);

        private void OnCollisionStay(Collision collision) => HandleCollision(collision);
    }
}