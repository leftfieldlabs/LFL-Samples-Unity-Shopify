using Shopify.Unity;
using StoreManagement.Components.CartPanel;
using StoreManagement.Components.DemoBrowserPanel;
using StoreManagement.Components.ProductPanel;
using UnityEngine;

namespace StoreManagement.Components
{
    public class StoreUIController : MonoBehaviour
    {
        private static StoreUIController Instance;

        [SerializeField] private DemoBrowserPanelController browserPanel;
        [SerializeField] private ProductPanelController productPanel;
        [SerializeField] private CartPanelController cartPanel;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"Multiple StoreUIControllers detected in scene. Destroying this GameObject: {gameObject.name}", gameObject);
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public static void ShowContentBrowser()
        {
            Instance.browserPanel.Show();
            HidePanels(product:true, cart:true);
        }
        
        public static void HideProductBrowser()
            => Instance.browserPanel.Hide();

        public static void ShowProductPanel()
        {
            Instance.productPanel.Show();
            HidePanels(browser:true, cart:true);
        }
        
        public static void ShowProductPanel(Product product)
        {
            Instance.productPanel.Show(product);
            HidePanels(browser:true, cart:true);
        }

        public static void ShowProductPanel(Product product, ProductVariant variant)
        {
            Instance.productPanel.Show(product, variant);
            HidePanels(browser:true, cart:true);
        }

        public static void HideProductPanel()
            => Instance.productPanel.Hide();

        public static void ShowCartPanel(bool backButtonEnabled = true)
        {
            Instance.cartPanel.Show(backButtonEnabled);
            HidePanels(browser:true, product:true);
        }
        
        public static void HideCartPanel() 
            => Instance.cartPanel.Hide();

        private static void HidePanels(bool browser = false, bool product = false, bool cart = false)
        {
            if (browser)
                Instance.browserPanel.Hide();
            if (product)
                Instance.productPanel.Hide();
            if (cart)
                Instance.cartPanel.Hide();
        }
    }
}