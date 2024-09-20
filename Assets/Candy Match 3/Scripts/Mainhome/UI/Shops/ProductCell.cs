using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.Controllers;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.GameData;
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
            int amount = _productInfo.Amount;
            GameDataManager.Instance.EarnResource(GameResourceType.Coin, amount);

            OnPurchase?.Invoke();
            UpdateResourceController.Instance?.UpdateResource(new UpdateResourceMessage
            {
                ResouceType = GameResourceType.Coin
            });
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
