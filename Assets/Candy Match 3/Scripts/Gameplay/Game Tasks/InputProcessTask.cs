using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Gameplay.GameInput;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks;
using CandyMatch3.Scripts.Common.Constants;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class InputProcessTask : IDisposable
    {
        private readonly BoardInput _boardInput;
        private readonly GridCellManager _gridCellManager;
        private readonly SwapItemTask _swapItemTask;

        private readonly ISubscriber<BoardStopMessage> _boardStopSubscriber;
        private InGameBoosterTasks _inGameBoosterTasks;

        private bool _isSwapped = false;
        private bool _isBoardStopped = false;

        private Vector2 _movePosition;
        private Vector3 _selectedPosition;
        private Vector3Int _selectedGridPosition;

        private IGridCell _selectecGridCell;
        private IGridCell _processGridCell;

        private IDisposable _disposable;
        private IDisposable _messageDisposable;

        public bool IsActive { get; set; }

        public InputProcessTask(BoardInput boardInput, GridCellManager gridCellManager, SwapItemTask swapItemTask)
        {
            _boardInput = boardInput;
            _gridCellManager = gridCellManager;
            _swapItemTask = swapItemTask;

            DisposableBagBuilder messageBuilder = MessagePipe.DisposableBag.CreateBuilder();
            _boardStopSubscriber = GlobalMessagePipe.GetSubscriber<BoardStopMessage>();
            _boardStopSubscriber.Subscribe(message => _isBoardStopped = message.IsStopped)
                                .AddTo(messageBuilder);
            _messageDisposable = messageBuilder.Build();

            DisposableBuilder builder = Disposable.CreateBuilder();
            Observable.EveryUpdate(UnityFrameProvider.Update)
                      .Subscribe(_ => OnUpdate()).AddTo(ref builder);
            _disposable = builder.Build();
        }

        private void OnUpdate()
        {
            if (IsActive && _isBoardStopped)
            {
                OnPress();
                OnDrag();
                OnRelease();
            }
        }

        public void SetInGameBoosterTasks(InGameBoosterTasks inGameBoosterTasks)
        {
            _inGameBoosterTasks = inGameBoosterTasks;
        }

        private void OnPress()
        {
            if (!_boardInput.IsPressed)
                return;

            GridCellView gridCellView = _boardInput.GetGridCellView();

            if (gridCellView == null)
                return;

            IGridCell gridCell = _gridCellManager.Get(gridCellView.GridPosition);

            if (gridCell != null)
            {
                _selectecGridCell = gridCell;
                _processGridCell = gridCell;
                _selectedPosition = _boardInput.WorldPointerPosition;
                _selectedGridPosition = gridCell.GridPosition;
            }
        }

        private void OnDrag()
        {
            if (!_boardInput.IsDragging)
                return;

            if (_selectecGridCell == null || !_selectecGridCell.HasItem)
                return;

            Vector3Int checkPosition = _selectecGridCell.GridPosition;
            Vector2 moveDirection = _boardInput.WorldPointerPosition - _selectedPosition;

            if (moveDirection.sqrMagnitude < Match3Constants.TouchMoveTorerance)
                return;

            _isSwapped = true;
            
            if (_inGameBoosterTasks.IsBoosterUsed)
            {
                if(_inGameBoosterTasks.CurrentBooster == InGameBoosterType.Swap)
                    SwapForward(checkPosition, moveDirection).Forget();
            }

            else
                SwapItem(checkPosition, moveDirection).Forget();

            _selectecGridCell = null; // Reset this variable to null to prevent duplicated action inside Update loop
        }

        private void OnRelease()
        {
            if (!_boardInput.IsReleased)
                return;

            if (_processGridCell == null)
                return;

            ProcessItemOnRelease(_processGridCell, _isSwapped).Forget();

            _selectecGridCell = null;
            _processGridCell = null;
            _isSwapped = false;
        }

        private async UniTask SwapForward(Vector3Int checkPosition, Vector3 moveDirection)
        {
            IsActive = false;
            Vector3Int swapDirection = GetSwapDirection(moveDirection);
            Vector3Int swapToPosition = checkPosition + swapDirection;
            await _inGameBoosterTasks.ActivateSwapBooster(checkPosition, swapToPosition);
            IsActive = true;
        }

        private async UniTask SwapItem(Vector3Int checkPosition, Vector3 moveDirection)
        {
            Vector3Int swapDirection = GetSwapDirection(moveDirection);
            Vector3Int swapToPosition = checkPosition + swapDirection;
            await _swapItemTask.SwapItem(checkPosition, swapToPosition, true);
        }

        private async UniTask ProcessItemOnRelease(IGridCell gridCell, bool isSwapped)
        {
            if (_inGameBoosterTasks.IsBoosterUsed)
            {
                Vector3Int position = gridCell.GridPosition;
                InGameBoosterType boosterType = _inGameBoosterTasks.CurrentBooster;
                
                if(boosterType != InGameBoosterType.Swap)
                    await _inGameBoosterTasks.ActivatePointBooster(position, boosterType);
            }

            else ProcessItemBounce(gridCell, isSwapped);
        }

        private Vector3Int GetSwapDirection(Vector3 direction)
        {
            if (IsCloseToDirection(Vector2.up, direction))
                return Vector3Int.up;
            
            else if (IsCloseToDirection(Vector2.left, direction))
                return Vector3Int.left;
            
            else if (IsCloseToDirection(Vector2.down, direction))
                return Vector3Int.down;
            
            else if (IsCloseToDirection(Vector2.right, direction))
                return Vector3Int.right;

            return Vector3Int.zero;
        }

        private bool IsCloseToDirection(Vector2 checkDirection, Vector2 closeDirection)
        {
            float dotProduct = Vector2.Dot(checkDirection, closeDirection.normalized);
            return dotProduct >= 0.5f && dotProduct <= 1f;
        }

        private void ProcessItemBounce(IGridCell gridCell, bool isSwapped)
        {
            if (gridCell == null || !gridCell.HasItem || isSwapped)
                return;

            if (gridCell.BlockItem is IItemSuggest suggest)
                if (suggest.IsSuggesting)
                    return;

            if (gridCell.BlockItem is IItemAnimation animation)
            {
                if (!gridCell.IsLocked)
                    animation.BounceOnTap();
            }

            PrintItemName(gridCell);
        }

        private void PrintItemName(IGridCell gridCell)
        {
#if UNITY_EDITOR
            Debug.Log(gridCell.BlockItem.GetName());
#endif
        }

        public void Dispose()
        {
            _disposable.Dispose();
            _messageDisposable.Dispose();
        }
    }
}
