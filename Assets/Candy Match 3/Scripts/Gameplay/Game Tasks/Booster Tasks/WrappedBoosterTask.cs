using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;
using GlobalScripts.Extensions;
using Sirenix.Utilities;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks
{
    public class WrappedBoosterTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;

        private CheckGridTask _checkGridTask;

        public WrappedBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask)
        {
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;
        }

        public async UniTask Activate(IGridCell gridCell)
        {
            Vector3Int position = gridCell.GridPosition;

            using (var attactListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> attackPositions))
            {
                BoundsInt checkRange = position.GetBounds2D(1);
                attackPositions.AddRange(checkRange.Iterator2D());

                int count = attackPositions.Count;
                Vector3Int min = attackPositions[0] + new Vector3Int(-1, -1);
                Vector3Int max = attackPositions[count - 1] + new Vector3Int(1, 1);

                using var brealListPool = ListPool<UniTask>.Get(out List<UniTask> breakTasks);
                using var encapsulateListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> encapsulatePositions);

                for (int i = 0; i < attackPositions.Count; i++)
                {
                    if (attackPositions[i] == position)
                        continue;

                    encapsulatePositions.Add(attackPositions[i]);
                    breakTasks.Add(_breakGridTask.BreakItem(attackPositions[i]));
                }

                await UniTask.WhenAll(breakTasks);
                encapsulatePositions.Add(min);
                encapsulatePositions.Add(max);

                BoundsInt attackedRange = BoundsExtension.Encapsulate(encapsulatePositions);
                _checkGridTask.CheckRange(attackedRange);
            }
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        public void Dispose()
        {
            
        }
    }
}
