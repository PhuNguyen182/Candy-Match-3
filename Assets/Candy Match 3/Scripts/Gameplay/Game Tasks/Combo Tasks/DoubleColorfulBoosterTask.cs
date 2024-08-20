using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using Cysharp.Threading.Tasks;
using GlobalScripts.Extensions;
using CandyMatch3.Scripts.Gameplay.Interfaces;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks
{
    public class DoubleColorfulBoosterTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;

        private CheckGridTask _checkGridTask;

        public DoubleColorfulBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask)
        {
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;
        }

        public async UniTask Activate(IGridCell gridCell1, IGridCell gridCell2)
        {
            using(var listPool = ListPool<Vector3Int>.Get(out List<Vector3Int> positions))
            {
                positions.AddRange(_gridCellManager.GetAllPositions());

                using var oddListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> oddPositions);
                using var evenListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> evenPositions);

                for (int i = 0; i < positions.Count; i++)
                {
                    IGridCell gridCell = _gridCellManager.Get(positions[i]);

                    if (!gridCell.HasItem)
                        continue;

                    var gridCellType = GetCellPositionType(positions[i]);
                    
                    if (gridCellType == GridPositionType.Odd)
                        oddPositions.Add(positions[i]);
                    
                    else if (gridCellType == GridPositionType.Even)
                        evenPositions.Add(positions[i]);
                }
            }

            await UniTask.CompletedTask;
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

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        public void Dispose()
        {

        }
    }
}
