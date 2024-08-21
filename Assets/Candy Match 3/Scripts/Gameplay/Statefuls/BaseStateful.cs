using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Statefuls
{
    public abstract class BaseStateful : IGridStateful, ISetHealthPoint, IBreakable
    {
        public IGridCellView GridCellView { get; set; }

        public abstract int MaxHealthPoint { get; }

        public abstract StatefulLayer StatefulLayer { get; }

        public abstract StatefulGroupType GroupType { get; }

        public abstract bool IsLocked { get; }

        public abstract bool CanContainItem { get; }

        public abstract bool IsAvailable { get; }

        public abstract void SetHealthPoint(int healthPoint);

        public abstract bool Break();

        public abstract void Release();
    }
}
