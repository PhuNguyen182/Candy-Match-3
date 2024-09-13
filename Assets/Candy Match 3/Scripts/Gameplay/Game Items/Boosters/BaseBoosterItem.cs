using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Boosters
{
    public abstract class BaseBoosterItem : BaseItem, IBooster, IItemAnimation
    {
        [SerializeField] protected ItemAnimation itemAnimation;

        public bool IsActivated { get; set; }

        public bool IsNewCreated { get; set; }

        public override bool CanBeReplace => false;

        public override bool IsMatchable => false;

        public override bool IsMoveable => true;

        public override void ReleaseItem()
        {
            base.ReleaseItem();
            IsActivated = false;
        }

        public abstract UniTask Activate();

        public UniTask BounceInDirection(Vector3 direction)
        {
            return itemAnimation.BounceMove(direction);
        }

        public void BounceOnTap()
        {
            itemAnimation.BounceTap();
        }

        public abstract void Explode();

        public void JumpDown(float amptitude)
        {
            itemAnimation.JumpDown(amptitude);
        }

        public UniTask MoveTo(Vector3 position, float duration)
        {
            return itemAnimation.MoveTo(position, duration);
        }

        public UniTask SwapTo(Vector3 position, float duration, bool isMoveFirst)
        {
            return itemAnimation.SwapTo(position, duration, isMoveFirst);
        }
    }
}
