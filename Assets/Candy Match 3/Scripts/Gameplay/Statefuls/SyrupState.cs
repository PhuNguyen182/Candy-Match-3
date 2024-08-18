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
        private bool _canContainItem;

        public override int MaxHealthPoint => _maxHealthPoint;

        public override StatefulGroupType GroupType => StatefulGroupType.Syrup;

        public override bool IsLocked => false;

        public override bool CanContainItem => _canContainItem;

        public override bool IsAvailable => true;

        public override bool Break()
        {
            _healthPoint = _healthPoint - 1;

            if (_healthPoint > 0)
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
            _canContainItem = true;
        }
    }
}
