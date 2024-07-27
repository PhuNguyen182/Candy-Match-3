using CandyMatch3.Scripts.Common.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.Statefuls
{
    public class AvailableState : BaseStateful
    {
        public override int MaxHealthPoint { get; }

        public override StatefulGroupType GroupType => StatefulGroupType.Available;

        public override bool Break()
        {
            return false;
        }

        public override void Release()
        {
            
        }

        public override void SetHealthPoint(int healthPoint)
        {
            
        }
    }
}
