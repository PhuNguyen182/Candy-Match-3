using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Colored
{
    public class UnbreakableItem : BaseItem
    {
        public override bool Replacable => false;

        public override bool IsMatchable => false;

        public override bool IsMoveable => false;

        public override void InitMessages()
        {
            
        }

        public override UniTask ItemBlast()
        {
            return UniTask.CompletedTask;
        }

        public override void ReleaseItem()
        {
            
        }
    }
}
