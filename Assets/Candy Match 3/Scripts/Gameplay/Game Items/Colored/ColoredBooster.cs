using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Gameplay.Effects;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Colored
{
    public class ColoredBooster : BaseItem, ISetColor, ISetColorBooster, IBooster, IItemAnimation, IItemEffect
    {
        [SerializeField] private ColorBoosterType colorBoosterType;
        [SerializeField] private ItemAnimation itemAnimation;

        [Header("Colored Sprites")]
        [SerializeField] private Sprite[] wrappedSprites;
        [SerializeField] private Sprite[] horizontalSprites;
        [SerializeField] private Sprite[] verticalSprites;

        private bool _isMatchable;

        public override bool IsMatchable => _isMatchable;

        public override bool IsMoveable => true;

        public override bool CanBeReplace => false;

        public bool IsIgnored { get; set; }

        public ColorBoosterType ColorBoosterType => colorBoosterType;

        public override void ResetItem()
        {
            base.ResetItem();
            SetMatchable(true);
            IsIgnored = false;
        }

        public override void SetMatchable(bool isMatchable)
        {
            _isMatchable = isMatchable;
        }

        public override void InitMessages()
        {
            
        }

        public override async UniTask ItemBlast()
        {
            await UniTask.CompletedTask;
        }

        public void SetColor(CandyColor candyColor)
        {
            this.candyColor = candyColor;
        }

        public void SetBoosterColor(ColorBoosterType colorBoosterType)
        {
            this.colorBoosterType = colorBoosterType;
            Sprite[] colorSprites = colorBoosterType switch
            {
                ColorBoosterType.Horizontal => horizontalSprites,
                ColorBoosterType.Vertical => verticalSprites,
                ColorBoosterType.Wrapped => wrappedSprites,
                _ => null
            };

            Sprite colorSprite = candyColor switch
            {
                CandyColor.Blue => colorSprites[0],
                CandyColor.Green => colorSprites[1],
                CandyColor.Orange => colorSprites[2],
                CandyColor.Purple => colorSprites[3],
                CandyColor.Red => colorSprites[4],
                CandyColor.Yellow => colorSprites[5],
                _ => null
            };

            itemGraphics.SetItemSprite(colorSprite);
            SetTargetType();
        }

        public async UniTask Activate()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(Match3Constants.ItemReleaseDelay), cancellationToken: destroyCancellationToken);
            Explode();
        }

        public void Explode()
        {
            float x = WorldPosition.x;
            float y = WorldPosition.y;

            Vector3 position = colorBoosterType switch
            {
                ColorBoosterType.Horizontal => new(0, y),
                ColorBoosterType.Vertical => new(x, 0),
                ColorBoosterType.Wrapped => new(x, y),
                _ => Vector3.zero
            };

            EffectManager.Instance.SpawnBoosterEffect(itemType, colorBoosterType, position);
        }

        public override void ReleaseItem()
        {
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

        public void PlayStartEffect()
        {
            EffectManager.Instance.SpawnNewCreatedEffect(WorldPosition);
        }

        public void PlayMatchEffect()
        {
            EffectManager.Instance.SpawnColorEffect(candyColor, WorldPosition);
        }

        public void PlayBreakEffect(int healthPoint)
        {
            EffectManager.Instance.SpawnColorEffect(candyColor, WorldPosition);
        }

        public void PlayReplaceEffect()
        {
            
        }
    }
}
