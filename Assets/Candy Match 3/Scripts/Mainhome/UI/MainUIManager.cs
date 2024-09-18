using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Mainhome.UI.ResourcesDisplayer;
using Cysharp.Threading.Tasks;
using GlobalScripts.SceneUtils;

namespace CandyMatch3.Scripts.Mainhome.UI
{
    public class MainUIManager : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private CoinCounter coinCounter;
        [SerializeField] private LifeCounter lifeCounter;

        private CancellationToken _token;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
            backButton.onClick.AddListener(() => OnBackClicked().Forget());
        }

        private async UniTask OnBackClicked()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: _token);
            await SceneLoader.LoadScene(SceneConstants.Loading);
        }
    }
}
