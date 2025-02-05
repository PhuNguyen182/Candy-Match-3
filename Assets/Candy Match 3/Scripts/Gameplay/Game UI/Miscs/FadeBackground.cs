using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Miscs
{
    public class FadeBackground : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private readonly int _fadeHash = Animator.StringToHash("Close");

        private CancellationToken _token;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
        }

        public void ShowBackground(bool isActive)
        {
            ShowBackgroundAsync(isActive).Forget();
        }

        public async UniTask ShowBackgroundAsync(bool isActive)
        {
            if (isActive)
            {
                gameObject.SetActive(true);
            }

            else
            {
                if (isActiveAndEnabled)
                {
                    animator.SetTrigger(_fadeHash);
                    await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);

                    if (_token.IsCancellationRequested)
                        return;

                    gameObject?.SetActive(false);
                }
            }
        }
    }
}
