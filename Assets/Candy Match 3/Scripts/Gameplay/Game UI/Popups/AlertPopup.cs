using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Gameplay.GameUI.Miscs;
using Cysharp.Threading.Tasks;
using TMPro;
using CandyMatch3.Scripts.Mainhome.Managers;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Popups
{
    public class AlertPopup : BasePopup<AlertPopup>
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Animator popupAnimator;
        [SerializeField] private FadeBackground background;

        private CancellationToken _token;
        private readonly int _closeHash = Animator.StringToHash("Close");

        protected override void OnAwake()
        {
            _token = this.GetCancellationTokenOnDestroy();

            closeButton.onClick.AddListener(() => CloseAsync().Forget());
            continueButton.onClick.AddListener(() => CloseAsync().Forget());
        }

        protected override void DoAppear()
        {
            MainhomeManager.Instance?.SetInputActive(false);
            background.ShowBackground(true);
        }

        public void SetMessage(string message)
        {
            messageText.text = message;
        }

        private async UniTask CloseAsync()
        {
            background.ShowBackground(false);
            popupAnimator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
            MainhomeManager.Instance?.SetInputActive(true);
            base.Close();
        }
    }
}
