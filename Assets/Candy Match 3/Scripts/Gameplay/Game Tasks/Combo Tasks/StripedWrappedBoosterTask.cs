using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Effects;
using Cysharp.Threading.Tasks;
using GlobalScripts.Extensions;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks
{
    public class StripedWrappedBoosterTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;

        private CancellationToken _token;
        private CancellationTokenSource _cts;

        private CheckGridTask _checkGridTask;

        public StripedWrappedBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask)
        {
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;

            _cts = new();
            _token = _cts.Token;
        }

        public async UniTask Activate(IGridCell gridCell1, IGridCell gridCell2)
        {
            _breakGridTask.ReleaseGridCell(gridCell1);
            _breakGridTask.ReleaseGridCell(gridCell2);

            Vector3Int checkPosition = gridCell2.GridPosition;
            BoundsInt activeBounds = _gridCellManager.GetActiveBounds();

            using (var breakListPool = ListPool<UniTask>.Get(out List<UniTask> breakTasks))
            {
                using var rowListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> rowListPositions);
                using var columnListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> columnListPositions);

                rowListPositions.AddRange(activeBounds.GetRow(checkPosition + new Vector3Int(0, -1)));
                rowListPositions.AddRange(activeBounds.GetRow(checkPosition));
                rowListPositions.AddRange(activeBounds.GetRow(checkPosition + new Vector3Int(0, 1)));

                columnListPositions.AddRange(activeBounds.GetColumn(checkPosition + new Vector3Int(-1, 0)));
                columnListPositions.AddRange(activeBounds.GetColumn(checkPosition));
                columnListPositions.AddRange(activeBounds.GetColumn(checkPosition + new Vector3Int(1, 0)));

                for (int i = 0; i < rowListPositions.Count; i++)
                {
                    if (rowListPositions[i] == checkPosition)
                        continue;

                    breakTasks.Add(_breakGridTask.BreakItem(rowListPositions[i]));
                }

                for (int i = 0; i < columnListPositions.Count; i++)
                {
                    if (columnListPositions[i] == checkPosition)
                        continue;

                    breakTasks.Add(_breakGridTask.BreakItem(columnListPositions[i]));
                }

                PlayEffect(checkPosition);
                await UniTask.WhenAll(breakTasks);

                int horizontalCount = rowListPositions.Count;
                Vector3Int minHorizontal = rowListPositions[0] + new Vector3Int(0, -1);
                Vector3Int maxHorizontal = rowListPositions[horizontalCount - 1] + new Vector3Int(0, 1);
                rowListPositions.Add(minHorizontal);
                rowListPositions.Add(maxHorizontal);

                int verticalCount = columnListPositions.Count;
                Vector3Int minVertical = columnListPositions[0] + new Vector3Int(-1, 0);
                Vector3Int maxVertical = columnListPositions[horizontalCount - 1] + new Vector3Int(1, 0);
                columnListPositions.Add(minVertical);
                columnListPositions.Add(maxVertical);

                BoundsInt horizontalCheckBounds = BoundsExtension.Encapsulate(rowListPositions);
                BoundsInt verticalCheckBounds = BoundsExtension.Encapsulate(columnListPositions);

                await UniTask.DelayFrame(3, PlayerLoopTiming.FixedUpdate, _token);
                _checkGridTask.CheckRange(horizontalCheckBounds);
                _checkGridTask.CheckRange(verticalCheckBounds);
            }
        }

        private void PlayEffect(Vector3Int checkPosition)
        {
            Vector3 position = _gridCellManager.Get(checkPosition).WorldPosition;
            EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Horizontal, position);
            EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Vertical, position);
            
            IGridCell horizontalGrid1 = _gridCellManager.Get(checkPosition + new Vector3Int(-1, 0));
            if(horizontalGrid1 != null)
            {
                Vector3 blastPosition = horizontalGrid1.WorldPosition;
                EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Vertical, blastPosition);
            }

            IGridCell horizontalGrid2 = _gridCellManager.Get(checkPosition + new Vector3Int(1, 0));
            if (horizontalGrid2 != null)
            {
                Vector3 blastPosition = horizontalGrid2.WorldPosition;
                EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Vertical, blastPosition);
            }

            IGridCell verticalGrid1 = _gridCellManager.Get(checkPosition + new Vector3Int(0, -1));
            if (verticalGrid1 != null)
            {
                Vector3 blastPosition = verticalGrid1.WorldPosition;
                EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Horizontal, blastPosition);
            }

            IGridCell verticalGrid2 = _gridCellManager.Get(checkPosition + new Vector3Int(0, 1));
            if (verticalGrid2 != null)
            {
                Vector3 blastPosition = verticalGrid2.WorldPosition;
                EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Horizontal, blastPosition);
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
