using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Common.Messages;
using Cysharp.Threading.Tasks;
using GlobalScripts.Utils;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Customs
{
    public class CollectibleItem : BaseItem, IItemAnimation, ICollectible
    {
        [SerializeField] private ItemAnimation itemAnimation;
        [SerializeField] private SoundEffectType collectSound;

        private IPublisher<DecreaseTargetMessage> _decreaseTargetPublisher;
        private IPublisher<AsyncMessage<MoveTargetData>> _moveToTargetPublisher;

        public override bool CanBeReplace => false;

        public override bool IsMatchable => false;

        public override bool IsMoveable => true;

        public UniTask BounceInDirection(Vector3 direction)
        {
            return itemAnimation.BounceMove(direction);
        }

        public void BounceOnTap()
        {
            itemAnimation.BounceTap();
        }

        public UniTask Collect()
        {
            EffectManager.Instance.PlaySoundEffect(collectSound);
            EffectManager.Instance.SpawnSpecialEffect(itemType, WorldPosition);

            return UniTask.Delay(TimeSpan.FromSeconds(Match3Constants.ItemMatchDelay)
                                 , false, PlayerLoopTiming.FixedUpdate, destroyToken);
        }

        public override void InitMessages()
        {
            _decreaseTargetPublisher = GlobalMessagePipe.GetPublisher<DecreaseTargetMessage>();
            _moveToTargetPublisher = GlobalMessagePipe.GetPublisher<AsyncMessage<MoveTargetData>>();
        }

        public override UniTask ItemBlast()
        {
            return UniTask.CompletedTask;
        }

        public void JumpDown(float amptitude)
        {
            itemAnimation.JumpDown(amptitude);
        }

        public UniTask MoveTo(Vector3 position, float duration)
        {
            return itemAnimation.MoveTo(position, duration);
        }

        public override void ReleaseItem()
        {
            MoveToTargetAndRelease().Forget();
            SimplePool.Despawn(this.gameObject);
        }

        public UniTask SwapTo(Vector3 position, float duration, bool isMoveFirst)
        {
            return itemAnimation.SwapTo(position, duration, isMoveFirst);
        }

        private async UniTask MoveToTargetAndRelease()
        {
            MoveTargetData data = new MoveTargetData { TargetType = targetType };
            MoveTargetData target = await MessageBrokerUtils<MoveTargetData>
                                          .PublishAsyncMessage(_moveToTargetPublisher, data);
            if (!target.IsCompleted)
            {
                var flyObject = EffectManager.Instance.SpawnFlyCompletedTarget(targetType, transform.position);
                flyObject.transform.localScale = Vector3.one;

                float distance = Vector3.Distance(target.Destination, transform.position);
                float speed = Mathf.Lerp(Match3Constants.NearSpeed, Match3Constants.FarSpeed, distance / Match3Constants.MaxDistance);
                float duration = distance / speed;

                UniTask moveTask = flyObject.MoveToTarget(target.Destination, duration);
                _decreaseTargetPublisher.Publish(new DecreaseTargetMessage
                {
                    Task = moveTask,
                    TargetType = targetType,
                    HasMoveTask = true
                });
            }
        }
    }
}
