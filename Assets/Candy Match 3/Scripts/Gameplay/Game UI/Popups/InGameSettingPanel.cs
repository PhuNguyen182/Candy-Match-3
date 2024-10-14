using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Gameplay.GameUI.Miscs;
using Cysharp.Threading.Tasks;
using GlobalScripts.Audios;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Popups
{
    public class InGameSettingPanel : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Transform panelParent;
        [SerializeField] private FadeBackground background;
        [SerializeField] private CanvasGroup panelGroup;

        [Header("Buttons")]
        [SerializeField] private Button settingButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private MultiSpriteButton musicButton;
        [SerializeField] private MultiSpriteButton soundButton;

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

            musicButton.AddListener(MusicButton);
            soundButton.AddListener(SoundButton);

            quitButton.onClick.AddListener(() => OpenQuitPopup().Forget());
            settingButton.onClick.AddListener(() => OnSettingClicked().Forget());
        }

        private void OnEnable()
        {
            UpdateButtons();
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

                await UniTask.Delay(TimeSpan.FromSeconds(0.917f), cancellationToken: _token);
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
            SetButtonSettingParent(false, panelParent);

            quitPopup.OnContinuePlaying = () =>
            {
                OnSetting?.Invoke(true);
                background.ShowBackground(false);
                SetButtonSettingParent(true);
            };

            quitPopup.OnPlayerQuit += () =>
            {
                SetButtonSettingParent(true);
                background.ShowBackground(false);
            };
        }

        private void MusicButton()
        {
            float musicVolume = AudioManager.Instance.MusicVolume;
            float newVolume = musicVolume > 0.5f ? 0.0001f : 1f;
            AudioManager.Instance.MusicVolume = newVolume;
            UpdateMusicButton();
        }

        private void SoundButton()
        {
            float soundVolume = AudioManager.Instance.SoundVolume;
            float newVolume = soundVolume > 0.5f ? 0.0001f : 1f;
            AudioManager.Instance.SoundVolume = newVolume;
            UpdateSoundButton();
        }

        private async UniTask CloseAnimation()
        {
            animator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.8f), cancellationToken: _token);
        }

        private void UpdateButtons()
        {
            UpdateMusicButton();
            UpdateSoundButton();
        }

        private void UpdateMusicButton()
        {
            float musicVolume = AudioManager.Instance.MusicVolume;
            int musicState = musicVolume > 0.5f ? 0 : 1;
            musicButton.SetState(musicState);
        }

        private void UpdateSoundButton()
        {
            float soundVolume = AudioManager.Instance.SoundVolume;
            int soundState = soundVolume > 0.5f ? 0 : 1;
            soundButton.SetState(soundState);
        }

        // This function is use to hide setting button every time end game popup or quit popup is shown
        public void SetButtonSettingParent(bool isActive, Transform parent = null)
        {
            settingButton.transform.SetParent(isActive ? transform.parent : parent);

            if (isActive)
                settingButton.transform.SetAsLastSibling();
            else
                settingButton.transform.SetAsFirstSibling();
        }
    }
}
