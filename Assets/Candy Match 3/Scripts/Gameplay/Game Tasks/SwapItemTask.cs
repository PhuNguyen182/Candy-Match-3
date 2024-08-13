using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;

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

            fromCell.LockStates = LockStates.Swapping;
            toCell.LockStates = LockStates.Swapping;

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
                CheckMatchOnSwap(fromCell, toCell).Forget();
            }

            fromCell.LockStates = LockStates.None;
            toCell.LockStates = LockStates.None;
        }

        private async UniTask CheckMatchOnSwap(IGridCell fromCell, IGridCell toCell)
        {
            bool isMatchedTo = _matchItemsTask.CheckMatch(toCell.GridPosition);
            bool isMatchedFrom = _matchItemsTask.CheckMatch(fromCell.GridPosition);
            bool isMatched = isMatchedFrom || isMatchedTo;

            if (!isMatched)
            {
                fromCell.LockStates = LockStates.None;
                toCell.LockStates = LockStates.None;
                await SwapItem(toCell.GridPosition, fromCell.GridPosition, false);
            }
        }
    }
}
