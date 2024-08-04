using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class CheckGridTask
    {
        private readonly GridCellManager _gridCellManager;

        public CheckGridTask(GridCellManager gridCellManager)
        {
            _gridCellManager = gridCellManager;
        }

        public bool Check(Vector3Int checkPosition)
        {
            IGridCell gridCell = _gridCellManager.Get(checkPosition);

            if (gridCell == null)
                return false;

            if (!gridCell.CanSetItem)
                return false;

            return true;
        }
    }
}
