using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Colored
{
    public class ColoredItem : BaseItem, ISetColor
    {
        [Header("Colored Sprites")]
        [SerializeField] private Sprite[] candyColors;

        public override bool IsMatchable => true;

        public override bool CanMove => true;

        public override void ResetItem()
        {
            base.ResetItem();
        }

        public override void InitMessages()
        {
            
        }

        public override async UniTask ItemBlast()
        {
            await UniTask.CompletedTask;
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

            itemGraphics.SetItemSprite(colorSprite);
        }
    }
}
