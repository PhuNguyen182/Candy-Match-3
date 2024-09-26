using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.SceneUtils;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.BoostScene
{
    public class BoostSceneController : MonoBehaviour
    {
        private CancellationToken _token;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
        }

        private void Start()
        {
            LoadNextScene().Forget();
        }

        private async UniTask LoadNextScene()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: _token);
            await SceneLoader.LoadScene(SceneConstants.Loading);
        }
    }
}
