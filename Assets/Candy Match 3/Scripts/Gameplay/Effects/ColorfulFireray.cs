using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using DG.Tweening;

namespace CandyMatch3.Scripts.Gameplay.Effects
{
    public class ColorfulFireray : MonoBehaviour
    {
        [SerializeField] private LightningRayLine rayfire;

        private float _elapsedTime = 0;
        private CancellationToken _token;

        private const float MaxSqrDistance = 133.1361f;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
        }

        public async UniTask Fire(IGridCell targetCell, Vector3 startPosition, float delay)
        {
            _elapsedTime = 0;
            rayfire.StartPosition = startPosition;
            rayfire.EndPosition = startPosition;
            Vector3 destination = targetCell.WorldPosition;

            await DOVirtual.Vector3(startPosition, destination, 0.5f, pos => SetEndRayfirePosition(pos, destination))
                           .OnComplete(() => OnRayfireComplete(targetCell)).SetDelay(delay)
                           .AwaitForComplete(TweenCancelBehaviour.KillWithCompleteCallback, _token);

            await UniTask.Delay(TimeSpan.FromSeconds(0.05f), false, PlayerLoopTiming.FixedUpdate, _token);
            SimplePool.Despawn(this.gameObject);
        }

        public void SetPhaseStep(int step) => rayfire.SetPhaseStep(step);

        private void SetEndRayfirePosition(Vector3 position, Vector3 destination)
        {
            rayfire.EndPosition = position;
            float sqrMagnitude = (destination - position).sqrMagnitude;
            rayfire.SetAmplitudeInterpolation((MaxSqrDistance - sqrMagnitude) / MaxSqrDistance);
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
