using System;
using System.Collections.Generic;
using System.Linq;
using Shopify.Helpers;
using Shopify.Unity;
using Shopify.Unity.GraphQL;
using Shopify.Unity.SDK;
using StoreManagement.Components.Utilities;
using UnityEngine;
using Product = Shopify.Unity.Product;

namespace StoreManagement.API
{
    public static class Store
    {
        public static event Action clearCartContent;
        /// <summary> Fires when a cart item is added/removed. The string param is the cart ID </summary>
        public static event Action<Cart, string> cartUpdated;
        /// <summary> Manually fired when a purchase status query succeeds. The string param is the checkout ID </summary>
        public static event Action<string> purchaseComplete;
        
        public const int MaxQuantity = 10;
        public const int MinQuantity = 1;

        public static bool IsInitialized { get; private set; } = false;
        public static List<Product> productList = null;

        private static string domain;
        private static string accessToken;
        private static string locale;

        
        public static void SetupDemoStore()
        {
            // TEMP - demo store public credentials
            Setup(
                "ir-demo-store.myshopify.com",
                "a74be9aac32c42546bff330925c3e61f",
                ""
            );
            GetProductList(null,null,false);
        }

        public static void Setup(string newDomain, string newAccessToken, string newLocale)
        {
            domain = newDomain;
            accessToken = newAccessToken;
            locale = newLocale;
            
            ShopifyHelper.Init(accessToken, domain, locale);
            IsInitialized = true;
            
            GetProductList(null,null,false);
        }

        /// <summary> I THINK you need to pull the product list and rebuild the UI after doing this. TBD. </summary>
        public static void ChangeLocale(string newLocale)
        {
            if (NotInitialized())
                return;
            ShopifyHelper.UpdateLocale(newLocale, domain);
        }

        public static ShopifyClient GetClient()
        {
            if (NotInitialized())
                return null;
            return ShopifyBuy.Client();
        }
        
        
        #region Product List
        
        /// <summary>
        /// Returns the existing product list if it has already been fetched, or fetches it if not. Pass the 'force'
        /// param to force a fetch. The 'Setup' function must have already been called.
        /// </summary>
        public static void GetProductList(Action<List<Product>> successCallback, Action failureCallback, bool force = false)
        {
            if(NotInitialized())
            {
                failureCallback?.Invoke();
                return;
            }
            
            if(productList != null && !force)
            {
                successCallback(productList);
                return;
            }
            
            void OnSuccess(List<Product> products, string cursor)
            {
                productList = products;
                //DebugProducts(products);
                successCallback?.Invoke(products);
            }

            void OnFailure()
            {
                Debug.LogWarning($"Fetching products from Shopify failed.");
                failureCallback?.Invoke();
            }

            ShopifyHelper.FetchProducts(OnSuccess, OnFailure);
        }

        /// <summary>
        /// Finds the first instance of a product with the given ID. Outputs the product. Methods 'Setup' and 'GetProductList'
        /// must have already been called. ID is expected in the format: "8294890668313". The full format of an ID is
        /// "gid://shopify/Product/8294890668313", but there is no reason to bother the user for all of that.
        /// </summary>
        public static bool HasProduct(string productID, out Product product)
        {
            if(NotInitialized())
            {
                product = null;
                return false;
            }
            
            string idToFind = $"gid://shopify/Product/{productID}";
            product = productList.FirstOrDefault(p => p.id() == idToFind);
            return product != null;
        }

        /// <summary> Finds the first instance of a variant of any product with the given variant ID. Outputs the variant
        /// and its parent product. Methods 'Setup' and 'GetProductList' must have already been called.</summary>
        public static bool HasVariant(string variantID, out Product parentProduct, out ProductVariant productVariant)
        {
            if(NotInitialized())
            {
                parentProduct = null;
                productVariant = null;
                return false;
            }
            
            string idToFind = $"gid://shopify/ProductVariant/{variantID}";
            foreach (var product in productList)
            {
                var variants = (List<ProductVariant>) product.variants();
                foreach (var variant in variants)
                {
                    if(variant.id() == idToFind)
                    {
                        parentProduct = product;
                        productVariant = variant;
                        return true;
                    }
                }
            }

            parentProduct = null;
            productVariant = null;
            return false;
        }

