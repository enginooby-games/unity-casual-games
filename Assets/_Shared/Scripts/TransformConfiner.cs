using UnityEngine;

namespace Shared
{
    public class TransformConfiner : MonoBehaviour
    {
        public Vector2 XRange;
        public Vector2 YRange;
        public Vector2 ZRange;

        private TransformInspector _transformInspector;

        private void Awake()
        {
            _transformInspector = gameObject.AddComponent<TransformInspector>();
        }

        public void UpdateConfine()
        {
            _transformInspector.UpdateCalculation();
            var rot = _transformInspector.localEulerAngles;
            var x = Mathf.Clamp(rot.x, XRange.x, XRange.y);
            var y = Mathf.Clamp(rot.y, YRange.x, YRange.y);
            var z = Mathf.Clamp(rot.z, ZRange.x, ZRange.y);

            transform.localEulerAngles = new(x, y, z);
        }
    }
}