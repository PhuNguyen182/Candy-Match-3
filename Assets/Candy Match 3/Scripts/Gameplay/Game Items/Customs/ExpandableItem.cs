using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Common.Constants;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Customs
{
    public class ExpandableItem : BaseItem, IAdjcentBreakable, IExpandableItem, IItemEffect
    {
        [SerializeField] private ItemAnimation itemAnimation;
        [SerializeField] private SoundEffectType breakSound = SoundEffectType.Chocolate;
        [SerializeField] private SoundEffectType expandSound = SoundEffectType.ChocolateExpand;

        private IPublisher<ExpandMessage> _expandPublisher;
        private IPublisher<BreakExpandableMessage> _breakExpandablePublisher;
        private IPublisher<DecreaseTargetMessage> _decreaseTargetPublisher;

        public override bool Replacable => false;

        public override bool IsMatchable => false;

        public override bool IsMoveable => false;

        public bool Break()
        {
            _expandPublisher.Publish(new ExpandMessage
            {
                CanExpand = false
            });

            _breakExpandablePublisher.Publish(new BreakExpandableMessage
            {
                Position = GridPosition
            });

            return true;
        }

        public void Expand(Vector3Int position)
        {
            itemAnimation.ItemAnimator.SetTrigger(ItemAnimationHashKeys.StartHash);
            EffectManager.Instance.PlaySoundEffect(expandSound);
        }

        public override void InitMessages()
        {
            _expandPublisher = GlobalMessagePipe.GetPublisher<ExpandMessage>();
            _breakExpandablePublisher = GlobalMessagePipe.GetPublisher<BreakExpandableMessage>();
            _decreaseTargetPublisher = GlobalMessagePipe.GetPublisher<DecreaseTargetMessage>();
        }

        public override async UniTask ItemBlast()
        {
            await UniTask.CompletedTask;
        }

        public void PlayBoosterEffect(BoosterType boosterType)
        {
            
        }

        public void PlayBreakEffect(int healthPoint)
        {
            EffectManager.Instance.PlaySoundEffect(breakSound);
            EffectManager.Instance.SpawnSpecialEffect(ItemType.Chocolate, WorldPosition);
        }

        public void PlayMatchEffect()
        {
            
        }

        public void PlayReplaceEffect()
        {
            
        }

        public void PlayStartEffect()
        {
            
        }

        public override void ReleaseItem()
        {
            _decreaseTargetPublisher.Publish(new DecreaseTargetMessage
            {
                TargetType = targetType,
                Task = UniTask.CompletedTask,
                HasMoveTask = false
            });

            PlayBreakEffect(0);
            itemAnimation.ItemAnimator.SetTrigger(ItemAnimationHashKeys.KillHash);
            SimplePool.Despawn(this.gameObject);
        }
    }
}
