using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Messages;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks 
{
    public class BreakBoosterTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;
        private readonly ExplodeItemTask _explodeItemTask;
        private readonly GameObject _boosterVisual;

        private readonly IPublisher<CameraShakeMessage> _cameraShakePublisher;
        private readonly IPublisher<UseInGameBoosterMessage> _useInGameBoosterPublisher;

        private CancellationToken _token;
        private CancellationTokenSource _cts;

        private const float HammerDelay = 1.05f;

        public BreakBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask, ExplodeItemTask explodeItemTask, GameObject boosterVisual)
        {
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;
            _explodeItemTask = explodeItemTask;
            _boosterVisual = boosterVisual;

            _cameraShakePublisher = GlobalMessagePipe.GetPublisher<CameraShakeMessage>();
            _useInGameBoosterPublisher = GlobalMessagePipe.GetPublisher<UseInGameBoosterMessage>();

            _cts = new();
            _token = _cts.Token;
        }

        public async UniTask Activate(Vector3Int position)
        {
            _useInGameBoosterPublisher.Publish(new UseInGameBoosterMessage
            {
                BoosterType = InGameBoosterType.Break
            });
            
            await PlayEffect(position);
            await _breakGridTask.Break(position);
        }

        private async UniTask PlayEffect(Vector3Int position)
        {
            Vector3 breakPosition = _gridCellManager.Get(position).WorldPosition;
            SimplePool.Spawn(_boosterVisual, EffectContainer.Transform, breakPosition, Quaternion.identity);
            await UniTask.Delay(TimeSpan.FromSeconds(HammerDelay), cancellationToken: _token);

            _cameraShakePublisher.Publish(new CameraShakeMessage
            {
                Amplitude = 2f,
                Duration = 0.3f
            });

            EffectManager.Instance.PlaySoundEffect(SoundEffectType.ItemHit);
            _explodeItemTask.Blast(position, 1).Forget();
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    } 
}
