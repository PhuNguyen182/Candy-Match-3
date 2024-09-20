using R3;
using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalScripts.Effects.Tweens;
using CandyMatch3.Scripts.GameData;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Mainhome.UI.Shops;
using CandyMatch3.Scripts.Common.Controllers;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Common.Constants;
using Cysharp.Threading.Tasks;
using GlobalScripts.Audios;
using MessagePipe;
using TMPro;

namespace CandyMatch3.Scripts.Gameplay.GameUI.InGameBooster
{
    public class InGameBoosterPopup : BasePopup<InGameBoosterPopup>
    {
        [Serializable]
        public struct InGameBoosterBoxInfo
        {
            public InGameBoosterType BoosterType;
            public string BoosterName;
            public string Description;
            public Sprite Icon;
        }

        [SerializeField] private Animator popupAnimator;
        [SerializeField] private Animator backgroundAnimator;
        [SerializeField] private AnimationClip closeClip;
        [SerializeField] private TweenValueEffect coinTweenEffect;
        [SerializeField] private InGameBoosterType boosterType;
        [SerializeField] private ParticleSystem coinEffect;
        [SerializeField] private UpdateResouceResponder resouceResponder;

        [Space(10)]
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private TMP_Text boosterName;
        [SerializeField] private TMP_Text description;
        [SerializeField] private TMP_Text boosterAmount;
        [SerializeField] private TMP_Text priceText;

        [Space(10)]
        [SerializeField] private Image boosterIcon;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button purchaseButton;

        [Header("Booster Info")]
        [SerializeField] private List<InGameBoosterBoxInfo> inGameBoosterBoxInfos;

        private CancellationToken _token;
        private InGameBoosterPack _boosterPack;
        private IPublisher<AddInGameBoosterMessage> _addInGameBoosterPublisher;

        private readonly int _closeHash = Animator.StringToHash("Close");
        private Dictionary<InGameBoosterType, InGameBoosterBoxInfo> _boxInfoCollection;
        private ReactiveProperty<int> _reactiveCoin = new();

        public Action UnblockInput;
        public Action BlockInput;

        public Dictionary<InGameBoosterType, InGameBoosterBoxInfo> PopupInfoCollection
        {
            get
            {
                if(_boxInfoCollection == null)
                {
                    _boxInfoCollection = inGameBoosterBoxInfos.ToDictionary(key => key.BoosterType, value => value);
                }

                return _boxInfoCollection;
            }
        }

        protected override void OnAwake()
        {
            _token = this.GetCancellationTokenOnDestroy();
            _addInGameBoosterPublisher = GlobalMessagePipe.GetPublisher<AddInGameBoosterMessage>();
            popupCanvas.worldCamera = Camera.main;

            closeButton.onClick.AddListener(() => ClosePopup().Forget());
            purchaseButton.onClick.AddListener(() => PurchaseBooster().Forget());

            coinTweenEffect.BindInt(_reactiveCoin, UpdateCoin);
            resouceResponder.OnUpdate = UpdateCoin;
        }

        protected override void DoAppear()
        {
            if (!IsPreload)
                MusicManager.Instance.PlaySoundEffect(SoundEffectType.PopupOpen);

            int coin = GameDataManager.Instance.GetResource(GameResourceType.Coin);
            _reactiveCoin.Value = coin; // Show the current value of coin in game data
        }

        public void SetBoosterPack(InGameBoosterPack boosterPack)
        {
            _boosterPack = boosterPack;
            priceText.text = $"{boosterPack.Price}";
            boosterAmount.text = $"+{boosterPack.Amount}";
        }

        public void SetBoosterInfo(InGameBoosterType boosterType)
        {
            this.boosterType = boosterType;
            InGameBoosterBoxInfo info = PopupInfoCollection[boosterType];
            SetPopupInfo(info);
        }

        private void UpdateCoin()
        {
            int coin = GameDataManager.Instance.GetResource(GameResourceType.Coin);
            _reactiveCoin.Value = coin;
        }

        private async UniTask PurchaseBooster()
        {
            int price = _boosterPack.Price;
            int coin = GameDataManager.Instance.GetResource(GameResourceType.Coin);

            if (coin >= price)
            {
                coinEffect.Play();
                MusicManager.Instance.PlaySoundEffect(SoundEffectType.CoinsPopButton);
                GameDataManager.Instance.SpendResource(GameResourceType.Coin, price);

                _addInGameBoosterPublisher.Publish(new AddInGameBoosterMessage
                {
                    BoosterType = boosterType,
                    Amount = _boosterPack.Amount
                });

                UpdateResourceController.Instance.UpdateResource(new UpdateResourceMessage
                {
                    ResouceType = GameResourceType.Coin
                });

                await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
                await ClosePopup();
            }

            else
            {
                Action unblockInput = UnblockInput;
                // If not enought money, do not release this UnblockInput action, then assign this action to shop close event
                // to ensure the on-off player input flow is not interupted

                await ClosePopup();
                BlockInput?.Invoke();
                BlockInput = null;

                var shop = await ShopPopup.CreateFromAddress(CommonPopupPaths.ShopPopupPath);
                shop.OnClose = unblockInput;
            }
        }

        private void SetPopupInfo(InGameBoosterBoxInfo info)
        {
            boosterName.text = info.BoosterName;
            description.text = info.Description;
            boosterIcon.sprite = info.Icon;
        }

        private void UpdateCoin(int coin)
        {
            coinText.text = $"{coin}";
        }

        private async UniTask ClosePopup()
        {
            popupAnimator.SetTrigger(_closeHash);
            backgroundAnimator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(closeClip.length), cancellationToken: _token);

            if (!IsPreload)
                MusicManager.Instance.PlaySoundEffect(SoundEffectType.PopupClose);

            base.DoClose();
            UnblockInput?.Invoke();
            UnblockInput = null;
        }
    }
}
