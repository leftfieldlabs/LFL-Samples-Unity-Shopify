using UnityEngine;

namespace ExtensionUtils
{
    public static class TransformExtensions
    {
        public static RectTransform AsRectTransform(this Transform transform)
            => transform as RectTransform;
        
        public static void ResetTransform(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}