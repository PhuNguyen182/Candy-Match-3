using R3;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalScripts.Effects.Tweens;
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

        private int _price = 0;
        private ReactiveProperty<int> _reactiveCoin = new(0);

        private CancellationToken _token;
        private UniTaskCompletionSource<bool> _completionSource;
        private readonly int _closeHash = Animator.StringToHash("Close");

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();

            playButton.onClick.AddListener(() => OnPlayClicked().Forget());
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

        public UniTask<bool> ShowContinue()
        {
            _completionSource = new();
            gameObject.SetActive(true);
            return _completionSource.Task;
        }

        private async UniTask OnPlayClicked()
        {
            await Close();
            _completionSource.TrySetResult(true);
            gameObject.SetActive(false);
            // To do: spend coins to purchase next moves
        }

        private void OnQuitClicked()
        {
            //_completionSource.TrySetResult(false); This line should be called in quit popup
        }

        private void UpdateCoin(int coin)
        {
            _reactiveCoin.Value = coin;
        }

        private void ShowCoin(int coin)
        {
            currentCoin.text = $"{coin:N1, ru-RU}";
        }

        private async UniTask Close()
        {
            popupAnimator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
        }
    }
}
