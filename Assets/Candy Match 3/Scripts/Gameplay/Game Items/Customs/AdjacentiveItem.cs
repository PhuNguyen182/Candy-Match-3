using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Effects;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Customs
{
    public class AdjacentiveItem : BaseItem, IAdjcentBreakable, IAdjacentItem, IItemEffect
    {
        [SerializeField] private SoundEffectType breakSound = SoundEffectType.Chocolate;
        [SerializeField] private SoundEffectType expandSound = SoundEffectType.ChocolateExpand;

        private IPublisher<DecreaseTargetMessage> _decreaseTargetPublisher;

        public override bool CanBeReplace => false;

        public override bool IsMatchable => false;

        public override bool IsMoveable => false;

        public bool Break()
        {
            // Do logic and effect here
            return true;
        }

        public void Expand(Vector3Int position)
        {
            EffectManager.Instance.PlaySoundEffect(expandSound);
        }

        public override void InitMessages()
        {
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
            SimplePool.Despawn(this.gameObject);
        }
    }
}
