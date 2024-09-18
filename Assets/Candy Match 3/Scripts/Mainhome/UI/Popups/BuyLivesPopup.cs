using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Mainhome.Managers;
using CandyMatch3.Scripts.Gameplay.GameUI.Miscs;
using Cysharp.Threading.Tasks;
using TMPro;

namespace CandyMatch3.Scripts.Mainhome.UI.Popups
{
    public class BuyLivesPopup : BasePopup<BuyLivesPopup>
    {
        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem heartEffect;
        [SerializeField] private FadeBackground background;

        [Space(10)]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TMP_Text timeText;

        [Space(10)]
        [SerializeField] private GameObject[] heartIcons;

        private CancellationToken _token;
        private readonly int _closeHash = Animator.StringToHash("Close");

        protected override void OnAwake()
        {
            _token = this.GetCancellationTokenOnDestroy();

            purchaseButton.onClick.AddListener(Purchase);
            closeButton.onClick.AddListener(() => CloseAsync().Forget());
        }

        protected override void DoAppear()
        {
            MainhomeManager.Instance?.SetInputActive(false);
            background.ShowBackground(true);
        }

        public void UpdateHeart(int heart)
        {
            for (int i = 0; i < heartIcons.Length; i++)
            {
                bool active = i + 1 <= heart;
                heartIcons[i].SetActive(active);
            }
        }

        public void UpdateTime(TimeSpan time)
        {
            if (time.TotalSeconds > 3600)
                timeText.text = $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";

            else
                timeText.text = $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        private void Purchase()
        {
            heartEffect.Play();
            // Do do: if enough coins, buy and fill full hearts, show a animation and effect, otherwise close this popup and open shop
        }

        private async UniTask CloseAsync()
        {
            animator.SetTrigger(_closeHash);
            background.ShowBackground(false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
            MainhomeManager.Instance?.SetInputActive(true);
            base.Close();
        }
    }
}
