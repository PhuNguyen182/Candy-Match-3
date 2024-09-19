using R3;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Mainhome.Managers;
using CandyMatch3.Scripts.Gameplay.GameUI.Miscs;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.Databases;
using GlobalScripts.Effects.Tweens;
using Cysharp.Threading.Tasks;
using GlobalScripts.Audios;
using TMPro;

namespace CandyMatch3.Scripts.Mainhome.UI.Shops
{
    public class ShopPopup : BasePopup<ShopPopup>
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text currentCoinText;

        [Space(10)]
        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem coinEffect;
        [SerializeField] private TweenValueEffect coinTween;
        [SerializeField] private FadeBackground background;
        [SerializeField] private UpdateResouceResponder resouceResponder;

        [Header("Product Cells")]
        [SerializeField] private ProductDatabase productDatabase;
        [SerializeField] private List<ProductCell> productCells;

        private CancellationToken _token;
        private readonly int _closeHash = Animator.StringToHash("Close");
        private ReactiveProperty<int> _reactiveCoin = new(0);

        protected override void OnAwake()
        {
            _token = this.GetCancellationTokenOnDestroy();

            productDatabase.Initialize();
            closeButton.onClick.AddListener(() => CloseAsync().Forget());
            InitProducts();

            coinTween.BindInt(_reactiveCoin, ShowCoin);
            resouceResponder.OnUpdate = () =>
            {
                // To do: update coin value here
            };
        }

        protected override void DoAppear()
        {
            if (!IsPreload)
                MusicManager.Instance.PlaySoundEffect(SoundEffectType.PopupOpen);

            MainhomeManager.Instance?.SetInputActive(false);
            background.ShowBackground(true);
        }

        private void InitProducts()
        {
            for (int i = 0; i < productDatabase.ProductInfos.Count; i++)
            {
                ProductInfo productInfo = productDatabase.ProductInfos[i];
                productCells[i].SetProductInfo(productInfo);
                productCells[i].OnPurchase = PlayPurchaseEffect;
            }
        }

        private async UniTask CloseAsync()
        {
            animator.SetTrigger(_closeHash);
            background.ShowBackground(false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
            base.Close();
        }

        private void ShowCoin(int coin)
        {
            currentCoinText.text = $"{coin}";
        }

        private void PlayPurchaseEffect()
        {
            coinEffect.Play();
        }

        protected override void DoDisappear()
        {
            MainhomeManager.Instance?.SetInputActive(true);

            if (!IsPreload)
                MusicManager.Instance.PlaySoundEffect(SoundEffectType.PopupClose);
        }
    }
}
