using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.GridCells
{
    public class GridCell : IGridCell
    {
        private IBlockItem _blockItem;
        private IGridStateful _gridStateful;
        private IGridCellView _gridCellView;

        public bool HasItem => _blockItem != null;
        public bool IsMoveable => LockStates == LockStates.None;
        public bool CanSetItem => _gridStateful.CanContainItem && !_gridStateful.IsLocked;
        public bool CanContainItem => _gridStateful.CanContainItem;

        public bool IsSpawner { get; set; }
        public bool CanPassThrough { get; set; }
        public bool IsExitable { get; set; }

        public ItemType ItemType => _blockItem.ItemType;

        public CandyColor CandyColor => _blockItem.CandyColor;

        public LockStates LockStates { get; set; }

        public Vector3Int GridPosition { get; set; }

        public Vector3 WorldPosition { get; set; }

        public IGridStateful GridStateful => _gridStateful;

        public IGridCellView GridCellView => _gridCellView;

        public IBlockItem BlockItem => _blockItem;

        public void ReleaseGrid()
        {
            if (BlockItem != null)
                BlockItem.ReleaseItem();

            SetBlockItem(null);
        }

        public void SetBlockItem(IBlockItem blockItem)
        {
            _blockItem = blockItem;
            
            if (_blockItem != null)
            {
                _blockItem.GridPosition = GridPosition;
                _blockItem.SetWorldPosition(WorldPosition);
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
    }
}
