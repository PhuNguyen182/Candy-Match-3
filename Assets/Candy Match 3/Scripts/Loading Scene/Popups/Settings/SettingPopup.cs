using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Common.UIElements;
using Cysharp.Threading.Tasks;
using GlobalScripts.Audios;

namespace CandyMatch3.Scripts.LoadingScene.Settings
{
    public class SettingPopup : MonoBehaviour
    {
        [SerializeField] private Animator popupAnimator;
        [SerializeField] private ToggleGroup avatarToggles;

        [Header("Setting Buttons")]
        [SerializeField] private SliderButton musicButton;
        [SerializeField] private SliderButton soundButton;

        private CancellationToken _token;
        private readonly int _closeHash = Animator.StringToHash("Close");

        private void Awake()
        {
            avatarToggles.allowSwitchOff = false;
            _token = this.GetCancellationTokenOnDestroy();

            musicButton.AddListener(OnMusicButtonClick);
            soundButton.AddListener(OnSoundButtonClick);
        }

        private void OnEnable()
        {
            float musicVolume = MusicManager.Instance.MusicVolume;
            float soundVolume = MusicManager.Instance.SoundVolume;

            musicButton.UpdateImmediately(musicVolume > 0.5f);
            soundButton.UpdateImmediately(soundVolume > 0.5f);
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

        private async UniTask CloseAnimation()
        {
            popupAnimator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
        }
    }
}