        /// <summary> Finds and outputs the specified variant of the specified product if it exists in the local
        /// product list. Methods 'Setup' and 'GetProductList' must have already been called. </summary>
        public static bool HasVariant(string productID, string variantID, out ProductVariant variant)
        {
            if (HasProduct(productID, out var baseProduct))
            {
                string idToFind = $"gid://shopify/ProductVariant/{variantID}";
                var variants = (List<ProductVariant>)baseProduct.variants();
                variant = variants.FirstOrDefault(v => v.id() == idToFind);
                return variant != null;
            }

            variant = null;
            return false;
        }

        #endregion
        // ----- End Product List -----
        
        
        #region CART

        /// <summary> Returns the specified cart of the default client. </summary>
        public static Cart GetCart(string optionalID = null)
        {
            if (NotInitialized())
                return null;
            return ShopifyBuy.Client().Cart(optionalID);
        }
        
        /// <summary> Adds a product variant to the cart while respecting the configured Min/Max limits. </summary>
        public static void AddToCart(ProductVariant variant, int quantity, string cartID=null)
        {
            if (NotInitialized())
                return;

            var cart = GetCart(cartID);
            var existingLineItem = cart.LineItems.Get(variant);
            
            if (existingLineItem != null)
            {
                quantity += (int)existingLineItem.Quantity;
                quantity = Math.Clamp(quantity, MinQuantity, MaxQuantity);
            }

            cart.LineItems.AddOrUpdate(variant, quantity);
            CartUpdated(cartID);
        }
        
        /// <summary> Removes the request amount of the specified variant from the cart. If the quantity param is less
        /// than 0, all of that variant will be removed. </summary>
        public static void RemoveFromCart(ProductVariant variant, int quantityToRemove=-1, string cartID=null)
        {
            if (NotInitialized())
                return;
            
            var cart = GetCart(cartID);
            if (quantityToRemove > 0)
            {
                var lineItem = cart.LineItems.Get(variant);
                if (lineItem != null)
                {
                    int itemQuantity = (int)lineItem.Quantity;
                    if(quantityToRemove >= itemQuantity)
                        cart.LineItems.Delete(variant);
                    else
                        cart.LineItems.AddOrUpdate(variant, itemQuantity - quantityToRemove);
                }
            }
            else
            {
                cart.LineItems.Delete(variant);
            }
            
            CartUpdated(cartID);
        }

        /// <summary> Clears the active cart. If you don't do this, sequential checkouts can conflict and cause errors. </summary>
        public static void ClearCart(string cartID = null)
        {
            var cart = GetCart(cartID);
            cart.Reset();
            clearCartContent?.Invoke();
            CartUpdated(cartID);
        }

        /// <summary> Call this if you externally modify the contents of a cart that you want the system to know about. </summary>
        public static void CartUpdated(string cartID = null)
        {
            var cart = GetCart(cartID);
            cartUpdated?.Invoke(cart, cartID);
        }

        /// <summary> Returns the total number of items in the specified cart. </summary>
        public static int GetCartQuantity(string cartID = null)
        {
            var cart = GetCart(cartID);
            var lineItems = cart.LineItems.All();
            
            int total = 0;
            foreach(var item in lineItems)
                total += (int)item.Quantity;
            return total;
        }

        #endregion
        // ----- End Cart -----
        
        
        #region Checkout

        /// <summary>
        /// Opens the checkout via OpenURL using the default cart. Shopify explicitly prevents you from opening the checkout
        /// in an IFrame for security reasons. The onSuccess callback will be passed the checkout ID if the checkout is successfully invoked.
        /// </summary>
        public static void CheckOut(Action<string> onSuccess, Action<ShopifyError> onError)
            => CheckOut(null, onSuccess, onError);
        
        /// <summary>
        /// Opens the checkout via OpenURL using the specified cart. Shopify explicitly prevents you from opening the checkout
        /// in an IFrame for security reasons. The onSuccess callback will be passed the checkout ID if the checkout is successfully invoked.
        /// </summary>
        public static void CheckOut(string cartID, Action<string> onSuccess, Action<ShopifyError> onError)
        {
            var cart = GetCart(cartID);
            cart.GetWebCheckoutLink(
                (checkoutLink) =>
                {
                    string checkoutID = cart.CurrentCheckout.id();
                    CreateCheckoutMonitor(checkoutID);
                    Application.OpenURL(checkoutLink);
                    onSuccess?.Invoke(checkoutID);
                },
                (checkoutError) =>
                {
                    onError?.Invoke(checkoutError);
                }
            );
        }

