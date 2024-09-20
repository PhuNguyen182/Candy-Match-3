using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Models;
using CandyMatch3.Scripts.Gameplay.GameUI.Popups;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class StartGameTask : IDisposable
    {
        private const string LevelStartPopopPath = "Common Popups/Level Start Popup.prefab";

        private CancellationToken _token;
        private CancellationTokenSource _cts;

        public StartGameTask()
        {
            PreloadPopup();

            _cts = new();
            _token = _cts.Token;
        }

        public async UniTask ShowLevelStart(LevelModel levelModel)
        {
            await UniTask.DelayFrame(6, cancellationToken: _token);
            var popup = await LevelStartPopup.CreateFromAddress(LevelStartPopopPath);
            popup.InitLevelTarget(levelModel);
            await popup.ClosePopup();
        }

        private void PreloadPopup()
        {
            LevelStartPopup.PreloadFromAddress(LevelStartPopopPath).Forget();
        }

        public void Dispose()
        {
            _cts.Dispose();
            LevelStartPopup.Release();
        }
    }
}
