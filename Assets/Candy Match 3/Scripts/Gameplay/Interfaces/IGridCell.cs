using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IGridCell
    {
        public int GridID { get; }
        public bool CanMove { get; }
        public bool HasItem { get; }
        public bool IsLocked { get; }
        public bool IsItemLocked { get; }
        public bool IsMoveable { get; }
        public bool CanSetItem { get; }
        public bool CanContainItem { get; }
        public bool IsAvailable { get; }
        
        public bool IsMoving { get; set; }
        public bool IsSpawner { get; set; }
        public bool CanPassThrough { get; set; }
        public bool IsCollectible { get; set; }

        public ItemType ItemType { get; }
        public LockStates LockStates { get; set; }
        public CandyColor CandyColor { get; }

        public Vector3 WorldPosition { get; }
        public Vector3Int GridPosition { get; set; }
        public IGridStateful GridStateful { get; }
        public IGridCellView GridCellView { get; }
        public IBlockItem BlockItem { get; }

        public ReactiveProperty<bool> GridLockProperty { get; }

        public void SetGridID(int gridId);
        public void SetGridCellViewPosition();
        public void SetBlockItem(IBlockItem blockItem, bool isSnapped = true);
        public void SetWorldPosition(Vector3 position);
        public void SetGridStateful(IGridStateful stateful);
        public void SetGridCellView(IGridCellView gridView);
        public void ReleaseGrid();
    }
}
