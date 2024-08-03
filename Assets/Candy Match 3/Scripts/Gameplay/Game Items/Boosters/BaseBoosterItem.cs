using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Boosters
{
    public abstract class BaseBoosterItem : BaseItem, IBooster, IItemAnimation
    {
        [SerializeField] private ItemAnimation itemAnimation;

        public override bool CanBeReplace => false;

        public override bool IsMatchable => false;

        public override bool IsMoveable => true;

        public abstract UniTask Activate();

        public UniTask BounceInDirection(Vector3 direction)
        {
            return UniTask.CompletedTask;
        }

        public UniTask BounceOnTap()
        {
            return UniTask.CompletedTask;
        }

        public abstract void Explode();

        public UniTask JumpDown(float amplitude)
        {
            return UniTask.CompletedTask;
        }

        public UniTask MoveTo(Vector3 position)
        {
            return itemAnimation.MoveTo(position);
        }
    }
}
