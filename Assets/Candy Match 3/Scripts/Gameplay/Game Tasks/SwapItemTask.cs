using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class SwapItemTask
    {
        private readonly BreakGridTask _breakGridTask;
        private readonly GridCellManager _gridCellManager;
        private readonly MatchItemsTask _matchItemsTask;
        private readonly SuggestTask _suggestTask;

        private readonly IPublisher<DecreaseMoveMessage> _decreaseMovePublisher;
        private readonly IPublisher<UseInGameBoosterMessage> _useInGameBoosterPublisher;

        private ComboBoosterHandleTask _comboBoosterHandleTask;
        private CheckGridTask _checkGridTask;

        public SwapItemTask(GridCellManager gridCellManager, MatchItemsTask matchItemsTask, SuggestTask suggestTask, BreakGridTask breakGridTask)
        {
            _breakGridTask = breakGridTask;
            _gridCellManager = gridCellManager;
            _matchItemsTask = matchItemsTask;
            _suggestTask = suggestTask;

            _decreaseMovePublisher = GlobalMessagePipe.GetPublisher<DecreaseMoveMessage>();
        }

        public async UniTask SwapItem(Vector3Int fromPosition, Vector3Int toPosition, bool isSwapBack)
        {
            if (fromPosition == toPosition)
                return;

            _suggestTask.Suggest(false);
            IGridCell fromCell = _gridCellManager.Get(fromPosition);
            IGridCell toCell = _gridCellManager.Get(toPosition);

            if (fromCell == null || toCell == null)
                return;

            if (!fromCell.HasItem || !toCell.HasItem)
                return;

            if (!fromCell.IsMoveable || !toCell.IsMoveable)
                return;

            IBlockItem fromItem = fromCell.BlockItem;
            IBlockItem toItem = toCell.BlockItem;

            if (fromItem is not IItemAnimation fromAnimation || toItem is not IItemAnimation toAnimation)
                return;

            fromCell.LockStates = LockStates.Swapping;
            toCell.LockStates = LockStates.Swapping;

            UniTask fromMoveTask = fromAnimation.SwapTo(toCell.WorldPosition, 0.1f, true);
            UniTask toMoveTask = toAnimation.SwapTo(fromCell.WorldPosition, 0.1f, false);
            await UniTask.WhenAll(fromMoveTask, toMoveTask);

            if (_comboBoosterHandleTask.IsComboBooster(fromCell, toCell))
            {
                DecreaseMove();
                fromCell.SetBlockItem(toItem);
                toItem.SetWorldPosition(fromCell.WorldPosition);
                toCell.SetBlockItem(fromItem);
                fromItem.SetWorldPosition(toCell.WorldPosition);

                fromCell.LockStates = LockStates.None;
                toCell.LockStates = LockStates.None;

                await _comboBoosterHandleTask.HandleComboBooster(fromCell, toCell);
            }

            else if (_comboBoosterHandleTask.IsSwapToColorful(fromCell, toCell))
            {
                DecreaseMove();
                fromCell.SetBlockItem(toItem);
                toItem.SetWorldPosition(fromCell.WorldPosition);
                toCell.SetBlockItem(fromItem);
                fromItem.SetWorldPosition(toCell.WorldPosition);

                fromCell.LockStates = LockStates.None;
                toCell.LockStates = LockStates.None;

                await _comboBoosterHandleTask.CombineColorfulItemWithColorItem(fromCell, toCell);
            }

            else if (CheckCollectible(fromCell, toCell))
            {
                IGridCell currentGrid;
                IGridCell remainGrid;
                IBlockItem blockItem;

                DecreaseMove();
                fromCell.SetBlockItem(toItem);
                toItem.SetWorldPosition(fromCell.WorldPosition);
                toCell.SetBlockItem(fromItem);
                fromItem.SetWorldPosition(toCell.WorldPosition);

                fromCell.LockStates = LockStates.None;
                toCell.LockStates = LockStates.None;

                if (fromCell.IsCollectible)
                {
                    currentGrid = fromCell;
                    remainGrid = toCell;
                    blockItem = fromCell.BlockItem;
                }

                else
                {
                    currentGrid = toCell;
                    remainGrid = fromCell;
                    blockItem = toCell.BlockItem;
                }

                if (blockItem is ICollectible collectible)
                {
                    await collectible.Collect();
                    _breakGridTask.ReleaseGridCell(currentGrid);
                    _checkGridTask.CheckAroundPosition(currentGrid.GridPosition, 1);
                }

                _matchItemsTask.CheckMatchInSwap(remainGrid.GridPosition);
            }

            else
            {
                fromCell.SetBlockItem(toItem);
                toItem.SetWorldPosition(fromCell.WorldPosition);
                toCell.SetBlockItem(fromItem);
                fromItem.SetWorldPosition(toCell.WorldPosition);

                fromCell.LockStates = LockStates.None;
                toCell.LockStates = LockStates.None;

                if (isSwapBack)
                {
                    await CheckMatchOnSwap(fromCell, toCell);
                }
            }
        }

        public async UniTask SwapForward(Vector3Int fromPosition, Vector3Int toPosition)
        {
            if (fromPosition == toPosition)
                return;

            _suggestTask.Suggest(false);
            IGridCell fromCell = _gridCellManager.Get(fromPosition);
            IGridCell toCell = _gridCellManager.Get(toPosition);

            if (fromCell == null || toCell == null)
                return;

            if (!fromCell.HasItem || !toCell.HasItem)
                return;

            if (!fromCell.IsMoveable || !toCell.IsMoveable)
                return;

            IBlockItem fromItem = fromCell.BlockItem;
            IBlockItem toItem = toCell.BlockItem;

            if (fromItem is not IItemAnimation fromAnimation || toItem is not IItemAnimation toAnimation)
                return;

            _useInGameBoosterPublisher.Publish(new UseInGameBoosterMessage
            {
                BoosterType = InGameBoosterType.Swap
            });

            fromCell.LockStates = LockStates.Swapping;
            toCell.LockStates = LockStates.Swapping;
            bool isCollectible = CheckCollectible(fromCell, toCell);

            UniTask fromMoveTask = fromAnimation.SwapTo(toCell.WorldPosition, 0.1f, true);
            UniTask toMoveTask = toAnimation.SwapTo(fromCell.WorldPosition, 0.1f, false);
            await UniTask.WhenAll(fromMoveTask, toMoveTask);

            fromCell.SetBlockItem(toItem);
            toItem.SetWorldPosition(fromCell.WorldPosition);
            toCell.SetBlockItem(fromItem);
            fromItem.SetWorldPosition(toCell.WorldPosition);

            fromCell.LockStates = LockStates.None;
            toCell.LockStates = LockStates.None;

            if (isCollectible)
            {
                IBlockItem collectItem;
                IGridCell collectCell, remainCell;

                if (fromCell.IsCollectible)
                {
                    collectCell = fromCell;
                    remainCell = toCell;
                }

                else
                {
                    collectCell = toCell;
                    remainCell = fromCell;
                }

                collectItem = collectCell.BlockItem;
                _matchItemsTask.CheckMatchInSwap(remainCell.GridPosition);

                if(collectItem is ICollectible collectible)
                {
                    await collectible.Collect();
                    _breakGridTask.ReleaseGridCell(collectCell);
                    _checkGridTask.CheckAroundPosition(collectCell.GridPosition, 1);
                }
            }

            else
            {
                _matchItemsTask.CheckMatchInSwap(fromPosition);
                _matchItemsTask.CheckMatchInSwap(toPosition);
            }
        }

        private bool CheckCollectible(IGridCell fromCell, IGridCell toCell)
        {
            return fromCell.IsCollectible || toCell.IsCollectible;
        }

        private async UniTask CheckMatchOnSwap(IGridCell fromCell, IGridCell toCell)
        {
            bool isMatchedTo = _matchItemsTask.CheckMatchInSwap(toCell.GridPosition);

            if (!isMatchedTo)
            {
                bool isMatchedFrom = _matchItemsTask.CheckMatchInSwap(fromCell.GridPosition);

                if (!isMatchedFrom)
                {
                    fromCell.LockStates = LockStates.None;
                    toCell.LockStates = LockStates.None;
                    await SwapItem(toCell.GridPosition, fromCell.GridPosition, false);
                    EffectManager.Instance.PlaySoundEffect(SoundEffectType.Error);
                }

                else
                    DecreaseMove();
            }

            else
            {
                DecreaseMove();
                _matchItemsTask.CheckMatchInSwap(fromCell.GridPosition);
            }
        }

        private void DecreaseMove()
        {
            _decreaseMovePublisher.Publish(new DecreaseMoveMessage { CanDecrease = true });
        }

        public void SetComboBoosterHandler(ComboBoosterHandleTask comboBoosterHandleTask)
        {
            _comboBoosterHandleTask = comboBoosterHandleTask;
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }
    }
}
