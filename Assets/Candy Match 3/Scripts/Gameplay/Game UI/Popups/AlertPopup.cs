using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Popups
{
    public class AlertPopup : BasePopup<AlertPopup>
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Animator popupAnimator;

        private CancellationToken _token;
        private readonly int _closeHash = Animator.StringToHash("Close");

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();

            closeButton.onClick.AddListener(() => CloseAsync().Forget());
            continueButton.onClick.AddListener(() => CloseAsync().Forget());
        }

        public void SetMessage(string message)
        {
            messageText.text = message;
        }

        private async UniTask CloseAsync()
        {
            popupAnimator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
            base.Close();
        }
    }
}
