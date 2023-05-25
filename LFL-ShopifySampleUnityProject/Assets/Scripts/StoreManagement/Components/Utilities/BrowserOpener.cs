using UnityEngine;
using UnityEngine.UI;

namespace StoreManagement.Components.Utilities
{
    public class BrowserOpener : MonoBehaviour
    {
        [SerializeField] private Button button;

        private void Awake()
        {
            button.onClick.AddListener(CartButtonClicked);
        }

        private void CartButtonClicked()
        {
            StoreUIController.ShowContentBrowser();
        }
    }
}