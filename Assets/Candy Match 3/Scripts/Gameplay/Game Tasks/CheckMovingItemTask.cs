using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;
using GlobalScripts.Extensions;
using System.Linq;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class CheckMovingItemTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;

        public CheckMovingItemTask(GridCellManager gridCellManager)
        {
            _gridCellManager = gridCellManager;

            List<ReactiveProperty<bool>> gridCells = new();
            var activeRange = _gridCellManager.GetActiveBounds();

            foreach (var position in activeRange.Iterator2D())
            {
                IGridCell cell = _gridCellManager.Get(position);
                gridCells.Add(cell.CheckLockProperty);
            }

            
        }

        public void Dispose()
        {

        }
    }
}
