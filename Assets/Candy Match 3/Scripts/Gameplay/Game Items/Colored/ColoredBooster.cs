using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Messages;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Colored
{
    public class ColoredBooster : BaseItem, ISetColor, IColorBooster, IItemAnimation, IItemEffect, IColorfulEffect, IItemSuggest
    {
        [SerializeField] private BoosterType colorBoosterType;
        [SerializeField] private ItemAnimation itemAnimation;

        [Header("Effects")]
        [SerializeField] private GameObject colorfulEffect;

        [Header("Colored Sprites")]
        [SerializeField] private Sprite[] wrappedSprites;
        [SerializeField] private Sprite[] horizontalSprites;
        [SerializeField] private Sprite[] verticalSprites;

        private bool _isMatchable;
        private Sprite _horizontalIcon;
        private Sprite _verticalIcon;

        private IPublisher<DecreaseTargetMessage> _decreaseTargetPublisher;

        public override bool IsMatchable => _isMatchable;

        public override bool IsMoveable => true;

        public override bool Replacable => false;

        public bool IsActivated { get; set; }

        public bool IsNewCreated { get; set; }

        public BoosterType ColorBoosterType => colorBoosterType;

        public override void ResetItem()
        {
            base.ResetItem();
            SetMatchable(true);
            itemAnimation.ResetItem();
            colorfulEffect.gameObject.SetActive(false);
            OnItemReset().Forget();
        }

        public override void SetMatchable(bool isMatchable)
        {
            _isMatchable = isMatchable;
        }

        public override void InitMessages()
        {
            _decreaseTargetPublisher = GlobalMessagePipe.GetPublisher<DecreaseTargetMessage>();
        }

        public override async UniTask ItemBlast()
        {
            await UniTask.CompletedTask;
        }

        public void SetColor(CandyColor candyColor)
        {
            this.candyColor = candyColor;
        }

        public void SetBoosterType(BoosterType colorBoosterType)
        {
            this.colorBoosterType = colorBoosterType;

            int colorIndex = candyColor switch
            {
                CandyColor.Blue => 0,
                CandyColor.Green => 1,
                CandyColor.Orange => 2,
                CandyColor.Purple => 3,
                CandyColor.Red => 4,
                CandyColor.Yellow => 5,
                _ => -1
            };

            Sprite colorSprite = colorBoosterType switch
            {
                BoosterType.Horizontal => horizontalSprites[colorIndex],
                BoosterType.Vertical => verticalSprites[colorIndex],
                BoosterType.Wrapped => wrappedSprites[colorIndex],
                _ => null
            };

            _horizontalIcon = horizontalSprites[colorIndex];
            _verticalIcon = verticalSprites[colorIndex];

            itemGraphics.SetItemSprite(colorSprite);
            SetTargetType();
        }

        public void ChangeItemSprite(int step)
        {
            if(step == 1)
                itemGraphics.SetItemSprite(_horizontalIcon);
            else if(step == 2)
                itemGraphics.SetItemSprite(_verticalIcon);
        }

        public UniTask PlayBoosterCombo(int direction, ComboBoosterType comboType, bool isFirstItem)
        {
            UniTask boosterTask = UniTask.CompletedTask;

            if (comboType == ComboBoosterType.StripedWrapped)
                boosterTask = itemAnimation.PlayStripedWrapped();
            
            else if(comboType == ComboBoosterType.DoubleWrapped)
                boosterTask = itemAnimation.PlayDoubleWrapped(direction, isFirstItem);
            
            return boosterTask;
        }

        public async UniTask Activate()
        {
            Explode();
            await UniTask.CompletedTask;
        }

        public void Explode()
        {
            float x = WorldPosition.x;
            float y = WorldPosition.y;

            Vector3 position = colorBoosterType switch
            {
                BoosterType.Horizontal => new(0, y),
                BoosterType.Vertical => new(x, 0),
                BoosterType.Wrapped => new(x, y),
                _ => Vector3.zero
            };

            SoundEffectType soundEffect = colorBoosterType == BoosterType.Wrapped ? SoundEffectType.CandyWrap
                                                                                  : SoundEffectType.LineVerticalHorizontal;
            EffectManager.Instance.SpawnBoosterEffect(itemType, colorBoosterType, position);
            EffectManager.Instance.PlaySoundEffect(soundEffect);
        }

        public bool Break()
        {
            return !IsNewCreated;
        }

        public override void ReleaseItem()
        {
            _decreaseTargetPublisher.Publish(new DecreaseTargetMessage
            {
                TargetType = targetType,
                Task = UniTask.CompletedTask,
                HasMoveTask = false
            });

            itemAnimation.ToggleSuggest(false);
            colorfulEffect.gameObject.SetActive(false);
            SimplePool.Despawn(this.gameObject);
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

        public UniTask SwapTo(Vector3 position, float duration, bool isMoveFirst)
        {
            return itemAnimation.SwapTo(position, duration, isMoveFirst);
        }

        private void SetTargetType()
        {
            targetType = itemType switch
            {
                ItemType.BlueHorizontal => TargetEnum.BlueHorizontal,
                ItemType.BlueVertical => TargetEnum.BlueVertical,
                ItemType.BlueWrapped => TargetEnum.BlueWrapped,

                ItemType.GreenHorizontal => TargetEnum.GreenHorizontal,
                ItemType.GreenVertical => TargetEnum.GreenVertical,
                ItemType.GreenWrapped => TargetEnum.GreenWrapped,

                ItemType.OrangeHorizontal => TargetEnum.OrangeHorizontal,
                ItemType.OrangeVertical => TargetEnum.OrangeVertical,
                ItemType.OrangeWrapped => TargetEnum.OrangeWrapped,

                ItemType.PurpleHorizontal => TargetEnum.PurpleHorizontal,
                ItemType.PurpleVertical => TargetEnum.PurpleVertical,
                ItemType.PurpleWrapped => TargetEnum.PurpleWrapped,

                ItemType.RedHorizontal => TargetEnum.RedHorizontal,
                ItemType.RedVertical => TargetEnum.RedVertical,
                ItemType.RedWrapped => TargetEnum.RedWrapped,

                ItemType.YellowHorizontal => TargetEnum.YellowHorizontal,
                ItemType.YellowVertical => TargetEnum.YellowVertical,
                ItemType.YellowWrapped => TargetEnum.YellowWrapped,

                _ => TargetEnum.None
            };
        }

        public void PlayColorfulEffect()
        {
            itemAnimation.TriggerVibrate();
            colorfulEffect.gameObject.SetActive(true);
        }

        public void PlayBoosterEffect(BoosterType boosterType) { }

        public void PlayStartEffect()
        {
            EffectManager.Instance.SpawnNewCreatedEffect(WorldPosition);
            EffectManager.Instance.PlaySoundEffect(SoundEffectType.BoosterAppear);
        }

        public void PlayMatchEffect()
        {
            EffectManager.Instance.SpawnColorEffect(candyColor, WorldPosition);
        }

        public void PlayBreakEffect()
        {
            EffectManager.Instance.SpawnColorEffect(candyColor, WorldPosition);
        }

        public void PlayReplaceEffect()
        {
            EffectManager.Instance.SpawnNewCreatedEffect(WorldPosition);
            EffectManager.Instance.PlaySoundEffect(SoundEffectType.BoosterAppear);
        }

        private async UniTask OnItemReset()
        {
            IsNewCreated = true;
            IsActivated = false;

            TimeSpan delay = TimeSpan.FromSeconds(Match3Constants.ItemMatchDelay);
            await UniTask.Delay(delay, false, PlayerLoopTiming.FixedUpdate, destroyToken);
            IsNewCreated = false;
        }

        public void Highlight(bool isActive)
        {
            itemAnimation.ToggleSuggest(isActive);
        }

        public void TriggerNextStage(int stage = 0)
        {
            itemAnimation.TriggerVibrate(stage);
        }
    }
}
