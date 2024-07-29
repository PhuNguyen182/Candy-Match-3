using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Customs
{
    public class AdjacentiveItem : BaseItem, IAdjcentBreakable
    {
        public override bool IsMatchable => false;

        public override bool CanMove => false;

        public bool Break()
        {
            // Do logic and effect here
            return true;
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
    }
}
