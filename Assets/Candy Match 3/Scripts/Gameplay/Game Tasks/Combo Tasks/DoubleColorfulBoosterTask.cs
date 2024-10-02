using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Effects;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks
{
    public class DoubleColorfulBoosterTask : IDisposable
    {
        private readonly ColorfulFireray _colorfulFireray;
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;

        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private CheckGridTask _checkGridTask;

        public DoubleColorfulBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask, ColorfulFireray colorfulFireray)
        {
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;
            _colorfulFireray = colorfulFireray;

            _cts = new();
            _token = _cts.Token;
        }

        public async UniTask Activate(IGridCell gridCell1, IGridCell gridCell2)
        {
            Vector3 oddStartPosition, evenStartPosition;
            BoundsInt activeBounds = _gridCellManager.GetActiveBounds();

            IBooster swapBooster1 = default, swapBooster2 = default;
            GridPositionType gridCellType = GetCellPositionType(gridCell1.GridPosition);

            gridCell1.LockStates = LockStates.Preparing;
            gridCell2.LockStates = LockStates.Preparing;

            if (gridCellType == GridPositionType.Odd)
            {
                oddStartPosition = gridCell1.WorldPosition;
                evenStartPosition = gridCell2.WorldPosition;
            }

            else
            {
                oddStartPosition = gridCell2.WorldPosition;
                evenStartPosition = gridCell1.WorldPosition;
            }

            IBooster booster1 = gridCell1.BlockItem as IBooster;
            IBooster booster2 = gridCell2.BlockItem as IBooster;

            booster1.IsActivated = true;
            booster2.IsActivated = true;

            (swapBooster1, swapBooster2) = (booster1, booster2);

            using (ListPool<Vector3Int>.Get(out List<Vector3Int> positions))
            {
                positions.AddRange(_gridCellManager.GetActivePositions());

                using var oddListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> oddPositions);
                using var evenListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> evenPositions);

                for (int i = 0; i < positions.Count; i++)
                {
                    IGridCell gridCell = _gridCellManager.Get(positions[i]);

                    if (!gridCell.HasItem || gridCell.IsLocked)
                        continue;

                    if (gridCell.GridPosition == gridCell1.GridPosition || gridCell.GridPosition == gridCell2.GridPosition)
                        continue;

                    if (gridCell.ItemType == ItemType.ColorBomb)
                        continue;

                    gridCellType = GetCellPositionType(positions[i]);

                    if (gridCellType == GridPositionType.Odd)
                        oddPositions.Add(positions[i]);

                    else if (gridCellType == GridPositionType.Even)
                        evenPositions.Add(positions[i]);
                }

                using var oddBreakListPool = ListPool<UniTask>.Get(out List<UniTask> oddBreakTasks);
                using var evenBreakListPool = ListPool<UniTask>.Get(out List<UniTask> evenBreakTasks);

                for (int i = 0; i < oddPositions.Count; i++)
                {
                    oddBreakTasks.Add(FireItemCatchRay(i, oddPositions[i], oddStartPosition, i * 0.02f));
                }

                for (int i = 0; i < evenPositions.Count; i++)
                {
                    evenBreakTasks.Add(FireItemCatchRay(i, evenPositions[i], evenStartPosition, i * 0.02f));
                }

                await UniTask.WhenAll(oddBreakTasks);
                await UniTask.WhenAll(evenBreakTasks);

                swapBooster1?.Explode();
                swapBooster2?.Explode();

                _breakGridTask.ReleaseGridCell(gridCell1);
                _breakGridTask.ReleaseGridCell(gridCell2);
                await UniTask.NextFrame(_token);

                gridCell1.LockStates = LockStates.None;
                gridCell2.LockStates = LockStates.None;
                _checkGridTask.CheckRange(activeBounds);
            }
        }

        private GridPositionType GetCellPositionType(Vector3Int position)
        {
            GridPositionType gridPosition = GridPositionType.None;
            
            if ((position.x % 2 == 0 && position.y % 2 == 0) || (position.x % 2 != 0 && position.y % 2 != 0))
                gridPosition = GridPositionType.Even;
            
            else if ((position.x % 2 == 0 && position.y % 2 != 0) || (position.x % 2 != 0 && position.y % 2 == 0))
                gridPosition = GridPositionType.Odd;

            return gridPosition;
        }

        private async UniTask FireItemCatchRay(int index, Vector3Int targetPosition, Vector3 position, float delay)
        {
            IGridCell targetGridCell = _gridCellManager.Get(targetPosition);
            ColorfulFireray fireray = SimplePool.Spawn(_colorfulFireray, EffectContainer.Transform
                                                       , Vector3.zero, Quaternion.identity);
            fireray.SetPhaseStep(index);
            fireray.SetColor(CandyColor.None, true);
            await fireray.Fire(targetGridCell, position, delay);
            await _breakGridTask.BreakItem(targetPosition);
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