        /// <summary> Use this to immediately open a checkout for the specific item selected. </summary>
        public static void OneShotCheckout(ProductVariant variant, int quantity)
        {
            string cartID = "BuyNowCart";
            var cart = GetCart(cartID);
            cart.LineItems.AddOrUpdate(variant, quantity);
            
            void CheckoutSuccess(string checkoutID)
            {
                Debug.LogWarning($"CHECKOUT SUCCESS: {checkoutID}");
            }

            void CheckoutError(ShopifyError error)
            {
                Debug.LogWarning($"CHECKOUT ERROR: {error.Description}");
            }
            
            CheckOut(cartID, CheckoutSuccess, CheckoutError);
        }

        /// <summary> Creates an object that will ping Shopify for the status of the checkout with the given ID. </summary>
        private static void CreateCheckoutMonitor(string idToMonitor)
        {
            var go = new GameObject("CheckoutMonitor");
            var monitor = go.AddComponent<CheckoutMonitor>();
            monitor.Configure(idToMonitor);
        }
        
        /// <summary> Pings Shopify for the status of the checkout with the given ID. </summary>
        public static void QueryCheckoutStatus(string checkoutID, Action<bool> resultCallback, Action<ShopifyError> errorCallback)
        {
            var query = new QueryRootQuery();
            DefaultQueries.checkout.Completed(query, checkoutID);
            
            ShopifyBuy.Client().Query(query, (response, error) => {
                if (error != null) {
                    errorCallback?.Invoke(error);
                } else {
                    var checkout = (Checkout) response.node();
                    bool isComplete = checkout.completedAt() != null;
                    resultCallback?.Invoke(isComplete);
                }
            });
        }
        
        /// <summary> Fires the PurchaseComplete event with the checkoutID of the purchase that completed. </summary>
        public static void PurchaseComplete(string checkoutID)
        {
            purchaseComplete?.Invoke(checkoutID);
        }
        
        #endregion
        // ----- End Checkout -----

        
        #region Utilities
        
        /// <summary> Searches the list of ProductVariants for one that matches the set of options provided. Returns null if no match is found. </summary>
        public static ProductVariant GetVariantFromOptionCombination(List<ProductVariant> variantList, Dictionary<string, string> options)
        {
            foreach(var variant in variantList)
            {
                bool allOptionsMatch = true;
                foreach(var key in options.Keys)
                {
                    if (!DoesVariantContainOptionWithValue(variant, key, options[key]))
                    {
                        allOptionsMatch = false;
                        break;
                    }
                }

                if (allOptionsMatch)
                    return variant;
            }

            return null;
        }

        /// <summary> Checks a specific variant for a specific option and value. </summary>
        private static bool DoesVariantContainOptionWithValue(ProductVariant variant, string optionName, string optionValue)
        {
            var options = variant.selectedOptions();
            foreach (var option in options)
            {
                if(option.name() == optionName && option.value() == optionValue)
                    return true;
            }
            return false;
        }
        
        /// <summary> Extracts the Product/Variant ID from the full URL so we can have the users only deal with the numerical ID. </summary>
        public static string SplitItemIDNumberFromFullID(string fullID)
        {
            return fullID[(fullID.LastIndexOf('/')+1)..];
        }
        
        private static bool NotInitialized()
        {
            if (IsInitialized)
                return false;
            
            Debug.LogError($"Shopify has not been initialized. Call Store.Setup() first.");
            return true;
        }

        #endregion
        // ----- End Misc Utils -----
        
        
        #region DEBUG FUNCTIONS
        
        private static void DebugProducts(List<Product> products)
        {
            foreach (var product in products) {
                Debug.LogWarning($"Product ID: {product.id()}");

                var options = product.options();
                foreach (var opt in options)
                {
                    Debug.LogWarning($"Option: {opt.name()}");
                    foreach (var val in opt.values())
                    {
                        Debug.Log($"Val: {val}");
                    }
                }
                
                var variants = (List<ProductVariant>)product.variants();
                Debug.LogWarning($"Variants: {variants.Count}");
                foreach (var variant in variants) {
                    Debug.Log($"ID: {variant.id()}");
                }
            }
        }
        
        #endregion
    }
}