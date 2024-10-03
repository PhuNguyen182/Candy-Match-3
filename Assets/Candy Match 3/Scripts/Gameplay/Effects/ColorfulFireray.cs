using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;
using DG.Tweening;

namespace CandyMatch3.Scripts.Gameplay.Effects
{
    public class ColorfulFireray : MonoBehaviour
    {
        [SerializeField] private LightningRayLine rayfire;

        private CancellationToken _token;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
        }

        public async UniTask Fire(IGridCell targetCell, Vector3 startPosition, float delay)
        {
            rayfire.StartPosition = startPosition;
            rayfire.EndPosition = startPosition;

            await DOVirtual.Vector3(startPosition, targetCell.WorldPosition, 0.5f, SetEndRayfirePosition)
                           .OnComplete(() => OnRayfireComplete(targetCell)).SetDelay(delay)
                           .AwaitForComplete(TweenCancelBehaviour.KillWithCompleteCallback, _token);

            await UniTask.Delay(TimeSpan.FromSeconds(0.05f), false, PlayerLoopTiming.FixedUpdate, _token);
            SimplePool.Despawn(this.gameObject);
        }

        public void SetPhaseStep(int step) => rayfire.SetPhaseStep(step);

        private void SetEndRayfirePosition(Vector3 position)
        {
            rayfire.EndPosition = position;
        }

        private void OnRayfireComplete(IGridCell gridCell)
        {
            if (gridCell.GridStateful.CanContainItem)
            {
                if (gridCell.BlockItem is IColorfulEffect colorful)
                    colorful.PlayColorfulEffect();
            }
        }
    }
}
