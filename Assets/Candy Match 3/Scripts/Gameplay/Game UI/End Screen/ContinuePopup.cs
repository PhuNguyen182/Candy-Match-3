using R3;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalScripts.Effects.Tweens;
using CandyMatch3.Scripts.Gameplay.GameUI.Popups;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private QuitPopup quitPopup;

        private int _price = 0;
        private ReactiveProperty<int> _reactiveCoin = new(0);

        private CancellationToken _token;
        private UniTaskCompletionSource<bool> _completionSource;
        private readonly int _closeHash = Animator.StringToHash("Close");

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();

            playButton.onClick.AddListener(() => OnPlayClicked().Forget());
            quitButton.onClick.AddListener(() => OnQuitClicked().Forget());

            coinTween.BindInt(_reactiveCoin, UpdateCoin);
            // _reactiveCoin.Value = 0; update letest value from shop
        }

        private void ShowQuitPopup(bool isActive)
        {
            quitPopup.OnContinueAddMove = () => 
            { 
                // If player click Continue in exit popup, turn off
                // it and show back this continue popup
                gameObject.SetActive(true);
            };

            quitPopup.OnPlayerQuit = () =>
            {
                // If player press Quit, the quit level immediately
                _completionSource.TrySetResult(false);
            };

            quitPopup.gameObject.SetActive(isActive);
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

        public UniTask<bool> ShowContinue()
        {
            _completionSource = new();
            gameObject.SetActive(true);
            return _completionSource.Task;
        }

        private async UniTask OnPlayClicked()
        {
            await CloseAnimation();
            _completionSource.TrySetResult(true);
            gameObject.SetActive(false);
            // To do: spend coins to purchase next moves
        }

        private async UniTask OnQuitClicked()
        {
            await CloseAnimation();
            ShowQuitPopup(true);
            gameObject.SetActive(false);
        }

        private void UpdateCoin(int coin)
        {
            _reactiveCoin.Value = coin;
        }

        private void ShowCoin(int coin)
        {
            currentCoin.text = $"{coin:N1}";
        }

        private async UniTask CloseAnimation()
        {
            popupAnimator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
        }
    }
}
