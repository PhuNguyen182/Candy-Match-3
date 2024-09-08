using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalScripts.Effects.Tweens;
using TMPro;

namespace CandyMatch3.Scripts.Gameplay.GameUI.EndScreen
{
    public class ContinuePopup : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;
        
        [Space(10)]
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text moveMessage;
        [SerializeField] private TMP_Text moveText;
        [SerializeField] private TMP_Text currentCoin;

        [Space(10)]
        [SerializeField] private TweenValueEffect coinTween;
        [SerializeField] private Animator popupAnimator;

        private int _price = 0;
        private ReactiveProperty<int> _reactiveCoin = new(0);
        private readonly int _closeHash = Animator.StringToHash("Close");

        private void Awake()
        {
            playButton.onClick.AddListener(OnPlayClicked);
            quitButton.onClick.AddListener(OnQuitClicked);

            coinTween.BindInt(_reactiveCoin, UpdateCoin);
            // _reactiveCoin.Value = 0; update letest value from shop
        }

        public void SetPrice(int price)
        {
            _price = price;
            priceText.text = $"{price}";
        }

        public void SetMove(int move)
        {
            moveText.text = $"{move}";
            moveMessage.text = $"Add +{move} extra moves to continue.";
        }

        private void OnPlayClicked()
        {

        }

        private void OnQuitClicked()
        {

        }

        private void UpdateCoin(int coin)
        {
            _reactiveCoin.Value = coin;
        }

        private void ShowCoin(int coin)
        {
            currentCoin.text = $"{coin:N1, ru-RU}";
        }
    }
}
