using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IGridCellView
    {
        public Vector3 WorldPosition { get; }
        public Vector3Int GridPosition { get; }

        public void PlayGlowGroundCell();
        public void SetGridPosition(Vector3Int position);
        public void SetWorldPosition(Vector3 position);
        public void UpdateStateView(Sprite state, StatefulLayer layer);
    }
}
