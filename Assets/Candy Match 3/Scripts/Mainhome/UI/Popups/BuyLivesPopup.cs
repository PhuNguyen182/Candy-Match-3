using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.GameData;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.GameData.Constants;
using CandyMatch3.Scripts.Gameplay.GameUI.Miscs;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Mainhome.Managers;
using CandyMatch3.Scripts.Mainhome.UI.Shops;
using CandyMatch3.Scripts.GameManagers;
using Cysharp.Threading.Tasks;
using GlobalScripts.Audios;
using TMPro;

namespace CandyMatch3.Scripts.Mainhome.UI.Popups
{
    public class BuyLivesPopup : BasePopup<BuyLivesPopup>
    {
        [SerializeField] private int price = 300;
        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem heartEffect;
        [SerializeField] private FadeBackground background;

        [Space(10)]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button playButton;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text messageText;

        [Space(10)]
        [SerializeField] private GameObject timerObject;
        [SerializeField] private GameObject[] heartIcons;

        private int _livesCount;
        private TimeSpan _timeCounter;
        private CancellationToken _token;

        private readonly int _closeHash = Animator.StringToHash("Close");
        private const string PlayMessage = "Your <color=#D44081>lives</color> are full now!";
        private const string BuyMessage = "Refill a <color=#FFFF9B>full set</color> of lives";

        protected override void OnAwake()
        {
            _token = this.GetCancellationTokenOnDestroy();

            purchaseButton.onClick.AddListener(() => Purchase().Forget());
            closeButton.onClick.AddListener(() => CloseAsync().Forget());
            playButton.onClick.AddListener(() => OpenPlayGamePopup().Forget());
        }

        protected override void DoAppear()
        {
            if (!IsPreload)
                AudioManager.Instance.PlaySoundEffect(SoundEffectType.PopupOpen);

            priceText.text = $"{price}";
            MainhomeManager.Instance?.SetInputActive(false);
            background.ShowBackground(true);
        }

        private void Update()
        {
            UpdateLives();
        }

        private void UpdateLives()
        {
            _livesCount = GameDataManager.Instance.GetResource(GameResourceType.Life);

            if (_livesCount < 5)
            {
                _timeCounter = GameManager.Instance.HeartTimer.HeartTimeDiff;
                UpdateHeart(_livesCount);
                UpdateTime(_timeCounter);
            }

            else
            {
                timerObject.SetActive(false);
                UpdateHeart(_livesCount);
            }
        }

        private void UpdateHeart(int heart)
        {
            playButton.gameObject.SetActive(heart >= GameDataConstants.MaxLives);
            purchaseButton.gameObject.SetActive(heart < GameDataConstants.MaxLives);
            messageText.text = heart >= GameDataConstants.MaxLives ? PlayMessage : BuyMessage;

            for (int i = 0; i < heartIcons.Length; i++)
            {
                bool active = i + 1 <= heart;
                heartIcons[i].SetActive(active);
            }
        }

        private void UpdateTime(TimeSpan time)
        {
            if (time.TotalSeconds > 3600)
                timeText.text = $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";

            else
                timeText.text = $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        private async UniTask OpenPlayGamePopup()
        {
            await CloseAsync();
            int level = GameDataManager.Instance.GetCurrentLevel();
            var startGamePopup = await StartGamePopup.CreateFromAddress(CommonPopupPaths.StartGamePopupPath);

            await startGamePopup.SetLevelInfo(new LevelBoxData
            {
                Level = level,
                Stars = 0
            });
        }

        private async UniTask Purchase()
        {
            int coins = GameDataManager.Instance.GetResource(GameResourceType.Coin);

            if(coins >= price)
            {
                heartEffect.Play();
                GameDataManager.Instance.SetResource(GameResourceType.Life, 5);
                GameDataManager.Instance.SpendResource(GameResourceType.Coin, price);
            }

            else
            {
                await CloseAsync();
                await ShopPopup.CreateFromAddress(CommonPopupPaths.ShopPopupPath);
            }
        }

        private async UniTask CloseAsync()
        {
            animator.SetTrigger(_closeHash);
            background.ShowBackground(false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
            base.Close();
        }

        protected override void DoDisappear()
        {
            MainhomeManager.Instance?.SetInputActive(true);

            if (!IsPreload)
                AudioManager.Instance.PlaySoundEffect(SoundEffectType.PopupClose);
        }
    }
}
