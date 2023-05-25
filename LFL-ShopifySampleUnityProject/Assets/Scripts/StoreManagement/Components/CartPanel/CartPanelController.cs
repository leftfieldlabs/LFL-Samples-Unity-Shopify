using System.Collections.Generic;
using ExtensionUtils;
using Shopify.Unity;
using Shopify.Unity.SDK;
using StoreManagement.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StoreManagement.Components.CartPanel
{
    public class CartPanelController : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button checkoutButton;

        [SerializeField] private GameObject emptyCartNotification;
        
        [SerializeField] private TMP_Text numItemsDisplay;
        private string numItemsString = "({0} items):";

        [SerializeField] private TMP_Text currencySymbol;
        [SerializeField] private TMP_Text priceWhole;
        [SerializeField] private TMP_Text priceFraction;

        [SerializeField] private RectTransform scrollRoot;
        [SerializeField] private CartItem cartItemPrefab;
        private List<CartItem> cartItems = new List<CartItem>();
        
        
        private void Awake()
        {
            backButton.onClick.AddListener(Back);
            closeButton.onClick.AddListener(Hide);
            checkoutButton.onClick.AddListener(OnCheckoutBtnClicked);
            UpdateQuantityDependentElements(null, null);
            
            Store.clearCartContent += ClearCartItems;
            Store.purchaseComplete += PurchaseComplete;
            Store.cartUpdated += UpdateQuantityDependentElements;
        }

        private void OnDestroy()
        {
            Store.clearCartContent -= ClearCartItems;
            Store.purchaseComplete -= PurchaseComplete;
            Store.cartUpdated -= UpdateQuantityDependentElements;
        }

        public void Show(bool backButtonEnabled = true)
        {
            if(gameObject.activeInHierarchy)
                return;
            
            gameObject.SetActive(true);
            backButton.gameObject.SetActive(backButtonEnabled);
            PopulateCartItems();
            UpdateQuantityDependentElements(null, null);
        }

        public void Back()
        {
            StoreUIController.ShowProductPanel();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnCheckoutBtnClicked()
        {
            void Success(string checkoutID)
            {
                Debug.Log($"Checkout invoked: {checkoutID}");
            }
            
            void Fail(ShopifyError error)
            {
                Debug.Log($"Checkout failed! Error: {error.Type}, {error.Description}");
            }

            Store.CheckOut(Success, Fail);
        }
        
        private void PopulateCartItems()
        {
            ClearCartItems(); 
            var cart = Store.GetCart();
            foreach (var item in cart.LineItems.All())
            {
                CreateCartItem(item);
            }
            
            UpdatePrice();
            UpdateSeparators();
        }
        
        private void CreateCartItem(CartLineItem cartItem)
        {
            string itemID = Store.SplitItemIDNumberFromFullID(cartItem.VariantId);
            if (Store.HasVariant(itemID, out var parentProduct, out var productVariant))
            {
                AddItemToList(parentProduct, productVariant, (int)cartItem.Quantity);
            }
        }

        private void AddItemToList(Product product, ProductVariant variant, int quantity)
        {
            var newItem = Instantiate(cartItemPrefab, scrollRoot);
            newItem.GetRectTransform().ResetTransform();
            newItem.gameObject.SetActive(true);
            
            newItem.Configure(product, variant, quantity);

            newItem.OnRemoveClicked += OnCartItemRemoveClicked;
            newItem.OnQuantityChanged += OnCartItemQuantityChanged;
                            
            cartItems.Add(newItem);
        }
        
        private void ClearCartItems()
        {
            foreach(var item in cartItems)
                Destroy(item.gameObject);
            cartItems.Clear();
            
            UpdatePrice();
        }

        private void PurchaseComplete(string checkoutID)
        {
            var cart = Store.GetCart();
            if (cart.CurrentCheckout != null && cart.CurrentCheckout.id() == checkoutID)
            {
                Store.ClearCart();
                Hide();
            }
        }

        private void OnCartItemRemoveClicked(CartItem item, Product product, ProductVariant variant)
        {
            cartItems.Remove(item);
            Destroy(item.gameObject);

            Store.RemoveFromCart(variant);

            UpdatePrice();
            UpdateSeparators();
            
            if(Store.GetCartQuantity() == 0)
                Store.ClearCart();
        }
        
        private void OnCartItemQuantityChanged(CartItem item, Product product, ProductVariant variant, int newQuantity)
        {
            var cart = Store.GetCart();
            cart.LineItems.AddOrUpdate(variant, newQuantity);
            UpdatePrice();
            Store.CartUpdated();
        }

        private void UpdatePrice()
        {
            var cart = Store.GetCart();
            decimal totalPrice = cart.Subtotal();
            
            string fullPriceText = totalPrice.ToString("C");
            string currencySymbolText = fullPriceText[0].ToString();
            string[] priceNumber = fullPriceText[1..].Split('.');
            
            currencySymbol.text = currencySymbolText;
            priceWhole.text = priceNumber[0];
            priceFraction.text = priceNumber[1];

            
            int totalItems = Store.GetCartQuantity();
            numItemsDisplay.text = string.Format(numItemsString, totalItems);
        }

        private void UpdateSeparators()
        {
            foreach (var item in cartItems)
            {
                item.SetSeparatorActive(true);
            }

            if (cartItems.Count > 0)
            {
                cartItems[^1].SetSeparatorActive(false);
            }
        }

        private void UpdateQuantityDependentElements(Cart _, string __)
        {
            bool cartIsEmpty = Store.GetCartQuantity() <= 0;
            checkoutButton.interactable = !cartIsEmpty;
            emptyCartNotification.SetActive(cartIsEmpty);
        }
    }
}