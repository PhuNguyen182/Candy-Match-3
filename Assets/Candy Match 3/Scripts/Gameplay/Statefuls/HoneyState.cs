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
        private bool _canContainItem;

        public override int MaxHealthPoint => _maxHealthPoint;

        public override StatefulGroupType GroupType => StatefulGroupType.Honey;

        public override bool IsLocked => false;

        public override bool CanContainItem => _canContainItem;

        public override bool Break()
        {
            _healthPoint = _healthPoint - 1;

            if(_healthPoint > 0)
            {
                // Do logic thing here
                Release();
                return false;
            }

            return true;
        }

        public override void SetHealthPoint(int healthPoint)
        {
            _maxHealthPoint = healthPoint;
            _healthPoint = healthPoint;
            _canContainItem = false;
        }

        public override void Release()
        {
            // When clear state, emit a message to score the target
            _canContainItem = true;
        }
    }
}
