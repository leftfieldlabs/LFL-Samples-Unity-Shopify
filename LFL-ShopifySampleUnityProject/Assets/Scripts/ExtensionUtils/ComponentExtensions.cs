using UnityEngine;

namespace ExtensionUtils
{
    public static class ComponentExtensions
    {
        public static RectTransform GetRectTransform(this Component component)
            => component.transform.AsRectTransform();
    }
}