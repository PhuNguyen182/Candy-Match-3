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
        [SerializeField] private CanvasGroup canvasGroup;

        private CancellationToken _token;
        private readonly int _closeHash = Animator.StringToHash("Close");

        public Action OnContinueAddMove; // Call this for continue playing after lose level, +5 moves
        public Action OnContinuePlaying; // Call this for continue playing during level
        public Action OnPlayerContinueQuit; // Call this to quit level after show add move offer
        public Action OnPlayerQuit; // Call this to quit level without end level

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();

            closeButton.onClick.AddListener(() => Continue().Forget());
            continueButton.onClick.AddListener(() => Continue().Forget());
            quitButton.onClick.AddListener(() => Quit().Forget());
        }

        private void OnEnable()
        {
            canvasGroup.interactable = true;
        }

        private async UniTask Continue()
        {
            canvasGroup.interactable = false;

            await Close();
            OnContinueAddMove?.Invoke();
            OnContinuePlaying?.Invoke();
            gameObject.SetActive(false);
        }

        private async UniTask Quit()
        {
            canvasGroup.interactable = true;

            await Close();
            if (OnPlayerContinueQuit != null)
                OnPlayerContinueQuit.Invoke();

            else if (OnPlayerQuit != null)
                OnPlayerQuit.Invoke();
        }

        private async UniTask Close()
        {
            popupAnimator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
        }

        private void OnDisable()
        {
            OnContinueAddMove = null;
            OnContinuePlaying = null;
            OnPlayerContinueQuit = null;
        }
    }
}
