using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Strategies;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class BreakGridTask
    {
        private readonly MetaItemManager _metaItemManager;
        private readonly GridCellManager _gridCellManager;
        private readonly List<Vector3Int> _adjacentStepCheck;
        
        private CheckGridTask _checkGridTask;

        public BreakGridTask(GridCellManager gridCellManager, MetaItemManager metaItemManager)
        {
            _gridCellManager = gridCellManager;
            _metaItemManager = metaItemManager;
            
            _adjacentStepCheck = new()
            {
                new(1, 0), new(0, 1), new(-1, 0), new(0, -1)
            };
        }

        public async UniTask<bool> Break(IGridCell gridCell)
        {
            if (gridCell == null)
                return false;

            if (!gridCell.HasItem)
                return false;

            IBlockItem blockItem = gridCell.BlockItem;

            if (blockItem is IBooster booster)
            {
                await booster.Activate(); // use activate booster task later
                ReleaseGridCell(gridCell);
                return true;
            }

            if (blockItem is IBreakable breakable)
            {
                if (breakable.Break())
                {
                    // To do: execute breaking stateful before execute break grid
                    await blockItem.ItemBlast();
                    ReleaseGridCell(gridCell);
                    return true;
                }
            }

            return false;
        }

        public async UniTask BreakMatch(List<IGridCell> matchCells, MatchType matchType)
        {
            using (var listPool = ListPool<UniTask>.Get(out List<UniTask> breakTasks))
            {
                for (int i = 0; i < matchCells.Count; i++)
                {
                    breakTasks.Add(Break(matchCells[i]));
                    breakTasks.Add(BreakAdjacent(matchCells[i]));
                }

                await UniTask.WhenAll(breakTasks);
            }
        }

        private async UniTask BreakAdjacent(IGridCell gridCell)
        {
            for (int i = 0; i < _adjacentStepCheck.Count; i++)
            {
                Vector3Int breakPosition = gridCell.GridPosition + _adjacentStepCheck[i];
                IGridCell breakGridCell = _gridCellManager.Get(breakPosition);

                if (breakGridCell == null || !breakGridCell.HasItem)
                    continue;

                if(breakGridCell.BlockItem is IAdjcentBreakable breakable)
                {
                    breakable.Break();
                }
            }

            await UniTask.CompletedTask;
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        private void ReleaseGridCell(IGridCell gridCell)
        {
            gridCell.SetBlockItem(null);
            _metaItemManager.ReleaseGridCell(gridCell.GridPosition);
        }
    }
}
