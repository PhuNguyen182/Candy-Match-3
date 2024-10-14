using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Messages;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.Statefuls
{
    public class HoneyState : BaseStateful, IBreakable
    {
        private int _healthPoint;
        private int _maxHealthPoint;

        private bool _isAvailable;

        private Sprite _state;

        private IPublisher<DecreaseTargetMessage> _decreaseTargetPublisher;

        public override int MaxHealthPoint => _maxHealthPoint;

        public override StatefulGroupType GroupType => StatefulGroupType.Honey;

        public override StatefulLayer StatefulLayer => StatefulLayer.Bottom;

        public override TargetEnum TargetType => TargetEnum.Honey;

        public override bool IsLocked => false;

        public override bool CanContainItem => true;

        public override bool IsAvailable => _isAvailable;

        public HoneyState(Sprite state)
        {
            _state = state;
        }

        public bool Break()
        {
            Release();
            return true;
        }

        public override void SetHealthPoint(int healthPoint)
        {
            _isAvailable = false;
            _maxHealthPoint = healthPoint;
            _healthPoint = healthPoint;
        }

        public override void Release()
        {
            // When clear state, emit a message to score the target
            _isAvailable = true;

            GridCellView.UpdateStateView(null, StatefulLayer);
            Vector3 position = GridCellView.WorldPosition;

            EffectManager.Instance.PlaySoundEffect(SoundEffectType.Honey, true);
            EffectManager.Instance.SpawnStatefulEffect(GroupType, position);

            _decreaseTargetPublisher.Publish(new DecreaseTargetMessage
            {
                TargetType = TargetType,
                Task = UniTask.CompletedTask,
                HasMoveTask = false
            });
        }

        public override void ResetState()
        {
            InitMessages();
            GridCellView.UpdateStateView(_state, StatefulLayer);
        }

        public override void InitMessages()
        {
            _decreaseTargetPublisher = GlobalMessagePipe.GetPublisher<DecreaseTargetMessage>();
        }
    }
}
