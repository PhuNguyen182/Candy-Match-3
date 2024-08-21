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

        private Sprite _state;

        public override int MaxHealthPoint => _maxHealthPoint;

        public override StatefulLayer StatefulLayer => StatefulLayer.Top;

        public override StatefulGroupType GroupType => StatefulGroupType.Ice;

        public override bool IsLocked => _isLocked;

        public override bool CanContainItem => _canContainItem;

        public override bool IsAvailable => true;

        public IceState(Sprite state)
        {
            _state = state;
        }

        public override bool Break()
        {
            Release();
            return true;
        }

        public override void SetHealthPoint(int healthPoint)
        {
            _isLocked = true;
            _canContainItem = false;
            
            _maxHealthPoint = healthPoint;
            _healthPoint = healthPoint;
            GridCellView.UpdateStateView(_state, StatefulLayer);
        }

        public override void Release()
        {
            _isLocked = false;
            _canContainItem = true;
            GridCellView.UpdateStateView(null, StatefulLayer);
        }
    }
}
