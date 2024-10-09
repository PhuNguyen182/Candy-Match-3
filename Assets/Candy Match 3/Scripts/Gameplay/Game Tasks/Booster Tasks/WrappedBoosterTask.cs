using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Common.Messages;
using Cysharp.Threading.Tasks;
using GlobalScripts.Extensions;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks
{
    public class WrappedBoosterTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly ExplodeItemTask _explodeItemTask;
        private readonly BreakGridTask _breakGridTask;
        private readonly IPublisher<CameraShakeMessage> _cameraShakePublisher;

        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private CheckGridTask _checkGridTask;

        private BoundsInt _attackRange;

        public WrappedBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask, ExplodeItemTask explodeItemTask)
        {
            _gridCellManager = gridCellManager;
            _explodeItemTask = explodeItemTask;
            _breakGridTask = breakGridTask;

            _cts = new();
            _token = _cts.Token;

            _cameraShakePublisher = GlobalMessagePipe.GetPublisher<CameraShakeMessage>();
        }

        public async UniTask Activate(IGridCell gridCell, bool useDelay, bool doNotCheck, bool isCreateBooster, Action<BoundsInt> attackRange)
        {
            Vector3Int position = gridCell.GridPosition;
            IBlockItem blockItem = gridCell.BlockItem;

            _breakGridTask.ReleaseGridCell(gridCell);
            using (ListPool<Vector3Int>.Get(out List<Vector3Int> attackPositions))
            {
                BoundsInt checkRange = position.GetBounds2D(2);
                attackPositions.AddRange(checkRange.Iterator2D());
                int count = attackPositions.Count;

                using var brealListPool = ListPool<UniTask>.Get(out List<UniTask> breakTasks);

                _explodeItemTask.Blast(position, 3).Forget();
                _explodeItemTask.Explode(position, ExplodeType.SingleWrapped);

                for (int i = 0; i < attackPositions.Count; i++)
                {
                    if (attackPositions[i] == position)
                        continue;

                    breakTasks.Add(BreakItem(attackPositions[i]));
                }

                ShakeCamera();
                await UniTask.WhenAll(breakTasks);
                await UniTask.DelayFrame(3, PlayerLoopTiming.FixedUpdate, _token);
                _attackRange = position.GetBounds2D(3);
                attackRange?.Invoke(_attackRange);

                if (useDelay)
                    await UniTask.DelayFrame(Match3Constants.BoosterDelayFrame, PlayerLoopTiming.FixedUpdate, _token);

                if (!doNotCheck)
                    _checkGridTask.CheckRange(_attackRange);
            }
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        private void ShakeCamera()
        {
            _cameraShakePublisher.Publish(new CameraShakeMessage
            {
                Amplitude = 1f,
                Duration = 0.3f
            });
        }

        private async UniTask BreakItem(Vector3Int position)
        {
            IGridCell gridCell = _gridCellManager.Get(position);

            // Prevent break items inside lock-on booster range
            if (gridCell == null || gridCell.LockStates == LockStates.Preparing)
                return;

            await _breakGridTask.BreakItem(position);
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
