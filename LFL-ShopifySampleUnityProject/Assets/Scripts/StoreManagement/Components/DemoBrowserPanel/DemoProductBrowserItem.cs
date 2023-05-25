using System;
using Shopify.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StoreManagement.Components.DemoBrowserPanel
{
    public class DemoProductBrowserItem : MonoBehaviour
    {
        public Action<Product> onClick;
        
        [SerializeField] private TMP_Text title;
        [SerializeField] private Button button;

        private Product product;
        
        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            onClick?.Invoke(product);
        }

        public void Configure(Product product)
        {
            this.product = product;
            title.text = product.title();
        }
    }
}