using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.LoadingScene.Settings;
using GlobalScripts.SceneUtils;
using Cysharp.Threading.Tasks;

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

#if !UNITY_EDITOR
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
#endif
        }

        private void OpenSetting()
        {
            settingPopup.gameObject.SetActive(true);
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
