using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Customs
{
    public class CollectibleItem : BaseItem
    {
        public override bool CanBeReplace => false;

        public override bool IsMatchable => false;

        public override bool CanMove => true;

        public override void InitMessages()
        {
            
        }

        public override UniTask ItemBlast()
        {
            return UniTask.CompletedTask;
        }

        public override void ReleaseItem()
        {
            SimplePool.Despawn(this.gameObject);
        }
    }
}
