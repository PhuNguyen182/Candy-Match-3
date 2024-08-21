using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Colored
{
    public class ColoredItem : BaseItem, ISetColor, IItemAnimation, IBreakable
    {
        [SerializeField] private ItemAnimation itemAnimation;

        [Header("Colored Sprites")]
        [SerializeField] private Sprite[] candyColors;

        public override bool CanBeReplace => true;

        public override bool IsMatchable => true;

        public override bool IsMoveable => true;

        public override void ResetItem()
        {
            base.ResetItem();
            itemAnimation.DisappearOnMatch(false).Forget();
        }

        public override void InitMessages()
        {
            
        }

        public override async UniTask ItemBlast()
        {
            await itemAnimation.DisappearOnMatch(true);
        }

        public override void ReleaseItem()
        {
            SimplePool.Despawn(this.gameObject);
        }

        public void SetColor(CandyColor candyColor)
        {
            this.candyColor = candyColor;
            Sprite colorSprite = candyColor switch
            {
                CandyColor.Blue => candyColors[0],
                CandyColor.Green => candyColors[1],
                CandyColor.Orange => candyColors[2],
                CandyColor.Purple => candyColors[3],
                CandyColor.Red => candyColors[4],
                CandyColor.Yellow => candyColors[5],
                _ => null
            };

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
    }
}
