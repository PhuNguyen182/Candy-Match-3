using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Common.Constants;
using Cysharp.Threading.Tasks;
using GlobalScripts.Utils;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Colored
{
    public class ColoredItem : BaseItem, ISetColor, IItemAnimation, IBreakable
        , IItemEffect, IColorfulEffect, IItemSuggest, IItemTransform, IMatchAnimation
    {
        [SerializeField] private ItemAnimation itemAnimation;

        [Header("Colored Sprites")]
        [SerializeField] private Sprite[] candyColors;

        [Header("Effects")]
        [SerializeField] private GameObject colorfulEffect;

        private bool _isMatchable;
        private IPublisher<DecreaseTargetMessage> _decreaseTargetPublisher;
        private IPublisher<AsyncMessage<MoveTargetData>> _moveToTargetPublisher;

        public override bool Replacable => true;

        public override bool IsMatchable => _isMatchable;

        public override bool IsMoveable => !IsLocking;

        public override void ResetItem()
        {
            base.ResetItem();
            IsLocking = false;
            SetMatchable(true);
            itemAnimation.ResetItem();
            colorfulEffect.gameObject.SetActive(false);
            itemAnimation.DisappearOnMatch(false).Forget();
        }

        public override void SetMatchable(bool isMatchable)
        {
            _isMatchable = isMatchable;
        }

        public override void InitMessages()
        {
            _decreaseTargetPublisher = GlobalMessagePipe.GetPublisher<DecreaseTargetMessage>();
            _moveToTargetPublisher = GlobalMessagePipe.GetPublisher<AsyncMessage<MoveTargetData>>();
        }

        public override async UniTask ItemBlast()
        {
            await itemAnimation.DisappearOnMatch(true);
        }

        public override void ReleaseItem()
        {
            base.ReleaseItem();
            colorfulEffect.gameObject.SetActive(false);
            itemAnimation.ToggleSuggest(false);
            MoveToTargetAndRelease().Forget();
            SimplePool.Despawn(this.gameObject);
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

        public void SetColor(CandyColor candyColor)
        {
            this.candyColor = candyColor;
            Sprite colorSprite = GetSprite(candyColor);

            targetType = candyColor switch
            {
                CandyColor.Blue => TargetEnum.Blue,
                CandyColor.Green => TargetEnum.Green,
                CandyColor.Orange => TargetEnum.Orange,
                CandyColor.Purple => TargetEnum.Purple,
                CandyColor.Red => TargetEnum.Red,
                CandyColor.Yellow => TargetEnum.Yellow,
                _ => TargetEnum.None
            };

            itemGraphics.SetItemSprite(colorSprite);
        }

        public bool Break()
        {
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

        public UniTask MatchTo(Vector3 position, float duration)
        {
            return itemAnimation.MatchTo(position, duration);
        }

        public UniTask MoveTo(Vector3 position, float duration)
        {
            return itemAnimation.MoveTo(position, duration);
        }

        public void JumpDown(float amptitude)
        {
            itemAnimation.JumpDown(amptitude);
        }

        public UniTask SwapTo(Vector3 position, float duration, bool isMoveFirst)
        {
            return itemAnimation.SwapTo(position, duration, isMoveFirst);
        }

        public void PlayStartEffect()
        {
            
        }

        public void PlayMatchEffect()
        {
            EffectManager.Instance.SpawnColorEffect(candyColor, WorldPosition);
        }

        public void PlayBreakEffect()
        {
            EffectManager.Instance.SpawnBlastEffect(WorldPosition);
            EffectManager.Instance.SpawnColorEffect(candyColor, WorldPosition);
        }

        public void PlayReplaceEffect()
        {
            EffectManager.Instance.SpawnNewCreatedEffect(WorldPosition);
        }

        public void PlayColorfulEffect()
        {
            itemAnimation.TriggerVibrate();
            colorfulEffect.gameObject.SetActive(true);
        }

        public void Highlight(bool isActive)
        {
            itemAnimation.ToggleSuggest(isActive);
        }

        public void PlayBoosterEffect(BoosterType boosterType)
        {
            
        }

        public void SwitchTo(ItemType itemType)
        {
            this.itemType = itemType;
            candyColor = GetColor(itemType);
        }

        public async UniTask Transform(float delay = 0)
        {
            Sprite alternativeSprite = GetSprite(candyColor);
            itemGraphics.SetAlternateSprite(alternativeSprite);
            itemAnimation.ChangeVisibleMask(true);

            if(delay > 0)
            {
                TimeSpan delayAmount = TimeSpan.FromSeconds(delay);
                await UniTask.Delay(delayAmount, cancellationToken: destroyToken);
            }

            itemAnimation.Transform();
            TimeSpan changeSpriteDelay = TimeSpan.FromSeconds(0.167);
            await UniTask.Delay(changeSpriteDelay, cancellationToken: destroyToken);
            SetColor(candyColor);

            TimeSpan itemTransformDelay = TimeSpan.FromSeconds(1.167);
            await UniTask.Delay(itemTransformDelay, cancellationToken: destroyToken);
            itemAnimation.ChangeVisibleMask(false);
        }

        public void TransformImmediately()
        {
            SetColor(candyColor);
        }

        private Sprite GetSprite(CandyColor candyColor)
        {
            return candyColor switch
            {
                CandyColor.Blue => candyColors[0],
                CandyColor.Green => candyColors[1],
                CandyColor.Orange => candyColors[2],
                CandyColor.Purple => candyColors[3],
                CandyColor.Red => candyColors[4],
                CandyColor.Yellow => candyColors[5],
                _ => null
            };
        }

        private CandyColor GetColor(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.Blue => CandyColor.Blue,
                ItemType.Green => CandyColor.Green,
                ItemType.Orange => CandyColor.Orange,
                ItemType.Purple => CandyColor.Purple,
                ItemType.Red => CandyColor.Red,
                ItemType.Yellow => CandyColor.Yellow,
                _ => CandyColor.None
            };
        }
    }
}
