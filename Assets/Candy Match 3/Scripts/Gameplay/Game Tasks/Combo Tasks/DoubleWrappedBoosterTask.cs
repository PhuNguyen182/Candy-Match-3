using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using GlobalScripts.Extensions;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks
{
    public class DoubleWrappedBoosterTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly ExplodeItemTask _explodeItemTask;
        private readonly BreakGridTask _breakGridTask;

        private CancellationToken _token;
        private CancellationTokenSource _cts;

        private CheckGridTask _checkGridTask;

        public DoubleWrappedBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask, ExplodeItemTask explodeItemTask)
        {
            _gridCellManager = gridCellManager;
            _explodeItemTask = explodeItemTask;
            _breakGridTask = breakGridTask;

            _cts = new();
            _token = _cts.Token;
        }

        public async UniTask Activate(IGridCell gridCell1, IGridCell gridCell2)
        {
            using (var listPool = ListPool<Vector3Int>.Get(out List<Vector3Int> positions))
            {
                _breakGridTask.ReleaseGridCell(gridCell1);
                _breakGridTask.ReleaseGridCell(gridCell2);

                Vector3Int checkPosition = gridCell2.GridPosition;
                BoundsInt attackRange = checkPosition.GetBounds2D(new Vector3Int(5, 6));
                positions.AddRange(attackRange.Iterator2D());

                using (var breakListPool = ListPool<UniTask>.Get(out List<UniTask> breakTasks))
                {
                    for (int i = 0; i < positions.Count; i++)
                    {
                        breakTasks.Add(_breakGridTask.BreakItem(positions[i]));
                    }

                    await UniTask.WhenAll(breakTasks);
                }

                int count = positions.Count;
                Vector3Int min = positions[0] + new Vector3Int(-1, -1);
                Vector3Int max = positions[count - 1] + new Vector3Int(1, 1);

                positions.Add(min);
                positions.Add(max);

                BoundsInt checkRange = BoundsExtension.Encapsulate(positions);
                await UniTask.DelayFrame(18, PlayerLoopTiming.Update, _token);
                _checkGridTask.CheckRange(checkRange);
            }
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
