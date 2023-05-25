using System;
using System.Collections;
using System.Collections.Generic;
using ExtensionUtils;
using Shopify.Helpers;
using Shopify.Unity;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace StoreManagement.Components.ProductPanel
{
    public class ImageGallery : MonoBehaviour
    {
        [SerializeField] private Image imageHolder;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;

        [SerializeField] private Button prevBtn;
        [SerializeField] private Button nextBtn;

        [SerializeField] private RectTransform dotHolder;
        [SerializeField] private PageDot dotPrefab;

        private int currentIndex = 0;
        private List<Shopify.Unity.Image> images = new List<Shopify.Unity.Image>();
        private List<PageDot> dots = new List<PageDot>();
        private Coroutine imageFetchingRoutine;
        
        private void Awake()
        {
            prevBtn.onClick.AddListener(OnPrevBtnClicked);
            nextBtn.onClick.AddListener(OnNextBtnClicked);
        }

        #region Setup
        
        public void Configure(Product product, ProductVariant variant)
        {
            currentIndex = 0;
            images = (List<Shopify.Unity.Image>)product.images();
            SetupDots();
            SetVariantImage(variant);
        }

        private void SetupDots()
        {
            if(dots.Count == images.Count)
                return;
            
            foreach(var dot in dots)
                Destroy(dot.gameObject);
            dots.Clear();

            for (int i = 0; i < images.Count; i++)
            {
                var dot = Instantiate(dotPrefab, dotHolder);
                dot.GetRectTransform().ResetTransform();
                dots.Add(dot);
                dot.SetSelected(false);
                dot.gameObject.SetActive(true);
            }
        }

        private void OnPrevBtnClicked()
        {
            if(images.Count == 0)
                return;
            
            currentIndex--;
            if(currentIndex < 0)
                currentIndex = images.Count - 1;
            GetImage(currentIndex);
        }
        
        private void OnNextBtnClicked()
        {
            if(images.Count == 0)
                return;

            currentIndex++;
            if(currentIndex >= images.Count)
                currentIndex = 0;
            GetImage(currentIndex);
        }

        #endregion

        public void SetVariantImage(ProductVariant variant)
        {
            // Any variant that does NOT have an image specifically assigned in the Shopify dashboard will return the
            // first image in the product's image list. However, if you directly assigned that same image to the variant
            // it will return the exact same URL. There is no way to differentiate between the two scenarios.
            try {
                string imageSrc = variant.image().transformedSrc();
                for (int i = 0; i < images.Count; i++)
                {
                    if(images[i].transformedSrc() == imageSrc)
                    {
                        currentIndex = i;
                        GetImage(currentIndex);
                        return;
                    }
                }
            } catch (NullReferenceException)
            {
                // To be honest, I'm not sure what will throw this NRE. Shopify's example code checks for it, but I've never seen it.
                currentIndex = 0;
                GetImage(currentIndex);
            }
        }
        
        private void GetImage(int index)
        {
            if (imageFetchingRoutine != null)
            {
                StopCoroutine(imageFetchingRoutine);
            }
            
            SetDot(currentIndex);
            string imageSource = images[index].transformedSrc();
            imageFetchingRoutine = StartCoroutine(GetImage(imageSource));
        }
        
        private IEnumerator GetImage(string imageSource)
        {
            yield return StartCoroutine(ImageHelper.AssignImage(imageSource, imageHolder));
            
            int width = imageHolder.sprite.texture.width;
            int height = imageHolder.sprite.texture.height;
            aspectRatioFitter.aspectRatio = (float)width / (float)height;
            aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspectRatioFitter.enabled = true;
            
            imageFetchingRoutine = null;
        }
        
        private void SetDot(int index)
        {
            for (int i = 0; i < dots.Count; i++)
            {
                dots[i].SetSelected(i == index);
            }
        }
    }
}