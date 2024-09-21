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
using Cysharp.Threading.Tasks;
using GlobalScripts.Extensions;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks
{
    public class VerticalStripedBoosterTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;

        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private CheckGridTask _checkGridTask;

        private BoundsInt _attackRange;

        public VerticalStripedBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask)
        {
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;

            _cts = new();
            _token = _cts.Token;
        }

        public async UniTask Activate(IGridCell gridCell, bool useDelay, bool doNotCheck, Action<BoundsInt> attackRange)
        {
            Vector3Int position = gridCell.GridPosition;
            BoundsInt activeBounds = _gridCellManager.GetActiveBounds();
            _breakGridTask.ReleaseGridCell(gridCell);

            using (var attactListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> attackPositions))
            {
                attackPositions.AddRange(activeBounds.GetColumn(position));
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
                
                Vector3Int min = attackPositions[0] + new Vector3Int(-1, 0);
                Vector3Int max = attackPositions[count - 1] + new Vector3Int(1, 0);

                encapsulatePositions.Clear();
                encapsulatePositions.Add(min);
                encapsulatePositions.Add(max);

                _attackRange = BoundsExtension.Encapsulate(encapsulatePositions);
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

        private async UniTask BreakItem(Vector3Int position)
        {
            IGridCell gridCell = _gridCellManager.Get(position);

            if (gridCell == null)
                return;

            if (gridCell.LockStates == LockStates.Preparing)
                return;

            await _breakGridTask.BreakItem(position);
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
