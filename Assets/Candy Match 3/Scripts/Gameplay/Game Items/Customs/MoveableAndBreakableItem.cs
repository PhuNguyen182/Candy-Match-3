using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Messages;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Customs
{
    public class MoveableAndBreakableItem : BaseItem, ISetHealthPoint, IAdjcentBreakable, IItemAnimation, IItemEffect
    {
        [SerializeField] private Sprite[] itemHealthStates;
        [SerializeField] private ItemAnimation itemAnimation;
        [SerializeField] private SoundEffectType breakSound;

        private int _healthPoint;
        private int _maxHealthPoint;

        private IPublisher<DecreaseTargetMessage> _decreaseTargetPublisher;

        public override bool IsMatchable => false;

        public override bool IsMoveable => true;

        public override bool Replacable => false;

        public int MaxHealthPoint => _maxHealthPoint;

        public override void ResetItem()
        {
            base.ResetItem();
            _healthPoint = _maxHealthPoint;
            SetItemSpriteViaHealthPoint();
            itemAnimation.ResetItem();
        }

        public override void InitMessages()
        {
            _decreaseTargetPublisher = GlobalMessagePipe.GetPublisher<DecreaseTargetMessage>();
        }

        public override async UniTask ItemBlast()
        {
            await UniTask.CompletedTask;
        }

        public override void ReleaseItem()
        {
            base.ReleaseItem();
            _decreaseTargetPublisher.Publish(new DecreaseTargetMessage
            {
                TargetType = targetType,
                Task = UniTask.CompletedTask,
                HasMoveTask = false
            });

            SimplePool.Despawn(this.gameObject);
        }

        public void SetHealthPoint(int healthPoint)
        {
            _maxHealthPoint = healthPoint;
        }

        public bool Break()
        {
            _healthPoint = _healthPoint - 1;

            if(_healthPoint > 0)
            {
                // Do logic and effect things here
                PlayBreakEffect();
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
            return itemAnimation.MoveTo(position, duration);
        }

        public void JumpDown(float amptitude)
        {
            itemAnimation.JumpDown(amptitude);
        }

        public void Nudge(Vector3Int direction)
        {
            itemAnimation.Nudge(direction);
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

        public void PlayBreakEffect()
        {
            EffectManager.Instance.PlaySoundEffect(breakSound, true);
            EffectManager.Instance.SpawnSpecialEffect(itemType, WorldPosition);
        }

        public void PlayReplaceEffect()
        {
            
        }

        public void PlayBoosterEffect(BoosterType boosterType)
        {
            
        }
    }
}
