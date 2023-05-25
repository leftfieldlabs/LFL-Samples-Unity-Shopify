using System.Collections.Generic;
using ExtensionUtils;
using Shopify.Unity;
using StoreManagement.API;
using UnityEngine;

namespace StoreManagement.Components.DemoBrowserPanel
{
    public class DemoBrowserPanelController : MonoBehaviour
    {
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private DemoProductBrowserItem productBrowserItemPrefab;
        
        private List<DemoProductBrowserItem> productBrowserItems = new List<DemoProductBrowserItem>();


        private void Awake()
        {
            Store.SetupDemoStore();
            PopulateProducts();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            PopulateProducts();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void PopulateProducts()
        {
            void AddProductsToList(List<Product> products)
            {
                ClearProducts();
                // for each product in the list of products, instantiate a product browser item and add it to the list
                foreach (var product in products)
                {
                    // Debug.LogWarning($"ADDING ITEM: {product.id()}");
                    // Debug.Log($"Info: {product.title()} : {product.description()}");
                    var productBrowserItem = Instantiate(productBrowserItemPrefab, contentRoot);
                    productBrowserItem.gameObject.transform.AsRectTransform().ResetTransform();
                    
                    productBrowserItem.Configure(product);
                    productBrowserItem.onClick += ProductClicked;
                    productBrowserItems.Add(productBrowserItem);
                    
                    productBrowserItem.gameObject.SetActive(true);
                }
            }

            void ProductFetchFailed()
            {
                Debug.LogWarning("Product fetch failed");
            }
            
            Store.GetProductList(AddProductsToList, ProductFetchFailed, force:true);
        }

        private void ClearProducts()
        {
            foreach(var product in productBrowserItems)
                Destroy(product.gameObject);
            productBrowserItems.Clear();
        }

        private void ProductClicked(Product product)
        {
            StoreUIController.ShowProductPanel(product);
            Hide();
        }
    }
}