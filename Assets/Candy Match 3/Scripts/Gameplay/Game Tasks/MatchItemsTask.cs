using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Models.Match;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class MatchItemsTask
    {
        private readonly MatchModel _matchModel;
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;

        public MatchItemsTask(GridCellManager gridCellManager, BreakGridTask breakGridTask)
        {
            _gridCellManager = gridCellManager;
            _matchModel = new(_gridCellManager);
            _breakGridTask = breakGridTask;
        }

        public void CheckMatch(Vector3Int position, Vector3Int inDirection)
        {
            List<IGridCell> matchedGridCell = new();
            
            if (_matchModel.CheckMatch(position, inDirection, out matchedGridCell))
            {
                // Execute logic here
            }
        }
    }
}
