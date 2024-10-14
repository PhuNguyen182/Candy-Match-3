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
    public class SyrupState : BaseStateful, IBreakable
    {
        private int _healthPoint;
        private int _maxHealthPoint;
        private bool _isAvailable;

        private Sprite[] _states;

        private IPublisher<DecreaseTargetMessage> _decreaseTargetPublisher;

        public override int MaxHealthPoint => _maxHealthPoint;

        public override StatefulLayer StatefulLayer => StatefulLayer.Bottom;

        public override StatefulGroupType GroupType => StatefulGroupType.Syrup;

        public override TargetEnum TargetType => TargetEnum.Syrup;

        public override bool IsLocked => false;

        public override bool CanContainItem => true;

        public override bool IsAvailable => _isAvailable;

        public SyrupState(Sprite[] states)
        {
            _states = states;
        }

        public bool Break()
        {
            _healthPoint = _healthPoint - 1;

            if (_healthPoint > 0)
            {
                Vector3 position = GridCellView.WorldPosition;
                EffectManager.Instance.SpawnStatefulEffect(GroupType, position);
                EffectManager.Instance.PlaySoundEffect(SoundEffectType.Syrup, true);
                GridCellView.UpdateStateView(_states[_healthPoint - 1], StatefulLayer);
                return false;
            }

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
            _isAvailable = true;

            GridCellView.UpdateStateView(null, StatefulLayer);
            Vector3 position = GridCellView.WorldPosition;

            EffectManager.Instance.SpawnStatefulEffect(GroupType, position);
            EffectManager.Instance.PlaySoundEffect(SoundEffectType.Syrup);

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
            GridCellView.UpdateStateView(_states[_healthPoint - 1], StatefulLayer);
        }

        public override void InitMessages()
        {
            _decreaseTargetPublisher = GlobalMessagePipe.GetPublisher<DecreaseTargetMessage>();
        }
    }
}
