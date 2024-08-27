using System;
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

        private CheckGridTask _checkGridTask;

        public DoubleColorfulBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask, ColorfulFireray colorfulFireray)
        {
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;
            _colorfulFireray = colorfulFireray;
        }

        public async UniTask Activate(IGridCell gridCell1, IGridCell gridCell2)
        {
            Vector3 oddStartPosition, evenStartPosition;
            BoundsInt activeBounds = _gridCellManager.GetActiveBounds();
            GridPositionType gridCellType = GetCellPositionType(gridCell1.GridPosition);

            if(gridCellType == GridPositionType.Odd)
            {
                oddStartPosition = gridCell1.WorldPosition;
                evenStartPosition = gridCell2.WorldPosition;
            }

            else
            {
                oddStartPosition = gridCell2.WorldPosition;
                evenStartPosition = gridCell1.WorldPosition;
            }

            using (var listPool = ListPool<Vector3Int>.Get(out List<Vector3Int> positions))
            {
                positions.AddRange(_gridCellManager.GetAllPositions());

                using var oddListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> oddPositions);
                using var evenListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> evenPositions);

                for (int i = 0; i < positions.Count; i++)
                {
                    IGridCell gridCell = _gridCellManager.Get(positions[i]);

                    if (!gridCell.HasItem)
                        continue;

                    if (gridCell.GridPosition == gridCell1.GridPosition || gridCell.GridPosition == gridCell2.GridPosition)
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
                    oddBreakTasks.Add(Fireray(oddPositions[i], oddStartPosition, i * 0.02f));
                }

                for (int i = 0; i < evenPositions.Count; i++)
                {
                    evenBreakTasks.Add(Fireray(evenPositions[i], evenStartPosition, i * 0.02f));
                }

                await UniTask.WhenAll(oddBreakTasks);
                await UniTask.WhenAll(evenBreakTasks);

                if(gridCell1.BlockItem is IBooster booster1 && gridCell2.BlockItem is IBooster booster2)
                {
                    booster1.Explode();
                    booster2.Explode();
                }

                _breakGridTask.ReleaseGridCell(gridCell1);
                _breakGridTask.ReleaseGridCell(gridCell2);

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

        private async UniTask Fireray(Vector3Int targetPosition, Vector3 position, float delay)
        {
            IGridCell targetGridCell = _gridCellManager.Get(targetPosition);
            ColorfulFireray fireray = SimplePool.Spawn(_colorfulFireray, EffectContainer.Transform
                                                       , Vector3.zero, Quaternion.identity);
            await fireray.Fire(targetGridCell, position, delay);
            await _breakGridTask.BreakItem(targetPosition);
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
