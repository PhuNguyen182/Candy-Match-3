using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IGridCell
    {
        public bool HasItem { get; }
        public bool IsMoveable { get; }
        public bool CanSetItem { get; }
        public bool CanContainItem { get; }
        
        public bool IsSpawner { get; set; }
        public bool CanPassThrough { get; set; }
        public bool IsExitable { get; set; }

        public ItemType ItemType { get; }
        public LockStates LockStates { get; set; }
        public CandyColor CandyColor { get; }

        public Vector3 WorldPosition { get; set; }
        public Vector3Int GridPosition { get; set; }
        public IGridStateful GridStateful { get; }
        public IGridCellView GridCellView { get; }
        public IBlockItem BlockItem { get; }

        public void SetGridCellViewPosition();
        public void SetBlockItem(IBlockItem blockItem);
        public void SetGridStateful(IGridStateful stateful);
        public void SetGridCellView(IGridCellView gridView);
        public void ReleaseGrid();
    }
}
