using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Popups
{
    public class ShufflePopup : BasePopup<ShufflePopup>
    {
        private CancellationToken _token;

        protected override void OnAwake()
        {
            _token = this.GetCancellationTokenOnDestroy();
        }

        public async UniTask ClosePopup()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.667f), cancellationToken: _token);
            gameObject.SetActive(false);
        }
    }
}
