using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Common.DataStructs;
using Sirenix.OdinInspector;
using TMPro;

namespace CandyMatch3.Scripts.Mainhome.UI.Shops
{
    public class ProductCell : MonoBehaviour
    {
        [ReadOnly]
        [SerializeField] private string productId;
        [SerializeField] private Image productIcon;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text coinText;

        private ProductInfo _productInfo;
        public string ProductID => productId;
        public Action OnPurchase;

        private void Awake()
        {
            purchaseButton.onClick.AddListener(Purchase);
        }

        private void Purchase()
        {
            // To do: update value on purchase
            OnPurchase?.Invoke();
        }

        public void SetProductInfo(ProductInfo productInfo)
        {
            _productInfo = productInfo;

            productId = _productInfo.ProductID;
            productIcon.sprite = _productInfo.ProductIcon;
            priceText.text = $"{_productInfo.DefaultPrice:F2}";
            coinText.text = $"{_productInfo.Amount}";
        }
    }
}
