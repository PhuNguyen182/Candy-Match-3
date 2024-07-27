using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IGridCellView
    {
        public Vector3Int GridPosition { get; }

        public void PlayGlowGroundCell();
        public void SetGridPosition(Vector3Int position);
    }
}
