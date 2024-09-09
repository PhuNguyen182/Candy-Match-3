using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Popups
{
    public class QuitPopup : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Animator popupAnimator;

        private CancellationToken _token;
        private readonly int _closeHash = Animator.StringToHash("Close");

        public Action OnContinueAddMove; // Call this for continue playing after lose level, +5 moves
        public Action OnContinuePlaying; // Call this for continue playing during level
        public Action OnPlayerQuit;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
            continueButton.onClick.AddListener(() => Continue().Forget());

            quitButton.onClick.AddListener(Quit);
            closeButton.onClick.AddListener(Quit);
        }

        private async UniTask Continue()
        {
            await Close();
            OnContinueAddMove?.Invoke();
            OnContinuePlaying?.Invoke();
            gameObject.SetActive(false);
        }

        private void Quit()
        {
            OnPlayerQuit?.Invoke();
        }

        private async UniTask Close()
        {
            popupAnimator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
        }

        private void OnDisable()
        {
            OnContinueAddMove = null;
        }
    }
}
