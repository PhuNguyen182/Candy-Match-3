using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.LoadingScene.Settings;
using GlobalScripts.SceneUtils;
using Cysharp.Threading.Tasks;
using GlobalScripts.Audios;

namespace CandyMatch3.Scripts.LoadingScene
{
    public class LoadingSceneController : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private SettingPopup settingPopup;

        private CancellationToken _token;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();

            settingButton.onClick.AddListener(OpenSetting);
            playButton.onClick.AddListener(() => PlayGame().Forget());
        }

        private void Start()
        {
            PlayMusic().Forget();
        }

        private void OpenSetting()
        {
            settingPopup.gameObject.SetActive(true);
        }

        private async UniTask PlayMusic()
        {
            await UniTask.NextFrame(_token);

#if !UNITY_EDITOR
            if(!MusicManager.Instance.IsMusicPlaying())
                MusicManager.Instance.PlayBackgroundMusic(BackgroundMusicType.Mainhome, volume: 0.6f);
#endif
        }

        private async UniTask PlayGame()
        {
            playButton.interactable = false;
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: _token);
            if (_token.IsCancellationRequested) return;

            await SceneLoader.LoadScene(SceneConstants.Mainhome);
        }
    }
}
