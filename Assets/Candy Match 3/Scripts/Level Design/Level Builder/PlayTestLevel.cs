using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using CandyMatch3.Scripts.Common.SingleConfigs;
using CandyMatch3.Scripts.Gameplay.Models;
using GlobalScripts.SceneUtils;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.LevelDesign.LevelBuilder
{
    public class PlayTestLevel : MonoBehaviour
    {
        [SerializeField] private LevelBuilder levelBuilder;

        private CancellationToken _token;

        private void Start()
        {
            _token = this.GetCancellationTokenOnDestroy();
            AutoPlay().Forget();
        }

        private async UniTask AutoPlay()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: _token);
            levelBuilder.Export(0, false);

            LevelModel levelModel;
            using (StringReader streamReader = new(levelBuilder.LevelData))
            {
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    JsonSerializer jsonSerializer = new();
                    levelModel = jsonSerializer.Deserialize<LevelModel>(jsonReader);
                }
            }

            PlayGameConfig.Current = new()
            {
                IsTestMode = true,
                LevelModel = levelModel
            };

            await SceneLoader.LoadScene(SceneConstants.Gameplay);
        }
    }
}
