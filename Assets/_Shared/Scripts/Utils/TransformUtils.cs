using UnityEngine;

namespace Shared
{
    public static class TransformUtils
    {
        public static void SetGlobalScale (this Transform transform, Vector3 globalScale)
        {
            transform.localScale = Vector3.one;
            transform.localScale = new Vector3 (globalScale.x/transform.lossyScale.x, globalScale.y/transform.lossyScale.y, globalScale.z/transform.lossyScale.z);
        }

        public static void ResetLocal(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            transform.localScale = Vector3.one;
        }
    }
}