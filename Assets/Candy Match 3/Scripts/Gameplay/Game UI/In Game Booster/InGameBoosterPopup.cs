using R3;
using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.Messages;
using GlobalScripts.Effects.Tweens;
using Cysharp.Threading.Tasks;
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

        public Action OnClose;

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
            _reactiveCoin.Value = 0; // Show the current value of coin in game data
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

        private async UniTask PurchaseBooster()
        {
            int price = _boosterPack.Price; // Use this value to check remain coins

            coinEffect.Play();
            _addInGameBoosterPublisher.Publish(new AddInGameBoosterMessage
            {
                BoosterType = boosterType,
                Amount = _boosterPack.Amount
            });

            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
            await ClosePopup();
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

            base.DoClose();
            OnClose?.Invoke();
            OnClose = null;
        }
    }
}
