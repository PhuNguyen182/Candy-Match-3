using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Customs
{
    public class UnmoveableAndBreakableItem : BaseItem, ISetHealthPoint, IAdjcentBreakable
    {
        private int _healthPoint;
        private int _maxHealthPoint;

        public override bool IsMatchable => false;

        public override bool CanMove => false;

        public int MaxHealthPoint => _maxHealthPoint;

        public override void ResetItem()
        {
            base.ResetItem();
            _healthPoint = _maxHealthPoint;
        }

        public override void InitMessages()
        {

        }

        public override async UniTask ItemBlast()
        {
            await UniTask.CompletedTask;
        }

        public override void ReleaseItem()
        {
            SimplePool.Despawn(this.gameObject);
        }

        public void SetHealthPoint(int healthPoint)
        {
            _maxHealthPoint = healthPoint;
        }

        public bool Break()
        {
            _healthPoint = _healthPoint - 1;

            if (_healthPoint > 0)
            {
                // Do logic thing here
                return false;
            }

            return true;
        }
    }
}
