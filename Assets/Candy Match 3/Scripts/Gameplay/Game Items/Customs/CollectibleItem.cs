using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Customs
{
    public class CollectibleItem : BaseItem, IItemAnimation
    {
        [SerializeField] private ItemAnimation itemAnimation;

        public override bool CanBeReplace => false;

        public override bool IsMatchable => false;

        public override bool IsMoveable => true;

        public UniTask BounceInDirection(Vector3 direction)
        {
            return UniTask.CompletedTask;
        }

        public UniTask BounceOnTap()
        {
            return UniTask.CompletedTask;
        }

        public override void InitMessages()
        {
            
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
            SimplePool.Despawn(this.gameObject);
        }
    }
}
