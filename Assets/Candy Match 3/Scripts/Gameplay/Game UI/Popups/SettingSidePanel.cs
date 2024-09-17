using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Gameplay.GameUI.Miscs;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Popups
{
    public class SettingSidePanel : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private FadeBackground background;
        [SerializeField] private CanvasGroup panelGroup;

        [Header("Buttons")]
        [SerializeField] private Button settingButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button musicButton;
        [SerializeField] private Button soundButton;

        [Header("Popup")]
        [SerializeField] private QuitPopup quitPopup;

        private bool _isOpen = false;
        private readonly int _closeHash = Animator.StringToHash("Close");
        private CancellationToken _token;

        public QuitPopup QuitPopup => quitPopup;
        public Action<bool> OnSetting;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();

            musicButton.onClick.AddListener(MusicButton);
            soundButton.onClick.AddListener(SoundButton);

            quitButton.onClick.AddListener(() => OpenQuitPopup().Forget());
            settingButton.onClick.AddListener(() => OnSettingClicked().Forget());
        }

        private async UniTask OnSettingClicked()
        {
            if (!_isOpen)
            {
                _isOpen = true;
                OnSetting?.Invoke(false);
                
                panelGroup.interactable = false;
                settingButton.interactable = false;
                
                panelGroup.gameObject.SetActive(true);
                background.ShowBackground(true);

                await UniTask.Delay(TimeSpan.FromSeconds(0.834f), cancellationToken: _token);
                panelGroup.interactable = true;
                settingButton.interactable = true;
            }

            else
            {
                _isOpen = false;
                panelGroup.interactable = false;
                settingButton.interactable = false;
                background.ShowBackground(false);

                await CloseAnimation();
                OnSetting?.Invoke(true);

                panelGroup.gameObject.SetActive(false);
                settingButton.interactable = true;
            }
        }

        private async UniTask OpenQuitPopup()
        {
            await OnSettingClicked();
            OnSetting?.Invoke(false);
            background.ShowBackground(true);
            quitPopup.gameObject.SetActive(true);

            quitPopup.OnContinuePlaying = () =>
            {
                OnSetting?.Invoke(true);
                background.ShowBackground(false);
            };

            quitPopup.OnPlayerQuit += () =>
            {
                background.ShowBackground(false);
            };
        }

        private void MusicButton()
        {

        }

        private void SoundButton()
        {

        }

        private async UniTask CloseAnimation()
        {
            animator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: _token);
        }
    }
}
