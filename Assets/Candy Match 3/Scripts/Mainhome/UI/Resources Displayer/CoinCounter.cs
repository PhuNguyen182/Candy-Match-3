using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalScripts.Effects.Tweens;
using TMPro;

namespace CandyMatch3.Scripts.Mainhome.UI.ResourcesDisplayer
{
    public class CoinCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private Button shopButton;
        [SerializeField] private TweenValueEffect coinTweenEffect;

        private ReactiveProperty<int> _reactiveCoin = new();

        private void Awake()
        {
            coinTweenEffect.BindInt(_reactiveCoin, ShowCoins);
            shopButton.onClick.AddListener(OpenShop);
        }

        private void OpenShop()
        {

        }

        private void ShowCoins(int coins)
        {
            coinText.text = $"{coins}";
        }

        public void UpdateCoins(int coins)
        {
            _reactiveCoin.Value = coins;
        }
    }
}
