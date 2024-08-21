using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Statefuls
{
    public class NotAvailableState : BaseStateful
    {
        public override int MaxHealthPoint { get; }

        public override StatefulLayer StatefulLayer => StatefulLayer.None;

        public override StatefulGroupType GroupType => StatefulGroupType.NotAvailable;

        public override bool IsLocked => true;

        public override bool CanContainItem => false;

        public override bool IsAvailable => false;

        public override void Release()
        {
            
        }

        public override void SetHealthPoint(int healthPoint)
        {
            
        }
    }
}
