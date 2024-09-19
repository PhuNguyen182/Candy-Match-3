using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Common.UIElements;
using CandyMatch3.Scripts.Gameplay.GameUI.Miscs;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;
using GlobalScripts.Audios;

namespace CandyMatch3.Scripts.LoadingScene.Settings
{
    public class SettingPopup : MonoBehaviour
    {
        [SerializeField] private Animator popupAnimator;
        [SerializeField] private FadeBackground fadeBackground;
        [SerializeField] private ToggleGroup avatarToggles;

        [Header("Setting Buttons")]
        [SerializeField] private SliderButton musicButton;
        [SerializeField] private SliderButton soundButton;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button resetButton;

        private CancellationToken _token;
        private readonly int _closeHash = Animator.StringToHash("Close");

        private void Awake()
        {
            avatarToggles.allowSwitchOff = false;
            _token = this.GetCancellationTokenOnDestroy();

            musicButton.AddListener(OnMusicButtonClick);
            soundButton.AddListener(OnSoundButtonClick);

            closeButton.onClick.AddListener(() => ClosePanel().Forget());
            saveButton.onClick.AddListener(SaveSetting);
            resetButton.onClick.AddListener(ResetProgress);
        }

        private void OnEnable()
        {
            float musicVolume = MusicManager.Instance.MusicVolume;
            float soundVolume = MusicManager.Instance.SoundVolume;

            musicButton.UpdateImmediately(musicVolume > 0.5f);
            soundButton.UpdateImmediately(soundVolume > 0.5f);

            fadeBackground?.ShowBackground(true);
            MusicManager.Instance.PlaySoundEffect(SoundEffectType.PopupOpen);
        }

        private void OnMusicButtonClick()
        {
            float musicVolume = MusicManager.Instance.MusicVolume;
            float newVolume = musicVolume > 0.5f ? 0.0001f : 1f;

            MusicManager.Instance.MusicVolume = newVolume;
            musicButton.UpdateValue(newVolume > 0.5f);
        }

        private void OnSoundButtonClick()
        {
            float soundVolume = MusicManager.Instance.SoundVolume;
            float newVolume = soundVolume > 0.5f ? 0.0001f : 1f;

            MusicManager.Instance.SoundVolume = newVolume;
            soundButton.UpdateValue(newVolume > 0.5f);
        }

        private void SaveSetting()
        {
            ClosePanel().Forget();
        }

        private void ResetProgress()
        {
            // To do: Execute logic reset level progress here
        }

        private async UniTask ClosePanel()
        {
            await CloseAnimation();
            gameObject.SetActive(false);
        }

        private async UniTask CloseAnimation()
        {
            popupAnimator.SetTrigger(_closeHash);
            fadeBackground?.ShowBackground(false);
            MusicManager.Instance.PlaySoundEffect(SoundEffectType.PopupClose);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
        }
    }
}
