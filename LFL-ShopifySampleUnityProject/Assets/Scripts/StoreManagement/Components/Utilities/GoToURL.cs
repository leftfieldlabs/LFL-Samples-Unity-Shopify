using UnityEngine;
using UnityEngine.EventSystems;

namespace StoreManagement.Components.Utilities
{
    public class GoToURL : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private string urlTarget = "https://www.leftfieldlabs.com/";
        
        public void OnPointerClick(PointerEventData eventData)
        {
            Application.OpenURL(urlTarget);
        }
    }
}