using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Statefuls
{
    public class IceState : BaseStateful
    {
        private int _healthPoint;
        private int _maxHealthPoint;
        private bool _isLocked;
        private bool _canContainItem;

        public override int MaxHealthPoint => _maxHealthPoint;

        public override StatefulGroupType GroupType => StatefulGroupType.Ice;

        public override bool IsLocked => _isLocked;

        public override bool CanContainItem => _canContainItem;

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
            _isLocked = true;
            _canContainItem = false;
            
            _maxHealthPoint = healthPoint;
            _healthPoint = healthPoint;
        }

        public override void Release()
        {
            _isLocked = false;
            _canContainItem = true;
        }
    }
}
