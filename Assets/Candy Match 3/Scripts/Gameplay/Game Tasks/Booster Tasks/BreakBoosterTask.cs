using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Messages;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks 
{
    public class BreakBoosterTask
    {
        private readonly BreakGridTask _breakGridTask;
        private readonly ExplodeItemTask _explodeItemTask;

        private readonly IPublisher<CameraShakeMessage> _cameraShakePublisher;
        private readonly IPublisher<UseInGameBoosterMessage> _useInGameBoosterPublisher;

        public BreakBoosterTask(BreakGridTask breakGridTask, ExplodeItemTask explodeItemTask)
        {
            _breakGridTask = breakGridTask;
            _explodeItemTask = explodeItemTask;
            _cameraShakePublisher = GlobalMessagePipe.GetPublisher<CameraShakeMessage>();
            _useInGameBoosterPublisher = GlobalMessagePipe.GetPublisher<UseInGameBoosterMessage>();
        }

        public async UniTask Activate(Vector3Int position)
        {
            _useInGameBoosterPublisher.Publish(new UseInGameBoosterMessage
            {
                BoosterType = InGameBoosterType.Break
            });
            
            PlayEffect(position);
            await _breakGridTask.Break(position);
        }

        private void PlayEffect(Vector3Int position)
        {
            _cameraShakePublisher.Publish(new CameraShakeMessage
            {
                Amplitude = 0.75f,
                Duration = 0.3f
            });

            _explodeItemTask.Blast(position, 1).Forget();
        }
    } 
}
