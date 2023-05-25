using System;
using System.Collections;
using System.Collections.Generic;
using Shopify.Helpers;
using Shopify.Unity;
using StoreManagement.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace StoreManagement.Components.CartPanel
{
    public class CartItem : MonoBehaviour
    {
        public Action<CartItem, Product, ProductVariant> OnRemoveClicked;
        public Action<CartItem, Product, ProductVariant, int> OnQuantityChanged;
        
        [SerializeField] private TMP_Text title;
        [SerializeField] private Button removeBtn;
        [SerializeField] private Image thumbnailImage;
        [SerializeField] private TMP_Dropdown quantityDropdown;
        [SerializeField] private TMP_Text priceDisplay;
        [SerializeField] private GameObject separator;

        private Product product;
        private ProductVariant variant;
        private Coroutine imageFetchingRoutine;
        
        public int Quantity => GetQuantity();
        public decimal TotalPrice => variant.priceV2().amount() * Quantity;


        private void Awake()
        {
            removeBtn.onClick.AddListener(OnRemoveBtnClicked);
            quantityDropdown.onValueChanged.AddListener(OnQuantityDropdownValueChanged);
            
            AddDropdownValues();
        }

        private void OnQuantityDropdownValueChanged(int newQuantity)
        {
            UpdatePriceDisplay();
            OnQuantityChanged?.Invoke(this, product, variant, newQuantity + 1);
        }

        private void OnRemoveBtnClicked()
        {
            OnRemoveClicked?.Invoke(this, product, variant);
        }
        
        private void AddDropdownValues()
        {
            quantityDropdown.ClearOptions();
            for(int i = Store.MinQuantity; i <= Store.MaxQuantity; i++)
            {
                quantityDropdown.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
            }
            quantityDropdown.SetValueWithoutNotify(0);
        }

        public void Configure(Product product, ProductVariant variant, int quantity)
        {
            this.product = product;
            this.variant = variant;
            title.text = $"{product.title()} : {variant.title()}";
            quantityDropdown.SetValueWithoutNotify(quantity-1);
            quantityDropdown.RefreshShownValue();
            UpdatePriceDisplay();
            GetImage();
        }

        private void UpdatePriceDisplay()
        {
            decimal totalPrice = variant.priceV2().amount() * GetQuantity();
            string priceText = totalPrice.ToString("C");
            priceDisplay.text = priceText;
        }
        
        
        private void GetImage()
        {
            if (imageFetchingRoutine != null)
            {
                StopCoroutine(imageFetchingRoutine);
            }

            string imageSrc = null;
            if (variant != null)
            {
                try
                {
                    imageSrc = variant.image().transformedSrc();
                } catch (Exception)
                {
                    // ignored
                }
            }
            
            if(string.IsNullOrEmpty(imageSrc))
            {
                imageSrc = ((List<Shopify.Unity.Image>)product.images())[0].transformedSrc();
            }
            
            
            imageFetchingRoutine = StartCoroutine(GetImage(imageSrc));
        }
        
        private IEnumerator GetImage(string imageSource)
        {
            yield return StartCoroutine(ImageHelper.AssignImage(imageSource, thumbnailImage));
            imageFetchingRoutine = null;
        }
        
        public int GetQuantity()
        {
            return quantityDropdown.value + 1;
        }
        
        public void SetQuantity(int newQuantity)
        {
            if (newQuantity < Store.MinQuantity)
            {
                OnRemoveClicked?.Invoke(this, product, variant);
                return;
            }

            if (newQuantity > Store.MaxQuantity)
                newQuantity = Store.MaxQuantity;
            
            quantityDropdown.SetValueWithoutNotify(newQuantity-1);
        }

        public void SetSeparatorActive(bool active)
        {
            separator.SetActive(active);
        }
    }
}