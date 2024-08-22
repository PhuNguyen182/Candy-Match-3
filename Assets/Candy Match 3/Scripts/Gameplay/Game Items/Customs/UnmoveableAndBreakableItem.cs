using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;
using CandyMatch3.Scripts.Gameplay.Effects;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Customs
{
    public class UnmoveableAndBreakableItem : BaseItem, ISetHealthPoint, IAdjcentBreakable, IItemAnimation, IItemEffect
    {
        [SerializeField] private Sprite[] itemHealthStates;
        [SerializeField] private ItemAnimation itemAnimation;

        private int _healthPoint;
        private int _maxHealthPoint;

        public override bool IsMatchable => false;

        public override bool IsMoveable => false;

        public int MaxHealthPoint => _maxHealthPoint;

        public override bool CanBeReplace => false;

        public override void ResetItem()
        {
            base.ResetItem();
            _healthPoint = _maxHealthPoint;
            SetItemSpriteViaHealthPoint();
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
                // Do logic and effect things here
                SetItemSpriteViaHealthPoint();
                return false;
            }

            return true;
        }

        public void BounceOnTap()
        {
            itemAnimation.BounceTap();
        }

        public UniTask BounceInDirection(Vector3 direction)
        {
            return itemAnimation.BounceMove(direction);
        }

        public UniTask MoveTo(Vector3 position, float duration)
        {
            return UniTask.CompletedTask;
        }

        public void JumpDown(float amptitude)
        {
            
        }

        public UniTask SwapTo(Vector3 position, float duration, bool isMoveFirst)
        {
            return itemAnimation.SwapTo(position, duration, isMoveFirst);
        }

        private void SetItemSpriteViaHealthPoint()
        {
            Sprite sprite = itemHealthStates[_healthPoint - 1];
            itemGraphics.SetItemSprite(sprite);
        }

        public void PlayStartEffect()
        {
            
        }

        public void PlayMatchEffect()
        {
            
        }

        public void PlayBreakEffect(int healthPoint)
        {
            EffectManager.Instance.SpawnSpecialEffect(itemType, WorldPosition);
        }

        public void PlayReplaceEffect()
        {
            
        }
    }
}
