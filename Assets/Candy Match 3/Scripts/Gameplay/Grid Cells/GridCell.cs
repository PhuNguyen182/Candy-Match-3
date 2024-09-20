using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.GridCells
{
    public class GridCell : IGridCell
    {
        private int _gridId;
        private Vector3 _worldPosition;

        private IBlockItem _blockItem;
        private LockStates _lockStates;
        private IGridStateful _gridStateful;
        private IGridCellView _gridCellView;

        public GridCell()
        {
            CheckLockProperty = new();
        }

        public int GridID => _gridId;
        public bool HasItem => _blockItem != null;
        public bool CanMove => !_gridStateful.IsLocked;
        public bool IsLocked => LockStates != LockStates.None;
        public bool IsItemLocked => HasItem ? _blockItem.IsLocking : false;
        public bool IsAvailable => _gridStateful != null && _gridStateful.IsAvailable;
        public bool IsMoveable => HasItem ? (!IsLocked && !_gridStateful.IsLocked && _blockItem.IsMoveable) : false;
        public bool CanSetItem => _gridStateful.CanContainItem && !_gridStateful.IsLocked && !HasItem;
        public bool CanContainItem => _gridStateful.CanContainItem;

        public bool IsSpawner { get; set; }
        public bool CanPassThrough { get; set; }
        public bool IsMatching { get; set; }
        public bool IsCollectible { get; set; }

        public bool IsMoving { get; set; }

        public ItemType ItemType => HasItem ? _blockItem.ItemType : ItemType.None;
        public CandyColor CandyColor => HasItem ? _blockItem.CandyColor : CandyColor.None;
        public LockStates LockStates
        {
            get => _lockStates;
            set
            {
                _lockStates = value;
                CheckLockProperty.Value = IsLocked || IsItemLocked;
            }
        }

        public Vector3 WorldPosition => _worldPosition;
        public Vector3Int GridPosition { get; set; }

        public IGridStateful GridStateful => _gridStateful;
        public IGridCellView GridCellView => _gridCellView;
        public IBlockItem BlockItem => _blockItem;

        public ReactiveProperty<bool> CheckLockProperty { get; }

        public void ReleaseGrid()
        {
            if (BlockItem != null)
                BlockItem.ReleaseItem();

            SetBlockItem(null);
        }

        public void SetGridID(int gridId)
        {
            _gridId = gridId;
        }

        public void SetBlockItem(IBlockItem blockItem, bool isSnapped = true)
        {
            IsMatching = false;
            _blockItem = blockItem;
            
            if (_blockItem != null)
            {
                _blockItem.GridPosition = GridPosition;

                if (isSnapped)
                    blockItem.SetWorldPosition(WorldPosition);
            }
        }

        public void SetGridCellView(IGridCellView gridView)
        {
            _gridCellView = gridView;
            _gridCellView.SetGridPosition(GridPosition);
        }

        public void SetGridCellViewPosition()
        {
            _gridCellView.SetWorldPosition(WorldPosition);
        }

        public void SetGridStateful(IGridStateful stateful)
        {
            _gridStateful = stateful;
        }

        public void SetWorldPosition(Vector3 position)
        {
            _worldPosition = position;
        }
    }
}
