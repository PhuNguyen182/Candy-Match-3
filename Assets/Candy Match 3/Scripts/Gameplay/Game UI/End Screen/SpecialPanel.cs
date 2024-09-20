using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameUI.EndScreen
{
    public class SpecialPanel : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private CancellationToken _token;
        private readonly int _closeHash = Animator.StringToHash("Close");

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
        }

        public async UniTask PlaySpecialEffect()
        {
            gameObject.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: _token);

            animator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
            gameObject.SetActive(false);
        }
    }
}
