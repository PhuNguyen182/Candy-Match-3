using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Statefuls
{
    public class SyrupState : BaseStateful
    {
        private int _healthPoint;
        private int _maxHealthPoint;

        public override int MaxHealthPoint => _maxHealthPoint;

        public override StatefulGroupType GroupType => StatefulGroupType.Syrup;

        public override bool Break()
        {
            _healthPoint = _healthPoint - 1;

            if (_healthPoint > 0)
            {
                // Do logic thing here
                return false;
            }

            return true;
        }

        public override void SetHealthPoint(int healthPoint)
        {
            _maxHealthPoint = healthPoint;
            _healthPoint = healthPoint;
        }

        public override void Release()
        {
            
        }
    }
}
