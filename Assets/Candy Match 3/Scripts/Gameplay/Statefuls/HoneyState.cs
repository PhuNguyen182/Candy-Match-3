using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Statefuls
{
    public class HoneyState : BaseStateful
    {
        private int _healthPoint;
        private int _maxHealthPoint;
        private bool _canContainItem;

        private Sprite _state;

        public override int MaxHealthPoint => _maxHealthPoint;

        public override StatefulGroupType GroupType => StatefulGroupType.Honey;

        public override StatefulLayer StatefulLayer => StatefulLayer.Bottom;

        public override bool IsLocked => false;

        public override bool CanContainItem => _canContainItem;

        public override bool IsAvailable => true;

        public HoneyState(Sprite state)
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
            _maxHealthPoint = healthPoint;
            _healthPoint = healthPoint;
            _canContainItem = false;

            GridCellView.UpdateStateView(_state, StatefulLayer);
        }

        public override void Release()
        {
            // When clear state, emit a message to score the target
            _canContainItem = true;
            GridCellView.UpdateStateView(_state, StatefulLayer);
        }
    }
}
