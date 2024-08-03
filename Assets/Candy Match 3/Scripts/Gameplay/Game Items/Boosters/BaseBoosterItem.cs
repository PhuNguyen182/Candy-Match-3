using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Boosters
{
    public abstract class BaseBoosterItem : BaseItem, IBooster
    {
        public override bool CanBeReplace => false;

        public override bool IsMatchable => false;

        public override bool CanMove => true;

        public abstract UniTask Activate();

        public abstract void Explode();
    }
}
