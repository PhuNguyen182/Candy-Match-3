using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class BreakGridTask
    {
        private readonly GridCellManager _gridCellManager;

        public BreakGridTask(GridCellManager gridCellManager)
        {
            _gridCellManager = gridCellManager;
        }

        public async UniTask Break(IGridCell gridCell)
        {
            if (gridCell == null)
                return;

            IBlockItem blockItem = gridCell.BlockItem;

            if (blockItem == null)
                return;

            if (blockItem is IBooster booster)
                await booster.Activate(); // use activate booster task later

            if (blockItem is IBreakable breakable)
            {
                if (breakable.Break())
                {
                    // To do: execute breaking stateful before execute break grid
                    await blockItem.ItemBlast();
                    gridCell.SetBlockItem(null);
                }
            }
        }
    }
}
