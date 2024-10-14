using R3;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalScripts.Effects.Tweens;
using CandyMatch3.Scripts.GameData;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Gameplay.GameUI.Miscs;
using CandyMatch3.Scripts.Gameplay.GameUI.Popups;
using CandyMatch3.Scripts.Common.Controllers;
using CandyMatch3.Scripts.Mainhome.UI.Shops;
using CandyMatch3.Scripts.Common.Messages;
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
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private ParticleSystem coinEffect;
        [SerializeField] private TweenValueEffect coinTween;
        [SerializeField] private UpdateResouceResponder resouceResponder;
        [SerializeField] private FadeBackground background;
        [SerializeField] private Animator popupAnimator;
        [SerializeField] private QuitPopup quitPopup;

        private int _price = 0;
        private int _currentCoin = 0;
        private ReactiveProperty<int> _reactiveCoin = new(0);

        private CancellationToken _token;
        private UniTaskCompletionSource<bool> _completionSource;
        private readonly int _closeHash = Animator.StringToHash("Close");

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();

            playButton.onClick.AddListener(() => OnPlayClicked().Forget());
            quitButton.onClick.AddListener(() => OnQuitClicked().Forget());

            coinTween.BindInt(_reactiveCoin, ShowCoin);
            resouceResponder.OnUpdate = UpdateCoin;
        }

        private void OnEnable()
        {
            canvasGroup.interactable = true;
            _currentCoin = GameDataManager.Instance.GetResource(GameResourceType.Coin);
            _reactiveCoin.Value = _currentCoin;
        }

        private void ShowQuitPopup(bool isActive)
        {
            quitPopup.OnContinueAddMove = () => 
            { 
                // If player click Continue in exit popup, turn off
                // it and show back this continue popup
                gameObject.SetActive(true);
            };

            quitPopup.OnPlayerContinueQuit = () =>
            {
                // If player press OpenQuitPopup, the quit level immediately
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
            moveText.text = $"+{move}";
            moveMessage.text = $"Add +{move} extra moves to continue.";
        }

        public UniTask<bool> ShowContinue()
        {
            _completionSource = new();
            gameObject.SetActive(true);
            return _completionSource.Task;
        }

        private void ShowPopupAgain()
        {
            background.ShowBackground(true);
            gameObject.SetActive(true);
        }

        private async UniTask OnPlayClicked()
        {
            if (_currentCoin >= _price)
            {
                coinEffect.Play();
                GameDataManager.Instance.SpendResource(GameResourceType.Coin, _price);
                UpdateResourceController.Instance.UpdateResource(new UpdateResourceMessage
                {
                    ResouceType = GameResourceType.Coin
                });
                
                await CloseAnimation();
                _completionSource.TrySetResult(true);
                gameObject.SetActive(false);
            }

            else
            {
                background.ShowBackground(false);
                await CloseAnimation();

                var shop = await ShopPopup.CreateFromAddress(CommonPopupPaths.ShopPopupPath);
                shop.OnClose = ShowPopupAgain;
                gameObject.SetActive(false);
            }
        }

        private async UniTask OnQuitClicked()
        {
            await CloseAnimation();
            ShowQuitPopup(true);
            gameObject.SetActive(false);
        }

        private void UpdateCoin()
        {
            _reactiveCoin.Value = GameDataManager.Instance.GetResource(GameResourceType.Coin);
        }

        private void ShowCoin(int coin)
        {
            currentCoin.text = $"{coin}";
        }

        private async UniTask CloseAnimation()
        {
            canvasGroup.interactable = false;
            popupAnimator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
        }
    }
}
