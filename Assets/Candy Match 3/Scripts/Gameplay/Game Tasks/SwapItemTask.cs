using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class SwapItemTask
    {
        private readonly GridCellManager _gridCellManager;
        private readonly MatchItemsTask _matchItemsTask;

        public SwapItemTask(GridCellManager gridCellManager, MatchItemsTask matchItemsTask)
        {
            _gridCellManager = gridCellManager;
            _matchItemsTask = matchItemsTask;
        }

        public async UniTask SwapItem(Vector3Int fromPosition, Vector3Int toPosition, bool isSwapBack)
        {
            if (fromPosition == toPosition)
                return;

            IGridCell fromCell = _gridCellManager.Get(fromPosition);
            IGridCell toCell = _gridCellManager.Get(toPosition);

            if (fromCell == null || toCell == null)
                return;

            if (!fromCell.HasItem || !toCell.HasItem)
                return;

            IBlockItem fromItem = fromCell.BlockItem;
            IBlockItem toItem = toCell.BlockItem;

            if (!fromCell.IsMoveable || !toItem.IsMoveable)
                return;

            if(fromItem is IItemAnimation fromAnimation && toItem is IItemAnimation toAnimation)
            {
                UniTask fromMoveTask = fromAnimation.SwapTo(toCell.WorldPosition, 0.1f, true);
                UniTask toMoveTask = toAnimation.SwapTo(fromCell.WorldPosition, 0.1f, false);
                await UniTask.WhenAll(fromMoveTask, toMoveTask);
            }

            fromCell.SetBlockItem(toItem);
            toItem.SetWorldPosition(fromCell.WorldPosition);
            toCell.SetBlockItem(fromItem);
            fromItem.SetWorldPosition(toCell.WorldPosition);

            if (isSwapBack)
            {
                CheckMatchOnSwap(fromPosition, toPosition).Forget();
            }
        }

        private async UniTask CheckMatchOnSwap(Vector3Int fromPosition, Vector3Int toPosition)
        {
            bool isMatchedTo = _matchItemsTask.CheckMatch(toPosition);
            bool isMatchedFrom = _matchItemsTask.CheckMatch(fromPosition);
            bool isMatched = isMatchedTo || isMatchedFrom;

            if (!isMatched)
                await SwapItem(toPosition, fromPosition, false);
        }
    }
}
