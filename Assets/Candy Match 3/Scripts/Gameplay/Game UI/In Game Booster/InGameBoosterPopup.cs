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
using GlobalScripts.Effects.Tweens;
using Cysharp.Threading.Tasks;
using MessagePipe;
using TMPro;
using CandyMatch3.Scripts.Common.Messages;

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

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
            _addInGameBoosterPublisher = GlobalMessagePipe.GetPublisher<AddInGameBoosterMessage>();

            closeButton.onClick.AddListener(() => ClosePopup().Forget());
            purchaseButton.onClick.AddListener(PurchaseBooster);

            coinTweenEffect.BindInt(_reactiveCoin, UpdateCoin);
            _reactiveCoin.Value = 0;
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

        private void PurchaseBooster()
        {
            coinEffect.Play();
        }

        private void SetPopupInfo(InGameBoosterBoxInfo info)
        {
            boosterName.text = info.BoosterName;
            description.text = info.Description;
            boosterIcon.sprite = info.Icon;
        }

        private void UpdateCoin(int coin)
        {
            coinText.text = $"{coin:N, ru-RU}";
        }

        private async UniTask ClosePopup()
        {
            popupAnimator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(closeClip.length), cancellationToken: _token);
            base.DoClose();
        }
    }
}
