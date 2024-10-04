using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Gameplay.Miscs;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class SwapItemTask : IDisposable
    {
        private readonly BreakGridTask _breakGridTask;
        private readonly GridCellManager _gridCellManager;
        private readonly MatchItemsTask _matchItemsTask;
        private readonly SuggestTask _suggestTask;
        private readonly SwitchHand _switchHand;

        private readonly IPublisher<DecreaseMoveMessage> _decreaseMovePublisher;
        private readonly IPublisher<UseInGameBoosterMessage> _useInGameBoosterPublisher;

        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private ComboBoosterHandleTask _comboBoosterHandleTask;
        private CheckGridTask _checkGridTask;

        private const float SwapDuration = 0.1f;
        private const float HandDelay = 0.5834f;

        public SwapItemTask(GridCellManager gridCellManager, MatchItemsTask matchItemsTask, SuggestTask suggestTask, BreakGridTask breakGridTask, EffectDatabase effectDatabase)
        {
            _breakGridTask = breakGridTask;
            _gridCellManager = gridCellManager;
            _matchItemsTask = matchItemsTask;
            _suggestTask = suggestTask;
            _switchHand = effectDatabase.SwitchHand;

            _cts = new();
            _token = _cts.Token;

            _decreaseMovePublisher = GlobalMessagePipe.GetPublisher<DecreaseMoveMessage>();
            _useInGameBoosterPublisher = GlobalMessagePipe.GetPublisher<UseInGameBoosterMessage>();
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

            IItemAnimation fromAnimation = fromItem as IItemAnimation;
            IItemAnimation toAnimation = toItem as IItemAnimation;

            UseSwapBooster();
            bool isCollectible = CheckCollectible(fromCell, toCell);
            var switchHand = SimplePool.Spawn(_switchHand, EffectContainer.Transform, fromCell.WorldPosition, Quaternion.identity);
            await UniTask.Delay(TimeSpan.FromSeconds(HandDelay), cancellationToken: _token);
            switchHand.Switch(toPosition - fromPosition);

            UniTask fromMoveTask = fromAnimation.SwapTo(toCell.WorldPosition, SwapDuration, true);
            UniTask toMoveTask = toAnimation.SwapTo(fromCell.WorldPosition, SwapDuration, false);
            await UniTask.WhenAll(fromMoveTask, toMoveTask);

            fromCell.SetBlockItem(toItem);
            toCell.SetBlockItem(fromItem);

            toItem.SetWorldPosition(fromCell.WorldPosition);
            fromItem.SetWorldPosition(toCell.WorldPosition);

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
                ICollectible collectible = collectItem as ICollectible;

                await collectible.Collect();
                _breakGridTask.ReleaseGridCell(collectCell);
                _checkGridTask.CheckAroundPosition(collectCell.GridPosition, 1);
            }

            else
            {
                _matchItemsTask.CheckMatchInSwap(fromPosition);
                _matchItemsTask.CheckMatchInSwap(toPosition);
            }
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

            IItemAnimation fromAnimation = fromItem as IItemAnimation;
            IItemAnimation toAnimation = toItem as IItemAnimation;

            if (!isSwapBack)
            {
                EffectManager.Instance.PlayItemSwapEffect(fromCell.WorldPosition);
                EffectManager.Instance.PlayItemSwapEffect(toCell.WorldPosition);
            }

            UniTask fromMoveTask = fromAnimation.SwapTo(toCell.WorldPosition, SwapDuration, true);
            UniTask toMoveTask = toAnimation.SwapTo(fromCell.WorldPosition, SwapDuration, false);
            await UniTask.WhenAll(fromMoveTask, toMoveTask);

            if (_comboBoosterHandleTask.IsComboBooster(fromCell, toCell))
            {
                DecreaseMove();
                await SwapComboBooster(fromCell, toCell, fromItem, toItem);
            }

            else if (_comboBoosterHandleTask.IsSwapToColorful(fromCell, toCell))
            {
                DecreaseMove();
                await SwapToColorful(fromCell, toCell, fromItem, toItem);
            }

            else if (CheckCollectible(fromCell, toCell))
            {
                DecreaseMove();
                await SwapCollectible(fromCell, toCell, fromItem, toItem, isSwapBack);
            }

            else
            {
                await SwapItems(fromCell, toCell, fromItem, toItem, isSwapBack);
            }
        }

        private async UniTask SwapComboBooster(IGridCell fromCell, IGridCell toCell, IBlockItem fromItem, IBlockItem toItem)
        {
            fromCell.SetBlockItem(toItem);
            toCell.SetBlockItem(fromItem);

            toItem.SetWorldPosition(fromCell.WorldPosition);
            fromItem.SetWorldPosition(toCell.WorldPosition);

            await _comboBoosterHandleTask.HandleComboBooster(fromCell, toCell);
        }

        private async UniTask SwapToColorful(IGridCell fromCell, IGridCell toCell, IBlockItem fromItem, IBlockItem toItem)
        {
            fromCell.SetBlockItem(toItem);
            toCell.SetBlockItem(fromItem);
            
            toItem.SetWorldPosition(fromCell.WorldPosition);
            fromItem.SetWorldPosition(toCell.WorldPosition);

            await _comboBoosterHandleTask.CombineColorfulItemWithColorItem(fromCell, toCell);
        }

        private async UniTask SwapCollectible(IGridCell fromCell, IGridCell toCell, IBlockItem fromItem, IBlockItem toItem, bool isSwapBack)
        {
            IGridCell currentGrid;
            IGridCell remainGrid;
            IBlockItem blockItem;

            fromCell.SetBlockItem(toItem);
            toCell.SetBlockItem(fromItem);

            toItem.SetWorldPosition(fromCell.WorldPosition);
            fromItem.SetWorldPosition(toCell.WorldPosition);

            if(fromItem is not ICollectible && toItem is not ICollectible)
            {
                if(isSwapBack)
                    await CheckMatchOnSwap(fromCell, toCell);
                return;
            }

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

        private async UniTask SwapItems(IGridCell fromCell, IGridCell toCell, IBlockItem fromItem, IBlockItem toItem, bool isSwapBack)
        {
            fromCell.SetBlockItem(toItem);
            toCell.SetBlockItem(fromItem);

            toItem.SetWorldPosition(fromCell.WorldPosition);
            fromItem.SetWorldPosition(toCell.WorldPosition);

            if (isSwapBack)
                await CheckMatchOnSwap(fromCell, toCell);
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
                    fromCell.LockStates = toCell.LockStates = LockStates.None;
                    await SwapItem(toCell.GridPosition, fromCell.GridPosition, false);
                    EffectManager.Instance.PlaySoundEffect(SoundEffectType.Error);
                }

                else DecreaseMove();
            }

            else
            {
                DecreaseMove();
                _matchItemsTask.CheckMatchInSwap(fromCell.GridPosition);
            }
        }

        private void UseSwapBooster()
        {
            _useInGameBoosterPublisher.Publish(new UseInGameBoosterMessage
            {
                BoosterType = InGameBoosterType.Swap
            });
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

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
