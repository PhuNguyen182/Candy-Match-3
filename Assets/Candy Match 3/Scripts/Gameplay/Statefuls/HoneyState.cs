using System.Collections;
using System.Collections.Generic;
using CandyMatch3.Scripts.Common.Enums;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.Statefuls
{
    public class HoneyState : BaseStateful
    {
        private int _healthPoint;
        private int _maxHealthPoint;

        public override int MaxHealthPoint => _maxHealthPoint;

        public override StatefulGroupType GroupType => StatefulGroupType.Honey;

        public override bool Break()
        {
            _healthPoint = _healthPoint - 1;

            if(_healthPoint > 0)
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
            // When clear state, emit a message to score the target
        }
    }
}
