using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Effects;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Miscs
{
    public class WinStarPopup : MonoBehaviour
    {
        [SerializeField] private float delay = 0.4f;
        [SerializeField] private ParticleSystem effect;

        private CancellationToken _token;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
        }

        private void OnEnable()
        {
            PlayEffect().Forget();
        }

        private async UniTask PlayEffect()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: _token);
            EffectManager.Instance.PlaySoundEffect(SoundEffectType.WinStarPop);
            effect.Play();
        }
    }
}
