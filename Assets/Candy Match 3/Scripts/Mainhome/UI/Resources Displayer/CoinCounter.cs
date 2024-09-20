using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalScripts.Effects.Tweens;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Mainhome.UI.Shops;
using CandyMatch3.Scripts.Common.Messages;
using Cysharp.Threading.Tasks;
using TMPro;

namespace CandyMatch3.Scripts.Mainhome.UI.ResourcesDisplayer
{
    public class CoinCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private Button shopButton;
        [SerializeField] private UpdateResouceResponder resouceResponder;
        [SerializeField] private TweenValueEffect coinTweenEffect;

        private ReactiveProperty<int> _reactiveCoin = new();

        private void Awake()
        {
            PreloadPopup();
            coinTweenEffect.BindInt(_reactiveCoin, ShowCoins);
            shopButton.onClick.AddListener(OpenShop);

            resouceResponder.OnUpdate = () =>
            {
                // To do: update coin value here
            };
        }

        private void PreloadPopup()
        {
            ShopPopup.PreloadFromAddress(CommonPopupPaths.ShopPopupPath).Forget();
        }

        private void OpenShop()
        {
            ShopPopup.CreateFromAddress(CommonPopupPaths.ShopPopupPath).Forget();
        }

        private void ShowCoins(int coins)
        {
            coinText.text = $"{coins}";
        }

        public void UpdateCoins(int coins)
        {
            _reactiveCoin.Value = coins;
        }

        private void OnDestroy()
        {
            ShopPopup.Release();
        }
    }
}
