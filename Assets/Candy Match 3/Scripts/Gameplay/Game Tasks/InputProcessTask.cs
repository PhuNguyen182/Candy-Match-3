using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GameInput;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Constants;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class InputProcessTask : IDisposable
    {
        private readonly BoardInput _boardInput;
        private readonly GridCellManager _gridCellManager;
        private readonly SwapItemTask _swapItemTask;

        private bool _isSwapped = false;

        private Vector2 _movePosition;
        private Vector3 _selectedPosition;
        private Vector3Int _selectedGridPosition;

        private IGridCell _selectecGridCell;
        private IGridCell _processGridCell;

        private IDisposable _disposable;

        public bool IsActive { get; set; }

        public InputProcessTask(BoardInput boardInput, GridCellManager gridCellManager, SwapItemTask swapItemTask)
        {
            _boardInput = boardInput;
            _gridCellManager = gridCellManager;
            _swapItemTask = swapItemTask;

            DisposableBuilder builder = Disposable.CreateBuilder();
            Observable.EveryUpdate(UnityFrameProvider.Update)
                      .Subscribe(_ => OnUpdate())
                      .AddTo(ref builder);
            _disposable = builder.Build();
        }

        private void OnUpdate()
        {
            if (IsActive)
            {
                OnPress();
                OnDrag();
                OnRelease();
            }
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
                _selectedPosition = _boardInput.WorldMoudePosition;
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
            Vector2 moveDirection = _boardInput.WorldMoudePosition - _selectedPosition;

            if (moveDirection.sqrMagnitude < Match3Constants.TouchMoveTorerance)
                return;

            _isSwapped = true;
            Vector3Int swapDirection = GetSwapDirection(moveDirection);
            Vector3Int swapToPosition = checkPosition + swapDirection;
            _swapItemTask.SwapItem(checkPosition, swapToPosition, true).Forget();

            // Reset this variable to null to prevent duplicated action inside Update loop
            _selectecGridCell = null;
        }

        private void OnRelease()
        {
            if (!_boardInput.IsReleased)
                return;

            if (_processGridCell == null)
                return;

            ProcessItemBounce(_processGridCell, _isSwapped);

            _selectecGridCell = null;
            _processGridCell = null;
            _isSwapped = false;
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

            if (gridCell.BlockItem is IItemAnimation animation)
            {
                if (!gridCell.IsLocked)
                    animation.BounceOnTap();

                Debug.Log(gridCell.BlockItem.GetName());
            }
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
