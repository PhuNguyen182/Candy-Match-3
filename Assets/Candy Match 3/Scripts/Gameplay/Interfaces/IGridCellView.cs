using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IGridCellView
    {
        public bool IsLocked { get; set; }
        public Vector3 WorldPosition { get; }
        public Vector3Int GridPosition { get; }
        public LockStates LockStates { get; set; }

        public void PlayGlowGroundCell();
        public void SetGridPosition(Vector3Int position);
        public void SetWorldPosition(Vector3 position);
        public void UpdateStateView(Sprite state, StatefulLayer layer);
    }
}
