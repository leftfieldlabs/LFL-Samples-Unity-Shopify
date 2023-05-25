using UnityEngine;
using UnityEngine.Events;

namespace StoreManagement.Components.ProductPanel
{
    public class PageDot : MonoBehaviour
    {
        [SerializeField] private UnityEvent onSelected;
        [SerializeField] private UnityEvent onDeselected;
        
        public void SetSelected(bool selected)
        {
            if (selected)
            {
                onSelected?.Invoke();
            }
            else
            {
                onDeselected?.Invoke();
            }
        }
    }
}