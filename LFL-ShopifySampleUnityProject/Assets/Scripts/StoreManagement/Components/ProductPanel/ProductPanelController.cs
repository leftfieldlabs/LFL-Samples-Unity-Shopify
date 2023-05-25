using System.Collections.Generic;
using ExtensionUtils;
using Shopify.Unity;
using StoreManagement.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StoreManagement.Components.ProductPanel
{
    public class ProductPanelController : MonoBehaviour
    {
        [SerializeField] private RawImage brandIcon;
        [SerializeField] private ImageGallery imageGallery;

        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text description;

        [SerializeField] private TMP_Dropdown quantityDropdown;
        [SerializeField] private GameObject soldOutWarning;

        [SerializeField] private RectTransform variantOptionsRoot;
        [SerializeField] private VariantDropdown variantDropdownPrefab;
        private List<VariantDropdown> variantDropdowns = new List<VariantDropdown>();

        [SerializeField] private TMP_Text currencySymbol;
        [SerializeField] private TMP_Text priceWhole;
        [SerializeField] private TMP_Text priceFraction;

        [SerializeField] private Button addToCartBtn;
        [SerializeField] private Button buyNowBtn;
        [SerializeField] private Button closeBtn;

        // Even though 'currentProductVariant' contains a reference to its parent product, that value is not pulled by default
        // by the Shopify SDK in its GraphQL miasma and I don't know how to fix that.
        private Product currentProduct;
        private ProductVariant currentProductVariant;


        private void Awake()
        {
            addToCartBtn.onClick.AddListener(OnAddToCartBtnClicked);
            buyNowBtn.onClick.AddListener(OnBuyNowBtnClicked);
            closeBtn.onClick.AddListener(Hide);
            quantityDropdown.onValueChanged.AddListener(OnQuantityDropdownValueChanged);
        }
        
        /// <summary> This function will do nothing if there is no previous product. </summary>
        public void Show()
        {
            if (currentProduct == null)
                return;
            
            if (currentProductVariant == null)
                Show(currentProduct);
            else
                Show(currentProduct, currentProductVariant);
        }
        
        public void Show(Product product)
        {
            gameObject.SetActive(true);
            
            var variants = (List<ProductVariant>)product.variants();
            ConfigureUI(product, variants[0]);
        }

        public void Show(Product product, ProductVariant variant)
        {
            gameObject.SetActive(true);
            ConfigureUI(product, variant);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnBuyNowBtnClicked()
        {
            var variant = GetVariantFromOptionCombination();
            int quantity = GetQuantity();
            Store.OneShotCheckout(variant, quantity);
        }

        private void OnAddToCartBtnClicked()
        {
            var variant = GetVariantFromOptionCombination();
            int quantity = GetQuantity();
            
            if(variant == null)
                return;
            
            Store.AddToCart(variant,quantity);
            StoreUIController.ShowCartPanel();
        }

        private void ConfigureUI(Product product, ProductVariant variant)
        {
            currentProduct = product;
            currentProductVariant = variant;
            
            title.text = product.title();
            description.text = product.description();
            
            ConfigureDropdownValues();
            imageGallery.Configure(product, variant);
            ConfigurePriceDisplay();
            CheckVariantAvailability();
        }
        
        private void OnQuantityDropdownValueChanged(int newQuantity)
        {
            ConfigurePriceDisplay();
        }
        
        private void ConfigureDropdownValues()
        {
            // Reset the Quantity dropdown
            quantityDropdown.ClearOptions();
            for(int i = Store.MinQuantity; i <= Store.MaxQuantity; i++)
            {
                quantityDropdown.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
            }
            quantityDropdown.SetValueWithoutNotify(0);
            quantityDropdown.RefreshShownValue();
            
            // Delete the old variant dropdowns
            foreach(var dropdown in variantDropdowns)
                Destroy(dropdown.gameObject);
            variantDropdowns.Clear();
            
            // Create new variant dropdowns
            var options = currentProduct.options();
            foreach(var opt in options)
            {
                var dropdown = Instantiate(variantDropdownPrefab, variantOptionsRoot);
                dropdown.GetRectTransform().ResetTransform();
                dropdown.Configure(opt.name(), opt.values());
                dropdown.onValueChanged += OnVariantDropdownValueChanged;
                variantDropdowns.Add(dropdown);
                dropdown.gameObject.SetActive(true);
                
                int variantValue = GetDropdownValueFromVariantOption(opt.name(), dropdown);
                if(variantValue != -1)
                    dropdown.SetValueWithoutNotify(variantValue);
            }
        }
        
        private void OnVariantDropdownValueChanged(string variantName, int value)
        {
            var newVariant = GetVariantFromOptionCombination();
            if(newVariant == null)
                return;
            
            currentProductVariant = newVariant;
            ConfigurePriceDisplay();
            imageGallery.SetVariantImage(currentProductVariant);
            CheckVariantAvailability();
        }

        private void CheckVariantAvailability()
        {
            bool isAvailable = currentProductVariant.availableForSale();
            soldOutWarning.SetActive(!isAvailable);
            addToCartBtn.interactable = isAvailable;
            buyNowBtn.interactable = isAvailable;
        }
        
        private int GetDropdownValueFromVariantOption(string optionName, VariantDropdown dropdown)
        {
            var options = currentProductVariant.selectedOptions();
            foreach(var option in options)
            {
                if(option.name() == optionName)
                    return dropdown.GetIndexOfOption( option.value() );
            }
            return -1;
        }

        private ProductVariant GetVariantFromOptionCombination()
        {
            var optionDictionary = new Dictionary<string, string>();
            foreach(var dropdown in variantDropdowns)
            {
                optionDictionary.Add(dropdown.Title, dropdown.Value);
            }

            var variantList = (List<ProductVariant>)currentProduct.variants();
            if (variantList.Count == 1)
                return variantList[0];
            return Store.GetVariantFromOptionCombination(variantList, optionDictionary);
        }

        private void ConfigurePriceDisplay()
        {
            decimal totalPrice = currentProductVariant.priceV2().amount() * GetQuantity();
            
            string fullPriceText = totalPrice.ToString("C");
            string currencySymbolText = fullPriceText[0].ToString();
            string[] priceNumber = fullPriceText[1..].Split('.');
            
            currencySymbol.text = currencySymbolText;
            priceWhole.text = priceNumber[0];
            priceFraction.text = priceNumber[1];
        }
        
        private int GetQuantity()
        {
            return quantityDropdown.value + 1;
        }

        private void Clear()
        {
            currentProduct = null;
            currentProductVariant = null;
            
            // No reason to clear the text and all that, it's not important to the state.
        }
    }
}