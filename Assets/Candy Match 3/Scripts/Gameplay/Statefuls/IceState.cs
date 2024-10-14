using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.Statefuls
{
    public class IceState : BaseStateful, IBreakable
    {
        private int _healthPoint;
        private int _maxHealthPoint;
        
        private bool _isLocked;
        private bool _isAvailable;
        private bool _canContainItem;

        private Sprite _state;

        private IPublisher<DecreaseTargetMessage> _decreaseTargetPublisher;

        public override int MaxHealthPoint => _maxHealthPoint;

        public override StatefulLayer StatefulLayer => StatefulLayer.Top;

        public override StatefulGroupType GroupType => StatefulGroupType.Ice;

        public override TargetEnum TargetType => TargetEnum.Ice;

        public override bool IsLocked => _isLocked;

        public override bool CanContainItem => _canContainItem;

        public override bool IsAvailable => _isAvailable;

        public IceState(Sprite state)
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
            _isLocked = true;
            _canContainItem = false;
            _isAvailable = false;
            
            _maxHealthPoint = healthPoint;
            _healthPoint = healthPoint;
        }

        public override void Release()
        {
            _isLocked = false;
            _canContainItem = true;
            _isAvailable = true;

            GridCellView.UpdateStateView(null, StatefulLayer);
            Vector3 position = GridCellView.WorldPosition;

            EffectManager.Instance.PlaySoundEffect(SoundEffectType.Ice, true);
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
