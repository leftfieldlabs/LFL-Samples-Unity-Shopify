using Shopify.Unity;
using StoreManagement.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StoreManagement.Components.Utilities
{
    public class CartOpener : MonoBehaviour
    {
        [SerializeField] private TMP_Text cartCountDisplay;
        [SerializeField] private Button cartButton;

        private void Awake()
        {
            cartButton.onClick.AddListener(CartButtonClicked);
            Store.cartUpdated += UpdateCartCountDisplay;
        }

        private void OnDestroy()
        {
            Store.cartUpdated -= UpdateCartCountDisplay;
        }

        private void CartButtonClicked()
        {
            StoreUIController.ShowCartPanel(backButtonEnabled:false);
        }

        private void UpdateCartCountDisplay(Cart cart, string cartID)
        {
            cartCountDisplay.text = Store.GetCartQuantity(cartID).ToString();
        }
    }
}