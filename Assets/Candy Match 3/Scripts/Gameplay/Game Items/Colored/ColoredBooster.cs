using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Colored
{
    public class ColoredBooster : BaseItem, ISetColor, ISetColorBooster, IBooster, IItemAnimation
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

        public ColorBoosterType ColorBoosterType => colorBoosterType;

        public override void ResetItem()
        {
            base.ResetItem();
            SetMatchable(true);
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
        }

        public async UniTask Activate()
        {
            await UniTask.CompletedTask;
        }

        public void Explode()
        {
            
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
    }
}
