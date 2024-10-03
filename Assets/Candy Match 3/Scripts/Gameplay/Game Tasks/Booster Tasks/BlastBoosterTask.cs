using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Gameplay.Effects;
using GlobalScripts.Extensions;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks
{
    public class BlastBoosterTask : IDisposable
    {
        private readonly ExplodeItemTask _explodeItemTask;
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;

        private readonly IPublisher<CameraShakeMessage> _cameraShakePublisher;
        private readonly IPublisher<UseInGameBoosterMessage> _useInGameBoosterPublisher;

        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private CheckGridTask _checkGridTask;

        public BlastBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask, ExplodeItemTask explodeItemTask)
        {
            _explodeItemTask = explodeItemTask;
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;

            _cts = new();
            _token = _cts.Token;

            _cameraShakePublisher = GlobalMessagePipe.GetPublisher<CameraShakeMessage>();
            _useInGameBoosterPublisher = GlobalMessagePipe.GetPublisher<UseInGameBoosterMessage>();
        }

        public async UniTask Activate(Vector3Int position)
        {
            IGridCell gridCell = _gridCellManager.Get(position);

            if (gridCell == null)
                return;

            PlayEffect(position);
            _breakGridTask.ReleaseGridCell(gridCell);

            _useInGameBoosterPublisher.Publish(new UseInGameBoosterMessage
            {
                BoosterType = InGameBoosterType.Blast
            });

            using (var attactListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> attackPositions))
            {
                BoundsInt checkRange = position.GetBounds2D(new Vector3Int(5, 5));
                attackPositions.AddRange(checkRange.Iterator2D());
                int count = attackPositions.Count;

                using var brealListPool = ListPool<UniTask>.Get(out List<UniTask> breakTasks);
                using var encapsulateListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> encapsulatePositions);

                for (int i = 0; i < attackPositions.Count; i++)
                {
                    if (attackPositions[i] == position)
                        continue;

                    encapsulatePositions.Add(attackPositions[i]);
                    breakTasks.Add(BreakItem(attackPositions[i]));
                }

                await UniTask.WhenAll(breakTasks);

                Vector3Int min = attackPositions[0] + new Vector3Int(-1, -1);
                Vector3Int max = attackPositions[count - 1] + new Vector3Int(1, 1);

                encapsulatePositions.Clear();
                encapsulatePositions.Add(min);
                encapsulatePositions.Add(max);

                BoundsInt attackRange = BoundsExtension.Encapsulate(encapsulatePositions);
                TimeSpan delay = TimeSpan.FromSeconds(Match3Constants.ItemMatchDelay);
                await UniTask.Delay(delay, false, PlayerLoopTiming.FixedUpdate, _token);
                _checkGridTask.CheckRange(attackRange);
            }
        }

        private async UniTask BreakItem(Vector3Int position)
        {
            IGridCell gridCell = _gridCellManager.Get(position);

            if (gridCell == null)
                return;

            await _breakGridTask.BreakItem(position);
        }

        private void PlayEffect(Vector3Int position)
        {
            _cameraShakePublisher.Publish(new CameraShakeMessage
            {
                Amplitude = 1.75f,
                Duration = 0.32f
            });

            // Play same sound to wrapped candy
            EffectManager.Instance.PlaySoundEffect(SoundEffectType.CandyWrap);
            _explodeItemTask.Blast(position, 3).Forget();
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
