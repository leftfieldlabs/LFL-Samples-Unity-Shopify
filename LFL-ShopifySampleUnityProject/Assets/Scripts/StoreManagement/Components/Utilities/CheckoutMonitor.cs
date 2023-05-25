using Shopify.Unity;
using StoreManagement.API;
using UnityEngine;

namespace StoreManagement.Components.Utilities
{
    public class CheckoutMonitor : MonoBehaviour
    {
        // I save the ID and URL because multiple checkouts could be in flight at the same time. There is no guarantee
        // of a 1-to-1 relationship.
        private string checkoutID;

        public void Configure(string checkoutID)
        {
            this.checkoutID = checkoutID;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if(!hasFocus)
                return;

            if (string.IsNullOrEmpty(checkoutID))
            {
                Destroy(this.gameObject);
                return;
            }
            
            Store.QueryCheckoutStatus(checkoutID, CheckoutResult, CheckoutError);
        }

        private void CheckoutResult(bool completed)
        {
            Debug.LogWarning($"CheckoutResult: {completed}");
            // True means the checkout was completed. False means it is still pending, but not expired yet.
            if (completed)
            {
                Store.PurchaseComplete(checkoutID);
                Destroy(this.gameObject);
            }
        }

        private void CheckoutError(ShopifyError error)
        {
            Debug.LogWarning($"CheckoutError: {error.Type}, {error.Description}");
            
            if (error.Type == ShopifyError.ErrorType.GraphQL)
            {
                // This call doesn't have other ways of generating a GraphQL error as far as I can tell.
                // The description would be: "Invalid global id 'xyz'"
                // This means that the checkout ID was abandoned, invalidated, or otherwise expired.
                Destroy(this.gameObject);
            }
        }
    }
}