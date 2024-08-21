using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Statefuls
{
    public class AvailableState : BaseStateful
    {
        public override int MaxHealthPoint { get; }

        public override StatefulGroupType GroupType => StatefulGroupType.Available;

        public override StatefulLayer StatefulLayer => StatefulLayer.Middle;

        public override bool IsLocked => false;

        public override bool CanContainItem => true;

        public override bool IsAvailable => true;

        public override void Release()
        {
            
        }

        public override void SetHealthPoint(int healthPoint)
        {
            
        }
    }
}
